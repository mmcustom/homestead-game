using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

// Wave 8 body-state screen effects (2026-11-20). A global volume above the scene's (which sets no vignette, colour or lens overrides, so these start
// from neutral) that shows how the body is doing:
//  - cold: a cold blue tint, less colour, a dark blue-edged vignette that throbs gently while shivering (SurvivalManager.WarmthTier, ShiverIntensity);
//  - heat: a warm tint and a slow wavering of the image, like heat haze (SurvivalManager.HeatLevel);
//  - tired: the edges darken and the colour drains in time with heavy breathing when stamina runs low;
//  - waking weakened: just after a collapse the world comes back grey, dark at the edges and swimming, clearing over about half a minute, then a
//    slight greyness stays while SurvivalManager.IsWeakened.
// The collapse itself already fades to black (SleepManager). Every level eases in and out; nothing flashes.
public class BodyStateFx : MonoBehaviour
{
    public const float WakeSeconds = 30f;
    public const float TiredBelow = 0.3f; // of the stamina bar

    VolumeProfile profile;
    Vignette vignette;
    ColorAdjustments colour;
    LensDistortion lens;
    PlayerController player;

    float cold, heat, tired, weak, wake, shiver;
    bool wasCollapsed;

    public float Cold => cold;
    public float Heat => heat;
    public float Tired => tired;
    public float Wake => wake;

    void Awake()
    {
        profile = ScriptableObject.CreateInstance<VolumeProfile>();
        profile.name = "Body State (runtime)";
        vignette = profile.Add<Vignette>();
        vignette.intensity.Override(0f);
        vignette.color.Override(Color.black);
        vignette.smoothness.Override(0.55f);
        vignette.rounded.Override(true);
        colour = profile.Add<ColorAdjustments>();
        colour.saturation.Override(0f);
        colour.colorFilter.Override(Color.white);
        colour.postExposure.Override(0f);
        lens = profile.Add<LensDistortion>();
        lens.intensity.Override(0f);

        var volume = gameObject.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 20f;
        volume.sharedProfile = profile;
    }

    void OnDestroy()
    {
        if (profile != null)
            Destroy(profile);
    }

    // 0-1 levels from the survival state, testable without a scene.
    public static float ColdLevel(SurvivalManager.Tier warmth) =>
        warmth == SurvivalManager.Tier.Fine ? 0f : warmth == SurvivalManager.Tier.Minor ? 0.25f :
        warmth == SurvivalManager.Tier.Reduced ? 0.5f : warmth == SurvivalManager.Tier.Severe ? 0.8f : 1f;

    public static float HeatLevel(SurvivalManager.HeatStage stage) =>
        stage == SurvivalManager.HeatStage.None ? 0f : stage == SurvivalManager.HeatStage.Hot ? 0.2f :
        stage == SurvivalManager.HeatStage.Exhaustion ? 0.5f : stage == SurvivalManager.HeatStage.Severe ? 0.75f : 1f;

    public static float TiredLevel(float stamina, float max) =>
        max <= 0f ? 0f : Mathf.Clamp01(1f - stamina / (max * TiredBelow));

    void Update()
    {
        SurvivalManager s = SurvivalManager.Instance;
        if (player == null)
            player = FindFirstObjectByType<PlayerController>();
        float dt = Time.unscaledDeltaTime;

        float coldTarget = 0f, heatTarget = 0f, tiredTarget = 0f, weakTarget = 0f, shiverTarget = 0f;
        if (s != null)
        {
            coldTarget = ColdLevel(s.WarmthTier);
            heatTarget = HeatLevel(s.HeatLevel);
            weakTarget = s.IsWeakened ? 1f : 0f;
            shiverTarget = s.IsShivering ? Mathf.Clamp01(s.ShiverIntensity) : 0f;
            if (wasCollapsed && !s.IsCollapsed)
                wake = 1f; // just came round
            wasCollapsed = s.IsCollapsed;
        }
        if (player != null)
            tiredTarget = TiredLevel(player.Stamina, player.EffectiveMaxStamina);

        cold = Mathf.MoveTowards(cold, coldTarget, dt * 0.25f);
        heat = Mathf.MoveTowards(heat, heatTarget, dt * 0.25f);
        tired = Mathf.MoveTowards(tired, tiredTarget, dt * 1.5f);
        weak = Mathf.MoveTowards(weak, weakTarget, dt * 0.2f);
        shiver = Mathf.MoveTowards(shiver, shiverTarget, dt * 0.5f);
        wake = Mathf.MoveTowards(wake, 0f, dt / WakeSeconds);

        float t = Time.unscaledTime;
        float breathing = 0.6f + 0.4f * Mathf.Sin(t * 2.4f);
        float throb = 0.5f + 0.5f * Mathf.Sin(t * 3.1f);

        vignette.intensity.value = Mathf.Clamp(0.24f * cold + 0.07f * shiver * throb + 0.14f * heat + 0.3f * tired * breathing + 0.5f * wake, 0f, 0.62f);
        vignette.color.value = Color.Lerp(Color.black, new Color(0.03f, 0.07f, 0.16f), cold / Mathf.Max(0.001f, cold + heat + tired + wake));
        colour.saturation.value = Mathf.Clamp(-24f * cold + 6f * heat - 20f * tired * breathing - 12f * weak - 50f * wake, -80f, 20f);
        Color filter = Color.Lerp(Color.white, new Color(0.84f, 0.92f, 1.0f), cold * 0.75f);
        filter = Color.Lerp(filter, new Color(1.0f, 0.93f, 0.82f), heat * 0.6f);
        colour.colorFilter.value = filter;
        colour.postExposure.value = -0.7f * wake;
        lens.intensity.value = heat * (0.05f * Mathf.Sin(t * 1.1f) + 0.025f * Mathf.Sin(t * 2.9f)) + wake * 0.14f * Mathf.Sin(t * 0.8f);
    }
}
