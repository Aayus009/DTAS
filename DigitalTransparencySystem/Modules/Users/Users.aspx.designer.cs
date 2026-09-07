namespace DigitalTransparencySystem.Modules.Users
{
    public partial class Users
    {
        protected global::System.Web.UI.WebControls.Button btnShowCreateUser;
        protected global::System.Web.UI.WebControls.Literal litTotalUsers;
        protected global::System.Web.UI.WebControls.Literal litAdmins;
        protected global::System.Web.UI.WebControls.Literal litFacultyStaff;
        protected global::System.Web.UI.WebControls.Literal litStudents;
        protected global::System.Web.UI.WebControls.TextBox txtSearch;
        protected global::System.Web.UI.WebControls.DropDownList ddlRoleFilter;
        protected global::System.Web.UI.WebControls.DropDownList ddlStatusFilter;
        protected global::System.Web.UI.WebControls.Button btnSearch;
        protected global::System.Web.UI.WebControls.Label lblModeration;
        protected global::System.Web.UI.WebControls.HiddenField hfTargetUserId;
        protected global::System.Web.UI.WebControls.Panel pnlModerationForm;
        protected global::System.Web.UI.WebControls.Literal litModerationTitle;
        protected global::System.Web.UI.WebControls.Literal litModerationName;
        protected global::System.Web.UI.WebControls.Panel pnlSuspendDays;
        protected global::System.Web.UI.WebControls.TextBox txtSuspendDays;
        protected global::System.Web.UI.WebControls.TextBox txtModerationReason;
        protected global::System.Web.UI.WebControls.Button btnConfirmSuspend;
        protected global::System.Web.UI.WebControls.Button btnConfirmBan;
        protected global::System.Web.UI.WebControls.Button btnCancelModeration;
        protected global::System.Web.UI.WebControls.Repeater rptUsers;
        protected global::System.Web.UI.WebControls.Panel pnlCreateUser;
        protected global::System.Web.UI.WebControls.Panel pnlMessage;
        protected global::System.Web.UI.WebControls.Label lblMessage;
        protected global::System.Web.UI.WebControls.TextBox txtFullName;
        protected global::System.Web.UI.WebControls.RequiredFieldValidator rfvFullName;
        protected global::System.Web.UI.WebControls.TextBox txtEmail;
        protected global::System.Web.UI.WebControls.RequiredFieldValidator rfvEmail;
        protected global::System.Web.UI.WebControls.DropDownList ddlCreateRole;
        protected global::System.Web.UI.WebControls.RequiredFieldValidator rfvRole;
        protected global::System.Web.UI.WebControls.DropDownList ddlDepartment;
        protected global::System.Web.UI.WebControls.TextBox txtPassword;
        protected global::System.Web.UI.WebControls.RequiredFieldValidator rfvPassword;
        protected global::System.Web.UI.WebControls.Button btnCreateUser;
        protected global::System.Web.UI.WebControls.Button btnCancel;
        protected global::System.Web.UI.WebControls.Button btnClosePanel;
    }
}
