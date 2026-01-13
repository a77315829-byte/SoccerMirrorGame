using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class SoccerEnvController : NetworkBehaviour
{
    [System.Serializable]
    public class PlayerInfo
    {
        public AgentSoccer Agent;
        [HideInInspector] public Rigidbody Rb;
    }

    // 서버에서 접근할 싱글톤
    public static SoccerEnvController ServerInstance { get; private set; }

    [Tooltip("Max Environment Steps")]
    public int MaxEnvironmentSteps = 25000;

    [Header("Scene References")]
    public GameObject ball;
    [HideInInspector] public Rigidbody ballRb;

    public List<PlayerInfo> AgentsList = new();

    Vector3 ballStartPos;
    int resetTimer;

    void Awake()
    {
        // ✅ 순서 꼬임 방지: Awake에서 무조건 잡아둔다
        ServerInstance = this;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("[ENV] OnStartServer -> server init");

        // ball 참조 확보
        if (ball == null)
        {
            var ballCtrl = FindFirstObjectByType<SoccerBallController>();
            if (ballCtrl != null) ball = ballCtrl.gameObject;
        }

        if (ball == null)
        {
            Debug.LogError("[ENV] Ball is NULL. Assign in Inspector or ensure ball exists.");
            return;
        }

        ballRb = ball.GetComponent<Rigidbody>();
        ballStartPos = ballRb.position;

        // Agents 자동 수집(비어있으면)
        if (AgentsList == null) AgentsList = new List<PlayerInfo>();
        if (AgentsList.Count == 0)
        {
            foreach (var a in FindObjectsByType<AgentSoccer>(FindObjectsSortMode.None))
                AgentsList.Add(new PlayerInfo { Agent = a });
        }

        foreach (var p in AgentsList)
            if (p.Agent != null) p.Rb = p.Agent.GetComponent<Rigidbody>();

        ResetScene();
    }

    [ServerCallback]
    void FixedUpdate()
    {
        resetTimer++;
        if (MaxEnvironmentSteps > 0 && resetTimer >= MaxEnvironmentSteps)
        {
            ResetScene();
        }
    }

    [Server]
    public void ServerGoalTouched(Team scoredTeam)
    {
        Debug.Log($"[GOAL] team={scoredTeam}");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ServerAddScore(scoredTeam);
        }
    }

    [Server]
    public void ResetScene()
    {
        resetTimer = 0;

        foreach (var p in AgentsList)
        {
            if (p.Agent == null || p.Rb == null) continue;

            var randomPosX = Random.Range(-3.5f, 3.5f);
            var newStartPos = p.Agent.initialPos + new Vector3(randomPosX, 0f, 0f);
            var rot = p.Agent.rotSign * Random.Range(80f, 100f);
            var newRot = Quaternion.Euler(0f, rot, 0f);

            p.Rb.linearVelocity = Vector3.zero;
            p.Rb.angularVelocity = Vector3.zero;
            p.Rb.position = newStartPos;
            p.Rb.rotation = newRot;
            p.Rb.Sleep();
        }

        ResetBall();
    }

    [Server]
    public void ResetBall()
    {
        if (ballRb == null) return;

        var rx = Random.Range(-2.5f, 2.5f);
        var rz = Random.Range(-2.5f, 2.5f);

        // 1) 속도 먼저 0
        ballRb.linearVelocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;

        // 2) 위치/회전 강제 이동 (MovePosition도 가능)
        ballRb.position = ballStartPos + new Vector3(rx, 0f, rz);
        ballRb.rotation = Quaternion.identity;

        // 3) Sleep() 제거 (체감 지연/전파 지연 유발 가능)
        // ballRb.Sleep();

        // 4) (선택) 잠깐 튕김 방지용으로 한 프레임 강제 깨우기
        ballRb.WakeUp();

        Debug.Log("[RESET] Ball reset");
    }

}
