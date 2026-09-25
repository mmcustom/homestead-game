using UnityEngine;

// The sight picture a weapon wants: a spread reticle (the bow, and the rifle from the hip), or the rifle's scope while aiming down sights.
public enum ReticleKind { None, Spread, Scope }

// What the shot line is on right now (Hunting_System.md's Shot Placement): nothing, an animal's body, or its vitals.
public enum AimTarget { None, Body, Vitals }

// What the equipped tool wants on the HUD this frame — fishing rod, bow, rifle or a trap being placed — read by
// ToolHud. A tool calls Report every frame it's active; anything not reported this frame or last is cleared, so an
// unequipped tool's line disappears by itself. Flash shows a short message (a catch, a miss) for a few seconds.
public static class ToolStatus
{
    static string line;
    static float progress = -1f;
    static bool crosshair;
    static int reportedFrame = -10;

    static ReticleKind reticle;
    static float spreadDegrees;
    static AimTarget aimTarget;
    static float aimDistance;
    static bool inEffectiveRange;
    static float scopeAmount;
    static int aimFrame = -10;

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

    // A weapon's sight picture this frame. spread: the shot's spread cone in degrees (drawn as the reticle's size).
    // scope: 0-1 how far the rifle's scope is raised.
    public static void ReportAim(ReticleKind kind, float spread, AimTarget target, float distance, bool effective, float scope = 0f)
    {
        reticle = kind;
        spreadDegrees = spread;
        aimTarget = target;
        aimDistance = distance;
        inEffectiveRange = effective;
        scopeAmount = scope;
        aimFrame = Time.frameCount;
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
    static bool AimCurrent => Time.frameCount - aimFrame <= 1;
    public static ReticleKind Reticle => AimCurrent ? reticle : ReticleKind.None;
    public static float SpreadDegrees => spreadDegrees;
    public static AimTarget Target => AimCurrent ? aimTarget : AimTarget.None;
    public static float AimDistance => aimDistance;
    public static bool InEffectiveRange => inEffectiveRange;
    public static float ScopeAmount => AimCurrent ? scopeAmount : 0f;

    public static string FlashMessage => Time.unscaledTime < flashUntil ? flash : null;
}
