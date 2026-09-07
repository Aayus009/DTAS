using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace DigitalTransparencySystem.Modules.Users
{
    public partial class UserActivity : System.Web.UI.Page
    {
        private string connectionString;
        private int profileUserID;

        protected void Page_Load(object sender, EventArgs e)
        {
            connectionString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;

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

            if (!int.TryParse(Request.QueryString["UserID"], out profileUserID) || profileUserID <= 0)
            {
                Response.Redirect("~/Modules/Users/Users.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadUserProfile();
                LoadUserStats();
                LoadUserTasks();
                LoadUserDecisions();
                LoadUserLoginHistory();
            }
        }

        private void LoadUserProfile()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
                    @"SELECT u.FullName, u.Email, u.Username, u.CreatedAt,
                             r.RoleName, d.DepartmentName
                      FROM Users u
                      LEFT JOIN Roles r ON u.RoleID = r.RoleID
                      LEFT JOIN Departments d ON u.DepartmentID = d.DepartmentID
                      WHERE u.UserID = @UserID", con);
                cmd.Parameters.AddWithValue("@UserID", profileUserID);
                con.Open();

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    litUserName.Text = reader["FullName"].ToString();
                    litProfileName.Text = reader["FullName"].ToString();
                    litProfileEmail.Text = reader["Email"].ToString();
                    litProfileUsername.Text = reader["Username"].ToString();
                    litProfileDepartment.Text = reader["DepartmentName"] != DBNull.Value ? reader["DepartmentName"].ToString() : "N/A";
                    litProfileJoined.Text = Convert.ToDateTime(reader["CreatedAt"]).ToString("MMM dd, yyyy");

                    string roleName = reader["RoleName"].ToString();
                    lblProfileRole.InnerText = roleName;
                    lblProfileRole.Attributes["class"] = GetRoleBadgeCss(roleName);
                }
                else
                {
                    Response.Redirect("~/Modules/Users/Users.aspx");
                }
            }
        }

        private void LoadUserStats()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                SqlCommand cmdTasksAssigned = new SqlCommand(
                    "SELECT COUNT(*) FROM TaskAssignments WHERE UserID = @UserID", con);
                cmdTasksAssigned.Parameters.AddWithValue("@UserID", profileUserID);
                litTasksAssigned.Text = cmdTasksAssigned.ExecuteScalar().ToString();

                SqlCommand cmdTasksCompleted = new SqlCommand(
                    @"SELECT COUNT(*) FROM TaskAssignments ta
                      INNER JOIN Tasks t ON ta.TaskID = t.TaskID
                      WHERE ta.UserID = @UserID AND t.Status = 'Completed'", con);
                cmdTasksCompleted.Parameters.AddWithValue("@UserID", profileUserID);
                litTasksCompleted.Text = cmdTasksCompleted.ExecuteScalar().ToString();

                SqlCommand cmdDecisions = new SqlCommand(
                    "SELECT COUNT(*) FROM Decisions WHERE CreatedBy = @UserID OR ResponsibleUserID = @UserID", con);
                cmdDecisions.Parameters.AddWithValue("@UserID", profileUserID);
                litDecisionsInvolved.Text = cmdDecisions.ExecuteScalar().ToString();

                SqlCommand cmdMeetings = new SqlCommand(
                    "SELECT COUNT(*) FROM MeetingParticipants WHERE UserID = @UserID", con);
                cmdMeetings.Parameters.AddWithValue("@UserID", profileUserID);
                litMeetingsAttended.Text = cmdMeetings.ExecuteScalar().ToString();
            }
        }

        private void LoadUserTasks()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
                    @"SELECT TOP 10 t.TaskTitle, t.Status, t.DueDate
                      FROM Tasks t
                      INNER JOIN TaskAssignments ta ON t.TaskID = ta.TaskID
                      WHERE ta.UserID = @UserID
                      ORDER BY t.DueDate DESC", con);
                cmd.Parameters.AddWithValue("@UserID", profileUserID);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                rptUserTasks.DataSource = dt;
                rptUserTasks.DataBind();
            }
        }

        private void LoadUserDecisions()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
                    @"SELECT TOP 10 d.DecisionTitle, d.Description, d.CreatedAt AS DecisionDate
                      FROM Decisions d
                      WHERE d.CreatedBy = @UserID OR d.ResponsibleUserID = @UserID
                      ORDER BY d.CreatedAt DESC", con);
                cmd.Parameters.AddWithValue("@UserID", profileUserID);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                rptUserDecisions.DataSource = dt;
                rptUserDecisions.DataBind();
            }
        }

        private void LoadUserLoginHistory()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
                    @"SELECT TOP 20
                          ll.LoginTime AS LoginDate,
                          ll.IPAddress,
                          CASE WHEN ll.LogoutTime IS NULL THEN N'Active' ELSE N'Success' END AS Status
                      FROM LoginLogs ll
                      WHERE ll.UserID = @UserID
                      ORDER BY ll.LoginTime DESC", con);
                cmd.Parameters.AddWithValue("@UserID", profileUserID);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                rptLoginHistory.DataSource = dt;
                rptLoginHistory.DataBind();
            }
        }

        public string FormatLoginTime(object value)
        {
            if (value == null || value == DBNull.Value)
                return "N/A";
            return Convert.ToDateTime(value).ToString("MMM dd, yyyy hh:mm tt");
        }

        public string GetLoginStatusCss(object status)
        {
            string value = Convert.ToString(status);
            if (string.Equals(value, "Success", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "Active", StringComparison.OrdinalIgnoreCase))
                return "bg-tertiary-container/20 text-tertiary px-2 py-0.5 rounded font-badge-cap text-badge-cap";
            return "bg-error-container/20 text-error px-2 py-0.5 rounded font-badge-cap text-badge-cap";
        }

        public string GetStatusBadgeCss(string status)
        {
            switch (status.ToLower())
            {
                case "completed":
                    return "bg-tertiary-container/20 text-tertiary px-2 py-0.5 rounded font-badge-cap text-badge-cap";
                case "inprogress":
                    return "bg-primary-container text-on-primary-container px-2 py-0.5 rounded font-badge-cap text-badge-cap";
                case "pending":
                    return "bg-surface-variant text-on-surface-variant px-2 py-0.5 rounded font-badge-cap text-badge-cap";
                default:
                    return "bg-outline/20 text-on-surface-variant px-2 py-0.5 rounded font-badge-cap text-badge-cap";
            }
        }

        public string GetRoleBadgeCss(string roleName)
        {
            switch (roleName.ToLower())
            {
                case "admin":
                    return "bg-primary-container text-on-primary-container px-2 py-0.5 rounded font-badge-cap text-badge-cap";
                case "faculty":
                    return "bg-secondary-container text-on-secondary-container px-2 py-0.5 rounded font-badge-cap text-badge-cap";
                case "student":
                    return "bg-tertiary-container text-on-tertiary-container px-2 py-0.5 rounded font-badge-cap text-badge-cap";
                case "staff":
                    return "bg-surface-variant text-on-surface-variant px-2 py-0.5 rounded font-badge-cap text-badge-cap";
                default:
                    return "bg-outline/20 text-on-surface-variant px-2 py-0.5 rounded font-badge-cap text-badge-cap";
            }
        }

    }
}
