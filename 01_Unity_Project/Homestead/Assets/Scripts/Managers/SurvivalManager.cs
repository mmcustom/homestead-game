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
    // Player Collapse (Health_System.md): recent collapses and the day their forgiveness clock started, and the Weakened
    // state left after waking. Saves from before it load all of these as 0, which is no collapses and not weakened.
    public int collapseCount;
    public int collapseClockDay;
    public float weakenedHours;
    public float weakenedStamina;
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
//
// The hot side (Weather_System.md's Heat Inputs, Phase 1, 2026-10-14): the sun heats the player (solar exposure from the sun's
// height, the sky and any shade, up to about 270 W), and above 37 °C the body sweats, but sweat only cools as far as it can
// evaporate (wet-bulb temperature and wind; zero near 35 °C) and as far as Hydration lets the body make it. Core temperature
// above 37.5 °C is Hot, then heat exhaustion (38), severe (39) and heat stroke (40, where sweating fades rather than stops).
// The tiers cut stamina and, from severe, Health at the cold tiers' rates. Health reaching 0 from heat is the same Player
// Collapse as any other, with the cause remembered so SleepManager can wake the player at dusk or dawn. Every number is in the
// Inspector; all first-pass.
public class SurvivalManager : MonoBehaviour, ISaveable
{
    public const float MaxValue = 100f;

    public enum Sickness { None, Mild, Severe }

    // Core_Survival_System.md's Hydration effect tiers; Hunger mirrors them.
    public enum Tier { Fine, Minor, Reduced, Severe, Empty }

    // Overheating steps (Weather_System.md's Heat Inputs): Hot from 37.5 °C, heat exhaustion from 38, severe from 39,
    // heat stroke above 40.
    public enum HeatStage { None, Hot, Exhaustion, Severe, Stroke }

    // What was costing the most Health on the last tick that cost any: remembered when Health reaches 0 (CollapseCause).
    public enum LossCause { Unknown, Thirst, Hunger, Illness, Cold, Heat }

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

    [Header("Heat: the sun (Weather_System.md, Sun Load)")]
    [Tooltip("Heat gained by a standing, clothed player in full midday sun, in watts (a 1940s field study: about 270).")]
    [SerializeField, Min(0f)] float sunWattsFull = 270f;
    [Tooltip("What canopy shade still lets through at full sun (sky and ground radiation), in watts. A roof or wall lets none.")]
    [SerializeField, Min(0f)] float shadeWattsFull = 50f;
    [Tooltip("Sun elevation at midday per season (Spring, Summer, Fall, Winter), in degrees. Keep in step with DayNightCycle's " +
             "Midday Elevation: this computes the sun itself because DayNightCycle only updates per frame (stale in a sleep).")]
    [SerializeField] float[] sunMiddayElevation = { 53f, 76f, 53f, 30f };
    [Tooltip("Share of the direct sun a full tree canopy keeps off with leaves on, and with the leaves gone (Winter).")]
    [SerializeField, Range(0f, 1f)] float canopySunBlock = 0.85f;
    [SerializeField, Range(0f, 1f)] float bareCanopySunBlock = 0.15f;
    [Tooltip("In cold air the sun's gain is scaled down to this (heavier clothes, snow, a low sun), easing to full strength " +
             "between the two air temperatures. 1 gives a sunny winter noon the full gain.")]
    [SerializeField, Range(0f, 1f)] float sunColdScale = 0.4f;
    [SerializeField] float sunColdAirC = 0f;
    [SerializeField] float sunWarmAirC = 15f;
    [Tooltip("Lying down (asleep) exposes less of the body to the sun: the sun gain is scaled by this.")]
    [SerializeField, Range(0f, 1f)] float sunLyingScale = 0.4f;
    [Tooltip("How quickly the sun's cloud dimming eases toward a new weather's, per in-game hour (as DayNightCycle does).")]
    [SerializeField, Min(0.01f)] float sunlightBlendPerHour = 2f;

