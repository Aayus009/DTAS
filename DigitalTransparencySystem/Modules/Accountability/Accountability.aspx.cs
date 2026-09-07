using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace DigitalTransparencySystem.Modules.Accountability
{
    public partial class Accountability : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadMetrics();
            }
        }

        private void LoadMetrics()
        {
            litLastUpdated.Text = DateTime.Now.ToString("MMM dd, yyyy - hh:mm tt");

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                int totalLogins = ScalarInt(con, "SELECT COUNT(1) FROM LoginLogs");
                int activeUsers = ScalarInt(con, "SELECT COUNT(DISTINCT UserID) FROM LoginLogs WHERE LoginTime >= DATEADD(month, -1, GETDATE())");
                int auditEvents = ScalarInt(con, "SELECT COUNT(1) FROM DecisionHistory");
                int totalFeedback = ScalarInt(con, "SELECT COUNT(1) FROM Feedback");
                int respondedFeedback = ScalarInt(con, "SELECT COUNT(1) FROM Feedback WHERE AdminResponse IS NOT NULL AND LTRIM(RTRIM(AdminResponse)) <> ''");

                litTotalLogins.Text = totalLogins.ToString();
                litActiveUsers.Text = activeUsers.ToString();
                litAuditEvents.Text = auditEvents.ToString();
                litCommunityEngaged.Text = respondedFeedback.ToString();

                if (totalFeedback > 0)
                {
                    int pct = (int)Math.Round((double)respondedFeedback / totalFeedback * 100);
                    barEngagement.Style["width"] = pct + "%";
                }

                LoadAuditTrail(con);
                LoadLogins(con);
            }
        }

        private void LoadAuditTrail(SqlConnection con)
        {
            string query = @"
                SELECT TOP 8 h.DecisionID, d.DecisionTitle, h.OldStatus, h.NewStatus,
                       ISNULL(u.FullName, 'System') AS ChangedByName, h.ChangedAt
                FROM DecisionHistory h
                INNER JOIN Decisions d ON d.DecisionID = h.DecisionID
                LEFT JOIN Users u ON u.UserID = h.ChangedBy
                ORDER BY h.ChangedAt DESC";

            SqlCommand cmd = new SqlCommand(query, con);
            DataTable dt = new DataTable();
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            adapter.Fill(dt);
            rptAuditTrail.DataSource = dt;
            rptAuditTrail.DataBind();
        }

        private void LoadLogins(SqlConnection con)
        {
            string query = @"
                SELECT TOP 6 ll.FullName, ll.Username, ll.LoginTime, ll.IPAddress
                FROM LoginLogs ll
                ORDER BY ll.LoginTime DESC";

            SqlCommand cmd = new SqlCommand(query, con);
            DataTable dt = new DataTable();
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            adapter.Fill(dt);
            rptLogins.DataSource = dt;
            rptLogins.DataBind();
        }

        private int ScalarInt(SqlConnection con, string query)
        {
            SqlCommand cmd = new SqlCommand(query, con);
            object result = cmd.ExecuteScalar();
            return result == null ? 0 : Convert.ToInt32(result);
        }

        protected string GetStatusClass(string status)
        {
            switch (status.ToLower())
            {
                case "implemented":
                case "completed":
                case "closed": return "bg-tertiary-container text-on-tertiary-container";
                case "approved":
                case "in progress": return "bg-primary-fixed-dim text-on-primary-fixed-variant";
                case "proposed":
                case "pending": return "bg-surface-variant text-on-surface-variant";
                case "rejected":
                case "cancelled": return "bg-error-container text-on-error-container";
                default: return "bg-surface-variant text-on-surface-variant";
            }
        }
    }
}