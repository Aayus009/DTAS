using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web;

namespace DigitalTransparencySystem.Helpers
{
    public class SearchHit
    {
        public string Type { get; set; }
        public string Group { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Url { get; set; }
        public string Icon { get; set; }
    }

    public static class SearchService
    {
        public static List<SearchHit> Search(int userId, string role, string query)
        {
            var hits = new List<SearchHit>();
            query = (query ?? "").Trim();
            if (query.Length < 2)
                return hits;

            bool isAdmin = RoleAccess.IsAdmin(role);
            string like = "%" + EscapeLike(query) + "%";

            using (var con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                TryAdd(delegate { AddPages(hits, role, query); });
                TryAdd(delegate { AddEvents(con, hits, userId, isAdmin, like); });
                TryAdd(delegate { AddTasks(con, hits, userId, isAdmin, like); });
                TryAdd(delegate { AddMeetings(con, hits, userId, isAdmin, like); });
                TryAdd(delegate { AddAssignments(con, hits, userId, role, like); });
                TryAdd(delegate { AddAssignmentGroups(con, hits, userId, isAdmin, like); });
                TryAdd(delegate { AddClubs(con, hits, userId, isAdmin, like); });
                TryAdd(delegate { AddConnectGroups(con, hits, userId, like); });
                TryAdd(delegate { AddPolls(con, hits, isAdmin, like); });
                if (isAdmin)
                    TryAdd(delegate { AddUsers(con, hits, like); });
            }

            return hits;
        }

        public static SearchHit BestMatch(int userId, string role, string query)
        {
            List<SearchHit> hits = Search(userId, role, query);
            if (hits == null || hits.Count == 0)
                return null;

            string q = (query ?? "").Trim();
            SearchHit pageExact = FirstMatch(hits, true, q, true, false);
            if (pageExact != null)
                return pageExact;
            SearchHit pageStart = FirstMatch(hits, true, q, false, true);
            if (pageStart != null)
                return pageStart;
            SearchHit pageHas = FirstMatch(hits, true, q, false, false);
            if (pageHas != null)
                return pageHas;
            SearchHit anyExact = FirstMatch(hits, false, q, true, false);
            if (anyExact != null)
                return anyExact;
            SearchHit anyStart = FirstMatch(hits, false, q, false, true);
            if (anyStart != null)
                return anyStart;
            return hits[0];
        }

        private static SearchHit FirstMatch(List<SearchHit> hits, bool pagesOnly, string query, bool exact, bool starts)
        {
            foreach (SearchHit hit in hits)
            {
                if (hit == null || string.IsNullOrEmpty(hit.Url))
                    continue;
                if (pagesOnly && !string.Equals(hit.Type, "Page", StringComparison.OrdinalIgnoreCase))
                    continue;
                string title = hit.Title ?? "";
                if (exact)
                {
                    if (title.Equals(query, StringComparison.OrdinalIgnoreCase))
                        return hit;
                    continue;
                }
                if (starts)
                {
                    if (title.StartsWith(query, StringComparison.OrdinalIgnoreCase))
                        return hit;
                    continue;
                }
                if (title.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                    return hit;
            }
            return null;
        }

        private static void TryAdd(Action add)
        {
            try
            {
                add();
            }
            catch
            {
            }
        }

        private static void AddEvents(SqlConnection con, List<SearchHit> hits, int userId, bool isAdmin, string like)
        {
            string sql = @"
                SELECT TOP 6 e.EventID, e.EventName, e.Status, ISNULL(e.Visibility, N'Private') AS Visibility
                FROM Events e
                WHERE ISNULL(e.IsDeleted, 0) = 0
                  AND (e.EventName LIKE @Q OR ISNULL(e.Description, N'') LIKE @Q)
                  AND (
                        @IsAdmin = 1
                        OR ISNULL(e.Visibility, N'Private') = N'Public'
                        OR EXISTS (
                            SELECT 1 FROM EventMembers em
                            WHERE em.EventID = e.EventID AND em.UserID = @UserID
                              AND em.IsActive = 1 AND em.InviteStatus = N'Accepted')
                  )
                ORDER BY e.EventName";

            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@Q", like);
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@IsAdmin", isAdmin ? 1 : 0);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = Convert.ToInt32(reader["EventID"]);
                        hits.Add(new SearchHit
                        {
                            Type = "Event",
                            Group = "Events",
                            Title = Convert.ToString(reader["EventName"]),
                            Subtitle = Convert.ToString(reader["Visibility"]) + " · " + Convert.ToString(reader["Status"]),
                            Icon = "event",
                            Url = ToUrl("~/Modules/Events/EventWorkspace.aspx?EventID=" + id)
                        });
                    }
                }
            }
        }

