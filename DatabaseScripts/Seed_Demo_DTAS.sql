-- DTAS isolated demo seed for DigitalTransparencyDB_Demo only.
-- Password for all demo accounts: Demo@DTAS2026
-- Hash is legacy SHA256 (AuthService PasswordHasher.Verify accepts it).

USE DigitalTransparencyDB_Demo;
GO

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;

DECLARE @Pwd NVARCHAR(128) = N'2ae319d05b50a05cb978b46ba84802f91cc06edb04791d31b61b082c27278833';
DECLARE @FacultyID INT, @AdminID INT, @StudentID INT, @StaffID INT, @GuestishID INT;
DECLARE @EventOpen INT, @EventWork INT, @ClubID INT, @MeetingID INT, @AssignID INT;

-- Soft hide noisy automated test accounts from admin lists
UPDATE Users
SET IsActive = 0,
    IsDeleted = 1,
    DeletedAt = ISNULL(DeletedAt, GETDATE())
WHERE Email LIKE N'dtas.autotest.%'
   OR Email LIKE N'dtas.sql.%';

-- Faculty: aayusgc901@gmail.com
IF EXISTS (SELECT 1 FROM Users WHERE Email = N'aayusgc901@gmail.com')
BEGIN
    UPDATE Users SET
        FullName = N'Aayus GC',
        Username = N'aayus.faculty',
        Password = @Pwd,
        RoleID = 2,
        DepartmentID = 1,
        Phone = N'9801112233',
        EmailVerified = 1,
        IdentityVerified = 1,
        VerificationStatus = N'Verified',
        AccountStatus = N'Active',
        IsActive = 1,
        IsDeleted = 0,
        InstitutionalID = N'FAC-901'
    WHERE Email = N'aayusgc901@gmail.com';
END
ELSE
BEGIN
    INSERT INTO Users
        (FullName, Email, Username, Password, RoleID, DepartmentID, Phone,
         EmailVerified, IdentityVerified, VerificationStatus, AccountStatus,
         InstitutionalID, IsActive, IsDeleted, CreatedAt)
    VALUES
        (N'Aayus GC', N'aayusgc901@gmail.com', N'aayus.faculty', @Pwd, 2, 1, N'9801112233',
         1, 1, N'Verified', N'Active', N'FAC-901', 1, 0, GETDATE());
END

-- Admin: aayusgc2061@gmail.com
IF EXISTS (SELECT 1 FROM Users WHERE Email = N'aayusgc2061@gmail.com')
BEGIN
    UPDATE Users SET
        FullName = N'Aayus Admin',
        Username = N'aayus.admin',
        Password = @Pwd,
        RoleID = 1,
        DepartmentID = 3,
        Phone = N'9802223344',
        EmailVerified = 1,
        IdentityVerified = 1,
        VerificationStatus = N'Verified',
        AccountStatus = N'Active',
        IsActive = 1,
        IsDeleted = 0,
        InstitutionalID = N'ADM-2061'
    WHERE Email = N'aayusgc2061@gmail.com';
END
ELSE
BEGIN
    INSERT INTO Users
        (FullName, Email, Username, Password, RoleID, DepartmentID, Phone,
         EmailVerified, IdentityVerified, VerificationStatus, AccountStatus,
         InstitutionalID, IsActive, IsDeleted, CreatedAt)
    VALUES
        (N'Aayus Admin', N'aayusgc2061@gmail.com', N'aayus.admin', @Pwd, 1, 3, N'9802223344',
         1, 1, N'Verified', N'Active', N'ADM-2061', 1, 0, GETDATE());
END

-- Extra student
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = N'demo.student@dtas.test')
BEGIN
    INSERT INTO Users
        (FullName, Email, Username, Password, RoleID, DepartmentID, Phone,
         EmailVerified, IdentityVerified, VerificationStatus, AccountStatus,
         InstitutionalID, IsActive, IsDeleted, CreatedAt)
    VALUES
        (N'Sneha Thapa', N'demo.student@dtas.test', N'sneha.thapa', @Pwd, 3, 1, N'9812345670',
         1, 1, N'Verified', N'Active', N'STU-4401', 1, 0, GETDATE());
