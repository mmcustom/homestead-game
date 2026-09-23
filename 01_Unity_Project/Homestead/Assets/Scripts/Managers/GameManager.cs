using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { Booting, MainMenu, Loading, Playing, Paused }

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

        StartCoroutine(LoadRoutine(WorldScene, GameState.Playing));
    }

    public void ReturnToMainMenu()
    {
        if (IsLoading)
            return;

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
        else
            PauseGame();
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
    IEnumerator LoadRoutine(string sceneName, GameState stateWhenLoaded)
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
        Time.timeScale = state == GameState.Paused ? 0f : 1f;

        TimeManager time = TimeManager.Instance;
        if (time == null)
            return;

        if (state == GameState.Playing)
            time.Resume();
        else
            time.Pause();
    }
}
