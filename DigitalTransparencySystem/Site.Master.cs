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
            bool marketing = IsMarketingPage();

            pnlPublicNavbar.Visible = !isAuthenticated || marketing;
            pnlPublicFooter.Visible = !isAuthenticated || marketing;

            bool showDashboardReturn = isAuthenticated && marketing;
            lnkSignIn.Visible = !isAuthenticated;
            lnkRegister.Visible = !isAuthenticated;
            lnkMobileSignIn.Visible = !isAuthenticated;
            lnkMobileRegister.Visible = !isAuthenticated;
            lnkBackToDashboard.Visible = showDashboardReturn;
            lnkMobileDashboard.Visible = showDashboardReturn;
            if (showDashboardReturn)
            {
                string url = ResolveUrl(AuthService.HomeDashboardUrl(Session));
                string label = AuthService.HomeDashboardLabel(Session);
                lnkBackToDashboard.NavigateUrl = url;
                lnkBackToDashboard.Text = label;
                lnkMobileDashboard.NavigateUrl = url;
                lnkMobileDashboard.Text = label;
            }

            if (isAuthenticated)
                TimelineService.RefreshOverdueStatuses();
        }

        private bool IsMarketingPage()
        {
            string path = (Request.Path ?? "").Replace("\\", "/").TrimEnd('/').ToLowerInvariant();
            if (path.EndsWith(".aspx"))
                path = path.Substring(0, path.Length - 5);
            if (string.IsNullOrEmpty(path) || path == "/")
                return true;
            return path == "/default"
                || path == "/about"
                || path == "/features"
                || path == "/faq"
                || path == "/contact"
                || path == "/demo";
        }
    }
}
