SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
DECLARE @Stamp NVARCHAR(32) = N'it01' + REPLACE(REPLACE(CONVERT(VARCHAR(17), GETDATE(), 113), ' ', ''), ':', '');
DECLARE @Email NVARCHAR(200) = N'dtas.sql.' + @Stamp + N'@dtas.test';
DECLARE @UserID INT;

INSERT INTO Users
    (FullName, Email, Username, RoleID, Password, IsActive, CreatedAt,
     EmailVerified, IdentityVerified, VerificationStatus, AccountStatus, IsDeleted)
VALUES
    (N'DTAS sqlcmd IT-01', @Email, N'sqlit01' + @Stamp, 3, N'x', 1, GETDATE(),
     0, 0, N'None', N'Active', 0);
SET @UserID = CAST(SCOPE_IDENTITY() AS INT);

IF NOT EXISTS (
    SELECT 1 FROM Users
    WHERE UserID = @UserID AND EmailVerified = 0 AND IdentityVerified = 0)
BEGIN
    RAISERROR('IT-01 failed: new user must be gated (EmailVerified=0, IdentityVerified=0).', 16, 1);
    RETURN;
END

PRINT 'PASS IT-01 Auth/Users gate';

DELETE FROM IdentityVerificationHistory WHERE UserID = @UserID;
DELETE FROM IdentityDocuments WHERE UserID = @UserID;
DELETE FROM Users WHERE UserID = @UserID;
