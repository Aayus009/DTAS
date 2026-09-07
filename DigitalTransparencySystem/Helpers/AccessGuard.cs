using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;

namespace DigitalTransparencySystem.Helpers
{
    public static class AccessGuard
    {
        private static readonly HashSet<string> PublicPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "",
            "/default",
            "/about",
            "/features",
            "/demo",
            "/faq",
            "/contact",
            "/events",
            "/eventdetails",
            "/loginpopup",
            "/registerpopup",
            "/modules/authentication/login",
            "/modules/authentication/register",
            "/modules/authentication/forgotpassword",
            "/modules/authentication/resetpassword",
            "/modules/authentication/verifyemail",
            "/modules/authentication/logout",
            "/modules/authentication/googlecallback",
            "/modules/authentication/googlecomplete"
        };

        private static readonly HashSet<string> IdentityExemptPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "/modules/dashboard/usersdashboard",
            "/modules/settings/userprofile",
            "/modules/settings/identityverification",
            "/modules/settings/submitreport",
            "/modules/notifications/notifications",
            "/modules/users/viewidentitydocument",
            "/modules/authentication/logout",
            "/modules/authentication/verifyemail"
        };

        public static void Enforce(HttpContext context)
        {
            if (context == null || context.Handler == null || !(context.Handler is Page))
                return;

            try { RestrictionService.EnsureSchema(); } catch { }
            try { AssignmentService.EnsureSchema(); } catch { }
            try { ConnectService.EnsureSchema(); } catch { }
            try { MeetingService.EnsureSchema(); } catch { }
            if (context.Session == null)
                return;

            string path = Normalize(context.Request.Path);
            bool isPublic = PublicPaths.Contains(path);

            object userIdObj = context.Session["UserID"];
            if (userIdObj == null)
            {
                if (!isPublic)
                    SendToSignIn(context, null);
                return;
            }

            int userId;
            if (!int.TryParse(userIdObj.ToString(), out userId))
            {
                context.Session.Clear();
                SendToSignIn(context, null);
                return;
            }

            UserAccount user = AuthService.FindById(userId);
            if (user == null)
            {
                context.Session.Clear();
                SendToSignIn(context, null);
                return;
            }

            string block = AuthService.AccountBlockReason(user);
            if (block != null)
            {
                context.Session.Clear();
                context.Session.Abandon();
                string reason = block.IndexOf("banned", StringComparison.OrdinalIgnoreCase) >= 0 ? "banned"
                    : "inactive";
                SendToSignIn(context, reason);
                return;
            }

            context.Session["Role"] = user.Role;
            context.Session["FullName"] = user.FullName;
            context.Session["Email"] = user.Email;
            context.Session["EmailVerified"] = user.EmailVerified;
            context.Session["IdentityVerified"] = user.IdentityVerified;
            context.Session["AccountStatus"] = user.AccountStatus;
            context.Session["VerificationStatus"] = user.VerificationStatus;
            AuthService.ApplySuspensionSession(context.Session, user);

            if (AuthService.IsSuspendedViewOnly(context.Session)
                && IsMutatingRequest(context)
                && !IsAllowedWhileSuspended(path))
            {
                context.Session["RestrictionNotice"] = "Your account is suspended. You can view pages but cannot use system features.";
                context.Response.Redirect(AuthService.DashboardUrl(user.Role), true);
                return;
            }

            if (!user.EmailVerified && path != "/modules/authentication/verifyemail"
                && path != "/modules/authentication/logout")
            {
                context.Response.Redirect("~/Modules/Authentication/VerifyEmail.aspx", true);
                return;
            }

            if (!user.IdentityVerified && user.RoleID != 1
                && !isPublic && !IdentityExemptPaths.Contains(path))
            {
                context.Response.Redirect("~/Modules/Settings/IdentityVerification.aspx", true);
            }
        }

        private static void SendToSignIn(HttpContext context, string reason)
        {
            string next = context.Request.RawUrl;
            if (Normalize(context.Request.Path) == "/transparency")
                next = "/Transparency.aspx";
            AuthService.RememberReturnUrl(context.Session, next);
            context.Response.Redirect(AuthService.PopupSignInUrl(next, reason, null), true);
        }

        public static bool RequireIdentityVerified(HttpContext context)
        {
            if (context == null || context.Session == null || context.Session["UserID"] == null)
                return false;

            bool verified = false;
            object flag = context.Session["IdentityVerified"];
            if (flag is bool)
                verified = (bool)flag;
            else if (flag != null)
                bool.TryParse(flag.ToString(), out verified);

            string role = context.Session["Role"] as string;
            if (RoleAccess.IsAdmin(role))
                return true;

            return verified;
        }

        private static bool IsMutatingRequest(HttpContext context)
        {
            string method = context.Request.HttpMethod ?? "";
            return method.Equals("POST", StringComparison.OrdinalIgnoreCase)
                || method.Equals("PUT", StringComparison.OrdinalIgnoreCase)
                || method.Equals("DELETE", StringComparison.OrdinalIgnoreCase)
                || method.Equals("PATCH", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsAllowedWhileSuspended(string path)
        {
            return path == "/modules/authentication/logout"
                || path == "/modules/authentication/verifyemail";
        }

        private static string Normalize(string path)
        {
            if (string.IsNullOrEmpty(path))
                return "";

            path = path.Replace("\\", "/").ToLowerInvariant();
            if (!path.StartsWith("/"))
                path = "/" + path;
            if (path.EndsWith("/"))
                path = path.TrimEnd('/');
            if (path.EndsWith(".aspx"))
                path = path.Substring(0, path.Length - 5);
            return path;
        }
    }
}
