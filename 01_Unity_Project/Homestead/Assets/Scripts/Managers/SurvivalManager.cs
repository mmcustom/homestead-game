using System;
using UnityEngine;

[Serializable]
public struct SurvivalSaveData
{
    public float health;
    public float hydration;
    public float hunger;
    public float illnessHours;
    // Warmth is saved as how far below full it is, so saves from before Warmth load as warm rather than frozen.
    public float chill;
    public float wetness;
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
//
// Illness (Phase 3, 2026-09-25): not Core_Survival_System.md's full Illness system (detailed diseases are Future
// Expansion), just a real consequence for eating raw meat or fish or drinking unpurified water. A bout of sickness
// lasts some in-game hours; while it lasts, stamina is cut, Hydration drains faster, Health drops and doesn't recover.
// Two levels (Mike, 2026-09-26): one bout is Mild — an upset stomach that grumbles now and then. Getting sick again
// while sick stacks another bout on, and past one bout's worth of hours the player is Very Sick — harsher effects, and
// they vomit every so often, losing some Hydration and Hunger. As it wears off a Very Sick player drops back to Mild.
// Numbers are a first proposal for Mike to confirm.
//
// Health recovery (Health_System.md's Recovery, Mike 2026-09-26): Health slowly rebuilds on its own whenever neither
// Hunger nor Hydration is empty — faster when both are at least half full — except while sick, and while anything
// else is actively costing Health (a Severe tier, the cold), which wins until it's dealt with.
//
// Warmth (Health_System.md's Exposure System, 2026-09-26): 0-100, drifting with how cold it feels. "Feels like" is the
// air temperature less wind chill (Weather's wind strength, partly blocked under trees) and less a wet chill; being
// wet also makes the cold bite faster. Rain and snow soak the player unless they're under a canopy; they dry slowly on
// their own and fast by a fire. Above about 50°F feels-like Warmth recovers; below it, it falls — faster the colder.
// A lit campfire warms strongly within a couple of metres, fading out by 6 m (the crackle's inner radius, not its
// 22 m hearing range). Tiers mirror Hydration's: Mild — a little more tired; Moderate — less stamina, Health loss
// begins; Severe — fast Health loss and slower movement; 0 — the same collapse path as Health reaching 0.
public class SurvivalManager : MonoBehaviour, ISaveable
{
    public const float MaxValue = 100f;

    public enum Sickness { None, Mild, Severe }

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
    [Tooltip("Health rebuilds at this rate while Hunger and Hydration are both at least Half Fed, and at Low Rate while " +
             "either is lower (but not empty).")]
    [SerializeField, Min(0f)] float recoveryPerHour = 3f;
    [SerializeField, Min(0f)] float lowRecoveryPerHour = 1.5f;
    [SerializeField] float halfFed = 50f;

    [Header("Warmth")]
    [Tooltip("Feels-like temperature (°F) at which Warmth holds steady; warmer recovers it, colder drains it.")]
    [SerializeField] float neutralF = 50f;
    [Tooltip("Warmth per in-game hour per °F away from neutral.")]
    [SerializeField, Min(0f)] float warmthPerDegree = 0.3f;
    [SerializeField, Min(0f)] float maxWarmthLossPerHour = 12f;
    [SerializeField, Min(0f)] float maxWarmthGainPerHour = 5f;
    [Tooltip("°F of wind chill at full wind strength in the open.")]
    [SerializeField, Min(0f)] float windChillF = 15f;
    [Tooltip("°F of chill when soaked through.")]
    [SerializeField, Min(0f)] float wetChillF = 12f;
    [Tooltip("Wetness gained per in-game hour in rain (heavy rain doubles it, snow halves it), 0-1 scale.")]
    [SerializeField, Min(0f)] float soakPerHour = 1.5f;
    [SerializeField, Min(0f)] float dryPerHour = 0.4f;
    [Tooltip("Warmth per in-game hour right beside a lit campfire, and how fast it dries the player.")]
    [SerializeField, Min(0f)] float fireWarmthPerHour = 30f;
    [SerializeField, Min(0f)] float fireDryPerHour = 3f;
    [Tooltip("Full heat within the inner radius, fading to none at the outer radius (metres).")]
    [SerializeField, Min(0f)] float fireInnerRadius = 1.5f;
    [SerializeField, Min(0.1f)] float fireOuterRadius = 6f;
    [SerializeField, Min(0f)] float coldModerateLoss = 1f;
    [SerializeField, Min(0f)] float coldSevereLoss = 4f;
    [SerializeField, Min(0f)] float coldEmptyLoss = 8f;

