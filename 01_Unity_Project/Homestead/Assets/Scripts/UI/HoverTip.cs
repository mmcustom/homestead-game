using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// A small label that follows the mouse while it is over a UI element (Mike, 2026-11-19: hovering an item's icon shows the item's name).
// One shared label per canvas, drawn on top of everything, big enough to read (22 px, cream on a dark panel). Only pointer enter, exit and
// click are handled, so scrolling and dragging still reach the list underneath; a click is passed on to `click` (the row's own action) because
// the icon sits over the row's click area.
// ExecuteAlways so OnDisable also clears the label outside Play Mode (edit-mode tests; in the game it makes no difference).
[ExecuteAlways]
public class HoverTip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    const int FontSize = 22;
    const float Pad = 10f;
    static readonly Vector2 Offset = new Vector2(18f, -22f);

    public string Text { get; set; }
    UnityAction click;

    static RectTransform box;
    static Text label;
    static HoverTip showing;

    // Makes the graphic answer the pointer and shows `text` while it is under it.
    public static HoverTip Add(Graphic target, string text, UnityAction onClick = null)
    {
        target.raycastTarget = true;
        HoverTip tip = target.gameObject.AddComponent<HoverTip>();
        tip.Text = text;
        tip.click = onClick;
        return tip;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        showing = this;
        Show(eventData.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (showing == this)
            Hide();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            click?.Invoke();
    }

    // The row is rebuilt or the screen closes while the pointer is on it: take the label away with it.
    void OnDisable()
    {
        if (showing == this)
            Hide();
    }

    void Update()
    {
        if (showing == this && Mouse.current != null)
            Place(Mouse.current.position.ReadValue());
    }

    void Show(Vector2 screenPoint)
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
            return;
        canvas = canvas.rootCanvas;
        if (box == null || box.GetComponentInParent<Canvas>() != canvas)
            Build(canvas);
        label.text = Text;
        box.sizeDelta = new Vector2(label.preferredWidth + Pad * 2f, label.preferredHeight + Pad);
        box.SetAsLastSibling();
        box.gameObject.SetActive(true);
        Place(screenPoint);
    }

    static void Hide()
    {
        showing = null;
        if (box != null)
            box.gameObject.SetActive(false);
    }

    static void Build(Canvas canvas)
    {
        if (box != null)
        {
            if (Application.isPlaying)
                Destroy(box.gameObject);
            else
                DestroyImmediate(box.gameObject);
        }
        Image panel = UiKit.Image(canvas.transform, "Hover Tip", new Color(0.05f, 0.07f, 0.05f, 0.94f));
        panel.raycastTarget = false;
        box = panel.rectTransform;
        box.anchorMin = box.anchorMax = new Vector2(0.5f, 0.5f);
        box.pivot = new Vector2(0f, 1f);
        label = UiKit.Text(box, "Label", "", FontSize, UiKit.Cream, TextAnchor.MiddleCenter);
        label.raycastTarget = false;
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        label.rectTransform.Fill();
    }

    // Beside the pointer, kept inside the canvas.
    static void Place(Vector2 screenPoint)
    {
        if (box == null)
            return;
        var canvasRt = (RectTransform)box.parent;
        Canvas canvas = canvasRt.GetComponent<Canvas>();
        Camera cam = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRt, screenPoint, cam, out Vector2 local))
            return;
        Vector2 pos = local + Offset;
        Rect area = canvasRt.rect;
        pos.x = Mathf.Min(pos.x, area.xMax - box.sizeDelta.x);
        pos.y = Mathf.Max(pos.y, area.yMin + box.sizeDelta.y);
        box.anchoredPosition = pos;
    }
}
