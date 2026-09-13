using System;
using System.Data;
using System.Web.UI;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.UserMeetings
{
    public partial class UserMeetingDetails : Page
    {
        private int meetingId;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Modules/Authentication/Login.aspx");
                return;
            }

            if (!int.TryParse(Request.QueryString["MeetingID"], out meetingId))
            {
                Response.Redirect("~/Modules/UserMeetings/UserMeetings.aspx");
                return;
            }

            MeetingService.EnsureSchema();
            int userId = Convert.ToInt32(Session["UserID"]);
            if (!MeetingService.CanView(meetingId, userId, Session["Role"] as string))
            {
                Response.Redirect("~/Modules/UserMeetings/UserMeetings.aspx");
                return;
            }

            if (!IsPostBack)
                BindMeeting();
        }

        private void BindMeeting()
        {
            DataRow meeting = MeetingService.GetMeeting(meetingId);
            if (meeting == null)
            {
                Response.Redirect("~/Modules/UserMeetings/UserMeetings.aspx");
                return;
            }

            int userId = Convert.ToInt32(Session["UserID"]);
            bool isHost = Convert.ToInt32(meeting["CreatedBy"]) == userId;

            bool roomOpen = MeetingService.IsRoomOpen(meeting);
            string status = Convert.ToString(meeting["Status"]);
            if (MeetingService.IsClosedStatus(status))
                status = "Ended";

            litTitle.Text = Server.HtmlEncode(meeting["MeetingTitle"].ToString());
            litLinked.Text = Server.HtmlEncode(MeetingService.BuildLinkedLabel(meeting));
            litStatus.Text = Server.HtmlEncode(status);
            litWhen.Text = Convert.ToDateTime(meeting["ScheduledDate"]).ToString("MMM dd, yyyy - hh:mm tt");
            litDuration.Text = meeting["Duration"] == DBNull.Value ? "—" : meeting["Duration"] + " minutes";
            litVenue.Text = Server.HtmlEncode(Convert.ToString(meeting["Venue"]));
            litType.Text = Server.HtmlEncode(Convert.ToString(meeting["MeetingType"]));
            litDescription.Text = meeting["Description"] == DBNull.Value || string.IsNullOrWhiteSpace(meeting["Description"].ToString())
                ? "No description."
                : Server.HtmlEncode(meeting["Description"].ToString());
            litAgenda.Text = meeting["Agenda"] == DBNull.Value || string.IsNullOrWhiteSpace(meeting["Agenda"].ToString())
                ? "No agenda set."
                : Server.HtmlEncode(meeting["Agenda"].ToString());
            bool canWriteMinutes = isHost;
            txtMinutes.Text = meeting["MinutesOfMeeting"] == DBNull.Value ? "" : meeting["MinutesOfMeeting"].ToString();
            txtMinutes.ReadOnly = !canWriteMinutes;
            btnSaveMinutes.Visible = canWriteMinutes;
            litMinutesHint.Text = canWriteMinutes
                ? "Record what was discussed. Only you, as host, can edit these minutes."
                : "Only the meeting host can write minutes. Participants can read them here.";

            string joinUrl = meeting["ZoomJoinUrl"] == DBNull.Value ? "" : meeting["ZoomJoinUrl"].ToString();
            string startUrl = meeting["ZoomStartUrl"] == DBNull.Value ? "" : meeting["ZoomStartUrl"].ToString();
            bool hadZoom = meeting.Table.Columns.Contains("ZoomMeetingId") && meeting["ZoomMeetingId"] != DBNull.Value
                || !string.IsNullOrWhiteSpace(joinUrl)
                || !string.IsNullOrWhiteSpace(startUrl);
            pnlZoom.Visible = hadZoom;
            pnlZoomOpen.Visible = hadZoom && roomOpen;
            pnlZoomClosed.Visible = hadZoom && !roomOpen;
            hidRoomEndsAt.Value = "";
            hidWaitForHost.Value = "";
            if (hadZoom && roomOpen)
            {
                DateTime start = Convert.ToDateTime(meeting["ScheduledDate"]);
                int duration = meeting["Duration"] == DBNull.Value ? 60 : Convert.ToInt32(meeting["Duration"]);
                hidRoomEndsAt.Value = MeetingService.GetRoomEnd(start, duration).ToString("o");

                litPasscode.Text = meeting["ZoomPasscode"] == DBNull.Value || string.IsNullOrWhiteSpace(meeting["ZoomPasscode"].ToString())
                    ? "None"
                    : Server.HtmlEncode(meeting["ZoomPasscode"].ToString());

                string liveStatus = null;
                if (meeting.Table.Columns.Contains("ZoomMeetingId") && meeting["ZoomMeetingId"] != DBNull.Value)
                {
                    ZoomService zoomService = new ZoomService();
                    if (zoomService.IsConfigured)
                        liveStatus = zoomService.GetMeetingLiveStatus(Convert.ToInt64(meeting["ZoomMeetingId"]));
                }
                bool hostStarted = ZoomService.HostHasStarted(liveStatus);
                bool hostNotStarted = string.Equals(liveStatus, "waiting", StringComparison.OrdinalIgnoreCase);

                lnkStart.Visible = isHost && !string.IsNullOrWhiteSpace(startUrl);
                if (lnkStart.Visible)
                    lnkStart.NavigateUrl = startUrl;

                bool showJoin = !string.IsNullOrWhiteSpace(joinUrl) && (isHost ? !lnkStart.Visible : !hostNotStarted);
                if (!string.IsNullOrWhiteSpace(joinUrl))
                {
                    lnkJoin.NavigateUrl = joinUrl;
                    lnkJoinUrlText.Text = Server.HtmlEncode(joinUrl);
                    lnkJoinUrlText.NavigateUrl = joinUrl;
                }
                lnkJoin.Visible = showJoin;
                lnkJoinUrlText.Visible = showJoin || isHost;

                if (isHost)
                    litZoomHelp.Text = "Click Start as host to open the room. Members cannot enter until you start it. After they join, admit them from the Zoom waiting room. The room closes when the scheduled duration ends.";
                else if (hostNotStarted)
                    litZoomHelp.Text = "The host has not started this meeting yet. You cannot enter until the host starts the room. This page will refresh automatically.";
                else if (hostStarted)
                    litZoomHelp.Text = "The host has started the meeting. Click Join. You will wait in the Zoom waiting room until the host admits you.";
                else
                    litZoomHelp.Text = "Click Join when you are ready. You cannot enter until the host starts the room, and you will wait in the waiting room until the host admits you.";

                hidWaitForHost.Value = (!isHost && hostNotStarted) ? "1" : "";
            }

            rptParticipants.DataSource = MeetingService.ListParticipants(meetingId);
            rptParticipants.DataBind();

            bool hasLink = false;
            lnkEvent.Visible = false;
            lnkWork.Visible = false;
            lnkGroup.Visible = false;
            if (meeting.Table.Columns.Contains("EventID") && meeting["EventID"] != DBNull.Value)
            {
                lnkEvent.Visible = true;
                lnkEvent.Text = "Open event: " + meeting["EventName"];
                lnkEvent.NavigateUrl = "~/Modules/Events/EventWorkspace.aspx?EventID=" + meeting["EventID"];
                hasLink = true;
            }
            if (meeting["TaskID"] != DBNull.Value)
            {
                lnkWork.Visible = true;
                lnkWork.Text = "Open workspace: " + meeting["WorkTitle"];
                lnkWork.NavigateUrl = "~/Modules/TaskWorkspaces/TaskWorkspace.aspx?TaskID=" + meeting["TaskID"];
                hasLink = true;
            }
            if (meeting["GroupID"] != DBNull.Value)
            {
                lnkGroup.Visible = true;
                lnkGroup.Text = "Open assignment group: " + meeting["GroupName"];
                lnkGroup.NavigateUrl = "~/Modules/Assignments/GroupWorkspace.aspx?GroupID=" + meeting["GroupID"];
                hasLink = true;
            }
            litNoLink.Visible = !hasLink;

            bool canSendInvites = isHost || RoleAccess.IsAdmin(Session["Role"] as string);
            if (meeting.Table.Columns.Contains("EventID") && meeting["EventID"] != DBNull.Value)
            {
                EventAccess eventAccess = EventService.GetAccess(Convert.ToInt32(meeting["EventID"]), userId, Session["Role"] as string);
                if (eventAccess != null && eventAccess.CanEditDetails)
                    canSendInvites = true;
            }

            btnSendInvites.Visible = roomOpen
                && meeting.Table.Columns.Contains("EventID")
                && meeting["EventID"] != DBNull.Value
                && canSendInvites;
        }

        protected void btnSendInvites_Click(object sender, EventArgs e)
        {
            string error = MeetingService.SendInvitesToEventMembers(meetingId, Convert.ToInt32(Session["UserID"]), Session["Role"] as string);
            if (error != null)
                ShowMessage(error, false);
            else
                ShowMessage("Invite sent to event members, including the Zoom join link.", true);
            BindMeeting();
        }

        protected void btnSaveMinutes_Click(object sender, EventArgs e)
        {
            string error = MeetingService.SaveMinutes(meetingId, Convert.ToInt32(Session["UserID"]), txtMinutes.Text);
            if (error != null)
                ShowMessage(error, false);
            else
                ShowMessage("Minutes saved.", true);
            BindMeeting();
        }

        private void ShowMessage(string text, bool success)
        {
            pnlMessage.Visible = true;
            litMessage.Text = Server.HtmlEncode(text);
            pnlMessage.CssClass = success
                ? "mb-6 p-4 rounded-xl bg-tertiary-container/30 text-on-tertiary-container"
                : "mb-6 p-4 rounded-xl bg-error-container/40 text-error";
        }
    }
}
