-- ============================================
-- DTAS PHASE 2: Identity, assignments, moderation
-- Additive only. Safe to re-run (IF NOT EXISTS / DROP+CREATE).
-- Does not drop or rewrite existing governance tables.
-- Keeps Users.Password as the hash column.
-- Reports stays as generated-file reports; user flags go in ContentReports.
-- Academic work uses Assignment* tables; Events/Tasks stay governance.
-- ============================================

SET NOCOUNT ON;
SET XACT_ABORT ON;

-- --------------------------------------------
-- USERS: identity, account status, soft delete
-- --------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'EmailVerified')
    ALTER TABLE dbo.Users ADD EmailVerified BIT NOT NULL CONSTRAINT DF_Users_EmailVerified DEFAULT 0;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'IdentityVerified')
    ALTER TABLE dbo.Users ADD IdentityVerified BIT NOT NULL CONSTRAINT DF_Users_IdentityVerified DEFAULT 0;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'VerificationStatus')
    ALTER TABLE dbo.Users ADD VerificationStatus NVARCHAR(50) NOT NULL CONSTRAINT DF_Users_VerificationStatus DEFAULT N'None';

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'AccountStatus')
    ALTER TABLE dbo.Users ADD AccountStatus NVARCHAR(50) NOT NULL CONSTRAINT DF_Users_AccountStatus DEFAULT N'Active';

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'InstitutionalID')
    ALTER TABLE dbo.Users ADD InstitutionalID NVARCHAR(100) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'VerifiedBy')
    ALTER TABLE dbo.Users ADD VerifiedBy INT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'VerifiedDate')
    ALTER TABLE dbo.Users ADD VerifiedDate DATETIME NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'RejectionReason')
    ALTER TABLE dbo.Users ADD RejectionReason NVARCHAR(MAX) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'IsDeleted')
    ALTER TABLE dbo.Users ADD IsDeleted BIT NOT NULL CONSTRAINT DF_Users_IsDeleted DEFAULT 0;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'DeletedAt')
    ALTER TABLE dbo.Users ADD DeletedAt DATETIME NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Users') AND name = 'DeletedBy')
    ALTER TABLE dbo.Users ADD DeletedBy INT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Users_VerifiedBy')
    ALTER TABLE dbo.Users ADD CONSTRAINT FK_Users_VerifiedBy FOREIGN KEY (VerifiedBy) REFERENCES dbo.Users(UserID);

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Users_DeletedBy')
    ALTER TABLE dbo.Users ADD CONSTRAINT FK_Users_DeletedBy FOREIGN KEY (DeletedBy) REFERENCES dbo.Users(UserID);

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Users_AccountStatus')
    ALTER TABLE dbo.Users ADD CONSTRAINT CK_Users_AccountStatus CHECK (AccountStatus IN (N'Active', N'Suspended', N'Banned'));

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Users_VerificationStatus')
    ALTER TABLE dbo.Users ADD CONSTRAINT CK_Users_VerificationStatus CHECK (VerificationStatus IN (N'None', N'Pending', N'Verified', N'Rejected'));

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_AccountStatus' AND object_id = OBJECT_ID('dbo.Users'))
    CREATE INDEX IX_Users_AccountStatus ON dbo.Users(AccountStatus) WHERE IsDeleted = 0;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_VerificationStatus' AND object_id = OBJECT_ID('dbo.Users'))
    CREATE INDEX IX_Users_VerificationStatus ON dbo.Users(VerificationStatus) WHERE IsDeleted = 0;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_Users_InstitutionalID' AND object_id = OBJECT_ID('dbo.Users'))
    CREATE UNIQUE INDEX UQ_Users_InstitutionalID ON dbo.Users(InstitutionalID) WHERE InstitutionalID IS NOT NULL AND IsDeleted = 0;

-- Existing accounts already sign in. Mark email verified so Phase 3
-- (OTP only on register / email change / recovery) does not lock them out.
-- Admin stays identity-verified so the verification queue remains usable.
UPDATE dbo.Users SET EmailVerified = 1 WHERE EmailVerified = 0 AND IsDeleted = 0;
UPDATE dbo.Users
SET IdentityVerified = 1,
    VerificationStatus = N'Verified',
    VerifiedDate = ISNULL(VerifiedDate, GETDATE())
WHERE RoleID = 1 AND IsDeleted = 0 AND IdentityVerified = 0;

PRINT 'Users columns upgraded.';

-- --------------------------------------------
-- OTPCodes: purpose for register / recovery / email change
-- --------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.OTPCodes') AND name = 'Purpose')
    ALTER TABLE dbo.OTPCodes ADD Purpose NVARCHAR(50) NOT NULL CONSTRAINT DF_OTPCodes_Purpose DEFAULT N'Login';

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.OTPCodes') AND name = 'AttemptCount')
    ALTER TABLE dbo.OTPCodes ADD AttemptCount INT NOT NULL CONSTRAINT DF_OTPCodes_AttemptCount DEFAULT 0;
GO

PRINT 'OTPCodes columns upgraded.';

-- --------------------------------------------
-- Events: visibility, invite code, disable, soft delete
-- --------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Events') AND name = 'InviteCode')
    ALTER TABLE dbo.Events ADD InviteCode NVARCHAR(50) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Events') AND name = 'Visibility')
    ALTER TABLE dbo.Events ADD Visibility NVARCHAR(20) NOT NULL CONSTRAINT DF_Events_Visibility DEFAULT N'Private';

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Events') AND name = 'IsDisabled')
    ALTER TABLE dbo.Events ADD IsDisabled BIT NOT NULL CONSTRAINT DF_Events_IsDisabled DEFAULT 0;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Events') AND name = 'IsDeleted')
    ALTER TABLE dbo.Events ADD IsDeleted BIT NOT NULL CONSTRAINT DF_Events_IsDeleted DEFAULT 0;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Events') AND name = 'DeletedAt')
    ALTER TABLE dbo.Events ADD DeletedAt DATETIME NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Events') AND name = 'DeletedBy')
    ALTER TABLE dbo.Events ADD DeletedBy INT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Events_Visibility')
    ALTER TABLE dbo.Events ADD CONSTRAINT CK_Events_Visibility CHECK (Visibility IN (N'Public', N'Private'));

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_Events_InviteCode' AND object_id = OBJECT_ID('dbo.Events'))
    CREATE UNIQUE INDEX UQ_Events_InviteCode ON dbo.Events(InviteCode) WHERE InviteCode IS NOT NULL AND IsDeleted = 0;

