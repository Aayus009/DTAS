using System;
using System.Data.SqlClient;

namespace DigitalTransparencySystem.Helpers
{
    public static class RestrictionService
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
                    IF COL_LENGTH('dbo.Tasks', 'IsRestricted') IS NULL
                        ALTER TABLE dbo.Tasks ADD IsRestricted BIT NOT NULL CONSTRAINT DF_Tasks_IsRestricted DEFAULT 0;
                    IF COL_LENGTH('dbo.Decisions', 'IsRestricted') IS NULL
                        ALTER TABLE dbo.Decisions ADD IsRestricted BIT NOT NULL CONSTRAINT DF_Decisions_IsRestricted DEFAULT 0;
                    IF COL_LENGTH('dbo.Clubs', 'IsRestricted') IS NULL
                        ALTER TABLE dbo.Clubs ADD IsRestricted BIT NOT NULL CONSTRAINT DF_Clubs_IsRestricted DEFAULT 0;
                    IF COL_LENGTH('dbo.Polls', 'IsRestricted') IS NULL
                        ALTER TABLE dbo.Polls ADD IsRestricted BIT NOT NULL CONSTRAINT DF_Polls_IsRestricted DEFAULT 0;
                ", con).ExecuteNonQuery();
            }
            schemaReady = true;
        }

        public static bool IsEventRestricted(int eventId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                "SELECT ISNULL(IsDisabled, 0) FROM Events WHERE EventID = @ID", con))
            {
                cmd.Parameters.AddWithValue("@ID", eventId);
                con.Open();
                object value = cmd.ExecuteScalar();
                return value != null && value != DBNull.Value && Convert.ToBoolean(value);
            }
        }

        public static bool IsTaskLocked(int taskId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT CASE
                      WHEN ISNULL(t.IsRestricted, 0) = 1 THEN 1
                      WHEN t.EventID IS NOT NULL AND ISNULL(e.IsDisabled, 0) = 1 THEN 1
                      ELSE 0
                  END
                  FROM Tasks t
                  LEFT JOIN Events e ON e.EventID = t.EventID
                  WHERE t.TaskID = @ID", con))
            {
                cmd.Parameters.AddWithValue("@ID", taskId);
                con.Open();
                object value = cmd.ExecuteScalar();
                return value != null && value != DBNull.Value && Convert.ToInt32(value) == 1;
            }
        }

        public static bool IsDecisionRestricted(int decisionId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT CASE
                      WHEN ISNULL(d.IsRestricted, 0) = 1 THEN 1
                      WHEN d.EventID IS NOT NULL AND ISNULL(e.IsDisabled, 0) = 1 THEN 1
                      ELSE 0
                  END
                  FROM Decisions d
                  LEFT JOIN Events e ON e.EventID = d.EventID
                  WHERE d.DecisionID = @ID", con))
            {
                cmd.Parameters.AddWithValue("@ID", decisionId);
                con.Open();
                object value = cmd.ExecuteScalar();
                return value != null && value != DBNull.Value && Convert.ToInt32(value) == 1;
            }
        }

        public static string SetEventRestricted(int eventId, int adminId, string role, bool restricted)
        {
            if (!RoleAccess.IsAdmin(role))
                return "Only the system administrator can restrict or restore an event.";

            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                "UPDATE Events SET IsDisabled = @Flag, UpdatedAt = GETDATE() WHERE EventID = @ID", con))
            {
                cmd.Parameters.AddWithValue("@Flag", restricted);
                cmd.Parameters.AddWithValue("@ID", eventId);
                con.Open();
                if (cmd.ExecuteNonQuery() == 0)
                    return "Event not found.";
            }

            AuthService.WriteAudit(adminId, restricted ? "EventRestricted" : "EventUnrestricted", "Event", eventId, null, null);
            return null;
        }

        public static string SetTaskRestricted(int taskId, int adminId, string role, bool restricted)
        {
            if (!RoleAccess.IsAdmin(role))
                return "Only the system administrator can restrict or restore a task.";

            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                "UPDATE Tasks SET IsRestricted = @Flag, UpdatedAt = GETDATE() WHERE TaskID = @ID", con))
            {
                cmd.Parameters.AddWithValue("@Flag", restricted);
                cmd.Parameters.AddWithValue("@ID", taskId);
                con.Open();
                if (cmd.ExecuteNonQuery() == 0)
                    return "Task not found.";
            }

            AuthService.WriteAudit(adminId, restricted ? "TaskRestricted" : "TaskUnrestricted", "Task", taskId, null, null);
            return null;
        }

        public static string SetDecisionRestricted(int decisionId, int adminId, string role, bool restricted)
        {
            if (!RoleAccess.IsAdmin(role))
                return "Only the system administrator can restrict or restore a decision.";

            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                "UPDATE Decisions SET IsRestricted = @Flag, UpdatedAt = GETDATE() WHERE DecisionID = @ID", con))
            {
                cmd.Parameters.AddWithValue("@Flag", restricted);
                cmd.Parameters.AddWithValue("@ID", decisionId);
                con.Open();
                if (cmd.ExecuteNonQuery() == 0)
                    return "Decision not found.";
            }

            AuthService.WriteAudit(adminId, restricted ? "DecisionRestricted" : "DecisionUnrestricted", "Decision", decisionId, null, null);
            return null;
        }

        public static bool IsClubRestricted(int clubId)
        {
            EnsureSchema();
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                "SELECT ISNULL(IsRestricted, 0) FROM Clubs WHERE ClubID = @ID", con))
            {
                cmd.Parameters.AddWithValue("@ID", clubId);
                con.Open();
                object value = cmd.ExecuteScalar();
                return value != null && value != DBNull.Value && Convert.ToBoolean(value);
            }
        }

        public static string SetClubRestricted(int clubId, int adminId, string role, bool restricted)
        {
            EnsureSchema();
            if (!RoleAccess.IsAdmin(role))
                return "Only the system administrator can restrict or restore a club.";

            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                "UPDATE Clubs SET IsRestricted = @Flag WHERE ClubID = @ID AND ISNULL(IsDeleted, 0) = 0", con))
            {
                cmd.Parameters.AddWithValue("@Flag", restricted);
                cmd.Parameters.AddWithValue("@ID", clubId);
                con.Open();
                if (cmd.ExecuteNonQuery() == 0)
                    return "Club not found.";
            }

            AuthService.WriteAudit(adminId, restricted ? "ClubRestricted" : "ClubUnrestricted", "Club", clubId, null, null);

            ClubRecord club = ClubService.GetClub(clubId);
            if (club != null && club.LeadUserID.HasValue)
            {
                NotificationService.Send(
                    club.LeadUserID.Value,
                    restricted ? "Club restricted" : "Club restored",
                    restricted
                        ? "\"" + club.ClubName + "\" was restricted after an administrator review. Club activity is paused until it is restored."
                        : "\"" + club.ClubName + "\" is active again. Members can resume club activity.",
                    "Club",
                    clubId,
                    "Club");
            }

            return null;
        }

        public static bool CanEditEvent(int eventId, int userId, string role)
        {
            if (RoleAccess.IsAdmin(role) || IsEventRestricted(eventId))
                return false;

            EventAccess access = EventService.GetAccess(eventId, userId, role);
            if (access.Event == null)
                return false;
            if (access.Event.CreatedBy.HasValue && access.Event.CreatedBy.Value == userId)
                return true;
            return access.CanEditDetails;
        }

        public static bool CanEditTask(int taskId, int userId, string role)
        {
            if (RoleAccess.IsAdmin(role) || IsTaskLocked(taskId))
                return false;

            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                "SELECT LeaderID, EventID FROM Tasks WHERE TaskID = @ID", con))
            {
                cmd.Parameters.AddWithValue("@ID", taskId);
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return false;
                    if (reader["LeaderID"] != DBNull.Value && Convert.ToInt32(reader["LeaderID"]) == userId)
                        return true;
                    if (reader["EventID"] != DBNull.Value)
                    {
                        int eventId = Convert.ToInt32(reader["EventID"]);
                        reader.Close();
                        return CanEditEvent(eventId, userId, role);
                    }
                }
            }
            return false;
        }

        public static bool CanEditDecision(int decisionId, int userId, string role)
        {
            if (RoleAccess.IsAdmin(role) || IsDecisionRestricted(decisionId))
                return false;

            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                "SELECT CreatedBy, ResponsibleUserID, EventID FROM Decisions WHERE DecisionID = @ID", con))
            {
                cmd.Parameters.AddWithValue("@ID", decisionId);
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return false;
                    if (reader["CreatedBy"] != DBNull.Value && Convert.ToInt32(reader["CreatedBy"]) == userId)
                        return true;
                    if (reader["ResponsibleUserID"] != DBNull.Value && Convert.ToInt32(reader["ResponsibleUserID"]) == userId)
                        return true;
                    if (reader["EventID"] != DBNull.Value)
                    {
                        int eventId = Convert.ToInt32(reader["EventID"]);
                        reader.Close();
                        return CanEditEvent(eventId, userId, role);
                    }
                }
            }
            return false;
        }
    }
}
