using System;
using System.Collections.Generic;

// Tanning slice T5a (Mike, 2026-11-23; Tanning_Hides_and_Furs.md section 6): clothing made at the Workshop's Leatherwork bench and worn in five slots, Head, Upper Body,
// Lower Body, Hands and Feet. A worn piece stays in the pack (its weight counts) and the Inventory remembers which piece is on in each slot (InventoryManager). Each worn piece
// adds warmth (clo) on top of the everyday clothes the player is assumed to wear, whose winter end Mike lowered from 2.0 to 1.2 clo so the clothes matter in winter
// (SurvivalManager). The Moccasins also make sprinting cheaper and steps quieter (PlayerController).
//
// Slice T5b (Mike, 2026-11-24) adds two leather goods that are not worn: Rawhide Lashing (1 Rawhide makes 4; it counts as Cordage everywhere, InventoryContainer) and the
// Leather Pack (+20 kg of carry room while carried, instead of the Pouch's +10: the bigger one counts, InventoryManager).
//
// A leather piece takes its hides, Sinew and the rest straight from the pack when it starts (no stocking step), then runs hour by hour on the Workshop's bench like the woodwork
// (WorkshopRules: a piece shares the bench's one job and its finished-piece slot). The Knife must be in the pack to start one. Pure and engine-free so edit-mode tests cover it.
public static class ClothingRules
{
    public enum Slot { Head, UpperBody, LowerBody, Hands, Feet }
    public const int SlotCount = 5;

    public const string CoatId = "buckskin_coat";
    public const string LeggingsId = "buckskin_leggings";
    public const string HatId = "fur_hat";
    public const string MittensId = "fur_mittens";
    public const string MoccasinsId = "moccasins";

    public const string BuckskinId = "buckskin";
    public const string TannedFursId = "tanned_furs";
    public const string SinewId = "sinew";
    public const string KnifeId = "knife";
    public const string RawhideId = "rawhide";
    public const string LeatherId = "leather";
    public const string LashingId = "rawhide_lashing";
    public const string PackId = "leather_pack";

    // The Leather Pack's extra carry room while it is in the pack (the Pouch's is 10 kg; the two do not add up).
    public const float PackBonusKg = 20f;

    // Mike's decision: the everyday clothes' warmth at the cold end of the year, lowered from 2.0 so a full set (+1.2 clo) is what keeps a winter comfortable.
    public const float WinterEverydayClo = 1.2f;
    public const float SummerEverydayClo = 0.8f;

    // The Moccasins: sprinting costs this share of its stamina, and walking and sprinting are this much as easy for wildlife to notice.
    public const float MoccasinSprintFactor = 0.8f;
    public const float MoccasinDetectionFactor = 0.8f;

    public struct Need
    {
        public string itemId;
        public string name;
        public int count;
    }

    public struct Piece
    {
        public string itemId;
        public string name;
        public Slot slot;
        public float clo;
        public Need[] needs;
        public int hours;
        public long baseCents;
        public float weightKg;
        public bool notWorn;   // T5b: a leather good that is not clothing (the Lashing, the Pack): no slot, no warmth
        public int makes;      // how many one job makes (0 means 1)
        public int Makes => makes > 0 ? makes : 1;
    }

    static Need N(string id, string name, int count) => new Need { itemId = id, name = name, count = count };