PRINT 'Events columns upgraded.';

-- --------------------------------------------
-- Tasks (governance): soft delete + flag
-- --------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Tasks') AND name = 'IsDeleted')
    ALTER TABLE dbo.Tasks ADD IsDeleted BIT NOT NULL CONSTRAINT DF_Tasks_IsDeleted DEFAULT 0;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Tasks') AND name = 'DeletedAt')
    ALTER TABLE dbo.Tasks ADD DeletedAt DATETIME NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Tasks') AND name = 'DeletedBy')
    ALTER TABLE dbo.Tasks ADD DeletedBy INT NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Tasks') AND name = 'FlagStatus')
    ALTER TABLE dbo.Tasks ADD FlagStatus NVARCHAR(50) NULL;
GO

PRINT 'Tasks columns upgraded.';

-- --------------------------------------------
-- Clubs: public flag, image, invite, soft delete
-- --------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Clubs') AND name = 'IsPublic')
    ALTER TABLE dbo.Clubs ADD IsPublic BIT NOT NULL CONSTRAINT DF_Clubs_IsPublic DEFAULT 0;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Clubs') AND name = 'ImagePath')
    ALTER TABLE dbo.Clubs ADD ImagePath NVARCHAR(500) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Clubs') AND name = 'InviteCode')
    ALTER TABLE dbo.Clubs ADD InviteCode NVARCHAR(50) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Clubs') AND name = 'IsDeleted')
    ALTER TABLE dbo.Clubs ADD IsDeleted BIT NOT NULL CONSTRAINT DF_Clubs_IsDeleted DEFAULT 0;
GO

PRINT 'Clubs columns upgraded.';

-- --------------------------------------------
-- EventMembers leftover table: reuse for Event Admin / Manager / Participant
-- --------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.EventMembers') AND name = 'InviteStatus')
    ALTER TABLE dbo.EventMembers ADD InviteStatus NVARCHAR(30) NOT NULL CONSTRAINT DF_EventMembers_InviteStatus DEFAULT N'Accepted';

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.EventMembers') AND name = 'IsActive')
    ALTER TABLE dbo.EventMembers ADD IsActive BIT NOT NULL CONSTRAINT DF_EventMembers_IsActive DEFAULT 1;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_EventMembers_EventUser' AND object_id = OBJECT_ID('dbo.EventMembers'))
    CREATE UNIQUE INDEX UQ_EventMembers_EventUser ON dbo.EventMembers(EventID, UserID);

PRINT 'EventMembers columns upgraded.';

-- --------------------------------------------
-- EmailVerification (register / email change / recovery)
-- OTPCodes remains for any leftover login OTP rows.
-- --------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EmailVerification')
BEGIN
    CREATE TABLE dbo.EmailVerification (
        VerificationID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserID INT NOT NULL,
        Email NVARCHAR(200) NOT NULL,
        OTP NVARCHAR(10) NOT NULL,
        Expiry DATETIME NOT NULL,
        IsUsed BIT NOT NULL CONSTRAINT DF_EmailVerification_IsUsed DEFAULT 0,
        AttemptCount INT NOT NULL CONSTRAINT DF_EmailVerification_AttemptCount DEFAULT 0,
        Purpose NVARCHAR(50) NOT NULL CONSTRAINT DF_EmailVerification_Purpose DEFAULT N'Register',
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_EmailVerification_CreatedAt DEFAULT GETDATE(),
        CONSTRAINT FK_EmailVerification_User FOREIGN KEY (UserID) REFERENCES dbo.Users(UserID),
        CONSTRAINT CK_EmailVerification_Purpose CHECK (Purpose IN (N'Register', N'Recovery', N'EmailChange'))
    );
    CREATE INDEX IX_EmailVerification_UserExpiry ON dbo.EmailVerification(UserID, IsUsed, Expiry);
    PRINT 'EmailVerification table created.';
END
ELSE
    PRINT 'EmailVerification table already exists.';

-- --------------------------------------------
-- Identity documents + history
-- --------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'IdentityDocuments')
BEGIN
    CREATE TABLE dbo.IdentityDocuments (
        DocumentID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserID INT NOT NULL,
        FileName NVARCHAR(300) NOT NULL,
        OriginalFileName NVARCHAR(300) NOT NULL,
        FilePath NVARCHAR(500) NOT NULL,
        FileType NVARCHAR(50) NOT NULL,
        FileSize INT NOT NULL,
        UploadDate DATETIME NOT NULL CONSTRAINT DF_IdentityDocuments_UploadDate DEFAULT GETDATE(),
        VerificationStatus NVARCHAR(50) NOT NULL CONSTRAINT DF_IdentityDocuments_Status DEFAULT N'Pending',
        IsCurrent BIT NOT NULL CONSTRAINT DF_IdentityDocuments_IsCurrent DEFAULT 1,
        RejectionReason NVARCHAR(MAX) NULL,
        CONSTRAINT FK_IdentityDocuments_User FOREIGN KEY (UserID) REFERENCES dbo.Users(UserID),
        CONSTRAINT CK_IdentityDocuments_Status CHECK (VerificationStatus IN (N'Pending', N'Verified', N'Rejected')),
        CONSTRAINT CK_IdentityDocuments_FileType CHECK (FileType IN (N'jpg', N'jpeg', N'png', N'pdf'))
    );
    CREATE INDEX IX_IdentityDocuments_UserCurrent ON dbo.IdentityDocuments(UserID, IsCurrent);
    PRINT 'IdentityDocuments table created.';
