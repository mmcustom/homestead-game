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
    // Core Temperature Model: core temperature is the stored value (chill above is still written, derived from Warmth, so an
    // older build can read a new save). A real core temperature is never below 28, so 20 or less means "not saved yet".
    public float coreTempC;
    public float damp;
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
// Warmth (Health_System.md's Exposure System, 2026-09-26) is now read off the body's core temperature (Core Temperature
// Model, 2026-10-07; the physics is in BodyHeat): 100 is 37.0 °C, 75 is 36.5, 50 is 35.0, 25 is 32.0, 0 is 28.0. Core
// temperature moves with heat produced (resting heat × the activity's MET, or the shivering the body adds when cold)
// against heat lost through clothing, an air layer that thins in wind, and any bedding or shelter. Rain and snow soak the
// player unless under a canopy, and wet clothing insulates far worse; hard work in the cold builds a sweat damp that does
// the same, more mildly. A lit campfire is radiant gain, strongest within 1.5 m and gone by 6 m (the crackle's inner
// radius, not its 22 m hearing range). Tiers mirror Hydration's: Mild — a little more tired; Moderate — less stamina,
// Health loss begins; Severe — fast Health loss and slower movement; 0 — the same collapse path as Health reaching 0.
// The hot side (humidity, sun, overheating) is not built; HeatLevel and HeatLoss are its hooks.
public class SurvivalManager : MonoBehaviour, ISaveable
{
    public const float MaxValue = 100f;

    public enum Sickness { None, Mild, Severe }

    // Core_Survival_System.md's Hydration effect tiers; Hunger mirrors them.
    public enum Tier { Fine, Minor, Reduced, Severe, Empty }

    // Overheating steps (Weather_System.md's Heat Inputs): Hot from 37.5 °C, heat exhaustion from 38, severe from 39,
    // heat stroke above 40. Nothing acts on them yet.
    public enum HeatStage { None, Hot, Exhaustion, Severe, Stroke }

    public static SurvivalManager Instance { get; private set; }

    [Header("Drain (points per in-game hour, at rest)")]
    [Tooltip("100 → 0 in about 36 in-game hours (45 real minutes) standing still.")]
    [SerializeField, Min(0f)] float hydrationPerHour = 2.8f;
    [Tooltip("\"Hunger decreases slower than hydration\": 100 → 0 in about 72 in-game hours (90 real minutes).")]
    [SerializeField, Min(0f)] float hungerPerHour = 1.4f;

    [Header("Activity (drain multipliers)")]
    [SerializeField, Min(0f)] float walkingHydration = 1.2f;
    [SerializeField, Min(0f)] float sprintingHydration = 2f;
    [Tooltip("Hunger drains this much faster per extra MET of body heat production (walking 3.5 MET, sprinting 10, shivering " +
             "up to 4.9). Replaces the old walking and sprinting multipliers, which were a rough fit to this.")]
    [SerializeField, Min(0f)] float hungerPerExtraMet = 0.06f;

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

    [Header("Rain and fire")]
    [Tooltip("Wetness gained per in-game hour in rain (heavy rain doubles it, snow halves it), 0-1 scale.")]
    [SerializeField, Min(0f)] float soakPerHour = 1.5f;
    [SerializeField, Min(0f)] float dryPerHour = 0.4f;
    [Tooltip("How fast a lit campfire dries the player, per in-game hour at full heat.")]
    [SerializeField, Min(0f)] float fireDryPerHour = 3f;
    [Tooltip("Full heat within the inner radius, fading to none at the outer radius (metres).")]
    [SerializeField, Min(0f)] float fireInnerRadius = 1.5f;
    [SerializeField, Min(0.1f)] float fireOuterRadius = 6f;
    [SerializeField, Min(0f)] float coldModerateLoss = 1f;
    [SerializeField, Min(0f)] float coldSevereLoss = 4f;
    [SerializeField, Min(0f)] float coldEmptyLoss = 8f;

