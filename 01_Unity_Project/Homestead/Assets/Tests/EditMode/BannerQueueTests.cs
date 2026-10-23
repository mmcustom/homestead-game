using NUnit.Framework;

// EditMode tests for BannerQueue, the FIFO behind ToolStatus.Banner: a payment-due banner and a statement-ready message on the same day
// both show, one after the other, and neither runs out while it cannot be seen.
public class BannerQueueTests
{
    [Test]
    public void ABannerShowsOnTheFirstTickItCanBeDrawn()
    {
        var q = new BannerQueue();
        q.Add("Payment due", 8f);
        Assert.That(q.Current, Is.Null, "nothing shows until it is ticked");
        q.Tick(true, 0.016f);
        Assert.That(q.Current, Is.EqualTo("Payment due"));
        Assert.That(q.Waiting, Is.EqualTo(0));
    }

    [Test]
    public void TwoBannersOnTheSameDayShowOneAfterTheOther()
    {
        var q = new BannerQueue();
        q.Add("Your land payment is due today.", 8f);
        q.Add("Your account statement is ready.", 8f);

        q.Tick(true, 0.1f);
        Assert.That(q.Current, Is.EqualTo("Your land payment is due today."));
        Assert.That(q.Waiting, Is.EqualTo(1), "the statement waits, it is not lost");

        q.Tick(true, 8f);
        Assert.That(q.Current, Is.EqualTo("Your account statement is ready."), "it takes over the moment the first runs out");
        Assert.That(q.Waiting, Is.EqualTo(0));

        q.Tick(true, 8.1f);
        Assert.That(q.Current, Is.Null);
    }

    [Test]
    public void TheOrderIsFirstInFirstOut()
    {
        var q = new BannerQueue();
        q.Add("a", 1f);
        q.Add("b", 1f);
        q.Add("c", 1f);
        var seen = new System.Collections.Generic.List<string>();
        for (int i = 0; i < 6; i++)
        {
            q.Tick(true, 1.01f);
            if (q.Current != null && (seen.Count == 0 || seen[seen.Count - 1] != q.Current))
                seen.Add(q.Current);
        }
        Assert.That(seen, Is.EqualTo(new[] { "a", "b", "c" }));
    }

    [Test]
    public void TimeDoesNotRunWhileABannerCannotBeSeen()
    {
        var q = new BannerQueue();
        q.Add("Statement ready", 8f);
        q.Tick(true, 1f);
        float left = q.SecondsLeft;

        for (int i = 0; i < 1000; i++)
            q.Tick(false, 1f); // a long sleep or a menu
        Assert.That(q.Current, Is.EqualTo("Statement ready"));
        Assert.That(q.SecondsLeft, Is.EqualTo(left), "the clock did not run");
    }

    [Test]
    public void BannersAddedWhileNothingCanBeDrawnWaitForTheWorldToComeBack()
    {
        var q = new BannerQueue();
        q.Tick(false, 0.016f);
        q.Add("added during the sleep fade", 8f);
        q.Add("another", 8f);
        for (int i = 0; i < 500; i++)
            q.Tick(false, 0.5f);
        Assert.That(q.Current, Is.Null);
        Assert.That(q.Waiting, Is.EqualTo(2), "nothing was lost");

        q.Tick(true, 0.016f);
        Assert.That(q.Current, Is.EqualTo("added during the sleep fade"));
    }

    [Test]
    public void TheSameMessageAgainDoesNotPileUp()
    {
        var q = new BannerQueue();
        q.Add("Due soon", 4f);
        q.Add("Due soon", 4f);
        q.Add("Due soon", 4f);
        Assert.That(q.Waiting, Is.EqualTo(1));

        q.Tick(true, 0.1f);
        q.Add("Due soon", 4f); // asked for again while it is up
        Assert.That(q.Waiting, Is.EqualTo(0), "it keeps the one on show");
    }

    [Test]
    public void AskingAgainKeepsTheLongerTime()
    {
        var q = new BannerQueue();
        q.Add("Warning", 4f);
        q.Tick(true, 3f);          // 1 s left
        q.Add("Warning", 12f);     // asked for again with a longer time
        Assert.That(q.SecondsLeft, Is.EqualTo(12f));

        q.Add("Other", 4f);
        q.Add("Other", 10f);
        q.Tick(true, 12.1f);
        Assert.That(q.Current, Is.EqualTo("Other"));
        Assert.That(q.SecondsLeft, Is.EqualTo(10f), "the waiting one kept the longer time too");
    }

    [Test]
    public void AtMostSixWaitAndTheOldestIsDroppedFirst()
    {
        var q = new BannerQueue();
        for (int i = 1; i <= 9; i++)
            q.Add("m" + i, 1f);
        Assert.That(q.Waiting, Is.EqualTo(BannerQueue.MaxWaiting));

        q.Tick(true, 0.1f);
        Assert.That(q.Current, Is.EqualTo("m4"), "m1 to m3 were dropped");
    }

    [Test]
    public void EmptyMessagesAreIgnored()
    {
        var q = new BannerQueue();
        q.Add(null, 8f);
        q.Add("", 8f);
        q.Tick(true, 0.1f);
        Assert.That(q.Current, Is.Null);
        Assert.That(q.Waiting, Is.EqualTo(0));
    }

    [Test]
    public void ClearEmptiesEverything()
    {
        var q = new BannerQueue();
        q.Add("a", 8f);
        q.Add("b", 8f);
        q.Tick(true, 0.1f);
        q.Clear();
        Assert.That(q.Current, Is.Null);
        Assert.That(q.Waiting, Is.EqualTo(0));
        Assert.That(q.SecondsLeft, Is.EqualTo(0f));
        q.Tick(true, 0.1f);
        Assert.That(q.Current, Is.Null);
    }

    [Test]
    public void ALongBannerDoesNotBlockTheNextOneForever()
    {
        var q = new BannerQueue();
        q.Add("Missed payment warning", 12f);
        q.Add("Statement ready", 8f);
        q.Tick(true, 0.1f);
        for (int i = 0; i < 125; i++)
            q.Tick(true, 0.1f); // a little over 12 seconds of play
        Assert.That(q.Current, Is.EqualTo("Statement ready"));
    }
}
