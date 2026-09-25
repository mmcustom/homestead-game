using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class CampfireState
{
    public int id;
    public Vector3 position;
    public float yaw;
    public float fuelHours;
    public bool lit;
}

[Serializable]
public struct FireSaveData
{
    public List<CampfireState> fires;
    public int nextId;
}

// Core_Survival_System.md's Fire System: campfires need fuel, an ignition source and maintenance. The player builds a
// campfire from Firewood, lights it with Flint and Steel, and it burns its fuel down over in-game time; add more
// Firewood or it goes out. This manager owns every campfire's state so fires persist across scenes and saves, and
// spawns a Campfire object for each one in the World. Cooking and Water Purification should use NearestLit.
//
// Numbers are Claude Code's first proposal (2026-09-25), pending Mike's playtest: 3 Firewood to build, 2 in-game hours
// of burning per Firewood (2.5 real minutes), up to 12 hours of fuel at once. Like Hydration and Hunger, fuel burns at
// the normal clock rate — the debug fast-forward and day skips don't burn it. Sleep should call PassHours.
public class FireManager : MonoBehaviour, ISaveable
{
    public const string FirewoodId = "firewood";
    public const string IgnitionId = "flint_and_steel";

    public static FireManager Instance { get; private set; }

    [SerializeField] Campfire campfirePrefab;

    [Header("Fuel")]
    [SerializeField, Min(1)] int firewoodToBuild = 3;
    [Tooltip("In-game hours of burning each piece of Firewood adds.")]
    [SerializeField, Min(0.1f)] float hoursPerFirewood = 2f;
    [Tooltip("A campfire can't hold more fuel than this, in in-game hours.")]
    [SerializeField, Min(1f)] float maxFuelHours = 12f;

    [Header("Placement")]
    [Tooltip("How far in front of the player a new campfire goes (metres).")]
    [SerializeField, Min(0.5f)] float buildDistance = 1.8f;
    [Tooltip("Campfires can't be built closer together than this (metres).")]
    [SerializeField, Min(0f)] float minSpacing = 3f;
    [Tooltip("Steepest ground a campfire can sit on (degrees).")]
    [SerializeField, Range(0f, 60f)] float maxSlope = 25f;

    readonly List<CampfireState> fires = new List<CampfireState>();
    readonly Dictionary<int, Campfire> views = new Dictionary<int, Campfire>();
    int nextId = 1;

    public event Action<CampfireState> FireChanged;

    public IReadOnlyList<CampfireState> Fires => fires;
    public int FirewoodToBuild => firewoodToBuild;
    public float HoursPerFirewood => hoursPerFirewood;
    public float MaxFuelHours => maxFuelHours;

