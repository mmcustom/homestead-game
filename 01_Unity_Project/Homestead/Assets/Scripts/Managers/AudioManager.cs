using System;
using System.Collections.Generic;
using UnityEngine;

public enum AudioChannel { Master, Music, Ambient, Sfx }

// A looping ambient sound that plays while its conditions match, e.g. crickets at Night in Summer,
// or rain during LightRain/HeavyRain/Thunderstorm. An empty condition list means "any".
[Serializable]
public class AmbientLayer
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    public List<Season> seasons = new List<Season>();
    public List<DayPhase> phases = new List<DayPhase>();
    public List<WeatherType> weather = new List<WeatherType>();

    [NonSerialized] public AudioSource source;

    // 0–1 fade position, separate from the output volume so a muted channel still fades and stops layers.
    [NonSerialized] public float level;

    // Tracked here rather than read from AudioSource.isPlaying, which is false while audio is paused —
    // relying on it would restart the loop from the beginning every frame during a pause.
    [NonSerialized] public bool started;

    public bool Matches(Season season, DayPhase phase, WeatherType currentWeather) =>
        (seasons.Count == 0 || seasons.Contains(season)) &&
        (phases.Count == 0 || phases.Contains(phase)) &&
        (weather.Count == 0 || weather.Contains(currentWeather));
}

// Music, ambient effects, sound effects (Unity_Architecture.md).
// Music follows GameManager's state (menu vs. world track, crossfaded) and keeps playing while paused.
// Ambient layers follow TimeManager's season/phase and WeatherManager's weather, and only play in-game.
// Volume settings are stored in PlayerPrefs — they're player preferences, not part of a save slot.
public class AudioManager : MonoBehaviour
{
    const string VolumePrefPrefix = "audio.volume.";

    public static AudioManager Instance { get; private set; }

    [Header("Music")]
    [SerializeField] AudioClip menuMusic;
    [SerializeField] AudioClip worldMusic;
    [SerializeField, Min(0f)] float musicFadeSeconds = 2f;

    [Header("Ambient")]
    [SerializeField] List<AmbientLayer> ambientLayers = new List<AmbientLayer>();
    [SerializeField, Min(0f)] float ambientFadeSeconds = 3f;

    [Header("Sound Effects")]
    [Tooltip("How many effects can play at once; the oldest is cut off when all are busy.")]
    [SerializeField, Min(1)] int sfxVoices = 12;

    readonly Dictionary<AudioChannel, float> volumes = new Dictionary<AudioChannel, float>();
    readonly AudioSource[] musicSources = new AudioSource[2];
    readonly float[] musicLevels = new float[2];
    readonly List<AudioSource> sfxSources = new List<AudioSource>();
    AudioSource uiSource;
    int activeMusic;
    int nextSfx;
    bool subscribed;

    public AudioClip CurrentMusic => musicSources[activeMusic] != null ? musicSources[activeMusic].clip : null;

    static bool InGame =>
        GameManager.Instance != null &&
        (GameManager.Instance.State == GameState.Playing || GameManager.Instance.State == GameState.Paused);

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        foreach (AudioChannel channel in Enum.GetValues(typeof(AudioChannel)))
            volumes[channel] = Mathf.Clamp01(PlayerPrefs.GetFloat(VolumePrefPrefix + channel, 1f));

        for (int i = 0; i < musicSources.Length; i++)
        {
            musicSources[i] = CreateSource("Music " + (i + 1), loop: true);
            musicSources[i].ignoreListenerPause = true;
        }

        foreach (AmbientLayer layer in ambientLayers)
        {
            if (layer.clip == null)
                continue;

            layer.source = CreateSource("Ambient " + layer.name, loop: true);
            layer.source.clip = layer.clip;
        }

        for (int i = 0; i < sfxVoices; i++)
            sfxSources.Add(CreateSource("SFX " + (i + 1), loop: false));

