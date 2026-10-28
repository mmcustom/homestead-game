using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

// EditMode tests for the Tripod (commit T), the parts that live in the main assembly and are reached by reflection on throwaway objects as the
// other tests do: the saved fire state (additive), the craft recipe, the item, the new-game kits and the boil vessel.
public class TripodWorldTests
{
    static Type Find(string name) =>
        AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => { try { return a.GetTypes(); } catch (Exception) { return new Type[0]; } })
                 .First(t => t.Name == name);

    static object Item(string id) => Find("ItemDatabase").GetMethod("Get").Invoke(null, new object[] { id });

    static object Prop(object item, string name) => item.GetType().GetProperty(name).GetValue(item);

    // --- The saved fire ---

    [Test]
    public void AFireHasAnAdditiveTripodFlagAndVesselName()
    {
        Type fire = Find("CampfireState");
        Assert.That(fire.GetField("tripod").FieldType, Is.EqualTo(typeof(bool)));
        Assert.That(fire.GetField("vessel").FieldType, Is.EqualTo(typeof(string)));

        object fresh = Activator.CreateInstance(fire);
        Assert.That(fire.GetField("tripod").GetValue(fresh), Is.EqualTo(false));
        Assert.That(fire.GetField("vessel").GetValue(fresh), Is.Null.Or.Empty);
    }

    [Test]
    public void AFireSavedBeforeTheTripodLoadsWithoutOne()
    {
        Type fire = Find("CampfireState");
        object old = JsonUtility.FromJson("{\"id\":4,\"fuelHours\":2.5,\"lit\":true,\"ashMade\":1.5,\"builtWithSticks\":true}", fire);
        Assert.That(fire.GetField("id").GetValue(old), Is.EqualTo(4));
        Assert.That(fire.GetField("lit").GetValue(old), Is.EqualTo(true));
        Assert.That(fire.GetField("tripod").GetValue(old), Is.EqualTo(false), "no tripod in an old save");
        Assert.That(fire.GetField("vessel").GetValue(old), Is.Null.Or.Empty, "and nothing hung");
    }

    [Test]
    public void ATripodAndItsVesselSurviveASaveAndALoad()
    {
        Type fire = Find("CampfireState"), save = Find("FireSaveData");
        object state = Activator.CreateInstance(fire);
        fire.GetField("id").SetValue(state, 7);
        fire.GetField("tripod").SetValue(state, true);
        fire.GetField("vessel").SetValue(state, "cooking_pot");

        object data = Activator.CreateInstance(save);
        var list = (System.Collections.IList)Activator.CreateInstance(typeof(System.Collections.Generic.List<>).MakeGenericType(fire));
        list.Add(state);
        save.GetField("fires").SetValue(data, list);
        save.GetField("nextId").SetValue(data, 8);

        string json = JsonUtility.ToJson(data);
        object back = JsonUtility.FromJson(json, save);
        var backList = (System.Collections.IList)save.GetField("fires").GetValue(back);
        Assert.That(backList.Count, Is.EqualTo(1));
        Assert.That(fire.GetField("tripod").GetValue(backList[0]), Is.EqualTo(true));
        Assert.That(fire.GetField("vessel").GetValue(backList[0]), Is.EqualTo("cooking_pot"));
    }

    // --- The craft recipe and the item ---

    [Test]
    public void TheTripodIsCraftedFromThreeBranchesAndOneCordage()
    {
        Type crafting = Find("Crafting");
        var recipes = (Array)crafting.GetField("Recipes").GetValue(null);
        object tripod = recipes.Cast<object>().Single(r => (string)r.GetType().GetField("outputId").GetValue(r) == "tripod");
        Assert.That(tripod.GetType().GetField("outputCount").GetValue(tripod), Is.EqualTo(1));

        var ingredients = ((Array)tripod.GetType().GetField("ingredients").GetValue(tripod)).Cast<object>()
            .Select(i => ((string)i.GetType().GetField("itemId").GetValue(i), (int)i.GetType().GetField("quantity").GetValue(i))).ToArray();
        Assert.That(ingredients, Is.EquivalentTo(new[] { ("branches", 3), ("cordage", 1) }));
    }

    [Test]
    public void TheTripodItemIsAThreeKiloMaterialThatIsNotFood()
    {
        object tripod = Item(TripodRules.TripodId);
        Assert.That(tripod, Is.Not.Null);
        Assert.That(Prop(tripod, "Id"), Is.EqualTo("tripod"));
        Assert.That(Prop(tripod, "DisplayName"), Is.EqualTo("Tripod"));
        Assert.That(Prop(tripod, "Category").ToString(), Is.EqualTo("Material"));
        Assert.That(Prop(tripod, "WeightKg"), Is.EqualTo(3f));
        Assert.That(Prop(tripod, "IsFood"), Is.EqualTo(false));
        Assert.That(Prop(tripod, "IsPerishable"), Is.EqualTo(false));
    }

    [Test]
    public void TheTripodHasNoMarketRow()
    {
        Assert.That(MarketCatalog.Table.TryGetRow("tripod", out _), Is.False, "it cannot be bought or sold");
    }

    // --- The new-game kits ---

    static (string item, int count)[] Kit(string tier)
    {
        Type difficulty = Find("Difficulty");
        object value = Enum.Parse(difficulty, tier);
        var kit = (Array)Find("DifficultyManager").GetMethod("Kit").Invoke(null, new[] { value });
        return kit.Cast<object>().Select(t => ((string)t.GetType().GetField("Item1").GetValue(t), (int)t.GetType().GetField("Item2").GetValue(t))).ToArray();
    }

    [TestCase("Homesteader")]
    [TestCase("Settler")]
    public void TheHomesteaderAndSettlerStartWithOneTripodAndTheirPot(string tier)
    {
        (string item, int count)[] kit = Kit(tier);
        Assert.That(kit.Single(k => k.item == "tripod").count, Is.EqualTo(1));
        Assert.That(kit.Single(k => k.item == "cooking_pot").count, Is.EqualTo(1), "the Pot is still in the kit; it is hung at the first fire");
    }

    [Test]
    public void ThePioneerStartsWithNeitherATripodNorAPot()
    {
        (string item, int count)[] kit = Kit("Pioneer");
        Assert.That(kit.Any(k => k.item == "tripod"), Is.False);
        Assert.That(kit.Any(k => k.item == "cooking_pot"), Is.False);
    }

    [Test]
    public void TheHomesteadersHammerAndTarpAreStillThere()
    {
        (string item, int count)[] kit = Kit("Homesteader");
        Assert.That(kit.Any(k => k.item == "hammer"), Is.True);
        Assert.That(kit.Any(k => k.item == "tarp"), Is.True);
        Assert.That(Kit("Settler").Any(k => k.item == "hammer"), Is.False, "the Settler still has no Hammer");
    }

    // --- The boil vessel ---

    [Test]
    public void WaterAndSapBoilInTheCookingPotAndNothingElseBoils()
    {
        Type cooking = Find("Cooking");
        var boilVessel = cooking.GetMethod("BoilVessel");
        Assert.That(boilVessel.Invoke(null, new object[] { "maple_sap" }), Is.EqualTo("cooking_pot"));
        Assert.That(boilVessel.Invoke(null, new object[] { "water_good" }), Is.EqualTo("cooking_pot"));
        Assert.That(boilVessel.Invoke(null, new object[] { "water_unsafe" }), Is.EqualTo("cooking_pot"));
        Assert.That(boilVessel.Invoke(null, new object[] { "venison" }), Is.Null, "meat and fish are cooked on the open fire");
        Assert.That(boilVessel.Invoke(null, new object[] { "bluegill" }), Is.Null);
        Assert.That(cooking.GetField("PotId").GetRawConstantValue(), Is.EqualTo(TripodRules.PotId));
    }
}
