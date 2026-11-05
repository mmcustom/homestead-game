using System;

// Hands and tools M0b (Mike, 2026-11-05): the swing poses of the chopping, digging, mining and building tools. Pure and engine-free like HandPoseRules (positions in metres in the camera's space,
// angles in degrees), so edit-mode tests cover every pose. The pose follows the existing swing (AxeTool.swingSeconds, 0.9 s) and adds no game time: the blow, its sound, its count and its stamina
// cost happen at the instant of the click; the picture shows the strike landing ContactSeconds later, then the follow-through and recovery, so a held click looks like repeated work.
//
// The pull-back (Mike's playtest, 2026-11-05: visual only, no delay). The Axe, Primitive Axe, Hammer and both Pick Axes are held COCKED: the tool drawn back and up behind the hand, so the first
// blow after a pause strikes from a pulled-back hand. The strike is a fast arc from there to the contact pose (still ContactSeconds after the click). The recovery comes back to a LOADED pose,
// a little farther back than cocked, and reaches it exactly at the end of the 0.9 s cycle, so a held click shows a pull-back before every blow. With no new blow the hands settle from loaded
// to cocked over SettleSeconds. The Saw and the Shovels are not cocked: their ready pose is the plain neutral hold, as in the first M0b.

// The kind of work a blow does. Chop and Split are the same overhead arc; they are told apart only so a hand animator that wants a different split pose can have one.
public enum WorkKind { Chop, Split, Saw, Dig, Mine, Build }

// Which tools M0b draws in hand, and what work each does. One table: a tool not listed keeps the generic M0a placeholder.
public static class ToolWork
{
    public static readonly string[] ViewToolIds =
    {
        "axe", "primitive_axe", "wood_saw", "hammer",
        "shovel", "iron_shovel", "stone_pick_axe", "iron_pick_axe",
        "fishing_rod", "cane_pole", "torch", "lantern",
        "primitive_bow", "recurve_bow", "bolt_action_rifle", "bow_drill", "bucket", "sap_bucket", "canteen", "spile",
        "box_trap", "fish_trap", "rabbit_snare", "knife", "flint_and_steel", "pouch", "clay_pot", "cooking_pot", "potash_kettle", "tarp",
    };

    public static bool IsViewTool(string toolId) => toolId != null && Array.IndexOf(ViewToolIds, toolId) >= 0;

    // The work a tool does. splitting: the blow is at a wood pile (the axes split it; the saw still saws).
    public static WorkKind KindFor(string toolId, bool splitting)
    {
        switch (toolId)
        {
            case "wood_saw": return WorkKind.Saw;
            case "hammer": return WorkKind.Build;
            case "shovel":
            case "iron_shovel": return WorkKind.Dig;
            case "stone_pick_axe":
            case "iron_pick_axe": return WorkKind.Mine;
            default: return splitting ? WorkKind.Split : WorkKind.Chop;
        }
    }

    // The five tools held cocked (drawn back before the blow) and the kind of cock each takes. The Saw, the Shovels and every other tool are not: false.
    public static bool CockKind(string toolId, out WorkKind kind)
    {
        switch (toolId)
        {
            case "axe":
            case "primitive_axe": kind = WorkKind.Chop; return true;
            case "stone_pick_axe":
            case "iron_pick_axe": kind = WorkKind.Mine; return true;
            case "hammer": kind = WorkKind.Build; return true;
            default: kind = WorkKind.Chop; return false;
        }
    }
}

public static class SwingRules
{
    // The existing swing interval (AxeTool.swingSeconds): one blow, the next at most this long after. A test pins it against the component's default.
    public const float SwingSeconds = 0.9f;

    // THE contact timing: seconds from the click (the sound and the count) to the strike landing in the picture. Unchanged by the pull-back.
    public const float ContactSeconds = 0.15f;

    // How long the tool stays at the strike before it comes back.
    public const float DwellSeconds = 0.05f;

    // The finishing blow of a job swings this much bigger.
    public const float FinishingScale = 1.35f;

