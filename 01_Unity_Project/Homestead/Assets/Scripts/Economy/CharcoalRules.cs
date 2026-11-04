using System;

// Metalworking, phase M1 (Mike, 2026-11-04): the Charcoal Pit. An open-air earth mound the player loads with Logs (R), lights (E), walks away from, and comes back to for Charcoal (E).
// It burns for 24 game hours, one hour at a time like the Kiln, so being away, sleeping and skipping days all count. Pure and engine-free so edit-mode tests cover it: what the pit
// holds and the hours left come in as plain numbers, and the texts come out. Every number is a named constant.
//
// How the saved pile holds it (no new field): the Logs loaded and the Charcoal waiting are ordinary stacks in contents. A burn is jobItem (BurnMarker), jobCount (the Charcoal it
// will make, fixed when it is lit) and jobHours (game hours left); the Logs are used up when it is lit. When the hours run out the Charcoal is added and the job fields are cleared.
public static class CharcoalRules
{
    public const string CharcoalId = MetalRules.CharcoalId;
    public const string LogsId = "logs";
    public const string ClayId = "clay";
    public const string TallGrassId = "tall_grass";

    // --- The site: staged like the Kiln, finished with the Hammer ---

    public const int SiteClay = 4;
    public const int SiteTallGrass = 6;
    public const int BuildBlows = 20;

    // --- The burn ---

    public const int LogsCap = 6;          // the most the pit holds
    public const int LogsToLight = 3;      // the fewest it can be lit with
    public const int BurnHours = 24;
    public const string BurnMarker = "charcoal_burn";

    // Charcoal from a load of Logs: two thirds of the Logs, rounded down (3 Logs 2, 4 Logs 2, 5 Logs 3, 6 Logs 4).
    public static int Yield(int logs) => logs < LogsToLight ? 0 : Math.Min(logs, LogsCap) * 2 / 3;

    // --- Where it may stand: open air, like the Kiln ---

    public const float PitRadiusMetres = 0.9f;                 // the mound's reach from its centre (it is about 1.5 m across with its ring of earth)
    public const float ClearanceMetres = PotteryRules.KilnClearanceMetres; // clear air from other structures and trees, and (both ways) from a placed pit: the Kiln's 3 m
    public const float TreeReachMetres = PitRadiusMetres + ClearanceMetres;
    public const float RegrowMetres = 1.4f;                    // no sprout inside the mound's footprint (RegrowRules.Blocker.Pit)

    // --- The smoke over a burning pit (KilnSmoke, smaller than the Kiln's): presentation numbers ---

    public const float SmokeHeight = 0.95f;
    public const float SmokeSpread = 0.07f;
    public const float SmokeSizeMin = 0.28f;
    public const float SmokeSizeMax = 0.45f;
    public const float SmokePuffsPerSecond = 2.2f;
    public const float SmokeRiseSpeed = 0.45f;
    public const int SmokeMaxPuffs = 30;

    // --- Phases ---

    public enum Phase { Empty, Loading, Ready, Burning, Done }

    // logs: the Logs loaded. charcoalWaiting: Charcoal in the pit to collect.
    public static Phase PhaseOf(bool burning, int charcoalWaiting, int logs)
    {
        if (burning)
            return Phase.Burning;
        if (charcoalWaiting > 0)
            return Phase.Done;
        if (logs <= 0)
            return Phase.Empty;
        return logs >= LogsToLight ? Phase.Ready : Phase.Loading;
    }

    public static bool CanLight(Phase phase) => phase == Phase.Ready;

    // The Hammer's R takes the pit down at any time but while it burns (the Logs in the fire are already used).
    public static bool CanDismantle(Phase phase) => phase != Phase.Burning;

    public static int RoomFor(int logsHeld) => Math.Max(0, LogsCap - Math.Max(0, logsHeld));

    public enum Gate { Ok, Burning, Waiting, Full, NothingCarried }

    // Whether R may load now: not while a burn runs, not while Charcoal waits to be collected, and only if there is room and something carried.
    public static Gate LoadGate(Phase phase, int logsHeld, int logsCarried)
    {
        if (phase == Phase.Burning)
            return Gate.Burning;
        if (phase == Phase.Done)
            return Gate.Waiting;
        if (RoomFor(logsHeld) <= 0)
            return Gate.Full;
        return logsCarried > 0 ? Gate.Ok : Gate.NothingCarried;
    }

