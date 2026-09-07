-- ============================================
-- PHASE 3: CLUBS MODULE (Team Assignment Option A)
-- Clubs carry a standing roster (ClubMembers); when assigning a Task team,
-- Admin can select an existing Club to pre-fill the roster and suggest the
-- Club Lead as Task Leader. Clubs are created/maintained independently in
-- /Admin/Clubs ahead of time.
-- ============================================

-- CLUBS
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Clubs')
BEGIN
    CREATE TABLE Clubs (
        ClubID INT PRIMARY KEY IDENTITY(1,1),
        ClubName NVARCHAR(200) NOT NULL,
        Description NVARCHAR(MAX),
        LeadUserID INT NULL,
        IsActive BIT DEFAULT 1,
        CreatedBy INT,
        CreatedAt DATETIME DEFAULT GETDATE(),
        CONSTRAINT FK_Clubs_Lead FOREIGN KEY (LeadUserID) REFERENCES Users(UserID),
        CONSTRAINT FK_Clubs_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(UserID)
    );
    PRINT 'Clubs table created.';
END
ELSE
    PRINT 'Clubs table already exists.';

-- CLUB MEMBERS (standing roster)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ClubMembers')
BEGIN
    CREATE TABLE ClubMembers (
        MembershipID INT PRIMARY KEY IDENTITY(1,1),
        ClubID INT NOT NULL,
        UserID INT NOT NULL,
        Role NVARCHAR(50) DEFAULT 'Member',
        JoinedAt DATETIME DEFAULT GETDATE(),
        CONSTRAINT FK_CM_Club FOREIGN KEY (ClubID) REFERENCES Clubs(ClubID) ON DELETE CASCADE,
        CONSTRAINT FK_CM_User FOREIGN KEY (UserID) REFERENCES Users(UserID),
        CONSTRAINT UQ_ClubMember UNIQUE (ClubID, UserID)
    );
    PRINT 'ClubMembers table created.';
END
ELSE
    PRINT 'ClubMembers table already exists.';

PRINT '==========================================';
PRINT 'Clubs module applied successfully.';
PRINT '==========================================';
