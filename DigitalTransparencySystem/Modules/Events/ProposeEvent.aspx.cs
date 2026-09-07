using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Events
{
    public partial class ProposeEvent : System.Web.UI.Page
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

            if (!IsPostBack)
            {
                BindLeads();
                BindClubs();
                LoadMyProposals();
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

        private void LoadMyProposals()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
                    @"SELECT EventName, Status, CreatedAt, RejectionReason
                      FROM Events
                      WHERE ProposedBy = @UserID
                      ORDER BY CreatedAt DESC", con);
                cmd.Parameters.AddWithValue("@UserID", Session["UserID"]);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                rptMyProposals.DataSource = dt;
                rptMyProposals.DataBind();
                pnlNoProposals.Visible = dt.Rows.Count == 0;
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
                return;

            DateTime startDate;
            DateTime endDate;
            bool hasStart = DateTime.TryParse(txtStartDate.Text, out startDate);
            bool hasEnd = DateTime.TryParse(txtEndDate.Text, out endDate);
            if (hasStart && hasEnd && endDate < startDate)
            {
                ShowMessage("End must be on or after start.", false);
                return;
            }

            int leadId;
            if (!int.TryParse(ddlEventLead.SelectedValue, out leadId) || leadId <= 0)
            {
                ShowMessage("Choose a suggested event lead.", false);
                return;
            }

            UserAccount lead = AuthService.FindById(leadId);
            if (lead == null || lead.IsDeleted || !RoleAccess.CanHoldEventAdmin(lead.Role))
            {
                ShowMessage("The suggested lead must be a verified faculty or staff member.", false);
                return;
            }

            try
            {
                int creatorId = Convert.ToInt32(Session["UserID"]);
                string clubError;
                int? clubId = EventService.ResolveClubForActor(ddlClub.SelectedValue, txtClubId.Text, creatorId, Session["Role"] as string, out clubError);
                if (clubError != null)
                {
                    ShowMessage(clubError, false);
                    return;
                }

                EventService.EnsureClubLinkSchema();
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand(@"
                        INSERT INTO Events
                            (EventName, Description, EventType, StartDate, EndDate, Venue, Budget, Organizer, Status, CreatedBy, ProposedBy, CreatedAt, UpdatedAt, Visibility, ClubID)
                        VALUES
                            (@EventName, @Description, @EventType, @StartDate, @EndDate, @Venue, @Budget, @Organizer, N'Proposed', @CreatedBy, @CreatedBy, GETDATE(), GETDATE(), @Visibility, @ClubID)", con);

                    cmd.Parameters.AddWithValue("@EventName", txtEventName.Text.Trim());
                    cmd.Parameters.AddWithValue("@Description", txtDescription.Text.Trim());
                    cmd.Parameters.AddWithValue("@EventType", ddlEventType.SelectedValue);
                    cmd.Parameters.AddWithValue("@StartDate", hasStart ? (object)startDate : DBNull.Value);
                    cmd.Parameters.AddWithValue("@EndDate", hasEnd ? (object)endDate : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Venue", string.IsNullOrWhiteSpace(txtVenue.Text) ? (object)DBNull.Value : txtVenue.Text.Trim());

                    decimal budget;
                    if (decimal.TryParse(txtBudget.Text.Trim(), out budget))
                        cmd.Parameters.AddWithValue("@Budget", budget);
                    else
                        cmd.Parameters.AddWithValue("@Budget", DBNull.Value);

                    cmd.Parameters.AddWithValue("@Organizer", string.IsNullOrWhiteSpace(lead.FullName) ? (object)DBNull.Value : lead.FullName);
                    cmd.Parameters.AddWithValue("@Visibility", ddlVisibility.SelectedValue);
                    cmd.Parameters.AddWithValue("@CreatedBy", creatorId);
                    cmd.Parameters.AddWithValue("@ClubID", clubId.HasValue ? (object)clubId.Value : DBNull.Value);
                    cmd.ExecuteNonQuery();

                    // Notify Admins about the new proposal
                    SqlCommand cmdNotif = new SqlCommand(@"
                        INSERT INTO Notifications (UserID, Title, Message, IsRead, NotificationType, CreatedAt)
                        SELECT u.UserID, @Title, @Message, 0, 'Event', GETDATE()
                        FROM Users u
                        INNER JOIN Roles r ON u.RoleID = r.RoleID
                        WHERE r.RoleName = 'Admin'", con);
                    cmdNotif.Parameters.AddWithValue("@Title", "New Event Proposal");
                    cmdNotif.Parameters.AddWithValue("@Message", $"A new event proposal '{txtEventName.Text.Trim()}' is awaiting your review.");
                    cmdNotif.ExecuteNonQuery();
                }

                ShowMessage("Your proposal has been submitted for admin review. You will be notified of the outcome.", true);
                ClearForm();
                LoadMyProposals();
            }
            catch (Exception ex)
            {
                ShowMessage("An error occurred while submitting your proposal: " + ex.Message, false);
            }
        }

        private void BindLeads()
        {
            ddlEventLead.Items.Clear();
            ddlEventLead.Items.Add(new ListItem("Select a verified faculty or staff member", "0"));
            DataTable leads = EventService.ListEventLeadCandidates();
            foreach (DataRow row in leads.Rows)
            {
                string label = Convert.ToString(row["FullName"]) + " - " + Convert.ToString(row["Email"]);
                ddlEventLead.Items.Add(new ListItem(label, Convert.ToString(row["UserID"])));
            }
        }

        private void ClearForm()
        {
            txtEventName.Text = string.Empty;
            txtDescription.Text = string.Empty;
            txtStartDate.Text = string.Empty;
            txtEndDate.Text = string.Empty;
            txtVenue.Text = string.Empty;
            txtBudget.Text = string.Empty;
            ddlEventType.SelectedIndex = 0;
            ddlVisibility.SelectedIndex = 0;
            ddlEventLead.SelectedIndex = 0;
        }

        private void ShowMessage(string message, bool isSuccess)
        {
            pnlMessage.Visible = true;
            pnlMessage.CssClass = isSuccess
                ? "mb-6 p-4 rounded-xl flex items-center gap-3 bg-tertiary-container/10 border border-tertiary-container text-on-tertiary-container"
                : "mb-6 p-4 rounded-xl flex items-center gap-3 bg-error-container border border-error text-error";
            lblMessage.Text = message;
        }
    }
}