END
ELSE
    PRINT 'IdentityDocuments table already exists.';

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'IdentityVerificationHistory')
BEGIN
    CREATE TABLE dbo.IdentityVerificationHistory (
        HistoryID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserID INT NOT NULL,
        DocumentID INT NULL,
        OldStatus NVARCHAR(50) NULL,
        NewStatus NVARCHAR(50) NOT NULL,
        Decision NVARCHAR(50) NOT NULL,
        RejectionReason NVARCHAR(MAX) NULL,
        AdminID INT NULL,
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_IdentityVerificationHistory_CreatedAt DEFAULT GETDATE(),
        CONSTRAINT FK_IVH_User FOREIGN KEY (UserID) REFERENCES dbo.Users(UserID),
        CONSTRAINT FK_IVH_Document FOREIGN KEY (DocumentID) REFERENCES dbo.IdentityDocuments(DocumentID),
        CONSTRAINT FK_IVH_Admin FOREIGN KEY (AdminID) REFERENCES dbo.Users(UserID)
    );
    CREATE INDEX IX_IVH_User ON dbo.IdentityVerificationHistory(UserID, CreatedAt DESC);
    PRINT 'IdentityVerificationHistory table created.';
END
ELSE
    PRINT 'IdentityVerificationHistory table already exists.';

-- --------------------------------------------
-- AuditLogs (new). ActivityLog leftover is left untouched.
-- --------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AuditLogs')
BEGIN
    CREATE TABLE dbo.AuditLogs (
        LogID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserID INT NULL,
        Action NVARCHAR(100) NOT NULL,
        EntityType NVARCHAR(50) NOT NULL,
        EntityID INT NULL,
        Description NVARCHAR(MAX) NULL,
        Timestamp DATETIME NOT NULL CONSTRAINT DF_AuditLogs_Timestamp DEFAULT GETDATE(),
        IPAddress NVARCHAR(100) NULL,
        CONSTRAINT FK_AuditLogs_User FOREIGN KEY (UserID) REFERENCES dbo.Users(UserID)
    );
    CREATE INDEX IX_AuditLogs_Entity ON dbo.AuditLogs(EntityType, EntityID, Timestamp DESC);
    CREATE INDEX IX_AuditLogs_User ON dbo.AuditLogs(UserID, Timestamp DESC);
    PRINT 'AuditLogs table created.';
END
ELSE
    PRINT 'AuditLogs table already exists.';

-- --------------------------------------------
-- Moderation: ContentReports, Suspensions, Bans
-- --------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ContentReports')
BEGIN
    CREATE TABLE dbo.ContentReports (
        ReportID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ReporterID INT NOT NULL,
        TargetType NVARCHAR(50) NOT NULL,
        TargetID INT NOT NULL,
        Reason NVARCHAR(200) NOT NULL,
        Description NVARCHAR(MAX) NULL,
        Status NVARCHAR(50) NOT NULL CONSTRAINT DF_ContentReports_Status DEFAULT N'Pending',
        AssignedAdminID INT NULL,
        Resolution NVARCHAR(MAX) NULL,
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_ContentReports_CreatedAt DEFAULT GETDATE(),
        ResolvedAt DATETIME NULL,
        CONSTRAINT FK_ContentReports_Reporter FOREIGN KEY (ReporterID) REFERENCES dbo.Users(UserID),
        CONSTRAINT FK_ContentReports_Admin FOREIGN KEY (AssignedAdminID) REFERENCES dbo.Users(UserID),
        CONSTRAINT CK_ContentReports_TargetType CHECK (TargetType IN (N'User', N'Group', N'Task', N'Event', N'Project', N'Assignment', N'Club', N'Message')),
        CONSTRAINT CK_ContentReports_Status CHECK (Status IN (N'Pending', N'Reviewing', N'Resolved', N'Rejected', N'Escalated'))
    );
    CREATE INDEX IX_ContentReports_Status ON dbo.ContentReports(Status, CreatedAt DESC);
    PRINT 'ContentReports table created.';
END
ELSE
    PRINT 'ContentReports table already exists.';

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Suspensions')
BEGIN
    CREATE TABLE dbo.Suspensions (
        SuspensionID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserID INT NOT NULL,
        Reason NVARCHAR(MAX) NOT NULL,
        StartDate DATETIME NOT NULL CONSTRAINT DF_Suspensions_StartDate DEFAULT GETDATE(),
        EndDate DATETIME NOT NULL,
        CreatedBy INT NOT NULL,
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_Suspensions_CreatedAt DEFAULT GETDATE(),
        IsActive BIT NOT NULL CONSTRAINT DF_Suspensions_IsActive DEFAULT 1,
        CONSTRAINT FK_Suspensions_User FOREIGN KEY (UserID) REFERENCES dbo.Users(UserID),
        CONSTRAINT FK_Suspensions_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES dbo.Users(UserID)
    );
    CREATE INDEX IX_Suspensions_UserActive ON dbo.Suspensions(UserID, IsActive, EndDate);
    PRINT 'Suspensions table created.';
END
ELSE
    PRINT 'Suspensions table already exists.';

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Bans')
BEGIN
    CREATE TABLE dbo.Bans (
        BanID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserID INT NULL,
        Email NVARCHAR(200) NOT NULL,
        InstitutionalID NVARCHAR(100) NULL,
        Reason NVARCHAR(MAX) NOT NULL,
        BannedBy INT NOT NULL,
        BannedAt DATETIME NOT NULL CONSTRAINT DF_Bans_BannedAt DEFAULT GETDATE(),
        IsActive BIT NOT NULL CONSTRAINT DF_Bans_IsActive DEFAULT 1,
        CONSTRAINT FK_Bans_User FOREIGN KEY (UserID) REFERENCES dbo.Users(UserID),
        CONSTRAINT FK_Bans_BannedBy FOREIGN KEY (BannedBy) REFERENCES dbo.Users(UserID)
    );
    CREATE INDEX IX_Bans_Email ON dbo.Bans(Email, IsActive);
    CREATE INDEX IX_Bans_InstitutionalID ON dbo.Bans(InstitutionalID, IsActive);
    PRINT 'Bans table created.';
END
ELSE
    PRINT 'Bans table already exists.';

