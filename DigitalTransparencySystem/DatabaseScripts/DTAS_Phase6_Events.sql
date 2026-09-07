-- DTAS PHASE 6: event invite role + notes
-- Additive only. Safe to re-run.

SET NOCOUNT ON;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.EventInvitations') AND name = 'InviteRole')
    ALTER TABLE dbo.EventInvitations ADD InviteRole NVARCHAR(50) NOT NULL
        CONSTRAINT DF_EventInvitations_InviteRole DEFAULT N'Participant';
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'EventNotes')
BEGIN
    CREATE TABLE dbo.EventNotes (
        NoteID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        EventID INT NOT NULL,
        UserID INT NOT NULL,
        NoteText NVARCHAR(MAX) NOT NULL,
        IsPinned BIT NOT NULL CONSTRAINT DF_EventNotes_IsPinned DEFAULT 0,
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_EventNotes_CreatedAt DEFAULT GETDATE(),
        CONSTRAINT FK_EventNotes_Event FOREIGN KEY (EventID) REFERENCES dbo.Events(EventID),
        CONSTRAINT FK_EventNotes_User FOREIGN KEY (UserID) REFERENCES dbo.Users(UserID)
    );
    CREATE INDEX IX_EventNotes_Event ON dbo.EventNotes(EventID, IsPinned DESC, CreatedAt DESC);
    PRINT 'EventNotes table created.';
END
ELSE
    PRINT 'EventNotes table already exists.';
GO
