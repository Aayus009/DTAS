using System;
using System.Data;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Settings
{
    public partial class SubmitReport : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Modules/Authentication/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                string type = Request.QueryString["type"];
                if (!string.IsNullOrEmpty(type) && ddlTargetType.Items.FindByValue(type) != null)
                    ddlTargetType.SelectedValue = type;

                string id = Request.QueryString["id"];
                if (!string.IsNullOrEmpty(id))
                    txtTarget.Text = id;

                BindMine();
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            int reporterId = Convert.ToInt32(Session["UserID"]);
            string targetType = ddlTargetType.SelectedValue;
            int targetId;
            string error = ResolveTarget(targetType, txtTarget.Text.Trim(), out targetId);
            if (error != null)
            {
                ShowMessage(error, false);
                return;
            }

            error = ModerationService.SubmitReport(reporterId, targetType, targetId, ddlReason.SelectedValue, txtDescription.Text.Trim());
            if (error != null)
            {
                ShowMessage(error, false);
                return;
            }

            txtDescription.Text = "";
            ShowMessage("Flag submitted. An administrator will review it.", true);
            BindMine();
        }

        private static string ResolveTarget(string targetType, string raw, out int targetId)
        {
            targetId = 0;
            if (string.IsNullOrWhiteSpace(raw))
                return "Enter a target email or ID.";

            if (int.TryParse(raw, out targetId) && targetId > 0)
                return null;

            if (string.Equals(targetType, "User", StringComparison.OrdinalIgnoreCase))
            {
                int? found = ModerationService.FindUserIdByEmail(raw);
                if (found == null)
                    return "No account uses that email.";
                targetId = found.Value;
                return null;
            }

            return "Enter a numeric ID for that target type.";
        }

        private void BindMine()
        {
            DataTable table = ModerationService.ListMyReports(Convert.ToInt32(Session["UserID"]));
            rptMine.DataSource = table;
            rptMine.DataBind();
            pnlEmpty.Visible = table.Rows.Count == 0;
        }

        private void ShowMessage(string message, bool success)
        {
            lblMessage.Text = message;
            lblMessage.CssClass = success
                ? "font-label-md block text-tertiary"
                : "font-label-md block text-error";
        }
    }
}
