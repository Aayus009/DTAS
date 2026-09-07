namespace DigitalTransparencySystem.Modules.Tasks
{
    public partial class EditTask
    {
        protected global::DigitalTransparencySystem.MasterPages.AdminTopbar adminTop;
        protected global::DigitalTransparencySystem.MasterPages.UserTopbar userTop;
        protected global::DigitalTransparencySystem.MasterPages.AdminSidebar adminSide;
        protected global::DigitalTransparencySystem.MasterPages.UserSidebar userSide;
        protected global::System.Web.UI.WebControls.Panel pnlError;
        protected global::System.Web.UI.WebControls.Label lblError;
        protected global::System.Web.UI.WebControls.TextBox txtTaskTitle;
        protected global::System.Web.UI.WebControls.RequiredFieldValidator rfvTaskTitle;
        protected global::System.Web.UI.WebControls.TextBox txtDescription;
        protected global::System.Web.UI.WebControls.DropDownList ddlPriority;
        protected global::System.Web.UI.WebControls.RequiredFieldValidator rfvPriority;
        protected global::System.Web.UI.WebControls.DropDownList ddlStatus;
        protected global::System.Web.UI.WebControls.RequiredFieldValidator rfvStatus;
        protected global::System.Web.UI.WebControls.TextBox txtDueDate;
        protected global::System.Web.UI.WebControls.RequiredFieldValidator rfvDueDate;
        protected global::System.Web.UI.WebControls.DropDownList ddlRelatedDecision;
        protected global::System.Web.UI.WebControls.DropDownList ddlRelatedEvent;
        protected global::System.Web.UI.WebControls.DropDownList ddlRelatedMeeting;
        protected global::System.Web.UI.WebControls.DropDownList ddlLeader;
        protected global::System.Web.UI.WebControls.CheckBoxList cblAssignUsers;
        protected global::System.Web.UI.WebControls.Panel pnlNoUsers;
        protected global::System.Web.UI.WebControls.Button btnUpdate;
        protected global::System.Web.UI.WebControls.Button btnCancel;
        protected global::System.Web.UI.WebControls.Button btnCancelTop;
    }
}
