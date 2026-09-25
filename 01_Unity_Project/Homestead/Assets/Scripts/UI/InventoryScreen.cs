using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Inventory_System.md's Inventory Screen (confirmed 2026-09-25): what the player is carrying, their weight against the
// Encumbered and maximum carry thresholds, and which Tool is equipped. Clicking a carried tool equips it, clicking the
// equipped tool again unequips it; clicking collected water drinks a litre of it. A Build section builds a campfire
// from carried Firewood (Fire System). Home Storage transfer is out of scope until a storage structure exists.
public class InventoryScreen : GameScreen
{
    const float KgToLb = 2.20462f;

    static readonly Color BarNormal = new Color(0.55f, 0.66f, 0.36f);
    static readonly Color BarEncumbered = new Color(0.84f, 0.63f, 0.3f);

    RectTransform list;
    Text weightLabel;
    Text statusLabel;
    Text equippedLabel;
    Button buildButton;
    Text buildLabel;
    RectTransform barFill;
    Image barFillImage;
    RectTransform encumberedTick;
    Text encumberedTickLabel;
    InventoryManager watched;

    public override string Title => "Inventory";

    public override void Build(RectTransform area)
    {
        // Item list on the left.
        RectTransform left = UiKit.Rect("Items", area).Region(0f, 0f, 0.6f, 1f);
        RectTransform header = UiKit.Rect("Header", left);
        header.anchorMin = new Vector2(0f, 1f);
        header.anchorMax = new Vector2(1f, 1f);
        header.offsetMin = new Vector2(0f, -36f);
        header.offsetMax = Vector2.zero;
        Column(header, "Item", 0f, 0.62f, TextAnchor.MiddleLeft, UiKit.Muted, 19, 18f);
        Column(header, "Qty", 0.62f, 0.78f, TextAnchor.MiddleRight, UiKit.Muted, 19, 0f);
        Column(header, "Weight", 0.78f, 1f, TextAnchor.MiddleRight, UiKit.Muted, 19, 0f, 18f);

        list = UiKit.ScrollList(left, "List");
        ((RectTransform)list.parent).Fill(0f, 0f, 0f, 40f);

        // Carrying summary on the right.
        RectTransform right = UiKit.Rect("Carrying", area).Region(0.63f, 0f, 1f, 1f);

        Heading(right, "Carrying", 0f);
        weightLabel = UiKit.Text(right, "Weight", "", 34, UiKit.Cream);
        Top(weightLabel.rectTransform, 44f, 48f);

        RectTransform bar = UiKit.Image(right, "Bar", new Color(0f, 0f, 0f, 0.5f)).rectTransform;
        Top(bar, 104f, 20f);
        barFillImage = UiKit.Image(bar, "Fill", BarNormal);
        barFill = barFillImage.rectTransform;
        barFill.anchorMin = Vector2.zero;
        barFill.anchorMax = new Vector2(0f, 1f);
        barFill.offsetMin = barFill.offsetMax = Vector2.zero;

        encumberedTick = UiKit.Image(bar, "Encumbered Tick", UiKit.Cream).rectTransform;
        encumberedTick.anchorMin = encumberedTick.anchorMax = new Vector2(0f, 0.5f);
        encumberedTick.sizeDelta = new Vector2(3f, 32f);
        encumberedTickLabel = UiKit.Text(encumberedTick, "Label", "", 16, UiKit.Muted, TextAnchor.UpperCenter);
        encumberedTickLabel.horizontalOverflow = HorizontalWrapMode.Overflow;
        encumberedTickLabel.rectTransform.anchorMin = encumberedTickLabel.rectTransform.anchorMax = new Vector2(0.5f, 0f);
        encumberedTickLabel.rectTransform.pivot = new Vector2(0.5f, 1f);
        encumberedTickLabel.rectTransform.sizeDelta = new Vector2(160f, 22f);
        encumberedTickLabel.rectTransform.anchoredPosition = new Vector2(0f, -2f);

        statusLabel = UiKit.Text(right, "Status", "", 21, UiKit.Cream);
        Top(statusLabel.rectTransform, 160f, 60f);

        Heading(right, "Equipped Tool", 250f);
        equippedLabel = UiKit.Text(right, "Equipped", "", 26, UiKit.Cream);
        Top(equippedLabel.rectTransform, 294f, 40f);

        Text hint = UiKit.Text(right, "Hint", "Click a tool to equip it, click it again to put it away. Click water to drink a litre.", 19, UiKit.Muted);
        Top(hint.rectTransform, 340f, 56f);

        // Building (Fire System).
        Heading(right, "Build", 420f);
        buildButton = UiKit.Button(right, "Build Campfire", "", 21, BuildCampfire, TextAnchor.MiddleLeft);
        Top((RectTransform)buildButton.transform, 464f, 50f);
        buildLabel = UiKit.Text(right, "Build Status", "", 19, UiKit.Muted);
        Top(buildLabel.rectTransform, 520f, 56f);
    }

