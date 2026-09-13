SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
DECLARE @Stamp NVARCHAR(32) = N'st01' + REPLACE(REPLACE(CONVERT(VARCHAR(17), GETDATE(), 113), ' ', ''), ':', '');
DECLARE @UserID INT, @Email BIT, @Id BIT;

INSERT INTO Users (FullName, Email, Username, RoleID, Password, IsActive, CreatedAt, EmailVerified, IdentityVerified, VerificationStatus, AccountStatus, IsDeleted)
VALUES (N'DTAS sqlcmd ST-01', N'dtas.sql.st01.' + @Stamp + N'@dtas.test', N'sqlst01' + @Stamp, 3, N'x', 1, GETDATE(), 0, 0, N'None', N'Active', 0);
SET @UserID = CAST(SCOPE_IDENTITY() AS INT);

SELECT @Email = EmailVerified, @Id = IdentityVerified FROM Users WHERE UserID = @UserID;
IF @Email <> 0 OR @Id <> 0
BEGIN RAISERROR('ST-01 failed at start: account must be gated.', 16, 1); RETURN; END

UPDATE Users SET EmailVerified = 1 WHERE UserID = @UserID;
SELECT @Email = EmailVerified, @Id = IdentityVerified FROM Users WHERE UserID = @UserID;
IF @Email <> 1 OR @Id <> 0
BEGIN RAISERROR('ST-01 failed: after email, identity must still be 0.', 16, 1); RETURN; END

UPDATE Users SET IdentityVerified = 1, VerificationStatus = N'Verified' WHERE UserID = @UserID;
SELECT @Email = EmailVerified, @Id = IdentityVerified FROM Users WHERE UserID = @UserID;
IF @Email <> 1 OR @Id <> 1
BEGIN RAISERROR('ST-01 failed: both flags must be true after ID.', 16, 1); RETURN; END

PRINT 'PASS ST-01 identity gate email then ID';
DELETE FROM IdentityVerificationHistory WHERE UserID = @UserID;
DELETE FROM IdentityDocuments WHERE UserID = @UserID;
DELETE FROM Users WHERE UserID = @UserID;
