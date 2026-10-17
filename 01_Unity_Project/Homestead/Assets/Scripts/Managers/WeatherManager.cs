using System;
using UnityEngine;

// Weather_System.md's weather types. Wind also has a standing direction/strength (see WindDirection),
// since Hunting_System.md uses wind direction regardless of the current weather type.
// HeatWave is appended, never inserted: saves store the type as an integer.
public enum WeatherType { Clear, Cloudy, LightRain, HeavyRain, Thunderstorm, ColdFront, Snow, Wind, HeatWave }

[Serializable]
public struct WeatherSaveData
{
    public WeatherType type;
    public int hoursRemaining;
    public float temperatureC;
    public float dailyTemperatureOffset;
    public float windDirection;
    public int dryDays;
    public bool rainedToday;
    public int seed;
    // Heat Waves (Weather_System.md's Heat Inputs): this wave's peak strength and full length, and the hours of humidity left
    // from recent rain. Saves from before them load 0: no wave, no recent rain.
    public float heatWaveStrengthC;
    public int heatWaveTotalHours;
    public float rainRecentHours;
}

// Weather, temperature, seasonal conditions (Unity_Architecture.md, Weather_System.md).
// Driven entirely by TimeManager's hour/day/season events, so weather stops whenever the clock stops.
public class WeatherManager : MonoBehaviour, ISaveable
{
    [Serializable]
    public class SeasonClimate
    {
        [Tooltip("Relative share of the season's time spent in each weather type, in WeatherType order: " +
                 "Clear, Cloudy, LightRain, HeavyRain, Thunderstorm, ColdFront, Snow, Wind, HeatWave. " +
                 "Roll chances are adjusted for each type's duration, so long types don't crowd out short ones.")]
        public float[] weights = new float[WeatherTypeCount];

        [Tooltip("Typical daily high and low in °C before weather and day-to-day variation.")]
        public float highC;
        public float lowC;

        [Tooltip("Baseline wind strength, 0–1.")]
        [Range(0f, 1f)] public float windStrength;
    }

    [Serializable]
    public class WeatherTypeSettings
    {
        [Min(1)] public int minHours = 1;
        [Min(1)] public int maxHours = 1;

        [Tooltip("Added to the temperature while this weather is active, in °C.")]
        public float temperatureOffsetC;

        [Tooltip("Wind strength while this weather is active never drops below this, 0–1.")]
        [Range(0f, 1f)] public float minWindStrength;
    }

    const int WeatherTypeCount = 9;
    const int SeasonCount = 4;
    const float PeakTemperatureHour = 15f;

    public static WeatherManager Instance { get; private set; }

    // All values below are tuning defaults — Weather_System.md describes seasonal tendencies but no numbers.
    [Header("Climate per season (Spring, Summer, Fall, Winter)")]
    [SerializeField] SeasonClimate[] seasons =
    {
        // Spring: frequent rain, unpredictable conditions.
        new SeasonClimate { weights = new float[] { 30, 30, 22, 10, 3, 2, 0, 3, 0 }, highC = 18f, lowC = 5f, windStrength = 0.3f },
        // Summer: heat, occasional thunderstorms, drought risk in dry stretches. Heat Wave 15 (Clear came down by the same 15)
        // is about one a Summer: weights are shares of time, and a wave averages 108 h of a 28-day (672 h) Summer.
        new SeasonClimate { weights = new float[] { 49, 25, 3, 1, 2, 0, 0, 5, 15 }, highC = 30f, lowC = 18f, windStrength = 0.2f },
        // Fall: cooling trends, increasing wind.
        new SeasonClimate { weights = new float[] { 40, 28, 10, 4, 1, 7, 0, 10, 0 }, highC = 17f, lowC = 5f, windStrength = 0.4f },
        // Winter: snow and cold fronts dominate.
        new SeasonClimate { weights = new float[] { 20, 25, 3, 0, 0, 15, 30, 7, 0 }, highC = 5f, lowC = -6f, windStrength = 0.35f },
    };

