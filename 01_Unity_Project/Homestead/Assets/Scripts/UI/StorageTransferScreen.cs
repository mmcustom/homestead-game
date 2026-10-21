using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Primitive_Storage_System.md's transfer screen, two-way (2026-10-04 — Mike's playtest: "i think we need to have an
// individual inventory screen for each structure that we can put items in individually and remove individually. maybe
// when working with one of the structures open 2 seperate inventory windows"). Opened by GameScreens.OpenStorage when
// the player presses E or R on a built storage pile — Wood Pile, Rock Pile, Water Barrel, Food Cache, Storage Bin or
// Tool Rack (WoodPile.UsesTransferScreen) — targeting that one pile. Two panels in the one window, so the other tabs
// stay reachable: the player's pack on the left (only what this structure holds), the structure's contents on the
// right, each with its weight or capacity. Every row is two lines (QuantityRowUi): the name and what is there, then a quantity stepper
// (Quantity_Controls.md) with the button. Click moves the amount shown in the stepper (1 by default, back to 1 after every move),
// Shift-click the whole stack, same convention as Trading Post's Buy/Sell. The most that can move is worked out by the pile
// (WoodPile.MaxStore and MaxTake: what is carried or stored, held to the carry weight, the barrel's litres or Bucket and Canteen room),
// so the amount is clamped to what fits and the status line reports what actually moved. A row that can't move stays, greyed, with the
// reason. The Water Barrel
// breaks out by quality on both sides, so the player picks which water goes in or comes out. A shortcut button heads
// each panel and keeps the old one-press behavior: "Store All Eligible" on the left, "Take Everything" (or "Fill Bucket
// (best quality)" for the barrel) on the right.
public class StorageTransferScreen : GameScreen
{
    // Water best quality first, the order the barrel and the Bucket have always used.
    static readonly string[] WaterOrder = { Cooking.PurifiedWaterId, "water_excellent", "water_good", "water_questionable", "water_unsafe" };

    RectTransform packList, structureList;
    Text packWeight, structureWeight, statusLabel;
    Button storeAllButton, takeAllButton, removeButton;
    WoodPile target;
    bool confirmingRemove;

    // The amount chosen in each row's stepper, by item id (the pack side and the structure side separately). The rows are rebuilt on
    // every refresh, so the amounts live here; they go back to 1 after a move and are cleared when the screen hides.
    readonly Dictionary<string, int> storeAmounts = new Dictionary<string, int>();
    readonly Dictionary<string, int> takeAmounts = new Dictionary<string, int>();

    public override string Title => target != null && target.State != null ? WoodManager.PileName(target.State.kind) : "Storage";

    public override void Build(RectTransform area)
    {
        statusLabel = UiKit.Text(area, "Status", "", 17, UiKit.Muted, TextAnchor.MiddleLeft);
        statusLabel.rectTransform.anchorMin = Vector2.zero;
        statusLabel.rectTransform.anchorMax = new Vector2(0.62f, 0f);
        statusLabel.rectTransform.offsetMin = new Vector2(6f, 0f);
        statusLabel.rectTransform.offsetMax = new Vector2(0f, 30f);

        // Takes a misplaced structure down (WoodPile.RemoveStructure) — two clicks, so it can't be done by accident.
        removeButton = UiKit.Button(area, "Remove", "", 16, RemoveClicked);
        var removeRt = (RectTransform)removeButton.transform;
        removeRt.anchorMin = new Vector2(0.64f, 0f);
        removeRt.anchorMax = new Vector2(1f, 0f);
        removeRt.offsetMin = new Vector2(0f, 0f);
        removeRt.offsetMax = new Vector2(0f, 30f);

        RectTransform body = UiKit.Rect("Body", area);
        body.Fill(0f, 34f, 0f, 0f);
        // (Region repositions the rect it's called on, so each side needs its own.)
        BuildPanel(UiKit.Rect("Pack", body).Region(0f, 0f, 0.495f, 1f), "You're carrying", out packList, out packWeight, out storeAllButton, StoreAll);
        BuildPanel(UiKit.Rect("Structure", body).Region(0.505f, 0f, 1f, 1f), "In the structure", out structureList, out structureWeight, out takeAllButton, TakeShortcut);
    }

