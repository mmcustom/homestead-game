using System;

// Hands and tools M0e-1 (Mike, 2026-11-05): fists. With empty hands a left click throws a punch; punches alternate right, left, right, left. Pure and engine-free like SwingRules, so edit-mode tests
// cover every pose and number. The same "visual only, no delay" rule as the tool swings: the blow, its sound, its stamina and its hit test happen AT THE CLICK (MeleeAttack); the picture shows the
// fist landing ContactSeconds later, then coming back. It is kept apart from SwingRules on purpose: WorkKind carries the tools' 0.9 s swing and 0.15 s contact, which a punch does not share, and
// the tools' swings stay exactly as they were. Fists NEVER kill (Mike, 2026-11-05): they stun and startle, nothing more.
public static class PunchRules
{
    // The cycle of a held click: one punch at most this long after the last. Faster than the 0.9 s tool swing.
    public const float CycleSeconds = 0.6f;

    // The picture: seconds from the click to the fist landing, and how long it stays there before it comes back. No wind-up: the hand has not moved at the click.
    public const float ContactSeconds = 0.10f, DwellSeconds = 0.04f;

    // "Too tired to punch" retries every half second while the click is held, like the tools (SwingRules.TiredSeconds); the droop takes the same time.
    public const float TiredSeconds = SwingRules.TiredSeconds;

    // What a punch costs and reaches. Stamina: half a tool blow (4). Reach: the same aim raycast the tools use (PlayerController.AimRaycast), stopped by the first collider in the way.
    public const float StaminaPerPunch = 2f, ReachMetres = 1.4f;

    // What a hit does to a small live animal: it stops for this long, then runs as it normally would. And how far the noise carries: a fist is nothing like a gunshot (HuntingWeapon: 80 m).
    public const float StunSeconds = 1f, AlertRadius = 8f;

    static HandPose P(float x, float y, float z, float pitch, float yaw = 0f, float roll = 0f) =>
        new HandPose { X = x, Y = y, Z = z, Pitch = pitch, Yaw = yaw, Roll = roll };

    public static HandPose Zero => new HandPose();

    // Whether a click throws a punch this frame: the world lets tools be used (not placing, not knocked down, not pushing the cart, only while playing), nothing is equipped, the click is held, and
    // the cycle of the last punch is over. MeleeAttack asks this and nothing else.
    public static bool ShouldPunch(bool canUseTools, bool handsEmpty, bool clickHeld, bool cycleReady) => canUseTools && handsEmpty && clickHeld && cycleReady;

    // Punches alternate: right after left, left after right. (MeleeAttack starts with the right.)
    public static bool NextIsRight(bool lastWasRight) => !lastWasRight;

    // Where the fist is, relative to the resting hand, at contact: driven forward along the view, toward the middle of it and a little up, the wrist turned in. The left hand is the mirror image.
    // Stays inside HandPoseRules.MaxRise (the arm shows no more than the Lantern's did) and is never scaled: the arm is only moved.
    public static HandPose Strike(bool right) =>
        right ? P(-0.14f, 0.04f, 0.14f, 8f, -14f, -8f)
              : P(0.14f, 0.04f, 0.14f, 8f, 14f, 8f);

    // The hand's offset from rest, t seconds after the click: nothing at t = 0 (no delay), the strike at ContactSeconds (a fast drive, slow start and hard arrival), held through the dwell,
    // then back to rest exactly at CycleSeconds.
    public static HandPose Offset(bool right, float t)
    {
        if (t <= 0f || t >= CycleSeconds)
            return Zero;
        HandPose strike = Strike(right);
        if (t < ContactSeconds)
        {
            float u = t / ContactSeconds;
            return SwingRules.Lerp(Zero, strike, u * u);
        }
        if (t < ContactSeconds + DwellSeconds)
            return strike;
        float r = (t - ContactSeconds - DwellSeconds) / (CycleSeconds - ContactSeconds - DwellSeconds);
        return SwingRules.Lerp(strike, Zero, HandPoseRules.Ease(r));
    }

    // "Too tired to punch": the same half raise and droop as a tired tool, no strike. Zero at both ends.
    public static HandPose TiredOffset(float t) => SwingRules.TiredOffset(t);

    // What a fist does to an animal in reach (MeleeAttack applies it). Never harm. A dead animal or a Carcass: nothing. A small live one (Rabbit, Squirrel, Turkey, Duck): stunned for a moment
    // and startled. A medium or large one (Deer): startled and gone. A wounded one gets exactly the same, with no extra damage.
    public enum Outcome { Nothing, StunAndStartle, StartleOnly }

    public static Outcome ForAnimal(bool dead, bool small)
    {
        if (dead)
            return Outcome.Nothing;
        return small ? Outcome.StunAndStartle : Outcome.StartleOnly;
    }
}

// The two fists over time, one state each, so the right and left hands punch alone, one at a time. PlayerHands starts it from ToolEvents.Punched and ticks it every frame. The tool swing
// (SwingState) is a separate object and is not touched. Pure, so a test can step it with fixed time.
public sealed class PunchState
{
    readonly float[] time = new float[2];      // 0 = right, 1 = left
    readonly bool[] active = new bool[2];
    readonly bool[] tired = new bool[2];

    static int Index(bool right) => right ? 0 : 1;

    public bool IsActive(bool right) => active[Index(right)];
    public bool IsTired(bool right) => tired[Index(right)];
    public float TimeOf(bool right) => time[Index(right)];
    public bool Any => active[0] || active[1];

    public void Start(bool right)
    {
        int i = Index(right);
        active[i] = true;
        tired[i] = false;
        time[i] = 0f;
    }

    public void StartTired(bool right)
    {
        int i = Index(right);
        active[i] = true;
        tired[i] = true;
        time[i] = 0f;
    }

    public void Tick(float deltaSeconds)
    {
        float dt = Math.Max(0f, deltaSeconds);
        for (int i = 0; i < 2; i++)
        {
            if (!active[i])
                continue;
            time[i] += dt;
            if (time[i] >= (tired[i] ? PunchRules.TiredSeconds : PunchRules.CycleSeconds))
            {
                active[i] = false;
                tired[i] = false;
                time[i] = 0f;
            }
        }
    }

    // Jumps to this moment of the punch in progress (tests and screenshots look at a pose without waiting for it).
    public void Seek(bool right, float seconds)
    {
        int i = Index(right);
        if (active[i])
            time[i] = Math.Max(0f, seconds);
    }

    public void Stop()
    {
        for (int i = 0; i < 2; i++)
        {
            active[i] = false;
            tired[i] = false;
            time[i] = 0f;
        }
    }

    // The hand's offset from its resting pose: the punch or the tired droop in progress, zero otherwise.
    public HandPose Offset(bool right)
    {
        int i = Index(right);
        if (!active[i])
            return PunchRules.Zero;
        return tired[i] ? PunchRules.TiredOffset(time[i]) : PunchRules.Offset(right, time[i]);
    }
}
