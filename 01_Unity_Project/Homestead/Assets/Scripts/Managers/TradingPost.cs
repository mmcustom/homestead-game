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
                    if (OnShelf(row.itemId))
                        list.Add((row.itemId, Market.BuyPrice(row.itemId)));
            return list;
        }
    }

    // Whether a stocked item is on the shelf today: everything is, except Tomato Plants out of their Spring season (GardenRules.TomatoPlantsInSeason).
    public static bool OnShelf(string itemId) =>
        itemId != "tomato_plant" || GardenRules.TomatoPlantsInSeason(CalendarRules.Current, TimeManager.Instance != null ? TimeManager.Instance.TotalDays : 0);

    // Cents (0: not for sale, which includes a seasonal item out of season).
    public static long BuyPrice(string itemId) => Market != null && OnShelf(itemId) ? Market.BuyPrice(itemId) : 0;

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
        long spent = Money.Mul(price, n);
        if (LedgerManager.Instance != null)
            LedgerManager.Instance.Record(LedgerCategories.ForPurchase(itemId), spent); // before the cash changes
        money = Money.Sub(money, spent);
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
        if (LedgerManager.Instance != null && Market.Table.TryGetRow(itemId, out MarketRow soldRow))
            LedgerManager.Instance.RecordSale(LedgerCategories.ForSale(soldRow.group), itemId, result.count, result.totalCents); // before the cash changes
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

    // --- Delivery (slice D1, DeliveryRules): the order list, placing it, and the deliveries on their way ---

    readonly List<DeliveryLine> order = new List<DeliveryLine>();          // being put together at the store; not saved
    readonly List<DeliveryOrder> deliveries = new List<DeliveryOrder>();   // paid for and on their way; saved
    bool waitingTold;

    public IReadOnlyList<DeliveryLine> Order => order;
    public IReadOnlyList<DeliveryOrder> Deliveries => deliveries;
    public event Action OrderChanged;

    // Adds to the order list (no weight limit). Returns how many were added.
    public int AddToOrder(string itemId, int count)
    {
        if (count <= 0 || BuyPrice(itemId) <= 0 || ItemDatabase.Get(itemId) == null)
            return 0;
        DeliveryLine line = order.Find(l => l.itemId == itemId);
        if (line == null)
            order.Add(line = new DeliveryLine { itemId = itemId, count = 0 });
        line.count += count;
        OrderChanged?.Invoke();
        return count;
    }

    public void RemoveFromOrder(string itemId)
    {
        if (order.RemoveAll(l => l.itemId == itemId) > 0)
            OrderChanged?.Invoke();
    }

    public void ClearOrder()
    {
        order.Clear();
        OrderChanged?.Invoke();
    }

    // The goods at today's prices, and their weight.
    public long OrderGoodsCents
    {
        get
        {
            long total = 0;
            foreach (DeliveryLine line in order)
                total = Money.Add(total, Money.Mul(BuyPrice(line.itemId), line.count));
            return total;
        }
    }

    public float OrderKg
    {
        get
        {
            float kg = 0f;
            foreach (DeliveryLine line in order)
                kg += (ItemDatabase.Get(line.itemId)?.WeightKg ?? 0f) * line.count;
            return kg;
        }
    }

    // Pays for the order and the fee at once and sends it on its way to the Delivery Pad. Returns what to tell the player.
    public bool PlaceOrder(out string message)
    {
        if (order.Count == 0)
        {
            message = DeliveryRules.EmptyOrder;
            return false;
        }
        if (WoodManager.Instance == null || WoodManager.Instance.DeliveryPad == null)
        {
            message = DeliveryRules.NoPad;
            return false;
        }
        long goods = OrderGoodsCents, fee = DeliveryRules.FeeFor(goods), total = Money.Add(goods, fee);
        if (total > money)
        {
            message = $"Not enough money - the order comes to {Money.Format(total)}.";
            return false;
        }
        float kg = OrderKg;
        int today = TimeManager.Instance != null ? TimeManager.Instance.TotalDays : 0;
        var sent = new DeliveryOrder { arriveHours = DeliveryRules.ArrivalHours(today, kg) };
        foreach (DeliveryLine line in order)
        {
            if (LedgerManager.Instance != null)
                LedgerManager.Instance.Record(LedgerCategories.ForPurchase(line.itemId), Money.Mul(BuyPrice(line.itemId), line.count)); // before the cash changes
            sent.lines.Add(new DeliveryLine { itemId = line.itemId, count = line.count });
        }
        if (fee > 0 && LedgerManager.Instance != null)
            LedgerManager.Instance.Record(LedgerCategory.DeliveryFees, fee);
        money = Money.Sub(money, total);
        deliveries.Add(sent);
        order.Clear();
        message = DeliveryRules.Placed(DeliveryRules.DaysFor(kg));
        MoneyChanged?.Invoke();
        OrderChanged?.Invoke();
        return true;
    }

    // Days until the next delivery comes (0: today), or -1 if none is on its way.
    public int NextDeliveryInDays()
    {
        if (deliveries.Count == 0 || TimeManager.Instance == null)
            return -1;
        double soonest = double.MaxValue;
        foreach (DeliveryOrder d in deliveries)
            soonest = Math.Min(soonest, d.arriveHours);
        return Math.Max(0, (int)(soonest / 24.0) - TimeManager.Instance.TotalDays);
    }

    // --- The Pole Barn (slice B1a, BarnRules): its kit, delivered onto its site, and the crew ---

    readonly List<CrewJob> crews = new List<CrewJob>();
    readonly HashSet<int> owedTold = new HashSet<int>();

    public IReadOnlyList<CrewJob> Crews => crews;
    public CrewJob CrewFor(int siteId) => crews.Find(c => c.siteId == siteId);
    public bool KitOrdered(int siteId) => deliveries.Exists(d => d.siteId == siteId);

    static int Today => TimeManager.Instance != null ? TimeManager.Instance.TotalDays : 0;

    static WoodPileState PileById(int id)
    {
        if (WoodManager.Instance != null)
            foreach (WoodPileState pile in WoodManager.Instance.Piles)
                if (pile.id == id)
                    return pile;
        return null;
    }

    // Whether this site has been started by the player (the kit on its way or on it, or a stage done).
    bool Started(WoodPileState site) => KitOrdered(site.id) || site.jobCount > 0 || site.buildProgress > 0f || !site.IsEmpty;

    public bool OrderBarnKit(WoodPileState site, out string message)
    {
        if (site == null || !WoodManager.IsBarnSite(site.kind))
        {
            message = BarnRules.NoSite;
            return false;
        }
        if (CrewFor(site.id) != null)
        {
            message = BarnRules.CrewHasIt;
            return false;
        }
        if (Started(site))
        {
            message = "The kit for this site is already ordered";
            return false;
        }
        BarnRules.Size size = WoodManager.BarnSizeOf(site.kind);
        long price = BarnRules.KitCents(size);
        if (price > money)
        {
            message = $"Not enough money - the kit is {Money.Format(price)}.";
            return false;
        }
        if (LedgerManager.Instance != null)
            LedgerManager.Instance.Record(LedgerCategory.BuyBuildingMaterials, price);
        money = Money.Sub(money, price);
        var order = new DeliveryOrder { siteId = site.id, arriveHours = DeliveryRules.ArrivalHours(Today, BarnRules.KitKg(size)) };
        foreach (WoodStack part in WoodManager.BarnKit(size))
            order.lines.Add(new DeliveryLine { itemId = part.itemId, count = part.count });
        deliveries.Add(order);
        message = $"Kit ordered - it arrives on the site {DeliveryRules.DayWord(DeliveryRules.DaysFor(BarnRules.KitKg(size)))} at {DeliveryRules.ArrivalHour} a.m.";
        MoneyChanged?.Invoke();
        OrderChanged?.Invoke();
        return true;
    }

    public bool HireBarnCrew(WoodPileState site, out string message)
    {
        if (site == null || !WoodManager.IsBarnSite(site.kind))
        {
            message = BarnRules.NoSite;
            return false;
        }
        if (CrewFor(site.id) != null)
        {
            message = BarnRules.CrewHasIt;
            return false;
        }
        if (Started(site))
        {
            message = "You've started this one yourself - the crew builds from a bare site";
            return false;
        }
        BarnRules.Size size = WoodManager.BarnSizeOf(site.kind);
        long deposit = BarnRules.CrewDeposit(size);
        if (deposit > money)
        {
            message = $"Not enough money - the crew wants {Money.Format(deposit)} now.";
            return false;
        }
        if (LedgerManager.Instance != null)
            LedgerManager.Instance.Record(LedgerCategory.Contractors, deposit);
        money = Money.Sub(money, deposit);
        site.jobItem = BarnRules.CrewMarker;
        crews.Add(new CrewJob { siteId = site.id, size = (int)size, bookedDay = Today });
        WoodManager.Instance.NotifyChanged(site);
        message = BarnRules.CrewBooked(size);
        MoneyChanged?.Invoke();
        OrderChanged?.Invoke();
        return true;
    }

    // The crews' hour: the site shows how far along they are; when they are done the balance is taken (as soon as there is the cash) and the barn stands.
    void CrewsUpdate(double now)
    {
        for (int i = crews.Count - 1; i >= 0; i--)
        {
            CrewJob job = crews[i];
            WoodPileState site = PileById(job.siteId);
            if (site == null || !WoodManager.IsBarnSite(site.kind))
            {
                crews.RemoveAt(i);
                continue;
            }
            var size = (BarnRules.Size)job.size;
            float progress = BarnRules.CrewProgress(size, job.bookedDay, now);
            int stage = BarnRules.CrewStage(progress);
            float within = progress * 3f - stage;
            if (progress < 1f)
            {
                if (site.jobCount != stage || Mathf.Abs(site.buildProgress - within) > 0.05f)
                {
                    site.jobCount = stage;
                    site.buildProgress = Mathf.Clamp01(within);
                    WoodManager.Instance.NotifyChanged(site);
                }
                continue;
            }
            if (!job.balancePaid)
            {
                long balance = BarnRules.CrewBalance(size);
                if (balance > money)
                {
                    if (owedTold.Add(job.siteId))
                        ToolStatus.Banner(BarnRules.CrewOwed(size), 10f);
                    continue;
                }
                if (LedgerManager.Instance != null)
                    LedgerManager.Instance.Record(LedgerCategory.Contractors, balance);
                money = Money.Sub(money, balance);
                job.balancePaid = true;
                MoneyChanged?.Invoke();
            }
            // Done and paid: the barn stands (the kit's parts are used up as the site finishes).
            site.jobItem = null;
            site.contents.Clear();
            foreach (WoodStack part in WoodManager.BarnKit(size))
                site.Add(part.itemId, part.count);
            site.jobCount = BarnRules.Stages - 1;
            site.buildProgress = 0f;
            crews.RemoveAt(i);
            owedTold.Remove(job.siteId);
            if (WellsAndOuthouses.Instance != null && WellsAndOuthouses.Instance.FinishSite(site))
                ToolStatus.Banner(BarnRules.CrewDone(size), 10f);
            OrderChanged?.Invoke();
        }
    }

    void Update()
    {
        if (Instance != this || (deliveries.Count == 0 && crews.Count == 0) || TimeManager.Instance == null || WoodManager.Instance == null)
            return;
        double now = TimeManager.Instance.TotalHours;
        CrewsUpdate(now);
        bool arrived = false, waiting = false, kit = false;
        for (int i = deliveries.Count - 1; i >= 0; i--)
        {
            if (!DeliveryRules.Due(deliveries[i].arriveHours, now))
                continue;
            // A Pole Barn kit goes onto its site; if the site is gone, to the pad like any other order.
            WoodPileState onSite = deliveries[i].siteId >= 0 ? PileById(deliveries[i].siteId) : null;
            if (onSite != null && !WoodManager.IsBarnSite(onSite.kind))
                onSite = null;
            if (onSite != null)
            {
                foreach (DeliveryLine line in deliveries[i].lines)
                    onSite.Add(line.itemId, line.count);
                WoodManager.Instance.NotifyChanged(onSite);
                deliveries.RemoveAt(i);
                kit = true;
                continue;
            }
            WoodPileState pad = WoodManager.Instance.DeliveryPad;
            if (pad == null)
            {
                waiting = true;
                continue;
            }
            int fresh = SpoilageManager.Convert(SpoilageManager.FreshStamp(), FreshClock.Outdoor, SpoilageManager.ClockOf(pad.kind));
            foreach (DeliveryLine line in deliveries[i].lines)
                pad.Add(line.itemId, line.count, ItemDatabase.Get(line.itemId)?.IsPerishable == true ? fresh : 0);
            WoodManager.Instance.NotifyChanged(pad);
            deliveries.RemoveAt(i);
            arrived = true;
        }
        if (kit)
            ToolStatus.Banner("The Pole Barn kit is on its site - build it with the Hammer, stage by stage", 8f);
        if (arrived)
        {
            ToolStatus.Banner(DeliveryRules.Arrived, 8f);
            waitingTold = false;
        }
        else if (waiting && !waitingTold)
        {
            ToolStatus.Banner(DeliveryRules.Waiting, 8f);
            waitingTold = true;
        }
    }

    // A new game, or before a load: nothing on its way.
    public void ResetDeliveries()
    {
        deliveries.Clear();
        crews.Clear();
        owedTold.Clear();
        order.Clear();
        waitingTold = false;
        OrderChanged?.Invoke();
    }

    public TradingPostSaveData CaptureState()
    {
        TradingPostSaveData data = TradingPostSaveData.FromCents(money);
        data.deliveries = new List<DeliveryOrder>(deliveries);
        data.crews = new List<CrewJob>(crews);
        return data;
    }

    public void RestoreState(TradingPostSaveData data)
    {
        ResetMoney(data.ToCents());
        deliveries.Clear();
        if (data.deliveries != null)
            foreach (DeliveryOrder d in data.deliveries)
                if (d != null && d.lines != null && d.lines.Count > 0)
                    deliveries.Add(d);
        crews.Clear();
        owedTold.Clear();
        if (data.crews != null)
            foreach (CrewJob c in data.crews)
                if (c != null)
                    crews.Add(c);
    }

    string ISaveable.SaveFile => "player";
    string ISaveable.SaveKey => "money";
    object ISaveable.CaptureState() => CaptureState();
    void ISaveable.RestoreState(string json) => RestoreState(JsonUtility.FromJson<TradingPostSaveData>(json));
}
