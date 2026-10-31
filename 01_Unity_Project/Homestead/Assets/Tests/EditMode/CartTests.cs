using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// EditMode tests for Vehicles_and_Hauling.md, Step 1, Phase 1 (2026-10-30): the Wooden Wheel recipe, the Hand Cart staged site and the finished cart as a
// parked 100 kg load. The rules are pure (CartRules, the Economy assembly); the kinds, the site rows, Accepts, the recipe and the save shape live in the main
// assembly, which a test assembly cannot reference, so they are reached by reflection on throwaway objects, as WoodShedTests does. Nothing here touches the
// Saves folder or a scene, and pushing the cart (Phase 2) is not tested because it is not built.
public class CartTests
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

    static string[] Stacks(object cost)
    {
        Type stack = Find("WoodStack");
        return ((Array)cost).Cast<object>().Select(s => (string)stack.GetField("itemId").GetValue(s) + ":" + (int)stack.GetField("count").GetValue(s)).ToArray();
    }

    // --- What the cart takes (pure) ---

    [TestCase("logs")]
    [TestCase("branches")]
    [TestCase("firewood")]
    [TestCase("stone")]
    [TestCase("clay")]
    [TestCase("tall_grass")]
    public void TheCartCarriesBulkBuildingGoods(string id) => Assert.That(CartRules.Carries(id), Is.True);

    [TestCase("venison")]
    [TestCase("water_good")]
    [TestCase("water_unsafe")]
    [TestCase("knife")]
    [TestCase("hammer")]
    [TestCase("cordage")]
    [TestCase("sticks")]
    [TestCase("wooden_wheel")]
    [TestCase("")]
    [TestCase(null)]
    public void TheCartRefusesFoodWaterToolsAndAnythingElse(string id) => Assert.That(CartRules.Carries(id), Is.False);

    [Test]
    public void TheRefusalNamesTheItemAndWhatTheCartDoesTake()
    {
        string text = CartRules.DoesntHold("Venison");
        Assert.That(text, Does.Contain("Venison"));
        Assert.That(text, Does.Contain("Logs").And.Contain("Stone").And.Contain("Clay").And.Contain("Tall Grass"));
        Assert.That(CartRules.HoldsText, Does.Contain("100 kg").And.Contain("not food, water or tools"));
    }

    // --- The 100 kg weight cap, converted per item (pure) ---

    [TestCase(8f, 12)]       // a Log
    [TestCase(5f, 20)]       // a Stone
    [TestCase(3.5f, 28)]     // a Clay
    [TestCase(1.5f, 66)]     // a Firewood
    [TestCase(1f, 100)]      // a Branch
    [TestCase(0.1f, 1000)]   // a Tall Grass
    public void AnEmptyCartTakesWholeUnitsUpToOneHundredKilograms(float unitKg, int expected) =>
        Assert.That(CartRules.UnitsThatFit(0f, unitKg), Is.EqualTo(expected));

    [Test]
    public void TwelveLogsLeaveRoomForFourBranchesButNotAThirteenthLogOrAStone()
    {
        Assert.That(CartRules.UnitsThatFit(96f, 1f), Is.EqualTo(4));
        Assert.That(CartRules.UnitsThatFit(96f, 8f), Is.EqualTo(0));
        Assert.That(CartRules.UnitsThatFit(96f, 5f), Is.EqualTo(0));
        Assert.That(CartRules.UnitsThatFit(88f, 8f), Is.EqualTo(1), "an exact fit is allowed");
    }

    [Test]
    public void AnExactFitSurvivesFloatError()
    {
        float load = 8f * 4 + 5f * 2 + 3.5f * 3 + 1.5f * 4 + 0.1f * 5; // 59 kg, summed as floats
        Assert.That(CartRules.UnitsThatFit(load, 1f), Is.EqualTo(41));
        Assert.That(CartRules.UnitsThatFit(load, 41f), Is.EqualTo(1), "exactly the room left");
        float tenth = 0.1f * 10 * 3; // 3 kg of Tall Grass, which float addition does not reach exactly
        Assert.That(CartRules.UnitsThatFit(100f - tenth, 1f), Is.EqualTo(3));
    }

    [Test]
    public void AFullOrOverfullCartTakesNothingAndNeverShowsNegativeRoom()
    {
        Assert.That(CartRules.UnitsThatFit(100f, 1f), Is.EqualTo(0));
        Assert.That(CartRules.UnitsThatFit(130f, 0.1f), Is.EqualTo(0), "an over-full cart (an edited save) still refuses");
        Assert.That(CartRules.RoomKg(130f), Is.EqualTo(0f));
        Assert.That(CartRules.IsFull(100f), Is.True);
        Assert.That(CartRules.IsFull(99.9f), Is.False);
        Assert.That(CartRules.RoomKg(-5f), Is.EqualTo(100f), "a negative load counts as empty");
    }

    [Test]
    public void AWeightlessOrUnknownItemIsLimitedByNothingUntilTheCartIsFull()
    {
        Assert.That(CartRules.UnitsThatFit(10f, 0f), Is.EqualTo(int.MaxValue));
        Assert.That(CartRules.UnitsThatFit(100f, 0f), Is.EqualTo(0));
    }

    [Test]
    public void FullCartOfSaysHowManyAnEmptyCartTakes()
    {
        Assert.That(CartRules.FullCartOf(8f), Is.EqualTo(12));
        Assert.That(CartRules.FullCartOf(5f), Is.EqualTo(20));
    }

    [Test]
    public void LoadFractionRunsFromZeroToOneAndClamps()
    {
        Assert.That(CartRules.LoadFraction(0f), Is.EqualTo(0f));
        Assert.That(CartRules.LoadFraction(50f), Is.EqualTo(0.5f).Within(0.0001f));
        Assert.That(CartRules.LoadFraction(100f), Is.EqualTo(1f));
        Assert.That(CartRules.LoadFraction(250f), Is.EqualTo(1f));
        Assert.That(CartRules.LoadFraction(-3f), Is.EqualTo(0f));
    }

    // --- The words on the transfer screen ---

    [Test]
    public void TheKilogramLineReadsXOverOneHundredKg()
    {
        Assert.That(CartRules.KgText(0f), Is.EqualTo("0 / 100 kg"));
        Assert.That(CartRules.KgText(12.5f), Is.EqualTo("12.5 / 100 kg"));
        Assert.That(CartRules.KgText(96f), Is.EqualTo("96 / 100 kg"));
        Assert.That(CartRules.KgText(-4f), Is.EqualTo("0 / 100 kg"));
    }

    [Test]
    public void TheNoRoomReasonSaysFullOrHowMuchRoomIsLeft()
    {
        Assert.That(CartRules.NoRoomReason(100f), Is.EqualTo("the cart is full (100 / 100 kg)"));
        Assert.That(CartRules.NoRoomReason(96f), Is.EqualTo("won't fit - only 4 kg of room left (96 / 100 kg)"));
    }

    // --- The numbers (pure) ---

    [Test]
    public void TheWheelIsOneLogAndOneCordageAndThreeKilograms()
    {
        Assert.That(CartRules.WheelId, Is.EqualTo("wooden_wheel"));
        Assert.That(CartRules.WheelLogs, Is.EqualTo(1));
        Assert.That(CartRules.WheelCordage, Is.EqualTo(1));
        Assert.That(CartRules.WheelWeightKg, Is.EqualTo(3f));
    }

    [Test]
    public void TheCartCostsTwoWheelsThreeLogsSixBranchesThreeCordageAndTwelveBlows()
    {
        Assert.That(CartRules.CartWheels, Is.EqualTo(2));
        Assert.That(CartRules.CartLogs, Is.EqualTo(3));
        Assert.That(CartRules.CartBranches, Is.EqualTo(6));
        Assert.That(CartRules.CartCordage, Is.EqualTo(3));
        Assert.That(CartRules.BuildBlows, Is.EqualTo(12));
        Assert.That(CartRules.CapacityKg, Is.EqualTo(100f));
    }

    [Test]
    public void TheCartIsHeavyEnoughToBeAStagedSite()
    {
        float kg = CartRules.CartWheels * CartRules.WheelWeightKg + CartRules.CartLogs * 8f + CartRules.CartBranches * 1f + CartRules.CartCordage * 0.3f;
        Assert.That(kg, Is.EqualTo(36.9f).Within(0.01f));
        Assert.That(kg, Is.GreaterThan(15f), "over the 15 kg staged-site line");
    }

    [Test]
    public void DismantlingGivesBackHalfOfEachMaterialRoundedDown()
    {
        Assert.That(CartRules.RefundOf(CartRules.CartWheels), Is.EqualTo(1));
        Assert.That(CartRules.RefundOf(CartRules.CartLogs), Is.EqualTo(1));
        Assert.That(CartRules.RefundOf(CartRules.CartBranches), Is.EqualTo(3));
        Assert.That(CartRules.RefundOf(CartRules.CartCordage), Is.EqualTo(1));
        Assert.That(CartRules.RefundOf(1), Is.EqualTo(0));
        Assert.That(CartRules.RefundOf(-2), Is.EqualTo(0));
    }

    [Test]
    public void AParkedCartKeepsLessRoomThanTheTwoMetreBuildingSpacing() =>
        Assert.That(CartRules.SpacingMetres, Is.LessThan(2f));

    // --- The kinds, the site and the finished cart (main assembly, by reflection) ---

    [Test]
    public void TheCartKindsAreAppendedAndOldNumbersAreUnchanged()
    {
        Assert.That((int)Kind("LeachHopperSite"), Is.EqualTo(29), "the last Potash kinds keep their numbers");
        Assert.That((int)Kind("LeachHopper"), Is.EqualTo(30));
        Assert.That((int)Kind("HandCartSite"), Is.EqualTo(31));
        Assert.That((int)Kind("HandCart"), Is.EqualTo(32));
        Assert.That((int)Kind("Felled"), Is.EqualTo(0));
        Assert.That(Enum.GetValues(Find("PileKind")).Length, Is.EqualTo(33), "nothing was inserted in the middle");
    }

    [Test]
    public void TheCartHasItsNames()
    {
        MethodInfo name = Find("WoodManager").GetMethod("PileName");
        Assert.That(name.Invoke(null, new[] { Kind("HandCartSite") }), Is.EqualTo("Hand Cart Site"));
        Assert.That(name.Invoke(null, new[] { Kind("HandCart") }), Is.EqualTo("Hand Cart"));
    }

    [Test]
    public void TheCartSiteIsAStagedSiteThatBecomesTheCartAndNeedsNoDig()
    {
        Type wells = Find("WellsAndOuthouses");
        Assert.That((bool)wells.GetMethod("IsStagedSite").Invoke(null, new[] { Kind("HandCartSite") }), Is.True);
        Assert.That((bool)wells.GetMethod("IsStagedSite").Invoke(null, new[] { Kind("HandCart") }), Is.False, "the finished cart is not a site");
        Assert.That(wells.GetMethod("FinishedKind").Invoke(null, new[] { Kind("HandCartSite") }), Is.EqualTo(Kind("HandCart")));
        Assert.That(wells.GetMethod("FinalKind").Invoke(null, new[] { Kind("HandCartSite") }), Is.EqualTo(Kind("HandCart")));
        Assert.That((bool)wells.GetMethod("RequiresDig").Invoke(null, new[] { Kind("HandCartSite") }), Is.False);
    }

    [Test]
    public void TheSiteIsDrawnByPlacedStructureAndTheFinishedCartByWoodPile()
    {
        object site = NewPile(out Type state, "HandCartSite");
        Assert.That((bool)state.GetProperty("IsPlacedStructure").GetValue(site), Is.True);
        object cart = NewPile(out _, "HandCart");
        Assert.That((bool)state.GetProperty("IsPlacedStructure").GetValue(cart), Is.False);
        Assert.That((bool)state.GetProperty("IsShelter").GetValue(cart), Is.False);
    }

    [Test]
    public void TheCartCostIsTwoWheelsThreeLogsSixBranchesAndThreeCordage()
    {
        Type wells = Find("WellsAndOuthouses");
        string[] want = { "wooden_wheel:2", "logs:3", "branches:6", "cordage:3" };
        Assert.That(Stacks(wells.GetMethod("HandCartCost").Invoke(null, null)), Is.EqualTo(want));
        Assert.That(Stacks(wells.GetMethod("FinishedCost").Invoke(null, new[] { Kind("HandCartSite") })), Is.EqualTo(want), "the site takes what the cart costs");
        Assert.That((bool)wells.GetMethod("NeedsStaging").Invoke(null, new object[] { wells.GetMethod("HandCartCost").Invoke(null, null) }), Is.True,
                    "needs the item assets; the wheel is 3 kg");
    }

    [Test]
    public void TheSiteTakesWheelsLogsBranchesAndCordageInTripsAndStopsAtTheCost()
    {
        Type wells = Find("WellsAndOuthouses");
        object site = NewPile(out Type state, "HandCartSite");
        MethodInfo accepts = wells.GetMethod("SiteAccepts");
        MethodInfo add = state.GetMethod("Add");
        bool Accepts(string id) => (bool)accepts.Invoke(null, new[] { site, id });

        Assert.That(Accepts("wooden_wheel") && Accepts("logs") && Accepts("branches") && Accepts("cordage"), Is.True);
        Assert.That(Accepts("stone") || Accepts("firewood") || Accepts("clay") || Accepts("tall_grass") || Accepts("sticks"), Is.False);

        add.Invoke(site, new object[] { "wooden_wheel", 2, 0 });
        Assert.That(Accepts("wooden_wheel"), Is.False, "two wheels is all it takes");
        add.Invoke(site, new object[] { "logs", 2, 0 });
        Assert.That(Accepts("logs"), Is.True, "one more Log still wanted");
        add.Invoke(site, new object[] { "logs", 1, 0 });
        Assert.That(Accepts("logs"), Is.False);
        Assert.That(wells.GetMethod("SiteNeeds").Invoke(null, new[] { site }), Is.Not.Null, "Branches and Cordage are still missing");

        add.Invoke(site, new object[] { "branches", 6, 0 });
        add.Invoke(site, new object[] { "cordage", 3, 0 });
        Assert.That(wells.GetMethod("SiteNeeds").Invoke(null, new[] { site }), Is.Null);
        Assert.That((bool)wells.GetMethod("SiteComplete").Invoke(null, new[] { site }), Is.True);
    }

    [Test]
    public void TheCartTakesTwelveHammerBlowsAndNeedsNoHammerToPlace()
    {
        Type wells = Find("WellsAndOuthouses");
        var go = new GameObject("test wells");
        try
        {
            Component rules = go.AddComponent(wells);
            Assert.That(wells.GetMethod("BuildBlows").Invoke(rules, new[] { Kind("HandCartSite") }), Is.EqualTo(12));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(go);
        }
        Assert.That((bool)Find("WoodManager").GetMethod("NeedsHammerToBuild").Invoke(null, new[] { Kind("HandCartSite") }), Is.False);
    }

    [Test]
    public void TheCartSiteIsOnTheBuildTabAndTheFinishedCartIsNot()
    {
        var buildable = ((Array)Find("WoodManager").GetField("Buildable").GetValue(null)).Cast<object>().ToArray();
        Assert.That(buildable.Contains(Kind("HandCartSite")), Is.True);
        Assert.That(buildable.Contains(Kind("HandCart")), Is.False, "the finished cart only ever comes from finishing the site");
    }

    [Test]
    public void TheFinishedCartAcceptsOnlyTheBulkGoodsAndTheCartKindIsRecognised()
    {
        object cart = NewPile(out Type state, "HandCart");
        MethodInfo accepts = state.GetMethod("Accepts");
        bool Accepts(string id) => (bool)accepts.Invoke(cart, new object[] { id });
        foreach (string id in new[] { "logs", "branches", "firewood", "stone", "clay", "tall_grass" })
            Assert.That(Accepts(id), Is.True, id);
        foreach (string id in new[] { "venison", "water_good", "knife", "hammer", "cordage", "sticks", "wooden_wheel" })
            Assert.That(Accepts(id), Is.False, id);

        MethodInfo isCart = Find("WoodManager").GetMethod("IsHandCart");
        Assert.That((bool)isCart.Invoke(null, new[] { Kind("HandCart") }), Is.True);
        Assert.That((bool)isCart.Invoke(null, new[] { Kind("HandCartSite") }), Is.True);
        Assert.That((bool)isCart.Invoke(null, new[] { Kind("StorageBin") }), Is.False);
        Assert.That((bool)isCart.Invoke(null, new[] { Kind("LeachHopper") }), Is.False);
    }

    // --- The recipe and the item ---

    [Test]
    public void TheWoodenWheelIsAnInstantCraftOfOneLogAndOneCordage()
    {
        Array recipes = (Array)Find("Crafting").GetField("Recipes").GetValue(null);
        Type recipe = Find("Crafting").GetNestedType("Recipe");
        object wheel = recipes.Cast<object>().Single(r => (string)recipe.GetField("outputId").GetValue(r) == "wooden_wheel");
        Assert.That((int)recipe.GetField("outputCount").GetValue(wheel), Is.EqualTo(1));
        Type ingredient = Find("Crafting").GetNestedType("Ingredient");
        string[] have = ((Array)recipe.GetField("ingredients").GetValue(wheel)).Cast<object>()
                           .Select(i => (string)ingredient.GetField("itemId").GetValue(i) + ":" + (int)ingredient.GetField("quantity").GetValue(i)).ToArray();
        Assert.That(have, Is.EquivalentTo(new[] { "logs:1", "cordage:1" }), "no tool, no other ingredient");
        Assert.That(recipe.GetField("alternatives").GetValue(wheel), Is.Null, "one way to make it");
    }

    [Test]
    public void TheWoodenWheelIsAThreeKilogramMaterialWithNoMarketEntry()
    {
        object item = Find("ItemDatabase").GetMethod("Get").Invoke(null, new object[] { "wooden_wheel" });
        Assert.That(item, Is.Not.Null, "Resources/Items/Materials/wooden_wheel.asset");
        Type def = item.GetType();
        Assert.That((string)def.GetProperty("DisplayName").GetValue(item), Is.EqualTo("Wooden Wheel"));
        Assert.That((float)def.GetProperty("WeightKg").GetValue(item), Is.EqualTo(CartRules.WheelWeightKg));
        Assert.That(def.GetProperty("Category").GetValue(item).ToString(), Is.EqualTo("Material"));
        Assert.That((bool)def.GetProperty("IsPerishable").GetValue(item), Is.False);
    }

    [Test]
    public void TheWoodenWheelSitsInTheMaterialsCategoryOfTheCraftTab()
    {
        MethodInfo category = Find("CraftingScreen").GetMethod("CraftCategory", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.That(category.Invoke(null, new object[] { "wooden_wheel" }), Is.EqualTo("Materials"));
    }

    // --- Saving: the cart is a WoodPileState and nothing else ---

    [Test]
    public void ACartSurvivesASaveAndLoadWithItsPositionFacingAndContents()
    {
        object cart = NewPile(out Type state, "HandCart");
        state.GetField("id").SetValue(cart, 12);
        state.GetField("position").SetValue(cart, new Vector3(18.5f, 2f, -61.25f));
        state.GetField("yaw").SetValue(cart, 135f);
        MethodInfo add = state.GetMethod("Add");
        add.Invoke(cart, new object[] { "logs", 5, 3 });
        add.Invoke(cart, new object[] { "logs", 2, 9 });
        add.Invoke(cart, new object[] { "stone", 7, 0 });

        object back = JsonUtility.FromJson(JsonUtility.ToJson(cart), state);
        Assert.That(state.GetField("kind").GetValue(back), Is.EqualTo(Kind("HandCart")));
        Assert.That((int)state.GetField("id").GetValue(back), Is.EqualTo(12));
        Assert.That((Vector3)state.GetField("position").GetValue(back), Is.EqualTo(new Vector3(18.5f, 2f, -61.25f)));
        Assert.That((float)state.GetField("yaw").GetValue(back), Is.EqualTo(135f));
        MethodInfo count = state.GetMethod("Count");
        Assert.That((int)count.Invoke(back, new object[] { "logs" }), Is.EqualTo(7), "both batches came back");
        Assert.That((int)count.Invoke(back, new object[] { "stone" }), Is.EqualTo(7));
        Assert.That((int)state.GetProperty("Total").GetValue(back), Is.EqualTo((int)state.GetProperty("Total").GetValue(cart)), "nothing lost or duplicated");
    }

    [Test]
    public void AnOldSaveWithNoCartLoadsUnchangedAndTheNewNumbersLoadAsCartKinds()
    {
        Type state = Find("WoodPileState");
        string oldJson = "{\"id\":4,\"kind\":30,\"position\":{\"x\":1,\"y\":2,\"z\":3},\"yaw\":10.0,\"contents\":[{\"itemId\":\"lye\",\"count\":3,\"day\":0}]}";
        object hopper = JsonUtility.FromJson(oldJson, state);
        Assert.That(state.GetField("kind").GetValue(hopper), Is.EqualTo(Kind("LeachHopper")));
        Assert.That((int)state.GetMethod("Count").Invoke(hopper, new object[] { "lye" }), Is.EqualTo(3));

        Assert.That(state.GetField("kind").GetValue(JsonUtility.FromJson("{\"id\":5,\"kind\":31}", state)), Is.EqualTo(Kind("HandCartSite")));
        Assert.That(state.GetField("kind").GetValue(JsonUtility.FromJson("{\"id\":6,\"kind\":32}", state)), Is.EqualTo(Kind("HandCart")));
    }

    [Test]
    public void TheCartAddsNoNewFieldToTheSavedPileState()
    {
        string[] expected =
        {
            "id", "kind", "position", "yaw", "contents", "litres", "contaminatedHours", "digProgress", "builtDay", "buildProgress", "depthToWater", "leachHours",
        };
        string[] fields = Find("WoodPileState").GetFields(BindingFlags.Public | BindingFlags.Instance).Select(f => f.Name).ToArray();
        Assert.That(fields, Is.EquivalentTo(expected), "Phase 1 saves a cart with the fields every pile already has");
    }
}
