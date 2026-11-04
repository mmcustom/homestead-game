using System;

// Hands and tools M0a (Mike, 2026-11-04): where the first-person hands sit and how they move. Pure and engine-free (no Vector3 or Quaternion: positions are metres in the camera's own space,
// x right, y up, z forward, and angles are degrees), so edit-mode tests cover every pose. PlayerHands reads these numbers and moves the hand objects; nothing here knows about Unity.
// M0a covers rest, walking, sprinting, crouching and the raise and lower when a tool is equipped or put away. The swing poses come in M0b.
public enum HandSide { Left, Right }

public enum HandMotion { Idle, Walk, Sprint, Crouch, CrouchWalk }

public struct HandPose
{
    public float X, Y, Z;
    public float Pitch, Yaw, Roll;
}

public static class HandPoseRules
{
    // How long a hand takes to come up from the bottom of the screen (and to go back down when a tool is put away).
    public const float RaiseSeconds = 0.3f;

    // An empty pair of hands going down to make way for a tool takes this long: they are already near the bottom.
    public const float QuickLowerSeconds = 0.12f;

    // How far below its place a hand starts its raise, in metres: out of the bottom of the view.
    public const float BottomDrop = 0.55f;

    // The wrist of a relaxed, empty hand: low and out toward the edge of the view, fingers a little up and in.
    const float RestX = 0.29f, RestY = -0.19f, RestZ = 0.50f, RestPitch = -30f, RestYaw = 10f, RestRoll = 10f;

    // The wrist of a hand holding a tool: higher and nearer the middle. The right hand holds the tool, the left hand steadies it on a two-handed one.
    const float HeldRightX = 0.24f, HeldRightY = -0.20f, HeldRightZ = 0.50f, HeldRightPitch = -12f, HeldRightYaw = 6f, HeldRightRoll = 4f;
    const float HeldLeftX = 0.10f, HeldLeftY = -0.16f, HeldLeftZ = 0.56f, HeldLeftPitch = -32f, HeldLeftYaw = 45f, HeldLeftRoll = -10f;

    // Breathing: a slow rise and fall that never stops.
    const float BreathSeconds = 4.5f, BreathRise = 0.004f, BreathTilt = 0.6f;

    // The motion state from what the player is doing. Crouching wins over sprinting (a crouched player cannot sprint); standing still is Idle.
    public static HandMotion MotionFor(bool moving, bool sprinting, bool crouching)
    {
        if (crouching)
            return moving ? HandMotion.CrouchWalk : HandMotion.Crouch;
        if (sprinting && moving)
            return HandMotion.Sprint;
        return moving ? HandMotion.Walk : HandMotion.Idle;
    }

    // Steps per second the sway runs at: zero when standing still.
    public static float StepRate(HandMotion motion)
    {
        switch (motion)
        {
            case HandMotion.Walk: return 1.8f;
            case HandMotion.Sprint: return 2.7f;
            case HandMotion.CrouchWalk: return 1.2f;
            default: return 0f;
        }
    }

    // How big the step sway is, as a multiple of a walk's.
    public static float StepAmount(HandMotion motion)
    {
        switch (motion)
        {
            case HandMotion.Walk: return 1f;
            case HandMotion.Sprint: return 1.7f;
            case HandMotion.CrouchWalk: return 0.5f;
            default: return 0f;
        }
    }

    // How far the whole hand sits lower than usual: a little in a sprint, a little crouched.
    public static float Lowering(HandMotion motion)
    {
        switch (motion)
        {
            case HandMotion.Sprint: return 0.035f;
            case HandMotion.Crouch:
            case HandMotion.CrouchWalk: return 0.025f;
            default: return 0f;
        }
    }

    // Eases a 0..1 raise so a hand starts and stops softly.
    public static float Ease(float t)
    {
        t = Math.Max(0f, Math.Min(1f, t));
        return t * t * (3f - 2f * t);
    }

    // The pose of one hand. holding: this hand is on a tool (right hand always; the left only on a two-handed tool), otherwise it is relaxed at rest.
    // raise: 0 = at the bottom of the screen, 1 = in place (HandRaise). breathTime: seconds, always running. stepPhase: whole steps taken so far, only moving while the player moves.
    public static HandPose Pose(HandSide side, bool holding, float raise, float breathTime, float stepPhase, HandMotion motion)
    {
        HandPose pose = Target(side, holding);

        // Breathing, a little out of step between the hands.
        float breath = (float)Math.Sin(2.0 * Math.PI * (breathTime / BreathSeconds + (side == HandSide.Right ? 0.0 : 0.13)));
        pose.Y += BreathRise * breath;
        pose.Pitch += BreathTilt * breath;

        // Step sway: a bob twice per stride and a slight side-to-side, opposite for the two hands.
        float amount = StepAmount(motion);
        if (amount > 0f)
        {
            float stride = (float)(2.0 * Math.PI * stepPhase);
            float lateral = side == HandSide.Right ? 1f : -1f;
            pose.X += 0.008f * amount * lateral * (float)Math.Sin(stride);
            pose.Y += 0.012f * amount * (float)Math.Abs(Math.Sin(stride));
            pose.Roll += 1.5f * amount * lateral * (float)Math.Sin(stride);
        }

        pose.Y -= Lowering(motion);

        // The raise: from the bottom of the screen, tilted a little further up so the hand swings into place.
        float e = Ease(raise);
        pose.Y -= (1f - e) * BottomDrop;
        pose.Pitch += (1f - e) * 25f;
        return pose;
    }

