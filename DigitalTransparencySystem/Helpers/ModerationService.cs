using System;
using System.Data;
using System.Data.SqlClient;

namespace DigitalTransparencySystem.Helpers
{
    public static class ModerationService
    {
        public static string CanModerate(int targetUserId, int adminId)
        {
            if (targetUserId == adminId)
                return "You cannot moderate your own account.";

            UserAccount target = AuthService.FindById(targetUserId);
            if (target == null || target.IsDeleted)
                return "That account was not found.";

            if (target.RoleID == 1)
                return "Administrator accounts cannot be suspended or banned here.";

            return null;
        }

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
                    IF COL_LENGTH('dbo.Users', 'AccountStatus') IS NULL
                        ALTER TABLE dbo.Users ADD AccountStatus NVARCHAR(50) NOT NULL CONSTRAINT DF_Users_AccountStatus_Runtime DEFAULT N'Active';
                    IF OBJECT_ID('dbo.Suspensions', 'U') IS NULL
                        CREATE TABLE dbo.Suspensions (
                            SuspensionID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            UserID INT NOT NULL,
                            Reason NVARCHAR(MAX) NOT NULL,
                            StartDate DATETIME NOT NULL DEFAULT GETDATE(),
                            EndDate DATETIME NOT NULL,
                            CreatedBy INT NOT NULL,
                            CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
                            IsActive BIT NOT NULL DEFAULT 1
                        );
                    IF OBJECT_ID('dbo.Bans', 'U') IS NULL
                        CREATE TABLE dbo.Bans (
                            BanID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            UserID INT NULL,
                            Email NVARCHAR(200) NOT NULL,
                            InstitutionalID NVARCHAR(100) NULL,
                            Reason NVARCHAR(MAX) NOT NULL,
                            BannedBy INT NOT NULL,
                            BannedAt DATETIME NOT NULL DEFAULT GETDATE(),
                            IsActive BIT NOT NULL DEFAULT 1
                        );
                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Bans_IsActive_Email' AND object_id = OBJECT_ID(N'dbo.Bans'))
                        CREATE NONCLUSTERED INDEX IX_Bans_IsActive_Email ON dbo.Bans(Email) INCLUDE (InstitutionalID) WHERE IsActive = 1;
                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Suspensions_UserActive' AND object_id = OBJECT_ID(N'dbo.Suspensions'))
                        CREATE NONCLUSTERED INDEX IX_Suspensions_UserActive ON dbo.Suspensions(UserID, EndDate) WHERE IsActive = 1;", con).ExecuteNonQuery();
                }
                schemaReady = true;
            }
        }

        public static string Suspend(int targetUserId, int adminId, string reason, int days)
        {
            EnsureSchema();
            string block = CanModerate(targetUserId, adminId);
            if (block != null)
                return block;

            if (string.IsNullOrWhiteSpace(reason))
                return "A reason is required.";

            if (days < 1)
                days = 7;
            if (days > 365)
                days = 365;

            using (var con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                using (var tx = con.BeginTransaction())
                {
                    var clear = new SqlCommand(
                        "UPDATE Suspensions SET IsActive = 0 WHERE UserID = @UserID AND IsActive = 1", con, tx);
                    clear.Parameters.AddWithValue("@UserID", targetUserId);
                    clear.ExecuteNonQuery();

                    var insert = new SqlCommand(
                        @"INSERT INTO Suspensions (UserID, Reason, StartDate, EndDate, CreatedBy, CreatedAt, IsActive)
                          VALUES (@UserID, @Reason, GETDATE(), DATEADD(DAY, @Days, GETDATE()), @AdminID, GETDATE(), 1)", con, tx);
                    insert.Parameters.AddWithValue("@UserID", targetUserId);
                    insert.Parameters.AddWithValue("@Reason", reason.Trim());
                    insert.Parameters.AddWithValue("@Days", days);
                    insert.Parameters.AddWithValue("@AdminID", adminId);
                    insert.ExecuteNonQuery();

                    var user = new SqlCommand(
                        "UPDATE Users SET AccountStatus = N'Suspended' WHERE UserID = @UserID", con, tx);
                    user.Parameters.AddWithValue("@UserID", targetUserId);
                    user.ExecuteNonQuery();

                    Notify(con, tx, targetUserId, "Account suspended",
                        "Your account is suspended for " + days + " day(s). Reason: " + reason.Trim());
                    tx.Commit();
                }
            }

            AuthService.WriteAudit(adminId, "UserSuspended", "User", targetUserId, reason.Trim() + " (" + days + " days)", null);
            return null;
        }

        public static string Unsuspend(int targetUserId, int adminId)
        {
            EnsureSchema();
            string block = CanModerate(targetUserId, adminId);
            if (block != null)
                return block;

            using (var con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                var cmd = new SqlCommand(
                    @"UPDATE Suspensions SET IsActive = 0 WHERE UserID = @UserID AND IsActive = 1;
                      UPDATE Users SET AccountStatus = N'Active' WHERE UserID = @UserID AND AccountStatus = N'Suspended';", con);
                cmd.Parameters.AddWithValue("@UserID", targetUserId);
                cmd.ExecuteNonQuery();
            }

            AuthService.WriteAudit(adminId, "UserUnsuspended", "User", targetUserId, "Suspension lifted.", null);
            return null;
        }

        public static string Ban(int targetUserId, int adminId, string reason)
        {
            EnsureSchema();
            string block = CanModerate(targetUserId, adminId);
            if (block != null)
                return block;

            if (string.IsNullOrWhiteSpace(reason))
                return "A reason is required.";

            UserAccount target = AuthService.FindById(targetUserId);
            using (var con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                using (var tx = con.BeginTransaction())
                {
                    var prior = new SqlCommand(
                        "UPDATE Bans SET IsActive = 0 WHERE UserID = @UserID AND IsActive = 1", con, tx);
                    prior.Parameters.AddWithValue("@UserID", targetUserId);
                    prior.ExecuteNonQuery();

                    var insert = new SqlCommand(
                        @"INSERT INTO Bans (UserID, Email, InstitutionalID, Reason, BannedBy, BannedAt, IsActive)
                          VALUES (@UserID, @Email, @InstitutionalID, @Reason, @AdminID, GETDATE(), 1)", con, tx);
                    insert.Parameters.AddWithValue("@UserID", targetUserId);
                    insert.Parameters.AddWithValue("@Email", (target.Email ?? "").Trim().ToLowerInvariant());
                    insert.Parameters.AddWithValue("@InstitutionalID",
                        string.IsNullOrWhiteSpace(target.InstitutionalID) ? (object)DBNull.Value : target.InstitutionalID.Trim());
                    insert.Parameters.AddWithValue("@Reason", reason.Trim());
                    insert.Parameters.AddWithValue("@AdminID", adminId);
                    insert.ExecuteNonQuery();

                    var user = new SqlCommand(
                        "UPDATE Users SET AccountStatus = N'Banned', IsActive = 0 WHERE UserID = @UserID", con, tx);
                    user.Parameters.AddWithValue("@UserID", targetUserId);
                    user.ExecuteNonQuery();

                    Notify(con, tx, targetUserId, "Account banned",
                        "Your account has been banned. Reason: " + reason.Trim());
                    tx.Commit();
                }
            }

            AuthService.WriteAudit(adminId, "UserBanned", "User", targetUserId, reason.Trim(), null);
            return null;
        }

        public static string Unban(int targetUserId, int adminId)
        {
            EnsureSchema();
            string block = CanModerate(targetUserId, adminId);
            if (block != null)
                return block;

            UserAccount target = AuthService.FindById(targetUserId);
            using (var con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                var cmd = new SqlCommand(
                    @"UPDATE Bans SET IsActive = 0
                      WHERE IsActive = 1 AND (UserID = @UserID OR Email = @Email);
                      UPDATE Users SET AccountStatus = N'Active', IsActive = 1 WHERE UserID = @UserID;", con);
                cmd.Parameters.AddWithValue("@UserID", targetUserId);
                cmd.Parameters.AddWithValue("@Email", (target.Email ?? "").Trim().ToLowerInvariant());
                cmd.ExecuteNonQuery();
            }

            AuthService.WriteAudit(adminId, "UserUnbanned", "User", targetUserId, "Ban lifted.", null);
            return null;
        }

        public static string SubmitReport(int reporterId, string targetType, int targetId, string reason, string description)
        {
            if (string.IsNullOrWhiteSpace(targetType) || targetId <= 0)
                return "Choose what you are reporting.";

            if (string.IsNullOrWhiteSpace(reason))
                return "A reason is required.";

            if (string.Equals(targetType, "User", StringComparison.OrdinalIgnoreCase) && targetId == reporterId)
                return "You cannot report your own account.";

            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"INSERT INTO ContentReports (ReporterID, TargetType, TargetID, Reason, Description, Status, CreatedAt)
                  VALUES (@ReporterID, @TargetType, @TargetID, @Reason, @Description, N'Pending', GETDATE())", con))
            {
                cmd.Parameters.AddWithValue("@ReporterID", reporterId);
                cmd.Parameters.AddWithValue("@TargetType", targetType);
                cmd.Parameters.AddWithValue("@TargetID", targetId);
                cmd.Parameters.AddWithValue("@Reason", reason.Trim());
                cmd.Parameters.AddWithValue("@Description", (object)description ?? DBNull.Value);
                con.Open();
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (SqlException)
                {
                    return "That report could not be saved. Check the target type and ID.";
                }
            }

            AuthService.WriteAudit(reporterId, "ContentReported", targetType, targetId, reason.Trim(), null);
            return null;
        }

        public static DataTable ListReports(string status)
        {
            string filter = string.Equals(status, "All", StringComparison.OrdinalIgnoreCase)
                ? ""
                : "WHERE cr.Status = @Status";

            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT cr.ReportID, cr.TargetType, cr.TargetID, cr.Reason, cr.Description, cr.Status,
                         cr.Resolution, cr.CreatedAt, cr.ResolvedAt,
                         r.FullName AS ReporterName, r.Email AS ReporterEmail,
                         a.FullName AS AdminName
                  FROM ContentReports cr
                  INNER JOIN Users r ON r.UserID = cr.ReporterID
                  LEFT JOIN Users a ON a.UserID = cr.AssignedAdminID
                  " + filter + @"
                  ORDER BY CASE cr.Status WHEN N'Pending' THEN 0 WHEN N'Reviewing' THEN 1 ELSE 2 END, cr.CreatedAt DESC", con))
            {
                if (!string.IsNullOrEmpty(filter))
                    cmd.Parameters.AddWithValue("@Status", status);

                var table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                return table;
            }
        }

        public static int CountPendingReports()
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM ContentReports WHERE Status = N'Pending'", con))
            {
                con.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public static DataTable ListMyReports(int reporterId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT ReportID, TargetType, TargetID, Reason, Description, Status, Resolution, CreatedAt, ResolvedAt
                  FROM ContentReports
                  WHERE ReporterID = @ReporterID
                  ORDER BY CreatedAt DESC", con))
            {
                cmd.Parameters.AddWithValue("@ReporterID", reporterId);
                var table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                return table;
            }
        }

        public static string ResolveReport(int reportId, int adminId, string newStatus, string resolution)
        {
            if (newStatus != "Resolved" && newStatus != "Rejected" && newStatus != "Reviewing" && newStatus != "Escalated")
                return "Invalid decision.";

            if ((newStatus == "Resolved" || newStatus == "Rejected") && string.IsNullOrWhiteSpace(resolution))
                return "Add a resolution note.";

            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"UPDATE ContentReports
                  SET Status = @Status,
                      AssignedAdminID = @AdminID,
                      Resolution = @Resolution,
                      ResolvedAt = CASE WHEN @Status IN (N'Resolved', N'Rejected') THEN GETDATE() ELSE ResolvedAt END
                  WHERE ReportID = @ReportID", con))
            {
                cmd.Parameters.AddWithValue("@Status", newStatus);
                cmd.Parameters.AddWithValue("@AdminID", adminId);
                cmd.Parameters.AddWithValue("@Resolution", (object)resolution ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ReportID", reportId);
                con.Open();
                cmd.ExecuteNonQuery();
            }

            AuthService.WriteAudit(adminId, "ContentReportUpdated", "ContentReport", reportId, newStatus, null);

            int reporterId = 0;
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand("SELECT ReporterID FROM ContentReports WHERE ReportID = @ID", con))
            {
                cmd.Parameters.AddWithValue("@ID", reportId);
                con.Open();
                object id = cmd.ExecuteScalar();
                if (id != null && id != DBNull.Value)
                    reporterId = Convert.ToInt32(id);
            }
            if (reporterId > 0)
            {
                NotificationService.Send(reporterId, "Report " + newStatus,
                    "Your flag was marked " + newStatus + "."
                    + (string.IsNullOrWhiteSpace(resolution) ? "" : " Note: " + resolution.Trim()),
                    "Report", reportId, "ContentReport");
            }
            return null;
        }

        public static bool TryGetReportTarget(int reportId, out string targetType, out int targetId)
        {
            targetType = null;
            targetId = 0;
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                "SELECT TargetType, TargetID FROM ContentReports WHERE ReportID = @ID", con))
            {
                cmd.Parameters.AddWithValue("@ID", reportId);
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return false;
                    targetType = Convert.ToString(reader["TargetType"]);
                    targetId = Convert.ToInt32(reader["TargetID"]);
                    return true;
                }
            }
        }

        public static int? FindUserIdByEmail(string email)
        {
            UserAccount user = AuthService.FindByEmail(email);
            return user == null ? (int?)null : user.UserID;
        }

        private static void Notify(SqlConnection con, SqlTransaction tx, int userId, string title, string message)
        {
            var cmd = new SqlCommand(
                @"INSERT INTO Notifications (UserID, Title, Message, IsRead, NotificationType, RelatedID, RelatedType, CreatedAt)
                  VALUES (@UserID, @Title, @Message, 0, N'Moderation', @UserID, N'User', GETDATE())", con, tx);
            cmd.Parameters.AddWithValue("@UserID", userId);
            cmd.Parameters.AddWithValue("@Title", title);
            cmd.Parameters.AddWithValue("@Message", message);
            cmd.ExecuteNonQuery();
        }
    }
}
