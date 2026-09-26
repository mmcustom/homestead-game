using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Inventory_System.md's Inventory Screen (confirmed 2026-09-25): what the player is carrying, their weight against the
// Encumbered and maximum carry thresholds, and which Tool is equipped. Clicking a carried tool equips it, clicking the
// equipped tool again unequips it; clicking food or water eats or drinks one (Food). Standing by a burning campfire,
// raw meat and fish rows get a Cook button and raw water a Boil button (Cooking) — Shift-click does the whole stack.
// Each piece takes a few seconds (CampfireCooking); the button shows the progress, and clicking it again stops.
// Carrying the Axe, Logs and Branches get a Split button the same way, making Firewood (AxeTool).
// A Build section builds a campfire from carried Firewood (Fire System), a Wood Pile or Rock Pile for storage
// (WoodManager), and makes traps (Crafting's recipes). Home Storage transfer is out of scope until a storage structure
// exists; the piles are stored into and taken from in the World.
public class InventoryScreen : GameScreen
{
    const float KgToLb = 2.20462f;

    static readonly Color BarNormal = new Color(0.55f, 0.66f, 0.36f);
    static readonly Color BarEncumbered = new Color(0.84f, 0.63f, 0.3f);

    RectTransform list;
    Text weightLabel;
    Text statusLabel;
    Text equippedLabel;
    Text hintLabel;
    Button buildButton, woodPileButton, rockPileButton;
    Text buildLabel;
    readonly Button[] craftButtons = new Button[Crafting.Recipes.Length];
    RectTransform barFill;
    Image barFillImage;
    RectTransform encumberedTick;
    Text encumberedTickLabel;
    InventoryManager watched;
    readonly Dictionary<ItemDefinition, Text> cookLabels = new Dictionary<ItemDefinition, Text>();

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

        hintLabel = UiKit.Text(right, "Hint", "", 18, UiKit.Muted);
        Top(hintLabel.rectTransform, 336f, 80f);

        // Building (Fire System).
        Heading(right, "Build", 434f);
        // Campfire, Wood Pile and Rock Pile side by side.
        buildButton = UiKit.Button(right, "Build Campfire", "", 18, BuildCampfire);
        woodPileButton = UiKit.Button(right, "Build Wood Pile", "", 18, () => BuildPile(PileKind.WoodStorage));
        rockPileButton = UiKit.Button(right, "Build Rock Pile", "", 18, () => BuildPile(PileKind.RockStorage));
        Button[] row = { buildButton, woodPileButton, rockPileButton };
        for (int i = 0; i < row.Length; i++)
        {
            var rt = (RectTransform)row[i].transform;
            Top(rt, 478f, 50f);
            rt.anchorMin = new Vector2(i / 3f, 1f);
            rt.anchorMax = new Vector2((i + 1) / 3f, 1f);
            rt.offsetMin = new Vector2(i == 0 ? 0f : 3f, rt.offsetMin.y);
            rt.offsetMax = new Vector2(i == 2 ? 0f : -3f, rt.offsetMax.y);
            rt.GetComponentInChildren<Text>().rectTransform.Fill(4f, 0f, 4f, 0f);
        }
        buildLabel = UiKit.Text(right, "Build Status", "", 18, UiKit.Muted);
        Top(buildLabel.rectTransform, 530f, 46f);

        for (int i = 0; i < Crafting.Recipes.Length; i++)
        {
            Crafting.Recipe recipe = Crafting.Recipes[i];
            craftButtons[i] = UiKit.Button(right, "Craft " + recipe.outputId, "", 20, () => Craft(recipe), TextAnchor.MiddleLeft);
            Top((RectTransform)craftButtons[i].transform, 580f + i * 46f, 42f);
        }
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

