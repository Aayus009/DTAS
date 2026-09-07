using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.TaskWorkspaces
{
    public partial class TaskWorkspace : System.Web.UI.Page
    {
        private string connectionString;
        private int taskId;
        private bool isLeader;
        private bool isEventReviewer;
        private bool isEventManager;
        private bool isViewOnly;

        protected void Page_Load(object sender, EventArgs e)
        {
            connectionString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;

            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Modules/Authentication/Login.aspx");
                return;
            }

            if (Request.QueryString["TaskID"] == null || !int.TryParse(Request.QueryString["TaskID"], out taskId))
            {
                Response.Redirect("~/Modules/TaskWorkspaces/TaskWorkspaces.aspx");
                return;
            }

            int? linkedEventId = EventTaskService.GetEventId(taskId);
            if (linkedEventId.HasValue)
            {
                Response.Redirect("~/Modules/Events/EventWorkspace.aspx?EventID=" + linkedEventId.Value + "#task-" + taskId);
                return;
            }

            int currentUserId = Convert.ToInt32(Session["UserID"]);
            isLeader = IsLeader(currentUserId);
            isEventReviewer = EventTaskService.CanReview(taskId, currentUserId, Session["Role"] as string);
            isEventManager = EventTaskService.CanAssign(taskId, currentUserId, Session["Role"] as string);

            bool canWork = CanAccessWorkspace(currentUserId);
            isViewOnly = !canWork && EventTaskService.IsAcceptedEventMember(taskId, currentUserId);
            if (!canWork && !isViewOnly)
            {
                pnlInternal.Visible = false;
                pnlAccessDenied.Visible = true;
                return;
            }

            pnlInternal.Visible = true;
            pnlAccessDenied.Visible = false;
            lnkScheduleMeeting.NavigateUrl = "~/Modules/UserMeetings/UserMeetings.aspx?TaskID=" + taskId;

            ApplyEventWorkflowChrome();

            if (!IsPostBack)
            {
                BindWorkerStatuses();
                BindAssigneeList();
                LoadTaskDetails();
                BindDeadlineRisk();
                LoadRoster();
                LoadUpdateHistory();
                LoadAttachments();
                LoadSharedComments();
            }
        }

        private bool CanAccessWorkspace(int userId)
        {
            if (RoleAccess.IsAdmin(Session["Role"] as string))
                return true;
            if (IsLeader(userId))
                return true;
            if (HasAcceptedMembership(userId))
                return true;
            if (HasTaskAssignment(userId))
                return true;
            return EventTaskService.IsEventOfficer(taskId, userId);
        }

        private bool HasAcceptedMembership(int userId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    @"SELECT COUNT(1) FROM TaskTeamMembers ttm
                      INNER JOIN TaskTeams tt ON ttm.TeamID = tt.TeamID
                      WHERE tt.TaskID = @TaskID AND ttm.UserID = @UserID AND ttm.Status = 'Accepted'", con);
                cmd.Parameters.AddWithValue("@TaskID", taskId);
                cmd.Parameters.AddWithValue("@UserID", userId);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private bool HasTaskAssignment(int userId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    "SELECT COUNT(1) FROM TaskAssignments WHERE TaskID = @TaskID AND UserID = @UserID", con);
                cmd.Parameters.AddWithValue("@TaskID", taskId);
                cmd.Parameters.AddWithValue("@UserID", userId);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private bool IsLeader(int userId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    "SELECT COUNT(1) FROM TaskTeams WHERE TaskID = @TaskID AND LeaderID = @UserID", con);
                cmd.Parameters.AddWithValue("@TaskID", taskId);
                cmd.Parameters.AddWithValue("@UserID", userId);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private void LoadTaskDetails()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    @"SELECT t.TaskTitle, t.Description, t.Priority, t.Status, t.DueDate,
                             ISNULL(t.PublicSummary, 'No public summary yet.') AS PublicSummary,
                             ISNULL(e.EventName, 'General') AS EventName
                      FROM Tasks t
                      LEFT JOIN Events e ON t.EventID = e.EventID
                      WHERE t.TaskID = @TaskID", con);
                cmd.Parameters.AddWithValue("@TaskID", taskId);

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    litTaskTitle.Text = Server.HtmlEncode(reader["TaskTitle"].ToString());
                    litDescription.Text = Server.HtmlEncode(reader["Description"].ToString());
                    object dueDate = reader["DueDate"];
                    string status = reader["Status"].ToString();
                    litDueDate.Text = TimelineService.DueLabel(dueDate, status);
                    litEvent.Text = Server.HtmlEncode(reader["EventName"].ToString());
                    litCurrentPublicSummary.Text = Server.HtmlEncode(reader["PublicSummary"].ToString());

                    string priority = reader["Priority"].ToString();
                    spanPriority.InnerText = priority;
                    spanPriority.Attributes["class"] = "badge-status-" + priority.ToLower();

                    spanStatus.InnerText = TimelineService.StatusLabel(status, dueDate);
                    spanStatus.Attributes["class"] = TimelineService.StatusBadgeClass(status, dueDate);

                    pnlPublicSummaryField.Visible = isLeader && !isEventReviewer;
                    ApplyEventWorkflowChrome(status);
                }
                else
                {
                    Response.Redirect("~/Modules/TaskWorkspaces/TaskWorkspaces.aspx");
                }
                reader.Close();
            }
        }

        protected void btnExtendDue_Click(object sender, EventArgs e)
        {
            DateTime due;
            if (!DateTime.TryParse(txtExtendDue.Text, out due))
            {
                ShowWorkflow("Enter a valid due date.");
                BindDeadlineRisk();
                return;
            }
            string error = DeadlineService.ExtendTaskDueDate(taskId, Convert.ToInt32(Session["UserID"]), Session["Role"] as string, due);
            ShowWorkflow(error ?? "Due date extended.");
            LoadTaskDetails();
            BindDeadlineRisk();
        }

        private void BindDeadlineRisk()
        {
            if (isViewOnly)
            {
                pnlDeadlineRisk.Visible = false;
                return;
            }
            DeadlineRisk risk = DeadlineService.GetTaskRisk(taskId);
            pnlDeadlineRisk.Visible = risk.ShowNearDueAlert;
            if (!risk.ShowNearDueAlert)
                return;
            spanRiskBadge.InnerText = risk.Severity;
            spanRiskBadge.Attributes["class"] = "inline-block px-2.5 py-1 rounded-full font-badge-cap text-badge-cap mb-2 " + risk.BadgeClass;
            litRiskTitle.Text = Server.HtmlEncode(risk.Title);
            litRiskMessage.Text = Server.HtmlEncode(risk.Message);
            bool canManage = DeadlineService.CanManageTaskDeadline(taskId, Convert.ToInt32(Session["UserID"]), Session["Role"] as string);
            pnlDeadlineActions.Visible = canManage && risk.ShowRecoveryActions;
            lnkRedistribute.NavigateUrl = "~/Modules/Tasks/TaskTeam.aspx?TaskID=" + taskId;
            txtExtendDue.Text = DateTime.Today.AddDays(risk.DaysRemaining >= 0 ? Math.Max(7, risk.DaysRemaining + 7) : 7).ToString("yyyy-MM-dd");
        }

        private void LoadRoster()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
                    @"SELECT u.FullName, ttm.Status, 
                             CASE WHEN tt.LeaderID = u.UserID THEN 1 ELSE 0 END AS IsLeader
                      FROM TaskTeams tt
                      INNER JOIN TaskTeamMembers ttm ON tt.TeamID = ttm.TeamID
                      INNER JOIN Users u ON ttm.UserID = u.UserID
                      WHERE tt.TaskID = @TaskID AND ttm.Status = 'Accepted'
                      ORDER BY CASE WHEN tt.LeaderID = u.UserID THEN 0 ELSE 1 END, u.FullName", con);
                cmd.Parameters.AddWithValue("@TaskID", taskId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    rptRoster.DataSource = dt;
                    rptRoster.DataBind();
                }
                else
                {
                    pnlNoRoster.Visible = true;
                    rptRoster.Visible = false;
                }
            }
        }

        protected string IsLeader(object isLeaderVal)
        {
            if (isLeaderVal != DBNull.Value && Convert.ToInt32(isLeaderVal) == 1)
            {
                return "<span class='inline-flex items-center gap-1 px-2 py-0.5 bg-[rgba(46,125,50,0.12)] text-[#2e7d32] rounded-full text-xs font-semibold'><span class='material-symbols-outlined text-[12px]'>star</span>Leader</span>";
            }
            return "";
        }

        private void LoadUpdateHistory()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
                    @"SELECT tu.UpdateID, tu.OldStatus, tu.NewStatus, tu.Comment, tu.UpdatedAt,
                             u.FullName AS UserName
                      FROM TaskUpdates tu
                      INNER JOIN Users u ON tu.UserID = u.UserID
                      WHERE tu.TaskID = @TaskID
                      ORDER BY tu.UpdatedAt DESC", con);

                SqlCommand attachCmd = new SqlCommand(
                    @"SELECT RelatedUpdateId FROM TaskUpdates WHERE TaskID = @TaskID", con);

                cmd.Parameters.AddWithValue("@TaskID", taskId);
                attachCmd.Parameters.AddWithValue("@TaskID", taskId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                // Attach attachments to each update row
                dt.Columns.Add("Attachments", typeof(object));

                SqlDataAdapter daAttach = new SqlDataAdapter(
                    @"SELECT a.AttachmentID, a.FileName, a.FilePath, a.RelatedID
                      FROM Attachments a
                      WHERE a.RelatedType = 'Task'", con);
                DataTable attachments = new DataTable();
                daAttach.Fill(attachments);

                foreach (DataRow row in dt.Rows)
                {
                    int updateId = Convert.ToInt32(row["UpdateID"]);
                    DataTable updateAttachments = new DataTable();
                    updateAttachments.Columns.Add("FileName", typeof(string));
                    updateAttachments.Columns.Add("FilePath", typeof(string));

                    // Attachments linked to this task are shown for all updates (current design keeps it simple).
                    // In a richer design, tie TaskUpdates to Attachments via RelatedID = UpdateID.
                    foreach (DataRow attRow in attachments.Rows)
                    {
                        if (Convert.ToInt32(attRow["RelatedID"]) == taskId)
                        {
                            updateAttachments.Rows.Add(attRow["FileName"].ToString(), attRow["FilePath"].ToString());
                        }
                    }
                    row["Attachments"] = updateAttachments;
                }

                if (dt.Rows.Count > 0)
                {
                    rptHistory.DataSource = dt;
                    rptHistory.DataBind();
                    pnlNoUpdates.Visible = false;
                    litUpdateCount.Text = dt.Rows.Count + " update" + (dt.Rows.Count != 1 ? "s" : "");
                }
                else
                {
                    rptHistory.Visible = false;
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
                    @"SELECT FileName, FilePath, UploadedAt FROM Attachments
                      WHERE RelatedID = @TaskID AND RelatedType = 'Task'
                      ORDER BY UploadedAt DESC", con);
                cmd.Parameters.AddWithValue("@TaskID", taskId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    rptAttachments.DataSource = dt;
                    rptAttachments.DataBind();
                }
                else
                {
                    pnlNoAttachments.Visible = true;
                    rptAttachments.Visible = false;
                }
            }
        }

        protected void btnSubmitUpdate_Click(object sender, EventArgs e)
        {
            int userId = Convert.ToInt32(Session["UserID"]);

            if (!CanAccessWorkspace(userId) || isViewOnly)
            {
                Response.Redirect("~/Modules/TaskWorkspaces/TaskWorkspaces.aspx");
                return;
            }

            if (string.IsNullOrEmpty(ddlStatus.SelectedValue))
            {
                ShowWorkflow("Please select a status.");
                return;
            }

            string newStatus = ddlStatus.SelectedValue;
            string comment = txtComment.Text.Trim();
            string blocked = EventTaskService.ValidateWorkerStatus(taskId, userId, Session["Role"] as string, newStatus);
            if (blocked != null)
            {
                ShowWorkflow(blocked);
                return;
            }

            if (string.Equals(newStatus, EventTaskService.UnderReview, StringComparison.OrdinalIgnoreCase)
                && comment.Length == 0)
            {
                ShowWorkflow("Add a short note for faculty before submitting for review.");
                return;
            }

            try
            {
                string oldStatus = "";
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    SqlCommand cmdOld = new SqlCommand("SELECT Status FROM Tasks WHERE TaskID = @TaskID", con);
                    cmdOld.Parameters.AddWithValue("@TaskID", taskId);
                    object oldResult = cmdOld.ExecuteScalar();
                    oldStatus = oldResult == null ? "" : oldResult.ToString();
                }

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    using (SqlTransaction transaction = con.BeginTransaction())
                    {
                        // Update task status
                        SqlCommand cmdUpdate = new SqlCommand(
                            "UPDATE Tasks SET Status = @Status, UpdatedAt = GETDATE() WHERE TaskID = @TaskID", con, transaction);
                        cmdUpdate.Parameters.AddWithValue("@Status", newStatus);
                        cmdUpdate.Parameters.AddWithValue("@TaskID", taskId);
                        cmdUpdate.ExecuteNonQuery();

                        // Update public summary if leader provided one
                        if (isLeader && !string.IsNullOrWhiteSpace(txtPublicSummary.Text))
                        {
                            SqlCommand cmdSummary = new SqlCommand(
                                "UPDATE Tasks SET PublicSummary = @Summary WHERE TaskID = @TaskID", con, transaction);
                            cmdSummary.Parameters.AddWithValue("@Summary", txtPublicSummary.Text.Trim());
                            cmdSummary.Parameters.AddWithValue("@TaskID", taskId);
                            cmdSummary.ExecuteNonQuery();
                        }

                        // Insert into TaskUpdates
                        SqlCommand cmdInsert = new SqlCommand(
                            @"INSERT INTO TaskUpdates (TaskID, UserID, OldStatus, NewStatus, Comment, UpdatedAt)
                              VALUES (@TaskID, @UserID, @OldStatus, @NewStatus, @Comment, GETDATE())", con, transaction);
                        cmdInsert.Parameters.AddWithValue("@TaskID", taskId);
                        cmdInsert.Parameters.AddWithValue("@UserID", userId);
                        cmdInsert.Parameters.AddWithValue("@OldStatus", oldStatus);
                        cmdInsert.Parameters.AddWithValue("@NewStatus", newStatus);
                        cmdInsert.Parameters.AddWithValue("@Comment", comment);
                        cmdInsert.ExecuteNonQuery();

                        // Handle file upload
                        if (fuEvidence.HasFile)
                        {
                            string fileName = Path.GetFileName(fuEvidence.FileName);
                            string uploadDir = Server.MapPath("~/Uploads/TaskEvidence/");
                            if (!Directory.Exists(uploadDir))
                                Directory.CreateDirectory(uploadDir);

                            string filePath = "~/Uploads/TaskEvidence/" + DateTime.Now.Ticks + "_" + fileName;
                            fuEvidence.SaveAs(Server.MapPath(filePath));

                            SqlCommand cmdAttach = new SqlCommand(
                                @"INSERT INTO Attachments (FileName, FilePath, FileType, UploadedBy, RelatedID, RelatedType, UploadedAt)
                                  VALUES (@FileName, @FilePath, @FileType, @UploadedBy, @RelatedID, 'Task', GETDATE())", con, transaction);
                            cmdAttach.Parameters.AddWithValue("@FileName", fileName);
                            cmdAttach.Parameters.AddWithValue("@FilePath", filePath);
                            cmdAttach.Parameters.AddWithValue("@FileType", Path.GetExtension(fileName));
                            cmdAttach.Parameters.AddWithValue("@UploadedBy", userId);
                            cmdAttach.Parameters.AddWithValue("@RelatedID", taskId);
                            cmdAttach.ExecuteNonQuery();
                        }

                        // Notify team members
                        SqlCommand cmdNotif = new SqlCommand(@"
                            INSERT INTO Notifications (UserID, Title, Message, IsRead, NotificationType, RelatedID, RelatedType, CreatedAt)
                            SELECT DISTINCT u.UserID, 'Task Status Updated', @Message, 0, 'TaskUpdate', @TaskID, 'Task', GETDATE()
                            FROM Users u
                            WHERE u.UserID IN (
                                SELECT ttm.UserID FROM TaskTeamMembers ttm
                                INNER JOIN TaskTeams tt ON ttm.TeamID = tt.TeamID
                                WHERE tt.TaskID = @TaskID AND ttm.Status = 'Accepted'
                            ) AND u.UserID != @CurrentUserID", con, transaction);
                        cmdNotif.Parameters.AddWithValue("@Message", "Task \"" + Server.HtmlDecode(litTaskTitle.Text) + "\" status changed to " + newStatus + ".");
                        cmdNotif.Parameters.AddWithValue("@TaskID", taskId);
                        cmdNotif.Parameters.AddWithValue("@CurrentUserID", userId);
                        cmdNotif.ExecuteNonQuery();

                        transaction.Commit();
                    }
                }

                EventTaskService.SyncEventStatusForTask(taskId, userId);

                ddlStatus.SelectedIndex = 0;
                txtComment.Text = "";
                txtPublicSummary.Text = "";
                LoadTaskDetails();
                LoadUpdateHistory();
                LoadAttachments();
                ShowWorkflow(string.Equals(newStatus, EventTaskService.UnderReview, StringComparison.OrdinalIgnoreCase)
                    ? "Submitted for faculty review."
                    : "Update posted successfully.");

                int? eventId = EventTaskService.GetEventId(taskId);
                if (eventId.HasValue && string.Equals(newStatus, EventTaskService.UnderReview, StringComparison.OrdinalIgnoreCase))
                {
                    EventTaskService.NotifyEventAdmins(
                        eventId.Value,
                        userId,
                        "Task ready for review",
                        "A member submitted \"" + Server.HtmlDecode(litTaskTitle.Text) + "\" for review.",
                        taskId);
                }
            }
            catch (Exception ex)
            {
                ShowWorkflow("Error posting update: " + ex.Message);
            }
        }

        protected void btnApprove_Click(object sender, EventArgs e)
        {
            PostReview(true);
        }

        protected void btnRequestChanges_Click(object sender, EventArgs e)
        {
            PostReview(false);
        }

        protected void btnSaveAssignees_Click(object sender, EventArgs e)
        {
            var ids = new List<int>();
            foreach (ListItem item in cblAssignees.Items)
            {
                int memberId;
                if (item.Selected && int.TryParse(item.Value, out memberId))
                    ids.Add(memberId);
            }

            string error = EventTaskService.SetAssignees(taskId, Convert.ToInt32(Session["UserID"]), Session["Role"] as string, ids);
            ShowWorkflow(error ?? "Task members updated.");
            LoadRoster();
            BindAssigneeList();
        }

        protected void btnAddSharedComment_Click(object sender, EventArgs e)
        {
            int? eventId = EventTaskService.GetEventId(taskId);
            if (!eventId.HasValue)
                return;

            string error = EventTaskService.AddComment(
                eventId.Value,
                taskId,
                Convert.ToInt32(Session["UserID"]),
                Session["Role"] as string,
                txtSharedComment.Text);
            ShowWorkflow(error ?? "Comment posted.");
            if (error == null)
                txtSharedComment.Text = "";
            LoadSharedComments();
        }

        private void LoadSharedComments()
        {
            if (!EventTaskService.IsEventLinked(taskId))
                return;

            DataTable comments = EventTaskService.ListComments(taskId);
            rptSharedComments.DataSource = comments;
            rptSharedComments.DataBind();
            pnlNoSharedComments.Visible = comments.Rows.Count == 0;
        }

        private void PostReview(bool approve)
        {
            string result = EventTaskService.Review(
                taskId,
                Convert.ToInt32(Session["UserID"]),
                Session["Role"] as string,
                approve,
                txtReviewComment.Text);
            if (result == "approved-concluded")
                ShowWorkflow("Approved. All event tasks are done, so the event is now concluded.");
            else if (result != null)
                ShowWorkflow(result);
            else
                ShowWorkflow(approve ? "Task approved." : "Changes requested. The member can continue the work.");
            txtReviewComment.Text = "";
            LoadTaskDetails();
            LoadUpdateHistory();
        }

        private void ShowWorkflow(string message)
        {
            lblWorkflowMessage.Text = message ?? "";
            lblWorkflowMessage.Visible = !string.IsNullOrEmpty(message);
            lblSubmitMessage.Text = message ?? "";
            lblSubmitMessage.Visible = pnlWorkerUpdate.Visible && !string.IsNullOrEmpty(message);
        }

        private void ApplyEventWorkflowChrome()
        {
            ApplyEventWorkflowChrome(null);
        }

        private void ApplyEventWorkflowChrome(string status)
        {
            bool eventTask = EventTaskService.IsEventLinked(taskId);
            bool closed = string.Equals(status, "Completed", StringComparison.OrdinalIgnoreCase);
            pnlViewOnly.Visible = isViewOnly;
            pnlSharedComments.Visible = EventTaskService.IsEventLinked(taskId)
                && (EventTaskService.IsAcceptedEventMember(taskId, Convert.ToInt32(Session["UserID"]))
                    || RoleAccess.IsAdmin(Session["Role"] as string));
            lnkScheduleMeeting.Visible = !isViewOnly;
            pnlWorkerUpdate.Visible = !isViewOnly && (eventTask ? (!isEventReviewer && !closed) : true);
            pnlReview.Visible = !isViewOnly && eventTask && isEventReviewer && !closed;
            pnlAssign.Visible = !isViewOnly && eventTask && isEventManager && !closed;
        }

        private void BindWorkerStatuses()
        {
            if (!EventTaskService.IsEventLinked(taskId) || isEventReviewer)
                return;

            ddlStatus.Items.Clear();
            ddlStatus.Items.Add(new ListItem("Select status...", ""));
            ddlStatus.Items.Add(new ListItem("Not started", "NotStarted"));
            ddlStatus.Items.Add(new ListItem("In progress", "InProgress"));
            ddlStatus.Items.Add(new ListItem("Submit for review", EventTaskService.UnderReview));
        }

        private void BindAssigneeList()
        {
            if (!isEventManager)
                return;

            int? eventId = EventTaskService.GetEventId(taskId);
            if (!eventId.HasValue)
                return;

            var current = new HashSet<int>(EventTaskService.ListAssigneeIds(taskId));
            cblAssignees.Items.Clear();
            foreach (DataRow row in EventTaskService.ListAssignableMembers(eventId.Value).Rows)
            {
                int memberId = Convert.ToInt32(row["UserID"]);
                var item = new ListItem(
                    EventTaskService.FormatMemberLabel(row["FullName"], row["Username"], row["Email"]),
                    memberId.ToString());
                item.Selected = current.Contains(memberId);
                cblAssignees.Items.Add(item);
            }
        }

        protected string GetTimelineDotClass(string status)
        {
            switch (status?.ToLower())
            {
                case "completed": return "dot-tertiary";
                case "inprogress": return "dot-primary";
                case "delayed": return "dot-error";
                case "pending":
                case "notstarted": return "dot-outline";
                default: return "dot-secondary";
            }
        }
    }
}
