using System;
using System.Collections.Generic;

// Metalworking, phase M3 (Mike, 2026-11-04; Mining_Metalworking_System.md's addendum): the Smithy. A roofed open-front shed with a forge and an anvil. The player stocks the forge
// with Iron Bars and Charcoal (R), chooses a good on the Smithy screen (E), starts it, walks off, and collects the finished good when it is done. A job's hours are saved on the
// Smithy and tick one game hour at a time like the Workshop's, so being away, sleeping and skipping days all count. Pure and engine-free so edit-mode tests cover it: what is in the
// forge, what is carried and the hours left come in as plain numbers, and the texts come out. Every number is a named constant.
//
// It copies the Workshop (WorkshopRules) closely: a staged site finished with the Hammer, a stock with caps, one job at a time, one good per job, a finished good that waits until it is
// taken, the same screen keys. It differs in what it uses (Iron Bars and Charcoal, not wood), in needing no tool in the pack, and in being a recipe table keyed by good so the Skillet,
// iron tools and other metals are later rows and not new code.
//
// How the saved pile holds it (no new field, as the Workshop): the stock and any finished good are ordinary stacks in contents; a running job is jobItem (the good's item id),
// jobHours (game hours left). A job takes its Bar and Charcoal out of the stock when it starts.
public static class SmithyRules
{
    public const string HingesId = "door_hinges";
    public const string HooksId = "hook_set";
    public const string NailsId = "nails_box";
    public const string BarId = MetalRules.IronBarId;
    public const string CharcoalId = MetalRules.CharcoalId;
    public const string StoneId = "stone";
    public const string ClayId = "clay";
    public const string LogsId = "logs";
    public const string BranchesId = "branches";
    public const string TallGrassId = "tall_grass";

    // --- The site: staged like the Workshop, finished with the Hammer (about 61 kg) ---

    public const int SiteStone = 6;
    public const int SiteClay = 3;
    public const int SiteLogs = 2;
    public const int SiteBranches = 4;
    public const int SiteTallGrass = 6;   // the thatch roof
    public const int BuildBlows = 40;

    // --- The forge's stock: the most it holds of each ---

    public const int BarsCap = 8;
    public const int CharcoalCap = 8;
    public const int SticksCap = 6;       // M4: the iron tools' hafts (an Iron Shovel takes 3, an Iron Pick Axe 2)
    public const int CordageCap = 4;      // M4: and their lashing (1 each)

    public const string SticksId = "sticks";
    public const string CordageId = "cordage";
    public const string IronShovelId = ToolRules.IronShovelId;
    public const string IronPickAxeId = ToolRules.IronPickAxeId;

    public static readonly string[] StockIds = { BarId, CharcoalId, SticksId, CordageId };

    public static bool IsStockId(string itemId) => Array.IndexOf(StockIds, itemId) >= 0;

    public static int CapOf(string itemId) =>
        itemId == BarId ? BarsCap : itemId == CharcoalId ? CharcoalCap : itemId == SticksId ? SticksCap : itemId == CordageId ? CordageCap : 0;

    // Whether the forge takes one more of this: it is a stock material and below its cap.
    public static bool Accepts(string itemId, int held) => IsStockId(itemId) && held < CapOf(itemId);

    // --- The recipes: one row per good (add the Skillet, tools and other metals here later) ---

    public struct Recipe
    {
        public string itemId;
        public string name;
        public string metalId;     // the MetalRules row whose bars it uses
        public int bars, charcoal;
        public int sticks, cordage; // M4: the iron tools' other ingredients, taken from the forge's stock like the Bars and the Charcoal (0 for the goods)
        public int hours;          // game hours the job takes
        public long baseCents;     // the Trading Post's base price; 0 = for the player's own use, with no market row (the iron tools)
        public float weightKg;
        public bool OwnUse => baseCents <= 0;
    }

