using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// A game screen shown inside GameScreens' window.
public abstract class GameScreen : MonoBehaviour
{
    public abstract string Title { get; }

    // Builds the screen's layout into the window's content area, once.
    public abstract void Build(RectTransform area);

    // Refreshes from the current game state each time the screen is shown.
    public virtual void OnShow() { }
    public virtual void OnHide() { }
}

// The full-screen game screens: World Map (M), Inventory (I) and Journal (J) — Discovery_System.md's World Map and
// Journal Screen sections and Inventory_System.md's Inventory Screen. They share one window with a tab per screen.
// A screen's key opens it, switches to it from another screen, or closes it if it's already showing; Esc also closes.
// While a screen is open GameManager is in the Menu state: the clock, survival drain and player stop, the cursor is
// free, and world audio keeps playing. Everything is built in code in the HUD's look, so the prefab only holds this.
public class GameScreens : MonoBehaviour
{
    enum Kind { None = -1, Map, Inventory, Journal }

    static readonly string[] ActionNames = { "Player/Map", "Player/Inventory", "Player/Journal" };
    static readonly string[] KeyHints = { "M", "I", "J" };

    readonly GameScreen[] screens = new GameScreen[3];
    readonly Button[] tabs = new Button[3];
    readonly InputAction[] actions = new InputAction[3];

    [Tooltip("The HUD canvas, hidden while a screen is open so the minimap and compass don't sit over the window.")]
    [SerializeField] Canvas hud;
    GameObject window;
    Text titleLabel;
    Kind current = Kind.None;
    bool subscribed;

    // True while the player is typing into a text field (a journal note), so letters don't act as hotkeys.
    public static bool IsTyping
    {
        get
        {
            GameObject selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            InputField field = selected != null ? selected.GetComponent<InputField>() : null;
            return field != null && field.isFocused;
        }
    }

    public bool IsOpen => current != Kind.None;

    void Awake()
    {
        BuildWindow();

        InputActionAsset asset = InputSystem.actions;
        for (int i = 0; i < ActionNames.Length; i++)
            actions[i] = asset != null ? asset.FindAction(ActionNames[i]) : null;

        window.SetActive(false);
    }

    void OnEnable()
    {
        if (GameManager.Instance != null && !subscribed)
        {
            GameManager.Instance.StateChanged += OnStateChanged;
            subscribed = true;
        }
    }

    void OnDisable()
    {
        if (subscribed && GameManager.Instance != null)
            GameManager.Instance.StateChanged -= OnStateChanged;
        subscribed = false;
    }

    void Update()
    {
        if (IsTyping)
            return;

        for (int i = 0; i < actions.Length; i++)
        {
            if (actions[i] != null && actions[i].WasPressedThisFrame())
            {
                Toggle((Kind)i);
                return;
            }
        }
    }

    void Toggle(Kind kind)
    {
        if (current == kind)
            Close();
        else
            Open(kind);
    }

    void Open(Kind kind)
    {
        // Only from gameplay: not over the pause menu, in menus or while loading.
        if (current == Kind.None && GameManager.Instance != null && !GameManager.Instance.OpenMenu())
            return;

        if (current != Kind.None)
            screens[(int)current].OnHide();

        current = kind;
        window.SetActive(true);
        if (hud != null)
            hud.enabled = false;
        for (int i = 0; i < screens.Length; i++)
        {
            screens[i].gameObject.SetActive(i == (int)kind);
            UiKit.SetSelected(tabs[i], i == (int)kind);
        }

        titleLabel.text = screens[(int)kind].Title;
        screens[(int)kind].OnShow();
        PlaySound(SoundCue.UiClick);
    }

    public void Close()
    {
        if (GameManager.Instance != null && GameManager.Instance.State == GameState.Menu)
            GameManager.Instance.CloseMenu(); // hides the window through OnStateChanged
        else
            Hide();
    }

    // Leaving the Menu state any way at all (Esc through PlayerController, returning to the main menu) closes the window.
    void OnStateChanged(GameState state)
    {
        if (state != GameState.Menu && current != Kind.None)
            Hide();
    }

