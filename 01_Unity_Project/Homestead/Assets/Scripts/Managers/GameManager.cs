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

    public void StartNewGame()
    {
        if (IsLoading)
            return;

        if (TimeManager.Instance != null)
            TimeManager.Instance.ResetClock();
        if (WeatherManager.Instance != null)
            WeatherManager.Instance.ResetWeather();
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.ResetInventory();
            // Day-one kit (a first proposal, 2026-09-25): Water_System.md's Buckets are "available from Day One", and a
            // way to light the fire that Core_Survival_System.md's loop puts right after water.
            InventoryManager.Instance.AddToPlayer(FireManager.IgnitionId, 1);
            InventoryManager.Instance.AddToPlayer(WaterSource.BucketItemId, 1);
            // A little cord for a first snare (2026-09-25 proposal): nothing on the property produces Cordage yet.
            InventoryManager.Instance.AddToPlayer("cordage", 3);
            // An axe for firewood and logs (Wood_Gathering_System.md, 2026-09-26 proposal): nothing else supplies one.
            InventoryManager.Instance.AddToPlayer(AxeTool.AxeId, 1);
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

        StartCoroutine(LoadRoutine(WorldScene, GameState.Playing));
    }

    // Loads World, then restores the save once the scene's systems exist and have registered.
    public void ContinueGame()
    {
        if (IsLoading || !HasSaveGame)
            return;

        StartCoroutine(LoadRoutine(WorldScene, GameState.Playing, () => SaveManager.Instance.LoadGame()));
    }

    public void ReturnToMainMenu()
    {
        if (IsLoading)
            return;

        // Save_System.md: never lose progress silently. State is captured before the World scene unloads.
        if (SaveManager.Instance != null && InGame)
            SaveManager.Instance.SaveGame();

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
