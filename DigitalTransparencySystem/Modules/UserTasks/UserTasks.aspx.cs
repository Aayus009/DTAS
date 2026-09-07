using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.UserTasks
{
    public partial class UserTasks : System.Web.UI.Page
    {
        private string connectionString;
        private int currentPage = 1;
        private int pageSize = 15;
        private string currentFilter = "All";

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
                if (Request.QueryString["filter"] != null)
                    currentFilter = Request.QueryString["filter"];
                else
                    currentFilter = "All";

                SetActiveFilterButton();
                LoadStats();
                LoadTasks();
            }
        }

        private void SetActiveFilterButton()
        {
            btnFilterAll.CssClass = "px-4 py-2 rounded-full font-label-md text-label-md bg-surface-container-high text-on-surface-variant hover:bg-surface-variant transition-colors";
            btnFilterPending.CssClass = btnFilterAll.CssClass;
            btnFilterInProgress.CssClass = btnFilterAll.CssClass;
            btnFilterCompleted.CssClass = btnFilterAll.CssClass;
            btnFilterLate.CssClass = btnFilterAll.CssClass;

            string activeClass = "px-4 py-2 rounded-full font-label-md text-label-md bg-primary text-on-primary";

            switch (currentFilter)
            {
                case "Pending": btnFilterPending.CssClass = activeClass; break;
                case "InProgress": btnFilterInProgress.CssClass = activeClass; break;
                case "Completed": btnFilterCompleted.CssClass = activeClass; break;
                case "Late": btnFilterLate.CssClass = activeClass; break;
                default: btnFilterAll.CssClass = activeClass; break;
            }
        }

        private void LoadStats()
        {
            string userId = Session["UserID"].ToString();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                string accessSql = TaskAccess.UserCanSeeTask;

                SqlCommand cmdTotal = new SqlCommand(
                    @"SELECT COUNT(*) FROM Tasks t
                      WHERE t.Status != 'Archived'
                      AND " + accessSql, con);
                cmdTotal.Parameters.AddWithValue("@UserID", userId);
                litTotal.Text = cmdTotal.ExecuteScalar().ToString();

                SqlCommand cmdPending = new SqlCommand(
                    @"SELECT COUNT(*) FROM Tasks t
                      WHERE t.Status IN ('Pending', 'NotStarted')
                      AND " + accessSql, con);
                cmdPending.Parameters.AddWithValue("@UserID", userId);
                litPending.Text = cmdPending.ExecuteScalar().ToString();

                SqlCommand cmdInProgress = new SqlCommand(
                    @"SELECT COUNT(*) FROM Tasks t
                      WHERE t.Status = 'InProgress'
                      AND " + accessSql, con);
                cmdInProgress.Parameters.AddWithValue("@UserID", userId);
                litInProgress.Text = cmdInProgress.ExecuteScalar().ToString();

                SqlCommand cmdCompleted = new SqlCommand(
                    @"SELECT COUNT(*) FROM Tasks t
                      WHERE t.Status = 'Completed'
                      AND " + accessSql, con);
                cmdCompleted.Parameters.AddWithValue("@UserID", userId);
                litCompleted.Text = cmdCompleted.ExecuteScalar().ToString();
            }
        }

        private void LoadTasks()
        {
            string userId = Session["UserID"].ToString();
            string whereClause = "";

            switch (currentFilter)
            {
                case "Pending": whereClause = "AND t.Status IN ('Pending', 'NotStarted')"; break;
                case "InProgress": whereClause = "AND t.Status = 'InProgress'"; break;
                case "Completed": whereClause = "AND t.Status = 'Completed'"; break;
                case "Late":
                    whereClause = @"AND t.Status NOT IN ('Completed', 'Cancelled', 'Archived')
                                    AND t.DueDate IS NOT NULL
                                    AND CAST(t.DueDate AS DATE) < CAST(GETDATE() AS DATE)";
                    break;
            }

            string accessClause = "AND " + TaskAccess.UserCanSeeTask;

            string query = $@"SELECT COUNT(*) FROM Tasks t 
                              WHERE t.Status != 'Archived' {accessClause} {whereClause}";

            int totalRecords = 0;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmdCount = new SqlCommand(query, con);
                cmdCount.Parameters.AddWithValue("@UserID", userId);
                totalRecords = (int)cmdCount.ExecuteScalar();
            }

            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            if (currentPage > totalPages) currentPage = Math.Max(1, totalPages);

            int offset = (currentPage - 1) * pageSize;

            string dataQuery = $@"SELECT TOP {pageSize} * FROM (
                SELECT ROW_NUMBER() OVER (ORDER BY 
                    CASE t.Status 
                        WHEN 'InProgress' THEN 1 
                        WHEN 'Pending' THEN 2 
                        WHEN 'NotStarted' THEN 3
                        WHEN 'Delayed' THEN 4 
                        WHEN 'Completed' THEN 5 
                        ELSE 6 
                    END, t.DueDate ASC
                ) AS RowNum,
                t.TaskID, t.TaskTitle, t.Description, t.Priority, t.Status, t.DueDate,
                ISNULL(e.EventName, 'General') AS EventName,
                CASE WHEN ISNULL(t.IsRestricted, 0) = 1 OR ISNULL(e.IsDisabled, 0) = 1 THEN 1 ELSE 0 END AS IsRestricted
                FROM Tasks t
                LEFT JOIN Events e ON t.EventID = e.EventID
                WHERE t.Status != 'Archived' {accessClause} {whereClause}
            ) AS Ranked WHERE RowNum > {offset}";

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(dataQuery, con);
                cmd.Parameters.AddWithValue("@UserID", userId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                rptTasks.DataSource = dt;
                rptTasks.DataBind();

                pnlNoTasks.Visible = dt.Rows.Count == 0;
            }

            lblPageInfo.Text = $"Showing {offset + 1}-{Math.Min(offset + pageSize, totalRecords)} of {totalRecords}";
            btnPrev.Enabled = currentPage > 1;
            btnNext.Enabled = currentPage < totalPages;
        }

        protected void btnFilterAll_Click(object sender, EventArgs e) { currentFilter = "All"; currentPage = 1; SetActiveFilterButton(); LoadStats(); LoadTasks(); }
        protected void btnFilterPending_Click(object sender, EventArgs e) { currentFilter = "Pending"; currentPage = 1; SetActiveFilterButton(); LoadStats(); LoadTasks(); }
        protected void btnFilterInProgress_Click(object sender, EventArgs e) { currentFilter = "InProgress"; currentPage = 1; SetActiveFilterButton(); LoadStats(); LoadTasks(); }
        protected void btnFilterCompleted_Click(object sender, EventArgs e) { currentFilter = "Completed"; currentPage = 1; SetActiveFilterButton(); LoadStats(); LoadTasks(); }
        protected void btnFilterLate_Click(object sender, EventArgs e) { currentFilter = "Late"; currentPage = 1; SetActiveFilterButton(); LoadStats(); LoadTasks(); }
        protected void btnPrev_Click(object sender, EventArgs e) { currentPage--; LoadTasks(); }
        protected void btnNext_Click(object sender, EventArgs e) { currentPage++; LoadTasks(); }
        protected void rptTasks_ItemCommand(object source, System.Web.UI.WebControls.RepeaterCommandEventArgs e) { }
    }
}
