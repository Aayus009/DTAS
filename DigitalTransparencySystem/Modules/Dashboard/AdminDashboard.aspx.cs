using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Dashboard
{
    public partial class AdminDashboard : System.Web.UI.Page
    {
        private string connectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            connectionString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;

            if (!IsPostBack)
            {
                // Check if user is logged in
                if (Session["UserID"] == null)
                {
                    Response.Redirect("~/Modules/Authentication/Login.aspx");
                    return;
                }

                // Admin only
                string role = Session["Role"] as string;
                if (role == null || !role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                {
                    Response.Redirect("~/Modules/Authentication/Login.aspx");
                    return;
                }

                EventTaskService.PromoteEventsInProgressFromTasks();
                LoadDashboardData();
            }
        }

        private void LoadDashboardData()
        {
            LoadMetrics();
            LoadChartMetrics();
            LoadPendingTasks();
            LoadSystemHealth();
            LoadAuditTrail();
            LoadPendingProposals();
            BindLiveEventProgress();
            BindIdentityQueueBanner();
            BindFlagsQueueBanner();
            BindSystemStats();
            NotificationService.EnsureAssignmentReminders(Convert.ToInt32(Session["UserID"]));
            litLastUpdated.Text = DateTime.Now.ToString("MMM dd, yyyy - hh:mm tt");
        }

        private void BindIdentityQueueBanner()
        {
            int pending = IdentityDocumentService.CountPending();
            pnlIdentityQueue.Visible = pending > 0;
            lblIdentityQueue.Text = pending == 1
                ? "1 community member submitted an ID and is waiting for a decision."
                : pending + " community members submitted IDs and are waiting for a decision.";
        }

        private void BindSystemStats()
        {
            DataTable stats = NotificationService.GetAdminStats();
            if (stats.Rows.Count == 0)
                return;
            DataRow row = stats.Rows[0];
            litStatUsers.Text = Convert.ToString(row["TotalUsers"]);
            litStatPendingId.Text = Convert.ToString(row["PendingVerification"]);
            litStatAssignments.Text = Convert.ToString(row["ActiveAssignments"]);
            litStatGroups.Text = Convert.ToString(row["ActiveGroups"]);
            litStatFlags.Text = Convert.ToString(row["PendingReports"]);
            litStatSuspended.Text = Convert.ToString(row["SuspendedUsers"]);
        }

        private void BindFlagsQueueBanner()
        {
            int pending = ModerationService.CountPendingReports();
            pnlFlagsQueue.Visible = pending > 0;
            lblFlagsQueue.Text = pending == 1
                ? "1 community flag is waiting for a decision."
                : pending + " community flags are waiting for a decision.";
        }

        private void LoadPendingProposals()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
                    @"SELECT e.EventID, e.EventName, e.EventType, e.Description, e.StartDate, e.EndDate,
                             ISNULL(p.FullName, 'Unknown') AS ProposedByName
                      FROM Events e
                      LEFT JOIN Users p ON e.ProposedBy = p.UserID
                      WHERE e.Status = 'Proposed' AND ISNULL(e.IsDeleted, 0) = 0
                      ORDER BY e.CreatedAt", con);
                con.Open();

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    pnlProposals.Visible = true;
                    rptProposals.DataSource = dt;
                    rptProposals.DataBind();
                    litProposalCount.Text =
                        $"<span class='ml-2 px-2 py-0.5 bg-[rgba(230,126,0,0.12)] text-[#e67e00] rounded-full font-badge-cap text-badge-cap'>{dt.Rows.Count}</span>";
                }
                else
                {
                    pnlProposals.Visible = false;
                }
            }
        }

        protected void rptProposals_ItemCommand(object source, System.Web.UI.WebControls.RepeaterCommandEventArgs e)
        {
            int eventId;
            if (!int.TryParse(e.CommandArgument.ToString(), out eventId))
                return;

            switch (e.CommandName)
            {
                case "Approve":
                    ApproveProposal(eventId);
                    break;
                case "Reject":
                    RejectProposal(eventId, hfRejectReason.Value);
                    hfRejectReason.Value = "";
                    break;
            }

            LoadPendingProposals();
        }

        private void ApproveProposal(int eventId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    "UPDATE Events SET Status = 'Planned', RejectionReason = NULL, UpdatedAt = GETDATE() WHERE EventID = @EventID", con);
                cmd.Parameters.AddWithValue("@EventID", eventId);
                cmd.ExecuteNonQuery();

                NotifyProposer(con, eventId, "Event Proposal Approved", "Your event proposal has been approved and is now in the planning stage.");
            }

            EventService.AfterApproved(eventId, Convert.ToInt32(Session["UserID"]));
        }

        private void RejectProposal(int eventId, string reason)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    "UPDATE Events SET Status = 'Rejected', RejectionReason = @Reason, UpdatedAt = GETDATE() WHERE EventID = @EventID", con);
                cmd.Parameters.AddWithValue("@EventID", eventId);
                cmd.Parameters.AddWithValue("@Reason", (object)reason ?? DBNull.Value);
                cmd.ExecuteNonQuery();

                NotifyProposer(con, eventId, "Event Proposal Rejected", $"Your event proposal was not approved. Reason: {reason}");
            }
        }

        private void NotifyProposer(SqlConnection con, int eventId, string title, string message)
        {
            SqlCommand cmd = new SqlCommand(@"
                INSERT INTO Notifications (UserID, Title, Message, IsRead, NotificationType, RelatedID, RelatedType, CreatedAt)
                SELECT ProposedBy, @Title, @Message, 0, 'Event', @EventID, 'Event', GETDATE()
                FROM Events WHERE EventID = @EventID AND ProposedBy IS NOT NULL", con);
            cmd.Parameters.AddWithValue("@Title", title);
            cmd.Parameters.AddWithValue("@Message", message);
            cmd.Parameters.AddWithValue("@EventID", eventId);
            cmd.ExecuteNonQuery();
        }

        private void BindLiveEventProgress()
        {
            var rows = EventTaskService.ListLiveEventProgress();
            var table = new DataTable();
            table.Columns.Add("EventID", typeof(int));
            table.Columns.Add("EventName", typeof(string));
            table.Columns.Add("StatusLabel", typeof(string));
            table.Columns.Add("CompletionLabel", typeof(string));
            table.Columns.Add("Percent", typeof(int));
            table.Columns.Add("DetailsUrl", typeof(string));

            int taskTotal = 0;
            int taskDone = 0;
            foreach (EventLiveProgress row in rows)
            {
                if (!EventTaskService.CountsTowardInstitutionProgress(row.StatusKey))
                    continue;

                table.Rows.Add(
                    row.EventId,
                    row.EventName,
                    EventTaskService.EventStatusDisplay(row.Status),
                    EventTaskService.CompletionLabel(row.CompletedTasks, row.TotalTasks),
                    row.Percent,
                    ResolveUrl("~/Modules/Events/EventDetails.aspx?EventID=" + row.EventId));
                taskTotal += row.TotalTasks;
                taskDone += row.CompletedTasks;
            }

            rptLiveEventProgress.DataSource = table;
            rptLiveEventProgress.DataBind();
            rptLiveEventProgress.Visible = table.Rows.Count > 0;
            pnlNoLiveEvents.Visible = table.Rows.Count == 0;

            int pct = EventTaskService.PercentFromCounts(taskDone, taskTotal);
            string pctText = pct + "%";
            litTrustIndex.Text = pctText;
            litComplianceRate.Text = pctText;
            ApplyMeter(barAccountability, pctText);
            ApplyMeter(barTaskCompletion, pctText);
        }

        private void LoadMetrics()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                SqlCommand cmdDecisions = new SqlCommand(
                    @"SELECT COUNT(*) FROM Decisions
                      WHERE Status IN ('Proposed', 'Created', 'UnderReview', 'Under Review', 'Reviewed', 'Approved')", con);
                litActiveDecisions.Text = cmdDecisions.ExecuteScalar().ToString();

                int pendingProposals = Convert.ToInt32(new SqlCommand(
                    "SELECT COUNT(*) FROM Events WHERE Status = 'Proposed' AND ISNULL(IsDeleted, 0) = 0", con).ExecuteScalar());
                int pendingInvites = Convert.ToInt32(new SqlCommand(
                    "SELECT COUNT(*) FROM TaskTeamMembers WHERE Status = 'Invited'", con).ExecuteScalar());
                int openFeedback = Convert.ToInt32(new SqlCommand(
                    "SELECT COUNT(*) FROM Feedback WHERE Status IS NULL OR Status <> 'Resolved'", con).ExecuteScalar());
                int overdueTasks = Convert.ToInt32(new SqlCommand(
                    @"SELECT COUNT(*) FROM Tasks
                      WHERE Status NOT IN ('Completed', 'Cancelled', 'Archived')
                        AND DueDate IS NOT NULL AND DueDate < GETDATE()", con).ExecuteScalar());

                litRecordRequests.Text = (pendingProposals + pendingInvites + openFeedback + overdueTasks).ToString();
                litResponseTime.Text = pendingInvites.ToString();
                litFulfillmentRate.Text = openFeedback.ToString();

                int decisionPct = EventTaskService.InstitutionDecisionPercent();
                litAccessibilityRate.Text = decisionPct + "%";
                ApplyMeter(barDecisionProgress, decisionPct + "%");
            }
        }

        private static void ApplyMeter(HtmlGenericControl bar, string pctText)
        {
            if (bar == null)
                return;
            bar.Style["width"] = pctText;
            bar.Attributes["data-width"] = pctText;
            string css = bar.Attributes["class"] ?? "";
            if (css.IndexOf("js-progress-fill", StringComparison.Ordinal) < 0)
                bar.Attributes["class"] = (css + " js-progress-fill").Trim();
        }

        private void LoadPendingTasks()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
                    @"SELECT TOP 5 
                        t.TaskTitle,
                        t.Status,
                        t.DueDate
                      FROM Tasks t
                      WHERE t.Status NOT IN ('Completed', 'Cancelled', 'Archived')
                        AND t.DueDate IS NOT NULL
                      ORDER BY t.DueDate", con);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                rptPendingTasks.DataSource = dt;
                rptPendingTasks.DataBind();
            }
        }

        private void LoadSystemHealth()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("IconName", typeof(string));
            dt.Columns.Add("IconColor", typeof(string));
            dt.Columns.Add("IconBgColor", typeof(string));
            dt.Columns.Add("Title", typeof(string));
            dt.Columns.Add("Description", typeof(string));

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                int proposals = Convert.ToInt32(new SqlCommand(
                    "SELECT COUNT(*) FROM Events WHERE Status = 'Proposed' AND ISNULL(IsDeleted, 0) = 0", con).ExecuteScalar());
                int invites = Convert.ToInt32(new SqlCommand(
                    "SELECT COUNT(*) FROM TaskTeamMembers WHERE Status = 'Invited'", con).ExecuteScalar());
                int overdue = Convert.ToInt32(new SqlCommand(
                    @"SELECT COUNT(*) FROM Tasks
                      WHERE Status NOT IN ('Completed', 'Cancelled', 'Archived')
                        AND DueDate IS NOT NULL AND DueDate < GETDATE()", con).ExecuteScalar());

                dt.Rows.Add("event", "#00236f", "rgba(0,35,111,0.1)", "Event Proposals",
                    proposals == 0 ? "No event proposals waiting for review." : proposals + " proposal(s) waiting for Admin review.");
                dt.Rows.Add("mail", "#27a577", "rgba(39,165,119,0.1)", "Team Invitations",
                    invites == 0 ? "No pending task invitations." : invites + " team invitation(s) still unanswered.");
                dt.Rows.Add("schedule", "#56657c", "#d4e3ff", "Overdue Tasks",
                    overdue == 0 ? "No overdue tasks in the current cycle." : overdue + " task(s) are past their due date.");
            }

            rptSystemHealth.DataSource = dt;
            rptSystemHealth.DataBind();
        }

        private void LoadAuditTrail()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
                    @"SELECT TOP 5 
                        'Decision Published' AS Title,
                        dh.ChangedAt AS EventTime,
                        u.FullName AS Department,
                        d.DecisionTitle AS Description,
                        '#001142' AS DotColor
                      FROM DecisionHistory dh
                      INNER JOIN Decisions d ON dh.DecisionID = d.DecisionID
                      INNER JOIN Users u ON dh.ChangedBy = u.UserID
                      ORDER BY dh.ChangedAt DESC", con);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dt.Columns.Add("TimeAgo", typeof(string));

                foreach (DataRow row in dt.Rows)
                {
                    DateTime eventTime = Convert.ToDateTime(row["EventTime"]);
                    TimeSpan diff = DateTime.Now - eventTime;
                    if (diff.TotalMinutes < 60)
                        row["TimeAgo"] = $"{(int)diff.TotalMinutes} mins ago";
                    else if (diff.TotalHours < 24)
                        row["TimeAgo"] = $"{(int)diff.TotalHours} hours ago";
                    else
                        row["TimeAgo"] = $"{(int)diff.TotalDays} days ago";
                }

                rptAuditTrail.DataSource = dt;
                rptAuditTrail.DataBind();
            }
        }

        private void LoadChartMetrics()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                // Task completion stats
                SqlCommand cmdTasks = new SqlCommand(
                    @"SELECT 
                        SUM(CASE WHEN Status = 'Completed' THEN 1 ELSE 0 END) AS Completed,
                        SUM(CASE WHEN Status = 'InProgress' THEN 1 ELSE 0 END) AS InProgress,
                        SUM(CASE WHEN Status = 'Pending' THEN 1 ELSE 0 END) AS Pending,
                        SUM(CASE WHEN Status = 'Delayed' THEN 1 ELSE 0 END) AS Delayed
                      FROM Tasks", con);
                SqlDataReader taskReader = cmdTasks.ExecuteReader();
                if (taskReader.Read())
                {
                    litTasksCompleted.Text = (taskReader["Completed"] == DBNull.Value ? 0 : Convert.ToInt32(taskReader["Completed"])).ToString();
                    litTasksInProgress.Text = (taskReader["InProgress"] == DBNull.Value ? 0 : Convert.ToInt32(taskReader["InProgress"])).ToString();
                    litTasksPending.Text = (taskReader["Pending"] == DBNull.Value ? 0 : Convert.ToInt32(taskReader["Pending"])).ToString();
                    litTasksDelayed.Text = (taskReader["Delayed"] == DBNull.Value ? 0 : Convert.ToInt32(taskReader["Delayed"])).ToString();
                }
                taskReader.Close();

                // Decision status stats
                SqlCommand cmdDecisions = new SqlCommand(
                    @"SELECT 
                        SUM(CASE WHEN Status = 'Proposed' THEN 1 ELSE 0 END) AS Proposed,
                        SUM(CASE WHEN Status = 'UnderReview' THEN 1 ELSE 0 END) AS UnderReview,
                        SUM(CASE WHEN Status = 'Approved' THEN 1 ELSE 0 END) AS Approved,
                        SUM(CASE WHEN Status = 'Rejected' THEN 1 ELSE 0 END) AS Rejected,
                        SUM(CASE WHEN Status = 'Completed' THEN 1 ELSE 0 END) AS Completed
                      FROM Decisions", con);
                SqlDataReader decReader = cmdDecisions.ExecuteReader();
                if (decReader.Read())
                {
                    litDecisionsProposed.Text = (decReader["Proposed"] == DBNull.Value ? 0 : Convert.ToInt32(decReader["Proposed"])).ToString();
                    litDecisionsUnderReview.Text = (decReader["UnderReview"] == DBNull.Value ? 0 : Convert.ToInt32(decReader["UnderReview"])).ToString();
                    litDecisionsApproved.Text = (decReader["Approved"] == DBNull.Value ? 0 : Convert.ToInt32(decReader["Approved"])).ToString();
                    litDecisionsRejected.Text = (decReader["Rejected"] == DBNull.Value ? 0 : Convert.ToInt32(decReader["Rejected"])).ToString();
                    litDecisionsCompleted.Text = (decReader["Completed"] == DBNull.Value ? 0 : Convert.ToInt32(decReader["Completed"])).ToString();
                }
                decReader.Close();

                // Event stats
                SqlCommand cmdEvents = new SqlCommand(
                    @"SELECT 
                        SUM(CASE WHEN REPLACE(LTRIM(RTRIM(ISNULL(LiveStatus, N''))), N' ', N'') = N'Planned' THEN 1 ELSE 0 END) AS Planned,
                        SUM(CASE WHEN REPLACE(LTRIM(RTRIM(ISNULL(LiveStatus, N''))), N' ', N'') = N'InProgress' THEN 1 ELSE 0 END) AS InProgress,
                        SUM(CASE WHEN REPLACE(LTRIM(RTRIM(ISNULL(LiveStatus, N''))), N' ', N'') = N'Completed' THEN 1 ELSE 0 END) AS Completed,
                        SUM(CASE WHEN REPLACE(LTRIM(RTRIM(ISNULL(LiveStatus, N''))), N' ', N'') = N'Cancelled' THEN 1 ELSE 0 END) AS Cancelled
                      FROM (
                        SELECT CASE
                            WHEN REPLACE(LTRIM(RTRIM(ISNULL(e.Status, N''))), N' ', N'') IN (N'Proposed', N'Rejected', N'Cancelled', N'Archived')
                                THEN LTRIM(RTRIM(e.Status))
                            WHEN ts.Total > 0 AND ts.Completed = ts.Total THEN N'Completed'
                            WHEN ISNULL(ts.InWork, 0) > 0 OR (ISNULL(ts.Completed, 0) > 0 AND ISNULL(ts.Completed, 0) < ts.Total) THEN N'InProgress'
                            ELSE LTRIM(RTRIM(ISNULL(e.Status, N'Planned')))
                        END AS LiveStatus
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
                      ) live", con);
                SqlDataReader evReader = cmdEvents.ExecuteReader();
                if (evReader.Read())
                {
                    litEventsPlanned.Text = (evReader["Planned"] == DBNull.Value ? 0 : Convert.ToInt32(evReader["Planned"])).ToString();
                    litEventsInProgress.Text = (evReader["InProgress"] == DBNull.Value ? 0 : Convert.ToInt32(evReader["InProgress"])).ToString();
                    litEventsCompleted.Text = (evReader["Completed"] == DBNull.Value ? 0 : Convert.ToInt32(evReader["Completed"])).ToString();
                    litEventsCancelled.Text = (evReader["Cancelled"] == DBNull.Value ? 0 : Convert.ToInt32(evReader["Cancelled"])).ToString();
                }
                evReader.Close();
            }
        }

        protected void btnFullAudit_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Modules/Reports/Reports.aspx");
        }

        protected void btnSeeAllActivity_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Modules/Reports/AuditTrail.aspx");
        }
    }
}