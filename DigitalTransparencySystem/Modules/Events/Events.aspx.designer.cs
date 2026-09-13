namespace DigitalTransparencySystem.Modules.Events
{
    public partial class Events
    {
        protected global::System.Web.UI.WebControls.HyperLink lnkCreateEvent;
        protected global::System.Web.UI.WebControls.TextBox txtSearch;
        protected global::System.Web.UI.WebControls.DropDownList ddlStatusFilter;
        protected global::System.Web.UI.WebControls.Button btnSearch;
        protected global::System.Web.UI.WebControls.Button btnReset;
        protected global::System.Web.UI.WebControls.Literal litTotalEvents;
        protected global::System.Web.UI.WebControls.Literal litPlanned;
        protected global::System.Web.UI.WebControls.Literal litInProgress;
        protected global::System.Web.UI.WebControls.Literal litCompleted;
        protected global::System.Web.UI.WebControls.Literal litPendingProposals;
        protected global::System.Web.UI.WebControls.SqlDataSource dsLiveEventStats;
        protected global::System.Web.UI.WebControls.FormView fvLiveEventStats;
        protected global::System.Web.UI.WebControls.Repeater rptEvents;
        protected global::System.Web.UI.WebControls.HiddenField hfRejectReason;
        protected global::System.Web.UI.WebControls.Panel pnlNoEvents;
    }
}
