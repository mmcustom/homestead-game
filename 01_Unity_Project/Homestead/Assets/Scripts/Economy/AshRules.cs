using System;

// Early_Income_Design.md section 1: burning fuel leaves Ash, which the player collects from the fire and sells raw. Pure and engine-free so
// edit-mode tests cover it. The Ash a fire will leave is kept as a fraction ("owed" while the fuel is still in the ring, "made" once it has
// burned), so burning one piece at a time still adds up: three Firewood burned in three separate burns leave exactly 1 Ash.
//
// Every number here is a named constant. The yields are per piece of fuel, set so that Ash follows burn time in line with Firewood
// (a Log is 5 Firewood; 2 Branches are 1 Firewood), except Sticks, which are deliberately fewer Ash than their burn time: 1 per 12.
public static class AshRules
{
    public const string ItemId = "ash";
    public const string KindlingItemId = "kindling_bundle";

    // Fuel item ids (the same ids FireManager and WoodManager use).
    public const string FirewoodId = "firewood";
    public const string BranchesId = "branches";
    public const string SticksId = "sticks";
    public const string LogsId = "logs";

    // How much fuel leaves one Ash.
    public const float FirewoodPerAsh = 3f;   // 1 Ash per 3 Firewood
    public const float BranchesPerAsh = 6f;   // 1 Ash per 6 Branches (a Firewood is 2 Branches)
    public const float SticksPerAsh = 12f;    // 1 Ash per 12 Sticks (Mike, 2026-10-25; not in line with burn time on purpose)
    public const float AshPerLog = 5f / 3f;   // a Log is 5 Firewood: about 1.67 Ash each

    // A whole Ash is counted once the made amount is within this of the next whole number (float noise from many small burns).
    public const float Epsilon = 0.0001f;

    // The market: Ash is its own group, sold raw at about $2, 6% off per unit, floor 0.25 of normal, the same price all year. Recovery is the
    // market's global 25% of the gap per game day.
    public const long BaseCents = 200;
    public const int DropBp = 600;
    public const int FloorBp = 2500;

    // The Kindling Bundle: 10 Sticks crafted into 1, sold in the Crafted goods group at about $1.
    public const int SticksPerBundle = 10;
    public const long KindlingBaseCents = 100;

    // Ash one piece of this fuel leaves; 0 for anything that does not burn for Ash.
    public static float AshPerPiece(string itemId)
    {
        switch (itemId)
        {
            case FirewoodId: return 1f / FirewoodPerAsh;
            case BranchesId: return 1f / BranchesPerAsh;
            case SticksId: return 1f / SticksPerAsh;
            case LogsId: return AshPerLog;
            default: return 0f;
        }
    }

    // Ash that count pieces of this fuel will leave once burned.
    public static float Owed(string itemId, int count) => count > 0 ? AshPerPiece(itemId) * count : 0f;

    // The share of the Ash still owed that this stretch of burning makes: all of it when the fuel runs out, otherwise in proportion to the
    // hours burned out of the hours the fire held.
    public static float Release(float owed, float fuelHoursBefore, float burnedHours)
    {
        if (owed <= 0f || burnedHours <= 0f)
            return 0f;
        if (fuelHoursBefore <= 0f || burnedHours >= fuelHoursBefore)
            return owed;
        return owed * (burnedHours / fuelHoursBefore);
    }

    // Whole Ash ready to collect from an amount made.
    public static int Whole(float made) => made <= 0f ? 0 : (int)Math.Floor(made + Epsilon);

    // What is left in the pan after taking some: the fraction carries on to the next burn.
    public static float AfterCollect(float made, int taken)
    {
        float left = made - Math.Max(0, taken);
        return left < Epsilon ? 0f : left;
    }
}
