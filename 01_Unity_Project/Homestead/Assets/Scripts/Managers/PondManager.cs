using System;
using System.Collections.Generic;
using UnityEngine;

public enum PondState { Normal, Low, VeryLow }

[Serializable]
public struct PondSaveData
{
    public string[] names;
    public float[] levels;
    public float[] overflowHours; // parallel to names; absent in saves from before floods
}

// Water_System.md's "Rainwater Collection and Pond Drying", Part 2. Each pond has a Pond Level from 0 (dry) to 1 (full), keyed
// by its WaterSource zone name (a zone whose name contains "pond", the same test FishingManager.WaterAt uses), so a second pond
// works with no code change. The level falls once a game day by evaporation (by season, doubled in a Heat Wave) and rises each
// game hour of rain; a pond never evaporates below its floor (the Bass Hole is fed by the spring and creek). The level changes
// what the pond is worth, through State():
//   Normal   above normalAbove
//   Low      from veryLowBelow up to normalAbove: water quality one tier worse, fishing slower, traps less likely
//   VeryLow  below veryLowBelow: Unsafe water, fishing and traps poor
// The water surface does not move in this version; this is level, effects and prompt text only. Reads the weather directly
// (rain, Heat Wave), never IsDrought or dryDays. Creates itself at startup, so nothing has to be added to a scene. Every number
// is a first pass and a public field; saves carry one float per pond, and a save without them loads every pond full.
public class PondManager : MonoBehaviour, ISaveable
{
    public static PondManager Instance { get; private set; }

    [Header("Evaporation (fraction of the pond's nominal usable depth lost per game day)")]
    public float evaporationPerDaySpring = 0.01f;
    public float evaporationPerDaySummer = 0.03f;
    public float evaporationPerDayFall = 0.01f;
    public float evaporationPerDayWinter = 0f;
    [Tooltip("Evaporation is multiplied by this during a Heat Wave.")]
    public float heatWaveEvapMultiplier = 2f;

    [Header("Rain (fraction of the pond's depth gained per game hour)")]
    public float rainGainPerHourLight = 0.02f;
    public float rainGainPerHourHeavy = 0.06f;
    public float rainGainPerHourThunderstorm = 0.10f;

    [Header("Floors (evaporation never takes a pond below its floor)")]
    [Tooltip("Zone names, matched by position with Pond Floor Levels below.")]
    public string[] pondFloorNames = { "Bass Hole pond" };
    [Tooltip("The floor for each name above: the Bass Hole is fed by the spring and creek.")]
    public float[] pondFloorLevels = { 0.35f };
    [Tooltip("The floor of any pond not listed above (no spring or creek feed: it can dry up).")]
    public float pondFloorOtherPonds = 0f;

    [Header("State thresholds")]
    [Tooltip("Above this level a pond is Normal.")]
    public float normalAbove = 0.66f;
    [Tooltip("Below this level a pond is Very Low; from here up to Normal Above it is Low.")]
    public float veryLowBelow = 0.33f;

    [Header("Effects on fishing")]
    [Tooltip("Rod and Cane Pole bite wait is multiplied by this at a Low pond...")]
    public float biteWaitMultiplierLow = 1.5f;
    [Tooltip("...and by this at a Very Low pond.")]
    public float biteWaitMultiplierVeryLow = 2.5f;
    [Tooltip("Fish Trap hourly chance is multiplied by this at a Low pond...")]
    public float trapChanceMultiplierLow = 0.7f;
    [Tooltip("...and by this at a Very Low pond.")]
    public float trapChanceMultiplierVeryLow = 0.4f;

    [Header("Overflow (a full pond while the creek is at High water or worse)")]
    [Tooltip("An overflowing pond is one tier worse, and stays so this many game hours after the overflow ends while it is still full.")]
    public float overflowMurkHours = 12f;
    [Tooltip("Tiers worse an overflowing (or still murky) full pond is.")]
    public int overflowTiersWorse = 1;
    [Tooltip("A pond at or above this level counts as full.")]
    public float fullLevel = 0.999f;

    readonly Dictionary<string, float> levels = new Dictionary<string, float>();
    readonly Dictionary<string, float> overflowHours = new Dictionary<string, float>();
    bool linked;
    TimeManager linkedTime;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => Instance = null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Create()
    {
        if (Instance != null)
            return;
        var go = new GameObject("Pond Manager");
        go.hideFlags = HideFlags.DontSave;
        DontDestroyOnLoad(go);
        go.AddComponent<PondManager>();
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
    }

    void Update()
    {
        if (!linked)
            Link();
    }

    // The clock and the save system may start after this does, so keep trying until both exist. The rain hour comes from
    // StreamManager (HourPassed, then ApplyOverflow), which is the one subscriber to the weather's HourEnded.
    void Link()
    {
        if (Instance != this || TimeManager.Instance == null || SaveManager.Instance == null)
            return;

        linkedTime = TimeManager.Instance;
        linkedTime.DayChanged += OnDayChanged;
        SaveManager.Instance.Register(this);
        linked = true;
    }

