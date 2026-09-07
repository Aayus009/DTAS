using System;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Settings
{
    public partial class IdentityVerification : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Modules/Authentication/Login.aspx");
                return;
            }

            if (RoleAccess.IsAdmin(Session["Role"] as string))
            {
                Response.Redirect("~/Modules/Users/IdentityQueue.aspx");
                return;
            }

            if (!IsPostBack)
                BindStatus();
        }

        protected void btnUpload_Click(object sender, EventArgs e)
        {
            int userId = Convert.ToInt32(Session["UserID"]);
            string error = IdentityDocumentService.SaveUpload(userId, fuDocument.PostedFile, txtInstitutionalID.Text, Server);
            if (error != null)
            {
                lblMessage.CssClass = "font-label-md block text-error";
                lblMessage.Text = error;
                return;
            }

            Session["IdentityVerified"] = false;
            Session["VerificationStatus"] = "Pending";
            AuthService.WriteAudit(userId, "IdentitySubmitted", "User", userId, "ID document submitted.", Request.UserHostAddress);
            lblMessage.CssClass = "font-label-md block text-tertiary";
            lblMessage.Text = "Document submitted. An administrator will review it.";
            BindStatus();
        }

        private void BindStatus()
        {
            int userId = Convert.ToInt32(Session["UserID"]);
            UserAccount user = AuthService.FindById(userId);
            IdentityDocumentInfo doc = IdentityDocumentService.GetCurrent(userId);

            txtInstitutionalID.Text = user != null ? (user.InstitutionalID ?? "") : "";

            if (user != null && user.IdentityVerified)
            {
                lblStatus.Text = "Verified";
                lblStatusDetail.Text = "Your identity is approved. Core features are unlocked.";
                pnlUpload.Visible = false;
            }
            else if (string.Equals(user != null ? user.VerificationStatus : "", "Pending", StringComparison.OrdinalIgnoreCase))
            {
                lblStatus.Text = "Pending review";
                lblStatusDetail.Text = "Your document is in the admin queue. You can replace it if you uploaded the wrong file.";
            }
            else if (string.Equals(user != null ? user.VerificationStatus : "", "Rejected", StringComparison.OrdinalIgnoreCase))
            {
                lblStatus.Text = "Rejected";
                lblStatusDetail.Text = string.IsNullOrEmpty(user.RejectionReason)
                    ? "The last submission was rejected. Upload a clearer document and try again."
                    : user.RejectionReason;
            }
            else
            {
                lblStatus.Text = "Not submitted";
                lblStatusDetail.Text = "Events, tasks, polls, and workspaces stay locked until an admin verifies your ID.";
            }

            if (doc != null)
            {
                pnlCurrent.Visible = true;
                litFileName.Text = Server.HtmlEncode(doc.OriginalFileName);
                litUploadDate.Text = doc.UploadDate.ToString("MMM dd, yyyy h:mm tt");
                lnkViewDoc.NavigateUrl = "~/Modules/Users/ViewIdentityDocument.aspx?id=" + doc.DocumentID;
            }
            else
            {
                pnlCurrent.Visible = false;
            }
        }
    }
}
