using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class WoodStack
{
    public string itemId;
    public int count;
}

[Serializable]
public class FelledTree
{
    public int index;         // the tree's place in the terrain's original tree list
    public bool stump;        // hardwoods leave a stump; a cut shrub leaves nothing
}

// Felled: what a felled tree left, gone once emptied. The other two are player-built storage (Wood_Gathering_System.md's
// Primitive Storage) that stay put, empty or not.
public enum PileKind { Felled, WoodStorage, RockStorage }

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
        foreach (WoodStack stack in contents)
            if (stack.itemId == itemId)
                return stack.count;
        return 0;
    }

    public void Add(string itemId, int count)
    {
        if (count <= 0)
            return;
        foreach (WoodStack stack in contents)
        {
            if (stack.itemId == itemId)
            {
                stack.count += count;
                return;
            }
        }
        contents.Add(new WoodStack { itemId = itemId, count = count });
    }

    public void Remove(string itemId, int count)
    {
        for (int i = contents.Count - 1; i >= 0; i--)
        {
            if (contents[i].itemId != itemId)
                continue;
            contents[i].count -= count;
            if (contents[i].count <= 0)
                contents.RemoveAt(i);
        }
    }

    public bool IsEmpty => contents.Count == 0;
    public bool IsRock => kind == PileKind.RockStorage;
    public bool IsBuilt => kind != PileKind.Felled;

    // What this pile will take in.
    public bool Accepts(string itemId) => IsRock ? itemId == WoodManager.StoneId : WoodManager.IsWood(itemId);
}

[Serializable]
public struct WoodSaveData
{
    public List<FelledTree> felled;
    public List<WoodPileState> piles;
    public int nextPileId;
}

// Wood_Gathering_System.md (2026-09-26): trees felled with the Axe, and the wood they leave. The World's trees are
// terrain trees, so felling one takes it out of RuntimeTerrain's play-time copy of the terrain data — the terrain asset
// itself is never touched — and remembers it by its place in the original tree list, so it stays down across saves.
// Hardwoods leave a stump (a small object in the World); every felled tree leaves a wood pile at its base holding its
// Logs, Branches and Sticks, which the player carries off as they can (Logs are heavy) and can split into Firewood
// right there. Piles last until emptied.
//
// Primitive Storage (2026-09-26): the player can also build a Wood Pile (4 Sticks for a ground frame) or a Rock Pile
// (free) just in front of them from the Inventory, then store wood or Stone in it and take it back out — the same pile
// as a felled tree's, made deliberate. Built piles stay even when empty and have no capacity limit for now; they're
// the stopgap until Building's storage sheds exist.
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

    public static readonly string[] WoodIds = { LogsId, BranchesId, SticksId, FireManager.FirewoodId };
    public static bool IsWood(string itemId) => Array.IndexOf(WoodIds, itemId) >= 0;

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
    [SerializeField, Min(0.5f)] float buildDistance = 1.8f;
    [Tooltip("Piles and campfires can't be built closer together than this (metres).")]
    [SerializeField, Min(0f)] float minSpacing = 2f;
    [SerializeField, Range(0f, 60f)] float maxSlope = 30f;

    readonly List<FelledTree> felled = new List<FelledTree>();
    readonly HashSet<int> felledSet = new HashSet<int>();
    readonly List<WoodPileState> piles = new List<WoodPileState>();
    readonly Dictionary<int, WoodPile> pileViews = new Dictionary<int, WoodPile>();
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
        foreach (WoodPile view in pileViews.Values)
            if (view != null)
                Destroy(view.gameObject);
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
        Vector3 pilePosition = basePosition + away * (TrunkRadius(index) + 0.9f) + side * 0.5f;
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

    // --- Piles ---

    public void NotifyChanged(WoodPileState pile)
    {
        if (pile.IsEmpty && !pile.IsBuilt)
        {
            piles.Remove(pile);
            if (pileViews.TryGetValue(pile.id, out WoodPile view) && view != null)
                Destroy(view.gameObject);
            pileViews.Remove(pile.id);
        }
        else if (pileViews.TryGetValue(pile.id, out WoodPile view) && view != null)
        {
            view.Rebuild();
        }
        PileChanged?.Invoke(pile);
    }

    // --- Building storage piles ---

    public string PileName(PileKind kind) => kind == PileKind.RockStorage ? "Rock Pile" : "Wood Pile";
    public int CostOf(PileKind kind) => kind == PileKind.WoodStorage ? woodPileSticks : 0;

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

        int cost = CostOf(kind);
        if (cost > 0 && inventory.Player.Count(SticksId) < cost)
        {
            reason = $"Needs {cost} Sticks (carrying {inventory.Player.Count(SticksId)}).";
            return false;
        }

        Vector3 forward = Vector3.ProjectOnPlane(player.transform.forward, Vector3.up).normalized;
        Vector3 probe = player.transform.position + forward * buildDistance + Vector3.up * 3f;
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
        foreach (WoodPileState other in piles)
        {
            if (Vector3.Distance(other.position, hit.point) < minSpacing)
            {
                reason = "Too close to another pile.";
                return false;
            }
        }
        if (FireManager.Instance != null && FireManager.Instance.NearestLit(hit.point, minSpacing) != null)
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
        if (kind == PileKind.Felled || !CanBuildPile(kind, player, out Vector3 position, out _))
            return false;

        int cost = CostOf(kind);
        if (cost > 0)
            InventoryManager.Instance.RemoveFromPlayer(SticksId, cost);
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

    void SpawnPile(WoodPileState pile)
    {
        var go = new GameObject($"Wood Pile {pile.id}");
        go.transform.SetPositionAndRotation(pile.position, Quaternion.Euler(0f, pile.yaw, 0f));
        var view = go.AddComponent<WoodPile>();
        view.Bind(pile, BarkMaterial());
        pileViews[pile.id] = view;
    }

    void SpawnStump(int index)
    {
        float radius = TrunkRadius(index);
        var stump = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        stump.name = "Stump";
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
        nextPileId = 1;
        bark = null;
        if (WorldLoaded)
            Apply();
    }

    public WoodSaveData CaptureState() => new WoodSaveData
    {
        felled = new List<FelledTree>(felled),
        piles = new List<WoodPileState>(piles),
        nextPileId = nextPileId,
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
        if (saved.piles != null)
            piles.AddRange(saved.piles);
        nextPileId = Mathf.Max(1, saved.nextPileId);
        Apply();
    }

    string ISaveable.SaveFile => "world";
    string ISaveable.SaveKey => "wood";
    object ISaveable.CaptureState() => CaptureState();
    void ISaveable.RestoreState(string json) => RestoreState(JsonUtility.FromJson<WoodSaveData>(json));
}
