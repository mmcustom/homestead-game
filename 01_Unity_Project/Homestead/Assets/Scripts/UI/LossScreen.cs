using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Money_System.md Phase C: the loss screen. Shown while GameManager is in the Lost state (three payments on the land went unpaid): the
// clock is stopped, the pause menu and every game window are blocked, and nothing in the world can be played. It says "Mother Nature beat
// you." with a line under it, a short summary of the run taken at the moment of the loss (days survived, seasons completed, income and
// expenses since the start from the ledger, the loan still owed, the due days of the missed payments, the cash at the end) and three
// buttons: Restart (a new game at once with the same difficulty, weapon, parcel and savings split), Load Save (the slot panel in load
// mode, greyed when there are no saves) and Main Menu. Nothing here saves anything: a lost game is never written. Built in code in the
// game's dark UI look, on its own canvas like the Finances screen.
public class LossScreen : MonoBehaviour
{
    static LossScreen instance;

    public static bool IsOpen => instance != null && instance.root != null && instance.root.activeSelf;

    const float PanelWidth = 1200f, PanelHeight = 930f;
    const float RowHeight = 46f, DatesHeight = 112f, RowsTop = 240f;

    GameObject root;
    Text titleLabel, lineLabel, statusLabel;
    readonly List<Text> labels = new List<Text>();
    readonly List<Text> values = new List<Text>();
    Button restartButton, loadButton, menuButton;
    Text loadLabel;
    bool subscribed;
    bool slotPanelOpen;

    // Shows the screen (building it the first time). GameManager calls this when it enters the Lost state.
    public static void Open()
    {
        if (instance == null)
        {
            var go = new GameObject("Loss Screen");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<LossScreen>();
            instance.Build();
        }
        instance.Show();
    }

    public static void CloseIfOpen()
    {
        if (instance != null)
            instance.Hide();
    }

    // --- Layout ---

    static readonly string[] Labels =
    {
        "Days survived", "Seasons completed", "Total income", "Total expenses", "Cash you had", "Taken for the debt", "Land loan still owed",
        "Cash left", "Missed payments, due on",
    };

    // Row indexes in Labels.
    const int CashLeftRow = 7, DatesRow = 8;

    void Build()
    {
        root = new GameObject("Root", typeof(RectTransform));
        root.transform.SetParent(transform, false);
        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 140; // above the HUD and the game windows, below the slot panel (200)
        var scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        root.AddComponent<GraphicRaycaster>();

        var rootRect = (RectTransform)root.transform;
        Image dim = UiKit.Image(rootRect, "Dim", new Color(0f, 0f, 0f, 0.97f));
        dim.rectTransform.Fill();
        dim.raycastTarget = true;

        Color solid = UiKit.PanelColor;
        solid.a = 1f;
        Image panelImage = UiKit.Image(rootRect, "Panel", solid);
        panelImage.raycastTarget = true;
        RectTransform panel = panelImage.rectTransform;
        panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.sizeDelta = new Vector2(PanelWidth, PanelHeight);

        titleLabel = UiKit.Text(panel, "Title", "Mother Nature beat you.", 76, UiKit.Cream, TextAnchor.MiddleCenter, FontStyle.Bold);
        Place(titleLabel.rectTransform, 40f, 40f, PanelWidth - 80f, 110f);
        lineLabel = UiKit.Text(panel, "Line", "", 34, UiKit.Muted, TextAnchor.MiddleCenter);
        Place(lineLabel.rectTransform, 40f, 150f, PanelWidth - 80f, 56f);

        Image rule = UiKit.Image(panel, "Rule", new Color(UiKit.Cream.r, UiKit.Cream.g, UiKit.Cream.b, 0.25f));
        Place(rule.rectTransform, 100f, 224f, PanelWidth - 200f, 2f);

        // Rows are placed in Show (Cash left only appears when the debt could not take all the cash).
        for (int i = 0; i < Labels.Length; i++)
        {
            bool dates = i == DatesRow;
            Text label = UiKit.Text(panel, Labels[i], Labels[i], 28, UiKit.Muted, dates ? TextAnchor.UpperLeft : TextAnchor.MiddleLeft);
            Text value = UiKit.Text(panel, Labels[i] + " Value", "", 28, UiKit.Cream, dates ? TextAnchor.UpperRight : TextAnchor.MiddleRight);
            value.horizontalOverflow = HorizontalWrapMode.Wrap;
            labels.Add(label);
            values.Add(value);
        }

        restartButton = UiKit.Button(panel, "Restart", "Restart", 30, OnRestart);
        Place((RectTransform)restartButton.transform, 80f, 790f, 320f, 72f);
        loadButton = UiKit.Button(panel, "Load Save", "Load Save", 30, OnLoadSave);
        Place((RectTransform)loadButton.transform, 440f, 790f, 320f, 72f);
        loadLabel = loadButton.GetComponentInChildren<Text>();
        menuButton = UiKit.Button(panel, "Main Menu", "Main Menu", 30, OnMainMenu);
        Place((RectTransform)menuButton.transform, 800f, 790f, 320f, 72f);

        statusLabel = UiKit.Text(panel, "Status", "", 22, UiKit.Muted, TextAnchor.MiddleCenter);
        Place(statusLabel.rectTransform, 80f, 872f, PanelWidth - 160f, 40f);

        root.SetActive(false);
    }