    // How many of the carried Logs one R press loads: what is carried, up to the room left.
    public static int LoadAmount(int logsHeld, int logsCarried) => Math.Max(0, Math.Min(logsCarried, RoomFor(logsHeld)));

    // --- The words ---

    public static string HoursText(float hoursLeft) => $"{Math.Max(0, (int)Math.Ceiling(hoursLeft - 0.0001f))} h";

    static string LogsText(int n) => n == 1 ? "1 Log" : $"{n} Logs";

    public static string CostText => $"{SiteClay} Clay and {SiteTallGrass} Tall Grass";

    public static string SiteHelp => $"Deliver {CostText}";

    public static string BuiltMessage =>
        $"Charcoal Pit built. Put the Hammer away, press R to load Logs (up to {LogsCap}), then E to light it; it burns for {BurnHours} hours and makes Charcoal.";

    // E's line, by phase. charcoal: the Charcoal waiting or (while burning) the Charcoal the burn will make.
    public static string Prompt(Phase phase, int logs, int charcoal, float hoursLeft)
    {
        switch (phase)
        {
            case Phase.Empty: return $"Charcoal Pit  (empty, holds up to {LogsCap} Logs)";
            case Phase.Loading: return $"Charcoal Pit  ({LogsText(logs)} in, {LogsToLight} needed to light)";
            case Phase.Ready: return $"Light the pit  ({LogsText(logs)} -> {charcoal} Charcoal in {BurnHours} h)";
            case Phase.Burning: return $"Charcoal Pit  (burning, {HoursText(hoursLeft)} to go, {charcoal} Charcoal)";
            default: return $"Collect Charcoal  ({charcoal})";
        }
    }

    // R's line without the Hammer: what it loads, or why it will not.
    public static string LoadPrompt(Gate gate, int amount)
    {
        switch (gate)
        {
            case Gate.Ok: return $"Load {LogsText(amount)}";
            case Gate.Burning: return "Can't load the pit while it burns";
            case Gate.Waiting: return "Collect the Charcoal before loading again";
            case Gate.Full: return $"The pit is full  ({LogsCap} Logs)";
            default: return "Load Logs  (you're not carrying any)";
        }
    }

    public static string LoadRefusal(Gate gate)
    {
        switch (gate)
        {
            case Gate.Burning: return "The pit is burning - nothing can be loaded until it is done.";
            case Gate.Waiting: return "Charcoal is waiting - collect it before loading again.";
            case Gate.Full: return $"The pit already holds {LogsCap} Logs.";
            default: return "You're not carrying any Logs to load.";
        }
    }

    public static string Loaded(int amount, int logsNow) =>
        $"Loaded {LogsText(amount)}  (the pit holds {logsNow} of {LogsCap}; lighting it makes {Yield(logsNow)} Charcoal)";

    // What E says when the pit is not ready to light.
    public static string NotReady(int logs) =>
        logs <= 0 ? $"Load at least {LogsToLight} Logs first (R)." : $"Needs {LogsToLight - logs} more {(LogsToLight - logs == 1 ? "Log" : "Logs")} to light (it holds {logs}).";

    public static string Started(int logs, int charcoal) =>
        $"The pit is lit: {LogsText(logs)} will burn for {BurnHours} hours and make {charcoal} Charcoal.";

    // What E flashes after collecting: the Charcoal taken, and what stayed behind for want of room.
    public static string Took(int took, int stayed)
    {
        if (took <= 0)
            return "Too heavy - no room to carry it";
        string text = $"Took {took} Charcoal";
        return stayed > 0 ? text + $" ({stayed} left in the pit, no room)." : text + ".";
    }

    // R with the Hammer in hand.
    public static string DismantlePrompt(Phase phase, float hoursLeft) =>
        phase == Phase.Burning
            ? $"Can't take down the pit while it burns  ({HoursText(hoursLeft)} to go)"
            : "Take down the pit  (half the Clay and Tall Grass back, rounded down; the Logs and Charcoal in it come back whole)";

    // --- The burn's hours: one game hour at a time, like the Kiln ---

    public static float AfterHour(float hoursLeft) => hoursLeft > 0f ? Math.Max(0f, hoursLeft - 1f) : 0f;

    public static bool IsDone(float hoursLeft) => hoursLeft <= 0f;
}