    [Header("Heat: sweat (Weather_System.md, Humidity and the overheating tiers)")]
    [Tooltip("Wet-bulb temperature (°C) at or below which sweat evaporates fully, and at or above which it cools nothing (the " +
             "physical survival limit, about 35).")]
    [SerializeField] float evaporationFullWetBulbC = 17f;
    [SerializeField] float evaporationZeroWetBulbC = 35f;
    [Tooltip("Wind raises the evaporation factor by up to this share at full wind (never lifts it from zero).")]
    [SerializeField, Min(0f)] float windEvaporationBoost = 0.4f;
    [Tooltip("Hydration at or above which the body can sweat fully, and the share of sweat left at Hydration 0.")]
    [SerializeField, Min(1f)] float sweatFullHydration = 50f;
    [SerializeField, Range(0f, 1f)] float sweatSupplyAtEmpty = 0.2f;
    [Tooltip("Hydration points lost per hour for each 1000 W of sweat shed, on top of the normal drain (a litre an hour is about 700 W).")]
    [SerializeField, Min(0f)] float sweatHydrationPerKilowatt = 12f;
    [Tooltip("Sweat only costs extra Hydration in air warmer than this, so working up a sweat in the cold keeps its old cost.")]
    [SerializeField] float sweatHydrationAboveAirC = 18f;

    [Header("Heat: overheating tiers (Hot, Heat exhaustion, Severe, Heat stroke)")]
    [Tooltip("Stamina multiplier per stage: None, Hot, Heat exhaustion, Severe, Heat stroke (it joins the other factors as the lowest).")]
    [SerializeField] float[] heatStamina = { 1f, 1f, 0.7f, 0.5f, 0.4f };
    [Tooltip("Health lost per in-game hour at severe heat exhaustion and at heat stroke (the cold tiers' Moderate and Severe rates).")]
    [SerializeField, Min(0f)] float heatSevereLoss = 1f;
    [SerializeField, Min(0f)] float heatStrokeLoss = 4f;

    [Header("Sleeping in the heat")]
    [Tooltip("A Sleeping Bag's own insulation fades out between these two air temperatures (°C): ignored above the second.")]
    [SerializeField] float bagIgnoredFromC = 20f;
    [SerializeField] float bagIgnoredAtC = 24f;

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

    [Header("Rewarming (game feel: speeds the rise in core temperature, never the fall)")]
    [Tooltip("Multiplies the body's net heat gain while core temperature is below the ceiling below. Resting, sleeping and " +
             "crouching are x1. A campfire's radiant heat is not multiplied, and heat loss never is. Health_System.md, Warmth Regeneration.")]
    [SerializeField, Min(1f)] float rewarmWalking = 1.5f;
    [SerializeField, Min(1f)] float rewarmWorking = 2f;
    [SerializeField, Min(1f)] float rewarmSprinting = 2.5f;
    [SerializeField, Min(1f)] float rewarmCrouching = 1f;
    [Tooltip("The boost stops at this core temperature (°C), so it can never push the player into Overheating.")]
    [SerializeField] float rewarmCeilingC = 37f;

