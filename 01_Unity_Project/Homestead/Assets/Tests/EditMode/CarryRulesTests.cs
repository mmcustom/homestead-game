using NUnit.Framework;

// EditMode tests for the felled tree pile's all-or-nothing carry check: pure, no scene.
public class CarryRulesTests
{
    [Test]
    public void AChoiceFitsWhenItIsNoHeavierThanTheRoomLeft()
    {
        Assert.That(CarryRules.Fits(12.5f, 12.4f), Is.True);
        Assert.That(CarryRules.Fits(12.5f, 12.5f), Is.True, "an exact fit is allowed");
        Assert.That(CarryRules.Fits(12.5f, 12.6f), Is.False);
    }

    [Test]
    public void AnExactFitSurvivesFloatError()
    {
        float free = 45f - (8f * 4 + 0.1f * 3); // 12.7, carried as floats
        Assert.That(CarryRules.Fits(free, 12.7f), Is.True);
    }

    [Test]
    public void WeightlessChoicesAlwaysFitEvenInAFullPack()
    {
        Assert.That(CarryRules.Fits(0f, 0f), Is.True);
        Assert.That(CarryRules.Fits(-1f, 0f), Is.True);
    }

    [Test]
    public void NothingFitsInAFullPack()
    {
        Assert.That(CarryRules.Fits(0f, 0.1f), Is.False);
        Assert.That(CarryRules.Fits(0f, 8f), Is.False);
    }

    [Test]
    public void ARefusalNamesTheChoiceTheWeightAndTheRoom()
    {
        string text = CarryRules.Refusal("3 Logs", 12.46f, 24f, 45f);
        Assert.That(text, Is.EqualTo("Too heavy: 3 Logs weighs 24 kg and you can carry only 12.4 kg more (limit 45 kg). Nothing was taken."));
    }

    [Test]
    public void ARefusalRoundsSoItNeverClaimsItShouldHaveFit()
    {
        string text = CarryRules.Refusal("a Log", 7.96f, 8f, 45f);
        Assert.That(text, Does.Contain("weighs 8 kg"));
        Assert.That(text, Does.Contain("only 7.9 kg more"));
    }

    [Test]
    public void AFullPackSaysThereIsNoRoom()
    {
        string text = CarryRules.Refusal("a Log", 0f, 8f, 45f);
        Assert.That(text, Is.EqualTo("No room: you are already carrying the most you can (45 kg). Nothing was taken."));
    }

    [Test]
    public void LessThanATenthOfAKilogramOfRoomCountsAsNoRoomNotZeroKgMore()
    {
        Assert.That(CarryRules.Refusal("everything in the pile", 0.0999f, 27.3f, 45f), Does.StartWith("No room:"));
        Assert.That(CarryRules.Refusal("everything in the pile", 0.1f, 27.3f, 45f), Does.Contain("only 0.1 kg more"));
    }
}