    public override void OnShow()
    {
        watched = InventoryManager.Instance;
        if (watched != null)
        {
            watched.Player.Changed += Refresh;
            watched.EquippedToolChanged += OnEquippedChanged;
        }
        Refresh();
    }

    public override void OnHide() => Unsubscribe();

    void OnDestroy() => Unsubscribe();

    void Unsubscribe()
    {
        if (watched == null)
            return;

        watched.Player.Changed -= Refresh;
        watched.EquippedToolChanged -= OnEquippedChanged;
        watched = null;
    }

    void OnEquippedChanged(ItemDefinition tool) => Refresh();

    void Refresh()
    {
        UiKit.Clear(list);
        InventoryManager inventory = InventoryManager.Instance;
        if (inventory == null)
        {
            weightLabel.text = statusLabel.text = equippedLabel.text = "";
            return;
        }

        // Carried items, grouped by category then name.
        var stacks = new List<(ItemDefinition item, int quantity)>();
        var totals = new Dictionary<string, int>();
        foreach (ItemStack stack in inventory.Player.Stacks)
            totals[stack.itemId] = (totals.TryGetValue(stack.itemId, out int n) ? n : 0) + stack.quantity;
        foreach (KeyValuePair<string, int> pair in totals)
        {
            ItemDefinition item = ItemDatabase.Get(pair.Key);
            if (item != null)
                stacks.Add((item, pair.Value));
        }
        stacks.Sort((a, b) => a.item.Category != b.item.Category
            ? a.item.Category.CompareTo(b.item.Category)
            : string.CompareOrdinal(a.item.DisplayName, b.item.DisplayName));

        if (stacks.Count == 0)
            UiKit.Height(UiKit.Text(list, "Empty", "You're not carrying anything.", 21, UiKit.Muted), 50f);

        ItemDefinition equipped = inventory.EquippedTool;
        foreach ((ItemDefinition item, int quantity) in stacks)
            AddRow(item, quantity, item == equipped);

        // Weight against the thresholds.
        float carried = inventory.CarriedWeightKg, max = inventory.MaxCarryWeightKg, limit = inventory.EncumberedWeightKg;
        weightLabel.text = $"{carried:0.0} kg <size=24><color=#EDE3C799>/ {max:0} kg  ({carried * KgToLb:0} / {max * KgToLb:0} lb)</color></size>";
        float fill = max > 0f ? Mathf.Clamp01(carried / max) : 0f;
        barFill.anchorMax = new Vector2(fill, 1f);
        barFillImage.color = inventory.IsEncumbered ? (fill >= 0.999f ? UiKit.Warning : BarEncumbered) : BarNormal;
        float tick = max > 0f ? limit / max : 0f;
        encumberedTick.anchorMin = encumberedTick.anchorMax = new Vector2(tick, 0.5f);
        encumberedTickLabel.text = $"Encumbered {limit:0} kg";

        if (carried >= max - 0.001f)
            statusLabel.text = "<color=#E67350>At your carrying limit.</color> Nothing more can be picked up.";
        else if (inventory.IsEncumbered)
            statusLabel.text = $"<color=#D6A04D>Encumbered</color> — moving slower and can't sprint at the limit. {max - carried:0.0} kg of room left.";
        else
            statusLabel.text = $"Unencumbered. {limit - carried:0.0} kg before you slow down.";

        equippedLabel.text = equipped != null ? equipped.DisplayName : "<color=#EDE3C799>None</color>";
        RefreshBuild();
    }

    void RefreshBuild()
    {
        FireManager fires = FireManager.Instance;
        buildButton.gameObject.SetActive(fires != null);
        if (fires == null)
        {
            buildLabel.text = "";
            return;
        }

        bool can = fires.CanBuild(FindAnyObjectByType<PlayerController>(), out _, out string reason);
        buildButton.interactable = can;
        buildButton.GetComponentInChildren<Text>().text = $"Campfire  <size=17><color=#EDE3C799>{fires.FirewoodToBuild} Firewood</color></size>";
        buildLabel.text = can ? "Builds just in front of you. Light it with Flint and Steel." : reason;
    }

