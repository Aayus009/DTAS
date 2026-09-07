using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web;

namespace DigitalTransparencySystem.Helpers
{
    public class SearchHit
    {
        public string Type { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Url { get; set; }
        public string Icon { get; set; }
    }

    public static class SearchService
    {
        public static List<SearchHit> Search(int userId, string role, string query)
        {
            var hits = new List<SearchHit>();
            query = (query ?? "").Trim();
            if (query.Length < 2)
                return hits;

            bool isAdmin = RoleAccess.IsAdmin(role);
            string like = "%" + EscapeLike(query) + "%";

            using (var con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                TryAdd(delegate { AddEvents(con, hits, userId, isAdmin, like); });
                TryAdd(delegate { AddTasks(con, hits, userId, isAdmin, like); });
                TryAdd(delegate { AddMeetings(con, hits, userId, isAdmin, like); });
            }

            return hits;
        }

        private static void TryAdd(Action add)
        {
            try
            {
                add();
            }
            catch
            {
            }
        }

        private static void AddEvents(SqlConnection con, List<SearchHit> hits, int userId, bool isAdmin, string like)
        {
            string sql = @"
                SELECT TOP 6 e.EventID, e.EventName, e.Status, ISNULL(e.Visibility, N'Private') AS Visibility
                FROM Events e
                WHERE ISNULL(e.IsDeleted, 0) = 0
                  AND (e.EventName LIKE @Q OR ISNULL(e.Description, N'') LIKE @Q)
                  AND (
                        @IsAdmin = 1
                        OR ISNULL(e.Visibility, N'Private') = N'Public'
                        OR EXISTS (
                            SELECT 1 FROM EventMembers em
                            WHERE em.EventID = e.EventID AND em.UserID = @UserID
                              AND em.IsActive = 1 AND em.InviteStatus = N'Accepted')
                  )
                ORDER BY e.EventName";

            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@Q", like);
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@IsAdmin", isAdmin ? 1 : 0);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = Convert.ToInt32(reader["EventID"]);
                        hits.Add(new SearchHit
                        {
                            Type = "Event",
                            Title = Convert.ToString(reader["EventName"]),
                            Subtitle = Convert.ToString(reader["Visibility"]) + " · " + Convert.ToString(reader["Status"]),
                            Icon = "event",
                            Url = ToUrl("~/Modules/Events/EventWorkspace.aspx?EventID=" + id)
                        });
                    }
                }
            }
        }

        private static void AddTasks(SqlConnection con, List<SearchHit> hits, int userId, bool isAdmin, string like)
        {
            string access = isAdmin ? "1 = 1" : TaskAccess.UserCanSeeTask;
            string sql = @"
                SELECT TOP 6 t.TaskID, t.TaskTitle, t.Status, ISNULL(e.EventName, N'') AS EventName
                FROM Tasks t
                LEFT JOIN Events e ON e.EventID = t.EventID
                WHERE ISNULL(t.IsDeleted, 0) = 0
                  AND t.Status <> N'Archived'
                  AND (t.TaskTitle LIKE @Q OR ISNULL(t.Description, N'') LIKE @Q)
                  AND (" + access + @")
                ORDER BY t.DueDate DESC, t.TaskID DESC";

            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@Q", like);
                cmd.Parameters.AddWithValue("@UserID", userId);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = Convert.ToInt32(reader["TaskID"]);
                        string eventName = Convert.ToString(reader["EventName"]);
                        string url = isAdmin
                            ? "~/Modules/Tasks/TaskDetails.aspx?TaskID=" + id
                            : "~/Modules/TaskWorkspaces/TaskWorkspace.aspx?TaskID=" + id;
                        hits.Add(new SearchHit
                        {
                            Type = "Task",
                            Title = Convert.ToString(reader["TaskTitle"]),
                            Subtitle = string.IsNullOrWhiteSpace(eventName)
                                ? Convert.ToString(reader["Status"])
                                : eventName + " · " + Convert.ToString(reader["Status"]),
                            Icon = "task_alt",
                            Url = ToUrl(url)
                        });
                    }
                }
            }
        }

        private static void AddMeetings(SqlConnection con, List<SearchHit> hits, int userId, bool isAdmin, string like)
        {
            MeetingService.EnsureSchema();
            string access = isAdmin
                ? "1 = 1"
                : @"m.CreatedBy = @UserID
                   OR EXISTS (SELECT 1 FROM MeetingParticipants mp WHERE mp.MeetingID = m.MeetingID AND mp.UserID = @UserID)
                   OR (m.EventID IS NOT NULL AND EXISTS (
                        SELECT 1 FROM EventMembers em
                        WHERE em.EventID = m.EventID AND em.UserID = @UserID
                          AND em.IsActive = 1 AND em.InviteStatus = N'Accepted'))";

            string sql = @"
                SELECT TOP 6 m.MeetingID, m.MeetingTitle, m.ScheduledDate, m.Status, ISNULL(e.EventName, N'') AS EventName
                FROM Meetings m
                LEFT JOIN Events e ON e.EventID = m.EventID
                WHERE (m.MeetingTitle LIKE @Q OR ISNULL(m.Description, N'') LIKE @Q OR ISNULL(e.EventName, N'') LIKE @Q)
                  AND (" + access + @")
                ORDER BY m.ScheduledDate DESC";

            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@Q", like);
                cmd.Parameters.AddWithValue("@UserID", userId);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = Convert.ToInt32(reader["MeetingID"]);
                        string eventName = Convert.ToString(reader["EventName"]);
                        string when = reader["ScheduledDate"] == DBNull.Value
                            ? Convert.ToString(reader["Status"])
                            : Convert.ToDateTime(reader["ScheduledDate"]).ToString("MMM dd, yyyy hh:mm tt");
                        string url = isAdmin
                            ? "~/Modules/Meetings/MeetingDetails.aspx?MeetingID=" + id
                            : "~/Modules/UserMeetings/UserMeetingDetails.aspx?MeetingID=" + id;
                        hits.Add(new SearchHit
                        {
                            Type = "Meeting",
                            Title = Convert.ToString(reader["MeetingTitle"]),
                            Subtitle = string.IsNullOrWhiteSpace(eventName) ? when : eventName + " · " + when,
                            Icon = "videocam",
                            Url = ToUrl(url)
                        });
                    }
                }
            }
        }

        private static string ToUrl(string virtualPath)
        {
            return VirtualPathUtility.ToAbsolute(virtualPath);
        }

        private static string EscapeLike(string value)
        {
            return value
                .Replace("[", "[[]")
                .Replace("%", "[%]")
                .Replace("_", "[_]");
        }
    }
}
