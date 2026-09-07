using System;
using System.Web.UI;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Transparency
{
    public partial class Transparency : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || !RoleAccess.IsAdmin(Session["Role"] as string))
            {
                Response.Redirect("~/Modules/Authentication/Login.aspx");
                return;
            }

            if (!IsPostBack)
                BindSnapshot(TransparencyService.LoadSnapshot());
        }

        private void BindSnapshot(TransparencySnapshot snap)
        {
            litLastUpdated.Text = snap.SyncedAt;
            litOpenDecisions.Text = snap.OpenDecisions.ToString();
            litImplemented.Text = snap.ImplementedDecisions.ToString();
            litImplementedPct.Text = snap.ImplementedPercent.ToString();
            litPublicEvents.Text = snap.PublicEvents.ToString();
            litEventPct.Text = snap.EventProgressPercent.ToString();
            litMinutesPublished.Text = snap.MeetingsWithMinutes.ToString();
            litMeetings.Text = snap.Meetings.ToString();
            litOpenPolls.Text = snap.OpenPolls.ToString();
            litGapMinutes.Text = snap.MeetingsWithMinutes + " / " + snap.Meetings;
            litGapDecisions.Text = snap.ImplementedDecisions + " / " + snap.TotalDecisions;
            litGapEvents.Text = snap.EventProgressPercent + "%";

            SetBar(txImplementedBar, snap.ImplementedPercent);
            SetBar(txEventBar, snap.EventProgressPercent);
            SetBar(txMinutesBar, snap.MinutesPercent);
            SetBar(txGapMinutesBar, snap.MinutesPercent);
            SetBar(txGapDecisionsBar, snap.ImplementedPercent);
            SetBar(txGapEventsBar, snap.EventProgressPercent);

            rptDecisions.DataSource = snap.Decisions;
            rptDecisions.DataBind();
            pnlDecisionsEmpty.Visible = snap.Decisions.Count == 0;

            rptEvents.DataSource = snap.Events;
            rptEvents.DataBind();
            pnlEventsEmpty.Visible = snap.Events.Count == 0;

            rptMeetings.DataSource = snap.MeetingsList;
            rptMeetings.DataBind();
            pnlMeetingsEmpty.Visible = snap.MeetingsList.Count == 0;
        }

        private static void SetBar(System.Web.UI.HtmlControls.HtmlGenericControl bar, int percent)
        {
            bar.Style["width"] = percent + "%";
        }
    }
}
