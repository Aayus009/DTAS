SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
DECLARE @Stamp NVARCHAR(32) = N'st04' + REPLACE(REPLACE(CONVERT(VARCHAR(17), GETDATE(), 113), ' ', ''), ':', '');
DECLARE @StudentID INT, @JoinID INT, @AdminID INT, @ClubID INT, @Req NVARCHAR(30), @AdminClubs INT;

INSERT INTO Users (FullName, Email, Username, RoleID, Password, IsActive, CreatedAt, EmailVerified, IdentityVerified, VerificationStatus, AccountStatus, IsDeleted)
VALUES (N'ST-04 student', N'dtas.sql.st04s.' + @Stamp + N'@dtas.test', N'st04s' + @Stamp, 3, N'x', 1, GETDATE(), 1, 1, N'Verified', N'Active', 0);
SET @StudentID = CAST(SCOPE_IDENTITY() AS INT);
INSERT INTO Users (FullName, Email, Username, RoleID, Password, IsActive, CreatedAt, EmailVerified, IdentityVerified, VerificationStatus, AccountStatus, IsDeleted)
VALUES (N'ST-04 join', N'dtas.sql.st04j.' + @Stamp + N'@dtas.test', N'st04j' + @Stamp, 3, N'x', 1, GETDATE(), 1, 1, N'Verified', N'Active', 0);
SET @JoinID = CAST(SCOPE_IDENTITY() AS INT);
INSERT INTO Users (FullName, Email, Username, RoleID, Password, IsActive, CreatedAt, EmailVerified, IdentityVerified, VerificationStatus, AccountStatus, IsDeleted)
VALUES (N'ST-04 admin', N'dtas.sql.st04a.' + @Stamp + N'@dtas.test', N'st04a' + @Stamp, 1, N'x', 1, GETDATE(), 1, 1, N'Verified', N'Active', 0);
SET @AdminID = CAST(SCOPE_IDENTITY() AS INT);

INSERT INTO Clubs (ClubName, Description, LeadUserID, IsActive, CreatedBy, CreatedAt, IsPublic, InviteCode, IsDeleted)
VALUES (N'ST-04 club ' + @Stamp, N'Test', @StudentID, 1, @StudentID, GETDATE(), 1, N'ST' + RIGHT(@Stamp, 6), 0);
SET @ClubID = CAST(SCOPE_IDENTITY() AS INT);
INSERT INTO ClubMembers (ClubID, UserID, Role, JoinedAt, InviteStatus, IsActive)
VALUES (@ClubID, @StudentID, N'Lead', GETDATE(), N'Accepted', 1);
INSERT INTO ClubMembers (ClubID, UserID, Role, JoinedAt, InviteStatus, IsActive)
VALUES (@ClubID, @JoinID, N'Member', GETDATE(), N'Requested', 0);

SELECT @Req = InviteStatus FROM ClubMembers WHERE ClubID = @ClubID AND UserID = @JoinID;
SELECT @AdminClubs = COUNT(*) FROM Clubs WHERE CreatedBy = @AdminID;
IF @Req <> N'Requested' OR @AdminClubs <> 0
BEGIN RAISERROR('ST-04 failed: public club join Requested; admin must not create.', 16, 1); RETURN; END

PRINT 'PASS ST-04 club create and public join';
DELETE FROM ClubMembers WHERE ClubID = @ClubID;
DELETE FROM Clubs WHERE ClubID = @ClubID;
DELETE FROM IdentityVerificationHistory WHERE UserID IN (@StudentID, @JoinID, @AdminID);
DELETE FROM IdentityDocuments WHERE UserID IN (@StudentID, @JoinID, @AdminID);
DELETE FROM Users WHERE UserID IN (@StudentID, @JoinID, @AdminID);