    void Hide()
    {
        if (current == Kind.None)
            return;

        screens[(int)current].OnHide();
        current = Kind.None;
        window.SetActive(false);
        if (hud != null)
            hud.enabled = true;
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
        PlaySound(SoundCue.UiBack);
    }

    static void PlaySound(SoundCue cue)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.Play(cue);
    }

    void BuildWindow()
    {
        var root = (RectTransform)transform;

        // Dims the world behind the window.
        window = UiKit.Image(root, "Window", new Color(0f, 0f, 0f, 0.55f)).gameObject;
        var windowRect = (RectTransform)window.transform;
        windowRect.Fill();
        window.GetComponent<Image>().raycastTarget = true; // clicks outside the panel don't fall through

        Image panel = UiKit.Image(windowRect, "Panel", UiKit.PanelColor);
        panel.rectTransform.anchorMin = panel.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        panel.rectTransform.sizeDelta = new Vector2(1500f, 880f);
        panel.raycastTarget = true;
        RectTransform p = panel.rectTransform;

        titleLabel = UiKit.Text(p, "Title", "", 34, UiKit.Accent, TextAnchor.MiddleLeft, FontStyle.Italic);
        titleLabel.rectTransform.anchorMin = new Vector2(0f, 1f);
        titleLabel.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        titleLabel.rectTransform.pivot = new Vector2(0f, 1f);
        titleLabel.rectTransform.offsetMin = new Vector2(32f, -70f);
        titleLabel.rectTransform.offsetMax = new Vector2(0f, -14f);

        string[] names = { "Map", "Inventory", "Journal" };
        for (int i = 0; i < names.Length; i++)
        {
            var kind = (Kind)i;
            Button tab = UiKit.Button(p, names[i] + " Tab", $"{names[i]}  [{KeyHints[i]}]", 22, () => Open(kind));
            RectTransform rt = (RectTransform)tab.transform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(1f, 1f);
            rt.sizeDelta = new Vector2(190f, 46f);
            rt.anchoredPosition = new Vector2(-32f - (names.Length - 1 - i) * 200f - 60f, -18f);
            tabs[i] = tab;
        }

        Button close = UiKit.Button(p, "Close", "X", 24, Close); // the UI font has no ✕
        RectTransform closeRect = (RectTransform)close.transform;
        closeRect.anchorMin = closeRect.anchorMax = closeRect.pivot = new Vector2(1f, 1f);
        closeRect.sizeDelta = new Vector2(46f, 46f);
        closeRect.anchoredPosition = new Vector2(-24f, -18f);

        Image rule = UiKit.Image(p, "Rule", new Color(UiKit.Cream.r, UiKit.Cream.g, UiKit.Cream.b, 0.2f));
        rule.rectTransform.anchorMin = new Vector2(0f, 1f);
        rule.rectTransform.anchorMax = new Vector2(1f, 1f);
        rule.rectTransform.offsetMin = new Vector2(24f, -82f);
        rule.rectTransform.offsetMax = new Vector2(-24f, -80f);

        Text hint = UiKit.Text(p, "Hint", "Esc to close", 18, UiKit.Muted, TextAnchor.MiddleRight);
        hint.rectTransform.anchorMin = new Vector2(0.5f, 0f);
        hint.rectTransform.anchorMax = new Vector2(1f, 0f);
        hint.rectTransform.offsetMin = new Vector2(0f, 8f);
        hint.rectTransform.offsetMax = new Vector2(-32f, 40f);

        screens[(int)Kind.Map] = CreateScreen<WorldMapScreen>(p, "World Map");
        screens[(int)Kind.Inventory] = CreateScreen<InventoryScreen>(p, "Inventory");
        screens[(int)Kind.Journal] = CreateScreen<JournalScreen>(p, "Journal");
    }

    static T CreateScreen<T>(RectTransform panel, string name) where T : GameScreen
    {
        RectTransform area = UiKit.Rect(name, panel).Fill(32f, 44f, 32f, 96f);
        var screen = area.gameObject.AddComponent<T>();
        screen.Build(area);
        area.gameObject.SetActive(false);
        return screen;
    }
}