-- --------------------------------------------
-- Event invitations
-- --------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EventInvitations')
BEGIN
    CREATE TABLE dbo.EventInvitations (
        InvitationID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        EventID INT NOT NULL,
        Email NVARCHAR(200) NOT NULL,
        InvitedUserID INT NULL,
        InviteCode NVARCHAR(50) NULL,
        Status NVARCHAR(50) NOT NULL CONSTRAINT DF_EventInvitations_Status DEFAULT N'Pending',
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_EventInvitations_CreatedAt DEFAULT GETDATE(),
        CreatedBy INT NOT NULL,
        CONSTRAINT FK_EventInvitations_Event FOREIGN KEY (EventID) REFERENCES dbo.Events(EventID),
        CONSTRAINT FK_EventInvitations_User FOREIGN KEY (InvitedUserID) REFERENCES dbo.Users(UserID),
        CONSTRAINT FK_EventInvitations_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES dbo.Users(UserID),
        CONSTRAINT CK_EventInvitations_Status CHECK (Status IN (N'Pending', N'Accepted', N'Declined', N'Expired')),
        CONSTRAINT UQ_EventInvitations_EventEmail UNIQUE (EventID, Email)
    );
    PRINT 'EventInvitations table created.';
END
ELSE
    PRINT 'EventInvitations table already exists.';

-- --------------------------------------------
-- Academic assignments (separate from governance Tasks)
-- --------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Assignments')
BEGIN
    CREATE TABLE dbo.Assignments (
        AssignmentID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        AssignmentName NVARCHAR(200) NOT NULL,
        Description NVARCHAR(MAX) NULL,
        Deadline DATETIME NOT NULL,
        AssignmentCode NVARCHAR(50) NOT NULL,
        CreatedBy INT NOT NULL,
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_Assignments_CreatedAt DEFAULT GETDATE(),
        Status NVARCHAR(50) NOT NULL CONSTRAINT DF_Assignments_Status DEFAULT N'Active',
        IsDeleted BIT NOT NULL CONSTRAINT DF_Assignments_IsDeleted DEFAULT 0,
        CONSTRAINT FK_Assignments_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES dbo.Users(UserID),
        CONSTRAINT UQ_Assignments_Code UNIQUE (AssignmentCode),
        CONSTRAINT CK_Assignments_Status CHECK (Status IN (N'Active', N'Closed', N'Archived'))
    );
    PRINT 'Assignments table created.';
END
ELSE
    PRINT 'Assignments table already exists.';

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AssignmentGroups')
BEGIN
    CREATE TABLE dbo.AssignmentGroups (
        GroupID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        AssignmentID INT NOT NULL,
        GroupName NVARCHAR(200) NOT NULL,
        LeaderID INT NOT NULL,
        IsFinalized BIT NOT NULL CONSTRAINT DF_AssignmentGroups_IsFinalized DEFAULT 0,
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_AssignmentGroups_CreatedAt DEFAULT GETDATE(),
        IsDeleted BIT NOT NULL CONSTRAINT DF_AssignmentGroups_IsDeleted DEFAULT 0,
        CONSTRAINT FK_AssignmentGroups_Assignment FOREIGN KEY (AssignmentID) REFERENCES dbo.Assignments(AssignmentID),
        CONSTRAINT FK_AssignmentGroups_Leader FOREIGN KEY (LeaderID) REFERENCES dbo.Users(UserID)
    );
    CREATE INDEX IX_AssignmentGroups_Assignment ON dbo.AssignmentGroups(AssignmentID) WHERE IsDeleted = 0;
    PRINT 'AssignmentGroups table created.';
END
ELSE
    PRINT 'AssignmentGroups table already exists.';

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AssignmentMembers')
BEGIN
    CREATE TABLE dbo.AssignmentMembers (
        MemberID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        GroupID INT NOT NULL,
        UserID INT NOT NULL,
        Responsibility NVARCHAR(50) NOT NULL CONSTRAINT DF_AssignmentMembers_Responsibility DEFAULT N'Member',
        JoinedAt DATETIME NOT NULL CONSTRAINT DF_AssignmentMembers_JoinedAt DEFAULT GETDATE(),
        CONSTRAINT FK_AssignmentMembers_Group FOREIGN KEY (GroupID) REFERENCES dbo.AssignmentGroups(GroupID),
        CONSTRAINT FK_AssignmentMembers_User FOREIGN KEY (UserID) REFERENCES dbo.Users(UserID),
        CONSTRAINT UQ_AssignmentMembers_GroupUser UNIQUE (GroupID, UserID),
        CONSTRAINT CK_AssignmentMembers_Responsibility CHECK (Responsibility IN (N'Leader', N'Member'))
    );
    PRINT 'AssignmentMembers table created.';
END
ELSE
    PRINT 'AssignmentMembers table already exists.';

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AssignmentTasks')
BEGIN
    CREATE TABLE dbo.AssignmentTasks (
        TaskID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        GroupID INT NOT NULL,
        Title NVARCHAR(200) NOT NULL,
        Description NVARCHAR(MAX) NULL,
        AssignedUserID INT NULL,
        SortOrder INT NOT NULL CONSTRAINT DF_AssignmentTasks_SortOrder DEFAULT 0,
        Status NVARCHAR(50) NOT NULL CONSTRAINT DF_AssignmentTasks_Status DEFAULT N'ToDo',
        DueDate DATETIME NULL,
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_AssignmentTasks_CreatedAt DEFAULT GETDATE(),
        UpdatedAt DATETIME NOT NULL CONSTRAINT DF_AssignmentTasks_UpdatedAt DEFAULT GETDATE(),
        ParentTaskID INT NULL,
        LastChangedBy INT NULL,
        IsDeleted BIT NOT NULL CONSTRAINT DF_AssignmentTasks_IsDeleted DEFAULT 0,
        CONSTRAINT FK_AssignmentTasks_Group FOREIGN KEY (GroupID) REFERENCES dbo.AssignmentGroups(GroupID),
        CONSTRAINT FK_AssignmentTasks_Assignee FOREIGN KEY (AssignedUserID) REFERENCES dbo.Users(UserID),
        CONSTRAINT FK_AssignmentTasks_Parent FOREIGN KEY (ParentTaskID) REFERENCES dbo.AssignmentTasks(TaskID),
        CONSTRAINT FK_AssignmentTasks_ChangedBy FOREIGN KEY (LastChangedBy) REFERENCES dbo.Users(UserID),
        CONSTRAINT CK_AssignmentTasks_Status CHECK (Status IN (N'ToDo', N'InProgress', N'UnderReview', N'Completed', N'Blocked'))
    );
    CREATE INDEX IX_AssignmentTasks_GroupStatus ON dbo.AssignmentTasks(GroupID, Status) WHERE IsDeleted = 0;
    PRINT 'AssignmentTasks table created.';
