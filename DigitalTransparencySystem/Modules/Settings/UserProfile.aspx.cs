using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Settings
{
    public partial class UserProfile : System.Web.UI.Page
    {
        private string connectionString;

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
                LoadProfile();
                LoadDepartments();
            }
        }

        private void LoadProfile()
        {
            string userId = Session["UserID"].ToString();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    @"SELECT u.*, d.DepartmentName 
                      FROM Users u
                      LEFT JOIN Departments d ON u.DepartmentID = d.DepartmentID
                      WHERE u.UserID = @UserID", con);
                cmd.Parameters.AddWithValue("@UserID", userId);

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    string fullName = reader["FullName"].ToString();
                    string email = reader["Email"].ToString();
                    int roleID = Convert.ToInt32(reader["RoleID"]);
                    string role = GetRoleName(roleID);
                    string phone = reader["Phone"] != DBNull.Value ? reader["Phone"].ToString() : "";
                    string department = reader["DepartmentName"] != DBNull.Value ? reader["DepartmentName"].ToString() : "Not assigned";
                    string profileImage = reader["ProfileImage"] != DBNull.Value ? reader["ProfileImage"].ToString() : "";
                    DateTime joinDate = Convert.ToDateTime(reader["CreatedAt"]);

                    litFullName.Text = Server.HtmlEncode(fullName);
                    litEmail.Text = Server.HtmlEncode(email);
                    litPhone.Text = Server.HtmlEncode(phone);
                    litDepartment.Text = Server.HtmlEncode(department);
                    litJoinDate.Text = joinDate.ToString("MMMM yyyy");

                    spanRole.InnerText = role;
                    spanRole.Attributes["class"] = "badge-status-" + GetRoleBadgeClass(role);

                    txtFullName.Text = fullName;
                    txtEmail.Text = email;
                    txtPhone.Text = phone;

                    string initials = GetInitials(fullName);
                    lblInitials.InnerText = initials;
                    lblUploadInitials.InnerText = initials;

                    if (!string.IsNullOrEmpty(profileImage))
                    {
                        string imgPath = profileImage.StartsWith("~") || profileImage.StartsWith("/")
                            ? profileImage : "~/Uploads/ProfileImages/" + profileImage;
                        imgProfile.ImageUrl = imgPath;
                        imgProfileUpload.ImageUrl = imgPath;
                        imgProfile.Visible = true;
                        imgProfileUpload.Visible = true;
                        lblInitials.Visible = false;
                        lblUploadInitials.Visible = false;
                    }

                    if (reader["DepartmentID"] != DBNull.Value)
                    {
                        ddlDepartment.SelectedValue = reader["DepartmentID"].ToString();
                    }
                }
                reader.Close();
            }
        }

        private void LoadDepartments()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("SELECT DepartmentID, DepartmentName FROM Departments WHERE IsActive = 1", con);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                ddlDepartment.DataSource = dt;
                ddlDepartment.DataTextField = "DepartmentName";
                ddlDepartment.DataValueField = "DepartmentID";
                ddlDepartment.DataBind();
                ddlDepartment.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Select Department", ""));
            }
        }

        private string GetRoleName(int roleID)
        {
            switch (roleID)
            {
                case 1: return "Admin";
                case 2: return "Faculty";
                case 3: return "Student";
                case 4: return "Staff";
                default: return "User";
            }
        }

        private string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "?";
            string[] parts = fullName.Trim().Split(' ');
            if (parts.Length >= 2)
                return (parts[0][0].ToString() + parts[parts.Length - 1][0].ToString()).ToUpper();
            return parts[0][0].ToString().ToUpper();
        }

        private string GetRoleBadgeClass(string role)
        {
            switch (role?.ToLower())
            {
                case "admin": return "scheduled";
                case "faculty": return "completed";
                case "student": return "pending";
                case "staff": return "inprogress";
                default: return "pending";
            }
        }

        protected void btnSaveProfile_Click(object sender, EventArgs e)
        {
            string userId = Session["UserID"].ToString();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    @"UPDATE Users 
                      SET FullName = @FullName, Phone = @Phone, DepartmentID = @DepartmentID
                      WHERE UserID = @UserID", con);
                cmd.Parameters.AddWithValue("@FullName", txtFullName.Text.Trim());
                cmd.Parameters.AddWithValue("@Phone", txtPhone.Text.Trim());
                cmd.Parameters.AddWithValue("@DepartmentID", 
                    string.IsNullOrEmpty(ddlDepartment.SelectedValue) ? (object)DBNull.Value : ddlDepartment.SelectedValue);
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.ExecuteNonQuery();
            }

            Session["FullName"] = txtFullName.Text.Trim();
            lblProfileSuccess.Visible = true;
            lblProfileSuccess.CssClass = "text-tertiary font-label-md";
            LoadProfile();
        }

        protected void btnChangePassword_Click(object sender, EventArgs e)
        {
            lblPasswordError.Visible = false;
            lblPasswordSuccess.Visible = false;

            if (string.IsNullOrEmpty(txtCurrentPassword.Text) || string.IsNullOrEmpty(txtNewPassword.Text))
            {
                lblPasswordError.Text = "Please fill in all password fields.";
                lblPasswordError.Visible = true;
                return;
            }

            if (txtNewPassword.Text != txtConfirmPassword.Text)
            {
                lblPasswordError.Text = "New passwords do not match.";
                lblPasswordError.Visible = true;
                return;
            }

            if (txtNewPassword.Text.Length < 8)
            {
                lblPasswordError.Text = "Password must be at least 8 characters.";
                lblPasswordError.Visible = true;
                return;
            }

            int userId = Convert.ToInt32(Session["UserID"]);
            UserAccount user = AuthService.FindById(userId);
            if (user == null || !PasswordHasher.Verify(txtCurrentPassword.Text, user.Password))
            {
                lblPasswordError.Text = "Current password is incorrect.";
                lblPasswordError.Visible = true;
                return;
            }

            AuthService.UpdatePassword(userId, PasswordHasher.Hash(txtNewPassword.Text));
            AuthService.WriteAudit(userId, "PasswordChanged", "User", userId, "User changed password.", Request.UserHostAddress);

            lblPasswordSuccess.Visible = true;
            lblPasswordSuccess.CssClass = "text-tertiary font-label-md";
            txtCurrentPassword.Text = "";
            txtNewPassword.Text = "";
            txtConfirmPassword.Text = "";
        }

        protected void btnUploadImage_Click(object sender, EventArgs e)
        {
            if (!fuProfileImage.HasFile) return;

            string uploadDir = Server.MapPath("~/Uploads/ProfileImages/");
            if (!Directory.Exists(uploadDir))
                Directory.CreateDirectory(uploadDir);

            string fileName = Path.GetFileName(fuProfileImage.FileName);
            string filePath = DateTime.Now.Ticks + "_" + fileName;
            fuProfileImage.SaveAs(Server.MapPath("~/Uploads/ProfileImages/" + filePath));

            string userId = Session["UserID"].ToString();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    "UPDATE Users SET ProfileImage = @ProfileImage WHERE UserID = @UserID", con);
                cmd.Parameters.AddWithValue("@ProfileImage", filePath);
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.ExecuteNonQuery();
            }

            LoadProfile();
        }

        protected void btnSendEmailCode_Click(object sender, EventArgs e)
        {
            lblEmailChangeMessage.Text = "";
            string newEmail = (txtNewEmail.Text ?? "").Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(newEmail) || newEmail.IndexOf('@') < 1)
            {
                lblEmailChangeMessage.CssClass = "text-error font-label-md";
                lblEmailChangeMessage.Text = "Enter a valid email address.";
                return;
            }

            UserAccount existing = AuthService.FindByEmail(newEmail);
            int userId = Convert.ToInt32(Session["UserID"]);
            if (existing != null && existing.UserID != userId)
            {
                lblEmailChangeMessage.CssClass = "text-error font-label-md";
                lblEmailChangeMessage.Text = "That email is already in use.";
                return;
            }

            if (AuthService.IsBanned(newEmail, null))
            {
                lblEmailChangeMessage.CssClass = "text-error font-label-md";
                lblEmailChangeMessage.Text = "That email cannot be used.";
                return;
            }

            Session["PendingEmailChange"] = newEmail;
            OtpIssueResult otp = EmailOtpService.Issue(userId, newEmail, EmailOtpService.PurposeEmailChange);
            pnlEmailOtp.Visible = true;
            lblEmailOtpSent.Text = "A code was sent to " + EmailOtpService.MaskEmail(newEmail) + ".";
            if (!otp.Sent && !string.IsNullOrEmpty(otp.Code))
                lblEmailOtpSent.Text += " (Dev: " + otp.Code + ")";
            if (!string.IsNullOrEmpty(otp.Error) && string.IsNullOrEmpty(otp.Code))
            {
                lblEmailChangeMessage.CssClass = "text-error font-label-md";
                lblEmailChangeMessage.Text = otp.Error;
            }
        }

        protected void btnConfirmEmail_Click(object sender, EventArgs e)
        {
            int userId = Convert.ToInt32(Session["UserID"]);
            string newEmail = Session["PendingEmailChange"] as string;
            if (string.IsNullOrEmpty(newEmail))
            {
                lblEmailChangeMessage.CssClass = "text-error font-label-md";
                lblEmailChangeMessage.Text = "Request a code first.";
                return;
            }

            string error;
            if (!EmailOtpService.Verify(userId, txtEmailOtp.Text.Trim(), EmailOtpService.PurposeEmailChange, out error))
            {
                lblEmailChangeMessage.CssClass = "text-error font-label-md";
                lblEmailChangeMessage.Text = error;
                return;
            }

            AuthService.ChangeEmail(userId, newEmail);
            Session["Email"] = newEmail;
            Session.Remove("PendingEmailChange");
            AuthService.WriteAudit(userId, "EmailChanged", "User", userId, "Email changed after OTP.", Request.UserHostAddress);
            lblEmailChangeMessage.CssClass = "text-tertiary font-label-md";
            lblEmailChangeMessage.Text = "Email updated.";
            pnlEmailOtp.Visible = false;
            txtNewEmail.Text = "";
            LoadProfile();
        }
    }
}
