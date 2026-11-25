using System;

// The water-powered Sawmill (Saw_Pit_Sawmill_and_Salt_Barrel.md section 2). Mike, 2026-11-24: on the creek bank, the big build with Lumber and Iron, 2 Logs an hour
// holding 20, and it stops during a flash flood (no damage).
//
// A staged site like the Workshop: marked out free on the creek bank, stocked with Logs, Lumber, Iron Bars, Cordage and Stone in any order, and hammered up. The finished
// mill takes Logs with R (up to LogsCap). Each game hour, while it holds at least LogsPerHour Logs and the creek is not in flash flood, it saws LogsPerHour Logs into
// LumberPerHour Lumber, which waits in the mill until E takes it; being away or asleep counts, as for the Workshop's bench. The saved pile holds it with no new field: Logs
// and Lumber are stacks in its contents. Pure and engine-free so edit-mode tests cover it.
public static class SawmillRules
{
    public const string LogsId = "logs";
    public const string LumberId = "lumber";
    public const string IronBarId = MetalRules.IronBarId;
    public const string CordageId = "cordage";
    public const string StoneId = "stone";

    // --- The site: about 254 kg (12 Logs at 8 kg, 10 Lumber at 5 kg, 2 Iron Bars at 2 kg, 4 Cordage, 6 Stone at 5 kg), then the Hammer ---

    public const int SiteLogs = 12;
    public const int SiteLumber = 10;
    public const int SiteIronBars = 2;   // the saw blade and the crank
    public const int SiteCordage = 4;
    public const int SiteStone = 6;      // the wheel's footings
    public const int BuildBlows = 60;

    // --- Sawing (Mike: 2 Logs an hour, holding 20) ---

    public const int LogsCap = 20;
    public const int LogsPerHour = 2;
    public const int LumberPerHour = 3;  // every 2 Logs make 3 Lumber, as at the Saw Pit

    // --- Where it may stand: a 4 m by 3 m mill on the creek bank, its wheel in the water ---

    public const float HalfWidth = 2.0f;
    public const float HalfDepth = 1.5f;
    public const float ReachMetres = 2.5f;        // its half-diagonal, for spacing and the trees on its footprint
    public const float MinBankMetres = 1.0f;      // the mill's middle at least this far from the water's edge: not in the channel
    public const float CreekReachMetres = 4.0f;   // and no further than this from it, so the wheel reaches the water

    public enum CreekFit { Ok, InTheCreek, TooFar }

    // edgeMetres: from the mill's middle to the nearest edge of the creek's water (negative inside the channel).
    public static CreekFit FitToCreek(float edgeMetres) =>
        edgeMetres < MinBankMetres ? CreekFit.InTheCreek : edgeMetres > CreekReachMetres ? CreekFit.TooFar : CreekFit.Ok;

    public static string CreekReason(CreekFit fit, float edgeMetres)
    {
        switch (fit)
        {
            case CreekFit.InTheCreek: return "Too close: the mill must stand on the bank, out of the creek itself.";
            case CreekFit.TooFar: return $"Too far from the creek ({edgeMetres:0} m): the water wheel needs the creek within {CreekReachMetres:0} m.";
            default: return "";
        }
    }

    // --- The hour ---

    public enum Status { WaitingForLogs, Running, Flooded }

    public static Status StatusOf(int logs, bool flashFlood) =>
        flashFlood ? Status.Flooded : logs >= LogsPerHour ? Status.Running : Status.WaitingForLogs;

    // One game hour: how many Logs it saws and how much Lumber that makes.
    public static void AfterHour(int logs, bool flashFlood, out int logsUsed, out int lumberMade)
    {
        bool runs = StatusOf(logs, flashFlood) == Status.Running;
        logsUsed = runs ? LogsPerHour : 0;
        lumberMade = runs ? LumberPerHour : 0;
    }

