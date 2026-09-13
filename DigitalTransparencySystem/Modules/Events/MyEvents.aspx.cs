using System;
using System.Data;
using System.Web.UI.WebControls;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Events
{
    public partial class MyEvents : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Modules/Authentication/Login.aspx");
                return;
            }

            lnkCreate.Visible = RoleAccess.CanCreateEvents(Session["Role"] as string);

            if (!IsPostBack)
            {
                string code = Request.QueryString["code"];
                if (!string.IsNullOrEmpty(code))
                    txtCode.Text = code;
                BindAll();
            }
        }

        protected void btnJoinCode_Click(object sender, EventArgs e)
        {
            int userId = Convert.ToInt32(Session["UserID"]);
            string error = EventService.JoinByCode(userId, txtCode.Text);
            Show(error, error == null ? "You joined the event." : null);
            if (error == null)
            {
                int? eventId = EventService.FindEventIdByCode(txtCode.Text);
                if (eventId.HasValue)
                    Response.Redirect("~/Modules/Events/EventWorkspace.aspx?EventID=" + eventId.Value);
            }
            BindAll();
        }

        protected void rptInvites_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int invitationId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out invitationId))
                return;

            int userId = Convert.ToInt32(Session["UserID"]);
            string error = e.CommandName == "Accept"
                ? EventService.AcceptInvitation(invitationId, userId)
                : EventService.DeclineInvitation(invitationId, userId);
            Show(error, error == null ? (e.CommandName == "Accept" ? "Invitation accepted." : "Invitation declined.") : null);
            BindAll();
        }

        protected void rptPublic_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int eventId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out eventId))
                return;

            string error = EventService.JoinPublic(eventId, Convert.ToInt32(Session["UserID"]));
            Show(error, error == null
                ? "Request sent. The event lead or manager must accept you before you can join."
                : null);
            BindAll();
        }

        private void BindAll()
        {
            int userId = Convert.ToInt32(Session["UserID"]);

            DataTable invites = EventService.ListMyInvites(userId);
            rptInvites.DataSource = invites;
            rptInvites.DataBind();
            pnlNoInvites.Visible = invites.Rows.Count == 0;

            DataTable mine = EventService.ListMyEvents(userId);
            rptMine.DataSource = mine;
            rptMine.DataBind();
            pnlNoMine.Visible = mine.Rows.Count == 0;

            DataTable pub = EventService.ListDiscoverablePublic(userId);
            rptPublic.DataSource = pub;
            rptPublic.DataBind();
            pnlNoPublic.Visible = pub.Rows.Count == 0;
        }

        private void Show(string error, string success)
        {
            UiNotice.Bind(lblMessage, error, success);
        }
    }
}
