using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

// EditMode tests for taking a plain Campfire down (FireRefund, Homestead.Economy): half the build materials back rounded down, the unspent
// fuel as Firewood, the whole Ash, and the new saved flag. CampfireState is in the main assembly, reached by reflection.
public class FireRefundTests
{
    const float HoursPerFirewood = 2f;

    static Type Find(string name) =>
        AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => { try { return a.GetTypes(); } catch (Exception) { return new Type[0]; } })
                 .First(t => t.Name == name);

    [TestCase(0, 0)]
    [TestCase(1, 0)]
    [TestCase(2, 1)]
    [TestCase(3, 1)]
    [TestCase(4, 2)]
    [TestCase(6, 3)]
    [TestCase(7, 3)]
    [TestCase(-5, 0)]
    public void HalfBackIsHalfRoundedDownAndASinglePieceRefundsNothing(int paid, int back)
    {
        Assert.That(FireRefund.HalfBack(paid), Is.EqualTo(back));
    }

    [Test]
    public void AFireBuiltFromThreeFirewoodGivesOneBack()
    {
        FireRefundResult r = FireRefund.Of(false, 3, 6, 0f, HoursPerFirewood, 0f);
        Assert.That((r.firewood, r.sticks, r.ash), Is.EqualTo((1, 0, 0)));
    }

    [Test]
    public void AFireBuiltFromSticksGivesHalfTheSticksBackAndNoFirewood()
    {
        FireRefundResult r = FireRefund.Of(true, 3, 6, 0f, HoursPerFirewood, 0f);
        Assert.That((r.firewood, r.sticks, r.ash), Is.EqualTo((0, 3, 0)));
    }

    [Test]
    public void UnspentFuelComesBackAsWholeFirewoodAndJoinsTheBuildFirewood()
    {
        Assert.That(FireRefund.FuelToFirewood(7.9f, HoursPerFirewood), Is.EqualTo(3));
        Assert.That(FireRefund.FuelToFirewood(8f, HoursPerFirewood), Is.EqualTo(4));
        Assert.That(FireRefund.FuelToFirewood(7.99995f, HoursPerFirewood), Is.EqualTo(4), "float noise just under a whole piece");
        Assert.That(FireRefund.FuelToFirewood(1.9f, HoursPerFirewood), Is.EqualTo(0));
        Assert.That(FireRefund.FuelToFirewood(0f, HoursPerFirewood), Is.EqualTo(0));
        Assert.That(FireRefund.FuelToFirewood(-3f, HoursPerFirewood), Is.EqualTo(0));
        Assert.That(FireRefund.FuelToFirewood(5f, 0f), Is.EqualTo(0), "no hours per piece: nothing to convert");

        FireRefundResult r = FireRefund.Of(false, 3, 6, 6f, HoursPerFirewood, 0f);
        Assert.That(r.firewood, Is.EqualTo(1 + 3), "1 from the build and 3 from the fuel, as one amount");
    }

    [Test]
    public void AFireBuiltFromSticksStillGetsItsFuelBackAsFirewood()
    {
        FireRefundResult r = FireRefund.Of(true, 3, 6, 4f, HoursPerFirewood, 0f);
        Assert.That((r.firewood, r.sticks), Is.EqualTo((2, 3)));
    }

    [Test]
    public void OnlyTheWholeAshComesBackAndTheFractionIsLost()
    {
        Assert.That(FireRefund.Of(false, 3, 6, 0f, HoursPerFirewood, 2.6f).ash, Is.EqualTo(2));
        Assert.That(FireRefund.Of(false, 3, 6, 0f, HoursPerFirewood, 0.9f).ash, Is.EqualTo(0));
        Assert.That(FireRefund.Of(false, 3, 6, 0f, HoursPerFirewood, 0.99995f).ash, Is.EqualTo(1), "noise just under a whole Ash");
        Assert.That(FireRefund.Of(false, 3, 6, 0f, HoursPerFirewood, 0f).ash, Is.EqualTo(0));
    }

    [Test]
    public void AnEmptyRefundSaysSo()
    {
        Assert.That(FireRefund.Of(false, 1, 1, 0f, HoursPerFirewood, 0f).IsEmpty, Is.True);
        Assert.That(FireRefund.Of(false, 3, 6, 0f, HoursPerFirewood, 0f).IsEmpty, Is.False);
    }

    [Test]
    public void AnOldFireSaveLoadsAsBuiltFromFirewood()
    {
        const string oldJson = "{\"id\":3,\"position\":{\"x\":1.0,\"y\":2.0,\"z\":3.0},\"yaw\":45.0,\"fuelHours\":7.5,\"lit\":false}";
        Type fire = Find("CampfireState");
        object state = JsonUtility.FromJson(oldJson, fire);
        Assert.That(fire.GetField("builtWithSticks").GetValue(state), Is.EqualTo(false), "an old fire refunds as 3 Firewood, 1 back");
        Assert.That(fire.GetField("fuelHours").GetValue(state), Is.EqualTo(7.5f));
    }

    [Test]
    public void TheBuiltWithSticksFlagRoundTrips()
    {
        Type fire = Find("CampfireState");
        object state = Activator.CreateInstance(fire);
        fire.GetField("builtWithSticks").SetValue(state, true);
        object back = JsonUtility.FromJson(JsonUtility.ToJson(state), fire);
        Assert.That(fire.GetField("builtWithSticks").GetValue(back), Is.EqualTo(true));
    }
}
