using UnityEngine;

// Falling rain and snow around the camera (Weather_System.md: Visual Feedback). Presentation only — it reads
// WeatherManager's current weather and never changes it. Light Rain is sparse; Heavy Rain and Thunderstorm share a
// denser rain; Snow falls slowly and drifts; Clear, Cloudy, Cold Front and Wind have none. Particles spawn in a box
// above the camera and are simulated in world space, so they don't slide along as the player walks. Overhead
// cover thins it out: lighter under tree canopy, none under a roof (OverheadCover).
public class Precipitation : MonoBehaviour
{
    [SerializeField] Material rainMaterial;
    [SerializeField] Material snowMaterial;

    [Header("Rain")]
    [Tooltip("Drops spawned per second, in WeatherType order: " +
             "Clear, Cloudy, LightRain, HeavyRain, Thunderstorm, ColdFront, Snow, Wind.")]
    [SerializeField] float[] rainPerSecond = { 0f, 0f, 1500f, 6000f, 6000f, 0f, 0f, 0f };
    [SerializeField, Min(0.1f)] float rainFallSpeed = 10f;

    [Header("Snow")]
    [Tooltip("Flakes spawned per second, in WeatherType order.")]
    [SerializeField] float[] snowPerSecond = { 0f, 0f, 0f, 0f, 0f, 0f, 800f, 0f };
    [SerializeField, Min(0.1f)] float snowFallSpeed = 1.2f;

    [Header("Area")]
    [Tooltip("Width of the square area around the camera that precipitation falls in, in metres.")]
    [SerializeField, Min(1f)] float areaSize = 50f;
    [Tooltip("How far above the camera particles spawn, in metres.")]
    [SerializeField, Min(1f)] float spawnHeight = 15f;
    [Tooltip("Sideways drift at full wind strength, in metres per second.")]
    [SerializeField, Min(0f)] float windDrift = 3f;
    [Tooltip("Seconds for precipitation to build up or die away when the weather changes.")]
    [SerializeField, Min(0.01f)] float fadeSeconds = 8f;

    [Header("Overhead cover")]
    [Tooltip("How much of the precipitation a full tree canopy keeps off the player, 0–1.")]
    [SerializeField, Range(0f, 1f)] float canopyShelter = 0.7f;
    [Tooltip("Seconds to adjust when walking in or out of cover.")]
    [SerializeField, Min(0.01f)] float coverFadeSeconds = 1f;

    const int WeatherTypeCount = 8;
    const float CoverCheckInterval = 0.25f;

    ParticleSystem rain, snow;
    Material rainInstance, snowInstance;
    Color rainColor, snowColor;
    float rainRate, snowRate;
    float canopy, roof, canopyTarget, roofTarget, nextCoverCheck;
    Transform cameraTransform;
    DayNightCycle dayNight;

    void Awake()
    {
        rain = CreateSystem("Rain", rainMaterial, out rainInstance, rainFallSpeed, 0.02f,
                            ParticleSystemRenderMode.Stretch, 12000);
        snow = CreateSystem("Snow", snowMaterial, out snowInstance, snowFallSpeed, 0.06f,
                            ParticleSystemRenderMode.Billboard, 15000);
        if (rainInstance != null)
            rainColor = rainInstance.GetColor("_UnlitColor");
        if (snowInstance != null)
            snowColor = snowInstance.GetColor("_UnlitColor");

        // Snow wanders as it falls.
        ParticleSystem.NoiseModule noise = snow.noise;
        noise.enabled = true;
        noise.strength = 0.6f;
        noise.frequency = 0.3f;
        noise.scrollSpeed = 0.2f;

        // Rain is drawn as streaks along its direction of travel.
        var rainRenderer = rain.GetComponent<ParticleSystemRenderer>();
        rainRenderer.velocityScale = 0.06f;
        rainRenderer.lengthScale = 1f;

        dayNight = FindAnyObjectByType<DayNightCycle>();
    }

    void OnDestroy()
    {
        if (rainInstance != null)
            Destroy(rainInstance);
        if (snowInstance != null)
            Destroy(snowInstance);
    }

    void OnValidate()
    {
        if (rainPerSecond == null || rainPerSecond.Length != WeatherTypeCount)
            System.Array.Resize(ref rainPerSecond, WeatherTypeCount);
        if (snowPerSecond == null || snowPerSecond.Length != WeatherTypeCount)
            System.Array.Resize(ref snowPerSecond, WeatherTypeCount);
    }

