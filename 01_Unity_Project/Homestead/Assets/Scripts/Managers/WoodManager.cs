using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class WoodStack
{
    public string itemId;
    public int count;
    // The in-game day a perishable batch was acquired, kept while it's stored so the Spoilage System can still age it.
    public int day;
}

[Serializable]
public class FelledTree
{
    public int index;         // the tree's place in the terrain's original tree list
    public bool stump;        // hardwoods leave a stump; a cut shrub leaves nothing
}

// Felled: what a felled tree left, gone once emptied. The rest are player-built storage that stay put, empty or not:
// Wood_Gathering_System.md's Wood Pile and Rock Pile, and Primitive_Storage_System.md's Water Barrel, Food Cache,
// Storage Bin and Tool Rack. Tent, LeanTo and Cabin aren't storage but shelters to sleep in (Building_Housing_System.md's
// Sleep System and Small Cabin), placed and saved the same way. CabinSite is a Wood-Pile-style accumulator — deposited
// into over multiple trips, then converted in place into a real Cabin once fully stocked (WoodManager.CompleteCabin).
// TarpShelter is a pitched Tarp (Mike, 2026-10-09): a shelter like the Tent, but the cheapest tier.
// (Saved as numbers, so new kinds go on the end.)
public enum PileKind { Felled, WoodStorage, RockStorage, WaterBarrel, FoodCache, StorageBin, Tent, LeanTo, Cabin, CabinSite, ToolRack, TarpShelter }

[Serializable]
public class WoodPileState
{
    public int id;
    public PileKind kind;
    public Vector3 position;
    public float yaw;
    public List<WoodStack> contents = new List<WoodStack>();

    public int Count(string itemId)
    {
        int total = 0;
        foreach (WoodStack stack in contents)
            if (stack.itemId == itemId)
                total += stack.count;
        return total;
    }

    public int Total
    {
        get
        {
            int total = 0;
            foreach (WoodStack stack in contents)
                total += stack.count;
            return total;
        }
    }

    // Adds to the batch from the same day (perishables stay apart by day; everything else is day 0).
    public void Add(string itemId, int count, int day = 0)
    {
        if (count <= 0)
            return;
        foreach (WoodStack stack in contents)
        {
            if (stack.itemId == itemId && stack.day == day)
            {
                stack.count += count;
                return;
            }
        }
        contents.Add(new WoodStack { itemId = itemId, count = count, day = day });
    }

    public void Remove(string itemId, int count) => Take(itemId, count);

    // Takes up to count, oldest batches first, and returns what was taken as (day, count) batches.
    public List<WoodStack> Take(string itemId, int count)
    {
        var taken = new List<WoodStack>();
        while (count > 0)
        {
            int index = -1;
            for (int i = 0; i < contents.Count; i++)
                if (contents[i].itemId == itemId && (index < 0 || contents[i].day < contents[index].day))
                    index = i;
            if (index < 0)
                break;
            WoodStack stack = contents[index];
            int n = Mathf.Min(count, stack.count);
            stack.count -= n;
            count -= n;
            taken.Add(new WoodStack { itemId = itemId, count = n, day = stack.day });
            if (stack.count <= 0)
                contents.RemoveAt(index);
        }
        return taken;
    }

    public bool IsEmpty => contents.Count == 0;
    public bool IsRock => kind == PileKind.RockStorage;
    public bool IsShelter => kind == PileKind.Tent || kind == PileKind.LeanTo || kind == PileKind.Cabin || kind == PileKind.TarpShelter;
    public bool IsWoodPile => kind == PileKind.Felled || kind == PileKind.WoodStorage;
    public bool IsBuilt => kind != PileKind.Felled;

    // What this pile will take in: each kind of storage takes only its own category (Primitive_Storage_System.md).
    // A CabinSite only takes what Small Cabin still needs — once a material reaches its required amount, it stops
    // accepting more of that one (same idea as the Water Barrel's litre cap, just tracked per material instead of
    // one shared total).
    public bool Accepts(string itemId)
    {
        switch (kind)
        {
            case PileKind.RockStorage: return itemId == WoodManager.StoneId;
            case PileKind.WaterBarrel: return WoodManager.IsWater(itemId);
            case PileKind.FoodCache: return WoodManager.IsStorableFood(itemId);
            case PileKind.StorageBin: return WoodManager.IsDryGoods(itemId);
            case PileKind.ToolRack: return WoodManager.IsTool(itemId);
            case PileKind.CabinSite:
                return WoodManager.Instance != null && Array.IndexOf(WoodManager.CabinMaterialIds, itemId) >= 0 &&
                       Count(itemId) < WoodManager.Instance.CabinRequired(itemId);
            case PileKind.Tent:
            case PileKind.LeanTo:
            case PileKind.TarpShelter:
            case PileKind.Cabin: return false;
            default: return WoodManager.IsWood(itemId);
        }
    }
}

[Serializable]
public struct WoodSaveData
{
    public List<FelledTree> felled;
    public List<WoodPileState> piles;
    public int nextPileId;
    public float snowLoadHours;
    public float stormAfterglow;
}

// Wood_Gathering_System.md (2026-09-26): trees felled with the Axe, and the wood they leave. The World's trees are
// terrain trees, so felling one takes it out of RuntimeTerrain's play-time copy of the terrain data — the terrain asset
// itself is never touched — and remembers it by its place in the original tree list, so it stays down across saves.
// Hardwoods leave a stump (a small object in the World); every felled tree leaves a wood pile at its base holding its
// Logs, Branches and Sticks, which the player carries off as they can (Logs are heavy) and can split into Firewood
// right there. Piles last until emptied.
//
// Primitive Storage (2026-09-26): the player can also build storage just in front of them from the Inventory and
// store things in it (R) and take them back (E) — the same pile as a felled tree's, made deliberate:
//   Wood Pile — 4 Sticks for a ground frame; wood.        Rock Pile — free; Stone.
//   Water Barrel — 3 Logs; up to 40 L of water, poured in from the Bucket and drawn off into it.
//   Food Cache — 2 Logs, 4 Branches; food, keeping each batch's age.   Storage Bin — 6 Branches, 6 Sticks; the rest.
    //   Tool Rack — 4 Branches, 4 Sticks; any Tool.
