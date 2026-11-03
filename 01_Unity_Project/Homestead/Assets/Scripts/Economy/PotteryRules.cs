using System;
using System.Collections.Generic;

// Pottery, phase P1 (Mike, 2026-11-02; Workshop_and_Handmade_Goods.md): a roofed Potter's Shed with a shelf. The player stocks the shed with Clay and Sand (R), shapes one to
// four pieces of one kind at once (E opens the Potter screen), and the pieces go on the shelf together when the job ends. They dry there for the piece's days, then E collects
// them as dry pieces (not sellable yet: firing is a later phase). Pure and engine-free so edit-mode tests cover it: what the shed holds, what is carried, the shelf's stacks and
// the game hour come in as plain numbers, and the texts come out. Every number is a named constant and the recipes are one table.
//
// How the saved pile holds it (no new kinds of field beyond jobCount): Clay and Sand are ordinary stacks (day 0); the running job is jobItem (the dry item's id), jobCount
// (how many pieces) and jobHours (game hours left, ticked one hour at a time like the Workshop); the shelf is stacks of dry-item ids whose day is the absolute game hour the
// piece is dry (AbsoluteHour + 24 x dryDays), so no hourly countdown is needed and sleeping or being away just works.
public static class PotteryRules
{
    public const string ClayId = "clay";
    public const string SandId = "sand";
    public const string LogsId = "logs";
    public const string BranchesId = "branches";
    public const string TallGrassId = "tall_grass";
    public const string CordageId = "cordage";

    public const string DryCupId = "dry_clay_cup";
    public const string DryBowlId = "dry_clay_bowl";
    public const string DryPotId = "dry_clay_pot";
    public const string DryCrockId = "dry_water_crock";
    public const string DryBrickId = "dry_brick_batch";

    // The fired pieces the Kiln makes (Pottery P2); they sell at the Trading Post, the dry ones do not.
    public const string FiredCupId = "clay_cup";
    public const string FiredBowlId = "clay_bowl";
    public const string FiredPotId = "clay_pot";
    public const string FiredCrockId = "water_crock";
    public const string FiredBrickId = "brick_batch";

    // --- The site: staged like the Workshop (about 35 kg), finished with the Hammer ---

    public const int SiteLogs = 3;
    public const int SiteBranches = 8;
    public const int SiteTallGrass = 8;   // the thatch roof
    public const int SiteCordage = 2;
    public const int BuildBlows = 24;

    // --- The shed's stock and shelf ---

    // One full job of 4 of any piece but the Brick Batch (4 Crocks = 16 Clay) and one Brick Batch (10 Clay) both fit. Sand is 1 a piece, so 4 is one job; 8 is two.
    public const int ClayCap = 16;
    public const int SandCap = 8;
    public const int ShelfSpace = 12;
    public const int MinCount = 1;
    public const int MaxCount = 4;
    public const int HoursPerDay = 24;

    public static readonly string[] StockIds = { ClayId, SandId };

    public static bool IsStockId(string itemId) => Array.IndexOf(StockIds, itemId) >= 0;

    public static int CapOf(string itemId) => itemId == ClayId ? ClayCap : itemId == SandId ? SandCap : 0;

    // Whether the shed takes one more of this: Clay or Sand, below its cap.
    public static bool Accepts(string itemId, int held) => IsStockId(itemId) && held < CapOf(itemId);

    // --- The recipes: one table, in the order the screen lists them. Every number is for ONE piece. ---

    public struct Recipe
    {
        public string itemId;       // the dry piece's item id
        public string name;         // as listed: "Clay Cup"
        public string one, many;    // as counted on the shelf: "Cup" / "Cups"
        public int clay, sand;
        public int shapeHours;      // game hours to shape one
        public int dryDays;         // game days on the shelf before it is dry
        public int space;           // shelf space units
        public float dryKg;
        public string firedId;      // the fired piece it becomes in the Kiln (Pottery P2)
        public float firedKg;
        public long firedCents;     // its base price at the Trading Post (the Crafted group)
    }

