using System;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Authentication
{
    public partial class GoogleComplete : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string email = Session["GooglePendingEmail"] as string;
            if (string.IsNullOrEmpty(email))
            {
                Response.Redirect(AuthService.PopupSignInUrl(null, null, null));
                return;
            }

            if (!IsPostBack)
            {
                litEmail.Text = Server.HtmlEncode(email);
                txtFullName.Text = Session["GooglePendingName"] as string ?? "";
            }
        }

        protected void btnFinish_Click(object sender, EventArgs e)
        {
            int roleId;
            if (!int.TryParse(ddlRole.SelectedValue, out roleId))
            {
                Show("Select a role.");
                return;
            }

            if (!string.IsNullOrWhiteSpace(txtFullName.Text))
                Session["GooglePendingName"] = txtFullName.Text.Trim();

            UserAccount user;
            string error = GoogleAuthService.FinishNewUser(Session, Request, roleId, out user);
            if (error != null)
            {
                Show(error);
                return;
            }

            Response.Redirect(AuthService.ConsumeReturnUrl(Session, AuthService.DashboardUrl(user.Role)));
        }

        private void Show(string message)
        {
            lblMessage.Text = message;
            lblMessage.Style["display"] = "flex";
        }
    }
}
