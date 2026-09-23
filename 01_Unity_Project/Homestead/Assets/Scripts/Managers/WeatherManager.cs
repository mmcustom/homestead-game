using System;
using UnityEngine;

// Weather_System.md's weather types. Wind also has a standing direction/strength (see WindDirection),
// since Hunting_System.md uses wind direction regardless of the current weather type.
public enum WeatherType { Clear, Cloudy, LightRain, HeavyRain, Thunderstorm, ColdFront, Snow, Wind }

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
}

// Weather, temperature, seasonal conditions (Unity_Architecture.md, Weather_System.md).
// Driven entirely by TimeManager's hour/day/season events, so weather stops whenever the clock stops.
public class WeatherManager : MonoBehaviour, ISaveable
{
    [Serializable]
    public class SeasonClimate
    {
        [Tooltip("Relative share of the season's time spent in each weather type, in WeatherType order: " +
                 "Clear, Cloudy, LightRain, HeavyRain, Thunderstorm, ColdFront, Snow, Wind. " +
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

    const int WeatherTypeCount = 8;
    const int SeasonCount = 4;
    const float PeakTemperatureHour = 15f;

    public static WeatherManager Instance { get; private set; }

    // All values below are tuning defaults — Weather_System.md describes seasonal tendencies but no numbers.
    [Header("Climate per season (Spring, Summer, Fall, Winter)")]
    [SerializeField] SeasonClimate[] seasons =
    {
        // Spring: frequent rain, unpredictable conditions.
        new SeasonClimate { weights = new float[] { 30, 30, 22, 10, 3, 2, 0, 3 }, highC = 18f, lowC = 5f, windStrength = 0.3f },
        // Summer: heat, occasional thunderstorms, drought risk in dry stretches.
        new SeasonClimate { weights = new float[] { 64, 25, 3, 1, 2, 0, 0, 5 }, highC = 30f, lowC = 18f, windStrength = 0.2f },
        // Fall: cooling trends, increasing wind.
        new SeasonClimate { weights = new float[] { 40, 28, 10, 4, 1, 7, 0, 10 }, highC = 17f, lowC = 5f, windStrength = 0.4f },
        // Winter: snow and cold fronts dominate.
        new SeasonClimate { weights = new float[] { 20, 25, 3, 0, 0, 15, 30, 7 }, highC = 5f, lowC = -6f, windStrength = 0.35f },
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
    };

    [Header("Variation")]
    [Tooltip("Each day's temperature shifts by up to ± this many °C, so no two years feel identical.")]
    [SerializeField, Min(0f)] float dailyTemperatureVariationC = 4f;
    [Tooltip("How far the prevailing wind direction can swing from one day to the next, in degrees.")]
    [SerializeField, Range(0f, 180f)] float dailyWindShiftDegrees = 45f;
    [Tooltip("Fastest the temperature can move toward its target, in °C per in-game hour — " +
             "a cold front arrives over a few hours rather than instantly.")]
    [SerializeField, Min(0.1f)] float maxTemperatureChangePerHour = 3f;

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

    public event Action<WeatherType> WeatherChanged;

    public WeatherType Current => current;
    public int HoursRemaining => hoursRemaining;

    public bool IsRaining => current == WeatherType.LightRain || current == WeatherType.HeavyRain || current == WeatherType.Thunderstorm;
    public bool IsPrecipitating => IsRaining || current == WeatherType.Snow;

    // Weather_System.md's Exposure Connection: these contribute to exposure risk without adequate shelter.
    public bool IsSevere => current == WeatherType.Thunderstorm || current == WeatherType.ColdFront || current == WeatherType.Snow;

    public bool IsDrought => TimeManager.Instance != null &&
                             TimeManager.Instance.CurrentSeason == Season.Summer &&
                             dryDays >= droughtDryDays;
    public int DryDays => dryDays;

    // Updated hourly, easing toward the target for the current season, time of day and weather.
    public float TemperatureC => temperature;
    public float TemperatureF => TemperatureC * 9f / 5f + 32f;
    public bool IsFreezing => TemperatureC <= 0f;

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
        if (seasons == null || seasons.Length != SeasonCount)
            Array.Resize(ref seasons, SeasonCount);
        if (weatherTypes == null || weatherTypes.Length != WeatherTypeCount)
            Array.Resize(ref weatherTypes, WeatherTypeCount);

        foreach (SeasonClimate climate in seasons)
        {
            if (climate != null && (climate.weights == null || climate.weights.Length != WeatherTypeCount))
                Array.Resize(ref climate.weights, WeatherTypeCount);
        }

        foreach (WeatherTypeSettings settings in weatherTypes)
        {
            if (settings != null)
                settings.maxHours = Mathf.Max(settings.maxHours, settings.minHours);
        }
    }

    // Fresh weather with a new random seed — used on startup and when starting a new game. Fires no events.
    public void ResetWeather()
    {
        seed = Environment.TickCount;
        dryDays = 0;
        rainedToday = false;

        System.Random rng = Rng(0);
        windDirection = (float)rng.NextDouble() * 360f;
        dailyTemperatureOffset = RollTemperatureOffset(rng);
        RollNextWeather(rng, notify: false);
        temperature = TargetTemperatureC();
    }

    void OnHourChanged(int hour)
    {
        if (IsRaining)
            rainedToday = true;

        if (--hoursRemaining <= 0)
            RollNextWeather(Rng(1), notify: true);

        temperature = Mathf.MoveTowards(temperature, TargetTemperatureC(), maxTemperatureChangePerHour);
    }

    void OnDayChanged()
    {
        dryDays = rainedToday ? 0 : dryDays + 1;
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

    void RollNextWeather(System.Random rng, bool notify)
    {
        WeatherType previous = current;
        current = PickWeather(rng, CurrentClimate.weights);

        WeatherTypeSettings settings = weatherTypes[(int)current];
        hoursRemaining = rng.Next(settings.minHours, settings.maxHours + 1);

        if (IsRaining)
            rainedToday = true;

        if (notify && current != previous)
            WeatherChanged?.Invoke(current);
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
        return Mathf.Max(0f, weights[type]) / ((settings.minHours + settings.maxHours) * 0.5f);
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

        float dayCurve = (Mathf.Cos((time.HourOfDay - PeakTemperatureHour) / 24f * 2f * Mathf.PI) + 1f) * 0.5f;
        float temperature = Mathf.Lerp(low, high, dayCurve)
                            + dailyTemperatureOffset
                            + weatherTypes[(int)current].temperatureOffsetC;

        // Snow pulls the temperature to freezing or below (reached over the next few hours).
        if (current == WeatherType.Snow)
            temperature = Mathf.Min(temperature, 0f);

        return temperature;
    }

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
    }

    // Save_Data_Model.md's World Block: weather state.
    string ISaveable.SaveFile => "world";
    string ISaveable.SaveKey => "weather";
    object ISaveable.CaptureState() => CaptureState();
    void ISaveable.RestoreState(string json) => RestoreState(JsonUtility.FromJson<WeatherSaveData>(json));
}
