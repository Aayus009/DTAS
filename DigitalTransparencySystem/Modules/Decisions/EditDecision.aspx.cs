using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Decisions
{
    public partial class EditDecision : System.Web.UI.Page
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

            if (!int.TryParse(Request.QueryString["DecisionID"], out decisionID))
            {
                Response.Redirect("~/Modules/UserDecisions/UserDecisions.aspx");
                return;
            }

            if (!RestrictionService.CanEditDecision(decisionID, Convert.ToInt32(Session["UserID"]), Session["Role"] as string))
            {
                Response.Redirect("~/Modules/UserDecisions/UserDecisions.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadDropdowns();
                LoadDecision();
            }
        }

        private void LoadDropdowns()
        {
            LoadUsers();
            LoadMeetings();
            LoadEvents();
        }

        private void LoadUsers()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("SELECT UserID, FullName FROM Users ORDER BY FullName", con);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                ddlResponsiblePerson.DataSource = dt;
                ddlResponsiblePerson.DataTextField = "FullName";
                ddlResponsiblePerson.DataValueField = "UserID";
                ddlResponsiblePerson.DataBind();
                ddlResponsiblePerson.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Select Person", ""));
            }
        }

        private void LoadMeetings()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("SELECT MeetingID, MeetingTitle FROM Meetings ORDER BY MeetingTitle", con);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                ddlRelatedMeeting.DataSource = dt;
                ddlRelatedMeeting.DataTextField = "MeetingTitle";
                ddlRelatedMeeting.DataValueField = "MeetingID";
                ddlRelatedMeeting.DataBind();
                ddlRelatedMeeting.Items.Insert(0, new System.Web.UI.WebControls.ListItem("None", ""));
            }
        }

        private void LoadEvents()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("SELECT EventID, EventName FROM Events ORDER BY EventName", con);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                ddlRelatedEvent.DataSource = dt;
                ddlRelatedEvent.DataTextField = "EventName";
                ddlRelatedEvent.DataValueField = "EventID";
                ddlRelatedEvent.DataBind();
                ddlRelatedEvent.Items.Insert(0, new System.Web.UI.WebControls.ListItem("None", ""));
            }
        }

        private void LoadDecision()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    @"SELECT d.DecisionTitle, d.Description, d.Priority, d.Status, 
                             d.ResponsibleUserID, d.MeetingID, d.EventID, d.DueDate
                      FROM Decisions d
                      WHERE d.DecisionID = @DecisionID", con);
                cmd.Parameters.AddWithValue("@DecisionID", decisionID);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        txtDecisionTitle.Text = reader["DecisionTitle"].ToString();
                        txtDescription.Text = reader["Description"].ToString();

                        string priority = reader["Priority"].ToString();
                        if (ddlPriority.Items.FindByValue(priority) != null)
                            ddlPriority.SelectedValue = priority;

                        string status = reader["Status"].ToString();
                        if (ddlStatus.Items.FindByValue(status) != null)
                            ddlStatus.SelectedValue = status;

                        if (reader["ResponsibleUserID"] != DBNull.Value)
                        {
                            string userId = reader["ResponsibleUserID"].ToString();
                            if (ddlResponsiblePerson.Items.FindByValue(userId) != null)
                                ddlResponsiblePerson.SelectedValue = userId;
                        }

                        if (reader["MeetingID"] != DBNull.Value)
                        {
                            string meetingId = reader["MeetingID"].ToString();
                            if (ddlRelatedMeeting.Items.FindByValue(meetingId) != null)
                                ddlRelatedMeeting.SelectedValue = meetingId;
                        }

                        if (reader["EventID"] != DBNull.Value)
                        {
                            string eventId = reader["EventID"].ToString();
                            if (ddlRelatedEvent.Items.FindByValue(eventId) != null)
                                ddlRelatedEvent.SelectedValue = eventId;
                        }

                        if (reader["DueDate"] != DBNull.Value)
                        {
                            DateTime dueDate = Convert.ToDateTime(reader["DueDate"]);
                            txtDueDate.Text = dueDate.ToString("yyyy-MM-ddTHH:mm");
                        }
                    }
                    else
                    {
                        Response.Redirect("~/Modules/UserDecisions/UserDecisions.aspx");
                    }
                }
            }
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            try
            {
                string oldStatus = GetCurrentStatus();
                string newStatus = ddlStatus.SelectedValue;
                bool statusChanged = !oldStatus.Equals(newStatus, StringComparison.OrdinalIgnoreCase);

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    SqlCommand cmd = new SqlCommand(@"
                        UPDATE Decisions SET
                            DecisionTitle = @DecisionTitle,
                            Description = @Description,
                            Priority = @Priority,
                            Status = @Status,
                            ResponsibleUserID = @ResponsibleUserID,
                            MeetingID = @MeetingID,
                            EventID = @EventID,
                            DueDate = @DueDate,
                            UpdatedAt = GETDATE()
                        WHERE DecisionID = @DecisionID", con);

                    cmd.Parameters.AddWithValue("@DecisionTitle", txtDecisionTitle.Text.Trim());
                    cmd.Parameters.AddWithValue("@Description", txtDescription.Text.Trim());
                    cmd.Parameters.AddWithValue("@Priority", ddlPriority.SelectedValue);
                    cmd.Parameters.AddWithValue("@Status", newStatus);
                    cmd.Parameters.AddWithValue("@ResponsibleUserID", int.Parse(ddlResponsiblePerson.SelectedValue));
                    cmd.Parameters.AddWithValue("@DecisionID", decisionID);

                    if (!string.IsNullOrEmpty(ddlRelatedMeeting.SelectedValue))
                        cmd.Parameters.AddWithValue("@MeetingID", int.Parse(ddlRelatedMeeting.SelectedValue));
                    else
                        cmd.Parameters.AddWithValue("@MeetingID", DBNull.Value);

                    if (!string.IsNullOrEmpty(ddlRelatedEvent.SelectedValue))
                        cmd.Parameters.AddWithValue("@EventID", int.Parse(ddlRelatedEvent.SelectedValue));
                    else
                        cmd.Parameters.AddWithValue("@EventID", DBNull.Value);

                    if (!string.IsNullOrEmpty(txtDueDate.Text))
                        cmd.Parameters.AddWithValue("@DueDate", DateTime.Parse(txtDueDate.Text));
                    else
                        cmd.Parameters.AddWithValue("@DueDate", DBNull.Value);

                    cmd.ExecuteNonQuery();

                    if (statusChanged)
                    {
                        SqlCommand cmdHistory = new SqlCommand(@"
                            INSERT INTO DecisionHistory 
                                (DecisionID, OldStatus, NewStatus, ChangedBy, Comments, ChangedAt)
                            VALUES 
                                (@DecisionID, @OldStatus, @NewStatus, @ChangedBy, @Comments, GETDATE())", con);
                        cmdHistory.Parameters.AddWithValue("@DecisionID", decisionID);
                        cmdHistory.Parameters.AddWithValue("@OldStatus", oldStatus);
                        cmdHistory.Parameters.AddWithValue("@NewStatus", newStatus);
                        cmdHistory.Parameters.AddWithValue("@ChangedBy", Session["UserID"]);
                        cmdHistory.Parameters.AddWithValue("@Comments", $"Decision updated. Status changed from {GetStatusDisplayText(oldStatus)} to {GetStatusDisplayText(newStatus)}.");
                        cmdHistory.ExecuteNonQuery();
                    }
                    else
                    {
                        SqlCommand cmdHistory = new SqlCommand(@"
                            INSERT INTO DecisionHistory 
                                (DecisionID, OldStatus, NewStatus, ChangedBy, Comments, ChangedAt)
                            VALUES 
                                (@DecisionID, @OldStatus, @NewStatus, @ChangedBy, @Comments, GETDATE())", con);
                        cmdHistory.Parameters.AddWithValue("@DecisionID", decisionID);
                        cmdHistory.Parameters.AddWithValue("@OldStatus", oldStatus);
                        cmdHistory.Parameters.AddWithValue("@NewStatus", oldStatus);
                        cmdHistory.Parameters.AddWithValue("@ChangedBy", Session["UserID"]);
                        cmdHistory.Parameters.AddWithValue("@Comments", "Decision details updated.");
                        cmdHistory.ExecuteNonQuery();
                    }
                }

                pnlSuccess.Visible = true;
                lblSuccess.Text = "Decision updated successfully!";
                pnlError.Visible = false;
            }
            catch (Exception ex)
            {
                pnlError.Visible = true;
                lblError.Text = "An error occurred while updating the decision: " + ex.Message;
                pnlSuccess.Visible = false;
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

        private string GetStatusDisplayText(string status)
        {
            switch (status)
            {
                case "Proposed": return "Proposed";
                case "UnderReview": return "Under Review";
                case "Approved": return "Approved";
                case "Rejected": return "Rejected";
                case "Completed": return "Completed";
                default: return status;
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Modules/UserDecisions/UserDecisions.aspx");
        }

    }
}