    static void Place(RectTransform rt, float x, float y, float width, float height)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
        rt.sizeDelta = new Vector2(width, height);
        rt.anchoredPosition = new Vector2(x, -y);
    }

    // --- Showing ---

    void Show()
    {
        Subscribe();
        MoneyManager money = MoneyManager.Instance;
        RunSummary s = money != null ? money.LossSummary : default;
        TimeManager time = TimeManager.Instance;

        lineLabel.text = LossRules.MissLine(money != null ? money.LossThreshold : LossRules.DefaultThreshold);

        string missed = "none";
        if (s.missedDueDays != null && s.missedDueDays.Length > 0)
        {
            var dates = new List<string>();
            foreach (int day in s.missedDueDays)
                dates.Add(time != null ? time.FormatDate(day) : "day " + day);
            missed = string.Join("\n", dates);
        }

        string[] texts =
        {
            s.daysSurvived.ToString(),
            s.seasonsCompleted.ToString(),
            Money.Format(s.incomeCents),
            Money.Format(s.expensesCents),
            Money.Format(s.cashCents),
            Money.Format(s.takenCents),
            Money.Format(s.owedAfterCents),
            Money.Format(s.cashLeftCents),
            missed,
        };

        // Top to bottom; Cash left only when the cash was more than everything owed.
        float y = RowsTop;
        for (int i = 0; i < values.Count; i++)
        {
            bool show = i != CashLeftRow || s.cashLeftCents > 0;
            labels[i].gameObject.SetActive(show);
            values[i].gameObject.SetActive(show);
            if (!show)
                continue;

            values[i].text = texts[i];
            float height = i == DatesRow ? DatesHeight : RowHeight;
            Place(labels[i].rectTransform, 110f, y, 440f, height);
            Place(values[i].rectTransform, 570f, y, PanelWidth - 570f - 110f, height);
            y += height;
        }

        slotPanelOpen = false;
        statusLabel.text = "";
        RefreshButtons();
        root.SetActive(true);
        UiKit.ReleaseFocus();
    }

    void Hide()
    {
        Unsubscribe();
        if (root != null)
            root.SetActive(false);
        slotPanelOpen = false;
    }

    void RefreshButtons()
    {
        bool hasSaves = GameManager.Instance != null && GameManager.Instance.HasSaveGame;
        bool free = !slotPanelOpen;
        restartButton.interactable = free;
        menuButton.interactable = free;
        loadButton.interactable = free && hasSaves;
        loadLabel.text = hasSaves ? "Load Save" : "Load Save (no saves)";
        SetDim(restartButton, restartButton.interactable);
        SetDim(menuButton, menuButton.interactable);
        SetDim(loadButton, loadButton.interactable);
    }

    // A Button only dims its background, so dim the label too.
    static void SetDim(Button button, bool enabled)
    {
        Text label = button.GetComponentInChildren<Text>();
        if (label != null)
            label.color = enabled ? UiKit.Cream : UiKit.Muted;
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

    // Any way out of the Lost state (Restart, a load, the main menu) takes the screen down.
    void OnStateChanged(GameState state)
    {
        if (state != GameState.Lost)
            Hide();
    }

    void OnDestroy()
    {
        Unsubscribe();
        if (instance == this)
            instance = null;
    }

    // The world is stopped and the cursor must be free for the buttons.
    void Update()
    {
        if (root == null || !root.activeSelf)
            return;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // --- Buttons ---

    static void PlaySound(SoundCue cue)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.Play(cue);
    }

    void OnRestart()
    {
        GameManager game = GameManager.Instance;
        if (game == null)
            return;
        PlaySound(SoundCue.UiClick);
        if (!game.HasRestartSetup)
            statusLabel.text = "That game did not record how it began. Going back to the New Game screens.";
        game.RestartGame();
    }

    void OnLoadSave()
    {
        if (GameManager.Instance == null || !GameManager.Instance.HasSaveGame)
        {
            statusLabel.text = "There are no saves to load.";
            return;
        }

        PlaySound(SoundCue.UiClick);
        slotPanelOpen = true;
        RefreshButtons();
        SlotPanel panel = SlotPanel.Open(root.transform, SlotPanel.Mode.Browse, false, null, () =>
        {
            slotPanelOpen = false;
            if (root != null && root.activeSelf)
                RefreshButtons();
        });
        if (panel == null)
        {
            slotPanelOpen = false;
            RefreshButtons();
        }
    }

    void OnMainMenu()
    {
        if (GameManager.Instance == null)
            return;
        PlaySound(SoundCue.UiClick);
        GameManager.Instance.ReturnToMainMenu();
    }
}
