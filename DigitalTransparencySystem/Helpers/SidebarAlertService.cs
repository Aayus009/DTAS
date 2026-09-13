using System;
using System.Data.SqlClient;

namespace DigitalTransparencySystem.Helpers
{
    public class SidebarAlertCounts
    {
        public int Events { get; set; }
        public int Decisions { get; set; }
        public int Tasks { get; set; }
        public int Assignments { get; set; }
        public int Clubs { get; set; }
        public int Polls { get; set; }
        public int Users { get; set; }
        public int Identity { get; set; }
        public int Flags { get; set; }
        public int Feedback { get; set; }
        public int Notifications { get; set; }
    }

    public static class SidebarAlertService
    {
        private static readonly object SchemaLock = new object();
        private static bool schemaReady;

        public static void EnsureSchema()
        {
            if (schemaReady)
                return;
            lock (SchemaLock)
            {
                if (schemaReady)
                    return;
                using (var con = new SqlConnection(AuthService.ConnectionString))
                {
                    con.Open();
                    new SqlCommand(@"
                    IF OBJECT_ID(N'dbo.SidebarSeen', N'U') IS NULL
                        CREATE TABLE dbo.SidebarSeen (
                            UserID INT NOT NULL,
                            Section NVARCHAR(40) NOT NULL,
                            SeenAt DATETIME NOT NULL CONSTRAINT DF_SidebarSeen_SeenAt DEFAULT GETDATE(),
                            CONSTRAINT PK_SidebarSeen PRIMARY KEY (UserID, Section)
                        );", con).ExecuteNonQuery();
                }
                schemaReady = true;
            }
        }

        public static SidebarAlertCounts Load(int userId, string activeSection)
        {
            EnsureSchema();
            string section = NormalizeSection(activeSection);
            if (ShouldMarkSeen(section))
            {
                try { MarkSeen(userId, section); }
                catch { }
            }

            var counts = new SidebarAlertCounts();
            try
            {
                using (var con = new SqlConnection(AuthService.ConnectionString))
                {
                    con.Open();
                    counts.Events = CountNew(con, userId, "events",
                        @"SELECT COUNT(*) FROM Events
                          WHERE ISNULL(IsDeleted, 0) = 0 AND CreatedAt > @Seen")
                        + CountNew(con, userId, "events",
                        @"SELECT COUNT(*) FROM EventMembers
                          WHERE InviteStatus = N'Requested' AND JoinedAt > @Seen");
                    counts.Decisions = CountNew(con, userId, "decisions",
                        "SELECT COUNT(*) FROM Decisions WHERE CreatedAt > @Seen");
                    counts.Tasks = CountNew(con, userId, "tasks",
                        @"SELECT COUNT(*) FROM Tasks
                          WHERE ISNULL(IsDeleted, 0) = 0
                            AND ISNULL(Status, N'') <> N'Archived'
                            AND CreatedAt > @Seen");
                    counts.Assignments = CountNew(con, userId, "assignments",
                        @"SELECT COUNT(*) FROM Assignments
                          WHERE ISNULL(IsDeleted, 0) = 0 AND CreatedAt > @Seen");
                    counts.Clubs = CountNew(con, userId, "clubs",
                        @"SELECT COUNT(*) FROM Clubs
                          WHERE ISNULL(IsDeleted, 0) = 0 AND CreatedAt > @Seen")
                        + CountNew(con, userId, "clubs",
                        @"SELECT COUNT(*) FROM ClubMembers
                          WHERE InviteStatus = N'Requested' AND JoinedAt > @Seen");
                    counts.Polls = CountNew(con, userId, "polls",
                        "SELECT COUNT(*) FROM Polls WHERE CreatedAt > @Seen");
                    counts.Users = CountNew(con, userId, "users",
                        @"SELECT COUNT(*) FROM Users
                          WHERE ISNULL(IsDeleted, 0) = 0 AND CreatedAt > @Seen");
                    counts.Identity = CountNew(con, userId, "identity",
                        @"SELECT COUNT(*) FROM IdentityDocuments
                          WHERE IsCurrent = 1 AND VerificationStatus = N'Pending' AND UploadDate > @Seen");
                    counts.Flags = CountNew(con, userId, "flags",
                        "SELECT COUNT(*) FROM ContentReports WHERE CreatedAt > @Seen");
                    counts.Feedback = CountNew(con, userId, "feedback",
                        "SELECT COUNT(*) FROM Feedback WHERE CreatedAt > @Seen");
                }
            }
            catch
            {
            }

            try { counts.Notifications = NotificationService.CountUnread(userId); }
            catch { counts.Notifications = 0; }

            ZeroSection(counts, section);
            return counts;
        }

        public static void MarkSeen(int userId, string section)
        {
            section = NormalizeSection(section);
            if (userId <= 0 || string.IsNullOrEmpty(section))
                return;

            EnsureSchema();
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(@"
                IF EXISTS (SELECT 1 FROM SidebarSeen WHERE UserID = @UserID AND Section = @Section)
                    UPDATE SidebarSeen SET SeenAt = GETDATE()
                    WHERE UserID = @UserID AND Section = @Section;
                ELSE
                    INSERT INTO SidebarSeen (UserID, Section, SeenAt)
                    VALUES (@UserID, @Section, GETDATE());", con))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@Section", section);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private static int CountNew(SqlConnection con, int userId, string section, string sql)
        {
            DateTime seenAt = GetSeenAt(con, userId, section);
            try
            {
                using (var cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@Seen", seenAt);
                    object value = cmd.ExecuteScalar();
                    if (value == null || value == DBNull.Value)
                        return 0;
                    return Convert.ToInt32(value);
                }
            }
            catch
            {
                return 0;
            }
        }

        private static DateTime GetSeenAt(SqlConnection con, int userId, string section)
        {
            using (var cmd = new SqlCommand(
                "SELECT SeenAt FROM SidebarSeen WHERE UserID = @UserID AND Section = @Section", con))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@Section", section);
                object value = cmd.ExecuteScalar();
                if (value != null && value != DBNull.Value)
                    return Convert.ToDateTime(value);
            }
            return DateTime.Now.AddDays(-7);
        }

        private static void ZeroSection(SidebarAlertCounts counts, string section)
        {
            switch (section)
            {
                case "events": counts.Events = 0; break;
                case "decisions": counts.Decisions = 0; break;
                case "tasks": counts.Tasks = 0; break;
                case "assignments": counts.Assignments = 0; break;
                case "clubs": counts.Clubs = 0; break;
                case "polls": counts.Polls = 0; break;
                case "users": counts.Users = 0; break;
                case "identity": counts.Identity = 0; break;
                case "flags": counts.Flags = 0; break;
                case "feedback": counts.Feedback = 0; break;
            }
        }

        private static bool ShouldMarkSeen(string section)
        {
            return section == "events"
                || section == "decisions"
                || section == "tasks"
                || section == "assignments"
                || section == "clubs"
                || section == "polls"
                || section == "users"
                || section == "identity"
                || section == "flags"
                || section == "feedback";
        }

        private static string NormalizeSection(string section)
        {
            if (string.IsNullOrWhiteSpace(section))
                return "";
            return section.Trim().ToLowerInvariant();
        }
    }
}
