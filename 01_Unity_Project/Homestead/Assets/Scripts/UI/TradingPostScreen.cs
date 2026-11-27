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
//
// Delivery (slice D1, DeliveryRules): the button at the top of the Buy column switches between buying to carry and buying for delivery. For
// delivery, a store row's button adds to the order list instead (no weight limit), and the right-hand column shows the order: its lines, the
// goods, the fee and the weight, when it will arrive, and Place Order and Clear. Selling waits until the switch is back on Carry.
public class TradingPostScreen : GameScreen
{
    RectTransform buyList, sellList;
    Text moneyLabel, statusLabel, marketLabel;
    Text buySub, sellHeading, sellSub;
    Button modeButton;
    bool deliverMode;
    InventoryManager watchedInventory;
    TradingPost watchedPost;
    MarketManager watchedMarket;

    // The amount chosen in each row's stepper, by item id. The rows are rebuilt on every refresh, so the amounts live here; they go
    // back to 1 after a move and are cleared when the screen hides.
    readonly Dictionary<string, int> buyAmounts = new Dictionary<string, int>();
    readonly Dictionary<string, int> sellAmounts = new Dictionary<string, int>();

    const string UpColor = "#2E6121", DownColor = "#AD2E14";

    public override string Title => "Trading Post";

    public override void Build(RectTransform area)
    {
        moneyLabel = UiKit.Text(area, "Money", "", 30, UiKit.Ink, TextAnchor.MiddleLeft);
        Top(moneyLabel.rectTransform, 0f, 40f, 0f, 1f);
        statusLabel = UiKit.Text(area, "Status", "", 18, UiKit.InkMuted, TextAnchor.MiddleRight);
        Top(statusLabel.rectTransform, 0f, 40f, 0.4f, 1f);

        marketLabel = UiKit.Text(area, "Market", "", 16, UiKit.InkMuted, TextAnchor.MiddleLeft);
        Top(marketLabel.rectTransform, 40f, 24f, 0f, 1f);

        buyList = Column(area, "Buy", 0f, 0.485f, out _, out buySub);
        sellList = Column(area, "Sell", 0.515f, 1f, out sellHeading, out sellSub);

        // Carry or Deliver, above the store's list.
        modeButton = UiKit.Button(buySub.transform.parent, "Mode", DeliveryRules.CarryMode, 18, () =>
        {
            deliverMode = !deliverMode;
            UiKit.ReleaseFocus();
            statusLabel.text = "";
            Refresh();
        });
        var modeRt = (RectTransform)modeButton.transform;
        modeRt.anchorMin = new Vector2(0.18f, 1f);
        modeRt.anchorMax = new Vector2(1f, 1f);
        modeRt.pivot = new Vector2(0.5f, 1f);
        modeRt.offsetMin = new Vector2(0f, -36f);
        modeRt.offsetMax = new Vector2(0f, 0f);
    }

    RectTransform Column(RectTransform area, string heading, float xMin, float xMax, out Text headingText, out Text subText)
    {
        RectTransform column = UiKit.Rect(heading, area).Region(xMin, 0f, xMax, 1f);
        column.offsetMax = new Vector2(0f, -78f);
        Text label = UiKit.Heading(column, "Heading", heading == "Buy" ? "Buy" : "Sell", 26, TextAnchor.MiddleLeft);
        Top(label.rectTransform, 0f, 36f, 0f, 1f);
        Text sub = UiKit.Text(column, "Sub", heading == "Buy" ? "What the store has    (Shift-click: 10)" : "What you carry that it'll take    (Shift-click: the lot)",
                              16, UiKit.InkMuted, TextAnchor.MiddleRight);
        Top(sub.rectTransform, 0f, 36f, 0.3f, 1f);
        RectTransform list = UiKit.ScrollList(column, "List");
        ((RectTransform)list.parent).Fill(0f, 0f, 0f, 42f);
        headingText = label;
        subText = sub;
        return list;
    }