    // Where a hand sits when settled, before sway.
    public static HandPose Target(HandSide side, bool holding)
    {
        if (!holding)
        {
            // Mirror the rest pose for the left hand.
            float m = side == HandSide.Right ? 1f : -1f;
            return new HandPose { X = RestX * m, Y = RestY, Z = RestZ, Pitch = RestPitch, Yaw = RestYaw * -m, Roll = RestRoll * m };
        }
        return side == HandSide.Right
            ? new HandPose { X = HeldRightX, Y = HeldRightY, Z = HeldRightZ, Pitch = HeldRightPitch, Yaw = HeldRightYaw, Roll = HeldRightRoll }
            : new HandPose { X = HeldLeftX, Y = HeldLeftY, Z = HeldLeftZ, Pitch = HeldLeftPitch, Yaw = HeldLeftYaw, Roll = HeldLeftRoll };
    }

    // The rifle's scope overlay shows once the scope fraction passes this (HuntingWeapon draws ReticleKind.Scope past 0.5). The hands are fully down by then and are not drawn at all.
    public const float ScopeHideAt = 0.5f;

    // How much of the raise is left as the scope comes up: 1 with the scope down, smoothly to 0 at ScopeHideAt, so the hands slide out of view with it.
    public static float ScopeLowering(float scopeFraction) => 1f - Ease(scopeFraction / ScopeHideAt);

    // True while the scope overlay is up: no hands, no held tool.
    public static bool HiddenByScope(float scopeFraction) => scopeFraction >= ScopeHideAt;

    // Takes a layer out of a camera's culling mask (the Viewmodel layer, so the world camera never draws the hands).
    public static int MaskWithout(int cullingMask, int layer) => layer >= 0 && layer < 32 ? cullingMask & ~(1 << layer) : cullingMask;

    public static bool MaskHas(int cullingMask, int layer) => layer >= 0 && layer < 32 && (cullingMask & (1 << layer)) != 0;
}

// Which hands a tool needs, by item id. One table: a two-handed tool shows the left hand on it, a one-handed one hides the left hand, and an empty hand pair shows both at rest.
public static class HeldTools
{
    public static readonly string[] TwoHandedIds =
    {
        "axe", "primitive_axe", "wood_saw",
        "shovel", "iron_shovel", "stone_pick_axe", "iron_pick_axe",
        "fishing_rod", "cane_pole",
        "primitive_bow", "recurve_bow", "bolt_action_rifle",
    };

    public static bool IsTwoHanded(string toolId) => toolId != null && Array.IndexOf(TwoHandedIds, toolId) >= 0;

    // Empty-handed (null): both hands show at rest. A two-handed tool shows both on it. Anything else is held in the right hand alone.
    public static bool LeftHandShown(string toolId) => toolId == null || IsTwoHanded(toolId);

    // The left hand is on the tool (steadying it) rather than at rest.
    public static bool LeftHandHolds(string toolId) => IsTwoHanded(toolId);
}

// What the hands are showing and how far up they are. Tick it every frame with the tool that should be in hand: when that differs from what is shown, the hands go down first (the way a put-away
// reverses a raise), the tool changes at the bottom, and they come back up. Pure, so a test can step it with fixed time.
public sealed class HandRaise
{
    public string Shown { get; private set; }
    public float Raise { get; private set; } = 1f;

    // Straight to this tool, fully raised (a loaded game starts with the tool already up).
    public void Snap(string wanted)
    {
        Shown = wanted;
        Raise = 1f;
    }

    public void Tick(float deltaSeconds, string wanted)
    {
        if (deltaSeconds < 0f)
            deltaSeconds = 0f;

        if (wanted != Shown)
        {
            float down = Shown == null ? HandPoseRules.QuickLowerSeconds : HandPoseRules.RaiseSeconds;
            Raise = Math.Max(0f, Raise - deltaSeconds / down);
            if (Raise <= 0f)
                Shown = wanted;
            return;
        }
        Raise = Math.Min(1f, Raise + deltaSeconds / HandPoseRules.RaiseSeconds);
    }
}