// Only the barrel has a limit; they're the stopgap until Building's storage buildings exist. No spoilage bonus.
//
// Shelters (Sleep System, 2026-09-26) are placed the same way: a Tent, from the one the player carries (not craftable —
// a starting-kit item), or a Lean-To from 8 Branches, 4 Sticks and 1 Cordage. Sleeping in one is Shelter's job; a Tent
// packs back up, a Lean-To comes down for some of its Branches. A Tarp Shelter pitches the Tarp the player carries
// (with 2 Sticks and 1 Cordage) and takes down for the Tarp and half the Sticks.
//
// Small Cabin (Building_Housing_System.md's first permanent residence, 2026-09-26): a two-phase build, not a one-trip
// BuildPile like everything above — Mike's call, so a first permanent residence costs real time and multiple trips
// rather than fitting in a single carry. Phase one places an empty CabinSite (free, no Hammer needed, but the same
// bigger clear site as the finished building — cabinFootprint, checked against both other piles and standing trees or
// un-dug stumps, the "automatic check the spot's clear" half of Site Preparation — and a bigger build distance so its
// footprint doesn't land on the player). Phase two is ordinary Wood-Pile storing (WoodPile/R and E), capped per
// material at what Small Cabin still needs (WoodPileState.Accepts, CabinRequired) — visibly growing as it fills, and
// freely takeable back out, nothing locked in early. Once every material's at its required amount, CompleteCabin (only
// with the Hammer equipped — the tool finally gates something, same "equip the tool for the job" pattern as the
// Axe/Pick Axe/Shovel) swaps the site in place for the real thing: permanent, no take-down unlike Tent/Lean-To, and
// furnished with a hearth — FireManager.BuildFurnished drops an unlit, unfuelled campfire inside it, on a stone slab, so
// Warmth-by-proximity and Cooking work exactly as they do at any other campfire, no new mechanic needed. Sleeping in
// it (Shelter) is the best tier yet, above the Tent.
//
// Stumps come out with the Primitive Shovel (AxeTool), leaving a Firewood's worth of root wood.
//
// Windthrow (Natural Tree Fall, 2026-09-26): standing hardwoods also come down on their own, anywhere on the property,
// exactly as if felled — pile, stump and crash — falling with the wind. The chance is rolled each in-game hour: rare
// on an ordinary day (about one a month), much likelier in a Thunderstorm (about one every eight storm hours) and for
// half a day after, and in late Winter and early Spring in proportion to how much snow fell that Winter.
//
// Yields scale with the tree's size and are Claude Code's first proposal, pending Mike's playtest:
//   broad hardwood (6.5 m) — about 2 Logs, 4 Branches, 5 Sticks; tall hardwood (9.5 m) — about 3 Logs, 3 Branches,
//   4 Sticks; understory shrub — 1 Branch, about 3 Sticks. Trees don't grow back yet.
public class WoodManager : MonoBehaviour, ISaveable
{
    public const string LogsId = "logs";
    public const string BranchesId = "branches";
    public const string SticksId = "sticks";
    public const string StoneId = "stone";
    public const string TallGrassId = "tall_grass";
    public const string ClayId = "clay";
    public const string HammerId = "hammer";

    public static readonly string[] CabinMaterialIds = { LogsId, BranchesId, TallGrassId, StoneId, ClayId };

    public static readonly string[] WoodIds = { LogsId, BranchesId, SticksId, FireManager.FirewoodId };
    public static bool IsWood(string itemId) => Array.IndexOf(WoodIds, itemId) >= 0;

    public static bool IsWater(string itemId) => WaterQualities.IsRawWater(itemId) || itemId == Cooking.PurifiedWaterId;

    // Food for the Food Cache: anything with a food value that isn't water.
    public static bool IsStorableFood(string itemId)
    {
        ItemDefinition item = ItemDatabase.Get(itemId);
        return item != null && item.IsFood && !IsWater(itemId);
    }

    // Everything else that isn't a tool: Cordage, hides, furs, feathers, arrows, rounds...
    public static bool IsDryGoods(string itemId)
    {
        ItemDefinition item = ItemDatabase.Get(itemId);
        return item != null && item.Category != ItemCategory.Tool && !IsWood(itemId) && itemId != StoneId &&
               !IsWater(itemId) && !IsStorableFood(itemId);
    }

    // Tools for the Tool Rack: Axe, Rifle, Bow, Fishing Rod, Bucket, Fish Trap, Pouch, and so on.
    public static bool IsTool(string itemId) => ItemDatabase.Get(itemId)?.Category == ItemCategory.Tool;

    // Terrain tree prototypes (PropertyTerrainBuilder): 0 broad hardwood, 1 tall hardwood, 2 understory shrub.
    public const int BroadHardwood = 0, TallHardwood = 1, Shrub = 2;

    public static WoodManager Instance { get; private set; }

    [Tooltip("Trunk radius of each prototype at scale 1 (matches the prefabs' capsule colliders).")]
    [SerializeField] float[] trunkRadius = { 0.3f, 0.27f, 0.5f };
    [Tooltip("How long the felled tree takes to fall, then lies before sinking away (seconds).")]
    [SerializeField, Min(0.1f)] float fallSeconds = 1.8f;
    [SerializeField, Min(0f)] float lieSeconds = 3f;

    [Header("Building storage piles")]
    [Tooltip("Sticks it takes to lay out a Wood Pile's frame. A Rock Pile is free.")]
    [SerializeField, Min(0)] int woodPileSticks = 4;
    [SerializeField, Min(1)] int barrelLitres = 40;
    [SerializeField, Min(0.5f)] float buildDistance = 1.8f;
    [Tooltip("Piles and campfires can't be built closer together than this (metres).")]
    [SerializeField, Min(0f)] float minSpacing = 2f;
    [SerializeField, Range(0f, 60f)] float maxSlope = 30f;

