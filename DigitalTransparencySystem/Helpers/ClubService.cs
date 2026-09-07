using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace DigitalTransparencySystem.Helpers
{
    public class ClubRecord
    {
        public int ClubID { get; set; }
        public string ClubName { get; set; }
        public string Description { get; set; }
        public int? LeadUserID { get; set; }
        public bool IsPublic { get; set; }
        public string InviteCode { get; set; }
        public string ImagePath { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
        public bool IsRestricted { get; set; }
    }

    public class ClubAccess
    {
        public ClubRecord Club { get; set; }
        public bool IsSystemAdmin { get; set; }
        public bool IsMember { get; set; }
        public bool IsLead { get; set; }
        public bool HasPendingInvite { get; set; }
        public bool HasPendingJoinRequest { get; set; }
        public int? PendingInvitationId { get; set; }

        public bool CanView
        {
            get
            {
                if (Club == null || Club.IsDeleted || !Club.IsActive)
                    return false;
                if (IsSystemAdmin || IsMember)
                    return true;
                return Club.IsPublic && !Club.IsRestricted;
            }
        }

        public bool CanManage
        {
            get { return IsLead && Club != null && !Club.IsRestricted; }
        }
    }

    public static class ClubService
    {
        private const string ActiveMemberFilter = "ISNULL(IsActive, 1) = 1 AND ISNULL(InviteStatus, N'Accepted') = N'Accepted'";
        private static bool membershipSchemaReady;

        public static void EnsureMembershipSchema()
        {
            if (membershipSchemaReady)
                return;
            using (var con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                new SqlCommand(@"
                    IF COL_LENGTH('dbo.ClubMembers', 'InviteStatus') IS NULL
                        ALTER TABLE dbo.ClubMembers ADD InviteStatus NVARCHAR(20) NOT NULL CONSTRAINT DF_ClubMembers_InviteStatus DEFAULT N'Accepted';
                    IF COL_LENGTH('dbo.ClubMembers', 'IsActive') IS NULL
                        ALTER TABLE dbo.ClubMembers ADD IsActive BIT NOT NULL CONSTRAINT DF_ClubMembers_IsActive DEFAULT 1;
                ", con).ExecuteNonQuery();
            }
            membershipSchemaReady = true;
        }

        public static ClubRecord GetClub(int clubId)
        {
            RestrictionService.EnsureSchema();
            EnsureMembershipSchema();
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT ClubID, ClubName, Description, LeadUserID,
                         ISNULL(IsPublic, 0) AS IsPublic, InviteCode, ImagePath,
                         ISNULL(IsDeleted, 0) AS IsDeleted, ISNULL(IsActive, 1) AS IsActive,
                         ISNULL(IsRestricted, 0) AS IsRestricted
                  FROM Clubs WHERE ClubID = @ID", con))
            {
                cmd.Parameters.AddWithValue("@ID", clubId);
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;
                    return ReadClub(reader);
                }
            }
        }

        public static ClubAccess GetAccess(int clubId, int userId, string systemRole)
        {
            var access = new ClubAccess
            {
                Club = GetClub(clubId),
                IsSystemAdmin = RoleAccess.IsAdmin(systemRole)
            };
            if (access.Club == null)
                return access;

            EnsureInviteCode(clubId);
            access.Club = GetClub(clubId);

            using (var con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                var member = new SqlCommand(
                    "SELECT Role FROM ClubMembers WHERE ClubID = @ClubID AND UserID = @UserID AND " + ActiveMemberFilter, con);
                member.Parameters.AddWithValue("@ClubID", clubId);
                member.Parameters.AddWithValue("@UserID", userId);
                object role = member.ExecuteScalar();
                if (role != null && role != DBNull.Value)
                {
                    access.IsMember = true;
                    access.IsLead = string.Equals(role.ToString(), "Lead", StringComparison.OrdinalIgnoreCase)
                        || access.Club.LeadUserID == userId;
                }
                else if (access.Club.LeadUserID == userId)
                {
                    access.IsLead = true;
                }

                access.HasPendingJoinRequest = HasPendingJoinRequest(clubId, userId);

                UserAccount user = AuthService.FindById(userId);
                var invite = new SqlCommand(
                    @"SELECT TOP 1 InvitationID FROM GroupInvitations
                      WHERE ClubID = @ClubID AND Status = N'Pending'
                        AND (InvitedUserID = @UserID OR Email = @Email)", con);
                invite.Parameters.AddWithValue("@ClubID", clubId);
                invite.Parameters.AddWithValue("@UserID", userId);
                invite.Parameters.AddWithValue("@Email", user == null ? "" : (user.Email ?? "").Trim().ToLowerInvariant());
                object invitationId = invite.ExecuteScalar();
                if (invitationId != null && invitationId != DBNull.Value)
                {
                    access.HasPendingInvite = true;
                    access.PendingInvitationId = Convert.ToInt32(invitationId);
                }
            }

            return access;
        }

        public static string ActivityBlocked(ClubRecord club)
        {
            if (club == null || club.IsDeleted || !club.IsActive)
                return "Club not found.";
            if (club.IsRestricted)
                return "This club is restricted after an administrator review. Club activity is paused.";
            return null;
        }

        public static string CreateClub(int creatorId, string name, string description, bool isPublic, HttpPostedFile image, HttpServerUtility server, out int clubId)
        {
            clubId = 0;
            UserAccount creator = AuthService.FindById(creatorId);
            if (creator == null)
                return "Account not found.";
            if (RoleAccess.IsAdmin(creator.Role) || !RoleAccess.CanCreateClubs(creator.Role))
                return "Only faculty, staff, and students can create a club. Administrators manage existing clubs.";

            if (string.IsNullOrWhiteSpace(name))
                return "Club name is required.";

            string imagePath = null;
            if (image != null && image.ContentLength > 0)
            {
                string error = SaveImage(image, server, out imagePath);
                if (error != null)
                    return error;
            }

            using (var con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                using (var tx = con.BeginTransaction())
                {
                    var insert = new SqlCommand(
                        @"INSERT INTO Clubs (ClubName, Description, LeadUserID, IsActive, CreatedBy, CreatedAt, IsPublic, ImagePath, InviteCode, IsDeleted)
                          VALUES (@Name, @Desc, @Lead, 1, @Lead, GETDATE(), @Public, @Image, @Code, 0);
                          SELECT CAST(SCOPE_IDENTITY() AS INT);", con, tx);
                    insert.Parameters.AddWithValue("@Name", name.Trim());
                    insert.Parameters.AddWithValue("@Desc", (object)description ?? DBNull.Value);
                    insert.Parameters.AddWithValue("@Lead", creatorId);
                    insert.Parameters.AddWithValue("@Public", isPublic);
                    insert.Parameters.AddWithValue("@Image", (object)imagePath ?? DBNull.Value);
                    insert.Parameters.AddWithValue("@Code", NewInviteCode());
                    clubId = Convert.ToInt32(insert.ExecuteScalar());

                    AddMember(con, tx, clubId, creatorId, "Lead");
                    tx.Commit();
                }
            }

            AuthService.WriteAudit(creatorId, "ClubCreated", "Club", clubId, name.Trim(), null);
            return null;
        }

        public static string EnsureInviteCode(int clubId)
        {
            ClubRecord club = GetClub(clubId);
            if (club == null)
                return null;
            if (!string.IsNullOrWhiteSpace(club.InviteCode))
                return club.InviteCode;

            for (int i = 0; i < 8; i++)
            {
                string code = NewInviteCode();
                try
                {
                    using (var con = new SqlConnection(AuthService.ConnectionString))
                    using (var cmd = new SqlCommand(
                        "UPDATE Clubs SET InviteCode = @Code WHERE ClubID = @ID AND InviteCode IS NULL", con))
                    {
                        cmd.Parameters.AddWithValue("@Code", code);
                        cmd.Parameters.AddWithValue("@ID", clubId);
                        con.Open();
                        if (cmd.ExecuteNonQuery() > 0)
                            return code;
                    }
                }
                catch (SqlException)
                {
                    continue;
                }
            }

            club = GetClub(clubId);
            return club == null ? null : club.InviteCode;
        }

        public static string InviteByEmail(int clubId, int actorId, string actorRole, string email)
        {
            ClubAccess access = GetAccess(clubId, actorId, actorRole);
            string blocked = ActivityBlocked(access.Club);
            if (blocked != null)
                return blocked;
            if (!access.CanManage)
                return "Only the club lead can invite members.";

            EmailLookupResult lookup = EventService.LookupByExactEmail(email);
            email = (email ?? "").Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(email) || email.IndexOf('@') < 1)
                return "Enter a complete email address.";

            if (lookup.Found && IsMember(clubId, lookup.UserID.Value))
                return "That user is already a member.";

            using (var con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                var existing = new SqlCommand(
                    "SELECT InvitationID, Status FROM GroupInvitations WHERE ClubID = @ClubID AND Email = @Email", con);
                existing.Parameters.AddWithValue("@ClubID", clubId);
                existing.Parameters.AddWithValue("@Email", email);
                using (SqlDataReader reader = existing.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int invitationId = Convert.ToInt32(reader["InvitationID"]);
                        string status = reader["Status"].ToString();
                        reader.Close();
                        if (string.Equals(status, "Pending", StringComparison.OrdinalIgnoreCase))
                            return "An invitation is already pending for that email.";

                        var update = new SqlCommand(
                            @"UPDATE GroupInvitations
                              SET Status = N'Pending', InvitedUserID = @UserID, CreatedBy = @Actor, CreatedAt = GETDATE()
                              WHERE InvitationID = @ID", con);
                        update.Parameters.AddWithValue("@UserID", (object)lookup.UserID ?? DBNull.Value);
                        update.Parameters.AddWithValue("@Actor", actorId);
                        update.Parameters.AddWithValue("@ID", invitationId);
                        update.ExecuteNonQuery();
                    }
                    else
                    {
                        reader.Close();
                        var insert = new SqlCommand(
                            @"INSERT INTO GroupInvitations (ClubID, Email, InvitedUserID, Status, CreatedBy, CreatedAt)
                              VALUES (@ClubID, @Email, @UserID, N'Pending', @Actor, GETDATE())", con);
                        insert.Parameters.AddWithValue("@ClubID", clubId);
                        insert.Parameters.AddWithValue("@Email", email);
                        insert.Parameters.AddWithValue("@UserID", (object)lookup.UserID ?? DBNull.Value);
                        insert.Parameters.AddWithValue("@Actor", actorId);
                        insert.ExecuteNonQuery();
                    }
                }
            }

            if (lookup.Found)
                Notify(lookup.UserID.Value, "Club invitation", "You were invited to " + access.Club.ClubName + ".", clubId);

            MailSender.Send(email, "DTAS club invitation",
                "You have been invited to the DTAS club \"" + access.Club.ClubName + "\".\r\nSign in to accept, or join with code "
                + (access.Club.InviteCode ?? "") + ".\r\n\r\nDTAS");
            return null;
        }

        public static DataTable ListMyInvites(int userId)
        {
            UserAccount user = AuthService.FindById(userId);
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT i.InvitationID, i.ClubID, c.ClubName
                  FROM GroupInvitations i
                  INNER JOIN Clubs c ON c.ClubID = i.ClubID AND ISNULL(c.IsDeleted, 0) = 0
                  WHERE i.Status = N'Pending'
                    AND (i.InvitedUserID = @UserID OR i.Email = @Email)
                  ORDER BY i.CreatedAt DESC", con))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@Email", user == null ? "" : (user.Email ?? "").Trim().ToLowerInvariant());
                var table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                return table;
            }
        }

        public static string AcceptInvitation(int invitationId, int userId)
        {
            UserAccount user = AuthService.FindById(userId);
            using (var con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                var read = new SqlCommand(
                    @"SELECT ClubID, Status FROM GroupInvitations
                      WHERE InvitationID = @ID
                        AND (InvitedUserID = @UserID OR Email = @Email)", con);
                read.Parameters.AddWithValue("@ID", invitationId);
                read.Parameters.AddWithValue("@UserID", userId);
                read.Parameters.AddWithValue("@Email", user == null ? "" : (user.Email ?? "").Trim().ToLowerInvariant());
                int clubId;
                using (SqlDataReader reader = read.ExecuteReader())
                {
                    if (!reader.Read())
                        return "Invitation not found.";
                    if (!string.Equals(Convert.ToString(reader["Status"]), "Pending", StringComparison.OrdinalIgnoreCase))
                        return "That invitation is no longer pending.";
                    clubId = Convert.ToInt32(reader["ClubID"]);
                }

                string blocked = ActivityBlocked(GetClub(clubId));
                if (blocked != null)
                    return blocked;

                var accept = new SqlCommand(
                    "UPDATE GroupInvitations SET Status = N'Accepted', InvitedUserID = @UserID WHERE InvitationID = @ID", con);
                accept.Parameters.AddWithValue("@UserID", userId);
                accept.Parameters.AddWithValue("@ID", invitationId);
                accept.ExecuteNonQuery();
                AddMember(con, null, clubId, userId, "Member");
            }
            return null;
        }

        public static string DeclineInvitation(int invitationId, int userId)
        {
            UserAccount user = AuthService.FindById(userId);
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"UPDATE GroupInvitations SET Status = N'Declined'
                  WHERE InvitationID = @ID AND Status = N'Pending'
                    AND (InvitedUserID = @UserID OR Email = @Email)", con))
            {
                cmd.Parameters.AddWithValue("@ID", invitationId);
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@Email", user == null ? "" : (user.Email ?? "").Trim().ToLowerInvariant());
                con.Open();
                if (cmd.ExecuteNonQuery() == 0)
                    return "Invitation not found.";
            }
            return null;
        }

        public static string JoinByCode(int userId, string code, out int clubId)
        {
            clubId = 0;
            if (string.IsNullOrWhiteSpace(code))
                return "Enter a club invitation code.";

            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT ClubID FROM Clubs
                  WHERE InviteCode = @Code AND ISNULL(IsDeleted, 0) = 0 AND ISNULL(IsActive, 1) = 1", con))
            {
                cmd.Parameters.AddWithValue("@Code", code.Trim().ToUpperInvariant());
                con.Open();
                object id = cmd.ExecuteScalar();
                if (id == null)
                    return "That invitation code is not valid.";
                clubId = Convert.ToInt32(id);
            }

            string blocked = ActivityBlocked(GetClub(clubId));
            if (blocked != null)
                return blocked;

            if (IsMember(clubId, userId))
                return "You are already a member.";

            using (var con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                AddMember(con, null, clubId, userId, "Member");
            }
            return null;
        }

        public static string JoinPublic(int clubId, int userId)
        {
            ClubRecord club = GetClub(clubId);
            string blocked = ActivityBlocked(club);
            if (blocked != null)
                return blocked;
            if (!club.IsPublic)
                return "This club is private. Use an invitation or code.";
            if (IsMember(clubId, userId))
                return "You are already a member.";
            if (HasPendingJoinRequest(clubId, userId))
                return "Your request is already waiting for the club lead.";

            UpsertJoinRequest(clubId, userId);

            UserAccount requester = AuthService.FindById(userId);
            string who = requester == null || string.IsNullOrWhiteSpace(requester.FullName)
                ? "Someone"
                : requester.FullName;
            if (club.LeadUserID.HasValue)
            {
                Notify(club.LeadUserID.Value, "Join request",
                    who + " asked to join \"" + club.ClubName + "\". Accept or decline the request in the club workspace.",
                    clubId);
            }

            AuthService.WriteAudit(userId, "ClubJoinRequested", "Club", clubId, null, null);
            return null;
        }

        public static bool HasPendingJoinRequest(int clubId, int userId)
        {
            EnsureMembershipSchema();
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT COUNT(*) FROM ClubMembers
                  WHERE ClubID = @ClubID AND UserID = @UserID AND InviteStatus = N'Requested'", con))
            {
                cmd.Parameters.AddWithValue("@ClubID", clubId);
                cmd.Parameters.AddWithValue("@UserID", userId);
                con.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        public static DataTable ListJoinRequests(int clubId)
        {
            EnsureMembershipSchema();
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT m.UserID, u.FullName, u.Email, m.JoinedAt
                  FROM ClubMembers m
                  INNER JOIN Users u ON u.UserID = m.UserID
                  WHERE m.ClubID = @ClubID AND m.InviteStatus = N'Requested'
                  ORDER BY m.JoinedAt DESC", con))
            {
                cmd.Parameters.AddWithValue("@ClubID", clubId);
                var table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                return table;
            }
        }

        public static string AcceptJoinRequest(int clubId, int actorId, string actorRole, int targetUserId)
        {
            ClubAccess access = GetAccess(clubId, actorId, actorRole);
            string blocked = ActivityBlocked(access.Club);
            if (blocked != null)
                return blocked;
            if (!access.CanManage)
                return "Only the club lead can accept join requests.";
            if (!HasPendingJoinRequest(clubId, targetUserId))
                return "That join request is no longer pending.";

            using (var con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                AddMember(con, null, clubId, targetUserId, "Member");
            }

            Notify(targetUserId, "Join request accepted",
                "You were accepted into \"" + access.Club.ClubName + "\". You can now use the club workspace.",
                clubId);
            AuthService.WriteAudit(actorId, "ClubJoinAccepted", "Club", clubId, "UserID " + targetUserId, null);
            return null;
        }

        public static string DeclineJoinRequest(int clubId, int actorId, string actorRole, int targetUserId)
        {
            ClubAccess access = GetAccess(clubId, actorId, actorRole);
            string blocked = ActivityBlocked(access.Club);
            if (blocked != null)
                return blocked;
            if (!access.CanManage)
                return "Only the club lead can decline join requests.";
            if (!HasPendingJoinRequest(clubId, targetUserId))
                return "That join request is no longer pending.";

            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"UPDATE ClubMembers
                  SET InviteStatus = N'Declined', IsActive = 0
                  WHERE ClubID = @ClubID AND UserID = @UserID AND InviteStatus = N'Requested'", con))
            {
                cmd.Parameters.AddWithValue("@ClubID", clubId);
                cmd.Parameters.AddWithValue("@UserID", targetUserId);
                con.Open();
                cmd.ExecuteNonQuery();
            }

            Notify(targetUserId, "Join request declined",
                "Your request to join \"" + access.Club.ClubName + "\" was declined.",
                clubId);
            AuthService.WriteAudit(actorId, "ClubJoinDeclined", "Club", clubId, "UserID " + targetUserId, null);
            return null;
        }

        public static string Leave(int clubId, int userId)
        {
            ClubRecord club = GetClub(clubId);
            if (club == null)
                return "Club not found.";

            UserAccount user = AuthService.FindById(userId);
            string name = user == null ? "A member" : user.FullName;
            bool wasLead = club.LeadUserID == userId || IsLead(clubId, userId);

            using (var con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                var remove = new SqlCommand(
                    "DELETE FROM ClubMembers WHERE ClubID = @ClubID AND UserID = @UserID", con);
                remove.Parameters.AddWithValue("@ClubID", clubId);
                remove.Parameters.AddWithValue("@UserID", userId);
                remove.ExecuteNonQuery();

                PostMessage(con, clubId, userId, name + " left the group.", "Text");

                if (wasLead)
                {
                    var next = new SqlCommand(
                        @"SELECT TOP 1 UserID FROM ClubMembers
                          WHERE ClubID = @ClubID AND " + ActiveMemberFilter + @"
                          ORDER BY JoinedAt, MembershipID", con);
                    next.Parameters.AddWithValue("@ClubID", clubId);
                    object nextId = next.ExecuteScalar();
                    if (nextId == null)
                    {
                        var archive = new SqlCommand(
                            "UPDATE Clubs SET IsDeleted = 1, IsActive = 0, LeadUserID = NULL WHERE ClubID = @ClubID", con);
                        archive.Parameters.AddWithValue("@ClubID", clubId);
                        archive.ExecuteNonQuery();
                    }
                    else
                    {
                        TransferLead(con, clubId, Convert.ToInt32(nextId));
                        UserAccount successor = AuthService.FindById(Convert.ToInt32(nextId));
                        PostMessage(con, clubId, Convert.ToInt32(nextId),
                            (successor == null ? "A member" : successor.FullName) + " is now the club lead.", "Announcement");
                    }
                }
            }

            AuthService.WriteAudit(userId, "ClubLeft", "Club", clubId, null, null);
            return null;
        }

        private static void UpsertJoinRequest(int clubId, int userId)
        {
            EnsureMembershipSchema();
            using (var con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                var update = new SqlCommand(
                    @"UPDATE ClubMembers
                      SET InviteStatus = N'Requested', IsActive = 0, Role = N'Member', JoinedAt = GETDATE()
                      WHERE ClubID = @ClubID AND UserID = @UserID", con);
                update.Parameters.AddWithValue("@ClubID", clubId);
                update.Parameters.AddWithValue("@UserID", userId);
                if (update.ExecuteNonQuery() > 0)
                    return;

                var insert = new SqlCommand(
                    @"INSERT INTO ClubMembers (ClubID, UserID, Role, JoinedAt, InviteStatus, IsActive)
                      VALUES (@ClubID, @UserID, N'Member', GETDATE(), N'Requested', 0)", con);
                insert.Parameters.AddWithValue("@ClubID", clubId);
                insert.Parameters.AddWithValue("@UserID", userId);
                insert.ExecuteNonQuery();
            }
        }

        public static string TransferLead(int clubId, int actorId, string actorRole, int newLeadId)
        {
            ClubAccess access = GetAccess(clubId, actorId, actorRole);
            string blocked = ActivityBlocked(access.Club);
            if (blocked != null)
                return blocked;
            if (!access.CanManage)
                return "Only the club lead can transfer leadership.";
            if (!IsMember(clubId, newLeadId))
                return "The new lead must already be a member.";

            using (var con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                TransferLead(con, clubId, newLeadId);
            }
            return null;
        }

        public static string SendMessage(int clubId, int userId, string systemRole, string text, string type)
        {
            ClubAccess access = GetAccess(clubId, userId, systemRole);
            string blocked = ActivityBlocked(access.Club);
            if (blocked != null)
                return blocked;
            if (!access.IsMember)
                return "Join the club to send messages.";
            if (string.IsNullOrWhiteSpace(text))
                return "Write a message first.";

            if (!string.Equals(type, "Urgent", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(type, "Announcement", StringComparison.OrdinalIgnoreCase))
                type = "Text";

            if ((type == "Urgent" || type == "Announcement") && !access.CanManage)
                return "Only the club lead can send announcements or urgent messages.";

            using (var con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                PostMessage(con, clubId, userId, text.Trim(), type);
                if (type == "Announcement" || type == "Urgent")
                {
                    var ann = new SqlCommand(
                        @"INSERT INTO GroupAnnouncements (ClubID, SenderID, Title, Message, IsUrgent, CreatedAt)
                          VALUES (@ClubID, @UserID, @Title, @Message, @Urgent, GETDATE())", con);
                    ann.Parameters.AddWithValue("@ClubID", clubId);
                    ann.Parameters.AddWithValue("@UserID", userId);
                    ann.Parameters.AddWithValue("@Title", type == "Urgent" ? "Urgent" : "Announcement");
                    ann.Parameters.AddWithValue("@Message", text.Trim());
                    ann.Parameters.AddWithValue("@Urgent", type == "Urgent");
                    ann.ExecuteNonQuery();
                }
            }

            if (type == "Announcement" || type == "Urgent")
            {
                DataTable members = ListMembers(clubId);
                foreach (DataRow row in members.Rows)
                {
                    int memberId = Convert.ToInt32(row["UserID"]);
                    if (memberId == userId)
                        continue;
                    NotificationService.Send(memberId,
                        type == "Urgent" ? "Urgent club message" : "Club announcement",
                        access.Club.ClubName + ": " + text.Trim(),
                        type,
                        clubId,
                        "Club");
                }
            }

            return null;
        }

        public static DataTable ListMyClubs(int userId)
        {
            RestrictionService.EnsureSchema();
            EnsureMembershipSchema();
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT c.ClubID, c.ClubName, c.Description, ISNULL(c.IsPublic, 0) AS IsPublic, m.Role,
                         c.InviteCode, ISNULL(c.IsRestricted, 0) AS IsRestricted
                  FROM ClubMembers m
                  INNER JOIN Clubs c ON c.ClubID = m.ClubID
                  WHERE m.UserID = @UserID AND ISNULL(c.IsDeleted, 0) = 0 AND ISNULL(c.IsActive, 1) = 1
                    AND ISNULL(m.IsActive, 1) = 1 AND ISNULL(m.InviteStatus, N'Accepted') = N'Accepted'
                  ORDER BY c.ClubName", con))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
                var table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                return table;
            }
        }

        public static DataTable ListPublic(int userId)
        {
            RestrictionService.EnsureSchema();
            EnsureMembershipSchema();
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT c.ClubID, c.ClubName, c.Description,
                         CASE WHEN EXISTS (
                             SELECT 1 FROM ClubMembers r
                             WHERE r.ClubID = c.ClubID AND r.UserID = @UserID AND r.InviteStatus = N'Requested'
                         ) THEN 1 ELSE 0 END AS HasPendingRequest
                  FROM Clubs c
                  WHERE ISNULL(c.IsPublic, 0) = 1 AND ISNULL(c.IsDeleted, 0) = 0 AND ISNULL(c.IsActive, 1) = 1
                    AND ISNULL(c.IsRestricted, 0) = 0
                    AND NOT EXISTS (
                        SELECT 1 FROM ClubMembers m
                        WHERE m.ClubID = c.ClubID AND m.UserID = @UserID
                          AND ISNULL(m.IsActive, 1) = 1 AND ISNULL(m.InviteStatus, N'Accepted') = N'Accepted')
                  ORDER BY c.ClubName", con))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
                var table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                return table;
            }
        }

        public static DataTable ListMembers(int clubId)
        {
            EnsureMembershipSchema();
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT m.UserID, m.Role, m.JoinedAt, u.FullName, u.Email
                  FROM ClubMembers m
                  INNER JOIN Users u ON u.UserID = m.UserID
                  WHERE m.ClubID = @ClubID
                    AND ISNULL(m.IsActive, 1) = 1 AND ISNULL(m.InviteStatus, N'Accepted') = N'Accepted'
                  ORDER BY CASE WHEN m.Role = N'Lead' THEN 0 ELSE 1 END, u.FullName", con))
            {
                cmd.Parameters.AddWithValue("@ClubID", clubId);
                var table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                return table;
            }
        }

        public static DataTable ListMessages(int clubId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT TOP 80 m.MessageID, m.MessageContent, m.MessageType, m.CreatedAt, u.FullName
                  FROM GroupMessages m
                  INNER JOIN Users u ON u.UserID = m.SenderID
                  WHERE m.ClubID = @ClubID AND m.IsDeleted = 0
                  ORDER BY m.CreatedAt DESC", con))
            {
                cmd.Parameters.AddWithValue("@ClubID", clubId);
                var table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                table.DefaultView.Sort = "CreatedAt ASC";
                return table.DefaultView.ToTable();
            }
        }

        public static DataTable ListAnnouncements(int clubId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT TOP 10 a.Title, a.Message, a.IsUrgent, a.CreatedAt, u.FullName
                  FROM GroupAnnouncements a
                  INNER JOIN Users u ON u.UserID = a.SenderID
                  WHERE a.ClubID = @ClubID
                  ORDER BY a.CreatedAt DESC", con))
            {
                cmd.Parameters.AddWithValue("@ClubID", clubId);
                var table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                return table;
            }
        }

        public static int? FindClubIdByCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return null;
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                "SELECT ClubID FROM Clubs WHERE InviteCode = @Code AND ISNULL(IsDeleted, 0) = 0", con))
            {
                cmd.Parameters.AddWithValue("@Code", code.Trim().ToUpperInvariant());
                con.Open();
                object id = cmd.ExecuteScalar();
                return id == null ? (int?)null : Convert.ToInt32(id);
            }
        }

        public static int? ResolveClubId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            value = value.Trim();
            int numeric;
            if (int.TryParse(value, out numeric) && numeric > 0)
            {
                ClubRecord club = GetClub(numeric);
                if (club != null && !club.IsDeleted && club.IsActive)
                    return numeric;
            }
            return FindClubIdByCode(value);
        }

        public static bool IsClubMember(int clubId, int userId)
        {
            EnsureMembershipSchema();
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM ClubMembers WHERE ClubID = @ClubID AND UserID = @UserID AND " + ActiveMemberFilter, con))
            {
                cmd.Parameters.AddWithValue("@ClubID", clubId);
                cmd.Parameters.AddWithValue("@UserID", userId);
                con.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        public static bool CanUseClub(int clubId, int userId, string systemRole)
        {
            ClubRecord club = GetClub(clubId);
            if (club == null || club.IsDeleted || !club.IsActive || club.IsRestricted)
                return false;
            if (RoleAccess.IsAdmin(systemRole))
                return false;
            if (club.LeadUserID == userId)
                return true;
            return IsClubMember(clubId, userId);
        }

        private static void TransferLead(SqlConnection con, int clubId, int newLeadId)
        {
            var clear = new SqlCommand(
                "UPDATE ClubMembers SET Role = N'Member' WHERE ClubID = @ClubID AND Role = N'Lead'", con);
            clear.Parameters.AddWithValue("@ClubID", clubId);
            clear.ExecuteNonQuery();

            var set = new SqlCommand(
                "UPDATE ClubMembers SET Role = N'Lead' WHERE ClubID = @ClubID AND UserID = @UserID", con);
            set.Parameters.AddWithValue("@ClubID", clubId);
            set.Parameters.AddWithValue("@UserID", newLeadId);
            set.ExecuteNonQuery();

            var club = new SqlCommand(
                "UPDATE Clubs SET LeadUserID = @UserID WHERE ClubID = @ClubID", con);
            club.Parameters.AddWithValue("@UserID", newLeadId);
            club.Parameters.AddWithValue("@ClubID", clubId);
            club.ExecuteNonQuery();
        }

        private static void AddMember(SqlConnection con, SqlTransaction tx, int clubId, int userId, string role)
        {
            EnsureMembershipSchema();
            var update = new SqlCommand(
                @"UPDATE ClubMembers
                  SET Role = @Role, JoinedAt = GETDATE(), InviteStatus = N'Accepted', IsActive = 1
                  WHERE ClubID = @ClubID AND UserID = @UserID", con, tx);
            update.Parameters.AddWithValue("@ClubID", clubId);
            update.Parameters.AddWithValue("@UserID", userId);
            update.Parameters.AddWithValue("@Role", role);
            if (update.ExecuteNonQuery() > 0)
                return;

            var insert = new SqlCommand(
                @"INSERT INTO ClubMembers (ClubID, UserID, Role, JoinedAt, InviteStatus, IsActive)
                  VALUES (@ClubID, @UserID, @Role, GETDATE(), N'Accepted', 1)", con, tx);
            insert.Parameters.AddWithValue("@ClubID", clubId);
            insert.Parameters.AddWithValue("@UserID", userId);
            insert.Parameters.AddWithValue("@Role", role);
            insert.ExecuteNonQuery();
        }

        private static void PostMessage(SqlConnection con, int clubId, int senderId, string text, string type)
        {
            var cmd = new SqlCommand(
                @"INSERT INTO GroupMessages (ClubID, SenderID, MessageContent, MessageType, CreatedAt, IsDeleted)
                  VALUES (@ClubID, @SenderID, @Text, @Type, GETDATE(), 0)", con);
            cmd.Parameters.AddWithValue("@ClubID", clubId);
            cmd.Parameters.AddWithValue("@SenderID", senderId);
            cmd.Parameters.AddWithValue("@Text", text);
            cmd.Parameters.AddWithValue("@Type", type);
            cmd.ExecuteNonQuery();
        }

        private static bool IsMember(int clubId, int userId)
        {
            return IsClubMember(clubId, userId);
        }

        private static bool IsLead(int clubId, int userId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM ClubMembers WHERE ClubID = @ClubID AND UserID = @UserID AND Role = N'Lead' AND " + ActiveMemberFilter, con))
            {
                cmd.Parameters.AddWithValue("@ClubID", clubId);
                cmd.Parameters.AddWithValue("@UserID", userId);
                con.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private static void Notify(int userId, string title, string message, int clubId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"INSERT INTO Notifications (UserID, Title, Message, IsRead, NotificationType, RelatedID, RelatedType, CreatedAt)
                  VALUES (@UserID, @Title, @Message, 0, N'Club', @ClubID, N'Club', GETDATE())", con))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@Title", title);
                cmd.Parameters.AddWithValue("@Message", message);
                cmd.Parameters.AddWithValue("@ClubID", clubId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private static string SaveImage(HttpPostedFile file, HttpServerUtility server, out string relativePath)
        {
            relativePath = null;
            if (file.ContentLength > 2 * 1024 * 1024)
                return "Club image must be 2 MB or smaller.";

            string ext = Path.GetExtension(file.FileName);
            if (ext == null)
                return "Choose a JPG or PNG image.";
            ext = ext.ToLowerInvariant();
            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png")
                return "Club image must be JPG or PNG.";

            string folder = server.MapPath("~/Uploads/ClubImages");
            Directory.CreateDirectory(folder);
            string name = Guid.NewGuid().ToString("N") + ext;
            file.SaveAs(Path.Combine(folder, name));
            relativePath = "~/Uploads/ClubImages/" + name;
            return null;
        }

        private static string NewInviteCode()
        {
            const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var bytes = new byte[5];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(bytes);
            var sb = new StringBuilder("DTAS-CLB-");
            for (int i = 0; i < 5; i++)
                sb.Append(alphabet[bytes[i] % alphabet.Length]);
            return sb.ToString();
        }

        private static ClubRecord ReadClub(SqlDataReader reader)
        {
            return new ClubRecord
            {
                ClubID = Convert.ToInt32(reader["ClubID"]),
                ClubName = Convert.ToString(reader["ClubName"]),
                Description = Convert.ToString(reader["Description"]),
                LeadUserID = reader["LeadUserID"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["LeadUserID"]),
                IsPublic = Convert.ToBoolean(reader["IsPublic"]),
                InviteCode = reader["InviteCode"] == DBNull.Value ? null : Convert.ToString(reader["InviteCode"]),
                ImagePath = reader["ImagePath"] == DBNull.Value ? null : Convert.ToString(reader["ImagePath"]),
                IsDeleted = Convert.ToBoolean(reader["IsDeleted"]),
                IsActive = Convert.ToBoolean(reader["IsActive"]),
                IsRestricted = Convert.ToBoolean(reader["IsRestricted"])
            };
        }
    }
}
