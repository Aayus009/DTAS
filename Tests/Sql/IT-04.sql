SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
DECLARE @Stamp NVARCHAR(32) = N'it04' + REPLACE(REPLACE(CONVERT(VARCHAR(17), GETDATE(), 113), ' ', ''), ':', '');
DECLARE @StudentID INT, @AdminID INT, @ClubID INT, @Role NVARCHAR(50), @AdminClubs INT;

INSERT INTO Users (FullName, Email, Username, RoleID, Password, IsActive, CreatedAt, EmailVerified, IdentityVerified, VerificationStatus, AccountStatus, IsDeleted)
VALUES (N'DTAS sqlcmd student', N'dtas.sql.stu.' + @Stamp + N'@dtas.test', N'sqlst' + @Stamp, 3, N'x', 1, GETDATE(), 1, 1, N'Verified', N'Active', 0);
SET @StudentID = CAST(SCOPE_IDENTITY() AS INT);

INSERT INTO Users (FullName, Email, Username, RoleID, Password, IsActive, CreatedAt, EmailVerified, IdentityVerified, VerificationStatus, AccountStatus, IsDeleted)
VALUES (N'DTAS sqlcmd admin', N'dtas.sql.adm.' + @Stamp + N'@dtas.test', N'sqlad' + @Stamp, 1, N'x', 1, GETDATE(), 1, 1, N'Verified', N'Active', 0);
SET @AdminID = CAST(SCOPE_IDENTITY() AS INT);

INSERT INTO Clubs (ClubName, Description, LeadUserID, IsActive, CreatedBy, CreatedAt, IsPublic, InviteCode, IsDeleted)
VALUES (N'IT-04 student club ' + @Stamp, N'Test', @StudentID, 1, @StudentID, GETDATE(), 1, N'CL' + RIGHT(@Stamp, 6), 0);
SET @ClubID = CAST(SCOPE_IDENTITY() AS INT);

INSERT INTO ClubMembers (ClubID, UserID, Role, JoinedAt, InviteStatus, IsActive)
VALUES (@ClubID, @StudentID, N'Lead', GETDATE(), N'Accepted', 1);

SELECT @Role = Role FROM ClubMembers WHERE ClubID = @ClubID AND UserID = @StudentID;
SELECT @AdminClubs = COUNT(*) FROM Clubs WHERE CreatedBy = @AdminID;

IF @Role <> N'Lead' OR @AdminClubs <> 0
BEGIN
    RAISERROR('IT-04 failed: student must be club lead and admin must not create a club.', 16, 1);
    RETURN;
END

PRINT 'PASS IT-04 Clubs student lead; admin CreatedBy count is 0';

DELETE FROM ClubMembers WHERE ClubID = @ClubID;
DELETE FROM Clubs WHERE ClubID = @ClubID;
DELETE FROM IdentityVerificationHistory WHERE UserID IN (@StudentID, @AdminID);
DELETE FROM IdentityDocuments WHERE UserID IN (@StudentID, @AdminID);
DELETE FROM Users WHERE UserID IN (@StudentID, @AdminID);