    public static readonly Recipe[] Recipes =
    {
        new Recipe { itemId = DryCupId,   name = "Clay Cup",          one = "Cup",           many = "Cups",           clay = 1,  sand = 1, shapeHours = 1, dryDays = 1, space = 1, dryKg = 0.3f, firedId = FiredCupId,   firedKg = 0.3f, firedCents = 400 },
        new Recipe { itemId = DryBowlId,  name = "Clay Bowl",         one = "Bowl",          many = "Bowls",          clay = 2,  sand = 1, shapeHours = 1, dryDays = 2, space = 1, dryKg = 0.6f, firedId = FiredBowlId,  firedKg = 0.6f, firedCents = 600 },
        new Recipe { itemId = DryPotId,   name = "Clay Cooking Pot",  one = "Cooking Pot",   many = "Cooking Pots",   clay = 3,  sand = 1, shapeHours = 2, dryDays = 3, space = 2, dryKg = 2f,   firedId = FiredPotId,   firedKg = 1.8f, firedCents = 1000 },
        new Recipe { itemId = DryCrockId, name = "Water Crock",       one = "Water Crock",   many = "Water Crocks",   clay = 4,  sand = 1, shapeHours = 2, dryDays = 3, space = 2, dryKg = 6f,   firedId = FiredCrockId, firedKg = 5.5f, firedCents = 1200 },
        new Recipe { itemId = DryBrickId, name = "Brick Batch",       one = "Brick Batch",   many = "Brick Batches",  clay = 10, sand = 1, shapeHours = 2, dryDays = 3, space = 4, dryKg = 16f,  firedId = FiredBrickId, firedKg = 14f,  firedCents = 600 },
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

    public static bool IsPiece(string itemId) => TryFind(itemId, out _);

    // --- The count of pieces in one job ---

    public static int ClampCount(int count) => Math.Max(MinCount, Math.Min(MaxCount, count));

    // A saved jobCount of 0 (a job from before the field, or none) means one piece.
    public static int SavedCount(int saved) => saved <= 0 ? MinCount : Math.Min(saved, MaxCount);

    public static int ClayFor(Recipe recipe, int count) => recipe.clay * count;
    public static int SandFor(Recipe recipe, int count) => recipe.sand * count;
    public static int SpaceFor(Recipe recipe, int count) => recipe.space * count;
    public static int HoursFor(Recipe recipe, int count) => recipe.shapeHours * count;

    // --- Time: the shelf's stamps are absolute game hours ---

    public static int AbsoluteHour(int totalDays, int hour) => totalDays * HoursPerDay + hour;

    // The stamp a piece gets when the job ends at endHour.
    public static int ReadyStamp(int endHour, int dryDays) => endHour + HoursPerDay * dryDays;

    public static bool IsDry(int readyStamp, int nowHour) => nowHour >= readyStamp;

    // Whole game days still to go (a started day counts), 0 once dry.
    public static int DaysLeft(int readyStamp, int nowHour) =>
        nowHour >= readyStamp ? 0 : (readyStamp - nowHour + HoursPerDay - 1) / HoursPerDay;

    // --- The job's hours: one game hour at a time, like the Workshop ---

    public static float AfterHour(float hoursLeft) => hoursLeft > 0f ? Math.Max(0f, hoursLeft - 1f) : 0f;

    public static bool IsDone(float hoursLeft) => hoursLeft <= 0f;

    public static string HoursText(float hoursLeft) => $"{Math.Max(0, (int)Math.Ceiling(hoursLeft - 0.0001f))} h";

    // --- The shelf ---

    public struct ShelfEntry
    {
        public string itemId;
        public int count;
        public int ready;   // the absolute game hour it is dry
    }

    public static int SpaceUsed(IEnumerable<ShelfEntry> shelf)
    {
        int used = 0;
        foreach (ShelfEntry entry in shelf)
            if (TryFind(entry.itemId, out Recipe r))
                used += r.space * entry.count;
        return used;
    }

    public static int FreeSpace(IEnumerable<ShelfEntry> shelf) => Math.Max(0, ShelfSpace - SpaceUsed(shelf));

    public static int DryPieces(IEnumerable<ShelfEntry> shelf, int nowHour)
    {
        int n = 0;
        foreach (ShelfEntry entry in shelf)
            if (IsPiece(entry.itemId) && IsDry(entry.ready, nowHour))
                n += entry.count;
        return n;
    }

    public static int WetPieces(IEnumerable<ShelfEntry> shelf, int nowHour)
    {
        int n = 0;
        foreach (ShelfEntry entry in shelf)
            if (IsPiece(entry.itemId) && !IsDry(entry.ready, nowHour))
                n += entry.count;
        return n;
    }

    static string Counted(Recipe recipe, int n) => n == 1 ? $"1 {recipe.one}" : $"{n} {recipe.many}";

    // The shelf in words, at most maxLines: wet pieces first ("Drying: 2 Cups, 1 day left"), then the dry ones ("Ready: 3 Bowls"). If there are more lines than fit, the last
    // one says how many more kinds there are.
    public static string[] ShelfLines(IEnumerable<ShelfEntry> shelf, int nowHour, int maxLines)
    {
        var entries = new List<ShelfEntry>(shelf);
        var drying = new List<(int order, int daysLeft, string text)>();
        var ready = new List<string>();
        for (int i = 0; i < Recipes.Length; i++)
        {
            Recipe recipe = Recipes[i];
            var byDays = new SortedDictionary<int, int>();
            int dry = 0;
            foreach (ShelfEntry entry in entries)
            {
                if (entry.itemId != recipe.itemId || entry.count <= 0)
                    continue;
                if (IsDry(entry.ready, nowHour))
                {
                    dry += entry.count;
                    continue;
                }
                int left = DaysLeft(entry.ready, nowHour);
                byDays.TryGetValue(left, out int have);
                byDays[left] = have + entry.count;
            }
            foreach (KeyValuePair<int, int> pair in byDays)
                drying.Add((i, pair.Key, $"Drying: {Counted(recipe, pair.Value)}, {pair.Key} {(pair.Key == 1 ? "day" : "days")} left"));
            if (dry > 0)
                ready.Add($"Ready: {Counted(recipe, dry)}");
        }

        var lines = new List<string>();
        foreach (var d in drying)
            lines.Add(d.text);
        lines.AddRange(ready);
        if (lines.Count == 0)
            return new[] { "The shelf is empty" };
        if (lines.Count <= maxLines)
            return lines.ToArray();
        var cut = lines.GetRange(0, Math.Max(0, maxLines - 1));
        cut.Add($"...and {lines.Count - cut.Count} more");
        return cut.ToArray();
    }

    public static string SpaceLine(int used) => $"On the shelf  -  {used} of {ShelfSpace} space used";

    // --- Starting a job ---

    public enum Gate { Ok, Busy, ShelfFull, ShortOfStock }

    // Why a job can or cannot start. working: a job is running. freeSpace: the shelf's free units now. A job needs room on the shelf for all its pieces, taken at the end of
    // the job (so the shelf must have the room now, and nothing else adds to it while the job runs).
    public static Gate CanStart(Recipe recipe, int count, bool working, int clayHeld, int sandHeld, int freeSpace)
    {
        if (working)
            return Gate.Busy;
        if (SpaceFor(recipe, count) > freeSpace)
            return Gate.ShelfFull;
        return clayHeld >= ClayFor(recipe, count) && sandHeld >= SandFor(recipe, count) ? Gate.Ok : Gate.ShortOfStock;
    }

    // --- Where it may stand (the Workshop's pair rule is reused: WorkshopRules.PairClearance) ---

    public const float SpacingMetres = 3f;
    public const float CartClearanceMetres = 2.6f;
    public const float TreeClearanceMetres = 2f;
    public const float KitchenRadiusMetres = 1.6f;   // half the diagonal of the 2.4 m by 2.0 m shed, for the Kitchen's setback rule

    // --- Words ---

    static string CountText(int n, string one, string many) => n == 1 ? $"1 {one}" : $"{n} {many}";

    public static string SiteCostText =>
        $"{SiteLogs} Logs, {SiteBranches} Branches, {SiteTallGrass} Tall Grass and {SiteCordage} Cordage";

    public static string CapsText => $"{ClayCap} Clay and {SandCap} Sand";

    public const string OpenPrompt = "Choose what to shape at the Potter's Shed";

    public static string WorkingPrompt(Recipe recipe, int count, float hoursLeft) => $"Potter's Shed  (shaping {Counted(recipe, count)}, {HoursText(hoursLeft)} to go)";

    public static string CollectPrompt(int dryPieces) => $"Collect {dryPieces} dry {(dryPieces == 1 ? "piece" : "pieces")}  (ready in the Potter's Shed)";

    public static string StorePrompt(string what) => $"Store {what}";

    public static string DeliverHelp => $"Deliver Clay and Sand  (the shed holds {CapsText})";

    // The Hammer's R: refused while a job runs or any piece is still wet; otherwise the stock and the dry pieces come back whole.
    public static string DismantlePrompt(bool working, float hoursLeft, int wetPieces)
    {
        if (working)
            return $"Can't dismantle the Potter's Shed while pieces are being shaped  ({HoursText(hoursLeft)} to go)";
        if (wetPieces > 0)
            return $"Can't dismantle the Potter's Shed while {CountText(wetPieces, "piece is", "pieces are")} still drying on the shelf";
        return "Dismantle the Potter's Shed  (half the materials back, rounded down; the stock and any dry pieces come back whole)";
    }

    // What the shed still lacks: "2 Clay", or "2 Clay, 1 Sand".
    public static string ShortText(Recipe recipe, int count, int clayHeld, int sandHeld)
    {
        string text = "";
        int clay = ClayFor(recipe, count) - clayHeld, sand = SandFor(recipe, count) - sandHeld;
        if (clay > 0)
            text += $"{clay} Clay";
        if (sand > 0)
            text += (text.Length > 0 ? ", " : "") + $"{sand} Sand";
        return text;
    }

    public static string StartedMessage(Recipe recipe, int count) =>
        $"Started shaping {Counted(recipe, count)}. They take {HoursText(HoursFor(recipe, count))}, then dry on the shelf for {recipe.dryDays} {(recipe.dryDays == 1 ? "day" : "days")}, even while you sleep or are away.";

    public static string FinishedMessage(Recipe recipe, int count) => $"{Counted(recipe, count)} shaped and put on the shelf to dry.";

    public static string BuiltMessage =>
        $"Potter's Shed built. Deliver Clay and Sand to it (it holds {CapsText}), and choose what to shape. {DigHelp}";

    public static string TookMessage(int took, int stayed) => stayed > 0
        ? $"Took {CountText(took, "dry piece", "dry pieces")} ({stayed} left on the shelf, no room)."
        : $"Took {CountText(took, "dry piece", "dry pieces")}. They need firing before they are worth anything.";

    // --- The screen's words ---

    public struct NeedRow
    {
        public string label;
        public int need, have;
        public bool enough;
        public int Short => Math.Max(0, need - have);
        public string Text => $"{label} - need {need} - shed has {have} - " + (enough ? "enough" : $"short by {Short}");
    }

    public static NeedRow[] NeedRows(Recipe recipe, int count, int clayHeld, int sandHeld)
    {
        NeedRow Row(string label, int need, int have) => new NeedRow { label = label, need = need, have = have, enough = have >= need };
        return new[] { Row("Clay", ClayFor(recipe, count), clayHeld), Row("Sand", SandFor(recipe, count), sandHeld) };
    }

    // The status line: the running job, or the idle shed and what it holds.
    public static string StatusLine(bool working, Recipe made, int madeCount, float hoursLeft, int clay, int sand) => working
        ? $"Shaping {Counted(made, madeCount)}, {HoursText(hoursLeft)} to go"
        : $"Shed is idle - it holds {clay} Clay, {sand} Sand";

    // A list row: the name, then the shaping hours and the drying days (no price in P1: the pieces cannot be sold until they are fired).
    public static string ListLabel(Recipe recipe) =>
        $"{recipe.name}\n{recipe.shapeHours} h each  -  dries in {recipe.dryDays} {(recipe.dryDays == 1 ? "day" : "days")}";

    // The detail line under the chosen piece's name.
    public static string DetailLine(Recipe recipe) =>
        $"{recipe.shapeHours} h each  -  dries {recipe.dryDays} {(recipe.dryDays == 1 ? "day" : "days")}  -  {recipe.space} shelf {(recipe.space == 1 ? "space" : "spaces")} each";

    public static string CountLabel(Recipe recipe, int count) => Counted(recipe, count);

    public static string TotalLine(Recipe recipe, int count) => $"Total shaping time: {HoursFor(recipe, count)} h  -  {SpaceFor(recipe, count)} shelf space";

    public static string StartButtonText(Gate gate, Recipe recipe, int count)
    {
        switch (gate)
        {
            case Gate.Ok: return $"START {Counted(recipe, count).ToUpperInvariant()}";
            case Gate.Busy: return "SHED IS BUSY";
            case Gate.ShelfFull: return "SHELF IS FULL";
            default: return "SHORT OF MATERIALS";
        }
    }

    public static string ReasonText(Gate gate, Recipe chosen, int count, int clayHeld, int sandHeld, int freeSpace, float hoursLeft, Recipe made, int madeCount)
    {
        switch (gate)
        {
            case Gate.Busy: return $"{Counted(made, madeCount)} {(madeCount == 1 ? "is" : "are")} {HoursText(hoursLeft)} from done.";
            case Gate.ShelfFull: return $"Only {freeSpace} shelf {(freeSpace == 1 ? "space is" : "spaces are")} free; {Counted(chosen, count)} need {SpaceFor(chosen, count)}. Collect dry pieces or make fewer.";
            case Gate.ShortOfStock: return $"Still short {ShortText(chosen, count, clayHeld, sandHeld)}. Deliver more.";
            default: return "";
        }
    }

    // --- The pack line and the stock button ---

    public static string PackLine(int clay, int sand)
    {
        var parts = new List<string>();
        if (clay > 0)
            parts.Add($"{clay} Clay");
        if (sand > 0)
            parts.Add($"{sand} Sand");
        return "You carry: " + (parts.Count > 0 ? string.Join(", ", parts) : "nothing the shed uses");
    }

    public enum StockGate { Can, NothingCarried, ShedFull }

    public static StockGate StockState(int carriedClay, int carriedSand, int shedClay, int shedSand)
    {
        if (carriedClay <= 0 && carriedSand <= 0)
            return StockGate.NothingCarried;
        bool room = (carriedClay > 0 && shedClay < ClayCap) || (carriedSand > 0 && shedSand < SandCap);
        return room ? StockGate.Can : StockGate.ShedFull;
    }

    public const string StockButtonText = "PUT MY CLAY AND SAND IN THE SHED";

    public static string StockReason(StockGate gate)
    {
        switch (gate)
        {
            case StockGate.NothingCarried: return "Your pack has no Clay or Sand.";
            case StockGate.ShedFull: return "The shed is already full of what you carry.";
            default: return "";
        }
    }

    // --- The keys the Potter screen answers (its own mapping: the Workshop's Key set is left alone) ---

    public enum Key { None, One, Two, Three, Four, Five, Up, Down, Left, Right, Enter, Stock }
    public enum ActionKind { None, Choose, Count, Start, Stock }

    public struct KeyAction
    {
        public ActionKind kind;
        public int choice;   // the list index to choose (Choose only)
        public int delta;    // -1 or +1 (Count only)
    }

    // The digits choose that piece (ignored past the end of the list), Up and Down move the choice and wrap round, Left and Right change the count, Enter starts, R stocks.
    public static KeyAction Resolve(Key key, int selected, int pieces)
    {
        if (pieces <= 0)
            return default;
        KeyAction Choose(int index) => index < pieces ? new KeyAction { kind = ActionKind.Choose, choice = index } : default;
        switch (key)
        {
            case Key.One: return Choose(0);
            case Key.Two: return Choose(1);
            case Key.Three: return Choose(2);
            case Key.Four: return Choose(3);
            case Key.Five: return Choose(4);
            case Key.Up: return new KeyAction { kind = ActionKind.Choose, choice = (selected - 1 + pieces) % pieces };
            case Key.Down: return new KeyAction { kind = ActionKind.Choose, choice = (selected + 1) % pieces };
            case Key.Left: return new KeyAction { kind = ActionKind.Count, delta = -1 };
            case Key.Right: return new KeyAction { kind = ActionKind.Count, delta = 1 };
            case Key.Enter: return new KeyAction { kind = ActionKind.Start };
            case Key.Stock: return new KeyAction { kind = ActionKind.Stock };
            default: return default;
        }
    }

    public const string KeyHint = "1-5 choose   Up/Down move   Left/Right count   Enter start   R stock   Esc close";

    // --- The Kiln (Pottery P2; Mike, 2026-11-03): an open-air kiln, staged like the Kitchen's hearth, loaded from the pack and fired for 24 game hours ---
    //
    // Saved with no new field: the loaded dry pieces and the Firewood are ordinary stacks (day 0), a running firing is jobItem = FiringMarker with jobHours the hours left (ticked one
    // game hour at a time like the other jobs, so sleeping and being away count), and when it ends each dry stack becomes its fired stack and the Ash is added. Fired pieces are told
    // from loaded ones by their id.

    public const string FirewoodId = "firewood";
    public const string AshId = "ash";
    public const string FiringMarker = "kiln_firing";

    public const int KilnStone = 6;
    public const int KilnClay = 6;          // about 51 kg with the Stone, so the Hand Cart (100 kg, which carries Stone and Clay) takes it in one trip
    public const int KilnBuildBlows = 30;
    public const int KilnSpace = 12;        // shelf-space units a firing holds (the same units as the shed's shelf)
    public const int KilnFirewood = 10;     // fixed per firing, whatever the load
    public const int KilnFireHours = 24;
    public const float KilnRadiusMetres = 1.2f;        // the dome's reach from its centre
    public const float KilnClearanceMetres = 3f;       // clear air from other structures and trees, and (both ways) from a placed Kiln
    public const float KilnTreeReachMetres = KilnRadiusMetres + KilnClearanceMetres;
    public const float RegrowMetres = 1.8f;            // no sprout inside the kiln's footprint (RegrowRules.Blocker.Kiln)

    // Ash from one firing: the Firewood burnt, through the same rule as a campfire (3.33 Ash, 3 whole).
    public static int KilnAsh => AshRules.Whole(AshRules.Owed(AshRules.FirewoodId, KilnFirewood));

    public static bool IsFired(string itemId) => FiredIdIndex(itemId) >= 0;

    static int FiredIdIndex(string itemId)
    {
        for (int i = 0; i < Recipes.Length; i++)
            if (Recipes[i].firedId == itemId)
                return i;
        return -1;
    }

    // The fired id a dry id becomes ("" for something that is not a dry piece), and the other way round.
    public static string FiredIdOf(string dryId) => TryFind(dryId, out Recipe recipe) ? recipe.firedId : "";

    public static string DryIdOf(string firedId)
    {
        int i = FiredIdIndex(firedId);
        return i >= 0 ? Recipes[i].itemId : "";
    }

    public static bool TryFindFired(string firedId, out Recipe recipe)
    {
        int i = FiredIdIndex(firedId);
        recipe = i >= 0 ? Recipes[i] : default;
        return i >= 0;
    }

    // The order R loads in: the dry pieces in the recipe order, then the Firewood.
    public static string[] LoadOrder
    {
        get
        {
            var ids = new string[Recipes.Length + 1];
            for (int i = 0; i < Recipes.Length; i++)
                ids[i] = Recipes[i].itemId;
            ids[Recipes.Length] = FirewoodId;
            return ids;
        }
    }

    public static int KilnFreeSpace(IEnumerable<ShelfEntry> loaded) => Math.Max(0, KilnSpace - SpaceUsed(loaded));

    // How many more of this the kiln takes: the dry pieces that still fit in the space left, or the Firewood up to the fixed 10.
    public static int KilnRoomFor(string itemId, int spaceUsed, int firewoodHeld)
    {
        if (itemId == FirewoodId)
            return Math.Max(0, KilnFirewood - firewoodHeld);
        return TryFind(itemId, out Recipe recipe) ? Math.Max(0, (KilnSpace - spaceUsed) / recipe.space) : 0;
    }

    public static bool KilnAccepts(string itemId, int spaceUsed, int firewoodHeld) => KilnRoomFor(itemId, spaceUsed, firewoodHeld) > 0;

    public enum KilnPhase { Empty, Loading, Ready, Firing, Done }

    // pieces: the loaded dry pieces, firewood: the Firewood in, firedWaiting: the fired pieces and Ash waiting to be collected.
    public static KilnPhase PhaseOf(bool firing, int firedWaiting, int pieces, int firewood)
    {
        if (firing)
            return KilnPhase.Firing;
        if (firedWaiting > 0)
            return KilnPhase.Done;
        if (pieces <= 0 && firewood <= 0)
            return KilnPhase.Empty;
        return pieces > 0 && firewood >= KilnFirewood ? KilnPhase.Ready : KilnPhase.Loading;
    }

    public enum KilnGate { Ok, Firing, Waiting, NothingFits }

    // Whether R may load now: not while a firing runs, and not while fired pieces wait to be collected. loadableCarried: what the pack carries that the kiln would take.
    public static KilnGate LoadGate(KilnPhase phase, int loadableCarried)
    {
        if (phase == KilnPhase.Firing)
            return KilnGate.Firing;
        if (phase == KilnPhase.Done)
            return KilnGate.Waiting;
        return loadableCarried > 0 ? KilnGate.Ok : KilnGate.NothingFits;
    }

    public static bool CanFire(KilnPhase phase) => phase == KilnPhase.Ready;

    // The Hammer's R takes it down only when it is empty (nothing in it) and not firing.
    public static bool CanDismantleKiln(KilnPhase phase) => phase == KilnPhase.Empty;

    static string PiecesText(int n) => CountText(n, "piece", "pieces");

    public static string KilnCostText => $"{KilnStone} Stone and {KilnClay} Clay";

    // E's line, by phase.
    public static string KilnPrompt(KilnPhase phase, int pieces, int firewood, int firedPieces, int ash, float hoursLeft)
    {
        switch (phase)
        {
            case KilnPhase.Empty: return $"Kiln  (load dry pieces and {KilnFirewood} Firewood)";
            case KilnPhase.Loading: return $"Kiln  ({PiecesText(pieces)}, {firewood} of {KilnFirewood} Firewood in)";
            case KilnPhase.Ready: return $"Fire the kiln  ({PiecesText(pieces)}, {KilnFirewood} Firewood)";
            case KilnPhase.Firing: return $"Kiln  (firing, {HoursText(hoursLeft)} to go)";
            default: return firedPieces > 0 ? $"Collect fired pieces  ({firedPieces})" : $"Collect the Ash  ({ash})";
        }
    }

    // R's line without the Hammer: what it loads, or why it will not.
    public static string KilnLoadPrompt(KilnGate gate, string carriedNames)
    {
        switch (gate)
        {
            case KilnGate.Ok: return $"Load {carriedNames}";
            case KilnGate.Firing: return "Can't load the kiln while it is firing";
            case KilnGate.Waiting: return "Collect the fired pieces before loading again";
            default: return "Load dry pieces and Firewood  (carry some)";
        }
    }

    public static string KilnLoadRefusal(KilnGate gate)
    {
        switch (gate)
        {
            case KilnGate.Firing: return "The kiln is firing - nothing can be loaded until it is done.";
            case KilnGate.Waiting: return "Fired pieces are waiting - collect them before loading again.";
            default: return $"Nothing you carry fits (dry pieces up to {KilnSpace} spaces, {KilnFirewood} Firewood).";
        }
    }

    // What E says when the kiln is not ready to fire.
    public static string KilnNotReady(int pieces, int firewood)
    {
        if (pieces <= 0 && firewood >= KilnFirewood)
            return "Load at least one dry piece first.";
        if (pieces <= 0)
            return $"Load at least one dry piece and {KilnFirewood} Firewood first.";
        return $"Needs {KilnFirewood - firewood} more Firewood to fire.";
    }

    public static string KilnStarted(int pieces) => $"The kiln is lit: {PiecesText(pieces)} will fire for {KilnFireHours} hours.";

    // What E flashes after collecting: the fired pieces, the Ash, and what stayed behind for want of room.
    public static string KilnTook(int took, int ash, int stayed)
    {
        var parts = new List<string>();
        if (took > 0)
            parts.Add(CountText(took, "fired piece", "fired pieces"));
        if (ash > 0)
            parts.Add($"{ash} Ash");
        if (parts.Count == 0)
            return "Too heavy - no room to carry them";
        string text = "Took " + string.Join(" and ", parts);
        return stayed > 0 ? text + $" ({stayed} left in the kiln, no room)." : text + ".";
    }

    public static string KilnLoaded(string what, int pieces, int firewood) =>
        $"Loaded {what}  (the kiln holds {PiecesText(pieces)}, {firewood} of {KilnFirewood} Firewood)";

    public static string KilnDismantlePrompt(KilnPhase phase, float hoursLeft)
    {
        switch (phase)
        {
            case KilnPhase.Firing: return $"Can't take down the kiln while it is firing  ({HoursText(hoursLeft)} to go)";
            case KilnPhase.Empty: return "Take down the kiln  (half the materials back, rounded down)";
            default: return "Empty the kiln before taking it down";
        }
    }

    public static string KilnSiteHelp => $"Deliver {KilnCostText} (Stone first)";

    public static string KilnBuiltMessage =>
        $"Kiln built. Load dry pieces and {KilnFirewood} Firewood from your pack, then light it; it fires for {KilnFireHours} hours and gives {KilnAsh} Ash.";

    // --- Digging: what a Shovel blow gives (Mike, 2026-11-02; the wet bank and shallows added the same day) ---
    //
    // Patches (ClayBank) are optional and none are placed in the scene. Every water question (over water? how deep? how far is the waterline?)
    // is answered by AxeTool.WaterAt and handed to DigFor as plain numbers, so the rule has no scene in it and follows a moving waterline.

    // None: nothing here to dig. Patch / WetBank / CreekBed give Clay; DryBank gives Sand; TooDeep is water over WadeDigDepthMetres.
    public enum Dig { None, Patch, WetBank, CreekBed, DryBank, TooDeep }

    public const float WetBankMetres = 2f;        // dry ground with water within this of the aim point is the wet bank (inside BankReachMetres)
    public const float BankReachMetres = 3f;      // within this the bank gives Sand (AxeTool's bankReach)
    public const float WadeDigDepthMetres = 0.3f; // the bed under water this shallow can be dug (ankle 0.15, the cart's limit 0.25, knee 0.5)
    public const float InnerProbeMetres = 1f;     // the wet bank is measured with a ring of probes here and at WetBankMetres

    // waterlineMetres: how far the nearest water is from the aim point on dry ground (infinity when none within reach); ignored over water.
    public static Dig DigFor(bool onClayPatch, bool overWater, float depthMetres, float waterlineMetres)
    {
        if (onClayPatch)
            return Dig.Patch;
        if (overWater)
            return depthMetres <= WadeDigDepthMetres ? Dig.CreekBed : Dig.TooDeep;
        if (waterlineMetres <= WetBankMetres)
            return Dig.WetBank;
        return waterlineMetres <= BankReachMetres ? Dig.DryBank : Dig.None;
    }

    public static bool GivesClay(Dig dig) => dig == Dig.Patch || dig == Dig.WetBank || dig == Dig.CreekBed;

    public static bool GivesSand(Dig dig) => dig == Dig.DryBank;

    // Something a blow can dig (a bank dig has no object of its own).
    public static bool CanDig(Dig dig) => GivesClay(dig) || GivesSand(dig);

    public static string DigName(Dig dig) => GivesSand(dig) ? "Sand" : "Clay";

    public static string PlaceName(Dig dig)
    {
        switch (dig)
        {
            case Dig.Patch: return "Clay patch";
            case Dig.WetBank: return "Wet clay bank";
            case Dig.CreekBed: return "Creek bed";
            case Dig.DryBank: return "Dry creek bank";
            default: return "";
        }
    }

    // The HUD line for a place the Shovel is aimed at: "Wet clay bank - hold click to dig Clay".
    public static string DigText(Dig dig) => dig == Dig.TooDeep
        ? "Water too deep to dig - try the shallows or the wet bank"
        : CanDig(dig) ? $"{PlaceName(dig)} - hold click to dig {DigName(dig)}" : "";

    public static string DiggingText(Dig dig) => dig == Dig.CreekBed ? "Digging Clay out of the creek bed" : $"Digging {DigName(dig)} out of the bank";

    public static string DugText(Dig dig) => $"Dug a lump of {DigName(dig)} out of the {(dig == Dig.CreekBed ? "creek bed" : "bank")}";

    // The Shovel in hand and nothing to dig where it is aimed.
    public const string ShovelIdleText = "Shovel - hold click on a stump to dig it out, at the water's edge or in the shallows to dig Clay, or up the bank to dig Sand";

    // Where to find them, for the shed's help and the line shown when it is built.
    public const string DigHelp = "Dig Clay with the Shovel at the water's edge or in the shallows, and Sand further up the creek bank.";
}
