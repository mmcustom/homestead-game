using System.Collections.Generic;
using UnityEngine;

// Structures Slice 1, phase A (Structures_Slice1_Dry_And_Clean.md): the Well and the Outhouse. This component holds every tunable
// (first-pass numbers, Claude, not yet confirmed) and the rules that are not about drawing or interacting (PlacedStructure does
// those): where each may stand (the pure Setbacks class plus the scene's water, creek and ground), the Well's hourly supply and
// flood contamination, the Outhouse's pit and the "sanitary homestead" effect on illness and sleep. Piles and their saved fields
// live in WoodManager (WoodPileState.litres, contaminatedHours, digProgress, builtDay); nothing here is saved itself.
// Creates itself at startup.
public class WellsAndOuthouses : MonoBehaviour
{
    public static WellsAndOuthouses Instance { get; private set; }

    [Header("Setbacks (metres; Ohio's distances turned into game metres)")]
    [Tooltip("A pit privy this far from the creek, a pond or the spring (measured to the water's edge).")]
    public float setbackPrivyFromWaterMetres = 30f;
    public float setbackPrivyFromWellMetres = 30f;
    public float setbackPrivyFromBuildingMetres = 7.5f;
    public float setbackWellFromBuildingMetres = 3f;
    [Tooltip("Used once animal pens exist; nothing places one yet.")]
    public float setbackWellFromPenMetres = 15f;
    [Tooltip("A well must stand higher than any outhouse within this distance, and an outhouse lower than any well.")]
    public float setbackUpgradientRangeMetres = 60f;
    [Tooltip("Ground lower than the average of a ring around it by this much is a hollow: no well there.")]
    public float setbackHollowDepthMetres = 0.5f;
    public float setbackHollowRingMetres = 6f;
    [Tooltip("Ground this close to the creek channel's edge (metres) ...")]
    public float floodplainBandMetres = 25f;
    [Tooltip("... and no higher than the creek's surface plus this (metres) is floodplain: no well or outhouse.")]
    public float floodplainRiseMetres = 1.5f;

    [Header("Well: building (a staged site)")]
    [Tooltip("Medium-depth wells and wells from before depth existed: shovel blows to dig the shaft (one about every 0.9 s, real time only; the clock is not advanced). Shallow and Deep wells take their own (WellWaterRules.Defaults).")]
    public int wellDigBlows = 40;
    [Tooltip("Medium-depth and old wells: Stone for the lining, delivered over trips (5 Stone is 25 kg).")]
    public int wellLiningStone = 10;
    [Tooltip("Cap and windlass.")]
    public int wellCapLogs = 2;
    public int wellCapBranches = 4;

    [Header("Well: supply and water (Medium-depth and old wells; Shallow and Deep take WellWaterRules.Defaults)")]
    public int wellBufferLitres = 150;
    public float wellRecoveryLitresPerHour = 6f;
    [Tooltip("Recovery during a drought (the weather's own drought flag).")]
    public float wellDroughtRecoveryLitresPerHour = 3f;
    [Tooltip("Quality when clear of every setback, and when a privy or pen is too close or uphill.")]
    public WaterQuality wellCleanQuality = WaterQuality.Good;
    public WaterQuality wellFouledQuality = WaterQuality.Questionable;
    [Tooltip("After a flash flood reaches the well its water is Unsafe for this many game hours.")]
    public float wellContaminatedHours = 48f;
    [Tooltip("A flash flood reaches a well within this distance (metres) of the creek channel's edge.")]
    public float wellFloodReachMetres = 40f;
    [Tooltip("Drinking from the well on the spot: litres taken and Hydration given.")]
    public int wellLitresPerDrink = 1;
    public float wellHydrationPerDrink = 20f;

    [Header("Outhouse")]
    public int outhouseLogs = 2;
    public int outhouseBranches = 6;
    public int outhouseTallGrass = 3;
    [Tooltip("In-game days until the pit is full.")]
    public int outhousePitFillDays = 28;
    [Tooltip("Digging a new pit moves the shack this far to the side (metres).")]
    public float outhouseMoveMetres = 2f;
    [Tooltip("Shovel blows (one about every 0.9 s, real time only, each costing the usual swing stamina) to dig a new pit once it is full.")]
    public int outhousePitDigBlows = 10;

    [Header("Wood Shed and wet or dry wood (phase B; first-pass tuning, not confirmed)")]
    [Tooltip("Cost of a Wood Shed (2 Logs + 8 Branches is 24 kg, so it is a staged site). It holds as much as a Wood Pile, which has no limit.")]
    public int woodShedLogs = 2;
    public int woodShedBranches = 8;
    [Tooltip("The shed stands at least this far (metres) from a Small Cabin.")]
    public float setbackShedFromBuildingMetres = 3f;
    [Tooltip("In-game seasons a wet batch of wood takes to dry. One season is 28 days, about 14 real hours; wood in an open Wood Pile is re-wetted by every hour of rain or snow.")]
    public float seasoningSeasons = 1f;
    [Tooltip("Wet wood burns this fraction of its normal time (about 30 percent of the heat goes into driving off the water).")]
    [Range(0.1f, 1f)] public float wetBurnFraction = 0.7f;

    [Header("Covered Outdoor Kitchen (phase C; first-pass tuning, not confirmed)")]
    [Tooltip("Stage 1, the hearth (4 Stone + 2 Clay is 27 kg, so a staged site): Stone first, then Clay.")]
    public int kitchenStone = 4;
    public int kitchenClay = 2;
    [Tooltip("Stage 2, the roof (2 Logs + 6 Branches + 4 Tall Grass is about 22 kg): delivered once the hearth is built.")]
    public int kitchenLogs = 2;
    public int kitchenBranches = 6;
    public int kitchenTallGrass = 4;
    [Tooltip("The roof's edge keeps this far (metres) clear of any other structure, and of any tree trunk (fire safety).")]
    public float setbackKitchenFromStructureMetres = 3f;
    public float setbackKitchenFromTreeMetres = 3f;
    [Tooltip("The roof's reach from its centre (its corner), so those gaps are measured from its edge.")]
    public float kitchenRoofRadiusMetres = 2.3f;
    [Tooltip("A sheltered meal (eaten under the kitchen roof): Health recovers this much faster (0.25 is 25 percent) for this many game hours. There is no Morale stat yet.")]
    public float shelteredMealRecoveryBonus = 0.25f;
    public float shelteredMealHours = 4f;

    [Header("Sanitary homestead")]
    [Tooltip("A working (not full) outhouse within this distance (metres) of where the player sleeps makes the homestead sanitary.")]
    public float sanitaryRadiusMetres = 60f;
    [Tooltip("Multiplies the chance of falling ill from water and food: sanitary, and not.")]
    public float sanitaryIllnessMultiplier = 0.85f;
    public float unsanitaryIllnessMultiplier = 1f;
    [Tooltip("Added to the Health recovery multiplier while sleeping in a sanitary homestead.")]
    public float sanitarySleepRecoveryBonus = 0.15f;

