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

    // --- Stocking: Ash and water go in separately and E starts the batch (Mike, 2026-11-04) ---
    //
    // The Ash (the running batch and any stock waiting for the next one) is in the hopper's contents; the Lye ready is there too. The stored water is its own saved litre count
    // (WoodPileState.hopperWater) and the running batch's size is hopperBatch (0 means the whole Ash count, as a save from before the field had it).

    // The most water the hopper stores, in litres. The Ash and Lye share HopperCapacity.
    public const int HopperWaterCapacity = 12;

    public static int WaterRoom(int litresStored) => Math.Max(0, HopperWaterCapacity - Math.Max(0, litresStored));

    // The Lye the running batch will make: the saved batch size, or every Ash in the hopper when none was saved (an old save mid-batch), never more than the Ash there.
    public static int RunningBatch(int ashHeld, int savedBatch)
    {
        if (ashHeld <= 0)
            return 0;
        return savedBatch > 0 ? Math.Min(savedBatch, ashHeld) : ashHeld;
    }

    // The Ash waiting for the next batch: all of it when nothing is leaching, the Ash beyond the running batch when something is.
    public static int AshStock(int ashHeld, bool leaching, int savedBatch) =>
        leaching ? Math.Max(0, ashHeld - RunningBatch(ashHeld, savedBatch)) : Math.Max(0, ashHeld);

    // What E starts: as many Lye as the Ash stock and the stored litres allow, up to the capacity. The leftover stays as stock.
    public static int NextBatch(int ashStock, int litresStored)
    {
        if (ashStock <= 0 || litresStored <= 0)
            return 0;
        return Math.Min(Math.Min(ashStock / AshPerLye, litresStored / WaterLitresPerLye), HopperCapacity);
    }

    public struct StockPlan
    {
        public int ash;      // Ash taken from the pack
        public int litres;   // litres taken from the pack, all kinds
        public int[] water;  // litres taken from each of WaterWorstFirst, in that order
    }

    // What one R press stocks: the Ash carried up to the Ash room, and the water carried (worst first) up to the water room. waterCarried lines up with WaterWorstFirst.
    public static StockPlan PlanStock(int ashCarried, int[] waterCarried, int ashRoom, int waterRoom)
    {
        var plan = new StockPlan { water = new int[WaterWorstFirst.Length] };
        plan.ash = Math.Max(0, Math.Min(ashCarried, ashRoom));
        int still = Math.Max(0, waterRoom);
        for (int i = 0; i < plan.water.Length && still > 0 && waterCarried != null && i < waterCarried.Length; i++)
        {
            plan.water[i] = Math.Min(Math.Max(0, waterCarried[i]), still);
            still -= plan.water[i];
            plan.litres += plan.water[i];
        }
        return plan;
    }

    public enum HopperAction { CollectLye, StillLeaching, Start, NeedAsh, NeedWater, NeedBoth }

    // What E does: Lye ready is collected first (even while a batch leaches or stock waits); otherwise a batch that runs is reported; otherwise a batch starts when there is Ash and water.
    public static HopperAction ActionFor(int lyeReady, bool leaching, int ashStock, int litresStored)
    {
        if (lyeReady > 0)
            return HopperAction.CollectLye;
        if (leaching)
            return HopperAction.StillLeaching;
        if (NextBatch(ashStock, litresStored) > 0)
            return HopperAction.Start;
        if (ashStock <= 0 && litresStored <= 0)
            return HopperAction.NeedBoth;
        return ashStock <= 0 ? HopperAction.NeedAsh : HopperAction.NeedWater;
    }

    // Room left in a hopper holding this many pieces (the Ash waiting to leach and the Lye waiting to be collected).
    public static int Room(int held) => Math.Max(0, HopperCapacity - Math.Max(0, held));

    // One game hour of leaching. hoursLeft is 0 when no batch is running.
    public static float LeachAfterHour(float hoursLeft) => hoursLeft > 0f ? Math.Max(0f, hoursLeft - 1f) : 0f;

    // True on the hour a running batch finishes (it was running and now is not).
    public static bool FinishedThisHour(float before, float after) => before > 0f && after <= 0f;

    public static string HoursText(float hoursLeft) => $"{Math.Max(0, (int)Math.Ceiling(hoursLeft - 0.0001f))} h";

    // --- The hopper's words ---

    static string StockText(int ash, int litres)
    {
        if (ash > 0 && litres > 0)
            return $"{ash} Ash and {litres} L of water";
        return ash > 0 ? $"{ash} Ash" : $"{litres} L of water";
    }

    // The E line while a batch runs: its Lye, the hours to go, any Lye already ready, and the stock waiting for the next batch.
    public static string LeachingPrompt(string name, int batchLye, float hoursLeft, int lyeReady, int ashStock, int litresStored)
    {
        string ready = lyeReady > 0 ? $", {lyeReady} ready" : "";
        string waiting = ashStock > 0 || litresStored > 0 ? $"; {StockText(ashStock, litresStored)} waiting" : "";
        return $"{name}  (leaching {batchLye} Lye, {HoursText(hoursLeft)} to go{ready}{waiting})";
    }

    // The E line when nothing leaches and no Lye is ready, with stock in: start it, or say what it still needs.
    public static string StockedPrompt(string name, int ashStock, int litresStored)
    {
        int batch = NextBatch(ashStock, litresStored);
        if (batch > 0)
        {
            int ashLeft = ashStock - AshFor(batch), litresLeft = litresStored - WaterFor(batch);
            string left = ashLeft > 0 || litresLeft > 0 ? $"; {StockText(ashLeft, litresLeft)} left over" : "";
            return $"Start leaching  ({AshFor(batch)} Ash + {WaterFor(batch)} L -> {batch} Lye in {HoursText(LeachGameHours)}{left})";
        }
        string needs = ashStock <= 0 ? "needs Ash to start" : "needs water to start";
        return $"{name}  ({ashStock} Ash, {litresStored} L of water - {needs})";
    }

    public static string StartedMessage(int batch, int ashLeft, int litresLeft)
    {
        string left = ashLeft > 0 || litresLeft > 0 ? $" {StockText(ashLeft, litresLeft)} wait for the next batch." : "";
        return $"Started leaching {AshFor(batch)} Ash with {WaterFor(batch)} L of water - {batch} Lye in {HoursText(LeachGameHours)}.{left}";
    }

    public static string StillLeachingMessage(float hoursLeft) => $"Still leaching - {HoursText(hoursLeft)} to go.";

    public static string NeedToStartMessage(HopperAction action, int ashStock, int litresStored)
    {
        switch (action)
        {
            case HopperAction.NeedAsh: return $"It has {litresStored} L of water but no Ash. Load Ash with R.";
            case HopperAction.NeedWater: return $"It has {ashStock} Ash but no water. Load water with R (1 L per Lye; raw or boiled).";
            default: return "The hopper is empty. Load Ash and water with R, then start it with E.";
        }
    }

    // What an R press would stock, with what is missing or full for the part that is not loaded. carried*: what the pack holds; room*: what the hopper can still take.
    public static string LoadPrompt(StockPlan plan, int ashCarried, int litresCarried, int ashRoom, int waterRoom)
    {
        if (plan.ash <= 0 && plan.litres <= 0)
        {
            if (ashCarried <= 0 && litresCarried <= 0)
                return "Load Ash and water  (you're not carrying any)";
            return "Can't load  (" + FullNotes(ashCarried > 0 && ashRoom <= 0, litresCarried > 0 && waterRoom <= 0) + ")";
        }

        string what = "Load " + StockText(plan.ash, plan.litres);
        string note = MissingNote(plan, ashCarried, litresCarried);
        return note.Length > 0 ? $"{what}  ({note})" : what;
    }

    static string FullNotes(bool ashFull, bool waterFull)
    {
        if (ashFull && waterFull)
            return $"the hopper is full of Ash and Lye ({HopperCapacity}) and of water ({HopperWaterCapacity} L)";
        return ashFull ? $"the hopper is full of Ash and Lye ({HopperCapacity})" : $"the hopper is full of water ({HopperWaterCapacity} L)";
    }

    // Why the part that is not being loaded is not: nothing carried, or the hopper is full of it.
    static string MissingNote(StockPlan plan, int ashCarried, int litresCarried)
    {
        if (plan.ash <= 0)
            return ashCarried <= 0 ? "no Ash in your pack" : "the Ash is full";
        if (plan.litres <= 0)
            return litresCarried <= 0 ? "no water in your pack" : "the water is full";
        return "";
    }

    // What R flashes after stocking: what went in, what the hopper holds now, and what was left out. carried*: what the pack held before.
    public static string LoadedMessage(StockPlan plan, int ashHeld, int litresHeld, int ashCarried, int litresCarried)
    {
        string text = $"Loaded {StockText(plan.ash, plan.litres)}  (the hopper holds {ashHeld} Ash and {litresHeld} L of water).";
        string note = MissingNote(plan, ashCarried, litresCarried);
        return note.Length > 0 ? $"{text} Not loaded: {note}." : text;
    }

    // What R flashes when it can load nothing.
    public static string LoadRefusedMessage(int ashCarried, int litresCarried, int ashRoom, int waterRoom) =>
        ashCarried <= 0 && litresCarried <= 0
            ? "You're not carrying any Ash or water to load. Load Ash and water with R, then start it with E."
            : $"Can't load - {FullNotes(ashCarried > 0 && ashRoom <= 0, litresCarried > 0 && waterRoom <= 0)}. Collect the Lye (E) or start a batch.";

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
