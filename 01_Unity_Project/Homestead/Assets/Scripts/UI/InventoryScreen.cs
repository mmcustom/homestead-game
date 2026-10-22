using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Inventory_System.md's Inventory Screen (confirmed 2026-09-25): what the player is carrying, their weight against the
// Encumbered and maximum carry thresholds, and which Tool is equipped. Clicking a carried tool equips it, clicking the
// equipped tool again unequips it; clicking food or water eats or drinks one (Food). Standing by a burning campfire,
// raw meat and fish rows get a Cook button and raw water a Boil button (Cooking) — Shift-click does the whole stack.
// Each piece takes a few seconds (CampfireCooking); the row shows the progress, and its Cancel button stops it.
// Carrying the Axe, Logs and Branches get a Split button the same way, making Firewood (AxeTool).
// Every row is two lines (Quantity_Controls.md, Part 2): the name, quantity and weight on top (click it to equip a tool, eat or drink),
// and below it ONE quantity stepper (QuantityStepper) that the row's buttons all use: "Drop n", "Cook n", "Boil n", "Split n" and
// "Refill n". A plain click uses the amount in the stepper (1 by default, back to 1 after every move); Shift-click keeps the old shortcut:
// Drop drops the stack, Cook and Split queue them all, Refill pours as many as fit. The stepper's most is what is carried; a button
// clamps to its own limit (the room in the Lantern's tank, the splits the pack can make). The queue buttons (Cook, Boil, Split) add to the
// queue, and Cancel is a separate button that shows while one is queued.
// Building and crafting moved to their own Craft and Build screen (CraftingScreen, key B, 2026-10-09) — a button here
// opens it. Storage is filled and emptied in the World, not from this screen.
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
    Button craftButton;
    Text noticeLabel;
    readonly ScreenBanner banner = new ScreenBanner();
    RectTransform barFill;
    Image barFillImage;
    RectTransform encumberedTick;
    Text encumberedTickLabel;
    InventoryManager watched;

    // The amount chosen in each row's stepper, by item id. Rows are rebuilt on every refresh, so the amounts live here; they go back to 1
    // after a move and are cleared when the screen hides.
    readonly Dictionary<string, int> amounts = new Dictionary<string, int>();

    // A row with a queue (Cook, Boil, Split): the line that shows its progress and the Cancel button that shows only while it is queued.
    class QueueRow
    {
        public ItemDefinition item;
        public bool split;
        public Text info;
        public Button cancel;
        public string idleInfo;
    }

    readonly List<QueueRow> queueRows = new List<QueueRow>();
    bool refreshPending; // a refresh was skipped while a quantity box had the keyboard: it happens when the typing ends
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
        Top(hintLabel.rectTransform, 278f, 88f);

        // Building and crafting live on their own screen now.
        craftButton = UiKit.Button(right, "Craft and Build", "Craft and Build  [B]", 22, OpenCrafting);
        Top((RectTransform)craftButton.transform, 374f, 52f);
        noticeLabel = UiKit.Text(right, "Notice", "", 18, UiKit.Muted);
        Top(noticeLabel.rectTransform, 436f, 56f);

        banner.Build(area); // last, so it draws over everything else
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

    public override void OnHide()
    {
        banner.Hide();
        Unsubscribe();
        amounts.Clear();
        refreshPending = false;
        UiKit.ReleaseFocus();
    }

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
            RefreshNotice();
        }
        banner.Tick();

        if (refreshPending && !TypingInRows)
        {
            Refresh();
            return;
        }

        CampfireCooking cooking = CampfireCooking.Instance;
        AxeTool axe = AxeTool.Instance;
        foreach (QueueRow q in queueRows)
        {
            if (q.info == null)
                continue;
            bool queued = q.split ? axe != null && axe.IsSplitQueued(q.item) : cooking != null && cooking.IsQueued(q.item);
            if (q.cancel != null)
                q.cancel.gameObject.SetActive(queued);
            if (!queued)
            {
                q.info.text = q.idleInfo;
                continue;
            }
            float p = q.split ? axe.SplitProgressOf(q.item) : cooking.ProgressOf(q.item);
            int left = q.split ? axe.SplitsLeft(q.item) : cooking.RemainingOf(q.item);
            q.info.text = p >= 0f ? $"{p * 100f:0}%  ({left} left)" : $"Queued ({left})";
        }
    }

    // True while the player is typing in one of this screen's quantity boxes: a rebuild then would throw the typed amount away.
    bool TypingInRows
    {
        get
        {
            if (!GameScreens.IsTyping || EventSystem.current == null)
                return false;
            GameObject selected = EventSystem.current.currentSelectedGameObject;
            return selected != null && selected.transform.IsChildOf(list);
        }
    }

    void Refresh()
    {
        if (TypingInRows)
        {
            refreshPending = true; // the typed amount must not be lost to a rebuild
            return;
        }

        refreshPending = false;
        UiKit.Clear(list);
        queueRows.Clear();
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

        hintLabel.text = "Click a tool to equip it, food or water to use one. Set an amount with - / + / Max or type it; Shift-click Drop: the stack. Point at a tool, press 1-9/0: hotkey. " +
                         (atFire ? "<color=#C7D68C>At the fire: Cook meat and fish, Boil water (needs the Pot).</color>"
                                 : "Cook at a burning campfire; split Logs and Branches with the Axe.");

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
        RefreshNotice();
    }

    // --- Notice ---

    // Two kinds of message, neither going through the HUD's message line (the screen draws over it):
    //  - Information (a drop, a hotkey assignment, a refill): a small status line under the Equipped Tool, for a few
    //    seconds.
    //  - A refusal (a drop that can't happen): a banner across the top of the screen in large type on a solid panel —
    //    Mike found the small line hard to read (2026-10-04); see ScreenBanner.
    string notice;
    float noticeUntil;
    string Notice => notice != null && Time.unscaledTime < noticeUntil ? notice : null;

    void RefreshNotice()
    {
        if (noticeLabel != null)
            noticeLabel.text = Notice ?? "";
    }

    void OpenCrafting()
    {
        GameScreens screens = GetComponentInParent<GameScreens>();
        if (screens != null)
            screens.OpenCrafting();
    }

    void Notify(string message, bool warning = true, float seconds = 4f)
    {
        if (warning)
        {
            banner.Show(message, null, new List<string>());
            return;
        }

        notice = message;
        noticeUntil = Time.unscaledTime + seconds;
        RefreshNotice();
    }

    // Pours Lamp Oil into the Lantern: the stepper's amount, or with Shift-click as many as fit. The tank takes what has room.
    void RefillLantern(int amount, bool all)
    {
        PortableLight light = PortableLight.Instance;
        if (light == null)
            return;
        if (!light.CanRefill)
        {
            Notify("The Lantern's tank is full.");
            return;
        }
        amounts.Remove(PortableLight.LampOilId); // back to 1 once the move has happened
        int used = light.Refill(all ? int.MaxValue : amount);
        Notify($"Poured in {used} Lamp Oil — the Lantern has {light.LanternFuelHours:0.0} h of oil.", warning: false, seconds: 3f);
        if (AudioManager.Instance != null)
            AudioManager.Instance.Play(SoundCue.Pour);
        Refresh();
    }

    bool inventoryHasLantern;

    const float RowHeight = 84f;
    const float ControlY = 5f; // line two's controls: this far above the row's bottom edge

    void AddRow(ItemDefinition item, int quantity, bool equipped, bool atFire, PlayerController player)
    {
        inventoryHasLantern = InventoryManager.Instance != null && InventoryManager.Instance.Player.Has(PortableLight.LanternId);
        bool isTool = item.Category == ItemCategory.Tool;
        UnityEngine.Events.UnityAction click = item.Id == SleepManager.SleepingBagId ? SleepHere
                                             : InventoryManager.IsCarriedOnly(item.Id) ? null
                                             : isTool ? () => ToggleEquip(item)
                                             : item.IsFood ? () => Food.Consume(item)
                                             : (UnityEngine.Events.UnityAction)null;

        // The row is a panel. Its top half is the old click-to-equip or eat row, a button so it lights up under the pointer; the bottom
        // half holds the stepper and the action buttons, outside that button so a click there never equips or eats by accident.
        Image panel = UiKit.Image(list, item.Id, equipped ? UiKit.SelectedColor : UiKit.RaisedColor);
        RectTransform rt = panel.rectTransform;
        UiKit.Height(rt, RowHeight);
        if (click != null)
        {
            Button main = UiKit.Button(rt, "Main", "", 20, click);
            Destroy(main.GetComponentInChildren<Text>().gameObject);
            var mainRt = (RectTransform)main.transform;
            mainRt.anchorMin = new Vector2(0f, 0.5f);
            mainRt.anchorMax = Vector2.one;
            mainRt.offsetMin = mainRt.offsetMax = Vector2.zero;
            ColorBlock mainColors = main.colors;
            var clear = new Color(0f, 0f, 0f, 0f);
            mainColors.normalColor = mainColors.selectedColor = mainColors.disabledColor = clear;
            mainColors.highlightedColor = new Color(UiKit.HoverColor.r, UiKit.HoverColor.g, UiKit.HoverColor.b, 0.6f);
            main.colors = mainColors;
        }

        string detail = item.IsFood ? FoodDetail(item) : CategoryName(item.Category);
        string tag = equipped ? "  <color=#C7D68C>• Equipped</color>" : "";
        PortableLight light = PortableLight.Instance;
        if (light != null && item.Id == PortableLight.TorchId && light.TorchHoursLeft > 0f)
            tag += $"  <size=16><color=#E6A050>{light.TorchHoursLeft:0.0} h</color></size>";
        else if (light != null && item.Id == PortableLight.LanternId)
            tag += $"  <size=16><color=#E6A050>{light.LanternFuelHours:0.0} h</color></size>";
        string risk = Food.RiskTag(item);
        if (risk != null)
            tag += $"  <size=16><color=#E6A050>{risk}</color></size>";

        bool cookable = Cooking.IsCookable(item.Id), boilable = Cooking.IsBoilable(item.Id);
        bool splittable = AxeTool.IsSplittable(item.Id) && AxeTool.AxeCarried;
        bool refillable = item.Id == PortableLight.LampOilId && inventoryHasLantern; // Lamp Oil, while a Lantern is carried
        bool cooks = atFire && (cookable || boilable);
        // Only Tools that clicking equips: the Sleeping Bag (sleeps), Tent (placed) and Weather Station (carried) are Tools by category but aren't held.
        bool hotkeyable = isTool && !InventoryManager.IsCarriedOnly(item.Id);

        // Line one: name, the hotkey button, quantity, weight.
        Column(rt, $"{item.DisplayName}  <size=16><color=#EDE3C799>{detail}</color></size>{tag}", 0f, hotkeyable ? 0.36f : QtyMin,
               TextAnchor.MiddleLeft, UiKit.Cream, 21, 12f, 0f, 0.5f);
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
            keyRt.anchorMin = new Vector2(0.37f, 0.5f);
            keyRt.anchorMax = new Vector2(0.49f, 1f);
            keyRt.offsetMin = new Vector2(0f, 4f);
            keyRt.offsetMax = new Vector2(0f, -4f);
        }
        Column(rt, $"×{quantity}", QtyMin, QtyMax, TextAnchor.MiddleRight, UiKit.Cream, 21, 0f, 0f, 0.5f);
        Column(rt, $"{item.WeightKg * quantity:0.0} kg", QtyMax, WeightMax, TextAnchor.MiddleRight, UiKit.Cream, 21, 0f, 8f, 0.5f);

        // Line two: the stepper (the most is what is carried), what the row's queue is doing, then the buttons from the right.
        QuantityStepper stepper = QuantityStepper.Create(rt, amounts.TryGetValue(item.Id, out int chosen) ? chosen : 1, quantity);
        RectTransform stepperRt = stepper.Root;
        stepperRt.anchorMin = stepperRt.anchorMax = stepperRt.pivot = Vector2.zero;
        stepperRt.anchoredPosition = new Vector2(12f, ControlY);

        Button AddButton(string label, float fromRight, float width, UnityEngine.Events.UnityAction onClick)
        {
            Button b = UiKit.Button(rt, label, label, 18, onClick);
            QuantityStepper.Style(b);
            var brt = (RectTransform)b.transform;
            brt.anchorMin = brt.anchorMax = brt.pivot = new Vector2(1f, 0f);
            brt.sizeDelta = new Vector2(width, QuantityStepper.Height);
            brt.anchoredPosition = new Vector2(-fromRight, ControlY);
            return b;
        }

        // A button's click: whether Shift was held, the keyboard handed back to the screen, and the stepper's amount.
        UnityEngine.Events.UnityAction Act(System.Action<bool, int> move) => () =>
        {
            bool shift = UiKit.ShiftHeld;
            UiKit.ReleaseFocus();
            move(shift, stepper.Amount);
        };

        Button drop = AddButton("Drop", 8f, 112f, Act((shift, n) => DropItem(item, n, shift)));
        Text dropLabel = drop.GetComponentInChildren<Text>();

        Button action = null;
        Text actionLabel = null;
        QueueRow queue = null;
        string idleInfo = "";
        System.Func<string> actionText = null;
        if (refillable)
        {
            PortableLight lantern = PortableLight.Instance;
            int room = lantern != null ? lantern.RefillRoom : 0;
            action = AddButton("Refill", 126f, 118f, Act((shift, n) => RefillLantern(n, shift)));
            QuantityStepper.SetEnabled(action, lantern != null && lantern.CanRefill);
            actionText = () => room > 0 ? $"Refill {QuantityRules.MaxOf(stepper.Amount, room)}" : "Refill";
            idleInfo = room > 0 ? $"room for {room} in the tank" : "the tank is full";
        }
        else if (splittable)
        {
            AxeTool axe = AxeTool.Instance;
            int splits = axe != null ? axe.SplitCapacity(item) : 0;
            action = AddButton("Split", 126f, 118f, Act((shift, n) => QueueSplit(item, shift ? int.MaxValue : n)));
            actionText = () => $"Split {Mathf.Min(stepper.Amount, Mathf.Max(1, splits))}";
            idleInfo = splits > 0 ? $"{splits} to split" : "not enough to split";
            queue = new QueueRow { item = item, split = true };
        }
        else if (cooks)
        {
            string verb = boilable ? "Boil" : "Cook";
            bool potMissing = boilable && !Cooking.HasPot;
            action = AddButton(verb, 126f, 118f, Act((shift, n) => QueueCooking(item, shift ? int.MaxValue : n)));
            QuantityStepper.SetEnabled(action, QuantityRules.MaxOf(quantity, potMissing ? 0 : QuantityRules.MaxAmount) > 0); // boiling needs the Cooking Pot
            actionText = () => $"{verb} {stepper.Amount}";
            idleInfo = potMissing ? "needs the Cooking Pot" : "";
            queue = new QueueRow { item = item, split = false };
        }

        if (action != null)
        {
            // Cook and Split are queues that run over time: a separate Cancel button stops one, so it never fights the stepper.
            actionLabel = action.GetComponentInChildren<Text>();
            var brownColors = action.colors;
            brownColors.normalColor = new Color(0.36f, 0.25f, 0.13f, 1f);
            brownColors.selectedColor = brownColors.normalColor;
            brownColors.highlightedColor = new Color(0.5f, 0.34f, 0.16f, 1f);
            action.colors = brownColors;
            actionLabel.text = actionText();
        }

        Text info = UiKit.Text(rt, "Info", idleInfo, 16, UiKit.Muted, TextAnchor.MiddleLeft);
        info.horizontalOverflow = HorizontalWrapMode.Overflow;
        info.rectTransform.anchorMin = Vector2.zero;
        info.rectTransform.anchorMax = new Vector2(1f, 0.5f);
        info.rectTransform.offsetMin = new Vector2(250f, 0f);
        info.rectTransform.offsetMax = new Vector2(action == null ? -130f : queue != null ? -356f : -256f, 0f);

        if (queue != null)
        {
            Button cancel = AddButton("Cancel", 252f, 92f, () =>
            {
                UiKit.ReleaseFocus();
                CancelQueue(item, queue.split);
            });
            cancel.gameObject.SetActive(false);
            queue.cancel = cancel;
            queue.info = info;
            queue.idleInfo = idleInfo;
            queueRows.Add(queue);
        }

        void Relabel()
        {
            dropLabel.text = $"Drop {stepper.Amount}";
            if (actionLabel != null)
                actionLabel.text = actionText();
        }

        stepper.Changed += () =>
        {
            amounts[item.Id] = stepper.Amount;
            Relabel();
        };
        Relabel();
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
        if (GameScreens.IsTyping)
            return; // typing a number (an amount box) must not assign a hotkey to the tool under the pointer

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

    // Drops the stepper's amount, or with Shift-click the whole stack.
    void DropItem(ItemDefinition item, int amount, bool all)
    {
        PlayerController player = FindAnyObjectByType<PlayerController>();
        InventoryManager inventory = InventoryManager.Instance;
        if (inventory == null)
            return;

        int count = all ? inventory.Player.Count(item.Id) : amount;
        amounts.Remove(item.Id); // back to 1 once the move has happened (the rebuild the drop causes reads it)
        int dropped = DroppedItems.Drop(player, item.Id, count, out string reason);
        Notify(dropped > 0 ? $"Dropped {dropped} {item.DisplayName} on the ground in front of you." : reason, warning: dropped <= 0, seconds: 2.5f);
        if (dropped <= 0)
            Refresh();
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

    // Queues count more at the fire (int.MaxValue: the whole stack). Cancel is its own button.
    void QueueCooking(ItemDefinition item, int count)
    {
        CampfireCooking cooking = CampfireCooking.Instance;
        if (cooking == null)
            return;
        amounts.Remove(item.Id); // back to 1 once it is queued
        if (cooking.Enqueue(item, count) && AudioManager.Instance != null)
            AudioManager.Instance.Play(SoundCue.UiClick);
        Refresh();
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

    // Queues count more Logs, or lots of Branches, to split into Firewood (int.MaxValue: all of them). Cancel is its own button.
    void QueueSplit(ItemDefinition item, int count)
    {
        AxeTool axe = AxeTool.Instance;
        if (axe == null)
            return;
        amounts.Remove(item.Id); // back to 1 once it is queued
        if (axe.EnqueueSplit(item, count) && AudioManager.Instance != null)
            AudioManager.Instance.Play(SoundCue.UiClick);
        Refresh();
    }

    // Stops a queued cook, boil or split.
    void CancelQueue(ItemDefinition item, bool split)
    {
        if (split)
        {
            if (AxeTool.Instance != null)
                AxeTool.Instance.CancelSplit(item);
        }
        else if (CampfireCooking.Instance != null)
        {
            CampfireCooking.Instance.Cancel(item);
        }
        if (AudioManager.Instance != null)
            AudioManager.Instance.Play(SoundCue.UiClick);
    }

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
                       float padLeft, float padRight = 0f, float yMin = 0f)
    {
        Text label = UiKit.Text(parent, "Column", text, size, color, anchor);
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        label.rectTransform.anchorMin = new Vector2(xMin, yMin);
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
