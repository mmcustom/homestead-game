using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

// EditMode tests for Potash B's parts in the main assembly, reached by reflection as the other world tests do: the Leach Hopper's pile kinds and
// saved field, its cost and staged site, what the hourly tick does to a batch, and the swap that boils Lye into Potash in a pack.
public class PotashWorldTests
{
    static Type Find(string name) =>
        AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => { try { return a.GetTypes(); } catch (Exception) { return new Type[0]; } })
                 .First(t => t.Name == name);

    static object Item(string id) => Find("ItemDatabase").GetMethod("Get").Invoke(null, new object[] { id });

    static object Kind(string name) => Enum.Parse(Find("PileKind"), name);

    static object Static(string type, string method, params object[] args) =>
        Find(type).GetMethod(method, args.Select(a => a.GetType()).ToArray()).Invoke(null, args);

    // --- The pile kinds ---

    [Test]
    public void TheHopperKindsAreAppendedAsTwentyNineAndThirty()
    {
        Type kinds = Find("PileKind");
        Assert.That((int)Kind("LeachHopperSite"), Is.EqualTo(29));
        Assert.That((int)Kind("LeachHopper"), Is.EqualTo(30));
        Assert.That((int)Kind("SlopSink"), Is.EqualTo(28), "the last kind before them is untouched");
        Assert.That((int)Kind("Felled"), Is.EqualTo(0));
        Assert.That((int)Kind("WoodShed"), Is.EqualTo(21), "saved piles keep their kinds");
        Assert.That(Enum.GetValues(kinds).Length, Is.EqualTo(31));
    }

    [Test]
    public void TheHopperSiteIsAStagedSiteThatBecomesTheHopper()
    {
        Assert.That(Static("WellsAndOuthouses", "IsStagedSite", Kind("LeachHopperSite")), Is.EqualTo(true));
        Assert.That(Static("WellsAndOuthouses", "IsStagedSite", Kind("LeachHopper")), Is.EqualTo(false));
        Assert.That(Static("WellsAndOuthouses", "FinishedKind", Kind("LeachHopperSite")), Is.EqualTo(Kind("LeachHopper")));
        Assert.That(Static("WellsAndOuthouses", "FinalKind", Kind("LeachHopperSite")), Is.EqualTo(Kind("LeachHopper")));
        Assert.That(Static("WoodManager", "PileName", Kind("LeachHopperSite")), Is.EqualTo("Leach Hopper Site"));
        Assert.That(Static("WoodManager", "PileName", Kind("LeachHopper")), Is.EqualTo("Leach Hopper"));
    }

    static (string id, int count)[] Cost(object[] stacks) =>
        stacks.Select(s => ((string)s.GetType().GetField("itemId").GetValue(s), (int)s.GetType().GetField("count").GetValue(s))).ToArray();

    [Test]
    public void TheHopperTakesThreeLogsAndFourTallGrassAndSoIsOverTheCarryRule()
    {
        object[] cost = ((Array)Static("WellsAndOuthouses", "LeachHopperCost")).Cast<object>().ToArray();
        Assert.That(Cost(cost), Is.EquivalentTo(new[] { ("logs", 3), ("tall_grass", 4) }));
        object stacks = Array.CreateInstance(Find("WoodStack"), cost.Length);
        for (int i = 0; i < cost.Length; i++)
            ((Array)stacks).SetValue(cost[i], i);
        float kg = (float)Static("WellsAndOuthouses", "WeightKg", stacks);
        Assert.That(kg, Is.EqualTo(24.4f).Within(0.05f), "3 Logs and 4 Tall Grass");
        Assert.That(kg, Is.GreaterThan(15f), "over the staged-site line, so it is delivered over trips");
        Assert.That(Static("WellsAndOuthouses", "NeedsStaging", stacks), Is.EqualTo(true));
    }

    [Test]
    public void TheSiteTakesLogsThenTallGrassAndNothingElse()
    {
        object order = Static("WellsAndOuthouses", "DeliveryOrder", Kind("LeachHopperSite"));
        string[][] groups = (string[][])order;
        Assert.That(groups.Length, Is.EqualTo(1));
        Assert.That(groups[0], Is.EquivalentTo(new[] { "logs", "tall_grass" }));
        Assert.That(Static("WellsAndOuthouses", "NeedOf", Kind("LeachHopperSite"), "logs"), Is.EqualTo(3));
        Assert.That(Static("WellsAndOuthouses", "NeedOf", Kind("LeachHopperSite"), "tall_grass"), Is.EqualTo(4));
        Assert.That(Static("WellsAndOuthouses", "NeedOf", Kind("LeachHopperSite"), "branches"), Is.EqualTo(0));
    }

    [Test]
    public void AFinishedHopperTakesNothingByTheNormalDeliveryRoute()
    {
        object hopper = NewPile("LeachHopper");
        foreach (string id in new[] { "logs", "tall_grass", "firewood", "ash", "water_good" })
            Assert.That(hopper.GetType().GetMethod("Accepts").Invoke(hopper, new object[] { id }), Is.EqualTo(false), id + " (R loads it through the hopper's own rule)");
        Assert.That(hopper.GetType().GetProperty("IsPlacedStructure").GetValue(hopper), Is.EqualTo(true));
        Assert.That(NewPile("LeachHopperSite").GetType().GetProperty("IsPlacedStructure").GetValue(NewPile("LeachHopperSite")), Is.EqualTo(true));
    }

    // --- The saved pile ---

    static object NewPile(string kind)
    {
        Type pile = Find("WoodPileState");
        object state = Activator.CreateInstance(pile);
        pile.GetField("kind").SetValue(state, Kind(kind));
        return state;
    }

    static void Add(object pile, string id, int count) => pile.GetType().GetMethod("Add").Invoke(pile, new object[] { id, count, 0 });

    static int Count(object pile, string id) => (int)pile.GetType().GetMethod("Count").Invoke(pile, new object[] { id });

    [Test]
    public void APileHasAnAdditiveLeachHoursFieldThatStartsAtZero()
    {
        Type pile = Find("WoodPileState");
        Assert.That(pile.GetField("leachHours").FieldType, Is.EqualTo(typeof(float)));
        Assert.That(pile.GetField("leachHours").GetValue(Activator.CreateInstance(pile)), Is.EqualTo(0f));
    }

    [Test]
    public void APileSavedBeforeTheHopperLoadsWithNoLeachInProgress()
    {
        Type pile = Find("WoodPileState");
        object old = JsonUtility.FromJson("{\"id\":5,\"kind\":21,\"yaw\":10.0,\"litres\":3,\"builtDay\":4,\"buildProgress\":0.5}", pile);
        Assert.That(pile.GetField("id").GetValue(old), Is.EqualTo(5));
        Assert.That(pile.GetField("kind").GetValue(old), Is.EqualTo(Kind("WoodShed")));
        Assert.That(pile.GetField("litres").GetValue(old), Is.EqualTo(3));
        Assert.That(pile.GetField("leachHours").GetValue(old), Is.EqualTo(0f));
    }

    [Test]
    public void AHopperMidLeachSurvivesASaveAndALoad()
    {
        Type pile = Find("WoodPileState"), save = Find("WoodSaveData");
        object hopper = NewPile("LeachHopper");
        pile.GetField("id").SetValue(hopper, 9);
        pile.GetField("leachHours").SetValue(hopper, 17f);
        Add(hopper, "ash", 8);
        Add(hopper, "lye", 2);

        object data = Activator.CreateInstance(save);
        IList piles = (IList)Activator.CreateInstance(typeof(System.Collections.Generic.List<>).MakeGenericType(pile));
        piles.Add(hopper);
        save.GetField("piles").SetValue(data, piles);

        object back = JsonUtility.FromJson(JsonUtility.ToJson(data), save);
        IList backPiles = (IList)save.GetField("piles").GetValue(back);
        Assert.That(backPiles.Count, Is.EqualTo(1));
        object loaded = backPiles[0];
        Assert.That(pile.GetField("kind").GetValue(loaded), Is.EqualTo(Kind("LeachHopper")));
        Assert.That(pile.GetField("leachHours").GetValue(loaded), Is.EqualTo(17f));
        Assert.That(Count(loaded, "ash"), Is.EqualTo(8));
        Assert.That(Count(loaded, "lye"), Is.EqualTo(2));
    }

    [Test]
    public void ASaveWithAHopperNoLongerLeachingKeepsItsLyeAndHasNoTimer()
    {
        Type pile = Find("WoodPileState");
        object hopper = NewPile("LeachHopper");
        Add(hopper, "lye", 12);
        object back = JsonUtility.FromJson(JsonUtility.ToJson(hopper), pile);
        Assert.That(pile.GetField("leachHours").GetValue(back), Is.EqualTo(0f));
        Assert.That(Count(back, "lye"), Is.EqualTo(12));
    }

    // --- The hourly tick ---

    [Test]
    public void WhenABatchFinishesItsAshBecomesLyeOneForOneAndKeepsTheLyeAlreadyThere()
    {
        object hopper = NewPile("LeachHopper");
        Add(hopper, "ash", 8);
        Add(hopper, "lye", 3);
        Find("WellsAndOuthouses").GetMethod("TurnAshIntoLye").Invoke(null, new[] { hopper });
        Assert.That(Count(hopper, "ash"), Is.EqualTo(0));
        Assert.That(Count(hopper, "lye"), Is.EqualTo(11));
        Assert.That((int)hopper.GetType().GetProperty("Total").GetValue(hopper), Is.EqualTo(11), "nothing made or lost");
    }

    [Test]
    public void AHopperWithNoAshIsUnchangedWhenItsClockRunsOut()
    {
        object hopper = NewPile("LeachHopper");
        Add(hopper, "lye", 4);
        Find("WellsAndOuthouses").GetMethod("TurnAshIntoLye").Invoke(null, new[] { hopper });
        Assert.That(Count(hopper, "lye"), Is.EqualTo(4));
    }

    // --- Dismantling ---

    [Test]
    public void DismantlingHalvesTheHopperCostRoundedDown()
    {
        // 3 Logs -> 1, 4 Tall Grass -> 2; the Ash and Lye inside come back whole on top of that (PlacedStructure.Dismantle).
        Assert.That(PotashRules.HopperLogs / 2, Is.EqualTo(1));
        Assert.That(PotashRules.HopperTallGrass / 2, Is.EqualTo(2));
    }

    // --- The items the hopper moves ---

    [Test]
    public void AshLyeAndWaterAreRealItemsTheHopperCanLoad()
    {
        foreach (string id in new[] { "ash", "lye" }.Concat(PotashRules.WaterWorstFirst))
            Assert.That(Item(id), Is.Not.Null, id);
    }

    // --- Boiling Lye into Potash in a pack (Cooking.BoilLyeInto) ---

    static object Pack(float maxKg) => Activator.CreateInstance(Find("InventoryContainer"), "test pack", maxKg);

    static int PackCount(object pack, string id) => (int)pack.GetType().GetMethod("Count").Invoke(pack, new object[] { id });

    static void Give(object pack, string id, int quantity, int day = 0) =>
        pack.GetType().GetMethod("Add").Invoke(pack, new object[] { Item(id), quantity, day });

    static int Boil(object pack, int batches) => (int)Find("Cooking").GetMethod("BoilLyeInto").Invoke(null, new object[] { pack, batches, 0 });

    static (int day, int quantity)[] Days(object pack, string id)
    {
        IList batches = (IList)pack.GetType().GetMethod("Batches").Invoke(pack, new object[] { id });
        return batches.Cast<object>().Select(b => ((int)b.GetType().GetField("Item1").GetValue(b), (int)b.GetType().GetField("Item2").GetValue(b))).ToArray();
    }

    [Test]
    public void ThreeLyeAndTwoFirewoodMakeOnePotash()
    {
        object pack = Pack(100f);
        Give(pack, "lye", 3);
        Give(pack, "firewood", 2);
        Assert.That(Boil(pack, int.MaxValue), Is.EqualTo(1));
        Assert.That(PackCount(pack, "lye"), Is.EqualTo(0));
        Assert.That(PackCount(pack, "firewood"), Is.EqualTo(0));
        Assert.That(PackCount(pack, "potash"), Is.EqualTo(1));
    }

    [Test]
    public void ABoilMakesWholeBatchesAndKeepsWhatIsLeftOver()
    {
        object pack = Pack(100f);
        Give(pack, "lye", 8);
        Give(pack, "firewood", 9);
        Assert.That(Boil(pack, int.MaxValue), Is.EqualTo(2), "8 Lye is two batches");
        Assert.That(PackCount(pack, "lye"), Is.EqualTo(2));
        Assert.That(PackCount(pack, "firewood"), Is.EqualTo(5));
        Assert.That(PackCount(pack, "potash"), Is.EqualTo(2));
    }

    [Test]
    public void TheFirewoodLimitsTheBatches()
    {
        object pack = Pack(100f);
        Give(pack, "lye", 12);
        Give(pack, "firewood", 5);
        Assert.That(Boil(pack, int.MaxValue), Is.EqualTo(2), "5 Firewood is two batches");
        Assert.That(PackCount(pack, "lye"), Is.EqualTo(6));
        Assert.That(PackCount(pack, "firewood"), Is.EqualTo(1));
    }

    [Test]
    public void OneBatchAtATimeTakesThreeLyeAndTwoFirewoodEachTime()
    {
        object pack = Pack(100f);
        Give(pack, "lye", 9);
        Give(pack, "firewood", 6);
        for (int made = 1; made <= 3; made++)
        {
            Assert.That(Boil(pack, 1), Is.EqualTo(1));
            Assert.That(PackCount(pack, "lye"), Is.EqualTo(9 - made * 3));
            Assert.That(PackCount(pack, "firewood"), Is.EqualTo(6 - made * 2));
            Assert.That(PackCount(pack, "potash"), Is.EqualTo(made));
        }
        Assert.That(Boil(pack, 1), Is.EqualTo(0), "nothing left to boil");
    }

    [TestCase(2, 10)]
    [TestCase(9, 1)]
    [TestCase(0, 0)]
    public void TooLittleLyeOrFirewoodChangesNothing(int lye, int firewood)
    {
        object pack = Pack(100f);
        Give(pack, "lye", lye);
        Give(pack, "firewood", firewood);
        Assert.That(Boil(pack, int.MaxValue), Is.EqualTo(0));
        Assert.That(PackCount(pack, "lye"), Is.EqualTo(lye));
        Assert.That(PackCount(pack, "firewood"), Is.EqualTo(firewood));
        Assert.That(PackCount(pack, "potash"), Is.EqualTo(0));
    }

    [Test]
    public void AskingForNoBatchesChangesNothing()
    {
        object pack = Pack(100f);
        Give(pack, "lye", 6);
        Give(pack, "firewood", 4);
        Assert.That(Boil(pack, 0), Is.EqualTo(0));
        Assert.That(PackCount(pack, "lye"), Is.EqualTo(6));
    }

    [Test]
    public void TheFirewoodUsedIsTheOldestAndTheRestKeepsItsDay()
    {
        object pack = Pack(100f);
        Give(pack, "lye", 3);
        Give(pack, "firewood", 1, day: 4);
        Give(pack, "firewood", 3, day: 9);
        Assert.That(Boil(pack, 1), Is.EqualTo(1));
        Assert.That(Days(pack, "firewood"), Is.EqualTo(new[] { (9, 2) }), "the day-4 piece and one day-9 piece went into the boil");
    }

    [Test]
    public void TheBoilGivesNoAshBack()
    {
        // The Firewood is an ingredient, not fuel: it leaves the pack and nothing else arrives but the Potash.
        object pack = Pack(100f);
        Give(pack, "lye", 3);
        Give(pack, "firewood", 2);
        Boil(pack, 1);
        Assert.That(PackCount(pack, "ash"), Is.EqualTo(0));
        Assert.That(PackCount(pack, "firewood"), Is.EqualTo(0));
    }

    // --- Which boil uses which vessel ---

    [Test]
    public void LyeBoilsInTheKettleAndWaterAndSapStillBoilInThePot()
    {
        Assert.That(Static("Cooking", "BoilVessel", "lye"), Is.EqualTo("potash_kettle"));
        Assert.That(Static("Cooking", "BoilVessel", "maple_sap"), Is.EqualTo("cooking_pot"));
        Assert.That(Static("Cooking", "BoilVessel", "water_good"), Is.EqualTo("cooking_pot"));
        Assert.That(Static("Cooking", "BoilVessel", "water_unsafe"), Is.EqualTo("cooking_pot"));
        Assert.That(Static("Cooking", "BoilVessel", "venison"), Is.Null);
        Assert.That(Static("Cooking", "BoilVessel", "potash"), Is.Null, "Potash is not boiled again");
        Assert.That(Static("Cooking", "BoilVessel", "ash"), Is.Null);
        Assert.That(Static("Cooking", "IsLyeBoil", "lye"), Is.EqualTo(true));
        Assert.That(Static("Cooking", "IsLyeBoil", "maple_sap"), Is.EqualTo(false));
        Assert.That(Static("Cooking", "IsSapBoil", "lye"), Is.EqualTo(false));
        Assert.That(Static("Cooking", "IsBoilable", "lye"), Is.EqualTo(false), "Lye is not water");
    }

    [Test]
    public void TheOldCheckBoilVesselStillWorksForThePot()
    {
        // The vessel parameter was added with a default, so the Pot's callers are unchanged.
        System.Reflection.MethodInfo check = Find("Cooking").GetMethod("CheckBoilVessel");
        System.Reflection.ParameterInfo[] parameters = check.GetParameters();
        Assert.That(parameters.Length, Is.EqualTo(3));
        Assert.That(parameters[2].HasDefaultValue, Is.True);
        Assert.That(parameters[2].DefaultValue, Is.EqualTo("cooking_pot"));
    }
}