    // One side: a heading, a weight/capacity line, a shortcut button, and a scrolling list.
    static void BuildPanel(RectTransform panel, string heading, out RectTransform list, out Text readout, out Button shortcut,
                           UnityEngine.Events.UnityAction shortcutClick)
    {
        Text title = UiKit.Text(panel, "Heading", heading, 26, UiKit.Accent, TextAnchor.MiddleLeft, FontStyle.Italic);
        Top(title.rectTransform, 0f, 34f);

        readout = UiKit.Text(panel, "Readout", "", 19, UiKit.Cream, TextAnchor.MiddleLeft);
        Top(readout.rectTransform, 34f, 28f);

        shortcut = UiKit.Button(panel, "Shortcut", "", 18, shortcutClick);
        Top((RectTransform)shortcut.transform, 66f, 38f);

        list = UiKit.ScrollList(panel, "List");
        ((RectTransform)list.parent).Fill(0f, 0f, 0f, 112f);
    }

    public void SetTarget(WoodPile pile) => target = pile;

    // No Player.Changed subscription: GameManager is in the Menu state while this is open, so nothing can change the
    // pile or the player's pack except this screen's own buttons — which call Refresh themselves after.
    public override void OnShow()
    {
        statusLabel.text = "Click moves the amount shown; Shift-click moves the whole stack.";
        storeAmounts.Clear();
        takeAmounts.Clear();
        Refresh();
    }

    public override void OnHide()
    {
        storeAmounts.Clear();
        takeAmounts.Clear();
        UiKit.ReleaseFocus();
    }

    void Refresh()
    {
        UiKit.Clear(packList);
        UiKit.Clear(structureList);
        confirmingRemove = false;
        InventoryManager inventory = InventoryManager.Instance;
        if (target == null || target.State == null || inventory == null)
        {
            storeAllButton.gameObject.SetActive(false);
            takeAllButton.gameObject.SetActive(false);
            removeButton.gameObject.SetActive(false);
            packWeight.text = structureWeight.text = "";
            return;
        }

        // Taking a stray structure down is only offered once it's empty — nothing disappears with it.
        removeButton.gameObject.SetActive(true);
        removeButton.interactable = target.CanRemove(out string removeReason);
        removeButton.GetComponentInChildren<Text>().text = removeButton.interactable ? "Take Down Structure" : $"Take Down — {removeReason}";

        WoodPileState state = target.State;
        bool barrel = state.IsWaterBarrel;
        storeAllButton.gameObject.SetActive(true);
        takeAllButton.gameObject.SetActive(true);
        storeAllButton.GetComponentInChildren<Text>().text = "Store All Eligible";
        takeAllButton.GetComponentInChildren<Text>().text = barrel ? "Fill Bucket (best quality)" : "Take Everything";

        RefreshPack(inventory, state, barrel);
        RefreshStructure(state, barrel);
    }

    // Left: carried weight against the cap, then each kind of thing carried that this structure holds.
    void RefreshPack(InventoryManager inventory, WoodPileState state, bool barrel)
    {
        float carried = inventory.CarriedWeightKg, max = inventory.MaxCarryWeightKg;
        string water = barrel ? $"   ·   water containers {WaterQualities.LitresCarried(inventory.Player)} / {WaterQualities.Capacity(inventory.Player)} L" : "";
        packWeight.text = $"{carried:0.0} / {max:0} kg{water}";

        var ids = new List<string>();
        foreach (ItemStack stack in inventory.Player.Stacks)
            if (state.Accepts(stack.itemId) && !ids.Contains(stack.itemId))
                ids.Add(stack.itemId);
        ids.Sort((a, b) => Rank(a).CompareTo(Rank(b)) != 0 ? Rank(a).CompareTo(Rank(b))
                                                          : string.CompareOrdinal(NameOf(a), NameOf(b)));

        storeAllButton.interactable = target.AnythingToStore();
        if (ids.Count == 0)
        {
            UiKit.Height(UiKit.Text(packList, "Empty", $"Nothing you're carrying fits in here. {Holds(state.kind)}", 19, UiKit.Muted), 70f);
            return;
        }

        foreach (string itemId in ids)
        {
            int count = inventory.Player.Count(itemId);
            string unit = WoodManager.IsWater(itemId) ? $"{count} L" : $"×{count}";
            unit += MoistureOfPack(inventory, itemId);
            bool can = target.CanStoreOne(itemId, out string reason);
            MoveRow(packList, itemId, unit, can ? null : reason, "Store", target.MaxStore(itemId), WoodManager.IsWater(itemId), storeAmounts,
                    (amount, all) => StoreAndRefresh(itemId, all ? int.MaxValue : amount));
        }
    }

