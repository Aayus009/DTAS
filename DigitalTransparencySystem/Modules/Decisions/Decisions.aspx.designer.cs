namespace DigitalTransparencySystem.Modules.Decisions
{
    public partial class Decisions
    {
        protected global::System.Web.UI.WebControls.TextBox txtSearch;
        protected global::System.Web.UI.WebControls.DropDownList ddlStatusFilter;
        protected global::System.Web.UI.WebControls.DropDownList ddlPriorityFilter;
        protected global::System.Web.UI.WebControls.Button btnSearch;
        protected global::System.Web.UI.WebControls.Button btnReset;
        protected global::System.Web.UI.WebControls.Literal litTotalDecisions;
        protected global::System.Web.UI.WebControls.Literal litProposed;
        protected global::System.Web.UI.WebControls.Literal litUnderReview;
        protected global::System.Web.UI.WebControls.Literal litApproved;
        protected global::System.Web.UI.WebControls.Literal litCompleted;
        protected global::System.Web.UI.WebControls.Literal litDecisionCount;
        protected global::System.Web.UI.WebControls.Repeater rptDecisions;
        protected global::System.Web.UI.WebControls.Panel pnlNoDecisions;
        protected global::System.Web.UI.WebControls.Panel pnlPagination;
        protected global::System.Web.UI.WebControls.Literal litPageInfo;
        protected global::System.Web.UI.WebControls.Button btnFirst;
        protected global::System.Web.UI.WebControls.Button btnPrev;
        protected global::System.Web.UI.WebControls.Repeater rptPager;
        protected global::System.Web.UI.WebControls.Button btnNext;
        protected global::System.Web.UI.WebControls.Button btnLast;
        protected global::System.Web.UI.WebControls.HiddenField litCurrentPage;
    }
}
