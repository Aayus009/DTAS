using System;
using System.Configuration;
using System.Data.SqlClient;

namespace DigitalTransparencySystem.Modules.Authentication
{
    public partial class Logout : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] != null)
            {
                string connectionString =
                    ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;

                SqlConnection con = new SqlConnection(connectionString);

                string query = "UPDATE LoginLogs " +
                               "SET LogoutTime = GETDATE() " +
                               "WHERE LogID = (" +
                               "SELECT MAX(LogID) FROM LoginLogs " +
                               "WHERE UserID = @UserID AND LogoutTime IS NULL)";

                SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@UserID", Session["UserID"]);

                con.Open();

                cmd.ExecuteNonQuery();

                con.Close();
            }

            Session.Clear();

            Session.Abandon();

            Response.Redirect("~/Default.aspx");
        }
    }
}