END
ELSE
BEGIN
    UPDATE Users SET FullName = N'Sneha Thapa', Password = @Pwd, RoleID = 3,
        EmailVerified = 1, IdentityVerified = 1, VerificationStatus = N'Verified',
        AccountStatus = N'Active', IsActive = 1, IsDeleted = 0
    WHERE Email = N'demo.student@dtas.test';
END

-- Extra staff
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = N'demo.staff@dtas.test')
BEGIN
    INSERT INTO Users
        (FullName, Email, Username, Password, RoleID, DepartmentID, Phone,
         EmailVerified, IdentityVerified, VerificationStatus, AccountStatus,
         InstitutionalID, IsActive, IsDeleted, CreatedAt)
    VALUES
        (N'Rajan KC', N'demo.staff@dtas.test', N'rajan.kc', @Pwd, 4, 2, N'9812345671',
         1, 1, N'Verified', N'Active', N'STF-2201', 1, 0, GETDATE());
END
ELSE
BEGIN
    UPDATE Users SET Password = @Pwd, RoleID = 4, EmailVerified = 1, IdentityVerified = 1,
        VerificationStatus = N'Verified', AccountStatus = N'Active', IsActive = 1, IsDeleted = 0
    WHERE Email = N'demo.staff@dtas.test';
END

-- Locked student (email verified, ID pending) for gate screenshots
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = N'demo.pending@dtas.test')
BEGIN
    INSERT INTO Users
        (FullName, Email, Username, Password, RoleID, DepartmentID,
         EmailVerified, IdentityVerified, VerificationStatus, AccountStatus,
         IsActive, IsDeleted, CreatedAt)
    VALUES
        (N'Kiran Lama', N'demo.pending@dtas.test', N'kiran.lama', @Pwd, 3, 1,
         1, 0, N'Pending', N'Active', 1, 0, GETDATE());
END

SELECT @FacultyID = UserID FROM Users WHERE Email = N'aayusgc901@gmail.com';
SELECT @AdminID = UserID FROM Users WHERE Email = N'aayusgc2061@gmail.com';
SELECT @StudentID = UserID FROM Users WHERE Email = N'demo.student@dtas.test';
SELECT @StaffID = UserID FROM Users WHERE Email = N'demo.staff@dtas.test';
SELECT @GuestishID = UserID FROM Users WHERE Email = N'demo.pending@dtas.test';

-- Club owned by faculty
IF NOT EXISTS (SELECT 1 FROM Clubs WHERE ClubName = N'Open Source Circle' AND ISNULL(IsDeleted,0) = 0)
BEGIN
    INSERT INTO Clubs (ClubName, Description, LeadUserID, IsActive, CreatedBy, CreatedAt, IsPublic, InviteCode, IsDeleted, IsRestricted)
    VALUES (N'Open Source Circle', N'Weekly coding meetups and peer reviews.', @FacultyID, 1, @FacultyID, GETDATE(), 1, N'OSC7K2', 0, 0);
END
SELECT @ClubID = ClubID FROM Clubs WHERE ClubName = N'Open Source Circle' AND ISNULL(IsDeleted,0) = 0;

IF NOT EXISTS (SELECT 1 FROM ClubMembers WHERE ClubID = @ClubID AND UserID = @FacultyID)
    INSERT INTO ClubMembers (ClubID, UserID, Role, JoinedAt, InviteStatus, IsActive)
    VALUES (@ClubID, @FacultyID, N'Lead', GETDATE(), N'Accepted', 1);
ELSE
    UPDATE ClubMembers SET Role = N'Lead', InviteStatus = N'Accepted', IsActive = 1
    WHERE ClubID = @ClubID AND UserID = @FacultyID;

IF NOT EXISTS (SELECT 1 FROM ClubMembers WHERE ClubID = @ClubID AND UserID = @StudentID)
    INSERT INTO ClubMembers (ClubID, UserID, Role, JoinedAt, InviteStatus, IsActive)
    VALUES (@ClubID, @StudentID, N'Member', GETDATE(), N'Accepted', 1);

