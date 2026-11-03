// SeasonalGround's snow and leaf litter weights, pure so edit-mode tests can pin them (Mike, 2026-11-04: the creek banks keep only a share of both).
//
// Snow lies over litter, which lies over the terrain's own painting (the banks are part of that painting); the weights always add up to 1. On ground with no bank the result is
// exactly what SeasonalGround always painted.
public static class GroundCoverRules
{
    // The two layers SeasonalGround adds at play time (Leaf Litter, Snow), and TerrainLit's most layers in one pass (_TERRAIN_8_LAYERS: 8, otherwise 4).
    public const int SeasonalLayers = 2;
    public const int MaxTerrainLayers = 8;

    // Testing only (TimeDebugControls, Ctrl+Home): the ground snow amount steps 0, 0.5, 1, then back to 0. Any amount in between goes to the next step up.
    public const float DebugSnowHoldGameHours = 24f;

    public static float NextDebugSnow(float current)
    {
        if (current < 0.25f)
            return 0.5f;
        return current < 0.75f ? 1f : 0f;
    }

    // snowWanted / litterWanted: the cover this spot would get with no bank (the snow amount times its shelter mask, the litter amount times its canopy mask).
    // bank: 0 to 1, how much of the spot's own painting is Wet Clay or Creek Sand. A full bank spot keeps only snowShare of the snow and litterShare of the litter.
    // keep: what to multiply every one of the terrain's own layers by. litter and snow: the weights of the two cover layers.
    public static void Cover(float snowWanted, float litterWanted, float bank, float snowShare, float litterShare, out float keep, out float litter, out float snow)
    {
        float snowHere = snowWanted * (1f - bank * (1f - snowShare));
        float leavesHere = litterWanted * (1f - bank * (1f - litterShare));
        keep = (1f - snowHere) * (1f - leavesHere);
        litter = leavesHere * (1f - snowHere);
        snow = snowHere;
    }
}
