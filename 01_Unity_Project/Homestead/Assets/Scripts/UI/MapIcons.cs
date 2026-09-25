using System;
using System.Collections.Generic;
using UnityEngine;

// Minimap sprites drawn in code, so the map needs no imported art: Discovery_System.md's marker set (💧 Spring,
// 🫐 Berry Patch, 🦌 Deer Trail, 🎣 Fishing Hole, 🏠 Cabin Site — one per DiscoveryCategory; the UI font has no
// emoji), plus the round mask, rim and player arrow. Built once and cached.
public static class MapIcons
{
    const int IconSize = 64;
    const int Supersample = 4;

    static readonly Color Outline = new Color(0.09f, 0.12f, 0.09f, 1f);
    static readonly Color Glyph = new Color(0.95f, 0.92f, 0.82f, 1f);

    static readonly Dictionary<string, Sprite> markers = new Dictionary<string, Sprite>();
    static Sprite circle, ring, arrow;

    public static Color CategoryColor(DiscoveryCategory category)
    {
        switch (category)
        {
            case DiscoveryCategory.WaterSource: return new Color(0.3f, 0.56f, 0.76f);
            case DiscoveryCategory.Plant: return new Color(0.52f, 0.27f, 0.5f);
            case DiscoveryCategory.Wildlife: return new Color(0.55f, 0.4f, 0.26f);
            case DiscoveryCategory.Fishing: return new Color(0.24f, 0.55f, 0.5f);
            default: return new Color(0.78f, 0.56f, 0.24f);
        }
    }

    // icon picks a variant within the category — Foraging_System.md's 🍎 fruit, 🌰 nut, greens and 🍄 mushroom
    // for Plants; empty (or "berry") is the category's own glyph.
    public static Sprite Marker(DiscoveryCategory category, string icon = null)
    {
        string key = category + "/" + (icon ?? "");
        if (markers.TryGetValue(key, out Sprite sprite) && sprite != null)
            return sprite;

        Func<float, float, bool> glyph = GlyphFor(icon) ?? GlyphFor(category);
        Color fill = CategoryColor(category);
        sprite = Draw($"Marker {category}", IconSize, (x, y) =>
        {
            float r = Mathf.Sqrt(x * x + y * y);
            if (r > 1f) return Color.clear;
            if (r > 0.84f) return Outline;
            // The glyph sits inside the disc, scaled into its inner 60%.
            return glyph(x / 0.62f, y / 0.62f) ? Glyph : fill;
        });
        markers[key] = sprite;
        return sprite;
    }

    // Solid white disc, for the minimap's round mask.
    public static Sprite Circle => circle != null ? circle :
        circle = Draw("Minimap Circle", 256, (x, y) => x * x + y * y <= 1f ? Color.white : Color.clear);

    // Thin rim drawn over the minimap's edge.
    public static Sprite Ring => ring != null ? ring : ring = Draw("Minimap Ring", 256, (x, y) =>
    {
        float r = Mathf.Sqrt(x * x + y * y);
        return r <= 1f && r >= 0.955f ? Color.white : Color.clear;
    });

    // Upward-pointing arrow for the player; rotate it to the player's heading.
    public static Sprite Arrow => arrow != null ? arrow : arrow = Draw("Minimap Arrow", IconSize, (x, y) =>
    {
        if (InArrow(x / 0.8f, y / 0.8f)) return Glyph;
        if (InArrow(x / 0.98f, y / 0.98f)) return Outline;
        return Color.clear;
    });

    static bool InArrow(float x, float y) =>
        InTriangle(x, y, new Vector2(0f, 1f), new Vector2(-0.75f, -0.8f), new Vector2(0f, -0.4f)) ||
        InTriangle(x, y, new Vector2(0f, 1f), new Vector2(0f, -0.4f), new Vector2(0.75f, -0.8f));

    static Func<float, float, bool> GlyphFor(string icon)
    {
        switch (icon)
        {
            // Apple: round fruit with a stem and leaf.
            case "fruit":
                return (x, y) => InCircle(x, y, -0.2f, -0.12f, 0.42f) || InCircle(x, y, 0.2f, -0.12f, 0.42f) ||
                                 (Mathf.Abs(x) < 0.06f && y > 0.2f && y < 0.62f) || InEllipse(x, y, 0.28f, 0.52f, 0.22f, 0.1f);
            // Acorn/nut: a cap over a rounded body.
            case "nut":
                return (x, y) => InEllipse(x, y, 0f, -0.2f, 0.36f, 0.5f) || (InEllipse(x, y, 0f, 0.28f, 0.55f, 0.26f) && y > 0.2f) ||
                                 (Mathf.Abs(x) < 0.06f && y > 0.45f && y < 0.75f);
            // Leaf on a stem, for wild greens.
            case "greens":
                return (x, y) =>
                {
                    float u = (x + y) * 0.7071f, v = (y - x) * 0.7071f; // rotated 45°
                    return InEllipse(u, v, 0.1f, 0f, 0.62f, 0.3f) || (Mathf.Abs(x + y + 0.9f) < 0.09f && x < -0.2f && y < -0.2f);
                };
            // Mushroom: a domed cap on a stem.
            case "mushroom":
                return (x, y) => (InEllipse(x, y, 0f, 0.08f, 0.72f, 0.5f) && y > 0.08f) || (Mathf.Abs(x) < 0.2f && y > -0.72f && y <= 0.1f);
            default:
                return null;
        }
    }

