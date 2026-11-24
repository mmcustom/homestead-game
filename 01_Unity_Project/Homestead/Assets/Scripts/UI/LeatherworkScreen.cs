using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// The Workshop's Leatherwork screen (tanning slice T5a, Mike 2026-11-23): the second tab of the Workshop, opened from the Workshop screen's Leatherwork button (and back with
// its Woodwork button or W). The same window as the Workshop screen (own canvas, 1920x1080 scaler, sort order 150) and the same large text: nothing under 26. The five pieces
// of clothing are listed on the left (name and slot, hours, what the Trading Post pays now); the chosen piece's details are on the right (slot, warmth, hours, weight, what
// it needs against what the pack carries, and why it cannot start) with a Start button. A leather piece takes its materials straight from the pack when it starts, so there
// is no stocking step; it needs the Knife in the pack. It then runs as the bench's one job, hour by hour, and E at the bench collects it, as for the woodwork.
// Keys: 1-5 choose, Up and Down move, Enter starts, W switches to Woodwork, Esc closes. The bench's job starts through PlacedStructure.TryStartLeather, which works the gate
// out again on the bench and the pack as they are at the click.
public class LeatherworkScreen : MonoBehaviour
{
    public const int MinimumSize = 26;
    public const int TitleSize = 36, StatusSize = 28, PackSize = 28, ListNameSize = 32, ListDetailSize = 28, DetailNameSize = 40, BodySize = 28, StartSize = 36,
                     TabSize = 28, CloseSize = 28, HintSize = 28;
    public const float RowHeight = 80f, RowGap = 6f;   // seven rows since T5b (88/8 for five): 7 x 80 + 6 x 6 = 596 px, from y 204 to 800

    const float PanelWidth = 1500f, PanelHeight = 920f;
    const float ListX = 32f, ListY = 204f, ListWidth = 700f;
    const float DetailX = 772f, DetailWidth = 696f;

    static LeatherworkScreen instance;
    static int lastChoice;

    public static bool IsOpen => instance != null && instance.open;

