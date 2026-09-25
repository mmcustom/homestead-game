using UnityEngine;

// The world object for one campfire (FireManager owns its state). A ring of stones with Firewood laid in it; once lit,
// flames, embers, a flickering light and a crackle, all scaled down as the fuel runs low. Burnt out, the logs go and
// only ash is left in the ring until more Firewood is added. The main interaction does whatever the fire needs next —
// light it, or add Firewood — and otherwise reports its state; the second one (R) puts a burning fire out, keeping
// its fuel for relighting, or adds Firewood to one that's out. Going out, the flames stop and the light and crackle
// fade over a moment rather than snapping off.
public class Campfire : MonoBehaviour, IInteractable, ISecondaryInteractable
{
    [SerializeField] GameObject logs;
    [SerializeField] GameObject ash;
    [SerializeField] ParticleSystem flames;
    [SerializeField] ParticleSystem embers;
    [SerializeField] Light fireLight;
    [SerializeField] AudioSource crackle;

    [Tooltip("Light intensity at full strength; it flickers around this and dims as fuel runs low.")]
    [SerializeField, Min(0f)] float lightIntensity = 150f; // candela
    [Tooltip("Below this many hours of fuel the fire visibly dies down.")]
    [SerializeField, Min(0.1f)] float lowFuelHours = 1f;
    [Tooltip("Crackle volume at full strength (Audio_System.md's ambient_campfire_crackle); quieter as the fire dies down.")]
    [SerializeField, Range(0f, 1f)] float crackleVolume = 0.5f;
    [Tooltip("Seconds the light and crackle take to fade when the fire goes out.")]
    [SerializeField, Min(0f)] float fadeOutSeconds = 1.5f;

    CampfireState state;
    float flameRate, emberRate;
    float flickerSeed;
    bool shownLit;
    float fadeStart = -1f, fadeFromIntensity, fadeFromVolume;

    public CampfireState State => state;

    void Awake()
    {
        flameRate = flames != null ? flames.emission.rateOverTime.constant : 0f;
        emberRate = embers != null ? embers.emission.rateOverTime.constant : 0f;
        flickerSeed = Random.value * 100f;
        if (crackle != null)
        {
            if (crackle.clip == null)
                crackle.clip = FireSound.Crackle;
            // Follows the Ambient volume setting, like the world's other ambience.
            if (AudioManager.Instance != null)
                crackle.outputAudioMixerGroup = AudioManager.Instance.GetGroup(AudioChannel.Ambient);
        }
    }

    public void Bind(CampfireState fireState)
    {
        state = fireState;
        Refresh();
    }

    // Shows the current state; FireManager calls this whenever the fire is lit, fed or goes out.
    public void Refresh()
    {
        if (state == null)
            return;

        bool lit = state.lit;
        if (logs != null)
            logs.SetActive(state.fuelHours > 0f);
        if (ash != null)
            ash.SetActive(state.fuelHours <= 0f || lit);

        SetPlaying(flames, lit);
        SetPlaying(embers, lit);

        if (lit)
        {
            fadeStart = -1f;
            if (fireLight != null)
                fireLight.enabled = true;
            if (crackle != null && !crackle.isPlaying)
                crackle.Play();
        }
        else if (shownLit)
        {
            // Just went out: let the light and sound die away (Update finishes the fade).
            fadeStart = Time.time;
            fadeFromIntensity = fireLight != null ? fireLight.intensity : 0f;
            fadeFromVolume = crackle != null ? crackle.volume : 0f;
        }
        else
        {
            // Spawned already out (e.g. loaded from a save).
            if (fireLight != null)
                fireLight.enabled = false;
            if (crackle != null)
                crackle.Stop();
        }

        shownLit = lit;
    }

    static void SetPlaying(ParticleSystem system, bool on)
    {
        if (system == null)
            return;
        if (on && !system.isPlaying)
            system.Play();
        else if (!on && system.isPlaying)
            system.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    void Update()
    {
        if (state == null)
            return;

        if (!state.lit)
        {
            FadeOut();
            return;
        }

        // Dies down over the last stretch of fuel.
        float strength = Mathf.Lerp(0.35f, 1f, Mathf.Clamp01(state.fuelHours / lowFuelHours));

        if (flames != null)
        {
            ParticleSystem.EmissionModule emission = flames.emission;
            emission.rateOverTime = flameRate * strength;
        }
        if (embers != null)
        {
            ParticleSystem.EmissionModule emission = embers.emission;
            emission.rateOverTime = emberRate * strength;
        }
        if (fireLight != null)
        {
            float t = Time.time * 7f + flickerSeed;
            float flicker = 0.8f + 0.12f * Mathf.PerlinNoise(t, 0.3f) + 0.08f * Mathf.Sin(t * 2.3f);
            fireLight.intensity = lightIntensity * strength * flicker;
        }
        if (crackle != null)
            crackle.volume = crackleVolume * Mathf.Lerp(0.4f, 1f, strength);
    }

    void FadeOut()
    {
        if (fadeStart < 0f)
            return;

        float t = fadeOutSeconds > 0f ? Mathf.Clamp01((Time.time - fadeStart) / fadeOutSeconds) : 1f;
        if (fireLight != null)
            fireLight.intensity = fadeFromIntensity * (1f - t);
        if (crackle != null)
            crackle.volume = fadeFromVolume * (1f - t);

        if (t >= 1f)
        {
            fadeStart = -1f;
            if (fireLight != null)
                fireLight.enabled = false;
            if (crackle != null)
                crackle.Stop();
        }
    }

    // --- Interaction ---

    public string InteractionPrompt
    {
        get
        {
            FireManager fires = FireManager.Instance;
            if (state == null || fires == null)
                return "";

            string fuel = $"{state.fuelHours:0.0} h of fuel";
            if (!state.lit && state.fuelHours > 0f)
                return FireManager.HasIgnition ? $"Light Fire  ({fuel})" : "Light Fire  — needs Flint and Steel";
            if (fires.CanAddFuel(state))
                return state.lit ? $"Add Firewood  (burning, {fuel})" : "Add Firewood";
            if (state.lit)
                return FireManager.FirewoodCarried > 0 ? $"Burning  ({fuel}, full)" : $"Burning  ({fuel})";
            return "Burnt out  — add Firewood to rebuild it";
        }
    }

    // Always true so the fire's state shows when looked at, even when there's nothing to do.
    public bool CanInteract(PlayerController player) => state != null && FireManager.Instance != null;

    public void Interact(PlayerController player)
    {
        FireManager fires = FireManager.Instance;
        if (state == null || fires == null)
            return;

        if (!fires.Light(state))
            fires.AddFuel(state);
    }

    // Second action: put a burning fire out, or feed one that's out but still has wood (so it isn't only "Light").
    public string SecondaryPrompt
    {
        get
        {
            FireManager fires = FireManager.Instance;
            if (state == null || fires == null)
                return "";
            if (state.lit)
                return "Put Out Fire  (keeps the Firewood left)";
            if (state.fuelHours > 0f && fires.CanAddFuel(state))
                return "Add Firewood";
            return "";
        }
    }

    public void SecondaryInteract(PlayerController player)
    {
        FireManager fires = FireManager.Instance;
        if (state == null || fires == null)
            return;

        if (!fires.Extinguish(state) && state.fuelHours > 0f)
            fires.AddFuel(state);
    }
}