    [Header("Weather types (WeatherType order)")]
    [SerializeField] WeatherTypeSettings[] weatherTypes =
    {
        new WeatherTypeSettings { minHours = 6, maxHours = 24, temperatureOffsetC = 0f, minWindStrength = 0f },     // Clear
        new WeatherTypeSettings { minHours = 4, maxHours = 16, temperatureOffsetC = -1f, minWindStrength = 0f },    // Cloudy
        new WeatherTypeSettings { minHours = 2, maxHours = 8, temperatureOffsetC = -2f, minWindStrength = 0.2f },   // LightRain
        new WeatherTypeSettings { minHours = 2, maxHours = 6, temperatureOffsetC = -3f, minWindStrength = 0.4f },   // HeavyRain
        new WeatherTypeSettings { minHours = 1, maxHours = 3, temperatureOffsetC = -3f, minWindStrength = 0.7f },   // Thunderstorm
        new WeatherTypeSettings { minHours = 24, maxHours = 48, temperatureOffsetC = -8f, minWindStrength = 0.5f },  // ColdFront
        new WeatherTypeSettings { minHours = 4, maxHours = 18, temperatureOffsetC = -2f, minWindStrength = 0.2f },  // Snow
        new WeatherTypeSettings { minHours = 4, maxHours = 12, temperatureOffsetC = -1f, minWindStrength = 0.8f },  // Wind
        // HeatWave: the heat comes from the per-wave strength and envelope under "Heat Waves", so this constant stays 0.
        new WeatherTypeSettings { minHours = 72, maxHours = 144, temperatureOffsetC = 0f, minWindStrength = 0f },   // HeatWave
    };

    [Header("Heat Waves (Summer only; weights above)")]
    [Tooltip("An ordinary wave's peak rise over the day's normal, in °C, plus or minus the variation. No design ceiling.")]
    [SerializeField] float heatWaveBaseC = 6f;
    [SerializeField, Min(0f)] float heatWaveVariationC = 1.5f;
    [Tooltip("Chance that a wave is a strong one, and the extra °C it adds (a random figure between the two).")]
    [SerializeField, Range(0f, 1f)] float heatWaveStrongChance = 0.2f;
    [SerializeField] Vector2 heatWaveStrongExtraC = new Vector2(4f, 10f);
    [Tooltip("In-game hours a wave takes to build to its peak, and to ease off at its end.")]
    [SerializeField, Min(1f)] float heatWaveBuildHours = 24f;
    [SerializeField, Min(1f)] float heatWaveEaseHours = 24f;
    [Tooltip("Warm nights: at the peak, the daily swing between the low and the high shrinks by this share (0.5 halves the night cooling).")]
    [SerializeField, Range(0f, 1f)] float heatWaveNightCoolingCut = 0.5f;
    [Tooltip("Whether the dry days of a wave count toward a Summer drought (nothing reads IsDrought yet; ponds don't dry).")]
    [SerializeField] bool heatWaveCountsAsDry;

    [Tooltip("When a wave runs out its natural length (not a debug end, not the end of Summer), the next weather is forced: this " +
             "chance of a Thunderstorm, otherwise a Cold Front, whatever the season's weights say.")]
    [SerializeField, Range(0f, 1f)] float heatWaveReliefThunderstormChance = 0.7f;