END
ELSE
    PRINT 'AssignmentTasks table already exists.';

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AssignmentTaskHistory')
BEGIN
    CREATE TABLE dbo.AssignmentTaskHistory (
        HistoryID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        TaskID INT NOT NULL,
        OldStatus NVARCHAR(50) NULL,
        NewStatus NVARCHAR(50) NULL,
        PreviousAssignee INT NULL,
        NewAssignee INT NULL,
        ChangedBy INT NULL,
        ChangedAt DATETIME NOT NULL CONSTRAINT DF_AssignmentTaskHistory_ChangedAt DEFAULT GETDATE(),
        Description NVARCHAR(MAX) NULL,
        CONSTRAINT FK_ATH_Task FOREIGN KEY (TaskID) REFERENCES dbo.AssignmentTasks(TaskID),
        CONSTRAINT FK_ATH_ChangedBy FOREIGN KEY (ChangedBy) REFERENCES dbo.Users(UserID)
    );
    CREATE INDEX IX_ATH_Task ON dbo.AssignmentTaskHistory(TaskID, ChangedAt DESC);
    PRINT 'AssignmentTaskHistory table created.';
END
ELSE
    PRINT 'AssignmentTaskHistory table already exists.';

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ContributionRecords')
BEGIN
    CREATE TABLE dbo.ContributionRecords (
        RecordID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        GroupID INT NOT NULL,
        UserID INT NOT NULL,
        AssignedTasks INT NOT NULL CONSTRAINT DF_CR_Assigned DEFAULT 0,
        CompletedTasks INT NOT NULL CONSTRAINT DF_CR_Completed DEFAULT 0,
        PendingTasks INT NOT NULL CONSTRAINT DF_CR_Pending DEFAULT 0,
        OverdueTasks INT NOT NULL CONSTRAINT DF_CR_Overdue DEFAULT 0,
        ProgressUpdates INT NOT NULL CONSTRAINT DF_CR_Updates DEFAULT 0,
        LastActivity DATETIME NULL,
        CompletionPercentage DECIMAL(5,2) NOT NULL CONSTRAINT DF_CR_Completion DEFAULT 0,
        ContributionScore DECIMAL(5,2) NOT NULL CONSTRAINT DF_CR_Score DEFAULT 0,
        CalculatedAt DATETIME NOT NULL CONSTRAINT DF_CR_CalculatedAt DEFAULT GETDATE(),
        CONSTRAINT FK_CR_Group FOREIGN KEY (GroupID) REFERENCES dbo.AssignmentGroups(GroupID),
        CONSTRAINT FK_CR_User FOREIGN KEY (UserID) REFERENCES dbo.Users(UserID),
        CONSTRAINT UQ_CR_GroupUser UNIQUE (GroupID, UserID)
    );
    PRINT 'ContributionRecords table created.';
END
ELSE
    PRINT 'ContributionRecords table already exists.';

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AssignmentInvitations')
BEGIN
    CREATE TABLE dbo.AssignmentInvitations (
        InvitationID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        AssignmentID INT NOT NULL,
        GroupID INT NULL,
        Email NVARCHAR(200) NOT NULL,
        InvitedUserID INT NULL,
        Status NVARCHAR(50) NOT NULL CONSTRAINT DF_AssignmentInvitations_Status DEFAULT N'Pending',
        CreatedBy INT NOT NULL,
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_AssignmentInvitations_CreatedAt DEFAULT GETDATE(),
        CONSTRAINT FK_AI_Assignment FOREIGN KEY (AssignmentID) REFERENCES dbo.Assignments(AssignmentID),
        CONSTRAINT FK_AI_Group FOREIGN KEY (GroupID) REFERENCES dbo.AssignmentGroups(GroupID),
        CONSTRAINT FK_AI_User FOREIGN KEY (InvitedUserID) REFERENCES dbo.Users(UserID),
        CONSTRAINT FK_AI_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES dbo.Users(UserID),
        CONSTRAINT CK_AI_Status CHECK (Status IN (N'Pending', N'Accepted', N'Declined', N'Expired'))
    );
    PRINT 'AssignmentInvitations table created.';
END
ELSE
    PRINT 'AssignmentInvitations table already exists.';

-- --------------------------------------------
-- Club messaging
-- --------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'GroupMessages')
BEGIN
    CREATE TABLE dbo.GroupMessages (
        MessageID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ClubID INT NOT NULL,
        SenderID INT NOT NULL,
        MessageContent NVARCHAR(MAX) NOT NULL,
        MessageType NVARCHAR(50) NOT NULL CONSTRAINT DF_GroupMessages_Type DEFAULT N'Text',
        AttachmentPath NVARCHAR(500) NULL,
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_GroupMessages_CreatedAt DEFAULT GETDATE(),
        EditedAt DATETIME NULL,
        IsDeleted BIT NOT NULL CONSTRAINT DF_GroupMessages_IsDeleted DEFAULT 0,
        CONSTRAINT FK_GroupMessages_Club FOREIGN KEY (ClubID) REFERENCES dbo.Clubs(ClubID),
        CONSTRAINT FK_GroupMessages_Sender FOREIGN KEY (SenderID) REFERENCES dbo.Users(UserID),
        CONSTRAINT CK_GroupMessages_Type CHECK (MessageType IN (N'Text', N'Image', N'Announcement', N'Urgent'))
    );
    CREATE INDEX IX_GroupMessages_Club ON dbo.GroupMessages(ClubID, CreatedAt DESC) WHERE IsDeleted = 0;
    PRINT 'GroupMessages table created.';
