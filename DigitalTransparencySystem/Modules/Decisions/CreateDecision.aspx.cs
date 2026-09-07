using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace DigitalTransparencySystem.Modules.Decisions
{
    public partial class CreateDecision : System.Web.UI.Page
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
            if (role == null || !role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("~/Modules/Authentication/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadUsers();
                LoadMeetings();
                LoadEvents();
            }
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

        protected void btnCreate_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    int newDecisionID = 0;

                    SqlCommand cmd = new SqlCommand(@"
                        INSERT INTO Decisions 
                            (DecisionTitle, Description, Priority, Status, ResponsibleUserID, MeetingID, EventID, DueDate, CreatedBy, CreatedAt, UpdatedAt)
                        VALUES 
                            (@DecisionTitle, @Description, @Priority, 'Proposed', @ResponsibleUserID, @MeetingID, @EventID, @DueDate, @CreatedBy, GETDATE(), GETDATE());
                        SELECT SCOPE_IDENTITY();", con);

                    cmd.Parameters.AddWithValue("@DecisionTitle", txtDecisionTitle.Text.Trim());
                    cmd.Parameters.AddWithValue("@Description", txtDescription.Text.Trim());
                    cmd.Parameters.AddWithValue("@Priority", ddlPriority.SelectedValue);
                    cmd.Parameters.AddWithValue("@ResponsibleUserID", int.Parse(ddlResponsiblePerson.SelectedValue));

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

                    cmd.Parameters.AddWithValue("@CreatedBy", Session["UserID"]);

                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        newDecisionID = Convert.ToInt32(result);
                    }

                    if (newDecisionID > 0)
                    {
                        SqlCommand cmdHistory = new SqlCommand(@"
                            INSERT INTO DecisionHistory 
                                (DecisionID, OldStatus, NewStatus, ChangedBy, Comments, ChangedAt)
                            VALUES 
                                (@DecisionID, NULL, 'Proposed', @ChangedBy, 'Decision created', GETDATE())", con);
                        cmdHistory.Parameters.AddWithValue("@DecisionID", newDecisionID);
                        cmdHistory.Parameters.AddWithValue("@ChangedBy", Session["UserID"]);
                        cmdHistory.ExecuteNonQuery();
                    }
                }

                Response.Redirect("~/Modules/Decisions/Decisions.aspx");
            }
            catch (Exception ex)
            {
                pnlError.Visible = true;
                lblError.Text = "An error occurred while creating the decision: " + ex.Message;
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Modules/Decisions/Decisions.aspx");
        }

    }
}