    // Right: the structure's capacity where it has one, then what's inside.
    void RefreshStructure(WoodPileState state, bool barrel)
    {
        int litres = WoodManager.Instance != null ? WoodManager.Instance.CapacityOf(state.kind) : 0;
        structureWeight.text = barrel ? $"{state.Total} / {litres} L" : state.IsEmpty ? "Empty" : $"{state.Total} stored   ·   no limit";

        takeAllButton.interactable = !state.IsEmpty;
        if (state.IsEmpty)
        {
            UiKit.Height(UiKit.Text(structureList, "Empty", "Nothing in here.", 20, UiKit.Muted), 50f);
            return;
        }

        foreach (KeyValuePair<string, int> pair in WoodPile.Contents(state))
        {
            if (pair.Value <= 0)
                continue;
            string itemId = pair.Key;
            string unit = barrel ? $"{pair.Value} L" : $"×{pair.Value}";
            unit += MoistureOfPile(state, itemId);
            bool can = target.CanTakeOne(itemId, out string reason);
            MoveRow(structureList, itemId, unit, can ? null : reason, barrel ? "Fill" : "Take", target.MaxTake(itemId), barrel, takeAmounts,
                    (amount, all) => TakeAndRefresh(itemId, all ? int.MaxValue : amount));
        }
    }

    // " · dry", " · wet" or " · 2 wet, 3 dry" after a row of wood (WoodMoisture); nothing for anything else.
    static string MoistureOfPack(InventoryManager inventory, string itemId)
    {
        if (!WoodMoisture.Applies(itemId))
            return "";
        int wet = 0, dry = 0;
        foreach ((int day, int quantity) in inventory.Player.Batches(itemId))
        {
            if (WoodMoisture.IsDry(day))
                dry += quantity;
            else
                wet += quantity;
        }
        return "  ·  " + WoodMoisture.Label(wet, dry);
    }

    static string MoistureOfPile(WoodPileState state, string itemId)
    {
        if (!WoodMoisture.Applies(itemId))
            return "";
        state.CountMoisture(itemId, WoodMoisture.Today, WoodMoisture.SeasoningDays, out int wet, out int dry);
        return "  ·  " + WoodMoisture.Label(wet, dry);
    }

    static string NameOf(string itemId) => ItemDatabase.Get(itemId)?.DisplayName ?? itemId;

    // Water by quality first, then everything else by name.
    static int Rank(string itemId)
    {
        int i = System.Array.IndexOf(WaterOrder, itemId);
        return i < 0 ? WaterOrder.Length : i;
    }

    // What each structure holds, for when nothing carried fits.
    static string Holds(PileKind kind)
    {
        switch (kind)
        {
            case PileKind.RockStorage: return "A Rock Pile only holds Stone.";
            case PileKind.WaterBarrel: return "A Water Barrel only holds water.";
            case PileKind.RainBarrel: return "A Rain Barrel only holds water.";
            case PileKind.FoodCache: return "A Food Cache only holds food.";
            case PileKind.StorageBin: return "A Storage Bin holds Cordage, hides, furs, arrows and the like — not Tools, wood, stone, food or water.";
            case PileKind.ToolRack: return "A Tool Rack only holds Tools.";
            case PileKind.WoodShed: return "A Wood Shed only holds wood.";
            default: return "A Wood Pile only holds wood.";
        }
    }

