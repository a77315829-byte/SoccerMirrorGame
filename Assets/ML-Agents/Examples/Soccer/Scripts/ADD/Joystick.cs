using UnityEngine;
using UnityEngine.EventSystems;

public class SimpleJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Assign")]
    public RectTransform handle;

    [Header("Tuning")]
    public float radius = 120f; // 조이스틱 반경(픽셀)

    public Vector2 Value { get; private set; } // (-1..1)

    private RectTransform _baseRect;
    private Canvas _canvas;
    private Camera _uiCam;

    void Awake()
    {
        _baseRect = (RectTransform)transform;
        _canvas = GetComponentInParent<Canvas>();
        _uiCam = (_canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            ? _canvas.worldCamera
            : null;

        if (handle != null) handle.anchoredPosition = Vector2.zero;
        Value = Vector2.zero;
    }

    public void OnPointerDown(PointerEventData eventData) => OnDrag(eventData);

    public void OnDrag(PointerEventData eventData)
    {
        if (handle == null) return;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _baseRect, eventData.position, _uiCam, out localPoint);

        // 반경 제한
        Vector2 clamped = Vector2.ClampMagnitude(localPoint, radius);
        handle.anchoredPosition = clamped;

        Value = clamped / radius; // -1..1
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (handle != null) handle.anchoredPosition = Vector2.zero;
        Value = Vector2.zero;
    }
}
