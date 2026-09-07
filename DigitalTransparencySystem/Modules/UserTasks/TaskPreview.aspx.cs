using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.UserTasks
{
    public partial class TaskPreview : System.Web.UI.Page
    {
        private string connectionString;
        private int taskId;

        protected void Page_Load(object sender, EventArgs e)
        {
            connectionString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;

            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Modules/Authentication/Login.aspx");
                return;
            }

            if (Request.QueryString["TaskID"] == null || !int.TryParse(Request.QueryString["TaskID"], out taskId))
            {
                Response.Redirect("~/Modules/UserTasks/UserTasks.aspx");
                return;
            }

            if (!HasAccess() || RestrictionService.IsTaskLocked(taskId))
            {
                Response.Redirect("~/Modules/UserTasks/UserTasks.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadTaskDetails();
            }
        }

        private bool HasAccess()
        {
            string userId = Session["UserID"].ToString();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    @"SELECT COUNT(1) FROM Tasks t
                      WHERE t.TaskID = @TaskID AND " + TaskAccess.UserCanSeeTask, con);
                cmd.Parameters.AddWithValue("@TaskID", taskId);
                cmd.Parameters.AddWithValue("@UserID", userId);
                if (Convert.ToInt32(cmd.ExecuteScalar()) > 0)
                    return true;
            }
            return EventTaskService.IsAcceptedEventMember(taskId, Convert.ToInt32(userId));
        }

        private void LoadTaskDetails()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    @"SELECT t.TaskTitle, t.Description, t.Priority, t.Status, t.DueDate,
                             ISNULL(t.PublicSummary, 'No public summary provided yet.') AS PublicSummary,
                             ISNULL(e.EventName, 'General') AS EventName
                      FROM Tasks t
                      LEFT JOIN Events e ON t.EventID = e.EventID
                      WHERE t.TaskID = @TaskID", con);
                cmd.Parameters.AddWithValue("@TaskID", taskId);

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    litTaskTitle.Text = Server.HtmlEncode(reader["TaskTitle"].ToString());
                    litDescription.Text = Server.HtmlEncode(reader["Description"].ToString());
                    litPublicSummary.Text = Server.HtmlEncode(reader["PublicSummary"].ToString());
                    object dueDate = reader["DueDate"];
                    string status = reader["Status"].ToString();
                    litDueDate.Text = TimelineService.DueLabel(dueDate, status);
                    litEvent.Text = Server.HtmlEncode(reader["EventName"].ToString());

                    string priority = reader["Priority"].ToString();
                    spanPriority.InnerText = priority;
                    spanPriority.Attributes["class"] = "badge-status-" + priority.ToLower();

                    spanStatus.InnerText = TimelineService.StatusLabel(status, dueDate);
                    spanStatus.Attributes["class"] = TimelineService.StatusBadgeClass(status, dueDate);
                }
                else
                {
                    Response.Redirect("~/Modules/UserTasks/UserTasks.aspx");
                }
                reader.Close();
            }
        }
    }
}
