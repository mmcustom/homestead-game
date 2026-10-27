using System;
using System.Linq;
using NUnit.Framework;

// EditMode tests for Maple Sugaring's pure rules (MapleRules, Homestead.Economy), Maple A: which trees are Maples, when the sap runs, how a
// Sap Bucket fills and sours, the boil ratio, the Syrup market entry and the four new items. Nothing here needs a scene, a tree or the clock.
public class MapleTests
{
    static MarketTable Table => MarketCatalog.Table;

    const int Spring = 0, Summer = 1, Fall = 2, Winter = 3;

    // --- Which trees are Maples ---

    [Test]
    public void OnlyLargeHardwoodsCanBeMaples()
    {
        int maple = FirstMapleIndex();
        Assert.That(MapleRules.IsMaple(maple, 0, 1.3f), Is.True);
        Assert.That(MapleRules.IsMaple(maple, 1, 1.3f), Is.True, "Hardwood Tall counts too");
        Assert.That(MapleRules.IsMaple(maple, 2, 1.3f), Is.False, "never an Understory Shrub");
        Assert.That(MapleRules.IsMaple(maple, 0, 1.19f), Is.False, "too small");
        Assert.That(MapleRules.IsMaple(maple, 0, 1.2f), Is.True, "large starts at exactly 1.2");
        Assert.That(MapleRules.IsMaple(-1, 0, 1.3f), Is.False, "no such tree");
        Assert.That(MapleRules.IsMaple(maple, 7, 1.3f), Is.False, "an unknown prototype");
    }

    static int FirstMapleIndex() => Enumerable.Range(0, 1000).First(i => MapleRules.IsMaple(i, 0, 1.3f));

    [Test]
    public void AboutOneLargeHardwoodInSevenIsAMaple()
    {
        const int trees = 100000;
        int maples = Enumerable.Range(0, trees).Count(i => MapleRules.IsMaple(i, 0, 1.3f));
        Assert.That(maples / (double)trees, Is.EqualTo(1.0 / 7.0).Within(0.01), "1 in 7: about 294 of the map's 2,060 large hardwoods");
        Assert.That(MapleRules.OneMapleIn, Is.EqualTo(7));
    }

    [Test]
    public void TheSameTreeIsAlwaysTheSameAnswer()
    {
        for (int i = 0; i < 2000; i++)
            Assert.That(MapleRules.IsMaple(i, 1, 1.25f), Is.EqualTo(MapleRules.IsMaple(i, 1, 1.25f)), "index " + i);
    }

    [Test]
    public void MaplesAreSpreadNotLinedUpInStripes()
    {
        // The terrain lists its trees in a sweep, so a plain index % 7 would put Maples in rows. Count them per block of 100 trees.
        int[] perBlock = Enumerable.Range(0, 200).Select(b => Enumerable.Range(b * 100, 100).Count(i => MapleRules.IsMaple(i, 0, 1.3f))).ToArray();
        Assert.That(perBlock.Min(), Is.GreaterThan(1), "no empty stretches");
        Assert.That(perBlock.Max(), Is.LessThan(30), "no clumps");
        // And it is not the plain remainder.
        Assert.That(Enumerable.Range(0, 100).Count(i => MapleRules.IsMaple(i, 0, 1.3f) == (i % 7 == 0)), Is.LessThan(100));
    }

    // --- When the sap runs ---

    [Test]
    public void SapRunsOnlyInWinterAndSpring()
    {
        Assert.That(MapleRules.InSapSeason(Winter), Is.True);
        Assert.That(MapleRules.InSapSeason(Spring), Is.True);
        Assert.That(MapleRules.InSapSeason(Summer), Is.False);
        Assert.That(MapleRules.InSapSeason(Fall), Is.False, "cold Fall nights do not start the sap");
    }

    [Test]
    public void AFreezingHourResetsTheMemoryAndAWarmHourAddsOne()
    {
        Assert.That(MapleRules.NextHoursSinceFreeze(10f, -1f), Is.EqualTo(0f), "exactly the freeze line counts");
        Assert.That(MapleRules.NextHoursSinceFreeze(10f, -5f), Is.EqualTo(0f));
        Assert.That(MapleRules.NextHoursSinceFreeze(10f, -0.9f), Is.EqualTo(11f), "just above it is a warm hour");
        Assert.That(MapleRules.NextHoursSinceFreeze(0f, 6f), Is.EqualTo(1f));
        Assert.That(MapleRules.NextHoursSinceFreeze(-3f, 6f), Is.EqualTo(1f), "a bad saved value is treated as zero");
        Assert.That(MapleRules.NextHoursSinceFreeze(500f, 6f), Is.EqualTo(MapleRules.FreezeMemoryCap), "capped");
    }