    // Quantities went back to the original, more substantial first pass once building moved to a staged CabinSite
    // (Mike's call, 2026-09-27): a single-trip 35 kg recipe made a first permanent residence cost less than a Lean-To.
    // Nothing here needs to fit in one carry any more, so it's sized for what a real log cabin's worth of materials
    // should feel like, not what 45 kg allows.
    [Header("Small Cabin")]
    [SerializeField, Min(1)] int cabinLogs = 20;
    [SerializeField, Min(1)] int cabinBranches = 10;
    [SerializeField, Min(1)] int cabinTallGrass = 15;
    [Tooltip("Stone for the whole cabin — its own foundation plus the stove's extra hearth stone.")]
    [SerializeField, Min(1)] int cabinStone = 12;
    [Tooltip("Clay for the stove's hearth, mortaring the stone under the indoor fire.")]
    [SerializeField, Min(1)] int cabinClay = 6;
    [Tooltip("How far in front of the player a Small Cabin site goes — further than other piles so its footprint clears them.")]
    [SerializeField, Min(1f)] float cabinBuildDistance = 3.5f;
    [Tooltip("A Small Cabin's clearance: kept clear of other piles and standing trees, checked automatically as its Site Preparation.")]
    [SerializeField, Min(1f)] float cabinFootprint = 3.2f;
    [Tooltip("How close the player needs to be to a CabinSite to complete it from the Inventory screen (metres).")]
    [SerializeField, Min(1f)] float cabinCompleteReach = 6f;

    readonly List<FelledTree> felled = new List<FelledTree>();
    readonly HashSet<int> felledSet = new HashSet<int>();
    readonly List<WoodPileState> piles = new List<WoodPileState>();
    readonly Dictionary<int, GameObject> pileViews = new Dictionary<int, GameObject>();
    readonly List<GameObject> stumps = new List<GameObject>();
    int nextPileId = 1;

    // The World's terrain while it's loaded.
    Terrain terrain;
    TerrainData data;
    TreeInstance[] originalTrees;
    readonly Dictionary<Vector2Int, List<int>> grid = new Dictionary<Vector2Int, List<int>>();
    const float CellSize = 4f;

    public event Action<WoodPileState> PileChanged;

    public IReadOnlyList<WoodPileState> Piles => piles;
    public int WoodPileSticks => woodPileSticks;
    public int BarrelLitres => barrelLitres;

    [Header("Windthrow (chance per in-game hour)")]
    [SerializeField, Min(0f)] float windthrowBase = 1f / 720f;
    [SerializeField, Min(0f)] float windthrowStorm = 0.12f;
    [SerializeField, Min(0f)] float windthrowAfterStorm = 0.03f;
    [Tooltip("Hours after a Thunderstorm that trees stay likelier to fall.")]
    [SerializeField, Min(0f)] float afterStormHours = 12f;
    [Tooltip("Extra chance at a full Winter's snow load, and the snow hours that count as full.")]
    [SerializeField, Min(0f)] float windthrowSnowLoad = 0.03f;
    [SerializeField, Min(1f)] float fullSnowLoadHours = 120f;
    [Tooltip("Trees closer than this to the player don't blow down (metres).")]
    [SerializeField, Min(0f)] float windthrowClearance = 6f;

    float snowLoadHours;   // snow that fell this Winter, melting off through Spring
    float stormAfterglow;  // hours left of the after-storm risk
    int windthrowCount;
    public int WindthrowCount => windthrowCount;
    public int FelledCount => felled.Count;

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
        if (scene.name != GameManager.WorldScene)
            return;

