using System;

// Hands and tools M0e-2 (Mike, 2026-11-05/06; Melee_Fists_and_Knife.md): the Knife in hand. Left click alternates a SLASH and a STAB (no Sprint+click: Mike changed that decision), the first click
// after a pause starting with a slash. Pure and engine-free like PunchRules, so edit-mode tests cover every number and pose. The same "visual only, no delay" rule as the fists and the tool swings:
// the blow, its sound, its stamina and its hit test happen AT THE CLICK (MeleeAttack); the picture shows the blade landing ContactSeconds later, then coming back to a loaded ready pose.
// Kept apart from SwingRules on purpose (WorkKind carries the tools' 0.9 s swing; the knife's cycles differ), so the tools' swings stay exactly as they were.
public static class KnifeRules
{
    public const string ToolId = "knife";

    // One slash or stab at most this long after the last. Slash first, stab second.
    public const float SlashCycleSeconds = 0.7f, StabCycleSeconds = 0.8f;

    // The picture: seconds from the click to the blade landing, and how long it stays there before it comes back.
    public const float SlashContactSeconds = 0.12f, StabContactSeconds = 0.10f, DwellSeconds = 0.04f;

    // "Too tired" retries every half second while the click is held, like the tools and the fists (SwingRules.TiredSeconds); the droop takes the same time.
    public const float TiredSeconds = SwingRules.TiredSeconds;

    // With no new blow the hand settles from the loaded pose to the cocked ready pose over this long (the tools' settle).
    public const float SettleSeconds = SwingRules.SettleSeconds;

    // What a blow costs and reaches. Stamina: a little under a tool blow (4). Reach: the aim raycast the tools use (PlayerController.AimRaycast), stopped by the first collider in the way.
    public const float StaminaPerSlash = 3f, StaminaPerStab = 3f, SlashReachMetres = 1.5f, StabReachMetres = 1.6f;

    // How far the noise of a hit carries: quieter than a fist's 8 m (PunchRules.AlertRadius), far less than a gunshot.
    public const float AlertRadius = 6f;

    // The pause after which the next click starts over with a slash: this long with no blow at all.
    public const float PauseResetSeconds = 1.5f;

    static HandPose P(float x, float y, float z, float pitch, float yaw = 0f, float roll = 0f) =>
        new HandPose { X = x, Y = y, Z = z, Pitch = pitch, Yaw = yaw, Roll = roll };

    public static HandPose Zero => new HandPose();

    public static bool IsKnife(string toolId) => toolId == ToolId;

    public static float CycleSeconds(bool stab) => stab ? StabCycleSeconds : SlashCycleSeconds;
    public static float ContactSeconds(bool stab) => stab ? StabContactSeconds : SlashContactSeconds;
    public static float StaminaCost(bool stab) => stab ? StaminaPerStab : StaminaPerSlash;
    public static float ReachMetres(bool stab) => stab ? StabReachMetres : SlashReachMetres;

    // Whether a click makes a blow this frame: the world lets tools be used (not placing, not knocked down by the creek, not pushing the cart, only while playing), the Knife is the tool in hand,
    // the click is held, and the cycle of the last blow is over. MeleeAttack asks this and nothing else.
    public static bool ShouldAttack(bool canUseTools, bool knifeEquipped, bool clickHeld, bool cycleReady) => canUseTools && knifeEquipped && clickHeld && cycleReady;

    // The next blow: a slash when there has been none yet or the last was PauseResetSeconds or more ago, otherwise the other one. secondsSinceLastBlow counts from the last blow's click
    // (a refusal for tiredness is not a blow).
    public static bool NextIsStab(bool hasLastBlow, bool lastWasStab, float secondsSinceLastBlow)
    {
        if (!hasLastBlow || secondsSinceLastBlow >= PauseResetSeconds)
            return false;
        return !lastWasStab;
    }

    // The right hand's ready poses, as offsets from the neutral hold: cocked (the blade drawn back and up a little, the wrist turned away so the point is off the crosshair) and loaded (a little
    // farther back, where a blow recovers to, so a held click shows a draw before every blow). The left hand stays relaxed.
    public static HandPose Cocked => P(0.02f, 0.015f, -0.03f, -12f, 4f, 8f);
    public static HandPose Loaded => P(0.03f, 0.03f, -0.05f, -18f, 6f, 12f);

    // Where the hand is, relative to the neutral hold, at the blow. Slash: a diagonal arc, from high on the right down across the view to the left (the wrist rolls and turns with it). Stab: a
    // straight thrust along the view with a slight lean and almost no turn.
    public static HandPose Strike(bool stab) =>
        stab ? P(-0.04f, -0.01f, 0.17f, 6f, -6f, -4f)
             : P(-0.13f, -0.03f, 0.10f, 16f, -20f, -28f);

