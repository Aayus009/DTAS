using System;
using System.Web.UI;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.MasterPages
{
    public partial class AdminSidebar : UserControl
    {
        private string _activePage = "";

        public string ActivePage
        {
            get { return _activePage; }
            set { _activePage = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] != null)
            {
                int count = NotificationService.CountUnread(Convert.ToInt32(Session["UserID"]));
                string text = NotificationService.BadgeText(count);
                lblNotifCount.Visible = !string.IsNullOrEmpty(text);
                lblNotifCount.Text = text;
            }
            else
            {
                lblNotifCount.Visible = false;
            }
            btnCreateTask.Visible = false;
            ApplyActiveState();
        }

        private void ApplyActiveState()
        {
            string baseClass = "group cursor-pointer flex items-center gap-4 px-4 py-3 mb-1 rounded-lg text-on-surface-variant hover:bg-surface-variant/50 transition-all hover:translate-x-1";
            string activeClass = baseClass + " active-nav-item";

            lnkDashboard.CssClass = baseClass;
            lnkNotifications.CssClass = baseClass;
            lnkEvents.CssClass = baseClass;
            lnkDecisions.CssClass = baseClass;
            lnkTasks.CssClass = baseClass;
            lnkAssignments.Visible = false;
            lnkAssignments.CssClass = baseClass;
            lnkClubs.CssClass = baseClass;
            lnkPolls.CssClass = baseClass;
            lnkReports.CssClass = baseClass;
            lnkTransparency.CssClass = baseClass;
            lnkUsers.CssClass = baseClass;
            lnkIdentity.CssClass = baseClass;
            lnkFlags.CssClass = baseClass;
            lnkFeedback.CssClass = baseClass;

            switch (_activePage.ToLower())
            {
                case "dashboard": lnkDashboard.CssClass = activeClass; break;
                case "notifications": lnkNotifications.CssClass = activeClass; break;
                case "events": lnkEvents.CssClass = activeClass; break;
                case "decisions": lnkDecisions.CssClass = activeClass; break;
                case "tasks": lnkTasks.CssClass = activeClass; break;
                case "assignments": lnkAssignments.CssClass = activeClass; break;
                case "clubs": lnkClubs.CssClass = activeClass; break;
                case "polls": lnkPolls.CssClass = activeClass; break;
                case "reports": lnkReports.CssClass = activeClass; break;
                case "transparency": lnkTransparency.CssClass = activeClass; break;
                case "users": lnkUsers.CssClass = activeClass; break;
                case "identity": lnkIdentity.CssClass = activeClass; break;
                case "flags": lnkFlags.CssClass = activeClass; break;
                case "feedback": lnkFeedback.CssClass = activeClass; break;
            }
        }

        protected void btnCreateTask_Click(object sender, EventArgs e)
        {
            Page.Response.Redirect("~/Modules/Tasks/CreateTask.aspx");
        }
    }
}
