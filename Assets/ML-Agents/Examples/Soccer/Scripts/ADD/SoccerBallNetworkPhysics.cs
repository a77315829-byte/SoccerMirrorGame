using Mirror;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SoccerBallNetworkPhysics : NetworkBehaviour
{
    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnStartServer()
    {
        // 서버에서는 정상 물리
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.detectCollisions = true;
    }

    public override void OnStartClient()
    {
        if (isServer) return; // Host(서버+클라)에서는 서버 로직이 우선

        // 클라이언트에서는 물리 계산을 끄고 위치 수신만 하게
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.detectCollisions = false;
    }
}

