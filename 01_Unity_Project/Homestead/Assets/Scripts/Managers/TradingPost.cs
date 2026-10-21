using System;
using System.Collections.Generic;
using UnityEngine;

// Difficulty_System.md's Trading Post (confirmed 2026-09-26) — the first real mechanic Economy_System.md has had. A
// two-way exchange, menu-only (TradingPostScreen, the T tab): sell what's gathered, hunted and made for money, and buy
// what can't be made yet. Open on every difficulty; only the starting cash differs (DifficultyManager).
//
// Prices are whole dollars (kept as cents), Claude Code's first pass for Mike to confirm by feel:
//   Buys (the store's stock) — the "camping kit" and hunting and fishing gear: Flint and Steel $8, Knife $4, Canteen $5,
//     Bucket $6, Cooking Pot $12, Tarp $8, Sleeping Bag $15, Tent $25, Axe $20, Hammer $10, Fishing Rod $12, Recurve
//     Bow $30, Arrow $1, Bolt-Action Rifle $60, Rifle Round $1, Cordage $2, Lamp Oil $3 (Portable Lighting, 2026-10-03:
//     the Lantern's fuel — 8 hours of light a bottle — until a free source like rendered tallow exists), Lumber $8
//     (plain basic lumber, 5 kg; nothing consumes it yet; sells back for $3 by the 40% rule), Weather Station $25 (Weather
//     panel Phase 2; sells back for $10).
//   Sells — anything the store stocks goes back for 40% (at least $1), except the $1-2 ammo and Cordage; and what the
//     land gives: hides and furs, antlers and feathers, meat and fish (a little more cooked), forage, firewood, logs and
//     stone. A day's gathering is worth a few dollars — Pioneer's first Flint and Steel is about eight Stone or a couple
//     of fish and some berries.
//   Selling always pays less than buying, so nothing can be bought and sold back at a profit.
public class TradingPost : MonoBehaviour, ISaveable
{
    public static TradingPost Instance { get; private set; }

    // The store's stock, in display order.
    static readonly (string item, long price)[] Stock =
    {
        ("flint_and_steel", D(8)), ("knife", D(4)), ("canteen", D(5)), ("bucket", D(6)), ("cooking_pot", D(12)), ("tarp", D(8)),
        ("sleeping_bag", D(15)), ("tent", D(25)), ("axe", D(20)), ("hammer", D(10)), ("fishing_rod", D(12)),
        ("recurve_bow", D(30)), ("arrows", D(1)), ("bolt_action_rifle", D(60)), ("rifle_rounds", D(1)), ("cordage", D(2)),
        ("lamp_oil", D(3)), ("lumber", D(8)), ("weather_station", D(25)),
    };

    // What the store pays for things it doesn't stock.
    static readonly Dictionary<string, long> Buys = new Dictionary<string, long>
    {
        { "deer_hide", D(8) }, { "deer_antlers", D(6) }, { "small_furs", D(3) }, { "feathers", D(1) }, { "sinew", D(1) },
        { "venison", D(3) }, { "turkey_meat", D(2) }, { "waterfowl_meat", D(2) }, { "small_game_meat", D(1) }, { "chicken_meat", D(1) },
        { "cooked_venison", D(4) }, { "cooked_turkey", D(3) }, { "cooked_waterfowl", D(3) }, { "cooked_small_game", D(2) }, { "cooked_chicken", D(2) },
        { "bluegill", D(1) }, { "crappie", D(1) }, { "bass", D(2) }, { "catfish", D(2) },
        { "cooked_bluegill", D(2) }, { "cooked_crappie", D(2) }, { "cooked_bass", D(3) }, { "cooked_catfish", D(3) },
        { "blackberries", D(1) }, { "raspberries", D(1) }, { "wild_apples", D(1) }, { "pawpaw", D(1) },
        { "chestnuts", D(1) }, { "hickory_nuts", D(1) }, { "walnuts", D(1) },
        { "morel", D(3) }, { "oyster_mushroom", D(1) }, { "dryads_saddle", D(1) }, { "wild_onion", D(1) }, { "cattail", D(1) },
        { "chicken_eggs", D(1) }, { "goat_milk", D(1) },
        { "firewood", D(1) }, { "logs", D(3) }, { "stone", D(1) },
        { "stone_pick_axe", D(4) }, { "shovel", D(4) }, { "primitive_axe", D(4) }, { "rabbit_snare", D(1) }, { "box_trap", D(2) },
        { "fish_trap", D(3) }, { "cane_pole", D(2) }, { "pouch", D(3) }, { "torch", D(1) }, { "bow_drill", D(1) }, { "lantern", D(4) },
        { "primitive_bow", D(2) },
    };

