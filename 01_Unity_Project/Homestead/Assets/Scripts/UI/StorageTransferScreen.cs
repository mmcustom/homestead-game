using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Primitive_Storage_System.md's transfer screen, two-way (2026-10-04 — Mike's playtest: "i think we need to have an
// individual inventory screen for each structure that we can put items in individually and remove individually. maybe
// when working with one of the structures open 2 seperate inventory windows"). Opened by GameScreens.OpenStorage when
// the player presses E or R on a built storage pile — Wood Pile, Rock Pile, Water Barrel, Food Cache, Storage Bin or
// Tool Rack (WoodPile.UsesTransferScreen) — targeting that one pile. Two panels in the one window, so the other tabs
// stay reachable: the player's pack on the left (only what this structure holds), the structure's contents on the
// right, each with its weight or capacity. Click moves one across, Shift-click the whole stack, same convention as
// Cooking's Split and Trading Post's Buy/Sell. A row that can't move stays, greyed, with the reason. The Water Barrel
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
        statusLabel.text = "Click moves one; Shift-click moves the whole stack.";
        Refresh();
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
        bool barrel = state.kind == PileKind.WaterBarrel;
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
            bool can = target.CanStoreOne(itemId, out string reason);
            Row(packList, NameOf(itemId), unit, can ? null : reason, "Store", can, () => StoreAndRefresh(itemId));
        }
    }

    // Right: the structure's capacity where it has one, then what's inside.
    void RefreshStructure(WoodPileState state, bool barrel)
    {
        int litres = WoodManager.Instance != null ? WoodManager.Instance.BarrelLitres : 0;
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
            bool can = target.CanTakeOne(itemId, out string reason);
            Row(structureList, NameOf(itemId), unit, can ? null : reason, barrel ? "Fill" : "Take", can, () => TakeAndRefresh(itemId));
        }
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
            case PileKind.FoodCache: return "A Food Cache only holds food.";
            case PileKind.StorageBin: return "A Storage Bin holds Cordage, hides, furs, arrows and the like — not Tools, wood, stone, food or water.";
            case PileKind.ToolRack: return "A Tool Rack only holds Tools.";
            default: return "A Wood Pile only holds wood.";
        }
    }

    void Row(RectTransform list, string name, string unit, string whyNot, string action, bool enabled, UnityEngine.Events.UnityAction click)
    {
        RectTransform row = UiKit.Image(list, "Row", UiKit.RaisedColor).rectTransform;
        UiKit.Height(row, 46f);
        string detail = whyNot != null ? $"{unit} — {whyNot}" : unit;
        Text label = UiKit.Text(row, "Name", $"{name}  <size=16><color=#EDE3C799>{detail}</color></size>", 20, UiKit.Cream, TextAnchor.MiddleLeft);
        label.rectTransform.anchorMin = Vector2.zero;
        label.rectTransform.anchorMax = new Vector2(0.74f, 1f);
        label.rectTransform.offsetMin = new Vector2(12f, 0f);
        label.rectTransform.offsetMax = Vector2.zero;
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        Button button = UiKit.Button(row, action, action, 18, click);
        var rt = (RectTransform)button.transform;
        rt.anchorMin = new Vector2(0.76f, 0f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.offsetMin = new Vector2(0f, 6f);
        rt.offsetMax = new Vector2(-6f, -6f);
        button.interactable = enabled;
    }

    void StoreAndRefresh(string itemId)
    {
        if (target == null)
            return;
        int stored = target.StoreOne(itemId, ShiftHeld);
        string name = NameOf(itemId);
        bool water = WoodManager.IsWater(itemId);
        statusLabel.text = stored > 0 ? (water ? $"Poured in {stored} L of {name}." : $"Stored {stored} {name}.")
                                      : (target.CanStoreOne(itemId, out string why) ? "Couldn't store that." : $"Can't store {name}: {why}.");
        if (stored > 0 && AudioManager.Instance != null && !water)
            AudioManager.Instance.Play(SoundCue.UiClick);
        Refresh();
    }

    void TakeAndRefresh(string itemId)
    {
        if (target == null)
            return;
        int taken = target.TakeOne(itemId, ShiftHeld);
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
            statusLabel.text = "Gives back half its building materials. It has to be empty first.";
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
        statusLabel.text = target.State.kind == PileKind.WaterBarrel ? target.DrawWaterToStatus() : target.TakeAllToStatus();
        if (AudioManager.Instance != null)
            AudioManager.Instance.Play(SoundCue.UiClick);
        Refresh();
    }

    static bool ShiftHeld =>
        UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.shiftKey.isPressed;

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
