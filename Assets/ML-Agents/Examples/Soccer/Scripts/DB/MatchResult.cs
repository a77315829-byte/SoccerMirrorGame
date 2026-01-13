using System;

[Serializable]
public class MatchResult
{
    public DateTime StartedAt;
    public DateTime EndedAt;
    public int PlayerGoals;
    public int TargetGoals;
    public bool Cleared;
}
