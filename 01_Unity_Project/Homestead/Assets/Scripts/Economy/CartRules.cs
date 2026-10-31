using System;

// Vehicles_and_Hauling.md, Step 1, Phase 1 (Mike, 2026-10-30): the Hand Cart as a parked, loaded object. Pure and engine-free so edit-mode tests
// cover it. A Wooden Wheel is a Craft recipe; the cart is a staged site (the wheels, Logs, Branches and Cordage are delivered over trips and the
// Hammer finishes it, like the Leach Hopper) that becomes a pile holding up to CapacityKg of bulk goods. Pushing it is Phase 2 and is not here.
//
// Every number is a named constant. Nothing here touches the pack, a pile, a fire or the clock: weights and counts come in as plain numbers.
public static class CartRules
{
    // --- The Wooden Wheel (Craft tab): 1 Log and 1 Cordage, instant, no tool. The item asset holds its 3 kg; WheelWeightKg is the figure it must match. ---

    public const string WheelId = "wooden_wheel";
    public const string LogsId = "logs";
    public const string BranchesId = "branches";
    public const string CordageId = "cordage";
    public const string FirewoodId = "firewood";
    public const string StoneId = "stone";
    public const string ClayId = "clay";
    public const string TallGrassId = "tall_grass";

    public const int WheelLogs = 1;
    public const int WheelCordage = 1;
    public const float WheelWeightKg = 3f;

    // --- The Hand Cart (Build tab, staged site): what the finished cart takes in total, delivered over trips, and the blows that finish it ---

    public const int CartWheels = 2;
    public const int CartLogs = 3;
    public const int CartBranches = 6;
    public const int CartCordage = 3;
    public const int BuildBlows = 12;

    // --- The load ---

    // The most a cart holds, in kilograms of what it carries (not counted against the pack's Encumbered line).
    public const float CapacityKg = 100f;

    // What a parked cart or its site keeps clear of other piles (the 2 m building spacing does not apply to it, so it can stand beside a site).
    public const float SpacingMetres = 1.2f;

    // Bulk building goods only: never food, water or tools.
    static readonly string[] Carried = { LogsId, BranchesId, FirewoodId, StoneId, ClayId, TallGrassId };

    public static bool Carries(string itemId) => Array.IndexOf(Carried, itemId) >= 0;

    // Shown on the transfer screen when nothing carried fits, and as the refusal for an item the cart does not take.
    public const string HoldsText = "A Hand Cart holds Logs, Branches, Firewood, Stone, Clay and Tall Grass, up to 100 kg - not food, water or tools.";

    // The one-line version for the transfer screen's status line.
    public const string HoldsShort = "Holds up to 100 kg of Logs, Branches, Firewood, Stone, Clay and Tall Grass.";

    public static string DoesntHold(string itemName) => $"the Hand Cart doesn't take {itemName} (Logs, Branches, Firewood, Stone, Clay and Tall Grass only)";

    // How many whole units of unitKg still fit in a cart already holding loadKg. A tiny tolerance keeps an exact fit (12 Logs = 96 kg, then 4 Branches)
    // from being lost to float error. A weightless or unknown item is limited by nothing but the caller's own count (int.MaxValue).
    public static int UnitsThatFit(float loadKg, float unitKg)
    {
        float room = RoomKg(loadKg);
        if (unitKg <= 0f)
            return room > 0f ? int.MaxValue : 0;
        return (int)Math.Floor((room + 0.0001f) / unitKg);
    }

    public static float RoomKg(float loadKg) => Math.Max(0f, CapacityKg - Math.Max(0f, loadKg));

    public static bool IsFull(float loadKg) => RoomKg(loadKg) < 0.0001f;

    // 0 for an empty cart, 1 for a full one (Phase 2's push speed reads this).
    public static float LoadFraction(float loadKg) => Math.Min(1f, Math.Max(0f, loadKg) / CapacityKg);

    // "12.5 / 100 kg" for the transfer screen's structure panel.
    public static string KgText(float loadKg) => $"{Math.Max(0f, loadKg):0.#} / {CapacityKg:0} kg";

    // Why a row can't take more: the cart is full, or the next one would not fit.
    public static string NoRoomReason(float loadKg) =>
        IsFull(loadKg) ? $"the cart is full ({KgText(loadKg)})" : $"won't fit - only {RoomKg(loadKg):0.#} kg of room left ({KgText(loadKg)})";

    // The 100 kg cap in the unit people think in: how many of an item a full, empty cart takes.
    public static int FullCartOf(float unitKg) => UnitsThatFit(0f, unitKg);

    // What dismantling gives back: half of each material, rounded down (the same rule as the other storage piles). The contents come back whole.
    public static int RefundOf(int cost) => Math.Max(0, cost) / 2;
}
