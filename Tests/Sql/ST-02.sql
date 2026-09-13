SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
DECLARE @Stamp NVARCHAR(32) = N'st02' + REPLACE(REPLACE(CONVERT(VARCHAR(17), GETDATE(), 113), ' ', ''), ':', '');
DECLARE @StudentID INT, @FacultyID INT, @JoinID INT, @CodeID INT, @EventID INT;
DECLARE @Status NVARCHAR(30), @Req NVARCHAR(30), @Acc NVARCHAR(30), @Code NVARCHAR(20) = N'ST02' + RIGHT(@Stamp, 6);

INSERT INTO Users (FullName, Email, Username, RoleID, Password, IsActive, CreatedAt, EmailVerified, IdentityVerified, VerificationStatus, AccountStatus, IsDeleted)
VALUES (N'ST-02 student', N'dtas.sql.st02s.' + @Stamp + N'@dtas.test', N'st02s' + @Stamp, 3, N'x', 1, GETDATE(), 1, 1, N'Verified', N'Active', 0);
SET @StudentID = CAST(SCOPE_IDENTITY() AS INT);
INSERT INTO Users (FullName, Email, Username, RoleID, Password, IsActive, CreatedAt, EmailVerified, IdentityVerified, VerificationStatus, AccountStatus, IsDeleted)
VALUES (N'ST-02 faculty', N'dtas.sql.st02f.' + @Stamp + N'@dtas.test', N'st02f' + @Stamp, 2, N'x', 1, GETDATE(), 1, 1, N'Verified', N'Active', 0);
SET @FacultyID = CAST(SCOPE_IDENTITY() AS INT);
INSERT INTO Users (FullName, Email, Username, RoleID, Password, IsActive, CreatedAt, EmailVerified, IdentityVerified, VerificationStatus, AccountStatus, IsDeleted)
VALUES (N'ST-02 join', N'dtas.sql.st02j.' + @Stamp + N'@dtas.test', N'st02j' + @Stamp, 3, N'x', 1, GETDATE(), 1, 1, N'Verified', N'Active', 0);
SET @JoinID = CAST(SCOPE_IDENTITY() AS INT);
INSERT INTO Users (FullName, Email, Username, RoleID, Password, IsActive, CreatedAt, EmailVerified, IdentityVerified, VerificationStatus, AccountStatus, IsDeleted)
VALUES (N'ST-02 code', N'dtas.sql.st02c.' + @Stamp + N'@dtas.test', N'st02c' + @Stamp, 3, N'x', 1, GETDATE(), 1, 1, N'Verified', N'Active', 0);
SET @CodeID = CAST(SCOPE_IDENTITY() AS INT);

INSERT INTO Events (EventName, Description, EventType, StartDate, EndDate, Venue, Organizer, Status, CreatedBy, ProposedBy, CreatedAt, UpdatedAt, Visibility, InviteCode)
VALUES (N'ST-02 propose', N'Test', N'Workshop', DATEADD(DAY, 7, GETDATE()), DATEADD(DAY, 8, GETDATE()), N'Hall', N'Faculty', N'Proposed', @StudentID, @StudentID, GETDATE(), GETDATE(), N'Public', @Code);
SET @EventID = CAST(SCOPE_IDENTITY() AS INT);
SELECT @Status = Status FROM Events WHERE EventID = @EventID;
IF @Status <> N'Proposed' BEGIN RAISERROR('ST-02 failed: Status must start Proposed.', 16, 1); RETURN; END

UPDATE Events SET Status = N'Planned' WHERE EventID = @EventID;
INSERT INTO EventMembers (EventID, UserID, RoleInEvent, JoinedBy, JoinedAt, InviteStatus, IsActive)
VALUES (@EventID, @FacultyID, N'EventAdmin', @FacultyID, GETDATE(), N'Accepted', 1);
INSERT INTO EventMembers (EventID, UserID, RoleInEvent, JoinedBy, JoinedAt, InviteStatus, IsActive)
VALUES (@EventID, @JoinID, N'Participant', @JoinID, GETDATE(), N'Requested', 0);
SELECT @Req = InviteStatus FROM EventMembers WHERE EventID = @EventID AND UserID = @JoinID;
UPDATE EventMembers SET InviteStatus = N'Accepted', IsActive = 1 WHERE EventID = @EventID AND UserID = @JoinID;
SELECT @Acc = InviteStatus FROM EventMembers WHERE EventID = @EventID AND UserID = @JoinID;
INSERT INTO EventMembers (EventID, UserID, RoleInEvent, JoinedBy, JoinedAt, InviteStatus, IsActive)
VALUES (@EventID, @CodeID, N'Participant', @CodeID, GETDATE(), N'Accepted', 1);

SELECT @Status = Status FROM Events WHERE EventID = @EventID;
IF @Status <> N'Planned' OR @Req <> N'Requested' OR @Acc <> N'Accepted'
BEGIN RAISERROR('ST-02 failed: Proposed to Planned, public Requested then Accepted.', 16, 1); RETURN; END

PRINT 'PASS ST-02 propose, approve, join';
DELETE FROM EventMembers WHERE EventID = @EventID;
DELETE FROM Events WHERE EventID = @EventID;
DELETE FROM IdentityVerificationHistory WHERE UserID IN (@StudentID, @FacultyID, @JoinID, @CodeID);
DELETE FROM IdentityDocuments WHERE UserID IN (@StudentID, @FacultyID, @JoinID, @CodeID);
DELETE FROM Users WHERE UserID IN (@StudentID, @FacultyID, @JoinID, @CodeID);
