using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;

namespace DigitalTransparencySystem.Helpers
{
    public static class EventTaskService
    {
        public const string UnderReview = "UnderReview";
        public const string RevisionNeeded = "RevisionNeeded";

        public static int? GetEventId(int taskId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                "SELECT EventID FROM Tasks WHERE TaskID = @TaskID AND ISNULL(IsDeleted, 0) = 0", con))
            {
                cmd.Parameters.AddWithValue("@TaskID", taskId);
                con.Open();
                object value = cmd.ExecuteScalar();
                if (value == null || value == DBNull.Value)
                    return null;
                return Convert.ToInt32(value);
            }
        }

        public static bool IsEventLinked(int taskId)
        {
            return GetEventId(taskId).HasValue;
        }

        public static EventAccess GetEventAccessForTask(int taskId, int userId, string systemRole)
        {
            int? eventId = GetEventId(taskId);
            if (!eventId.HasValue)
                return null;
            return EventService.GetAccess(eventId.Value, userId, systemRole);
        }

        public static bool CanReview(int taskId, int userId, string systemRole)
        {
            EventAccess access = GetEventAccessForTask(taskId, userId, systemRole);
            return access != null && access.CanManage;
        }

        public static bool CanAssign(int taskId, int userId, string systemRole)
        {
            EventAccess access = GetEventAccessForTask(taskId, userId, systemRole);
            return access != null && access.CanAssign;
        }

        public static bool IsEventOfficer(int taskId, int userId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT COUNT(1)
                  FROM Tasks t
                  INNER JOIN EventMembers em ON em.EventID = t.EventID
                  WHERE t.TaskID = @TaskID
                    AND em.UserID = @UserID
                    AND em.IsActive = 1
                    AND em.InviteStatus = N'Accepted'
                    AND em.RoleInEvent IN (N'EventAdmin', N'EventManager')", con))
            {
                cmd.Parameters.AddWithValue("@TaskID", taskId);
                cmd.Parameters.AddWithValue("@UserID", userId);
                con.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        public static bool IsAcceptedEventMember(int taskId, int userId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT COUNT(1)
                  FROM Tasks t
                  INNER JOIN EventMembers em ON em.EventID = t.EventID
                  WHERE t.TaskID = @TaskID
                    AND em.UserID = @UserID
                    AND em.IsActive = 1
                    AND em.InviteStatus = N'Accepted'", con))
            {
                cmd.Parameters.AddWithValue("@TaskID", taskId);
                cmd.Parameters.AddWithValue("@UserID", userId);
                con.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        public static DataTable ListAssignableMembers(int eventId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT em.UserID, u.FullName, u.Username, u.Email, em.RoleInEvent
                  FROM EventMembers em
                  INNER JOIN Users u ON u.UserID = em.UserID
                  WHERE em.EventID = @EventID
                    AND em.IsActive = 1
                    AND em.InviteStatus = N'Accepted'
                    AND em.RoleInEvent <> N'EventAdmin'
                  ORDER BY u.FullName", con))
            {
                cmd.Parameters.AddWithValue("@EventID", eventId);
                var table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                return table;
            }
        }

        public static DataTable SearchAssignableMembers(int eventId, string query, IList<int> excludeIds)
        {
            DataTable source = ListAssignableMembers(eventId);
            var table = source.Clone();
            string term = (query ?? "").Trim();
            if (term.Length == 0)
                return table;

            var excluded = new HashSet<int>();
            if (excludeIds != null)
            {
                foreach (int id in excludeIds)
                    excluded.Add(id);
            }

            foreach (DataRow row in source.Rows)
            {
                int userId = Convert.ToInt32(row["UserID"]);
                if (excluded.Contains(userId))
                    continue;
                if (MatchesMemberSearch(row, term))
                    table.ImportRow(row);
            }
            return table;
        }

        public static DataTable ListAssigneePeople(int taskId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT DISTINCT u.UserID, u.FullName, u.Username, u.Email
                  FROM Users u
                  WHERE u.UserID IN (
                      SELECT UserID FROM TaskAssignments WHERE TaskID = @TaskID
                      UNION
                      SELECT ttm.UserID
                      FROM TaskTeamMembers ttm
                      INNER JOIN TaskTeams tt ON tt.TeamID = ttm.TeamID
                      WHERE tt.TaskID = @TaskID AND ttm.Status = N'Accepted'
                  )
                  ORDER BY u.FullName", con))
            {
                cmd.Parameters.AddWithValue("@TaskID", taskId);
                var table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                return table;
            }
        }

        public static string FormatMemberLabel(object fullName, object username, object email)
        {
            string name = Convert.ToString(fullName) ?? "";
            string user = Convert.ToString(username) ?? "";
            string mail = Convert.ToString(email) ?? "";
            if (user.Length > 0 && mail.Length > 0)
                return name + " · " + user + " · " + mail;
            if (user.Length > 0)
                return name + " · " + user;
            if (mail.Length > 0)
                return name + " · " + mail;
            return name;
        }

        public static string FormatAssignees(int taskId)
        {
            DataTable people = ListAssigneePeople(taskId);
            if (people.Rows.Count == 0)
                return "Not assigned yet";
            var parts = new List<string>();
            foreach (DataRow row in people.Rows)
                parts.Add(FormatMemberLabel(row["FullName"], row["Username"], row["Email"]));
            return string.Join("; ", parts);
        }

        public static string FormatAssigneeNames(int taskId)
        {
            DataTable people = ListAssigneePeople(taskId);
            if (people.Rows.Count == 0)
                return "Unassigned";
            var parts = new List<string>();
            foreach (DataRow row in people.Rows)
            {
                string name = Convert.ToString(row["FullName"]);
                if (!string.IsNullOrWhiteSpace(name))
                    parts.Add(name.Trim());
            }
            return parts.Count == 0 ? "Unassigned" : string.Join(", ", parts);
        }

        private static bool MatchesMemberSearch(DataRow row, string term)
        {
            string needle = term.ToLowerInvariant();
            return Contains(row["FullName"], needle)
                || Contains(row["Username"], needle)
                || Contains(row["Email"], needle);
        }

        private static bool Contains(object value, string needle)
        {
            string text = Convert.ToString(value) ?? "";
            return text.ToLowerInvariant().IndexOf(needle, StringComparison.Ordinal) >= 0;
        }

        public static List<int> ListAssigneeIds(int taskId)
        {
            var ids = new List<int>();
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT UserID FROM TaskAssignments WHERE TaskID = @TaskID
                  UNION
                  SELECT ttm.UserID
                  FROM TaskTeamMembers ttm
                  INNER JOIN TaskTeams tt ON tt.TeamID = ttm.TeamID
                  WHERE tt.TaskID = @TaskID AND ttm.Status = N'Accepted'", con))
            {
                cmd.Parameters.AddWithValue("@TaskID", taskId);
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        ids.Add(Convert.ToInt32(reader["UserID"]));
                }
            }
            return ids;
        }

        public static string ValidateWorkerStatus(int taskId, int userId, string systemRole, string newStatus)
        {
            if (!IsEventLinked(taskId))
                return null;

            EventAccess access = GetEventAccessForTask(taskId, userId, systemRole);
            if (access == null || access.Event == null)
                return "Event not found.";
            if (access.Event.IsDisabled)
                return "This event is restricted.";
            if (string.Equals(access.Event.Status, "Completed", StringComparison.OrdinalIgnoreCase)
                || string.Equals(access.Event.Status, "Cancelled", StringComparison.OrdinalIgnoreCase)
                || string.Equals(access.Event.Status, "Archived", StringComparison.OrdinalIgnoreCase))
                return "This event is already closed.";

            if (string.IsNullOrWhiteSpace(newStatus))
                return "Select a status.";

            if (access.CanManage)
                return "Use review to approve or send this task back.";

            if (!IsAllowedWorkerStatus(newStatus))
                return "Members can work on the task or submit it for faculty review. The event administrator gives the final approval.";

            return null;
        }

        public static string Review(int taskId, int actorId, string systemRole, bool approve, string comment)
        {
            if (!CanReview(taskId, actorId, systemRole))
                return "Only the event administrator can review this task.";

            EventAccess access = GetEventAccessForTask(taskId, actorId, systemRole);
            if (access.Event.IsDisabled)
                return "This event is restricted.";

            comment = (comment ?? "").Trim();
            if (!approve && comment.Length == 0)
                return "Add a comment so the member knows what to change.";
            if (approve && comment.Length == 0)
                comment = "Approved by the event administrator.";

            string newStatus = approve ? "Completed" : RevisionNeeded;
            string oldStatus = ReadStatus(taskId);
            WriteStatusAndComment(taskId, actorId, oldStatus, newStatus, comment);

            string title = ReadTitle(taskId);
            string message = approve
                ? "Faculty approved \"" + title + "\"."
                : "Faculty asked for changes on \"" + title + "\": " + comment;
            NotifyAssignees(taskId, actorId, approve ? "Task approved" : "Task needs changes", message);

            bool concluded = approve && access.Event != null && AllOpenTasksApproved(access.Event.EventID);
            return concluded ? "approved-concluded" : null;
        }

        public static bool TryConcludeEvent(int eventId, int actorId)
        {
            if (!AllOpenTasksApproved(eventId))
                return false;

            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"UPDATE Events
                  SET Status = N'Completed', UpdatedAt = GETDATE()
                  WHERE EventID = @EventID
                    AND Status NOT IN (N'Completed', N'Cancelled', N'Archived', N'Rejected')", con))
            {
                cmd.Parameters.AddWithValue("@EventID", eventId);
                con.Open();
                if (cmd.ExecuteNonQuery() == 0)
                    return false;
            }

            EventRecord ev = EventService.GetEvent(eventId);
            string name = ev == null ? "the event" : ev.EventName;
            NotifyEventMembers(eventId, actorId,
                "Event concluded",
                "Faculty approved the last task. \"" + name + "\" is now complete.");
            AuthService.WriteAudit(actorId, "EventConcluded", "Event", eventId, null, null);
            return true;
        }

        public static bool AllOpenTasksApproved(int eventId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                var any = new SqlCommand(
                    @"SELECT COUNT(*) FROM Tasks
                      WHERE EventID = @EventID AND ISNULL(IsDeleted, 0) = 0 AND Status <> N'Archived'", con);
                any.Parameters.AddWithValue("@EventID", eventId);
                if (Convert.ToInt32(any.ExecuteScalar()) == 0)
                    return false;

                var open = new SqlCommand(
                    @"SELECT COUNT(*) FROM Tasks
                      WHERE EventID = @EventID
                        AND ISNULL(IsDeleted, 0) = 0
                        AND Status NOT IN (N'Completed', N'Cancelled', N'Archived')", con);
                open.Parameters.AddWithValue("@EventID", eventId);
                return Convert.ToInt32(open.ExecuteScalar()) == 0;
            }
        }

        public static string SyncEventStatusForTask(int taskId, int actorId)
        {
            int? eventId = GetEventId(taskId);
            if (!eventId.HasValue)
                return null;
            return SyncEventStatusFromTasks(eventId.Value, actorId);
        }

        public static string SyncEventStatusFromTasks(int eventId, int actorId)
        {
            if (eventId <= 0)
                return null;

            EventRecord ev = EventService.GetEvent(eventId);
            if (ev == null || ev.IsDeleted)
                return null;

            string current = NormalizeStatusKey(ev.Status);
            if (current == "Proposed" || current == "Rejected" || current == "Cancelled" || current == "Archived")
                return current;

            string derived = DeriveEventStatusFromTasks(eventId);
            if (string.IsNullOrEmpty(derived))
                return current;

            if (derived == "Completed")
            {
                if (current == "Completed")
                    return "Completed";
                return TryConcludeEvent(eventId, actorId) ? "Completed" : current;
            }

            if (derived == "InProgress" && current != "InProgress" && current != "Completed")
            {
                using (var con = new SqlConnection(AuthService.ConnectionString))
                using (var cmd = new SqlCommand(
                    @"SET QUOTED_IDENTIFIER ON;
                      UPDATE Events
                      SET Status = N'InProgress', UpdatedAt = GETDATE()
                      WHERE EventID = @EventID
                        AND ISNULL(IsDeleted, 0) = 0
                        AND REPLACE(LTRIM(RTRIM(ISNULL(Status, N''))), N' ', N'') IN
                            (N'Planned', N'Created', N'Scheduled', N'Approved', N'Ongoing')", con))
                {
                    cmd.Parameters.AddWithValue("@EventID", eventId);
                    con.Open();
                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        AuthService.WriteAudit(actorId, "EventInProgress", "Event", eventId, ev.EventName, null);
                        return "InProgress";
                    }
                }
            }

            if (derived == "Planned" && current == "InProgress")
            {
                using (var con = new SqlConnection(AuthService.ConnectionString))
                using (var cmd = new SqlCommand(
                    @"UPDATE Events
                      SET Status = N'Planned', UpdatedAt = GETDATE()
                      WHERE EventID = @EventID
                        AND ISNULL(IsDeleted, 0) = 0
                        AND REPLACE(LTRIM(RTRIM(ISNULL(Status, N''))), N' ', N'') = N'InProgress'", con))
                {
                    cmd.Parameters.AddWithValue("@EventID", eventId);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
                return "Planned";
            }

            return current;
        }

        public static int PromoteEventsInProgressFromTasks()
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SET QUOTED_IDENTIFIER ON;
                  UPDATE e
                  SET Status = N'InProgress', UpdatedAt = GETDATE()
                  FROM Events e
                  WHERE ISNULL(e.IsDeleted, 0) = 0
                    AND REPLACE(LTRIM(RTRIM(ISNULL(e.Status, N''))), N' ', N'') IN
                        (N'Planned', N'Created', N'Scheduled', N'Approved', N'Ongoing')
                    AND EXISTS (
                        SELECT 1 FROM Tasks t
                        WHERE t.EventID = e.EventID
                          AND ISNULL(t.IsDeleted, 0) = 0
                          AND REPLACE(LTRIM(RTRIM(ISNULL(t.Status, N''))), N' ', N'') IN
                              (N'InProgress', N'UnderReview', N'Delayed', N'RevisionNeeded', N'Submitted', N'ChangesRequested')
                    )", con))
            {
                con.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        private static string DeriveEventStatusFromTasks(int eventId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT
                    COUNT(*) AS Total,
                    SUM(CASE WHEN REPLACE(LTRIM(RTRIM(ISNULL(Status, N''))), N' ', N'') = N'Completed' THEN 1 ELSE 0 END) AS Completed,
                    SUM(CASE WHEN REPLACE(LTRIM(RTRIM(ISNULL(Status, N''))), N' ', N'') IN
                        (N'InProgress', N'UnderReview', N'Delayed', N'RevisionNeeded', N'Submitted', N'ChangesRequested')
                        THEN 1 ELSE 0 END) AS InWork
                  FROM Tasks
                  WHERE EventID = @EventID
                    AND ISNULL(IsDeleted, 0) = 0
                    AND REPLACE(LTRIM(RTRIM(ISNULL(Status, N''))), N' ', N'') NOT IN (N'Archived', N'Cancelled')", con))
            {
                cmd.Parameters.AddWithValue("@EventID", eventId);
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;
                    int total = reader["Total"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Total"]);
                    if (total == 0)
                        return null;
                    int completed = reader["Completed"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Completed"]);
                    int inWork = reader["InWork"] == DBNull.Value ? 0 : Convert.ToInt32(reader["InWork"]);
                    if (completed == total)
                        return "Completed";
                    if (inWork > 0 || completed > 0)
                        return "InProgress";
                    return "Planned";
                }
            }
        }

        private static string NormalizeStatusKey(string status)
        {
            return (status ?? "").Replace(" ", "").Trim();
        }

        public static string SetAssignees(int taskId, int actorId, string systemRole, IList<int> userIds)
        {
            if (!CanAssign(taskId, actorId, systemRole))
                return "Only the event manager can assign members to this task.";

            int? eventId = GetEventId(taskId);
            if (!eventId.HasValue)
                return "This task is not linked to an event.";

            var selected = NormalizeAssigneeIds(eventId.Value, userIds);
            if (selected.Count == 0)
                return "Select at least one member to assign.";

            ApplyAssignees(taskId, selected);
            return null;
        }

        public static void ApplyAssignees(int taskId, IList<int> userIds)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                using (SqlTransaction tx = con.BeginTransaction())
                {
                    ApplyAssignees(con, tx, taskId, userIds);
                    tx.Commit();
                }
            }
        }

        public static void ApplyAssignees(SqlConnection con, SqlTransaction tx, int taskId, IList<int> userIds)
        {
            var clearAssign = new SqlCommand("DELETE FROM TaskAssignments WHERE TaskID = @TaskID", con, tx);
            clearAssign.Parameters.AddWithValue("@TaskID", taskId);
            clearAssign.ExecuteNonQuery();

            var clearTeam = new SqlCommand(
                @"DELETE ttm
                  FROM TaskTeamMembers ttm
                  INNER JOIN TaskTeams tt ON tt.TeamID = ttm.TeamID
                  WHERE tt.TaskID = @TaskID", con, tx);
            clearTeam.Parameters.AddWithValue("@TaskID", taskId);
            clearTeam.ExecuteNonQuery();

            foreach (int userId in userIds)
            {
                var assign = new SqlCommand(
                    "INSERT INTO TaskAssignments (TaskID, UserID, AssignedAt) VALUES (@TaskID, @UserID, GETDATE())", con, tx);
                assign.Parameters.AddWithValue("@TaskID", taskId);
                assign.Parameters.AddWithValue("@UserID", userId);
                assign.ExecuteNonQuery();

                var member = new SqlCommand(
                    @"INSERT INTO TaskTeamMembers (TeamID, UserID, Status, InvitedAt, RespondedAt)
                      SELECT tt.TeamID, @UserID, N'Accepted', GETDATE(), GETDATE()
                      FROM TaskTeams tt WHERE tt.TaskID = @TaskID", con, tx);
                member.Parameters.AddWithValue("@TaskID", taskId);
                member.Parameters.AddWithValue("@UserID", userId);
                member.ExecuteNonQuery();
            }
        }

        public static void NotifyEventAdmins(int eventId, int exceptUserId, string title, string message, int relatedId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT UserID FROM EventMembers
                  WHERE EventID = @EventID AND IsActive = 1 AND InviteStatus = N'Accepted'
                    AND RoleInEvent = N'EventAdmin' AND UserID <> @Except", con))
            {
                cmd.Parameters.AddWithValue("@EventID", eventId);
                cmd.Parameters.AddWithValue("@Except", exceptUserId);
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        NotificationService.Send(
                            Convert.ToInt32(reader["UserID"]),
                            title,
                            message,
                            "TaskReview",
                            relatedId,
                            "Task");
                    }
                }
            }
        }

        public static List<int> NormalizeAssigneeIds(int eventId, IList<int> userIds)
        {
            var allowed = new HashSet<int>();
            foreach (DataRow row in ListAssignableMembers(eventId).Rows)
                allowed.Add(Convert.ToInt32(row["UserID"]));

            var selected = new List<int>();
            if (userIds == null)
                return selected;
            foreach (int id in userIds)
            {
                if (allowed.Contains(id) && !selected.Contains(id))
                    selected.Add(id);
            }
            return selected;
        }

        public static bool BelongsToEvent(int eventId, int taskId)
        {
            int? linked = GetEventId(taskId);
            return linked.HasValue && linked.Value == eventId;
        }

        public static bool IsAssignedWorker(int taskId, int userId)
        {
            return ListAssigneeIds(taskId).Contains(userId);
        }

        public static DataTable ListComments(int taskId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT tc.CommentID, tc.Comment, tc.CreatedAt, u.FullName
                  FROM TaskComments tc
                  INNER JOIN Users u ON u.UserID = tc.UserID
                  WHERE tc.TaskID = @TaskID
                  ORDER BY tc.CreatedAt ASC", con))
            {
                cmd.Parameters.AddWithValue("@TaskID", taskId);
                var table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                return table;
            }
        }

        public static string AddComment(int eventId, int taskId, int userId, string systemRole, string comment)
        {
            EventAccess access = EventService.GetAccess(eventId, userId, systemRole);
            if (access == null || access.Event == null || !access.IsMember)
                return "Only event members can comment in this workplace.";
            if (access.Event.IsDisabled)
                return "This event is restricted.";
            if (!BelongsToEvent(eventId, taskId))
                return "That task is not part of this event.";

            comment = (comment ?? "").Trim();
            if (comment.Length == 0)
                return "Enter a comment.";

            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"INSERT INTO TaskComments (TaskID, UserID, Comment, CreatedAt)
                  VALUES (@TaskID, @UserID, @Comment, GETDATE())", con))
            {
                cmd.Parameters.AddWithValue("@TaskID", taskId);
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@Comment", comment);
                con.Open();
                cmd.ExecuteNonQuery();
            }

            NotifyAssignees(taskId, userId, "New comment on task",
                "A comment was added on \"" + ReadTitle(taskId) + "\".");
            return null;
        }

        public static string PostProgress(int eventId, int taskId, int userId, string systemRole, string newStatus, string comment)
        {
            if (!BelongsToEvent(eventId, taskId))
                return "That task is not part of this event.";
            if (!IsAssignedWorker(taskId, userId))
                return "You can only update progress on a task assigned to you.";

            string blocked = ValidateWorkerStatus(taskId, userId, systemRole, newStatus);
            if (blocked != null)
                return blocked;

            comment = (comment ?? "").Trim();
            if (string.Equals(newStatus, UnderReview, StringComparison.OrdinalIgnoreCase) && comment.Length == 0)
                return "Add a note for faculty before submitting for review.";

            string oldStatus = ReadStatus(taskId);
            WriteStatusAndComment(taskId, userId, oldStatus, newStatus, comment);

            if (string.Equals(newStatus, UnderReview, StringComparison.OrdinalIgnoreCase))
            {
                NotifyEventAdmins(eventId, userId, "Task ready for review",
                    "A member submitted \"" + ReadTitle(taskId) + "\" for review.", taskId);
            }

            return null;
        }

        public static bool IsAllowedWorkerStatus(string status)
        {
            return string.Equals(status, "Pending", StringComparison.OrdinalIgnoreCase)
                || string.Equals(status, "NotStarted", StringComparison.OrdinalIgnoreCase)
                || string.Equals(status, "InProgress", StringComparison.OrdinalIgnoreCase)
                || string.Equals(status, "Delayed", StringComparison.OrdinalIgnoreCase)
                || string.Equals(status, UnderReview, StringComparison.OrdinalIgnoreCase)
                || string.Equals(status, RevisionNeeded, StringComparison.OrdinalIgnoreCase);
        }

        private static string ReadStatus(int taskId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand("SELECT Status FROM Tasks WHERE TaskID = @TaskID", con))
            {
                cmd.Parameters.AddWithValue("@TaskID", taskId);
                con.Open();
                object value = cmd.ExecuteScalar();
                return value == null || value == DBNull.Value ? "" : value.ToString();
            }
        }

        private static string ReadTitle(int taskId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand("SELECT TaskTitle FROM Tasks WHERE TaskID = @TaskID", con))
            {
                cmd.Parameters.AddWithValue("@TaskID", taskId);
                con.Open();
                object value = cmd.ExecuteScalar();
                return value == null || value == DBNull.Value ? "Task" : value.ToString();
            }
        }

        private static void WriteStatusAndComment(int taskId, int userId, string oldStatus, string newStatus, string comment)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                using (SqlTransaction tx = con.BeginTransaction())
                {
                    var update = new SqlCommand(
                        "UPDATE Tasks SET Status = @Status, UpdatedAt = GETDATE() WHERE TaskID = @TaskID", con, tx);
                    update.Parameters.AddWithValue("@Status", newStatus);
                    update.Parameters.AddWithValue("@TaskID", taskId);
                    update.ExecuteNonQuery();

                    var insert = new SqlCommand(
                        @"INSERT INTO TaskUpdates (TaskID, UserID, OldStatus, NewStatus, Comment, UpdatedAt)
                          VALUES (@TaskID, @UserID, @OldStatus, @NewStatus, @Comment, GETDATE())", con, tx);
                    insert.Parameters.AddWithValue("@TaskID", taskId);
                    insert.Parameters.AddWithValue("@UserID", userId);
                    insert.Parameters.AddWithValue("@OldStatus", oldStatus ?? "");
                    insert.Parameters.AddWithValue("@NewStatus", newStatus);
                    insert.Parameters.AddWithValue("@Comment", comment ?? "");
                    insert.ExecuteNonQuery();
                    tx.Commit();
                }
            }

            SyncEventStatusForTask(taskId, userId);
        }

        private static void NotifyAssignees(int taskId, int exceptUserId, string title, string message)
        {
            NotificationService.NotifyUsers(ListAssigneeIds(taskId), exceptUserId, title, message, "TaskReview", taskId, "Task");
        }

        private static void NotifyEventMembers(int eventId, int exceptUserId, string title, string message)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT UserID FROM EventMembers
                  WHERE EventID = @EventID AND IsActive = 1 AND InviteStatus = N'Accepted'", con))
            {
                cmd.Parameters.AddWithValue("@EventID", eventId);
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int userId = Convert.ToInt32(reader["UserID"]);
                        if (userId == exceptUserId)
                            continue;
                        NotificationService.Send(userId, title, message, "Event", eventId, "Event");
                    }
                }
            }
        }

        public const int MaxAttachmentBytes = 1000 * 1024 * 1024;

        private static readonly HashSet<string> AllowedAttachmentExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png", ".zip"
        };

        public static DataTable ListAttachments(int taskId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT a.AttachmentID, a.FileName, a.FilePath, a.FileType, a.UploadedAt, ISNULL(u.FullName, N'') AS FullName
                  FROM Attachments a
                  LEFT JOIN Users u ON u.UserID = a.UploadedBy
                  WHERE a.RelatedID = @TaskID AND a.RelatedType = N'Task'
                  ORDER BY a.UploadedAt DESC", con))
            {
                cmd.Parameters.AddWithValue("@TaskID", taskId);
                var table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                return table;
            }
        }

        public static EventAttachmentRecord GetAttachment(int attachmentId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT a.AttachmentID, a.FileName, a.FilePath, a.RelatedID AS TaskID, t.EventID
                  FROM Attachments a
                  INNER JOIN Tasks t ON t.TaskID = a.RelatedID
                  WHERE a.AttachmentID = @ID AND a.RelatedType = N'Task' AND t.EventID IS NOT NULL
                    AND ISNULL(t.IsDeleted, 0) = 0", con))
            {
                cmd.Parameters.AddWithValue("@ID", attachmentId);
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;
                    return new EventAttachmentRecord
                    {
                        AttachmentID = Convert.ToInt32(reader["AttachmentID"]),
                        FileName = Convert.ToString(reader["FileName"]),
                        FilePath = Convert.ToString(reader["FilePath"]),
                        TaskID = Convert.ToInt32(reader["TaskID"]),
                        EventID = reader["EventID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["EventID"])
                    };
                }
            }
        }

        public static string AttachmentPhysicalPath(string relativePath, HttpServerUtility server)
        {
            if (server == null || string.IsNullOrWhiteSpace(relativePath))
                return null;
            if (!relativePath.StartsWith("~/Uploads/EventTaskFiles/", StringComparison.OrdinalIgnoreCase)
                && !relativePath.StartsWith("~/Uploads/TaskEvidence/", StringComparison.OrdinalIgnoreCase))
                return null;
            return server.MapPath(relativePath);
        }

        public static string SaveAttachments(int eventId, int taskId, int userId, string systemRole, IList<HttpPostedFile> files, HttpServerUtility server)
        {
            if (!BelongsToEvent(eventId, taskId))
                return "That task is not part of this event.";
            EventAccess access = EventService.GetAccess(eventId, userId, systemRole);
            if (access == null || access.Event == null || access.Event.IsDisabled)
                return "This event is restricted.";
            if (string.Equals(access.Event.Status, "Completed", StringComparison.OrdinalIgnoreCase))
                return "This event is already concluded.";
            if (!IsAssignedWorker(taskId, userId))
                return "You can only upload files on a task assigned to you.";
            if (files == null || files.Count == 0)
                return "Choose a PDF, Word, image, or ZIP file.";
            if (server == null)
                return "Upload is unavailable.";

            for (int i = 0; i < files.Count; i++)
            {
                string error = SaveOneAttachment(files[i], server, eventId, taskId, userId, i);
                if (error != null)
                    return error;
            }
            return null;
        }

        private static string SaveOneAttachment(HttpPostedFile file, HttpServerUtility server, int eventId, int taskId, int userId, int index)
        {
            if (file == null || file.ContentLength <= 0)
                return "Choose a file to upload.";
            if (file.ContentLength > MaxAttachmentBytes)
                return "Each file must be 1000 MB or smaller.";

            string originalName = Path.GetFileName(file.FileName ?? "");
            if (string.IsNullOrWhiteSpace(originalName))
                return "Choose a file to upload.";

            string ext = Path.GetExtension(originalName);
            if (string.IsNullOrEmpty(ext) || !AllowedAttachmentExtensions.Contains(ext))
                return "Upload a PDF, Word document, JPG, PNG, or ZIP file.";
            if (originalName.Length > 200)
                originalName = originalName.Substring(0, 200);

            string folder = server.MapPath("~/Uploads/EventTaskFiles/" + eventId);
            Directory.CreateDirectory(folder);
            string stored = taskId + "_" + DateTime.UtcNow.Ticks + "_" + index + ext.ToLowerInvariant();
            string relative = "~/Uploads/EventTaskFiles/" + eventId + "/" + stored;
            file.SaveAs(Path.Combine(folder, stored));

            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"INSERT INTO Attachments (FileName, FilePath, FileSize, FileType, UploadedBy, RelatedID, RelatedType, UploadedAt)
                  VALUES (@FileName, @FilePath, @FileSize, @FileType, @UploadedBy, @RelatedID, N'Task', GETDATE())", con))
            {
                cmd.Parameters.AddWithValue("@FileName", originalName);
                cmd.Parameters.AddWithValue("@FilePath", relative);
                cmd.Parameters.AddWithValue("@FileSize", file.ContentLength);
                cmd.Parameters.AddWithValue("@FileType", ext.ToLowerInvariant());
                cmd.Parameters.AddWithValue("@UploadedBy", userId);
                cmd.Parameters.AddWithValue("@RelatedID", taskId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
            return null;
        }

        private static bool mentionSchemaReady;

        public static void EnsureCommentMentionSchema()
        {
            if (mentionSchemaReady)
                return;
            using (var con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                new SqlCommand(@"
                    IF COL_LENGTH('dbo.TaskComments', 'MentionedUserID') IS NULL
                        ALTER TABLE dbo.TaskComments ADD MentionedUserID INT NULL;", con).ExecuteNonQuery();
            }
            mentionSchemaReady = true;
        }

        public static bool CanViewTask(int taskId, int userId, string role)
        {
            if (RoleAccess.IsAdmin(role))
                return true;

            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT COUNT(1) FROM Tasks t
                  WHERE t.TaskID = @TaskID AND ISNULL(t.IsDeleted, 0) = 0 AND " + TaskAccess.UserCanSeeTask, con))
            {
                cmd.Parameters.AddWithValue("@TaskID", taskId);
                cmd.Parameters.AddWithValue("@UserID", userId);
                con.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        public static DataTable ListCommentTargets(int taskId)
        {
            int? eventId = GetEventId(taskId);
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT u.UserID, u.FullName,
                         ISNULL(STUFF((
                             SELECT N', ' + t2.TaskTitle
                             FROM TaskAssignments ta2
                             INNER JOIN Tasks t2 ON t2.TaskID = ta2.TaskID
                             WHERE ta2.UserID = u.UserID
                               AND (
                                    t2.TaskID = @TaskID
                                    OR (@EventID IS NOT NULL AND t2.EventID = @EventID)
                               )
                               AND ISNULL(t2.IsDeleted, 0) = 0
                               AND t2.Status <> N'Archived'
                             FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, ''), N'') AS AssignedTasks
                  FROM Users u
                  WHERE ISNULL(u.IsDeleted, 0) = 0
                    AND (
                        EXISTS (SELECT 1 FROM TaskAssignments ta WHERE ta.TaskID = @TaskID AND ta.UserID = u.UserID)
                        OR (
                            @EventID IS NOT NULL
                            AND EXISTS (
                                SELECT 1 FROM EventMembers em
                                WHERE em.EventID = @EventID
                                  AND em.UserID = u.UserID
                                  AND em.IsActive = 1
                                  AND em.InviteStatus = N'Accepted'
                            )
                        )
                        OR (
                            @EventID IS NOT NULL
                            AND EXISTS (
                                SELECT 1 FROM TaskAssignments ta
                                INNER JOIN Tasks t ON t.TaskID = ta.TaskID
                                WHERE ta.UserID = u.UserID
                                  AND t.EventID = @EventID
                                  AND ISNULL(t.IsDeleted, 0) = 0
                                  AND t.Status <> N'Archived'
                            )
                        )
                    )
                  ORDER BY u.FullName", con))
            {
                cmd.Parameters.AddWithValue("@TaskID", taskId);
                cmd.Parameters.AddWithValue("@EventID", (object)eventId ?? DBNull.Value);
                var table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                return table;
            }
        }

        public static bool IsValidCommentTarget(int taskId, int mentionedUserId)
        {
            foreach (DataRow row in ListCommentTargets(taskId).Rows)
            {
                if (Convert.ToInt32(row["UserID"]) == mentionedUserId)
                    return true;
            }
            return false;
        }

        public static EventTeamProgress GetEventTeamProgress(int eventId)
        {
            var progress = new EventTeamProgress
            {
                EventId = eventId,
                Members = new List<EventMemberProgress>()
            };

            using (var con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                using (var cmd = new SqlCommand("SELECT EventName FROM Events WHERE EventID = @EventID", con))
                {
                    cmd.Parameters.AddWithValue("@EventID", eventId);
                    object name = cmd.ExecuteScalar();
                    progress.EventName = name == null || name == DBNull.Value ? "" : name.ToString();
                }

                using (var cmd = new SqlCommand(
                    @"SELECT COUNT(*) AS Total,
                             SUM(CASE WHEN Status = N'Completed' THEN 1 ELSE 0 END) AS Completed
                      FROM Tasks
                      WHERE EventID = @EventID
                        AND ISNULL(IsDeleted, 0) = 0
                        AND Status <> N'Archived'", con))
                {
                    cmd.Parameters.AddWithValue("@EventID", eventId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            progress.TotalTasks = reader["Total"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Total"]);
                            progress.CompletedTasks = reader["Completed"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Completed"]);
                        }
                    }
                }

                progress.Percent = PercentFromCounts(progress.CompletedTasks, progress.TotalTasks);

                using (var cmd = new SqlCommand(
                    @"SELECT u.UserID, u.FullName,
                             COUNT(*) AS AssignedTasks,
                             SUM(CASE WHEN t.Status = N'Completed' THEN 1 ELSE 0 END) AS CompletedTasks
                      FROM TaskAssignments ta
                      INNER JOIN Tasks t ON t.TaskID = ta.TaskID
                      INNER JOIN Users u ON u.UserID = ta.UserID
                      WHERE t.EventID = @EventID
                        AND ISNULL(t.IsDeleted, 0) = 0
                        AND t.Status <> N'Archived'
                      GROUP BY u.UserID, u.FullName
                      ORDER BY u.FullName", con))
                {
                    cmd.Parameters.AddWithValue("@EventID", eventId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int assigned = Convert.ToInt32(reader["AssignedTasks"]);
                            int done = reader["CompletedTasks"] == DBNull.Value ? 0 : Convert.ToInt32(reader["CompletedTasks"]);
                            progress.Members.Add(new EventMemberProgress
                            {
                                UserId = Convert.ToInt32(reader["UserID"]),
                                FullName = Convert.ToString(reader["FullName"]),
                                AssignedTasks = assigned,
                                CompletedTasks = done,
                                Percent = assigned <= 0 ? 0 : (int)Math.Round((done * 100.0) / assigned)
                            });
                        }
                    }
                }
            }

            return progress;
        }

        public static List<EventLiveProgress> ListLiveEventProgress()
        {
            return ListLiveEventProgress(null);
        }

        public static EventLiveProgress GetLiveEventProgress(int eventId)
        {
            List<EventLiveProgress> rows = ListLiveEventProgress(eventId);
            return rows.Count > 0 ? rows[0] : null;
        }

        public static List<EventLiveProgress> ListLiveEventProgress(int? eventId)
        {
            var list = new List<EventLiveProgress>();
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT e.EventID, e.EventName,
                         CASE
                             WHEN REPLACE(LTRIM(RTRIM(ISNULL(e.Status, N''))), N' ', N'') IN (N'Proposed', N'Rejected', N'Cancelled', N'Archived')
                                 THEN LTRIM(RTRIM(e.Status))
                             WHEN ts.Total > 0 AND ts.Completed = ts.Total THEN N'Completed'
                             WHEN ISNULL(ts.InWork, 0) > 0 OR (ISNULL(ts.Completed, 0) > 0 AND ISNULL(ts.Completed, 0) < ts.Total) THEN N'InProgress'
                             ELSE LTRIM(RTRIM(ISNULL(e.Status, N'Planned')))
                         END AS Status,
                         ISNULL(ts.Total, 0) AS TaskTotal,
                         ISNULL(ts.Completed, 0) AS TaskCompleted
                  FROM Events e
                  OUTER APPLY (
                      SELECT COUNT(*) AS Total,
                             SUM(CASE WHEN REPLACE(LTRIM(RTRIM(ISNULL(t.Status, N''))), N' ', N'') = N'Completed' THEN 1 ELSE 0 END) AS Completed,
                             SUM(CASE WHEN REPLACE(LTRIM(RTRIM(ISNULL(t.Status, N''))), N' ', N'') IN
                                 (N'InProgress', N'UnderReview', N'Delayed', N'RevisionNeeded', N'Submitted', N'ChangesRequested')
                                 THEN 1 ELSE 0 END) AS InWork
                      FROM Tasks t
                      WHERE t.EventID = e.EventID
                        AND ISNULL(t.IsDeleted, 0) = 0
                        AND REPLACE(LTRIM(RTRIM(ISNULL(t.Status, N''))), N' ', N'') NOT IN (N'Archived', N'Cancelled')
                  ) ts
                  WHERE ISNULL(e.IsDeleted, 0) = 0
                    AND (@EventID IS NULL OR e.EventID = @EventID)", con))
            {
                cmd.Parameters.AddWithValue("@EventID", (object)eventId ?? DBNull.Value);
                var table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                foreach (DataRow row in table.Rows)
                {
                    int total = row["TaskTotal"] == DBNull.Value ? 0 : Convert.ToInt32(row["TaskTotal"]);
                    int completed = row["TaskCompleted"] == DBNull.Value ? 0 : Convert.ToInt32(row["TaskCompleted"]);
                    string status = row["Status"] == DBNull.Value ? "Planned" : row["Status"].ToString();
                    list.Add(new EventLiveProgress
                    {
                        EventId = Convert.ToInt32(row["EventID"]),
                        EventName = row["EventName"] == DBNull.Value ? "" : row["EventName"].ToString(),
                        Status = status,
                        StatusKey = NormalizeStatusKey(status).ToLowerInvariant(),
                        TotalTasks = total,
                        CompletedTasks = completed,
                        Percent = PercentFromCounts(completed, total)
                    });
                }
            }
            return list;
        }

        public static int LifecycleIndex(string status)
        {
            switch (NormalizeStatusKey(status).ToLowerInvariant())
            {
                case "proposed":
                case "created":
                    return 0;
                case "planned":
                case "scheduled":
                case "approved":
                    return 1;
                case "inprogress":
                case "ongoing":
                    return 2;
                case "completed":
                    return 3;
                case "archived":
                    return 4;
                default:
                    return 0;
            }
        }

        public static int PercentFromCounts(int completed, int total)
        {
            if (total <= 0)
                return 0;
            int pct = (int)Math.Round((completed * 100.0) / total);
            if (pct < 0) return 0;
            if (pct > 100) return 100;
            return pct;
        }

        public static string CompletionLabel(int completed, int total)
        {
            if (total <= 0)
                return "No tasks yet";
            return completed + " of " + total + " tasks";
        }

        public static string EventStatusDisplay(string status)
        {
            switch (NormalizeStatusKey(status).ToLowerInvariant())
            {
                case "created": return "Created";
                case "planned": return "Planned";
                case "inprogress": return "In Progress";
                case "completed": return "Completed";
                case "cancelled": return "Cancelled";
                case "archived": return "Archived";
                case "proposed": return "Proposed";
                case "rejected": return "Rejected";
                default: return string.IsNullOrWhiteSpace(status) ? status : status.Trim();
            }
        }
    }

    public class EventAttachmentRecord
    {
        public int AttachmentID { get; set; }
        public int TaskID { get; set; }
        public int EventID { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
    }

    public class EventMemberProgress
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public int AssignedTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int Percent { get; set; }
    }

    public class EventTeamProgress
    {
        public int EventId { get; set; }
        public string EventName { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int Percent { get; set; }
        public List<EventMemberProgress> Members { get; set; }
    }

    public class EventLiveProgress
    {
        public int EventId { get; set; }
        public string EventName { get; set; }
        public string Status { get; set; }
        public string StatusKey { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int Percent { get; set; }
    }
}
