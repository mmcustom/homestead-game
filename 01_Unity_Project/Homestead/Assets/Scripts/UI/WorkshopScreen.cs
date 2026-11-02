using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// The Workshop screen (Workshop phases a2-1 and a2-2, Mike 2026-11-01): E at the bench opens it to choose what to make. Its own window like the Finances screen (own canvas,
// 1920x1080 scaler, sort order 150), not a tab of the shared game window. The four pieces are listed on the left (name, hours, what the Trading Post pays now) and, under them,
// the stock button; the chosen piece's details are on the right (hours, weight, base price, what it needs against what the bench holds in plain words, and why it cannot start
// when it cannot) with a Start button below. Across the top: the bench's status (idle, a piece being made, a piece ready) and what the pack carries of the three materials.
// While a piece is being made the list can still be browsed and the bench stocked, but Start is off.
// Mouse and keyboard (a2-2): 1-4 choose, Up and Down move the choice, Enter starts, R stocks the bench, Esc closes (the pause key, as for the other screens). The keys are read
// from the new Input System's keyboard, one action per fresh press, and WorkshopRules.Resolve turns each into its action. Esc and the Close button shut it, and so does any change
// out of the Menu state (a load, the main menu). The screen holds the bench it was opened for and starts a job through PlacedStructure.TryStartJob and stocks it through
// PlacedStructure.StockFromPack (the same method as the R key in the world), which work the gate out again on the bench as it is at the click. The Menu state stops the clock
// and the player, so the bench can only change by this screen's own buttons. It never dismantles. Text is large because Mike cannot read small text: nothing under 26.
public class WorkshopScreen : MonoBehaviour
{
    // The text sizes in use (UI pixels at the 1920x1080 reference). A test builds the screen and checks every label against MinimumSize.
    public const int MinimumSize = 26;
    public const int TitleSize = 36, StatusSize = 28, PackSize = 28, ListNameSize = 32, ListDetailSize = 28, DetailNameSize = 40, BodySize = 28, StartSize = 36,
                     StockSize = 32, CloseSize = 28, HintSize = 28;
    // Row heights and the gap between rows (UI pixels), and the stock button's height.
    public const float RowHeight = 104f, RowGap = 12f, StockButtonHeight = 88f;

    const float PanelWidth = 1500f, PanelHeight = 920f;
    const float ListX = 32f, ListY = 204f, ListWidth = 700f;
    const float DetailX = 772f, DetailWidth = 696f;

    static WorkshopScreen instance;
    static int lastChoice;

    public static bool IsOpen => instance != null && instance.open;

    // Opens the screen for a bench (E on a Workshop with no finished piece waiting).
    public static void Open(PlacedStructure bench)
    {
        if (bench == null || GameManager.Instance == null)
            return;
        if (instance == null)
        {
            var go = new GameObject("Workshop Screen");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<WorkshopScreen>();
            instance.Build();
        }
        instance.Show(bench);
    }

    public static void Close()
    {
        if (instance != null)
            instance.Hide(true);
    }

    bool open, subscribed;
    PlacedStructure bench;
    int selected;
    string notice, stockNotice;
    bool noticeGood, stockNoticeGood;
    GameObject root;
    Text statusLabel, packLabel, detailName, detailLine, needsHeading, reasonLabel, stockReasonLabel;
    readonly Text[] needLabels = new Text[3];
    readonly Button[] rows = new Button[WorkshopRules.Recipes.Length];
    readonly Text[] rowNames = new Text[WorkshopRules.Recipes.Length];
    readonly Text[] rowDetails = new Text[WorkshopRules.Recipes.Length];
    Button startButton, stockButton;
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

        Text title = UiKit.Text(panel, "Title", "Workshop", TitleSize, UiKit.Accent, TextAnchor.MiddleLeft, FontStyle.Italic);
        Place(title.rectTransform, 32f, 14f, 600f, 60f);
        Button close = UiKit.Button(panel, "Close", "Close", CloseSize, () => Hide(true));
        Place((RectTransform)close.transform, PanelWidth - 32f - 190f, 16f, 190f, 56f);

