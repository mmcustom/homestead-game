using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using NUnit.Framework;
using UnityEngine;

// EditMode tests for the Covered Outdoor Kitchen add-ons, batch 1: the four fixed slots (pure, in Setbacks.KitchenLayout), the add-on kinds,
// their costs and the placement rule that reads a pile list. The main assembly is reached by reflection on throwaway objects, as
// KitchenTests does; WoodManager is made without running Awake (GetUninitializedObject) and given a pile list to look at. Nothing here
// touches the Saves folder or a scene.
public class KitchenAddonsTests
{
    static Type Find(string name) =>
        AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => { try { return a.GetTypes(); } catch (Exception) { return new Type[0]; } })
                 .First(t => t.Name == name);

    static object Kind(string name) => Enum.Parse(Find("PileKind"), name);

    static object NewPile(string kind, Vector3 position, float yaw)
    {
        object pile = Activator.CreateInstance(Find("WoodPileState"));
        Type state = pile.GetType();
        state.GetField("kind").SetValue(pile, Kind(kind));
        state.GetField("position").SetValue(pile, position);
        state.GetField("yaw").SetValue(pile, yaw);
        return pile;
    }

    static object ManagerWith(params object[] piles)
    {
        Type wood = Find("WoodManager");
        object manager = FormatterServices.GetUninitializedObject(wood);
        Type listType = typeof(List<>).MakeGenericType(Find("WoodPileState"));
        object list = Activator.CreateInstance(listType);
        foreach (object pile in piles)
            listType.GetMethod("Add").Invoke(list, new[] { pile });
        wood.GetField("piles", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(manager, list);
        return manager;
    }

    static string[] Lines(object cost)
    {
        Type stack = Find("WoodStack");
        return ((Array)cost).Cast<object>().Select(s => (string)stack.GetField("itemId").GetValue(s) + ":" + (int)stack.GetField("count").GetValue(s)).ToArray();
    }

    static readonly Dictionary<string, float> Weight = new Dictionary<string, float>
    {
        { "logs", 8f }, { "branches", 1f }, { "sticks", 0.1f }, { "stone", 5f }, { "clay", 3.5f },
    };

    static float KgOf(object cost) => Lines(cost).Sum(l => Weight[l.Split(':')[0]] * int.Parse(l.Split(':')[1]));

    // --- The kinds ---

    [Test]
    public void AddonKindsAreAppendedAfterTheKitchenAndOldNumbersAreUnchanged()
    {
        Assert.That((int)Kind("Kitchen"), Is.EqualTo(24), "the kitchen keeps its number");
        Assert.That((int)Kind("Countertop"), Is.EqualTo(25));
        Assert.That((int)Kind("Shelving"), Is.EqualTo(26));
        Assert.That((int)Kind("Grill"), Is.EqualTo(27));
        Assert.That((int)Kind("SlopSink"), Is.EqualTo(28));
        Assert.That((int)Kind("WoodShed"), Is.EqualTo(21));
    }

    [Test]
    public void TheFourAreKitchenAddonsAndNothingElseIs()
    {
        MethodInfo is_ = Find("WoodManager").GetMethod("IsKitchenAddon");
        foreach (string name in new[] { "Countertop", "Shelving", "Grill", "SlopSink" })
            Assert.That((bool)is_.Invoke(null, new[] { Kind(name) }), Is.True, name);
        foreach (string name in new[] { "Kitchen", "KitchenSite", "KitchenRoofSite", "WoodShed", "StorageBin", "WaterBarrel", "Well", "Outhouse" })
            Assert.That((bool)is_.Invoke(null, new[] { Kind(name) }), Is.False, name);
    }

    [Test]
    public void CountertopGrillAndSinkAreDrawnByPlacedStructureAndShelvingByWoodPile()
    {
        Type state = Find("WoodPileState");
        foreach (string name in new[] { "Countertop", "Grill", "SlopSink" })
            Assert.That((bool)state.GetProperty("IsPlacedStructure").GetValue(NewPile(name, Vector3.zero, 0f)), Is.True, name);
        Assert.That((bool)state.GetProperty("IsPlacedStructure").GetValue(NewPile("Shelving", Vector3.zero, 0f)), Is.False, "Shelving is a WoodPile with the transfer screen");
        Assert.That((bool)state.GetProperty("IsKitchenAddon").GetValue(NewPile("Shelving", Vector3.zero, 0f)), Is.True);
    }

    [Test]
    public void AllFourAreSingleStepBuildsThatNeedTheHammerAndNoneIsAStagedSite()
    {
        Type wood = Find("WoodManager"), wells = Find("WellsAndOuthouses");
        foreach (string name in new[] { "Countertop", "Shelving", "Grill", "SlopSink" })
        {
            Assert.That((bool)wood.GetMethod("NeedsHammerToBuild").Invoke(null, new[] { Kind(name) }), Is.True, name + " needs the Hammer to build");
            Assert.That((bool)wells.GetMethod("IsStagedSite").Invoke(null, new[] { Kind(name) }), Is.False, name + " is one trip");
        }
    }

    [Test]
    public void TheFourAcceptNothingThroughTheStoragePathExceptShelvingWhichNeedsTheItemList()
    {
        Type state = Find("WoodPileState");
        foreach (string name in new[] { "Countertop", "Grill", "SlopSink" })
            Assert.That((bool)state.GetMethod("Accepts").Invoke(NewPile(name, Vector3.zero, 0f), new object[] { "branches" }), Is.False, name);
    }

    // --- Costs: first-pass numbers, each under the 15 kg staged line ---

    [Test]
    public void CostsAreTheFirstPassNumbersAndEachStaysUnderTheStagedLine()
    {
        Type wells = Find("WellsAndOuthouses");
        object counter = wells.GetMethod("CountertopCost").Invoke(null, null);
        object shelving = wells.GetMethod("ShelvingCost").Invoke(null, null);
        object grill = wells.GetMethod("GrillCost").Invoke(null, null);
        object sink = wells.GetMethod("SlopSinkCost").Invoke(null, null);

        Assert.That(Lines(counter), Is.EqualTo(new[] { "logs:1", "branches:4" }));
        Assert.That(Lines(shelving), Is.EqualTo(new[] { "branches:10", "sticks:6" }));
        Assert.That(Lines(grill), Is.EqualTo(new[] { "stone:2", "clay:1" }));
        Assert.That(Lines(sink), Is.EqualTo(new[] { "logs:1", "clay:1", "branches:2" }));

        Assert.That(KgOf(counter), Is.EqualTo(12f).Within(0.01f));
        Assert.That(KgOf(shelving), Is.EqualTo(10.6f).Within(0.01f));
        Assert.That(KgOf(grill), Is.EqualTo(13.5f).Within(0.01f));
        Assert.That(KgOf(sink), Is.EqualTo(13.5f).Within(0.01f));
        foreach (object cost in new[] { counter, shelving, grill, sink })
            Assert.That(KgOf(cost), Is.LessThanOrEqualTo(15f), "single step: not over the staged line");
    }

    [Test]
    public void TheSinkHoldsTwentyLitresByDefault()
    {
        Type wells = Find("WellsAndOuthouses");
        var go = new GameObject("test wells");
        try
        {
            Component rules = go.AddComponent(wells);
            Assert.That(wells.GetField("slopSinkLitres").GetValue(rules), Is.EqualTo(20));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(go);
        }
    }

    [Test]
    public void DismantleRefundsHalfOfEachLineRoundedDown()
    {
        object grill = Find("WellsAndOuthouses").GetMethod("GrillCost").Invoke(null, null);
        var halves = ((Array)grill).Cast<object>().Select(s => (int)Find("WoodStack").GetField("count").GetValue(s) / 2).ToArray();
        Assert.That(halves, Is.EqualTo(new[] { 1, 0 }), "2 Stone gives 1 back, a single Clay gives nothing");
        object sink = Find("WellsAndOuthouses").GetMethod("SlopSinkCost").Invoke(null, null);
        Assert.That(((Array)sink).Cast<object>().Select(s => (int)Find("WoodStack").GetField("count").GetValue(s) / 2).ToArray(), Is.EqualTo(new[] { 0, 0, 1 }));
    }

    // --- The slots (pure) ---

    [Test]
    public void ThereAreFourSlotsAndAllAreUnderTheRoof()
    {
        Assert.That(Setbacks.KitchenLayout.SlotCount, Is.EqualTo(4));
        for (int i = 0; i < Setbacks.KitchenLayout.SlotCount; i++)
            Assert.That(Setbacks.KitchenLayout.UnderRoof(Vector3.zero, 0f, Setbacks.KitchenLayout.SlotAt(Vector3.zero, 0f, i)), Is.True, "slot " + i);
    }

    [Test]
    public void NoSlotIsWithinThreeQuartersOfAMetreOfTheHearthFire()
    {
        for (int i = 0; i < Setbacks.KitchenLayout.SlotCount; i++)
            Assert.That(Setbacks.KitchenLayout.SlotToHearth(i), Is.GreaterThan(Setbacks.KitchenLayout.SlotMinHearthGap), "slot " + i + " keeps the hearth lookup unambiguous");
    }

    // The footprint of an item in slot i, in the kitchen's own space: the slot's width runs along its wall, its depth across it.
    static Rect Footprint(int i)
    {
        Vector3 c = Setbacks.KitchenLayout.SlotLocalPosition(i);
        bool alongZ = Mathf.Abs(Mathf.Sin(Setbacks.KitchenLayout.SlotYaw(0f, i) * Mathf.Deg2Rad)) > 0.5f;
        float hx = alongZ ? Setbacks.KitchenLayout.SlotHalfDepth : Setbacks.KitchenLayout.SlotHalfWidth;
        float hz = alongZ ? Setbacks.KitchenLayout.SlotHalfWidth : Setbacks.KitchenLayout.SlotHalfDepth;
        return Rect.MinMaxRect(c.x - hx, c.z - hz, c.x + hx, c.z + hz);
    }

    [Test]
    public void SlotsDoNotOverlapEachOtherTheHearthSlabOrThePrepTable()
    {
        Rect slab = Rect.MinMaxRect(-1.1f, -0.6f, 0.4f, 0.9f);       // the hearth slab and its backing wall
        Rect table = Rect.MinMaxRect(0.4f, -1.3f, 1.5f, -0.7f);      // the prep table
        for (int i = 0; i < Setbacks.KitchenLayout.SlotCount; i++)
        {
            Assert.That(Footprint(i).Overlaps(slab), Is.False, "slot " + i + " vs the hearth slab");
            Assert.That(Footprint(i).Overlaps(table), Is.False, "slot " + i + " vs the prep table");
            for (int j = i + 1; j < Setbacks.KitchenLayout.SlotCount; j++)
                Assert.That(Footprint(i).Overlaps(Footprint(j)), Is.False, $"slot {i} vs slot {j}");
        }
    }

    [Test]
    public void TheOpenFrontAndTheWalkInToTheHearthStayClear()
    {
        Rect aisle = Rect.MinMaxRect(-1.1f, 0.9f, 0.4f, Setbacks.KitchenLayout.HalfDepth + 0.5f); // from the front edge to the front of the hearth slab
        for (int i = 0; i < Setbacks.KitchenLayout.SlotCount; i++)
        {
            Assert.That(Footprint(i).Overlaps(aisle), Is.False, "slot " + i + " stays out of the walk-in");
            Assert.That(Footprint(i).yMax, Is.LessThanOrEqualTo(Setbacks.KitchenLayout.HalfDepth - 0.2f), "slot " + i + " stays behind the front posts");
        }
        // And the gap between the side items is a doorway wide enough to walk through.
        float leftEdge = Enumerable.Range(0, 4).Select(Footprint).Where(r => r.center.x < 0f).Max(r => r.xMax);
        float rightEdge = Enumerable.Range(0, 4).Select(Footprint).Where(r => r.center.x > 0f).Min(r => r.xMin);
        Assert.That(rightEdge - leftEdge, Is.GreaterThan(1.5f));
    }

    [Test]
    public void EachSlotFacesTheMiddleOfTheKitchenAtAnyYaw()
    {
        foreach (float yaw in new[] { 0f, 37f, 90f, 200f, 359f })
        {
            for (int i = 0; i < Setbacks.KitchenLayout.SlotCount; i++)
            {
                Vector3 slot = Setbacks.KitchenLayout.SlotAt(Vector3.zero, yaw, i);
                Vector3 facing = Quaternion.Euler(0f, Setbacks.KitchenLayout.SlotYaw(yaw, i), 0f) * Vector3.forward;
                Assert.That(Vector3.Dot(facing, -slot.normalized), Is.GreaterThan(0.6f), $"slot {i} at yaw {yaw}");
            }
        }
    }

    [Test]
    public void ASlotIsFoundAgainFromItsPositionAtAnyYawAndOnlyThere()
    {
        var pivot = new Vector3(40f, 3f, -12f);
        foreach (float yaw in new[] { 0f, 37f, 200f })
        {
            for (int i = 0; i < Setbacks.KitchenLayout.SlotCount; i++)
                Assert.That(Setbacks.KitchenLayout.SlotIndexOf(pivot, yaw, Setbacks.KitchenLayout.SlotAt(pivot, yaw, i)), Is.EqualTo(i));
            Assert.That(Setbacks.KitchenLayout.SlotIndexOf(pivot, yaw, pivot), Is.EqualTo(-1), "the middle of the kitchen is no slot");
            Assert.That(Setbacks.KitchenLayout.SlotIndexOf(pivot, yaw, Setbacks.KitchenLayout.HearthAt(pivot, yaw)), Is.EqualTo(-1), "nor is the hearth");
        }
    }

    [Test]
    public void NearestFreeSlotSkipsTakenSlotsAndSaysMinusOneWhenAllAreTaken()
    {
        Vector3 pivot = Vector3.zero;
        Vector3 near0 = Setbacks.KitchenLayout.SlotAt(pivot, 0f, 0);
        Assert.That(Setbacks.KitchenLayout.NearestFreeSlot(pivot, 0f, near0, null, out float d), Is.EqualTo(0));
        Assert.That(d, Is.LessThan(0.01f));

        var taken = new List<Vector3> { near0 };
        Assert.That(Setbacks.KitchenLayout.NearestFreeSlot(pivot, 0f, near0, taken, out _), Is.EqualTo(1), "slot 1 is the next nearest to slot 0");

        for (int i = 0; i < Setbacks.KitchenLayout.SlotCount; i++)
            taken.Add(Setbacks.KitchenLayout.SlotAt(pivot, 0f, i));
        Assert.That(Setbacks.KitchenLayout.NearestFreeSlot(pivot, 0f, near0, taken, out _), Is.EqualTo(-1));
    }

    // --- The placement rule over a pile list ---

    static object SpotAt(Vector3 point)
    {
        object spot = Activator.CreateInstance(Find("BuildSpot"));
        spot.GetType().GetField("point").SetValue(spot, point);
        return spot;
    }

    static string Refusal(object manager, string kind, Vector3 point, out bool ok)
    {
        var args = new object[] { Kind(kind), SpotAt(point), null };
        ok = (bool)Find("WoodManager").GetMethod("CanBuildAddonAt").Invoke(manager, args);
        return (string)args[2];
    }

    [Test]
    public void WithNoFinishedKitchenAnAddonIsRefusedWithTheNamedReason()
    {
        object site = NewPile("KitchenSite", Vector3.zero, 0f);
        object roofSite = NewPile("KitchenRoofSite", new Vector3(20f, 0f, 0f), 0f);
        object manager = ManagerWith(site, roofSite);
        string reason = Refusal(manager, "Countertop", Setbacks.KitchenLayout.SlotAt(Vector3.zero, 0f, 0), out bool ok);
        Assert.That(ok, Is.False);
        Assert.That(reason, Is.EqualTo("Needs a finished Covered Outdoor Kitchen."), "a site (stage 22) is not a kitchen");
        reason = Refusal(manager, "Countertop", Setbacks.KitchenLayout.SlotAt(new Vector3(20f, 0f, 0f), 0f, 1), out ok);
        Assert.That(reason, Is.EqualTo("Needs a finished Covered Outdoor Kitchen."), "nor is the roof stage (23)");
        Assert.That((bool)Find("WoodManager").GetProperty("AnyFinishedKitchen").GetValue(manager), Is.False);
    }

    [Test]
    public void ASlotOfAFinishedKitchenIsFreeUntilSomethingStandsInItThenThatSpotIsTaken()
    {
        var pivot = new Vector3(10f, 0f, 5f);
        object kitchen = NewPile("Kitchen", pivot, 33f);
        Vector3 slot2 = Setbacks.KitchenLayout.SlotAt(pivot, 33f, 2);
        object manager = ManagerWith(kitchen);

        string reason = Refusal(manager, "Shelving", slot2, out bool ok);
        Assert.That(ok, Is.False, "it gets as far as the Hammer");
        Assert.That(reason, Is.EqualTo("Equip the Hammer to build."), "free slot, no Hammer equipped in a test: the rules before it all pass");

        object counter = NewPile("Countertop", slot2, Setbacks.KitchenLayout.SlotYaw(33f, 2));
        manager = ManagerWith(kitchen, counter);
        reason = Refusal(manager, "Shelving", slot2, out ok);
        Assert.That(ok, Is.False);
        Assert.That(reason, Is.EqualTo("That spot is taken."));

        // Another slot of the same kitchen stays free even with the counter beside it (no 2 m pile spacing against its own kitchen's add-ons).
        reason = Refusal(manager, "Grill", Setbacks.KitchenLayout.SlotAt(pivot, 33f, 3), out ok);
        Assert.That(reason, Is.EqualTo("Equip the Hammer to build."));
    }

    [Test]
    public void AnUnrelatedPileInsideTwoMetresStillRefusesAnAddon()
    {
        Type wood = Find("WoodManager");
        var pivot = Vector3.zero;
        object kitchen = NewPile("Kitchen", pivot, 0f);
        Vector3 slot = Setbacks.KitchenLayout.SlotAt(pivot, 0f, 0);
        object pile = NewPile("WoodStorage", slot + new Vector3(0.5f, 0f, 0f), 0f);
        object manager = ManagerWith(kitchen, pile);
        wood.GetField("minSpacing", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(manager, 2f);

        string reason = Refusal(manager, "Countertop", slot, out bool ok);
        Assert.That(ok, Is.False);
        Assert.That(reason, Is.EqualTo("Too close to another pile."));

        // The kitchen itself, 2 m or nearer, is not "another pile" for its own add-on.
        manager = ManagerWith(kitchen);
        wood.GetField("minSpacing", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(manager, 2f);
        Assert.That(Refusal(manager, "Countertop", slot, out _), Is.EqualTo("Equip the Hammer to build."));
    }

    [Test]
    public void TheKitchenKnowsWhichAddonsStandInItAndNotOthers()
    {
        Type wood = Find("WoodManager");
        var a = Vector3.zero;
        var b = new Vector3(30f, 0f, 0f);
        object kitchenA = NewPile("Kitchen", a, 0f);
        object kitchenB = NewPile("Kitchen", b, 90f);
        object sinkInB = NewPile("SlopSink", Setbacks.KitchenLayout.SlotAt(b, 90f, 3), 0f);
        object manager = ManagerWith(kitchenA, kitchenB, sinkInB);

        Assert.That((bool)wood.GetMethod("KitchenHasAddons").Invoke(manager, new[] { kitchenA }), Is.False);
        Assert.That((bool)wood.GetMethod("KitchenHasAddons").Invoke(manager, new[] { kitchenB }), Is.True, "so B cannot be dismantled until the sink is gone");
        Assert.That(wood.GetMethod("AddonInSlot").Invoke(manager, new object[] { kitchenB, 3 }), Is.SameAs(sinkInB));
        Assert.That(wood.GetMethod("AddonInSlot").Invoke(manager, new object[] { kitchenB, 0 }), Is.Null);
    }

    [Test]
    public void AGrillFireIsToldFromTheHearthByWhereItStands()
    {
        Type wood = Find("WoodManager");
        var pivot = Vector3.zero;
        object kitchen = NewPile("Kitchen", pivot, 0f);
        Vector3 slot = Setbacks.KitchenLayout.SlotAt(pivot, 0f, 2);
        object grill = NewPile("Grill", slot, Setbacks.KitchenLayout.SlotYaw(0f, 2));
        object manager = ManagerWith(kitchen, grill);

        Type fire = Find("CampfireState");
        object grillFire = Activator.CreateInstance(fire);
        fire.GetField("position").SetValue(grillFire, wood.GetMethod("GrillFireSpot").Invoke(null, new[] { grill }));
        object hearthFire = Activator.CreateInstance(fire);
        fire.GetField("position").SetValue(hearthFire, Setbacks.KitchenLayout.HearthAt(pivot, 0f));

        Assert.That((bool)wood.GetMethod("IsGrillFire").Invoke(manager, new[] { grillFire }), Is.True);
        Assert.That((bool)wood.GetMethod("IsGrillFire").Invoke(manager, new[] { hearthFire }), Is.False, "the hearth is never mistaken for a grill");
        Assert.That((float)wood.GetMethod("FireViewScale").Invoke(manager, new[] { hearthFire }), Is.EqualTo(1f));
        Assert.That((float)wood.GetMethod("FireViewScale").Invoke(manager, new[] { grillFire }), Is.LessThan(1f), "a grill's fire is drawn smaller to fit the firebox");
        float apart = Vector3.Distance((Vector3)fire.GetField("position").GetValue(grillFire), (Vector3)fire.GetField("position").GetValue(hearthFire));
        Assert.That(apart, Is.GreaterThan(Setbacks.KitchenLayout.SlotMinHearthGap), "KitchenHearthOf's 0.75 m lookup cannot reach the grill's fire");
    }
}
