using System;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.SessionState;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Search
{
    public class GlobalSearch : IHttpHandler, IRequiresSessionState
    {
        public bool IsReusable { get { return false; } }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);

            if (context.Session == null || context.Session["UserID"] == null)
            {
                Write(context, new { ok = false, error = "Sign in required." });
                return;
            }

            if (AuthService.IsSuspendedViewOnly(context.Session))
            {
                Write(context, new { ok = true, url = (string)null, error = "Search is not available while suspended." });
                return;
            }

            int userId = Convert.ToInt32(context.Session["UserID"]);
            string role = context.Session["Role"] as string;
            string query = (context.Request["q"] ?? "").Trim();

            try
            {
                SearchHit best = SearchService.BestMatch(userId, role, query);
                if (best == null || string.IsNullOrEmpty(best.Url))
                {
                    Write(context, new { ok = true, url = (string)null, error = "No matching page or record." });
                    return;
                }

                Write(context, new { ok = true, url = best.Url, title = best.Title });
            }
            catch (Exception)
            {
                Write(context, new { ok = false, error = "Search could not be completed." });
            }
        }

        private static void Write(HttpContext context, object payload)
        {
            context.Response.Write(new JavaScriptSerializer().Serialize(payload));
        }
    }
}
