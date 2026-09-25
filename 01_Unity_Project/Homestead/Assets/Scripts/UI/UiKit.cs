using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Small helpers for building the game screens in code, in the HUD's established look: dark-green panels, cream text,
// the legacy UI font. Keeps GameScreens and the individual screens short and consistent with each other.
public static class UiKit
{
    public static readonly Color PanelColor = new Color(0.09f, 0.12f, 0.09f, 0.96f);
    public static readonly Color RaisedColor = new Color(0.15f, 0.19f, 0.14f, 1f);
    public static readonly Color HoverColor = new Color(0.22f, 0.27f, 0.2f, 1f);
    public static readonly Color SelectedColor = new Color(0.3f, 0.36f, 0.24f, 1f);
    public static readonly Color Cream = new Color(0.93f, 0.89f, 0.78f, 1f);
    public static readonly Color Muted = new Color(0.93f, 0.89f, 0.78f, 0.6f);
    public static readonly Color Accent = new Color(0.78f, 0.84f, 0.55f, 1f);
    public static readonly Color Warning = new Color(0.9f, 0.45f, 0.3f, 1f);

    static Font font;
    public static Font Font => font != null ? font : font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

    public static RectTransform Rect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    // Stretches to fill the parent, inset by the given margins.
    public static RectTransform Fill(this RectTransform rt, float left = 0f, float bottom = 0f, float right = 0f, float top = 0f)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(left, bottom);
        rt.offsetMax = new Vector2(-right, -top);
        return rt;
    }

    // Anchors to a region of the parent given in 0-1 coordinates, inset by a margin in pixels.
    public static RectTransform Region(this RectTransform rt, float xMin, float yMin, float xMax, float yMax, float margin = 0f)
    {
        rt.anchorMin = new Vector2(xMin, yMin);
        rt.anchorMax = new Vector2(xMax, yMax);
        rt.offsetMin = new Vector2(margin, margin);
        rt.offsetMax = new Vector2(-margin, -margin);
        return rt;
    }

    public static Image Image(Transform parent, string name, Color color)
    {
        RectTransform rt = Rect(name, parent);
        var image = rt.gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    public static Text Text(Transform parent, string name, string text, int size, Color color,
                            TextAnchor anchor = TextAnchor.MiddleLeft, FontStyle style = FontStyle.Normal)
    {
        RectTransform rt = Rect(name, parent);
        var label = rt.gameObject.AddComponent<Text>();
        label.font = Font;
        label.text = text;
        label.fontSize = size;
        label.fontStyle = style;
        label.color = color;
        label.alignment = anchor;
        label.raycastTarget = false;
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Truncate;
        return label;
    }

    public static Button Button(Transform parent, string name, string label, int size, UnityAction onClick,
                                TextAnchor anchor = TextAnchor.MiddleCenter)
    {
        RectTransform rt = Rect(name, parent);
        var image = rt.gameObject.AddComponent<Image>();
        image.color = Color.white;
        var button = rt.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.colors = new ColorBlock
        {
            normalColor = RaisedColor,
            highlightedColor = HoverColor,
            pressedColor = SelectedColor,
            selectedColor = RaisedColor,
            disabledColor = new Color(RaisedColor.r, RaisedColor.g, RaisedColor.b, 0.5f),
            colorMultiplier = 1f,
            fadeDuration = 0.05f,
        };
        // Keyboard/gamepad navigation isn't wired up for these screens, and an automatic "selected" state would
        // leave clicked buttons highlighted.
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        if (onClick != null)
            button.onClick.AddListener(onClick);

        Text text = Text(rt, "Label", label, size, Cream, anchor);
        text.rectTransform.Fill(12f, 0f, 12f, 0f);
        return button;
    }

    // Tints a button to show it's the selected one of a group (tabs, list rows).
    public static void SetSelected(Button button, bool selected)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = selected ? SelectedColor : RaisedColor;
        colors.selectedColor = colors.normalColor;
        colors.highlightedColor = selected ? SelectedColor : HoverColor;
        button.colors = colors;
    }

    // A vertically scrolling list. Add rows to the returned content; each row needs a LayoutElement height.
    public static RectTransform ScrollList(Transform parent, string name, float spacing = 4f)
    {
        RectTransform viewport = Rect(name, parent);
        viewport.gameObject.AddComponent<RectMask2D>();
        var background = viewport.gameObject.AddComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.2f); // also gives the scroll wheel something to hit
        var scroll = viewport.gameObject.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 30f;

        RectTransform content = Rect("Content", viewport);
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.offsetMin = content.offsetMax = Vector2.zero;
        var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = spacing;
        layout.padding = new RectOffset(6, 6, 6, 6);
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.viewport = viewport;
        scroll.content = content;
        return content;
    }

    public static void Clear(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
            Object.Destroy(parent.GetChild(i).gameObject);
    }

    public static T Height<T>(T component, float height) where T : Component
    {
        LayoutElement element = component.GetComponent<LayoutElement>();
        if (element == null)
            element = component.gameObject.AddComponent<LayoutElement>();
        element.minHeight = element.preferredHeight = height;
        return component;
    }

    public static InputField Input(Transform parent, string name, string placeholder, int size, bool multiline)
    {
        RectTransform rt = Rect(name, parent);
        var image = rt.gameObject.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.35f);

        Text text = Text(rt, "Text", "", size, Cream, multiline ? TextAnchor.UpperLeft : TextAnchor.MiddleLeft);
        text.rectTransform.Fill(10f, 6f, 10f, 6f);
        text.supportRichText = false;
        text.raycastTarget = false;

        Text hint = Text(rt, "Placeholder", placeholder, size, Muted,
                         multiline ? TextAnchor.UpperLeft : TextAnchor.MiddleLeft, FontStyle.Italic);
        hint.rectTransform.Fill(10f, 6f, 10f, 6f);

        var field = rt.gameObject.AddComponent<InputField>();
        field.targetGraphic = image;
        field.textComponent = text;
        field.placeholder = hint;
        field.lineType = multiline ? InputField.LineType.MultiLineNewline : InputField.LineType.SingleLine;
        field.caretColor = Cream;
        field.selectionColor = new Color(0.78f, 0.84f, 0.55f, 0.4f);
        field.navigation = new Navigation { mode = Navigation.Mode.None };
        return field;
    }
}
