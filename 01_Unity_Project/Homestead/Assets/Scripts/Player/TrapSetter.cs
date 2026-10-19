using UnityEngine;
using UnityEngine.InputSystem;

// Setting traps (Trapping_System.md, Fishing_System.md's Fish Trap): with a Rabbit Snare, Box Trap or Fish Trap equipped, a live
// ghost follows the aim point (the ground, or water for a Fish Trap): green where it can be set, red with the reason where it can't,
// within 6 m. The HUD says how good the spot is ("Trap placement matters"). One left click (Attack) sets the trap at the ghost; on
// red it only says why. Right click puts the trap away (Unequip; it stays in the pack). There is no mode to cancel: the ghost simply goes when the trap is no longer in hand, a screen or the pause
// menu opens, the player sleeps or collapses, or a game is loaded. Esc still pauses. Separate from Build-tab Placement Mode.
// The rules are CanSetAt, unchanged, and the ghost is never registered with TrapManager (see PlacementGhost.ForTrap).
public class TrapSetter : MonoBehaviour
{
    [SerializeField] PlayerController player;
    [Tooltip("The furthest the ghost can be from the player (metres). Beyond it the ghost is red: Too far.")]
    [SerializeField, Min(1f)] float previewReach = 6f;
    [Tooltip("How far the aim ray looks for ground or water, so a spot beyond reach can say Too far instead of nothing.")]
    [SerializeField, Min(1f)] float previewAimRange = 40f;
    [Tooltip("Traps can't be set closer together than this (metres).")]
    [SerializeField, Min(0f)] float minSpacing = 2f;
    [SerializeField, Range(0f, 60f)] float maxSlope = 30f;

    InputAction attack;
    PlacementGhost ghost;
    TrapType ghostType;
    Vector3 placementAt = new Vector3(float.NaN, 0f, 0f); // where the spot quality was last worked out (the tree scan is not cheap)
    string placementWord = "";

    void Awake()
    {
        if (player == null)
            player = GetComponent<PlayerController>();
        attack = InputSystem.actions != null ? InputSystem.actions.FindAction("Player/Attack") : null;
    }

    void OnDisable() => DropGhost();

    void OnDestroy() => DropGhost();

    void DropGhost()
    {
        ghost?.Destroy();
        ghost = null;
    }

    // After the player's own Update (which turns the camera), so the ghost follows this frame's aim.
    void LateUpdate()
    {
        if (!TrapInHand(out TrapType type) || !CanPreview())
        {
            DropGhost();
            return;
        }

        // Right click puts the trap away: the existing Unequip (what pressing its number key again does). It stays in the pack.
        Mouse mouse = Mouse.current;
        if (mouse != null && mouse.rightButton.wasPressedThisFrame)
        {
            InventoryManager.Instance.Unequip();
            DropGhost();
            return;
        }

        TrapManager traps = TrapManager.Instance;
        if (ghost == null || ghostType != type)
        {
            DropGhost();
            ghost = PlacementGhost.ForTrap(traps.PrefabFor(type), TrapManager.NameOf(type));
            ghost.SetVisible(false);
            ghostType = type;
            placementAt = new Vector3(float.NaN, 0f, 0f);
        }

        string name = TrapManager.NameOf(type);
        if (!player.AimRaycast(previewAimRange, out RaycastHit hit))
        {
            ghost.SetVisible(false);
            ToolStatus.Report(type == TrapType.FishTrap ? $"{name} — aim at water close by" : $"{name} — aim at the ground close by", -1f, false);
            return;
        }

        bool valid;
        string reason;
        if (Vector3.Distance(player.transform.position, hit.point) > previewReach)
        {
            valid = false;
            reason = $"Too far — come within {previewReach:0} m.";
        }
        else
        {
            valid = CanSetAt(type, hit.point, hit.normal, hit.collider, out reason);
        }

        ghost.SetPose(hit.point, player.transform.eulerAngles.y); // faces the way the player does, as a set trap always has
        ghost.SetValid(valid);
        ghost.SetVisible(true);

        if (valid)
        {
            if (float.IsNaN(placementAt.x) || (placementAt - hit.point).sqrMagnitude > 0.0625f)
            {
                placementAt = hit.point;
                placementWord = TrapManager.PlacementWord(TrapManager.PlacementAt(type, hit.point));
            }
            ToolStatus.Report($"Place {name} here — {placementWord} spot", -1f, false);
        }
        else
        {
            ToolStatus.Report(reason, -1f, false);
        }

        if (attack != null && attack.WasPressedThisFrame())
        {
            if (!valid)
                ToolStatus.Flash(reason, 2.5f);
            else if (!SetTrap(type, hit.point, player.transform.eulerAngles.y))
                ToolStatus.Flash("Can't set it there.", 2.5f);
        }
    }

