using Mirror;
using UnityEngine;

public class ScoreManager : NetworkBehaviour
{
    public static ScoreManager Instance;

    [SyncVar(hook = nameof(OnBlueScoreChanged))]
    public int blueScore;

    [SyncVar(hook = nameof(OnPurpleScoreChanged))]
    public int purpleScore;

    // UI 연결(나중에)
    public System.Action<int> BlueScoreChanged;
    public System.Action<int> PurpleScoreChanged;

    void Awake() => Instance = this;

    [Server]
    public void AddScore(Team scoredTeam, int amount = 1)
    {
        if (scoredTeam == Team.Blue) blueScore += amount;
        else purpleScore += amount;
    }

    [Server]
    public void ResetScores()
    {
        blueScore = 0;
        purpleScore = 0;
    }

    void OnBlueScoreChanged(int _, int newValue) => BlueScoreChanged?.Invoke(newValue);
    void OnPurpleScoreChanged(int _, int newValue) => PurpleScoreChanged?.Invoke(newValue);
}
