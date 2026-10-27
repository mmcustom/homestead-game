using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// One tap on a Maple: a Spile in the trunk and a Sap Bucket hung under it (Maple_Sugaring.md; Early_Income_Design.md section 3).
[Serializable]
public class TapState
{
    public int id;
    public int treeIndex;          // the tree's index in the terrain's tree list (the same index WoodManager saves felled trees by)
    public Vector3 position;       // where the tap sits on the trunk, saved so a terrain that changed is noticed on load
    public float yaw;              // which way the bucket faces, away from the trunk
    public float heightScale = 1f; // the tree's size scale, which sets how much sap it gives
    public float litres;           // sap in the bucket
    public float ageHours;         // hours since the bucket's first sap (0 while it is empty)
    public bool sour;              // the sap sat too long and turned

    public MapleRules.Bucket Bucket
    {
        get => new MapleRules.Bucket { litres = litres, ageHours = ageHours, sour = sour };
        set
        {
            litres = value.litres;
            ageHours = value.ageHours;
            sour = value.sour;
        }
    }
}

// The taps section of the world file. Additive: a save from before Maple B has none and loads with no taps.
[Serializable]
public struct TapSaveData
{
    public List<TapState> taps;
    public float hoursSinceFreeze; // hours since the temperature was last at or below the freeze line (MapleRules.NextHoursSinceFreeze)
    public int nextId;
}

// Maple Sugaring, Maple B: the taps in the world. Owns every tap's saved state, runs the hour (the freeze-thaw memory, the sap into each
// bucket, souring) on TimeManager.HourChanged, which sleep and day skips replay hour by hour, and handles putting a tap up (Spile equipped, a
// click on a Maple), checking it (E collects sap, R takes it down) and a tapped tree coming down (WoodManager.Fell calls OnTreeFelled).
// All the numbers and the rules for what a check takes are in MapleRules and TapRules (Homestead.Economy). Boiling sap into Syrup is Maple C.
//
// Self-installing, like GroundWoodManager: no scene or prefab holds it, and GameManager.ResetManagers clears it before every new game and load.
public class TapManager : MonoBehaviour, ISaveable
{
    public static TapManager Instance { get; private set; }

    readonly List<TapState> taps = new List<TapState>();
    readonly Dictionary<int, Tap> views = new Dictionary<int, Tap>();
    int nextId = 1;
    float hoursSinceFreeze = MapleRules.FreezeMemoryCap;
    bool flowingNow;
    int lastFlashDay = -1;
    bool subscribed;
    bool needsReconcile;

    PlayerController player;
    InputAction attack;
    float nextPlayerSearch, nextLook;
    int lookIndex = -1;
    bool lookTapped;

    public IReadOnlyList<TapState> Taps => taps;
    public int TapCount => taps.Count;
    public bool FlowingNow => flowingNow;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    // Testing only (TimeDebugControls, Home): every hour is a flow hour, whatever the season or the weather. Never saved.
    public static bool DebugForceFlow;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetDebug() => DebugForceFlow = false;

    // Testing only (TimeDebugControls, Shift+Home): fills every bucket to the top with fresh sap.
    public int DebugFillBuckets()
    {
        foreach (TapState tap in taps)
        {
            tap.Bucket = new MapleRules.Bucket { litres = MapleRules.BucketCapacityLitres, ageHours = 0f, sour = false };
            Changed(tap);
        }
        return taps.Count;
    }
#endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Create()
    {
        if (Instance == null)
            new GameObject("Tap Manager").AddComponent<TapManager>();
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
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        if (Instance == this && SaveManager.Instance != null)
            SaveManager.Instance.Register(this);
        Subscribe();
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

    void Subscribe()
    {
        if (subscribed || TimeManager.Instance == null)
            return;
        TimeManager.Instance.HourChanged += OnHour;
        subscribed = true;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == GameManager.WorldScene)
            SpawnViews();
    }

    // --- Time passing ---

