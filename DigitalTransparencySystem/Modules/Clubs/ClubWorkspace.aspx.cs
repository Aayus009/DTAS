using System;
using System.Data;
using System.Web.UI.WebControls;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Clubs
{
    public partial class ClubWorkspace : System.Web.UI.Page
    {
        private int clubId;
        private ClubAccess access;

        public bool CanManage
        {
            get { return access != null && access.CanManage; }
        }

        protected string ClubsHomeUrl
        {
            get
            {
                return ResolveUrl(RoleAccess.IsAdmin(Session["Role"] as string)
                    ? "~/Modules/Clubs/Clubs.aspx"
                    : "~/Modules/Clubs/MyClubs.aspx");
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || !int.TryParse(Request.QueryString["ClubID"], out clubId))
            {
                Response.Redirect(ClubsHomeUrl);
                return;
            }

            access = ClubService.GetAccess(clubId, Convert.ToInt32(Session["UserID"]), Session["Role"] as string);
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

        public string MessageCss(object type)
        {
            string value = Convert.ToString(type);
            if (string.Equals(value, "Urgent", StringComparison.OrdinalIgnoreCase))
                return "club-msg club-msg-urgent";
            if (string.Equals(value, "Announcement", StringComparison.OrdinalIgnoreCase))
                return "club-msg club-msg-announce";
            return "club-msg";
        }

        public bool IsUrgent(object type)
        {
            return string.Equals(Convert.ToString(type), "Urgent", StringComparison.OrdinalIgnoreCase);
        }

        public bool CanTransferLead(object role)
        {
            return CanManage && !string.Equals(Convert.ToString(role), "Lead", StringComparison.OrdinalIgnoreCase);
        }

        public string Initials(object name)
        {
            string value = Convert.ToString(name);
            if (string.IsNullOrWhiteSpace(value))
                return "?";
            string[] parts = value.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
                return parts[0].Substring(0, 1).ToUpperInvariant();
            return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpperInvariant();
        }

        protected void btnJoinPublic_Click(object sender, EventArgs e)
        {
            Show(ClubService.JoinPublic(clubId, Convert.ToInt32(Session["UserID"])),
                "Request sent. The club lead must accept you before you can join.");
            Reload();
        }

        protected void btnAccept_Click(object sender, EventArgs e)
        {
            if (access.PendingInvitationId.HasValue)
                Show(ClubService.AcceptInvitation(access.PendingInvitationId.Value, Convert.ToInt32(Session["UserID"])), "Invitation accepted.");
            Reload();
        }

        protected void btnLeave_Click(object sender, EventArgs e)
        {
            string error = ClubService.Leave(clubId, Convert.ToInt32(Session["UserID"]));
            if (error != null)
            {
                Show(error, null);
                return;
            }
            Response.Redirect(ClubsHomeUrl);
        }

        protected void btnSend_Click(object sender, EventArgs e)
        {
            string type = "Text";
            if (chkUrgent.Checked)
                type = "Urgent";
            else if (chkAnnounce.Checked)
                type = "Announcement";

            Show(ClubService.SendMessage(clubId, Convert.ToInt32(Session["UserID"]), Session["Role"] as string, txtMessage.Text, type), "Message sent.");
            if (!UiNotice.HasError(lblMessage))
                txtMessage.Text = "";
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
            Show(ClubService.InviteByEmail(clubId, Convert.ToInt32(Session["UserID"]), Session["Role"] as string, txtInviteEmail.Text), "Invitation sent.");
            Reload();
        }

        protected void rptMembers_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int userId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out userId))
                return;
            if (e.CommandName == "MakeLead")
                Show(ClubService.TransferLead(clubId, Convert.ToInt32(Session["UserID"]), Session["Role"] as string, userId), "Lead updated.");
            Reload();
        }

        protected void rptJoinRequests_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int targetId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out targetId))
                return;

            int actorId = Convert.ToInt32(Session["UserID"]);
            string role = Session["Role"] as string;
            string error = e.CommandName == "AcceptJoin"
                ? ClubService.AcceptJoinRequest(clubId, actorId, role, targetId)
                : ClubService.DeclineJoinRequest(clubId, actorId, role, targetId);
            Show(error, e.CommandName == "AcceptJoin" ? "Join request accepted." : "Join request declined.");
            Reload();
        }

        private void Reload()
        {
            access = ClubService.GetAccess(clubId, Convert.ToInt32(Session["UserID"]), Session["Role"] as string);
            BindAll();
        }

        private void BindAll()
        {
            ClubRecord club = access.Club;
            litName.Text = Server.HtmlEncode(club.ClubName);
            litVisibility.Text = club.IsPublic ? "Public" : "Private";
            litRoleChip.Text = access.IsLead ? "Lead" : (access.IsMember ? "Member" : "Visitor");
            litAvatarInitial.Text = Initials(club.ClubName);
            bool hasImage = !string.IsNullOrEmpty(club.ImagePath);
            imgClub.Visible = hasImage;
            pnlAvatarFallback.Visible = !hasImage;
            if (hasImage)
                imgClub.ImageUrl = club.ImagePath;

            bool restricted = club.IsRestricted;
            pnlRestricted.Visible = restricted;
            btnRestrict.Visible = access.IsSystemAdmin && !restricted;
            btnRestore.Visible = access.IsSystemAdmin && restricted;

            btnJoinPublic.Visible = !restricted && !access.IsMember && !access.HasPendingJoinRequest && club.IsPublic;
            pnlJoinPending.Visible = !restricted && access.HasPendingJoinRequest && !access.IsMember;
            btnAccept.Visible = !restricted && access.HasPendingInvite && !access.IsMember;
            btnLeave.Visible = access.IsMember;
            pnlCompose.Visible = !restricted && access.IsMember;
            chkUrgent.Visible = access.CanManage;
            chkAnnounce.Visible = access.CanManage;
            pnlInvite.Visible = access.CanManage;
            pnlCode.Visible = access.CanManage && !string.IsNullOrEmpty(club.InviteCode);
            litCode.Text = Server.HtmlEncode(club.InviteCode ?? "");
            hfInviteCode.Value = club.InviteCode ?? "";
            litClubId.Text = club.ClubID.ToString();
            pnlClubId.Visible = access.IsSystemAdmin || (!restricted && access.IsMember);

            DataTable messages = ClubService.ListMessages(clubId);
            rptMessages.DataSource = messages;
            rptMessages.DataBind();
            pnlNoMessages.Visible = messages.Rows.Count == 0;

            DataTable members = ClubService.ListMembers(clubId);
            rptMembers.DataSource = members;
            rptMembers.DataBind();
            litMemberCount.Text = members.Rows.Count.ToString();

            DataTable joinRequests = ClubService.ListJoinRequests(clubId);
            rptJoinRequests.DataSource = joinRequests;
            rptJoinRequests.DataBind();
            litJoinCount.Text = joinRequests.Rows.Count.ToString();
            pnlJoinRequests.Visible = access.CanManage && !restricted && joinRequests.Rows.Count > 0;

            DataTable announcements = ClubService.ListAnnouncements(clubId);
            rptAnnouncements.DataSource = announcements;
            rptAnnouncements.DataBind();
            pnlAnnouncements.Visible = announcements.Rows.Count > 0;
        }

        protected void btnRestrict_Click(object sender, EventArgs e)
        {
            Show(RestrictionService.SetClubRestricted(clubId, Convert.ToInt32(Session["UserID"]), Session["Role"] as string, true),
                "Club restricted. Members cannot perform club activity until it is restored.");
            Reload();
        }

        protected void btnRestore_Click(object sender, EventArgs e)
        {
            Show(RestrictionService.SetClubRestricted(clubId, Convert.ToInt32(Session["UserID"]), Session["Role"] as string, false),
                "Club restored. Members can resume club activity.");
            Reload();
        }

        private void ApplyChrome()
        {
            bool admin = RoleAccess.IsAdmin(Session["Role"] as string);
            adminTop.Visible = admin;
            adminSide.Visible = admin;
            userTop.Visible = !admin;
            userSide.Visible = !admin;
        }

        private void Show(string error, string ok)
        {
            UiNotice.Bind(lblMessage, error, ok);
        }
    }
}
