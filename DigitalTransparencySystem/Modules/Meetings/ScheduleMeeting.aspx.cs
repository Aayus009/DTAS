using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Meetings
{
    public partial class ScheduleMeeting : Page
    {
        private string connectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            connectionString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;

            if (!IsPostBack)
            {
                if (Session["UserID"] == null)
                {
                    Response.Redirect("~/Modules/Authentication/Login.aspx");
                    return;
                }

                Response.Redirect("~/Modules/Dashboard/AdminDashboard.aspx");
                return;
            }
        }

        private void LoadParticipants()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("SELECT UserID, FullName FROM Users ORDER BY FullName", con);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                lstParticipants.DataSource = dt;
                lstParticipants.DataTextField = "FullName";
                lstParticipants.DataValueField = "UserID";
                lstParticipants.DataBind();
            }
        }

        protected void btnSchedule_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            int createdBy = Convert.ToInt32(Session["UserID"]);
            DateTime scheduledDate = Convert.ToDateTime(txtDateTime.Text);
            int duration = string.IsNullOrEmpty(txtDuration.Text) ? 60 : Convert.ToInt32(txtDuration.Text);

            ZoomMeetingResult zoom = null;
            var zoomService = new ZoomService();
            bool wantsZoom = chkZoom.Checked;

            if (wantsZoom)
            {
                if (!zoomService.IsConfigured)
                {
                    ShowError("Zoom is not configured. Add Zoom_AccountId, Zoom_ClientId and Zoom_ClientSecret to Web.config before creating a Zoom meeting.");
                    return;
                }

                string tz = ConfigurationManager.AppSettings["Zoom_Timezone"];
                if (string.IsNullOrWhiteSpace(tz)) tz = "UTC";

                zoom = zoomService.CreateMeeting(txtTitle.Text.Trim(), scheduledDate, duration, txtAgenda.Text.Trim(), tz);

                if (zoom == null || !string.IsNullOrWhiteSpace(zoom.Error) || string.IsNullOrWhiteSpace(zoom.JoinUrl))
                {
                    ShowError(zoom != null && !string.IsNullOrWhiteSpace(zoom.Error)
                        ? zoom.Error
                        : "Failed to create the Zoom meeting. Check your Zoom credentials and try again.");
                    return;
                }
            }

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                SqlCommand cmd = new SqlCommand(@"INSERT INTO Meetings
                    (MeetingTitle, Description, MeetingType, ScheduledDate, Duration, Venue, Agenda, Status, CreatedBy,
                     ZoomMeetingId, ZoomJoinUrl, ZoomStartUrl, ZoomPasscode, ZoomHostId)
                    VALUES (@Title, @Description, @MeetingType, @ScheduledDate, @Duration, @Venue, @Agenda, 'Scheduled', @CreatedBy,
                     @ZoomMeetingId, @ZoomJoinUrl, @ZoomStartUrl, @ZoomPasscode, @ZoomHostId);
                    SELECT SCOPE_IDENTITY();", con);

                cmd.Parameters.AddWithValue("@Title", txtTitle.Text.Trim());
                cmd.Parameters.AddWithValue("@Description", (object)txtDescription.Text.Trim() ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@MeetingType", ddlMeetingType.SelectedValue);
                cmd.Parameters.AddWithValue("@ScheduledDate", scheduledDate);
                cmd.Parameters.AddWithValue("@Duration", duration);
                cmd.Parameters.AddWithValue("@Venue", txtVenue.Text.Trim());
                cmd.Parameters.AddWithValue("@Agenda", (object)txtAgenda.Text.Trim() ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CreatedBy", createdBy);

                if (zoom != null)
                {
                    cmd.Parameters.AddWithValue("@ZoomMeetingId", (object)zoom.MeetingId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ZoomJoinUrl", (object)zoom.JoinUrl ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ZoomStartUrl", (object)zoom.StartUrl ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ZoomPasscode", (object)zoom.Passcode ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ZoomHostId", (object)zoom.HostId ?? DBNull.Value);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@ZoomMeetingId", DBNull.Value);
                    cmd.Parameters.AddWithValue("@ZoomJoinUrl", DBNull.Value);
                    cmd.Parameters.AddWithValue("@ZoomStartUrl", DBNull.Value);
                    cmd.Parameters.AddWithValue("@ZoomPasscode", DBNull.Value);
                    cmd.Parameters.AddWithValue("@ZoomHostId", DBNull.Value);
                }

                int meetingId = Convert.ToInt32(cmd.ExecuteScalar());

                foreach (var item in lstParticipants.GetSelectedIndices())
                {
                    int userId = Convert.ToInt32(lstParticipants.Items[item].Value);
                    SqlCommand cmdParticipant = new SqlCommand(@"INSERT INTO MeetingParticipants
                        (MeetingID, UserID, Role, AttendanceStatus)
                        VALUES (@MeetingID, @UserID, 'Participant', 'Pending')", con);
                    cmdParticipant.Parameters.AddWithValue("@MeetingID", meetingId);
                    cmdParticipant.Parameters.AddWithValue("@UserID", userId);
                    cmdParticipant.ExecuteNonQuery();
                }
            }

            Response.Redirect("~/Modules/Dashboard/AdminDashboard.aspx");
        }

        private void ShowError(string message)
        {
            pnlError.Visible = true;
            lblError.Text = message;
        }

    }
}
