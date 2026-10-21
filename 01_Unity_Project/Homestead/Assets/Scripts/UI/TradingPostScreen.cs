using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Difficulty_System.md's Trading Post as a game screen (T): menu-only, no place or trader in the world. The store's
// stock on the left with Buy buttons, everything carried that it'll take on the right with Sell buttons, and the money
// and difficulty across the top, and a one-line market note under them. Every row is two lines (QuantityRowUi): the name, the price
// and, on Sell rows, a trend marker (green +: below its seasonal target and recovering; red -: above it and falling; =: steady) and how
// far from normal the price is; then a quantity stepper (Quantity_Controls.md), the live total for the amount chosen and the button.
// A plain click moves the amount shown in the stepper (1 by default, back to 1 after every move); Shift-click keeps the old shortcut:
// buys ten (arrows, rounds, cordage) or sells the whole stack, unit by unit at the falling price. The most that can be bought is what the
// cash and the carry weight allow (Money.BuyCount); the most that can be sold is what is carried. The Sell total is the market's own
// quote for that many, the Buy total is price times amount. The Sell list is ordered by base price, never by current price, so rows
// don't jump under the cursor as prices move. Prices and the buying and selling themselves live in TradingPost and MarketManager.
public class TradingPostScreen : GameScreen
{
    RectTransform buyList, sellList;
    Text moneyLabel, statusLabel, marketLabel;
    InventoryManager watchedInventory;
    TradingPost watchedPost;
    MarketManager watchedMarket;

    // The amount chosen in each row's stepper, by item id. The rows are rebuilt on every refresh, so the amounts live here; they go
    // back to 1 after a move and are cleared when the screen hides.
    readonly Dictionary<string, int> buyAmounts = new Dictionary<string, int>();
    readonly Dictionary<string, int> sellAmounts = new Dictionary<string, int>();

    const string UpColor = "#C7D68C", DownColor = "#E07A6A";

    public override string Title => "Trading Post";

    public override void Build(RectTransform area)
    {
        moneyLabel = UiKit.Text(area, "Money", "", 30, UiKit.Cream, TextAnchor.MiddleLeft);
        Top(moneyLabel.rectTransform, 0f, 40f, 0f, 1f);
        statusLabel = UiKit.Text(area, "Status", "", 18, UiKit.Muted, TextAnchor.MiddleRight);
        Top(statusLabel.rectTransform, 0f, 40f, 0.4f, 1f);

        marketLabel = UiKit.Text(area, "Market", "", 16, UiKit.Muted, TextAnchor.MiddleLeft);
        Top(marketLabel.rectTransform, 40f, 24f, 0f, 1f);

        buyList = Column(area, "Buy", 0f, 0.485f);
        sellList = Column(area, "Sell", 0.515f, 1f);
    }

    RectTransform Column(RectTransform area, string heading, float xMin, float xMax)
    {
        RectTransform column = UiKit.Rect(heading, area).Region(xMin, 0f, xMax, 1f);
        column.offsetMax = new Vector2(0f, -78f);
        Text label = UiKit.Text(column, "Heading", heading == "Buy" ? "Buy" : "Sell", 26, UiKit.Accent, TextAnchor.MiddleLeft, FontStyle.Italic);
        Top(label.rectTransform, 0f, 36f, 0f, 1f);
        Text sub = UiKit.Text(column, "Sub", heading == "Buy" ? "What the store has    (Shift-click: 10)" : "What you carry that it'll take    (Shift-click: the lot)",
                              16, UiKit.Muted, TextAnchor.MiddleRight);
        Top(sub.rectTransform, 0f, 36f, 0.3f, 1f);
        RectTransform list = UiKit.ScrollList(column, "List");
        ((RectTransform)list.parent).Fill(0f, 0f, 0f, 42f);
        return list;
    }

