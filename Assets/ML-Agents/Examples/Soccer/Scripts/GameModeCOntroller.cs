using UnityEngine;
using TMPro;

public class GameModeController : MonoBehaviour
{
    [Header("UI Text")]
    public TMP_Text scoreText;
    public TMP_Text messageText;

    [Header("Panels")]
    public GameObject startPanel;
    public GameObject endPanel;
    public TMP_Text endResultText;

    [Header("Buttons (Host only)")]
    public GameObject startButton;     // Start 버튼 GameObject
    public GameObject restartButton;   // Restart 버튼 GameObject
    public GameObject resetBallButton; // Reset Ball 버튼 GameObject

    void Start()
    {
        // 버튼은 호스트만 보이게
        bool isHost = Mirror.NetworkServer.active; // Host는 true, Client exe는 false
        if (startButton != null) startButton.SetActive(isHost);
        if (restartButton != null) restartButton.SetActive(isHost);
        if (resetBallButton != null) resetBallButton.SetActive(isHost);

        RefreshUI();
    }

    void Update()
    {
        RefreshUI();
    }

    void RefreshUI()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        // 점수 표시: "YOU: X / 3"
        if (scoreText != null)
            scoreText.text = $"YOU: {gm.blueScore} / {gm.winScore}";

        if (gm.state == GameState.Waiting)
        {
            if (startPanel != null) startPanel.SetActive(true);
            if (endPanel != null) endPanel.SetActive(false);
            if (messageText != null) messageText.text = "Start";
        }
        else if (gm.state == GameState.Playing)
        {
            if (startPanel != null) startPanel.SetActive(false);
            if (endPanel != null) endPanel.SetActive(false);
            if (messageText != null) messageText.text = "Beat AI";
        }
        else // GameOver
        {
            if (startPanel != null) startPanel.SetActive(false);
            if (endPanel != null) endPanel.SetActive(true);

            if (endResultText != null) endResultText.text = "YOU WIN!";
            if (messageText != null) messageText.text = "GOAL!!";
        }
    }

    // =========================
    // ✅ 버튼 OnClick 연결용 (호스트 버튼)
    // =========================

    public void OnClickStart()
    {
        if (NetPlayerInput.Local != null)
            NetPlayerInput.Local.UI_Start();
    }

    public void OnClickRestart()
    {
        if (NetPlayerInput.Local != null)
            NetPlayerInput.Local.UI_Restart();
    }

    public void OnClickResetBall()
    {
        if (NetPlayerInput.Local != null)
            NetPlayerInput.Local.UI_ResetBall();
    }
}
