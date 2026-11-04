using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// The Smithy screen (Metalworking M3, Mike 2026-11-04), modelled on the Workshop screen: E at the forge opens it to choose what to forge. Its own window (own canvas, 1920x1080 scaler,
// sort order 150), not a tab of the shared game window. The three goods are listed on the left (name, hours, what the Trading Post pays now) and, under them, the stock button; the
// chosen good's details are on the right (hours, weight, base price, what it needs against what the forge holds in plain words, and why it cannot start when it cannot) with a Start
// button below. Across the top: the forge's status (idle, a good being forged, a good ready) and what the pack carries of Iron Bars and Charcoal. While a good is being forged the list
// can still be browsed and the forge stocked, but Start is off.
// Mouse and keyboard: 1-3 choose, Up and Down move the choice, Enter starts, R stocks the forge, Esc closes (the pause key, as for the other screens). The keys are read from the new
// Input System's keyboard, one action per fresh press, and SmithyRules.Resolve turns each into its action. Esc and the Close button shut it, and so does any change out of the Menu
// state (a load, the main menu). The screen holds the Smithy it was opened for and starts a job through PlacedStructure.TryStartForge and stocks it through
// PlacedStructure.ForgeStockFromPack (the same method as the R key in the world), which work the gate out again on the Smithy as it is at the click. The Menu state stops the clock
// and the player, so the Smithy can only change by this screen's own buttons. It never dismantles. Text is large because Mike cannot read small text: nothing under 26.
//
// Fitting any window: the panel is 1500 x 920 on the 1920 x 1080 reference. A blended scaler (the Workshop's 0.5) shrinks the canvas below 920 px tall on a very wide window (32:9),
// which would clip the panel, so the scaler matches the HEIGHT on every window wide enough to hold the panel (21:9 and 32:9 included) and the width on a narrow one (4:3):
// SmithyRules.ScreenMatch chooses, each time the screen opens.
public class SmithyScreen : MonoBehaviour
{
    // The text sizes in use (UI pixels at the 1920x1080 reference). A test builds the screen and checks every label against MinimumSize.
    public const int MinimumSize = 26;
    public const int TitleSize = 36, StatusSize = 28, PackSize = 28, ListNameSize = 32, ListDetailSize = 28, DetailNameSize = 40, BodySize = 28, StartSize = 36,
                     StockSize = 32, CloseSize = 28, HintSize = 28;
    // Row heights and the gap between rows (UI pixels), and the stock button's height.
    public const float RowHeight = 104f, RowGap = 12f, StockButtonHeight = 88f;

    const float PanelWidth = SmithyRules.PanelWidthPx, PanelHeight = SmithyRules.PanelHeightPx;
    const float ListX = 32f, ListY = 204f, ListWidth = 700f;
    const float DetailX = 772f, DetailWidth = 696f;

    static SmithyScreen instance;
    static int lastChoice;

    public static bool IsOpen => instance != null && instance.open;

    // Opens the screen for a Smithy (E on a Smithy with no finished good waiting).
    public static void Open(PlacedStructure smithy)
    {
        if (smithy == null || GameManager.Instance == null)
            return;
        if (instance == null)
        {
            var go = new GameObject("Smithy Screen");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<SmithyScreen>();
            instance.Build();
        }
        instance.Show(smithy);
    }

    public static void Close()
    {
        if (instance != null)
            instance.Hide(true);
    }

