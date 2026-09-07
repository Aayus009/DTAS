using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.UserMeetings
{
    public partial class UserMeetings : Page
    {
        private string currentFilter = "All";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Modules/Authentication/Login.aspx");
                return;
            }

            MeetingService.EnsureSchema();

            if (!IsPostBack)
            {
                BindLookups();
                PrefillFromQuery();
                BindZoomHint();
                LoadMeetings();
            }
        }

        private int UserId
        {
            get { return Convert.ToInt32(Session["UserID"]); }
        }

        private void BindLookups()
        {
            string role = Session["Role"] as string;

            ddlWork.Items.Clear();
            ddlWork.Items.Add(new ListItem("- No workspace / work -", ""));
            foreach (DataRow row in MeetingService.ListLinkableWorks(UserId).Rows)
                ddlWork.Items.Add(new ListItem(row["TaskTitle"].ToString(), row["TaskID"].ToString()));

            ddlEvent.Items.Clear();
            ddlEvent.Items.Add(new ListItem("- Choose the event -", ""));
            foreach (DataRow row in MeetingService.ListLinkableEvents(UserId).Rows)
                ddlEvent.Items.Add(new ListItem(row["EventName"].ToString(), row["EventID"].ToString()));

            ddlGroup.Items.Clear();
            ddlGroup.Items.Add(new ListItem("- No assignment group -", ""));
            foreach (DataRow row in MeetingService.ListLinkableGroups(UserId).Rows)
            {
                string label = row["AssignmentName"] + " / " + row["GroupName"];
                ddlGroup.Items.Add(new ListItem(label, row["GroupID"].ToString()));
            }

            DataTable assignments = MeetingService.ListLinkableAssignments(UserId, role);
            pnlAssignment.Visible = assignments.Rows.Count > 0;
            ddlAssignment.Items.Clear();
            ddlAssignment.Items.Add(new ListItem("- No faculty assignment -", ""));
            foreach (DataRow row in assignments.Rows)
                ddlAssignment.Items.Add(new ListItem(row["AssignmentName"].ToString(), row["AssignmentID"].ToString()));
        }

        private void PrefillFromQuery()
        {
            bool open = false;
            string taskId = Request.QueryString["TaskID"];
            string groupId = Request.QueryString["GroupID"];
            string assignmentId = Request.QueryString["AssignmentID"];
            string eventId = Request.QueryString["EventID"];

            if (!string.IsNullOrEmpty(taskId) && ddlWork.Items.FindByValue(taskId) != null)
            {
                ddlWork.SelectedValue = taskId;
                open = true;
            }
            if (!string.IsNullOrEmpty(groupId) && ddlGroup.Items.FindByValue(groupId) != null)
            {
                ddlGroup.SelectedValue = groupId;
                open = true;
            }
            if (!string.IsNullOrEmpty(assignmentId) && ddlAssignment.Items.FindByValue(assignmentId) != null)
            {
                ddlAssignment.SelectedValue = assignmentId;
                open = true;
            }
            if (!string.IsNullOrEmpty(eventId) && ddlEvent.Items.FindByValue(eventId) != null)
            {
                ddlEvent.SelectedValue = eventId;
                open = true;
            }

            if (open)
                pnlForm.Visible = true;
        }

        private void BindZoomHint()
        {
            bool configured = new ZoomService().IsConfigured;
            pnlZoomHint.Visible = !configured;
            pnlZoomReady.Visible = configured;
        }

        private void SetActiveFilterButton()
        {
            string baseClass = "px-4 py-2 rounded-full font-label-md text-label-md bg-surface-container-high text-on-surface-variant hover:bg-surface-variant transition-colors";
            string activeClass = "px-4 py-2 rounded-full font-label-md text-label-md bg-primary text-on-primary";
            btnAll.CssClass = baseClass;
            btnUpcoming.CssClass = baseClass;
            btnPast.CssClass = baseClass;
            switch (currentFilter)
            {
                case "Upcoming": btnUpcoming.CssClass = activeClass; break;
                case "Past": btnPast.CssClass = activeClass; break;
                default: btnAll.CssClass = activeClass; break;
            }
        }

        private void LoadMeetings()
        {
            SetActiveFilterButton();
            DataTable dt = MeetingService.ListForUser(UserId, currentFilter);
            rptMeetings.DataSource = dt;
            rptMeetings.DataBind();
            pnlNoMeetings.Visible = dt.Rows.Count == 0;
        }

        protected void btnNew_Click(object sender, EventArgs e)
        {
            pnlForm.Visible = true;
            pnlMessage.Visible = false;
            BindZoomHint();
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            pnlForm.Visible = false;
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                pnlForm.Visible = true;
                return;
            }

            DateTime when;
            if (!DateTime.TryParse(txtDateTime.Text, out when))
            {
                ShowMessage("Enter a valid date and time.", false);
                pnlForm.Visible = true;
                return;
            }

            int duration;
            if (!int.TryParse(txtDuration.Text, out duration) || duration <= 0)
                duration = 60;

            int meetingId;
            string error = MeetingService.Create(
                UserId,
                txtTitle.Text,
                txtDescription.Text,
                ddlMeetingType.SelectedValue,
                when,
                duration,
                txtVenue.Text,
                txtAgenda.Text,
                true,
                ParseOptional(ddlWork.SelectedValue),
                ParseOptional(ddlAssignment.SelectedValue),
                ParseOptional(ddlGroup.SelectedValue),
                ParseOptional(ddlEvent.SelectedValue),
                null,
                out meetingId);

            if (error != null)
            {
                ShowMessage(error, false);
                pnlForm.Visible = true;
                return;
            }

            Response.Redirect("~/Modules/UserMeetings/UserMeetingDetails.aspx?MeetingID=" + meetingId);
        }

        private static int? ParseOptional(string value)
        {
            int id;
            if (int.TryParse(value, out id) && id > 0)
                return id;
            return null;
        }

        private void ShowMessage(string text, bool success)
        {
            pnlMessage.Visible = true;
            litMessage.Text = Server.HtmlEncode(text);
            pnlMessage.CssClass = success
                ? "mb-6 p-4 rounded-xl bg-tertiary-container/30 text-on-tertiary-container"
                : "mb-6 p-4 rounded-xl bg-error-container/40 text-error";
        }

        protected string FormatStatus(object status)
        {
            string value = Convert.ToString(status) ?? "";
            if (value.Equals("Completed", StringComparison.OrdinalIgnoreCase)
                || value.Equals("Cancelled", StringComparison.OrdinalIgnoreCase)
                || value.Equals("Ended", StringComparison.OrdinalIgnoreCase)
                || value.Equals("Closed", StringComparison.OrdinalIgnoreCase))
                return "Ended";
            return value;
        }

        protected string FormatRoom(object status, object hasZoom)
        {
            string label = FormatStatus(status);
            if (label == "Ended")
                return "Room closed";
            return Convert.ToInt32(hasZoom) == 1 ? "Join link sent to event members" : "No Zoom link yet";
        }

        protected void btnAll_Click(object sender, EventArgs e) { currentFilter = "All"; LoadMeetings(); }
        protected void btnUpcoming_Click(object sender, EventArgs e) { currentFilter = "Upcoming"; LoadMeetings(); }
        protected void btnPast_Click(object sender, EventArgs e) { currentFilter = "Past"; LoadMeetings(); }
    }
}