        Image rule = UiKit.Image(panel, "Rule", new Color(UiKit.Cream.r, UiKit.Cream.g, UiKit.Cream.b, 0.2f));
        Place(rule.rectTransform, 24f, 84f, PanelWidth - 48f, 2f);

        // Across the top: the bench's status, then what the pack carries.
        statusLabel = UiKit.Text(panel, "Status", "", StatusSize, UiKit.Cream, TextAnchor.MiddleLeft);
        Place(statusLabel.rectTransform, 32f, 94f, PanelWidth - 64f, 48f);
        packLabel = UiKit.Text(panel, "Pack", "", PackSize, UiKit.Cream, TextAnchor.MiddleLeft);
        Place(packLabel.rectTransform, 32f, 142f, PanelWidth - 64f, 48f);

        // The list: four rows, each a button holding the name and, under it, the hours and the price.
        for (int i = 0; i < rows.Length; i++)
        {
            int index = i;
            Button row = UiKit.Button(panel, "Row " + i, "", ListNameSize, () => Choose(index));
            Place((RectTransform)row.transform, ListX, ListY + i * (RowHeight + RowGap), ListWidth, RowHeight);
            rowNames[i] = row.GetComponentInChildren<Text>(); // the button's own label becomes the name; the hours and price go in a second label under it
            rowNames[i].fontStyle = FontStyle.Bold;
            rowNames[i].alignment = TextAnchor.MiddleLeft;
            Place(rowNames[i].rectTransform, 18f, 6f, ListWidth - 36f, 46f);
            rowDetails[i] = UiKit.Text(row.transform, "Detail", "", ListDetailSize, UiKit.Cream, TextAnchor.MiddleLeft);
            Place(rowDetails[i].rectTransform, 18f, 52f, ListWidth - 36f, 44f);
            rows[i] = row;
        }

        // Under the list: why the stock button is off (or what it just did), then the button.
        float listEnd = ListY + rows.Length * RowHeight + (rows.Length - 1) * RowGap;
        stockReasonLabel = UiKit.Text(panel, "Stock Reason", "", BodySize, UiKit.Cream, TextAnchor.UpperLeft);
        Place(stockReasonLabel.rectTransform, ListX, listEnd + 12f, ListWidth, 72f);
        stockButton = UiKit.Button(panel, "Stock", WorkshopRules.StockButtonText, StockSize, StockClicked);
        Place((RectTransform)stockButton.transform, ListX, listEnd + 12f + 72f + 8f, ListWidth, StockButtonHeight);
        stockButton.GetComponentInChildren<Text>().fontStyle = FontStyle.Bold;

        // The chosen piece.
        detailName = UiKit.Text(panel, "Piece", "", DetailNameSize, UiKit.Cream, TextAnchor.MiddleLeft, FontStyle.Bold);
        Place(detailName.rectTransform, DetailX, ListY, DetailWidth, 60f);
        detailLine = UiKit.Text(panel, "Piece Line", "", BodySize, UiKit.Cream, TextAnchor.MiddleLeft);
        Place(detailLine.rectTransform, DetailX, ListY + 66f, DetailWidth, 44f);
        needsHeading = UiKit.Text(panel, "Needs Heading", "It needs", BodySize, UiKit.Accent, TextAnchor.MiddleLeft, FontStyle.Italic);
        Place(needsHeading.rectTransform, DetailX, ListY + 128f, DetailWidth, 44f);
        for (int i = 0; i < needLabels.Length; i++)
        {
            needLabels[i] = UiKit.Text(panel, "Need " + i, "", BodySize, UiKit.Cream, TextAnchor.MiddleLeft);
            needLabels[i].horizontalOverflow = HorizontalWrapMode.Overflow;
            Place(needLabels[i].rectTransform, DetailX, ListY + 176f + i * 48f, DetailWidth, 44f);
        }

        reasonLabel = UiKit.Text(panel, "Reason", "", BodySize, UiKit.Cream, TextAnchor.UpperLeft);
        Place(reasonLabel.rectTransform, DetailX, ListY + 330f, DetailWidth, 72f);

