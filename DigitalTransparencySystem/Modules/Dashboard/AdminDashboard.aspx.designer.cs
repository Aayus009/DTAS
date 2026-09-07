namespace DigitalTransparencySystem.Modules.Dashboard
{
    public partial class AdminDashboard
    {
        protected global::System.Web.UI.WebControls.Literal litLastUpdated;
        protected global::System.Web.UI.WebControls.Literal litTrustIndex;
        protected global::System.Web.UI.HtmlControls.HtmlGenericControl barAccountability;
        protected global::System.Web.UI.WebControls.Literal litActiveDecisions;
        protected global::System.Web.UI.WebControls.Literal litRecordRequests;
        protected global::System.Web.UI.WebControls.Literal litResponseTime;
        protected global::System.Web.UI.WebControls.Literal litFulfillmentRate;
        protected global::System.Web.UI.WebControls.Repeater rptPendingTasks;
        protected global::System.Web.UI.WebControls.Literal litComplianceRate;
        protected global::System.Web.UI.HtmlControls.HtmlGenericControl barTaskCompletion;
        protected global::System.Web.UI.WebControls.Literal litAccessibilityRate;
        protected global::System.Web.UI.HtmlControls.HtmlGenericControl barDecisionProgress;
        protected global::System.Web.UI.WebControls.Repeater rptSystemHealth;
        protected global::System.Web.UI.WebControls.Repeater rptAuditTrail;

        protected global::System.Web.UI.WebControls.Button btnFullAudit;
        protected global::System.Web.UI.WebControls.Button btnSeeAllActivity;
        protected global::System.Web.UI.WebControls.Literal litTasksCompleted;
        protected global::System.Web.UI.WebControls.Literal litTasksInProgress;
        protected global::System.Web.UI.WebControls.Literal litTasksPending;
        protected global::System.Web.UI.WebControls.Literal litTasksDelayed;
        protected global::System.Web.UI.WebControls.Literal litDecisionsProposed;
        protected global::System.Web.UI.WebControls.Literal litDecisionsUnderReview;
        protected global::System.Web.UI.WebControls.Literal litDecisionsApproved;
        protected global::System.Web.UI.WebControls.Literal litDecisionsRejected;
        protected global::System.Web.UI.WebControls.Literal litDecisionsCompleted;
        protected global::System.Web.UI.WebControls.Literal litEventsPlanned;
        protected global::System.Web.UI.WebControls.Literal litEventsInProgress;
        protected global::System.Web.UI.WebControls.Literal litEventsCompleted;
        protected global::System.Web.UI.WebControls.Literal litEventsCancelled;
        protected global::System.Web.UI.WebControls.Panel pnlProposals;
        protected global::System.Web.UI.WebControls.Literal litProposalCount;
        protected global::System.Web.UI.WebControls.Repeater rptProposals;
        protected global::System.Web.UI.WebControls.HyperLink lnkAllEvents;
        protected global::System.Web.UI.WebControls.HiddenField hfRejectReason;
        protected global::System.Web.UI.WebControls.Panel pnlIdentityQueue;
        protected global::System.Web.UI.WebControls.Label lblIdentityQueue;
        protected global::System.Web.UI.WebControls.HyperLink lnkIdentityQueue;
        protected global::System.Web.UI.WebControls.Literal litStatUsers;
        protected global::System.Web.UI.WebControls.Literal litStatPendingId;
        protected global::System.Web.UI.WebControls.Literal litStatAssignments;
        protected global::System.Web.UI.WebControls.Literal litStatGroups;
        protected global::System.Web.UI.WebControls.Literal litStatFlags;
        protected global::System.Web.UI.WebControls.Literal litStatSuspended;
        protected global::System.Web.UI.WebControls.Panel pnlFlagsQueue;
        protected global::System.Web.UI.WebControls.Label lblFlagsQueue;
        protected global::System.Web.UI.WebControls.HyperLink lnkFlagsQueue;
        protected global::System.Web.UI.WebControls.Repeater rptLiveEventProgress;
        protected global::System.Web.UI.WebControls.Panel pnlNoLiveEvents;
    }
}
