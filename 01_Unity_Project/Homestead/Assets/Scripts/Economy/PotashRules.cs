using System;

// Early_Income_Design.md section 1 (Potash, decided by Mike 2026-10-27): Ash leached with water makes Lye, and Lye boiled down with Firewood in
// the Potash Kettle makes Potash, which sells for far more per Ash than the Ash does raw. Pure and engine-free so edit-mode tests cover it; the
// leach hopper and the Kettle boil in the world (Potash B) call these. Nothing here touches a fire, a hopper, the pack or the clock: what is
// carried, what is in the hopper and how many game hours have passed come in as plain numbers.
//
// Every number is a named constant. The Kettle's id is TripodRules.KettleId (it hangs on a tripod like the Pot), so it is not repeated here.
public static class PotashRules
{
    public const string LyeId = "lye";
    public const string PotashId = "potash";

    // --- Leaching: Ash and water in the hopper make Lye ---

    public const int AshPerLye = 1;
    public const int WaterLitresPerLye = 1;

    // The hopper holds this many Lye worth of Ash and water at once, and a batch takes this many game hours to drip through.
    public const int HopperCapacity = 12;
    public const float LeachGameHours = 24f;

    // The most Lye a hopper can start now: limited by the Ash carried, the litres of water carried and the room left in the hopper.
    public static int LyeBatch(int ash, int waterLitres, int room)
    {
        if (ash <= 0 || waterLitres <= 0 || room <= 0)
            return 0;
        return Math.Min(Math.Min(ash / AshPerLye, waterLitres / WaterLitresPerLye), room);
    }

    public static int AshFor(int lye) => lye > 0 ? lye * AshPerLye : 0;

    public static int WaterFor(int lye) => lye > 0 ? lye * WaterLitresPerLye : 0;

    // Hours left until a batch that began leachedHours ago is ready (0 once done). A negative elapsed time counts as none.
    public static float LeachHoursLeft(float hoursElapsed) => Math.Max(0f, LeachGameHours - Math.Max(0f, hoursElapsed));

    public static bool IsLeached(float hoursElapsed) => hoursElapsed >= LeachGameHours;

    // --- Boiling: Lye and Firewood in the Kettle make Potash ---

    public const int LyePerPotash = 3;
    public const int FirewoodPerPotash = 2;   // an ingredient burned away in the boil, not fuel for the fire

    // One batch of Potash takes this long at a lit fire, 6 s for each of its Lye.
    public const float SecondsPerLye = 6f;
    public const float BoilSeconds = LyePerPotash * SecondsPerLye;   // 18

    // Whole batches the Lye and Firewood carried allow.
    public static int PotashBatches(int lye, int firewood)
    {
        if (lye < LyePerPotash || firewood < FirewoodPerPotash)
            return 0;
        return Math.Min(lye / LyePerPotash, firewood / FirewoodPerPotash);
    }

    public static int LyeFor(int batches) => batches > 0 ? batches * LyePerPotash : 0;

    public static int FirewoodFor(int batches) => batches > 0 ? batches * FirewoodPerPotash : 0;

    public static float SecondsFor(int batches) => batches > 0 ? batches * BoilSeconds : 0f;

    // --- The market ---

    // Lye and Potash are one group, Potash, each item its own pool: 5% off per unit, floor 0.25 of normal, the same price all year. Recovery
    // is the market's global 25% of the gap per game day. Prices in cents.
    public const long LyeBaseCents = 300;
    public const long PotashBaseCents = 1600;
    public const int DropBp = 500;
    public const int FloorBp = 2500;

    // The Potash Kettle: a Trading Post item that sells back at the usual 40%.
    public const long KettleBaseCents = 2500;

    // What one Ash is worth made into Lye, and made into Potash (3 Lye, so 3 Ash), at the base prices, in cents.
    public static long LyeCentsPerAsh => LyeBaseCents / AshPerLye;
    public static long PotashCentsPerAsh => PotashBaseCents / (LyePerPotash * AshPerLye);
}