    readonly Dictionary<int, float> recoveryCarry = new Dictionary<int, float>();
    bool linked;
    TimeManager linkedTime;
    WaterSource water;
    float nextWaterSearch;
    bool hasSleepPlace;
    Vector3 lastSleepPlace;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => Instance = null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Create()
    {
        if (Instance != null)
            return;
        var go = new GameObject("Wells And Outhouses");
        go.hideFlags = HideFlags.DontSave;
        DontDestroyOnLoad(go);
        go.AddComponent<WellsAndOuthouses>();
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
        if (!linked && Instance == this && TimeManager.Instance != null)
        {
            linkedTime = TimeManager.Instance;
            linkedTime.HourChanged += OnHour;
            linked = true;
        }
    }

    void OnDestroy()
    {
        if (Instance != this)
            return;
        if (linked && linkedTime != null)
            linkedTime.HourChanged -= OnHour;
        Instance = null;
    }

    // --- The Hammer (Mike's rule: the Build screen is permanent and the Hammer does the construction; the Craft tab never needs it) ---
    // Free without the Hammer: the ghost, placing a staged site, delivering materials, taking materials back. The Hammer: the click
    // that builds a single-step structure (WoodManager.NeedsHammerToBuild), finishing a staged site (pounding it with the Hammer at the site,
    // like the Small Cabin), taking an empty site down, and Dismantle. The Shovel does the digging (the shaft, a new pit).

    public const string EquipHammerToBuild = "Equip the Hammer to build.";

    // --- Staged sites: one mechanism for every build too heavy to carry in a single trip ---
    // Mike (2026-10-19): a build whose materials weigh more than about 15 kg is a staged site, like the Small Cabin: placed free,
    // stocked over any number of trips (R; E takes it back), finished by pounding with the Hammer at the site. The
    // Well Site and the Outhouse Site use this; the Wood Shed and the Kitchen (phases B and C) are meant to join by adding a row to
    // the four data methods below (FinishedKind, FinishedCost, DeliveryOrder, RequiresDig) and a PileKind.

    [Header("Staged sites")]
    [Tooltip("A build whose materials weigh more than this (kg) is a staged site rather than one trip (first-pass tuning, not confirmed).")]
    public float stagedSiteWeightKg = 15f;

    // Mike (2026-10-19, later): the Water Barrel (24 kg), Rain Barrel (26 kg) and Food Cache (20 kg) are staged too. Their finished
    // forms are WoodPiles (storage); a completed site is respawned as one (WoodManager.Respawn) at the same place and facing.
    public static bool IsStagedSite(PileKind kind)
    {
        switch (kind)
        {
            case PileKind.WellSite:
            case PileKind.OuthouseSite:
            case PileKind.WaterBarrelSite:
            case PileKind.RainBarrelSite:
            case PileKind.FoodCacheSite:
            case PileKind.WoodShedSite:
            case PileKind.LeachHopperSite:
            case PileKind.KitchenSite:
            case PileKind.KitchenRoofSite:
                return true;
            default:
                return false;
        }
    }

    // The structure a site turns into.
    public static PileKind FinishedKind(PileKind site)
    {
        switch (site)
        {
            case PileKind.OuthouseSite: return PileKind.Outhouse;
            case PileKind.WaterBarrelSite: return PileKind.WaterBarrel;
            case PileKind.RainBarrelSite: return PileKind.RainBarrel;
            case PileKind.FoodCacheSite: return PileKind.FoodCache;
            case PileKind.WoodShedSite: return PileKind.WoodShed;
            case PileKind.LeachHopperSite: return PileKind.LeachHopper;
            case PileKind.KitchenSite: return PileKind.KitchenRoofSite;
            case PileKind.KitchenRoofSite: return PileKind.Kitchen;
            default: return PileKind.Well;
        }
    }

    // The structure a site finally becomes, through every stage (the Covered Kitchen's hearth site passes through a roof site first).
    public static PileKind FinalKind(PileKind site)
    {
        PileKind kind = site;
        for (int guard = 0; guard < 4 && IsStagedSite(kind); guard++)
            kind = FinishedKind(kind);
        return kind;
    }

    // What the finished structure takes in total, delivered to the site over trips. The storage kinds read WoodManager's own costs
    // (the same ones Dismantle halves), falling back to the defaults when it isn't running (Edit Mode tests).
    public static WoodStack[] FinishedCost(PileKind site)
    {
        switch (site)
        {
            case PileKind.OuthouseSite: return OuthouseCost();
            case PileKind.WaterBarrelSite: return Stored(PileKind.WaterBarrel, (WoodManager.LogsId, 3));
            case PileKind.RainBarrelSite: return Stored(PileKind.RainBarrel, (WoodManager.LogsId, 3), (WoodManager.BranchesId, 2));
            case PileKind.FoodCacheSite: return Stored(PileKind.FoodCache, (WoodManager.LogsId, 2), (WoodManager.BranchesId, 4));
            case PileKind.WoodShedSite: return WoodShedCost();
            case PileKind.LeachHopperSite: return LeachHopperCost();
            case PileKind.KitchenSite: return KitchenHearthCost();
            case PileKind.KitchenRoofSite: return KitchenRoofCost();
            default: return WellCost();
        }
    }

    // What this one site takes in total: its kind's cost, except a Well's Stone lining, which is its own depth's (WellNumbers).
    public static WoodStack[] FinishedCostFor(WoodPileState site) =>
        site.kind == PileKind.WellSite || site.kind == PileKind.Well ? WellCostFor(site) : FinishedCost(site.kind);

    static WoodStack[] Stored(PileKind finished, params (string id, int count)[] fallback)
    {
        if (WoodManager.Instance != null)
            return WoodManager.Instance.CostOf(finished);
        var cost = new WoodStack[fallback.Length];
        for (int i = 0; i < fallback.Length; i++)
            cost[i] = new WoodStack { itemId = fallback[i].id, count = fallback[i].count };
        return cost;
    }

    // A stage that must be done before anything is delivered (the Well's shaft, dug with the Shovel; the Outhouse has none).
    public static bool RequiresDig(PileKind site) => site == PileKind.WellSite;

    // The order materials are taken in: a group opens once every earlier group is full. The Well lines the shaft with Stone before
    // the cap timber is taken; the Outhouse's materials go in any order.
    public static string[][] DeliveryOrder(PileKind site)
    {
        if (site == PileKind.OuthouseSite)
            return new[] { new[] { WoodManager.LogsId, WoodManager.BranchesId, WoodManager.TallGrassId } };
        if (site == PileKind.WaterBarrelSite)
            return new[] { new[] { WoodManager.LogsId } };
        if (site == PileKind.KitchenSite)
            return new[] { new[] { WoodManager.StoneId }, new[] { WoodManager.ClayId } }; // the Stone first, then the Clay
        if (site == PileKind.KitchenRoofSite)
            return new[] { new[] { WoodManager.LogsId, WoodManager.BranchesId, WoodManager.TallGrassId } };
        if (site == PileKind.LeachHopperSite)
            return new[] { new[] { WoodManager.LogsId, WoodManager.TallGrassId } };
        if (site == PileKind.RainBarrelSite || site == PileKind.FoodCacheSite || site == PileKind.WoodShedSite)
            return new[] { new[] { WoodManager.LogsId, WoodManager.BranchesId } };
        return new[] { new[] { WoodManager.StoneId }, new[] { WoodManager.LogsId, WoodManager.BranchesId } };
    }