    // "Too tired to swing" retries every 0.5 s while the button is held (AxeTool); the droop takes the same time.
    public const float TiredSeconds = 0.5f;

    // The pivot of a swing: the elbow, this far behind the wrist along the forearm (rig units). The hand and tool arc about it, so the forearm and sleeve stay where they are instead of swinging up into view.
    public const float ElbowBack = 0.20f;

    // How much of a pitch offset turns the hand (and the forearm and sleeve with it); the rest tilts the tool alone about its grip. Turning the whole arm swung the sleeve up across the view.
    public const float HandPitchShare = 0.35f;

    // With no new blow the hands settle from the loaded pose to the cocked ready pose over this long.
    public const float SettleSeconds = 0.25f;

    // The cocked ready pose and the loaded pose, as offsets from the neutral hold: how far the tool is tilted up (pitch, degrees; negative = the head goes back and up) and how far the hand is
    // pulled back toward the player (z, rig metres). Tuned against screenshots. Loaded is farther back than cocked; the Axes and Pick Axes are drawn back farther than the Hammer.
    public const float ChopCockPitch = -14f, ChopCockBack = -0.04f, ChopLoadPitch = -24f, ChopLoadBack = -0.07f;
    public const float MineCockPitch = -12f, MineCockBack = -0.03f, MineLoadPitch = -21f, MineLoadBack = -0.06f;
    public const float BuildCockPitch = -10f, BuildCockBack = -0.02f, BuildLoadPitch = -16f, BuildLoadBack = -0.04f;

    // The hand also rides a little higher when cocked and higher still when loaded, and turns a few degrees away from the middle of the screen so the head stays off the crosshair.
    public const float CockUp = 0.015f, LoadUp = 0.03f, CockYaw = 4f, LoadYaw = 6f;

    static HandPose P(float x, float y, float z, float pitch, float yaw = 0f, float roll = 0f) =>
        new HandPose { X = x, Y = y, Z = z, Pitch = pitch, Yaw = yaw, Roll = roll };

    // A split is the same arc as a chop.
    static WorkKind Base(WorkKind kind) => kind == WorkKind.Split ? WorkKind.Chop : kind;

    // The cocked ready pose of a kind of work, relative to the neutral hold. Zero for the Saw and the Dig (the Shovels): they are not cocked.
    public static HandPose Cocked(WorkKind kind)
    {
        switch (Base(kind))
        {
            case WorkKind.Chop: return P(0f, CockUp, ChopCockBack, ChopCockPitch, CockYaw);
            case WorkKind.Mine: return P(0f, CockUp, MineCockBack, MineCockPitch, CockYaw);
            case WorkKind.Build: return P(0f, CockUp, BuildCockBack, BuildCockPitch, CockYaw);
            default: return Zero;
        }
    }

    // The loaded pose a swing recovers to: the pre-load, farther back than cocked.
    public static HandPose Loaded(WorkKind kind)
    {
        switch (Base(kind))
        {
            case WorkKind.Chop: return P(0f, LoadUp, ChopLoadBack, ChopLoadPitch, LoadYaw);
            case WorkKind.Mine: return P(0f, LoadUp, MineLoadBack, MineLoadPitch, LoadYaw);
            case WorkKind.Build: return P(0f, LoadUp, BuildLoadBack, BuildLoadPitch, LoadYaw);
            default: return Zero;
        }
    }

    // Where the hand is, relative to the neutral hold, at the strike.
    public static HandPose Strike(WorkKind kind)
    {
        switch (kind)
        {
            case WorkKind.Saw: return P(0f, 0f, 0.11f, 2f);                                  // a push along the cut
            case WorkKind.Mine: return P(0f, -0.01f, 0.14f, 18f);                             // like a chop, shorter and more forward
            case WorkKind.Dig: return P(-0.02f, -0.03f, 0.08f, 16f, 0f, 4f);                  // a short downward push with a lean
            case WorkKind.Build: return P(0f, -0.01f, 0.08f, 13f);                          // a short forward tap
            default: return P(0f, -0.02f, 0.12f, 24f);                                      // chop and split: the overhead arc, down
        }
    }

