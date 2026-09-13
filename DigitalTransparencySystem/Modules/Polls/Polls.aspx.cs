using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using DigitalTransparencySystem.Helpers;

namespace DigitalTransparencySystem.Modules.Polls
{
    public partial class Polls : System.Web.UI.Page
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

            if (!RoleAccess.IsAdmin(Session["Role"] as string))
            {
                Response.Redirect("~/Modules/Authentication/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadLookups();
                PrefillFromQuery();
                LoadPolls();
                int viewId;
                if (int.TryParse(Request.QueryString["PollID"], out viewId))
                    ShowPollDetails(viewId);
            }
        }

        private void LoadLookups()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                SqlCommand cmdEvents = new SqlCommand(
                    @"SELECT EventID, EventName FROM Events
                      WHERE Status IN ('Proposed', 'Planned', 'Ongoing', 'InProgress', 'Approved')
                      ORDER BY EventName", con);
                using (SqlDataReader reader = cmdEvents.ExecuteReader())
                {
                    ddlEvent.DataSource = reader;
                    ddlEvent.DataTextField = "EventName";
                    ddlEvent.DataValueField = "EventID";
                    ddlEvent.DataBind();
                }

                SqlCommand cmdDecisions = new SqlCommand(
                    @"SELECT DecisionID, DecisionTitle FROM Decisions
                      WHERE Status NOT IN ('Rejected', 'Cancelled', 'Completed', 'Closed')
                      ORDER BY DecisionTitle", con);
                using (SqlDataReader reader = cmdDecisions.ExecuteReader())
                {
                    ddlDecision.DataSource = reader;
                    ddlDecision.DataTextField = "DecisionTitle";
                    ddlDecision.DataValueField = "DecisionID";
                    ddlDecision.DataBind();
                }
            }

            ddlEvent.Items.Insert(0, new ListItem("- Not linked to an event -", ""));
            ddlDecision.Items.Insert(0, new ListItem("- Not linked to a decision -", ""));
        }

        private void PrefillFromQuery()
        {
            string eventId = Request.QueryString["EventID"];
            string decisionId = Request.QueryString["DecisionID"];

            if (!string.IsNullOrEmpty(eventId) && ddlEvent.Items.FindByValue(eventId) != null)
            {
                ddlEvent.SelectedValue = eventId;
                pnlForm.Visible = true;
            }

            if (!string.IsNullOrEmpty(decisionId) && ddlDecision.Items.FindByValue(decisionId) != null)
            {
                ddlDecision.SelectedValue = decisionId;
                pnlForm.Visible = true;
            }
        }

        private void LoadPolls()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
                    @"SELECT p.PollID, p.PollTitle, p.Description, p.EndDate, p.IsActive,
                             ISNULL(e.EventName, '') AS EventName,
                             ISNULL(d.DecisionTitle, '') AS DecisionTitle,
                             (SELECT COUNT(*) FROM Votes v WHERE v.PollID = p.PollID) AS VoteCount
                      FROM Polls p
                      LEFT JOIN Events e ON p.EventID = e.EventID
                      LEFT JOIN Decisions d ON p.DecisionID = d.DecisionID
                      ORDER BY p.CreatedAt DESC", con);

                DataTable dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                dt.Columns.Add("LinkedTo", typeof(string));
                dt.Columns.Add("IsOpen", typeof(bool));
                dt.Columns.Add("StatusLabel", typeof(string));

                foreach (DataRow row in dt.Rows)
                {
                    bool active = row["IsActive"] != DBNull.Value && Convert.ToBoolean(row["IsActive"]);
                    bool notEnded = row["EndDate"] == DBNull.Value || Convert.ToDateTime(row["EndDate"]) >= DateTime.Today;
                    bool isOpen = active && notEnded;
                    row["IsOpen"] = isOpen;
                    row["StatusLabel"] = isOpen ? "Open" : "Closed";

                    string eventName = row["EventName"].ToString();
                    string decision = row["DecisionTitle"].ToString();
                    if (!string.IsNullOrEmpty(eventName) && !string.IsNullOrEmpty(decision))
                        row["LinkedTo"] = eventName + " / " + decision;
                    else if (!string.IsNullOrEmpty(eventName))
                        row["LinkedTo"] = eventName;
                    else if (!string.IsNullOrEmpty(decision))
                        row["LinkedTo"] = decision;
                    else
                        row["LinkedTo"] = "General";
                }