    // Mike's first set, in the order the Smithy screen lists it. Prices run through the Crafted market group: 10% off per unit sold, floor 0.1, 25% daily recovery.
    public static readonly Recipe[] Recipes =
    {
        new Recipe { itemId = HingesId, name = "Door Hinges", metalId = MetalRules.IronId, bars = 1, charcoal = 1, hours = 3, baseCents = 2600, weightKg = 0.8f },
        new Recipe { itemId = HooksId, name = "Hook Set", metalId = MetalRules.IronId, bars = 1, charcoal = 1, hours = 2, baseCents = 1800, weightKg = 0.5f },
        new Recipe { itemId = NailsId, name = "Box of Nails (20)", metalId = MetalRules.IronId, bars = 1, charcoal = 1, hours = 2, baseCents = 1600, weightKg = 0.6f },
        // M4: the iron tools, for the player's own use (baseCents 0: no market row, no price, no Ledger line). Each is a Bar-and-Charcoal job with a haft and lashing, like the stone tool's sticks and cordage.
        new Recipe { itemId = IronShovelId, name = "Iron Shovel", metalId = MetalRules.IronId, bars = 2, charcoal = 2, sticks = 3, cordage = 1, hours = 3, baseCents = 0, weightKg = 1.8f },
        new Recipe { itemId = IronPickAxeId, name = "Iron Pick Axe", metalId = MetalRules.IronId, bars = 2, charcoal = 2, sticks = 2, cordage = 1, hours = 3, baseCents = 0, weightKg = 2f },
    };

    public static bool TryFind(string itemId, out Recipe recipe)
    {
        foreach (Recipe r in Recipes)
        {
            if (r.itemId == itemId)
            {
                recipe = r;
                return true;
            }
        }
        recipe = default;
        return false;
    }

    public static bool IsProduct(string itemId) => TryFind(itemId, out _);

    // Whether the forge's stock covers a recipe. Sticks and Cordage default to 0 (the goods need none).
    public static bool Covers(Recipe recipe, int bars, int charcoal, int sticks = 0, int cordage = 0) =>
        bars >= recipe.bars && charcoal >= recipe.charcoal && sticks >= recipe.sticks && cordage >= recipe.cordage;

    // --- Starting a job ---

    public enum Gate { Ok, Busy, ProductWaiting, ShortOfStock }

    // Why a job can or cannot start. working: a job is running. productWaiting: a finished good has not been collected. No tool is needed in the pack (the Workshop needs an Axe).
    public static Gate CanStart(Recipe recipe, bool working, bool productWaiting, int bars, int charcoal, int sticks = 0, int cordage = 0)
    {
        if (working)
            return Gate.Busy;
        if (productWaiting)
            return Gate.ProductWaiting;
        return Covers(recipe, bars, charcoal, sticks, cordage) ? Gate.Ok : Gate.ShortOfStock;
    }

    // --- The job's hours: one game hour at a time, like the Workshop ---

    public static float AfterHour(float hoursLeft) => hoursLeft > 0f ? Math.Max(0f, hoursLeft - 1f) : 0f;

    public static bool IsDone(float hoursLeft) => hoursLeft <= 0f;

    public static string HoursText(float hoursLeft) => $"{Math.Max(0, (int)Math.Ceiling(hoursLeft - 0.0001f))} h";

    // --- Where it may stand: the Workshop's rules for a roofed shed, plus clear air for the fire ---

    public const float ReachMetres = 1.9f;                    // the shed's half-diagonal (3 m by 2.4 m), as the Workshop's
    public const float FireClearanceMetres = 3f;              // no tree within this of the shed's edge (the forge fire)
    public const float TreeReachMetres = ReachMetres + FireClearanceMetres; // from the centre: 4.9 m
    public const float RegrowMetres = 2.9f;                   // no sprout inside the shed's footprint (the Workshop's)

    // --- The smoke over a working forge (KilnSmoke, from the chimney) ---

    public const float ChimneyX = -1.0f;                      // the chimney's place in the shed's own space (+Z is the open front)
    public const float ChimneyZ = -0.78f;

    public const float SmokeHeight = 3.0f;
    public const float SmokeSpread = 0.1f;
    public const float SmokeSizeMin = 0.35f;
    public const float SmokeSizeMax = 0.55f;
    public const float SmokePuffsPerSecond = 2.8f;
    public const float SmokeRiseSpeed = 0.5f;
    public const int SmokeMaxPuffs = 36;

