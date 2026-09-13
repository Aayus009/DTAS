using System;
using System.Data;
using System.Web.UI.WebControls;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Assignments
{
    public partial class MyAssignments : System.Web.UI.Page
    {
        private string GroupFilter
        {
            get { return ViewState["GroupFilter"] as string ?? "All"; }
            set { ViewState["GroupFilter"] = value; }
        }

        private string CreateType
        {
            get { return ViewState["CreateType"] as string ?? "Personal"; }
            set { ViewState["CreateType"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Modules/Authentication/Login.aspx");
                return;
            }

            lnkFaculty.Visible = RoleAccess.CanCreateAssignments(Session["Role"] as string);
            btnShowCreate.Visible = RoleAccess.IsStudent(Session["Role"] as string);
            NotificationService.MarkTypesRead(Convert.ToInt32(Session["UserID"]), NotificationService.AssignmentTypes);

            if (!IsPostBack)
            {
                GroupFilter = "All";
                CreateType = "Personal";
                SetFilterButtons();
                SetCreateTypeButtons();
                BindAll();
            }
        }

        protected void btnShowCreate_Click(object sender, EventArgs e)
        {
            pnlCreate.Visible = true;
            CreateType = "Personal";
            SetCreateTypeButtons();
        }

        protected void btnCancelCreate_Click(object sender, EventArgs e)
        {
            pnlCreate.Visible = false;
        }

        protected void btnTypePersonal_Click(object sender, EventArgs e)
        {
            CreateType = "Personal";
            SetCreateTypeButtons();
            pnlCreate.Visible = true;
        }

        protected void btnTypeCollege_Click(object sender, EventArgs e)
        {
            CreateType = "College";
            SetCreateTypeButtons();
            pnlCreate.Visible = true;
        }

        protected void btnCreatePersonal_Click(object sender, EventArgs e)
        {
            DateTime deadline;
            if (!DateTime.TryParse(txtPersonalDeadline.Text, out deadline))
            {
                Show("Enter a deadline.", null);
                pnlCreate.Visible = true;
                return;
            }

            int groupId;
            string error = AssignmentService.CreatePersonalGroup(
                Convert.ToInt32(Session["UserID"]),
                txtPersonalName.Text,
                txtPersonalDescription.Text,
                deadline,
                out groupId);
            if (error != null)
            {
                Show(error, null);
                pnlCreate.Visible = true;
                BindAll();
                return;
            }

            Response.Redirect("~/Modules/Assignments/GroupWorkspace.aspx?GroupID=" + groupId);
        }

        protected void btnCreateCollege_Click(object sender, EventArgs e)
        {
            int groupId;
            string error = AssignmentService.CreateSubgroup(
                Convert.ToInt32(Session["UserID"]), txtCode.Text, txtGroupName.Text, out groupId);
            if (error != null)
            {
                Show(error, null);
                pnlCreate.Visible = true;
                BindAll();
                return;
            }

            Response.Redirect("~/Modules/Assignments/GroupWorkspace.aspx?GroupID=" + groupId);
        }

        protected void btnFilterAll_Click(object sender, EventArgs e)
        {
            ApplyFilter("All");
        }

        protected void btnFilterPersonal_Click(object sender, EventArgs e)
        {
            ApplyFilter("Personal");
        }

        protected void btnFilterCollege_Click(object sender, EventArgs e)
        {
            ApplyFilter("College");
        }

        protected void rptInvites_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int invitationId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out invitationId))
                return;

            int userId = Convert.ToInt32(Session["UserID"]);
            string error = e.CommandName == "Accept"
                ? AssignmentService.AcceptInvitation(invitationId, userId)
                : AssignmentService.DeclineInvitation(invitationId, userId);
            Show(error, e.CommandName == "Accept" ? "You joined the group." : "Invitation declined.");
            BindAll();
        }

        public string TypeLabel(object type)
        {
            return string.Equals(Convert.ToString(type), "Personal", StringComparison.OrdinalIgnoreCase)
                ? "Personal Assignment Group"
                : "College Assignment Group";
        }

        private void ApplyFilter(string filter)
        {
            GroupFilter = filter;
            SetFilterButtons();
            BindAll();
        }

        private void SetFilterButtons()
        {
            string idle = "px-4 py-2 rounded-full font-label-md text-label-md bg-surface-container-high text-on-surface-variant hover:bg-surface-variant transition-colors";
            string active = "px-4 py-2 rounded-full font-label-md text-label-md bg-primary text-on-primary";
            btnFilterAll.CssClass = idle;
            btnFilterPersonal.CssClass = idle;
            btnFilterCollege.CssClass = idle;
            switch (GroupFilter)
            {
                case "Personal": btnFilterPersonal.CssClass = active; break;
                case "College": btnFilterCollege.CssClass = active; break;
                default: btnFilterAll.CssClass = active; break;
            }
        }

        private void SetCreateTypeButtons()
        {
            string idle = "px-4 py-2 rounded-full font-label-md text-label-md bg-surface-container-high text-on-surface-variant hover:bg-surface-variant transition-colors";
            string active = "px-4 py-2 rounded-full font-label-md text-label-md bg-primary text-on-primary";
            bool personal = CreateType != "College";
            btnTypePersonal.CssClass = personal ? active : idle;
            btnTypeCollege.CssClass = personal ? idle : active;
            pnlPersonalFields.Visible = personal;
            pnlCollegeFields.Visible = !personal;
        }

        private void BindAll()
        {
            int userId = Convert.ToInt32(Session["UserID"]);
            DataTable invites = AssignmentService.ListMyInvites(userId);
            rptInvites.DataSource = invites;
            rptInvites.DataBind();
            pnlNoInvites.Visible = invites.Rows.Count == 0;
            pnlPendingInvitations.Visible = invites.Rows.Count > 0;

            string filter = GroupFilter == "All" ? null : GroupFilter;
            DataTable mine = AssignmentService.ListMyGroups(userId, filter);
            rptMine.DataSource = mine;
            rptMine.DataBind();
            pnlNoMine.Visible = mine.Rows.Count == 0;
        }

        private void Show(string error, string ok)
        {
            UiNotice.Bind(lblMessage, error, ok);
        }
    }
}