    // The Cook / Boil buttons count down while their item is cooking.
    void Update()
    {
        CampfireCooking cooking = CampfireCooking.Instance;
        AxeTool axe = AxeTool.Instance;
        foreach (KeyValuePair<ItemDefinition, Text> pair in cookLabels)
        {
            if (pair.Value == null)
                continue;
            ItemDefinition item = pair.Key;
            bool split = AxeTool.IsSplittable(item.Id);
            string idle = split ? "Split" : Cooking.IsBoilable(item.Id) ? "Boil" : "Cook";
            bool queued = split ? axe != null && axe.IsSplitQueued(item) : cooking != null && cooking.IsQueued(item);
            if (!queued)
            {
                pair.Value.text = idle;
                continue;
            }
            float p = split ? axe.SplitProgressOf(item) : cooking.ProgressOf(item);
            int left = split ? axe.SplitsLeft(item) : cooking.RemainingOf(item);
            pair.Value.text = p >= 0f ? $"{p * 100f:0}%  ({left})" : $"Queued ({left})";
        }
    }

    void Refresh()
    {
        UiKit.Clear(list);
        cookLabels.Clear();
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
        PlayerController player = FindAnyObjectByType<PlayerController>();
        bool atFire = Cooking.FireInReach(player) != null;
        foreach ((ItemDefinition item, int quantity) in stacks)
            AddRow(item, quantity, item == equipped, atFire, player);

        hintLabel.text = "Click a tool to equip or put it away, food or water to eat or drink one. " +
                         (atFire ? "<color=#C7D68C>At the campfire: Cook meat and fish, Boil water (needs the Bucket). Shift: whole stack.</color>"
                                 : "Cook and boil at a burning campfire. With the Axe, Split Logs and Branches into Firewood.");

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

        PlayerController player = FindAnyObjectByType<PlayerController>();
        bool can = fires.CanBuild(player, out _, out string reason);
        buildButton.interactable = can;
        buildButton.GetComponentInChildren<Text>().text = $"Campfire\n<size=15><color=#EDE3C799>{fires.FirewoodToBuild} Firewood</color></size>";

        // Storage piles: a reason shows only if nothing can be built, so the campfire's own line isn't crowded out.
        WoodManager wood = WoodManager.Instance;
        string pileReason = null;
        foreach ((Button button, PileKind kind) in new[] { (woodPileButton, PileKind.WoodStorage), (rockPileButton, PileKind.RockStorage) })
        {
            button.gameObject.SetActive(wood != null);
            if (wood == null)
                continue;
            bool canPile = wood.CanBuildPile(kind, player, out _, out string why);
            button.interactable = canPile;
            int cost = wood.CostOf(kind);
            button.GetComponentInChildren<Text>().text =
                $"{wood.PileName(kind)}\n<size=15><color=#EDE3C799>{(cost > 0 ? $"{cost} Sticks" : "free")}</color></size>";
            if (!canPile && pileReason == null && kind == PileKind.WoodStorage)
                pileReason = why;
        }
        buildLabel.text = can ? "Builds just in front of you. Light it with Flint and Steel."
                        : pileReason != null && reason != pileReason ? $"Campfire: {reason}" : reason;

        for (int i = 0; i < Crafting.Recipes.Length; i++)
        {
            Crafting.Recipe recipe = Crafting.Recipes[i];
            bool canCraft = Crafting.CanCraft(recipe, out _);
            craftButtons[i].interactable = canCraft;
            string name = ItemDatabase.Get(recipe.outputId)?.DisplayName ?? recipe.outputId;
            craftButtons[i].GetComponentInChildren<Text>().text = $"{name}  <size=17><color=#EDE3C799>{Crafting.Cost(recipe)}</color></size>";
        }
    }

    void Craft(Crafting.Recipe recipe)
    {
        if (Crafting.Craft(recipe))
            ToolStatus.Flash($"Made a {ItemDatabase.Get(recipe.outputId)?.DisplayName} — equip it to set it");
    }

