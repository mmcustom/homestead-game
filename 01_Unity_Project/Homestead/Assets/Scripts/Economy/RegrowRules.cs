using System;

// Tree Regrowth (Mike, 2026-10-31; Tree_Regrowth.md) as pure, engine-free rules so edit-mode tests cover it. WoodManager holds the state and the terrain; this holds the
// calendar, the growth curve and the obstruction numbers.
//
// A felled spot keeps one stamp (the day it was felled plus one; 0 means a save from before this existed). The days since then give its stage:
//   0          Cut: before day 14 there is no tree (a hardwood leaves its stump).
//   1 to 14    Growing: from day 14 a scaled tree stands there, growing in 7-day steps.
//   15 (Full)  At day 112 (one game year) the original tree is back and the spot is forgotten.
// A hardwood whose stump was dug out is Cleared for good (CanRegrow). Growth g runs from 0.08 at the sprout to 1 at the full year; height and width are both
// scaled by g^(2/3), so a tree's size (height times the square root of its width) is g times the full size: blows and Logs follow it in proportion.
public static class RegrowRules
{
    public const int ShrubPrototype = 2;

    public const int SproutDays = 14;
    public const int FullDays = 112;
    public const int StepDays = 7;
    public const int LastGrowingStage = 14;
    public const int FullStage = LastGrowingStage + 1;

    public const float SproutGrowth = 0.08f;
    // A standing stump is swallowed by the growing tree from this growth on; Windthrow only takes trees at least this grown.
    public const float StumpGoneGrowth = 0.5f;
    public const float WindthrowGrowth = 0.75f;

    // Obstruction at the sprout moment (metres): a built structure, a fire and the player. The Cabin's and the Kitchen's come from WoodManager's own tunables.
    public const float StructureMetres = 1.5f;
    public const float FireMetres = 2f;
    public const float PlayerMetres = 1.5f;

    // The Workshop (about 3 m by 2.4 m) is wider than an ordinary structure: no sprout comes up inside its footprint (Workshop W1(a1)).
    public const float WorkshopMetres = 2.9f;
    // The Potter's Shed (about 2.4 m by 2.0 m) likewise (Pottery P1).
    public const float PotteryMetres = 2.4f;
    // The Kiln (a clay-and-stone dome about 2.4 m across) likewise (Pottery P2).
    public const float KilnMetres = PotteryRules.RegrowMetres;

    // The Charcoal Pit (an earth mound about 1.5 m across; Metalworking M1) likewise.
    public const float PitMetres = CharcoalRules.RegrowMetres;

    // The Bloomery (a stone-and-clay stack about 0.9 m across; Metalworking M2) likewise.
    public const float BloomeryMetres = BloomeryRules.RegrowMetres;

    // The Smithy (a roofed shed about 3 m by 2.4 m, the Workshop's size; Metalworking M3) likewise.
    public const float SmithyMetres = SmithyRules.RegrowMetres;

    public enum Blocker { Structure, Cabin, Kitchen, Fire, Player, ClearedGround, Workshop, Pottery, Kiln, Pit, Bloomery, Smithy }

    // Whether a felled spot can ever grow again: a shrub always; a hardwood only while its stump is still there (a dug-out stump clears the spot for good).
    public static bool CanRegrow(int prototype, bool stump) => prototype == ShrubPrototype || stump;

    // The stamp to keep: the saved one, or for a save from before regrowth the first day it is seen (so old stumps start their year then).
    public static int StampFor(int saved, int today) => saved > 0 ? saved : today + 1;

    public static int DaysSince(int stamp, int today) => Math.Max(0, today - (stamp - 1));

    // A stamp that holds a blocked sprout at day 13: its clock waits until the spot is clear.
    public static int HeldStamp(int today) => today - (SproutDays - 1) + 1;

    // A stamp that makes a spot a whole year old today (the dev "regrow now").
    public static int FullStamp(int today) => today - FullDays + 1;

    public static int Stage(int days)
    {
        if (days < SproutDays)
            return 0;
        if (days >= FullDays)
            return FullStage;
        return 1 + (days - SproutDays) / StepDays;
    }

    // 0 for a cut spot, 1 for a full tree, otherwise 0.08 rising in even steps (stage 1 is the sprout, stage 14 is 0.934).
    public static float GrowthFraction(int stage)
    {
        if (stage <= 0)
            return 0f;
        if (stage >= FullStage)
            return 1f;
        return SproutGrowth + (1f - SproutGrowth) * (stage - 1) / LastGrowingStage;
    }

    // The scale of both height and width for a growth: size (height times the square root of width) is then g times the full size.
    public static float ScaleOf(float growth) => growth >= 1f ? 1f : (float)Math.Pow(Math.Max(0f, growth), 2.0 / 3.0);

    public static bool StumpGone(float growth) => growth >= StumpGoneGrowth;
    public static bool WindthrowEligible(float growth) => growth >= WindthrowGrowth;

    // Logs a felled tree gives: proportional to its size, with no forced minimum (a seedling gives none), so cutting saplings is no quicker per Log.
    public static int LogsFor(float logsAtSize1, float size) => Math.Max(0, (int)Math.Round(logsAtSize1 * size));

    // The distance within which this blocks a sprout.
    public static float BlockRadius(Blocker kind, float cabinMetres, float kitchenMetres, float zoneMetres = 0f)
    {
        switch (kind)
        {
            case Blocker.Cabin: return cabinMetres;
            case Blocker.Kitchen: return kitchenMetres;
            case Blocker.Fire: return FireMetres;
            case Blocker.Player: return PlayerMetres;
            case Blocker.ClearedGround: return zoneMetres;
            case Blocker.Workshop: return WorkshopMetres;
            case Blocker.Pottery: return PotteryMetres;
            case Blocker.Kiln: return KilnMetres;
            case Blocker.Pit: return PitMetres;
            case Blocker.Bloomery: return BloomeryMetres;
            case Blocker.Smithy: return SmithyMetres;
            default: return StructureMetres;
        }
    }

    public static bool Blocks(Blocker kind, float distance, float cabinMetres, float kitchenMetres, float zoneMetres = 0f) =>
        distance < BlockRadius(kind, cabinMetres, kitchenMetres, zoneMetres);
}
