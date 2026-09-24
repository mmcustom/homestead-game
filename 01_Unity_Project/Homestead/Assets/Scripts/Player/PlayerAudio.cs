using UnityEngine;

// The player's sounds (Audio_System.md, Player SFX): footsteps by ground surface, sprint breathing, and heavier
// encumbered breathing at or above InventoryManager's encumbered weight. Clips come from AudioManager so all audio
// content stays in one place; the sources live here, on the player, routed to the SFX mixer group.
//
// Footsteps play one step per stride of distance actually covered, so the step rate follows movement and is the
// same on every surface — only the sound changes. Each footstep clip is a recorded sequence of steps, and each step
// plays one footfall from it (AudioManager's step times), not the recording's own pace.
[RequireComponent(typeof(PlayerController))]
public class PlayerAudio : MonoBehaviour
{
    [Header("Footsteps")]
    [Tooltip("Used when the ground under the player has no GroundSurface.")]
    [SerializeField] SurfaceType defaultSurface = SurfaceType.Grass;
    [SerializeField] LayerMask groundMask = ~0;
    [Tooltip("Distance covered per footstep, in metres. With PlayerController's speeds these give roughly 2.3 steps " +
             "a second walking, 3.1 sprinting and 2 crouching.")]
    [SerializeField, Min(0.1f)] float walkStride = 1.1f;
    [SerializeField, Min(0.1f)] float sprintStride = 1.6f;
    [SerializeField, Min(0.1f)] float crouchStride = 0.65f;
    [SerializeField, Range(0f, 1f)] float crouchVolume = 0.5f;
    [Tooltip("How long one footfall plays before fading out, in seconds.")]
    [SerializeField, Min(0.05f)] float stepSeconds = 0.3f;
    [SerializeField, Min(0.01f)] float stepFadeSeconds = 0.05f;
    [Tooltip("Random pitch variation per step, so repeats don't sound identical.")]
    [SerializeField, Range(0f, 0.2f)] float stepPitchJitter = 0.05f;

    [Header("Breathing")]
    [SerializeField, Min(0.01f)] float breathingFadeInSeconds = 0.5f;
    [Tooltip("Breathing trails off rather than cutting out when the effort stops.")]
    [SerializeField, Min(0.01f)] float breathingFadeOutSeconds = 1.5f;

    const float SurfaceCheckInterval = 0.2f;
    const float StepLeadIn = 0.03f; // start just before each measured step time, which sits slightly after the attack
    const float FirstStepProgress = 0.6f; // starting to move lands the first step after a short partial stride

    PlayerController controller;
    AudioSource sprintBreath, encumberedBreath;
    readonly AudioSource[] stepSources = new AudioSource[3];
    readonly float[] stepEnds = new float[3];
    int nextStepSource, lastStepIndex = -1;
    float strideProgress = FirstStepProgress;
    float sprintLevel, encumberedLevel;
    float nextSurfaceCheck;

    public SurfaceType CurrentSurface { get; private set; }

    void Awake()
    {
        controller = GetComponent<PlayerController>();
        CurrentSurface = defaultSurface;
        for (int i = 0; i < stepSources.Length; i++)
        {
            stepSources[i] = CreateSource("Footstep " + (i + 1));
            stepSources[i].loop = false;
        }
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

        // Footsteps: one per stride of distance covered.
        if (moving)
        {
            float stride = controller.IsSprinting ? sprintStride : controller.IsCrouching ? crouchStride : walkStride;
            strideProgress += controller.HorizontalSpeed * dt / stride;
            if (strideProgress >= 1f)
            {
                strideProgress -= 1f;
                PlayStep(audio);
            }
        }
        else
        {
            strideProgress = FirstStepProgress;
        }
        FadeFinishedSteps(dt);

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
                return surface.SurfaceAt(hit.point);
        }
        return defaultSurface;
    }

    void PlayStep(AudioManager audio)
    {
        Sound sound = audio.Footsteps(CurrentSurface);
        if (sound.clip == null)
            return;

        // A different footfall from the recording each time.
        System.Collections.Generic.IReadOnlyList<float> times = audio.FootstepTimes(CurrentSurface);
        float start = 0f;
        if (times.Count > 0)
        {
            int index = Random.Range(0, times.Count);
            if (times.Count > 1 && index == lastStepIndex)
                index = (index + 1) % times.Count;
            lastStepIndex = index;
            start = Mathf.Max(0f, times[index] - StepLeadIn);
        }

        AudioSource source = stepSources[nextStepSource];
        stepEnds[nextStepSource] = Time.time + stepSeconds;
        nextStepSource = (nextStepSource + 1) % stepSources.Length;

        source.Stop();
        source.clip = sound.clip;
        source.time = Mathf.Min(start, sound.clip.length - 0.05f);
        source.volume = sound.volume * (controller.IsCrouching ? crouchVolume : 1f);
        source.pitch = 1f + Random.Range(-stepPitchJitter, stepPitchJitter);
        source.Play();
    }

    // Each footfall is cut from a longer recording, so it fades out quickly before the next step on the tape.
    void FadeFinishedSteps(float dt)
    {
        for (int i = 0; i < stepSources.Length; i++)
        {
            AudioSource source = stepSources[i];
            if (!source.isPlaying || Time.time < stepEnds[i])
                continue;
            source.volume = Mathf.MoveTowards(source.volume, 0f, dt / stepFadeSeconds);
            if (source.volume <= 0f)
                source.Stop();
        }
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
