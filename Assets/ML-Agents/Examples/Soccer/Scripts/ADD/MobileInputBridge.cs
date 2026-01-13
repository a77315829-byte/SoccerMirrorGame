using UnityEngine;

public class MobileInputBridge : MonoBehaviour
{
    public bl_Joystick moveStick;   // 이동 스틱
    public bl_Joystick lookStick;   // (선택) 회전 스틱

    void Update()
    {
        var p = NetPlayerInput.Local;
        if (p == null) return;

        // 네가 말한 축: Blue 공격이 X, 좌우가 Z라고 했으니
        // bl_Joystick: Horizontal(x), Vertical(y) -> 우리 게임: X / Z 로 매핑
        if (moveStick != null)
        {
            p.joyX = Mathf.Clamp(moveStick.Horizontal, -1f, 1f);
            p.joyZ = Mathf.Clamp(moveStick.Vertical, -1f, 1f);
        }

        // 회전은 오른쪽 스틱 X를 쓰는 방식이 일반적
        if (lookStick != null)
        {
            p.joyRotate = Mathf.Clamp(lookStick.Horizontal, -1f, 1f);
        }
        else
        {
            p.joyRotate = 0f;
        }
    }
}
