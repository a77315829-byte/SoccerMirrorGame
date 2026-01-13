using Mirror;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BlueAgentController : NetworkBehaviour
{
    Rigidbody rb;

    [Header("Tuning")]
    public float moveSpeed = 4.5f;      // m/s 정도
    public float rotateSpeed = 200f;    // deg/s

    float inputX, inputZ, inputRot;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
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

        // 1) 회전 (Q/E)
        if (Mathf.Abs(inputRot) > 0.01f)
        {
            rb.MoveRotation(
                rb.rotation * Quaternion.Euler(0f, inputRot * rotateSpeed * Time.fixedDeltaTime, 0f)
            );
        }

        // 2) 이동 (WASD) - "확실히" 움직이게 velocity로 처리
        Vector3 localMove = (transform.right * inputX + transform.forward * inputZ);

        if (localMove.sqrMagnitude > 0.0001f)
        {
            localMove = localMove.normalized;

            Vector3 targetVel = localMove * moveSpeed;
            Vector3 vel = rb.linearVelocity;
            // y 속도는 유지(점프/낙하 고려), xz만 제어
            rb.linearVelocity = new Vector3(targetVel.x, vel.y, targetVel.z);
        }
        else
        {
            // 입력 없을 때 xz만 멈춤(드리프트 방지)
            Vector3 vel = rb.linearVelocity;
            rb.linearVelocity = new Vector3(0f, vel.y, 0f);
        }
    }
}