                rptPolls.DataSource = dt;
                rptPolls.DataBind();
                pnlNoPolls.Visible = dt.Rows.Count == 0;
            }
        }

        protected void btnNewPoll_Click(object sender, EventArgs e)
        {
            ClearForm();
            pnlForm.Visible = true;
            pnlDetails.Visible = false;
            ViewState["ViewPollID"] = null;
            pnlMessage.Visible = false;
        }

        protected void btnCancelPoll_Click(object sender, EventArgs e)
        {
            ClearForm();
            pnlForm.Visible = false;
        }

        protected void btnSavePoll_Click(object sender, EventArgs e)
        {
            string title = (txtTitle.Text ?? "").Trim();
            if (string.IsNullOrEmpty(title))
            {
                ShowMessage("Add a poll question.", false);
                return;
            }

            List<string> options = new List<string>();
            foreach (string raw in new[] { txtOption1.Text, txtOption2.Text, txtOption3.Text, txtOption4.Text, txtOption5.Text, txtOption6.Text })
            {
                string option = (raw ?? "").Trim();
                if (!string.IsNullOrEmpty(option))
                    options.Add(option);
            }

            if (options.Count < 2)
            {
                ShowMessage("Add at least two options.", false);
                return;
            }

            DateTime? endDate = null;
            DateTime parsedEnd;
            if (!string.IsNullOrWhiteSpace(txtEndDate.Text) && DateTime.TryParse(txtEndDate.Text, out parsedEnd))
                endDate = parsedEnd;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlTransaction tx = con.BeginTransaction();
                try
                {
                    SqlCommand cmdPoll = new SqlCommand(
                        @"INSERT INTO Polls (PollTitle, Description, StartDate, EndDate, IsActive, CreatedBy, CreatedAt, EventID, DecisionID)
                          VALUES (@Title, @Description, GETDATE(), @EndDate, 1, @CreatedBy, GETDATE(), @EventID, @DecisionID);
                          SELECT CAST(SCOPE_IDENTITY() AS INT);", con, tx);
                    cmdPoll.Parameters.AddWithValue("@Title", title);
                    cmdPoll.Parameters.AddWithValue("@Description", (object)(txtDescription.Text ?? "").Trim() ?? DBNull.Value);
                    cmdPoll.Parameters.AddWithValue("@EndDate", (object)endDate ?? DBNull.Value);
                    cmdPoll.Parameters.AddWithValue("@CreatedBy", Convert.ToInt32(Session["UserID"]));
                    cmdPoll.Parameters.AddWithValue("@EventID", string.IsNullOrEmpty(ddlEvent.SelectedValue) ? (object)DBNull.Value : int.Parse(ddlEvent.SelectedValue));
                    cmdPoll.Parameters.AddWithValue("@DecisionID", string.IsNullOrEmpty(ddlDecision.SelectedValue) ? (object)DBNull.Value : int.Parse(ddlDecision.SelectedValue));
                    int pollId = Convert.ToInt32(cmdPoll.ExecuteScalar());

                    foreach (string option in options)
                    {
                        SqlCommand cmdOpt = new SqlCommand(
                            "INSERT INTO PollOptions (PollID, OptionText, VoteCount) VALUES (@PollID, @OptionText, 0)", con, tx);
                        cmdOpt.Parameters.AddWithValue("@PollID", pollId);
                        cmdOpt.Parameters.AddWithValue("@OptionText", option);
                        cmdOpt.ExecuteNonQuery();
                    }

                    SqlCommand cmdNotify = new SqlCommand(
                        @"INSERT INTO Notifications (UserID, Title, Message, IsRead, NotificationType, RelatedID, RelatedType, CreatedAt)
                          SELECT UserID, @Title, @Message, 0, 'Poll', @RelatedID, 'Poll', GETDATE()
                          FROM Users WHERE ISNULL(IsActive, 1) = 1", con, tx);
                    cmdNotify.Parameters.AddWithValue("@Title", "New community poll");
                    cmdNotify.Parameters.AddWithValue("@Message", "A new poll is open: " + title + ". Cast your vote from Polls.");
                    cmdNotify.Parameters.AddWithValue("@RelatedID", pollId);
                    cmdNotify.ExecuteNonQuery();

                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }

            ClearForm();
            pnlForm.Visible = false;
            ShowMessage("Poll published. The community can vote from Polls and Transparency.", true);
            LoadPolls();
        }

        protected void rptPolls_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int pollId;
            if (!int.TryParse(e.CommandArgument.ToString(), out pollId))
                return;

            if (e.CommandName == "View")
            {
                pnlForm.Visible = false;
                ShowPollDetails(pollId);
                return;
            }

            if (e.CommandName != "Close" && e.CommandName != "Reopen")
                return;

            bool activate = e.CommandName == "Reopen";
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    "UPDATE Polls SET IsActive = @Active WHERE PollID = @PollID", con);
                cmd.Parameters.AddWithValue("@Active", activate);
                cmd.Parameters.AddWithValue("@PollID", pollId);
                cmd.ExecuteNonQuery();
            }

            ShowMessage(activate ? "Poll reopened." : "Poll closed.", true);
            LoadPolls();
            if (ViewState["ViewPollID"] != null && Convert.ToInt32(ViewState["ViewPollID"]) == pollId)
                ShowPollDetails(pollId);
        }

        protected void btnHideDetails_Click(object sender, EventArgs e)
        {
            pnlDetails.Visible = false;
            ViewState["ViewPollID"] = null;
        }

        private void ShowPollDetails(int pollId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    @"SELECT p.PollID, p.PollTitle, p.Description, p.EndDate, p.IsActive,
                             ISNULL(e.EventName, '') AS EventName,
                             ISNULL(d.DecisionTitle, '') AS DecisionTitle,
                             (SELECT COUNT(*) FROM Votes v WHERE v.PollID = p.PollID) AS VoteCount
                      FROM Polls p
                      LEFT JOIN Events e ON p.EventID = e.EventID
                      LEFT JOIN Decisions d ON p.DecisionID = d.DecisionID
                      WHERE p.PollID = @PollID", con);
                cmd.Parameters.AddWithValue("@PollID", pollId);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        pnlDetails.Visible = false;
                        ViewState["ViewPollID"] = null;
                        ShowMessage("That poll could not be found.", false);
                        return;
                    }

                    bool active = reader["IsActive"] != DBNull.Value && Convert.ToBoolean(reader["IsActive"]);
                    bool notEnded = reader["EndDate"] == DBNull.Value || Convert.ToDateTime(reader["EndDate"]) >= DateTime.Today;
                    string eventName = reader["EventName"].ToString();
                    string decision = reader["DecisionTitle"].ToString();
                    string linked = !string.IsNullOrEmpty(eventName) && !string.IsNullOrEmpty(decision)
                        ? eventName + " / " + decision
                        : (!string.IsNullOrEmpty(eventName) ? eventName : (!string.IsNullOrEmpty(decision) ? decision : "General"));

                    litDetailTitle.Text = Server.HtmlEncode(reader["PollTitle"].ToString());
                    string description = reader["Description"] == DBNull.Value ? "" : reader["Description"].ToString();
                    litDetailDescription.Text = string.IsNullOrWhiteSpace(description)
                        ? "No description provided."
                        : Server.HtmlEncode(description);
                    litDetailLinked.Text = Server.HtmlEncode(linked);
                    litDetailStatus.Text = active && notEnded ? "Open" : "Closed";
                    litDetailCloses.Text = reader["EndDate"] == DBNull.Value
                        ? "No close date"
                        : Convert.ToDateTime(reader["EndDate"]).ToString("MMM dd, yyyy");
                    litDetailVotes.Text = Convert.ToInt32(reader["VoteCount"]).ToString();
                }

                DataTable results = new DataTable();
                SqlCommand cmdResults = new SqlCommand(
                    @"SELECT o.OptionText,
                             (SELECT COUNT(*) FROM Votes v WHERE v.OptionID = o.OptionID) AS VoteCount
                      FROM PollOptions o
                      WHERE o.PollID = @PollID
                      ORDER BY o.OptionID", con);
                cmdResults.Parameters.AddWithValue("@PollID", pollId);
                new SqlDataAdapter(cmdResults).Fill(results);
                results.Columns.Add("Percentage", typeof(double));
                results.Columns.Add("IsLeading", typeof(bool));

                int total = 0;
                int maxVotes = 0;
                foreach (DataRow row in results.Rows)
                {
                    int count = Convert.ToInt32(row["VoteCount"] == DBNull.Value ? 0 : row["VoteCount"]);
                    total += count;
                    if (count > maxVotes) maxVotes = count;
                }
                foreach (DataRow row in results.Rows)
                {
                    int count = Convert.ToInt32(row["VoteCount"] == DBNull.Value ? 0 : row["VoteCount"]);
                    row["Percentage"] = total > 0 ? Math.Round((double)count / total * 100) : 0;
                    row["IsLeading"] = total > 0 && count == maxVotes && count > 0;
                    row["OptionText"] = Server.HtmlEncode(row["OptionText"].ToString());
                }

                rptResults.DataSource = results;
                rptResults.DataBind();
                pnlNoResults.Visible = total == 0;

                DataTable voters = new DataTable();
                SqlCommand cmdVoters = new SqlCommand(
                    @"SELECT ISNULL(NULLIF(LTRIM(RTRIM(u.FullName)), ''), ISNULL(u.Username, 'Unknown voter')) AS FullName,
                             ISNULL(u.Email, '') AS Email,
                             o.OptionText, v.VotedAt
                      FROM Votes v
                      LEFT JOIN Users u ON u.UserID = v.UserID
                      INNER JOIN PollOptions o ON o.OptionID = v.OptionID
                      WHERE v.PollID = @PollID
                      ORDER BY v.VotedAt DESC", con);
                cmdVoters.Parameters.AddWithValue("@PollID", pollId);
                new SqlDataAdapter(cmdVoters).Fill(voters);
                foreach (DataRow row in voters.Rows)
                {
                    row["FullName"] = Server.HtmlEncode(row["FullName"].ToString());
                    row["Email"] = Server.HtmlEncode(row["Email"].ToString());
                    row["OptionText"] = Server.HtmlEncode(row["OptionText"].ToString());
                }

                rptVoters.DataSource = voters;
                rptVoters.DataBind();
                pnlNoVoters.Visible = voters.Rows.Count == 0;
            }

            ViewState["ViewPollID"] = pollId;
            pnlDetails.Visible = true;
        }

        private void ClearForm()
        {
            txtTitle.Text = "";
            txtDescription.Text = "";
            txtEndDate.Text = "";
            txtOption1.Text = "";
            txtOption2.Text = "";
            txtOption3.Text = "";
            txtOption4.Text = "";
            txtOption5.Text = "";
            txtOption6.Text = "";
            if (ddlEvent.Items.Count > 0) ddlEvent.SelectedIndex = 0;
            if (ddlDecision.Items.Count > 0) ddlDecision.SelectedIndex = 0;
        }

        private void ShowMessage(string text, bool success)
        {
            UiNotice.BindPanel(pnlMessage, lblMessage, text, success);
        }
    }
}
