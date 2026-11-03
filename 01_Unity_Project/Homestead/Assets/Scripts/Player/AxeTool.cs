using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// The Axe (Wood_Gathering_System.md, 2026-09-26), used from the equipped tool slot. Hold Attack while looking at a
// tree's trunk up close to swing: one blow every so often, each costing a little stamina and landing a chop. Enough
// blows and it comes down (WoodManager), leaving a wood pile — a broad hardwood takes about 10, a tall one about 13,
// bigger trees more, a shrub 3. Hunger and thirst slow the work (SurvivalManager.WorkEfficiency). Looking away from
// the tree keeps its blows for a moment, so a glance aside doesn't lose them.
//
// Firewood: swinging at a wood pile splits its Logs, then its Branches, into Firewood where they lie. Carried Logs and
// Branches split from the Inventory screen instead (a Split button while the Axe is carried) — a few seconds each,
// with the chopping sound, carrying on while the screen is open. One Log makes 5 Firewood (10 hours of fire), two
// Branches make 1. All numbers are Claude Code's first proposal, pending Mike's playtest.
//
// The other swung tools work the same way (2026-09-26): the Stone Pick Axe on South Ridge's rock outcrop, a Stone every
// 4 blows (Stone_Gathering_System.md, sfx_pickaxe), and the Primitive Shovel on a stump, dug out in 6 blows
// (Wood_Gathering_System.md, sfx_digging).
//
// The Primitive Axe (2026-09-26) is a stone head on a stick handle, a tier below the Axe: it does everything the Axe
// does, taking half as many blows again (a broad hardwood about 15 instead of 10) and splitting carried wood half as
// slowly again. Either one carried lets the Inventory split wood; the metal Axe is used when both are.
//
// The Wood Saw (Mike, 2026-10-31) is bought at the Trading Post, a step above the Axe: the same swing, 0.65 times the blows (a broad
// hardwood about 7 instead of 11) and 0.65 times the splitting time. Equipped it fells and splits at a pile; carried, the fastest of
// the Saw, Axe and Primitive Axe sets the Inventory's splitting speed (AxeRules). Later it is also the tool boards will need.
//
// Clay and Sand (Building_Housing_System.md's Small Cabin, 2026-09-26; Pottery P1, 2026-11-02): the Shovel also digs the creek and
// pond banks, with no placed deposit object. Clay comes from the wet bank (dry ground with water within PotteryRules.WetBankMetres of
// the aim point) and from the bed in shallow water (PotteryRules.WadeDigDepthMetres); Sand from the dry bank beyond that, out to
// bankReach. A ClayBank patch is optional and always gives Clay, but none are placed in the scene. Every water question goes through
// WaterAt (Animal.IsOverWater), and PotteryRules.DigFor decides from plain numbers. Banks run the whole length of the property's
// water, so unlike the one rock outcrop this never runs out.
[DefaultExecutionOrder(90)]
public class AxeTool : MonoBehaviour
{
    public const string AxeId = AxeRules.AxeId;
    public const string PrimitiveAxeId = AxeRules.PrimitiveAxeId;
    public const string SawId = AxeRules.SawId;
    public const string PickAxeId = "stone_pick_axe";
    public const string ShovelId = "shovel";

    [SerializeField] PlayerController player;

    [Header("Swinging")]
    [SerializeField, Min(0.5f)] float reach = 2.3f;
    [SerializeField, Min(0.1f)] float swingSeconds = 0.9f;
    [SerializeField, Min(0f)] float staminaPerSwing = 4f;

    [Header("Blows to fell (at size 1)")]
    [SerializeField, Min(1f)] float broadHardwoodBlows = 10f;
    [SerializeField, Min(1f)] float tallHardwoodBlows = 13f;
    [SerializeField, Min(1f)] float shrubBlows = 3f;

    [Tooltip("How many times the blows (and splitting time) the Primitive Axe needs, compared with the Axe.")]
    [SerializeField, Min(1f)] float primitiveAxeFactor = AxeRules.DefaultPrimitiveFactor;

    [Tooltip("How many times the blows (and splitting time) the Wood Saw needs, compared with the Axe: under 1, so it is faster.")]
    [SerializeField, Range(0.1f, 1f)] float sawFactor = AxeRules.DefaultSawFactor;

