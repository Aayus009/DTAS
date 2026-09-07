using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DigitalTransparencySystem.Helpers
{
    public class TransparencyDecisionRow
    {
        public string Title { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public string Due { get; set; }
        public string Recorded { get; set; }
        public string Responsible { get; set; }
    }

    public class TransparencyEventRow
    {
        public int EventId { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public int Percent { get; set; }
        public string Label { get; set; }
    }

    public class TransparencyMeetingRow
    {
        public string Title { get; set; }
        public string When { get; set; }
        public bool HasMinutes { get; set; }
        public string MinutesLabel { get; set; }
    }

    public class TransparencySnapshot
    {
        public bool Ok { get; set; }
        public string SyncedAt { get; set; }
        public int OpenDecisions { get; set; }
        public int ImplementedDecisions { get; set; }
        public int TotalDecisions { get; set; }
        public int ImplementedPercent { get; set; }
        public int PublicEvents { get; set; }
        public int EventProgressPercent { get; set; }
        public int Meetings { get; set; }
        public int MeetingsWithMinutes { get; set; }
        public int MinutesPercent { get; set; }
        public int OpenPolls { get; set; }
        public List<TransparencyDecisionRow> Decisions { get; set; }
        public List<TransparencyEventRow> Events { get; set; }
        public List<TransparencyMeetingRow> MeetingsList { get; set; }
    }

    public static class TransparencyService
    {
        public static TransparencySnapshot LoadSnapshot()
        {
            EventTaskService.PromoteEventsInProgressFromTasks();
            var snap = new TransparencySnapshot
            {
                Ok = true,
                SyncedAt = DateTime.Now.ToString("MMM dd, yyyy · hh:mm:ss tt"),
                Decisions = new List<TransparencyDecisionRow>(),
                Events = new List<TransparencyEventRow>(),
                MeetingsList = new List<TransparencyMeetingRow>()
            };

            using (var con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                snap.OpenDecisions = Scalar(con,
                    @"SELECT COUNT(1) FROM Decisions
                      WHERE Status NOT IN (N'Implemented', N'Completed', N'Closed', N'Rejected', N'Cancelled')");
                snap.ImplementedDecisions = Scalar(con,
                    @"SELECT COUNT(1) FROM Decisions
                      WHERE Status IN (N'Implemented', N'Completed', N'Closed')");
                snap.TotalDecisions = Scalar(con,
                    @"SELECT COUNT(1) FROM Decisions
                      WHERE Status NOT IN (N'Rejected', N'Cancelled')");
                snap.PublicEvents = Scalar(con,
                    @"SELECT COUNT(1) FROM Events
                      WHERE ISNULL(IsDeleted, 0) = 0
                        AND ISNULL(IsDisabled, 0) = 0
                        AND Visibility = N'Public'");
                snap.Meetings = Scalar(con, "SELECT COUNT(1) FROM Meetings");
                snap.MeetingsWithMinutes = Scalar(con,
                    @"SELECT COUNT(1) FROM Meetings
                      WHERE MinutesOfMeeting IS NOT NULL AND LTRIM(RTRIM(MinutesOfMeeting)) <> N''");
                try
                {
                    snap.OpenPolls = Scalar(con,
                        @"SELECT COUNT(1) FROM Polls
                          WHERE ISNULL(IsActive, 1) = 1
                            AND (EndDate IS NULL OR EndDate >= GETDATE())");
                }
                catch (SqlException)
                {
                    snap.OpenPolls = 0;
                }

                LoadDecisions(con, snap);
                LoadMeetings(con, snap);
            }

            snap.ImplementedPercent = Percent(snap.ImplementedDecisions, snap.TotalDecisions);
            snap.MinutesPercent = Percent(snap.MeetingsWithMinutes, snap.Meetings);
            LoadPublicEvents(snap);
            return snap;
        }

        private static void LoadPublicEvents(TransparencySnapshot snap)
        {
            var ids = new HashSet<int>();
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT EventID FROM Events
                  WHERE ISNULL(IsDeleted, 0) = 0
                    AND ISNULL(IsDisabled, 0) = 0
                    AND Visibility = N'Public'", con))
            {
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        ids.Add(Convert.ToInt32(reader["EventID"]));
                }
            }

            int weightedDone = 0;
            int weightedTotal = 0;
            foreach (EventLiveProgress row in EventTaskService.ListLiveEventProgress())
            {
                if (!ids.Contains(row.EventId))
                    continue;
                snap.Events.Add(new TransparencyEventRow
                {
                    EventId = row.EventId,
                    Name = row.EventName,
                    Status = EventTaskService.EventStatusDisplay(row.Status),
                    Percent = row.Percent,
                    Label = EventTaskService.CompletionLabel(row.CompletedTasks, row.TotalTasks)
                });
                weightedDone += row.CompletedTasks;
                weightedTotal += row.TotalTasks;
            }

            snap.EventProgressPercent = Percent(weightedDone, weightedTotal);
            if (snap.Events.Count > 8)
                snap.Events = snap.Events.GetRange(0, 8);
        }

        private static void LoadDecisions(SqlConnection con, TransparencySnapshot snap)
        {
            using (var cmd = new SqlCommand(
                @"SELECT TOP 8 d.DecisionTitle, d.Priority, d.Status, d.DueDate, d.CreatedAt,
                         ISNULL(u.FullName, N'Unassigned') AS ResponsibleName
                  FROM Decisions d
                  LEFT JOIN Users u ON u.UserID = d.ResponsibleUserID
                  WHERE d.Status NOT IN (N'Rejected', N'Cancelled')
                  ORDER BY d.CreatedAt DESC", con))
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    snap.Decisions.Add(new TransparencyDecisionRow
                    {
                        Title = Convert.ToString(reader["DecisionTitle"]),
                        Priority = Convert.ToString(reader["Priority"]),
                        Status = Convert.ToString(reader["Status"]),
                        Due = reader["DueDate"] == DBNull.Value
                            ? "—"
                            : Convert.ToDateTime(reader["DueDate"]).ToString("MMM dd, yyyy"),
                        Recorded = Convert.ToDateTime(reader["CreatedAt"]).ToString("MMM dd, yyyy"),
                        Responsible = Convert.ToString(reader["ResponsibleName"])
                    });
                }
            }
        }

        private static void LoadMeetings(SqlConnection con, TransparencySnapshot snap)
        {
            using (var cmd = new SqlCommand(
                @"SELECT TOP 8 MeetingTitle, ScheduledDate,
                         CASE WHEN MinutesOfMeeting IS NOT NULL AND LTRIM(RTRIM(MinutesOfMeeting)) <> N'' THEN 1 ELSE 0 END AS HasMinutes
                  FROM Meetings
                  ORDER BY ScheduledDate DESC", con))
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    bool hasMinutes = Convert.ToInt32(reader["HasMinutes"]) == 1;
                    snap.MeetingsList.Add(new TransparencyMeetingRow
                    {
                        Title = Convert.ToString(reader["MeetingTitle"]),
                        When = Convert.ToDateTime(reader["ScheduledDate"]).ToString("MMM dd, yyyy · hh:mm tt"),
                        HasMinutes = hasMinutes,
                        MinutesLabel = hasMinutes ? "Minutes published" : "Minutes pending"
                    });
                }
            }
        }

        private static int Scalar(SqlConnection con, string sql)
        {
            using (var cmd = new SqlCommand(sql, con))
            {
                object value = cmd.ExecuteScalar();
                return value == null || value == DBNull.Value ? 0 : Convert.ToInt32(value);
            }
        }

        private static int Percent(int part, int total)
        {
            return total <= 0 ? 0 : (int)Math.Round((double)part / total * 100);
        }
    }
}
