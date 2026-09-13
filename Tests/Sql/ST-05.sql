SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
DECLARE @Stamp NVARCHAR(32) = N'st05' + REPLACE(REPLACE(CONVERT(VARCHAR(17), GETDATE(), 113), ' ', ''), ':', '');
DECLARE @Admin1 INT, @Admin2 INT, @Bad INT;

INSERT INTO Users (FullName, Email, Username, RoleID, Password, IsActive, CreatedAt, EmailVerified, IdentityVerified, VerificationStatus, AccountStatus, IsDeleted)
VALUES (N'ST-05 admin a', N'dtas.sql.st05a.' + @Stamp + N'@dtas.test', N'st05a' + @Stamp, 1, N'x', 1, GETDATE(), 1, 1, N'Verified', N'Active', 0);
SET @Admin1 = CAST(SCOPE_IDENTITY() AS INT);
INSERT INTO Users (FullName, Email, Username, RoleID, Password, IsActive, CreatedAt, EmailVerified, IdentityVerified, VerificationStatus, AccountStatus, IsDeleted)
VALUES (N'ST-05 admin b', N'dtas.sql.st05b.' + @Stamp + N'@dtas.test', N'st05b' + @Stamp, 1, N'x', 1, GETDATE(), 1, 1, N'Verified', N'Active', 0);
SET @Admin2 = CAST(SCOPE_IDENTITY() AS INT);

SELECT @Bad = COUNT(*) FROM Users
WHERE UserID IN (@Admin1, @Admin2) AND (RoleID <> 1 OR ISNULL(AccountStatus, N'Active') <> N'Active');
IF @Bad <> 0
BEGIN RAISERROR('ST-05 failed: admin accounts must stay Active; admin is not moderated.', 16, 1); RETURN; END

PRINT 'PASS ST-05 admin AccountStatus stays Active';
DELETE FROM IdentityVerificationHistory WHERE UserID IN (@Admin1, @Admin2);
DELETE FROM IdentityDocuments WHERE UserID IN (@Admin1, @Admin2);
DELETE FROM Users WHERE UserID IN (@Admin1, @Admin2);
