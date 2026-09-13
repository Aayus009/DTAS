using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI.WebControls;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Users
{
    public partial class Users : System.Web.UI.Page
    {
        private string connectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            connectionString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;

            ModerationService.EnsureSchema();

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

                LoadRoles();
                LoadDepartments();
                LoadStats();
                LoadUsers();
            }
        }

        private void LoadRoles()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("SELECT RoleID, RoleName FROM Roles ORDER BY RoleID", con);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                ddlCreateRole.DataSource = reader;
                ddlCreateRole.DataTextField = "RoleName";
                ddlCreateRole.DataValueField = "RoleID";
                ddlCreateRole.DataBind();
                ddlCreateRole.Items.Insert(0, new ListItem("Select Role", "0"));
            }
        }

        private void LoadDepartments()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("SELECT DepartmentID, DepartmentName FROM Departments ORDER BY DepartmentName", con);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                ddlDepartment.DataSource = reader;
                ddlDepartment.DataTextField = "DepartmentName";
                ddlDepartment.DataValueField = "DepartmentID";
                ddlDepartment.DataBind();
                ddlDepartment.Items.Insert(0, new ListItem("Select Department", "0"));
            }
        }

        private void LoadStats()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                SqlCommand cmdTotal = new SqlCommand("SELECT COUNT(*) FROM Users", con);
                litTotalUsers.Text = cmdTotal.ExecuteScalar().ToString();

                SqlCommand cmdAdmins = new SqlCommand("SELECT COUNT(*) FROM Users WHERE RoleID = 1", con);
                litAdmins.Text = cmdAdmins.ExecuteScalar().ToString();

                SqlCommand cmdFacultyStaff = new SqlCommand("SELECT COUNT(*) FROM Users WHERE RoleID IN (2, 4)", con);
                litFacultyStaff.Text = cmdFacultyStaff.ExecuteScalar().ToString();

                SqlCommand cmdStudents = new SqlCommand("SELECT COUNT(*) FROM Users WHERE RoleID = 3", con);
                litStudents.Text = cmdStudents.ExecuteScalar().ToString();
            }
        }

        private void LoadUsers()
        {
            string search = txtSearch.Text.Trim();
            string roleFilter = ddlRoleFilter.SelectedValue;

            StringBuilder query = new StringBuilder();
            query.Append(@"SELECT u.UserID, u.FullName, u.Email, u.Username, u.IsActive, u.CreatedAt,
                            u.RoleID, ISNULL(u.AccountStatus, N'Active') AS AccountStatus,
                            r.RoleName, d.DepartmentName, s.EndDate AS SuspensionEnd
                           FROM Users u
                           LEFT JOIN Roles r ON u.RoleID = r.RoleID
                           LEFT JOIN Departments d ON u.DepartmentID = d.DepartmentID
                           LEFT JOIN Suspensions s ON s.UserID = u.UserID AND s.IsActive = 1 AND s.EndDate > GETDATE()
                           WHERE ISNULL(u.IsDeleted, 0) = 0");

            if (!string.IsNullOrEmpty(search))
            {
                query.Append(" AND (u.FullName LIKE @Search OR u.Email LIKE @Search)");
            }

            if (roleFilter != "0")
            {
                query.Append(" AND u.RoleID = @RoleID");
            }

            string statusFilter = ddlStatusFilter.SelectedValue;
            if (statusFilter == "Inactive")
                query.Append(" AND u.IsActive = 0 AND ISNULL(u.AccountStatus, N'Active') <> N'Banned'");
            else if (statusFilter != "All")
                query.Append(" AND ISNULL(u.AccountStatus, N'Active') = @AccountStatus");

            query.Append(" ORDER BY u.CreatedAt DESC");

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query.ToString(), con);

                if (!string.IsNullOrEmpty(search))
                    cmd.Parameters.AddWithValue("@Search", "%" + search + "%");

                if (roleFilter != "0")
                    cmd.Parameters.AddWithValue("@RoleID", Convert.ToInt32(roleFilter));

                if (statusFilter != "All" && statusFilter != "Inactive")
                    cmd.Parameters.AddWithValue("@AccountStatus", statusFilter);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                rptUsers.DataSource = dt;
                rptUsers.DataBind();
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            LoadUsers();
        }

        protected void ddlRoleFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadUsers();
        }

        protected void ddlStatusFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadUsers();
        }

        protected void rptUsers_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int userID;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out userID))
                return;

            int adminId = Convert.ToInt32(Session["UserID"]);
            string error = null;

            switch (e.CommandName)
            {
                case "ToggleStatus":
                    error = ToggleUserStatus(userID, adminId);
                    break;
                case "BeginSuspend":
                    OpenModerationForm(userID, false);
                    return;
                case "BeginBan":
                    OpenModerationForm(userID, true);
                    return;
                case "Unsuspend":
                    error = SafeModerate(() => ModerationService.Unsuspend(userID, adminId));
                    break;
                case "Unban":
                    error = SafeModerate(() => ModerationService.Unban(userID, adminId));
                    break;
            }

            ShowModeration(error, e.CommandName);
            LoadStats();
            LoadUsers();
        }

        protected void btnConfirmSuspend_Click(object sender, EventArgs e)
        {
            int userId;
            int days;
            if (!int.TryParse(hfTargetUserId.Value, out userId))
                return;
            if (!int.TryParse(txtSuspendDays.Text.Trim(), out days))
                days = 7;
            string error = SafeModerate(() => ModerationService.Suspend(userId, Convert.ToInt32(Session["UserID"]), txtModerationReason.Text, days));
            CloseModerationForm();
            ShowModeration(error, "Suspend");
            LoadStats();
            LoadUsers();
        }

        protected void btnConfirmBan_Click(object sender, EventArgs e)
        {
            int userId;
            if (!int.TryParse(hfTargetUserId.Value, out userId))
                return;
            string error = SafeModerate(() => ModerationService.Ban(userId, Convert.ToInt32(Session["UserID"]), txtModerationReason.Text));
            CloseModerationForm();
            ShowModeration(error, "Ban");
            LoadStats();
            LoadUsers();
        }

        protected void btnCancelModeration_Click(object sender, EventArgs e)
        {
            CloseModerationForm();
        }

        private void OpenModerationForm(int userId, bool ban)
        {
            UserAccount target = AuthService.FindById(userId);
            hfTargetUserId.Value = userId.ToString();
            litModerationName.Text = target == null ? ("User #" + userId) : (target.FullName + " (" + target.Email + ")");
            litModerationTitle.Text = ban ? "Ban this account" : "Suspend this account";
            txtModerationReason.Text = "";
            txtSuspendDays.Text = "7";
            pnlSuspendDays.Visible = !ban;
            btnConfirmSuspend.Visible = !ban;
            btnConfirmBan.Visible = ban;
            pnlModerationForm.Visible = true;
            lblModeration.Text = "";
        }

        private void CloseModerationForm()
        {
            pnlModerationForm.Visible = false;
            hfTargetUserId.Value = "";
            txtModerationReason.Text = "";
        }

        private static string SafeModerate(Func<string> action)
        {
            try
            {
                return action();
            }
            catch (Exception ex)
            {
                return "The action could not be saved: " + ex.Message;
            }
        }

        private string ToggleUserStatus(int userID, int adminId)
        {
            string block = ModerationService.CanModerate(userID, adminId);
            if (block != null)
                return block;

            UserAccount target = AuthService.FindById(userID);
            if (target != null && string.Equals(target.AccountStatus, "Banned", StringComparison.OrdinalIgnoreCase))
                return "Unban this account instead of toggling active status.";
            if (target != null && string.Equals(target.AccountStatus, "Suspended", StringComparison.OrdinalIgnoreCase))
                return "Unsuspend this account instead of toggling active status.";

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
                    "UPDATE Users SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END WHERE UserID = @UserID", con);
                cmd.Parameters.AddWithValue("@UserID", userID);
                con.Open();
                cmd.ExecuteNonQuery();
            }

            AuthService.WriteAudit(adminId, "UserActiveToggled", "User", userID, "IsActive toggled.", null);
            return null;
        }

        protected void btnShowCreateUser_Click(object sender, EventArgs e)
        {
            pnlCreateUser.Visible = true;
            pnlMessage.Visible = false;
        }

        protected void btnClosePanel_Click(object sender, EventArgs e)
        {
            pnlCreateUser.Visible = false;
            ClearCreateForm();
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            pnlCreateUser.Visible = false;
            ClearCreateForm();
        }

        protected void btnCreateUser_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            string fullName = txtFullName.Text.Trim();
            string email = txtEmail.Text.Trim().ToLower();
            string username = email.Split('@')[0];
            int roleID = Convert.ToInt32(ddlCreateRole.SelectedValue);
            int departmentID = Convert.ToInt32(ddlDepartment.SelectedValue);
            string password = txtPassword.Text;

            if (AuthService.IsBanned(email, null))
            {
                ShowMessage("This email cannot be used to create an account.", false);
                return;
            }

            if (EmailExists(email))
            {
                ShowMessage("An account with this email already exists.", false);
                return;
            }

            string passwordHash = PasswordHasher.Hash(password);
            username = AuthService.AllocateUsername(username);

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"INSERT INTO Users (FullName, Email, Username, Password, RoleID, DepartmentID, IsActive, CreatedAt,
                                 EmailVerified, IdentityVerified, VerificationStatus, AccountStatus, IsDeleted)
                                 VALUES (@FullName, @Email, @Username, @Password, @RoleID, @DepartmentID, 1, GETDATE(),
                                 1, 0, N'None', N'Active', 0)";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@FullName", fullName);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@Password", passwordHash);
                cmd.Parameters.AddWithValue("@RoleID", roleID);
                cmd.Parameters.AddWithValue("@DepartmentID", departmentID == 0 ? (object)DBNull.Value : departmentID);

                con.Open();
                int result = cmd.ExecuteNonQuery();

                if (result > 0)
                {
                    ShowMessage("User created successfully!", true);
                    ClearCreateForm();
                    LoadStats();
                    LoadUsers();
                }
                else
                {
                    ShowMessage("An error occurred while creating the user.", false);
                }
            }
        }

        private bool EmailExists(string email)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("SELECT COUNT(1) FROM Users WHERE Email = @Email", con);
                cmd.Parameters.AddWithValue("@Email", email);
                con.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private void ShowMessage(string message, bool isSuccess)
        {
            UiNotice.BindPanel(pnlMessage, lblMessage, message, isSuccess);
        }

        private void ClearCreateForm()
        {
            txtFullName.Text = string.Empty;
            txtEmail.Text = string.Empty;
            txtPassword.Text = string.Empty;
            ddlCreateRole.SelectedIndex = 0;
            ddlDepartment.SelectedIndex = 0;
            pnlMessage.Visible = false;
        }

        public string GetAccountStatusLabel(object status, object isActive)
        {
            return FormatUserStatus(status, isActive, null);
        }

        public string FormatUserStatus(object status, object isActive, object suspensionEnd)
        {
            string account = Convert.ToString(status);
            if (string.Equals(account, "Banned", StringComparison.OrdinalIgnoreCase))
                return "Banned";
            if (string.Equals(account, "Suspended", StringComparison.OrdinalIgnoreCase))
            {
                if (suspensionEnd != null && suspensionEnd != DBNull.Value)
                    return "Suspended until " + Convert.ToDateTime(suspensionEnd).ToString("MMM dd, yyyy");
                return "Suspended";
            }
            if (!Convert.ToBoolean(isActive))
                return "Inactive";
            return "Active";
        }

        public string GetAccountStatusCss(object status, object isActive)
        {
            string label = GetAccountStatusLabel(status, isActive);
            if (label == "Active")
                return "bg-tertiary-container/20 text-tertiary px-2 py-0.5 rounded font-badge-cap text-badge-cap";
            if (label == "Suspended")
                return "bg-secondary-container/40 text-on-secondary-container px-2 py-0.5 rounded font-badge-cap text-badge-cap";
            return "bg-error-container/20 text-error px-2 py-0.5 rounded font-badge-cap text-badge-cap";
        }

        public bool ShowSuspend(object userId, object roleId, object status)
        {
            return CanAct(userId, roleId) && !IsStatus(status, "Suspended") && !IsStatus(status, "Banned");
        }

        public bool ShowBan(object userId, object roleId, object status)
        {
            return CanAct(userId, roleId) && !IsStatus(status, "Banned");
        }

        public bool ShowUnsuspend(object status)
        {
            return IsStatus(status, "Suspended");
        }

        public bool ShowUnban(object status)
        {
            return IsStatus(status, "Banned");
        }

        public bool ShowToggle(object userId, object roleId, object status)
        {
            return CanAct(userId, roleId) && !IsStatus(status, "Banned") && !IsStatus(status, "Suspended");
        }

        private bool CanAct(object userId, object roleId)
        {
            int current = Session["UserID"] == null ? 0 : Convert.ToInt32(Session["UserID"]);
            return Convert.ToInt32(userId) != current && Convert.ToInt32(roleId) != 1;
        }

        private static bool IsStatus(object status, string expected)
        {
            return string.Equals(Convert.ToString(status), expected, StringComparison.OrdinalIgnoreCase);
        }

        private void ShowModeration(string error, string action)
        {
            if (error != null)
            {
                lblModeration.CssClass = "font-label-md block px-6 pt-4 text-error";
                lblModeration.Text = error;
                return;
            }

            lblModeration.CssClass = "font-label-md block px-6 pt-4 text-tertiary";
            switch (action)
            {
                case "Suspend": lblModeration.Text = "Account suspended."; break;
                case "Unsuspend": lblModeration.Text = "Suspension lifted."; break;
                case "Ban": lblModeration.Text = "Account banned."; break;
                case "Unban": lblModeration.Text = "Ban lifted."; break;
                case "ToggleStatus": lblModeration.Text = "Account active flag updated."; break;
                default: lblModeration.Text = ""; break;
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
