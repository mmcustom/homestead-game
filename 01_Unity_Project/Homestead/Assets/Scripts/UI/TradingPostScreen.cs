using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Difficulty_System.md's Trading Post as a game screen (T): menu-only, no place or trader in the world. The store's
// stock on the left with Buy buttons, everything carried that it'll take on the right with Sell buttons, and the money
// and difficulty across the top. Shift-click buys ten (arrows, rounds, cordage) or sells the whole stack. Prices and
// the buying and selling themselves live in TradingPost.
public class TradingPostScreen : GameScreen
{
    RectTransform buyList, sellList;
    Text moneyLabel, statusLabel;
    InventoryManager watchedInventory;
    TradingPost watchedPost;

    public override string Title => "Trading Post";

    public override void Build(RectTransform area)
    {
        moneyLabel = UiKit.Text(area, "Money", "", 30, UiKit.Cream, TextAnchor.MiddleLeft);
        Top(moneyLabel.rectTransform, 0f, 40f, 0f, 1f);
        statusLabel = UiKit.Text(area, "Status", "", 18, UiKit.Muted, TextAnchor.MiddleRight);
        Top(statusLabel.rectTransform, 0f, 40f, 0.4f, 1f);

        buyList = Column(area, "Buy", 0f, 0.485f);
        sellList = Column(area, "Sell", 0.515f, 1f);
    }

    RectTransform Column(RectTransform area, string heading, float xMin, float xMax)
    {
        RectTransform column = UiKit.Rect(heading, area).Region(xMin, 0f, xMax, 1f);
        column.offsetMax = new Vector2(0f, -52f);
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
        watchedInventory = null;
        watchedPost = null;
    }

    void Refresh()
    {
        TradingPost post = TradingPost.Instance;
        InventoryManager inventory = InventoryManager.Instance;
        moneyLabel.text = post != null
            ? $"${post.Money}   <size=18><color=#EDE3C799>{DifficultyManager.Name(DifficultyManager.Tier)}</color></size>"
            : "";

        UiKit.Clear(buyList);
        UiKit.Clear(sellList);
        if (post == null || inventory == null)
            return;

        foreach ((string item, int price) in TradingPost.Catalog)
        {
            ItemDefinition definition = ItemDatabase.Get(item);
            if (definition == null)
                continue;
            bool afford = post.Money >= price;
            string have = inventory.Player.Count(item) > 0 ? $"  <size=15><color=#C7D68C>have {inventory.Player.Count(item)}</color></size>" : "";
            Row(buyList, $"{definition.DisplayName}{have}", $"${price}", "Buy", afford, () => Buy(item));
        }

        // Everything carried the store will take, one row per kind, highest value first.
        var carried = new List<(ItemDefinition item, int count, int price)>();
        var seen = new HashSet<string>();
        foreach (ItemStack stack in inventory.Player.Stacks)
        {
            if (!seen.Add(stack.itemId))
                continue;
            int price = TradingPost.SellPrice(stack.itemId);
            ItemDefinition definition = ItemDatabase.Get(stack.itemId);
            if (price > 0 && definition != null)
                carried.Add((definition, inventory.Player.Count(stack.itemId), price));
        }
        carried.Sort((a, b) => b.price.CompareTo(a.price));
        if (carried.Count == 0)
            UiKit.Height(UiKit.Text(sellList, "Empty", "Nothing you're carrying that the store buys. Hides, furs, meat, fish, forage, firewood, logs and stone all sell.", 18, UiKit.Muted), 60f);
        foreach ((ItemDefinition item, int count, int price) in carried)
        {
            bool equipped = inventory.EquippedTool == item;
            string note = equipped ? "  <size=15><color=#C7D68C>equipped</color></size>" : "";
            Row(sellList, $"{item.DisplayName}  <size=16><color=#EDE3C799>×{count}</color></size>{note}", $"${price}", "Sell", true, () => Sell(item));
        }
    }

    void Row(RectTransform list, string label, string price, string action, bool enabled, UnityEngine.Events.UnityAction click)
    {
        RectTransform row = UiKit.Image(list, "Row", UiKit.RaisedColor).rectTransform;
        UiKit.Height(row, 44f);
        Text name = UiKit.Text(row, "Name", label, 20, UiKit.Cream, TextAnchor.MiddleLeft);
        name.rectTransform.anchorMin = Vector2.zero;
        name.rectTransform.anchorMax = new Vector2(0.62f, 1f);
        name.rectTransform.offsetMin = new Vector2(12f, 0f);
        name.rectTransform.offsetMax = Vector2.zero;
        name.horizontalOverflow = HorizontalWrapMode.Overflow;
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
        int made = post.Sell(item.Id, count);
        if (made > 0)
        {
            statusLabel.text = $"Sold {(count > 1 ? count + " × " : "")}{item.DisplayName} for ${made}";
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