    // Every material a site takes, in delivery order.
    public static List<string> DeliveryIds(PileKind site)
    {
        var ids = new List<string>();
        foreach (string[] group in DeliveryOrder(site))
            ids.AddRange(group);
        return ids;
    }

    public static int NeedOf(PileKind site, string itemId)
    {
        foreach (WoodStack c in FinishedCost(site))
            if (c.itemId == itemId)
                return c.count;
        return 0;
    }

    // The same for one site, so a Well asks for the Stone its own depth needs.
    public static int NeedOf(WoodPileState site, string itemId)
    {
        foreach (WoodStack c in FinishedCostFor(site))
            if (c.itemId == itemId)
                return c.count;
        return 0;
    }

    // Total weight of a list of materials (kg), from the item assets.
    public static float WeightKg(IEnumerable<WoodStack> cost)
    {
        float total = 0f;
        foreach (WoodStack c in cost)
        {
            ItemDefinition item = ItemDatabase.Get(c.itemId);
            if (item != null)
                total += item.WeightKg * c.count;
        }
        return total;
    }

    // Whether a build of this cost is too heavy to carry in one go and so is a staged site (strictly more than the cutoff).
    public static bool NeedsStaging(IEnumerable<WoodStack> cost)
    {
        float cutoff = Instance != null ? Instance.stagedSiteWeightKg : 15f;
        return WeightKg(cost) > cutoff;
    }


    // What a site still lacks: "the shaft dug (Shovel)", then "5 Stone", then "2 Logs, 4 Branches". Null when it is fully stocked.
    public static string SiteNeeds(WoodPileState site)
    {
        if (RequiresDig(site.kind) && site.digProgress < 1f)
            return "the shaft dug (Shovel)";
        var parts = new List<string>();
        foreach (WoodStack c in FinishedCostFor(site))
        {
            int missing = c.count - site.Count(c.itemId);
            if (missing > 0)
                parts.Add($"{missing} {ItemDatabase.Get(c.itemId)?.DisplayName ?? c.itemId}");
        }
        return parts.Count == 0 ? null : string.Join(", ", parts);
    }

    // What a site takes now: nothing until a required dig is done, then each material up to what the structure needs, group by group
    // (partial deliveries are fine, over-delivery is refused).
    public static bool SiteAccepts(WoodPileState site, string itemId)
    {
        if (RequiresDig(site.kind) && site.digProgress < 1f)
            return false;
        bool earlierFull = true;
        foreach (string[] group in DeliveryOrder(site.kind))
        {
            if (System.Array.IndexOf(group, itemId) >= 0)
                return earlierFull && site.Count(itemId) < NeedOf(site, itemId);
            foreach (string id in group)
                if (site.Count(id) < NeedOf(site, id))
                    earlierFull = false;
        }
        return false;
    }

    // Dug (if it needs digging) and stocked.
    public static bool SiteComplete(WoodPileState site) => SiteNeeds(site) == null;

    // --- Pounding with the Hammer (Mike, 2026-10-19: no "Complete" button; equip the Hammer at a fully stocked site and hold click) ---
    // Each blow advances the site's buildProgress (saved on the site), the way the Shovel's blows advance the Well shaft (DigBlow);
    // blows take real time only (no clock jump); at 1 the site becomes the finished structure in place. Once it has begun the delivered materials are locked in.

    [Header("Pounding with the Hammer (first-pass tuning, not confirmed)")]
    [Tooltip("Hammer blows (one about every 0.9 s) to finish each stocked site; heavier structures take more.")]
    public int wellBuildBlows = 30;
    public int outhouseBuildBlows = 20;
    public int waterBarrelBuildBlows = 15;
    public int rainBarrelBuildBlows = 16;
    public int foodCacheBuildBlows = 12;
    public int woodShedBuildBlows = 25;
    [Tooltip("Covered Outdoor Kitchen: the hearth stage and the roof stage are each pounded in turn.")]
    public int kitchenHearthBuildBlows = 15;
    public int kitchenRoofBuildBlows = 30;

    [Header("Kitchen add-ons, batch 1 (first-pass costs, Claude's, not confirmed; each is a single-step build under the staged line)")]
    [Tooltip("Kitchen-Countertop: 1 Log + 4 Branches (12 kg).")]
    public int countertopLogs = 1;
    public int countertopBranches = 4;
    [Tooltip("Kitchen-Shelves: 10 Branches + 6 Sticks (10.6 kg).")]
    public int shelvingBranches = 10;
    public int shelvingSticks = 6;
    [Tooltip("Kitchen-Grill: 2 Stone + 1 Clay (13.5 kg).")]
    public int grillStone = 2;
    public int grillClay = 1;
    [Tooltip("Kitchen-Sink: 1 Log + 1 Clay + 2 Branches (13.5 kg).")]
    public int slopSinkLogs = 1;
    public int slopSinkClay = 1;
    public int slopSinkBranches = 2;
    [Tooltip("Litres the Kitchen-Sink holds, of any quality (WoodManager.CapacityOf).")]
    [Min(1)] public int slopSinkLitres = 20;
    public int cabinBuildBlows = 60;

    public int BuildBlows(PileKind site)
    {
        switch (site)
        {
            case PileKind.OuthouseSite: return outhouseBuildBlows;
            case PileKind.WaterBarrelSite: return waterBarrelBuildBlows;
            case PileKind.RainBarrelSite: return rainBarrelBuildBlows;
            case PileKind.FoodCacheSite: return foodCacheBuildBlows;
            case PileKind.WoodShedSite: return woodShedBuildBlows;
            case PileKind.LeachHopperSite: return PotashRules.HopperBuildBlows;
            case PileKind.KitchenSite: return kitchenHearthBuildBlows;
            case PileKind.KitchenRoofSite: return kitchenRoofBuildBlows;
            case PileKind.CabinSite: return cabinBuildBlows;
            default: return wellBuildBlows;
        }
    }

    // The progress after one blow (pure): efficiency / blows of the whole, capped at 1.
    public static float AdvanceBuild(float progress, float efficiency, int blows) =>
        Mathf.Min(1f, progress + efficiency / Mathf.Max(1, blows));

    // True once the Hammer has begun on the site: its materials are built in and E no longer takes them back.
    public static bool MaterialsLocked(WoodPileState site) => site.buildProgress > 0f;

    // One Hammer blow on a fully stocked site. Returns true on the blow that finishes it (the caller then completes it).
    public bool BuildBlow(WoodPileState site, float efficiency)
    {
        site.buildProgress = AdvanceBuild(site.buildProgress, efficiency, BuildBlows(site.kind));
        return site.buildProgress >= 1f;
    }

    // What the player is told when a site is finished.
    public static string FinishedMessage(PileKind site)
    {
        if (site == PileKind.KitchenSite)
            return "Hearth built - now deliver the roof: " + KitchenRoofText() + ".";
        string name = WoodManager.PileName(FinishedKind(site));
        if (site == PileKind.LeachHopperSite)
            return KnowHowRules.HopperBuilt;
        return site == PileKind.WellSite ? "Well built - water fills it from below." : $"{name} built.";
    }

