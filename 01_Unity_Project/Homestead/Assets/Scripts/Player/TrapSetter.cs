using UnityEngine;
using UnityEngine.InputSystem;

// Setting traps (Trapping_System.md, Fishing_System.md's Fish Trap): with a Rabbit Snare, Box Trap or Fish Trap
// equipped, aim at the ground nearby — or at water, for a Fish Trap — and click (Attack) to set it there. The HUD
// says how good the spot is before committing ("Trap placement matters"), so the player learns where traps pay off.
public class TrapSetter : MonoBehaviour
{
    [SerializeField] PlayerController player;
    [SerializeField, Min(1f)] float reach = 4f;
    [Tooltip("Traps can't be set closer together than this (metres).")]
    [SerializeField, Min(0f)] float minSpacing = 2f;
    [SerializeField, Range(0f, 60f)] float maxSlope = 30f;

    InputAction attack;

    void Awake()
    {
        if (player == null)
            player = GetComponent<PlayerController>();
        attack = InputSystem.actions != null ? InputSystem.actions.FindAction("Player/Attack") : null;
    }

    void Update()
    {
        InventoryManager inventory = InventoryManager.Instance;
        TrapManager traps = TrapManager.Instance;
        string equipped = inventory != null && inventory.EquippedTool != null ? inventory.EquippedTool.Id : null;
        TrapType type;
        if (equipped == "rabbit_snare") type = TrapType.RabbitSnare;
        else if (equipped == "box_trap") type = TrapType.BoxTrap;
        else if (equipped == "fish_trap") type = TrapType.FishTrap;
        else return;

        if (traps == null || !player.CanUseTools)
            return;

        string name = TrapManager.NameOf(type);
        if (!CanSet(type, out Vector3 point, out string reason))
        {
            ToolStatus.Report(reason, -1f, false);
            return;
        }

        float placement = TrapManager.PlacementAt(type, point);
        ToolStatus.Report($"Click to set the {name} here  ({TrapManager.PlacementWord(placement)} spot)", -1f, false);

        if (attack == null || !attack.WasPressedThisFrame())
            return;
        if (AudioManager.Instance != null)
            AudioManager.Instance.SilenceNextRemoval(); // the trap-set sound below replaces the item-drop sound
        if (traps.Place(type, point, player.transform.eulerAngles.y))
        {
            ToolStatus.Flash(type == TrapType.BoxTrap ? $"Set the {name} — bait it with fruit or greens" : $"Set the {name}");
            // sfx_trap_set for the land traps; a Fish Trap goes into the water with a splash (sfx_splash).
            AudioManager audio = AudioManager.Instance;
            if (audio != null)
                audio.PlayAt(type == TrapType.FishTrap ? audio.Splash : audio.TrapSet, point);
        }
    }

    bool CanSet(TrapType type, out Vector3 point, out string reason)
    {
        point = Vector3.zero;
        string name = TrapManager.NameOf(type);
        if (!player.AimRaycast(reach, out RaycastHit hit))
        {
            reason = type == TrapType.FishTrap ? $"{name} — aim at water close by" : $"{name} — aim at the ground close by";
            return false;
        }

        WaterSource water = hit.collider.GetComponentInParent<WaterSource>();
        if (type == TrapType.FishTrap)
        {
            if (water == null || FishingManager.WaterAt(water, hit.point) == FishWater.None)
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
            if (hit.collider.GetComponentInParent<Terrain>() == null || Vector3.Angle(hit.normal, Vector3.up) > maxSlope)
            {
                reason = $"{name} — needs open, level ground";
                return false;
            }
        }

        TrapManager traps = TrapManager.Instance;
        foreach (TrapState other in traps.Traps)
        {
            if (Vector3.Distance(other.position, hit.point) < minSpacing)
            {
                reason = "Too close to another trap";
                return false;
            }
        }

        point = hit.point;
        reason = "";
        return true;
    }
}
