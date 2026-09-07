-- Multiple files per completed assignment task.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AssignmentTaskFiles')
BEGIN
    CREATE TABLE dbo.AssignmentTaskFiles (
        FileID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        TaskID INT NOT NULL,
        FilePath NVARCHAR(500) NOT NULL,
        FileName NVARCHAR(255) NOT NULL,
        UploadedBy INT NULL,
        UploadedAt DATETIME NOT NULL CONSTRAINT DF_AssignmentTaskFiles_UploadedAt DEFAULT GETDATE(),
        CONSTRAINT FK_AssignmentTaskFiles_Task FOREIGN KEY (TaskID) REFERENCES dbo.AssignmentTasks(TaskID),
        CONSTRAINT FK_AssignmentTaskFiles_User FOREIGN KEY (UploadedBy) REFERENCES dbo.Users(UserID)
    );
    CREATE INDEX IX_AssignmentTaskFiles_Task ON dbo.AssignmentTaskFiles(TaskID);
    PRINT 'AssignmentTaskFiles table created.';
END
ELSE
    PRINT 'AssignmentTaskFiles table already exists.';

IF COL_LENGTH('dbo.AssignmentTasks', 'SubmissionFilePath') IS NOT NULL
BEGIN
    INSERT INTO dbo.AssignmentTaskFiles (TaskID, FilePath, FileName, UploadedBy, UploadedAt)
    SELECT t.TaskID, t.SubmissionFilePath, ISNULL(t.SubmissionFileName, N'submission'), t.SubmittedBy, ISNULL(t.SubmittedAt, GETDATE())
    FROM dbo.AssignmentTasks t
    WHERE t.SubmissionFilePath IS NOT NULL
      AND LTRIM(RTRIM(t.SubmissionFilePath)) <> N''
      AND NOT EXISTS (
          SELECT 1 FROM dbo.AssignmentTaskFiles f
          WHERE f.TaskID = t.TaskID AND f.FilePath = t.SubmissionFilePath
      );
    PRINT 'Existing single-file submissions copied into AssignmentTaskFiles.';
END
GO