    // A lift on the way back, for the tools that are not cocked (the Saw's pull, the Shovel's small lift). The cocked tools recover straight to loaded, so theirs is zero.
    public static HandPose Lift(WorkKind kind)
    {
        switch (kind)
        {
            case WorkKind.Saw: return P(0f, 0f, -0.07f, -2f);                                // the pull back
            case WorkKind.Dig: return P(0f, 0.02f, -0.01f, -6f);
            default: return Zero;
        }
    }

    public static HandPose Zero => new HandPose();

    static HandPose Scale(HandPose a, float s) => new HandPose { X = a.X * s, Y = a.Y * s, Z = a.Z * s, Pitch = a.Pitch * s, Yaw = a.Yaw * s, Roll = a.Roll * s };

    static HandPose Add(HandPose a, HandPose b) => new HandPose { X = a.X + b.X, Y = a.Y + b.Y, Z = a.Z + b.Z, Pitch = a.Pitch + b.Pitch, Yaw = a.Yaw + b.Yaw, Roll = a.Roll + b.Roll };

    public static HandPose Sum(HandPose a, HandPose b) => Add(a, b);

    public static HandPose Lerp(HandPose a, HandPose b, float s) => Add(Scale(a, 1f - s), Scale(b, s));

    // How far apart two poses are (position in metres plus pitch in degrees over 100), for tests.
    public static float Distance(HandPose a, HandPose b) => Magnitude(Add(b, Scale(a, -1f)));

    // The hand's offset from the neutral hold, t seconds after the blow, for a blow that starts from the pose from (the cocked pose after a pause, the loaded pose in a held click) and recovers
    // to the pose loaded. At t = 0 the hand has not moved (no delay: from), it reaches the strike at ContactSeconds, holds it through the dwell, then recovers (with the lift, if any) to loaded
    // exactly at SwingSeconds, so the next blow of a held click starts from loaded.
    public static HandPose Offset(WorkKind kind, float t, bool finishing, HandPose from, HandPose loaded)
    {
        if (t <= 0f)
            return from;
        if (t >= SwingSeconds)
            return loaded;

        float scale = finishing ? FinishingScale : 1f;
        HandPose strike = Scale(Strike(kind), scale);
        HandPose lift = Scale(Lift(kind), scale);

        if (t < ContactSeconds)
        {
            float u = t / ContactSeconds;
            return Lerp(from, strike, u * u); // fast in: slow start, hard arrival
        }
        if (t < ContactSeconds + DwellSeconds)
            return strike;

        float r = (t - ContactSeconds - DwellSeconds) / (SwingSeconds - ContactSeconds - DwellSeconds);
        float bump = (float)Math.Sin(Math.PI * r);
        return Add(Lerp(strike, loaded, HandPoseRules.Ease(r)), Scale(lift, bump));
    }

    // The same for a blow after a pause: from the kind's cocked pose to its loaded pose (zero and zero for the tools that are not cocked).
    public static HandPose Offset(WorkKind kind, float t, bool finishing) => Offset(kind, t, finishing, Cocked(kind), Loaded(kind));

    // "Too tired to swing": a half raise that droops, no strike. Zero at both ends (it is added to the tool's ready pose).
    public static HandPose TiredOffset(float t)
    {
        if (t <= 0f || t >= TiredSeconds)
            return Zero;
        HandPose up = P(0f, 0.03f, -0.01f, -7f);
        HandPose droop = P(0f, -0.03f, 0.01f, 9f);
        float u = t / TiredSeconds;
        if (u < 0.35f)
            return Scale(up, HandPoseRules.Ease(u / 0.35f));
        if (u < 0.75f)
        {
            float e = HandPoseRules.Ease((u - 0.35f) / 0.40f);
            return Add(Scale(up, 1f - e), Scale(droop, e));
        }
        return Scale(droop, 1f - HandPoseRules.Ease((u - 0.75f) / 0.25f));
    }