    void BuildCampfire()
    {
        FireManager fires = FireManager.Instance;
        if (fires == null || !fires.Build(FindAnyObjectByType<PlayerController>()))
            return;

        // (Using up the Firewood already plays the item-drop sound.)
        // Back to the world, so the player sees what they built.
        GameScreens screens = GetComponentInParent<GameScreens>();
        if (screens != null)
            screens.Close();
    }

    void AddRow(ItemDefinition item, int quantity, bool equipped)
    {
        bool isTool = item.Category == ItemCategory.Tool;
        bool isWater = WaterQualities.IsRawWater(item.Id);
        UnityEngine.Events.UnityAction click = isTool ? () => ToggleEquip(item)
                                             : isWater ? () => DrinkWater(item)
                                             : (UnityEngine.Events.UnityAction)null;
        Button row = UiKit.Button(list, item.Id, "", 20, click);
        UiKit.Height(row, 46f);
        row.interactable = click != null;
        ColorBlock colors = row.colors;
        colors.disabledColor = UiKit.RaisedColor; // plain items aren't clickable but shouldn't look greyed out
        row.colors = colors;
        UiKit.SetSelected(row, equipped);

        var rt = (RectTransform)row.transform;
        Destroy(row.GetComponentInChildren<Text>().gameObject);
        string category = CategoryName(item.Category);
        string tag = equipped ? "  <color=#C7D68C>• Equipped</color>" : "";
        Column(rt, $"{item.DisplayName}  <size=16><color=#EDE3C799>{category}</color></size>{tag}", 0f, 0.62f,
               TextAnchor.MiddleLeft, UiKit.Cream, 21, 12f);
        Column(rt, $"×{quantity}", 0.62f, 0.78f, TextAnchor.MiddleRight, UiKit.Cream, 21, 0f);
        Column(rt, $"{item.WeightKg * quantity:0.0} kg", 0.78f, 1f, TextAnchor.MiddleRight, UiKit.Cream, 21, 0f, 12f);
    }

    static void ToggleEquip(ItemDefinition tool)
    {
        InventoryManager inventory = InventoryManager.Instance;
        if (inventory == null)
            return;

        if (inventory.EquippedTool == tool)
            inventory.Unequip();
        else
            inventory.Equip(tool.Id);

        if (AudioManager.Instance != null)
            AudioManager.Instance.Play(SoundCue.UiClick);
    }

    // Drinks one litre of collected water. Illness risk from lower qualities comes with Water Purification.
    static void DrinkWater(ItemDefinition water)
    {
        SurvivalManager survival = SurvivalManager.Instance;
        InventoryManager inventory = InventoryManager.Instance;
        if (survival == null || inventory == null || survival.Hydration >= SurvivalManager.MaxValue)
            return; // not thirsty — don't waste it

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayOnConsume(SoundCue.Drink); // the drink sound replaces the item-drop sound
        if (inventory.RemoveFromPlayer(water.Id, 1) == 1)
            survival.Drink(WaterQualities.HydrationPerLitre);
    }

    static string CategoryName(ItemCategory category)
    {
        switch (category)
        {
            case ItemCategory.Resource: return "Resource";
            case ItemCategory.Tool: return "Tool";
            case ItemCategory.Consumable: return "Consumable";
            default: return "Material";
        }
    }

    static Text Column(RectTransform parent, string text, float xMin, float xMax, TextAnchor anchor, Color color, int size,
                       float padLeft, float padRight = 0f)
    {
        Text label = UiKit.Text(parent, "Column", text, size, color, anchor);
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        label.rectTransform.anchorMin = new Vector2(xMin, 0f);
        label.rectTransform.anchorMax = new Vector2(xMax, 1f);
        label.rectTransform.offsetMin = new Vector2(padLeft, 0f);
        label.rectTransform.offsetMax = new Vector2(-padRight, 0f);
        return label;
    }

    static void Heading(RectTransform parent, string text, float top)
    {
        Text label = UiKit.Text(parent, text, text, 26, UiKit.Accent, TextAnchor.MiddleLeft, FontStyle.Italic);
        Top(label.rectTransform, top, 36f);
    }

    // Places a full-width element a fixed distance from the parent's top.
    static void Top(RectTransform rt, float top, float height)
    {
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.offsetMin = new Vector2(0f, -top - height);
        rt.offsetMax = new Vector2(0f, -top);
    }
}
