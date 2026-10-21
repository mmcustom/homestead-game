using System;
using System.Collections.Generic;
using UnityEngine;

// Difficulty_System.md's Trading Post (confirmed 2026-09-26) — the first real mechanic Economy_System.md has had. A
// two-way exchange, menu-only (TradingPostScreen, the T tab): sell what's gathered, hunted and made for money, and buy
// what can't be made yet. Open on every difficulty; only the starting cash differs (DifficultyManager).
//
// Prices are cents and live in the MarketData asset (Trading_Post_Market.md): the store's stock and what it buys, each with a base price
// that is exactly the dollar price it always had (the "camping kit" and hunting and fishing gear, and the hides and furs, meat, fish,
// forage, firewood, logs, stone and crafted goods the land gives). MarketManager turns a base price into the price paid now: a multiplier
// per pool that each unit sold pushes down and each game day pulls back toward the season's target, and a seasonal buy price for survival
// gear. Anything the store stocks goes back for 40% of its base buy price (at least $1; none of the $1-2 arrows, rounds and cordage), times
// the same multiplier. Selling always pays less than buying, so nothing can be bought and sold back at a profit.
public class TradingPost : MonoBehaviour, ISaveable
{
    public static TradingPost Instance { get; private set; }

    long money; // cents (Money)

    public long Cents => money;
    public event Action MoneyChanged;

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

    static MarketManager Market => MarketManager.Instance;

    // The store's stock in display order, with the price now (the season included), in cents.
    public static IReadOnlyList<(string item, long price)> Catalog
    {
        get
        {
            var list = new List<(string item, long price)>();
            if (Market != null)
                foreach (MarketRow row in Market.Stock)
                    list.Add((row.itemId, Market.BuyPrice(row.itemId)));
            return list;
        }
    }

    // Cents (0: not for sale).
    public static long BuyPrice(string itemId) => Market != null ? Market.BuyPrice(itemId) : 0;

    // What the store pays for one now, in cents (0: it doesn't buy it): its base price times the market multiplier.
    public static long SellPrice(string itemId) => Market != null ? Market.SellPrice(itemId) : 0;

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

    // Sells up to count of a carried item, unit by unit at the falling market price. Returns what happened (count 0: nothing sold).
    public SaleResult Sell(string itemId, int count)
    {
        InventoryManager inventory = InventoryManager.Instance;
        if (inventory == null || Market == null || count <= 0 || !Market.CanSell(itemId))
            return default;
        if (AudioManager.Instance != null)
            AudioManager.Instance.SilenceNextRemoval(); // a sale, not an item dropped
        int sold = inventory.RemoveFromPlayer(itemId, count);
        if (sold <= 0)
            return default;
        SaleResult result = Market.Sell(itemId, sold);
        money = Money.Add(money, result.totalCents);
        MoneyChanged?.Invoke();
        return result;
    }

    // Takes money out for something other than a purchase (a loan payment). Never lets cash go below zero.
    public bool TrySpend(long cents)
    {
        if (cents < 0 || cents > money)
            return false;
        money = Money.Sub(money, cents);
        MoneyChanged?.Invoke();
        return true;
    }

    // New Game: the cash reserve the player kept from their savings, in cents. Loading a save: nothing, until the save says otherwise.
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
