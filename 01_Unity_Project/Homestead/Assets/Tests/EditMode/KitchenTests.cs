using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// EditMode tests for Structures Slice 1, phase C: the Covered Outdoor Kitchen (a two-stage staged site). The kitchen code lives in the main
// assembly, which a test assembly cannot reference, so it is reached by reflection on throwaway objects, as WoodShedTests does; the
// placement rule and the roof's footprint are in the pure Setbacks assembly. Nothing here touches the Saves folder or a scene.
public class KitchenTests
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

    static string[] Lines(object cost)
    {
        Type stack = Find("WoodStack");
        return ((Array)cost).Cast<object>().Select(s => (string)stack.GetField("itemId").GetValue(s) + ":" + (int)stack.GetField("count").GetValue(s)).ToArray();
    }

    // --- The kinds and the two stages ---

    [Test]
    public void KitchenKindsAreAppendedAndOldNumbersAreUnchanged()
    {
        Assert.That((int)Kind("WoodShed"), Is.EqualTo(21), "the last phase B kind keeps its number");
        Assert.That((int)Kind("KitchenSite"), Is.EqualTo(22));
        Assert.That((int)Kind("KitchenRoofSite"), Is.EqualTo(23));
        Assert.That((int)Kind("Kitchen"), Is.EqualTo(24));
        Assert.That((int)Kind("Felled"), Is.EqualTo(0));
        Assert.That((int)Kind("WoodShedSite"), Is.EqualTo(20));
    }

    [Test]
    public void TheKitchenIsTwoStagedSitesThatEndInTheFinishedKitchen()
    {
        Type wells = Find("WellsAndOuthouses");
        MethodInfo staged = wells.GetMethod("IsStagedSite");
        Assert.That((bool)staged.Invoke(null, new[] { Kind("KitchenSite") }), Is.True);
        Assert.That((bool)staged.Invoke(null, new[] { Kind("KitchenRoofSite") }), Is.True);
        Assert.That((bool)staged.Invoke(null, new[] { Kind("Kitchen") }), Is.False, "the finished kitchen is not a site");
        Assert.That(wells.GetMethod("FinishedKind").Invoke(null, new[] { Kind("KitchenSite") }), Is.EqualTo(Kind("KitchenRoofSite")));
        Assert.That(wells.GetMethod("FinishedKind").Invoke(null, new[] { Kind("KitchenRoofSite") }), Is.EqualTo(Kind("Kitchen")));
        Assert.That(wells.GetMethod("FinalKind").Invoke(null, new[] { Kind("KitchenSite") }), Is.EqualTo(Kind("Kitchen")), "the ghost draws the finished kitchen");
        Assert.That(wells.GetMethod("FinalKind").Invoke(null, new[] { Kind("OuthouseSite") }), Is.EqualTo(Kind("Outhouse")), "single-stage sites are unchanged");
        Assert.That((bool)wells.GetMethod("RequiresDig").Invoke(null, new[] { Kind("KitchenSite") }), Is.False);
        Assert.That((bool)wells.GetMethod("RequiresDig").Invoke(null, new[] { Kind("KitchenRoofSite") }), Is.False);
    }

    [Test]
    public void HearthStageIsFourStoneAndTwoClayAndRoofStageIsLogsBranchesAndTallGrassEachOverTheCutoffAndUnderEncumbered()
    {
        Type wells = Find("WellsAndOuthouses");
        object hearth = wells.GetMethod("FinishedCost").Invoke(null, new[] { Kind("KitchenSite") });
        object roof = wells.GetMethod("FinishedCost").Invoke(null, new[] { Kind("KitchenRoofSite") });
        Assert.That(Lines(hearth), Is.EquivalentTo(new[] { "stone:4", "clay:2" }));
        Assert.That(Lines(roof), Is.EquivalentTo(new[] { "logs:2", "branches:6", "tall_grass:4" }));

        MethodInfo weight = wells.GetMethod("WeightKg");
        Assert.That((float)weight.Invoke(null, new[] { hearth }), Is.EqualTo(27f).Within(0.01f), "4 x 5 + 2 x 3.5");
        Assert.That((float)weight.Invoke(null, new[] { roof }), Is.EqualTo(22.4f).Within(0.01f), "2 x 8 + 6 x 1 + 4 x 0.1");
        foreach (object stage in new[] { hearth, roof })
        {
            Assert.That((bool)wells.GetMethod("NeedsStaging").Invoke(null, new[] { stage }), Is.True, "over the 15 kg cutoff, so a staged site");
            Assert.That((float)weight.Invoke(null, new[] { stage }), Is.LessThan(30f), "a stage fits under the Encumbered line");
        }
    }

    [Test]
    public void HearthMaterialsGoInStoneFirstThenClayAndTheRoofTakesItsThreeInAnyOrder()
    {
        Type wells = Find("WellsAndOuthouses");
        object site = NewPile(out Type state, "KitchenSite");
        MethodInfo accepts = wells.GetMethod("SiteAccepts");
        MethodInfo add = state.GetMethod("Add");
        bool Accepts(object s, string id) => (bool)accepts.Invoke(null, new[] { s, id });

        Assert.That(Accepts(site, "stone"), Is.True);
        Assert.That(Accepts(site, "clay"), Is.False, "Stone first");
        Assert.That(Accepts(site, "logs") || Accepts(site, "branches") || Accepts(site, "tall_grass"), Is.False, "the roof timber waits for the roof stage");
        add.Invoke(site, new object[] { "stone", 3, 0 });
        Assert.That(Accepts(site, "clay"), Is.False, "three of four Stone is not enough");
        add.Invoke(site, new object[] { "stone", 1, 0 });
        Assert.That(Accepts(site, "stone"), Is.False, "full");
        Assert.That(Accepts(site, "clay"), Is.True);
        Assert.That(wells.GetMethod("SiteNeeds").Invoke(null, new[] { site }), Is.EqualTo("2 Clay"));
        add.Invoke(site, new object[] { "clay", 2, 0 });
        Assert.That((bool)wells.GetMethod("SiteComplete").Invoke(null, new[] { site }), Is.True);

        object roof = NewPile(out _, "KitchenRoofSite");
        Assert.That(Accepts(roof, "logs") && Accepts(roof, "branches") && Accepts(roof, "tall_grass"), Is.True);
        Assert.That(Accepts(roof, "stone") || Accepts(roof, "clay"), Is.False, "the hearth is already built");
        add.Invoke(roof, new object[] { "tall_grass", 4, 0 });
        add.Invoke(roof, new object[] { "logs", 2, 3 });
        Assert.That(Accepts(roof, "tall_grass") || Accepts(roof, "logs"), Is.False);
        Assert.That(Accepts(roof, "branches"), Is.True);
        add.Invoke(roof, new object[] { "branches", 6, 3 });
        Assert.That(wells.GetMethod("SiteNeeds").Invoke(null, new[] { roof }), Is.Null);
    }

    [Test]
    public void HearthTakesFifteenBlowsAndRoofThirtyAndNeitherNeedsTheHammerToPlace()
    {
        Type wells = Find("WellsAndOuthouses");
        var go = new GameObject("test wells");
        try
        {
            Component rules = go.AddComponent(wells);
            Assert.That(wells.GetMethod("BuildBlows").Invoke(rules, new[] { Kind("KitchenSite") }), Is.EqualTo(15));
            Assert.That(wells.GetMethod("BuildBlows").Invoke(rules, new[] { Kind("KitchenRoofSite") }), Is.EqualTo(30));
            Assert.That(wells.GetField("kitchenHearthBuildBlows").GetValue(rules), Is.EqualTo(15));
            Assert.That(wells.GetField("kitchenRoofBuildBlows").GetValue(rules), Is.EqualTo(30));
            Assert.That(wells.GetField("shelteredMealRecoveryBonus").GetValue(rules), Is.EqualTo(0.25f));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(go);
        }
        MethodInfo needsHammer = Find("WoodManager").GetMethod("NeedsHammerToBuild");
        Assert.That((bool)needsHammer.Invoke(null, new[] { Kind("KitchenSite") }), Is.False, "placing and stocking are free; only pounding needs the Hammer");
    }

    [Test]
    public void ABlowsMaterialsLockInAndDismantleHalvesEachLineOfBothStagesRoundedDown()
    {
        Type wells = Find("WellsAndOuthouses");
        object site = NewPile(out Type state, "KitchenSite");
        Assert.That((bool)wells.GetMethod("MaterialsLocked").Invoke(null, new[] { site }), Is.False, "free to take back until the first blow");
        state.GetField("buildProgress").SetValue(site, 0.07f);
        Assert.That((bool)wells.GetMethod("MaterialsLocked").Invoke(null, new[] { site }), Is.True);

        object total = wells.GetMethod("KitchenCost").Invoke(null, null);
        Assert.That(Lines(total), Is.EqualTo(new[] { "stone:4", "clay:2", "logs:2", "branches:6", "tall_grass:4" }));
        var halves = ((Array)total).Cast<object>().Select(s => (int)Find("WoodStack").GetField("count").GetValue(s) / 2).ToArray();
        Assert.That(halves, Is.EqualTo(new[] { 2, 1, 1, 3, 2 }));
        Assert.That(7 / 2, Is.EqualTo(3), "rounded down");
        Assert.That(1 / 2, Is.EqualTo(0), "a single item refunds nothing");
    }

    [Test]
    public void KitchenSitesAndTheFinishedKitchenAreDrawnByPlacedStructure()
    {
        Type state = Find("WoodPileState");
        foreach (string kind in new[] { "KitchenSite", "KitchenRoofSite", "Kitchen" })
        {
            object pile = NewPile(out _, kind);
            Assert.That((bool)state.GetProperty("IsPlacedStructure").GetValue(pile), Is.True, kind);
            Assert.That((bool)state.GetProperty("IsKitchen").GetValue(pile), Is.True, kind);
            Assert.That((bool)state.GetProperty("IsShelter").GetValue(pile), Is.False, kind + ": the Tent, Lean-To and Cabin path is not the kitchen's");
        }
        object kitchen = NewPile(out _, "Kitchen");
        Assert.That((bool)state.GetMethod("Accepts").Invoke(kitchen, new object[] { "logs" }), Is.False, "nothing is stored in the finished kitchen");
        Assert.That((bool)state.GetProperty("IsKitchen").GetValue(NewPile(out _, "WoodShed")), Is.False);
    }

    // --- Placement ---

    [Test]
    public void KitchenKeepsThreeMetresOfClearAirFromStructuresMeasuredFromItsRoofEdge()
    {
        SetbackRules rules = SetbackRules.Defaults;
        Assert.That(rules.kitchenFromStructure, Is.EqualTo(3f));
        Assert.That(rules.kitchenFromTree, Is.EqualTo(3f));
        var cabin = new List<SiteInfo> { new SiteInfo { kind = SiteKind.Building, position = Vector3.zero, name = "Small Cabin", radius = 2.5f } };
        float needed = rules.kitchenRadius + 2.5f + 3f;
        Assert.That(Setbacks.CanPlaceKitchen(new Vector3(needed - 0.1f, 0f, 0f), cabin, false, rules, out string reason), Is.False);
        Assert.That(reason, Is.EqualTo("Too close to the Small Cabin: 3 m needed."));
        Assert.That(Setbacks.CanPlaceKitchen(new Vector3(needed + 0.1f, 0f, 0f), cabin, false, rules, out _), Is.True);

        var shed = new List<SiteInfo> { new SiteInfo { kind = SiteKind.Building, position = Vector3.zero, name = "Wood Shed", radius = 1.2f } };
        Assert.That(Setbacks.CanPlaceKitchen(new Vector3(rules.kitchenRadius + 1.2f + 2.9f, 0f, 0f), shed, false, rules, out reason), Is.False);
        Assert.That(reason, Is.EqualTo("Too close to the Wood Shed: 3 m needed."));
    }

    [Test]
    public void KitchenRefusesATreeWithinThreeMetresOfItsRoofEdgeAndSaysSo()
    {
        var none = new List<SiteInfo>();
        Assert.That(Setbacks.CanPlaceKitchen(Vector3.zero, none, true, SetbackRules.Defaults, out string reason), Is.False);
        Assert.That(reason, Is.EqualTo("Too close to a tree: 3 m needed."));
        Assert.That(Setbacks.CanPlaceKitchen(Vector3.zero, none, false, SetbackRules.Defaults, out reason), Is.True);
        Assert.That(reason, Is.Empty);
    }

    [Test]
    public void RoofFootprintDecidesWhoIsStandingUnderItWhateverWayTheKitchenFaces()
    {
        Vector3 pivot = new Vector3(10f, 2f, -4f);
        Assert.That(Setbacks.KitchenLayout.UnderRoof(pivot, 0f, pivot), Is.True);
        Assert.That(Setbacks.KitchenLayout.UnderRoof(pivot, 0f, pivot + new Vector3(1.6f, 0f, 1.4f)), Is.True, "near a corner");
        Assert.That(Setbacks.KitchenLayout.UnderRoof(pivot, 0f, pivot + new Vector3(1.8f, 0f, 0f)), Is.False, "past the side");
        Assert.That(Setbacks.KitchenLayout.UnderRoof(pivot, 0f, pivot + new Vector3(0f, 0f, 1.6f)), Is.False, "past the front");
        // Turned a quarter: the roof is wider along world Z than along world X.
        Assert.That(Setbacks.KitchenLayout.UnderRoof(pivot, 90f, pivot + new Vector3(0f, 0f, 1.6f)), Is.True);
        Assert.That(Setbacks.KitchenLayout.UnderRoof(pivot, 90f, pivot + new Vector3(1.6f, 0f, 0f)), Is.False);
        // The hearth stands under its own roof, whichever way it faces.
        foreach (float yaw in new[] { 0f, 37f, 90f, 200f })
            Assert.That(Setbacks.KitchenLayout.UnderRoof(pivot, yaw, Setbacks.KitchenLayout.HearthAt(pivot, yaw)), Is.True, "yaw " + yaw);
        Assert.That(Setbacks.KitchenLayout.RoofHeight, Is.GreaterThan(2.2f), "the roof is well clear of the fire and of a standing player");
    }

    // --- Saves ---

    [Test]
    public void EachKitchenStageRoundTripsThroughASaveWithItsContentsFacingAndProgress()
    {
        Type state = Find("WoodPileState");
        foreach (string kind in new[] { "KitchenSite", "KitchenRoofSite", "Kitchen" })
        {
            object pile = NewPile(out _, kind);
            if (kind != "Kitchen")
                state.GetMethod("Add").Invoke(pile, new object[] { kind == "KitchenSite" ? "stone" : "logs", 2, 0 });
            state.GetField("yaw").SetValue(pile, 72f);
            state.GetField("buildProgress").SetValue(pile, 0.3f);
            state.GetField("position").SetValue(pile, new Vector3(5f, 1f, 9f));
            object back = JsonUtility.FromJson(JsonUtility.ToJson(pile), state);
            Assert.That(state.GetField("kind").GetValue(back), Is.EqualTo(Kind(kind)), kind);
            Assert.That(state.GetField("yaw").GetValue(back), Is.EqualTo(72f), kind);
            Assert.That(state.GetField("buildProgress").GetValue(back), Is.EqualTo(0.3f), kind);
            Assert.That(state.GetField("position").GetValue(back), Is.EqualTo(new Vector3(5f, 1f, 9f)), kind);
            if (kind != "Kitchen")
                Assert.That(state.GetMethod("Count").Invoke(back, new object[] { kind == "KitchenSite" ? "stone" : "logs" }), Is.EqualTo(2), kind);
        }
    }

    [Test]
    public void AnOldSaveWithNoKitchenLoadsUnchanged()
    {
        Type state = Find("WoodPileState");
        const string old = "{\"id\":9,\"kind\":21,\"position\":{\"x\":1.0,\"y\":0.0,\"z\":2.0},\"yaw\":10.0,\"contents\":[{\"itemId\":\"logs\",\"count\":4,\"day\":0}]}";
        object pile = JsonUtility.FromJson(old, state);
        Assert.That(state.GetField("kind").GetValue(pile), Is.EqualTo(Kind("WoodShed")));
        Assert.That(state.GetMethod("Count").Invoke(pile, new object[] { "logs" }), Is.EqualTo(4));
        Assert.That((bool)state.GetProperty("IsKitchen").GetValue(pile), Is.False);
        // A save made before phase A had no site fields at all; they load as zero.
        const string older = "{\"id\":2,\"kind\":8,\"position\":{\"x\":0.0,\"y\":0.0,\"z\":0.0},\"yaw\":0.0,\"contents\":[]}";
        object cabin = JsonUtility.FromJson(older, state);
        Assert.That(state.GetField("kind").GetValue(cabin), Is.EqualTo(Kind("Cabin")));
        Assert.That(state.GetField("buildProgress").GetValue(cabin), Is.EqualTo(0f));
    }

    [Test]
    public void TheKitchenAddedNoClockFieldsAndItsCostTextNamesBothStages()
    {
        Type wells = Find("WellsAndOuthouses");
        foreach (FieldInfo f in wells.GetFields())
            Assert.That(f.Name.ToLower(), Does.Not.Contain("minutes").And.Not.Contain("skip"), f.Name);
        Assert.That(wells.GetMethod("KitchenRoofText").Invoke(null, null), Is.EqualTo("2 Logs, 6 Branches, 4 Tall Grass"));
        Assert.That(wells.GetMethod("FinishedMessage").Invoke(null, new[] { Kind("KitchenSite") }), Does.StartWith("Hearth built"));
        Assert.That(wells.GetMethod("FinishedMessage").Invoke(null, new[] { Kind("KitchenRoofSite") }), Is.EqualTo("Covered Outdoor Kitchen built."));
    }
}
