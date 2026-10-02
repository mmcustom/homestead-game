using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Primitive_Storage_System.md's per-item transfer screen (2026-10-02 — Mike: "i think we need a per-item transfer
// screen now"). Opened by GameScreens.OpenStorage when the player presses E on a built storage pile — Wood Pile,
// Rock Pile, Water Barrel, Food Cache, Storage Bin or Tool Rack (WoodPile.UsesTransferScreen) — instead of the old
// instant take-all. One row per kind of thing stored; Take picks one, Shift-click takes the whole stack, same
// convention as Trading Post's Buy/Sell and Cooking's Shift-click. A shortcut button up top keeps the old one-press
// behavior available too — "Take Everything" normally, "Fill Bucket (best quality)" for the Water Barrel. Storing
// (R) stays bulk, untouched, out in the World — Mike only asked about picking what comes back out.
public class StorageTransferScreen : GameScreen
{
    RectTransform list;
    Text statusLabel;
    Button shortcutButton;
    WoodPile target;

    public override string Title => target != null && target.State != null ? WoodManager.PileName(target.State.kind) : "Storage";

    public override void Build(RectTransform area)
    {
        shortcutButton = UiKit.Button(area, "Shortcut", "", 18, TakeShortcut);
        RectTransform shortcutRect = (RectTransform)shortcutButton.transform;
        Top(shortcutRect, 0f, 38f, 0f, 0.3f);

        statusLabel = UiKit.Text(area, "Status", "", 17, UiKit.Muted, TextAnchor.MiddleRight);
        Top(statusLabel.rectTransform, 0f, 38f, 0.3f, 1f);

        list = UiKit.ScrollList(area, "List");
        ((RectTransform)list.parent).Fill(0f, 0f, 0f, 46f);
    }

    public void SetTarget(WoodPile pile) => target = pile;

    // No Player.Changed subscription: GameManager is in the Menu state while this is open, so nothing can change the
    // pile or the player's carry weight except this screen's own buttons — which call Refresh themselves after.
    public override void OnShow()
    {
        statusLabel.text = "Take picks one; Shift-click takes the whole stack.";
        Refresh();
    }

    void Refresh()
    {
        UiKit.Clear(list);
        if (target == null || target.State == null)
        {
            shortcutButton.gameObject.SetActive(false);
            return;
        }

        WoodPileState state = target.State;
        bool barrel = state.kind == PileKind.WaterBarrel;
        shortcutButton.gameObject.SetActive(true);
        shortcutButton.GetComponentInChildren<Text>().text = barrel ? "Fill Bucket (best quality)" : "Take Everything";

        if (state.IsEmpty)
        {
            shortcutButton.interactable = false;
            UiKit.Height(UiKit.Text(list, "Empty", "Nothing in here.", 20, UiKit.Muted), 50f);
            return;
        }
        shortcutButton.interactable = true;

        foreach (KeyValuePair<string, int> pair in WoodPile.Contents(state))
        {
            string itemId = pair.Key;
            int count = pair.Value;
            if (count <= 0)
                continue;
            ItemDefinition item = ItemDatabase.Get(itemId);
            string name = item != null ? item.DisplayName : itemId;
            string unit = barrel ? $"{count} L" : $"×{count}";
            bool canTake = target.CanTakeOne(itemId, out string reason);
            string label = canTake ? $"{name}  <size=16><color=#EDE3C799>{unit}</color></size>"
                                    : $"{name}  <size=16><color=#EDE3C799>{unit} — {reason}</color></size>";
            Row(label, barrel ? "Fill" : "Take", canTake, () => TakeAndRefresh(itemId));
        }
    }

    void Row(string label, string action, bool enabled, UnityEngine.Events.UnityAction click)
    {
        RectTransform row = UiKit.Image(list, "Row", UiKit.RaisedColor).rectTransform;
        UiKit.Height(row, 46f);
        Text name = UiKit.Text(row, "Name", label, 20, UiKit.Cream, TextAnchor.MiddleLeft);
        name.rectTransform.anchorMin = Vector2.zero;
        name.rectTransform.anchorMax = new Vector2(0.78f, 1f);
        name.rectTransform.offsetMin = new Vector2(12f, 0f);
        name.rectTransform.offsetMax = Vector2.zero;
        name.horizontalOverflow = HorizontalWrapMode.Overflow;
        Button button = UiKit.Button(row, action, action, 18, click);
        var rt = (RectTransform)button.transform;
        rt.anchorMin = new Vector2(0.8f, 0f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.offsetMin = new Vector2(0f, 6f);
        rt.offsetMax = new Vector2(-6f, -6f);
        button.interactable = enabled;
    }

    void TakeAndRefresh(string itemId)
    {
        if (target == null)
            return;
        int taken = target.TakeOne(itemId, ShiftHeld);
        string name = ItemDatabase.Get(itemId)?.DisplayName ?? itemId;
        statusLabel.text = taken > 0 ? $"Took {taken} {name}." : "No room to carry any more.";
        if (taken > 0 && AudioManager.Instance != null)
            AudioManager.Instance.Play(SoundCue.UiClick);
        Refresh();
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

    // Places an element a fixed distance from its parent's top, across part of its width.
    static void Top(RectTransform rt, float top, float height, float xMin, float xMax)
    {
        rt.anchorMin = new Vector2(xMin, 1f);
        rt.anchorMax = new Vector2(xMax, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.offsetMin = new Vector2(xMin == 0f ? 0f : 6f, -top - height);
        rt.offsetMax = new Vector2(xMax >= 1f ? 0f : -6f, -top);
    }
}