    // Turns a fully stocked site into the finished structure in place (the Hammer has pounded it to 1): the materials are used up.
    // A Well starts with a full buffer, an Outhouse with an empty pit dug today.
    public bool FinishSite(WoodPileState site)
    {
        if (site == null || !IsStagedSite(site.kind) || SiteNeeds(site) != null)
            return false;
        PileKind was = site.kind;
        site.contents.Clear();
        site.kind = FinishedKind(site.kind);
        site.buildProgress = 0f;
        if (site.kind == PileKind.Well)
        {
            site.litres = NumbersOf(site).bufferLitres;
            site.contaminatedHours = 0f;
        }
        site.builtDay = Today;
        // The hearth stage's last blow puts the hearth on its slab: an ordinary Campfire, unlit and out of fuel until it is tended.
        if (was == PileKind.KitchenSite && FireManager.Instance != null && FireManager.Instance.FireNear(WoodManager.KitchenHearthSpot(site), 0.75f) == null)
            FireManager.Instance.BuildFurnished(WoodManager.KitchenHearthSpot(site), site.yaw);
        // A barrel or cache is a WoodPile, not a PlacedStructure: swap the view, keeping the site's position and facing (a Rain
        // Barrel stays where the site was snapped to the eave).
        if (!site.IsPlacedStructure)
            WoodManager.Instance.Respawn(site);
        else
            WoodManager.Instance.NotifyChanged(site);
        return true;
    }

    // --- Wood Shed facing (the open front is the shed's local +Z) ---

    static readonly string[] Compass = { "north", "north-east", "east", "south-east", "south", "south-west", "west", "north-west" };

    public static string CompassWord(float bearing) => Compass[Mathf.RoundToInt(Mathf.Repeat(bearing, 360f) / 45f) % 8];

    // The yaw that turns the open front away from the wind: toward the bearing the wind blows to. (WindDirection is the bearing it blows FROM; 0 is north, +Z.)
    public static float DownwindFor(float windFromBearing) => Mathf.Repeat(windFromBearing + 180f, 360f);

    public static float DownwindYaw() => WeatherManager.Instance != null ? DownwindFor(WeatherManager.Instance.WindDirection) : 0f;

    // Whether a front facing yaw is within a quarter turn of straight downwind (so the wind does not blow in).
    public static bool OpensAwayFromWind(float yaw, float windFromBearing) =>
        Mathf.Abs(Mathf.DeltaAngle(yaw, DownwindFor(windFromBearing))) <= 90f;

    // What Placement Mode says beside "Place Wood Shed Site here": which way it opens, and how that sits with the wind.
    public static string ShedOpeningNote(float yaw)
    {
        string opens = "opens toward the " + CompassWord(yaw);
        WeatherManager weather = WeatherManager.Instance;
        if (weather == null)
            return opens;
        return OpensAwayFromWind(yaw, weather.WindDirection)
            ? opens + ", away from the " + weather.WindFromWords + " wind"
            : opens + ", into the " + weather.WindFromWords + " wind (turn it with the wheel)";
    }

    // --- Costs (used by WoodManager) ---

    public static WoodStack[] WoodShedCost()
    {
        WellsAndOuthouses w = Instance;
        return new[]
        {
            new WoodStack { itemId = WoodManager.LogsId, count = w != null ? w.woodShedLogs : 2 },
            new WoodStack { itemId = WoodManager.BranchesId, count = w != null ? w.woodShedBranches : 8 },
        };
    }

    // A Leach Hopper (Potash B): 3 Logs and 4 Tall Grass, 24.4 kg, so a staged site. The numbers are PotashRules's.
    public static WoodStack[] LeachHopperCost() => new[]
    {
        new WoodStack { itemId = WoodManager.LogsId, count = PotashRules.HopperLogs },
        new WoodStack { itemId = WoodManager.TallGrassId, count = PotashRules.HopperTallGrass },
    };

    public static WoodStack[] CountertopCost()
    {
        WellsAndOuthouses w = Instance;
        return new[]
        {
            new WoodStack { itemId = WoodManager.LogsId, count = w != null ? w.countertopLogs : 1 },
            new WoodStack { itemId = WoodManager.BranchesId, count = w != null ? w.countertopBranches : 4 },
        };
    }

    public static WoodStack[] ShelvingCost()
    {
        WellsAndOuthouses w = Instance;
        return new[]
        {
            new WoodStack { itemId = WoodManager.BranchesId, count = w != null ? w.shelvingBranches : 10 },
            new WoodStack { itemId = WoodManager.SticksId, count = w != null ? w.shelvingSticks : 6 },
        };
    }

    public static WoodStack[] GrillCost()
    {
        WellsAndOuthouses w = Instance;
        return new[]
        {
            new WoodStack { itemId = WoodManager.StoneId, count = w != null ? w.grillStone : 2 },
            new WoodStack { itemId = WoodManager.ClayId, count = w != null ? w.grillClay : 1 },
        };
    }

    public static WoodStack[] SlopSinkCost()
    {
        WellsAndOuthouses w = Instance;
        return new[]
        {
            new WoodStack { itemId = WoodManager.LogsId, count = w != null ? w.slopSinkLogs : 1 },
            new WoodStack { itemId = WoodManager.ClayId, count = w != null ? w.slopSinkClay : 1 },
            new WoodStack { itemId = WoodManager.BranchesId, count = w != null ? w.slopSinkBranches : 2 },
        };
    }

    public static WoodStack[] KitchenHearthCost()
    {
        WellsAndOuthouses w = Instance;
        return new[]
        {
            new WoodStack { itemId = WoodManager.StoneId, count = w != null ? w.kitchenStone : 4 },
            new WoodStack { itemId = WoodManager.ClayId, count = w != null ? w.kitchenClay : 2 },
        };
    }

    public static WoodStack[] KitchenRoofCost()
    {
        WellsAndOuthouses w = Instance;
        return new[]
        {
            new WoodStack { itemId = WoodManager.LogsId, count = w != null ? w.kitchenLogs : 2 },
            new WoodStack { itemId = WoodManager.BranchesId, count = w != null ? w.kitchenBranches : 6 },
            new WoodStack { itemId = WoodManager.TallGrassId, count = w != null ? w.kitchenTallGrass : 4 },
        };
    }

    // Both stages together: what the finished kitchen is worth, and so what Dismantle halves line by line.
    public static WoodStack[] KitchenCost()
    {
        var all = new List<WoodStack>(KitchenHearthCost());
        all.AddRange(KitchenRoofCost());
        return all.ToArray();
    }

    // e.g. "2 Logs, 6 Branches, 4 Tall Grass".
    public static string KitchenRoofText()
    {
        var parts = new List<string>();
        foreach (WoodStack c in KitchenRoofCost())
            parts.Add($"{c.count} {ItemDatabase.Get(c.itemId)?.DisplayName ?? c.itemId}");
        return string.Join(", ", parts);
    }

    public static WoodStack[] WellCost()
    {
        WellsAndOuthouses w = Instance;
        return new[]
        {
            new WoodStack { itemId = WoodManager.StoneId, count = w != null ? w.wellLiningStone : 10 },
            new WoodStack { itemId = WoodManager.LogsId, count = w != null ? w.wellCapLogs : 2 },
            new WoodStack { itemId = WoodManager.BranchesId, count = w != null ? w.wellCapBranches : 4 },
        };
    }

