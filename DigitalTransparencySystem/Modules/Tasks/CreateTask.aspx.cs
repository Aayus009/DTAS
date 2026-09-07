using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Tasks
{
    public partial class CreateTask : System.Web.UI.Page
    {
        private string connectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            connectionString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;

            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Modules/Authentication/Login.aspx");
                return;
            }

            string role = Session["Role"] as string;
            if (role == null || !role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("~/Modules/Authentication/Login.aspx");
                return;
            }

            Response.Redirect("~/Modules/Tasks/Tasks.aspx");
            return;

            if (!IsPostBack)
            {
                LoadRelatedDecisions();
                LoadRelatedEvents();
                LoadRelatedMeetings();
                LoadClubs();
                LoadUsers();
                LoadLeaders();
                ApplyClubRoster();
            }
        }

        private void LoadClubs()
        {
            RestrictionService.EnsureSchema();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
                    @"SELECT ClubID, ClubName FROM Clubs
                      WHERE IsActive = 1 AND ISNULL(IsDeleted, 0) = 0 AND ISNULL(IsRestricted, 0) = 0
                      ORDER BY ClubName", con);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                ddlClub.DataSource = reader;
                ddlClub.DataTextField = "ClubName";
                ddlClub.DataValueField = "ClubID";
                ddlClub.DataBind();
                reader.Close();
            }
            ddlClub.Items.Insert(0, new System.Web.UI.WebControls.ListItem("- No Club -", ""));
        }

        protected void ddlClub_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Rebind the user lists after the postback, then apply the club roster
            LoadUsers();
            LoadLeaders();
            ApplyClubRoster();
        }

        private void ApplyClubRoster()
        {
            if (string.IsNullOrEmpty(ddlClub.SelectedValue) || ddlClub.SelectedValue == "0")
                return;

            ClubService.EnsureMembershipSchema();

            int clubId = int.Parse(ddlClub.SelectedValue);
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                SqlCommand cmdLead = new SqlCommand("SELECT LeadUserID FROM Clubs WHERE ClubID = @ClubID", con);
                cmdLead.Parameters.AddWithValue("@ClubID", clubId);
                object lead = cmdLead.ExecuteScalar();

                SqlCommand cmdMembers = new SqlCommand(
                    @"SELECT UserID FROM ClubMembers
                      WHERE ClubID = @ClubID
                        AND ISNULL(IsActive, 1) = 1 AND ISNULL(InviteStatus, N'Accepted') = N'Accepted'", con);
                cmdMembers.Parameters.AddWithValue("@ClubID", clubId);
                SqlDataReader reader = cmdMembers.ExecuteReader();
                var memberIds = new System.Collections.Generic.HashSet<string>();
                while (reader.Read())
                    memberIds.Add(reader["UserID"].ToString());
                reader.Close();

                if (lead != null && lead != DBNull.Value)
                {
                    System.Web.UI.WebControls.ListItem leadItem = ddlLeader.Items.FindByValue(lead.ToString());
                    if (leadItem != null)
                        ddlLeader.SelectedValue = lead.ToString();
                }

                foreach (System.Web.UI.WebControls.ListItem item in cblAssignUsers.Items)
                {
                    if (memberIds.Contains(item.Value))
                        item.Selected = true;
                }
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

        protected void btnCreate_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            try
            {
                int newTaskId;

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    SqlCommand cmd = new SqlCommand(@"
                        INSERT INTO Tasks 
                            (TaskTitle, Description, Priority, Status, DueDate, DecisionID, EventID, MeetingID, CreatedBy, CreatedAt, UpdatedAt)
                        VALUES 
                            (@TaskTitle, @Description, @Priority, 'Pending', @DueDate, @DecisionID, @EventID, @MeetingID, @CreatedBy, GETDATE(), GETDATE());
                        SELECT SCOPE_IDENTITY();", con);

                    cmd.Parameters.AddWithValue("@TaskTitle", txtTaskTitle.Text.Trim());
                    cmd.Parameters.AddWithValue("@Description", (object)txtDescription.Text.Trim() ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Priority", ddlPriority.SelectedValue);
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

                    cmd.Parameters.AddWithValue("@CreatedBy", Session["UserID"]);

                    newTaskId = Convert.ToInt32(cmd.ExecuteScalar());

                    int leaderID = 0;
                    if (!string.IsNullOrWhiteSpace(ddlLeader.SelectedValue))
                    {
                        leaderID = int.Parse(ddlLeader.SelectedValue);

                        SqlCommand cmdUpdateLeader = new SqlCommand(
                            "UPDATE Tasks SET LeaderID = @LeaderID WHERE TaskID = @TaskID", con);
                        cmdUpdateLeader.Parameters.AddWithValue("@LeaderID", leaderID);
                        cmdUpdateLeader.Parameters.AddWithValue("@TaskID", newTaskId);
                        cmdUpdateLeader.ExecuteNonQuery();

                        SqlCommand cmdTeam = new SqlCommand(
                            @"INSERT INTO TaskTeams (TaskID, LeaderID, ProgressPercent, CreatedAt)
                              VALUES (@TaskID, @LeaderID, 0, GETDATE())", con);
                        cmdTeam.Parameters.AddWithValue("@TaskID", newTaskId);
                        cmdTeam.Parameters.AddWithValue("@LeaderID", leaderID);
                        cmdTeam.ExecuteNonQuery();

                        SqlCommand cmdTeamMember = new SqlCommand(
                            @"INSERT INTO TaskTeamMembers (TeamID, UserID, Status, InvitedAt, RespondedAt)
                              SELECT tt.TeamID, @UserID, 'Invited', GETDATE(), NULL
                              FROM TaskTeams tt WHERE tt.TaskID = @TaskID", con);
                        cmdTeamMember.Parameters.AddWithValue("@TaskID", newTaskId);
                        cmdTeamMember.Parameters.AddWithValue("@UserID", leaderID);
                        cmdTeamMember.ExecuteNonQuery();

                        SqlCommand cmdNotifLeader = new SqlCommand(
                            @"INSERT INTO Notifications (UserID, Title, Message, IsRead, NotificationType, RelatedID, RelatedType, CreatedAt)
                              VALUES (@UserID, @Title, @Message, 0, 'TaskTeamInvite', @RelatedID, 'Task', GETDATE())", con);
                        cmdNotifLeader.Parameters.AddWithValue("@UserID", leaderID);
                        cmdNotifLeader.Parameters.AddWithValue("@Title", "Task Team Invitation");
                        cmdNotifLeader.Parameters.AddWithValue("@Message", $"You have been invited to join the team for task: {txtTaskTitle.Text.Trim()} as Leader. Accept to manage your team.");
                        cmdNotifLeader.Parameters.AddWithValue("@RelatedID", newTaskId);
                        cmdNotifLeader.ExecuteNonQuery();
                    }

                    foreach (System.Web.UI.WebControls.ListItem item in cblAssignUsers.Items)
                    {
                        if (item.Selected)
                        {
                            int userId = int.Parse(item.Value);

                            SqlCommand cmdAssignment = new SqlCommand(@"
                                INSERT INTO TaskAssignments (TaskID, UserID, AssignedAt)
                                VALUES (@TaskID, @UserID, GETDATE())", con);
                            cmdAssignment.Parameters.AddWithValue("@TaskID", newTaskId);
                            cmdAssignment.Parameters.AddWithValue("@UserID", userId);
                            cmdAssignment.ExecuteNonQuery();

                            bool hasTeam = false;
                            SqlCommand cmdHasTeam = new SqlCommand(
                                "SELECT COUNT(*) FROM TaskTeams WHERE TaskID = @TaskID", con);
                            cmdHasTeam.Parameters.AddWithValue("@TaskID", newTaskId);
                            object teamCount = cmdHasTeam.ExecuteScalar();
                            hasTeam = (teamCount != null && Convert.ToInt32(teamCount) > 0);

                            if (hasTeam && userId != leaderID)
                            {
                                SqlCommand cmdTeamMember = new SqlCommand(@"
                                    INSERT INTO TaskTeamMembers (TeamID, UserID, Status, InvitedAt, RespondedAt)
                                    SELECT tt.TeamID, @UserID, 'Invited', GETDATE(), NULL
                                    FROM TaskTeams tt WHERE tt.TaskID = @TaskID", con);
                                cmdTeamMember.Parameters.AddWithValue("@TaskID", newTaskId);
                                cmdTeamMember.Parameters.AddWithValue("@UserID", userId);
                                cmdTeamMember.ExecuteNonQuery();

                                SqlCommand cmdInvite = new SqlCommand(@"
                                    INSERT INTO Notifications (UserID, Title, Message, IsRead, NotificationType, RelatedID, RelatedType, CreatedAt)
                                    VALUES (@UserID, @Title, @Message, 0, 'TaskTeamInvite', @RelatedID, 'Task', GETDATE())", con);
                                cmdInvite.Parameters.AddWithValue("@UserID", userId);
                                cmdInvite.Parameters.AddWithValue("@Title", "Task Team Invitation");
                                cmdInvite.Parameters.AddWithValue("@Message", $"You have been invited to join the team for task: {txtTaskTitle.Text.Trim()} as Member. Accept to join the workspace.");
                                cmdInvite.Parameters.AddWithValue("@RelatedID", newTaskId);
                                cmdInvite.ExecuteNonQuery();
                            }
                            else
                            {
                                SqlCommand cmdNotification = new SqlCommand(@"
                                    INSERT INTO Notifications (UserID, Title, Message, IsRead, NotificationType, RelatedID, RelatedType, CreatedAt)
                                    VALUES (@UserID, @Title, @Message, 0, 'TaskAssignment', @RelatedID, 'Task', GETDATE())", con);
                                cmdNotification.Parameters.AddWithValue("@UserID", userId);
                                cmdNotification.Parameters.AddWithValue("@Title", "New Task Assigned");
                                cmdNotification.Parameters.AddWithValue("@Message", $"You have been assigned a new task: {txtTaskTitle.Text.Trim()}");
                                cmdNotification.Parameters.AddWithValue("@RelatedID", newTaskId);
                                cmdNotification.ExecuteNonQuery();
                            }
                        }
                    }
                }

                Response.Redirect("~/Modules/Tasks/Tasks.aspx");
            }
            catch (Exception ex)
            {
                pnlError.Visible = true;
                lblError.Text = "An error occurred while creating the task: " + ex.Message;
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Modules/Tasks/Tasks.aspx");
        }
    }
}