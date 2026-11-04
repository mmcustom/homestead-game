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

    // M0c-1: the Fishing Rod and Cane Pole (FishingRod) and the Torch and Lantern (PortableLight). Each carries the tool's item id; each is raised by one line where the thing already happens.
    public static event Action<string> Cast;      // the line went out
    public static event Action<string> Bite;     // a fish took it
    public static event Action<string> Landed;   // a catch
    public static event Action<string> Failed;   // a missed hookset, a fish that let go, or a snapped line
    public static event Action<string> ReelStart; // the line is being wound in
    public static event Action<string> ReelStop;
    public static event Action<string> LineIn;   // the line is back in (a catch, a miss, or a click to reel in): no cast in progress any more
    public static event Action<string> Lit;       // a Torch or Lantern was lit
    public static event Action<string> Snuffed;   // and put out by a click (a burn-out is not an event: the lit state is read from PortableLight)

    static string reelingTool;

    public static void RaiseCast(string toolId) => Cast?.Invoke(toolId);
    public static void RaiseBite(string toolId) => Bite?.Invoke(toolId);
    public static void RaiseLanded(string toolId) => Landed?.Invoke(toolId);
    public static void RaiseFailed(string toolId) => Failed?.Invoke(toolId);
    public static void RaiseLineIn(string toolId) => LineIn?.Invoke(toolId);
    public static void RaiseLit(string toolId) => Lit?.Invoke(toolId);
    public static void RaiseSnuffed(string toolId) => Snuffed?.Invoke(toolId);

    // The reel: called every frame it could be winding, raises ReelStart and ReelStop only when it changes. A null tool id with on = false stops whatever is winding.
    public static void SetReeling(string toolId, bool on)
    {
        if (on && reelingTool == null && !string.IsNullOrEmpty(toolId))
        {
            reelingTool = toolId;
            ReelStart?.Invoke(toolId);
        }
        else if (!on && reelingTool != null)
        {
            string was = reelingTool;
            reelingTool = null;
            ReelStop?.Invoke(was);
        }
    }

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
        reelingTool = null;
    }
}