        private static void AddTasks(SqlConnection con, List<SearchHit> hits, int userId, bool isAdmin, string like)
        {
            string access = isAdmin ? "1 = 1" : TaskAccess.UserCanSeeTask;
            string sql = @"
                SELECT TOP 6 t.TaskID, t.TaskTitle, t.Status, ISNULL(e.EventName, N'') AS EventName
                FROM Tasks t
                LEFT JOIN Events e ON e.EventID = t.EventID
                WHERE ISNULL(t.IsDeleted, 0) = 0
                  AND t.Status <> N'Archived'
                  AND (t.TaskTitle LIKE @Q OR ISNULL(t.Description, N'') LIKE @Q)
                  AND (" + access + @")
                ORDER BY t.DueDate DESC, t.TaskID DESC";

            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@Q", like);
                cmd.Parameters.AddWithValue("@UserID", userId);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = Convert.ToInt32(reader["TaskID"]);
                        string eventName = Convert.ToString(reader["EventName"]);
                        string url = isAdmin
                            ? "~/Modules/Tasks/TaskDetails.aspx?TaskID=" + id
                            : "~/Modules/TaskWorkspaces/TaskWorkspace.aspx?TaskID=" + id;
                        hits.Add(new SearchHit
                        {
                            Type = "Task",
                            Group = "Tasks",
                            Title = Convert.ToString(reader["TaskTitle"]),
                            Subtitle = string.IsNullOrWhiteSpace(eventName)
                                ? Convert.ToString(reader["Status"])
                                : eventName + " · " + Convert.ToString(reader["Status"]),
                            Icon = "task_alt",
                            Url = ToUrl(url)
                        });
                    }
                }
            }
        }

        private static void AddMeetings(SqlConnection con, List<SearchHit> hits, int userId, bool isAdmin, string like)
        {
            MeetingService.EnsureSchema();
            string access = isAdmin
                ? "1 = 1"
                : @"m.CreatedBy = @UserID
                   OR EXISTS (SELECT 1 FROM MeetingParticipants mp WHERE mp.MeetingID = m.MeetingID AND mp.UserID = @UserID)
                   OR (m.EventID IS NOT NULL AND EXISTS (
                        SELECT 1 FROM EventMembers em
                        WHERE em.EventID = m.EventID AND em.UserID = @UserID
                          AND em.IsActive = 1 AND em.InviteStatus = N'Accepted'))";

            string sql = @"
                SELECT TOP 6 m.MeetingID, m.MeetingTitle, m.ScheduledDate, m.Status, ISNULL(e.EventName, N'') AS EventName
                FROM Meetings m
                LEFT JOIN Events e ON e.EventID = m.EventID
                WHERE (m.MeetingTitle LIKE @Q OR ISNULL(m.Description, N'') LIKE @Q OR ISNULL(e.EventName, N'') LIKE @Q)
                  AND (" + access + @")
                ORDER BY m.ScheduledDate DESC";

            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@Q", like);
                cmd.Parameters.AddWithValue("@UserID", userId);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = Convert.ToInt32(reader["MeetingID"]);
                        string eventName = Convert.ToString(reader["EventName"]);
                        string when = reader["ScheduledDate"] == DBNull.Value
                            ? Convert.ToString(reader["Status"])
                            : Convert.ToDateTime(reader["ScheduledDate"]).ToString("MMM dd, yyyy hh:mm tt");
                        string url = isAdmin
                            ? "~/Modules/Meetings/MeetingDetails.aspx?MeetingID=" + id
                            : "~/Modules/UserMeetings/UserMeetingDetails.aspx?MeetingID=" + id;
                        hits.Add(new SearchHit
                        {
                            Type = "Meeting",
                            Group = "Meetings",
                            Title = Convert.ToString(reader["MeetingTitle"]),
                            Subtitle = string.IsNullOrWhiteSpace(eventName) ? when : eventName + " · " + when,
                            Icon = "videocam",
                            Url = ToUrl(url)
                        });
                    }
                }
            }
        }

        private static void AddPages(List<SearchHit> hits, string role, string query)
        {
            bool admin = RoleAccess.IsAdmin(role);
            bool faculty = RoleAccess.CanCreateAssignments(role);
            bool student = RoleAccess.IsStudent(role);
            bool staff = RoleAccess.IsFacultyOrStaff(role);

            var pages = new[]
            {
                new { Title = "Dashboard", Hint = "Home overview", Keys = "dashboard home portal start", Icon = "dashboard", Url = admin ? "~/Modules/Dashboard/AdminDashboard.aspx" : "~/Modules/Dashboard/UsersDashboard.aspx", Show = true },
                new { Title = "Assignments", Hint = "My assignment groups", Keys = "assignment homework subgroup school", Icon = "school", Url = "~/Modules/Assignments/MyAssignments.aspx", Show = student || faculty },
                new { Title = "Faculty assignments", Hint = "Create and monitor assignments", Keys = "faculty assignment create college", Icon = "school", Url = "~/Modules/Assignments/Assignments.aspx", Show = faculty },
                new { Title = "Task workspaces", Hint = "Your event task work", Keys = "task workspace work", Icon = "workspaces", Url = "~/Modules/TaskWorkspaces/TaskWorkspaces.aspx", Show = !admin },
                new { Title = "Tasks", Hint = "Manage event tasks", Keys = "task list admin", Icon = "assignment", Url = "~/Modules/Tasks/Tasks.aspx", Show = admin },
                new { Title = "Events", Hint = admin ? "Manage campus events" : "Your events", Keys = "event calendar campus", Icon = "event", Url = admin ? "~/Modules/Events/Events.aspx" : "~/Modules/Events/MyEvents.aspx", Show = true },
                new { Title = "Campus events", Hint = "Public campus calendar", Keys = "campus event public browse", Icon = "campaign", Url = "~/Modules/Events/CampusEvents.aspx", Show = !admin },
                new { Title = "Create event", Hint = "Start a new event", Keys = "create event new", Icon = "add_circle", Url = "~/Modules/Events/CreateEvent.aspx", Show = RoleAccess.CanCreateEvents(role) && !admin },
                new { Title = "Propose event", Hint = "Suggest a campus event", Keys = "propose event student", Icon = "add_circle", Url = "~/Modules/Events/ProposeEvent.aspx", Show = student },
                new { Title = "Clubs", Hint = admin ? "Manage clubs" : "Your clubs", Keys = "club society group", Icon = "groups", Url = admin ? "~/Modules/Clubs/Clubs.aspx" : "~/Modules/Clubs/MyClubs.aspx", Show = true },
                new { Title = "Connect", Hint = "Group chat rooms", Keys = "connect chat message forum", Icon = "forum", Url = "~/Modules/Connect/Connect.aspx", Show = true },
                new { Title = "Meetings", Hint = admin ? "All meetings" : "Your meetings", Keys = "meeting zoom call schedule", Icon = "videocam", Url = admin ? "~/Modules/Meetings/Meetings.aspx" : "~/Modules/UserMeetings/UserMeetings.aspx", Show = true },
                new { Title = "Polls", Hint = "Vote and surveys", Keys = "poll vote survey", Icon = "how_to_vote", Url = admin ? "~/Modules/Polls/Polls.aspx" : "~/Modules/UserPolls/UserPolls.aspx", Show = true },
                new { Title = "Decisions", Hint = "Governance decisions", Keys = "decision governance vote", Icon = "gavel", Url = "~/Modules/Decisions/Decisions.aspx", Show = admin },
                new { Title = "Reports", Hint = "Accountability reports", Keys = "report analytics excel", Icon = "analytics", Url = admin ? "~/Modules/Reports/Reports.aspx" : "~/Modules/UserReports/UserReports.aspx", Show = admin || staff },
                new { Title = "Transparency", Hint = "Public progress board", Keys = "transparency public gist", Icon = "visibility", Url = admin ? "~/Modules/Transparency/Transparency.aspx" : "~/Transparency.aspx", Show = true },
                new { Title = "Users", Hint = "Accounts and roles", Keys = "user account member people", Icon = "manage_accounts", Url = "~/Modules/Users/Users.aspx", Show = admin },
                new { Title = "ID queue", Hint = "Identity verification", Keys = "identity verify id badge queue", Icon = "badge", Url = "~/Modules/Users/IdentityQueue.aspx", Show = admin },
                new { Title = "Verify ID", Hint = "Submit identity documents", Keys = "identity verify id badge document", Icon = "badge", Url = "~/Modules/Settings/IdentityVerification.aspx", Show = !admin },
                new { Title = "Flags", Hint = "Moderation flags", Keys = "flag report moderation", Icon = "flag", Url = "~/Modules/Users/Flags.aspx", Show = admin },
                new { Title = "Feedback", Hint = "Comments and feedback", Keys = "feedback comment", Icon = "feedback", Url = admin ? "~/Modules/Participation/Feedback.aspx" : "~/Modules/UserFeedback/UserFeedback.aspx", Show = true },
                new { Title = "Notifications", Hint = "Your alerts", Keys = "notification alert inbox", Icon = "notifications", Url = "~/Modules/Notifications/Notifications.aspx", Show = true },
                new { Title = "Settings", Hint = "Profile and account", Keys = "settings profile account password", Icon = "settings", Url = admin ? "~/Modules/Settings/AdminProfile.aspx" : "~/Modules/Settings/UserProfile.aspx", Show = true },
                new { Title = "FAQ", Hint = "Help and answers", Keys = "faq help question support", Icon = "help", Url = "~/FAQ.aspx", Show = true },
                new { Title = "About", Hint = "About DTAS", Keys = "about dtas info", Icon = "info", Url = "~/About.aspx", Show = true },
                new { Title = "Contact", Hint = "Contact DTAS", Keys = "contact email support", Icon = "mail", Url = "~/Contact.aspx", Show = true },
                new { Title = "Features", Hint = "What DTAS includes", Keys = "feature module capability", Icon = "auto_awesome", Url = "~/Features.aspx", Show = true }
            };

            int added = 0;
            foreach (var page in pages)
            {
                if (!page.Show || added >= 8)
                    continue;
                if (!Matches(query, page.Title, page.Hint, page.Keys))
                    continue;
                hits.Add(new SearchHit
                {
                    Type = "Page",
                    Group = "Pages",
                    Title = page.Title,
                    Subtitle = page.Hint,
                    Icon = page.Icon,
                    Url = ToUrl(page.Url)
                });
                added++;
            }
        }

        private static void AddAssignments(SqlConnection con, List<SearchHit> hits, int userId, string role, string like)
        {
            bool faculty = RoleAccess.CanCreateAssignments(role);
            bool admin = RoleAccess.IsAdmin(role);
            string sql = @"
                SELECT TOP 6 a.AssignmentID, a.AssignmentName, a.AssignmentCode, a.CreatedBy,
                       ISNULL(a.AssignmentType, N'College') AS AssignmentType
                FROM Assignments a
                WHERE a.IsDeleted = 0
                  AND (a.AssignmentName LIKE @Q OR a.AssignmentCode LIKE @Q)
                  AND (
                        @Admin = 1
                     OR a.CreatedBy = @UserID
                     OR EXISTS (
                            SELECT 1 FROM AssignmentGroups g
                            INNER JOIN AssignmentMembers m ON m.GroupID = g.GroupID AND m.UserID = @UserID
                            WHERE g.AssignmentID = a.AssignmentID AND g.IsDeleted = 0)
                  )
                ORDER BY a.Deadline";

            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@Q", like);
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@Admin", admin ? 1 : 0);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = Convert.ToInt32(reader["AssignmentID"]);
                        int createdBy = Convert.ToInt32(reader["CreatedBy"]);
                        bool openDetails = (faculty || admin) && createdBy == userId;
                        hits.Add(new SearchHit
                        {
                            Type = "Assignment",
                            Group = "Assignments",
                            Title = Convert.ToString(reader["AssignmentName"]),
                            Subtitle = Convert.ToString(reader["AssignmentCode"]) + " · " + Convert.ToString(reader["AssignmentType"]),
                            Icon = "school",
                            Url = ToUrl(openDetails
                                ? "~/Modules/Assignments/AssignmentDetails.aspx?AssignmentID=" + id
                                : "~/Modules/Assignments/MyAssignments.aspx")
                        });
                    }
                }
            }
        }

        private static void AddAssignmentGroups(SqlConnection con, List<SearchHit> hits, int userId, bool isAdmin, string like)
        {
            string sql = @"
                SELECT TOP 6 g.GroupID, g.GroupName, a.AssignmentName
                FROM AssignmentGroups g
                INNER JOIN Assignments a ON a.AssignmentID = g.AssignmentID AND a.IsDeleted = 0
                WHERE g.IsDeleted = 0
                  AND (g.GroupName LIKE @Q OR a.AssignmentName LIKE @Q OR a.AssignmentCode LIKE @Q)
                  AND (
                        @Admin = 1
                     OR a.CreatedBy = @UserID
                     OR EXISTS (
                            SELECT 1 FROM AssignmentMembers m
                            WHERE m.GroupID = g.GroupID AND m.UserID = @UserID)
                  )
                ORDER BY g.GroupName";

            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@Q", like);
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@Admin", isAdmin ? 1 : 0);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        hits.Add(new SearchHit
                        {
                            Type = "Assignment",
                            Group = "Assignments",
                            Title = Convert.ToString(reader["GroupName"]),
                            Subtitle = "Subgroup · " + Convert.ToString(reader["AssignmentName"]),
                            Icon = "group_work",
                            Url = ToUrl("~/Modules/Assignments/GroupWorkspace.aspx?GroupID=" + Convert.ToInt32(reader["GroupID"]))
                        });
                    }
                }
            }
        }

        private static void AddClubs(SqlConnection con, List<SearchHit> hits, int userId, bool isAdmin, string like)
        {
            string sql = @"
                SELECT TOP 6 c.ClubID, c.ClubName, ISNULL(c.IsPublic, 0) AS IsPublic
                FROM Clubs c
                WHERE ISNULL(c.IsDeleted, 0) = 0
                  AND (c.ClubName LIKE @Q OR ISNULL(c.Description, N'') LIKE @Q)
                  AND (
                        @Admin = 1
                     OR ISNULL(c.IsPublic, 0) = 1
                     OR EXISTS (
                            SELECT 1 FROM ClubMembers m
                            WHERE m.ClubID = c.ClubID AND m.UserID = @UserID
                              AND ISNULL(m.IsActive, 1) = 1
                              AND ISNULL(m.InviteStatus, N'Accepted') = N'Accepted')
                  )
                ORDER BY c.ClubName";

            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@Q", like);
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@Admin", isAdmin ? 1 : 0);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        hits.Add(new SearchHit
                        {
                            Type = "Club",
                            Group = "Clubs",
                            Title = Convert.ToString(reader["ClubName"]),
                            Subtitle = Convert.ToBoolean(reader["IsPublic"]) ? "Public club" : "Club",
                            Icon = "groups",
                            Url = ToUrl("~/Modules/Clubs/ClubWorkspace.aspx?ClubID=" + Convert.ToInt32(reader["ClubID"]))
                        });
                    }
                }
            }
        }

        private static void AddConnectGroups(SqlConnection con, List<SearchHit> hits, int userId, string like)
        {
            string sql = @"
                SELECT TOP 6 g.GroupID, g.GroupName
                FROM ConnectGroups g
                INNER JOIN ConnectMembers m ON m.GroupID = g.GroupID AND m.UserID = @UserID
                WHERE g.IsDeleted = 0
                  AND (g.GroupName LIKE @Q OR ISNULL(g.Description, N'') LIKE @Q)
                ORDER BY g.GroupName";

            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@Q", like);
                cmd.Parameters.AddWithValue("@UserID", userId);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        hits.Add(new SearchHit
                        {
                            Type = "Connect",
                            Group = "Connect",
                            Title = Convert.ToString(reader["GroupName"]),
                            Subtitle = "Chat room",
                            Icon = "forum",
                            Url = ToUrl("~/Modules/Connect/ConnectRoom.aspx?GroupID=" + Convert.ToInt32(reader["GroupID"]))
                        });
                    }
                }
            }
        }

        private static void AddPolls(SqlConnection con, List<SearchHit> hits, bool isAdmin, string like)
        {
            string sql = @"
                SELECT TOP 6 p.PollID, p.PollTitle, p.IsActive
                FROM Polls p
                WHERE (p.PollTitle LIKE @Q OR ISNULL(p.Description, N'') LIKE @Q)
                ORDER BY p.CreatedAt DESC";

            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@Q", like);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        hits.Add(new SearchHit
                        {
                            Type = "Poll",
                            Group = "Polls",
                            Title = Convert.ToString(reader["PollTitle"]),
                            Subtitle = Convert.ToBoolean(reader["IsActive"]) ? "Open poll" : "Closed poll",
                            Icon = "how_to_vote",
                            Url = ToUrl(isAdmin ? "~/Modules/Polls/Polls.aspx" : "~/Modules/UserPolls/UserPolls.aspx")
                        });
                    }
                }
            }
        }

        private static void AddUsers(SqlConnection con, List<SearchHit> hits, string like)
        {
            string sql = @"
                SELECT TOP 6 u.UserID, u.FullName, u.Email
                FROM Users u
                WHERE ISNULL(u.IsDeleted, 0) = 0
                  AND (u.FullName LIKE @Q OR u.Email LIKE @Q OR u.Username LIKE @Q)
                ORDER BY u.FullName";

            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@Q", like);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        hits.Add(new SearchHit
                        {
                            Type = "Person",
                            Group = "People",
                            Title = Convert.ToString(reader["FullName"]),
                            Subtitle = Convert.ToString(reader["Email"]),
                            Icon = "person",
                            Url = ToUrl("~/Modules/Users/UserActivity.aspx?UserID=" + Convert.ToInt32(reader["UserID"]))
                        });
                    }
                }
            }
        }

        private static bool Matches(string query, params string[] fields)
        {
            if (string.IsNullOrEmpty(query))
                return false;
            foreach (string field in fields)
            {
                if (!string.IsNullOrEmpty(field)
                    && field.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            return false;
        }

        private static string ToUrl(string virtualPath)
        {
            return VirtualPathUtility.ToAbsolute(virtualPath);
        }

        private static string EscapeLike(string value)
        {
            return value
                .Replace("[", "[[]")
                .Replace("%", "[%]")
                .Replace("_", "[_]");
        }
    }
}
