using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// The Potter screen (Pottery P1, Mike 2026-11-02): E at the Potter's Shed (with no dry piece waiting) opens it to choose what to shape. Its own window like the Workshop screen
// (own canvas, 1920x1080 scaler, sort order 150), built in code. The five pieces are listed on the left (name, shaping hours and drying days) with the stock button under them;
// the chosen piece is on the right (hours, drying days, shelf space), a count control for 1 to 4 pieces, what the job needs against what the shed holds, the total shaping time,
// why it cannot start when it cannot, the Start button, and under it the shelf: what is drying (with days left), what is ready, and the space used out of 12. Across the top:
// the shed's status (idle or shaping) and what the pack carries of Clay and Sand. While a job runs the list can still be browsed and the shed stocked, but Start is off.
// Keys: 1-5 choose, Up and Down move the choice (wrapping), Left and Right change the count, Enter starts, R stocks, Esc closes (the pause key). They are read from the new Input
// System's keyboard, one action per fresh press, and PotteryRules.Resolve turns each into its action. Esc and the Close button shut it, and so does any change out of the Menu
// state. The screen starts a job through PlacedStructure.PotteryStartJob and stocks through PlacedStructure.PotteryStockFromPack (the same method as the R key in the world),
// which work the gate out again on the shed as it is at the click. The Menu state stops the clock and the player, so the shed can only change by this screen's own buttons.
// It never dismantles and never collects (E in the world collects dry pieces first). Mike cannot read small text: nothing under 26, body 28.
public class PotteryScreen : MonoBehaviour
{
    // The text sizes in use (UI pixels at the 1920x1080 reference). A test builds the screen and checks every label against MinimumSize.
    public const int MinimumSize = 26;
    public const int TitleSize = 36, StatusSize = 28, PackSize = 28, ListNameSize = 32, ListDetailSize = 28, DetailNameSize = 40, BodySize = 28, StartSize = 36,
                     StockSize = 32, CloseSize = 28, HintSize = 28, CountSize = 36, StepSize = 48, ShelfSize = 28;
    // Row heights and the gap between rows (UI pixels), and the heights of the stock, count and Start buttons.
    public const float RowHeight = 96f, RowGap = 12f, StockButtonHeight = 88f, StepButtonSize = 80f, StartHeight = 96f;

    public const float PanelWidth = 1500f, PanelHeight = 1000f;
    const float ListX = 32f, ListY = 200f, ListWidth = 700f;
    const float DetailX = 772f, DetailWidth = 696f;
    public const int ShelfLineCount = 3;

    static PotteryScreen instance;
    static int lastChoice;
    static int lastCount = PotteryRules.MinCount;

    public static bool IsOpen => instance != null && instance.open;

    // Opens the screen for a shed (E on a Potter's Shed with no dry piece waiting).
    public static void Open(PlacedStructure shed)
    {
        if (shed == null || GameManager.Instance == null)
            return;
        if (instance == null)
        {
            var go = new GameObject("Potter Screen");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<PotteryScreen>();
            instance.Build();
        }
        instance.Show(shed);
    }

    public static void Close()
    {
        if (instance != null)
            instance.Hide(true);
    }

    bool open, subscribed;
    PlacedStructure shed;
    int selected, count = PotteryRules.MinCount;
    string notice, stockNotice;
    bool noticeGood, stockNoticeGood;
    GameObject root;
    Text statusLabel, packLabel, detailName, detailLine, countLabel, needsHeading, totalLabel, reasonLabel, stockReasonLabel, shelfHeading;
    readonly Text[] needLabels = new Text[2];
    readonly Text[] shelfLabels = new Text[ShelfLineCount];
    readonly Button[] rows = new Button[PotteryRules.Recipes.Length];
    readonly Text[] rowNames = new Text[PotteryRules.Recipes.Length];
    readonly Text[] rowDetails = new Text[PotteryRules.Recipes.Length];
    Button startButton, stockButton, minusButton, plusButton;
    Text startLabel;

    // --- Layout ---

