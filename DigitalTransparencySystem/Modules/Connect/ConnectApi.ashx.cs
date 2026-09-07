using System;
using System.Data;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.SessionState;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Connect
{
    public class ConnectApi : IHttpHandler, IRequiresSessionState
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

            int userId = Convert.ToInt32(context.Session["UserID"]);
            string action = (context.Request["action"] ?? "messages").Trim().ToLowerInvariant();

            try
            {
                if (action == "messages")
                    ListMessages(context, userId);
                else if (action == "send")
                    Send(context, userId);
                else
                    Write(context, new { ok = false, error = "Unknown action." });
            }
            catch (Exception ex)
            {
                Write(context, new { ok = false, error = ex.Message });
            }
        }

        private static void ListMessages(HttpContext context, int userId)
        {
            int groupId, afterId, beforeId;
            if (!int.TryParse(context.Request["groupId"], out groupId))
            {
                Write(context, new { ok = false, error = "Missing group." });
                return;
            }
            int.TryParse(context.Request["afterId"], out afterId);
            int.TryParse(context.Request["beforeId"], out beforeId);

            ConnectAccess access = ConnectService.GetAccess(groupId, userId);
            if (!access.CanView)
            {
                Write(context, new { ok = false, error = "Access denied." });
                return;
            }

            DataTable table = ConnectService.ListMessages(groupId, afterId, beforeId);
            ConnectService.MarkRead(groupId, userId);
            var items = new object[table.Rows.Count];
            for (int i = 0; i < table.Rows.Count; i++)
            {
                DataRow row = table.Rows[i];
                string image = row["ImagePath"] == DBNull.Value ? null : Convert.ToString(row["ImagePath"]);
                items[i] = new
                {
                    id = Convert.ToInt32(row["MessageID"]),
                    senderId = Convert.ToInt32(row["SenderID"]),
                    name = Convert.ToString(row["FullName"]),
                    type = Convert.ToString(row["MessageType"]),
                    content = row["Content"] == DBNull.Value ? "" : Convert.ToString(row["Content"]),
                    image = string.IsNullOrEmpty(image) ? null : VirtualPathUtility.ToAbsolute(image),
                    at = Convert.ToDateTime(row["CreatedAt"]).ToString("MMM dd HH:mm")
                };
            }
            Write(context, new { ok = true, messages = items });
        }

        private static void Send(HttpContext context, int userId)
        {
            int groupId;
            if (!int.TryParse(context.Request["groupId"], out groupId))
            {
                Write(context, new { ok = false, error = "Missing group." });
                return;
            }

            string type = context.Request["type"];
            string content = context.Request["content"];
            string imagePath = null;
            HttpPostedFile file = context.Request.Files["image"];
            if (file != null && file.ContentLength > 0)
            {
                string error = ConnectService.SaveImage(file, context.Server, out imagePath);
                if (error != null)
                {
                    Write(context, new { ok = false, error = error });
                    return;
                }
            }

            int messageId;
            string sendError = ConnectService.SendMessage(groupId, userId, type, content, imagePath, out messageId);
            if (sendError != null)
            {
                Write(context, new { ok = false, error = sendError });
                return;
            }
            Write(context, new { ok = true, id = messageId });
        }

        private static void Write(HttpContext context, object payload)
        {
            context.Response.Write(new JavaScriptSerializer().Serialize(payload));
        }
    }
}
