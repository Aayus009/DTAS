-- ============================================
-- PHASE 1b: EVENT PROPOSAL MODULE
-- Adds the proposal path to Events:
--   - ProposedBy (FK Users): who proposed the event
--   - RejectionReason: recorded when Admin rejects a proposal
--   - Event statuses: Proposed -> (Approve -> Planned | Reject -> Rejected)
-- Rejected events stay in the system as a record (not deleted).
-- ============================================

-- Add ProposedBy column to Events
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Events') AND name = 'ProposedBy')
BEGIN
    ALTER TABLE Events ADD ProposedBy INT NULL;
    ALTER TABLE Events ADD CONSTRAINT FK_Events_ProposedBy FOREIGN KEY (ProposedBy) REFERENCES Users(UserID);
    PRINT 'ProposedBy column added to Events.';
END
ELSE
    PRINT 'ProposedBy column already exists.';

-- Add RejectionReason column to Events
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Events') AND name = 'RejectionReason')
BEGIN
    ALTER TABLE Events ADD RejectionReason NVARCHAR(MAX) NULL;
    PRINT 'RejectionReason column added to Events.';
END
ELSE
    PRINT 'RejectionReason column already exists.';

-- Add ProposeEvent type to Notifications (no schema change needed - NotificationType is NVARCHAR)

PRINT '==========================================';
PRINT 'Event Proposal module applied successfully.';
PRINT '==========================================';
