using NUnit.Framework;

// EditMode tests for QuantityRules (Quantity_Controls.md): the clamp, the plus and minus steps, typed amounts and the combined maximum.
public class QuantityRulesTests
{
    // --- Clamp ---

    [TestCase(5, 10, 5)]
    [TestCase(1, 10, 1)]
    [TestCase(10, 10, 10)]
    public void AnAmountInRangeIsKept(int amount, int max, int expected)
    {
        Assert.That(QuantityRules.Clamp(amount, max), Is.EqualTo(expected));
    }

    [TestCase(0, 5)]
    [TestCase(-3, 5)]
    public void AnAmountBelowOneBecomesOne(int amount, int max)
    {
        Assert.That(QuantityRules.Clamp(amount, max), Is.EqualTo(1));
    }

    [Test]
    public void AnAmountAboveTheMaximumBecomesTheMaximum()
    {
        Assert.That(QuantityRules.Clamp(50, 12), Is.EqualTo(12));
        Assert.That(QuantityRules.Clamp(int.MaxValue, 7), Is.EqualTo(7));
    }

    [TestCase(1, 0)]
    [TestCase(5, 0)]
    [TestCase(0, 0)]
    [TestCase(3, -4)]
    public void WhenNothingCanMoveTheAmountIsZero(int amount, int max)
    {
        Assert.That(QuantityRules.Clamp(amount, max), Is.EqualTo(0));
    }

    [Test]
    public void TheAmountNeverPassesTheLargestAmount()
    {
        Assert.That(QuantityRules.Clamp(int.MaxValue, int.MaxValue), Is.EqualTo(QuantityRules.MaxAmount));
    }

    // --- Step ---

    [Test]
    public void PlusAndMinusStepByOne()
    {
        Assert.That(QuantityRules.Step(5, 1, 10), Is.EqualTo(6));
        Assert.That(QuantityRules.Step(5, -1, 10), Is.EqualTo(4));
    }

    [Test]
    public void MinusStopsAtOneAndPlusStopsAtTheMaximum()
    {
        Assert.That(QuantityRules.Step(1, -1, 10), Is.EqualTo(1));
        Assert.That(QuantityRules.Step(10, 1, 10), Is.EqualTo(10));
    }

    [Test]
    public void ShiftStepsByTenAndStaysInRange()
    {
        Assert.That(QuantityRules.StepFor(false), Is.EqualTo(1));
        Assert.That(QuantityRules.StepFor(true), Is.EqualTo(10));
        Assert.That(QuantityRules.Step(5, 10, 40), Is.EqualTo(15));
        Assert.That(QuantityRules.Step(35, 10, 40), Is.EqualTo(40), "clamped to the maximum");
        Assert.That(QuantityRules.Step(25, -10, 40), Is.EqualTo(15));
        Assert.That(QuantityRules.Step(5, -10, 40), Is.EqualTo(1), "clamped to one");
    }

    [Test]
    public void SteppingWhenNothingCanMoveStaysAtZero()
    {
        Assert.That(QuantityRules.Step(1, 1, 0), Is.EqualTo(0));
        Assert.That(QuantityRules.Step(0, 10, 0), Is.EqualTo(0));
    }

    [Test]
    public void SteppingFromAnOutOfRangeAmountComesBackIntoRange()
    {
        Assert.That(QuantityRules.Step(50, 1, 12), Is.EqualTo(12), "the stock fell since the amount was set");
        Assert.That(QuantityRules.Step(int.MaxValue, 10, 5), Is.EqualTo(5), "no overflow");
    }

    // --- TryParse ---

    [TestCase("5", 5)]
    [TestCase("12", 12)]
    [TestCase("999999", 999999)]
    [TestCase("007", 7)]
    [TestCase(" 7 ", 7)]
    public void DigitsAreRead(string text, int expected)
    {
        Assert.That(QuantityRules.TryParse(text, out int amount), Is.True);
        Assert.That(amount, Is.EqualTo(expected));
    }

    [TestCase("0")]
    [TestCase("000")]
    public void ZerosReadAsZero(string text)
    {
        Assert.That(QuantityRules.TryParse(text, out int amount), Is.True);
        Assert.That(amount, Is.EqualTo(0));
    }

    [TestCase("")]
    [TestCase("   ")]
    [TestCase(null)]
    [TestCase("abc")]
    [TestCase("1a")]
    [TestCase("a1")]
    [TestCase("-5")]
    [TestCase("+5")]
    [TestCase("12.5")]
    [TestCase("1,000")]
    [TestCase("1 000")]
    public void AnythingButDigitsIsIgnored(string text)
    {
        Assert.That(QuantityRules.TryParse(text, out int amount), Is.False);
        Assert.That(amount, Is.EqualTo(0));
    }

    [TestCase("1000000")]
    [TestCase("1234567890")]
    [TestCase("99999999999999999999999")]
    public void TooManyDigitsAreCappedNotRejected(string text)
    {
        Assert.That(QuantityRules.TryParse(text, out int amount), Is.True);
        Assert.That(amount, Is.EqualTo(QuantityRules.MaxAmount));
    }

    [Test]
    public void LeadingZerosDoNotCountTowardsTheLengthCap()
    {
        Assert.That(QuantityRules.TryParse("0000000012", out int amount), Is.True);
        Assert.That(amount, Is.EqualTo(12));
    }

    // --- ParseAndClamp ---

