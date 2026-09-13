using System;
using System.Data;
using System.Web.UI.WebControls;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Assignments
{
    public partial class AssignmentDetails : System.Web.UI.Page
    {
        private int assignmentId;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || !int.TryParse(Request.QueryString["AssignmentID"], out assignmentId))
            {
                Response.Redirect("~/Modules/Assignments/Assignments.aspx");
                return;
            }

            AssignmentRecord assignment = AssignmentService.GetAssignment(assignmentId);
            if (!AssignmentService.CanViewAssignment(assignment, Convert.ToInt32(Session["UserID"]), Session["Role"] as string)
                || !RoleAccess.CanMonitorAssignments(Session["Role"] as string))
            {
                Response.Redirect("~/Modules/Assignments/MyAssignments.aspx");
                return;
            }

            ApplyChrome();
            if (!IsPostBack)
                Bind(assignment);
        }

        protected void btnExtendDeadline_Click(object sender, EventArgs e)
        {
            DateTime deadline;
            if (!DateTime.TryParse(txtNewDeadline.Text, out deadline))
            {
                Show("Enter a valid deadline.", null);
                Bind(AssignmentService.GetAssignment(assignmentId));
                return;
            }
            string error = AssignmentService.UpdateDeadline(assignmentId, Convert.ToInt32(Session["UserID"]), Session["Role"] as string, deadline);
            Show(error, "Deadline updated. Associated members were notified.");
            Bind(AssignmentService.GetAssignment(assignmentId));
        }

        protected void btnClose_Click(object sender, EventArgs e)
        {
            string error = AssignmentService.CloseAssignment(assignmentId, Convert.ToInt32(Session["UserID"]), Session["Role"] as string);
            Show(error, "Assignment closed.");
            Bind(AssignmentService.GetAssignment(assignmentId));
        }

        protected void rptGroups_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int groupId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out groupId))
                return;
            if (e.CommandName == "Remove")
                Show(AssignmentService.SoftDeleteGroup(groupId, Convert.ToInt32(Session["UserID"]), Session["Role"] as string), "Subgroup removed.");
            Bind(AssignmentService.GetAssignment(assignmentId));
        }

        private void Bind(AssignmentRecord assignment)
        {
            if (assignment == null)
                return;
            litName.Text = Server.HtmlEncode(assignment.AssignmentName);
            litCode.Text = Server.HtmlEncode(assignment.AssignmentCode);
            litDeadline.Text = assignment.Deadline.ToString("MMM dd, yyyy HH:mm");
            litStatus.Text = assignment.Status;
            litDescription.Text = Server.HtmlEncode(assignment.Description);
            lnkScheduleMeeting.NavigateUrl = "~/Modules/UserMeetings/UserMeetings.aspx?AssignmentID=" + assignmentId;
            int connectId;
            ConnectService.CreateGroupForAssignment(assignmentId, out connectId);
            lnkAssignmentConnect.Visible = connectId > 0;
            lnkAssignmentConnect.NavigateUrl = connectId > 0
                ? "~/Modules/Connect/ConnectRoom.aspx?GroupID=" + connectId
                : "";
            btnClose.Visible = assignment.Status == "Active";
            txtNewDeadline.Text = assignment.Deadline.ToString("yyyy-MM-ddTHH:mm");
            int days = (assignment.Deadline.Date - DateTime.Today).Days;
            pnlNearDeadline.Visible = assignment.Status == "Active" && days >= 0 && days <= 3;
            litNearDeadline.Text = days == 0
                ? "Deadline is today. Associated members have been alerted."
                : "Deadline is in " + days + " day(s). Associated members have been alerted.";

            DataTable groups = AssignmentService.ListGroups(assignmentId);
            rptGroups.DataSource = groups;
            rptGroups.DataBind();
            pnlEmpty.Visible = groups.Rows.Count == 0;
        }

        private void ApplyChrome()
        {
            adminTop.Visible = false;
            adminSide.Visible = false;
            userTop.Visible = true;
            userSide.Visible = true;
        }

        private void Show(string error, string ok)
        {
            UiNotice.Bind(lblMessage, error, ok);
        }
    }
}
