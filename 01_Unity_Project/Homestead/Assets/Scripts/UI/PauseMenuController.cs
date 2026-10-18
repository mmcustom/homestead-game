using System;
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
    Button slotsButton; // "Save / Load..." — built in code beside Save Game, which writes the active slot

    [Tooltip("Current in-game date, time and weather.")]
    [SerializeField] Text dateLabel;
    [Tooltip("Save feedback: Saving… / Game saved. / failure.")]
    [SerializeField] Text statusLabel;

    bool subscribed;

    // Only report on saves started from this menu, not auto-saves finishing in the background.
    bool saveRequested;

    void Awake()
    {
        BuildSlotsButton();
        resumeButton.onClick.AddListener(() => GameManager.Instance?.ResumeGame());
        saveButton.onClick.AddListener(OnSave);
        mainMenuButton.onClick.AddListener(() => GameManager.Instance?.ReturnToMainMenu());
        quitButton.onClick.AddListener(() => GameManager.Instance?.QuitGame());

        // Resume's sound comes from the menu closing (OnStateChanged), which also covers closing it with Esc.
        slotsButton.onClick.AddListener(() => OpenSlotPanel(SlotPanel.Mode.Save));
        foreach (Button button in new[] { saveButton, slotsButton, mainMenuButton, quitButton })
            button.onClick.AddListener(() => PlaySound(SoundCue.UiClick));
    }

    static void PlaySound(SoundCue cue)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.Play(cue);
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

    // Opening the menu clicks; closing it back to gameplay (Resume or Esc) plays the back sound.
    void OnStateChanged(GameState state)
    {
        bool wasShowing = menuRoot.activeSelf;
        Show(state == GameState.Paused);

        if (state == GameState.Paused && !wasShowing)
            PlaySound(SoundCue.UiClick);
        else if (state == GameState.Playing && wasShowing)
            PlaySound(SoundCue.UiBack);
    }

    void Show(bool visible)
    {
        menuRoot.SetActive(visible);
        if (!visible)
            return;

        SetStatus("");
        saveButton.interactable = SaveManager.Instance != null;
        RefreshSaveLabel();
        dateLabel.text = DescribeNow();

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
    }

    // "Save to Slot N" names where Save Game writes (the slot panel's number); with no active slot it opens the panel,
    // so the label says so. Refreshed on every open and whenever the panel closes (copy, delete or a save may change it).
    void RefreshSaveLabel()
    {
        Text label = saveButton != null ? saveButton.GetComponentInChildren<Text>() : null;
        if (label == null)
            return;

        string active = SaveManager.Instance != null ? SaveManager.Instance.ActiveSlot : null;
        int number = SaveManager.SlotNumber(active);
        label.text = number > 0 ? $"Save to Slot {number}" : "Save Game…";
    }

    // A second button under Save Game, cloned from it so it matches the prefab's look and sits in its layout group.
    void BuildSlotsButton()
    {
        slotsButton = Instantiate(saveButton, saveButton.transform.parent);
        slotsButton.name = "Save Load Game";
        slotsButton.onClick.RemoveAllListeners();
        slotsButton.GetComponentInChildren<Text>().text = "Save / Load…";
        slotsButton.transform.SetSiblingIndex(saveButton.transform.GetSiblingIndex() + 1);

        // Room for the extra button in the column's fixed-size rect.
        var column = (RectTransform)saveButton.transform.parent;
        var layout = column.GetComponent<VerticalLayoutGroup>();
        float step = ((RectTransform)slotsButton.transform).sizeDelta.y + (layout != null ? layout.spacing : 0f);
        column.sizeDelta += new Vector2(0f, step);
    }

    // Save_System.md: manual save, player-initiated at any time. Writes the active slot; with none (a new game not yet
    // placed, or one loaded from the Autosave) it opens the slot panel in Save mode instead.
    void OnSave()
    {
        if (SaveManager.Instance == null)
            return;

        string active = SaveManager.Instance.ActiveSlot;
        if (active == null)
            OpenSlotPanel(SlotPanel.Mode.Save);
        else
            SaveToSlot(active);
    }

    void SaveToSlot(string slotId)
    {
        saveRequested = true;
        SetStatus("Saving…");
        SaveManager.Instance.SaveGame(slotId);
    }

    void OpenSlotPanel(SlotPanel.Mode mode)
    {
        if (SaveManager.Instance == null)
            return;

        SlotPanel.Open(menuRoot.transform, mode, allowModeSwitch: true, warnUnsavedOnLoad: true, onPick: SaveToSlot, onClosed: RefreshSaveLabel);
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

    // e.g. "Spring 3, Year 1 · 14:30 · Light Rain, 54°F" (Weather_System.md: temperature is displayed in °F)
    static string DescribeNow()
    {
        TimeManager time = TimeManager.Instance;
        if (time == null)
            return "";

        string text = $"{time.FormatDate(time.TotalDays)} · {time.Hour:00}:{time.Minute:00}";

        WeatherManager weather = WeatherManager.Instance;
        if (weather != null)
        {
            // The same player-facing name and feels-like text as the Weather panel (a Heat Wave reads as Clear).
            text += $" · {weather.PlayerWeatherName()}, {weather.TemperatureF:0}°F";
            string feels = weather.FeelsLikeText();
            if (feels.Length > 0)
                text += ", " + feels;
        }

        return text;
    }
}
