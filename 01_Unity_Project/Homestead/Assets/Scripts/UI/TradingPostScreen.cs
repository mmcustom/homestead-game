using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Difficulty_System.md's Trading Post as a game screen (T): menu-only, no place or trader in the world. The store's
// stock on the left with Buy buttons, everything carried that it'll take on the right with Sell buttons, and the money
// and difficulty across the top, and a one-line market note under them. Shift-click buys ten (arrows, rounds, cordage) or sells the whole
// stack, unit by unit at the falling price. Each Sell row shows the price now, a trend marker (green +: below its seasonal target and
// recovering; red -: above it and falling; =: steady), how far from normal it is, and what the whole stack would fetch. The Sell list is
// ordered by base price, never by current price, so rows don't jump under the cursor as prices move. Prices and the buying and selling
// themselves live in TradingPost and MarketManager.
public class TradingPostScreen : GameScreen
{
    RectTransform buyList, sellList;
    Text moneyLabel, statusLabel, marketLabel;
    InventoryManager watchedInventory;
    TradingPost watchedPost;
    MarketManager watchedMarket;

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
        Text sub = UiKit.Text(column, "Sub", heading == "Buy" ? "What the store has    (Shift: 10 at a time)" : "What you carry that it'll take    (Shift: the lot)",
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
        Refresh();
    }

    public override void OnHide() => Unsubscribe();

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
            bool afford = post.Cents >= price;
            string have = inventory.Player.Count(item) > 0 ? $"  <size=15><color=#C7D68C>have {inventory.Player.Count(item)}</color></size>" : "";
            Row(buyList, $"{definition.DisplayName}{have}", Money.Format(price), "Buy", afford, () => Buy(item));
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
        {
            bool equipped = inventory.EquippedTool == item;
            string note = equipped ? "  <size=15><color=#C7D68C>equipped</color></size>" : "";

            var detail = new List<string>();
            int percent = MarketRules.PercentOfNormal(market.Multiplier(item.Id));
            if (percent != 100)
                detail.Add($"{percent}% of normal");
            if (count > 1)
                detail.Add($"Shift: all {count} for about {Money.Format(market.Quote(item.Id, count).totalCents)}");
            string second = detail.Count > 0 ? $"\n<size=13><color=#EDE3C799>{string.Join("  ·  ", detail)}</color></size>" : "";

            string trend;
            switch (market.Trend(item.Id))
            {
                case MarketTrend.Up: trend = $"<color={UpColor}>+</color>"; break;
                case MarketTrend.Down: trend = $"<color={DownColor}>-</color>"; break;
                default: trend = "<color=#EDE3C766>=</color>"; break;
            }

            Row(sellList, $"{item.DisplayName}  <size=16><color=#EDE3C799>×{count}</color></size>{note}{second}",
                Money.Format(market.SellPrice(item.Id)), "Sell", true, () => Sell(item), trend, 52f);
        }
    }

    // trend: a marker between the name and the price (Sell rows), or null for none (Buy rows).
    void Row(RectTransform list, string label, string price, string action, bool enabled, UnityEngine.Events.UnityAction click,
             string trend = null, float height = 44f)
    {
        RectTransform row = UiKit.Image(list, "Row", UiKit.RaisedColor).rectTransform;
        UiKit.Height(row, height);
        Text name = UiKit.Text(row, "Name", label, 20, UiKit.Cream, TextAnchor.MiddleLeft);
        name.rectTransform.anchorMin = Vector2.zero;
        name.rectTransform.anchorMax = new Vector2(trend != null ? 0.56f : 0.62f, 1f);
        name.rectTransform.offsetMin = new Vector2(12f, 0f);
        name.rectTransform.offsetMax = Vector2.zero;
        name.horizontalOverflow = HorizontalWrapMode.Overflow;
        if (trend != null)
        {
            Text marker = UiKit.Text(row, "Trend", trend, 26, UiKit.Cream, TextAnchor.MiddleCenter, FontStyle.Bold);
            marker.rectTransform.anchorMin = new Vector2(0.56f, 0f);
            marker.rectTransform.anchorMax = new Vector2(0.62f, 1f);
            marker.rectTransform.offsetMin = marker.rectTransform.offsetMax = Vector2.zero;
        }
        Text cost = UiKit.Text(row, "Price", price, 20, enabled ? UiKit.Cream : UiKit.Muted, TextAnchor.MiddleRight);
        cost.rectTransform.anchorMin = new Vector2(0.62f, 0f);
        cost.rectTransform.anchorMax = new Vector2(0.78f, 1f);
        cost.rectTransform.offsetMin = cost.rectTransform.offsetMax = Vector2.zero;
        Button button = UiKit.Button(row, action, action, 18, click);
        var rt = (RectTransform)button.transform;
        rt.anchorMin = new Vector2(0.8f, 0f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.offsetMin = new Vector2(0f, 6f);
        rt.offsetMax = new Vector2(-6f, -6f);
        button.interactable = enabled;
    }

    static bool ShiftHeld =>
        UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.shiftKey.isPressed;

    void Buy(string itemId)
    {
        TradingPost post = TradingPost.Instance;
        if (post == null)
            return;
        int bought = post.Buy(itemId, ShiftHeld ? 10 : 1, out string problem);
        string name = ItemDatabase.Get(itemId)?.DisplayName ?? itemId;
        statusLabel.text = bought > 0 ? $"Bought {(bought > 1 ? bought + " × " : "")}{name}" + (problem != null ? $" — {problem}" : "") : problem;
        if (AudioManager.Instance != null && bought == 0)
            AudioManager.Instance.Play(SoundCue.UiBack);
    }

    void Sell(ItemDefinition item)
    {
        TradingPost post = TradingPost.Instance;
        InventoryManager inventory = InventoryManager.Instance;
        if (post == null || inventory == null)
            return;
        int count = ShiftHeld ? inventory.Player.Count(item.Id) : 1;
        SaleResult sale = post.Sell(item.Id, count);
        if (sale.count > 0)
        {
            statusLabel.text = $"Sold {(sale.count > 1 ? sale.count + " × " : "")}{item.DisplayName} for {Money.Format(sale.totalCents)}";
            if (AudioManager.Instance != null)
                AudioManager.Instance.Play(SoundCue.UiClick);
        }
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
