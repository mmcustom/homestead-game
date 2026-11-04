using System;

// Metalworking, phase M2 (Mike, 2026-11-04): the Bloomery. A small stone-and-clay furnace, open to the sky, that turns Iron Ore and Charcoal into Iron Bars. The player loads Ore and
// Charcoal separately and at any time (R), starts a smelt with E, walks away, and comes back for the Bars (E). A smelt takes 10 game hours, counted one hour at a time like the
// Kiln and the Pit, so being away, sleeping and skipping days all count. Pure and engine-free so edit-mode tests cover it: what the furnace holds and the hours left come in as
// plain numbers, and the texts come out. Every number is a named constant.
//
// How the saved pile holds it (no new field): the Ore and Charcoal stocked and the Bars waiting are ordinary stacks in contents. A smelt is jobItem (SmeltMarker), jobCount (the
// Bars it will make, fixed when it is started) and jobHours (game hours left). The Ore and Charcoal a smelt uses are TAKEN OUT of contents when it starts, so what is in contents is
// always stock for the next smelt and never material already consumed by the running one. When the hours run out the Bars are added and the job fields are cleared.
//
// The table in MetalRules says what one bar takes; iron is its only row, so this file reads the iron row. When copper arrives the furnace will pick the metal from the ore in it.
public static class BloomeryRules
{
    public const string OreId = MetalRules.IronOreId;
    public const string CharcoalId = MetalRules.CharcoalId;
    public const string BarId = MetalRules.IronBarId;
    public const string StoneId = "stone";
    public const string ClayId = "clay";

    // --- The site: staged like the Kiln, finished with the Hammer (about 61 kg: 8 Stone at 5 kg and 6 Clay at 3.5 kg) ---

    public const int SiteStone = 8;
    public const int SiteClay = 6;
    public const int BuildBlows = 36;

    // --- The furnace's stock and the smelt ---

    public const int OreCap = 4;                              // the most Ore it holds in stock
    public const int CharcoalCap = 4;                         // the most Charcoal
    public const int OrePerBar = MetalRules.IronOrePerBar;    // 2 Ore ...
    public const int CharcoalPerBar = MetalRules.IronCharcoalPerBar; // ... and 2 Charcoal make 1 Iron Bar
    public const int MaxBars = 2;                             // one smelt makes at most this many
    public const int SmeltHours = 10;
    public const string SmeltMarker = "iron_smelt";

    // --- Where it may stand: open air, like the Kiln and the Pit ---

    public const float RadiusMetres = 0.7f;                   // the stack's reach from its centre (about 0.9 m across with its stone footing)
    public const float ClearanceMetres = PotteryRules.KilnClearanceMetres; // clear air from other structures and trees, and (both ways) from a placed bloomery: the Kiln's 3 m
    public const float TreeReachMetres = RadiusMetres + ClearanceMetres;
    public const float RegrowMetres = 1.2f;                   // no sprout inside its footprint (RegrowRules.Blocker.Bloomery)

    // --- The smoke over a smelting furnace (KilnSmoke, smaller than the Kiln's): presentation numbers ---

    public const float SmokeHeight = 1.25f;
    public const float SmokeSpread = 0.06f;
    public const float SmokeSizeMin = 0.3f;
    public const float SmokeSizeMax = 0.5f;
    public const float SmokePuffsPerSecond = 2.5f;
    public const float SmokeRiseSpeed = 0.5f;
    public const int SmokeMaxPuffs = 32;

    // --- The batch table ---

    // The Bars a smelt would make from this stock: 2 Ore and 2 Charcoal each, up to MaxBars. 3 Ore and 3 Charcoal make 1; 4 and 4 make 2; 4 Ore and 2 Charcoal make 1.
    public static int BarsFor(int ore, int charcoal) =>
        Math.Max(0, Math.Min(Math.Min(ore / OrePerBar, charcoal / CharcoalPerBar), MaxBars));

    public static int OreUsed(int bars) => bars * OrePerBar;

    public static int CharcoalUsed(int bars) => bars * CharcoalPerBar;

    // --- Phases ---

    public enum Phase { Empty, Loading, Ready, Smelting, Done }

    // smelting: a smelt runs. barsWaiting: finished Bars in the furnace. ore, charcoal: the stock in it.
    public static Phase PhaseOf(bool smelting, int barsWaiting, int ore, int charcoal)
    {
        if (smelting)
            return Phase.Smelting;
        if (barsWaiting > 0)
            return Phase.Done;
        if (ore <= 0 && charcoal <= 0)
            return Phase.Empty;
        return BarsFor(ore, charcoal) > 0 ? Phase.Ready : Phase.Loading;
    }

