using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.SessionState;

namespace DigitalTransparencySystem.Helpers
{
    public class UserAccount
    {
        public int UserID { get; set; }
        public string FullName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int RoleID { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public bool EmailVerified { get; set; }
        public bool IdentityVerified { get; set; }
        public string VerificationStatus { get; set; }
        public string AccountStatus { get; set; }
        public string InstitutionalID { get; set; }
        public string RejectionReason { get; set; }
    }

    public class LoginAttemptResult
    {
        public bool Success { get; set; }
        public bool NeedsEmailOtp { get; set; }
        public string Error { get; set; }
        public string DevOtp { get; set; }
        public UserAccount User { get; set; }
    }

    public class SuspensionInfo
    {
        public string Reason { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int DaysRemaining
        {
            get
            {
                double days = (EndDate - DateTime.Now).TotalDays;
                if (days <= 0)
                    return 0;
                return (int)Math.Ceiling(days);
            }
        }

        public string BannerText
        {
            get
            {
                int days = DaysRemaining;
                string time = days <= 0
                    ? "a short time"
                    : days == 1
                        ? "1 day"
                        : days + " days";
                string reason = string.IsNullOrWhiteSpace(Reason) ? "" : " Reason: " + Reason.Trim();
                return "Your account is suspended until " + EndDate.ToString("MMM dd, yyyy h:mm tt")
                    + " (" + time + " remaining)." + reason
                    + " You can view the system, but you cannot use any features until the suspension ends.";
            }
        }
    }

    public static class AuthService
    {
        public static string ConnectionString
        {
            get { return ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString; }
        }

        public static string RoleName(int roleId)
        {
            switch (roleId)
            {
                case 1: return "Admin";
                case 2: return "Faculty";
                case 3: return "Student";
                case 4: return "Staff";
                default: return "User";
            }
        }

        public static UserAccount FindByLogin(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            using (var con = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT TOP 1 UserID, FullName, Username, Email, Password, RoleID, IsActive,
                         ISNULL(IsDeleted, 0) AS IsDeleted,
                         ISNULL(EmailVerified, 0) AS EmailVerified,
                         ISNULL(IdentityVerified, 0) AS IdentityVerified,
                         ISNULL(VerificationStatus, N'None') AS VerificationStatus,
                         ISNULL(AccountStatus, N'Active') AS AccountStatus,
                         InstitutionalID, RejectionReason
                  FROM Users
                  WHERE Username = @Input OR Email = @Input", con))
            {
                cmd.Parameters.AddWithValue("@Input", input.Trim());
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;
                    return ReadUser(reader);
                }
            }
        }

        public static UserAccount FindById(int userId)
        {
            using (var con = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT TOP 1 UserID, FullName, Username, Email, Password, RoleID, IsActive,
                         ISNULL(IsDeleted, 0) AS IsDeleted,
                         ISNULL(EmailVerified, 0) AS EmailVerified,
                         ISNULL(IdentityVerified, 0) AS IdentityVerified,
                         ISNULL(VerificationStatus, N'None') AS VerificationStatus,
                         ISNULL(AccountStatus, N'Active') AS AccountStatus,
                         InstitutionalID, RejectionReason
                  FROM Users
                  WHERE UserID = @UserID", con))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;
                    return ReadUser(reader);
                }
            }
        }

        public static UserAccount FindByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            using (var con = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT TOP 1 UserID, FullName, Username, Email, Password, RoleID, IsActive,
                         ISNULL(IsDeleted, 0) AS IsDeleted,
                         ISNULL(EmailVerified, 0) AS EmailVerified,
                         ISNULL(IdentityVerified, 0) AS IdentityVerified,
                         ISNULL(VerificationStatus, N'None') AS VerificationStatus,
                         ISNULL(AccountStatus, N'Active') AS AccountStatus,
                         InstitutionalID, RejectionReason
                  FROM Users
                  WHERE Email = @Email", con))
            {
                cmd.Parameters.AddWithValue("@Email", email.Trim().ToLowerInvariant());
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;
                    return ReadUser(reader);
                }
            }
        }

