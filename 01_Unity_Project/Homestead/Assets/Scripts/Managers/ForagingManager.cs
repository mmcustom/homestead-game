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
    public int season1;       // (Season + 1) this state applies to; 0 on older saves, which match any season of that year
}

[Serializable]
public struct ForagingSaveData
{
    public List<ForagePatchState> patches;
    public int seed;          // decides which pool patches are active each season (0 on older saves: still fixed)
}

// One row of Foraging_System.md's Seasonal Forage Frequency table: how many of a type's patches are active in each season,
// as a multiple of baseCount (the number of patches the type had before the pool, i.e. today's amount at 1.0).
[Serializable]
public class ForageSeasonRow
{
    public ForageType type;
    [Min(0)] public int baseCount = 1;
    [Min(0f)] public float spring, summer, fall, winter;

    public float Multiplier(Season s)
    {
        switch (s)
        {
            case Season.Spring: return spring;
            case Season.Summer: return summer;
            case Season.Fall: return fall;
            default: return winter;
        }
    }
}

// Foraging_System.md: what's been picked from each forage patch, so patches stay picked across scenes and saves and
// regrow on their species' cadence (Plants docs). The patches themselves are ForageNodes in the World; this only
// holds their state. Seasons come from TimeManager, so a day skip to next Summer brings the berries back.
public class ForagingManager : MonoBehaviour, ISaveable
{
    public static ForagingManager Instance { get; private set; }

    readonly Dictionary<string, ForagePatchState> patches = new Dictionary<string, ForagePatchState>();

    [Header("Seasonal frequency (Foraging_System.md, Seasonal Forage Frequency)")]
    [Tooltip("Per forage type and season: active patches = multiplier x Base Count, chosen from the pool placed in the world. " +
             "0 = none that season. A patch only counts if one of its species bears in that season (ForageSpecies Season / Also Bears In), " +
             "so a cell with no species behind it is 0 in practice.")]
    [SerializeField] ForageSeasonRow[] seasonalFrequency = DefaultRows();

    // First pass, Claude 2026-10-10 (not confirmed by Mike). Base counts are the patches per type before the pool.
    static ForageSeasonRow[] DefaultRows() => new[]
    {
        Row(ForageType.EdiblePlants, 2, 1.5f, 1.0f, 0.4f, 0f),
        Row(ForageType.Cattail, 1, 1.0f, 1.0f, 1.0f, 0.5f),
        Row(ForageType.Mushrooms, 3, 1.2f, 0.5f, 1.5f, 0f),
        Row(ForageType.Berries, 2, 0.1f, 2.0f, 0.5f, 0f),
        Row(ForageType.Fruit, 2, 0f, 0.3f, 1.5f, 0.1f),
        Row(ForageType.Nuts, 3, 0f, 0f, 2.0f, 0.4f),
    };

    static ForageSeasonRow Row(ForageType type, int baseCount, float spring, float summer, float fall, float winter) =>
        new ForageSeasonRow { type = type, baseCount = baseCount, spring = spring, summer = summer, fall = fall, winter = winter };

    int seed = Environment.TickCount;
    readonly Dictionary<int, HashSet<string>> activeBySeason = new Dictionary<int, HashSet<string>>();
    int cachedNodeCount = -1, cachedSeed;

    // Whether a patch is out in the world this season. Out-of-season patches are hidden entirely (ForageNode), not left empty.
    public bool IsActive(ForageNode node)
    {
        TimeManager time = Time;
        if (time == null || node == null || node.Species == null)
            return true;
        return ActiveSet(time.CurrentSeason, time.Year).Contains(node.Id);
    }

    // The next season (up to two years on) the patch is active again, for the map and journal ("returns in Summer").
    public bool TryNextActive(ForageNode node, out Season season)
    {
        season = default;
        TimeManager time = Time;
        if (time == null || node == null || node.Species == null)
            return false;
        int current = (int)time.CurrentSeason;
        for (int i = 1; i <= 8; i++)
        {
            int index = current + i;
            if (ActiveSet((Season)(index % 4), time.Year + index / 4).Contains(node.Id))
            {
                season = (Season)(index % 4);
                return true;
            }
        }
        return false;
    }

