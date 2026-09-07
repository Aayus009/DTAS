using System;
using System.Data;
using System.Web.UI.WebControls;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Notifications
{
    public partial class NotificationsPage : System.Web.UI.Page
    {
        private string CurrentFilter
        {
            get { return ViewState["Filter"] as string ?? "All"; }
            set { ViewState["Filter"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Modules/Authentication/Login.aspx");
                return;
            }

            int userId = Convert.ToInt32(Session["UserID"]);
            if (string.Equals(Request.QueryString["seen"], "1", StringComparison.OrdinalIgnoreCase))
            {
                NotificationService.MarkAllRead(userId);
                Response.Clear();
                Response.ContentType = "application/json";
                Response.Write("{\"ok\":true}");
                Response.End();
                return;
            }

            ApplyChrome();
            if (!IsPostBack)
            {
                NotificationService.MarkAllRead(userId);
                LoadNotifications();
            }
        }

        private void ApplyChrome()
        {
            bool admin = RoleAccess.IsAdmin(Session["Role"] as string);
            adminTop.Visible = admin;
            adminSide.Visible = admin;
            userTop.Visible = !admin;
            userSide.Visible = !admin;
        }

        private void SetActiveFilterButton()
        {
            string baseClass = "px-4 py-2 rounded-full font-label-md text-label-md bg-surface-container-high text-on-surface-variant hover:bg-surface-variant transition-colors";
            string activeClass = "px-4 py-2 rounded-full font-label-md text-label-md bg-primary text-on-primary";

            btnAll.CssClass = baseClass;
            btnUnread.CssClass = baseClass;
            btnTasks.CssClass = baseClass;
            btnAssignments.CssClass = baseClass;
            btnEvents.CssClass = baseClass;
            btnClubs.CssClass = baseClass;
            btnIdentity.CssClass = baseClass;
            btnModeration.CssClass = baseClass;

            switch (CurrentFilter)
            {
                case "Unread": btnUnread.CssClass = activeClass; break;
                case "Task": btnTasks.CssClass = activeClass; break;
                case "Assignment": btnAssignments.CssClass = activeClass; break;
                case "Event": btnEvents.CssClass = activeClass; break;
                case "Club": btnClubs.CssClass = activeClass; break;
                case "Identity": btnIdentity.CssClass = activeClass; break;
                case "Moderation": btnModeration.CssClass = activeClass; break;
                default: btnAll.CssClass = activeClass; break;
            }
        }

        private void LoadNotifications()
        {
            SetActiveFilterButton();
            int userId = Convert.ToInt32(Session["UserID"]);
            DataTable dt = NotificationService.ListFiltered(userId, CurrentFilter);
            rptNotifications.DataSource = dt;
            rptNotifications.DataBind();
            pnlNoNotifications.Visible = dt.Rows.Count == 0;
        }

        protected void rptNotifications_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int id;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out id))
                return;

            int userId = Convert.ToInt32(Session["UserID"]);
            if (e.CommandName == "MarkRead")
                NotificationService.MarkRead(userId, id);
            else if (e.CommandName == "Delete")
                NotificationService.Delete(userId, id);
            LoadNotifications();
        }

        protected void btnMarkAllRead_Click(object sender, EventArgs e)
        {
            NotificationService.MarkAllRead(Convert.ToInt32(Session["UserID"]));
            LoadNotifications();
        }

        protected void btnClearAll_Click(object sender, EventArgs e)
        {
            NotificationService.DeleteAll(Convert.ToInt32(Session["UserID"]));
            LoadNotifications();
        }

        protected void btnAll_Click(object sender, EventArgs e) { CurrentFilter = "All"; LoadNotifications(); }
        protected void btnUnread_Click(object sender, EventArgs e) { CurrentFilter = "Unread"; LoadNotifications(); }
        protected void btnTasks_Click(object sender, EventArgs e) { CurrentFilter = "Task"; LoadNotifications(); }
        protected void btnAssignments_Click(object sender, EventArgs e) { CurrentFilter = "Assignment"; LoadNotifications(); }
        protected void btnEvents_Click(object sender, EventArgs e) { CurrentFilter = "Event"; LoadNotifications(); }
        protected void btnClubs_Click(object sender, EventArgs e) { CurrentFilter = "Club"; LoadNotifications(); }
        protected void btnIdentity_Click(object sender, EventArgs e) { CurrentFilter = "Identity"; LoadNotifications(); }
        protected void btnModeration_Click(object sender, EventArgs e) { CurrentFilter = "Moderation"; LoadNotifications(); }
    }
}
