using UnityEngine;
using UnityEngine.UI;

// Shows ToolStatus on the HUD: a small crosshair while an aimed tool is equipped, the tool's status line and
// progress bar under the centre of the screen, and brief messages (a catch, a miss) above them.
//
// Weapons get a real sight picture instead of the dot (Hunting_System.md's Aiming section):
//   Spread reticle — four ticks around a centre dot, spaced to the shot's actual spread cone on screen, so a partial
//   bow draw or a shot on the move visibly opens up and a full draw closes to a point.
//   Scope — the rifle aiming down sights: a round sight picture with a black surround and a duplex crosshair.
// Both take their colour from what the shot line is on — cream on nothing, amber on an animal's body, green through
// its heart and lungs (the clean-kill zone) — with the range and, past the weapon's effective range, a warning.
public class ToolHud : MonoBehaviour
{
    [SerializeField] Graphic crosshair;
    [SerializeField] Text statusLabel;
    [SerializeField] RectTransform bar;
    [SerializeField] RectTransform barFill;
    [SerializeField] CanvasGroup flashGroup;
    [SerializeField] Text flashLabel;

    [Header("Weapon sights")]
    [SerializeField] Color idleColor = new Color(0.93f, 0.89f, 0.78f, 0.9f);
    [SerializeField] Color bodyColor = new Color(0.95f, 0.66f, 0.25f, 1f);
    [SerializeField] Color vitalsColor = new Color(0.45f, 0.88f, 0.4f, 1f);
    [Tooltip("Smallest gap between the spread reticle's ticks and the centre (canvas pixels).")]
    [SerializeField, Min(0f)] float minSpreadRadius = 7f;

    RectTransform spreadRoot, scopeRoot;
    readonly Image[] ticks = new Image[4];
    Image spreadDot, scopeDot;
    Image[] scopeLines;
    Text readout;

    void Awake() => BuildSights();

    void LateUpdate()
    {
        bool playing = GameManager.Instance == null || GameManager.Instance.State == GameState.Playing;
        ReticleKind reticle = playing ? ToolStatus.Reticle : ReticleKind.None;

        crosshair.enabled = playing && ToolStatus.Crosshair && reticle == ReticleKind.None;

        string line = playing ? ToolStatus.Line : null;
        statusLabel.enabled = !string.IsNullOrEmpty(line);
        if (statusLabel.enabled)
            statusLabel.text = line;

        float progress = playing ? ToolStatus.Progress : -1f;
        bar.gameObject.SetActive(progress >= 0f);
        if (progress >= 0f)
            barFill.anchorMax = new Vector2(Mathf.Clamp01(progress), 1f);

        string flash = ToolStatus.FlashMessage;
        flashGroup.alpha = Mathf.MoveTowards(flashGroup.alpha, string.IsNullOrEmpty(flash) ? 0f : 1f, Time.unscaledDeltaTime * 4f);
        if (!string.IsNullOrEmpty(flash))
            flashLabel.text = flash;

        UpdateSights(reticle);
    }

    void UpdateSights(ReticleKind reticle)
    {
        spreadRoot.gameObject.SetActive(reticle == ReticleKind.Spread);
        scopeRoot.gameObject.SetActive(reticle == ReticleKind.Scope);
        readout.enabled = reticle != ReticleKind.None && ToolStatus.Target != AimTarget.None;
        if (reticle == ReticleKind.None)
            return;

        AimTarget target = ToolStatus.Target;
        Color color = target == AimTarget.Vitals ? vitalsColor : target == AimTarget.Body ? bodyColor : idleColor;

        if (reticle == ReticleKind.Spread)
        {
            float radius = Mathf.Max(minSpreadRadius, DegreesToCanvas(ToolStatus.SpreadDegrees));
            Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
            for (int i = 0; i < ticks.Length; i++)
            {
                ticks[i].rectTransform.anchoredPosition = directions[i] * (radius + 5f);
                ticks[i].color = color;
            }
            spreadDot.color = color;
            readout.rectTransform.anchoredPosition = new Vector2(0f, -(radius + 26f)); // just below the lower tick
        }
        else
        {
            scopeDot.color = color;
            readout.rectTransform.anchoredPosition = new Vector2(0f, -60f);
            foreach (Image lineImage in scopeLines)
                lineImage.color = new Color(0.05f, 0.05f, 0.05f, 0.95f);
        }

        if (readout.enabled)
        {
            string placement = target == AimTarget.Vitals ? "heart / lungs" : "body — wounding shot";
            readout.text = ToolStatus.InEffectiveRange
                ? $"{ToolStatus.AimDistance:0} m · {placement}"
                : $"{ToolStatus.AimDistance:0} m · {placement} · beyond effective range";
            readout.color = color;
        }
    }