    [Header("Pick Axe and Shovel")]
    [SerializeField, Min(1)] int blowsPerStone = 4;
    [SerializeField, Min(1)] int blowsPerStump = 6;
    [SerializeField, Min(1)] int blowsPerClay = 4;
    [Tooltip("How close to the water's edge counts as bank, for digging Sand (metres); the wet bank, where Clay is, is PotteryRules.WetBankMetres.")]
    [SerializeField, Min(0.5f)] float bankReach = PotteryRules.BankReachMetres;

    [Header("Firewood")]
    [SerializeField, Min(1)] int firewoodPerLog = 5;
    [Tooltip("Branches it takes to make one Firewood.")]
    [SerializeField, Min(1)] int branchesPerFirewood = 2;
    [Tooltip("Blows at a wood pile to split one Log / one lot of Branches.")]
    [SerializeField, Min(1)] int blowsPerLog = 4, blowsPerBranches = 2;
    [Tooltip("Seconds to split one carried Log / one lot of carried Branches from the Inventory.")]
    [SerializeField, Min(0.1f)] float splitLogSeconds = 4f, splitBranchesSeconds = 2f;

    InputAction attack;
    float nextSwing;

    // The tree or pile being worked, and how far along.
    int treeIndex = -1;
    WoodPile pileTarget;
    Object otherTarget; // the outcrop or stump the Pick Axe or Shovel is working
    bool diggingBank; // the Shovel digging Clay out of a creek/pond bank, which has no target object of its own
    PotteryRules.Dig diggingKind; // what the Shovel is aimed at on the bank: patch, wet bank, creek bed or dry bank (Pottery P1)

    // The last water measurement (WaterAt, IsWetBank, IsCreekBank), kept while the aim point stays within MeasureMoveMetres of it so
    // a steady aim costs nothing; it is refreshed after MeasureSeconds anyway, so a waterline that moves under a standing player is followed.
    const float MeasureMoveMetres = 0.25f, MeasureSeconds = 1f;
    Vector3 measuredAt;
    float measuredTime = float.NegativeInfinity;
    WaterReading measuredWater;
    float measuredWaterline;

    // What the water is at a point: over it or not, the surface height, and how deep it is above the ground there (0 on dry ground).
    public struct WaterReading
    {
        public bool over;
        public float surface, depth;
    }
    float blows;
    float lastBlowTime;

    // Carried wood queued to split from the Inventory.
    class SplitJob
    {
        public ItemDefinition item;
        public int remaining;
    }
    readonly List<SplitJob> splits = new List<SplitJob>();
    float splitProgress, nextSplitChop;

    public static AxeTool Instance { get; private set; }

    void Awake()
    {
        if (player == null)
            player = GetComponent<PlayerController>();
        attack = InputSystem.actions != null ? InputSystem.actions.FindAction("Player/Attack") : null;
    }

    void OnEnable() => Instance = this;
    void OnDisable()
    {
        if (Instance == this)
            Instance = null;
    }

    static string Equipped
    {
        get
        {
            InventoryManager inventory = InventoryManager.Instance;
            return inventory != null && inventory.EquippedTool != null ? inventory.EquippedTool.Id : null;
        }
    }

    // Any of the three felling tools in the pack lets the Inventory split wood.
    public static bool AxeCarried => InventoryManager.Instance != null &&
                                     (InventoryManager.Instance.Player.Has(AxeId) || InventoryManager.Instance.Player.Has(PrimitiveAxeId) ||
                                      InventoryManager.Instance.Player.Has(SawId));

    // The fastest felling tool in the pack sets the Inventory's splitting speed: 0 when none is carried.
    float CarriedFactor
    {
        get
        {
            InventoryManager inventory = InventoryManager.Instance;
            return inventory == null ? 0f : AxeRules.FastestCarried(inventory.Player.Has(AxeId), inventory.Player.Has(PrimitiveAxeId),
                                                                    inventory.Player.Has(SawId), primitiveAxeFactor, sawFactor);
        }
    }

