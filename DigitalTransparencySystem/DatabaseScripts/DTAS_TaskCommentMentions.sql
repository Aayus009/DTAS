IF COL_LENGTH('dbo.TaskComments', 'MentionedUserID') IS NULL
    ALTER TABLE dbo.TaskComments ADD MentionedUserID INT NULL;