    void OnHour(int hour)
    {
        GameManager game = GameManager.Instance;
        if (game != null && !game.InGame)
            return;
        WeatherManager weather = WeatherManager.Instance;
        TimeManager time = TimeManager.Instance;
        if (weather == null || time == null)
            return;

        float temperature = weather.TemperatureC;
        hoursSinceFreeze = MapleRules.NextHoursSinceFreeze(hoursSinceFreeze, temperature);
        bool flow = MapleRules.IsFlowHour((int)time.CurrentSeason, temperature, hoursSinceFreeze);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (DebugForceFlow)
            flow = true;
#endif
        flowingNow = flow;

        bool becameReady = false;
        foreach (TapState tap in taps)
        {
            MapleRules.Bucket before = tap.Bucket;
            MapleRules.Bucket after = TapRules.TapHour(before, flow, tap.heightScale);
            if (after.litres == before.litres && after.ageHours == before.ageHours && after.sour == before.sour)
                continue;
            tap.Bucket = after;
            if (TapRules.BecameReady(before.litres, after.litres))
                becameReady = true;
            Changed(tap);
        }

        // One short message when sap first comes ready, and at most once a game day.
        if (becameReady && time.TotalDays != lastFlashDay && game != null && game.State == GameState.Playing)
        {
            int ready = 0;
            foreach (TapState tap in taps)
                if (!tap.sour && tap.litres >= TapRules.ReadyLitres)
                    ready++;
            if (ready > 0)
            {
                ToolStatus.Flash(ready == 1 ? "Sap is ready at a tap." : $"Sap is ready at {ready} taps.");
                lastFlashDay = time.TotalDays;
            }
        }
    }

    // --- Looking at trees and putting a tap up ---

    static bool IsMapleTree(WoodManager wood, int index) =>
        wood.IsStanding(index) && MapleRules.IsMaple(index, wood.Prototype(index), wood.HeightScale(index));

    public TapState TapAt(int treeIndex)
    {
        foreach (TapState tap in taps)
            if (tap.treeIndex == treeIndex)
                return tap;
        return null;
    }

    void Update()
    {
        Subscribe();
        UpdatePlacer();
    }

    void UpdatePlacer()
    {
        GameManager game = GameManager.Instance;
        InventoryManager inventory = InventoryManager.Instance;
        WoodManager wood = WoodManager.Instance;
        if (game == null || game.State != GameState.Playing || inventory == null || wood == null || !wood.TreesReady)
            return;

        if (needsReconcile)
            Reconcile(wood);

        if (player == null)
        {
            if (Time.unscaledTime < nextPlayerSearch)
                return;
            nextPlayerSearch = Time.unscaledTime + 1f;
            player = FindAnyObjectByType<PlayerController>();
            attack ??= InputSystem.actions != null ? InputSystem.actions.FindAction("Player/Attack") : null;
            return;
        }

        string equipped = inventory.EquippedTool != null ? inventory.EquippedTool.Id : null;
        bool spile = equipped == MapleRules.SpileId;
        if ((!spile && equipped != null) || !player.CanUseTools)
        {
            lookIndex = -1;
            return;
        }

        // The look (a raycast and a tree search) a few times a second is plenty.
        if (Time.unscaledTime >= nextLook)
        {
            nextLook = Time.unscaledTime + 0.12f;
            lookIndex = -1;
            if (player.AimRaycast(TapRules.ReachMetres, out RaycastHit hit))
            {
                int tree = wood.FindTree(hit.point, 0.6f);
                if (tree >= 0 && IsMapleTree(wood, tree))
                    lookIndex = tree;
            }
            lookTapped = lookIndex >= 0 && TapAt(lookIndex) != null;
        }

        if (lookIndex >= 0 && lookTapped && !spile)
            return; // its own tap shows the prompt

        if (lookIndex >= 0)
        {
            string line = lookTapped ? "Sugar Maple, already tapped"
                : spile ? "Sugar Maple, tappable  ·  click to tap it"
                : inventory.Player.Has(MapleRules.SpileId) ? "Sugar Maple, tappable  ·  equip a Spile to tap it"
                : "Sugar Maple, tappable";
            ToolStatus.Report(line, -1f, spile);
        }
        else if (spile)
        {
            ToolStatus.Report("Spile  ·  click on a Sugar Maple to tap it");
        }

        if (spile && attack != null && attack.WasPressedThisFrame())
        {
            if (lookIndex >= 0)
                TryPlace(wood, inventory, lookIndex);
            else
                ToolStatus.Flash("Look at a Sugar Maple within reach to tap it.");
        }
    }