    // The equipped tool's blows multiplier: 1 for the Axe, more for the Primitive Axe, less for the Wood Saw.
    float AxeFactor => AxeRules.FactorFor(Equipped, primitiveAxeFactor, sawFactor);

    void Update()
    {
        UpdateSplitting();
        string equipped = Equipped;
        if (attack == null || !player.CanUseTools)
            return;
        if (equipped == PickAxeId || equipped == ShovelId)
        {
            UpdateDigging(equipped == PickAxeId);
            return;
        }
        if (equipped == WoodManager.HammerId)
        {
            UpdateHammering();
            return;
        }
        if (!AxeRules.IsFellingTool(equipped))
            return;

        // What the axe would hit: a wood pile, or the trunk of a standing tree.
        WoodPile pile = null;
        int tree = -1;
        Vector3 point = Vector3.zero;
        WoodManager wood = WoodManager.Instance;
        if (player.AimRaycast(reach, out RaycastHit hit))
        {
            point = hit.point;
            pile = hit.collider.GetComponentInParent<WoodPile>();
            if (pile != null && pile.State.IsRock)
                pile = null; // nothing to chop in a heap of stones
            if (pile == null && wood != null)
                tree = wood.FindTree(hit.point, 0.6f);
        }

        // Switching to something else starts its count again — unless it was only a glance away.
        bool same = (pile != null && pile == pileTarget) || (tree >= 0 && tree == treeIndex);
        if (!same && (pile != null || tree >= 0))
        {
            pileTarget = pile;
            treeIndex = tree;
            blows = 0f;
        }
        else if (!same && Time.time - lastBlowTime > 4f)
        {
            pileTarget = null;
            treeIndex = -1;
            blows = 0f;
        }

        if (pile != null)
            WorkPile(pile, point);
        else if (tree >= 0)
            WorkTree(wood, tree, point);
        else
            ToolStatus.Report($"{InventoryManager.Instance.EquippedTool.DisplayName} — hold click on a tree trunk to chop it down, or on a wood pile to split Firewood");
    }

    // The Pick Axe on the rock outcrop, or the Shovel on a stump or a creek/pond bank.
    void UpdateDigging(bool pick)
    {
        Object target = null;
        bool bank = false;
        PotteryRules.Dig dig = PotteryRules.Dig.None;
        Vector3 point = Vector3.zero;
        if (player.AimRaycast(reach, out RaycastHit hit))
        {
            point = hit.point;
            target = pick ? (Object)hit.collider.GetComponentInParent<RockDeposit>() : hit.collider.GetComponentInParent<Stump>();
            // A Well Site whose shaft is not dug yet (WellsAndOuthouses): the Shovel digs it, blow by blow.
            if (!pick && target == null)
            {
                PlacedStructure site = hit.collider.GetComponentInParent<PlacedStructure>();
                if (site != null && site.State != null && site.State.kind == PileKind.WellSite && site.State.digProgress < 1f)
                    target = site;
                // A full Outhouse pit: the Shovel digs the new one the shack moves onto (WellsAndOuthouses.PitDigBlow).
                else if (site != null && site.State != null && site.State.kind == PileKind.Outhouse && WellsAndOuthouses.Instance != null &&
                         WellsAndOuthouses.Instance.PitFull(site.State))
                    target = site;
            }
            // A felled tree's wood pile sits right beside its stump and can be in the way: take a stump just behind it.
            if (!pick && target == null)
                foreach (Collider nearby in Physics.OverlapSphere(hit.point, 0.8f, ~0, QueryTriggerInteraction.Ignore))
                    if (nearby.TryGetComponent(out Stump stump))
                    {
                        target = stump;
                        break;
                    }
            // The bank and the shallows (PotteryRules.DigFor): Clay at a patch, at the wet bank and on the bed in shallow water; Sand up the bank.
            if (!pick && target == null)
            {
                dig = DigAt(hit.point, hit.collider.GetComponentInParent<ClayBank>() != null);
                bank = PotteryRules.CanDig(dig);
            }
        }

        bool changed = target != otherTarget || bank != diggingBank || dig != diggingKind;
        if (changed && (target != null || bank))
        {
            otherTarget = target;
            diggingBank = bank;
            diggingKind = dig;
            blows = 0f;
        }
        else if (changed && Time.time - lastBlowTime > 4f)
        {
            otherTarget = null;
            diggingBank = false;
            diggingKind = PotteryRules.Dig.None;
            blows = 0f;
        }

        if (target == null && !bank)
        {
            ToolStatus.Report(pick ? "Stone Pick Axe — hold click on the rock outcrop on South Ridge to mine Stone"
                                   : dig == PotteryRules.Dig.TooDeep ? PotteryRules.DigText(dig) : PotteryRules.ShovelIdleText);
            return;
        }

        if (pick)
            MineStone(point);
        else if (target is Stump stump)
            DigStump(stump, point);
        else if (target is PlacedStructure site && site.State.kind == PileKind.Outhouse)
            DigPit(site, point);
        else if (target is PlacedStructure well)
            DigWell(well, point);
        else
            DigBank(point, dig);
    }