    // How many canvas pixels an angle from the screen centre covers, at the camera's current field of view.
    float DegreesToCanvas(float degrees)
    {
        Camera cam = Camera.main;
        var canvas = (RectTransform)transform;
        float halfHeight = canvas.rect.height * 0.5f;
        float halfFov = (cam != null ? cam.fieldOfView : 60f) * 0.5f * Mathf.Deg2Rad;
        return Mathf.Tan(degrees * Mathf.Deg2Rad) / Mathf.Tan(halfFov) * halfHeight;
    }

    // --- Building the sights ---

    void BuildSights()
    {
        var root = (RectTransform)transform;

        // Spread reticle: centre dot and four ticks.
        spreadRoot = UiKit.Rect("Spread Reticle", root);
        spreadRoot.anchorMin = spreadRoot.anchorMax = new Vector2(0.5f, 0.5f);
        spreadDot = Block(spreadRoot, "Dot", new Vector2(4f, 4f));
        for (int i = 0; i < ticks.Length; i++)
            ticks[i] = Block(spreadRoot, "Tick", i < 2 ? new Vector2(2f, 9f) : new Vector2(9f, 2f));
        AddOutline(spreadRoot);

        // Scope: a round sight picture, black beyond it, with a duplex crosshair.
        scopeRoot = UiKit.Rect("Scope", root);
        scopeRoot.Fill();
        scopeRoot.SetAsFirstSibling(); // under the status line and messages
        var mask = UiKit.Image(scopeRoot, "Sight Picture", Color.black);
        mask.sprite = Sprite.Create(ScopeMaskTexture(), new Rect(0, 0, 512, 512), new Vector2(0.5f, 0.5f));
        mask.rectTransform.anchorMin = new Vector2(0.5f, 0f);
        mask.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        mask.rectTransform.sizeDelta = new Vector2(1080f, 0f);
        mask.gameObject.AddComponent<AspectRatioFitter>().aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
        // Black beyond the circle's left and right edges, out past any screen width.
        foreach (float side in new[] { 0f, 1f })
        {
            var fill = UiKit.Image(mask.rectTransform, "Surround", Color.black);
            fill.rectTransform.anchorMin = new Vector2(side, 0f);
            fill.rectTransform.anchorMax = new Vector2(side, 1f);
            fill.rectTransform.pivot = new Vector2(1f - side, 0.5f);
            fill.rectTransform.sizeDelta = new Vector2(3000f, 0f);
            fill.rectTransform.anchoredPosition = Vector2.zero;
        }

        // Thin wires across the middle, heavy posts toward the edge (a duplex reticle), and a centre dot for colour.
        var lines = new System.Collections.Generic.List<Image>();
        var centre = UiKit.Rect("Crosshair", scopeRoot);
        centre.anchorMin = centre.anchorMax = new Vector2(0.5f, 0.5f);
        lines.Add(Block(centre, "Wire H", new Vector2(360f, 1.5f)));
        lines.Add(Block(centre, "Wire V", new Vector2(1.5f, 360f)));
        foreach (Vector2 dir in new[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right })
        {
            Image post = Block(centre, "Post", dir.x == 0f ? new Vector2(5f, 320f) : new Vector2(320f, 5f));
            post.rectTransform.anchoredPosition = dir * (180f + 160f);
            lines.Add(post);
        }
        scopeLines = lines.ToArray();
        scopeDot = Block(centre, "Dot", new Vector2(5f, 5f));

        // Range and placement readout under the reticle.
        readout = UiKit.Text(root, "Aim Readout", "", 18, idleColor, TextAnchor.UpperCenter);
        readout.rectTransform.anchorMin = readout.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        readout.rectTransform.sizeDelta = new Vector2(600f, 26f);
        readout.rectTransform.anchoredPosition = new Vector2(0f, -40f);
        readout.horizontalOverflow = HorizontalWrapMode.Overflow;
        readout.gameObject.AddComponent<Shadow>().effectColor = new Color(0f, 0f, 0f, 0.9f);

        spreadRoot.gameObject.SetActive(false);
        scopeRoot.gameObject.SetActive(false);
        readout.enabled = false;
    }

    static Image Block(RectTransform parent, string name, Vector2 size)
    {
        Image image = UiKit.Image(parent, name, Color.white);
        image.rectTransform.anchorMin = image.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        image.rectTransform.sizeDelta = size;
        return image;
    }

    // A dark edge so the reticle reads against bright sky and dark woods alike.
    static void AddOutline(RectTransform root)
    {
        foreach (Image image in root.GetComponentsInChildren<Image>())
            image.gameObject.AddComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0.7f);
    }

    // Opaque outside a circle, clear inside, with a slightly soft edge.
    static Texture2D ScopeMaskTexture()
    {
        const int size = 512;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
        var pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x + 0.5f) / size * 2f - 1f, dy = (y + 0.5f) / size * 2f - 1f;
                float r = Mathf.Sqrt(dx * dx + dy * dy);
                byte alpha = (byte)(Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.94f, 0.99f, r)) * 255f);
                pixels[y * size + x] = new Color32(255, 255, 255, alpha);
            }
        }
        texture.SetPixels32(pixels);
        texture.Apply(false, true);
        return texture;
    }
}
