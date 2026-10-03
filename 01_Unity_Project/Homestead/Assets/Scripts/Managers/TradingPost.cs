using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct TradingPostSaveData
{
    public int money;
}

// Difficulty_System.md's Trading Post (confirmed 2026-09-26) — the first real mechanic Economy_System.md has had. A
// two-way exchange, menu-only (TradingPostScreen, the T tab): sell what's gathered, hunted and made for money, and buy
// what can't be made yet. Open on every difficulty; only the starting cash differs (DifficultyManager).
//
// Prices are whole dollars, Claude Code's first pass for Mike to confirm by feel:
//   Buys (the store's stock) — the "camping kit" and hunting and fishing gear: Flint and Steel $8, Knife $4, Canteen $5,
//     Bucket $6, Cooking Pot $12, Tarp $8, Sleeping Bag $15, Tent $25, Axe $20, Hammer $10, Fishing Rod $12, Recurve
//     Bow $30, Arrow $1, Bolt-Action Rifle $60, Rifle Round $1, Cordage $2, Lamp Oil $3 (Portable Lighting, 2026-10-03:
//     the Lantern's fuel — 8 hours of light a bottle — until a free source like rendered tallow exists).
//   Sells — anything the store stocks goes back for 40% (at least $1), except the $1-2 ammo and Cordage; and what the
//     land gives: hides and furs, antlers and feathers, meat and fish (a little more cooked), forage, firewood, logs and
//     stone. A day's gathering is worth a few dollars — Pioneer's first Flint and Steel is about eight Stone or a couple
//     of fish and some berries.
//   Selling always pays less than buying, so nothing can be bought and sold back at a profit.
public class TradingPost : MonoBehaviour, ISaveable
{
    public static TradingPost Instance { get; private set; }

    // The store's stock, in display order.
    static readonly (string item, int price)[] Stock =
    {
        ("flint_and_steel", 8), ("knife", 4), ("canteen", 5), ("bucket", 6), ("cooking_pot", 12), ("tarp", 8),
        ("sleeping_bag", 15), ("tent", 25), ("axe", 20), ("hammer", 10), ("fishing_rod", 12),
        ("recurve_bow", 30), ("arrows", 1), ("bolt_action_rifle", 60), ("rifle_rounds", 1), ("cordage", 2),
        ("lamp_oil", 3),
    };

    // What the store pays for things it doesn't stock.
    static readonly Dictionary<string, int> Buys = new Dictionary<string, int>
    {
        { "deer_hide", 8 }, { "deer_antlers", 6 }, { "small_furs", 3 }, { "feathers", 1 }, { "sinew", 1 },
        { "venison", 3 }, { "turkey_meat", 2 }, { "waterfowl_meat", 2 }, { "small_game_meat", 1 }, { "chicken_meat", 1 },
        { "cooked_venison", 4 }, { "cooked_turkey", 3 }, { "cooked_waterfowl", 3 }, { "cooked_small_game", 2 }, { "cooked_chicken", 2 },
        { "bluegill", 1 }, { "crappie", 1 }, { "bass", 2 }, { "catfish", 2 },
        { "cooked_bluegill", 2 }, { "cooked_crappie", 2 }, { "cooked_bass", 3 }, { "cooked_catfish", 3 },
        { "blackberries", 1 }, { "raspberries", 1 }, { "wild_apples", 1 }, { "pawpaw", 1 },
        { "chestnuts", 1 }, { "hickory_nuts", 1 }, { "walnuts", 1 },
        { "morel", 3 }, { "oyster_mushroom", 1 }, { "dryads_saddle", 1 }, { "wild_onion", 1 }, { "cattail", 1 },
        { "chicken_eggs", 1 }, { "goat_milk", 1 },
        { "firewood", 1 }, { "logs", 3 }, { "stone", 1 },
        { "stone_pick_axe", 4 }, { "shovel", 4 }, { "primitive_axe", 4 }, { "rabbit_snare", 1 }, { "box_trap", 2 },
        { "fish_trap", 3 }, { "cane_pole", 2 }, { "pouch", 3 }, { "torch", 1 }, { "lantern", 4 },
    };

    int money;

    public int Money => money;
    public event Action MoneyChanged;

    public static IReadOnlyList<(string item, int price)> Catalog => Stock;

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

    public static int BuyPrice(string itemId)
    {
        foreach ((string item, int price) in Stock)
            if (item == itemId)
                return price;
        return 0;
    }

    // What the store pays for one (0: it doesn't buy it).
    public static int SellPrice(string itemId)
    {
        int stocked = BuyPrice(itemId);
        if (stocked > 0)
            return stocked >= 3 ? Mathf.Max(1, Mathf.FloorToInt(stocked * 0.4f)) : 0; // no buying back ammo and cordage
        return Buys.TryGetValue(itemId, out int price) ? price : 0;
    }

    // Buys up to count. Returns how many were bought, and why not all (null if all were).
    public int Buy(string itemId, int count, out string problem)
    {
        problem = null;
        InventoryManager inventory = InventoryManager.Instance;
        ItemDefinition item = ItemDatabase.Get(itemId);
        int price = BuyPrice(itemId);
        if (inventory == null || item == null || price <= 0 || count <= 0)
        {
            problem = "Not for sale.";
            return 0;
        }

        int affordable = money / price;
        int fits = inventory.Player.SpaceFor(item);
        int n = Mathf.Min(count, Mathf.Min(affordable, fits));
        if (n <= 0)
        {
            problem = affordable <= 0 ? $"Not enough money — {item.DisplayName} costs ${price}." : "No room to carry it.";
            return 0;
        }
        n = inventory.AddToPlayer(itemId, n);
        money -= n * price;
        MoneyChanged?.Invoke();
        if (n < count)
            problem = affordable < count ? "Ran out of money." : "Ran out of room.";
        return n;
    }

    // Sells up to count of a carried item. Returns the money made.
    public int Sell(string itemId, int count)
    {
        InventoryManager inventory = InventoryManager.Instance;
        int price = SellPrice(itemId);
        if (inventory == null || price <= 0 || count <= 0)
            return 0;
        if (AudioManager.Instance != null)
            AudioManager.Instance.SilenceNextRemoval(); // a sale, not an item dropped
        int sold = inventory.RemoveFromPlayer(itemId, count);
        money += sold * price;
        MoneyChanged?.Invoke();
        return sold * price;
    }

    // New Game: the tier's starting cash. Loading a save: nothing, until the save says otherwise.
    public void ResetMoney(int amount)
    {
        money = Mathf.Max(0, amount);
        MoneyChanged?.Invoke();
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public void DebugAddMoney(int amount) => ResetMoney(money + amount);
#endif

    public TradingPostSaveData CaptureState() => new TradingPostSaveData { money = money };
    public void RestoreState(TradingPostSaveData data) => ResetMoney(data.money);

    string ISaveable.SaveFile => "player";
    string ISaveable.SaveKey => "money";
    object ISaveable.CaptureState() => CaptureState();
    void ISaveable.RestoreState(string json) => RestoreState(JsonUtility.FromJson<TradingPostSaveData>(json));
}