    [Header("Illness")]
    [Tooltip("In-game hours one bout of sickness lasts (8 h = 10 real minutes). More than this left is Very Sick.")]
    [SerializeField, Min(0.1f)] float illnessHoursPerBout = 8f;
    [Tooltip("Getting sick again while sick adds another bout, up to this many hours.")]
    [SerializeField, Min(0f)] float maxIllnessHours = 16f;
    [Tooltip("Stamina multiplier while Mild / Very Sick (on top of Hydration and Hunger).")]
    [SerializeField, Range(0f, 1f)] float mildStamina = 0.8f, severeStamina = 0.5f;
    [Tooltip("Hydration drain multiplier while Mild / Very Sick.")]
    [SerializeField, Min(1f)] float mildHydration = 1.25f, severeHydration = 2f;
    [Tooltip("Health lost per in-game hour while Mild / Very Sick.")]
    [SerializeField, Min(0f)] float mildHealthLoss = 0.5f, severeHealthLoss = 2f;
    [Tooltip("In-game hours between stomach grumbles while Mild.")]
    [SerializeField, Min(0.1f)] float grumbleEveryHours = 2f;
    [Tooltip("In-game hours between bouts of vomiting while Very Sick.")]
    [SerializeField, Min(0.1f)] float vomitEveryHours = 1.5f;
    [Tooltip("Hydration and Hunger lost each time the player vomits.")]
    [SerializeField, Min(0f)] float vomitHydrationLoss = 8f, vomitHungerLoss = 8f;

    // Stamina and work-efficiency multipliers per tier (Fine, Minor, Reduced, Severe, Empty). The worse of Hydration's
    // and Hunger's applies rather than both stacking.
    static readonly float[] HydrationStamina = { 1f, 0.85f, 0.65f, 0.4f, 0.4f };
    static readonly float[] HungerStamina = { 1f, 0.9f, 0.75f, 0.5f, 0.5f };
    static readonly float[] HydrationEfficiency = { 1f, 1f, 0.75f, 0.5f, 0.4f };
    static readonly float[] HungerEfficiency = { 1f, 1f, 0.85f, 0.6f, 0.5f };
    static readonly float[] WarmthStamina = { 1f, 0.9f, 0.7f, 0.5f, 0.4f };
    static readonly float[] WarmthMovement = { 1f, 1f, 1f, 0.75f, 0.6f };

    float health = MaxValue;
    float hydration = MaxValue;
    float hunger = MaxValue;
    float illnessHours;
    float warmth = MaxValue;
    float wetness;       // 0-1
    float fireHeat;      // 0-1, how close to a lit fire the player was last step
    float symptomHours; // in-game hours until the next grumble or vomit
    bool collapsed;
    PlayerController player;
    float nextPlayerSearch;

    public event Action Collapsed;
    // Sickness symptoms, for sound and messages: a stomach grumble while Mild (including when falling ill), and
    // vomiting while Very Sick (including on becoming Very Sick).
    public event Action StomachUpset;
    public event Action Vomited;

    public float Health => health;
    public float Hydration => hydration;
    public float Hunger => hunger;
    public bool IsCollapsed => collapsed;
    public float Warmth => warmth;
    public Tier WarmthTier => TierOf(warmth);
    public float Wetness => wetness;
    public bool NearFire => fireHeat > 0.05f;
    // Severe cold slows movement.
    public float MovementMultiplier => WarmthMovement[(int)WarmthTier];
    public bool IsSick => illnessHours > 0f;
    public Sickness SicknessLevel => illnessHours <= 0f ? Sickness.None
                                   : illnessHours > illnessHoursPerBout ? Sickness.Severe : Sickness.Mild;
    public float IllnessHoursLeft => illnessHours;

