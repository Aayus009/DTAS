using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Web.UI;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Reports
{
    public partial class AuditTrail : System.Web.UI.Page
    {
        private string connectionString;
        private int currentPageIndex = 0;
        private int pageSize = 20;

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
                if (role == null || !role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                {
                    Response.Redirect("~/Modules/Authentication/Login.aspx");
                    return;
                }

                if (ViewState["PageIndex"] != null)
                    currentPageIndex = Convert.ToInt32(ViewState["PageIndex"]);

                LoadUsers();
                LoadAuditTrail();
            }
            else
            {
                if (ViewState["PageIndex"] != null)
                    currentPageIndex = Convert.ToInt32(ViewState["PageIndex"]);
            }

            LoadSystemAudit();
        }

        private void LoadSystemAudit()
        {
            DataTable logs = NotificationService.ListAuditLogs(50, null);
            rptSystemAudit.DataSource = logs;
            rptSystemAudit.DataBind();
            pnlNoSystemAudit.Visible = logs.Rows.Count == 0;
        }

        private void LoadUsers()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT UserID, FullName FROM Users ORDER BY FullName";
                SqlCommand cmd = new SqlCommand(query, con);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                ddlUser.DataSource = dt;
                ddlUser.DataTextField = "FullName";
                ddlUser.DataValueField = "UserID";
                ddlUser.DataBind();
                ddlUser.Items.Insert(0, new System.Web.UI.WebControls.ListItem("All Users", ""));
            }
        }

        private string BuildQuery(bool countOnly = false)
        {
            string selectClause = countOnly
                ? "SELECT COUNT(*)"
                : "SELECT Timestamp, UserName, ActionType, Description, RelatedItem, TypeBgColor, TypeColor";

            string baseQuery =
                selectClause + " FROM (" +
                "SELECT dh.ChangedAt AS Timestamp, u.FullName AS UserName, " +
                "'Decision' AS ActionType, " +
                "CONCAT(d.DecisionTitle, ' - status changed from ', dh.OldStatus, ' to ', dh.NewStatus) AS Description, " +
                "d.DecisionTitle AS RelatedItem, " +
                "'rgba(0,35,111,0.1)' AS TypeBgColor, '#00236f' AS TypeColor " +
                "FROM DecisionHistory dh " +
                "INNER JOIN Decisions d ON dh.DecisionID = d.DecisionID " +
                "INNER JOIN Users u ON dh.ChangedBy = u.UserID " +
                "UNION ALL " +
                "SELECT tu.UpdatedAt AS Timestamp, u.FullName AS UserName, " +
                "'Task' AS ActionType, " +
                "CONCAT(t.TaskTitle, ' - status changed from ', tu.OldStatus, ' to ', tu.NewStatus) AS Description, " +
                "t.TaskTitle AS RelatedItem, " +
                "'rgba(39,165,119,0.1)' AS TypeBgColor, '#27a577' AS TypeColor " +
                "FROM TaskUpdates tu " +
                "INNER JOIN Tasks t ON tu.TaskID = t.TaskID " +
                "INNER JOIN Users u ON tu.UserID = u.UserID " +
                "UNION ALL " +
                "SELECT ll.LoginTime AS Timestamp, ll.FullName AS UserName, " +
                "'Login' AS ActionType, " +
                "CASE WHEN ll.LogoutTime IS NULL THEN 'Logged in' ELSE CONCAT('Logged in - session ended at ', CONVERT(VARCHAR, ll.LogoutTime, 108)) END AS Description, " +
                "ll.Username AS RelatedItem, " +
                "'rgba(142,36,170,0.1)' AS TypeBgColor, '#8e24aa' AS TypeColor " +
                "FROM LoginLogs ll " +
                ") AS CombinedAudit";

            // Apply filters
            string whereClause = "";
            string separator = " WHERE ";

            if (!string.IsNullOrEmpty(txtDateFrom.Text))
            {
                whereClause += separator + "Timestamp >= @DateFrom";
                separator = " AND ";
            }

            if (!string.IsNullOrEmpty(txtDateTo.Text))
            {
                whereClause += separator + "Timestamp < DATEADD(DAY, 1, @DateTo)";
                separator = " AND ";
            }

            if (!string.IsNullOrEmpty(ddlUser.SelectedValue))
            {
                whereClause += separator + "UserName = (SELECT FullName FROM Users WHERE UserID = @UserID)";
                separator = " AND ";
            }

            if (!string.IsNullOrEmpty(ddlActionType.SelectedValue))
            {
                whereClause += separator + "ActionType = @ActionType";
                separator = " AND ";
            }

            string orderClause = countOnly ? "" : " ORDER BY Timestamp DESC";

            return baseQuery + whereClause + orderClause;
        }

        private void AddFilterParameters(SqlCommand cmd)
        {
            if (!string.IsNullOrEmpty(txtDateFrom.Text))
                cmd.Parameters.AddWithValue("@DateFrom", Convert.ToDateTime(txtDateFrom.Text));

            if (!string.IsNullOrEmpty(txtDateTo.Text))
                cmd.Parameters.AddWithValue("@DateTo", Convert.ToDateTime(txtDateTo.Text));

            if (!string.IsNullOrEmpty(ddlUser.SelectedValue))
                cmd.Parameters.AddWithValue("@UserID", Convert.ToInt32(ddlUser.SelectedValue));

            if (!string.IsNullOrEmpty(ddlActionType.SelectedValue))
                cmd.Parameters.AddWithValue("@ActionType", ddlActionType.SelectedValue);
        }

        private void LoadAuditTrail()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                // Get total count
                string countQuery = BuildQuery(countOnly: true);
                SqlCommand cmdCount = new SqlCommand(countQuery, con);
                AddFilterParameters(cmdCount);
                con.Open();
                int totalRecords = Convert.ToInt32(cmdCount.ExecuteScalar());
                int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
                if (totalPages < 1) totalPages = 1;

                litTotalEntries.Text = totalRecords.ToString();
                litTotalPages.Text = totalPages.ToString();

                if (currentPageIndex >= totalPages)
                    currentPageIndex = totalPages - 1;
                if (currentPageIndex < 0)
                    currentPageIndex = 0;

                litPageNumber.Text = (currentPageIndex + 1).ToString();
                ViewState["PageIndex"] = currentPageIndex;

                // Enable/disable pagination buttons
                btnPrev.Enabled = currentPageIndex > 0;
                btnNext.Enabled = currentPageIndex < totalPages - 1;
                btnPrev.CssClass = currentPageIndex > 0
                    ? "py-2 px-4 border border-outline text-on-surface-variant rounded-lg font-label-md text-label-md font-bold hover:bg-surface-container-low transition-colors cursor-pointer"
                    : "py-2 px-4 border border-outline text-on-surface-variant rounded-lg font-label-md text-label-md font-bold opacity-50 cursor-not-allowed";
                btnNext.CssClass = currentPageIndex < totalPages - 1
                    ? "py-2 px-4 border border-outline text-on-surface-variant rounded-lg font-label-md text-label-md font-bold hover:bg-surface-container-low transition-colors cursor-pointer"
                    : "py-2 px-4 border border-outline text-on-surface-variant rounded-lg font-label-md text-label-md font-bold opacity-50 cursor-not-allowed";

                // Get paginated data
                string dataQuery = BuildQuery(countOnly: false);
                dataQuery += " OFFSET " + (currentPageIndex * pageSize) + " ROWS FETCH NEXT " + pageSize + " ROWS ONLY";

                SqlCommand cmdData = new SqlCommand(dataQuery, con);
                AddFilterParameters(cmdData);

                SqlDataAdapter da = new SqlDataAdapter(cmdData);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    rptAuditTrail.DataSource = dt;
                    rptAuditTrail.DataBind();
                    pnlNoResults.Visible = false;
                }
                else
                {
                    pnlNoResults.Visible = true;
                }
            }
        }

        protected void btnApplyFilters_Click(object sender, EventArgs e)
        {
            currentPageIndex = 0;
            ViewState["PageIndex"] = currentPageIndex;
            LoadAuditTrail();
        }

        protected void btnClearFilters_Click(object sender, EventArgs e)
        {
            txtDateFrom.Text = "";
            txtDateTo.Text = "";
            ddlUser.SelectedIndex = 0;
            ddlActionType.SelectedIndex = 0;
            currentPageIndex = 0;
            ViewState["PageIndex"] = currentPageIndex;
            LoadAuditTrail();
        }

        protected void btnPrev_Click(object sender, EventArgs e)
        {
            if (currentPageIndex > 0)
            {
                currentPageIndex--;
                ViewState["PageIndex"] = currentPageIndex;
                LoadAuditTrail();
            }
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            currentPageIndex++;
            ViewState["PageIndex"] = currentPageIndex;
            LoadAuditTrail();
        }

        protected void btnExport_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Timestamp,User,Action Type,Description,Related Item");

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = BuildQuery(countOnly: false);
                SqlCommand cmd = new SqlCommand(query, con);
                AddFilterParameters(cmd);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string description = reader["Description"].ToString().Replace("\"", "\"\"");
                    string relatedItem = reader["RelatedItem"].ToString().Replace("\"", "\"\"");

                    sb.AppendLine(string.Format("{0},\"{1}\",\"{2}\",\"{3}\",\"{4}\"",
                        Convert.ToDateTime(reader["Timestamp"]).ToString("yyyy-MM-dd HH:mm:ss"),
                        reader["UserName"],
                        reader["ActionType"],
                        description,
                        relatedItem));
                }
            }

            string fileName = "AuditTrail_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";

            Response.Clear();
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Disposition", "attachment; filename=" + fileName);
            Response.ContentEncoding = Encoding.UTF8;
            Response.Write(sb.ToString());
            Response.End();
        }
    }
}