    // --- The screen's panel and how it keeps inside any window ---

    public const float PanelWidthPx = 1500f;
    public const float PanelHeightPx = 920f;
    public const float ReferenceHeightPx = 1080f;

    // The canvas scaler's match setting for a window of this aspect ratio (width over height): 1 (match the height, so the panel is never taller than the window) when the window is wide
    // enough to hold the panel's width at that scale (every wide screen, 21:9 and 32:9 included), otherwise 0 (match the width, for a narrow window like 4:3).
    public static float ScreenMatch(float aspect) => aspect >= PanelWidthPx / ReferenceHeightPx ? 1f : 0f;

    // --- Words ---

    static string Count(int n, string one, string many) => n == 1 ? $"1 {one}" : $"{n} {many}";

    static string BarsText(int n) => Count(n, "Iron Bar", "Iron Bars");

    static string SticksText(int n) => Count(n, "Stick", "Sticks");

    public static string SiteCostText =>
        $"{SiteStone} Stone, {SiteClay} Clay, {SiteLogs} Logs, {SiteBranches} Branches and {SiteTallGrass} Tall Grass";

    public static string CapsText => $"{BarsCap} Iron Bars, {CharcoalCap} Charcoal, {SticksCap} Sticks and {CordageCap} Cordage";

    // What the forge still lacks for a recipe: "1 Iron Bar", "1 Charcoal" or "2 Iron Bars, 2 Charcoal, 3 Sticks, 1 Cordage".
    public static string ShortText(Recipe recipe, int bars, int charcoal, int sticks = 0, int cordage = 0)
    {
        var parts = new List<string>();
        if (recipe.bars - bars > 0)
            parts.Add(BarsText(recipe.bars - bars));
        if (recipe.charcoal - charcoal > 0)
            parts.Add($"{recipe.charcoal - charcoal} Charcoal");
        if (recipe.sticks - sticks > 0)
            parts.Add(SticksText(recipe.sticks - sticks));
        if (recipe.cordage - cordage > 0)
            parts.Add($"{recipe.cordage - cordage} Cordage");
        return string.Join(", ", parts);
    }

    // The prompts (no key names: the HUD adds them).
    public const string OpenPrompt = "Choose what to forge at the Smithy";

    public static string WorkingPrompt(Recipe recipe, float hoursLeft) => $"Smithy  (forging {recipe.name}, {HoursText(hoursLeft)} to go)";

    public static string DonePrompt(Recipe recipe) => $"Collect {recipe.name}  (ready in the Smithy)";

    public static string StorePrompt(string what) => $"Store {what}";

    public static string DeliverHelp =>
        $"Deliver Iron Bars, Charcoal, Sticks and Cordage  (the forge holds up to {BarsCap}, {CharcoalCap}, {SticksCap} and {CordageCap})";

    public static string DismantlePrompt(bool working, float hoursLeft) => working
        ? $"Can't dismantle the Smithy while something is being forged  ({HoursText(hoursLeft)} to go)"
        : "Dismantle the Smithy  (half the materials back, rounded down; the stock and any finished item come back whole)";

    // The flash when a job cannot start.
    public static string GateMessage(Gate gate, Recipe recipe, int bars, int charcoal, float hoursLeft, int sticks = 0, int cordage = 0)
    {
        switch (gate)
        {
            case Gate.Busy: return $"The forge is busy: {recipe.name} is {HoursText(hoursLeft)} from done.";
            case Gate.ProductWaiting: return "A finished item is waiting: collect it first.";
            case Gate.ShortOfStock: return $"The forge is short of {ShortText(recipe, bars, charcoal, sticks, cordage)} for {recipe.name}. Deliver more.";
            default: return "";
        }
    }

    public static string StartedMessage(Recipe recipe) => $"Started {recipe.name}. It will be ready in {HoursText(recipe.hours)}, even while you sleep or are away.";

