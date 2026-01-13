using UnityEngine;

public class MobileOnlyUI : MonoBehaviour
{
    void Start()
    {
        gameObject.SetActive(Application.isMobilePlatform);
    }
}
