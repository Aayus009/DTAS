using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace DigitalTransparencySystem
{
    public partial class EventDetails : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadEvent();
            }
        }

        private void LoadEvent()
        {
            int eventID;
            if (!int.TryParse(Request.QueryString["id"], out eventID))
            {
                pnlEvent.Visible = false;
                pnlNotFound.Visible = true;
                return;
            }

            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection con = new SqlConnection(connStr))
                {
                    con.Open();

                    SqlCommand cmd = new SqlCommand(
                        @"SELECT e.EventID, e.EventName, e.EventType, e.Description, e.StartDate, e.EndDate,
                                 e.Venue, e.Status, e.Budget, e.Organizer
                          FROM Events e
                          WHERE e.EventID = @EventID", con);
                    cmd.Parameters.AddWithValue("@EventID", eventID);

                    SqlDataReader reader = cmd.ExecuteReader();
                    if (!reader.Read())
                    {
                        reader.Close();
                        pnlEvent.Visible = false;
                        pnlNotFound.Visible = true;
                        return;
                    }

                    litEventName.Text = reader["EventName"].ToString();
                    litEventType.Text = reader["EventType"].ToString();
                    litVenue.Text = reader["Venue"] == DBNull.Value ? "TBA" : reader["Venue"].ToString();
                    litOrganizer.Text = reader["Organizer"] == DBNull.Value ? "Institutional Committee" : reader["Organizer"].ToString();

                    string status = reader["Status"].ToString();
                    litStatus.Text = status;

                    litStatusBadge.Text = status.Equals("Ongoing", StringComparison.OrdinalIgnoreCase)
                        ? @"<span class=""flex items-center gap-1.5 bg-tertiary-container text-on-tertiary-container px-3 py-1 rounded-lg text-[10px] font-bold uppercase tracking-wider""><span class=""w-2 h-2 bg-on-tertiary-container rounded-full status-pulse""></span>LIVE</span>"
                        : @"<span class=""bg-surface-variant text-on-surface-variant px-3 py-1 rounded-lg text-[10px] font-bold uppercase tracking-wider"">" + status + "</span>";

                    DateTime start = Convert.ToDateTime(reader["StartDate"]);
                    DateTime end = reader["EndDate"] == DBNull.Value ? start : Convert.ToDateTime(reader["EndDate"]);

                    litDay.Text = start.Day.ToString();
                    litMonth.Text = start.ToString("MMM", CultureInfo.InvariantCulture);

                    string dateRange = start.ToString("MMM dd, yyyy hh:mm tt");
                    if (end.Date != start.Date)
                        dateRange += " - " + end.ToString("MMM dd, yyyy hh:mm tt");
                    else if (end.TimeOfDay != TimeSpan.Zero)
                        dateRange += " - " + end.ToString("hh:mm tt");
                    litDateRange.Text = dateRange;

                    litDescription.Text = (reader["Description"] == DBNull.Value || string.IsNullOrWhiteSpace(reader["Description"].ToString()))
                        ? "Detailed information about this public session is being prepared and will be published here shortly."
                        : reader["Description"].ToString();

                    if (reader["Budget"] == DBNull.Value || Convert.ToDecimal(reader["Budget"]) == 0)
                        litBudget.Text = "Not disclosed";
                    else
                        litBudget.Text = string.Format(CultureInfo.InvariantCulture, "{0:C0}", Convert.ToDecimal(reader["Budget"]));

                    reader.Close();
                    LoadRelatedMeetings(con, eventID);
                }
            }
            catch
            {
                pnlEvent.Visible = false;
                pnlNotFound.Visible = true;
            }
        }

        private void LoadRelatedMeetings(SqlConnection con, int eventID)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("MeetingTitle", typeof(string));
            dt.Columns.Add("MeetingType", typeof(string));
            dt.Columns.Add("ScheduledDate", typeof(DateTime));
            dt.Columns.Add("MinutesStatus", typeof(string));

            SqlCommand cmd = new SqlCommand(
                @"SELECT MeetingTitle, MeetingType, ScheduledDate, MinutesOfMeeting
                  FROM Meetings
                  WHERE EventID = @EventID
                  ORDER BY ScheduledDate DESC", con);
            cmd.Parameters.AddWithValue("@EventID", eventID);

            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataTable raw = new DataTable();
            adapter.Fill(raw);

            foreach (DataRow row in raw.Rows)
            {
                DataRow newRow = dt.NewRow();
                newRow["MeetingTitle"] = row["MeetingTitle"].ToString();
                newRow["MeetingType"] = row["MeetingType"].ToString();
                newRow["ScheduledDate"] = Convert.ToDateTime(row["ScheduledDate"]);

                bool hasMinutes = row["MinutesOfMeeting"] != DBNull.Value
                    && !string.IsNullOrWhiteSpace(row["MinutesOfMeeting"].ToString());
                newRow["MinutesStatus"] = hasMinutes
                    ? @"<span class=""inline-block mt-2 text-[12px] font-bold text-tertiary flex items-center gap-1""><span class=""material-symbols-outlined text-[14px]"">check_circle</span>Minutes published</span>"
                    : @"<span class=""inline-block mt-2 text-[12px] font-bold text-on-surface-variant flex items-center gap-1""><span class=""material-symbols-outlined text-[14px]"">pending</span>Minutes pending</span>";
                dt.Rows.Add(newRow);
            }

            rptRelatedMeetings.DataSource = dt;
            rptRelatedMeetings.DataBind();
        }
    }
}