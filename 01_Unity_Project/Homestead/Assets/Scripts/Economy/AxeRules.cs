using System;

// The felling tools' arithmetic as pure, engine-free rules so edit-mode tests cover it (AxeTool has no tests of its own: it needs a scene).
// The Wood Saw (Mike, 2026-10-31) is a bought tool, a step above the Axe: it fells and cuts up wood in 0.65 times the blows or seconds
// the Axe needs. The factors themselves are tunables on the AxeTool component; this holds the ids, the Trading Post figures and the sums.
//
// A tool's factor multiplies blows (felling, splitting at a pile) and seconds (splitting a carried Log or Branches): the Axe is 1, the
// Primitive Axe 1.5 (slower), the Wood Saw 0.65 (faster). Equipped, the tool in hand sets the blows. Carried, the fastest felling tool in the
// pack sets the Inventory's splitting speed, so an Axe plus a Saw splits at the Saw's pace and a Primitive Axe plus an Axe at the Axe's.
public static class AxeRules
{
    public const string AxeId = "axe";
    public const string PrimitiveAxeId = "primitive_axe";
    public const string SawId = "wood_saw";

    // The Trading Post's price: 1.5 times the Axe ($20.00); it sells back at 40% ($12.00).
    public const long SawBaseCents = 3000;
    public const float SawWeightKg = 2f;

    public const float DefaultPrimitiveFactor = 1.5f;
    public const float DefaultSawFactor = 0.65f;

    // Whether this tool fells trees and splits wood at a pile when equipped.
    public static bool IsFellingTool(string toolId) => toolId == AxeId || toolId == PrimitiveAxeId || toolId == SawId;

    // The blows multiplier of an equipped tool: 1 for the Axe (and for anything that is not a felling tool).
    public static float FactorFor(string toolId, float primitiveFactor, float sawFactor) =>
        toolId == PrimitiveAxeId ? primitiveFactor : toolId == SawId ? sawFactor : 1f;

    // The factor of the fastest felling tool carried, or 0 when none is (nothing to split with).
    public static float FastestCarried(bool axe, bool primitiveAxe, bool saw, float primitiveFactor, float sawFactor)
    {
        float best = 0f;
        if (primitiveAxe)
            best = primitiveFactor;
        if (axe && (best <= 0f || 1f < best))
            best = 1f;
        if (saw && (best <= 0f || sawFactor < best))
            best = sawFactor;
        return best;
    }

    // A tree's size: how tall it is times the square root of how wide (the sum WoodManager.YieldOf and the blows both use).
    public static float TreeSize(float heightScale, float widthScale) => heightScale * (float)Math.Sqrt(widthScale);

    // Blows to fell a tree: the base for its kind at size 1, scaled by its size and the tool, never fewer than 1.
    public static float FellBlows(float blowsAtSize1, float size, float factor) =>
        Math.Max(1f, (float)Math.Round(blowsAtSize1 * size * factor));

    // Blows to split one Log (or lot of Branches) at a wood pile with this tool.
    public static int SplitBlows(int baseBlows, float factor) => Math.Max(1, (int)Math.Round(baseBlows * factor));

    // Seconds to split one carried Log or lot of Branches from the Inventory.
    public static float SplitSeconds(float baseSeconds, float factor) => baseSeconds * factor;

    // The stamina one tree costs: a blow lands per swing (work efficiency 1 lets each swing count as a whole blow), at a fixed cost each.
    public static float StaminaForBlows(float blows, float staminaPerSwing) => blows * staminaPerSwing;
}
