using NUnit.Framework;

// EditMode tests for the debug cash key amounts: $1,000 a press in whole cents, saturating, and the zero rule. All pure.
public class DebugCashTests
{
    [Test]
    public void OnePressAddsExactlyOneThousandDollarsInCents()
    {
        Assert.That(DebugCash.StepCents, Is.EqualTo(100000L));
        Assert.That(DebugCash.After(0L), Is.EqualTo(100000L));
        Assert.That(DebugCash.After(12345L), Is.EqualTo(112345L));
    }

    [Test]
    public void ManyPressesAddUpExactly()
    {
        long cash = 5L;
        for (int i = 0; i < 25; i++)
            cash = DebugCash.After(cash);
        Assert.That(cash, Is.EqualTo(5L + 25 * 100000L));
    }

    [Test]
    public void AddingNeverWrapsAtTheTopOfTheRange()
    {
        Assert.That(DebugCash.After(long.MaxValue), Is.EqualTo(long.MaxValue));
    }

    [Test]
    public void TheZeroKeySetsNoCents()
    {
        Assert.That(DebugCash.ZeroCents, Is.EqualTo(0L));
    }

    [Test]
    public void TheMessagesShowTheAmountAndTheCash()
    {
        Assert.That(DebugCash.AddedMessage(112345L), Is.EqualTo("Debug: +$1,000.00 (cash $1,123.45)"));
        Assert.That(DebugCash.ZeroedMessage, Is.EqualTo("Debug: cash set to $0"));
    }
}
