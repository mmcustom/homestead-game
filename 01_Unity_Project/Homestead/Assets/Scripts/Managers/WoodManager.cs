using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class WoodStack
{
    public string itemId;
    public int count;
    // The in-game day a batch was acquired (perishables: so the Spoilage System can still age it) or, for wood, last made wet
    // (WoodMoisture); kept while it is stored. 0 is "no stamp": food from before this existed, wood that is dry.
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
// RainBarrel is a Water Barrel stood at a Small Cabin's long-wall eave that fills itself from that roof slope in rain
// (Water_System.md's Rainwater Collection; WoodManager.CollectRain).
// WellSite, Well and Outhouse are Structures Slice 1 (WellsAndOuthouses, PlacedStructure): a WellSite is a staged build (dig, line,
// cap) that turns into a Well in place, like CabinSite into Cabin; an OuthouseSite is a staged site too (materials over trips, no dig)
// that turns into an Outhouse, a shack with a pit that fills. Any build over ~15 kg of materials is staged (WellsAndOuthouses.IsStagedSite):
// that now includes the WaterBarrelSite, RainBarrelSite and FoodCacheSite, which turn into the (unchanged) WaterBarrel, RainBarrel and FoodCache.
// Phase B adds the Wood Shed: a WoodShedSite (staged, 24 kg) that turns into a WoodShed, a roofed rack drawn and used like a Wood Pile
// (WoodPile, the transfer screen) whose wood is never rained on and so seasons from wet to dry (WoodMoisture).
// Phase C adds the Covered Outdoor Kitchen: a staged site in two stages (KitchenSite: the hearth, Stone and Clay; then KitchenRoofSite: the roof,
// Logs, Branches and Tall Grass) that turns into a Kitchen, a roof on posts over a stone hearth that is an ordinary Campfire (CampfireState).
// Kitchen add-ons, batch 1: Countertop, Shelving, Grill (a Wood-Fired Grill: a second furnished fire) and SlopSink stand in the four fixed slots of a finished Kitchen
// (Setbacks.KitchenLayout; WoodManager.AddonSlots). Single-step builds with the Hammer; the slot is just their position, so there is no parent link saved.
// (Saved as numbers, so new kinds go on the end.)
public enum PileKind { Felled, WoodStorage, RockStorage, WaterBarrel, FoodCache, StorageBin, Tent, LeanTo, Cabin, CabinSite, ToolRack, TarpShelter, RainBarrel, WellSite, Well, Outhouse, OuthouseSite, WaterBarrelSite, RainBarrelSite, FoodCacheSite, WoodShedSite, WoodShed, KitchenSite, KitchenRoofSite, Kitchen, Countertop, Shelving, Grill, SlopSink }

[Serializable]
public class WoodPileState
{
    public int id;
    public PileKind kind;
    public Vector3 position;
    public float yaw;
    public List<WoodStack> contents = new List<WoodStack>();
    // Structures Slice 1 (appended, so older saves load these as 0): a Well's litres in its buffer and the hours its water stays
    // Unsafe after a flood; a Well site's dig progress (0-1); the in-game day an Outhouse's pit was dug.
    public int litres;
    public float contaminatedHours;
    public float digProgress;
    public int builtDay;
    // A staged site pounded with the Hammer (0-1; WellsAndOuthouses.BuildBlow). Above 0 its delivered materials are locked in. Appended, so older saves load 0.
    public float buildProgress;
    // Well Water Table B3 (appended, so older saves load 0): metres to the water where a Well Site was placed (WellWater). It sets the well's dig,
    // Stone, buffer and recovery for good. 0 is a well from before depth existed: it keeps today's numbers (WellWater's Legacy result).
    public float depthToWater;

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

    // Adds to the batch from the same day (perishables and wood stay apart by day; everything else is day 0).
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

    // Re-stamps every wood batch to a day (rain wets it: today; marked dry: 0), merging the batches of one item that now share a day.
    // Returns whether any batch changed.
    public bool RestampWood(int day)
    {
        bool changed = false;
        foreach (WoodStack stack in contents)
        {
            if (WoodManager.IsWood(stack.itemId) && stack.day != day)
            {
                stack.day = day;
                changed = true;
            }
        }
        if (changed)
            MergeBatches();
        return changed;
    }

    // Seasons the wood a number of days further along (WoodMoisture.Aged): the debug "advance seasoning" and nothing else.
    public void AgeWood(int days)
    {
        foreach (WoodStack stack in contents)
            if (WoodManager.IsWood(stack.itemId))
                stack.day = WoodMoisture.Aged(stack.day, days);
        MergeBatches();
    }

    void MergeBatches()
    {
        for (int i = contents.Count - 1; i > 0; i--)
        {
            for (int j = 0; j < i; j++)
            {
                if (contents[j].itemId == contents[i].itemId && contents[j].day == contents[i].day)
                {
                    contents[j].count += contents[i].count;
                    contents.RemoveAt(i);
                    break;
                }
            }
        }
    }

    // How many of an item stored here are wet and how many dry, on a given day.
    public void CountMoisture(string itemId, int today, int seasoningDays, out int wet, out int dry)
    {
        wet = dry = 0;
        foreach (WoodStack stack in contents)
        {
            if (stack.itemId != itemId)
                continue;
            if (WoodMoisture.IsDry(stack.day, today, seasoningDays))
                dry += stack.count;
            else
                wet += stack.count;
        }
    }

    public bool IsEmpty => contents.Count == 0;
    public bool IsRock => kind == PileKind.RockStorage;
    public bool IsShelter => kind == PileKind.Tent || kind == PileKind.LeanTo || kind == PileKind.Cabin || kind == PileKind.TarpShelter;
    public bool IsWoodPile => kind == PileKind.Felled || kind == PileKind.WoodStorage;
    // A pile of wood with no roof, which rain wets (WoodManager.RainOnOpenPiles); the Wood Shed keeps its wood dry.
    public bool IsOpenWoodPile => IsWoodPile;
    public bool IsWoodShed => kind == PileKind.WoodShed;
    // Drawn and handled by PlacedStructure, not WoodPile or Shelter.
    public bool IsPlacedStructure => kind == PileKind.WellSite || kind == PileKind.Well || kind == PileKind.Outhouse || kind == PileKind.Kitchen || kind == PileKind.Countertop || kind == PileKind.Grill || kind == PileKind.SlopSink || WellsAndOuthouses.IsStagedSite(kind);
    public bool IsKitchen => kind == PileKind.KitchenSite || kind == PileKind.KitchenRoofSite || kind == PileKind.Kitchen;
    // A Countertop, Shelving, Grill or Slop Sink: stands in one of a finished Kitchen's four fixed slots (Setbacks.KitchenLayout).
    public bool IsKitchenAddon => WoodManager.IsKitchenAddon(kind);
    public bool IsSlopSink => kind == PileKind.SlopSink;
    public bool IsBuilt => kind != PileKind.Felled;
    // Both barrels hold water by the litre under one shared cap (WoodManager.CapacityOf).
    public bool IsWaterBarrel => kind == PileKind.WaterBarrel || kind == PileKind.RainBarrel;

    // What this pile will take in: each kind of storage takes only its own category (Primitive_Storage_System.md).
    // A CabinSite only takes what Small Cabin still needs — once a material reaches its required amount, it stops
    // accepting more of that one (same idea as the Water Barrel's litre cap, just tracked per material instead of
    // one shared total).
    public bool Accepts(string itemId)
    {
        switch (kind)
        {
            case PileKind.RockStorage: return itemId == WoodManager.StoneId;
            case PileKind.WaterBarrel:
            case PileKind.RainBarrel: return WoodManager.IsWater(itemId);
            case PileKind.FoodCache: return WoodManager.IsStorableFood(itemId);
            case PileKind.StorageBin: return WoodManager.IsDryGoods(itemId);
            case PileKind.Shelving: return WoodManager.IsDryGoods(itemId) || WoodManager.IsStorableFood(itemId);
            case PileKind.ToolRack: return WoodManager.IsTool(itemId);
            case PileKind.CabinSite:
                return WoodManager.Instance != null && Array.IndexOf(WoodManager.CabinMaterialIds, itemId) >= 0 &&
                       Count(itemId) < WoodManager.Instance.CabinRequired(itemId);
            case PileKind.WellSite:
            case PileKind.OuthouseSite:
            case PileKind.WaterBarrelSite:
            case PileKind.RainBarrelSite:
            case PileKind.FoodCacheSite:
            case PileKind.KitchenSite:
            case PileKind.KitchenRoofSite:
            case PileKind.WoodShedSite: return WellsAndOuthouses.SiteAccepts(this, itemId);
            case PileKind.Countertop:
            case PileKind.Grill:
            case PileKind.SlopSink:
            case PileKind.Kitchen:
            case PileKind.Well:
            case PileKind.Outhouse:
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

    // The four Kitchen add-ons: they stand in a finished Kitchen's fixed slots and nowhere else.
    public static bool IsKitchenAddon(PileKind kind) =>
        kind == PileKind.Countertop || kind == PileKind.Shelving || kind == PileKind.Grill || kind == PileKind.SlopSink;

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

    [Header("Rain Barrel (Water_System.md's Rainwater Collection)")]
    [Tooltip("Litres a Rain Barrel holds, twice the hand-filled Water Barrel.")]
    [SerializeField, Min(1)] int rainBarrelLitres = 80;
    [Tooltip("Logs to build one (8 kg each). No Cordage.")]
    [SerializeField, Min(1)] int rainBarrelLogCount = 3;
    [Tooltip("Branches to build one, so the load stays under the Encumbered limit.")]
    [SerializeField, Min(0)] int rainBarrelBranches = 2;
    [Tooltip("Share of the rain that falls on a roof slope that reaches the barrel (splash and evaporation take the rest). A placeholder, not a sourced figure.")]
    [SerializeField, Range(0f, 1f)] float rainEfficiency = 0.8f;
    [Tooltip("A Rain Barrel's centre must stand this far to this far (metres) outside the roof's edge, along the cabin's long walls. The minimum keeps it clear of the wall.")]
    [SerializeField, Min(0f)] float eaveMinOutside = 0.45f;
    [SerializeField, Min(0f)] float eaveMaxOutside = 1.2f;
    [Tooltip("Rain Barrels can stand this close to each other (metres), so three fit along one slope.")]
    [SerializeField, Min(0.5f)] float rainBarrelSpacing = 1.2f;
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

    // Litres a water barrel of this kind holds (0 for anything that isn't one).
    public int CapacityOf(PileKind kind) =>
        kind == PileKind.RainBarrel ? rainBarrelLitres : kind == PileKind.WaterBarrel ? barrelLitres
        : kind == PileKind.SlopSink && WellsAndOuthouses.Instance != null ? WellsAndOuthouses.Instance.slopSinkLitres : 0;

    // Rain that fell on a roof slope but fits no barrel's whole litre yet, per barrel (not saved: under a litre is lost on load).
    readonly Dictionary<int, float> rainCarry = new Dictionary<int, float>();
    bool rainSubscribed;

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
        SubscribeToRain();
    }

    // Rain Barrels fill once per game hour, from the weather's own HourEnded (so sleep, collapse and day skips count too).
    void SubscribeToRain()
    {
        if (rainSubscribed || Instance != this || WeatherManager.Instance == null)
            return;
        WeatherManager.Instance.HourEnded += CollectRain;
        WeatherManager.Instance.HourEnded += RainOnOpenPiles;
        rainSubscribed = true;
    }

    void OnDestroy()
    {
        if (Instance != this)
            return;

        if (rainSubscribed && WeatherManager.Instance != null)
            WeatherManager.Instance.HourEnded -= CollectRain;
        if (rainSubscribed && WeatherManager.Instance != null)
            WeatherManager.Instance.HourEnded -= RainOnOpenPiles;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (SaveManager.Instance != null)
            SaveManager.Instance.Unregister(this);
        Instance = null;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SubscribeToRain(); // in case the weather didn't exist yet at Start
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

    // Whether the terrain's trees are loaded (TapManager checks its taps against them once the world is up).
    public bool TreesReady => WorldLoaded;
    public int TreeCount => WorldLoaded ? originalTrees.Length : 0;
    public bool InWorld => WorldLoaded;

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

    // Stamps every batch of a list of wood with today as the day it was last wet, so it starts wet and seasons from there.
    public static List<WoodStack> WetStamped(List<WoodStack> wood)
    {
        int stamp = WoodMoisture.WetStamp;
        foreach (WoodStack stack in wood)
            if (IsWood(stack.itemId))
                stack.day = stamp;
        return wood;
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
        if (TapManager.Instance != null)
            TapManager.Instance.OnTreeFelled(index); // a tapped maple gives its Spile and Sap Bucket back (the Axe and a storm both come here)
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
            contents = WetStamped(YieldOf(index)), // freshly felled wood is wet (WoodMoisture)
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
        else if (view != null && view.TryGetComponent(out PlacedStructure structure))
            structure.Rebuild();
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
        { PileKind.WoodStorage, PileKind.RockStorage, PileKind.WaterBarrelSite, PileKind.RainBarrelSite, PileKind.FoodCacheSite, PileKind.WoodShedSite, PileKind.StorageBin, PileKind.ToolRack, PileKind.TarpShelter, PileKind.Tent, PileKind.LeanTo, PileKind.CabinSite,
          PileKind.WellSite, PileKind.OuthouseSite, PileKind.KitchenSite,
          PileKind.Countertop, PileKind.Shelving, PileKind.Grill, PileKind.SlopSink };

    public const string TentId = "tent";
    // The Tarp Shelter's stakes and ridgeline: 2 Sticks and 1 Cordage, plus the Tarp itself. Taking it down returns the
    // Tarp and half the Sticks (rounded down) — the Tarp is placed, not used up.
    public const int TarpShelterSticks = 2;

    // Add-on naming convention: <Building>-<Item>, each part capitalised (Kitchen-Countertop). A future Cabin-... or Shed-... add-on
    // gets its own prefix constant and goes through AddonName. Names are derived from the kind here, never saved.
    public const string KitchenAddonPrefix = "Kitchen";
    public static string AddonName(string building, string item) => building + "-" + item;

    public static string PileName(PileKind kind)
    {
        switch (kind)
        {
            case PileKind.RockStorage: return "Rock Pile";
            case PileKind.WaterBarrel: return "Water Barrel";
            case PileKind.RainBarrel: return "Rain Barrel";
            case PileKind.FoodCache: return "Food Cache";
            case PileKind.StorageBin: return "Storage Bin";
            case PileKind.ToolRack: return "Tool Rack";
            case PileKind.Tent: return "Tent";
            case PileKind.TarpShelter: return "Tarp Shelter";
            case PileKind.LeanTo: return "Lean-To";
            case PileKind.Cabin: return "Small Cabin";
            case PileKind.CabinSite: return "Small Cabin Site";
            case PileKind.WellSite: return "Well Site";
            case PileKind.Well: return "Well";
            case PileKind.Outhouse: return "Outhouse";
            case PileKind.OuthouseSite: return "Outhouse Site";
            case PileKind.WaterBarrelSite: return "Water Barrel Site";
            case PileKind.RainBarrelSite: return "Rain Barrel Site";
            case PileKind.FoodCacheSite: return "Food Cache Site";
            case PileKind.WoodShedSite: return "Wood Shed Site";
            case PileKind.WoodShed: return "Wood Shed";
            case PileKind.KitchenSite: return "Covered Kitchen Site";
            case PileKind.KitchenRoofSite: return "Covered Kitchen Site (roof)";
            case PileKind.Kitchen: return "Covered Outdoor Kitchen";
            case PileKind.Countertop: return AddonName(KitchenAddonPrefix, "Countertop");
            case PileKind.Shelving: return AddonName(KitchenAddonPrefix, "Shelves");
            case PileKind.Grill: return AddonName(KitchenAddonPrefix, "Grill");
            case PileKind.SlopSink: return AddonName(KitchenAddonPrefix, "Sink");
            default: return "Wood Pile";
        }
    }

    // What building one takes. A CabinSite is free to place — what Small Cabin needs is deposited into it afterwards,
    // over as many trips as it takes (WoodPileState.Accepts caps each material at CabinRequired).
    // What this one standing structure took: the same as its kind's, except a Well, whose Stone lining is its own depth's (what Dismantle halves).
    public WoodStack[] CostOf(WoodPileState pile) =>
        pile.kind == PileKind.Well ? WellsAndOuthouses.WellCostFor(pile) : CostOf(pile.kind);

    public WoodStack[] CostOf(PileKind kind)
    {
        switch (kind)
        {
            case PileKind.WoodStorage: return new[] { new WoodStack { itemId = SticksId, count = woodPileSticks } };
            case PileKind.WaterBarrel: return new[] { new WoodStack { itemId = LogsId, count = 3 } };
            case PileKind.RainBarrel: return new[] { new WoodStack { itemId = LogsId, count = rainBarrelLogCount }, new WoodStack { itemId = BranchesId, count = rainBarrelBranches } };
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
            // A Well Site is free to mark out (like a Cabin Site); PileKind.Well holds what the finished Well takes in total, delivered
            // to the site over several trips (WellsAndOuthouses.WellSiteAccepts) and halved by Dismantle.
            // (an Outhouse Site is free to mark out the same way; PileKind.Outhouse holds its total.)
            case PileKind.Well: return WellsAndOuthouses.WellCost();
            case PileKind.Outhouse: return WellsAndOuthouses.OuthouseCost();
            // A Wood Shed Site is free to mark out the same way; PileKind.WoodShed holds its total and what Dismantle halves.
            case PileKind.WoodShed: return WellsAndOuthouses.WoodShedCost();
            // A Kitchen Site is free to mark out too; PileKind.Kitchen holds both stages' materials in total, what Dismantle halves line by line.
            case PileKind.Kitchen: return WellsAndOuthouses.KitchenCost();
            // The Kitchen add-ons are single-step builds (each under the 15 kg staged line); WellsAndOuthouses holds the numbers.
            case PileKind.Countertop: return WellsAndOuthouses.CountertopCost();
            case PileKind.Shelving: return WellsAndOuthouses.ShelvingCost();
            case PileKind.Grill: return WellsAndOuthouses.GrillCost();
            case PileKind.SlopSink: return WellsAndOuthouses.SlopSinkCost();
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
        if (WellsAndOuthouses.IsStagedSite(kind))
        {
            PileKind finished = WellsAndOuthouses.FinalKind(kind);
            return $"free to mark out; the {PileName(finished)} then takes " + CostText(finished) + " over several trips";
        }
        WoodStack[] cost = CostOf(kind);
        if (cost.Length == 0)
            return "free";
        var parts = new List<string>();
        foreach (WoodStack c in cost)
            parts.Add($"{c.count} {ItemDatabase.Get(c.itemId)?.DisplayName ?? c.itemId}");
        return string.Join(", ", parts);
    }

    // Whether building this can start at all — the player is in the World, the materials are in the pack, and for a Cabin Site
    // no other site is waiting — before any spot is looked at. The Craft and Build screen asks this to start Placement Mode.
    public bool CanStartBuild(PileKind kind, PlayerController player, out string reason)
    {
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
                    reason = $"You already have a Small Cabin site, {away:0} m away — finish it (stock it and pound it with the Hammer), or take it down first.";
                    return false;
                }
            }
        }

        // A Kitchen add-on needs a finished Covered Outdoor Kitchen to stand in (its slots), before any materials are asked for.
        if (IsKitchenAddon(kind) && !AnyFinishedKitchen)
        {
            reason = NeedsKitchenReason;
            return false;
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

        reason = "";
        return true;
    }

    // The old fixed spot: a probe the build distance ahead of the player (further for a Cabin Site) and the ground under it.
    // Placement Mode aims instead; this stays for anything that still builds "just in front of you".
    BuildSpot ProbeAhead(PileKind kind, PlayerController player)
    {
        float distance = kind == PileKind.CabinSite ? cabinBuildDistance : buildDistance;
        Vector3 forward = Vector3.ProjectOnPlane(player.transform.forward, Vector3.up).normalized;
        Vector3 probe = player.transform.position + forward * distance + Vector3.up * 3f;
        if (!Physics.Raycast(probe, Vector3.down, out RaycastHit hit, 8f, ~0, QueryTriggerInteraction.Ignore))
            return new BuildSpot { point = probe, normal = Vector3.up, onTerrain = false };
        return new BuildSpot
        {
            point = hit.point,
            normal = hit.normal,
            onTerrain = !hit.collider.transform.IsChildOf(player.transform) && hit.collider is TerrainCollider,
        };
    }

    // Where a storage pile would go if built now in front of the player, or why it can't be built there.
    public bool CanBuildPile(PileKind kind, PlayerController player, out Vector3 position, out string reason)
    {
        position = Vector3.zero;
        if (player == null)
        {
            reason = "Not available here.";
            return false;
        }

        BuildSpot spot = ProbeAhead(kind, player);
        if (!CanBuildPileAt(kind, player, spot, player.transform.eulerAngles.y, false, out reason))
            return false;
        position = spot.point;
        return true;
    }

    // Every rule for building this kind at a given spot and facing. aimed only changes the wording of the ground messages
    // ("in front of you" makes no sense for a spot you chose); the rules themselves are the same for the fixed probe and
    // for Placement Mode, so a ghost can never disagree with the build. Facing is not a rule today (it is only saved).
    public bool CanBuildPileAt(PileKind kind, PlayerController player, BuildSpot spot, float yaw, bool aimed, out string reason)
    {
        if (!CanStartBuild(kind, player, out reason))
            return false;

        // A Kitchen add-on goes in a slot of a finished Kitchen and answers to its own rules (CanBuildAddonAt), not the open-ground ones below.
        if (IsKitchenAddon(kind))
            return CanBuildAddonAt(kind, spot, out reason);

        bool cabin = kind == PileKind.CabinSite;
        float spacing = cabin ? cabinFootprint : minSpacing;
        if (!spot.onTerrain)
        {
            reason = aimed ? "No clear ground there." : "No clear ground in front of you.";
            return false;
        }
        if (Animal.IsOverWater(spot.point, out _))
        {
            reason = "Can't build that in water.";
            return false;
        }
        if (Vector3.Angle(spot.normal, Vector3.up) > maxSlope)
        {
            reason = aimed ? "The ground there is too steep." : "The ground in front of you is too steep.";
            return false;
        }
        if (cabin && FindTree(spot.point, cabinFootprint) >= 0)
        {
            reason = "Clear the trees off the building site first.";
            return false;
        }
        if (cabin)
        {
            foreach (Collider nearby in Physics.OverlapSphere(spot.point, cabinFootprint, ~0, QueryTriggerInteraction.Ignore))
            {
                if (nearby.GetComponent<Stump>() == null)
                    continue;
                reason = "Dig out the stumps on the building site first (Primitive Shovel).";
                return false;
            }
        }
        // A Rain Barrel only goes at the eave of a finished Small Cabin's long wall.
        bool rainBarrel = IsRainBarrel(kind);
        if (rainBarrel && !AtAnEave(spot.point, out _, out _))
        {
            reason = NotAtAnEaveReason;
            return false;
        }
        // A Well and an Outhouse keep their real-world distances from water, each other and buildings, off floodplain and out of
        // hollows (WellsAndOuthouses.CanPlace); the refusal names what it is too close to.
        // A Wood Shed Site keeps off floodplain and 3 m from the cabin (the same call).

        if ((kind == PileKind.WellSite || kind == PileKind.OuthouseSite || kind == PileKind.WoodShedSite || kind == PileKind.KitchenSite) && WellsAndOuthouses.Instance != null &&
            !WellsAndOuthouses.Instance.CanPlace(kind, spot.point, null, out reason))
            return false;
        foreach (WoodPileState other in piles)
        {
            if (rainBarrel && other.kind == PileKind.Cabin)
                continue; // the eave band already keeps it off the wall; the 3.2 m cabin clearance would rule out every eave
            bool otherIsCabin = other.kind == PileKind.Cabin || other.kind == PileKind.CabinSite;
            float clearance = rainBarrel && IsRainBarrel(other.kind)
                ? rainBarrelSpacing
                : Mathf.Max(spacing, otherIsCabin ? cabinFootprint : minSpacing);
            if (Vector3.Distance(other.position, spot.point) < clearance)
            {
                reason = cabin ? "Too close to another pile or building." : "Too close to another pile.";
                return false;
            }
        }
        if (FireManager.Instance != null && FireManager.Instance.NearestLit(spot.point, spacing) != null)
        {
            reason = "Too close to the fire.";
            return false;
        }

        // The Hammer does the building (Mike's rule): the ghost shows without it, but red, and the click builds nothing. Last, so a
        // bad spot still says what is wrong with the spot first. Staged sites and the Tent, Lean-To and Tarp are not on this list.
        if (NeedsHammerToBuild(kind) && !HammerEquipped)
        {
            reason = WellsAndOuthouses.EquipHammerToBuild;
            return false;
        }

        reason = "";
        return true;
    }

    // The single-step structures on the Build screen: the click that builds them takes the Hammer in hand. (A staged site is placed
    // and stocked freely and finished with the Hammer; the Craft tab never needs it.)
    public static bool NeedsHammerToBuild(PileKind kind)
    {
        switch (kind)
        {
            case PileKind.WoodStorage:
            case PileKind.RockStorage:
            case PileKind.StorageBin:
            case PileKind.ToolRack:
            case PileKind.Countertop:
            case PileKind.Shelving:
            case PileKind.Grill:
            case PileKind.SlopSink:
                return true;
            default:
                return false;
        }
    }

    // A Rain Barrel or its staged site: both stand at an eave and keep the barrel spacing from each other.
    public static bool IsRainBarrel(PileKind kind) => kind == PileKind.RainBarrel || kind == PileKind.RainBarrelSite;

    // Drops a pile's view and draws it again as whatever its kind is now (a completed staged site that becomes a WoodPile).
    public void Respawn(WoodPileState pile)
    {
        if (pileViews.TryGetValue(pile.id, out GameObject oldView) && oldView != null)
            Destroy(oldView);
        pileViews.Remove(pile.id);
        SpawnPile(pile);
        PileChanged?.Invoke(pile);
    }

    // Builds it in front of the player (the fixed probe), facing the way the player does.
    public bool BuildPile(PileKind kind, PlayerController player)
    {
        if (player == null)
            return false;
        return BuildPile(kind, player, ProbeAhead(kind, player), player.transform.eulerAngles.y);
    }

    // Builds it at a chosen spot and facing, spending the materials only now.
    public bool BuildPile(PileKind kind, PlayerController player, BuildSpot spot, float yaw)
    {
        if (kind == PileKind.Felled || kind == PileKind.Cabin || kind == PileKind.Well || kind == PileKind.Outhouse ||
            kind == PileKind.WaterBarrel || kind == PileKind.RainBarrel || kind == PileKind.FoodCache || kind == PileKind.WoodShed || kind == PileKind.KitchenRoofSite || kind == PileKind.Kitchen || !CanBuildPileAt(kind, player, spot, yaw, true, out _))
            return false;

        foreach (WoodStack c in CostOf(kind))
            InventoryManager.Instance.RemoveFromPlayer(c.itemId, c.count);
        // An add-on is built exactly in its slot, facing the middle of the kitchen, whatever the ghost said.
        if (IsKitchenAddon(kind) && KitchenForSlot(spot.point, out WoodPileState slotKitchen, out int slotIndex))
            yaw = Setbacks.KitchenLayout.SlotYaw(slotKitchen.yaw, slotIndex);

        var pile = new WoodPileState
        {
            id = nextPileId++,
            kind = kind,
            position = spot.point,
            yaw = yaw,
            builtDay = 0, // an Outhouse's pit starts when its site is completed (WellsAndOuthouses.FinishSite)
        };
        // A Well Site remembers how deep the water is where it was placed (the same lookup the ghost's preview showed).
        if (kind == PileKind.WellSite && WellsAndOuthouses.Instance != null && WellsAndOuthouses.Instance.WellWaterAt(spot.point, out WellWaterResult water) && water.Diggable)
            pile.depthToWater = water.depthToWater;
        piles.Add(pile);
        // A Wood-Fired Grill comes with its fire: a second furnished campfire in the firebox, unlit and out of fuel like the hearth.
        if (kind == PileKind.Grill && FireManager.Instance != null)
            FireManager.Instance.BuildFurnished(GrillFireSpot(pile), pile.yaw);
        SpawnPile(pile);
        PileChanged?.Invoke(pile);
        return true;
    }

    // --- Rain Barrels (Water_System.md's Rainwater Collection) ---

    // Whether a spot is at the eave of a finished Small Cabin's long wall, and which cabin and which slope (-1 left, +1 right
    // of the ridge in the cabin's own space). A barrel's roof is found this way each hour rather than saved, so a dismantled
    // cabin just means its barrels stop filling.
    public bool AtAnEave(Vector3 spot, out WoodPileState cabin, out int side)
    {
        foreach (WoodPileState pile in piles)
        {
            if (pile.kind == PileKind.Cabin &&
                RainCatchment.IsAtEave(pile.position, pile.yaw, spot, eaveMinOutside, eaveMaxOutside, out side))
            {
                cabin = pile;
                return true;
            }
        }
        cabin = null;
        side = 0;
        return false;
    }

    public const string NotAtAnEaveReason = "A Rain Barrel stands just outside a Small Cabin's long wall, under the roof's edge.";

    // The bark material the piles and shelters are drawn with, for Placement Mode's ghosts.
    public Material SharedBark => BarkMaterial();

    // The spots a Rain Barrel snaps to along every finished cabin's two long walls within range of a point: in the middle of
    // the eave band, one every rainBarrelSpacing metres along the roof's depth. Only the ground position (x, z) matters;
    // the caller finds the ground height. Used by Placement Mode, which then asks CanBuildPileAt about each one.
    public List<(Vector3 spot, int side, WoodPileState cabin)> EaveSpots(Vector3 near, float range)
    {
        var spots = new List<(Vector3 spot, int side, WoodPileState cabin)>();
        float outside = Shelter.RoofEaveX + (eaveMinOutside + eaveMaxOutside) / 2f;
        // A hair over the spacing rule's distance, so a barrel stood on one spot never fails the 1.2 m test against its neighbour
        // through float rounding.
        float step = rainBarrelSpacing + 0.02f;
        int steps = Mathf.FloorToInt(Shelter.RoofPanelDepth / 2f / step);
        foreach (WoodPileState pile in piles)
        {
            if (pile.kind != PileKind.Cabin || Vector3.Distance(pile.position, near) > range + Shelter.CabinDepth)
                continue;
            Quaternion turn = Quaternion.Euler(0f, pile.yaw, 0f);
            foreach (int side in new[] { -1, 1 })
            {
                for (int k = -steps; k <= steps; k++)
                {
                    Vector3 spot = pile.position + turn * new Vector3(side * outside, 0f, k * step);
                    if (Vector3.Distance(spot, near) <= range)
                        spots.Add((spot, side, pile));
                }
            }
        }
        return spots;
    }

    // One game hour of weather has ended: if it rained, each roof slope sends its litres into the Rain Barrels beneath it,
    // the first barrel (lowest id) first, overflowing to the next, and anything past the last full barrel is lost. Rain water
    // is Good quality and, in version 1, does not spoil. Snow is not rain (RainMmPerHour is 0 for it). Whole litres only; the
    // part of a litre left over carries to the next hour.
    void CollectRain(WeatherType precipitation)
    {
        WeatherManager weather = WeatherManager.Instance;
        float mm = weather != null ? weather.RainMmPerHour(precipitation) : 0f;
        if (mm <= 0f)
            return;

        float slopeLitres = RainCatchment.LitresPerHour(mm, rainEfficiency);
        string goodWater = WaterQualities.ItemId(WaterQuality.Good);
        var remaining = new Dictionary<(int cabin, int side), float>();
        foreach (WoodPileState barrel in piles)
        {
            if (barrel.kind != PileKind.RainBarrel || !AtAnEave(barrel.position, out WoodPileState cabin, out int side))
                continue;

            (int, int) slope = (cabin.id, side);
            if (!remaining.TryGetValue(slope, out float pool))
                pool = slopeLitres;
            rainCarry.TryGetValue(barrel.id, out float carry);

            float available = pool + carry;
            int whole = Mathf.FloorToInt(available);
            int taken = Mathf.Min(whole, Mathf.Max(0, rainBarrelLitres - barrel.Total));
            if (taken > 0)
            {
                barrel.Add(goodWater, taken);
                NotifyChanged(barrel); // redraws the water level
            }

            if (taken == whole)
            {
                rainCarry[barrel.id] = available - whole;
                remaining[slope] = 0f;
            }
            else
            {
                rainCarry[barrel.id] = 0f;
                remaining[slope] = available - taken; // this barrel is full: the rest runs on to the next
            }
        }
    }

    // --- Wet and dry wood (WoodMoisture) ---

    // One game hour of rain or snow has ended: the wood in every open Wood Pile is wet again (restamped to today), so a pile that keeps
    // being rained on never finishes seasoning. A Wood Shed has a roof and is left alone.
    void RainOnOpenPiles(WeatherType precipitation)
    {
        if (WoodMoisture.Wets(precipitation))
            WetOpenPiles();
    }

    // Wets the wood in every open Wood Pile. Returns how many piles changed.
    public int WetOpenPiles()
    {
        int stamp = WoodMoisture.WetStamp;
        int changed = 0;
        foreach (WoodPileState pile in piles)
            if (pile.IsOpenWoodPile && pile.RestampWood(stamp))
                changed++;
        return changed;
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    // Testing only (RainDebugControls): the Rain Barrel nearest a point within reach, or null.
    public WoodPileState DebugNearestRainBarrel(Vector3 from, float reach)
    {
        WoodPileState best = null;
        float bestDistance = reach;
        foreach (WoodPileState pile in piles)
        {
            float d = Vector3.Distance(pile.position, from);
            if (pile.kind == PileKind.RainBarrel && d <= bestDistance)
            {
                best = pile;
                bestDistance = d;
            }
        }
        return best;
    }

    // Testing only: sets a barrel to this many litres of Good water (0 empties it).
    public void DebugSetBarrelLitres(WoodPileState barrel, int litres)
    {
        barrel.contents.Clear();
        barrel.Add(WaterQualities.ItemId(WaterQuality.Good), Mathf.Clamp(litres, 0, CapacityOf(barrel.kind)));
        rainCarry.Remove(barrel.id);
        NotifyChanged(barrel);
    }
#endif

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
        site.buildProgress = 0f; // the Hammer finished it (AxeTool.UpdateHammering)

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
            refund.Return(AshRules.ItemId, FireManager.AshReady(hearth));
            fires.RemoveFire(hearth);
        }

        RemovePile(cabin);
        return refund.Describe("Small Cabin");
    }

    // --- The Covered Outdoor Kitchen (Structures Slice 1, phase C) ---
    // Its hearth is an ordinary Campfire (CampfireState, saved by FireManager) that the hearth stage's last Hammer blow puts on the
    // stone slab under the roof (WellsAndOuthouses.FinishSite); like the cabin's it is found again by where it stands, not saved on the pile.

    public static Vector3 KitchenHearthSpot(WoodPileState kitchen) => Setbacks.KitchenLayout.HearthAt(kitchen.position, kitchen.yaw);

    public CampfireState KitchenHearthOf(WoodPileState kitchen) =>
        FireManager.Instance == null ? null : FireManager.Instance.FireNear(KitchenHearthSpot(kitchen), 0.75f);

    // The finished kitchen whose roof the point stands under, or null (the sheltered meal asks this).
    public WoodPileState KitchenRoofAt(Vector3 point)
    {
        foreach (WoodPileState pile in piles)
            if (pile.kind == PileKind.Kitchen && Setbacks.KitchenLayout.UnderRoof(pile.position, pile.yaw, point))
                return pile;
        return null;
    }

    // Dismantle works on the finished kitchen, and on a hearth-built site that has no roof materials in it (or whose roof is built in).
    public bool CanDismantleKitchen(WoodPileState kitchen, out string reason)
    {
        reason = "";
        if (kitchen == null || (kitchen.kind != PileKind.Kitchen && kitchen.kind != PileKind.KitchenRoofSite))
        {
            reason = "not a kitchen";
            return false;
        }
        if (!HammerEquipped)
        {
            reason = "equip the Hammer";
            return false;
        }
        if (kitchen.kind == PileKind.KitchenRoofSite && !kitchen.IsEmpty && !WellsAndOuthouses.MaterialsLocked(kitchen))
        {
            reason = "take the roof materials back first (E)";
            return false;
        }
        // Anything standing in a slot (a countertop, shelving, grill or sink) comes down first; a kitchen site has none.
        if (kitchen.kind == PileKind.Kitchen && KitchenHasAddons(kitchen))
        {
            reason = "remove the add-ons first";
            return false;
        }
        CampfireState hearth = KitchenHearthOf(kitchen);
        if (hearth != null && hearth.lit)
        {
            reason = "put the hearth fire out first";
            return false;
        }
        return true;
    }

    // Takes the kitchen down: half of each cost line back, rounded down (a single item refunds nothing), as dry wood. A hearth-only site
    // gives back half the hearth, and half the roof too once the roof is built in. The hearth goes with it, its Firewood returned.
    public string DismantleKitchen(WoodPileState kitchen, Transform view)
    {
        if (!CanDismantleKitchen(kitchen, out string reason))
            return $"Can't dismantle the kitchen - {reason}.";

        var refund = new StructureRefund(kitchen.position, view);
        if (kitchen.kind == PileKind.Kitchen)
        {
            foreach (WoodStack cost in CostOf(PileKind.Kitchen))
                refund.Return(cost.itemId, cost.count / 2);
        }
        else
        {
            foreach (WoodStack cost in WellsAndOuthouses.KitchenHearthCost())
                refund.Return(cost.itemId, cost.count / 2);
            if (WellsAndOuthouses.MaterialsLocked(kitchen))
                foreach (WoodStack cost in WellsAndOuthouses.KitchenRoofCost())
                    refund.Return(cost.itemId, cost.count / 2);
        }

        CampfireState hearth = KitchenHearthOf(kitchen);
        if (hearth != null)
        {
            FireManager fires = FireManager.Instance;
            refund.Return(FireManager.FirewoodId, Mathf.FloorToInt(hearth.fuelHours / fires.HoursPerFirewood));
            refund.Return(AshRules.ItemId, FireManager.AshReady(hearth));
            fires.RemoveFire(hearth);
        }

        RemovePile(kitchen);
        return refund.Describe("Covered Outdoor Kitchen");
    }

    // --- Kitchen add-ons, batch 1 (Countertop, Shelving, Wood-Fired Grill, Slop Sink) ---
    // They stand in the four fixed slots of a finished Kitchen (Setbacks.KitchenLayout). A slot is only a position: an add-on belongs to the
    // kitchen whose slot its position is in, found each time (KitchenForSlot), so nothing about the link is saved.

    public const string NeedsKitchenReason = "Needs a finished Covered Outdoor Kitchen.";
    public const string SlotTakenReason = "That spot is taken.";
    // A Wood-Fired Grill's fire sits on top of the firebox, drawn smaller than a ring campfire so it fits the slot.
    public const float GrillFireHeight = 0.45f;
    public const float GrillFireScale = 0.6f;

    public bool AnyFinishedKitchen
    {
        get
        {
            foreach (WoodPileState pile in piles)
                if (pile.kind == PileKind.Kitchen)
                    return true;
            return false;
        }
    }

    // The finished kitchen whose slot the point stands in, and which slot (false if none).
    public bool KitchenForSlot(Vector3 point, out WoodPileState kitchen, out int index)
    {
        foreach (WoodPileState pile in piles)
        {
            if (pile.kind != PileKind.Kitchen)
                continue;
            int found = Setbacks.KitchenLayout.SlotIndexOf(pile.position, pile.yaw, point);
            if (found >= 0)
            {
                kitchen = pile;
                index = found;
                return true;
            }
        }
        kitchen = null;
        index = -1;
        return false;
    }

    // The add-on standing in a kitchen's slot, or null.
    public WoodPileState AddonInSlot(WoodPileState kitchen, int index)
    {
        Vector3 slot = Setbacks.KitchenLayout.SlotAt(kitchen.position, kitchen.yaw, index);
        foreach (WoodPileState pile in piles)
            if (pile.IsKitchenAddon && Vector2.Distance(new Vector2(pile.position.x, pile.position.z), new Vector2(slot.x, slot.z)) <= Setbacks.KitchenLayout.SlotTolerance)
                return pile;
        return null;
    }

    public List<WoodPileState> AddonsOf(WoodPileState kitchen)
    {
        var found = new List<WoodPileState>();
        for (int i = 0; i < Setbacks.KitchenLayout.SlotCount; i++)
        {
            WoodPileState addon = AddonInSlot(kitchen, i);
            if (addon != null)
                found.Add(addon);
        }
        return found;
    }

    public bool KitchenHasAddons(WoodPileState kitchen) => AddonsOf(kitchen).Count > 0;

    // Every slot of every finished kitchen within range of a point: where it stands (the pivot's height; the caller finds the ground), the
    // way an add-on there faces, which kitchen and slot, and whether something already stands in it. Placement Mode snaps to these.
    public List<(Vector3 world, float yaw, WoodPileState kitchen, int index, bool taken)> AddonSlots(Vector3 near, float range)
    {
        var slots = new List<(Vector3 world, float yaw, WoodPileState kitchen, int index, bool taken)>();
        foreach (WoodPileState pile in piles)
        {
            if (pile.kind != PileKind.Kitchen || Vector3.Distance(pile.position, near) > range + 3f)
                continue;
            for (int i = 0; i < Setbacks.KitchenLayout.SlotCount; i++)
            {
                Vector3 world = Setbacks.KitchenLayout.SlotAt(pile.position, pile.yaw, i);
                if (Vector3.Distance(world, near) <= range)
                    slots.Add((world, Setbacks.KitchenLayout.SlotYaw(pile.yaw, i), pile, i, AddonInSlot(pile, i) != null));
            }
        }
        return slots;
    }

    // The one place a Kitchen add-on is allowed or refused (CanBuildPileAt hands it over, so the ghost and the real build never differ). It
    // must stand in a free slot of a finished kitchen; it skips the 2 m pile spacing against its own kitchen and that kitchen's other add-ons
    // and the lit-fire rule against that kitchen's hearth and grills, and still keeps both against anything unrelated.
    public bool CanBuildAddonAt(PileKind kind, BuildSpot spot, out string reason)
    {
        if (!KitchenForSlot(spot.point, out WoodPileState kitchen, out int index))
        {
            reason = NeedsKitchenReason;
            return false;
        }
        if (AddonInSlot(kitchen, index) != null)
        {
            reason = SlotTakenReason;
            return false;
        }

        foreach (WoodPileState other in piles)
        {
            if (other == kitchen || (other.IsKitchenAddon && KitchenForSlot(other.position, out WoodPileState ownerKitchen, out _) && ownerKitchen == kitchen))
                continue;
            if (Vector3.Distance(other.position, spot.point) < minSpacing)
            {
                reason = "Too close to another pile.";
                return false;
            }
        }
        FireManager fires = FireManager.Instance;
        if (fires != null)
        {
            CampfireState hearth = KitchenHearthOf(kitchen);
            foreach (CampfireState fire in fires.Fires)
            {
                if (!fire.lit || fire == hearth || IsGrillFire(fire) || Vector3.Distance(fire.position, spot.point) >= minSpacing)
                    continue;
                reason = "Too close to the fire.";
                return false;
            }
        }

        if (NeedsHammerToBuild(kind) && !HammerEquipped)
        {
            reason = WellsAndOuthouses.EquipHammerToBuild;
            return false;
        }
        reason = "";
        return true;
    }

    // The Wood-Fired Grill's fire stands on the firebox, at the slot's middle; a second furnished campfire (FireManager.BuildFurnished).
    public static Vector3 GrillFireSpot(WoodPileState grill) => grill.position + Vector3.up * GrillFireHeight;

    public CampfireState GrillFireOf(WoodPileState grill) =>
        FireManager.Instance == null ? null : FireManager.Instance.FireNear(GrillFireSpot(grill), 0.4f);

    // Whether a fire is a grill's (the fire's prompt says "Wood-Fired Grill"; its view is drawn smaller).
    public bool IsGrillFire(CampfireState fire)
    {
        foreach (WoodPileState pile in piles)
            if (pile.kind == PileKind.Grill && Vector3.Distance(GrillFireSpot(pile), fire.position) < 0.4f)
                return true;
        return false;
    }

    public float FireViewScale(CampfireState fire) => IsGrillFire(fire) ? GrillFireScale : 1f;

    // Whether a fire belongs to a structure (a Grill, a Small Cabin's hearth, a Kitchen's hearth): those are taken down with the structure,
    // never as a plain Campfire.
    public bool IsStructureFire(CampfireState fire)
    {
        if (fire == null)
            return false;
        if (IsGrillFire(fire))
            return true;
        foreach (WoodPileState pile in piles)
        {
            if (pile.kind == PileKind.Cabin && HearthOf(pile) == fire)
                return true;
            if ((pile.kind == PileKind.Kitchen || pile.kind == PileKind.KitchenSite || pile.kind == PileKind.KitchenRoofSite) && KitchenHearthOf(pile) == fire)
                return true;
        }
        return false;
    }

    // Dismantling a Countertop, Grill or Slop Sink: the Hammer, a grill must be out, a sink empty (Shelving is WoodPile's, emptied first).
    public bool CanDismantleAddon(WoodPileState addon, out string reason)
    {
        reason = "";
        if (addon == null || !addon.IsKitchenAddon)
        {
            reason = "not a kitchen add-on";
            return false;
        }
        if (!HammerEquipped)
        {
            reason = "equip the Hammer";
            return false;
        }
        if (addon.kind == PileKind.Grill)
        {
            CampfireState fire = GrillFireOf(addon);
            if (fire != null && fire.lit)
            {
                reason = "put the grill's fire out first";                return false;
            }
        }
        if (addon.kind == PileKind.SlopSink && !addon.IsEmpty)
        {
            reason = $"drain the {PileName(addon.kind)} first";
            return false;
        }
        return true;
    }

    // Half of each cost line back, rounded down, as dry wood; a grill's fire goes with it and its Firewood is returned like the hearth's.
    public string DismantleAddon(WoodPileState addon, Transform view)
    {
        string name = PileName(addon.kind);
        if (!CanDismantleAddon(addon, out string reason))
            return $"Can't take the {name} down - {reason}.";

        var refund = new StructureRefund(addon.position, view);
        foreach (WoodStack cost in CostOf(addon.kind))
            refund.Return(cost.itemId, cost.count / 2);
        if (addon.kind == PileKind.Grill)
        {
            CampfireState fire = GrillFireOf(addon);
            if (fire != null)
            {
                FireManager fires = FireManager.Instance;
                refund.Return(FireManager.FirewoodId, Mathf.FloorToInt(fire.fuelHours / fires.HoursPerFirewood));
                refund.Return(AshRules.ItemId, FireManager.AshReady(fire));
                fires.RemoveFire(fire);
            }
        }
        RemovePile(addon);
        return refund.Describe(name);
    }

    void SpawnPile(WoodPileState pile)
    {
        var go = new GameObject($"{PileName(pile.kind)} {pile.id}");
        go.transform.SetPositionAndRotation(pile.position, Quaternion.Euler(0f, pile.yaw, 0f));
        if (pile.IsShelter)
            go.AddComponent<Shelter>().Bind(pile, BarkMaterial());
        else if (pile.IsPlacedStructure)
            go.AddComponent<PlacedStructure>().Bind(pile, BarkMaterial());
        else
            go.AddComponent<WoodPile>().Bind(pile, BarkMaterial());
        pileViews[pile.id] = go;

        // A grill's fire may already be standing (loaded, or just built): draw it at the grill's size.
        if (pile.kind == PileKind.Grill && FireManager.Instance != null)
        {
            CampfireState fire = GrillFireOf(pile);
            if (fire != null)
                FireManager.Instance.SetViewScale(fire, GrillFireScale);
        }
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
        pile.Add(itemId, count, IsWood(itemId) ? WoodMoisture.WetStamp : 0);
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
