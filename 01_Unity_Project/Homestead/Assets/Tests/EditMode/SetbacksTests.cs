using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// EditMode tests for Structures Slice 1, phase A. The setbacks and floodplain maths are pure (Homestead.Setbacks). The saved pile
// fields, the PileKind numbers and the Well Site's stage rules live in the main assembly, which a test assembly cannot reference, so
// those are reached by reflection on throwaway objects. Nothing here touches the Saves folder.
public class SetbacksTests
{
    static SetbackRules Rules => SetbackRules.Defaults;

    static SiteInfo Site(SiteKind kind, float x, float z, float y = 0f, string name = "") =>
        new SiteInfo { kind = kind, position = new Vector3(x, y, z), name = name };

    static Type Find(string name) =>
        AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => { try { return a.GetTypes(); } catch (Exception) { return new Type[0]; } })
                 .First(t => t.Name == name);

    // --- A Well ---

    [Test]
    public void WellKeepsThirtyMetresFromAnOuthouse()
    {
        var sites = new List<SiteInfo> { Site(SiteKind.Outhouse, 0f, 0f, 0f, "outhouse") };
        Assert.That(Setbacks.CanPlaceWell(new Vector3(29f, 5f, 0f), sites, Rules, out string reason), Is.False);
        Assert.That(reason, Is.EqualTo("Too close to the outhouse: 30 m needed."));
        Assert.That(Setbacks.CanPlaceWell(new Vector3(30.1f, 5f, 0f), sites, Rules, out _), Is.True, "past 30 m and uphill");
    }

    [Test]
    public void WellMustBeUphillOfAnOuthouse()
    {
        var sites = new List<SiteInfo> { Site(SiteKind.Outhouse, 0f, 0f, 3f) };
        Assert.That(Setbacks.CanPlaceWell(new Vector3(40f, 2f, 0f), sites, Rules, out string reason), Is.False, "the outhouse is higher");
        Assert.That(reason, Does.Contain("higher ground"));
        Assert.That(Setbacks.CanPlaceWell(new Vector3(40f, 4f, 0f), sites, Rules, out _), Is.True, "the well is higher");
        Assert.That(Setbacks.CanPlaceWell(new Vector3(80f, 2f, 0f), sites, Rules, out _), Is.True, "beyond the up-gradient range the slope doesn't matter");
    }

    [Test]
    public void WellKeepsThreeMetresFromABuildingAndFifteenFromAPen()
    {
        var sites = new List<SiteInfo>
        {
            Site(SiteKind.Building, 0f, 0f, 0f, "Small Cabin"),
            Site(SiteKind.AnimalPen, 100f, 0f),
        };
        Assert.That(Setbacks.CanPlaceWell(new Vector3(2.5f, 0f, 0f), sites, Rules, out string reason), Is.False);
        Assert.That(reason, Is.EqualTo("Too close to the Small Cabin: 3 m needed."));
        Assert.That(Setbacks.CanPlaceWell(new Vector3(3.5f, 0f, 0f), sites, Rules, out _), Is.True);
        Assert.That(Setbacks.CanPlaceWell(new Vector3(90f, 0f, 0f), sites, Rules, out reason), Is.False);
        Assert.That(reason, Is.EqualTo("Too close to the animal pen: 15 m needed."));
        Assert.That(Setbacks.CanPlaceWell(new Vector3(84.5f, 0f, 0f), sites, Rules, out _), Is.True, "15.5 m from the pen");
    }

    // --- An Outhouse ---

    [Test]
    public void OuthouseKeepsThirtyMetresFromWaterMeasuredToTheEdge()
    {
        var water = new List<WaterEdge> { new WaterEdge { name = "the creek", distanceMetres = 12f }, new WaterEdge { name = "Bass Hole pond", distanceMetres = 80f } };
        Assert.That(Setbacks.CanPlaceOuthouse(Vector3.zero, new List<SiteInfo>(), water, Rules, out string reason), Is.False);
        Assert.That(reason, Is.EqualTo("Too close to the creek: 30 m needed."));
        water[0] = new WaterEdge { name = "the creek", distanceMetres = 30f };
        Assert.That(Setbacks.CanPlaceOuthouse(Vector3.zero, new List<SiteInfo>(), water, Rules, out _), Is.True, "exactly 30 m is allowed");
        water[1] = new WaterEdge { name = "Bass Hole pond", distanceMetres = -3f };
        Assert.That(Setbacks.CanPlaceOuthouse(Vector3.zero, new List<SiteInfo>(), water, Rules, out reason), Is.False, "inside the pond");
        Assert.That(reason, Does.Contain("Bass Hole pond"));
    }

    [Test]
    public void OuthouseKeepsSevenAndAHalfMetresFromBuildingsAndThirtyFromAWell()
    {
        var none = new List<WaterEdge>();
        var sites = new List<SiteInfo> { Site(SiteKind.Building, 0f, 0f, 0f, "Small Cabin") };
        Assert.That(Setbacks.CanPlaceOuthouse(new Vector3(7f, 0f, 0f), sites, none, Rules, out string reason), Is.False);
        Assert.That(reason, Is.EqualTo("Too close to the Small Cabin: 7.5 m needed."));
        Assert.That(Setbacks.CanPlaceOuthouse(new Vector3(7.6f, 0f, 0f), sites, none, Rules, out _), Is.True);

        sites.Add(Site(SiteKind.Well, 50f, 0f, 1f));
        Assert.That(Setbacks.CanPlaceOuthouse(new Vector3(25f, 0f, 0f), sites, none, Rules, out reason), Is.False);
        Assert.That(reason, Is.EqualTo("Too close to the well: 30 m needed."));
    }

    [Test]
    public void OuthouseMustNotStandUphillOfAWell()
    {
        var none = new List<WaterEdge>();
        var sites = new List<SiteInfo> { Site(SiteKind.Well, 0f, 0f, 2f) };
        Assert.That(Setbacks.CanPlaceOuthouse(new Vector3(40f, 3f, 0f), sites, none, Rules, out string reason), Is.False, "outhouse higher than the well");
        Assert.That(reason, Does.Contain("downhill"));
        Assert.That(Setbacks.CanPlaceOuthouse(new Vector3(40f, 1f, 0f), sites, none, Rules, out _), Is.True);
    }

    [Test]
    public void ExistingWellIsFouledByATooCloseOrUphillOuthouse()
    {
        var well = new Vector3(0f, 2f, 0f);
        Assert.That(Setbacks.Fouls(well, new List<SiteInfo> { Site(SiteKind.Outhouse, 10f, 0f, 0f) }, Rules, out string reason), Is.True);
        Assert.That(reason, Does.Contain("too close"));
        Assert.That(Setbacks.Fouls(well, new List<SiteInfo> { Site(SiteKind.Outhouse, 40f, 0f, 3f) }, Rules, out reason), Is.True, "uphill");
        Assert.That(reason, Does.Contain("uphill"));
        Assert.That(Setbacks.Fouls(well, new List<SiteInfo> { Site(SiteKind.Outhouse, 40f, 0f, 1f) }, Rules, out _), Is.False, "clear and downhill");
        Assert.That(Setbacks.Fouls(well, new List<SiteInfo> { Site(SiteKind.Outhouse, 90f, 0f, 9f) }, Rules, out _), Is.False, "out of range");
        Assert.That(Setbacks.Fouls(well, new List<SiteInfo>(), Rules, out _), Is.False);
    }

    // --- Floodplain and hollows ---

    [Test]
    public void FloodplainIsNearTheChannelAndLowEnough()
    {
        Assert.That(Setbacks.Floods(10.5f, 10f, 5f, Rules), Is.True, "close and within 1.5 m of the surface");
        Assert.That(Setbacks.Floods(12f, 10f, 5f, Rules), Is.False, "higher than the surface plus the rise");
        Assert.That(Setbacks.Floods(10.2f, 10f, 40f, Rules), Is.False, "too far from the channel");
        Assert.That(Setbacks.Floods(11.5f, 10f, 25f, Rules), Is.True, "exactly on both limits");
        Assert.That(Setbacks.Floods(8f, 10f, -1f, Rules), Is.True, "in the channel");
    }

    [Test]
    public void HollowIsLowerThanTheGroundAroundIt()
    {
        var ring = new List<float> { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f };
        Assert.That(Setbacks.InHollow(0.4f, ring, 0.5f), Is.True);
        Assert.That(Setbacks.InHollow(0.6f, ring, 0.5f), Is.False, "0.4 m lower is not a hollow");
        Assert.That(Setbacks.InHollow(1f, ring, 0.5f), Is.False, "flat");
        Assert.That(Setbacks.InHollow(2f, ring, 0.5f), Is.False, "a rise");
        Assert.That(Setbacks.InHollow(0f, new List<float>(), 0.5f), Is.False, "nothing sampled");
    }

    // --- Saves and kinds (main assembly, by reflection) ---

    [Test]
    public void NewPileKindsAreAppendedAndOldNumbersAreUnchanged()
    {
        Type kind = Find("PileKind");
        Assert.That((int)Enum.Parse(kind, "RainBarrel"), Is.EqualTo(12), "the last old kind keeps its number");
        Assert.That((int)Enum.Parse(kind, "WellSite"), Is.EqualTo(13));
        Assert.That((int)Enum.Parse(kind, "Well"), Is.EqualTo(14));
        Assert.That((int)Enum.Parse(kind, "Outhouse"), Is.EqualTo(15), "a finished Outhouse in an old save keeps its number");
        Assert.That((int)Enum.Parse(kind, "OuthouseSite"), Is.EqualTo(16), "the new staged site goes on the end");
        Assert.That((int)Enum.Parse(kind, "WaterBarrelSite"), Is.EqualTo(17));
        Assert.That((int)Enum.Parse(kind, "RainBarrelSite"), Is.EqualTo(18));
        Assert.That((int)Enum.Parse(kind, "FoodCacheSite"), Is.EqualTo(19));
        Assert.That((int)Enum.Parse(kind, "WaterBarrel"), Is.EqualTo(3), "finished barrels and caches in old saves keep their numbers");
        Assert.That((int)Enum.Parse(kind, "FoodCache"), Is.EqualTo(4));
        Assert.That((int)Enum.Parse(kind, "Cabin"), Is.EqualTo(8));
    }

    [Test]
    public void AnOldPileSaveLoadsWithTheNewFieldsAtZero()
    {
        Type state = Find("WoodPileState");
        const string oldJson = "{\"id\":7,\"kind\":8,\"position\":{\"x\":1.0,\"y\":2.0,\"z\":3.0},\"yaw\":90.0,\"contents\":[{\"itemId\":\"logs\",\"count\":4,\"day\":0}]}";
        object pile = JsonUtility.FromJson(oldJson, state);
        Assert.That(pile, Is.Not.Null);
        Assert.That(state.GetField("id").GetValue(pile), Is.EqualTo(7));
        Assert.That(state.GetField("litres").GetValue(pile), Is.EqualTo(0));
        Assert.That(state.GetField("contaminatedHours").GetValue(pile), Is.EqualTo(0f));
        Assert.That(state.GetField("digProgress").GetValue(pile), Is.EqualTo(0f));
        Assert.That(state.GetField("builtDay").GetValue(pile), Is.EqualTo(0));

        state.GetField("litres").SetValue(pile, 120);
        state.GetField("digProgress").SetValue(pile, 0.5f);
        object back = JsonUtility.FromJson(JsonUtility.ToJson(pile), state);
        Assert.That(state.GetField("litres").GetValue(back), Is.EqualTo(120), "the new fields round-trip");
        Assert.That(state.GetField("digProgress").GetValue(back), Is.EqualTo(0.5f));
    }

    object NewSite(out Type state, string kind = "WellSite")
    {
        state = Find("WoodPileState");
        object site = Activator.CreateInstance(state);
        state.GetField("kind").SetValue(site, Enum.Parse(Find("PileKind"), kind));
        return site;
    }

    [Test]
    public void WellSiteTakesItsStagesInOrderAndAcceptsPartialDeliveries()
    {
        Type state;
        object site = NewSite(out state);
        MethodInfo accepts = Find("WellsAndOuthouses").GetMethod("SiteAccepts");
        MethodInfo add = state.GetMethod("Add");
        bool Accepts(string id) => (bool)accepts.Invoke(null, new[] { site, id });

        Assert.That(Accepts("stone"), Is.False, "nothing is taken before the shaft is dug");
        state.GetField("digProgress").SetValue(site, 1f);
        Assert.That(Accepts("stone"), Is.True);
        Assert.That(Accepts("logs"), Is.False, "the cap timber waits for the lining");

        add.Invoke(site, new object[] { "stone", 5, 0 }); // one trip of 5
        Assert.That(Accepts("stone"), Is.True, "a partial delivery leaves the stage open");
        Assert.That(Accepts("logs"), Is.False);
        add.Invoke(site, new object[] { "stone", 5, 0 }); // the second trip
        Assert.That(Accepts("stone"), Is.False, "the lining is full");
        Assert.That(Accepts("logs"), Is.True);
        Assert.That(Accepts("branches"), Is.True);
        Assert.That(Accepts("sticks"), Is.False);

        MethodInfo complete = Find("WellsAndOuthouses").GetMethod("SiteComplete");
        add.Invoke(site, new object[] { "logs", 2, 0 });
        Assert.That((bool)complete.Invoke(null, new[] { site }), Is.False, "branches still missing");
        Assert.That(Accepts("logs"), Is.False);
        add.Invoke(site, new object[] { "branches", 4, 0 });
        Assert.That((bool)complete.Invoke(null, new[] { site }), Is.True);
    }

    // --- The Hammer rule ---

    [Test]
    public void OnlySingleStepBuildScreenStructuresNeedTheHammerToBuild()
    {
        Type kind = Find("PileKind");
        MethodInfo needs = Find("WoodManager").GetMethod("NeedsHammerToBuild");
        bool Needs(string name) => (bool)needs.Invoke(null, new[] { Enum.Parse(kind, name) });

        // The click that builds these takes the Hammer (the ghost shows without it, red).
        foreach (string single in new[] { "WoodStorage", "RockStorage", "StorageBin", "ToolRack" })
            Assert.That(Needs(single), Is.True, single);
        // Staged sites are placed freely (the Hammer finishes them), and the Tent, Lean-To and Tarp are not on Mike's list.
        foreach (string free in new[] { "WellSite", "OuthouseSite", "WaterBarrelSite", "RainBarrelSite", "FoodCacheSite", "WaterBarrel", "RainBarrel", "FoodCache", "Outhouse", "CabinSite", "Tent", "LeanTo", "TarpShelter", "Cabin", "Well", "Felled" })
            Assert.That(Needs(free), Is.False, free);

        string text = (string)Find("WellsAndOuthouses").GetField("EquipHammerToBuild").GetValue(null);
        Assert.That(text, Is.EqualTo("Equip the Hammer to build."));
    }

    [Test]
    public void WellSiteNeedsNameWhatIsStillMissingAndCompleteOnlyWhenStocked()
    {
        Type state;
        object site = NewSite(out state);
        MethodInfo needs = Find("WellsAndOuthouses").GetMethod("SiteNeeds");
        MethodInfo add = state.GetMethod("Add");
        string Needs() => (string)needs.Invoke(null, new[] { site });

        Assert.That(Needs(), Is.EqualTo("the shaft dug (Shovel)"));
        state.GetField("digProgress").SetValue(site, 1f);
        Assert.That(Needs(), Is.EqualTo("10 Stone, 2 Logs, 4 Branches"));
        add.Invoke(site, new object[] { "stone", 5, 0 });
        Assert.That(Needs(), Is.EqualTo("5 Stone, 2 Logs, 4 Branches"));
        add.Invoke(site, new object[] { "stone", 5, 0 });
        add.Invoke(site, new object[] { "logs", 2, 0 });
        Assert.That(Needs(), Is.EqualTo("4 Branches"));
        add.Invoke(site, new object[] { "branches", 4, 0 });
        Assert.That(Needs(), Is.Null, "stocked: delivering alone does not finish it - the Hammer's pounding does");
    }

    [Test]
    public void BuildCostsAreTheDocumentedOnesAndStayWithinATrip()
    {
        Type wells = Find("WellsAndOuthouses");
        Type stack = Find("WoodStack");
        Func<object, int> count = s => (int)stack.GetField("count").GetValue(s);
        Func<object, string> id = s => (string)stack.GetField("itemId").GetValue(s);

        var well = ((Array)wells.GetMethod("WellCost").Invoke(null, null)).Cast<object>().ToDictionary(id, count);
        Assert.That(well["stone"], Is.EqualTo(10));
        Assert.That(well["logs"], Is.EqualTo(2));
        Assert.That(well["branches"], Is.EqualTo(4));

        var outhouse = ((Array)wells.GetMethod("OuthouseCost").Invoke(null, null)).Cast<object>().ToDictionary(id, count);
        Assert.That(outhouse["logs"], Is.EqualTo(2));
        Assert.That(outhouse["branches"], Is.EqualTo(6));
        Assert.That(outhouse["tall_grass"], Is.EqualTo(3));

        // Weights from the item assets (kg): Logs 8, Branches 1, Tall Grass 0.1 -> the Outhouse is one 22.3 kg trip; Stone 5 -> 5 per trip.
        Assert.That(outhouse["logs"] * 8f + outhouse["branches"] * 1f + outhouse["tall_grass"] * 0.1f, Is.LessThanOrEqualTo(25f));
        Assert.That(5 * 5f, Is.LessThanOrEqualTo(25f));
        Assert.That(well["logs"] * 8f + well["branches"] * 1f, Is.LessThanOrEqualTo(25f), "the cap stage is one trip");
    }

    // --- Staged sites over the weight cutoff (Mike, 2026-10-19) ---

    [Test]
    public void OuthouseIsOverTheFifteenKilogramCutoffAndSoIsAStagedSite()
    {
        Type wells = Find("WellsAndOuthouses");
        Type kind = Find("PileKind");
        object outhouseCost = wells.GetMethod("OuthouseCost").Invoke(null, null);
        object wellCost = wells.GetMethod("WellCost").Invoke(null, null);
        MethodInfo weight = wells.GetMethod("WeightKg");
        MethodInfo staging = wells.GetMethod("NeedsStaging");

        // Item assets: Logs 8, Branches 1, Tall Grass 0.1 -> 2*8 + 6*1 + 3*0.1.
        Assert.That((float)weight.Invoke(null, new[] { outhouseCost }), Is.EqualTo(22.3f).Within(0.001f));
        Assert.That((bool)staging.Invoke(null, new[] { outhouseCost }), Is.True, "22.3 kg is over the cutoff");
        Assert.That((bool)staging.Invoke(null, new[] { wellCost }), Is.True);

        MethodInfo isStaged = wells.GetMethod("IsStagedSite");
        MethodInfo finished = wells.GetMethod("FinishedKind");
        Assert.That((bool)isStaged.Invoke(null, new[] { Enum.Parse(kind, "OuthouseSite") }), Is.True);
        Assert.That((bool)isStaged.Invoke(null, new[] { Enum.Parse(kind, "WellSite") }), Is.True);
        Assert.That((bool)isStaged.Invoke(null, new[] { Enum.Parse(kind, "Outhouse") }), Is.False, "the finished structure is not a site");
        Assert.That(finished.Invoke(null, new[] { Enum.Parse(kind, "OuthouseSite") }), Is.EqualTo(Enum.Parse(kind, "Outhouse")));
        Assert.That(finished.Invoke(null, new[] { Enum.Parse(kind, "WellSite") }), Is.EqualTo(Enum.Parse(kind, "Well")));
        Assert.That((bool)wells.GetMethod("RequiresDig").Invoke(null, new[] { Enum.Parse(kind, "OuthouseSite") }), Is.False, "no Shovel stage on the Outhouse");
    }

    [Test]
    public void TheCutoffIsATunableFieldDefaultingToFifteen()
    {
        Type wells = Find("WellsAndOuthouses");
        FieldInfo cutoff = wells.GetField("stagedSiteWeightKg");
        Assert.That(cutoff, Is.Not.Null);
        var go = new GameObject("test wells");
        try
        {
            Assert.That((float)cutoff.GetValue(go.AddComponent(wells)), Is.EqualTo(15f));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(go);
        }
    }

    [Test]
    public void OuthouseSiteTakesItsMaterialsInAnyOrderAndTwoTripsStayUnderTheEncumberedLine()
    {
        Type state;
        object site = NewSite(out state, "OuthouseSite");
        Type wells = Find("WellsAndOuthouses");
        MethodInfo accepts = wells.GetMethod("SiteAccepts");
        MethodInfo needs = wells.GetMethod("SiteNeeds");
        MethodInfo complete = wells.GetMethod("SiteComplete");
        MethodInfo add = state.GetMethod("Add");
        bool Accepts(string id) => (bool)accepts.Invoke(null, new[] { site, id });
        string Needs() => (string)needs.Invoke(null, new[] { site });

        Assert.That(Accepts("logs") && Accepts("branches") && Accepts("tall_grass"), Is.True, "no dig stage: everything is taken at once");
        Assert.That(Accepts("stone") || Accepts("sticks"), Is.False, "only what the Outhouse uses");
        Assert.That(Needs(), Is.EqualTo("2 Logs, 6 Branches, 3 Tall Grass"));

        // Trip one: the two Logs (16 kg - well under the 30 kg Encumbered line).
        add.Invoke(site, new object[] { "logs", 2, 0 });
        Assert.That(Accepts("logs"), Is.False, "full");
        Assert.That(Needs(), Is.EqualTo("6 Branches, 3 Tall Grass"));
        Assert.That((bool)complete.Invoke(null, new[] { site }), Is.False);

        // Trip two: 6 Branches and 3 Tall Grass (6.3 kg). A partial delivery in between is fine.
        add.Invoke(site, new object[] { "branches", 3, 0 });
        Assert.That(Accepts("branches"), Is.True);
        add.Invoke(site, new object[] { "branches", 3, 0 });
        add.Invoke(site, new object[] { "tall_grass", 3, 0 });
        Assert.That(Needs(), Is.Null);
        Assert.That((bool)complete.Invoke(null, new[] { site }), Is.True, "stocked; the Hammer's Complete Outhouse finishes it");

        float trip1 = 2 * 8f, trip2 = 6 * 1f + 3 * 0.1f;
        Assert.That(trip1, Is.LessThan(30f));
        Assert.That(trip2, Is.LessThan(30f));
        Assert.That(trip1 + trip2, Is.EqualTo(22.3f).Within(0.001f), "one trip would have been the whole 22.3 kg");
    }

    [Test]
    public void AHalfDeliveredOuthouseSiteSavesAndLoadsWithItsDeliveries()
    {
        Type state;
        object site = NewSite(out state, "OuthouseSite");
        MethodInfo add = state.GetMethod("Add");
        add.Invoke(site, new object[] { "logs", 2, 0 });
        add.Invoke(site, new object[] { "branches", 4, 0 });

        object back = JsonUtility.FromJson(JsonUtility.ToJson(site), state);
        Assert.That(state.GetField("kind").GetValue(back).ToString(), Is.EqualTo("OuthouseSite"));
        MethodInfo count = state.GetMethod("Count");
        Assert.That(count.Invoke(back, new object[] { "logs" }), Is.EqualTo(2));
        Assert.That(count.Invoke(back, new object[] { "branches" }), Is.EqualTo(4));
        Assert.That(Find("WellsAndOuthouses").GetMethod("SiteNeeds").Invoke(null, new[] { back }), Is.EqualTo("2 Branches, 3 Tall Grass"));

        // A finished Outhouse from an older save (kind 15) is still a finished Outhouse, not a site.
        const string oldJson = "{\"id\":3,\"kind\":15,\"position\":{\"x\":1.0,\"y\":2.0,\"z\":3.0},\"yaw\":0.0,\"contents\":[],\"builtDay\":4}";
        object old = JsonUtility.FromJson(oldJson, state);
        Assert.That(state.GetField("kind").GetValue(old).ToString(), Is.EqualTo("Outhouse"));
        Assert.That(state.GetProperty("IsPlacedStructure").GetValue(old), Is.EqualTo(true));
    }

    // --- Water Barrel, Rain Barrel and Food Cache as staged sites ---

    [Test]
    public void BarrelsAndCacheAreOverTheCutoffAndStagedWithTheDocumentedMaterials()
    {
        Type wells = Find("WellsAndOuthouses");
        Type kind = Find("PileKind");
        Type stack = Find("WoodStack");
        MethodInfo cost = wells.GetMethod("FinishedCost");
        MethodInfo finished = wells.GetMethod("FinishedKind");
        MethodInfo weight = wells.GetMethod("WeightKg");
        MethodInfo staged = wells.GetMethod("IsStagedSite");
        object K(string n) => Enum.Parse(kind, n);

        var expected = new (string site, string done, float kg, string[] ids)[]
        {
            ("WaterBarrelSite", "WaterBarrel", 24f, new[] { "logs:3" }),
            ("RainBarrelSite", "RainBarrel", 26f, new[] { "logs:3", "branches:2" }),
            ("FoodCacheSite", "FoodCache", 20f, new[] { "logs:2", "branches:4" }),
        };
        foreach (var e in expected)
        {
            Assert.That((bool)staged.Invoke(null, new[] { K(e.site) }), Is.True, e.site);
            Assert.That((bool)staged.Invoke(null, new[] { K(e.done) }), Is.False, e.done + " is the finished form");
            Assert.That(finished.Invoke(null, new[] { K(e.site) }), Is.EqualTo(K(e.done)));
            object c = cost.Invoke(null, new[] { K(e.site) });
            Assert.That((float)weight.Invoke(null, new[] { c }), Is.EqualTo(e.kg).Within(0.001f), e.site);
            Assert.That((bool)wells.GetMethod("NeedsStaging").Invoke(null, new[] { c }), Is.True, e.site + " is over 15 kg");
            var got = ((Array)c).Cast<object>().Select(s => (string)stack.GetField("itemId").GetValue(s) + ":" + (int)stack.GetField("count").GetValue(s)).ToArray();
            Assert.That(got, Is.EquivalentTo(e.ids), e.site);
            Assert.That((bool)wells.GetMethod("RequiresDig").Invoke(null, new[] { K(e.site) }), Is.False, "no Shovel stage");
        }
        // The single-step ones stay under the cutoff or are not staged at all.
        foreach (string single in new[] { "WoodStorage", "RockStorage", "StorageBin", "ToolRack", "Tent", "LeanTo", "TarpShelter" })
            Assert.That((bool)staged.Invoke(null, new[] { K(single) }), Is.False, single);
    }

    [Test]
    public void EachBarrelSiteTakesItsMaterialsInTripsAndStopsAtTheCost()
    {
        foreach (var e in new[] { ("WaterBarrelSite", 3, 0), ("RainBarrelSite", 3, 2), ("FoodCacheSite", 2, 4) })
        {
            Type state;
            object site = NewSite(out state, e.Item1);
            Type wells = Find("WellsAndOuthouses");
            MethodInfo accepts = wells.GetMethod("SiteAccepts");
            MethodInfo add = state.GetMethod("Add");
            bool Accepts(string id) => (bool)accepts.Invoke(null, new[] { site, id });

            Assert.That(Accepts("logs"), Is.True, e.Item1);
            Assert.That(Accepts("stone") || Accepts("sticks") || Accepts("tall_grass"), Is.False, e.Item1);
            add.Invoke(site, new object[] { "logs", 1, 0 }); // first trip: one Log (8 kg)
            Assert.That(Accepts("logs"), Is.True, "a partial delivery leaves it open");
            add.Invoke(site, new object[] { "logs", e.Item2 - 1, 0 }); // second trip: the rest
            Assert.That(Accepts("logs"), Is.False, "full");
            Assert.That(Accepts("branches"), Is.EqualTo(e.Item3 > 0));
            if (e.Item3 > 0)
                add.Invoke(site, new object[] { "branches", e.Item3, 0 });
            Assert.That(wells.GetMethod("SiteNeeds").Invoke(null, new[] { site }), Is.Null, e.Item1);
            Assert.That((bool)wells.GetMethod("SiteComplete").Invoke(null, new[] { site }), Is.True, e.Item1);
        }
    }

    [Test]
    public void HalfDeliveredBarrelSitesRoundTripAndFinishedBarrelsAndCachesLoadWithTheirContents()
    {
        Type state = Find("WoodPileState");
        foreach (string name in new[] { "WaterBarrelSite", "RainBarrelSite", "FoodCacheSite" })
        {
            object site = NewSite(out _, name);
            state.GetMethod("Add").Invoke(site, new object[] { "logs", 1, 0 });
            object back = JsonUtility.FromJson(JsonUtility.ToJson(site), state);
            Assert.That(state.GetField("kind").GetValue(back).ToString(), Is.EqualTo(name));
            Assert.That(state.GetMethod("Count").Invoke(back, new object[] { "logs" }), Is.EqualTo(1));
            Assert.That(state.GetProperty("IsPlacedStructure").GetValue(back), Is.EqualTo(true));
        }

        // Older saves (kinds 3 Water Barrel, 12 Rain Barrel, 4 Food Cache) with their contents: still the finished storage, not sites.
        var old = new[]
        {
            ("{\"id\":1,\"kind\":3,\"position\":{\"x\":0.0,\"y\":0.0,\"z\":0.0},\"yaw\":0.0,\"contents\":[{\"itemId\":\"good_water\",\"count\":25,\"day\":0}]}", "WaterBarrel", "good_water", 25),
            ("{\"id\":2,\"kind\":12,\"position\":{\"x\":0.0,\"y\":0.0,\"z\":0.0},\"yaw\":90.0,\"contents\":[{\"itemId\":\"good_water\",\"count\":60,\"day\":0}]}", "RainBarrel", "good_water", 60),
            ("{\"id\":3,\"kind\":4,\"position\":{\"x\":0.0,\"y\":0.0,\"z\":0.0},\"yaw\":0.0,\"contents\":[{\"itemId\":\"cooked_venison\",\"count\":2,\"day\":5}]}", "FoodCache", "cooked_venison", 2),
        };
        foreach (var o in old)
        {
            object pile = JsonUtility.FromJson(o.Item1, state);
            Assert.That(state.GetField("kind").GetValue(pile).ToString(), Is.EqualTo(o.Item2));
            Assert.That(state.GetProperty("IsPlacedStructure").GetValue(pile), Is.EqualTo(false), o.Item2 + " stays a WoodPile");
            Assert.That(state.GetMethod("Count").Invoke(pile, new object[] { o.Item3 }), Is.EqualTo(o.Item4), o.Item2 + " keeps its contents");
        }
    }

    [Test]
    public void RainBarrelSitesAreTreatedAsRainBarrelsForTheEaveAndSpacingRules()
    {
        MethodInfo isRain = Find("WoodManager").GetMethod("IsRainBarrel");
        Type kind = Find("PileKind");
        Assert.That((bool)isRain.Invoke(null, new[] { Enum.Parse(kind, "RainBarrelSite") }), Is.True);
        Assert.That((bool)isRain.Invoke(null, new[] { Enum.Parse(kind, "RainBarrel") }), Is.True);
        Assert.That((bool)isRain.Invoke(null, new[] { Enum.Parse(kind, "WaterBarrelSite") }), Is.False);
    }

    // --- Pounding with the Hammer ---

    [Test]
    public void PoundingProgressStartsAtZeroInOldSavesAndRoundTrips()
    {
        Type state = Find("WoodPileState");
        const string oldJson = "{\"id\":9,\"kind\":13,\"position\":{\"x\":1.0,\"y\":2.0,\"z\":3.0},\"yaw\":0.0,\"contents\":[]}";
        object old = JsonUtility.FromJson(oldJson, state);
        Assert.That(state.GetField("buildProgress").GetValue(old), Is.EqualTo(0f), "an older save has no pounding in it");

        object site = NewSite(out _, "OuthouseSite");
        state.GetField("buildProgress").SetValue(site, 0.4f);
        state.GetMethod("Add").Invoke(site, new object[] { "logs", 2, 0 });
        object back = JsonUtility.FromJson(JsonUtility.ToJson(site), state);
        Assert.That(state.GetField("buildProgress").GetValue(back), Is.EqualTo(0.4f), "a half-pounded site keeps its progress");
        Assert.That(state.GetMethod("Count").Invoke(back, new object[] { "logs" }), Is.EqualTo(2), "and its deliveries");
    }

    [Test]
    public void BlowsAdvanceTheSiteByEfficiencyOverBlowsAndStopAtOne()
    {
        MethodInfo advance = Find("WellsAndOuthouses").GetMethod("AdvanceBuild");
        float Advance(float p, float eff, int blows) => (float)advance.Invoke(null, new object[] { p, eff, blows });

        float progress = 0f;
        for (int i = 0; i < 19; i++)
            progress = Advance(progress, 1f, 20);
        Assert.That(progress, Is.EqualTo(0.95f).Within(0.0001f), "19 of 20 blows");
        Assert.That(Advance(progress, 1f, 20), Is.EqualTo(1f).Within(0.0001f), "the 20th finishes it");
        Assert.That(Advance(1f, 1f, 20), Is.EqualTo(1f), "never past 1");
        Assert.That(Advance(0f, 0.5f, 20), Is.EqualTo(0.025f).Within(0.0001f), "tired or hungry: smaller steps");
    }

    [Test]
    public void MaterialsAreLockedOnceThePoundingHasBegun()
    {
        Type state;
        object site = NewSite(out state, "WaterBarrelSite");
        MethodInfo locked = Find("WellsAndOuthouses").GetMethod("MaterialsLocked");
        Assert.That((bool)locked.Invoke(null, new[] { site }), Is.False);
        state.GetField("buildProgress").SetValue(site, 0.05f);
        Assert.That((bool)locked.Invoke(null, new[] { site }), Is.True);
    }

    [Test]
    public void HeavierStructuresTakeMoreBlowsAndAnUnstockedSiteIsNotFinished()
    {
        Type wells = Find("WellsAndOuthouses");
        Type kind = Find("PileKind");
        var go = new GameObject("test wells");
        try
        {
            Component rules = go.AddComponent(wells);
            MethodInfo blows = wells.GetMethod("BuildBlows");
            int Blows(string site) => (int)blows.Invoke(rules, new[] { Enum.Parse(kind, site) });

            Assert.That(Blows("CabinSite"), Is.GreaterThan(Blows("WellSite")));
            Assert.That(Blows("WellSite"), Is.GreaterThan(Blows("OuthouseSite")));
            Assert.That(Blows("OuthouseSite"), Is.GreaterThan(Blows("RainBarrelSite")));
            Assert.That(Blows("RainBarrelSite"), Is.GreaterThan(Blows("WaterBarrelSite")));
            Assert.That(Blows("WaterBarrelSite"), Is.GreaterThan(Blows("FoodCacheSite")));

            object empty = NewSite(out _, "OuthouseSite");
            Assert.That((bool)wells.GetMethod("FinishSite").Invoke(rules, new[] { empty }), Is.False, "nothing delivered: not finished");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(go);
        }
    }

    // --- No time jumps when working ---

    [Test]
    public void DiggingANewOutPitIsTenBlowsOfRealTimeAndNoClockFieldsRemain()
    {
        Type wells = Find("WellsAndOuthouses");
        Assert.That(wells.GetField("outhousePitDigMinutes"), Is.Null, "the 3-hour lump is gone");
        Assert.That(wells.GetField("outhousePitDigStamina"), Is.Null, "stamina is the usual swing cost per blow");
        Assert.That(wells.GetField("wellDigSkipMinutes"), Is.Null);
        Assert.That(wells.GetField("buildMinutesPerBlow"), Is.Null);
        Assert.That(wells.GetMethod("SkipTime"), Is.Null, "nothing in the structures code advances the clock any more");

        var go = new GameObject("test wells");
        try
        {
            Component rules = go.AddComponent(wells);
            Assert.That(wells.GetField("outhousePitDigBlows").GetValue(rules), Is.EqualTo(10));
            Type state;
            object outhouse = NewSite(out state, "Outhouse");
            MethodInfo blow = wells.GetMethod("PitDigBlow");
            bool Blow() => (bool)blow.Invoke(rules, new[] { outhouse, 1f });
            for (int i = 0; i < 9; i++)
                Assert.That(Blow(), Is.False, "blow " + (i + 1));
            Assert.That((float)state.GetField("digProgress").GetValue(outhouse), Is.EqualTo(0.9f).Within(0.0001f));
            Assert.That(Blow(), Is.True, "the tenth blow finishes the pit");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(go);
        }
    }

    [Test]
    public void BowDrillAttemptIsSixSecondsOfRealTimeWithNoClockField()
    {
        Type settings = Find("BowDrillSettings");
        Assert.That(settings.GetField("minutesPerAttempt"), Is.Null, "no game-minute jump per attempt");
        object defaults = Activator.CreateInstance(settings);
        Assert.That(settings.GetField("drillSeconds").GetValue(defaults), Is.EqualTo(6f));
        Assert.That(settings.GetField("staminaCost").GetValue(defaults), Is.EqualTo(15f));
        Assert.That(settings.GetField("chanceCalm").GetValue(defaults), Is.EqualTo(0.45f), "the success chance is unchanged");
    }
}