    [Test]
    public void ItFlowsOnAThawJustAfterAFreezeAndNotOtherwise()
    {
        Assert.That(MapleRules.IsFlowHour(Winter, 3f, 4f), Is.True, "a thaw four hours after a freeze");
        Assert.That(MapleRules.IsFlowHour(Winter, 1.1f, 23f), Is.True);
        Assert.That(MapleRules.IsFlowHour(Winter, 1f, 4f), Is.False, "not warmer than the thaw line");
        Assert.That(MapleRules.IsFlowHour(Winter, -3f, 0f), Is.False, "frozen tap");
        Assert.That(MapleRules.IsFlowHour(Winter, 4f, 24f), Is.False, "the last freeze is a day old: the memory has run out");
        Assert.That(MapleRules.IsFlowHour(Winter, 4f, 40f), Is.False);
        Assert.That(MapleRules.IsFlowHour(Fall, 4f, 4f), Is.False, "wrong season");
        Assert.That(MapleRules.IsFlowHour(Spring, 4f, 4f), Is.True);
    }

    [Test]
    public void ACoolNightThenAWarmDayRunsAboutTheRightNumberOfHours()
    {
        // 06:00 to 05:00 the next day: a freeze through the small hours, warm from late morning to evening.
        float[] day = { -3, -3, -2, -2, -1, 0, 0, 1, 2, 4, 6, 7, 8, 8, 7, 6, 4, 3, 2, 1, 0, 0, -1, -2 };
        float since = MapleRules.FreezeMemoryCap;
        int flow = 0;
        foreach (float t in day)
        {
            since = MapleRules.NextHoursSinceFreeze(since, t);
            if (MapleRules.IsFlowHour(Winter, t, since))
                flow++;
        }
        Assert.That(flow, Is.InRange(8, 14));
    }

    // --- A tap and its bucket ---

    [Test]
    public void BiggerTreesGiveMore()
    {
        Assert.That(MapleRules.TreeFactor(0.8f), Is.EqualTo(0.85f).Within(1e-5f));
        Assert.That(MapleRules.TreeFactor(1.35f), Is.EqualTo(1.2f).Within(1e-5f));
        Assert.That(MapleRules.TreeFactor(0.5f), Is.EqualTo(0.85f), "clamped below");
        Assert.That(MapleRules.TreeFactor(2f), Is.EqualTo(1.2f), "clamped above");
        Assert.That(MapleRules.TreeFactor(1.2f), Is.GreaterThan(MapleRules.TreeFactor(1.0f)));
        Assert.That(MapleRules.SapThisHour(false, 1.3f), Is.EqualTo(0f));
        Assert.That(MapleRules.SapThisHour(true, 1.075f), Is.EqualTo(0.7f).Within(0.02f), "an average tree gives 0.7 L");
    }

    [Test]
    public void AFlowDayGivesAboutFourLitresATap()
    {
        float perDay = 5.5f * MapleRules.SapThisHour(true, 1.0f);
        Assert.That(perDay, Is.InRange(3.5f, 4.4f));
    }

    [Test]
    public void ABucketFillsUpToItsCapacityAndWastesTheRest()
    {
        var bucket = new MapleRules.Bucket();
        for (int h = 0; h < 6; h++)
            bucket = MapleRules.Hour(bucket, 0.7f);
        Assert.That(bucket.litres, Is.EqualTo(4.2f).Within(1e-4f));
        Assert.That(bucket.IsFull, Is.False);

        for (int h = 0; h < 20; h++)
            bucket = MapleRules.Hour(bucket, 0.7f);
        Assert.That(bucket.litres, Is.EqualTo(MapleRules.BucketCapacityLitres));
        Assert.That(bucket.IsFull, Is.True);
        Assert.That(MapleRules.BucketCapacityLitres, Is.EqualTo(8f));
    }

    [Test]
    public void AnEmptyBucketDoesNotAgeAndAFillingOneDoes()
    {
        var bucket = new MapleRules.Bucket();
        for (int h = 0; h < 100; h++)
            bucket = MapleRules.Hour(bucket, 0f);
        Assert.That(bucket.ageHours, Is.EqualTo(0f));
        Assert.That(bucket.sour, Is.False, "an empty bucket never sours");
        Assert.That(bucket.IsEmpty, Is.True);

        bucket = MapleRules.Hour(bucket, 0.7f); // the first sap: age 0
        Assert.That(bucket.ageHours, Is.EqualTo(0f));
        bucket = MapleRules.Hour(bucket, 0f);
        Assert.That(bucket.ageHours, Is.EqualTo(1f));
    }

