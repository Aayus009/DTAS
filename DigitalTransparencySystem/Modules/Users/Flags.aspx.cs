using System;
using System.Data;
using System.Web.UI.WebControls;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Users
{
    public partial class Flags : System.Web.UI.Page
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

        protected void rptFlags_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int reportId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out reportId))
                return;

            int adminId = Convert.ToInt32(Session["UserID"]);
            string status = e.CommandName;
            string note = hfResolution.Value;
            hfResolution.Value = "";

            if (string.Equals(status, "RestrictClub", StringComparison.OrdinalIgnoreCase))
            {
                string targetType;
                int targetId;
                if (!ModerationService.TryGetReportTarget(reportId, out targetType, out targetId)
                    || !string.Equals(targetType, "Club", StringComparison.OrdinalIgnoreCase))
                {
                    UiNotice.Bind(lblMessage, "That flag is not for a club.", null);
                    BindQueue();
                    return;
                }

                string restrictError = RestrictionService.SetClubRestricted(targetId, adminId, Session["Role"] as string, true);
                if (!string.IsNullOrEmpty(restrictError))
                {
                    UiNotice.Bind(lblMessage, restrictError, null);
                    BindQueue();
                    return;
                }

                status = "Resolved";
                if (string.IsNullOrWhiteSpace(note))
                    note = "Club restricted after review.";
            }

            string error = ModerationService.ResolveReport(reportId, adminId, status, note);
            if (!string.IsNullOrEmpty(error))
                UiNotice.Bind(lblMessage, error, null);
            else
                UiNotice.Bind(lblMessage, null, "Flag updated to " + status + ".");

            BindQueue();
        }

        public bool IsClubTarget(object targetType)
        {
            return string.Equals(Convert.ToString(targetType), "Club", StringComparison.OrdinalIgnoreCase);
        }

        public bool IsOpen(object status)
        {
            string value = Convert.ToString(status);
            return string.Equals(value, "Pending", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "Reviewing", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "Escalated", StringComparison.OrdinalIgnoreCase);
        }

        private void BindQueue()
        {
            DataTable table = ModerationService.ListReports(ddlStatus.SelectedValue);
            rptFlags.DataSource = table;
            rptFlags.DataBind();
            pnlEmpty.Visible = table.Rows.Count == 0;
        }
    }
}
