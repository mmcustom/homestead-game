using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Save_System.md's Save Slots: the one slot panel, built in code in the game's UI look (like the main menu's difficulty
// panel: an overlay with its own Canvas, sorted above everything). Two modes:
//   Browse  - from the pause menu (inGame) or the main menu's Load Game: every slot at once, the Autosave on top. A filled slot
//             has Load; in game every manual slot also has Save (the Autosave never does). No Save-or-Load pick first.
//   NewGame - from the main menu after the difficulty step: pick the manual slot the new game will use.
// Rows: slot number, name (with a Current badge on the last slot saved to or loaded from), in-game date and time, difficulty,
// when it was saved (or "Empty"), then Save/Load/Select, Copy (then pick the target slot) and Delete. Overwrites and deletes ask
// first, except saving over the current slot or an empty one (SlotRules). Esc or Back closes (or backs out of a copy or a
// confirmation). Load hands the slot to GameManager; Save and New Game report the chosen slot to the caller.
public class SlotPanel : MonoBehaviour
{
    public enum Mode { Browse, NewGame }
    enum View { Browse, CopyTarget }

    const float PanelWidth = 1500f;
    const float RowHeight = 52f;
    const float RowGap = 4f;
    static readonly Color CurrentTint = new Color(0.3f, 0.38f, 0.22f, 0.6f);
    static readonly Color SelectedTint = new Color(0.45f, 0.4f, 0.25f, 0.55f);
    const float HeaderHeight = 120f;
    const float FooterHeight = 76f;
    const int RowCount = SaveManager.ManualSlotCount + 1; // the manual slots and the Autosave
    const float InnerWidth = PanelWidth - 64f;

    // Row columns (x from the row's left edge, 21 px text). Widths hold the longest values with room to spare: "Autumn 28, Year 10 · 23:59"
    // is 262 px, "Homesteader" 129, "Oct 20, 10:25 AM" 167, "[CURRENT GAME]" at 18 px about 166. The badge has its own column so it
    // can't run into the date. The four buttons start at ButtonsLeft and nothing else reaches past ColSaved + SavedWidth.
    const float ColSlot = 12f, ColName = 52f, NameWidth = 140f;
    const float ColBadge = 196f, BadgeWidth = 176f;
    const float ColInGame = 378f, InGameWidth = 282f;
    const float ColDifficulty = 666f, DifficultyWidth = 145f;
    const float ColSaved = 816f, SavedWidth = 174f;
    const float ButtonWidth = 90f, ButtonGap = 6f, ButtonMargin = 8f;
    static float ButtonX(int fromRight) => InnerWidth - ButtonMargin - (fromRight + 1) * ButtonWidth - fromRight * ButtonGap;

    static bool open;
    static int closedFrame = -1;

    // True while the panel is up, and for the rest of the frame it closes in, so the Esc that closed it doesn't also
    // reach the pause menu's own Esc handling.
    public static bool IsOpen => open || Time.frameCount == closedFrame;

    Mode mode;
    View view = View.Browse;
    bool inGame, confirming;
    string copySource, selectedId, current;
    Button selectedButton;
    Action<string> onPick;
    Action onClosed;

    RectTransform panel, rowsHost;
    Text titleLabel, messageLabel;
    GameObject confirmLayer;
    Action confirmYes;

    // underCanvas is anything inside the canvas to cover. onPick receives the slot for Save and NewGame; onClosed runs when the
    // player backs out without choosing. inGame (the pause menu) offers Save, marks the current slot, and asks before loading over
    // a running game.
    public static SlotPanel Open(Transform underCanvas, Mode mode, bool inGame, Action<string> onPick, Action onClosed)
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
        slotPanel.inGame = inGame;
        slotPanel.SelectInitial();
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

        string[] heads = { "Slot", "Name", "In game", "Difficulty", "Saved" };
        float[] xs = { 32f + ColSlot, 32f + ColName, 32f + ColInGame, 32f + ColDifficulty, 32f + ColSaved };
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

    // The row to start on (see SlotRules.PreSelect). In game: the current slot, else the first empty manual slot, else nothing.
    // New Game: the first empty slot. Main menu Load: the current slot, else the Autosave if it holds a save.
    void SelectInitial()
    {
        SaveManager saves = SaveManager.Instance;
        current = saves.CurrentSlot;
        var exists = new bool[SaveManager.ManualSlotCount];
        for (int i = 0; i < exists.Length; i++)
            exists[i] = saves.SlotExists(SaveManager.SlotId(i + 1));

        if (mode == Mode.NewGame)
            selectedId = IdFor(SlotRules.PreSelect(null, exists));
        else if (inGame)
            selectedId = IdFor(SlotRules.PreSelect(current, exists));
        else
            selectedId = current ?? (saves.SlotExists(SaveManager.AutosaveId) ? SaveManager.AutosaveId : null);
    }

    static string IdFor(int number) => number > 0 ? SaveManager.SlotId(number) : null;

    void Refresh()
    {
        SaveManager saves = SaveManager.Instance;
        if (saves == null)
            return;

        UiKit.Clear(rowsHost);
        selectedButton = null;
        current = saves.CurrentSlot;
        bool copying = view == View.CopyTarget;

        titleLabel.text = copying ? $"Copy {saves.GetSlotInfo(copySource).Name} to which slot?"
                        : mode == Mode.NewGame ? "Choose a slot for your new game"
                        : inGame ? "Save / Load" : "Load Game";

        float y = 0f;
        // The Autosave sits on top: a place to load from or copy out of, never a save target.
        if (mode == Mode.Browse && !copying || copying && copySource == SaveManager.AutosaveId)
        {
            BuildRow(saves.GetSlotInfo(SaveManager.AutosaveId), y, copying);
            y += RowHeight + RowGap;
        }
        for (int number = 1; number <= SaveManager.ManualSlotCount; number++)
        {
            BuildRow(saves.GetSlotInfo(SaveManager.SlotId(number)), y, copying);
            y += RowHeight + RowGap;
        }

        if (selectedButton != null && UnityEngine.EventSystems.EventSystem.current != null)
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(selectedButton.gameObject);
    }

    void BuildRow(SaveManager.SlotInfo info, float y, bool copying)
    {
        bool isSource = copying && info.Id == copySource;
        bool isCurrent = inGame && (info.IsAutosave ? SlotRules.AutosaveIsCurrent(true, current) : info.Id == current);
        bool isSelected = !copying && info.Id == selectedId;
        Color tint = isSource ? new Color(0.3f, 0.36f, 0.24f, 0.5f) : isCurrent ? CurrentTint : isSelected ? SelectedTint : new Color(0f, 0f, 0f, 0.25f);
        Image row = UiKit.Image(rowsHost, info.Id + " Row", tint);
        Place(row.rectTransform, 0f, y, InnerWidth, RowHeight);

        Color nameColor = info.Exists ? UiKit.Cream : UiKit.Muted;
        string badge = isCurrent ? (info.IsAutosave ? "[CURRENT GAME]" : "[CURRENT]") : "";
        AddCell(row, info.IsAutosave ? "A" : SaveManager.SlotNumber(info.Id).ToString(), ColSlot, 36f, UiKit.Muted);
        Text nameCell = AddCell(row, info.Name, ColName, NameWidth, nameColor);
        nameCell.horizontalOverflow = HorizontalWrapMode.Wrap; // a long name wraps and clips inside its column
        nameCell.verticalOverflow = VerticalWrapMode.Truncate;
        if (badge.Length > 0)
            AddCell(row, badge, ColBadge, BadgeWidth, new Color(0.78f, 0.84f, 0.55f), 18);
        if (info.Exists)
        {
            AddCell(row, info.InGame, ColInGame, InGameWidth, UiKit.Cream);
            AddCell(row, info.Difficulty, ColDifficulty, DifficultyWidth, UiKit.Cream);
            AddCell(row, info.SavedAtLocal, ColSaved, SavedWidth, UiKit.Cream);
        }
        else
        {
            AddCell(row, "Empty", ColInGame, InGameWidth, UiKit.Muted);
        }

        // Buttons, right to left: Delete, Copy, Load (or Select), Save. All on this one row, inside the panel.
        if (copying)
        {
            if (!isSource && !info.IsAutosave)
                AddButton(row, "Copy here", InnerWidth - ButtonMargin - 150f, 150f, () => OnCopyTarget(info));
            return;
        }

        if (info.Exists && !info.IsAutosave)
            AddButton(row, "Delete", ButtonX(0), ButtonWidth, () => OnDelete(info));
        if (info.Exists && mode == Mode.Browse)
            AddButton(row, "Copy", ButtonX(1), ButtonWidth, () => OnCopy(info));

        Button primary = null;
        if (mode == Mode.NewGame)
        {
            primary = AddButton(row, "Select", ButtonX(2) - 20f, ButtonWidth + 20f, () => OnSelectForNewGame(info));
        }
        else
        {
            if (info.Exists)
                primary = AddButton(row, "Load", ButtonX(2), ButtonWidth, () => OnLoad(info));
            if (inGame && !info.IsAutosave)
                primary = AddButton(row, "Save", ButtonX(3), ButtonWidth, () => OnSave(info));
        }
        if (isSelected && primary != null)
            selectedButton = primary;
    }

    static Text AddCell(Image row, string text, float x, float width, Color color, int size = 21)
    {
        Text cell = UiKit.Text(row.transform, "Cell", text, size, color, TextAnchor.MiddleLeft);
        cell.horizontalOverflow = HorizontalWrapMode.Overflow;
        Place(cell.rectTransform, x, 0f, width, RowHeight);
        return cell;
    }

    static Button AddButton(Image row, string label, float x, float width, UnityEngine.Events.UnityAction click)
    {
        Button button = UiKit.Button(row.transform, label, label, 19, click);
        Place((RectTransform)button.transform, x, 6f, width, RowHeight - 12f);
        return button;
    }

    // --- Actions ---

    void OnLoad(SaveManager.SlotInfo info)
    {
        Play(SoundCue.UiClick);
        selectedId = info.Id;
        if (inGame)
            Confirm($"Load {info.Name}?\nUnsaved progress in the current game will be lost.", () => DoLoad(info.Id));
        else
            DoLoad(info.Id);
    }

    // Saving over the current slot or an empty one is one click; any other filled slot asks first, with its details.
    void OnSave(SaveManager.SlotInfo info)
    {
        Play(SoundCue.UiClick);
        selectedId = info.Id;
        ConfirmOverwriteThenPick(info);
    }

    void OnSelectForNewGame(SaveManager.SlotInfo info)
    {
        Play(SoundCue.UiClick);
        selectedId = info.Id;
        ConfirmOverwriteThenPick(info);
    }

    void ConfirmOverwriteThenPick(SaveManager.SlotInfo info)
    {
        if (SlotRules.NeedsOverwriteConfirm(info.Exists, info.Id, inGame ? current : null))
        {
            string details = info.InGame.Length > 0 ? $" ({info.InGame})" : "";
            Confirm($"Overwrite {info.Name}{details}?", () => DoPick(info.Id));
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