    [Header("Body heat (Core Temperature Model)")]
    [Tooltip("The physics: body size, heat capacity, insulation, shivering. See BodyHeat.")]
    [SerializeField] BodyHeat.Settings body = new BodyHeat.Settings();
    [Tooltip("Radiant heat on the player standing close to a lit campfire at full fuel, in watts.")]
    [SerializeField, Min(0f)] float fireRadiantWatts = 250f;
    [Tooltip("Wind speed in m/s at full wind strength (the weather's 0-1 strength has no units).")]
    [SerializeField, Min(0f)] float windMetersPerSecondAtFull = 8f;
    [Tooltip("Resistance (m²K/W) that a bed's or shelter's \"cuts the cold by X\" is converted against: about 2 clo of " +
             "clothing plus still air.")]
    [SerializeField, Min(0.05f)] float bedReferenceResistance = 0.44f;

    [Header("Seasonal clothing (until a clothing system exists)")]
    [Tooltip("The player dresses for the recent temperature: this much insulation (clo) at or below the cold temperature, " +
             "falling to the warm figure at or above the warm temperature.")]
    [SerializeField, Min(0f)] float winterClo = 2f;
    [SerializeField, Min(0f)] float summerClo = 0.8f;
    [SerializeField] float clothingColdC = -1f;
    [SerializeField] float clothingWarmC = 24f;
    [Tooltip("In-game hours over which the recent temperature is averaged, so clothing doesn't change with every swing of the day.")]
    [SerializeField, Min(1f)] float clothingSmoothHours = 24f;

    [Header("Activity heat (MET: multiples of resting heat)")]
    [SerializeField, Min(0.5f)] float sleepMet = 0.9f;
    [SerializeField, Min(0.5f)] float idleMet = 1f;
    [SerializeField, Min(0.5f)] float crouchMet = 2.5f;
    [SerializeField, Min(0.5f)] float walkMet = 3.5f;
    [SerializeField, Min(0.5f)] float sprintMet = 10f;
    [Tooltip("Extra MET for moving at the carry limit, scaled by the share of the limit carried (carrying 25-49 lb is 5 MET).")]
    [SerializeField, Min(0f)] float carryMetBonus = 1.5f;
    [Tooltip("MET while working flat out with a tool (chopping 5, digging 5-6). Swings raise the working level; it fades in seconds.")]
    [SerializeField, Min(1f)] float workMet = 5.5f;
    [SerializeField, Range(0f, 1f)] float workPerSwing = 0.35f;
    [SerializeField, Min(0f)] float workFadePerSecond = 0.15f;

    [Header("Shivering and sweat")]
    [Tooltip("Stamina multiplier at full shivering (through StaminaMultiplier, as the lowest of the factors).")]
    [SerializeField, Range(0f, 1f)] float shiverStamina = 0.7f;
    [Tooltip("Sweat-damp (0-1) builds this much per in-game hour while the body is shedding work heat in cold air.")]
    [SerializeField, Min(0f)] float dampBuildPerHour = 0.5f;
    [Tooltip("Sweat only builds while the body is working at least this hard (MET): real work, not walking, and not just sitting by a fire.")]
    [SerializeField, Min(1f)] float dampMinMet = 4.5f;
    [Tooltip("Core temperature above which work heat is being shed as sweat.")]
    [SerializeField] float dampAboveCoreC = 36.9f;
    [Tooltip("Sweat only soaks clothing in air colder than this.")]
    [SerializeField] float dampBelowAirC = 15f;
    [Tooltip("Damp cleared per in-game hour: beside a fire (about an hour), in a dry shelter (about three), in open dry air; none in rain.")]
    [SerializeField, Min(0f)] float dampClearByFire = 1f;
    [SerializeField, Min(0f)] float dampClearInShelter = 0.33f;
    [SerializeField, Min(0f)] float dampClearInOpen = 0.1f;

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
    float coreC = 37f;   // the stored value; Warmth is read off it
    float wetness;       // 0-1, soaked by rain
    float damp;          // 0-1, soaked by sweat
    float shiver;        // 0-1, as of the last step
    float totalMet = 1f; // what the body produced last step, in multiples of resting heat
    float workLevel;     // 0-1, raised by tool swings, fades in seconds
    float smoothedTempC; // recent air temperature, for the seasonal clothing
    bool smoothedReady;
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
    // Derived from core temperature (Core Temperature Model); 100 at 37.0 °C and above.
    public float Warmth => BodyHeat.CoreToWarmth(coreC);
    public Tier WarmthTier => TierOf(Warmth);
    public float CoreTemperatureC => coreC;
    public float Wetness => wetness;
    public float Damp => damp;
    public bool NearFire => fireHeat > 0.05f;
    public float ShiverIntensity => shiver;
    public bool IsShivering => shiver > 0.05f;
    // Hard work is making real heat (chopping and digging are about 5 MET) without the body having to shiver for it.
    public bool IsWorkingUpHeat => totalMet >= 4f && shiver <= 0.05f;
    public HeatStage HeatLevel => coreC > 40f ? HeatStage.Stroke : coreC > 39f ? HeatStage.Severe
                                : coreC > 38f ? HeatStage.Exhaustion : coreC > 37.5f ? HeatStage.Hot : HeatStage.None;
    // Severe cold slows movement.
    public float MovementMultiplier => WarmthMovement[(int)WarmthTier];
    public bool IsSick => illnessHours > 0f;

