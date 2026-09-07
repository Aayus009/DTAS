using System;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Authentication
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] != null)
            {
                Response.Redirect(AuthService.DashboardUrl(Session["Role"] as string));
                return;
            }

            Response.Redirect(AuthService.PopupSignInUrl(
                Request.QueryString["next"],
                Request.QueryString["reason"],
                Request.QueryString["google"]));
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            LoginAttemptResult result = AuthService.TryPasswordLogin(txtUsername.Text.Trim(), txtPassword.Text);
            if (result.User == null)
            {
                ShowError(result.Error ?? "Invalid username or password.");
                return;
            }

            if (!string.IsNullOrEmpty(result.Error) && !result.NeedsEmailOtp)
            {
                ShowError(result.Error);
                return;
            }

            if (result.NeedsEmailOtp)
            {
                Session["PendingUserID"] = result.User.UserID.ToString();
                Session["PendingEmail"] = result.User.Email;
                pnlLogin.Visible = false;
                pnlOTP.Visible = true;
                lblOtpSentTo.Text = "Confirm your email to finish setting up this account. A code was sent to "
                    + EmailOtpService.MaskEmail(result.User.Email) + ".";
                if (!string.IsNullOrEmpty(result.DevOtp))
                    lblOtpSentTo.Text += " (Dev: " + result.DevOtp + ")";
                return;
            }

            AuthService.CompleteLogin(Session, Request, result.User);
            Response.Redirect(AuthService.ConsumeReturnUrl(Session, AuthService.DashboardUrl(result.User.Role)));
        }

        protected void btnVerifyOTP_Click(object sender, EventArgs e)
        {
            int userId;
            if (!int.TryParse(Session["PendingUserID"] as string, out userId))
            {
                ShowError("Session expired. Please sign in again.");
                pnlOTP.Visible = false;
                pnlLogin.Visible = true;
                return;
            }

            string error;
            if (!EmailOtpService.Verify(userId, txtOTP.Text.Trim(), EmailOtpService.PurposeRegister, out error))
            {
                ShowError(error);
                return;
            }

            AuthService.MarkEmailVerified(userId);
            UserAccount user = AuthService.FindById(userId);
            AuthService.WriteAudit(userId, "EmailVerified", "User", userId, "Email verified at sign-in.", Request.UserHostAddress);
            Session.Remove("PendingUserID");
            Session.Remove("PendingEmail");
            AuthService.CompleteLogin(Session, Request, user);
            Response.Redirect(AuthService.ConsumeReturnUrl(Session, AuthService.DashboardUrl(user.Role)));
        }

        protected void btnResendOTP_Click(object sender, EventArgs e)
        {
            int userId;
            string email = Session["PendingEmail"] as string;
            if (!int.TryParse(Session["PendingUserID"] as string, out userId) || string.IsNullOrEmpty(email))
                return;

            OtpIssueResult otp = EmailOtpService.Issue(userId, email, EmailOtpService.PurposeRegister);
            if (!string.IsNullOrEmpty(otp.Error) && string.IsNullOrEmpty(otp.Code))
                ShowError(otp.Error);
            else if (otp.Sent)
                ShowError("A new code has been sent to your email.");
            else
                ShowError("A new code has been sent to your email. (Dev: " + otp.Code + ")");
        }

        protected void btnGoogleLogin_Click(object sender, EventArgs e)
        {
            GoogleAuthService.StartFromPage(Context, false, ShowError);
        }

        private void ShowError(string message)
        {
            lblMessage.Text = message;
            lblMessage.Style["display"] = "flex";
        }
    }
}
