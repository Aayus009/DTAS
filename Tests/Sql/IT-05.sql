SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
DECLARE @Stamp NVARCHAR(32) = N'it05' + REPLACE(REPLACE(CONVERT(VARCHAR(17), GETDATE(), 113), ' ', ''), ':', '');
DECLARE @FacultyID INT, @EventID INT, @MeetingID INT, @Status NVARCHAR(30);

INSERT INTO Users (FullName, Email, Username, RoleID, Password, IsActive, CreatedAt, EmailVerified, IdentityVerified, VerificationStatus, AccountStatus, IsDeleted)
VALUES (N'DTAS sqlcmd host', N'dtas.sql.host.' + @Stamp + N'@dtas.test', N'sqlho' + @Stamp, 2, N'x', 1, GETDATE(), 1, 1, N'Verified', N'Active', 0);
SET @FacultyID = CAST(SCOPE_IDENTITY() AS INT);

INSERT INTO Events (EventName, Description, EventType, StartDate, EndDate, Venue, Organizer, Status, CreatedBy, CreatedAt, UpdatedAt, Visibility)
VALUES (N'IT-05 meeting', N'Test', N'Workshop', DATEADD(DAY, 7, GETDATE()), DATEADD(DAY, 8, GETDATE()), N'Test hall', N'Faculty', N'Planned', @FacultyID, GETDATE(), GETDATE(), N'Public');
SET @EventID = CAST(SCOPE_IDENTITY() AS INT);

INSERT INTO Meetings
    (MeetingTitle, Description, MeetingType, ScheduledDate, Duration, Venue, Agenda, Status, CreatedBy,
     ZoomJoinUrl, ZoomStartUrl, ZoomPasscode, EventID)
VALUES
    (N'IT-05 expired', N'Test', N'General', DATEADD(HOUR, -3, GETDATE()), 60, N'Zoom (online)', N'Test', N'Scheduled', @FacultyID,
     N'https://zoom.example/j/test', N'https://zoom.example/s/test', N'123456', @EventID);
SET @MeetingID = CAST(SCOPE_IDENTITY() AS INT);

UPDATE Meetings
SET Status = N'Completed', ZoomJoinUrl = NULL, ZoomStartUrl = NULL, ZoomPasscode = NULL
WHERE MeetingID = @MeetingID
  AND DATEADD(MINUTE, Duration, ScheduledDate) <= GETDATE();

SELECT @Status = Status FROM Meetings WHERE MeetingID = @MeetingID;
IF @Status <> N'Completed' OR EXISTS (SELECT 1 FROM Meetings WHERE MeetingID = @MeetingID AND ZoomJoinUrl IS NOT NULL)
BEGIN
    RAISERROR('IT-05 failed: expired meeting must be Completed with join URL cleared.', 16, 1);
    RETURN;
END

PRINT 'PASS IT-05 Meetings close after duration';

DELETE FROM Meetings WHERE MeetingID = @MeetingID;
DELETE FROM Events WHERE EventID = @EventID;
DELETE FROM IdentityVerificationHistory WHERE UserID = @FacultyID;
DELETE FROM IdentityDocuments WHERE UserID = @FacultyID;
DELETE FROM Users WHERE UserID = @FacultyID;
