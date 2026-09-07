using System;
using System.Configuration;
using System.Net;
using System.Text;
using System.Web;
using System.Web.SessionState;
using Newtonsoft.Json.Linq;

namespace DigitalTransparencySystem.Helpers
{
    public class GoogleProfile
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public bool EmailVerified { get; set; }
    }

    public static class GoogleAuthService
    {
        public static string ClientId
        {
            get { return (ConfigurationManager.AppSettings["Google_ClientId"] ?? "").Trim(); }
        }

        public static string ClientSecret
        {
            get { return (ConfigurationManager.AppSettings["Google_ClientSecret"] ?? "").Trim(); }
        }

        public static bool IsConfigured
        {
            get { return ClientId.Length > 0 && ClientSecret.Length > 0; }
        }

        public static string CallbackPath
        {
            get { return VirtualPathUtility.ToAbsolute("~/Modules/Authentication/GoogleCallback.aspx"); }
        }

        public static string RedirectUri(HttpRequest request)
        {
            return request.Url.GetLeftPart(UriPartial.Authority) + CallbackPath;
        }

        public static string GetAuthorizeUrl(HttpContext context, bool embed, out string error)
        {
            error = null;
            if (!IsConfigured)
            {
                error = "Google sign-in is not set up yet. Add Google_ClientId and Google_ClientSecret in Web.config.";
                return null;
            }

            string nonce = Guid.NewGuid().ToString("N");
            context.Session["GoogleOAuthNonce"] = nonce;
            context.Session["GoogleOAuthEmbed"] = embed;

            var query = HttpUtility.ParseQueryString(string.Empty);
            query["client_id"] = ClientId;
            query["redirect_uri"] = RedirectUri(context.Request);
            query["response_type"] = "code";
            query["scope"] = "openid email profile";
            query["access_type"] = "online";
            query["include_granted_scopes"] = "true";
            query["prompt"] = "select_account";
            query["state"] = nonce;
            return "https://accounts.google.com/o/oauth2/v2/auth?" + query;
        }

        public static string ExchangeCode(HttpContext context, string code, string state, out GoogleProfile profile)
        {
            profile = null;
            string expected = context.Session["GoogleOAuthNonce"] as string;
            if (string.IsNullOrEmpty(expected) || !string.Equals(expected, state, StringComparison.Ordinal))
                return "Google sign-in expired. Please try again.";

            context.Session.Remove("GoogleOAuthNonce");
            EnsureTls();

            string tokenJson;
            try
            {
                using (var client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    var body = new StringBuilder();
                    body.Append("code=").Append(HttpUtility.UrlEncode(code));
                    body.Append("&client_id=").Append(HttpUtility.UrlEncode(ClientId));
                    body.Append("&client_secret=").Append(HttpUtility.UrlEncode(ClientSecret));
                    body.Append("&redirect_uri=").Append(HttpUtility.UrlEncode(RedirectUri(context.Request)));
                    body.Append("&grant_type=authorization_code");
                    tokenJson = client.UploadString("https://oauth2.googleapis.com/token", body.ToString());
                }
            }
            catch (WebException ex)
            {
                return "Google could not verify this sign-in. " + AuthService.FriendlyGoogleError(ReadWebError(ex));
            }

            JObject token = JObject.Parse(tokenJson);
            string accessToken = (string)token["access_token"];
            if (string.IsNullOrEmpty(accessToken))
                return "Google did not return an access token.";

            string infoJson;
            try
            {
                using (var client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.Authorization] = "Bearer " + accessToken;
                    infoJson = client.DownloadString("https://www.googleapis.com/oauth2/v2/userinfo");
                }
            }
            catch (WebException ex)
            {
                return "Could not read your Google profile. " + ReadWebError(ex);
            }

            JObject info = JObject.Parse(infoJson);
            string email = ((string)info["email"] ?? "").Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(email))
                return "Google did not share an email address.";
            if (!info.Value<bool>("verified_email"))
                return "That Google email is not verified.";

            profile = new GoogleProfile
            {
                Email = email,
                Name = string.IsNullOrWhiteSpace((string)info["name"]) ? email : ((string)info["name"]).Trim(),
                EmailVerified = true
            };
            return null;
        }

        public static string SignInOrPrepare(HttpSessionState session, HttpRequest request, GoogleProfile profile, out UserAccount user, out bool needsRole)
        {
            user = null;
            needsRole = false;
            if (AuthService.IsBanned(profile.Email, null))
                return "This email cannot be used to sign in.";

            user = AuthService.FindByEmail(profile.Email);
            if (user == null)
            {
                session["GooglePendingEmail"] = profile.Email;
                session["GooglePendingName"] = profile.Name;
                needsRole = true;
                return null;
            }

            string block = AuthService.AccountBlockReason(user);
            if (block != null)
                return block;

            if (!user.EmailVerified)
            {
                AuthService.MarkEmailVerified(user.UserID);
                user.EmailVerified = true;
            }

            AuthService.CompleteLogin(session, request, user);
            return null;
        }

        public static string FinishNewUser(HttpSessionState session, HttpRequest request, int roleId, out UserAccount user)
        {
            user = null;
            string email = session["GooglePendingEmail"] as string;
            string name = session["GooglePendingName"] as string;
            if (string.IsNullOrEmpty(email))
                return "Google sign-in expired. Please try again.";
            if (roleId < 2 || roleId > 4)
                return "Select a role.";
            if (AuthService.IsBanned(email, null))
                return "This email cannot be used to register.";
            if (AuthService.EmailExists(email))
                return "An account with this email already exists. Sign in instead.";

            int userId = AuthService.CreateGoogleUser(name, email, roleId);
            AuthService.WriteAudit(userId, "Register", "User", userId, "Registered with Google as " + AuthService.RoleName(roleId) + ".", request.UserHostAddress);
            session.Remove("GooglePendingEmail");
            session.Remove("GooglePendingName");
            user = AuthService.FindById(userId);
            AuthService.CompleteLogin(session, request, user);
            return null;
        }

        public static void StartFromPage(HttpContext context, bool embed, Action<string> showError)
        {
            string error;
            string url = GetAuthorizeUrl(context, embed, out error);
            if (error != null)
            {
                showError(error);
                return;
            }

            if (embed)
            {
                string script = "window.top.location.href=" + HttpUtility.JavaScriptStringEncode(url, true) + ";";
                context.Response.Clear();
                context.Response.ContentType = "text/html";
                context.Response.Write("<!DOCTYPE html><html><body><script>" + script + "</script></body></html>");
                context.Response.End();
                return;
            }

            context.Response.Redirect(url);
        }

        private static void EnsureTls()
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
        }

        private static string ReadWebError(WebException ex)
        {
            try
            {
                if (ex.Response == null)
                    return ex.Message;
                using (var stream = ex.Response.GetResponseStream())
                using (var reader = new System.IO.StreamReader(stream))
                {
                    string body = reader.ReadToEnd();
                    if (string.IsNullOrWhiteSpace(body))
                        return ex.Message;
                    return body.Length > 180 ? body.Substring(0, 180) : body;
                }
            }
            catch
            {
                return ex.Message;
            }
        }
    }
}
