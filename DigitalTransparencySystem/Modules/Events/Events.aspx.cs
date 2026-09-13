using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Events
{
    public partial class Events : System.Web.UI.Page
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

                string role = Session["Role"] as string;
                if (role == null || !role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                {
                    Response.Redirect("~/Modules/Authentication/Login.aspx");
                    return;
                }

                EventTaskService.PromoteEventsInProgressFromTasks();
                LoadEvents();
                LoadStats();
            }
        }

        private const string LiveEventFromSql = @"
            FROM Events e
            LEFT JOIN Users p ON e.ProposedBy = p.UserID
            OUTER APPLY (
                SELECT
                    COUNT(*) AS Total,
                    SUM(CASE WHEN REPLACE(LTRIM(RTRIM(ISNULL(t.Status, N''))), N' ', N'') = N'Completed' THEN 1 ELSE 0 END) AS Completed,
                    SUM(CASE WHEN REPLACE(LTRIM(RTRIM(ISNULL(t.Status, N''))), N' ', N'') IN
                        (N'InProgress', N'UnderReview', N'Delayed', N'RevisionNeeded', N'Submitted', N'ChangesRequested')
                        THEN 1 ELSE 0 END) AS InWork
                FROM Tasks t
                WHERE t.EventID = e.EventID
                  AND ISNULL(t.IsDeleted, 0) = 0
                  AND REPLACE(LTRIM(RTRIM(ISNULL(t.Status, N''))), N' ', N'') NOT IN (N'Archived', N'Cancelled')
            ) ts";

        private const string LiveEventStatusSql = @"
            CASE
                WHEN REPLACE(LTRIM(RTRIM(ISNULL(e.Status, N''))), N' ', N'') IN (N'Proposed', N'Rejected', N'Cancelled', N'Archived')
                    THEN LTRIM(RTRIM(e.Status))
                WHEN ts.Total > 0 AND ts.Completed = ts.Total THEN N'Completed'
                WHEN ISNULL(ts.InWork, 0) > 0 OR (ISNULL(ts.Completed, 0) > 0 AND ISNULL(ts.Completed, 0) < ts.Total) THEN N'InProgress'
                ELSE LTRIM(RTRIM(ISNULL(e.Status, N'Planned')))
            END";

        private void LoadEvents(string search = "", string status = "")
        {
            EventTaskService.PromoteEventsInProgressFromTasks();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"SELECT e.EventID, e.EventName, e.Description, e.EventType, e.StartDate, e.EndDate, e.Venue, 
                                        e.Budget, e.Organizer, " + LiveEventStatusSql + @" AS Status, e.CreatedBy, e.CreatedAt, e.UpdatedAt,
                                        e.RejectionReason, ISNULL(p.FullName, '') AS ProposedBy,
                                        ISNULL(e.Visibility, N'Private') AS Visibility,
                                        ISNULL(e.IsDisabled, 0) AS IsRestricted,
                                        ISNULL(ts.Total, 0) AS TaskTotal,
                                        ISNULL(ts.Completed, 0) AS TaskCompleted,
                                        CASE WHEN ISNULL(ts.Total, 0) = 0 THEN 0
                                             ELSE CAST(ROUND((ISNULL(ts.Completed, 0) * 100.0) / ts.Total, 0) AS INT)
                                        END AS CompletionPercent
                                 " + LiveEventFromSql + @"
                                 WHERE ISNULL(e.IsDeleted, 0) = 0";

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query += " AND e.EventName LIKE @Search";
                }

                if (!string.IsNullOrWhiteSpace(status))
                {
                    query += " AND REPLACE(LTRIM(RTRIM(" + LiveEventStatusSql + ")), N' ', N'') = REPLACE(LTRIM(RTRIM(@Status)), N' ', N'')";
                }

                query += " ORDER BY e.StartDate DESC";

                SqlCommand cmd = new SqlCommand(query, con);

                if (!string.IsNullOrWhiteSpace(search))
                {
                    cmd.Parameters.AddWithValue("@Search", "%" + search.Trim() + "%");
                }

                if (!string.IsNullOrWhiteSpace(status))
                {
                    cmd.Parameters.AddWithValue("@Status", status.Trim());
                }

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                rptEvents.DataSource = dt;
                rptEvents.DataBind();

                int totalFiltered = dt.Rows.Count;
                pnlNoEvents.Visible = totalFiltered == 0;
                rptEvents.Visible = totalFiltered > 0;
            }
        }

        private void LoadStats()
        {
            int total = 0, planned = 0, inProgress = 0, completed = 0, proposed = 0;

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT " + LiveEventStatusSql + @" AS Status
                 " + LiveEventFromSql + @"
                 WHERE ISNULL(e.IsDeleted, 0) = 0", con))
            {
                DataTable dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                total = dt.Rows.Count;
                foreach (DataRow row in dt.Rows)
                {
                    string status = NormalizeStatus(row["Status"] == DBNull.Value ? "" : row["Status"].ToString());
                    if (status.Equals("Planned", StringComparison.OrdinalIgnoreCase))
                        planned++;
                    else if (status.Equals("InProgress", StringComparison.OrdinalIgnoreCase))
                        inProgress++;
                    else if (status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                        completed++;
                    else if (status.Equals("Proposed", StringComparison.OrdinalIgnoreCase))
                        proposed++;
                }
            }

            litTotalEvents.Text = total.ToString();
            litPlanned.Text = planned.ToString();
            litInProgress.Text = inProgress.ToString();
            litCompleted.Text = completed.ToString();
            litPendingProposals.Text = proposed.ToString();
        }

        protected void rptEvents_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                Label lblStatus = (Label)e.Item.FindControl("lblStatus");
                if (lblStatus != null)
                {
                    string status = DataBinder.Eval(e.Item.DataItem, "Status").ToString();
                    lblStatus.Text = EventTaskService.EventStatusDisplay(status);
                    lblStatus.CssClass += " " + GetStatusBadgeClass(status);
                }

                Label lblProposedBy = (Label)e.Item.FindControl("lblProposedBy");
                if (lblProposedBy != null)
                {
                    string proposer = DataBinder.Eval(e.Item.DataItem, "ProposedBy").ToString();
                    lblProposedBy.Text = string.IsNullOrEmpty(proposer) ? "-" : proposer;
                    lblProposedBy.CssClass = string.IsNullOrEmpty(proposer)
                        ? "text-xs text-outline"
                        : "text-sm font-semibold text-on-surface";
                }
            }
        }

        protected void rptEvents_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int eventId;
            if (!int.TryParse(e.CommandArgument.ToString(), out eventId))
                return;

            switch (e.CommandName)
            {
                case "ViewEvent":
                    Response.Redirect($"~/Modules/Events/EventDetails.aspx?EventID={eventId}");
                    break;
                case "RestrictEvent":
                    RestrictionService.SetEventRestricted(eventId, Convert.ToInt32(Session["UserID"]), Session["Role"] as string, true);
                    LoadEvents();
                    LoadStats();
                    break;
                case "RestoreEvent":
                    RestrictionService.SetEventRestricted(eventId, Convert.ToInt32(Session["UserID"]), Session["Role"] as string, false);
                    LoadEvents();
                    LoadStats();
                    break;
                case "ApproveEvent":
                    ApproveEvent(eventId);
                    LoadEvents();
                    LoadStats();
                    break;
                case "RejectEvent":
                    RejectEvent(eventId, hfRejectReason.Value);
                    hfRejectReason.Value = "";
                    LoadEvents();
                    LoadStats();
                    break;
            }
        }

        private void ApproveEvent(int eventId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    "UPDATE Events SET Status = 'Planned', RejectionReason = NULL, UpdatedAt = GETDATE() WHERE EventID = @EventID", con);
                cmd.Parameters.AddWithValue("@EventID", eventId);
                cmd.ExecuteNonQuery();

                NotifyProposer(con, eventId, "Event Proposal Approved",
                    $"Your event proposal has been approved and is now in the planning stage.");
            }

            EventService.AfterApproved(eventId, Convert.ToInt32(Session["UserID"]));
        }

        private void RejectEvent(int eventId, string reason)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    "UPDATE Events SET Status = 'Rejected', RejectionReason = @Reason, UpdatedAt = GETDATE() WHERE EventID = @EventID", con);
                cmd.Parameters.AddWithValue("@EventID", eventId);
                cmd.Parameters.AddWithValue("@Reason", (object)reason ?? DBNull.Value);
                cmd.ExecuteNonQuery();

                string proposerName = GetProposerName(con, eventId);
                NotifyProposer(con, eventId, "Event Proposal Rejected",
                    $"Your event proposal was not approved. Reason: {reason}");
            }
        }

        private string GetProposerName(SqlConnection con, int eventId)
        {
            SqlCommand cmd = new SqlCommand(
                "SELECT ISNULL(p.FullName, '') FROM Events e LEFT JOIN Users p ON e.ProposedBy = p.UserID WHERE e.EventID = @EventID", con);
            cmd.Parameters.AddWithValue("@EventID", eventId);
            return cmd.ExecuteScalar()?.ToString() ?? "";
        }

        private void NotifyProposer(SqlConnection con, int eventId, string title, string message)
        {
            SqlCommand cmd = new SqlCommand(@"
                INSERT INTO Notifications (UserID, Title, Message, IsRead, NotificationType, RelatedID, RelatedType, CreatedAt)
                SELECT ProposedBy, @Title, @Message, 0, 'Event', @EventID, 'Event', GETDATE()
                FROM Events WHERE EventID = @EventID AND ProposedBy IS NOT NULL", con);
            cmd.Parameters.AddWithValue("@Title", title);
            cmd.Parameters.AddWithValue("@Message", message);
            cmd.Parameters.AddWithValue("@EventID", eventId);
            cmd.ExecuteNonQuery();
        }

        private void ArchiveEvent(int eventId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    "UPDATE Events SET Status = 'Archived', UpdatedAt = GETDATE() WHERE EventID = @EventID", con);
                cmd.Parameters.AddWithValue("@EventID", eventId);
                cmd.ExecuteNonQuery();
            }
        }

        private string GetStatusBadgeClass(string status)
        {
            switch (NormalizeStatus(status).ToLowerInvariant())
            {
                case "planned":
                    return "badge-status-planned";
                case "inprogress":
                    return "badge-status-inprogress";
                case "completed":
                    return "badge-status-completed";
                case "cancelled":
                    return "badge-status-cancelled";
                case "proposed":
                    return "badge-status-proposed";
                case "rejected":
                    return "badge-status-rejected";
                default:
                    return "badge-status-planned";
            }
        }

        protected string CompletionMeta(object totalObj, object doneObj)
        {
            int total = totalObj == null || totalObj == DBNull.Value ? 0 : Convert.ToInt32(totalObj);
            int done = doneObj == null || doneObj == DBNull.Value ? 0 : Convert.ToInt32(doneObj);
            return EventTaskService.CompletionLabel(done, total);
        }

        private static string NormalizeStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return string.Empty;
            return status.Replace(" ", "").Trim();
        }
    }
}
