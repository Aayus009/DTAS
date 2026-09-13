using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace DigitalTransparencySystem.Helpers
{
    public static class TimelineService
    {
        private static readonly object RefreshLock = new object();
        private static DateTime lastCoreUtc = DateTime.MinValue;
        private static DateTime lastMailUtc = DateTime.MinValue;

        public static DateTime? AsDate(object value)
        {
            if (value == null || value == DBNull.Value)
                return null;
            DateTime due;
            if (value is DateTime)
                return ((DateTime)value).Date;
            return DateTime.TryParse(Convert.ToString(value), out due) ? due.Date : (DateTime?)null;
        }

        public static bool IsClosed(object status)
        {
            string value = Convert.ToString(status) ?? "";
            return value.Equals("Completed", StringComparison.OrdinalIgnoreCase)
                || value.Equals("Cancelled", StringComparison.OrdinalIgnoreCase)
                || value.Equals("Archived", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsOverdue(object dueDate, object status)
        {
            if (IsClosed(status))
                return false;
            DateTime? due = AsDate(dueDate);
            return due.HasValue && due.Value < DateTime.Today;
        }

        public static bool IsDueToday(object dueDate, object status)
        {
            if (IsClosed(status))
                return false;
            DateTime? due = AsDate(dueDate);
            return due.HasValue && due.Value == DateTime.Today;
        }

        public static string DueLabel(object dueDate, object status = null)
        {
            DateTime? due = AsDate(dueDate);
            if (!due.HasValue)
                return "No due date";

            string dateText = due.Value.ToString("MMM dd, yyyy");
            if (IsClosed(status))
                return dateText;

            int days = (due.Value - DateTime.Today).Days;
            if (days < 0)
            {
                int late = Math.Abs(days);
                return late == 1 ? "Late by 1 day · " + dateText : "Late by " + late + " days · " + dateText;
            }
            if (days == 0)
                return "Due today · " + dateText;
            if (days == 1)
                return "Due tomorrow · " + dateText;
            return "Due in " + days + " days · " + dateText;
        }

        public static string ShortDueLabel(object dueDate, object status = null)
        {
            DateTime? due = AsDate(dueDate);
            if (!due.HasValue)
                return "No due date";
            if (IsClosed(status))
                return due.Value.ToString("MMM dd, yyyy");

            int days = (due.Value - DateTime.Today).Days;
            if (days < 0)
            {
                int late = Math.Abs(days);
                return late == 1 ? "Late by 1 day" : "Late by " + late + " days";
            }
            if (days == 0)
                return "Due today";
            if (days == 1)
                return "Due tomorrow";
            return "Due in " + days + " days";
        }

        public static string StatusLabel(object status, object dueDate)
        {
            if (IsOverdue(dueDate, status))
                return "Late";
            string value = Convert.ToString(status) ?? "";
            if (value.Equals("Delayed", StringComparison.OrdinalIgnoreCase))
                return "Late";
            if (value.Equals("InProgress", StringComparison.OrdinalIgnoreCase) || value.Equals("In Progress", StringComparison.OrdinalIgnoreCase))
                return "In Progress";
            if (value.Equals("NotStarted", StringComparison.OrdinalIgnoreCase) || value.Equals("Not Started", StringComparison.OrdinalIgnoreCase))
                return "Not Started";
            if (value.Equals("UnderReview", StringComparison.OrdinalIgnoreCase) || value.Equals("Under Review", StringComparison.OrdinalIgnoreCase)
                || value.Equals("Submitted", StringComparison.OrdinalIgnoreCase))
                return "Under Review";
            if (value.Equals("RevisionNeeded", StringComparison.OrdinalIgnoreCase)
                || value.Equals("ChangesRequested", StringComparison.OrdinalIgnoreCase))
                return "Changes requested";
            if (value.Equals("ToDo", StringComparison.OrdinalIgnoreCase))
                return "To Do";
            return string.IsNullOrWhiteSpace(value) ? "Pending" : value;
        }

        public static string StatusBadgeClass(object status, object dueDate)
        {
            if (IsOverdue(dueDate, status))
                return "badge-status-late";
            if (IsDueToday(dueDate, status))
                return "badge-status-duetoday";
            string value = (Convert.ToString(status) ?? "").ToLowerInvariant().Replace(" ", "");
            if (value == "delayed")
                return "badge-status-late";
            return "badge-status-" + value;
        }

        public static string DueClass(object dueDate, object status)
        {
            if (IsOverdue(dueDate, status))
                return "text-[#c62828] font-semibold";
            if (IsDueToday(dueDate, status))
                return "text-[#e67e00] font-semibold";
            return "text-on-surface-variant";
        }

        public static void RefreshOverdueStatuses()
        {
            bool runCore;
            bool runMail;
            lock (RefreshLock)
            {
                DateTime now = DateTime.UtcNow;
                runCore = (now - lastCoreUtc).TotalSeconds >= 90;
                runMail = (now - lastMailUtc).TotalMinutes >= 10;
                if (runCore)
                    lastCoreUtc = now;
                if (runMail)
                    lastMailUtc = now;
            }

            try
            {
                if (runCore)
                {
                    MarkInstitutionTasksLate();
                    DeadlineService.RefreshAssignmentDeadlines();
                    MeetingService.CloseExpiredMeetings(false);
                }
                if (runMail)
                    DeadlineService.SendNearDeadlineEmails();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("RefreshOverdueStatuses: " + ex.Message);
            }
        }

        private static void MarkInstitutionTasksLate()
        {
            var marked = new List<Tuple<int, string, int?>>();

            using (SqlConnection con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(
                    @"UPDATE Tasks
                      SET Status = 'Delayed', UpdatedAt = GETDATE()
                      OUTPUT inserted.TaskID, inserted.TaskTitle, inserted.LeaderID
                      WHERE DueDate IS NOT NULL
                        AND CAST(DueDate AS DATE) < CAST(GETDATE() AS DATE)
                        AND Status NOT IN ('Completed', 'Cancelled', 'Archived', 'Delayed', 'UnderReview')", con))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int? leaderId = reader["LeaderID"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["LeaderID"]);
                        marked.Add(Tuple.Create(Convert.ToInt32(reader["TaskID"]), Convert.ToString(reader["TaskTitle"]), leaderId));
                    }
                }
            }

            foreach (var item in marked)
                NotifyTaskLate(item.Item1, item.Item2, item.Item3);

            var eventIds = new HashSet<int>();
            foreach (var item in marked)
            {
                int? eventId = EventTaskService.GetEventId(item.Item1);
                if (eventId.HasValue)
                    eventIds.Add(eventId.Value);
            }
            foreach (int eventId in eventIds)
                EventTaskService.SyncEventStatusFromTasks(eventId, 0);
        }

        private static void NotifyTaskLate(int taskId, string title, int? leaderId)
        {
            var ids = new HashSet<int>();
            if (leaderId.HasValue && leaderId.Value > 0)
                ids.Add(leaderId.Value);

            using (SqlConnection con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT UserID FROM TaskAssignments WHERE TaskID = @TaskID
                      UNION
                      SELECT ttm.UserID
                      FROM TaskTeamMembers ttm
                      INNER JOIN TaskTeams tt ON tt.TeamID = ttm.TeamID
                      WHERE tt.TaskID = @TaskID AND ttm.Status = 'Accepted'", con))
                {
                    cmd.Parameters.AddWithValue("@TaskID", taskId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            ids.Add(Convert.ToInt32(reader["UserID"]));
                    }
                }
            }

            string message = "\"" + title + "\" is past its due date and is now marked Late.";
            NotificationService.NotifyUsers(ids, null, "Task is late", message, "Task", taskId, "Task");
        }
    }
}