    // For the map and journal: a discovered patch that is out of season right now, and when it returns. False when the
    // patch isn't loaded (other scenes) or is active.
    public static bool IsDormant(string siteId, out string returnsLabel)
    {
        returnsLabel = "";
        ForageNode node = ForageNode.Find(siteId);
        if (node == null || Instance == null || Instance.IsActive(node))
            return false;
        returnsLabel = Instance.TryNextActive(node, out Season next) ? next.ToString() : "";
        return true;
    }

    HashSet<string> ActiveSet(Season season, int year)
    {
        if (cachedNodeCount != ForageNode.All.Count || cachedSeed != seed)
        {
            activeBySeason.Clear();
            cachedNodeCount = ForageNode.All.Count;
            cachedSeed = seed;
        }
        int key = year * 4 + (int)season;
        if (!activeBySeason.TryGetValue(key, out HashSet<string> active))
            activeBySeason[key] = active = ComputeActive(season, year);
        return active;
    }

    // Same save, same season, same year: same patches. Per type, the patches whose species bear this season are put in a
    // seeded order and the first (multiplier x base count) are taken, the fraction rolled with the same seed.
    HashSet<string> ComputeActive(Season season, int year)
    {
        var active = new HashSet<string>();
        foreach (ForageType type in Enum.GetValues(typeof(ForageType)))
        {
            var eligible = new List<string>();
            foreach (ForageNode node in ForageNode.All)
            {
                if (node.Species != null && node.Species.Type == type && node.Species.BearsIn(season))
                    eligible.Add(node.Id);
            }
            eligible.Sort(StringComparer.Ordinal);

            ForageSeasonRow row = RowFor(type);
            if (row == null)
            {
                active.UnionWith(eligible); // no row: always on
                continue;
            }

            var rng = new System.Random(unchecked(seed * 73856093 ^ year * 19349663 ^ (int)season * 83492791 ^ (int)type * 2654435));
            for (int i = eligible.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (eligible[i], eligible[j]) = (eligible[j], eligible[i]);
            }
            float expected = row.Multiplier(season) * row.baseCount;
            int count = Mathf.FloorToInt(expected) + (rng.NextDouble() < expected - Mathf.Floor(expected) ? 1 : 0);
            for (int i = 0; i < Mathf.Min(count, eligible.Count); i++)
                active.Add(eligible[i]);
        }
        return active;
    }

    ForageSeasonRow RowFor(ForageType type)
    {
        if (seasonalFrequency != null)
        {
            foreach (ForageSeasonRow row in seasonalFrequency)
                if (row != null && row.type == type)
                    return row;
        }
        return null;
    }

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

        // A patch's picked state is for one season of one year, so a patch that bears in several seasons starts fresh in each.
        if (!patches.TryGetValue(nodeId, out ForagePatchState state) || state.year != time.Year ||
            (state.season1 != 0 && state.season1 != (int)time.CurrentSeason + 1))
            return species.yield; // untouched this season

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
        state.season1 = (int)Time.CurrentSeason + 1;
        state.remaining = available - taken;
        if (state.remaining == 0)
            state.depletedDay = Time.TotalDays;

        PatchChanged?.Invoke(nodeId);
        return taken;
    }

    // Every patch fresh — used when starting a new game.
    public void ResetPatches()
    {
        patches.Clear();
        seed = Environment.TickCount; // a new game gets its own run of seasons
    }

    public ForagingSaveData CaptureState() => new ForagingSaveData { patches = new List<ForagePatchState>(patches.Values), seed = seed };

    public void RestoreState(ForagingSaveData data)
    {
        patches.Clear();
        seed = data.seed;
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
