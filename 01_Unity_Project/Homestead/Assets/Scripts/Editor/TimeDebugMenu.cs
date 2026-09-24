using UnityEditor;
using UnityEngine;

// Testing only: the same calendar skips as TimeDebugControls' hotkeys, from the Homestead menu in Play Mode.
public static class TimeDebugMenu
{
    const string Root = "Homestead/Debug Time/";

    [MenuItem(Root + "Skip 1 Day")]
    static void SkipDay() => Skip(time => time.DebugSkipDays(1));

    [MenuItem(Root + "Skip 7 Days")]
    static void SkipWeek() => Skip(time => time.DebugSkipDays(7));

    [MenuItem(Root + "Skip to Next Season")]
    static void SkipSeason() => Skip(time => time.DebugSkipToSeason((Season)(((int)time.CurrentSeason + 1) % 4)));

    [MenuItem(Root + "Skip to Spring")]
    static void SkipSpring() => Skip(time => time.DebugSkipToSeason(Season.Spring));

    [MenuItem(Root + "Skip to Summer")]
    static void SkipSummer() => Skip(time => time.DebugSkipToSeason(Season.Summer));

    [MenuItem(Root + "Skip to Fall")]
    static void SkipFall() => Skip(time => time.DebugSkipToSeason(Season.Fall));

    [MenuItem(Root + "Skip to Winter")]
    static void SkipWinter() => Skip(time => time.DebugSkipToSeason(Season.Winter));

    [MenuItem(Root + "Skip 1 Day", true)]
    [MenuItem(Root + "Skip 7 Days", true)]
    [MenuItem(Root + "Skip to Next Season", true)]
    [MenuItem(Root + "Skip to Spring", true)]
    [MenuItem(Root + "Skip to Summer", true)]
    [MenuItem(Root + "Skip to Fall", true)]
    [MenuItem(Root + "Skip to Winter", true)]
    static bool CanSkip() => Application.isPlaying && TimeManager.Instance != null;

    static void Skip(System.Action<TimeManager> skip)
    {
        TimeManager time = TimeManager.Instance;
        skip(time);
        Debug.Log($"[TimeDebug] Now {time.FormatDate(time.TotalDays)} {time.Hour:00}:{time.Minute:00}");
    }
}
