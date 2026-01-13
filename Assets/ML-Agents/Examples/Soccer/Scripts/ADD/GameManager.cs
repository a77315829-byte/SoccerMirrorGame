using Mirror;
using UnityEngine;

public enum GameState
{
    Waiting,
    Playing,
    GameOver
}

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [Header("Rules")]
    public int winScore = 3;

    [Header("State (Synced)")]
    [SyncVar(hook = nameof(OnStateChanged))]
    public GameState state = GameState.Waiting;

    [SyncVar(hook = nameof(OnScoreChanged))]
    public int blueScore;

    [SyncVar(hook = nameof(OnScoreChanged))]
    public int purpleScore;

    void Awake() => Instance = this;

    public override void OnStartServer()
    {
        base.OnStartServer();
        // 시작은 "대기"로 두고, Start 버튼(호스트) 누르면 시작
        state = GameState.Waiting;
        blueScore = 0;
        purpleScore = 0;
    }

    // =========================
    // 서버에서만 실행되는 API
    // =========================

    [Server]
    public void ServerStartMatch()
    {
        if (state == GameState.Playing) return;

        blueScore = 0;
        purpleScore = 0;
        state = GameState.Playing;

        SoccerEnvController.ServerInstance.ResetScene();
        Debug.Log("[GAME] ServerStartMatch");
    }

    [Server]
    public void ServerRestartMatch()
    {
        // 게임오버 상태에서 재시작 버튼을 누르면 다시 플레이로
        blueScore = 0;
        purpleScore = 0;
        state = GameState.Playing;

        SoccerEnvController.ServerInstance.ResetScene();
        Debug.Log("[GAME] ServerRestartMatch");
    }

    [Server]
    public void ServerResetBallOnly()
    {
        // 공만 중앙으로
        SoccerEnvController.ServerInstance.ResetBall();
        Debug.Log("[GAME] ServerResetBallOnly");
    }

    // 골 들어갔을 때 서버에서 호출
    [Server]
    public void ServerAddScore(Team scoredTeam)
    {
        if (state != GameState.Playing) return;

        if (scoredTeam == Team.Blue) blueScore++;
        else purpleScore++;

        Debug.Log($"[GAME] Score => Blue:{blueScore} Purple:{purpleScore}");

        // 당신 요구사항: "우리(블루) 3골"만 게임오버
        if (scoredTeam == Team.Blue && blueScore >= winScore)
        {
            state = GameState.GameOver;
            Debug.Log("[GAME] GameOver (Blue Win)");
            return;
        }

        // 퍼플이 넣었거나, 블루가 아직 3 미만이면 바로 리셋만
        SoccerEnvController.ServerInstance.ResetScene();
    }

    // =========================
    // SyncVar hooks (클라 UI 갱신 트리거)
    // =========================

    void OnStateChanged(GameState oldState, GameState newState)
    {
        // UI는 GameModeController가 GameManager.Instance를 보고 갱신하는 구조라
        // 여기서는 로그만. (원하면 여기서 UI 호출도 가능)
        Debug.Log($"[GAME] State {oldState} -> {newState}");
    }

    void OnScoreChanged(int oldValue, int newValue)
    {
        // 점수 변경 hook (로그 정도만)
    }
}
