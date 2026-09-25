using UnityEngine;
using UnityEngine.InputSystem;

// Hunting_System.md's early weapons, used from the equipped tool slot:
//   Recurve Bow — hold Attack to draw, release to loose an arrow. Silent, short effective range (about 30 m), and a
//   partial draw or shooting on the move throws the arrow wide. A killing arrow is recovered when field dressing.
//   Bolt-Action Rifle — click to fire, then a moment to work the bolt. Accurate out to about 150 m, but loud: every
//   animal within earshot bolts.
// Aim is the centre of the screen. A hit in an animal's vitals is a clean kill with a chance that falls off beyond
// the weapon's effective range; anywhere else on larger game is a wounding hit (Animal decides the rest). A miss
// by an arrow startles only what's near where it landed.
//
// Aiming (Hunting_System.md's Aiming section, 2026-09-25): the bow shows a reticle whose size is the arrow's actual
// spread right now — wide on a partial draw or on the move, tight at full draw. The rifle shoots loose from the hip
// and raises a scope with Aim (right mouse): 6x zoom, slower look, and a precise shot. Both sights read the shot
// line against Shot Placement: amber on an animal's body, green through its heart and lungs, with the range.
public class HuntingWeapon : MonoBehaviour
{
    const string BowId = "recurve_bow";
    const string RifleId = "bolt_action_rifle";
    const string ArrowId = "arrows";
    const string RoundId = "rifle_rounds";

    [SerializeField] PlayerController player;

    [Header("Recurve Bow")]
    [SerializeField, Min(0.1f)] float drawSeconds = 0.8f;
    [SerializeField, Min(1f)] float bowEffectiveRange = 30f;
    [SerializeField, Min(1f)] float bowMaxRange = 60f;
    [Tooltip("Spread in degrees at a full draw and at the weakest shot.")]
    [SerializeField] float bowSpreadFull = 0.35f, bowSpreadWeak = 2.5f;

    [Header("Bolt-Action Rifle")]
    [SerializeField, Min(0f)] float boltSeconds = 1.3f;
    [SerializeField, Min(1f)] float rifleEffectiveRange = 150f;
    [SerializeField, Min(1f)] float rifleMaxRange = 250f;
    [Tooltip("Spread in degrees from the hip and through the scope.")]
    [SerializeField] float rifleHipSpread = 0.9f, rifleScopedSpread = 0.08f;
    [SerializeField, Min(1f)] float scopeZoom = 6f;
    [Tooltip("Seconds to raise or lower the scope.")]
    [SerializeField, Min(0.01f)] float scopeRaiseSeconds = 0.2f;
    [Tooltip("Look sensitivity with the scope fully raised, as a share of normal.")]
    [SerializeField, Range(0.05f, 1f)] float scopedLookScale = 0.35f;
    [Tooltip("Animals within this distance flee at a rifle shot (metres).")]
    [SerializeField, Min(0f)] float gunshotEarshot = 250f;

    [Tooltip("Spread multiplier while moving.")]
    [SerializeField, Min(1f)] float movingSpread = 3f;

    InputAction attack, aim;
    float draw;
    float readyAt;
    float scope;          // 0-1, how far the rifle's scope is raised
    Camera view;
    float baseFov;

    void Awake()
    {
        if (player == null)
            player = GetComponent<PlayerController>();
        attack = InputSystem.actions != null ? InputSystem.actions.FindAction("Player/Attack") : null;
        aim = InputSystem.actions != null ? InputSystem.actions.FindAction("Player/Aim") : null;
        view = player.CameraTransform != null ? player.CameraTransform.GetComponent<Camera>() : null;
        if (view != null)
            baseFov = view.fieldOfView;
    }

    void OnDisable()
    {
        scope = 0f;
        ApplyScope();
    }

    void Update()
    {
        InventoryManager inventory = InventoryManager.Instance;
        string equipped = inventory != null && inventory.EquippedTool != null ? inventory.EquippedTool.Id : null;
        if (equipped != BowId && equipped != RifleId)
        {
            draw = 0f;
            if (scope > 0f)
            {
                scope = 0f;
                ApplyScope();
            }
            return;
        }
        if (!player.CanUseTools || attack == null)
            return;

        // The scope comes up while Aim is held with the rifle out, and drops for anything else.
        bool wantScope = equipped == RifleId && aim != null && aim.IsPressed();
        scope = Mathf.MoveTowards(scope, wantScope ? 1f : 0f, Time.deltaTime / scopeRaiseSeconds);
        ApplyScope();

        if (equipped == BowId)
            UpdateBow(inventory);
        else
            UpdateRifle(inventory);
    }

    // Zoom and look sensitivity follow how far the scope is raised.
    void ApplyScope()
    {
        if (view != null && baseFov > 0f)
            view.fieldOfView = Mathf.Lerp(baseFov, baseFov / scopeZoom, scope);
        if (player != null)
            player.LookScale = Mathf.Lerp(1f, scopedLookScale, scope);
    }

    float BowSpread => Mathf.Lerp(bowSpreadWeak, bowSpreadFull, draw) * (Moving ? movingSpread : 1f);
    float RifleSpread => Mathf.Lerp(rifleHipSpread, rifleScopedSpread, scope) * (Moving ? movingSpread : 1f);

