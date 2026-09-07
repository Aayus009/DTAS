using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DigitalTransparencySystem.Modules.UserPolls
{
    public partial class UserPolls : System.Web.UI.Page
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

            if (!IsPostBack)
            {
                LoadActivePolls();
                LoadPastPolls();
            }
        }

        protected bool UserHasVoted(object value)
        {
            if (value == null || value == DBNull.Value)
                return false;
            if (value is bool)
                return (bool)value;

            int count;
            if (int.TryParse(value.ToString(), out count))
                return count > 0;

            bool flag;
            return bool.TryParse(value.ToString(), out flag) && flag;
        }

        private void LoadActivePolls()
        {
            int userId = Convert.ToInt32(Session["UserID"]);

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
                    @"SELECT p.PollID, p.PollTitle, p.Description, p.EndDate, p.IsActive, p.CreatedAt,
                        CAST(CASE WHEN EXISTS (
                            SELECT 1 FROM Votes v WHERE v.PollID = p.PollID AND v.UserID = @UserID
                        ) THEN 1 ELSE 0 END AS BIT) AS HasVoted,
                        ISNULL(e.EventName, '') AS RelatedEvent,
                        ISNULL(d.DecisionTitle, '') AS RelatedDecision
                      FROM Polls p
                      LEFT JOIN Events e ON p.EventID = e.EventID
                      LEFT JOIN Decisions d ON p.DecisionID = d.DecisionID
                      WHERE p.IsActive = 1 AND (p.EndDate IS NULL OR p.EndDate >= GETDATE())
                      ORDER BY p.CreatedAt DESC", con);
                cmd.Parameters.AddWithValue("@UserID", userId);

                DataTable dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                dt.Columns.Add("Options", typeof(object));
                dt.Columns.Add("ContextLabel", typeof(string));

                foreach (DataRow row in dt.Rows)
                {
                    row["Options"] = GetPollOptions(Convert.ToInt32(row["PollID"]));
                    row["ContextLabel"] = BuildContextLabel(row);
                }

                rptActivePolls.DataSource = dt;
                rptActivePolls.DataBind();
                pnlNoActivePolls.Visible = dt.Rows.Count == 0;
            }
        }

        private static string BuildContextLabel(DataRow row)
        {
            string eventName = row.Table.Columns.Contains("RelatedEvent") ? Convert.ToString(row["RelatedEvent"]) : "";
            string decision = row.Table.Columns.Contains("RelatedDecision") ? Convert.ToString(row["RelatedDecision"]) : "";
            if (!string.IsNullOrEmpty(eventName) && !string.IsNullOrEmpty(decision))
                return "Event: " + eventName + " - Decision: " + decision;
            if (!string.IsNullOrEmpty(eventName))
                return "Event: " + eventName;
            if (!string.IsNullOrEmpty(decision))
                return "Decision: " + decision;
            return "Community poll";
        }

        private DataTable GetPollOptions(int pollId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
                    @"SELECT o.OptionID, o.PollID, o.OptionText,
                             (SELECT COUNT(*) FROM Votes v WHERE v.OptionID = o.OptionID) AS VoteCount
                      FROM PollOptions o
                      WHERE o.PollID = @PollID
                      ORDER BY o.OptionID", con);
                cmd.Parameters.AddWithValue("@PollID", pollId);

                DataTable dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                dt.Columns.Add("Percentage", typeof(double));

                int totalVotes = 0;
                foreach (DataRow row in dt.Rows)
                    totalVotes += Convert.ToInt32(row["VoteCount"] == DBNull.Value ? 0 : row["VoteCount"]);

                foreach (DataRow row in dt.Rows)
                {
                    int count = Convert.ToInt32(row["VoteCount"] == DBNull.Value ? 0 : row["VoteCount"]);
                    row["Percentage"] = totalVotes > 0 ? Math.Round((double)count / totalVotes * 100) : 0;
                }

                return dt;
            }
        }

        private void LoadPastPolls()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(
                    @"SELECT p.PollID, p.PollTitle, p.Description, p.EndDate, p.IsActive
                      FROM Polls p
                      WHERE p.IsActive = 0 OR (p.EndDate IS NOT NULL AND p.EndDate < GETDATE())
                      ORDER BY p.EndDate DESC", con);

                DataTable dt = new DataTable();
                new SqlDataAdapter(cmd).Fill(dt);
                dt.Columns.Add("Options", typeof(object));

                foreach (DataRow row in dt.Rows)
                    row["Options"] = GetPollOptions(Convert.ToInt32(row["PollID"]));

                rptPastPolls.DataSource = dt;
                rptPastPolls.DataBind();
            }
        }

        protected void rptActivePolls_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "Vote")
                return;

            int pollId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out pollId))
                return;

            int userId = Convert.ToInt32(Session["UserID"]);
            string selectedOptionId = FindSelectedOption(pollId);
            int optionId;
            if (string.IsNullOrEmpty(selectedOptionId) || !int.TryParse(selectedOptionId, out optionId))
            {
                ShowVoteMessage("Choose an option before you vote.", false);
                LoadActivePolls();
                LoadPastPolls();
                return;
            }

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                using (SqlTransaction transaction = con.BeginTransaction())
                {
                    try
                    {
                        SqlCommand cmdCheck = new SqlCommand(
                            "SELECT COUNT(*) FROM Votes WHERE PollID = @PollID AND UserID = @UserID", con, transaction);
                        cmdCheck.Parameters.AddWithValue("@PollID", pollId);
                        cmdCheck.Parameters.AddWithValue("@UserID", userId);
                        if (Convert.ToInt32(cmdCheck.ExecuteScalar()) > 0)
                        {
                            transaction.Rollback();
                            ShowVoteMessage("You have already voted on this poll.", false);
                            LoadActivePolls();
                            LoadPastPolls();
                            return;
                        }

                        SqlCommand cmdBelong = new SqlCommand(
                            "SELECT COUNT(*) FROM PollOptions WHERE OptionID = @OptionID AND PollID = @PollID", con, transaction);
                        cmdBelong.Parameters.AddWithValue("@OptionID", optionId);
                        cmdBelong.Parameters.AddWithValue("@PollID", pollId);
                        if (Convert.ToInt32(cmdBelong.ExecuteScalar()) == 0)
                        {
                            transaction.Rollback();
                            ShowVoteMessage("That option is not part of this poll.", false);
                            LoadActivePolls();
                            LoadPastPolls();
                            return;
                        }

                        SqlCommand cmdVote = new SqlCommand(
                            "INSERT INTO Votes (PollID, OptionID, UserID) VALUES (@PollID, @OptionID, @UserID)", con, transaction);
                        cmdVote.Parameters.AddWithValue("@PollID", pollId);
                        cmdVote.Parameters.AddWithValue("@OptionID", optionId);
                        cmdVote.Parameters.AddWithValue("@UserID", userId);
                        cmdVote.ExecuteNonQuery();

                        SqlCommand cmdUpdate = new SqlCommand(
                            "UPDATE PollOptions SET VoteCount = (SELECT COUNT(*) FROM Votes WHERE OptionID = @OptionID) WHERE OptionID = @OptionID", con, transaction);
                        cmdUpdate.Parameters.AddWithValue("@OptionID", optionId);
                        cmdUpdate.ExecuteNonQuery();

                        transaction.Commit();
                        ShowVoteMessage("Your vote was recorded.", true);
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        ShowVoteMessage("Your vote could not be saved. Try again.", false);
                    }
                }
            }

            LoadActivePolls();
            LoadPastPolls();
        }

        private string FindSelectedOption(int pollId)
        {
            string exact = Request.Form["poll_" + pollId];
            if (!string.IsNullOrEmpty(exact))
                return exact;

            string suffix = "poll_" + pollId;
            string[] keys = Request.Form.AllKeys;
            if (keys == null)
                return null;

            foreach (string key in keys)
            {
                if (string.IsNullOrEmpty(key))
                    continue;
                if (key == suffix || key.EndsWith("$" + suffix, StringComparison.OrdinalIgnoreCase)
                    || key.EndsWith(":" + suffix, StringComparison.OrdinalIgnoreCase))
                    return Request.Form[key];
            }

            return null;
        }

        private void ShowVoteMessage(string text, bool success)
        {
            pnlVoteMessage.Visible = true;
            litVoteMessage.Text = text;
            pnlVoteMessage.CssClass = success
                ? "mb-6 p-4 rounded-xl bg-tertiary-container/30 text-on-tertiary-container"
                : "mb-6 p-4 rounded-xl bg-error-container/40 text-error";
        }
    }
}
