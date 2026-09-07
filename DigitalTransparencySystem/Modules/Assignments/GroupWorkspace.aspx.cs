using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Assignments
{
    public partial class GroupWorkspace : System.Web.UI.Page
    {
        private int groupId;
        private AssignmentAccess access;
        private DataTable memberUpdates;
        private DataTable taskFiles;
        private DataTable taskLinks;

        public bool CanCorrect
        {
            get { return access != null && access.CanCorrect; }
        }

        public bool CanAssignLeader
        {
            get { return access != null && access.CanAssignLeader; }
        }

        public string FileIcon(object fileName)
        {
            string ext = Path.GetExtension(Convert.ToString(fileName) ?? "").ToLowerInvariant();
            switch (ext)
            {
                case ".pdf": return "picture_as_pdf";
                case ".png":
                case ".jpg":
                case ".jpeg": return "image";
                case ".zip": return "folder_zip";
                case ".doc":
                case ".docx": return "description";
                case ".xls":
                case ".xlsx": return "table";
                case ".ppt":
                case ".pptx": return "slideshow";
                case ".txt": return "notes";
                default: return "draft";
            }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            HtmlForm form = Master.FindControl("form1") as HtmlForm;
            if (form != null)
                form.Enctype = "multipart/form-data";
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || !int.TryParse(Request.QueryString["GroupID"], out groupId))
            {
                Response.Redirect("~/Modules/Assignments/MyAssignments.aspx");
                return;
            }

            access = AssignmentService.GetAccess(groupId, Convert.ToInt32(Session["UserID"]), Session["Role"] as string);
            ApplyChrome();

            if (!access.CanView)
            {
                pnlWorkspace.Visible = false;
                pnlDenied.Visible = true;
                return;
            }

            if (!IsPostBack)
                BindAll();
        }

        protected void btnFacultyExtend_Click(object sender, EventArgs e)
        {
            if (access == null || access.Group == null || !access.CanCorrect)
            {
                Show("Only faculty can extend the assignment deadline.", null);
                BindAll();
                return;
            }
            DateTime deadline;
            if (!DateTime.TryParse(txtFacultyDeadline.Text, out deadline))
            {
                Show("Enter a valid deadline.", null);
                BindAll();
                return;
            }
            Show(AssignmentService.UpdateDeadline(access.Group.AssignmentID, Convert.ToInt32(Session["UserID"]), Session["Role"] as string, deadline),
                "Deadline extended. Group members were notified.");
            Reload();
        }

        protected void btnFinalize_Click(object sender, EventArgs e)
        {
            Show(AssignmentService.Finalize(groupId, Convert.ToInt32(Session["UserID"]), Session["Role"] as string), "Group finalized.");
            Reload();
        }

        protected void btnLookup_Click(object sender, EventArgs e)
        {
            EmailLookupResult result = EventService.LookupByExactEmail(txtInviteEmail.Text);
            lblLookup.CssClass = result.Found ? "font-label-md block mt-3 text-tertiary" : "font-label-md block mt-3 text-error";
            lblLookup.Text = result.Found
                ? result.Message + " " + result.FullName + " (" + result.Role + ")."
                : result.Message;
            BindAll();
        }

        protected void btnInvite_Click(object sender, EventArgs e)
        {
            Show(AssignmentService.InviteMember(groupId, Convert.ToInt32(Session["UserID"]), Session["Role"] as string, txtInviteEmail.Text), "Invitation sent.");
            Reload();
        }

        protected void btnAddTask_Click(object sender, EventArgs e)
        {
            int parent;
            int? parentId = int.TryParse(ddlParent.SelectedValue, out parent) && parent > 0 ? parent : (int?)null;
            int assignee;
            int? assigneeId = int.TryParse(ddlAssignee.SelectedValue, out assignee) && assignee > 0 ? assignee : (int?)null;
            DateTime due;
            DateTime? dueDate = DateTime.TryParse(txtDue.Text, out due) ? due : (DateTime?)null;

            Show(AssignmentService.AddTask(
                groupId,
                Convert.ToInt32(Session["UserID"]),
                Session["Role"] as string,
                txtTaskTitle.Text,
                null,
                parentId,
                assigneeId,
                dueDate), "Task added.");
            if (lblMessage.CssClass.IndexOf("error", StringComparison.OrdinalIgnoreCase) < 0)
                txtTaskTitle.Text = "";
            Reload();
        }

        protected void rptTasks_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int taskId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out taskId))
                return;

            int userId = Convert.ToInt32(Session["UserID"]);
            string role = Session["Role"] as string;

            if (e.CommandName == "SubmitWork")
            {
                TextBox note = e.Item.FindControl("txtWorkNote") as TextBox;
                string links;
                string linkError = CollectWorkLinks(e.Item, out links);
                if (linkError != null)
                {
                    Show(linkError, null);
                    BindAll();
                    return;
                }

                IList<HttpPostedFile> files;
                string fileError = CollectTypedUploads(e.Item, out files);
                if (fileError != null)
                {
                    Show(fileError, null);
                    BindAll();
                    return;
                }

                Show(AssignmentService.SubmitWork(
                    taskId,
                    userId,
                    role,
                    note == null ? null : note.Text,
                    links,
                    files,
                    Server), "Work saved.");
                Reload();
                return;
            }

            DropDownList ddl = e.Item.FindControl("ddlStatus") as DropDownList;
            string status = ddl == null ? null : ddl.SelectedValue;
            Show(AssignmentService.UpdateTask(taskId, userId, role, null, status), "Task updated.");
            Reload();
        }

        protected void rptMembers_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int userId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out userId))
                return;
            if (e.CommandName == "MakeLeader")
                Show(AssignmentService.ChangeLeader(groupId, Convert.ToInt32(Session["UserID"]), Session["Role"] as string, userId), "Leader updated.");
            Reload();
        }

        protected void rptMembers_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
                return;

            DataRowView member = e.Item.DataItem as DataRowView;
            Repeater rptUpdates = e.Item.FindControl("rptUpdates") as Repeater;
            Panel pnlNoUpdates = e.Item.FindControl("pnlNoUpdates") as Panel;
            if (member == null || rptUpdates == null)
                return;

            DataView view = memberUpdates == null
                ? new DataView()
                : new DataView(memberUpdates) { RowFilter = "UserID = " + Convert.ToInt32(member["UserID"]) };
            rptUpdates.DataSource = view;
            rptUpdates.DataBind();
            if (pnlNoUpdates != null)
                pnlNoUpdates.Visible = view.Count == 0;
            Literal litUpdateCount = e.Item.FindControl("litUpdateCount") as Literal;
            if (litUpdateCount != null)
                litUpdateCount.Text = view.Count == 0 ? "" : "(" + view.Count + ")";
        }

        protected void rptUpdates_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
                return;

            DataRowView row = e.Item.DataItem as DataRowView;
            Repeater rptUpdateFiles = e.Item.FindControl("rptUpdateFiles") as Repeater;
            Repeater rptUpdateLinks = e.Item.FindControl("rptUpdateLinks") as Repeater;
            if (row == null)
                return;

            string description = Convert.ToString(row["Description"]);
            if (string.IsNullOrEmpty(description) || !description.StartsWith("Submitted work", StringComparison.OrdinalIgnoreCase))
                return;

            int updateTaskId = Convert.ToInt32(row["TaskID"]);
            if (rptUpdateLinks != null)
                BindTaskLinks(rptUpdateLinks, updateTaskId);
            if (rptUpdateFiles != null)
                BindTaskFiles(rptUpdateFiles, updateTaskId);
        }

        protected void rptTasks_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
                return;
            DataRowView row = e.Item.DataItem as DataRowView;
            if (row == null)
                return;

            DropDownList ddl = e.Item.FindControl("ddlStatus") as DropDownList;
            string status = Convert.ToString(row["Status"]);
            if (ddl != null && ddl.Items.FindByValue(status) != null)
                ddl.SelectedValue = status;
            var actions = e.Item.FindControl("pnlTaskActions") as System.Web.UI.HtmlControls.HtmlGenericControl;
            if (actions != null)
                actions.Visible = access != null && (!access.IsDeadlineLocked || access.CanCorrect);

            int userId = Convert.ToInt32(Session["UserID"]);
            int? assignee = row["AssignedUserID"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["AssignedUserID"]);
            string note = row["SubmissionNote"] == DBNull.Value ? null : Convert.ToString(row["SubmissionNote"]);
            int currentTaskId = Convert.ToInt32(row["TaskID"]);
            DataView files = FilesForTask(currentTaskId);
            DataView links = LinksForTask(currentTaskId);
            bool hasWork = !string.IsNullOrWhiteSpace(note) || files.Count > 0 || links.Count > 0;

            Panel pnlWork = e.Item.FindControl("pnlWork") as Panel;
            if (pnlWork != null)
            {
                pnlWork.Visible = hasWork;
                Literal litNote = e.Item.FindControl("litWorkNote") as Literal;
                if (litNote != null)
                    litNote.Text = AssignmentService.FormatNoteHtml(note);
                Repeater rptWorkLinks = e.Item.FindControl("rptWorkLinks") as Repeater;
                if (rptWorkLinks != null)
                {
                    rptWorkLinks.DataSource = links;
                    rptWorkLinks.DataBind();
                }
                Repeater rptWorkFiles = e.Item.FindControl("rptWorkFiles") as Repeater;
                if (rptWorkFiles != null)
                {
                    rptWorkFiles.DataSource = files;
                    rptWorkFiles.DataBind();
                }
            }

            Panel pnlSubmit = e.Item.FindControl("pnlSubmit") as Panel;
            if (pnlSubmit != null)
            {
                pnlSubmit.Visible = access != null && access.CanSubmitWork(assignee, userId, status);
                TextBox txtNote = e.Item.FindControl("txtWorkNote") as TextBox;
                if (txtNote != null && !string.IsNullOrWhiteSpace(note))
                    txtNote.Text = note;
            }
        }

        private void Reload()
        {
            access = AssignmentService.GetAccess(groupId, Convert.ToInt32(Session["UserID"]), Session["Role"] as string);
            BindAll();
        }

        private void BindAll()
        {
            AssignmentGroupRecord group = access.Group;
            lnkScheduleMeeting.NavigateUrl = "~/Modules/UserMeetings/UserMeetings.aspx?GroupID=" + groupId;
            lnkBack.NavigateUrl = access.IsFacultyOwner
                ? "~/Modules/Assignments/AssignmentDetails.aspx?AssignmentID=" + group.AssignmentID
                : "~/Modules/Assignments/MyAssignments.aspx";
            litGroup.Text = Server.HtmlEncode(group.GroupName);
            string assignmentName = group.Assignment == null ? "" : group.Assignment.AssignmentName;
            string roleLabel = access.IsFacultyOwner ? "Faculty administrator" : (access.IsLeader ? "Leader" : (access.Responsibility ?? "Participant"));
            litMeta.Text = Server.HtmlEncode(assignmentName + " - " + roleLabel);
            litDeadline.Text = group.Assignment == null ? "-" : group.Assignment.Deadline.ToString("MMM dd, yyyy HH:mm");
            bool locked = access.IsDeadlineLocked;
            pnlLocked.Visible = locked && !access.CanCorrect;
            pnlFacultyDeadline.Visible = access.CanCorrect && group.Assignment != null;
            if (group.Assignment != null)
                txtFacultyDeadline.Text = group.Assignment.Deadline.ToString("yyyy-MM-ddTHH:mm");
            int daysLeft = group.Assignment == null ? 99 : (group.Assignment.Deadline.Date - DateTime.Today).Days;
            pnlDeadlineAlert.Visible = !locked && group.Assignment != null
                && string.Equals(group.Assignment.Status, "Active", StringComparison.OrdinalIgnoreCase)
                && daysLeft >= 0 && daysLeft <= 3;
            litDeadlineAlert.Text = daysLeft == 0
                ? "Assignment deadline is today. Finish remaining work. Students cannot extend this deadline."
                : "Assignment deadline is in " + daysLeft + " day(s). All associated members have been alerted.";
            pnlFinalized.Visible = group.IsFinalized;
            btnFinalize.Visible = access.CanManageStructure && !group.IsFinalized;
            pnlAddTask.Visible = access.CanManageStructure;
            pnlInvite.Visible = access.CanInvite;

            DataSet progress = AssignmentService.GetProgress(groupId);
            if (progress.Tables.Count > 0 && progress.Tables[0].Rows.Count > 0)
                litProgress.Text = AssignmentService.FormatPercent(progress.Tables[0].Rows[0]["CompletionPercentage"]);
            else
                litProgress.Text = "0%";

            DataTable tasks = AssignmentService.ListTasks(groupId);
            rptTasks.DataSource = tasks;
            rptTasks.DataBind();
            pnlNoTasks.Visible = tasks.Rows.Count == 0;

            DataTable members = AssignmentService.ListMembers(groupId);
            memberUpdates = AssignmentService.ListMemberUpdates(groupId);
            taskFiles = AssignmentService.ListGroupFiles(groupId);
            taskLinks = AssignmentService.ListGroupLinks(groupId);
            rptMembers.DataSource = members;
            rptMembers.DataBind();

            if (access.CanManageStructure)
            {
                ddlParent.Items.Clear();
                ddlParent.Items.Add(new ListItem("Top-level section", "0"));
                foreach (DataRow row in tasks.Rows)
                {
                    if (row["ParentTaskID"] == DBNull.Value)
                        ddlParent.Items.Add(new ListItem("Under: " + Convert.ToString(row["Title"]), Convert.ToString(row["TaskID"])));
                }

                ddlAssignee.Items.Clear();
                ddlAssignee.Items.Add(new ListItem("Unassigned", "0"));
                foreach (DataRow row in members.Rows)
                    ddlAssignee.Items.Add(new ListItem(Convert.ToString(row["FullName"]), Convert.ToString(row["UserID"])));
            }

            rptContribution.DataSource = AssignmentService.GetContribution(groupId);
            rptContribution.DataBind();
        }

        private static string CollectWorkLinks(RepeaterItem item, out string links)
        {
            links = null;
            var parts = new List<string>();
            TextBox owner = item.FindControl("txtGithubOwner") as TextBox;
            TextBox repo = item.FindControl("txtGithubRepo") as TextBox;
            string gitError;
            string gitUrl = AssignmentService.ComposeGitHubUrl(
                owner == null ? null : owner.Text,
                repo == null ? null : repo.Text,
                out gitError);
            if (gitError != null)
                return gitError;
            if (!string.IsNullOrEmpty(gitUrl))
                parts.Add(gitUrl);

            TextBox url = item.FindControl("txtWorkUrl") as TextBox;
            if (url != null && !string.IsNullOrWhiteSpace(url.Text))
                parts.Add(url.Text.Trim());

            links = parts.Count == 0 ? null : string.Join("\n", parts.ToArray());
            return null;
        }

        private static string CollectTypedUploads(RepeaterItem item, out IList<HttpPostedFile> files)
        {
            files = new List<HttpPostedFile>();
            string error = AddTypedFiles(item.FindControl("fuPdf") as FileUpload, new[] { ".pdf" }, "PDF", files);
            if (error != null)
                return error;
            error = AddTypedFiles(item.FindControl("fuWord") as FileUpload, new[] { ".doc", ".docx" }, "Word", files);
            if (error != null)
                return error;
            return AddTypedFiles(item.FindControl("fuImage") as FileUpload, new[] { ".jpg", ".jpeg", ".png" }, "image", files);
        }

        private static string AddTypedFiles(FileUpload upload, string[] allowed, string label, IList<HttpPostedFile> files)
        {
            foreach (HttpPostedFile file in CollectUploads(upload))
            {
                string ext = Path.GetExtension(file.FileName ?? "").ToLowerInvariant();
                bool ok = false;
                for (int i = 0; i < allowed.Length; i++)
                {
                    if (ext == allowed[i])
                    {
                        ok = true;
                        break;
                    }
                }
                if (!ok)
                    return "The " + label + " button only accepts " + string.Join(", ", allowed) + " files.";
                files.Add(file);
            }
            return null;
        }

        private static IList<HttpPostedFile> CollectUploads(FileUpload upload)
        {
            var files = new List<HttpPostedFile>();
            if (upload == null)
                return files;

            if (upload.HasFiles)
            {
                foreach (HttpPostedFile file in upload.PostedFiles)
                {
                    if (file != null && file.ContentLength > 0)
                        files.Add(file);
                }
            }
            else if (upload.HasFile)
            {
                files.Add(upload.PostedFile);
            }

            return files;
        }

        private DataView FilesForTask(int taskId)
        {
            if (taskFiles == null)
                return new DataView();
            return new DataView(taskFiles) { RowFilter = "TaskID = " + taskId };
        }

        private DataView LinksForTask(int taskId)
        {
            if (taskLinks == null)
                return new DataView();
            return new DataView(taskLinks) { RowFilter = "TaskID = " + taskId };
        }

        private void BindTaskFiles(Repeater repeater, int taskId)
        {
            DataView files = FilesForTask(taskId);
            repeater.DataSource = files;
            repeater.DataBind();
            repeater.Visible = files.Count > 0;
        }

        private void BindTaskLinks(Repeater repeater, int taskId)
        {
            DataView links = LinksForTask(taskId);
            repeater.DataSource = links;
            repeater.DataBind();
            repeater.Visible = links.Count > 0;
        }

        private void ApplyChrome()
        {
            adminTop.Visible = false;
            adminSide.Visible = false;
            userTop.Visible = true;
            userSide.Visible = true;
        }

        private void Show(string error, string ok)
        {
            lblMessage.Text = error ?? ok;
            lblMessage.CssClass = error != null
                ? "font-label-md block mb-4 text-error"
                : "font-label-md block mb-4 text-tertiary";
        }
    }
}
