using System;

// Hands and tools M0c-1 (Mike, 2026-11-05): the poses of the Fishing Rod, the Cane Pole, the Torch and the Lantern in hand. Pure and engine-free like SwingRules and HandPoseRules (offsets from the
// neutral hold: metres in the rig's space and degrees), so edit-mode tests cover every pose. Nothing here adds game time or a delay: casting, a bite, a catch, lighting and snuffing all happen where
// the fishing and light scripts already do them, and the picture follows (ToolEvents). A hand animator that wants more than this listens to the same events.
public enum ToolAction { Cast, Bite, Catch, Miss, Light, Snuff, Release, Shot, Dip, Drink, Tap, PlaceDown }

public static class ToolActionRules
{
    public const string RodId = "fishing_rod", CaneId = "cane_pole", TorchId = "torch", LanternId = "lantern";

    // How long each one-shot takes. The cast is a back-then-forward flick; the bite a quick dip of the tip; the catch a short pull up and back; the miss (a missed hookset or a snapped line) a jerk.
    public const float CastSeconds = 0.6f, BiteSeconds = 0.35f, CatchSeconds = 0.6f, MissSeconds = 0.45f, LightSeconds = 0.45f, SnuffSeconds = 0.4f;

    // M0c-2: the bow's release snap, the rifle's kick and bolt, a fill at the water, a drink, the Spile's tap and a trap set down.
    public const float ReleaseSeconds = 0.25f, ShotSeconds = 1.2f, DipSeconds = 1.0f, DrinkSeconds = 1.1f, TapSeconds = 0.7f, PlaceDownSeconds = 0.7f;

    // How far a bow is drawn back at full draw (the hand and tool pull toward the player), and the bow drill's stroke: strokes a second and how far.
    public const float DrillStrokesPerSecond = 3f, DrillStroke = 0.05f;

    // The crank: turns a second while the line is being wound in, and how fast it starts and stops.
    public const float ReelTurnsPerSecond = 2.2f, ReelRampSeconds = 0.15f;

    public static bool IsRod(string toolId) => toolId == RodId || toolId == CaneId;
    public static bool IsLight(string toolId) => toolId == TorchId || toolId == LanternId;
    public static bool IsBow(string toolId) => toolId == "primitive_bow" || toolId == "recurve_bow";
    public static bool IsRifle(string toolId) => toolId == "bolt_action_rifle";
    public static bool IsDrill(string toolId) => toolId == "bow_drill";
    public static bool IsContainer(string toolId) => toolId == "bucket" || toolId == "sap_bucket" || toolId == "canteen";
    public static bool IsSpile(string toolId) => toolId == "spile";
    public static bool IsTrap(string toolId) => toolId == "box_trap" || toolId == "fish_trap" || toolId == "rabbit_snare";

    // Everything carried by a handle, bail, strap or cord (M0c-2 fix): the hand grips it at the top and the body hangs straight down in the view. The Lantern hangs the same way but is a light and
    // keeps its own sway; the Rabbit Snare is carried by its stake, so it is not in this list.
    public static readonly string[] HangingIds = { "bucket", "sap_bucket", "canteen", "cooking_pot", "potash_kettle", "pouch", "fish_trap", "box_trap" };
    public static bool IsHanging(string toolId) => toolId != null && Array.IndexOf(HangingIds, toolId) >= 0;

    // The tools whose pose this class drives (everything else keeps the plain neutral hold: the Knife, the Clay Pot, the tarp, Flint and Steel).
    public static bool Handles(string toolId) => IsRod(toolId) || IsLight(toolId) || IsBow(toolId) || IsRifle(toolId) || IsDrill(toolId) || IsContainer(toolId) || IsSpile(toolId) || IsTrap(toolId) || IsHanging(toolId);

    static HandPose P(float x, float y, float z, float pitch, float yaw = 0f, float roll = 0f) =>
        new HandPose { X = x, Y = y, Z = z, Pitch = pitch, Yaw = yaw, Roll = roll };

    static readonly HandPose Zero = new HandPose();

    // The carry pose (M0c-2 fix 2): the hand comes up only a little (Y 0.10, so the wrist is no higher above the bottom edge than the Lantern's: HandPoseRules.MaxRise), is turned to point left so the
    // sleeve trails off to the right and clears the body hanging below, and the body hangs short (a bail of 3.5 cm) so it all fits above the bottom of the screen.
    static readonly HandPose HangCarry = P(-0.04f, 0.10f, 0.03f, 0f, -22f);