    static Func<float, float, bool> GlyphFor(DiscoveryCategory category)
    {
        switch (category)
        {
            // Water drop: a round base narrowing to a point.
            case DiscoveryCategory.WaterSource:
                return (x, y) => InCircle(x, y, 0f, -0.25f, 0.5f) ||
                                 InTriangle(x, y, new Vector2(0f, 0.85f), new Vector2(-0.45f, -0.1f), new Vector2(0.45f, -0.1f));
            // Berry cluster with a stem.
            case DiscoveryCategory.Plant:
                return (x, y) => InCircle(x, y, -0.3f, -0.3f, 0.33f) || InCircle(x, y, 0.3f, -0.3f, 0.33f) ||
                                 InCircle(x, y, 0f, 0.18f, 0.33f) ||
                                 (Mathf.Abs(x - 0.05f) < 0.07f && y > 0.4f && y < 0.85f);
            // Deer track: the two halves of a cloven hoof.
            case DiscoveryCategory.Wildlife:
                return (x, y) => InEllipse(x, y, -0.25f, 0f, 0.2f, 0.62f) || InEllipse(x, y, 0.25f, 0f, 0.2f, 0.62f);
            // Fish: body and tail.
            case DiscoveryCategory.Fishing:
                return (x, y) => InEllipse(x, y, -0.15f, 0f, 0.58f, 0.34f) ||
                                 InTriangle(x, y, new Vector2(0.3f, 0f), new Vector2(0.85f, 0.42f), new Vector2(0.85f, -0.42f));
            // Cabin: walls and roof.
            default:
                return (x, y) => (Mathf.Abs(x) < 0.48f && y > -0.7f && y < 0.05f) ||
                                 InTriangle(x, y, new Vector2(0f, 0.8f), new Vector2(-0.72f, 0.05f), new Vector2(0.72f, 0.05f));
        }
    }

    static bool InCircle(float x, float y, float cx, float cy, float r) => (x - cx) * (x - cx) + (y - cy) * (y - cy) <= r * r;

    static bool InEllipse(float x, float y, float cx, float cy, float rx, float ry)
    {
        float u = (x - cx) / rx, v = (y - cy) / ry;
        return u * u + v * v <= 1f;
    }

    static bool InTriangle(float x, float y, Vector2 a, Vector2 b, Vector2 c)
    {
        float d1 = Cross(x, y, a, b), d2 = Cross(x, y, b, c), d3 = Cross(x, y, c, a);
        bool negative = d1 < 0f || d2 < 0f || d3 < 0f, positive = d1 > 0f || d2 > 0f || d3 > 0f;
        return !(negative && positive);
    }

    static float Cross(float x, float y, Vector2 a, Vector2 b) => (x - b.x) * (a.y - b.y) - (a.x - b.x) * (y - b.y);

    // Rasterises shape(x, y) over -1..1 with supersampling for smooth edges.
    static Sprite Draw(string name, int size, Func<float, float, Color> shape)
    {
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = name,
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            hideFlags = HideFlags.DontSave,
        };

        var pixels = new Color[size * size];
        const float Samples = Supersample * Supersample;
        for (int py = 0; py < size; py++)
        {
            for (int px = 0; px < size; px++)
            {
                Color sum = Color.clear;
                for (int sy = 0; sy < Supersample; sy++)
                {
                    for (int sx = 0; sx < Supersample; sx++)
                    {
                        float x = ((px + (sx + 0.5f) / Supersample) / size) * 2f - 1f;
                        float y = ((py + (sy + 0.5f) / Supersample) / size) * 2f - 1f;
                        Color c = shape(x, y);
                        sum += new Color(c.r * c.a, c.g * c.a, c.b * c.a, c.a); // premultiply so edges don't darken
                    }
                }

                float a = sum.a / Samples;
                pixels[py * size + px] = a > 0f ? new Color(sum.r / sum.a, sum.g / sum.a, sum.b / sum.a, a) : Color.clear;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, true);
        var sprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        sprite.name = name;
        sprite.hideFlags = HideFlags.DontSave;
        return sprite;
    }
}
