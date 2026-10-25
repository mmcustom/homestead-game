using System;
using System.Collections.Generic;

// Trading_Post_Market.md's item catalog and group table as pure data (the engine-free Homestead.Economy assembly, so edit-mode tests can
// check it). The MarketData asset (Resources/Market) is seeded from MarketCatalog.Rows and Groups and is what the game reads; this is the
// default set and the fallback if the asset is missing.

// What an item is grouped as for selling pressure and the seasonal price curve.
// Ash was appended after Stocked: the ids are saved in MarketData, so never reorder them.
public enum MarketGroup { WoodStone, Fish, GameMeat, Hides, Forage, Crafted, Farm, Stocked, Ash }

// Stocked: the store sells it (and buys it back at the sell-back share). Bought: the store only buys it from the player.
public enum MarketKind { Stocked, Bought }

public enum MarketTrend { Flat, Up, Down }

// One item. Pool: items that share a pool share one multiplier (cooked meat and fish share their raw item's pool; everything else is
// its own pool). baseCents is today's price: the buy price of a stocked item, the sell price of a bought one. seasonalBuy: the buy
// price drifts with the season (survival gear).
[Serializable]
public struct MarketRow
{
    public string itemId;
    public MarketGroup group;
    public string pool;
    public long baseCents;
    public MarketKind kind;
    public bool seasonalBuy;
}

// A group's selling rules. All in basis points, 10000 = 1.0: the drop per unit sold, the floor, and the level the price recovers toward
// in each season (Spring, Summer, Fall, Winter).
[Serializable]
public struct MarketGroupRule
{
    public MarketGroup group;
    public int dropBp;
    public int floorBp;
    public int spring, summer, fall, winter;

    public int TargetFor(int season)
    {
        switch (season)
        {
            case 0: return spring;
            case 1: return summer;
            case 2: return fall;
            default: return winter;
        }
    }

    public bool SeasonalChange => spring != summer || summer != fall || fall != winter;
}

// What selling a stack did. Also the hook the Phase B ledger records each sale from.
public struct SaleResult
{
    public int count;
    public long totalCents;
    public long firstUnitCents, lastUnitCents;
    public int newMultiplierBp;
}

public static class MarketCatalog
{
    static MarketRow Row(string id, MarketGroup group, long cents, MarketKind kind, string pool = null, bool seasonalBuy = false) =>
        new MarketRow { itemId = id, group = group, pool = pool ?? id, baseCents = cents, kind = kind, seasonalBuy = seasonalBuy };

    static MarketGroupRule Rule(MarketGroup group, int dropBp, int floorBp, int spring, int summer, int fall, int winter) =>
        new MarketGroupRule { group = group, dropBp = dropBp, floorBp = floorBp, spring = spring, summer = summer, fall = fall, winter = winter };

