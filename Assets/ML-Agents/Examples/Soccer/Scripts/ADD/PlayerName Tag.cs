using UnityEngine;

#if TMP_PRESENT
using TMPro;
#endif

public class PlayerNameTag : MonoBehaviour
{
    [Header("Assign in Prefab")]
    public GameObject tagRoot; // NameTag 오브젝트(텍스트 포함) 통째로 넣기

#if TMP_PRESENT
    public TMP_Text tmpText;
#else
    public TextMesh textMesh;
#endif

    private void Awake()
    {
        // 시작은 꺼짐(안전)
        if (tagRoot != null) tagRoot.SetActive(false);
    }

    public void SetYou(bool on)
    {
        if (tagRoot == null) return;

        tagRoot.SetActive(on);
        if (!on) return;

#if TMP_PRESENT
        if (tmpText != null) tmpText.text = "YOU";
#else
        if (textMesh != null) textMesh.text = "YOU";
#endif
    }
}
