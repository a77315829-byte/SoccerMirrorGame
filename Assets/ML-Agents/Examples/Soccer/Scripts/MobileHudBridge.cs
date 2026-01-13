using UnityEngine;

public class MobileHudBridge : MonoBehaviour
{
    public void ToggleCamera()
    {
        if (NetPlayerInput.Local != null)
            NetPlayerInput.Local.UI_ToggleCamera();
    }

    public void StartMatch()
    {
        if (NetPlayerInput.Local != null)
            NetPlayerInput.Local.UI_Start();
    }

    public void RestartMatch()
    {
        if (NetPlayerInput.Local != null)
            NetPlayerInput.Local.UI_Restart();
    }

    public void ResetBall()
    {
        if (NetPlayerInput.Local != null)
            NetPlayerInput.Local.UI_ResetBall();
    }
}
