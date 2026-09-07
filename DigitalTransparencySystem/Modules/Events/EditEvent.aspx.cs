using System;
using System.Configuration;
using System.Data.SqlClient;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Events
{
    public partial class EditEvent : System.Web.UI.Page
    {
        private string connectionString;
        private int eventID;

        protected void Page_Load(object sender, EventArgs e)
        {
            connectionString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;

            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Modules/Authentication/Login.aspx");
                return;
            }

            if (!int.TryParse(Request.QueryString["EventID"], out eventID))
            {
                Response.Redirect("~/Modules/Events/MyEvents.aspx");
                return;
            }

            if (!RestrictionService.CanEditEvent(eventID, Convert.ToInt32(Session["UserID"]), Session["Role"] as string))
            {
                Response.Redirect("~/Modules/Events/MyEvents.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadEvent();
            }
        }

        private void LoadEvent()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    @"SELECT EventName, Description, EventType, StartDate, EndDate,
                             Venue, Budget, Organizer, Status, ISNULL(Visibility, N'Private') AS Visibility
                      FROM Events WHERE EventID = @EventID", con);
                cmd.Parameters.AddWithValue("@EventID", eventID);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        txtEventName.Text = reader["EventName"].ToString();
                        txtDescription.Text = reader["Description"].ToString();
                        txtVenue.Text = reader["Venue"].ToString();
                        txtOrganizer.Text = reader["Organizer"].ToString();

                        if (ddlEventType.Items.FindByValue(reader["EventType"].ToString()) != null)
                            ddlEventType.SelectedValue = reader["EventType"].ToString();

                        if (ddlStatus.Items.FindByValue(reader["Status"].ToString()) != null)
                            ddlStatus.SelectedValue = reader["Status"].ToString();

                        if (reader["StartDate"] != DBNull.Value)
                            txtStartDate.Text = Convert.ToDateTime(reader["StartDate"]).ToString("yyyy-MM-ddTHH:mm");

                        if (reader["EndDate"] != DBNull.Value)
                            txtEndDate.Text = Convert.ToDateTime(reader["EndDate"]).ToString("yyyy-MM-ddTHH:mm");

                        if (reader["Budget"] != DBNull.Value && reader["Budget"] != null)
                            txtBudget.Text = Convert.ToDecimal(reader["Budget"]).ToString("0.00");

                        if (ddlVisibility.Items.FindByValue(reader["Visibility"].ToString()) != null)
                            ddlVisibility.SelectedValue = reader["Visibility"].ToString();
                    }
                    else
                    {
                        Response.Redirect("~/Modules/Events/Events.aspx");
                    }
                }
            }
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            DateTime startDate, endDate;
            if (!DateTime.TryParse(txtStartDate.Text, out startDate) ||
                !DateTime.TryParse(txtEndDate.Text, out endDate))
            {
                pnlError.Visible = true;
                lblError.Text = "Please enter valid start and end dates.";
                return;
            }

            if (endDate < startDate)
            {
                pnlError.Visible = true;
                lblError.Text = "End date must be on or after start date.";
                return;
            }

            if (string.Equals(ddlStatus.SelectedValue, "Completed", StringComparison.OrdinalIgnoreCase)
                && !EventTaskService.AllOpenTasksApproved(eventID))
            {
                pnlError.Visible = true;
                lblError.Text = "The event can be marked complete only after the event administrator approves every task.";
                return;
            }

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand(@"
                        UPDATE Events SET
                            EventName = @EventName,
                            Description = @Description,
                            EventType = @EventType,
                            StartDate = @StartDate,
                            EndDate = @EndDate,
                            Venue = @Venue,
                            Budget = @Budget,
                            Organizer = @Organizer,
                            Status = @Status,
                            Visibility = @Visibility,
                            UpdatedAt = GETDATE()
                        WHERE EventID = @EventID", con);

                    cmd.Parameters.AddWithValue("@EventID", eventID);
                    cmd.Parameters.AddWithValue("@EventName", txtEventName.Text.Trim());
                    cmd.Parameters.AddWithValue("@Description", (object)txtDescription.Text.Trim() ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@EventType", ddlEventType.SelectedValue);
                    cmd.Parameters.AddWithValue("@StartDate", startDate);
                    cmd.Parameters.AddWithValue("@EndDate", endDate);
                    cmd.Parameters.AddWithValue("@Venue", txtVenue.Text.Trim());

                    if (decimal.TryParse(txtBudget.Text.Trim(), out decimal budget))
                        cmd.Parameters.AddWithValue("@Budget", budget);
                    else
                        cmd.Parameters.AddWithValue("@Budget", DBNull.Value);

                    cmd.Parameters.AddWithValue("@Organizer", (object)txtOrganizer.Text.Trim() ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", ddlStatus.SelectedValue);
                    cmd.Parameters.AddWithValue("@Visibility", ddlVisibility.SelectedValue);

                    cmd.ExecuteNonQuery();
                }

                Response.Redirect("~/Modules/Events/EventWorkspace.aspx?EventID=" + eventID);
            }
            catch (Exception ex)
            {
                pnlError.Visible = true;
                lblError.Text = "An error occurred while updating the event: " + ex.Message;
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Modules/Events/EventWorkspace.aspx?EventID=" + eventID);
        }

    }
}