    // Set by SleepManager while the player sleeps (Building_Housing_System.md's Sleep System): how much of the cold the
    // bedding and shelter keep off (0-1, cutting heat loss), how much rain and wind the shelter blocks, and how much
    // faster Health recovers at rest. Back to nothing when they wake.
    public float SleepInsulation { get; set; }
    public float SleepRainShelter { get; set; }
    public float SleepWindShelter { get; set; }
    public float SleepRecovery { get; set; } = 1f;
    public Sickness SicknessLevel => illnessHours <= 0f ? Sickness.None
                                   : illnessHours > illnessHoursPerBout ? Sickness.Severe : Sickness.Mild;
    public float IllnessHoursLeft => illnessHours;

    public Tier HydrationTier => TierOf(hydration);
    public Tier HungerTier => TierOf(hunger);

    // Shivering saps stamina, down to this at full shivering. It joins the other factors as the lowest of them, so it
    // never stacks on top of the Warmth tier's own penalty. Heat exhaustion's penalty will join the same way.
    float ShiverStaminaFactor => Mathf.Lerp(1f, shiverStamina, shiver);

    // Multiplies the player's maximum stamina and recovery rate.
    public float StaminaMultiplier => Mathf.Min(Mathf.Min(HydrationStamina[(int)HydrationTier], HungerStamina[(int)HungerTier]),
                                                Mathf.Min(WarmthStamina[(int)WarmthTier], ShiverStaminaFactor)) *
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

