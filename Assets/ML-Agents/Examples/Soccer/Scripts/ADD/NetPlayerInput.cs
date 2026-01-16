using Mirror;
using System.Collections;
using Unity.Cinemachine;   // ✅ Cinemachine 3
using UnityEngine;

public class NetPlayerInput : NetworkBehaviour
{
    public static NetPlayerInput Local;

    private BlueAgentController myAgent;

    [HideInInspector] public float joyX;
    [HideInInspector] public float joyZ;
    [HideInInspector] public float joyRotate;


    // ✅ 서버가 할당한 striker netId를 클라로 Sync
    [SyncVar(hook = nameof(OnControlledNetIdChanged))]
    private uint controlledNetId;

    [Header("Cinemachine (Scene)")]
    public CinemachineCamera overviewCam; // 씬의 OverviewCam
    public CinemachineCamera followCam;   // 씬의 FollowCam

    [Header("Camera Priority")]
    public int overviewPriority = 20;
    public int followPriorityOff = 10;
    public int followPriorityOn = 30;

    private bool followMode = false;
    private bool targetReady = false;

    private Coroutine attachRoutine;

    // =========================
    // ✅ Mobile Dual Joystick
    // =========================
    [Header("Mobile Joystick (Scene)")]
    public SimpleJoystick leftJoy;   // 이동
    public SimpleJoystick rightJoy;  // 회전
    public float mobileRotateScale = 1.5f;

    [Tooltip("에디터에서 모바일 입력 테스트용(실기기에서는 꺼도 됨)")]
    public bool forceMobileInput = false;

    [Header("Mobile Axis Mapping")]
    [Tooltip("좌스틱 X를 moveX로 쓸지 (기본은 false: 공격방향=Y, 좌우=X 형태 조이스틱을 가정)")]
    public bool leftXIsMoveX = false;

    [Tooltip("좌스틱 Y를 moveZ로 쓸지 (기본은 false: moveZ=좌스틱X)")]
    public bool leftYIsMoveZ = false;

    [Tooltip("좌우 반전이 필요하면 체크")]
    public bool invertMoveX = false;
    public bool invertMoveZ = false;
    public bool invertRotate = false;

    [Header("Dash")]
    public float dashCooldown = 1f;
    private float nextDashLocalTime = 0f; // 입력 난사 방지용(로컬)


    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        Local = this;

        // ✅ (카메라) 프리팹에서 씬 오브젝트 드래그가 어려우니 자동 탐색
        if (overviewCam == null || followCam == null)
        {
            var cams = FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);
            foreach (var c in cams)
            {
                if (overviewCam == null && c.name.Contains("Overview")) overviewCam = c;
                if (followCam == null && c.name.Contains("Follow")) followCam = c;
            }
        }

        // ✅ (조이스틱) 씬에서 자동 탐색
        EnsureJoysticks();

        // 기본은 Overview
        SetCameraMode(false);
        targetReady = false;

        // Host에서 SyncVar가 이미 들어왔을 수 있어 1회 더 시도
        if (controlledNetId != 0)
            OnControlledNetIdChanged(0, controlledNetId);
    }

    public override void OnStartServer()
    {
        myAgent = BlueAgentManager.Instance.AssignAgent();
        if (myAgent == null) return;

        var ml = myAgent.GetComponent<AgentSoccer>();
        if (ml != null) ml.enabled = false;

        // ✅ 내 조종 대상 netId Sync
        var ni = myAgent.GetComponent<NetworkIdentity>();
        if (ni != null) controlledNetId = ni.netId;
    }

    private void OnControlledNetIdChanged(uint oldId, uint newId)
    {
        if (!isLocalPlayer) return;
        if (newId == 0) return;

        targetReady = false; // ✅ 추가 (새 타겟을 다시 붙이는 중)

        if (attachRoutine != null) StopCoroutine(attachRoutine);
        attachRoutine = StartCoroutine(AttachTargetWhenReady(newId));
    }


    private IEnumerator AttachTargetWhenReady(uint netId)
    {
        int guard = 300; // 약 5초

        while (guard-- > 0)
        {
            if (NetworkClient.spawned.TryGetValue(netId, out var idObj) && idObj != null)
            {
                var target = idObj.transform;

                // ✅ (YOU 네임태그) 내 striker에만 켜기
                var nameTag = target.GetComponentInChildren<PlayerNameTag>(true);
                if (nameTag != null) nameTag.SetYou(true);

                // ✅ (카메라 타겟 연결)
                if (followCam != null)
                {
                    var camTarget = followCam.Target;
                    camTarget.TrackingTarget = target;
                    camTarget.LookAtTarget = target;
                    camTarget.CustomLookAtTarget = true;
                    followCam.Target = camTarget;

                    // ✅ 추가: 카메라 초기 배치 (튐 방지)
                    followCam.transform.position = target.position + (-target.forward * 5f) + (Vector3.up * 2f);
                    followCam.transform.rotation = Quaternion.LookRotation(
                        (target.position + Vector3.up * 1.5f) - followCam.transform.position
                    );

                    // (선택) 초기화 트리거
                    followCam.enabled = false;
                    yield return null;
                    followCam.enabled = true;

                    targetReady = true;
                    SetCameraMode(followMode);
                }

                yield break;
            }

            yield return null;
        }

        Debug.LogWarning($"[NetPlayerInput] TrackingTarget not found. netId={netId}");
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        // ✅ PC: V 토글, 모바일은 버튼(UI_ToggleCamera)로
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetKeyDown(KeyCode.V))
        {
            UI_ToggleCamera();
        }
