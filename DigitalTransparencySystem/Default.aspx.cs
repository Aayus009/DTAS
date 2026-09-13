using System;
using System.Configuration;
using System.Data.SqlClient;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            bool signedIn = Session["UserID"] != null;
            if (signedIn)
            {
                lnkAccessDashboard.NavigateUrl = AuthService.HomeDashboardUrl(Session);
                lnkCreateAccount.Visible = false;
            }
            else
            {
                lnkAccessDashboard.NavigateUrl = "javascript:void(0)";
                lnkAccessDashboard.Attributes["onclick"] = "openLogin(); return false;";
            }

            if (!IsPostBack)
            {
                LoadStatistics();
            }
        }

        private void LoadStatistics()
        {
            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection con = new SqlConnection(connStr))
                {
                    con.Open();

                    SqlCommand cmd = new SqlCommand(
                        @"SELECT
                            (SELECT COUNT(*) FROM Decisions) AS TotalDecisions,
                            (SELECT COUNT(*) FROM Decisions WHERE Status = 'Completed') AS CompletedDecisions,
                            (SELECT COUNT(*) FROM Users WHERE IsActive = 1) AS ActiveUsers,
                            (SELECT COUNT(*) FROM Tasks) AS TotalTasks,
                            (SELECT COUNT(*) FROM Tasks WHERE Status = 'Completed') AS CompletedTasks", con);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int totalDecisions = reader["TotalDecisions"] == DBNull.Value ? 0 : Convert.ToInt32(reader["TotalDecisions"]);
                            int completedDecisions = reader["CompletedDecisions"] == DBNull.Value ? 0 : Convert.ToInt32(reader["CompletedDecisions"]);
                            int activeUsers = reader["ActiveUsers"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ActiveUsers"]);
                            int totalTasks = reader["TotalTasks"] == DBNull.Value ? 0 : Convert.ToInt32(reader["TotalTasks"]);
                            int completedTasks = reader["CompletedTasks"] == DBNull.Value ? 0 : Convert.ToInt32(reader["CompletedTasks"]);

                            litDecisions.Text = totalDecisions.ToString("N0");
                            litCompleted.Text = completedDecisions.ToString("N0");
                            litParticipants.Text = activeUsers.ToString("N0");

                            int accountability = totalTasks > 0 ? (int)Math.Round((completedTasks * 100.0) / totalTasks) : 92;
                            litAccountability.Text = accountability.ToString();
                        }
                    }
                }
            }
            catch
            {
                litDecisions.Text = "0";
                litCompleted.Text = "0";
                litParticipants.Text = "0";
                litAccountability.Text = "0";
            }
        }
    }
}