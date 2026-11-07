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
//
// M0e-2: the same click, with the KNIFE equipped, slashes then stabs (KnifeRules: alternating, the first after a 1.5 s pause a slash; 3 stamina, 1.5 m / 1.6 m reach, noise 6 m). A knife kills small
// game (Rabbit, Squirrel, Turkey, Duck) through Animal.Slay and only startles a Deer; a trapped animal is not hittable here either (see above). Same CanUseTools gate; PlacementMode stays ahead of it.
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

    // M0e-2: the Knife is the equipped tool.
    static bool KnifeEquipped
    {
        get
        {
            InventoryManager inventory = InventoryManager.Instance;
            return inventory != null && inventory.EquippedTool != null && KnifeRules.IsKnife(inventory.EquippedTool.Id);
        }
    }

    // M0e-2: the Knife's own click state. Slash and stab alternate; a pause of KnifeRules.PauseResetSeconds with no blow starts over with a slash. A refusal for tiredness is not a blow.
    float nextKnife;
    float lastKnifeTime;
    bool hasLastKnife, lastWasStab;

    public bool NextIsStab => KnifeRules.NextIsStab(hasLastKnife, lastWasStab, Time.time - lastKnifeTime);

    // M0e-3: the aimed-target prompt. While you aim at a LIVE animal within the reach of what is in your hands (the same raycast as the blow: the first collider stops it, so never through a wall), the
    // HUD says what a click will do. Reported through ToolStatus.Report like every other aimed prompt; the text is MeleeRules.PromptFor. The Knife shows the NEXT blow (NextIsStab), not both. A dead
    // animal or a Carcass gets no melee prompt, so its own field-dress or harvest prompt (the interaction prompt) is untouched.
    void UpdatePrompt()
    {
        MeleeRules.Mode mode = MeleeRules.ModeFor(HandsEmpty, KnifeEquipped);
        bool canUse = player.CanUseTools;
        bool stab = NextIsStab;
        Animal target = canUse && mode != MeleeRules.Mode.None ? TargetAnimalWithin(player, MeleeRules.ReachFor(mode, stab)) : null;
        string text = MeleeRules.PromptFor(canUse, mode, stab, target != null, target != null && target.IsDead, target != null ? target.DisplayName : null);
        if (text != null)
            ToolStatus.Report(text);
    }

    void Update()
    {
        if (attack == null || player == null)
            return;
        UpdatePrompt();
        if (KnifeEquipped)
        {
            UpdateKnife();
            return;
        }
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
        if (JournalManager.Instance != null)
            JournalManager.Instance.NoteHappening(KnowHowRules.Happening.Punched); // the first punch opens the Know-how entry (once; later calls add nothing)
        Strike(player);
    }

    // The Knife with its click held: a slash or a stab, one per cycle. PlacementMode and the rest are covered by CanUseTools, as for the fists. The blow, its sound, its stamina and its hit test all
    // happen here, at the click; PlayerHands draws the blade landing a moment later (ToolEvents.Knifed).
    void UpdateKnife()
    {
        if (!KnifeRules.ShouldAttack(player.CanUseTools, true, attack.IsPressed(), Time.time >= nextKnife))
            return;

        bool stab = NextIsStab;
        if (!player.TrySpendStamina(KnifeRules.StaminaCost(stab)))
        {
            nextKnife = Time.time + KnifeRules.TiredSeconds; // too tired: no blow, the hand droops, and no free blows
            ToolEvents.RaiseKnife(stab, true);
            return;
        }

        hasLastKnife = true;
        lastWasStab = stab;
        lastKnifeTime = Time.time;
        nextKnife = Time.time + KnifeRules.CycleSeconds(stab);
        ToolEvents.RaiseKnife(stab, false);
        KnifeStrike(player, stab);
    }

    // The Knife's blow, at the click: the sound, the noise, then whatever the aim raycast finds within reach (1.5 m slash, 1.6 m stab).
    public static void KnifeStrike(PlayerController player, bool stab)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.Play(SoundCue.ItemDrop); // the closest existing sound; no audio file added
        Vector3 from = player.transform.position;
        Animal target = TargetAnimalWithin(player, KnifeRules.ReachMetres(stab));
        if (target != null)
            HitWithKnife(target, from);
        if (WildlifeManager.Instance != null)
            WildlifeManager.Instance.Alert(from, KnifeRules.AlertRadius);
    }

    // What the blade does: nothing to the dead, a kill to small game (alive or wounded), a startle to a Deer. Never a wound.
    public static void HitWithKnife(Animal animal, Vector3 from)
    {
        KnifeRules.Outcome outcome = KnifeRules.ForAnimal(animal.IsDead, KnifeRules.IsSmallGame(animal.SpeciesId));
        if (outcome != KnifeRules.Outcome.Nothing && AudioManager.Instance != null)
            AudioManager.Instance.PlayMeleeHit(animal.transform.position); // M0e-3: a second cue on a hit; the blow itself keeps its click-time cue
        switch (outcome)
        {
            case KnifeRules.Outcome.Kill:
                animal.Slay();
                break;
            case KnifeRules.Outcome.StartleOnly:
                animal.Startle(from);
                break;
        }
    }

    // The animal the aim raycast reaches first within this many metres; the first collider in the way stops it.
    public static Animal TargetAnimalWithin(PlayerController player, float reach) =>
        player != null && player.AimRaycast(reach, out RaycastHit hit) ? hit.collider.GetComponentInParent<Animal>() : null;

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
        switch (PunchRules.ForAnimal(animal.IsDead, PunchRules.IsSmallGame(animal.SpeciesId))) // M0e-3: by species id (rabbit, squirrel, turkey, waterfowl), the list the Knife shares
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
