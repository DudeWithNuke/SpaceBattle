namespace Network.Match
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
        DeploymentTimerExpired = 3,
        Started = 4,
        Submitted = 5,
        TimerExpired = 6,
        TurnChanged = 8
    }
}