    // Hours to saw what it holds now.
    public static int HoursLeft(int logs) => Math.Max(0, logs) / LogsPerHour;

    // Logs it still takes, holding logsHeld.
    public static int LogRoom(int logsHeld) => Math.Max(0, LogsCap - Math.Max(0, logsHeld));

    public static bool Accepts(string itemId, int logsHeld) => itemId == LogsId && LogRoom(logsHeld) > 0;

    // --- The words ---

    public static string CostText => $"{SiteLogs} Logs, {SiteLumber} Lumber, {SiteIronBars} Iron Bars, {SiteCordage} Cordage and {SiteStone} Stone";

    public static string BuiltMessage =>
        $"Sawmill built. Put the Hammer away and press R to load Logs (up to {LogsCap}): the water wheel saws {LogsPerHour} Logs into {LumberPerHour} Lumber every hour, even while you are away.";

    static string LogsText(int n) => n == 1 ? "1 Log" : $"{n} Logs";

    static string LumberText(int n) => n == 1 ? "1 Lumber" : $"{n} Lumber";

    public static string StatusText(Status status, int logs)
    {
        switch (status)
        {
            case Status.Flooded: return "stopped: flash flood";
            case Status.Running: return $"sawing, about {HoursLeft(logs)} h of Logs";
            default: return logs > 0 ? $"needs {LogsPerHour} Logs to saw" : "waiting for Logs";
        }
    }

    // E's line. logs, lumber: what it holds.
    public static string Prompt(int logs, int lumber, bool flashFlood)
    {
        string status = StatusText(StatusOf(logs, flashFlood), logs);
        if (lumber > 0)
            return $"Take the Lumber  ({LumberText(lumber)} waiting; {LogsText(logs)}, {status})";
        return logs <= 0 ? "Sawmill  (empty - load Logs with R)" : $"Sawmill  ({LogsText(logs)}, {status})";
    }

    // R's line without the Hammer.
    public static string LoadPrompt(int canLoad, int carried) =>
        carried <= 0 ? "Load Logs into the mill  (you're not carrying any)"
        : canLoad <= 0 ? $"Can't load  (the mill holds {LogsCap} Logs)"
        : $"Load {LogsText(canLoad)} into the mill";

    public static string Loaded(int loaded, int logsNow) =>
        $"Loaded {LogsText(loaded)} (the mill holds {LogsText(logsNow)}: about {HoursLeft(logsNow)} h of sawing).";

    public static string NothingToTake(int logs, bool flashFlood) =>
        logs <= 0 ? "No Lumber yet: load Logs with R and the wheel saws them, 2 an hour." : $"No Lumber yet ({StatusText(StatusOf(logs, flashFlood), logs)}).";

    public static string Took(int took, int stayed)
    {
        if (took <= 0)
            return "Too heavy - no room to carry the Lumber";
        return stayed > 0 ? $"Took {LumberText(took)} ({stayed} left in the mill, no room)." : $"Took {LumberText(took)}.";
    }

    // The banner when a load runs out.
    public static string LoadDone(int lumberWaiting) => $"The sawmill has sawn its last Logs: {LumberText(lumberWaiting)} waiting.";

    public static string DismantlePrompt => "Take down the sawmill  (half its materials back, rounded down; the Logs and Lumber in it come back whole)";

    public const string SiteHelp =
        "a water-powered mill on the creek bank, built in stages. Mark it out (free) within 4 m of the creek's water, out of the creek itself; the water wheel goes on the creek side. " +
        "Deliver 12 Logs, 10 Lumber, 2 Iron Bars, 4 Cordage and 6 Stone with R in any order, then hammer it up (60 blows).";

    public const string Help =
        "load Logs with R (up to 20): the water wheel saws 2 Logs into 3 Lumber every game hour, even while you are away or asleep, and E takes the Lumber. " +
        "It stops during a flash flood and starts again when the creek goes down. The Hammer takes it down for half its materials.";
}
