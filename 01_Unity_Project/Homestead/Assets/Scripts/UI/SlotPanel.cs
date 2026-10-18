using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Save_System.md's Save Slots: the one slot panel, built in code in the game's UI look (like the main menu's difficulty
// panel: an overlay with its own Canvas, sorted above everything). Three modes:
//   Save    - from the pause menu: pick a slot to save to. The Autosave is never a manual save target, so it isn't listed.
//   Load    - from the pause menu or the main menu: pick any slot, the Autosave included.
//   NewGame - from the main menu after the difficulty step: pick the manual slot the new game will use.
// Rows: slot number, name, in-game date and time, difficulty, when it was saved (or "Empty"). Each row has Save/Load/Select,
// Copy (then pick the target slot) and Delete, every overwrite and delete behind a confirmation. Esc or Back closes
// (or backs out of a copy or a confirmation). Save and Load never write or load anything themselves except Load, which
// hands the slot to GameManager; Save and New Game report the chosen slot to the caller.
public class SlotPanel : MonoBehaviour
{
    public enum Mode { Save, Load, NewGame }
    enum View { Browse, CopyTarget }

    const float PanelWidth = 1500f;
    const float RowHeight = 52f;
    const float RowGap = 4f;
    const float HeaderHeight = 120f;
    const float FooterHeight = 76f;
    const int RowCount = SaveManager.ManualSlotCount + 1; // the manual slots and the Autosave
    const float InnerWidth = PanelWidth - 64f;

    static bool open;
    static int closedFrame = -1;

    // True while the panel is up, and for the rest of the frame it closes in, so the Esc that closed it doesn't also
    // reach the pause menu's own Esc handling.
    public static bool IsOpen => open || Time.frameCount == closedFrame;

    Mode mode;
    View view = View.Browse;
    bool allowModeSwitch, warnUnsavedOnLoad, confirming;
    string copySource;
    Action<string> onPick;
    Action onClosed;

    RectTransform panel, rowsHost;
    Text titleLabel, messageLabel;
    Button saveTab, loadTab;
    GameObject confirmLayer;
    Action confirmYes;

    // underCanvas is anything inside the canvas to cover. onPick receives the slot for Save and NewGame; onClosed runs when the
    // player backs out without choosing. allowModeSwitch shows the Save/Load tabs (the pause menu); warnUnsavedOnLoad asks
    // before loading over a running game.
    public static SlotPanel Open(Transform underCanvas, Mode mode, bool allowModeSwitch, bool warnUnsavedOnLoad,
                                 Action<string> onPick, Action onClosed)
    {
        if (open || SaveManager.Instance == null)
            return null;

        Canvas canvas = underCanvas.GetComponentInParent<Canvas>();
        Transform parent = canvas != null ? canvas.rootCanvas.transform : underCanvas;

        Image shade = UiKit.Image(parent, "Slot Panel", new Color(0f, 0f, 0f, 0.7f));
        shade.rectTransform.Fill();
        shade.raycastTarget = true;
        var overlay = shade.gameObject.AddComponent<Canvas>();
        overlay.overrideSorting = true;
        overlay.sortingOrder = 200;
        shade.gameObject.AddComponent<GraphicRaycaster>();

        var slotPanel = shade.gameObject.AddComponent<SlotPanel>();
        slotPanel.mode = mode;
        slotPanel.allowModeSwitch = allowModeSwitch;
        slotPanel.warnUnsavedOnLoad = warnUnsavedOnLoad;
        slotPanel.onPick = onPick;
        slotPanel.onClosed = onClosed;
        slotPanel.Build();
        slotPanel.Refresh();
        open = true;
        return slotPanel;
    }

    void OnDestroy()
    {
        if (open)
        {
            open = false;
            closedFrame = Time.frameCount;
        }
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame)
            return;

        if (confirming)
            DismissConfirm();
        else if (view == View.CopyTarget)
            CancelCopy();
        else
            Close(true);
    }

    // --- Layout ---

    void Build()
    {
        Color solid = UiKit.PanelColor;
        solid.a = 1f;
        Image panelImage = UiKit.Image(transform, "Panel", solid);
        panelImage.raycastTarget = true;
        panel = panelImage.rectTransform;
        panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.sizeDelta = new Vector2(PanelWidth, HeaderHeight + RowCount * (RowHeight + RowGap) + FooterHeight);

        titleLabel = UiKit.Text(panel, "Title", "", 34, UiKit.Accent, TextAnchor.MiddleLeft, FontStyle.Italic);
        Place(titleLabel.rectTransform, 32f, 14f, 800f, 54f);

        if (allowModeSwitch)
        {
            saveTab = UiKit.Button(panel, "Save Tab", "Save", 22, () => SwitchMode(Mode.Save));
            Place((RectTransform)saveTab.transform, PanelWidth - 32f - 2 * 150f - 10f, 18f, 150f, 46f);
            loadTab = UiKit.Button(panel, "Load Tab", "Load", 22, () => SwitchMode(Mode.Load));
            Place((RectTransform)loadTab.transform, PanelWidth - 32f - 150f, 18f, 150f, 46f);
        }

        string[] heads = { "Slot", "Name", "In game", "Difficulty", "Saved" };
        float[] xs = { 32f + 14f, 32f + 80f, 32f + 330f, 32f + 700f, 32f + 850f };
        for (int i = 0; i < heads.Length; i++)
            Place(UiKit.Text(panel, heads[i], heads[i], 18, UiKit.Muted, TextAnchor.MiddleLeft).rectTransform, xs[i], 82f, 300f, 30f);

        rowsHost = UiKit.Rect("Rows", panel);
        Place(rowsHost, 32f, HeaderHeight, InnerWidth, RowCount * (RowHeight + RowGap));

        float footerY = panel.sizeDelta.y - FooterHeight + 12f;
        Button back = UiKit.Button(panel, "Back", "Back", 24, () => { Play(SoundCue.UiBack); Close(true); });
        Place((RectTransform)back.transform, 32f, footerY, 200f, 52f);
        messageLabel = UiKit.Text(panel, "Message", "", 22, UiKit.Cream, TextAnchor.MiddleLeft);
        Place(messageLabel.rectTransform, 256f, footerY, PanelWidth - 288f, 52f);
    }

    // Places a child by its top-left corner within its parent, in pixels.
    static void Place(RectTransform rt, float x, float y, float width, float height)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
        rt.sizeDelta = new Vector2(width, height);
        rt.anchoredPosition = new Vector2(x, -y);
    }

    void Refresh()
    {
        SaveManager saves = SaveManager.Instance;
        if (saves == null)
            return;

        UiKit.Clear(rowsHost);
        bool copying = view == View.CopyTarget;

        titleLabel.text = copying ? $"Copy {saves.GetSlotInfo(copySource).Name} to which slot?"
                        : mode == Mode.Save ? "Save Game"
                        : mode == Mode.Load ? "Load Game"
                        : "Choose a slot for your new game";
        if (allowModeSwitch)
        {
            UiKit.SetSelected(saveTab, mode == Mode.Save);
            UiKit.SetSelected(loadTab, mode == Mode.Load);
        }

        float y = 0f;
        for (int number = 1; number <= SaveManager.ManualSlotCount; number++)
        {
            BuildRow(saves.GetSlotInfo(SaveManager.SlotId(number)), y, copying);
            y += RowHeight + RowGap;
        }

        // The Autosave is a place to load from or copy out of, never a save target.
        if (mode == Mode.Load && !copying || copying && copySource == SaveManager.AutosaveId)
            BuildRow(saves.GetSlotInfo(SaveManager.AutosaveId), y, copying);
    }

    void BuildRow(SaveManager.SlotInfo info, float y, bool copying)
    {
        SaveManager saves = SaveManager.Instance;
        bool isSource = copying && info.Id == copySource;
        Image row = UiKit.Image(rowsHost, info.Id + " Row", isSource ? new Color(0.3f, 0.36f, 0.24f, 0.5f) : new Color(0f, 0f, 0f, 0.25f));
        Place(row.rectTransform, 0f, y, InnerWidth, RowHeight);

        bool active = info.Id == saves.ActiveSlot;
        Color nameColor = info.Exists ? UiKit.Cream : UiKit.Muted;
        AddCell(row, info.IsAutosave ? "A" : SaveManager.SlotNumber(info.Id).ToString(), 14f, 60f, UiKit.Muted);
        AddCell(row, info.Name + (active ? "  <color=#C7D68C>• current</color>" : ""), 80f, 250f, nameColor);
        if (info.Exists)
        {
            AddCell(row, info.InGame, 330f, 370f, UiKit.Cream);
            AddCell(row, info.Difficulty, 700f, 150f, UiKit.Cream);
            AddCell(row, info.SavedAtLocal, 850f, 220f, UiKit.Cream);
        }
        else
        {
            AddCell(row, "Empty", 330f, 370f, UiKit.Muted);
        }

        // Buttons, right to left: the main action, Copy, Delete.
        float x = InnerWidth - 8f - 100f;
        if (copying)
        {
            if (!isSource && !info.IsAutosave)
                AddButton(row, "Copy here", InnerWidth - 8f - 150f, 150f, () => OnCopyTarget(info));
            return;
        }

        if (info.Exists && !info.IsAutosave)
            AddButton(row, "Delete", x, 100f, () => OnDelete(info));
        if (info.Exists && mode != Mode.NewGame)
            AddButton(row, "Copy", x - 108f, 100f, () => OnCopy(info));

        string primary = mode == Mode.Save ? "Save" : mode == Mode.Load ? "Load" : "Select";
        bool offered = !(mode != Mode.Load && info.IsAutosave) && (mode != Mode.Load || info.Exists);
        if (offered)
            AddButton(row, primary, x - 246f, 130f, () => OnPrimary(info));
    }

    static void AddCell(Image row, string text, float x, float width, Color color)
    {
        Text cell = UiKit.Text(row.transform, "Cell", text, 21, color, TextAnchor.MiddleLeft);
        cell.horizontalOverflow = HorizontalWrapMode.Overflow;
        Place(cell.rectTransform, x, 0f, width, RowHeight);
    }

    static Button AddButton(Image row, string label, float x, float width, UnityEngine.Events.UnityAction click)
    {
        Button button = UiKit.Button(row.transform, label, label, 19, click);
        Place((RectTransform)button.transform, x, 6f, width, RowHeight - 12f);
        return button;
    }

    // --- Actions ---

    void SwitchMode(Mode newMode)
    {
        if (mode == newMode)
            return;

        Play(SoundCue.UiClick);
        mode = newMode;
        view = View.Browse;
        messageLabel.text = "";
        Refresh();
    }

    void OnPrimary(SaveManager.SlotInfo info)
    {
        Play(SoundCue.UiClick);
        if (mode == Mode.Load)
        {
            if (warnUnsavedOnLoad)
                Confirm($"Load {info.Name}?\nUnsaved progress in the current game will be lost.", () => DoLoad(info.Id));
            else
                DoLoad(info.Id);
        }
        else if (info.Exists)
        {
            Confirm($"Overwrite {info.Name}?\nIts current save will be replaced.", () => DoPick(info.Id));
        }
        else
        {
            DoPick(info.Id);
        }
    }

    void DoPick(string slotId)
    {
        Action<string> pick = onPick;
        Close(false);
        pick?.Invoke(slotId);
    }

    void DoLoad(string slotId)
    {
        Close(false);
        if (GameManager.Instance != null)
            GameManager.Instance.LoadSlot(slotId);
    }

    void OnCopy(SaveManager.SlotInfo info)
    {
        Play(SoundCue.UiClick);
        copySource = info.Id;
        view = View.CopyTarget;
        messageLabel.text = "";
        Refresh();
    }

    void CancelCopy()
    {
        Play(SoundCue.UiBack);
        view = View.Browse;
        Refresh();
    }

    void OnCopyTarget(SaveManager.SlotInfo target)
    {
        Play(SoundCue.UiClick);
        string source = SaveManager.Instance.GetSlotInfo(copySource).Name;
        if (target.Exists)
            Confirm($"Overwrite {target.Name} with a copy of {source}?\nWhat is in {target.Name} now will be lost.", () => DoCopy(target));
        else
            DoCopy(target);
    }

    void DoCopy(SaveManager.SlotInfo target)
    {
        string sourceName = SaveManager.Instance.GetSlotInfo(copySource).Name;
        bool ok = SaveManager.Instance.CopySlot(copySource, target.Id, out string error);
        view = View.Browse;
        messageLabel.color = ok ? UiKit.Cream : UiKit.Warning;
        messageLabel.text = ok ? $"Copied {sourceName} to {target.Name}." : error;
        Refresh();
    }

    void OnDelete(SaveManager.SlotInfo info)
    {
        Play(SoundCue.UiClick);
        Confirm($"Delete {info.Name}?\nThis can't be undone.", () =>
        {
            bool ok = SaveManager.Instance.DeleteSave(info.Id);
            messageLabel.color = ok ? UiKit.Cream : UiKit.Warning;
            messageLabel.text = ok ? $"Deleted {info.Name}." : "Couldn't delete it. Details are in the log.";
            Refresh();
        });
    }

    // Back out: Esc, the Back button, or a pick/load that closes the panel (cancelled is false then).
    void Close(bool cancelled)
    {
        if (confirming)
            DismissConfirm();
        open = false;
        closedFrame = Time.frameCount;
        Action closed = onClosed;
        Destroy(gameObject);
        if (cancelled)
            closed?.Invoke();
    }

    // --- Confirmation ---

    void Confirm(string message, Action yes)
    {
        DismissConfirm();
        confirming = true;
        confirmYes = yes;

        Image dim = UiKit.Image(transform, "Confirm", new Color(0f, 0f, 0f, 0.6f));
        dim.rectTransform.Fill();
        dim.raycastTarget = true;
        confirmLayer = dim.gameObject;

        Color solid = UiKit.RaisedColor;
        solid.a = 1f;
        Image box = UiKit.Image(dim.rectTransform, "Box", solid);
        box.raycastTarget = true;
        RectTransform b = box.rectTransform;
        b.anchorMin = b.anchorMax = new Vector2(0.5f, 0.5f);
        b.sizeDelta = new Vector2(760f, 260f);

        Text text = UiKit.Text(b, "Message", message, 26, UiKit.Cream, TextAnchor.MiddleCenter);
        Place(text.rectTransform, 30f, 24f, 700f, 130f);
        Button yesButton = UiKit.Button(b, "Yes", "Yes", 24, () =>
        {
            Play(SoundCue.UiClick);
            Action yesAction = confirmYes;
            DismissConfirm();
            yesAction?.Invoke();
        });
        Place((RectTransform)yesButton.transform, 130f, 176f, 220f, 56f);
        Button noButton = UiKit.Button(b, "No", "No", 24, () => { Play(SoundCue.UiBack); DismissConfirm(); });
        Place((RectTransform)noButton.transform, 410f, 176f, 220f, 56f);
    }

    void DismissConfirm()
    {
        if (confirmLayer != null)
            Destroy(confirmLayer);
        confirmLayer = null;
        confirmYes = null;
        confirming = false;
    }

    static void Play(SoundCue cue)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.Play(cue);
    }
}