    void TryPlace(WoodManager wood, InventoryManager inventory, int index)
    {
        bool hasSpile = inventory.Player.Has(MapleRules.SpileId), hasBucket = inventory.Player.Has(MapleRules.SapBucketId);
        TapRules.PlaceResult result = TapRules.CanPlace(IsMapleTree(wood, index), TapAt(index) != null, taps.Count, hasSpile, hasBucket);
        if (result != TapRules.PlaceResult.Ok)
        {
            ToolStatus.Flash(TapRules.PlaceMessage(result));
            return;
        }

        // Both items go, or neither: a failed second removal gives the first back.
        if (inventory.RemoveFromPlayer(MapleRules.SpileId, 1) != 1)
            return;
        if (inventory.RemoveFromPlayer(MapleRules.SapBucketId, 1) != 1)
        {
            inventory.AddToPlayer(MapleRules.SpileId, 1);
            return;
        }

        Vector3 tree = wood.WorldPosition(index);
        Vector3 toPlayer = player.transform.position - tree;
        toPlayer.y = 0f;
        toPlayer = toPlayer.sqrMagnitude < 0.01f ? Vector3.forward : toPlayer.normalized;
        var tap = new TapState
        {
            id = nextId++,
            treeIndex = index,
            position = tree + toPlayer * (wood.TrunkRadius(index) + 0.02f) + Vector3.up * TapRules.TapHeightMetres,
            yaw = Mathf.Atan2(toPlayer.x, toPlayer.z) * Mathf.Rad2Deg,
            heightScale = wood.HeightScale(index),
        };
        taps.Add(tap);
        SpawnView(tap);
        lookTapped = true;
        ToolStatus.Flash($"Tapped the maple. {taps.Count} of {MapleRules.MaxTaps} taps out.");
    }

    // --- Checking ---

    // E: takes the sap the pack can carry; what does not fit stays in the bucket.
    public string Collect(TapState tap)
    {
        InventoryManager inventory = InventoryManager.Instance;
        ItemDefinition sap = ItemDatabase.Get(MapleRules.SapId);
        if (inventory == null || sap == null)
            return "";

        MapleRules.Bucket before = tap.Bucket;
        int room = inventory.Player.SpaceFor(sap);
        MapleRules.Collected taken = TapRules.CollectUpTo(before, room);
        if (taken.litres > 0)
        {
            int added = inventory.AddToPlayer(MapleRules.SapId, taken.litres);
            if (added != taken.litres) // the pack took less than it said it would: keep the difference in the bucket
                taken = TapRules.CollectUpTo(before, added);
        }

        string message = TapRules.CollectMessage(before, taken);
        tap.Bucket = taken.bucket;
        Changed(tap);
        return message;
    }

    // R: takes the tap down. The Spile and Bucket come back; sap left in the bucket is lost.
    public string TakeDown(TapState tap)
    {
        bool lostSap = !tap.sour && MapleRules.Whole(tap.litres) > 0;
        int lost = MapleRules.Whole(tap.litres);
        string message = Refund(tap, true, "tap");
        RemoveTap(tap);
        return lostSap ? $"{message} The {lost} L of sap in the bucket was lost." : message;
    }

    // WoodManager.Fell calls this for every tree that comes down (the Axe and a storm both): a tapped one gives back its Spile and Bucket.
    // Close by they go straight to the pack; far off (a storm) they are left on the ground by the stump.
    public void OnTreeFelled(int treeIndex)
    {
        TapState tap = TapAt(treeIndex);
        if (tap == null)
            return;

        PlayerController who = player != null ? player : FindAnyObjectByType<PlayerController>();
        bool near = who != null && Vector3.Distance(who.transform.position, tap.position) <= TapRules.PackRefundMetres;
        string message = Refund(tap, near, "tap");
        RemoveTap(tap);
        ToolStatus.Flash(near ? "The tapped maple came down. " + message
                              : "A tapped maple came down. Its Spile and Sap Bucket are on the ground by the stump.", 5f);
    }

    string Refund(TapState tap, bool toPack, string what)
    {
        Transform view = views.TryGetValue(tap.id, out Tap v) && v != null ? v.transform : null;
        if (toPack)
        {
            var refund = new StructureRefund(tap.position, view);
            refund.Return(MapleRules.SpileId, TapRules.SpilesBack);
            refund.Return(MapleRules.SapBucketId, TapRules.BucketsBack);
            return refund.Describe(what);
        }

        DroppedItems.Place(MapleRules.SpileId, TapRules.SpilesBack, tap.position, view);
        DroppedItems.Place(MapleRules.SapBucketId, TapRules.BucketsBack, tap.position, view);
        return "";
    }