END
ELSE
    PRINT 'GroupMessages table already exists.';

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'GroupMessageAttachments')
BEGIN
    CREATE TABLE dbo.GroupMessageAttachments (
        AttachmentID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        MessageID INT NOT NULL,
        FileName NVARCHAR(300) NOT NULL,
        FilePath NVARCHAR(500) NOT NULL,
        FileType NVARCHAR(50) NOT NULL,
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_GMA_CreatedAt DEFAULT GETDATE(),
        CONSTRAINT FK_GMA_Message FOREIGN KEY (MessageID) REFERENCES dbo.GroupMessages(MessageID)
    );
    PRINT 'GroupMessageAttachments table created.';
END
ELSE
    PRINT 'GroupMessageAttachments table already exists.';

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'GroupAnnouncements')
BEGIN
    CREATE TABLE dbo.GroupAnnouncements (
        AnnouncementID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ClubID INT NOT NULL,
        SenderID INT NOT NULL,
        Title NVARCHAR(200) NOT NULL,
        Message NVARCHAR(MAX) NOT NULL,
        IsUrgent BIT NOT NULL CONSTRAINT DF_GroupAnnouncements_IsUrgent DEFAULT 0,
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_GroupAnnouncements_CreatedAt DEFAULT GETDATE(),
        CONSTRAINT FK_GroupAnnouncements_Club FOREIGN KEY (ClubID) REFERENCES dbo.Clubs(ClubID),
        CONSTRAINT FK_GroupAnnouncements_Sender FOREIGN KEY (SenderID) REFERENCES dbo.Users(UserID)
    );
    PRINT 'GroupAnnouncements table created.';
END
ELSE
    PRINT 'GroupAnnouncements table already exists.';

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'GroupInvitations')
BEGIN
    CREATE TABLE dbo.GroupInvitations (
        InvitationID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ClubID INT NOT NULL,
        Email NVARCHAR(200) NOT NULL,
        InvitedUserID INT NULL,
        Status NVARCHAR(50) NOT NULL CONSTRAINT DF_GroupInvitations_Status DEFAULT N'Pending',
        CreatedBy INT NOT NULL,
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_GroupInvitations_CreatedAt DEFAULT GETDATE(),
        CONSTRAINT FK_GroupInvitations_Club FOREIGN KEY (ClubID) REFERENCES dbo.Clubs(ClubID),
        CONSTRAINT FK_GroupInvitations_User FOREIGN KEY (InvitedUserID) REFERENCES dbo.Users(UserID),
        CONSTRAINT FK_GroupInvitations_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES dbo.Users(UserID),
        CONSTRAINT CK_GroupInvitations_Status CHECK (Status IN (N'Pending', N'Accepted', N'Declined', N'Expired')),
        CONSTRAINT UQ_GroupInvitations_ClubEmail UNIQUE (ClubID, Email)
    );
    PRINT 'GroupInvitations table created.';
END
ELSE
    PRINT 'GroupInvitations table already exists.';

PRINT 'Phase 2 tables and columns applied.';
GO

-- ============================================
-- STORED PROCEDURES
-- ============================================

IF OBJECT_ID('dbo.sp_VerifyUserIdentity', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_VerifyUserIdentity;
GO

CREATE PROCEDURE dbo.sp_VerifyUserIdentity
    @UserID INT,
    @AdminID INT,
    @Decision NVARCHAR(20),
    @RejectionReason NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @Decision NOT IN (N'Approved', N'Rejected')
    BEGIN
        RAISERROR(N'Decision must be Approved or Rejected.', 16, 1);
        RETURN;
    END

    IF @Decision = N'Rejected' AND (NULLIF(LTRIM(RTRIM(@RejectionReason)), N'') IS NULL)
    BEGIN
        RAISERROR(N'Rejection reason is required.', 16, 1);
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE UserID = @UserID AND IsDeleted = 0)
    BEGIN
        RAISERROR(N'Target user was not found.', 16, 1);
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE UserID = @AdminID AND IsDeleted = 0 AND RoleID = 1)
    BEGIN
        RAISERROR(N'Only an active Admin can verify identity.', 16, 1);
        RETURN;
    END

    DECLARE @DocumentID INT;
    SELECT TOP 1 @DocumentID = DocumentID
    FROM dbo.IdentityDocuments
    WHERE UserID = @UserID AND IsCurrent = 1
    ORDER BY DocumentID DESC;

    IF @DocumentID IS NULL
    BEGIN
        RAISERROR(N'No current identity document is on file for this user.', 16, 1);
        RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        IF @Decision = N'Approved'
        BEGIN
            UPDATE dbo.Users
            SET IdentityVerified = 1,
                VerificationStatus = N'Verified',
                VerifiedBy = @AdminID,
                VerifiedDate = GETDATE(),
                RejectionReason = NULL
            WHERE UserID = @UserID;

            UPDATE dbo.IdentityDocuments
            SET VerificationStatus = N'Verified',
                RejectionReason = NULL
            WHERE DocumentID = @DocumentID;

            INSERT INTO dbo.Notifications (UserID, Title, Message, IsRead, NotificationType, RelatedID, RelatedType, CreatedAt)
            VALUES (@UserID, N'Identity verified', N'Your institutional ID has been approved. Core features are now unlocked.', 0, N'Identity', @DocumentID, N'IdentityDocument', GETDATE());
        END
        ELSE
        BEGIN
            UPDATE dbo.Users
            SET IdentityVerified = 0,
                VerificationStatus = N'Rejected',
                VerifiedBy = @AdminID,
                VerifiedDate = GETDATE(),
                RejectionReason = @RejectionReason
            WHERE UserID = @UserID;

            UPDATE dbo.IdentityDocuments
            SET VerificationStatus = N'Rejected',
                RejectionReason = @RejectionReason
            WHERE DocumentID = @DocumentID;

            INSERT INTO dbo.Notifications (UserID, Title, Message, IsRead, NotificationType, RelatedID, RelatedType, CreatedAt)
            VALUES (@UserID, N'Identity verification rejected', @RejectionReason, 0, N'Identity', @DocumentID, N'IdentityDocument', GETDATE());
        END

        INSERT INTO dbo.AuditLogs (UserID, Action, EntityType, EntityID, Description, Timestamp)
        VALUES (
            @AdminID,
            CASE WHEN @Decision = N'Approved' THEN N'IdentityApproved' ELSE N'IdentityRejected' END,
            N'User',
            @UserID,
            CASE WHEN @Decision = N'Approved' THEN N'Admin approved identity verification.' ELSE @RejectionReason END,
            GETDATE()
        );

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        DECLARE @Err NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@Err, 16, 1);
    END CATCH
