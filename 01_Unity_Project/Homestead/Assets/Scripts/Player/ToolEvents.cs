using System;
using UnityEngine;

// Hands and tools M0b (Mike, 2026-11-05): what a tool tells the hands. The hand animator (PlayerHands) listens; the tool scripts only raise. AxeTool is the first, with the smallest hooks:
// one line where a blow lands (Swing), one where a blow is refused for lack of stamina, one before a blow at a wood pile (so a split is told from a chop), and one at the point each job
// finishes. Nothing here changes what a tool does. M0c adds the other tools' events here.
public static class ToolEvents
{
    // A blow landed: the tool's item id, the work it did, and whether it was the last blow of its job. The finishing blow is raised a second time, the same frame, with the flag set (the job
    // is only known to be done after the blow), so a listener treats a finishing Swung that arrives the frame its swing started as that swing made bigger.
    public static event Action<string, WorkKind, bool> Swung;

    // A blow was refused: too tired to swing.
    public static event Action<string> Tired;

    static string lastTool;
    static WorkKind lastKind;
    static int lastFrame = -1;
    static bool splitHint;
    static int hintFrame = -1;

    // Called just before a blow at a wood pile: the blow of this frame is a split, not a chop.
    public static void WorkingOn(WorkKind kind)
    {
        splitHint = kind == WorkKind.Split;
        hintFrame = Time.frameCount;
    }

    // The one line in AxeTool.Swing: a blow landed with this tool.
    public static void RaiseSwung(string toolId)
    {
        if (string.IsNullOrEmpty(toolId))
            return;
        lastTool = toolId;
        lastKind = ToolWork.KindFor(toolId, splitHint && hintFrame == Time.frameCount);
        lastFrame = Time.frameCount;
        Swung?.Invoke(lastTool, lastKind, false);
    }

    // The job this blow was part of is done: the blow just raised was its last.
    public static void JobFinished()
    {
        if (lastFrame == Time.frameCount && lastTool != null)
            Swung?.Invoke(lastTool, lastKind, true);
    }

    public static void RaiseTired(string toolId)
    {
        if (!string.IsNullOrEmpty(toolId))
            Tired?.Invoke(toolId);
    }

    // Tests: forget what was raised.
    public static void ResetForTests()
    {
        lastTool = null;
        lastFrame = -1;
        splitHint = false;
        hintFrame = -1;
    }
}
