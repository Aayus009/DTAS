using System;
using System.Data;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Assignments
{
    public partial class Assignments : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || !RoleAccess.CanCreateAssignments(Session["Role"] as string))
            {
                Response.Redirect("~/Modules/Assignments/MyAssignments.aspx");
                return;
            }

            ApplyChrome();
            NotificationService.MarkTypesRead(Convert.ToInt32(Session["UserID"]), NotificationService.AssignmentTypes);
            if (!IsPostBack)
                BindList();
        }

        protected void btnCreate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                Show("Assignment name is required.", false);
                return;
            }

            DateTime deadline;
            if (!DateTime.TryParse(txtDeadline.Text, out deadline))
            {
                Show("Enter a deadline.", false);
                return;
            }

            int assignmentId;
            string code;
            string error = AssignmentService.CreateAssignment(
                Convert.ToInt32(Session["UserID"]),
                txtName.Text,
                txtDescription.Text.Trim(),
                deadline,
                out assignmentId,
                out code);

            if (error != null)
            {
                Show(error, false);
                return;
            }

            txtName.Text = "";
            txtDescription.Text = "";
            Show("Assignment created. Share this code with students: " + code, true);
            BindList();
        }

        private void BindList()
        {
            DataTable table = AssignmentService.ListAssignments(Convert.ToInt32(Session["UserID"]), Session["Role"] as string);
            rptAssignments.DataSource = table;
            rptAssignments.DataBind();
            pnlEmpty.Visible = table.Rows.Count == 0;
        }

        private void ApplyChrome()
        {
            adminTop.Visible = false;
            adminSide.Visible = false;
            userTop.Visible = true;
            userSide.Visible = true;
        }

        private void Show(string text, bool ok)
        {
            UiNotice.Bind(lblMessage, text, ok);
        }
    }
}