    [Test]
    public void SapSoursFortyEightHoursAfterTheFirstSapAndTheBucketTakesNoMore()
    {
        var bucket = MapleRules.Hour(new MapleRules.Bucket(), 1f);
        for (int h = 0; h < 46; h++)
            bucket = MapleRules.Hour(bucket, 0f);
        Assert.That(bucket.ageHours, Is.EqualTo(46f));
        Assert.That(bucket.sour, Is.False, "46 hours is still good");
        bucket = MapleRules.Hour(bucket, 0f);
        Assert.That(bucket.ageHours, Is.EqualTo(47f));
        Assert.That(bucket.sour, Is.False);
        bucket = MapleRules.Hour(bucket, 0f);
        Assert.That(bucket.ageHours, Is.EqualTo(48f));
        Assert.That(bucket.sour, Is.True, "48 hours turns it");

        MapleRules.Bucket after = MapleRules.Hour(bucket, 5f);
        Assert.That(after.litres, Is.EqualTo(bucket.litres), "a sour bucket takes no more sap");
        Assert.That(after.sour, Is.True);
    }

    [Test]
    public void CollectingTakesWholeLitresAndKeepsTheFractionFresh()
    {
        var bucket = new MapleRules.Bucket { litres = 3.9f, ageHours = 20f };
        MapleRules.Collected got = MapleRules.Collect(bucket);
        Assert.That((got.litres, got.spoiled), Is.EqualTo((3, 0)));
        Assert.That(got.bucket.litres, Is.EqualTo(0.9f).Within(1e-4f));
        Assert.That(got.bucket.ageHours, Is.EqualTo(0f), "the fraction left starts fresh");
        Assert.That(got.bucket.sour, Is.False);

        MapleRules.Collected whole = MapleRules.Collect(new MapleRules.Bucket { litres = 8f, ageHours = 10f });
        Assert.That(whole.litres, Is.EqualTo(8));
        Assert.That(whole.bucket.IsEmpty, Is.True);

        MapleRules.Collected noise = MapleRules.Collect(new MapleRules.Bucket { litres = 2.00005f });
        Assert.That(noise.litres, Is.EqualTo(2));
        Assert.That(noise.bucket.litres, Is.EqualTo(0f), "a speck of noise is dropped");

        Assert.That(MapleRules.Collect(new MapleRules.Bucket()).litres, Is.EqualTo(0));
    }

    [Test]
    public void ASourBucketIsPouredOutAndGivesNothing()
    {
        MapleRules.Collected got = MapleRules.Collect(new MapleRules.Bucket { litres = 5.5f, ageHours = 48f, sour = true });
        Assert.That(got.litres, Is.EqualTo(0));
        Assert.That(got.spoiled, Is.EqualTo(5));
        Assert.That(got.bucket.IsEmpty, Is.True, "ready for fresh sap");
        Assert.That(got.bucket.sour, Is.False);
    }

    [Test]
    public void ACheckedDailyTapNeverWastesAndASkippedWeekendDoes()
    {
        // 24 hours with 5.5 flow hours: about 3.9 L, well inside the 8 L bucket and under the souring time.
        var daily = new MapleRules.Bucket();
        for (int h = 0; h < 24; h++)
            daily = MapleRules.Hour(daily, h < 6 ? 0.7f : 0f);
        Assert.That(MapleRules.Collect(daily).litres, Is.EqualTo(4));

        // Left for three days of 4.2 L: full after two, then sour on the third.
        var left = new MapleRules.Bucket();
        for (int h = 0; h < 72; h++)
            left = MapleRules.Hour(left, h % 24 < 6 ? 0.7f : 0f);
        Assert.That(left.sour, Is.True);
        Assert.That(MapleRules.Collect(left).litres, Is.EqualTo(0));
    }

    [Test]
    public void ATapLimitOfTwelve()
    {
        Assert.That(MapleRules.MaxTaps, Is.EqualTo(12));
        Assert.That(MapleRules.CanAddTap(0), Is.True);
        Assert.That(MapleRules.CanAddTap(11), Is.True);
        Assert.That(MapleRules.CanAddTap(12), Is.False);
    }

