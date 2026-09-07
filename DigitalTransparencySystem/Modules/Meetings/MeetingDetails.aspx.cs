using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Meetings
{
    public partial class MeetingDetails : Page
    {
        private string connectionString;
        private int meetingId;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            btnEnableCloud.Click += btnEnableCloud_Click;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            connectionString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;

            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Modules/Authentication/Login.aspx");
                return;
            }

            string role = Session["Role"] as string;
            if (role == null || !role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("~/Modules/Authentication/Login.aspx");
                return;
            }

            if (!int.TryParse(Request.QueryString["MeetingID"], out meetingId))
            {
                Response.Redirect("~/Modules/Meetings/Meetings.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadMeetingDetails();
                LoadZoomInfo();
                LoadParticipants();
                LoadRecordings();
                LoadMeetingDecisions();
                LoadMeetingTasks();
                SetActionButtons();
            }
        }

        private void LoadMeetingDetails()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(@"SELECT MeetingTitle, Description, MeetingType, ScheduledDate,
                    Duration, Venue, Agenda, MinutesOfMeeting, Status
                    FROM Meetings WHERE MeetingID = @MeetingID", con);
                cmd.Parameters.AddWithValue("@MeetingID", meetingId);

                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    litMeetingTitle.Text = reader["MeetingTitle"].ToString();
                    litMeetingType.Text = reader["MeetingType"].ToString();
                    litDate.Text = Convert.ToDateTime(reader["ScheduledDate"]).ToString("MMM dd, yyyy - hh:mm tt");
                    litDuration.Text = reader["Duration"] != DBNull.Value ? reader["Duration"] + " minutes" : "N/A";
                    litVenue.Text = reader["Venue"].ToString();
                    litDescription.Text = reader["Description"] != DBNull.Value ? reader["Description"].ToString() : "No description provided.";
                    litAgenda.Text = reader["Agenda"] != DBNull.Value ? reader["Agenda"].ToString() : "No agenda set.";
                    txtMinutes.Text = reader["MinutesOfMeeting"] != DBNull.Value ? reader["MinutesOfMeeting"].ToString() : "";

                    string status = reader["Status"].ToString();
                    litStatus.Text = GetStatusBadgeHtml(status);
                }
            }
        }

        private void LoadZoomInfo()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(@"SELECT ZoomMeetingId, ZoomJoinUrl, ZoomStartUrl, ZoomPasscode, ZoomHostId
                    FROM Meetings WHERE MeetingID = @MeetingID", con);
                cmd.Parameters.AddWithValue("@MeetingID", meetingId);

                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    string joinUrl = reader["ZoomJoinUrl"] != DBNull.Value ? reader["ZoomJoinUrl"].ToString() : "";
                    string startUrl = reader["ZoomStartUrl"] != DBNull.Value ? reader["ZoomStartUrl"].ToString() : "";
                    string passcode = reader["ZoomPasscode"] != DBNull.Value ? reader["ZoomPasscode"].ToString() : "";
                    bool hasZoomId = reader["ZoomMeetingId"] != DBNull.Value;
                    bool hasZoom = !string.IsNullOrEmpty(joinUrl) || !string.IsNullOrEmpty(startUrl);

                    pnlZoom.Visible = hasZoom;
                    btnEnableCloud.Visible = hasZoom && hasZoomId;
                    if (hasZoom)
                    {
                        if (!string.IsNullOrEmpty(joinUrl))
                        {
                            lnkJoinUrl.NavigateUrl = joinUrl;
                            lnkJoinUrl.Text = joinUrl;
                            lnkJoinButton.NavigateUrl = joinUrl;
                        }
                        else
                        {
                            lnkJoinUrl.Text = "No public join link";
                            lnkJoinButton.Visible = false;
                        }

                        if (!string.IsNullOrEmpty(startUrl))
                        {
                            lnkStartButton.NavigateUrl = startUrl;
                        }
                        else
                        {
                            lnkStartButton.Visible = false;
                        }

                        litPasscode.Text = string.IsNullOrEmpty(passcode) ? "-" : Server.HtmlEncode(passcode);
                    }
                }
            }
        }

        private void LoadRecordings()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(@"SELECT RecordingType, Title, PlayUrl, DownloadUrl, FileSize, DurationSeconds, RecordedAt
                    FROM MeetingRecordings
                    WHERE MeetingID = @MeetingID
                    ORDER BY RecordedAt DESC, RecordingID DESC", con);
                cmd.Parameters.AddWithValue("@MeetingID", meetingId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    rptRecordings.DataSource = dt;
                    rptRecordings.DataBind();
                    pnlRecordings.Visible = true;
                }
                else
                {
                    pnlRecordings.Visible = false;
                }
            }
        }

        protected void btnEnableCloud_Click(object sender, EventArgs e)
        {
            string error = MeetingService.EnableCloudRecording(meetingId);
            if (error != null)
                ShowZoomError(error);
            else
                ShowZoomError("Cloud recording is on. Start the meeting with Start (Host) — Zoom will record automatically.");
            LoadZoomInfo();
        }

        protected void btnSyncRecordings_Click(object sender, EventArgs e)
        {
            SyncZoomRecordings();
        }

        private void SyncZoomRecordings()
        {
            var zoomService = new ZoomService();
            if (!zoomService.IsConfigured)
            {
                ShowZoomError("Zoom is not configured. Add Zoom credentials to Web.config.");
                return;
            }

            if (!SyncRecordingsFromZoom(zoomService))
            {
                ShowZoomError("Could not retrieve recordings from Zoom. Check credentials or whether cloud recording is enabled for your account.");
            }
        }

        private bool SyncRecordingsFromZoom(ZoomService zoomService)
        {
            string hostId = null, joinUrl = null;
            long? zoomMeetingId = null;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(@"SELECT ZoomHostId, ZoomJoinUrl, ZoomMeetingId FROM Meetings WHERE MeetingID = @MeetingID", con);
                cmd.Parameters.AddWithValue("@MeetingID", meetingId);
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    hostId = reader["ZoomHostId"] != DBNull.Value ? reader["ZoomHostId"].ToString() : null;
                    joinUrl = reader["ZoomJoinUrl"] != DBNull.Value ? reader["ZoomJoinUrl"].ToString() : "";
                    zoomMeetingId = reader["ZoomMeetingId"] != DBNull.Value ? (long?)Convert.ToInt64(reader["ZoomMeetingId"]) : null;
                }
            }

            if (zoomMeetingId == null)
            {
                ShowZoomError("This meeting has no Zoom meeting id.");
                return false;
            }

            var recordings = zoomService.GetMeetingRecordings(hostId, zoomMeetingId, DateTime.UtcNow.AddDays(-90), DateTime.UtcNow);
            if (recordings.Count == 0) return true; // no recordings yet is a valid state

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                foreach (var rec in recordings)
                {
                    SqlCommand check = new SqlCommand(
                        "SELECT COUNT(*) FROM MeetingRecordings WHERE MeetingID = @MeetingID AND PlayUrl = @PlayUrl", con);
                    check.Parameters.AddWithValue("@MeetingID", meetingId);
                    check.Parameters.AddWithValue("@PlayUrl", rec.PlayUrl ?? "");
                    int exists = Convert.ToInt32(check.ExecuteScalar());
                    if (exists > 0) continue;

                    SqlCommand insert = new SqlCommand(@"INSERT INTO MeetingRecordings
                        (MeetingID, RecordingType, Title, PlayUrl, DownloadUrl, FileSize, DurationSeconds, RecordedAt)
                        VALUES (@MeetingID, @RecordingType, @Title, @PlayUrl, @DownloadUrl, @FileSize, @DurationSeconds, @RecordedAt)", con);
                    insert.Parameters.AddWithValue("@MeetingID", meetingId);
                    insert.Parameters.AddWithValue("@RecordingType", (object)rec.RecordingType ?? DBNull.Value);
                    insert.Parameters.AddWithValue("@Title", (object)(rec.Title ?? "Meeting Recording") ?? DBNull.Value);
                    insert.Parameters.AddWithValue("@PlayUrl", (object)rec.PlayUrl ?? DBNull.Value);
                    insert.Parameters.AddWithValue("@DownloadUrl", (object)rec.DownloadUrl ?? DBNull.Value);
                    insert.Parameters.AddWithValue("@FileSize", (object)rec.FileSize ?? DBNull.Value);
                    insert.Parameters.AddWithValue("@DurationSeconds", (object)rec.DurationSeconds ?? DBNull.Value);
                    insert.Parameters.AddWithValue("@RecordedAt", (object)rec.RecordedAt ?? DBNull.Value);
                    insert.ExecuteNonQuery();
                }
            }

            LoadRecordings();
            return true;
        }

        private void ShowZoomError(string message)
        {
            pnlZoomError.Visible = true;
            litZoomError.Text = Server.HtmlEncode(message);
        }

        protected string GetRecordingMeta(object durationSeconds, object recordedAt)
        {
            var parts = new System.Collections.Generic.List<string>();
            if (durationSeconds != null && durationSeconds != DBNull.Value)
            {
                long secs = Convert.ToInt64(durationSeconds);
                parts.Add(FormatDuration(secs));
            }
            if (recordedAt != null && recordedAt != DBNull.Value)
            {
                DateTime dt = Convert.ToDateTime(recordedAt);
                parts.Add(dt.ToString("MMM dd, yyyy - hh:mm tt"));
            }
            return parts.Count > 0 ? "- " + string.Join(" - ", parts) : "";
        }

        private string FormatDuration(long seconds)
        {
            long hours = seconds / 3600;
            long mins = (seconds % 3600) / 60;
            long secs = seconds % 60;
            if (hours > 0) return string.Format("{0}h {1}m", hours, mins);
            if (mins > 0) return string.Format("{0}m {1}s", mins, secs);
            return secs + "s";
        }

        protected string BuildRecordingLinks(string playUrl, string downloadUrl)
        {
            var sb = new System.Text.StringBuilder();
            if (!string.IsNullOrEmpty(playUrl))
            {
                sb.Append("<a href=\"" + System.Web.HttpUtility.HtmlAttributeEncode(playUrl) + "\" target=\"_blank\" class=\"inline-flex items-center gap-1 text-xs font-semibold text-primary px-3 py-1.5 bg-primary-container/30 rounded-full hover:bg-primary-container/50 transition-colors\"><span class=\"material-symbols-outlined text-[14px]\">play_arrow</span>Watch</a>");
            }
            if (!string.IsNullOrEmpty(downloadUrl))
            {
                sb.Append("<a href=\"" + System.Web.HttpUtility.HtmlAttributeEncode(downloadUrl) + "\" target=\"_blank\" class=\"inline-flex items-center gap-1 text-xs font-semibold text-on-surface-variant px-3 py-1.5 border border-outline rounded-full hover:bg-surface-container-low transition-colors\"><span class=\"material-symbols-outlined text-[14px]\">download</span>Download</a>");
            }
            if (sb.Length == 0) return "<span class='text-xs text-on-surface-variant'>No link available</span>";
            return sb.ToString();
        }

        private void LoadParticipants()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(@"SELECT u.FullName, mp.Role, mp.AttendanceStatus
                    FROM MeetingParticipants mp
                    INNER JOIN Users u ON mp.UserID = u.UserID
                    WHERE mp.MeetingID = @MeetingID
                    ORDER BY u.FullName", con);
                cmd.Parameters.AddWithValue("@MeetingID", meetingId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                rptParticipants.DataSource = dt;
                rptParticipants.DataBind();
            }
        }

        private void LoadMeetingDecisions()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(@"SELECT DecisionTitle, Status
                    FROM Decisions
                    WHERE MeetingID = @MeetingID
                    ORDER BY DecisionTitle", con);
                cmd.Parameters.AddWithValue("@MeetingID", meetingId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                rptDecisions.DataSource = dt;
                rptDecisions.DataBind();
            }
        }

        private void LoadMeetingTasks()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(@"SELECT TaskTitle, Status, DueDate
                    FROM Tasks
                    WHERE MeetingID = @MeetingID
                    ORDER BY DueDate", con);
                cmd.Parameters.AddWithValue("@MeetingID", meetingId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                rptTasks.DataSource = dt;
                rptTasks.DataBind();
            }
        }

        private void SetActionButtons()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("SELECT Status FROM Meetings WHERE MeetingID = @MeetingID", con);
                cmd.Parameters.AddWithValue("@MeetingID", meetingId);
                con.Open();
                string status = cmd.ExecuteScalar()?.ToString() ?? "";

                btnStartMeeting.Visible = (status == "Scheduled");
                btnEndMeeting.Visible = (status == "InProgress");
                btnRecordDecision.Visible = (status == "InProgress" || status == "Completed");
                btnAssignTask.Visible = false;
            }
        }

        protected void btnStartMeeting_Click(object sender, EventArgs e)
        {
            UpdateMeetingStatus("InProgress");
            Response.Redirect(Request.RawUrl);
        }

        protected void btnEndMeeting_Click(object sender, EventArgs e)
        {
            UpdateMeetingStatus("Completed");
            Response.Redirect(Request.RawUrl);
        }

        protected void btnRecordDecision_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Modules/Decisions/CreateDecision.aspx?MeetingID=" + meetingId);
        }

        protected void btnAssignTask_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Modules/Tasks/CreateTask.aspx?MeetingID=" + meetingId);
        }

        protected void btnSaveMinutes_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("UPDATE Meetings SET MinutesOfMeeting = @Minutes WHERE MeetingID = @MeetingID", con);
                cmd.Parameters.AddWithValue("@Minutes", txtMinutes.Text);
                cmd.Parameters.AddWithValue("@MeetingID", meetingId);
                cmd.ExecuteNonQuery();
            }

            LoadMeetingDetails();
        }

        private void UpdateMeetingStatus(string status)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("UPDATE Meetings SET Status = @Status WHERE MeetingID = @MeetingID", con);
                cmd.Parameters.AddWithValue("@Status", status);
                cmd.Parameters.AddWithValue("@MeetingID", meetingId);
                cmd.ExecuteNonQuery();
            }
        }

        protected string GetInitials(string fullName)
        {
            if (string.IsNullOrEmpty(fullName)) return "?";
            string[] parts = fullName.Trim().Split(' ');
            if (parts.Length >= 2)
                return (parts[0][0] + parts[1][0]).ToString().ToUpper();
            return parts[0][0].ToString().ToUpper();
        }

        protected string GetStatusBadgeHtml(string status)
        {
            switch (status)
            {
                case "Scheduled":
                    return "<span class=\"inline-block px-3 py-1 rounded-full bg-primary-container/20 text-primary font-label-md text-badge-cap font-bold\">Scheduled</span>";
                case "InProgress":
                    return "<span class=\"inline-block px-3 py-1 rounded-full bg-amber-100 text-amber-700 font-label-md text-badge-cap font-bold\">In Progress</span>";
                case "Completed":
                    return "<span class=\"inline-block px-3 py-1 rounded-full bg-green-100 text-green-700 font-label-md text-badge-cap font-bold\">Completed</span>";
                case "Cancelled":
                    return "<span class=\"inline-block px-3 py-1 rounded-full bg-error-container/40 text-error font-label-md text-badge-cap font-bold\">Cancelled</span>";
                default:
                    return "<span class=\"inline-block px-3 py-1 rounded-full bg-surface-container-high text-on-surface-variant font-label-md text-badge-cap font-bold\">" + status + "</span>";
            }
        }

        protected string GetAttendanceBadge(string attendanceStatus)
        {
            switch (attendanceStatus)
            {
                case "Present":
                    return "inline-block px-2 py-0.5 rounded-full bg-green-100 text-green-700 font-label-md text-[10px] font-bold";
                case "Absent":
                    return "inline-block px-2 py-0.5 rounded-full bg-error-container/40 text-error font-label-md text-[10px] font-bold";
                case "Pending":
                    return "inline-block px-2 py-0.5 rounded-full bg-surface-container-high text-on-surface-variant font-label-md text-[10px] font-bold";
                default:
                    return "inline-block px-2 py-0.5 rounded-full bg-surface-container-high text-on-surface-variant font-label-md text-[10px] font-bold";
            }
        }

        protected string GetDecisionStatusBadge(string status)
        {
            switch (status)
            {
                case "Proposed":
                    return "inline-block px-3 py-1 rounded-full bg-primary-container/20 text-primary font-label-md text-badge-cap font-bold";
                case "Approved":
                    return "inline-block px-3 py-1 rounded-full bg-green-100 text-green-700 font-label-md text-badge-cap font-bold";
                case "Rejected":
                    return "inline-block px-3 py-1 rounded-full bg-error-container/40 text-error font-label-md text-badge-cap font-bold";
                default:
                    return "inline-block px-3 py-1 rounded-full bg-surface-container-high text-on-surface-variant font-label-md text-badge-cap font-bold";
            }
        }

        protected string GetTaskStatusBadge(string status)
        {
            switch (status)
            {
                case "Pending":
                    return "inline-block px-3 py-1 rounded-full bg-surface-container-high text-on-surface-variant font-label-md text-badge-cap font-bold";
                case "InProgress":
                    return "inline-block px-3 py-1 rounded-full bg-amber-100 text-amber-700 font-label-md text-badge-cap font-bold";
                case "Completed":
                    return "inline-block px-3 py-1 rounded-full bg-green-100 text-green-700 font-label-md text-badge-cap font-bold";
                default:
                    return "inline-block px-3 py-1 rounded-full bg-surface-container-high text-on-surface-variant font-label-md text-badge-cap font-bold";
            }
        }
    }
}
