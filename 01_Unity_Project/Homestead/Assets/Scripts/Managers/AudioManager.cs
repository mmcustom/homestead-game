using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum AudioChannel { Master, Music, Ambient, Sfx, Ui }

// Audio_System.md's one-shot sounds.
public enum SoundCue { UiClick, UiBack, DiscoveryChime, JournalUpdated, Milestone, ItemPickup, ItemDrop, Drink, Eat, UpsetStomach, Vomit, Forage }

// A clip and the level it plays at within its mixer group. Levels start from each file's measured loudness,
// so sounds sourced from different libraries sit sensibly together; adjust by ear in the Inspector.
[Serializable]
public class Sound
{
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    [Tooltip("Seconds into the clip to start from, to skip silence at the front of a sourced file.")]
    [Min(0f)] public float startTime;
    [Tooltip("For looped sounds: seconds into the clip where the loop wraps back to Start Time (0 = the clip's end).")]
    [Min(0f)] public float loopEnd;
    [Tooltip("For one-shots: stop after this many seconds, fading out over Fade Out (0 = play the whole clip).")]
    [Min(0f)] public float maxDuration;
    [Min(0f)] public float fadeOut = 0.2f;
    [Tooltip("Playback speed and pitch together; above 1 is quicker and snappier.")]
    [Range(0.5f, 2f)] public float pitch = 1f;
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

    [Tooltip("Quieter under tree canopy or a roof (Audio_System.md: rain and snow ambience under cover).")]
    public bool softenUnderCover;

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
    [Tooltip("Volume of cover-softened ambience (rain, snow) under a full tree canopy, and under a roof.")]
    [SerializeField, Range(0f, 1f)] float canopyAmbientVolume = 0.75f;
    [SerializeField, Range(0f, 1f)] float roofAmbientVolume = 0.4f;
    [Tooltip("Plays at each discovered Water Source site, fading with distance (Audio_System.md: ambient_water_proximity).")]
    [SerializeField] Sound waterProximity = new Sound();
    [SerializeField, Min(1f)] float waterAudibleDistance = 25f;
    [Tooltip("sfx_thunder (Audio_System.md). Played by Lightning, a slice at a time.")]
    [SerializeField] Sound thunder = new Sound();
    [Tooltip("Stretches of the thunder clip that each hold one thunderclap, as (start, length) in seconds. The clip is " +
             "a long storm recording, so each strike plays a different slice rather than the whole file.")]
    [SerializeField] List<Vector2> thunderSlices = new List<Vector2>();

    [Header("Player")]
    [SerializeField] Sound footstepsGrass = new Sound();
    [SerializeField] Sound footstepsDirt = new Sound();
    [SerializeField] Sound footstepsGravel = new Sound();
    [Tooltip("Where each footfall starts in the footstep recordings, in seconds. The recordings are sequences of steps " +
             "at their own pace; PlayerAudio plays one footfall per stride instead of looping them.")]
    [SerializeField] List<float> footstepsGrassTimes = new List<float>();
    [SerializeField] List<float> footstepsDirtTimes = new List<float>();
    [SerializeField] List<float> footstepsGravelTimes = new List<float>();
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
    [Tooltip("Audio_System.md's sfx_drink: drinking at a water source or from carried water.")]
    [SerializeField] Sound drink = new Sound();
    [Tooltip("Audio_System.md's sfx_eating: eating food from the Inventory screen.")]
    [SerializeField] Sound eating = new Sound();
    [Tooltip("Audio_System.md's sfx_foraging: harvesting a forage patch (foraging.wav), in place of the pickup sound.")]
    [SerializeField] Sound foraging = new Sound();
    [Tooltip("sfx_chop_tree / sfx_chop_firewood: one axe blow (chopping-wood.wav). Each blow plays one chop from the " +
             "recording, starting at one of Chop Times and lasting Max Duration.")]
    [SerializeField] Sound chop = new Sound();
    [Tooltip("Where each single, clean chop starts in the chopping recording, in seconds.")]
    [SerializeField] List<float> chopTimes = new List<float>();
    [Tooltip("sfx_tree_fall: a felled tree creaking over and crashing down (tree-fall.wav).")]
    [SerializeField] Sound treeFall = new Sound();
    [Tooltip("Seconds into the tree-fall recording where the crash hits, so it can be cued to land with the tree.")]
    [SerializeField, Min(0f)] float treeFallImpact = 4.6f;
    [Tooltip("Meat or fish sizzling at the campfire, looped from the fire while it cooks (cooking.mp3).")]
    [SerializeField] Sound cooking = new Sound();
    [Tooltip("Water boiling in the bucket at the campfire, looped from the fire while it boils (boiling_water.wav).")]
    [SerializeField] Sound boiling = new Sound();
    [Tooltip("Sickness while Mild: the stomach turning on falling ill, and grumbling now and then (Upset stomach.wav).")]
    [SerializeField] Sound upsetStomach = new Sound();
    [Tooltip("Sickness while Very Sick: vomiting (vomiting.wav).")]
    [SerializeField] Sound vomit = new Sound();
    // Audio_System.md's Hunting / Fishing / Trapping SFX, played in the world by the tools that make them.
    [Header("Hunting, fishing and trapping")]
    [Tooltip("sfx_gunshot: the Bolt-Action Rifle fired.")]
    [SerializeField] Sound gunshot = new Sound();
    [Tooltip("sfx_bow_release: the Recurve Bow fired.")]
    [SerializeField] Sound bowRelease = new Sound();
    [Tooltip("sfx_fish_bite: the bobber dipping when a fish bites.")]
    [SerializeField] Sound fishBite = new Sound();
    [Tooltip("sfx_splash: the line landing, a shot duck dropping, a fish trap going in, stepping into water.")]
    [SerializeField] Sound splash = new Sound();
    [Tooltip("sfx_fishing_reel: looped while reeling in.")]
    [SerializeField] Sound fishingReel = new Sound();
    [Tooltip("sfx_trap_set: setting a Rabbit Snare or Box Trap.")]
    [SerializeField] Sound trapSet = new Sound();

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
    float coverVolume = 1f, coverVolumeTarget = 1f, nextCoverCheck;
    InventoryContainer watchedInventory;
    SoundCue addCue;
    int addFrame = -1;
    int lastChop = -1;
    SoundCue consumeCue;
    int consumeFrame = -1;
    bool consumeSilently;

