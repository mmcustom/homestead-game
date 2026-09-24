using UnityEngine;

// The player's looping sounds (Audio_System.md, Player SFX): footsteps by ground surface, sprint breathing, and
// heavier encumbered breathing at or above InventoryManager's encumbered weight. Clips come from AudioManager so
// all audio content stays in one place; the sources live here, on the player, routed to the SFX mixer group.
[RequireComponent(typeof(PlayerController))]
public class PlayerAudio : MonoBehaviour
{
    [Header("Footsteps")]
    [Tooltip("Used when the ground under the player has no GroundSurface.")]
    [SerializeField] SurfaceType defaultSurface = SurfaceType.Grass;
    [SerializeField] LayerMask groundMask = ~0;
    [Tooltip("Playback speed of the footstep loop per movement state — faster steps when sprinting.")]
    [SerializeField, Range(0.5f, 2f)] float walkPitch = 1f;
    [SerializeField, Range(0.5f, 2f)] float sprintPitch = 1.35f;
    [SerializeField, Range(0.5f, 2f)] float crouchPitch = 0.8f;
    [SerializeField, Range(0f, 1f)] float crouchVolume = 0.5f;
    [SerializeField, Min(0.01f)] float footstepFadeSeconds = 0.12f;

    [Header("Breathing")]
    [SerializeField, Min(0.01f)] float breathingFadeInSeconds = 0.5f;
    [Tooltip("Breathing trails off rather than cutting out when the effort stops.")]
    [SerializeField, Min(0.01f)] float breathingFadeOutSeconds = 1.5f;

    const float SurfaceCheckInterval = 0.2f;

    PlayerController controller;
    AudioSource footsteps, sprintBreath, encumberedBreath;
    float footstepLevel, sprintLevel, encumberedLevel;
    float nextSurfaceCheck;

    public SurfaceType CurrentSurface { get; private set; }

    void Awake()
    {
        controller = GetComponent<PlayerController>();
        CurrentSurface = defaultSurface;
        footsteps = CreateSource("Footsteps");
        sprintBreath = CreateSource("Sprint Breathing");
        encumberedBreath = CreateSource("Encumbered Breathing");
    }

    void Update()
    {
        AudioManager audio = AudioManager.Instance;
        if (audio == null)
            return;

        float dt = Time.deltaTime;
        bool moving = controller.State != MovementState.Idle && controller.IsGrounded && controller.HorizontalSpeed > 0.2f;

        if (Time.time >= nextSurfaceCheck)
        {
            nextSurfaceCheck = Time.time + SurfaceCheckInterval;
            CurrentSurface = DetectSurface();
        }

        // Footsteps: one loop per surface, sped up or slowed down to match the movement state.
        Sound step = audio.Footsteps(CurrentSurface);
        if (footsteps.clip != step.clip)
        {
            footsteps.clip = step.clip;
            if (footsteps.isPlaying)
                footsteps.Play();
        }

        footstepLevel = Fade(footstepLevel, moving, footstepFadeSeconds, footstepFadeSeconds, dt);
        footsteps.pitch = controller.IsSprinting ? sprintPitch : controller.IsCrouching ? crouchPitch : walkPitch;
        footsteps.volume = footstepLevel * step.volume * (controller.IsCrouching ? crouchVolume : 1f);
        SetPlaying(footsteps, footstepLevel > 0f);

        // Breathing: sprinting takes priority; encumbered breathing plays while moving under a heavy load.
        InventoryManager inventory = InventoryManager.Instance;
        bool encumbered = inventory != null && inventory.CarriedWeightKg >= inventory.EncumberedWeightKg;
        bool sprinting = controller.IsSprinting;

        sprintLevel = Fade(sprintLevel, sprinting, breathingFadeInSeconds, breathingFadeOutSeconds, dt);
        encumberedLevel = Fade(encumberedLevel, encumbered && moving && !sprinting, breathingFadeInSeconds, breathingFadeOutSeconds, dt);

        ApplyLoop(sprintBreath, audio.SprintBreathing, sprintLevel);
        ApplyLoop(encumberedBreath, audio.EncumberedBreathing, encumberedLevel);
    }

    SurfaceType DetectSurface()
    {
        Vector3 origin = transform.position + Vector3.up * 0.3f;
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 1f, groundMask, QueryTriggerInteraction.Ignore))
        {
            GroundSurface surface = hit.collider.GetComponentInParent<GroundSurface>();
            if (surface != null)
                return surface.Surface;
        }
        return defaultSurface;
    }

    static float Fade(float level, bool on, float fadeIn, float fadeOut, float dt) =>
        Mathf.MoveTowards(level, on ? 1f : 0f, dt / (on ? fadeIn : fadeOut));

    static void ApplyLoop(AudioSource source, Sound sound, float level)
    {
        if (source.clip != sound.clip)
            source.clip = sound.clip;
        source.volume = level * sound.volume;
        SetPlaying(source, level > 0f);
    }

    // Uses isPlaying only to start/stop; a listener-paused source reports false but play-state is left alone
    // while the game is paused because time (and so the fade) doesn't advance.
    static void SetPlaying(AudioSource source, bool play)
    {
        if (source.clip == null)
            return;
        if (play && !source.isPlaying && !AudioListener.pause)
            source.Play();
        else if (!play && source.isPlaying)
            source.Stop();
    }

    AudioSource CreateSource(string sourceName)
    {
        var child = new GameObject(sourceName);
        child.transform.SetParent(transform, false);

        var source = child.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = true;
        source.spatialBlend = 0f;
        source.volume = 0f;
        source.outputAudioMixerGroup = AudioManager.Instance != null ? AudioManager.Instance.GetGroup(AudioChannel.Sfx) : null;
        return source;
    }
}
