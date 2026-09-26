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
[DefaultExecutionOrder(90)]
public class AxeTool : MonoBehaviour
{
    public const string AxeId = "axe";

    [SerializeField] PlayerController player;

    [Header("Swinging")]
    [SerializeField, Min(0.5f)] float reach = 2.3f;
    [SerializeField, Min(0.1f)] float swingSeconds = 0.9f;
    [SerializeField, Min(0f)] float staminaPerSwing = 4f;

    [Header("Blows to fell (at size 1)")]
    [SerializeField, Min(1f)] float broadHardwoodBlows = 10f;
    [SerializeField, Min(1f)] float tallHardwoodBlows = 13f;
    [SerializeField, Min(1f)] float shrubBlows = 3f;

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

    static bool AxeEquipped
    {
        get
        {
            InventoryManager inventory = InventoryManager.Instance;
            return inventory != null && inventory.EquippedTool != null && inventory.EquippedTool.Id == AxeId;
        }
    }

    public static bool AxeCarried => InventoryManager.Instance != null && InventoryManager.Instance.Player.Has(AxeId);

    void Update()
    {
        UpdateSplitting();
        if (!AxeEquipped || attack == null || !player.CanUseTools)
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
            ToolStatus.Report("Axe — hold click on a tree trunk to chop it down, or on a wood pile to split Firewood");
    }

    float BlowsToFell(WoodManager wood, int tree)
    {
        int prototype = wood.Prototype(tree);
        float size = wood.HeightScale(tree) * Mathf.Sqrt(wood.WidthScale(tree));
        float blowsAtSize1 = prototype == WoodManager.Shrub ? shrubBlows
                           : prototype == WoodManager.TallHardwood ? tallHardwoodBlows : broadHardwoodBlows;
        return Mathf.Max(1f, Mathf.Round(blowsAtSize1 * size));
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

        int needed = logs ? blowsPerLog : blowsPerBranches;
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
            state.Remove(WoodManager.LogsId, 1);
            state.Add(FireManager.FirewoodId, firewoodPerLog);
        }
        else
        {
            state.Remove(WoodManager.BranchesId, branchesPerFirewood);
            state.Add(FireManager.FirewoodId, 1);
        }
        ToolStatus.Flash(logs ? $"Split a Log into {firewoodPerLog} Firewood" : "Split Branches into Firewood");
        WoodManager.Instance.NotifyChanged(state);
    }

    static float Efficiency => SurvivalManager.Instance != null ? SurvivalManager.Instance.WorkEfficiency : 1f;

    // Swings if Attack is held and the axe is ready. Returns whether a blow landed.
    bool Swing(Vector3 point)
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
        if (AudioManager.Instance != null)
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

    float SplitSeconds(ItemDefinition item) => item.Id == WoodManager.LogsId ? splitLogSeconds : splitBranchesSeconds;

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
        if (inventory.RemoveFromPlayer(current.item.Id, used) == used)
        {
            inventory.AddToPlayer(FireManager.FirewoodId, log ? firewoodPerLog : 1);
            if (current.remaining != int.MaxValue)
                current.remaining--;
        }
    }
}
