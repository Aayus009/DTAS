using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace DigitalTransparencySystem.Helpers
{
    public static class MeetingService
    {
        private static readonly object SchemaLock = new object();
        private static bool schemaReady;
        private static readonly object ReminderLock = new object();
        private static DateTime lastReminderSweep = DateTime.MinValue;
        private static bool hostAdmissionReady;

        public static void EnsureSchema()
        {
            if (schemaReady)
                return;
            lock (SchemaLock)
            {
                if (schemaReady)
                    return;

                using (SqlConnection con = new SqlConnection(AuthService.ConnectionString))
                {
                    con.Open();
                    new SqlCommand(@"
                    IF COL_LENGTH('dbo.Meetings', 'ZoomMeetingId') IS NULL
                        ALTER TABLE dbo.Meetings ADD ZoomMeetingId BIGINT NULL;
                    IF COL_LENGTH('dbo.Meetings', 'ZoomJoinUrl') IS NULL
                        ALTER TABLE dbo.Meetings ADD ZoomJoinUrl NVARCHAR(500) NULL;
                    IF COL_LENGTH('dbo.Meetings', 'ZoomStartUrl') IS NULL
                        ALTER TABLE dbo.Meetings ADD ZoomStartUrl NVARCHAR(500) NULL;
                    IF COL_LENGTH('dbo.Meetings', 'ZoomPasscode') IS NULL
                        ALTER TABLE dbo.Meetings ADD ZoomPasscode NVARCHAR(50) NULL;
                    IF COL_LENGTH('dbo.Meetings', 'ZoomHostId') IS NULL
                        ALTER TABLE dbo.Meetings ADD ZoomHostId NVARCHAR(100) NULL;
                    IF COL_LENGTH('dbo.Meetings', 'TaskID') IS NULL
                        ALTER TABLE dbo.Meetings ADD TaskID INT NULL;
                    IF COL_LENGTH('dbo.Meetings', 'AssignmentID') IS NULL
                        ALTER TABLE dbo.Meetings ADD AssignmentID INT NULL;
                    IF COL_LENGTH('dbo.Meetings', 'GroupID') IS NULL
                        ALTER TABLE dbo.Meetings ADD GroupID INT NULL;
                    IF COL_LENGTH('dbo.Meetings', 'EventID') IS NULL
                        ALTER TABLE dbo.Meetings ADD EventID INT NULL;
                    IF COL_LENGTH('dbo.Meetings', 'ReminderSentAt') IS NULL
                        ALTER TABLE dbo.Meetings ADD ReminderSentAt DATETIME NULL;
                    IF OBJECT_ID('dbo.MeetingRecordings', 'U') IS NULL
                    BEGIN
                        CREATE TABLE dbo.MeetingRecordings (
                            RecordingID INT PRIMARY KEY IDENTITY(1,1),
                            MeetingID INT NOT NULL,
                            RecordingType NVARCHAR(50) DEFAULT 'Zoom',
                            Title NVARCHAR(300) NULL,
                            PlayUrl NVARCHAR(1000) NOT NULL,
                            DownloadUrl NVARCHAR(1000) NULL,
                            FileSize BIGINT NULL,
                            DurationSeconds INT NULL,
                            RecordedAt DATETIME NULL,
                            CreatedAt DATETIME DEFAULT GETDATE(),
                            CONSTRAINT FK_MR_Meeting FOREIGN KEY (MeetingID) REFERENCES dbo.Meetings(MeetingID) ON DELETE CASCADE
                        );
                    END
                ", con).ExecuteNonQuery();
                }
                schemaReady = true;
            }
            EnsureHostAdmission();
        }

        public static void EnsureHostAdmission()
        {
            if (hostAdmissionReady)
                return;
            lock (SchemaLock)
            {
                if (hostAdmissionReady)
                    return;
                hostAdmissionReady = true;
                var worker = new System.Threading.Thread(ApplyHostAdmissionToOpenMeetings);
                worker.IsBackground = true;
                worker.Name = "DTAS-ZoomAdmission";
                worker.Start();
            }
        }

        private static void ApplyHostAdmissionToOpenMeetings()
        {
            try
            {
                ZoomService zoom = new ZoomService();
                if (!zoom.IsConfigured)
                    return;

                var open = new DataTable();
                using (SqlConnection con = new SqlConnection(AuthService.ConnectionString))
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT ZoomMeetingId
                    FROM Meetings
                    WHERE ZoomMeetingId IS NOT NULL
                      AND Status IN (N'Scheduled', N'InProgress')
                      AND DATEADD(MINUTE, CASE WHEN ISNULL(Duration, 0) <= 0 THEN 60 ELSE Duration END, ScheduledDate) > GETDATE()", con))
                {
                    cmd.CommandTimeout = 8;
                    new SqlDataAdapter(cmd).Fill(open);
                }

                foreach (DataRow row in open.Rows)
                {
                    if (row["ZoomMeetingId"] == DBNull.Value)
                        continue;
                    zoom.ApplyHostAdmission(Convert.ToInt64(row["ZoomMeetingId"]));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Zoom host admission: " + ex.Message);
            }
        }

        public static bool IsClosedStatus(string status)
        {
            return string.Equals(status, "Completed", StringComparison.OrdinalIgnoreCase)
                || string.Equals(status, "Cancelled", StringComparison.OrdinalIgnoreCase)
                || string.Equals(status, "Ended", StringComparison.OrdinalIgnoreCase)
                || string.Equals(status, "Closed", StringComparison.OrdinalIgnoreCase);
        }

        public static DateTime GetRoomEnd(DateTime start, int durationMinutes)
        {
            return start.AddMinutes(durationMinutes > 0 ? durationMinutes : 60);
        }

        public static bool IsRoomOpen(DataRow meeting)
        {
            if (meeting == null)
                return false;
            if (IsClosedStatus(Convert.ToString(meeting["Status"])))
                return false;
            DateTime start = Convert.ToDateTime(meeting["ScheduledDate"]);
            int duration = meeting["Duration"] == DBNull.Value ? 60 : Convert.ToInt32(meeting["Duration"]);
            return DateTime.Now < GetRoomEnd(start, duration);
        }

        public static void CloseExpiredMeetings()
        {
            CloseExpiredMeetings(true);
        }

        public static void CloseExpiredMeetings(bool closeZoomRooms)
        {
            EnsureSchema();
            DataTable expired = new DataTable();
            using (SqlConnection con = new SqlConnection(AuthService.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT MeetingID, ZoomMeetingId
                FROM Meetings
                WHERE Status IN (N'Scheduled', N'InProgress')
                  AND DATEADD(MINUTE, CASE WHEN ISNULL(Duration, 0) <= 0 THEN 60 ELSE Duration END, ScheduledDate) <= GETDATE()", con))
            {
                cmd.CommandTimeout = 8;
                new SqlDataAdapter(cmd).Fill(expired);
            }

            if (expired.Rows.Count == 0)
                return;

            using (SqlConnection con = new SqlConnection(AuthService.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(@"
                UPDATE Meetings
                SET Status = N'Completed',
                    ZoomJoinUrl = NULL,
                    ZoomStartUrl = NULL,
                    ZoomPasscode = NULL
                WHERE Status IN (N'Scheduled', N'InProgress')
                  AND DATEADD(MINUTE, CASE WHEN ISNULL(Duration, 0) <= 0 THEN 60 ELSE Duration END, ScheduledDate) <= GETDATE()", con))
            {
                cmd.CommandTimeout = 8;
                con.Open();
                cmd.ExecuteNonQuery();
            }

            if (!closeZoomRooms)
                return;

            ZoomService zoom = new ZoomService();
            if (!zoom.IsConfigured)
                return;

            foreach (DataRow row in expired.Rows)
            {
                if (row["ZoomMeetingId"] == DBNull.Value)
                    continue;
                try
                {
                    zoom.EndAndDeleteMeeting(Convert.ToInt64(row["ZoomMeetingId"]));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Zoom close failed: " + ex.Message);
                }
            }
        }

        public static DataTable ListForUser(int userId, string filter)
        {
            EnsureSchema();
            CloseExpiredMeetings();
            string whereExtra = "";
            if (string.Equals(filter, "Upcoming", StringComparison.OrdinalIgnoreCase))
                whereExtra = "AND DATEADD(MINUTE, CASE WHEN ISNULL(m.Duration, 0) <= 0 THEN 60 ELSE m.Duration END, m.ScheduledDate) > GETDATE() AND m.Status IN ('Scheduled', 'InProgress')";
            else if (string.Equals(filter, "Past", StringComparison.OrdinalIgnoreCase))
                whereExtra = "AND (DATEADD(MINUTE, CASE WHEN ISNULL(m.Duration, 0) <= 0 THEN 60 ELSE m.Duration END, m.ScheduledDate) <= GETDATE() OR m.Status IN ('Completed', 'Cancelled', 'Ended'))";

            using (SqlConnection con = new SqlConnection(AuthService.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT m.MeetingID, m.MeetingTitle, m.Description, m.ScheduledDate, m.Duration, m.Venue, m.Status, m.CreatedBy,
                       m.ZoomJoinUrl, m.ZoomStartUrl,
                       CASE WHEN NULLIF(LTRIM(RTRIM(ISNULL(m.ZoomJoinUrl, ''))), '') IS NOT NULL THEN 1 ELSE 0 END AS HasZoom,
                       (SELECT COUNT(*) FROM MeetingRecordings r WHERE r.MeetingID = m.MeetingID) AS RecordingCount,
                       ISNULL(e.EventName, '') AS EventName,
                       ISNULL(t.TaskTitle, '') AS WorkTitle,
                       ISNULL(a.AssignmentName, '') AS AssignmentName,
                       ISNULL(g.GroupName, '') AS GroupName
                FROM Meetings m
                LEFT JOIN Events e ON m.EventID = e.EventID
                LEFT JOIN Tasks t ON m.TaskID = t.TaskID
                LEFT JOIN Assignments a ON m.AssignmentID = a.AssignmentID
                LEFT JOIN AssignmentGroups g ON m.GroupID = g.GroupID
                WHERE (" + VisibleFilter + @")
                " + whereExtra + @"
                ORDER BY m.ScheduledDate DESC", con))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
                DataTable dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                dt.Columns.Add("LinkedLabel", typeof(string));
                dt.Columns.Add("RoleLabel", typeof(string));
                dt.Columns.Add("StatusLabel", typeof(string));
                dt.Columns.Add("RoomLabel", typeof(string));
                foreach (DataRow row in dt.Rows)
                {
                    row["LinkedLabel"] = BuildLinkedLabel(row);
                    row["RoleLabel"] = Convert.ToInt32(row["CreatedBy"]) == userId ? "Host" : "Participant";
                    string status = Convert.ToString(row["Status"]);
                    bool closed = IsClosedStatus(status);
                    row["StatusLabel"] = closed ? "Ended" : status;
                    row["RoomLabel"] = closed
                        ? "Room closed"
                        : (Convert.ToInt32(row["HasZoom"]) == 1 ? "Join link sent to event members" : "No Zoom link yet");
                }
                return dt;
            }
        }

        private const string VisibleFilter = @"
            m.CreatedBy = @UserID
            OR EXISTS (SELECT 1 FROM MeetingParticipants mp WHERE mp.MeetingID = m.MeetingID AND mp.UserID = @UserID)
            OR (m.TaskID IS NOT NULL AND (
                    EXISTS (
                        SELECT 1 FROM TaskTeams tt
                        INNER JOIN TaskTeamMembers ttm ON ttm.TeamID = tt.TeamID
                        WHERE tt.TaskID = m.TaskID AND ttm.UserID = @UserID AND ttm.Status = 'Accepted')
                    OR EXISTS (SELECT 1 FROM TaskTeams tt WHERE tt.TaskID = m.TaskID AND tt.LeaderID = @UserID)
                ))
            OR (m.GroupID IS NOT NULL AND (
                    EXISTS (SELECT 1 FROM AssignmentMembers am WHERE am.GroupID = m.GroupID AND am.UserID = @UserID)
                    OR EXISTS (SELECT 1 FROM AssignmentGroups ag WHERE ag.GroupID = m.GroupID AND ag.LeaderID = @UserID)
                ))
            OR (m.AssignmentID IS NOT NULL AND EXISTS (
                    SELECT 1 FROM Assignments ax WHERE ax.AssignmentID = m.AssignmentID AND ax.CreatedBy = @UserID AND ax.IsDeleted = 0))
            OR (m.EventID IS NOT NULL AND EXISTS (
                    SELECT 1 FROM EventMembers em
                    WHERE em.EventID = m.EventID AND em.UserID = @UserID
                      AND em.IsActive = 1 AND em.InviteStatus = N'Accepted'))";

        public static bool CanView(int meetingId, int userId, string role)
        {
            if (RoleAccess.IsAdmin(role))
                return true;
            EnsureSchema();
            using (SqlConnection con = new SqlConnection(AuthService.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT COUNT(1) FROM Meetings m WHERE m.MeetingID = @MeetingID AND (" + VisibleFilter + ")", con))
            {
                cmd.Parameters.AddWithValue("@MeetingID", meetingId);
                cmd.Parameters.AddWithValue("@UserID", userId);
                con.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        public static DataRow GetMeeting(int meetingId)
        {
            EnsureSchema();
            CloseExpiredMeetings();
            using (SqlConnection con = new SqlConnection(AuthService.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT m.*, ISNULL(e.EventName, '') AS EventName, ISNULL(t.TaskTitle, '') AS WorkTitle,
                       ISNULL(a.AssignmentName, '') AS AssignmentName, ISNULL(g.GroupName, '') AS GroupName
                FROM Meetings m
                LEFT JOIN Events e ON m.EventID = e.EventID
                LEFT JOIN Tasks t ON m.TaskID = t.TaskID
                LEFT JOIN Assignments a ON m.AssignmentID = a.AssignmentID
                LEFT JOIN AssignmentGroups g ON m.GroupID = g.GroupID
                WHERE m.MeetingID = @MeetingID", con))
            {
                cmd.Parameters.AddWithValue("@MeetingID", meetingId);
                DataTable dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt.Rows.Count == 0 ? null : dt.Rows[0];
            }
        }

        public static DataTable ListParticipants(int meetingId)
        {
            using (SqlConnection con = new SqlConnection(AuthService.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT ISNULL(u.FullName, u.Username) AS FullName, ISNULL(mp.Role, 'Participant') AS Role,
                       ISNULL(mp.AttendanceStatus, 'Pending') AS AttendanceStatus
                FROM MeetingParticipants mp
                INNER JOIN Users u ON u.UserID = mp.UserID
                WHERE mp.MeetingID = @MeetingID
                ORDER BY CASE WHEN mp.Role = 'Host' THEN 0 ELSE 1 END, u.FullName", con))
            {
                cmd.Parameters.AddWithValue("@MeetingID", meetingId);
                DataTable dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt;
            }
        }

        public static DataTable ListRecordings(int meetingId)
        {
            EnsureSchema();
            using (SqlConnection con = new SqlConnection(AuthService.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT RecordingType, Title, PlayUrl, DownloadUrl, FileSize, DurationSeconds, RecordedAt
                FROM MeetingRecordings
                WHERE MeetingID = @MeetingID
                ORDER BY RecordedAt DESC, RecordingID DESC", con))
            {
                cmd.Parameters.AddWithValue("@MeetingID", meetingId);
                DataTable dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt;
            }
        }

        public static DataTable ListLinkableWorks(int userId)
        {
            using (SqlConnection con = new SqlConnection(AuthService.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT DISTINCT t.TaskID, t.TaskTitle
                FROM Tasks t
                WHERE EXISTS (
                        SELECT 1 FROM TaskTeams tt
                        INNER JOIN TaskTeamMembers ttm ON ttm.TeamID = tt.TeamID
                        WHERE tt.TaskID = t.TaskID AND ttm.UserID = @UserID AND ttm.Status = 'Accepted')
                   OR EXISTS (SELECT 1 FROM TaskTeams tt WHERE tt.TaskID = t.TaskID AND tt.LeaderID = @UserID)
                ORDER BY t.TaskTitle", con))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
                DataTable dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt;
            }
        }

        public static DataTable ListLinkableGroups(int userId)
        {
            AssignmentService.EnsureSchema();
            using (SqlConnection con = new SqlConnection(AuthService.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT g.GroupID, g.GroupName, a.AssignmentID, a.AssignmentName
                FROM AssignmentGroups g
                INNER JOIN Assignments a ON a.AssignmentID = g.AssignmentID AND a.IsDeleted = 0
                WHERE g.IsDeleted = 0 AND (
                    a.CreatedBy = @UserID
                    OR EXISTS (SELECT 1 FROM AssignmentMembers m WHERE m.GroupID = g.GroupID AND m.UserID = @UserID)
                )
                ORDER BY a.AssignmentName, g.GroupName", con))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
                DataTable dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt;
            }
        }

        public static DataTable ListLinkableAssignments(int userId, string role)
        {
            if (!RoleAccess.CanCreateAssignments(role) && !RoleAccess.IsFacultyOrStaff(role))
                return new DataTable();
            AssignmentService.EnsureSchema();
            return AssignmentService.ListAssignments(userId, role);
        }

        public static DataTable ListLinkableEvents(int userId)
        {
            using (SqlConnection con = new SqlConnection(AuthService.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT e.EventID, e.EventName
                FROM Events e
                INNER JOIN EventMembers em ON em.EventID = e.EventID
                WHERE em.UserID = @UserID AND em.IsActive = 1 AND em.InviteStatus = N'Accepted'
                  AND ISNULL(e.IsDeleted, 0) = 0 AND ISNULL(e.IsDisabled, 0) = 0
                  AND e.Status NOT IN (N'Proposed', N'Rejected', N'Archived', N'Cancelled')
                ORDER BY e.EventName", con))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
                DataTable dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt;
            }
        }

        public static DataTable ListForEvent(int eventId)
        {
            EnsureSchema();
            using (SqlConnection con = new SqlConnection(AuthService.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT MeetingID, MeetingTitle, ScheduledDate, Duration, Venue, Status, ZoomJoinUrl,
                       CASE WHEN NULLIF(LTRIM(RTRIM(ISNULL(ZoomJoinUrl, ''))), '') IS NOT NULL THEN 1 ELSE 0 END AS HasZoom
                FROM Meetings
                WHERE EventID = @EventID
                ORDER BY ScheduledDate DESC", con))
            {
                cmd.Parameters.AddWithValue("@EventID", eventId);
                DataTable dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                return dt;
            }
        }

        public static string Create(int creatorId, string title, string description, string meetingType,
            DateTime scheduledDate, int duration, string venue, string agenda, bool createZoom,
            int? taskId, int? assignmentId, int? groupId, int? eventId, string joinUrl, out int meetingId)
        {
            meetingId = 0;
            EnsureSchema();
            title = (title ?? "").Trim();
            if (string.IsNullOrEmpty(title))
                return "Add a meeting title.";

            if (groupId.HasValue && groupId.Value > 0 && !assignmentId.HasValue)
            {
                AssignmentGroupRecord group = AssignmentService.GetGroup(groupId.Value);
                if (group != null)
                    assignmentId = group.AssignmentID;
            }

            if ((!eventId.HasValue || eventId.Value <= 0) && taskId.HasValue && taskId.Value > 0)
                eventId = ReadTaskEventId(taskId.Value);

            if (createZoom && (!eventId.HasValue || eventId.Value <= 0)
                && (!taskId.HasValue || taskId.Value <= 0)
                && (!groupId.HasValue || groupId.Value <= 0)
                && (!assignmentId.HasValue || assignmentId.Value <= 0))
                return "Choose an event or an assignment group. The Zoom join link is sent to that group's members.";

            joinUrl = (joinUrl ?? "").Trim();
            bool hasManualLink = joinUrl.Length > 0;

            ZoomMeetingResult zoom = null;
            if (createZoom)
            {
                ZoomService zoomService = new ZoomService();
                if (zoomService.IsConfigured)
                {
                    string tz = ConfigurationManager.AppSettings["Zoom_Timezone"];
                    if (string.IsNullOrWhiteSpace(tz))
                        tz = "Asia/Kolkata";

                    zoom = zoomService.CreateMeeting(title, scheduledDate, duration, agenda ?? "", tz);
                    if (zoom == null || !string.IsNullOrWhiteSpace(zoom.Error) || string.IsNullOrWhiteSpace(zoom.JoinUrl))
                        return zoom != null && !string.IsNullOrWhiteSpace(zoom.Error)
                            ? zoom.Error
                            : "The Zoom room could not be created. Check the Zoom app credentials and scopes.";
                    if (zoom.MeetingId > 0)
                        zoomService.ApplyHostAdmission(zoom.MeetingId);
                }
                else if (hasManualLink)
                {
                    zoom = new ZoomMeetingResult { JoinUrl = joinUrl };
                }
                else
                {
                    return "Zoom is not configured in Web.config. Add Zoom_AccountId, Zoom_ClientId, and Zoom_ClientSecret so DTAS can create the room.";
                }
            }
            else if (hasManualLink)
            {
                zoom = new ZoomMeetingResult { JoinUrl = joinUrl };
            }

            if (string.IsNullOrWhiteSpace(venue))
                venue = zoom != null && !string.IsNullOrWhiteSpace(zoom.JoinUrl) ? "Zoom (online)" : "To be confirmed";

            using (SqlConnection con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                using (SqlTransaction tx = con.BeginTransaction())
                {
                    try
                    {
                        SqlCommand cmd = new SqlCommand(@"
                            INSERT INTO Meetings
                                (MeetingTitle, Description, MeetingType, ScheduledDate, Duration, Venue, Agenda, Status, CreatedBy,
                                 ZoomMeetingId, ZoomJoinUrl, ZoomStartUrl, ZoomPasscode, ZoomHostId, TaskID, AssignmentID, GroupID, EventID)
                            VALUES
                                (@Title, @Description, @MeetingType, @ScheduledDate, @Duration, @Venue, @Agenda, 'Scheduled', @CreatedBy,
                                 @ZoomMeetingId, @ZoomJoinUrl, @ZoomStartUrl, @ZoomPasscode, @ZoomHostId, @TaskID, @AssignmentID, @GroupID, @EventID);
                            SELECT CAST(SCOPE_IDENTITY() AS INT);", con, tx);
                        cmd.Parameters.AddWithValue("@Title", title);
                        cmd.Parameters.AddWithValue("@Description", (object)NullIfEmpty(description) ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@MeetingType", string.IsNullOrWhiteSpace(meetingType) ? "General" : meetingType);
                        cmd.Parameters.AddWithValue("@ScheduledDate", scheduledDate);
                        cmd.Parameters.AddWithValue("@Duration", duration);
                        cmd.Parameters.AddWithValue("@Venue", venue.Trim());
                        cmd.Parameters.AddWithValue("@Agenda", (object)NullIfEmpty(agenda) ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@CreatedBy", creatorId);
                        cmd.Parameters.AddWithValue("@ZoomMeetingId", zoom == null || zoom.MeetingId <= 0 ? (object)DBNull.Value : zoom.MeetingId);
                        cmd.Parameters.AddWithValue("@ZoomJoinUrl", zoom == null || string.IsNullOrWhiteSpace(zoom.JoinUrl) ? (object)DBNull.Value : zoom.JoinUrl);
                        cmd.Parameters.AddWithValue("@ZoomStartUrl", zoom == null || string.IsNullOrWhiteSpace(zoom.StartUrl) ? (object)DBNull.Value : zoom.StartUrl);
                        cmd.Parameters.AddWithValue("@ZoomPasscode", zoom == null || string.IsNullOrWhiteSpace(zoom.Passcode) ? (object)DBNull.Value : zoom.Passcode);
                        cmd.Parameters.AddWithValue("@ZoomHostId", zoom == null || string.IsNullOrWhiteSpace(zoom.HostId) ? (object)DBNull.Value : zoom.HostId);
                        cmd.Parameters.AddWithValue("@TaskID", taskId.HasValue && taskId.Value > 0 ? (object)taskId.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@AssignmentID", assignmentId.HasValue && assignmentId.Value > 0 ? (object)assignmentId.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@GroupID", groupId.HasValue && groupId.Value > 0 ? (object)groupId.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@EventID", eventId.HasValue && eventId.Value > 0 ? (object)eventId.Value : DBNull.Value);
                        meetingId = Convert.ToInt32(cmd.ExecuteScalar());

                        AddParticipant(con, tx, meetingId, creatorId, "Host");
                        foreach (int memberId in CollectLinkedMembers(con, tx, taskId, assignmentId, groupId, eventId))
                            AddParticipant(con, tx, meetingId, memberId, memberId == creatorId ? "Host" : "Participant");

                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }

            NotifyMeetingCreated(meetingId, creatorId);
            SendDueReminders(true);
            return null;
        }

        public static string SendInvitesToEventMembers(int meetingId, int actorId, string actorRole)
        {
            EnsureSchema();
            DataRow meeting = GetMeeting(meetingId);
            if (meeting == null)
                return "Meeting not found.";
            if (!IsRoomOpen(meeting))
                return "This meeting has ended. The Zoom room is closed, so the join link cannot be sent again.";
            if (meeting["EventID"] == DBNull.Value)
                return "Link this meeting to an event before sending invites.";

            int eventId = Convert.ToInt32(meeting["EventID"]);
            int createdBy = Convert.ToInt32(meeting["CreatedBy"]);
            EventAccess access = EventService.GetAccess(eventId, actorId, actorRole);
            if (actorId != createdBy && (access == null || !access.CanEditDetails) && !RoleAccess.IsAdmin(actorRole))
                return "Only the host, event lead, or manager can send the invite.";

            int added = 0;
            using (SqlConnection con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                using (SqlTransaction tx = con.BeginTransaction())
                {
                    foreach (int memberId in CollectEventMemberIds(con, tx, eventId))
                    {
                        if (IsParticipant(con, tx, meetingId, memberId))
                            continue;
                        AddParticipant(con, tx, meetingId, memberId, memberId == createdBy ? "Host" : "Participant");
                        added++;
                    }
                    tx.Commit();
                }
            }

            NotifyMeetingCreated(meetingId, actorId);
            AuthService.WriteAudit(actorId, "MeetingInvitesSent", "Meeting", meetingId, added + " members added", null);
            return null;
        }

        private static void NotifyMeetingCreated(int meetingId, int creatorId)
        {
            DataRow meeting = GetMeeting(meetingId);
            if (meeting == null)
                return;

            string title = Convert.ToString(meeting["MeetingTitle"]);
            string when = Convert.ToDateTime(meeting["ScheduledDate"]).ToString("MMM dd, yyyy hh:mm tt");
            string eventName = meeting.Table.Columns.Contains("EventName") ? Convert.ToString(meeting["EventName"]) : "";
            string joinUrl = MeetingText(meeting, "ZoomJoinUrl");
            string passcode = MeetingText(meeting, "ZoomPasscode");

            string message = "A meeting was scheduled";
            if (!string.IsNullOrWhiteSpace(eventName))
                message += " for \"" + eventName + "\"";
            message += ": " + title + " on " + when + ".";
            if (!string.IsNullOrWhiteSpace(joinUrl))
            {
                message += " Join: " + joinUrl;
                if (!string.IsNullOrWhiteSpace(passcode))
                    message += " Passcode: " + passcode;
            }
            else
            {
                message += " Open My Meetings for details.";
            }

            List<int> ids = ListParticipantIds(meetingId);
            NotificationService.NotifyUsers(ids, creatorId, "Meeting scheduled", message, "Meeting", meetingId, "Meeting");

            foreach (int userId in ids)
            {
                UserAccount user = AuthService.FindById(userId);
                if (user == null || string.IsNullOrWhiteSpace(user.Email))
                    continue;
                MailSender.Send(
                    user.Email,
                    "Meeting scheduled: " + title,
                    BuildParticipantMail(meeting, false, MailComposer.FirstName(user.FullName)));
            }
        }

        public static void SendDueReminders(bool force = false)
        {
            lock (ReminderLock)
            {
                if (!force && (DateTime.UtcNow - lastReminderSweep).TotalSeconds < 45)
                    return;
                lastReminderSweep = DateTime.UtcNow;
            }

            try
            {
                EnsureSchema();
                CloseExpiredMeetings(false);

                var due = new DataTable();
                using (SqlConnection con = new SqlConnection(AuthService.ConnectionString))
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT MeetingID
                    FROM Meetings
                    WHERE Status IN (N'Scheduled', N'InProgress')
                      AND ScheduledDate > GETDATE()
                      AND ScheduledDate <= DATEADD(HOUR, 2, GETDATE())
                      AND ReminderSentAt IS NULL", con))
                {
                    cmd.CommandTimeout = 8;
                    new SqlDataAdapter(cmd).Fill(due);
                }

                foreach (DataRow row in due.Rows)
                    SendReminderForMeeting(Convert.ToInt32(row["MeetingID"]));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Meeting reminders: " + ex.Message);
            }
        }

        private static void SendReminderForMeeting(int meetingId)
        {
            using (SqlConnection con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                SqlCommand claim = new SqlCommand(@"
                    UPDATE Meetings
                    SET ReminderSentAt = GETDATE()
                    WHERE MeetingID = @MeetingID
                      AND ReminderSentAt IS NULL
                      AND Status IN (N'Scheduled', N'InProgress')
                      AND ScheduledDate > GETDATE()
                      AND ScheduledDate <= DATEADD(HOUR, 2, GETDATE())", con);
                claim.Parameters.AddWithValue("@MeetingID", meetingId);
                if (claim.ExecuteNonQuery() <= 0)
                    return;
            }

            DataRow meeting = GetMeeting(meetingId);
            if (meeting == null)
                return;

            string title = Convert.ToString(meeting["MeetingTitle"]);
            string when = Convert.ToDateTime(meeting["ScheduledDate"]).ToString("MMM dd, yyyy hh:mm tt");
            string notice = "Reminder: \"" + title + "\" starts at " + when + ".";
            List<int> ids = ListParticipantIds(meetingId);
            NotificationService.NotifyUsers(ids, null, "Meeting starts in 2 hours", notice, "MeetingReminder", meetingId, "Meeting");

            foreach (int userId in ids)
            {
                UserAccount user = AuthService.FindById(userId);
                if (user == null || string.IsNullOrWhiteSpace(user.Email))
                    continue;
                MailSender.Send(
                    user.Email,
                    "Reminder: " + title + " starts soon",
                    BuildParticipantMail(meeting, true, MailComposer.FirstName(user.FullName)));
            }
        }

        private static string MeetingText(DataRow meeting, string column)
        {
            if (meeting == null || !meeting.Table.Columns.Contains(column) || meeting[column] == DBNull.Value)
                return "";
            return Convert.ToString(meeting[column]) ?? "";
        }

        private static List<int> ListParticipantIds(int meetingId)
        {
            var ids = new List<int>();
            using (SqlConnection con = new SqlConnection(AuthService.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT UserID FROM MeetingParticipants WHERE MeetingID = @MeetingID", con))
            {
                cmd.Parameters.AddWithValue("@MeetingID", meetingId);
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        ids.Add(Convert.ToInt32(reader["UserID"]));
                }
            }
            return ids;
        }

        private static MailContent BuildParticipantMail(DataRow meeting, bool reminder, string greetingName)
        {
            string title = Convert.ToString(meeting["MeetingTitle"]);
            DateTime start = Convert.ToDateTime(meeting["ScheduledDate"]);
            string when = start.ToString("MMM dd, yyyy hh:mm tt");
            string eventName = MeetingText(meeting, "EventName");
            string joinUrl = MeetingText(meeting, "ZoomJoinUrl");
            string passcode = MeetingText(meeting, "ZoomPasscode");
            int duration = meeting["Duration"] == DBNull.Value ? 60 : Convert.ToInt32(meeting["Duration"]);
            if (duration <= 0)
                duration = 60;

            string intro = reminder
                ? "This is a reminder. Your DTAS meeting starts in about 2 hours."
                : "A DTAS meeting was scheduled and you are on the participant list.";

            var details = new List<MailDetail>
            {
                new MailDetail("Meeting", title),
                new MailDetail("When", when),
                new MailDetail("Duration", duration + " minutes")
            };
            if (!string.IsNullOrWhiteSpace(eventName))
                details.Insert(1, new MailDetail("Event", eventName));
            string venue = MeetingText(meeting, "Venue");
            if (!string.IsNullOrWhiteSpace(venue))
                details.Add(new MailDetail("Venue", venue));
            if (!string.IsNullOrWhiteSpace(joinUrl))
                details.Add(new MailDetail("Zoom", joinUrl));
            if (!string.IsNullOrWhiteSpace(passcode))
                details.Add(new MailDetail("Passcode", passcode));

            string meetingUrl = MailSender.AbsoluteUrl("~/Modules/UserMeetings/UserMeetings.aspx");
            return MailComposer.Build(
                null,
                intro,
                reminder
                    ? "Please wait for the host to start the Zoom room. After that, the host must admit you from the waiting room."
                    : "The host must start the Zoom room first. Members cannot enter until the host starts it and admits them from the waiting room.",
                details,
                string.IsNullOrWhiteSpace(joinUrl) ? "Open My Meetings" : "Join Zoom",
                string.IsNullOrWhiteSpace(joinUrl) ? meetingUrl : joinUrl,
                greetingName);
        }

        private static IEnumerable<int> CollectLinkedMembers(SqlConnection con, SqlTransaction tx, int? taskId, int? assignmentId, int? groupId, int? eventId)
        {
            HashSet<int> ids = new HashSet<int>();
            if (taskId.HasValue && taskId.Value > 0)
            {
                AddIds(con, tx, ids, @"
                    SELECT ttm.UserID FROM TaskTeamMembers ttm
                    INNER JOIN TaskTeams tt ON tt.TeamID = ttm.TeamID
                    WHERE tt.TaskID = @ID AND ttm.Status = 'Accepted'
                    UNION
                    SELECT LeaderID FROM TaskTeams WHERE TaskID = @ID AND LeaderID IS NOT NULL", taskId.Value);
            }
            if (groupId.HasValue && groupId.Value > 0)
            {
                AddIds(con, tx, ids, @"
                    SELECT UserID FROM AssignmentMembers WHERE GroupID = @ID
                    UNION
                    SELECT LeaderID FROM AssignmentGroups WHERE GroupID = @ID AND LeaderID IS NOT NULL", groupId.Value);
            }
            else if (assignmentId.HasValue && assignmentId.Value > 0)
            {
                AddIds(con, tx, ids, @"
                    SELECT m.UserID FROM AssignmentMembers m
                    INNER JOIN AssignmentGroups g ON g.GroupID = m.GroupID AND g.IsDeleted = 0
                    WHERE g.AssignmentID = @ID
                    UNION
                    SELECT CreatedBy FROM Assignments WHERE AssignmentID = @ID", assignmentId.Value);
            }
            if (eventId.HasValue && eventId.Value > 0)
                CollectEventMemberIds(con, tx, eventId.Value, ids);
            return ids;
        }

        private static IEnumerable<int> CollectEventMemberIds(SqlConnection con, SqlTransaction tx, int eventId)
        {
            HashSet<int> ids = new HashSet<int>();
            CollectEventMemberIds(con, tx, eventId, ids);
            return ids;
        }

        private static void CollectEventMemberIds(SqlConnection con, SqlTransaction tx, int eventId, HashSet<int> ids)
        {
            AddIds(con, tx, ids, @"
                SELECT UserID FROM EventMembers
                WHERE EventID = @ID AND IsActive = 1 AND InviteStatus = N'Accepted'
                UNION
                SELECT ProposedBy FROM Events WHERE EventID = @ID AND ProposedBy IS NOT NULL
                UNION
                SELECT CreatedBy FROM Events WHERE EventID = @ID AND CreatedBy IS NOT NULL", eventId);
        }

        private static bool IsParticipant(SqlConnection con, SqlTransaction tx, int meetingId, int userId)
        {
            SqlCommand check = new SqlCommand(
                "SELECT COUNT(*) FROM MeetingParticipants WHERE MeetingID = @MeetingID AND UserID = @UserID", con, tx);
            check.Parameters.AddWithValue("@MeetingID", meetingId);
            check.Parameters.AddWithValue("@UserID", userId);
            return Convert.ToInt32(check.ExecuteScalar()) > 0;
        }

        private static int? ReadTaskEventId(int taskId)
        {
            using (SqlConnection con = new SqlConnection(AuthService.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT EventID FROM Tasks WHERE TaskID = @TaskID AND ISNULL(IsDeleted, 0) = 0", con))
            {
                cmd.Parameters.AddWithValue("@TaskID", taskId);
                con.Open();
                object value = cmd.ExecuteScalar();
                if (value == null || value == DBNull.Value)
                    return null;
                return Convert.ToInt32(value);
            }
        }

        private static void AddIds(SqlConnection con, SqlTransaction tx, HashSet<int> ids, string sql, int id)
        {
            using (SqlCommand cmd = new SqlCommand(sql, con, tx))
            {
                cmd.Parameters.AddWithValue("@ID", id);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader[0] != DBNull.Value)
                            ids.Add(Convert.ToInt32(reader[0]));
                    }
                }
            }
        }

        private static void AddParticipant(SqlConnection con, SqlTransaction tx, int meetingId, int userId, string role)
        {
            SqlCommand check = new SqlCommand(
                "SELECT COUNT(*) FROM MeetingParticipants WHERE MeetingID = @MeetingID AND UserID = @UserID", con, tx);
            check.Parameters.AddWithValue("@MeetingID", meetingId);
            check.Parameters.AddWithValue("@UserID", userId);
            if (Convert.ToInt32(check.ExecuteScalar()) > 0)
                return;

            SqlCommand cmd = new SqlCommand(@"
                INSERT INTO MeetingParticipants (MeetingID, UserID, Role, AttendanceStatus)
                VALUES (@MeetingID, @UserID, @Role, 'Pending')", con, tx);
            cmd.Parameters.AddWithValue("@MeetingID", meetingId);
            cmd.Parameters.AddWithValue("@UserID", userId);
            cmd.Parameters.AddWithValue("@Role", role);
            cmd.ExecuteNonQuery();
        }

        public static string SyncRecordings(int meetingId)
        {
            EnsureSchema();
            ZoomService zoomService = new ZoomService();
            if (!zoomService.IsConfigured)
                return "Zoom is not configured.";

            string hostId = null;
            long? zoomMeetingId = null;
            using (SqlConnection con = new SqlConnection(AuthService.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT ZoomHostId, ZoomMeetingId FROM Meetings WHERE MeetingID = @MeetingID", con))
            {
                cmd.Parameters.AddWithValue("@MeetingID", meetingId);
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return "Meeting not found.";
                    hostId = reader["ZoomHostId"] == DBNull.Value ? null : reader["ZoomHostId"].ToString();
                    zoomMeetingId = reader["ZoomMeetingId"] == DBNull.Value ? (long?)null : Convert.ToInt64(reader["ZoomMeetingId"]);
                }
            }

            if (zoomMeetingId == null)
                return "This meeting has no Zoom meeting id.";

            List<ZoomRecording> recordings = zoomService.GetMeetingRecordings(hostId, zoomMeetingId, DateTime.UtcNow.AddDays(-90), DateTime.UtcNow);
            if (recordings == null || recordings.Count == 0)
                return null;

            using (SqlConnection con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                foreach (ZoomRecording rec in recordings)
                {
                    if (string.IsNullOrEmpty(rec.PlayUrl))
                        continue;
                    SqlCommand check = new SqlCommand(
                        "SELECT COUNT(*) FROM MeetingRecordings WHERE MeetingID = @MeetingID AND PlayUrl = @PlayUrl", con);
                    check.Parameters.AddWithValue("@MeetingID", meetingId);
                    check.Parameters.AddWithValue("@PlayUrl", rec.PlayUrl);
                    if (Convert.ToInt32(check.ExecuteScalar()) > 0)
                        continue;

                    SqlCommand insert = new SqlCommand(@"
                        INSERT INTO MeetingRecordings
                            (MeetingID, RecordingType, Title, PlayUrl, DownloadUrl, FileSize, DurationSeconds, RecordedAt)
                        VALUES (@MeetingID, @RecordingType, @Title, @PlayUrl, @DownloadUrl, @FileSize, @DurationSeconds, @RecordedAt)", con);
                    insert.Parameters.AddWithValue("@MeetingID", meetingId);
                    insert.Parameters.AddWithValue("@RecordingType", (object)rec.RecordingType ?? "Zoom");
                    insert.Parameters.AddWithValue("@Title", (object)(rec.Title ?? "Meeting recording") ?? DBNull.Value);
                    insert.Parameters.AddWithValue("@PlayUrl", rec.PlayUrl);
                    insert.Parameters.AddWithValue("@DownloadUrl", (object)rec.DownloadUrl ?? DBNull.Value);
                    insert.Parameters.AddWithValue("@FileSize", rec.FileSize.HasValue ? (object)rec.FileSize.Value : DBNull.Value);
                    insert.Parameters.AddWithValue("@DurationSeconds", rec.DurationSeconds.HasValue ? (object)rec.DurationSeconds.Value : DBNull.Value);
                    insert.Parameters.AddWithValue("@RecordedAt", rec.RecordedAt.HasValue ? (object)rec.RecordedAt.Value : DBNull.Value);
                    insert.ExecuteNonQuery();
                }
            }
            return null;
        }

        public static string EnableCloudRecording(int meetingId)
        {
            EnsureSchema();
            ZoomService zoomService = new ZoomService();
            if (!zoomService.IsConfigured)
                return "Zoom is not configured.";

            long? zoomMeetingId = null;
            using (SqlConnection con = new SqlConnection(AuthService.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT ZoomMeetingId FROM Meetings WHERE MeetingID = @MeetingID", con))
            {
                cmd.Parameters.AddWithValue("@MeetingID", meetingId);
                con.Open();
                object value = cmd.ExecuteScalar();
                if (value != null && value != DBNull.Value)
                    zoomMeetingId = Convert.ToInt64(value);
            }

            if (zoomMeetingId == null)
                return "This meeting has no Zoom meeting id. A pasted invite link cannot turn on cloud recording — schedule a new Zoom meeting from DTAS.";

            return zoomService.EnableCloudRecording(zoomMeetingId.Value);
        }

        public static bool IsHost(int meetingId, int userId)
        {
            DataRow meeting = GetMeeting(meetingId);
            return meeting != null
                && meeting["CreatedBy"] != DBNull.Value
                && Convert.ToInt32(meeting["CreatedBy"]) == userId;
        }

        public static string SaveMinutes(int meetingId, int userId, string minutes)
        {
            if (!IsHost(meetingId, userId))
                return "Only the meeting host can write minutes.";

            using (SqlConnection con = new SqlConnection(AuthService.ConnectionString))
            using (SqlCommand cmd = new SqlCommand(
                "UPDATE Meetings SET MinutesOfMeeting = @Minutes WHERE MeetingID = @MeetingID", con))
            {
                cmd.Parameters.AddWithValue("@Minutes", (object)NullIfEmpty(minutes) ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@MeetingID", meetingId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
            return null;
        }

        public static string BuildLinkedLabel(DataRow row)
        {
            List<string> parts = new List<string>();
            AddPart(parts, row, "WorkTitle", "Work");
            AddPart(parts, row, "GroupName", "Group");
            AddPart(parts, row, "AssignmentName", "Assignment");
            AddPart(parts, row, "EventName", "Event");
            return parts.Count == 0 ? "General" : string.Join(" · ", parts);
        }

        private static void AddPart(List<string> parts, DataRow row, string column, string prefix)
        {
            if (!row.Table.Columns.Contains(column) || row[column] == DBNull.Value)
                return;
            string value = Convert.ToString(row[column]);
            if (!string.IsNullOrWhiteSpace(value))
                parts.Add(prefix + ": " + value);
        }

        private static string NullIfEmpty(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
