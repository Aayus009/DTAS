using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem
{
    public partial class Transparency : System.Web.UI.Page
    {
        private string connectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Default.aspx?signin=1&next=/Transparency.aspx");
                return;
            }

            ApplyChrome();
            connectionString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;

            if (!IsPostBack)
            {
                LoadHeroStats();
                LoadWorks();
                LoadActiveDecisions();
                LoadDecisions();
                LoadOpenPolls();
                LoadTrackerData();
            }
        }

        private void ApplyChrome()
        {
            bool admin = RoleAccess.IsAdmin(Session["Role"] as string);
            adminTop.Visible = admin;
            adminSide.Visible = admin;
            userTop.Visible = !admin;
            userSide.Visible = !admin;
            pnlAdminPreview.Visible = admin;
        }

        private void LoadHeroStats()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand(
                        @"SELECT
                            (SELECT COUNT(*) FROM Decisions WHERE Status NOT IN ('Rejected', 'Cancelled')) AS TotalDecisions,
                            (SELECT COUNT(*) FROM Decisions WHERE Status IN ('Implemented', 'Completed', 'Closed', 'Approved')) AS Implemented,
                            (SELECT COUNT(*) FROM Users WHERE IsActive = 1) AS Members,
                            (SELECT COUNT(*) FROM Tasks WHERE Status NOT IN ('Cancelled', 'Archived')) AS TotalTasks,
                            (SELECT COUNT(*) FROM Tasks WHERE Status = 'Completed') AS CompletedTasks", con);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            litTotalDecisions.Text = Convert.ToInt32(reader["TotalDecisions"]).ToString("N0");
                            litImplemented.Text = Convert.ToInt32(reader["Implemented"]).ToString("N0");
                            litMembers.Text = Convert.ToInt32(reader["Members"]).ToString("N0");

                            int totalTasks = reader["TotalTasks"] == DBNull.Value ? 0 : Convert.ToInt32(reader["TotalTasks"]);
                            int completedTasks = reader["CompletedTasks"] == DBNull.Value ? 0 : Convert.ToInt32(reader["CompletedTasks"]);
                            int accountability = totalTasks > 0 ? (int)Math.Round((completedTasks * 100.0) / totalTasks) : 0;
                            litAccountability.Text = accountability.ToString();
                        }
                    }
                }
            }
            catch
            {
                litTotalDecisions.Text = "0";
                litImplemented.Text = "0";
                litAccountability.Text = "0";
                litMembers.Text = "0";
            }
        }

        private void LoadWorks()
        {
            try
            {
                DataTable dt = new DataTable();
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand(
                        @"SELECT TOP 10
                            e.EventID AS ItemID,
                            e.EventName AS ItemTitle,
                            e.Priority,
                            e.Status,
                            e.EndDate AS DueDate,
                            'Event' AS ItemType,
                            LEFT(ISNULL(NULLIF(LTRIM(RTRIM(e.Objective)), ''), ISNULL(e.Description, 'Community event in progress.')), 180) AS ReasonGist,
                            ISNULL(e.CompletionPercent, 0) AS ProgressPct,
                            ISNULL(NULLIF(LTRIM(RTRIM(e.Organizer)), ''), ISNULL(u.FullName, 'Institutional lead')) AS HandledBy,
                            NULL AS RelatedDecision,
                            NULL AS RelatedEvent
                          FROM Events e
                          LEFT JOIN Users u ON e.ProposedBy = u.UserID
                          WHERE e.Status IN ('Proposed', 'Planned', 'Ongoing', 'InProgress', 'Approved', 'Completed')
                          ORDER BY ISNULL(e.StartDate, e.CreatedAt) DESC", con);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand(
                        @"SELECT TOP 8
                            t.TaskID AS ItemID,
                            t.TaskTitle AS ItemTitle,
                            t.Priority,
                            t.Status,
                            t.DueDate,
                            'Task' AS ItemType,
                            LEFT(ISNULL(NULLIF(LTRIM(RTRIM(t.PublicSummary)), ''), 'Work is underway. Detailed notes stay inside the team workspace.'), 180) AS ReasonGist,
                            ISNULL(tt.ProgressPercent, CASE WHEN t.Status = 'Completed' THEN 100 ELSE 0 END) AS ProgressPct,
                            ISNULL(leader.FullName, 'Lead to be assigned') AS HandledBy,
                            d.DecisionTitle AS RelatedDecision,
                            e.EventName AS RelatedEvent
                          FROM Tasks t
                          LEFT JOIN TaskTeams tt ON tt.TaskID = t.TaskID
                          LEFT JOIN Users leader ON ISNULL(tt.LeaderID, t.LeaderID) = leader.UserID
                          LEFT JOIN Decisions d ON t.DecisionID = d.DecisionID
                          LEFT JOIN Events e ON t.EventID = e.EventID
                          WHERE t.Status NOT IN ('Cancelled', 'Archived')
                          ORDER BY t.DueDate", con);

                    DataTable dtTasks = new DataTable();
                    new SqlDataAdapter(cmd).Fill(dtTasks);
                    foreach (DataRow row in dtTasks.Rows)
                        dt.ImportRow(row);
                }

                dt.Columns.Add("TypeIcon", typeof(string));
                dt.Columns.Add("StatusBadge", typeof(string));
                dt.Columns.Add("TypeBadge", typeof(string));
                dt.Columns.Add("ProgressHtml", typeof(string));

                foreach (DataRow row in dt.Rows)
                {
                    string itemType = row["ItemType"].ToString();
                    string status = row["Status"].ToString();
                    row["ItemTitle"] = HttpUtility.HtmlEncode(row["ItemTitle"].ToString());
                    row["ReasonGist"] = HttpUtility.HtmlEncode(row["ReasonGist"].ToString());
                    row["HandledBy"] = HttpUtility.HtmlEncode(row["HandledBy"].ToString());
                    if (row["RelatedDecision"] != DBNull.Value)
                        row["RelatedDecision"] = HttpUtility.HtmlEncode(row["RelatedDecision"].ToString());
                    if (row["RelatedEvent"] != DBNull.Value)
                        row["RelatedEvent"] = HttpUtility.HtmlEncode(row["RelatedEvent"].ToString());

                    row["TypeIcon"] = itemType.Equals("Event", StringComparison.OrdinalIgnoreCase) ? "event" : "task_alt";
                    row["StatusBadge"] = BuildWorksBadge(status);
                    row["TypeBadge"] = itemType.Equals("Event", StringComparison.OrdinalIgnoreCase)
                        ? "<span class=\"tx-chip tx-chip-event\">Event</span>"
                        : "<span class=\"tx-chip\">Work</span>";

                    int progress = row["ProgressPct"] == DBNull.Value ? 0 : Convert.ToInt32(row["ProgressPct"]);
                    row["ProgressHtml"] = BuildProgress(progress);
                }

                rptWorks.DataSource = dt;
                rptWorks.DataBind();
                pnlWorksEmpty.Visible = dt.Rows.Count == 0;
            }
            catch
            {
                rptWorks.DataSource = null;
                rptWorks.DataBind();
                pnlWorksEmpty.Visible = true;
            }
        }

        private void LoadActiveDecisions()
        {
            try
            {
                DataTable dt = new DataTable();
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand(
                        @"SELECT TOP 8
                            d.DecisionID, d.DecisionTitle, d.Priority, d.Status, d.DueDate,
                            ISNULL(d.ProgressPercent, 0) AS ProgressPct,
                            LEFT(ISNULL(NULLIF(LTRIM(RTRIM(d.Description)), ''), 'Decision recorded for this event or project.'), 180) AS ReasonGist,
                            ISNULL(u.FullName, 'Committee') AS ResponsibleName,
                            ISNULL(e.EventName, ISNULL(m.MeetingTitle, 'Institutional Committee')) AS ContextName
                          FROM Decisions d
                          LEFT JOIN Users u ON d.ResponsibleUserID = u.UserID
                          LEFT JOIN Events e ON d.EventID = e.EventID
                          LEFT JOIN Meetings m ON d.MeetingID = m.MeetingID
                          WHERE d.Status IN ('Proposed', 'Created', 'UnderReview', 'Under Review', 'Reviewed', 'Approved', 'Implemented')
                          ORDER BY ISNULL(d.DueDate, d.CreatedAt)", con);

                    new SqlDataAdapter(cmd).Fill(dt);
                }

                dt.Columns.Add("StatusBadge", typeof(string));
                dt.Columns.Add("ResponsibleInitials", typeof(string));
                dt.Columns.Add("ProgressHtml", typeof(string));

                foreach (DataRow row in dt.Rows)
                {
                    row["DecisionTitle"] = HttpUtility.HtmlEncode(row["DecisionTitle"].ToString());
                    row["ReasonGist"] = HttpUtility.HtmlEncode(row["ReasonGist"].ToString());
                    row["ResponsibleName"] = HttpUtility.HtmlEncode(row["ResponsibleName"].ToString());
                    row["ContextName"] = HttpUtility.HtmlEncode(row["ContextName"].ToString());
                    row["StatusBadge"] = BuildStatusBadge(row["Status"].ToString());
                    row["ResponsibleInitials"] = Initials(row["ResponsibleName"].ToString());
                    row["ProgressHtml"] = BuildProgress(Convert.ToInt32(row["ProgressPct"]));
                }

                rptActiveDecisions.DataSource = dt;
                rptActiveDecisions.DataBind();
                pnlActiveDecisionsEmpty.Visible = dt.Rows.Count == 0;
            }
            catch
            {
                rptActiveDecisions.DataSource = null;
                rptActiveDecisions.DataBind();
                pnlActiveDecisionsEmpty.Visible = true;
            }
        }

        private void LoadDecisions()
        {
            try
            {
                DataTable dt = new DataTable();
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand(
                        @"SELECT d.DecisionID, d.DecisionTitle, d.Priority, d.Status,
                            LEFT(ISNULL(NULLIF(LTRIM(RTRIM(d.Description)), ''), 'Published institutional decision.'), 180) AS ReasonGist,
                            ISNULL(d.ProgressPercent, 0) AS ProgressPct,
                            ISNULL(u.FullName, 'Committee') AS ResponsibleName,
                            ISNULL(e.EventName, ISNULL(m.MeetingTitle, 'Institutional Committee')) AS ContextName,
                            ISNULL((SELECT COUNT(*) FROM Tasks t WHERE t.DecisionID = d.DecisionID AND t.Status = 'Completed'), 0) AS DoneTasks,
                            ISNULL((SELECT COUNT(*) FROM Tasks t WHERE t.DecisionID = d.DecisionID), 0) AS TotalTasks,
                            d.CreatedAt
                          FROM Decisions d
                          LEFT JOIN Users u ON d.ResponsibleUserID = u.UserID
                          LEFT JOIN Events e ON d.EventID = e.EventID
                          LEFT JOIN Meetings m ON d.MeetingID = m.MeetingID
                          WHERE d.Status NOT IN ('Rejected', 'Cancelled')
                          ORDER BY d.CreatedAt DESC", con);

                    new SqlDataAdapter(cmd).Fill(dt);
                }

                dt.Columns.Add("FilterKey", typeof(string));
                dt.Columns.Add("StatusBadge", typeof(string));
                dt.Columns.Add("ProgressHtml", typeof(string));
                dt.Columns.Add("ContextLabel", typeof(string));
                dt.Columns.Add("CommitteeInitials", typeof(string));

                foreach (DataRow row in dt.Rows)
                {
                    string status = row["Status"].ToString();
                    row["DecisionTitle"] = HttpUtility.HtmlEncode(row["DecisionTitle"].ToString());
                    row["ReasonGist"] = HttpUtility.HtmlEncode(row["ReasonGist"].ToString());
                    row["ContextName"] = HttpUtility.HtmlEncode(row["ContextName"].ToString());
                    row["ResponsibleName"] = HttpUtility.HtmlEncode(row["ResponsibleName"].ToString());
                    row["Priority"] = HttpUtility.HtmlEncode(row["Priority"].ToString());
                    row["FilterKey"] = MapStatusKey(status);
                    row["StatusBadge"] = BuildStatusBadge(status);
                    row["ContextLabel"] = "Responsible";
                    row["CommitteeInitials"] = Initials(row["ResponsibleName"].ToString());

                    int done = Convert.ToInt32(row["DoneTasks"]);
                    int total = Convert.ToInt32(row["TotalTasks"]);
                    int progress = total > 0
                        ? (int)Math.Round((done * 100.0) / total)
                        : Convert.ToInt32(row["ProgressPct"]);

                    row["ProgressHtml"] = "<p class=\"tx-row-copy\">" + row["ReasonGist"] + "</p>" + BuildProgress(progress);
                }

                rptDecisions.DataSource = dt;
                rptDecisions.DataBind();
                pnlDecisionsEmpty.Visible = dt.Rows.Count == 0;
            }
            catch
            {
                rptDecisions.DataSource = null;
                rptDecisions.DataBind();
                pnlDecisionsEmpty.Visible = true;
            }
        }

        private void LoadOpenPolls()
        {
            try
            {
                DataTable dt = new DataTable();
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand(
                        @"SELECT p.PollID, p.PollTitle, p.Description, p.EndDate,
                                 ISNULL(e.EventName, '') AS EventName,
                                 ISNULL(d.DecisionTitle, '') AS DecisionTitle
                          FROM Polls p
                          LEFT JOIN Events e ON p.EventID = e.EventID
                          LEFT JOIN Decisions d ON p.DecisionID = d.DecisionID
                          WHERE p.IsActive = 1 AND (p.EndDate IS NULL OR p.EndDate >= GETDATE())
                          ORDER BY p.CreatedAt DESC", con);
                    new SqlDataAdapter(cmd).Fill(dt);
                }

                dt.Columns.Add("ContextLabel", typeof(string));
                foreach (DataRow row in dt.Rows)
                {
                    string eventName = row["EventName"].ToString();
                    string decision = row["DecisionTitle"].ToString();
                    string context = !string.IsNullOrEmpty(eventName)
                        ? "Event: " + eventName
                        : (!string.IsNullOrEmpty(decision) ? "Decision: " + decision : "Open community poll");
                    row["PollTitle"] = HttpUtility.HtmlEncode(row["PollTitle"].ToString());
                    row["Description"] = HttpUtility.HtmlEncode(Gist(row["Description"].ToString(), 140));
                    row["ContextLabel"] = HttpUtility.HtmlEncode(context);
                }

                rptOpenPolls.DataSource = dt;
                rptOpenPolls.DataBind();
                pnlPollsEmpty.Visible = dt.Rows.Count == 0;
            }
            catch
            {
                rptOpenPolls.DataSource = null;
                rptOpenPolls.DataBind();
                pnlPollsEmpty.Visible = true;
            }
        }

        private void LoadTrackerData()
        {
            try
            {
                DataTable dt = new DataTable();
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand(
                        @"SELECT TOP 12
                            t.TaskTitle AS ItemName,
                            ISNULL(leader.FullName, 'Lead to be assigned') AS Department,
                            LEFT(ISNULL(NULLIF(LTRIM(RTRIM(t.PublicSummary)), ''), 'Public summary not posted yet.'), 120) AS PublicGist,
                            t.DueDate, t.Status,
                            DATEDIFF(DAY, GETDATE(), t.DueDate) AS DaysRemaining
                          FROM Tasks t
                          LEFT JOIN TaskTeams tt ON tt.TaskID = t.TaskID
                          LEFT JOIN Users leader ON ISNULL(tt.LeaderID, t.LeaderID) = leader.UserID
                          WHERE t.Status IN ('Pending', 'InProgress', 'In Progress', 'Completed', 'Delayed', 'NotStarted')
                          ORDER BY t.DueDate", con);

                    new SqlDataAdapter(cmd).Fill(dt);
                }

                dt.Columns.Add("StatusBadge", typeof(string));
                dt.Columns.Add("DaysLabel", typeof(string));

                foreach (DataRow row in dt.Rows)
                {
                    string status = row["Status"].ToString();
                    row["ItemName"] = HttpUtility.HtmlEncode(row["ItemName"].ToString());
                    row["Department"] = HttpUtility.HtmlEncode(row["Department"].ToString());
                    row["StatusBadge"] = BuildTrackerBadge(status);

                    int days = row["DaysRemaining"] == DBNull.Value ? 0 : Convert.ToInt32(row["DaysRemaining"]);
                    if (status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                        row["DaysLabel"] = "Completed";
                    else if (days < 0)
                        row["DaysLabel"] = Math.Abs(days) + " Days Overdue";
                    else
                        row["DaysLabel"] = days + " Days";
                }

                rptTracker.DataSource = dt;
                rptTracker.DataBind();
                trTrackerEmpty.Visible = dt.Rows.Count == 0;
            }
            catch
            {
                rptTracker.DataSource = null;
                rptTracker.DataBind();
                trTrackerEmpty.Visible = true;
            }
        }

        private string MapStatusKey(string status)
        {
            switch (status.ToLowerInvariant().Replace(" ", ""))
            {
                case "proposed":
                case "created": return "proposed";
                case "underreview":
                case "reviewed": return "deliberating";
                case "approved": return "approved";
                case "implemented": return "implemented";
                case "completed":
                case "closed": return "completed";
                case "rejected": return "rejected";
                default: return "approved";
            }
        }

        private string BuildWorksBadge(string status)
        {
            string label = status;
            string colorClass = "";
            switch (status.ToLowerInvariant().Replace(" ", ""))
            {
                case "inprogress":
                case "ongoing":
                    label = "On Going";
                    colorClass = "tx-chip-wait";
                    break;
                case "proposed":
                    label = "Under Review";
                    colorClass = "tx-chip-wait";
                    break;
                case "planned":
                case "pending":
                case "approved":
                    label = "Planned";
                    break;
                case "delayed":
                    label = "Delayed";
                    colorClass = "tx-chip-high";
                    break;
                case "completed":
                    label = "Completed";
                    colorClass = "tx-chip-ok";
                    break;
            }
            return "<span class=\"tx-chip " + colorClass + "\">" + HttpUtility.HtmlEncode(label) + "</span>";
        }

        private string BuildStatusBadge(string status)
        {
            string label = status;
            string colorClass;
            switch (status.ToLowerInvariant().Replace(" ", ""))
            {
                case "implemented":
                case "completed":
                case "closed":
                    colorClass = "tx-chip-ok";
                    break;
                case "approved":
                    colorClass = "tx-chip-ok";
                    break;
                case "proposed":
                case "created":
                case "underreview":
                case "reviewed":
                    colorClass = "tx-chip-wait";
                    break;
                default:
                    colorClass = "tx-chip-wait";
                    break;
            }
            return "<span class=\"tx-chip " + colorClass + "\">" + HttpUtility.HtmlEncode(label) + "</span>";
        }

        private string BuildTrackerBadge(string status)
        {
            string label;
            string colorClass = "";
            switch (status.ToLowerInvariant().Replace(" ", ""))
            {
                case "completed":
                    label = "Completed";
                    colorClass = "tx-chip-ok";
                    break;
                case "inprogress":
                    label = "On Track";
                    colorClass = "tx-chip-wait";
                    break;
                case "delayed":
                    label = "Delayed";
                    colorClass = "tx-chip-high";
                    break;
                default:
                    label = "Pending";
                    break;
            }
            return "<span class=\"tx-chip " + colorClass + "\">" + label + "</span>";
        }

        private string BuildProgress(int progress)
        {
            if (progress < 0) progress = 0;
            if (progress > 100) progress = 100;
            return "<div class=\"tx-progress\"><div class=\"tx-progress-top\"><span>Progress</span><span>" + progress + "%</span></div><div class=\"tx-bar\"><span style=\"width:" + progress + "%\"></span></div></div>";
        }

        private string Gist(string text, int max)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";
            text = text.Trim();
            return text.Length <= max ? text : text.Substring(0, max).TrimEnd() + "...";
        }

        private string Initials(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "IC";
            StringBuilder sb = new StringBuilder();
            string[] words = name.Split(new[] { ' ', '-', '&', '/' }, StringSplitOptions.RemoveEmptyEntries);
            int count = 0;
            foreach (string word in words)
            {
                if (word.Length > 0)
                {
                    sb.Append(char.ToUpperInvariant(word[0]));
                    count++;
                }
                if (count >= 2) break;
            }
            return sb.Length > 0 ? sb.ToString() : "IC";
        }
    }
}