    // Generated from the baseline TradingPost.cs (b03ebc3) Stock and Buys tables, in that order: the 19 stocked items, then the 52 the
    // store only buys. Prices in cents.
    public static readonly MarketRow[] Rows =
    {
        Row("flint_and_steel", MarketGroup.Stocked, 800, MarketKind.Stocked),
        Row("knife", MarketGroup.Stocked, 400, MarketKind.Stocked),
        Row("canteen", MarketGroup.Stocked, 500, MarketKind.Stocked),
        Row("bucket", MarketGroup.Stocked, 600, MarketKind.Stocked),
        Row("cooking_pot", MarketGroup.Stocked, 1200, MarketKind.Stocked),
        Row("tarp", MarketGroup.Stocked, 800, MarketKind.Stocked, seasonalBuy: true),
        Row("sleeping_bag", MarketGroup.Stocked, 1500, MarketKind.Stocked, seasonalBuy: true),
        Row("tent", MarketGroup.Stocked, 2500, MarketKind.Stocked, seasonalBuy: true),
        Row("axe", MarketGroup.Stocked, 2000, MarketKind.Stocked),
        Row("hammer", MarketGroup.Stocked, 1000, MarketKind.Stocked),
        Row("fishing_rod", MarketGroup.Stocked, 1200, MarketKind.Stocked),
        Row("recurve_bow", MarketGroup.Stocked, 3000, MarketKind.Stocked),
        Row("arrows", MarketGroup.Stocked, 100, MarketKind.Stocked),
        Row("bolt_action_rifle", MarketGroup.Stocked, 6000, MarketKind.Stocked),
        Row("rifle_rounds", MarketGroup.Stocked, 100, MarketKind.Stocked),
        Row("cordage", MarketGroup.Stocked, 200, MarketKind.Stocked),
        Row("lamp_oil", MarketGroup.Stocked, 300, MarketKind.Stocked, seasonalBuy: true),
        Row("lumber", MarketGroup.Stocked, 800, MarketKind.Stocked),
        Row("weather_station", MarketGroup.Stocked, 2500, MarketKind.Stocked),
        Row("deer_hide", MarketGroup.Hides, 800, MarketKind.Bought),
        Row("deer_antlers", MarketGroup.Hides, 600, MarketKind.Bought),
        Row("small_furs", MarketGroup.Hides, 300, MarketKind.Bought),
        Row("feathers", MarketGroup.Hides, 100, MarketKind.Bought),
        Row("sinew", MarketGroup.Hides, 100, MarketKind.Bought),
        Row("venison", MarketGroup.GameMeat, 300, MarketKind.Bought),
        Row("turkey_meat", MarketGroup.GameMeat, 200, MarketKind.Bought),
        Row("waterfowl_meat", MarketGroup.GameMeat, 200, MarketKind.Bought),
        Row("small_game_meat", MarketGroup.GameMeat, 100, MarketKind.Bought),
        Row("chicken_meat", MarketGroup.Farm, 100, MarketKind.Bought),
        Row("cooked_venison", MarketGroup.GameMeat, 400, MarketKind.Bought, pool: "venison"),
        Row("cooked_turkey", MarketGroup.GameMeat, 300, MarketKind.Bought, pool: "turkey_meat"),
        Row("cooked_waterfowl", MarketGroup.GameMeat, 300, MarketKind.Bought, pool: "waterfowl_meat"),
        Row("cooked_small_game", MarketGroup.GameMeat, 200, MarketKind.Bought, pool: "small_game_meat"),
        Row("cooked_chicken", MarketGroup.Farm, 200, MarketKind.Bought, pool: "chicken_meat"),
        Row("bluegill", MarketGroup.Fish, 100, MarketKind.Bought),
        Row("crappie", MarketGroup.Fish, 100, MarketKind.Bought),
        Row("bass", MarketGroup.Fish, 200, MarketKind.Bought),
        Row("catfish", MarketGroup.Fish, 200, MarketKind.Bought),
        Row("cooked_bluegill", MarketGroup.Fish, 200, MarketKind.Bought, pool: "bluegill"),
        Row("cooked_crappie", MarketGroup.Fish, 200, MarketKind.Bought, pool: "crappie"),
        Row("cooked_bass", MarketGroup.Fish, 300, MarketKind.Bought, pool: "bass"),
        Row("cooked_catfish", MarketGroup.Fish, 300, MarketKind.Bought, pool: "catfish"),
        Row("blackberries", MarketGroup.Forage, 100, MarketKind.Bought),
        Row("raspberries", MarketGroup.Forage, 100, MarketKind.Bought),
        Row("wild_apples", MarketGroup.Forage, 100, MarketKind.Bought),
        Row("pawpaw", MarketGroup.Forage, 100, MarketKind.Bought),
        Row("chestnuts", MarketGroup.Forage, 100, MarketKind.Bought),
        Row("hickory_nuts", MarketGroup.Forage, 100, MarketKind.Bought),
        Row("walnuts", MarketGroup.Forage, 100, MarketKind.Bought),
        Row("morel", MarketGroup.Forage, 300, MarketKind.Bought),
        Row("oyster_mushroom", MarketGroup.Forage, 100, MarketKind.Bought),
        Row("dryads_saddle", MarketGroup.Forage, 100, MarketKind.Bought),
        Row("wild_onion", MarketGroup.Forage, 100, MarketKind.Bought),
        Row("cattail", MarketGroup.Forage, 100, MarketKind.Bought),
        Row("chicken_eggs", MarketGroup.Farm, 100, MarketKind.Bought),
        Row("goat_milk", MarketGroup.Farm, 100, MarketKind.Bought),
        Row("firewood", MarketGroup.WoodStone, 100, MarketKind.Bought),
        Row("logs", MarketGroup.WoodStone, 300, MarketKind.Bought),
        Row("stone", MarketGroup.WoodStone, 100, MarketKind.Bought),
        Row("stone_pick_axe", MarketGroup.Crafted, 400, MarketKind.Bought),
        Row("shovel", MarketGroup.Crafted, 400, MarketKind.Bought),
        Row("primitive_axe", MarketGroup.Crafted, 400, MarketKind.Bought),
        Row("rabbit_snare", MarketGroup.Crafted, 100, MarketKind.Bought),
        Row("box_trap", MarketGroup.Crafted, 200, MarketKind.Bought),
        Row("fish_trap", MarketGroup.Crafted, 300, MarketKind.Bought),
        Row("cane_pole", MarketGroup.Crafted, 200, MarketKind.Bought),
        Row("pouch", MarketGroup.Crafted, 300, MarketKind.Bought),
        Row("torch", MarketGroup.Crafted, 100, MarketKind.Bought),
        Row("bow_drill", MarketGroup.Crafted, 100, MarketKind.Bought),
        Row("lantern", MarketGroup.Crafted, 400, MarketKind.Bought),
        Row("primitive_bow", MarketGroup.Crafted, 200, MarketKind.Bought),
        // Early_Income_Design.md: raw Ash from burnt fuel (its own group), and 10 Sticks crafted into a cheap Kindling Bundle.
        Row(AshRules.ItemId, MarketGroup.Ash, AshRules.BaseCents, MarketKind.Bought),
        Row(AshRules.KindlingItemId, MarketGroup.Crafted, AshRules.KindlingBaseCents, MarketKind.Bought),
    };

