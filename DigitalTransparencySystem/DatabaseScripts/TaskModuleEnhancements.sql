-- ============================================
-- TASK MODULE ENHANCEMENTS
-- Run this script to add new tables for
-- Task Comments, Checklists, Attachments,
-- and Task Teams
-- ============================================

-- Task Comments table (Discussion thread)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TaskComments')
BEGIN
    CREATE TABLE TaskComments (
        CommentID INT PRIMARY KEY IDENTITY(1,1),
        TaskID INT NOT NULL,
        UserID INT NOT NULL,
        Comment NVARCHAR(MAX) NOT NULL,
        CreatedAt DATETIME DEFAULT GETDATE(),
        FOREIGN KEY (TaskID) REFERENCES Tasks(TaskID) ON DELETE CASCADE,
        FOREIGN KEY (UserID) REFERENCES Users(UserID)
    );
    PRINT 'TaskComments table created.';
END
ELSE
    PRINT 'TaskComments table already exists.';

-- Task Checklist table (Sub-tasks)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TaskChecklist')
BEGIN
    CREATE TABLE TaskChecklist (
        ItemID INT PRIMARY KEY IDENTITY(1,1),
        TaskID INT NOT NULL,
        ItemText NVARCHAR(300) NOT NULL,
        IsCompleted BIT DEFAULT 0,
        CreatedAt DATETIME DEFAULT GETDATE(),
        FOREIGN KEY (TaskID) REFERENCES Tasks(TaskID) ON DELETE CASCADE
    );
    PRINT 'TaskChecklist table created.';
END
ELSE
    PRINT 'TaskChecklist table already exists.';

-- Ensure Attachments table exists (may already exist from init script)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Attachments')
BEGIN
    CREATE TABLE Attachments (
        AttachmentID INT PRIMARY KEY IDENTITY(1,1),
        FileName NVARCHAR(255) NOT NULL,
        FilePath NVARCHAR(500) NOT NULL,
        FileSize BIGINT NOT NULL,
        FileType NVARCHAR(50),
        UploadedBy INT NOT NULL,
        RelatedID INT NOT NULL,
        RelatedType NVARCHAR(50) NOT NULL,
        CreatedAt DATETIME DEFAULT GETDATE(),
        FOREIGN KEY (UploadedBy) REFERENCES Users(UserID)
    );
    PRINT 'Attachments table created.';
END
ELSE
    PRINT 'Attachments table already exists.';

-- Add UploadedAt column to Attachments if missing
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Attachments') AND name = 'UploadedAt')
BEGIN
    ALTER TABLE Attachments ADD UploadedAt DATETIME DEFAULT GETDATE();
    PRINT 'UploadedAt column added to Attachments.';
END

-- Ensure Notifications table has the required columns
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Notifications') AND name = 'RelatedType')
BEGIN
    ALTER TABLE Notifications ADD RelatedType NVARCHAR(50);
    PRINT 'RelatedType column added to Notifications.';
END

PRINT '==========================================';
PRINT 'Task module enhancements applied successfully.';
PRINT '==========================================';

-- ============================================
-- TASK TEAM MODULE
-- Ad-hoc team management per task
-- ============================================

-- Add LeaderID column to Tasks table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Tasks') AND name = 'LeaderID')
BEGIN
    ALTER TABLE Tasks ADD LeaderID INT NULL;
    PRINT 'LeaderID column added to Tasks.';
END

-- TaskTeams table (one team per task)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TaskTeams')
BEGIN
    CREATE TABLE TaskTeams (
        TeamID INT PRIMARY KEY IDENTITY(1,1),
        TaskID INT NOT NULL UNIQUE,
        LeaderID INT NOT NULL,
        ProgressPercent INT DEFAULT 0,
        CreatedAt DATETIME DEFAULT GETDATE(),
        FOREIGN KEY (TaskID) REFERENCES Tasks(TaskID) ON DELETE CASCADE,
        FOREIGN KEY (LeaderID) REFERENCES Users(UserID)
    );
    PRINT 'TaskTeams table created.';
END
ELSE
    PRINT 'TaskTeams table already exists.';

-- TaskTeamMembers table (invite/accept flow)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TaskTeamMembers')
BEGIN
    CREATE TABLE TaskTeamMembers (
        MemberID INT PRIMARY KEY IDENTITY(1,1),
        TeamID INT NOT NULL,
        UserID INT NOT NULL,
        Status NVARCHAR(20) DEFAULT 'Invited',
        InvitedAt DATETIME DEFAULT GETDATE(),
        RespondedAt DATETIME NULL,
        FOREIGN KEY (TeamID) REFERENCES TaskTeams(TeamID) ON DELETE CASCADE,
        FOREIGN KEY (UserID) REFERENCES Users(UserID)
    );
    PRINT 'TaskTeamMembers table created.';
END
ELSE
    PRINT 'TaskTeamMembers table already exists.';

-- LeadershipHistory audit table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LeadershipHistory')
BEGIN
    CREATE TABLE LeadershipHistory (
        HistoryID INT PRIMARY KEY IDENTITY(1,1),
        TaskID INT NOT NULL,
        PreviousLeaderID INT NULL,
        NewLeaderID INT NOT NULL,
        ChangedBy INT NOT NULL,
        ChangedAt DATETIME DEFAULT GETDATE(),
        Reason NVARCHAR(500) NULL,
        FOREIGN KEY (TaskID) REFERENCES Tasks(TaskID),
        FOREIGN KEY (PreviousLeaderID) REFERENCES Users(UserID),
        FOREIGN KEY (NewLeaderID) REFERENCES Users(UserID),
        FOREIGN KEY (ChangedBy) REFERENCES Users(UserID)
    );
    PRINT 'LeadershipHistory table created.';
END
ELSE
    PRINT 'LeadershipHistory table already exists.';

PRINT '==========================================';
PRINT 'Task Team module applied successfully.';
PRINT '==========================================';