    // The finished Well's total for this one well: the lining Stone its depth needs, and the cap.
    public static WoodStack[] WellCostFor(WoodPileState well)
    {
        WoodStack[] cost = WellCost();
        for (int i = 0; i < cost.Length; i++)
            if (cost[i].itemId == WoodManager.StoneId)
                cost[i] = new WoodStack { itemId = WoodManager.StoneId, count = WellNumbers(well).liningStone };
        return cost;
    }

    public static WoodStack[] OuthouseCost()
    {
        WellsAndOuthouses w = Instance;
        return new[]
        {
            new WoodStack { itemId = WoodManager.LogsId, count = w != null ? w.outhouseLogs : 2 },
            new WoodStack { itemId = WoodManager.BranchesId, count = w != null ? w.outhouseBranches : 6 },
            new WoodStack { itemId = WoodManager.TallGrassId, count = w != null ? w.outhouseTallGrass : 3 },
        };
    }

    // --- Where they may stand ---

    SetbackRules Rules => new SetbackRules
    {
        privyFromWater = setbackPrivyFromWaterMetres,
        privyFromWell = setbackPrivyFromWellMetres,
        privyFromBuilding = setbackPrivyFromBuildingMetres,
        wellFromBuilding = setbackWellFromBuildingMetres,
        wellFromPen = setbackWellFromPenMetres,
        shedFromBuilding = setbackShedFromBuildingMetres,
        kitchenFromStructure = setbackKitchenFromStructureMetres,
        kitchenFromTree = setbackKitchenFromTreeMetres,
        kitchenRadius = kitchenRoofRadiusMetres,
        upgradientRange = setbackUpgradientRangeMetres,
        hollowDepth = setbackHollowDepthMetres,
        floodBand = floodplainBandMetres,
        floodRise = floodplainRiseMetres,
    };

    WaterSource Source()
    {
        if (water == null && Time.unscaledTime >= nextWaterSearch)
        {
            nextWaterSearch = Time.unscaledTime + 1f;
            water = FindAnyObjectByType<WaterSource>();
        }
        return water;
    }

    static float GroundAt(float x, float z, float fallback)
    {
        Terrain terrain = Terrain.activeTerrain;
        return terrain != null ? terrain.SampleHeight(new Vector3(x, 0f, z)) + terrain.transform.position.y : fallback;
    }

    // includeSites: an Outhouse Site counts when placing something (a planned outhouse keeps its distances) but does not foul a well
    // until it is built.
    List<SiteInfo> Sites(WoodPileState ignore, bool includeSites = true)
    {
        var sites = new List<SiteInfo>();
        WoodManager wood = WoodManager.Instance;
        if (wood == null)
            return sites;
        foreach (WoodPileState pile in wood.Piles)
        {
            if (pile == ignore)
                continue;
            switch (pile.kind)
            {
                case PileKind.Cabin: sites.Add(new SiteInfo { kind = SiteKind.Building, position = pile.position, name = "Small Cabin" }); break;
                case PileKind.CabinSite: sites.Add(new SiteInfo { kind = SiteKind.Building, position = pile.position, name = "Small Cabin site" }); break;
                case PileKind.Well:
                case PileKind.WellSite: sites.Add(new SiteInfo { kind = SiteKind.Well, position = pile.position, name = "well" }); break;
                case PileKind.OuthouseSite:
                    if (includeSites)
                        sites.Add(new SiteInfo { kind = SiteKind.Outhouse, position = pile.position, name = "outhouse" });
                    break;
                case PileKind.Outhouse: sites.Add(new SiteInfo { kind = SiteKind.Outhouse, position = pile.position, name = "outhouse" }); break;
            }
        }
        return sites;
    }

    // Every built structure but the one being placed, with how far each reaches from its position (the kitchen's gap is clear air).
    List<SiteInfo> KitchenNeighbours(WoodPileState ignore)
    {
        var found = new List<SiteInfo>();
        WoodManager wood = WoodManager.Instance;
        if (wood == null)
            return found;
        foreach (WoodPileState pile in wood.Piles)
        {
            if (pile == ignore || pile.kind == PileKind.Felled || pile.IsKitchenAddon)
                continue; // a kitchen's own countertop, shelving, grill or sink is part of it, not a neighbour
            float radius;
            switch (pile.kind)
            {
                case PileKind.Cabin:
                case PileKind.CabinSite: radius = 2.5f; break;      // half the cabin's depth
                case PileKind.Kitchen:
                case PileKind.KitchenSite:
                case PileKind.KitchenRoofSite: radius = kitchenRoofRadiusMetres; break;
                case PileKind.WoodShed:
                case PileKind.WoodShedSite:
                case PileKind.Tent:
                case PileKind.LeanTo:
                case PileKind.TarpShelter: radius = 1.2f; break;
                default: radius = 0.7f; break;
            }
            found.Add(new SiteInfo { kind = SiteKind.Building, position = pile.position, name = WoodManager.PileName(pile.kind), radius = radius });
        }
        return found;
    }

    // Metres from the point to the creek channel's edge (negative inside the channel).
    public float CreekEdgeDistance(Vector3 point)
    {
        int segment = CreekFlow.NearestSegment(new Vector2(point.x, point.z), out float distance);
        return distance - CreekFlow.HalfWidthOfSegment(segment);
    }

    List<WaterEdge> WaterEdges(Vector3 point)
    {
        var edges = new List<WaterEdge> { new WaterEdge { name = "the creek", distanceMetres = CreekEdgeDistance(point) } };
        WaterSource source = Source();
        if (source != null)
        {
            foreach (WaterSource.QualityZone zone in source.Zones)
            {
                float d = Vector2.Distance(new Vector2(point.x, point.z), new Vector2(zone.center.x, zone.center.z)) - zone.radius;
                edges.Add(new WaterEdge { name = zone.name, distanceMetres = d });
            }
        }
        return edges;
    }

    // Ground the creek would reach: near the channel and no higher than its surface (plus a rise). The surface is read off the
    // Water layer at the channel's centre nearest the point; if that can't be found the ground there stands in for it.
    bool Floods(Vector3 point)
    {
        int segment = CreekFlow.NearestSegment(new Vector2(point.x, point.z), out float distance);
        float edge = distance - CreekFlow.HalfWidthOfSegment(segment);
        if (edge > floodplainBandMetres)
            return false;
        Vector2 centre = CreekFlow.PathPoint(segment);
        float surface = Animal.IsOverWater(new Vector3(centre.x, point.y, centre.y), out float found) ? found : GroundAt(centre.x, centre.y, point.y);
        return Setbacks.Floods(point.y, surface, edge, Rules);
    }

    bool InHollow(Vector3 point)
    {
        var ring = new List<float>();
        for (int i = 0; i < 8; i++)
        {
            float a = i / 8f * Mathf.PI * 2f;
            ring.Add(GroundAt(point.x + Mathf.Cos(a) * setbackHollowRingMetres, point.z + Mathf.Sin(a) * setbackHollowRingMetres, point.y));
        }
        return Setbacks.InHollow(point.y, ring, setbackHollowDepthMetres);
    }

