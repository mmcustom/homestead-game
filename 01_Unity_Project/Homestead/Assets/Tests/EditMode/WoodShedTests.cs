using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// EditMode tests for Structures Slice 1, phase B: the Wood Shed (a staged site) and wet or dry wood. The shed and moisture code live in
// the main assembly, which a test assembly cannot reference, so they are reached by reflection on throwaway objects, as SetbacksTests
// does; the placement rule is in the pure Setbacks assembly. Nothing here touches the Saves folder or a scene.
public class WoodShedTests
{
    static Type Find(string name) =>
        AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => { try { return a.GetTypes(); } catch (Exception) { return new Type[0]; } })
                 .First(t => t.Name == name);

    static object Kind(string name) => Enum.Parse(Find("PileKind"), name);

    static object NewPile(out Type state, string kind)
    {
        state = Find("WoodPileState");
        object pile = Activator.CreateInstance(state);
        state.GetField("kind").SetValue(pile, Kind(kind));
        return pile;
    }

    // --- The kinds and the site ---

    [Test]
    public void WoodShedKindsAreAppendedAndOldNumbersAreUnchanged()
    {
        Assert.That((int)Kind("FoodCacheSite"), Is.EqualTo(19), "the last phase A kind keeps its number");
        Assert.That((int)Kind("WoodShedSite"), Is.EqualTo(20));
        Assert.That((int)Kind("WoodShed"), Is.EqualTo(21));
        Assert.That((int)Kind("Felled"), Is.EqualTo(0));
        Assert.That((int)Kind("WoodStorage"), Is.EqualTo(1));
    }

    [Test]
    public void WoodShedIsAStagedSiteOf24KgWithTwoLogsAndEightBranches()
    {
        Type wells = Find("WellsAndOuthouses");
        Type stack = Find("WoodStack");
        Assert.That((bool)wells.GetMethod("IsStagedSite").Invoke(null, new[] { Kind("WoodShedSite") }), Is.True);
        Assert.That((bool)wells.GetMethod("IsStagedSite").Invoke(null, new[] { Kind("WoodShed") }), Is.False, "the finished shed is not a site");
        Assert.That(wells.GetMethod("FinishedKind").Invoke(null, new[] { Kind("WoodShedSite") }), Is.EqualTo(Kind("WoodShed")));
        Assert.That((bool)wells.GetMethod("RequiresDig").Invoke(null, new[] { Kind("WoodShedSite") }), Is.False, "no Shovel stage");

        object cost = wells.GetMethod("FinishedCost").Invoke(null, new[] { Kind("WoodShedSite") });
        var got = ((Array)cost).Cast<object>().Select(s => (string)stack.GetField("itemId").GetValue(s) + ":" + (int)stack.GetField("count").GetValue(s)).ToArray();
        Assert.That(got, Is.EquivalentTo(new[] { "logs:2", "branches:8" }));
        Assert.That((float)wells.GetMethod("WeightKg").Invoke(null, new[] { cost }), Is.EqualTo(24f).Within(0.001f), "2 x 8 + 8 x 1");
        Assert.That((bool)wells.GetMethod("NeedsStaging").Invoke(null, new[] { cost }), Is.True, "over the 15 kg cutoff");
        Assert.That(((Array)wells.GetMethod("WoodShedCost").Invoke(null, null)).Length, Is.EqualTo(2), "the finished shed's cost is what Dismantle halves");
    }

    [Test]
    public void WoodShedSiteTakesLogsAndBranchesInTripsAndStopsAtTheCost()
    {
        Type wells = Find("WellsAndOuthouses");
        object site = NewPile(out Type state, "WoodShedSite");
        MethodInfo accepts = wells.GetMethod("SiteAccepts");
        MethodInfo add = state.GetMethod("Add");
        bool Accepts(string id) => (bool)accepts.Invoke(null, new[] { site, id });

        Assert.That(Accepts("logs") && Accepts("branches"), Is.True);
        Assert.That(Accepts("sticks") || Accepts("firewood") || Accepts("stone") || Accepts("tall_grass"), Is.False);
        add.Invoke(site, new object[] { "logs", 2, 5 });       // trip one: 16 kg of Logs
        add.Invoke(site, new object[] { "branches", 8, 5 });   // trip two: 8 kg of Branches
        Assert.That(Accepts("logs") || Accepts("branches"), Is.False, "full");
        Assert.That(wells.GetMethod("SiteNeeds").Invoke(null, new[] { site }), Is.Null);
        Assert.That((bool)wells.GetMethod("SiteComplete").Invoke(null, new[] { site }), Is.True);
        Assert.That((bool)state.GetMethod("Accepts").Invoke(site, new object[] { "logs" }), Is.False, "WoodPileState.Accepts agrees");
    }

    [Test]
    public void WoodShedTakesTwentyFiveHammerBlowsAndNeedsNoHammerToPlace()
    {
        Type wells = Find("WellsAndOuthouses");
        var go = new GameObject("test wells");
        try
        {
            Component rules = go.AddComponent(wells);
            Assert.That(wells.GetField("woodShedBuildBlows").GetValue(rules), Is.EqualTo(25));
            Assert.That(wells.GetMethod("BuildBlows").Invoke(rules, new[] { Kind("WoodShedSite") }), Is.EqualTo(25));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(go);
        }
        Assert.That((bool)Find("WoodManager").GetMethod("NeedsHammerToBuild").Invoke(null, new[] { Kind("WoodShedSite") }), Is.False,
                    "placing and stocking a staged site are free; only pounding needs the Hammer");
    }

    [Test]
    public void WoodShedPlacementKeepsThreeMetresFromTheCabinAndSaysSo()
    {
        var sites = new List<SiteInfo> { new SiteInfo { kind = SiteKind.Building, position = Vector3.zero, name = "Small Cabin" } };
        Assert.That(SetbackRules.Defaults.shedFromBuilding, Is.EqualTo(3f));
        Assert.That(Setbacks.CanPlaceShed(new Vector3(2.9f, 0f, 0f), sites, SetbackRules.Defaults, out string reason), Is.False);
        Assert.That(reason, Is.EqualTo("Too close to the Small Cabin: 3 m needed."));
        Assert.That(Setbacks.CanPlaceShed(new Vector3(3.1f, 0f, 0f), sites, SetbackRules.Defaults, out _), Is.True);
        var farWell = new List<SiteInfo> { new SiteInfo { kind = SiteKind.Well, position = Vector3.zero, name = "well" } };
        Assert.That(Setbacks.CanPlaceShed(new Vector3(0.5f, 0f, 0f), farWell, SetbackRules.Defaults, out _), Is.True, "only buildings count");
    }

    [Test]
    public void WoodShedOpensAwayFromTheWind()
    {
        Type wells = Find("WellsAndOuthouses");
        MethodInfo downwind = wells.GetMethod("DownwindFor");
        MethodInfo away = wells.GetMethod("OpensAwayFromWind");
        Assert.That((float)downwind.Invoke(null, new object[] { 315f }), Is.EqualTo(135f).Within(0.001f), "north-west wind: the front faces south-east");
        Assert.That((float)downwind.Invoke(null, new object[] { 270f }), Is.EqualTo(90f).Within(0.001f));
        Assert.That((bool)away.Invoke(null, new object[] { 135f, 315f }), Is.True);
        Assert.That((bool)away.Invoke(null, new object[] { 315f, 315f }), Is.False, "facing into the wind");
        Assert.That((bool)away.Invoke(null, new object[] { 225f, 315f }), Is.True, "a quarter turn off is still sheltered");
        Assert.That((bool)away.Invoke(null, new object[] { 45f, 315f }), Is.True);
        Assert.That((bool)away.Invoke(null, new object[] { 0f, 315f }), Is.False);
        Assert.That(wells.GetMethod("CompassWord").Invoke(null, new object[] { 135f }), Is.EqualTo("south-east"));
        Assert.That(wells.GetMethod("CompassWord").Invoke(null, new object[] { 359f }), Is.EqualTo("north"));
    }

    // --- Wet and dry ---

    [Test]
    public void WoodIsDryOnceASeasonHasPassedSinceItWasWetAndNoStampMeansDry()
    {
        Type m = Find("WoodMoisture");
        MethodInfo isDry = m.GetMethod("IsDry", new[] { typeof(int), typeof(int), typeof(int) });
        bool Dry(int day, int today) => (bool)isDry.Invoke(null, new object[] { day, today, 28 });
        Assert.That(Dry(0, 3), Is.True, "no stamp: wood from before this existed, or marked dry");
        Assert.That(Dry(5, 5), Is.False, "wet today");
        Assert.That(Dry(5, 32), Is.False, "27 days on");
        Assert.That(Dry(5, 33), Is.True, "28 days on: a season");
        Assert.That(Dry(5, 100), Is.True);
    }

    [Test]
    public void SeasoningIsOneSeasonScaledToTheCalendarAndWetWoodBurnsSeventyPercent()
    {
        Type m = Find("WoodMoisture");
        MethodInfo daysFor = m.GetMethod("DaysFor");
        Assert.That(daysFor.Invoke(null, new object[] { 28, 1f }), Is.EqualTo(28), "one season of 28 days, about 14 real hours");
        Assert.That(daysFor.Invoke(null, new object[] { 28, 0.5f }), Is.EqualTo(14));
        Assert.That(daysFor.Invoke(null, new object[] { 20, 1f }), Is.EqualTo(20), "follows the calendar's season length");
        Assert.That((int)m.GetProperty("SeasoningDays").GetValue(null), Is.EqualTo(28), "default with no game running");

        MethodInfo factor = m.GetMethod("BurnFactor", new[] { typeof(int), typeof(int), typeof(int), typeof(float) });
        Assert.That((float)factor.Invoke(null, new object[] { 5, 10, 28, 0.7f }), Is.EqualTo(0.7f));
        Assert.That((float)factor.Invoke(null, new object[] { 0, 10, 28, 0.7f }), Is.EqualTo(1f));
        Assert.That((float)m.GetProperty("WetBurnFraction").GetValue(null), Is.EqualTo(0.7f).Within(0.0001f));

        Type wells = Find("WellsAndOuthouses");
        var go = new GameObject("test wells");
        try
        {
            Component rules = go.AddComponent(wells);
            Assert.That(wells.GetField("seasoningSeasons").GetValue(rules), Is.EqualTo(1f));
            Assert.That(wells.GetField("wetBurnFraction").GetValue(rules), Is.EqualTo(0.7f));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(go);
        }
    }

    [Test]
    public void AgedWoodMovesItsStampBackAndAnythingPastZeroIsDry()
    {
        Type m = Find("WoodMoisture");
        MethodInfo aged = m.GetMethod("Aged");
        Assert.That(aged.Invoke(null, new object[] { 40, 28 }), Is.EqualTo(12));
        Assert.That(aged.Invoke(null, new object[] { 10, 28 }), Is.EqualTo(0), "never negative: 0 is the dry stamp");
        Assert.That(aged.Invoke(null, new object[] { 0, 28 }), Is.EqualTo(0));
        Assert.That(m.GetMethod("DaysLeft").Invoke(null, new object[] { 5, 10, 28 }), Is.EqualTo(23));
        Assert.That(m.GetMethod("DaysLeft").Invoke(null, new object[] { 0, 10, 28 }), Is.EqualTo(0));
        Assert.That(m.GetMethod("Label").Invoke(null, new object[] { 0, 4 }), Is.EqualTo("dry"));
        Assert.That(m.GetMethod("Label").Invoke(null, new object[] { 3, 0 }), Is.EqualTo("wet"));
        Assert.That(m.GetMethod("Label").Invoke(null, new object[] { 2, 3 }), Is.EqualTo("2 wet, 3 dry"));
    }

    [Test]
    public void OnlyWoodHasAWetOrDryStateAndTheFourWoodItemsKeepTheirBatchDay()
    {
        Type m = Find("WoodMoisture");
        Type db = Find("ItemDatabase");
        foreach (string id in new[] { "logs", "branches", "sticks", "firewood" })
        {
            Assert.That((bool)m.GetMethod("Applies").Invoke(null, new object[] { id }), Is.True, id);
            object item = db.GetMethod("Get").Invoke(null, new object[] { id });
            Assert.That(item, Is.Not.Null, id);
            Assert.That((bool)item.GetType().GetProperty("KeepsBatchDay").GetValue(item), Is.True, id + " keeps its batch day");
            Assert.That((bool)item.GetType().GetProperty("TracksBatchDay").GetValue(item), Is.True, id);
        }
        object stone = db.GetMethod("Get").Invoke(null, new object[] { "stone" });
        Assert.That((bool)stone.GetType().GetProperty("TracksBatchDay").GetValue(stone), Is.False, "stone has no moisture and does not split into batches");
        Assert.That((bool)m.GetMethod("Applies").Invoke(null, new object[] { "stone" }), Is.False);
    }

    // --- Batches carried in a pack ---

    [Test]
    public void WoodBatchesStayApartByDayAndMoveAndRemoveOldestFirst()
    {
        Type container = Find("InventoryContainer");
        Type db = Find("ItemDatabase");
        object logs = db.GetMethod("Get").Invoke(null, new object[] { "logs" });
        object pack = Activator.CreateInstance(container, "pack", 500f);
        object other = Activator.CreateInstance(container, "shed", 500f);
        MethodInfo add = container.GetMethod("Add");
        add.Invoke(pack, new object[] { logs, 2, 30 });  // wet batch
        add.Invoke(pack, new object[] { logs, 3, 0 });   // dry batch
        add.Invoke(pack, new object[] { logs, 1, 30 });  // joins the wet one
        var batches = ((IEnumerable<(int day, int quantity)>)container.GetMethod("Batches").Invoke(pack, new object[] { "logs" })).ToList();
        Assert.That(batches, Is.EqualTo(new List<(int, int)> { (0, 3), (30, 3) }), "two batches, driest (oldest) first");

        Assert.That(container.GetMethod("MoveTo").Invoke(pack, new[] { other, "logs", 4 }), Is.EqualTo(4));
        var moved = ((IEnumerable<(int day, int quantity)>)container.GetMethod("Batches").Invoke(other, new object[] { "logs" })).ToList();
        Assert.That(moved, Is.EqualTo(new List<(int, int)> { (0, 3), (30, 1) }), "the day goes with the wood");
        var left = ((IEnumerable<(int day, int quantity)>)container.GetMethod("Batches").Invoke(pack, new object[] { "logs" })).ToList();
        Assert.That(left, Is.EqualTo(new List<(int, int)> { (30, 2) }));
    }

    [Test]
    public void RemoveBatchesReportsEachBatchTakenSoACallerCanKeepItsMoisture()
    {
        Type container = Find("InventoryContainer");
        object logs = Find("ItemDatabase").GetMethod("Get").Invoke(null, new object[] { "logs" });
        object pack = Activator.CreateInstance(container, "pack", 500f);
        container.GetMethod("Add").Invoke(pack, new object[] { logs, 2, 40 });
        container.GetMethod("Add").Invoke(pack, new object[] { logs, 2, 0 });
        var taken = new List<(int day, int count)>();
        Action<int, int> record = (day, count) => taken.Add((day, count));
        Assert.That(container.GetMethod("RemoveBatches").Invoke(pack, new object[] { "logs", 3, record }), Is.EqualTo(3));
        Assert.That(taken, Is.EqualTo(new List<(int, int)> { (0, 2), (40, 1) }), "dry first, then one wet");
    }

    [Test]
    public void MarkingCarriedWoodWetOrDryAndSeasoningItMergesTheBatches()
    {
        Type container = Find("InventoryContainer");
        MethodInfo get = Find("ItemDatabase").GetMethod("Get");
        object pack = Activator.CreateInstance(container, "pack", 500f);
        MethodInfo add = container.GetMethod("Add");
        add.Invoke(pack, new object[] { get.Invoke(null, new object[] { "logs" }), 2, 30 });
        add.Invoke(pack, new object[] { get.Invoke(null, new object[] { "logs" }), 2, 45 });
        add.Invoke(pack, new object[] { get.Invoke(null, new object[] { "stone" }), 2, 45 });
        MethodInfo map = container.GetMethod("MapBatchDays");
        Func<string, bool> wood = id => id == "logs";

        map.Invoke(pack, new object[] { wood, (Func<int, int>)(_ => 50) });
        var batches = ((IEnumerable<(int day, int quantity)>)container.GetMethod("Batches").Invoke(pack, new object[] { "logs" })).ToList();
        Assert.That(batches, Is.EqualTo(new List<(int, int)> { (50, 4) }), "marked wet: one batch");
        Assert.That(container.GetMethod("Count").Invoke(pack, new object[] { "stone" }), Is.EqualTo(2), "other items are left alone");

        map.Invoke(pack, new object[] { wood, (Func<int, int>)(d => Math.Max(0, d - 28)) });
        batches = ((IEnumerable<(int day, int quantity)>)container.GetMethod("Batches").Invoke(pack, new object[] { "logs" })).ToList();
        Assert.That(batches, Is.EqualTo(new List<(int, int)> { (22, 4) }));
    }

    [Test]
    public void SplittingCarriesTheWettestDayOntoTheFirewood()
    {
        Type axe = Find("AxeTool");
        Type stack = Find("WoodStack");
        MethodInfo wettest = axe.GetMethod("WettestDay", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.That(wettest, Is.Not.Null);
        var list = (System.Collections.IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(stack));
        object Batch(int day) { object b = Activator.CreateInstance(stack); stack.GetField("day").SetValue(b, day); return b; }
        Assert.That(wettest.Invoke(null, new object[] { list }), Is.EqualTo(0), "nothing taken is dry");
        list.Add(Batch(0));
        list.Add(Batch(31));
        list.Add(Batch(12));
        Assert.That(wettest.Invoke(null, new object[] { list }), Is.EqualTo(31), "a mix is as wet as its wettest part");
    }

    // --- Fires ---

    [Test]
    public void WetFuelBurnsSeventyPercentAndDryFuelBurnsAsBefore()
    {
        Type fire = Find("FireManager");
        MethodInfo factorOf = fire.GetMethod("BurnFactorOf");
        Assert.That((float)factorOf.Invoke(null, new object[] { "logs", 5 }), Is.EqualTo(0.7f).Within(0.0001f), "stamped (wet) and today is day 0 here");
        Assert.That((float)factorOf.Invoke(null, new object[] { "firewood", 0 }), Is.EqualTo(1f), "no stamp: dry, as every fire burned before");
        Assert.That((float)factorOf.Invoke(null, new object[] { "stone", 5 }), Is.EqualTo(1f), "not wood");
    }

    [Test]
    public void FuelGoesOnDriestFirstEachBatchAddingItsOwnBurnTimeUntilTheCapStops()
    {
        MethodInfo fits = Find("FireManager").GetMethod("PiecesThatFit");
        Func<int, float> factor = day => day == 0 ? 1f : 0.7f;
        var batches = new List<(int day, int quantity)> { (0, 2), (30, 4) }; // 2 dry then 4 wet Firewood
        object[] args = { batches, 2f, 0f, 12f, int.MaxValue, factor, 0f };
        Assert.That(fits.Invoke(null, args), Is.EqualTo(6), "4 + 4 x 1.4 = 9.6 h, all fit under 12");
        Assert.That((float)args[6], Is.EqualTo(9.6f).Within(0.001f));

        args = new object[] { batches, 2f, 6f, 12f, int.MaxValue, factor, 0f };
        Assert.That(fits.Invoke(null, args), Is.EqualTo(3), "6 h + 2 dry (4 h) + 1 wet (1.4 h) = 11.4 h; a second wet would pass 12");
        Assert.That((float)args[6], Is.EqualTo(11.4f).Within(0.001f));

        args = new object[] { batches, 2f, 0f, 12f, 3, factor, 0f };
        Assert.That(fits.Invoke(null, args), Is.EqualTo(3), "Shift adds all that fit; a plain click asks for one");
        Assert.That((float)args[6], Is.EqualTo(5.4f).Within(0.001f), "2 dry (4 h) + 1 wet (1.4 h)");

        args = new object[] { new List<(int, int)> { (0, 5) }, 10f, 4f, 12f, int.MaxValue, factor, 0f };
        Assert.That(fits.Invoke(null, args), Is.EqualTo(0), "a dry Log (10 h) does not fit on 4 h of fuel under a 12 h cap");
    }

    // --- Where the day is stamped and how rain acts ---

    [Test]
    public void FelledWoodIsStampedWetAndOnlyWoodIsStamped()
    {
        Type manager = Find("WoodManager");
        Type stack = Find("WoodStack");
        var list = (System.Collections.IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(stack));
        foreach (string id in new[] { "logs", "branches", "sticks", "stone" })
        {
            object s = Activator.CreateInstance(stack);
            stack.GetField("itemId").SetValue(s, id);
            stack.GetField("count").SetValue(s, 1);
            list.Add(s);
        }
        manager.GetMethod("WetStamped").Invoke(null, new object[] { list });
        int Day(int i) => (int)stack.GetField("day").GetValue(list[i]);
        Assert.That(new[] { Day(0), Day(1), Day(2) }, Is.All.EqualTo(1), "wet: never below 1, so it cannot be mistaken for the dry stamp");
        Assert.That(Day(3), Is.EqualTo(0));
    }

    [Test]
    public void RainWetsOpenPilesButNotTheShedAndRestampingMergesBatches()
    {
        Type state = Find("WoodPileState");
        foreach (string open in new[] { "Felled", "WoodStorage" })
        {
            object pile = NewPile(out _, open);
            Assert.That((bool)state.GetProperty("IsOpenWoodPile").GetValue(pile), Is.True, open);
        }
        foreach (string covered in new[] { "WoodShed", "FoodCache", "RockStorage", "WoodShedSite" })
        {
            object pile = NewPile(out _, covered);
            Assert.That((bool)state.GetProperty("IsOpenWoodPile").GetValue(pile), Is.False, covered);
        }

        object p = NewPile(out _, "WoodStorage");
        MethodInfo add = state.GetMethod("Add");
        add.Invoke(p, new object[] { "logs", 2, 0 });     // dry
        add.Invoke(p, new object[] { "logs", 1, 12 });    // wet, older
        add.Invoke(p, new object[] { "stone", 1, 0 });
        Assert.That((bool)state.GetMethod("RestampWood").Invoke(p, new object[] { 40 }), Is.True, "rain changed something");
        var contents = (System.Collections.IList)state.GetField("contents").GetValue(p);
        Type stack = Find("WoodStack");
        Assert.That(contents.Count, Is.EqualTo(2), "the three Logs are one wet batch now, plus the Stone");
        Assert.That(state.GetMethod("Count").Invoke(p, new object[] { "logs" }), Is.EqualTo(3));
        Assert.That((bool)state.GetMethod("RestampWood").Invoke(p, new object[] { 40 }), Is.False, "already wet: nothing to do");

        object[] args = { "logs", 41, 28, 0, 0 };
        state.GetMethod("CountMoisture").Invoke(p, args);
        Assert.That(((int)args[3], (int)args[4]), Is.EqualTo((3, 0)), "a day on: all wet");
        args = new object[] { "logs", 68, 28, 0, 0 };
        state.GetMethod("CountMoisture").Invoke(p, args);
        Assert.That(((int)args[3], (int)args[4]), Is.EqualTo((0, 3)), "28 days dry from the last rain: seasoned");
        Assert.That(stack, Is.Not.Null);
    }

    [Test]
    public void WoodInAShedSeasonsToDryAfterASeasonAndAgingAllWoodDoesIt()
    {
        Type state = Find("WoodPileState");
        object shed = NewPile(out _, "WoodShed");
        MethodInfo add = state.GetMethod("Add");
        add.Invoke(shed, new object[] { "logs", 2, 10 });
        add.Invoke(shed, new object[] { "firewood", 5, 20 });
        add.Invoke(shed, new object[] { "sticks", 3, 0 });
        object[] args = { "logs", 30, 28, 0, 0 };
        state.GetMethod("CountMoisture").Invoke(shed, args);
        Assert.That(((int)args[3], (int)args[4]), Is.EqualTo((2, 0)), "20 days: still wet");

        state.GetMethod("AgeWood").Invoke(shed, new object[] { 28 }); // the debug "advance a season" key
        args = new object[] { "logs", 30, 28, 0, 0 };
        state.GetMethod("CountMoisture").Invoke(shed, args);
        Assert.That(((int)args[3], (int)args[4]), Is.EqualTo((0, 2)), "a season on: dry");
        args = new object[] { "firewood", 30, 28, 0, 0 };
        state.GetMethod("CountMoisture").Invoke(shed, args);
        Assert.That(((int)args[3], (int)args[4]), Is.EqualTo((0, 5)));
    }

    // --- Saves ---

    [Test]
    public void AnOldWoodStackWithNoDayLoadsAsDryWithoutErrors()
    {
        Type state = Find("WoodPileState");
        const string old = "{\"id\":3,\"kind\":1,\"position\":{\"x\":1.0,\"y\":0.0,\"z\":2.0},\"yaw\":0.0,\"contents\":[{\"itemId\":\"logs\",\"count\":4},{\"itemId\":\"firewood\",\"count\":6,\"day\":0}]}";
        object pile = JsonUtility.FromJson(old, state);
        Assert.That(pile, Is.Not.Null);
        object[] args = { "logs", 5, 28, 0, 0 };
        state.GetMethod("CountMoisture").Invoke(pile, args);
        Assert.That(((int)args[3], (int)args[4]), Is.EqualTo((0, 4)), "old wood has no stamp: dry, so every fire burns it exactly as before");
        Assert.That((float)Find("FireManager").GetMethod("BurnFactorOf").Invoke(null, new object[] { "logs", 0 }), Is.EqualTo(1f));
    }

    [Test]
    public void WoodShedSiteAndFinishedShedRoundTripWithTheirDays()
    {
        Type state = Find("WoodPileState");
        object site = NewPile(out _, "WoodShedSite");
        state.GetMethod("Add").Invoke(site, new object[] { "logs", 2, 14 });
        state.GetField("yaw").SetValue(site, 135f);
        state.GetField("buildProgress").SetValue(site, 0.4f);
        object back = JsonUtility.FromJson(JsonUtility.ToJson(site), state);
        Assert.That(state.GetField("kind").GetValue(back), Is.EqualTo(Kind("WoodShedSite")));
        Assert.That(state.GetMethod("Count").Invoke(back, new object[] { "logs" }), Is.EqualTo(2));
        Assert.That(state.GetField("yaw").GetValue(back), Is.EqualTo(135f));
        Assert.That(state.GetField("buildProgress").GetValue(back), Is.EqualTo(0.4f));

        object shed = NewPile(out _, "WoodShed");
        state.GetMethod("Add").Invoke(shed, new object[] { "firewood", 7, 22 });
        state.GetMethod("Add").Invoke(shed, new object[] { "firewood", 3, 0 });
        object loaded = JsonUtility.FromJson(JsonUtility.ToJson(shed), state);
        Assert.That(state.GetField("kind").GetValue(loaded), Is.EqualTo(Kind("WoodShed")));
        object[] args = { "firewood", 30, 28, 0, 0 };
        state.GetMethod("CountMoisture").Invoke(loaded, args);
        Assert.That(((int)args[3], (int)args[4]), Is.EqualTo((7, 3)), "the wet and dry batches survive the save");
        Assert.That((bool)state.GetMethod("Accepts").Invoke(loaded, new object[] { "logs" }), Is.True);
        Assert.That((bool)state.GetMethod("Accepts").Invoke(loaded, new object[] { "stone" }), Is.False);
        Assert.That((bool)state.GetProperty("IsPlacedStructure").GetValue(loaded), Is.False, "a finished shed is drawn by WoodPile");
        Assert.That((bool)state.GetProperty("IsPlacedStructure").GetValue(site), Is.True, "the site is drawn by PlacedStructure");
    }

    [Test]
    public void NoClockFieldsWereAddedByThisWork()
    {
        Type wells = Find("WellsAndOuthouses");
        foreach (FieldInfo f in wells.GetFields())
            Assert.That(f.Name.ToLower(), Does.Not.Contain("minutes").And.Not.Contain("skip"), f.Name);
        Assert.That(wells.GetMethods().Any(x => x.Name.Contains("Skip") || x.Name.Contains("Advance") && x.Name != "AdvanceBuild"), Is.False);
    }
}
