using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// The MainMenu scene's buttons. All game flow goes through GameManager; this only wires the UI to it.
// New Game asks for a difficulty first (Difficulty_System.md): Homesteader, Settler or Pioneer, each described, plus the
// Bow or Rifle choice for the two tiers that pack a weapon. The choice is locked in for the save. That panel is built
// in code, in the game's UI look.
public class MainMenuController : MonoBehaviour
{
    [SerializeField] Button continueButton;
    [SerializeField] Button newGameButton;
    [SerializeField] Button quitButton;

    [Header("New game confirmation")]
    [Tooltip("Shown when starting a new game would overwrite an existing save.")]
    [SerializeField] GameObject confirmPanel;
    [SerializeField] Button confirmYesButton;
    [SerializeField] Button confirmNoButton;

    [SerializeField] Text versionLabel;

    GameObject difficultyPanel;
    readonly Button[] tierButtons = new Button[3];
    Button bowButton, rifleButton, startButton, backButton;
    Text blurbLabel;
    GameObject weaponRow;
    Difficulty chosenTier = Difficulty.Settler;
    StartingWeapon chosenWeapon = StartingWeapon.Bow;

    void Awake()
    {
        // Pressing Play with MainMenu open skips Bootstrap, so no managers exist. Boot properly instead;
        // Bootstrap loads MainMenu again once the managers are up.
        if (GameManager.Instance == null)
        {
            SceneManager.LoadScene(GameManager.BootstrapScene);
            return;
        }

        continueButton.onClick.AddListener(OnContinue);
        newGameButton.onClick.AddListener(OnNewGame);
        quitButton.onClick.AddListener(OnQuit);
        confirmYesButton.onClick.AddListener(ShowDifficulty);
        confirmNoButton.onClick.AddListener(CloseConfirm);

        // Audio_System.md: click for choices, back for cancelling out.
        foreach (Button button in new[] { continueButton, newGameButton, quitButton, confirmYesButton })
            button.onClick.AddListener(() => PlaySound(SoundCue.UiClick));
        confirmNoButton.onClick.AddListener(() => PlaySound(SoundCue.UiBack));
    }

