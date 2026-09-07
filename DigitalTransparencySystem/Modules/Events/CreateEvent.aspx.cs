using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Events
{
    public partial class CreateEvent : System.Web.UI.Page
    {
        private string connectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            connectionString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;

            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Modules/Authentication/Login.aspx");
                return;
            }

            string role = Session["Role"] as string;
            if (RoleAccess.IsAdmin(role) || !RoleAccess.CanCreateEvents(role))
            {
                Response.Redirect(RoleAccess.IsAdmin(role)
                    ? "~/Modules/Events/Events.aspx"
                    : "~/Modules/Events/ProposeEvent.aspx");
                return;
            }

            adminTop.Visible = false;
            adminSide.Visible = false;
            userTop.Visible = true;
            userSide.Visible = true;

            if (!IsPostBack)
            {
                BindLeads();
                BindClubs();
            }
        }

        private void BindClubs()
        {
            ddlClub.Items.Clear();
            ddlClub.Items.Add(new ListItem("No club", "0"));
            int userId = Convert.ToInt32(Session["UserID"]);
            DataTable clubs = ClubService.ListMyClubs(userId);
            foreach (DataRow row in clubs.Rows)
            {
                if (row.Table.Columns.Contains("IsRestricted") && row["IsRestricted"] != DBNull.Value
                    && Convert.ToBoolean(row["IsRestricted"]))
                    continue;
                string name = Convert.ToString(row["ClubName"]);
                string id = Convert.ToString(row["ClubID"]);
                string code = row.Table.Columns.Contains("InviteCode") && row["InviteCode"] != DBNull.Value
                    ? Convert.ToString(row["InviteCode"])
                    : "";
                string label = name + " (ID " + id + (string.IsNullOrEmpty(code) ? "" : " · " + code) + ")";
                ddlClub.Items.Add(new ListItem(label, id));
            }
        }

        protected void btnCreate_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
                return;

            DateTime start;
            DateTime end;
            if (!DateTime.TryParse(txtStartDate.Text, out start) || !DateTime.TryParse(txtEndDate.Text, out end))
            {
                ShowError("Enter a valid start and end.");
                return;
            }
            if (end < start)
            {
                ShowError("End must be on or after start.");
                return;
            }

            int leadId;
            if (!int.TryParse(ddlEventLead.SelectedValue, out leadId) || leadId <= 0)
            {
                ShowError("Choose an event lead.");
                return;
            }

            UserAccount lead = AuthService.FindById(leadId);
            if (lead == null || lead.IsDeleted || !RoleAccess.CanHoldEventAdmin(lead.Role))
            {
                ShowError("The event lead must be a verified faculty or staff member.");
                return;
            }

            try
            {
                int eventId;
                int creatorId = Convert.ToInt32(Session["UserID"]);
                string actorRole = Session["Role"] as string;
                string clubError;
                int? clubId = EventService.ResolveClubForActor(ddlClub.SelectedValue, txtClubId.Text, creatorId, actorRole, out clubError);
                if (clubError != null)
                {
                    ShowError(clubError);
                    return;
                }

                string description = txtDescription.Text.Trim();
                EventService.EnsureClubLinkSchema();
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand(@"
                        INSERT INTO Events
                            (EventName, Description, EventType, StartDate, EndDate, Venue, Budget, Organizer, Status, CreatedBy, CreatedAt, UpdatedAt, Visibility, ClubID)
                        VALUES
                            (@EventName, @Description, @EventType, @StartDate, @EndDate, @Venue, @Budget, @Organizer, N'Planned', @CreatedBy, GETDATE(), GETDATE(), @Visibility, @ClubID);
                        SELECT CAST(SCOPE_IDENTITY() AS INT);", con);

                    cmd.Parameters.AddWithValue("@EventName", txtEventName.Text.Trim());
                    cmd.Parameters.AddWithValue("@Description", string.IsNullOrEmpty(description) ? (object)DBNull.Value : description);
                    cmd.Parameters.AddWithValue("@EventType", ddlEventType.SelectedValue);
                    cmd.Parameters.AddWithValue("@StartDate", start);
                    cmd.Parameters.AddWithValue("@EndDate", end);
                    cmd.Parameters.AddWithValue("@Venue", txtVenue.Text.Trim());

                    decimal budget;
                    if (decimal.TryParse(txtBudget.Text.Trim(), out budget))
                        cmd.Parameters.AddWithValue("@Budget", budget);
                    else
                        cmd.Parameters.AddWithValue("@Budget", DBNull.Value);

                    cmd.Parameters.AddWithValue("@Organizer", string.IsNullOrWhiteSpace(lead.FullName) ? (object)DBNull.Value : lead.FullName);
                    cmd.Parameters.AddWithValue("@CreatedBy", creatorId);
                    cmd.Parameters.AddWithValue("@Visibility", ddlVisibility.SelectedValue);
                    cmd.Parameters.AddWithValue("@ClubID", clubId.HasValue ? (object)clubId.Value : DBNull.Value);

                    eventId = Convert.ToInt32(cmd.ExecuteScalar());
                }

                EventService.AfterCreated(eventId, creatorId, leadId);
                if (leadId != creatorId)
                {
                    NotificationService.Send(leadId, "Event lead",
                        "You are the lead for " + txtEventName.Text.Trim() + ".",
                        "Event", eventId, "Event");
                }

                foreach (string email in SplitEmails(txtInvites.Text))
                    EventService.InviteByEmail(eventId, creatorId, actorRole, email, EventRoles.Participant);

                if (clubId.HasValue && chkCreateConnect.Checked)
                {
                    int groupId;
                    ConnectService.CreateGroupForEvent(eventId, creatorId, clubId, out groupId);
                }

                Response.Redirect("~/Modules/Events/EventWorkspace.aspx?EventID=" + eventId);
            }
            catch (System.Threading.ThreadAbortException)
            {
                throw;
            }
            catch (Exception ex)
            {
                ShowError("The event could not be created: " + ex.Message);
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Modules/Events/MyEvents.aspx");
        }

        private void BindLeads()
        {
            ddlEventLead.Items.Clear();
            ddlEventLead.Items.Add(new ListItem("Select a verified faculty or staff member", "0"));

            int currentId = Convert.ToInt32(Session["UserID"]);
            bool currentListed = false;
            DataTable leads = EventService.ListEventLeadCandidates();
            foreach (DataRow row in leads.Rows)
            {
                int userId = Convert.ToInt32(row["UserID"]);
                string label = Convert.ToString(row["FullName"]) + " - " + Convert.ToString(row["Email"]);
                ddlEventLead.Items.Add(new ListItem(label, userId.ToString()));
                if (userId == currentId)
                    currentListed = true;
            }

            if (!currentListed && RoleAccess.CanHoldEventAdmin(Session["Role"] as string))
            {
                string name = Convert.ToString(Session["FullName"]);
                string email = Convert.ToString(Session["Email"]);
                string label = (string.IsNullOrWhiteSpace(name) ? "You" : name)
                    + (string.IsNullOrWhiteSpace(email) ? "" : " - " + email);
                ddlEventLead.Items.Insert(1, new ListItem(label, currentId.ToString()));
            }

            ListItem self = ddlEventLead.Items.FindByValue(currentId.ToString());
            if (self != null && RoleAccess.CanHoldEventAdmin(Session["Role"] as string))
                ddlEventLead.SelectedValue = currentId.ToString();
        }

        private static IEnumerable<string> SplitEmails(string raw)
        {
            var emails = new List<string>();
            if (string.IsNullOrWhiteSpace(raw))
                return emails;

            string[] parts = raw.Split(new[] { '\r', '\n', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string part in parts)
            {
                string email = part.Trim();
                if (email.IndexOf('@') < 1)
                    continue;
                if (seen.Add(email))
                    emails.Add(email);
            }
            return emails;
        }

        private void ShowError(string message)
        {
            pnlError.Visible = true;
            lblError.Text = message;
        }
    }
}