    // The Hammer on a fully stocked staged site (Well, Outhouse, Water Barrel, Rain Barrel or Food Cache Site, or the Small Cabin site):
    // hold click and each blow advances the site a little (WellsAndOuthouses.BuildBlow), the way the Shovel digs the
    // Well shaft. At the end the site becomes the finished structure in place. Progress is saved on the site, so walking away and
    // coming back (or saving) keeps it. It uses the same swing timing, stamina and Efficiency as the other tools, with the chop sound.
    void UpdateHammering()
    {
        WellsAndOuthouses rules = WellsAndOuthouses.Instance;
        WoodManager wood = WoodManager.Instance;
        if (rules == null || wood == null)
            return;

        WoodPileState state = null;
        Vector3 point = Vector3.zero;
        if (player.AimRaycast(reach, out RaycastHit hit))
        {
            point = hit.point;
            PlacedStructure placed = hit.collider.GetComponentInParent<PlacedStructure>();
            WoodPile pile = placed == null ? hit.collider.GetComponentInParent<WoodPile>() : null;
            if (placed != null && placed.State != null && WellsAndOuthouses.IsStagedSite(placed.State.kind))
                state = placed.State;
            else if (pile != null && pile.State != null && pile.State.kind == PileKind.CabinSite)
                state = pile.State;
        }
        if (state == null)
        {
            ToolStatus.Report("Hammer - stock a building site with its materials, then hold click on it to build it");
            return;
        }

        bool cabin = state.kind == PileKind.CabinSite;
        string name = WoodManager.PileName(state.kind);
        string needs = cabin ? wood.SiteNeeds(state) : WellsAndOuthouses.SiteNeeds(state);
        if (needs != null)
        {
            ToolStatus.Report($"{name} - still needs {needs}");
            return;
        }

        ToolStatus.Report(state.buildProgress > 0f ? $"Building the {name}" : $"{name} - hold click with the Hammer to build it", state.buildProgress > 0f ? state.buildProgress : -1f);
        if (!Swing(point))
            return;

        bool finished = rules.BuildBlow(state, Efficiency);
        if (!finished)
            return; // the prompts read the saved progress directly

        if (cabin)
        {
            if (wood.CompleteCabin(player))
                ToolStatus.Flash("Small Cabin built — you can sleep in it, and its hearth is ready for Firewood.");
            else
                state.buildProgress = 0.99f; // CompleteCabin refused (the site is out of its own reach): try again next blow
            return;
        }
        string message = WellsAndOuthouses.FinishedMessage(state.kind);
        if (rules.FinishSite(state))
            ToolStatus.Flash(message);
    }

    // The Shovel on a full Outhouse pit: hold click, a blow every interval (real time only, the usual swing stamina) until the new pit is
    // dug, then the shack moves onto it. Progress is saved on the Outhouse, so walking away keeps it. Only where a new pit is allowed.
    void DigPit(PlacedStructure outhouse, Vector3 point)
    {
        WoodPileState state = outhouse.State;
        WellsAndOuthouses rules = WellsAndOuthouses.Instance;
        if (rules == null || state == null)
            return;
        if (!outhouse.FindNewPitSpot(out Vector3 chosen))
        {
            ToolStatus.Report("No room to dig a new pit here - dismantle it and build the outhouse somewhere else");
            return;
        }

        ToolStatus.Report(state.digProgress > 0f ? "Digging a new pit" : "Outhouse - hold click to dig a new pit", state.digProgress > 0f ? state.digProgress : -1f);
        if (!Swing(point, AudioManager.Instance != null ? (System.Action<Vector3>)AudioManager.Instance.PlayDig : null))
            return;

        if (rules.PitDigBlow(state, Efficiency))
        {
            otherTarget = null;
            outhouse.MoveToNewPit(chosen);
            ToolStatus.Flash("Dug a new pit and moved the outhouse over it - good for another season", 4f);
        }
    }