    // The pieces, in the order the Leatherwork screen lists them.
    public static readonly Piece[] Pieces =
    {
        new Piece { itemId = CoatId, name = "Buckskin Coat", slot = Slot.UpperBody, clo = 0.5f, hours = 6, baseCents = 9000, weightKg = 2.0f,
                    needs = new[] { N(BuckskinId, "Buckskin", 3), N(TannedFursId, "Tanned Furs", 1), N(SinewId, "Sinew", 2) } },
        new Piece { itemId = LeggingsId, name = "Buckskin Leggings", slot = Slot.LowerBody, clo = 0.3f, hours = 4, baseCents = 5000, weightKg = 1.2f,
                    needs = new[] { N(BuckskinId, "Buckskin", 2), N(SinewId, "Sinew", 1) } },
        new Piece { itemId = HatId, name = "Fur Hat", slot = Slot.Head, clo = 0.2f, hours = 2, baseCents = 1800, weightKg = 0.3f,
                    needs = new[] { N(TannedFursId, "Tanned Furs", 2), N(SinewId, "Sinew", 1) } },
        new Piece { itemId = MittensId, name = "Fur Mittens", slot = Slot.Hands, clo = 0.15f, hours = 2, baseCents = 1600, weightKg = 0.25f,
                    needs = new[] { N(TannedFursId, "Tanned Furs", 2), N(SinewId, "Sinew", 1) } },
        new Piece { itemId = MoccasinsId, name = "Moccasins", slot = Slot.Feet, clo = 0.05f, hours = 2, baseCents = 3000, weightKg = 0.5f,
                    needs = new[] { N(BuckskinId, "Buckskin", 1), N(SinewId, "Sinew", 1) } },
        new Piece { itemId = LashingId, name = "Rawhide Lashing", notWorn = true, makes = 4, hours = 1, baseCents = 0, weightKg = 0.1f,
                    needs = new[] { N(RawhideId, "Rawhide", 1) } },
        new Piece { itemId = PackId, name = "Leather Pack", notWorn = true, hours = 4, baseCents = 7500, weightKg = 1.5f,
                    needs = new[] { N(LeatherId, "Leather", 2), N(SinewId, "Sinew", 2), N(LashingId, "Rawhide Lashing", 2) } },
    };

    // The pieces that are worn (the five pieces of clothing), in screen order.
    public static Piece[] Wearables => Array.FindAll(Pieces, p => !p.notWorn);

    public static bool IsWearable(string itemId) => TryFind(itemId, out Piece p) && !p.notWorn;

    // The carry room the pack gets from what it carries: the bigger of the Pouch's and the Leather Pack's.
    public static float CarryBonusKg(bool pouch, float pouchBonusKg, bool leatherPack) =>
        Math.Max(pouch ? pouchBonusKg : 0f, leatherPack ? PackBonusKg : 0f);

    public static bool TryFind(string itemId, out Piece piece)
    {
        foreach (Piece p in Pieces)
        {
            if (p.itemId == itemId)
            {
                piece = p;
                return true;
            }
        }
        piece = default;
        return false;
    }

    public static bool IsPiece(string itemId) => TryFind(itemId, out _);

    public static string SlotName(Slot slot)
    {
        switch (slot)
        {
            case Slot.Head: return "Head";
            case Slot.UpperBody: return "Upper Body";
            case Slot.LowerBody: return "Lower Body";
            case Slot.Hands: return "Hands";
            default: return "Feet";
        }
    }

    // The slot a piece goes in, or -1 for anything that is not clothing.
    public static int SlotOf(string itemId) => TryFind(itemId, out Piece p) && !p.notWorn ? (int)p.slot : -1;

    // The warmth of what is worn: one item id per slot (null or "" for an empty slot). Unknown ids add nothing.
    public static float WornClo(IList<string> worn)
    {
        float clo = 0f;
        if (worn == null)
            return clo;
        foreach (string id in worn)
            if (!string.IsNullOrEmpty(id) && TryFind(id, out Piece p) && !p.notWorn)
                clo += p.clo;
        return clo;
    }

    public static bool Wears(IList<string> worn, string itemId)
    {
        if (worn == null)
            return false;
        foreach (string id in worn)
            if (id == itemId)
                return true;
        return false;
    }

    public static float SprintFactor(bool moccasins) => moccasins ? MoccasinSprintFactor : 1f;
    public static float DetectionFactor(bool moccasins) => moccasins ? MoccasinDetectionFactor : 1f;

    // --- The Workshop's bench: a leather piece as the bench's own recipe (no Logs, Branches or Cordage), so the bench's job, prompts and finished piece work for it unchanged ---

