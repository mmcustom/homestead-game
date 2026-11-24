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

    // --- Slice 5: the Smokehouse (SmokeRules; Mike 2026-11-22: staged build, top of the line, the fire as proposed) ---

    [Test]
    public void RawMeatAndFishSmokeIntoOnePiecePerCookedDollar()
    {
        Assert.That(SmokeRules.TryFind("venison", out string meat) && meat == SmokeRules.SmokedMeatId, Is.True);
        Assert.That(SmokeRules.TryFind("catfish", out string fish) && fish == SmokeRules.SmokedFishId, Is.True);
        Assert.That(SmokeRules.Smokes("blackberries"), Is.False);
        Assert.That(SmokeRules.Smokes("cooked_venison"), Is.False);
        Assert.That(SmokeRules.YieldOf("venison"), Is.EqualTo(4));
        Assert.That(SmokeRules.YieldOf("bass"), Is.EqualTo(3));
        Assert.That(SmokeRules.YieldOf("bluegill"), Is.EqualTo(2));
        foreach (string raw in SmokeRules.RawIds)
            Assert.That(SmokeRules.Smokes(raw) && SaltRules.Salts(raw), Is.True, raw + ": the same raw list as salting");
        Assert.That(SmokeRules.FirewoodPerLoad, Is.EqualTo(6), "the design doc: a day of smoking burns 6 Firewood");
        Assert.That(SmokeRules.MeatCap, Is.EqualTo(20));
    }

    [Test]
    public void TheSmokehouseLightsWithMeatAndFirewoodAndCollectsFirst()
    {
        Assert.That(SmokeRules.PhaseOf(false, 0, 0, 0, 0), Is.EqualTo(SmokeRules.Phase.Empty));
        Assert.That(SmokeRules.PhaseOf(false, 0, 4, 0, 0), Is.EqualTo(SmokeRules.Phase.Loading), "meat but no wood");
        Assert.That(SmokeRules.PhaseOf(false, 0, 0, 6, 0), Is.EqualTo(SmokeRules.Phase.Loading), "wood but no meat");
        Assert.That(SmokeRules.PhaseOf(false, 0, 4, 1, 0), Is.EqualTo(SmokeRules.Phase.Ready), "one Firewood is enough to light it");
        Assert.That(SmokeRules.PhaseOf(false, 0, 4, 6, 8), Is.EqualTo(SmokeRules.Phase.Done), "smoked food waiting is collected first");
        Assert.That(SmokeRules.PhaseOf(true, 2, 4, 0, 0), Is.EqualTo(SmokeRules.Phase.Smoking), "a log still burning");
        Assert.That(SmokeRules.PhaseOf(true, 0, 4, 3, 0), Is.EqualTo(SmokeRules.Phase.Smoking), "the next log is there");
        Assert.That(SmokeRules.PhaseOf(true, 0, 4, 0, 0), Is.EqualTo(SmokeRules.Phase.Paused), "out of wood");
        Assert.That(SmokeRules.CanDismantle(SmokeRules.Phase.Paused), Is.False);
        Assert.That(SmokeRules.CanDismantle(SmokeRules.Phase.Done), Is.True);
    }

    [Test]
    public void MeatGoesInOnlyBetweenLoadsAndFirewoodAtAnyTime()
    {
        Assert.That(SmokeRules.Accepts("venison", 19, 0, false), Is.True);
        Assert.That(SmokeRules.Accepts("venison", 20, 0, false), Is.False, "20 pieces is a full load");
        Assert.That(SmokeRules.Accepts("venison", 0, 0, true), Is.False, "not while it smokes");
        Assert.That(SmokeRules.Accepts("firewood", 4, 11, true), Is.True);
        Assert.That(SmokeRules.Accepts("firewood", 4, 12, false), Is.False);
        Assert.That(SmokeRules.Accepts("stone", 0, 0, false), Is.False);
        SmokeRules.LoadPlan plan = SmokeRules.PlanLoad(15, 10, false, 8, 5);
        Assert.That(plan.raw, Is.EqualTo(5));
        Assert.That(plan.firewood, Is.EqualTo(2));
        plan = SmokeRules.PlanLoad(4, 0, true, 3, 2);
        Assert.That(plan.raw, Is.EqualTo(0));
        Assert.That(plan.firewood, Is.EqualTo(2));
        Assert.That(SmokeRules.LoadPrompt(plan, true, 3, 2), Is.EqualTo("Load 2 Firewood  (meat waits until this load is done)"));
    }

    [Test]
    public void AFullDayBurnsSixFirewoodAndRunningOutPauses()
    {
        float hours = SmokeRules.SmokeHours;
        int fuel = SmokeRules.HoursPerFirewood, wood = 5, burned = 1, smokedHours = 0;
        for (int i = 0; i < 30 && !SmokeRules.IsDone(hours); i++)
        {
            SmokeRules.Hour h = SmokeRules.AfterHour(hours, fuel, wood);
            wood -= h.firewoodUsed;
            burned += h.firewoodUsed;
            hours = h.hoursLeft;
            fuel = h.fuelHours;
            if (h.smoked)
                smokedHours++;
        }
        Assert.That(SmokeRules.IsDone(hours), Is.True);
        Assert.That(burned, Is.EqualTo(6), "the one lit with and five more");
        Assert.That(smokedHours, Is.EqualTo(24));

        SmokeRules.Hour paused = SmokeRules.AfterHour(10f, 0, 0);
        Assert.That(paused.smoked, Is.False);
        Assert.That(paused.hoursLeft, Is.EqualTo(10f), "no wood, no progress");
        SmokeRules.Hour resumed = SmokeRules.AfterHour(10f, 0, 1);
        Assert.That(resumed.smoked && resumed.firewoodUsed == 1 && resumed.hoursLeft == 9f, Is.True, "new wood takes from the embers");
        Assert.That(SmokeRules.FirewoodStillNeeded(10f, 2), Is.EqualTo(2), "8 hours beyond the log burning");
    }

    [Test]
    public void SmokedFoodIsTheTopOfTheLineEvenAfterTheFirewood()
    {
        long firewood = MarketCatalog.Rows.Single(r => r.itemId == "firewood").baseCents;
        foreach (string raw in SmokeRules.RawIds)
        {
            SmokeRules.TryFind(raw, out string smokedId);
            SaltRules.TryFind(raw, out string saltedId);
            double weight = raw == "venison" ? 2.5 : raw == "turkey_meat" ? 1.5 : raw == "waterfowl_meat" ? 1.3 : 0.8;
            double smoked = SoldWithDrops(MarketCatalog.Rows.Single(r => r.itemId == smokedId).baseCents, SmokeRules.YieldOf(raw));
            double salted = SoldWithDrops(MarketCatalog.Rows.Single(r => r.itemId == saltedId).baseCents, SaltRules.YieldOf(raw)) - SaltRules.SaltFor(weight) * SaltRules.SaltCents;
            // A full load of 20 shares its 6 Firewood: under a third of a Firewood a piece.
            double firewoodShare = firewood * (double)SmokeRules.FirewoodPerLoad / SmokeRules.MeatCap;
            Assert.That(smoked - firewoodShare, Is.GreaterThan(salted), raw + " smoked pays more than salted");
            Assert.That(MarketCatalog.Rows.Single(r => r.itemId == smokedId).kind, Is.EqualTo(MarketKind.Bought));
        }
        foreach (string id in SmokeRules.SmokedIds)
            Assert.That(KnowHowRules.ForItem(id), Does.Contain(KnowHowRules.Smoking), id);
        Assert.That(KnowHowRules.ForHappening(KnowHowRules.Happening.SmokehouseSitePlaced), Is.EqualTo(new[] { KnowHowRules.Smoking }));
    }

    // The hourly burn on a real pile (WoodPileState and SpoilageManager are game types): the meat's stamp moves on with the clock while the fire burns,
    // stays put while it is out, and at the end every raw piece becomes its smoked pieces with the stamp given.
    [Test]
    public void ABurningHourKeepsTheMeatFreshAndTheLastHourSmokesIt()
    {
        Type pileType = Find("WoodPileState"), kindType = Find("PileKind"), manager = Find("SpoilageManager");
        object house = Activator.CreateInstance(pileType);
        pileType.GetField("kind").SetValue(house, Enum.Parse(kindType, "Smokehouse"));
        var add = pileType.GetMethod("Add");
        add.Invoke(house, new object[] { "venison", 2, 100 });
        add.Invoke(house, new object[] { "bass", 1, 100 });
        add.Invoke(house, new object[] { "firewood", 1, 0 });
        pileType.GetField("jobItem").SetValue(house, SmokeRules.SmokeMarker);
        pileType.GetField("jobHours").SetValue(house, 2f);
        pileType.GetField("jobCount").SetValue(house, 0);
        var burn = manager.GetMethod("BurnHour");
        var contents = (System.Collections.IList)pileType.GetField("contents").GetValue(house);
        int StampOf(string id) { foreach (object s in contents) if ((string)s.GetType().GetField("itemId").GetValue(s) == id) return (int)s.GetType().GetField("day").GetValue(s); return -1; }
        int CountOf(string id) { int n = 0; foreach (object s in contents) if ((string)s.GetType().GetField("itemId").GetValue(s) == id) n += (int)s.GetType().GetField("count").GetValue(s); return n; }

        Assert.That((bool)burn.Invoke(null, new object[] { house, 1.5, 500 }), Is.False);
        Assert.That(CountOf("firewood"), Is.EqualTo(0), "the Firewood went on");
        Assert.That(StampOf("venison"), Is.EqualTo(101), "1.5 clock hours: one whole hour moved on, half kept");
        Assert.That((bool)burn.Invoke(null, new object[] { house, 1.5, 500 }), Is.True, "the last hour finishes the load");
        Assert.That(CountOf("venison") + CountOf("bass"), Is.EqualTo(0));
        Assert.That(CountOf(SmokeRules.SmokedMeatId), Is.EqualTo(8), "2 Venison at 4 each");
        Assert.That(CountOf(SmokeRules.SmokedFishId), Is.EqualTo(3));
        Assert.That(StampOf(SmokeRules.SmokedMeatId), Is.EqualTo(500));
        Assert.That((string)pileType.GetField("jobItem").GetValue(house), Is.Empty);

        object idle = Activator.CreateInstance(pileType);
        pileType.GetField("kind").SetValue(idle, Enum.Parse(kindType, "Smokehouse"));
        add.Invoke(idle, new object[] { "venison", 1, 100 });
        pileType.GetField("jobItem").SetValue(idle, SmokeRules.SmokeMarker);
        pileType.GetField("jobHours").SetValue(idle, 5f);
        burn.Invoke(null, new object[] { idle, 1.5, 500 });
        var idleContents = (System.Collections.IList)pileType.GetField("contents").GetValue(idle);
        Assert.That((int)idleContents[0].GetType().GetField("day").GetValue(idleContents[0]), Is.EqualTo(100), "out of wood: the meat ages");
        Assert.That((float)pileType.GetField("jobHours").GetValue(idle), Is.EqualTo(5f));
    }

    // --- Slice 6: the Root Cellar (RootCellarRules; Mike 2026-11-23: dug then built, walked into, 300 kg) ---

    [Test]
    public void TheRootCellarHolds300KgOfFoodByWeight()
    {
        Assert.That(RootCellarRules.UnitsThatFit(0f, 2.5f), Is.EqualTo(120), "120 Venison");
        Assert.That(RootCellarRules.UnitsThatFit(299f, 2.5f), Is.EqualTo(0));
        Assert.That(RootCellarRules.UnitsThatFit(297.5f, 2.5f), Is.EqualTo(1), "an exact fit");
        Assert.That(RootCellarRules.UnitsThatFit(300f, 0.1f), Is.EqualTo(0));
        Assert.That(RootCellarRules.KgText(42.4f), Is.EqualTo("42 of 300 kg"));
        Assert.That(SpoilageRules.ClockRate(FreshClock.Cellar, 30.0, 1.0), Is.EqualTo(SpoilageRules.ClockRate(FreshClock.Cellar, -10.0, 1.0)), "the same in summer and winter");
        Assert.That(SpoilageRules.ClockRate(FreshClock.Cellar, 20.0, 1.0), Is.EqualTo(0.3).Within(1e-9));
    }

    [Test]
    public void TheShelvesAnswerFromInsideTheRoomOrItsDoorwayOnly()
    {
        Assert.That(RootCellarRules.IsInside(0f, 0f), Is.True, "the middle of the room");
        Assert.That(RootCellarRules.IsInside(0f, RootCellarRules.InnerBack + 0.1f), Is.True, "at the shelves");
        Assert.That(RootCellarRules.IsInside(0f, RootCellarRules.FrontZ + 0.2f), Is.True, "in the doorway");
        Assert.That(RootCellarRules.IsInside(0f, RootCellarRules.FrontZ + 1f), Is.False, "outside the door");
        Assert.That(RootCellarRules.IsInside(1.6f, 0f), Is.False, "on the mound's side");
        Assert.That(RootCellarRules.DoorHalfWidth * 2f, Is.GreaterThanOrEqualTo(1f), "a doorway the player fits through");
        Assert.That(RootCellarRules.DoorHeight, Is.GreaterThanOrEqualTo(2f));
    }

    [Test]
    public void TheRootCellarIsDugThenBuiltAndTakesFoodOnly()
    {
        Type rules = Find("WellsAndOuthouses"), kindType = Find("PileKind"), pileType = Find("WoodPileState"), manager = Find("SpoilageManager");
        object site = Enum.Parse(kindType, "RootCellarSite"), cellar = Enum.Parse(kindType, "RootCellar");
        Assert.That((bool)rules.GetMethod("RequiresDig").Invoke(null, new[] { site }), Is.True);
        Assert.That((bool)rules.GetMethod("IsStagedSite").Invoke(null, new[] { site }), Is.True);
        Assert.That(rules.GetMethod("FinishedKind").Invoke(null, new[] { site }), Is.EqualTo(cellar));
        Assert.That(manager.GetMethod("ClockOf").Invoke(null, new[] { cellar }), Is.EqualTo(FreshClock.Cellar));
        object pile = Activator.CreateInstance(pileType);
        pileType.GetField("kind").SetValue(pile, cellar);
        var accepts = pileType.GetMethod("Accepts");
        Assert.That((bool)accepts.Invoke(pile, new object[] { "venison" }), Is.True);
        Assert.That((bool)accepts.Invoke(pile, new object[] { "logs" }), Is.False);
        Assert.That(KnowHowRules.ForHappening(KnowHowRules.Happening.RootCellarSitePlaced), Is.EqualTo(new[] { KnowHowRules.Cellar }));
    }

    // --- The Saw Pit (SawPitRules; Mike 2026-11-23: "a lumber mill or something to turn logs into boards or lumber") ---

    [Test]
    public void EveryTwoLogsSawnMakeThreeLumber()
    {
        Assert.That(SawPitRules.Accepts("logs", 5), Is.True);
        Assert.That(SawPitRules.Accepts("logs", 6), Is.False, "six Logs at most");
        Assert.That(SawPitRules.Accepts("lumber", 0), Is.False);
        Assert.That(SawPitRules.CanSaw(1), Is.False, "a single Log waits for a second");
        Assert.That(SawPitRules.CanSaw(2), Is.True);
        Assert.That(SawPitRules.StrokesPerPair, Is.EqualTo(20), "Mike: 20 strokes a pair");
        float strokes = 0f; int pairs = 0;
        for (int i = 0; i < SawPitRules.StrokesPerPair; i++)
        {
            strokes = SawPitRules.AfterStroke(strokes, 1f, out bool done);
            if (done) pairs++;
        }
        Assert.That(pairs, Is.EqualTo(1));
        Assert.That(strokes, Is.EqualTo(0f));
        Assert.That(SawPitRules.LumberPerPair * 5f, Is.LessThanOrEqualTo(SawPitRules.LogsPerPair * 8f), "no more board than log by weight");
        Assert.That(SawPitRules.Prompt(3, 0, 0f, true), Is.EqualTo("Saw Pit  (3 Logs -> 3 Lumber; hold click to saw)"));
        Type rules = Find("WellsAndOuthouses"), kindType = Find("PileKind");
        object site = Enum.Parse(kindType, "SawPitSite");
        Assert.That((bool)rules.GetMethod("RequiresDig").Invoke(null, new[] { site }), Is.True);
        Assert.That(rules.GetMethod("FinishedKind").Invoke(null, new[] { site }), Is.EqualTo(Enum.Parse(kindType, "SawPit")));
        Assert.That(KnowHowRules.ForHappening(KnowHowRules.Happening.SawPitSitePlaced), Is.EqualTo(new[] { KnowHowRules.Sawing }));
    }

    [Test]
    public void TheSaltBarrelHoldsDryGoodsUpToAHundredKilograms()
    {
        Assert.That(SaltBarrelRules.CapacityKg, Is.EqualTo(100f));
        Assert.That(SaltBarrelRules.UnitsThatFit(0f, 1f), Is.EqualTo(100), "100 Salt at 1 kg each");
        Assert.That(SaltBarrelRules.UnitsThatFit(99.5f, 1f), Is.EqualTo(0));
        Assert.That(SaltBarrelRules.UnitsThatFit(99.5f, 0.3f), Is.EqualTo(1), "one more Cordage still fits");
        Assert.That(SaltBarrelRules.KgText(42f), Is.EqualTo("42 of 100 kg"));
        Type manager = Find("WoodManager"), kindType = Find("PileKind");
        object barrel = Enum.Parse(kindType, "SaltBarrel");
        Assert.That((string)manager.GetMethod("PileName").Invoke(null, new[] { barrel }), Is.EqualTo("Salt Barrel"));
        Assert.That((bool)manager.GetMethod("NeedsHammerToBuild").Invoke(null, new[] { barrel }), Is.True, "built in one go with the Hammer, like the Storage Bin");
        Assert.That(Array.IndexOf((Array)manager.GetField("Buildable").GetValue(null), barrel), Is.GreaterThanOrEqualTo(0));
        Assert.That((bool)manager.GetMethod("IsDryGoods").Invoke(null, new object[] { "salt" }), Is.True, "Salt is dry goods, so the barrel takes it");
    }

    [Test]
    public void RawHidesSpoilAndSaltingKeepsThemAYear()
    {
        Assert.That(HideRules.TryFind("deer_hide", out string hide, out int hideSalt), Is.True);
        Assert.That(hide, Is.EqualTo("salted_deer_hide")); Assert.That(hideSalt, Is.EqualTo(2));
        Assert.That(HideRules.TryFind("small_furs", out string furs, out int fursSalt), Is.True);
        Assert.That(furs, Is.EqualTo("salted_furs")); Assert.That(fursSalt, Is.EqualTo(1));
        Assert.That(HideRules.Salts("venison"), Is.False, "meat salts in the crock (SaltRules), not here");
        Assert.That(HideRules.PiecesFor("deer_hide", 3, 5), Is.EqualTo(2), "5 Salt salts two Deer Hides");
        Assert.That(HideRules.PiecesFor("small_furs", 3, 5), Is.EqualTo(3), "no more than are carried");
        Assert.That(HideRules.PiecesFor("deer_hide", 1, 1), Is.EqualTo(0));
        Assert.That(HideRules.HalfFreshAgeHours(7), Is.EqualTo(84.0));
        Assert.That(HideRules.RowInfo("deer_hide", 0), Is.EqualTo("2 Salt each, keeps about a year; you have no Salt"));
        Assert.That(HideRules.SaltedMessage(1, "Deer Hide", 2), Is.EqualTo("Salted 1 Deer Hide (2 Salt): it keeps about a year now"));

        // The item assets carry the shelf lives; the salted hides weigh what the raw ones do and are not food.
        Type db = Find("ItemDatabase");
        Func<string, object> get = id => db.GetMethod("Get", new[] { typeof(string) }).Invoke(null, new object[] { id });
        Func<string, string, object> prop = (id, name) => { object item = get(id); return item.GetType().GetProperty(name).GetValue(item, null); };
        Assert.That(prop("deer_hide", "ShelfLifeDays"), Is.EqualTo(HideRules.DeerHideShelfDays));
        Assert.That(prop("small_furs", "ShelfLifeDays"), Is.EqualTo(HideRules.SmallFursShelfDays));
        Assert.That(prop("salted_deer_hide", "ShelfLifeDays"), Is.EqualTo(HideRules.SaltedShelfDays));
        Assert.That(prop("salted_furs", "ShelfLifeDays"), Is.EqualTo(HideRules.SaltedShelfDays));
        Assert.That(prop("salted_deer_hide", "WeightKg"), Is.EqualTo(prop("deer_hide", "WeightKg")));
        Assert.That(prop("salted_furs", "WeightKg"), Is.EqualTo(prop("small_furs", "WeightKg")));
        foreach (string id in new[] { "deer_hide", "small_furs", "salted_deer_hide", "salted_furs" })
            Assert.That(prop(id, "IsFood"), Is.False, id + " is not food, so the Food Cache and Root Cellar refuse it");
        Assert.That(KnowHowRules.ForItem("salted_furs"), Is.EqualTo(new[] { KnowHowRules.Hides }));
    }

    [Test]
    public void TheHideFrameScrapesThenDriesAHideIntoRawhide()
    {
        Assert.That(HideFrameRules.KindOf("deer_hide"), Is.EqualTo(HideFrameRules.Kind.Deer));
        Assert.That(HideFrameRules.KindOf("salted_deer_hide"), Is.EqualTo(HideFrameRules.Kind.Deer), "salted hides go on the frame too");
        Assert.That(HideFrameRules.KindOf("salted_furs"), Is.EqualTo(HideFrameRules.Kind.Furs));
        Assert.That(HideFrameRules.KindOf("venison"), Is.EqualTo(HideFrameRules.Kind.None));
        Assert.That(HideFrameRules.MaxOn(HideFrameRules.Kind.Deer), Is.EqualTo(1));
        Assert.That(HideFrameRules.MaxOn(HideFrameRules.Kind.Furs), Is.EqualTo(4), "a bundle of up to 4 Small Furs");

        // 20 strokes scrape a Deer Hide, 8 a bundle of furs.
        float strokes = 0f; int blows = 0; bool done = false;
        while (!done && blows < 100) { strokes = HideFrameRules.AfterStroke(strokes, HideFrameRules.Kind.Deer, out done); blows++; }
        Assert.That(blows, Is.EqualTo(20));
        strokes = 0f; blows = 0; done = false;
        while (!done && blows < 100) { strokes = HideFrameRules.AfterStroke(strokes, HideFrameRules.Kind.Furs, out done); blows++; }
        Assert.That(blows, Is.EqualTo(8));

        // 12 daylight hours dry it.
        float hours = 0f; int dryHours = 0; done = false;
        while (!done && dryHours < 100) { hours = HideFrameRules.AfterDryHour(hours, out done); dryHours++; }
        Assert.That(dryHours, Is.EqualTo(12));

        Assert.That(HideFrameRules.OutputFor(HideFrameRules.Kind.Deer), Is.EqualTo("rawhide"));
        Assert.That(HideFrameRules.OutputCount(HideFrameRules.Kind.Deer, 1), Is.EqualTo(1));
        Assert.That(HideFrameRules.OutputFor(HideFrameRules.Kind.Furs), Is.EqualTo("dried_furs"));
        Assert.That(HideFrameRules.OutputCount(HideFrameRules.Kind.Furs, 3), Is.EqualTo(3), "one Dried Furs for each fur");

        Assert.That(HideFrameRules.PhaseOf(false, false, null), Is.EqualTo(HideFrameRules.Phase.Empty));
        Assert.That(HideFrameRules.PhaseOf(true, false, null), Is.EqualTo(HideFrameRules.Phase.Scraping));
        Assert.That(HideFrameRules.PhaseOf(false, true, HideFrameRules.DryingMarker), Is.EqualTo(HideFrameRules.Phase.Drying));
        Assert.That(HideFrameRules.PhaseOf(false, true, null), Is.EqualTo(HideFrameRules.Phase.Done));

        // Each step pays more than the last.
        Assert.That(HideFrameRules.RawhideCents, Is.GreaterThan(HideRules.SaltedDeerHideCents));
        Assert.That(HideFrameRules.DriedFursCents, Is.GreaterThan(HideRules.SaltedFursCents));

        Type db = Find("ItemDatabase");
        Func<string, object> get = id => db.GetMethod("Get", new[] { typeof(string) }).Invoke(null, new object[] { id });
        Func<string, string, object> prop = (id, name) => { object item = get(id); return item.GetType().GetProperty(name).GetValue(item, null); };
        Assert.That(prop("rawhide", "WeightKg"), Is.EqualTo(2f));
        Assert.That(prop("rawhide", "IsPerishable"), Is.False, "Rawhide keeps for years");
        Assert.That(prop("dried_furs", "IsPerishable"), Is.False);

        Type manager = Find("WoodManager"), kindType = Find("PileKind");
        object frame = Enum.Parse(kindType, "HideFrame");
        Assert.That((string)manager.GetMethod("PileName").Invoke(null, new[] { frame }), Is.EqualTo("Hide Frame"));
        Assert.That((bool)manager.GetMethod("NeedsHammerToBuild").Invoke(null, new[] { frame }), Is.True, "one click with the Hammer, like the Drying Rack");
        Assert.That(KnowHowRules.ForItem("rawhide"), Is.EqualTo(new[] { KnowHowRules.HideFrame }));
        Assert.That(KnowHowRules.ForHappening(KnowHowRules.Happening.HideFramePlaced), Is.EqualTo(new[] { KnowHowRules.HideFrame }));
    }

    [Test]
    public void BrainTanningSoftensOnTheFrameThenSmokesIntoBuckskin()
    {
        Assert.That(HideFrameRules.TrySoften("rawhide", out string soft), Is.True); Assert.That(soft, Is.EqualTo("soft_hide"));
        Assert.That(HideFrameRules.TrySoften("dried_furs", out soft), Is.True); Assert.That(soft, Is.EqualTo("soft_furs"));
        Assert.That(HideFrameRules.Softens("deer_hide"), Is.False, "a raw hide is scraped and dried first");
        Assert.That(HideFrameRules.KindOfOutput("rawhide"), Is.EqualTo(HideFrameRules.Kind.Deer));
        Assert.That(HideFrameRules.KindOfOutput("soft_furs"), Is.EqualTo(HideFrameRules.Kind.Furs));
        Assert.That(HideFrameRules.PhaseOf(false, true, HideFrameRules.SofteningMarker), Is.EqualTo(HideFrameRules.Phase.Softening));

        float strokes = 0f; int blows = 0; bool done = false;
        while (!done && blows < 100) { strokes = HideFrameRules.AfterSoftenStroke(strokes, HideFrameRules.Kind.Deer, out done); blows++; }
        Assert.That(blows, Is.EqualTo(30));
        strokes = 0f; blows = 0; done = false;
        while (!done && blows < 100) { strokes = HideFrameRules.AfterSoftenStroke(strokes, HideFrameRules.Kind.Furs, out done); blows++; }
        Assert.That(blows, Is.EqualTo(10));

        Assert.That(HideFrameRules.SoftenRefusal(true, 2), Is.Null);
        Assert.That(HideFrameRules.SoftenRefusal(false, 2), Does.Contain("Crock Pot"));
        Assert.That(HideFrameRules.SoftenRefusal(true, 0), Does.Contain("water"));

        // The Smokehouse takes them with the meat: a Soft Hide is 2 pieces' room, Soft Furs 1, each makes one.
        Assert.That(SmokeRules.TryFind("soft_hide", out string smoked), Is.True); Assert.That(smoked, Is.EqualTo("buckskin"));
        Assert.That(SmokeRules.TryFind("soft_furs", out smoked), Is.True); Assert.That(smoked, Is.EqualTo("tanned_furs"));
        Assert.That(SmokeRules.RoomOf("soft_hide"), Is.EqualTo(2));
        Assert.That(SmokeRules.RoomOf("soft_furs"), Is.EqualTo(1));
        Assert.That(SmokeRules.RoomOf("venison"), Is.EqualTo(1));
        Assert.That(SmokeRules.YieldOf("soft_hide"), Is.EqualTo(1));
        Assert.That(SmokeRules.IsSmoked("buckskin"), Is.True, "E collects it with the smoked food");
        Assert.That(SmokeRules.Accepts("soft_hide", SmokeRules.MeatCap - 1, 0, false), Is.False, "no room for 2 pieces' worth");
        Assert.That(SmokeRules.Accepts("soft_furs", SmokeRules.MeatCap - 1, 0, false), Is.True);
        Assert.That(SmokeRules.Accepts("soft_hide", 0, 0, true), Is.False, "nothing goes in while a load smokes");
        Assert.That(SmokeRules.SmokedGoodsText(4, 0, 1, 2), Is.EqualTo("4 Smoked Meat, 1 Buckskin and 2 Tanned Furs"));

        // Each step pays more than the last.
        Assert.That(SmokeRules.BuckskinCents, Is.GreaterThan(HideFrameRules.RawhideCents));
        Assert.That(SmokeRules.TannedFursCents, Is.GreaterThan(HideFrameRules.DriedFursCents));

        Type db = Find("ItemDatabase");
        Func<string, object> get = id => db.GetMethod("Get", new[] { typeof(string) }).Invoke(null, new object[] { id });
        foreach (string id in new[] { "soft_hide", "soft_furs", "buckskin", "tanned_furs" })
        {
            object item = get(id);
            Assert.That(item, Is.Not.Null, id);
            Assert.That(item.GetType().GetProperty("IsPerishable").GetValue(item, null), Is.False, id + " does not spoil");
        }
        Assert.That(KnowHowRules.ForItem("buckskin"), Is.EqualTo(new[] { KnowHowRules.BrainTanning }));
    }

    [Test]
    public void TheTanningVatSoaksRawhideSevenDaysIntoLeather()
    {
        Assert.That(TanningVatRules.SoakHours, Is.EqualTo(7 * 24), "halved from 14 days for gameplay (Mike, 2026-11-23)");
        Assert.That(TanningVatRules.CanStart(2, 8, 40), Is.True, "4 Tanbark and 20 litres a hide");
        Assert.That(TanningVatRules.CanStart(2, 7, 40), Is.False);
        Assert.That(TanningVatRules.CanStart(2, 8, 39), Is.False);
        Assert.That(TanningVatRules.CanStart(0, 8, 40), Is.False);
        Assert.That(TanningVatRules.PhaseOf(false, 0, 0, 0, 0), Is.EqualTo(TanningVatRules.Phase.Empty));
        Assert.That(TanningVatRules.PhaseOf(false, 1, 2, 0, 0), Is.EqualTo(TanningVatRules.Phase.Loading));
        Assert.That(TanningVatRules.PhaseOf(false, 1, 4, 20, 0), Is.EqualTo(TanningVatRules.Phase.Ready));
        Assert.That(TanningVatRules.PhaseOf(true, 1, 0, 0, 0), Is.EqualTo(TanningVatRules.Phase.Soaking));
        Assert.That(TanningVatRules.PhaseOf(false, 0, 0, 0, 2), Is.EqualTo(TanningVatRules.Phase.Done));

        // R loads the hides first, then only the bark and water they need.
        TanningVatRules.LoadPlan plan = TanningVatRules.PlanLoad(0, 0, 0, 6, 30, 100);
        Assert.That(plan.hides, Is.EqualTo(4), "up to 4 hides");
        Assert.That(plan.tanbark, Is.EqualTo(16));
        Assert.That(plan.water, Is.EqualTo(80));
        plan = TanningVatRules.PlanLoad(1, 4, 10, 0, 10, 10);
        Assert.That(plan.tanbark, Is.EqualTo(0), "the one hide already has its bark");
        Assert.That(plan.water, Is.EqualTo(10));

        float hours = TanningVatRules.SoakHours; int ticks = 0; bool done = false;
        while (!done && ticks < 1000) { hours = TanningVatRules.AfterHour(hours, out done); ticks++; }
        Assert.That(ticks, Is.EqualTo(TanningVatRules.SoakHours));
        Assert.That(TanningVatRules.DaysLeft(25f), Is.EqualTo(2));

        Assert.That(TanningVatRules.LeatherCents, Is.GreaterThan(SmokeRules.BuckskinCents), "Leather pays the most");
        Assert.That(TanningVatRules.Stripped(1), Is.EqualTo("Stripped the bark from 1 Log: 2 Tanbark and a Peeled Log"));

        Type db = Find("ItemDatabase");
        Func<string, object> get = id => db.GetMethod("Get", new[] { typeof(string) }).Invoke(null, new object[] { id });
        foreach (string id in new[] { "tanbark", "leather", "peeled_log" })
        {
            object item = get(id);
            Assert.That(item, Is.Not.Null, id);
            Assert.That(item.GetType().GetProperty("IsPerishable").GetValue(item, null), Is.False, id + " keeps");
        }
        Type manager = Find("WoodManager"), kindType = Find("PileKind");
        object vat = Enum.Parse(kindType, "TanningVat");
        Assert.That((string)manager.GetMethod("PileName").Invoke(null, new[] { vat }), Is.EqualTo("Tanning Vat"));
        Assert.That((bool)manager.GetMethod("NeedsHammerToBuild").Invoke(null, new[] { vat }), Is.True);
        Assert.That((bool)manager.GetMethod("IsWood").Invoke(null, new object[] { "peeled_log" }), Is.True, "a Peeled Log stores with the wood");
        Assert.That(KnowHowRules.ForItem("leather"), Is.EqualTo(new[] { KnowHowRules.BarkTanning }));
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