    // The Shovel on a Well Site's shaft: each blow advances the dig (WellsAndOuthouses.DigBlow). It takes real time only, no clock jump,
    // and a half-dug shaft stays half dug (saved on the site).
    void DigWell(PlacedStructure site, Vector3 point)
    {
        WoodPileState state = site.State;
        WellsAndOuthouses rules = WellsAndOuthouses.Instance;
        if (rules == null || state == null)
            return;

        ToolStatus.Report(state.digProgress > 0f ? "Digging the well shaft" : "Well site - hold click to dig the shaft", state.digProgress > 0f ? state.digProgress : -1f);
        if (!Swing(point, AudioManager.Instance != null ? (System.Action<Vector3>)AudioManager.Instance.PlayDig : null))
            return;

        bool finished = rules.DigBlow(state, Efficiency);
        otherTarget = finished ? null : otherTarget;
        if (finished)
        {
            ToolStatus.Banner("The shaft is dug - line it with Stone (R stores Stone you carry).", 6f);
            WoodManager.Instance.NotifyChanged(state);
        }
        else if (Mathf.FloorToInt(state.digProgress * 10f) != Mathf.FloorToInt((state.digProgress - Efficiency / rules.DigBlowsOf(state)) * 10f))
        {
            WoodManager.Instance.NotifyChanged(state); // the visible shaft deepens a tenth at a time
        }
    }

    // THE one place the dig rule asks about water: is the point over it, the surface height there, and how deep it is above the ground
    // (Animal.IsOverWater reads the Water layer, so a water surface that rises and falls, or a replaced water plane on that layer, is
    // followed with no other change; IsOverWater itself says dry when the water is within 5 cm of the ground).
    static WaterReading WaterAt(Vector3 point)
    {
        if (!Animal.IsOverWater(point, out float surface))
            return default;
        Terrain terrain = Terrain.activeTerrain;
        float ground = terrain != null ? terrain.SampleHeight(point) + terrain.transform.position.y : surface;
        return new WaterReading { over = true, surface = surface, depth = Mathf.Max(0f, surface - ground) };
    }

    // What the Shovel finds at the aim point. Measured again only when the aim moves MeasureMoveMetres or MeasureSeconds pass.
    PotteryRules.Dig DigAt(Vector3 point, bool onClayPatch)
    {
        if (onClayPatch)
            return PotteryRules.DigFor(true, false, 0f, float.PositiveInfinity);

        Vector3 moved = point - measuredAt;
        moved.y = 0f;
        if (Time.time - measuredTime >= MeasureSeconds || moved.sqrMagnitude > MeasureMoveMetres * MeasureMoveMetres)
        {
            measuredAt = point;
            measuredTime = Time.time;
            measuredWater = WaterAt(point);
            measuredWaterline = measuredWater.over ? 0f
                : IsWetBank(point) ? PotteryRules.WetBankMetres
                : IsCreekBank(point) ? PotteryRules.BankReachMetres
                : float.PositiveInfinity;
        }
        return PotteryRules.DigFor(false, measuredWater.over, measuredWater.depth, measuredWaterline);
    }

    // Whether any of 8 probes on a ring of this radius around a dry point is over water.
    bool WaterOnRing(Vector3 point, float radius)
    {
        const int rays = 8;
        for (int i = 0; i < rays; i++)
        {
            float angle = i * (360f / rays) * Mathf.Deg2Rad;
            if (WaterAt(point + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius).over)
                return true;
        }
        return false;
    }

    // Dry ground with water within PotteryRules.WetBankMetres: two rings, at 1 m and 2 m, so a narrow creek tip is not missed.
    bool IsWetBank(Vector3 point) => WaterOnRing(point, PotteryRules.InnerProbeMetres) || WaterOnRing(point, PotteryRules.WetBankMetres);