IF NOT EXISTS (SELECT 1 FROM ClubMembers WHERE ClubID = @ClubID AND UserID = @StaffID)
    INSERT INTO ClubMembers (ClubID, UserID, Role, JoinedAt, InviteStatus, IsActive)
    VALUES (@ClubID, @StaffID, N'Member', DATEADD(DAY,-1,GETDATE()), N'Requested', 0);

-- Planned public event
IF NOT EXISTS (SELECT 1 FROM Events WHERE EventName = N'Spring Project Fair' AND ISNULL(IsDeleted,0) = 0)
BEGIN
    INSERT INTO Events
        (EventName, Description, EventType, StartDate, EndDate, Venue, Budget, Organizer, Status,
         CreatedBy, CreatedAt, Objective, Priority, CompletionPercent, InviteCode, Visibility,
         IsDisabled, IsDeleted, ClubID)
    VALUES
        (N'Spring Project Fair', N'Student project demos for the CS department.', N'Academic',
         DATEADD(DAY, 12, GETDATE()), DATEADD(HOUR, 4, DATEADD(DAY, 12, GETDATE())),
         N'Block A Hall', 15000, N'Aayus GC', N'Planned',
         @FacultyID, GETDATE(), N'Show completed student projects.', N'Medium', 18, N'FAIR9X', N'Public',
         0, 0, @ClubID);
END
SELECT @EventOpen = EventID FROM Events WHERE EventName = N'Spring Project Fair' AND ISNULL(IsDeleted,0) = 0;

-- In progress workspace event
IF NOT EXISTS (SELECT 1 FROM Events WHERE EventName = N'Team Progress Week' AND ISNULL(IsDeleted,0) = 0)
BEGIN
    INSERT INTO Events
        (EventName, Description, EventType, StartDate, EndDate, Venue, Budget, Organizer, Status,
         CreatedBy, CreatedAt, Objective, Priority, CompletionPercent, InviteCode, Visibility,
         IsDisabled, IsDeleted, ClubID)
    VALUES
        (N'Team Progress Week', N'Short sprint for FYP checkpoints and peer review.', N'Project',
         DATEADD(DAY, -2, GETDATE()), DATEADD(DAY, 5, GETDATE()),
         N'Lab 2', 5000, N'Aayus GC', N'InProgress',
         @FacultyID, GETDATE(), N'Finish checkpoint slides.', N'High', 42, N'TPW4M1', N'Private',
         0, 0, NULL);
END
SELECT @EventWork = EventID FROM Events WHERE EventName = N'Team Progress Week' AND ISNULL(IsDeleted,0) = 0;

-- Memberships
MERGE EventMembers AS t
USING (SELECT @EventOpen AS EventID, @FacultyID AS UserID, N'Event Admin' AS RoleInEvent, N'Accepted' AS InviteStatus, 1 AS IsActive
       UNION ALL SELECT @EventOpen, @StudentID, N'Member', N'Accepted', 1
       UNION ALL SELECT @EventOpen, @StaffID, N'Member', N'Requested', 0
       UNION ALL SELECT @EventWork, @FacultyID, N'Event Admin', N'Accepted', 1
       UNION ALL SELECT @EventWork, @StudentID, N'Member', N'Accepted', 1) AS s
ON t.EventID = s.EventID AND t.UserID = s.UserID
WHEN MATCHED THEN UPDATE SET RoleInEvent = s.RoleInEvent, InviteStatus = s.InviteStatus, IsActive = s.IsActive
WHEN NOT MATCHED THEN
    INSERT (EventID, UserID, RoleInEvent, JoinedBy, JoinedAt, InviteStatus, IsActive)
    VALUES (s.EventID, s.UserID, s.RoleInEvent, @FacultyID, GETDATE(), s.InviteStatus, s.IsActive);

-- Meeting for progress week (live window)
IF NOT EXISTS (SELECT 1 FROM Meetings WHERE MeetingTitle = N'Checkpoint sync' AND EventID = @EventWork)
BEGIN
    INSERT INTO Meetings
        (MeetingTitle, Description, MeetingType, ScheduledDate, Duration, Venue, Agenda, Status,
         CreatedBy, EventID, CreatedAt, ZoomJoinUrl)
    VALUES
        (N'Checkpoint sync', N'Quick status round for each team.', N'Online',
         DATEADD(MINUTE, -10, GETDATE()), 60, N'Zoom', N'Status, blockers, next steps.', N'Scheduled',
         @FacultyID, @EventWork, GETDATE(), N'https://zoom.us/j/87000000001');
