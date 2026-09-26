using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public struct GrassSaveData
{
    public List<int> ids;
    public List<int> cutDays;
}

// Tall grass for Cordage (Trapping_System.md's sourcing, 2026-09-26). Clumps of tall grass stand in the open meadows
// and pasture (TallGrass, placed by Homestead > Place Tall Grass); cutting one gives 3 Tall Grass, and it grows back
// in a few in-game days — dry stalks in Winter work as well as green ones. This remembers which clumps are cut and on
// what day, across saves, and brings them back when they've regrown.
public class GrassManager : MonoBehaviour, ISaveable
{
    public static GrassManager Instance { get; private set; }

    [Tooltip("In-game days before a cut clump stands again.")]
    [SerializeField, Min(1)] int regrowDays = 4;

    readonly Dictionary<int, int> cutDay = new Dictionary<int, int>();
    float nextCheck;

    public int RegrowDays => regrowDays;
    public int CutCount => cutDay.Count;

    static int Today => TimeManager.Instance != null ? TimeManager.Instance.TotalDays : 0;

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

    // Every few seconds: anything regrown stands again.
    void Update()
    {
        if (Time.unscaledTime < nextCheck || cutDay.Count == 0)
            return;
        nextCheck = Time.unscaledTime + 5f;
        Apply();
    }

    public bool IsCut(int id) => cutDay.TryGetValue(id, out int day) && Today - day < regrowDays;

    public void MarkCut(TallGrass grass)
    {
        cutDay[grass.Id] = Today;
        grass.gameObject.SetActive(false);
    }

    void Apply()
    {
        var regrown = new List<int>();
        foreach (KeyValuePair<int, int> pair in cutDay)
            if (Today - pair.Value >= regrowDays)
                regrown.Add(pair.Key);
        foreach (int id in regrown)
            cutDay.Remove(id);

        foreach (TallGrass grass in FindObjectsByType<TallGrass>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            bool standing = !cutDay.ContainsKey(grass.Id);
            if (grass.gameObject.activeSelf != standing)
                grass.gameObject.SetActive(standing);
        }
    }

    public void ResetGrass()
    {
        cutDay.Clear();
        Apply();
    }

    public GrassSaveData CaptureState()
    {
        var data = new GrassSaveData { ids = new List<int>(), cutDays = new List<int>() };
        foreach (KeyValuePair<int, int> pair in cutDay)
        {
            data.ids.Add(pair.Key);
            data.cutDays.Add(pair.Value);
        }
        return data;
    }

    public void RestoreState(GrassSaveData data)
    {
        cutDay.Clear();
        if (data.ids != null && data.cutDays != null)
            for (int i = 0; i < Mathf.Min(data.ids.Count, data.cutDays.Count); i++)
                cutDay[data.ids[i]] = data.cutDays[i];
        Apply();
    }

    string ISaveable.SaveFile => "world";
    string ISaveable.SaveKey => "grass";
    object ISaveable.CaptureState() => CaptureState();
    void ISaveable.RestoreState(string json) => RestoreState(JsonUtility.FromJson<GrassSaveData>(json));
}
