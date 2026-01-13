using UnityEngine;

public class BillboardToMainCamera : MonoBehaviour
{
    private Transform camT;

    void LateUpdate()
    {
        if (camT == null)
        {
            var cam = Camera.main;
            if (cam == null) return;
            camT = cam.transform;
        }

        // 카메라를 바라보게
        Vector3 dir = transform.position - camT.position;
        if (dir.sqrMagnitude < 0.0001f) return;

        transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
    }
}