    public AudioClip CurrentMusic => musicSources[activeMusic] != null ? musicSources[activeMusic].clip : null;

    static bool InGame =>
        GameManager.Instance != null && GameManager.Instance.InGame;

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
        // PlayOneShot scales by the source's own volume, and CreateSource starts sources silent — so this one must
        // be full volume, leaving each cue's Sound.volume and the UI mixer group to set the level.
        uiSource.volume = 1f;
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

        if (SurvivalManager.Instance != null)
        {
            SurvivalManager.Instance.StomachUpset += OnStomachUpset;
            SurvivalManager.Instance.Vomited += OnVomited;
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
        if (SurvivalManager.Instance != null)
        {
            SurvivalManager.Instance.StomachUpset -= OnStomachUpset;
            SurvivalManager.Instance.Vomited -= OnVomited;
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
        UpdateTrims();

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

    public Sound Thunder => thunder;
    public IReadOnlyList<Vector2> ThunderSlices => thunderSlices;

    public IReadOnlyList<float> FootstepTimes(SurfaceType surface)
    {
        switch (surface)
        {
            case SurfaceType.Dirt: return footstepsDirtTimes;
            case SurfaceType.Gravel: return footstepsGravelTimes;
            default: return footstepsGrassTimes;
        }
    }

    // The generated placeholders (SynthSounds) stand in for any of these left without a clip.
    public Sound Gunshot => gunshot.clip != null ? gunshot : SynthSounds.Gunshot;
    public Sound BowRelease => bowRelease.clip != null ? bowRelease : SynthSounds.BowRelease;
    public Sound FishBite => fishBite.clip != null ? fishBite : SynthSounds.Plop;
    public Sound Splash => splash.clip != null ? splash : SynthSounds.Splash;
    public Sound FishingReel => fishingReel;
    public Sound TrapSet => trapSet;
    public Sound CookingSizzle => cooking;
    public Sound TreeFall => treeFall;

    // A tree starting to fall that hits the ground in fallSeconds: plays the recording from the point that puts its
    // crash on the impact (the creaking before it covers the fall).
    public void PlayTreeFall(Vector3 at, float fallSeconds)
    {
        if (treeFall == null || treeFall.clip == null)
            return;
        PlayOn(NextSfxSource(), treeFall, at, Mathf.Max(0f, treeFallImpact - fallSeconds));
    }
    public Sound Boiling => boiling;

    public Sound SprintBreathing => sprintBreathing;
    public Sound EncumberedBreathing => encumberedBreathing;

    public void Play(SoundCue cue)
    {
        Sound sound = SoundFor(cue);
        if (sound == null || sound.clip == null)
            return;

        if (cue == SoundCue.ItemPickup || cue == SoundCue.ItemDrop || cue == SoundCue.Drink || cue == SoundCue.Eat ||
            cue == SoundCue.UpsetStomach || cue == SoundCue.Vomit || cue == SoundCue.Forage)
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
            case SoundCue.Drink: return drink;
            case SoundCue.Eat: return eating;
            case SoundCue.UpsetStomach: return upsetStomach;
            case SoundCue.Vomit: return vomit;
            case SoundCue.Forage: return foraging;
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

    void OnStomachUpset()
    {
        if (InGame)
            Play(SoundCue.UpsetStomach);
    }

    void OnVomited()
    {
        if (InGame)
            Play(SoundCue.Vomit);
    }

    void OnItemsAdded(string itemId, int quantity)
    {
        if (!InGame)
            return;
        // An item gathered in its own way (foraging) plays that sound instead of the pickup.
        bool special = addFrame == Time.frameCount;
        addFrame = -1;
        Play(special ? addCue : SoundCue.ItemPickup);
    }

    // Call just before adding items gathered in a way with its own sound: the addition plays this cue instead of the
    // pickup sound.
    public void PlayOnNextAdd(SoundCue cue)
    {
        addCue = cue;
        addFrame = Time.frameCount;
    }

    // One axe blow at a point in the world: a different single chop from the recording each time.
    public void PlayChop(Vector3 at)
    {
        if (chop == null || chop.clip == null)
            return;
        float start = chop.startTime;
        if (chopTimes.Count > 0)
        {
            int pick = UnityEngine.Random.Range(0, chopTimes.Count);
            if (chopTimes.Count > 1 && pick == lastChop)
                pick = (pick + 1) % chopTimes.Count;
            lastChop = pick;
            start = chopTimes[pick];
        }
        PlayOn(NextSfxSource(), chop, at, start);
    }

    void OnItemsRemoved(string itemId, int quantity)
    {
        if (!InGame)
            return;

        // An item used up rather than put down (drinking carried water) plays its own sound instead of the drop.
        bool consumed = consumeFrame == Time.frameCount;
        consumeFrame = -1;
        if (consumed && consumeSilently)
            return; // the caller plays its own sound (e.g. setting a trap)
        Play(consumed ? consumeCue : SoundCue.ItemDrop);
    }

    // Call just before removing an item whose use plays its own sound elsewhere: the removal makes no drop sound.
    public void SilenceNextRemoval()
    {
        consumeSilently = true;
        consumeFrame = Time.frameCount;
    }

    // Call just before removing an item that's being consumed: the removal plays this cue instead of the drop sound.
    public void PlayOnConsume(SoundCue cue)
    {
        consumeCue = cue;
        consumeSilently = false;
        consumeFrame = Time.frameCount;
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
        UpdateCover(dt);

        foreach (AmbientLayer layer in ambientLayers)
        {
            AudioSource source = layer.source;
            if (source == null)
                continue;

            bool matches = active && layer.Matches(time.CurrentSeason, time.Phase,
                                                   weather != null ? weather.PrecipitationWeather : WeatherType.Clear);
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

            float coverScale = layer.softenUnderCover ? coverVolume : 1f;

            source.volume = layer.level * layer.sound.volume * seasonScale * windScale * coverScale;
        }
    }

    // How sheltered the listener is, from the same OverheadCover check Precipitation uses.
    void UpdateCover(float dt)
    {
        if (Time.unscaledTime >= nextCoverCheck)
        {
            nextCoverCheck = Time.unscaledTime + 0.25f;
            Camera listener = InGame ? Camera.main : null;
            coverVolumeTarget = 1f;
            if (listener != null)
            {
                Vector3 position = listener.transform.position;
                coverVolumeTarget = OverheadCover.Roofed(position)
                    ? roofAmbientVolume
                    : Mathf.Lerp(1f, canopyAmbientVolume, OverheadCover.CanopyFraction(position));
            }
        }
        coverVolume = Mathf.MoveTowards(coverVolume, coverVolumeTarget, dt);
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

    void PlayOn(AudioSource source, Sound sound, Vector3? position, float? startAt = null)
    {
        source.Stop();
        source.clip = sound.clip;
        source.volume = sound.volume;
        source.pitch = sound.pitch > 0f ? sound.pitch : 1f; // 0 would only come from data saved before the field existed
        source.spatialBlend = position.HasValue ? 1f : 0f;
        source.transform.position = position ?? transform.position;
        source.time = Mathf.Clamp(startAt ?? sound.startTime, 0f, Mathf.Max(0f, sound.clip.length - 0.01f));
        source.Play();

        // A trimmed one-shot fades out and stops early (UpdateTrims); reusing a source clears its old trim.
        if (sound.maxDuration > 0f)
            trims[source] = new Trim { start = Time.unscaledTime, duration = sound.maxDuration, fade = sound.fadeOut, volume = sound.volume };
        else
            trims.Remove(source);
    }

    struct Trim
    {
        public float start, duration, fade, volume;
    }

    readonly Dictionary<AudioSource, Trim> trims = new Dictionary<AudioSource, Trim>();
    readonly List<AudioSource> finishedTrims = new List<AudioSource>();

    void UpdateTrims()
    {
        foreach (KeyValuePair<AudioSource, Trim> entry in trims)
        {
            AudioSource source = entry.Key;
            Trim trim = entry.Value;
            float elapsed = Time.unscaledTime - trim.start;
            if (source == null || !source.isPlaying || elapsed >= trim.duration)
            {
                if (source != null)
                    source.Stop();
                finishedTrims.Add(source);
                continue;
            }
            float fadeFrom = trim.duration - trim.fade;
            source.volume = elapsed <= fadeFrom || trim.fade <= 0f ? trim.volume : trim.volume * (trim.duration - elapsed) / trim.fade;
        }
        foreach (AudioSource source in finishedTrims)
            trims.Remove(source);
        finishedTrims.Clear();
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
