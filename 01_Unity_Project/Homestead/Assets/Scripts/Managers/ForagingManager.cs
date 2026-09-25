using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ForagePatchState
{
    public string nodeId;
    public int year;          // the year this state applies to; a new year means a fresh crop
    public int remaining;     // units left to pick this cycle
    public int depletedDay;   // TotalDays the patch was picked clean, for Days regrowth
}

[Serializable]
public struct ForagingSaveData
{
    public List<ForagePatchState> patches;
}

// Foraging_System.md: what's been picked from each forage patch, so patches stay picked across scenes and saves and
// regrow on their species' cadence (Plants docs). The patches themselves are ForageNodes in the World; this only
// holds their state. Seasons come from TimeManager, so a day skip to next Summer brings the berries back.
public class ForagingManager : MonoBehaviour, ISaveable
{
    public static ForagingManager Instance { get; private set; }

    readonly Dictionary<string, ForagePatchState> patches = new Dictionary<string, ForagePatchState>();

    public event Action<string> PatchChanged;

    static TimeManager Time => TimeManager.Instance;

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

    void Start()
    {
        if (Instance == this && SaveManager.Instance != null)
            SaveManager.Instance.Register(this);
    }

    void OnDestroy()
    {
        if (Instance != this)
            return;

        if (SaveManager.Instance != null)
            SaveManager.Instance.Unregister(this);
        Instance = null;
    }

    // Units that can be picked from this patch right now.
    public int Available(string nodeId, ForageSpecies species)
    {
        TimeManager time = Time;
        if (species == null || !species.InWindow(time))
            return 0;

        if (!patches.TryGetValue(nodeId, out ForagePatchState state) || state.year != time.Year)
            return species.yield; // untouched this year

        if (state.remaining <= 0 && species.regrowth == ForageRegrowth.Days &&
            time.TotalDays - state.depletedDay >= species.regrowDays)
            return species.yield;

        return state.remaining;
    }

    // True once picked clean this cycle (so the patch can say when it'll bear again).
    public bool IsPickedClean(string nodeId, ForageSpecies species) =>
        species != null && species.InWindow(Time) && Available(nodeId, species) == 0;

    // Takes up to quantity units; returns how many were taken.
    public int Take(string nodeId, ForageSpecies species, int quantity)
    {
        int available = Available(nodeId, species);
        int taken = Mathf.Clamp(quantity, 0, available);
        if (taken == 0)
            return 0;

        if (!patches.TryGetValue(nodeId, out ForagePatchState state))
            patches[nodeId] = state = new ForagePatchState { nodeId = nodeId };

        state.year = Time.Year;
        state.remaining = available - taken;
        if (state.remaining == 0)
            state.depletedDay = Time.TotalDays;

        PatchChanged?.Invoke(nodeId);
        return taken;
    }

    // Every patch fresh — used when starting a new game.
    public void ResetPatches() => patches.Clear();

    public ForagingSaveData CaptureState() => new ForagingSaveData { patches = new List<ForagePatchState>(patches.Values) };

    public void RestoreState(ForagingSaveData data)
    {
        patches.Clear();
        if (data.patches != null)
        {
            foreach (ForagePatchState state in data.patches)
            {
                if (!string.IsNullOrEmpty(state.nodeId))
                    patches[state.nodeId] = state;
            }
        }
    }

    // Save_Data_Model.md's World Block.
    string ISaveable.SaveFile => "world";
    string ISaveable.SaveKey => "forage";
    object ISaveable.CaptureState() => CaptureState();
    void ISaveable.RestoreState(string json) => RestoreState(JsonUtility.FromJson<ForagingSaveData>(json));
}
