using System;
using UnityEngine;

[Serializable]
public struct SurvivalSaveData
{
    public float health;
    public float hydration;
    public float hunger;
}

// Core_Survival_System.md's Health, Hydration and Hunger (Unity_Architecture.md: player survival stats).
// All three run 0-100. Hydration and Hunger drain continuously while the game is playing; low values cut stamina and
// work efficiency and eventually cost Health. Health collapsing at 0 raises Collapsed — death is still TBD in the doc.
//
// Rates are in in-game hours and run at the normal clock rate (30 real minutes per day, so one in-game hour is 75 real
// seconds). The debug fast-forward and day skips deliberately don't drain them, so testing weather doesn't starve the
// player; Sleep, when built, should call PassHours for the time slept.
//
// Hydration's tiers come from the doc; Hunger's tiers, every decay rate, Health recovery and the stamina/efficiency
// numbers are Claude Code's first proposal (2026-09-25), pending Mike's playtest.
public class SurvivalManager : MonoBehaviour, ISaveable
{
    public const float MaxValue = 100f;

    // Core_Survival_System.md's Hydration effect tiers; Hunger mirrors them.
    public enum Tier { Fine, Minor, Reduced, Severe, Empty }

    public static SurvivalManager Instance { get; private set; }

    [Header("Drain (points per in-game hour, at rest)")]
    [Tooltip("100 → 0 in about 36 in-game hours (45 real minutes) standing still.")]
    [SerializeField, Min(0f)] float hydrationPerHour = 2.8f;
    [Tooltip("\"Hunger decreases slower than hydration\": 100 → 0 in about 72 in-game hours (90 real minutes).")]
    [SerializeField, Min(0f)] float hungerPerHour = 1.4f;

    [Header("Activity (drain multipliers)")]
    [SerializeField, Min(0f)] float walkingHydration = 1.2f;
    [SerializeField, Min(0f)] float sprintingHydration = 2f;
    [SerializeField, Min(0f)] float walkingHunger = 1.15f;
    [SerializeField, Min(0f)] float sprintingHunger = 1.6f;

    [Header("Temperature (°F)")]
    [Tooltip("Hydration drains faster above this, reaching the max multiplier at the hot limit.")]
    [SerializeField] float warmF = 80f;
    [SerializeField] float hotF = 95f;
    [SerializeField, Min(1f)] float hotHydrationMultiplier = 1.5f;
    [Tooltip("Hunger drains faster below this (keeping warm burns food), reaching the max multiplier at the frigid limit.")]
    [SerializeField] float coolF = 40f;
    [SerializeField] float frigidF = 10f;
    [SerializeField, Min(1f)] float coldHungerMultiplier = 1.3f;

    [Header("Health (points per in-game hour)")]
    [SerializeField, Min(0f)] float dehydrationSevereLoss = 1.5f;
    [SerializeField, Min(0f)] float dehydrationEmptyLoss = 8f;
    [SerializeField, Min(0f)] float starvationSevereLoss = 1f;
    [SerializeField, Min(0f)] float starvationEmptyLoss = 4f;
    [Tooltip("Slow recovery while both Hydration and Hunger are at least this high and nothing is draining Health.")]
    [SerializeField] float recoveryThreshold = 75f;
    [SerializeField, Min(0f)] float recoveryPerHour = 2f;

    // Stamina and work-efficiency multipliers per tier (Fine, Minor, Reduced, Severe, Empty). The worse of Hydration's
    // and Hunger's applies rather than both stacking.
    static readonly float[] HydrationStamina = { 1f, 0.85f, 0.65f, 0.4f, 0.4f };
    static readonly float[] HungerStamina = { 1f, 0.9f, 0.75f, 0.5f, 0.5f };
    static readonly float[] HydrationEfficiency = { 1f, 1f, 0.75f, 0.5f, 0.4f };
    static readonly float[] HungerEfficiency = { 1f, 1f, 0.85f, 0.6f, 0.5f };

    float health = MaxValue;
    float hydration = MaxValue;
    float hunger = MaxValue;
    bool collapsed;
    PlayerController player;
    float nextPlayerSearch;

    public event Action Collapsed;

    public float Health => health;
    public float Hydration => hydration;
    public float Hunger => hunger;
    public bool IsCollapsed => collapsed;

    public Tier HydrationTier => TierOf(hydration);
    public Tier HungerTier => TierOf(hunger);

    // Multiplies the player's maximum stamina and recovery rate.
    public float StaminaMultiplier => Mathf.Min(HydrationStamina[(int)HydrationTier], HungerStamina[(int)HungerTier]);

    // For work actions (chopping, harvesting, building) once they exist: 1 = full speed.
    public float WorkEfficiency => Mathf.Min(HydrationEfficiency[(int)HydrationTier], HungerEfficiency[(int)HungerTier]);

    // Hook for the Illness system (not yet implemented), which should raise Hydration drain.
    public float IllnessHydrationMultiplier { get; set; } = 1f;

    static bool Playing => GameManager.Instance == null || GameManager.Instance.State == GameState.Playing;