    public static bool CanStart(Phase phase) => phase == Phase.Ready;

    // The Hammer's R takes the bloomery down at any time but while it smelts (the Ore and Charcoal in the fire are already used).
    public static bool CanDismantle(Phase phase) => phase != Phase.Smelting;

    // --- Loading: Ore and Charcoal separately, at any time, each up to its cap ---

    public static int OreRoom(int oreHeld) => Math.Max(0, OreCap - Math.Max(0, oreHeld));

    public static int CharcoalRoom(int charcoalHeld) => Math.Max(0, CharcoalCap - Math.Max(0, charcoalHeld));

    public struct LoadPlan
    {
        public int ore, charcoal;
        public bool Any => ore > 0 || charcoal > 0;
    }

    // What one R press loads: the Ore carried up to the Ore room, and the Charcoal carried up to the Charcoal room.
    public static LoadPlan PlanLoad(int oreHeld, int charcoalHeld, int oreCarried, int charcoalCarried) => new LoadPlan
    {
        ore = Math.Max(0, Math.Min(oreCarried, OreRoom(oreHeld))),
        charcoal = Math.Max(0, Math.Min(charcoalCarried, CharcoalRoom(charcoalHeld))),
    };

    // --- The words ---

    public static string HoursText(float hoursLeft) => $"{Math.Max(0, (int)Math.Ceiling(hoursLeft - 0.0001f))} h";

    public static string BarsText(int n) => n == 1 ? "1 Iron Bar" : $"{n} Iron Bars";

    static string LoadText(int ore, int charcoal)
    {
        if (ore > 0 && charcoal > 0)
            return $"{ore} Ore and {charcoal} Charcoal";
        return ore > 0 ? $"{ore} Ore" : $"{charcoal} Charcoal";
    }

    static string StockText(int ore, int charcoal) => LoadText(ore, charcoal);

    public static string CostText => $"{SiteStone} Stone and {SiteClay} Clay";

    public static string SiteHelp => $"Deliver {CostText}";

    public static string BuiltMessage =>
        $"Bloomery built. Put the Hammer away, press R to load Ore and Charcoal (up to {OreCap} of each), then E to start a smelt; it takes {SmeltHours} hours and makes Iron Bars.";

    // What is still short for a smelt: "needs 2 Ore and 2 Charcoal", "needs 1 more Charcoal".
    public static string ShortText(int ore, int charcoal)
    {
        int needOre = Math.Max(0, OrePerBar - ore), needCharcoal = Math.Max(0, CharcoalPerBar - charcoal);
        if (needOre > 0 && needCharcoal > 0)
            return $"needs {needOre} Ore and {needCharcoal} Charcoal";
        if (needOre > 0)
            return $"needs {needOre} more Ore";
        return needCharcoal > 0 ? $"needs {needCharcoal} more Charcoal" : "";
    }

    // E's line, by phase. smeltBars: the Bars the running smelt will make. barsWaiting: finished Bars to collect. ore, charcoal: the stock (waiting for the next smelt while one runs).
    public static string Prompt(Phase phase, int ore, int charcoal, int barsWaiting, int smeltBars, float hoursLeft)
    {
        switch (phase)
        {
            case Phase.Empty: return $"Bloomery  (empty, {ShortText(0, 0)})";
            case Phase.Loading: return $"Bloomery  ({StockText(ore, charcoal)} - {ShortText(ore, charcoal)})";
            case Phase.Ready:
            {
                int bars = BarsFor(ore, charcoal);
                int oreLeft = ore - OreUsed(bars), charcoalLeft = charcoal - CharcoalUsed(bars);
                string left = oreLeft > 0 || charcoalLeft > 0 ? $"; {StockText(oreLeft, charcoalLeft)} left over" : "";
                return $"Start the smelt  ({OreUsed(bars)} Ore + {CharcoalUsed(bars)} Charcoal -> {BarsText(bars)} in {SmeltHours} h{left})";
            }
            case Phase.Smelting:
            {
                string waiting = ore > 0 || charcoal > 0 ? $"; {StockText(ore, charcoal)} waiting" : "";
                return $"Bloomery  (smelting {BarsText(smeltBars)}, {HoursText(hoursLeft)} to go{waiting})";
            }
            default: return $"Collect Iron Bars  ({barsWaiting})";
        }
    }

