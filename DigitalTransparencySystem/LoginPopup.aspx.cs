using System;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem
{
    public partial class LoginPopup : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            AuthService.RememberReturnUrl(Session, Request.QueryString["next"]);
            if (!IsPostBack)
            {
                string google = Request.QueryString["google"];
                string reason = Request.QueryString["reason"];
                if (!string.IsNullOrEmpty(google))
                    ShowError(AuthService.FriendlyGoogleError(google));
                else if (string.Equals(reason, "banned", StringComparison.OrdinalIgnoreCase))
                    ShowError("This account has been banned. You cannot sign in with this email.");
                else if (string.Equals(reason, "inactive", StringComparison.OrdinalIgnoreCase))
                    ShowError("This account is no longer active.");

                if (Session["UserID"] != null)
                    RedirectByRole(Session["Role"] as string);
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            LoginAttemptResult result = AuthService.TryPasswordLogin(txtUsername.Text.Trim(), txtPassword.Text);
            if (result.User == null || (!result.Success && !result.NeedsEmailOtp))
            {
                ShowError(result.Error ?? "Invalid username or password.");
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
            RedirectByRole(result.User.Role);
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
            Session.Remove("PendingUserID");
            Session.Remove("PendingEmail");
            AuthService.CompleteLogin(Session, Request, user);
            RedirectByRole(user.Role);
        }

        protected void btnResendOTP_Click(object sender, EventArgs e)
        {
            int userId;
            string pendingEmail = Session["PendingEmail"] as string;
            if (!int.TryParse(Session["PendingUserID"] as string, out userId) || pendingEmail == null)
                return;

            OtpIssueResult otp = EmailOtpService.Issue(userId, pendingEmail, EmailOtpService.PurposeRegister);
            if (!string.IsNullOrEmpty(otp.Error) && string.IsNullOrEmpty(otp.Code))
                ShowError(otp.Error);
            else if (otp.Sent)
                ShowError("A new code has been sent to your email.");
            else
                ShowError("A new code has been sent to your email. (Dev: " + otp.Code + ")");
        }

        protected void btnGoogleLogin_Click(object sender, EventArgs e)
        {
            GoogleAuthService.StartFromPage(Context, Request.QueryString["embed"] == "1", ShowError);
        }

        private void RedirectByRole(string role)
        {
            string url = ResolveUrl(AuthService.ConsumeReturnUrl(Session, AuthService.DashboardUrl(role)));

            if (Request.QueryString["embed"] == "1")
            {
                Session["AuthRedirect"] = url;
                string script = @"(function(){try{if(window.parent&&window.parent!==window){window.parent.postMessage({type:'auth-redirect',url:'" + url.Replace("'", "\\'") + "'},'*');}else{window.location.href='" + url.Replace("'", "\\'") + "';}}catch(e){window.location.href='" + url.Replace("'", "\\'") + "';}})();";
                ClientScript.RegisterStartupScript(GetType(), "authRedirect", script, true);
            }
            else
            {
                Response.Redirect(url);
            }
        }

        private void ShowError(string message)
        {
            lblMessage.Text = message;
            lblMessage.Style["display"] = "block";
        }
    }
}