    void RemoveTap(TapState tap)
    {
        taps.Remove(tap);
        if (views.TryGetValue(tap.id, out Tap view) && view != null)
            Destroy(view.gameObject);
        views.Remove(tap.id);
    }

    // After a load, once the trees are up: every tap must still sit on a standing Maple at the spot it was saved. If the terrain changed it
    // moves to the Maple now there, or, if there is none, its Spile and Bucket are left on the ground where it hung.
    void Reconcile(WoodManager wood)
    {
        needsReconcile = false;
        int lost = 0;
        foreach (TapState tap in new List<TapState>(taps))
        {
            bool ok = false;
            if (IsMapleTree(wood, tap.treeIndex) && TapOtherThan(tap, tap.treeIndex) == null)
            {
                Vector3 trunk = wood.WorldPosition(tap.treeIndex);
                ok = TapRules.PositionMatches(Vector2.Distance(new Vector2(trunk.x, trunk.z), new Vector2(tap.position.x, tap.position.z)),
                                              wood.TrunkRadius(tap.treeIndex));
            }
            if (!ok)
            {
                int other = wood.FindTree(tap.position, TapRules.SearchMetres);
                if (other >= 0 && IsMapleTree(wood, other) && TapAt(other) == null)
                {
                    tap.treeIndex = other;
                    tap.heightScale = wood.HeightScale(other);
                    ok = true;
                }
            }
            if (ok)
                continue;

            Refund(tap, false, "tap");
            RemoveTap(tap);
            lost++;
        }
        if (lost > 0)
            ToolStatus.Flash(lost == 1 ? "A tapped tree is gone: its Spile and Sap Bucket are on the ground."
                                       : $"{lost} tapped trees are gone: their Spiles and Sap Buckets are on the ground.", 6f);
    }

    TapState TapOtherThan(TapState self, int treeIndex)
    {
        foreach (TapState tap in taps)
            if (tap != self && tap.treeIndex == treeIndex)
                return tap;
        return null;
    }

    // --- World objects ---

    void Changed(TapState tap)
    {
        if (views.TryGetValue(tap.id, out Tap view) && view != null)
            view.Refresh();
    }

    void SpawnViews()
    {
        foreach (Tap view in views.Values)
            if (view != null)
                Destroy(view.gameObject);
        views.Clear();

        if (!SceneManager.GetSceneByName(GameManager.WorldScene).isLoaded)
            return;
        foreach (TapState tap in taps)
            SpawnView(tap);
    }

    void SpawnView(TapState tap)
    {
        var go = new GameObject($"Tap {tap.id}");
        go.transform.SetPositionAndRotation(tap.position, Quaternion.Euler(0f, tap.yaw, 0f));
        Tap view = go.AddComponent<Tap>();
        view.Bind(tap);
        views[tap.id] = view;
    }

    // No taps: a new game, and the start of every load (a save with no taps section keeps this).
    public void ResetTaps()
    {
        taps.Clear();
        nextId = 1;
        hoursSinceFreeze = MapleRules.FreezeMemoryCap;
        flowingNow = false;
        lastFlashDay = -1;
        needsReconcile = false;
        lookIndex = -1;
        SpawnViews();
    }

    public TapSaveData CaptureState() => new TapSaveData { taps = new List<TapState>(taps), hoursSinceFreeze = hoursSinceFreeze, nextId = nextId };

    public void RestoreState(TapSaveData data)
    {
        taps.Clear();
        if (data.taps != null)
            taps.AddRange(data.taps);
        nextId = Mathf.Max(1, data.nextId);
        foreach (TapState tap in taps)
            nextId = Mathf.Max(nextId, tap.id + 1);
        hoursSinceFreeze = Mathf.Clamp(data.hoursSinceFreeze, 0f, MapleRules.FreezeMemoryCap);
        needsReconcile = taps.Count > 0;
        SpawnViews();
    }

    // Save_Data_Model.md's World Block.
    string ISaveable.SaveFile => "world";
    string ISaveable.SaveKey => "taps";
    object ISaveable.CaptureState() => CaptureState();
    void ISaveable.RestoreState(string json) => RestoreState(JsonUtility.FromJson<TapSaveData>(json));
}