    // The hand's offset from the neutral hold, t seconds after the click, for a blow that starts from the pose from (cocked after a pause, loaded in a held click) and recovers to loaded: nothing
    // has moved at t = 0 (no delay), the blow lands at the contact time, holds through the dwell, then comes back to loaded exactly at the end of the cycle.
    public static HandPose Offset(bool stab, float t, HandPose from, HandPose loaded)
    {
        float cycle = CycleSeconds(stab), contact = ContactSeconds(stab);
        if (t <= 0f)
            return from;
        if (t >= cycle)
            return loaded;
        HandPose strike = Strike(stab);
        if (t < contact)
        {
            float u = t / contact;
            return SwingRules.Lerp(from, strike, u * u); // fast in: slow start, hard arrival
        }
        if (t < contact + DwellSeconds)
            return strike;
        float r = (t - contact - DwellSeconds) / (cycle - contact - DwellSeconds);
        return SwingRules.Lerp(strike, loaded, HandPoseRules.Ease(r));
    }

    // "Too tired": the same half raise and droop as a tired tool or fist, no blow. Zero at both ends.
    public static HandPose TiredOffset(float t) => SwingRules.TiredOffset(t);

    // What the blade does to an animal in reach (MeleeAttack applies it). A dead animal or a Carcass: nothing. Small game (SmallGameIds) dies from any solid hit, wounded or not: slash and stab
    // are the same. Deer: startled and gone, never hurt (Mike's decision 1: Medium and Large game is never harmed by melee).
    public enum Outcome { Nothing, Kill, StartleOnly }

    public static Outcome ForAnimal(bool dead, bool smallGame)
    {
        if (dead)
            return Outcome.Nothing;
        return smallGame ? Outcome.Kill : Outcome.StartleOnly;
    }

    // The species the Knife kills. Keyed by species id, not by Animal's GameTier, because the Turkey and the Duck are tier Medium in the prefabs (their shots need a vitals hit) but Mike counts
    // them as small game a Knife can finish. The Deer is not in the list.
    public static readonly string[] SmallGameIds = { "rabbit", "squirrel", "turkey", "waterfowl" };

    public static bool IsSmallGame(string speciesId) => speciesId != null && Array.IndexOf(SmallGameIds, speciesId) >= 0;
}

// The right hand's knife offset over time: the ready pose (cocked), the blow in progress, and the settle after it. PlayerHands starts it from ToolEvents.Knifed and ticks it every frame. A new blow
// replaces the one in progress and starts from wherever the hand is. Pure, so a test can step it with fixed time. Not used unless the Knife is the tool in hand (Configure).
public sealed class KnifeState
{
    float time;
    float settleTime = KnifeRules.SettleSeconds; // settled: not in a settle
    bool inHand;
    HandPose from;

    public bool Active { get; private set; }
    public bool Tired { get; private set; }
    public bool Stab { get; private set; }
    public float Time => time;
    public bool InHand => inHand;

    // The tool in hand changed: whether it is the Knife. Drops any blow in progress.
    public void Configure(bool knifeInHand)
    {
        inHand = knifeInHand;
        Stop();
    }

    public HandPose Rest => inHand ? KnifeRules.Cocked : KnifeRules.Zero;
    public HandPose LoadedPose => inHand ? KnifeRules.Loaded : KnifeRules.Zero;

    public void Start(bool stab)
    {
        from = Offset; // wherever the hand is now: cocked after a pause, loaded in a held click, or partway through a settle
        Stab = stab;
        Tired = false;
        Active = true;
        time = 0f;
    }

    public void StartTired()
    {
        Tired = true;
        Active = true;
        time = 0f;
    }

    public void Tick(float deltaSeconds)
    {
        float dt = Math.Max(0f, deltaSeconds);
        if (!Active)
        {
            settleTime = Math.Min(KnifeRules.SettleSeconds, settleTime + dt);
            return;
        }
        time += dt;
        if (time >= (Tired ? KnifeRules.TiredSeconds : KnifeRules.CycleSeconds(Stab)))
        {
            // A blow ends at the loaded pose and settles to cocked from there; a tired droop ends at cocked.
            settleTime = Tired ? KnifeRules.SettleSeconds : 0f;
            Active = false;
            time = 0f;
        }
    }

    // Jumps to this moment of the blow in progress (tests and screenshots look at a pose without waiting for it).
    public void Seek(float seconds)
    {
        if (Active)
            time = Math.Max(0f, seconds);
    }

    public void Stop()
    {
        Active = false;
        Tired = false;
        time = 0f;
        settleTime = KnifeRules.SettleSeconds;
    }

    public HandPose Offset
    {
        get
        {
            if (Active)
                return Tired ? SwingRules.Sum(Rest, KnifeRules.TiredOffset(time)) : KnifeRules.Offset(Stab, time, from, LoadedPose);
            if (settleTime < KnifeRules.SettleSeconds)
                return SwingRules.Lerp(LoadedPose, Rest, HandPoseRules.Ease(settleTime / KnifeRules.SettleSeconds));
            return Rest;
        }
    }
}