    // R's line without the Hammer: what it loads, naming what is missing or full for the part that is not loaded.
    public static string LoadPrompt(LoadPlan plan, int oreHeld, int charcoalHeld, int oreCarried, int charcoalCarried)
    {
        if (!plan.Any)
        {
            if (oreCarried <= 0 && charcoalCarried <= 0)
                return "Load Ore and Charcoal  (you're not carrying any)";
            return "Can't load  (" + FullNotes(oreCarried > 0 && OreRoom(oreHeld) <= 0, charcoalCarried > 0 && CharcoalRoom(charcoalHeld) <= 0) + ")";
        }
        string note = MissingNote(plan, oreCarried, charcoalCarried);
        return note.Length > 0 ? $"Load {LoadText(plan.ore, plan.charcoal)}  ({note})" : $"Load {LoadText(plan.ore, plan.charcoal)}";
    }

    static string FullNotes(bool oreFull, bool charcoalFull)
    {
        if (oreFull && charcoalFull)
            return $"it already holds {OreCap} Ore and {CharcoalCap} Charcoal";
        return oreFull ? $"it already holds {OreCap} Ore" : $"it already holds {CharcoalCap} Charcoal";
    }

    // Why the part that is not being loaded is not: nothing carried, or the furnace is full of it.
    static string MissingNote(LoadPlan plan, int oreCarried, int charcoalCarried)
    {
        if (plan.ore <= 0)
            return oreCarried <= 0 ? "no Ore in your pack" : "the Ore is full";
        if (plan.charcoal <= 0)
            return charcoalCarried <= 0 ? "no Charcoal in your pack" : "the Charcoal is full";
        return "";
    }

    // What R flashes after loading: what went in, what the furnace holds now, and what was left out.
    public static string Loaded(LoadPlan plan, int oreNow, int charcoalNow, int oreCarriedBefore, int charcoalCarriedBefore)
    {
        string text = $"Loaded {LoadText(plan.ore, plan.charcoal)}  (the bloomery holds {oreNow} Ore and {charcoalNow} Charcoal).";
        string note = MissingNote(plan, oreCarriedBefore, charcoalCarriedBefore);
        return note.Length > 0 ? $"{text} Not loaded: {note}." : text;
    }

    // What R flashes when it can load nothing.
    public static string LoadRefusal(int oreHeld, int charcoalHeld, int oreCarried, int charcoalCarried) =>
        oreCarried <= 0 && charcoalCarried <= 0
            ? "You're not carrying any Ore or Charcoal to load. Load them with R, then start the smelt with E."
            : $"Can't load - {FullNotes(oreCarried > 0 && OreRoom(oreHeld) <= 0, charcoalCarried > 0 && CharcoalRoom(charcoalHeld) <= 0)}. Collect the Bars (E) or start a smelt.";

    // What E says when the furnace is not ready to start.
    public static string NotReady(int ore, int charcoal) =>
        ore <= 0 && charcoal <= 0
            ? $"Load at least {OrePerBar} Ore and {CharcoalPerBar} Charcoal first (R)."
            : $"It {ShortText(ore, charcoal)} to smelt (it holds {ore} Ore and {charcoal} Charcoal).";

    public static string Started(int bars, int oreLeft, int charcoalLeft)
    {
        string left = oreLeft > 0 || charcoalLeft > 0 ? $" {StockText(oreLeft, charcoalLeft)} wait for the next smelt." : "";
        return $"The smelt is started: {OreUsed(bars)} Ore + {CharcoalUsed(bars)} Charcoal -> {BarsText(bars)} in {SmeltHours} h.{left}";
    }

    // What E flashes after collecting: the Bars taken, and what stayed behind for want of room.
    public static string Took(int took, int stayed)
    {
        if (took <= 0)
            return "Too heavy - no room to carry it";
        string text = $"Took {BarsText(took)}";
        return stayed > 0 ? text + $" ({stayed} left in the bloomery, no room)." : text + ".";
    }

    // R with the Hammer in hand.
    public static string DismantlePrompt(Phase phase, float hoursLeft) =>
        phase == Phase.Smelting
            ? $"Can't take down the bloomery while it smelts  ({HoursText(hoursLeft)} to go)"
            : "Take down the bloomery  (half the Stone and Clay back, rounded down; the Ore, Charcoal and Bars in it come back whole)";

    // --- The smelt's hours: one game hour at a time, like the Kiln and the Pit ---

    public static float AfterHour(float hoursLeft) => hoursLeft > 0f ? Math.Max(0f, hoursLeft - 1f) : 0f;

    public static bool IsDone(float hoursLeft) => hoursLeft <= 0f;
}
