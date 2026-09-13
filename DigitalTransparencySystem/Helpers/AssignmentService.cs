using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace DigitalTransparencySystem.Helpers
{
    public class AssignmentRecord
    {
        public int AssignmentID { get; set; }
        public string AssignmentName { get; set; }
        public string Description { get; set; }
        public DateTime Deadline { get; set; }
        public string AssignmentCode { get; set; }
        public string Status { get; set; }
        public int CreatedBy { get; set; }
        public bool IsDeleted { get; set; }
        public string AssignmentType { get; set; }

        public bool IsPersonal
        {
            get { return string.Equals(AssignmentType, "Personal", StringComparison.OrdinalIgnoreCase); }
        }
    }

    public class AssignmentGroupRecord
    {
        public int GroupID { get; set; }
        public int AssignmentID { get; set; }
        public string GroupName { get; set; }
        public int LeaderID { get; set; }
        public bool IsFinalized { get; set; }
        public bool IsDeleted { get; set; }
        public AssignmentRecord Assignment { get; set; }
    }

    public class AssignmentAccess
    {
        public AssignmentGroupRecord Group { get; set; }
        public bool IsSystemAdmin { get; set; }
        public bool IsFacultyOwner { get; set; }
        public bool IsMember { get; set; }
        public bool IsLeader { get; set; }
        public string Responsibility { get; set; }

        public bool CanView
        {
            get { return Group != null && !Group.IsDeleted && (IsFacultyOwner || IsMember); }
        }

        public bool CanCorrect
        {
            get { return IsFacultyOwner; }
        }

        public bool IsDeadlineLocked
        {
            get { return Group != null && AssignmentService.IsLockedForStudents(Group.Assignment); }
        }

        public bool CanManageStructure
        {
            get { return IsLeader && Group != null && !Group.IsDeleted && !Group.IsFinalized && !IsDeadlineLocked; }
        }

        public bool CanAssignLeader
        {
            get { return CanManageStructure || CanCorrect; }
        }

        public bool CanInvite
        {
            get { return CanManageStructure; }
        }

        public bool CanUpdateTaskStatus(int? assignedUserId, int actorId)
        {
            if (!CanView)
                return false;
            if (IsDeadlineLocked && !CanCorrect)
                return false;
            if (CanCorrect || IsLeader)
                return true;
            return assignedUserId.HasValue && assignedUserId.Value == actorId;
        }

        public bool CanSubmitWork(int? assignedUserId, int actorId, string status)
        {
            if (IsDeadlineLocked && !CanCorrect)
                return false;
            if (!IsMember || !assignedUserId.HasValue || assignedUserId.Value != actorId)
                return false;
            return true;
        }

        public bool CanComment
        {
            get { return CanView && (IsMember || IsFacultyOwner || IsSystemAdmin); }
        }
    }

    public class AssignmentTaskRecord
    {
        public int TaskID { get; set; }
        public int GroupID { get; set; }
        public int? AssignedUserID { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public string SubmissionNote { get; set; }
        public string SubmissionFilePath { get; set; }
        public string SubmissionFileName { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public int? SubmittedBy { get; set; }
    }

    public class AssignmentTaskFileRecord
    {
        public int FileID { get; set; }
        public int TaskID { get; set; }
        public int GroupID { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
    }

    public static class AssignmentService
    {
        private static readonly object SchemaLock = new object();
        private static bool schemaReady;
        private static bool assignmentCodesRefreshed;
        private const string TaskProgressWeightSql = @"CASE
                            WHEN t.Status = N'Completed' THEN 1.0
                            WHEN t.Status = N'UnderReview' THEN 0.75
                            WHEN t.Status = N'InProgress' THEN 0.50
                            ELSE 0.0
                        END";
        private const string AssignmentSelectColumns =
            "AssignmentID, AssignmentName, Description, Deadline, AssignmentCode, Status, CreatedBy, IsDeleted, ISNULL(AssignmentType, N'College') AS AssignmentType";

        public static void EnsureSchema()
        {
            if (schemaReady)
                return;
            lock (SchemaLock)
            {
                if (schemaReady)
                    return;
                using (var con = new SqlConnection(AuthService.ConnectionString))
                {
                    con.Open();
                    new SqlCommand(@"
                    IF COL_LENGTH('dbo.Assignments', 'AssignmentType') IS NULL
                        ALTER TABLE dbo.Assignments ADD AssignmentType NVARCHAR(20) NOT NULL
                            CONSTRAINT DF_Assignments_AssignmentType DEFAULT N'College';
                    IF OBJECT_ID('dbo.AssignmentTaskFiles', 'U') IS NULL
                    BEGIN
                        CREATE TABLE dbo.AssignmentTaskFiles (
                            FileID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            TaskID INT NOT NULL,
                            FilePath NVARCHAR(500) NOT NULL,
                            FileName NVARCHAR(260) NOT NULL,
                            UploadedBy INT NOT NULL,
                            UploadedAt DATETIME NOT NULL CONSTRAINT DF_AssignmentTaskFiles_UploadedAt DEFAULT GETDATE(),
                            CONSTRAINT FK_AssignmentTaskFiles_Task FOREIGN KEY (TaskID) REFERENCES dbo.AssignmentTasks(TaskID),
                            CONSTRAINT FK_AssignmentTaskFiles_User FOREIGN KEY (UploadedBy) REFERENCES dbo.Users(UserID)
                        );
                        CREATE INDEX IX_AssignmentTaskFiles_Task ON dbo.AssignmentTaskFiles(TaskID);
                    END
                    IF OBJECT_ID('dbo.AssignmentTaskComments', 'U') IS NULL
                    BEGIN
                        CREATE TABLE dbo.AssignmentTaskComments (
                            CommentID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            TaskID INT NOT NULL,
                            UserID INT NOT NULL,
                            Comment NVARCHAR(2000) NOT NULL,
                            CreatedAt DATETIME NOT NULL CONSTRAINT DF_AssignmentTaskComments_CreatedAt DEFAULT GETDATE(),
                            CONSTRAINT FK_ATC_Task FOREIGN KEY (TaskID) REFERENCES dbo.AssignmentTasks(TaskID),
                            CONSTRAINT FK_ATC_User FOREIGN KEY (UserID) REFERENCES dbo.Users(UserID)
                        );
                        CREATE INDEX IX_AssignmentTaskComments_Task ON dbo.AssignmentTaskComments(TaskID, CreatedAt);
                    END
                ", con).ExecuteNonQuery();
                    try
                    {
                        RefreshProgressProcedures(con);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Assignment progress procedures: " + ex.Message);
                    }
                }
                schemaReady = true;
            }
            RefreshSnaAssignmentCodes();
        }

        private static void RefreshProgressProcedures(SqlConnection con)
        {
            new SqlCommand(@"
CREATE OR ALTER PROCEDURE dbo.sp_GetAssignmentProgress
    @GroupID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        g.GroupID,
        g.GroupName,
        g.LeaderID,
        g.IsFinalized,
        a.AssignmentID,
        a.AssignmentName,
        a.Deadline,
        a.AssignmentCode,
        COUNT(t.TaskID) AS TotalTasks,
        SUM(CASE WHEN t.Status = N'Completed' THEN 1 ELSE 0 END) AS CompletedTasks,
        SUM(CASE WHEN t.Status = N'InProgress' THEN 1 ELSE 0 END) AS InProgressTasks,
        SUM(CASE WHEN t.Status = N'UnderReview' THEN 1 ELSE 0 END) AS UnderReviewTasks,
        SUM(CASE WHEN t.Status = N'Blocked' THEN 1 ELSE 0 END) AS BlockedTasks,
        SUM(CASE WHEN t.DueDate IS NOT NULL AND t.DueDate < GETDATE() AND t.Status <> N'Completed' THEN 1 ELSE 0 END) AS OverdueTasks,
        CAST(
            SUM(" + TaskProgressWeightSql + @") * 100.0
            / NULLIF(COUNT(t.TaskID), 0)
            AS DECIMAL(5,2)
        ) AS CompletionPercentage
    FROM dbo.AssignmentGroups g
    INNER JOIN dbo.Assignments a ON a.AssignmentID = g.AssignmentID
    LEFT JOIN dbo.AssignmentTasks t ON t.GroupID = g.GroupID AND t.IsDeleted = 0
    WHERE g.GroupID = @GroupID AND g.IsDeleted = 0
    GROUP BY
        g.GroupID, g.GroupName, g.LeaderID, g.IsFinalized,
        a.AssignmentID, a.AssignmentName, a.Deadline, a.AssignmentCode;

    SELECT
        m.UserID,
        u.FullName,
        m.Responsibility,
        COUNT(t.TaskID) AS AssignedTasks,
        SUM(CASE WHEN t.Status = N'Completed' THEN 1 ELSE 0 END) AS CompletedTasks,
        SUM(CASE WHEN t.Status <> N'Completed' THEN 1 ELSE 0 END) AS PendingTasks,
        SUM(CASE WHEN t.DueDate IS NOT NULL AND t.DueDate < GETDATE() AND t.Status <> N'Completed' THEN 1 ELSE 0 END) AS OverdueTasks,
        ISNULL(CAST(
            SUM(" + TaskProgressWeightSql + @") * 100.0
            / NULLIF(COUNT(t.TaskID), 0)
            AS DECIMAL(5,2)
        ), 0) AS CompletionPercentage
    FROM dbo.AssignmentMembers m
    INNER JOIN dbo.Users u ON u.UserID = m.UserID
    LEFT JOIN dbo.AssignmentTasks t
        ON t.GroupID = m.GroupID
       AND t.AssignedUserID = m.UserID
       AND t.IsDeleted = 0
    WHERE m.GroupID = @GroupID
    GROUP BY m.UserID, u.FullName, m.Responsibility
    ORDER BY m.Responsibility, u.FullName;
END", con).ExecuteNonQuery();

            new SqlCommand(@"
CREATE OR ALTER PROCEDURE dbo.sp_GetMemberContributionReport
    @GroupID INT,
    @UserID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH MemberBase AS (
        SELECT m.GroupID, m.UserID
        FROM dbo.AssignmentMembers m
        WHERE m.GroupID = @GroupID
          AND (@UserID IS NULL OR m.UserID = @UserID)
    ),
    TaskStats AS (
        SELECT
            b.GroupID,
            b.UserID,
            AssignedTasks = COUNT(t.TaskID),
            CompletedTasks = SUM(CASE WHEN t.Status = N'Completed' THEN 1 ELSE 0 END),
            PendingTasks = SUM(CASE WHEN t.Status <> N'Completed' THEN 1 ELSE 0 END),
            OverdueTasks = SUM(CASE WHEN t.DueDate IS NOT NULL AND t.DueDate < GETDATE() AND t.Status <> N'Completed' THEN 1 ELSE 0 END),
            ProgressWeight = SUM(CASE
                WHEN t.Status = N'Completed' THEN 1.0
                WHEN t.Status = N'UnderReview' THEN 0.75
                WHEN t.Status = N'InProgress' THEN 0.50
                ELSE 0.0
            END),
            LastTaskActivity = MAX(t.UpdatedAt)
        FROM MemberBase b
        LEFT JOIN dbo.AssignmentTasks t
            ON t.GroupID = b.GroupID
           AND t.AssignedUserID = b.UserID
           AND t.IsDeleted = 0
        GROUP BY b.GroupID, b.UserID
    ),
    UpdateStats AS (
        SELECT
            b.GroupID,
            b.UserID,
            ProgressUpdates = COUNT(h.HistoryID),
            LastHistory = MAX(h.ChangedAt)
        FROM MemberBase b
        LEFT JOIN dbo.AssignmentTasks t
            ON t.GroupID = b.GroupID AND t.IsDeleted = 0
        LEFT JOIN dbo.AssignmentTaskHistory h
            ON h.TaskID = t.TaskID AND h.ChangedBy = b.UserID
        GROUP BY b.GroupID, b.UserID
    ),
    Scored AS (
        SELECT
            ts.GroupID,
            ts.UserID,
            ts.AssignedTasks,
            ts.CompletedTasks,
            ts.PendingTasks,
            ts.OverdueTasks,
            us.ProgressUpdates,
            LastActivity = CASE
                WHEN ts.LastTaskActivity IS NULL THEN us.LastHistory
                WHEN us.LastHistory IS NULL THEN ts.LastTaskActivity
                WHEN us.LastHistory > ts.LastTaskActivity THEN us.LastHistory
                ELSE ts.LastTaskActivity
            END,
            CompletionPercentage = CAST(
                CASE WHEN ts.AssignedTasks = 0 THEN 0
                     ELSE ISNULL(ts.ProgressWeight, 0) * 100.0 / ts.AssignedTasks
                END AS DECIMAL(5,2)
            ),
            ContributionScore = CAST(
                CASE WHEN ts.AssignedTasks = 0 THEN 0
                     ELSE
                        (50.0 * ISNULL(ts.ProgressWeight, 0) / ts.AssignedTasks)
                      + (20.0 * (1.0 - (ts.OverdueTasks * 1.0 / ts.AssignedTasks)))
                      + (20.0 * (CASE WHEN us.ProgressUpdates * 20.0 > 100 THEN 100 ELSE us.ProgressUpdates * 20.0 END) / 100.0)
                      + (10.0 * CASE WHEN COALESCE(
                            CASE
                                WHEN ts.LastTaskActivity IS NULL THEN us.LastHistory
                                WHEN us.LastHistory IS NULL THEN ts.LastTaskActivity
                                WHEN us.LastHistory > ts.LastTaskActivity THEN us.LastHistory
                                ELSE ts.LastTaskActivity
                            END, '19000101') >= DATEADD(DAY, -14, GETDATE()) THEN 1 ELSE 0 END)
                END AS DECIMAL(5,2)
            )
        FROM TaskStats ts
        INNER JOIN UpdateStats us ON us.GroupID = ts.GroupID AND us.UserID = ts.UserID
    )
    MERGE dbo.ContributionRecords AS target
    USING Scored AS src
        ON target.GroupID = src.GroupID AND target.UserID = src.UserID
    WHEN MATCHED THEN
        UPDATE SET
            AssignedTasks = src.AssignedTasks,
            CompletedTasks = src.CompletedTasks,
            PendingTasks = src.PendingTasks,
            OverdueTasks = src.OverdueTasks,
            ProgressUpdates = src.ProgressUpdates,
            LastActivity = src.LastActivity,
            CompletionPercentage = src.CompletionPercentage,
            ContributionScore = src.ContributionScore,
            CalculatedAt = GETDATE()
    WHEN NOT MATCHED THEN
        INSERT (GroupID, UserID, AssignedTasks, CompletedTasks, PendingTasks, OverdueTasks, ProgressUpdates, LastActivity, CompletionPercentage, ContributionScore, CalculatedAt)
        VALUES (src.GroupID, src.UserID, src.AssignedTasks, src.CompletedTasks, src.PendingTasks, src.OverdueTasks, src.ProgressUpdates, src.LastActivity, src.CompletionPercentage, src.ContributionScore, GETDATE());

    SELECT
        cr.RecordID,
        cr.GroupID,
        cr.UserID,
        u.FullName,
        m.Responsibility,
        cr.AssignedTasks,
        cr.CompletedTasks,
        cr.PendingTasks,
        cr.OverdueTasks,
        cr.ProgressUpdates,
        cr.LastActivity,
        cr.CompletionPercentage,
        cr.ContributionScore,
        cr.CalculatedAt
    FROM dbo.ContributionRecords cr
    INNER JOIN dbo.Users u ON u.UserID = cr.UserID
    INNER JOIN dbo.AssignmentMembers m ON m.GroupID = cr.GroupID AND m.UserID = cr.UserID
    WHERE cr.GroupID = @GroupID
      AND (@UserID IS NULL OR cr.UserID = @UserID)
    ORDER BY cr.ContributionScore DESC, u.FullName;
END", con).ExecuteNonQuery();
        }

        public static decimal TaskProgressWeight(string status)
        {
            if (string.Equals(status, "Completed", StringComparison.OrdinalIgnoreCase))
                return 1m;
            if (string.Equals(status, "UnderReview", StringComparison.OrdinalIgnoreCase)
                || string.Equals(status, "Under Review", StringComparison.OrdinalIgnoreCase))
                return 0.75m;
            if (string.Equals(status, "InProgress", StringComparison.OrdinalIgnoreCase)
                || string.Equals(status, "In Progress", StringComparison.OrdinalIgnoreCase))
                return 0.50m;
            return 0m;
        }

        public static decimal ComputeGroupProgress(DataTable tasks)
        {
            if (tasks == null || tasks.Rows.Count == 0)
                return 0m;
            decimal sum = 0m;
            foreach (DataRow row in tasks.Rows)
                sum += TaskProgressWeight(Convert.ToString(row["Status"]));
            return Math.Round(sum * 100m / tasks.Rows.Count, 2, MidpointRounding.AwayFromZero);
        }

        private static void RefreshSnaAssignmentCodes()
        {
            if (assignmentCodesRefreshed)
                return;
            lock (SchemaLock)
            {
                if (assignmentCodesRefreshed)
                    return;
                try
                {
                    using (var con = new SqlConnection(AuthService.ConnectionString))
                    {
                        con.Open();
                        var pending = new DataTable();
                        using (var cmd = new SqlCommand(@"
                            SELECT AssignmentID, AssignmentName
                            FROM Assignments
                            WHERE IsDeleted = 0
                              AND AssignmentCode LIKE N'SNA-ASSIGN-%'
                              AND ISNULL(AssignmentType, N'College') <> N'Personal'", con))
                        {
                            new SqlDataAdapter(cmd).Fill(pending);
                        }
                        foreach (DataRow row in pending.Rows)
                        {
                            string next = NextCollegeCode(Convert.ToString(row["AssignmentName"]), con);
                            using (var upd = new SqlCommand(
                                "UPDATE Assignments SET AssignmentCode = @Code WHERE AssignmentID = @ID", con))
                            {
                                upd.Parameters.AddWithValue("@Code", next);
                                upd.Parameters.AddWithValue("@ID", Convert.ToInt32(row["AssignmentID"]));
                                upd.ExecuteNonQuery();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Assignment codes: " + ex.Message);
                }
                assignmentCodesRefreshed = true;
            }
        }

        public static string CreateAssignment(int creatorId, string name, string description, DateTime deadline, out int assignmentId, out string code)
        {
            assignmentId = 0;
            code = null;
            EnsureSchema();
            UserAccount creator = AuthService.FindById(creatorId);
            if (creator == null || !RoleAccess.CanCreateAssignments(creator.Role))
                return "Only Faculty can create an academic assignment.";
            try
            {
                string generated = NextCollegeCode(name.Trim());
                using (var con = new SqlConnection(AuthService.ConnectionString))
                using (var cmd = new SqlCommand(@"
                    INSERT INTO Assignments
                        (AssignmentName, Description, Deadline, AssignmentCode, CreatedBy, CreatedAt, Status, IsDeleted, AssignmentType)
                    VALUES
                        (@Name, @Description, @Deadline, @Code, @CreatedBy, GETDATE(), N'Active', 0, N'College');
                    SELECT CAST(SCOPE_IDENTITY() AS INT);", con))
                {
                    cmd.Parameters.AddWithValue("@Name", name.Trim());
                    cmd.Parameters.AddWithValue("@Description", string.IsNullOrWhiteSpace(description) ? (object)DBNull.Value : description.Trim());
                    cmd.Parameters.AddWithValue("@Deadline", deadline);
                    cmd.Parameters.AddWithValue("@Code", generated);
                    cmd.Parameters.AddWithValue("@CreatedBy", creatorId);
                    con.Open();
                    assignmentId = Convert.ToInt32(cmd.ExecuteScalar());
                    code = generated;
                }
                AuthService.WriteAudit(creatorId, "AssignmentCreated", "Assignment", assignmentId, name.Trim() + " (" + code + ")", null);
                int connectId;
                ConnectService.CreateGroupForAssignment(assignmentId, out connectId);
                return null;
            }
            catch (SqlException ex)
            {
                return ex.Message;
            }
        }

        public static AssignmentRecord GetAssignment(int assignmentId)
        {
            EnsureSchema();
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT " + AssignmentSelectColumns + @"
                  FROM Assignments WHERE AssignmentID = @ID", con))
            {
                cmd.Parameters.AddWithValue("@ID", assignmentId);
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;
                    return ReadAssignment(reader);
                }
            }
        }

        public static AssignmentRecord FindByCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return null;
            EnsureSchema();
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT " + AssignmentSelectColumns + @"
                  FROM Assignments
                  WHERE AssignmentCode = @Code AND IsDeleted = 0 AND Status = N'Active'
                    AND ISNULL(AssignmentType, N'College') = N'College'", con))
            {
                cmd.Parameters.AddWithValue("@Code", code.Trim().ToUpperInvariant());
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;
                    return ReadAssignment(reader);
                }
            }
        }

        public static AssignmentGroupRecord GetGroup(int groupId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT GroupID, AssignmentID, GroupName, LeaderID, IsFinalized, IsDeleted
                  FROM AssignmentGroups WHERE GroupID = @ID", con))
            {
                cmd.Parameters.AddWithValue("@ID", groupId);
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;
                    return new AssignmentGroupRecord
                    {
                        GroupID = Convert.ToInt32(reader["GroupID"]),
                        AssignmentID = Convert.ToInt32(reader["AssignmentID"]),
                        GroupName = Convert.ToString(reader["GroupName"]),
                        LeaderID = Convert.ToInt32(reader["LeaderID"]),
                        IsFinalized = Convert.ToBoolean(reader["IsFinalized"]),
                        IsDeleted = Convert.ToBoolean(reader["IsDeleted"])
                    };
                }
            }
        }

        public static AssignmentAccess GetAccess(int groupId, int userId, string systemRole)
        {
            var access = new AssignmentAccess
            {
                Group = GetGroup(groupId),
                IsSystemAdmin = RoleAccess.IsAdmin(systemRole)
            };
            if (access.Group == null)
                return access;

            access.Group.Assignment = GetAssignment(access.Group.AssignmentID);
            access.IsFacultyOwner = access.Group.Assignment != null
                && !access.Group.Assignment.IsPersonal
                && access.Group.Assignment.CreatedBy == userId
                && RoleAccess.CanMonitorAssignments(systemRole);

            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                "SELECT Responsibility FROM AssignmentMembers WHERE GroupID = @GroupID AND UserID = @UserID", con))
            {
                cmd.Parameters.AddWithValue("@GroupID", groupId);
                cmd.Parameters.AddWithValue("@UserID", userId);
                con.Open();
                object role = cmd.ExecuteScalar();
                if (role != null && role != DBNull.Value)
                {
                    access.IsMember = true;
                    access.Responsibility = role.ToString();
                    access.IsLeader = string.Equals(access.Responsibility, "Leader", StringComparison.OrdinalIgnoreCase)
                        || access.Group.LeaderID == userId;
                }
            }

            return access;
        }

        public static bool CanViewAssignment(AssignmentRecord assignment, int userId, string systemRole)
        {
            if (assignment == null || assignment.IsDeleted)
                return false;
            if (assignment.CreatedBy == userId && RoleAccess.CanMonitorAssignments(systemRole))
                return true;
            return IsOnAssignment(assignment.AssignmentID, userId);
        }

        public static DataTable ListAssignments(int userId, string systemRole)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT a.AssignmentID, a.AssignmentName, a.Deadline, a.AssignmentCode, a.Status, a.CreatedAt,
                         (SELECT COUNT(*) FROM AssignmentGroups g WHERE g.AssignmentID = a.AssignmentID AND g.IsDeleted = 0) AS GroupCount
                  FROM Assignments a
                  WHERE a.IsDeleted = 0 AND a.CreatedBy = @UserID
                    AND ISNULL(a.AssignmentType, N'College') = N'College'
                  ORDER BY a.CreatedAt DESC", con))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
                var table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                return table;
            }
        }

        public static DataTable ListGroups(int assignmentId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT g.GroupID, g.GroupName, g.LeaderID, u.FullName AS LeaderName, g.IsFinalized, g.CreatedAt,
                         COUNT(t.TaskID) AS TotalTasks,
                         SUM(CASE WHEN t.Status = N'Completed' THEN 1 ELSE 0 END) AS CompletedTasks,
                         CAST(
                            SUM(CASE
                                WHEN t.Status = N'Completed' THEN 1.0
                                WHEN t.Status = N'UnderReview' THEN 0.75
                                WHEN t.Status = N'InProgress' THEN 0.50
                                ELSE 0.0
                            END) * 100.0
                            / NULLIF(COUNT(t.TaskID), 0)
                            AS DECIMAL(5,2)
                         ) AS CompletionPercentage
                  FROM AssignmentGroups g
                  INNER JOIN Users u ON u.UserID = g.LeaderID
                  LEFT JOIN AssignmentTasks t ON t.GroupID = g.GroupID AND t.IsDeleted = 0
                  WHERE g.AssignmentID = @AssignmentID AND g.IsDeleted = 0
                  GROUP BY g.GroupID, g.GroupName, g.LeaderID, u.FullName, g.IsFinalized, g.CreatedAt
                  ORDER BY g.CreatedAt", con))
            {
                cmd.Parameters.AddWithValue("@AssignmentID", assignmentId);
                var table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                return table;
            }
        }

        public static DataTable ListMyGroups(int userId, string typeFilter = null)
        {
            EnsureSchema();
            string typeClause = "";
            if (string.Equals(typeFilter, "Personal", StringComparison.OrdinalIgnoreCase))
                typeClause = "AND ISNULL(a.AssignmentType, N'College') = N'Personal'";
            else if (string.Equals(typeFilter, "College", StringComparison.OrdinalIgnoreCase))
                typeClause = "AND ISNULL(a.AssignmentType, N'College') = N'College'";

            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT g.GroupID, g.GroupName, g.IsFinalized, a.AssignmentID, a.AssignmentName, a.AssignmentCode, a.Deadline,
                         m.Responsibility, ISNULL(a.AssignmentType, N'College') AS AssignmentType,
                         COUNT(t.TaskID) AS TotalTasks,
                         SUM(CASE WHEN t.Status = N'Completed' THEN 1 ELSE 0 END) AS CompletedTasks,
                         CAST(
                            SUM(CASE
                                WHEN t.Status = N'Completed' THEN 1.0
                                WHEN t.Status = N'UnderReview' THEN 0.75
                                WHEN t.Status = N'InProgress' THEN 0.50
                                ELSE 0.0
                            END) * 100.0
                            / NULLIF(COUNT(t.TaskID), 0)
                            AS DECIMAL(5,2)
                         ) AS CompletionPercentage
                  FROM AssignmentMembers m
                  INNER JOIN AssignmentGroups g ON g.GroupID = m.GroupID AND g.IsDeleted = 0
                  INNER JOIN Assignments a ON a.AssignmentID = g.AssignmentID AND a.IsDeleted = 0
                  LEFT JOIN AssignmentTasks t ON t.GroupID = g.GroupID AND t.IsDeleted = 0
                  WHERE m.UserID = @UserID " + typeClause + @"
                  GROUP BY g.GroupID, g.GroupName, g.IsFinalized, a.AssignmentID, a.AssignmentName, a.AssignmentCode, a.Deadline, m.Responsibility, ISNULL(a.AssignmentType, N'College')
                  ORDER BY a.Deadline", con))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
                var table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                return table;
            }
        }

        public static int CountSidebarItems(int userId, string systemRole)
        {
            if (RoleAccess.CanCreateAssignments(systemRole))
            {
                using (var con = new SqlConnection(AuthService.ConnectionString))
                using (var cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM Assignments WHERE IsDeleted = 0 AND CreatedBy = @UserID", con))
                {
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    con.Open();
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }

            UserAccount user = AuthService.FindById(userId);
            string email = user == null ? "" : (user.Email ?? "");
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT
                    (SELECT COUNT(*)
                     FROM AssignmentMembers m
                     INNER JOIN AssignmentGroups g ON g.GroupID = m.GroupID AND g.IsDeleted = 0
                     INNER JOIN Assignments a ON a.AssignmentID = g.AssignmentID AND a.IsDeleted = 0
                     WHERE m.UserID = @UserID)
                  + (SELECT COUNT(*)
                     FROM AssignmentInvitations i
                     INNER JOIN AssignmentGroups g ON g.GroupID = i.GroupID AND g.IsDeleted = 0
                     INNER JOIN Assignments a ON a.AssignmentID = i.AssignmentID AND a.IsDeleted = 0
                     WHERE i.Status = N'Pending'
                       AND (i.InvitedUserID = @UserID OR i.Email = @Email))", con))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@Email", email);
                con.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public static string CreatePersonalGroup(int userId, string groupName, string description, DateTime deadline, out int groupId)
        {
            groupId = 0;
            EnsureSchema();
            UserAccount actor = AuthService.FindById(userId);
            if (actor == null || !RoleAccess.IsStudent(actor.Role))
                return "Only students can create a personal assignment group.";
            if (string.IsNullOrWhiteSpace(groupName))
                return "Enter a group name.";
            if (deadline <= DateTime.Now)
                return "Choose a deadline in the future.";

            string code = NextPersonalCode();
            using (var con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                using (var tx = con.BeginTransaction())
                {
                    var insertAssignment = new SqlCommand(
                        @"INSERT INTO Assignments (AssignmentName, Description, Deadline, AssignmentCode, CreatedBy, Status, IsDeleted, AssignmentType)
                          VALUES (@Name, @Description, @Deadline, @Code, @CreatedBy, N'Active', 0, N'Personal');
                          SELECT CAST(SCOPE_IDENTITY() AS INT);", con, tx);
                    insertAssignment.Parameters.AddWithValue("@Name", groupName.Trim());
                    insertAssignment.Parameters.AddWithValue("@Description", string.IsNullOrWhiteSpace(description) ? (object)DBNull.Value : description.Trim());
                    insertAssignment.Parameters.AddWithValue("@Deadline", deadline);
                    insertAssignment.Parameters.AddWithValue("@Code", code);
                    insertAssignment.Parameters.AddWithValue("@CreatedBy", userId);
                    int assignmentId = Convert.ToInt32(insertAssignment.ExecuteScalar());

                    var insertGroup = new SqlCommand(
                        @"INSERT INTO AssignmentGroups (AssignmentID, GroupName, LeaderID, IsFinalized, CreatedAt, IsDeleted)
                          VALUES (@AssignmentID, @Name, @LeaderID, 0, GETDATE(), 0);
                          SELECT CAST(SCOPE_IDENTITY() AS INT);", con, tx);
                    insertGroup.Parameters.AddWithValue("@AssignmentID", assignmentId);
                    insertGroup.Parameters.AddWithValue("@Name", groupName.Trim());
                    insertGroup.Parameters.AddWithValue("@LeaderID", userId);
                    groupId = Convert.ToInt32(insertGroup.ExecuteScalar());

                    var member = new SqlCommand(
                        @"INSERT INTO AssignmentMembers (GroupID, UserID, Responsibility, JoinedAt)
                          VALUES (@GroupID, @UserID, N'Leader', GETDATE())", con, tx);
                    member.Parameters.AddWithValue("@GroupID", groupId);
                    member.Parameters.AddWithValue("@UserID", userId);
                    member.ExecuteNonQuery();
                    tx.Commit();
                }
            }

            AuthService.WriteAudit(userId, "PersonalAssignmentGroupCreated", "AssignmentGroup", groupId, groupName.Trim(), null);
            int connectId;
            ConnectService.CreateGroupForAssignmentSubgroup(groupId, out connectId);
            return null;
        }

        private static string NextPersonalCode()
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                for (int attempt = 0; attempt < 20; attempt++)
                {
                    string code = "PERS-ASSIGN-" + (Math.Abs(Guid.NewGuid().GetHashCode()) % 10000).ToString("0000");
                    var check = new SqlCommand("SELECT COUNT(*) FROM Assignments WHERE AssignmentCode = @Code", con);
                    check.Parameters.AddWithValue("@Code", code);
                    if (Convert.ToInt32(check.ExecuteScalar()) == 0)
                        return code;
                }
            }
            return "PERS-ASSIGN-" + DateTime.UtcNow.Ticks.ToString().Substring(10, 4);
        }

        private static string NextCollegeCode(string assignmentName)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                return NextCollegeCode(assignmentName, con);
            }
        }

        private static string NextCollegeCode(string assignmentName, SqlConnection con)
        {
            string prefix = CodePrefixFromName(assignmentName);
            for (int attempt = 0; attempt < 40; attempt++)
            {
                string code = prefix + "-" + (Math.Abs(Guid.NewGuid().GetHashCode()) % 10000).ToString("0000");
                using (var check = new SqlCommand("SELECT COUNT(*) FROM Assignments WHERE AssignmentCode = @Code", con))
                {
                    check.Parameters.AddWithValue("@Code", code);
                    if (Convert.ToInt32(check.ExecuteScalar()) == 0)
                        return code;
                }
            }
            string ticks = DateTime.UtcNow.Ticks.ToString();
            return prefix + "-" + ticks.Substring(Math.Max(0, ticks.Length - 4));
        }

        internal static string CodePrefixFromName(string name)
        {
            string raw = (name ?? "").Trim().ToUpperInvariant();
            var tokens = new List<string>();
            foreach (string part in Regex.Split(raw, @"[^A-Z0-9]+"))
            {
                if (string.IsNullOrEmpty(part))
                    continue;
                if (part == "A" || part == "AN" || part == "THE" || part == "OF" || part == "FOR"
                    || part == "AND" || part == "TO" || part == "IN" || part == "ON"
                    || part == "ASSIGNMENT" || part == "ASSIGNMENTS" || part == "GROUP" || part == "GROUPS")
                    continue;
                tokens.Add(part);
            }

            if (tokens.Count == 0)
            {
                string letters = Regex.Replace(raw, @"[^A-Z0-9]", "");
                if (letters.Length >= 2)
                    return letters.Length <= 6 ? letters : letters.Substring(0, 6);
                return "ASN";
            }

            if (tokens[0].Length >= 2 && tokens[0].Length <= 5)
                return tokens[0];

            if (tokens.Count >= 2)
            {
                var prefix = new StringBuilder();
                for (int i = 0; i < tokens.Count && prefix.Length < 6; i++)
                    prefix.Append(tokens[i][0]);
                if (prefix.Length >= 2)
                    return prefix.ToString();
            }

            string token = tokens[0];
            return token.Length <= 6 ? token : token.Substring(0, 4);
        }

        public static string CreateSubgroup(int userId, string code, string groupName, out int groupId)
        {
            groupId = 0;
            AssignmentRecord assignment = FindByCode(code);
            UserAccount actor = AuthService.FindById(userId);
            if (actor == null || !RoleAccess.IsStudent(actor.Role))
                return "Only students can create a subgroup.";
            if (assignment == null)
                return "That assignment code is not valid.";
            if (assignment.Deadline < DateTime.Now)
                return "The assignment deadline has passed.";
            if (string.IsNullOrWhiteSpace(groupName))
                return "Enter a group name.";
            if (IsOnAssignment(assignment.AssignmentID, userId))
                return "You already belong to a subgroup for this assignment.";

            using (var con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                using (var tx = con.BeginTransaction())
                {
                    var insert = new SqlCommand(
                        @"INSERT INTO AssignmentGroups (AssignmentID, GroupName, LeaderID, IsFinalized, CreatedAt, IsDeleted)
                          VALUES (@AssignmentID, @Name, @LeaderID, 0, GETDATE(), 0);
                          SELECT CAST(SCOPE_IDENTITY() AS INT);", con, tx);
                    insert.Parameters.AddWithValue("@AssignmentID", assignment.AssignmentID);
                    insert.Parameters.AddWithValue("@Name", groupName.Trim());
                    insert.Parameters.AddWithValue("@LeaderID", userId);
                    groupId = Convert.ToInt32(insert.ExecuteScalar());

                    var member = new SqlCommand(
                        @"INSERT INTO AssignmentMembers (GroupID, UserID, Responsibility, JoinedAt)
                          VALUES (@GroupID, @UserID, N'Leader', GETDATE())", con, tx);
                    member.Parameters.AddWithValue("@GroupID", groupId);
                    member.Parameters.AddWithValue("@UserID", userId);
                    member.ExecuteNonQuery();
                    tx.Commit();
                }
            }

            AuthService.WriteAudit(userId, "AssignmentGroupCreated", "AssignmentGroup", groupId, groupName.Trim(), null);
            int connectId;
            ConnectService.CreateGroupForAssignmentSubgroup(groupId, out connectId);
            return null;
        }

        public static string InviteMember(int groupId, int actorId, string actorRole, string email)
        {
            AssignmentAccess access = GetAccess(groupId, actorId, actorRole);
            if (!access.CanInvite)
                return access.IsDeadlineLocked
                    ? "The assignment deadline has passed. Students cannot invite members unless faculty extends it."
                    : (access.Group != null && access.Group.IsFinalized
                        ? "This group is finalized. Only the faculty owner can change membership."
                        : "Only the group leader can invite members.");

            EmailLookupResult lookup = EventService.LookupByExactEmail(email);
            if (!lookup.Found)
                return lookup.Message;
            if (!RoleAccess.IsStudent(lookup.Role))
                return "Only students can be invited to an assignment subgroup.";

            if (IsOnAssignment(access.Group.AssignmentID, lookup.UserID.Value))
                return "That student already belongs to a subgroup for this assignment.";

            email = email.Trim().ToLowerInvariant();
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"INSERT INTO AssignmentInvitations (AssignmentID, GroupID, Email, InvitedUserID, Status, CreatedBy, CreatedAt)
                  VALUES (@AssignmentID, @GroupID, @Email, @UserID, N'Pending', @Actor, GETDATE())", con))
            {
                cmd.Parameters.AddWithValue("@AssignmentID", access.Group.AssignmentID);
                cmd.Parameters.AddWithValue("@GroupID", groupId);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@UserID", lookup.UserID.Value);
                cmd.Parameters.AddWithValue("@Actor", actorId);
                con.Open();
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (SqlException)
                {
                    return "An invitation is already pending for that email.";
                }
            }

            Notify(lookup.UserID.Value, "Assignment group invitation",
                "You were invited to " + access.Group.GroupName + " for " + access.Group.Assignment.AssignmentName + ".",
                groupId);

            UserAccount invitee = AuthService.FindById(lookup.UserID.Value);
            if (invitee != null && !string.IsNullOrWhiteSpace(invitee.Email))
            {
                MailSender.Send(invitee.Email, "Invited to " + access.Group.GroupName,
                    MailComposer.Build(
                        null,
                        "You were invited to " + access.Group.GroupName + " for " + access.Group.Assignment.AssignmentName + ".",
                        null,
                        null,
                        "Open My Assignments",
                        MailSender.AbsoluteUrl("~/Modules/Assignments/MyAssignments.aspx"),
                        MailComposer.FirstName(invitee.FullName)));
            }
            return null;
        }

        public static DataTable ListMyInvites(int userId)
        {
            UserAccount user = AuthService.FindById(userId);
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT i.InvitationID, i.GroupID, g.GroupName, a.AssignmentName, a.AssignmentCode,
                         ISNULL(a.AssignmentType, N'College') AS AssignmentType
                  FROM AssignmentInvitations i
                  INNER JOIN AssignmentGroups g ON g.GroupID = i.GroupID AND g.IsDeleted = 0
                  INNER JOIN Assignments a ON a.AssignmentID = i.AssignmentID AND a.IsDeleted = 0
                  WHERE i.Status = N'Pending'
                    AND (i.InvitedUserID = @UserID OR i.Email = @Email)
                  ORDER BY i.CreatedAt DESC", con))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@Email", user == null ? "" : (user.Email ?? "").Trim().ToLowerInvariant());
                var table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                return table;
            }
        }

        public static string AcceptInvitation(int invitationId, int userId)
        {
            int groupId;
            using (var con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                var read = new SqlCommand(
                    @"SELECT InvitationID, AssignmentID, GroupID, Email, Status
                      FROM AssignmentInvitations WHERE InvitationID = @ID", con);
                read.Parameters.AddWithValue("@ID", invitationId);
                int assignmentId;
                using (SqlDataReader reader = read.ExecuteReader())
                {
                    if (!reader.Read())
                        return "Invitation not found.";
                    if (!string.Equals(Convert.ToString(reader["Status"]), "Pending", StringComparison.OrdinalIgnoreCase))
                        return "That invitation is no longer pending.";
                    assignmentId = Convert.ToInt32(reader["AssignmentID"]);
                    groupId = Convert.ToInt32(reader["GroupID"]);
                }

                AssignmentGroupRecord group = GetGroup(groupId);
                if (group == null || group.IsDeleted)
                    return "That group is no longer available.";
                if (group.IsFinalized)
                    return "That group is already finalized.";
                if (IsOnAssignment(assignmentId, userId))
                    return "You already belong to a subgroup for this assignment.";

                var accept = new SqlCommand(
                    "UPDATE AssignmentInvitations SET Status = N'Accepted', InvitedUserID = @UserID WHERE InvitationID = @ID", con);
                accept.Parameters.AddWithValue("@UserID", userId);
                accept.Parameters.AddWithValue("@ID", invitationId);
                accept.ExecuteNonQuery();

                var member = new SqlCommand(
                    @"INSERT INTO AssignmentMembers (GroupID, UserID, Responsibility, JoinedAt)
                      VALUES (@GroupID, @UserID, N'Member', GETDATE())", con);
                member.Parameters.AddWithValue("@GroupID", groupId);
                member.Parameters.AddWithValue("@UserID", userId);
                member.ExecuteNonQuery();
            }

            ConnectService.SyncUserToAssignmentConnect(groupId, userId);
            return null;
        }

        public static string DeclineInvitation(int invitationId, int userId)
        {
            UserAccount user = AuthService.FindById(userId);
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"UPDATE AssignmentInvitations SET Status = N'Declined'
                  WHERE InvitationID = @ID AND Status = N'Pending'
                    AND (InvitedUserID = @UserID OR Email = @Email)", con))
            {
                cmd.Parameters.AddWithValue("@ID", invitationId);
                cmd.Parameters.AddWithValue("@UserID", userId);
                cmd.Parameters.AddWithValue("@Email", user == null ? "" : (user.Email ?? "").Trim().ToLowerInvariant());
                con.Open();
                if (cmd.ExecuteNonQuery() == 0)
                    return "Invitation not found.";
            }
            return null;
        }

        public static string Finalize(int groupId, int actorId, string actorRole)
        {
            AssignmentAccess access = GetAccess(groupId, actorId, actorRole);
            if (!access.CanManageStructure && !access.CanCorrect)
                return "Only the group leader can finalize, or the faculty owner after review.";
            if (access.Group.IsFinalized)
                return "This group is already finalized.";

            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                "UPDATE AssignmentGroups SET IsFinalized = 1 WHERE GroupID = @GroupID", con))
            {
                cmd.Parameters.AddWithValue("@GroupID", groupId);
                con.Open();
                cmd.ExecuteNonQuery();
            }

            AuthService.WriteAudit(actorId, "AssignmentGroupFinalized", "AssignmentGroup", groupId, null, null);
            NotifyGroupMembers(groupId, actorId, "Assignment group finalized",
                "The subgroup \"" + access.Group.GroupName + "\" is finalized. Open the contribution report to see recorded scores.",
                "Assignment");
            return null;
        }

        public static string ChangeLeader(int groupId, int actorId, string actorRole, int newLeaderId)
        {
            AssignmentAccess access = GetAccess(groupId, actorId, actorRole);
            if (!access.CanAssignLeader)
                return "Only the current group leader can assign a new leader before the group is finalized. After that, the faculty owner can correct it.";
            if (!IsMember(groupId, newLeaderId))
                return "The new leader must already be a member of the group.";

            using (var con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                var clear = new SqlCommand(
                    "UPDATE AssignmentMembers SET Responsibility = N'Member' WHERE GroupID = @GroupID AND Responsibility = N'Leader'", con);
                clear.Parameters.AddWithValue("@GroupID", groupId);
                clear.ExecuteNonQuery();

                var set = new SqlCommand(
                    "UPDATE AssignmentMembers SET Responsibility = N'Leader' WHERE GroupID = @GroupID AND UserID = @UserID", con);
                set.Parameters.AddWithValue("@GroupID", groupId);
                set.Parameters.AddWithValue("@UserID", newLeaderId);
                set.ExecuteNonQuery();

                var group = new SqlCommand(
                    "UPDATE AssignmentGroups SET LeaderID = @UserID WHERE GroupID = @GroupID", con);
                group.Parameters.AddWithValue("@UserID", newLeaderId);
                group.Parameters.AddWithValue("@GroupID", groupId);
                group.ExecuteNonQuery();
            }

            AuthService.WriteAudit(actorId, "AssignmentLeaderChanged", "AssignmentGroup", groupId, newLeaderId.ToString(), null);
            int? connectId = ConnectService.FindGroupIdByAssignmentGroup(groupId);
            if (connectId.HasValue)
                ConnectService.TransferOwner(connectId.Value, newLeaderId);
            return null;
        }

        public static string SoftDeleteGroup(int groupId, int actorId, string actorRole)
        {
            AssignmentAccess access = GetAccess(groupId, actorId, actorRole);
            if (!access.CanCorrect)
                return "Only the faculty owner can remove a subgroup.";

            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                "UPDATE AssignmentGroups SET IsDeleted = 1 WHERE GroupID = @GroupID", con))
            {
                cmd.Parameters.AddWithValue("@GroupID", groupId);
                con.Open();
                cmd.ExecuteNonQuery();
            }

            AuthService.WriteAudit(actorId, "AssignmentGroupDeleted", "AssignmentGroup", groupId, null, null);
            ConnectService.CloseAssignmentConnect(groupId);
            return null;
        }

        public static string RenameGroup(int groupId, int actorId, string actorRole, string name)
        {
            AssignmentAccess access = GetAccess(groupId, actorId, actorRole);
            if (!access.CanCorrect && !access.CanManageStructure)
                return "This group cannot be renamed.";
            if (string.IsNullOrWhiteSpace(name))
                return "Enter a group name.";

            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                "UPDATE AssignmentGroups SET GroupName = @Name WHERE GroupID = @GroupID", con))
            {
                cmd.Parameters.AddWithValue("@Name", name.Trim());
                cmd.Parameters.AddWithValue("@GroupID", groupId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
            ConnectService.RenameAssignmentSubgroupConnect(groupId, name.Trim());
            return null;
        }

        public static string AddTask(int groupId, int actorId, string actorRole, string title, string description, int? parentTaskId, int? assigneeId, DateTime? dueDate)
        {
            AssignmentAccess access = GetAccess(groupId, actorId, actorRole);
            if (!access.CanManageStructure)
                return access.IsDeadlineLocked
                    ? "The assignment deadline has passed. Students cannot add tasks unless faculty extends it."
                    : (access.Group != null && access.Group.IsFinalized
                        ? "Finalized groups cannot add tasks. Ask Faculty."
                        : "Only the group leader can add tasks.");
            if (string.IsNullOrWhiteSpace(title))
                return "Task title is required.";
            if (assigneeId.HasValue && !IsMember(groupId, assigneeId.Value))
                return "Tasks can only be assigned to group members.";

            int sort;
            using (var con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                var max = new SqlCommand(
                    "SELECT ISNULL(MAX(SortOrder), 0) + 1 FROM AssignmentTasks WHERE GroupID = @GroupID AND ISNULL(ParentTaskID, 0) = ISNULL(@Parent, 0)", con);
                max.Parameters.AddWithValue("@GroupID", groupId);
                max.Parameters.AddWithValue("@Parent", (object)parentTaskId ?? 0);
                sort = Convert.ToInt32(max.ExecuteScalar());

                var insert = new SqlCommand(
                    @"INSERT INTO AssignmentTasks (GroupID, Title, Description, AssignedUserID, SortOrder, Status, DueDate, CreatedAt, UpdatedAt, ParentTaskID, LastChangedBy, IsDeleted)
                      VALUES (@GroupID, @Title, @Description, @Assignee, @Sort, N'ToDo', @Due, GETDATE(), GETDATE(), @Parent, @Actor, 0)", con);
                insert.Parameters.AddWithValue("@GroupID", groupId);
                insert.Parameters.AddWithValue("@Title", title.Trim());
                insert.Parameters.AddWithValue("@Description", (object)description ?? DBNull.Value);
                insert.Parameters.AddWithValue("@Assignee", (object)assigneeId ?? DBNull.Value);
                insert.Parameters.AddWithValue("@Sort", sort);
                insert.Parameters.AddWithValue("@Due", (object)dueDate ?? DBNull.Value);
                insert.Parameters.AddWithValue("@Parent", (object)parentTaskId ?? DBNull.Value);
                insert.Parameters.AddWithValue("@Actor", actorId);
                insert.ExecuteNonQuery();
            }

            return null;
        }

        public static string AddTasks(int groupId, int actorId, string actorRole, IList<string> titles, IList<int> assigneeIds, int? parentTaskId, DateTime? dueDate)
        {
            AssignmentAccess access = GetAccess(groupId, actorId, actorRole);
            if (!access.CanManageStructure)
                return access.IsDeadlineLocked
                    ? "The assignment deadline has passed. Students cannot add tasks unless faculty extends it."
                    : (access.Group != null && access.Group.IsFinalized
                        ? "Finalized groups cannot add tasks. Ask Faculty."
                        : "Only the group leader can add tasks.");

            var cleanTitles = new List<string>();
            if (titles != null)
            {
                foreach (string raw in titles)
                {
                    if (string.IsNullOrWhiteSpace(raw))
                        continue;
                    string title = raw.Trim();
                    if (title.Length > 200)
                        return "Keep each task title to 200 characters or fewer.";
                    cleanTitles.Add(title);
                    if (cleanTitles.Count > 20)
                        return "Allocate at most 20 tasks at a time.";
                }
            }
            if (cleanTitles.Count == 0)
                return "Enter at least one task title. Put each title on its own line.";

            var assignees = new List<int>();
            if (assigneeIds != null)
            {
                foreach (int assigneeId in assigneeIds)
                {
                    if (assigneeId <= 0 || assignees.Contains(assigneeId))
                        continue;
                    if (!IsMember(groupId, assigneeId))
                        return "Tasks can only be assigned to group members.";
                    assignees.Add(assigneeId);
                }
            }
            if (assignees.Count == 0)
                assignees.Add(0);

            using (var con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                foreach (string title in cleanTitles)
                {
                    foreach (int assigneeId in assignees)
                    {
                        var max = new SqlCommand(
                            "SELECT ISNULL(MAX(SortOrder), 0) + 1 FROM AssignmentTasks WHERE GroupID = @GroupID AND ISNULL(ParentTaskID, 0) = ISNULL(@Parent, 0)", con);
                        max.Parameters.AddWithValue("@GroupID", groupId);
                        max.Parameters.AddWithValue("@Parent", (object)parentTaskId ?? 0);
                        int sort = Convert.ToInt32(max.ExecuteScalar());

                        var insert = new SqlCommand(
                            @"INSERT INTO AssignmentTasks (GroupID, Title, Description, AssignedUserID, SortOrder, Status, DueDate, CreatedAt, UpdatedAt, ParentTaskID, LastChangedBy, IsDeleted)
                              VALUES (@GroupID, @Title, NULL, @Assignee, @Sort, N'ToDo', @Due, GETDATE(), GETDATE(), @Parent, @Actor, 0)", con);
                        insert.Parameters.AddWithValue("@GroupID", groupId);
                        insert.Parameters.AddWithValue("@Title", title);
                        insert.Parameters.AddWithValue("@Assignee", assigneeId > 0 ? (object)assigneeId : DBNull.Value);
                        insert.Parameters.AddWithValue("@Sort", sort);
                        insert.Parameters.AddWithValue("@Due", (object)dueDate ?? DBNull.Value);
                        insert.Parameters.AddWithValue("@Parent", (object)parentTaskId ?? DBNull.Value);
                        insert.Parameters.AddWithValue("@Actor", actorId);
                        insert.ExecuteNonQuery();
                    }
                }
            }

            return null;
        }

        public static DataTable SearchStudentsForInvite(int groupId, int actorId, string actorRole, string query)
        {
            var table = new DataTable();
            AssignmentAccess access = GetAccess(groupId, actorId, actorRole);
            if (!access.CanInvite || access.Group == null)
                return table;

            string term = (query ?? "").Trim();
            if (term.Length < 2)
                return table;

            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT TOP 20 u.UserID, u.FullName, u.Email, u.Username,
                         ISNULL(u.InstitutionalID, N'') AS InstitutionalID,
                         CASE WHEN EXISTS (
                                SELECT 1 FROM AssignmentInvitations i
                                WHERE i.AssignmentID = @AssignmentID AND i.Status = N'Pending'
                                  AND (i.InvitedUserID = u.UserID OR LOWER(i.Email) = LOWER(u.Email))
                             )
                             THEN N'Pending' ELSE N'Available' END AS InviteStatus
                  FROM Users u
                  INNER JOIN Roles r ON r.RoleID = u.RoleID
                  WHERE ISNULL(u.IsDeleted, 0) = 0
                    AND ISNULL(u.IsActive, 1) = 1
                    AND r.RoleName = N'Student'
                    AND (
                        u.FullName LIKE @Q OR
                        u.Email LIKE @Q OR
                        u.Username LIKE @Q OR
                        ISNULL(u.InstitutionalID, N'') LIKE @Q
                    )
                    AND NOT EXISTS (
                        SELECT 1
                        FROM AssignmentMembers m
                        INNER JOIN AssignmentGroups g ON g.GroupID = m.GroupID AND g.IsDeleted = 0
                        WHERE g.AssignmentID = @AssignmentID AND m.UserID = u.UserID
                    )
                  ORDER BY
                    CASE WHEN u.Email = @Exact OR u.Username = @Exact THEN 0
                         WHEN u.FullName LIKE @Prefix THEN 1
                         ELSE 2 END,
                    u.FullName", con))
            {
                string like = LikeContains(term);
                cmd.Parameters.AddWithValue("@AssignmentID", access.Group.AssignmentID);
                cmd.Parameters.AddWithValue("@Q", like);
                cmd.Parameters.AddWithValue("@Exact", term);
                cmd.Parameters.AddWithValue("@Prefix", LikePrefix(term));
                new SqlDataAdapter(cmd).Fill(table);
            }
            return table;
        }

        public static string InviteMemberByUserId(int groupId, int actorId, string actorRole, int inviteeUserId)
        {
            UserAccount user = AuthService.FindById(inviteeUserId);
            if (user == null || user.IsDeleted)
                return "No DTAS user available.";
            if (string.IsNullOrWhiteSpace(user.Email))
                return "That student does not have an email on file.";
            return InviteMember(groupId, actorId, actorRole, user.Email);
        }

        private static string LikeContains(string query)
        {
            return "%" + EscapeLike(query) + "%";
        }

        private static string LikePrefix(string query)
        {
            return EscapeLike(query) + "%";
        }

        private static string EscapeLike(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";
            return value.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");
        }

        public static string UpdateTask(int taskId, int actorId, string actorRole, int? assigneeId, string status)
        {
            int groupId;
            int? currentAssignee;
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                "SELECT GroupID, AssignedUserID FROM AssignmentTasks WHERE TaskID = @ID AND IsDeleted = 0", con))
            {
                cmd.Parameters.AddWithValue("@ID", taskId);
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return "Task not found.";
                    groupId = Convert.ToInt32(reader["GroupID"]);
                    currentAssignee = reader["AssignedUserID"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["AssignedUserID"]);
                }
            }

            AssignmentAccess access = GetAccess(groupId, actorId, actorRole);
            bool changingAssignee = assigneeId.HasValue && assigneeId != currentAssignee;
            if (changingAssignee && !access.CanManageStructure)
                return "Finalized groups cannot reassign tasks. Ask Faculty or Admin.";
            if (!access.CanUpdateTaskStatus(currentAssignee, actorId))
                return access.IsDeadlineLocked
                    ? "The assignment deadline has passed. Students cannot update work unless faculty extends it."
                    : "You can only update tasks assigned to you.";
            if (!string.IsNullOrEmpty(status) && !IsValidStatus(status))
                return "Invalid task status.";
            if (assigneeId.HasValue && !IsMember(groupId, assigneeId.Value))
                return "Tasks can only be assigned to group members.";

            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"UPDATE AssignmentTasks
                  SET AssignedUserID = COALESCE(@Assignee, AssignedUserID),
                      Status = COALESCE(@Status, Status),
                      UpdatedAt = GETDATE(),
                      LastChangedBy = @Actor
                  WHERE TaskID = @ID", con))
            {
                cmd.Parameters.AddWithValue("@Assignee", (object)assigneeId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Status", string.IsNullOrEmpty(status) ? (object)DBNull.Value : status);
                cmd.Parameters.AddWithValue("@Actor", actorId);
                cmd.Parameters.AddWithValue("@ID", taskId);
                con.Open();
                cmd.ExecuteNonQuery();
            }

            return null;
        }

        public static DataTable ListTasks(int groupId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT t.TaskID, t.Title, t.Description, t.Status, t.DueDate, t.SortOrder, t.ParentTaskID,
                         t.AssignedUserID, u.FullName AS AssigneeName,
                         t.SubmissionNote, t.SubmissionFilePath, t.SubmissionFileName, t.SubmittedAt
                  FROM AssignmentTasks t
                  LEFT JOIN Users u ON u.UserID = t.AssignedUserID
                  WHERE t.GroupID = @GroupID AND t.IsDeleted = 0
                  ORDER BY ISNULL(t.ParentTaskID, t.TaskID), t.ParentTaskID, t.SortOrder, t.TaskID", con))
            {
                cmd.Parameters.AddWithValue("@GroupID", groupId);
                var table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                return table;
            }
        }

        public static AssignmentTaskRecord GetTask(int taskId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT TaskID, GroupID, AssignedUserID, Title, Status,
                         SubmissionNote, SubmissionFilePath, SubmissionFileName, SubmittedAt, SubmittedBy
                  FROM AssignmentTasks
                  WHERE TaskID = @ID AND IsDeleted = 0", con))
            {
                cmd.Parameters.AddWithValue("@ID", taskId);
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;
                    return new AssignmentTaskRecord
                    {
                        TaskID = Convert.ToInt32(reader["TaskID"]),
                        GroupID = Convert.ToInt32(reader["GroupID"]),
                        AssignedUserID = reader["AssignedUserID"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["AssignedUserID"]),
                        Title = Convert.ToString(reader["Title"]),
                        Status = Convert.ToString(reader["Status"]),
                        SubmissionNote = reader["SubmissionNote"] == DBNull.Value ? null : Convert.ToString(reader["SubmissionNote"]),
                        SubmissionFilePath = reader["SubmissionFilePath"] == DBNull.Value ? null : Convert.ToString(reader["SubmissionFilePath"]),
                        SubmissionFileName = reader["SubmissionFileName"] == DBNull.Value ? null : Convert.ToString(reader["SubmissionFileName"]),
                        SubmittedAt = reader["SubmittedAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["SubmittedAt"]),
                        SubmittedBy = reader["SubmittedBy"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["SubmittedBy"])
                    };
                }
            }
        }

        public static string SubmitWork(int taskId, int actorId, string actorRole, string note, string linkText, IList<HttpPostedFile> files, HttpServerUtility server)
        {
            AssignmentTaskRecord task = GetTask(taskId);
            if (task == null)
                return "Task not found.";

            AssignmentAccess access = GetAccess(task.GroupID, actorId, actorRole);
            if (!access.CanSubmitWork(task.AssignedUserID, actorId, task.Status))
                return "Only the assigned member can add files, notes, or links to this task.";

            note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
            if (note != null && note.Length > 2000)
                return "The note must be 2000 characters or fewer.";

            List<string> links;
            string linkError = ParseWorkLinks(linkText, out links);
            if (linkError != null)
                return linkError;

            var uploads = new List<HttpPostedFile>();
            if (files != null)
            {
                foreach (HttpPostedFile file in files)
                {
                    if (file != null && file.ContentLength > 0)
                        uploads.Add(file);
                }
            }

            if (uploads.Count > 20)
                return "Upload up to 20 files at a time.";

            int existingFiles = CountTaskFiles(taskId);
            if (existingFiles == 0 && !string.IsNullOrEmpty(task.SubmissionFilePath))
                existingFiles = 1;
            int existingLinks = CountTaskLinks(taskId);

            if (note == null && uploads.Count == 0 && links.Count == 0 && existingFiles == 0 && existingLinks == 0)
                return "Add a note, a link, or at least one file.";

            var saved = new List<KeyValuePair<string, string>>();
            for (int i = 0; i < uploads.Count; i++)
            {
                string relativePath;
                string originalName;
                string saveError = SaveSubmissionFile(uploads[i], server, task.GroupID, taskId, i, out relativePath, out originalName);
                if (saveError != null)
                    return saveError;
                saved.Add(new KeyValuePair<string, string>(relativePath, originalName));
            }

            string lastPath = saved.Count > 0 ? saved[saved.Count - 1].Key : task.SubmissionFilePath;
            string lastName = saved.Count > 0 ? saved[saved.Count - 1].Value : task.SubmissionFileName;

            string history = "Submitted work";
            if (note != null)
                history += ": " + (note.Length > 200 ? note.Substring(0, 200) : note);
            if (saved.Count > 0)
            {
                var names = new List<string>();
                foreach (KeyValuePair<string, string> item in saved)
                    names.Add(item.Value);
                history += " [" + string.Join(", ", names.ToArray()) + "]";
            }
            if (links.Count > 0)
                history += " " + string.Join(" ", links.ToArray());

            using (var con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                using (var cmd = new SqlCommand(
                    @"UPDATE AssignmentTasks
                      SET SubmissionNote = CASE WHEN @Note IS NULL THEN SubmissionNote ELSE @Note END,
                          SubmissionFilePath = COALESCE(@Path, SubmissionFilePath),
                          SubmissionFileName = COALESCE(@FileName, SubmissionFileName),
                          SubmittedAt = GETDATE(),
                          SubmittedBy = @Actor,
                          UpdatedAt = GETDATE(),
                          LastChangedBy = @Actor
                      WHERE TaskID = @ID", con))
                {
                    cmd.Parameters.AddWithValue("@Note", (object)note ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Path", (object)lastPath ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FileName", (object)lastName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Actor", actorId);
                    cmd.Parameters.AddWithValue("@ID", taskId);
                    cmd.ExecuteNonQuery();
                }

                foreach (KeyValuePair<string, string> item in saved)
                {
                    using (var insert = new SqlCommand(
                        @"INSERT INTO AssignmentTaskFiles (TaskID, FilePath, FileName, UploadedBy, UploadedAt)
                          VALUES (@TaskID, @Path, @FileName, @Actor, GETDATE())", con))
                    {
                        insert.Parameters.AddWithValue("@TaskID", taskId);
                        insert.Parameters.AddWithValue("@Path", item.Key);
                        insert.Parameters.AddWithValue("@FileName", item.Value);
                        insert.Parameters.AddWithValue("@Actor", actorId);
                        insert.ExecuteNonQuery();
                    }
                }

                foreach (string url in links)
                {
                    using (var insert = new SqlCommand(
                        @"INSERT INTO AssignmentTaskLinks (TaskID, Url, AddedBy, AddedAt)
                          VALUES (@TaskID, @Url, @Actor, GETDATE())", con))
                    {
                        insert.Parameters.AddWithValue("@TaskID", taskId);
                        insert.Parameters.AddWithValue("@Url", url);
                        insert.Parameters.AddWithValue("@Actor", actorId);
                        insert.ExecuteNonQuery();
                    }
                }

                using (var hist = new SqlCommand(
                    @"INSERT INTO AssignmentTaskHistory
                        (TaskID, OldStatus, NewStatus, PreviousAssignee, NewAssignee, ChangedBy, ChangedAt, Description)
                      VALUES
                        (@TaskID, @Status, @Status, @Assignee, @Assignee, @Actor, GETDATE(), @Description)", con))
                {
                    hist.Parameters.AddWithValue("@TaskID", taskId);
                    hist.Parameters.AddWithValue("@Status", (object)task.Status ?? DBNull.Value);
                    hist.Parameters.AddWithValue("@Assignee", (object)task.AssignedUserID ?? DBNull.Value);
                    hist.Parameters.AddWithValue("@Actor", actorId);
                    hist.Parameters.AddWithValue("@Description", history);
                    hist.ExecuteNonQuery();
                }
            }

            return null;
        }

        public static DataTable ListTaskComments(int taskId)
        {
            EnsureSchema();
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT c.CommentID, c.Comment, c.CreatedAt, ISNULL(u.FullName, u.Username) AS FullName
                  FROM AssignmentTaskComments c
                  INNER JOIN Users u ON u.UserID = c.UserID
                  WHERE c.TaskID = @TaskID
                  ORDER BY c.CreatedAt ASC, c.CommentID ASC", con))
            {
                cmd.Parameters.AddWithValue("@TaskID", taskId);
                var table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                return table;
            }
        }

        public static string AddTaskComment(int taskId, int actorId, string actorRole, string comment)
        {
            AssignmentTaskRecord task = GetTask(taskId);
            if (task == null)
                return "Task not found.";

            AssignmentAccess access = GetAccess(task.GroupID, actorId, actorRole);
            if (!access.CanComment)
                return "Only group members can comment on this task.";

            comment = (comment ?? "").Trim();
            if (comment.Length == 0)
                return "Enter a comment.";
            if (comment.Length > 2000)
                return "The comment must be 2000 characters or fewer.";

            using (var con = new SqlConnection(AuthService.ConnectionString))
            {
                con.Open();
                using (var cmd = new SqlCommand(
                    @"INSERT INTO AssignmentTaskComments (TaskID, UserID, Comment, CreatedAt)
                      VALUES (@TaskID, @UserID, @Comment, GETDATE())", con))
                {
                    cmd.Parameters.AddWithValue("@TaskID", taskId);
                    cmd.Parameters.AddWithValue("@UserID", actorId);
                    cmd.Parameters.AddWithValue("@Comment", comment);
                    cmd.ExecuteNonQuery();
                }

                using (var hist = new SqlCommand(
                    @"INSERT INTO AssignmentTaskHistory
                        (TaskID, OldStatus, NewStatus, PreviousAssignee, NewAssignee, ChangedBy, ChangedAt, Description)
                      VALUES
                        (@TaskID, @Status, @Status, @Assignee, @Assignee, @Actor, GETDATE(), @Description)", con))
                {
                    string history = "Comment: " + (comment.Length > 200 ? comment.Substring(0, 200) : comment);
                    hist.Parameters.AddWithValue("@TaskID", taskId);
                    hist.Parameters.AddWithValue("@Status", (object)task.Status ?? DBNull.Value);
                    hist.Parameters.AddWithValue("@Assignee", (object)task.AssignedUserID ?? DBNull.Value);
                    hist.Parameters.AddWithValue("@Actor", actorId);
                    hist.Parameters.AddWithValue("@Description", history);
                    hist.ExecuteNonQuery();
                }
            }

            if (task.AssignedUserID.HasValue)
            {
                NotificationService.NotifyUsers(
                    new[] { task.AssignedUserID.Value },
                    actorId,
                    "New comment on assignment task",
                    "A comment was added on \"" + task.Title + "\".",
                    "Assignment",
                    task.GroupID,
                    "Assignment");
            }
            return null;
        }

        public static DataTable ListGroupLinks(int groupId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT l.LinkID, l.TaskID, l.Url, l.AddedAt, l.AddedBy
                  FROM AssignmentTaskLinks l
                  INNER JOIN AssignmentTasks t ON t.TaskID = l.TaskID AND t.IsDeleted = 0
                  WHERE t.GroupID = @GroupID
                  ORDER BY l.AddedAt, l.LinkID", con))
            {
                cmd.Parameters.AddWithValue("@GroupID", groupId);
                var table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                table.Columns.Add("DisplayLabel", typeof(string));
                foreach (DataRow row in table.Rows)
                    row["DisplayLabel"] = LinkDisplayLabel(Convert.ToString(row["Url"]));
                return table;
            }
        }

        public static string FormatNoteHtml(string note)
        {
            if (string.IsNullOrWhiteSpace(note))
                return "";

            var rx = new Regex(@"https?://[^\s<>""']+", RegexOptions.IgnoreCase);
            var sb = new StringBuilder();
            int last = 0;
            foreach (Match match in rx.Matches(note))
            {
                sb.Append(HttpUtility.HtmlEncode(note.Substring(last, match.Index - last)));
                string raw = match.Value.TrimEnd('.', ',', ';', ')', ']');
                string url;
                if (TryNormalizeHttpUrl(raw, out url))
                {
                    sb.Append("<a href=\"")
                        .Append(HttpUtility.HtmlAttributeEncode(url))
                        .Append("\" target=\"_blank\" rel=\"noopener noreferrer\" class=\"text-primary font-bold break-all\">")
                        .Append(HttpUtility.HtmlEncode(LinkDisplayLabel(url)))
                        .Append("</a>");
                }
                else
                {
                    sb.Append(HttpUtility.HtmlEncode(match.Value));
                }
                last = match.Index + match.Length;
            }
            sb.Append(HttpUtility.HtmlEncode(note.Substring(last)));
            return "<p class=\"text-sm text-on-surface mb-1\">" + sb + "</p>";
        }

        public static string ParseWorkLinks(string text, out List<string> urls)
        {
            urls = new List<string>();
            if (string.IsNullOrWhiteSpace(text))
                return null;

            string[] parts = text.Split(new[] { '\r', '\n', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string part in parts)
            {
                string url;
                if (!TryNormalizeHttpUrl(part.Trim(), out url))
                    return "Enter a full http or https link, such as a GitHub or Discord URL.";
                if (seen.Add(url))
                    urls.Add(url);
            }

            if (urls.Count > 10)
                return "Add up to 10 links at a time.";
            return null;
        }

        public static string ComposeGitHubUrl(string owner, string repo, out string error)
        {
            error = null;
            owner = owner == null ? "" : owner.Trim().Trim('/');
            repo = repo == null ? "" : repo.Trim().Trim('/');
            if (string.IsNullOrEmpty(owner) && string.IsNullOrEmpty(repo))
                return null;

            if (!string.IsNullOrEmpty(repo) &&
                (repo.IndexOf("github.com/", StringComparison.OrdinalIgnoreCase) >= 0
                 || repo.StartsWith("http", StringComparison.OrdinalIgnoreCase)))
            {
                string pasted;
                if (!TryNormalizeHttpUrl(repo, out pasted))
                {
                    error = "Enter a valid GitHub repository URL.";
                    return null;
                }
                Uri pastedUri;
                if (!Uri.TryCreate(pasted, UriKind.Absolute, out pastedUri)
                    || pastedUri.Host.IndexOf("github.com", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    error = "The repository link must be on github.com.";
                    return null;
                }
                return pasted;
            }

            if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(repo))
            {
                error = "Enter both the GitHub owner and repository name.";
                return null;
            }

            var token = new Regex(@"^[A-Za-z0-9_.-]+$");
            if (!token.IsMatch(owner) || !token.IsMatch(repo))
            {
                error = "GitHub owner and repository can only use letters, numbers, dots, hyphens, and underscores.";
                return null;
            }

            return "https://github.com/" + owner + "/" + repo;
        }

        public static string LinkDisplayLabel(string url)
        {
            Uri uri;
            if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out uri))
                return url ?? "";

            string host = (uri.Host ?? "").ToLowerInvariant();
            if (host.StartsWith("www."))
                host = host.Substring(4);

            string path = uri.AbsolutePath == "/" ? "" : uri.AbsolutePath.TrimEnd('/');
            string label = host + path;
            if (label.Length > 60)
                label = label.Substring(0, 57) + "...";
            return string.IsNullOrEmpty(label) ? host : label;
        }

        private static bool TryNormalizeHttpUrl(string value, out string url)
        {
            url = null;
            if (string.IsNullOrWhiteSpace(value))
                return false;

            value = value.Trim();
            if (value.IndexOf("://", StringComparison.Ordinal) < 0)
                value = "https://" + value;

            Uri uri;
            if (!Uri.TryCreate(value, UriKind.Absolute, out uri))
                return false;
            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                return false;
            if (string.IsNullOrWhiteSpace(uri.Host))
                return false;
            if (value.Length > 2000)
                return false;

            url = uri.AbsoluteUri;
            return true;
        }

        public static DataTable ListGroupFiles(int groupId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT f.FileID, f.TaskID, f.FileName, f.FilePath, f.UploadedAt, f.UploadedBy
                  FROM AssignmentTaskFiles f
                  INNER JOIN AssignmentTasks t ON t.TaskID = f.TaskID AND t.IsDeleted = 0
                  WHERE t.GroupID = @GroupID
                  ORDER BY f.UploadedAt, f.FileID", con))
            {
                cmd.Parameters.AddWithValue("@GroupID", groupId);
                var table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                return table;
            }
        }

        public static AssignmentTaskFileRecord GetTaskFile(int fileId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT f.FileID, f.TaskID, t.GroupID, f.FilePath, f.FileName
                  FROM AssignmentTaskFiles f
                  INNER JOIN AssignmentTasks t ON t.TaskID = f.TaskID AND t.IsDeleted = 0
                  WHERE f.FileID = @ID", con))
            {
                cmd.Parameters.AddWithValue("@ID", fileId);
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;
                    return new AssignmentTaskFileRecord
                    {
                        FileID = Convert.ToInt32(reader["FileID"]),
                        TaskID = Convert.ToInt32(reader["TaskID"]),
                        GroupID = Convert.ToInt32(reader["GroupID"]),
                        FilePath = Convert.ToString(reader["FilePath"]),
                        FileName = Convert.ToString(reader["FileName"])
                    };
                }
            }
        }

        public static AssignmentTaskFileRecord GetFirstTaskFile(int taskId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT TOP 1 f.FileID, f.TaskID, t.GroupID, f.FilePath, f.FileName
                  FROM AssignmentTaskFiles f
                  INNER JOIN AssignmentTasks t ON t.TaskID = f.TaskID AND t.IsDeleted = 0
                  WHERE f.TaskID = @TaskID
                  ORDER BY f.UploadedAt, f.FileID", con))
            {
                cmd.Parameters.AddWithValue("@TaskID", taskId);
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;
                    return new AssignmentTaskFileRecord
                    {
                        FileID = Convert.ToInt32(reader["FileID"]),
                        TaskID = Convert.ToInt32(reader["TaskID"]),
                        GroupID = Convert.ToInt32(reader["GroupID"]),
                        FilePath = Convert.ToString(reader["FilePath"]),
                        FileName = Convert.ToString(reader["FileName"])
                    };
                }
            }
        }

        private static int CountTaskFiles(int taskId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM AssignmentTaskFiles WHERE TaskID = @TaskID", con))
            {
                cmd.Parameters.AddWithValue("@TaskID", taskId);
                con.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private static int CountTaskLinks(int taskId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM AssignmentTaskLinks WHERE TaskID = @TaskID", con))
            {
                cmd.Parameters.AddWithValue("@TaskID", taskId);
                con.Open();
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public static string SubmissionPhysicalPath(string relativePath, HttpServerUtility server)
        {
            if (server == null || string.IsNullOrWhiteSpace(relativePath))
                return null;
            if (!relativePath.StartsWith("~/Uploads/AssignmentSubmissions/", StringComparison.OrdinalIgnoreCase))
                return null;
            return server.MapPath(relativePath);
        }

        public const int MaxSubmissionBytes = 1000 * 1024 * 1024;

        private static readonly HashSet<string> SubmissionExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
            ".jpg", ".jpeg", ".png", ".txt", ".zip"
        };

        private static string SaveSubmissionFile(HttpPostedFile file, HttpServerUtility server, int groupId, int taskId, int index, out string relativePath, out string originalName)
        {
            relativePath = null;
            originalName = Path.GetFileName(file.FileName ?? "");
            if (string.IsNullOrWhiteSpace(originalName))
                return "Choose a file to upload.";
            if (file.ContentLength > MaxSubmissionBytes)
                return "Each file must be 1000 MB or smaller.";

            string ext = Path.GetExtension(originalName);
            if (string.IsNullOrEmpty(ext) || !SubmissionExtensions.Contains(ext))
                return "Upload a PDF, Office document, image, text, or ZIP file.";

            if (originalName.Length > 200)
                originalName = originalName.Substring(0, 200);

            string folder = server.MapPath("~/Uploads/AssignmentSubmissions/" + groupId);
            Directory.CreateDirectory(folder);
            string stored = taskId + "_" + DateTime.UtcNow.Ticks + "_" + index + ext.ToLowerInvariant();
            file.SaveAs(Path.Combine(folder, stored));
            relativePath = "~/Uploads/AssignmentSubmissions/" + groupId + "/" + stored;
            return null;
        }

        private static void DeleteSubmissionFile(string relativePath, HttpServerUtility server)
        {
            try
            {
                string physical = SubmissionPhysicalPath(relativePath, server);
                if (!string.IsNullOrEmpty(physical) && File.Exists(physical))
                    File.Delete(physical);
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        public static DataTable ListMembers(int groupId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT m.UserID, m.Responsibility, m.JoinedAt, u.FullName, u.Email
                  FROM AssignmentMembers m
                  INNER JOIN Users u ON u.UserID = m.UserID
                  WHERE m.GroupID = @GroupID
                  ORDER BY CASE m.Responsibility WHEN N'Leader' THEN 0 ELSE 1 END, u.FullName", con))
            {
                cmd.Parameters.AddWithValue("@GroupID", groupId);
                var table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                return table;
            }
        }

        public static DataTable ListMemberUpdates(int groupId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT HistoryID, UserID, ChangedAt, OldStatus, NewStatus, Description, TaskID, TaskTitle,
                         SubmissionFilePath, SubmissionFileName
                  FROM (
                  SELECT h.HistoryID, h.ChangedBy AS UserID, h.ChangedAt, h.OldStatus, h.NewStatus,
                         h.Description, t.TaskID, t.Title AS TaskTitle,
                         t.SubmissionFilePath, t.SubmissionFileName
                  FROM AssignmentTaskHistory h
                  INNER JOIN AssignmentTasks t ON t.TaskID = h.TaskID AND t.IsDeleted = 0
                  WHERE t.GroupID = @GroupID AND h.ChangedBy IS NOT NULL
                  UNION ALL
                  SELECT -t.TaskID, t.SubmittedBy, t.SubmittedAt, t.Status, t.Status,
                         N'Submitted work', t.TaskID, t.Title,
                         t.SubmissionFilePath, t.SubmissionFileName
                  FROM AssignmentTasks t
                  WHERE t.GroupID = @GroupID AND t.IsDeleted = 0
                    AND t.SubmittedAt IS NOT NULL AND t.SubmittedBy IS NOT NULL
                    AND NOT EXISTS (
                        SELECT 1 FROM AssignmentTaskHistory h
                        WHERE h.TaskID = t.TaskID AND h.ChangedBy = t.SubmittedBy
                          AND h.Description LIKE N'Submitted work%'
                    )
                  ) updates
                  ORDER BY ChangedAt DESC", con))
            {
                cmd.Parameters.AddWithValue("@GroupID", groupId);
                var table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                table.Columns.Add("Summary", typeof(string));
                table.Columns.Add("HasFile", typeof(bool));
                foreach (DataRow row in table.Rows)
                {
                    row["Summary"] = FormatMemberUpdate(row);
                    bool submitted = Convert.ToString(row["Description"] ?? "").StartsWith("Submitted work", StringComparison.OrdinalIgnoreCase);
                    row["HasFile"] = submitted && row["SubmissionFilePath"] != DBNull.Value
                        && !string.IsNullOrWhiteSpace(Convert.ToString(row["SubmissionFilePath"]));
                }
                return table;
            }
        }

        public static string FormatMemberUpdate(DataRow row)
        {
            if (row == null)
                return "";
            string title = HttpUtility.HtmlEncode(Convert.ToString(row["TaskTitle"]));
            string description = Convert.ToString(row["Description"]);
            string oldStatus = StatusLabel(row["OldStatus"]);
            string newStatus = StatusLabel(row["NewStatus"]);

            if (string.Equals(description, "Status changed", StringComparison.OrdinalIgnoreCase))
                return "Changed \"" + title + "\" from " + oldStatus + " to " + newStatus;
            if (string.Equals(description, "Assignee changed", StringComparison.OrdinalIgnoreCase))
                return "Changed the owner of \"" + title + "\"";
            if (!string.IsNullOrWhiteSpace(description) && description.StartsWith("Submitted work", StringComparison.OrdinalIgnoreCase))
            {
                string extra = description.Length > "Submitted work".Length
                    ? description.Substring("Submitted work".Length).TrimStart(':', ' ')
                    : "";
                extra = HttpUtility.HtmlEncode(extra);
                return string.IsNullOrEmpty(extra)
                    ? "Submitted work on \"" + title + "\""
                    : "Submitted work on \"" + title + "\" - " + extra;
            }
            if (!string.IsNullOrWhiteSpace(description) && description.StartsWith("Comment:", StringComparison.OrdinalIgnoreCase))
            {
                string extra = description.Substring("Comment:".Length).Trim();
                extra = HttpUtility.HtmlEncode(extra);
                return string.IsNullOrEmpty(extra)
                    ? "Commented on \"" + title + "\""
                    : "Commented on \"" + title + "\" - " + extra;
            }
            if (string.IsNullOrWhiteSpace(description))
                return title;
            return HttpUtility.HtmlEncode(description) + " - " + title;
        }

        private static string StatusLabel(object status)
        {
            string value = status == null || status == DBNull.Value ? "" : status.ToString();
            switch (value)
            {
                case "ToDo": return "To Do";
                case "InProgress": return "In Progress";
                case "UnderReview": return "Under Review";
                case "Completed": return "Completed";
                case "Blocked": return "Blocked";
                default: return string.IsNullOrEmpty(value) ? "-" : value;
            }
        }

        public static DataSet GetProgress(int groupId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand("dbo.sp_GetAssignmentProgress", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@GroupID", groupId);
                var set = new DataSet();
                new SqlDataAdapter(cmd).Fill(set);
                return set;
            }
        }

        public static DataTable GetContribution(int groupId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand("dbo.sp_GetMemberContributionReport", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@GroupID", groupId);
                cmd.Parameters.AddWithValue("@UserID", DBNull.Value);
                var table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                return table;
            }
        }

        public static bool IsLockedForStudents(AssignmentRecord assignment)
        {
            if (assignment == null || assignment.IsDeleted)
                return true;
            if (string.Equals(assignment.Status, "Closed", StringComparison.OrdinalIgnoreCase))
                return true;
            return assignment.Deadline.Date < DateTime.Today;
        }

        public static string UpdateDeadline(int assignmentId, int actorId, string actorRole, DateTime newDeadline)
        {
            AssignmentRecord assignment = GetAssignment(assignmentId);
            if (assignment == null)
                return "Assignment not found.";
            if (assignment.CreatedBy != actorId)
                return "Only the person who created this assignment can change the deadline.";
            if (!RoleAccess.CanMonitorAssignments(actorRole) && !assignment.IsPersonal)
                return "Only faculty can change a college assignment deadline.";
            if (newDeadline <= DateTime.Now)
                return "Choose a deadline after now.";

            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                "UPDATE Assignments SET Deadline = @Deadline, Status = N'Active' WHERE AssignmentID = @ID", con))
            {
                cmd.Parameters.AddWithValue("@Deadline", newDeadline);
                cmd.Parameters.AddWithValue("@ID", assignmentId);
                con.Open();
                cmd.ExecuteNonQuery();
            }

            AuthService.WriteAudit(actorId, "AssignmentDeadlineChanged", "Assignment", assignmentId, null, newDeadline.ToString("yyyy-MM-dd HH:mm"));
            NotifyAssignmentMembers(assignmentId, actorId, "Assignment deadline updated",
                "\"" + assignment.AssignmentName + "\" is now due " + newDeadline.ToString("MMM dd, yyyy HH:mm") + ".",
                "Assignment");
            return null;
        }

        public static void NotifyAllMembers(int assignmentId, string title, string message)
        {
            NotifyAssignmentMembers(assignmentId, 0, title, message, "Assignment");
        }

        public static string CloseAssignment(int assignmentId, int actorId, string actorRole)
        {
            AssignmentRecord assignment = GetAssignment(assignmentId);
            if (assignment == null)
                return "Assignment not found.";
            if (assignment.CreatedBy != actorId || !RoleAccess.CanMonitorAssignments(actorRole))
                return "Only the faculty member who created this assignment can close it.";

            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                "UPDATE Assignments SET Status = N'Closed' WHERE AssignmentID = @ID", con))
            {
                cmd.Parameters.AddWithValue("@ID", assignmentId);
                con.Open();
                cmd.ExecuteNonQuery();
            }

            AuthService.WriteAudit(actorId, "AssignmentClosed", "Assignment", assignmentId, null, null);
            NotifyAssignmentMembers(assignmentId, actorId, "Assignment closed",
                "\"" + assignment.AssignmentName + "\" is closed. Contribution scores are available on the group report.",
                "Assignment");
            return null;
        }

        public static DataTable ListTasksForReport(int groupId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT t.TaskID, t.Title, t.Status, t.DueDate, t.UpdatedAt,
                         ISNULL(u.FullName, 'Unassigned') AS OwnerName
                  FROM AssignmentTasks t
                  LEFT JOIN Users u ON u.UserID = t.AssignedUserID
                  WHERE t.GroupID = @GroupID AND t.IsDeleted = 0
                  ORDER BY t.SortOrder, t.TaskID", con))
            {
                cmd.Parameters.AddWithValue("@GroupID", groupId);
                var table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                return table;
            }
        }

        public static DataTable ListReportFiles(int groupId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT f.FileID, f.FileName, f.UploadedAt, t.Title AS TaskTitle,
                         ISNULL(u.FullName, 'Unknown') AS UploadedByName
                  FROM AssignmentTaskFiles f
                  INNER JOIN AssignmentTasks t ON t.TaskID = f.TaskID AND t.IsDeleted = 0
                  LEFT JOIN Users u ON u.UserID = f.UploadedBy
                  WHERE t.GroupID = @GroupID
                  ORDER BY f.UploadedAt DESC, f.FileID", con))
            {
                cmd.Parameters.AddWithValue("@GroupID", groupId);
                var table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                return table;
            }
        }

        public static DataTable ListReportLinks(int groupId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT l.LinkID, l.Url, l.AddedAt, t.Title AS TaskTitle,
                         ISNULL(u.FullName, 'Unknown') AS AddedByName
                  FROM AssignmentTaskLinks l
                  INNER JOIN AssignmentTasks t ON t.TaskID = l.TaskID AND t.IsDeleted = 0
                  LEFT JOIN Users u ON u.UserID = l.AddedBy
                  WHERE t.GroupID = @GroupID
                  ORDER BY l.AddedAt DESC, l.LinkID", con))
            {
                cmd.Parameters.AddWithValue("@GroupID", groupId);
                var table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                table.Columns.Add("DisplayLabel", typeof(string));
                foreach (DataRow row in table.Rows)
                    row["DisplayLabel"] = LinkDisplayLabel(Convert.ToString(row["Url"]));
                return table;
            }
        }

        public static DataTable ListFacultyContributionGroups(int userId, string systemRole)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT g.GroupID, g.GroupName, g.IsFinalized, a.AssignmentName, a.Deadline, a.Status,
                         u.FullName AS LeaderName
                  FROM AssignmentGroups g
                  INNER JOIN Assignments a ON a.AssignmentID = g.AssignmentID
                  INNER JOIN Users u ON u.UserID = g.LeaderID
                  WHERE a.IsDeleted = 0 AND g.IsDeleted = 0 AND a.CreatedBy = @UserID
                  ORDER BY a.Deadline DESC, g.GroupName", con))
            {
                cmd.Parameters.AddWithValue("@UserID", userId);
                var table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                return table;
            }
        }

        public static string FormatPercent(object value)
        {
            if (value == null || value == DBNull.Value)
                return "0%";
            return Convert.ToDecimal(value).ToString("0") + "%";
        }

        private static bool IsOnAssignment(int assignmentId, int userId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT COUNT(*)
                  FROM AssignmentMembers m
                  INNER JOIN AssignmentGroups g ON g.GroupID = m.GroupID AND g.IsDeleted = 0
                  WHERE g.AssignmentID = @AssignmentID AND m.UserID = @UserID", con))
            {
                cmd.Parameters.AddWithValue("@AssignmentID", assignmentId);
                cmd.Parameters.AddWithValue("@UserID", userId);
                con.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private static bool IsMember(int groupId, int userId)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM AssignmentMembers WHERE GroupID = @GroupID AND UserID = @UserID", con))
            {
                cmd.Parameters.AddWithValue("@GroupID", groupId);
                cmd.Parameters.AddWithValue("@UserID", userId);
                con.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private static bool IsValidStatus(string status)
        {
            return status == "ToDo" || status == "InProgress" || status == "UnderReview"
                || status == "Completed" || status == "Blocked";
        }

        private static void Notify(int userId, string title, string message, int groupId)
        {
            NotificationService.Send(userId, title, message, "Assignment", groupId, "AssignmentGroup");
        }

        private static void NotifyGroupMembers(int groupId, int exceptUserId, string title, string message, string type)
        {
            DataTable members = ListMembers(groupId);
            foreach (DataRow row in members.Rows)
                NotificationService.Send(Convert.ToInt32(row["UserID"]), title, message, type, groupId, "AssignmentGroup");
        }

        private static void NotifyAssignmentMembers(int assignmentId, int exceptUserId, string title, string message, string type)
        {
            using (var con = new SqlConnection(AuthService.ConnectionString))
            using (var cmd = new SqlCommand(
                @"SELECT DISTINCT m.UserID
                  FROM AssignmentMembers m
                  INNER JOIN AssignmentGroups g ON g.GroupID = m.GroupID AND g.IsDeleted = 0
                  WHERE g.AssignmentID = @AssignmentID", con))
            {
                cmd.Parameters.AddWithValue("@AssignmentID", assignmentId);
                var table = new DataTable();
                new SqlDataAdapter(cmd).Fill(table);
                foreach (DataRow row in table.Rows)
                    NotificationService.Send(Convert.ToInt32(row["UserID"]), title, message, type, assignmentId, "Assignment");
            }
        }

        private static AssignmentRecord ReadAssignment(SqlDataReader reader)
        {
            return new AssignmentRecord
            {
                AssignmentID = Convert.ToInt32(reader["AssignmentID"]),
                AssignmentName = Convert.ToString(reader["AssignmentName"]),
                Description = Convert.ToString(reader["Description"]),
                Deadline = Convert.ToDateTime(reader["Deadline"]),
                AssignmentCode = Convert.ToString(reader["AssignmentCode"]),
                Status = Convert.ToString(reader["Status"]),
                CreatedBy = Convert.ToInt32(reader["CreatedBy"]),
                IsDeleted = Convert.ToBoolean(reader["IsDeleted"]),
                AssignmentType = reader["AssignmentType"] == DBNull.Value ? "College" : Convert.ToString(reader["AssignmentType"])
            };
        }
    }
}
