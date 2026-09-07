using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Decisions
{
    public partial class Decisions : System.Web.UI.Page
    {
        private string connectionString;
        private const int PageSize = 10;
        private int CurrentPage
        {
            get { return ViewState["CurrentPage"] != null ? (int)ViewState["CurrentPage"] : 1; }
            set { ViewState["CurrentPage"] = value; }
        }
        private int TotalRecords
        {
            get { return ViewState["TotalRecords"] != null ? (int)ViewState["TotalRecords"] : 0; }
            set { ViewState["TotalRecords"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            connectionString = ConfigurationManager.ConnectionStrings["DBConnection"].ConnectionString;

            if (Session["UserID"] == null)
            {
                Response.Redirect("~/Modules/Authentication/Login.aspx");
                return;
            }

            string role = Session["Role"] as string;
            if (role == null || !role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                Response.Redirect("~/Modules/Authentication/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                CurrentPage = 1;
                LoadDecisions();
                LoadStats();
            }
        }

        private string BuildBaseQuery(string search, string status, string priority, out List<SqlParameter> parameters)
        {
            parameters = new List<SqlParameter>();
            string query = @"SELECT d.DecisionID, d.DecisionTitle, d.Description, d.Priority, d.Status, 
                                    d.ResponsibleUserID, d.DueDate, d.CreatedAt, d.UpdatedAt,
                                    ISNULL(u.FullName, 'Unassigned') AS ResponsiblePerson,
                                    CASE WHEN ISNULL(d.IsRestricted, 0) = 1 OR ISNULL(e.IsDisabled, 0) = 1 THEN 1 ELSE 0 END AS IsRestricted
                             FROM Decisions d
                             LEFT JOIN Users u ON d.ResponsibleUserID = u.UserID
                             LEFT JOIN Events e ON e.EventID = d.EventID
                             WHERE 1=1";

            if (!string.IsNullOrWhiteSpace(search))
            {
                query += " AND d.DecisionTitle LIKE @Search";
                parameters.Add(new SqlParameter("@Search", "%" + search.Trim() + "%"));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query += " AND d.Status = @Status";
                parameters.Add(new SqlParameter("@Status", status.Trim()));
            }

            if (!string.IsNullOrWhiteSpace(priority))
            {
                query += " AND d.Priority = @Priority";
                parameters.Add(new SqlParameter("@Priority", priority.Trim()));
            }

            return query;
        }

        private void LoadDecisions(string search = "", string status = "", string priority = "")
        {
            List<SqlParameter> parameters;
            string baseQuery = BuildBaseQuery(search, status, priority, out parameters);

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                SqlCommand countCmd = new SqlCommand("SELECT COUNT(*) FROM (" + baseQuery + ") AS CountQuery", con);
                foreach (var p in parameters) countCmd.Parameters.AddWithValue(p.ParameterName, p.Value);
                TotalRecords = (int)countCmd.ExecuteScalar();

                int totalPages = (int)Math.Ceiling((double)TotalRecords / PageSize);
                if (CurrentPage > totalPages) CurrentPage = Math.Max(1, totalPages);
                if (CurrentPage < 1) CurrentPage = 1;

                string pagedQuery = baseQuery + " ORDER BY d.CreatedAt DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
                SqlCommand cmd = new SqlCommand(pagedQuery, con);
                foreach (var p in parameters) cmd.Parameters.AddWithValue(p.ParameterName, p.Value);
                cmd.Parameters.AddWithValue("@Offset", (CurrentPage - 1) * PageSize);
                cmd.Parameters.AddWithValue("@PageSize", PageSize);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                rptDecisions.DataSource = dt;
                rptDecisions.DataBind();

                pnlNoDecisions.Visible = dt.Rows.Count == 0;

                int totalFiltered = TotalRecords;
                litDecisionCount.Text = $"<span class='font-label-md text-label-md text-on-surface-variant'>{totalFiltered} decision{(totalFiltered != 1 ? "s" : "")}</span>";

                BindPagination(totalPages);
            }
        }

        private void BindPagination(int totalPages)
        {
            if (totalPages <= 1)
            {
                pnlPagination.Visible = false;
                return;
            }

            pnlPagination.Visible = true;
            litPageInfo.Text = $"Showing {((CurrentPage - 1) * PageSize) + 1}-{Math.Min(CurrentPage * PageSize, TotalRecords)} of {TotalRecords}";
            litCurrentPage.Value = CurrentPage.ToString();

            List<dynamic> pages = new List<dynamic>();
            for (int i = 1; i <= totalPages; i++)
            {
                pages.Add(new { Page = i });
            }
            rptPager.DataSource = pages;
            rptPager.DataBind();

            btnFirst.Enabled = CurrentPage > 1;
            btnPrev.Enabled = CurrentPage > 1;
            btnNext.Enabled = CurrentPage < totalPages;
            btnLast.Enabled = CurrentPage < totalPages;
        }

        private void LoadStats()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                SqlCommand cmdTotal = new SqlCommand("SELECT COUNT(*) FROM Decisions", con);
                litTotalDecisions.Text = cmdTotal.ExecuteScalar().ToString();

                SqlCommand cmdProposed = new SqlCommand("SELECT COUNT(*) FROM Decisions WHERE Status = 'Proposed'", con);
                litProposed.Text = cmdProposed.ExecuteScalar().ToString();

                SqlCommand cmdReview = new SqlCommand("SELECT COUNT(*) FROM Decisions WHERE Status = 'UnderReview'", con);
                litUnderReview.Text = cmdReview.ExecuteScalar().ToString();

                SqlCommand cmdApproved = new SqlCommand("SELECT COUNT(*) FROM Decisions WHERE Status = 'Approved'", con);
                litApproved.Text = cmdApproved.ExecuteScalar().ToString();

                SqlCommand cmdCompleted = new SqlCommand("SELECT COUNT(*) FROM Decisions WHERE Status = 'Completed'", con);
                litCompleted.Text = cmdCompleted.ExecuteScalar().ToString();
            }
        }

        protected void rptDecisions_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                Label lblStatus = (Label)e.Item.FindControl("lblStatus");
                if (lblStatus != null)
                {
                    string status = DataBinder.Eval(e.Item.DataItem, "Status").ToString();
                    lblStatus.Text = GetStatusDisplayText(status);
                    lblStatus.CssClass += " " + GetStatusBadgeClass(status);
                }

                Label lblPriority = (Label)e.Item.FindControl("lblPriority");
                if (lblPriority != null)
                {
                    string priority = DataBinder.Eval(e.Item.DataItem, "Priority").ToString();
                    lblPriority.Text = GetPriorityDisplayText(priority);
                    lblPriority.CssClass += " " + GetPriorityBadgeClass(priority);
                }
            }
        }

        protected void rptDecisions_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int decisionId;
            if (!int.TryParse(e.CommandArgument.ToString(), out decisionId))
                return;

            switch (e.CommandName)
            {
                case "ViewDecision":
                    Response.Redirect($"~/Modules/Decisions/DecisionDetails.aspx?DecisionID={decisionId}");
                    break;
                case "RestrictDecision":
                    RestrictionService.SetDecisionRestricted(decisionId, Convert.ToInt32(Session["UserID"]), Session["Role"] as string, true);
                    LoadDecisions(txtSearch.Text.Trim(), ddlStatusFilter.SelectedValue, ddlPriorityFilter.SelectedValue);
                    LoadStats();
                    break;
                case "RestoreDecision":
                    RestrictionService.SetDecisionRestricted(decisionId, Convert.ToInt32(Session["UserID"]), Session["Role"] as string, false);
                    LoadDecisions(txtSearch.Text.Trim(), ddlStatusFilter.SelectedValue, ddlPriorityFilter.SelectedValue);
                    LoadStats();
                    break;
            }
        }

        private void DeleteDecision(int decisionId)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("DELETE FROM Decisions WHERE DecisionID = @DecisionID", con);
                    cmd.Parameters.AddWithValue("@DecisionID", decisionId);
                    cmd.ExecuteNonQuery();
                }

                CurrentPage = 1;
                LoadDecisions(txtSearch.Text.Trim(), ddlStatusFilter.SelectedValue, ddlPriorityFilter.SelectedValue);
                LoadStats();
            }
            catch (Exception)
            {
                // FK constraint may prevent deletion if tasks reference this decision
                // Silently fail or show error - for now just reload
                LoadDecisions(txtSearch.Text.Trim(), ddlStatusFilter.SelectedValue, ddlPriorityFilter.SelectedValue);
            }
        }

        private string GetStatusBadgeClass(string status)
        {
            switch (status)
            {
                case "Proposed":
                    return "bg-[rgba(1,87,155,0.12)] text-[#01579b]";
                case "UnderReview":
                    return "bg-[rgba(230,126,0,0.12)] text-[#e67e00]";
                case "Approved":
                    return "bg-[rgba(46,125,50,0.12)] text-[#2e7d32]";
                case "Rejected":
                    return "bg-[rgba(211,47,47,0.12)] text-[#d32f2f]";
                case "Completed":
                    return "bg-[rgba(39,165,119,0.12)] text-[#27a577]";
                default:
                    return "bg-surface-container-high text-on-surface-variant";
            }
        }

        private string GetStatusDisplayText(string status)
        {
            switch (status)
            {
                case "Proposed":
                    return "Proposed";
                case "UnderReview":
                    return "Under Review";
                case "Approved":
                    return "Approved";
                case "Rejected":
                    return "Rejected";
                case "Completed":
                    return "Completed";
                default:
                    return status;
            }
        }

        private string GetPriorityBadgeClass(string priority)
        {
            switch (priority)
            {
                case "High":
                    return "bg-[rgba(211,47,47,0.12)] text-[#d32f2f]";
                case "Critical":
                    return "bg-[rgba(211,47,47,0.2)] text-[#b71c1c]";
                case "Medium":
                    return "bg-[rgba(230,126,0,0.12)] text-[#e67e00]";
                case "Low":
                    return "bg-[rgba(1,87,155,0.12)] text-[#01579b]";
                default:
                    return "bg-surface-container-high text-on-surface-variant";
            }
        }

        private string GetPriorityDisplayText(string priority)
        {
            switch (priority)
            {
                case "High":
                    return "High";
                case "Critical":
                    return "Critical";
                case "Medium":
                    return "Medium";
                case "Low":
                    return "Low";
                default:
                    return priority;
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            CurrentPage = 1;
            LoadDecisions(txtSearch.Text.Trim(), ddlStatusFilter.SelectedValue, ddlPriorityFilter.SelectedValue);
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            txtSearch.Text = string.Empty;
            ddlStatusFilter.SelectedValue = string.Empty;
            ddlPriorityFilter.SelectedValue = string.Empty;
            CurrentPage = 1;
            LoadDecisions();
        }

        protected void btnFirst_Click(object sender, EventArgs e)
        {
            CurrentPage = 1;
            LoadDecisions(txtSearch.Text.Trim(), ddlStatusFilter.SelectedValue, ddlPriorityFilter.SelectedValue);
        }

        protected void btnPrev_Click(object sender, EventArgs e)
        {
            CurrentPage--;
            LoadDecisions(txtSearch.Text.Trim(), ddlStatusFilter.SelectedValue, ddlPriorityFilter.SelectedValue);
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            CurrentPage++;
            LoadDecisions(txtSearch.Text.Trim(), ddlStatusFilter.SelectedValue, ddlPriorityFilter.SelectedValue);
        }

        protected void btnLast_Click(object sender, EventArgs e)
        {
            int totalPages = (int)Math.Ceiling((double)TotalRecords / PageSize);
            CurrentPage = totalPages;
            LoadDecisions(txtSearch.Text.Trim(), ddlStatusFilter.SelectedValue, ddlPriorityFilter.SelectedValue);
        }

        protected void rptPager_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Page")
            {
                int page;
                if (int.TryParse(e.CommandArgument.ToString(), out page))
                {
                    CurrentPage = page;
                    LoadDecisions(txtSearch.Text.Trim(), ddlStatusFilter.SelectedValue, ddlPriorityFilter.SelectedValue);
                }
            }
        }
    }
}