    public override void OnShow()
    {
        watchedInventory = InventoryManager.Instance;
        watchedPost = TradingPost.Instance;
        if (watchedInventory != null)
            watchedInventory.Player.Changed += Refresh;
        if (watchedPost != null)
            watchedPost.MoneyChanged += Refresh;
        watchedMarket = MarketManager.Instance;
        if (watchedMarket != null)
            watchedMarket.Changed += Refresh;
        statusLabel.text = "";
        buyAmounts.Clear();
        sellAmounts.Clear();
        Refresh();
    }

    public override void OnHide()
    {
        Unsubscribe();
        buyAmounts.Clear();
        sellAmounts.Clear();
        UiKit.ReleaseFocus();
    }

    void OnDestroy() => Unsubscribe();

    void Unsubscribe()
    {
        if (watchedInventory != null)
            watchedInventory.Player.Changed -= Refresh;
        if (watchedPost != null)
            watchedPost.MoneyChanged -= Refresh;
        if (watchedMarket != null)
            watchedMarket.Changed -= Refresh;
        watchedInventory = null;
        watchedPost = null;
        watchedMarket = null;
    }

    void Refresh()
    {
        TradingPost post = TradingPost.Instance;
        InventoryManager inventory = InventoryManager.Instance;
        moneyLabel.text = post != null
            ? $"{Money.Format(post.Cents)}   <size=18><color=#EDE3C799>{DifficultyManager.Name(DifficultyManager.Tier)}</color></size>"
            : "";

        MarketManager market = MarketManager.Instance;
        marketLabel.text = market != null ? market.Note : "";

        UiKit.Clear(buyList);
        UiKit.Clear(sellList);
        if (post == null || inventory == null || market == null)
            return;

        foreach ((string item, long price) in TradingPost.Catalog)
        {
            ItemDefinition definition = ItemDatabase.Get(item);
            if (definition == null)
                continue;
            BuyRow(post, inventory, item, definition, price);
        }

        // Everything carried the store will take, one row per kind, highest BASE price first, then by name (never by the price now).
        var carried = new List<(ItemDefinition item, int count, long baseCents)>();
        var seen = new HashSet<string>();
        foreach (ItemStack stack in inventory.Player.Stacks)
        {
            if (!seen.Add(stack.itemId))
                continue;
            ItemDefinition definition = ItemDatabase.Get(stack.itemId);
            if (definition != null && market.CanSell(stack.itemId))
                carried.Add((definition, inventory.Player.Count(stack.itemId), market.SellBase(stack.itemId)));
        }
        carried.Sort((a, b) =>
        {
            int byBase = b.baseCents.CompareTo(a.baseCents);
            return byBase != 0 ? byBase : string.CompareOrdinal(a.item.DisplayName, b.item.DisplayName);
        });
        if (carried.Count == 0)
            UiKit.Height(UiKit.Text(sellList, "Empty", "Nothing you're carrying that the store buys. Hides, furs, meat, fish, forage, firewood, logs and stone all sell.", 18, UiKit.Muted), 60f);
        foreach ((ItemDefinition item, int count, long baseCents) in carried)
            SellRow(inventory, market, item, count);
    }

    // One store item: the most that can be bought is what the cash and the carry weight allow.
    void BuyRow(TradingPost post, InventoryManager inventory, string item, ItemDefinition definition, long price)
    {
        int max = Money.BuyCount(post.Cents, price, QuantityRules.MaxAmount, inventory.Player.SpaceFor(definition));
        int carriedCount = inventory.Player.Count(item);
        string have = carriedCount > 0 ? $"  <size=16><color=#C7D68C>have {carriedCount}</color></size>" : "";
        string why = max > 0 ? "" : post.Cents < price ? "  <size=16><color=#E07A6A>can't afford</color></size>" : "  <size=16><color=#E07A6A>too heavy to carry more</color></size>";

        QuantityRowUi row = null;
        row = QuantityRowUi.Create(buyList, "Buy", true, buyAmounts.TryGetValue(item, out int chosen) ? chosen : 1, max,
                                   shift => Buy(item, shift ? 10 : row.Stepper.Amount));
        row.Name.text = $"{definition.DisplayName}{have}{why}";
        row.Price.text = Money.Format(price);
        row.Price.color = max > 0 ? UiKit.Cream : UiKit.Muted;
        row.SetActionEnabled(max > 0);

        void ShowTotal() => row.Total.text = max > 0 ? $"{row.Stepper.Amount} -> {Money.Format(Money.Mul(price, row.Stepper.Amount))}" : "";
        row.Stepper.Changed += () =>
        {
            buyAmounts[item] = row.Stepper.Amount;
            ShowTotal();
        };
        ShowTotal();
    }

