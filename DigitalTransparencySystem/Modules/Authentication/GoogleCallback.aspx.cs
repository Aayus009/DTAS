using System;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Authentication
{
    public partial class GoogleCallback : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string googleError = Request.QueryString["error"];
            if (!string.IsNullOrEmpty(googleError))
            {
                Fail("Google sign-in was cancelled.");
                return;
            }

            string code = Request.QueryString["code"];
            string state = Request.QueryString["state"];
            if (string.IsNullOrEmpty(code))
            {
                Fail("Google did not return a sign-in code.");
                return;
            }

            GoogleProfile profile;
            string error = GoogleAuthService.ExchangeCode(Context, code, state, out profile);
            if (error != null)
            {
                Fail(error);
                return;
            }

            UserAccount user;
            bool needsRole;
            error = GoogleAuthService.SignInOrPrepare(Session, Request, profile, out user, out needsRole);
            if (error != null)
            {
                Fail(error);
                return;
            }

            if (needsRole)
            {
                Response.Redirect("~/Modules/Authentication/GoogleComplete.aspx");
                return;
            }

            Response.Redirect(AuthService.ConsumeReturnUrl(Session, AuthService.DashboardUrl(user.Role)));
        }

        private void Fail(string message)
        {
            Response.Redirect(AuthService.PopupSignInUrl(null, null, message));
        }
    }
}
