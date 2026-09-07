using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace DigitalTransparencySystem.Modules.Meetings
{
    public partial class Meetings : Page
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

        private void LoadMeetings()
        {
            string search = txtSearch.Text.Trim();
            string status = ddlStatusFilter.SelectedValue;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"SELECT MeetingID, MeetingTitle, MeetingType, ScheduledDate, Venue, Status,
                                         ZoomJoinUrl,
                                         (SELECT COUNT(*) FROM MeetingRecordings mr WHERE mr.MeetingID = m.MeetingID) AS RecordingCount
                                 FROM Meetings m WHERE 1=1";

                if (!string.IsNullOrEmpty(search))
                    query += " AND MeetingTitle LIKE @Search";

                if (status != "All")
                    query += " AND Status = @Status";

                query += " ORDER BY ScheduledDate DESC";

                SqlCommand cmd = new SqlCommand(query, con);

                if (!string.IsNullOrEmpty(search))
                    cmd.Parameters.AddWithValue("@Search", "%" + search + "%");

                if (status != "All")
                    cmd.Parameters.AddWithValue("@Status", status);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                rptMeetings.DataSource = dt;
                rptMeetings.DataBind();
            }
        }

        protected void txtSearch_TextChanged(object sender, EventArgs e)
        {
            LoadMeetings();
        }

        protected void ddlStatusFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadMeetings();
        }

        protected void rptMeetings_ItemCommand(object source, System.Web.UI.WebControls.RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Archive")
            {
                ArchiveMeeting(Convert.ToInt32(e.CommandArgument));
                LoadMeetings();
            }
        }

        private void ArchiveMeeting(int meetingId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("DELETE FROM Meetings WHERE MeetingID = @MeetingID", con);
                cmd.Parameters.AddWithValue("@MeetingID", meetingId);
                cmd.ExecuteNonQuery();
            }
        }

        protected string GetZoomIndicator(object zoomJoinUrl, object recordingCount)
        {
            bool hasZoom = zoomJoinUrl != null && !string.IsNullOrEmpty(zoomJoinUrl.ToString());
            int recCount = recordingCount != null && recordingCount != DBNull.Value ? Convert.ToInt32(recordingCount) : 0;
            var badges = new System.Collections.Generic.List<string>();

            if (hasZoom)
            {
                badges.Add("<span class=\"inline-flex items-center gap-1 px-2 py-0.5 rounded-full bg-[#eaf3ff] text-[#2d8cff] font-badge-cap text-[10px] font-bold\"><span class=\"material-symbols-outlined text-[12px]\">videocam</span>Zoom</span>");
            }
            if (recCount > 0)
            {
                badges.Add("<span class=\"inline-flex items-center gap-1 px-2 py-0.5 rounded-full bg-[#fdf0e6] text-[#c2560c] font-badge-cap text-[10px] font-bold\"><span class=\"material-symbols-outlined text-[12px]\">movie</span>" + recCount + " recording" + (recCount != 1 ? "s" : "") + "</span>");
            }

            return badges.Count > 0 ? string.Join(" ", badges) : "";
        }

        protected string GetStatusBadgeClass(string status)
        {
            switch (status)
            {
                case "Scheduled":
                    return "inline-block px-3 py-1 rounded-full bg-primary-container/20 text-primary font-label-md text-badge-cap font-bold";
                case "InProgress":
                    return "inline-block px-3 py-1 rounded-full bg-amber-100 text-amber-700 font-label-md text-badge-cap font-bold";
                case "Completed":
                    return "inline-block px-3 py-1 rounded-full bg-green-100 text-green-700 font-label-md text-badge-cap font-bold";
                case "Cancelled":
                    return "inline-block px-3 py-1 rounded-full bg-error-container/40 text-error font-label-md text-badge-cap font-bold";
                default:
                    return "inline-block px-3 py-1 rounded-full bg-surface-container-high text-on-surface-variant font-label-md text-badge-cap font-bold";
            }
        }

    }
}
