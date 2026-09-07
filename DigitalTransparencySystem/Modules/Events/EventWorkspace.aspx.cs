using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Events
{
    public partial class EventWorkspace : System.Web.UI.Page
    {
        private int eventId;
        private EventAccess access;

        public bool CanManageMembers
        {
            get { return access != null && access.CanManage; }
        }

        public bool CanRemoveMembers
        {
            get { return access != null && access.CanRemoveMembers; }
        }

        public bool CanReviewJoins
        {
            get { return access != null && access.CanReviewJoins; }
        }

        public bool IsEventOfficer
        {
            get { return access != null && (access.CanAssign || access.CanManage || access.IsSystemAdmin); }
        }

        public bool EventIsOpen
        {
            get
            {
                return access != null && access.Event != null
                    && !access.Event.IsDisabled
                    && !string.Equals(access.Event.Status, "Completed", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(access.Event.Status, "Cancelled", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(access.Event.Status, "Archived", StringComparison.OrdinalIgnoreCase);
            }
        }

        public bool CreateIssueOpen { get; set; }

        protected string BoardColumn(object status)
        {
            string value = (Convert.ToString(status) ?? "").Replace(" ", "");
            if (value.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                return "done";
            if (value.Equals("UnderReview", StringComparison.OrdinalIgnoreCase)
                || value.Equals("Submitted", StringComparison.OrdinalIgnoreCase))
                return "review";
            if (value.Equals("RevisionNeeded", StringComparison.OrdinalIgnoreCase)
                || value.Equals("ChangesRequested", StringComparison.OrdinalIgnoreCase))
                return "changes";
            if (value.Equals("InProgress", StringComparison.OrdinalIgnoreCase))
                return "progress";
            return "todo";
        }

        protected string AvatarStack(object assignees)
        {
            string text = Convert.ToString(assignees) ?? "";
            if (string.IsNullOrWhiteSpace(text) || text.Equals("Unassigned", StringComparison.OrdinalIgnoreCase))
                return "<span class=\"jira-unassigned\">Unassigned</span>";

            var html = new StringBuilder("<span class=\"jira-avatars\">");
            foreach (string part in text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string name = part.Trim();
                if (name.Length == 0)
                    continue;
                html.Append("<span class=\"jira-avatar\" title=\"")
                    .Append(Server.HtmlEncode(name))
                    .Append("\">")
                    .Append(Server.HtmlEncode(InitialsFromName(name)))
                    .Append("</span>");
            }
            html.Append("</span>");
            return html.ToString();
        }

        private static string InitialsFromName(string name)
        {
            string[] parts = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return "?";
            if (parts.Length == 1)
                return parts[0].Substring(0, 1).ToUpperInvariant();
            return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpperInvariant();
        }

        public string FileIcon(object fileName)
        {
            string ext = Path.GetExtension(Convert.ToString(fileName) ?? "").ToLowerInvariant();
            switch (ext)
            {
                case ".pdf": return "picture_as_pdf";
                case ".png":
                case ".jpg":
                case ".jpeg": return "image";
                case ".zip": return "folder_zip";
                case ".doc":
                case ".docx": return "description";
                default: return "draft";
            }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            HtmlForm form = Master.FindControl("form1") as HtmlForm;
            if (form != null)
                form.Enctype = "multipart/form-data";
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || !int.TryParse(Request.QueryString["EventID"], out eventId))
            {
                Response.Redirect("~/Modules/Events/MyEvents.aspx");
                return;
            }

            bool isAdmin = RoleAccess.IsAdmin(Session["Role"] as string);
            adminTop.Visible = isAdmin;
            adminSide.Visible = isAdmin;
            userTop.Visible = !isAdmin;
            userSide.Visible = !isAdmin;

            int userId = Convert.ToInt32(Session["UserID"]);
            EventService.EnsureCreatorMembership(eventId, userId);
            access = EventService.GetAccess(eventId, userId, Session["Role"] as string);

            if (access.Event == null || !access.CanView)
            {
                pnlWorkspace.Visible = false;
                pnlDenied.Visible = true;
                return;
            }

            if (access.Event.IsDisabled && !access.IsSystemAdmin)
            {
                pnlWorkspace.Visible = true;
                BindWorkspace();
                return;
            }

            if (!IsPostBack)
                BindWorkspace();
            else
                ApplyChrome();
        }

        protected void btnJoinPublic_Click(object sender, EventArgs e)
        {
            Show(EventService.JoinPublic(eventId, Convert.ToInt32(Session["UserID"])),
                "Request sent. The event lead or manager must accept you before you can work here.");
            Reload();
        }

        protected void btnAccept_Click(object sender, EventArgs e)
        {
            if (access.PendingInvitationId.HasValue)
                Show(EventService.AcceptInvitation(access.PendingInvitationId.Value, Convert.ToInt32(Session["UserID"])), "Invitation accepted.");
            Reload();
        }

        protected void btnLeave_Click(object sender, EventArgs e)
        {
            Show(EventService.Leave(eventId, Convert.ToInt32(Session["UserID"])), "You left the event.");
            if (string.IsNullOrEmpty(lblMessage.CssClass) || lblMessage.CssClass.IndexOf("error") < 0)
                Response.Redirect("~/Modules/Events/MyEvents.aspx");
        }

        protected void btnDisable_Click(object sender, EventArgs e)
        {
            Show(EventService.SetDisabled(eventId, Convert.ToInt32(Session["UserID"]), Session["Role"] as string, true), "Event disabled.");
            Reload();
        }

        protected void btnEnable_Click(object sender, EventArgs e)
        {
            Show(EventService.SetDisabled(eventId, Convert.ToInt32(Session["UserID"]), Session["Role"] as string, false), "Event enabled.");
            Reload();
        }

        protected void btnLookup_Click(object sender, EventArgs e)
        {
            EmailLookupResult result = EventService.LookupByExactEmail(txtInviteEmail.Text);
            lblLookup.CssClass = result.Found ? "font-label-md block mt-3 text-tertiary" : "font-label-md block mt-3 text-error";
            lblLookup.Text = result.Found
                ? result.Message + " " + result.FullName + " (" + result.Role + ")."
                : result.Message;
            ApplyChrome();
        }

        protected void btnInvite_Click(object sender, EventArgs e)
        {
            string error = EventService.InviteByEmail(
                eventId,
                Convert.ToInt32(Session["UserID"]),
                Session["Role"] as string,
                txtInviteEmail.Text,
                ddlInviteRole.SelectedValue);
            Show(error, "Invitation sent.");
            Reload();
        }

        protected void btnCreateTask_Click(object sender, EventArgs e)
        {
            DateTime? dueDate = null;
            DateTime parsed;
            if (DateTime.TryParse(txtTaskDue.Text, out parsed))
                dueDate = parsed;

            List<int> assigneeIds = SelectedAssigneeIds;

            string error = EventService.CreateEventTask(
                eventId,
                Convert.ToInt32(Session["UserID"]),
                Session["Role"] as string,
                txtTaskTitle.Text,
                txtTaskDescription.Text,
                dueDate,
                assigneeIds);
            Show(error, "Task added to the shared workplace.");
            if (error == null)
            {
                txtTaskTitle.Text = "";
                txtTaskDescription.Text = "";
                txtMemberSearch.Text = "";
                hfAssigneeIds.Value = "";
                Reload();
            }
            else
            {
                CreateIssueOpen = true;
                ApplyChrome();
            }
        }

        protected void btnAddNote_Click(object sender, EventArgs e)
        {
            string error = EventService.AddNote(
                eventId,
                Convert.ToInt32(Session["UserID"]),
                Session["Role"] as string,
                txtNote.Text,
                chkPin.Checked);
            Show(error, "Note added.");
            if (error == null)
                txtNote.Text = "";
            Reload();
        }

        protected void rptMembers_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int targetId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out targetId))
                return;

            int actorId = Convert.ToInt32(Session["UserID"]);
            string role = Session["Role"] as string;
            string error = null;

            switch (e.CommandName)
            {
                case "MakeAdmin":
                    error = EventService.SetMemberRole(eventId, actorId, role, targetId, EventRoles.EventAdmin);
                    break;
                case "MakeManager":
                    error = EventService.SetMemberRole(eventId, actorId, role, targetId, EventRoles.EventManager);
                    break;
                case "MakeParticipant":
                    error = EventService.SetMemberRole(eventId, actorId, role, targetId, EventRoles.Participant);
                    break;
                case "Remove":
                    error = EventService.RemoveMember(eventId, actorId, role, targetId);
                    break;
            }

            Show(error, "Member updated.");
            Reload();
        }

        protected void rptMembers_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
                return;

            var btnRemove = e.Item.FindControl("btnRemoveMember") as LinkButton;
            var pnlActions = e.Item.FindControl("pnlMemberActions") as Panel;
            if (btnRemove == null)
                return;

            int targetId = Convert.ToInt32(DataBinder.Eval(e.Item.DataItem, "UserID"));
            string targetRole = Convert.ToString(DataBinder.Eval(e.Item.DataItem, "RoleInEvent"));
            int actorId = Convert.ToInt32(Session["UserID"]);

            bool canRemove = access != null && access.CanRemoveMembers
                && access.Event != null && !access.Event.IsDisabled
                && targetId != actorId;

            if (canRemove
                && !access.CanManage
                && string.Equals(targetRole, EventRoles.EventAdmin, StringComparison.OrdinalIgnoreCase))
                canRemove = false;

            btnRemove.Visible = canRemove;
            if (pnlActions != null)
                pnlActions.Visible = CanManageMembers || canRemove;
        }

        protected void rptJoinRequests_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int targetId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out targetId))
                return;

            int actorId = Convert.ToInt32(Session["UserID"]);
            string role = Session["Role"] as string;
            string error = null;
            string success = "Join request updated.";

            if (e.CommandName == "AcceptJoin")
            {
                error = EventService.AcceptJoinRequest(eventId, actorId, role, targetId);
                success = "Join request accepted.";
            }
            else if (e.CommandName == "DeclineJoin")
            {
                error = EventService.DeclineJoinRequest(eventId, actorId, role, targetId);
                success = "Join request declined.";
            }

            Show(error, success);
            Reload();
        }

        protected void rptTasks_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
                return;

            int taskId = Convert.ToInt32(DataBinder.Eval(e.Item.DataItem, "TaskID"));
            bool assigned = Convert.ToBoolean(DataBinder.Eval(e.Item.DataItem, "IsAssigned"));
            string status = Convert.ToString(DataBinder.Eval(e.Item.DataItem, "Status"));
            bool completed = string.Equals(status, "Completed", StringComparison.OrdinalIgnoreCase);
            bool needsReview = string.Equals(status, EventTaskService.UnderReview, StringComparison.OrdinalIgnoreCase)
                || string.Equals(status, "Submitted", StringComparison.OrdinalIgnoreCase);

            var pnlMyProgress = (Panel)e.Item.FindControl("pnlMyProgress");
            var pnlFacultyReview = (Panel)e.Item.FindControl("pnlFacultyReview");
            var pnlAddComment = (Panel)e.Item.FindControl("pnlAddComment");
            var rptTaskComments = (Repeater)e.Item.FindControl("rptTaskComments");
            var lblNoComments = (Label)e.Item.FindControl("lblNoComments");
            var ddlTaskStatus = (DropDownList)e.Item.FindControl("ddlTaskStatus");
            var pnlUploadWork = (Panel)e.Item.FindControl("pnlUploadWork");
            var rptTaskFiles = (Repeater)e.Item.FindControl("rptTaskFiles");
            var lblNoFiles = (Label)e.Item.FindControl("lblNoFiles");

            if (pnlMyProgress != null)
                pnlMyProgress.Visible = assigned && EventIsOpen && !completed && !CanManageMembers;
            if (pnlUploadWork != null)
                pnlUploadWork.Visible = assigned && EventIsOpen && !completed;
            if (pnlFacultyReview != null)
                pnlFacultyReview.Visible = CanManageMembers && EventIsOpen && needsReview;
            if (pnlAddComment != null)
                pnlAddComment.Visible = access != null && access.IsMember && access.Event != null && !access.Event.IsDisabled;

            DataTable files = EventTaskService.ListAttachments(taskId);
            if (rptTaskFiles != null)
            {
                rptTaskFiles.DataSource = files;
                rptTaskFiles.DataBind();
            }
            if (lblNoFiles != null)
                lblNoFiles.Visible = files.Rows.Count == 0;

            if (ddlTaskStatus != null && ddlTaskStatus.Items.FindByValue(status) != null)
                ddlTaskStatus.SelectedValue = status;

            DataTable comments = EventTaskService.ListComments(taskId);
            if (rptTaskComments != null)
            {
                rptTaskComments.DataSource = comments;
                rptTaskComments.DataBind();
            }
            if (lblNoComments != null)
                lblNoComments.Visible = comments.Rows.Count == 0;
        }

        protected void rptTasks_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int taskId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out taskId))
                return;

            hfOpenTask.Value = taskId.ToString();
            int userId = Convert.ToInt32(Session["UserID"]);
            string role = Session["Role"] as string;
            string error = null;
            string success = "Updated.";

            if (string.Equals(e.CommandName, "Progress", StringComparison.OrdinalIgnoreCase))
            {
                var ddl = (DropDownList)e.Item.FindControl("ddlTaskStatus");
                var note = (TextBox)e.Item.FindControl("txtProgressNote");
                error = EventTaskService.PostProgress(
                    eventId,
                    taskId,
                    userId,
                    role,
                    ddl == null ? "" : ddl.SelectedValue,
                    note == null ? "" : note.Text);
                success = "Progress saved in the shared workplace.";
            }
            else if (string.Equals(e.CommandName, "Upload", StringComparison.OrdinalIgnoreCase))
            {
                IList<HttpPostedFile> files;
                error = CollectTypedUploads(e.Item, out files);
                if (error == null)
                    error = EventTaskService.SaveAttachments(eventId, taskId, userId, role, files, Server);
                success = "Files uploaded to the shared workplace.";
            }
            else if (string.Equals(e.CommandName, "Comment", StringComparison.OrdinalIgnoreCase))
            {
                var comment = (TextBox)e.Item.FindControl("txtTaskComment");
                error = EventTaskService.AddComment(eventId, taskId, userId, role, comment == null ? "" : comment.Text);
                success = "Comment posted.";
            }
            else if (string.Equals(e.CommandName, "Approve", StringComparison.OrdinalIgnoreCase)
                || string.Equals(e.CommandName, "Revise", StringComparison.OrdinalIgnoreCase))
            {
                var review = (TextBox)e.Item.FindControl("txtReviewNote");
                bool approve = string.Equals(e.CommandName, "Approve", StringComparison.OrdinalIgnoreCase);
                string result = EventTaskService.Review(taskId, userId, role, approve, review == null ? "" : review.Text);
                if (result == "approved-concluded")
                {
                    Show(null, "Approved. All event tasks are done, so this workplace is concluded.");
                    Reload();
                    return;
                }
                error = result;
                success = approve ? "Task approved." : "Sent back. The assigned member can redo the work.";
            }

            Show(error, success);
            Reload();
        }

        private void Reload()
        {
            access = EventService.GetAccess(eventId, Convert.ToInt32(Session["UserID"]), Session["Role"] as string);
            BindWorkspace();
        }

        private void BindWorkspace()
        {
            EventTaskService.SyncEventStatusFromTasks(eventId, Convert.ToInt32(Session["UserID"]));
            access = EventService.GetAccess(eventId, Convert.ToInt32(Session["UserID"]), Session["Role"] as string);
            ApplyChrome();
            bool canWork = access.IsMember || access.IsSystemAdmin;
            pnlMemberWork.Visible = canWork;
            pnlMemberAside.Visible = canWork;
            pnlIssueOverlay.Visible = canWork;
            pnlPublicPreview.Visible = !canWork;

            EventRecord ev = access.Event;
            litName.Text = Server.HtmlEncode(ev.EventName);
            litMeta.Text = Server.HtmlEncode(ev.EventType + " - " + ev.Visibility + " - " + ev.Status);
            litDescription.Text = string.IsNullOrWhiteSpace(ev.Description) ? "No description." : Server.HtmlEncode(ev.Description);
            litStart.Text = ev.StartDate.HasValue ? ev.StartDate.Value.ToString("MMM dd, yyyy") : "-";
            litEnd.Text = ev.EndDate.HasValue ? ev.EndDate.Value.ToString("MMM dd, yyyy") : "-";
            litVenue.Text = string.IsNullOrWhiteSpace(ev.Venue) ? "-" : Server.HtmlEncode(ev.Venue);
            litMyRole.Text = access.IsMember ? EventService.DisplayRole(access.RoleInEvent) : (access.IsSystemAdmin ? "System administrator" : "Visitor");

            pnlDisabled.Visible = ev.IsDisabled;
            pnlCode.Visible = access.CanManage && !ev.IsDisabled && !string.IsNullOrEmpty(ev.InviteCode);
            litCode.Text = ev.InviteCode;

            btnJoinPublic.Visible = !access.IsMember
                && string.Equals(ev.Visibility, "Public", StringComparison.OrdinalIgnoreCase)
                && EventIsOpen
                && !access.HasPendingJoinRequest;
            pnlJoinPending.Visible = !access.IsMember && access.HasPendingJoinRequest;
            btnAccept.Visible = access.HasPendingInvite && !access.IsMember && !ev.IsDisabled;
            btnLeave.Visible = access.IsMember && !ev.IsDisabled;
            btnDisable.Visible = false;
            btnEnable.Visible = false;
            lnkEditEvent.Visible = RestrictionService.CanEditEvent(eventId, Convert.ToInt32(Session["UserID"]), Session["Role"] as string);
            lnkEditEvent.NavigateUrl = "~/Modules/Events/EditEvent.aspx?EventID=" + eventId;
            lnkScheduleMeeting.NavigateUrl = "~/Modules/UserMeetings/UserMeetings.aspx?EventID=" + eventId;
            lnkScheduleMeeting.Visible = access.IsMember && EventIsOpen;

            if (!canWork)
            {
                BindPublicPreview();
                return;
            }

            bool eventClosed = string.Equals(ev.Status, "Completed", StringComparison.OrdinalIgnoreCase);
            pnlConcluded.Visible = eventClosed;
            pnlCreateTask.Visible = access.CanAssign && !ev.IsDisabled && !eventClosed;
            if (pnlCreateTask.Visible)
            {
                if (string.IsNullOrWhiteSpace(txtTaskDue.Text))
                {
                    DateTime? defaultDue = ev.EndDate ?? ev.StartDate;
                    if (defaultDue.HasValue)
                        txtTaskDue.Text = defaultDue.Value.ToString("yyyy-MM-dd");
                }
                BindMemberDirectory();
            }
            pnlInvite.Visible = access.CanManage && !ev.IsDisabled;
            pnlPending.Visible = access.CanManage && !ev.IsDisabled;
            pnlAddNote.Visible = access.CanModify && !ev.IsDisabled;
            chkPin.Visible = access.CanManage && !ev.IsDisabled;

            DataTable tasks = EventService.ListEventTasks(eventId, Convert.ToInt32(Session["UserID"]));
            rptTasks.DataSource = tasks;
            rptTasks.DataBind();
            pnlNoTasks.Visible = tasks.Rows.Count == 0;
            litTaskCount.Text = tasks.Rows.Count.ToString();
            if (tasks.Rows.Count == 0 && pnlCreateTask.Visible)
                CreateIssueOpen = true;

            DataTable review = tasks.Clone();
            foreach (DataRow row in tasks.Rows)
            {
                string status = Convert.ToString(row["Status"]);
                if (string.Equals(status, EventTaskService.UnderReview, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(status, "Submitted", StringComparison.OrdinalIgnoreCase))
                    review.ImportRow(row);
            }
            pnlReviewQueue.Visible = access.CanManage && !ev.IsDisabled && review.Rows.Count > 0;
            rptReviewQueue.DataSource = review;
            rptReviewQueue.DataBind();

            DataTable notes = EventService.ListNotes(eventId);
            rptNotes.DataSource = notes;
            rptNotes.DataBind();
            pnlNoNotes.Visible = notes.Rows.Count == 0;

            DataTable members = EventService.ListMembers(eventId);
            rptMembers.DataSource = members;
            rptMembers.DataBind();
            litMemberCount.Text = members.Rows.Count.ToString();

            DataTable pending = EventService.ListPendingInvites(eventId);
            rptPending.DataSource = pending;
            rptPending.DataBind();
            pnlPending.Visible = access.CanManage && pending.Rows.Count > 0;

            DataTable joinRequests = EventService.ListJoinRequests(eventId);
            rptJoinRequests.DataSource = joinRequests;
            rptJoinRequests.DataBind();
            pnlJoinRequests.Visible = access.CanReviewJoins && !ev.IsDisabled && joinRequests.Rows.Count > 0;

            BindEventMeetings();
            BindClubLink();
        }

        private void BindClubLink()
        {
            pnlClubLink.Visible = false;
            btnAddClubMembers.Visible = false;
            btnCreateEventConnect.Visible = false;
            lnkEventConnect.Visible = false;
            if (access.Event == null || !access.Event.ClubID.HasValue)
                return;

            ClubRecord club = ClubService.GetClub(access.Event.ClubID.Value);
            if (club == null)
                return;

            pnlClubLink.Visible = access.IsMember || access.IsSystemAdmin;
            litClubLink.Text = Server.HtmlEncode(club.ClubName + " · Club ID " + club.ClubID
                + (string.IsNullOrEmpty(club.InviteCode) ? "" : " · " + club.InviteCode)
                + (club.IsRestricted ? " · Restricted" : ""));

            bool canManage = (access.CanManage || access.CanAssign) && EventIsOpen && !access.Event.IsDisabled && !club.IsRestricted;
            btnAddClubMembers.Visible = canManage;
            int? groupId = ConnectService.FindGroupIdByEvent(eventId);
            if (groupId.HasValue)
            {
                lnkEventConnect.Visible = true;
                lnkEventConnect.NavigateUrl = "~/Modules/Connect/ConnectRoom.aspx?GroupID=" + groupId.Value;
            }
            else
            {
                btnCreateEventConnect.Visible = canManage;
            }
        }

        protected void btnAddClubMembers_Click(object sender, EventArgs e)
        {
            if (access.Event == null || !access.Event.ClubID.HasValue)
                return;
            int added = EventService.AddMembersFromClub(eventId, access.Event.ClubID.Value, Convert.ToInt32(Session["UserID"]));
            Show(null, added + " club member(s) added to this event.");
            Reload();
        }

        protected void btnCreateEventConnect_Click(object sender, EventArgs e)
        {
            int? clubId = access.Event == null ? (int?)null : access.Event.ClubID;
            int groupId;
            string error = ConnectService.CreateGroupForEvent(eventId, Convert.ToInt32(Session["UserID"]), clubId, out groupId);
            if (error != null)
            {
                Show(error, null);
                Reload();
                return;
            }
            Response.Redirect("~/Modules/Connect/ConnectRoom.aspx?GroupID=" + groupId);
        }

        private void BindEventMeetings()
        {
            MeetingService.EnsureSchema();
            DataTable meetings = MeetingService.ListForEvent(eventId);
            rptEventMeetings.DataSource = meetings;
            rptEventMeetings.DataBind();
            pnlNoEventMeetings.Visible = meetings.Rows.Count == 0;
            lnkScheduleMeetingCard.NavigateUrl = "~/Modules/UserMeetings/UserMeetings.aspx?EventID=" + eventId;
            lnkScheduleMeetingCard.Visible = access.IsMember && EventIsOpen;
        }

        private void BindPublicPreview()
        {
            EventTeamProgress progress = EventTaskService.GetEventTeamProgress(eventId);
            litPublicProgressLabel.Text = progress.TotalTasks == 0
                ? "No tasks yet"
                : progress.CompletedTasks + " of " + progress.TotalTasks + " tasks · " + progress.Percent + "%";
            publicProgressFill.Style["width"] = progress.Percent + "%";

            DataTable members = EventService.ListMembers(eventId);
            var leads = new System.Text.StringBuilder();
            var handlers = new System.Text.StringBuilder();
            var team = new System.Text.StringBuilder();
            foreach (DataRow row in members.Rows)
            {
                string name = Convert.ToString(row["FullName"]);
                string role = Convert.ToString(row["RoleInEvent"]);
                if (team.Length > 0)
                    team.Append(", ");
                team.Append(name);
                if (string.Equals(role, EventRoles.EventAdmin, StringComparison.OrdinalIgnoreCase))
                {
                    if (leads.Length > 0)
                        leads.Append(", ");
                    leads.Append(name);
                }
                else if (string.Equals(role, EventRoles.EventManager, StringComparison.OrdinalIgnoreCase))
                {
                    if (handlers.Length > 0)
                        handlers.Append(", ");
                    handlers.Append(name);
                }
            }

            string leadText = leads.Length == 0 ? "Not assigned" : leads.ToString();
            litPublicLead.Text = Server.HtmlEncode(leadText);
            litPublicHandler.Text = Server.HtmlEncode(handlers.Length == 0 ? leadText : handlers.ToString());
            litPublicTeam.Text = members.Rows.Count == 0
                ? "No members listed yet."
                : members.Rows.Count + " on the team: " + Server.HtmlEncode(team.ToString());
        }

        private static string CollectTypedUploads(RepeaterItem item, out IList<HttpPostedFile> files)
        {
            files = new List<HttpPostedFile>();
            string error = AddTypedFiles(item.FindControl("fuPdf") as FileUpload, new[] { ".pdf" }, "PDF", files);
            if (error != null)
                return error;
            error = AddTypedFiles(item.FindControl("fuWord") as FileUpload, new[] { ".doc", ".docx" }, "Word", files);
            if (error != null)
                return error;
            error = AddTypedFiles(item.FindControl("fuImage") as FileUpload, new[] { ".jpg", ".jpeg", ".png" }, "image", files);
            if (error != null)
                return error;
            return AddTypedFiles(item.FindControl("fuZip") as FileUpload, new[] { ".zip" }, "ZIP", files);
        }

        private static string AddTypedFiles(FileUpload upload, string[] allowed, string label, IList<HttpPostedFile> files)
        {
            foreach (HttpPostedFile file in CollectUploads(upload))
            {
                string ext = Path.GetExtension(file.FileName ?? "").ToLowerInvariant();
                bool ok = false;
                for (int i = 0; i < allowed.Length; i++)
                {
                    if (ext == allowed[i])
                    {
                        ok = true;
                        break;
                    }
                }
                if (!ok)
                    return "The " + label + " button only accepts " + string.Join(", ", allowed) + " files.";
                files.Add(file);
            }
            return null;
        }

        private static IList<HttpPostedFile> CollectUploads(FileUpload upload)
        {
            var files = new List<HttpPostedFile>();
            if (upload == null)
                return files;
            if (upload.HasFiles)
            {
                foreach (HttpPostedFile file in upload.PostedFiles)
                {
                    if (file != null && file.ContentLength > 0)
                        files.Add(file);
                }
            }
            else if (upload.HasFile)
            {
                files.Add(upload.PostedFile);
            }
            return files;
        }

        private List<int> SelectedAssigneeIds
        {
            get
            {
                var ids = new List<int>();
                foreach (string part in (hfAssigneeIds.Value ?? "").Split(','))
                {
                    int id;
                    if (int.TryParse(part, out id) && !ids.Contains(id))
                        ids.Add(id);
                }
                return ids;
            }
        }

        private void BindMemberDirectory()
        {
            var json = new StringBuilder("[");
            bool first = true;
            foreach (DataRow row in EventTaskService.ListAssignableMembers(eventId).Rows)
            {
                if (!first)
                    json.Append(",");
                first = false;
                json.Append("{\"id\":").Append(Convert.ToInt32(row["UserID"]))
                    .Append(",\"name\":\"").Append(JsonEscape(Convert.ToString(row["FullName"])))
                    .Append("\",\"username\":\"").Append(JsonEscape(Convert.ToString(row["Username"])))
                    .Append("\",\"email\":\"").Append(JsonEscape(Convert.ToString(row["Email"])))
                    .Append("\"}");
            }
            json.Append("]");
            hfMemberDirectory.Value = json.ToString();
        }

        private static string JsonEscape(string value)
        {
            return (value ?? "")
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "")
                .Replace("\n", " ");
        }

        private void ApplyChrome()
        {
            bool isAdmin = RoleAccess.IsAdmin(Session["Role"] as string);
            adminTop.Visible = isAdmin;
            adminSide.Visible = isAdmin;
            userTop.Visible = !isAdmin;
            userSide.Visible = !isAdmin;
        }

        private void Show(string error, string success)
        {
            if (!string.IsNullOrEmpty(error))
            {
                lblMessage.CssClass = "font-label-md block mb-4 text-error";
                lblMessage.Text = error;
            }
            else
            {
                lblMessage.CssClass = "font-label-md block mb-4 text-tertiary";
                lblMessage.Text = success ?? "";
            }
        }
    }
}
