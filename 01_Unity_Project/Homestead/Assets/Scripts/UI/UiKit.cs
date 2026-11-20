using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Small helpers for building the game screens in code, so they stay short and look alike.
//
// The look is "Ledger" (Wave 7, chosen 2026-11-20): the screens are parchment pages in a dark wood frame, with brown ink,
// Alegreya Sans for text and Alegreya Bold for headings (both SIL Open Font Licence, in Resources/Fonts with their OFL files).
// Two families of colours:
//  - Ink, InkMuted, InkAccent, InkWarning, HeadingColor: text on the parchment screens.
//  - Cream, Muted, Accent, Warning: text drawn straight over the world (the HUD), which stays light with a shadow.
// Images given PanelColor get the framed parchment sprite, and RaisedColor, HoverColor and SelectedColor get the paper-card sprite
// tinted by that colour, so the screens that build "Image(…, UiKit.PanelColor)" pick up the art without knowing about it.
// If the sprites are missing (Resources/Art/ui_*.png) everything still works as plain coloured rectangles.
public static class UiKit
{
    // Paper colours (the card sprite is near-white, so these are what the rows and boxes end up as).
    public static readonly Color PanelColor = new Color(0.88f, 0.81f, 0.66f, 1f);
    public static readonly Color RaisedColor = new Color(0.96f, 0.92f, 0.81f, 1f);
    public static readonly Color HoverColor = new Color(1f, 0.97f, 0.88f, 1f);
    public static readonly Color SelectedColor = new Color(0.82f, 0.70f, 0.48f, 1f);
    public static readonly Color ButtonColor = new Color(0.86f, 0.78f, 0.62f, 1f);
    public static readonly Color ButtonHover = new Color(0.94f, 0.88f, 0.74f, 1f);
    public static readonly Color ButtonPressed = new Color(0.72f, 0.60f, 0.42f, 1f);

    // Ink on parchment.
    public static readonly Color Ink = new Color(0.19f, 0.13f, 0.07f, 1f);
    public static readonly Color InkMuted = new Color(0.40f, 0.31f, 0.20f, 1f);
    public static readonly Color InkAccent = new Color(0.18f, 0.38f, 0.13f, 1f);   // green ink: good news, badges
    public static readonly Color InkWarning = new Color(0.68f, 0.18f, 0.08f, 1f);  // red ink
    public static readonly Color HeadingColor = new Color(0.28f, 0.17f, 0.08f, 1f);

    // Notices that must not be missed (refusals, warnings): a parchment slip with a red-ink rim and heading.
    public static readonly Color NoticeRed = new Color(0.62f, 0.13f, 0.07f, 1f);
    public static readonly Color NoticePaper = new Color(1f, 0.96f, 0.86f, 1f);
    public const string NoticeHeadingHex = "#9E2410";

    // Light text over the world (HUD).
    public static readonly Color Cream = new Color(0.93f, 0.89f, 0.78f, 1f);
    public static readonly Color Muted = new Color(0.93f, 0.89f, 0.78f, 0.6f);
    public static readonly Color Accent = new Color(0.78f, 0.84f, 0.55f, 1f);
    public static readonly Color Warning = new Color(0.9f, 0.45f, 0.3f, 1f);

    // The new fonts read a little smaller than the old Arial at the same size, and Mike reads small text with difficulty,
    // so text under LargeText is raised by this much (never past LargeText). Sizes of LargeText and up were already agreed
    // with Mike for legibility (the Workshop, Smithy and Potter screens) and are left as they are.
    public const float TextScale = 1.12f;
    public const int LargeText = 26;

    static Font font, boldFont, titleFont;
    public static Font Font => font != null ? font : font = LoadFont("Fonts/AlegreyaSans-Regular");
    public static Font BoldFont => boldFont != null ? boldFont : boldFont = LoadFont("Fonts/AlegreyaSans-Bold");
    public static Font TitleFont => titleFont != null ? titleFont : titleFont = LoadFont("Fonts/Alegreya-Bold");