        startButton = UiKit.Button(panel, "Start", "", StartSize, StartClicked);
        Place((RectTransform)startButton.transform, DetailX, ListY + 414f, DetailWidth, 104f);
        startLabel = startButton.GetComponentInChildren<Text>();
        startLabel.fontStyle = FontStyle.Bold;

        // One line of keys along the bottom.
        Text hint = UiKit.Text(panel, "Hint", WorkshopRules.KeyHint, HintSize, UiKit.Cream, TextAnchor.MiddleLeft);
        hint.horizontalOverflow = HorizontalWrapMode.Overflow;
        Place(hint.rectTransform, 32f, PanelHeight - 60f, PanelWidth - 64f, 44f);

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
        bench = target;
        selected = Mathf.Clamp(lastChoice, 0, WorkshopRules.Recipes.Length - 1);
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
        bench = null;
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

    // The bench is still there to work: the object lives and its pile is still in the world.
    bool BenchAlive()
    {
        if (bench == null || bench.State == null || WoodManager.Instance == null)
            return false;
        foreach (WoodPileState pile in WoodManager.Instance.Piles)
            if (pile == bench.State)
                return true;
        return false;
    }

    // --- Keys ---

    // Enter and the digits are also other things elsewhere (Enter is the Attack binding with the left mouse button, the digits pick the tool hotkeys, R is the world's second
    // interaction), but none of that runs in the Menu state: the player, the tools and the hotkeys are all gated on Playing. A button the mouse clicked stays selected in the
    // event system and would answer Enter as a click too, so the selection is cleared every frame: one press, one action.
    void Update()
    {
        if (!open)
            return;
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
            EventSystem.current.SetSelectedGameObject(null);

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;
        WorkshopRules.KeyAction action = WorkshopRules.Resolve(PressedKey(keyboard), selected, WorkshopRules.Recipes.Length);
        switch (action.kind)
        {
            case WorkshopRules.ActionKind.Choose:
                Choose(action.choice);
                break;
            case WorkshopRules.ActionKind.Start:
                StartClicked();
                break;
            case WorkshopRules.ActionKind.Stock:
                StockClicked();
                break;
        }
    }

