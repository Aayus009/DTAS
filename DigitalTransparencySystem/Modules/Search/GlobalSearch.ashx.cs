using System;
using System.Collections.Generic;
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
                Write(context, new { ok = true, items = new object[0] });
                return;
            }

            int userId = Convert.ToInt32(context.Session["UserID"]);
            string role = context.Session["Role"] as string;
            string query = (context.Request["q"] ?? "").Trim();

            try
            {
                List<SearchHit> hits = SearchService.Search(userId, role, query);
                var items = new object[hits.Count];
                for (int i = 0; i < hits.Count; i++)
                {
                    SearchHit hit = hits[i];
                    items[i] = new
                    {
                        type = hit.Type,
                        title = hit.Title,
                        subtitle = hit.Subtitle,
                        url = hit.Url,
                        icon = hit.Icon
                    };
                }

                Write(context, new { ok = true, items });
            }
            catch (Exception)
            {
                Write(context, new { ok = false, error = "Search could not be completed.", items = new object[0] });
            }
        }

        private static void Write(HttpContext context, object payload)
        {
            context.Response.Write(new JavaScriptSerializer().Serialize(payload));
        }
    }
}