    // The one place a Well Site or an Outhouse is allowed or refused (WoodManager.CanBuildPileAt asks it, so the ghost and the real
    // build never differ). ignore is the structure itself when an Outhouse checks a new pit spot.
    public bool CanPlace(PileKind kind, Vector3 point, WoodPileState ignore, out string reason)
    {
        bool well = kind == PileKind.WellSite || kind == PileKind.Well;
        SetbackRules rules = Rules;

        if (well)
        {
            WaterSource source = Source();
            string zone = source != null ? source.ZoneAt(point) : null;
            if (zone != null)
            {
                reason = $"Not inside {zone}: a well can't stand in the water's own zone.";
                return false;
            }
        }
        if (Floods(point))
        {
            reason = "This ground floods.";
            return false;
        }
        if (well && InHollow(point))
        {
            reason = "In a hollow: rain would run into the well.";
            return false;
        }

        // A Leach Hopper stands on any dry ground (the floodplain test above is its only rule).
        if (kind == PileKind.LeachHopperSite || kind == PileKind.LeachHopper)
        {
            reason = "";
            return true;
        }

        // A Wood Shed only needs dry ground clear of the cabin (the floodplain test above is its other rule).
        if (kind == PileKind.WoodShedSite || kind == PileKind.WoodShed)
            return Setbacks.CanPlaceShed(point, Sites(ignore), rules, out reason);

        // A Covered Kitchen keeps its roof edge clear of every other structure and every tree (a fire under a roof).
        if (kind == PileKind.KitchenSite || kind == PileKind.KitchenRoofSite || kind == PileKind.Kitchen)
        {
            bool treeNear = WoodManager.Instance != null && WoodManager.Instance.FindTree(point, kitchenRoofRadiusMetres + setbackKitchenFromTreeMetres) >= 0;
            return Setbacks.CanPlaceKitchen(point, KitchenNeighbours(ignore), treeNear, rules, out reason);
        }

        List<SiteInfo> sites = Sites(ignore);
        if (!well)
            return Setbacks.CanPlaceOuthouse(point, sites, WaterEdges(point), rules, out reason);
        if (!Setbacks.CanPlaceWell(point, sites, rules, out reason))
            return false;

        // Water too deep to hand-dig (Well Water Table: past 9 m, a drilled well needs a rig). Skipped when no permanent water can be found.
        if (kind == PileKind.WellSite && WellWaterAt(point, out WellWaterResult water) && !water.Diggable)
        {
            reason = WellPreview.TooDeepMessage;
            return false;
        }
        return true;
    }

    // --- Well Water Table B2: what the Well Site ghost shows ---

    // The surface of the nearest permanent water (the creek's channel, a pond, the Spring Hollow) and its edge distance in metres. The
    // surface is the Water layer's own height at the channel centre or the zone centre (the same read Floods uses); where that finds no
    // water the ground there stands in. The creek is walked segment by segment (about 325 one-metre segments), which costs microseconds, so
    // nothing is cached: the answer follows a moving pond level too.
    public bool NearestWater(Vector3 point, out float surface, out string name, out float edgeMetres)
    {
        int segment = CreekFlow.NearestSegment(new Vector2(point.x, point.z), out float distance);
        edgeMetres = distance - CreekFlow.HalfWidthOfSegment(segment);
        Vector2 centre = CreekFlow.PathPoint(segment);
        surface = SurfaceAt(centre.x, centre.y, point.y);
        name = "the creek";

        WaterSource source = Source();
        if (source != null)
        {
            foreach (WaterSource.QualityZone zone in source.Zones)
            {
                float d = Vector2.Distance(new Vector2(point.x, point.z), new Vector2(zone.center.x, zone.center.z)) - zone.radius;
                if (d < edgeMetres)
                {
                    edgeMetres = d;
                    surface = SurfaceAt(zone.center.x, zone.center.z, point.y);
                    name = zone.name;
                }
            }
        }
        return true;
    }

    static float SurfaceAt(float x, float z, float referenceY) =>
        Animal.IsOverWater(new Vector3(x, referenceY, z), out float found) ? found : GroundAt(x, z, referenceY);

    // Depth to water under a spot (WellWater, from the ground height there and the nearest permanent water surface).
    public bool WellWaterAt(Vector3 point, out WellWaterResult result)
    {
        result = default;
        if (!NearestWater(point, out float surface, out _, out _))
            return false;
        result = WellWater.Compute(point.y, surface, WellRules());
        return true;
    }

    // What the ghost says at a spot: legal and reason are what CanPlace already answered for it (so the rules are asked once). Read-only.
    public WellPreviewResult PreviewWell(Vector3 point, bool legal, string reason)
    {
        WellWaterResult? water = WellWaterAt(point, out WellWaterResult found) ? found : (WellWaterResult?)null;

        var hazards = new List<PreviewHazard>();
        foreach (SiteInfo site in Sites(null))
        {
            if (site.kind == SiteKind.Outhouse || site.kind == SiteKind.AnimalPen)
                hazards.Add(new PreviewHazard { name = site.kind == SiteKind.Outhouse ? "outhouse" : "animal pen", metres = Setbacks.Flat(point, site.position) });
        }
        foreach (WaterEdge edge in WaterEdges(point))
            hazards.Add(new PreviewHazard { name = edge.name, metres = edge.distanceMetres });

        return WellPreview.Build(legal ? null : reason, water, CreekEdgeDistance(point), wellFloodReachMetres, hazards);
    }

    // --- The Covered Outdoor Kitchen's sheltered meal ---

    // A meal eaten standing under a finished kitchen's roof (Food.Consume asks after each one): Health recovers faster for a few game
    // hours (there is no Morale stat in the game yet; SurvivalManager.NoteShelteredMeal). Returns the words to add to the eating message, or null.
    public static string ShelteredMeal(ItemDefinition item)
    {
        WellsAndOuthouses w = Instance;
        PlayerController player = w != null ? FindAnyObjectByType<PlayerController>() : null;
        if (player == null || item == null || item.IsDrink || item.HungerRestored <= 0f || WoodManager.Instance == null || SurvivalManager.Instance == null)
            return null;
        if (WoodManager.Instance.KitchenRoofAt(player.transform.position) == null)
            return null;
        SurvivalManager.Instance.NoteShelteredMeal(w.shelteredMealHours, w.shelteredMealRecoveryBonus);
        return "a sheltered meal";
    }

    // --- The Well's water ---

    // Unsafe while flood water is in it; the fouled quality when a privy or pen is too close or uphill (or it was built before
    // that was a rule); otherwise clean. note says why when it is not clean.
    public WaterQuality WellQuality(WoodPileState well, out string note)
    {
        note = "";
        if (well.contaminatedHours > 0f)
        {
            note = "flood water got in";
            return WaterQuality.Unsafe;
        }
        if (Setbacks.Fouls(well.position, Sites(well, false), Rules, out string why))
        {
            note = why;
            return wellFouledQuality;
        }
        return wellCleanQuality;
    }

    // --- Well Water Table B3: each well has its own numbers, from the depth to water saved where its site was placed ---

    // The tier table with this component's tunables as the Medium and Legacy rows (today's numbers, tuned here); Shallow and Deep come
    // from WellWaterRules.Defaults.
    public WellWaterRules WellRules()
    {
        WellWaterRules rules = WellWaterRules.Defaults;
        var medium = new WellTierNumbers
        {
            digBlows = wellDigBlows, liningStone = wellLiningStone, bufferLitres = wellBufferLitres,
            recoveryLitresPerHour = wellRecoveryLitresPerHour, droughtRecoveryLitresPerHour = wellDroughtRecoveryLitresPerHour,
        };
        rules.medium = medium;
        rules.legacy = medium;
        return rules;
    }