    // How many one job makes of a piece (the Lashing makes 4), 1 for anything else.
    public static int MakesOf(string itemId) => TryFind(itemId, out Piece p) ? p.Makes : 1;

    public static WorkshopRules.Recipe AsBenchRecipe(Piece piece) =>
        new WorkshopRules.Recipe { itemId = piece.itemId, name = piece.name, logs = 0, branches = 0, cordage = 0, hours = piece.hours, baseCents = piece.baseCents, weightKg = piece.weightKg };

    // Whether the pack covers a piece: count(itemId) gives what the pack carries.
    public static bool Covers(Piece piece, Func<string, int> count)
    {
        foreach (Need need in piece.needs)
            if (count(need.itemId) < need.count)
                return false;
        return true;
    }

    // Why a piece can or cannot start: the bench's own gates (busy, a finished piece waiting), then the Knife, then the pack.
    public static WorkshopRules.Gate CanStart(Piece piece, bool hasKnife, bool working, bool productWaiting, Func<string, int> count)
    {
        if (working)
            return WorkshopRules.Gate.Busy;
        if (productWaiting)
            return WorkshopRules.Gate.ProductWaiting;
        if (!hasKnife)
            return WorkshopRules.Gate.NoTool;
        return Covers(piece, count) ? WorkshopRules.Gate.Ok : WorkshopRules.Gate.ShortOfStock;
    }

    // --- Words ---

    static string Count(int n, string name) => $"{n} {name}";

    // "3 Buckskin, 1 Tanned Furs, 2 Sinew".
    public static string NeedsText(Piece piece)
    {
        var parts = new List<string>();
        foreach (Need need in piece.needs)
            parts.Add(Count(need.count, need.name));
        return string.Join(", ", parts);
    }

    // What the pack still lacks: "1 Buckskin, 1 Sinew".
    public static string ShortText(Piece piece, Func<string, int> count)
    {
        var parts = new List<string>();
        foreach (Need need in piece.needs)
        {
            int missing = need.count - count(need.itemId);
            if (missing > 0)
                parts.Add(Count(missing, need.name));
        }
        return string.Join(", ", parts);
    }

    // One line of a piece's needs against the pack.
    public static WorkshopRules.NeedRow[] NeedRows(Piece piece, Func<string, int> count)
    {
        var rows = new WorkshopRules.NeedRow[piece.needs.Length];
        for (int i = 0; i < rows.Length; i++)
        {
            int have = count(piece.needs[i].itemId);
            rows[i] = new WorkshopRules.NeedRow { label = piece.needs[i].name, need = piece.needs[i].count, have = have, enough = have >= piece.needs[i].count };
        }
        return rows;
    }

    // The need line on the Leatherwork screen: the pack, not the bench, supplies a leather piece.
    public static string NeedText(WorkshopRules.NeedRow row) =>
        $"{row.label} - need {row.need} - you carry {row.have} - " + (row.enough ? "enough" : $"short by {row.Short}");

    // A list row: the name and slot, then the hours and what the Trading Post pays now (cents; 0 shows the base price).
    public static string ListLabel(Piece piece, long sellCents) =>
        $"{piece.name}\n{KindText(piece)}  -  {piece.hours} h  -  " + (piece.baseCents > 0 ? Money.Format(sellCents > 0 ? sellCents : piece.baseCents) : "not sold");

    // What a piece is: its slot for clothing; "Makes 4" for the Lashing; "Carried" for the Pack.
    public static string KindText(Piece piece) =>
        !piece.notWorn ? SlotName(piece.slot) : piece.Makes > 1 ? $"Makes {piece.Makes}" : "Carried";

    // The detail line under the name.
    public static string DetailLine(Piece piece) => piece.notWorn
        ? $"{KindText(piece)}  -  {piece.hours} h  -  {piece.weightKg:0.##} kg" + (piece.Makes > 1 ? " each" : "")
        : $"{SlotName(piece.slot)}  -  {WarmthText(piece)}  -  {piece.hours} h  -  {piece.weightKg:0.##} kg";