    // One carried item the store takes: the most that can be sold is what is carried, and the total is the market's quote for the amount.
    void SellRow(InventoryManager inventory, MarketManager market, ItemDefinition item, int count)
    {
        bool equipped = inventory.EquippedTool == item;
        string note = equipped ? "  <size=15><color=#C7D68C>equipped</color></size>" : "";
        int percent = MarketRules.PercentOfNormal(market.Multiplier(item.Id));
        string normal = percent != 100 ? $"  <size=16><color=#EDE3C799>· {percent}% of normal</color></size>" : "";

        string trend;
        switch (market.Trend(item.Id))
        {
            case MarketTrend.Up: trend = $"<color={UpColor}>+</color>"; break;
            case MarketTrend.Down: trend = $"<color={DownColor}>-</color>"; break;
            default: trend = "<color=#EDE3C766>=</color>"; break;
        }

        QuantityRowUi row = null;
        row = QuantityRowUi.Create(sellList, "Sell", true, sellAmounts.TryGetValue(item.Id, out int chosen) ? chosen : 1, count,
                                   shift => Sell(item, shift ? inventory.Player.Count(item.Id) : row.Stepper.Amount));
        row.Name.text = $"{item.DisplayName}  <size=16><color=#EDE3C799>×{count}</color></size>{note}{normal}";
        row.Trend.text = trend;
        row.Price.text = Money.Format(market.SellPrice(item.Id));
        row.SetActionEnabled(count > 0);

        void ShowTotal() => row.Total.text = row.Stepper.Amount > 0 ? $"{row.Stepper.Amount} -> {Money.Format(market.Quote(item.Id, row.Stepper.Amount).totalCents)}" : "";
        row.Stepper.Changed += () =>
        {
            sellAmounts[item.Id] = row.Stepper.Amount;
            ShowTotal();
        };
        ShowTotal();
    }

    void Buy(string itemId, int count)
    {
        TradingPost post = TradingPost.Instance;
        if (post == null)
            return;
        buyAmounts.Remove(itemId); // back to 1 once the move has happened
        int bought = post.Buy(itemId, count, out string problem);
        string name = ItemDatabase.Get(itemId)?.DisplayName ?? itemId;
        statusLabel.text = bought > 0 ? $"Bought {(bought > 1 ? bought + " × " : "")}{name}" + (problem != null ? $" — {problem}" : "") : problem;
        if (AudioManager.Instance != null && bought == 0)
            AudioManager.Instance.Play(SoundCue.UiBack);
        Refresh();
    }

    void Sell(ItemDefinition item, int count)
    {
        TradingPost post = TradingPost.Instance;
        InventoryManager inventory = InventoryManager.Instance;
        if (post == null || inventory == null)
            return;
        sellAmounts.Remove(item.Id); // back to 1 once the move has happened
        SaleResult sale = post.Sell(item.Id, count);
        if (sale.count > 0)
        {
            statusLabel.text = $"Sold {(sale.count > 1 ? sale.count + " × " : "")}{item.DisplayName} for {Money.Format(sale.totalCents)}";
            if (AudioManager.Instance != null)
                AudioManager.Instance.Play(SoundCue.UiClick);
        }
        Refresh();
    }

    // Places an element a fixed distance from its parent's top, across part of its width.
    static void Top(RectTransform rt, float top, float height, float xMin, float xMax)
    {
        rt.anchorMin = new Vector2(xMin, 1f);
        rt.anchorMax = new Vector2(xMax, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.offsetMin = new Vector2(0f, -top - height);
        rt.offsetMax = new Vector2(0f, -top);
    }
}