    [Header("Humidity (dew point, derived to relative humidity from the air temperature)")]
    [Tooltip("The typical dew point in °C at midseason, Spring, Summer, Fall, Winter; blended toward the neighbouring season like the temperature.")]
    [SerializeField] float[] seasonDewPointC = { 6f, 17f, 7f, -4f };
    [SerializeField, Min(0f)] float dailyDewPointVariationC = 3f;
    [Tooltip("Added to the dew point while each weather type is active, in WeatherType order.")]
    [SerializeField] float[] weatherDewPointC = { 0f, 1.5f, 3f, 4f, 4f, -4f, 0f, -2f, 0f };
    [Tooltip("While precipitation falls the air is nearly saturated: the dew point sits this far under the air temperature.")]
    [SerializeField, Min(0f)] float precipitationDewDepressionC = 1f;
    [Tooltip("After rain the air stays humid: this much extra dew point, falling to nothing over the hours below.")]
    [SerializeField] float rainRecentDewPointC = 3f;
    [SerializeField, Min(1f)] float rainRecentHoursMax = 24f;
    [Tooltip("A Heat Wave raises the dew point by this much at its ordinary and its strongest (+12 °C) strength.")]
    [SerializeField] Vector2 heatWaveDewPointC = new Vector2(3f, 5f);
    [Tooltip("The dew point never gets closer than this to the air temperature outside precipitation.")]
    [SerializeField, Min(0f)] float minDewDepressionC = 0.5f;

    [Header("Variation")]
    [Tooltip("Each day's temperature shifts by up to ± this many °C, so no two years feel identical.")]
    [SerializeField, Min(0f)] float dailyTemperatureVariationC = 4f;
    [Tooltip("How far the prevailing wind direction can swing from one day to the next, in degrees.")]
    [SerializeField, Range(0f, 180f)] float dailyWindShiftDegrees = 45f;
    [Tooltip("Fastest the temperature can move toward its target, in °C per in-game hour — " +
             "a cold front arrives over a few hours rather than instantly.")]
    [SerializeField, Min(0.1f)] float maxTemperatureChangePerHour = 3f;
    [Tooltip("A code guard, not a design limit (real extremes run higher): nothing, Homesteader's offset included, " +
             "takes the air above this. Applied last.")]
    [SerializeField] float maxTemperatureC = 46f;

    [Header("Difficulty temperature offset")]
    [Tooltip("Difficulty's ±°C offset applies in full (colder for Pioneer) at or below this temperature...")]
    [SerializeField] float offsetColdBelowC = 14f;
    [Tooltip("...and reversed (hotter for Pioneer) at or above this one, easing through zero in between.")]
    [SerializeField] float offsetHotAboveC = 26f;

    [Header("Late Fall snow")]
    [Tooltip("Weather_System.md: light snow can occasionally occur late in Fall. Snow's share of the time " +
             "(same scale as the season weights) once Fall is this far through.")]
    [SerializeField, Min(0f)] float lateFallSnowShare = 4f;
    [SerializeField, Range(0f, 1f)] float lateFallStartsAt = 0.75f;

    [Header("Drought")]
    [Tooltip("Consecutive Summer days without rain before a drought sets in (Season_System.md, Water_System.md).")]
    [SerializeField, Min(1)] int droughtDryDays = 5;

    WeatherType current;
    int hoursRemaining;
    float temperature;
    float dailyTemperatureOffset;
    float windDirection;
    int dryDays;
    bool rainedToday;
    int seed;
    bool subscribed;
    float heatWaveStrengthC;   // this wave's peak rise in °C (0 outside a wave)
    int heatWaveTotalHours;    // this wave's full length
    float rainRecentHours;     // hours of humid air left from recent rain
    int dewDay = -1;
    float dewDayVariation;

    public event Action<WeatherType> WeatherChanged;

    public WeatherType Current => current;
    public int HoursRemaining => hoursRemaining;

    // What actually falls from the sky. Same as Current, except a Thunderstorm below freezing snows (thundersnow) —
    // it stays a Thunderstorm for lightning and lighting, but rain visuals, sound and snow buildup should read this.
    public WeatherType PrecipitationWeather =>
        current == WeatherType.Thunderstorm && temperature <= 0f ? WeatherType.Snow : current;

    public bool IsRaining => PrecipitationWeather == WeatherType.LightRain || PrecipitationWeather == WeatherType.HeavyRain ||
                             PrecipitationWeather == WeatherType.Thunderstorm;
    public bool IsSnowing => PrecipitationWeather == WeatherType.Snow;
    public bool IsPrecipitating => IsRaining || IsSnowing;