    // The ready pose, relative to the neutral hold: a rod held out and up in front with the tip drooping a little; a torch held up and forward; a lantern hanging low.
    public static HandPose Ready(string toolId)
    {
        switch (toolId)
        {
            case RodId: return P(0f, 0.015f, 0.01f, -6f);
            case CaneId: return P(0f, 0.015f, 0.01f, -4f);
            case TorchId: return P(0f, 0.02f, 0.02f, -8f);
            case LanternId: return P(0f, 0.10f, 0.03f, -6f);
            case "primitive_bow": case "recurve_bow": return P(0f, 0f, 0f, -4f, -30f);   // the STRING hand (right): the bow itself is the left hand's (BowHandReady), the right hand rests on the nock
            case "bolt_action_rifle": return P(0f, 0.03f, 0.02f, -10f);                 // across the body, the muzzle up
            case "bow_drill": return P(0f, 0.01f, 0.02f, -6f);
            // The carry pose of everything that hangs (M0c-2 fix): the hand is out, forward and up, so the body hangs below it in plain view instead of behind the sleeve or off the bottom of the screen.
            // The taller the hanging body, the higher the hand. No pitch: the forearm keeps its natural angle and the tool hangs by gravity (PlayerHands.AlignHanging).
            case "bucket": case "sap_bucket": case "canteen": case "cooking_pot": case "potash_kettle": case "pouch": case "fish_trap": case "box_trap": return HangCarry;
            case "spile": return P(0f, 0.01f, 0.01f, -8f);
            case "rabbit_snare": return P(0f, 0.09f, 0.02f, 2f);
            default: return Zero;
        }
    }

    // One keyframed one-shot, as an offset from the ready pose: zero at the start and at the end.
    public static HandPose ActionOffset(ToolAction action, float t)
    {
        switch (action)
        {
            case ToolAction.Cast:
                // Back first (the flick's wind), then forward, then settle. It starts at the click with no movement: the cast itself has already happened.
                return Keys(t, new[] { 0f, 0.22f, 0.36f, 0.6f },
                               new[] { Zero, P(0f, 0.025f, -0.045f, -15f), P(0f, -0.02f, 0.09f, 17f), Zero });
            case ToolAction.Bite:
                return Keys(t, new[] { 0f, 0.06f, 0.15f, 0.24f, 0.35f },
                               new[] { Zero, P(0f, -0.02f, 0f, 11f), P(0f, 0.005f, 0f, -3f), P(0f, -0.01f, 0f, 5f), Zero });
            case ToolAction.Catch:
                return Keys(t, new[] { 0f, 0.25f, 0.6f },
                               new[] { Zero, P(0f, 0.04f, -0.08f, -20f), Zero });
            case ToolAction.Miss:
                return Keys(t, new[] { 0f, 0.07f, 0.18f, 0.45f },
                               new[] { Zero, P(0f, 0.02f, -0.02f, -16f), P(0f, -0.015f, 0.02f, 9f), Zero });
            case ToolAction.Light:
                return Keys(t, new[] { 0f, 0.2f, 0.45f },
                               new[] { Zero, P(0f, 0.05f, 0.01f, -8f), Zero });
            case ToolAction.Snuff:
                return Keys(t, new[] { 0f, 0.15f, 0.4f },
                               new[] { Zero, P(0f, -0.05f, 0f, 8f), Zero });
            case ToolAction.Release:
                // The bow's snap on the shot: it starts from the full-draw pose (the pull-back is already on), springs forward and settles.
                return Keys(t, new[] { 0f, 0.06f, 0.16f, 0.25f },
                               new[] { DrawOffset(1f, 0f), P(0f, -0.01f, 0.03f, 6f), P(0f, 0f, -0.01f, -2f), Zero });
            case ToolAction.Shot:
                // The rifle: the kick of the shot, then the bolt worked (the hand pulls back and pushes forward) while it reloads.
                return Keys(t, new[] { 0f, 0.05f, 0.2f, 0.45f, 0.6f, 0.85f, 1.2f },
                               new[] { Zero, P(0f, 0.02f, -0.07f, -9f), P(0f, 0.005f, -0.02f, -3f), Zero, P(0f, 0.02f, -0.04f, -4f), P(0f, 0.02f, 0.02f, -4f), Zero });
            case ToolAction.Dip:
                // Bucket or Canteen at the water: down, hold while it fills, up.
                return Keys(t, new[] { 0f, 0.3f, 0.6f, 0.9f, 1.0f },
                               new[] { Zero, P(0f, -0.07f, 0.05f, 12f), P(0f, -0.07f, 0.05f, 12f), P(0f, 0.02f, 0f, -3f), Zero });
            case ToolAction.Drink:
                return Keys(t, new[] { 0f, 0.35f, 0.75f, 1.1f },
                               new[] { Zero, P(0f, 0.03f, -0.07f, -26f), P(0f, 0.03f, -0.07f, -26f), Zero });
            case ToolAction.Tap:
                // The Spile pushed into the bark: a short drive, a back-off, and a second drive.
                return Keys(t, new[] { 0f, 0.12f, 0.24f, 0.36f, 0.48f, 0.7f },
                               new[] { Zero, P(0f, 0.01f, -0.03f, -6f), P(0f, -0.01f, 0.08f, 6f), P(0f, 0.005f, -0.02f, -4f), P(0f, -0.01f, 0.08f, 6f), Zero });
            case ToolAction.PlaceDown:
                return Keys(t, new[] { 0f, 0.25f, 0.45f, 0.7f },
                               new[] { Zero, P(0f, -0.08f, 0.04f, 14f), P(0f, -0.08f, 0.04f, 14f), Zero });
            default:
                return Zero;
        }
    }