    // What the sight is on right now, for the reticle: nothing, an animal's body, or its vitals — and how far.
    void ReportSight(ReticleKind kind, float spread, float effectiveRange, float maxRange)
    {
        AimTarget target = AimTarget.None;
        float distance = 0f;
        Transform cam = player.CameraTransform;
        if (player.AimRaycast(maxRange, out RaycastHit hit))
        {
            Animal animal = hit.collider.GetComponentInParent<Animal>();
            if (animal != null && !animal.IsDead)
            {
                target = animal.IsVitalsHit(cam.position, cam.forward) ? AimTarget.Vitals : AimTarget.Body;
                distance = hit.distance;
            }
        }
        ToolStatus.ReportAim(kind, spread, target, distance, distance <= effectiveRange, scope);
    }

    void UpdateBow(InventoryManager inventory)
    {
        int arrows = inventory.Player.Count(ArrowId);
        ReportSight(ReticleKind.Spread, BowSpread, bowEffectiveRange, bowMaxRange);
        if (arrows <= 0)
        {
            draw = 0f;
            ToolStatus.Report("Recurve Bow — no arrows", -1f);
            return;
        }

        if (attack.IsPressed())
        {
            draw = Mathf.Min(1f, draw + Time.deltaTime / drawSeconds);
            ToolStatus.Report(draw >= 1f ? $"Full draw — release to shoot   Arrows: {arrows}" : $"Drawing…   Arrows: {arrows}", draw);
            return;
        }

        if (draw > 0f)
        {
            float power = draw;
            draw = 0f;
            if (power < 0.3f)
            {
                ToolStatus.Flash("Let down — not drawn far enough to shoot");
                return;
            }

            SilenceAmmoRemoval();
            inventory.RemoveFromPlayer(ArrowId, 1);
            float spread = Mathf.Lerp(bowSpreadWeak, bowSpreadFull, power) * (Moving ? movingSpread : 1f); // as the reticle showed
            PlaySound(a => a.BowRelease, player.transform.position);
            Fire(spread, bowEffectiveRange, Mathf.Lerp(bowMaxRange * 0.5f, bowMaxRange, power), arrow: true);
            return;
        }

        ToolStatus.Report($"Recurve Bow — hold to draw, release to shoot   Arrows: {arrows}");
    }

    void UpdateRifle(InventoryManager inventory)
    {
        int rounds = inventory.Player.Count(RoundId);
        ReportSight(scope > 0.5f ? ReticleKind.Scope : ReticleKind.Spread, RifleSpread, rifleEffectiveRange, rifleMaxRange);
        if (Time.time < readyAt)
        {
            ToolStatus.Report($"Working the bolt…   Rounds: {rounds}", 1f - (readyAt - Time.time) / boltSeconds);
            return;
        }
        if (rounds <= 0)
        {
            ToolStatus.Report("Rifle — no ammunition");
            return;
        }

        ToolStatus.Report(scope > 0.5f ? $"Rounds: {rounds}" : $"Bolt-Action Rifle — right-click to aim, click to fire   Rounds: {rounds}");
        if (!attack.WasPressedThisFrame())
            return;

        SilenceAmmoRemoval();
        inventory.RemoveFromPlayer(RoundId, 1);
        readyAt = Time.time + boltSeconds;
        PlaySound(a => a.Gunshot, player.transform.position);
        Fire(RifleSpread, rifleEffectiveRange, rifleMaxRange, arrow: false);

        // Hunting_System.md: loud, may disperse nearby wildlife.
        if (WildlifeManager.Instance != null)
            WildlifeManager.Instance.Alert(player.transform.position, gunshotEarshot);
    }

    bool Moving => player.HorizontalSpeed > 0.5f;

    void Fire(float spreadDegrees, float effectiveRange, float maxRange, bool arrow)
    {
        Transform cam = player.CameraTransform;
        Vector2 jitter = Random.insideUnitCircle * spreadDegrees;
        Vector3 direction = Quaternion.AngleAxis(jitter.x, cam.up) * Quaternion.AngleAxis(jitter.y, cam.right) * cam.forward;

        if (!player.AimRaycast(maxRange, out RaycastHit hit, direction))
        {
            ToolStatus.Flash("Miss");
            return;
        }

        Animal animal = hit.collider.GetComponentInParent<Animal>();
        if (animal == null || animal.IsDead)
        {
            ToolStatus.Flash("Miss");
            if (arrow && WildlifeManager.Instance != null)
                WildlifeManager.Instance.Alert(hit.point, 20f); // the arrow thumping in nearby spooks what's close
            return;
        }

        float distance = hit.distance;
        float cleanKill = distance <= effectiveRange
            ? (arrow ? Mathf.Lerp(0.95f, 0.8f, distance / effectiveRange) : 0.97f)
            : Mathf.Lerp(arrow ? 0.8f : 0.97f, 0.35f, Mathf.InverseLerp(effectiveRange, maxRange, distance));
        bool vitals = animal.IsVitalsHit(cam.position, direction);

        string outcome = animal.OnShot(vitals, cleanKill, player.transform.position);
        if (arrow && animal.IsDead && animal.TryGetComponent(out Carcass carcass))
            carcass.KilledByArrow = true;
        ToolStatus.Flash($"{outcome}  ({distance:0} m)");
    }

    // Spending an arrow or round isn't dropping an item — the shot's own sound plays instead.
    static void SilenceAmmoRemoval()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SilenceNextRemoval();
    }

    static void PlaySound(System.Func<AudioManager, Sound> sound, Vector3 at)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayAt(sound(AudioManager.Instance), at);
    }
}
