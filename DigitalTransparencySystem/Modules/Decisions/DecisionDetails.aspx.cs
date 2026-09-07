using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace DigitalTransparencySystem.Modules.Decisions
{
    public partial class DecisionDetails : System.Web.UI.Page
    {
        private string connectionString;
        private int decisionID;

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

            if (!int.TryParse(Request.QueryString["DecisionID"], out decisionID))
            {
                Response.Redirect("~/Modules/Decisions/Decisions.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadDecisionDetails();
                LoadDecisionHistory();
                LoadStatusTimeline();
            }
        }

        private void LoadDecisionDetails()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    @"SELECT d.DecisionTitle, d.Description, d.Priority, d.Status, d.DueDate,
                             d.CreatedAt, d.UpdatedAt,
                             ISNULL(u.FullName, 'Unassigned') AS ResponsiblePerson,
                             ISNULL(m.MeetingTitle, 'N/A') AS RelatedMeeting,
                             ISNULL(e.EventName, 'N/A') AS RelatedEvent
                      FROM Decisions d
                      LEFT JOIN Users u ON d.ResponsibleUserID = u.UserID
                      LEFT JOIN Meetings m ON d.MeetingID = m.MeetingID
                      LEFT JOIN Events e ON d.EventID = e.EventID
                      WHERE d.DecisionID = @DecisionID", con);
                cmd.Parameters.AddWithValue("@DecisionID", decisionID);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string decisionTitle = reader["DecisionTitle"].ToString();
                        string status = reader["Status"].ToString();
                        string priority = reader["Priority"].ToString();

                        litTitle.Text = $"<h1 class=\"font-headline-lg text-headline-lg text-primary\">{Server.HtmlEncode(decisionTitle)}</h1>";
                        litStatusBadge.Text = GetStatusBadgeHtml(status);
                        litPriorityBadge.Text = GetPriorityBadgeHtml(priority);
                        litResponsiblePerson.Text = Server.HtmlEncode(reader["ResponsiblePerson"].ToString());
                        litDescription.Text = Server.HtmlEncode(reader["Description"].ToString());
                        litRelatedMeeting.Text = Server.HtmlEncode(reader["RelatedMeeting"].ToString());
                        litRelatedEvent.Text = Server.HtmlEncode(reader["RelatedEvent"].ToString());

                        if (reader["DueDate"] != DBNull.Value)
                            litDueDate.Text = Convert.ToDateTime(reader["DueDate"]).ToString("MMM dd, yyyy");
                        else
                            litDueDate.Text = "Not set";

                        litCreatedAt.Text = Convert.ToDateTime(reader["CreatedAt"]).ToString("MMM dd, yyyy - hh:mm tt");
                        litUpdatedAt.Text = Convert.ToDateTime(reader["UpdatedAt"]).ToString("MMM dd, yyyy - hh:mm tt");
                        litResponsibleDetail.Text = Server.HtmlEncode(reader["ResponsiblePerson"].ToString());
                    }
                    else
                    {
                        Response.Redirect("~/Modules/Decisions/Decisions.aspx");
                    }
                }

                // Get created by name
                SqlCommand cmdCreator = new SqlCommand(
                    @"SELECT ISNULL(u.FullName, 'Unknown') AS CreatorName
                      FROM Decisions d
                      LEFT JOIN Users u ON d.CreatedBy = u.UserID
                      WHERE d.DecisionID = @DecisionID", con);
                cmdCreator.Parameters.AddWithValue("@DecisionID", decisionID);
                object creatorResult = cmdCreator.ExecuteScalar();
                litCreatedBy.Text = creatorResult != null ? Server.HtmlEncode(creatorResult.ToString()) : "Unknown";

                LoadExecutionProgress(con);
            }
        }

        private void LoadExecutionProgress(SqlConnection con)
        {
            SqlCommand cmd = new SqlCommand(
                @"SELECT COUNT(*) AS Total,
                         SUM(CASE WHEN Status = 'Completed' THEN 1 ELSE 0 END) AS Completed
                  FROM Tasks WHERE DecisionID = @DecisionID", con);
            cmd.Parameters.AddWithValue("@DecisionID", decisionID);
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    int total = Convert.ToInt32(reader["Total"]);
                    int completed = reader["Completed"] != DBNull.Value ? Convert.ToInt32(reader["Completed"]) : 0;
                    int pct = total > 0 ? (int)Math.Round((double)completed / total * 100) : 0;
                    litProgressPercent.Text = pct + "%";
                    litProgressBar.Text = pct + "%";
                }
            }
        }

        private void LoadDecisionHistory()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    @"SELECT dh.OldStatus, dh.NewStatus, dh.Comments, dh.ChangedAt,
                             ISNULL(u.FullName, 'System') AS ChangedByName
                      FROM DecisionHistory dh
                      LEFT JOIN Users u ON dh.ChangedBy = u.UserID
                      WHERE dh.DecisionID = @DecisionID
                      ORDER BY dh.ChangedAt DESC", con);
                cmd.Parameters.AddWithValue("@DecisionID", decisionID);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dt.Columns.Add("OldStatusDisplay", typeof(string));

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        string oldStatus = row["OldStatus"] != DBNull.Value ? row["OldStatus"].ToString() : null;
                        row["OldStatusDisplay"] = oldStatus != null ? GetStatusDisplayText(oldStatus) : "Created";
                    }

                    rptHistory.DataSource = dt;
                    rptHistory.DataBind();
                    pnlNoHistory.Visible = false;
                    litHistoryCount.Text = dt.Rows.Count.ToString();
                }
                else
                {
                    rptHistory.Visible = false;
                    pnlNoHistory.Visible = true;
                    litHistoryCount.Text = "0";
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

            string status = GetCurrentStatus();

            string[] allStatuses;
            if (status == "Rejected")
            {
                allStatuses = new string[] { "Proposed", "Under Review", "Rejected" };
            }
            else
            {
                allStatuses = new string[] { "Proposed", "Under Review", "Approved", "Completed" };
            }

            string statusKey = GetStatusKey(status);
            int currentIndex = -1;
            for (int i = 0; i < allStatuses.Length; i++)
            {
                if (allStatuses[i].Replace(" ", "") == statusKey)
                {
                    currentIndex = i;
                    break;
                }
            }
            if (currentIndex < 0) currentIndex = 0;

            for (int i = 0; i < allStatuses.Length; i++)
            {
                string label = allStatuses[i];
                bool isCompleted = i < currentIndex;
                bool isCurrent = i == currentIndex;

                string dotStyle = isCompleted || isCurrent
                    ? "background-color: #00236f;"
                    : "background-color: #e0e0e0;";
                string iconColor = isCompleted || isCurrent ? "color: white;" : "color: #999;";
                string icon = isCompleted ? "check" : isCurrent ? "radio_button_checked" : "circle";

                timeline.Rows.Add(label, icon, "", dotStyle, iconColor);
            }

            rptTimeline.DataSource = timeline;
            rptTimeline.DataBind();

            int progressPercent = (currentIndex * 100) / (allStatuses.Length - 1);
            litTimelineWidth.Text = progressPercent + "%";
        }

        private string GetStatusKey(string status)
        {
            switch (status)
            {
                case "Proposed": return "Proposed";
                case "UnderReview": return "UnderReview";
                case "Approved": return "Approved";
                case "Rejected": return "Rejected";
                case "Completed": return "Completed";
                default: return status;
            }
        }

        private string GetCurrentStatus()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("SELECT Status FROM Decisions WHERE DecisionID = @DecisionID", con);
                cmd.Parameters.AddWithValue("@DecisionID", decisionID);
                object result = cmd.ExecuteScalar();
                return result != null ? result.ToString() : "Proposed";
            }
        }

        private string GetStatusBadgeHtml(string status)
        {
            string bgColor, textColor, icon;

            switch (status)
            {
                case "Proposed":
                    bgColor = "background-color: rgba(1,87,155,0.12);";
                    textColor = "color: #01579b;";
                    icon = "new_releases";
                    break;
                case "UnderReview":
                    bgColor = "background-color: rgba(230,126,0,0.12);";
                    textColor = "color: #e67e00;";
                    icon = "pending";
                    break;
                case "Approved":
                    bgColor = "background-color: rgba(46,125,50,0.12);";
                    textColor = "color: #2e7d32;";
                    icon = "check_circle";
                    break;
                case "Rejected":
                    bgColor = "background-color: rgba(211,47,47,0.12);";
                    textColor = "color: #d32f2f;";
                    icon = "cancel";
                    break;
                case "Completed":
                    bgColor = "background-color: rgba(39,165,119,0.12);";
                    textColor = "color: #27a577;";
                    icon = "task_alt";
                    break;
                default:
                    bgColor = "background-color: rgba(0,35,111,0.1);";
                    textColor = "color: #00236f;";
                    icon = "circle";
                    break;
            }

            return $"<span class=\"inline-flex items-center gap-1 px-2.5 py-1 rounded-full font-badge-cap text-badge-cap\" style=\"{bgColor}{textColor}\">" +
                   $"<span class=\"material-symbols-outlined text-[14px]\">{icon}</span>{GetStatusDisplayText(status)}</span>";
        }

        private string GetPriorityBadgeHtml(string priority)
        {
            string bgColor, textColor, icon;

            switch (priority)
            {
                case "Critical":
                    bgColor = "background-color: rgba(183,28,28,0.15);";
                    textColor = "color: #b71c1c;";
                    icon = "priority_high";
                    break;
                case "High":
                    bgColor = "background-color: rgba(211,47,47,0.12);";
                    textColor = "color: #d32f2f;";
                    icon = "arrow_upward";
                    break;
                case "Medium":
                    bgColor = "background-color: rgba(230,126,0,0.12);";
                    textColor = "color: #e67e00;";
                    icon = "remove";
                    break;
                case "Low":
                    bgColor = "background-color: rgba(1,87,155,0.12);";
                    textColor = "color: #01579b;";
                    icon = "arrow_downward";
                    break;
                default:
                    bgColor = "background-color: rgba(0,35,111,0.1);";
                    textColor = "color: #00236f;";
                    icon = "help";
                    break;
            }

            return $"<span class=\"inline-flex items-center gap-1 px-2.5 py-1 rounded-full font-badge-cap text-badge-cap\" style=\"{bgColor}{textColor}\">" +
                   $"<span class=\"material-symbols-outlined text-[14px]\">{icon}</span>{Server.HtmlEncode(priority)}</span>";
        }

        private string GetStatusDisplayText(string status)
        {
            switch (status)
            {
                case "Proposed":
                    return "Proposed";
                case "UnderReview":
                    return "Under Review";
                case "Approved":
                    return "Approved";
                case "Rejected":
                    return "Rejected";
                case "Completed":
                    return "Completed";
                default:
                    return status;
            }
        }

        protected void btnAdvanceStatus_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ddlNextStatus.SelectedValue))
                return;

            try
            {
                string oldStatus = GetCurrentStatus();
                string newStatus = ddlNextStatus.SelectedValue;

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    SqlCommand cmdUpdate = new SqlCommand(
                        "UPDATE Decisions SET Status = @Status, UpdatedAt = GETDATE() WHERE DecisionID = @DecisionID", con);
                    cmdUpdate.Parameters.AddWithValue("@Status", newStatus);
                    cmdUpdate.Parameters.AddWithValue("@DecisionID", decisionID);
                    cmdUpdate.ExecuteNonQuery();

                    SqlCommand cmdHistory = new SqlCommand(@"
                        INSERT INTO DecisionHistory 
                            (DecisionID, OldStatus, NewStatus, ChangedBy, Comments, ChangedAt)
                        VALUES 
                            (@DecisionID, @OldStatus, @NewStatus, @ChangedBy, @Comments, GETDATE())", con);
                    cmdHistory.Parameters.AddWithValue("@DecisionID", decisionID);
                    cmdHistory.Parameters.AddWithValue("@OldStatus", oldStatus);
                    cmdHistory.Parameters.AddWithValue("@NewStatus", newStatus);
                    cmdHistory.Parameters.AddWithValue("@ChangedBy", Session["UserID"]);
                    cmdHistory.Parameters.AddWithValue("@Comments", $"Status changed from {GetStatusDisplayText(oldStatus)} to {GetStatusDisplayText(newStatus)}");
                    cmdHistory.ExecuteNonQuery();
                }

                Response.Redirect(Request.RawUrl);
            }
            catch (Exception ex)
            {
                pnlError.Visible = true;
                lblError.Text = "An error occurred: " + ex.Message;
            }
        }

        protected void btnAddComment_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtComment.Text))
                return;

            try
            {
                string currentStatus = GetCurrentStatus();

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand(@"
                        INSERT INTO DecisionHistory 
                            (DecisionID, OldStatus, NewStatus, ChangedBy, Comments, ChangedAt)
                        VALUES 
                            (@DecisionID, @OldStatus, @NewStatus, @ChangedBy, @Comments, GETDATE())", con);
                    cmd.Parameters.AddWithValue("@DecisionID", decisionID);
                    cmd.Parameters.AddWithValue("@OldStatus", currentStatus);
                    cmd.Parameters.AddWithValue("@NewStatus", currentStatus);
                    cmd.Parameters.AddWithValue("@ChangedBy", Session["UserID"]);
                    cmd.Parameters.AddWithValue("@Comments", txtComment.Text.Trim());
                    cmd.ExecuteNonQuery();
                }

                Response.Redirect(Request.RawUrl);
            }
            catch (Exception ex)
            {
                pnlError.Visible = true;
                lblError.Text = "An error occurred: " + ex.Message;
            }
        }

        protected void btnBackToList_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Modules/Decisions/Decisions.aspx");
        }

    }
}
