using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Tasks
{
    public partial class EditTask : System.Web.UI.Page
    {
        private string connectionString;
        private int taskID;

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
                Response.Redirect("~/Modules/UserTasks/UserTasks.aspx");
                return;
            }

            if (!RestrictionService.CanEditTask(taskID, Convert.ToInt32(Session["UserID"]), Session["Role"] as string))
            {
                Response.Redirect("~/Modules/UserTasks/UserTasks.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadRelatedDecisions();
                LoadRelatedEvents();
                LoadRelatedMeetings();
                LoadUsers();
                LoadLeaders();
                LoadTask();
            }
        }

        private void LoadTask()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    @"SELECT TaskTitle, Description, Priority, Status, DueDate,
                             DecisionID, EventID, MeetingID, LeaderID
                      FROM Tasks WHERE TaskID = @TaskID", con);
                cmd.Parameters.AddWithValue("@TaskID", taskID);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        txtTaskTitle.Text = reader["TaskTitle"].ToString();
                        txtDescription.Text = reader["Description"].ToString();

                        if (ddlPriority.Items.FindByValue(reader["Priority"].ToString()) != null)
                            ddlPriority.SelectedValue = reader["Priority"].ToString();

                        if (ddlStatus.Items.FindByValue(reader["Status"].ToString()) != null)
                            ddlStatus.SelectedValue = reader["Status"].ToString();

                        if (reader["DueDate"] != DBNull.Value)
                            txtDueDate.Text = Convert.ToDateTime(reader["DueDate"]).ToString("yyyy-MM-ddTHH:mm");

                        if (reader["DecisionID"] != DBNull.Value)
                        {
                            string decisionId = reader["DecisionID"].ToString();
                            if (ddlRelatedDecision.Items.FindByValue(decisionId) != null)
                                ddlRelatedDecision.SelectedValue = decisionId;
                        }

                        if (reader["EventID"] != DBNull.Value)
                        {
                            string eventId = reader["EventID"].ToString();
                            if (ddlRelatedEvent.Items.FindByValue(eventId) != null)
                                ddlRelatedEvent.SelectedValue = eventId;
                        }

                        if (reader["MeetingID"] != DBNull.Value)
                        {
                            string meetingId = reader["MeetingID"].ToString();
                            if (ddlRelatedMeeting.Items.FindByValue(meetingId) != null)
                                ddlRelatedMeeting.SelectedValue = meetingId;
                        }

                        if (reader["LeaderID"] != DBNull.Value)
                        {
                            string leaderId = reader["LeaderID"].ToString();
                            if (ddlLeader.Items.FindByValue(leaderId) != null)
                                ddlLeader.SelectedValue = leaderId;
                        }
                    }
                    else
                    {
                        Response.Redirect("~/Modules/Tasks/Tasks.aspx");
                    }
                }
            }

            LoadCurrentAssignments();
        }

        private void LoadCurrentAssignments()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
                    "SELECT UserID FROM TaskAssignments WHERE TaskID = @TaskID", con);
                cmd.Parameters.AddWithValue("@TaskID", taskID);
                con.Open();

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int userId = Convert.ToInt32(reader["UserID"]);
                    foreach (System.Web.UI.WebControls.ListItem item in cblAssignUsers.Items)
                    {
                        if (item.Value == userId.ToString())
                        {
                            item.Selected = true;
                            break;
                        }
                    }
                }
                reader.Close();
            }
        }

        private void LoadRelatedDecisions()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
                    "SELECT DecisionID, DecisionTitle FROM Decisions WHERE Status != 'Archived' ORDER BY DecisionTitle", con);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                ddlRelatedDecision.DataSource = reader;
                ddlRelatedDecision.DataTextField = "DecisionTitle";
                ddlRelatedDecision.DataValueField = "DecisionID";
                ddlRelatedDecision.DataBind();
                reader.Close();
            }
            ddlRelatedDecision.Items.Insert(0, new System.Web.UI.WebControls.ListItem("None", ""));
        }

        private void LoadRelatedEvents()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
                    "SELECT EventID, EventName FROM Events WHERE Status != 'Archived' ORDER BY EventName", con);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                ddlRelatedEvent.DataSource = reader;
                ddlRelatedEvent.DataTextField = "EventName";
                ddlRelatedEvent.DataValueField = "EventID";
                ddlRelatedEvent.DataBind();
                reader.Close();
            }
            ddlRelatedEvent.Items.Insert(0, new System.Web.UI.WebControls.ListItem("None", ""));
        }

        private void LoadRelatedMeetings()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
                    "SELECT MeetingID, MeetingTitle FROM Meetings WHERE Status != 'Archived' ORDER BY MeetingTitle", con);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                ddlRelatedMeeting.DataSource = reader;
                ddlRelatedMeeting.DataTextField = "MeetingTitle";
                ddlRelatedMeeting.DataValueField = "MeetingID";
                ddlRelatedMeeting.DataBind();
                reader.Close();
            }
            ddlRelatedMeeting.Items.Insert(0, new System.Web.UI.WebControls.ListItem("None", ""));
        }

        private void LoadUsers()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
                    @"SELECT u.UserID, u.FullName + ' (' + r.RoleName + ')' AS DisplayName 
                      FROM Users u
                      INNER JOIN Roles r ON u.RoleID = r.RoleID
                      WHERE u.IsActive = 1 
                      ORDER BY u.FullName", con);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                cblAssignUsers.DataSource = reader;
                cblAssignUsers.DataTextField = "DisplayName";
                cblAssignUsers.DataValueField = "UserID";
                cblAssignUsers.DataBind();
                reader.Close();
            }

            if (cblAssignUsers.Items.Count == 0)
            {
                cblAssignUsers.Visible = false;
                pnlNoUsers.Visible = true;
            }
            else
            {
                cblAssignUsers.Visible = true;
                pnlNoUsers.Visible = false;
            }
        }

        private void LoadLeaders()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
                    @"SELECT u.UserID, u.FullName + ' (' + r.RoleName + ')' AS DisplayName 
                      FROM Users u
                      INNER JOIN Roles r ON u.RoleID = r.RoleID
                      WHERE u.IsActive = 1 
                      ORDER BY u.FullName", con);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                ddlLeader.DataSource = reader;
                ddlLeader.DataTextField = "DisplayName";
                ddlLeader.DataValueField = "UserID";
                ddlLeader.DataBind();
                reader.Close();
            }
            ddlLeader.Items.Insert(0, new System.Web.UI.WebControls.ListItem("No Leader (Individual Task)", ""));
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    int currentLeaderID = 0;
                    SqlCommand cmdGetLeader = new SqlCommand("SELECT ISNULL(LeaderID, 0) FROM Tasks WHERE TaskID = @TaskID", con);
                    cmdGetLeader.Parameters.AddWithValue("@TaskID", taskID);
                    object leaderResult = cmdGetLeader.ExecuteScalar();
                    if (leaderResult != null && leaderResult != DBNull.Value)
                        currentLeaderID = Convert.ToInt32(leaderResult);

                    SqlCommand cmd = new SqlCommand(@"
                        UPDATE Tasks SET
                            TaskTitle = @TaskTitle,
                            Description = @Description,
                            Priority = @Priority,
                            Status = @Status,
                            DueDate = @DueDate,
                            DecisionID = @DecisionID,
                            EventID = @EventID,
                            MeetingID = @MeetingID,
                            LeaderID = @LeaderID,
                            UpdatedAt = GETDATE()
                        WHERE TaskID = @TaskID", con);

                    cmd.Parameters.AddWithValue("@TaskID", taskID);
                    cmd.Parameters.AddWithValue("@TaskTitle", txtTaskTitle.Text.Trim());
                    cmd.Parameters.AddWithValue("@Description", (object)txtDescription.Text.Trim() ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Priority", ddlPriority.SelectedValue);
                    cmd.Parameters.AddWithValue("@Status", ddlStatus.SelectedValue);
                    cmd.Parameters.AddWithValue("@DueDate", DateTime.Parse(txtDueDate.Text));

                    if (!string.IsNullOrWhiteSpace(ddlRelatedDecision.SelectedValue))
                        cmd.Parameters.AddWithValue("@DecisionID", int.Parse(ddlRelatedDecision.SelectedValue));
                    else
                        cmd.Parameters.AddWithValue("@DecisionID", DBNull.Value);

                    if (!string.IsNullOrWhiteSpace(ddlRelatedEvent.SelectedValue))
                        cmd.Parameters.AddWithValue("@EventID", int.Parse(ddlRelatedEvent.SelectedValue));
                    else
                        cmd.Parameters.AddWithValue("@EventID", DBNull.Value);

                    if (!string.IsNullOrWhiteSpace(ddlRelatedMeeting.SelectedValue))
                        cmd.Parameters.AddWithValue("@MeetingID", int.Parse(ddlRelatedMeeting.SelectedValue));
                    else
                        cmd.Parameters.AddWithValue("@MeetingID", DBNull.Value);

                    int newLeaderID = 0;
                    if (!string.IsNullOrWhiteSpace(ddlLeader.SelectedValue))
                        newLeaderID = int.Parse(ddlLeader.SelectedValue);

                    cmd.Parameters.AddWithValue("@LeaderID", newLeaderID > 0 ? (object)newLeaderID : DBNull.Value);
                    cmd.ExecuteNonQuery();

                    EventTaskService.SyncEventStatusForTask(taskID, Convert.ToInt32(Session["UserID"]));

                    if (currentLeaderID != newLeaderID)
                    {
                        if (currentLeaderID > 0)
                        {
                            SqlCommand cmdDeleteTeam = new SqlCommand(
                                "DELETE FROM TaskTeams WHERE TaskID = @TaskID", con);
                            cmdDeleteTeam.Parameters.AddWithValue("@TaskID", taskID);
                            cmdDeleteTeam.ExecuteNonQuery();
                        }

                        if (newLeaderID > 0)
                        {
                            SqlCommand cmdCreateTeam = new SqlCommand(
                                @"INSERT INTO TaskTeams (TaskID, LeaderID, ProgressPercent, CreatedAt)
                                  VALUES (@TaskID, @LeaderID, 0, GETDATE())", con);
                            cmdCreateTeam.Parameters.AddWithValue("@TaskID", taskID);
                            cmdCreateTeam.Parameters.AddWithValue("@LeaderID", newLeaderID);
                            cmdCreateTeam.ExecuteNonQuery();

                            SqlCommand cmdAutoMember = new SqlCommand(
                                @"INSERT INTO TaskTeamMembers (TeamID, UserID, Status, InvitedAt, RespondedAt)
                                  SELECT tt.TeamID, @UserID, 'Accepted', GETDATE(), GETDATE()
                                  FROM TaskTeams tt WHERE tt.TaskID = @TaskID", con);
                            cmdAutoMember.Parameters.AddWithValue("@TaskID", taskID);
                            cmdAutoMember.Parameters.AddWithValue("@UserID", newLeaderID);
                            cmdAutoMember.ExecuteNonQuery();

                            SqlCommand cmdNotif = new SqlCommand(
                                @"INSERT INTO Notifications (UserID, Title, Message, IsRead, NotificationType, RelatedID, RelatedType, CreatedAt)
                                  VALUES (@UserID, @Title, @Message, 0, 'TaskAssignment', @TaskID, 'Task', GETDATE())", con);
                            cmdNotif.Parameters.AddWithValue("@UserID", newLeaderID);
                            cmdNotif.Parameters.AddWithValue("@Title", "You are the Team Leader");
                            cmdNotif.Parameters.AddWithValue("@Message", $"You have been assigned as the team leader for task: {txtTaskTitle.Text.Trim()}. Go to Task Details to manage your team.");
                            cmdNotif.Parameters.AddWithValue("@TaskID", taskID);
                            cmdNotif.ExecuteNonQuery();
                        }

                        SqlCommand cmdAudit = new SqlCommand(
                            @"INSERT INTO LeadershipHistory (TaskID, PreviousLeaderID, NewLeaderID, ChangedBy, ChangedAt)
                              VALUES (@TaskID, @PrevLeader, @NewLeader, @ChangedBy, GETDATE())", con);
                        cmdAudit.Parameters.AddWithValue("@TaskID", taskID);
                        cmdAudit.Parameters.AddWithValue("@PrevLeader", currentLeaderID > 0 ? (object)currentLeaderID : DBNull.Value);
                        cmdAudit.Parameters.AddWithValue("@NewLeader", newLeaderID > 0 ? (object)newLeaderID : DBNull.Value);
                        cmdAudit.Parameters.AddWithValue("@ChangedBy", Session["UserID"]);
                        cmdAudit.ExecuteNonQuery();
                    }

                    SqlCommand cmdDeleteAssign = new SqlCommand(
                        "DELETE FROM TaskAssignments WHERE TaskID = @TaskID", con);
                    cmdDeleteAssign.Parameters.AddWithValue("@TaskID", taskID);
                    cmdDeleteAssign.ExecuteNonQuery();

                    foreach (System.Web.UI.WebControls.ListItem item in cblAssignUsers.Items)
                    {
                        if (item.Selected)
                        {
                            int userId = int.Parse(item.Value);

                            SqlCommand cmdAssignment = new SqlCommand(@"
                                INSERT INTO TaskAssignments (TaskID, UserID, AssignedAt)
                                VALUES (@TaskID, @UserID, GETDATE())", con);
                            cmdAssignment.Parameters.AddWithValue("@TaskID", taskID);
                            cmdAssignment.Parameters.AddWithValue("@UserID", userId);
                            cmdAssignment.ExecuteNonQuery();

                            SqlCommand cmdCheckNotif = new SqlCommand(@"
                                SELECT COUNT(*) FROM Notifications 
                                WHERE UserID = @UserID AND RelatedID = @TaskID AND RelatedType = 'Task' AND NotificationType = 'TaskAssignment'", con);
                            cmdCheckNotif.Parameters.AddWithValue("@UserID", userId);
                            cmdCheckNotif.Parameters.AddWithValue("@TaskID", taskID);
                            int existingCount = (int)cmdCheckNotif.ExecuteScalar();

                            if (existingCount == 0)
                            {
                                SqlCommand cmdNotification = new SqlCommand(@"
                                    INSERT INTO Notifications (UserID, Title, Message, IsRead, NotificationType, RelatedID, RelatedType, CreatedAt)
                                    VALUES (@UserID, @Title, @Message, 0, 'TaskAssignment', @RelatedID, 'Task', GETDATE())", con);
                                cmdNotification.Parameters.AddWithValue("@UserID", userId);
                                cmdNotification.Parameters.AddWithValue("@Title", "Task Updated");
                                cmdNotification.Parameters.AddWithValue("@Message", $"A task you are assigned to has been updated: {txtTaskTitle.Text.Trim()}");
                                cmdNotification.Parameters.AddWithValue("@RelatedID", taskID);
                                cmdNotification.ExecuteNonQuery();
                            }
                        }
                    }
                }

                Response.Redirect($"~/Modules/Tasks/TaskDetails.aspx?TaskID={taskID}");
            }
            catch (Exception ex)
            {
                pnlError.Visible = true;
                lblError.Text = "An error occurred while updating the task: " + ex.Message;
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect($"~/Modules/Tasks/TaskDetails.aspx?TaskID={taskID}");
        }

    }
}
