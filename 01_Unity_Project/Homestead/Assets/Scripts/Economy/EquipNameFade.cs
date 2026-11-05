using System;

// Hands and tools M0d (Mike, 2026-11-05): the equipped-tool line at the crosshair is no longer permanent, now that the hands draw every tool. It shows for a short while when a tool is picked up, then
// fades away. Pure and engine-free like SwingRules and ToolActionRules, so edit-mode tests cover the timing; ToolHud feeds it each frame and draws the result.
//
// What shows is exactly what the idle line used to say (the tool's name, its short hint, a count such as "Arrows: 10"): the tool scripts still build those strings, they just report them as IDLE
// lines (ToolStatus.ReportIdle) and this decides whether they are on screen. A tool that never had an idle line shows its display name instead. A prompt that belongs to what you are doing or aiming
// at (a target, a draw, a cast, a fire) is not an idle line: it is never touched and, while it is up, nothing from here shows over it.
public sealed class EquipNameFade
{
    // About 2.5 seconds in all: a quick fade in, a hold, and a slower fade out.
    public const float FadeInSeconds = 0.25f, HoldSeconds = 1.75f, FadeOutSeconds = 0.5f;
    public const float TotalSeconds = FadeInSeconds + HoldSeconds + FadeOutSeconds;

    string toolKey;
    int countKey = -1;
    float age = TotalSeconds;

    public string ToolKey => toolKey;
    public float Age => age;

    // 0 = nothing on screen, 1 = full. Nothing at rest, and nothing at all with empty hands.
    public float Alpha
    {
        get
        {
            if (toolKey == null || age >= TotalSeconds)
                return 0f;
            if (age < FadeInSeconds)
                return age / FadeInSeconds;
            if (age < FadeInSeconds + HoldSeconds)
                return 1f;
            return 1f - (age - FadeInSeconds - HoldSeconds) / FadeOutSeconds;
        }
    }

    // Once a frame while the world is live. toolKey: the equipped tool's id, or null for empty hands. countKey: a count the tool shows (arrows, rounds, traps in the pack), or negative when it has none
    // or is not reporting one this frame (a negative count never restarts the fade). A new tool restarts it, and so does a changed count on the same tool.
    public void Tick(float deltaSeconds, string toolKey, int countKey)
    {
        bool switched = toolKey != this.toolKey;
        bool counted = !switched && countKey >= 0 && this.countKey >= 0 && countKey != this.countKey;
        if (switched || counted)
            age = 0f;
        this.toolKey = toolKey;
        if (countKey >= 0)
            this.countKey = countKey;
        else if (switched)
            this.countKey = -1;
        age = Math.Min(TotalSeconds, age + Math.Max(0f, deltaSeconds));
    }

    // What to put on screen: the idle line when the tool is only being held; its display name when it has no idle line and nothing is aimed at; nothing while a prompt or a status is showing (the
    // prompt owns the place) or while the fade is over. line: the text the tools reported this frame (null when none did); lineIsIdle: whether that was reported as an idle line.
    public string Pick(string line, bool lineIsIdle, string displayName)
    {
        if (Alpha <= 0f)
            return null;
        if (line == null)
            return string.IsNullOrEmpty(displayName) ? null : displayName;
        return lineIsIdle ? line : null;
    }
}
