using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Shown while GameManager is Paused (Esc toggles pause via PlayerController). Main Menu and Quit both save on the
// way out (GameManager.ReturnToMainMenu, SaveManager's save on quit), so they don't need a confirmation.
public class PauseMenuController : MonoBehaviour
{
    [SerializeField] GameObject menuRoot;
    [SerializeField] Button resumeButton;
    [SerializeField] Button saveButton;
    [SerializeField] Button mainMenuButton;
    [SerializeField] Button quitButton;

    [Tooltip("Current in-game date, time and weather.")]
    [SerializeField] Text dateLabel;
    [Tooltip("Save feedback: Saving… / Game saved. / failure.")]
    [SerializeField] Text statusLabel;

    bool subscribed;

    // Only report on saves started from this menu, not auto-saves finishing in the background.
    bool saveRequested;

    void Awake()
    {
        resumeButton.onClick.AddListener(() => GameManager.Instance?.ResumeGame());
        saveButton.onClick.AddListener(OnSave);
        mainMenuButton.onClick.AddListener(() => GameManager.Instance?.ReturnToMainMenu());
        quitButton.onClick.AddListener(() => GameManager.Instance?.QuitGame());
    }

    void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.StateChanged += OnStateChanged;
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveCompleted += OnSaveCompleted;
            SaveManager.Instance.SaveFailed += OnSaveFailed;
        }
        subscribed = true;

        Show(GameManager.Instance != null && GameManager.Instance.State == GameState.Paused);
    }

    void OnDisable()
    {
        if (!subscribed)
            return;

        if (GameManager.Instance != null)
            GameManager.Instance.StateChanged -= OnStateChanged;
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveCompleted -= OnSaveCompleted;
            SaveManager.Instance.SaveFailed -= OnSaveFailed;
        }
        subscribed = false;
    }

    void OnStateChanged(GameState state) => Show(state == GameState.Paused);

    void Show(bool visible)
    {
        menuRoot.SetActive(visible);
        if (!visible)
            return;

        SetStatus("");
        saveButton.interactable = SaveManager.Instance != null;
        dateLabel.text = DescribeNow();

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
    }

    // Save_System.md: manual save, player-initiated at any time.
    void OnSave()
    {
        if (SaveManager.Instance == null)
            return;

        saveRequested = true;
        SetStatus("Saving…");
        SaveManager.Instance.SaveGame();
    }

    void OnSaveCompleted()
    {
        if (saveRequested && menuRoot.activeSelf)
            SetStatus("Game saved.");
        saveRequested = false;
    }

    void OnSaveFailed(Exception error)
    {
        if (saveRequested && menuRoot.activeSelf)
            SetStatus("Save failed. Details are in the log.");
        saveRequested = false;
    }

    void SetStatus(string message)
    {
        if (statusLabel != null)
            statusLabel.text = message;
    }

    // e.g. "Spring 3, Year 1 · 14:30 · Light Rain, 12°C"
    static string DescribeNow()
    {
        TimeManager time = TimeManager.Instance;
        if (time == null)
            return "";

        string text = $"{time.FormatDate(time.TotalDays)} · {time.Hour:00}:{time.Minute:00}";

        WeatherManager weather = WeatherManager.Instance;
        if (weather != null)
            text += $" · {SplitWords(weather.Current.ToString())}, {weather.TemperatureC:0}°C";

        return text;
    }

    // "LightRain" -> "Light Rain"
    static string SplitWords(string name) => Regex.Replace(name, "(?<=[a-z])(?=[A-Z])", " ");
}