    public override void OnShow()
    {
        watchedInventory = InventoryManager.Instance;
        watchedPost = TradingPost.Instance;
        if (watchedInventory != null)
            watchedInventory.Player.Changed += Refresh;
        if (watchedPost != null)
        {
            watchedPost.MoneyChanged += Refresh;
            watchedPost.OrderChanged += Refresh;
        }
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
        {
            watchedPost.MoneyChanged -= Refresh;
            watchedPost.OrderChanged -= Refresh;
        }
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
            ? $"{Money.Format(post.Cents)}   <size=20><color=#6B5538>{DifficultyManager.Name(DifficultyManager.Tier)}</color></size>"
            : "";

        MarketManager market = MarketManager.Instance;
        marketLabel.text = market != null ? market.Note : "";

        UiKit.Clear(buyList);
        UiKit.Clear(sellList);
        if (post == null || inventory == null || market == null)
            return;

        modeButton.GetComponentInChildren<Text>().text = deliverMode ? DeliveryRules.DeliverMode : DeliveryRules.CarryMode;
        buySub.text = "";
        foreach ((string item, long price) in TradingPost.Catalog)
        {
            ItemDefinition definition = ItemDatabase.Get(item);
            if (definition == null)
                continue;
            if (deliverMode)
                OrderRow(post, item, definition, price);
            else
                BuyRow(post, inventory, item, definition, price);
        }

        sellHeading.text = deliverMode ? "Delivery order" : "Sell";
        sellSub.text = deliverMode ? "Paid all at once; it comes to the Delivery Pad" : "What you carry that it'll take    (Shift-click: the lot)";
        if (deliverMode)
        {
            OrderPanel(post);
            return;
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
            UiKit.Height(UiKit.Text(sellList, "Empty", "Nothing you're carrying that the store buys. Hides, furs, meat, fish, forage, firewood, logs and stone all sell.", 18, UiKit.InkMuted), 60f);
        foreach ((ItemDefinition item, int count, long baseCents) in carried)
            SellRow(inventory, market, item, count);
    }

    // One store item: the most that can be bought is what the cash and the carry weight allow.
    void BuyRow(TradingPost post, InventoryManager inventory, string item, ItemDefinition definition, long price)
    {
        int max = Money.BuyCount(post.Cents, price, QuantityRules.MaxAmount, inventory.Player.SpaceFor(definition));
        int carriedCount = inventory.Player.Count(item);
        string have = carriedCount > 0 ? $"  <size=18><color=#2E6121>have {carriedCount}</color></size>" : "";
        string why = max > 0 ? "" : post.Cents < price ? "  <size=18><color=#AD2E14>can't afford</color></size>" : "  <size=18><color=#AD2E14>too heavy to carry more</color></size>";

        QuantityRowUi row = null;
        row = QuantityRowUi.Create(buyList, "Buy", true, buyAmounts.TryGetValue(item, out int chosen) ? chosen : 1, max,
                                   shift => Buy(item, shift ? 10 : row.Stepper.Amount));
        row.Name.text = $"{definition.DisplayName}{have}{why}";
        row.SetIcon(item, definition.DisplayName);
        row.Price.text = Money.Format(price);
        row.Price.color = max > 0 ? UiKit.Ink : UiKit.InkMuted;
        row.SetActionEnabled(max > 0);

        void ShowTotal() => row.Total.text = max > 0 ? $"{row.Stepper.Amount} -> {Money.Format(Money.Mul(price, row.Stepper.Amount))}" : "";
        row.Stepper.Changed += () =>
        {
            buyAmounts[item] = row.Stepper.Amount;
            ShowTotal();
        };
        ShowTotal();
    }

    // One store item, for delivery: no weight limit, and the money is checked when the order is placed.
    void OrderRow(TradingPost post, string item, ItemDefinition definition, long price)
    {
        int ordered = 0;
        foreach (DeliveryLine line in post.Order)
            if (line.itemId == item)
                ordered = line.count;
        string have = ordered > 0 ? $"  <size=18><color=#2E6121>{ordered} on order</color></size>" : "";
        QuantityRowUi row = null;
        row = QuantityRowUi.Create(buyList, "Add", true, buyAmounts.TryGetValue(item, out int chosen) ? chosen : 1, QuantityRules.MaxAmount,
                                   shift => AddToOrder(item, shift ? 10 : row.Stepper.Amount));
        row.Name.text = $"{definition.DisplayName}{have}";
        row.SetIcon(item, definition.DisplayName);
        row.Price.text = Money.Format(price);
        void ShowTotal() => row.Total.text = $"{row.Stepper.Amount} -> {Money.Format(Money.Mul(price, row.Stepper.Amount))}";
        row.Stepper.Changed += () =>
        {
            buyAmounts[item] = row.Stepper.Amount;
            ShowTotal();
        };
        ShowTotal();
    }

    void AddToOrder(string itemId, int count)
    {
        TradingPost post = TradingPost.Instance;
        if (post == null)
            return;
        buyAmounts.Remove(itemId);
        int added = post.AddToOrder(itemId, count);
        string name = ItemDatabase.Get(itemId)?.DisplayName ?? itemId;
        statusLabel.text = added > 0 ? $"Added {(added > 1 ? added + " x " : "")}{name} to the order" : "Not for sale.";
        if (AudioManager.Instance != null)
            AudioManager.Instance.Play(added > 0 ? SoundCue.UiClick : SoundCue.UiBack);
    }

    // The order: one line per item with a Remove button, then the totals, when it comes, and Place Order and Clear.
    void OrderPanel(TradingPost post)
    {
        BarnRows(post);
        bool pad = WoodManager.Instance != null && WoodManager.Instance.DeliveryPad != null;
        if (!pad)
            UiKit.Height(UiKit.Text(sellList, "NoPad", DeliveryRules.NoPad, 20, new Color(0.68f, 0.18f, 0.08f)), 56f);
        if (post.Order.Count == 0)
            UiKit.Height(UiKit.Text(sellList, "Empty", "Nothing on the order yet. Add things from the store on the left.", 20, UiKit.InkMuted), 56f);
        foreach (DeliveryLine line in post.Order)
        {
            ItemDefinition definition = ItemDatabase.Get(line.itemId);
            string name = definition != null ? definition.DisplayName : line.itemId;
            RectTransform r = UiKit.Image(sellList, "Line", UiKit.RaisedColor).rectTransform;
            UiKit.Height(r, 52f);
            Text label = UiKit.Text(r, "Label", $"{line.count} x {name}   {Money.Format(Money.Mul(TradingPost.BuyPrice(line.itemId), line.count))}", 20, UiKit.Ink, TextAnchor.MiddleLeft);
            label.rectTransform.Fill(12f, 0f, 130f, 0f);
            string id = line.itemId;
            Button remove = UiKit.Button(r, "Remove", "Remove", 18, () =>
            {
                UiKit.ReleaseFocus();
                post.RemoveFromOrder(id);
            });
            var removeRt = (RectTransform)remove.transform;
            removeRt.anchorMin = removeRt.anchorMax = removeRt.pivot = new Vector2(1f, 0.5f);
            removeRt.sizeDelta = new Vector2(112f, 40f);
            removeRt.anchoredPosition = new Vector2(-8f, 0f);
        }
        if (post.Order.Count > 0)
        {
            UiKit.Height(UiKit.Text(sellList, "Summary", DeliveryRules.Summary(post.OrderGoodsCents, post.OrderKg), 20, UiKit.Ink), 64f);
            RectTransform buttons = UiKit.Rect("Buttons", sellList);
            UiKit.Height(buttons, 48f);
            Button place = UiKit.Button(buttons, "Place", "Place Order", 20, () =>
            {
                UiKit.ReleaseFocus();
                bool ok = post.PlaceOrder(out string message);
                statusLabel.text = message;
                if (AudioManager.Instance != null)
                    AudioManager.Instance.Play(ok ? SoundCue.UiClick : SoundCue.UiBack);
                Refresh();
            });
            ((RectTransform)place.transform).Region(0f, 0f, 0.6f, 1f);
            QuantityStepper.SetEnabled(place, pad);
            Button clear = UiKit.Button(buttons, "Clear", "Clear", 20, () =>
            {
                UiKit.ReleaseFocus();
                post.ClearOrder();
            });
            ((RectTransform)clear.transform).Region(0.64f, 0f, 1f, 1f);
        }
        int days = post.NextDeliveryInDays();
        if (days >= 0)
            UiKit.Height(UiKit.Text(sellList, "Pending", DeliveryRules.PendingLine(post.Deliveries.Count, days), 18, UiKit.InkMuted), 40f);
    }

    // The Pole Barn sites marked out (B1a): for each, order its kit or hire the crew, or how it stands.
    void BarnRows(TradingPost post)
    {
        if (WoodManager.Instance == null)
            return;
        foreach (WoodPileState site in WoodManager.Instance.BarnSites())
        {
            BarnRules.Size size = WoodManager.BarnSizeOf(site.kind);
            UiKit.Height(UiKit.Heading(sellList, "Barn", BarnRules.SiteName(size), 22, TextAnchor.MiddleLeft), 34f);
            CrewJob crew = post.CrewFor(site.id);
            string status = null;
            if (crew != null)
            {
                int toStart = crew.bookedDay + BarnRules.LeadDays - (TimeManager.Instance != null ? TimeManager.Instance.TotalDays : 0);
                status = crew.balancePaid ? "The crew is finishing up" : BarnRules.CrewWaiting(size, toStart);
            }
            else if (post.KitOrdered(site.id))
                status = "The kit is on its way to the site";
            else if (!site.IsEmpty || site.jobCount > 0)
                status = $"The kit is on the site - stage {Mathf.Clamp(site.jobCount, 0, 2) + 1} of 3: {BarnRules.StageWork(site.jobCount)} (Hammer)";
            if (status != null)
            {
                UiKit.Height(UiKit.Text(sellList, "BarnStatus", status, 20, UiKit.Ink), 34f);
                continue;
            }
            BarnChoice(BarnRules.KitRow(size), "Order", () => post.OrderBarnKit(site, out string m) ? m : m);
            BarnChoice(BarnRules.CrewRow(size), "Hire", () => post.HireBarnCrew(site, out string m) ? m : m);
        }
    }

    void BarnChoice(string text, string action, System.Func<string> act)
    {
        RectTransform r = UiKit.Image(sellList, "BarnChoice", UiKit.RaisedColor).rectTransform;
        UiKit.Height(r, 72f);
        Text label = UiKit.Text(r, "Label", text, 18, UiKit.Ink, TextAnchor.MiddleLeft);
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.rectTransform.Fill(12f, 0f, 130f, 0f);
        Button button = UiKit.Button(r, action, action, 20, () =>
        {
            UiKit.ReleaseFocus();
            statusLabel.text = act();
            Refresh();
        });
        var rt = (RectTransform)button.transform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(1f, 0.5f);
        rt.sizeDelta = new Vector2(112f, 44f);
        rt.anchoredPosition = new Vector2(-8f, 0f);
    }

    // One carried item the store takes: the most that can be sold is what is carried, and the total is the market's quote for the amount.
    void SellRow(InventoryManager inventory, MarketManager market, ItemDefinition item, int count)
    {
        bool equipped = inventory.EquippedTool == item;
        string note = equipped ? "  <size=17><color=#2E6121>equipped</color></size>" : "";
        int percent = MarketRules.PercentOfNormal(market.Multiplier(item.Id));
        string normal = percent != 100 ? $"  <size=18><color=#6B5538>· {percent}% of normal</color></size>" : "";

        string trend;
        switch (market.Trend(item.Id))
        {
            case MarketTrend.Up: trend = $"<color={UpColor}>+</color>"; break;
            case MarketTrend.Down: trend = $"<color={DownColor}>-</color>"; break;
            default: trend = "<color=#8C7759>=</color>"; break;
        }

        QuantityRowUi row = null;
        row = QuantityRowUi.Create(sellList, "Sell", true, sellAmounts.TryGetValue(item.Id, out int chosen) ? chosen : 1, count,
                                   shift => Sell(item, shift ? inventory.Player.Count(item.Id) : row.Stepper.Amount));
        row.Name.text = $"{item.DisplayName}  <size=18><color=#6B5538>×{count}</color></size>{note}{normal}";
        row.SetIcon(item.Id, item.DisplayName);
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