    public static readonly MarketGroupRule[] Groups =
    {
        Rule(MarketGroup.WoodStone, 400, 2500, 10000, 8000, 10000, 15000),
        Rule(MarketGroup.Fish, 500, 2500, 10000, 9000, 10000, 10000),
        Rule(MarketGroup.GameMeat, 500, 2500, 10000, 10000, 10000, 13000),
        Rule(MarketGroup.Hides, 500, 2500, 10000, 10000, 10000, 13000),
        Rule(MarketGroup.Forage, 600, 2500, 8500, 8500, 8500, 8500),
        Rule(MarketGroup.Crafted, 1000, 1000, 10000, 10000, 10000, 10000),
        Rule(MarketGroup.Farm, 500, 2500, 10000, 10000, 10000, 10000),
        Rule(MarketGroup.Stocked, 800, 2500, 10000, 10000, 10000, 10000),
        Rule(MarketGroup.Ash, AshRules.DropBp, AshRules.FloorBp, 10000, 10000, 10000, 10000),
    };

    static MarketTable table;

    // The default set as a table (sell-back 40%).
    public static MarketTable Table => table ?? (table = new MarketTable(Rows, Groups));
}

// The rows and group rules indexed for lookup. Built once from the MarketData asset (or the defaults).
public sealed class MarketTable
{
    readonly Dictionary<string, MarketRow> rows = new Dictionary<string, MarketRow>();
    readonly Dictionary<MarketGroup, MarketGroupRule> groups = new Dictionary<MarketGroup, MarketGroupRule>();
    readonly Dictionary<string, MarketGroup> poolGroup = new Dictionary<string, MarketGroup>();
    readonly List<string> pools = new List<string>();
    readonly List<MarketRow> stock = new List<MarketRow>();
    readonly List<MarketRow> allRows = new List<MarketRow>();

    public int SellBackPercent { get; }