        uiSource = CreateSource("UI", loop: false);
        uiSource.ignoreListenerPause = true;
    }

    void Start()
    {
        if (Instance != this)
            return;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.StateChanged += OnGameStateChanged;
            subscribed = true;
            OnGameStateChanged(GameManager.Instance.State);
        }
    }

    void OnDestroy()
    {
        if (Instance != this)
            return;

        if (subscribed && GameManager.Instance != null)
            GameManager.Instance.StateChanged -= OnGameStateChanged;
        AudioListener.pause = false;
        Instance = null;
    }

    // Unscaled time so fades keep running while the game is paused (timeScale 0).
    void Update()
    {
        float dt = Time.unscaledDeltaTime;
        UpdateMusic(dt);
        UpdateAmbient(dt);
    }

    public float GetVolume(AudioChannel channel) => volumes[channel];

    public void SetVolume(AudioChannel channel, float volume)
    {
        volumes[channel] = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(VolumePrefPrefix + channel, volumes[channel]);
        PlayerPrefs.Save();
    }

    // Crossfades to a track. Null fades music out. Playing the current track again does nothing.
    public void PlayMusic(AudioClip clip)
    {
        if (musicSources[activeMusic].clip == clip)
            return;

        activeMusic = 1 - activeMusic;
        AudioSource next = musicSources[activeMusic];
        next.Stop();
        next.clip = clip;
        musicLevels[activeMusic] = 0f;
        if (clip != null)
            next.Play();
    }

    // A non-positional effect, e.g. picking up an item.
    public void PlaySfx(AudioClip clip, float volume = 1f, float pitchVariance = 0f) =>
        PlayOn(NextSfxSource(), clip, volume, pitchVariance, null);

    // An effect heard from a point in the world, e.g. a gunshot or a splash.
    public void PlaySfxAt(AudioClip clip, Vector3 position, float volume = 1f, float pitchVariance = 0f) =>
        PlayOn(NextSfxSource(), clip, volume, pitchVariance, position);

    // Menu sounds — still heard while the game is paused.
    public void PlayUi(AudioClip clip, float volume = 1f)
    {
        if (clip != null)
            uiSource.PlayOneShot(clip, Mathf.Clamp01(volume) * ChannelVolume(AudioChannel.Sfx));
    }

    void OnGameStateChanged(GameState state)
    {
        // Gameplay audio stops while paused; music and UI sounds ignore the listener pause.
        AudioListener.pause = state == GameState.Paused;

        if (state == GameState.MainMenu)
            PlayMusic(menuMusic);
        else if (state == GameState.Playing)
            PlayMusic(worldMusic);
    }

    void UpdateMusic(float dt)
    {
        float step = musicFadeSeconds > 0f ? dt / musicFadeSeconds : 1f;
        for (int i = 0; i < musicSources.Length; i++)
        {
            AudioSource source = musicSources[i];
            float target = i == activeMusic && source.clip != null ? 1f : 0f;
            musicLevels[i] = Mathf.MoveTowards(musicLevels[i], target, step);
            source.volume = musicLevels[i] * ChannelVolume(AudioChannel.Music);

            if (musicLevels[i] <= 0f && i != activeMusic && source.clip != null)
            {
                source.Stop();
                source.clip = null;
            }
        }
    }

    void UpdateAmbient(float dt)
    {
        TimeManager time = TimeManager.Instance;
        WeatherManager weather = WeatherManager.Instance;
        bool active = InGame && time != null;
        float step = ambientFadeSeconds > 0f ? dt / ambientFadeSeconds : 1f;

        foreach (AmbientLayer layer in ambientLayers)
        {
            AudioSource source = layer.source;
            if (source == null)
                continue;

            bool matches = active && layer.Matches(time.CurrentSeason, time.Phase,
                                                   weather != null ? weather.Current : WeatherType.Clear);
            layer.level = Mathf.MoveTowards(layer.level, matches ? 1f : 0f, step);

            if (layer.level > 0f && !layer.started)
            {
                source.Play();
                layer.started = true;
            }
            else if (layer.level <= 0f && layer.started)
            {
                source.Stop();
                layer.started = false;
            }

            source.volume = layer.level * layer.volume * ChannelVolume(AudioChannel.Ambient);
        }
    }

    float ChannelVolume(AudioChannel channel) =>
        channel == AudioChannel.Master ? volumes[AudioChannel.Master] : volumes[channel] * volumes[AudioChannel.Master];

    AudioSource NextSfxSource()
    {
        // Prefer an idle voice; if all are busy, reuse them in turn (cutting off the oldest).
        for (int i = 0; i < sfxSources.Count; i++)
        {
            AudioSource candidate = sfxSources[(nextSfx + i) % sfxSources.Count];
            if (!candidate.isPlaying)
            {
                nextSfx = (nextSfx + i + 1) % sfxSources.Count;
                return candidate;
            }
        }

        AudioSource oldest = sfxSources[nextSfx];
        nextSfx = (nextSfx + 1) % sfxSources.Count;
        return oldest;
    }

    void PlayOn(AudioSource source, AudioClip clip, float volume, float pitchVariance, Vector3? position)
    {
        if (clip == null)
            return;

        source.Stop();
        source.clip = clip;
        source.volume = Mathf.Clamp01(volume) * ChannelVolume(AudioChannel.Sfx);
        source.pitch = 1f + UnityEngine.Random.Range(-pitchVariance, pitchVariance);
        source.spatialBlend = position.HasValue ? 1f : 0f;
        source.transform.position = position ?? transform.position;
        source.Play();
    }

    AudioSource CreateSource(string sourceName, bool loop)
    {
        var child = new GameObject(sourceName);
        child.transform.SetParent(transform, false);

        var source = child.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = loop;
        source.spatialBlend = 0f;
        source.volume = 0f;
        return source;
    }
}