    public static float Seconds(ToolAction action)
    {
        switch (action)
        {
            case ToolAction.Cast: return CastSeconds;
            case ToolAction.Bite: return BiteSeconds;
            case ToolAction.Catch: return CatchSeconds;
            case ToolAction.Miss: return MissSeconds;
            case ToolAction.Release: return ReleaseSeconds;
            case ToolAction.Shot: return ShotSeconds;
            case ToolAction.Dip: return DipSeconds;
            case ToolAction.Drink: return DrinkSeconds;
            case ToolAction.Tap: return TapSeconds;
            case ToolAction.PlaceDown: return PlaceDownSeconds;
            case ToolAction.Light: return LightSeconds;
            default: return SnuffSeconds;
        }
    }

    // The crank of the right hand while winding in: a small circle, started and stopped by weight (0..1).
    public static HandPose ReelOffset(float seconds, float weight)
    {
        float a = (float)(2.0 * Math.PI * ReelTurnsPerSecond * seconds);
        return Scale(P(0.012f * (float)Math.Cos(a), 0.012f * (float)Math.Sin(a), 0f, 1.6f * (float)Math.Sin(a)), weight);
    }

    // A bow drawn to this fraction (0..1): the STRING hand pulls back toward the face as the draw builds (the bow stays where it is), and at full draw holds with a faint tremble.
    public static HandPose DrawOffset(float draw, float seconds)
    {
        float d = Math.Max(0f, Math.Min(1f, draw));
        float tremble = d >= 1f ? 0.002f * (float)Math.Sin(2.0 * Math.PI * 11.0 * seconds) : 0f;
        // Round 3: mostly BACK toward the face (0.14 toward the camera), up a little and in toward the right cheek, with only a small sideways move (0.03). The hand turns from pointing at the string
        // to pointing forward. The string reads as pulled closer to the eye, not swung to the right.
        return P(0.03f * d, 0.04f * d + tremble, -0.14f * d, -6f * d, 14f * d, 0f);
    }

    // The bow drill's sawing: the hand drives back and forth about three times a second, eased in by weight.
    public static HandPose DrillOffset(float seconds, float weight)
    {
        float s = (float)Math.Sin(2.0 * Math.PI * DrillStrokesPerSecond * seconds);
        return Scale(P(0f, 0.004f * s, DrillStroke * s, 1.5f * s), weight);
    }

    // The rifle brought up as the scope rises (0..0.5, after which the hands are hidden anyway): the tool moves toward the middle of the view and comes up.
    public static HandPose AimOffset(float scope)
    {
        float s = Math.Max(0f, Math.Min(1f, scope / 0.5f));
        return P(-0.05f * s, 0.04f * s, 0.03f * s, -8f * s);
    }

    // The BOW HAND (M0c-2 fix): the bow is held still in the LEFT hand, out in front and a little left of centre, and never moves during a draw or a release; only the right hand does. This is the
    // left hand's offset from its two-handed hold: it turns the hand back out of its 45 degree reach toward the right so the bow stands upright in front.
    public static HandPose BowHandReady() => P(-0.28f, 0.04f, 0.02f, 0f, -15f, 0f); // round 4: a little higher, so the whole (shorter) bow is on screen; the wrist's Rise is 0.215, under MaxRise

