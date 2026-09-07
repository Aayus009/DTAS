using System;
using System.Data;
using System.Web.UI.WebControls;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Connect
{
    public partial class ConnectRoom : System.Web.UI.Page
    {
        private int groupId;
        private ConnectAccess access;
        private string filter = "all";

        public int GroupID { get { return groupId; } }
        public string Filter { get { return filter; } }
        public bool IsArchiveView { get { return filter == "archive"; } }
        public bool MembersOpen { get; set; }
        public int CurrentUserID { get { return Session["UserID"] == null ? 0 : Convert.ToInt32(Session["UserID"]); } }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || !int.TryParse(Request.QueryString["GroupID"], out groupId))
            {
                Response.Redirect("~/Modules/Connect/Connect.aspx");
                return;
            }

            filter = ConnectService.NormalizeFilter(Request.QueryString["filter"]);
            if (Page.Form != null)
                Page.Form.Enctype = "multipart/form-data";

            access = ConnectService.GetAccess(groupId, Convert.ToInt32(Session["UserID"]));
            if (!access.CanView)
            {
                pnlRoom.Visible = false;
                pnlDenied.Visible = true;
                return;
            }

            pnlAddMember.Visible = access.CanManage;
            if (!IsPostBack)
            {
                ConnectService.MarkRead(groupId, Convert.ToInt32(Session["UserID"]));
                BindRoom();
            }
        }

        protected void btnLeave_Click(object sender, EventArgs e)
        {
            string error = ConnectService.LeaveGroup(groupId, Convert.ToInt32(Session["UserID"]));
            if (error != null)
                return;
            Response.Redirect("~/Modules/Connect/Connect.aspx?filter=" + filter);
        }

        protected void btnArchive_Click(object sender, EventArgs e)
        {
            access = ConnectService.GetAccess(groupId, Convert.ToInt32(Session["UserID"]));
            ConnectService.SetArchived(groupId, Convert.ToInt32(Session["UserID"]), !access.IsArchived);
            Response.Redirect(RoomUrl(groupId));
        }

        protected void btnSaveGroup_Click(object sender, EventArgs e)
        {
            int userId = Convert.ToInt32(Session["UserID"]);
            string error = ConnectService.RenameGroup(groupId, userId, txtEditName.Text);
            if (error == null && fuGroupPhoto.HasFile)
                error = ConnectService.UpdatePhoto(groupId, userId, fuGroupPhoto.PostedFile, Server);

            access = ConnectService.GetAccess(groupId, userId);
            if (error != null)
            {
                lblEditGroup.Text = error;
                pnlEditGroup.CssClass = "connect-overlay";
                BindRoom();
                return;
            }

            pnlEditGroup.CssClass = "connect-overlay hidden";
            BindRoom();
        }

        protected void btnLookup_Click(object sender, EventArgs e)
        {
            EmailLookupResult result = EventService.LookupByExactEmail(txtInviteEmail.Text);
            lblLookup.Text = result.Found ? result.FullName + " can be added." : result.Message;
            MembersOpen = true;
            BindRoom();
        }

        protected void btnAdd_Click(object sender, EventArgs e)
        {
            string error = ConnectService.AddMember(groupId, Convert.ToInt32(Session["UserID"]), txtInviteEmail.Text);
            lblLookup.Text = error ?? "Member added.";
            if (error == null)
                txtInviteEmail.Text = "";
            MembersOpen = true;
            BindRoom();
        }

        protected void rptThreads_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int id;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out id))
                return;
            bool archive = string.Equals(e.CommandName, "Archive", StringComparison.OrdinalIgnoreCase);
            ConnectService.SetArchived(id, Convert.ToInt32(Session["UserID"]), archive);
            access = ConnectService.GetAccess(groupId, Convert.ToInt32(Session["UserID"]));
            BindRoom();
        }

        public string FilterClass(string value)
        {
            return "connect-filter" + (filter == value ? " is-active" : "");
        }

        public string FilterUrl(string value)
        {
            return ResolveUrl("~/Modules/Connect/ConnectRoom.aspx?GroupID=" + groupId + "&filter=" + value);
        }

        public string RoomUrl(object id)
        {
            return ResolveUrl("~/Modules/Connect/ConnectRoom.aspx?GroupID=" + id + "&filter=" + filter);
        }

        public string AvatarMarkup(object name, object photo)
        {
            string path = Convert.ToString(photo);
            if (!string.IsNullOrWhiteSpace(path))
                return "<img src=\"" + ResolveUrl(path) + "\" alt=\"\" />";
            return Server.HtmlEncode(Initial(name));
        }

        public string UnreadBadge(object count)
        {
            int value = 0;
            int.TryParse(Convert.ToString(count), out value);
            if (value <= 0)
                return "";
            return "<span class=\"connect-unread\">" + value + "</span>";
        }

        public string Initial(object name)
        {
            string value = Convert.ToString(name);
            return string.IsNullOrWhiteSpace(value) ? "?" : value.Trim().Substring(0, 1).ToUpperInvariant();
        }

        public string Preview(object text)
        {
            string value = Convert.ToString(text);
            if (string.IsNullOrWhiteSpace(value))
                return "No messages yet";
            return value.Length > 42 ? value.Substring(0, 42) + "..." : value;
        }

        public string TimeLabel(object value)
        {
            if (value == null || value == DBNull.Value)
                return "";
            DateTime time = Convert.ToDateTime(value);
            if (time.Date == DateTime.Today)
                return time.ToString("h:mm tt");
            if (time.Date == DateTime.Today.AddDays(-1))
                return "Yesterday";
            return time.ToString("MMM d");
        }

        private void BindRoom()
        {
            litName.Text = Server.HtmlEncode(access.Group.GroupName);
            litInitial.Text = Initial(access.Group.GroupName);
            litCode.Text = Server.HtmlEncode(access.Group.InviteCode);
            txtEditName.Text = access.Group.GroupName;
            litArchiveAction.Text = access.IsArchived ? "Unarchive chat" : "Archive chat";

            bool hasPhoto = !string.IsNullOrWhiteSpace(access.Group.PhotoPath);
            phPhoto.Visible = hasPhoto;
            litInitial.Visible = !hasPhoto;
            if (hasPhoto)
                imgGroupPhoto.ImageUrl = access.Group.PhotoPath;

            DataTable members = ConnectService.ListMembers(groupId);
            rptMembers.DataSource = members;
            rptMembers.DataBind();
            litMemberCount.Text = members.Rows.Count.ToString();

            DataTable threads = ConnectService.ListMyGroups(Convert.ToInt32(Session["UserID"]), filter);
            rptThreads.DataSource = threads;
            rptThreads.DataBind();
        }
    }
}
