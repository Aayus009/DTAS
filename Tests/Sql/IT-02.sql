SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
DECLARE @Stamp NVARCHAR(32) = N'it02' + REPLACE(REPLACE(CONVERT(VARCHAR(17), GETDATE(), 113), ' ', ''), ':', '');
DECLARE @FacultyEmail NVARCHAR(200) = N'dtas.sql.fac.' + @Stamp + N'@dtas.test';
DECLARE @JoinEmail NVARCHAR(200) = N'dtas.sql.join.' + @Stamp + N'@dtas.test';
DECLARE @FacultyID INT, @JoinID INT, @EventID INT, @Status NVARCHAR(30);

INSERT INTO Users (FullName, Email, Username, RoleID, Password, IsActive, CreatedAt, EmailVerified, IdentityVerified, VerificationStatus, AccountStatus, IsDeleted)
VALUES (N'DTAS sqlcmd faculty', @FacultyEmail, N'sqlfac' + @Stamp, 2, N'x', 1, GETDATE(), 1, 1, N'Verified', N'Active', 0);
SET @FacultyID = CAST(SCOPE_IDENTITY() AS INT);

INSERT INTO Users (FullName, Email, Username, RoleID, Password, IsActive, CreatedAt, EmailVerified, IdentityVerified, VerificationStatus, AccountStatus, IsDeleted)
VALUES (N'DTAS sqlcmd joiner', @JoinEmail, N'sqljn' + @Stamp, 3, N'x', 1, GETDATE(), 1, 1, N'Verified', N'Active', 0);
SET @JoinID = CAST(SCOPE_IDENTITY() AS INT);

INSERT INTO Events (EventName, Description, EventType, StartDate, EndDate, Venue, Organizer, Status, CreatedBy, CreatedAt, UpdatedAt, Visibility)
VALUES (N'IT-02 public join', N'Test', N'Workshop', DATEADD(DAY, 7, GETDATE()), DATEADD(DAY, 8, GETDATE()), N'Test hall', N'Faculty', N'Planned', @FacultyID, GETDATE(), GETDATE(), N'Public');
SET @EventID = CAST(SCOPE_IDENTITY() AS INT);

INSERT INTO EventMembers (EventID, UserID, RoleInEvent, JoinedBy, JoinedAt, InviteStatus, IsActive)
VALUES (@EventID, @FacultyID, N'EventAdmin', @FacultyID, GETDATE(), N'Accepted', 1);

INSERT INTO EventMembers (EventID, UserID, RoleInEvent, JoinedBy, JoinedAt, InviteStatus, IsActive)
VALUES (@EventID, @JoinID, N'Participant', @JoinID, GETDATE(), N'Requested', 0);

SELECT @Status = InviteStatus FROM EventMembers WHERE EventID = @EventID AND UserID = @JoinID;
IF @Status <> N'Requested'
BEGIN
    RAISERROR('IT-02 failed: public join must store InviteStatus=Requested.', 16, 1);
    RETURN;
END

PRINT 'PASS IT-02 EventMembers public join is Requested';

DELETE FROM EventMembers WHERE EventID = @EventID;
DELETE FROM Events WHERE EventID = @EventID;
DELETE FROM IdentityVerificationHistory WHERE UserID IN (@FacultyID, @JoinID);
DELETE FROM IdentityDocuments WHERE UserID IN (@FacultyID, @JoinID);
DELETE FROM Users WHERE UserID IN (@FacultyID, @JoinID);
