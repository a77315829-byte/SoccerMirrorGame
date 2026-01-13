using UnityEngine;

public class PlayerTagFollow : MonoBehaviour
{
    public Transform target;                 // BlueStriker 또는 AgentCube_Blue
    public Vector3 offset = new Vector3(0f, 2.0f, 0f);
    public bool faceCamera = true;

    void LateUpdate()
    {
        if (target == null) return;

        // 위치는 따라가기
        transform.position = target.position + offset;

        // 회전은 고정(또는 카메라를 바라보게)
        if (faceCamera && Camera.main != null)
        {
            var cam = Camera.main.transform;
            transform.rotation = Quaternion.LookRotation(transform.position - cam.position, Vector3.up);
        }
        else
        {
            transform.rotation = Quaternion.identity; // 월드 기준 고정
        }
    }
}
