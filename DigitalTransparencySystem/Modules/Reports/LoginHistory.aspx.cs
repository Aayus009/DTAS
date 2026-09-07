using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Web.UI;

namespace DigitalTransparencySystem.Modules.Reports
{
    public partial class LoginHistory : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
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

            if (!IsPostBack)
            {
                LoadLoginHistory();
            }
        }

        private void LoadLoginHistory(string search = "")
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT FullName, Username, LoginTime, LogoutTime, IPAddress FROM LoginLogs WHERE (FullName LIKE @Search OR Username LIKE @Search)";

                if (!string.IsNullOrEmpty(txtDate.Text))
                    query += " AND LoginTime >= @StartDate AND LoginTime < @EndDate";

                if (!string.IsNullOrEmpty(txtStartTime.Text))
                    query += " AND CONVERT(time, LoginTime) >= @StartTime";

                if (!string.IsNullOrEmpty(txtEndTime.Text))
                    query += " AND CONVERT(time, LoginTime) <= @EndTime";

                query += " ORDER BY LoginTime DESC";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Search", "%" + search + "%");

                if (!string.IsNullOrEmpty(txtDate.Text))
                {
                    DateTime selectedDate = Convert.ToDateTime(txtDate.Text);
                    cmd.Parameters.AddWithValue("@StartDate", selectedDate.Date);
                    cmd.Parameters.AddWithValue("@EndDate", selectedDate.Date.AddDays(1));
                }

                if (!string.IsNullOrEmpty(txtStartTime.Text))
                    cmd.Parameters.AddWithValue("@StartTime", TimeSpan.Parse(txtStartTime.Text));

                if (!string.IsNullOrEmpty(txtEndTime.Text))
                    cmd.Parameters.AddWithValue("@EndTime", TimeSpan.Parse(txtEndTime.Text));

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    rptLoginHistory.DataSource = dt;
                    rptLoginHistory.DataBind();
                    litRecordCount.Text = dt.Rows.Count.ToString();
                    pnlNoResults.Visible = false;
                }
                else
                {
                    rptLoginHistory.DataSource = null;
                    rptLoginHistory.DataBind();
                    litRecordCount.Text = "0";
                    pnlNoResults.Visible = true;
                }
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            LoadLoginHistory(txtSearch.Text);
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            txtDate.Text = "";
            txtStartTime.Text = "";
            txtEndTime.Text = "";
            LoadLoginHistory();
        }

        protected void btnExport_Click(object sender, EventArgs e)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Full Name,Username,Login Time,Logout Time,IP Address");

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT FullName, Username, LoginTime, LogoutTime, IPAddress FROM LoginLogs WHERE (FullName LIKE @Search OR Username LIKE @Search)";

                if (!string.IsNullOrEmpty(txtDate.Text))
                    query += " AND LoginTime >= @StartDate AND LoginTime < @EndDate";

                if (!string.IsNullOrEmpty(txtStartTime.Text))
                    query += " AND CONVERT(time, LoginTime) >= @StartTime";

                if (!string.IsNullOrEmpty(txtEndTime.Text))
                    query += " AND CONVERT(time, LoginTime) <= @EndTime";

                query += " ORDER BY LoginTime DESC";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Search", "%" + txtSearch.Text + "%");

                if (!string.IsNullOrEmpty(txtDate.Text))
                {
                    DateTime selectedDate = Convert.ToDateTime(txtDate.Text);
                    cmd.Parameters.AddWithValue("@StartDate", selectedDate.Date);
                    cmd.Parameters.AddWithValue("@EndDate", selectedDate.Date.AddDays(1));
                }

                if (!string.IsNullOrEmpty(txtStartTime.Text))
                    cmd.Parameters.AddWithValue("@StartTime", TimeSpan.Parse(txtStartTime.Text));

                if (!string.IsNullOrEmpty(txtEndTime.Text))
                    cmd.Parameters.AddWithValue("@EndTime", TimeSpan.Parse(txtEndTime.Text));

                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    sb.AppendLine(string.Format("\"{0}\",\"{1}\",{2},{3},{4}",
                        reader["FullName"].ToString().Replace("\"", "\"\""),
                        reader["Username"].ToString().Replace("\"", "\"\""),
                        Convert.ToDateTime(reader["LoginTime"]).ToString("yyyy-MM-dd HH:mm:ss"),
                        reader["LogoutTime"] != DBNull.Value ? Convert.ToDateTime(reader["LogoutTime"]).ToString("yyyy-MM-dd HH:mm:ss") : "Active",
                        reader["IPAddress"]));
                }
            }

            string fileName = "LoginHistory_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";

            Response.Clear();
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Disposition", "attachment; filename=" + fileName);
            Response.ContentEncoding = Encoding.UTF8;
            Response.Write(sb.ToString());
            Response.End();
        }

    }
}
