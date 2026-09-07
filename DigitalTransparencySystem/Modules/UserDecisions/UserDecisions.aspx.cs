using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace DigitalTransparencySystem.Modules.UserDecisions
{
    public partial class UserDecisions : System.Web.UI.Page
    {
        private string connectionString;
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
                LoadStats();
                LoadDecisions();
            }
        }

        private void SetActiveFilterButton()
        {
            string baseClass = "px-4 py-2 rounded-full font-label-md text-label-md bg-surface-container-high text-on-surface-variant hover:bg-surface-variant transition-colors";
            string activeClass = "px-4 py-2 rounded-full font-label-md text-label-md bg-primary text-on-primary";

            btnAll.CssClass = baseClass;
            btnProposed.CssClass = baseClass;
            btnUnderReview.CssClass = baseClass;
            btnApproved.CssClass = baseClass;
            btnCompleted.CssClass = baseClass;

            switch (currentFilter)
            {
                case "Proposed": btnProposed.CssClass = activeClass; break;
                case "UnderReview": btnUnderReview.CssClass = activeClass; break;
                case "Approved": btnApproved.CssClass = activeClass; break;
                case "Completed": btnCompleted.CssClass = activeClass; break;
                default: btnAll.CssClass = activeClass; break;
            }
        }

        private void LoadStats()
        {
            string userId = Session["UserID"].ToString();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                SqlCommand cmdTotal = new SqlCommand("SELECT COUNT(*) FROM Decisions WHERE ResponsibleUserID = @UserID", con);
                cmdTotal.Parameters.AddWithValue("@UserID", userId);
                litTotal.Text = cmdTotal.ExecuteScalar().ToString();

                SqlCommand cmdProposed = new SqlCommand("SELECT COUNT(*) FROM Decisions WHERE ResponsibleUserID = @UserID AND Status = 'Proposed'", con);
                cmdProposed.Parameters.AddWithValue("@UserID", userId);
                litProposed.Text = cmdProposed.ExecuteScalar().ToString();

                SqlCommand cmdUR = new SqlCommand("SELECT COUNT(*) FROM Decisions WHERE ResponsibleUserID = @UserID AND Status = 'UnderReview'", con);
                cmdUR.Parameters.AddWithValue("@UserID", userId);
                litUnderReview.Text = cmdUR.ExecuteScalar().ToString();

                SqlCommand cmdApproved = new SqlCommand("SELECT COUNT(*) FROM Decisions WHERE ResponsibleUserID = @UserID AND Status = 'Approved'", con);
                cmdApproved.Parameters.AddWithValue("@UserID", userId);
                litApproved.Text = cmdApproved.ExecuteScalar().ToString();

                SqlCommand cmdCompleted = new SqlCommand("SELECT COUNT(*) FROM Decisions WHERE ResponsibleUserID = @UserID AND Status = 'Completed'", con);
                cmdCompleted.Parameters.AddWithValue("@UserID", userId);
                litCompleted.Text = cmdCompleted.ExecuteScalar().ToString();
            }
        }

        private void LoadDecisions()
        {
            SetActiveFilterButton();

            string userId = Session["UserID"].ToString();
            string whereClause = "";

            switch (currentFilter)
            {
                case "Proposed": whereClause = "AND d.Status = 'Proposed'"; break;
                case "UnderReview": whereClause = "AND d.Status = 'UnderReview'"; break;
                case "Approved": whereClause = "AND d.Status = 'Approved'"; break;
                case "Completed": whereClause = "AND d.Status = 'Completed'"; break;
            }

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
                    $@"SELECT d.*, ISNULL(e.EventName, 'General') AS EventName
                      FROM Decisions d
                      LEFT JOIN Events e ON d.EventID = e.EventID
                      WHERE d.ResponsibleUserID = @UserID {whereClause}
                      ORDER BY d.CreatedAt DESC", con);
                cmd.Parameters.AddWithValue("@UserID", userId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                rptDecisions.DataSource = dt;
                rptDecisions.DataBind();
                pnlNoDecisions.Visible = dt.Rows.Count == 0;
            }
        }

        protected void btnAll_Click(object sender, EventArgs e) { currentFilter = "All"; LoadStats(); LoadDecisions(); }
        protected void btnProposed_Click(object sender, EventArgs e) { currentFilter = "Proposed"; LoadStats(); LoadDecisions(); }
        protected void btnUnderReview_Click(object sender, EventArgs e) { currentFilter = "UnderReview"; LoadStats(); LoadDecisions(); }
        protected void btnApproved_Click(object sender, EventArgs e) { currentFilter = "Approved"; LoadStats(); LoadDecisions(); }
        protected void btnCompleted_Click(object sender, EventArgs e) { currentFilter = "Completed"; LoadStats(); LoadDecisions(); }
    }
}
