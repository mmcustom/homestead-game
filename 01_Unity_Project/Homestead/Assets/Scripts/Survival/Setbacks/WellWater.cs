// Well Water Table, B1 (Well_Water_Table.md v0.3): how deep the water is under a spot, and what that depth does to a dug well. Pure maths over
// two heights, no scene access, so the edit-mode tests can run it. Nothing calls it yet (B2 is the placement preview, B3 the effects).
// Every number is first pass and lives in WellWaterRules.Defaults, the one table to tune; the WellsAndOuthouses component will copy them in
// later, the way it does for SetbackRules.
public enum WellTier
{
    Legacy,   // a well from before depth existed (saved depthToWater 0): today's fixed numbers
    Shallow,  // under 3 m: low ground near water
    Medium,   // 3 m up to 6 m
    Deep,     // 6 m up to the cap
    TooDeep,  // past the cap: not diggable by hand
}

// What one tier gives a well.
public struct WellTierNumbers
{
    public int digBlows;                   // Shovel blows to dig the shaft (about 0.9 s each, real time)
    public int liningStone;                // Stone to line it (5 kg each)
    public int bufferLitres;               // what the well holds
    public float recoveryLitresPerHour;
    public float droughtRecoveryLitresPerHour;
}

public struct WellWaterRules
{
    public float baseDepthMetres;          // depth at the water's own edge
    public float slopeMetresPerMetre;      // extra depth for every metre the ground stands above the nearest permanent water
    public float shallowBelowMetres;       // under this: Shallow
    public float deepFromMetres;           // from this: Deep
    public float capMetres;                // deeper than this: TooDeep
    public WellTierNumbers shallow, medium, deep, legacy;

    // Medium and Legacy are the numbers the Well has always had (WellsAndOuthouses: 40 blows, 10 Stone, 150 L, 6 L per hour, 3 in a drought).
    // Deep is Mike's decision of 2026-10-24 (60 blows, 14 Stone, 120 L, 4.5 L per hour and the same in a drought).
    public static WellWaterRules Defaults => new WellWaterRules
    {
        baseDepthMetres = 1.5f, slopeMetresPerMetre = 0.5f, shallowBelowMetres = 3f, deepFromMetres = 6f, capMetres = 9f,
        shallow = new WellTierNumbers { digBlows = 20, liningStone = 6, bufferLitres = 200, recoveryLitresPerHour = 8f, droughtRecoveryLitresPerHour = 4f },
        medium = new WellTierNumbers { digBlows = 40, liningStone = 10, bufferLitres = 150, recoveryLitresPerHour = 6f, droughtRecoveryLitresPerHour = 3f },
        deep = new WellTierNumbers { digBlows = 60, liningStone = 14, bufferLitres = 120, recoveryLitresPerHour = 4.5f, droughtRecoveryLitresPerHour = 4.5f },
        legacy = new WellTierNumbers { digBlows = 40, liningStone = 10, bufferLitres = 150, recoveryLitresPerHour = 6f, droughtRecoveryLitresPerHour = 3f },
    };
}

// The answer for one spot. For a TooDeep spot the depth is the real (uncapped) figure and the numbers are all 0: there is nothing to dig.
public readonly struct WellWaterResult
{
    public readonly float depthToWater;
    public readonly WellTier tier;
    public readonly int digBlows;
    public readonly int liningStone;
    public readonly int bufferLitres;
    public readonly float recoveryLitresPerHour;
    public readonly float droughtRecoveryLitresPerHour;

    public WellWaterResult(float depth, WellTier tier, WellTierNumbers numbers)
    {
        depthToWater = depth;
        this.tier = tier;
        digBlows = numbers.digBlows;
        liningStone = numbers.liningStone;
        bufferLitres = numbers.bufferLitres;
        recoveryLitresPerHour = numbers.recoveryLitresPerHour;
        droughtRecoveryLitresPerHour = numbers.droughtRecoveryLitresPerHour;
    }

    // Whether a well can be hand-dug here (Legacy wells exist already, so they count).
    public bool Diggable => tier != WellTier.TooDeep;
}

public static class WellWater
{
    // Depth to water at a spot: base depth plus the slope for every metre the ground stands above the nearest permanent water surface
    // (creek, pond or Spring Hollow). Ground at or below the water gets the base depth.
    public static float DepthToWater(float groundHeight, float waterHeight, WellWaterRules rules)
    {
        float above = groundHeight - waterHeight;
        return rules.baseDepthMetres + rules.slopeMetresPerMetre * (above > 0f ? above : 0f);
    }

    public static WellWaterResult Compute(float groundHeight, float waterHeight) => Compute(groundHeight, waterHeight, WellWaterRules.Defaults);

    public static WellWaterResult Compute(float groundHeight, float waterHeight, WellWaterRules rules) =>
        ForDepth(DepthToWater(groundHeight, waterHeight, rules), rules);

    // The tier and numbers for a depth. A saved depth of 0 (or less) is an old well: Legacy, today's fixed numbers. Exactly the shallow line
    // is Medium, exactly the deep line is Deep, exactly the cap is still Deep; only past the cap is it TooDeep.
    public static WellWaterResult ForDepth(float depth) => ForDepth(depth, WellWaterRules.Defaults);

    public static WellWaterResult ForDepth(float depth, WellWaterRules rules)
    {
        if (depth <= 0f)
            return new WellWaterResult(0f, WellTier.Legacy, rules.legacy);
        if (depth > rules.capMetres)
            return new WellWaterResult(depth, WellTier.TooDeep, default);
        if (depth < rules.shallowBelowMetres)
            return new WellWaterResult(depth, WellTier.Shallow, rules.shallow);
        if (depth < rules.deepFromMetres)
            return new WellWaterResult(depth, WellTier.Medium, rules.medium);
        return new WellWaterResult(depth, WellTier.Deep, rules.deep);
    }

    // The old-well result, for a well saved before depth existed.
    public static WellWaterResult Legacy() => ForDepth(0f);
}
