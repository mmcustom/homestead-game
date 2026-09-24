using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum AudioChannel { Master, Music, Ambient, Sfx, Ui }

// Audio_System.md's one-shot sounds.
public enum SoundCue { UiClick, UiBack, DiscoveryChime, JournalUpdated, Milestone, ItemPickup, ItemDrop }

// A clip and the level it plays at within its mixer group. Levels start from each file's measured loudness,
// so sounds sourced from different libraries sit sensibly together; adjust by ear in the Inspector.
[Serializable]
public class Sound
{
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
}

// A looping ambient sound that plays while its conditions match. An empty condition list means "any".
[Serializable]
public class AmbientLayer
{
    public string name;
    public Sound sound = new Sound();
    public List<Season> seasons = new List<Season>();
    public List<DayPhase> phases = new List<DayPhase>();
    public List<WeatherType> weather = new List<WeatherType>();

    [Tooltip("Volume multiplier per season (Spring, Summer, Fall, Winter).")]
    public float[] seasonVolume = { 1f, 1f, 1f, 1f };

    [Tooltip("Scale with WeatherManager.WindStrength rather than switching on and off (Audio_System.md: ambient_wind).")]
    public bool scaleWithWind;

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

// Music, ambient effects, sound effects (Unity_Architecture.md, Audio_System.md).
// Everything routes through Homestead.mixer's Music / Ambient / SFX / UI groups, whose exposed volumes hold each
// group's base gain plus the player's volume setting (PlayerPrefs — a preference, not part of a save slot).
public class AudioManager : MonoBehaviour
{
    const string VolumePrefPrefix = "audio.volume.";
    const float SilentDb = -80f;

    public static AudioManager Instance { get; private set; }

    [Header("Mixer")]
    [SerializeField] AudioMixer mixer;
    [Tooltip("Fixed gain per group in dB, before the player's volume setting. Lets quiet source files reach their " +
             "target level, since AudioSource volume can only attenuate.")]
    [SerializeField] float musicGainDb;
    [SerializeField] float ambientGainDb;
    [SerializeField] float sfxGainDb;
    [SerializeField] float uiGainDb;

    [Header("Music")]
    [SerializeField] Sound menuMusic = new Sound();
    [SerializeField] Sound dayMusic = new Sound();
    [SerializeField] Sound nightMusic = new Sound();
    [SerializeField, Min(0f)] float musicFadeSeconds = 2f;

    [Header("Ambient")]
    [SerializeField] List<AmbientLayer> ambientLayers = new List<AmbientLayer>();
    [SerializeField, Min(0f)] float ambientFadeSeconds = 3f;
    [Tooltip("Plays at each discovered Water Source site, fading with distance (Audio_System.md: ambient_water_proximity).")]
    [SerializeField] Sound waterProximity = new Sound();
    [SerializeField, Min(1f)] float waterAudibleDistance = 25f;

    [Header("Player")]
    [SerializeField] Sound footstepsGrass = new Sound();
    [SerializeField] Sound footstepsDirt = new Sound();
    [SerializeField] Sound footstepsGravel = new Sound();
    [SerializeField] Sound sprintBreathing = new Sound();
    [SerializeField] Sound encumberedBreathing = new Sound();

    [Header("One-shots")]
    [SerializeField] Sound itemPickup = new Sound();
    [SerializeField] Sound itemDrop = new Sound();
    [SerializeField] Sound uiClick = new Sound();
    [SerializeField] Sound uiBack = new Sound();
    [SerializeField] Sound discoveryChime = new Sound();
    [SerializeField] Sound journalUpdated = new Sound();
    [SerializeField] Sound milestone = new Sound();
    [Tooltip("How many effects can play at once; the oldest is cut off when all are busy.")]
    [SerializeField, Min(1)] int sfxVoices = 12;