    void LateUpdate()
    {
        if (cameraTransform == null)
        {
            Camera main = Camera.main;
            if (main == null)
                return;
            cameraTransform = main.transform;
        }

        transform.position = cameraTransform.position + Vector3.up * spawnHeight;

        WeatherManager weather = WeatherManager.Instance;
        WeatherType current = weather != null ? weather.PrecipitationWeather : WeatherType.Clear;

        // Scaled time, so precipitation stops building or fading while the game is paused (as the particles do).
        float step = Time.deltaTime / fadeSeconds;
        rainRate = Mathf.MoveTowards(rainRate, PerWeather(rainPerSecond, current), step * MaxOf(rainPerSecond));
        snowRate = Mathf.MoveTowards(snowRate, PerWeather(snowPerSecond, current), step * MaxOf(snowPerSecond));

        Vector3 drift = Vector3.zero;
        if (weather != null)
        {
            // WindDirection is the compass bearing the wind blows from, so drift points the other way.
            float bearing = weather.WindDirection * Mathf.Deg2Rad;
            drift = -new Vector3(Mathf.Sin(bearing), 0f, Mathf.Cos(bearing)) * (weather.WindStrength * windDrift);
        }

        if (Time.time >= nextCoverCheck)
        {
            nextCoverCheck = Time.time + CoverCheckInterval;
            roofTarget = OverheadCover.Roofed(cameraTransform.position) ? 1f : 0f;
            canopyTarget = canopyShelter * OverheadCover.CanopyFraction(cameraTransform.position);
        }
        float coverStep = Time.deltaTime / coverFadeSeconds;
        canopy = Mathf.MoveTowards(canopy, canopyTarget, coverStep);
        roof = Mathf.MoveTowards(roof, roofTarget, coverStep);
        // Canopy thins what falls; a roof also hides drops already in the air, so it's dry indoors straight away.
        float reach = (1f - canopy) * (1f - roof);

        // Unlit particles would glow at night, so they follow the daylight.
        float light = dayNight != null ? Mathf.Lerp(0.12f, 1f, Mathf.InverseLerp(-6f, 10f, dayNight.SunElevation)) : 1f;

        Drive(rain, rainRate * reach, rainFallSpeed, drift, rainInstance, rainColor, light, 1f - roof);
        Drive(snow, snowRate * reach, snowFallSpeed, drift * 0.6f, snowInstance, snowColor, light, 1f - roof);
    }

    static void Drive(ParticleSystem system, float rate, float fallSpeed, Vector3 drift, Material material, Color color,
                      float light, float opacity)
    {
        ParticleSystem.EmissionModule emission = system.emission;
        emission.rateOverTime = rate;

        ParticleSystem.VelocityOverLifetimeModule velocity = system.velocityOverLifetime;
        velocity.x = drift.x;
        velocity.y = -fallSpeed;
        velocity.z = drift.z;

        if (material != null)
            material.SetColor("_UnlitColor", new Color(color.r * light, color.g * light, color.b * light, color.a * opacity));

        if (rate > 0f && !system.isPlaying)
            system.Play();
        else if (rate <= 0f && system.isPlaying && system.particleCount == 0)
            system.Stop();
    }

    ParticleSystem CreateSystem(string systemName, Material material, out Material instance, float fallSpeed,
                                float size, ParticleSystemRenderMode renderMode, int maxParticles)
    {
        var child = new GameObject(systemName);
        child.transform.SetParent(transform, false);
        var system = child.AddComponent<ParticleSystem>();
        system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem.MainModule main = system.main;
        main.loop = true;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startSpeed = 0f;
        main.startSize = size;
        // Long enough to fall from the spawn box to below the player's feet.
        main.startLifetime = (spawnHeight + 3f) / fallSpeed;
        main.maxParticles = maxParticles;

        ParticleSystem.EmissionModule emission = system.emission;
        emission.rateOverTime = 0f;

        ParticleSystem.ShapeModule shape = system.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(areaSize, 1f, areaSize);

        ParticleSystem.VelocityOverLifetimeModule velocity = system.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;
        velocity.x = 0f;
        velocity.y = -fallSpeed;
        velocity.z = 0f;

        var particleRenderer = child.GetComponent<ParticleSystemRenderer>();
        particleRenderer.renderMode = renderMode;
        particleRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        particleRenderer.receiveShadows = false;
        // An instance, so darkening at night never writes into the material asset.
        instance = material != null ? new Material(material) : null;
        particleRenderer.sharedMaterial = instance;
        return system;
    }

    static float PerWeather(float[] values, WeatherType weather)
    {
        int index = (int)weather;
        return values != null && index < values.Length ? Mathf.Max(0f, values[index]) : 0f;
    }

    static float MaxOf(float[] values)
    {
        float max = 1f;
        if (values != null)
        {
            foreach (float value in values)
                max = Mathf.Max(max, value);
        }
        return max;
    }
}