    [Header("Shivering and sweat")]
    [Tooltip("Core temperature (°C) at which shivering starts to SHOW — the HUD word and the Shiver sound. 35.5 is about Warmth 58. " +
             "The shivering that makes heat still starts at Body > Shiver Start C, so this changes only when the player is told.")]
    [SerializeField] float visibleShiverOnsetC = 35.5f;
    [Tooltip("Core temperature (°C) at which the shown shivering is at full strength. 34.5 is about Warmth 46.")]
    [SerializeField] float visibleShiverFullC = 34.5f;
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
    float shiver;        // 0-1, as of the last step: the heat-making ramp (stamina, hunger, work heat)
    float visibleShiver; // 0-1: the ramp the player sees and hears (visibleShiverOnsetC down to visibleShiverFullC)
    float totalMet = 1f; // what the body produced last step, in multiples of resting heat
    float workLevel;     // 0-1, raised by tool swings, fades in seconds
    float smoothedTempC; // recent air temperature, for the seasonal clothing
    bool smoothedReady;
    float fireHeat;      // 0-1, how close to a lit fire the player was last step
    float symptomHours; // in-game hours until the next grumble or vomit
    bool collapsed;
    int collapseCount;      // recent collapses: +1 each, -1 per 3 collapse-free in-game days (RegisterCollapse)
    int collapseClockDay;   // the in-game day of the last collapse, where those free days are counted from
    float weakenedHours;    // in-game hours of Weakened left after waking from a collapse
    float weakenedStamina = 1f;
    PlayerController player;
    float nextPlayerSearch;
    // The hot side (nothing here is saved: it all follows from the clock, the weather and where the player stands).
    float sunlight = 1f;        // the sky's dimming of the sun, eased toward the weather's
    bool sunlightReady;
    float solarWatts;           // sun heat on the player as of the last step
    float evaporation;          // 0-1, how well sweat cools as of the last step
    float sweatWatts;           // heat shed by sweating as of the last step
    float sunCover;             // cached: share of the direct sun kept off by trees (OverheadCover.SunBlocked)
    bool sunRoofed;             // cached: a roof or wall on the line to the sun
    bool sunCoverReady;
    Vector3 sunCoverAt;
    long sunCoverKey;
    float sunCoverTime;
    LossCause lastLossCause;    // what cost the most Health on the last tick that cost any
    LossCause collapseCause;    // what it was when Health reached 0

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
    // What the HUD and Shiver.wav read: the visible ramp, which starts lower than the shivering that makes heat.
    public float ShiverIntensity => visibleShiver;
    public bool IsShivering => visibleShiver > 0.05f;
    // Hard work is making real heat (chopping and digging are about 5 MET) without the body having to shiver for it.
    public bool IsWorkingUpHeat => totalMet >= 4f && shiver <= 0.05f;
    public HeatStage HeatLevel => coreC > 40f ? HeatStage.Stroke : coreC > 39f ? HeatStage.Severe
                                : coreC > 38f ? HeatStage.Exhaustion : coreC > 37.5f ? HeatStage.Hot : HeatStage.None;
    // For the HUD and debugging: sun heat on the player (W), how well sweat cools (0-1), heat shed by sweat (W).
    public float SolarWatts => solarWatts;
    public float SweatEvaporation => evaporation;
    public float SweatWatts => sweatWatts;
    // What was draining Health when the player last collapsed (SleepManager wakes a heat collapse at dusk or dawn).
    public LossCause CollapseCause => collapseCause;
    // Severe cold slows movement.
    public float MovementMultiplier => WarmthMovement[(int)WarmthTier];
    public bool IsSick => illnessHours > 0f;
    public bool IsWeakened => weakenedHours > 0f;
    public float WeakenedHoursLeft => weakenedHours;
    public int RecentCollapses => collapseCount;

    // Set by SleepManager while the player sleeps (Building_Housing_System.md's Sleep System): how much of the cold the
    // bedding and shelter keep off (0-1, cutting heat loss), how much rain and wind the shelter blocks, and how much
    // faster Health recovers at rest. Back to nothing when they wake.
    public float SleepInsulation { get; set; }
    // The same without the Sleeping Bag's share: a bag is dropped from the sum in warm air (Bag Ignored From C).
    public float SleepShelterInsulation { get; set; }
    // Also what a shelter keeps off the sun while the player sleeps in it (a Tent or Cabin keeps off all of it).
    public float SleepRainShelter { get; set; }
    public float SleepWindShelter { get; set; }
    public float SleepRecovery { get; set; } = 1f;
    public Sickness SicknessLevel => illnessHours <= 0f ? Sickness.None
                                   : illnessHours > illnessHoursPerBout ? Sickness.Severe : Sickness.Mild;
    public float IllnessHoursLeft => illnessHours;

    public Tier HydrationTier => TierOf(hydration);
    public Tier HungerTier => TierOf(hunger);

