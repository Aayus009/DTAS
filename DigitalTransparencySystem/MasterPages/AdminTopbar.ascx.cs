using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.MasterPages
{
    public partial class AdminTopbar : UserControl
    {
        private string connectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            AuthService.RememberHomePortal(Session, true);
            connectionString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;

            if (!IsPostBack)
            {
                if (Session["UserID"] == null)
                {
                    Response.Redirect("~/Modules/Authentication/Login.aspx");
                    return;
                }

                LoadUserInfo();
            }

            LoadNotifications();
        }

        private void LoadUserInfo()
        {
            string userId = Session["UserID"] as string;
            if (string.IsNullOrEmpty(userId)) return;

            string fullName = Session["FullName"] as string ?? "";
            string email = Session["Email"] as string ?? "";
            string role = Session["Role"] as string ?? "";

            if (string.IsNullOrEmpty(fullName))
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("SELECT FullName, Email FROM Users WHERE UserID = @UserID", con);
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        fullName = reader["FullName"].ToString();
                        email = reader["Email"].ToString();
                        Session["FullName"] = fullName;
                        Session["Email"] = email;
                    }
                    reader.Close();
                }
            }

            lblUserName.Text = fullName;
            lblUserEmail.Text = email;
            lblUserRole.Text = role;

            string initials = GetInitials(fullName);
            lblAvatarInitial.InnerText = initials;

            string profileImage = GetUserProfileImage(userId);
            if (!string.IsNullOrEmpty(profileImage))
            {
                imgUserAvatar.ImageUrl = profileImage;
                imgUserAvatar.Visible = true;
                lblAvatarInitial.Visible = false;
            }
            else
            {
                imgUserAvatar.Visible = false;
                lblAvatarInitial.Visible = true;
            }
        }

        private string GetUserProfileImage(string userId)
        {
            object cached = Session["ProfileImageUrl"];
            if (cached != null)
                return string.IsNullOrEmpty(cached.ToString()) ? null : cached.ToString();

            string imgPath = null;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("SELECT ProfileImage FROM Users WHERE UserID = @UserID", con);
                cmd.Parameters.AddWithValue("@UserID", userId);
                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    imgPath = result.ToString();
                    if (!string.IsNullOrEmpty(imgPath))
                    {
                        if (!imgPath.StartsWith("~") && !imgPath.StartsWith("/"))
                            imgPath = "~/Uploads/ProfileImages/" + imgPath;
                    }
                    else
                        imgPath = null;
                }
            }
            Session["ProfileImageUrl"] = imgPath ?? "";
            return imgPath;
        }

        private string GetInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return "?";
            string[] parts = fullName.Trim().Split(' ');
            if (parts.Length >= 2)
                return (parts[0][0].ToString() + parts[parts.Length - 1][0].ToString()).ToUpper();
            return parts[0][0].ToString().ToUpper();
        }

        private void LoadNotifications()
        {
            if (Session["UserID"] == null)
                return;

            int userId = Convert.ToInt32(Session["UserID"]);
            int unreadCount = NotificationService.CountUnread(userId);
            string badge = NotificationService.BadgeText(unreadCount);
            if (!string.IsNullOrEmpty(badge))
            {
                notifBadge.InnerText = badge;
                notifBadge.Visible = true;
                string css = notifBadge.Attributes["class"] ?? "";
                notifBadge.Attributes["class"] = css.Replace("hidden", "").Trim();
            }
            else
            {
                notifBadge.Visible = false;
            }

            notifList.Controls.Clear();
            notifList.Controls.Add(new Literal { Text = NotificationService.RenderDropdownHtml(userId, Server) });
        }

        protected void lnkMarkAllRead_Click(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
                return;
            NotificationService.MarkAllRead(Convert.ToInt32(Session["UserID"]));
            LoadNotifications();
        }
    }
}
