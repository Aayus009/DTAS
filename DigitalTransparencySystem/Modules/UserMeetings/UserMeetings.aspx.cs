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
            MeetingService.SendDueReminders();

            if (!IsPostBack)
            {
                BindLookups();
                PrefillFromQuery();
                BindZoomHint();
                LoadMeetings();
            }
            ApplyAssignmentContext();
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
            if (!string.IsNullOrEmpty(groupId))
            {
                EnsureGroupInList(groupId);
                if (ddlGroup.Items.FindByValue(groupId) != null)
                    ddlGroup.SelectedValue = groupId;
                hidGroupId.Value = groupId;
                open = true;
            }
            if (!string.IsNullOrEmpty(assignmentId))
            {
                if (ddlAssignment.Items.FindByValue(assignmentId) != null)
                    ddlAssignment.SelectedValue = assignmentId;
                hidAssignmentId.Value = assignmentId;
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

        private void EnsureGroupInList(string groupId)
        {
            if (string.IsNullOrEmpty(groupId) || ddlGroup.Items.FindByValue(groupId) != null)
                return;
            int id;
            if (!int.TryParse(groupId, out id))
                return;
            AssignmentGroupRecord group = AssignmentService.GetGroup(id);
            if (group == null || group.IsDeleted)
                return;
            AssignmentRecord assignment = AssignmentService.GetAssignment(group.AssignmentID);
            string label = (assignment == null ? "" : assignment.AssignmentName + " / ") + group.GroupName;
            ddlGroup.Items.Add(new ListItem(label, groupId));
        }

        private bool HasAssignmentGroupContext()
        {
            return ParseOptional(Request.QueryString["GroupID"]).HasValue
                || ParseOptional(hidGroupId.Value).HasValue;
        }

        private bool HasAssignmentContext()
        {
            return HasAssignmentGroupContext()
                || ParseOptional(Request.QueryString["AssignmentID"]).HasValue
                || ParseOptional(hidAssignmentId.Value).HasValue;
        }

        private void ApplyAssignmentContext()
        {
            if (!HasAssignmentContext())
            {
                rfvEvent.Enabled = pnlEventLink.Visible;
                return;
            }

            pnlEventLink.Visible = false;
            pnlWorkLink.Visible = false;
            rfvEvent.Enabled = false;
            if (ddlMeetingType.Items.FindByValue("Assignment") != null)
                ddlMeetingType.SelectedValue = "Assignment";

            if (HasAssignmentGroupContext())
            {
                string groupId = Request.QueryString["GroupID"];
                if (string.IsNullOrEmpty(groupId))
                    groupId = hidGroupId.Value;
                EnsureGroupInList(groupId);
                if (ddlGroup.Items.FindByValue(groupId) != null)
                {
                    ddlGroup.Items.Clear();
                    AssignmentGroupRecord group = AssignmentService.GetGroup(Convert.ToInt32(groupId));
                    AssignmentRecord assignment = group == null ? null : AssignmentService.GetAssignment(group.AssignmentID);
                    string label = (assignment == null ? "Assignment group" : assignment.AssignmentName + " / " + group.GroupName);
                    ddlGroup.Items.Add(new ListItem(label, groupId));
                    ddlGroup.SelectedValue = groupId;
                }
                hidGroupId.Value = groupId;
                pnlAssignment.Visible = false;
                litGroupHint.Text = "The Zoom join link is sent only to members of this assignment group.";
                litIntro.Text = "Schedule a meeting for this assignment group. DTAS creates the Zoom room and emails this group's members. You do not need an event or workspace.";
                litZoomReady.Text = "DTAS will create a Zoom room and email the members of this assignment group. Members cannot enter until the host starts the room and admits them from the waiting room.";
            }
            else
            {
                string assignmentId = Request.QueryString["AssignmentID"];
                if (string.IsNullOrEmpty(assignmentId))
                    assignmentId = hidAssignmentId.Value;
                hidAssignmentId.Value = assignmentId;
                FilterGroupsToAssignment(assignmentId);
                pnlAssignment.Visible = false;
                litGroupHint.Text = "Choose the assignment group. The Zoom join link goes to that group's members. An event is not required.";
                litIntro.Text = "Schedule a meeting for this college assignment. Pick the assignment group that should receive the Zoom link. You do not need an event or workspace.";
                litZoomReady.Text = "DTAS will create a Zoom room and email the members of the selected assignment group. Members cannot enter until the host starts the room and admits them from the waiting room.";
            }
        }

        private void FilterGroupsToAssignment(string assignmentId)
        {
            int id;
            if (!int.TryParse(assignmentId, out id) || id <= 0)
                return;
            string selected = ddlGroup.SelectedValue;
            var keep = new System.Collections.Generic.List<ListItem>();
            keep.Add(new ListItem("- Choose the assignment group -", ""));
            foreach (DataRow row in AssignmentService.ListGroups(id).Rows)
            {
                string value = Convert.ToString(row["GroupID"]);
                keep.Add(new ListItem(Convert.ToString(row["GroupName"]), value));
            }
            ddlGroup.Items.Clear();
            foreach (ListItem item in keep)
                ddlGroup.Items.Add(item);
            if (!string.IsNullOrEmpty(selected) && ddlGroup.Items.FindByValue(selected) != null)
                ddlGroup.SelectedValue = selected;
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
            ApplyAssignmentContext();
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

            int? groupId = ParseOptional(hidGroupId.Value) ?? ParseOptional(ddlGroup.SelectedValue);
            int? assignmentId = ParseOptional(hidAssignmentId.Value) ?? ParseOptional(ddlAssignment.SelectedValue);
            int? workId = HasAssignmentContext() ? null : ParseOptional(ddlWork.SelectedValue);
            int? eventId = HasAssignmentContext() ? null : ParseOptional(ddlEvent.SelectedValue);

            if (HasAssignmentContext() && !groupId.HasValue)
            {
                ShowMessage("Choose the assignment group that should receive the Zoom join link.", false);
                pnlForm.Visible = true;
                return;
            }

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
                workId,
                assignmentId,
                groupId,
                eventId,
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
            return Convert.ToInt32(hasZoom) == 1 ? "Host starts the room; members wait to be admitted" : "No Zoom link yet";
        }

        protected void btnAll_Click(object sender, EventArgs e) { currentFilter = "All"; LoadMeetings(); }
        protected void btnUpcoming_Click(object sender, EventArgs e) { currentFilter = "Upcoming"; LoadMeetings(); }
        protected void btnPast_Click(object sender, EventArgs e) { currentFilter = "Past"; LoadMeetings(); }
    }
}