    void BuildPile(PileKind kind)
    {
        WoodManager wood = WoodManager.Instance;
        if (wood == null || !wood.BuildPile(kind, FindAnyObjectByType<PlayerController>()))
            return;
        ToolStatus.Flash(kind == PileKind.RockStorage ? "Rock Pile built — R to store Stone, E to take it"
                                                      : "Wood Pile built — R to store wood, E to take it");
        GameScreens screens = GetComponentInParent<GameScreens>();
        if (screens != null)
            screens.Close();
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

    void AddRow(ItemDefinition item, int quantity, bool equipped, bool atFire, PlayerController player)
    {
        bool isTool = item.Category == ItemCategory.Tool;
        UnityEngine.Events.UnityAction click = isTool ? () => ToggleEquip(item)
                                             : item.IsFood ? () => Food.Consume(item)
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
        string detail = item.IsFood ? FoodDetail(item) : CategoryName(item.Category);
        string tag = equipped ? "  <color=#C7D68C>• Equipped</color>" : "";
        string risk = Food.RiskTag(item);
        if (risk != null)
            tag += $"  <size=16><color=#E6A050>{risk}</color></size>";

        // Cook / Boil, beside the name, while standing at a lit campfire.
        bool cookable = Cooking.IsCookable(item.Id), boilable = Cooking.IsBoilable(item.Id);
        bool splittable = AxeTool.IsSplittable(item.Id) && AxeTool.AxeCarried;
        float nameRight = 0.62f;
        if ((atFire && (cookable || boilable)) || splittable)
        {
            nameRight = 0.5f;
            Button cook = UiKit.Button(rt, "Cook", splittable ? "Split" : boilable ? "Boil" : "Cook", 18,
                                       splittable ? (UnityEngine.Events.UnityAction)(() => ToggleSplit(item)) : () => ToggleCooking(item));
            cookLabels[item] = cook.GetComponentInChildren<Text>();
            var cookRt = (RectTransform)cook.transform;
            cookRt.anchorMin = new Vector2(0.5f, 0f);
            cookRt.anchorMax = new Vector2(0.61f, 1f);
            cookRt.offsetMin = new Vector2(0f, 7f);
            cookRt.offsetMax = new Vector2(0f, -7f);
            ColorBlock cookColors = cook.colors;
            cookColors.normalColor = new Color(0.36f, 0.25f, 0.13f, 1f);
            cookColors.highlightedColor = new Color(0.5f, 0.34f, 0.16f, 1f);
            cook.colors = cookColors;
        }
        Column(rt, $"{item.DisplayName}  <size=16><color=#EDE3C799>{detail}</color></size>{tag}", 0f, nameRight,
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

    // Queues one (Shift: the whole stack) at the fire, or stops it if it's already cooking.
    static void ToggleCooking(ItemDefinition item)
    {
        CampfireCooking cooking = CampfireCooking.Instance;
        if (cooking == null)
            return;
        if (cooking.IsQueued(item))
            cooking.Cancel(item);
        else
            cooking.Enqueue(item, ShiftHeld ? int.MaxValue : 1);
        if (AudioManager.Instance != null)
            AudioManager.Instance.Play(SoundCue.UiClick);
    }

    // Queues one Log or lot of Branches (Shift: all of them) to split into Firewood, or stops it.
    static void ToggleSplit(ItemDefinition item)
    {
        AxeTool axe = AxeTool.Instance;
        if (axe == null)
            return;
        if (axe.IsSplitQueued(item))
            axe.CancelSplit(item);
        else
            axe.EnqueueSplit(item, ShiftHeld ? int.MaxValue : 1);
        if (AudioManager.Instance != null)
            AudioManager.Instance.Play(SoundCue.UiClick);
    }

    static bool ShiftHeld =>
        UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.shiftKey.isPressed;

    // What eating or drinking one restores, e.g. "+30 food · +5 water".
    static string FoodDetail(ItemDefinition item)
    {
        string food = item.HungerRestored > 0f ? $"+{item.HungerRestored:0} food" : null;
        string water = item.HydrationRestored > 0f ? $"+{item.HydrationRestored:0} water" : null;
        return food != null && water != null ? $"{food} · {water}" : food ?? water;
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
