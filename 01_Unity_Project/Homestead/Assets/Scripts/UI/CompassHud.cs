using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// HUD compass (Mike's ask, 2026-09-25): a heading strip across the top of the screen showing which way the camera
// faces, with the cardinal and intercardinal directions and a tick every 15°. Direction only — no locations or
// waypoints, so it doesn't touch Discovery_System.md's "the map should be learned, not revealed" rule.
// North is world +Z, matching the minimap and WeatherManager's wind bearing (0 = north).
public class CompassHud : MonoBehaviour
{
    static readonly string[] Labels = { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };

    [Tooltip("Where the ticks and labels are laid out; its width shows the visible arc.")]
    [SerializeField] RectTransform strip;
    [SerializeField] Font font;
    [Tooltip("Degrees of heading visible across the strip.")]
    [SerializeField, Range(60f, 360f)] float visibleDegrees = 160f;
    [SerializeField] Color labelColor = new Color(0.93f, 0.89f, 0.78f, 1f);
    [SerializeField] Color northColor = new Color(0.9f, 0.45f, 0.3f, 1f);

    struct Mark
    {
        public float bearing;
        public RectTransform rect;
        public Graphic graphic;
        public float baseAlpha;
    }

    readonly List<Mark> marks = new List<Mark>();
    CanvasGroup group;
    PlayerController player;
    float nextPlayerSearch;

    void Awake()
    {
        group = GetComponent<CanvasGroup>();

        for (int bearing = 0; bearing < 360; bearing += 15)
        {
            if (bearing % 45 == 0)
            {
                string text = Labels[bearing / 45];
                bool cardinal = bearing % 90 == 0;
                AddLabel(bearing, text, cardinal ? 26 : 19, bearing == 0 ? northColor : labelColor, cardinal);
            }
            else
            {
                AddTick(bearing);
            }
        }
    }

    void LateUpdate()
    {
        Transform view = Camera.main != null ? Camera.main.transform : null;
        if (player == null && Time.unscaledTime >= nextPlayerSearch)
        {
            nextPlayerSearch = Time.unscaledTime + 1f;
            player = FindAnyObjectByType<PlayerController>();
        }

        bool visible = player != null && view != null;
        if (group != null)
            group.alpha = visible ? 1f : 0f;
        if (!visible)
            return;

        float heading = Heading(view.forward);
        float halfWidth = strip.rect.width / 2f;
        float pixelsPerDegree = strip.rect.width / visibleDegrees;

        foreach (Mark mark in marks)
        {
            float delta = Mathf.DeltaAngle(heading, mark.bearing);
            float x = delta * pixelsPerDegree;
            bool shown = Mathf.Abs(x) <= halfWidth;
            mark.rect.gameObject.SetActive(shown);
            if (!shown)
                continue;

            mark.rect.anchoredPosition = new Vector2(x, mark.rect.anchoredPosition.y);
            // Fade toward the ends so marks don't pop at the strip's edges.
            float edge = Mathf.Clamp01((halfWidth - Mathf.Abs(x)) / (halfWidth * 0.25f));
            Color c = mark.graphic.color;
            c.a = mark.baseAlpha * edge;
            mark.graphic.color = c;
        }
    }

    // Compass bearing of a direction, 0-360, clockwise from north (+Z).
    public static float Heading(Vector3 forward) => Mathf.Repeat(Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg, 360f);

    void AddLabel(float bearing, string text, int size, Color color, bool bold)
    {
        var go = new GameObject(text, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(strip, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(60f, 36f);
        rt.anchoredPosition = new Vector2(0f, 2f);

        var label = go.GetComponent<Text>();
        label.font = font != null ? font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.text = text;
        label.fontSize = size;
        label.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
        label.color = color;
        label.alignment = TextAnchor.MiddleCenter;
        label.raycastTarget = false;
        marks.Add(new Mark { bearing = bearing, rect = rt, graphic = label, baseAlpha = color.a });
    }

    void AddTick(float bearing)
    {
        var go = new GameObject($"{bearing}°", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(strip, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(2f, 10f);
        rt.anchoredPosition = new Vector2(0f, 2f);

        var image = go.GetComponent<Image>();
        image.color = new Color(labelColor.r, labelColor.g, labelColor.b, 0.6f);
        image.raycastTarget = false;
        marks.Add(new Mark { bearing = bearing, rect = rt, graphic = image, baseAlpha = 0.6f });
    }
}