        public static bool EmailExists(string email)
        {
            return FindByEmail(email) != null;
        }

        public static bool IsBanned(string email, string institutionalId)
        {
            string normalizedEmail = (email ?? string.Empty).Trim().ToLowerInvariant();
            string id = (institutionalId ?? string.Empty).Trim();
            if (normalizedEmail.Length == 0 && id.Length == 0)
                return false;

            try
            {
                using (var con = new SqlConnection(ConnectionString))
                using (var cmd = new SqlCommand(
                    @"SELECT CASE WHEN EXISTS (
                            SELECT 1 FROM Bans
                            WHERE IsActive = 1
                              AND (
                                  (@Email <> N'' AND Email = @Email)
                                  OR (@InstitutionalID <> N'' AND InstitutionalID = @InstitutionalID)
                              )
                        ) THEN 1 ELSE 0 END", con))
                {
                    cmd.CommandTimeout = 8;
                    cmd.Parameters.AddWithValue("@Email", normalizedEmail);
                    cmd.Parameters.AddWithValue("@InstitutionalID", id);
                    con.Open();
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Debug.WriteLine("IsBanned: " + ex.Message);
                return false;
            }
        }

        public static SuspensionInfo GetActiveSuspension(int userId)
        {
            using (var con = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT TOP 1 Reason, StartDate, EndDate
                  FROM Suspensions
                  WHERE UserID = @UserID AND IsActive = 1 AND EndDate > GETDATE()
                  ORDER BY EndDate DESC", con))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;
                    return new SuspensionInfo
                    {
                        Reason = Convert.ToString(reader["Reason"]),
                        StartDate = Convert.ToDateTime(reader["StartDate"]),
                        EndDate = Convert.ToDateTime(reader["EndDate"])
                    };
                }
            }
        }

        public static void LiftExpiredSuspension(UserAccount user)
        {
            if (user == null || !string.Equals(user.AccountStatus, "Suspended", StringComparison.OrdinalIgnoreCase))
                return;

            using (var con = new SqlConnection(ConnectionString))
            {
                con.Open();
                var active = new SqlCommand(
                    @"SELECT COUNT(1) FROM Suspensions
                      WHERE UserID = @UserID AND IsActive = 1 AND EndDate > GETDATE()", con);
                active.Parameters.AddWithValue("@UserID", user.UserID);
                if (Convert.ToInt32(active.ExecuteScalar()) > 0)
                    return;

                var clear = new SqlCommand(
                    @"UPDATE Suspensions SET IsActive = 0 WHERE UserID = @UserID AND IsActive = 1;
                      UPDATE Users SET AccountStatus = N'Active' WHERE UserID = @UserID;", con);
                clear.Parameters.AddWithValue("@UserID", user.UserID);
                clear.ExecuteNonQuery();
                user.AccountStatus = "Active";
            }
        }

        public static string AccountBlockReason(UserAccount user)
        {
            return AccountBlockReason(user, true);
        }

        public static string AccountBlockReason(UserAccount user, bool checkGlobalBanList)
        {
            if (user == null)
                return "Invalid username or password.";

            LiftExpiredSuspension(user);

            if (string.Equals(user.AccountStatus, "Banned", StringComparison.OrdinalIgnoreCase)
                || (checkGlobalBanList && IsBanned(user.Email, user.InstitutionalID)))
                return "This account has been banned. You cannot sign in with this email.";

            if (user.IsDeleted || !user.IsActive)
                return "This account is no longer active.";

            return null;
        }

        public static LoginAttemptResult TryPasswordLogin(string input, string password)
        {
            var result = new LoginAttemptResult();
            UserAccount user = FindByLogin(input);
            if (user == null || !PasswordHasher.Verify(password, user.Password))
            {
                result.Error = "Invalid username or password.";
                return result;
            }

            string block = AccountBlockReason(user);
            if (block != null)
            {
                result.Error = block;
                return result;
            }

            if (PasswordHasher.NeedsUpgrade(user.Password))
                UpdatePassword(user.UserID, PasswordHasher.Hash(password));

            result.User = user;
            if (!user.EmailVerified)
            {
                OtpIssueResult otp = EmailOtpService.Issue(user.UserID, user.Email, EmailOtpService.PurposeRegister);
                result.NeedsEmailOtp = true;
                if (!string.IsNullOrEmpty(otp.Error) && string.IsNullOrEmpty(otp.Code))
                {
                    result.Error = otp.Error;
                    return result;
                }

                if (!otp.Sent)
                    result.DevOtp = otp.Code;
                return result;
            }

            result.Success = true;
            return result;
        }

        public static void CompleteLogin(HttpSessionState session, HttpRequest request, UserAccount user)
        {
            session["UserID"] = user.UserID.ToString();
            session["Username"] = user.Username;
            session["Role"] = user.Role;
            session["FullName"] = user.FullName;
            session["Email"] = user.Email;
            session["EmailVerified"] = user.EmailVerified;
            session["IdentityVerified"] = user.IdentityVerified;
            session["AccountStatus"] = user.AccountStatus;
            session["VerificationStatus"] = user.VerificationStatus;
            ApplySuspensionSession(session, user);

            using (var con = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(
                @"INSERT INTO LoginLogs (UserID, FullName, Username, LoginTime, IPAddress)
                  VALUES (@UserID, @FullName, @Username, GETDATE(), @IPAddress)", con))
            {
                cmd.Parameters.AddWithValue("@UserID", user.UserID);
                cmd.Parameters.AddWithValue("@FullName", user.FullName ?? "");
                cmd.Parameters.AddWithValue("@Username", user.Username ?? "");
                cmd.Parameters.AddWithValue("@IPAddress", request.UserHostAddress ?? "unknown");
                con.Open();
                cmd.ExecuteNonQuery();
            }

            WriteAudit(user.UserID, "Login", "User", user.UserID, "Signed in.", request.UserHostAddress);
        }

        public static void ApplySuspensionSession(HttpSessionState session, UserAccount user)
        {
            if (session == null || user == null)
                return;

            LiftExpiredSuspension(user);
            SuspensionInfo info = string.Equals(user.AccountStatus, "Suspended", StringComparison.OrdinalIgnoreCase)
                ? GetActiveSuspension(user.UserID)
                : null;

            bool viewOnly = info != null;
            session["IsSuspended"] = viewOnly;
            session["AccountStatus"] = user.AccountStatus;
            if (viewOnly)
            {
                session["SuspensionUntil"] = info.EndDate.ToString("o");
                session["SuspensionReason"] = info.Reason ?? "";
                session["SuspensionBanner"] = info.BannerText;
            }
            else
            {
                session.Remove("SuspensionUntil");
                session.Remove("SuspensionReason");
                session.Remove("SuspensionBanner");
            }
        }

        public static bool IsSuspendedViewOnly(HttpSessionState session)
        {
            if (session == null)
                return false;
            object flag = session["IsSuspended"];
            if (flag is bool)
                return (bool)flag;
            bool parsed;
            return flag != null && bool.TryParse(flag.ToString(), out parsed) && parsed;
        }

        public static string GetSuspensionBanner(HttpSessionState session)
        {
            return session == null ? null : session["SuspensionBanner"] as string;
        }

        public static string DashboardUrl(string role)
        {
            if (!string.IsNullOrEmpty(role) && role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                return "~/Modules/Dashboard/AdminDashboard.aspx";
            return "~/Modules/Dashboard/UsersDashboard.aspx";
        }

        public static void RememberHomePortal(HttpSessionState session, bool adminPortal)
        {
            if (session == null)
                return;
            session["HomePortal"] = adminPortal ? "admin" : "user";
        }

        public static string HomeDashboardUrl(HttpSessionState session)
        {
            string portal = session == null ? null : session["HomePortal"] as string;
            if (string.Equals(portal, "admin", StringComparison.OrdinalIgnoreCase))
                return "~/Modules/Dashboard/AdminDashboard.aspx";
            if (string.Equals(portal, "user", StringComparison.OrdinalIgnoreCase))
                return "~/Modules/Dashboard/UsersDashboard.aspx";
            return DashboardUrl(session == null ? null : session["Role"] as string);
        }

        public static string HomeDashboardLabel(HttpSessionState session)
        {
            string portal = session == null ? null : session["HomePortal"] as string;
            if (string.IsNullOrEmpty(portal) && session != null && RoleAccess.IsAdmin(session["Role"] as string))
                portal = "admin";
            if (string.Equals(portal, "admin", StringComparison.OrdinalIgnoreCase))
                return "Back to admin dashboard";
            return "Back to dashboard";
        }

        public static bool IsSafeLocalUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;
            url = url.Trim();
            return url.StartsWith("/", StringComparison.Ordinal) && !url.StartsWith("//", StringComparison.Ordinal);
        }

        public static void RememberReturnUrl(HttpSessionState session, string url)
        {
            if (session != null && IsSafeLocalUrl(url))
                session["AuthNext"] = url.Trim();
        }

        public static string ConsumeReturnUrl(HttpSessionState session, string fallback)
        {
            string next = session == null ? null : session["AuthNext"] as string;
            if (session != null)
                session.Remove("AuthNext");
            return IsSafeLocalUrl(next) ? next : fallback;
        }

        public static string FriendlyGoogleError(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return "Google sign-in failed. Try again, or sign in with email.";
            if (raw.IndexOf("invalid_client", StringComparison.OrdinalIgnoreCase) >= 0
                || raw.IndexOf("client secret is invalid", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Google sign-in is not configured. The client secret in Web.config does not match the OAuth client in Google Cloud Console. Create a new secret there and paste it into Google_ClientSecret.";
            if (raw.IndexOf("redirect_uri", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Google rejected this site’s callback URL. In Google Cloud Console add this exact Authorized redirect URI: the site origin plus /Modules/Authentication/GoogleCallback.aspx";
            if (raw.IndexOf("{", StringComparison.Ordinal) >= 0 || raw.Length > 160)
                return "Google could not complete this sign-in. Try again, or sign in with email.";
            return raw.Trim();
        }

        public static string PopupSignInUrl(string next, string reason, string googleError)
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["signin"] = "1";
            if (IsSafeLocalUrl(next))
                query["next"] = next.Trim();
            if (!string.IsNullOrWhiteSpace(reason))
                query["reason"] = reason.Trim();
            if (!string.IsNullOrWhiteSpace(googleError))
                query["google"] = FriendlyGoogleError(googleError);
            return "~/Default.aspx?" + query;
        }

        public static string PopupRegisterUrl()
        {
            return "~/Default.aspx?signup=1";
        }

        public static int CreateGoogleUser(string fullName, string email, int roleId)
        {
            string username = AllocateUsername((email ?? "user").Split('@')[0]);
            string hash = PasswordHasher.Hash(Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N"));
            string name = string.IsNullOrWhiteSpace(fullName) ? username : fullName.Trim();

            using (var con = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(
                @"INSERT INTO Users
                    (FullName, Email, Username, RoleID, Password, IsActive, CreatedAt,
                     EmailVerified, IdentityVerified, VerificationStatus, AccountStatus, IsDeleted)
                  VALUES
                    (@FullName, @Email, @Username, @RoleID, @Password, 1, GETDATE(),
                     1, 0, N'None', N'Active', 0);
                  SELECT CAST(SCOPE_IDENTITY() AS INT);", con))
            {
                cmd.Parameters.AddWithValue("@FullName", name);
                cmd.Parameters.AddWithValue("@Email", email.Trim().ToLowerInvariant());
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@RoleID", roleId);
                cmd.Parameters.AddWithValue("@Password", hash);
                con.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public static int CreateSelfRegisteredUser(string fullName, string email, string password, int roleId)
        {
            string username = AllocateUsername(email.Split('@')[0]);
            string hash = PasswordHasher.Hash(password);

            using (var con = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(
                @"INSERT INTO Users
                    (FullName, Email, Username, RoleID, Password, IsActive, CreatedAt,
                     EmailVerified, IdentityVerified, VerificationStatus, AccountStatus, IsDeleted)
                  VALUES
                    (@FullName, @Email, @Username, @RoleID, @Password, 1, GETDATE(),
                     0, 0, N'None', N'Active', 0);
                  SELECT CAST(SCOPE_IDENTITY() AS INT);", con))
            {
                cmd.Parameters.AddWithValue("@FullName", fullName);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@RoleID", roleId);
                cmd.Parameters.AddWithValue("@Password", hash);
                con.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public static void MarkEmailVerified(int userId)
        {
            using (var con = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(
                "UPDATE Users SET EmailVerified = 1 WHERE UserID = @UserID", con))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static void UpdatePassword(int userId, string hash)
        {
            using (var con = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(
                "UPDATE Users SET Password = @Password WHERE UserID = @UserID", con))
            {
                cmd.Parameters.AddWithValue("@Password", hash);
                cmd.Parameters.AddWithValue("@UserID", userId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static void ChangeEmail(int userId, string newEmail)
        {
            using (var con = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(
                @"UPDATE Users
                  SET Email = @Email, Username = CASE WHEN Username = Email THEN @Email ELSE Username END
                  WHERE UserID = @UserID", con))
            {
                cmd.Parameters.AddWithValue("@Email", newEmail);
                cmd.Parameters.AddWithValue("@UserID", userId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static string AllocateUsername(string preferred)
        {
            string baseName = string.IsNullOrWhiteSpace(preferred) ? "user" : preferred.Trim();
            if (baseName.Length > 80)
                baseName = baseName.Substring(0, 80);

            using (var con = new SqlConnection(ConnectionString))
            {
                con.Open();
                var exists = new SqlCommand("SELECT COUNT(1) FROM Users WHERE Username = @Username", con);
                exists.Parameters.AddWithValue("@Username", baseName);
                if (Convert.ToInt32(exists.ExecuteScalar()) == 0)
                    return baseName;

                string candidate = baseName + DateTime.UtcNow.ToString("mmssff");
                exists.Parameters["@Username"].Value = candidate;
                if (Convert.ToInt32(exists.ExecuteScalar()) == 0)
                    return candidate;

                return baseName + Guid.NewGuid().ToString("N").Substring(0, 6);
            }
        }

        public static void WriteAudit(int? userId, string action, string entityType, int? entityId, string description, string ip)
        {
            try
            {
                using (var con = new SqlConnection(ConnectionString))
                using (var cmd = new SqlCommand(
                    @"INSERT INTO AuditLogs (UserID, Action, EntityType, EntityID, Description, Timestamp, IPAddress)
                      VALUES (@UserID, @Action, @EntityType, @EntityID, @Description, GETDATE(), @IP)", con))
                {
                    cmd.Parameters.AddWithValue("@UserID", (object)userId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Action", action);
                    cmd.Parameters.AddWithValue("@EntityType", entityType);
                    cmd.Parameters.AddWithValue("@EntityID", (object)entityId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Description", (object)description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IP", (object)ip ?? DBNull.Value);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Audit write failed: " + ex.Message);
            }
        }

        private static UserAccount ReadUser(SqlDataReader reader)
        {
            int roleId = Convert.ToInt32(reader["RoleID"]);
            return new UserAccount
            {
                UserID = Convert.ToInt32(reader["UserID"]),
                FullName = reader["FullName"].ToString(),
                Username = reader["Username"].ToString(),
                Email = reader["Email"].ToString(),
                Password = reader["Password"].ToString(),
                RoleID = roleId,
                Role = RoleName(roleId),
                IsActive = Convert.ToBoolean(reader["IsActive"]),
                IsDeleted = Convert.ToBoolean(reader["IsDeleted"]),
                EmailVerified = Convert.ToBoolean(reader["EmailVerified"]),
                IdentityVerified = Convert.ToBoolean(reader["IdentityVerified"]),
                VerificationStatus = reader["VerificationStatus"].ToString(),
                AccountStatus = reader["AccountStatus"].ToString(),
                InstitutionalID = reader["InstitutionalID"] == DBNull.Value ? null : reader["InstitutionalID"].ToString(),
                RejectionReason = reader["RejectionReason"] == DBNull.Value ? null : reader["RejectionReason"].ToString()
            };
        }
    }
}