    // Whole dollars as cents, so the price tables below read as dollars.
    static long D(int dollars) => Money.Dollars(dollars);

    long money; // cents (Money)

    public long Cents => money;
    public event Action MoneyChanged;

    public static IReadOnlyList<(string item, long price)> Catalog => Stock;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (Instance == this && SaveManager.Instance != null)
            SaveManager.Instance.Register(this);
    }

    void OnDestroy()
    {
        if (Instance != this)
            return;

        if (SaveManager.Instance != null)
            SaveManager.Instance.Unregister(this);
        Instance = null;
    }

    // Cents (0: not for sale).
    public static long BuyPrice(string itemId)
    {
        foreach ((string item, long price) in Stock)
            if (item == itemId)
                return price;
        return 0;
    }

    // What the store pays for one, in cents (0: it doesn't buy it). Stocked items go back at 40% (Money.SellBackCents; none of
    // the under-$3 ammo and cordage); anything else has its own fixed price.
    public static long SellPrice(string itemId)
    {
        long stocked = BuyPrice(itemId);
        if (stocked > 0)
            return Money.SellBackCents(stocked);
        return Buys.TryGetValue(itemId, out long price) ? price : 0;
    }

    // Buys up to count. Returns how many were bought, and why not all (null if all were).
    public int Buy(string itemId, int count, out string problem)
    {
        problem = null;
        InventoryManager inventory = InventoryManager.Instance;
        ItemDefinition item = ItemDatabase.Get(itemId);
        long price = BuyPrice(itemId);
        if (inventory == null || item == null || price <= 0 || count <= 0)
        {
            problem = "Not for sale.";
            return 0;
        }

        long affordable = money / price;
        int fits = inventory.Player.SpaceFor(item);
        int n = Money.BuyCount(money, price, count, fits);
        if (n <= 0)
        {
            problem = affordable <= 0 ? $"Not enough money — {item.DisplayName} costs {Money.Format(price)}." : "No room to carry it.";
            return 0;
        }
        n = inventory.AddToPlayer(itemId, n);
        money = Money.Sub(money, Money.Mul(price, n));
        MoneyChanged?.Invoke();
        if (n < count)
            problem = affordable < count ? "Ran out of money." : "Ran out of room.";
        return n;
    }

    // Sells up to count of a carried item. Returns the money made, in cents.
    public long Sell(string itemId, int count)
    {
        InventoryManager inventory = InventoryManager.Instance;
        long price = SellPrice(itemId);
        if (inventory == null || price <= 0 || count <= 0)
            return 0;
        if (AudioManager.Instance != null)
            AudioManager.Instance.SilenceNextRemoval(); // a sale, not an item dropped
        int sold = inventory.RemoveFromPlayer(itemId, count);
        long made = Money.Mul(price, sold);
        money = Money.Add(money, made);
        MoneyChanged?.Invoke();
        return made;
    }

    // New Game: the tier's starting cash, in cents. Loading a save: nothing, until the save says otherwise.
    public void ResetMoney(long cents)
    {
        money = Math.Max(0L, cents);
        MoneyChanged?.Invoke();
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public void DebugAddMoney(int dollars) => ResetMoney(Money.Add(money, Money.Dollars(dollars)));
#endif

    public TradingPostSaveData CaptureState() => TradingPostSaveData.FromCents(money);
    public void RestoreState(TradingPostSaveData data) => ResetMoney(data.ToCents());

    string ISaveable.SaveFile => "player";
    string ISaveable.SaveKey => "money";
    object ISaveable.CaptureState() => CaptureState();
    void ISaveable.RestoreState(string json) => RestoreState(JsonUtility.FromJson<TradingPostSaveData>(json));
}