    // --- Boiling ---

    [Test]
    public void TenLitresOfSapMakeOneSyrup()
    {
        Assert.That(MapleRules.SapPerSyrup, Is.EqualTo(10));
        Assert.That(MapleRules.SyrupFrom(9), Is.EqualTo(0));
        Assert.That(MapleRules.SyrupFrom(10), Is.EqualTo(1));
        Assert.That(MapleRules.SyrupFrom(35), Is.EqualTo(3));
        Assert.That(MapleRules.LitresLeftOver(35), Is.EqualTo(5));
        Assert.That(MapleRules.SyrupFrom(-4), Is.EqualTo(0));
        Assert.That(MapleRules.LitresLeftOver(0), Is.EqualTo(0));
    }

    [Test]
    public void ABoilTakesTwelveSecondsASyrup()
    {
        Assert.That(MapleRules.SecondsPerSyrup, Is.EqualTo(12f).Within(1e-4f));
    }

    [Test]
    public void FourTapsCheckedDailyMakeAboutOneAndAHalfSyrupADay()
    {
        float litresPerDay = 4 * 5.5f * MapleRules.SapThisHour(true, 1.0f);
        float syrupPerDay = litresPerDay / MapleRules.SapPerSyrup;
        Assert.That(syrupPerDay, Is.InRange(1.3f, 1.8f));
    }

    [Test]
    public void TheTapKitCostsWhatTheDesignSays()
    {
        Assert.That(MapleRules.SpileSticks, Is.EqualTo(3));
        Assert.That((MapleRules.BucketLogs, MapleRules.BucketBranches, MapleRules.BucketCordage), Is.EqualTo((1, 3, 1)));
    }

    // --- The market ---

    [Test]
    public void SyrupIsItsOwnGroupAtTenDollars()
    {
        Assert.That(Table.Problems, Is.Empty);
        Assert.That(Table.TryGetRow("maple_syrup", out MarketRow row), Is.True);
        Assert.That(row.group, Is.EqualTo(MarketGroup.Maple));
        Assert.That(row.kind, Is.EqualTo(MarketKind.Bought));
        Assert.That(row.baseCents, Is.EqualTo(1000L));
        Assert.That(row.pool, Is.EqualTo("maple_syrup"));
        Assert.That(Table.Rows.Count(r => r.group == MarketGroup.Maple), Is.EqualTo(1), "nothing else shares the group");
        Assert.That(Table.SellBaseCents("maple_syrup"), Is.EqualTo(1000L));
        Assert.That(Table.HasPool("maple_syrup"), Is.True);
    }

    [Test]
    public void SapIsNotSoldAndNeitherAreTheTapParts()
    {
        foreach (string id in new[] { "maple_sap", "spile", "sap_bucket" })
            Assert.That(Table.TryGetRow(id, out _), Is.False, id + " has no market row");
    }

    [Test]
    public void TheSyrupGroupDropsFivePercentFloorsAtAQuarterAndHasNoSeasons()
    {
        Assert.That(Table.TryGetRule(MarketGroup.Maple, out MarketGroupRule rule), Is.True);
        Assert.That((rule.dropBp, rule.floorBp), Is.EqualTo((500, 2500)));
        Assert.That(new[] { rule.spring, rule.summer, rule.fall, rule.winter }, Is.All.EqualTo(10000));
        Assert.That(rule.SeasonalChange, Is.False);
    }

    [Test]
    public void SyrupSellsAtAFallingPriceThatMatchesThePlan()
    {
        SaleResult two = MarketRules.SellStack(1000, 10000, 2, 500, 2500);
        Assert.That(two.firstUnitCents, Is.EqualTo(1000L));
        Assert.That(two.lastUnitCents, Is.EqualTo(950L));
        Assert.That(two.newMultiplierBp, Is.EqualTo(9025));

        SaleResult many = MarketRules.SellStack(1000, 10000, 100, 500, 2500);
        Assert.That(many.newMultiplierBp, Is.EqualTo(2500), "never below the floor");
    }

    [Test]
    public void SyrupIncomeGoesToFarmProduceAndNoNewLedgerCategoryIsAdded()
    {
        Assert.That(LedgerCategories.ForSale(MarketGroup.Maple), Is.EqualTo(LedgerCategory.SalesFarm));
        Assert.That(LedgerCategories.ForSale(Table.RuleOf("maple_syrup").group), Is.EqualTo(LedgerCategory.SalesFarm));
        Assert.That(LedgerCategories.IsIncome(LedgerCategories.ForSale(MarketGroup.Maple)), Is.True);
        Assert.That(LedgerCategories.Count, Is.EqualTo(19), "no new ledger category");
        Assert.That(LedgerSaveData.CurrentVersion, Is.EqualTo(1));
        Assert.That(MarketRules.GroupName(MarketGroup.Maple), Is.EqualTo("Maple syrup"));
    }

