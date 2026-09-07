using System;
using System.Web.UI;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Authentication
{
    public partial class Register : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] != null)
            {
                Response.Redirect(AuthService.DashboardUrl(Session["Role"] as string));
                return;
            }

            Response.Redirect(AuthService.PopupRegisterUrl());
        }

        protected void btnRegister_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;
            if (!chkCharter.Checked)
            {
                ShowMessage("You must agree to the Data Ethics & Transparency Charter.", false);
                return;
            }

            string fullName = txtFullName.Text.Trim();
            string email = txtEmail.Text.Trim().ToLowerInvariant();
            string password = txtPassword.Text;
            int roleID = GetRoleID(ddlRole.SelectedValue);

            if (roleID == 0)
            {
                ShowMessage("Please select a valid role.", false);
                return;
            }

            if (password.Length < 8)
            {
                ShowMessage("Password must be at least 8 characters.", false);
                return;
            }

            try
            {
                if (AuthService.IsBanned(email, null))
                {
                    ShowMessage("This email cannot be used to register.", false);
                    return;
                }

                if (AuthService.EmailExists(email))
                {
                    ShowMessage("An account with this email already exists.", false);
                    return;
                }

                int userId = AuthService.CreateSelfRegisteredUser(fullName, email, password, roleID);
                AuthService.WriteAudit(userId, "Register", "User", userId, "Self-registered as " + AuthService.RoleName(roleID) + ".", Request.UserHostAddress);

                OtpIssueResult otp = EmailOtpService.Issue(userId, email, EmailOtpService.PurposeRegister);
                Session["PendingUserID"] = userId.ToString();
                Session["PendingEmail"] = email;

                pnlRegister.Visible = false;
                pnlOTP.Visible = true;
                lblOtpSentTo.Text = "A verification code was sent to " + EmailOtpService.MaskEmail(email) + ".";
                if (!otp.Sent && !string.IsNullOrEmpty(otp.Code))
                    lblOtpSentTo.Text += " (Dev: " + otp.Code + ")";
                if (!string.IsNullOrEmpty(otp.Error) && string.IsNullOrEmpty(otp.Code))
                    ShowMessage(otp.Error, false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Registration error: " + ex.Message);
                ShowMessage("An unexpected error occurred.", false);
            }
        }

        protected void btnVerifyOTP_Click(object sender, EventArgs e)
        {
            int userId;
            if (!int.TryParse(Session["PendingUserID"] as string, out userId))
            {
                ShowMessage("Session expired. Please register again.", false);
                return;
            }

            string error;
            if (!EmailOtpService.Verify(userId, txtOTP.Text.Trim(), EmailOtpService.PurposeRegister, out error))
            {
                ShowMessage(error, false);
                return;
            }

            AuthService.MarkEmailVerified(userId);
            UserAccount user = AuthService.FindById(userId);
            AuthService.WriteAudit(userId, "EmailVerified", "User", userId, "Email verified after registration.", Request.UserHostAddress);
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
                ShowMessage(otp.Error, false);
            else if (otp.Sent)
                ShowMessage("A new code has been sent to your email.", true);
            else
                ShowMessage("A new code has been sent to your email. (Dev: " + otp.Code + ")", true);
        }

        protected void btnGoogleLogin_Click(object sender, EventArgs e)
        {
            GoogleAuthService.StartFromPage(Context, false, delegate(string message) { ShowMessage(message, false); });
        }

        protected void cvCharter_ServerValidate(object source, System.Web.UI.WebControls.ServerValidateEventArgs args)
        {
            args.IsValid = chkCharter.Checked;
        }

        private int GetRoleID(string roleValue)
        {
            switch ((roleValue ?? "").ToLowerInvariant())
            {
                case "teacher": return 2;
                case "student": return 3;
                case "staff": return 4;
                default: return 0;
            }
        }

        private void ShowMessage(string message, bool isSuccess)
        {
            string css = isSuccess
                ? "mt-4 p-4 rounded-lg bg-tertiary-container/10 border border-tertiary-container text-on-tertiary-container"
                : "mt-4 p-4 rounded-lg bg-error-container border border-error text-error";

            if (pnlOTP.Visible)
            {
                pnlOtpMessage.Visible = true;
                pnlOtpMessage.CssClass = css;
                lblOtpMessage.Text = message;
                return;
            }

            pnlMessage.Visible = true;
            lblMessage.Text = message;
            pnlMessage.CssClass = css;
        }
    }
}
