using System;
using System.Data;
using System.Web.UI.WebControls;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Connect
{
    public partial class ConnectHome : System.Web.UI.Page
    {
        private string filter = "all";

        public bool IsArchiveView { get { return filter == "archive"; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Modules/Authentication/Login.aspx");
                return;
            }

            filter = ConnectService.NormalizeFilter(Request.QueryString["filter"]);
            if (Page.Form != null)
                Page.Form.Enctype = "multipart/form-data";

            if (!IsPostBack)
                BindGroups();
        }

        protected void btnShowCreate_Click(object sender, EventArgs e)
        {
            pnlJoin.Visible = false;
            pnlCreate.Visible = true;
            BindGroups();
        }

        protected void btnCancelCreate_Click(object sender, EventArgs e)
        {
            pnlCreate.Visible = false;
            BindGroups();
        }

        protected void btnShowJoin_Click(object sender, EventArgs e)
        {
            pnlCreate.Visible = false;
            pnlJoin.Visible = true;
            BindGroups();
        }

        protected void btnCancelJoin_Click(object sender, EventArgs e)
        {
            pnlJoin.Visible = false;
            BindGroups();
        }

        protected void btnCreate_Click(object sender, EventArgs e)
        {
            int groupId;
            string code;
            int userId = Convert.ToInt32(Session["UserID"]);
            string error = ConnectService.CreateGroup(userId, txtName.Text, txtDescription.Text, out groupId, out code);
            if (error != null)
            {
                Show(error);
                pnlCreate.Visible = true;
                BindGroups();
                return;
            }
            if (fuCreatePhoto.HasFile)
                ConnectService.UpdatePhoto(groupId, userId, fuCreatePhoto.PostedFile, Server);
            Response.Redirect("~/Modules/Connect/ConnectRoom.aspx?GroupID=" + groupId);
        }

        protected void btnJoin_Click(object sender, EventArgs e)
        {
            int groupId;
            string error = ConnectService.JoinByCode(Convert.ToInt32(Session["UserID"]), txtCode.Text, out groupId);
            if (error != null)
            {
                Show(error);
                pnlJoin.Visible = true;
                BindGroups();
                return;
            }
            Response.Redirect("~/Modules/Connect/ConnectRoom.aspx?GroupID=" + groupId);
        }

        protected void rptGroups_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int groupId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out groupId))
                return;

            bool archive = string.Equals(e.CommandName, "Archive", StringComparison.OrdinalIgnoreCase);
            string error = ConnectService.SetArchived(groupId, Convert.ToInt32(Session["UserID"]), archive);
            if (error != null)
                Show(error);
            BindGroups();
        }

        public string FilterClass(string value)
        {
            return "connect-filter" + (filter == value ? " is-active" : "");
        }

        public string FilterUrl(string value)
        {
            return ResolveUrl("~/Modules/Connect/Connect.aspx?filter=" + value);
        }

        public string RoomUrl(object groupId)
        {
            return ResolveUrl("~/Modules/Connect/ConnectRoom.aspx?GroupID=" + groupId + "&filter=" + filter);
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

        private void BindGroups()
        {
            DataTable table = ConnectService.ListMyGroups(Convert.ToInt32(Session["UserID"]), filter);
            rptGroups.DataSource = table;
            rptGroups.DataBind();
            pnlEmpty.Visible = table.Rows.Count == 0;
            if (filter == "unread")
                litEmpty.Text = "No unread messages";
            else if (filter == "archive")
                litEmpty.Text = "No archived chats";
            else
                litEmpty.Text = "No conversations yet";
        }

        private void Show(string error)
        {
            lblMessage.Text = error ?? "";
        }
    }
}
