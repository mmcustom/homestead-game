using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public struct StoneSaveData
{
    public List<int> gathered;
    public int mined;
}

// Stone_Gathering_System.md (2026-09-26): where Stone comes from. Loose fieldstone lies all over the property — a light
// scatter everywhere and more along the creek and pond banks (LooseStone, placed by Homestead > Place Stone) — and is
// picked up by hand, one Stone each. South Ridge has the property's one real rock outcrop (RockDeposit), which only
// gives up Stone to the Stone Pick Axe (AxeTool), a Stone per few blows until it's worked out. Nothing grows back for
// Alpha 0.1, so this remembers which loose stones are gone and how much the outcrop has given, across saves.
public class StoneManager : MonoBehaviour, ISaveable
{
    public static StoneManager Instance { get; private set; }

    [Tooltip("Stone the South Ridge outcrop holds before it's worked out.")]
    [SerializeField, Min(1)] int depositStone = 40;

    readonly HashSet<int> gathered = new HashSet<int>();
    int mined;

    public int DepositRemaining => Mathf.Max(0, depositStone - mined);
    public int DepositCapacity => depositStone;
    public int GatheredCount => gathered.Count;

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

    // Hides the loose stones already picked up.
    void Apply()
    {
        foreach (LooseStone stone in FindObjectsByType<LooseStone>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            stone.gameObject.SetActive(!gathered.Contains(stone.Id));
    }

    public bool IsGathered(int id) => gathered.Contains(id);

    public void MarkGathered(LooseStone stone)
    {
        gathered.Add(stone.Id);
        stone.gameObject.SetActive(false);
    }

    // One Stone off the outcrop. False once it's worked out.
    public bool TakeFromDeposit()
    {
        if (DepositRemaining <= 0)
            return false;
        mined++;
        return true;
    }

    public void ResetStone()
    {
        gathered.Clear();
        mined = 0;
        Apply();
    }

    public StoneSaveData CaptureState() => new StoneSaveData { gathered = new List<int>(gathered), mined = mined };

    public void RestoreState(StoneSaveData data)
    {
        gathered.Clear();
        if (data.gathered != null)
            gathered.UnionWith(data.gathered);
        mined = Mathf.Max(0, data.mined);
        Apply();
    }

    string ISaveable.SaveFile => "world";
    string ISaveable.SaveKey => "stone";
    object ISaveable.CaptureState() => CaptureState();
    void ISaveable.RestoreState(string json) => RestoreState(JsonUtility.FromJson<StoneSaveData>(json));
}
