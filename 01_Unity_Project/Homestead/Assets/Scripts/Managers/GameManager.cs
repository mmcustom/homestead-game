using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Menu: a full-screen game screen (Inventory, Journal, World Map) is open. Like Paused, the clock and gameplay stop,
// but the pause menu stays hidden and world audio keeps playing.
public enum GameState { Booting, MainMenu, Loading, Playing, Paused, Menu }

// Application lifecycle, scene transitions and system initialization (Unity_Architecture.md).
// Bootstrap is loaded first, managers initialize there, then Bootstrap loads MainMenu.
public class GameManager : MonoBehaviour
{
    public const string BootstrapScene = "Bootstrap";
    public const string MainMenuScene = "MainMenu";
    public const string WorldScene = "World";
    public const string LoadingScene = "Loading";

    public static GameManager Instance { get; private set; }

    GameState state = GameState.Booting;

    public event Action<GameState> StateChanged;

    public GameState State => state;
    public bool IsLoading => state == GameState.Loading;

    // In the World with a game underway, whether playing, paused or in a game screen.
    public bool InGame => state == GameState.Playing || state == GameState.Paused || state == GameState.Menu;
    public bool HasSaveGame => SaveManager.Instance != null && SaveManager.Instance.HasSave;

    // 0–1 progress of the scene currently loading behind the Loading scene.
    public float LoadProgress { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // Start runs after every manager's Awake, so all singletons are available here.
    void Start()
    {
        if (SceneManager.GetActiveScene().name == BootstrapScene)
            GoToMainMenu();
    }

    // New Game on a difficulty (Difficulty_System.md): the tier's starting kit and cash, locked in for this save.
    // slotId is the manual slot the player picked for this game (SlotPanel's New Game mode); it becomes the active slot
    // up front, and nothing is written to it (or to any other slot) until a save.
    public void StartNewGame(Difficulty difficulty = Difficulty.Settler, StartingWeapon weapon = StartingWeapon.Bow, string slotId = null)
    {
        if (IsLoading)
            return;

        if (SaveManager.Instance != null)
            SaveManager.Instance.SetActiveSlot(slotId);

        if (DifficultyManager.Instance != null)
            DifficultyManager.Instance.Begin(difficulty);
        if (TradingPost.Instance != null)
            TradingPost.Instance.ResetMoney(DifficultyManager.StartingCash);

        ResetManagers(() =>
        {
            foreach ((string item, int count) in DifficultyManager.Kit(difficulty, weapon))
                InventoryManager.Instance.AddToPlayer(item, count);
        });

        StartCoroutine(LoadRoutine(WorldScene, GameState.Playing));
    }

    // The full reset every new game and every load starts from, so a save that lacks a section never inherits the previous
    // game's state. afterInventoryReset runs right after the inventory is cleared (the new game's starting kit).
    void ResetManagers(Action afterInventoryReset = null)
    {
        if (TimeManager.Instance != null)
            TimeManager.Instance.ResetClock();
        if (WeatherManager.Instance != null)
            WeatherManager.Instance.ResetWeather();
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.ResetInventory();
            afterInventoryReset?.Invoke();
        }
        if (DiscoveryManager.Instance != null)
            DiscoveryManager.Instance.ResetDiscoveries();
        if (JournalManager.Instance != null)
            JournalManager.Instance.ResetJournal();
        if (SurvivalManager.Instance != null)
            SurvivalManager.Instance.ResetStats();
        if (MapManager.Instance != null)
            MapManager.Instance.ResetMap();
        if (FireManager.Instance != null)
            FireManager.Instance.ResetFires();
        if (ForagingManager.Instance != null)
            ForagingManager.Instance.ResetPatches();
        if (TrapManager.Instance != null)
            TrapManager.Instance.ResetTraps();
        if (WoodManager.Instance != null)
            WoodManager.Instance.ResetWood();
        if (StoneManager.Instance != null)
            StoneManager.Instance.ResetStone();
        if (GrassManager.Instance != null)
            GrassManager.Instance.ResetGrass();
        if (GroundWoodManager.Instance != null)
            GroundWoodManager.Instance.ResetGroundWood();
        if (PortableLight.Instance != null)
            PortableLight.Instance.ResetLight();
        if (PondManager.Instance != null)
            PondManager.Instance.ResetPonds();
        if (StreamManager.Instance != null)
            StreamManager.Instance.ResetStream();
        CreekCrossing.ResetCrossing();
    }

