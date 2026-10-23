using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Menu: a full-screen game screen (Inventory, Journal, World Map) is open. Like Paused, the clock and gameplay stop,
// but the pause menu stays hidden and world audio keeps playing.
// Lost: three payments on the land went unpaid (Money_System.md Phase C). The clock is stopped, the pause menu and every game window are
// blocked, and the loss screen (LossScreen) is up. It is not InGame, so nothing is saved from it.
public enum GameState { Booting, MainMenu, Loading, Playing, Paused, Menu, Lost }

// Application lifecycle, scene transitions and system initialization (Unity_Architecture.md).
// Bootstrap is loaded first, managers initialize there, then Bootstrap loads MainMenu.
public class GameManager : MonoBehaviour, ISaveable
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

    // How this game began (difficulty, parcel, savings split; the weapon follows from the difficulty), so the loss screen's Restart can start the same one again. Saved
    // with the game (player/setup); a save made before it existed has none and Restart falls back to the New Game screens.
    NewGameSetup? setup;
    public bool HasRestartSetup => setup.HasValue;

    // A line for the main menu to show once, then clear (set when Restart had to fall back to the New Game screens).
    public string MainMenuNote { get; set; }

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
        if (Instance == this && SaveManager.Instance != null)
            SaveManager.Instance.Register(this);
        if (SceneManager.GetActiveScene().name == BootstrapScene)
            GoToMainMenu();
    }

    // New Game on a difficulty (Difficulty_System.md): the tier's starting kit, locked in for this save, and the land bought (below).
    // slotId is the manual slot the player picked for this game (SlotPanel's New Game mode); it becomes the active slot
    // up front, and nothing is written to it (or to any other slot) until a save.
    // land is the parcel and savings split chosen at the land purchase (LandPurchasePanel): the cash reserve becomes the player's money
    // and the rest of the price becomes the land loan. Without it (nothing calls it that way today) there is no loan and no cash.
    public void StartNewGame(Difficulty difficulty = Difficulty.Settler, string slotId = null, LandChoice? land = null)
    {
        if (IsLoading)
            return;

        if (SaveManager.Instance != null)
            SaveManager.Instance.SetActiveSlot(slotId);

        setup = land.HasValue
            ? NewGameSetup.Of((int)difficulty, true, land.Value.parcelIndex, land.Value.downCents, land.Value.reserveCents)
            : NewGameSetup.Of((int)difficulty, false, 0, 0, 0);

        if (DifficultyManager.Instance != null)
            DifficultyManager.Instance.Begin(difficulty);
        if (TradingPost.Instance != null)
            TradingPost.Instance.ResetMoney(land.HasValue ? land.Value.reserveCents : 0);
        if (MarketManager.Instance != null)
            MarketManager.Instance.ResetMarket(true); // a new market: every price starts at its current-season target
        if (LedgerManager.Instance != null)
        {
            // A new ledger: the opening cash is the whole savings and the down payment is a season-1 expense (cash starts at the reserve).
            long down = land.HasValue ? land.Value.downCents : 0;
            LedgerManager.Instance.BeginNewGame(land.HasValue ? land.Value.reserveCents + down : 0, down);
        }
        if (MoneyManager.Instance != null)
        {
            if (land.HasValue)
            {
                Parcel parcel = LandRules.Starters[land.Value.parcelIndex];
                MoneyManager.Instance.BeginLoan(parcel.acres, parcel.priceCents, land.Value.downCents);
            }
            else
            {
                MoneyManager.Instance.ResetLoan();
            }
        }

        ResetManagers(() =>
        {
            foreach ((string item, int count) in DifficultyManager.Kit(difficulty))
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

    // The loss screen's Restart: a new game at once with the same difficulty, parcel and savings split, in a fresh world, with no
    // slot picked (like any new game until its first manual save). A game with no setup record (an old save) goes back to the New Game
    // screens instead, with a line saying so.
    public void RestartGame()
    {
        if (IsLoading)
            return;

        if (!setup.HasValue || !setup.Value.IsValid)
        {
            MainMenuNote = "That game did not record how it began, so pick your start again.";
            ReturnToMainMenu();
            return;
        }

        NewGameSetup s = setup.Value;
        LandChoice? land = s.hasLand
            ? new LandChoice { parcelIndex = s.parcelIndex, downCents = s.downCents, reserveCents = s.reserveCents }
            : (LandChoice?)null;
        StartNewGame((Difficulty)s.difficulty, null, land); // the weapon follows from the difficulty
    }

    IEnumerator LoadSlotRoutine(string slotId)
    {
        setup = null; // a save with no setup section (an old game) has none; one that has it restores it
        // Saves from before difficulty and money existed have nothing to restore for them: Settler, no cash.
        if (DifficultyManager.Instance != null)
            DifficultyManager.Instance.ResetForLoad();
        if (TradingPost.Instance != null)
            TradingPost.Instance.ResetMoney(0);
        if (MoneyManager.Instance != null)
            MoneyManager.Instance.ResetLoan(); // a save with no loan section (an old game) has no loan
        if (MarketManager.Instance != null)
            MarketManager.Instance.ResetMarket(false); // a save with no market section (an old game) starts at normal prices
        if (LedgerManager.Instance != null)
            LedgerManager.Instance.ResetLedger(); // a save with no ledger section (an old game) starts its ledger at the first ProcessNow
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
        if (state == GameState.Lost)
            return; // the loss screen has its own buttons
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

    // A lost game (MoneyManager.Lost) goes to the Lost state, but never over the sleep fade: if the third miss lands during a sleep the
    // flag is already set (so nothing saves) and the screen waits until the player has woken.
    void Update()
    {
        if (state == GameState.Lost || !InGame || MoneyManager.Instance == null || !MoneyManager.Instance.Lost)
            return;
        if (SleepManager.Instance != null && SleepManager.Instance.IsSleeping)
            return;

        SetState(GameState.Lost);
        LossScreen.Open();
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
        Time.timeScale = state == GameState.Paused || state == GameState.Menu || state == GameState.Lost ? 0f : 1f;

        TimeManager time = TimeManager.Instance;
        if (time == null)
            return;

        if (state == GameState.Playing)
            time.Resume();
        else
            time.Pause();
    }

    // --- Saving (player/setup) ---

    string ISaveable.SaveFile => "player";
    string ISaveable.SaveKey => "setup";
    object ISaveable.CaptureState() => NewGameSetupSaveData.Of(setup);
    void ISaveable.RestoreState(string json) => setup = JsonUtility.FromJson<NewGameSetupSaveData>(json).ToSetup();
}