    // The key pressed this frame (a fresh press only, so holding a key does nothing more), as the rules' own names.
    static WorkshopRules.Key PressedKey(Keyboard keyboard)
    {
        if (keyboard.digit1Key.wasPressedThisFrame) return WorkshopRules.Key.One;
        if (keyboard.digit2Key.wasPressedThisFrame) return WorkshopRules.Key.Two;
        if (keyboard.digit3Key.wasPressedThisFrame) return WorkshopRules.Key.Three;
        if (keyboard.digit4Key.wasPressedThisFrame) return WorkshopRules.Key.Four;
        if (keyboard.upArrowKey.wasPressedThisFrame) return WorkshopRules.Key.Up;
        if (keyboard.downArrowKey.wasPressedThisFrame) return WorkshopRules.Key.Down;
        if (keyboard.enterKey.wasPressedThisFrame) return WorkshopRules.Key.Enter;
        if (keyboard.rKey.wasPressedThisFrame) return WorkshopRules.Key.Stock;
        return WorkshopRules.Key.None;
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

    void StartClicked()
    {
        if (!BenchAlive())
        {
            Hide(true);
            return;
        }
        stockNotice = null;
        noticeGood = bench.TryStartJob(WorkshopRules.Recipes[selected], out string message);
        notice = message; // "Started a Chair..." or the reason it could not (a key press can ask when the button is off, or the bench may have changed)
        if (noticeGood && AudioManager.Instance != null)
            AudioManager.Instance.Play(SoundCue.UiClick);
        Refresh();
    }

    // The button and the R key: put what the pack carries into the bench, the one method the R key in the world uses too. When there is nothing to move it says why instead.
    void StockClicked()
    {
        if (!BenchAlive())
        {
            Hide(true);
            return;
        }
        notice = null;
        WorkshopRules.StockGate gate = bench.StockGateNow();
        if (gate != WorkshopRules.StockGate.Can)
        {
            stockNoticeGood = false;
            stockNotice = WorkshopRules.StockReason(gate);
        }
        else
        {
            stockNoticeGood = bench.StockFromPack(out string message);
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
        if (!BenchAlive())
        {
            Hide(true);
            return;
        }

        WoodPileState state = bench.State;
        int logs = state.Count(WorkshopRules.LogsId), branches = state.Count(WorkshopRules.BranchesId), cordage = state.Count(WorkshopRules.CordageId);
        bool working = bench.WorkshopRunning(out WorkshopRules.Recipe made);
        bool waiting = bench.WorkshopWaiting(out WorkshopRules.Recipe waitingPiece);
        statusLabel.text = WorkshopRules.BenchLine(working, made, state.jobHours, waiting, waitingPiece, logs, branches, cordage);
        statusLabel.color = working || waiting ? UiKit.Accent : UiKit.Cream;

        // What the pack carries, all of a kind together whatever its wet or dry day.
        InventoryManager inventory = InventoryManager.Instance;
        int packLogs = inventory != null ? inventory.Player.Count(WorkshopRules.LogsId) : 0;
        int packBranches = inventory != null ? inventory.Player.Count(WorkshopRules.BranchesId) : 0;
        int packCordage = inventory != null ? inventory.Player.Count(WorkshopRules.CordageId) : 0;
        packLabel.text = WorkshopRules.PackLine(packLogs, packBranches, packCordage);

        for (int i = 0; i < rows.Length; i++)
        {
            WorkshopRules.Recipe recipe = WorkshopRules.Recipes[i];
            long price = MarketManager.Instance != null ? MarketManager.Instance.SellPrice(recipe.itemId) : 0;
            string[] lines = WorkshopRules.ListLabel(recipe, price).Split('\n');
            rowNames[i].text = lines[0];
            rowDetails[i].text = lines[1];
            bool covered = WorkshopRules.Covers(recipe, logs, branches, cordage);
            rowNames[i].color = covered ? UiKit.Cream : UiKit.Muted;
            rowDetails[i].color = covered ? UiKit.Cream : UiKit.Muted;
            UiKit.SetSelected(rows[i], i == selected);
        }

        WorkshopRules.Recipe chosen = WorkshopRules.Recipes[selected];
        detailName.text = chosen.name;
        detailLine.text = WorkshopRules.DetailLine(chosen);
        WorkshopRules.NeedRow[] needs = WorkshopRules.NeedRows(chosen, logs, branches, cordage);
        for (int i = 0; i < needLabels.Length; i++)
        {
            needLabels[i].gameObject.SetActive(i < needs.Length);
            if (i < needs.Length)
            {
                needLabels[i].text = needs[i].Text;
                needLabels[i].color = needs[i].enough ? UiKit.Cream : UiKit.Warning;
            }
        }

        WorkshopRules.Gate gate = bench.WorkshopGateFor(chosen);
        startLabel.text = WorkshopRules.StartButtonText(gate, chosen);
        startButton.interactable = gate == WorkshopRules.Gate.Ok;
        string reason = WorkshopRules.ReasonText(gate, chosen, made, logs, branches, cordage, state.jobHours);
        reasonLabel.text = !string.IsNullOrEmpty(notice) ? notice : reason;
        reasonLabel.color = !string.IsNullOrEmpty(notice) && noticeGood ? UiKit.Accent : gate == WorkshopRules.Gate.Ok ? UiKit.Cream : UiKit.Warning;

        // The stock button: on when the pack has something the bench has room for; otherwise greyed with the reason. A message from the last click shows until something else is done.
        WorkshopRules.StockGate stock = bench.StockGateNow();
        stockButton.interactable = stock == WorkshopRules.StockGate.Can;
        stockReasonLabel.text = !string.IsNullOrEmpty(stockNotice) ? stockNotice : WorkshopRules.StockReason(stock);
        stockReasonLabel.color = !string.IsNullOrEmpty(stockNotice) && stockNoticeGood ? UiKit.Accent : UiKit.Warning;
    }
}
