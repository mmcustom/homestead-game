using UnityEngine;

// What the equipped tool wants on the HUD this frame — fishing rod, bow, rifle or a trap being placed — read by
// ToolHud. A tool calls Report every frame it's active; anything not reported this frame or last is cleared, so an
// unequipped tool's line disappears by itself. Flash shows a short message (a catch, a miss) for a few seconds.
public static class ToolStatus
{
    static string line;
    static float progress = -1f;
    static bool crosshair;
    static int reportedFrame = -10;

    static string flash;
    static float flashUntil;

    // hudLine: e.g. "Arrows: 11" or "Bite! Click to set the hook". progress: 0-1 for a bar, or negative for none.
    public static void Report(string hudLine, float barProgress = -1f, bool showCrosshair = true)
    {
        line = hudLine;
        progress = barProgress;
        crosshair = showCrosshair;
        reportedFrame = Time.frameCount;
    }

    public static void Flash(string message, float seconds = 3f)
    {
        flash = message;
        flashUntil = Time.unscaledTime + seconds;
    }

    static bool Current => Time.frameCount - reportedFrame <= 1;

    public static string Line => Current ? line : null;
    public static float Progress => Current ? progress : -1f;
    public static bool Crosshair => Current && crosshair;
    public static string FlashMessage => Time.unscaledTime < flashUntil ? flash : null;
}
