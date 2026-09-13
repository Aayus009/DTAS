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

        public string EncodeComment(object value)
        {
            return HttpUtility.HtmlEncode(Convert.ToString(value) ?? "");
        }

        public string AssigneeLabel(object name)
        {
            string value = Convert.ToString(name);
            return string.IsNullOrWhiteSpace(value) ? "Unassigned" : value;
        }

        public string Initials(object name)
        {
            string value = Convert.ToString(name) ?? "";
            string[] parts = value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return "?";
            if (parts.Length == 1)
                return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpperInvariant();
            return (char.ToUpperInvariant(parts[0][0]).ToString() + char.ToUpperInvariant(parts[parts.Length - 1][0]));
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

            AssignmentService.EnsureSchema();
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

        protected void btnInviteSearch_Click(object sender, EventArgs e)
        {
            BindInviteResults();
            txtInviteSearch.Focus();
        }

        protected void rptInviteResults_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int inviteeId;
            if (e.CommandName != "Invite" || !int.TryParse(Convert.ToString(e.CommandArgument), out inviteeId))
                return;

            Show(AssignmentService.InviteMemberByUserId(
                groupId,
                Convert.ToInt32(Session["UserID"]),
                Session["Role"] as string,
                inviteeId), "Invitation sent.");
            Reload();
            BindInviteResults();
        }

        protected void btnAddTask_Click(object sender, EventArgs e)
        {
            int parent;
            int? parentId = int.TryParse(ddlParent.SelectedValue, out parent) && parent > 0 ? parent : (int?)null;
            DateTime due;
            DateTime? dueDate = DateTime.TryParse(txtDue.Text, out due) ? due : (DateTime?)null;

            var titles = new List<string>();
            foreach (string line in (txtTaskTitles.Text ?? "").Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!string.IsNullOrWhiteSpace(line))
                    titles.Add(line.Trim());
            }

            var assignees = new List<int>();
            foreach (ListItem item in cblAssignees.Items)
            {
                int assigneeId;
                if (item.Selected && int.TryParse(item.Value, out assigneeId) && assigneeId > 0)
                    assignees.Add(assigneeId);
            }

            string error = AssignmentService.AddTasks(
                groupId,
                Convert.ToInt32(Session["UserID"]),
                Session["Role"] as string,
                titles,
                assignees,
                parentId,
                dueDate);
            int created = titles.Count * Math.Max(assignees.Count, 1);
            Show(error, created == 1 ? "Task allocated." : "Allocated " + created + " tasks.");
            if (!UiNotice.HasError(lblMessage))
                txtTaskTitles.Text = "";
            Reload();
        }

        protected void rptTasks_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int taskId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out taskId))
                return;

            ViewState["OpenTaskId"] = taskId;
            int userId = Convert.ToInt32(Session["UserID"]);
            string role = Session["Role"] as string;

            if (e.CommandName == "Comment")
            {
                TextBox comment = e.Item.FindControl("txtTaskComment") as TextBox;
                Show(AssignmentService.AddTaskComment(
                    taskId,
                    userId,
                    role,
                    comment == null ? "" : comment.Text), "Comment added.");
                Reload();
                return;
            }

            string workError;
            bool savedWork = TrySaveAttachedWork(e.Item, taskId, userId, role, out workError);

            DropDownList ddl = e.Item.FindControl("ddlStatus") as DropDownList;
            string status = ddl == null ? null : ddl.SelectedValue;
            string statusError = AssignmentService.UpdateTask(taskId, userId, role, null, status);
            if (!string.IsNullOrEmpty(statusError))
                Show(statusError, null);
            else if (!string.IsNullOrEmpty(workError))
                Show(workError, null);
            else if (savedWork)
                Show(null, "Status and file saved. The file is under Work files and in your Updates.");
            else
                Show(null, "Task updated.");
            Reload();
        }

        private bool TrySaveAttachedWork(RepeaterItem item, int taskId, int userId, string role, out string error)
        {
            error = null;
            TextBox note = item.FindControl("txtWorkNote") as TextBox;
            string links;
            error = CollectWorkLinks(item, out links);
            if (error != null)
                return false;

            IList<HttpPostedFile> files;
            error = CollectTypedUploads(item, out files);
            if (error != null)
                return false;

            bool hasNote = note != null && !string.IsNullOrWhiteSpace(note.Text);
            bool hasLinks = !string.IsNullOrWhiteSpace(links);
            bool hasFiles = files != null && files.Count > 0;
            if (!hasNote && !hasLinks && !hasFiles)
                return false;

            error = AssignmentService.SubmitWork(
                taskId,
                userId,
                role,
                hasNote ? note.Text : null,
                links,
                files,
                Server);
            return error == null;
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
            HtmlGenericControl details = e.Item.FindControl("taskDetails") as HtmlGenericControl;
            if (details != null && ViewState["OpenTaskId"] != null && Convert.ToInt32(ViewState["OpenTaskId"]) == currentTaskId)
                details.Attributes["open"] = "open";
            DataView files = FilesForTask(currentTaskId);
            DataView links = LinksForTask(currentTaskId);
            bool hasWork = !string.IsNullOrWhiteSpace(note) || files.Count > 0 || links.Count > 0;

            Panel pnlWork = e.Item.FindControl("pnlWork") as Panel;
            if (pnlWork != null)
            {
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
                Label lblNoFiles = e.Item.FindControl("lblNoFiles") as Label;
                if (lblNoFiles != null)
                    lblNoFiles.Visible = !hasWork;
            }

            Panel pnlSubmit = e.Item.FindControl("pnlSubmit") as Panel;
            if (pnlSubmit != null)
            {
                pnlSubmit.Visible = access != null && access.CanSubmitWork(assignee, userId, status);
                TextBox txtNote = e.Item.FindControl("txtWorkNote") as TextBox;
                if (txtNote != null && !string.IsNullOrWhiteSpace(note))
                    txtNote.Text = note;
            }

            Repeater rptTaskComments = e.Item.FindControl("rptTaskComments") as Repeater;
            Label lblNoComments = e.Item.FindControl("lblNoComments") as Label;
            Panel pnlAddComment = e.Item.FindControl("pnlAddComment") as Panel;
            DataTable comments = AssignmentService.ListTaskComments(currentTaskId);
            if (rptTaskComments != null)
            {
                rptTaskComments.DataSource = comments;
                rptTaskComments.DataBind();
            }
            if (lblNoComments != null)
                lblNoComments.Visible = comments.Rows.Count == 0;
            if (pnlAddComment != null)
                pnlAddComment.Visible = access != null && access.CanComment;
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
            int connectId;
            ConnectService.CreateGroupForAssignmentSubgroup(groupId, out connectId);
            lnkGroupConnect.Visible = connectId > 0;
            lnkGroupConnect.NavigateUrl = connectId > 0
                ? "~/Modules/Connect/ConnectRoom.aspx?GroupID=" + connectId
                : "";
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

            DataTable tasks = AssignmentService.ListTasks(groupId);
            decimal pct = AssignmentService.ComputeGroupProgress(tasks);
            litProgress.Text = AssignmentService.FormatPercent(pct);
            if (pct < 0) pct = 0;
            if (pct > 100) pct = 100;
            progressFill.Style["width"] = pct.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "%";
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

                var selectedIds = new List<string>();
                foreach (ListItem item in cblAssignees.Items)
                {
                    if (item.Selected)
                        selectedIds.Add(item.Value);
                }
                cblAssignees.Items.Clear();
                foreach (DataRow row in members.Rows)
                {
                    var item = new ListItem(Convert.ToString(row["FullName"]), Convert.ToString(row["UserID"]));
                    item.Selected = selectedIds.Contains(item.Value);
                    cblAssignees.Items.Add(item);
                }
            }

            rptContribution.DataSource = AssignmentService.GetContribution(groupId);
            rptContribution.DataBind();
        }

        private void BindInviteResults()
        {
            string query = txtInviteSearch == null ? "" : txtInviteSearch.Text;
            DataTable results = AssignmentService.SearchStudentsForInvite(
                groupId,
                Convert.ToInt32(Session["UserID"]),
                Session["Role"] as string,
                query);
            rptInviteResults.DataSource = results;
            rptInviteResults.DataBind();

            string term = (query ?? "").Trim();
            if (term.Length < 2)
            {
                pnlInviteEmpty.Visible = false;
                lblLookup.CssClass = "font-label-md block mb-3 text-on-surface-variant";
                lblLookup.Text = "Type at least 2 characters to search.";
                return;
            }

            pnlInviteEmpty.Visible = results.Rows.Count == 0;
            lblLookup.Text = results.Rows.Count == 0
                ? ""
                : results.Rows.Count + " student" + (results.Rows.Count == 1 ? "" : "s") + " found.";
            lblLookup.CssClass = "font-label-md block mb-3 text-on-surface-variant";
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
            error = AddTypedFiles(item.FindControl("fuImage") as FileUpload, new[] { ".jpg", ".jpeg", ".png" }, "image", files);
            if (error != null)
                return error;
            return AddTypedFiles(item.FindControl("fuZip") as FileUpload, new[] { ".zip" }, "ZIP", files);
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
            UiNotice.Bind(lblMessage, error, ok);
        }
    }
}
