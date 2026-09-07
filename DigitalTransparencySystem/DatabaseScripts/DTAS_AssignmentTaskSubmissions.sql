-- Completed assignment tasks can store a member note and one uploaded file.
IF COL_LENGTH('dbo.AssignmentTasks', 'SubmissionNote') IS NULL
BEGIN
    ALTER TABLE dbo.AssignmentTasks ADD
        SubmissionNote NVARCHAR(2000) NULL,
        SubmissionFilePath NVARCHAR(500) NULL,
        SubmissionFileName NVARCHAR(255) NULL,
        SubmittedAt DATETIME NULL,
        SubmittedBy INT NULL;
    PRINT 'AssignmentTasks submission columns added.';
END
ELSE
    PRINT 'AssignmentTasks submission columns already exist.';

IF COL_LENGTH('dbo.AssignmentTasks', 'SubmittedBy') IS NOT NULL
   AND NOT EXISTS (
        SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_AssignmentTasks_SubmittedBy'
   )
BEGIN
    ALTER TABLE dbo.AssignmentTasks
        ADD CONSTRAINT FK_AssignmentTasks_SubmittedBy
        FOREIGN KEY (SubmittedBy) REFERENCES dbo.Users(UserID);
    PRINT 'FK_AssignmentTasks_SubmittedBy created.';
END
GO