    // Dry ground close enough to the water's edge in some direction to count as a bank: a ring at bankReach. (Both ring tests are for a
    // point already known to be dry.)
    bool IsCreekBank(Vector3 point) => WaterOnRing(point, bankReach);

    // The Shovel on the bank or the bed in shallow water: Clay at a patch, the wet bank or the creek bed, Sand up the dry bank, with the same four blows.
    void DigBank(Vector3 point, PotteryRules.Dig dig)
    {
        bool sand = PotteryRules.GivesSand(dig);
        string what = PotteryRules.DigName(dig);
        string itemId = sand ? PotteryRules.SandId : WoodManager.ClayId;
        InventoryManager inventory = InventoryManager.Instance;
        ItemDefinition item = ItemDatabase.Get(itemId);
        if (inventory == null || item == null || inventory.Player.SpaceFor(item) <= 0)
        {
            ToolStatus.Report($"{PotteryRules.PlaceName(dig)} - no room to carry more {what}");
            return;
        }

        ToolStatus.Report(blows > 0f ? PotteryRules.DiggingText(dig) : PotteryRules.DigText(dig),
                          blows > 0f ? blows / blowsPerClay : -1f);
        if (!Swing(point, AudioManager.Instance != null ? (System.Action<Vector3>)AudioManager.Instance.PlayDig : null))
            return;
        blows += Efficiency;
        if (blows < blowsPerClay)
            return;

        blows = 0f;
        inventory.AddToPlayer(itemId, 1);
        ToolStatus.Flash(PotteryRules.DugText(dig));
    }

    void MineStone(Vector3 point)
    {
        StoneManager stone = StoneManager.Instance;
        InventoryManager inventory = InventoryManager.Instance;
        if (stone == null || stone.DepositRemaining <= 0)
        {
            ToolStatus.Report("The outcrop is worked out — no more loose rock to break off");
            return;
        }
        ItemDefinition item = ItemDatabase.Get(WoodManager.StoneId);
        if (inventory == null || item == null || inventory.Player.SpaceFor(item) <= 0)
        {
            ToolStatus.Report("Rock Outcrop — no room to carry more Stone");
            return;
        }

        ToolStatus.Report(blows > 0f ? "Breaking off Stone" : $"Rock Outcrop — hold click to mine Stone  ({stone.DepositRemaining} left)",
                          blows > 0f ? blows / blowsPerStone : -1f);
        if (!Swing(point, AudioManager.Instance != null ? (System.Action<Vector3>)AudioManager.Instance.PlayPickaxe : null))
            return;
        blows += Efficiency;
        if (blows < blowsPerStone)
            return;

        blows = 0f;
        if (stone.TakeFromDeposit())
        {
            inventory.AddToPlayer(WoodManager.StoneId, 1);
            ToolStatus.Flash($"Broke off a Stone  ({stone.DepositRemaining} left in the outcrop)");
        }
    }

    void DigStump(Stump stump, Vector3 point)
    {
        ToolStatus.Report(blows > 0f ? "Digging out the stump" : "Stump — hold click to dig it out", blows > 0f ? blows / blowsPerStump : -1f);
        if (!Swing(point, AudioManager.Instance != null ? (System.Action<Vector3>)AudioManager.Instance.PlayDig : null))
            return;
        blows += Efficiency;
        if (blows < blowsPerStump)
            return;

        blows = 0f;
        otherTarget = null;
        if (WoodManager.Instance != null)
            WoodManager.Instance.RemoveStump(stump.TreeIndex);
        ToolStatus.Flash("Dug the stump out — its roots make a Firewood");
    }

    float BlowsToFell(WoodManager wood, int tree)
    {
        int prototype = wood.Prototype(tree);
        float size = AxeRules.TreeSize(wood.HeightScale(tree), wood.WidthScale(tree));
        float blowsAtSize1 = prototype == WoodManager.Shrub ? shrubBlows
                           : prototype == WoodManager.TallHardwood ? tallHardwoodBlows : broadHardwoodBlows;
        return AxeRules.FellBlows(blowsAtSize1, size, AxeFactor);
    }

