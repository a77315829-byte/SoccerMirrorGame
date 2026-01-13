using UnityEngine;
using Mirror;

[RequireComponent(typeof(Rigidbody))]
public class NetworkBallAuthority : NetworkBehaviour
{
    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnStartServer()
    {
        // 서버만 물리 시뮬레이션
        rb.isKinematic = false;
    }

    public override void OnStartClient()
    {
        // 클라는 물리 끄고 위치만 수신
        if (!isServer)
            rb.isKinematic = true;
    }
}

