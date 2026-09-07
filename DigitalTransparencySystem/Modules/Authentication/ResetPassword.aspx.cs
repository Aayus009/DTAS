using System;
using System.Configuration;
using System.Data.SqlClient;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Authentication
{
    public partial class ResetPassword : System.Web.UI.Page
    {
        private string _token;

        protected void Page_Load(object sender, EventArgs e)
        {
            lblMessage.Style["display"] = "none";
            _token = Request.QueryString["token"];

            if (!IsPostBack)
            {
                if (string.IsNullOrEmpty(_token) || !IsTokenValid(_token))
                {
                    pnlValid.Visible = false;
                    pnlInvalid.Visible = true;
                }
                else
                {
                    pnlValid.Visible = true;
                    txtNewPassword.Focus();
                }
            }
        }

        protected void btnResetPassword_Click(object sender, EventArgs e)
        {
            _token = ViewState["Token"] as string;
            if (string.IsNullOrEmpty(_token))
                _token = Request.QueryString["token"];

            if (string.IsNullOrEmpty(_token) || !IsTokenValid(_token))
            {
                pnlValid.Visible = false;
                pnlInvalid.Visible = true;
                return;
            }

            string userId = GetTokenUserID(_token);
            if (userId == null)
            {
                pnlValid.Visible = false;
                pnlInvalid.Visible = true;
                return;
            }

            if (txtNewPassword.Text.Length < 8)
            {
                lblMessage.Text = "Password must be at least 8 characters.";
                lblMessage.Style["display"] = "flex";
                return;
            }

            AuthService.UpdatePassword(Convert.ToInt32(userId), PasswordHasher.Hash(txtNewPassword.Text));

            string connectionString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
            using (var con = new SqlConnection(connectionString))
            using (var tokenCmd = new SqlCommand("UPDATE PasswordResetTokens SET IsUsed = 1 WHERE Token = @Token", con))
            {
                tokenCmd.Parameters.AddWithValue("@Token", _token);
                con.Open();
                tokenCmd.ExecuteNonQuery();
            }

            AuthService.WriteAudit(Convert.ToInt32(userId), "PasswordReset", "User", Convert.ToInt32(userId), "Password reset with emailed link.", Request.UserHostAddress);

            pnlValid.Visible = false;
            pnlSuccess.Visible = true;
        }

        private bool IsTokenValid(string token)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;

            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(
                @"SELECT COUNT(1) FROM PasswordResetTokens
                  WHERE Token = @Token AND IsUsed = 0 AND ExpiresAt > @Now", con))
            {
                cmd.Parameters.AddWithValue("@Token", token);
                cmd.Parameters.AddWithValue("@Now", DateTime.UtcNow);
                con.Open();
                int count = (int)cmd.ExecuteScalar();
                ViewState["Token"] = token;
                return count > 0;
            }
        }

        private string GetTokenUserID(string token)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;

            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(
                @"SELECT UserID FROM PasswordResetTokens
                  WHERE Token = @Token AND IsUsed = 0 AND ExpiresAt > @Now", con))
            {
                cmd.Parameters.AddWithValue("@Token", token);
                con.Open();
                object result = cmd.ExecuteScalar();
                return result != null ? result.ToString() : null;
            }
        }
    }
}
