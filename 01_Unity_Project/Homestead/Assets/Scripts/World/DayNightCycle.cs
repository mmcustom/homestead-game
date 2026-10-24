using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

// Moves the sun (and a dim moon) through the sky from TimeManager's clock, so the world matches Dawn, Day, Dusk and
// Night (Wildlife_System.md's activity cycles) and Season_System.md's long Summer / short Winter days. Works with
// HDRP's Physically Based Sky, which colours dawn, dusk and night from the sun's angle on its own. Overcast and
// stormy weather dims the sun, thickens the fog and covers the sky (Weather_System.md: lower visibility). Everything
// follows the in-game clock, so the lighting freezes while paused.
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
             "Clear, Cloudy, LightRain, HeavyRain, Thunderstorm, ColdFront, Snow, Wind, HeatWave. The body's sun load " +
             "reads these too (SunlightFor), so the heat matches what the sky shows.")]
    [SerializeField] float[] weatherSunlight = { 1f, 0.5f, 0.35f, 0.2f, 0.15f, 0.8f, 0.35f, 0.9f, 1f };
    [Tooltip("Fog attenuation distance per weather type, in metres, in WeatherType order. Shorter = thicker fog.")]
    [SerializeField] float[] weatherFogDistance = { 400f, 300f, 200f, 120f, 80f, 350f, 150f, 400f, 380f };
    [Tooltip("Cloud cover (0–1) per weather type, in WeatherType order.")]
    [SerializeField] float[] weatherCloudCover = { 0f, 0.9f, 0.9f, 1f, 1f, 0.5f, 1f, 0.3f, 0.1f };
    [Tooltip("How much of the sun's weather dimming shows on screen. Auto exposure would otherwise brighten a " +
             "stormy day back to look like a clear one; 0 lets it, 1 shows the full dimming.")]
    [SerializeField, Range(0f, 1f)] float weatherDarkening = 0.7f;
    [Tooltip("Fog is thickest below this height and thins out up to the maximum, in world metres. Set for the " +
             "property's terrain, which sits roughly 15–35m up.")]
    [SerializeField] float fogBaseHeight = 20f;
    [SerializeField] float fogMaximumHeight = 80f;
    [Tooltip("How quickly the light adjusts to a weather change, per in-game hour.")]
    [SerializeField, Min(0.01f)] float weatherBlendPerHour = 2f;

    [Header("Without a TimeManager")]
    [Tooltip("Used when World is played directly without Bootstrap.")]
    [SerializeField, Range(0f, 24f)] float fallbackHour = 12f;

    const int SeasonCount = 4;
    const int WeatherTypeCount = 9;
    const float SunriseAzimuth = 90f; // east; the sun crosses south (180°) to set in the west (270°)

    float weatherFactor = 1f;
    float fogDistance, cloudCover;
    float lastHour = -1f;

    // Weather overrides live on a runtime volume above the scene's own, so the profile asset is never modified.
    VolumeProfile weatherProfile;
    Fog fog;
    CloudLayer clouds;
    Exposure exposure;

    // Current sun elevation in degrees; negative is below the horizon.
    public float SunElevation { get; private set; }

    void Start()
    {
        WeatherType weather = WeatherManager.Instance != null ? WeatherManager.Instance.Current : WeatherType.Clear;
        weatherFactor = WeatherSunlight(weather);
        fogDistance = PerWeather(weatherFogDistance, weather, 400f);
        cloudCover = PerWeather(weatherCloudCover, weather, 0f);
        CreateWeatherVolume();
        Apply();
    }

    void OnDestroy()
    {
        if (weatherProfile != null)
            Destroy(weatherProfile);
    }

    void CreateWeatherVolume()
    {
        weatherProfile = ScriptableObject.CreateInstance<VolumeProfile>();
        weatherProfile.name = "Weather (runtime)";

        fog = weatherProfile.Add<Fog>();
        fog.meanFreePath.overrideState = true;
        fog.baseHeight.Override(fogBaseHeight);
        fog.maximumHeight.Override(fogMaximumHeight);

        var environment = weatherProfile.Add<VisualEnvironment>();
        environment.cloudType.Override((int)CloudType.CloudLayer);
        clouds = weatherProfile.Add<CloudLayer>();
        clouds.opacity.overrideState = true;
        // The default cloud map's red channel is scattered cloud; the other channels fill the gaps toward overcast.
        clouds.layerA.opacityR.Override(1f);
        clouds.layerA.opacityG.overrideState = true;
        clouds.layerA.opacityB.overrideState = true;
        clouds.layerA.opacityA.overrideState = true;

        exposure = weatherProfile.Add<Exposure>();
        exposure.compensation.overrideState = true;

        var holder = new GameObject("Weather Volume");
        holder.transform.SetParent(transform, false);
        var volume = holder.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 10f;
        volume.sharedProfile = weatherProfile;
    }

    void Update() => Apply();

    // Built-in values, for the Heat Wave entry a scene saved before it lacks (Inspector arrays never pick up C# defaults).
    static readonly float[] DefaultSunlight = { 1f, 0.5f, 0.35f, 0.2f, 0.15f, 0.8f, 0.35f, 0.9f, 1f };
    static readonly float[] DefaultFogDistance = { 400f, 300f, 200f, 120f, 80f, 350f, 150f, 400f, 380f };
    static readonly float[] DefaultCloudCover = { 0f, 0.9f, 0.9f, 1f, 1f, 0.5f, 1f, 0.3f, 0.1f };

    // The scene's live instance, so the body model can read the same per-weather sunlight the sky uses without going
    // through the frame-updated sun elevation (which is stale during sleep and collapse time skips).
    static DayNightCycle active;

    void OnEnable() => active = this;

    void OnDisable()
    {
        if (active == this)
            active = null;
    }

    // Sunlight multiplier (0-1) for a weather type: what the sun's light uses, so heat and visuals agree.
    public static float SunlightFor(WeatherType weather)
    {
        float[] values = active != null && active.weatherSunlight != null ? active.weatherSunlight : DefaultSunlight;
        return Mathf.Clamp01(PerWeather(values, weather, DefaultSunlight[Mathf.Min((int)weather, DefaultSunlight.Length - 1)]));
    }

    static void Grow(ref float[] values, float[] defaults)
    {
        int had = values != null ? values.Length : 0;
        System.Array.Resize(ref values, WeatherTypeCount);
        for (int i = had; i < WeatherTypeCount; i++)
            values[i] = defaults[i];
    }

    // Inspector-serialized arrays keep what the scene saved and never pick up the C# defaults, so a scene saved before the Heat Wave has
    // 8-entry weather arrays. OnValidate does not run in a built game, so Awake grows them too: the new entries get the defaults and
    // the scene's own values are kept.
    void Awake() => EnsureArrays();

    void OnValidate() => EnsureArrays();

    void EnsureArrays()
    {
        if (middayElevation == null || middayElevation.Length != SeasonCount)
            System.Array.Resize(ref middayElevation, SeasonCount);
        if (weatherSunlight == null || weatherSunlight.Length != WeatherTypeCount)
            Grow(ref weatherSunlight, DefaultSunlight);
        if (weatherFogDistance == null || weatherFogDistance.Length != WeatherTypeCount)
            Grow(ref weatherFogDistance, DefaultFogDistance);
        if (weatherCloudCover == null || weatherCloudCover.Length != WeatherTypeCount)
            Grow(ref weatherCloudCover, DefaultCloudCover);
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
            WeatherType weather = WeatherManager.Instance.Current;
            float blend = weatherBlendPerHour * elapsedHours;
            weatherFactor = Mathf.MoveTowards(weatherFactor, WeatherSunlight(weather), blend);
            fogDistance = Mathf.MoveTowards(fogDistance, PerWeather(weatherFogDistance, weather, 400f), blend * 400f);
            cloudCover = Mathf.MoveTowards(cloudCover, PerWeather(weatherCloudCover, weather, 0f), blend);
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

        if (fog != null)
        {
            fog.meanFreePath.value = Mathf.Max(1f, fogDistance);
            float cover = Mathf.Clamp01(cloudCover);
            clouds.opacity.value = cover;
            clouds.layerA.opacityG.value = cover;
            clouds.layerA.opacityB.value = cover;
            clouds.layerA.opacityA.value = cover;
            exposure.compensation.value = Mathf.Log(Mathf.Max(weatherFactor, 0.01f), 2f) * weatherDarkening;
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

    float WeatherSunlight(WeatherType weather) => Mathf.Clamp01(PerWeather(weatherSunlight, weather, 1f));

    static float PerWeather(float[] values, WeatherType weather, float fallback)
    {
        int index = (int)weather;
        return values != null && index < values.Length ? values[index] : fallback;
    }
}
