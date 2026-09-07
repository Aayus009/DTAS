using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace DigitalTransparencySystem.Modules.UserFeedback
{
    public partial class UserFeedback : System.Web.UI.Page
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
                LoadMyFeedback();
                LoadFeedbackCount();
            }
        }

        private void LoadMyFeedback()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
                    @"SELECT * FROM Feedback 
                      WHERE UserID = @UserID 
                      ORDER BY CreatedAt DESC", con);
                cmd.Parameters.AddWithValue("@UserID", Session["UserID"]);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                rptMyFeedback.DataSource = dt;
                rptMyFeedback.DataBind();
                pnlNoFeedback.Visible = dt.Rows.Count == 0;
            }
        }

        private void LoadFeedbackCount()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Feedback WHERE UserID = @UserID", con);
                cmd.Parameters.AddWithValue("@UserID", Session["UserID"]);
                litFeedbackCount.Text = cmd.ExecuteScalar().ToString();
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text) || string.IsNullOrWhiteSpace(txtDescription.Text))
                return;

            int rating = 0;
            int.TryParse(hfRating.Value, out rating);

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    @"INSERT INTO Feedback (UserID, Category, Title, Description, Rating, Status)
                      VALUES (@UserID, @Category, @Title, @Description, @Rating, 'New')", con);
                cmd.Parameters.AddWithValue("@UserID", Session["UserID"]);
                cmd.Parameters.AddWithValue("@Category", ddlCategory.SelectedValue);
                cmd.Parameters.AddWithValue("@Title", txtTitle.Text.Trim());
                cmd.Parameters.AddWithValue("@Description", txtDescription.Text.Trim());
                cmd.Parameters.AddWithValue("@Rating", rating > 0 ? (object)rating : DBNull.Value);
                cmd.ExecuteNonQuery();
            }

            txtTitle.Text = "";
            txtDescription.Text = "";
            hfRating.Value = "0";
            ddlCategory.SelectedIndex = 0;

            lblSuccess.Visible = true;
            lblSuccess.CssClass = "text-tertiary font-label-md";

            LoadMyFeedback();
            LoadFeedbackCount();
        }
    }
}