    // The equipped tool is a trap.
    static bool TrapInHand(out TrapType type)
    {
        type = TrapType.RabbitSnare;
        InventoryManager inventory = InventoryManager.Instance;
        string equipped = inventory != null && inventory.EquippedTool != null ? inventory.EquippedTool.Id : null;
        if (equipped == "rabbit_snare") type = TrapType.RabbitSnare;
        else if (equipped == "box_trap") type = TrapType.BoxTrap;
        else if (equipped == "fish_trap") type = TrapType.FishTrap;
        else return false;
        return TrapManager.Instance != null;
    }

    // Plain play only: not in a screen, the pause menu, a load, Build-tab Placement Mode, a knock-down, asleep or collapsed.
    bool CanPreview()
    {
        if (player == null || !player.CanUseTools)
            return false;
        SleepManager sleep = SleepManager.Instance;
        if (sleep != null && sleep.IsSleeping)
            return false;
        SurvivalManager survival = SurvivalManager.Instance;
        return survival == null || !survival.IsCollapsed;
    }

    // The trap rules, unchanged, evaluated for a surface point: good-spot water and ground tests, the slope limit and the spacing.
    // The player and the aim are not involved, so the ghost and the real set ask the very same question.
    public bool CanSetAt(TrapType type, Vector3 point, Vector3 normal, Collider surface, out string reason)
    {
        string name = TrapManager.NameOf(type);
        WaterSource water = surface != null ? surface.GetComponentInParent<WaterSource>() : null;
        if (type == TrapType.FishTrap)
        {
            if (water == null || FishingManager.WaterAt(water, point) == FishWater.None)
            {
                reason = water == null ? $"{name} — aim at water close by" : "Too shallow for fish here";
                return false;
            }
        }
        else
        {
            if (water != null)
            {
                reason = $"Can't set a {name} in water";
                return false;
            }
            if (surface == null || surface.GetComponentInParent<Terrain>() == null || Vector3.Angle(normal, Vector3.up) > maxSlope)
            {
                reason = $"{name} — needs open, level ground";
                return false;
            }
        }

        TrapManager traps = TrapManager.Instance;
        foreach (TrapState other in traps.Traps)
        {
            if (Vector3.Distance(other.position, point) < minSpacing)
            {
                reason = "Too close to another trap";
                return false;
            }
        }

        reason = "";
        return true;
    }

    // Sets the trap (spending the one from the pack, as before) with its message and sound. Called when Placement Mode confirms.
    public bool SetTrap(TrapType type, Vector3 point, float yaw)
    {
        TrapManager traps = TrapManager.Instance;
        if (traps == null)
            return false;
        string name = TrapManager.NameOf(type);
        if (AudioManager.Instance != null)
            AudioManager.Instance.SilenceNextRemoval(); // the trap-set sound below replaces the item-drop sound
        if (!traps.Place(type, point, yaw))
            return false;

        ToolStatus.Flash(type == TrapType.BoxTrap ? $"Set the {name} — bait it with fruit or greens" : $"Set the {name}");
        // sfx_trap_set for the land traps; a Fish Trap goes into the water with a splash (sfx_splash).
        AudioManager audio = AudioManager.Instance;
        if (audio != null)
            audio.PlayAt(type == TrapType.FishTrap ? audio.Splash : audio.TrapSet, point);
        return true;
    }
}