    readonly Dictionary<AudioChannel, float> volumes = new Dictionary<AudioChannel, float>();
    readonly Dictionary<AudioChannel, AudioMixerGroup> groups = new Dictionary<AudioChannel, AudioMixerGroup>();
    readonly AudioSource[] musicSources = new AudioSource[2];
    readonly Sound[] musicSounds = new Sound[2];
    readonly float[] musicLevels = new float[2];
    readonly List<AudioSource> sfxSources = new List<AudioSource>();
    readonly Dictionary<string, AudioSource> waterSources = new Dictionary<string, AudioSource>();
    readonly HashSet<string> knownWaterSites = new HashSet<string>();
    readonly List<string> staleWaterSites = new List<string>();
    AudioSource uiSource;
    int activeMusic;
    int nextSfx;
    bool pendingDiscoveryChime;
    bool pendingMilestone;
    bool volumesApplied;
    InventoryContainer watchedInventory;

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

        if (mixer != null)
        {
            groups[AudioChannel.Master] = FindGroup("Master");
            groups[AudioChannel.Music] = FindGroup("Music");
            groups[AudioChannel.Ambient] = FindGroup("Ambient");
            groups[AudioChannel.Sfx] = FindGroup("SFX");
            groups[AudioChannel.Ui] = FindGroup("UI");
        }
        else
        {
            Debug.LogWarning("[AudioManager] No mixer assigned; channel volumes won't apply.", this);
        }

        for (int i = 0; i < musicSources.Length; i++)
        {
            musicSources[i] = CreateSource("Music " + (i + 1), AudioChannel.Music, loop: true);
            musicSources[i].ignoreListenerPause = true;
        }

        foreach (AmbientLayer layer in ambientLayers)
        {
            if (layer.sound.clip == null)
                continue;

            layer.source = CreateSource("Ambient " + layer.name, AudioChannel.Ambient, loop: true);
            layer.source.clip = layer.sound.clip;
        }

        for (int i = 0; i < sfxVoices; i++)
            sfxSources.Add(CreateSource("SFX " + (i + 1), AudioChannel.Sfx, loop: false));

        uiSource = CreateSource("UI", AudioChannel.Ui, loop: false);
        uiSource.ignoreListenerPause = true;
    }

    void Start()
    {
        if (Instance != this)
            return;

        if (GameManager.Instance != null)
            GameManager.Instance.StateChanged += OnGameStateChanged;

        if (DiscoveryManager.Instance != null)
        {
            DiscoveryManager.Instance.Discovered += OnDiscovered;
            DiscoveryManager.Instance.MilestoneReached += OnMilestoneReached;
        }

        if (InventoryManager.Instance != null)
        {
            watchedInventory = InventoryManager.Instance.Player;
            watchedInventory.ItemsAdded += OnItemsAdded;
            watchedInventory.ItemsRemoved += OnItemsRemoved;
        }

        if (GameManager.Instance != null)
            OnGameStateChanged(GameManager.Instance.State);
    }

    void OnDestroy()
    {
        if (Instance != this)
            return;

        if (GameManager.Instance != null)
            GameManager.Instance.StateChanged -= OnGameStateChanged;
        if (DiscoveryManager.Instance != null)
        {
            DiscoveryManager.Instance.Discovered -= OnDiscovered;
            DiscoveryManager.Instance.MilestoneReached -= OnMilestoneReached;
        }
        if (watchedInventory != null)
        {
            watchedInventory.ItemsAdded -= OnItemsAdded;
            watchedInventory.ItemsRemoved -= OnItemsRemoved;
        }

        AudioListener.pause = false;
        Instance = null;
    }

    // Unscaled time so music keeps fading while the game is paused (timeScale 0).
    void Update()
    {
        // Mixer parameters can only be set at runtime, so apply saved volumes on the first frame.
        if (!volumesApplied)
        {
            ApplyVolumes();
            volumesApplied = true;
        }

        float dt = Time.unscaledDeltaTime;
        UpdateMusic(dt);
        UpdateAmbient(dt);
        UpdateWaterSources();
    }