    public Tier HydrationTier => TierOf(hydration);
    public Tier HungerTier => TierOf(hunger);

    // Multiplies the player's maximum stamina and recovery rate.
    public float StaminaMultiplier => Mathf.Min(Mathf.Min(HydrationStamina[(int)HydrationTier], HungerStamina[(int)HungerTier]),
                                                WarmthStamina[(int)WarmthTier]) *
                                      ByLevel(1f, mildStamina, severeStamina);

    // For work actions (chopping, harvesting, building) once they exist: 1 = full speed.
    public float WorkEfficiency => Mathf.Min(HydrationEfficiency[(int)HydrationTier], HungerEfficiency[(int)HungerTier]) *
                                   ByLevel(1f, mildStamina, severeStamina);

    public float IllnessHydrationMultiplier => ByLevel(1f, mildHydration, severeHydration);

    float ByLevel(float none, float mild, float severe)
    {
        Sickness level = SicknessLevel;
        return level == Sickness.Severe ? severe : level == Sickness.Mild ? mild : none;
    }

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
        StepWarmth(hours, tempF);

        float healthLoss = HealthLoss(HydrationTier, dehydrationSevereLoss, dehydrationEmptyLoss) +
                           HealthLoss(HungerTier, starvationSevereLoss, starvationEmptyLoss) +
                           ByLevel(0f, mildHealthLoss, severeHealthLoss) +
                           ColdLoss(WarmthTier);
        if (IsSick)
        {
            illnessHours = Mathf.Max(0f, illnessHours - hours);
            symptomHours -= hours;
            if (IsSick && symptomHours <= 0f)
                Symptom();
        }
        if (healthLoss > 0f)
            SetHealth(health - healthLoss * hours);
        else if (!IsSick && hydration > 0f && hunger > 0f)
            SetHealth(health + (hydration >= halfFed && hunger >= halfFed ? recoveryPerHour : lowRecoveryPerHour) * hours);
    }

    float ColdLoss(Tier tier) =>
        tier == Tier.Empty ? coldEmptyLoss : tier == Tier.Severe ? coldSevereLoss : tier == Tier.Reduced ? coldModerateLoss : 0f;

    // Warmth and wetness for a stretch of time, from the weather where the player stands and any fire nearby.
    void StepWarmth(float hours, float tempF)
    {
        WeatherManager weather = WeatherManager.Instance;
        bool placed = player != null;
        Vector3 position = placed ? player.transform.position : Vector3.zero;

        // Under trees: most rain and some wind are kept off.
        float rainShelter = placed ? OverheadCover.At(position + Vector3.up, 0.8f) : 0f;
        float windShelter = placed ? OverheadCover.At(position + Vector3.up, 0.4f) : 0f;

        fireHeat = placed ? FireHeat(position) : 0f;

        float soak = 0f;
        if (weather != null && weather.IsPrecipitating)
        {
            float intensity = weather.IsSnowing ? 0.5f
                            : weather.PrecipitationWeather == WeatherType.LightRain ? 1f : 2f;
            soak = soakPerHour * intensity * (1f - rainShelter);
        }
        float dry = soak > 0f ? 0f : dryPerHour;
        wetness = Mathf.Clamp01(wetness + (soak - dry - fireDryPerHour * fireHeat) * hours);

        float wind = weather != null ? weather.WindStrength : 0f;
        float feelsF = tempF - windChillF * wind * (1f - windShelter) - wetChillF * wetness;
        float rate = (feelsF - neutralF) * warmthPerDegree;
        if (rate < 0f)
            rate = Mathf.Max(-maxWarmthLossPerHour, rate * (1f + wetness));
        else
            rate = Mathf.Min(maxWarmthGainPerHour, rate);
        rate += fireWarmthPerHour * fireHeat;
        warmth = Mathf.Clamp(warmth + rate * hours, 0f, MaxValue);
    }

