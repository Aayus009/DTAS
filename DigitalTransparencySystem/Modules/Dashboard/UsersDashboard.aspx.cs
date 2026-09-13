using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Dashboard
{
    public partial class UserDashboard : System.Web.UI.Page
    {
        private string connectionString;
        private bool isFacultyStaff;

        private const string MyTaskMembershipFilter = TaskAccess.UserCanSeeTask;

        protected void Page_Load(object sender, EventArgs e)
        {
            connectionString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;

            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Modules/Authentication/Login.aspx");
                return;
            }

            if (RoleAccess.IsAdmin(Session["Role"] as string))
            {
                Response.Redirect("~/Modules/Dashboard/AdminDashboard.aspx");
                return;
            }

            isFacultyStaff = RoleAccess.IsFacultyOrStaff(Session["Role"] as string);
            ApplyRoleLayout();
            BindSuspensionBanner();

            if (!IsPostBack)
            {
                TaskFilter = "All";
                SetActiveFilterButton();
                LoadDashboardData();
                BindIdentityBanner();
                NotificationService.EnsureAssignmentReminders(Convert.ToInt32(Session["UserID"]));
            }
        }

        private void BindSuspensionBanner()
        {
            if (!AuthService.IsSuspendedViewOnly(Session))
            {
                int userId;
                if (int.TryParse(Convert.ToString(Session["UserID"]), out userId))
                    AuthService.ApplySuspensionSession(Session, AuthService.FindById(userId));
            }

            if (!AuthService.IsSuspendedViewOnly(Session))
            {
                pnlSuspendedBanner.Visible = false;
                return;
            }

            string notice = AuthService.GetSuspensionBanner(Session);
            if (string.IsNullOrEmpty(notice))
            {
                int userId;
                SuspensionInfo info = int.TryParse(Convert.ToString(Session["UserID"]), out userId)
                    ? AuthService.GetActiveSuspension(userId)
                    : null;
                notice = info == null ? "Your account is suspended. You can view pages but cannot use system features." : info.BannerText;
            }

            pnlSuspendedBanner.Visible = true;
            litSuspendedBanner.Text = Server.HtmlEncode(notice);
            ClientScript.RegisterStartupScript(GetType(), "dtasSuspendedDash",
                "document.body.classList.add('is-suspended');", true);
        }

        private void BindIdentityBanner()
        {
            object verified = Session["IdentityVerified"];
            bool isVerified = verified is bool && (bool)verified;
            if (!isVerified && verified != null)
                bool.TryParse(verified.ToString(), out isVerified);

            if (isVerified)
            {
                pnlIdentityBanner.Visible = false;
                return;
            }

            pnlIdentityBanner.Visible = true;
            string status = Session["VerificationStatus"] as string ?? "None";
            if (string.Equals(status, "Pending", StringComparison.OrdinalIgnoreCase))
                lblIdentityBanner.Text = "Your ID is waiting for admin review. Tasks, events, and polls stay locked until it is approved.";
            else if (string.Equals(status, "Rejected", StringComparison.OrdinalIgnoreCase))
                lblIdentityBanner.Text = "Your last ID submission was rejected. Upload a clearer document to request another review.";
            else
                lblIdentityBanner.Text = "Upload your institutional ID to unlock tasks, events, workspaces, and polls.";
        }

        private void ApplyRoleLayout()
        {
            pnlMeetingsMetric.Visible = isFacultyStaff;
            pnlMeetingsSection.Visible = true;
            pnlReportsSection.Visible = isFacultyStaff;
            pnlQuickMeetings.Visible = true;
            pnlQuickReports.Visible = isFacultyStaff;
            pnlNotificationsMetric.Visible = !isFacultyStaff;

            litSubtitle.Text = isFacultyStaff
                ? "Here's an overview of your tasks, meetings, and accountability reports."
                : "Here's an overview of your tasks and community activity.";
        }

        private void LoadDashboardData()
        {
            LoadUserInfo();
            LoadMetrics();
            LoadChartMetrics();
            LoadMyTasks();
            LoadMyMeetings();
            if (isFacultyStaff)
                LoadReportSnapshot();
            LoadRecentActivity();
            LoadPendingInvitations();
            litLastUpdated.Text = DateTime.Now.ToString("MMM dd, yyyy - hh:mm tt");
        }

        private void LoadUserInfo()
        {
            string fullName = Session["FullName"] as string ?? "";
            if (string.IsNullOrEmpty(fullName))
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("SELECT FullName FROM Users WHERE UserID = @UserID", con);
                    cmd.Parameters.AddWithValue("@UserID", Session["UserID"]);
                    object result = cmd.ExecuteScalar();
                    if (result != null) fullName = result.ToString();
                }
            }

            if (!string.IsNullOrEmpty(fullName))
            {
                string firstName = fullName.Split(' ')[0];
                litFirstName.Text = Server.HtmlEncode(firstName);
            }
        }

        private void LoadMetrics()
        {
            string userId = Session["UserID"].ToString();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                if (isFacultyStaff)
                {
                    SqlCommand cmdMeetings = new SqlCommand(
                        @"SELECT COUNT(*) FROM MeetingParticipants
                          WHERE UserID = @UserID", con);
                    cmdMeetings.Parameters.AddWithValue("@UserID", userId);
                    litMyMeetings.Text = cmdMeetings.ExecuteScalar().ToString();

                    SqlCommand cmdUpcoming = new SqlCommand(
                        @"SELECT COUNT(*) FROM MeetingParticipants mp
                          INNER JOIN Meetings m ON mp.MeetingID = m.MeetingID
                          WHERE mp.UserID = @UserID AND m.ScheduledDate >= GETDATE()
                          AND m.Status IN ('Scheduled', 'InProgress')", con);
                    cmdUpcoming.Parameters.AddWithValue("@UserID", userId);
                    litUpcomingMeetings.Text = cmdUpcoming.ExecuteScalar().ToString();
                }
                else
                {
                    SqlCommand cmdUnread = new SqlCommand(
                        @"SELECT COUNT(*) FROM Notifications
                          WHERE UserID = @UserID AND ISNULL(IsRead, 0) = 0", con);
                    cmdUnread.Parameters.AddWithValue("@UserID", userId);
                    litUnreadNotifications.Text = cmdUnread.ExecuteScalar().ToString();
                }

                SqlCommand cmdFeedback = new SqlCommand(
                    @"SELECT COUNT(*) FROM Feedback WHERE UserID = @UserID", con);
                cmdFeedback.Parameters.AddWithValue("@UserID", userId);
                litTotalFeedback.Text = cmdFeedback.ExecuteScalar().ToString();

                SqlCommand cmdResolved = new SqlCommand(
                    @"SELECT COUNT(*) FROM Feedback WHERE UserID = @UserID AND Status = 'Resolved'", con);
                cmdResolved.Parameters.AddWithValue("@UserID", userId);
                litResolvedFeedback.Text = cmdResolved.ExecuteScalar().ToString();

                litPendingFeedback.Text = (int.Parse(litTotalFeedback.Text) - int.Parse(litResolvedFeedback.Text)).ToString();
            }
        }

        private void LoadChartMetrics()
        {
            string userId = Session["UserID"].ToString();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                SqlCommand cmdTasks = new SqlCommand(
                    @"SELECT 
                        SUM(CASE WHEN t.Status = 'Completed' THEN 1 ELSE 0 END) AS Completed,
                        SUM(CASE WHEN t.Status IN ('InProgress', 'In Progress') THEN 1 ELSE 0 END) AS InProgress,
                        SUM(CASE WHEN t.Status IN ('Pending', 'NotStarted', 'Not Started') THEN 1 ELSE 0 END) AS Pending,
                        SUM(CASE WHEN t.Status = 'Delayed' THEN 1 ELSE 0 END) AS Delayed
                      FROM Tasks t
                      WHERE " + MyTaskMembershipFilter, con);
                cmdTasks.Parameters.AddWithValue("@UserID", userId);
                SqlDataReader taskReader = cmdTasks.ExecuteReader();
                if (taskReader.Read())
                {
                    litTasksCompleted.Text = (taskReader["Completed"] == DBNull.Value ? 0 : Convert.ToInt32(taskReader["Completed"])).ToString();
                    litTasksInProgress.Text = (taskReader["InProgress"] == DBNull.Value ? 0 : Convert.ToInt32(taskReader["InProgress"])).ToString();
                    litTasksPending.Text = (taskReader["Pending"] == DBNull.Value ? 0 : Convert.ToInt32(taskReader["Pending"])).ToString();
                    litTasksDelayed.Text = (taskReader["Delayed"] == DBNull.Value ? 0 : Convert.ToInt32(taskReader["Delayed"])).ToString();
                }
                taskReader.Close();

                SqlCommand cmdDecisions = new SqlCommand(
                    @"SELECT 
                        SUM(CASE WHEN Status IN ('Proposed', 'Created') THEN 1 ELSE 0 END) AS Proposed,
                        SUM(CASE WHEN Status IN ('UnderReview', 'Under Review', 'Reviewed') THEN 1 ELSE 0 END) AS UnderReview,
                        SUM(CASE WHEN Status = 'Approved' THEN 1 ELSE 0 END) AS Approved,
                        SUM(CASE WHEN Status = 'Rejected' THEN 1 ELSE 0 END) AS Rejected,
                        SUM(CASE WHEN Status IN ('Completed', 'Implemented', 'Closed') THEN 1 ELSE 0 END) AS Completed
                      FROM Decisions
                      WHERE ResponsibleUserID = @UserID", con);
                cmdDecisions.Parameters.AddWithValue("@UserID", userId);
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
            }
        }

        private string TaskFilter
        {
            get { return ViewState["TaskFilter"] as string ?? "All"; }
            set { ViewState["TaskFilter"] = value; }
        }

        private void SetActiveFilterButton()
        {
            string idle = "dash-task-filter";
            string active = "dash-task-filter is-active";

            btnFilterAll.CssClass = idle;
            btnFilterPending.CssClass = idle;
            btnFilterInProgress.CssClass = idle;
            btnFilterCompleted.CssClass = idle;
            btnFilterLate.CssClass = idle;

            switch (TaskFilter)
            {
                case "Pending": btnFilterPending.CssClass = active; break;
                case "InProgress": btnFilterInProgress.CssClass = active; break;
                case "Completed": btnFilterCompleted.CssClass = active; break;
                case "Late": btnFilterLate.CssClass = active; break;
                default: btnFilterAll.CssClass = active; break;
            }
        }

        private void ApplyTaskFilter(string filter)
        {
            TaskFilter = filter;
            SetActiveFilterButton();
            LoadMyTasks();
            upMyTasks.Update();
        }

        protected void btnFilterAll_Click(object sender, EventArgs e)
        {
            ApplyTaskFilter("All");
        }

        protected void btnFilterPending_Click(object sender, EventArgs e)
        {
            ApplyTaskFilter("Pending");
        }

        protected void btnFilterInProgress_Click(object sender, EventArgs e)
        {
            ApplyTaskFilter("InProgress");
        }

        protected void btnFilterCompleted_Click(object sender, EventArgs e)
        {
            ApplyTaskFilter("Completed");
        }

        protected void btnFilterLate_Click(object sender, EventArgs e)
        {
            ApplyTaskFilter("Late");
        }

        private void LoadMyTasks()
        {
            string userId = Session["UserID"].ToString();
            string statusClause = "t.Status NOT IN ('Cancelled', 'Archived')";

            switch (TaskFilter)
            {
                case "Pending":
                    statusClause = "t.Status IN ('Pending', 'NotStarted', 'Not Started')";
                    break;
                case "InProgress":
                    statusClause = "t.Status IN ('InProgress', 'In Progress')";
                    break;
                case "Completed":
                    statusClause = "t.Status = 'Completed'";
                    break;
                case "Late":
                    statusClause = @"t.Status NOT IN ('Completed', 'Cancelled', 'Archived')
                        AND t.DueDate IS NOT NULL AND CAST(t.DueDate AS DATE) < CAST(GETDATE() AS DATE)";
                    break;
            }

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
                    @"SELECT TOP 20
                        t.TaskID,
                        t.TaskTitle,
                        t.Priority,
                        t.Status,
                        t.DueDate,
                        ISNULL(e.EventName, 'General') AS EventName
                      FROM Tasks t
                      LEFT JOIN Events e ON t.EventID = e.EventID
                      WHERE " + statusClause + @"
                        AND " + MyTaskMembershipFilter + @"
                      ORDER BY
                        CASE t.Status
                            WHEN 'InProgress' THEN 1
                            WHEN 'In Progress' THEN 1
                            WHEN 'Pending' THEN 2
                            WHEN 'NotStarted' THEN 3
                            WHEN 'Not Started' THEN 3
                            WHEN 'Delayed' THEN 4
                            WHEN 'Completed' THEN 5
                            ELSE 6
                        END,
                        t.DueDate", con);
                cmd.Parameters.AddWithValue("@UserID", userId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                rptMyTasks.DataSource = dt;
                rptMyTasks.DataBind();
                bool hasTasks = dt.Rows.Count > 0;
                pnlTaskTable.Visible = hasTasks;
                pnlNoTasks.Visible = !hasTasks;
                pnlTaskFooter.Visible = hasTasks;
            }
        }

        private void LoadMyMeetings()
        {
            string userId = Session["UserID"].ToString();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
                    @"SELECT TOP 5 
                        m.MeetingID,
                        m.MeetingTitle,
                        ISNULL(e.EventName, 'General') AS EventName,
                        m.ScheduledDate,
                        m.Status
                      FROM Meetings m
                      INNER JOIN MeetingParticipants mp ON m.MeetingID = mp.MeetingID
                      LEFT JOIN Events e ON m.EventID = e.EventID
                      WHERE mp.UserID = @UserID
                        AND m.ScheduledDate >= GETDATE()
                        AND m.Status IN ('Scheduled', 'InProgress')
                      ORDER BY m.ScheduledDate", con);
                cmd.Parameters.AddWithValue("@UserID", userId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                rptMyMeetings.DataSource = dt;
                rptMyMeetings.DataBind();
            }
        }

        private void LoadReportSnapshot()
        {
            string userId = Session["UserID"].ToString();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                SqlCommand cmdEvents = new SqlCommand(
                    @"SELECT COUNT(DISTINCT e.EventID)
                      FROM Events e
                      WHERE e.ProposedBy = @UserID
                         OR e.EventID IN (SELECT EventID FROM Tasks t WHERE " + MyTaskMembershipFilter + @")
                         OR e.EventID IN (
                            SELECT m.EventID FROM Meetings m
                            INNER JOIN MeetingParticipants mp ON m.MeetingID = mp.MeetingID
                            WHERE mp.UserID = @UserID AND m.EventID IS NOT NULL
                         )", con);
                cmdEvents.Parameters.AddWithValue("@UserID", userId);
                litReportEvents.Text = Convert.ToInt32(cmdEvents.ExecuteScalar()).ToString();

                SqlCommand cmdTasks = new SqlCommand(
                    "SELECT COUNT(*) FROM Tasks t WHERE " + MyTaskMembershipFilter, con);
                cmdTasks.Parameters.AddWithValue("@UserID", userId);
                litReportTasks.Text = cmdTasks.ExecuteScalar().ToString();

                SqlCommand cmdOverdue = new SqlCommand(
                    @"SELECT COUNT(*) FROM Tasks t
                      WHERE t.Status NOT IN ('Completed', 'Cancelled', 'Archived')
                        AND t.DueDate IS NOT NULL AND t.DueDate < GETDATE()
                        AND " + MyTaskMembershipFilter, con);
                cmdOverdue.Parameters.AddWithValue("@UserID", userId);
                litReportOverdue.Text = cmdOverdue.ExecuteScalar().ToString();
            }
        }

        private void LoadRecentActivity()
        {
            string userId = Session["UserID"].ToString();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
                    @"SELECT TOP 6 
                        ActivityTitle AS Title,
                        ActivityTime AS EventTime,
                        ActivityDescription AS Description,
                        DotColor
                      FROM (
                        SELECT TOP 3
                            'Task Updated' AS ActivityTitle,
                            tu.UpdatedAt AS ActivityTime,
                            CONCAT(t.TaskTitle, ' - ', tu.NewStatus) AS ActivityDescription,
                            '#001142' AS DotColor
                        FROM TaskUpdates tu
                        INNER JOIN Tasks t ON tu.TaskID = t.TaskID
                        WHERE " + MyTaskMembershipFilter + @"
                        ORDER BY tu.UpdatedAt DESC

                        UNION ALL

                        SELECT TOP 3
                            'Decision Changed' AS ActivityTitle,
                            dh.ChangedAt AS ActivityTime,
                            CONCAT(d.DecisionTitle, ' - ', dh.NewStatus) AS ActivityDescription,
                            '#005137' AS DotColor
                        FROM DecisionHistory dh
                        INNER JOIN Decisions d ON dh.DecisionID = d.DecisionID
                        WHERE d.ResponsibleUserID = @UserID
                        ORDER BY dh.ChangedAt DESC
                      ) AS CombinedActivity
                      ORDER BY ActivityTime DESC", con);
                cmd.Parameters.AddWithValue("@UserID", userId);

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

                rptRecentActivity.DataSource = dt;
                rptRecentActivity.DataBind();
            }
        }

        private void LoadPendingInvitations()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    @"SELECT ttm.MemberID, t.TaskTitle,
                             CASE WHEN ISNULL(tt.LeaderID, 0) = ttm.UserID THEN 'Leader' ELSE 'Member' END AS TeamRole,
                             ISNULL(u.FullName, '') AS LeaderName
                      FROM TaskTeamMembers ttm
                      INNER JOIN TaskTeams tt ON ttm.TeamID = tt.TeamID
                      INNER JOIN Tasks t ON tt.TaskID = t.TaskID
                      LEFT JOIN Users u ON tt.LeaderID = u.UserID
                      WHERE ttm.UserID = @UserID AND ttm.Status = 'Invited'
                      ORDER BY ttm.InvitedAt DESC", con);
                cmd.Parameters.AddWithValue("@UserID", Session["UserID"]);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                pnlInvitations.Visible = dt.Rows.Count > 0;
                rptInvitations.DataSource = dt;
                rptInvitations.DataBind();
            }
        }

        protected void rptInvitations_ItemCommand(object source, System.Web.UI.WebControls.RepeaterCommandEventArgs e)
        {
            int memberId;
            if (!int.TryParse(e.CommandArgument.ToString(), out memberId))
                return;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlTransaction tx = con.BeginTransaction();
                try
                {
                    SqlCommand cmdFetch = new SqlCommand(
                        @"SELECT tt.TaskID, tt.TeamID, tt.LeaderID, t.TaskTitle
                          FROM TaskTeamMembers ttm
                          INNER JOIN TaskTeams tt ON ttm.TeamID = tt.TeamID
                          INNER JOIN Tasks t ON tt.TaskID = t.TaskID
                          WHERE ttm.MemberID = @MemberID", con, tx);
                    cmdFetch.Parameters.AddWithValue("@MemberID", memberId);
                    SqlDataReader reader = cmdFetch.ExecuteReader();
                    int taskId = 0, leaderId = 0;
                    string taskTitle = "";
                    if (reader.Read())
                    {
                        taskId = Convert.ToInt32(reader["TaskID"]);
                        taskTitle = reader["TaskTitle"].ToString();
                        if (reader["LeaderID"] != DBNull.Value)
                            leaderId = Convert.ToInt32(reader["LeaderID"]);
                    }
                    reader.Close();

                    if (e.CommandName == "Accept")
                    {
                        SqlCommand cmdAccept = new SqlCommand(
                            @"UPDATE TaskTeamMembers SET Status = 'Accepted', RespondedAt = GETDATE()
                              WHERE MemberID = @MemberID AND UserID = @UserID", con, tx);
                        cmdAccept.Parameters.AddWithValue("@MemberID", memberId);
                        cmdAccept.Parameters.AddWithValue("@UserID", Session["UserID"]);
                        cmdAccept.ExecuteNonQuery();

                        NotifyUser(con, tx, leaderId, "Invitation Accepted",
                            $"A user has accepted the invitation to the team for task: {taskTitle}.", taskId);
                    }
                    else if (e.CommandName == "Decline")
                    {
                        SqlCommand cmdDecline = new SqlCommand(
                            @"UPDATE TaskTeamMembers SET Status = 'Declined', RespondedAt = GETDATE()
                              WHERE MemberID = @MemberID AND UserID = @UserID", con, tx);
                        cmdDecline.Parameters.AddWithValue("@MemberID", memberId);
                        cmdDecline.Parameters.AddWithValue("@UserID", Session["UserID"]);
                        cmdDecline.ExecuteNonQuery();

                        NotifyUser(con, tx, leaderId, "Invitation Declined",
                            $"A user has declined the invitation to the team for task: {taskTitle}. You may need to fill the seat or proceed short-handed.", taskId);

                        SqlCommand cmdAdmins = new SqlCommand(
                            @"INSERT INTO Notifications (UserID, Title, Message, IsRead, NotificationType, RelatedID, RelatedType, CreatedAt)
                              SELECT u.UserID, @Title, @Message, 0, 'TaskTeamInvite', @RelatedID, 'Task', GETDATE()
                              FROM Users u
                              INNER JOIN Roles r ON u.RoleID = r.RoleID
                              WHERE r.RoleName = 'Admin' AND ISNULL(u.IsActive, 1) = 1", con, tx);
                        cmdAdmins.Parameters.AddWithValue("@Title", "Team invitation declined");
                        cmdAdmins.Parameters.AddWithValue("@Message", $"A team invitation for task '{taskTitle}' was declined and may need reassignment.");
                        cmdAdmins.Parameters.AddWithValue("@RelatedID", taskId);
                        cmdAdmins.ExecuteNonQuery();
                    }

                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }

            LoadPendingInvitations();
            LoadMyTasks();
            LoadMetrics();
        }

        private void NotifyUser(SqlConnection con, SqlTransaction tx, int userId, string title, string message, int relatedId)
        {
            if (userId <= 0)
                return;

            SqlCommand cmdNotif = new SqlCommand(
                @"INSERT INTO Notifications (UserID, Title, Message, IsRead, NotificationType, RelatedID, RelatedType, CreatedAt)
                  VALUES (@UserID, @Title, @Message, 0, 'TaskTeamInvite', @RelatedID, 'Task', GETDATE())", con, tx);
            cmdNotif.Parameters.AddWithValue("@UserID", userId);
            cmdNotif.Parameters.AddWithValue("@Title", title);
            cmdNotif.Parameters.AddWithValue("@Message", message);
            cmdNotif.Parameters.AddWithValue("@RelatedID", relatedId);
            cmdNotif.ExecuteNonQuery();
        }

        protected string GetPriorityDotColor(string priority)
        {
            switch (priority?.ToLower())
            {
                case "high":
                case "critical":
                    return "background-color: #ba1a1a";
                case "medium":
                    return "background-color: #e6ba00";
                case "low":
                    return "background-color: #005137";
                default:
                    return "background-color: #757682";
            }
        }
    }
}
