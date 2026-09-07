using System;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.SessionState;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Tasks
{
    public class TaskProgressApi : IHttpHandler, IRequiresSessionState
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
            string role = context.Session["Role"] as string;
            int taskId;
            if (!int.TryParse(context.Request["taskId"], out taskId))
            {
                Write(context, new { ok = false, error = "Missing task." });
                return;
            }

            if (!EventTaskService.CanViewTask(taskId, userId, role))
            {
                Write(context, new { ok = false, error = "Access denied." });
                return;
            }

            int? eventId = EventTaskService.GetEventId(taskId);
            if (!eventId.HasValue)
            {
                Write(context, new { ok = false, error = "This task is not linked to an event." });
                return;
            }

            EventTeamProgress progress = EventTaskService.GetEventTeamProgress(eventId.Value);
            var members = new object[progress.Members.Count];
            for (int i = 0; i < progress.Members.Count; i++)
            {
                EventMemberProgress member = progress.Members[i];
                members[i] = new
                {
                    userId = member.UserId,
                    name = member.FullName,
                    assigned = member.AssignedTasks,
                    completed = member.CompletedTasks,
                    percent = member.Percent
                };
            }

            Write(context, new
            {
                ok = true,
                eventId = progress.EventId,
                eventName = progress.EventName,
                total = progress.TotalTasks,
                completed = progress.CompletedTasks,
                percent = progress.Percent,
                members
            });
        }

        private static void Write(HttpContext context, object payload)
        {
            context.Response.Write(new JavaScriptSerializer().Serialize(payload));
        }
    }
}