END

-- Past meeting (closed look)
IF NOT EXISTS (SELECT 1 FROM Meetings WHERE MeetingTitle = N'Kickoff notes' AND EventID = @EventWork)
BEGIN
    INSERT INTO Meetings
        (MeetingTitle, Description, MeetingType, ScheduledDate, Duration, Venue, Agenda, MinutesOfMeeting, Status,
         CreatedBy, EventID, CreatedAt)
    VALUES
        (N'Kickoff notes', N'Opened the sprint and shared the checklist.', N'Hybrid',
         DATEADD(DAY, -3, GETDATE()), 45, N'Lab 2', N'Roles and deadlines.',
         N'Teams confirmed checkpoint dates. Next sync in three days.', N'Completed',
         @FacultyID, @EventWork, GETDATE());
END

-- Faculty assignment with group
IF NOT EXISTS (SELECT 1 FROM Assignments WHERE AssignmentName = N'FYP Checkpoint One' AND ISNULL(IsDeleted,0) = 0)
BEGIN
    INSERT INTO Assignments
        (AssignmentName, Description, Deadline, AssignmentCode, CreatedBy, CreatedAt, Status, IsDeleted, AssignmentType)
    VALUES
        (N'FYP Checkpoint One', N'Submit progress notes and a short demo plan.',
         DATEADD(DAY, 8, GETDATE()), N'CP1-7742', @FacultyID, GETDATE(), N'Active', 0, N'College');
END
SELECT @AssignID = AssignmentID FROM Assignments WHERE AssignmentName = N'FYP Checkpoint One' AND ISNULL(IsDeleted,0) = 0;

IF NOT EXISTS (SELECT 1 FROM AssignmentGroups WHERE AssignmentID = @AssignID AND GroupName = N'Team Nettle')
BEGIN
    INSERT INTO AssignmentGroups (AssignmentID, GroupName, LeaderID, IsFinalized, CreatedAt, IsDeleted)
    VALUES (@AssignID, N'Team Nettle', @StudentID, 0, GETDATE(), 0);
END

DECLARE @GroupID INT = (SELECT TOP 1 GroupID FROM AssignmentGroups WHERE AssignmentID = @AssignID AND GroupName = N'Team Nettle' AND ISNULL(IsDeleted,0)=0);

IF NOT EXISTS (SELECT 1 FROM AssignmentMembers WHERE GroupID = @GroupID AND UserID = @StudentID)
    INSERT INTO AssignmentMembers (GroupID, UserID, Responsibility, JoinedAt)
    VALUES (@GroupID, @StudentID, N'Member', GETDATE());
IF NOT EXISTS (SELECT 1 FROM AssignmentMembers WHERE GroupID = @GroupID AND UserID = @FacultyID)
    INSERT INTO AssignmentMembers (GroupID, UserID, Responsibility, JoinedAt)
    VALUES (@GroupID, @FacultyID, N'Leader', GETDATE());

IF NOT EXISTS (SELECT 1 FROM Notifications WHERE UserID = @FacultyID AND Title = N'Join request waiting')
BEGIN
    INSERT INTO Notifications (UserID, Title, Message, IsRead, NotificationType, CreatedAt)
    VALUES (@FacultyID, N'Join request waiting', N'Rajan KC asked to join Spring Project Fair.', 0, N'Event', GETDATE());
END

PRINT 'Demo seed complete.';
PRINT 'FacultyID=' + CAST(@FacultyID AS NVARCHAR(20));
PRINT 'AdminID=' + CAST(@AdminID AS NVARCHAR(20));
PRINT 'StudentID=' + CAST(@StudentID AS NVARCHAR(20));
PRINT 'EventOpen=' + CAST(@EventOpen AS NVARCHAR(20));
PRINT 'EventWork=' + CAST(@EventWork AS NVARCHAR(20));
GO