    // What collecting a finished item flashes: goods are for sale, tools are the player's own.
    public static string TookMessage(Recipe good, int took, int stayed) =>
        stayed > 0 ? $"Took {took} {good.name} ({stayed} left in the Smithy, no room)."
        : good.OwnUse ? $"Took the {good.name}. It is yours to use; it cannot be sold."
        : $"Took the {good.name}. Sell it at the Trading Post.";

    public static string BuiltMessage =>
        $"Smithy built. Deliver Iron Bars, Charcoal, Sticks and Cordage to the forge (it holds {CapsText}), then choose what to forge at it.";

    // --- The Smithy screen's words ---

    // One line of a recipe's needs: the material, what the recipe needs, what the forge holds and whether that is enough.
    public struct NeedRow
    {
        public string label;
        public int need, have;
        public bool enough;
        public int Short => Math.Max(0, need - have);
        public string Text => $"{label} - need {need} - forge has {have} - " + (enough ? "enough" : $"short by {Short}");
    }

    // The rows for a recipe: Iron Bars and Charcoal always, then Sticks and Cordage when it needs them (the goods do not).
    public static NeedRow[] NeedRows(Recipe recipe, int bars, int charcoal, int sticks = 0, int cordage = 0)
    {
        NeedRow Row(string label, int need, int have) => new NeedRow { label = label, need = need, have = have, enough = have >= need };
        var rows = new List<NeedRow> { Row("Iron Bars", recipe.bars, bars), Row("Charcoal", recipe.charcoal, charcoal) };
        if (recipe.sticks > 0)
            rows.Add(Row("Sticks", recipe.sticks, sticks));
        if (recipe.cordage > 0)
            rows.Add(Row("Cordage", recipe.cordage, cordage));
        return rows.ToArray();
    }

    // The screen's status line: a finished item, the running job, or the idle forge and what it holds.
    public static string ForgeLine(bool working, Recipe made, float hoursLeft, bool waiting, Recipe waitingGood, int bars, int charcoal, int sticks = 0, int cordage = 0)
    {
        if (waiting)
            return $"{waitingGood.name} is ready - collect it";
        if (working)
            return $"Forging {made.name}, {HoursText(hoursLeft)} to go";
        return $"Forge is idle - it holds {BarsText(bars)}, {charcoal} Charcoal, {SticksText(sticks)}, {cordage} Cordage";
    }

    // A list row: the name, then the hours and what the Trading Post pays now (cents; 0 shows the base price), or "own use" for a tool that is not sold.
    public static string ListLabel(Recipe recipe, long sellCents) =>
        recipe.OwnUse ? $"{recipe.name}\n{recipe.hours} h  -  own use" : $"{recipe.name}\n{recipe.hours} h  -  {Money.Format(sellCents > 0 ? sellCents : recipe.baseCents)}";

    // The detail line under the name: hours, weight and the base price, or what a tool is for.
    public static string DetailLine(Recipe recipe) =>
        recipe.OwnUse ? $"{recipe.hours} h  -  {recipe.weightKg:0.#} kg  -  your own use, not sold"
                      : $"{recipe.hours} h  -  {recipe.weightKg:0.#} kg  -  base price {Money.Format(recipe.baseCents)}";

    // The Start button: "START DOOR HINGES" when it can start, otherwise why not in a few words.
    public static string StartButtonText(Gate gate, Recipe recipe)
    {
        switch (gate)
        {
            case Gate.Ok: return $"START {recipe.name.ToUpperInvariant()}";
            case Gate.Busy: return "FORGE IS BUSY";
            case Gate.ProductWaiting: return "COLLECT THE ITEM FIRST";
            default: return "SHORT OF MATERIALS";
        }
    }

    // The reason line under the Start button (empty when it can start). Busy names the item being made, not the one chosen.
    public static string ReasonText(Gate gate, Recipe chosen, Recipe made, int bars, int charcoal, float hoursLeft, int sticks = 0, int cordage = 0)
    {
        switch (gate)
        {
            case Gate.Busy: return $"{made.name} is {HoursText(hoursLeft)} from done.";
            case Gate.ProductWaiting: return "A finished item is waiting: collect it first.";
            case Gate.ShortOfStock: return $"Still short {ShortText(chosen, bars, charcoal, sticks, cordage)}. Deliver more.";
            default: return "";
        }
    }

