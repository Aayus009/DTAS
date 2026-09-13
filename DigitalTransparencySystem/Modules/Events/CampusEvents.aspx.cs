using System;
using System.Data;
using System.Web;
using System.Web.UI.WebControls;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Events
{
    public partial class CampusEvents : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Modules/Authentication/Login.aspx");
                return;
            }

            if (RoleAccess.IsAdmin(Session["Role"] as string))
            {
                Response.Redirect("~/Modules/Events/Events.aspx");
                return;
            }

            lnkCreate.Visible = RoleAccess.CanCreateEvents(Session["Role"] as string);

            if (!IsPostBack)
                BindEvents();
        }

        protected void rptCampusEvents_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int eventId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out eventId))
                return;
            if (e.CommandName != "JoinPublic")
                return;

            string error = EventService.JoinPublic(eventId, Convert.ToInt32(Session["UserID"]));
            UiNotice.Bind(lblMessage, error, "Request sent. The event lead or manager must accept you before you can join.");

            BindEvents();
        }

        private void BindEvents()
        {
            int userId = Convert.ToInt32(Session["UserID"]);
            DataTable dt = EventService.ListDashboardEvents(userId);
            dt.Columns.Add("EventNameDisplay", typeof(string));
            dt.Columns.Add("DescriptionGist", typeof(string));
            dt.Columns.Add("WhenWhere", typeof(string));
            dt.Columns.Add("LeadDisplay", typeof(string));
            dt.Columns.Add("HandlerDisplay", typeof(string));
            dt.Columns.Add("ProgressPercent", typeof(int));
            dt.Columns.Add("ProgressLabel", typeof(string));
            dt.Columns.Add("IsPublic", typeof(int));
            dt.Columns.Add("CanRequestJoin", typeof(int));

            foreach (DataRow row in dt.Rows)
            {
                string name = Convert.ToString(row["EventName"]);
                string description = Convert.ToString(row["Description"]);
                if (string.IsNullOrWhiteSpace(description))
                    description = "No public description yet.";
                if (description.Length > 180)
                    description = description.Substring(0, 180) + "...";

                int total = row["TaskTotal"] == DBNull.Value ? 0 : Convert.ToInt32(row["TaskTotal"]);
                int done = row["TaskCompleted"] == DBNull.Value ? 0 : Convert.ToInt32(row["TaskCompleted"]);
                int percent = total <= 0 ? 0 : (int)Math.Round((done * 100.0) / total);

                string lead = Convert.ToString(row["LeadNames"]);
                string managers = Convert.ToString(row["ManagerNames"]);
                string visibility = Convert.ToString(row["Visibility"]);

                string start = row["StartDate"] == DBNull.Value ? "" : Convert.ToDateTime(row["StartDate"]).ToString("MMM dd, yyyy");
                string end = row["EndDate"] == DBNull.Value ? "" : Convert.ToDateTime(row["EndDate"]).ToString("MMM dd, yyyy");
                string venue = Convert.ToString(row["Venue"]);
                string whenWhere = start;
                if (!string.IsNullOrEmpty(end) && end != start)
                    whenWhere += " – " + end;
                if (!string.IsNullOrWhiteSpace(venue))
                    whenWhere += " · " + venue;
                if (string.IsNullOrWhiteSpace(whenWhere))
                    whenWhere = Convert.ToString(row["EventType"]);

                row["EventNameDisplay"] = HttpUtility.HtmlEncode(name);
                row["DescriptionGist"] = HttpUtility.HtmlEncode(description);
                row["WhenWhere"] = HttpUtility.HtmlEncode(whenWhere);
                row["LeadDisplay"] = HttpUtility.HtmlEncode(string.IsNullOrWhiteSpace(lead) ? "Not assigned" : lead);
                row["HandlerDisplay"] = HttpUtility.HtmlEncode(string.IsNullOrWhiteSpace(managers)
                    ? (string.IsNullOrWhiteSpace(lead) ? "Not assigned" : lead)
                    : managers);
                row["ProgressPercent"] = percent;
                row["ProgressLabel"] = total == 0 ? "No tasks yet" : done + " of " + total + " tasks · " + percent + "%";
                row["IsPublic"] = string.Equals(visibility, "Public", StringComparison.OrdinalIgnoreCase) ? 1 : 0;

                int hasRequest = row["HasJoinRequest"] == DBNull.Value ? 0 : Convert.ToInt32(row["HasJoinRequest"]);
                bool openToJoin = !string.Equals(Convert.ToString(row["Status"]), "Completed", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(Convert.ToString(row["Status"]), "Cancelled", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(Convert.ToString(row["Status"]), "Archived", StringComparison.OrdinalIgnoreCase);
                row["CanRequestJoin"] = Convert.ToInt32(row["IsMember"]) == 0
                    && string.Equals(visibility, "Public", StringComparison.OrdinalIgnoreCase)
                    && hasRequest == 0
                    && openToJoin ? 1 : 0;
            }

            rptCampusEvents.DataSource = dt;
            rptCampusEvents.DataBind();
            rptCampusEvents.Visible = dt.Rows.Count > 0;
            pnlNoCampusEvents.Visible = dt.Rows.Count == 0;
        }
    }
}