    // Weather_System.md's Exposure Connection: these contribute to exposure risk without adequate shelter.
    public bool IsSevere => current == WeatherType.Thunderstorm || current == WeatherType.ColdFront || current == WeatherType.Snow;

    public bool IsDrought => TimeManager.Instance != null &&
                             TimeManager.Instance.CurrentSeason == Season.Summer &&
                             dryDays >= droughtDryDays;
    public int DryDays => dryDays;

    public bool IsHeatWave => current == WeatherType.HeatWave;
    // 0-1: how far into its build and ease-off the current wave is (0 outside a wave).
    public float HeatWaveIntensity
    {
        get
        {
            if (current != WeatherType.HeatWave || heatWaveTotalHours <= 0)
                return 0f;
            float elapsed = heatWaveTotalHours - hoursRemaining;
            float t = Mathf.Min(Mathf.Clamp01((elapsed + 1f) / heatWaveBuildHours), Mathf.Clamp01(hoursRemaining / heatWaveEaseHours));
            return t * t * (3f - 2f * t);
        }
    }
    public float HeatWaveRiseC => heatWaveStrengthC * HeatWaveIntensity;

    // Humidity (Weather_System.md's Heat Inputs): a dew point for the day and hour, from the season, the day's random shift,
    // the weather, recent rain and any Heat Wave; relative humidity then follows from the air temperature (Magnus), so it
    // is lowest in the afternoon and highest near dawn without any extra curve.
    public float DewPointC
    {
        get
        {
            TimeManager time = TimeManager.Instance;
            if (time == null)
                return temperature - 5f;

            int season = (int)time.CurrentSeason;
            float progress = time.SeasonProgress;
            int neighbour = progress < 0.5f ? (season + SeasonCount - 1) % SeasonCount : (season + 1) % SeasonCount;
            float dew = Mathf.Lerp(seasonDewPointC[season], seasonDewPointC[neighbour], Mathf.Abs(progress - 0.5f));

            if (dewDay != time.TotalDays)
            {
                dewDay = time.TotalDays;
                dewDayVariation = (float)new System.Random(unchecked(seed * 73856093 ^ dewDay * 19349663 ^ 5 * 2654435)).NextDouble() * 2f - 1f;
            }
            dew += dewDayVariation * dailyDewPointVariationC;
            dew += weatherDewPointC[(int)current];
            dew += rainRecentDewPointC * Mathf.Clamp01(rainRecentHours / rainRecentHoursMax);
            dew += Mathf.Lerp(heatWaveDewPointC.x, heatWaveDewPointC.y, Mathf.InverseLerp(6f, 12f, heatWaveStrengthC)) * HeatWaveIntensity;

            if (IsPrecipitating)
                dew = Mathf.Max(dew, temperature - precipitationDewDepressionC);
            return Mathf.Min(dew, temperature - minDewDepressionC);
        }
    }

    // 1-100 percent.
    public float RelativeHumidity => BodyHeat.RelativeHumidity(temperature, DewPointC);

    // Updated hourly, easing toward the target for the current season, time of day and weather.
    public float TemperatureC => temperature;
    public float TemperatureF => TemperatureC * 9f / 5f + 32f;
    public bool IsFreezing => TemperatureC <= 0f;
    // The NWS heat index in °F, for display (the pause screen, later the Weather panel); the body model does not use it.
    public float HeatIndexF => BodyHeat.HeatIndexF(TemperatureF, RelativeHumidity);

    // Compass bearing the wind blows from, 0–360 (0 = from the north).
    public float WindDirection => windDirection;
    public float WindStrength => Mathf.Max(CurrentClimate.windStrength, weatherTypes[(int)current].minWindStrength);

