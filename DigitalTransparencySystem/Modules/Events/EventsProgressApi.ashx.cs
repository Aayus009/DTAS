using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.SessionState;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Events
{
    public class EventsProgressApi : IHttpHandler, IRequiresSessionState
    {
        public bool IsReusable { get { return false; } }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);

            if (context.Session == null || context.Session["UserID"] == null
                || !RoleAccess.IsAdmin(context.Session["Role"] as string))
            {
                Write(context, new { ok = false, error = "Admin sign in required." });
                return;
            }

            List<EventLiveProgress> rows = EventTaskService.ListLiveEventProgress();
            var events = new object[rows.Count];
            int planned = 0, inProgress = 0, completed = 0, proposed = 0;

            for (int i = 0; i < rows.Count; i++)
            {
                EventLiveProgress row = rows[i];
                string key = row.StatusKey ?? "";
                if (key.Equals("planned", StringComparison.OrdinalIgnoreCase))
                    planned++;
                else if (key.Equals("inprogress", StringComparison.OrdinalIgnoreCase))
                    inProgress++;
                else if (key.Equals("completed", StringComparison.OrdinalIgnoreCase))
                    completed++;
                else if (key.Equals("proposed", StringComparison.OrdinalIgnoreCase))
                    proposed++;

                events[i] = new
                {
                    eventId = row.EventId,
                    eventName = row.EventName,
                    percent = row.Percent,
                    total = row.TotalTasks,
                    completed = row.CompletedTasks,
                    label = EventTaskService.CompletionLabel(row.CompletedTasks, row.TotalTasks),
                    status = row.StatusKey,
                    statusLabel = EventTaskService.EventStatusDisplay(row.Status),
                    lifecycleIndex = EventTaskService.LifecycleIndex(row.Status)
                };
            }

            Write(context, new
            {
                ok = true,
                events,
                stats = new
                {
                    total = rows.Count,
                    planned,
                    inProgress,
                    completed,
                    proposed
                }
            });
        }

        private static void Write(HttpContext context, object payload)
        {
            context.Response.Write(new JavaScriptSerializer().Serialize(payload));
        }
    }
}
