using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web;

namespace DigitalTransparencySystem.Helpers
{
    public static class NotificationService
    {
        public static void Send(int userId, string title, string message, string type, int? relatedId, string relatedType)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"INSERT INTO Notifications (UserID, Title, Message, IsRead, NotificationType, RelatedID, RelatedType, CreatedAt)
                  VALUES (@UserID, @Title, @Message, 0, @Type, @RelatedID, @RelatedType, GETDATE())", con))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@Title", title ?? "");
                cmd.Parameters.AddWithValue("@Message", message ?? "");
                cmd.Parameters.AddWithValue("@Type", string.IsNullOrWhiteSpace(type) ? "General" : type);
                cmd.Parameters.AddWithValue("@RelatedID", (object)relatedId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@RelatedType", (object)relatedType ?? DBNull.Value);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static void NotifyUsers(IEnumerable<int> userIds, int? exceptUserId, string title, string message, string type, int? relatedId, string relatedType)
        {
            if (userIds == null)
                return;
            foreach (int userId in userIds)
            {
                if (exceptUserId.HasValue && userId == exceptUserId.Value)
                    continue;
                Send(userId, title, message, type, relatedId, relatedType);
            }
        }

        public static readonly string[] AssignmentTypes = { "Assignment", "AssignmentDeadline", "AssignmentOverdue" };

        public static int CountUnread(int userId)
        {
            return CountUnread(userId, null);
        }

        public static int CountUnread(int userId, IList<string> types)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand())
            {
                cmd.Connection = con;
                cmd.CommandText = "SELECT COUNT(*) FROM Notifications WHERE UserID = @UserID AND IsRead = 0";
                cmd.Parameters.AddWithValue("@UserID", userId);
                AppendTypeFilter(cmd, types);
                con.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public static string BadgeText(int count)
        {
            if (count <= 0)
                return "";
            return count > 9 ? "9+" : count.ToString();
        }

        public static DataTable ListRecent(int userId, int take)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT TOP (@Take) NotificationID, Title, Message, IsRead, NotificationType, RelatedID, RelatedType, CreatedAt
                  FROM Notifications
                  WHERE UserID = @UserID
                  ORDER BY CreatedAt DESC", con))
            {
                cmd.Parameters.AddWithValue("@Take", take);
                cmd.Parameters.AddWithValue("@UserID", userId);
                var table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                return table;
            }
        }

