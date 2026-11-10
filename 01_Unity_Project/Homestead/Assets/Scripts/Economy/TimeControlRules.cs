using System;

// Player time control (real-calendar Phase 2a): Wait, and the one loop that passes the world's hours for Wait and for sleep. Pure and engine-free, so the stop reasons, the gating and
// the step order are all unit-tested. TimeControlManager reads the real game into a WorldWatch and acts on what this decides.
//
//   Wait lengths   1 hour, 3 hours, 6 hours, until dawn, until dusk.
//   Stop reasons   a stat low, severe weather arriving, the creek rising, a fire dying or out, a trap catching, a notice banner (loan, statement, later bills), danger near.
//   Gating         not in the creek, not in a menu (not Playing), not collapsed or asleep, and not with a stat already low (that would stop it at once).
//   The loop       WorldPass.Run: 0.25-hour steps; each step runs survival, fire, wood seasoning, portable-light fuel, then the clock (the clock fires the hourly and daily events:
//                  weather, traps, taps, loan, market, ledger, autosave). Sleep ran survival, fire and wood and then the clock; Wait and Sleep now share this one loop.

public enum WaitKind { OneHour, ThreeHours, SixHours, UntilDawn, UntilDusk }

public enum StopReason { None, Collapsed, Danger, StatLow, SevereWeather, Flood, FireDying, TrapCatch, Notice }

// What the stop rules look at, read once before a Wait and again after every step.
public struct WorldWatch
{
    public float hydration, hunger, warmth, health; // 0-100
    public float coreC;                             // core temperature, degrees C
    public int severeWeather;                       // 0 = none, otherwise an id for the severe weather type now falling
    public string severeName;                       // "a thunderstorm"
    public int streamState;                         // 0 normal, 1 high water, 2 flash flood
    public int litFiresNear;                        // burning campfires within reach of the player
    public float lowestFireFuelHours;               // the lowest fuel among them (meaningless when none)
    public int trapCatches;                         // traps with a catch waiting
    public int notices;                             // banners raised so far (a counter that only goes up)
    public bool danger;                             // something dangerous is close
    public bool collapsed;
}

public static class TimeControlRules
{
    public const float StepHours = 0.25f;           // the world is passed in steps this long (what sleep always used)
    public const float StatLowBelow = 25f;          // Hydration, Hunger, Warmth or Health under this: the Severe tier
    public const float TooHotCoreC = 38f;           // heat exhaustion starts here (sleep wakes at the same figure)
    public const float FireDyingHours = 0.5f;       // a fire with half an hour of fuel or less is dying
    public const float FireWatchMetres = 30f;       // fires this near count; the fire you left at camp
    public const float MaxWaitHours = 24f;

    public static readonly WaitKind[] Kinds = { WaitKind.OneHour, WaitKind.ThreeHours, WaitKind.SixHours, WaitKind.UntilDawn, WaitKind.UntilDusk };

    public static string Label(WaitKind kind)
    {
        switch (kind)
        {
            case WaitKind.OneHour: return "Wait 1 hour";
            case WaitKind.ThreeHours: return "Wait 3 hours";
            case WaitKind.SixHours: return "Wait 6 hours";
            case WaitKind.UntilDawn: return "Wait until dawn";
            default: return "Wait until dusk";
        }
    }

    // --- How long ---

    // Game hours a Wait of this kind runs, from the time of day now. Dawn and dusk are the next sunrise or sunset: a Wait that starts at that very moment (within a minute)
    // runs to the next one, a day on.
    public static float HoursFor(WaitKind kind, float hourOfDay, float sunriseHour, float sunsetHour)
    {
        switch (kind)
        {
            case WaitKind.OneHour: return 1f;
            case WaitKind.ThreeHours: return 3f;
            case WaitKind.SixHours: return 6f;
            case WaitKind.UntilDawn: return HoursUntil(hourOfDay, sunriseHour);
            default: return HoursUntil(hourOfDay, sunsetHour);
        }
    }

    public static float HoursUntil(float hourOfDay, float targetHour)
    {
        float ahead = Repeat(targetHour - hourOfDay, 24f);
        return ahead < 1f / 60f ? 24f : ahead;
    }

    static float Repeat(float value, float length)
    {
        float r = value - (float)Math.Floor(value / length) * length;
        return r;
    }

    // --- Gating ---

    // Why a Wait cannot start (null if it can). Order: asleep or collapsed, not in plain play (a menu, a pause), in the water, then a stat too low to wait out.
    public static string CantWait(bool playing, bool asleep, bool inWater, WorldWatch w)
    {
        if (asleep || w.collapsed)
            return "You can't wait now.";
        if (!playing)
            return "Not now.";
        if (inWater)
            return "You can't wait in the water.";
        string low = LowStatText(w);
        if (low != null)
            return "You're too " + low + " to wait.";
        if (w.danger)
            return "Something is too close to wait.";
        return null;
    }