    public static string WarmthText(Piece piece) => $"+{piece.clo:0.##} warmth";

    // What the piece does when worn, for the screen and the Inventory.
    public static string EffectText(Piece piece) =>
        piece.itemId == LashingId ? "counts as Cordage in every recipe and building (plain Cordage is used first)"
        : piece.itemId == PackId ? $"+{PackBonusKg:0} kg of carry room while carried (instead of the Pouch's +10)"
        : piece.itemId == MoccasinsId
        ? $"{WarmthText(piece)}; sprinting costs {Math.Round((1f - MoccasinSprintFactor) * 100f)}% less stamina, and wildlife notice your steps less"
        : $"{WarmthText(piece)} when worn";

    public const string Title = "Workshop - Leatherwork";
    public const string TabWoodwork = "Woodwork";
    public const string TabLeatherwork = "Leatherwork";

    public static string PackLine(Func<string, int> count) =>
        $"You carry: {count(BuckskinId)} Buckskin, {count(TannedFursId)} Tanned Furs, {count(SinewId)} Sinew, {count(RawhideId)} Rawhide, {count(LeatherId)} Leather";

    public static string StartButtonText(WorkshopRules.Gate gate, Piece piece)
    {
        switch (gate)
        {
            case WorkshopRules.Gate.Ok: return $"START {piece.name.ToUpperInvariant()}";
            case WorkshopRules.Gate.Busy: return "BENCH IS BUSY";
            case WorkshopRules.Gate.ProductWaiting: return "COLLECT THE PIECE FIRST";
            case WorkshopRules.Gate.NoTool: return "NEED A KNIFE";
            default: return "SHORT OF MATERIALS";
        }
    }

    public static string ReasonText(WorkshopRules.Gate gate, Piece chosen, WorkshopRules.Recipe made, Func<string, int> count, float hoursLeft)
    {
        switch (gate)
        {
            case WorkshopRules.Gate.Busy: return $"A {made.name} is {WorkshopRules.HoursText(hoursLeft)} from done.";
            case WorkshopRules.Gate.ProductWaiting: return "A finished piece is waiting: collect it first.";
            case WorkshopRules.Gate.NoTool: return "Carry a Knife to cut and sew leather.";
            case WorkshopRules.Gate.ShortOfStock: return $"Your pack is short {ShortText(chosen, count)}.";
            default: return "Takes the materials from your pack.";
        }
    }

    // --- Keys on the Leatherwork screen: 1-7 choose, Up and Down move, Enter starts, W switches to Woodwork ---

    public enum Key { None, Digit, Up, Down, Enter, Woodwork }
    public enum ActionKind { None, Choose, Start, Woodwork }

    public static (ActionKind kind, int choice) Resolve(Key key, int digit, int selected, int count)
    {
        switch (key)
        {
            case Key.Digit: return digit >= 1 && digit <= count ? (ActionKind.Choose, digit - 1) : (ActionKind.None, selected);
            case Key.Up: return (ActionKind.Choose, Math.Max(0, selected - 1));
            case Key.Down: return (ActionKind.Choose, Math.Min(count - 1, selected + 1));
            case Key.Enter: return (ActionKind.Start, selected);
            case Key.Woodwork: return (ActionKind.Woodwork, selected);
            default: return (ActionKind.None, selected);
        }
    }

    public const string KeyHint = "1-7 choose    Up/Down move    Enter start    W woodwork    Esc close";

    // --- The Inventory's Wearing panel ---

    public static string WornLine(Slot slot, string pieceName) => $"{SlotName(slot)}: {(string.IsNullOrEmpty(pieceName) ? "-" : pieceName)}";
    public static string PutOnMessage(Piece piece) => $"Put on the {piece.name} ({EffectText(piece)}).";
    public static string TookOffMessage(Piece piece) => $"Took off the {piece.name}.";
}
