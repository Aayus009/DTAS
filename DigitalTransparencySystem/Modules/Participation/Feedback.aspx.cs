using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DigitalTransparencySystem.Modules.Participation
{
    public partial class Feedback : System.Web.UI.Page
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"] == null || Session["Role"].ToString() != "Admin")
            {
                Response.Redirect("~/Modules/Authentication/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadStats();
                LoadFeedback();
            }
        }

        private void LoadStats()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Feedback WHERE Status = 'New'", conn))
                {
                    lblNewCount.Text = cmd.ExecuteScalar().ToString();
                }

                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Feedback WHERE Status = 'Under Review'", conn))
                {
                    lblUnderReviewCount.Text = cmd.ExecuteScalar().ToString();
                }

                using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Feedback WHERE Status = 'Resolved'", conn))
                {
                    lblResolvedCount.Text = cmd.ExecuteScalar().ToString();
                }
            }
        }

        private void LoadFeedback()
        {
            string query = @"SELECT f.FeedbackID, f.Title, f.Description, f.Category, f.Rating, f.Status,
                             f.AdminResponse, f.CreatedAt, u.FullName
                             FROM Feedback f
                             INNER JOIN Users u ON f.UserID = u.UserID
                             WHERE 1=1";

            string search = txtSearchFilter.Text.Trim();
            string category = ddlCategory.SelectedValue;
            string status = ddlStatus.SelectedValue;

            if (!string.IsNullOrEmpty(search))
            {
                query += " AND f.Title LIKE @Search";
            }

            if (category != "All")
            {
                query += " AND f.Category = @Category";
            }

            if (status != "All")
            {
                query += " AND f.Status = @Status";
            }

            query += " ORDER BY f.CreatedAt DESC";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(search))
                        cmd.Parameters.AddWithValue("@Search", "%" + search + "%");

                    if (category != "All")
                        cmd.Parameters.AddWithValue("@Category", category);

                    if (status != "All")
                        cmd.Parameters.AddWithValue("@Status", status);

                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    rptFeedback.DataSource = reader;
                    rptFeedback.DataBind();
                }
            }

            pnlNoFeedback.Visible = rptFeedback.Items.Count == 0;
        }

        protected void Filter_Changed(object sender, EventArgs e)
        {
            LoadFeedback();
        }

        protected void rptFeedback_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Review" || e.CommandName == "Resolve")
            {
                int feedbackID = Convert.ToInt32(e.CommandArgument);
                hfFeedbackID.Value = feedbackID.ToString();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT Title, Description, Status, AdminResponse FROM Feedback WHERE FeedbackID = @FeedbackID", conn))
                    {
                        cmd.Parameters.AddWithValue("@FeedbackID", feedbackID);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                lblModalTitle.Text = reader["Title"].ToString();
                                lblModalDescription.Text = reader["Description"].ToString();
                                ddlModalStatus.SelectedValue = reader["Status"].ToString();
                                txtAdminResponse.Text = reader["AdminResponse"] != DBNull.Value ? reader["AdminResponse"].ToString() : string.Empty;
                            }
                        }
                    }
                }

                if (e.CommandName == "Resolve")
                {
                    ddlModalStatus.SelectedValue = "Resolved";
                }

                pnlReviewModal.Visible = true;
            }
        }

        protected void btnUpdateFeedback_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(hfFeedbackID.Value)) return;

            int feedbackID = Convert.ToInt32(hfFeedbackID.Value);
            string newStatus = ddlModalStatus.SelectedValue;
            string adminResponse = txtAdminResponse.Text.Trim();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"UPDATE Feedback
                    SET Status = @Status, AdminResponse = @AdminResponse, UpdatedAt = GETDATE()
                    WHERE FeedbackID = @FeedbackID", conn))
                {
                    cmd.Parameters.AddWithValue("@Status", newStatus);
                    cmd.Parameters.AddWithValue("@AdminResponse", string.IsNullOrEmpty(adminResponse) ? (object)DBNull.Value : adminResponse);
                    cmd.Parameters.AddWithValue("@FeedbackID", feedbackID);
                    cmd.ExecuteNonQuery();
                }
            }

            pnlReviewModal.Visible = false;
            LoadStats();
            LoadFeedback();
        }

        protected void btnCloseModal_Click(object sender, EventArgs e)
        {
            pnlReviewModal.Visible = false;
        }

        public string GetStatusBadgeClass(string status)
        {
            switch (status)
            {
                case "New": return "px-2.5 py-1 rounded-full text-xs font-medium bg-blue-500/10 text-blue-600";
                case "Under Review": return "px-2.5 py-1 rounded-full text-xs font-medium bg-amber-500/10 text-amber-600";
                case "Resolved": return "px-2.5 py-1 rounded-full text-xs font-medium bg-green-500/10 text-green-600";
                default: return "px-2.5 py-1 rounded-full text-xs font-medium bg-gray-500/10 text-gray-600";
            }
        }

        public string GetCategoryBadgeClass(string category)
        {
            switch (category)
            {
                case "Suggestion": return "px-2.5 py-1 rounded-full text-xs font-medium bg-blue-500/10 text-blue-600";
                case "Complaint": return "px-2.5 py-1 rounded-full text-xs font-medium bg-red-500/10 text-red-600";
                case "Recommendation": return "px-2.5 py-1 rounded-full text-xs font-medium bg-green-500/10 text-green-600";
                default: return "px-2.5 py-1 rounded-full text-xs font-medium bg-gray-500/10 text-gray-600";
            }
        }

        public string GetInitials(string fullName)
        {
            if (string.IsNullOrEmpty(fullName)) return "?";
            string[] parts = fullName.Split(' ');
            if (parts.Length >= 2)
                return (parts[0][0] + parts[1][0]).ToString().ToUpper();
            return fullName.Substring(0, Math.Min(2, fullName.Length)).ToUpper();
        }

        public string GetStarRating(object ratingObj)
        {
            int rating = 0;
            if (ratingObj != DBNull.Value)
                rating = Convert.ToInt32(ratingObj);

            string stars = "";
            for (int i = 1; i <= 5; i++)
            {
                if (i <= rating)
                    stars += "<span class='material-symbols-outlined text-sm text-amber-400'>star</span>";
                else
                    stars += "<span class='material-symbols-outlined text-sm text-gray-300'>star</span>";
            }
            return stars;
        }
    }
}