    void WorkTree(WoodManager wood, int tree, Vector3 point)
    {
        float needed = BlowsToFell(wood, tree);
        string name = WoodManager.TreeName(wood.Prototype(tree));
        ToolStatus.Report(blows > 0f ? $"Chopping {name} — keep swinging" : $"{name} — hold click to chop it down",
                          blows > 0f ? blows / needed : -1f);

        if (!Swing(point))
            return;
        blows += Efficiency;
        if (blows < needed)
            return;

        wood.Fell(tree, player.transform.position);
        ToolStatus.Flash(wood.Prototype(tree) == WoodManager.Shrub ? "Cut down the shrub" : "Timber! The tree comes down");
        treeIndex = -1;
        blows = 0f;
    }

    void WorkPile(WoodPile pile, Vector3 point)
    {
        WoodPileState state = pile.State;
        bool logs = state.Count(WoodManager.LogsId) > 0;
        bool branches = state.Count(WoodManager.BranchesId) >= branchesPerFirewood;
        if (!logs && !branches)
        {
            ToolStatus.Report("Nothing here to split — press E to take the wood");
            return;
        }

        int needed = AxeRules.SplitBlows(logs ? blowsPerLog : blowsPerBranches, AxeFactor);
        string what = logs ? "a Log" : $"{branchesPerFirewood} Branches";
        ToolStatus.Report(blows > 0f ? $"Splitting {what} into Firewood" : $"Wood pile — hold click to split {what} into Firewood",
                          blows > 0f ? blows / needed : -1f);

        if (!Swing(point))
            return;
        blows += Efficiency;
        if (blows < needed)
            return;

        blows = 0f;
        if (logs)
        {
            // The Firewood is as wet as the wood it came from (WoodMoisture): the day of the wettest batch used.
            state.Add(FireManager.FirewoodId, firewoodPerLog, WettestDay(state.Take(WoodManager.LogsId, 1)));
        }
        else
        {
            state.Add(FireManager.FirewoodId, 1, WettestDay(state.Take(WoodManager.BranchesId, branchesPerFirewood)));
        }
        ToolStatus.Flash(logs ? $"Split a Log into {firewoodPerLog} Firewood" : "Split Branches into Firewood");
        WoodManager.Instance.NotifyChanged(state);
    }

    // The most recently wetted stamp among the batches (0 when all were dry): a mix is as wet as its wettest part.
    static int WettestDay(List<WoodStack> batches)
    {
        int day = 0;
        foreach (WoodStack batch in batches)
            day = Mathf.Max(day, batch.day);
        return day;
    }

    static float Efficiency => SurvivalManager.Instance != null ? SurvivalManager.Instance.WorkEfficiency : 1f;

    // Swings if Attack is held and the tool is ready. Returns whether a blow landed. sound: the blow's sound (the chop
    // unless given).
    bool Swing(Vector3 point, System.Action<Vector3> sound = null)
    {
        if (!attack.IsPressed() || Time.time < nextSwing)
            return false;
        if (!player.TrySpendStamina(staminaPerSwing))
        {
            nextSwing = Time.time + 0.5f;
            ToolStatus.Flash("Too tired to swing — catch your breath", 1.5f);
            return false;
        }

        nextSwing = Time.time + swingSeconds;
        lastBlowTime = Time.time;
        if (sound != null)
            sound(point);
        else if (AudioManager.Instance != null)
            AudioManager.Instance.PlayChop(point);
        return true;
    }

    // --- Splitting carried wood (from the Inventory screen) ---

    public static bool IsSplittable(string itemId) => itemId == WoodManager.LogsId || itemId == WoodManager.BranchesId;

    // Queues one Log or one lot of Branches (int.MaxValue = all of them). Needs the Axe carried.
    public bool EnqueueSplit(ItemDefinition item, int count)
    {
        if (item == null || !IsSplittable(item.Id) || count <= 0)
            return false;
        if (!AxeCarried)
        {
            ToolStatus.Flash("Splitting wood needs the Axe");
            return false;
        }
        if (item.Id == WoodManager.BranchesId && Carried(item) < branchesPerFirewood)
        {
            ToolStatus.Flash($"Takes {branchesPerFirewood} Branches to make a Firewood");
            return false;
        }

        SplitJob job = FindSplit(item);
        if (job == null)
            splits.Add(job = new SplitJob { item = item });
        job.remaining = count == int.MaxValue || job.remaining == int.MaxValue ? int.MaxValue : job.remaining + count;
        return true;
    }

