-- External links (GitHub, Discord, and other https URLs) on completed tasks.
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AssignmentTaskLinks')
BEGIN
    CREATE TABLE dbo.AssignmentTaskLinks (
        LinkID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        TaskID INT NOT NULL,
        Url NVARCHAR(2000) NOT NULL,
        AddedBy INT NULL,
        AddedAt DATETIME NOT NULL CONSTRAINT DF_AssignmentTaskLinks_AddedAt DEFAULT GETDATE(),
        CONSTRAINT FK_AssignmentTaskLinks_Task FOREIGN KEY (TaskID) REFERENCES dbo.AssignmentTasks(TaskID),
        CONSTRAINT FK_AssignmentTaskLinks_User FOREIGN KEY (AddedBy) REFERENCES dbo.Users(UserID)
    );
    CREATE INDEX IX_AssignmentTaskLinks_Task ON dbo.AssignmentTaskLinks(TaskID);
    PRINT 'AssignmentTaskLinks table created.';
END
ELSE
    PRINT 'AssignmentTaskLinks table already exists.';
GO
