using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.HighDefinition;

[Serializable]
public struct PortableLightSaveData
{
    public float torchHoursLeft;
    public float lanternFuelHours;
}

// Core_Survival_System.md's Portable Lighting (2026-10-03): the Torch and the Lantern, the two Tools that put a light
// on the player away from a campfire. A Flashlight stays Future until batteries exist (Power_System.md).
//
// Both work from the equipped tool slot: equip one and click (Attack) to light or snuff it. Only the equipped one can
// be lit — putting it away or switching tools puts it out — and the light is a point light riding on the camera.
//   Torch  — Stick + Cordage. Lit with Flint and Steel (carried, not used up) like the campfire. It burns down and is
//            used up when it's gone, like Firewood — the cheap, disposable tier, not refuelable. A torch put out early
//            keeps what's left of it.
//   Lantern — a clay oil lamp (Clay + Cordage). It isn't used up: it has a fuel tank, topped up with Lamp Oil from the
//            Inventory screen (Refill), and burns that down instead. Brighter than a torch and about 2.5 times as long
//            per fill-up. Lights without Flint and Steel — a lit wick, not a fire to start.
// Lamp Oil is bought at the Trading Post (Economy_System.md); a rendered tallow from butchering is a possible free
// source later.
//
// Like fires, they burn at the normal clock rate — paused in menus, not sped by day skips. Numbers are Claude Code's
// first pass (2026-10-03), pending Mike's playtest. Created on startup like DroppedItems rather than placed in a scene.
public class PortableLight : MonoBehaviour, ISaveable
{
    public const string TorchId = "torch";
    public const string LanternId = "lantern";
    public const string LampOilId = "lamp_oil";

    // In-game hours.
    const float TorchBurnHours = 3f;
    const float LanternHoursPerOil = 8f;
    const float LanternCapacityHours = 16f;
    const float TorchLowHours = 0.4f; // a torch dies down over its last stretch

    // Light (HDRP candela; the Campfire's is 150) and how far it reaches (metres).
    const float TorchIntensity = 70f, TorchRange = 13f;
    const float LanternIntensity = 120f, LanternRange = 19f;
    const float FadeSpeed = 6f; // intensity per second as a proportion of full, so lighting and snuffing don't pop

    static readonly Color TorchColor = new Color(1f, 0.62f, 0.28f);
    static readonly Color LanternColor = new Color(1f, 0.78f, 0.45f);

    public static PortableLight Instance { get; private set; }

    float torchHoursLeft;   // of the torch in use (it's still carried until it's gone); 0 = a fresh one next
    float lanternFuelHours;
    string litId;           // what's burning right now, or null
    string lastLitId;

    InputAction attack;
    PlayerController player;
    Light heldLight;
    float shown;            // 0-1, the light's current brightness as a share of full
    bool registered;