    // One row: line one is the name and what is there (or why it can't move), line two the stepper, how much of the most that can move
    // is chosen, and the button. onMove gets the chosen amount and whether Shift was held (the old shortcut: the whole stack).
    static void MoveRow(RectTransform list, string itemId, string unit, string whyNot, string action, int max, bool litres,
                        Dictionary<string, int> amounts, System.Action<int, bool> onMove)
    {
        QuantityRowUi row = null;
        row = QuantityRowUi.Create(list, action, false, amounts.TryGetValue(itemId, out int chosen) ? chosen : 1, max,
                                   shift => onMove(row.Stepper.Amount, shift));
        string detail = whyNot != null ? $"{unit} — {whyNot}" : unit;
        row.Name.text = $"{NameOf(itemId)}  <size=16><color=#EDE3C799>{detail}</color></size>";
        row.SetActionEnabled(max > 0);

        void ShowTotal() => row.Total.text = max > 0 ? $"{row.Stepper.Amount} of {max}{(litres ? " L" : "")}" : "";
        row.Stepper.Changed += () =>
        {
            amounts[itemId] = row.Stepper.Amount;
            ShowTotal();
        };
        ShowTotal();
    }

    void StoreAndRefresh(string itemId, int count)
    {
        if (target == null)
            return;
        storeAmounts.Remove(itemId); // back to 1 once the move has happened
        int stored = target.Store(itemId, count);
        string name = NameOf(itemId);
        bool water = WoodManager.IsWater(itemId);
        statusLabel.text = stored > 0 ? (water ? $"Poured in {stored} L of {name}." : $"Stored {stored} {name}.")
                                      : (target.CanStoreOne(itemId, out string why) ? "Couldn't store that." : $"Can't store {name}: {why}.");
        if (stored > 0 && AudioManager.Instance != null && !water)
            AudioManager.Instance.Play(SoundCue.UiClick);
        Refresh();
    }

    void TakeAndRefresh(string itemId, int count)
    {
        if (target == null)
            return;
        takeAmounts.Remove(itemId); // back to 1 once the move has happened
        int taken = target.Take(itemId, count);
        statusLabel.text = taken > 0 ? $"Took {taken} {NameOf(itemId)}." : "No room to carry any more.";
        if (taken > 0 && AudioManager.Instance != null)
            AudioManager.Instance.Play(SoundCue.UiClick);
        Refresh();
    }

    void StoreAll()
    {
        if (target == null || target.State == null)
            return;
        statusLabel.text = target.StoreAllToStatus();
        if (AudioManager.Instance != null)
            AudioManager.Instance.Play(SoundCue.UiClick);
        Refresh();
    }

    // First click asks, second takes it down, returns the half of its materials, and closes the screen.
    void RemoveClicked()
    {
        if (target == null || target.State == null)
            return;
        if (!confirmingRemove)
        {
            confirmingRemove = true;
            removeButton.GetComponentInChildren<Text>().text = "Click again to take it down";
            statusLabel.text = "Needs the Hammer equipped and the structure empty. Gives back half its materials.";
            return;
        }

        string message = target.RemoveStructure();
        target = null;
        ToolStatus.Flash(message, 5f);
        GameScreens screens = GetComponentInParent<GameScreens>();
        if (screens != null)
            screens.Close();
    }

    void TakeShortcut()
    {
        if (target == null || target.State == null)
            return;
        statusLabel.text = target.State.IsWaterBarrel ? target.DrawWaterToStatus() : target.TakeAllToStatus();
        if (AudioManager.Instance != null)
            AudioManager.Instance.Play(SoundCue.UiClick);
        Refresh();
    }

    // Places an element a fixed distance from its parent's top, full width.
    static void Top(RectTransform rt, float top, float height)
    {
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.offsetMin = new Vector2(0f, -top - height);
        rt.offsetMax = new Vector2(0f, -top);
    }
}