END
GO

IF OBJECT_ID('dbo.sp_CreateAssignmentGroup', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_CreateAssignmentGroup;
GO

-- Creates the parent academic assignment (name kept from the master spec).
CREATE PROCEDURE dbo.sp_CreateAssignmentGroup
    @AssignmentName NVARCHAR(200),
    @Description NVARCHAR(MAX),
    @Deadline DATETIME,
    @CreatedBy INT,
    @AssignmentID INT OUTPUT,
    @AssignmentCode NVARCHAR(50) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF NULLIF(LTRIM(RTRIM(@AssignmentName)), N'') IS NULL
    BEGIN
        RAISERROR(N'Assignment name is required.', 16, 1);
        RETURN;
    END

    IF @Deadline IS NULL
    BEGIN
        RAISERROR(N'Deadline is required.', 16, 1);
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE UserID = @CreatedBy AND IsDeleted = 0 AND RoleID IN (1, 2))
    BEGIN
        RAISERROR(N'Only Faculty or Admin can create an assignment.', 16, 1);
        RETURN;
    END

    DECLARE @Tries INT = 0;
    SET @AssignmentCode = NULL;

    WHILE @Tries < 25 AND @AssignmentCode IS NULL
    BEGIN
        SET @AssignmentCode = N'SNA-ASSIGN-' + RIGHT(N'0000' + CAST(ABS(CHECKSUM(NEWID())) % 10000 AS NVARCHAR(4)), 4);
        IF EXISTS (SELECT 1 FROM dbo.Assignments WHERE AssignmentCode = @AssignmentCode)
            SET @AssignmentCode = NULL;
        SET @Tries += 1;
    END

    IF @AssignmentCode IS NULL
    BEGIN
        RAISERROR(N'Could not generate a unique assignment code.', 16, 1);
        RETURN;
    END

    INSERT INTO dbo.Assignments (AssignmentName, Description, Deadline, AssignmentCode, CreatedBy, CreatedAt, Status, IsDeleted)
    VALUES (@AssignmentName, @Description, @Deadline, @AssignmentCode, @CreatedBy, GETDATE(), N'Active', 0);

    SET @AssignmentID = SCOPE_IDENTITY();

    INSERT INTO dbo.AuditLogs (UserID, Action, EntityType, EntityID, Description, Timestamp)
    VALUES (@CreatedBy, N'AssignmentCreated', N'Assignment', @AssignmentID, @AssignmentName + N' (' + @AssignmentCode + N')', GETDATE());
END
GO

