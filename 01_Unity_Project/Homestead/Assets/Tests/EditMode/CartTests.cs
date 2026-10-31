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
        Assert.That((string)def.GetProperty("DisplayName").GetValue(item), Is.EqualTo("Wooden Wheels"));
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

    // =================================================================================================================================
    // Phase 2 (2026-10-31): grabbing and pushing the cart. The pure rules are CartRules'; the follower, the sweeps and the controller hooks are scene
    // behaviour (CartHandler) and are covered by the playtest, not here.
    // =================================================================================================================================

    // --- Speed: exact values, because "a full cart is about 1.875 m/s" was once mis-stated for 96 kg ---

    [Test]
    public void ThePushSpeedFactorRunsInAStraightLineFromOneEmptyToThreeQuartersFull()
    {
        Assert.That(CartRules.PushSpeedFactor(0f), Is.EqualTo(1f).Within(1e-5f), "empty");
        Assert.That(CartRules.PushSpeedFactor(0.5f), Is.EqualTo(0.875f).Within(1e-5f), "half");
        Assert.That(CartRules.PushSpeedFactor(0.96f), Is.EqualTo(0.76f).Within(1e-5f), "96 kg");
        Assert.That(CartRules.PushSpeedFactor(1f), Is.EqualTo(0.75f).Within(1e-5f), "full");
        Assert.That(CartRules.PushSpeedFactor(-2f), Is.EqualTo(1f), "clamped");
        Assert.That(CartRules.PushSpeedFactor(3f), Is.EqualTo(0.75f), "clamped");
    }

    [Test]
    public void WalkingPaceOfTwoAndAHalfMetresASecondBecomesTheseExactPushSpeeds()
    {
        const float walk = 2.5f;
        Assert.That(CartRules.PushSpeed(walk, CartRules.LoadFraction(0f)), Is.EqualTo(2.5f).Within(1e-4f), "empty cart: full walking pace");
        Assert.That(CartRules.PushSpeed(walk, CartRules.LoadFraction(50f)), Is.EqualTo(2.1875f).Within(1e-4f), "50 kg");
        Assert.That(CartRules.PushSpeed(walk, CartRules.LoadFraction(96f)), Is.EqualTo(1.9f).Within(1e-4f), "96 kg, 12 Logs: 1.90, not 1.81");
        Assert.That(CartRules.PushSpeed(walk, CartRules.LoadFraction(100f)), Is.EqualTo(1.875f).Within(1e-4f), "100 kg: three quarters");
    }

    [Test]
    public void ALighterLoadIsNeverSlowerThanAHeavierOne()
    {
        float previous = float.MaxValue;
        for (int kg = 0; kg <= 100; kg += 4)
        {
            float speed = CartRules.PushSpeed(2.5f, CartRules.LoadFraction(kg));
            Assert.That(speed, Is.LessThanOrEqualTo(previous), kg + " kg");
            previous = speed;
        }
    }

    [Test]
    public void ThePackSlowingStacksOnTopOfTheCartLoad()
    {
        float packSlowed = 2.5f * 0.8f; // a pack part of the way to encumbered
        Assert.That(CartRules.PushSpeed(packSlowed, 1f), Is.EqualTo(1.5f).Within(1e-4f), "2.0 x 0.75");
    }

    // --- The swing: the cart trails the player's heading at a limited rate ---

    [Test]
    public void TheShortestTurnIsSignedAndStaysInsideHalfACircle()
    {
        Assert.That(CartRules.DeltaAngle(10f, 350f), Is.EqualTo(-20f).Within(1e-4f));
        Assert.That(CartRules.DeltaAngle(350f, 10f), Is.EqualTo(20f).Within(1e-4f));
        Assert.That(CartRules.DeltaAngle(90f, 90f), Is.EqualTo(0f));
        Assert.That(CartRules.DeltaAngle(0f, 270f), Is.EqualTo(-90f).Within(1e-4f));
        Assert.That(CartRules.DeltaAngle(0f, 540f), Is.EqualTo(180f).Within(1e-4f));
    }

    [Test]
    public void TheCartTurnsNoFasterThanNinetyDegreesASecondTheShortWayRound()
    {
        Assert.That(CartRules.TurnDegreesPerSecond, Is.EqualTo(90f));
        Assert.That(CartRules.NextYaw(0f, 90f, 0.5f), Is.EqualTo(45f).Within(1e-4f));
        Assert.That(CartRules.NextYaw(0f, 90f, 1f), Is.EqualTo(90f).Within(1e-4f));
        Assert.That(CartRules.NextYaw(0f, 90f, 5f), Is.EqualTo(90f).Within(1e-4f), "never overshoots");
        Assert.That(CartRules.NextYaw(350f, 10f, 0.1f), Is.EqualTo(359f).Within(1e-3f), "across north, the short way");
        Assert.That(CartRules.NextYaw(5f, 350f, 0.2f), Is.EqualTo(350f).Within(1e-3f), "turning left through zero wraps to 350");
        Assert.That(CartRules.NextYaw(40f, 200f, 0f), Is.EqualTo(40f), "no time, no turn");
        Assert.That(CartRules.NextYaw(40f, 200f, -1f), Is.EqualTo(40f), "negative time counts as none");
    }

    [Test]
    public void TheCartYawStaysBetweenZeroAndThreeSixty()
    {
        for (float from = 0f; from < 360f; from += 37f)
        for (float to = 0f; to < 360f; to += 53f)
        {
            float next = CartRules.NextYaw(from, to, 0.3f);
            Assert.That(next, Is.GreaterThanOrEqualTo(0f).And.LessThan(360f), $"{from} toward {to}");
        }
    }

    [Test]
    public void TheCartCentreIsOnePointSevenMetresAheadOfThePlayerAlongItsFacing()
    {
        Assert.That(CartRules.FollowMetres, Is.EqualTo(1.7f));
        CartRules.CartCentreAhead(10f, 20f, 0f, out float x, out float z);
        Assert.That(x, Is.EqualTo(10f).Within(1e-4f));
        Assert.That(z, Is.EqualTo(21.7f).Within(1e-4f));
        CartRules.CartCentreAhead(10f, 20f, 90f, out x, out z);
        Assert.That(x, Is.EqualTo(11.7f).Within(1e-4f));
        Assert.That(z, Is.EqualTo(20f).Within(1e-4f));
        CartRules.CartCentreAhead(10f, 20f, 180f, out x, out z);
        Assert.That(z, Is.EqualTo(18.3f).Within(1e-4f));
        CartRules.CartCentreAhead(0f, 0f, 0f, out x, out z, 1.3f);
        Assert.That(z, Is.EqualTo(1.3f).Within(1e-4f), "the distance is a parameter");
    }

    [Test]
    public void TheFootprintHasFifteenDistinctPointsThatTurnWithTheCart()
    {
        var xs = new float[CartRules.SampleCount];
        var zs = new float[CartRules.SampleCount];
        Assert.That(CartRules.SampleCount, Is.EqualTo(15));

        Assert.That(CartRules.FootprintSamples(0f, 0f, 0f, xs, zs), Is.EqualTo(15));
        Assert.That(xs.Min(), Is.EqualTo(-0.6f).Within(1e-4f));
        Assert.That(xs.Max(), Is.EqualTo(0.6f).Within(1e-4f));
        Assert.That(zs.Min(), Is.EqualTo(-1.1f).Within(1e-4f), "the handles' end, behind the centre");
        Assert.That(zs.Max(), Is.EqualTo(0.85f).Within(1e-4f), "the front board");
        Assert.That(Enumerable.Range(0, 15).Select(i => (xs[i], zs[i])).Distinct().Count(), Is.EqualTo(15));

        Assert.That(CartRules.FootprintSamples(5f, -3f, 90f, xs, zs), Is.EqualTo(15));
        Assert.That(xs.Max(), Is.EqualTo(5f + 0.85f).Within(1e-4f), "facing +X, the front is at +X");
        Assert.That(xs.Min(), Is.EqualTo(5f - 1.1f).Within(1e-4f), "and the handles trail toward -X");
        Assert.That(zs.Min(), Is.EqualTo(-3f - 0.6f).Within(1e-4f));
        Assert.That(zs.Max(), Is.EqualTo(-3f + 0.6f).Within(1e-4f));
    }

    [Test]
    public void TheGrabReachIsTwoPointTwoMetres()
    {
        Assert.That(CartRules.CanReach(0f), Is.True);
        Assert.That(CartRules.CanReach(2.2f), Is.True);
        Assert.That(CartRules.CanReach(2.3f), Is.False);
        Assert.That(CartRules.CanReach(-1f), Is.False);
    }

    // --- Slope: only uphill, and a load lowers the limit ---

    [Test]
    public void TheSlopeLimitIsTwentyFiveDegreesEmptyAndEighteenFull()
    {
        Assert.That(CartRules.MaxPushSlopeDegrees(0f), Is.EqualTo(25f).Within(1e-4f));
        Assert.That(CartRules.MaxPushSlopeDegrees(1f), Is.EqualTo(18f).Within(1e-4f));
        Assert.That(CartRules.MaxPushSlopeDegrees(0.5f), Is.EqualTo(21.5f).Within(1e-4f));
        Assert.That(CartRules.MaxPushSlopeDegrees(9f), Is.EqualTo(18f), "clamped");
    }

    [Test]
    public void ALoadedCartStopsOnAThirtyPercentGradeThatAnEmptyOneCanStillClimb()
    {
        // 0.6 m up over 1.8 m is 18.4 degrees; 0.8 m up over 1.8 m is 24.0 degrees; 1.0 m up over 1.8 m is 29.1 degrees.
        Assert.That(CartRules.SlopeBlocksPush(0.6f, 1.8f, 1f), Is.True, "loaded, 18.4 degrees");
        Assert.That(CartRules.SlopeBlocksPush(0.6f, 1.8f, 0f), Is.False, "empty, 18.4 degrees");
        Assert.That(CartRules.SlopeBlocksPush(0.8f, 1.8f, 0f), Is.False, "empty, 24.0 degrees");
        Assert.That(CartRules.SlopeBlocksPush(1.0f, 1.8f, 0f), Is.True, "empty, 29.1 degrees");
        Assert.That(CartRules.SlopeBlocksPush(0.4f, 1.8f, 1f), Is.False, "loaded, 12.5 degrees");
    }

    [Test]
    public void DownhillLevelAndNoRunNeverBlock()
    {
        Assert.That(CartRules.SlopeBlocksPush(-5f, 1.8f, 1f), Is.False, "any downhill, so a cart can always be backed down");
        Assert.That(CartRules.SlopeBlocksPush(0f, 1.8f, 1f), Is.False);
        Assert.That(CartRules.SlopeBlocksPush(2f, 0f, 1f), Is.False, "no run is no direction");
    }

    // --- Water ---

    [Test]
    public void WaterDeeperThanAnAnkleStopsTheCart()
    {
        Assert.That(CartRules.WaterLimitMetres, Is.EqualTo(0.25f));
        Assert.That(CartRules.WaterBlocksPush(0f), Is.False);
        Assert.That(CartRules.WaterBlocksPush(0.25f), Is.False, "exactly the limit is allowed");
        Assert.That(CartRules.WaterBlocksPush(0.26f), Is.True);
    }

    [Test]
    public void ACartAlreadyAtTheWatersEdgeCanAlwaysBeBackedOut()
    {
        Assert.That(CartRules.WaterStepBlocked(0.1f, 0.4f), Is.True, "a step into deeper water");
        Assert.That(CartRules.WaterStepBlocked(0.1f, 0.2f), Is.False, "still shallow");
        Assert.That(CartRules.WaterStepBlocked(0.4f, 0.4f), Is.False, "no deeper than now: not refused");
        Assert.That(CartRules.WaterStepBlocked(0.4f, 0.1f), Is.False, "backing out");
        Assert.That(CartRules.WaterStepBlocked(0.4f, 0.5f), Is.True, "deeper still, in too deep already");
    }

    // --- The stuck message waits six tenths of a second and only counts a player who is trying to move ---

    [Test]
    public void TheStuckMessageNeedsSixTenthsOfASecondOfTryingToMove()
    {
        Assert.That(CartRules.StuckMessageSeconds, Is.EqualTo(0.6f));
        float t = 0f;
        t = CartRules.StuckTime(t, true, false, 0.3f);
        Assert.That(CartRules.ShowStuckMessage(t), Is.False, "0.3 s");
        t = CartRules.StuckTime(t, true, false, 0.29f);
        Assert.That(CartRules.ShowStuckMessage(t), Is.False, "0.59 s");
        t = CartRules.StuckTime(t, true, false, 0.02f);
        Assert.That(CartRules.ShowStuckMessage(t), Is.True, "0.61 s");
    }

    [Test]
    public void MovingOrNotTryingToMoveResetsTheStuckTimer()
    {
        Assert.That(CartRules.StuckTime(0.5f, true, true, 0.1f), Is.EqualTo(0f), "free to move");
        Assert.That(CartRules.StuckTime(0.5f, false, false, 0.1f), Is.EqualTo(0f), "standing still on purpose");
        Assert.That(CartRules.StuckTime(0.5f, true, false, -1f), Is.EqualTo(0.5f), "negative time adds nothing");
        Assert.That(CartRules.ShowStuckMessage(-2f), Is.False, "the cool-down after a message");
    }

    // --- The words ---

    [Test]
    public void ThePromptsReadExactlyAsAgreedAndNeverCarryAKeyInBrackets()
    {
        Assert.That(CartRules.GrabPrompt(62f), Is.EqualTo("Push Hand Cart  (62 / 100 kg)"));
        Assert.That(CartRules.PushingLine(62f), Is.EqualTo("Pushing the Hand Cart  (62 / 100 kg)  -  E to park"));
        Assert.That(CartRules.GrabPrompt(0f), Is.EqualTo("Push Hand Cart  (0 / 100 kg)"));
        foreach (string text in new[]
        {
            CartRules.GrabPrompt(10f), CartRules.PushingLine(10f), CartRules.ParkedText, CartRules.LetGoText, CartRules.NoRoomToGrabText,
            CartRules.StandUpText, CartRules.StuckText, CartRules.TooSteepText, CartRules.TooDeepText, CartRules.FloodText,
        })
        {
            Assert.That(text, Does.Not.Contain("[").And.Not.Contain("]"), "the prompt HUD adds the key itself: " + text);
        }
    }

    [Test]
    public void TheFloodWarningNamesTheFloodAndSaysWhatToDo() =>
        Assert.That(CartRules.FloodText, Does.Contain("flood zone").And.Contain("Park it higher up"));

    // --- Nothing about holding the cart is saved, and no number that is saved has moved ---

    [Test]
    public void WhetherTheCartIsHeldIsNotSavedAnywhere()
    {
        Type saveable = Find("ISaveable");
        Assert.That(saveable.IsAssignableFrom(Find("CartHandler")), Is.False, "CartHandler is not a saved system");
        Assert.That(saveable.IsAssignableFrom(Find("CartHandle")), Is.False);
        Assert.That(Find("CartHandler").GetCustomAttributes(typeof(SerializableAttribute), false), Is.Empty, "and not a serialized class");

        string[] world = Find("WoodSaveData").GetFields(BindingFlags.Public | BindingFlags.Instance).Select(f => f.Name).ToArray();
        Assert.That(world, Is.EquivalentTo(new[] { "felled", "piles", "nextPileId", "snowLoadHours", "stormAfterglow" }),
                    "the world file's wood section has no grab flag");
    }

    [Test]
    public void NoHandlerMeansNothingIsHeld()
    {
        Type handler = Find("CartHandler");
        Assert.That((bool)handler.GetProperty("Pushing").GetValue(null), Is.False);
        Assert.That((float)handler.GetProperty("LoadFraction").GetValue(null), Is.EqualTo(0f));
    }

    [Test]
    public void LedgerCategoriesAndTheLedgerVersionAreUntouched()
    {
        Assert.That(LedgerCategories.Count, Is.EqualTo(19));
        Assert.That(LedgerCategories.LastIncomeId, Is.EqualTo(7));
        Assert.That((int)LedgerCategory.BuyOtherGoods, Is.EqualTo(18));
        Assert.That(Find("LedgerSaveData").GetField("CurrentVersion").GetRawConstantValue(), Is.EqualTo(1));
    }

    [Test]
    public void PileKindNumbersAreUntouchedByPhaseTwo()
    {
        Assert.That((int)Kind("LeachHopper"), Is.EqualTo(30));
        Assert.That((int)Kind("HandCartSite"), Is.EqualTo(31));
        Assert.That((int)Kind("HandCart"), Is.EqualTo(32));
        Assert.That(Enum.GetValues(Find("PileKind")).Length, Is.EqualTo(33), "no kind was added for the cart handle or the held cart");
    }
}