    void OnDestroy()
    {
        if (Instance != this)
            return;

        if (linked)
        {
            if (linkedTime != null)
                linkedTime.DayChanged -= OnDayChanged;
            if (SaveManager.Instance != null)
                SaveManager.Instance.Unregister(this);
        }
        Instance = null;
    }

    // --- Ticks ---

    void OnDayChanged()
    {
        RefreshKnownPonds();
        ApplyDay(TimeManager.Instance.CurrentSeason, WeatherManager.Instance != null && WeatherManager.Instance.IsHeatWave);
    }

    // Called by StreamManager once per game hour, before it updates the stream.
    public void HourPassed(WeatherType precipitation) => ApplyHour(precipitation);

    // Called by StreamManager right after it updates the stage, with the new state. A pond that is full while the creek is at
    // High water or worse is overflowing: its overflow hours are set to overflowMurkHours, and they count down hour by hour
    // when it isn't. Kept per pond, so a pond that wasn't full never carries a flood's murk.
    public void ApplyOverflow(StreamState state)
    {
        if (state >= StreamState.HighWater)
            RefreshKnownPonds(); // a pond nobody has looked at yet still overflows

        foreach (string pond in new List<string>(levels.Keys))
        {
            bool overflowing = state >= StreamState.HighWater && levels[pond] >= fullLevel;
            if (overflowing)
                overflowHours[pond] = overflowMurkHours;
            else if (overflowHours.TryGetValue(pond, out float hours))
                overflowHours[pond] = Mathf.Max(0f, hours - 1f);
        }
    }

    // A full pond that is overflowing, or is still murky from an overflow and is still full.
    public bool IsOverflowing(string pond) =>
        pond != null && overflowHours.TryGetValue(pond, out float hours) && hours > 0f && LevelOf(pond) >= fullLevel;

    // Every pond zone of the property's water gets a level (full), so a pond nobody has looked at still dries.
    void RefreshKnownPonds()
    {
        WaterSource source = FindAnyObjectByType<WaterSource>();
        if (source == null)
            return;
        foreach (WaterSource.QualityZone zone in source.Zones)
        {
            if (IsPondZone(zone.name) && !levels.ContainsKey(zone.name))
                levels[zone.name] = 1f;
        }
    }

    public float EvaporationFor(Season season, bool heatWave)
    {
        float perDay;
        switch (season)
        {
            case Season.Spring: perDay = evaporationPerDaySpring; break;
            case Season.Summer: perDay = evaporationPerDaySummer; break;
            case Season.Fall: perDay = evaporationPerDayFall; break;
            default: perDay = evaporationPerDayWinter; break;
        }
        return heatWave ? perDay * heatWaveEvapMultiplier : perDay;
    }

    public float RainGainFor(WeatherType precipitation)
    {
        switch (precipitation)
        {
            case WeatherType.LightRain: return rainGainPerHourLight;
            case WeatherType.HeavyRain: return rainGainPerHourHeavy;
            case WeatherType.Thunderstorm: return rainGainPerHourThunderstorm;
            default: return 0f; // snow and everything else adds nothing
        }
    }

    // One game day of evaporation on every pond that has a level. A pond below its floor comes back up to it.
    public void ApplyDay(Season season, bool heatWave)
    {
        float loss = EvaporationFor(season, heatWave);
        foreach (string pond in new List<string>(levels.Keys))
            levels[pond] = Mathf.Clamp01(Mathf.Max(levels[pond] - loss, FloorOf(pond)));
    }

    // One game hour of weather on every pond that has a level (a pond not listed is full and stays full).
    public void ApplyHour(WeatherType precipitation)
    {
        float gain = RainGainFor(precipitation);
        if (gain <= 0f)
            return;
        foreach (string pond in new List<string>(levels.Keys))
            levels[pond] = Mathf.Clamp01(levels[pond] + gain);
    }

    // --- Levels ---

    public static bool IsPondZone(string zone) => zone != null && zone.Contains("pond"); // as FishingManager.WaterAt

    public float FloorOf(string pond)
    {
        for (int i = 0; pond != null && pondFloorNames != null && pondFloorLevels != null && i < pondFloorNames.Length && i < pondFloorLevels.Length; i++)
        {
            if (pondFloorNames[i] == pond)
                return Mathf.Clamp01(pondFloorLevels[i]);
        }
        return Mathf.Clamp01(pondFloorOtherPonds);
    }

    public float LevelOf(string pond) =>
        pond != null && levels.TryGetValue(pond, out float level) ? level : 1f;

    public void SetLevel(string pond, float level)
    {
        if (!string.IsNullOrEmpty(pond))
            levels[pond] = Mathf.Clamp01(level);
    }

