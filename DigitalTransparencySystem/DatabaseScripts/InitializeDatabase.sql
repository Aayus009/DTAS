-- ============================================
-- DTAS - Database Initialization Script
-- Run this against the DigitalTransparencyDB database
-- ============================================

-- ROLES
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Roles')
BEGIN
    CREATE TABLE Roles (
        RoleID INT PRIMARY KEY IDENTITY(1,1),
        RoleName NVARCHAR(50) NOT NULL UNIQUE,
        Description NVARCHAR(200),
        CreatedAt DATETIME DEFAULT GETDATE()
    );
    INSERT INTO Roles (RoleName, Description) VALUES
        ('Admin', 'Full system access'),
        ('Faculty', 'Faculty member with task and meeting access'),
        ('Student', 'Student representative with limited access'),
        ('Staff', 'Institutional staff member');
END;

-- DEPARTMENTS
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Departments')
BEGIN
    CREATE TABLE Departments (
        DepartmentID INT PRIMARY KEY IDENTITY(1,1),
        DepartmentName NVARCHAR(100) NOT NULL,
        Description NVARCHAR(300),
        IsActive BIT DEFAULT 1,
        CreatedAt DATETIME DEFAULT GETDATE()
    );
    INSERT INTO Departments (DepartmentName, Description) VALUES
        ('Computer Science', 'CS Department'),
        ('Engineering', 'Engineering Department'),
        ('Administration', 'Central Administration'),
        ('Student Affairs', 'Student Services');
END;

-- USERS
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        UserID INT PRIMARY KEY IDENTITY(1,1),
        FullName NVARCHAR(200) NOT NULL,
        Email NVARCHAR(200) NOT NULL UNIQUE,
        Username NVARCHAR(100) NOT NULL UNIQUE,
        Password NVARCHAR(500) NOT NULL,
        RoleID INT NOT NULL DEFAULT 3,
        DepartmentID INT NULL,
        Phone NVARCHAR(20) NULL,
        ProfileImage NVARCHAR(500) NULL,
        IsActive BIT DEFAULT 1,
        CreatedAt DATETIME DEFAULT GETDATE(),
        CONSTRAINT FK_Users_Role FOREIGN KEY (RoleID) REFERENCES Roles(RoleID),
        CONSTRAINT FK_Users_Department FOREIGN KEY (DepartmentID) REFERENCES Departments(DepartmentID)
    );
    PRINT 'Users table created.';
END
ELSE
BEGIN
    -- Legacy layout assurance: add missing columns if the table already exists
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'DepartmentID')
    BEGIN
        ALTER TABLE Users ADD DepartmentID INT NULL;
        ALTER TABLE Users ADD CONSTRAINT FK_Users_Department FOREIGN KEY (DepartmentID) REFERENCES Departments(DepartmentID);
    END;
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'Phone')
        ALTER TABLE Users ADD Phone NVARCHAR(20) NULL;
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'ProfileImage')
        ALTER TABLE Users ADD ProfileImage NVARCHAR(500) NULL;
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'Username')
        ALTER TABLE Users ADD Username NVARCHAR(100) NULL;
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'Password')
        ALTER TABLE Users ADD Password NVARCHAR(500) NULL;
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'IsActive')
        ALTER TABLE Users ADD IsActive BIT DEFAULT 1;
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'CreatedAt')
        ALTER TABLE Users ADD CreatedAt DATETIME DEFAULT GETDATE();
    PRINT 'Users table verified / upgraded.';
END

-- OTP CODES (Two-factor authentication tokens)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OTPCodes')
BEGIN
    CREATE TABLE OTPCodes (
        OTPID INT PRIMARY KEY IDENTITY(1,1),
        UserID INT NOT NULL,
        OTP NVARCHAR(10) NOT NULL,
        Expiry DATETIME NOT NULL,
        IsUsed BIT DEFAULT 0,
        CreatedAt DATETIME DEFAULT GETDATE(),
        CONSTRAINT FK_OTP_User FOREIGN KEY (UserID) REFERENCES Users(UserID)
    );
    PRINT 'OTPCodes table created.';
END