    // Continue: the newest save across every slot, the Autosave included.
    public void ContinueGame()
    {
        if (SaveManager.Instance != null)
            LoadSlot(SaveManager.Instance.NewestSlot());
    }

    // Loads one slot: the full manager reset, then World, then the slot's save once the scene's systems exist and have
    // registered. Never a hot-swap: from a running game it goes through the main menu first. A failed load falls back
    // to the main menu without entering gameplay, so nothing autosaves defaults over a save.
    public void LoadSlot(string slotId)
    {
        if (IsLoading || SaveManager.Instance == null || !SaveManager.Instance.SlotExists(slotId))
            return;

        if (InGame)
            StartCoroutine(LoadFromGameRoutine(slotId));
        else
            StartCoroutine(LoadSlotRoutine(slotId));
    }

    IEnumerator LoadFromGameRoutine(string slotId)
    {
        yield return LoadRoutine(MainMenuScene, GameState.MainMenu); // no save on the way out: the player chose to load
        yield return LoadSlotRoutine(slotId);
    }

    IEnumerator LoadSlotRoutine(string slotId)
    {
        // Saves from before difficulty and money existed have nothing to restore for them: Settler, no cash.
        if (DifficultyManager.Instance != null)
            DifficultyManager.Instance.ResetForLoad();
        if (TradingPost.Instance != null)
            TradingPost.Instance.ResetMoney(0);
        ResetManagers();
        yield return LoadRoutine(WorldScene, GameState.Playing, () => SaveManager.Instance.LoadGame(slotId));
    }

    public void ReturnToMainMenu()
    {
        if (IsLoading)
            return;

        // Save_System.md: never lose progress silently. State is captured before the World scene unloads. It goes to the
        // autosave slot, never over a manual save.
        if (SaveManager.Instance != null && InGame)
            SaveManager.Instance.SaveGame(SaveManager.AutosaveId);

        StartCoroutine(LoadRoutine(MainMenuScene, GameState.MainMenu));
    }

    public void PauseGame()
    {
        if (state == GameState.Playing)
            SetState(GameState.Paused);
    }

    public void ResumeGame()
    {
        if (state == GameState.Paused)
            SetState(GameState.Playing);
    }

    public void TogglePause()
    {
        if (SleepManager.Instance != null && SleepManager.Instance.IsSleeping)
            return; // asleep: nothing to pause or back out of
        if (state == GameState.Paused)
            ResumeGame();
        else if (state == GameState.Menu)
            CloseMenu(); // Esc backs out of a game screen rather than opening the pause menu over it
        else
            PauseGame();
    }

    // For game screens (GameScreens). Only from gameplay — not over the pause menu or while loading.
    public bool OpenMenu()
    {
        if (state == GameState.Menu)
            return true;
        if (state != GameState.Playing)
            return false;

        SetState(GameState.Menu);
        return true;
    }

    public void CloseMenu()
    {
        if (state == GameState.Menu)
            SetState(GameState.Playing);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void GoToMainMenu()
    {
        SceneManager.LoadScene(MainMenuScene);
        SetState(GameState.MainMenu);
    }

    // Shows the Loading scene, then loads the target scene in the background.
    // If afterLoad returns false, falls back to the main menu instead of entering stateWhenLoaded.
    IEnumerator LoadRoutine(string sceneName, GameState stateWhenLoaded, Func<bool> afterLoad = null)
    {
        SetState(GameState.Loading);
        LoadProgress = 0f;

        yield return SceneManager.LoadSceneAsync(LoadingScene);

        AsyncOperation load = SceneManager.LoadSceneAsync(sceneName);
        while (!load.isDone)
        {
            LoadProgress = load.progress;
            yield return null;
        }

        LoadProgress = 1f;

        // Don't enter gameplay on a failed load — an auto-save would overwrite the real save with default state.
        if (afterLoad != null && !afterLoad())
        {
            yield return LoadRoutine(MainMenuScene, GameState.MainMenu);
            yield break;
        }

        SetState(stateWhenLoaded);
    }

    void SetState(GameState newState)
    {
        if (newState == state)
            return;

        state = newState;
        ApplyState();
        StateChanged?.Invoke(state);
    }

    // The in-game clock only runs while playing. Pausing also freezes Unity time so
    // gameplay (and TimeManager, which reads deltaTime) stops together.
    void ApplyState()
    {
        Time.timeScale = state == GameState.Paused || state == GameState.Menu ? 0f : 1f;

        TimeManager time = TimeManager.Instance;
        if (time == null)
            return;

        if (state == GameState.Playing)
            time.Resume();
        else
            time.Pause();
    }
}