    void Build()
    {
        root = new GameObject("Root", typeof(RectTransform));
        root.transform.SetParent(transform, false);
        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 150;
        var scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        root.AddComponent<GraphicRaycaster>();

        var rootRect = (RectTransform)root.transform;
        Image dim = UiKit.Image(rootRect, "Dim", new Color(0f, 0f, 0f, 0.6f));
        dim.rectTransform.Fill();
        dim.raycastTarget = true;

        Color solid = UiKit.PanelColor;
        solid.a = 1f;
        Image panelImage = UiKit.Image(rootRect, "Panel", solid);
        panelImage.raycastTarget = true;
        RectTransform panel = panelImage.rectTransform;
        panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.sizeDelta = new Vector2(PanelWidth, PanelHeight);

        Text title = UiKit.Text(panel, "Title", "Potter's Shed", TitleSize, UiKit.Accent, TextAnchor.MiddleLeft, FontStyle.Italic);
        Place(title.rectTransform, 32f, 14f, 600f, 60f);
        Button close = UiKit.Button(panel, "Close", "Close", CloseSize, () => Hide(true));
        Place((RectTransform)close.transform, PanelWidth - 32f - 190f, 16f, 190f, 56f);

        Image rule = UiKit.Image(panel, "Rule", new Color(UiKit.Cream.r, UiKit.Cream.g, UiKit.Cream.b, 0.2f));
        Place(rule.rectTransform, 24f, 84f, PanelWidth - 48f, 2f);

        // Across the top: the shed's status, then what the pack carries.
        statusLabel = UiKit.Text(panel, "Status", "", StatusSize, UiKit.Cream, TextAnchor.MiddleLeft);
        Place(statusLabel.rectTransform, 32f, 94f, PanelWidth - 64f, 48f);
        packLabel = UiKit.Text(panel, "Pack", "", PackSize, UiKit.Cream, TextAnchor.MiddleLeft);
        Place(packLabel.rectTransform, 32f, 142f, PanelWidth - 64f, 48f);

        // The list: five rows, each a button holding the name and, under it, the shaping hours and the drying days.
        for (int i = 0; i < rows.Length; i++)
        {
            int index = i;
            Button row = UiKit.Button(panel, "Row " + i, "", ListNameSize, () => Choose(index));
            Place((RectTransform)row.transform, ListX, ListY + i * (RowHeight + RowGap), ListWidth, RowHeight);
            rowNames[i] = row.GetComponentInChildren<Text>(); // the button's own label becomes the name; the hours and days go in a second label under it
            rowNames[i].fontStyle = FontStyle.Bold;
            rowNames[i].alignment = TextAnchor.MiddleLeft;
            Place(rowNames[i].rectTransform, 18f, 4f, ListWidth - 36f, 44f);
            rowDetails[i] = UiKit.Text(row.transform, "Detail", "", ListDetailSize, UiKit.Cream, TextAnchor.MiddleLeft);
            Place(rowDetails[i].rectTransform, 18f, 48f, ListWidth - 36f, 42f);
            rows[i] = row;
        }

        // Under the list: why the stock button is off (or what it just did), then the button.
        float listEnd = ListY + rows.Length * RowHeight + (rows.Length - 1) * RowGap;
        stockReasonLabel = UiKit.Text(panel, "Stock Reason", "", BodySize, UiKit.Cream, TextAnchor.UpperLeft);
        Place(stockReasonLabel.rectTransform, ListX, listEnd + 12f, ListWidth, 72f);
        stockButton = UiKit.Button(panel, "Stock", PotteryRules.StockButtonText, StockSize, StockClicked);
        Place((RectTransform)stockButton.transform, ListX, listEnd + 12f + 72f + 8f, ListWidth, StockButtonHeight);
        stockButton.GetComponentInChildren<Text>().fontStyle = FontStyle.Bold;

        // The chosen piece.
        detailName = UiKit.Text(panel, "Piece", "", DetailNameSize, UiKit.Cream, TextAnchor.MiddleLeft, FontStyle.Bold);
        Place(detailName.rectTransform, DetailX, ListY, DetailWidth, 60f);
        detailLine = UiKit.Text(panel, "Piece Line", "", BodySize, UiKit.Cream, TextAnchor.MiddleLeft);
        detailLine.horizontalOverflow = HorizontalWrapMode.Overflow;
        Place(detailLine.rectTransform, DetailX, ListY + 62f, DetailWidth, 44f);

        // The count control: a large minus, the count in words, a large plus.
        minusButton = UiKit.Button(panel, "Count Minus", "-", StepSize, () => ChangeCount(-1));
        Place((RectTransform)minusButton.transform, DetailX, ListY + 114f, StepButtonSize, StepButtonSize);
        minusButton.GetComponentInChildren<Text>().fontStyle = FontStyle.Bold;
        countLabel = UiKit.Text(panel, "Count Label", "", CountSize, UiKit.Cream, TextAnchor.MiddleCenter, FontStyle.Bold);
        Place(countLabel.rectTransform, DetailX + StepButtonSize + 12f, ListY + 114f, 380f, StepButtonSize);
        plusButton = UiKit.Button(panel, "Count Plus", "+", StepSize, () => ChangeCount(1));
        Place((RectTransform)plusButton.transform, DetailX + StepButtonSize + 12f + 380f + 12f, ListY + 114f, StepButtonSize, StepButtonSize);
        plusButton.GetComponentInChildren<Text>().fontStyle = FontStyle.Bold;

        needsHeading = UiKit.Text(panel, "Needs Heading", "It needs", BodySize, UiKit.Accent, TextAnchor.MiddleLeft, FontStyle.Italic);
        Place(needsHeading.rectTransform, DetailX, ListY + 204f, DetailWidth, 44f);
        for (int i = 0; i < needLabels.Length; i++)
        {
            needLabels[i] = UiKit.Text(panel, "Need " + i, "", BodySize, UiKit.Cream, TextAnchor.MiddleLeft);
            needLabels[i].horizontalOverflow = HorizontalWrapMode.Overflow;
            Place(needLabels[i].rectTransform, DetailX, ListY + 248f + i * 44f, DetailWidth, 44f);
        }

        totalLabel = UiKit.Text(panel, "Total", "", BodySize, UiKit.Cream, TextAnchor.MiddleLeft);
        totalLabel.horizontalOverflow = HorizontalWrapMode.Overflow;
        Place(totalLabel.rectTransform, DetailX, ListY + 340f, DetailWidth, 44f);

        reasonLabel = UiKit.Text(panel, "Reason", "", BodySize, UiKit.Cream, TextAnchor.UpperLeft);
        Place(reasonLabel.rectTransform, DetailX, ListY + 388f, DetailWidth, 72f);

        startButton = UiKit.Button(panel, "Start", "", StartSize, StartClicked);
        Place((RectTransform)startButton.transform, DetailX, ListY + 468f, DetailWidth, StartHeight);
        startLabel = startButton.GetComponentInChildren<Text>();
        startLabel.fontStyle = FontStyle.Bold;

        // The shelf, under Start: the space used, then what is drying and what is ready.
        float shelfY = ListY + 468f + StartHeight + 12f;
        shelfHeading = UiKit.Text(panel, "Shelf Heading", "", ShelfSize, UiKit.Accent, TextAnchor.MiddleLeft, FontStyle.Italic);
        shelfHeading.horizontalOverflow = HorizontalWrapMode.Overflow;
        Place(shelfHeading.rectTransform, DetailX, shelfY, DetailWidth, 44f);
        for (int i = 0; i < shelfLabels.Length; i++)
        {
            shelfLabels[i] = UiKit.Text(panel, "Shelf " + i, "", ShelfSize, UiKit.Cream, TextAnchor.MiddleLeft);
            shelfLabels[i].horizontalOverflow = HorizontalWrapMode.Overflow;
            Place(shelfLabels[i].rectTransform, DetailX, shelfY + 44f + i * 40f, DetailWidth, 40f);
        }

        // One line of keys along the bottom.
        Text hint = UiKit.Text(panel, "Hint", PotteryRules.KeyHint, HintSize, UiKit.Cream, TextAnchor.MiddleLeft);
        hint.horizontalOverflow = HorizontalWrapMode.Overflow;
        Place(hint.rectTransform, 32f, PanelHeight - 54f, PanelWidth - 64f, 44f);

        root.SetActive(false);
    }

