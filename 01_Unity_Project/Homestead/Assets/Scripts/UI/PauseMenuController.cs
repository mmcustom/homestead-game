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
    Button slotsButton; // "Save / Load..." — built in code beside Save to Slot N, which writes the active slot (shown only when there is one)
    float columnHeight, columnStep;

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
        slotsButton.onClick.AddListener(() => OpenSlotPanel());
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
        label.text = $"Save to Slot {number}";

        // With no slot to save to, "Save / Load…" is the only way in, so the one-click button goes away (and the column
        // shrinks back to the prefab's size; the layout group skips an inactive child).
        bool show = number > 0;
        bool changed = saveButton.gameObject.activeSelf != show;
        saveButton.gameObject.SetActive(show);
        var column = (RectTransform)saveButton.transform.parent;
        column.sizeDelta = new Vector2(column.sizeDelta.x, columnHeight + (show ? columnStep : 0f));
        if (changed && EventSystem.current != null && EventSystem.current.currentSelectedGameObject == saveButton.gameObject)
            EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
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
        columnStep = ((RectTransform)slotsButton.transform).sizeDelta.y + (layout != null ? layout.spacing : 0f);
        columnHeight = column.sizeDelta.y; // the prefab's, with Save to Slot N but not this button
    }

    // Save_System.md: manual save, player-initiated at any time. Writes the active slot; with none (a new game not yet
    // placed, or one loaded from the Autosave) it opens the slot panel in Save mode instead.
    void OnSave()
    {
        if (SaveManager.Instance == null)
            return;

        string active = SaveManager.Instance.ActiveSlot;
        if (active == null)
            OpenSlotPanel();
        else
            SaveToSlot(active);
    }

    void SaveToSlot(string slotId)
    {
        saveRequested = true;
        SetStatus("Saving…");
        SaveManager.Instance.SaveGame(slotId);
    }

    void OpenSlotPanel()
    {
        if (SaveManager.Instance == null)
            return;

        SlotPanel.Open(menuRoot.transform, SlotPanel.Mode.Browse, inGame: true, onPick: SaveToSlot, onClosed: RefreshSaveLabel);
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
