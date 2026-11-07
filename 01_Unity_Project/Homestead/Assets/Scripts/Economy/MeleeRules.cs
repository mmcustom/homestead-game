using System;

// Hands and tools M0e-3 (Mike, 2026-11-07; Melee_Fists_and_Knife.md): what the fists and the Knife share. Pure and engine-free like PunchRules and KnifeRules, so edit-mode tests cover every string.
//
// One list of small game for both: the species a fist STUNS (never kills) and a Knife KILLS. Keyed by species id, not by Animal's GameTier, because the Turkey and the Duck are tier Medium in the
// prefabs (a shot needs a vitals hit for them) but Mike counts them as small game for melee. The Deer is not in the list: startled only, never hurt.
//
// And the aimed-target prompt: while you aim at a LIVE animal within the reach of what is in your hands, a short line says what a click will do. It is reported through ToolStatus.Report like every
// other aimed prompt (MeleeAttack, every frame). The Knife shows the NEXT blow only, from MeleeAttack.NextIsStab; the old "hold Sprint to stab" plan is gone (the Knife alternates per click).
public static class MeleeRules
{
    public static readonly string[] SmallGameIds = { "rabbit", "squirrel", "turkey", "waterfowl" };

    public static bool IsSmallGame(string speciesId) => speciesId != null && Array.IndexOf(SmallGameIds, speciesId) >= 0;

    // What is in the hands: nothing (fists), the Knife, or something else (no melee, no prompt).
    public enum Mode { None, Fists, Knife }

    public static Mode ModeFor(bool handsEmpty, bool knifeEquipped) => knifeEquipped ? Mode.Knife : handsEmpty ? Mode.Fists : Mode.None;

    // How far the next blow reaches, the same number the blow's own raycast uses (PunchRules.ReachMetres, KnifeRules.ReachMetres). Zero when there is no melee.
    public static float ReachFor(Mode mode, bool nextIsStab)
    {
        switch (mode)
        {
            case Mode.Fists: return PunchRules.ReachMetres;
            case Mode.Knife: return KnifeRules.ReachMetres(nextIsStab);
            default: return 0f;
        }
    }

    public const string PunchVerb = "punch", SlashVerb = "slash", StabVerb = "stab";

    // "Rabbit: click to punch". The name is the animal's display name; a blank one reads as "Animal".
    public static string Prompt(string animalName, string verb) => $"{(string.IsNullOrEmpty(animalName) ? "Animal" : animalName)}: click to {verb}";

    // The prompt for this frame, or null for none. A prompt only when: the world lets tools be used (not placing, not knocked down, not pushing the Hand Cart, not in a menu), something melee is in the
    // hands, the aim raycast found an animal within the reach of the next blow (hasTarget; the raycast stops at the first collider, so never through a wall) and it is alive (a dead animal or a
    // Carcass keeps its own harvest prompt).
    public static string PromptFor(bool canUseTools, Mode mode, bool nextIsStab, bool hasTarget, bool targetDead, string animalName)
    {
        if (!canUseTools || mode == Mode.None || !hasTarget || targetDead)
            return null;
        string verb = mode == Mode.Fists ? PunchVerb : nextIsStab ? StabVerb : SlashVerb;
        return Prompt(animalName, verb);
    }
}