        terrain = Terrain.activeTerrain;
        data = null;
        originalTrees = null;
        if (terrain != null && terrain.terrainData != null)
        {
            data = RuntimeTerrain.Data(terrain); // a fresh copy each time World loads, with every tree standing
            originalTrees = data.treeInstances;
        }
        Apply();
    }

    bool WorldLoaded => terrain != null && data != null && originalTrees != null;

    // Takes every felled tree out of the terrain and (re)spawns stumps and piles.
    void Apply()
    {
        foreach (GameObject stump in stumps)
            if (stump != null)
                Destroy(stump);
        stumps.Clear();
        foreach (GameObject view in pileViews.Values)
            if (view != null)
                Destroy(view);
        pileViews.Clear();

        if (!WorldLoaded)
            return;

        RebuildTrees();
        foreach (FelledTree tree in felled)
            if (tree.stump)
                SpawnStump(tree.index);
        foreach (WoodPileState pile in piles)
            SpawnPile(pile);
    }

    // The terrain's trees minus the felled ones, and the grid used to find the tree being looked at.
    void RebuildTrees()
    {
        var live = new List<TreeInstance>(originalTrees.Length);
        grid.Clear();
        for (int i = 0; i < originalTrees.Length; i++)
        {
            if (felledSet.Contains(i))
                continue;
            live.Add(originalTrees[i]);
            Vector3 world = WorldPosition(i);
            var cell = new Vector2Int(Mathf.FloorToInt(world.x / CellSize), Mathf.FloorToInt(world.z / CellSize));
            if (!grid.TryGetValue(cell, out List<int> list))
                grid[cell] = list = new List<int>();
            list.Add(i);
        }
        data.SetTreeInstances(live.ToArray(), true);

        // The terrain collider only picks up tree changes when it's rebuilt.
        if (terrain.TryGetComponent(out TerrainCollider terrainCollider))
        {
            terrainCollider.enabled = false;
            terrainCollider.enabled = true;
        }
        OverheadCover.Refresh();
    }

    public Vector3 WorldPosition(int index)
    {
        Vector3 local = Vector3.Scale(originalTrees[index].position, data.size);
        Vector3 world = terrain.transform.position + local;
        world.y = terrain.SampleHeight(world) + terrain.transform.position.y;
        return world;
    }

    public int Prototype(int index) => originalTrees[index].prototypeIndex;
    public float HeightScale(int index) => originalTrees[index].heightScale;
    public float WidthScale(int index) => originalTrees[index].widthScale;

    public float TrunkRadius(int index)
    {
        int prototype = Prototype(index);
        float radius = prototype < trunkRadius.Length ? trunkRadius[prototype] : 0.3f;
        return radius * WidthScale(index);
    }

    // The standing tree whose trunk is nearest a point (horizontally), within reach of its bark. -1 if none.
    public int FindTree(Vector3 point, float reach)
    {
        if (!WorldLoaded)
            return -1;

        int best = -1;
        float bestDistance = float.MaxValue;
        var centre = new Vector2Int(Mathf.FloorToInt(point.x / CellSize), Mathf.FloorToInt(point.z / CellSize));
        for (int dz = -1; dz <= 1; dz++)
        for (int dx = -1; dx <= 1; dx++)
        {
            if (!grid.TryGetValue(new Vector2Int(centre.x + dx, centre.y + dz), out List<int> list))
                continue;
            foreach (int index in list)
            {
                Vector3 tree = WorldPosition(index);
                float distance = new Vector2(point.x - tree.x, point.z - tree.z).magnitude - TrunkRadius(index);
                if (distance <= reach && distance < bestDistance && point.y > tree.y - 0.5f && point.y < tree.y + 4f)
                {
                    best = index;
                    bestDistance = distance;
                }
            }
        }
        return best;
    }

    public bool IsStanding(int index) => WorldLoaded && index >= 0 && index < originalTrees.Length && !felledSet.Contains(index);

    public static string TreeName(int prototype) =>
        prototype == Shrub ? "Shrub" : prototype == TallHardwood ? "Tall Hardwood" : "Hardwood";

    // What a tree yields when felled, scaled by its size.
    public List<WoodStack> YieldOf(int index)
    {
        float size = HeightScale(index) * Mathf.Sqrt(WidthScale(index));
        var yield = new List<WoodStack>();
        switch (Prototype(index))
        {
            case BroadHardwood:
                yield.Add(new WoodStack { itemId = LogsId, count = Mathf.Max(1, Mathf.RoundToInt(2f * size)) });
                yield.Add(new WoodStack { itemId = BranchesId, count = Mathf.Max(1, Mathf.RoundToInt(4f * size)) });
                yield.Add(new WoodStack { itemId = SticksId, count = Mathf.Max(1, Mathf.RoundToInt(5f * size)) });
                break;
            case TallHardwood:
                yield.Add(new WoodStack { itemId = LogsId, count = Mathf.Max(1, Mathf.RoundToInt(3f * size)) });
                yield.Add(new WoodStack { itemId = BranchesId, count = Mathf.Max(1, Mathf.RoundToInt(3f * size)) });
                yield.Add(new WoodStack { itemId = SticksId, count = Mathf.Max(1, Mathf.RoundToInt(4f * size)) });
                break;
            default:
                yield.Add(new WoodStack { itemId = BranchesId, count = 1 });
                yield.Add(new WoodStack { itemId = SticksId, count = Mathf.Max(1, Mathf.RoundToInt(3f * size)) });
                break;
        }
        return yield;
    }

    // Fells a standing tree, falling away from where the player stands: it topples, and a wood pile holding its
    // yield appears at its base.
    public void Fell(int index, Vector3 from)
    {
        if (!IsStanding(index))
            return;

        int prototype = Prototype(index);
        Vector3 basePosition = WorldPosition(index);
        Vector3 away = Vector3.ProjectOnPlane(basePosition - from, Vector3.up);
        if (away.sqrMagnitude < 0.01f)
            away = Vector3.forward;
        away.Normalize();

        TreeInstance instance = originalTrees[index];
        GameObject prefab = prototype < data.treePrototypes.Length ? data.treePrototypes[prototype].prefab : null;

        felled.Add(new FelledTree { index = index, stump = prototype != Shrub });
        felledSet.Add(index);
        RebuildTrees();
        if (prototype != Shrub)
            SpawnStump(index);
        if (prefab != null)
            FallingTree.Spawn(prefab, basePosition, instance, away, prototype == Shrub ? 0.6f : fallSeconds,
                              prototype == Shrub ? 0.5f : lieSeconds, crash: prototype != Shrub);

        // The pile lands just beside the trunk on the side it fell toward.
        Vector3 side = Vector3.Cross(Vector3.up, away);
        // Far enough along that its heap doesn't cover the stump.
        Vector3 pilePosition = basePosition + away * (TrunkRadius(index) + 1.8f) + side * 0.5f;
        pilePosition.y = terrain.SampleHeight(pilePosition) + terrain.transform.position.y;
        var pile = new WoodPileState
        {
            id = nextPileId++,
            position = pilePosition,
            yaw = Quaternion.LookRotation(away).eulerAngles.y,
            contents = YieldOf(index),
        };
        piles.Add(pile);
        SpawnPile(pile);
    }

    // --- Windthrow ---

    void Update()
    {
        TimeManager time = TimeManager.Instance;
        bool playing = GameManager.Instance == null || GameManager.Instance.State == GameState.Playing;
        if (time == null || !time.IsRunning || !playing || !WorldLoaded)
            return;
        if (Time.unscaledTime >= nextHearthCheck)
        {
            nextHearthCheck = Time.unscaledTime + 1f;
            MigrateHearths();
        }
        PassHours(Time.deltaTime * time.GameHoursPerRealSecond);
    }

    // The weather's chance of bringing a tree down over some in-game time (also for hours slept, once Sleep exists).
    public void PassHours(float hours)
    {
        if (hours <= 0f || !WorldLoaded)
            return;

        WeatherManager weather = WeatherManager.Instance;
        TimeManager time = TimeManager.Instance;
        bool storm = weather != null && weather.Current == WeatherType.Thunderstorm;
        if (storm)
            stormAfterglow = afterStormHours;
        else
            stormAfterglow = Mathf.Max(0f, stormAfterglow - hours);

        bool winter = time != null && time.CurrentSeason == Season.Winter;
        if (winter && weather != null && weather.IsSnowing)
            snowLoadHours += hours;
        else if (!winter)
            snowLoadHours = Mathf.Max(0f, snowLoadHours - hours); // melts off through the first days of Spring

        float rate = windthrowBase
                   + (storm ? windthrowStorm : stormAfterglow > 0f ? windthrowAfterStorm : 0f)
                   + windthrowSnowLoad * Mathf.Clamp01(snowLoadHours / fullSnowLoadHours);
        if (UnityEngine.Random.value < 1f - Mathf.Exp(-rate * hours))
            Windthrow();
    }

    // Brings down a random standing hardwood somewhere on the property, falling with the wind.
    public bool Windthrow()
    {
        if (!WorldLoaded)
            return false;
        PlayerController player = FindAnyObjectByType<PlayerController>();
        for (int attempt = 0; attempt < 30; attempt++)
        {
            int index = UnityEngine.Random.Range(0, originalTrees.Length);
            if (!IsStanding(index) || Prototype(index) == Shrub)
                continue;
            Vector3 position = WorldPosition(index);
            if (player != null && Vector3.Distance(player.transform.position, position) < windthrowClearance)
                continue;

            float windYaw = (WeatherManager.Instance != null ? WeatherManager.Instance.WindDirection : 0f) +
                            UnityEngine.Random.Range(-35f, 35f);
            Vector3 downwind = Quaternion.Euler(0f, windYaw, 0f) * Vector3.forward;
            Fell(index, position - downwind);
            windthrowCount++;
            return true;
        }
        return false;
    }

    // --- Piles ---

    public void NotifyChanged(WoodPileState pile)
    {
        if (pile.IsEmpty && !pile.IsBuilt)
        {
            RemovePile(pile);
            return;
        }
        if (pileViews.TryGetValue(pile.id, out GameObject view) && view != null && view.TryGetComponent(out WoodPile woodPile))
            woodPile.Rebuild();
        PileChanged?.Invoke(pile);
    }

    // Takes a pile or shelter out of the world for good (emptied, packed up, taken down).
    public void RemovePile(WoodPileState pile)
    {
        piles.Remove(pile);
        if (pileViews.TryGetValue(pile.id, out GameObject view) && view != null)
            Destroy(view);
        pileViews.Remove(pile.id);
        PileChanged?.Invoke(pile);
    }

    // --- Building storage piles ---

    // CabinSite, not Cabin, is what the player actually builds from the Inventory screen — the finished Cabin only
    // ever comes from CompleteCabin, converting an already-stocked site in place.
    public static readonly PileKind[] Buildable =
        { PileKind.WoodStorage, PileKind.RockStorage, PileKind.WaterBarrel, PileKind.FoodCache, PileKind.StorageBin, PileKind.ToolRack, PileKind.TarpShelter, PileKind.Tent, PileKind.LeanTo, PileKind.CabinSite };

    public const string TentId = "tent";
    // The Tarp Shelter's stakes and ridgeline: 2 Sticks and 1 Cordage, plus the Tarp itself. Taking it down returns the
    // Tarp and half the Sticks (rounded down) — the Tarp is placed, not used up.
    public const int TarpShelterSticks = 2;

    public static string PileName(PileKind kind)
    {
        switch (kind)
        {
            case PileKind.RockStorage: return "Rock Pile";
            case PileKind.WaterBarrel: return "Water Barrel";
            case PileKind.FoodCache: return "Food Cache";
            case PileKind.StorageBin: return "Storage Bin";
            case PileKind.ToolRack: return "Tool Rack";
            case PileKind.Tent: return "Tent";
            case PileKind.TarpShelter: return "Tarp Shelter";
            case PileKind.LeanTo: return "Lean-To";
            case PileKind.Cabin: return "Small Cabin";
            case PileKind.CabinSite: return "Small Cabin Site";
            default: return "Wood Pile";
        }
    }

    // What building one takes. A CabinSite is free to place — what Small Cabin needs is deposited into it afterwards,
    // over as many trips as it takes (WoodPileState.Accepts caps each material at CabinRequired).
    public WoodStack[] CostOf(PileKind kind)
    {
        switch (kind)
        {
            case PileKind.WoodStorage: return new[] { new WoodStack { itemId = SticksId, count = woodPileSticks } };
            case PileKind.WaterBarrel: return new[] { new WoodStack { itemId = LogsId, count = 3 } };
            case PileKind.FoodCache: return new[] { new WoodStack { itemId = LogsId, count = 2 }, new WoodStack { itemId = BranchesId, count = 4 } };
            case PileKind.StorageBin: return new[] { new WoodStack { itemId = BranchesId, count = 6 }, new WoodStack { itemId = SticksId, count = 6 } };
            case PileKind.ToolRack: return new[] { new WoodStack { itemId = BranchesId, count = 4 }, new WoodStack { itemId = SticksId, count = 4 } };
            case PileKind.Tent: return new[] { new WoodStack { itemId = TentId, count = 1 } };
            case PileKind.TarpShelter: return new[] { new WoodStack { itemId = SleepManager.TarpId, count = 1 }, new WoodStack { itemId = SticksId, count = TarpShelterSticks },
                                                      new WoodStack { itemId = "cordage", count = 1 } };
            case PileKind.LeanTo: return new[] { new WoodStack { itemId = BranchesId, count = 8 }, new WoodStack { itemId = SticksId, count = 4 },
                                                 new WoodStack { itemId = "cordage", count = 1 } };
            // Not what a CabinSite costs to place (free) — what Small Cabin needs deposited into one before it can be
            // completed. Kept under PileKind.Cabin as the single source of truth CabinRequired reads from.
            case PileKind.Cabin: return new[] { new WoodStack { itemId = LogsId, count = cabinLogs }, new WoodStack { itemId = BranchesId, count = cabinBranches },
                                                new WoodStack { itemId = TallGrassId, count = cabinTallGrass }, new WoodStack { itemId = StoneId, count = cabinStone },
                                                new WoodStack { itemId = ClayId, count = cabinClay } };
            default: return new WoodStack[0];
        }
    }

    // How much of a material a Small Cabin needs in total, for CabinSite's per-material cap and CompleteCabin.
    public int CabinRequired(string itemId)
    {
        foreach (WoodStack c in CostOf(PileKind.Cabin))
            if (c.itemId == itemId)
                return c.count;
        return 0;
    }

    // e.g. "3 Logs" or "free".
    public string CostText(PileKind kind)
    {
        WoodStack[] cost = CostOf(kind);
        if (cost.Length == 0)
            return "free";
        var parts = new List<string>();
        foreach (WoodStack c in cost)
            parts.Add($"{c.count} {ItemDatabase.Get(c.itemId)?.DisplayName ?? c.itemId}");
        return string.Join(", ", parts);
    }

    // Where a storage pile would go if built now in front of the player, or why it can't be built there.
    public bool CanBuildPile(PileKind kind, PlayerController player, out Vector3 position, out string reason)
    {
        position = Vector3.zero;
        InventoryManager inventory = InventoryManager.Instance;
        if (player == null || inventory == null || !WorldLoaded)
        {
            reason = "Not available here.";
            return false;
        }

        // Placing a CabinSite is free and needs no tool — the Hammer only gates CompleteCabin, once it's fully stocked.
        bool cabin = kind == PileKind.CabinSite;

        // One site in progress at a time (Mike placed two by accident, 2026-10-04). A finished cabin isn't a site, so
        // another can be started once the first is built — only an unfinished one blocks it.
        if (cabin)
        {
            foreach (WoodPileState other in piles)
            {
                if (other.kind == PileKind.CabinSite)
                {
                    float away = Vector3.Distance(other.position, player.transform.position);
                    reason = $"You already have a Small Cabin site, {away:0} m away — finish it (go within {cabinCompleteReach:0} m), or take it down first.";
                    return false;
                }
            }
        }

        var shortfalls = new List<(string, int)>();
        foreach (WoodStack c in CostOf(kind))
            shortfalls.Add((c.itemId, c.count - inventory.Player.Count(c.itemId)));
        string needs = Crafting.NeedsText(shortfalls);
        if (needs != null)
        {
            reason = needs;
            return false;
        }

        float distance = cabin ? cabinBuildDistance : buildDistance;
        float spacing = cabin ? cabinFootprint : minSpacing;
        Vector3 forward = Vector3.ProjectOnPlane(player.transform.forward, Vector3.up).normalized;
        Vector3 probe = player.transform.position + forward * distance + Vector3.up * 3f;
        if (!Physics.Raycast(probe, Vector3.down, out RaycastHit hit, 8f, ~0, QueryTriggerInteraction.Ignore) ||
            hit.collider.transform.IsChildOf(player.transform) || !(hit.collider is TerrainCollider))
        {
            reason = "No clear ground in front of you.";
            return false;
        }
        if (Animal.IsOverWater(hit.point, out _))
        {
            reason = "Can't build that in water.";
            return false;
        }
        if (Vector3.Angle(hit.normal, Vector3.up) > maxSlope)
        {
            reason = "The ground in front of you is too steep.";
            return false;
        }
        if (cabin && FindTree(hit.point, cabinFootprint) >= 0)
        {
            reason = "Clear the trees off the building site first.";
            return false;
        }
        if (cabin)
        {
            foreach (Collider nearby in Physics.OverlapSphere(hit.point, cabinFootprint, ~0, QueryTriggerInteraction.Ignore))
            {
                if (nearby.GetComponent<Stump>() == null)
                    continue;
                reason = "Dig out the stumps on the building site first (Primitive Shovel).";
                return false;
            }
        }
        foreach (WoodPileState other in piles)
        {
            bool otherIsCabin = other.kind == PileKind.Cabin || other.kind == PileKind.CabinSite;
            float clearance = Mathf.Max(spacing, otherIsCabin ? cabinFootprint : minSpacing);
            if (Vector3.Distance(other.position, hit.point) < clearance)
            {
                reason = cabin ? "Too close to another pile or building." : "Too close to another pile.";
                return false;
            }
        }
        if (FireManager.Instance != null && FireManager.Instance.NearestLit(hit.point, spacing) != null)
        {
            reason = "Too close to the fire.";
            return false;
        }

        position = hit.point;
        reason = "";
        return true;
    }

    public bool BuildPile(PileKind kind, PlayerController player)
    {
        if (kind == PileKind.Felled || kind == PileKind.Cabin || !CanBuildPile(kind, player, out Vector3 position, out _))
            return false;

        foreach (WoodStack c in CostOf(kind))
            InventoryManager.Instance.RemoveFromPlayer(c.itemId, c.count);
        var pile = new WoodPileState
        {
            id = nextPileId++,
            kind = kind,
            position = position,
            yaw = player.transform.eulerAngles.y,
        };
        piles.Add(pile);
        SpawnPile(pile);
        PileChanged?.Invoke(pile);
        return true;
    }

    // --- Completing a Small Cabin ---

    // The nearest unfinished Small Cabin site within cabinCompleteReach of the player, stocked or not, or null.
    public WoodPileState SiteInReach(PlayerController player)
    {
        if (player == null)
            return null;
        WoodPileState nearest = null;
        float bestDistance = cabinCompleteReach;
        foreach (WoodPileState p in piles)
        {
            if (p.kind != PileKind.CabinSite)
                continue;
            float d = Vector3.Distance(p.position, player.transform.position);
            if (d <= bestDistance)
            {
                nearest = p;
                bestDistance = d;
            }
        }
        return nearest;
    }

    // What a site still lacks, quantity first: "17 Logs, 10 Branches". Null when every material is at its requirement.
    public string SiteNeeds(WoodPileState site)
    {
        var parts = new List<string>();
        foreach (string itemId in CabinMaterialIds)
        {
            int missing = CabinRequired(itemId) - site.Count(itemId);
            if (missing > 0)
                parts.Add($"{missing} {ItemDatabase.Get(itemId)?.DisplayName ?? itemId}");
        }
        return parts.Count == 0 ? null : string.Join(", ", parts);
    }

    // Have / need for every material: "Logs 3 / 20, Branches 0 / 10, ...".
    public string SiteProgress(WoodPileState site)
    {
        var parts = new List<string>();
        foreach (string itemId in CabinMaterialIds)
            parts.Add($"{ItemDatabase.Get(itemId)?.DisplayName ?? itemId} {site.Count(itemId)} / {CabinRequired(itemId)}");
        return string.Join(", ", parts);
    }

    // The nearest CabinSite within reach that's fully stocked, the Hammer equipped and ready to complete — or why not.
    public bool CanCompleteCabin(PlayerController player, out WoodPileState site, out string reason)
    {
        site = null;
        if (player == null || !WorldLoaded)
        {
            reason = "Not available here.";
            return false;
        }

        WoodPileState nearest = SiteInReach(player);
        if (nearest == null)
        {
            reason = "No Small Cabin site nearby — build one first.";
            return false;
        }

        var missing = new List<(string, int)>();
        foreach (string itemId in CabinMaterialIds)
            missing.Add((itemId, CabinRequired(itemId) - nearest.Count(itemId)));
        string needsAtSite = Crafting.NeedsText(missing);
        if (needsAtSite != null)
        {
            reason = needsAtSite.TrimEnd('.') + " at the site.";
            return false;
        }

        InventoryManager inventory = InventoryManager.Instance;
        if (inventory == null || inventory.EquippedTool == null || inventory.EquippedTool.Id != HammerId)
        {
            reason = "Fully stocked — equip the Hammer to build it.";
            return false;
        }

        site = nearest;
        reason = "";
        return true;
    }

    // Converts a fully-stocked CabinSite into the real thing, in place: consumes the deposited materials, swaps its
    // WoodPile view for a Shelter (Small Cabin) view, and furnishes it with a hearth.
    public bool CompleteCabin(PlayerController player)
    {
        if (!CanCompleteCabin(player, out WoodPileState site, out _))
            return false;

        foreach (string itemId in CabinMaterialIds)
            site.Remove(itemId, CabinRequired(itemId));
        site.contents.Clear(); // nothing else should be left — Accepts() caps every material at what's required
        site.kind = PileKind.Cabin;

        if (pileViews.TryGetValue(site.id, out GameObject oldView) && oldView != null)
            Destroy(oldView);
        pileViews.Remove(site.id);
        SpawnPile(site);
        PileChanged?.Invoke(site);

        // Furnished with a hearth — a real campfire, unlit and unfuelled until the player tends it, so
        // Warmth-by-proximity and Cooking work there exactly as at any other campfire.
        if (FireManager.Instance != null)
            FireManager.Instance.BuildFurnished(HearthSpot(site), site.yaw);
        return true;
    }

    // Where a Small Cabin's hearth sits: inside, against the back wall (Shelter.HearthLocal; the slab under it is built by
    // the cabin's view).
    static Vector3 HearthSpot(WoodPileState cabin) =>
        cabin.position + Quaternion.Euler(0f, cabin.yaw, 0f) * Shelter.HearthLocal;

    // Where cabins used to put it: just outside the walls. Only read to find and move a hearth saved that way.
    static Vector3 OldHearthSpot(WoodPileState cabin) =>
        cabin.position + Quaternion.Euler(0f, cabin.yaw, 0f) * new Vector3(Shelter.CabinWidth / 2f + 1.1f, 0f, 0f);

    // Cabins whose hearth is known to be at the indoor spot this session (so they aren't searched again).
    readonly HashSet<int> hearthSettled = new HashSet<int>();
    float nextHearthCheck;

    // A cabin saved with its hearth outside gets it moved indoors, fuel and lit state intact (the fire is moved, not
    // rebuilt). Run about once a second from Update, because the fires and the piles load separately and either can come
    // first; a cabin is settled once a fire is found at the new spot, or the old one has been moved there.
    void MigrateHearths()
    {
        FireManager fires = FireManager.Instance;
        if (fires == null)
            return;
        foreach (WoodPileState cabin in piles)
        {
            if (cabin.kind != PileKind.Cabin || hearthSettled.Contains(cabin.id))
                continue;
            if (fires.FireNear(HearthSpot(cabin), 0.75f) != null)
            {
                hearthSettled.Add(cabin.id);
                continue;
            }
            CampfireState old = fires.FireNear(OldHearthSpot(cabin), 0.75f);
            if (old != null)
            {
                fires.MoveFire(old, HearthSpot(cabin), cabin.yaw);
                hearthSettled.Add(cabin.id);
            }
        }
    }

    // --- Dismantling a finished Small Cabin (Building_Housing_System.md, Tools: Hammer) ---

    // The Hammer is what dismantles anything finished, the way it's what completes a cabin.
    public static bool HammerEquipped =>
        InventoryManager.Instance != null && InventoryManager.Instance.EquippedTool != null &&
        InventoryManager.Instance.EquippedTool.Id == HammerId;

    // The cabin's hearth, found by where CompleteCabin put it — or, for a cabin saved before the hearth moved indoors and
    // not yet migrated, where it used to be.
    CampfireState HearthOf(WoodPileState cabin) =>
        FireManager.Instance == null ? null
        : FireManager.Instance.FireNear(HearthSpot(cabin), 0.75f) ?? FireManager.Instance.FireNear(OldHearthSpot(cabin), 0.75f);

    // Whether the player can dismantle this cabin now, and why not if not: the Hammer in hand, and the hearth out — a
    // lit fire shouldn't just vanish with the house.
    public bool CanDismantleCabin(WoodPileState cabin, out string reason)
    {
        reason = "";
        if (cabin == null || cabin.kind != PileKind.Cabin)
        {
            reason = "not a finished cabin";
            return false;
        }
        if (!HammerEquipped)
        {
            reason = "equip the Hammer";
            return false;
        }
        CampfireState hearth = HearthOf(cabin);
        if (hearth != null && hearth.lit)
        {
            reason = "put the hearth fire out first";
            return false;
        }
        return true;
    }

    // Takes a finished Small Cabin down for good: half its building materials back (rounded down, the Lean-To's
    // precedent), its hearth removed with it and any Firewood still in the hearth returned too. What the pack can't
    // carry is left on the ground as pickup piles. The caller has already confirmed it with the player.
    public string DismantleCabin(WoodPileState cabin, Transform view)
    {
        if (!CanDismantleCabin(cabin, out string reason))
            return $"Can't dismantle the Small Cabin — {reason}.";

        var refund = new StructureRefund(cabin.position, view);
        foreach (WoodStack cost in CostOf(PileKind.Cabin))
            refund.Return(cost.itemId, cost.count / 2);

        CampfireState hearth = HearthOf(cabin);
        if (hearth != null)
        {
            FireManager fires = FireManager.Instance;
            refund.Return(FireManager.FirewoodId, Mathf.FloorToInt(hearth.fuelHours / fires.HoursPerFirewood));
            fires.RemoveFire(hearth);
        }

        RemovePile(cabin);
        return refund.Describe("Small Cabin");
    }

    void SpawnPile(WoodPileState pile)
    {
        var go = new GameObject($"{PileName(pile.kind)} {pile.id}");
        go.transform.SetPositionAndRotation(pile.position, Quaternion.Euler(0f, pile.yaw, 0f));
        if (pile.IsShelter)
            go.AddComponent<Shelter>().Bind(pile, BarkMaterial());
        else
            go.AddComponent<WoodPile>().Bind(pile, BarkMaterial());
        pileViews[pile.id] = go;
    }

    // The Primitive Shovel digging a stump out: it's gone for good, and its roots make a Firewood.
    public void RemoveStump(int index)
    {
        FelledTree tree = felled.Find(t => t.index == index);
        if (tree == null || !tree.stump)
            return;
        tree.stump = false;
        for (int i = stumps.Count - 1; i >= 0; i--)
        {
            if (stumps[i] != null && stumps[i].TryGetComponent(out Stump marker) && marker.TreeIndex == index)
            {
                Destroy(stumps[i]);
                stumps.RemoveAt(i);
            }
        }

        InventoryManager inventory = InventoryManager.Instance;
        if (inventory == null || inventory.AddToPlayer(FireManager.FirewoodId, 1) < 1)
            DropPile(WorldPosition(index), FireManager.FirewoodId, 1);
    }

    // Leaves a small felled-type pile on the ground (what didn't fit in the player's arms).
    public void DropPile(Vector3 at, string itemId, int count)
    {
        var pile = new WoodPileState { id = nextPileId++, kind = PileKind.Felled, position = at };
        pile.Add(itemId, count);
        piles.Add(pile);
        SpawnPile(pile);
    }

    void SpawnStump(int index)
    {
        float radius = TrunkRadius(index);
        var stump = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        stump.name = "Stump";
        stump.AddComponent<Stump>().TreeIndex = index;
        // A true cylinder to look at and dig: the primitive's capsule is nearly a ball at this height and misses its rim.
        Mesh cylinder = stump.GetComponent<MeshFilter>().sharedMesh;
        Destroy(stump.GetComponent<Collider>());
        var stumpCollider = stump.AddComponent<MeshCollider>();
        stumpCollider.sharedMesh = cylinder;
        stumpCollider.convex = true;
        const float height = 0.45f;
        stump.transform.position = WorldPosition(index) + Vector3.up * (height / 2f - 0.05f);
        stump.transform.localScale = new Vector3(radius * 2.1f, height / 2f, radius * 2.1f);
        stump.transform.rotation = Quaternion.Euler(0f, originalTrees[index].rotation * Mathf.Rad2Deg, 0f);
        Material bark = BarkMaterial();
        if (bark != null)
            stump.GetComponent<MeshRenderer>().sharedMaterial = bark;
        stumps.Add(stump);
    }

    Material bark;

    // The trees' own bark, so stumps and logs match them.
    Material BarkMaterial()
    {
        if (bark != null)
            return bark;
        if (data == null || data.treePrototypes.Length == 0 || data.treePrototypes[0].prefab == null)
            return null;
        MeshRenderer renderer = data.treePrototypes[0].prefab.GetComponentInChildren<MeshRenderer>();
        bark = renderer != null && renderer.sharedMaterials.Length > 0 ? renderer.sharedMaterials[0] : null;
        return bark;
    }

    public void ResetWood()
    {
        felled.Clear();
        felledSet.Clear();
        piles.Clear();
        hearthSettled.Clear();
        nextPileId = 1;
        snowLoadHours = stormAfterglow = 0f;
        bark = null;
        if (WorldLoaded)
            Apply();
    }

    public WoodSaveData CaptureState() => new WoodSaveData
    {
        felled = new List<FelledTree>(felled),
        piles = new List<WoodPileState>(piles),
        nextPileId = nextPileId,
        snowLoadHours = snowLoadHours,
        stormAfterglow = stormAfterglow,
    };

    // Runs after the World has loaded (GameManager.ContinueGame), so the terrain is updated straight away.
    public void RestoreState(WoodSaveData saved)
    {
        felled.Clear();
        felledSet.Clear();
        if (saved.felled != null)
        {
            foreach (FelledTree tree in saved.felled)
            {
                if (felledSet.Add(tree.index))
                    felled.Add(tree);
            }
        }
        piles.Clear();
        hearthSettled.Clear();
        if (saved.piles != null)
            piles.AddRange(saved.piles);
        nextPileId = Mathf.Max(1, saved.nextPileId);
        snowLoadHours = saved.snowLoadHours;
        stormAfterglow = saved.stormAfterglow;
        Apply();
    }

    string ISaveable.SaveFile => "world";
    string ISaveable.SaveKey => "wood";
    object ISaveable.CaptureState() => CaptureState();
    void ISaveable.RestoreState(string json) => RestoreState(JsonUtility.FromJson<WoodSaveData>(json));
}
