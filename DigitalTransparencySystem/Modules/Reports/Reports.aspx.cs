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

                int liveEvents = 0;
                int taskTotal = 0;
                int taskDone = 0;
                foreach (EventLiveProgress row in EventTaskService.ListLiveEventProgress())
                {
                    if (!EventTaskService.CountsTowardInstitutionProgress(row.StatusKey))
                        continue;
                    liveEvents++;
                    taskTotal += row.TotalTasks;
                    taskDone += row.CompletedTasks;
                }

                litEventsSummary.Text = liveEvents.ToString("N0");

                int completionRate = EventTaskService.PercentFromCounts(taskDone, taskTotal);
                litTasksRate.Text = completionRate + "%";
                litTasksRateBar.Text = "<div class='bg-primary h-full' style='width: " + completionRate + "%'></div>";

                int decisionPct = EventTaskService.InstitutionDecisionPercent();
                litDecisionOutcomes.Text = decisionPct + "%";
                litDecisionBar.Text = "<div class='bg-primary h-full' style='width: " + decisionPct + "%'></div>";
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
            var staff = new DataTable();
            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(
                @"SELECT u.UserID,
                         LTRIM(RTRIM(ISNULL(u.FullName, N''))) AS FullName,
                         LTRIM(RTRIM(ISNULL(u.Username, N''))) AS Username,
                         ISNULL(r.RoleName, N'') AS Role,
                         COUNT(t.TaskID) AS TotalTasks,
                         ISNULL(SUM(CASE WHEN REPLACE(LTRIM(RTRIM(ISNULL(t.Status, N''))), N' ', N'') = N'Completed' THEN 1 ELSE 0 END), 0) AS Completed,
                         ISNULL(SUM(CASE WHEN REPLACE(LTRIM(RTRIM(ISNULL(t.Status, N''))), N' ', N'') = N'Pending' THEN 1 ELSE 0 END), 0) AS Pending,
                         ISNULL(SUM(CASE WHEN REPLACE(LTRIM(RTRIM(ISNULL(t.Status, N''))), N' ', N'') IN (N'InProgress', N'In Progress') THEN 1 ELSE 0 END), 0) AS InProgress,
                         ISNULL(SUM(CASE WHEN REPLACE(LTRIM(RTRIM(ISNULL(t.Status, N''))), N' ', N'') NOT IN (N'Completed', N'Cancelled', N'Archived')
                                           AND t.DueDate IS NOT NULL AND t.DueDate < GETDATE() THEN 1 ELSE 0 END), 0) AS Overdue
                  FROM Users u
                  LEFT JOIN Roles r ON r.RoleID = u.RoleID
                  INNER JOIN TaskAssignments ta ON ta.UserID = u.UserID
                  INNER JOIN Tasks t ON t.TaskID = ta.TaskID AND ISNULL(t.IsDeleted, 0) = 0
                  WHERE ISNULL(u.IsDeleted, 0) = 0
                  GROUP BY u.UserID, u.FullName, u.Username, r.RoleName
                  HAVING COUNT(t.TaskID) > 0
                  ORDER BY u.FullName, u.Username", con))
            {
                new SqlDataAdapter(cmd).Fill(staff);
            }

            int liveEvents = 0;
            int taskTotal = 0;
            int taskDone = 0;
            foreach (EventLiveProgress row in EventTaskService.ListLiveEventProgress())
            {
                if (!EventTaskService.CountsTowardInstitutionProgress(row.StatusKey))
                    continue;
                liveEvents++;
                taskTotal += row.TotalTasks;
                taskDone += row.CompletedTasks;
            }

            byte[] xlsx = AccountabilityExcelReport.Build(
                staff,
                EventTaskService.PercentFromCounts(taskDone, taskTotal),
                EventTaskService.InstitutionDecisionPercent(),
                liveEvents,
                Convert.ToString(Session["FullName"]));

            DownloadXlsx("Accountability_Report_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx", xlsx);
        }

        private void DownloadXlsx(string fileName, byte[] bytes)
        {
            InsertReport(fileName.Replace(".xlsx", "").Replace("_", " "), "Excel accountability report with charts.");

            Response.Clear();
            Response.Buffer = true;
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("Content-Disposition", "attachment; filename=" + fileName);
            Response.BinaryWrite(bytes);
            Response.End();
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