    // How the bow itself stands in the view, whatever angle the left forearm comes in at: seen from its side (turned 75 degrees, so the limbs curve up and down in the view and the string faces right, toward the string hand), the top leaning a little toward the middle.
    public const float BowPitch = 0f, BowYaw = -20f, BowRoll = 0f; // no roll: the bow stands straight up and down

    // A hanging item's swing as the player walks (degrees): small, in step with the stride, bigger in a sprint (amount is HandPoseRules.StepAmount: 0 standing, 1 walking, 1.7 sprinting). Pitch is a
    // pendulum forward and back; roll a little side to side. Nothing here is big enough to move the body off the screen.
    public const float HangSwayPitch = 3.5f, HangSwayRoll = 2f;
    public static HandPose HangSway(float stepPhase, float amount)
    {
        float stride = (float)(2.0 * Math.PI * stepPhase);
        return P(0f, 0f, 0f, HangSwayPitch * amount * (float)Math.Sin(stride + 0.9), 0f, HangSwayRoll * amount * (float)Math.Sin(stride));
    }

    // The tiny settle when a hanging item is first equipped: a damped swing about the grip that dies out within about a second. Zero at the start and nothing at all once it has settled.
    public const float HangSettleSeconds = 1.2f;
    public static float HangSettle(float seconds)
    {
        if (seconds <= 0f || seconds >= HangSettleSeconds)
            return 0f;
        return 5f * (float)(Math.Exp(-3.5 * seconds) * Math.Sin(2.0 * Math.PI * 2.2 * seconds));
    }

    // A rod waiting for a bite: a still hold with a tiny sway.
    public static HandPose WaitSway(float seconds) =>
        P(0f, 0.004f * (float)Math.Sin(2.0 * Math.PI * seconds / 3.2), 0f, 0.8f * (float)Math.Sin(2.0 * Math.PI * seconds / 2.6));

    // A lit light's sway: the torch flickers about its hold; the lantern swings a little on its handle.
    public static HandPose LightSway(string toolId, float seconds)
    {
        if (toolId == LanternId)
            return P(0f, 0f, 0f, 0f, 0f, 3f * (float)Math.Sin(2.0 * Math.PI * seconds / 2.4));
        return P(0f, 0f, 0f, 0.5f * (float)Math.Sin(2.0 * Math.PI * seconds * 1.7), 0f, 1.2f * (float)Math.Sin(2.0 * Math.PI * seconds * 0.9 + 1.0));
    }

    static HandPose Scale(HandPose a, float s) => new HandPose { X = a.X * s, Y = a.Y * s, Z = a.Z * s, Pitch = a.Pitch * s, Yaw = a.Yaw * s, Roll = a.Roll * s };

    static HandPose Lerp(HandPose a, HandPose b, float s) => new HandPose
    {
        X = a.X + (b.X - a.X) * s, Y = a.Y + (b.Y - a.Y) * s, Z = a.Z + (b.Z - a.Z) * s,
        Pitch = a.Pitch + (b.Pitch - a.Pitch) * s, Yaw = a.Yaw + (b.Yaw - a.Yaw) * s, Roll = a.Roll + (b.Roll - a.Roll) * s,
    };

    // Eased keyframes: the pose at time t between the keys, zero outside the first and last.
    static HandPose Keys(float t, float[] times, HandPose[] poses)
    {
        if (t <= times[0])
            return poses[0];
        for (int i = 1; i < times.Length; i++)
        {
            if (t <= times[i])
                return Lerp(poses[i - 1], poses[i], HandPoseRules.Ease((t - times[i - 1]) / (times[i] - times[i - 1])));
        }
        return poses[poses.Length - 1];
    }

    public static HandPose Sum(HandPose a, HandPose b) => new HandPose
    {
        X = a.X + b.X, Y = a.Y + b.Y, Z = a.Z + b.Z, Pitch = a.Pitch + b.Pitch, Yaw = a.Yaw + b.Yaw, Roll = a.Roll + b.Roll,
    };

    public static float Magnitude(HandPose p) => (float)Math.Sqrt(p.X * p.X + p.Y * p.Y + p.Z * p.Z) + Math.Abs(p.Pitch) / 100f;
}