    public static void Open(PlacedStructure bench)
    {
        if (bench == null || GameManager.Instance == null)
            return;
        if (instance == null)
        {
            var go = new GameObject("Leatherwork Screen");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<LeatherworkScreen>();
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
    string notice;
    bool noticeGood;
    GameObject root;
    Text statusLabel, packLabel, detailName, detailLine, effectLabel, needsHeading, reasonLabel;
    readonly Text[] needLabels = new Text[3];
    readonly Button[] rows = new Button[ClothingRules.Pieces.Length];
    readonly Text[] rowNames = new Text[ClothingRules.Pieces.Length];
    readonly Text[] rowDetails = new Text[ClothingRules.Pieces.Length];
    Button startButton;
    Text startLabel;

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

        Text title = UiKit.Heading(panel, "Title", ClothingRules.Title, TitleSize, TextAnchor.MiddleLeft);
        Place(title.rectTransform, 32f, 14f, 640f, 60f);
        Button tab = UiKit.Button(panel, "Woodwork Tab", ClothingRules.TabWoodwork, TabSize, SwitchToWoodwork);
        Place((RectTransform)tab.transform, PanelWidth - 32f - 190f - 20f - 260f, 16f, 260f, 56f);
        Button close = UiKit.Button(panel, "Close", "Close", CloseSize, () => Hide(true));
        Place((RectTransform)close.transform, PanelWidth - 32f - 190f, 16f, 190f, 56f);

        Image rule = UiKit.Image(panel, "Rule", new Color(UiKit.Ink.r, UiKit.Ink.g, UiKit.Ink.b, 0.2f));
        Place(rule.rectTransform, 24f, 84f, PanelWidth - 48f, 2f);

        statusLabel = UiKit.Text(panel, "Status", "", StatusSize, UiKit.Ink, TextAnchor.MiddleLeft);
        Place(statusLabel.rectTransform, 32f, 94f, PanelWidth - 64f, 48f);
        packLabel = UiKit.Text(panel, "Pack", "", PackSize, UiKit.Ink, TextAnchor.MiddleLeft);
        Place(packLabel.rectTransform, 32f, 142f, PanelWidth - 64f, 48f);

        for (int i = 0; i < rows.Length; i++)
        {
            int index = i;
            Button row = UiKit.Button(panel, "Row " + i, "", ListNameSize, () => Choose(index));
            Place((RectTransform)row.transform, ListX, ListY + i * (RowHeight + RowGap), ListWidth, RowHeight);
            rowNames[i] = row.GetComponentInChildren<Text>();
            rowNames[i].fontStyle = FontStyle.Bold;
            rowNames[i].alignment = TextAnchor.MiddleLeft;
            Place(rowNames[i].rectTransform, 18f, 2f, ListWidth - 36f, 38f);
            rowDetails[i] = UiKit.Text(row.transform, "Detail", "", ListDetailSize, UiKit.Ink, TextAnchor.MiddleLeft);
            Place(rowDetails[i].rectTransform, 18f, 40f, ListWidth - 36f, 38f);
            rows[i] = row;
        }

        detailName = UiKit.Text(panel, "Piece", "", DetailNameSize, UiKit.Ink, TextAnchor.MiddleLeft, FontStyle.Bold);
        Place(detailName.rectTransform, DetailX, ListY, DetailWidth, 60f);
        detailLine = UiKit.Text(panel, "Piece Line", "", BodySize, UiKit.Ink, TextAnchor.MiddleLeft);
        Place(detailLine.rectTransform, DetailX, ListY + 66f, DetailWidth, 44f);
        effectLabel = UiKit.Text(panel, "Effect", "", BodySize, UiKit.Ink, TextAnchor.UpperLeft);
        Place(effectLabel.rectTransform, DetailX, ListY + 114f, DetailWidth, 72f);
        needsHeading = UiKit.Heading(panel, "Needs Heading", "It needs", BodySize, TextAnchor.MiddleLeft);
        Place(needsHeading.rectTransform, DetailX, ListY + 192f, DetailWidth, 44f);
        for (int i = 0; i < needLabels.Length; i++)
        {
            needLabels[i] = UiKit.Text(panel, "Need " + i, "", BodySize, UiKit.Ink, TextAnchor.MiddleLeft);
            needLabels[i].horizontalOverflow = HorizontalWrapMode.Overflow;
            Place(needLabels[i].rectTransform, DetailX, ListY + 240f + i * 48f, DetailWidth, 44f);
        }

        reasonLabel = UiKit.Text(panel, "Reason", "", BodySize, UiKit.Ink, TextAnchor.UpperLeft);
        Place(reasonLabel.rectTransform, DetailX, ListY + 390f, DetailWidth, 72f);

        startButton = UiKit.Button(panel, "Start", "", StartSize, StartClicked);
        Place((RectTransform)startButton.transform, DetailX, ListY + 470f, DetailWidth, 104f);
        startLabel = startButton.GetComponentInChildren<Text>();
        startLabel.fontStyle = FontStyle.Bold;

        Text hint = UiKit.Text(panel, "Hint", ClothingRules.KeyHint, HintSize, UiKit.Ink, TextAnchor.MiddleLeft);
        hint.horizontalOverflow = HorizontalWrapMode.Overflow;
        Place(hint.rectTransform, 32f, PanelHeight - 60f, PanelWidth - 64f, 44f);

        root.SetActive(false);
    }

    static void Place(RectTransform rt, float x, float y, float width, float height)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
        rt.sizeDelta = new Vector2(width, height);
        rt.anchoredPosition = new Vector2(x, -y);
    }

    void Show(PlacedStructure target)
    {
        if (open || GameManager.Instance == null || !GameManager.Instance.OpenMenu())
            return;

        open = true;
        bench = target;
        selected = Mathf.Clamp(lastChoice, 0, ClothingRules.Pieces.Length - 1);
        notice = null;
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
        if (closeMenu && AudioManager.Instance != null)
            AudioManager.Instance.Play(SoundCue.UiBack);
    }

