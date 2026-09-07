-- ============================================
-- TASK WORKSPACES MODULE ENHANCEMENTS
-- Adds PublicSummary column to Tasks
-- ============================================

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Tasks') AND name = 'PublicSummary')
BEGIN
    ALTER TABLE Tasks ADD PublicSummary NVARCHAR(MAX) NULL;
    PRINT 'PublicSummary column added to Tasks.';
END
ELSE
    PRINT 'PublicSummary column already exists on Tasks.';

PRINT '==========================================';
PRINT 'Task Workspaces module applied successfully.';
PRINT '==========================================';