    // This well's tier numbers. A depth of 0 is an old well (Legacy: today's). A depth that cannot be dug (never saved, since placement
    // refuses it) falls back to Legacy too, so nothing ever divides by zero.
    public WellWaterResult NumbersOf(WoodPileState well)
    {
        WellWaterResult result = WellWater.ForDepth(well.depthToWater, WellRules());
        return result.Diggable ? result : WellWater.ForDepth(0f, WellRules());
    }

    // The same when the component is not there (Edit Mode tests): the defaults.
    public static WellWaterResult WellNumbers(WoodPileState well) =>
        Instance != null ? Instance.NumbersOf(well) : WellWater.ForDepth(well.depthToWater);

    public int DigBlowsOf(WoodPileState well) => Mathf.Max(1, NumbersOf(well).digBlows);

    public float RecoveryPerHour(WoodPileState well)
    {
        WellWaterResult numbers = NumbersOf(well);
        WeatherManager weather = WeatherManager.Instance;
        return weather != null && weather.IsDrought ? numbers.droughtRecoveryLitresPerHour : numbers.recoveryLitresPerHour;
    }

    // The batch has leached: the Ash in the hopper is Lye now (one for one).
    public static void TurnAshIntoLye(WoodPileState hopper)
    {
        int ash = hopper.Count(AshRules.ItemId);
        if (ash <= 0)
            return;
        hopper.Take(AshRules.ItemId, ash);
        hopper.Add(PotashRules.LyeId, ash);
    }

    // Testing only (TimeDebugControls): every running batch is done now. Returns how many hoppers finished.
    public int DebugFinishLeaches()
    {
        WoodManager wood = WoodManager.Instance;
        int done = 0;
        if (wood == null)
            return 0;
        foreach (WoodPileState hopper in wood.Piles)
        {
            if (hopper.kind != PileKind.LeachHopper || hopper.leachHours <= 0f)
                continue;
            hopper.leachHours = 0f;
            TurnAshIntoLye(hopper);
            wood.NotifyChanged(hopper);
            done++;
        }
        return done;
    }

    // One game hour (sleep and day skips cross hours one at a time): wells refill, flood water clears, and a flash flood fouls any
    // well within reach of the channel.
    void OnHour(int hour)
    {
        WoodManager wood = WoodManager.Instance;
        if (wood == null)
            return;

        // A Leach Hopper's batch drips through one game hour at a time, so sleep and day skips (which cross every hour) finish it too.
        foreach (WoodPileState hopper in wood.Piles)
        {
            if (hopper.kind != PileKind.LeachHopper || hopper.leachHours <= 0f)
                continue;
            float before = hopper.leachHours;
            hopper.leachHours = PotashRules.LeachAfterHour(before);
            if (PotashRules.FinishedThisHour(before, hopper.leachHours))
                TurnAshIntoLye(hopper);
            wood.NotifyChanged(hopper);
        }

        bool flood = StreamManager.State == StreamState.FlashFlood;
        foreach (WoodPileState pile in wood.Piles)
        {
            if (pile.kind != PileKind.Well)
                continue;
            float rate = RecoveryPerHour(pile);
            int buffer = NumbersOf(pile).bufferLitres;

            bool changed = false;
            if (pile.contaminatedHours > 0f)
            {
                pile.contaminatedHours = Mathf.Max(0f, pile.contaminatedHours - 1f);
                changed = true;
            }
            if (flood && CreekEdgeDistance(pile.position) <= wellFloodReachMetres)
            {
                pile.contaminatedHours = wellContaminatedHours;
                changed = true;
            }

            if (pile.litres < buffer)
            {
                recoveryCarry.TryGetValue(pile.id, out float carry);
                carry += rate;
                int whole = Mathf.FloorToInt(carry);
                recoveryCarry[pile.id] = carry - whole;
                if (whole > 0)
                {
                    pile.litres = Mathf.Min(buffer, pile.litres + whole);
                    changed = true;
                }
            }
            if (changed)
                wood.NotifyChanged(pile);
        }
    }

    // --- Digging ---


    // One Shovel blow on a Well Site's shaft, scaled by the player's work efficiency. Returns true on the blow that finishes it.
    public bool DigBlow(WoodPileState site, float efficiency)
    {
        if (site.digProgress >= 1f)
            return false;
        float step = efficiency / DigBlowsOf(site);
        site.digProgress = Mathf.Min(1f, site.digProgress + step);
        return site.digProgress >= 1f;
    }

    // One Shovel blow on a full Outhouse pit's replacement, scaled by work efficiency; progress is kept on the Outhouse (digProgress,
    // saved). Returns true on the blow that finishes it (the caller then moves the shack: PlacedStructure.MoveToNewPit).
    public bool PitDigBlow(WoodPileState outhouse, float efficiency)
    {
        outhouse.digProgress = AdvanceBuild(outhouse.digProgress, efficiency, outhousePitDigBlows);
        return outhouse.digProgress >= 1f;
    }

    // --- The Outhouse's pit and the sanitary homestead ---

    public static int Today => TimeManager.Instance != null ? TimeManager.Instance.TotalDays : 0;

    // 0 (fresh) to 1 (full): days since the pit was dug over the days it lasts.
    public float PitFill(WoodPileState outhouse) =>
        Mathf.Clamp01((Today - outhouse.builtDay) / (float)Mathf.Max(1, outhousePitFillDays));

    public bool PitFull(WoodPileState outhouse) => PitFill(outhouse) >= 1f;

    // The player slept here: the homestead's heart for the sanitary test. Not saved; after a load the nearest finished shelter stands in.
    public static void NoteSleepPlace(Vector3 position)
    {
        if (Instance == null)
            return;
        Instance.hasSleepPlace = true;
        Instance.lastSleepPlace = position;
    }

    bool HomeAnchor(out Vector3 anchor)
    {
        anchor = Vector3.zero;
        if (hasSleepPlace)
        {
            anchor = lastSleepPlace;
            return true;
        }
        PlayerController player = FindAnyObjectByType<PlayerController>();
        WoodManager wood = WoodManager.Instance;
        if (player == null || wood == null)
            return false;
        float best = float.MaxValue;
        bool found = false;
        foreach (WoodPileState pile in wood.Piles)
        {
            if (pile.kind != PileKind.Cabin && pile.kind != PileKind.Tent && pile.kind != PileKind.LeanTo && pile.kind != PileKind.TarpShelter)
                continue;
            float d = Setbacks.Flat(pile.position, player.transform.position);
            if (d < best)
            {
                best = d;
                anchor = pile.position;
                found = true;
            }
        }
        return found;
    }

    // A working outhouse (pit not full) within sanitaryRadiusMetres of where the player sleeps.
    public bool IsSanitary()
    {
        WoodManager wood = WoodManager.Instance;
        if (wood == null || !HomeAnchor(out Vector3 anchor))
            return false;
        foreach (WoodPileState pile in wood.Piles)
        {
            if (pile.kind == PileKind.Outhouse && !PitFull(pile) && Setbacks.Flat(anchor, pile.position) <= sanitaryRadiusMetres)
                return true;
        }
        return false;
    }

