using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace DigitalTransparencySystem.Helpers
{
    public class DeadlineRisk
    {
        public string Severity { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public int DaysRemaining { get; set; }
        public int ProgressPercent { get; set; }
        public int ExpectedPercent { get; set; }
        public bool ShowNearDueAlert { get; set; }
        public bool ShowRecoveryActions { get; set; }
        public string BadgeClass { get; set; }
    }

    public static class DeadlineService
    {
        public static DeadlineRisk Evaluate(DateTime? createdAt, DateTime? dueDate, int progressPercent, string status)
        {
            var risk = new DeadlineRisk
            {
                Severity = "OnTrack",
                Title = "",
                Message = "",
                ProgressPercent = progressPercent < 0 ? 0 : (progressPercent > 100 ? 100 : progressPercent),
                ExpectedPercent = 0,
                BadgeClass = "bg-surface-container-low text-on-surface-variant"
            };

            if (!dueDate.HasValue)
                return risk;

            DateTime due = dueDate.Value.Date;
            DateTime start = createdAt.HasValue && createdAt.Value.Date < due
                ? createdAt.Value.Date
                : due.AddDays(-14);
            int totalDays = Math.Max(1, (due - start).Days);
            int elapsed = (DateTime.Today - start).Days;
            if (elapsed < 0) elapsed = 0;
            if (elapsed > totalDays) elapsed = totalDays;
            risk.ExpectedPercent = (int)Math.Round(elapsed * 100.0 / totalDays);
            risk.DaysRemaining = (due - DateTime.Today).Days;

            bool closed = TimelineService.IsClosed(status);
            if (closed)
            {
                risk.Severity = "OnTrack";
                return risk;
            }

            int gap = risk.ExpectedPercent - risk.ProgressPercent;
            bool nearWeek = risk.DaysRemaining <= 7;
            risk.ShowNearDueAlert = nearWeek;
            risk.ShowRecoveryActions = nearWeek;

            if (risk.DaysRemaining < 0)
            {
                risk.Severity = "Critical";
                risk.Title = "Past due";
                risk.Message = "This work is late. Completion is " + risk.ProgressPercent +
                    "% against an expected " + risk.ExpectedPercent + "%.";
                risk.BadgeClass = "bg-[rgba(198,40,40,0.12)] text-[#c62828]";
                return risk;
            }

            if (!nearWeek)
            {
                if (gap >= 25)
                {
                    risk.Severity = "Watch";
                    risk.Title = "Behind schedule";
                    risk.Message = "Completion is " + risk.ProgressPercent +
                        "%; expected " + risk.ExpectedPercent + "% by now.";
                    risk.BadgeClass = "bg-[rgba(230,126,0,0.12)] text-[#b86b00]";
                }
                return risk;
            }

            if (gap <= 0)
            {
                risk.Severity = "Watch";
                risk.Title = "Due date is near";
                risk.Message = "Due in " + DaysText(risk.DaysRemaining) +
                    ". Completion is on pace at " + risk.ProgressPercent + "%.";
                risk.BadgeClass = "bg-[rgba(230,126,0,0.12)] text-[#b86b00]";
            }
            else if (gap < 20)
            {
                risk.Severity = "High";
                risk.Title = "Due date is near";
                risk.Message = "Due in " + DaysText(risk.DaysRemaining) +
                    ". Completion is " + risk.ProgressPercent +
                    "%; expected " + risk.ExpectedPercent + "%. Consider extending or adding members.";
                risk.BadgeClass = "bg-[rgba(198,40,40,0.10)] text-[#c62828]";
            }
            else
            {
                risk.Severity = "Critical";
                risk.Title = "Due date is near";
                risk.Message = "Due in " + DaysText(risk.DaysRemaining) +
                    ". Completion is " + risk.ProgressPercent +
                    "% against " + risk.ExpectedPercent +
                    "% expected. Extend the due date or redistribute the work.";
                risk.BadgeClass = "bg-[rgba(198,40,40,0.14)] text-[#c62828]";
            }

            return risk;
        }

        public static DeadlineRisk GetTaskRisk(int taskId)
        {
            using (SqlConnection con = new SqlConnection(AuthService.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(
                @"SELECT t.CreatedAt, t.DueDate, t.Status,
                         ISNULL(tt.ProgressPercent,
                            CASE WHEN t.Status = 'Completed' THEN 100 ELSE 0 END) AS ProgressPercent
                  FROM Tasks t
                  LEFT JOIN TaskTeams tt ON tt.TaskID = t.TaskID
                  WHERE t.TaskID = @TaskID", con))
            {
                cmd.Parameters.AddWithValue("@TaskID", taskId);
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return Evaluate(null, null, 0, null);
                    DateTime? created = reader["CreatedAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["CreatedAt"]);
                    DateTime? due = reader["DueDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DueDate"]);
                    return Evaluate(created, due, Convert.ToInt32(reader["ProgressPercent"]), Convert.ToString(reader["Status"]));
                }
            }
        }

        public static bool CanManageTaskDeadline(int taskId, int userId, string role)
        {
            if (RoleAccess.IsAdmin(role) || RoleAccess.IsFacultyOrStaff(role))
                return true;
            using (SqlConnection con = new SqlConnection(AuthService.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(
                @"SELECT COUNT(1) FROM Tasks t
                  LEFT JOIN TaskTeams tt ON tt.TaskID = t.TaskID
                  WHERE t.TaskID = @TaskID
                    AND (t.LeaderID = @UserID OR tt.LeaderID = @UserID)", con))
            {
                cmd.Parameters.AddWithValue("@TaskID", taskId);
                cmd.Parameters.AddWithValue("@UserID", userId);
                con.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        public static string ExtendTaskDueDate(int taskId, int actorId, string role, DateTime newDue)
        {
            if (!CanManageTaskDeadline(taskId, actorId, role))
                return "Only the leader, faculty, staff, or admin can extend this due date.";
            if (newDue.Date <= DateTime.Today)
                return "Choose a due date after today.";

            string title;
            using (SqlConnection con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(
                    @"UPDATE Tasks
                      SET DueDate = @Due,
                          Status = CASE WHEN Status = 'Delayed' THEN 'InProgress' ELSE Status END,
                          UpdatedAt = GETDATE()
                      WHERE TaskID = @TaskID;
                      SELECT TaskTitle FROM Tasks WHERE TaskID = @TaskID;", con))
                {
                    cmd.Parameters.AddWithValue("@Due", newDue);
                    cmd.Parameters.AddWithValue("@TaskID", taskId);
                    title = Convert.ToString(cmd.ExecuteScalar());
                }
            }

            var ids = new HashSet<int>();
            using (SqlConnection con = new SqlConnection(AuthService.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(
                @"SELECT LeaderID FROM Tasks WHERE TaskID = @TaskID AND LeaderID IS NOT NULL
                  UNION
                  SELECT LeaderID FROM TaskTeams WHERE TaskID = @TaskID AND LeaderID IS NOT NULL
                  UNION
                  SELECT UserID FROM TaskAssignments WHERE TaskID = @TaskID
                  UNION
                  SELECT ttm.UserID
                  FROM TaskTeamMembers ttm
                  INNER JOIN TaskTeams tt ON tt.TeamID = ttm.TeamID
                  WHERE tt.TaskID = @TaskID AND ttm.Status = 'Accepted'", con))
            {
                cmd.Parameters.AddWithValue("@TaskID", taskId);
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        ids.Add(Convert.ToInt32(reader[0]));
                }
            }

            NotificationService.NotifyUsers(ids, actorId, "Due date extended",
                "\"" + title + "\" is now due " + newDue.ToString("MMM dd, yyyy") + ".",
                "Task", taskId, "Task");
            AuthService.WriteAudit(actorId, "TaskDueExtended", "Task", taskId, null, newDue.ToString("yyyy-MM-dd"));
            return null;
        }

        public static void RefreshAssignmentDeadlines()
        {
            NotifyAssignmentNearDeadlines();
            AutoCloseOverdueAssignments();
        }

        public static void SendNearDeadlineEmails()
        {
            SendTaskDeadlineEmails();
            SendEventDeadlineEmails();
        }

        private static void SendTaskDeadlineEmails()
        {
            var pending = new List<DeadlineMail>();
            using (SqlConnection con = new SqlConnection(AuthService.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(
                @"SELECT DISTINCT t.TaskID, t.TaskTitle, t.DueDate, u.UserID, u.FullName, u.Email
                  FROM Tasks t
                  INNER JOIN (
                      SELECT TaskID, LeaderID AS UserID FROM Tasks WHERE LeaderID IS NOT NULL
                      UNION
                      SELECT TaskID, LeaderID FROM TaskTeams WHERE LeaderID IS NOT NULL
                      UNION
                      SELECT TaskID, UserID FROM TaskAssignments
                      UNION
                      SELECT tt.TaskID, ttm.UserID
                      FROM TaskTeams tt
                      INNER JOIN TaskTeamMembers ttm ON ttm.TeamID = tt.TeamID
                      WHERE ttm.Status = 'Accepted'
                  ) r ON r.TaskID = t.TaskID
                  INNER JOIN Users u ON u.UserID = r.UserID
                  WHERE t.DueDate IS NOT NULL
                    AND t.Status NOT IN ('Completed', 'Cancelled', 'Archived')
                    AND DATEDIFF(DAY, CAST(GETDATE() AS DATE), CAST(t.DueDate AS DATE)) IN (4, 5)
                    AND ISNULL(u.Email, N'') <> N''
                    AND ISNULL(u.IsActive, 1) = 1
                    AND ISNULL(u.IsDeleted, 0) = 0
                    AND NOT EXISTS (
                        SELECT 1 FROM Notifications n
                        WHERE n.UserID = u.UserID
                          AND n.RelatedID = t.TaskID
                          AND n.RelatedType = N'Task'
                          AND n.NotificationType = N'TaskDeadlineEmail'
                          AND n.CreatedAt >= DATEADD(DAY, -7, GETDATE())
                    )", con))
            {
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        pending.Add(new DeadlineMail
                        {
                            RelatedId = Convert.ToInt32(reader["TaskID"]),
                            Title = Convert.ToString(reader["TaskTitle"]),
                            Due = Convert.ToDateTime(reader["DueDate"]),
                            UserId = Convert.ToInt32(reader["UserID"]),
                            Name = Convert.ToString(reader["FullName"]),
                            Email = Convert.ToString(reader["Email"])
                        });
                    }
                }
            }

            foreach (DeadlineMail item in pending)
            {
                int days = (item.Due.Date - DateTime.Today).Days;
                string dayLabel = days == 1 ? "1 day" : days + " days";
                string subject = "Task due in " + dayLabel + ": " + item.Title;
                MailContent mail = MailComposer.Build(
                    null,
                    "\"" + item.Title + "\" is due in " + dayLabel + " (" + item.Due.ToString("MMM dd, yyyy") + ").",
                    null,
                    null,
                    "Open the task",
                    MailSender.AbsoluteUrl("~/Modules/TaskWorkspaces/TaskWorkspace.aspx?TaskID=" + item.RelatedId),
                    MailComposer.FirstName(item.Name));
                DispatchDeadlineMail(item, subject, mail, "Task deadline warning", "TaskDeadlineEmail", "Task");
            }
        }

        private static void SendEventDeadlineEmails()
        {
            var pending = new List<DeadlineMail>();
            using (SqlConnection con = new SqlConnection(AuthService.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(
                @"SELECT DISTINCT e.EventID, e.EventName, ISNULL(e.EndDate, e.StartDate) AS Deadline,
                         u.UserID, u.FullName, u.Email
                  FROM Events e
                  INNER JOIN (
                      SELECT EventID, UserID
                      FROM EventMembers
                      WHERE IsActive = 1 AND InviteStatus = N'Accepted'
                      UNION
                      SELECT EventID, ProposedBy FROM Events WHERE ProposedBy IS NOT NULL
                      UNION
                      SELECT EventID, CreatedBy FROM Events WHERE CreatedBy IS NOT NULL
                  ) r ON r.EventID = e.EventID
                  INNER JOIN Users u ON u.UserID = r.UserID
                  WHERE ISNULL(e.IsDeleted, 0) = 0
                    AND e.Status NOT IN ('Cancelled', 'Completed', 'Rejected', 'Archived')
                    AND ISNULL(e.EndDate, e.StartDate) IS NOT NULL
                    AND DATEDIFF(DAY, CAST(GETDATE() AS DATE), CAST(ISNULL(e.EndDate, e.StartDate) AS DATE)) IN (4, 5)
                    AND ISNULL(u.Email, N'') <> N''
                    AND ISNULL(u.IsActive, 1) = 1
                    AND ISNULL(u.IsDeleted, 0) = 0
                    AND NOT EXISTS (
                        SELECT 1 FROM Notifications n
                        WHERE n.UserID = u.UserID
                          AND n.RelatedID = e.EventID
                          AND n.RelatedType = N'Event'
                          AND n.NotificationType = N'EventDeadlineEmail'
                          AND n.CreatedAt >= DATEADD(DAY, -7, GETDATE())
                    )", con))
            {
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        pending.Add(new DeadlineMail
                        {
                            RelatedId = Convert.ToInt32(reader["EventID"]),
                            Title = Convert.ToString(reader["EventName"]),
                            Due = Convert.ToDateTime(reader["Deadline"]),
                            UserId = Convert.ToInt32(reader["UserID"]),
                            Name = Convert.ToString(reader["FullName"]),
                            Email = Convert.ToString(reader["Email"])
                        });
                    }
                }
            }

            foreach (DeadlineMail item in pending)
            {
                int days = (item.Due.Date - DateTime.Today).Days;
                string dayLabel = days == 1 ? "1 day" : days + " days";
                string subject = "Event coming up in " + dayLabel + ": " + item.Title;
                MailContent mail = MailComposer.Build(
                    null,
                    item.Title + " is in " + dayLabel + " (" + item.Due.ToString("MMM dd, yyyy") + ").",
                    null,
                    null,
                    "Open the event",
                    MailSender.AbsoluteUrl("~/Modules/Events/EventWorkspace.aspx?EventID=" + item.RelatedId),
                    MailComposer.FirstName(item.Name));
                DispatchDeadlineMail(item, subject, mail, "Event deadline warning", "EventDeadlineEmail", "Event");
            }
        }

        private static void DispatchDeadlineMail(DeadlineMail item, string subject, MailContent mail, string inAppTitle, string notificationType, string relatedType)
        {
            if (!string.IsNullOrWhiteSpace(item.Email))
                MailSender.Send(item.Email.Trim(), subject, mail);
            NotificationService.Send(item.UserId, inAppTitle,
                item.Title + " is due " + item.Due.ToString("MMM dd, yyyy") + ".",
                notificationType, item.RelatedId, relatedType);
        }

        private class DeadlineMail
        {
            public int RelatedId { get; set; }
            public string Title { get; set; }
            public DateTime Due { get; set; }
            public int UserId { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
        }

        private static void NotifyAssignmentNearDeadlines()
        {
            var rows = new List<Tuple<int, string, DateTime>>();
            using (SqlConnection con = new SqlConnection(AuthService.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(
                @"SELECT AssignmentID, AssignmentName, Deadline
                  FROM Assignments
                  WHERE IsDeleted = 0 AND Status = N'Active'
                    AND CAST(Deadline AS DATE) >= CAST(GETDATE() AS DATE)
                    AND CAST(Deadline AS DATE) <= DATEADD(DAY, 3, CAST(GETDATE() AS DATE))", con))
            {
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        rows.Add(Tuple.Create(
                            Convert.ToInt32(reader["AssignmentID"]),
                            Convert.ToString(reader["AssignmentName"]),
                            Convert.ToDateTime(reader["Deadline"])));
                    }
                }
            }

            foreach (var row in rows)
                NotifyMembersOnce(row.Item1, row.Item2, row.Item3);
        }

        private static void NotifyMembersOnce(int assignmentId, string name, DateTime deadline)
        {
            using (SqlConnection con = new SqlConnection(AuthService.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(
                @"SELECT DISTINCT m.UserID
                  FROM AssignmentMembers m
                  INNER JOIN AssignmentGroups g ON g.GroupID = m.GroupID AND g.IsDeleted = 0
                  WHERE g.AssignmentID = @AssignmentID
                    AND NOT EXISTS (
                        SELECT 1 FROM Notifications n
                        WHERE n.UserID = m.UserID
                          AND n.RelatedID = @AssignmentID
                          AND n.RelatedType = N'Assignment'
                          AND n.NotificationType = N'AssignmentDeadline'
                          AND n.CreatedAt >= DATEADD(DAY, -3, GETDATE())
                    )", con))
            {
                cmd.Parameters.AddWithValue("@AssignmentID", assignmentId);
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        NotificationService.Send(
                            Convert.ToInt32(reader["UserID"]),
                            "Assignment deadline soon",
                            "\"" + name + "\" is due " + deadline.ToString("MMM dd, yyyy") + ". Complete remaining work before the deadline. Students cannot extend it.",
                            "AssignmentDeadline",
                            assignmentId,
                            "Assignment");
                    }
                }
            }
        }

        private static void AutoCloseOverdueAssignments()
        {
            var closed = new List<Tuple<int, string>>();
            using (SqlConnection con = new SqlConnection(AuthService.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(
                @"UPDATE Assignments
                  SET Status = N'Closed'
                  OUTPUT inserted.AssignmentID, inserted.AssignmentName
                  WHERE IsDeleted = 0
                    AND Status = N'Active'
                    AND CAST(Deadline AS DATE) < CAST(GETDATE() AS DATE)
                    AND EXISTS (
                        SELECT 1
                        FROM AssignmentGroups g
                        LEFT JOIN AssignmentTasks t ON t.GroupID = g.GroupID AND t.IsDeleted = 0
                        WHERE g.AssignmentID = Assignments.AssignmentID AND g.IsDeleted = 0
                          AND (t.TaskID IS NULL OR t.Status <> N'Completed')
                    )", con))
            {
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        closed.Add(Tuple.Create(Convert.ToInt32(reader["AssignmentID"]), Convert.ToString(reader["AssignmentName"])));
                }
            }

            foreach (var item in closed)
            {
                AssignmentService.NotifyAllMembers(
                    item.Item1,
                    "Assignment closed",
                    "\"" + item.Item2 + "\" closed because the deadline passed with unfinished work. Students cannot update it unless faculty extends the deadline.");
            }
        }

        private static string DaysText(int days)
        {
            if (days <= 0) return "today";
            if (days == 1) return "1 day";
            return days + " days";
        }
    }
}
