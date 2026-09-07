using System;
using System.Data;
using System.Web.UI.WebControls;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Clubs
{
    public partial class MyClubs : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Modules/Authentication/Login.aspx");
                return;
            }

            if (RoleAccess.IsAdmin(Session["Role"] as string))
            {
                Response.Redirect("~/Modules/Clubs/Clubs.aspx");
                return;
            }

            pnlCreate.Visible = RoleAccess.CanCreateClubs(Session["Role"] as string);

            if (!IsPostBack)
                BindAll();
        }

        protected void btnCreate_Click(object sender, EventArgs e)
        {
            if (!RoleAccess.CanCreateClubs(Session["Role"] as string))
            {
                Show("Only faculty, staff, and students can create a club.", null);
                return;
            }

            int clubId;
            string error = ClubService.CreateClub(
                Convert.ToInt32(Session["UserID"]),
                txtName.Text,
                txtDescription.Text.Trim(),
                chkPublic.Checked,
                fuImage.PostedFile,
                Server,
                out clubId);
            if (error != null)
            {
                Show(error, null);
                return;
            }

            Response.Redirect("~/Modules/Clubs/ClubWorkspace.aspx?ClubID=" + clubId);
        }

        protected void btnJoinCode_Click(object sender, EventArgs e)
        {
            int clubId;
            string error = ClubService.JoinByCode(Convert.ToInt32(Session["UserID"]), txtCode.Text, out clubId);
            if (error != null)
            {
                Show(error, null);
                BindAll();
                return;
            }

            Response.Redirect("~/Modules/Clubs/ClubWorkspace.aspx?ClubID=" + clubId);
        }

        protected void rptInvites_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int invitationId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out invitationId))
                return;

            int userId = Convert.ToInt32(Session["UserID"]);
            string error = e.CommandName == "Accept"
                ? ClubService.AcceptInvitation(invitationId, userId)
                : ClubService.DeclineInvitation(invitationId, userId);
            Show(error, e.CommandName == "Accept" ? "You joined the club." : "Invitation declined.");
            BindAll();
        }

        protected void rptPublic_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int clubId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out clubId))
                return;
            string error = ClubService.JoinPublic(clubId, Convert.ToInt32(Session["UserID"]));
            Show(error, "Request sent. The club lead must accept you before you can join.");
            BindAll();
        }

        private void BindAll()
        {
            int userId = Convert.ToInt32(Session["UserID"]);

            DataTable invites = ClubService.ListMyInvites(userId);
            rptInvites.DataSource = invites;
            rptInvites.DataBind();
            pnlNoInvites.Visible = invites.Rows.Count == 0;

            DataTable mine = ClubService.ListMyClubs(userId);
            rptMine.DataSource = mine;
            rptMine.DataBind();
            pnlNoMine.Visible = mine.Rows.Count == 0;

            DataTable pub = ClubService.ListPublic(userId);
            rptPublic.DataSource = pub;
            rptPublic.DataBind();
            pnlNoPublic.Visible = pub.Rows.Count == 0;
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

        private void Show(string error, string ok)
        {
            lblMessage.Text = error ?? ok;
            lblMessage.CssClass = error != null
                ? "font-label-md block mb-4 text-error"
                : "font-label-md block mb-4 text-tertiary";
        }
    }
}