    SeasonClimate CurrentClimate =>
        seasons[TimeManager.Instance != null ? (int)TimeManager.Instance.CurrentSeason : 0];

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureArrays();
    }

    void Start()
    {
        if (Instance != this)
            return;

        TimeManager time = TimeManager.Instance;
        if (time != null)
        {
            time.HourChanged += OnHourChanged;
            time.DayChanged += OnDayChanged;
            time.SeasonChanged += OnSeasonChanged;
            subscribed = true;
        }

        if (SaveManager.Instance != null)
            SaveManager.Instance.Register(this);

        ResetWeather();
    }

    void OnDestroy()
    {
        if (Instance != this)
            return;

        TimeManager time = TimeManager.Instance;
        if (subscribed && time != null)
        {
            time.HourChanged -= OnHourChanged;
            time.DayChanged -= OnDayChanged;
            time.SeasonChanged -= OnSeasonChanged;
        }

        if (SaveManager.Instance != null)
            SaveManager.Instance.Unregister(this);
        Instance = null;
    }

    void OnValidate()
    {
        EnsureArrays();

        foreach (WeatherTypeSettings settings in weatherTypes)
        {
            if (settings != null)
                settings.maxHours = Mathf.Max(settings.maxHours, settings.minHours);
        }
    }

    // Inspector-serialized arrays keep what the scene saved and never pick up the C# defaults, so a scene saved before Heat
    // Waves has 8-entry arrays. Grow them to the current size here (at startup and on validate) with the new entries'
    // defaults, rather than with zeros and nulls: a missing weatherTypes entry would be null, and Summer would never roll a
    // Heat Wave. When Summer's weights grow, Clear gives up the Heat Wave's share, as in the C# defaults.
    void EnsureArrays()
    {
        if (seasons == null || seasons.Length != SeasonCount)
            Array.Resize(ref seasons, SeasonCount);

        if (weatherTypes == null || weatherTypes.Length != WeatherTypeCount)
        {
            int had = weatherTypes != null ? weatherTypes.Length : 0;
            Array.Resize(ref weatherTypes, WeatherTypeCount);
            for (int i = had; i < WeatherTypeCount; i++)
                weatherTypes[i] = i == (int)WeatherType.HeatWave
                    ? new WeatherTypeSettings { minHours = 72, maxHours = 144 }
                    : new WeatherTypeSettings();
        }
        for (int i = 0; i < weatherTypes.Length; i++)
            weatherTypes[i] ??= new WeatherTypeSettings();

        for (int s = 0; s < seasons.Length; s++)
        {
            SeasonClimate climate = seasons[s] ??= new SeasonClimate();
            if (climate.weights != null && climate.weights.Length == WeatherTypeCount)
                continue;

            int had = climate.weights != null ? climate.weights.Length : 0;
            Array.Resize(ref climate.weights, WeatherTypeCount);
            if (s == (int)Season.Summer && had > 0 && had <= (int)WeatherType.HeatWave)
            {
                const float HeatWaveShare = 15f;
                climate.weights[(int)WeatherType.HeatWave] = HeatWaveShare;
                climate.weights[(int)WeatherType.Clear] = Mathf.Max(0f, climate.weights[(int)WeatherType.Clear] - HeatWaveShare);
            }
        }

        if (seasonDewPointC == null || seasonDewPointC.Length != SeasonCount)
            seasonDewPointC = new[] { 6f, 17f, 7f, -4f };
        if (weatherDewPointC == null || weatherDewPointC.Length != WeatherTypeCount)
        {
            float[] old = weatherDewPointC;
            weatherDewPointC = new[] { 0f, 1.5f, 3f, 4f, 4f, -4f, 0f, -2f, 0f };
            if (old != null)
                for (int i = 0; i < Mathf.Min(old.Length, WeatherTypeCount); i++)
                    weatherDewPointC[i] = old[i];
        }
    }

    // Fresh weather with a new random seed — used on startup and when starting a new game. Fires no events.
    public void ResetWeather()
    {
        seed = Environment.TickCount;
        dryDays = 0;
        rainedToday = false;
        rainRecentHours = 0f;
        dewDay = -1;

        System.Random rng = Rng(0);
        windDirection = (float)rng.NextDouble() * 360f;
        dailyTemperatureOffset = RollTemperatureOffset(rng);
        RollNextWeather(rng, notify: false);
        temperature = TargetTemperatureC();
        ResolvePrecipitationType(notify: false);
    }

    void OnHourChanged(int hour)
    {
        if (IsRaining)
        {
            rainedToday = true;
            rainRecentHours = rainRecentHoursMax;
        }
        else
        {
            rainRecentHours = Mathf.Max(0f, rainRecentHours - 1f);
        }

        if (--hoursRemaining <= 0)
            RollAfterExpiry(notify: true);

        temperature = Mathf.MoveTowards(temperature, TargetTemperatureC(), maxTemperatureChangePerHour);
        ResolvePrecipitationType(notify: true);
    }

    void OnDayChanged()
    {
        // A Heat Wave is dry but not an ordinary dry stretch unless the Inspector says so (it would trip the drought every time).
        dryDays = rainedToday ? 0 : current == WeatherType.HeatWave && !heatWaveCountsAsDry ? dryDays : dryDays + 1;
        rainedToday = IsRaining;

        System.Random rng = Rng(2);
        dailyTemperatureOffset = RollTemperatureOffset(rng);
        float shift = ((float)rng.NextDouble() * 2f - 1f) * dailyWindShiftDegrees;
        windDirection = Mathf.Repeat(windDirection + shift, 360f);
    }

    // Weather that can't happen in the new season (e.g. Snow carried into Spring) ends immediately.
    void OnSeasonChanged(Season season)
    {
        if (seasons[(int)season].weights[(int)current] <= 0f)
            RollNextWeather(Rng(3), notify: true);
    }

    // A weather type has run out its time. A Heat Wave that does so ends with relief (a Thunderstorm or a Cold Front), forced
    // past the season's weights; anything else rolls as usual.
    void RollAfterExpiry(bool notify)
    {
        System.Random rng = Rng(1);
        WeatherType? forced = null;
        if (current == WeatherType.HeatWave)
            forced = rng.NextDouble() < heatWaveReliefThunderstormChance ? WeatherType.Thunderstorm : WeatherType.ColdFront;
        RollNextWeather(rng, notify, forced);
    }

    void RollNextWeather(System.Random rng, bool notify, WeatherType? forced = null)
    {
        WeatherType previous = current;
        current = forced ?? PickWeather(rng, CurrentWeights());

        WeatherTypeSettings settings = weatherTypes[(int)current];
        hoursRemaining = rng.Next(settings.minHours, settings.maxHours + 1);
        ResolvePrecipitationType(notify: false);

        if (IsRaining)
        {
            rainedToday = true;
            rainRecentHours = rainRecentHoursMax;
        }

        if (current == WeatherType.HeatWave)
        {
            heatWaveTotalHours = hoursRemaining;
            heatWaveStrengthC = RollHeatWaveStrength(rng);
        }
        else
        {
            heatWaveTotalHours = 0;
            heatWaveStrengthC = 0f;
        }

        if (notify && current != previous)
            WeatherChanged?.Invoke(current);
    }

    // An ordinary wave is about +6 °C; a rarer strong one adds more, with no design ceiling.
    float RollHeatWaveStrength(System.Random rng)
    {
        float strength = heatWaveBaseC + ((float)rng.NextDouble() * 2f - 1f) * heatWaveVariationC;
        if (rng.NextDouble() < heatWaveStrongChance)
            strength += Mathf.Lerp(heatWaveStrongExtraC.x, heatWaveStrongExtraC.y, (float)rng.NextDouble());
        return Mathf.Max(0f, strength);
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    // Testing only (TimeDebugControls, F4): starts a wave now, or ends the one under way.
    public void DebugStartHeatWave(float strengthC, int hours)
    {
        current = WeatherType.HeatWave;
        heatWaveStrengthC = strengthC;
        heatWaveTotalHours = hoursRemaining = Mathf.Max(1, hours);
        WeatherChanged?.Invoke(current);
    }

    // Testing only (TimeDebugControls, F2): the wave runs out its natural length now, so the forced Thunderstorm or Cold Front
    // follows as it would after a real one. Returns false when there is no wave.
    public bool DebugExpireHeatWave()
    {
        if (current != WeatherType.HeatWave)
            return false;
        hoursRemaining = 0;
        RollAfterExpiry(notify: true);
        temperature = Mathf.MoveTowards(temperature, TargetTemperatureC(), maxTemperatureChangePerHour);
        ResolvePrecipitationType(notify: true);
        return true;
    }

    public void DebugEndHeatWave()
    {
        if (current != WeatherType.HeatWave)
            return;
        current = WeatherType.Clear;
        hoursRemaining = 12;
        heatWaveStrengthC = 0f;
        heatWaveTotalHours = 0;
        WeatherChanged?.Invoke(current);
    }
#endif

    // Weather_System.md's Precipitation Type by Temperature: rain at or below freezing falls as Snow instead, in any
    // season, and Snow's gameplay effects apply. Checked when weather is rolled, every hour as the temperature
    // moves, and on load. One way only, as the doc specifies. A Thunderstorm keeps its type (see PrecipitationWeather).
    void ResolvePrecipitationType(bool notify)
    {
        if ((current != WeatherType.LightRain && current != WeatherType.HeavyRain) || temperature > 0f)
            return;

        current = WeatherType.Snow;
        if (notify)
            WeatherChanged?.Invoke(current);
    }

    // The current season's weights, plus the rare late-Fall snow.
    float[] CurrentWeights()
    {
        float[] weights = CurrentClimate.weights;
        TimeManager time = TimeManager.Instance;
        if (time == null || time.CurrentSeason != Season.Fall || time.SeasonProgress < lateFallStartsAt)
            return weights;

        var lateFall = (float[])weights.Clone();
        lateFall[(int)WeatherType.Snow] = Mathf.Max(lateFall[(int)WeatherType.Snow], lateFallSnowShare);
        return lateFall;
    }

    // Weights are shares of time, so each type's roll chance is divided by its average duration.
    WeatherType PickWeather(System.Random rng, float[] weights)
    {
        float total = 0f;
        for (int i = 0; i < weights.Length; i++)
            total += RollWeight(weights, i);

        if (total <= 0f)
            return WeatherType.Clear;

        float roll = (float)rng.NextDouble() * total;
        for (int i = 0; i < weights.Length; i++)
        {
            roll -= RollWeight(weights, i);
            if (roll < 0f)
                return (WeatherType)i;
        }

        return WeatherType.Clear;
    }

    float RollWeight(float[] weights, int type)
    {
        WeatherTypeSettings settings = weatherTypes[type];
        float weight = Mathf.Max(0f, weights[type]) / ((settings.minHours + settings.maxHours) * 0.5f);
        // Difficulty's weather severity: the rough weather comes more or less often (DifficultyManager).
        var kind = (WeatherType)type;
        // Heat Waves are on the same list as the storms (Difficulty_System.md's Storm odds: Homesteader x0.6, Settler x1, Pioneer x1.5).
        if (kind == WeatherType.HeavyRain || kind == WeatherType.Thunderstorm || kind == WeatherType.ColdFront || kind == WeatherType.Snow ||
            kind == WeatherType.HeatWave)
            weight *= DifficultyManager.StormMultiplier;
        return weight;
    }

    float RollTemperatureOffset(System.Random rng) =>
        ((float)rng.NextDouble() * 2f - 1f) * dailyTemperatureVariationC;

    // Seasonal high/low blended toward the neighbouring season (so there's no jump at a season change),
    // shaped by a daily curve peaking mid-afternoon, plus daily variation and the current weather.
    float TargetTemperatureC()
    {
        TimeManager time = TimeManager.Instance;
        if (time == null)
            return 0f;

        int season = (int)time.CurrentSeason;
        float progress = time.SeasonProgress;
        int neighbour = progress < 0.5f ? (season + SeasonCount - 1) % SeasonCount : (season + 1) % SeasonCount;
        float blend = Mathf.Abs(progress - 0.5f);

        float high = Mathf.Lerp(seasons[season].highC, seasons[neighbour].highC, blend);
        float low = Mathf.Lerp(seasons[season].lowC, seasons[neighbour].lowC, blend);

        // A Heat Wave adds its per-wave rise and shrinks the daily swing (warm nights): at 0 intensity this is exactly the
        // plain lerp(low, high, dayCurve).
        float wave = HeatWaveIntensity;
        float dayCurve = (Mathf.Cos((time.HourOfDay - PeakTemperatureHour) / 24f * 2f * Mathf.PI) + 1f) * 0.5f;
        float temperature = high - (high - low) * (1f - dayCurve) * (1f - heatWaveNightCoolingCut * wave)
                            + dailyTemperatureOffset
                            + weatherTypes[(int)current].temperatureOffsetC
                            + heatWaveStrengthC * wave;
        temperature += DifficultyOffsetC(temperature);

        // Snow pulls the temperature to freezing or below (reached over the next few hours).
        if (current == WeatherType.Snow)
            temperature = Mathf.Min(temperature, 0f);

        return Mathf.Min(temperature, maxTemperatureC);
    }

    // Difficulty's temperature offset in the harmful direction (Core Temperature Model): Pioneer is colder in cold weather
    // and hotter in hot weather, Homesteader the reverse, easing through zero between the two temperatures below.
    float DifficultyOffsetC(float baseC) =>
        DifficultyManager.TemperatureOffsetC * Mathf.Lerp(1f, -1f, Mathf.InverseLerp(offsetColdBelowC, offsetHotAboveC, baseC));

    // Deterministic per seed and point in time, so a reloaded save keeps rolling the same way.
    System.Random Rng(int salt)
    {
        TimeManager time = TimeManager.Instance;
        int days = time != null ? time.TotalDays : 0;
        int hour = time != null ? time.Hour : 0;
        return new System.Random(unchecked(seed * 73856093 ^ days * 19349663 ^ hour * 83492791 ^ salt * 2654435));
    }

    public WeatherSaveData CaptureState() => new WeatherSaveData
    {
        type = current,
        hoursRemaining = hoursRemaining,
        temperatureC = temperature,
        dailyTemperatureOffset = dailyTemperatureOffset,
        windDirection = windDirection,
        dryDays = dryDays,
        rainedToday = rainedToday,
        seed = seed,
        heatWaveStrengthC = heatWaveStrengthC,
        heatWaveTotalHours = heatWaveTotalHours,
        rainRecentHours = rainRecentHours,
    };

    // Restores silently — no change events fire, so listeners should re-read state after a load.
    public void RestoreState(WeatherSaveData data)
    {
        current = data.type;
        hoursRemaining = Mathf.Max(1, data.hoursRemaining);
        temperature = data.temperatureC;
        dailyTemperatureOffset = data.dailyTemperatureOffset;
        windDirection = Mathf.Repeat(data.windDirection, 360f);
        dryDays = Mathf.Max(0, data.dryDays);
        rainedToday = data.rainedToday;
        seed = data.seed;
        // Saves from before Heat Waves carry no wave strength: a HeatWave type with none just reads as no wave.
        heatWaveStrengthC = current == WeatherType.HeatWave ? Mathf.Max(0f, data.heatWaveStrengthC) : 0f;
        heatWaveTotalHours = current == WeatherType.HeatWave ? Mathf.Max(hoursRemaining, data.heatWaveTotalHours) : 0;
        rainRecentHours = Mathf.Clamp(data.rainRecentHours, 0f, rainRecentHoursMax);
        dewDay = -1;
        ResolvePrecipitationType(notify: false); // saves from before this rule can hold rain below freezing
    }

    // Save_Data_Model.md's World Block: weather state.
    string ISaveable.SaveFile => "world";
    string ISaveable.SaveKey => "weather";
    object ISaveable.CaptureState() => CaptureState();
    void ISaveable.RestoreState(string json) => RestoreState(JsonUtility.FromJson<WeatherSaveData>(json));
}
