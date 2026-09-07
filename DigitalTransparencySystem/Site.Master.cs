using System;
using System.Web.UI;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem
{
    public partial class SiteMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string role = Session["Role"] as string;
            bool isAuthenticated = !string.IsNullOrEmpty(role);

            pnlPublicNavbar.Visible = !isAuthenticated;
            pnlPublicFooter.Visible = !isAuthenticated;

            if (isAuthenticated)
                TimelineService.RefreshOverdueStatuses();
        }
    }
}