    // Anything wrong with the data (a duplicate item, a missing group, a pool spanning two groups). Empty for good data.
    public List<string> Problems { get; } = new List<string>();

    public IReadOnlyList<MarketRow> Rows => allRows;
    public IReadOnlyList<MarketRow> Stock => stock;
    public IReadOnlyList<string> Pools => pools;
    public IEnumerable<MarketGroupRule> Groups => groups.Values;

    public MarketTable(IEnumerable<MarketRow> rowData, IEnumerable<MarketGroupRule> groupData, int sellBackPercent = 40)
    {
        SellBackPercent = sellBackPercent;
        foreach (MarketGroupRule rule in groupData)
        {
            if (groups.ContainsKey(rule.group))
                Problems.Add("duplicate group " + rule.group);
            else
                groups[rule.group] = rule;
        }

        foreach (MarketRow source in rowData)
        {
            MarketRow row = source;
            if (string.IsNullOrEmpty(row.itemId))
            {
                Problems.Add("a row has no item id");
                continue;
            }

            if (string.IsNullOrEmpty(row.pool))
                row.pool = row.itemId;
            if (rows.ContainsKey(row.itemId))
            {
                Problems.Add("duplicate item " + row.itemId);
                continue;
            }

            if (!groups.ContainsKey(row.group))
                Problems.Add(row.itemId + " is in the missing group " + row.group);
            if (row.baseCents <= 0)
                Problems.Add(row.itemId + " has no base price");

            rows[row.itemId] = row;
            allRows.Add(row);
            if (row.kind == MarketKind.Stocked)
                stock.Add(row);

            if (SellBaseCents(row.itemId) <= 0)
                continue; // never bought from the player (arrows, rifle rounds, cordage): no sell row, so no pool

            if (poolGroup.TryGetValue(row.pool, out MarketGroup existing))
            {
                if (existing != row.group)
                    Problems.Add("pool " + row.pool + " spans " + existing + " and " + row.group);
            }
            else
            {
                poolGroup[row.pool] = row.group;
                pools.Add(row.pool);
            }
        }
    }

    public bool TryGetRow(string itemId, out MarketRow row) => rows.TryGetValue(itemId ?? "", out row);

    // What the store charges for a stocked item before the season, in cents (0: not stocked).
    public long BuyBaseCents(string itemId) =>
        TryGetRow(itemId, out MarketRow row) && row.kind == MarketKind.Stocked ? row.baseCents : 0;

    // The price one unit sells for at multiplier 1.0, in cents (0: the store doesn't buy it). A stocked item goes back at the sell-back
    // share of its BASE buy price (Money.SellBackCents); anything else has its own base price.
    public long SellBaseCents(string itemId)
    {
        if (!TryGetRow(itemId, out MarketRow row))
            return 0;
        return row.kind == MarketKind.Stocked ? Money.SellBackCents(row.baseCents, SellBackPercent) : row.baseCents;
    }

    public string PoolOf(string itemId) => TryGetRow(itemId, out MarketRow row) ? row.pool : null;

    public bool HasPool(string pool) => pool != null && poolGroup.ContainsKey(pool);

    public bool TryGetRule(MarketGroup group, out MarketGroupRule rule) => groups.TryGetValue(group, out rule);

    static MarketGroupRule Neutral => new MarketGroupRule
    {
        floorBp = MarketRules.One, spring = MarketRules.One, summer = MarketRules.One, fall = MarketRules.One, winter = MarketRules.One,
    };

    // The rule that governs an item's selling (a neutral rule if the data is broken, so nothing crashes).
    public MarketGroupRule RuleOf(string itemId) =>
        TryGetRow(itemId, out MarketRow row) && groups.TryGetValue(row.group, out MarketGroupRule rule) ? rule : Neutral;

    public MarketGroupRule RuleOfPool(string pool) =>
        pool != null && poolGroup.TryGetValue(pool, out MarketGroup group) && groups.TryGetValue(group, out MarketGroupRule rule) ? rule : Neutral;
}
