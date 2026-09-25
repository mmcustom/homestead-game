using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Brings Wildlife_System.md's game animals to the property around the player, when and where the docs put them:
//   Deer — forest edges and meadows at Dawn and Dusk, all day in the Fall Rut (second half of Fall).
//   Turkey — forest openings, at Dawn.         Rabbit and Squirrel — brush and woods, through daylight.
//   Waterfowl — on the pond once it's been discovered, through daylight; gone in Winter (they migrate).
// Seasons scale numbers (Fall is the hunting season, Winter is lean) and heavy weather keeps animals bedded down.
// Animals appear out of sight 60-130 m away and are removed when the player moves far off; a count is steady within
// an in-game hour so animals don't pop in and out. Not saved — the property is simply as lively as the hour allows.
// Numbers are Claude Code's first pass (2026-09-25), pending Mike's playtest.
public class WildlifeManager : MonoBehaviour
{
    [Serializable]
    public class Yield
    {
        public string speciesId;
        public string displayName;
        public Animal prefab;
        public string meatItem;
        [Min(0)] public int meat;
        [Tooltip("Fur or feathers (deer hide and antlers are handled by Carcass).")]
        public string extraItem;
        [Min(0)] public int extraCount;
    }

    public static WildlifeManager Instance { get; private set; }

    // Yields from the Wildlife sheets: Small 1 meat + fur, Turkey 4, Waterfowl 3 (+ feathers), Deer 8 (+ hide, antlers).
    [SerializeField] Yield[] species =
    {
        new Yield { speciesId = "deer", displayName = "Deer", meatItem = "venison", meat = 8 },
        new Yield { speciesId = "turkey", displayName = "Turkey", meatItem = "turkey_meat", meat = 4, extraItem = "feathers", extraCount = 3 },
        new Yield { speciesId = "waterfowl", displayName = "Duck", meatItem = "waterfowl_meat", meat = 3, extraItem = "feathers", extraCount = 2 },
        new Yield { speciesId = "rabbit", displayName = "Rabbit", meatItem = "small_game_meat", meat = 1, extraItem = "small_furs", extraCount = 1 },
        new Yield { speciesId = "squirrel", displayName = "Squirrel", meatItem = "small_game_meat", meat = 1, extraItem = "small_furs", extraCount = 1 },
    };

    [SerializeField, Min(10f)] float despawnDistance = 200f;
    [SerializeField, Min(1f)] float tickSeconds = 2f;
    [SerializeField] string pondSiteName = "Bass Hole";

    readonly List<Animal> animals = new List<Animal>();
    float nextTick;
    int[,] treeGrid;
    const float Cell = 5f;
    Vector3 gridOrigin;
    Terrain terrain;

    public static PlayerController Player { get; private set; }
    public IReadOnlyList<Animal> Animals => animals;

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

    void OnDestroy()
    {
        if (Instance != this)
            return;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Instance = null;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        animals.Clear();
        treeGrid = null;
        Player = null;
    }

    public Yield YieldFor(string speciesId) => Array.Find(species, s => s.speciesId == speciesId);
    public string NameOf(string speciesId) => YieldFor(speciesId)?.displayName ?? speciesId;

    // A loud noise (a gunshot): everything within earshot runs.
    public void Alert(Vector3 position, float radius)
    {
        foreach (Animal animal in animals)
        {
            if (animal != null && !animal.IsDead && Vector3.Distance(animal.transform.position, position) <= radius)
                animal.Startle(position);
        }
    }

    void Update()
    {
        if (Time.time < nextTick)
            return;
        nextTick = Time.time + tickSeconds;

        if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing)
            return;
        if (Player == null)
            Player = FindAnyObjectByType<PlayerController>();
        if (Player == null || Terrain.activeTerrain == null)
            return;
        if (treeGrid == null || terrain != Terrain.activeTerrain)
            BuildTreeGrid();

        animals.RemoveAll(a => a == null);
        Vector3 playerPos = Player.transform.position;

        // Clear out living animals the player has left far behind.
        for (int i = animals.Count - 1; i >= 0; i--)
        {
            Animal animal = animals[i];
            if (!animal.IsDead && Vector3.Distance(animal.transform.position, playerPos) > despawnDistance)
            {
                Destroy(animal.gameObject);
                animals.RemoveAt(i);
            }
        }