    // What the pack carries of the four stock materials, the kinds it has only: "You carry: 3 Iron Bars, 5 Charcoal, 6 Sticks, 2 Cordage".
    public static string PackLine(int bars, int charcoal, int sticks = 0, int cordage = 0)
    {
        var parts = new List<string>();
        if (bars > 0)
            parts.Add(BarsText(bars));
        if (charcoal > 0)
            parts.Add($"{charcoal} Charcoal");
        if (sticks > 0)
            parts.Add(SticksText(sticks));
        if (cordage > 0)
            parts.Add($"{cordage} Cordage");
        return "You carry: " + (parts.Count > 0 ? string.Join(", ", parts) : "nothing the forge uses");
    }

    public enum StockGate { Can, NothingCarried, ForgeFull }

    // Whether the pack can put anything in the forge: the pack holds some of a stock material and the forge is below that material's cap.
    public static StockGate StockState(int carriedBars, int carriedCharcoal, int forgeBars, int forgeCharcoal, int carriedSticks = 0, int carriedCordage = 0, int forgeSticks = 0, int forgeCordage = 0)
    {
        if (carriedBars <= 0 && carriedCharcoal <= 0 && carriedSticks <= 0 && carriedCordage <= 0)
            return StockGate.NothingCarried;
        bool room = (carriedBars > 0 && forgeBars < BarsCap) || (carriedCharcoal > 0 && forgeCharcoal < CharcoalCap)
                 || (carriedSticks > 0 && forgeSticks < SticksCap) || (carriedCordage > 0 && forgeCordage < CordageCap);
        return room ? StockGate.Can : StockGate.ForgeFull;
    }

    public const string StockButtonText = "PUT MY IRON, CHARCOAL, STICKS AND CORDAGE IN THE FORGE";

    // The line under the stock button when it is greyed (empty when it can stock).
    public static string StockReason(StockGate gate)
    {
        switch (gate)
        {
            case StockGate.NothingCarried: return "Your pack has no Iron Bars, Charcoal, Sticks or Cordage.";
            case StockGate.ForgeFull: return "The forge is already full of what you carry.";
            default: return "";
        }
    }

    public static string Stored(string what, int bars, int charcoal, int sticks = 0, int cordage = 0) =>
        $"Stored {what}  (the forge holds {BarsText(bars)}, {charcoal} Charcoal, {SticksText(sticks)}, {cordage} Cordage)";

    // The keys the Smithy screen answers, as plain names (the screen turns the keyboard's own keys into these) and what each does.
    public enum Key { None, One, Two, Three, Four, Five, Up, Down, Enter, Stock }
    public enum ActionKind { None, Choose, Start, Stock }

    public struct KeyAction
    {
        public ActionKind kind;
        public int choice; // the list index to choose (Choose only)
    }

    // The digits choose that entry (ignored past the end of the list), Up and Down move the choice and wrap round, Enter starts it, R stocks; anything else does nothing.
    public static KeyAction Resolve(Key key, int selected, int goods)
    {
        if (goods <= 0)
            return default;
        switch (key)
        {
            case Key.One:
            case Key.Two:
            case Key.Three:
            case Key.Four:
            case Key.Five:
            {
                int index = (int)key - (int)Key.One;
                return index < goods ? new KeyAction { kind = ActionKind.Choose, choice = index } : default;
            }
            case Key.Up: return new KeyAction { kind = ActionKind.Choose, choice = (selected - 1 + goods) % goods };
            case Key.Down: return new KeyAction { kind = ActionKind.Choose, choice = (selected + 1) % goods };
            case Key.Enter: return new KeyAction { kind = ActionKind.Start };
            case Key.Stock: return new KeyAction { kind = ActionKind.Stock };
            default: return default;
        }
    }

    // The one-line key hint along the bottom of the screen.
    public const string KeyHint = "1-5 choose    Up/Down move    Enter start    R stock    Esc close";
}
