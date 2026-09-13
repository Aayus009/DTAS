using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Clubs
{
    public partial class Clubs : System.Web.UI.Page
    {
        private string connectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            connectionString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;

            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Modules/Authentication/Login.aspx");
                return;
            }

            string role = Session["Role"] as string;
            if (!RoleAccess.IsAdmin(role))
            {
                Response.Redirect("~/Modules/Authentication/Login.aspx");
                return;
            }

            RestrictionService.EnsureSchema();
            ClubService.EnsureMembershipSchema();

            if (!IsPostBack)
                LoadClubs();
        }

        private void LoadClubs()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
                    @"SELECT c.ClubID, c.ClubName, c.Description, c.LeadUserID,
                             ISNULL(u.FullName, 'No lead') AS LeadName,
                             ISNULL(c.IsRestricted, 0) AS IsRestricted,
                             (SELECT COUNT(*) FROM ClubMembers m
                              WHERE m.ClubID = c.ClubID
                                AND ISNULL(m.IsActive, 1) = 1 AND ISNULL(m.InviteStatus, N'Accepted') = N'Accepted') AS MemberCount,
                             (SELECT COUNT(*) FROM ContentReports cr
                              WHERE cr.TargetType = N'Club' AND cr.TargetID = c.ClubID
                                AND cr.Status IN (N'Pending', N'Reviewing', N'Escalated')) AS FlagCount
                      FROM Clubs c
                      LEFT JOIN Users u ON c.LeadUserID = u.UserID
                      WHERE ISNULL(c.IsDeleted, 0) = 0
                      ORDER BY ISNULL(c.IsRestricted, 0) DESC, c.ClubName", con);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                rptClubs.DataSource = dt;
                rptClubs.DataBind();
                pnlNoClubs.Visible = dt.Rows.Count == 0;

                litClubCount.Text = "<span class='font-label-md text-label-md text-on-surface-variant'>"
                    + dt.Rows.Count + " club" + (dt.Rows.Count != 1 ? "s" : "") + "</span>";
            }
        }

        protected void rptClubs_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                Label lblLead = (Label)e.Item.FindControl("lblLead");
                if (lblLead != null)
                {
                    lblLead.Text = DataBinder.Eval(e.Item.DataItem, "LeadName").ToString();
                    lblLead.CssClass = DataBinder.Eval(e.Item.DataItem, "LeadUserID") == DBNull.Value
                        ? "text-sm text-on-surface-variant"
                        : "text-sm font-semibold text-on-surface";
                }

                Literal litMemberCount = (Literal)e.Item.FindControl("litMemberCount");
                if (litMemberCount != null)
                    litMemberCount.Text = DataBinder.Eval(e.Item.DataItem, "MemberCount").ToString();
            }
        }

        protected void rptClubs_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int clubId;
            if (!int.TryParse(e.CommandArgument.ToString(), out clubId))
                return;

            int adminId = Convert.ToInt32(Session["UserID"]);
            string role = Session["Role"] as string;

            switch (e.CommandName)
            {
                case "ViewMembers":
                    ShowMembers(clubId);
                    break;
                case "Restrict":
                    ShowMessage(RestrictionService.SetClubRestricted(clubId, adminId, role, true),
                        "Club restricted. Members cannot perform club activity until it is restored.");
                    LoadClubs();
                    break;
                case "Restore":
                    ShowMessage(RestrictionService.SetClubRestricted(clubId, adminId, role, false),
                        "Club restored. Members can resume club activity.");
                    LoadClubs();
                    break;
                case "Delete":
                    DeleteClub(clubId);
                    break;
            }
        }

        private void DeleteClub(int clubId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    "UPDATE Clubs SET IsDeleted = 1, IsActive = 0 WHERE ClubID = @ClubID", con);
                cmd.Parameters.AddWithValue("@ClubID", clubId);
                cmd.ExecuteNonQuery();
            }
            ShowMessage(null, "Club archived.");
            LoadClubs();
        }

        private void ShowMembers(int clubId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    @"SELECT m.MembershipID, u.FullName, r.RoleName,
                             CASE WHEN c.LeadUserID = m.UserID THEN ' (Lead)' ELSE '' END AS IsLead
                      FROM ClubMembers m
                      INNER JOIN Users u ON m.UserID = u.UserID
                      INNER JOIN Roles r ON u.RoleID = r.RoleID
                      LEFT JOIN Clubs c ON c.ClubID = m.ClubID
                      WHERE m.ClubID = @ClubID
                        AND ISNULL(m.IsActive, 1) = 1 AND ISNULL(m.InviteStatus, N'Accepted') = N'Accepted'
                      ORDER BY u.FullName", con);
                cmd.Parameters.AddWithValue("@ClubID", clubId);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                rptMembers.DataSource = dt;
                rptMembers.DataBind();
                pnlNoMembers.Visible = dt.Rows.Count == 0;
                litMembersTitle.Text = dt.Rows.Count + " member(s)";
                pnlMembers.Visible = true;
            }
        }

        protected void btnCloseMembers_Click(object sender, EventArgs e)
        {
            pnlMembers.Visible = false;
        }

        private void ShowMessage(string error, string ok)
        {
            UiNotice.BindPanel(pnlMessage, lblMessage, error, ok);
        }
    }
}
