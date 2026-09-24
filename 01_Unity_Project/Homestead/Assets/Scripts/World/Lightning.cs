using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

// Lightning and thunder during a Thunderstorm (Weather_System.md: Visual Feedback). Most strikes are distant: a dim
// flash low on the horizon and, after a long delay, a dull rumble. Now and then one lands close: a bright flash, a
// visible bolt, and a sharp crack right behind it. Thunder arrives after the flash at the speed of sound. The clip
// is a long storm recording, so each strike plays one of AudioManager's thunder slices. Presentation only.
public class Lightning : MonoBehaviour
{
    const float SpeedOfSound = 343f;

    [Header("Timing")]
    [Tooltip("Seconds between strikes during a Thunderstorm (random within this range, game time).")]
    [SerializeField] Vector2 secondsBetweenStrikes = new Vector2(8f, 30f);
    [Tooltip("Chance a strike lands close enough to see the bolt.")]
    [SerializeField, Range(0f, 1f)] float closeChance = 0.2f;
    [SerializeField] Vector2 closeDistance = new Vector2(150f, 500f);
    [SerializeField] Vector2 distantDistance = new Vector2(1500f, 4000f);

    [Header("Flash")]
    [Tooltip("Peak brightness of a close flash, in lux.")]
    [SerializeField, Min(0f)] float closeFlashLux = 50000f;
    [Tooltip("Peak brightness of a distant flash, in lux.")]
    [SerializeField, Min(0f)] float distantFlashLux = 6000f;
    [SerializeField] Material boltMaterial;
    [SerializeField, Min(10f)] float boltHeight = 250f;

    [Header("Thunder")]
    [Tooltip("Low-pass cutoff for the nearest and farthest strikes, in Hz — distant thunder loses its crack.")]
    [SerializeField] Vector2 thunderCutoff = new Vector2(22000f, 600f);
    [Tooltip("Volume for the nearest and farthest strikes.")]
    [SerializeField] Vector2 thunderVolume = new Vector2(1f, 0.35f);
    [SerializeField, Min(0.01f)] float thunderFadeOutSeconds = 1.5f;

    Light flash;
    LineRenderer bolt;
    AudioSource[] thunderSources;
    AudioLowPassFilter[] thunderFilters;
    int[] thunderPlays; // bumped each time a source is reused, so an older strike's fade-out leaves it alone
    int nextThunderSource = -1;
    int lastSlice = -1;
    float nextStrike;

    void Awake()
    {
        var flashObject = new GameObject("Lightning Flash");
        flashObject.transform.SetParent(transform, false);
        flash = flashObject.AddComponent<Light>();
        flash.type = LightType.Directional;
        flash.shadows = LightShadows.None;
        flash.color = new Color(0.85f, 0.9f, 1f);
        flash.intensity = 0f;
        flash.enabled = false;
        flashObject.AddComponent<HDAdditionalLightData>();

        var boltObject = new GameObject("Lightning Bolt");
        boltObject.transform.SetParent(transform, false);
        bolt = boltObject.AddComponent<LineRenderer>();
        bolt.useWorldSpace = true;
        bolt.sharedMaterial = boltMaterial;
        bolt.widthMultiplier = 2.5f;
        bolt.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        bolt.receiveShadows = false;
        bolt.enabled = false;

        thunderSources = new AudioSource[2];
        thunderFilters = new AudioLowPassFilter[2];
        thunderPlays = new int[2];
        for (int i = 0; i < thunderSources.Length; i++)
        {
            var sourceObject = new GameObject("Thunder " + (i + 1));
            sourceObject.transform.SetParent(transform, false);
            thunderSources[i] = sourceObject.AddComponent<AudioSource>();
            thunderSources[i].playOnAwake = false;
            thunderSources[i].spatialBlend = 0f;
            thunderFilters[i] = sourceObject.AddComponent<AudioLowPassFilter>();
        }
    }

    void Start()
    {
        AudioManager audio = AudioManager.Instance;
        foreach (AudioSource source in thunderSources)
            source.outputAudioMixerGroup = audio != null ? audio.GetGroup(AudioChannel.Ambient) : null;
        ScheduleNext();
    }

    void Update()
    {
        WeatherManager weather = WeatherManager.Instance;
        GameManager game = GameManager.Instance;
        bool storming = weather != null && weather.Current == WeatherType.Thunderstorm &&
                        (game == null || game.State == GameState.Playing);
        if (!storming)
        {
            ScheduleNext(); // the first strike of a new storm doesn't land the instant it starts
            return;
        }

        if (Time.time >= nextStrike)
        {
            ScheduleNext();
            StartCoroutine(Strike(Random.value < closeChance));
        }
    }

