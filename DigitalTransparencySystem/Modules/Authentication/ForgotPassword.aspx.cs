using System;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Authentication
{
    public partial class ForgotPassword : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            lblMessage.Style["display"] = "none";
        }

        protected void btnSendReset_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim().ToLowerInvariant();
            UserAccount user = AuthService.FindByEmail(email);

            pnlRequest.Visible = false;
            pnlOTP.Visible = true;

            if (user == null || AuthService.AccountBlockReason(user) != null)
            {
                lblOtpSentTo.Text = "If an account exists for that email, a verification code has been sent.";
                return;
            }

            Session["PendingUserID"] = user.UserID.ToString();
            Session["PendingEmail"] = user.Email;

            OtpIssueResult otp = EmailOtpService.Issue(user.UserID, user.Email, EmailOtpService.PurposeRecovery);
            lblOtpSentTo.Text = "Enter the code sent to " + EmailOtpService.MaskEmail(user.Email) + ", then choose a new password.";
            if (!otp.Sent && !string.IsNullOrEmpty(otp.Code))
                lblOtpSentTo.Text += " (Dev: " + otp.Code + ")";
            if (!string.IsNullOrEmpty(otp.Error) && string.IsNullOrEmpty(otp.Code))
                ShowError(otp.Error);
        }

        protected void btnVerifyReset_Click(object sender, EventArgs e)
        {
            int userId;
            if (!int.TryParse(Session["PendingUserID"] as string, out userId))
            {
                ShowError("No reset request is pending. Start again.");
                return;
            }

            if (txtNewPassword.Text.Length < 8)
            {
                ShowError("Password must be at least 8 characters.");
                return;
            }

            if (txtNewPassword.Text != txtConfirmPassword.Text)
            {
                ShowError("Passwords do not match.");
                return;
            }

            string error;
            if (!EmailOtpService.Verify(userId, txtOTP.Text.Trim(), EmailOtpService.PurposeRecovery, out error))
            {
                ShowError(error);
                return;
            }

            AuthService.UpdatePassword(userId, PasswordHasher.Hash(txtNewPassword.Text));
            AuthService.WriteAudit(userId, "PasswordReset", "User", userId, "Password reset with email OTP.", Request.UserHostAddress);
            Session.Remove("PendingUserID");
            Session.Remove("PendingEmail");

            pnlOTP.Visible = false;
            pnlSuccess.Visible = true;
        }

        protected void btnResendOTP_Click(object sender, EventArgs e)
        {
            int userId;
            string email = Session["PendingEmail"] as string;
            if (!int.TryParse(Session["PendingUserID"] as string, out userId) || string.IsNullOrEmpty(email))
                return;

            OtpIssueResult otp = EmailOtpService.Issue(userId, email, EmailOtpService.PurposeRecovery);
            if (!string.IsNullOrEmpty(otp.Error) && string.IsNullOrEmpty(otp.Code))
                ShowError(otp.Error);
            else if (otp.Sent)
                ShowError("A new code has been sent to your email.");
            else
                ShowError("A new code has been sent to your email. (Dev: " + otp.Code + ")");
        }

        private void ShowError(string message)
        {
            lblMessage.Text = message;
            lblMessage.Style["display"] = "flex";
        }
    }
}
