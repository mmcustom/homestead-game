using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

// Food spoilage, slice 1 (Food_Spoilage_and_Preservation.md, Mike 2026-11-22). SpoilageRules is pure (Homestead.Economy); the
// container and the item are game types (Assembly-CSharp), reached by reflection as the other tests do.
public class SpoilageTests
{
    static Type Find(string name) =>
        AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(name)).FirstOrDefault(t => t != null);

    [Test]
    public void TheRateFollowsTheDesignTableAndRisesSmoothlyWithHeat()
    {
        Assert.That(SpoilageRules.RateAt(-10), Is.EqualTo(0.1).Within(1e-9), "frozen");
        Assert.That(SpoilageRules.RateAt(2), Is.EqualTo(0.3).Within(1e-9), "near freezing");
        Assert.That(SpoilageRules.RateAt(7), Is.EqualTo(0.6).Within(1e-9), "cool");
        Assert.That(SpoilageRules.RateAt(15), Is.EqualTo(1.0).Within(1e-9), "mild: the shelf life as written");
        Assert.That(SpoilageRules.RateAt(23.5), Is.EqualTo(1.5).Within(1e-9), "warm");
        Assert.That(SpoilageRules.RateAt(35), Is.EqualTo(2.0).Within(1e-9), "hot");
        double last = SpoilageRules.RateAt(-20.5);
        for (double c = -20; c <= 40; c += 0.5)
        {
            double r = SpoilageRules.RateAt(c);
            Assert.That(r, Is.GreaterThanOrEqualTo(last), $"{c} C");
            Assert.That(r - last, Is.LessThan(0.1), $"no jump at {c} C");
            last = r;
        }
    }

    [Test]
    public void TheCacheIsALittleSlowerTheCellarIsSteadyAndDifficultyScalesBoth()
    {
        Assert.That(SpoilageRules.ClockRate(FreshClock.Cache, 20, 1), Is.EqualTo(SpoilageRules.ClockRate(FreshClock.Outdoor, 20, 1) * 0.8).Within(1e-9));
        Assert.That(SpoilageRules.ClockRate(FreshClock.Cellar, 35, 1), Is.EqualTo(0.3).Within(1e-9));
        Assert.That(SpoilageRules.ClockRate(FreshClock.Cellar, -15, 1), Is.EqualTo(0.3).Within(1e-9));
        Assert.That(SpoilageRules.ClockRate(FreshClock.Outdoor, 15, 1.3), Is.EqualTo(1.3).Within(1e-9), "Homesteader spoils faster");
        Assert.That(SpoilageRules.ClockRate(FreshClock.Outdoor, 15, 0.75), Is.EqualTo(0.75).Within(1e-9), "Settler slower");
    }

    [Test]
    public void ABatchSpoilsWhenItsClockHasRunItsShelfLife()
    {
        int stamp = SpoilageRules.Stamp(1000);
        Assert.That(stamp % SpoilageRules.StampStepHours, Is.EqualTo(0));
        Assert.That(Math.Abs(stamp - 1000), Is.LessThanOrEqualTo(SpoilageRules.StampStepHours / 2));
        Assert.That(SpoilageRules.IsSpoiled(1000, 1000 + 3 * 24 - 1, 3), Is.False);
        Assert.That(SpoilageRules.IsSpoiled(1000, 1000 + 3 * 24, 3), Is.True, "venison: 3 days at mild temperatures");
        Assert.That(SpoilageRules.IsSpoiled(1000, 1_000_000, 0), Is.False, "no shelf life: never spoils");
        Assert.That(SpoilageRules.Stamp(0), Is.EqualTo(1), "never 0, which a pile reads as no stamp");
    }

    [Test]
    public void MovingBetweenClocksKeepsTheAgeExactly()
    {
        int stamp = 500;
        int inCellar = SpoilageRules.Convert(stamp, 560, 220);
        Assert.That(SpoilageRules.AgeHours(inCellar, 220), Is.EqualTo(60).Within(0.5));
        int back = SpoilageRules.Convert(inCellar, 220, 560);
        Assert.That(back, Is.EqualTo(stamp).Within(1));
    }

    [Test]
    public void OldFoodCarriesARisingChanceOfSicknessOnlyInItsSecondHalf()
    {
        Assert.That(SpoilageRules.IllnessChance(0.0), Is.EqualTo(0));
        Assert.That(SpoilageRules.IllnessChance(0.5), Is.EqualTo(0));
        Assert.That(SpoilageRules.IllnessChance(0.75), Is.EqualTo(0.075).Within(1e-9));
        Assert.That(SpoilageRules.IllnessChance(0.999), Is.EqualTo(0.15).Within(0.001));
        Assert.That(SpoilageRules.Combine(0.2, 0.0), Is.EqualTo(0.2).Within(1e-9), "raw meat alone");
        Assert.That(SpoilageRules.Combine(0.2, 0.1), Is.EqualTo(0.28).Within(1e-9));
        Assert.That(SpoilageRules.StageOf(0.3), Is.EqualTo(Freshness.Fresh));
        Assert.That(SpoilageRules.StageOf(0.6), Is.EqualTo(Freshness.Aging));
        Assert.That(SpoilageRules.StageOf(0.9), Is.EqualTo(Freshness.GoingOff));
    }

    [Test]
    public void TheTimeLeftReadsPlainly()
    {
        Assert.That(SpoilageRules.LeftText(2.6, 1), Is.EqualTo("3 days left"));
        Assert.That(SpoilageRules.LeftText(1.2, 1), Is.EqualTo("1 day left"));
        Assert.That(SpoilageRules.LeftText(0.25, 1), Is.EqualTo("6 hours left"));
        Assert.That(SpoilageRules.LeftText(0.01, 1), Is.EqualTo("less than an hour left"));
        Assert.That(SpoilageRules.LeftText(30, 0.1), Is.EqualTo("keeping while frozen"));
        // a warm day halves what is left in real days
        Assert.That(SpoilageRules.DaysLeft(0, 24, 3, 2.0), Is.EqualTo(1.0).Within(1e-9));
    }

    [Test]
    public void AnOlderSaveKeepsItsFoodsAgeButNothingSpoilsOnLoad()
    {
        // ten days old berries (3-day life) come in at half their life, not spoiled
        int stamp = SpoilageRules.MigrateDayStamp(90, 100, 2400, 3);
        Assert.That(SpoilageRules.FractionUsed(stamp, 2400, 3), Is.EqualTo(0.5).Within(0.01));
        // one day old nuts (60-day life) keep that one day
        stamp = SpoilageRules.MigrateDayStamp(99, 100, 2400, 60);
        Assert.That(SpoilageRules.AgeHours(stamp, 2400), Is.EqualTo(24).Within(0.5));
    }

    // ---- Slice 2: the Compost Bin ----

    [Test]
    public void CompostTakesFourRotAndTwoGrassABatchAndStopsWhenFrozen()
    {
        Assert.That(CompostRules.Batches(4, 2), Is.EqualTo(1));
        Assert.That(CompostRules.Batches(9, 10), Is.EqualTo(2), "two whole batches of Rot");
        Assert.That(CompostRules.Batches(20, 3), Is.EqualTo(1), "one batch of grass");
        Assert.That(CompostRules.Batches(400, 400), Is.EqualTo(CompostRules.MaxBatchesPerJob));
        Assert.That(CompostRules.RateAt(-5), Is.EqualTo(0));
        Assert.That(CompostRules.RateAt(0), Is.EqualTo(0));
        Assert.That(CompostRules.RateAt(2.5), Is.EqualTo(0.2).Within(1e-9));
        Assert.That(CompostRules.RateAt(20), Is.EqualTo(1));
        Assert.That(CompostRules.HoursPerJob, Is.EqualTo(7 * 24), "about a week at mild to warm temperatures (Mike 2026-11-22)");
        Assert.That(CompostRules.Accepts(SpoilageRules.RotId) && CompostRules.Accepts("tall_grass"), Is.True);
        Assert.That(CompostRules.Accepts("venison"), Is.False);
        Assert.That(CompostRules.Status(2, 24 * 9, 1.0), Is.EqualTo("composting 2 batches, about 9 days left"));
        Assert.That(CompostRules.Status(1, 100, 0), Is.EqualTo("composting 1 batch, stopped while frozen"));
        Assert.That(CompostRules.Status(0, 100, 1), Is.Null);
        // an idle bin says what it still needs (Mike 2026-11-22: 3 Rot and 6 Tall Grass said nothing)
        Assert.That(CompostRules.IdleStatus(3, 6), Is.EqualTo("needs 1 more Rot to start"));
        Assert.That(CompostRules.IdleStatus(0, 0), Is.EqualTo("needs 4 more Rot and 2 more Tall Grass to start"));
        Assert.That(CompostRules.IdleStatus(8, 1), Is.EqualTo("needs 1 more Tall Grass to start"));
        Assert.That(CompostRules.IdleStatus(4, 2), Is.EqualTo("starts composting within the hour"));
    }

    [Test]
    public void TheCompostBinIsBuiltFromSticksAndCordageAndSellsFertilizer()
    {
        Type pk = Find("PileKind"), wm = Find("WoodManager");
        object bin = Enum.Parse(pk, "CompostBin");
        Assert.That(((Array)wm.GetField("Buildable").GetValue(null)).Cast<object>(), Does.Contain(bin));
        Assert.That(wm.GetMethod("PileName").Invoke(null, new[] { bin }), Is.EqualTo("Compost Bin"));
        Assert.That(wm.GetMethod("NeedsHammerToBuild").Invoke(null, new[] { bin }), Is.True);
        Assert.That(MarketCatalog.Rows.Any(r => r.itemId == CompostRules.FertilizerId && r.kind == MarketKind.Bought && r.baseCents == 50), Is.True);
        Assert.That(KnowHowRules.ForItem(SpoilageRules.RotId), Does.Contain(KnowHowRules.Spoilage));
    }

    [Test]
    public void AContainerTurnsSpoiledStacksIntoTheSameNumberOfRot()
    {
        Type itemType = Find("ItemDefinition"), container = Find("InventoryContainer"), category = Find("ItemCategory");
        object resource = Enum.Parse(category, "Resource");
        var create = itemType.GetMethod("Create");
        var berries = create.Invoke(null, new object[] { "test_berries", resource, 0.3f, 3f, "Berries", 5f, 0f, 0f, false });
        Type db = Find("ItemDatabase");
        object rot = db.GetMethod("Get").Invoke(null, new object[] { SpoilageRules.RotId });
        Assert.That(rot, Is.Not.Null, "the Rot item exists (Resources/Items/Resources/rot.asset)");
        db.GetMethod("Register").Invoke(null, new[] { berries });
        try
        {
            object pack = Activator.CreateInstance(container, "test", 100f);
            container.GetMethod("Add").Invoke(pack, new[] { berries, (object)4, (object)100 });
            container.GetMethod("Add").Invoke(pack, new[] { berries, (object)2, (object)170 });
            // (item, stamp) => SpoilageRules.IsSpoiled(stamp, 172, 3), typed as the container wants it
            var method = container.GetMethod("ReplaceSpoiled");
            Type funcType = method.GetParameters()[0].ParameterType;
            var item = System.Linq.Expressions.Expression.Parameter(itemType, "item");
            var stamp = System.Linq.Expressions.Expression.Parameter(typeof(int), "stamp");
            var body = System.Linq.Expressions.Expression.Call(typeof(SpoilageRules).GetMethod("IsSpoiled"), stamp,
                System.Linq.Expressions.Expression.Constant(172.0), System.Linq.Expressions.Expression.Constant(3.0));
            Delegate spoiled = System.Linq.Expressions.Expression.Lambda(funcType, body, item, stamp).Compile();
            var lost = (System.Collections.IList)method.Invoke(pack, new[] { spoiled, rot });
            Assert.That(lost.Count, Is.EqualTo(1), "only the older batch");
            Assert.That(container.GetMethod("Count").Invoke(pack, new object[] { "test_berries" }), Is.EqualTo(2));
            Assert.That(container.GetMethod("Count").Invoke(pack, new object[] { SpoilageRules.RotId }), Is.EqualTo(4));
        }
        finally
        {
            db.GetMethod("ResetCache", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)?.Invoke(null, null);
            UnityEngine.Object.DestroyImmediate((UnityEngine.Object)berries);
        }
    }
}
