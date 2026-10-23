using System;
using System.Collections.Generic;

// ToolStatus's big banner messages as a small FIFO queue, pure and engine-free so edit-mode tests cover it. A payment due and an account
// statement ready can land on the same day (a land payment falls due on every season start), and both must show: one after the other,
// never one over the other. The message on show counts down only while a banner can be seen (the world is live: not a menu, not asleep),
// so none runs out unseen. The same message asked for again keeps the one already up or waiting instead of piling up, and at most
// MaxWaiting wait (the oldest is dropped first).
public sealed class BannerQueue
{
    public const int MaxWaiting = 6;

    readonly List<(string text, float seconds)> waiting = new List<(string, float)>();

    public string Current { get; private set; }
    public float SecondsLeft { get; private set; }
    public int Waiting => waiting.Count;

    public void Add(string message, float seconds)
    {
        if (string.IsNullOrEmpty(message))
            return;
        if (Current == message)
        {
            SecondsLeft = Math.Max(SecondsLeft, seconds);
            return;
        }
        for (int i = 0; i < waiting.Count; i++)
        {
            if (waiting[i].text == message)
            {
                waiting[i] = (message, Math.Max(waiting[i].seconds, seconds));
                return;
            }
        }
        if (waiting.Count >= MaxWaiting)
            waiting.RemoveAt(0);
        waiting.Add((message, seconds));
    }

    // Every frame: canDraw is whether a banner can be seen right now. The one on show loses time only then; when it runs out the next
    // in line takes its place (in the same tick, so there is no empty frame between two banners).
    public void Tick(bool canDraw, float deltaSeconds)
    {
        if (!canDraw)
            return;
        if (Current != null)
        {
            SecondsLeft -= deltaSeconds;
            if (SecondsLeft <= 0f)
                Current = null;
        }
        if (Current == null && waiting.Count > 0)
        {
            Current = waiting[0].text;
            SecondsLeft = waiting[0].seconds;
            waiting.RemoveAt(0);
        }
    }

    public void Clear()
    {
        Current = null;
        SecondsLeft = 0f;
        waiting.Clear();
    }
}