    // Shivering saps stamina, down to this at full shivering. It joins the other factors as the lowest of them, so it
    // never stacks on top of the Warmth tier's own penalty. Heat exhaustion's penalty joins the same way.
    float ShiverStaminaFactor => Mathf.Lerp(1f, shiverStamina, visibleShiver);

    float HeatStaminaFactor => heatStamina != null && heatStamina.Length > (int)HeatLevel ? heatStamina[(int)HeatLevel] : 1f;

    // Multiplies the player's maximum stamina and recovery rate.
    public float StaminaMultiplier => Mathf.Min(Mathf.Min(HydrationStamina[(int)HydrationTier], HungerStamina[(int)HungerTier]),
                                                Mathf.Min(Mathf.Min(WarmthStamina[(int)WarmthTier], ShiverStaminaFactor), HeatStaminaFactor)) *
                                      ByLevel(1f, mildStamina, severeStamina) * (IsWeakened ? weakenedStamina : 1f);

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

        // Sweat costs water: every 1000 W shed takes some Hydration, but only in warm air (cold work keeps its old cost).
        if (tempC > sweatHydrationAboveAirC)
            hydrationRate += sweatHydrationPerKilowatt * severity * sweatWatts / 1000f;

        hydration = Mathf.Max(0f, hydration - hydrationRate * hours);
        hunger = Mathf.Max(0f, hunger - hungerRate * hours);

        float thirstLoss = HealthLoss(HydrationTier, dehydrationSevereLoss, dehydrationEmptyLoss);
        float hungerLoss = HealthLoss(HungerTier, starvationSevereLoss, starvationEmptyLoss);
        float illnessLoss = ByLevel(0f, mildHealthLoss, severeHealthLoss);
        float coldLoss = ColdLoss(WarmthTier);
        float heatLoss = HeatLoss(HeatLevel);
        float healthLoss = thirstLoss + hungerLoss + illnessLoss + coldLoss + heatLoss;
        if (healthLoss > 0f)
        {
            float top = Mathf.Max(Mathf.Max(thirstLoss, hungerLoss), Mathf.Max(illnessLoss, Mathf.Max(coldLoss, heatLoss)));
            lastLossCause = top == heatLoss ? LossCause.Heat : top == coldLoss ? LossCause.Cold : top == thirstLoss ? LossCause.Thirst
                          : top == hungerLoss ? LossCause.Hunger : LossCause.Illness;
        }
        if (weakenedHours > 0f)
            weakenedHours = Mathf.Max(0f, weakenedHours - hours);
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

    // The hot side's Health cost (Weather_System.md's overheating tiers), at the cold tiers' rates for matching severity: none
    // for Hot and heat exhaustion, slow at severe heat exhaustion, steady at heat stroke.
    float HeatLoss(HeatStage stage) => stage == HeatStage.Stroke ? heatStrokeLoss : stage == HeatStage.Severe ? heatSevereLoss : 0f;

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