    static Font LoadFont(string path)
    {
        Font loaded = Resources.Load<Font>(path);
        return loaded != null ? loaded : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    static Sprite panelSprite, cardSprite, buttonSprite, trackSprite, woodSprite;
    static bool spritesLoaded;
    public static Sprite PanelSprite { get { LoadSprites(); return panelSprite; } }
    public static Sprite CardSprite { get { LoadSprites(); return cardSprite; } }
    public static Sprite ButtonSprite { get { LoadSprites(); return buttonSprite; } }
    public static Sprite TrackSprite { get { LoadSprites(); return trackSprite; } }
    public static Sprite WoodSprite { get { LoadSprites(); return woodSprite; } }

    static void LoadSprites()
    {
        if (spritesLoaded)
            return;
        spritesLoaded = true;
        panelSprite = Resources.Load<Sprite>("Art/ui_panel");
        cardSprite = Resources.Load<Sprite>("Art/ui_card");
        buttonSprite = Resources.Load<Sprite>("Art/ui_button");
        trackSprite = Resources.Load<Sprite>("Art/ui_track");
        woodSprite = Resources.Load<Sprite>("Art/ui_wood");
    }

    static bool SameRgb(Color a, Color b) =>
        Mathf.Abs(a.r - b.r) < 0.002f && Mathf.Abs(a.g - b.g) < 0.002f && Mathf.Abs(a.b - b.b) < 0.002f;

    // Whether a colour is dark enough that text on it should be light.
    public static bool IsDark(Color c) => 0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b < 0.42f;

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
        if (SameRgb(color, PanelColor) && PanelSprite != null)
        {
            Slice(image, PanelSprite, PanelFrameScale);
            image.color = new Color(1f, 1f, 1f, color.a);
        }
        else if ((SameRgb(color, RaisedColor) || SameRgb(color, HoverColor) || SameRgb(color, SelectedColor)) && CardSprite != null)
            Slice(image, CardSprite);
        return image;
    }

    // A paper card (list row, box) of the given tint.
    public static Image Card(Transform parent, string name, Color tint)
    {
        Image image = Image(parent, name, tint);
        if (CardSprite != null)
            Slice(image, CardSprite);
        return image;
    }

    // The panel art's wood frame is 26 px; drawn at 1/1.4 it is about 19 px, which clears the screens' existing 24-32 px margins.
    const float PanelFrameScale = 1.4f;

    // A notice slip: a red rim (the returned image) round a parchment card ("Panel"), for banners that must stand out.
    public static Image NoticeSlip(Transform parent, string name, out RectTransform panel)
    {
        Image rim = Card(parent, name, NoticeRed);
        Image paper = Card(rim.rectTransform, "Panel", NoticePaper);
        paper.rectTransform.Fill(5f, 5f, 5f, 5f);
        panel = paper.rectTransform;
        return rim;
    }

    static void Slice(Image image, Sprite sprite, float scale = 1f)
    {
        image.sprite = sprite;
        image.type = UnityEngine.UI.Image.Type.Sliced;
        image.fillCenter = true;
        image.pixelsPerUnitMultiplier = scale;
    }

    public static int Scaled(int size) => size >= LargeText ? size : Mathf.Min(LargeText, Mathf.RoundToInt(size * TextScale));

    public static Text Text(Transform parent, string name, string text, int size, Color color,
                            TextAnchor anchor = TextAnchor.MiddleLeft, FontStyle style = FontStyle.Normal)
    {
        RectTransform rt = Rect(name, parent);
        var label = rt.gameObject.AddComponent<Text>();
        // The bold face is its own file, so bold text uses it rather than Unity's smeared fake bold.
        bool bold = style == FontStyle.Bold || style == FontStyle.BoldAndItalic;
        label.font = bold ? BoldFont : Font;
        label.fontStyle = bold ? (style == FontStyle.BoldAndItalic ? FontStyle.Italic : FontStyle.Normal) : style;
        label.text = text;
        label.fontSize = Scaled(size);
        label.color = color;
        label.alignment = anchor;
        label.raycastTarget = false;
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Truncate;
        return label;
    }

    // A screen or section title: Alegreya Bold in heading brown.
    public static Text Heading(Transform parent, string name, string text, int size, TextAnchor anchor = TextAnchor.MiddleLeft)
    {
        Text label = Text(parent, name, text, size, HeadingColor, anchor);
        label.font = TitleFont;
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        return label;
    }

    public static Button Button(Transform parent, string name, string label, int size, UnityAction onClick,
                                TextAnchor anchor = TextAnchor.MiddleCenter)
    {
        RectTransform rt = Rect(name, parent);
        var image = rt.gameObject.AddComponent<Image>();
        image.color = Color.white;
        if (ButtonSprite != null)
            Slice(image, ButtonSprite);
        var button = rt.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.colors = new ColorBlock
        {
            normalColor = ButtonColor,
            highlightedColor = ButtonHover,
            pressedColor = ButtonPressed,
            selectedColor = ButtonColor,
            disabledColor = new Color(ButtonColor.r, ButtonColor.g, ButtonColor.b, 0.5f),
            colorMultiplier = 1f,
            fadeDuration = 0.05f,
        };
        // Keyboard/gamepad navigation isn't wired up for these screens, and an automatic "selected" state would
        // leave clicked buttons highlighted.
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        if (onClick != null)
            button.onClick.AddListener(onClick);

        Text text = Text(rt, "Label", label, size, Ink, anchor, FontStyle.Bold);
        text.rectTransform.Fill(12f, 0f, 12f, 0f);
        return button;
    }