-- LOGIN LOGS (Audit trail)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LoginLogs')
BEGIN
    CREATE TABLE LoginLogs (
        LogID INT PRIMARY KEY IDENTITY(1,1),
        UserID INT NOT NULL,
        FullName NVARCHAR(200),
        Username NVARCHAR(100),
        LoginTime DATETIME DEFAULT GETDATE(),
        LogoutTime DATETIME NULL,
        IPAddress NVARCHAR(100),
        CONSTRAINT FK_LoginLogs_User FOREIGN KEY (UserID) REFERENCES Users(UserID)
    );
    PRINT 'LoginLogs table created.';
END

-- PASSWORD RESET TOKENS (expiring reset links)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PasswordResetTokens')
BEGIN
    CREATE TABLE PasswordResetTokens (
        TokenID INT PRIMARY KEY IDENTITY(1,1),
        UserID INT NOT NULL,
        Token NVARCHAR(500) NOT NULL,
        ExpiresAt DATETIME NOT NULL,
        IsUsed BIT DEFAULT 0,
        CreatedAt DATETIME DEFAULT GETDATE(),
        CONSTRAINT FK_PRT_User FOREIGN KEY (UserID) REFERENCES Users(UserID)
    );
    PRINT 'PasswordResetTokens table created.';
END

-- EVENTS
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Events')
BEGIN
    CREATE TABLE Events (
        EventID INT PRIMARY KEY IDENTITY(1,1),
        EventName NVARCHAR(200) NOT NULL,
        Description NVARCHAR(MAX),
        EventType NVARCHAR(50) DEFAULT 'General',
        StartDate DATETIME NOT NULL,
        EndDate DATETIME NOT NULL,
        Venue NVARCHAR(200),
        Budget DECIMAL(18,2) DEFAULT 0,
        Organizer NVARCHAR(200),
        Status NVARCHAR(30) DEFAULT 'Planned',
        CreatedBy INT,
        CreatedAt DATETIME DEFAULT GETDATE(),
        UpdatedAt DATETIME DEFAULT GETDATE(),
        CONSTRAINT FK_Events_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(UserID)
    );
END;

-- MEETINGS
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Meetings')
BEGIN
    CREATE TABLE Meetings (
        MeetingID INT PRIMARY KEY IDENTITY(1,1),
        MeetingTitle NVARCHAR(200) NOT NULL,
        Description NVARCHAR(MAX),
        MeetingType NVARCHAR(50) DEFAULT 'General',
        ScheduledDate DATETIME NOT NULL,
        Duration INT DEFAULT 60,
        Venue NVARCHAR(200),
        Agenda NVARCHAR(MAX),
        MinutesOfMeeting NVARCHAR(MAX),
        Status NVARCHAR(30) DEFAULT 'Scheduled',
        CreatedBy INT,
        EventID INT NULL,
        CreatedAt DATETIME DEFAULT GETDATE(),
        CONSTRAINT FK_Meetings_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(UserID),
        CONSTRAINT FK_Meetings_Event FOREIGN KEY (EventID) REFERENCES Events(EventID)
    );
END;

-- MEETING PARTICIPANTS
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MeetingParticipants')
BEGIN
    CREATE TABLE MeetingParticipants (
        ParticipantID INT PRIMARY KEY IDENTITY(1,1),
        MeetingID INT NOT NULL,
        UserID INT NOT NULL,
        Role NVARCHAR(50) DEFAULT 'Attendee',
        AttendanceStatus NVARCHAR(30) DEFAULT 'Pending',
        CONSTRAINT FK_MP_Meeting FOREIGN KEY (MeetingID) REFERENCES Meetings(MeetingID) ON DELETE CASCADE,
        CONSTRAINT FK_MP_User FOREIGN KEY (UserID) REFERENCES Users(UserID)
    );
END;

-- DECISIONS
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Decisions')
BEGIN
    CREATE TABLE Decisions (
        DecisionID INT PRIMARY KEY IDENTITY(1,1),
        DecisionTitle NVARCHAR(300) NOT NULL,
        Description NVARCHAR(MAX),
        Priority NVARCHAR(30) DEFAULT 'Medium',
        Status NVARCHAR(30) DEFAULT 'Proposed',
        ResponsibleUserID INT NULL,
        MeetingID INT NULL,
        EventID INT NULL,
        DueDate DATETIME NULL,
        CreatedBy INT,
        CreatedAt DATETIME DEFAULT GETDATE(),
        UpdatedAt DATETIME DEFAULT GETDATE(),
        CONSTRAINT FK_Decisions_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(UserID),
        CONSTRAINT FK_Decisions_Responsible FOREIGN KEY (ResponsibleUserID) REFERENCES Users(UserID),
        CONSTRAINT FK_Decisions_Meeting FOREIGN KEY (MeetingID) REFERENCES Meetings(MeetingID),
        CONSTRAINT FK_Decisions_Event FOREIGN KEY (EventID) REFERENCES Events(EventID)
    );