    public void CancelSplit(ItemDefinition item)
    {
        SplitJob job = FindSplit(item);
        if (job == null)
            return;
        if (splits.IndexOf(job) == 0)
            splitProgress = 0f;
        splits.Remove(job);
    }

    public bool IsSplitQueued(ItemDefinition item) => FindSplit(item) != null;

    public float SplitProgressOf(ItemDefinition item)
    {
        if (splits.Count == 0 || splits[0].item != item)
            return -1f;
        return Mathf.Clamp01(splitProgress / SplitSeconds(item));
    }

    // How many splits what is carried could make: one per Log, one per lot of Branches (the stepper's limit for a Split button).
    public int SplitCapacity(ItemDefinition item)
    {
        if (item == null || !IsSplittable(item.Id))
            return 0;
        return item.Id == WoodManager.BranchesId ? Carried(item) / branchesPerFirewood : Carried(item);
    }

    // Logs, or lots of Branches, still to split — capped at what's carried.
    public int SplitsLeft(ItemDefinition item)
    {
        SplitJob job = FindSplit(item);
        if (job == null)
            return 0;
        int possible = item.Id == WoodManager.BranchesId ? Carried(item) / branchesPerFirewood : Carried(item);
        return Mathf.Min(job.remaining, possible);
    }

    SplitJob FindSplit(ItemDefinition item)
    {
        foreach (SplitJob job in splits)
            if (job.item == item)
                return job;
        return null;
    }

    float SplitSeconds(ItemDefinition item) =>
        AxeRules.SplitSeconds(item.Id == WoodManager.LogsId ? splitLogSeconds : splitBranchesSeconds, CarriedFactor > 0f ? CarriedFactor : 1f);

    static int Carried(ItemDefinition item) =>
        InventoryManager.Instance != null ? InventoryManager.Instance.Player.Count(item.Id) : 0;

    void UpdateSplitting()
    {
        while (splits.Count > 0 && SplitsLeft(splits[0].item) <= 0)
        {
            splits.RemoveAt(0);
            splitProgress = 0f;
        }
        if (splits.Count == 0)
            return;

        GameState state = GameManager.Instance != null ? GameManager.Instance.State : GameState.Playing;
        if (state != GameState.Playing && state != GameState.Menu)
            return;
        if (!AxeCarried)
        {
            splits.Clear();
            splitProgress = 0f;
            return;
        }

        SplitJob current = splits[0];
        float seconds = SplitSeconds(current.item);
        splitProgress += Time.unscaledDeltaTime; // carries on behind the Inventory screen, like cooking
        if (state == GameState.Playing)
            ToolStatus.Report($"Splitting {current.item.DisplayName} into Firewood, {SplitsLeft(current.item)} to go",
                              Mathf.Clamp01(splitProgress / seconds), false);

        // A chop about every swing's worth of time while splitting.
        if (Time.unscaledTime >= nextSplitChop)
        {
            nextSplitChop = Time.unscaledTime + swingSeconds;
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayChop(player.transform.position + player.transform.forward * 0.8f);
        }

        if (splitProgress < seconds)
            return;
        splitProgress = 0f;

        InventoryManager inventory = InventoryManager.Instance;
        bool log = current.item.Id == WoodManager.LogsId;
        int used = log ? 1 : branchesPerFirewood;
        if (AudioManager.Instance != null)
            AudioManager.Instance.SilenceNextRemoval(); // the chop is the sound, not an item drop
        int wettest = 0;
        if (inventory.Player.RemoveBatches(current.item.Id, used, (day, count) => wettest = Mathf.Max(wettest, day)) == used)
        {
            inventory.AddToPlayer(FireManager.FirewoodId, log ? firewoodPerLog : 1, wettest); // as wet as the wood it came from
            if (current.remaining != int.MaxValue)
                current.remaining--;
        }
    }
}