    // The label colour that reads on a button's current background: ink on paper, cream on dark wood; muted when disabled.
    public static Color LabelColor(Button button, bool enabled)
    {
        bool dark = IsDark(button.colors.normalColor);
        if (dark)
            return enabled ? Cream : Muted;
        return enabled ? Ink : new Color(InkMuted.r, InkMuted.g, InkMuted.b, 0.7f);
    }

    // Tints a button to show it's the selected one of a group (tabs, list rows).
    public static void SetSelected(Button button, bool selected)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = selected ? SelectedColor : ButtonColor;
        colors.selectedColor = colors.normalColor;
        colors.highlightedColor = selected ? SelectedColor : ButtonHover;
        button.colors = colors;
    }

    // A vertically scrolling list. Add rows to the returned content; each row needs a LayoutElement height. withScrollbar adds a
    // slim bar down the right edge that shows only when the rows overflow.
    public static RectTransform ScrollList(Transform parent, string name, float spacing = 4f, bool withScrollbar = false)
    {
        RectTransform viewport = Rect(name, parent);
        viewport.gameObject.AddComponent<RectMask2D>();
        var background = viewport.gameObject.AddComponent<Image>();
        background.color = new Color(0.45f, 0.32f, 0.16f, 0.08f); // a faint shadow on the page; also gives the scroll wheel something to hit
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

        if (withScrollbar)
        {
            RectTransform bar = Rect("Scrollbar", viewport);
            bar.anchorMin = new Vector2(1f, 0f);
            bar.anchorMax = Vector2.one;
            bar.pivot = new Vector2(1f, 0.5f);
            bar.sizeDelta = new Vector2(12f, 0f);
            bar.anchoredPosition = Vector2.zero;
            var track = bar.gameObject.AddComponent<Image>();
            track.color = new Color(0.40f, 0.28f, 0.14f, 0.18f);
            RectTransform handle = Rect("Handle", bar);
            handle.Fill();
            var handleImage = handle.gameObject.AddComponent<Image>();
            handleImage.color = new Color(0.36f, 0.24f, 0.13f, 0.85f);
            var scrollbar = bar.gameObject.AddComponent<Scrollbar>();
            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            scrollbar.handleRect = handle;
            scrollbar.targetGraphic = handleImage;
            scroll.verticalScrollbar = scrollbar;
            scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
        }
        return content;
    }

    // Whether either Shift key is down (the click shortcuts: the whole stack, ten, or plus and minus ten).
    public static bool ShiftHeld =>
        UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.shiftKey.isPressed;

    // Gives the keyboard back to the game screens (an amount box keeps it until something else is selected), so the hotkeys work again.
    public static void ReleaseFocus()
    {
        if (UnityEngine.EventSystems.EventSystem.current != null)
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
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
        image.color = new Color(1f, 0.98f, 0.92f, 1f);
        if (CardSprite != null)
            Slice(image, CardSprite);

        Text text = Text(rt, "Text", "", size, Ink, multiline ? TextAnchor.UpperLeft : TextAnchor.MiddleLeft);
        text.rectTransform.Fill(10f, 6f, 10f, 6f);
        text.supportRichText = false;
        text.raycastTarget = false;

        Text hint = Text(rt, "Placeholder", placeholder, size, InkMuted,
                         multiline ? TextAnchor.UpperLeft : TextAnchor.MiddleLeft, FontStyle.Italic);
        hint.rectTransform.Fill(10f, 6f, 10f, 6f);

        var field = rt.gameObject.AddComponent<InputField>();
        field.targetGraphic = image;
        field.textComponent = text;
        field.placeholder = hint;
        field.lineType = multiline ? InputField.LineType.MultiLineNewline : InputField.LineType.SingleLine;
        field.caretColor = Ink;
        field.selectionColor = new Color(0.82f, 0.70f, 0.48f, 0.55f);
        field.navigation = new Navigation { mode = Navigation.Mode.None };
        return field;
    }
}
