using UnityEngine;

// Moves the sun (and a dim moon) through the sky from TimeManager's clock, so the world matches Dawn, Day, Dusk and
// Night (Wildlife_System.md's activity cycles) and Season_System.md's long Summer / short Winter days. Works with
// HDRP's Physically Based Sky, which colours dawn, dusk and night from the sun's angle on its own. Overcast and
// stormy weather dims the sun (Weather_System.md: lower visibility). Everything follows the in-game clock, so the
// lighting freezes while paused.
public class DayNightCycle : MonoBehaviour
{
    [SerializeField] Light sun;
    [Tooltip("Optional dim directional light for nights. Doesn't cast shadows.")]
    [SerializeField] Light moon;

    // Tuning defaults — the design docs describe day length and seasons, not sun angles or light levels.
    [Header("Sun path")]
    [Tooltip("Sun elevation at midday per season (Spring, Summer, Fall, Winter), in degrees. " +
             "Defaults approximate a temperate latitude (~37°N).")]
    [SerializeField] float[] middayElevation = { 53f, 76f, 53f, 30f };
    [Tooltip("How far below the horizon the sun sinks at midnight, in degrees.")]
    [SerializeField, Range(1f, 90f)] float midnightDepression = 35f;

    [Header("Light levels (lux)")]
    [SerializeField, Min(0f)] float sunIntensity = 100000f;
    [SerializeField, Min(0f)] float moonIntensity = 0.5f;
    [Tooltip("The sun fades out over this many degrees either side of the horizon.")]
    [SerializeField, Range(0.1f, 10f)] float horizonFadeDegrees = 2f;

    [Header("Weather")]
    [Tooltip("Sunlight multiplier per weather type, in WeatherType order: " +
             "Clear, Cloudy, LightRain, HeavyRain, Thunderstorm, ColdFront, Snow, Wind.")]
    [SerializeField] float[] weatherSunlight = { 1f, 0.5f, 0.35f, 0.2f, 0.15f, 0.8f, 0.35f, 0.9f };
    [Tooltip("How quickly the light adjusts to a weather change, per in-game hour.")]
    [SerializeField, Min(0.01f)] float weatherBlendPerHour = 2f;

    [Header("Without a TimeManager")]
    [Tooltip("Used when World is played directly without Bootstrap.")]
    [SerializeField, Range(0f, 24f)] float fallbackHour = 12f;

    const int SeasonCount = 4;
    const float SunriseAzimuth = 90f; // east; the sun crosses south (180°) to set in the west (270°)

    float weatherFactor = 1f;
    float lastHour = -1f;

    // Current sun elevation in degrees; negative is below the horizon.
    public float SunElevation { get; private set; }

    void Start()
    {
        if (WeatherManager.Instance != null)
            weatherFactor = WeatherSunlight(WeatherManager.Instance.Current);
        Apply();
    }

    void Update() => Apply();

    void OnValidate()
    {
        if (middayElevation == null || middayElevation.Length != SeasonCount)
            System.Array.Resize(ref middayElevation, SeasonCount);
        if (weatherSunlight == null || weatherSunlight.Length != 8)
            System.Array.Resize(ref weatherSunlight, 8);
    }

    void Apply()
    {
        TimeManager time = TimeManager.Instance;
        float hour = time != null ? time.HourOfDay : fallbackHour;
        float sunrise = time != null ? time.SunriseHour : 6f;
        float sunset = time != null ? time.SunsetHour : 18f;
        float noonElevation = time != null ? BlendedMiddayElevation(time) : middayElevation[0];

        // Ease toward the current weather's light level at a rate tied to in-game time (frozen while paused).
        if (WeatherManager.Instance != null && time != null)
        {
            float elapsedHours = lastHour < 0f ? 0f : Mathf.Repeat(hour - lastHour, 24f);
            weatherFactor = Mathf.MoveTowards(weatherFactor, WeatherSunlight(WeatherManager.Instance.Current),
                                              weatherBlendPerHour * elapsedHours);
        }
        lastHour = hour;

        SunPosition(hour, sunrise, sunset, noonElevation, midnightDepression, out float elevation, out float azimuth);
        SunElevation = elevation;

        float daylight = Mathf.Clamp01((elevation + horizonFadeDegrees) / (2f * horizonFadeDegrees));

        if (sun != null)
        {
            // A directional light shines along its forward axis, so point it from the sun toward the ground.
            sun.transform.rotation = Quaternion.Euler(elevation, azimuth + 180f, 0f);
            sun.intensity = sunIntensity * daylight * weatherFactor;
            sun.enabled = daylight > 0f;
        }

        if (moon != null)
        {
            // Opposite the sun: high in the sky around midnight.
            moon.transform.rotation = Quaternion.Euler(-elevation, azimuth, 0f);
            moon.intensity = moonIntensity * (1f - daylight) * Mathf.Lerp(0.5f, 1f, weatherFactor);
            moon.enabled = daylight < 1f;
        }
    }

    // Day: rises in the east at sunrise, peaks at midday, sets in the west at sunset.
    // Night: continues below the horizon through the north, lowest at the middle of the night.
    static void SunPosition(float hour, float sunrise, float sunset, float noonElevation, float depression,
                            out float elevation, out float azimuth)
    {
        float dayLength = Mathf.Max(0.01f, sunset - sunrise);

        if (hour >= sunrise && hour < sunset)
        {
            float t = (hour - sunrise) / dayLength;
            elevation = noonElevation * Mathf.Sin(Mathf.PI * t);
            azimuth = SunriseAzimuth + 180f * t;
        }
        else
        {
            float nightLength = Mathf.Max(0.01f, 24f - dayLength);
            float t = Mathf.Repeat(hour - sunset, 24f) / nightLength;
            elevation = -depression * Mathf.Sin(Mathf.PI * t);
            azimuth = SunriseAzimuth + 180f + 180f * t;
        }
    }

    // Blended toward the neighbouring season like WeatherManager's temperatures, so the sun's height changes
    // gradually instead of jumping at a season boundary.
    float BlendedMiddayElevation(TimeManager time)
    {
        int season = (int)time.CurrentSeason;
        float progress = time.SeasonProgress;
        int neighbour = progress < 0.5f ? (season + SeasonCount - 1) % SeasonCount : (season + 1) % SeasonCount;
        return Mathf.Lerp(middayElevation[season], middayElevation[neighbour], Mathf.Abs(progress - 0.5f));
    }

    float WeatherSunlight(WeatherType weather)
    {
        int index = (int)weather;
        return index < weatherSunlight.Length ? Mathf.Clamp01(weatherSunlight[index]) : 1f;
    }
}