        public static DataTable ListFiltered(int userId, string filter)
        {
            string where = "";
            switch ((filter ?? "All").Trim())
            {
                case "Unread":
                    where = "AND IsRead = 0";
                    break;
                case "Task":
                    where = "AND NotificationType IN (N'Task', N'TaskAssignment')";
                    break;
                case "Assignment":
                    where = "AND NotificationType IN (N'Assignment', N'AssignmentDeadline', N'AssignmentOverdue')";
                    break;
                case "Event":
                    where = "AND NotificationType IN (N'Event', N'Meeting')";
                    break;
                case "Club":
                    where = "AND NotificationType IN (N'Club', N'Urgent', N'Announcement')";
                    break;
                case "Identity":
                    where = "AND NotificationType IN (N'Identity', N'Verification')";
                    break;
                case "Moderation":
                    where = "AND NotificationType IN (N'Moderation', N'Report')";
                    break;
                case "Decision":
                    where = "AND NotificationType = N'Decision'";
                    break;
            }

            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT NotificationID, Title, Message, IsRead, NotificationType, RelatedID, RelatedType, CreatedAt
                  FROM Notifications
                  WHERE UserID = @UserID " + where + @"
                  ORDER BY CreatedAt DESC", con))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
                var table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                table.Columns.Add("IconName", typeof(string));
                table.Columns.Add("TimeAgo", typeof(string));
                foreach (DataRow row in table.Rows)
                {
                    row["IconName"] = IconForType(Convert.ToString(row["NotificationType"]));
                    row["TimeAgo"] = TimeAgo(Convert.ToDateTime(row["CreatedAt"]));
                }
                return table;
            }
        }

        public static void MarkRead(int userId, int notificationId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                "UPDATE Notifications SET IsRead = 1 WHERE NotificationID = @ID AND UserID = @UserID", con))
            {
                cmd.Parameters.AddWithValue("@ID", notificationId);
                cmd.Parameters.AddWithValue("@UserID", userId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static void MarkAllRead(int userId)
        {
            MarkTypesRead(userId, null);
        }

        public static void MarkTypesRead(int userId, IList<string> types)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand())
            {
                cmd.Connection = con;
                cmd.CommandText = "UPDATE Notifications SET IsRead = 1 WHERE UserID = @UserID AND IsRead = 0";
                cmd.Parameters.AddWithValue("@UserID", userId);
                AppendTypeFilter(cmd, types);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private static void AppendTypeFilter(SqlCommand cmd, IList<string> types)
        {
            if (types == null || types.Count == 0)
                return;

            var names = new List<string>();
            for (int i = 0; i < types.Count; i++)
            {
                string name = "@Type" + i;
                names.Add(name);
                cmd.Parameters.AddWithValue(name, types[i]);
            }
            cmd.CommandText += " AND NotificationType IN (" + string.Join(", ", names.ToArray()) + ")";
        }

        public static void Delete(int userId, int notificationId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                "DELETE FROM Notifications WHERE NotificationID = @ID AND UserID = @UserID", con))
            {
                cmd.Parameters.AddWithValue("@ID", notificationId);
                cmd.Parameters.AddWithValue("@UserID", userId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static void DeleteAll(int userId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand("DELETE FROM Notifications WHERE UserID = @UserID", con))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static string IconForType(string type)
        {
            switch ((type ?? "").ToLowerInvariant())
            {
                case "task":
                case "taskassignment":
                    return "assignment";
                case "assignment":
                case "assignmentdeadline":
                case "assignmentoverdue":
                    return "school";
                case "meeting":
                    return "groups";
                case "decision":
                    return "gavel";
                case "event":
                    return "calendar_today";
                case "club":
                    return "forum";
                case "urgent":
                    return "priority_high";
                case "announcement":
                    return "campaign";
                case "identity":
                case "verification":
                    return "badge";
                case "moderation":
                    return "gpp_maybe";
                case "report":
                    return "flag";
                case "feedback":
                    return "feedback";
                default:
                    return "notifications";
            }
        }

        public static string TimeAgo(DateTime dateTime)
        {
            TimeSpan diff = DateTime.Now - dateTime;
            if (diff.TotalMinutes < 1) return "Just now";
            if (diff.TotalMinutes < 60) return ((int)diff.TotalMinutes) + "m ago";
            if (diff.TotalHours < 24) return ((int)diff.TotalHours) + "h ago";
            if (diff.TotalDays < 7) return ((int)diff.TotalDays) + "d ago";
            return dateTime.ToString("MMM dd");
        }

        public static string RenderDropdownHtml(int userId, HttpServerUtility server)
        {
            DataTable table = ListRecent(userId, 10);
            if (table.Rows.Count == 0)
                return "<div class='px-4 py-8 text-center text-on-surface-variant text-label-md'>No notifications yet</div>";

            var sb = new StringBuilder();
            foreach (DataRow row in table.Rows)
            {
                string title = Convert.ToString(row["Title"]);
                string message = Convert.ToString(row["Message"]) ?? "";
                bool isRead = Convert.ToBoolean(row["IsRead"]);
                string preview = message.Length > 80 ? message.Substring(0, 80) + "..." : message;
                sb.AppendFormat(
                    @"<div class='px-4 py-3 border-b border-surface-container-high hover:bg-surface-container-low transition-colors {0}'>
                        <div class='flex items-start gap-3'>
                            <span class='material-symbols-outlined text-[18px] text-primary mt-0.5'>{1}</span>
                            <div class='flex-1 min-w-0'>
                                <p class='font-label-md text-label-md text-on-surface {2}'>{3}</p>
                                <p class='text-body-md text-on-surface-variant mt-0.5 truncate'>{4}</p>
                                <p class='text-badge-cap text-outline mt-1'>{5}</p>
                            </div>
                        </div>
                    </div>",
                    isRead ? "" : "bg-primary-container/20",
                    IconForType(Convert.ToString(row["NotificationType"])),
                    isRead ? "" : "font-bold",
                    server.HtmlEncode(title),
                    server.HtmlEncode(preview),
                    TimeAgo(Convert.ToDateTime(row["CreatedAt"])));
            }
            return sb.ToString();
        }

        public static void EnsureAssignmentReminders(int userId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                var pending = new SqlCommand(
                    @"SELECT a.AssignmentID, a.AssignmentName, a.Deadline
                      FROM AssignmentMembers m
                      INNER JOIN AssignmentGroups g ON g.GroupID = m.GroupID AND g.IsDeleted = 0
                      INNER JOIN Assignments a ON a.AssignmentID = g.AssignmentID AND a.IsDeleted = 0
                      WHERE m.UserID = @UserID
                        AND a.Status = N'Active'
                        AND a.Deadline <= DATEADD(DAY, 3, GETDATE())
                        AND NOT EXISTS (
                            SELECT 1 FROM Notifications n
                            WHERE n.UserID = @UserID
                              AND n.RelatedID = a.AssignmentID
                              AND n.RelatedType = N'Assignment'
                              AND n.NotificationType IN (N'AssignmentDeadline', N'AssignmentOverdue')
                              AND n.CreatedAt >= DATEADD(DAY, -7, GETDATE())
                        )", con);
                pending.Parameters.AddWithValue("@UserID", userId);
                var rows = new List<Tuple<int, string, DateTime>>();
                using (SqlDataReader reader = pending.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        rows.Add(Tuple.Create(
                            Convert.ToInt32(reader["AssignmentID"]),
                            Convert.ToString(reader["AssignmentName"]),
                            Convert.ToDateTime(reader["Deadline"])));
                    }
                }

                foreach (var row in rows)
                {
                    bool overdue = row.Item3.Date < DateTime.Today;
                    Send(userId,
                        overdue ? "Assignment overdue" : "Assignment deadline soon",
                        overdue
                            ? row.Item2 + " is past its deadline (" + row.Item3.ToString("MMM dd, yyyy") + ")."
                            : row.Item2 + " is due " + row.Item3.ToString("MMM dd, yyyy") + ".",
                        overdue ? "AssignmentOverdue" : "AssignmentDeadline",
                        row.Item1,
                        "Assignment");
                }
            }
        }

        public static DataTable GetAdminStats()
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand("dbo.sp_GetAdminDashboardStats", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                var table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                return table;
            }
        }

        public static DataTable ListAuditLogs(int take, int? userId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT TOP (@Take) a.LogID, a.Action, a.EntityType, a.EntityID, a.Description, a.Timestamp, a.IPAddress,
                         ISNULL(u.FullName, 'System') AS FullName
                  FROM AuditLogs a
                  LEFT JOIN Users u ON u.UserID = a.UserID
                  WHERE (@UserID IS NULL OR a.UserID = @UserID)
                  ORDER BY a.Timestamp DESC", con))
            {
                cmd.Parameters.AddWithValue("@Take", take);
                cmd.Parameters.AddWithValue("@UserID", (object)userId ?? DBNull.Value);
                var table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                return table;
            }
        }
    }
}
