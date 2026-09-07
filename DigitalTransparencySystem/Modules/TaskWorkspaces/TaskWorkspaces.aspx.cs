using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.TaskWorkspaces
{
    public partial class TaskWorkspaces : System.Web.UI.Page
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

            if (!IsPostBack)
            {
                LoadPendingInvitations();
                LoadEventWorkspaces();
                LoadWorkspaces();
            }
        }

        private int GetCurrentUserId()
        {
            return Convert.ToInt32(Session["UserID"]);
        }

        private void LoadPendingInvitations()
        {
            string userId = Session["UserID"].ToString();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
                    @"SELECT t.TaskID, t.TaskTitle, t.Description, t.Priority, t.DueDate
                      FROM TaskTeamMembers ttm
                      INNER JOIN TaskTeams tt ON ttm.TeamID = tt.TeamID
                      INNER JOIN Tasks t ON tt.TaskID = t.TaskID
                      WHERE ttm.UserID = @UserID AND ttm.Status = 'Invited'
                      ORDER BY t.DueDate ASC", con);
                cmd.Parameters.AddWithValue("@UserID", userId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    pnlNoPending.Visible = false;
                    rptPending.DataSource = dt;
                    rptPending.DataBind();
                    pnlPendingCount.InnerText = dt.Rows.Count.ToString();
                }
                else
                {
                    pnlNoPending.Visible = true;
                    rptPending.Visible = false;
                    pnlPendingCount.InnerText = "0";
                }
            }
        }

        private void LoadEventWorkspaces()
        {
            DataTable events = EventService.ListEventWorkspaces(
                GetCurrentUserId(),
                RoleAccess.IsAdmin(Session["Role"] as string));
            rptEventWorkspaces.DataSource = events;
            rptEventWorkspaces.DataBind();
            pnlNoEvents.Visible = events.Rows.Count == 0;
            rptEventWorkspaces.Visible = events.Rows.Count > 0;
        }

        private void LoadWorkspaces()
        {
            string userId = Session["UserID"].ToString();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = RoleAccess.IsAdmin(Session["Role"] as string)
                    ? @"SELECT t.TaskID, t.TaskTitle, t.Description, t.Priority, t.Status, t.DueDate
                        FROM Tasks t
                        WHERE t.EventID IS NULL
                        ORDER BY 
                            CASE t.Status 
                                WHEN 'InProgress' THEN 1 
                                WHEN 'Pending' THEN 2 
                                WHEN 'NotStarted' THEN 3
                                WHEN 'Delayed' THEN 4 
                                WHEN 'Completed' THEN 5 
                                ELSE 6 
                            END, t.DueDate ASC"
                    : @"SELECT t.TaskID, t.TaskTitle, t.Description, t.Priority, t.Status, t.DueDate
                        FROM Tasks t
                        WHERE t.Status != 'Archived' AND t.EventID IS NULL AND " + TaskAccess.UserCanSeeTask + @"
                        ORDER BY 
                            CASE t.Status 
                                WHEN 'InProgress' THEN 1 
                                WHEN 'Pending' THEN 2 
                                WHEN 'NotStarted' THEN 3
                                WHEN 'Delayed' THEN 4 
                                WHEN 'Completed' THEN 5 
                                ELSE 6 
                            END, t.DueDate ASC";
                SqlCommand cmd = new SqlCommand(query, con);
                if (!RoleAccess.IsAdmin(Session["Role"] as string))
                    cmd.Parameters.AddWithValue("@UserID", userId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    pnlNoAccepted.Visible = false;
                    rptWorkspaces.DataSource = dt;
                    rptWorkspaces.DataBind();
                }
                else
                {
                    pnlNoAccepted.Visible = true;
                    rptWorkspaces.Visible = false;
                }
            }
        }

        protected void rptPending_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int taskId;
            if (!int.TryParse(e.CommandArgument.ToString(), out taskId))
                return;

            switch (e.CommandName)
            {
                case "Accept":
                    RespondToInvite(taskId, "Accepted");
                    break;
                case "Decline":
                    RespondToInvite(taskId, "Declined");
                    break;
            }

            LoadPendingInvitations();
            LoadEventWorkspaces();
            LoadWorkspaces();
        }

        private void RespondToInvite(int taskId, string newStatus)
        {
            int userId = GetCurrentUserId();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    @"UPDATE TaskTeamMembers SET Status = @Status, RespondedAt = GETDATE()
                      WHERE TeamID IN (SELECT TeamID FROM TaskTeams WHERE TaskID = @TaskID)
                      AND UserID = @UserID AND Status = 'Invited'", con);
                cmd.Parameters.AddWithValue("@Status", newStatus);
                cmd.Parameters.AddWithValue("@TaskID", taskId);
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.ExecuteNonQuery();
            }

            lblMessage.Visible = true;
            lblMessage.Text = newStatus == "Accepted"
                ? "Invitation accepted. The task workspace is now available below."
                : "Invitation declined.";
        }
    }
}
