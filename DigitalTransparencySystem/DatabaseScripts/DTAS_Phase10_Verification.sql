-- ============================================
-- DTAS PHASE 10: Read-only verification
-- Confirms procedures, triggers, and key rules.
-- Does not drop objects or invent scores.
-- ============================================

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;

PRINT '--- Stored procedures ---';
SELECT name
FROM sys.procedures
WHERE name IN (
    'sp_VerifyUserIdentity',
    'sp_CreateAssignmentGroup',
    'sp_GetAssignmentProgress',
    'sp_GetMemberContributionReport',
    'sp_GetAdminDashboardStats',
    'sp_UpdateProgressForTask'
)
ORDER BY name;

PRINT '--- Triggers ---';
SELECT name
FROM sys.triggers
WHERE name IN (
    'trg_UserVerificationHistory',
    'trg_TaskAssignmentHistory',
    'trg_TaskStatusHistory',
    'trg_GroupMembershipAudit',
    'trg_AccountStatusAudit'
)
ORDER BY name;

PRINT '--- sp_GetAdminDashboardStats ---';
EXEC dbo.sp_GetAdminDashboardStats;

PRINT '--- Roles (Student=3, Faculty=2, Admin=1, Staff=4) ---';
SELECT RoleID, RoleName FROM dbo.Roles ORDER BY RoleID;

PRINT '--- Identity lock expectation ---';
SELECT
    UnverifiedStudents = (
        SELECT COUNT(*) FROM dbo.Users
        WHERE RoleID = 3 AND ISNULL(IsDeleted, 0) = 0 AND ISNULL(IdentityVerified, 0) = 0
    ),
    VerifiedAdmins = (
        SELECT COUNT(*) FROM dbo.Users
        WHERE RoleID = 1 AND ISNULL(IdentityVerified, 0) = 1
    );

PRINT '--- Contribution formula is stored in sp_GetMemberContributionReport ---';
PRINT '50% completion + 20% timeliness + 20% activity + 10% recency';

PRINT '--- Sample contribution (first group if any) ---';
IF EXISTS (SELECT 1 FROM dbo.AssignmentGroups WHERE IsDeleted = 0)
BEGIN
    DECLARE @GroupID INT = (SELECT TOP 1 GroupID FROM dbo.AssignmentGroups WHERE IsDeleted = 0 ORDER BY GroupID);
    EXEC dbo.sp_GetMemberContributionReport @GroupID = @GroupID, @UserID = NULL;
END
ELSE
    PRINT 'No assignment groups yet.';

PRINT '--- Users.Password remains the hash column ---';
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'Password')
    PRINT 'Users.Password exists.';
ELSE
    PRINT 'ERROR: Users.Password missing.';

PRINT '--- ContentReports is separate from generated Reports ---';
SELECT
    ContentReports = CASE WHEN OBJECT_ID('dbo.ContentReports') IS NULL THEN 'MISSING' ELSE 'OK' END,
    GeneratedReports = CASE WHEN OBJECT_ID('dbo.Reports') IS NULL THEN 'MISSING' ELSE 'OK' END;

PRINT 'Phase 10 verification complete.';
GO
