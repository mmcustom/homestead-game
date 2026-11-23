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

    // ---- Slice 3: the Drying Rack ----

    [Test]
    public void TheRackDriesFruitOneForOneAndCutsMeatAndFishIntoStrips()
    {
        Assert.That(DryingRules.TryFind("blackberries", out string berries, out int fruitHours), Is.True);
        Assert.That((berries, fruitHours), Is.EqualTo((DryingRules.DriedBerriesId, 24)));
        Assert.That(DryingRules.TryFind("pawpaw", out string apples, out _) && apples == DryingRules.DriedApplesId, Is.True);
        Assert.That(DryingRules.TryFind("dryads_saddle", out string mushrooms, out _) && mushrooms == DryingRules.DriedMushroomsId, Is.True);
        Assert.That(DryingRules.TryFind("venison", out string meat, out int meatHours), Is.True);
        Assert.That((meat, meatHours), Is.EqualTo((DryingRules.DriedMeatId, 36)));
        Assert.That(DryingRules.TryFind("catfish", out string fish, out _) && fish == DryingRules.DriedFishId, Is.True);
        Assert.That(DryingRules.TryFind("cooked_venison", out _, out _), Is.False, "cooked food is not dried");
        Assert.That(DryingRules.TryFind("hickory_nuts", out _, out _), Is.False);

        Assert.That(DryingRules.YieldOf("raspberries", 0.3), Is.EqualTo(1));
        Assert.That(DryingRules.YieldOf("venison", 2.5), Is.EqualTo(8), "two strips per dollar of its cooked price ($4)");
        Assert.That(DryingRules.YieldOf("chicken_meat", 0.8), Is.EqualTo(4));
        Assert.That(DryingRules.YieldOf("small_game_meat", 0.4), Is.EqualTo(4));
        Assert.That(DryingRules.YieldOf("bluegill", 0.3), Is.EqualTo(4));
        Assert.That(DryingRules.YieldOf("bass", 0.6), Is.EqualTo(6));
        Assert.That(DryingRules.YieldOf("crappie", 0.5), Is.EqualTo(4));
        Assert.That(DryingRules.YieldOf("cattail", 0.3), Is.EqualTo(0), "not a rack food");

        Assert.That(DryingRules.Accepts("morel") && DryingRules.Accepts(DryingRules.DriedFishId), Is.True);
        Assert.That(DryingRules.Accepts("logs"), Is.False);
    }

    [Test]
    public void DryingCountsOnlyDryDaylightAndTheRackAgesFoodAtHalfRate()
    {
        Assert.That(DryingRules.DriesNow(true, false), Is.True);
        Assert.That(DryingRules.DriesNow(false, false), Is.False, "night");
        Assert.That(DryingRules.DriesNow(true, true), Is.False, "rain or snow");
        Assert.That(SpoilageRules.ClockRate(FreshClock.Rack, 15, 1), Is.EqualTo(0.5).Within(1e-9));
        Assert.That(SpoilageRules.ClockRate(FreshClock.Rack, 30, 1.3), Is.EqualTo(SpoilageRules.ClockRate(FreshClock.Outdoor, 30, 1.3) * 0.5).Within(1e-9));

        Assert.That(DryingRules.HoursLeft(24, 10.5f), Is.EqualTo(14));
        Assert.That(DryingRules.HoursLeft(24, 30f), Is.EqualTo(0));
        Assert.That(DryingRules.RowText(14), Is.EqualTo("drying, 14 daylight hours to go"));
        Assert.That(DryingRules.RowText(1), Is.EqualTo("drying, 1 daylight hour to go"));
        Assert.That(DryingRules.Status(6, true, false, false), Is.EqualTo("drying 6 pieces"));
        Assert.That(DryingRules.Status(1, true, false, false), Is.EqualTo("drying 1 piece"));
        Assert.That(DryingRules.Status(6, false, false, false), Is.EqualTo("drying paused for the night"));
        Assert.That(DryingRules.Status(6, true, true, false), Is.EqualTo("drying paused while it rains"));
        Assert.That(DryingRules.Status(6, true, false, true), Is.EqualTo("drying paused while it snows"));
        Assert.That(DryingRules.Status(0, true, false, false), Is.Null);
    }

    [Test]
    public void TheDryingRackIsBuiltWithTheHammerAndItsFoodSellsAndUnlocksKnowHow()
    {
        Type pk = Find("PileKind"), wm = Find("WoodManager");
        object rack = Enum.Parse(pk, "DryingRack");
        Assert.That((int)rack, Is.EqualTo(46), "appended after the Compost Bin");
        Assert.That(((Array)wm.GetField("Buildable").GetValue(null)).Cast<object>(), Does.Contain(rack));
        Assert.That(wm.GetMethod("PileName").Invoke(null, new[] { rack }), Is.EqualTo("Drying Rack"));
        Assert.That(wm.GetMethod("NeedsHammerToBuild").Invoke(null, new[] { rack }), Is.True);
        foreach (string id in DryingRules.DriedIds)
        {
            Assert.That(MarketCatalog.Rows.Any(r => r.itemId == id && r.kind == MarketKind.Bought), Is.True, id);
            Assert.That(KnowHowRules.ForItem(id), Does.Contain(KnowHowRules.Drying), id);
        }
        Assert.That(MarketCatalog.Rows.Single(r => r.itemId == DryingRules.DriedMeatId).baseCents, Is.EqualTo(80));
    }

    [Test]
    public void FoodHungLaterOnTheRackStartsItsOwnBatch()
    {
        Type state = Find("WoodPileState"), stackType = Find("WoodStack");
        object pile = Activator.CreateInstance(state);
        state.GetField("kind").SetValue(pile, Enum.Parse(Find("PileKind"), "DryingRack"));
        var add = state.GetMethod("Add");
        add.Invoke(pile, new object[] { "venison", 1, 600 });
        var contents = (System.Collections.IList)state.GetField("contents").GetValue(pile);
        stackType.GetField("dried").SetValue(contents[0], 12f);
        add.Invoke(pile, new object[] { "venison", 1, 600 });
        Assert.That(contents.Count, Is.EqualTo(2), "the half-dried Venison keeps its own progress");
        add.Invoke(pile, new object[] { "venison", 1, 600 });
        Assert.That(contents.Count, Is.EqualTo(2), "two fresh ones hung together share a batch");
    }

    // ---- Slice 4: salt curing ----

    [Test]
    public void EachRawPieceTakesOneSaltPerTwoKilogramsAndBecomesOneSaltPiece()
    {
        Assert.That(SaltRules.TryFind("venison", out string meat) && meat == SaltRules.SaltMeatId, Is.True);
        Assert.That(SaltRules.TryFind("chicken_meat", out string chicken) && chicken == SaltRules.SaltMeatId, Is.True);
        Assert.That(SaltRules.TryFind("bluegill", out string fish) && fish == SaltRules.SaltFishId, Is.True);
        Assert.That(SaltRules.Salts("blackberries"), Is.False, "fruit is dried, not salted");
        Assert.That(SaltRules.Salts("cooked_venison"), Is.False);
        Assert.That(SaltRules.SaltFor(2.5), Is.EqualTo(2), "a Venison");
        Assert.That(SaltRules.SaltFor(2.0), Is.EqualTo(1));
        Assert.That(SaltRules.SaltFor(0.3), Is.EqualTo(1), "at least one");
        Assert.That(SaltRules.PiecesFor(4, 5, 2.5), Is.EqualTo(2), "5 Salt salts two Venison");
        Assert.That(SaltRules.PiecesFor(4, 10, 0.6), Is.EqualTo(4), "no more than are carried");
        Assert.That(SaltRules.PiecesFor(3, 0, 0.6), Is.EqualTo(0));
        Assert.That(SaltRules.RowInfo("small_game_meat", 4, 0.4), Is.EqualTo("1 Salt each, makes 2; 4 carried"));
        Assert.That(SaltRules.RowInfo("venison", 0, 2.5), Is.EqualTo("2 Salt each, makes 4; you have no Salt"));
        Assert.That(SaltRules.YieldOf("venison"), Is.EqualTo(4));
        Assert.That(SaltRules.YieldOf("bass"), Is.EqualTo(3));
        Assert.That(SaltRules.YieldOf("bluegill"), Is.EqualTo(2));
        Assert.That(SaltRules.YieldOf("blackberries"), Is.EqualTo(0));
        Assert.That(SaltRules.SaltedMessage(1, "Venison", 4, "Salt Meat", 2), Is.EqualTo("Salted 1 Venison into 4 Salt Meat (2 Salt)"));
        Assert.That(SaltRules.CrockId, Is.EqualTo("water_crock"), "Mike: use the Water Crock (renamed Crock Pot)");
    }

    [Test]
    public void SaltIsSoldAndTheSaltFoodIsBoughtAndUnlocksKnowHow()
    {
        Assert.That(MarketCatalog.Rows.Single(r => r.itemId == SaltRules.SaltId).kind, Is.EqualTo(MarketKind.Stocked));
        Assert.That(MarketCatalog.Rows.Single(r => r.itemId == SaltRules.SaltId).baseCents, Is.EqualTo(200));
        Assert.That(MarketCatalog.Rows.Single(r => r.itemId == SaltRules.SaltMeatId).kind, Is.EqualTo(MarketKind.Bought));
        Assert.That(MarketCatalog.Rows.Single(r => r.itemId == SaltRules.SaltFishId).kind, Is.EqualTo(MarketKind.Bought));
        foreach (string id in new[] { SaltRules.SaltId, SaltRules.SaltMeatId, SaltRules.SaltFishId })
            Assert.That(KnowHowRules.ForItem(id), Does.Contain(KnowHowRules.Salting), id);
        Assert.That(PotteryRules.Recipes.Length, Is.EqualTo(5), "the Potter screen keeps its five rows");
    }

    // Mike, 2026-11-22: "make salted meat and dried meat more profitable than just cooked meat and even more than fresh raw meat". Selling many
    // strips or pieces drops their price a little each time (5% a unit in the meat and fish groups), so the check allows for that.
    [Test]
    public void DriedAndSaltedMeatAndFishPayMoreThanCookedAndCookedMoreThanRaw()
    {
        foreach (string raw in new[] { "venison", "turkey_meat", "waterfowl_meat", "small_game_meat", "chicken_meat", "bluegill", "crappie", "bass", "catfish" })
        {
            long rawCents = MarketCatalog.Rows.Single(r => r.itemId == raw).baseCents;
            string cookedId = raw == "turkey_meat" ? "cooked_turkey" : raw == "waterfowl_meat" ? "cooked_waterfowl" : raw == "small_game_meat" ? "cooked_small_game"
                            : raw == "chicken_meat" ? "cooked_chicken" : "cooked_" + raw;
            long cooked = MarketCatalog.Rows.Single(r => r.itemId == cookedId).baseCents;
            Assert.That(DryingRules.CookedCents(raw), Is.EqualTo(cooked), raw + ": the drying table follows the Trading Post");
            Assert.That(cooked, Is.GreaterThan(rawCents), raw);

            DryingRules.TryFind(raw, out string driedId, out _);
            int strips = DryingRules.YieldOf(raw, 0);
            double dried = SoldWithDrops(MarketCatalog.Rows.Single(r => r.itemId == driedId).baseCents, strips);
            Assert.That(dried, Is.GreaterThan(cooked), raw + " dried");

            SaltRules.TryFind(raw, out string saltedId);
            double weight = raw == "venison" ? 2.5 : raw == "turkey_meat" ? 1.5 : raw == "waterfowl_meat" ? 1.3 : 0.8;
            double salted = SoldWithDrops(MarketCatalog.Rows.Single(r => r.itemId == saltedId).baseCents, SaltRules.YieldOf(raw)) - SaltRules.SaltFor(weight) * SaltRules.SaltCents;
            Assert.That(salted, Is.GreaterThan(cooked), raw + " salted, less its Salt");
        }
    }

    // What n units fetch when each sale drops the price 5% of the base.
    static double SoldWithDrops(long cents, int n)
    {
        double total = 0;
        for (int i = 0; i < n; i++)
            total += cents * Math.Max(0.25, 1.0 - 0.05 * i);
        return total;
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