IF OBJECT_ID('dbo.sp_GetAssignmentProgress', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetAssignmentProgress;
GO

CREATE PROCEDURE dbo.sp_GetAssignmentProgress
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
            SUM(CASE WHEN t.Status = N'Completed' THEN 1 ELSE 0 END) * 100.0
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
            SUM(CASE WHEN t.Status = N'Completed' THEN 1 ELSE 0 END) * 100.0
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
END
GO

IF OBJECT_ID('dbo.sp_GetMemberContributionReport', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetMemberContributionReport;
GO

/*
ContributionScore (0-100), documented so C# does not invent percentages:

  Completion  50%  = CompletedTasks / AssignedTasks
  Timeliness  20%  = 1 - (OverdueTasks / AssignedTasks)
  Activity    20%  = min(100, ProgressUpdates * 20) / 100
  Recency     10%  = 100 if last activity within 14 days, else 0

Users with zero assigned tasks score 0. ProgressUpdates counts
AssignmentTaskHistory rows this member authored for the group.
*/
CREATE PROCEDURE dbo.sp_GetMemberContributionReport
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
                     ELSE ts.CompletedTasks * 100.0 / ts.AssignedTasks
                END AS DECIMAL(5,2)
            ),
            ContributionScore = CAST(
                CASE WHEN ts.AssignedTasks = 0 THEN 0
                     ELSE
                        (50.0 * ts.CompletedTasks / ts.AssignedTasks)
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
END
GO

IF OBJECT_ID('dbo.sp_GetAdminDashboardStats', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetAdminDashboardStats;
GO

CREATE PROCEDURE dbo.sp_GetAdminDashboardStats
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        TotalUsers = (SELECT COUNT(*) FROM dbo.Users WHERE IsDeleted = 0),
        Students = (SELECT COUNT(*) FROM dbo.Users WHERE IsDeleted = 0 AND RoleID = 3),
        Faculty = (SELECT COUNT(*) FROM dbo.Users WHERE IsDeleted = 0 AND RoleID = 2),
        Staff = (SELECT COUNT(*) FROM dbo.Users WHERE IsDeleted = 0 AND RoleID = 4),
        PendingVerification = (SELECT COUNT(*) FROM dbo.Users WHERE IsDeleted = 0 AND VerificationStatus = N'Pending'),
        VerifiedUsers = (SELECT COUNT(*) FROM dbo.Users WHERE IsDeleted = 0 AND IdentityVerified = 1),
        RejectedUsers = (SELECT COUNT(*) FROM dbo.Users WHERE IsDeleted = 0 AND VerificationStatus = N'Rejected'),
        SuspendedUsers = (SELECT COUNT(*) FROM dbo.Users WHERE IsDeleted = 0 AND AccountStatus = N'Suspended'),
        BannedUsers = (SELECT COUNT(*) FROM dbo.Users WHERE IsDeleted = 0 AND AccountStatus = N'Banned'),
        ActiveEvents = (
            SELECT COUNT(*) FROM dbo.Events
            WHERE IsDeleted = 0 AND IsDisabled = 0 AND Status NOT IN (N'Cancelled')
        ),
        ActiveAssignments = (SELECT COUNT(*) FROM dbo.Assignments WHERE IsDeleted = 0 AND Status = N'Active'),
        ActiveGroups = (SELECT COUNT(*) FROM dbo.Clubs WHERE IsActive = 1 AND IsDeleted = 0),
        PendingReports = (SELECT COUNT(*) FROM dbo.ContentReports WHERE Status = N'Pending'),
        FlaggedTasks = (
            SELECT COUNT(*) FROM dbo.Tasks
            WHERE ISNULL(IsDeleted, 0) = 0
              AND FlagStatus IS NOT NULL
              AND FlagStatus <> N'None'
        );
END
GO

-- ============================================
-- TRIGGERS (set-based, no cursors)
-- Identity history is written here, not also in the SP,
-- so approve/reject does not create duplicate rows.
-- ============================================

IF OBJECT_ID('dbo.trg_UserVerificationHistory', 'TR') IS NOT NULL
    DROP TRIGGER dbo.trg_UserVerificationHistory;
GO

CREATE TRIGGER dbo.trg_UserVerificationHistory
ON dbo.Users
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT UPDATE(VerificationStatus) AND NOT UPDATE(IdentityVerified)
        RETURN;

    INSERT INTO dbo.IdentityVerificationHistory (
        UserID, DocumentID, OldStatus, NewStatus, Decision, RejectionReason, AdminID, CreatedAt
    )
    SELECT
        i.UserID,
        d.DocumentID,
        del.VerificationStatus,
        i.VerificationStatus,
        CASE
            WHEN i.VerificationStatus = N'Pending' THEN N'Submitted'
            WHEN i.VerificationStatus = N'Verified' THEN N'Approved'
            WHEN i.VerificationStatus = N'Rejected' THEN N'Rejected'
            ELSE i.VerificationStatus
        END,
        i.RejectionReason,
        i.VerifiedBy,
        GETDATE()
    FROM inserted i
    INNER JOIN deleted del ON del.UserID = i.UserID
    OUTER APPLY (
        SELECT TOP 1 DocumentID
        FROM dbo.IdentityDocuments doc
        WHERE doc.UserID = i.UserID AND doc.IsCurrent = 1
        ORDER BY doc.DocumentID DESC
    ) d
    WHERE ISNULL(i.VerificationStatus, N'') <> ISNULL(del.VerificationStatus, N'')
       OR ISNULL(i.IdentityVerified, 0) <> ISNULL(del.IdentityVerified, 0);
END
GO

IF OBJECT_ID('dbo.trg_TaskAssignmentHistory', 'TR') IS NOT NULL
    DROP TRIGGER dbo.trg_TaskAssignmentHistory;
GO

CREATE TRIGGER dbo.trg_TaskAssignmentHistory
ON dbo.AssignmentTasks
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT UPDATE(AssignedUserID)
        RETURN;

    INSERT INTO dbo.AssignmentTaskHistory (
        TaskID, OldStatus, NewStatus, PreviousAssignee, NewAssignee, ChangedBy, ChangedAt, Description
    )
    SELECT
        i.TaskID,
        i.Status,
        i.Status,
        del.AssignedUserID,
        i.AssignedUserID,
        i.LastChangedBy,
        GETDATE(),
        N'Assignee changed'
    FROM inserted i
    INNER JOIN deleted del ON del.TaskID = i.TaskID
    WHERE ISNULL(i.AssignedUserID, -1) <> ISNULL(del.AssignedUserID, -1);
END
GO

IF OBJECT_ID('dbo.trg_TaskStatusHistory', 'TR') IS NOT NULL
    DROP TRIGGER dbo.trg_TaskStatusHistory;
GO

CREATE TRIGGER dbo.trg_TaskStatusHistory
ON dbo.AssignmentTasks
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT UPDATE(Status)
        RETURN;

    INSERT INTO dbo.AssignmentTaskHistory (
        TaskID, OldStatus, NewStatus, PreviousAssignee, NewAssignee, ChangedBy, ChangedAt, Description
    )
    SELECT
        i.TaskID,
        del.Status,
        i.Status,
        i.AssignedUserID,
        i.AssignedUserID,
        i.LastChangedBy,
        GETDATE(),
        N'Status changed'
    FROM inserted i
    INNER JOIN deleted del ON del.TaskID = i.TaskID
    WHERE ISNULL(i.Status, N'') <> ISNULL(del.Status, N'');
END
GO

IF OBJECT_ID('dbo.trg_GroupMembershipAudit', 'TR') IS NOT NULL
    DROP TRIGGER dbo.trg_GroupMembershipAudit;
GO

CREATE TRIGGER dbo.trg_GroupMembershipAudit
ON dbo.AssignmentMembers
AFTER INSERT, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.AuditLogs (UserID, Action, EntityType, EntityID, Description, Timestamp)
    SELECT
        i.UserID,
        N'MemberJoined',
        N'AssignmentGroup',
        i.GroupID,
        N'Joined assignment group as ' + i.Responsibility,
        GETDATE()
    FROM inserted i;

    INSERT INTO dbo.AuditLogs (UserID, Action, EntityType, EntityID, Description, Timestamp)
    SELECT
        d.UserID,
        N'MemberRemoved',
        N'AssignmentGroup',
        d.GroupID,
        N'Removed from assignment group (' + d.Responsibility + N')',
        GETDATE()
    FROM deleted d;
END
GO

IF OBJECT_ID('dbo.trg_AccountStatusAudit', 'TR') IS NOT NULL
    DROP TRIGGER dbo.trg_AccountStatusAudit;
GO

CREATE TRIGGER dbo.trg_AccountStatusAudit
ON dbo.Users
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT UPDATE(AccountStatus)
        RETURN;

    INSERT INTO dbo.AuditLogs (UserID, Action, EntityType, EntityID, Description, Timestamp)
    SELECT
        i.VerifiedBy,
        N'AccountStatusChanged',
        N'User',
        i.UserID,
        N'AccountStatus ' + ISNULL(del.AccountStatus, N'?') + N' -> ' + ISNULL(i.AccountStatus, N'?')
            + CASE WHEN i.RejectionReason IS NULL THEN N'' ELSE N'. ' + i.RejectionReason END,
        GETDATE()
    FROM inserted i
    INNER JOIN deleted del ON del.UserID = i.UserID
    WHERE ISNULL(i.AccountStatus, N'') <> ISNULL(del.AccountStatus, N'');
END
GO

PRINT '==========================================';
PRINT 'Phase 2 identity / assignments / moderation applied.';
PRINT '==========================================';
GO
