using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum TrapType { RabbitSnare, BoxTrap, FishTrap }

// Trapping_System.md's Catch Results, as stored until the player checks the trap.
public enum TrapResult { NoCatch, SmallCatch, SuccessfulCatch, TrapDamage, BaitConsumed }

[Serializable]
public class TrapState
{
    public int id;
    public TrapType type;
    public Vector3 position;
    public float yaw;
    [Range(0f, 1f)] public float condition = 1f;
    public string bait = "";          // Box Trap only
    public TrapResult result;         // what's waiting to be found
    public string catchItem = "";     // for a catch: the animal or fish caught
    public int catchCount;            // fish in a Fish Trap
    public float placement = 1f;      // how good the spot is, fixed when set
}

[Serializable]
public struct TrapSaveData
{
    public List<TrapState> traps;
    public int nextId;
}

// Trapping_System.md (Rabbit Snare, Box Trap) and Fishing_System.md's Fish Trap loop: place, (bait), wait, check,
// harvest. Traps work on the in-game clock hour by hour — including through day skips and sleep — rolling for a catch
// with odds from placement, season, bait and the trap's condition. Condition wears down every day a trap is out and
// faster when something damages it; a worn trap catches less and a broken one catches nothing until repaired.
// This manager owns every placed trap's state (saved) and spawns a Trap object for each in the World.
//
// All odds are Claude Code's first pass (2026-09-25), pending Mike's playtest:
//   Snare ~3.5%/hour (about a catch a day in good cover); Box Trap ~3%/hour baited, a fraction unbaited;
//   Fish Trap ~4%/hour, holding up to 3 fish. Placement 0.4-1.3: snares want brush and forest edges, box traps want
//   food nearby (forage patches), both do better near a discovered wildlife trail. Condition -0.1 a day.
public class TrapManager : MonoBehaviour, ISaveable
{
    public static TrapManager Instance { get; private set; }

    [Serializable]
    public class TrapPrefabs
    {
        public Trap rabbitSnare;
        public Trap boxTrap;
        public Trap fishTrap;
    }

    [SerializeField] TrapPrefabs prefabs = new TrapPrefabs();

    [Header("Odds (chance per in-game hour at full placement and condition)")]
    [SerializeField, Range(0f, 0.3f)] float snareChance = 0.035f;
    [SerializeField, Range(0f, 0.3f)] float boxChance = 0.03f;
    [Tooltip("Box Trap without bait, as a share of the baited chance.")]
    [SerializeField, Range(0f, 1f)] float unbaitedShare = 0.15f;
    [SerializeField, Range(0f, 0.3f)] float fishChance = 0.04f;
    [SerializeField, Min(1)] int fishTrapCapacity = 3;
    [Tooltip("Chance per hour that something takes a Box Trap's bait without being caught.")]
    [SerializeField, Range(0f, 0.2f)] float baitStolenChance = 0.006f;

    [Header("Wear")]
    [Tooltip("Condition lost per in-game day set out.")]
    [SerializeField, Range(0f, 1f)] float wearPerDay = 0.1f;
    [Tooltip("Condition lost when an animal damages the trap.")]
    [SerializeField, Range(0f, 1f)] float damageWear = 0.5f;

    public static readonly string[] BaitItems = { "wild_apples", "blackberries", "raspberries", "pawpaw", "wild_onion", "dandelion" };

    readonly List<TrapState> traps = new List<TrapState>();
    readonly Dictionary<int, Trap> views = new Dictionary<int, Trap>();
    int nextId = 1;
    bool subscribed;

