using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

// Slice E1 (Mike, 2026-11-28): the power system's hour (PowerRules has every number and the words). Created at start-up like the Ledger Manager; saved
// with the world. Every game hour: the Solar Panel Racks' watt-hours by the sun, the sky and their shade; the loads of everything wired to the Power Shed;
// the Generator by its own switch; the batteries' charge. While the system is powered, wired buildings have their lights on from dusk.
// Wiring: R at the Power Shed starts a cable run; E on a building ends it there, using Buried Cable from the pack by the length; R at the shed stops.
public class PowerManager : MonoBehaviour, ISaveable
{
    public static PowerManager Instance { get; private set; }

    PowerState state = new PowerState();
    long lastHour = -1;
    public PowerState State => state;
    public event System.Action Changed;

    // The cable being laid (null when not wiring): the shed it starts from.
    public static WoodPileState WiringFrom { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void CreateIfMissing()
    {
        if (Instance == null)
            new GameObject("Power Manager").AddComponent<PowerManager>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
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
        if (SaveManager.Instance != null)
            SaveManager.Instance.Unregister(this);
        Instance = null;
    }

    // --- What is wired, and what it draws ---

    // What can be wired in this slice: buildings with lights. (E2 adds the Thermostat Controller and the well pump; E3 the Charger.)
    public static bool Wireable(PileKind kind) =>
        kind == PileKind.Cabin || WoodManager.IsBarn(kind) || WoodManager.IsWalkIn(kind) || kind == PileKind.Workshop || kind == PileKind.PotterShed ||
        kind == PileKind.Smithy || kind == PileKind.Kitchen;

    // The lights' watts (LED fixtures).
    public static int LightWatts(PileKind kind)
    {
        if (WoodManager.IsBarn(kind))
            return WoodManager.BarnSizeOf(kind) == BarnRules.Size.Small ? 120 : WoodManager.BarnSizeOf(kind) == BarnRules.Size.Medium ? 180 : 300;
        if (WoodManager.IsWalkIn(kind))
            return 80;
        return kind == PileKind.Cabin ? 60 : 50;
    }

    public static WoodPileState Shed
    {
        get
        {
            if (WoodManager.Instance != null)
                foreach (WoodPileState p in WoodManager.Instance.Piles)
                    if (p.kind == PileKind.PowerShed)
                        return p;
            return null;
        }
    }

    public static int Batteries => Shed != null ? Shed.Count(PowerRules.BatteryId) : 0;

    public bool IsWired(WoodPileState target) => target != null && state.wires.Exists(w => w.targetId == target.id);

    // Whether a wired thing has power now.
    public bool HasPower(WoodPileState target) => state.powered && IsWired(target);

    public float LoadWattsNow()
    {
        TimeManager time = TimeManager.Instance;
        if (time == null || WoodManager.Instance == null)
            return 0f;
        bool lights = PowerRules.LightsOn(time.HourOfDay, time.SunriseHour, time.SunsetHour);
        float watts = 0f;
        foreach (WireRun w in state.wires)
        {
            WoodPileState t = Pile(w.targetId);
            if (t != null && lights)
                watts += LightWatts(t.kind);
        }
        return watts;
    }

    public float SolarWhNow()
    {
        TimeManager time = TimeManager.Instance;
        WoodManager wood = WoodManager.Instance;
        if (time == null || wood == null)
            return 0f;
        WeatherManager weather = WeatherManager.Instance;
        float clear = DayNightCycle.SunlightFor(weather != null ? weather.Current : WeatherType.Clear);
        float height = ColdFrameRules.Sun(time.HourOfDay, time.SunriseHour, time.SunsetHour, ColdFrameRules.NoonSin((int)time.CurrentSeason), 1f);
        float wh = 0f;
        foreach (WoodPileState rack in wood.Piles)
            if (rack.kind == PileKind.SolarRack)
                wh += PowerRules.SolarWh(rack.Count(PowerRules.PanelId), height, clear, PowerRules.Shade(TreesNear(rack.position)));
        return wh;
    }

    static int TreesNear(Vector3 at)
    {
        WoodManager wood = WoodManager.Instance;
        return wood != null ? wood.CountTrees(at, PowerRules.ShadeReachMetres) : 0;
    }

    static WoodPileState Pile(int id)
    {
        if (WoodManager.Instance != null)
            foreach (WoodPileState p in WoodManager.Instance.Piles)
                if (p.id == id)
                    return p;
        return null;
    }

    static WoodPileState GeneratorPile
    {
        get
        {
            if (WoodManager.Instance != null)
                foreach (WoodPileState p in WoodManager.Instance.Piles)
                    if (p.kind == PileKind.Generator)
                        return p;
            return null;
        }
    }

    // --- The hour ---

    void Update()
    {
        if (Instance != this || GameManager.Instance == null || !GameManager.Instance.InGame || TimeManager.Instance == null || WoodManager.Instance == null)
            return;
        UpdateWiring();
        long hour = (long)System.Math.Floor(TimeManager.Instance.TotalHours);
        if (lastHour < 0 || hour < lastHour)
            lastHour = hour;
        int steps = 0;
        while (lastHour < hour && steps < 24 * 60)
        {
            lastHour++;
            steps++;
            RunHour();
        }
        if (steps > 0)
        {
            RefreshLights();
            Changed?.Invoke();
        }
    }

    void RunHour()
    {
        WoodPileState shed = Shed;
        // Wires to things that are gone are taken off the list.
        state.wires.RemoveAll(w => Pile(w.targetId) == null || Pile(w.shedId) == null);
        if (shed == null)
        {
            state.powered = false;
            state.generatorOn = false;
            return;
        }
        WoodPileState gen = GeneratorPile;
        bool fuel = gen != null && (state.fuelLeft > 0f || gen.Count(WateringRules.GasolineId) > 0);
        PowerHour h = PowerRules.Hour(state.chargeWh, shed.Count(PowerRules.BatteryId), true, SolarWhNow(), LoadWattsNow(), state.generatorOn, fuel);
        bool wasPowered = state.powered;
        state.chargeWh = h.chargeWh;
        state.powered = h.powered;
        state.generatorOn = h.generatorOn;
        state.lastSolarWh = h.solarWh;
        state.lastLoadWh = h.loadWh;
        state.lastGeneratorWh = h.generatorWh;
        if (h.generatorOn && gen != null)
            Burn(gen, h.gallons);
        if (wasPowered && !state.powered && state.wires.Count > 0 && h.loadWh > 0f)
            ToolStatus.Banner(h.problem ?? PowerRules.BatteriesLow, 6f);
    }

    void Burn(WoodPileState gen, float gallons)
    {
        while (gallons > 0f)
        {
            if (state.fuelLeft <= 0f)
            {
                if (gen.Count(WateringRules.GasolineId) <= 0)
                    break;
                gen.Take(WateringRules.GasolineId, 1);
                state.fuelLeft += PowerRules.GallonsPerCan;
            }
            float use = Mathf.Min(gallons, state.fuelLeft);
            state.fuelLeft -= use;
            gallons -= use;
        }
        WoodManager.Instance.NotifyChanged(gen);
    }

    // --- Lights ---

    readonly Dictionary<int, GameObject> lights = new Dictionary<int, GameObject>();

    void RefreshLights()
    {
        TimeManager time = TimeManager.Instance;
        bool on = time != null && state.powered && PowerRules.LightsOn(time.HourOfDay, time.SunriseHour, time.SunsetHour);
        var keep = new HashSet<int>();
        foreach (WireRun w in state.wires)
        {
            WoodPileState t = Pile(w.targetId);
            if (t == null || !on)
                continue;
            keep.Add(t.id);
            if (!lights.TryGetValue(t.id, out GameObject go) || go == null)
            {
                go = new GameObject($"Electric Light {t.id}");
                go.transform.position = t.position + Vector3.up * LightHeight(t.kind);
                Light l = go.AddComponent<Light>();
                l.type = LightType.Point;
                l.color = new Color(1f, 0.9f, 0.75f);
                l.range = WoodManager.IsBarn(t.kind) ? 14f : 8f;
                l.shadows = LightShadows.None;
                l.intensity = WoodManager.IsBarn(t.kind) ? 900f : 450f; // the same units as the held Torch (70) and Lantern (120): an LED fixture is much brighter
                go.AddComponent<HDAdditionalLightData>();
                lights[t.id] = go;
            }
        }
        var gone = new List<int>();
        foreach (var pair in lights)
            if (!keep.Contains(pair.Key))
                gone.Add(pair.Key);
        foreach (int id in gone)
        {
            if (lights[id] != null)
                Destroy(lights[id]);
            lights.Remove(id);
        }
    }

    static float LightHeight(PileKind kind) =>
        WoodManager.IsBarn(kind) ? BarnRules.EaveHeight(WoodManager.BarnSizeOf(kind)) - 0.3f : WoodManager.IsWalkIn(kind) ? 2f : 2.2f;

    // --- Wiring ---

    public void StartWiring(WoodPileState shed)
    {
        WiringFrom = shed;
        ToolStatus.Flash("Laying cable from the Power Shed - press E on a building to wire it; R at the shed to stop", 5f);
    }

    public static void StopWiring() => WiringFrom = null;

    void UpdateWiring()
    {
        if (WiringFrom == null)
            return;
        if (Pile(WiringFrom.id) == null || GameManager.Instance.State != GameState.Playing)
        {
            WiringFrom = null;
            return;
        }
        int carried = InventoryManager.Instance != null ? InventoryManager.Instance.Player.Count(PowerRules.CableId) : 0;
        ToolStatus.Report($"Laying cable from the Power Shed  ({carried} ft of Buried Cable carried)  -  E on a building to wire it, R at the shed to stop");
    }

    // E on something while laying cable: wires it if it can be. Returns whether E was used.
    public bool TryWire(WoodPileState target)
    {
        if (WiringFrom == null || target == null || target.id == WiringFrom.id)
            return false;
        if (!Wireable(target.kind))
        {
            ToolStatus.Flash($"Nothing to power at the {WoodManager.PileName(target.kind)} yet", 3f);
            return true;
        }
        if (IsWired(target))
        {
            ToolStatus.Flash($"The {WoodManager.PileName(target.kind)} is wired already", 3f);
            return true;
        }
        float metres = Vector3.Distance(WiringFrom.position, target.position);
        if (metres > PowerRules.MaxRunMetres)
        {
            ToolStatus.Flash($"Too far - {metres:0} m; a run can be up to {PowerRules.MaxRunMetres:0} m", 3f);
            return true;
        }
        int feet = PowerRules.CableFeet(metres);
        InventoryManager inventory = InventoryManager.Instance;
        int carried = inventory != null ? inventory.Player.Count(PowerRules.CableId) : 0;
        if (carried < feet)
        {
            ToolStatus.Flash($"That run needs {PowerRules.CableNeeded(feet)} - you carry {carried} ft", 3.5f);
            return true;
        }
        inventory.RemoveFromPlayer(PowerRules.CableId, feet);
        state.wires.Add(new WireRun { shedId = WiringFrom.id, targetId = target.id, metres = metres });
        DrawTrench(state.wires[state.wires.Count - 1]);
        ToolStatus.Flash($"Wired the {WoodManager.PileName(target.kind)} - {feet} ft of cable buried", 3.5f);
        if (JournalManager.Instance != null)
            JournalManager.Instance.NoteHappening(KnowHowRules.Happening.PowerShedPlaced);
        RefreshLights();
        Changed?.Invoke();
        return true;
    }

    // --- The trench lines ---

    readonly List<GameObject> trenches = new List<GameObject>();

    void DrawTrenches()
    {
        foreach (GameObject t in trenches)
            if (t != null)
                Destroy(t);
        trenches.Clear();
        foreach (WireRun w in state.wires)
            DrawTrench(w);
    }

    void DrawTrench(WireRun w)
    {
        WoodPileState a = Pile(w.shedId), b = Pile(w.targetId);
        if (a == null || b == null)
            return;
        var root = new GameObject($"Cable Trench {w.shedId}-{w.targetId}");
        trenches.Add(root);
        Terrain terrain = Terrain.activeTerrain;
        int n = Mathf.Max(1, Mathf.CeilToInt(w.metres / 2f));
        for (int i = 0; i < n; i++)
        {
            Vector3 p0 = Vector3.Lerp(a.position, b.position, i / (float)n), p1 = Vector3.Lerp(a.position, b.position, (i + 1) / (float)n);
            if (terrain != null)
            {
                p0.y = terrain.SampleHeight(p0) + terrain.transform.position.y;
                p1.y = terrain.SampleHeight(p1) + terrain.transform.position.y;
            }
            GameObject seg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(seg.GetComponent<Collider>());
            seg.transform.SetParent(root.transform, true);
            seg.transform.position = (p0 + p1) / 2f + Vector3.up * 0.01f;
            seg.transform.rotation = Quaternion.LookRotation(p1 - p0);
            seg.transform.localScale = new Vector3(0.25f, 0.03f, Vector3.Distance(p0, p1) + 0.05f);
            var block = new MaterialPropertyBlock();
            block.SetColor("_BaseColor", new Color(0.26f, 0.19f, 0.12f));
            seg.GetComponent<MeshRenderer>().SetPropertyBlock(block);
        }
    }

    // --- Saving ---

    public void ResetPower()
    {
        state = new PowerState();
        lastHour = -1;
        WiringFrom = null;
        foreach (var pair in lights)
            if (pair.Value != null)
                Destroy(pair.Value);
        lights.Clear();
        DrawTrenches();
        Changed?.Invoke();
    }

    public void RestoreState(PowerState data)
    {
        ResetPower();
        state = data ?? new PowerState();
        if (state.wires == null)
            state.wires = new List<WireRun>();
        pendingDraw = true;
    }

    bool pendingDraw;

    void LateUpdate()
    {
        // The trenches and lights wait for the world to be loaded.
        if (pendingDraw && WoodManager.Instance != null && GameManager.Instance != null && GameManager.Instance.State == GameState.Playing)
        {
            pendingDraw = false;
            DrawTrenches();
            RefreshLights();
        }
    }

    string ISaveable.SaveFile => "world";
    string ISaveable.SaveKey => "power";
    object ISaveable.CaptureState() => state;
    void ISaveable.RestoreState(string json) => RestoreState(JsonUtility.FromJson<PowerState>(json));
}