    // 0-1: full beside a lit campfire, fading out with distance, and weaker as its fuel runs low.
    float FireHeat(Vector3 position)
    {
        FireManager fires = FireManager.Instance;
        if (fires == null)
            return 0f;
        CampfireState fire = fires.NearestLit(position, fireOuterRadius);
        if (fire == null)
            return 0f;
        float distance = Vector2.Distance(new Vector2(fire.position.x, fire.position.z), new Vector2(position.x, position.z));
        float heat = 1f - Mathf.InverseLerp(fireInnerRadius, fireOuterRadius, distance);
        return heat * Mathf.Clamp01(0.4f + fire.fuelHours); // a fire down to its embers warms less
    }

    static float HealthLoss(Tier tier, float severe, float empty) =>
        tier == Tier.Empty ? empty : tier == Tier.Severe ? severe : 0f;

    // Water System: drinking. Returns how much was actually restored.
    public float Drink(float amount) => Restore(ref hydration, amount);

    // Food System: eating. Returns how much was actually restored.
    public float Eat(float amount) => Restore(ref hunger, amount);

    // Rolls a chance of falling ill (eating raw meat, drinking unpurified water). Returns whether it happened.
    public bool RollIllness(float chance)
    {
        if (chance <= 0f || UnityEngine.Random.value >= chance)
            return false;
        MakeSick();
        return true;
    }

    public void MakeSick()
    {
        Sickness before = SicknessLevel;
        illnessHours = Mathf.Min(maxIllnessHours, illnessHours + illnessHoursPerBout);
        if (SicknessLevel != before)
            Symptom(); // the stomach turns straight away, or the first vomit on becoming Very Sick
    }

    // A grumble while Mild, or vomiting while Very Sick; then waits for the next one.
    void Symptom()
    {
        if (SicknessLevel == Sickness.Severe)
        {
            symptomHours = vomitEveryHours;
            hydration = Mathf.Max(0f, hydration - vomitHydrationLoss);
            hunger = Mathf.Max(0f, hunger - vomitHungerLoss);
            Vomited?.Invoke();
        }
        else
        {
            symptomHours = grumbleEveryHours;
            StomachUpset?.Invoke();
        }
    }

    public void Cure() => illnessHours = symptomHours = 0f;

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
        health = hydration = hunger = warmth = MaxValue;
        illnessHours = symptomHours = wetness = 0f;
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

    public void DebugSetWarmth(float newWarmth, float newWetness)
    {
        warmth = Mathf.Clamp(newWarmth, 0f, MaxValue);
        wetness = Mathf.Clamp01(newWetness);
    }
#endif

    public SurvivalSaveData CaptureState() => new SurvivalSaveData { health = health, hydration = hydration, hunger = hunger, illnessHours = illnessHours,
                                                                  chill = MaxValue - warmth, wetness = wetness };

    public void RestoreState(SurvivalSaveData data)
    {
        hydration = Mathf.Clamp(data.hydration, 0f, MaxValue);
        hunger = Mathf.Clamp(data.hunger, 0f, MaxValue);
        health = Mathf.Clamp(data.health, 0f, MaxValue);
        illnessHours = Mathf.Clamp(data.illnessHours, 0f, maxIllnessHours);
        warmth = Mathf.Clamp(MaxValue - data.chill, 0f, MaxValue);
        wetness = Mathf.Clamp01(data.wetness);
        symptomHours = grumbleEveryHours * 0.5f;
        collapsed = health <= 0f;
    }

    // Save_Data_Model.md's Player Block: survival stats.
    string ISaveable.SaveFile => "player";
    string ISaveable.SaveKey => "survival";
    object ISaveable.CaptureState() => CaptureState();
    void ISaveable.RestoreState(string json) => RestoreState(JsonUtility.FromJson<SurvivalSaveData>(json));
}
