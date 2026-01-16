using Mirror;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BlueAgentController : NetworkBehaviour
{
    Rigidbody rb;

    [Header("Tuning")]
    public float moveSpeed = 4.5f;      // m/s 정도
    public float rotateSpeed = 200f;    // deg/s

    [Header("Dash")]
    public float dashMultiplier = 2.2f;     // 순간 가속 배수
    public float dashDuration = 0.15f;      // 대시 지속시간(초)
    public float dashCooldown = 1.0f;       // 쿨타임

    private double nextDashServerTime = 0;  // Mirror 서버 시간 기준
    private double dashEndServerTime = 0;   // Mirror 서버 시간 기준
    private bool isDashing = false;

    float inputX, inputZ, inputRot;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    [Server]
    public void TryDash()
    {
        double now = NetworkTime.time;

        if (now < nextDashServerTime) return;

        nextDashServerTime = now + dashCooldown;
        dashEndServerTime = now + dashDuration;
        isDashing = true;
    }

    [Server]
    public void SetInput(float moveX, float moveZ, float rotate)
    {
        inputX = moveX;
        inputZ = moveZ;
        inputRot = rotate;
    }

    void FixedUpdate()
    {
        if (!isServer) return;

        // ✅ 대시 시간 끝났는지 갱신
        if (isDashing && NetworkTime.time >= dashEndServerTime)
            isDashing = false;

        // 1) 회전
        if (Mathf.Abs(inputRot) > 0.01f)
        {
            rb.MoveRotation(
                rb.rotation * Quaternion.Euler(0f, inputRot * rotateSpeed * Time.fixedDeltaTime, 0f)
            );
        }

        // 2) 이동
        Vector3 localMove = (transform.right * inputX + transform.forward * inputZ);

        if (localMove.sqrMagnitude > 0.0001f)
        {
            localMove = localMove.normalized;

            float speed = moveSpeed * (isDashing ? dashMultiplier : 1f);

            Vector3 targetVel = localMove * speed;
            Vector3 vel = rb.linearVelocity;
            rb.linearVelocity = new Vector3(targetVel.x, vel.y, targetVel.z);
        }
        else
        {
            Vector3 vel = rb.linearVelocity;
            rb.linearVelocity = new Vector3(0f, vel.y, 0f);
        }
    }

}