    public static Tier TierOf(float value)
    {
        if (value <= 0f) return Tier.Empty;
        if (value < 25f) return Tier.Severe;
        if (value < 50f) return Tier.Reduced;
        if (value < 75f) return Tier.Minor;
        return Tier.Fine;
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

    void Update()
    {
        TimeManager time = TimeManager.Instance;
        if (time == null || !time.IsRunning || !Playing)
            return;

        FindPlayer();
        if (player == null)
            return; // no player in this scene (menus)

        PassHours(Time.deltaTime * time.GameHoursPerRealSecond, player.State);
    }

    void FindPlayer()
    {
        if (player != null || Time.unscaledTime < nextPlayerSearch)
            return;

        nextPlayerSearch = Time.unscaledTime + 1f;
        player = FindAnyObjectByType<PlayerController>();
    }

    // Advances the stats by an amount of in-game time, e.g. hours spent asleep. Long spans run in short steps so a
    // stat crossing into a worse tier partway through only costs Health from that point on.
    public void PassHours(float hours, MovementState activity = MovementState.Idle)
    {
        const float MaxStepHours = 0.25f;
        while (hours > 0f)
        {
            float step = Mathf.Min(hours, MaxStepHours);
            Step(step, activity);
            hours -= step;
        }
    }

    void Step(float hours, MovementState activity)
    {
        float tempF = WeatherManager.Instance != null ? WeatherManager.Instance.TemperatureF : 60f;

        float hydrationRate = hydrationPerHour * IllnessHydrationMultiplier *
                              Mathf.Lerp(1f, hotHydrationMultiplier, Mathf.InverseLerp(warmF, hotF, tempF));
        float hungerRate = hungerPerHour * Mathf.Lerp(1f, coldHungerMultiplier, Mathf.InverseLerp(coolF, frigidF, tempF));

        if (activity == MovementState.Sprinting)
        {
            hydrationRate *= sprintingHydration;
            hungerRate *= sprintingHunger;
        }
        else if (activity == MovementState.Walking || activity == MovementState.Crouching)
        {
            hydrationRate *= walkingHydration;
            hungerRate *= walkingHunger;
        }

        hydration = Mathf.Max(0f, hydration - hydrationRate * hours);
        hunger = Mathf.Max(0f, hunger - hungerRate * hours);

        float healthLoss = HealthLoss(HydrationTier, dehydrationSevereLoss, dehydrationEmptyLoss) +
                           HealthLoss(HungerTier, starvationSevereLoss, starvationEmptyLoss);
        if (healthLoss > 0f)
            SetHealth(health - healthLoss * hours);
        else if (hydration >= recoveryThreshold && hunger >= recoveryThreshold)
            SetHealth(health + recoveryPerHour * hours);
    }

    static float HealthLoss(Tier tier, float severe, float empty) =>
        tier == Tier.Empty ? empty : tier == Tier.Severe ? severe : 0f;

    // Water System: drinking. Returns how much was actually restored.
    public float Drink(float amount) => Restore(ref hydration, amount);

    // Food System: eating. Returns how much was actually restored.
    public float Eat(float amount) => Restore(ref hunger, amount);

    static float Restore(ref float stat, float amount)
    {
        float before = stat;
        stat = Mathf.Clamp(stat + Mathf.Max(0f, amount), 0f, MaxValue);
        return stat - before;
    }

    void SetHealth(float value)
    {
        health = Mathf.Clamp(value, 0f, MaxValue);

        if (health > 0f)
        {
            collapsed = false;
        }
        else if (!collapsed)
        {
            collapsed = true;
            Debug.LogWarning("[Survival] Player collapsed (Health 0). Death mechanics are TBD in Core_Survival_System.md.");
            Collapsed?.Invoke();
        }
    }

    // Everything full — used when starting a new game.
    public void ResetStats()
    {
        health = hydration = hunger = MaxValue;
        collapsed = false;
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    // Testing only (TimeDebugControls).
    public void DebugSet(float newHydration, float newHunger, float newHealth)
    {
        hydration = Mathf.Clamp(newHydration, 0f, MaxValue);
        hunger = Mathf.Clamp(newHunger, 0f, MaxValue);
        SetHealth(newHealth);
    }
#endif

    public SurvivalSaveData CaptureState() => new SurvivalSaveData { health = health, hydration = hydration, hunger = hunger };

    public void RestoreState(SurvivalSaveData data)
    {
        hydration = Mathf.Clamp(data.hydration, 0f, MaxValue);
        hunger = Mathf.Clamp(data.hunger, 0f, MaxValue);
        health = Mathf.Clamp(data.health, 0f, MaxValue);
        collapsed = health <= 0f;
    }

    // Save_Data_Model.md's Player Block: survival stats.
    string ISaveable.SaveFile => "player";
    string ISaveable.SaveKey => "survival";
    object ISaveable.CaptureState() => CaptureState();
    void ISaveable.RestoreState(string json) => RestoreState(JsonUtility.FromJson<SurvivalSaveData>(json));
}
