using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace DigitalTransparencySystem.Helpers
{
    public static class EventRoles
    {
        public const string EventAdmin = "EventAdmin";
        public const string EventManager = "EventManager";
        public const string Participant = "Participant";
    }

    public class EventRecord
    {
        public int EventID { get; set; }
        public string EventName { get; set; }
        public string Description { get; set; }
        public string EventType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Venue { get; set; }
        public string Status { get; set; }
        public string Visibility { get; set; }
        public string InviteCode { get; set; }
        public bool IsDisabled { get; set; }
        public bool IsDeleted { get; set; }
        public int? CreatedBy { get; set; }
        public int? ProposedBy { get; set; }
        public int? ClubID { get; set; }
    }

    public class EventAccess
    {
        public EventRecord Event { get; set; }
        public bool IsSystemAdmin { get; set; }
        public bool IsMember { get; set; }
        public string RoleInEvent { get; set; }
        public bool HasPendingInvite { get; set; }
        public bool HasPendingJoinRequest { get; set; }
        public int? PendingInvitationId { get; set; }

        public bool CanReviewJoins
        {
            get { return CanManage || CanAssign; }
        }

        public bool CanRemoveMembers
        {
            get { return CanManage || CanAssign; }
        }

        public bool CanView
        {
            get
            {
                if (Event == null || Event.IsDeleted)
                    return false;
                if (IsSystemAdmin || IsMember)
                    return true;
                return string.Equals(Event.Visibility, "Public", StringComparison.OrdinalIgnoreCase);
            }
        }

        public bool CanRestrict
        {
            get { return IsSystemAdmin; }
        }

        public bool CanManage
        {
            get
            {
                return string.Equals(RoleInEvent, EventRoles.EventAdmin, StringComparison.OrdinalIgnoreCase);
            }
        }

        public bool CanAssign
        {
            get
            {
                return string.Equals(RoleInEvent, EventRoles.EventManager, StringComparison.OrdinalIgnoreCase);
            }
        }

        public bool CanEditDetails
        {
            get
            {
                return CanManage || CanAssign;
            }
        }

        public bool CanModify
        {
            get
            {
                if (!CanView || Event == null || Event.IsDisabled)
                    return false;
                return IsMember;
            }
        }
    }

    public class EmailLookupResult
    {
        public bool Found { get; set; }
        public string Message { get; set; }
        public int? UserID { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
    }

    public static class EventService
    {
        private static bool clubLinkSchemaReady;

        public static void EnsureClubLinkSchema()
        {
            if (clubLinkSchemaReady)
                return;
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"IF COL_LENGTH('dbo.Events', 'ClubID') IS NULL
                      ALTER TABLE dbo.Events ADD ClubID INT NULL;", con))
            {
                con.Open();
                cmd.ExecuteNonQuery();
            }
            clubLinkSchemaReady = true;
        }

        public static string DisplayRole(string roleInEvent)
        {
            if (string.Equals(roleInEvent, EventRoles.EventAdmin, StringComparison.OrdinalIgnoreCase))
                return "Event Administrator";
            if (string.Equals(roleInEvent, EventRoles.EventManager, StringComparison.OrdinalIgnoreCase))
                return "Event Manager";
            return "Participant";
        }

        public static EventRecord GetEvent(int eventId)
        {
            EnsureClubLinkSchema();
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT EventID, EventName, Description, EventType, StartDate, EndDate, Venue, Status,
                         ISNULL(Visibility, N'Private') AS Visibility, InviteCode,
                         ISNULL(IsDisabled, 0) AS IsDisabled, ISNULL(IsDeleted, 0) AS IsDeleted,
                         CreatedBy, ProposedBy, ClubID
                  FROM Events WHERE EventID = @EventID", con))
            {
                cmd.Parameters.AddWithValue("@EventID", eventId);
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;
                    return ReadEvent(reader);
                }
            }
        }

        public static EventAccess GetAccess(int eventId, int userId, string systemRole)
        {
            var access = new EventAccess
            {
                Event = GetEvent(eventId),
                IsSystemAdmin = RoleAccess.IsAdmin(systemRole)
            };
            if (access.Event == null)
                return access;

            EnsureInviteCode(eventId);
            access.Event.InviteCode = GetEvent(eventId).InviteCode;

            using (var con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                var member = new SqlCommand(
                    @"SELECT RoleInEvent FROM EventMembers
                      WHERE EventID = @EventID AND UserID = @UserID AND IsActive = 1
                        AND InviteStatus = N'Accepted'", con);
                member.Parameters.AddWithValue("@EventID", eventId);
                member.Parameters.AddWithValue("@UserID", userId);
                object role = member.ExecuteScalar();
                if (role != null && role != DBNull.Value)
                {
                    access.IsMember = true;
                    access.RoleInEvent = role.ToString();
                }

                UserAccount user = AuthService.FindById(userId);
                var invite = new SqlCommand(
                    @"SELECT TOP 1 InvitationID FROM EventInvitations
                      WHERE EventID = @EventID AND Status = N'Pending'
                        AND (InvitedUserID = @UserID OR Email = @Email)", con);
                invite.Parameters.AddWithValue("@EventID", eventId);
                invite.Parameters.AddWithValue("@UserID", userId);
                invite.Parameters.AddWithValue("@Email", user == null ? "" : (user.Email ?? "").Trim().ToLowerInvariant());
                object invitationId = invite.ExecuteScalar();
                if (invitationId != null && invitationId != DBNull.Value)
                {
                    access.HasPendingInvite = true;
                    access.PendingInvitationId = Convert.ToInt32(invitationId);
                }

                if (!access.IsMember)
                {
                    var joinRequest = new SqlCommand(
                        @"SELECT COUNT(*) FROM EventMembers
                          WHERE EventID = @EventID AND UserID = @UserID AND InviteStatus = N'Requested'", con);
                    joinRequest.Parameters.AddWithValue("@EventID", eventId);
                    joinRequest.Parameters.AddWithValue("@UserID", userId);
                    access.HasPendingJoinRequest = Convert.ToInt32(joinRequest.ExecuteScalar()) > 0;
                }
            }

            return access;
        }

        public static string EnsureInviteCode(int eventId)
        {
            EventRecord ev = GetEvent(eventId);
            if (ev == null)
                return null;
            if (!string.IsNullOrWhiteSpace(ev.InviteCode))
                return ev.InviteCode;

            for (int i = 0; i < 8; i++)
            {
                string code = NewInviteCode();
                try
                {
                    using (var con = new SqlConnection(AuthService.ConnectionString))
                    using (var cmd = new SqlCommand(
                        "UPDATE Events SET InviteCode = @Code WHERE EventID = @EventID AND InviteCode IS NULL", con))
                    {
                        cmd.Parameters.AddWithValue("@Code", code);
                        cmd.Parameters.AddWithValue("@EventID", eventId);
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

            ev = GetEvent(eventId);
            return ev == null ? null : ev.InviteCode;
        }

        public static void EnsureCreatorMembership(int eventId, int? fallbackUserId)
        {
            EventRecord ev = GetEvent(eventId);
            if (ev == null)
                return;

            using (var con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                var count = new SqlCommand(
                    "SELECT COUNT(*) FROM EventMembers WHERE EventID = @EventID AND IsActive = 1", con);
                count.Parameters.AddWithValue("@EventID", eventId);
                if (Convert.ToInt32(count.ExecuteScalar()) > 0)
                    return;

                int? owner = ev.CreatedBy ?? ev.ProposedBy ?? fallbackUserId;
                if (owner == null)
                    return;

                UserAccount user = AuthService.FindById(owner.Value);
                string role = user != null && RoleAccess.CanHoldEventAdmin(user.Role)
                    ? EventRoles.EventAdmin
                    : EventRoles.EventManager;
                AddMember(con, null, eventId, owner.Value, role, owner.Value, "Accepted");
            }
        }

        public static void AfterCreated(int eventId, int creatorId)
        {
            AfterCreated(eventId, creatorId, null);
        }

        public static void AfterCreated(int eventId, int creatorId, int? leadUserId)
        {
            EnsureInviteCode(eventId);
            UserAccount creator = AuthService.FindById(creatorId);
            string creatorRole = creator != null && RoleAccess.CanHoldEventAdmin(creator.Role)
                ? EventRoles.EventAdmin
                : EventRoles.EventManager;
            AddOrActivateMember(eventId, creatorId, creatorRole, creatorId);
            if (leadUserId.HasValue && leadUserId.Value != creatorId)
            {
                UserAccount lead = AuthService.FindById(leadUserId.Value);
                if (lead != null && !lead.IsDeleted && RoleAccess.CanHoldEventAdmin(lead.Role))
                    AddOrActivateMember(eventId, leadUserId.Value, EventRoles.EventAdmin, creatorId);
            }
            AuthService.WriteAudit(creatorId, "EventCreated", "Event", eventId, "Event created.", null);
            ApplyLinkedClub(eventId, creatorId, false);
        }

        public static DataTable ListEventLeadCandidates()
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT u.UserID, u.FullName, u.Email, u.RoleID
                  FROM Users u
                  WHERE ISNULL(u.IsDeleted, 0) = 0
                    AND ISNULL(u.AccountStatus, N'Active') = N'Active'
                    AND ISNULL(u.IdentityVerified, 0) = 1
                    AND u.RoleID IN (2, 4)
                  ORDER BY u.FullName, u.Email", con))
            {
                var table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                return table;
            }
        }

        public static void AfterApproved(int eventId, int adminId)
        {
            EnsureInviteCode(eventId);
            EventRecord ev = GetEvent(eventId);
            if (ev != null && ev.CreatedBy.HasValue)
                AddOrActivateMember(eventId, ev.CreatedBy.Value, EventRoles.EventAdmin, adminId);
            if (ev != null && ev.ProposedBy.HasValue)
            {
                UserAccount proposer = AuthService.FindById(ev.ProposedBy.Value);
                string role = proposer != null && RoleAccess.CanHoldEventAdmin(proposer.Role)
                    ? EventRoles.EventAdmin
                    : EventRoles.EventManager;
                    AddOrActivateMember(eventId, ev.ProposedBy.Value, role, adminId);
            }
            ApplyLinkedClub(eventId, adminId, true);
        }

        public static void SetEventClub(int eventId, int? clubId)
        {
            EnsureClubLinkSchema();
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand("UPDATE Events SET ClubID = @ClubID, UpdatedAt = GETDATE() WHERE EventID = @EventID", con))
            {
                cmd.Parameters.AddWithValue("@EventID", eventId);
                cmd.Parameters.AddWithValue("@ClubID", clubId.HasValue ? (object)clubId.Value : DBNull.Value);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static string ApplyLinkedClub(int eventId, int actorId, bool createConnectGroup)
        {
            EventRecord ev = GetEvent(eventId);
            if (ev == null || !ev.ClubID.HasValue)
                return null;

            int added = AddMembersFromClub(eventId, ev.ClubID.Value, actorId);
            if (createConnectGroup)
            {
                int groupId;
                ConnectService.CreateGroupForEvent(eventId, actorId, ev.ClubID.Value, out groupId);
            }
            return added + " club member(s) added to the event.";
        }

        public static int AddMembersFromClub(int eventId, int clubId, int actorId)
        {
            EventRecord ev = GetEvent(eventId);
            ClubRecord club = ClubService.GetClub(clubId);
            if (ev == null || club == null || club.IsRestricted)
                return 0;

            DataTable members = ClubService.ListMembers(clubId);
            int added = 0;
            foreach (DataRow row in members.Rows)
            {
                int userId = Convert.ToInt32(row["UserID"]);
                if (IsActiveMember(eventId, userId))
                    continue;
                AddOrActivateMember(eventId, userId, EventRoles.Participant, actorId);
                Notify(userId,
                    "Added to event from club",
                    "You were added to \"" + ev.EventName + "\" from club \"" + club.ClubName + "\".",
                    eventId);
                added++;
            }
            AuthService.WriteAudit(actorId, "EventClubRosterAdded", "Event", eventId, club.ClubName, null);
            return added;
        }

        public static int? ResolveClubForActor(string selectedClubId, string typedRef, int actorId, string role, out string error)
        {
            error = null;
            string raw = !string.IsNullOrWhiteSpace(selectedClubId) && selectedClubId != "0"
                ? selectedClubId.Trim()
                : (typedRef ?? "").Trim();
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            int? clubId = ClubService.ResolveClubId(raw);
            if (!clubId.HasValue)
            {
                error = "Enter a valid club ID or invitation code.";
                return null;
            }
            ClubRecord club = ClubService.GetClub(clubId.Value);
            if (club != null && club.IsRestricted)
            {
                error = "That club is restricted after a review and cannot be linked to events.";
                return null;
            }
            if (!ClubService.CanUseClub(clubId.Value, actorId, role))
            {
                error = "You can only use a club you belong to.";
                return null;
            }
            return clubId;
        }

        public static string AttachClubToEvent(int eventId, int actorId, string actorRole, string clubRef, bool createConnectGroup)
        {
            int? clubId = ClubService.ResolveClubId(clubRef);
            if (!clubId.HasValue)
                return "Enter a valid club ID or invitation code.";
            ClubRecord club = ClubService.GetClub(clubId.Value);
            if (club != null && club.IsRestricted)
                return "That club is restricted after a review and cannot be linked to events.";
            if (!ClubService.CanUseClub(clubId.Value, actorId, actorRole))
                return "You can only use a club you belong to.";

            SetEventClub(eventId, clubId.Value);
            ApplyLinkedClub(eventId, actorId, createConnectGroup);
            return null;
        }

        public static EmailLookupResult LookupByExactEmail(string email)
        {
            var result = new EmailLookupResult();
            if (string.IsNullOrWhiteSpace(email) || email.IndexOf('@') < 1)
            {
                result.Message = "Enter a complete email address.";
                return result;
            }

            UserAccount user = AuthService.FindByEmail(email.Trim());
            if (user == null || user.IsDeleted)
            {
                result.Message = "No DTAS user available with this email.";
                return result;
            }

            result.Found = true;
            result.UserID = user.UserID;
            result.FullName = user.FullName;
            result.Role = user.Role;
            result.Message = "DTAS user found.";
            return result;
        }

        public static string InviteByEmail(int eventId, int actorId, string actorRole, string email, string inviteRole)
        {
            EventAccess access = GetAccess(eventId, actorId, actorRole);
            if (access.Event != null && access.Event.IsDisabled)
                return "This event is restricted.";

            if (string.IsNullOrWhiteSpace(email) || email.IndexOf('@') < 1)
                return "Enter a complete email address.";

            email = email.Trim().ToLowerInvariant();
            inviteRole = NormalizeRole(inviteRole);
            bool creatorInvitingParticipant = access.Event != null
                && access.Event.CreatedBy.HasValue
                && access.Event.CreatedBy.Value == actorId
                && inviteRole == EventRoles.Participant;
            if (!access.CanManage && !creatorInvitingParticipant)
                return "Only the event administrator can assign or reassign members.";

            EmailLookupResult lookup = LookupByExactEmail(email);
            if (inviteRole == EventRoles.EventAdmin && lookup.Found
                && !RoleAccess.CanHoldEventAdmin(lookup.Role))
                return "Only Faculty, Staff, or an Administrator can be an Event Administrator.";

            if (lookup.Found && IsActiveMember(eventId, lookup.UserID.Value))
                return "That user is already a member of this event.";

            using (var con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                var existing = new SqlCommand(
                    "SELECT InvitationID, Status FROM EventInvitations WHERE EventID = @EventID AND Email = @Email", con);
                existing.Parameters.AddWithValue("@EventID", eventId);
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
                            @"UPDATE EventInvitations
                              SET Status = N'Pending', InvitedUserID = @UserID, InviteRole = @Role,
                                  InviteCode = @Code, CreatedBy = @Actor, CreatedAt = GETDATE()
                              WHERE InvitationID = @InvitationID", con);
                        update.Parameters.AddWithValue("@UserID", (object)lookup.UserID ?? DBNull.Value);
                        update.Parameters.AddWithValue("@Role", inviteRole);
                        update.Parameters.AddWithValue("@Code", access.Event.InviteCode ?? (object)DBNull.Value);
                        update.Parameters.AddWithValue("@Actor", actorId);
                        update.Parameters.AddWithValue("@InvitationID", invitationId);
                        update.ExecuteNonQuery();
                    }
                    else
                    {
                        reader.Close();
                        var insert = new SqlCommand(
                            @"INSERT INTO EventInvitations (EventID, Email, InvitedUserID, InviteCode, Status, CreatedAt, CreatedBy, InviteRole)
                              VALUES (@EventID, @Email, @UserID, @Code, N'Pending', GETDATE(), @Actor, @Role)", con);
                        insert.Parameters.AddWithValue("@EventID", eventId);
                        insert.Parameters.AddWithValue("@Email", email);
                        insert.Parameters.AddWithValue("@UserID", (object)lookup.UserID ?? DBNull.Value);
                        insert.Parameters.AddWithValue("@Code", access.Event.InviteCode ?? (object)DBNull.Value);
                        insert.Parameters.AddWithValue("@Actor", actorId);
                        insert.Parameters.AddWithValue("@Role", inviteRole);
                        insert.ExecuteNonQuery();
                    }
                }
            }

            if (lookup.Found)
            {
                Notify(lookup.UserID.Value, "Event invitation",
                    "You were invited to " + access.Event.EventName + " as " + DisplayRole(inviteRole) + ".",
                    eventId);
            }

            MailSender.Send(email, "DTAS event invitation",
                "You have been invited to the DTAS event \"" + access.Event.EventName + "\".\r\n\r\n" +
                (string.IsNullOrEmpty(access.Event.InviteCode)
                    ? "Sign in to DTAS to accept the invitation."
                    : "Sign in to DTAS, or join with code " + access.Event.InviteCode + ".") +
                "\r\n\r\nDTAS");

            AuthService.WriteAudit(actorId, "EventInvited", "Event", eventId, email, null);
            return null;
        }

        public static string AcceptInvitation(int invitationId, int userId)
        {
            UserAccount user = AuthService.FindById(userId);
            if (user == null)
                return "Account not found.";

            using (var con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                var cmd = new SqlCommand(
                    @"SELECT InvitationID, EventID, Email, InviteRole, Status
                      FROM EventInvitations WHERE InvitationID = @ID", con);
                cmd.Parameters.AddWithValue("@ID", invitationId);
                int eventId;
                string role;
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return "Invitation not found.";
                    if (!string.Equals(reader["Status"].ToString(), "Pending", StringComparison.OrdinalIgnoreCase))
                        return "That invitation is no longer pending.";
                    string email = Convert.ToString(reader["Email"]);
                    if (!EmailsMatch(email, user.Email))
                        return "This invitation was sent to a different email.";
                    eventId = Convert.ToInt32(reader["EventID"]);
                    role = NormalizeRole(Convert.ToString(reader["InviteRole"]));
                }

                var accept = new SqlCommand(
                    "UPDATE EventInvitations SET Status = N'Accepted', InvitedUserID = @UserID WHERE InvitationID = @ID", con);
                accept.Parameters.AddWithValue("@UserID", userId);
                accept.Parameters.AddWithValue("@ID", invitationId);
                accept.ExecuteNonQuery();

                AddMember(con, null, eventId, userId, role, userId, "Accepted");
            }

            AuthService.WriteAudit(userId, "EventInviteAccepted", "EventInvitation", invitationId, null, null);
            return null;
        }

        public static string DeclineInvitation(int invitationId, int userId)
        {
            UserAccount user = AuthService.FindById(userId);
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"UPDATE EventInvitations SET Status = N'Declined'
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

        public static string JoinByCode(int userId, string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return "Enter an event invitation code.";

            code = code.Trim().ToUpperInvariant();
            int eventId;
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT EventID FROM Events
                  WHERE InviteCode = @Code AND ISNULL(IsDeleted, 0) = 0", con))
            {
                cmd.Parameters.AddWithValue("@Code", code);
                con.Open();
                object id = cmd.ExecuteScalar();
                if (id == null)
                    return "That invitation code is not valid.";
                eventId = Convert.ToInt32(id);
            }

            EventRecord ev = GetEvent(eventId);
            if (ev.IsDisabled)
                return "This event is disabled.";

            if (IsActiveMember(eventId, userId))
                return "You are already a member of this event.";

            AddOrActivateMember(eventId, userId, EventRoles.Participant, userId);
            MarkInvitesAccepted(eventId, userId);
            AuthService.WriteAudit(userId, "EventJoinedByCode", "Event", eventId, code, null);
            return null;
        }

        public static string JoinPublic(int eventId, int userId)
        {
            EventRecord ev = GetEvent(eventId);
            if (ev == null || ev.IsDeleted)
                return "Event not found.";
            if (!string.Equals(ev.Visibility, "Public", StringComparison.OrdinalIgnoreCase))
                return "This event is private. Use an invitation or invitation code.";
            if (ev.IsDisabled)
                return "This event is disabled.";
            if (string.Equals(ev.Status, "Completed", StringComparison.OrdinalIgnoreCase)
                || string.Equals(ev.Status, "Cancelled", StringComparison.OrdinalIgnoreCase)
                || string.Equals(ev.Status, "Archived", StringComparison.OrdinalIgnoreCase)
                || string.Equals(ev.Status, "Proposed", StringComparison.OrdinalIgnoreCase)
                || string.Equals(ev.Status, "Rejected", StringComparison.OrdinalIgnoreCase))
                return "This event is not open to new members.";
            if (IsActiveMember(eventId, userId))
                return "You are already a member.";
            if (HasPendingJoinRequest(eventId, userId))
                return "Your request is already waiting for the event lead or manager.";

            UpsertJoinRequest(eventId, userId);
            UserAccount requester = AuthService.FindById(userId);
            string who = requester == null || string.IsNullOrWhiteSpace(requester.FullName)
                ? "Someone"
                : requester.FullName;
            NotifyEventOfficers(eventId, userId,
                "Join request",
                who + " asked to join \"" + ev.EventName + "\". Accept or decline the request in the event workplace.");
            AuthService.WriteAudit(userId, "EventJoinRequested", "Event", eventId, null, null);
            return null;
        }

        public static bool HasPendingJoinRequest(int eventId, int userId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT COUNT(*) FROM EventMembers
                  WHERE EventID = @EventID AND UserID = @UserID AND InviteStatus = N'Requested'", con))
            {
                cmd.Parameters.AddWithValue("@EventID", eventId);
                cmd.Parameters.AddWithValue("@UserID", userId);
                con.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        public static DataTable ListJoinRequests(int eventId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT em.UserID, u.FullName, u.Email, em.JoinedAt
                  FROM EventMembers em
                  INNER JOIN Users u ON u.UserID = em.UserID
                  WHERE em.EventID = @EventID AND em.InviteStatus = N'Requested'
                  ORDER BY em.JoinedAt DESC", con))
            {
                cmd.Parameters.AddWithValue("@EventID", eventId);
                var table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                return table;
            }
        }

        public static string AcceptJoinRequest(int eventId, int actorId, string actorRole, int targetUserId)
        {
            EventAccess access = GetAccess(eventId, actorId, actorRole);
            if (access.Event == null || access.Event.IsDeleted)
                return "Event not found.";
            if (access.Event.IsDisabled)
                return "This event is disabled.";
            if (!access.CanReviewJoins)
                return "Only the event administrator or manager can accept join requests.";
            if (!HasPendingJoinRequest(eventId, targetUserId))
                return "That join request is no longer pending.";

            AddOrActivateMember(eventId, targetUserId, EventRoles.Participant, actorId);
            MarkInvitesAccepted(eventId, targetUserId);
            Notify(targetUserId, "Join request accepted",
                "You were accepted into \"" + access.Event.EventName + "\". You can now work in the event workplace.",
                eventId);
            AuthService.WriteAudit(actorId, "EventJoinAccepted", "Event", eventId, "UserID " + targetUserId, null);
            return null;
        }

        public static string DeclineJoinRequest(int eventId, int actorId, string actorRole, int targetUserId)
        {
            EventAccess access = GetAccess(eventId, actorId, actorRole);
            if (access.Event == null || access.Event.IsDeleted)
                return "Event not found.";
            if (!access.CanReviewJoins)
                return "Only the event administrator or manager can decline join requests.";
            if (!HasPendingJoinRequest(eventId, targetUserId))
                return "That join request is no longer pending.";

            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"UPDATE EventMembers
                  SET InviteStatus = N'Declined', IsActive = 0
                  WHERE EventID = @EventID AND UserID = @UserID AND InviteStatus = N'Requested'", con))
            {
                cmd.Parameters.AddWithValue("@EventID", eventId);
                cmd.Parameters.AddWithValue("@UserID", targetUserId);
                con.Open();
                cmd.ExecuteNonQuery();
            }

            Notify(targetUserId, "Join request declined",
                "Your request to join \"" + access.Event.EventName + "\" was declined.",
                eventId);
            AuthService.WriteAudit(actorId, "EventJoinDeclined", "Event", eventId, "UserID " + targetUserId, null);
            return null;
        }

        public static string Leave(int eventId, int userId)
        {
            if (CountAdmins(eventId) <= 1 && string.Equals(GetMemberRole(eventId, userId), EventRoles.EventAdmin, StringComparison.OrdinalIgnoreCase))
                return "The last event administrator cannot leave. Transfer the role first.";

            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                "UPDATE EventMembers SET IsActive = 0 WHERE EventID = @EventID AND UserID = @UserID", con))
            {
                cmd.Parameters.AddWithValue("@EventID", eventId);
                cmd.Parameters.AddWithValue("@UserID", userId);
                con.Open();
                cmd.ExecuteNonQuery();
            }

            AuthService.WriteAudit(userId, "EventLeft", "Event", eventId, null, null);
            return null;
        }

        public static string SetMemberRole(int eventId, int actorId, string actorRole, int targetUserId, string newRole)
        {
            EventAccess access = GetAccess(eventId, actorId, actorRole);
            if (!access.CanManage)
                return "Only an event administrator can change responsibilities.";

            newRole = NormalizeRole(newRole);
            UserAccount target = AuthService.FindById(targetUserId);
            if (target == null)
                return "Member not found.";
            if (newRole == EventRoles.EventAdmin && !RoleAccess.CanHoldEventAdmin(target.Role))
                return "Only Faculty, Staff, or an Administrator can be an Event Administrator.";

            if (newRole != EventRoles.EventAdmin
                && string.Equals(GetMemberRole(eventId, targetUserId), EventRoles.EventAdmin, StringComparison.OrdinalIgnoreCase)
                && CountAdmins(eventId) <= 1)
                return "Keep at least one event administrator.";

            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                "UPDATE EventMembers SET RoleInEvent = @Role WHERE EventID = @EventID AND UserID = @UserID AND IsActive = 1", con))
            {
                cmd.Parameters.AddWithValue("@Role", newRole);
                cmd.Parameters.AddWithValue("@EventID", eventId);
                cmd.Parameters.AddWithValue("@UserID", targetUserId);
                con.Open();
                cmd.ExecuteNonQuery();
            }

            AuthService.WriteAudit(actorId, "EventRoleChanged", "Event", eventId, targetUserId + " -> " + newRole, null);
            return null;
        }

        public static string RemoveMember(int eventId, int actorId, string actorRole, int targetUserId)
        {
            EventAccess access = GetAccess(eventId, actorId, actorRole);
            if (!access.CanRemoveMembers)
                return "Only the event administrator or manager can remove members.";
            if (targetUserId == actorId && !access.IsSystemAdmin)
                return Leave(eventId, actorId);

            string targetRole = GetMemberRole(eventId, targetUserId);
            if (!access.CanManage
                && string.Equals(targetRole, EventRoles.EventAdmin, StringComparison.OrdinalIgnoreCase))
                return "Only the faculty in charge can remove an event administrator.";
            if (string.Equals(targetRole, EventRoles.EventAdmin, StringComparison.OrdinalIgnoreCase)
                && CountAdmins(eventId) <= 1)
                return "Keep at least one event administrator.";

            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                "UPDATE EventMembers SET IsActive = 0 WHERE EventID = @EventID AND UserID = @UserID", con))
            {
                cmd.Parameters.AddWithValue("@EventID", eventId);
                cmd.Parameters.AddWithValue("@UserID", targetUserId);
                con.Open();
                cmd.ExecuteNonQuery();
            }

            AuthService.WriteAudit(actorId, "EventMemberRemoved", "Event", eventId, targetUserId.ToString(), null);
            return null;
        }

        public static string SetDisabled(int eventId, int actorId, string actorRole, bool disabled)
        {
            EventAccess access = GetAccess(eventId, actorId, actorRole);
            if (!access.CanRestrict)
                return "Only the system administrator can restrict or restore an event.";

            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                "UPDATE Events SET IsDisabled = @Disabled, UpdatedAt = GETDATE() WHERE EventID = @EventID", con))
            {
                cmd.Parameters.AddWithValue("@Disabled", disabled);
                cmd.Parameters.AddWithValue("@EventID", eventId);
                con.Open();
                cmd.ExecuteNonQuery();
            }

            AuthService.WriteAudit(actorId, disabled ? "EventDisabled" : "EventEnabled", "Event", eventId, null, null);
            return null;
        }

        public static string AddNote(int eventId, int userId, string systemRole, string text, bool pin)
        {
            EventAccess access = GetAccess(eventId, userId, systemRole);
            if (!access.CanModify)
                return access.Event != null && access.Event.IsDisabled
                    ? "This event is disabled. Only an event administrator can add notes."
                    : "Join the event to add a note.";
            if (string.IsNullOrWhiteSpace(text))
                return "Write a note first.";
            if (pin && !access.CanManage)
                pin = false;

            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"INSERT INTO EventNotes (EventID, UserID, NoteText, IsPinned, CreatedAt)
                  VALUES (@EventID, @UserID, @Text, @Pinned, GETDATE())", con))
            {
                cmd.Parameters.AddWithValue("@EventID", eventId);
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@Text", text.Trim());
                cmd.Parameters.AddWithValue("@Pinned", pin);
                con.Open();
                cmd.ExecuteNonQuery();
            }

            return null;
        }

        public static DataTable ListNotes(int eventId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT n.NoteID, n.NoteText, n.IsPinned, n.CreatedAt, u.FullName
                  FROM EventNotes n
                  INNER JOIN Users u ON u.UserID = n.UserID
                  WHERE n.EventID = @EventID
                  ORDER BY n.IsPinned DESC, n.CreatedAt DESC", con))
            {
                cmd.Parameters.AddWithValue("@EventID", eventId);
                var table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                return table;
            }
        }

        public static DataTable ListMembers(int eventId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT em.UserID, em.RoleInEvent, em.JoinedAt, u.FullName, u.Email, r.RoleName
                  FROM EventMembers em
                  INNER JOIN Users u ON u.UserID = em.UserID
                  LEFT JOIN Roles r ON r.RoleID = u.RoleID
                  WHERE em.EventID = @EventID AND em.IsActive = 1 AND em.InviteStatus = N'Accepted'
                  ORDER BY CASE em.RoleInEvent
                             WHEN N'EventAdmin' THEN 0
                             WHEN N'EventManager' THEN 1
                             ELSE 2 END, u.FullName", con))
            {
                cmd.Parameters.AddWithValue("@EventID", eventId);
                var table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                return table;
            }
        }

        public static DataTable ListPendingInvites(int eventId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT InvitationID, Email, InviteRole, CreatedAt
                  FROM EventInvitations
                  WHERE EventID = @EventID AND Status = N'Pending'
                  ORDER BY CreatedAt DESC", con))
            {
                cmd.Parameters.AddWithValue("@EventID", eventId);
                var table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                return table;
            }
        }

        public static DataTable ListMyInvites(int userId)
        {
            UserAccount user = AuthService.FindById(userId);
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT i.InvitationID, i.EventID, i.InviteRole, i.CreatedAt, e.EventName, e.StartDate, e.Visibility
                  FROM EventInvitations i
                  INNER JOIN Events e ON e.EventID = i.EventID
                  WHERE i.Status = N'Pending'
                    AND ISNULL(e.IsDeleted, 0) = 0
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

        public static DataTable ListEventWorkspaces(int userId, bool includeAll)
        {
            string sql = includeAll
                ? @"SELECT e.EventID, e.EventName, e.Description, e.EventType, e.Status, e.EndDate,
                            ISNULL(e.IsDisabled, 0) AS IsDisabled,
                            (SELECT COUNT(*) FROM Tasks t
                             WHERE t.EventID = e.EventID AND ISNULL(t.IsDeleted, 0) = 0 AND t.Status <> N'Archived') AS TaskCount
                     FROM Events e
                     WHERE ISNULL(e.IsDeleted, 0) = 0
                     ORDER BY e.StartDate DESC"
                : @"SELECT e.EventID, e.EventName, e.Description, e.EventType, e.Status, e.EndDate,
                            ISNULL(e.IsDisabled, 0) AS IsDisabled,
                            (SELECT COUNT(*) FROM Tasks t
                             WHERE t.EventID = e.EventID AND ISNULL(t.IsDeleted, 0) = 0 AND t.Status <> N'Archived') AS TaskCount
                     FROM EventMembers em
                     INNER JOIN Events e ON e.EventID = em.EventID
                     WHERE em.UserID = @UserID AND em.IsActive = 1 AND em.InviteStatus = N'Accepted'
                       AND ISNULL(e.IsDeleted, 0) = 0
                     ORDER BY e.StartDate DESC";

            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(sql, con))
            {
                if (!includeAll)
                    cmd.Parameters.AddWithValue("@UserID", userId);
                var table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                return table;
            }
        }

        public static DataTable ListMyEvents(int userId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT e.EventID, e.EventName, e.EventType, e.StartDate, e.EndDate, e.Status,
                         ISNULL(e.Visibility, N'Private') AS Visibility, ISNULL(e.IsDisabled, 0) AS IsDisabled,
                         em.RoleInEvent
                  FROM EventMembers em
                  INNER JOIN Events e ON e.EventID = em.EventID
                  WHERE em.UserID = @UserID AND em.IsActive = 1 AND em.InviteStatus = N'Accepted'
                    AND ISNULL(e.IsDeleted, 0) = 0
                  ORDER BY e.StartDate DESC", con))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
                var table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                return table;
            }
        }

        public static DataTable ListDiscoverablePublic(int userId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT e.EventID, e.EventName, e.EventType, e.StartDate, e.EndDate, e.Status, e.Venue,
                         CASE WHEN EXISTS (
                             SELECT 1 FROM EventMembers r
                             WHERE r.EventID = e.EventID AND r.UserID = @UserID AND r.InviteStatus = N'Requested'
                         ) THEN 1 ELSE 0 END AS HasJoinRequest,
                         CASE WHEN e.Status IN (N'Completed', N'Cancelled', N'Archived') THEN 0 ELSE 1 END AS OpenToJoin
                  FROM Events e
                  WHERE ISNULL(e.Visibility, N'Private') = N'Public'
                    AND ISNULL(e.IsDeleted, 0) = 0
                    AND ISNULL(e.IsDisabled, 0) = 0
                    AND e.Status NOT IN (N'Proposed', N'Rejected', N'Archived', N'Cancelled')
                    AND NOT EXISTS (
                        SELECT 1 FROM EventMembers em
                        WHERE em.EventID = e.EventID AND em.UserID = @UserID
                          AND em.IsActive = 1 AND em.InviteStatus = N'Accepted')
                  ORDER BY e.StartDate DESC", con))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
                var table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                return table;
            }
        }

        public static DataTable ListDashboardEvents(int userId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT e.EventID, e.EventName, e.Description, e.EventType, e.StartDate, e.EndDate, e.Venue, e.Status,
                         ISNULL(e.Visibility, N'Private') AS Visibility,
                         CASE WHEN em.UserID IS NULL THEN 0 ELSE 1 END AS IsMember,
                         ISNULL(em.RoleInEvent, N'') AS RoleInEvent,
                         ISNULL((
                             SELECT COUNT(*) FROM Tasks t
                             WHERE t.EventID = e.EventID AND ISNULL(t.IsDeleted, 0) = 0 AND t.Status <> N'Archived'
                         ), 0) AS TaskTotal,
                         ISNULL((
                             SELECT COUNT(*) FROM Tasks t
                             WHERE t.EventID = e.EventID AND ISNULL(t.IsDeleted, 0) = 0 AND t.Status = N'Completed'
                         ), 0) AS TaskCompleted,
                         ISNULL((
                             SELECT COUNT(*) FROM EventMembers m
                             WHERE m.EventID = e.EventID AND m.IsActive = 1 AND m.InviteStatus = N'Accepted'
                         ), 0) AS MemberCount,
                         ISNULL(STUFF((
                             SELECT N', ' + u.FullName
                             FROM EventMembers m
                             INNER JOIN Users u ON u.UserID = m.UserID
                             WHERE m.EventID = e.EventID AND m.IsActive = 1 AND m.InviteStatus = N'Accepted'
                               AND m.RoleInEvent = N'EventAdmin'
                             FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, ''), N'') AS LeadNames,
                         ISNULL(STUFF((
                             SELECT N', ' + u.FullName
                             FROM EventMembers m
                             INNER JOIN Users u ON u.UserID = m.UserID
                             WHERE m.EventID = e.EventID AND m.IsActive = 1 AND m.InviteStatus = N'Accepted'
                               AND m.RoleInEvent = N'EventManager'
                             FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, ''), N'') AS ManagerNames,
                         CASE WHEN EXISTS (
                             SELECT 1 FROM EventMembers r
                             WHERE r.EventID = e.EventID AND r.UserID = @UserID AND r.InviteStatus = N'Requested'
                         ) THEN 1 ELSE 0 END AS HasJoinRequest
                  FROM Events e
                  LEFT JOIN EventMembers em
                    ON em.EventID = e.EventID AND em.UserID = @UserID
                   AND em.IsActive = 1 AND em.InviteStatus = N'Accepted'
                  WHERE ISNULL(e.IsDeleted, 0) = 0
                    AND ISNULL(e.IsDisabled, 0) = 0
                    AND e.Status NOT IN (N'Proposed', N'Rejected', N'Archived', N'Cancelled')
                    AND (
                        ISNULL(e.Visibility, N'Private') = N'Public'
                        OR em.UserID IS NOT NULL
                    )
                  ORDER BY CASE WHEN em.UserID IS NULL THEN 1 ELSE 0 END, e.StartDate DESC", con))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
                var table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                return table;
            }
        }

        public static DataTable ListEventTasks(int eventId)
        {
            return ListEventTasks(eventId, 0);
        }

        public static DataTable ListEventTasks(int eventId, int viewerUserId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT TaskID, TaskTitle, Description, Status, DueDate,
                         ISNULL(Priority, N'Medium') AS Priority,
                         (SELECT COUNT(*) FROM TaskComments c WHERE c.TaskID = Tasks.TaskID) AS CommentCount,
                         (SELECT COUNT(*) FROM Attachments a WHERE a.RelatedID = Tasks.TaskID AND a.RelatedType = N'Task') AS AttachmentCount
                  FROM Tasks
                  WHERE EventID = @EventID AND ISNULL(IsDeleted, 0) = 0
                  ORDER BY DueDate", con))
            {
                cmd.Parameters.AddWithValue("@EventID", eventId);
                var table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                table.Columns.Add("Assignees", typeof(string));
                table.Columns.Add("AssigneesShort", typeof(string));
                table.Columns.Add("IsAssigned", typeof(bool));
                foreach (DataRow row in table.Rows)
                {
                    int taskId = Convert.ToInt32(row["TaskID"]);
                    row["Assignees"] = EventTaskService.FormatAssignees(taskId);
                    row["AssigneesShort"] = EventTaskService.FormatAssigneeNames(taskId);
                    row["IsAssigned"] = viewerUserId > 0 && EventTaskService.ListAssigneeIds(taskId).Contains(viewerUserId);
                }
                return table;
            }
        }

        public static string CreateEventTask(int eventId, int actorId, string actorRole, string title, string description, DateTime? dueDate, IList<int> assigneeUserIds)
        {
            EventAccess access = GetAccess(eventId, actorId, actorRole);
            if (access.Event == null || access.Event.IsDeleted)
                return "Event not found.";
            if (access.Event.IsDisabled)
                return "This event is restricted.";
            if (!access.CanAssign)
                return "Only the event manager can assign tasks.";
            if (string.Equals(access.Event.Status, "Completed", StringComparison.OrdinalIgnoreCase))
                return "This event is already concluded.";

            title = (title ?? "").Trim();
            if (title.Length == 0)
                return "Enter a task title.";

            description = (description ?? "").Trim();
            if (!dueDate.HasValue)
                dueDate = access.Event.EndDate ?? access.Event.StartDate;
            if (!dueDate.HasValue)
                return "Enter a due date.";

            List<int> memberIds = EventTaskService.NormalizeAssigneeIds(eventId, assigneeUserIds);
            if (memberIds.Count == 0)
                return "Select at least one member to assign this task to.";

            int taskId;
            using (var con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                using (SqlTransaction tx = con.BeginTransaction())
                {
                    var insert = new SqlCommand(
                        @"INSERT INTO Tasks
                            (TaskTitle, Description, Priority, Status, DueDate, EventID, CreatedBy, CreatedAt, UpdatedAt, LeaderID)
                          VALUES
                            (@Title, @Description, N'Medium', N'Pending', @DueDate, @EventID, @CreatedBy, GETDATE(), GETDATE(), @LeaderID);
                          SELECT SCOPE_IDENTITY();", con, tx);
                    insert.Parameters.AddWithValue("@Title", title);
                    insert.Parameters.AddWithValue("@Description", string.IsNullOrEmpty(description) ? (object)DBNull.Value : description);
                    insert.Parameters.AddWithValue("@DueDate", dueDate.Value);
                    insert.Parameters.AddWithValue("@EventID", eventId);
                    insert.Parameters.AddWithValue("@CreatedBy", actorId);
                    insert.Parameters.AddWithValue("@LeaderID", actorId);
                    taskId = Convert.ToInt32(insert.ExecuteScalar());

                    var team = new SqlCommand(
                        @"INSERT INTO TaskTeams (TaskID, LeaderID, ProgressPercent, CreatedAt)
                          VALUES (@TaskID, @LeaderID, 0, GETDATE())", con, tx);
                    team.Parameters.AddWithValue("@TaskID", taskId);
                    team.Parameters.AddWithValue("@LeaderID", actorId);
                    team.ExecuteNonQuery();

                    EventTaskService.ApplyAssignees(con, tx, taskId, memberIds);
                    tx.Commit();
                }
            }

            foreach (int memberId in memberIds)
            {
                if (memberId == actorId)
                    continue;
                NotificationService.Send(
                    memberId,
                    "Event task assigned",
                    "You were assigned a task for " + access.Event.EventName + ": " + title,
                    "TaskAssignment",
                    taskId,
                    "Task");
            }

            AuthService.WriteAudit(actorId, "EventTaskCreated", "Task", taskId, title, null);
            return null;
        }

        public static int? FindEventIdByCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return null;
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                "SELECT EventID FROM Events WHERE InviteCode = @Code AND ISNULL(IsDeleted, 0) = 0", con))
            {
                cmd.Parameters.AddWithValue("@Code", code.Trim().ToUpperInvariant());
                con.Open();
                object id = cmd.ExecuteScalar();
                return id == null ? (int?)null : Convert.ToInt32(id);
            }
        }

        private static void UpsertJoinRequest(int eventId, int userId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                var existing = new SqlCommand(
                    "SELECT EventMemberID FROM EventMembers WHERE EventID = @EventID AND UserID = @UserID", con);
                existing.Parameters.AddWithValue("@EventID", eventId);
                existing.Parameters.AddWithValue("@UserID", userId);
                object id = existing.ExecuteScalar();
                if (id != null)
                {
                    var update = new SqlCommand(
                        @"UPDATE EventMembers
                          SET IsActive = 0, InviteStatus = N'Requested', RoleInEvent = @Role,
                              JoinedBy = @JoinedBy, JoinedAt = GETDATE()
                          WHERE EventID = @EventID AND UserID = @UserID", con);
                    update.Parameters.AddWithValue("@Role", EventRoles.Participant);
                    update.Parameters.AddWithValue("@JoinedBy", userId);
                    update.Parameters.AddWithValue("@EventID", eventId);
                    update.Parameters.AddWithValue("@UserID", userId);
                    update.ExecuteNonQuery();
                }
                else
                {
                    var insert = new SqlCommand(
                        @"INSERT INTO EventMembers (EventID, UserID, RoleInEvent, JoinedBy, JoinedAt, InviteStatus, IsActive)
                          VALUES (@EventID, @UserID, @Role, @JoinedBy, GETDATE(), N'Requested', 0)", con);
                    insert.Parameters.AddWithValue("@EventID", eventId);
                    insert.Parameters.AddWithValue("@UserID", userId);
                    insert.Parameters.AddWithValue("@Role", EventRoles.Participant);
                    insert.Parameters.AddWithValue("@JoinedBy", userId);
                    insert.ExecuteNonQuery();
                }
            }
        }

        private static void NotifyEventOfficers(int eventId, int exceptUserId, string title, string message)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT UserID FROM EventMembers
                  WHERE EventID = @EventID AND IsActive = 1 AND InviteStatus = N'Accepted'
                    AND RoleInEvent IN (N'EventAdmin', N'EventManager') AND UserID <> @Except", con))
            {
                cmd.Parameters.AddWithValue("@EventID", eventId);
                cmd.Parameters.AddWithValue("@Except", exceptUserId);
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        Notify(Convert.ToInt32(reader["UserID"]), title, message, eventId);
                }
            }
        }

        private static void AddOrActivateMember(int eventId, int userId, string role, int joinedBy)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                AddMember(con, null, eventId, userId, role, joinedBy, "Accepted");
            }
        }

        private static void AddMember(SqlConnection con, SqlTransaction tx, int eventId, int userId, string role, int joinedBy, string inviteStatus)
        {
            var existing = new SqlCommand(
                "SELECT EventMemberID FROM EventMembers WHERE EventID = @EventID AND UserID = @UserID", con, tx);
            existing.Parameters.AddWithValue("@EventID", eventId);
            existing.Parameters.AddWithValue("@UserID", userId);
            object id = existing.ExecuteScalar();
            if (id != null)
            {
                var update = new SqlCommand(
                    @"UPDATE EventMembers
                      SET IsActive = 1, InviteStatus = @Status, RoleInEvent = @Role, JoinedBy = @JoinedBy, JoinedAt = GETDATE()
                      WHERE EventID = @EventID AND UserID = @UserID", con, tx);
                update.Parameters.AddWithValue("@Status", inviteStatus);
                update.Parameters.AddWithValue("@Role", role);
                update.Parameters.AddWithValue("@JoinedBy", joinedBy);
                update.Parameters.AddWithValue("@EventID", eventId);
                update.Parameters.AddWithValue("@UserID", userId);
                update.ExecuteNonQuery();
            }
            else
            {
                var insert = new SqlCommand(
                    @"INSERT INTO EventMembers (EventID, UserID, RoleInEvent, JoinedBy, JoinedAt, InviteStatus, IsActive)
                      VALUES (@EventID, @UserID, @Role, @JoinedBy, GETDATE(), @Status, 1)", con, tx);
                insert.Parameters.AddWithValue("@EventID", eventId);
                insert.Parameters.AddWithValue("@UserID", userId);
                insert.Parameters.AddWithValue("@Role", role);
                insert.Parameters.AddWithValue("@JoinedBy", joinedBy);
                insert.Parameters.AddWithValue("@Status", inviteStatus);
                insert.ExecuteNonQuery();
            }

        }

        private static bool IsActiveMember(int eventId, int userId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT COUNT(*) FROM EventMembers
                  WHERE EventID = @EventID AND UserID = @UserID AND IsActive = 1 AND InviteStatus = N'Accepted'", con))
            {
                cmd.Parameters.AddWithValue("@EventID", eventId);
                cmd.Parameters.AddWithValue("@UserID", userId);
                con.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private static string GetMemberRole(int eventId, int userId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT RoleInEvent FROM EventMembers
                  WHERE EventID = @EventID AND UserID = @UserID AND IsActive = 1", con))
            {
                cmd.Parameters.AddWithValue("@EventID", eventId);
                cmd.Parameters.AddWithValue("@UserID", userId);
                con.Open();
                object role = cmd.ExecuteScalar();
                return role == null || role == DBNull.Value ? null : role.ToString();
            }
        }

        private static int CountAdmins(int eventId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT COUNT(*) FROM EventMembers
                  WHERE EventID = @EventID AND IsActive = 1 AND InviteStatus = N'Accepted'
                    AND RoleInEvent = N'EventAdmin'", con))
            {
                cmd.Parameters.AddWithValue("@EventID", eventId);
                con.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private static void MarkInvitesAccepted(int eventId, int userId)
        {
            UserAccount user = AuthService.FindById(userId);
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"UPDATE EventInvitations
                  SET Status = N'Accepted', InvitedUserID = @UserID
                  WHERE EventID = @EventID AND Status = N'Pending'
                    AND (InvitedUserID = @UserID OR Email = @Email)", con))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@EventID", eventId);
                cmd.Parameters.AddWithValue("@Email", user == null ? "" : (user.Email ?? "").Trim().ToLowerInvariant());
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private static void Notify(int userId, string title, string message, int eventId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"INSERT INTO Notifications (UserID, Title, Message, IsRead, NotificationType, RelatedID, RelatedType, CreatedAt)
                  VALUES (@UserID, @Title, @Message, 0, N'Event', @EventID, N'Event', GETDATE())", con))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@Title", title);
                cmd.Parameters.AddWithValue("@Message", message);
                cmd.Parameters.AddWithValue("@EventID", eventId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private static string NormalizeRole(string role)
        {
            if (string.Equals(role, EventRoles.EventAdmin, StringComparison.OrdinalIgnoreCase))
                return EventRoles.EventAdmin;
            if (string.Equals(role, EventRoles.EventManager, StringComparison.OrdinalIgnoreCase))
                return EventRoles.EventManager;
            return EventRoles.Participant;
        }

        private static bool EmailsMatch(string a, string b)
        {
            return string.Equals((a ?? "").Trim(), (b ?? "").Trim(), StringComparison.OrdinalIgnoreCase);
        }

        private static string NewInviteCode()
        {
            const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var bytes = new byte[5];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(bytes);
            var sb = new StringBuilder("DTAS-EVT-");
            for (int i = 0; i < 5; i++)
                sb.Append(alphabet[bytes[i] % alphabet.Length]);
            return sb.ToString();
        }

        private static EventRecord ReadEvent(SqlDataReader reader)
        {
            return new EventRecord
            {
                EventID = Convert.ToInt32(reader["EventID"]),
                EventName = Convert.ToString(reader["EventName"]),
                Description = Convert.ToString(reader["Description"]),
                EventType = Convert.ToString(reader["EventType"]),
                StartDate = reader["StartDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["StartDate"]),
                EndDate = reader["EndDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["EndDate"]),
                Venue = Convert.ToString(reader["Venue"]),
                Status = Convert.ToString(reader["Status"]),
                Visibility = Convert.ToString(reader["Visibility"]),
                InviteCode = reader["InviteCode"] == DBNull.Value ? null : Convert.ToString(reader["InviteCode"]),
                IsDisabled = reader["IsDisabled"] != DBNull.Value && Convert.ToBoolean(reader["IsDisabled"]),
                IsDeleted = reader["IsDeleted"] != DBNull.Value && Convert.ToBoolean(reader["IsDeleted"]),
                CreatedBy = reader["CreatedBy"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["CreatedBy"]),
                ProposedBy = reader["ProposedBy"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["ProposedBy"]),
                ClubID = reader["ClubID"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["ClubID"])
            };
        }
    }
}