    bool open, subscribed;
    PlacedStructure smithy;
    int selected;
    string notice, stockNotice;
    bool noticeGood, stockNoticeGood;
    GameObject root;
    CanvasScaler scaler;
    Text statusLabel, packLabel, detailName, detailLine, needsHeading, reasonLabel, stockReasonLabel;
    readonly Text[] needLabels = new Text[2];
    readonly Button[] rows = new Button[SmithyRules.Recipes.Length];
    readonly Text[] rowNames = new Text[SmithyRules.Recipes.Length];
    readonly Text[] rowDetails = new Text[SmithyRules.Recipes.Length];
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
        scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, SmithyRules.ReferenceHeightPx);
        scaler.matchWidthOrHeight = 1f;
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

        Text title = UiKit.Text(panel, "Title", "Smithy", TitleSize, UiKit.Accent, TextAnchor.MiddleLeft, FontStyle.Italic);
        Place(title.rectTransform, 32f, 14f, 600f, 60f);
        Button close = UiKit.Button(panel, "Close", "Close", CloseSize, () => Hide(true));
        Place((RectTransform)close.transform, PanelWidth - 32f - 190f, 16f, 190f, 56f);

        Image rule = UiKit.Image(panel, "Rule", new Color(UiKit.Cream.r, UiKit.Cream.g, UiKit.Cream.b, 0.2f));
        Place(rule.rectTransform, 24f, 84f, PanelWidth - 48f, 2f);

        // Across the top: the forge's status, then what the pack carries.
        statusLabel = UiKit.Text(panel, "Status", "", StatusSize, UiKit.Cream, TextAnchor.MiddleLeft);
        Place(statusLabel.rectTransform, 32f, 94f, PanelWidth - 64f, 48f);
        packLabel = UiKit.Text(panel, "Pack", "", PackSize, UiKit.Cream, TextAnchor.MiddleLeft);
        Place(packLabel.rectTransform, 32f, 142f, PanelWidth - 64f, 48f);

        // The list: three rows, each a button holding the name and, under it, the hours and the price.
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
        stockButton = UiKit.Button(panel, "Stock", SmithyRules.StockButtonText, StockSize, StockClicked);
        Place((RectTransform)stockButton.transform, ListX, listEnd + 12f + 72f + 8f, ListWidth, StockButtonHeight);
        stockButton.GetComponentInChildren<Text>().fontStyle = FontStyle.Bold;

        // The chosen good.
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
        Text hint = UiKit.Text(panel, "Hint", SmithyRules.KeyHint, HintSize, UiKit.Cream, TextAnchor.MiddleLeft);
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
        smithy = target;
        selected = Mathf.Clamp(lastChoice, 0, SmithyRules.Recipes.Length - 1);
        notice = stockNotice = null;
        scaler.matchWidthOrHeight = SmithyRules.ScreenMatch(Screen.height > 0 ? Screen.width / (float)Screen.height : 16f / 9f);
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
        smithy = null;
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

    // The Smithy is still there to work: the object lives and its pile is still in the world.
    bool SmithyAlive()
    {
        if (smithy == null || smithy.State == null || WoodManager.Instance == null)
            return false;
        foreach (WoodPileState pile in WoodManager.Instance.Piles)
            if (pile == smithy.State)
                return true;
        return false;
    }

    // --- Keys ---

    // Enter and the digits are also other things elsewhere (Enter is the Attack binding with the left mouse button, the digits pick the tool hotkeys, R is the world's second
    // interaction), but none of that runs in the Menu state. A button the mouse clicked stays selected in the event system and would answer Enter as a click too, so the selection
    // is cleared every frame: one press, one action.
    void Update()
    {
        if (!open)
            return;
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
            EventSystem.current.SetSelectedGameObject(null);

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;
        SmithyRules.KeyAction action = SmithyRules.Resolve(PressedKey(keyboard), selected, SmithyRules.Recipes.Length);
        switch (action.kind)
        {
            case SmithyRules.ActionKind.Choose:
                Choose(action.choice);
                break;
            case SmithyRules.ActionKind.Start:
                StartClicked();
                break;
            case SmithyRules.ActionKind.Stock:
                StockClicked();
                break;
        }
    }

    // The key pressed this frame (a fresh press only, so holding a key does nothing more), as the rules' own names.
    static SmithyRules.Key PressedKey(Keyboard keyboard)
    {
        if (keyboard.digit1Key.wasPressedThisFrame) return SmithyRules.Key.One;
        if (keyboard.digit2Key.wasPressedThisFrame) return SmithyRules.Key.Two;
        if (keyboard.digit3Key.wasPressedThisFrame) return SmithyRules.Key.Three;
        if (keyboard.upArrowKey.wasPressedThisFrame) return SmithyRules.Key.Up;
        if (keyboard.downArrowKey.wasPressedThisFrame) return SmithyRules.Key.Down;
        if (keyboard.enterKey.wasPressedThisFrame) return SmithyRules.Key.Enter;
        if (keyboard.rKey.wasPressedThisFrame) return SmithyRules.Key.Stock;
        return SmithyRules.Key.None;
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
        if (!SmithyAlive())
        {
            Hide(true);
            return;
        }
        stockNotice = null;
        noticeGood = smithy.TryStartForge(SmithyRules.Recipes[selected], out string message);
        notice = message; // "Started Door Hinges..." or the reason it could not (a key press can ask when the button is off, or the forge may have changed)
        if (noticeGood && AudioManager.Instance != null)
            AudioManager.Instance.Play(SoundCue.UiClick);
        Refresh();
    }

    // The button and the R key: put what the pack carries into the forge, the one method the R key in the world uses too. When there is nothing to move it says why instead.
    void StockClicked()
    {
        if (!SmithyAlive())
        {
            Hide(true);
            return;
        }
        notice = null;
        SmithyRules.StockGate gate = smithy.ForgeStockGateNow();
        if (gate != SmithyRules.StockGate.Can)
        {
            stockNoticeGood = false;
            stockNotice = SmithyRules.StockReason(gate);
        }
        else
        {
            stockNoticeGood = smithy.ForgeStockFromPack(out string message);
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
        if (!SmithyAlive())
        {
            Hide(true);
            return;
        }

        WoodPileState state = smithy.State;
        int bars = state.Count(SmithyRules.BarId), charcoal = state.Count(SmithyRules.CharcoalId);
        bool working = smithy.SmithyRunning(out SmithyRules.Recipe made);
        bool waiting = smithy.SmithyWaiting(out SmithyRules.Recipe waitingGood);
        statusLabel.text = SmithyRules.ForgeLine(working, made, state.jobHours, waiting, waitingGood, bars, charcoal);
        statusLabel.color = working || waiting ? UiKit.Accent : UiKit.Cream;

        // What the pack carries of Iron Bars and Charcoal.
        InventoryManager inventory = InventoryManager.Instance;
        int packBars = inventory != null ? inventory.Player.Count(SmithyRules.BarId) : 0;
        int packCharcoal = inventory != null ? inventory.Player.Count(SmithyRules.CharcoalId) : 0;
        packLabel.text = SmithyRules.PackLine(packBars, packCharcoal);

        for (int i = 0; i < rows.Length; i++)
        {
            SmithyRules.Recipe recipe = SmithyRules.Recipes[i];
            long price = MarketManager.Instance != null ? MarketManager.Instance.SellPrice(recipe.itemId) : 0;
            string[] lines = SmithyRules.ListLabel(recipe, price).Split('\n');
            rowNames[i].text = lines[0];
            rowDetails[i].text = lines[1];
            bool covered = SmithyRules.Covers(recipe, bars, charcoal);
            rowNames[i].color = covered ? UiKit.Cream : UiKit.Muted;
            rowDetails[i].color = covered ? UiKit.Cream : UiKit.Muted;
            UiKit.SetSelected(rows[i], i == selected);
        }

        SmithyRules.Recipe chosen = SmithyRules.Recipes[selected];
        detailName.text = chosen.name;
        detailLine.text = SmithyRules.DetailLine(chosen);
        SmithyRules.NeedRow[] needs = SmithyRules.NeedRows(chosen, bars, charcoal);
        for (int i = 0; i < needLabels.Length; i++)
        {
            needLabels[i].gameObject.SetActive(i < needs.Length);
            if (i < needs.Length)
            {
                needLabels[i].text = needs[i].Text;
                needLabels[i].color = needs[i].enough ? UiKit.Cream : UiKit.Warning;
            }
        }

        SmithyRules.Gate gate = smithy.SmithyGateFor(chosen);
        startLabel.text = SmithyRules.StartButtonText(gate, chosen);
        startButton.interactable = gate == SmithyRules.Gate.Ok;
        string reason = SmithyRules.ReasonText(gate, chosen, made, bars, charcoal, state.jobHours);
        reasonLabel.text = !string.IsNullOrEmpty(notice) ? notice : reason;
        reasonLabel.color = !string.IsNullOrEmpty(notice) && noticeGood ? UiKit.Accent : gate == SmithyRules.Gate.Ok ? UiKit.Cream : UiKit.Warning;

        // The stock button: on when the pack has something the forge has room for; otherwise greyed with the reason. A message from the last click shows until something else is done.
        SmithyRules.StockGate stock = smithy.ForgeStockGateNow();
        stockButton.interactable = stock == SmithyRules.StockGate.Can;
        stockReasonLabel.text = !string.IsNullOrEmpty(stockNotice) ? stockNotice : SmithyRules.StockReason(stock);
        stockReasonLabel.color = !string.IsNullOrEmpty(stockNotice) && stockNoticeGood ? UiKit.Accent : UiKit.Warning;
    }
}