    [Test]
    public void ASyrupSaleIsRecordedOnItsItemRowAndAddsToFarmProduce()
    {
        var ledger = new LedgerState();
        ledger.Begin(100000, 0, 360, 28);
        SaleResult sale = MarketRules.SellStack(1000, 10000, 2, 500, 2500);
        ledger.RecordSale(LedgerCategories.ForSale(MarketGroup.Maple), "maple_syrup", sale.count, sale.totalCents);
        Assert.That(ledger.Open.items.Single().itemId, Is.EqualTo("maple_syrup"));
        Assert.That(ledger.Open.items.Single().units, Is.EqualTo(2));
        Assert.That(ledger.Open.cents[(int)LedgerCategory.SalesFarm], Is.EqualTo(sale.totalCents));
    }

    [Test]
    public void TheMarketGroupIdsOfTheOlderGroupsDidNotMove()
    {
        // MarketData saves the ids: Ash and Maple were appended, so everything before them keeps its number.
        Assert.That((int)MarketGroup.WoodStone, Is.EqualTo(0));
        Assert.That((int)MarketGroup.Stocked, Is.EqualTo(7));
        Assert.That((int)MarketGroup.Ash, Is.EqualTo(8));
        Assert.That((int)MarketGroup.Maple, Is.EqualTo(9));
    }

    [Test]
    public void AnOldMarketSaveWithNoSyrupPoolLoadsSyrupAtNormalPrice()
    {
        var data = new MarketSaveData
        {
            version = MarketSaveData.CurrentVersion,
            lastDay = 12,
            pools = new[] { new MarketPoolSave { pool = "firewood", bp = 6000 } },
        };
        var state = new MarketState();
        state.Restore(Table, data);
        Assert.That(state.Get("firewood"), Is.EqualTo(6000));
        Assert.That(state.Get("maple_syrup"), Is.EqualTo(10000), "a pool the save lacks");
    }

    // --- The items (ItemDatabase and Crafting are in the main assembly, reached by reflection as the other tests do) ---

    static Type Find(string name) =>
        AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => { try { return a.GetTypes(); } catch (Exception) { return new Type[0]; } })
                 .First(t => t.Name == name);

    static object Item(string id) => Find("ItemDatabase").GetMethod("Get").Invoke(null, new object[] { id });

    static object Prop(object item, string name) => item.GetType().GetProperty(name).GetValue(item);

    [Test]
    public void TheFourNewItemsExistWithTheRightKinds()
    {
        object sap = Item(MapleRules.SapId), syrup = Item(MapleRules.SyrupId), spile = Item(MapleRules.SpileId), bucket = Item(MapleRules.SapBucketId);
        Assert.That(new[] { sap, syrup, spile, bucket }, Is.All.Not.Null);

        Assert.That(Prop(sap, "Id"), Is.EqualTo("maple_sap"));
        Assert.That(Prop(sap, "Category").ToString(), Is.EqualTo("Material"));
        Assert.That(Prop(sap, "WeightKg"), Is.EqualTo(1f), "a litre of sap weighs a kilo");
        Assert.That(Prop(sap, "IsFood"), Is.EqualTo(false), "raw sap is not food");
        Assert.That(Prop(sap, "IsPerishable"), Is.EqualTo(false), "sap sours in the bucket; carried sap does not spoil");

        Assert.That(Prop(syrup, "Id"), Is.EqualTo("maple_syrup"));
        Assert.That(Prop(syrup, "Category").ToString(), Is.EqualTo("Consumable"));
        Assert.That(Prop(syrup, "IsFood"), Is.EqualTo(true), "Syrup is a sweetener");
        Assert.That(Prop(syrup, "IllnessChance"), Is.EqualTo(0f));

        Assert.That(Prop(spile, "Id"), Is.EqualTo("spile"));
        Assert.That(Prop(spile, "Category").ToString(), Is.EqualTo("Tool"));
        Assert.That(Prop(bucket, "Id"), Is.EqualTo("sap_bucket"));
        Assert.That(Prop(bucket, "Category").ToString(), Is.EqualTo("Tool"));
    }
}