    public IReadOnlyList<TrapState> Traps => traps;

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
        if (Instance != this)
            return;
        if (SaveManager.Instance != null)
            SaveManager.Instance.Register(this);
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.HourChanged += OnHour;
            subscribed = true;
        }
    }

    void OnDestroy()
    {
        if (Instance != this)
            return;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (subscribed && TimeManager.Instance != null)
            TimeManager.Instance.HourChanged -= OnHour;
        if (SaveManager.Instance != null)
            SaveManager.Instance.Unregister(this);
        Instance = null;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == GameManager.WorldScene)
            SpawnViews();
    }

    public static string ItemFor(TrapType type) =>
        type == TrapType.RabbitSnare ? "rabbit_snare" : type == TrapType.BoxTrap ? "box_trap" : "fish_trap";

    public static string NameOf(TrapType type) =>
        type == TrapType.RabbitSnare ? "Rabbit Snare" : type == TrapType.BoxTrap ? "Box Trap" : "Fish Trap";

    // --- Time passing ---

    void OnHour(int hour)
    {
        if (GameManager.Instance != null && !GameManager.Instance.InGame)
            return;

        foreach (TrapState trap in traps)
            Tick(trap);
    }

    void Tick(TrapState trap)
    {
        float before = trap.condition;
        trap.condition = Mathf.Max(0f, trap.condition - wearPerDay / 24f);
        bool changed = false;

        if (trap.condition > 0f && !Occupied(trap))
        {
            float effect = Mathf.Lerp(0.3f, 1f, trap.condition) * trap.placement * SeasonFactor();
            switch (trap.type)
            {
                case TrapType.RabbitSnare:
                    changed = RollLand(trap, snareChance * effect);
                    break;
                case TrapType.BoxTrap:
                    bool baited = !string.IsNullOrEmpty(trap.bait);
                    if (baited && UnityEngine.Random.value < baitStolenChance)
                    {
                        trap.bait = "";
                        trap.result = TrapResult.BaitConsumed;
                        changed = true;
                    }
                    else
                    {
                        changed = RollLand(trap, boxChance * effect * (baited ? BaitQuality(trap.bait) : unbaitedShare));
                    }
                    break;
                case TrapType.FishTrap:
                    changed = RollFish(trap, effect);
                    break;
            }
        }

        if (changed || (before > 0f && trap.condition <= 0f))
            Changed(trap);
    }

    // A snare or box trap already holding a catch, or a full fish trap, catches nothing more until emptied.
    bool Occupied(TrapState trap) =>
        trap.type == TrapType.FishTrap ? trap.catchCount >= fishTrapCapacity
                                       : trap.result == TrapResult.SuccessfulCatch || trap.result == TrapResult.SmallCatch;

    bool RollLand(TrapState trap, float chance)
    {
        if (UnityEngine.Random.value >= chance)
            return false;

        // Something came through: usually a catch, sometimes a poor one, sometimes the trap gets wrecked.
        float roll = UnityEngine.Random.value;
        trap.catchItem = UnityEngine.Random.value < SquirrelShare(trap) ? "squirrel" : "rabbit";
        if (roll < 0.1f)
        {
            trap.result = TrapResult.TrapDamage;
            trap.condition = Mathf.Max(0f, trap.condition - damageWear);
            trap.catchItem = "";
        }
        else if (roll < 0.3f)
        {
            trap.result = TrapResult.SmallCatch;
        }
        else
        {
            trap.result = TrapResult.SuccessfulCatch;
        }

        if (trap.type == TrapType.BoxTrap)
            trap.bait = ""; // eaten either way
        return true;
    }

    bool RollFish(TrapState trap, float effect)
    {
        FishingManager fishing = FishingManager.Instance;
        if (fishing == null)
            return false;
        float chance = fishing.TrapHourlyChance(fishChance) * effect;
        if (UnityEngine.Random.value >= chance)
            return false;

        FishingManager.Species fish = fishing.PickSpecies(FishWaterAt(trap.position), false, true);
        if (fish == null)
            return false;

        // A trap holds one kind at a time in this first pass: a new species replaces what's there only if it's empty.
        if (trap.catchCount == 0 || trap.catchItem == fish.itemId)
        {
            trap.catchItem = fish.itemId;
            trap.catchCount++;
            trap.result = TrapResult.SuccessfulCatch;
            return true;
        }
        return false;
    }

    static FishWater FishWaterAt(Vector3 position)
    {
        WaterSource source = FindAnyObjectByType<WaterSource>();
        return source != null ? FishingManager.WaterAt(source, position) : FishWater.Creek;
    }

    // Squirrels in the woods, rabbits in brush and field edges (Small_Game.md: interchangeable yields).
    static float SquirrelShare(TrapState trap) => trap.placement > 1f ? 0.35f : 0.5f;

    static float SeasonFactor()
    {
        TimeManager time = TimeManager.Instance;
        if (time == null)
            return 1f;
        switch (time.CurrentSeason)
        {
            case Season.Spring: return 1.1f;   // increased animal movement
            case Season.Fall: return 1.3f;     // excellent trapping, animals actively feeding
            case Season.Winter: return 0.6f;   // lower activity
            default: return 1f;
        }
    }

    // Trapping_System.md: bait quality affects success rates (apples best, greens weakest).
    static float BaitQuality(string bait)
    {
        switch (bait)
        {
            case "wild_apples": return 1.25f;
            case "pawpaw": return 1.15f;
            case "blackberries":
            case "raspberries": return 1f;
            default: return 0.75f;
        }
    }

    // --- Placing ---

    // How good a spot is for this trap, 0.4-1.3, from the trees around it, nearby food and wildlife trails.
    public static float PlacementAt(TrapType type, Vector3 position)
    {
        if (type == TrapType.FishTrap)
            return 1f;

        int trees = TreesNear(position, 10f);
        float quality;
        if (type == TrapType.RabbitSnare)
            quality = trees == 0 ? 0.45f : trees <= 4 ? 1.1f : 0.8f; // brush lines and edges beat open field and deep woods
        else
            quality = trees == 0 ? 0.6f : 0.9f;

        foreach (ForageNode node in FindObjectsByType<ForageNode>(FindObjectsSortMode.None))
        {
            if (type == TrapType.BoxTrap && Vector3.Distance(node.transform.position, position) < 25f)
            {
                quality += 0.3f; // near food sources
                break;
            }
        }

        DiscoveryManager discovery = DiscoveryManager.Instance;
        if (discovery != null)
        {
            foreach (DiscoveryRecord record in discovery.Records)
            {
                if (record.category == DiscoveryCategory.Wildlife && Vector3.Distance(record.position, position) < 35f)
                {
                    quality += 0.2f; // an animal trail the player has found
                    break;
                }
            }
        }

        return Mathf.Clamp(quality, 0.4f, 1.3f);
    }

    static int TreesNear(Vector3 position, float radius)
    {
        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null)
            return 0;
        TerrainData data = terrain.terrainData;
        Vector3 origin = terrain.transform.position;
        int count = 0;
        foreach (TreeInstance tree in data.treeInstances)
        {
            float x = origin.x + tree.position.x * data.size.x, z = origin.z + tree.position.z * data.size.z;
            if ((x - position.x) * (x - position.x) + (z - position.z) * (z - position.z) < radius * radius)
                count++;
        }
        return count;
    }

    public static string PlacementWord(float placement) =>
        placement >= 1.1f ? "excellent" : placement >= 0.85f ? "good" : placement >= 0.6f ? "fair" : "poor";

    // Sets a trap from the player's pack at a spot. Returns false if they don't carry one.
    public bool Place(TrapType type, Vector3 position, float yaw)
    {
        InventoryManager inventory = InventoryManager.Instance;
        if (inventory == null || inventory.RemoveFromPlayer(ItemFor(type), 1) != 1)
            return false;

        var trap = new TrapState
        {
            id = nextId++,
            type = type,
            position = position,
            yaw = yaw,
            condition = 1f,
            result = TrapResult.NoCatch,
            placement = PlacementAt(type, position),
        };
        traps.Add(trap);
        SpawnView(trap);
        return true;
    }

    // --- Checking ---

    // Takes what's in the trap. Returns a message describing the check (Trapping_System.md's Catch Results).
    public string Check(TrapState trap)
    {
        InventoryManager inventory = InventoryManager.Instance;
        string message;

        if (trap.type == TrapType.FishTrap)
        {
            if (trap.catchCount <= 0)
                return "Empty — no catch yet";

            FishingManager.Species fish = FishingManager.Instance != null ? FishingManager.Instance.Get(trap.catchItem) : null;
            int units = fish != null ? fish.yield * trap.catchCount : trap.catchCount;
            int added = inventory != null ? inventory.AddToPlayer(trap.catchItem, units) : 0;
            string name = ItemDatabase.Get(trap.catchItem)?.DisplayName ?? trap.catchItem;
            if (added <= 0)
                return $"{trap.catchCount} {name} in the trap — no room to carry them";
            message = added < units ? $"Took {added} of {units} {name} — no room for the rest" : $"{trap.catchCount} {name} in the trap  (+{added})";
            trap.catchCount = 0;
            trap.catchItem = "";
            trap.result = TrapResult.NoCatch;
            Changed(trap);
            return message;
        }

        switch (trap.result)
        {
            case TrapResult.SuccessfulCatch:
            case TrapResult.SmallCatch:
            {
                bool full = trap.result == TrapResult.SuccessfulCatch;
                string animal = trap.catchItem == "squirrel" ? "Squirrel" : "Rabbit";
                int meat = inventory != null ? inventory.AddToPlayer("small_game_meat", 1) : 0;
                int fur = full && inventory != null ? inventory.AddToPlayer("small_furs", 1) : 0;
                if (meat == 0 && fur == 0)
                    return $"A {animal} in the trap — no room to carry it";
                message = full ? $"Successful catch — a {animal}  (+meat, +fur)" : $"Small catch — a {animal}, fur torn  (+meat)";
                break;
            }
            case TrapResult.TrapDamage:
                message = "Trap damaged — something broke free";
                break;
            case TrapResult.BaitConsumed:
                message = "Bait consumed — something took it and got away";
                break;
            default:
                message = trap.condition <= 0f ? "Broken — needs repair before it'll catch anything" : "No catch";
                break;
        }

        trap.result = TrapResult.NoCatch;
        trap.catchItem = "";
        Changed(trap);
        return message;
    }

    public bool NeedsBait(TrapState trap) => trap.type == TrapType.BoxTrap && string.IsNullOrEmpty(trap.bait);

    // The best bait the player is carrying, or null.
    public static string CarriedBait()
    {
        InventoryManager inventory = InventoryManager.Instance;
        if (inventory == null)
            return null;
        foreach (string bait in BaitItems)
        {
            if (inventory.Player.Has(bait))
                return bait;
        }
        return null;
    }

    public bool Bait(TrapState trap)
    {
        string bait = CarriedBait();
        if (!NeedsBait(trap) || bait == null || InventoryManager.Instance.RemoveFromPlayer(bait, 1) != 1)
            return false;
        trap.bait = bait;
        Changed(trap);
        return true;
    }

    // Repair materials (Trapping_System.md: players must repair traps).
    public static string RepairItem(TrapType type) => type == TrapType.RabbitSnare ? "cordage" : "firewood";

    public bool CanRepair(TrapState trap) =>
        trap.condition < 0.95f && InventoryManager.Instance != null && InventoryManager.Instance.Player.Has(RepairItem(trap.type));

    public bool Repair(TrapState trap)
    {
        if (!CanRepair(trap) || InventoryManager.Instance.RemoveFromPlayer(RepairItem(trap.type), 1) != 1)
            return false;
        trap.condition = 1f;
        if (trap.result == TrapResult.TrapDamage)
            trap.result = TrapResult.NoCatch;
        Changed(trap);
        return true;
    }

    // Picks the trap back up into the pack (any catch or bait is lost with it only if the pack is full).
    public bool PickUp(TrapState trap)
    {
        InventoryManager inventory = InventoryManager.Instance;
        if (inventory == null || inventory.AddToPlayer(ItemFor(trap.type), 1) != 1)
            return false;

        traps.Remove(trap);
        if (views.TryGetValue(trap.id, out Trap view) && view != null)
            Destroy(view.gameObject);
        views.Remove(trap.id);
        return true;
    }

    void Changed(TrapState trap)
    {
        if (views.TryGetValue(trap.id, out Trap view) && view != null)
            view.Refresh();
    }

    // --- World objects ---

    void SpawnViews()
    {
        foreach (Trap view in views.Values)
        {
            if (view != null)
                Destroy(view.gameObject);
        }
        views.Clear();

        if (!SceneManager.GetSceneByName(GameManager.WorldScene).isLoaded)
            return;
        foreach (TrapState trap in traps)
            SpawnView(trap);
    }

    void SpawnView(TrapState trap)
    {
        Trap prefab = trap.type == TrapType.RabbitSnare ? prefabs.rabbitSnare : trap.type == TrapType.BoxTrap ? prefabs.boxTrap : prefabs.fishTrap;
        if (prefab == null)
        {
            Debug.LogError($"[TrapManager] No prefab for {trap.type}.", this);
            return;
        }

        Trap view = Instantiate(prefab, trap.position, Quaternion.Euler(0f, trap.yaw, 0f));
        view.name = $"{NameOf(trap.type)} {trap.id}";
        view.Bind(trap);
        views[trap.id] = view;
    }

    // No traps — used when starting a new game.
    public void ResetTraps()
    {
        traps.Clear();
        nextId = 1;
        SpawnViews();
    }

    public TrapSaveData CaptureState() => new TrapSaveData { traps = new List<TrapState>(traps), nextId = nextId };

    public void RestoreState(TrapSaveData data)
    {
        traps.Clear();
        if (data.traps != null)
            traps.AddRange(data.traps);
        nextId = Mathf.Max(1, data.nextId);
        SpawnViews();
    }

    // Save_Data_Model.md's World Block.
    string ISaveable.SaveFile => "world";
    string ISaveable.SaveKey => "traps";
    object ISaveable.CaptureState() => CaptureState();
    void ISaveable.RestoreState(string json) => RestoreState(JsonUtility.FromJson<TrapSaveData>(json));
}
