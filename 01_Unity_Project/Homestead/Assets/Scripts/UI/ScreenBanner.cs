using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// The large refusal banner shared by the Inventory screen and the Crafting screen (Inventory_System.md's Build/Craft
// Missing-Materials Notice): a banner across the top of the screen in large type on a solid panel — Mike found the small
// line hard to read (2026-10-04). A missing-materials refusal lists one material per line, quantity first, and stays up
// long enough to read; it doesn't block clicks. A screen builds one, calls Tick() every frame and Hide() when it closes.
public class ScreenBanner
{
    static readonly Color BannerBorder = new Color(0.96f, 0.72f, 0.30f, 1f);
    static readonly Color BannerFill = new Color(0.36f, 0.07f, 0.06f, 1f);

    RectTransform rect;
    Text text;
    float until;

    // Build it last, so it draws over everything else on the screen.
    public void Build(RectTransform area)
    {
        Image border = UiKit.Image(area, "Notice Banner", BannerBorder);
        rect = border.rectTransform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -6f);
        rect.sizeDelta = new Vector2(-40f, 100f);

        Image panel = UiKit.Image(rect, "Panel", BannerFill);
        panel.rectTransform.Fill(4f, 4f, 4f, 4f);
        text = UiKit.Text(panel.rectTransform, "Text", "", 30, UiKit.Cream, TextAnchor.UpperLeft);
        text.rectTransform.Fill(22f, 8f, 22f, 8f);
        text.verticalOverflow = VerticalWrapMode.Overflow;
        rect.gameObject.SetActive(false);
    }

    public void Hide()
    {
        until = 0f;
        if (rect != null)
            rect.gameObject.SetActive(false);
    }

    public void Tick()
    {
        if (until > 0f && Time.unscaledTime >= until)
            Hide();
    }

    // Shows the banner: a heading, an optional smaller sub-heading, then one big line per entry.
    public void Show(string heading, string subheading, List<string> lines)
    {
        var builder = new System.Text.StringBuilder();
        builder.Append($"<size=34><b><color=#FFE2BC>{heading}</color></b></size>");
        if (subheading != null)
            builder.Append($"\n<size=27><color=#F6CFA3>{subheading}</color></size>");
        foreach (string line in lines)
            builder.Append($"\n<size=40><b>{line}</b></size>");
        text.text = builder.ToString();

        // About 42 px for the heading, 33 for a sub-heading and 50 a line, plus padding.
        float height = 30f + 42f + (subheading != null ? 33f : 0f) + lines.Count * 50f;
        rect.sizeDelta = new Vector2(-40f, height);
        rect.SetAsLastSibling();
        rect.gameObject.SetActive(true);
        until = Time.unscaledTime + Mathf.Min(14f, 6f + 1.5f * lines.Count);
        if (AudioManager.Instance != null)
            AudioManager.Instance.Play(SoundCue.UiBack);
    }

    // A Build/Craft refusal. The reasons from Crafting, WoodManager and FireManager read "Needs 4 more Logs, 2 more
    // Stone." (also "… at the site." and "… (or 3 Tall Grass, or 2 Cattail).") — split into one material per line.
    public void NotifyCant(string heading, string reason)
    {
        var lines = new List<string>();
        string message = (reason ?? "").Trim();
        if (!message.StartsWith("Needs "))
        {
            if (message.Length > 0)
                lines.Add(message);
            Show(heading, null, lines);
            return;
        }

        string body = message.Substring("Needs ".Length).TrimEnd('.');
        string alternatives = null, where = null;
        int alt = body.IndexOf(" (or ", System.StringComparison.Ordinal);
        if (alt >= 0)
        {
            alternatives = body.Substring(alt + " (or ".Length).TrimEnd(')');
            body = body.Substring(0, alt);
        }
        const string AtSite = " at the site";
        if (body.EndsWith(AtSite, System.StringComparison.Ordinal))
        {
            where = "You still need, at the cabin site:";
            body = body.Substring(0, body.Length - AtSite.Length);
        }

        lines.AddRange(body.Split(new[] { ", " }, System.StringSplitOptions.RemoveEmptyEntries));
        if (alternatives != null)
            lines.Add($"or {alternatives}");
        Show(heading, where ?? "You still need:", lines);
    }
}
