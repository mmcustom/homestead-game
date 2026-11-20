using UnityEngine;
using UnityEngine.UI;

// The Loading scene's screen (Wave 7 and 10, 2026-11-20). GameManager shows the Loading scene while the next scene loads in the
// background; before this it was an empty, black scene. Built in code: the title picture (the homestead at autumn dusk), the Homestead
// logo, and a parchment slip with "Loading your homestead..." and a bar that follows GameManager.LoadProgress.
public class LoadingScreen : MonoBehaviour
{
    public const string BackgroundPath = "Art/title_background";
    public const string LogoPath = "Art/logo_lockup";

    RectTransform barFill;
    float shown;

    void Awake() => Build();

    void Build()
    {
        var root = new GameObject("Loading Canvas", typeof(RectTransform));
        root.transform.SetParent(transform, false);
        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        var area = (RectTransform)root.transform;

        UiKit.Image(area, "Black", Color.black).rectTransform.Fill();
        Texture2D picture = Resources.Load<Texture2D>(BackgroundPath);
        if (picture != null)
        {
            var background = UiKit.Rect("Background", area).gameObject.AddComponent<RawImage>();
            background.texture = picture;
            background.raycastTarget = false;
            var fitter = background.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = (float)picture.width / picture.height;
        }

        Texture2D logo = Resources.Load<Texture2D>(LogoPath);
        if (logo != null)
        {
            var image = UiKit.Rect("Logo", area).gameObject.AddComponent<RawImage>();
            image.texture = logo;
            image.raycastTarget = false;
            RectTransform rt = image.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.27f, 0.68f);
            rt.sizeDelta = new Vector2(820f, 820f * logo.height / logo.width);
        }

        // The slip: a parchment card with the line and the bar.
        Image slip = UiKit.Card(area, "Progress", UiKit.HoverColor);
        RectTransform s = slip.rectTransform;
        s.anchorMin = s.anchorMax = new Vector2(0.27f, 0.2f);
        s.sizeDelta = new Vector2(720f, 120f);

        Text label = UiKit.Heading(s, "Label", "Loading your homestead...", 32, TextAnchor.MiddleCenter);
        label.rectTransform.anchorMin = new Vector2(0f, 0.5f);
        label.rectTransform.anchorMax = Vector2.one;
        label.rectTransform.offsetMin = new Vector2(24f, 0f);
        label.rectTransform.offsetMax = new Vector2(-24f, -8f);

        Image track = UiKit.Image(s, "Track", new Color(0.30f, 0.20f, 0.10f, 0.9f));
        if (UiKit.TrackSprite != null)
        {
            track.sprite = UiKit.TrackSprite;
            track.type = Image.Type.Sliced;
            track.pixelsPerUnitMultiplier = 1.6f;
        }
        RectTransform tr = track.rectTransform;
        tr.anchorMin = new Vector2(0f, 0.18f);
        tr.anchorMax = new Vector2(1f, 0.42f);
        tr.offsetMin = new Vector2(36f, 0f);
        tr.offsetMax = new Vector2(-36f, 0f);

        Image fill = UiKit.Image(tr, "Fill", UiKit.InkAccent);
        barFill = fill.rectTransform;
        barFill.anchorMin = Vector2.zero;
        barFill.anchorMax = new Vector2(0f, 1f);
        barFill.offsetMin = new Vector2(3f, 3f);
        barFill.offsetMax = new Vector2(0f, -3f);
    }

    void Update()
    {
        float target = GameManager.Instance != null ? GameManager.Instance.LoadProgress : 0f;
        // Unity reports 0.9 when the scene is ready to switch; count that as full. Ease toward it so the bar never jumps back.
        target = Mathf.Clamp01(target / 0.9f);
        shown = Mathf.Max(shown, Mathf.MoveTowards(shown, target, Time.unscaledDeltaTime * 1.5f));
        if (barFill != null)
            barFill.anchorMax = new Vector2(shown, 1f);
    }
}
