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

    // --- The Kettle's Lye row in the Inventory, and the words ---

    // Boiling waits on the same gate the Pot's boils use (TripodRules.Gate), but the Kettle is the vessel. lye and firewood are what is carried; a
    // batch takes LyePerPotash and FirewoodPerPotash.
    public static string RowInfo(TripodRules.Gate gate, int lye, int firewood)
    {
        switch (gate)
        {
            case TripodRules.Gate.NoFire: return "needs a burning campfire";
            case TripodRules.Gate.NoTripod:
            case TripodRules.Gate.NoVessel: return "needs the Potash Kettle hung on a tripod";
            case TripodRules.Gate.WrongVessel: return "hang the Potash Kettle (another vessel is on the tripod)";
        }

        int batches = PotashBatches(lye, firewood);
        if (batches <= 0)
            return NeedMessage(lye, firewood);
        int lyeLeft = lye - LyeFor(batches), firewoodLeft = firewood - FirewoodFor(batches);
        string left = lyeLeft > 0 || firewoodLeft > 0 ? $" ({lyeLeft} Lye, {firewoodLeft} Firewood left)" : "";
        return $"{LyeFor(batches)} Lye + {FirewoodFor(batches)} Firewood -> {batches} Potash{left}";
    }

    public static string NeedMessage(int lye, int firewood) =>
        $"needs {LyePerPotash} Lye and {FirewoodPerPotash} Firewood (have {Math.Max(0, lye)} Lye and {Math.Max(0, firewood)} Firewood)";

    public static string BoiledMessage(int potash) =>
        $"Boiled {LyeFor(potash)} Lye and {FirewoodFor(potash)} Firewood into {potash} Potash";

    // --- The leach hopper: what a load takes ---

    // The Hopper site: 3 Logs and 4 Tall Grass (24.4 kg, so a staged site), finished with the Hammer.
    public const int HopperLogs = 3;
    public const int HopperTallGrass = 4;
    public const int HopperBuildBlows = 20;

    // The raw and boiled water a load can take, worst first: the poorest water goes into the hopper and the best is kept to drink.
    public static readonly string[] WaterWorstFirst = { "water_unsafe", "water_questionable", "water_good", "water_excellent", "water" };

    public struct LoadPlan
    {
        public int lye;      // Lye the load will make
        public int ash;      // Ash it takes
        public int[] water;  // litres taken from each of WaterWorstFirst, in that order
    }

    // What one R press loads: up to the room in the hopper, limited by the Ash and the litres carried. waterCarried lines up with WaterWorstFirst.
    public static LoadPlan PlanLoad(int ash, int[] waterCarried, int room)
    {
        int total = 0;
        if (waterCarried != null)
            foreach (int litres in waterCarried)
                total += Math.Max(0, litres);

        var plan = new LoadPlan { lye = LyeBatch(ash, total, room), water = new int[WaterWorstFirst.Length] };
        plan.ash = AshFor(plan.lye);
        int still = WaterFor(plan.lye);
        for (int i = 0; i < plan.water.Length && still > 0 && waterCarried != null && i < waterCarried.Length; i++)
        {
            plan.water[i] = Math.Min(Math.Max(0, waterCarried[i]), still);
            still -= plan.water[i];
        }
        return plan;
    }

    // Room left in a hopper holding this many pieces (the Ash waiting to leach and the Lye waiting to be collected).
    public static int Room(int held) => Math.Max(0, HopperCapacity - Math.Max(0, held));

    // One game hour of leaching. hoursLeft is 0 when no batch is running.
    public static float LeachAfterHour(float hoursLeft) => hoursLeft > 0f ? Math.Max(0f, hoursLeft - 1f) : 0f;

    // True on the hour a running batch finishes (it was running and now is not).
    public static bool FinishedThisHour(float before, float after) => before > 0f && after <= 0f;

    public static string HoursText(float hoursLeft) => $"{Math.Max(0, (int)Math.Ceiling(hoursLeft - 0.0001f))} h";

    public static string LoadedMessage(int lye, int ash, int litres) =>
        $"Loaded {ash} Ash and {litres} L of water - {lye} Lye in {HoursText(LeachGameHours)}";

    public static string LoadRefusedWhileLeaching(float hoursLeft) =>
        $"The hopper is still leaching - {HoursText(hoursLeft)} to go. Collect the Lye when it is done.";

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
