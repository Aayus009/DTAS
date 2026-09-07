using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Tasks
{
    public partial class TaskDetails : System.Web.UI.Page
    {
        private string connectionString;
        private int taskID;
        private bool isAdmin;
        private bool isLeader;
        private int currentUserID;

        protected void Page_Load(object sender, EventArgs e)
        {
            connectionString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;

            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Modules/Authentication/Login.aspx");
                return;
            }

            if (!int.TryParse(Request.QueryString["TaskID"], out taskID))
            {
                string redirectPage = isAdmin ? "~/Modules/Tasks/Tasks.aspx" : "~/Modules/UserTasks/UserTasks.aspx";
                Response.Redirect(redirectPage);
                return;
            }

            currentUserID = Convert.ToInt32(Session["UserID"]);
            string role = Session["Role"] as string;
            isAdmin = role != null && role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
            isLeader = IsLeader(currentUserID);

            if (!IsLeaderOrMember(currentUserID) && !isAdmin)
            {
                Response.Redirect("~/Modules/UserTasks/UserTasks.aspx");
                return;
            }

            if (!IsPostBack)
            {
                ApplyRoleLayout();
                lnkEditTask.Visible = RestrictionService.CanEditTask(taskID, currentUserID, role);
                lnkEditTask.NavigateUrl = $"~/Modules/Tasks/EditTask.aspx?TaskID={taskID}";
                bool locked = RestrictionService.IsTaskLocked(taskID);
                pnlRestricted.Visible = locked;
                pnlStatusUpdate.Visible = !isAdmin && !locked;
                BindEventTaskStatuses();
                LoadTaskDetails();
                BindDeadlineRisk();
                LoadAssignedUsers();
                LoadTaskUpdates();
                LoadAttachments();
                EventTaskService.EnsureCommentMentionSchema();
                LoadTagUsers();
                LoadComments();
                LoadChecklist();
                LoadTeamInfo();
                LoadEventTeamProgress();
            }
        }

        private void ApplyRoleLayout()
        {
            pnlAdminLayout.Visible = isAdmin;
            pnlUserLayout.Visible = !isAdmin;
            pnlAdminSidebar.Visible = isAdmin;
            pnlUserSidebar.Visible = !isAdmin;

            if (isAdmin)
            {
                lnkBreadcrumbDashboard.NavigateUrl = "~/Modules/Dashboard/AdminDashboard.aspx";
                lnkBreadcrumbDashboard.Text = "Dashboard";
                lnkBreadcrumbTasks.NavigateUrl = "~/Modules/Tasks/Tasks.aspx";
                lnkBreadcrumbTasks.Text = "Tasks";
                btnBackToList.PostBackUrl = "~/Modules/Tasks/Tasks.aspx";
                pnlUploadAttachment.Visible = false;
                pnlAdminNoUpload.Visible = true;
                pnlChecklistEdit.Visible = false;
            }
            else
            {
                lnkBreadcrumbDashboard.NavigateUrl = "~/Modules/Dashboard/UsersDashboard.aspx";
                lnkBreadcrumbDashboard.Text = "Dashboard";
                lnkBreadcrumbTasks.NavigateUrl = "~/Modules/UserTasks/UserTasks.aspx";
                lnkBreadcrumbTasks.Text = "My Tasks";
                btnBackToList.PostBackUrl = "~/Modules/UserTasks/UserTasks.aspx";
                pnlUploadAttachment.Visible = true;
                pnlAdminNoUpload.Visible = false;
                pnlChecklistEdit.Visible = true;
            }
        }

        private bool IsLeader(int userID)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    @"SELECT COUNT(1) FROM TaskTeams WHERE TaskID = @TaskID AND LeaderID = @UserID", con);
                cmd.Parameters.AddWithValue("@TaskID", taskID);
                cmd.Parameters.AddWithValue("@UserID", userID);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private void LoadTaskDetails()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    @"SELECT t.TaskID, t.TaskTitle, t.Description, t.Priority, t.Status, t.DueDate,
                             t.CreatedBy, t.CreatedAt, t.UpdatedAt,
                             t.DecisionID, t.EventID, t.MeetingID,
                             u.FullName AS CreatedByName,
                             d.DecisionTitle,
                             e.EventName,
                             m.MeetingTitle
                      FROM Tasks t
                      LEFT JOIN Users u ON t.CreatedBy = u.UserID
                      LEFT JOIN Decisions d ON t.DecisionID = d.DecisionID
                      LEFT JOIN Events e ON t.EventID = e.EventID
                      LEFT JOIN Meetings m ON t.MeetingID = m.MeetingID
                      WHERE t.TaskID = @TaskID", con);
                cmd.Parameters.AddWithValue("@TaskID", taskID);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string taskTitle = reader["TaskTitle"].ToString();
                        string status = reader["Status"].ToString();
                        string priority = reader["Priority"].ToString();

                        litTaskTitle.Text = $"<h1 class=\"font-headline-lg text-headline-lg text-primary\">{Server.HtmlEncode(taskTitle)}</h1>";
                        object dueDate = reader["DueDate"];
                        litStatusBadge.Text = GetStatusBadgeHtml(status, dueDate);
                        litPriorityBadge.Text = GetPriorityBadgeHtml(priority);
                        litDescription.Text = Server.HtmlEncode(reader["Description"].ToString());
                        litCreatedBy.Text = Server.HtmlEncode(reader["CreatedByName"].ToString());
                        litDueDate.Text = TimelineService.DueLabel(dueDate, status);
                        litCreatedAt.Text = reader["CreatedAt"] == DBNull.Value ? "Not set" : Convert.ToDateTime(reader["CreatedAt"]).ToString("MMM dd, yyyy - hh:mm tt");
                        litUpdatedAt.Text = reader["UpdatedAt"] == DBNull.Value ? "Not set" : Convert.ToDateTime(reader["UpdatedAt"]).ToString("MMM dd, yyyy - hh:mm tt");

                        // Related Decision
                        if (reader["DecisionID"] != DBNull.Value)
                        {
                            pnlDecision.Visible = true;
                            lnkDecision.Text = Server.HtmlEncode(reader["DecisionTitle"].ToString());
                            lnkDecision.NavigateUrl = $"~/Modules/Decisions/DecisionDetails.aspx?DecisionID={reader["DecisionID"]}";
                        }

                        // Related Event
                        if (reader["EventID"] != DBNull.Value)
                        {
                            pnlEvent.Visible = true;
                            lnkEvent.Text = Server.HtmlEncode(reader["EventName"].ToString());
                            lnkEvent.NavigateUrl = $"~/Modules/Events/EventDetails.aspx?EventID={reader["EventID"]}";
                        }

                        // Related Meeting
                        if (reader["MeetingID"] != DBNull.Value)
                        {
                            pnlMeeting.Visible = true;
                            lnkMeeting.Text = Server.HtmlEncode(reader["MeetingTitle"].ToString());
                            lnkMeeting.NavigateUrl = $"~/Modules/Meetings/MeetingDetails.aspx?MeetingID={reader["MeetingID"]}";
                        }

                        // Show "no related items" if none
                        if (pnlDecision.Visible == false && pnlEvent.Visible == false && pnlMeeting.Visible == false)
                        {
                            pnlNoRelated.Visible = true;
                        }
                    }
                    else
                    {
                        Response.Redirect("~/Modules/Tasks/Tasks.aspx");
                    }
                }
            }
        }

        protected void btnExtendDue_Click(object sender, EventArgs e)
        {
            DateTime due;
            if (!DateTime.TryParse(txtExtendDue.Text, out due))
            {
                pnlError.Visible = true;
                lblError.Text = "Enter a valid due date.";
                BindDeadlineRisk();
                return;
            }
            string error = DeadlineService.ExtendTaskDueDate(taskID, currentUserID, Session["Role"] as string, due);
            if (error != null)
            {
                pnlError.Visible = true;
                lblError.Text = error;
            }
            LoadTaskDetails();
            BindDeadlineRisk();
        }

        private void BindDeadlineRisk()
        {
            DeadlineRisk risk = DeadlineService.GetTaskRisk(taskID);
            pnlDeadlineRisk.Visible = risk.ShowNearDueAlert;
            if (!risk.ShowNearDueAlert)
                return;

            spanRiskBadge.InnerText = risk.Severity;
            spanRiskBadge.Attributes["class"] = "inline-block px-2.5 py-1 rounded-full font-badge-cap text-badge-cap mb-2 " + risk.BadgeClass;
            litRiskTitle.Text = Server.HtmlEncode(risk.Title);
            litRiskMessage.Text = Server.HtmlEncode(risk.Message);
            bool canManage = DeadlineService.CanManageTaskDeadline(taskID, currentUserID, Session["Role"] as string);
            pnlDeadlineActions.Visible = canManage && risk.ShowRecoveryActions;
            lnkRedistribute.NavigateUrl = "~/Modules/Tasks/TaskTeam.aspx?TaskID=" + taskID;
            if (risk.DaysRemaining >= 0)
                txtExtendDue.Text = DateTime.Today.AddDays(Math.Max(7, risk.DaysRemaining + 7)).ToString("yyyy-MM-dd");
            else
                txtExtendDue.Text = DateTime.Today.AddDays(7).ToString("yyyy-MM-dd");
        }

        private void LoadTeamInfo()
        {
            int userID = Convert.ToInt32(Session["UserID"]);

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                SqlCommand cmdTeam = new SqlCommand(
                    @"SELECT tt.TeamID, tt.LeaderID, ISNULL(tt.ProgressPercent, 0) AS ProgressPercent,
                             u.FullName AS LeaderName
                      FROM TaskTeams tt
                      INNER JOIN Users u ON tt.LeaderID = u.UserID
                      WHERE tt.TaskID = @TaskID", con);
                cmdTeam.Parameters.AddWithValue("@TaskID", taskID);

                using (SqlDataReader reader = cmdTeam.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int leaderID = reader["LeaderID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["LeaderID"]);
                        int progress = reader["ProgressPercent"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ProgressPercent"]);
                        string leaderName = reader["LeaderName"].ToString();

                        pnlTeamProgress.Visible = true;
                        litTeamProgress.Text = $"<div class='h-full bg-[#2e7d32] rounded-full' style='width: {progress}%;'></div>";
                        litTeamProgressText.Text = $"{progress}%";
                        litLeaderInfo.Text = $"<p class='text-sm text-on-surface-variant mt-2 flex items-center gap-1'><span class='material-symbols-outlined text-[14px]'>person</span> Leader: <strong>{Server.HtmlEncode(leaderName)}</strong></p>";

                        if (leaderID == userID && !isAdmin)
                        {
                            lnkManageTeam.Visible = true;
                            lnkManageTeam.NavigateUrl = $"~/Modules/Tasks/TaskTeam.aspx?TaskID={taskID}";
                        }
                        else
                        {
                            lnkManageTeam.Visible = false;
                        }

                        if (EventTaskService.IsEventLinked(taskID))
                            pnlTeamProgress.Visible = false;
                    }
                }

                SqlCommand cmdInvite = new SqlCommand(
                    "SELECT Status FROM TaskTeamMembers WHERE TeamID IN (SELECT TeamID FROM TaskTeams WHERE TaskID = @TaskID) AND UserID = @UserID", con);
                cmdInvite.Parameters.AddWithValue("@TaskID", taskID);
                cmdInvite.Parameters.AddWithValue("@UserID", userID);
                object inviteStatus = cmdInvite.ExecuteScalar();

                if (inviteStatus != null && inviteStatus.ToString() == "Invited")
                {
                    pnlTeamInvite.Visible = true;
                }
            }
        }

        protected void btnAcceptInvite_Click(object sender, EventArgs e)
        {
            int userID = Convert.ToInt32(Session["UserID"]);
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand(
                        @"UPDATE TaskTeamMembers SET Status = 'Accepted', RespondedAt = GETDATE()
                          WHERE TeamID IN (SELECT TeamID FROM TaskTeams WHERE TaskID = @TaskID)
                          AND UserID = @UserID AND Status = 'Invited'", con);
                    cmd.Parameters.AddWithValue("@TaskID", taskID);
                    cmd.Parameters.AddWithValue("@UserID", userID);
                    cmd.ExecuteNonQuery();
                }

                pnlTeamInvite.Visible = false;
                LoadTeamInfo();
            }
            catch (Exception ex)
            {
                pnlError.Visible = true;
                lblError.Text = "Error accepting invitation: " + ex.Message;
            }
        }

        protected void btnDeclineInvite_Click(object sender, EventArgs e)
        {
            int userID = Convert.ToInt32(Session["UserID"]);
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand(
                        @"UPDATE TaskTeamMembers SET Status = 'Declined', RespondedAt = GETDATE()
                          WHERE TeamID IN (SELECT TeamID FROM TaskTeams WHERE TaskID = @TaskID)
                          AND UserID = @UserID AND Status = 'Invited'", con);
                    cmd.Parameters.AddWithValue("@TaskID", taskID);
                    cmd.Parameters.AddWithValue("@UserID", userID);
                    cmd.ExecuteNonQuery();
                }

                pnlTeamInvite.Visible = false;
            }
            catch (Exception ex)
            {
                pnlError.Visible = true;
                lblError.Text = "Error declining invitation: " + ex.Message;
            }
        }

        private void LoadAssignedUsers()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
                    @"SELECT u.FullName, r.RoleName AS Role, ta.AssignedAt
                      FROM TaskAssignments ta
                      INNER JOIN Users u ON ta.UserID = u.UserID
                      INNER JOIN Roles r ON u.RoleID = r.RoleID
                      WHERE ta.TaskID = @TaskID
                      ORDER BY ta.AssignedAt", con);
                cmd.Parameters.AddWithValue("@TaskID", taskID);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    rptAssignedUsers.DataSource = dt;
                    rptAssignedUsers.DataBind();
                    pnlNoAssignees.Visible = false;
                }
                else
                {
                    rptAssignedUsers.Visible = false;
                    pnlNoAssignees.Visible = true;
                }
            }
        }

        private void LoadTaskUpdates()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
                    @"SELECT tu.OldStatus, tu.NewStatus, tu.Comment, tu.UpdatedAt,
                             u.FullName AS UserName
                      FROM TaskUpdates tu
                      INNER JOIN Users u ON tu.UserID = u.UserID
                      WHERE tu.TaskID = @TaskID
                      ORDER BY tu.UpdatedAt DESC", con);
                cmd.Parameters.AddWithValue("@TaskID", taskID);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dt.Columns.Add("DotStyle", typeof(string));
                dt.Columns.Add("IconStyle", typeof(string));
                dt.Columns.Add("Icon", typeof(string));
                dt.Columns.Add("OldStatusStyle", typeof(string));
                dt.Columns.Add("NewStatusStyle", typeof(string));

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        string oldStatus = row["OldStatus"].ToString();
                        string newStatus = row["NewStatus"].ToString();

                        string[] statusInfo = GetStatusUpdateVisuals(newStatus);
                        row["DotStyle"] = statusInfo[0];
                        row["IconStyle"] = statusInfo[1];
                        row["Icon"] = statusInfo[2];
                        row["OldStatusStyle"] = GetInlineStatusStyle(oldStatus);
                        row["NewStatusStyle"] = GetInlineStatusStyle(newStatus);
                    }

                    rptUpdates.DataSource = dt;
                    rptUpdates.DataBind();
                    pnlNoUpdates.Visible = false;
                    litUpdateCount.Text = $"<span class='font-label-md text-label-md text-on-surface-variant'>{dt.Rows.Count} update{(dt.Rows.Count != 1 ? "s" : "")}</span>";
                }
                else
                {
                    rptUpdates.Visible = false;
                    pnlNoUpdates.Visible = true;
                    litUpdateCount.Text = "";
                }
            }
        }

        private void LoadAttachments()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
                    @"SELECT AttachmentID, FileName, FilePath, FileSize, FileType, UploadedAt
                      FROM Attachments
                      WHERE RelatedID = @TaskID AND RelatedType = 'Task'
                      ORDER BY UploadedAt DESC", con);
                cmd.Parameters.AddWithValue("@TaskID", taskID);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dt.Columns.Add("Icon", typeof(string));
                dt.Columns.Add("FileSizeText", typeof(string));

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        string fileType = row["FileType"].ToString().ToLower();
                        string icon = "description";
                        if (fileType.Contains("image")) icon = "image";
                        else if (fileType.Contains("pdf")) icon = "picture_as_pdf";
                        else if (fileType.Contains("word") || fileType.Contains("doc")) icon = "article";
                        else if (fileType.Contains("excel") || fileType.Contains("sheet")) icon = "table_chart";
                        row["Icon"] = icon;

                        row["FileSizeText"] = row["FileSize"] == DBNull.Value
                            ? "Unknown size"
                            : FormatFileSize(Convert.ToInt64(row["FileSize"]));
                    }

                    rptAttachments.DataSource = dt;
                    rptAttachments.DataBind();
                    pnlNoAttachments.Visible = false;
                    litAttachmentCount.Text = $"<span class='font-label-md text-label-md text-on-surface-variant'>{dt.Rows.Count} file{(dt.Rows.Count != 1 ? "s" : "")}</span>";
                }
                else
                {
                    rptAttachments.Visible = false;
                    pnlNoAttachments.Visible = true;
                    litAttachmentCount.Text = "";
                }
            }
        }

        private void LoadEventTeamProgress()
        {
            int? eventId = EventTaskService.GetEventId(taskID);
            if (!eventId.HasValue)
            {
                pnlEventTeamProgress.Visible = false;
                return;
            }

            EventTeamProgress progress = EventTaskService.GetEventTeamProgress(eventId.Value);
            pnlEventTeamProgress.Visible = true;
            litEventProgressEvent.Text = Server.HtmlEncode(progress.EventName);
            litEventProgressPercent.Text = progress.Percent + "%";
            litEventProgressMeta.Text = progress.CompletedTasks + " of " + progress.TotalTasks + " event tasks completed";
            eventProgressFill.Style["width"] = progress.Percent + "%";

            rptMemberProgress.DataSource = progress.Members;
            rptMemberProgress.DataBind();
            rptMemberProgress.Visible = progress.Members.Count > 0;
            pnlNoMemberProgress.Visible = progress.Members.Count == 0;
        }

        private void LoadTagUsers()
        {
            ddlTagUser.Items.Clear();
            ddlTagUser.Items.Add(new ListItem("Everyone on this task", ""));
            foreach (DataRow row in EventTaskService.ListCommentTargets(taskID).Rows)
            {
                string name = Convert.ToString(row["FullName"]);
                string assigned = Convert.ToString(row["AssignedTasks"]);
                string label = string.IsNullOrWhiteSpace(assigned) ? name : name + " — " + assigned;
                ddlTagUser.Items.Add(new ListItem(label, Convert.ToString(row["UserID"])));
            }
        }

        private void LoadComments()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
                    @"SELECT tc.CommentID, tc.Comment, tc.CreatedAt, u.FullName AS UserName,
                             tc.MentionedUserID, ISNULL(mu.FullName, '') AS MentionedName
                      FROM TaskComments tc
                      INNER JOIN Users u ON tc.UserID = u.UserID
                      LEFT JOIN Users mu ON tc.MentionedUserID = mu.UserID
                      WHERE tc.TaskID = @TaskID
                      ORDER BY tc.CreatedAt ASC", con);
                cmd.Parameters.AddWithValue("@TaskID", taskID);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dt.Columns.Add("CommentHtml", typeof(string));
                dt.Columns.Add("HasMention", typeof(bool));

                foreach (DataRow row in dt.Rows)
                {
                    row["CommentHtml"] = HttpUtility.HtmlEncode(Convert.ToString(row["Comment"])).Replace("\n", "<br />");
                    row["HasMention"] = row["MentionedUserID"] != DBNull.Value && Convert.ToString(row["MentionedName"]).Length > 0;
                    row["UserName"] = HttpUtility.HtmlEncode(Convert.ToString(row["UserName"]));
                    row["MentionedName"] = HttpUtility.HtmlEncode(Convert.ToString(row["MentionedName"]));
                }

                if (dt.Rows.Count > 0)
                {
                    rptComments.Visible = true;
                    rptComments.DataSource = dt;
                    rptComments.DataBind();
                    pnlNoComments.Visible = false;
                    litCommentCount.Text = $"<span class='font-label-md text-label-md text-on-surface-variant'>{dt.Rows.Count} comment{(dt.Rows.Count != 1 ? "s" : "")}</span>";
                }
                else
                {
                    rptComments.Visible = false;
                    pnlNoComments.Visible = true;
                    litCommentCount.Text = "";
                }
            }
        }

        private void LoadChecklist()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
                    @"SELECT ItemID, ItemText, IsCompleted
                      FROM TaskChecklist
                      WHERE TaskID = @TaskID
                      ORDER BY IsCompleted ASC, CreatedAt ASC", con);
                cmd.Parameters.AddWithValue("@TaskID", taskID);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dt.Columns.Add("CheckIcon", typeof(string));
                dt.Columns.Add("CheckStyle", typeof(string));
                dt.Columns.Add("TextStyle", typeof(string));

                if (dt.Rows.Count > 0)
                {
                    int completedCount = 0;
                    foreach (DataRow row in dt.Rows)
                    {
                        bool isCompleted = row["IsCompleted"] != DBNull.Value && Convert.ToBoolean(row["IsCompleted"]);
                        if (isCompleted) completedCount++;

                        row["CheckIcon"] = isCompleted ? "check_circle" : "radio_button_unchecked";
                        row["CheckStyle"] = isCompleted
                            ? "color: #2e7d32;"
                            : "color: #757682;";
                        row["TextStyle"] = isCompleted
                            ? "text-decoration: line-through; color: #757682;"
                            : "";
                    }

                    rptChecklist.DataSource = dt;
                    rptChecklist.DataBind();
                    pnlNoChecklist.Visible = false;

                    int totalItems = dt.Rows.Count;
                    int percentage = totalItems > 0 ? (completedCount * 100 / totalItems) : 0;

                    pnlProgressBar.Visible = true;
                    litProgressBar.Text = $"<div class='h-full bg-[#2e7d32] rounded-full animate-progress' style='width: {percentage}%;'></div>";
                    litProgressText.Text = $"{completedCount}/{totalItems} ({percentage}%)";
                    litChecklistProgress.Text = $"<span class='font-label-md text-label-md text-on-surface-variant'>{percentage}%</span>";
                }
                else
                {
                    rptChecklist.Visible = false;
                    pnlNoChecklist.Visible = true;
                    pnlProgressBar.Visible = false;
                    litChecklistProgress.Text = "";
                }
            }
        }

        protected void btnUpdateStatus_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;
            if (isAdmin || RestrictionService.IsTaskLocked(taskID))
            {
                pnlError.Visible = true;
                lblError.Text = "This task is restricted. Work is locked until the system administrator restores it.";
                return;
            }

            try
            {
                string oldStatus = "";
                string newStatus = ddlNewStatus.SelectedValue;
                int actorId = Convert.ToInt32(Session["UserID"]);
                string actorRole = Session["Role"] as string;
                if (EventTaskService.IsEventLinked(taskID))
                {
                    if (EventTaskService.CanReview(taskID, actorId, actorRole))
                    {
                        bool approve = string.Equals(newStatus, "Completed", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(newStatus, "Approved", StringComparison.OrdinalIgnoreCase);
                        string reviewResult = EventTaskService.Review(
                            taskID,
                            actorId,
                            actorRole,
                            approve,
                            txtStatusComment.Text);
                        if (reviewResult != null && reviewResult != "approved-concluded")
                        {
                            pnlError.Visible = true;
                            lblError.Text = reviewResult;
                            return;
                        }
                        ddlNewStatus.SelectedIndex = 0;
                        txtStatusComment.Text = "";
                        LoadTaskDetails();
                        LoadTaskUpdates();
                        return;
                    }

                    string blocked = EventTaskService.ValidateWorkerStatus(taskID, actorId, actorRole, newStatus);
                    if (blocked != null)
                    {
                        pnlError.Visible = true;
                        lblError.Text = blocked;
                        return;
                    }
                }

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // Get current status
                    SqlCommand cmdOld = new SqlCommand("SELECT Status FROM Tasks WHERE TaskID = @TaskID", con);
                    cmdOld.Parameters.AddWithValue("@TaskID", taskID);
                    object oldResult = cmdOld.ExecuteScalar();
                    if (oldResult != null)
                        oldStatus = oldResult.ToString();

                    // Update task status
                    SqlCommand cmdUpdate = new SqlCommand(
                        "UPDATE Tasks SET Status = @Status, UpdatedAt = GETDATE() WHERE TaskID = @TaskID", con);
                    cmdUpdate.Parameters.AddWithValue("@Status", newStatus);
                    cmdUpdate.Parameters.AddWithValue("@TaskID", taskID);
                    cmdUpdate.ExecuteNonQuery();

                    // Auto roll up progress to parent Decision and Event
                    SqlCommand cmdRollup = new SqlCommand("sp_UpdateProgressForTask", con);
                    cmdRollup.CommandType = System.Data.CommandType.StoredProcedure;
                    cmdRollup.Parameters.AddWithValue("@TaskID", taskID);
                    cmdRollup.ExecuteNonQuery();

                    // Insert into TaskUpdates
                    SqlCommand cmdInsert = new SqlCommand(@"
                        INSERT INTO TaskUpdates (TaskID, UserID, OldStatus, NewStatus, Comment, UpdatedAt)
                        VALUES (@TaskID, @UserID, @OldStatus, @NewStatus, @Comment, GETDATE())", con);
                    cmdInsert.Parameters.AddWithValue("@TaskID", taskID);
                    cmdInsert.Parameters.AddWithValue("@UserID", Session["UserID"]);
                    cmdInsert.Parameters.AddWithValue("@OldStatus", oldStatus);
                    cmdInsert.Parameters.AddWithValue("@NewStatus", newStatus);
                    cmdInsert.Parameters.AddWithValue("@Comment", (object)txtStatusComment.Text.Trim() ?? DBNull.Value);
                    cmdInsert.ExecuteNonQuery();

                    // Create notifications for assigned users and team members
                    SqlCommand cmdNotif = new SqlCommand(@"
                        INSERT INTO Notifications (UserID, Title, Message, IsRead, NotificationType, RelatedID, RelatedType, CreatedAt)
                        SELECT DISTINCT AllUsers.UserID, @Title, @Message, 0, 'TaskUpdate', @TaskID, 'Task', GETDATE()
                        FROM (
                            SELECT ta.UserID FROM TaskAssignments ta WHERE ta.TaskID = @TaskID
                            UNION
                            SELECT ttm.UserID FROM TaskTeamMembers ttm
                            INNER JOIN TaskTeams tt ON ttm.TeamID = tt.TeamID
                            WHERE tt.TaskID = @TaskID AND ttm.Status = 'Accepted'
                        ) AllUsers
                        WHERE AllUsers.UserID != @CurrentUserID", con);
                    cmdNotif.Parameters.AddWithValue("@Title", "Task Status Updated");
                    cmdNotif.Parameters.AddWithValue("@Message", $"Task \"{Server.HtmlDecode(litTaskTitle.Text)}\" status changed from {GetStatusDisplayText(oldStatus)} to {GetStatusDisplayText(newStatus)}.");
                    cmdNotif.Parameters.AddWithValue("@TaskID", taskID);
                    cmdNotif.Parameters.AddWithValue("@CurrentUserID", Session["UserID"]);
                    cmdNotif.ExecuteNonQuery();
                }

                EventTaskService.SyncEventStatusForTask(taskID, Convert.ToInt32(Session["UserID"]));

                // Reload page
                ddlNewStatus.SelectedIndex = 0;
                txtStatusComment.Text = "";
                LoadTaskDetails();
                LoadTaskUpdates();
            }
            catch (Exception ex)
            {
                pnlError.Visible = true;
                lblError.Text = "An error occurred while updating the task: " + ex.Message;
            }
        }

        protected void btnUpload_Click(object sender, EventArgs e)
        {
            if (isAdmin)
            {
                pnlError.Visible = true;
                lblError.Text = "System admin cannot upload files on a task. Comment and tag the member instead.";
                return;
            }
            if (fuAttachment.HasFile)
            {
                try
                {
                    string fileName = Path.GetFileName(fuAttachment.FileName);
                    string fileExtension = Path.GetExtension(fileName).ToLower();
                    long fileSize = fuAttachment.FileContent.Length;

                    // Validate file size (10MB max)
                    if (fileSize > 10 * 1024 * 1024)
                    {
                        pnlError.Visible = true;
                        lblError.Text = "File size must be less than 10MB.";
                        return;
                    }

                    // Validate file type
                    string[] allowedTypes = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png", ".gif", ".txt", ".ppt", ".pptx" };
                    if (Array.IndexOf(allowedTypes, fileExtension) == -1)
                    {
                        pnlError.Visible = true;
                        lblError.Text = "File type not allowed. Supported: PDF, Word, Excel, Images, Text, PowerPoint.";
                        return;
                    }

                    // Save file
                    string uploadDir = Server.MapPath("~/Uploads/Tasks/");
                    if (!Directory.Exists(uploadDir))
                        Directory.CreateDirectory(uploadDir);

                    string uniqueFileName = $"{taskID}_{DateTime.Now:yyyyMMddHHmmss}_{fileName}";
                    string filePath = Path.Combine(uploadDir, uniqueFileName);
                    fuAttachment.SaveAs(filePath);

                    // Save to database
                    using (SqlConnection con = new SqlConnection(connectionString))
                    {
                        con.Open();
                        SqlCommand cmd = new SqlCommand(@"
                            INSERT INTO Attachments (FileName, FilePath, FileSize, FileType, UploadedBy, RelatedID, RelatedType, UploadedAt)
                            VALUES (@FileName, @FilePath, @FileSize, @FileType, @UploadedBy, @RelatedID, 'Task', GETDATE())", con);
                        cmd.Parameters.AddWithValue("@FileName", fileName);
                        cmd.Parameters.AddWithValue("@FilePath", $"~/Uploads/Tasks/{uniqueFileName}");
                        cmd.Parameters.AddWithValue("@FileSize", fileSize);
                        cmd.Parameters.AddWithValue("@FileType", fileExtension);
                        cmd.Parameters.AddWithValue("@UploadedBy", Session["UserID"]);
                        cmd.Parameters.AddWithValue("@RelatedID", taskID);
                        cmd.ExecuteNonQuery();
                    }

                    LoadAttachments();
                }
                catch (Exception ex)
                {
                    pnlError.Visible = true;
                    lblError.Text = "Error uploading file: " + ex.Message;
                }
            }
        }

        protected void rptAttachments_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int attachmentId;
            if (!int.TryParse(e.CommandArgument.ToString(), out attachmentId))
                return;

            switch (e.CommandName)
            {
                case "Download":
                    DownloadAttachment(attachmentId);
                    break;
                case "RemoveAttachment":
                    if (isAdmin)
                        return;
                    RemoveAttachment(attachmentId);
                    LoadAttachments();
                    break;
            }
        }

        private void DownloadAttachment(int attachmentId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    "SELECT FilePath, FileName FROM Attachments WHERE AttachmentID = @AttachmentID", con);
                cmd.Parameters.AddWithValue("@AttachmentID", attachmentId);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string filePath = reader["FilePath"].ToString();
                        string fileName = reader["FileName"].ToString();
                        string fullPath = Server.MapPath(filePath);

                        if (File.Exists(fullPath))
                        {
                            Response.Clear();
                            Response.ContentType = "application/octet-stream";
                            Response.AddHeader("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                            Response.TransmitFile(fullPath);
                            Response.End();
                        }
                    }
                }
            }
        }

        private void RemoveAttachment(int attachmentId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                // Get file path before deleting
                SqlCommand cmdGet = new SqlCommand(
                    "SELECT FilePath FROM Attachments WHERE AttachmentID = @AttachmentID", con);
                cmdGet.Parameters.AddWithValue("@AttachmentID", attachmentId);
                object result = cmdGet.ExecuteScalar();

                if (result != null)
                {
                    string fullPath = Server.MapPath(result.ToString());
                    if (File.Exists(fullPath))
                        File.Delete(fullPath);
                }

                SqlCommand cmd = new SqlCommand(
                    "DELETE FROM Attachments WHERE AttachmentID = @AttachmentID", con);
                cmd.Parameters.AddWithValue("@AttachmentID", attachmentId);
                cmd.ExecuteNonQuery();
            }
        }

        protected void rptAttachments_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
                return;
            var pnlRemove = e.Item.FindControl("pnlRemoveAttachment") as Panel;
            if (pnlRemove != null)
                pnlRemove.Visible = !isAdmin;
        }

        protected void btnAddComment_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNewComment.Text)) return;

            try
            {
                EventTaskService.EnsureCommentMentionSchema();
                int? mentionedUserId = null;
                int parsedMention;
                if (int.TryParse(ddlTagUser.SelectedValue, out parsedMention) && parsedMention > 0)
                {
                    if (!EventTaskService.IsValidCommentTarget(taskID, parsedMention))
                    {
                        pnlError.Visible = true;
                        lblError.Text = "Choose a member of this task or its event.";
                        return;
                    }
                    mentionedUserId = parsedMention;
                }

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand(@"
                        INSERT INTO TaskComments (TaskID, UserID, Comment, MentionedUserID, CreatedAt)
                        VALUES (@TaskID, @UserID, @Comment, @MentionedUserID, GETDATE())", con);
                    cmd.Parameters.AddWithValue("@TaskID", taskID);
                    cmd.Parameters.AddWithValue("@UserID", Session["UserID"]);
                    cmd.Parameters.AddWithValue("@Comment", txtNewComment.Text.Trim());
                    cmd.Parameters.AddWithValue("@MentionedUserID", (object)mentionedUserId ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }

                string title = mentionedUserId.HasValue ? "You were tagged on a task" : "New Comment on Task";
                string shortComment = txtNewComment.Text.Trim();
                if (shortComment.Length > 80) shortComment = shortComment.Substring(0, 80) + "...";
                string taskTitle = Server.HtmlDecode(litTaskTitle.Text);
                taskTitle = System.Text.RegularExpressions.Regex.Replace(taskTitle, "<.*?>", "");
                string message = mentionedUserId.HasValue
                    ? "You were tagged on \"" + taskTitle + "\": \"" + shortComment + "\""
                    : "New comment on task: \"" + shortComment + "\"";

                if (mentionedUserId.HasValue)
                {
                    if (mentionedUserId.Value != currentUserID)
                        NotificationService.Send(mentionedUserId.Value, title, message, "TaskComment", taskID, "Task");
                }
                else
                {
                    NotificationService.NotifyUsers(EventTaskService.ListAssigneeIds(taskID), currentUserID, title, message, "TaskComment", taskID, "Task");
                }

                txtNewComment.Text = "";
                ddlTagUser.SelectedIndex = 0;
                LoadComments();
            }
            catch (Exception ex)
            {
                pnlError.Visible = true;
                lblError.Text = "Error posting comment: " + ex.Message;
            }
        }

        protected void rptComments_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            // Reserved for future: edit/delete comments
        }

        protected void btnAddChecklistItem_Click(object sender, EventArgs e)
        {
            if (isAdmin) return;
            if (string.IsNullOrWhiteSpace(txtNewChecklistItem.Text)) return;

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand(@"
                        INSERT INTO TaskChecklist (TaskID, ItemText, IsCompleted, CreatedAt)
                        VALUES (@TaskID, @ItemText, 0, GETDATE())", con);
                    cmd.Parameters.AddWithValue("@TaskID", taskID);
                    cmd.Parameters.AddWithValue("@ItemText", txtNewChecklistItem.Text.Trim());
                    cmd.ExecuteNonQuery();
                }

                txtNewChecklistItem.Text = "";
                LoadChecklist();
            }
            catch (Exception ex)
            {
                pnlError.Visible = true;
                lblError.Text = "Error adding checklist item: " + ex.Message;
            }
        }

        protected void rptChecklist_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (isAdmin)
                return;

            int itemId;
            if (!int.TryParse(e.CommandArgument.ToString(), out itemId))
                return;

            switch (e.CommandName)
            {
                case "ToggleCheck":
                    ToggleChecklistItem(itemId);
                    LoadChecklist();
                    break;
                case "DeleteItem":
                    DeleteChecklistItem(itemId);
                    LoadChecklist();
                    break;
            }
        }

        protected void rptChecklist_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
                return;
            var toggle = e.Item.FindControl("btnToggleCheck") as LinkButton;
            var deletePanel = e.Item.FindControl("pnlDeleteChecklist") as Panel;
            if (isAdmin)
            {
                if (toggle != null)
                    toggle.Enabled = false;
                if (deletePanel != null)
                    deletePanel.Visible = false;
            }
        }

        private void ToggleChecklistItem(int itemId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    "UPDATE TaskChecklist SET IsCompleted = CASE WHEN IsCompleted = 1 THEN 0 ELSE 1 END WHERE ItemID = @ItemID", con);
                cmd.Parameters.AddWithValue("@ItemID", itemId);
                cmd.ExecuteNonQuery();
            }
        }

        private void DeleteChecklistItem(int itemId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    "DELETE FROM TaskChecklist WHERE ItemID = @ItemID", con);
                cmd.Parameters.AddWithValue("@ItemID", itemId);
                cmd.ExecuteNonQuery();
            }
        }

        private string GetStatusBadgeHtml(string status, object dueDate = null)
        {
            if (TimelineService.IsOverdue(dueDate, status))
                status = "Delayed";

            string bgColor, textColor, icon;

            switch (status)
            {
                case "Pending":
                    bgColor = "background-color: rgba(117,117,117,0.15);";
                    textColor = "color: #616161;";
                    icon = "pending";
                    break;
                case "InProgress":
                    bgColor = "background-color: rgba(230,126,0,0.15);";
                    textColor = "color: #e67e00;";
                    icon = "autorenew";
                    break;
                case "Submitted":
                    bgColor = "background-color: rgba(1,87,155,0.15);";
                    textColor = "color: #01579b;";
                    icon = "send";
                    break;
                case "UnderReview":
                    bgColor = "background-color: rgba(123,31,162,0.15);";
                    textColor = "color: #7b1fa2;";
                    icon = "rate_review";
                    break;
                case "RevisionNeeded":
                    bgColor = "background-color: rgba(198,40,40,0.15);";
                    textColor = "color: #c62828;";
                    icon = "replay";
                    break;
                case "Approved":
                    bgColor = "background-color: rgba(46,125,50,0.15);";
                    textColor = "color: #2e7d32;";
                    icon = "verified";
                    break;
                case "Completed":
                    bgColor = "background-color: rgba(46,125,50,0.15);";
                    textColor = "color: #2e7d32;";
                    icon = "check_circle";
                    break;
                case "Delayed":
                    bgColor = "background-color: rgba(198,40,40,0.15);";
                    textColor = "color: #c62828;";
                    icon = "warning";
                    break;
                default:
                    bgColor = "background-color: rgba(117,117,117,0.15);";
                    textColor = "color: #616161;";
                    icon = "circle";
                    break;
            }

            return $"<span class=\"inline-flex items-center gap-1 px-2.5 py-1 rounded-full font-badge-cap text-badge-cap\" style=\"{bgColor}{textColor}\">" +
                   $"<span class=\"material-symbols-outlined text-[14px]\">{icon}</span>{GetStatusDisplayText(status)}</span>";
        }

        private string GetPriorityBadgeHtml(string priority)
        {
            string bgColor, textColor, icon;

            switch (priority)
            {
                case "Critical":
                    bgColor = "background-color: rgba(198,40,40,0.15);";
                    textColor = "color: #c62828;";
                    icon = "priority_high";
                    break;
                case "High":
                    bgColor = "background-color: rgba(198,40,40,0.1);";
                    textColor = "color: #d32f2f;";
                    icon = "arrow_upward";
                    break;
                case "Medium":
                    bgColor = "background-color: rgba(230,126,0,0.15);";
                    textColor = "color: #e67e00;";
                    icon = "remove";
                    break;
                case "Low":
                    bgColor = "background-color: rgba(25,118,210,0.15);";
                    textColor = "color: #1976d2;";
                    icon = "arrow_downward";
                    break;
                default:
                    bgColor = "background-color: rgba(117,117,117,0.15);";
                    textColor = "color: #616161;";
                    icon = "circle";
                    break;
            }

            return $"<span class=\"inline-flex items-center gap-1 px-2.5 py-1 rounded-full font-badge-cap text-badge-cap\" style=\"{bgColor}{textColor}\">" +
                   $"<span class=\"material-symbols-outlined text-[14px]\">{icon}</span>{priority}</span>";
        }

        private string[] GetStatusUpdateVisuals(string status)
        {
            switch (status)
            {
                case "Pending":
                    return new[] { "background-color: #757575;", "color: white;", "pending" };
                case "InProgress":
                    return new[] { "background-color: #e67e00;", "color: white;", "autorenew" };
                case "Submitted":
                    return new[] { "background-color: #01579b;", "color: white;", "send" };
                case "UnderReview":
                    return new[] { "background-color: #7b1fa2;", "color: white;", "rate_review" };
                case "RevisionNeeded":
                    return new[] { "background-color: #c62828;", "color: white;", "replay" };
                case "Approved":
                    return new[] { "background-color: #2e7d32;", "color: white;", "verified" };
                case "Completed":
                    return new[] { "background-color: #2e7d32;", "color: white;", "check" };
                case "Delayed":
                    return new[] { "background-color: #c62828;", "color: white;", "warning" };
                default:
                    return new[] { "background-color: #757575;", "color: white;", "circle" };
            }
        }

        private string GetInlineStatusStyle(string status)
        {
            switch (status)
            {
                case "Pending":
                    return "background-color: rgba(117,117,117,0.12); color: #616161;";
                case "InProgress":
                    return "background-color: rgba(230,126,0,0.12); color: #e67e00;";
                case "Submitted":
                    return "background-color: rgba(1,87,155,0.12); color: #01579b;";
                case "UnderReview":
                    return "background-color: rgba(123,31,162,0.12); color: #7b1fa2;";
                case "RevisionNeeded":
                    return "background-color: rgba(198,40,40,0.12); color: #c62828;";
                case "Approved":
                    return "background-color: rgba(46,125,50,0.12); color: #2e7d32;";
                case "Completed":
                    return "background-color: rgba(46,125,50,0.12); color: #2e7d32;";
                case "Delayed":
                    return "background-color: rgba(198,40,40,0.12); color: #c62828;";
                default:
                    return "background-color: rgba(117,117,117,0.12); color: #616161;";
            }
        }

        private string GetStatusDisplayText(string status)
        {
            switch (status)
            {
                case "Pending": return "Pending";
                case "InProgress": return "In Progress";
                case "Submitted": return "Submitted";
                case "Completed": return "Completed";
                case "Delayed": return "Late";
                case "UnderReview": return "Under Review";
                case "RevisionNeeded": return "Revision Needed";
                case "Approved": return "Approved";
                default: return status;
            }
        }

        private string FormatFileSize(long bytes)
        {
            if (bytes < 1024) return bytes + " B";
            if (bytes < 1048576) return (bytes / 1024.0).ToString("0.#") + " KB";
            return (bytes / 1048576.0).ToString("0.#") + " MB";
        }

        protected void btnBackToList_Click(object sender, EventArgs e)
        {
            if (isAdmin)
                Response.Redirect("~/Modules/Tasks/Tasks.aspx");
            else
                Response.Redirect("~/Modules/UserTasks/UserTasks.aspx");
        }

        private void BindEventTaskStatuses()
        {
            if (!EventTaskService.IsEventLinked(taskID))
                return;

            int userId = Convert.ToInt32(Session["UserID"]);
            string role = Session["Role"] as string;
            ddlNewStatus.Items.Clear();
            ddlNewStatus.Items.Add(new ListItem("Select Status", ""));
            if (EventTaskService.CanReview(taskID, userId, role))
            {
                ddlNewStatus.Items.Add(new ListItem("Send back for changes", EventTaskService.RevisionNeeded));
                ddlNewStatus.Items.Add(new ListItem("Approve and complete", "Completed"));
            }
            else
            {
                ddlNewStatus.Items.Add(new ListItem("Not Started", "NotStarted"));
                ddlNewStatus.Items.Add(new ListItem("In Progress", "InProgress"));
                ddlNewStatus.Items.Add(new ListItem("Submit for review", EventTaskService.UnderReview));
            }
        }

        private bool IsLeaderOrMember(int userID)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    @"SELECT COUNT(1) FROM Tasks t
                      WHERE t.TaskID = @TaskID AND " + TaskAccess.UserCanSeeTask, con);
                cmd.Parameters.AddWithValue("@TaskID", taskID);
                cmd.Parameters.AddWithValue("@UserID", userID);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

    }
}
