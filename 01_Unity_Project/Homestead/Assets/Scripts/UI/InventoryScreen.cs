using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Inventory_System.md's Inventory Screen (confirmed 2026-09-25): what the player is carrying, their weight against the
// Encumbered and maximum carry thresholds, and which Tool is equipped. Clicking a carried tool equips it, clicking the
// equipped tool again unequips it; clicking food or water eats or drinks one (Food). Standing by a burning campfire,
// raw meat and fish rows get a Cook button and raw water a Boil button (Cooking) — Shift-click does the whole stack.
// Each piece takes a few seconds (CampfireCooking); the button shows the progress, and clicking it again stops.
// Carrying the Axe, Logs and Branches get a Split button the same way, making Firewood (AxeTool).
// A Build section builds a campfire from carried Firewood (Fire System) or storage — Wood Pile, Rock Pile, Water
// Barrel, Food Cache, Storage Bin, Tool Rack (WoodManager) — and crafts traps and tools (Crafting's recipes), rows of
// three each; hovering a button says what it needs or why it can't be built there. Storage is filled and emptied in the
// World, not from this screen.
public class InventoryScreen : GameScreen
{
    const float KgToLb = 2.20462f;

    // Row columns: Item | Qty | Weight | Drop button.
    const float QtyMin = 0.5f, QtyMax = 0.66f, WeightMax = 0.84f;

    static readonly Color BarNormal = new Color(0.55f, 0.66f, 0.36f);
    static readonly Color BarEncumbered = new Color(0.84f, 0.63f, 0.3f);

    RectTransform list;
    Text weightLabel;
    Text statusLabel;
    Text equippedLabel;
    Text hintLabel;
    Button buildButton;
    readonly Button[] pileButtons = new Button[WoodManager.Buildable.Length];
    Text buildLabel;
    int hovered = -1; // build buttons 0 (campfire) up, then craft buttons from CraftHover
    const int CraftHover = 100;
    readonly Button[] craftButtons = new Button[Crafting.Recipes.Length];
    RectTransform barFill;
    Image barFillImage;
    RectTransform encumberedTick;
    Text encumberedTickLabel;
    InventoryManager watched;
    readonly Dictionary<ItemDefinition, Text> cookLabels = new Dictionary<ItemDefinition, Text>();
    readonly List<(RectTransform row, ItemDefinition tool)> toolRows = new List<(RectTransform, ItemDefinition)>();

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
        Column(header, "Qty", QtyMin, QtyMax, TextAnchor.MiddleRight, UiKit.Muted, 19, 0f);
        Column(header, "Weight", QtyMax, WeightMax, TextAnchor.MiddleRight, UiKit.Muted, 19, 0f, 8f);

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
        Top(statusLabel.rectTransform, 148f, 52f);

        Heading(right, "Equipped Tool", 204f);
        equippedLabel = UiKit.Text(right, "Equipped", "", 26, UiKit.Cream);
        Top(equippedLabel.rectTransform, 242f, 34f);

        hintLabel = UiKit.Text(right, "Hint", "", 16, UiKit.Muted);
        Top(hintLabel.rectTransform, 278f, 64f);

        // Building (Fire System).
        Heading(right, "Build", 344f);
        // Campfire and the storage kinds, three to a row.
        buildButton = UiKit.Button(right, "Build Campfire", "", 17, BuildCampfire);
        Grid(buildButton, 0, 382f, 42f);
        for (int i = 0; i < pileButtons.Length; i++)
        {
            PileKind kind = WoodManager.Buildable[i];
            pileButtons[i] = UiKit.Button(right, "Build " + kind, "", 17, () => BuildPile(kind));
            Grid(pileButtons[i], i + 1, 382f, 42f);
        }
        buildLabel = UiKit.Text(right, "Build Status", "", 15, UiKit.Muted);
        Top(buildLabel.rectTransform, 560f, 38f);

        // Traps and tools, three to a row.
        for (int i = 0; i < Crafting.Recipes.Length; i++)
        {
            Crafting.Recipe recipe = Crafting.Recipes[i];
            craftButtons[i] = UiKit.Button(right, "Craft " + recipe.outputId, "", 16, () => Craft(recipe));
            Grid(craftButtons[i], i, 600f, 40f);
        }