    // SurvivalManager.RollIllness multiplies its chance by this; SleepManager adds the bonus to the sleep recovery multiplier.
    public static float IllnessMultiplier =>
        Instance == null ? 1f : Instance.IsSanitary() ? Instance.sanitaryIllnessMultiplier : Instance.unsanitaryIllnessMultiplier;

    public static float SleepRecoveryBonus => Instance != null && Instance.IsSanitary() ? Instance.sanitarySleepRecoveryBonus : 0f;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    // Testing only (RainDebugControls): the nearest Well or Outhouse within reach of a point, or null.
    public WoodPileState DebugNearest(PileKind kind, Vector3 from, float reach)
    {
        WoodPileState best = null;
        float bestDistance = reach;
        if (WoodManager.Instance == null)
            return null;
        foreach (WoodPileState pile in WoodManager.Instance.Piles)
        {
            float d = Vector3.Distance(pile.position, from);
            if (pile.kind == kind && d <= bestDistance)
            {
                best = pile;
                bestDistance = d;
            }
        }
        return best;
    }

    // Numpad /: for the nearest finished Covered Kitchen within reach of a point, what stands in each of its four slots, the grill's fire, the
    // sink's litres, how much is on the shelving, and the 0.75 m check that the hearth lookup cannot pick a grill's fire.
    public string DebugKitchenAddonsReport(Vector3 from, float reach)
    {
        WoodManager wood = WoodManager.Instance;
        WoodPileState kitchen = null;
        float bestDistance = reach;
        if (wood != null)
        {
            foreach (WoodPileState pile in wood.Piles)
            {
                float d = Vector3.Distance(pile.position, from);
                if (pile.kind == PileKind.Kitchen && d <= bestDistance)
                {
                    kitchen = pile;
                    bestDistance = d;
                }
            }
        }
        if (kitchen == null)
            return $"no finished Covered Outdoor Kitchen within {reach:0} m";

        var lines = new List<string>();
        for (int i = 0; i < Setbacks.KitchenLayout.SlotCount; i++)
        {
            WoodPileState addon = wood.AddonInSlot(kitchen, i);
            string detail = "empty";
            if (addon != null)
            {
                detail = WoodManager.PileName(addon.kind);
                switch (addon.kind)
                {
                    case PileKind.Grill:
                        CampfireState fire = wood.GrillFireOf(addon);
                        detail += fire == null ? " (no fire)" : fire.lit ? $" (lit, {fire.fuelHours:0.#} h fuel)" : $" (out, {fire.fuelHours:0.#} h fuel)";
                        break;
                    case PileKind.SlopSink: detail += $" ({addon.Total}/{wood.CapacityOf(PileKind.SlopSink)} L)"; break;
                    case PileKind.Shelving: detail += $" ({addon.contents.Count} stacks, {addon.Total} items)"; break;
                }
            }
            lines.Add($"slot {i + 1} {Setbacks.KitchenLayout.SlotLabel(i)}: {detail}");
        }

        CampfireState hearth = wood.KitchenHearthOf(kitchen);
        string hearthCheck = hearth == null ? "hearth: NONE found"
            : wood.IsGrillFire(hearth) ? "hearth lookup picked a Kitchen-Grill fire (BAD)"
            : "hearth lookup ok";
        float closest = float.MaxValue;
        foreach (WoodPileState addon in wood.AddonsOf(kitchen))
        {
            if (addon.kind != PileKind.Grill)
                continue;
            CampfireState gf = wood.GrillFireOf(addon);
            CampfireState real = wood.KitchenHearthOf(kitchen);
            if (gf != null && real != null)
                closest = Mathf.Min(closest, Vector3.Distance(gf.position, real.position));
        }
        string gap = closest == float.MaxValue ? $"no {WoodManager.PileName(PileKind.Grill)}" : $"{WoodManager.PileName(PileKind.Grill)} fire {closest:0.00} m from the hearth fire ({(closest > Setbacks.KitchenLayout.SlotMinHearthGap ? "ok" : "TOO CLOSE")})";
        return $"Kitchen {kitchen.id}: " + string.Join("; ", lines) + $" | {hearthCheck}; {gap}";
    }

    // Numpad *: for the nearest Covered Kitchen (any stage) within reach of a point, what the roof does there: is the roof in place, does it
    // shade a high sun, does it keep rain off the head, and is the hearth lit. Probes the kitchen's own centre, so it reads the same wherever you stand.
    public string DebugKitchenReport(Vector3 from, float reach)
    {
        WoodManager wood = WoodManager.Instance;
        WoodPileState best = null;
        float bestDistance = reach;
        if (wood != null)
        {
            foreach (WoodPileState pile in wood.Piles)
            {
                float d = Vector3.Distance(pile.position, from);
                if (pile.IsKitchen && d <= bestDistance)
                {
                    best = pile;
                    bestDistance = d;
                }
            }
        }
        if (best == null)
            return $"Kitchen: none within {reach:0} m";

        Vector3 centre = best.position + Vector3.up * 0.1f;
        bool roof = Physics.Raycast(centre, Vector3.up, out RaycastHit hit, 8f, ~0, QueryTriggerInteraction.Ignore) && !(hit.collider is TerrainCollider);
        float shaded = OverheadCover.SunBlocked(best.position + Vector3.up * 1.1f, 70f, 180f, 0f, 0f, 0f, out bool sunRoofed);
        bool shade = sunRoofed || shaded >= 0.99f;
        bool rain = OverheadCover.At(best.position + Vector3.up, 0.8f) >= 0.99f;
        CampfireState hearth = wood.KitchenHearthOf(best);
        string hearthText = hearth == null ? "none" : hearth.lit ? $"lit ({hearth.fuelHours:0.#} h of fuel)" : $"out ({hearth.fuelHours:0.#} h of fuel)";
        string covered = hearth == null ? "" : $", Bow Drill covered {(OverheadCover.Roofed(hearth.position + Vector3.up * 0.3f) ? "yes" : "no")}";
        string where = Setbacks.KitchenLayout.UnderRoof(best.position, best.yaw, from) ? ", you are under the roof" : "";
        return $"Kitchen ({WoodManager.PileName(best.kind)}): roof {(roof ? $"present ({hit.distance + 0.1f:0.0} m up)" : "absent")}, shade {(shade ? "yes" : "no")} (high sun), rain-sheltered {(rain ? "yes" : "no")}, hearth {hearthText}{covered}{where}";
    }

    // mode: 0 empty it, 1 fill it, 2 flood-contaminate it.
    public void DebugSetWell(WoodPileState well, int mode)
    {
        if (mode == 2)
            well.contaminatedHours = wellContaminatedHours;
        else
            well.litres = mode == 1 ? NumbersOf(well).bufferLitres : 0;
        recoveryCarry.Remove(well.id);
        WoodManager.Instance.NotifyChanged(well);
    }

    public void DebugSetPit(WoodPileState outhouse, bool full)
    {
        outhouse.builtDay = full ? Today - outhousePitFillDays : Today;
        WoodManager.Instance.NotifyChanged(outhouse);
    }
#endif
}
