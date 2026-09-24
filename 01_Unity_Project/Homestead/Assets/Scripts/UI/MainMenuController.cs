using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// The MainMenu scene's buttons. All game flow goes through GameManager; this only wires the UI to it.
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
        confirmYesButton.onClick.AddListener(StartNewGame);
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
            StartNewGame();
        }
    }

    void StartNewGame()
    {
        confirmPanel.SetActive(false);
        SetMainButtonsInteractable(false);
        GameManager.Instance.StartNewGame();
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
