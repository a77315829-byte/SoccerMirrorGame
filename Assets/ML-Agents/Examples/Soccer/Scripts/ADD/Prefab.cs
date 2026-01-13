using Mirror;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ServerPhysicsOnly : NetworkBehaviour
{
    Rigidbody rb;

    void Awake() => rb = GetComponent<Rigidbody>();

    public override void OnStartServer()
    {
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.None;
    }

    public override void OnStartClient()
    {
        // 클라에서는 물리 계산 금지(서버가 NetworkRigidbody/Transform으로 보내는 값만 받음)
        if (!isServer)
        {
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.None;
        }
    }
}
