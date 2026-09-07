using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            SeedDefaultAdmin();
            try
            {
                RestrictionService.EnsureSchema();
                AssignmentService.EnsureSchema();
                ConnectService.EnsureSchema();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Restriction schema: " + ex.Message);
            }
        }

        protected void Application_PostAcquireRequestState(object sender, EventArgs e)
        {
            AccessGuard.Enforce(HttpContext.Current);
        }

        private void SeedDefaultAdmin()
        {
            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;
                using (SqlConnection con = new SqlConnection(connStr))
                {
                    con.Open();
                    SqlCommand checkCmd = new SqlCommand("SELECT COUNT(1) FROM Users WHERE Username = @Username", con);
                    checkCmd.Parameters.AddWithValue("@Username", "admin");
                    int exists = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (exists == 0)
                    {
                        string passwordHash = PasswordHasher.Hash("Admin@2026");
                        SqlCommand insertCmd = new SqlCommand(
                            @"INSERT INTO Users (FullName, Email, Username, RoleID, Password, IsActive, CreatedAt,
                                 EmailVerified, IdentityVerified, VerificationStatus, AccountStatus, IsDeleted)
                              VALUES (@FullName, @Email, @Username, 1, @Password, 1, GETDATE(),
                                 1, 1, N'Verified', N'Active', 0)", con);
                        insertCmd.Parameters.AddWithValue("@FullName", "System Admin");
                        insertCmd.Parameters.AddWithValue("@Email", "edu.digitaltransparency@gmail.com");
                        insertCmd.Parameters.AddWithValue("@Username", "admin");
                        insertCmd.Parameters.AddWithValue("@Password", passwordHash);
                        insertCmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Admin seed failed: {ex.Message}");
            }
        }

    }
}