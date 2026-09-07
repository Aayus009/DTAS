using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;

namespace DigitalTransparencySystem
{
    public partial class Events : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadEvents();
                LoadStats();
            }
        }

        private void LoadEvents()
        {
            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection con = new SqlConnection(connStr))
                {
                    SqlCommand cmd = new SqlCommand(
                        @"SELECT e.EventID, e.EventName, e.EventType, e.StartDate, e.EndDate, e.Venue, e.Status,
                            ISNULL((SELECT COUNT(*) FROM Tasks t WHERE t.EventID = e.EventID AND t.Status = 'Completed'), 0) AS DoneTasks,
                            ISNULL((SELECT COUNT(*) FROM Tasks t WHERE t.EventID = e.EventID), 0) AS TotalTasks
                          FROM Events e
                          WHERE e.Status IN ('Planned', 'Ongoing', 'Completed')
                          ORDER BY e.StartDate DESC", con);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dt.Columns.Add("Day", typeof(string));
                    dt.Columns.Add("Month", typeof(string));
                    dt.Columns.Add("TimeRange", typeof(string));
                    dt.Columns.Add("Category", typeof(string));
                    dt.Columns.Add("LiveBadge", typeof(string));
                    dt.Columns.Add("ProgressHtml", typeof(string));

                    foreach (DataRow row in dt.Rows)
                    {
                        DateTime start = Convert.ToDateTime(row["StartDate"]);
                        DateTime end = row["EndDate"] == DBNull.Value ? start : Convert.ToDateTime(row["EndDate"]);

                        row["Day"] = start.Day.ToString();
                        row["Month"] = start.ToString("MMM", CultureInfo.InvariantCulture);

                        string timeRange = start.ToString("hh:mm tt");
                        if (end.Date == start.Date)
                            timeRange += " - " + end.ToString("hh:mm tt");
                        else
                            timeRange += " - " + end.ToString("MMM dd, hh:mm tt");
                        row["TimeRange"] = timeRange;

                        string status = row["Status"].ToString().ToLowerInvariant();
                        string typeKey = row["EventType"].ToString().ToLowerInvariant();
                        string phase = (start > DateTime.Now) ? "upcoming" : "past";
                        row["Category"] = (typeKey + " " + status + " " + phase).Trim();

                        row["LiveBadge"] = row["Status"].ToString().Equals("Ongoing", StringComparison.OrdinalIgnoreCase)
                            ? @"<div class=""flex items-center gap-1.5 text-on-tertiary-container font-bold text-[12px]""><span class=""w-2 h-2 bg-on-tertiary-container rounded-full status-pulse""></span>LIVE</div>"
                            : "";

                        int done = row["DoneTasks"] == DBNull.Value ? 0 : Convert.ToInt32(row["DoneTasks"]);
                        int total = row["TotalTasks"] == DBNull.Value ? 0 : Convert.ToInt32(row["TotalTasks"]);
                        int progress = total > 0 ? (int)Math.Round((done * 100.0) / total) : 0;

                        row["ProgressHtml"] = total > 0
                            ? @"<div class=""mt-3 max-w-xs"">
                                    <div class=""flex justify-between text-[12px] font-medium text-secondary mb-1"">
                                        <span>Progress</span><span>" + progress + @"%</span>
                                    </div>
                                    <div class=""w-full bg-surface-container-high h-2 rounded-full overflow-hidden"">
                                        <div class=""progress-bar bg-primary h-full rounded-full"" style=""width:" + progress + @"%""></div>
                                    </div>
                                </div>"
                            : "";
                    }

                    rptEvents.DataSource = dt;
                    rptEvents.DataBind();
                }
            }
            catch
            {
                rptEvents.DataSource = null;
                rptEvents.DataBind();
            }
        }

        private void LoadStats()
        {
            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection con = new SqlConnection(connStr))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand(
                        @"SELECT
                            (SELECT COUNT(*) FROM Events WHERE Status IN ('Planned', 'Scheduled') AND StartDate >= GETDATE()) AS Upcoming,
                            (SELECT COUNT(*) FROM Events WHERE EndDate < GETDATE()) AS PastEvents,
                            (SELECT COUNT(*) FROM Users WHERE IsActive = 1) AS Participants,
                            (SELECT COUNT(*) FROM Meetings) AS Sessions", con);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            litUpcoming.Text = (reader["Upcoming"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Upcoming"])).ToString("N0");
                            litPast.Text = (reader["PastEvents"] == DBNull.Value ? 0 : Convert.ToInt32(reader["PastEvents"])).ToString("N0");
                            litParticipants.Text = (reader["Participants"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Participants"])).ToString("N0");
                            litSessions.Text = (reader["Sessions"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Sessions"])).ToString("N0");
                        }
                    }
                }
            }
            catch
            {
                litUpcoming.Text = "0";
                litPast.Text = "0";
                litParticipants.Text = "0";
                litSessions.Text = "0";
            }
        }
    }
}