        workLevel = Mathf.MoveTowards(workLevel, 0f, workFadePerSecond * Time.deltaTime);
        PassHours(Time.deltaTime * time.GameHoursPerRealSecond, player.State);
    }

    void FindPlayer()
    {
        if (player != null || Time.unscaledTime < nextPlayerSearch)
            return;

        nextPlayerSearch = Time.unscaledTime + 1f;
        player = FindAnyObjectByType<PlayerController>();
    }

    // A tool swing (PlayerController.TrySpendStamina): the player is working hard, so they're making work heat.
    public void NoteWork() => workLevel = Mathf.Min(1f, workLevel + workPerSwing);

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
        float tempC = WeatherManager.Instance != null ? WeatherManager.Instance.TemperatureC : 15f;

        // Body heat first: the heat the body makes (activity, shivering) sets how fast Hunger burns.
        StepBody(hours, tempC, activity);

        // Difficulty's survival severity scales every drain (DifficultyManager).
        float severity = DifficultyManager.DrainMultiplier;
        float hydrationRate = hydrationPerHour * severity * IllnessHydrationMultiplier;
        float hungerRate = hungerPerHour * severity * (1f + hungerPerExtraMet * Mathf.Max(0f, totalMet - 1f));

        if (activity == MovementState.Sprinting)
            hydrationRate *= sprintingHydration;
        else if (activity == MovementState.Walking || activity == MovementState.Crouching)
            hydrationRate *= walkingHydration;

        hydration = Mathf.Max(0f, hydration - hydrationRate * hours);
        hunger = Mathf.Max(0f, hunger - hungerRate * hours);

        float healthLoss = HealthLoss(HydrationTier, dehydrationSevereLoss, dehydrationEmptyLoss) +
                           HealthLoss(HungerTier, starvationSevereLoss, starvationEmptyLoss) +
                           ByLevel(0f, mildHealthLoss, severeHealthLoss) +
                           ColdLoss(WarmthTier) +
                           HeatLoss(HeatLevel);
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
            SetHealth(health + (hydration >= halfFed && hunger >= halfFed ? recoveryPerHour : lowRecoveryPerHour) * SleepRecovery * hours);
    }

    float ColdLoss(Tier tier) =>
        tier == Tier.Empty ? coldEmptyLoss : tier == Tier.Severe ? coldSevereLoss : tier == Tier.Reduced ? coldModerateLoss : 0f;

    // The hot side's Health cost (Weather_System.md's overheating tiers) is not built yet.
    static float HeatLoss(HeatStage stage) => 0f;

    // How hard the body is working, in multiples of resting heat (MET): sleeping, standing, moving, or a tool in hand.
    float ActivityMet(MovementState activity)
    {
        if (SleepManager.Instance != null && SleepManager.Instance.IsSleeping)
            return sleepMet;

        float met;
        switch (activity)
        {
            case MovementState.Sprinting: met = sprintMet; break;
            case MovementState.Walking:
            case MovementState.Jumping: met = walkMet; break;
            case MovementState.Crouching: met = crouchMet; break;
            default: met = idleMet; break;
        }
        InventoryManager inventory = InventoryManager.Instance;
        if (met > idleMet && inventory != null && inventory.MaxCarryWeightKg > 0f)
            met += carryMetBonus * Mathf.Clamp01(inventory.CarriedWeightKg / inventory.MaxCarryWeightKg);
        return Mathf.Max(met, 1f + (workMet - 1f) * workLevel);
    }

    // Core temperature, wetness and sweat-damp for a stretch of time, from the weather where the player stands and any fire nearby.
    void StepBody(float hours, float tempC, MovementState activity)
    {
        WeatherManager weather = WeatherManager.Instance;
        bool placed = player != null;
        Vector3 position = placed ? player.transform.position : Vector3.zero;

        // Under trees: most rain and some wind are kept off.
        float rainShelter = Mathf.Max(SleepRainShelter, placed ? OverheadCover.At(position + Vector3.up, 0.8f) : 0f);
        float windShelter = Mathf.Max(SleepWindShelter, placed ? OverheadCover.At(position + Vector3.up, 0.4f) : 0f);

        fireHeat = placed ? FireHeat(position) : 0f;

        float soak = 0f;
        if (weather != null && weather.IsPrecipitating)
        {
            float intensity = weather.IsSnowing ? 0.5f
                            : weather.PrecipitationWeather == WeatherType.LightRain ? 1f : 2f;
            soak = soakPerHour * intensity * (1f - rainShelter);
        }
        // Drying never stops, so a trickle past a tarp or canopy doesn't soak through; open rain outpaces it.
        wetness = Mathf.Clamp01(wetness + (soak - dryPerHour - fireDryPerHour * fireHeat) * hours);

        // The player dresses for the recent temperature, not the moment's.
        if (!smoothedReady)
        {
            smoothedTempC = tempC;
            smoothedReady = true;
        }
        else
        {
            smoothedTempC += (tempC - smoothedTempC) * (1f - Mathf.Exp(-hours / clothingSmoothHours));
        }

        float wind = weather != null ? weather.WindStrength : 0f;
        // A bed or shelter that "cuts the cold by X" is an added resistance that does exactly that against the reference.
        float cut = Mathf.Clamp(SleepInsulation, 0f, 0.95f);
        var conditions = new BodyHeat.Conditions
        {
            airC = tempC,
            windMetersPerSecond = wind * windMetersPerSecondAtFull * (1f - windShelter),
            clo = BodyHeat.Clo(smoothedTempC, winterClo, summerClo, clothingColdC, clothingWarmC),
            wetness = wetness,
            damp = damp,
            extraResistance = bedReferenceResistance * cut / (1f - cut),
            activityMet = ActivityMet(activity),
            radiantWatts = fireRadiantWatts * fireHeat,
            hunger = hunger,
            lossMultiplier = DifficultyManager.DrainMultiplier, // harmful direction only: it scales heat lost, never gained
        };
        BodyHeat.Result result = BodyHeat.Advance(body, coreC, conditions, hours);
        coreC = result.coreC;
        shiver = result.shiver;
        totalMet = result.totalMet;

        // Sweat: shedding work heat in cold air soaks the clothes; it clears by a fire, in shelter, slowly in the open,
        // and not at all in the rain.
        if (totalMet >= dampMinMet && coreC > dampAboveCoreC && tempC < dampBelowAirC)
        {
            damp += dampBuildPerHour * Mathf.Clamp01((coreC - dampAboveCoreC) / 0.3f) * hours;
        }
        else
        {
            bool inRain = weather != null && weather.IsPrecipitating && rainShelter < 0.5f;
            float clear = inRain ? 0f : fireHeat > 0.3f ? dampClearByFire : rainShelter >= 0.8f ? dampClearInShelter : dampClearInOpen;
            damp -= clear * hours;
        }
        damp = Mathf.Clamp01(damp);
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

    // Pulls core temperature into a band — for waking from a collapse (Player Collapse: no lower than 35 °C, no higher
    // than 37.5 °C). Nothing calls it yet.
    public void ClampCoreTemperature(float minC = 35f, float maxC = 37.5f) => coreC = Mathf.Clamp(coreC, minC, maxC);

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
        coreC = 37f;
        illnessHours = symptomHours = wetness = damp = shiver = workLevel = 0f;
        totalMet = 1f;
        smoothedReady = false;
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
        coreC = BodyHeat.WarmthToCore(Mathf.Clamp(newWarmth, 0f, MaxValue));
        wetness = Mathf.Clamp01(newWetness);
        if (newWarmth >= MaxValue && newWetness <= 0f)
            damp = 0f; // F9 dries the player off
    }