// The rod or light in hand over time: its ready pose, the one-shot in progress (cast, bite, catch, miss, light, snuff), whether a line is out, whether it is being wound in, and whether the light is
// lit. PlayerHands feeds it from ToolEvents and ticks it every frame. Pure, so a test can step it with fixed time.
public sealed class ToolActionState
{
    string toolId;
    float time;         // the one-shot's clock
    float sway;         // the always-running clock for the wait sway, the flicker and the crank
    float reelWeight;   // 0..1, eased in and out
    bool reeling;
    float draw;         // a bow's draw fraction, as the weapon reports it
    float aim;          // the rifle's scope fraction
    float workWeight;   // 0..1, the bow drill sawing, eased in and out
    bool working;

    public bool Active { get; private set; }
    public ToolAction Action { get; private set; }
    public bool Fishing { get; private set; }
    public bool Reeling => reeling;
    public float ReelWeight => reelWeight;
    public float OneShotTime => time;
    public string ToolId => toolId;

    // The tool in hand changed: forget everything that belonged to the old one.
    public void Configure(string id)
    {
        toolId = id;
        Active = false;
        Fishing = false;
        reeling = false;
        reelWeight = 0f;
        draw = 0f;
        aim = 0f;
        workWeight = 0f;
        working = false;
        time = 0f;
    }

    public void Start(ToolAction action)
    {
        Action = action;
        Active = true;
        time = 0f;
    }

    public void SetFishing(bool on)
    {
        Fishing = on;
        if (!on)
            reeling = false;
    }

    public void SetReeling(bool on) => reeling = on && Fishing;
    public void SetDraw(float fraction) => draw = Math.Max(0f, Math.Min(1f, fraction));
    public void SetAim(float scope) => aim = scope;
    public void SetWorking(bool on) => working = on;
    public float Draw => draw;
    public float WorkWeight => workWeight;

    public void Tick(float deltaSeconds)
    {
        float dt = Math.Max(0f, deltaSeconds);
        sway += dt;
        reelWeight = Math.Max(0f, Math.Min(1f, reelWeight + (reeling ? dt : -dt) / ToolActionRules.ReelRampSeconds));
        workWeight = Math.Max(0f, Math.Min(1f, workWeight + (working ? dt : -dt) / ToolActionRules.ReelRampSeconds));
        if (!Active)
            return;
        time += dt;
        if (time >= ToolActionRules.Seconds(Action))
        {
            Active = false;
            time = 0f;
        }
    }

    public void Seek(float seconds)
    {
        if (Active)
            time = Math.Max(0f, seconds);
    }

    // The left hand's offset on a bow: the same whatever the draw or release is doing (the bow does not move), zero for every other tool.
    public HandPose BowHandOffset() => ToolActionRules.IsBow(toolId) ? ToolActionRules.BowHandReady() : new HandPose();

    // The right hand's offset from the neutral hold. lit: whether the light in hand is burning (for the lights' sway). On a bow this is the STRING hand's pull (ready, the draw, the release snap).
    public HandPose Offset(bool lit)
    {
        if (!ToolActionRules.Handles(toolId))
            return new HandPose();
        HandPose pose = ToolActionRules.Ready(toolId);
        if (Active)
            pose = ToolActionRules.Sum(pose, ToolActionRules.ActionOffset(Action, time));
        if (ToolActionRules.IsRod(toolId))
        {
            if (Fishing && !Active)
                pose = ToolActionRules.Sum(pose, ToolActionRules.WaitSway(sway));
            if (reelWeight > 0f)
                pose = ToolActionRules.Sum(pose, ToolActionRules.ReelOffset(sway, reelWeight));
        }
        else if (ToolActionRules.IsLight(toolId))
        {
            if (lit)
                pose = ToolActionRules.Sum(pose, ToolActionRules.LightSway(toolId, sway));
        }
        else if (ToolActionRules.IsBow(toolId))
        {
            // The draw builds the string hand's pull-back as the weapon reports it; the release snap (a one-shot) takes over from the full-draw pose.
            if (draw > 0f)
                pose = ToolActionRules.Sum(pose, ToolActionRules.DrawOffset(draw, sway));
        }
        else if (ToolActionRules.IsRifle(toolId))
        {
            if (aim > 0f)
                pose = ToolActionRules.Sum(pose, ToolActionRules.AimOffset(aim));
        }
        else if (ToolActionRules.IsDrill(toolId))
        {
            if (workWeight > 0f)
                pose = ToolActionRules.Sum(pose, ToolActionRules.DrillOffset(sway, workWeight));
        }
        return pose;
    }
}
