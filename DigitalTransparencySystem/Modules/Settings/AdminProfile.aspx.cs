using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Settings
{
    public partial class AdminProfile : System.Web.UI.Page
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

            HandleProfileImageUpload();
        }

        private void LoadProfile()
        {
            string userId = Session["UserID"] as string;
            if (string.IsNullOrEmpty(userId)) return;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    @"SELECT u.FullName, u.Username, u.Email, u.Phone, u.ProfileImage, u.DepartmentID, 
                             d.DepartmentName, r.RoleName
                      FROM Users u
                      LEFT JOIN Departments d ON u.DepartmentID = d.DepartmentID
                      LEFT JOIN Roles r ON u.RoleID = r.RoleID
                      WHERE u.UserID = @UserID", con);
                cmd.Parameters.AddWithValue("@UserID", userId);

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    string fullName = reader["FullName"].ToString();
                    txtFullName.Text = fullName;
                    txtUsername.Text = reader["Username"].ToString();
                    txtEmail.Text = reader["Email"].ToString();
                    txtPhone.Text = reader["Phone"] != DBNull.Value ? reader["Phone"].ToString() : "";
                    lblProfileName.Text = fullName;
                    lblProfileRole.Text = reader["RoleName"] != DBNull.Value ? reader["RoleName"].ToString() : "";
                    lblProfileEmail.Text = reader["Email"].ToString();

string initials = GetInitials(fullName);
lblProfileInitial.InnerText = initials;

                    string profileImage = reader["ProfileImage"] != DBNull.Value ? reader["ProfileImage"].ToString() : "";
                    if (!string.IsNullOrEmpty(profileImage))
                    {
                        if (!profileImage.StartsWith("~") && !profileImage.StartsWith("/"))
                            profileImage = "~/Uploads/ProfileImages/" + profileImage;
                        imgProfile.ImageUrl = profileImage;
                        imgProfile.Visible = true;
                        lblProfileInitial.Visible = false;
                        btnRemoveImage.Visible = true;
                    }
                    else
                    {
                        imgProfile.Visible = false;
                        lblProfileInitial.Visible = true;
                        btnRemoveImage.Visible = false;
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
                con.Open();
                SqlCommand cmd = new SqlCommand("SELECT DepartmentID, DepartmentName FROM Departments WHERE IsActive = 1 ORDER BY DepartmentName", con);
                SqlDataReader reader = cmd.ExecuteReader();

                ddlDepartment.Items.Clear();
                ddlDepartment.Items.Add(new System.Web.UI.WebControls.ListItem("-- Select Department --", ""));

                while (reader.Read())
                {
                    ddlDepartment.Items.Add(new System.Web.UI.WebControls.ListItem(
                        reader["DepartmentName"].ToString(),
                        reader["DepartmentID"].ToString()));
                }
                reader.Close();
            }
        }

        private void HandleProfileImageUpload()
        {
            if (fuProfileImage.HasFile)
            {
                try
                {
                    string userId = Session["UserID"] as string;
                    string ext = Path.GetExtension(fuProfileImage.FileName).ToLower();
                    string[] allowedExts = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

                    if (Array.IndexOf(allowedExts, ext) < 0)
                    {
                        ShowMessage("Only image files (JPG, PNG, GIF, WEBP) are allowed.", false);
                        return;
                    }

                    if (fuProfileImage.PostedFile.ContentLength > 5 * 1024 * 1024)
                    {
                        ShowMessage("Image size must be less than 5MB.", false);
                        return;
                    }

                    string uploadDir = Server.MapPath("~/Uploads/ProfileImages/");
                    if (!Directory.Exists(uploadDir))
                        Directory.CreateDirectory(uploadDir);

                    string fileName = $"{userId}_{Guid.NewGuid():N}{ext}";
                    string filePath = Path.Combine(uploadDir, fileName);
                    fuProfileImage.SaveAs(filePath);

                    using (SqlConnection con = new SqlConnection(connectionString))
                    {
                        con.Open();
                        SqlCommand cmd = new SqlCommand(
                            "UPDATE Users SET ProfileImage = @ProfileImage WHERE UserID = @UserID", con);
                        cmd.Parameters.AddWithValue("@ProfileImage", fileName);
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        cmd.ExecuteNonQuery();
                    }

                    Session["ProfileImage"] = fileName;
                    Session.Remove("ProfileImageUrl");
                    ShowMessage("Profile image updated successfully.", true);
                    LoadProfile();
                }
                catch (Exception ex)
                {
                    ShowMessage("Error uploading image: " + ex.Message, false);
                }
            }
        }

        protected void btnSaveProfile_Click(object sender, EventArgs e)
        {
            string userId = Session["UserID"] as string;
            if (string.IsNullOrEmpty(userId)) return;

            string fullName = txtFullName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string deptId = ddlDepartment.SelectedValue;

            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email))
            {
                ShowMessage("Full name and email are required.", false);
                return;
            }

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    SqlCommand checkCmd = new SqlCommand(
                        "SELECT COUNT(*) FROM Users WHERE Email = @Email AND UserID != @UserID", con);
                    checkCmd.Parameters.AddWithValue("@Email", email);
                    checkCmd.Parameters.AddWithValue("@UserID", userId);
                    int exists = (int)checkCmd.ExecuteScalar();

                    if (exists > 0)
                    {
                        ShowMessage("This email address is already in use by another account.", false);
                        return;
                    }

                    SqlCommand cmd = new SqlCommand(
                        @"UPDATE Users 
                          SET FullName = @FullName, Email = @Email, Phone = @Phone, 
                              DepartmentID = CASE WHEN @DepartmentID = '' THEN NULL ELSE @DepartmentID END
                          WHERE UserID = @UserID", con);
                    cmd.Parameters.AddWithValue("@FullName", fullName);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Phone", string.IsNullOrEmpty(phone) ? (object)DBNull.Value : phone);
                    cmd.Parameters.AddWithValue("@DepartmentID", deptId);
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.ExecuteNonQuery();
                }

                Session["FullName"] = fullName;
                Session["Email"] = email;

                ShowMessage("Profile updated successfully.", true);
                LoadProfile();
            }
            catch (Exception ex)
            {
                ShowMessage("Error updating profile: " + ex.Message, false);
            }
        }

        protected void btnChangePassword_Click(object sender, EventArgs e)
        {
            string userId = Session["UserID"] as string;
            if (string.IsNullOrEmpty(userId)) return;

            string currentPassword = txtCurrentPassword.Text.Trim();
            string newPassword = txtNewPassword.Text.Trim();
            string confirmPassword = txtConfirmPassword.Text.Trim();

            if (string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword))
            {
                ShowMessage("Please fill in all password fields.", false);
                return;
            }

            if (newPassword.Length < 8)
            {
                ShowMessage("New password must be at least 8 characters long.", false);
                return;
            }

            if (newPassword != confirmPassword)
            {
                ShowMessage("New password and confirmation do not match.", false);
                return;
            }

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    int id = Convert.ToInt32(userId);
                    UserAccount account = AuthService.FindById(id);
                    if (account == null || !PasswordHasher.Verify(currentPassword, account.Password))
                    {
                        ShowMessage("Current password is incorrect.", false);
                        return;
                    }

                    AuthService.UpdatePassword(id, PasswordHasher.Hash(newPassword));
                    AuthService.WriteAudit(id, "PasswordChanged", "User", id, "Admin changed password.", Request.UserHostAddress);
                }

                txtCurrentPassword.Text = "";
                txtNewPassword.Text = "";
                txtConfirmPassword.Text = "";

                ShowMessage("Password updated successfully.", true);
            }
            catch (Exception ex)
            {
                ShowMessage("Error updating password: " + ex.Message, false);
            }
        }

        protected void btnRemoveImage_Click(object sender, EventArgs e)
        {
            string userId = Session["UserID"] as string;
            if (string.IsNullOrEmpty(userId)) return;

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand(
                        "UPDATE Users SET ProfileImage = NULL WHERE UserID = @UserID", con);
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.ExecuteNonQuery();
                }

                Session.Remove("ProfileImage");
                Session.Remove("ProfileImageUrl");
                ShowMessage("Profile image removed.", true);
                LoadProfile();
            }
            catch (Exception ex)
            {
                ShowMessage("Error removing image: " + ex.Message, false);
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

        private void ShowMessage(string message, bool isSuccess)
        {
            pnlMessage.Visible = true;
            lblMessage.Text = message;
            pnlMessage.CssClass = isSuccess
                ? UiNotice.ToastSuccess + " dtas-notice-rich"
                : UiNotice.StickyDanger + " dtas-notice-rich";
            divMessage.Attributes["class"] = "flex items-start gap-3";
            msgIcon.InnerText = isSuccess ? "check_circle" : "error";
        }
    }
}
