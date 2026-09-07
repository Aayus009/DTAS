using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace DigitalTransparencySystem.Helpers
{
    public class ConnectGroupRecord
    {
        public int GroupID { get; set; }
        public string GroupName { get; set; }
        public string Description { get; set; }
        public string InviteCode { get; set; }
        public int CreatedBy { get; set; }
        public bool IsDeleted { get; set; }
        public string PhotoPath { get; set; }
    }

    public class ConnectAccess
    {
        public ConnectGroupRecord Group { get; set; }
        public bool IsMember { get; set; }
        public bool IsOwner { get; set; }
        public string Role { get; set; }

        public bool CanView
        {
            get { return Group != null && !Group.IsDeleted && IsMember; }
        }

        public bool IsArchived { get; set; }

        public bool CanManage
        {
            get { return CanView && IsOwner; }
        }
    }

    public static class ConnectService
    {
        private static bool schemaReady;

        public static void EnsureSchema()
        {
            if (schemaReady)
                return;
            using (var con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                new SqlCommand(@"
                    IF OBJECT_ID('dbo.ConnectGroups', 'U') IS NULL
                    BEGIN
                        CREATE TABLE dbo.ConnectGroups (
                            GroupID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            GroupName NVARCHAR(200) NOT NULL,
                            Description NVARCHAR(MAX) NULL,
                            InviteCode NVARCHAR(50) NOT NULL,
                            CreatedBy INT NOT NULL,
                            CreatedAt DATETIME NOT NULL CONSTRAINT DF_ConnectGroups_CreatedAt DEFAULT GETDATE(),
                            IsDeleted BIT NOT NULL CONSTRAINT DF_ConnectGroups_IsDeleted DEFAULT 0,
                            CONSTRAINT UQ_ConnectGroups_Code UNIQUE (InviteCode),
                            CONSTRAINT FK_ConnectGroups_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES dbo.Users(UserID)
                        );
                    END

                    IF OBJECT_ID('dbo.ConnectMembers', 'U') IS NULL
                    BEGIN
                        CREATE TABLE dbo.ConnectMembers (
                            MemberID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            GroupID INT NOT NULL,
                            UserID INT NOT NULL,
                            Role NVARCHAR(20) NOT NULL CONSTRAINT DF_ConnectMembers_Role DEFAULT N'Member',
                            JoinedAt DATETIME NOT NULL CONSTRAINT DF_ConnectMembers_JoinedAt DEFAULT GETDATE(),
                            CONSTRAINT FK_ConnectMembers_Group FOREIGN KEY (GroupID) REFERENCES dbo.ConnectGroups(GroupID),
                            CONSTRAINT FK_ConnectMembers_User FOREIGN KEY (UserID) REFERENCES dbo.Users(UserID),
                            CONSTRAINT UQ_ConnectMembers UNIQUE (GroupID, UserID)
                        );
                    END

                    IF OBJECT_ID('dbo.ConnectMessages', 'U') IS NULL
                    BEGIN
                        CREATE TABLE dbo.ConnectMessages (
                            MessageID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            GroupID INT NOT NULL,
                            SenderID INT NOT NULL,
                            MessageType NVARCHAR(30) NOT NULL CONSTRAINT DF_ConnectMessages_Type DEFAULT N'Message',
                            Content NVARCHAR(MAX) NULL,
                            ImagePath NVARCHAR(500) NULL,
                            CreatedAt DATETIME NOT NULL CONSTRAINT DF_ConnectMessages_CreatedAt DEFAULT GETDATE(),
                            IsDeleted BIT NOT NULL CONSTRAINT DF_ConnectMessages_IsDeleted DEFAULT 0,
                            CONSTRAINT FK_ConnectMessages_Group FOREIGN KEY (GroupID) REFERENCES dbo.ConnectGroups(GroupID),
                            CONSTRAINT FK_ConnectMessages_Sender FOREIGN KEY (SenderID) REFERENCES dbo.Users(UserID)
                        );
                        CREATE INDEX IX_ConnectMessages_Group ON dbo.ConnectMessages(GroupID, MessageID) WHERE IsDeleted = 0;
                    END

                    IF COL_LENGTH('dbo.ConnectGroups', 'PhotoPath') IS NULL
                        ALTER TABLE dbo.ConnectGroups ADD PhotoPath NVARCHAR(500) NULL;

                    IF COL_LENGTH('dbo.ConnectMembers', 'IsArchived') IS NULL
                        ALTER TABLE dbo.ConnectMembers ADD IsArchived BIT NOT NULL CONSTRAINT DF_ConnectMembers_Archived DEFAULT 0;

                    IF COL_LENGTH('dbo.ConnectMembers', 'LastReadMessageID') IS NULL
                        ALTER TABLE dbo.ConnectMembers ADD LastReadMessageID INT NOT NULL CONSTRAINT DF_ConnectMembers_LastRead DEFAULT 0;

                    IF COL_LENGTH('dbo.ConnectGroups', 'EventID') IS NULL
                        ALTER TABLE dbo.ConnectGroups ADD EventID INT NULL;

                    IF COL_LENGTH('dbo.ConnectGroups', 'ClubID') IS NULL
                        ALTER TABLE dbo.ConnectGroups ADD ClubID INT NULL;
                ", con).ExecuteNonQuery();
            }
            schemaReady = true;
        }

        public static string CreateGroup(int userId, string name, string description, out int groupId, out string code)
        {
            groupId = 0;
            code = null;
            EnsureSchema();
            if (string.IsNullOrWhiteSpace(name))
                return "Enter a group name.";

            code = NewInviteCode();
            using (var con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                using (var tx = con.BeginTransaction())
                {
                    var insert = new SqlCommand(
                        @"INSERT INTO ConnectGroups (GroupName, Description, InviteCode, CreatedBy, IsDeleted)
                          VALUES (@Name, @Description, @Code, @UserID, 0);
                          SELECT CAST(SCOPE_IDENTITY() AS INT);", con, tx);
                    insert.Parameters.AddWithValue("@Name", name.Trim());
                    insert.Parameters.AddWithValue("@Description", string.IsNullOrWhiteSpace(description) ? (object)DBNull.Value : description.Trim());
                    insert.Parameters.AddWithValue("@Code", code);
                    insert.Parameters.AddWithValue("@UserID", userId);
                    groupId = Convert.ToInt32(insert.ExecuteScalar());

                    var member = new SqlCommand(
                        @"INSERT INTO ConnectMembers (GroupID, UserID, Role)
                          VALUES (@GroupID, @UserID, N'Owner')", con, tx);
                    member.Parameters.AddWithValue("@GroupID", groupId);
                    member.Parameters.AddWithValue("@UserID", userId);
                    member.ExecuteNonQuery();
                    tx.Commit();
                }
            }
            return null;
        }

        public static int? FindGroupIdByEvent(int eventId)
        {
            EnsureSchema();
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT TOP 1 GroupID FROM ConnectGroups
                  WHERE EventID = @EventID AND IsDeleted = 0
                  ORDER BY GroupID", con))
            {
                cmd.Parameters.AddWithValue("@EventID", eventId);
                con.Open();
                object id = cmd.ExecuteScalar();
                return id == null || id == DBNull.Value ? (int?)null : Convert.ToInt32(id);
            }
        }

        public static string CreateGroupForEvent(int eventId, int actorId, int? clubId, out int groupId)
        {
            groupId = 0;
            EnsureSchema();
            EventRecord ev = EventService.GetEvent(eventId);
            if (ev == null)
                return "Event not found.";

            UserAccount actor = AuthService.FindById(actorId);
            EventAccess access = EventService.GetAccess(eventId, actorId, actor == null ? null : actor.Role);
            if (access == null || (!access.CanManage && !access.CanAssign && !access.IsSystemAdmin && ev.CreatedBy != actorId))
                return "Only the event lead or manager can create this Connect group.";

            int? existing = FindGroupIdByEvent(eventId);
            if (existing.HasValue)
            {
                groupId = existing.Value;
                if (clubId.HasValue)
                    SyncClubMembersToGroup(groupId, clubId.Value, actorId, ev.EventName);
                return null;
            }

            string error = CreateGroup(actorId, ev.EventName + " Connect",
                "Connect group for event " + ev.EventName, out groupId, out _);
            if (error != null)
                return error;

            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"UPDATE ConnectGroups
                  SET EventID = @EventID, ClubID = @ClubID
                  WHERE GroupID = @GroupID", con))
            {
                cmd.Parameters.AddWithValue("@EventID", eventId);
                cmd.Parameters.AddWithValue("@ClubID", clubId.HasValue ? (object)clubId.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@GroupID", groupId);
                con.Open();
                cmd.ExecuteNonQuery();
            }

            if (clubId.HasValue)
                SyncClubMembersToGroup(groupId, clubId.Value, actorId, ev.EventName);
            return null;
        }

        public static int SyncClubMembersToGroup(int groupId, int clubId, int actorId, string eventName)
        {
            ConnectGroupRecord group = GetGroup(groupId);
            string groupName = group == null ? "a Connect group" : group.GroupName;
            DataTable members = ClubService.ListMembers(clubId);
            int added = 0;
            foreach (DataRow row in members.Rows)
            {
                int userId = Convert.ToInt32(row["UserID"]);
                if (AddMemberDirect(groupId, userId, groupName, eventName))
                    added++;
            }
            return added;
        }

        private static bool AddMemberDirect(int groupId, int userId, string groupName, string eventName)
        {
            if (IsMember(groupId, userId))
                return false;

            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"INSERT INTO ConnectMembers (GroupID, UserID, Role)
                  VALUES (@GroupID, @UserID, N'Member')", con))
            {
                cmd.Parameters.AddWithValue("@GroupID", groupId);
                cmd.Parameters.AddWithValue("@UserID", userId);
                con.Open();
                cmd.ExecuteNonQuery();
            }

            string detail = string.IsNullOrWhiteSpace(eventName)
                ? "You were added to " + groupName + "."
                : "You were added to the Connect group for \"" + eventName + "\".";
            NotificationService.Send(userId, "Connect group invitation", detail, "Connect", groupId, "ConnectGroup");
            return true;
        }

        public static string JoinByCode(int userId, string code, out int groupId)
        {
            groupId = 0;
            EnsureSchema();
            if (string.IsNullOrWhiteSpace(code))
                return "Enter an invite code.";

            using (var con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                var find = new SqlCommand(
                    @"SELECT GroupID FROM ConnectGroups
                      WHERE InviteCode = @Code AND IsDeleted = 0", con);
                find.Parameters.AddWithValue("@Code", code.Trim().ToUpperInvariant());
                object found = find.ExecuteScalar();
                if (found == null)
                    return "That invite code is not valid.";
                groupId = Convert.ToInt32(found);

                if (IsMember(groupId, userId))
                    return null;

                var insert = new SqlCommand(
                    @"INSERT INTO ConnectMembers (GroupID, UserID, Role)
                      VALUES (@GroupID, @UserID, N'Member')", con);
                insert.Parameters.AddWithValue("@GroupID", groupId);
                insert.Parameters.AddWithValue("@UserID", userId);
                insert.ExecuteNonQuery();
            }

            ConnectGroupRecord group = GetGroup(groupId);
            NotificationService.Send(userId, "Joined Connect group",
                "You joined " + (group == null ? "a Connect group" : group.GroupName) + ".",
                "Connect", groupId, "ConnectGroup");
            return null;
        }

        public static string AddMember(int groupId, int actorId, string email)
        {
            ConnectAccess access = GetAccess(groupId, actorId);
            if (!access.CanManage)
                return "Only the group owner can add members.";

            EmailLookupResult lookup = EventService.LookupByExactEmail(email);
            if (!lookup.Found)
                return lookup.Message;
            if (IsMember(groupId, lookup.UserID.Value))
                return "That user is already in this group.";

            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"INSERT INTO ConnectMembers (GroupID, UserID, Role)
                  VALUES (@GroupID, @UserID, N'Member')", con))
            {
                cmd.Parameters.AddWithValue("@GroupID", groupId);
                cmd.Parameters.AddWithValue("@UserID", lookup.UserID.Value);
                con.Open();
                cmd.ExecuteNonQuery();
            }

            NotificationService.Send(lookup.UserID.Value, "Added to Connect group",
                "You were added to " + access.Group.GroupName + ".",
                "Connect", groupId, "ConnectGroup");
            return null;
        }

        public static string LeaveGroup(int groupId, int userId)
        {
            ConnectAccess access = GetAccess(groupId, userId);
            if (!access.CanView)
                return "You are not in this group.";

            using (var con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                if (access.IsOwner)
                {
                    var next = new SqlCommand(
                        @"SELECT TOP 1 UserID FROM ConnectMembers
                          WHERE GroupID = @GroupID AND UserID <> @UserID
                          ORDER BY JoinedAt", con);
                    next.Parameters.AddWithValue("@GroupID", groupId);
                    next.Parameters.AddWithValue("@UserID", userId);
                    object nextOwner = next.ExecuteScalar();
                    if (nextOwner == null || nextOwner == DBNull.Value)
                    {
                        var close = new SqlCommand("UPDATE ConnectGroups SET IsDeleted = 1 WHERE GroupID = @GroupID", con);
                        close.Parameters.AddWithValue("@GroupID", groupId);
                        close.ExecuteNonQuery();
                        return null;
                    }

                    var promote = new SqlCommand(
                        "UPDATE ConnectMembers SET Role = N'Owner' WHERE GroupID = @GroupID AND UserID = @Next", con);
                    promote.Parameters.AddWithValue("@GroupID", groupId);
                    promote.Parameters.AddWithValue("@Next", Convert.ToInt32(nextOwner));
                    promote.ExecuteNonQuery();

                    var created = new SqlCommand(
                        "UPDATE ConnectGroups SET CreatedBy = @Next WHERE GroupID = @GroupID", con);
                    created.Parameters.AddWithValue("@Next", Convert.ToInt32(nextOwner));
                    created.Parameters.AddWithValue("@GroupID", groupId);
                    created.ExecuteNonQuery();
                }

                var leave = new SqlCommand(
                    "DELETE FROM ConnectMembers WHERE GroupID = @GroupID AND UserID = @UserID", con);
                leave.Parameters.AddWithValue("@GroupID", groupId);
                leave.Parameters.AddWithValue("@UserID", userId);
                leave.ExecuteNonQuery();
            }
            return null;
        }

        public static ConnectAccess GetAccess(int groupId, int userId)
        {
            EnsureSchema();
            var access = new ConnectAccess { Group = GetGroup(groupId) };
            if (access.Group == null)
                return access;

            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                "SELECT Role, IsArchived FROM ConnectMembers WHERE GroupID = @GroupID AND UserID = @UserID", con))
            {
                cmd.Parameters.AddWithValue("@GroupID", groupId);
                cmd.Parameters.AddWithValue("@UserID", userId);
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        access.IsMember = true;
                        access.Role = Convert.ToString(reader["Role"]);
                        access.IsArchived = reader["IsArchived"] != DBNull.Value && Convert.ToBoolean(reader["IsArchived"]);
                        access.IsOwner = string.Equals(access.Role, "Owner", StringComparison.OrdinalIgnoreCase)
                            || access.Group.CreatedBy == userId;
                    }
                }
            }
            return access;
        }

        public static ConnectGroupRecord GetGroup(int groupId)
        {
            EnsureSchema();
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT GroupID, GroupName, Description, InviteCode, CreatedBy, IsDeleted, PhotoPath
                  FROM ConnectGroups WHERE GroupID = @ID", con))
            {
                cmd.Parameters.AddWithValue("@ID", groupId);
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;
                    return new ConnectGroupRecord
                    {
                        GroupID = Convert.ToInt32(reader["GroupID"]),
                        GroupName = Convert.ToString(reader["GroupName"]),
                        Description = reader["Description"] == DBNull.Value ? "" : Convert.ToString(reader["Description"]),
                        InviteCode = Convert.ToString(reader["InviteCode"]),
                        CreatedBy = Convert.ToInt32(reader["CreatedBy"]),
                        IsDeleted = Convert.ToBoolean(reader["IsDeleted"]),
                        PhotoPath = reader["PhotoPath"] == DBNull.Value ? "" : Convert.ToString(reader["PhotoPath"])
                    };
                }
            }
        }

        public static DataTable ListMyGroups(int userId)
        {
            return ListMyGroups(userId, "all");
        }

        public static DataTable ListMyGroups(int userId, string filter)
        {
            EnsureSchema();
            filter = NormalizeFilter(filter);
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT g.GroupID, g.GroupName, g.Description, g.InviteCode, g.PhotoPath, m.Role,
                         ISNULL(m.IsArchived, 0) AS IsArchived,
                         (SELECT COUNT(*) FROM ConnectMembers cm WHERE cm.GroupID = g.GroupID) AS MemberCount,
                         (SELECT COUNT(*) FROM ConnectMessages msg
                          WHERE msg.GroupID = g.GroupID AND msg.IsDeleted = 0
                            AND msg.MessageID > ISNULL(m.LastReadMessageID, 0)
                            AND msg.SenderID <> @UserID) AS UnreadCount,
                         (SELECT MAX(msg.CreatedAt) FROM ConnectMessages msg WHERE msg.GroupID = g.GroupID AND msg.IsDeleted = 0) AS LastMessageAt,
                         (SELECT TOP 1 CASE
                              WHEN ISNULL(msg.ImagePath, N'') <> N'' AND ISNULL(msg.Content, N'') = N'' THEN N'Photo'
                              WHEN ISNULL(msg.Content, N'') = N'' THEN N''
                              ELSE msg.Content
                          END
                          FROM ConnectMessages msg
                          WHERE msg.GroupID = g.GroupID AND msg.IsDeleted = 0
                          ORDER BY msg.MessageID DESC) AS LastPreview
                  FROM ConnectMembers m
                  INNER JOIN ConnectGroups g ON g.GroupID = m.GroupID AND g.IsDeleted = 0
                  WHERE m.UserID = @UserID
                    AND (
                         (@Filter = N'archive' AND ISNULL(m.IsArchived, 0) = 1)
                      OR (@Filter = N'unread' AND ISNULL(m.IsArchived, 0) = 0 AND (
                            SELECT COUNT(*) FROM ConnectMessages msg
                            WHERE msg.GroupID = g.GroupID AND msg.IsDeleted = 0
                              AND msg.MessageID > ISNULL(m.LastReadMessageID, 0)
                              AND msg.SenderID <> @UserID) > 0)
                      OR (@Filter = N'all' AND ISNULL(m.IsArchived, 0) = 0)
                    )
                  ORDER BY ISNULL((SELECT MAX(msg.CreatedAt) FROM ConnectMessages msg WHERE msg.GroupID = g.GroupID AND msg.IsDeleted = 0), g.CreatedAt) DESC", con))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@Filter", filter);
                var table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                return table;
            }
        }

        public static DataTable ListMembers(int groupId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT m.UserID, m.Role, m.JoinedAt, u.FullName, u.Email
                  FROM ConnectMembers m
                  INNER JOIN Users u ON u.UserID = m.UserID
                  WHERE m.GroupID = @GroupID
                  ORDER BY CASE m.Role WHEN N'Owner' THEN 0 ELSE 1 END, m.JoinedAt", con))
            {
                cmd.Parameters.AddWithValue("@GroupID", groupId);
                var table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                return table;
            }
        }

        public static DataTable ListMessages(int groupId, int afterId)
        {
            return ListMessages(groupId, afterId, 0);
        }

        public static DataTable ListMessages(int groupId, int afterId, int beforeId)
        {
            string sql;
            if (beforeId > 0)
            {
                sql = @"SELECT * FROM (
                            SELECT TOP 40 m.MessageID, m.SenderID, m.MessageType, m.Content, m.ImagePath, m.CreatedAt, u.FullName
                            FROM ConnectMessages m
                            INNER JOIN Users u ON u.UserID = m.SenderID
                            WHERE m.GroupID = @GroupID AND m.IsDeleted = 0 AND m.MessageID < @BeforeID
                            ORDER BY m.MessageID DESC
                        ) x ORDER BY x.MessageID";
            }
            else if (afterId > 0)
            {
                sql = @"SELECT TOP 80 m.MessageID, m.SenderID, m.MessageType, m.Content, m.ImagePath, m.CreatedAt, u.FullName
                        FROM ConnectMessages m
                        INNER JOIN Users u ON u.UserID = m.SenderID
                        WHERE m.GroupID = @GroupID AND m.IsDeleted = 0 AND m.MessageID > @AfterID
                        ORDER BY m.MessageID";
            }
            else
            {
                sql = @"SELECT * FROM (
                            SELECT TOP 80 m.MessageID, m.SenderID, m.MessageType, m.Content, m.ImagePath, m.CreatedAt, u.FullName
                            FROM ConnectMessages m
                            INNER JOIN Users u ON u.UserID = m.SenderID
                            WHERE m.GroupID = @GroupID AND m.IsDeleted = 0
                            ORDER BY m.MessageID DESC
                        ) x ORDER BY x.MessageID";
            }

            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@GroupID", groupId);
                cmd.Parameters.AddWithValue("@AfterID", afterId);
                cmd.Parameters.AddWithValue("@BeforeID", beforeId);
                var table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                return table;
            }
        }

        public static void MarkRead(int groupId, int userId)
        {
            EnsureSchema();
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"UPDATE ConnectMembers
                  SET LastReadMessageID = ISNULL((
                        SELECT MAX(MessageID) FROM ConnectMessages
                        WHERE GroupID = @GroupID AND IsDeleted = 0
                      ), LastReadMessageID)
                  WHERE GroupID = @GroupID AND UserID = @UserID", con))
            {
                cmd.Parameters.AddWithValue("@GroupID", groupId);
                cmd.Parameters.AddWithValue("@UserID", userId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static string SetArchived(int groupId, int userId, bool archived)
        {
            if (!IsMember(groupId, userId))
                return "You are not in this group.";

            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"UPDATE ConnectMembers SET IsArchived = @Archived
                  WHERE GroupID = @GroupID AND UserID = @UserID", con))
            {
                cmd.Parameters.AddWithValue("@GroupID", groupId);
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@Archived", archived);
                con.Open();
                cmd.ExecuteNonQuery();
            }
            return null;
        }

        public static string RenameGroup(int groupId, int userId, string name)
        {
            ConnectAccess access = GetAccess(groupId, userId);
            if (!access.CanView)
                return "You are not in this group.";
            if (string.IsNullOrWhiteSpace(name))
                return "Enter a group name.";

            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                "UPDATE ConnectGroups SET GroupName = @Name WHERE GroupID = @GroupID AND IsDeleted = 0", con))
            {
                cmd.Parameters.AddWithValue("@Name", name.Trim());
                cmd.Parameters.AddWithValue("@GroupID", groupId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
            return null;
        }

        public static string UpdatePhoto(int groupId, int userId, HttpPostedFile file, HttpServerUtility server)
        {
            ConnectAccess access = GetAccess(groupId, userId);
            if (!access.CanView)
                return "You are not in this group.";

            string path;
            string error = SaveImage(file, server, out path);
            if (error != null)
                return error;

            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                "UPDATE ConnectGroups SET PhotoPath = @Path WHERE GroupID = @GroupID AND IsDeleted = 0", con))
            {
                cmd.Parameters.AddWithValue("@Path", path);
                cmd.Parameters.AddWithValue("@GroupID", groupId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
            return null;
        }

        public static string SendMessage(int groupId, int userId, string type, string content, string imagePath, out int messageId)
        {
            messageId = 0;
            ConnectAccess access = GetAccess(groupId, userId);
            if (!access.CanView)
                return "You are not in this group.";

            type = NormalizeType(type);
            if (string.IsNullOrWhiteSpace(content) && string.IsNullOrWhiteSpace(imagePath))
                return "Write a message or attach an image.";

            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"INSERT INTO ConnectMessages (GroupID, SenderID, MessageType, Content, ImagePath, IsDeleted)
                  VALUES (@GroupID, @UserID, @Type, @Content, @Image, 0);
                  SELECT CAST(SCOPE_IDENTITY() AS INT);", con))
            {
                cmd.Parameters.AddWithValue("@GroupID", groupId);
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@Type", type);
                cmd.Parameters.AddWithValue("@Content", string.IsNullOrWhiteSpace(content) ? (object)DBNull.Value : content.Trim());
                cmd.Parameters.AddWithValue("@Image", string.IsNullOrWhiteSpace(imagePath) ? (object)DBNull.Value : imagePath);
                con.Open();
                messageId = Convert.ToInt32(cmd.ExecuteScalar());
            }

            if (type == "Alert" || type == "Important" || type == "Announcement")
            {
                DataTable members = ListMembers(groupId);
                foreach (DataRow row in members.Rows)
                {
                    int memberId = Convert.ToInt32(row["UserID"]);
                    if (memberId == userId)
                        continue;
                    NotificationService.Send(memberId, type + " in " + access.Group.GroupName,
                        string.IsNullOrWhiteSpace(content) ? "New " + type.ToLowerInvariant() + " posted." : content.Trim(),
                        "Connect", groupId, "ConnectGroup");
                }
            }
            return null;
        }

        public static string SaveImage(HttpPostedFile file, HttpServerUtility server, out string relativePath)
        {
            relativePath = null;
            if (file == null || file.ContentLength <= 0)
                return "Choose an image.";
            if (file.ContentLength > 8 * 1024 * 1024)
                return "Images must be 8 MB or smaller.";

            string ext = Path.GetExtension(file.FileName ?? "").ToLowerInvariant();
            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".gif" && ext != ".webp")
                return "Use a JPG, PNG, GIF, or WEBP image.";

            string folder = server.MapPath("~/Uploads/Connect/");
            Directory.CreateDirectory(folder);
            string name = Guid.NewGuid().ToString("N") + ext;
            file.SaveAs(Path.Combine(folder, name));
            relativePath = "~/Uploads/Connect/" + name;
            return null;
        }

        private static bool IsMember(int groupId, int userId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM ConnectMembers WHERE GroupID = @GroupID AND UserID = @UserID", con))
            {
                cmd.Parameters.AddWithValue("@GroupID", groupId);
                cmd.Parameters.AddWithValue("@UserID", userId);
                con.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        public static string NormalizeFilter(string filter)
        {
            if (string.Equals(filter, "unread", StringComparison.OrdinalIgnoreCase))
                return "unread";
            if (string.Equals(filter, "archive", StringComparison.OrdinalIgnoreCase))
                return "archive";
            return "all";
        }

        private static string NormalizeType(string type)
        {
            if (string.Equals(type, "Alert", StringComparison.OrdinalIgnoreCase))
                return "Alert";
            if (string.Equals(type, "Important", StringComparison.OrdinalIgnoreCase))
                return "Important";
            if (string.Equals(type, "Announcement", StringComparison.OrdinalIgnoreCase))
                return "Announcement";
            return "Message";
        }

        private static string NewInviteCode()
        {
            const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var bytes = new byte[5];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(bytes);
            var sb = new StringBuilder("DTAS-CNN-");
            for (int i = 0; i < 5; i++)
                sb.Append(alphabet[bytes[i] % alphabet.Length]);
            return sb.ToString();
        }
    }
}