    // The rewarming factor for what the body is doing: the highest of the moving factor and the tool-work factor
    // (the work factor eases in with the working level, as the work MET does). Asleep is x1.
    float RewarmFactor(MovementState activity)
    {
        if (SleepManager.Instance != null && SleepManager.Instance.IsSleeping)
            return 1f;
        float moving;
        switch (activity)
        {
            case MovementState.Sprinting: moving = rewarmSprinting; break;
            case MovementState.Walking:
            case MovementState.Jumping: moving = rewarmWalking; break;
            case MovementState.Crouching: moving = rewarmCrouching; break;
            default: moving = 1f; break;
        }
        return Mathf.Max(moving, 1f + (rewarmWorking - 1f) * workLevel);
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
        // A bed or shelter that "cuts the cold by X" is an added resistance that does exactly that against the reference. It
        // adds to the dry skin-to-air exchange only, never to sweat evaporation (dry heat only); and a Sleeping Bag's own share
        // is dropped as the air warms past the Bag Ignored From C figure, so a hot night isn't made worse by sleeping in a bag.
        float bagIgnored = Mathf.InverseLerp(bagIgnoredFromC, bagIgnoredAtC, tempC);
        float cut = Mathf.Clamp(Mathf.Lerp(SleepInsulation, Mathf.Min(SleepInsulation, SleepShelterInsulation), bagIgnored), 0f, 0.95f);

        // The hot side: how well sweat cools (wet-bulb from the air and its humidity, and wind), how much the body can make
        // (Hydration), and the sun on the player.
        float humidity = weather != null ? weather.RelativeHumidity : 50f;
        evaporation = BodyHeat.EvaporationFactor(BodyHeat.WetBulbC(tempC, humidity), evaporationFullWetBulbC, evaporationZeroWetBulbC,
                                                 wind * (1f - windShelter), windEvaporationBoost);
        float sweatSupply = Mathf.Lerp(sweatSupplyAtEmpty, 1f, Mathf.Clamp01(hydration / sweatFullHydration));
        solarWatts = SunWatts(position, placed, hours, tempC);

        var conditions = new BodyHeat.Conditions
        {
            airC = tempC,
            solarWatts = solarWatts,
            sweatEfficiency = evaporation * sweatSupply,
            windMetersPerSecond = wind * windMetersPerSecondAtFull * (1f - windShelter),
            clo = BodyHeat.Clo(smoothedTempC, winterClo, summerClo, clothingColdC, clothingWarmC),
            wetness = wetness,
            damp = damp,
            extraResistance = bedReferenceResistance * cut / (1f - cut),
            activityMet = ActivityMet(activity),
            radiantWatts = fireRadiantWatts * fireHeat,
            hunger = hunger,
            lossMultiplier = DifficultyManager.DrainMultiplier, // harmful direction only: it scales heat lost, never gained
            rewarmMultiplier = RewarmFactor(activity),
            rewarmBelowC = rewarmCeilingC,
        };
        BodyHeat.Result result = BodyHeat.Advance(body, coreC, conditions, hours);
        coreC = result.coreC;
        sweatWatts = result.sweatWatts;
        shiver = result.shiver;
        visibleShiver = BodyHeat.VisibleShiver(body, coreC, hunger, visibleShiverOnsetC, visibleShiverFullC);
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

    // The sun's height and bearing now, from the clock, the season and the day's sunrise and sunset (the same path as
    // DayNightCycle's, but computed here so it is right during a sleep or collapse skip). Elevation is negative at night.
    void SunNow(TimeManager time, out float elevation, out float azimuth)
    {
        float hour = time.HourOfDay, sunrise = time.SunriseHour, sunset = time.SunsetHour;
        int season = (int)time.CurrentSeason;
        float progress = time.SeasonProgress;
        int neighbour = progress < 0.5f ? (season + 3) % 4 : (season + 1) % 4;
        float noon = sunMiddayElevation != null && sunMiddayElevation.Length == 4
            ? Mathf.Lerp(sunMiddayElevation[season], sunMiddayElevation[neighbour], Mathf.Abs(progress - 0.5f)) : 50f;

        if (hour >= sunrise && hour < sunset)
        {
            float t = (hour - sunrise) / Mathf.Max(0.01f, sunset - sunrise);
            elevation = noon * Mathf.Sin(Mathf.PI * t);
            azimuth = 90f + 180f * t;
        }
        else
        {
            elevation = -1f;
            azimuth = 0f;
        }
    }

    // Heat gained from the sun, in watts: full sun × the sun's height (the sine of its elevation) × the sky's clearness, with
    // trees and structures keeping off a share of the direct part. Under a roof or wall none reaches the player; in canopy
    // shade a little sky and ground radiation still does.
    float SunWatts(Vector3 position, bool placed, float hours, float airC)
    {
        TimeManager time = TimeManager.Instance;
        WeatherManager weather = WeatherManager.Instance;
        if (time == null || !placed || sunWattsFull <= 0f)
            return 0f;

        SunNow(time, out float elevation, out float azimuth);
        if (elevation <= 0.5f)
            return 0f;

        // The sky's dimming eases in over a couple of hours, as the visible light does, whatever the speed of the clock.
        float target = DayNightCycle.SunlightFor(weather != null ? weather.Current : WeatherType.Clear);
        sunlight = sunlightReady ? Mathf.MoveTowards(sunlight, target, sunlightBlendPerHour * hours) : target;
        sunlightReady = true;

        // The probe is not cheap, so it is repeated only when the player has moved, the sun has moved on, or half a second has passed.
        Vector3 head = position + Vector3.up * 1.1f;
        long key = (long)(time.TotalHours * 2.0);
        if (!sunCoverReady || (head - sunCoverAt).sqrMagnitude > 0.5f || key != sunCoverKey || Time.unscaledTime - sunCoverTime > 0.5f)
        {
            sunCover = OverheadCover.SunBlocked(head, elevation, azimuth, SeasonalTrees.LeafAmount, canopySunBlock, bareCanopySunBlock,
                                                out sunRoofed);
            sunCoverReady = true;
            sunCoverAt = head;
            sunCoverKey = key;
            sunCoverTime = Time.unscaledTime;
        }

        // A Tent, Lean-To, Tarp or Cabin the player is sleeping in keeps off the sun as it does the rain.
        float cover = Mathf.Max(sunCover, SleepRainShelter);
        bool roofed = sunRoofed || SleepRainShelter >= 0.99f;

        float strength = Mathf.Sin(elevation * Mathf.Deg2Rad) * sunlight;
        float watts = strength * (sunWattsFull * (1f - cover) + (roofed ? 0f : shadeWattsFull * cover));
        float coldScale = Mathf.Lerp(sunColdScale, 1f, Mathf.InverseLerp(sunColdAirC, sunWarmAirC, airC));
        float posture = SleepManager.Instance != null && SleepManager.Instance.IsSleeping ? sunLyingScale : 1f;
        return watts * coldScale * posture;
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
    // than 37.5 °C). WakeFromCollapse does its own version of this, a hair above the floor.
    public void ClampCoreTemperature(float minC = 35f, float maxC = 37.5f) => coreC = Mathf.Clamp(coreC, minC, maxC);

    // --- Player Collapse (Health_System.md, "wake weakened") ---
    // Health reaching 0 raises Collapsed whatever the cause; SleepManager runs the fade, the skipped hours and the waking
    // (CollapseRoutine), asking this for the numbers and to apply the waking state.

    public struct CollapsePlan
    {
        public int count;              // recent collapses, including this one
        public float skipHours;        // in-game hours the world runs on while the player is out
        public float weakenedHours;    // how long the Weakened state lasts after waking
        public float staminaMultiplier;
        public float wakeHealth;
        public bool dropsItem;         // 4th and later: one carried item is left where the player fell
    }

    // The scale for the Nth recent collapse (Health_System.md's "Repeated collapses"; 4 and up share the last row).
    public static CollapsePlan PlanFor(int count)
    {
        switch (Mathf.Max(1, count))
        {
            case 1: return new CollapsePlan { count = count, skipHours = 8f, weakenedHours = 4f, staminaMultiplier = 0.7f, wakeHealth = 20f };
            case 2: return new CollapsePlan { count = count, skipHours = 12f, weakenedHours = 6f, staminaMultiplier = 0.6f, wakeHealth = 15f };
            case 3: return new CollapsePlan { count = count, skipHours = 16f, weakenedHours = 8f, staminaMultiplier = 0.5f, wakeHealth = 10f };
            default: return new CollapsePlan { count = count, skipHours = 16f, weakenedHours = 12f, staminaMultiplier = 0.5f, wakeHealth = 10f, dropsItem = true };
        }
    }

    // Counts a collapse on the given in-game day and returns how many are recent: the count falls by 1 for every 3
    // collapse-free days since the last one, then this one adds 1.
    public int RegisterCollapse(int today)
    {
        int free = Mathf.Max(0, (today - collapseClockDay) / 3);
        collapseCount = Mathf.Max(0, collapseCount - free) + 1;
        collapseClockDay = today;
        return collapseCount;
    }

    // The waking state: wet clothes dry, sickness gone, Hunger, Hydration and Warmth lifted to at least 50 (never lowered),
    // core temperature clamped into 35-37.5 °C (above that, back to 37.2: 37.5 is exactly where Hot starts, so the player
    // would wake one tick from sweating), Health set by the plan, and the Weakened timer started. Hunger, Hydration and the
    // rest did not tick while the player was out.
    public void WakeFromCollapse(CollapsePlan plan)
    {
        wetness = 0f;
        Cure();
        hydration = Mathf.Max(hydration, 50f);
        hunger = Mathf.Max(hunger, 50f);
        // Warmth 50 is 35.0 °C exactly, the edge of the Reduced tier; a hair over keeps it clear of the cost to Health.
        coreC = coreC > 37.5f ? 37.2f : Mathf.Max(coreC, BodyHeat.WarmthToCore(50f) + 0.05f);
        shiver = visibleShiver = workLevel = 0f;
        sweatWatts = solarWatts = 0f;
        weakenedHours = plan.weakenedHours;
        weakenedStamina = plan.staminaMultiplier;
        SetHealth(plan.wakeHealth);
    }

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
            collapseCause = lastLossCause;
            Debug.Log($"[Survival] Player collapsed (Health 0, cause {collapseCause}): SleepManager handles the wake-weakened sequence.");
            Collapsed?.Invoke();
        }
    }