    static void PlaySound(SoundCue cue)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.Play(cue);
    }

    void OnEnable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Start()
    {
        if (GameManager.Instance == null)
            return;

        if (versionLabel != null)
            versionLabel.text = "Alpha " + Application.version;

        confirmPanel.SetActive(false);
        BuildDifficultyPanel();
        SetInteractable(continueButton, GameManager.Instance.HasSaveGame);
        Select(continueButton.interactable ? continueButton : newGameButton);
    }

    void OnContinue() => GameManager.Instance.ContinueGame();

    // Save_System.md: never lose progress silently — the first auto-save of a new game would replace the
    // existing save, so ask first.
    void OnNewGame()
    {
        if (GameManager.Instance.HasSaveGame)
        {
            confirmPanel.SetActive(true);
            SetMainButtonsInteractable(false);
            Select(confirmNoButton);
        }
        else
        {
            ShowDifficulty();
        }
    }

    // --- Choosing a difficulty ---

    void ShowDifficulty()
    {
        confirmPanel.SetActive(false);
        SetMainButtonsInteractable(false);
        difficultyPanel.SetActive(true);
        ChooseTier(chosenTier);
        Select(startButton);
    }

    void HideDifficulty()
    {
        difficultyPanel.SetActive(false);
        SetMainButtonsInteractable(true);
        Select(newGameButton);
    }

    void ChooseTier(Difficulty tier)
    {
        chosenTier = tier;
        for (int i = 0; i < tierButtons.Length; i++)
            UiKit.SetSelected(tierButtons[i], i == (int)tier);
        blurbLabel.text = DifficultyManager.Blurb(tier);
        weaponRow.SetActive(tier != Difficulty.Pioneer);
        ChooseWeapon(chosenWeapon);
    }

    void ChooseWeapon(StartingWeapon weapon)
    {
        chosenWeapon = weapon;
        UiKit.SetSelected(bowButton, weapon == StartingWeapon.Bow);
        UiKit.SetSelected(rifleButton, weapon == StartingWeapon.Rifle);
    }

    void StartNewGame()
    {
        difficultyPanel.SetActive(false);
        confirmPanel.SetActive(false);
        SetMainButtonsInteractable(false);
        GameManager.Instance.StartNewGame(chosenTier, chosenWeapon);
    }

    void BuildDifficultyPanel()
    {
        Canvas canvas = newGameButton.GetComponentInParent<Canvas>();
        var root = (RectTransform)canvas.transform;

        Image shade = UiKit.Image(root, "Difficulty", new Color(0f, 0f, 0f, 0.6f));
        shade.rectTransform.Fill();
        shade.raycastTarget = true;
        difficultyPanel = shade.gameObject;
        // Its own canvas, sorted above the menu's, so it always draws on top.
        var overlay = difficultyPanel.AddComponent<Canvas>();
        overlay.overrideSorting = true;
        overlay.sortingOrder = 100;
        difficultyPanel.AddComponent<GraphicRaycaster>();

        // Opaque: the menu's big title would otherwise show faintly through the usual panel colour.
        Color solid = UiKit.PanelColor;
        solid.a = 1f;
        Image panel = UiKit.Image(shade.rectTransform, "Panel", solid);
        RectTransform p = panel.rectTransform;
        p.anchorMin = p.anchorMax = new Vector2(0.5f, 0.5f);
        p.sizeDelta = new Vector2(1100f, 600f);

        Text title = UiKit.Text(p, "Title", "Choose how you start", 34, UiKit.Accent, TextAnchor.MiddleLeft, FontStyle.Italic);
        Place(title.rectTransform, 32f, 24f, 700f, 50f);
        Text lockNote = UiKit.Text(p, "Lock", "Locked in for this save", 18, UiKit.Muted, TextAnchor.MiddleRight);
        Place(lockNote.rectTransform, 700f, 24f, 368f, 50f);

        string[] names = { "Homesteader", "Settler", "Pioneer" };
        string[] subtitles = { "Easiest", "Normal", "Hardest" };
        for (int i = 0; i < names.Length; i++)
        {
            var tier = (Difficulty)i;
            Button tierButton = UiKit.Button(p, names[i], $"{names[i]}\n<size=16><color=#EDE3C799>{subtitles[i]}</color></size>", 26,
                                             () => { ChooseTier(tier); PlaySound(SoundCue.UiClick); });
            Place((RectTransform)tierButton.transform, 32f, 100f + i * 104f, 300f, 92f);
            tierButtons[i] = tierButton;
        }

        blurbLabel = UiKit.Text(p, "Blurb", "", 22, UiKit.Cream, TextAnchor.UpperLeft);
        Place(blurbLabel.rectTransform, 364f, 104f, 700f, 220f);

        weaponRow = UiKit.Rect("Weapon", p).gameObject;
        var weaponRect = (RectTransform)weaponRow.transform;
        Place(weaponRect, 364f, 340f, 700f, 56f);
        Text weaponLabel = UiKit.Text(weaponRect, "Label", "Your weapon:", 22, UiKit.Muted, TextAnchor.MiddleLeft);
        Place(weaponLabel.rectTransform, 0f, 0f, 170f, 56f);
        bowButton = UiKit.Button(weaponRect, "Bow", "Bow + 20 arrows", 20, () => { ChooseWeapon(StartingWeapon.Bow); PlaySound(SoundCue.UiClick); });
        Place((RectTransform)bowButton.transform, 180f, 0f, 250f, 56f);
        rifleButton = UiKit.Button(weaponRect, "Rifle", "Rifle + 10 rounds", 20, () => { ChooseWeapon(StartingWeapon.Rifle); PlaySound(SoundCue.UiClick); });
        Place((RectTransform)rifleButton.transform, 445f, 0f, 250f, 56f);

        backButton = UiKit.Button(p, "Back", "Back", 24, () => { HideDifficulty(); PlaySound(SoundCue.UiBack); });
        Place((RectTransform)backButton.transform, 32f, 520f, 200f, 56f);
        startButton = UiKit.Button(p, "Start", "Start", 26, () => { PlaySound(SoundCue.UiClick); StartNewGame(); });
        Place((RectTransform)startButton.transform, 868f, 520f, 200f, 56f);

        difficultyPanel.SetActive(false);
    }

    // Places a child by its top-left corner within the panel, in panel pixels.
    static void Place(RectTransform rt, float x, float y, float width, float height)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
        rt.sizeDelta = new Vector2(width, height);
        rt.anchoredPosition = new Vector2(x, -y);
    }

    void CloseConfirm()
    {
        confirmPanel.SetActive(false);
        SetMainButtonsInteractable(true);
        Select(newGameButton);
    }

    void OnQuit() => GameManager.Instance.QuitGame();

    void SetMainButtonsInteractable(bool value)
    {
        SetInteractable(continueButton, value && GameManager.Instance.HasSaveGame);
        SetInteractable(newGameButton, value);
        SetInteractable(quitButton, value);
    }

    // The button's color tint only covers its background, so dim the label too.
    static void SetInteractable(Button button, bool value)
    {
        button.interactable = value;

        Text label = button.GetComponentInChildren<Text>();
        if (label != null)
        {
            Color color = label.color;
            color.a = value ? 1f : 0.35f;
            label.color = color;
        }
    }

    // Gives keyboard/gamepad navigation a starting point.
    static void Select(Selectable selectable)
    {
        if (EventSystem.current != null && selectable != null)
            EventSystem.current.SetSelectedGameObject(selectable.gameObject);
    }
}
