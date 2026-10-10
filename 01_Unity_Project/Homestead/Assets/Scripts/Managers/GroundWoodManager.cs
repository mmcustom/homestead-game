using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public struct GroundWoodSaveData
{
    public List<int> ids;
    public List<int> takenDays;
}

// Ground Sticks and Fallen Branches (Wood_Gathering_System.md, 2026-10-07): the hand-pickup source of Sticks and Branches
// that needs no Axe. Bundles of Sticks and single Branches lie on the ground (GroundWood, placed by Homestead > Place
// Sticks and Branches); unlike Stone, a forest keeps shedding them, so a taken one comes back after a few in-game days —
// Sticks sooner than Branches — in every season, Winter included, on every difficulty. This remembers which are taken and
// on what day, across saves (a save from before this existed simply has none taken).
//
// It has no scene object: it creates itself once at startup, so no scene needs editing to add it.
public class GroundWoodManager : MonoBehaviour, ISaveable
{
    public static GroundWoodManager Instance { get; private set; }

    [Tooltip("In-game days before a taken bundle of Sticks lies on the ground again.")]
    [SerializeField, Min(1)] int sticksRegrowDays = 5;
    [Tooltip("In-game days before a taken Fallen Branch lies on the ground again.")]
    [SerializeField, Min(1)] int branchRegrowDays = 7;

    readonly Dictionary<int, int> takenDay = new Dictionary<int, int>();
    float nextCheck;

    public int TakenCount => takenDay.Count;

    static int Today => TimeManager.Instance != null ? TimeManager.Instance.TotalDays : 0;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Create()
    {
        if (Instance == null)
            new GameObject("Ground Wood Manager").AddComponent<GroundWoodManager>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        if (Instance == this && SaveManager.Instance != null)
            SaveManager.Instance.Register(this);
    }

    void OnDestroy()
    {
        if (Instance != this)
            return;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (SaveManager.Instance != null)
            SaveManager.Instance.Unregister(this);
        Instance = null;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == GameManager.WorldScene)
            Apply();
    }

    // Every few seconds: anything regrown lies on the ground again.
    void Update()
    {
        if (Time.unscaledTime < nextCheck || takenDay.Count == 0)
            return;
        nextCheck = Time.unscaledTime + 5f;
        Apply();
    }

    int RegrowDays(GroundWood.Kind kind) => kind == GroundWood.Kind.Sticks ? sticksRegrowDays : branchRegrowDays;

    public void MarkTaken(GroundWood wood)
    {
        takenDay[wood.Id] = Today;
        wood.gameObject.SetActive(false);
    }

    // Shows what's on the ground: everything not taken, and everything taken long enough ago to have come back.
    void Apply()
    {
        foreach (GroundWood wood in FindObjectsByType<GroundWood>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            bool onGround = true;
            if (takenDay.TryGetValue(wood.Id, out int day))
            {
                if (Today - day >= RegrowDays(wood.WoodKind))
                    takenDay.Remove(wood.Id);
                else
                    onGround = false;
            }
            if (wood.gameObject.activeSelf != onGround)
                wood.gameObject.SetActive(onGround);
        }
    }

    public void ResetGroundWood()
    {
        takenDay.Clear();
        Apply();
    }

    public GroundWoodSaveData CaptureState()
    {
        var data = new GroundWoodSaveData { ids = new List<int>(), takenDays = new List<int>() };
        foreach (KeyValuePair<int, int> pair in takenDay)
        {
            data.ids.Add(pair.Key);
            data.takenDays.Add(pair.Value);
        }
        return data;
    }

    public void RestoreState(GroundWoodSaveData data)
    {
        takenDay.Clear();
        if (data.ids != null && data.takenDays != null)
            for (int i = 0; i < Mathf.Min(data.ids.Count, data.takenDays.Count); i++)
                takenDay[data.ids[i]] = data.takenDays[i];
        Apply();
    }

    string ISaveable.SaveFile => "world";
    string ISaveable.SaveKey => "groundwood";
    object ISaveable.CaptureState() => CaptureState();
    void ISaveable.RestoreState(string json) => RestoreState(JsonUtility.FromJson<GroundWoodSaveData>(json));
}
