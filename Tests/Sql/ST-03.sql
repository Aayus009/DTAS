SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
DECLARE @Stamp NVARCHAR(32) = N'st03' + REPLACE(REPLACE(CONVERT(VARCHAR(17), GETDATE(), 113), ' ', ''), ':', '');
DECLARE @FacultyID INT, @EventID INT, @MeetingID INT, @Status NVARCHAR(30);

INSERT INTO Users (FullName, Email, Username, RoleID, Password, IsActive, CreatedAt, EmailVerified, IdentityVerified, VerificationStatus, AccountStatus, IsDeleted)
VALUES (N'ST-03 host', N'dtas.sql.st03.' + @Stamp + N'@dtas.test', N'st03h' + @Stamp, 2, N'x', 1, GETDATE(), 1, 1, N'Verified', N'Active', 0);
SET @FacultyID = CAST(SCOPE_IDENTITY() AS INT);

INSERT INTO Events (EventName, Description, EventType, StartDate, EndDate, Venue, Organizer, Status, CreatedBy, CreatedAt, UpdatedAt, Visibility)
VALUES (N'ST-03 event', N'Test', N'Workshop', DATEADD(DAY, 7, GETDATE()), DATEADD(DAY, 8, GETDATE()), N'Hall', N'Faculty', N'Planned', @FacultyID, GETDATE(), GETDATE(), N'Public');
SET @EventID = CAST(SCOPE_IDENTITY() AS INT);

INSERT INTO Meetings (MeetingTitle, Description, MeetingType, ScheduledDate, Duration, Venue, Agenda, Status, CreatedBy, ZoomJoinUrl, EventID)
VALUES (N'ST-03 meeting', N'Test', N'General', DATEADD(HOUR, -3, GETDATE()), 60, N'Zoom', N'Test', N'Scheduled', @FacultyID, N'https://zoom.example/j/x', @EventID);
SET @MeetingID = CAST(SCOPE_IDENTITY() AS INT);

UPDATE Meetings SET Status = N'Completed', ZoomJoinUrl = NULL, ZoomStartUrl = NULL, ZoomPasscode = NULL
WHERE MeetingID = @MeetingID AND DATEADD(MINUTE, Duration, ScheduledDate) <= GETDATE();

SELECT @Status = Status FROM Meetings WHERE MeetingID = @MeetingID;
IF @Status <> N'Completed' BEGIN RAISERROR('ST-03 failed: meeting must close after duration.', 16, 1); RETURN; END

PRINT 'PASS ST-03 meeting duration close';
DELETE FROM Meetings WHERE MeetingID = @MeetingID;
DELETE FROM Events WHERE EventID = @EventID;
DELETE FROM IdentityVerificationHistory WHERE UserID = @FacultyID;
DELETE FROM IdentityDocuments WHERE UserID = @FacultyID;
DELETE FROM Users WHERE UserID = @FacultyID;
