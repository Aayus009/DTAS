using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI.WebControls;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Events
{
    public partial class EventDetails : System.Web.UI.Page
    {
        private string connectionString;
        private int eventID;
        protected int CurrentLifecycleIndex;

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

            if (!int.TryParse(Request.QueryString["EventID"], out eventID))
            {
                Response.Redirect("~/Modules/Events/Events.aspx");
                return;
            }

            EventService.EnsureInviteCode(eventID);
            EventTaskService.SyncEventStatusFromTasks(eventID, Convert.ToInt32(Session["UserID"]));

            if (!IsPostBack)
            {
                LoadEventDetails();
                LoadEventTasks();
                LoadEventDecisions();
                LoadLinkedMeetings();
                LoadStatusTimeline();
            }
            else
            {
                EventLiveProgress live = EventTaskService.GetLiveEventProgress(eventID);
                CurrentLifecycleIndex = EventTaskService.LifecycleIndex(live == null ? "" : live.Status);
            }
        }

        private void LoadEventDetails()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    @"SELECT e.EventName, e.Description, e.EventType, e.StartDate, e.EndDate,
                             e.Venue, e.Budget, e.Organizer, e.Status, e.CreatedBy, e.CreatedAt,
                             ISNULL(u.FullName, CAST(e.CreatedBy AS nvarchar(50))) AS CreatedByName
                      FROM Events e
                      LEFT JOIN Users u ON u.UserID = e.CreatedBy
                      WHERE e.EventID = @EventID", con);
                cmd.Parameters.AddWithValue("@EventID", eventID);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string eventName = reader["EventName"].ToString();
                        EventLiveProgress live = EventTaskService.GetLiveEventProgress(eventID);
                        string status = live != null ? live.Status : reader["Status"].ToString();
                        string eventType = reader["EventType"].ToString();

                        litEventName.Text = $"<h1 class=\"font-headline-lg text-headline-lg text-primary\">{Server.HtmlEncode(eventName)}</h1>";
                        litStatusBadge.Text = GetStatusBadgeHtml(status);
                        litEventType.Text = GetEventTypeBadgeHtml(eventType);
                        litCreatedBy.Text = Server.HtmlEncode(reader["CreatedByName"].ToString());
                        litDescription.Text = Server.HtmlEncode(reader["Description"].ToString());
                        litStartDate.Text = FormatDate(reader["StartDate"], "MMM dd, yyyy");
                        litEndDate.Text = FormatDate(reader["EndDate"], "MMM dd, yyyy");
                        litVenue.Text = Server.HtmlEncode(reader["Venue"].ToString());
                        litBudget.Text = reader["Budget"] == DBNull.Value || reader["Budget"] == null
                            ? "Not set"
                            : Convert.ToDecimal(reader["Budget"]).ToString("C2");
                        litOrganizer.Text = Server.HtmlEncode(reader["Organizer"].ToString());
                        litCreatedDate.Text = FormatDate(reader["CreatedAt"], "MMM dd, yyyy - hh:mm tt");

                        ApplyRestrictionButtons();
                    }
                    else
                    {
                        Response.Redirect("~/Modules/Events/Events.aspx");
                    }
                }
            }
        }

        private void LoadEventTasks()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    @"SELECT TaskID, TaskTitle, Status, DueDate
                      FROM Tasks
                      WHERE EventID = @EventID
                        AND ISNULL(IsDeleted, 0) = 0
                        AND REPLACE(LTRIM(RTRIM(ISNULL(Status, N''))), N' ', N'') NOT IN (N'Archived', N'Cancelled')
                      ORDER BY DueDate", con);
                cmd.Parameters.AddWithValue("@EventID", eventID);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dt.Columns.Add("StatusBadge", typeof(string));
                dt.Columns.Add("DueDateFormatted", typeof(string));
                dt.Columns.Add("MemberNames", typeof(string));
                dt.Columns.Add("MemberRole", typeof(string));

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        int taskId = Convert.ToInt32(row["TaskID"]);
                        string rawStatus = row["Status"] == DBNull.Value ? "" : row["Status"].ToString();
                        string taskStatus = TimelineService.IsOverdue(row["DueDate"], rawStatus)
                            ? "Delayed"
                            : rawStatus;
                        row["StatusBadge"] = GetStatusBadgeHtml(taskStatus);
                        row["DueDateFormatted"] = TimelineService.DueLabel(row["DueDate"], rawStatus);

                        string names = EventTaskService.FormatAssigneeNames(taskId);
                        bool unassigned = string.IsNullOrWhiteSpace(names)
                            || names.Equals("Unassigned", StringComparison.OrdinalIgnoreCase);
                        row["MemberNames"] = Server.HtmlEncode(unassigned ? "Unassigned" : names);
                        row["MemberRole"] = unassigned
                            ? "No member assigned"
                            : (NormalizeTaskDone(rawStatus) ? "Completed this task" : "Assigned to this task");
                    }

                    rptTasks.DataSource = dt;
                    rptTasks.DataBind();
                    pnlNoTasks.Visible = false;
                    litTaskCount.Text = dt.Rows.Count.ToString();
                }
                else
                {
                    rptTasks.Visible = false;
                    pnlNoTasks.Visible = true;
                    litTaskCount.Text = "0";
                }
            }

            BindCompletion(EventTaskService.GetLiveEventProgress(eventID));
        }

        private static bool NormalizeTaskDone(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return false;
            string key = status.Replace(" ", "").Trim();
            return key.Equals("Completed", StringComparison.OrdinalIgnoreCase)
                || key.Equals("Done", StringComparison.OrdinalIgnoreCase);
        }

        private void BindCompletion(EventLiveProgress live)
        {
            int pct = live == null ? 0 : live.Percent;
            int total = live == null ? 0 : live.TotalTasks;
            int done = live == null ? 0 : live.CompletedTasks;
            litCompletionPercent.Text = pct + "%";
            litCompletionMeta.Text = EventTaskService.CompletionLabel(done, total);
            completionBarFill.Style["width"] = pct + "%";
            completionBarFill.Style["background-color"] = pct > 0 ? "#2e7d32" : "#c5d0c8";
        }

        private void LoadEventDecisions()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    @"SELECT DecisionID, DecisionTitle, Status
                      FROM Decisions WHERE EventID = @EventID
                      ORDER BY DecisionID DESC", con);
                cmd.Parameters.AddWithValue("@EventID", eventID);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dt.Columns.Add("StatusBadge", typeof(string));

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        row["StatusBadge"] = GetStatusBadgeHtml(row["Status"].ToString());
                    }

                    rptDecisions.DataSource = dt;
                    rptDecisions.DataBind();
                    pnlNoDecisions.Visible = false;
                }
                else
                {
                    rptDecisions.Visible = false;
                    pnlNoDecisions.Visible = true;
                }
            }
        }

        private void LoadLinkedMeetings()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    @"SELECT MeetingTitle, ScheduledDate
                      FROM Meetings WHERE EventID = @EventID
                      ORDER BY ScheduledDate DESC", con);
                cmd.Parameters.AddWithValue("@EventID", eventID);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    rptLinkedMeetings.DataSource = dt;
                    rptLinkedMeetings.DataBind();
                    pnlNoMeetings.Visible = false;
                }
                else
                {
                    rptLinkedMeetings.Visible = false;
                    pnlNoMeetings.Visible = true;
                }
            }
        }

        private void LoadStatusTimeline()
        {
            DataTable timeline = new DataTable();
            timeline.Columns.Add("Label", typeof(string));
            timeline.Columns.Add("Icon", typeof(string));
            timeline.Columns.Add("Date", typeof(string));
            timeline.Columns.Add("DotClass", typeof(string));
            timeline.Columns.Add("IconColor", typeof(string));

            string[] allStatuses = { "Created", "Planned", "In Progress", "Completed", "Archived" };
            EventLiveProgress live = EventTaskService.GetLiveEventProgress(eventID);
            CurrentLifecycleIndex = EventTaskService.LifecycleIndex(live == null ? "" : live.Status);

            for (int i = 0; i < allStatuses.Length; i++)
            {
                string label = allStatuses[i];
                bool isCompleted = i < CurrentLifecycleIndex;
                bool isCurrent = i == CurrentLifecycleIndex;

                string dotStyle = isCompleted || isCurrent
                    ? "background-color: #0077b6;"
                    : "background-color: #e0e0e0;";
                string iconColor = isCompleted || isCurrent ? "color: white;" : "color: #999;";
                string icon = isCompleted ? "check" : isCurrent ? "radio_button_checked" : "circle";

                timeline.Rows.Add(label, icon, "", dotStyle, iconColor);
            }

            rptTimeline.DataSource = timeline;
            rptTimeline.DataBind();

            int progressPercent = (CurrentLifecycleIndex * 100) / (allStatuses.Length - 1);
            timelineFill.Style["width"] = progressPercent + "%";
        }

        private static string FormatDate(object value, string format)
        {
            if (value == null || value == DBNull.Value)
                return "Not set";
            return Convert.ToDateTime(value).ToString(format);
        }

        private void ApplyRestrictionButtons()
        {
            bool restricted = RestrictionService.IsEventRestricted(eventID);
            btnRestrict.Visible = !restricted;
            btnRestore.Visible = restricted;
            if (restricted)
                litStatusBadge.Text += " <span class=\"inline-flex items-center gap-1 px-2.5 py-1 rounded-full font-badge-cap text-badge-cap bg-[rgba(198,40,40,0.12)] text-[#c62828]\">Restricted</span>";
        }

        private string GetStatusBadgeHtml(string status)
        {
            string bgColor, textColor, icon;
            string key = (status ?? "").Replace(" ", "").ToLowerInvariant();
            string display = EventTaskService.EventStatusDisplay(status);

            switch (key)
            {
                case "created":
                case "proposed":
                    bgColor = "background-color: rgba(230,126,0,0.12);";
                    textColor = "color: #e67e00;";
                    icon = "hourglass_top";
                    break;
                case "completed":
                    bgColor = "background-color: rgba(39,165,119,0.15);";
                    textColor = "color: #27a577;";
                    icon = "check_circle";
                    break;
                case "in progress":
                case "inprogress":
                    bgColor = "background-color: rgba(0,35,111,0.1);";
                    textColor = "color: #00236f;";
                    icon = "pending";
                    break;
                case "planned":
                    bgColor = "background-color: rgba(86,101,124,0.1);";
                    textColor = "color: #56657c;";
                    icon = "event_note";
                    break;
                case "archived":
                    bgColor = "background-color: rgba(150,150,150,0.1);";
                    textColor = "color: #999;";
                    icon = "archive";
                    break;
                case "cancelled":
                    bgColor = "background-color: rgba(220,53,69,0.1);";
                    textColor = "color: #dc3545;";
                    icon = "cancel";
                    break;
                case "delayed":
                    bgColor = "background-color: rgba(198,40,40,0.15);";
                    textColor = "color: #c62828;";
                    icon = "warning";
                    status = "Late";
                    display = "Late";
                    break;
                default:
                    bgColor = "background-color: rgba(0,35,111,0.1);";
                    textColor = "color: #00236f;";
                    icon = "circle";
                    break;
            }

            return $"<span class=\"inline-flex items-center gap-1 px-2.5 py-1 rounded-full font-badge-cap text-badge-cap\" style=\"{bgColor}{textColor}\">" +
                   $"<span class=\"material-symbols-outlined text-[14px]\">{icon}</span>{Server.HtmlEncode(display)}</span>";
        }

        private string GetEventTypeBadgeHtml(string eventType)
        {
            return $"<span class=\"inline-flex items-center gap-1 px-2.5 py-1 rounded-full font-badge-cap text-badge-cap bg-primary-container text-on-primary-container\">" +
                   $"<span class=\"material-symbols-outlined text-[14px]\">category</span>{Server.HtmlEncode(eventType)}</span>";
        }

        protected void btnRestrict_Click(object sender, EventArgs e)
        {
            RestrictionService.SetEventRestricted(eventID, Convert.ToInt32(Session["UserID"]), Session["Role"] as string, true);
            Response.Redirect(Request.RawUrl);
        }

        protected void btnRestore_Click(object sender, EventArgs e)
        {
            RestrictionService.SetEventRestricted(eventID, Convert.ToInt32(Session["UserID"]), Session["Role"] as string, false);
            Response.Redirect(Request.RawUrl);
        }

        protected void btnBackToList_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Modules/Events/Events.aspx");
        }

    }
}
