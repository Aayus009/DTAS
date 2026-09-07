-- ============================================
-- PHASES 5 & 7: AUTO PROGRESS ROLLUP + EVENT COMPLETION
-- Adds progress columns to Events and Decisions, and a stored
-- procedure that recalculates them automatically from their Tasks
-- (percentage of tasks marked Completed). Triggered whenever a
-- Task's status changes. Event auto-moves to Completed when all
-- of its tasks are Completed.
-- ============================================

-- EVENTS: overall completion percentage
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Events') AND name = 'CompletionPercent')
    ALTER TABLE Events ADD CompletionPercent INT DEFAULT 0;

-- DECISIONS: progress percentage derived from its tasks
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Decisions') AND name = 'ProgressPercent')
    ALTER TABLE Decisions ADD ProgressPercent INT DEFAULT 0;

-- Recalculate completion/progress for the Event (and its Decisions)
-- that contain the given Task, then auto-complete the Event when
-- every task is done. Created via EXEC so no GO batch separator is needed.
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_UpdateProgressForTask')
    DROP PROCEDURE sp_UpdateProgressForTask;

EXEC(N'
CREATE PROCEDURE sp_UpdateProgressForTask
    @TaskID INT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @EventID INT = (SELECT EventID FROM Tasks WHERE TaskID = @TaskID);
    DECLARE @DecisionID INT = (SELECT DecisionID FROM Tasks WHERE TaskID = @TaskID);

    IF @DecisionID IS NOT NULL
    BEGIN
        UPDATE D
        SET ProgressPercent = T.Pct, UpdatedAt = GETDATE()
        FROM Decisions D
        INNER JOIN (
            SELECT
                CAST(SUM(CASE WHEN t.Status = ''Completed'' THEN 1 ELSE 0 END) * 100.0 / NULLIF(COUNT(*), 0) AS INT) AS Pct
            FROM Tasks t
            WHERE t.DecisionID = @DecisionID
        ) T ON 1 = 1
        WHERE D.DecisionID = @DecisionID;
    END

    IF @EventID IS NOT NULL
    BEGIN
        UPDATE E
        SET CompletionPercent = T.Pct,
            Status = CASE WHEN T.Pending = 0 THEN ''Completed'' ELSE E.Status END,
            UpdatedAt = GETDATE()
        FROM Events E
        INNER JOIN (
            SELECT
                CAST(SUM(CASE WHEN t.Status = ''Completed'' THEN 1 ELSE 0 END) * 100.0 / NULLIF(COUNT(*), 0) AS INT) AS Pct,
                SUM(CASE WHEN t.Status <> ''Completed'' THEN 1 ELSE 0 END) AS Pending
            FROM Tasks t
            WHERE t.EventID = @EventID
        ) T ON 1 = 1
        WHERE E.EventID = @EventID;
    END
END
');