    // Places a child by its top-left corner within its parent, in pixels.
    static void Place(RectTransform rt, float x, float y, float width, float height)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
        rt.sizeDelta = new Vector2(width, height);
        rt.anchoredPosition = new Vector2(x, -y);
    }

    // --- Showing ---

    void Show(PlacedStructure target)
    {
        if (open || GameManager.Instance == null || !GameManager.Instance.OpenMenu())
            return;

        open = true;
        shed = target;
        selected = Mathf.Clamp(lastChoice, 0, PotteryRules.Recipes.Length - 1);
        count = PotteryRules.ClampCount(lastCount);
        notice = stockNotice = null;
        root.SetActive(true);
        Subscribe();
        Refresh();
        if (AudioManager.Instance != null)
            AudioManager.Instance.Play(SoundCue.UiClick);
    }

    void Hide(bool closeMenu)
    {
        if (!open)
            return;

        open = false;
        shed = null;
        root.SetActive(false);
        Unsubscribe();
        UiKit.ReleaseFocus();
        if (closeMenu && GameManager.Instance != null && GameManager.Instance.State == GameState.Menu)
            GameManager.Instance.CloseMenu();
        if (AudioManager.Instance != null)
            AudioManager.Instance.Play(SoundCue.UiBack);
    }

    void Subscribe()
    {
        if (subscribed || GameManager.Instance == null)
            return;
        subscribed = true;
        GameManager.Instance.StateChanged += OnStateChanged;
    }

    void Unsubscribe()
    {
        if (!subscribed)
            return;
        subscribed = false;
        if (GameManager.Instance != null)
            GameManager.Instance.StateChanged -= OnStateChanged;
    }

    // Esc (back to Playing), the main menu or a load: any way out of the Menu state closes the screen.
    void OnStateChanged(GameState state)
    {
        if (state != GameState.Menu)
            Hide(false);
    }

    void OnDestroy()
    {
        Unsubscribe();
        if (instance == this)
            instance = null;
    }

    // The shed is still there to work: the object lives and its pile is still in the world.
    bool ShedAlive()
    {
        if (shed == null || shed.State == null || WoodManager.Instance == null)
            return false;
        foreach (WoodPileState pile in WoodManager.Instance.Piles)
            if (pile == shed.State)
                return true;
        return false;
    }

    // --- Keys ---

    // Enter, the digits and the arrows are also other things elsewhere, but none of that runs in the Menu state: the player, the tools and the hotkeys are all gated on Playing.
    // A button the mouse clicked stays selected in the event system and would answer Enter as a click too, so the selection is cleared every frame: one press, one action.
    void Update()
    {
        if (!open)
            return;
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
            EventSystem.current.SetSelectedGameObject(null);

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;
        PotteryRules.KeyAction action = PotteryRules.Resolve(PressedKey(keyboard), selected, PotteryRules.Recipes.Length);
        switch (action.kind)
        {
            case PotteryRules.ActionKind.Choose:
                Choose(action.choice);
                break;
            case PotteryRules.ActionKind.Count:
                ChangeCount(action.delta);
                break;
            case PotteryRules.ActionKind.Start:
                StartClicked();
                break;
            case PotteryRules.ActionKind.Stock:
                StockClicked();
                break;
        }
    }

    // The key pressed this frame (a fresh press only, so holding a key does nothing more), as the rules' own names.
    static PotteryRules.Key PressedKey(Keyboard keyboard)
    {
        if (keyboard.digit1Key.wasPressedThisFrame) return PotteryRules.Key.One;
        if (keyboard.digit2Key.wasPressedThisFrame) return PotteryRules.Key.Two;
        if (keyboard.digit3Key.wasPressedThisFrame) return PotteryRules.Key.Three;
        if (keyboard.digit4Key.wasPressedThisFrame) return PotteryRules.Key.Four;
        if (keyboard.digit5Key.wasPressedThisFrame) return PotteryRules.Key.Five;
        if (keyboard.upArrowKey.wasPressedThisFrame) return PotteryRules.Key.Up;
        if (keyboard.downArrowKey.wasPressedThisFrame) return PotteryRules.Key.Down;
        if (keyboard.leftArrowKey.wasPressedThisFrame) return PotteryRules.Key.Left;
        if (keyboard.rightArrowKey.wasPressedThisFrame) return PotteryRules.Key.Right;
        if (keyboard.enterKey.wasPressedThisFrame) return PotteryRules.Key.Enter;
        if (keyboard.rKey.wasPressedThisFrame) return PotteryRules.Key.Stock;
        return PotteryRules.Key.None;
    }

    // --- Content ---

    void Choose(int index)
    {
        selected = index;
        lastChoice = index;
        notice = stockNotice = null;
        Refresh();
        if (AudioManager.Instance != null)
            AudioManager.Instance.Play(SoundCue.UiClick);
    }

    void ChangeCount(int delta)
    {
        int next = PotteryRules.ClampCount(count + delta);
        if (next == count)
            return;
        count = next;
        lastCount = next;
        notice = stockNotice = null;
        Refresh();
        if (AudioManager.Instance != null)
            AudioManager.Instance.Play(SoundCue.UiClick);
    }

    void StartClicked()
    {
        if (!ShedAlive())
        {
            Hide(true);
            return;
        }
        stockNotice = null;
        noticeGood = shed.PotteryStartJob(PotteryRules.Recipes[selected], count, out string message);
        notice = message; // "Started shaping..." or the reason it could not (a key press can ask when the button is off, or the shed may have changed)
        if (noticeGood && AudioManager.Instance != null)
            AudioManager.Instance.Play(SoundCue.UiClick);
        Refresh();
    }

    // The button and the R key: put what the pack carries into the shed, the one method the R key in the world uses too. When there is nothing to move it says why instead.
    void StockClicked()
    {
        if (!ShedAlive())
        {
            Hide(true);
            return;
        }
        notice = null;
        PotteryRules.StockGate gate = shed.PotteryStockGateNow();
        if (gate != PotteryRules.StockGate.Can)
        {
            stockNoticeGood = false;
            stockNotice = PotteryRules.StockReason(gate);
        }
        else
        {
            stockNoticeGood = shed.PotteryStockFromPack(out string message);
            stockNotice = message;
            if (stockNoticeGood && AudioManager.Instance != null)
                AudioManager.Instance.Play(SoundCue.UiClick);
        }
        Refresh();
    }

    void Refresh()
    {
        if (!open)
            return;
        if (!ShedAlive())
        {
            Hide(true);
            return;
        }

        WoodPileState state = shed.State;
        int clay = state.Count(PotteryRules.ClayId), sand = state.Count(PotteryRules.SandId);
        bool working = shed.PotteryRunning(out PotteryRules.Recipe made, out int madeCount);
        statusLabel.text = PotteryRules.StatusLine(working, made, madeCount, state.jobHours, clay, sand);
        statusLabel.color = working ? UiKit.Accent : UiKit.Cream;

        InventoryManager inventory = InventoryManager.Instance;
        int packClay = inventory != null ? inventory.Player.Count(PotteryRules.ClayId) : 0;
        int packSand = inventory != null ? inventory.Player.Count(PotteryRules.SandId) : 0;
        packLabel.text = PotteryRules.PackLine(packClay, packSand);

        for (int i = 0; i < rows.Length; i++)
        {
            PotteryRules.Recipe recipe = PotteryRules.Recipes[i];
            string[] lines = PotteryRules.ListLabel(recipe).Split('\n');
            rowNames[i].text = lines[0];
            rowDetails[i].text = lines[1];
            bool covered = clay >= recipe.clay && sand >= recipe.sand;
            rowNames[i].color = covered ? UiKit.Cream : UiKit.Muted;
            rowDetails[i].color = covered ? UiKit.Cream : UiKit.Muted;
            UiKit.SetSelected(rows[i], i == selected);
        }

        PotteryRules.Recipe chosen = PotteryRules.Recipes[selected];
        count = PotteryRules.ClampCount(count);
        detailName.text = chosen.name;
        detailLine.text = PotteryRules.DetailLine(chosen);
        countLabel.text = PotteryRules.CountLabel(chosen, count);
        minusButton.interactable = count > PotteryRules.MinCount;
        plusButton.interactable = count < PotteryRules.MaxCount;

        PotteryRules.NeedRow[] needs = PotteryRules.NeedRows(chosen, count, clay, sand);
        for (int i = 0; i < needLabels.Length; i++)
        {
            needLabels[i].text = needs[i].Text;
            needLabels[i].color = needs[i].enough ? UiKit.Cream : UiKit.Warning;
        }
        totalLabel.text = PotteryRules.TotalLine(chosen, count);

        var shelf = shed.PotteryShelf();
        int free = PotteryRules.FreeSpace(shelf);
        PotteryRules.Gate gate = shed.PotteryGateFor(chosen, count);
        startLabel.text = PotteryRules.StartButtonText(gate, chosen, count);
        startButton.interactable = gate == PotteryRules.Gate.Ok;
        string reason = PotteryRules.ReasonText(gate, chosen, count, clay, sand, free, state.jobHours, made, madeCount);
        reasonLabel.text = !string.IsNullOrEmpty(notice) ? notice : reason;
        reasonLabel.color = !string.IsNullOrEmpty(notice) && noticeGood ? UiKit.Accent : gate == PotteryRules.Gate.Ok ? UiKit.Cream : UiKit.Warning;

        shelfHeading.text = PotteryRules.SpaceLine(PotteryRules.SpaceUsed(shelf));
        string[] shelfLines = PotteryRules.ShelfLines(shelf, WellsAndOuthouses.PotteryNow(), ShelfLineCount);
        for (int i = 0; i < shelfLabels.Length; i++)
        {
            shelfLabels[i].gameObject.SetActive(i < shelfLines.Length);
            if (i < shelfLines.Length)
                shelfLabels[i].text = shelfLines[i];
        }

        // The stock button: on when the pack has something the shed has room for; otherwise greyed with the reason. A message from the last click shows until something else is done.
        PotteryRules.StockGate stock = shed.PotteryStockGateNow();
        stockButton.interactable = stock == PotteryRules.StockGate.Can;
        stockReasonLabel.text = !string.IsNullOrEmpty(stockNotice) ? stockNotice : PotteryRules.StockReason(stock);
        stockReasonLabel.color = !string.IsNullOrEmpty(stockNotice) && stockNoticeGood ? UiKit.Accent : UiKit.Warning;
    }
}
