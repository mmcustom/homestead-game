using System;

// Workshop and Handmade Goods, phase W1(a1) (Mike, 2026-10-31; Workshop_and_Handmade_Goods.md): a roofed shed with a Woodworking bench built in. The player stocks the
// bench with Logs, Branches and Cordage (R), starts a piece (E), walks off, and collects it when it is done. A job's hours are saved on the bench and tick one game hour at a
// time like the Leach Hopper's, so it keeps running while the player is away and through sleep. Pure and engine-free so edit-mode tests cover it: what is in the bench, what is
// carried and how many game hours have passed come in as plain numbers, and the texts come out.
//
// Phase a2-1 (Mike, 2026-11-01) adds the End Table, Chair and Table and a Workshop screen (WorkshopScreen) to choose between the four: E opens it, and the rules below also
// build the words the screen shows. Every number is a named constant.
public static class WorkshopRules
{
    public const string StoolId = "rough_stool";
    public const string EndTableId = "end_table";
    public const string ChairId = "chair";
    public const string TableId = "table";
    public const string LogsId = "logs";
    public const string BranchesId = "branches";
    public const string CordageId = "cordage";
    public const string TallGrassId = "tall_grass";

    // --- The site: staged like the Hopper and the Cart (about 52 kg), finished with the Hammer ---

    public const int SiteLogs = 5;
    public const int SiteBranches = 10;
    public const int SiteTallGrass = 10;   // the thatch roof
    public const int SiteCordage = 3;
    public const int BuildBlows = 40;

    // --- The bench's stock: the most it holds of each material (twice the biggest single need) ---

    public const int LogsCap = 6;
    public const int BranchesCap = 8;
    public const int CordageCap = 4;

    public static readonly string[] StockIds = { LogsId, BranchesId, CordageId };

    public static bool IsStockId(string itemId) => Array.IndexOf(StockIds, itemId) >= 0;

    public static int CapOf(string itemId) =>
        itemId == LogsId ? LogsCap : itemId == BranchesId ? BranchesCap : itemId == CordageId ? CordageCap : 0;

    // Whether the bench takes one more of this: it is a stock material and below its cap.
    public static bool Accepts(string itemId, int held) => IsStockId(itemId) && held < CapOf(itemId);

    // --- The recipes ---

    public struct Recipe
    {
        public string itemId;
        public string name;
        public int logs, branches, cordage;
        public int hours;          // game hours the job takes
        public long baseCents;     // the Trading Post's base price
        public float weightKg;
    }