        // Hovering a button explains it in the status line.
        Hover(buildButton, 0);
        for (int i = 0; i < pileButtons.Length; i++)
            Hover(pileButtons[i], i + 1);
        for (int i = 0; i < craftButtons.Length; i++)
            Hover(craftButtons[i], CraftHover + i);
    }

    // Places a button in a three-wide grid starting at top.
    static void Grid(Button button, int index, float top, float height)
    {
        int column = index % 3, row = index / 3;
        var rt = (RectTransform)button.transform;
        Top(rt, top + row * (height + 2f), height);
        rt.anchorMin = new Vector2(column / 3f, 1f);
        rt.anchorMax = new Vector2((column + 1) / 3f, 1f);
        rt.offsetMin = new Vector2(column == 0 ? 0f : 2f, rt.offsetMin.y);
        rt.offsetMax = new Vector2(column == 2 ? 0f : -2f, rt.offsetMax.y);
        Text label = rt.GetComponentInChildren<Text>();
        label.rectTransform.Fill(4f, 0f, 4f, 0f);
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.lineSpacing = 0.9f;
    }

    void Hover(Button button, int index)
    {
        var trigger = button.gameObject.AddComponent<EventTrigger>();
        var enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enter.callback.AddListener(_ => { hovered = index; noticeUntil = 0f; RefreshBuild(); });
        var exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exit.callback.AddListener(_ => { if (hovered == index) { hovered = -1; RefreshBuild(); } });
        trigger.triggers.Add(enter);
        trigger.triggers.Add(exit);
    }

    public override void OnShow()
    {
        watched = InventoryManager.Instance;
        if (watched != null)
        {
            watched.Player.Changed += Refresh;
            watched.EquippedToolChanged += OnEquippedChanged;
            watched.HotkeysChanged += Refresh;
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
        watched.HotkeysChanged -= Refresh;
        watched = null;
    }

    void OnEquippedChanged(ItemDefinition tool) => Refresh();

    // The Cook / Boil buttons count down while their item is cooking.
    void Update()
    {
        if (watched != null)
            PollHotkeyAssign();
        if (notice != null && Time.unscaledTime >= noticeUntil)
        {
            notice = null;
            RefreshBuild();
        }

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
        toolRows.Clear();
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

        hintLabel.text = "Click a tool to equip it, food or water to use one, the Sleeping Bag to sleep. Drop: Shift = stack. Point at a tool, press 1-9/0 for a hotkey. " +
                         (atFire ? "<color=#C7D68C>At the campfire: Cook meat and fish, Boil water (needs the Cooking Pot). Shift: whole stack.</color>"
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
            buildLabel.text = Notice ?? "";
            return;
        }

        PlayerController player = FindAnyObjectByType<PlayerController>();
        bool can = fires.CanBuild(player, out _, out string reason);
        SetAvailable(buildButton, can);
        buildButton.GetComponentInChildren<Text>().text = $"Campfire\n<size=13><color=#EDE3C799>{fires.FirewoodToBuild} Firewood</color></size>";
        string status = hovered == 0 ? (can ? "Campfire: builds just in front of you. Light it with Flint and Steel." : $"Campfire: {reason}") : null;

        WoodManager wood = WoodManager.Instance;
        for (int i = 0; i < pileButtons.Length; i++)
        {
            Button button = pileButtons[i];
            button.gameObject.SetActive(wood != null);
            if (wood == null)
                continue;
            PileKind kind = WoodManager.Buildable[i];

            // The Small Cabin site's button does double duty: place an (empty, free) site normally, or — once a
            // nearby site is fully stocked and the Hammer's equipped — complete it into the real thing instead.
            if (kind == PileKind.CabinSite && wood.CanCompleteCabin(player, out _, out string completeReason))
            {
                SetAvailable(button, true);
                button.GetComponentInChildren<Text>().text = "Complete\nSmall Cabin";
                if (hovered == i + 1)
                    status = "Small Cabin: fully stocked — build it now.";
                continue;
            }

            bool canPile = wood.CanBuildPile(kind, player, out _, out string why);
            SetAvailable(button, canPile);
            button.GetComponentInChildren<Text>().text =
                $"{WoodManager.PileName(kind)}\n<size={CostSize(wood.CostText(kind), 13)}><color=#EDE3C799>{wood.CostText(kind)}</color></size>";
            if (hovered == i + 1)
                status = $"{WoodManager.PileName(kind)}: " + (canPile ? PileHelp(kind) : why);
        }

        for (int i = 0; i < Crafting.Recipes.Length; i++)
        {
            Crafting.Recipe recipe = Crafting.Recipes[i];
            bool canCraft = Crafting.CanCraft(recipe, out string why);
            SetAvailable(craftButtons[i], canCraft);
            string name = ItemDatabase.Get(recipe.outputId)?.DisplayName ?? recipe.outputId;
            string cost = Crafting.Cost(recipe);
            craftButtons[i].GetComponentInChildren<Text>().text = $"{name}\n<size={CostSize(cost, 12)}><color=#EDE3C799>{cost}</color></size>";
            if (hovered == CraftHover + i)
                status = $"{name}: " + (canCraft ? CraftHelp(recipe.outputId) : why);
        }

        buildLabel.text = Notice ?? status ?? "Hover a button to see what it needs. Click one you can't afford to see what's missing. Things you build go just in front of you.";
    }

    // --- Notice (Inventory_System.md's Build/Craft Missing-Materials Notice) ---

    // A refused click on a Build/Craft button, a drop, or a hotkey assignment: shown on the status line above the Craft
    // buttons for a few seconds, in place of the hover text, until it times out or the mouse moves to another button.
    // The screen draws over the HUD's message line, so this lives in the screen rather than going through ToolStatus.
    string notice;
    float noticeUntil;
    string Notice => notice != null && Time.unscaledTime < noticeUntil ? notice : null;

    void Notify(string message, bool warning = true, float seconds = 4f)
    {
        notice = warning ? $"<color=#E67350>{message}</color>" : message;
        noticeUntil = Time.unscaledTime + seconds;
        if (warning && AudioManager.Instance != null)
            AudioManager.Instance.Play(SoundCue.UiBack);
        RefreshBuild();
    }

    // Build and Craft buttons stay clickable when they can't be used — a click explains why (Notify) — so "unavailable"
    // is shown by dimming rather than by Button.interactable, which would swallow the click.
    static void SetAvailable(Button button, bool available)
    {
        button.interactable = true;
        CanvasGroup group = button.GetComponent<CanvasGroup>();
        if (group == null)
            group = button.gameObject.AddComponent<CanvasGroup>();
        group.alpha = available ? 1f : 0.45f;
    }

    // Long costs (the Lean-To's, Cordage's alternatives) in smaller type so they fit their button on one line.
    static int CostSize(string cost, int normal) => cost.Length > 30 ? 10 : cost.Length > 24 ? 11 : normal;

    static string PileHelp(PileKind kind)
    {
        switch (kind)
        {
            case PileKind.RockStorage: return "store Stone (R), take it back (E).";
            case PileKind.WaterBarrel: return "pour water in from the Bucket (R), fill the Bucket from it (E). Holds 40 L.";
            case PileKind.FoodCache: return "store food (R), take it back (E).";
            case PileKind.StorageBin: return "store Cordage, hides, furs, arrows and the like (R), take them back (E).";
            case PileKind.ToolRack: return "store Tools you're not carrying for a trip (R), take them back (E).";
            case PileKind.Tent: return "sleep in it (E), pack it up again (R). Keeps off rain, wind and much of the cold.";
            case PileKind.LeanTo: return "sleep in it (E), take it down (R). Keeps off most rain and wind, and some cold.";
            case PileKind.Cabin: return "sleep in it (E) — permanent, the best shelter yet. Comes with a hearth to warm up and cook at.";
            case PileKind.CabinSite: return "an empty building site — deposit Logs, Branches, Tall Grass, Stone and Clay into it (R) over however many trips it takes, take any of it back any time (E). Fully stocked, this button completes it (needs the Hammer equipped).";
            default: return "store wood (R), take it back (E).";
        }
    }

    static string CraftHelp(string itemId)
    {
        switch (itemId)
        {
            case "stone_pick_axe": return "mines Stone from the rock outcrop on South Ridge.";
            case "shovel": return "digs out stumps.";
            case "primitive_axe": return "fells trees and splits wood like the Axe, just slower.";
            case "knife": return "carried, it lets you field dress kills and take game from traps.";
            case "pouch": return "carried, it lets you carry 10 kg more.";
            case "cordage": return "twisted from whichever fibre you have.";
            case "hammer": return "equip it to build a Small Cabin.";
            default: return "equip it to set it.";
        }
    }

    void Craft(Crafting.Recipe recipe)
    {
        string name = ItemDatabase.Get(recipe.outputId)?.DisplayName ?? recipe.outputId;
        if (!Crafting.CanCraft(recipe, out string why))
        {
            Notify($"Can't make {name} — {why}");
            return;
        }

        noticeUntil = 0f;
        if (Crafting.Craft(recipe))
            ToolStatus.Flash($"Made a {name} — {CraftHelp(recipe.outputId)}");
    }

    void BuildPile(PileKind kind)
    {
        WoodManager wood = WoodManager.Instance;
        if (wood == null)
            return;
        PlayerController player = FindAnyObjectByType<PlayerController>();

        if (kind == PileKind.CabinSite && wood.CanCompleteCabin(player, out _, out _))
        {
            if (!wood.CompleteCabin(player))
                return;
            ToolStatus.Flash("Small Cabin built — you can sleep in it, and its hearth is ready for Firewood.");
            Close();
            return;
        }

        if (!wood.CanBuildPile(kind, player, out _, out string why))
        {
            Notify($"Can't build {WoodManager.PileName(kind)} — {why}");
            return;
        }

        if (!wood.BuildPile(kind, player))
            return;
        noticeUntil = 0f;
        ToolStatus.Flash($"{WoodManager.PileName(kind)} built — {PileHelp(kind)}");
        Close();
    }

    void Close()
    {
        GameScreens screens = GetComponentInParent<GameScreens>();
        if (screens != null)
            screens.Close();
    }

    void BuildCampfire()
    {
        FireManager fires = FireManager.Instance;
        if (fires == null)
            return;
        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (!fires.CanBuild(player, out _, out string why))
        {
            Notify($"Can't build a Campfire — {why}");
            return;
        }
        if (!fires.Build(player))
            return;
        noticeUntil = 0f;

        // (Using up the Firewood already plays the item-drop sound.)
        // Back to the world, so the player sees what they built.
        GameScreens screens = GetComponentInParent<GameScreens>();
        if (screens != null)
            screens.Close();
    }

    void AddRow(ItemDefinition item, int quantity, bool equipped, bool atFire, PlayerController player)
    {
        bool isTool = item.Category == ItemCategory.Tool;
        UnityEngine.Events.UnityAction click = item.Id == SleepManager.SleepingBagId ? SleepHere
                                             : item.Id == WoodManager.TentId ? null
                                             : isTool ? () => ToggleEquip(item)
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
        float nameRight = QtyMin;
        if ((atFire && (cookable || boilable)) || splittable)
        {
            nameRight = 0.36f;
            Button cook = UiKit.Button(rt, "Cook", splittable ? "Split" : boilable ? "Boil" : "Cook", 18,
                                       splittable ? (UnityEngine.Events.UnityAction)(() => ToggleSplit(item)) : () => ToggleCooking(item));
            cookLabels[item] = cook.GetComponentInChildren<Text>();
            var cookRt = (RectTransform)cook.transform;
            cookRt.anchorMin = new Vector2(0.37f, 0f);
            cookRt.anchorMax = new Vector2(0.49f, 1f);
            cookRt.offsetMin = new Vector2(0f, 7f);
            cookRt.offsetMax = new Vector2(0f, -7f);
            ColorBlock cookColors = cook.colors;
            cookColors.normalColor = new Color(0.36f, 0.25f, 0.13f, 1f);
            cookColors.highlightedColor = new Color(0.5f, 0.34f, 0.16f, 1f);
            cook.colors = cookColors;
        }
        // Only Tools that clicking equips: the Sleeping Bag (sleeps) and Tent (placed) are Tools by category but aren't held.
        bool hotkeyable = isTool && item.Id != SleepManager.SleepingBagId && item.Id != WoodManager.TentId;
        if (hotkeyable)
            nameRight = 0.36f; // room for the hotkey button
        Column(rt, $"{item.DisplayName}  <size=16><color=#EDE3C799>{detail}</color></size>{tag}", 0f, nameRight,
               TextAnchor.MiddleLeft, UiKit.Cream, 21, 12f);
        // Tool Hotkeys (Inventory_System.md): the number key that equips this Tool. Click cycles 1-9, 0, none;
        // or point at the row and press a number key.
        if (hotkeyable)
        {
            toolRows.Add((rt, item));
            InventoryManager inv = InventoryManager.Instance;
            int slot = inv != null ? inv.HotkeySlotOf(item.Id) : -1;
            Button key = UiKit.Button(rt, "Hotkey", slot >= 0 ? $"Key {InventoryManager.HotkeyLabel(slot)}" : "Key —", 16,
                                      () => CycleHotkey(item));
            var keyRt = (RectTransform)key.transform;
            keyRt.anchorMin = new Vector2(0.37f, 0f);
            keyRt.anchorMax = new Vector2(0.49f, 1f);
            keyRt.offsetMin = new Vector2(0f, 7f);
            keyRt.offsetMax = new Vector2(0f, -7f);
        }

        Column(rt, $"×{quantity}", QtyMin, QtyMax, TextAnchor.MiddleRight, UiKit.Cream, 21, 0f);
        Column(rt, $"{item.WeightKg * quantity:0.0} kg", QtyMax, WeightMax, TextAnchor.MiddleRight, UiKit.Cream, 21, 0f, 8f);

        // Drop (Inventory_System.md's Dropping Items): one, or Shift-click the whole stack. Works on tools too —
        // dropping the last one puts it away first, since InventoryManager unequips a tool that's no longer carried.
        Button drop = UiKit.Button(rt, "Drop", "Drop", 18, () => DropItem(item));
        var dropRt = (RectTransform)drop.transform;
        dropRt.anchorMin = new Vector2(0.86f, 0f);
        dropRt.anchorMax = new Vector2(0.99f, 1f);
        dropRt.offsetMin = new Vector2(0f, 7f);
        dropRt.offsetMax = new Vector2(0f, -7f);
    }

    // Next slot for a Tool: 1..9, 0, then off.
    static void CycleHotkey(ItemDefinition tool)
    {
        InventoryManager inventory = InventoryManager.Instance;
        if (inventory == null)
            return;
        int current = inventory.HotkeySlotOf(tool.Id);
        if (current + 1 < InventoryManager.HotkeySlots)
            inventory.SetHotkey(current + 1, tool.Id); // none -> slot 0, then onward
        else
            inventory.SetHotkey(current, null);        // past the last slot: off
        if (AudioManager.Instance != null)
            AudioManager.Instance.Play(SoundCue.UiClick);
    }

    // Pointing at a Tool row and pressing 1-9 or 0 assigns that key to it.
    void PollHotkeyAssign()
    {
        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        var mouse = UnityEngine.InputSystem.Mouse.current;
        InventoryManager inventory = InventoryManager.Instance;
        if (keyboard == null || mouse == null || inventory == null || toolRows.Count == 0)
            return;

        int pressed = -1;
        for (int i = 0; i < InventoryManager.HotkeySlots; i++)
        {
            if (keyboard[InventoryManager.HotkeyKeys[i]].wasPressedThisFrame)
                pressed = i;
        }
        if (pressed < 0)
            return;

        Canvas canvas = GetComponentInParent<Canvas>();
        Camera cam = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
        Vector2 point = mouse.position.ReadValue();
        foreach ((RectTransform row, ItemDefinition tool) in toolRows)
        {
            if (row != null && RectTransformUtility.RectangleContainsScreenPoint(row, point, cam))
            {
                inventory.SetHotkey(pressed, tool.Id);
                Notify($"{tool.DisplayName} is on key {InventoryManager.HotkeyLabel(pressed)}.", warning: false, seconds: 2.5f);
                return;
            }
        }
    }

    void DropItem(ItemDefinition item)
    {
        PlayerController player = FindAnyObjectByType<PlayerController>();
        InventoryManager inventory = InventoryManager.Instance;
        if (inventory == null)
            return;

        int count = ShiftHeld ? inventory.Player.Count(item.Id) : 1;
        int dropped = DroppedItems.Drop(player, item.Id, count, out string reason);
        Notify(dropped > 0 ? $"Dropped {dropped} {item.DisplayName} on the ground in front of you." : reason, warning: dropped <= 0, seconds: 2.5f);
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

    // The Sleeping Bag: sleep right here (SleepManager), back to the world first.
    void SleepHere()
    {
        SleepManager sleep = SleepManager.Instance;
        if (sleep == null)
            return;
        string reason = sleep.CantSleepReason(null);
        if (reason != null)
        {
            ToolStatus.Flash(reason);
            return;
        }
        GameScreens screens = GetComponentInParent<GameScreens>();
        if (screens != null)
            screens.Close();
        sleep.Sleep(null);
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
