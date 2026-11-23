using System;

// The Saw Pit (Saw_Pit_Sawmill_and_Salt_Barrel.md section 1). Mike, 2026-11-23: "a lumber mill or something to turn logs into boards or lumber" -
// the Saw Pit first, a water-powered Sawmill later ("Both, in order").
//
// A staged site: marked out, the trench dug with the Shovel (DigBlows), stocked with Logs and Branches in any order (no Lumber: this is how Lumber is
// made), and pounded up with the Hammer. The finished pit takes Logs with R (up to LogsCap). With the Wood Saw equipped, hold click on it: each pair of
// Logs takes StrokesPerPair strokes and becomes LumberPerPair Lumber, stacked beside the pit until E takes it. The saved pile holds it with no new
// field: Logs and Lumber are stacks in contents and the strokes so far are jobHours. Pure and engine-free so edit-mode tests cover it.
public static class SawPitRules
{
    public const string LogsId = "logs";
    public const string BranchesId = "branches";
    public const string LumberId = "lumber";
    public const string SawId = AxeRules.SawId;

    // --- The site (about 56 kg: 6 Logs at 8 kg, 4 Branches), after the trench ---

    public const int SiteLogs = 6;
    public const int SiteBranches = 4;
    public const int DigBlows = 20;
    public const int BuildBlows = 30;

    // --- Sawing ---

    public const int LogsCap = 6;
    public const int LogsPerPair = 2;
    public const int LumberPerPair = 3;    // 16 kg of Logs make 15 kg of boards
    public const int StrokesPerPair = 20;

    // --- Where it may stand: a 3 m trench (along X) with its sills, the log lying across it; dry ground out of hollows ---

    public const float HalfLength = 1.5f;
    public const float HalfWidth = 0.75f;
    public const float ReachMetres = 1.7f;

    // Logs it still takes, holding logsHeld.
    public static int LogRoom(int logsHeld) => Math.Max(0, LogsCap - Math.Max(0, logsHeld));

    public static bool Accepts(string itemId, int logsHeld) => itemId == LogsId && LogRoom(logsHeld) > 0;

    public static bool CanSaw(int logsHeld) => logsHeld >= LogsPerPair;

    // One stroke of the saw (weight: what the stroke counts for, 1 for a full one). Returns the strokes so far after it and whether a pair was finished.
    public static float AfterStroke(float strokes, float weight, out bool pairDone)
    {
        float next = Math.Max(0f, strokes) + Math.Max(0f, weight);
        pairDone = next >= StrokesPerPair - 0.0001f;
        return pairDone ? 0f : next;
    }

    public static float Progress(float strokes) => Math.Max(0f, Math.Min(1f, strokes / StrokesPerPair));

    // --- The words ---

    public static string CostText => $"{SiteLogs} Logs and {SiteBranches} Branches";

    public static string DugBanner => $"The saw pit is dug - now deliver {CostText} with R, in any order.";

    public static string BuiltMessage =>
        $"Saw Pit built. Put the Hammer away and press R to lay Logs across it (up to {LogsCap}), then hold click with the Wood Saw: every {LogsPerPair} Logs make {LumberPerPair} Lumber.";

    static string LogsText(int n) => n == 1 ? "1 Log" : $"{n} Logs";

    static string LumberText(int n) => n == 1 ? "1 Lumber" : $"{n} Lumber";

    // E's line. logs, lumber: what it holds; sawEquipped: the Wood Saw in hand.
    public static string Prompt(int logs, int lumber, float strokes, bool sawEquipped)
    {
        if (lumber > 0)
            return $"Take the Lumber  ({LumberText(lumber)} stacked; {LogsText(logs)} on the pit)";
        if (logs <= 0)
            return "Saw Pit  (empty - lay Logs across it with R)";
        if (!CanSaw(logs))
            return $"Saw Pit  ({LogsText(logs)} - needs {LogsPerPair} to saw, R)";
        string how = sawEquipped ? "hold click to saw" : "equip the Wood Saw, hold click";
        return strokes > 0f ? $"Sawing  ({Progress(strokes) * 100f:0}% of the next {LumberPerPair} Lumber; {how})"
                            : $"Saw Pit  ({LogsText(logs)} -> {logs / LogsPerPair * LumberPerPair} Lumber; {how})";
    }

    // R's line without the Hammer.
    public static string LoadPrompt(int canLoad, int carried) =>
        carried <= 0 ? "Lay Logs on the pit  (you're not carrying any)"
        : canLoad <= 0 ? $"Can't load  (the pit holds {LogsCap} Logs)"
        : $"Lay {LogsText(canLoad)} on the pit";

    public static string Loaded(int loaded, int logsNow) => $"Laid {LogsText(loaded)} on the pit (it holds {LogsText(logsNow)}).";

    public static string NoSaw => "Equip the Wood Saw to saw Logs into Lumber (the Trading Post sells it).";

    public static string PairDone(int lumberNow) => $"Sawn: {LumberPerPair} Lumber ({LumberText(lumberNow)} stacked beside the pit; E takes it).";

    public static string Took(int took, int stayed)
    {
        if (took <= 0)
            return "Too heavy - no room to carry the Lumber";
        return stayed > 0 ? $"Took {LumberText(took)} ({stayed} left by the pit, no room)." : $"Took {LumberText(took)}.";
    }

    public static string DismantlePrompt => "Take down the saw pit  (half the Logs and Branches back, rounded down; the Logs and Lumber on it come back whole)";
}
