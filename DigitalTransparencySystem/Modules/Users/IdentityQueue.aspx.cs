using System;
using System.Data;
using System.Web.UI.WebControls;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Users
{
    public partial class IdentityQueue : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || !RoleAccess.IsAdmin(Session["Role"] as string))
            {
                Response.Redirect("~/Modules/Authentication/Login.aspx");
                return;
            }

            if (!IsPostBack)
                BindQueue();
        }

        protected void ddlStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindQueue();
        }

        protected void rptQueue_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int userId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out userId))
                return;

            int adminId = Convert.ToInt32(Session["UserID"]);
            string error = null;

            if (e.CommandName == "Approve")
                error = IdentityDocumentService.Decide(userId, adminId, true, null);
            else if (e.CommandName == "Reject")
                error = IdentityDocumentService.Decide(userId, adminId, false, hfRejectReason.Value);

            hfRejectReason.Value = "";

            if (!string.IsNullOrEmpty(error))
            {
                lblMessage.CssClass = "font-label-md block mb-4 text-error";
                lblMessage.Text = error;
            }
            else
            {
                lblMessage.CssClass = "font-label-md block mb-4 text-tertiary";
                lblMessage.Text = e.CommandName == "Approve" ? "Identity approved." : "Identity rejected.";
            }

            BindQueue();
        }

        private void BindQueue()
        {
            DataTable table = IdentityDocumentService.ListQueue(ddlStatus.SelectedValue);
            rptQueue.DataSource = table;
            rptQueue.DataBind();
            pnlEmpty.Visible = table.Rows.Count == 0;
        }
    }
}
