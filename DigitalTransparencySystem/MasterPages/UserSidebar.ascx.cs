using System;
using System.Web.UI;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.MasterPages
{
    public partial class UserSidebar : UserControl
    {
        private string _activePage = "";

        public string ActivePage
        {
            get { return _activePage; }
            set { _activePage = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            ApplyRoleVisibility();
            ApplyActiveState();
        }

        private void ApplyRoleVisibility()
        {
            string role = Session["Role"] as string;
            bool facultyStaff = RoleAccess.IsFacultyOrStaff(role);
            lnkMyMeetings.Visible = true;
            lnkReports.Visible = facultyStaff;
            lnkCreateEvent.Visible = RoleAccess.CanCreateEvents(role);
            lnkProposeEvent.Visible = RoleAccess.IsStudent(role);
            pnlEventActions.Visible = true;
            lnkAssignments.Visible = RoleAccess.IsStudent(role) || RoleAccess.CanCreateAssignments(role);
            lnkAssignments.NavigateUrl = "~/Modules/Assignments/MyAssignments.aspx";
            lnkIdentity.Visible = !IsIdentityVerified();

            if (Session["UserID"] == null)
            {
                lblAssignmentCount.Visible = false;
                return;
            }

            int userId = Convert.ToInt32(Session["UserID"]);
            if (lnkAssignments.Visible)
                BindUnreadBadge(lblAssignmentCount, NotificationService.CountUnread(userId, NotificationService.AssignmentTypes));
            else
                lblAssignmentCount.Visible = false;
        }

        private bool IsIdentityVerified()
        {
            object flag = Session["IdentityVerified"];
            if (flag is bool)
                return (bool)flag;
            bool verified;
            return flag != null && bool.TryParse(flag.ToString(), out verified) && verified;
        }

        private static void BindUnreadBadge(System.Web.UI.WebControls.Label badge, int count)
        {
            string text = NotificationService.BadgeText(count);
            badge.Visible = !string.IsNullOrEmpty(text);
            badge.Text = text;
        }

        private void ApplyActiveState()
        {
            string baseClass = "group cursor-pointer flex items-center gap-4 px-4 py-3 mb-1 rounded-lg text-on-surface-variant hover:bg-surface-variant/50 transition-all hover:translate-x-1";
            string activeClass = baseClass + " active-nav-item";
            string childClass = "sidebar-sublink group cursor-pointer flex items-center gap-3 px-4 py-2 mb-1 rounded-lg text-on-surface-variant hover:bg-surface-variant/50 transition-all";
            string childActiveClass = childClass + " active-nav-item";

            lnkDashboard.CssClass = baseClass;
            lnkIdentity.CssClass = baseClass;
            lnkTaskWorkspaces.CssClass = baseClass;
            lnkClubs.CssClass = baseClass;
            lnkAssignments.CssClass = baseClass;
            lnkConnect.CssClass = baseClass;
            lnkMyEvents.CssClass = baseClass;
            lnkCampusEvents.CssClass = childClass;
            lnkCreateEvent.CssClass = childClass;
            lnkProposeEvent.CssClass = childClass;
            lnkMyMeetings.CssClass = baseClass;
            lnkFeedback.CssClass = baseClass;
            lnkFlag.CssClass = baseClass;
            lnkPolls.CssClass = baseClass;
            lnkReports.CssClass = baseClass;
            lnkTransparency.CssClass = baseClass;

            switch (_activePage.ToLower())
            {
                case "dashboard": lnkDashboard.CssClass = activeClass; break;
                case "identity": lnkIdentity.CssClass = activeClass; break;
                case "tasks":
                case "workspaces": lnkTaskWorkspaces.CssClass = activeClass; break;
                case "clubs": lnkClubs.CssClass = activeClass; break;
                case "assignments": lnkAssignments.CssClass = activeClass; break;
                case "connect": lnkConnect.CssClass = activeClass; break;
                case "myevents": lnkMyEvents.CssClass = activeClass; break;
                case "campusevents":
                    lnkMyEvents.CssClass = activeClass;
                    lnkCampusEvents.CssClass = childActiveClass;
                    break;
                case "createevent":
                    lnkMyEvents.CssClass = activeClass;
                    lnkCreateEvent.CssClass = childActiveClass;
                    break;
                case "proposeevent":
                    lnkMyEvents.CssClass = activeClass;
                    lnkProposeEvent.CssClass = childActiveClass;
                    break;
                case "meetings": lnkMyMeetings.CssClass = activeClass; break;
                case "feedback": lnkFeedback.CssClass = activeClass; break;
                case "flag": lnkFlag.CssClass = activeClass; break;
                case "polls": lnkPolls.CssClass = activeClass; break;
                case "reports": lnkReports.CssClass = activeClass; break;
                case "transparency": lnkTransparency.CssClass = activeClass; break;
            }
        }
    }
}