END;

-- DECISION HISTORY
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DecisionHistory')
BEGIN
    CREATE TABLE DecisionHistory (
        HistoryID INT PRIMARY KEY IDENTITY(1,1),
        DecisionID INT NOT NULL,
        OldStatus NVARCHAR(30),
        NewStatus NVARCHAR(30) NOT NULL,
        ChangedBy INT,
        Comments NVARCHAR(MAX),
        ChangedAt DATETIME DEFAULT GETDATE(),
        CONSTRAINT FK_DH_Decision FOREIGN KEY (DecisionID) REFERENCES Decisions(DecisionID) ON DELETE CASCADE,
        CONSTRAINT FK_DH_ChangedBy FOREIGN KEY (ChangedBy) REFERENCES Users(UserID)
    );
END;

-- TASKS
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Tasks')
BEGIN
    CREATE TABLE Tasks (
        TaskID INT PRIMARY KEY IDENTITY(1,1),
        TaskTitle NVARCHAR(300) NOT NULL,
        Description NVARCHAR(MAX),
        Priority NVARCHAR(30) DEFAULT 'Medium',
        Status NVARCHAR(30) DEFAULT 'Pending',
        DueDate DATETIME,
        DecisionID INT NULL,
        EventID INT NULL,
        MeetingID INT NULL,
        CreatedBy INT,
        CreatedAt DATETIME DEFAULT GETDATE(),
        UpdatedAt DATETIME DEFAULT GETDATE(),
        CONSTRAINT FK_Tasks_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(UserID),
        CONSTRAINT FK_Tasks_Decision FOREIGN KEY (DecisionID) REFERENCES Decisions(DecisionID),
        CONSTRAINT FK_Tasks_Event FOREIGN KEY (EventID) REFERENCES Events(EventID),
        CONSTRAINT FK_Tasks_Meeting FOREIGN KEY (MeetingID) REFERENCES Meetings(MeetingID)
    );
END;

-- TASK ASSIGNMENTS
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TaskAssignments')
BEGIN
    CREATE TABLE TaskAssignments (
        AssignmentID INT PRIMARY KEY IDENTITY(1,1),
        TaskID INT NOT NULL,
        UserID INT NOT NULL,
        AssignedAt DATETIME DEFAULT GETDATE(),
        CONSTRAINT FK_TA_Task FOREIGN KEY (TaskID) REFERENCES Tasks(TaskID) ON DELETE CASCADE,
        CONSTRAINT FK_TA_User FOREIGN KEY (UserID) REFERENCES Users(UserID)
    );
END;

-- TASK UPDATES
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TaskUpdates')
BEGIN
    CREATE TABLE TaskUpdates (
        UpdateID INT PRIMARY KEY IDENTITY(1,1),
        TaskID INT NOT NULL,
        UserID INT NOT NULL,
        OldStatus NVARCHAR(30),
        NewStatus NVARCHAR(30) NOT NULL,
        Comment NVARCHAR(MAX),
        AttachmentPath NVARCHAR(500) NULL,
        UpdatedAt DATETIME DEFAULT GETDATE(),
        CONSTRAINT FK_TU_Task FOREIGN KEY (TaskID) REFERENCES Tasks(TaskID) ON DELETE CASCADE,
        CONSTRAINT FK_TU_User FOREIGN KEY (UserID) REFERENCES Users(UserID)
    );
END;

-- FEEDBACK
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Feedback')
BEGIN
    CREATE TABLE Feedback (
        FeedbackID INT PRIMARY KEY IDENTITY(1,1),
        UserID INT NULL,
        Category NVARCHAR(50) DEFAULT 'Suggestion',
        Title NVARCHAR(200) NOT NULL,
        Description NVARCHAR(MAX),
        Rating INT NULL,
        Status NVARCHAR(30) DEFAULT 'New',
        AdminResponse NVARCHAR(MAX) NULL,
        CreatedAt DATETIME DEFAULT GETDATE(),
        UpdatedAt DATETIME DEFAULT GETDATE(),
        CONSTRAINT FK_Feedback_User FOREIGN KEY (UserID) REFERENCES Users(UserID)
    );
