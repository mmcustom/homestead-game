using UnityEngine;

// A placed Rabbit Snare, Box Trap or Fish Trap (TrapManager owns its state). Looking at it shows what it is, how good
// the spot is and its condition; E checks it — taking any catch — or baits an empty Box Trap, and R repairs a worn
// trap or picks it back up. A caught animal or fish shows in the trap until it's checked.
public class Trap : MonoBehaviour, IInteractable, ISecondaryInteractable
{
    [Tooltip("Shown while there's a catch waiting.")]
    [SerializeField] GameObject catchVisual;
    [Tooltip("Shown while a Box Trap is baited.")]
    [SerializeField] GameObject baitVisual;
    [Tooltip("Tilted over when the trap is broken.")]
    [SerializeField] Transform body;

    TrapState state;
    Quaternion bodyRest = Quaternion.identity;

    public TrapState State => state;

    void Awake()
    {
        if (body != null)
            bodyRest = body.localRotation;
    }

    public void Bind(TrapState trapState)
    {
        state = trapState;
        Refresh();
    }

    public void Refresh()
    {
        if (state == null)
            return;

        bool caught = state.type == TrapType.FishTrap ? state.catchCount > 0
            : state.result == TrapResult.SuccessfulCatch || state.result == TrapResult.SmallCatch;
        if (catchVisual != null)
            catchVisual.SetActive(caught);
        if (baitVisual != null)
            baitVisual.SetActive(!string.IsNullOrEmpty(state.bait));
        if (body != null)
            body.localRotation = state.condition <= 0f ? bodyRest * Quaternion.Euler(0f, 0f, 25f) : bodyRest;
    }

    string Name => TrapManager.NameOf(state.type);

    string Details => state.condition <= 0f
        ? "broken"
        : $"{TrapManager.PlacementWord(state.placement)} spot, {Mathf.RoundToInt(state.condition * 100f)}% condition";

    public string InteractionPrompt
    {
        get
        {
            TrapManager traps = TrapManager.Instance;
            if (state == null || traps == null)
                return "";

            if (state.type == TrapType.FishTrap && state.catchCount > 0)
                return $"Check {Name}  — fish in it!";
            if (state.result == TrapResult.SuccessfulCatch || state.result == TrapResult.SmallCatch)
                return $"Check {Name}  — something's caught!";

            if (traps.NeedsBait(state))
            {
                string bait = TrapManager.CarriedBait();
                if (bait != null)
                    return $"Bait {Name}  with {ItemDatabase.Get(bait)?.DisplayName ?? bait}";
                return $"Check {Name}  (unbaited — carry fruit or greens to bait it; {Details})";
            }

            string baitNote = state.type == TrapType.BoxTrap ? $", baited with {ItemDatabase.Get(state.bait)?.DisplayName ?? state.bait}" : "";
            return $"Check {Name}  ({Details}{baitNote})";
        }
    }

    public bool CanInteract(PlayerController player) => state != null && TrapManager.Instance != null;

    public void Interact(PlayerController player)
    {
        TrapManager traps = TrapManager.Instance;
        if (state == null || traps == null)
            return;

        bool caught = state.catchCount > 0 || state.result == TrapResult.SuccessfulCatch || state.result == TrapResult.SmallCatch;
        if (!caught && traps.NeedsBait(state) && traps.Bait(state))
        {
            ToolStatus.Flash($"Baited the {Name}");
            return;
        }

        ToolStatus.Flash(traps.Check(state));
    }

    public string SecondaryPrompt
    {
        get
        {
            TrapManager traps = TrapManager.Instance;
            if (state == null || traps == null)
                return "";
            if (state.condition < 0.7f && traps.CanRepair(state))
                return $"Repair  (1 {ItemDatabase.Get(TrapManager.RepairItem(state.type))?.DisplayName})";
            return $"Pick Up {Name}";
        }
    }

    public void SecondaryInteract(PlayerController player)
    {
        TrapManager traps = TrapManager.Instance;
        if (state == null || traps == null)
            return;

        if (state.condition < 0.7f && traps.Repair(state))
            ToolStatus.Flash($"Repaired the {Name}");
        else if (traps.PickUp(state))
            ToolStatus.Flash($"Picked up the {Name}");
        else
            ToolStatus.Flash("No room to carry it");
    }
}
