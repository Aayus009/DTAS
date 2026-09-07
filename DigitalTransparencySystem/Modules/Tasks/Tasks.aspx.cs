using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Tasks
{
    public partial class Tasks : System.Web.UI.Page
    {
        private string connectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            connectionString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;

            if (!IsPostBack)
            {
                if (Session["UserID"] == null)
                {
                    Response.Redirect("~/Modules/Authentication/Login.aspx");
                    return;
                }

                string role = Session["Role"] as string;
                bool isAdmin = role != null && role.Equals("Admin", StringComparison.OrdinalIgnoreCase);

                pnlCreateTask.Visible = false;

                if (!isAdmin)
                {
                    pnlFilters.Visible = false;
                    pnlStats.Visible = false;
                }

                LoadAssignedUsersFilter();
                LoadEventsFilter();
                LoadTasks();
                if (isAdmin) LoadStats();
            }
        }

        private void LoadAssignedUsersFilter()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
                    @"SELECT DISTINCT u.UserID, u.FullName
                      FROM Users u
                      INNER JOIN TaskAssignments ta ON u.UserID = ta.UserID
                      INNER JOIN Tasks t ON ta.TaskID = t.TaskID
                      WHERE t.Status != 'Archived'
                      ORDER BY u.FullName", con);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                ddlAssignedUserFilter.DataSource = reader;
                ddlAssignedUserFilter.DataTextField = "FullName";
                ddlAssignedUserFilter.DataValueField = "UserID";
                ddlAssignedUserFilter.DataBind();
                reader.Close();
            }
            ddlAssignedUserFilter.Items.Insert(0, new ListItem("All Users", ""));
        }

        private void LoadEventsFilter()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
                    @"SELECT DISTINCT e.EventID, e.EventName
                      FROM Events e
                      INNER JOIN Tasks t ON t.EventID = e.EventID
                      WHERE t.Status != 'Archived'
                        AND ISNULL(t.IsDeleted, 0) = 0
                        AND ISNULL(e.IsDeleted, 0) = 0
                      ORDER BY e.EventName", con);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                ddlEventFilter.DataSource = reader;
                ddlEventFilter.DataTextField = "EventName";
                ddlEventFilter.DataValueField = "EventID";
                ddlEventFilter.DataBind();
                reader.Close();
            }
            ddlEventFilter.Items.Insert(0, new ListItem("All Events", ""));
            ddlEventFilter.Items.Insert(1, new ListItem("Not linked to an event", "none"));
        }

        private void LoadTasks(string search = "", string status = "", string priority = "", string assignedUser = "", string eventFilter = "")
        {
            string role = Session["Role"] as string;
            bool isAdmin = role != null && role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
            int currentUserID = Convert.ToInt32(Session["UserID"]);

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"SELECT t.TaskID, t.TaskTitle, t.Description, t.DueDate, t.Status,
                                        t.EventID, e.EventName,
                                        CASE WHEN ISNULL(t.IsRestricted, 0) = 1 OR ISNULL(e.IsDisabled, 0) = 1 THEN 1 ELSE 0 END AS IsRestricted,
                                        CASE 
                                            WHEN LEN(t.Description) > 60 THEN LEFT(t.Description, 60) + '...'
                                            ELSE t.Description
                                        END AS ShortDescription,
                                        t.Priority, t.CreatedAt,
                                        ISNULL((SELECT u.FullName FROM Users u WHERE u.UserID = t.LeaderID), 'Unassigned') AS LeaderName,
                                        ISNULL(STUFF((SELECT ', ' + u.FullName 
                                            FROM TaskAssignments ta2 
                                            INNER JOIN Users u ON ta2.UserID = u.UserID 
                                            WHERE ta2.TaskID = t.TaskID 
                                            FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, ''), 'Unassigned') AS AssignedTo
                                 FROM Tasks t
                                 LEFT JOIN Events e ON e.EventID = t.EventID
                                 WHERE t.Status != 'Archived'
                                   AND ISNULL(t.IsDeleted, 0) = 0";

                if (!isAdmin)
                {
                    query += @" AND (t.LeaderID = @CurrentUserID 
                                OR t.TaskID IN (SELECT TaskID FROM TaskTeamMembers ttm 
                                                INNER JOIN TaskTeams tt ON ttm.TeamID = tt.TeamID 
                                                WHERE ttm.UserID = @CurrentUserID AND ttm.Status = 'Accepted')
                                OR t.TaskID IN (SELECT TaskID FROM TaskAssignments WHERE UserID = @CurrentUserID))";
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query += " AND (t.TaskTitle LIKE @Search OR e.EventName LIKE @Search)";
                }

                if (!string.IsNullOrWhiteSpace(status) && status == "Overdue")
                {
                    query += @" AND t.DueDate IS NOT NULL
                                AND CAST(t.DueDate AS DATE) < CAST(GETDATE() AS DATE)
                                AND t.Status NOT IN ('Completed', 'Cancelled', 'Archived')";
                }
                else if (!string.IsNullOrWhiteSpace(status))
                {
                    query += " AND t.Status = @Status";
                }

                if (!string.IsNullOrWhiteSpace(priority))
                {
                    query += " AND t.Priority = @Priority";
                }

                if (!string.IsNullOrWhiteSpace(assignedUser))
                {
                    query += " AND t.TaskID IN (SELECT TaskID FROM TaskAssignments WHERE UserID = @AssignedUser)";
                }

                if (!string.IsNullOrWhiteSpace(eventFilter) && eventFilter == "none")
                {
                    query += " AND t.EventID IS NULL";
                }
                else if (!string.IsNullOrWhiteSpace(eventFilter))
                {
                    query += " AND t.EventID = @EventID";
                }

                query += " ORDER BY t.DueDate ASC";

                SqlCommand cmd = new SqlCommand(query, con);

                if (!isAdmin)
                {
                    cmd.Parameters.AddWithValue("@CurrentUserID", currentUserID);
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    cmd.Parameters.AddWithValue("@Search", "%" + search.Trim() + "%");
                }

                if (!string.IsNullOrWhiteSpace(status) && status != "Overdue")
                {
                    cmd.Parameters.AddWithValue("@Status", status.Trim());
                }

                if (!string.IsNullOrWhiteSpace(priority))
                {
                    cmd.Parameters.AddWithValue("@Priority", priority.Trim());
                }

                if (!string.IsNullOrWhiteSpace(assignedUser))
                {
                    cmd.Parameters.AddWithValue("@AssignedUser", int.Parse(assignedUser));
                }

                if (!string.IsNullOrWhiteSpace(eventFilter) && eventFilter != "none")
                {
                    cmd.Parameters.AddWithValue("@EventID", int.Parse(eventFilter));
                }

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                rptTasks.DataSource = dt;
                rptTasks.DataBind();

                pnlNoTasks.Visible = dt.Rows.Count == 0;

                int totalFiltered = dt.Rows.Count;
                litTaskCount.Text = $"<span class='font-label-md text-label-md text-on-surface-variant'>{totalFiltered} task{(totalFiltered != 1 ? "s" : "")}</span>";
            }
        }

        private void LoadStats()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                SqlCommand cmdTotal = new SqlCommand("SELECT COUNT(*) FROM Tasks WHERE Status != 'Archived' AND ISNULL(IsDeleted, 0) = 0", con);
                litTotalTasks.Text = cmdTotal.ExecuteScalar().ToString();

                SqlCommand cmdPending = new SqlCommand("SELECT COUNT(*) FROM Tasks WHERE Status = 'Pending' AND ISNULL(IsDeleted, 0) = 0", con);
                litPending.Text = cmdPending.ExecuteScalar().ToString();

                SqlCommand cmdInProgress = new SqlCommand("SELECT COUNT(*) FROM Tasks WHERE Status = 'InProgress' AND ISNULL(IsDeleted, 0) = 0", con);
                litInProgress.Text = cmdInProgress.ExecuteScalar().ToString();

                SqlCommand cmdCompleted = new SqlCommand("SELECT COUNT(*) FROM Tasks WHERE Status = 'Completed' AND ISNULL(IsDeleted, 0) = 0", con);
                litCompleted.Text = cmdCompleted.ExecuteScalar().ToString();

                SqlCommand cmdOverdue = new SqlCommand(
                    @"SELECT COUNT(*) FROM Tasks
                      WHERE Status NOT IN ('Completed', 'Cancelled', 'Archived')
                        AND ISNULL(IsDeleted, 0) = 0
                        AND DueDate IS NOT NULL
                        AND CAST(DueDate AS DATE) < CAST(GETDATE() AS DATE)", con);
                litOverdue.Text = cmdOverdue.ExecuteScalar().ToString();
            }
        }

        protected void rptTasks_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                Label lblStatus = (Label)e.Item.FindControl("lblStatus");
                Label lblPriority = (Label)e.Item.FindControl("lblPriority");
                Label lblOverdue = (Label)e.Item.FindControl("lblOverdue");

                object dueValue = DataBinder.Eval(e.Item.DataItem, "DueDate");
                string status = DataBinder.Eval(e.Item.DataItem, "Status").ToString();

                if (lblStatus != null)
                {
                    lblStatus.Text = TimelineService.StatusLabel(status, dueValue);
                    lblStatus.CssClass += TimelineService.IsOverdue(dueValue, status)
                        ? " bg-[rgba(198,40,40,0.12)] text-[#c62828]"
                        : " " + GetStatusBadgeClass(status);
                }

                if (lblPriority != null)
                {
                    string priority = DataBinder.Eval(e.Item.DataItem, "Priority").ToString();
                    lblPriority.Text = priority;
                    lblPriority.CssClass += " " + GetPriorityBadgeClass(priority);
                }

                bool overdue = TimelineService.IsOverdue(dueValue, status);
                if (lblOverdue != null)
                    lblOverdue.Visible = overdue;

                var spanDue = e.Item.FindControl("spanDueLabel") as System.Web.UI.HtmlControls.HtmlGenericControl;
                if (spanDue != null)
                {
                    spanDue.InnerText = TimelineService.DueLabel(dueValue, status);
                    spanDue.Attributes["class"] = TimelineService.DueClass(dueValue, status);
                }

                var taskRow = e.Item.FindControl("taskRow") as System.Web.UI.HtmlControls.HtmlTableRow;
                if (taskRow != null && overdue)
                    taskRow.Style["background-color"] = "rgba(198, 40, 40, 0.04)";
            }
        }

        protected void rptTasks_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int taskId;
            if (!int.TryParse(e.CommandArgument.ToString(), out taskId))
                return;

            switch (e.CommandName)
            {
                case "ViewTask":
                    Response.Redirect($"~/Modules/Tasks/TaskDetails.aspx?TaskID={taskId}");
                    break;
                case "RestrictTask":
                    RestrictionService.SetTaskRestricted(taskId, Convert.ToInt32(Session["UserID"]), Session["Role"] as string, true);
                    ReloadAdminList();
                    break;
                case "RestoreTask":
                    RestrictionService.SetTaskRestricted(taskId, Convert.ToInt32(Session["UserID"]), Session["Role"] as string, false);
                    ReloadAdminList();
                    break;
            }
        }

        private void ArchiveTask(int taskId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    "UPDATE Tasks SET Status = 'Archived', UpdatedAt = GETDATE() WHERE TaskID = @TaskID", con);
                cmd.Parameters.AddWithValue("@TaskID", taskId);
                cmd.ExecuteNonQuery();
            }
        }

        private void DeleteTask(int taskId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    "DELETE FROM Tasks WHERE TaskID = @TaskID", con);
                cmd.Parameters.AddWithValue("@TaskID", taskId);
                cmd.ExecuteNonQuery();
            }
        }

        private string GetStatusBadgeClass(string status)
        {
            switch (status)
            {
                case "Pending":
                    return "bg-[rgba(117,117,117,0.12)] text-[#616161]";
                case "InProgress":
                    return "bg-[rgba(230,126,0,0.12)] text-[#e67e00]";
                case "Submitted":
                    return "bg-[rgba(1,87,155,0.12)] text-[#01579b]";
                case "UnderReview":
                    return "bg-[rgba(123,31,162,0.12)] text-[#7b1fa2]";
                case "RevisionNeeded":
                    return "bg-[rgba(198,40,40,0.12)] text-[#c62828]";
                case "Approved":
                    return "bg-[rgba(46,125,50,0.12)] text-[#2e7d32]";
                case "Completed":
                    return "bg-[rgba(46,125,50,0.12)] text-[#2e7d32]";
                case "Delayed":
                    return "bg-[rgba(198,40,40,0.12)] text-[#c62828]";
                default:
                    return "bg-surface-container-high text-on-surface-variant";
            }
        }

        private string GetPriorityBadgeClass(string priority)
        {
            switch (priority)
            {
                case "Critical":
                    return "bg-[rgba(198,40,40,0.12)] text-[#c62828]";
                case "High":
                    return "bg-[rgba(198,40,40,0.08)] text-[#d32f2f]";
                case "Medium":
                    return "bg-[rgba(230,126,0,0.12)] text-[#e67e00]";
                case "Low":
                    return "bg-[rgba(25,118,210,0.12)] text-[#1976d2]";
                default:
                    return "bg-surface-container-high text-on-surface-variant";
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

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            ReloadAdminList();
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            txtSearch.Text = string.Empty;
            ddlStatusFilter.SelectedValue = string.Empty;
            ddlPriorityFilter.SelectedValue = string.Empty;
            ddlAssignedUserFilter.SelectedValue = string.Empty;
            ddlEventFilter.SelectedValue = string.Empty;
            LoadTasks();
            LoadStats();
        }

        private void ReloadAdminList()
        {
            LoadTasks(txtSearch.Text.Trim(), ddlStatusFilter.SelectedValue, ddlPriorityFilter.SelectedValue, ddlAssignedUserFilter.SelectedValue, ddlEventFilter.SelectedValue);
            LoadStats();
        }
    }
}
