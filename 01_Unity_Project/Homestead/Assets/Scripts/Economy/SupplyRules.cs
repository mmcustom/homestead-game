// Everything the player needs or can build is on the Trading Post's shelf (Trading_Post_Market.md, "Change 2026-11-27"; Mike, 2026-11-27).
// Raw materials and things the player can make sell at ToolRules.StoreCents (2.5 times what the store pays); the items that would skip a whole
// production chain (Charcoal, Leather, Buckskin, Tanned Furs) sell at ChainStoreCents (4 times), so making them pays. The prices the store pays
// for the materials it did not deal in before are here; the rest keep their old rows' prices. Pure and engine-free so edit-mode tests cover it.
public static class SupplyRules
{
    public const long ChainMarkupPercent = 400;
    public static long ChainStoreCents(long sellCents) => sellCents * ChainMarkupPercent / 100;

    // What the store pays for the materials it did not deal in before (Claude's proposal, 2026-11-27).
    public const string SticksId = "sticks";
    public const long SticksCents = 10;          // 10 make a $1 Kindling Bundle
    public const string BranchesId = "branches";
    public const long BranchesCents = 20;
    public const string TallGrassId = "tall_grass";
    public const long TallGrassCents = 10;
    public const string ClayId = "clay";
    public const long ClayCents = 50;
    public const string SandId = "sand";
    public const long SandCents = 30;
    public const string TanbarkId = "tanbark";
    public const long TanbarkCents = 50;
    public const string WheelId = "wooden_wheel";
    public const long WheelCents = 200;          // half a Log
    public const string TripodId = "tripod";
    public const long TripodCents = 200;

    // Iron Bars are sold but never bought back (nobody buys them, as before): 2 Ore and 2 Charcoal at the store's prices, and the work.
    public const string IronBarId = "iron_bar";
    public const long IronBarStoreCents = 2000;
    public const long NotBoughtBack = -1;        // a MarketRow's sellCents: the store sells it but will not buy it
}
