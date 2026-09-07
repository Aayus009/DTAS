using System.Web;
using System.Web.Script.Serialization;
using System.Web.SessionState;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Transparency
{
    public class TransparencySync : IHttpHandler, IRequiresSessionState
    {
        public bool IsReusable { get { return false; } }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);

            if (context.Session == null || context.Session["UserID"] == null
                || !RoleAccess.IsAdmin(context.Session["Role"] as string))
            {
                context.Response.Write(new JavaScriptSerializer().Serialize(new { ok = false, error = "Admin sign in required." }));
                return;
            }

            context.Response.Write(new JavaScriptSerializer().Serialize(TransparencyService.LoadSnapshot()));
        }
    }
}