        // At most one new animal per tick, for whichever species is furthest under its target.
        Yield bestSpecies = null;
        int bestShortfall = 0;
        foreach (Yield s in species)
        {
            if (s.prefab == null)
                continue;
            int shortfall = Target(s.speciesId) - LivingNear(s.speciesId, playerPos);
            if (shortfall > bestShortfall)
            {
                bestShortfall = shortfall;
                bestSpecies = s;
            }
        }

        if (bestSpecies != null && FindSpawn(bestSpecies.speciesId, out Vector3 spawn))
        {
            Animal animal = Instantiate(bestSpecies.prefab, spawn, Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f));
            animal.name = bestSpecies.displayName;
            animals.Add(animal);
        }
    }

    int LivingNear(string speciesId, Vector3 position)
    {
        int count = 0;
        foreach (Animal animal in animals)
        {
            if (!animal.IsDead && animal.SpeciesId == speciesId && Vector3.Distance(animal.transform.position, position) < despawnDistance)
                count++;
        }
        return count;
    }

    // --- When and how many ---

    int Target(string speciesId)
    {
        TimeManager time = TimeManager.Instance;
        WeatherManager weather = WeatherManager.Instance;
        if (time == null)
            return 0;

        DayPhase phase = time.Phase;
        Season season = time.CurrentSeason;
        bool rut = season == Season.Fall && time.SeasonProgress >= 0.5f;
        float count;

        switch (speciesId)
        {
            case "deer":
                count = phase == DayPhase.Dawn || phase == DayPhase.Dusk ? 2f : phase == DayPhase.Day ? (rut ? 2f : 0.4f) : 0.2f;
                count *= season == Season.Fall ? 1.5f : season == Season.Winter ? 0.6f : 0.6f;
                break;
            case "turkey":
                count = phase == DayPhase.Dawn ? 3f : phase == DayPhase.Day ? 0.6f : phase == DayPhase.Dusk ? 0.5f : 0f;
                count *= season == Season.Fall ? 1.3f : season == Season.Winter ? 0.7f : 0.8f;
                break;
            case "rabbit":
                count = phase == DayPhase.Night ? 0.5f : phase == DayPhase.Day ? 1.5f : 2f;
                count *= season == Season.Winter ? 0.5f : season == Season.Spring ? 1.3f : 1f;
                break;
            case "squirrel":
                count = phase == DayPhase.Day ? 2f : phase == DayPhase.Night ? 0f : 1f;
                count *= season == Season.Winter ? 0.5f : season == Season.Fall ? 1.3f : 1f;
                break;
            case "waterfowl":
                if (season == Season.Winter || !PondKnownAndNear())
                    return 0;
                count = phase == DayPhase.Night ? 0.5f : 3f;
                break;
            default:
                return 0;
        }

        // Heavy weather keeps animals bedded down (Hunting_System.md's Weather Effects).
        if (weather != null && (weather.Current == WeatherType.HeavyRain || weather.Current == WeatherType.Thunderstorm ||
                                weather.Current == WeatherType.Snow))
            count *= 0.5f;

        // Round with a random that's fixed for this species within this in-game hour, so counts don't flicker.
        int hourKey = time.TotalDays * 24 + time.Hour;
        var random = new System.Random(hourKey * 31 + speciesId.GetHashCode());
        return Mathf.FloorToInt(count + (float)random.NextDouble());
    }

    bool PondKnownAndNear()
    {
        DiscoveryManager discovery = DiscoveryManager.Instance;
        if (discovery == null || Player == null)
            return false;
        foreach (DiscoveryRecord record in discovery.Records)
        {
            if (record.displayName == pondSiteName)
                return Vector3.Distance(record.position, Player.transform.position) < 220f;
        }
        return false;
    }

    // --- Where ---

    bool FindSpawn(string speciesId, out Vector3 position)
    {
        Transform view = Player.CameraTransform != null ? Player.CameraTransform : Player.transform;
        Vector3 playerPos = Player.transform.position;
        bool small = speciesId == "rabbit" || speciesId == "squirrel";
        float min = small ? 30f : 60f, max = small ? 70f : 130f;

        for (int attempt = 0; attempt < 30; attempt++)
        {
            Vector2 dir = UnityEngine.Random.insideUnitCircle.normalized;
            Vector3 candidate = playerPos + new Vector3(dir.x, 0f, dir.y) * UnityEngine.Random.Range(min, max);

            if (speciesId == "waterfowl")
            {
                if (!PondSurface(candidate, out candidate))
                    continue;
            }
            else
            {
                if (!InsideProperty(candidate) || OnWater(candidate) || !HabitatFits(speciesId, candidate))
                    continue;
                candidate.y = terrain.SampleHeight(candidate) + terrain.transform.position.y;
            }

            // Out of view, so animals don't pop into existence in front of the player.
            Vector3 to = (candidate - view.position).normalized;
            if (Vector3.Dot(view.forward, to) > 0.35f)
                continue;

            position = candidate;
            return true;
        }

        position = Vector3.zero;
        return false;
    }

    bool HabitatFits(string speciesId, Vector3 at)
    {
        int near = TreesNear(at, 10f);
        switch (speciesId)
        {
            case "deer": return near <= 6;                          // edges and meadows
            case "turkey": return near >= 1 && near <= 8;            // forest openings
            case "rabbit": return near >= 1 && near <= 6;            // brush lines and field edges
            case "squirrel": return near >= 4;                       // woods
            default: return true;
        }
    }

    bool PondSurface(Vector3 near, out Vector3 surface)
    {
        surface = near;
        WaterSource water = FindAnyObjectByType<WaterSource>();
        if (water == null)
            return false;

        // Somewhere on the pond itself, not the creek.
        foreach (DiscoveryRecord record in DiscoveryManager.Instance.Records)
        {
            if (record.displayName != pondSiteName)
                continue;
            for (int i = 0; i < 12; i++)
            {
                Vector2 offset = UnityEngine.Random.insideUnitCircle * 30f;
                Vector3 p = record.position + new Vector3(offset.x, 20f, offset.y);
                if (Physics.Raycast(p, Vector3.down, out RaycastHit hit, 60f, ~0, QueryTriggerInteraction.Ignore) &&
                    hit.collider.GetComponentInParent<WaterSource>() != null &&
                    FishingManager.WaterAt(water, hit.point) == FishWater.Pond &&
                    Vector3.Distance(hit.point, Player.transform.position) > 25f)
                {
                    surface = hit.point;
                    return true;
                }
            }
        }
        return false;
    }

    bool InsideProperty(Vector3 p)
    {
        Vector3 local = p - terrain.transform.position;
        Vector3 size = terrain.terrainData.size;
        return local.x > 20f && local.z > 20f && local.x < size.x - 20f && local.z < size.z - 20f;
    }

    static bool OnWater(Vector3 at) =>
        Physics.Raycast(new Vector3(at.x, at.y + 30f, at.z), Vector3.down, out RaycastHit hit, 80f, ~0, QueryTriggerInteraction.Ignore) &&
        hit.collider.GetComponentInParent<WaterSource>() != null;

    // Tree counts on a coarse grid, for quick habitat checks.
    void BuildTreeGrid()
    {
        terrain = Terrain.activeTerrain;
        TerrainData data = terrain.terrainData;
        gridOrigin = terrain.transform.position;
        int w = Mathf.CeilToInt(data.size.x / Cell), h = Mathf.CeilToInt(data.size.z / Cell);
        treeGrid = new int[w, h];
        foreach (TreeInstance tree in data.treeInstances)
        {
            int x = Mathf.Clamp((int)(tree.position.x * data.size.x / Cell), 0, w - 1);
            int z = Mathf.Clamp((int)(tree.position.z * data.size.z / Cell), 0, h - 1);
            treeGrid[x, z]++;
        }
    }

    int TreesNear(Vector3 p, float radius)
    {
        int cx = (int)((p.x - gridOrigin.x) / Cell), cz = (int)((p.z - gridOrigin.z) / Cell);
        int r = Mathf.CeilToInt(radius / Cell);
        int count = 0;
        for (int x = cx - r; x <= cx + r; x++)
        {
            for (int z = cz - r; z <= cz + r; z++)
            {
                if (x >= 0 && z >= 0 && x < treeGrid.GetLength(0) && z < treeGrid.GetLength(1))
                    count += treeGrid[x, z];
            }
        }
        return count;
    }
}