    // Everything full — used when starting a new game.
    public void ResetStats()
    {
        health = hydration = hunger = MaxValue;
        coreC = 37f;
        illnessHours = symptomHours = wetness = damp = shiver = visibleShiver = workLevel = 0f;
        totalMet = 1f;
        smoothedReady = false;
        sunlightReady = sunCoverReady = false;
        sweatWatts = solarWatts = 0f;
        lastLossCause = collapseCause = LossCause.Unknown;
        collapsed = false;
        collapseCount = collapseClockDay = 0;
        weakenedHours = 0f;
        weakenedStamina = 1f;
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    // Testing only (TimeDebugControls).
    public void DebugSet(float newHydration, float newHunger, float newHealth)
    {
        hydration = Mathf.Clamp(newHydration, 0f, MaxValue);
        hunger = Mathf.Clamp(newHunger, 0f, MaxValue);
        lastLossCause = LossCause.Unknown; // a debug collapse has no cause
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
                                                                  chill = MaxValue - Warmth, wetness = wetness, coreTempC = coreC, damp = damp,
                                                                  collapseCount = collapseCount, collapseClockDay = collapseClockDay,
                                                                  weakenedHours = weakenedHours, weakenedStamina = weakenedStamina };

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
        shiver = visibleShiver = workLevel = 0f;
        totalMet = 1f;
        smoothedReady = false; // dress for the weather as it is on loading
        sunlightReady = sunCoverReady = false;
        sweatWatts = solarWatts = 0f;
        lastLossCause = collapseCause = LossCause.Unknown;
        symptomHours = grumbleEveryHours * 0.5f;
        collapsed = health <= 0f;
        collapseCount = Mathf.Max(0, data.collapseCount);
        collapseClockDay = Mathf.Max(0, data.collapseClockDay);
        weakenedHours = Mathf.Max(0f, data.weakenedHours);
        weakenedStamina = weakenedHours > 0f ? Mathf.Clamp(data.weakenedStamina, 0.1f, 1f) : 1f;
    }

    // Save_Data_Model.md's Player Block: survival stats.
    string ISaveable.SaveFile => "player";
    string ISaveable.SaveKey => "survival";
    object ISaveable.CaptureState() => CaptureState();
    void ISaveable.RestoreState(string json) => RestoreState(JsonUtility.FromJson<SurvivalSaveData>(json));
}