    // Mike's first set, in the order the Workshop screen lists it. Prices run through the crafted market group: 10% off per unit sold, floor 0.1, 25% daily recovery.
    public static readonly Recipe[] Recipes =
    {
        new Recipe { itemId = StoolId, name = "Rough Stool", logs = 2, branches = 2, cordage = 0, hours = 3, baseCents = 1400, weightKg = 10f },
        new Recipe { itemId = EndTableId, name = "End Table", logs = 1, branches = 3, cordage = 0, hours = 2, baseCents = 1200, weightKg = 8f },
        new Recipe { itemId = ChairId, name = "Chair", logs = 2, branches = 4, cordage = 1, hours = 4, baseCents = 1800, weightKg = 12f },
        new Recipe { itemId = TableId, name = "Table", logs = 3, branches = 4, cordage = 2, hours = 5, baseCents = 2800, weightKg = 24f },
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

    public static bool Covers(Recipe recipe, int logs, int branches, int cordage) =>
        logs >= recipe.logs && branches >= recipe.branches && cordage >= recipe.cordage;

    // --- Starting a job ---

    public enum Gate { Ok, Busy, ProductWaiting, NoTool, ShortOfStock }

    // Why a job can or cannot start. working: a job is running. productWaiting: a finished piece has not been collected. hasTool: any felling tool in the pack
    // (the Axe, the Primitive Axe or the Wood Saw: AxeRules.FastestCarried above 0), checked now and never while the job runs.
    public static Gate CanStart(Recipe recipe, bool hasTool, bool working, bool productWaiting, int logs, int branches, int cordage)
    {
        if (working)
            return Gate.Busy;
        if (productWaiting)
            return Gate.ProductWaiting;
        if (!hasTool)
            return Gate.NoTool;
        return Covers(recipe, logs, branches, cordage) ? Gate.Ok : Gate.ShortOfStock;
    }

    // --- The job's hours: one game hour at a time, like the Leach Hopper ---

    public static float AfterHour(float hoursLeft) => hoursLeft > 0f ? Math.Max(0f, hoursLeft - 1f) : 0f;

    public static bool IsDone(float hoursLeft) => hoursLeft <= 0f;

    // True on the hour a running job finishes (it was running and now is not).
    public static bool FinishedThisHour(float before, float after) => before > 0f && after <= 0f;

    public static string HoursText(float hoursLeft) => $"{Math.Max(0, (int)Math.Ceiling(hoursLeft - 0.0001f))} h";

    // --- Where it may stand ---

    // Centre to centre from another pile or building (the shed is about 3 m by 2.4 m), and from a parked Hand Cart, whose own exemption (CartRules.SpacingMetres)
    // would otherwise let a cart stand on the shed. A Cabin keeps its own, larger clearance.
    public const float SpacingMetres = 3f;
    public const float CartClearanceMetres = 2.6f;
    // No tree trunk may stand within this of the centre.
    public const float TreeClearanceMetres = 2f;

    public static float PairClearance(bool cartPair, bool cabinPair, float cabinMetres) =>
        cabinPair ? Math.Max(cabinMetres, SpacingMetres) : cartPair ? CartClearanceMetres : SpacingMetres;

    // --- Words ---

    static string Count(int n, string one, string many) => n == 1 ? $"1 {one}" : $"{n} {many}";

    // "2 Logs, 2 Branches" (Cordage only when the piece uses it).
    public static string NeedsText(Recipe recipe)
    {
        string text = Count(recipe.logs, "Log", "Logs") + ", " + Count(recipe.branches, "Branch", "Branches");
        if (recipe.cordage > 0)
            text += ", " + Count(recipe.cordage, "Cordage", "Cordage");
        return text;
    }

    // What the bench still lacks for a piece: "1 Log", or "1 Log, 2 Branches".
    public static string ShortText(Recipe recipe, int logs, int branches, int cordage)
    {
        string text = "";
        void Add(int missing, string one, string many)
        {
            if (missing <= 0)
                return;
            text += (text.Length > 0 ? ", " : "") + Count(missing, one, many);
        }
        Add(recipe.logs - logs, "Log", "Logs");
        Add(recipe.branches - branches, "Branch", "Branches");
        Add(recipe.cordage - cordage, "Cordage", "Cordage");
        return text;
    }

    public static string CapsText => $"{LogsCap} Logs, {BranchesCap} Branches and {CordageCap} Cordage";

    public static string SiteCostText =>
        $"{SiteLogs} Logs, {SiteBranches} Branches, {SiteTallGrass} Tall Grass and {SiteCordage} Cordage";

    // The prompts (no key names: the HUD adds them).
    // E at an idle bench (or one at work) opens the Workshop screen.
    public const string OpenPrompt = "Choose what to make at the Workshop";

    public static string WorkingPrompt(Recipe recipe, float hoursLeft) => $"Workshop  (making a {recipe.name}, {HoursText(hoursLeft)} to go)";

    public static string DonePrompt(Recipe recipe) => $"Collect {recipe.name}  (ready in the Workshop)";

    public static string StorePrompt(string what) => $"Store {what}";

    public static string DeliverHelp => $"Deliver Logs, Branches and Cordage  (the bench holds {CapsText})";

    public static string DismantlePrompt(bool working, float hoursLeft) => working
        ? $"Can't dismantle the Workshop while a piece is being made  ({HoursText(hoursLeft)} to go)"
        : "Dismantle the Workshop  (half the materials back, rounded down; the stock and any finished piece come back whole)";

    // The flash when E cannot start a job.
    public static string GateMessage(Gate gate, Recipe recipe, int logs, int branches, int cordage, float hoursLeft)
    {
        switch (gate)
        {
            case Gate.Busy: return $"The bench is busy: a {recipe.name} is {HoursText(hoursLeft)} from done.";
            case Gate.ProductWaiting: return "A finished piece is waiting: collect it first.";
            case Gate.NoTool: return "You need an Axe, Primitive Axe or Wood Saw in your pack to work the bench.";
            case Gate.ShortOfStock: return $"The bench is short of {ShortText(recipe, logs, branches, cordage)} for a {recipe.name}. Deliver more.";
            default: return "";
        }
    }

    public static string StartedMessage(Recipe recipe) => $"Started a {recipe.name}. It will be ready in {HoursText(recipe.hours)}, even while you sleep or are away.";

    public static string BuiltMessage =>
        $"Workshop built. Deliver Logs, Branches and Cordage to the bench (it holds {CapsText}), carry an Axe, and choose what to make at the bench.";

    // --- The Workshop screen's words ---

    // One line of a piece's needs: the material, what the piece needs, what the bench holds and whether that is enough.
    public struct NeedRow
    {
        public string label;
        public int need, have;
        public bool enough;
        public int Short => Math.Max(0, need - have);
        public string Text => $"{label} - need {need} - bench has {have} - " + (enough ? "enough" : $"short by {Short}");
    }

    // The rows for a piece: Logs, Branches, and Cordage only when the piece uses it.
    public static NeedRow[] NeedRows(Recipe recipe, int logs, int branches, int cordage)
    {
        NeedRow Row(string label, int need, int have) => new NeedRow { label = label, need = need, have = have, enough = have >= need };
        return recipe.cordage > 0
            ? new[] { Row("Logs", recipe.logs, logs), Row("Branches", recipe.branches, branches), Row("Cordage", recipe.cordage, cordage) }
            : new[] { Row("Logs", recipe.logs, logs), Row("Branches", recipe.branches, branches) };
    }

    // The screen's status line: a finished piece, the running job, or the idle bench and what it holds.
    public static string BenchLine(bool working, Recipe made, float hoursLeft, bool waiting, Recipe waitingPiece, int logs, int branches, int cordage)
    {
        if (waiting)
            return $"A {waitingPiece.name} is ready - collect it";
        if (working)
            return $"Making a {made.name}, {HoursText(hoursLeft)} to go";
        return $"Bench is idle - it holds {Count(logs, "Log", "Logs")}, {Count(branches, "Branch", "Branches")}, {cordage} Cordage";
    }

    // A list row: the name, then the hours and what the Trading Post pays now (cents; 0 shows the base price).
    public static string ListLabel(Recipe recipe, long sellCents) => $"{recipe.name}\n{recipe.hours} h  -  {Money.Format(sellCents > 0 ? sellCents : recipe.baseCents)}";

    // The detail line under the name: hours, weight and the base price.
    public static string DetailLine(Recipe recipe) => $"{recipe.hours} h  -  {recipe.weightKg:0.#} kg  -  base price {Money.Format(recipe.baseCents)}";

    // The Start button: "START CHAIR" when it can start, otherwise why not in a few words.
    public static string StartButtonText(Gate gate, Recipe recipe)
    {
        switch (gate)
        {
            case Gate.Ok: return $"START {recipe.name.ToUpperInvariant()}";
            case Gate.Busy: return "BENCH IS BUSY";
            case Gate.ProductWaiting: return "COLLECT THE PIECE FIRST";
            case Gate.NoTool: return "NEED A TOOL";
            default: return "SHORT OF MATERIALS";
        }
    }

    // The reason line under the Start button (empty when it can start). Busy names the piece being made, not the one chosen.
    public static string ReasonText(Gate gate, Recipe chosen, Recipe made, int logs, int branches, int cordage, float hoursLeft)
    {
        switch (gate)
        {
            case Gate.Busy: return $"A {made.name} is {HoursText(hoursLeft)} from done.";
            case Gate.ProductWaiting: return "A finished piece is waiting: collect it first.";
            case Gate.NoTool: return "Carry an Axe, Primitive Axe or Wood Saw to work the bench.";
            case Gate.ShortOfStock: return $"Still short {ShortText(chosen, logs, branches, cordage)}. Deliver more.";
            default: return "";
        }
    }

    // --- Phase a2-2: the pack line, the stock button and the keys ---

    // What the pack carries of the three stock materials, the kinds it has only: "You carry: 4 Logs, 6 Branches, 1 Cordage". Wood is counted whatever its wet or dry day.
    public static string PackLine(int logs, int branches, int cordage)
    {
        var parts = new System.Collections.Generic.List<string>();
        if (logs > 0)
            parts.Add(Count(logs, "Log", "Logs"));
        if (branches > 0)
            parts.Add(Count(branches, "Branch", "Branches"));
        if (cordage > 0)
            parts.Add($"{cordage} Cordage");
        return "You carry: " + (parts.Count > 0 ? string.Join(", ", parts) : "nothing the bench uses");
    }

    public enum StockGate { Can, NothingCarried, BenchFull }

    // Whether the pack can put anything in the bench: the pack holds some of the three materials and the bench is below its cap on at least one of those.
    public static StockGate StockState(int carriedLogs, int carriedBranches, int carriedCordage, int benchLogs, int benchBranches, int benchCordage)
    {
        if (carriedLogs <= 0 && carriedBranches <= 0 && carriedCordage <= 0)
            return StockGate.NothingCarried;
        bool room = (carriedLogs > 0 && benchLogs < LogsCap) || (carriedBranches > 0 && benchBranches < BranchesCap) || (carriedCordage > 0 && benchCordage < CordageCap);
        return room ? StockGate.Can : StockGate.BenchFull;
    }

    public const string StockButtonText = "PUT MY MATERIALS IN THE BENCH";

    // The line under the stock button when it is greyed (empty when it can stock).
    public static string StockReason(StockGate gate)
    {
        switch (gate)
        {
            case StockGate.NothingCarried: return "Your pack has none of these materials.";
            case StockGate.BenchFull: return "The bench is already full of what you carry.";
            default: return "";
        }
    }

    // The keys the Workshop screen answers, as plain names (the screen turns the keyboard's own keys into these) and what each does.
    public enum Key { None, One, Two, Three, Four, Up, Down, Enter, Stock }
    public enum ActionKind { None, Choose, Start, Stock }

    public struct KeyAction
    {
        public ActionKind kind;
        public int choice; // the list index to choose (Choose only)
    }

    // The digits choose that piece (ignored past the end of the list), Up and Down move the choice and wrap round, Enter starts it, R stocks; anything else does nothing.
    public static KeyAction Resolve(Key key, int selected, int pieces)
    {
        if (pieces <= 0)
            return default;
        switch (key)
        {
            case Key.One: return new KeyAction { kind = ActionKind.Choose, choice = 0 };
            case Key.Two: return pieces > 1 ? new KeyAction { kind = ActionKind.Choose, choice = 1 } : default;
            case Key.Three: return pieces > 2 ? new KeyAction { kind = ActionKind.Choose, choice = 2 } : default;
            case Key.Four: return pieces > 3 ? new KeyAction { kind = ActionKind.Choose, choice = 3 } : default;
            case Key.Up: return new KeyAction { kind = ActionKind.Choose, choice = (selected - 1 + pieces) % pieces };
            case Key.Down: return new KeyAction { kind = ActionKind.Choose, choice = (selected + 1) % pieces };
            case Key.Enter: return new KeyAction { kind = ActionKind.Start };
            case Key.Stock: return new KeyAction { kind = ActionKind.Stock };
            default: return default;
        }
    }

    // The one-line key hint along the bottom of the screen.
    public const string KeyHint = "1-4 choose    Up/Down move    Enter start    R stock    Esc close";
}