    // The Woodwork tab: this screen closes without leaving the Menu state, and the Workshop screen opens on the same bench.
    void SwitchToWoodwork()
    {
        PlacedStructure target = bench;
        Hide(false);
        if (target != null)
            WorkshopScreen.Open(target);
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

    bool BenchAlive()
    {
        if (bench == null || bench.State == null || WoodManager.Instance == null)
            return false;
        foreach (WoodPileState pile in WoodManager.Instance.Piles)
            if (pile == bench.State)
                return true;
        return false;
    }

    void Update()
    {
        if (!open)
            return;
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
            EventSystem.current.SetSelectedGameObject(null);

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;
        int digit = 0;
        ClothingRules.Key key = PressedKey(keyboard, ref digit);
        var action = ClothingRules.Resolve(key, digit, selected, ClothingRules.Pieces.Length);
        switch (action.kind)
        {
            case ClothingRules.ActionKind.Choose:
                Choose(action.choice);
                break;
            case ClothingRules.ActionKind.Start:
                StartClicked();
                break;
            case ClothingRules.ActionKind.Woodwork:
                SwitchToWoodwork();
                break;
        }
    }

    static ClothingRules.Key PressedKey(Keyboard keyboard, ref int digit)
    {
        Key[] digits = { Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5, Key.Digit6, Key.Digit7 };
        for (int i = 0; i < digits.Length; i++)
        {
            if (keyboard[digits[i]].wasPressedThisFrame)
            {
                digit = i + 1;
                return ClothingRules.Key.Digit;
            }
        }
        if (keyboard.upArrowKey.wasPressedThisFrame) return ClothingRules.Key.Up;
        if (keyboard.downArrowKey.wasPressedThisFrame) return ClothingRules.Key.Down;
        if (keyboard.enterKey.wasPressedThisFrame) return ClothingRules.Key.Enter;
        if (keyboard.wKey.wasPressedThisFrame) return ClothingRules.Key.Woodwork;
        return ClothingRules.Key.None;
    }

    void Choose(int index)
    {
        selected = index;
        lastChoice = index;
        notice = null;
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
        noticeGood = bench.TryStartLeather(ClothingRules.Pieces[selected], out string message);
        notice = message;
        if (noticeGood && AudioManager.Instance != null)
            AudioManager.Instance.Play(SoundCue.UiClick);
        Refresh();
    }

    static int PackCount(string itemId) => InventoryManager.Instance != null ? InventoryManager.Instance.Player.Count(itemId) : 0;

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
        bool working = bench.WorkshopRunning(out WorkshopRules.Recipe made);
        bool waiting = bench.WorkshopWaiting(out WorkshopRules.Recipe waitingPiece);
        statusLabel.text = WorkshopRules.BenchLine(working, made, state.jobHours, waiting, waitingPiece,
                                                   state.Count(WorkshopRules.LogsId), state.Count(WorkshopRules.BranchesId), state.Count(WorkshopRules.CordageId));
        statusLabel.color = working || waiting ? UiKit.InkAccent : UiKit.Ink;
        packLabel.text = ClothingRules.PackLine(PackCount);

        for (int i = 0; i < rows.Length; i++)
        {
            ClothingRules.Piece piece = ClothingRules.Pieces[i];
            long price = MarketManager.Instance != null ? MarketManager.Instance.SellPrice(piece.itemId) : 0;
            string[] lines = ClothingRules.ListLabel(piece, price).Split('\n');
            rowNames[i].text = lines[0];
            rowDetails[i].text = lines[1];
            bool covered = ClothingRules.Covers(piece, PackCount);
            rowNames[i].color = covered ? UiKit.Ink : UiKit.InkMuted;
            rowDetails[i].color = covered ? UiKit.Ink : UiKit.InkMuted;
            UiKit.SetSelected(rows[i], i == selected);
        }

        ClothingRules.Piece chosen = ClothingRules.Pieces[selected];
        detailName.text = chosen.name;
        detailLine.text = ClothingRules.DetailLine(chosen);
        effectLabel.text = (chosen.notWorn ? "Use: " : "Worn: ") + ClothingRules.EffectText(chosen) + ".";
        WorkshopRules.NeedRow[] needs = ClothingRules.NeedRows(chosen, PackCount);
        for (int i = 0; i < needLabels.Length; i++)
        {
            needLabels[i].gameObject.SetActive(i < needs.Length);
            if (i < needs.Length)
            {
                needLabels[i].text = ClothingRules.NeedText(needs[i]);
                needLabels[i].color = needs[i].enough ? UiKit.Ink : UiKit.InkWarning;
            }
        }

        WorkshopRules.Gate gate = bench.LeatherGateFor(chosen);
        startLabel.text = ClothingRules.StartButtonText(gate, chosen);
        startButton.interactable = gate == WorkshopRules.Gate.Ok;
        string reason = ClothingRules.ReasonText(gate, chosen, made, PackCount, state.jobHours);
        reasonLabel.text = !string.IsNullOrEmpty(notice) ? notice : reason;
        reasonLabel.color = !string.IsNullOrEmpty(notice) && noticeGood ? UiKit.InkAccent : gate == WorkshopRules.Gate.Ok ? UiKit.Ink : UiKit.InkWarning;
    }
}