    // "thirsty", "hungry", "cold", "hurt" or "hot" for the first stat that is low (null when none is).
    public static string LowStatText(WorldWatch w)
    {
        if (w.hydration < StatLowBelow) return "thirsty";
        if (w.hunger < StatLowBelow) return "hungry";
        if (w.warmth < StatLowBelow) return "cold";
        if (w.coreC > TooHotCoreC) return "hot";
        if (w.health < StatLowBelow) return "hurt";
        return null;
    }

    // --- Stopping ---

    // After a step: whether the Wait should stop, comparing now with how things stood at the start. Changes stop it (a rise, an arrival, a crossing), so a storm, a flood or a dying
    // fire already there when the Wait began does not stop it again at once. A low stat is a level, not a change: it stops the Wait while it is low.
    public static StopReason Evaluate(WorldWatch start, WorldWatch now, out string detail)
    {
        detail = null;
        if (now.collapsed)
            return StopReason.Collapsed;
        if (now.danger && !start.danger)
        {
            detail = "something is close";
            return StopReason.Danger;
        }
        string low = LowStatText(now);
        if (low != null)
        {
            detail = "you're getting " + low;
            if (low == "hot") detail = "you're overheating";
            if (low == "hurt") detail = "you're hurt";
            return StopReason.StatLow;
        }
        if (now.severeWeather != 0 && now.severeWeather != start.severeWeather)
        {
            detail = (now.severeName ?? "severe weather") + " is here";
            return StopReason.SevereWeather;
        }
        if (now.streamState > start.streamState)
        {
            detail = now.streamState >= 2 ? "the creek is in flood" : "the creek is rising";
            return StopReason.Flood;
        }
        if (now.litFiresNear < start.litFiresNear)
        {
            detail = "a fire has gone out";
            return StopReason.FireDying;
        }
        if (now.litFiresNear > 0 && now.lowestFireFuelHours <= FireDyingHours &&
            (start.litFiresNear == 0 || start.lowestFireFuelHours > FireDyingHours))
        {
            detail = "the fire is dying";
            return StopReason.FireDying;
        }
        if (now.trapCatches > start.trapCatches)
        {
            detail = "a trap has caught something";
            return StopReason.TrapCatch;
        }
        if (now.notices > start.notices)
        {
            detail = "there is a notice";
            return StopReason.Notice;
        }
        return StopReason.None;
    }

    // --- Words ---

    public static string HoursText(float hours)
    {
        float rounded = (float)Math.Round(hours * 4f) / 4f; // quarter hours, as the steps are
        if (rounded < 0.001f)
            return "no time";
        if (Math.Abs(rounded - 1f) < 0.001f)
            return "1 hour";
        string number = rounded.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
        return number + " hours";
    }

    public static string DoneMessage(float hours) => "Waited " + HoursText(hours) + ".";

    // One short line for a Wait that ended early: why, and how long it ran.
    public static string StoppedMessage(StopReason reason, string detail, float hoursPassed)
    {
        string why = string.IsNullOrEmpty(detail) ? "something happened" : detail;
        return "Stopped waiting after " + HoursText(hoursPassed) + ": " + why + ".";
    }
}

// What passing the world's hours touches, in the order it must happen. The game implements it (TimeControlManager); the tests implement it with a recorder.
public interface IWorldPass
{
    void Survival(float hours);
    void Fire(float hours);
    void Wood(float hours);
    void Light(float hours);
    void Clock(float hours);
}

public static class WorldPass
{
    // Passes `hours` of game time in steps of TimeControlRules.StepHours (the last one shorter). Each step: the player's survival stats (only when tickPlayer), the fires, the wood,
    // portable-light fuel, then the clock, which fires the hourly and daily events. afterStep(hoursPassedSoFar) is asked after each step and returns true to stop there.
    // Returns the hours passed.
    public static float Run(float hours, IWorldPass world, bool tickPlayer, Func<float, bool> afterStep)
    {
        float passed = 0f;
        while (passed < hours - 0.001f)
        {
            float step = Math.Min(TimeControlRules.StepHours, hours - passed);
            if (tickPlayer)
                world.Survival(step);
            world.Fire(step);
            world.Wood(step);
            world.Light(step);
            world.Clock(step);
            passed += step;
            if (afterStep != null && afterStep(passed))
                break;
        }
        return passed;
    }
}
