using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace DigitalTransparencySystem.Modules.Tasks
{
    public partial class TaskTeam : System.Web.UI.Page
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
                Response.Redirect("~/Modules/Tasks/Tasks.aspx");
                return;
            }

            if (!IsLeader())
            {
                Response.Redirect($"~/Modules/Tasks/TaskDetails.aspx?TaskID={taskID}");
                return;
            }

            if (!IsPostBack)
            {
                LoadTaskTitle();
                LoadTeamMembers();
                LoadProgress();
                LoadMemberUpdates();
                LoadAvailableUsers();
            }
        }

        private bool IsLeader()
        {
            int userID = Convert.ToInt32(Session["UserID"]);
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    "SELECT COUNT(1) FROM TaskTeams WHERE TaskID = @TaskID AND LeaderID = @UserID", con);
                cmd.Parameters.AddWithValue("@TaskID", taskID);
                cmd.Parameters.AddWithValue("@UserID", userID);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private void LoadTaskTitle()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("SELECT TaskTitle FROM Tasks WHERE TaskID = @TaskID", con);
                cmd.Parameters.AddWithValue("@TaskID", taskID);
                object result = cmd.ExecuteScalar();
                if (result != null)
                    litTaskTitle.Text = Server.HtmlEncode(result.ToString());
            }
        }

        private void LoadAvailableUsers()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                string query = @"SELECT u.UserID, u.FullName + ' (' + r.RoleName + ') - ' + ISNULL(d.DepartmentName, 'No Dept') AS DisplayName
                    FROM Users u
                    INNER JOIN Roles r ON u.RoleID = r.RoleID
                    LEFT JOIN Departments d ON u.DepartmentID = d.DepartmentID
                    WHERE u.IsActive = 1
                    AND u.UserID NOT IN (
                        SELECT ttm.UserID FROM TaskTeamMembers ttm
                        INNER JOIN TaskTeams tt ON ttm.TeamID = tt.TeamID
                        WHERE tt.TaskID = @TaskID AND ttm.Status != 'Declined'
                    )
                    AND u.UserID != @LeaderID";

                if (!string.IsNullOrWhiteSpace(txtSearchUser.Text.Trim()))
                {
                    query += " AND (u.FullName LIKE @Search OR d.DepartmentName LIKE @Search)";
                }

                query += " ORDER BY u.FullName";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@TaskID", taskID);
                cmd.Parameters.AddWithValue("@LeaderID", Convert.ToInt32(Session["UserID"]));

                if (!string.IsNullOrWhiteSpace(txtSearchUser.Text.Trim()))
                    cmd.Parameters.AddWithValue("@Search", "%" + txtSearchUser.Text.Trim() + "%");

                SqlDataReader reader = cmd.ExecuteReader();
                lstAvailableUsers.DataSource = reader;
                lstAvailableUsers.DataTextField = "DisplayName";
                lstAvailableUsers.DataValueField = "UserID";
                lstAvailableUsers.DataBind();
                reader.Close();
            }

            pnlNoUsers.Visible = lstAvailableUsers.Items.Count == 0;
            lstAvailableUsers.Visible = lstAvailableUsers.Items.Count > 0;
        }

        private void LoadTeamMembers()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    @"SELECT ttm.MemberID, ttm.UserID, u.FullName, r.RoleName AS Role, ttm.Status, ttm.InvitedAt
                      FROM TaskTeamMembers ttm
                      INNER JOIN Users u ON ttm.UserID = u.UserID
                      INNER JOIN Roles r ON u.RoleID = r.RoleID
                      INNER JOIN TaskTeams tt ON ttm.TeamID = tt.TeamID
                      WHERE tt.TaskID = @TaskID
                      ORDER BY ttm.Status, u.FullName", con);
                cmd.Parameters.AddWithValue("@TaskID", taskID);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    dt.Columns.Add("StatusStyle", typeof(string));
                    dt.Columns.Add("StatusText", typeof(string));

                    foreach (DataRow row in dt.Rows)
                    {
                        string status = row["Status"].ToString();
                        switch (status)
                        {
                            case "Accepted":
                                row["StatusStyle"] = "background-color: rgba(46,125,50,0.12); color: #2e7d32;";
                                row["StatusText"] = "Accepted";
                                break;
                            case "Declined":
                                row["StatusStyle"] = "background-color: rgba(198,40,40,0.12); color: #c62828;";
                                row["StatusText"] = "Declined";
                                break;
                            default:
                                row["StatusStyle"] = "background-color: rgba(230,126,0,0.12); color: #e67e00;";
                                row["StatusText"] = "Invited";
                                break;
                        }
                    }

                    rptTeamMembers.DataSource = dt;
                    rptTeamMembers.DataBind();
                    pnlNoMembers.Visible = false;
                    litMemberCount.Text = $"<span class='font-label-md text-label-md text-on-surface-variant'>{dt.Rows.Count} member{(dt.Rows.Count != 1 ? "s" : "")}</span>";
                }
                else
                {
                    rptTeamMembers.Visible = false;
                    pnlNoMembers.Visible = true;
                    litMemberCount.Text = "";
                }
            }
        }

        private void LoadProgress()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    "SELECT ISNULL(ProgressPercent, 0) FROM TaskTeams WHERE TaskID = @TaskID", con);
                cmd.Parameters.AddWithValue("@TaskID", taskID);
                object result = cmd.ExecuteScalar();
                int progress = result != null ? Convert.ToInt32(result) : 0;

                txtProgress.Text = progress.ToString();
                pnlProgressBar.Visible = true;
                litProgressBar.Text = $"<div class='h-full bg-[#2e7d32] rounded-full animate-progress' style='width: {progress}%;'></div>";
                litProgressText.Text = $"{progress}%";
            }
        }

        private void LoadMemberUpdates()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    @"SELECT tu.OldStatus, tu.NewStatus, tu.Comment, tu.UpdatedAt,
                             u.FullName AS UserName
                      FROM TaskUpdates tu
                      INNER JOIN Users u ON tu.UserID = u.UserID
                      INNER JOIN TaskTeamMembers ttm ON tu.UserID = ttm.UserID
                      INNER JOIN TaskTeams tt ON ttm.TeamID = tt.TeamID
                      WHERE tt.TaskID = @TaskID AND ttm.Status = 'Accepted'
                      ORDER BY tu.UpdatedAt DESC", con);
                cmd.Parameters.AddWithValue("@TaskID", taskID);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dt.Columns.Add("OldStatusStyle", typeof(string));
                dt.Columns.Add("NewStatusStyle", typeof(string));

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        string oldStatus = row["OldStatus"].ToString();
                        string newStatus = row["NewStatus"].ToString();
                        row["OldStatusStyle"] = GetInlineStatusStyle(oldStatus);
                        row["NewStatusStyle"] = GetInlineStatusStyle(newStatus);
                    }

                    rptMemberUpdates.DataSource = dt;
                    rptMemberUpdates.DataBind();
                    pnlNoUpdates.Visible = false;
                }
                else
                {
                    rptMemberUpdates.Visible = false;
                    pnlNoUpdates.Visible = true;
                }
            }
        }

        protected void txtSearchUser_TextChanged(object sender, EventArgs e)
        {
            LoadAvailableUsers();
        }

        protected void btnInviteMember_Click(object sender, EventArgs e)
        {
            if (lstAvailableUsers.SelectedIndex == -1)
            {
                ShowError("Please select a user to invite.");
                return;
            }

            int selectedUserID = int.Parse(lstAvailableUsers.SelectedValue);

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    SqlCommand cmdTeam = new SqlCommand(
                        "SELECT TeamID FROM TaskTeams WHERE TaskID = @TaskID", con);
                    cmdTeam.Parameters.AddWithValue("@TaskID", taskID);
                    int teamID = Convert.ToInt32(cmdTeam.ExecuteScalar());

                    SqlCommand cmdCheck = new SqlCommand(
                        "SELECT COUNT(1) FROM TaskTeamMembers WHERE TeamID = @TeamID AND UserID = @UserID AND Status != 'Declined'", con);
                    cmdCheck.Parameters.AddWithValue("@TeamID", teamID);
                    cmdCheck.Parameters.AddWithValue("@UserID", selectedUserID);
                    if (Convert.ToInt32(cmdCheck.ExecuteScalar()) > 0)
                    {
                        ShowError("This user is already a member or has a pending invitation.");
                        return;
                    }

                    SqlCommand cmdInvite = new SqlCommand(
                        @"INSERT INTO TaskTeamMembers (TeamID, UserID, Status, InvitedAt)
                          VALUES (@TeamID, @UserID, 'Invited', GETDATE())", con);
                    cmdInvite.Parameters.AddWithValue("@TeamID", teamID);
                    cmdInvite.Parameters.AddWithValue("@UserID", selectedUserID);
                    cmdInvite.ExecuteNonQuery();

                    SqlCommand cmdNotif = new SqlCommand(
                        @"INSERT INTO Notifications (UserID, Title, Message, IsRead, NotificationType, RelatedID, RelatedType, CreatedAt)
                          VALUES (@UserID, @Title, @Message, 0, 'TaskTeamInvite', @TaskID, 'Task', GETDATE())", con);
                    cmdNotif.Parameters.AddWithValue("@UserID", selectedUserID);
                    cmdNotif.Parameters.AddWithValue("@Title", "Task Team Invitation");
                    cmdNotif.Parameters.AddWithValue("@Message", $"You have been invited to join a task team. Go to Task Details to accept or decline.");
                    cmdNotif.Parameters.AddWithValue("@TaskID", taskID);
                    cmdNotif.ExecuteNonQuery();
                }

                ShowSuccess("Invitation sent successfully.");
                LoadTeamMembers();
                LoadAvailableUsers();
            }
            catch (Exception ex)
            {
                ShowError("Error inviting member: " + ex.Message);
            }
        }

        protected void btnUpdateProgress_Click(object sender, EventArgs e)
        {
            int progress;
            if (!int.TryParse(txtProgress.Text, out progress) || progress < 0 || progress > 100)
            {
                ShowError("Progress must be a number between 0 and 100.");
                return;
            }

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand(
                        "UPDATE TaskTeams SET ProgressPercent = @Progress WHERE TaskID = @TaskID", con);
                    cmd.Parameters.AddWithValue("@Progress", progress);
                    cmd.Parameters.AddWithValue("@TaskID", taskID);
                    cmd.ExecuteNonQuery();

                    SqlCommand cmdTask = new SqlCommand(
                        "UPDATE Tasks SET UpdatedAt = GETDATE() WHERE TaskID = @TaskID", con);
                    cmdTask.Parameters.AddWithValue("@TaskID", taskID);
                    cmdTask.ExecuteNonQuery();

                    SqlCommand cmdRollup = new SqlCommand("sp_UpdateProgressForTask", con);
                    cmdRollup.CommandType = System.Data.CommandType.StoredProcedure;
                    cmdRollup.Parameters.AddWithValue("@TaskID", taskID);
                    cmdRollup.ExecuteNonQuery();
                }

                LoadProgress();
                ShowSuccess("Progress updated successfully.");
            }
            catch (Exception ex)
            {
                ShowError("Error updating progress: " + ex.Message);
            }
        }

        protected void rptTeamMembers_ItemCommand(object source, System.Web.UI.WebControls.RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "RemoveMember")
            {
                int memberID;
                if (!int.TryParse(e.CommandArgument.ToString(), out memberID))
                    return;

                try
                {
                    using (SqlConnection con = new SqlConnection(connectionString))
                    {
                        con.Open();
                        SqlCommand cmd = new SqlCommand(
                            "DELETE FROM TaskTeamMembers WHERE MemberID = @MemberID", con);
                        cmd.Parameters.AddWithValue("@MemberID", memberID);
                        cmd.ExecuteNonQuery();
                    }

                    LoadTeamMembers();
                    LoadAvailableUsers();
                    ShowSuccess("Member removed from team.");
                }
                catch (Exception ex)
                {
                    ShowError("Error removing member: " + ex.Message);
                }
            }
        }

        protected void btnBackToTask_Click(object sender, EventArgs e)
        {
            Response.Redirect($"~/Modules/Tasks/TaskDetails.aspx?TaskID={taskID}");
        }

        private string GetInlineStatusStyle(string status)
        {
            switch (status)
            {
                case "Pending": return "background-color: rgba(117,117,117,0.12); color: #616161;";
                case "InProgress": return "background-color: rgba(230,126,0,0.12); color: #e67e00;";
                case "Submitted": return "background-color: rgba(1,87,155,0.12); color: #01579b;";
                case "UnderReview": return "background-color: rgba(123,31,162,0.12); color: #7b1fa2;";
                case "RevisionNeeded": return "background-color: rgba(198,40,40,0.12); color: #c62828;";
                case "Approved": return "background-color: rgba(46,125,50,0.12); color: #2e7d32;";
                case "Completed": return "background-color: rgba(46,125,50,0.12); color: #2e7d32;";
                case "Delayed": return "background-color: rgba(198,40,40,0.12); color: #c62828;";
                default: return "background-color: rgba(117,117,117,0.12); color: #616161;";
            }
        }

        private void ShowError(string message)
        {
            pnlError.Visible = true;
            lblError.Text = message;
            pnlSuccess.Visible = false;
        }

        private void ShowSuccess(string message)
        {
            pnlSuccess.Visible = true;
            lblSuccess.Text = message;
            pnlError.Visible = false;
        }
    }
}