    void ScheduleNext() =>
        nextStrike = Time.time + Random.Range(secondsBetweenStrikes.x, secondsBetweenStrikes.y);

    IEnumerator Strike(bool close)
    {
        float distance = close ? Random.Range(closeDistance.x, closeDistance.y)
                               : Random.Range(distantDistance.x, distantDistance.y);
        float bearing = Random.Range(0f, 360f);
        Vector3 toward = Quaternion.Euler(0f, bearing, 0f) * Vector3.forward;

        // Close strikes light the scene from high up; distant ones glow from low on the horizon.
        float elevation = close ? Random.Range(45f, 70f) : Random.Range(3f, 12f);
        flash.transform.rotation = Quaternion.LookRotation(-toward) * Quaternion.Euler(elevation, 0f, 0f);
        float peak = close ? closeFlashLux : distantFlashLux * Random.Range(0.6f, 1.2f);

        if (close)
            PlaceBolt(toward, distance);

        float far = Mathf.InverseLerp(closeDistance.x, distantDistance.y, distance);
        StartCoroutine(Thunder(distance / SpeedOfSound, far));

        // A few quick pulses, like a real flash flickering.
        int pulses = Random.Range(2, 4);
        for (int i = 0; i < pulses; i++)
        {
            flash.enabled = true;
            flash.intensity = peak * (i == 0 ? 1f : Random.Range(0.4f, 0.9f));
            bolt.enabled = close;
            yield return new WaitForSeconds(Random.Range(0.04f, 0.1f));
            flash.enabled = false;
            bolt.enabled = false;
            yield return new WaitForSeconds(Random.Range(0.04f, 0.12f));
        }
    }

    void PlaceBolt(Vector3 toward, float distance)
    {
        Camera main = Camera.main;
        Vector3 origin = main != null ? main.transform.position : transform.position;
        Vector3 ground = origin + toward * distance;
        Terrain terrain = Terrain.activeTerrain;
        ground.y = terrain != null ? terrain.SampleHeight(ground) + terrain.transform.position.y : origin.y;

        const int Segments = 14;
        bolt.positionCount = Segments + 1;
        Vector3 top = ground + Vector3.up * boltHeight + Random.insideUnitSphere * 20f;
        for (int i = 0; i <= Segments; i++)
        {
            float t = i / (float)Segments;
            Vector3 point = Vector3.Lerp(top, ground, t);
            if (i > 0 && i < Segments)
                point += new Vector3(Random.Range(-8f, 8f), 0f, Random.Range(-8f, 8f));
            bolt.SetPosition(i, point);
        }
    }

    IEnumerator Thunder(float delay, float far)
    {
        yield return new WaitForSeconds(delay);

        AudioManager audio = AudioManager.Instance;
        if (audio == null || audio.Thunder.clip == null)
            yield break;

        AudioClip clip = audio.Thunder.clip;
        int sliceCount = audio.ThunderSlices.Count;
        Vector2 slice = new Vector2(0f, clip.length);
        if (sliceCount > 0)
        {
            int index = Random.Range(0, sliceCount);
            if (sliceCount > 1 && index == lastSlice)
                index = (index + 1) % sliceCount;
            lastSlice = index;
            slice = audio.ThunderSlices[index];
        }

        nextThunderSource = (nextThunderSource + 1) % thunderSources.Length;
        int sourceIndex = nextThunderSource;
        int play = ++thunderPlays[sourceIndex];
        AudioSource source = thunderSources[sourceIndex];
        thunderFilters[nextThunderSource].cutoffFrequency = Mathf.Lerp(thunderCutoff.x, thunderCutoff.y, far);
        float volume = audio.Thunder.volume * Mathf.Lerp(thunderVolume.x, thunderVolume.y, far);

        source.Stop();
        source.clip = clip;
        source.volume = volume;
        source.time = Mathf.Clamp(slice.x, 0f, clip.length - 0.1f);
        source.Play();

        // Hold, then fade out at the end of the slice so it doesn't cut off mid-rumble.
        float hold = Mathf.Max(0f, slice.y - thunderFadeOutSeconds);
        yield return new WaitForSeconds(hold);
        for (float t = 0f; t < thunderFadeOutSeconds; t += Time.deltaTime)
        {
            if (thunderPlays[sourceIndex] != play)
                yield break;
            source.volume = volume * (1f - t / thunderFadeOutSeconds);
            yield return null;
        }
        if (thunderPlays[sourceIndex] == play)
            source.Stop();
    }
}