    public PondState StateOfLevel(float level)
    {
        if (level > normalAbove)
            return PondState.Normal;
        return level >= veryLowBelow ? PondState.Low : PondState.VeryLow;
    }

    public void ResetPonds()
    {
        levels.Clear();
        overflowHours.Clear();
    }

    // Tiers worse a zone's water is from its pond: Low is one tier, Very Low reaches Unsafe from anywhere, and an overflowing
    // full pond adds overflowTiersWorse. 0 for anything that isn't a pond. WaterSource.QualityAt adds this to the creek's.
    public static int TiersWorse(string pond)
    {
        if (Instance == null || !IsPondZone(pond))
            return 0;
        PondState state = Instance.StateOfLevel(Instance.LevelOf(pond));
        int tiers = state == PondState.VeryLow ? 3 : state == PondState.Low ? 1 : 0;
        return Instance.IsOverflowing(pond) ? tiers + Instance.overflowTiersWorse : tiers;
    }

    // The one shared answer everything asks. Anything that isn't a pond (null, the spring, the creek) is Normal.
    public static PondState State(string pond) =>
        Instance != null && IsPondZone(pond) ? Instance.StateOfLevel(Instance.LevelOf(pond)) : PondState.Normal;

    // One tier worse when Low, Unsafe when Very Low. Order: Excellent, Good, Questionable, Unsafe.
    public static WaterQuality Shift(WaterQuality quality, PondState state)
    {
        switch (state)
        {
            case PondState.Low: return (WaterQuality)Mathf.Min((int)quality + 1, (int)WaterQuality.Unsafe);
            case PondState.VeryLow: return WaterQuality.Unsafe;
            default: return quality;
        }
    }

    // What WaterSource.QualityAt calls: the zone's own quality, shifted if the zone is a pond.
    public static WaterQuality Adjust(string zone, WaterQuality quality) => Shift(quality, State(zone));

    public static float BiteWaitMultiplier(string pond)
    {
        PondState state = State(pond);
        return state == PondState.Low ? Instance.biteWaitMultiplierLow
             : state == PondState.VeryLow ? Instance.biteWaitMultiplierVeryLow : 1f;
    }

    public static float TrapChanceMultiplier(string pond)
    {
        PondState state = State(pond);
        return state == PondState.Low ? Instance.trapChanceMultiplierLow
             : state == PondState.VeryLow ? Instance.trapChanceMultiplierVeryLow : 1f;
    }

    // "Pond water (low)" for the interaction prompt; nothing for Normal and for water that isn't a pond.
    public static string PromptNote(string pond)
    {
        switch (State(pond))
        {
            case PondState.Low: return "Pond water (low)";
            case PondState.VeryLow: return "Pond water (very low)";
            default: return Instance != null && Instance.IsOverflowing(pond) ? "Pond water (overflowing)" : "";
        }
    }

    // The pond zone nearest a point (by distance to its centre), or null if the property has none. For the debug keys.
    public static string NearestPond(Vector3 position)
    {
        WaterSource source = FindAnyObjectByType<WaterSource>();
        if (source == null)
            return null;
        string best = null;
        float bestDistance = float.MaxValue;
        foreach (WaterSource.QualityZone zone in source.Zones)
        {
            if (!IsPondZone(zone.name))
                continue;
            float d = Vector2.Distance(new Vector2(position.x, position.z), new Vector2(zone.center.x, zone.center.z));
            if (d < bestDistance)
            {
                bestDistance = d;
                best = zone.name;
            }
        }
        return best;
    }

    // --- Save: one float per pond ---

    string ISaveable.SaveFile => "world";
    string ISaveable.SaveKey => "ponds";

    object ISaveable.CaptureState()
    {
        var names = new List<string>(levels.Keys);
        var values = new float[names.Count];
        var overflow = new float[names.Count];
        for (int i = 0; i < names.Count; i++)
        {
            values[i] = levels[names[i]];
            overflow[i] = overflowHours.TryGetValue(names[i], out float hours) ? hours : 0f;
        }
        return new PondSaveData { names = names.ToArray(), levels = values, overflowHours = overflow };
    }

    void ISaveable.RestoreState(string json)
    {
        levels.Clear();
        overflowHours.Clear();
        PondSaveData data = JsonUtility.FromJson<PondSaveData>(json);
        if (data.names == null || data.levels == null)
            return;
        for (int i = 0; i < data.names.Length && i < data.levels.Length; i++)
        {
            levels[data.names[i]] = Mathf.Clamp01(data.levels[i]);
            // Saves from before floods carry no overflow hours: calm.
            if (data.overflowHours != null && i < data.overflowHours.Length && data.overflowHours[i] > 0f)
                overflowHours[data.names[i]] = data.overflowHours[i];
        }
    }
}
