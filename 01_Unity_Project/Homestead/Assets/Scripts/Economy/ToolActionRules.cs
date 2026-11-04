using System;

// Hands and tools M0c-1 (Mike, 2026-11-05): the poses of the Fishing Rod, the Cane Pole, the Torch and the Lantern in hand. Pure and engine-free like SwingRules and HandPoseRules (offsets from the
// neutral hold: metres in the rig's space and degrees), so edit-mode tests cover every pose. Nothing here adds game time or a delay: casting, a bite, a catch, lighting and snuffing all happen where
// the fishing and light scripts already do them, and the picture follows (ToolEvents). A hand animator that wants more than this listens to the same events.
public enum ToolAction { Cast, Bite, Catch, Miss, Light, Snuff }

public static class ToolActionRules
{
    public const string RodId = "fishing_rod", CaneId = "cane_pole", TorchId = "torch", LanternId = "lantern";

    // How long each one-shot takes. The cast is a back-then-forward flick; the bite a quick dip of the tip; the catch a short pull up and back; the miss (a missed hookset or a snapped line) a jerk.
    public const float CastSeconds = 0.6f, BiteSeconds = 0.35f, CatchSeconds = 0.6f, MissSeconds = 0.45f, LightSeconds = 0.45f, SnuffSeconds = 0.4f;

    // The crank: turns a second while the line is being wound in, and how fast it starts and stops.
    public const float ReelTurnsPerSecond = 2.2f, ReelRampSeconds = 0.15f;

    public static bool IsRod(string toolId) => toolId == RodId || toolId == CaneId;
    public static bool IsLight(string toolId) => toolId == TorchId || toolId == LanternId;
    public static bool Handles(string toolId) => IsRod(toolId) || IsLight(toolId);

    static HandPose P(float x, float y, float z, float pitch, float yaw = 0f, float roll = 0f) =>
        new HandPose { X = x, Y = y, Z = z, Pitch = pitch, Yaw = yaw, Roll = roll };

    static readonly HandPose Zero = new HandPose();

    // The ready pose, relative to the neutral hold: a rod held out and up in front with the tip drooping a little; a torch held up and forward; a lantern hanging low.
    public static HandPose Ready(string toolId)
    {
        switch (toolId)
        {
            case RodId: return P(0f, 0.015f, 0.01f, -6f);
            case CaneId: return P(0f, 0.015f, 0.01f, -4f);
            case TorchId: return P(0f, 0.02f, 0.02f, -8f);
            case LanternId: return P(0f, 0.10f, 0.03f, -6f);
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

    public void Tick(float deltaSeconds)
    {
        float dt = Math.Max(0f, deltaSeconds);
        sway += dt;
        reelWeight = Math.Max(0f, Math.Min(1f, reelWeight + (reeling ? dt : -dt) / ToolActionRules.ReelRampSeconds));
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

    // The right hand's offset from the neutral hold. lit: whether the light in hand is burning (for the lights' sway).
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
        else if (lit)
        {
            pose = ToolActionRules.Sum(pose, ToolActionRules.LightSway(toolId, sway));
        }
        return pose;
    }
}
