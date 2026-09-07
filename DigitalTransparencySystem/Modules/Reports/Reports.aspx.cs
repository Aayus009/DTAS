using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Reports
{
    public partial class Reports : System.Web.UI.Page
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
                if (role == null || !role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                {
                    Response.Redirect("~/Modules/Authentication/Login.aspx");
                    return;
                }

                LoadStats();
                LoadSystemSnapshot();
                LoadRecentReports();
                litLastUpdated.Text = DateTime.Now.ToString("MMM dd, yyyy - hh:mm tt");
            }
        }

        private void LoadSystemSnapshot()
        {
            DataTable stats = NotificationService.GetAdminStats();
            if (stats.Rows.Count == 0)
                return;
            DataRow row = stats.Rows[0];
            litSnapUsers.Text = Convert.ToString(row["TotalUsers"]);
            litSnapPendingId.Text = Convert.ToString(row["PendingVerification"]);
            litSnapAssignments.Text = Convert.ToString(row["ActiveAssignments"]);
            litSnapGroups.Text = Convert.ToString(row["ActiveGroups"]);
            litSnapFlags.Text = Convert.ToString(row["PendingReports"]);
            litSnapBanned.Text = Convert.ToString(row["BannedUsers"]);
        }

        private void LoadStats()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                SqlCommand cmdTotal = new SqlCommand("SELECT COUNT(*) FROM Reports", con);
                object totalResult = cmdTotal.ExecuteScalar();
                litTotalReports.Text = totalResult != DBNull.Value ? Convert.ToInt32(totalResult).ToString("N0") : "0";

                SqlCommand cmdEvents = new SqlCommand("SELECT COUNT(*) FROM Events WHERE ISNULL(IsDeleted, 0) = 0", con);
                object eventsResult = cmdEvents.ExecuteScalar();
                litEventsSummary.Text = eventsResult != DBNull.Value ? Convert.ToInt32(eventsResult).ToString("N0") : "0";

                SqlCommand cmdTotalTasks = new SqlCommand("SELECT COUNT(*) FROM Tasks", con);
                object totalTasks = cmdTotalTasks.ExecuteScalar();

                SqlCommand cmdCompletedTasks = new SqlCommand("SELECT COUNT(*) FROM Tasks WHERE Status = 'Completed'", con);
                object completedTasks = cmdCompletedTasks.ExecuteScalar();

                int tTotal = totalTasks != DBNull.Value ? Convert.ToInt32(totalTasks) : 0;
                int tCompleted = completedTasks != DBNull.Value ? Convert.ToInt32(completedTasks) : 0;
                decimal completionRate = tTotal > 0 ? (decimal)tCompleted / tTotal * 100 : 0;
                litTasksRate.Text = completionRate.ToString("F1") + "%";
                litTasksRateBar.Text = "<div class='bg-primary h-full' style='width: " + completionRate.ToString("F0") + "%'></div>";

                SqlCommand cmdDecisions = new SqlCommand(
                    "SELECT COUNT(*) FROM Decisions WHERE Status IN ('Approved', 'Rejected', 'Implemented')", con);
                object decisionsResult = cmdDecisions.ExecuteScalar();
                litDecisionOutcomes.Text = decisionsResult != DBNull.Value ? Convert.ToInt32(decisionsResult).ToString("N0") : "0";
            }
        }

        private void LoadRecentReports()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"SELECT TOP 10 r.ReportTitle, r.ReportType, r.CreatedAt,
                                 u.FullName AS GeneratedBy
                          FROM Reports r
                          LEFT JOIN Users u ON r.GeneratedBy = u.UserID
                          ORDER BY r.CreatedAt DESC";

                SqlCommand cmd = new SqlCommand(query, con);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    rptRecentReports.DataSource = dt;
                    rptRecentReports.DataBind();
                    litRecentCount.Text = dt.Rows.Count.ToString();
                    pnlNoReports.Visible = false;
                }
                else
                {
                    pnlNoReports.Visible = true;
                    litRecentCount.Text = "0";
                }
            }
        }

        private string GetReportTitle(string reportType)
        {
            return reportType + " Report - " + DateTime.Now.ToString("MMM dd, yyyy");
        }

        private void InsertReport(string reportType, string description)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"INSERT INTO Reports (ReportTitle, ReportType, Description, GeneratedBy, CreatedAt)
                                  VALUES (@Title, @Type, @Description, @GeneratedBy, @CreatedAt)";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Title", GetReportTitle(reportType));
                cmd.Parameters.AddWithValue("@Type", reportType);
                cmd.Parameters.AddWithValue("@Description", description);
                cmd.Parameters.AddWithValue("@GeneratedBy", Convert.ToInt32(Session["UserID"]));
                cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private void DownloadCsv(string fileName, string csvContent)
        {
            InsertReport(fileName.Replace(".csv", "").Replace("_", " "), "Generated report download.");

            Response.Clear();
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Disposition", "attachment; filename=" + fileName);
            Response.ContentEncoding = Encoding.UTF8;
            Response.Write(csvContent);
            Response.End();
        }

        private string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            value = value.Replace("\"", "\"\"");
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
                return "\"" + value + "\"";
            return value;
        }

        private static string CsvText(object value)
        {
            if (value == null || value == DBNull.Value)
                return "";
            return Convert.ToString(value) ?? "";
        }

        private string CsvEscaped(object value)
        {
            return EscapeCsv(CsvText(value));
        }

        private static string CsvDate(object value, string format)
        {
            if (value == null || value == DBNull.Value)
                return "";
            return Convert.ToDateTime(value).ToString(format);
        }

        private static string CsvDecimal(object value)
        {
            if (value == null || value == DBNull.Value)
                return "";
            return Convert.ToDecimal(value).ToString("F2");
        }

        protected void btnGenerateEventReport_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Event ID,Event Name,Event Type,Start Date,End Date,Venue,Budget,Organizer,Status,Created At");

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"SELECT EventID, EventName, EventType, StartDate, EndDate, Venue, Budget, Organizer, Status, CreatedAt
                                  FROM Events ORDER BY CreatedAt DESC";
                SqlCommand cmd = new SqlCommand(query, con);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    sb.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}",
                        CsvText(reader["EventID"]),
                        CsvEscaped(reader["EventName"]),
                        CsvEscaped(reader["EventType"]),
                        CsvDate(reader["StartDate"], "yyyy-MM-dd HH:mm"),
                        CsvDate(reader["EndDate"], "yyyy-MM-dd HH:mm"),
                        CsvEscaped(reader["Venue"]),
                        CsvDecimal(reader["Budget"]),
                        CsvEscaped(reader["Organizer"]),
                        CsvEscaped(reader["Status"]),
                        CsvDate(reader["CreatedAt"], "yyyy-MM-dd HH:mm")));
                }
            }

            DownloadCsv("Event_Report_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv", sb.ToString());
        }

        protected void btnGenerateDecisionReport_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Decision ID,Decision Title,Priority,Status,Due Date,Created By,Created At,Updated At");

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"SELECT d.DecisionID, d.DecisionTitle, d.Priority, d.Status, d.DueDate,
                                        u.FullName AS CreatedByName, d.CreatedAt, d.UpdatedAt
                                  FROM Decisions d
                                  LEFT JOIN Users u ON d.CreatedBy = u.UserID
                                  ORDER BY d.CreatedAt DESC";
                SqlCommand cmd = new SqlCommand(query, con);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    sb.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7}",
                        CsvText(reader["DecisionID"]),
                        CsvEscaped(reader["DecisionTitle"]),
                        CsvEscaped(reader["Priority"]),
                        CsvEscaped(reader["Status"]),
                        CsvDate(reader["DueDate"], "yyyy-MM-dd"),
                        CsvEscaped(reader["CreatedByName"]),
                        CsvDate(reader["CreatedAt"], "yyyy-MM-dd HH:mm"),
                        CsvDate(reader["UpdatedAt"], "yyyy-MM-dd HH:mm")));
                }
            }

            DownloadCsv("Decision_Report_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv", sb.ToString());
        }

        protected void btnGenerateAccountabilityReport_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("User,Total Tasks,Completed Tasks,Pending Tasks,In-Progress Tasks,Overdue Tasks");

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"SELECT u.FullName,
                                        COUNT(t.TaskID) AS TotalTasks,
                                        SUM(CASE WHEN t.Status = 'Completed' THEN 1 ELSE 0 END) AS Completed,
                                        SUM(CASE WHEN t.Status = 'Pending' THEN 1 ELSE 0 END) AS Pending,
                                        SUM(CASE WHEN t.Status = 'InProgress' THEN 1 ELSE 0 END) AS InProgress,
                                        SUM(CASE WHEN t.Status != 'Completed' AND t.DueDate < GETDATE() THEN 1 ELSE 0 END) AS Overdue
                                  FROM Users u
                                  LEFT JOIN TaskAssignments ta ON u.UserID = ta.UserID
                                  LEFT JOIN Tasks t ON ta.TaskID = t.TaskID
                                  GROUP BY u.FullName
                                  HAVING COUNT(t.TaskID) > 0
                                  ORDER BY TotalTasks DESC";
                SqlCommand cmd = new SqlCommand(query, con);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    sb.AppendLine(string.Format("{0},{1},{2},{3},{4},{5}",
                        EscapeCsv(reader["FullName"].ToString()),
                        reader["TotalTasks"],
                        reader["Completed"],
                        reader["Pending"],
                        reader["InProgress"],
                        reader["Overdue"]));
                }
            }

            DownloadCsv("Accountability_Report_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv", sb.ToString());
        }

        protected void btnGenerateTaskCompletionReport_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Task ID,Task Title,Priority,Status,Due Date,Assigned To,Created At");

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"SELECT t.TaskID, t.TaskTitle, t.Priority, t.Status, t.DueDate,
                                        STUFF((SELECT ', ' + u2.FullName
                                               FROM TaskAssignments ta2
                                               INNER JOIN Users u2 ON ta2.UserID = u2.UserID
                                               WHERE ta2.TaskID = t.TaskID
                                               FOR XML PATH('')), 1, 2, '') AS AssignedTo,
                                        t.CreatedAt
                                  FROM Tasks t
                                  ORDER BY t.CreatedAt DESC";
                SqlCommand cmd = new SqlCommand(query, con);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    sb.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6}",
                        CsvText(reader["TaskID"]),
                        CsvEscaped(reader["TaskTitle"]),
                        CsvEscaped(reader["Priority"]),
                        CsvEscaped(reader["Status"]),
                        CsvDate(reader["DueDate"], "yyyy-MM-dd"),
                        CsvEscaped(reader["AssignedTo"]),
                        CsvDate(reader["CreatedAt"], "yyyy-MM-dd HH:mm")));
                }
            }

            DownloadCsv("Task_Completion_Report_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv", sb.ToString());
        }

        protected void btnGenerateUsersReport_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("User ID,Full Name,Username,Email,Role,Department,Status,Joined Date");

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"SELECT u.UserID, u.FullName, u.Username, u.Email,
                                        r.RoleName AS Role,
                                        ISNULL(d.DepartmentName, 'N/A') AS Department,
                                        CASE WHEN u.IsActive = 1 THEN 'Active' ELSE 'Inactive' END AS Status,
                                        u.CreatedAt
                                  FROM Users u
                                  LEFT JOIN Roles r ON u.RoleID = r.RoleID
                                  LEFT JOIN Departments d ON u.DepartmentID = d.DepartmentID
                                  ORDER BY u.CreatedAt DESC";
                SqlCommand cmd = new SqlCommand(query, con);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    sb.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7}",
                        CsvText(reader["UserID"]),
                        CsvEscaped(reader["FullName"]),
                        CsvEscaped(reader["Username"]),
                        CsvEscaped(reader["Email"]),
                        CsvEscaped(reader["Role"]),
                        CsvEscaped(reader["Department"]),
                        CsvEscaped(reader["Status"]),
                        CsvDate(reader["CreatedAt"], "yyyy-MM-dd HH:mm")));
                }
            }

            DownloadCsv("Users_Report_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv", sb.ToString());
        }

        protected void btnGenerateFeedbackReport_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Feedback ID,Title,Category,Description,Rating,Status,Admin Response,Submitted By,Created At");

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"SELECT f.FeedbackID, f.Title, f.Category, f.Description, f.Rating,
                                        f.Status, f.AdminResponse, u.FullName AS SubmittedBy, f.CreatedAt
                                  FROM Feedback f
                                  LEFT JOIN Users u ON f.UserID = u.UserID
                                  ORDER BY f.CreatedAt DESC";
                SqlCommand cmd = new SqlCommand(query, con);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    sb.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}",
                        CsvText(reader["FeedbackID"]),
                        CsvEscaped(reader["Title"]),
                        CsvEscaped(reader["Category"]),
                        CsvEscaped(reader["Description"]),
                        reader["Rating"] == DBNull.Value ? "N/A" : reader["Rating"].ToString(),
                        CsvEscaped(reader["Status"]),
                        CsvEscaped(reader["AdminResponse"]),
                        string.IsNullOrEmpty(CsvText(reader["SubmittedBy"])) ? "Anonymous" : CsvEscaped(reader["SubmittedBy"]),
                        CsvDate(reader["CreatedAt"], "yyyy-MM-dd HH:mm")));
                }
            }

            DownloadCsv("Feedback_Report_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv", sb.ToString());
        }
    }
}
