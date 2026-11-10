using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// The Wait menu (real-calendar Phase 2a): key Z, in plain play, opens a small window with Wait 1 hour, 3 hours, 6 hours, until dawn and until dusk. A button closes the window and runs
// TimeControlManager.Wait; the rules for when a Wait cannot start and when it stops early are TimeControlRules. Like the other game windows it holds the game in the Menu state while
// open (so the clock stops), Esc or Z closes it, and anything that takes the game out of Menu closes it too. Built in code in the game's dark UI look (UiKit); there is no
// 3D visual here, so no prefab swap point. The sleep keys are untouched: sleep stays on the bed or Sleeping Bag.
public class WaitPanel : MonoBehaviour
{
    static WaitPanel instance;

    public static bool IsOpen => instance != null && instance.open;

    const float PanelWidth = 640f, PanelHeight = 600f;

    bool open, subscribed;
    GameObject root;
    Text clockLabel, noteLabel;
    readonly Button[] buttons = new Button[TimeControlRules.Kinds.Length];
    readonly Text[] buttonLabels = new Text[TimeControlRules.Kinds.Length];

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Create()
    {
        if (instance != null)
            return;
        var go = new GameObject("Wait Panel");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<WaitPanel>();
        instance.Build();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => instance = null;

    // --- Key ---

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || !keyboard.zKey.wasPressedThisFrame || GameScreens.IsTyping)
            return;

        GameManager game = GameManager.Instance;
        if (open)
        {
            Hide(true);
            return;
        }
        // Only from plain play: not over another window (Inventory, Finances, a build mode), a pause, or the main menu.
        if (game == null || game.State != GameState.Playing || PlacementMode.Active || FinancesScreen.IsOpen)
            return;
        Show();
    }

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

        Text title = UiKit.Text(panel, "Title", "Wait", 34, UiKit.Accent, TextAnchor.MiddleLeft, FontStyle.Italic);
        Place(title.rectTransform, 32f, 14f, 200f, 54f);
        clockLabel = UiKit.Text(panel, "Clock", "", 24, UiKit.Cream, TextAnchor.MiddleRight);
        Place(clockLabel.rectTransform, 240f, 14f, PanelWidth - 240f - 32f, 54f);

        Image rule = UiKit.Image(panel, "Rule", new Color(UiKit.Cream.r, UiKit.Cream.g, UiKit.Cream.b, 0.2f));
        Place(rule.rectTransform, 24f, 80f, PanelWidth - 48f, 2f);

        for (int i = 0; i < TimeControlRules.Kinds.Length; i++)
        {
            WaitKind kind = TimeControlRules.Kinds[i];
            Button button = UiKit.Button(panel, kind + " Button", TimeControlRules.Label(kind), 26, () => Choose(kind));
            Place((RectTransform)button.transform, 40f, 104f + i * 70f, PanelWidth - 80f, 58f);
            buttons[i] = button;
            buttonLabels[i] = button.GetComponentInChildren<Text>();
        }

        noteLabel = UiKit.Text(panel, "Note", "", 22, UiKit.Muted, TextAnchor.UpperLeft);
        Place(noteLabel.rectTransform, 40f, 462f, PanelWidth - 80f, 70f);

        Button close = UiKit.Button(panel, "Close", "Close  [Z]", 20, () => Hide(true));
        Place((RectTransform)close.transform, PanelWidth - 32f - 170f, PanelHeight - 62f, 170f, 46f);

        root.SetActive(false);
    }

    static void Place(RectTransform rt, float x, float y, float width, float height)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.sizeDelta = new Vector2(width, height);
        rt.anchoredPosition = new Vector2(x, -y);
    }

    // --- Showing ---

    void Show()
    {
        if (open || GameManager.Instance == null || !GameManager.Instance.OpenMenu())
            return;

        open = true;
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
        root.SetActive(false);
        Unsubscribe();
        if (UnityEngine.EventSystems.EventSystem.current != null)
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        if (closeMenu && GameManager.Instance != null && GameManager.Instance.State == GameState.Menu)
            GameManager.Instance.CloseMenu();
        if (AudioManager.Instance != null)
            AudioManager.Instance.Play(SoundCue.UiBack);
    }

    // Names the time and says what each button would do; a Wait that cannot start greys every button and says why.
    void Refresh()
    {
        TimeManager time = TimeManager.Instance;
        TimeControlManager control = TimeControlManager.Instance;
        clockLabel.text = time != null ? $"{time.FormatDate(time.TotalDays)}   {time.Hour:00}:{time.Minute:00}" : "";

        string reason = control != null ? control.CantWaitFromMenu() : "Not now.";
        for (int i = 0; i < buttons.Length; i++)
        {
            WaitKind kind = TimeControlRules.Kinds[i];
            float hours = control != null ? control.HoursFor(kind) : 0f;
            string label = TimeControlRules.Label(kind);
            if (kind == WaitKind.UntilDawn || kind == WaitKind.UntilDusk)
                label += $"   ({TimeControlRules.HoursText(hours)})";
            buttonLabels[i].text = label;
            buttons[i].interactable = reason == null;
        }
        noteLabel.text = reason ?? "A Wait stops by itself if a stat runs low, bad weather or a flood arrives, a fire is dying, a trap catches something, a notice comes in or something is close. Sleep is in a bed or Sleeping Bag.";
    }

    void Choose(WaitKind kind)
    {
        Hide(true);
        if (TimeControlManager.Instance != null)
            TimeControlManager.Instance.Wait(kind);
    }

    // Esc (back to Playing), the main menu or a load: any way out of the Menu state closes the window.
    void OnStateChanged(GameState state)
    {
        if (state != GameState.Menu)
            Hide(false);
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

    void OnDestroy()
    {
        Unsubscribe();
        if (instance == this)
            instance = null;
    }
}
