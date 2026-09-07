using System;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Authentication
{
    public partial class VerifyEmail : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                int userId;
                if (int.TryParse(Session["UserID"] as string ?? Session["PendingUserID"] as string, out userId))
                {
                    UserAccount user = AuthService.FindById(userId);
                    if (user != null && user.EmailVerified && Session["UserID"] != null)
                    {
                        Response.Redirect(AuthService.DashboardUrl(user.Role));
                        return;
                    }

                    if (user != null)
                    {
                        Session["PendingUserID"] = user.UserID.ToString();
                        Session["PendingEmail"] = user.Email;
                        txtEmail.Text = user.Email;
                    }
                }
            }
        }

        protected void btnSendCode_Click(object sender, EventArgs e)
        {
            UserAccount user = AuthService.FindByEmail(txtEmail.Text.Trim());
            if (user == null)
            {
                ShowError("If that email is registered, a code has been sent.");
                return;
            }

            if (user.EmailVerified)
            {
                ShowError("That email is already verified. You can sign in with your password.");
                return;
            }

            if (AuthService.AccountBlockReason(user) != null)
            {
                ShowError("This account cannot be verified.");
                return;
            }

            OtpIssueResult otp = EmailOtpService.Issue(user.UserID, user.Email, EmailOtpService.PurposeRegister);
            Session["PendingUserID"] = user.UserID.ToString();
            Session["PendingEmail"] = user.Email;
            pnlEmail.Visible = false;
            pnlOTP.Visible = true;
            lblOtpSentTo.Text = "A code was sent to " + EmailOtpService.MaskEmail(user.Email) + ".";
            if (!otp.Sent && !string.IsNullOrEmpty(otp.Code))
                lblOtpSentTo.Text += " (Dev: " + otp.Code + ")";
            if (!string.IsNullOrEmpty(otp.Error) && string.IsNullOrEmpty(otp.Code))
                ShowError(otp.Error);
        }

        protected void btnVerifyOTP_Click(object sender, EventArgs e)
        {
            int userId;
            if (!int.TryParse(Session["PendingUserID"] as string, out userId))
            {
                ShowError("Session expired. Request a new code.");
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
            AuthService.WriteAudit(userId, "EmailVerified", "User", userId, "Email verified.", Request.UserHostAddress);
            Session.Remove("PendingUserID");
            Session.Remove("PendingEmail");
            AuthService.CompleteLogin(Session, Request, user);
            Response.Redirect(AuthService.DashboardUrl(user.Role));
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

        private void ShowError(string message)
        {
            lblMessage.Text = message;
            lblMessage.Style["display"] = "flex";
        }
    }
}
