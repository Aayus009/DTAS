using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.UserReports
{
    public partial class UserReports : System.Web.UI.Page
    {
        private string connectionString;

        private const string MyTaskMembershipFilter = TaskAccess.UserCanSeeTask;

        protected void Page_Load(object sender, EventArgs e)
        {
            connectionString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;

            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Modules/Authentication/Login.aspx");
                return;
            }

            if (!RoleAccess.IsFacultyOrStaff(Session["Role"] as string))
            {
                Response.Redirect("~/Modules/Dashboard/UsersDashboard.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadStats();
                LoadMyReports();
                LoadAssignmentReports();
                NotificationService.EnsureAssignmentReminders(Convert.ToInt32(Session["UserID"]));
                litLastUpdated.Text = DateTime.Now.ToString("MMM dd, yyyy - hh:mm tt");
            }
        }

        private void LoadStats()
        {
            string userId = Session["UserID"].ToString();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                SqlCommand cmdEvents = new SqlCommand(
                    @"SELECT COUNT(DISTINCT e.EventID)
                      FROM Events e
                      WHERE e.ProposedBy = @UserID
                         OR e.EventID IN (SELECT EventID FROM Tasks t WHERE " + MyTaskMembershipFilter + @")
                         OR e.EventID IN (
                            SELECT m.EventID FROM Meetings m
                            INNER JOIN MeetingParticipants mp ON m.MeetingID = mp.MeetingID
                            WHERE mp.UserID = @UserID AND m.EventID IS NOT NULL
                         )", con);
                cmdEvents.Parameters.AddWithValue("@UserID", userId);
                litEvents.Text = Convert.ToInt32(cmdEvents.ExecuteScalar()).ToString();

                SqlCommand cmdTasks = new SqlCommand(
                    "SELECT COUNT(*) FROM Tasks t WHERE " + MyTaskMembershipFilter, con);
                cmdTasks.Parameters.AddWithValue("@UserID", userId);
                litTasks.Text = cmdTasks.ExecuteScalar().ToString();

                SqlCommand cmdCompleted = new SqlCommand(
                    "SELECT COUNT(*) FROM Tasks t WHERE t.Status = 'Completed' AND " + MyTaskMembershipFilter, con);
                cmdCompleted.Parameters.AddWithValue("@UserID", userId);
                litCompleted.Text = cmdCompleted.ExecuteScalar().ToString();

                SqlCommand cmdOverdue = new SqlCommand(
                    @"SELECT COUNT(*) FROM Tasks t
                      WHERE t.Status NOT IN ('Completed', 'Cancelled', 'Archived')
                        AND t.DueDate IS NOT NULL AND t.DueDate < GETDATE()
                        AND " + MyTaskMembershipFilter, con);
                cmdOverdue.Parameters.AddWithValue("@UserID", userId);
                litOverdue.Text = cmdOverdue.ExecuteScalar().ToString();
            }
        }

        private void LoadAssignmentReports()
        {
            DataTable groups = AssignmentService.ListFacultyContributionGroups(
                Convert.ToInt32(Session["UserID"]), Session["Role"] as string);
            rptAssignmentGroups.DataSource = groups;
            rptAssignmentGroups.DataBind();
            pnlNoAssignmentGroups.Visible = groups.Rows.Count == 0;
        }

        private void LoadMyReports()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
                    @"SELECT ReportTitle, ReportType, CreatedAt
                      FROM Reports
                      WHERE GeneratedBy = @UserID
                      ORDER BY CreatedAt DESC", con);
                cmd.Parameters.AddWithValue("@UserID", Session["UserID"]);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                rptReports.DataSource = dt;
                rptReports.DataBind();
                pnlNoReports.Visible = dt.Rows.Count == 0;
            }
        }

        protected void btnGenerate_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Task ID,Task Title,Status,Due Date,Event,Decision");

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    @"SELECT t.TaskID, t.TaskTitle, t.Status, t.DueDate,
                             ISNULL(e.EventName, '') AS EventName,
                             ISNULL(d.DecisionTitle, '') AS DecisionTitle
                      FROM Tasks t
                      LEFT JOIN Events e ON t.EventID = e.EventID
                      LEFT JOIN Decisions d ON t.DecisionID = d.DecisionID
                      WHERE " + MyTaskMembershipFilter + @"
                      ORDER BY t.DueDate", con);
                cmd.Parameters.AddWithValue("@UserID", Session["UserID"]);

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string due = reader["DueDate"] == DBNull.Value
                        ? ""
                        : Convert.ToDateTime(reader["DueDate"]).ToString("yyyy-MM-dd");
                    sb.AppendLine(string.Format("{0},{1},{2},{3},{4},{5}",
                        reader["TaskID"],
                        EscapeCsv(reader["TaskTitle"].ToString()),
                        EscapeCsv(reader["Status"].ToString()),
                        due,
                        EscapeCsv(reader["EventName"].ToString()),
                        EscapeCsv(reader["DecisionTitle"].ToString())));
                }
                reader.Close();

                SqlCommand cmdInsert = new SqlCommand(
                    @"INSERT INTO Reports (ReportTitle, ReportType, Description, GeneratedBy, CreatedAt)
                      VALUES (@Title, @Type, @Description, @GeneratedBy, GETDATE())", con);
                cmdInsert.Parameters.AddWithValue("@Title", "My Accountability Report - " + DateTime.Now.ToString("MMM dd, yyyy"));
                cmdInsert.Parameters.AddWithValue("@Type", "Accountability");
                cmdInsert.Parameters.AddWithValue("@Description", "Scoped accountability export for the signed-in faculty/staff member.");
                cmdInsert.Parameters.AddWithValue("@GeneratedBy", Convert.ToInt32(Session["UserID"]));
                cmdInsert.ExecuteNonQuery();
            }

            Response.Clear();
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Disposition",
                "attachment; filename=My_Accountability_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv");
            Response.ContentEncoding = Encoding.UTF8;
            Response.Write(sb.ToString());
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
    }
}