    public bool IsLit(string itemId) => litId != null && litId == itemId;
    public float TorchHoursLeft => torchHoursLeft;
    public float LanternFuelHours => lanternFuelHours;
    public static float LanternCapacity => LanternCapacityHours;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => Instance = null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (Instance == null)
            new GameObject("Portable Light").AddComponent<PortableLight>();
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
        attack = InputSystem.actions != null ? InputSystem.actions.FindAction("Player/Attack") : null;
    }

    void OnDestroy()
    {
        if (Instance != this)
            return;
        if (SaveManager.Instance != null)
            SaveManager.Instance.Unregister(this);
        Instance = null;
    }

    static bool Playing => GameManager.Instance == null || GameManager.Instance.State == GameState.Playing;

    void Update()
    {
        if (!registered && SaveManager.Instance != null)
        {
            SaveManager.Instance.Register(this);
            registered = true;
        }

        InventoryManager inventory = InventoryManager.Instance;
        if (inventory == null)
            return;

        string equipped = inventory.EquippedTool != null ? inventory.EquippedTool.Id : null;
        if (litId != null && (equipped != litId || !inventory.Player.Has(litId)))
            litId = null;
        if (!inventory.Player.Has(TorchId))
            torchHoursLeft = 0f;

        bool holding = equipped == TorchId || equipped == LanternId;
        if (holding && Playing)
        {
            if (attack != null && attack.WasPressedThisFrame())
                Toggle(equipped, inventory);
            Burn(inventory);
            ReportStatus(equipped);
        }

        UpdateLight();
    }

    // --- Lighting, snuffing, burning ---

    void Toggle(string id, InventoryManager inventory)
    {
        if (litId == id)
        {
            litId = null;
            return;
        }

        if (id == TorchId)
        {
            if (!inventory.Player.Has(FireManager.IgnitionId))
            {
                ToolStatus.Flash("You need Flint and Steel to light the Torch.", 2.5f);
                return;
            }
            if (torchHoursLeft <= 0f)
                torchHoursLeft = TorchBurnHours;
        }
        else if (lanternFuelHours <= 0f)
        {
            ToolStatus.Flash("The Lantern is out of oil — refill it with Lamp Oil from the Inventory (I).", 3f);
            return;
        }

        litId = id;
    }

    void Burn(InventoryManager inventory)
    {
        TimeManager time = TimeManager.Instance;
        if (litId == null || time == null || !time.IsRunning)
            return;

        float hours = Time.deltaTime * time.GameHoursPerRealSecond;
        if (litId == TorchId)
        {
            torchHoursLeft -= hours;
            if (torchHoursLeft <= 0f)
            {
                // Burnt right down: that torch is gone.
                torchHoursLeft = 0f;
                litId = null;
                inventory.RemoveFromPlayer(TorchId, 1);
                ToolStatus.Flash("Your torch has burned out.", 3f);
            }
        }
        else
        {
            lanternFuelHours -= hours;
            if (lanternFuelHours <= 0f)
            {
                lanternFuelHours = 0f;
                litId = null;
                ToolStatus.Flash("The Lantern has run out of oil.", 3f);
            }
        }
    }

    void ReportStatus(string equipped)
    {
        bool lit = litId == equipped;
        string line;
        if (equipped == TorchId)
        {
            float left = lit || torchHoursLeft > 0f ? torchHoursLeft : TorchBurnHours;
            line = lit ? $"Torch — burning, {left:0.0} h left. Click to snuff it."
                       : FireManager.HasIgnition ? $"Torch — {left:0.0} h of burn. Click to light it."
                                                 : "Torch — needs Flint and Steel to light.";
        }
        else
        {
            line = lit ? $"Lantern — lit, {lanternFuelHours:0.0} h of oil. Click to snuff it."
                 : lanternFuelHours > 0f ? $"Lantern — {lanternFuelHours:0.0} h of oil. Click to light it."
                 : "Lantern — out of oil. Refill it with Lamp Oil from the Inventory (I).";
        }
        ToolStatus.Report(line);
    }

    // --- The light itself ---

    void UpdateLight()
    {
        if (litId != null)
            lastLitId = litId; // what a fading light still looks like after it's put out
        float target = litId != null ? 1f : 0f;
        shown = Mathf.MoveTowards(shown, target, FadeSpeed * Time.unscaledDeltaTime);

        if (shown <= 0f)
        {
            if (heldLight != null)
                heldLight.enabled = false;
            return;
        }

        if (!EnsureLight())
            return;

        bool torch = lastLitId == TorchId;
        float full = torch ? TorchIntensity : LanternIntensity;
        float flicker = torch
            ? 0.82f + 0.12f * Mathf.PerlinNoise(Time.time * 9f, 0.7f) + 0.06f * Mathf.Sin(Time.time * 17f)
            : 0.95f + 0.05f * Mathf.PerlinNoise(Time.time * 3f, 0.2f);
        float dying = torch ? Mathf.Lerp(0.4f, 1f, Mathf.Clamp01(torchHoursLeft / TorchLowHours)) : 1f;

        heldLight.enabled = true;
        heldLight.color = torch ? TorchColor : LanternColor;
        heldLight.range = torch ? TorchRange : LanternRange;
        heldLight.intensity = full * shown * flicker * dying;
    }

    // A point light low at the right of the view, about where a held flame would be — kept just out of frame, since
    // directly ahead the light's own glow shows as an orb floating in the air.
    bool EnsureLight()
    {
        if (heldLight != null)
            return true;

        if (player == null)
            player = FindAnyObjectByType<PlayerController>();
        if (player == null || player.CameraTransform == null)
            return false;

        var go = new GameObject("Held Light");
        go.transform.SetParent(player.CameraTransform, false);
        go.transform.localPosition = new Vector3(0.45f, -0.3f, 0.35f);
        heldLight = go.AddComponent<Light>();
        heldLight.type = LightType.Point;
        heldLight.shadows = LightShadows.None;
        go.AddComponent<HDAdditionalLightData>();
        return true;
    }

    // --- Refuelling ---

    public bool CanRefill =>
        InventoryManager.Instance != null && InventoryManager.Instance.Player.Has(LanternId) &&
        InventoryManager.Instance.Player.Has(LampOilId) && lanternFuelHours + LanternHoursPerOil <= LanternCapacityHours + 0.01f;

    // Pours Lamp Oil into the Lantern, one (or as many as fit). Returns how many were used.
    public int Refill(bool all)
    {
        InventoryManager inventory = InventoryManager.Instance;
        int used = 0;
        while (CanRefill && (all || used == 0))
        {
            if (inventory.RemoveFromPlayer(LampOilId, 1) < 1)
                break;
            lanternFuelHours = Mathf.Min(LanternCapacityHours, lanternFuelHours + LanternHoursPerOil);
            used++;
        }
        return used;
    }

    // --- Saving ---

    public void ResetLight()
    {
        torchHoursLeft = 0f;
        lanternFuelHours = 0f;
        litId = null;
        shown = 0f;
    }

    public PortableLightSaveData CaptureState() =>
        new PortableLightSaveData { torchHoursLeft = torchHoursLeft, lanternFuelHours = lanternFuelHours };

    // Whatever was burning is out after a load.
    public void RestoreState(PortableLightSaveData data)
    {
        torchHoursLeft = Mathf.Clamp(data.torchHoursLeft, 0f, TorchBurnHours);
        lanternFuelHours = Mathf.Clamp(data.lanternFuelHours, 0f, LanternCapacityHours);
        litId = null;
    }

    string ISaveable.SaveFile => "player";
    string ISaveable.SaveKey => "portable_light";
    object ISaveable.CaptureState() => CaptureState();
    void ISaveable.RestoreState(string json) => RestoreState(JsonUtility.FromJson<PortableLightSaveData>(json));
}