END;

-- POLLS
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Polls')
BEGIN
    CREATE TABLE Polls (
        PollID INT PRIMARY KEY IDENTITY(1,1),
        PollTitle NVARCHAR(200) NOT NULL,
        Description NVARCHAR(MAX),
        StartDate DATETIME DEFAULT GETDATE(),
        EndDate DATETIME,
        IsActive BIT DEFAULT 1,
        CreatedBy INT,
        CreatedAt DATETIME DEFAULT GETDATE(),
        EventID INT NULL,
        DecisionID INT NULL,
        CONSTRAINT FK_Polls_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(UserID)
    );
END
ELSE
BEGIN
    IF COL_LENGTH('Polls', 'EventID') IS NULL
        ALTER TABLE Polls ADD EventID INT NULL;
    IF COL_LENGTH('Polls', 'DecisionID') IS NULL
        ALTER TABLE Polls ADD DecisionID INT NULL;
END;

-- POLL OPTIONS
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PollOptions')
BEGIN
    CREATE TABLE PollOptions (
        OptionID INT PRIMARY KEY IDENTITY(1,1),
        PollID INT NOT NULL,
        OptionText NVARCHAR(300) NOT NULL,
        VoteCount INT DEFAULT 0,
        CONSTRAINT FK_PO_Poll FOREIGN KEY (PollID) REFERENCES Polls(PollID) ON DELETE CASCADE
    );
END;

-- VOTES
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Votes')
BEGIN
    CREATE TABLE Votes (
        VoteID INT PRIMARY KEY IDENTITY(1,1),
        PollID INT NOT NULL,
        OptionID INT NOT NULL,
        UserID INT NOT NULL,
        VotedAt DATETIME DEFAULT GETDATE(),
        CONSTRAINT FK_V_Poll FOREIGN KEY (PollID) REFERENCES Polls(PollID),
        CONSTRAINT FK_V_Option FOREIGN KEY (OptionID) REFERENCES PollOptions(OptionID),
        CONSTRAINT FK_V_User FOREIGN KEY (UserID) REFERENCES Users(UserID),
        CONSTRAINT UQ_Vote UNIQUE (PollID, UserID)
    );
END;

-- NOTIFICATIONS
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Notifications')
BEGIN
    CREATE TABLE Notifications (
        NotificationID INT PRIMARY KEY IDENTITY(1,1),
        UserID INT NOT NULL,
        Title NVARCHAR(200) NOT NULL,
        Message NVARCHAR(MAX),
        IsRead BIT DEFAULT 0,
        NotificationType NVARCHAR(50) DEFAULT 'General',
        RelatedID INT NULL,
        RelatedType NVARCHAR(50) NULL,
        CreatedAt DATETIME DEFAULT GETDATE(),
        CONSTRAINT FK_Notifications_User FOREIGN KEY (UserID) REFERENCES Users(UserID)
    );
END;

-- REPORTS
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Reports')
BEGIN
    CREATE TABLE Reports (
        ReportID INT PRIMARY KEY IDENTITY(1,1),
        ReportTitle NVARCHAR(300) NOT NULL,
        ReportType NVARCHAR(50) NOT NULL,
        Description NVARCHAR(MAX),
        GeneratedBy INT,
        FilePath NVARCHAR(500) NULL,
        CreatedAt DATETIME DEFAULT GETDATE(),
        CONSTRAINT FK_Reports_GeneratedBy FOREIGN KEY (GeneratedBy) REFERENCES Users(UserID)
    );
END;

-- ATTACHMENTS
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Attachments')
BEGIN
    CREATE TABLE Attachments (
        AttachmentID INT PRIMARY KEY IDENTITY(1,1),
        FileName NVARCHAR(300) NOT NULL,
        FilePath NVARCHAR(500) NOT NULL,
        FileSize INT,
        FileType NVARCHAR(50),
        UploadedBy INT,
        RelatedID INT,
        RelatedType NVARCHAR(50),
        UploadedAt DATETIME DEFAULT GETDATE(),
        CONSTRAINT FK_Attachments_UploadedBy FOREIGN KEY (UploadedBy) REFERENCES Users(UserID)
    );
END;

PRINT 'Database initialization complete.';