    static bool Playing => GameManager.Instance == null || GameManager.Instance.State == GameState.Playing;
    static InventoryContainer Carried => InventoryManager.Instance != null ? InventoryManager.Instance.Player : null;

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
        if (scene.name == GameManager.WorldScene)
            SpawnViews();
    }

    void Update()
    {
        TimeManager time = TimeManager.Instance;
        if (time == null || !time.IsRunning || !Playing)
            return;

        PassHours(Time.deltaTime * time.GameHoursPerRealSecond);
    }

    // Burns every lit fire's fuel for an amount of in-game time, e.g. hours spent asleep.
    public void PassHours(float hours)
    {
        if (hours <= 0f)
            return;

        foreach (CampfireState fire in fires)
        {
            if (!fire.lit)
                continue;

            fire.fuelHours = Mathf.Max(0f, fire.fuelHours - hours);
            if (fire.fuelHours <= 0f)
            {
                fire.lit = false;
                Changed(fire);
            }
        }
    }

    // --- Building ---

    // Where a campfire would go if built now in front of the player, or why it can't be built there.
    public bool CanBuild(PlayerController player, out Vector3 position, out string reason)
    {
        position = Vector3.zero;
        InventoryContainer carried = Carried;
        if (player == null || carried == null)
        {
            reason = "Not available here.";
            return false;
        }

        if (carried.Count(FirewoodId) < firewoodToBuild)
        {
            reason = $"Needs {firewoodToBuild} Firewood (carrying {carried.Count(FirewoodId)}).";
            return false;
        }

        Vector3 forward = Vector3.ProjectOnPlane(player.transform.forward, Vector3.up).normalized;
        Vector3 probe = player.transform.position + forward * buildDistance + Vector3.up * 3f;
        if (!Physics.Raycast(probe, Vector3.down, out RaycastHit hit, 8f, ~0, QueryTriggerInteraction.Ignore) ||
            hit.collider.transform.IsChildOf(player.transform))
        {
            reason = "No level ground in front of you.";
            return false;
        }

        if (hit.collider.GetComponentInParent<WaterSource>() != null)
        {
            reason = "Can't build a fire in water.";
            return false;
        }

        if (Vector3.Angle(hit.normal, Vector3.up) > maxSlope)
        {
            reason = "The ground in front of you is too steep.";
            return false;
        }

        foreach (CampfireState fire in fires)
        {
            if (Vector3.Distance(fire.position, hit.point) < minSpacing)
            {
                reason = "Too close to another campfire.";
                return false;
            }
        }

        position = hit.point;
        reason = "";
        return true;
    }

    // Builds an unlit campfire in front of the player, using up the Firewood as its first fuel.
    public bool Build(PlayerController player)
    {
        if (!CanBuild(player, out Vector3 position, out _))
            return false;

        Carried.Remove(FirewoodId, firewoodToBuild);
        var fire = new CampfireState
        {
            id = nextId++,
            position = position,
            yaw = player.transform.eulerAngles.y,
            fuelHours = Mathf.Min(maxFuelHours, firewoodToBuild * hoursPerFirewood),
            lit = false,
        };
        fires.Add(fire);
        SpawnView(fire);
        return true;
    }

    // --- Tending ---

    public static bool HasIgnition => Carried != null && Carried.Has(IgnitionId);
    public static int FirewoodCarried => Carried != null ? Carried.Count(FirewoodId) : 0;

    public bool CanLight(CampfireState fire) => fire != null && !fire.lit && fire.fuelHours > 0f && HasIgnition;

    public bool CanAddFuel(CampfireState fire) =>
        fire != null && FirewoodCarried > 0 && fire.fuelHours + hoursPerFirewood <= maxFuelHours + 0.01f;

    public bool Light(CampfireState fire)
    {
        if (!CanLight(fire))
            return false;

        fire.lit = true;
        Changed(fire);
        return true;
    }

    // Puts a burning fire out on purpose. The fuel left stays in the ring, so it can be relit later with Flint and Steel
    // instead of rebuilt — nothing is lost but the time it spent burning.
    public bool Extinguish(CampfireState fire)
    {
        if (fire == null || !fire.lit)
            return false;

        fire.lit = false;
        Changed(fire);
        return true;
    }

    public bool AddFuel(CampfireState fire)
    {
        if (!CanAddFuel(fire))
            return false;

        Carried.Remove(FirewoodId, 1);
        fire.fuelHours = Mathf.Min(maxFuelHours, fire.fuelHours + hoursPerFirewood);
        Changed(fire);
        return true;
    }

    // For Cooking and Water Purification: the closest burning campfire within range, or null.
    public CampfireState NearestLit(Vector3 position, float range)
    {
        CampfireState best = null;
        float bestDistance = range;
        foreach (CampfireState fire in fires)
        {
            float d = Vector3.Distance(fire.position, position);
            if (fire.lit && d <= bestDistance)
            {
                best = fire;
                bestDistance = d;
            }
        }
        return best;
    }

    void Changed(CampfireState fire)
    {
        if (views.TryGetValue(fire.id, out Campfire view) && view != null)
            view.Refresh();
        FireChanged?.Invoke(fire);
    }

    // --- World objects ---

    void SpawnViews()
    {
        foreach (Campfire view in views.Values)
        {
            if (view != null)
                Destroy(view.gameObject);
        }
        views.Clear();

        if (!SceneManager.GetSceneByName(GameManager.WorldScene).isLoaded)
            return;

        foreach (CampfireState fire in fires)
            SpawnView(fire);
    }

    void SpawnView(CampfireState fire)
    {
        if (campfirePrefab == null)
        {
            Debug.LogError("[FireManager] No campfire prefab assigned.", this);
            return;
        }

        Campfire view = Instantiate(campfirePrefab, fire.position, Quaternion.Euler(0f, fire.yaw, 0f));
        view.name = $"Campfire {fire.id}";
        view.Bind(fire);
        views[fire.id] = view;
    }

    // No fires — used when starting a new game.
    public void ResetFires()
    {
        fires.Clear();
        nextId = 1;
        SpawnViews();
    }

    public FireSaveData CaptureState() => new FireSaveData { fires = new List<CampfireState>(fires), nextId = nextId };

    // Runs after the World has loaded (GameManager.ContinueGame), so the restored fires are spawned straight away.
    public void RestoreState(FireSaveData data)
    {
        fires.Clear();
        if (data.fires != null)
            fires.AddRange(data.fires);
        nextId = Mathf.Max(1, data.nextId);
        SpawnViews();
    }

    // Save_Data_Model.md's World Block.
    string ISaveable.SaveFile => "world";
    string ISaveable.SaveKey => "fires";
    object ISaveable.CaptureState() => CaptureState();
    void ISaveable.RestoreState(string json) => RestoreState(JsonUtility.FromJson<FireSaveData>(json));
}