    [Test]
    public void ATypedAmountIsKeptInRange()
    {
        Assert.That(QuantityRules.ParseAndClamp("8", 1, 20), Is.EqualTo(8));
        Assert.That(QuantityRules.ParseAndClamp("500", 1, 20), Is.EqualTo(20), "more than there is: the most");
        Assert.That(QuantityRules.ParseAndClamp("0", 4, 20), Is.EqualTo(1), "zero: at least one");
    }

    [Test]
    public void TypedNonsenseKeepsTheCurrentAmount()
    {
        Assert.That(QuantityRules.ParseAndClamp("abc", 6, 20), Is.EqualTo(6));
        Assert.That(QuantityRules.ParseAndClamp("", 6, 20), Is.EqualTo(6));
        Assert.That(QuantityRules.ParseAndClamp("abc", 50, 20), Is.EqualTo(20), "the kept amount is clamped too");
    }

    [Test]
    public void TypingWhenNothingCanMoveGivesZero()
    {
        Assert.That(QuantityRules.ParseAndClamp("5", 1, 0), Is.EqualTo(0));
    }

    // --- MaxOf ---

    [Test]
    public void TheMaximumIsTheSmallestLimit()
    {
        Assert.That(QuantityRules.MaxOf(10, 4, 7), Is.EqualTo(4));
        Assert.That(QuantityRules.MaxOf(3), Is.EqualTo(3));
        Assert.That(QuantityRules.MaxOf(100, 100), Is.EqualTo(100));
    }

    [Test]
    public void AnyLimitOfZeroMeansNothingCanMove()
    {
        Assert.That(QuantityRules.MaxOf(10, 0, 7), Is.EqualTo(0));
        Assert.That(QuantityRules.MaxOf(10, -2, 7), Is.EqualTo(0));
        Assert.That(QuantityRules.MaxOf(0), Is.EqualTo(0));
    }

    [Test]
    public void NoLimitsMeansNothingCanMove()
    {
        Assert.That(QuantityRules.MaxOf(), Is.EqualTo(0));
        Assert.That(QuantityRules.MaxOf(null), Is.EqualTo(0));
    }

    [Test]
    public void UnlimitedLimitsStopAtTheLargestAmount()
    {
        Assert.That(QuantityRules.MaxOf(int.MaxValue, int.MaxValue), Is.EqualTo(QuantityRules.MaxAmount));
        Assert.That(QuantityRules.MaxOf(int.MaxValue, 12), Is.EqualTo(12), "a weightless item: the other limit decides");
    }

    [Test]
    public void TheMaximumFeedsTheClamp()
    {
        int max = QuantityRules.MaxOf(40, 12, 25); // carried 40, cash for 12, room for 25
        Assert.That(max, Is.EqualTo(12));
        Assert.That(QuantityRules.Clamp(30, max), Is.EqualTo(12));
        Assert.That(QuantityRules.Step(12, 1, max), Is.EqualTo(12));
    }

    // --- UnitsThatFit (the lantern tank's room for bottles, in hundredths of an hour) ---

    [TestCase(1601, 800, 2)]   // an empty 16 h tank with the 0.01 h tolerance: two 8 h bottles
    [TestCase(801, 800, 1)]    // 8 h of oil already in: one more
    [TestCase(1000, 800, 1)]
    [TestCase(799, 800, 0)]    // not quite a whole bottle
    [TestCase(1, 800, 0)]
    public void WholeUnitsThatFitInTheSpace(long free, long each, int expected)
    {
        Assert.That(QuantityRules.UnitsThatFit(free, each), Is.EqualTo(expected));
    }

    [TestCase(0, 800)]
    [TestCase(-50, 800)]
    public void NoFreeSpaceFitsNothing(long free, long each)
    {
        Assert.That(QuantityRules.UnitsThatFit(free, each), Is.EqualTo(0));
    }

    [Test]
    public void AUnitThatTakesNoRoomHasNoLimitButTheLargestAmount()
    {
        Assert.That(QuantityRules.UnitsThatFit(100, 0), Is.EqualTo(QuantityRules.MaxAmount));
        Assert.That(QuantityRules.UnitsThatFit(100, -3), Is.EqualTo(QuantityRules.MaxAmount));
        Assert.That(QuantityRules.UnitsThatFit(long.MaxValue, 1), Is.EqualTo(QuantityRules.MaxAmount));
    }

    [Test]
    public void TheRefillMaximumIsTheSmallerOfOilCarriedAndRoomInTheTank()
    {
        int room = QuantityRules.UnitsThatFit(1601, 800);
        Assert.That(QuantityRules.MaxOf(5, room), Is.EqualTo(2), "5 bottles carried, room for 2");
        Assert.That(QuantityRules.MaxOf(1, room), Is.EqualTo(1), "1 bottle carried, room for 2");
        Assert.That(QuantityRules.MaxOf(5, QuantityRules.UnitsThatFit(799, 800)), Is.EqualTo(0), "a full tank: nothing to pour");
    }

    [Test]
    public void BoilingNeedsThePotSoWithoutOneNothingCanBeQueued()
    {
        const int carried = 6;
        Assert.That(QuantityRules.MaxOf(carried, QuantityRules.MaxAmount), Is.EqualTo(6), "with a Cooking Pot: the carried count");
        Assert.That(QuantityRules.MaxOf(carried, 0), Is.EqualTo(0), "without one: nothing");
    }
}
