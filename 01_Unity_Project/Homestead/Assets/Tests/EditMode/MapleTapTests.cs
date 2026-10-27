using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

// EditMode tests for Maple B, taps in the world: the pure rules (TapRules, Homestead.Economy) for placing, checking, the hourly step and the
// bucket's words, plus the saved tap state and the two craft recipes, which are in the main assembly and reached by reflection on throwaway
// objects as the other tests do.
public class MapleTapTests
{
    const int Winter = 3, Summer = 1;

    static Type Find(string name) =>
        AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => { try { return a.GetTypes(); } catch (Exception) { return new Type[0]; } })
                 .First(t => t.Name == name);

    static MapleRules.Bucket Bucket(float litres, float age = 0f, bool sour = false) =>
        new MapleRules.Bucket { litres = litres, ageHours = age, sour = sour };

    // --- Putting a tap up ---

    [Test]
    public void ATapGoesUpOnAMapleWithASpileAndABucket()
    {
        Assert.That(TapRules.CanPlace(true, false, 0, true, true), Is.EqualTo(TapRules.PlaceResult.Ok));
        Assert.That(TapRules.CanPlace(true, false, 11, true, true), Is.EqualTo(TapRules.PlaceResult.Ok), "the twelfth tap is allowed");
    }

    [Test]
    public void EveryReasonForRefusingHasAMessage()
    {
        Assert.That(TapRules.CanPlace(false, false, 0, true, true), Is.EqualTo(TapRules.PlaceResult.NotAMaple));
        Assert.That(TapRules.CanPlace(true, true, 0, true, true), Is.EqualTo(TapRules.PlaceResult.AlreadyTapped), "one tap per tree");
        Assert.That(TapRules.CanPlace(true, false, 12, true, true), Is.EqualTo(TapRules.PlaceResult.TooManyTaps), "the soft cap is 12");
        Assert.That(TapRules.CanPlace(true, false, 0, false, true), Is.EqualTo(TapRules.PlaceResult.NoSpile));
        Assert.That(TapRules.CanPlace(true, false, 0, true, false), Is.EqualTo(TapRules.PlaceResult.NoBucket));
        foreach (TapRules.PlaceResult result in Enum.GetValues(typeof(TapRules.PlaceResult)).Cast<TapRules.PlaceResult>().Where(r => r != TapRules.PlaceResult.Ok))
            Assert.That(TapRules.PlaceMessage(result), Is.Not.Empty, result.ToString());
        Assert.That(TapRules.PlaceMessage(TapRules.PlaceResult.Ok), Is.Empty);
        Assert.That(TapRules.PlaceMessage(TapRules.PlaceResult.TooManyTaps), Does.Contain("12"));
    }

    [Test]
    public void TheReasonsAreCheckedInOrder()
    {
        // Not a maple beats everything; already tapped beats the cap and the missing parts.
        Assert.That(TapRules.CanPlace(false, true, 12, false, false), Is.EqualTo(TapRules.PlaceResult.NotAMaple));
        Assert.That(TapRules.CanPlace(true, true, 12, false, false), Is.EqualTo(TapRules.PlaceResult.AlreadyTapped));
        Assert.That(TapRules.CanPlace(true, false, 12, false, false), Is.EqualTo(TapRules.PlaceResult.TooManyTaps));
        Assert.That(TapRules.CanPlace(true, false, 0, false, false), Is.EqualTo(TapRules.PlaceResult.NoSpile));
    }

    // --- Checking ---

    [Test]
    public void APackWithRoomTakesAllTheWholeLitres()
    {
        MapleRules.Collected got = TapRules.CollectUpTo(Bucket(7.6f, 20f), 50);
        Assert.That((got.litres, got.spoiled), Is.EqualTo((7, 0)));
        Assert.That(got.bucket.litres, Is.EqualTo(0.6f).Within(1e-4f));
        Assert.That(got.bucket.ageHours, Is.EqualTo(20f), "the fraction left is the same sap, so it keeps its age");
    }

    [Test]
    public void ANearlyFullPackTakesWhatFitsAndLeavesTheRestInTheBucket()
    {
        MapleRules.Collected got = TapRules.CollectUpTo(Bucket(8f, 30f), 3);
        Assert.That(got.litres, Is.EqualTo(3));
        Assert.That(got.bucket.litres, Is.EqualTo(5f).Within(1e-4f));
        Assert.That(got.bucket.ageHours, Is.EqualTo(30f), "still the same old sap");
        Assert.That(got.bucket.sour, Is.False);
    }

    [Test]
    public void AFullPackTakesNothingAndChangesNothing()
    {
        MapleRules.Collected got = TapRules.CollectUpTo(Bucket(6.4f, 12f), 0);
        Assert.That(got.litres, Is.EqualTo(0));
        Assert.That(got.bucket.litres, Is.EqualTo(6.4f).Within(1e-4f));
        Assert.That(got.bucket.ageHours, Is.EqualTo(12f));
    }

    [Test]
    public void TakingEverythingEmptiesTheBucketAndResetsItsAge()
    {
        MapleRules.Collected got = TapRules.CollectUpTo(Bucket(5f, 40f), 99);
        Assert.That(got.litres, Is.EqualTo(5));
        Assert.That(got.bucket.IsEmpty, Is.True);
        Assert.That(got.bucket.ageHours, Is.EqualTo(0f));
        Assert.That(TapRules.CollectUpTo(Bucket(0.4f), 99).litres, Is.EqualTo(0), "under a litre is nothing to take yet");
        Assert.That(TapRules.CollectUpTo(Bucket(0.4f), 99).bucket.litres, Is.EqualTo(0.4f).Within(1e-4f));
    }

    [Test]
    public void ASourBucketIsPouredOutEvenWithAFullPack()
    {
        MapleRules.Collected got = TapRules.CollectUpTo(Bucket(6f, 48f, true), 0);
        Assert.That((got.litres, got.spoiled), Is.EqualTo((0, 6)));
        Assert.That(got.bucket.IsEmpty, Is.True);
        Assert.That(got.bucket.sour, Is.False, "ready for fresh sap");
    }

    [Test]
    public void TheCheckMessagesSayWhatHappened()
    {
        MapleRules.Bucket full = Bucket(8f, 10f);
        Assert.That(TapRules.CollectMessage(full, TapRules.CollectUpTo(full, 99)), Is.EqualTo("Collected 8 L of sap."));
        Assert.That(TapRules.CollectMessage(full, TapRules.CollectUpTo(full, 3)), Does.Contain("Took 3 L").And.Contain("other 5 L"));
        Assert.That(TapRules.CollectMessage(full, TapRules.CollectUpTo(full, 0)), Does.Contain("No room"));
        MapleRules.Bucket empty = Bucket(0.3f);
        Assert.That(TapRules.CollectMessage(empty, TapRules.CollectUpTo(empty, 99)), Does.Contain("No sap"));
        MapleRules.Bucket sour = Bucket(5.5f, 48f, true);
        Assert.That(TapRules.CollectMessage(sour, TapRules.CollectUpTo(sour, 0)), Does.Contain("soured").And.Contain("5 L"));
    }

    // --- The hour at a tap ---

    [Test]
    public void AFlowHourFillsABucketByTheTreesSizeAndAnyOtherHourDoesNot()
    {
        MapleRules.Bucket small = TapRules.TapHour(Bucket(0f), true, 0.8f);
        MapleRules.Bucket large = TapRules.TapHour(Bucket(0f), true, 1.35f);
        Assert.That(small.litres, Is.EqualTo(0.7f * 0.85f).Within(1e-4f));
        Assert.That(large.litres, Is.EqualTo(0.7f * 1.2f).Within(1e-4f));
        Assert.That(TapRules.TapHour(Bucket(1f, 3f), false, 1.3f).litres, Is.EqualTo(1f), "no sap on an hour that is not a flow hour");
        Assert.That(TapRules.TapHour(Bucket(1f, 3f), false, 1.3f).ageHours, Is.EqualTo(4f), "but the sap ages");
    }

    [Test]
    public void AFreezeThawDayFillsTapsAndAnUntendedTapSoursAfterTwoDays()
    {
        // A cold night then a warm day, over and over, as the hourly tick sees it.
        float[] day = { -4, -4, -3, -3, -2, -2, -1, 0, 1, 3, 5, 6, 7, 7, 6, 5, 3, 2, 1, 0, -1, -2, -3, -4 };
        float since = MapleRules.FreezeMemoryCap;
        MapleRules.Bucket bucket = Bucket(0f);
        int firstFull = -1;
        for (int hour = 0; hour < 24 * 4; hour++)
        {
            float t = day[hour % 24];
            since = MapleRules.NextHoursSinceFreeze(since, t);
            bool flow = MapleRules.IsFlowHour(Winter, t, since);
            bucket = TapRules.TapHour(bucket, flow, 1.25f);
            if (firstFull < 0 && bucket.IsFull)
                firstFull = hour;
        }
        Assert.That(firstFull, Is.InRange(24, 48), "full on the second day");
        Assert.That(bucket.sour, Is.True, "four days untended: sour");
    }

    [Test]
    public void ReadyIsTheHourABucketFirstReachesFourLitres()
    {
        Assert.That(TapRules.BecameReady(3.5f, 4.2f), Is.True);
        Assert.That(TapRules.BecameReady(3.9f, 4f), Is.True);
        Assert.That(TapRules.BecameReady(4f, 4.7f), Is.False, "already ready");
        Assert.That(TapRules.BecameReady(2f, 3.9f), Is.False);
        Assert.That(TapRules.ReadyLitres, Is.EqualTo(4f));
    }

    // --- The bucket in words ---

    [Test]
    public void TheStatusSaysHowMuchHowFreshAndWhetherItIsFlowing()
    {
        Assert.That(TapRules.Status(Bucket(0f), true), Is.EqualTo("empty, flowing"));
        Assert.That(TapRules.Status(Bucket(0f), false), Is.EqualTo("empty, not flowing"));
        Assert.That(TapRules.Status(Bucket(3.9f, 5f), true), Is.EqualTo("3.9 L, fresh, flowing"));
        Assert.That(TapRules.Status(Bucket(6f, 38f), false), Is.EqualTo("6 L, souring in 10 h, not flowing"));
        Assert.That(TapRules.Status(Bucket(8f, 10f), true), Is.EqualTo("full, 8 L, fresh, flowing"));
        Assert.That(TapRules.Status(Bucket(5f, 48f, true), true), Is.EqualTo("sour, empty it"));
        Assert.That(TapRules.Status(Bucket(5f, 47.5f), true), Does.Contain("souring in 1 h"), "never says 0 hours");
    }

    // --- A tree that came down, or moved ---

    [Test]
    public void AFelledTapGivesBackOneSpileAndOneBucket()
    {
        Assert.That((TapRules.SpilesBack, TapRules.BucketsBack), Is.EqualTo((1, 1)));
        Assert.That(TapRules.PackRefundMetres, Is.EqualTo(10f), "inside this the refund goes to the pack; beyond it, to the ground");
    }

    [Test]
    public void TheTapMustStillSitOnItsTreeAfterALoad()
    {
        Assert.That(TapRules.PositionMatches(0.3f, 0.4f), Is.True, "right at the trunk");
        Assert.That(TapRules.PositionMatches(1f, 0.4f), Is.True, "the edge of the match");
        Assert.That(TapRules.PositionMatches(1.1f, 0.4f), Is.False, "a different tree now stands at that index");
        Assert.That(TapRules.SearchMetres, Is.GreaterThan(TapRules.MatchMetres));
    }

    // --- Saved state (TapState, TapSaveData and TapManager are in the main assembly) ---

    static object NewTap(int id, int tree, float litres, float age, bool sour)
    {
        Type type = Find("TapState");
        object tap = Activator.CreateInstance(type);
        type.GetField("id").SetValue(tap, id);
        type.GetField("treeIndex").SetValue(tap, tree);
        type.GetField("position").SetValue(tap, new Vector3(12.5f, 3f, -40.25f));
        type.GetField("yaw").SetValue(tap, 135f);
        type.GetField("heightScale").SetValue(tap, 1.3f);
        type.GetField("litres").SetValue(tap, litres);
        type.GetField("ageHours").SetValue(tap, age);
        type.GetField("sour").SetValue(tap, sour);
        return tap;
    }

    static object NewSave(IEnumerable<object> taps, float since, int nextId)
    {
        Type save = Find("TapSaveData"), tapType = Find("TapState");
        object data = Activator.CreateInstance(save);
        object list = Activator.CreateInstance(typeof(List<>).MakeGenericType(tapType));
        foreach (object tap in taps)
            list.GetType().GetMethod("Add").Invoke(list, new[] { tap });
        save.GetField("taps").SetValue(data, list);
        save.GetField("hoursSinceFreeze").SetValue(data, since);
        save.GetField("nextId").SetValue(data, nextId);
        return data;
    }

    [Test]
    public void ATapSaveRoundTripsEveryField()
    {
        object data = NewSave(new[] { NewTap(4, 1234, 5.5f, 20f, false), NewTap(9, 77, 8f, 48f, true) }, 13f, 10);
        object back = JsonUtility.FromJson(JsonUtility.ToJson(data), Find("TapSaveData"));

        Type save = Find("TapSaveData"), tapType = Find("TapState");
        var list = ((System.Collections.IEnumerable)save.GetField("taps").GetValue(back)).Cast<object>().ToList();
        Assert.That(list.Count, Is.EqualTo(2));
        Assert.That(tapType.GetField("id").GetValue(list[0]), Is.EqualTo(4));
        Assert.That(tapType.GetField("treeIndex").GetValue(list[0]), Is.EqualTo(1234));
        Assert.That(tapType.GetField("position").GetValue(list[0]), Is.EqualTo(new Vector3(12.5f, 3f, -40.25f)));
        Assert.That(tapType.GetField("yaw").GetValue(list[0]), Is.EqualTo(135f));
        Assert.That(tapType.GetField("heightScale").GetValue(list[0]), Is.EqualTo(1.3f));
        Assert.That(tapType.GetField("litres").GetValue(list[0]), Is.EqualTo(5.5f));
        Assert.That(tapType.GetField("ageHours").GetValue(list[0]), Is.EqualTo(20f));
        Assert.That(tapType.GetField("sour").GetValue(list[0]), Is.EqualTo(false));
        Assert.That(tapType.GetField("sour").GetValue(list[1]), Is.EqualTo(true));
        Assert.That(save.GetField("hoursSinceFreeze").GetValue(back), Is.EqualTo(13f));
        Assert.That(save.GetField("nextId").GetValue(back), Is.EqualTo(10));
    }

    sealed class Manager : IDisposable
    {
        public readonly GameObject Go = new GameObject("TapManager test");
        public readonly Component Component;
        public readonly Type Type = Find("TapManager");

        public Manager()
        {
            Component = Go.AddComponent(Type); // Awake does not run in edit mode, so no singleton and no scene is touched
        }

        public object Call(string method, params object[] args) => Type.GetMethod(method).Invoke(Component, args);
        public int Count => (int)Type.GetProperty("TapCount").GetValue(Component);
        public object Capture() => Call("CaptureState");
        public void Dispose() => UnityEngine.Object.DestroyImmediate(Go);
    }

    [Test]
    public void ARestoredSaveBringsBackItsTapsAndNeverReusesAnId()
    {
        using (var m = new Manager())
        {
            m.Call("RestoreState", NewSave(new[] { NewTap(4, 1234, 5.5f, 20f, false), NewTap(9, 77, 8f, 48f, true) }, 13f, 2));
            Assert.That(m.Count, Is.EqualTo(2));
            object captured = m.Capture();
            Type save = Find("TapSaveData");
            Assert.That((int)save.GetField("nextId").GetValue(captured), Is.GreaterThanOrEqualTo(10), "past the highest id even if nextId was behind");
            Assert.That((float)save.GetField("hoursSinceFreeze").GetValue(captured), Is.EqualTo(13f));
        }
    }

    [Test]
    public void TheFreezeMemoryIsKeptInsideItsRangeOnLoad()
    {
        using (var m = new Manager())
        {
            Type save = Find("TapSaveData");
            m.Call("RestoreState", NewSave(new object[0], 999f, 1));
            Assert.That((float)save.GetField("hoursSinceFreeze").GetValue(m.Capture()), Is.EqualTo(MapleRules.FreezeMemoryCap));
            m.Call("RestoreState", NewSave(new object[0], -5f, 1));
            Assert.That((float)save.GetField("hoursSinceFreeze").GetValue(m.Capture()), Is.EqualTo(0f));
        }
    }

    [Test]
    public void AnOlderSaveWithNoTapsSectionLoadsWithNoTapsAndNoFreezeMemory()
    {
        using (var m = new Manager())
        {
            // Restoring something first, then the reset every load starts with: a save with no taps section never inherits the last game's.
            m.Call("RestoreState", NewSave(new[] { NewTap(1, 5, 2f, 3f, false) }, 7f, 2));
            Assert.That(m.Count, Is.EqualTo(1));
            m.Call("ResetTaps");
            Assert.That(m.Count, Is.EqualTo(0));
            object captured = m.Capture();
            Type save = Find("TapSaveData");
            Assert.That((float)save.GetField("hoursSinceFreeze").GetValue(captured), Is.EqualTo(MapleRules.FreezeMemoryCap), "no recent freeze known");
            Assert.That((int)save.GetField("nextId").GetValue(captured), Is.EqualTo(1));
        }
    }

    [Test]
    public void AWorldSectionWithNoTapListRestoresAsEmpty()
    {
        using (var m = new Manager())
        {
            object empty = JsonUtility.FromJson("{}", Find("TapSaveData"));
            m.Call("RestoreState", empty);
            Assert.That(m.Count, Is.EqualTo(0));
        }
    }

    [Test]
    public void ATreeFellingThatHasNoTapIsIgnored()
    {
        using (var m = new Manager())
        {
            m.Call("RestoreState", NewSave(new[] { NewTap(1, 5, 2f, 3f, false) }, 7f, 2));
            m.Call("OnTreeFelled", 99999);
            Assert.That(m.Count, Is.EqualTo(1), "a different tree");
        }
    }

    // --- The recipes ---

    static (string id, int quantity)[] Recipe(string output)
    {
        Type crafting = Find("Crafting");
        Array recipes = (Array)crafting.GetField("Recipes").GetValue(null);
        Type recipeType = recipes.GetType().GetElementType();
        object recipe = recipes.Cast<object>().Single(r => (string)recipeType.GetField("outputId").GetValue(r) == output);
        Assert.That(recipeType.GetField("outputCount").GetValue(recipe), Is.EqualTo(1), output);
        Array ingredients = (Array)recipeType.GetField("ingredients").GetValue(recipe);
        Type ingredient = ingredients.GetType().GetElementType();
        return ingredients.Cast<object>().Select(i => ((string)ingredient.GetField("itemId").GetValue(i), (int)ingredient.GetField("quantity").GetValue(i))).ToArray();
    }

    [Test]
    public void TheSpileIsThreeSticks()
    {
        Assert.That(Recipe("spile"), Is.EqualTo(new[] { ("sticks", 3) }));
    }

    [Test]
    public void TheSapBucketIsALogThreeBranchesAndACordage()
    {
        Assert.That(Recipe("sap_bucket"), Is.EqualTo(new[] { ("logs", 1), ("branches", 3), ("cordage", 1) }));
    }

    [Test]
    public void SyrupAndSapCannotBeCraftedAndThereIsNothingToBoilYet()
    {
        Type crafting = Find("Crafting");
        Array recipes = (Array)crafting.GetField("Recipes").GetValue(null);
        Type recipeType = recipes.GetType().GetElementType();
        var outputs = recipes.Cast<object>().Select(r => (string)recipeType.GetField("outputId").GetValue(r)).ToList();
        Assert.That(outputs, Does.Not.Contain("maple_sap"));
        Assert.That(outputs, Does.Not.Contain("maple_syrup"));
        Assert.That(outputs, Does.Contain("spile"));
        Assert.That(outputs, Does.Contain("sap_bucket"));
    }
}