#endif

        // ✅ 대기/게임오버면 입력 잠금
        if (GameManager.Instance != null && GameManager.Instance.state != GameState.Playing)
            return;

        float moveX = 0f;
        float moveZ = 0f;
        float rotate = 0f;

        bool useMobile = forceMobileInput || Application.isMobilePlatform;

        if (useMobile)
        {
            EnsureJoysticks();

            if (leftJoy != null)
            {
                // leftJoy.Value: (-1..1)
                float lx = Mathf.Clamp(leftJoy.Value.x, -1f, 1f);
                float ly = Mathf.Clamp(leftJoy.Value.y, -1f, 1f);

                // ✅ 기본 매핑(권장):
                // - 많은 조이스틱 UI는 X=좌우, Y=위아래
                // - 너 게임은 "공격 방향이 X", "좌우가 Z" 라고 했으니
                //   기본값을: moveX = ly(앞/뒤), moveZ = lx(좌/우) 로 둠.
                //   (이게 보통 “앞으로 밀면 전진” 감각과 일치)
                if (!leftXIsMoveX && !leftYIsMoveZ)
                {
                    moveX = lx; // 전진/후진 -> X
                    moveZ = ly; // 좌/우     -> Z
                }
                else
                {
                    // 필요하면 옵션으로 뒤집기
                    moveX = leftXIsMoveX ? lx : ly;
                    moveZ = leftYIsMoveZ ? ly : lx;
                }

                if (invertMoveX) moveX = -moveX;
                if (invertMoveZ) moveZ = -moveZ;
            }

            if (rightJoy != null)
            {
                // 오른쪽 가로축만 회전으로 사용
                rotate = Mathf.Clamp(rightJoy.Value.x, -1f, 1f) * mobileRotateScale;
                if (invertRotate) rotate = -rotate;
            }
        }
        else
        {
            // ✅ PC 키보드
            if (Input.GetKey(KeyCode.A)) moveX -= 1f;
            if (Input.GetKey(KeyCode.D)) moveX += 1f;
            if (Input.GetKey(KeyCode.W)) moveZ += 1f;
            if (Input.GetKey(KeyCode.S)) moveZ -= 1f;

            if (Input.GetKey(KeyCode.Q)) rotate -= 1f;
            if (Input.GetKey(KeyCode.E)) rotate += 1f;
        }

        CmdSendInput(moveX, moveZ, rotate);

#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (Time.time >= nextDashLocalTime)
            {
                nextDashLocalTime = Time.time + dashCooldown;
                CmdDash();
            }
        }
#endif

    }

    [Command]
    private void CmdDash()
    {
        if (myAgent == null) return;
        myAgent.TryDash(); // 서버에서 쿨타임 최종 판정
    }


    private void EnsureJoysticks()
    {
        if (leftJoy != null && rightJoy != null) return;

        var joys = FindObjectsByType<SimpleJoystick>(FindObjectsSortMode.None);
        foreach (var j in joys)
        {
            var n = j.name.ToLower();
            if (leftJoy == null && (n.Contains("left") || n.Contains("ljoy"))) leftJoy = j;
            else if (rightJoy == null && (n.Contains("right") || n.Contains("rjoy"))) rightJoy = j;
        }
    }

    private void SetCameraMode(bool follow)
    {
        if (overviewCam != null)
            overviewCam.Priority = overviewPriority;

        if (followCam != null)
            followCam.Priority = follow ? followPriorityOn : followPriorityOff;
    }

    // =========================
    // ✅ UI 버튼에서 카메라 토글 호출
    // =========================
    public void UI_ToggleCamera()
    {
        if (!isLocalPlayer) return;

        followMode = !followMode;

        // ✅ 타겟 준비 전이면 follow로 못 넘어가게 막기
        if (followMode && !targetReady)
        {
            followMode = false;
            SetCameraMode(false);
            Debug.Log("[NetPlayerInput] Follow target not ready yet.");
            return;
        }

        SetCameraMode(followMode);
    }


    [Command]
    void CmdSendInput(float moveX, float moveZ, float rotate)
    {
        if (myAgent == null) return;
        myAgent.SetInput(moveX, moveZ, rotate);
    }

    // =========================
    // ✅ UI 버튼 (Host만 실행되게 방어)
    // =========================
    public void UI_Start()
    {
        if (!isLocalPlayer || !isServer) return; // Host만
        CmdStartMatch();
    }

    public void UI_Restart()
    {
        if (!isLocalPlayer || !isServer) return; // Host만
        CmdRestartMatch();
    }

    public void UI_ResetBall()
    {
        if (!isLocalPlayer || !isServer) return; // Host만
        CmdResetBall();
    }

    [Command] void CmdStartMatch() => GameManager.Instance.ServerStartMatch();
    [Command] void CmdRestartMatch() => GameManager.Instance.ServerRestartMatch();
    [Command] void CmdResetBall() => GameManager.Instance.ServerResetBallOnly();
}