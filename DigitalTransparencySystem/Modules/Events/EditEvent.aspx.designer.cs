namespace DigitalTransparencySystem.Modules.Events
{
    public partial class EditEvent
    {
        protected global::DigitalTransparencySystem.MasterPages.AdminTopbar adminTop;
        protected global::DigitalTransparencySystem.MasterPages.UserTopbar userTop;
        protected global::DigitalTransparencySystem.MasterPages.AdminSidebar adminSide;
        protected global::DigitalTransparencySystem.MasterPages.UserSidebar userSide;
        protected global::System.Web.UI.WebControls.Panel pnlError;
        protected global::System.Web.UI.WebControls.Label lblError;
        protected global::System.Web.UI.WebControls.Panel pnlSuccess;
        protected global::System.Web.UI.WebControls.Label lblSuccess;
        protected global::System.Web.UI.WebControls.TextBox txtEventName;
        protected global::System.Web.UI.WebControls.RequiredFieldValidator rfvEventName;
        protected global::System.Web.UI.WebControls.DropDownList ddlEventType;
        protected global::System.Web.UI.WebControls.TextBox txtDescription;
        protected global::System.Web.UI.WebControls.TextBox txtStartDate;
        protected global::System.Web.UI.WebControls.RequiredFieldValidator rfvStartDate;
        protected global::System.Web.UI.WebControls.TextBox txtEndDate;
        protected global::System.Web.UI.WebControls.RequiredFieldValidator rfvEndDate;
        protected global::System.Web.UI.WebControls.CompareValidator cvEndDate;
        protected global::System.Web.UI.WebControls.TextBox txtVenue;
        protected global::System.Web.UI.WebControls.RequiredFieldValidator rfvVenue;
        protected global::System.Web.UI.WebControls.TextBox txtBudget;
        protected global::System.Web.UI.WebControls.TextBox txtOrganizer;
        protected global::System.Web.UI.WebControls.DropDownList ddlStatus;
        protected global::System.Web.UI.WebControls.DropDownList ddlVisibility;
        protected global::System.Web.UI.WebControls.Button btnUpdate;
        protected global::System.Web.UI.WebControls.Button btnCancel;
        protected global::System.Web.UI.WebControls.Button btnCancelTop;
    }
}
