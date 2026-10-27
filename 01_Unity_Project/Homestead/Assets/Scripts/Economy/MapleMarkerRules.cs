using System;
using System.Collections.Generic;

// Maple D: the small droplet over each tappable Maple while a Spile is equipped, and a bucket over each tapped Maple at all times (the HUD side
// is MapleMarkerHud, and the minimap pins are in MinimapHud). Pure and engine-free so edit-mode tests cover it: which Maples get a marker, how
// it fades, what state a tap's marker shows, and when it shows at all. Positions, distances and screen numbers come in as plain floats.
public static class MapleMarkerRules
{
    public const int MaxMarkers = 30;                  // the nearest this many untapped Maples are marked
    public const float RadiusMetres = 80f;             // none beyond this
    public const float FadeStartMetres = 55f;          // full strength out to here, then fading to nothing at RadiusMetres
    public const float HideUnderMetres = 4f;           // closer than this the look prompt ("Sugar Maple, tappable") takes over
    public const float ClearAboveTreeMetres = 1f;      // the marker floats this far over the top of the tree
    public const float FallbackTreeHeightMetres = 12f; // used only if a tree prefab's height cannot be read (it reads as 0)
    public const float CrosshairClearPixels = 60f;     // a marker this close to the middle of the screen is hidden, so it never sits on the prompt
    public const float ScreenMargin = 0.05f;           // and one this close to a screen edge (share of the screen) is not drawn
    public const float RefreshSeconds = 0.25f;         // how often the nearest set is chosen again

    // The markers are on only with a Spile equipped, tools usable (not a menu, pause, sleep or placing something) and a tap still to spare:
    // at the cap a marker would promise a tap that the game refuses.
    public static bool ShouldShow(bool spileEquipped, bool canUseTools, int tapsOut) =>
        spileEquipped && canUseTools && MapleRules.CanAddTap(tapsOut);

    // Indices into positions of the nearest Maples to (px, pz), nearest first, at most max, flat (x, z) distance only. A Maple closer than
    // minMetres or farther than maxMetres is left out (both limits are inclusive: exactly on a limit counts), and so is any index excluded
    // returns true for (a tapped or felled tree). Equal distances go to the lower index, so the choice never flickers.
    public static List<int> Nearest(IReadOnlyList<(float x, float z)> positions, float px, float pz, float minMetres, float maxMetres, int max,
                                    Func<int, bool> excluded = null)
    {
        var found = new List<(float distance, int index)>();
        if (positions != null && max > 0)
        {
            for (int i = 0; i < positions.Count; i++)
            {
                if (excluded != null && excluded(i))
                    continue;
                float dx = positions[i].x - px, dz = positions[i].z - pz;
                float distance = (float)Math.Sqrt(dx * dx + dz * dz);
                if (distance < minMetres || distance > maxMetres)
                    continue;
                found.Add((distance, i));
            }
        }

        found.Sort((a, b) =>
        {
            int byDistance = a.distance.CompareTo(b.distance);
            return byDistance != 0 ? byDistance : a.index.CompareTo(b.index);
        });

        var result = new List<int>(Math.Min(Math.Max(max, 0), found.Count));
        for (int k = 0; k < found.Count && k < max; k++)
            result.Add(found[k].index);
        return result;
    }

    // The untapped marker's strength (0 to 1) at this distance: 1 out to FadeStartMetres, then a straight line down to 0 at RadiusMetres.
    public static float Fade(float distanceMetres) => Fade(distanceMetres, FadeStartMetres, RadiusMetres);

    // The same line for any pair of limits (the tap markers use TapFadeStartMetres and TapRadiusMetres).
    public static float Fade(float distanceMetres, float fadeStartMetres, float radiusMetres)
    {
        if (distanceMetres <= fadeStartMetres)
            return 1f;
        if (distanceMetres >= radiusMetres)
            return 0f;
        return (radiusMetres - distanceMetres) / (radiusMetres - fadeStartMetres);
    }

    // --- The markers over tapped Maples: always on, no Spile needed ---

    public const float TapRadiusMetres = 100f;    // a tapped tree is marked out to here
    public const float TapFadeStartMetres = 75f;  // full strength out to here, fading to nothing at TapRadiusMetres
    public const float TapHideUnderMetres = 4f;   // closer than this the tap's own prompt takes over

    // What a tap's marker shows: Empty and Filling look alike (neutral), Ready is the colour that says "go and collect", Sour is muted.
    public enum TapMarkerState { Empty, Filling, Ready, Sour }

    // Sour wins over everything (a soured bucket is not worth collecting, whatever it holds); then Ready at TapRules.ReadyLitres or more; then
    // Filling for any sap; else Empty.
    public static TapMarkerState StateOf(float litres, bool sour)
    {
        if (sour)
            return TapMarkerState.Sour;
        if (litres >= TapRules.ReadyLitres)
            return TapMarkerState.Ready;
        return litres > 0f ? TapMarkerState.Filling : TapMarkerState.Empty;
    }

    public static float TapFade(float distanceMetres) => Fade(distanceMetres, TapFadeStartMetres, TapRadiusMetres);

    // Whether a marker at this viewport point can be drawn. vx, vy: 0 to 1 across the screen; depth: metres in front of the camera (negative
    // or zero means behind it). Not behind the camera, not inside the edge margin, not within CrosshairClearPixels of the screen centre.
    public static bool ScreenVisible(float vx, float vy, float depth, float screenWidthPx, float screenHeightPx)
    {
        if (depth <= 0f)
            return false;
        if (vx < ScreenMargin || vx > 1f - ScreenMargin || vy < ScreenMargin || vy > 1f - ScreenMargin)
            return false;
        float dx = (vx - 0.5f) * screenWidthPx, dy = (vy - 0.5f) * screenHeightPx;
        return dx * dx + dy * dy >= CrosshairClearPixels * CrosshairClearPixels;
    }
}
