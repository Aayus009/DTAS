using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using DigitalTransparencySystem.Helpers;
using Newtonsoft.Json;

namespace DigitalTransparencySystem.Modules.Assignments
{
    public partial class ContributionReport : System.Web.UI.Page
    {
        private int groupId;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || !int.TryParse(Request.QueryString["GroupID"], out groupId))
            {
                Response.Redirect("~/Modules/Assignments/MyAssignments.aspx");
                return;
            }

            adminTop.Visible = false;
            adminSide.Visible = false;
            userTop.Visible = true;
            userSide.Visible = true;

            AssignmentAccess access = AssignmentService.GetAccess(groupId, Convert.ToInt32(Session["UserID"]), Session["Role"] as string);
            if (!access.CanView)
            {
                pnlReport.Visible = false;
                pnlDenied.Visible = true;
                return;
            }

            if (!IsPostBack)
                BindReport(access);
        }

        public string ActivityLabel(object value)
        {
            if (value == null || value == DBNull.Value)
                return "No recorded activity";
            DateTime when = Convert.ToDateTime(value);
            return when.ToString("MMM dd, yyyy HH:mm");
        }

        private void BindReport(AssignmentAccess access)
        {
            AssignmentGroupRecord group = access.Group;
            AssignmentRecord assignment = group.Assignment;
            UserAccount leader = AuthService.FindById(group.LeaderID);
            string assignmentName = assignment == null ? group.GroupName : assignment.AssignmentName;

            litAssignment.Text = Server.HtmlEncode(assignmentName);
            litMeta.Text = Server.HtmlEncode(
                "Group " + group.GroupName
                + " - Leader " + (leader == null ? "-" : leader.FullName)
                + " - Deadline " + (assignment == null ? "-" : assignment.Deadline.ToString("MMM dd, yyyy"))
                + " - " + (group.IsFinalized ? "Finalized" : "Open")
                + (assignment != null && assignment.Deadline.Date < DateTime.Today ? " - Past deadline" : ""));
            litGenerated.Text = DateTime.Now.ToString("MMM dd, yyyy HH:mm");
            hidDownloadName.Value = SafeFileName("DTAS-Contribution-" + assignmentName + "-" + group.GroupName);
            lnkWorkspace.NavigateUrl = "~/Modules/Assignments/GroupWorkspace.aspx?GroupID=" + groupId;

            DataSet progress = AssignmentService.GetProgress(groupId);
            if (progress.Tables.Count > 0 && progress.Tables[0].Rows.Count > 0)
            {
                DataRow row = progress.Tables[0].Rows[0];
                object pct = progress.Tables[0].Columns.Contains("CompletionPercentage")
                    ? row["CompletionPercentage"]
                    : (progress.Tables[0].Columns.Contains("ProgressPercentage") ? row["ProgressPercentage"] : null);
                litGroupProgress.Text = AssignmentService.FormatPercent(pct);
            }
            else
            {
                litGroupProgress.Text = "0%";
            }

            DataTable members = AssignmentService.GetContribution(groupId);
            AddBreakdownColumns(members);
            rptMembers.DataSource = members;
            rptMembers.DataBind();
            rptBreakdown.DataSource = members;
            rptBreakdown.DataBind();

            DataTable tasks = AssignmentService.ListTasksForReport(groupId);
            rptTasks.DataSource = tasks;
            rptTasks.DataBind();
            pnlNoTasks.Visible = tasks.Rows.Count == 0;

            DataTable files = AssignmentService.ListReportFiles(groupId);
            rptFiles.DataSource = files;
            rptFiles.DataBind();
            pnlNoFiles.Visible = files.Rows.Count == 0;

            DataTable links = AssignmentService.ListReportLinks(groupId);
            rptLinks.DataSource = links;
            rptLinks.DataBind();
            pnlNoLinks.Visible = links.Rows.Count == 0;

            hidChartJson.Value = BuildChartJson(members, tasks);
        }

        private static void AddBreakdownColumns(DataTable table)
        {
            if (!table.Columns.Contains("CompletionPts"))
                table.Columns.Add("CompletionPts", typeof(decimal));
            if (!table.Columns.Contains("TimelinessPts"))
                table.Columns.Add("TimelinessPts", typeof(decimal));
            if (!table.Columns.Contains("ActivityPts"))
                table.Columns.Add("ActivityPts", typeof(decimal));
            if (!table.Columns.Contains("RecencyPts"))
                table.Columns.Add("RecencyPts", typeof(decimal));

            foreach (DataRow row in table.Rows)
            {
                decimal assigned = ToDec(row["AssignedTasks"]);
                decimal completed = ToDec(row["CompletedTasks"]);
                decimal overdue = ToDec(row["OverdueTasks"]);
                decimal updates = ToDec(row["ProgressUpdates"]);
                DateTime? last = row["LastActivity"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["LastActivity"]);

                decimal completion = 0;
                decimal timeliness = 0;
                decimal activity = 0;
                decimal recency = 0;
                if (assigned > 0)
                {
                    completion = Math.Round(50m * completed / assigned, 2);
                    timeliness = Math.Round(20m * (1m - overdue / assigned), 2);
                    decimal activityRaw = updates * 20m;
                    if (activityRaw > 100m)
                        activityRaw = 100m;
                    activity = Math.Round(20m * activityRaw / 100m, 2);
                    if (last.HasValue && last.Value >= DateTime.Now.AddDays(-14))
                        recency = 10m;
                }

                row["CompletionPts"] = completion;
                row["TimelinessPts"] = timeliness;
                row["ActivityPts"] = activity;
                row["RecencyPts"] = recency;
            }
        }

        private static string BuildChartJson(DataTable members, DataTable tasks)
        {
            var names = new List<string>();
            var scores = new List<decimal>();
            var completion = new List<decimal>();
            var timeliness = new List<decimal>();
            var activity = new List<decimal>();
            var recency = new List<decimal>();
            var assigned = new List<decimal>();
            var completed = new List<decimal>();

            foreach (DataRow row in members.Rows)
            {
                names.Add(Convert.ToString(row["FullName"]));
                scores.Add(ToDec(row["ContributionScore"]));
                completion.Add(ToDec(row["CompletionPts"]));
                timeliness.Add(ToDec(row["TimelinessPts"]));
                activity.Add(ToDec(row["ActivityPts"]));
                recency.Add(ToDec(row["RecencyPts"]));
                assigned.Add(ToDec(row["AssignedTasks"]));
                completed.Add(ToDec(row["CompletedTasks"]));
            }

            int todo = 0, progress = 0, review = 0, done = 0, blocked = 0;
            foreach (DataRow row in tasks.Rows)
            {
                switch (Convert.ToString(row["Status"]))
                {
                    case "InProgress": progress++; break;
                    case "UnderReview": review++; break;
                    case "Completed": done++; break;
                    case "Blocked": blocked++; break;
                    default: todo++; break;
                }
            }

            return JsonConvert.SerializeObject(new
            {
                names,
                scores,
                completion,
                timeliness,
                activity,
                recency,
                assigned,
                completed,
                statusLabels = new[] { "To Do", "In Progress", "Under Review", "Completed", "Blocked" },
                statusCounts = new[] { todo, progress, review, done, blocked }
            });
        }

        private static decimal ToDec(object value)
        {
            if (value == null || value == DBNull.Value)
                return 0;
            return Convert.ToDecimal(value);
        }

        private static string SafeFileName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "DTAS-Contribution-Report";
            string cleaned = Regex.Replace(value, @"[^\w\-. ]+", "-");
            cleaned = Regex.Replace(cleaned, @"\s+", "-").Trim('-');
            if (cleaned.Length > 80)
                cleaned = cleaned.Substring(0, 80);
            return string.IsNullOrEmpty(cleaned) ? "DTAS-Contribution-Report" : cleaned;
        }
    }
}
