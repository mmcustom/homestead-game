using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// EditMode tests for Well Water Table B3: the real Well uses the depth to water saved on its WoodPileState. The pure tier numbers are in
// WellWaterTests; here the main-assembly pieces that read a pile's depth (the Stone lining it asks for, what it accepts, its numbers, the
// saved field and the old-save default) are reached by reflection on throwaway piles, the way SetbacksTests does.
public class WellDepthTests
{
    static Type Find(string name) =>
        AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => { try { return a.GetTypes(); } catch (Exception) { return new Type[0]; } })
                 .First(t => t.Name == name);

    static readonly Type Wells = Find("WellsAndOuthouses");
    static readonly Type State = Find("WoodPileState");
    static readonly Type Kind = Find("PileKind");

    static object Pile(string kind, float depth)
    {
        object pile = Activator.CreateInstance(State);
        State.GetField("kind").SetValue(pile, Enum.Parse(Kind, kind));
        State.GetField("depthToWater").SetValue(pile, depth);
        return pile;
    }

    static int StoneNeeded(object pile)
    {
        var cost = (Array)Wells.GetMethod("FinishedCostFor").Invoke(null, new[] { pile });
        Type stack = Find("WoodStack");
        foreach (object c in cost)
            if ((string)stack.GetField("itemId").GetValue(c) == "stone")
                return (int)stack.GetField("count").GetValue(c);
        return -1;
    }

    static WellWaterResult Numbers(object pile) => (WellWaterResult)Wells.GetMethod("WellNumbers").Invoke(null, new[] { pile });

    static void Deliver(object pile, string itemId, int count) => State.GetMethod("Add").Invoke(pile, new object[] { itemId, count, 0 });

    [Test]
    public void TheDepthFieldIsAppendedSoAnOldPileSaveLoadsItAtZero()
    {
        const string oldJson = "{\"id\":7,\"kind\":14,\"position\":{\"x\":1.0,\"y\":2.0,\"z\":3.0},\"yaw\":90.0,\"contents\":[],\"litres\":120,\"contaminatedHours\":0.0,\"digProgress\":1.0,\"builtDay\":3,\"buildProgress\":0.0}";
        object pile = JsonUtility.FromJson(oldJson, State);
        Assert.That(State.GetField("depthToWater").GetValue(pile), Is.EqualTo(0f), "a well saved before depth existed");
        Assert.That(State.GetField("litres").GetValue(pile), Is.EqualTo(120), "the rest of the old well is untouched");
    }

    [Test]
    public void TheDepthRoundTripsThroughTheSave()
    {
        object pile = Pile("WellSite", 7.25f);
        object back = JsonUtility.FromJson(JsonUtility.ToJson(pile), State);
        Assert.That((float)State.GetField("depthToWater").GetValue(back), Is.EqualTo(7.25f));
    }

    [Test]
    public void AWellAskedForTheStoneItsDepthNeeds()
    {
        Assert.That(StoneNeeded(Pile("WellSite", 2f)), Is.EqualTo(6), "Shallow");
        Assert.That(StoneNeeded(Pile("WellSite", 4.5f)), Is.EqualTo(10), "Medium");
        Assert.That(StoneNeeded(Pile("WellSite", 7.5f)), Is.EqualTo(14), "Deep");
        Assert.That(StoneNeeded(Pile("Well", 7.5f)), Is.EqualTo(14), "the finished Well, which Dismantle halves");
    }

    [Test]
    public void AnOldWellWithDepthZeroStillNeedsTodaysTenStoneAndTheOtherSitesAreUnchanged()
    {
        Assert.That(StoneNeeded(Pile("WellSite", 0f)), Is.EqualTo(10));
        Assert.That(StoneNeeded(Pile("Well", 0f)), Is.EqualTo(10));
        Assert.That(StoneNeeded(Pile("OuthouseSite", 7.5f)), Is.EqualTo(-1), "an Outhouse takes no Stone whatever its field says");
        Assert.That(StoneNeeded(Pile("KitchenSite", 7.5f)), Is.EqualTo(4), "the Kitchen's hearth Stone is its own");
    }

    [Test]
    public void TheCapTimberIsTheSameAtEveryDepth()
    {
        Type stack = Find("WoodStack");
        foreach (float depth in new[] { 0f, 2f, 4.5f, 7.5f })
        {
            var cost = ((Array)Wells.GetMethod("FinishedCostFor").Invoke(null, new[] { Pile("WellSite", depth) })).Cast<object>()
                .ToDictionary(c => (string)stack.GetField("itemId").GetValue(c), c => (int)stack.GetField("count").GetValue(c));
            Assert.That(cost["logs"], Is.EqualTo(2));
            Assert.That(cost["branches"], Is.EqualTo(4));
        }
    }

    [Test]
    public void AShallowSiteStopsAcceptingStoneAtSixAndADeepOneKeepsGoingToFourteen()
    {
        MethodInfo accepts = Wells.GetMethod("SiteAccepts");
        foreach ((float depth, int need) in new[] { (2f, 6), (4.5f, 10), (7.5f, 14), (0f, 10) })
        {
            object site = Pile("WellSite", depth);
            State.GetField("digProgress").SetValue(site, 1f); // the shaft is dug, so the lining is next
            Deliver(site, "stone", need - 1);
            Assert.That((bool)accepts.Invoke(null, new[] { site, "stone" }), Is.True, $"depth {depth}: one more Stone is wanted");
            Deliver(site, "stone", 1);
            Assert.That((bool)accepts.Invoke(null, new[] { site, "stone" }), Is.False, $"depth {depth}: full at {need}");
        }
    }

    [Test]
    public void AWellSiteSaysWhatItStillLacksInItsOwnStone()
    {
        MethodInfo needs = Wells.GetMethod("SiteNeeds");
        object deep = Pile("WellSite", 7.5f);
        State.GetField("digProgress").SetValue(deep, 1f);
        Deliver(deep, "stone", 6);
        Assert.That((string)needs.Invoke(null, new[] { deep }), Does.StartWith("8 Stone"), "14 needed, 6 delivered");

        object shallow = Pile("WellSite", 2f);
        State.GetField("digProgress").SetValue(shallow, 1f);
        Deliver(shallow, "stone", 6);
        Assert.That((string)needs.Invoke(null, new[] { shallow }), Does.StartWith("2 Logs"), "6 Stone is the whole lining: only the cap is left");
    }

    [Test]
    public void EveryWellReadsItsNumbersFromItsOwnDepthAndOldWellsGetTodays()
    {
        Assert.That(Numbers(Pile("Well", 2f)).digBlows, Is.EqualTo(20));
        Assert.That(Numbers(Pile("Well", 4.5f)).bufferLitres, Is.EqualTo(150));
        WellWaterResult deep = Numbers(Pile("Well", 7.5f));
        Assert.That((deep.digBlows, deep.liningStone, deep.bufferLitres, deep.recoveryLitresPerHour), Is.EqualTo((60, 14, 120, 4.5f)));

        WellWaterResult old = Numbers(Pile("Well", 0f));
        Assert.That((old.digBlows, old.liningStone, old.bufferLitres, old.recoveryLitresPerHour, old.droughtRecoveryLitresPerHour),
                    Is.EqualTo((40, 10, 150, 6f, 3f)));
        Assert.That(old.tier, Is.EqualTo(WellTier.Legacy));
    }

    [Test]
    public void LegacyEqualsMediumInTheDefaultTable()
    {
        WellWaterRules rules = WellWaterRules.Defaults;
        Assert.That((rules.legacy.digBlows, rules.legacy.liningStone, rules.legacy.bufferLitres, rules.legacy.recoveryLitresPerHour, rules.legacy.droughtRecoveryLitresPerHour),
                    Is.EqualTo((rules.medium.digBlows, rules.medium.liningStone, rules.medium.bufferLitres, rules.medium.recoveryLitresPerHour, rules.medium.droughtRecoveryLitresPerHour)));
    }

    [Test]
    public void TheDeepRowIsMikesDecision()
    {
        WellTierNumbers deep = WellWaterRules.Defaults.deep;
        Assert.That((deep.digBlows, deep.liningStone, deep.bufferLitres, deep.recoveryLitresPerHour, deep.droughtRecoveryLitresPerHour),
                    Is.EqualTo((60, 14, 120, 4.5f, 4.5f)));
    }
}
