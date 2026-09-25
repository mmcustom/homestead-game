using System;
using UnityEngine;

// Where a line or trap is in the water, for which fish live there (Fishing_System.md's Fishing Locations).
public enum FishWater { None, Pond, Creek }

// Fishing_System.md and the four species sheets in 05_Documentation/Fishing, as numbers: which fish bite where, when
// and how readily, for both the rod (FishingRod) and Fish Traps (TrapManager). Yields are confirmed per sheet —
// Bluegill 1, Crappie 2, Bass 3, Catfish 4 units. The weights and timings are Claude Code's first pass (2026-09-25):
//   Bluegill — common everywhere, any hour, the easy catch even on a Cane Pole.
//   Crappie — deep pond water at dawn, dusk and night; in the Spring Spawn Window far easier and they school.
//   Bass — daylight sight hunters, best near cover (the Bass Hole); hard on a Cane Pole, nearly never in a trap.
//   Catfish — bottom scavengers, day or night but better in low light; the fish trap's best catch.
// Seasons: Spring and Fall bite quickest, Summer heat slows midday, Winter is slow. Rain helps, cold fronts hurt.
public class FishingManager : MonoBehaviour
{
    [Serializable]
    public class Species
    {
        public string itemId;
        [Min(1)] public int yield = 1;
        [Tooltip("How common in the pond and in the creek (relative weights).")]
        public float pond = 1f, creek = 1f;
        [Tooltip("Multipliers by time of day: Night, Dawn, Day, Dusk.")]
        public float night = 1f, dawn = 1f, day = 1f, dusk = 1f;
        [Tooltip("Multiplier in Spring (Crappie's spawn) and in Winter.")]
        public float spring = 1f, winter = 0.5f;
        [Tooltip("Extra weight within reach of the Bass Hole's cover.")]
        public float nearCover = 1f;
        [Tooltip("Chance the hook sets on a bite, with a Rod and Reel and with a Cane Pole.")]
        [Range(0f, 1f)] public float hookRod = 0.9f, hookCane = 0.8f;
        [Tooltip("Seconds of reeling to land it.")]
        [Min(0.2f)] public float reelSeconds = 2f;
        [Tooltip("Relative weight in a Fish Trap (Bass hunts by sight and almost never goes in one).")]
        public float trap = 1f;
    }

    public static FishingManager Instance { get; private set; }

    [SerializeField] Species[] species =
    {
        new Species { itemId = "bluegill", yield = 1, pond = 1f, creek = 0.7f, winter = 0.5f, hookRod = 0.95f, hookCane = 0.9f, reelSeconds = 1.5f, trap = 1.5f },
        new Species { itemId = "crappie", yield = 2, pond = 0.35f, creek = 0.1f, night = 2.2f, dawn = 2.2f, day = 0.35f, dusk = 2.2f,
                      spring = 3.5f, winter = 0.5f, hookRod = 0.8f, hookCane = 0.65f, reelSeconds = 2.5f, trap = 0.6f },
        new Species { itemId = "bass", yield = 3, pond = 0.5f, creek = 0.25f, night = 0.15f, day = 1.4f, winter = 0.4f, nearCover = 1.8f,
                      hookRod = 0.7f, hookCane = 0.35f, reelSeconds = 4f, trap = 0.05f },
        new Species { itemId = "catfish", yield = 4, pond = 0.4f, creek = 0.5f, night = 2f, dawn = 1.4f, dusk = 1.4f, winter = 0.5f,
                      hookRod = 0.8f, hookCane = 0.5f, reelSeconds = 5f, trap = 3f },
    };

    [Header("Bite timing (real seconds)")]
    [Tooltip("Typical wait for a bite in good conditions.")]
    [SerializeField, Min(1f)] float baseWaitSeconds = 11f;
    [Tooltip("The Bass Hole's cover counts within this distance (metres).")]
    [SerializeField, Min(0f)] float coverRange = 30f;
    [SerializeField] string coverSiteName = "Bass Hole";

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

    public Species Get(string itemId) => Array.Find(species, s => s.itemId == itemId);

    // Which water a point is in: the spring pool holds no fish, the pond zone is the pond, the rest is creek.
    public static FishWater WaterAt(WaterSource source, Vector3 point)
    {
        if (source == null)
            return FishWater.None;
        string zone = source.ZoneAt(point);
        if (zone == null)
            return FishWater.Creek;
        if (zone.Contains("pond"))
            return FishWater.Pond;
        return FishWater.None; // the spring
    }

    public bool NearCover(Vector3 point)
    {
        foreach (DiscoverySite site in FindObjectsByType<DiscoverySite>(FindObjectsSortMode.None))
        {
            if (site.DisplayName == coverSiteName && Vector3.Distance(site.transform.position, point) <= coverRange)
                return true;
        }
        return false;
    }

    // How likely each species is to be the one that bites right now, in this water. forTrap uses trap weights.
    float Weight(Species s, FishWater water, bool cover, bool forTrap)
    {
        TimeManager time = TimeManager.Instance;
        float w = water == FishWater.Pond ? s.pond : water == FishWater.Creek ? s.creek : 0f;
        if (forTrap)
            w *= s.trap;
        if (time != null)
        {
            switch (time.Phase)
            {
                case DayPhase.Night: w *= s.night; break;
                case DayPhase.Dawn: w *= s.dawn; break;
                case DayPhase.Dusk: w *= s.dusk; break;
                default: w *= s.day; break;
            }
            if (time.CurrentSeason == Season.Spring) w *= s.spring;
            if (time.CurrentSeason == Season.Winter) w *= s.winter;
        }
        if (cover)
            w *= s.nearCover;
        return w;
    }

    public Species PickSpecies(FishWater water, bool cover, bool forTrap, System.Random random = null)
    {
        float total = 0f;
        foreach (Species s in species)
            total += Weight(s, water, cover, forTrap);
        if (total <= 0f)
            return null;

        float roll = (float)(random != null ? random.NextDouble() : UnityEngine.Random.value) * total;
        foreach (Species s in species)
        {
            roll -= Weight(s, water, cover, forTrap);
            if (roll <= 0f)
                return s;
        }
        return species[species.Length - 1];
    }

    // Seconds until the next bite, by season, time of day, weather and gear.
    public float BiteWait(bool canePole)
    {
        float wait = baseWaitSeconds;
        TimeManager time = TimeManager.Instance;
        WeatherManager weather = WeatherManager.Instance;
        if (time != null)
        {
            switch (time.CurrentSeason)
            {
                case Season.Spring: wait *= 0.8f; break;
                case Season.Fall: wait *= 0.8f; break;
                case Season.Winter: wait *= 2.5f; break;
            }
            // Summer heat: fish go slow in the middle of a hot day (Fishing_System.md: early morning and evening matter).
            if (time.Phase == DayPhase.Day && weather != null && weather.TemperatureF > 85f)
                wait *= 1.5f;
        }
        if (weather != null)
        {
            if (weather.IsRaining) wait *= 0.8f;
            if (weather.Current == WeatherType.ColdFront) wait *= 1.4f;
        }
        if (canePole)
            wait *= 1.25f;
        return wait * UnityEngine.Random.Range(0.4f, 1.6f);
    }

    // For Fish Traps: chance per in-game hour that a fish goes in, by season.
    public float TrapHourlyChance(float baseChance)
    {
        TimeManager time = TimeManager.Instance;
        if (time == null)
            return baseChance;
        switch (time.CurrentSeason)
        {
            case Season.Winter: return baseChance * 0.4f;
            case Season.Summer: return baseChance;
            default: return baseChance * 1.2f;
        }
    }
}
