-- =============================================================
-- Zoom Meetings Module
-- Adds Zoom integration columns to the Meetings table and a
-- MeetingRecordings table to store cloud recording links.
-- Run this against DigitalTransparencyDB.
-- =============================================================

-- ZOOM COLDATA ON MEETINGS
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Meetings') AND name = 'ZoomMeetingId')
    ALTER TABLE Meetings ADD ZoomMeetingId BIGINT NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Meetings') AND name = 'ZoomJoinUrl')
    ALTER TABLE Meetings ADD ZoomJoinUrl NVARCHAR(500) NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Meetings') AND name = 'ZoomStartUrl')
    ALTER TABLE Meetings ADD ZoomStartUrl NVARCHAR(500) NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Meetings') AND name = 'ZoomPasscode')
    ALTER TABLE Meetings ADD ZoomPasscode NVARCHAR(50) NULL;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Meetings') AND name = 'ZoomHostId')
    ALTER TABLE Meetings ADD ZoomHostId NVARCHAR(100) NULL;
GO

-- MEETING RECORDINGS
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MeetingRecordings')
BEGIN
    CREATE TABLE MeetingRecordings (
        RecordingID INT PRIMARY KEY IDENTITY(1,1),
        MeetingID INT NOT NULL,
        RecordingType NVARCHAR(50) DEFAULT 'Zoom',
        Title NVARCHAR(300) NULL,
        PlayUrl NVARCHAR(1000) NOT NULL,
        DownloadUrl NVARCHAR(1000) NULL,
        FileSize BIGINT NULL,
        DurationSeconds INT NULL,
        RecordedAt DATETIME NULL,
        CreatedAt DATETIME DEFAULT GETDATE(),
        CONSTRAINT FK_MR_Meeting FOREIGN KEY (MeetingID) REFERENCES Meetings(MeetingID) ON DELETE CASCADE
    );
END;
GO
