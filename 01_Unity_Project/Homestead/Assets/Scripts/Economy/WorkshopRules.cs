using System;

// Workshop and Handmade Goods, phase W1(a1) (Mike, 2026-10-31; Workshop_and_Handmade_Goods.md): a roofed shed with a Woodworking bench built in. The player stocks the
// bench with Logs, Branches and Cordage (R), starts a piece (E), walks off, and collects it when it is done. A job's hours are saved on the bench and tick one game hour at a
// time like the Leach Hopper's, so it keeps running while the player is away and through sleep. Pure and engine-free so edit-mode tests cover it: what is in the bench, what is
// carried and how many game hours have passed come in as plain numbers, and the texts come out.
//
// The first piece is the Rough Stool; the End Table, Chair and Table come in a2. Every number is a named constant.
public static class WorkshopRules
{
    public const string StoolId = "rough_stool";
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

    // Mike's first set (a1 has only the first). Prices run through the crafted market group: 10% off per unit sold, floor 0.1, 25% daily recovery.
    public static readonly Recipe[] Recipes =
    {
        new Recipe { itemId = StoolId, name = "Rough Stool", logs = 2, branches = 2, cordage = 0, hours = 3, baseCents = 1400, weightKg = 10f },
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
    public static string StartPrompt(Recipe recipe) => $"Start {recipe.name}  ({NeedsText(recipe)}, {recipe.hours} h)";

    public static string EmptyPrompt(Recipe recipe) => $"Workshop  (empty: deliver {NeedsText(recipe)} for a {recipe.name})";

    public static string ShortPrompt(Recipe recipe, int logs, int branches, int cordage) =>
        $"Workshop  (a {recipe.name} needs {NeedsText(recipe)}; still short {ShortText(recipe, logs, branches, cordage)})";

    public static string NoToolPrompt(Recipe recipe) =>
        $"Workshop  (stocked for a {recipe.name}; carry an Axe, Primitive Axe or Wood Saw to start)";

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
        $"Workshop built. Deliver Logs, Branches and Cordage to the bench (it holds {CapsText}), carry an Axe, and start a Rough Stool.";
}
