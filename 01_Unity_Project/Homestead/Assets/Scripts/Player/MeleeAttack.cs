using UnityEngine;
using UnityEngine.InputSystem;

// Hands and tools M0e-1 (Mike, 2026-11-05; Melee_Fists_and_Knife.md): fists. With empty hands, holding the left click (the Player/Attack action) throws punches, right then left then right, one per
// PunchRules.CycleSeconds. The blow, its sound, its stamina and its hit test all happen at the click, as the axe's do; PlayerHands draws the fist landing a moment later (ToolEvents.Punched), so there
// is no delay. Added to the Player at run time (PlayerController.Awake, like the Cart handler and the hands), so no prefab or scene is edited.
//
// It only acts with EMPTY hands and while the world lets tools be used (PlayerController.CanUseTools: not placing, not knocked down by the creek, not pushing the Hand Cart, only while the game is
// playing). Every tool script reads the same click only while its own tool is equipped, so nothing else reads it with empty hands.
//
// FISTS NEVER KILL. A punch only stuns a small animal for a moment and startles it, or just startles a bigger one (PunchRules.ForAnimal), whether it was wounded already or not. The noise carries
// PunchRules.AlertRadius (8 m): everything that close bolts, which on its own means a punch rarely finds anything to hit (see the report). A trapped animal is only a string in TrapManager, not a live
// Animal, so a punch at a trap hits nothing and only makes the noise.
public class MeleeAttack : MonoBehaviour
{
    PlayerController player;
    InputAction attack;
    float nextPunch;
    bool nextRight = true;

    public bool NextIsRight => nextRight;

    void Awake()
    {
        player = GetComponent<PlayerController>();
        attack = InputSystem.actions != null ? InputSystem.actions.FindAction("Player/Attack") : null;
    }

    // Whether the hands are empty: nothing is equipped.
    static bool HandsEmpty
    {
        get
        {
            InventoryManager inventory = InventoryManager.Instance;
            return inventory == null || inventory.EquippedTool == null;
        }
    }

    void Update()
    {
        if (attack == null || player == null)
            return;
        if (!PunchRules.ShouldPunch(player.CanUseTools, HandsEmpty, attack.IsPressed(), Time.time >= nextPunch))
            return;

        if (!player.TrySpendStamina(PunchRules.StaminaPerPunch))
        {
            nextPunch = Time.time + PunchRules.TiredSeconds; // too tired: no punch, the hands droop, and no free punches
            ToolEvents.RaisePunch(nextRight, true);
            return;
        }

        bool right = nextRight;
        nextRight = PunchRules.NextIsRight(nextRight);
        nextPunch = Time.time + PunchRules.CycleSeconds;
        ToolEvents.RaisePunch(right, false);
        Strike(player);
    }

    // The blow itself, at the click: the sound, the noise, then whatever the aim raycast finds within reach.
    public static void Strike(PlayerController player)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.Play(SoundCue.ItemDrop); // the soft thud of something dropped: the closest existing sound; no audio file added
        Vector3 from = player.transform.position;
        if (WildlifeManager.Instance != null)
            WildlifeManager.Instance.Alert(from, PunchRules.AlertRadius);
        Animal target = TargetAnimal(player);
        if (target != null)
            Hit(target, from);
    }

    // The animal the aim raycast reaches first, within PunchRules.ReachMetres. The raycast stops at the first collider in the way, so a punch never reaches through a wall.
    public static Animal TargetAnimal(PlayerController player) =>
        player != null && player.AimRaycast(PunchRules.ReachMetres, out RaycastHit hit) ? hit.collider.GetComponentInParent<Animal>() : null;

    // What the fist does: nothing to the dead, a stun and a startle to a small live animal, a startle to a bigger one. Never a kill, never a wound.
    public static void Hit(Animal animal, Vector3 from)
    {
        switch (PunchRules.ForAnimal(animal.IsDead, animal.Tier == GameTier.Small))
        {
            case PunchRules.Outcome.StunAndStartle:
                animal.Stun(PunchRules.StunSeconds, from);
                break;
            case PunchRules.Outcome.StartleOnly:
                animal.Startle(from);
                break;
        }
    }
}
