namespace Network
{
    public enum MatchPhase
    {
        None = 0,
        Deployment = 1,
        Battle = 2,
        Finished = 3
    }

    public enum MatchStatus
    {
        None = 0,
        DeploymentStarted = 1,
        DeploymentSubmitted = 2,
        DeploymentTimerExpired = 3,
        Started = 4,
        Submitted = 5,
        TimerExpired = 6,
        AbilityResolved = 7,
        TurnChanged = 8
    }
}