    // A discovery that is also a first-of-category milestone plays the milestone sound instead of the chime —
    // the bigger moment wins rather than both sounding at once.
    void LateUpdate()
    {
        if (pendingMilestone)
            Play(SoundCue.Milestone);
        else if (pendingDiscoveryChime)
            Play(SoundCue.DiscoveryChime);

        pendingMilestone = false;
        pendingDiscoveryChime = false;
    }

    public float GetVolume(AudioChannel channel) => volumes[channel];

    public void SetVolume(AudioChannel channel, float volume)
    {
        volumes[channel] = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(VolumePrefPrefix + channel, volumes[channel]);
        PlayerPrefs.Save();
        ApplyVolumes();
    }

    public AudioMixerGroup GetGroup(AudioChannel channel) =>
        groups.TryGetValue(channel, out AudioMixerGroup group) ? group : null;

    public Sound Footsteps(SurfaceType surface)
    {
        switch (surface)
        {
            case SurfaceType.Dirt: return footstepsDirt;
            case SurfaceType.Gravel: return footstepsGravel;
            default: return footstepsGrass;
        }
    }

    public Sound SprintBreathing => sprintBreathing;
    public Sound EncumberedBreathing => encumberedBreathing;

    public void Play(SoundCue cue)
    {
        Sound sound = SoundFor(cue);
        if (sound == null || sound.clip == null)
            return;

        if (cue == SoundCue.ItemPickup || cue == SoundCue.ItemDrop)
            PlayOn(NextSfxSource(), sound, null);
        else
            uiSource.PlayOneShot(sound.clip, sound.volume); // UI group; still heard while paused
    }

    // An effect heard from a point in the world.
    public void PlayAt(Sound sound, Vector3 position)
    {
        if (sound != null && sound.clip != null)
            PlayOn(NextSfxSource(), sound, position);
    }

    // Crossfades to a track. Playing the current track again does nothing.
    public void PlayMusic(Sound sound)
    {
        AudioClip clip = sound != null ? sound.clip : null;
        if (musicSources[activeMusic].clip == clip)
            return;

        activeMusic = 1 - activeMusic;
        AudioSource next = musicSources[activeMusic];
        next.Stop();
        next.clip = clip;
        musicSounds[activeMusic] = sound;
        musicLevels[activeMusic] = 0f;
        if (clip != null)
            next.Play();
    }

    Sound SoundFor(SoundCue cue)
    {
        switch (cue)
        {
            case SoundCue.UiClick: return uiClick;
            case SoundCue.UiBack: return uiBack;
            case SoundCue.DiscoveryChime: return discoveryChime;
            case SoundCue.JournalUpdated: return journalUpdated;
            case SoundCue.Milestone: return milestone;
            case SoundCue.ItemPickup: return itemPickup;
            case SoundCue.ItemDrop: return itemDrop;
            default: return null;
        }
    }

    void OnGameStateChanged(GameState state)
    {
        // Gameplay audio stops while paused; music and UI sounds ignore the listener pause.
        AudioListener.pause = state == GameState.Paused;

        if (state == GameState.MainMenu)
            PlayMusic(menuMusic);
    }

    void OnDiscovered(DiscoveryRecord record) => pendingDiscoveryChime = true;
    void OnMilestoneReached(DiscoveryMilestone reached) => pendingMilestone = true;

    void OnItemsAdded(string itemId, int quantity)
    {
        if (InGame)
            Play(SoundCue.ItemPickup);
    }

    void OnItemsRemoved(string itemId, int quantity)
    {
        if (InGame)
            Play(SoundCue.ItemDrop);
    }

    void ApplyVolumes()
    {
        if (mixer == null)
            return;

        mixer.SetFloat("MasterVolume", ToDb(volumes[AudioChannel.Master]));
        mixer.SetFloat("MusicVolume", musicGainDb + ToDb(volumes[AudioChannel.Music]));
        mixer.SetFloat("AmbientVolume", ambientGainDb + ToDb(volumes[AudioChannel.Ambient]));
        mixer.SetFloat("SfxVolume", sfxGainDb + ToDb(volumes[AudioChannel.Sfx]));
        mixer.SetFloat("UiVolume", uiGainDb + ToDb(volumes[AudioChannel.Ui]));
    }