#endif

    public SurvivalSaveData CaptureState() => new SurvivalSaveData { health = health, hydration = hydration, hunger = hunger, illnessHours = illnessHours,
                                                                  chill = MaxValue - Warmth, wetness = wetness, coreTempC = coreC, damp = damp };

    public void RestoreState(SurvivalSaveData data)
    {
        hydration = Mathf.Clamp(data.hydration, 0f, MaxValue);
        hunger = Mathf.Clamp(data.hunger, 0f, MaxValue);
        health = Mathf.Clamp(data.health, 0f, MaxValue);
        illnessHours = Mathf.Clamp(data.illnessHours, 0f, maxIllnessHours);
        // A save from before the Core Temperature Model has Warmth only (as chill): read core temperature off that.
        coreC = data.coreTempC > 20f ? data.coreTempC : BodyHeat.WarmthToCore(Mathf.Clamp(MaxValue - data.chill, 0f, MaxValue));
        wetness = Mathf.Clamp01(data.wetness);
        damp = Mathf.Clamp01(data.damp);
        shiver = workLevel = 0f;
        totalMet = 1f;
        smoothedReady = false; // dress for the weather as it is on loading
        symptomHours = grumbleEveryHours * 0.5f;
        collapsed = health <= 0f;
    }

    // Save_Data_Model.md's Player Block: survival stats.
    string ISaveable.SaveFile => "player";
    string ISaveable.SaveKey => "survival";
    object ISaveable.CaptureState() => CaptureState();
    void ISaveable.RestoreState(string json) => RestoreState(JsonUtility.FromJson<SurvivalSaveData>(json));
}
