using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// The Bow Drill (Core_Survival_System.md, 2026-10-07): a primitive fire starter for a player with no Flint and Steel.
// Crafted from 2 Sticks + 1 Cordage; reusable, no durability. Equip it, face a built Campfire that has fuel and hold
// click: a progress bar fills over a few seconds (an attempt), which costs stamina and moves the clock on about ten
// minutes, and then the ember either catches (the Campfire lights exactly as with Flint and Steel) or dies. A failure
// loses nothing else. The chance depends on the weather, is cut by rain and snow unless the Campfire is under a roof,
// and grows with each failed attempt at the same Campfire (a growing ember) until it catches or the player has been away
// about an hour. Flint and Steel is untouched and still never fails. All numbers are BowDrillSettings on FireManager.
[DefaultExecutionOrder(90)]
public class BowDrill : MonoBehaviour
{
    public const string ItemId = "bow_drill";

    public static BowDrill Instance { get; private set; }

    struct Ember
    {
        public float bonus;
        public double lastHour;
    }

    InputAction attack;
    PlayerController player;
    float nextPlayerSearch;
    int targetId = -1;
    float progress;
    bool needRelease;
    readonly Dictionary<int, Ember> embers = new Dictionary<int, Ember>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => Instance = null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (Instance == null)
            new GameObject("Bow Drill").AddComponent<BowDrill>();
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
        if (Instance == this)
            Instance = null;
    }

    static bool Playing => GameManager.Instance == null || GameManager.Instance.State == GameState.Playing;

    void Update()
    {
        InventoryManager inventory = InventoryManager.Instance;
        FireManager fires = FireManager.Instance;
        bool holding = inventory != null && inventory.EquippedTool != null && inventory.EquippedTool.Id == ItemId &&
                       inventory.Player.Has(ItemId);
        if (!holding || fires == null || attack == null || !Playing)
        {
            Cancel();
            return;
        }

        if (player == null && Time.unscaledTime >= nextPlayerSearch)
        {
            nextPlayerSearch = Time.unscaledTime + 1f;
            player = FindAnyObjectByType<PlayerController>();
        }
        if (player == null || !player.CanUseTools)
        {
            Cancel();
            return;
        }

        BowDrillSettings settings = fires.BowDrill;
        CampfireState fire = null;
        if (player.AimRaycast(settings.reach, out RaycastHit hit))
        {
            Campfire view = hit.collider.GetComponentInParent<Campfire>();
            fire = view != null ? view.State : null;
        }

        if (fire == null)
        {
            Cancel();
            ToolStatus.Report("Bow Drill — face a built Campfire with fuel in it and hold click to start a fire");
            return;
        }
        if (fire.lit)
        {
            Cancel();
            ToolStatus.Report("Bow Drill — the Campfire is already burning");
            return;
        }
        if (fire.fuelHours <= 0f)
        {
            Cancel();
            ToolStatus.Report("Bow Drill — the fire needs fuel before you can start it");
            if (attack.WasPressedThisFrame())
                ToolStatus.Flash("The fire needs fuel before you can start it.", 2.5f);
            return;
        }

        if (targetId != fire.id)
        {
            Cancel();
            targetId = fire.id;
        }

        float chance = Chance(settings, fire);
        if (!attack.IsPressed())
        {
            needRelease = false;
            progress = 0f;
            ToolStatus.Report($"Bow Drill — hold click to drill  ({chance:P0} chance to catch)");
            return;
        }
        if (needRelease)
        {
            ToolStatus.Report("Bow Drill — release click to try again");
            return;
        }
        if (progress <= 0f && player.Stamina < settings.staminaCost)
        {
            ToolStatus.Report("Bow Drill — too tired to drill; catch your breath");
            return;
        }

        progress += Time.deltaTime / Mathf.Max(0.1f, settings.drillSeconds);
        ToolStatus.Report($"Drilling…  ({chance:P0} chance to catch)", Mathf.Clamp01(progress));
        if (progress >= 1f)
        {
            progress = 0f;
            needRelease = true;
            Attempt(settings, fire, chance);
        }
    }

    void Cancel()
    {
        targetId = -1;
        progress = 0f;
        needRelease = false;
    }

    // The chance the next attempt at this Campfire catches: the weather's figure plus any growing-ember bonus.
    float Chance(BowDrillSettings s, CampfireState fire)
    {
        WeatherManager weather = WeatherManager.Instance;
        WeatherType type = weather != null ? weather.Current : WeatherType.Clear;
        float chance;
        switch (type)
        {
            case WeatherType.Wind:
                chance = s.chanceWind;
                break;
            case WeatherType.LightRain:
            case WeatherType.Snow:
                chance = Covered(fire) && s.coverIgnoresPrecipitation ? s.chanceCalm : s.chanceLightPrecipitation;
                break;
            case WeatherType.HeavyRain:
            case WeatherType.Thunderstorm:
                chance = Covered(fire) && s.coverIgnoresPrecipitation ? s.chanceCalm : s.chanceHeavyPrecipitation;
                break;
            default:
                chance = s.chanceCalm;
                break;
        }
        return Mathf.Clamp01(chance + EmberBonus(s, fire));
    }

    // Under a roof (a built shelter), not just trees.
    static bool Covered(CampfireState fire) => OverheadCover.Roofed(fire.position + Vector3.up * 0.3f);

    float EmberBonus(BowDrillSettings s, CampfireState fire)
    {
        if (!embers.TryGetValue(fire.id, out Ember ember))
            return 0f;
        TimeManager time = TimeManager.Instance;
        if (time != null && time.TotalHours - ember.lastHour > s.emberForgetHours)
        {
            embers.Remove(fire.id);
            return 0f;
        }
        return ember.bonus;
    }

    void Attempt(BowDrillSettings s, CampfireState fire, float chance)
    {
        if (!player.TrySpendStamina(s.staminaCost))
        {
            ToolStatus.Flash("Too tired to drill — catch your breath", 2f);
            return;
        }

        // Ten minutes go by: the clock, the player's own needs, the fires and the wood, as in a night's sleep.
        TimeManager time = TimeManager.Instance;
        float hours = s.minutesPerAttempt / 60f;
        if (time != null && hours > 0f)
        {
            if (SurvivalManager.Instance != null)
                SurvivalManager.Instance.PassHours(hours, MovementState.Idle);
            if (FireManager.Instance != null)
                FireManager.Instance.PassHours(hours);
            if (WoodManager.Instance != null)
                WoodManager.Instance.PassHours(hours);
            time.AdvanceMinutes(s.minutesPerAttempt);
        }

        if (Random.value < chance && FireManager.Instance.Ignite(fire))
        {
            embers.Remove(fire.id);
            ToolStatus.Flash("The ember catches — the fire is lit.", 3f);
            return;
        }

        float bonus = embers.TryGetValue(fire.id, out Ember previous) ? previous.bonus : 0f;
        embers[fire.id] = new Ember
        {
            bonus = Mathf.Min(s.emberBonusCap, bonus + s.emberBonusPerFailure),
            lastHour = time != null ? time.TotalHours : 0.0,
        };
        ToolStatus.Flash("The ember died. Try again.", 2.5f);
    }
}