    // The size of an offset: how far the hand has moved from the neutral hold, for tests (position in metres plus pitch in degrees over 100).
    public static float Magnitude(HandPose p) => (float)Math.Sqrt(p.X * p.X + p.Y * p.Y + p.Z * p.Z) + Math.Abs(p.Pitch) / 100f;
}

// The right hand's offset from the neutral hold, over time: the ready pose of the tool in hand (cocked, or neutral), the swing in progress, and the settle after it. PlayerHands starts it from
// ToolEvents and ticks it every frame. A new blow replaces the one in progress and starts from wherever the hand is (a held click starts every blow from the loaded pose), and the finishing flag
// of a blow that arrives the same frame as its swing makes that swing the bigger finishing one. Pure, so a test can step it with fixed time.
public sealed class SwingState
{
    float time;
    float settleTime = SwingRules.SettleSeconds; // settled: not in a settle
    bool cocked;
    WorkKind restKind;
    HandPose from;

    public bool Active { get; private set; }
    public bool Tired { get; private set; }
    public bool Finishing { get; private set; }
    public WorkKind Kind { get; private set; }
    public float Time => time;
    public bool Cocked => cocked;

    // The tool in hand: whether it is held cocked, and the kind of cock. Called when the tool changes, so it also drops any swing in progress.
    public void Configure(bool isCocked, WorkKind kind)
    {
        cocked = isCocked;
        restKind = kind;
        Stop();
    }

    // The ready pose: cocked for the five tools, neutral (zero) for the rest.
    public HandPose Rest => cocked ? SwingRules.Cocked(restKind) : SwingRules.Zero;

    // Where a swing ends: the loaded pose (zero when the tool is not cocked).
    public HandPose LoadedPose => cocked ? SwingRules.Loaded(restKind) : SwingRules.Zero;

    public void Start(WorkKind kind, bool finishing)
    {
        from = Offset; // wherever the hand is now: cocked after a pause, loaded in a held click, or partway through a settle
        Kind = kind;
        Finishing = finishing;
        Tired = false;
        Active = true;
        time = 0f;
    }

    public void StartTired()
    {
        Tired = true;
        Finishing = false;
        Active = true;
        time = 0f;
    }

    // The blow that just landed was the last of its job: the same swing, made bigger. Only counts in the swing's first moments.
    public void MakeFinishing()
    {
        if (Active && !Tired && time < 0.05f)
            Finishing = true;
    }

    public void Tick(float deltaSeconds)
    {
        float dt = Math.Max(0f, deltaSeconds);
        if (!Active)
        {
            settleTime = Math.Min(SwingRules.SettleSeconds, settleTime + dt);
            return;
        }
        time += dt;
        if (time >= (Tired ? SwingRules.TiredSeconds : SwingRules.SwingSeconds))
        {
            // A swing ends at the loaded pose and settles to cocked from there; a tired droop ends at cocked.
            settleTime = Tired ? SwingRules.SettleSeconds : 0f;
            Active = false;
            time = 0f;
        }
    }

    // Jumps to this moment of the swing in progress (tests and screenshots look at a pose without waiting for it).
    public void Seek(float seconds)
    {
        if (Active)
            time = Math.Max(0f, seconds);
    }

    public void Stop()
    {
        Active = false;
        time = 0f;
        settleTime = SwingRules.SettleSeconds;
    }

    public HandPose Offset
    {
        get
        {
            if (Active)
                return Tired ? SwingRules.Sum(Rest, SwingRules.TiredOffset(time))
                             : SwingRules.Offset(Kind, time, Finishing, from, LoadedPose);
            if (settleTime < SwingRules.SettleSeconds)
                return SwingRules.Lerp(LoadedPose, Rest, HandPoseRules.Ease(settleTime / SwingRules.SettleSeconds));
            return Rest;
        }
    }
}