    static float ToDb(float linear) => linear > 0.0001f ? 20f * Mathf.Log10(linear) : SilentDb;

    // Audio_System.md: menu theme on the main menu; the day or night track in World by the in-game clock.
    void UpdateMusic(float dt)
    {
        TimeManager time = TimeManager.Instance;
        if (InGame && time != null)
            PlayMusic(time.IsDaylight ? dayMusic : nightMusic);

        float step = musicFadeSeconds > 0f ? dt / musicFadeSeconds : 1f;
        for (int i = 0; i < musicSources.Length; i++)
        {
            AudioSource source = musicSources[i];
            float target = i == activeMusic && source.clip != null ? 1f : 0f;
            musicLevels[i] = Mathf.MoveTowards(musicLevels[i], target, step);
            source.volume = musicLevels[i] * (musicSounds[i] != null ? musicSounds[i].volume : 1f);

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

            float seasonScale = time != null && layer.seasonVolume != null && layer.seasonVolume.Length == 4
                ? layer.seasonVolume[(int)time.CurrentSeason]
                : 1f;
            // Baseline wind (~0.2–0.4) gives a quiet bed; the Wind weather type (0.8+) brings it up close to full.
            float windScale = layer.scaleWithWind && weather != null
                ? Mathf.InverseLerp(0.1f, 1f, weather.WindStrength)
                : 1f;

            source.volume = layer.level * layer.sound.volume * seasonScale * windScale;
        }
    }

    // One positional loop per discovered Water Source, audible only in-game. Sites that are no longer known
    // (e.g. after starting a new game) lose their source.
    void UpdateWaterSources()
    {
        DiscoveryManager discovery = DiscoveryManager.Instance;
        bool active = InGame && discovery != null && waterProximity.clip != null;

        knownWaterSites.Clear();
        if (active)
        {
            foreach (DiscoveryRecord record in discovery.InCategory(DiscoveryCategory.WaterSource))
            {
                knownWaterSites.Add(record.siteId);
                if (waterSources.ContainsKey(record.siteId))
                    continue;

                AudioSource source = CreateSource("Water " + record.displayName, AudioChannel.Ambient, loop: true);
                source.transform.position = record.position;
                source.clip = waterProximity.clip;
                source.volume = waterProximity.volume;
                source.spatialBlend = 1f;
                source.rolloffMode = AudioRolloffMode.Linear;
                source.minDistance = 2f;
                source.maxDistance = waterAudibleDistance;
                source.Play();
                waterSources.Add(record.siteId, source);
            }
        }

        if (waterSources.Count == knownWaterSites.Count)
            return;

        staleWaterSites.Clear();
        foreach (string siteId in waterSources.Keys)
        {
            if (!knownWaterSites.Contains(siteId))
                staleWaterSites.Add(siteId);
        }

        foreach (string siteId in staleWaterSites)
        {
            Destroy(waterSources[siteId].gameObject);
            waterSources.Remove(siteId);
        }
    }

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

    void PlayOn(AudioSource source, Sound sound, Vector3? position)
    {
        source.Stop();
        source.clip = sound.clip;
        source.volume = sound.volume;
        source.pitch = 1f;
        source.spatialBlend = position.HasValue ? 1f : 0f;
        source.transform.position = position ?? transform.position;
        source.Play();
    }

    AudioSource CreateSource(string sourceName, AudioChannel channel, bool loop)
    {
        var child = new GameObject(sourceName);
        child.transform.SetParent(transform, false);

        var source = child.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = loop;
        source.spatialBlend = 0f;
        source.volume = 0f;
        source.outputAudioMixerGroup = GetGroup(channel);
        return source;
    }

    AudioMixerGroup FindGroup(string groupName)
    {
        foreach (AudioMixerGroup group in mixer.FindMatchingGroups(groupName))
        {
            if (group.name == groupName)
                return group;
        }

        Debug.LogWarning($"[AudioManager] Mixer group '{groupName}' not found.", this);
        return null;
    }
}
