SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
DECLARE @Stamp NVARCHAR(32) = N'it03' + REPLACE(REPLACE(CONVERT(VARCHAR(17), GETDATE(), 113), ' ', ''), ':', '');
DECLARE @Code NVARCHAR(20) = N'IT03' + RIGHT(@Stamp, 6);
DECLARE @FacultyID INT, @JoinID INT, @EventID INT, @Status NVARCHAR(30);

INSERT INTO Users (FullName, Email, Username, RoleID, Password, IsActive, CreatedAt, EmailVerified, IdentityVerified, VerificationStatus, AccountStatus, IsDeleted)
VALUES (N'DTAS sqlcmd faculty', N'dtas.sql.fac.' + @Stamp + N'@dtas.test', N'sqlfac' + @Stamp, 2, N'x', 1, GETDATE(), 1, 1, N'Verified', N'Active', 0);
SET @FacultyID = CAST(SCOPE_IDENTITY() AS INT);

INSERT INTO Users (FullName, Email, Username, RoleID, Password, IsActive, CreatedAt, EmailVerified, IdentityVerified, VerificationStatus, AccountStatus, IsDeleted)
VALUES (N'DTAS sqlcmd code', N'dtas.sql.code.' + @Stamp + N'@dtas.test', N'sqlcd' + @Stamp, 3, N'x', 1, GETDATE(), 1, 1, N'Verified', N'Active', 0);
SET @JoinID = CAST(SCOPE_IDENTITY() AS INT);

INSERT INTO Events (EventName, Description, EventType, StartDate, EndDate, Venue, Organizer, Status, CreatedBy, CreatedAt, UpdatedAt, Visibility, InviteCode)
VALUES (N'IT-03 code join', N'Test', N'Workshop', DATEADD(DAY, 7, GETDATE()), DATEADD(DAY, 8, GETDATE()), N'Test hall', N'Faculty', N'Planned', @FacultyID, GETDATE(), GETDATE(), N'Public', @Code);
SET @EventID = CAST(SCOPE_IDENTITY() AS INT);

INSERT INTO EventMembers (EventID, UserID, RoleInEvent, JoinedBy, JoinedAt, InviteStatus, IsActive)
VALUES (@EventID, @JoinID, N'Participant', @JoinID, GETDATE(), N'Accepted', 1);

SELECT @Status = InviteStatus FROM EventMembers WHERE EventID = @EventID AND UserID = @JoinID;
IF @Status <> N'Accepted'
BEGIN
    RAISERROR('IT-03 failed: invite code join must be Accepted.', 16, 1);
    RETURN;
END

PRINT 'PASS IT-03 EventMembers invite code is Accepted';

DELETE FROM EventMembers WHERE EventID = @EventID;
DELETE FROM Events WHERE EventID = @EventID;
DELETE FROM IdentityVerificationHistory WHERE UserID IN (@FacultyID, @JoinID);
DELETE FROM IdentityDocuments WHERE UserID IN (@FacultyID, @JoinID);
DELETE FROM Users WHERE UserID IN (@FacultyID, @JoinID);
