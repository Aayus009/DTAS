namespace DigitalTransparencySystem.Helpers
{
    public static class TaskAccess
    {
        public const string UserCanSeeTask = @"
            (
                t.LeaderID = @UserID
                OR t.TaskID IN (SELECT TaskID FROM TaskAssignments WHERE UserID = @UserID)
                OR t.TaskID IN (SELECT tt.TaskID FROM TaskTeams tt WHERE tt.LeaderID = @UserID)
                OR t.TaskID IN (
                    SELECT tt.TaskID FROM TaskTeams tt
                    INNER JOIN TaskTeamMembers ttm ON tt.TeamID = ttm.TeamID
                    WHERE ttm.UserID = @UserID AND ISNULL(ttm.Status, '') <> 'Declined'
                )
                OR (
                    t.EventID IS NOT NULL
                    AND EXISTS (
                        SELECT 1 FROM EventMembers em
                        WHERE em.EventID = t.EventID
                          AND em.UserID = @UserID
                          AND em.IsActive = 1
                          AND em.InviteStatus = N'Accepted'
                          AND em.RoleInEvent IN (N'EventAdmin', N'EventManager')
                    )
                )
            )";
    }
}
