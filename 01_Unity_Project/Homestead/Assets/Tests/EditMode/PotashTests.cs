using System;
using System.Linq;
using NUnit.Framework;

// EditMode tests for Potash A (Early_Income_Design.md section 1): the pure rules in PotashRules (Homestead.Economy), the Lye and Potash market
// entries, the Potash Kettle as a stocked item, the ledger lines and the three new items. Nothing here needs a scene, a hopper or the clock;
// the hopper and the Kettle boil are Potash B.
public class PotashTests
{
    static MarketTable Table => MarketCatalog.Table;

    static Type Find(string name) =>
        AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => { try { return a.GetTypes(); } catch (Exception) { return new Type[0]; } })
                 .First(t => t.Name == name);

    static object Item(string id) => Find("ItemDatabase").GetMethod("Get").Invoke(null, new object[] { id });

    static object Prop(object item, string name) => item.GetType().GetProperty(name).GetValue(item);

    // --- The numbers ---

    [Test]
    public void TheIdsAreLyePotashAndTheKettleIdTheTripodAlreadyReserved()
    {
        Assert.That(PotashRules.LyeId, Is.EqualTo("lye"));
        Assert.That(PotashRules.PotashId, Is.EqualTo("potash"));
        Assert.That(TripodRules.KettleId, Is.EqualTo("potash_kettle"));
        Assert.That(typeof(PotashRules).GetField("KettleId"), Is.Null, "the Kettle id has one home, TripodRules");
        Assert.That(TripodRules.IsVessel("potash_kettle"), Is.True);
    }

    [Test]
    public void OneLyeIsOneAshAndOneLitreOfWater()
    {
        Assert.That(PotashRules.AshPerLye, Is.EqualTo(1));
        Assert.That(PotashRules.WaterLitresPerLye, Is.EqualTo(1));
    }

    [Test]
    public void TheHopperHoldsTwelveLyeAndLeachesForADayOfGameTime()
    {
        Assert.That(PotashRules.HopperCapacity, Is.EqualTo(12));
        Assert.That(PotashRules.LeachGameHours, Is.EqualTo(24f));
    }

    [Test]
    public void OnePotashIsThreeLyeAndTwoFirewoodBoiledForEighteenSeconds()
    {
        Assert.That(PotashRules.LyePerPotash, Is.EqualTo(3));
        Assert.That(PotashRules.FirewoodPerPotash, Is.EqualTo(2));
        Assert.That(PotashRules.SecondsPerLye, Is.EqualTo(6f));
        Assert.That(PotashRules.BoilSeconds, Is.EqualTo(18f));
        Assert.That(PotashRules.BoilSeconds, Is.EqualTo(PotashRules.LyePerPotash * PotashRules.SecondsPerLye), "6 s for each Lye");
    }

    // --- Leaching ---

    [TestCase(12, 12, 12, 12)]
    [TestCase(5, 12, 12, 5)]
    [TestCase(12, 3, 12, 3)]
    [TestCase(12, 12, 4, 4)]
    [TestCase(30, 40, 12, 12)]
    [TestCase(0, 12, 12, 0)]
    [TestCase(12, 0, 12, 0)]
    [TestCase(12, 12, 0, 0)]
    [TestCase(-3, 12, 12, 0)]
    public void ALyeBatchIsLimitedByTheAshTheWaterAndTheRoomInTheHopper(int ash, int litres, int room, int expected)
    {
        Assert.That(PotashRules.LyeBatch(ash, litres, room), Is.EqualTo(expected));
    }

    [Test]
    public void TheAshAndWaterAFullHopperTakes()
    {
        Assert.That(PotashRules.AshFor(12), Is.EqualTo(12));
        Assert.That(PotashRules.WaterFor(12), Is.EqualTo(12));
        Assert.That(PotashRules.AshFor(0), Is.EqualTo(0));
        Assert.That(PotashRules.WaterFor(-1), Is.EqualTo(0));
    }

    [TestCase(0f, 24f, false)]
    [TestCase(23.9f, 0.1f, false)]
    [TestCase(24f, 0f, true)]
    [TestCase(100f, 0f, true)]
    [TestCase(-5f, 24f, false)]
    public void ABatchIsLeachedAfterTwentyFourGameHours(float elapsed, float left, bool done)
    {
        Assert.That(PotashRules.IsLeached(elapsed), Is.EqualTo(done));
        Assert.That(PotashRules.LeachHoursLeft(elapsed), Is.EqualTo(left).Within(0.001f));
    }

    // --- Boiling ---

    [TestCase(3, 2, 1)]
    [TestCase(2, 2, 0)]
    [TestCase(3, 1, 0)]
    [TestCase(9, 6, 3)]
    [TestCase(9, 4, 2)]
    [TestCase(12, 100, 4)]
    [TestCase(100, 4, 2)]
    [TestCase(0, 10, 0)]
    [TestCase(10, 0, 0)]
    public void PotashBatchesAreLimitedByTheLyeAndTheFirewood(int lye, int firewood, int batches)
    {
        Assert.That(PotashRules.PotashBatches(lye, firewood), Is.EqualTo(batches));
    }

    [Test]
    public void WhatBatchesOfPotashCost()
    {
        Assert.That(PotashRules.LyeFor(1), Is.EqualTo(3));
        Assert.That(PotashRules.LyeFor(4), Is.EqualTo(12), "a full hopper is four batches");
        Assert.That(PotashRules.FirewoodFor(4), Is.EqualTo(8));
        Assert.That(PotashRules.SecondsFor(1), Is.EqualTo(18f));
        Assert.That(PotashRules.SecondsFor(4), Is.EqualTo(72f));
        Assert.That(PotashRules.LyeFor(0), Is.EqualTo(0));
        Assert.That(PotashRules.FirewoodFor(-2), Is.EqualTo(0));
        Assert.That(PotashRules.SecondsFor(0), Is.EqualTo(0f));
    }

    [Test]
    public void AFullHopperMakesExactlyWholeBatches()
    {
        Assert.That(PotashRules.HopperCapacity % PotashRules.LyePerPotash, Is.EqualTo(0), "12 Lye is 4 Potash with nothing left over");
    }

    // --- The market: Lye and Potash ---

    [Test]
    public void LyeAndPotashAreBoughtItemsInTheirOwnGroupEachInItsOwnPool()
    {
        Assert.That(Table.Problems, Is.Empty);
        Assert.That(Table.TryGetRow("lye", out MarketRow lye), Is.True);
        Assert.That(Table.TryGetRow("potash", out MarketRow potash), Is.True);
        Assert.That(lye.group, Is.EqualTo(MarketGroup.Potash));
        Assert.That(potash.group, Is.EqualTo(MarketGroup.Potash));
        Assert.That(lye.kind, Is.EqualTo(MarketKind.Bought));
        Assert.That(potash.kind, Is.EqualTo(MarketKind.Bought));
        Assert.That(lye.pool, Is.EqualTo("lye"));
        Assert.That(potash.pool, Is.EqualTo("potash"));
        Assert.That(Table.Rows.Count(r => r.group == MarketGroup.Potash), Is.EqualTo(2), "nothing else shares the group");
        Assert.That(Table.HasPool("lye"), Is.True);
        Assert.That(Table.HasPool("potash"), Is.True);
    }

    [Test]
    public void LyeSellsForThreeDollarsAndPotashForSixteen()
    {
        Assert.That(PotashRules.LyeBaseCents, Is.EqualTo(300L));
        Assert.That(PotashRules.PotashBaseCents, Is.EqualTo(1600L));
        Assert.That(Table.SellBaseCents("lye"), Is.EqualTo(300L));
        Assert.That(Table.SellBaseCents("potash"), Is.EqualTo(1600L));
        Assert.That(Table.BuyBaseCents("lye"), Is.EqualTo(0L), "the store does not sell them");
        Assert.That(Table.BuyBaseCents("potash"), Is.EqualTo(0L));
    }

    [Test]
    public void ThePotashGroupDropsFivePercentFloorsAtAQuarterAndHasNoSeasons()
    {
        Assert.That(Table.TryGetRule(MarketGroup.Potash, out MarketGroupRule rule), Is.True);
        Assert.That((rule.dropBp, rule.floorBp), Is.EqualTo((500, 2500)));
        Assert.That(new[] { rule.spring, rule.summer, rule.fall, rule.winter }, Is.All.EqualTo(10000), "flat all year");
        Assert.That(rule.SeasonalChange, Is.False);
        Assert.That(PotashRules.DropBp, Is.EqualTo(500));
        Assert.That(PotashRules.FloorBp, Is.EqualTo(2500));
    }

    [Test]
    public void SellingLyeFallsFivePercentAUnitAndStopsAtTheFloor()
    {
        SaleResult four = MarketRules.SellStack(300, 10000, 4, PotashRules.DropBp, PotashRules.FloorBp);
        Assert.That(four.firstUnitCents, Is.EqualTo(300L));
        Assert.That(four.lastUnitCents, Is.LessThan(four.firstUnitCents));
        Assert.That(four.newMultiplierBp, Is.EqualTo(8145).Within(5), "about 95% a unit: 0.95 to the 4th");

        SaleResult many = MarketRules.SellStack(300, 10000, 100, PotashRules.DropBp, PotashRules.FloorBp);
        Assert.That(many.newMultiplierBp, Is.EqualTo(2500), "never below a quarter of normal");
        Assert.That(many.lastUnitCents, Is.EqualTo(75L));

        SaleResult potash = MarketRules.SellStack(1600, 10000, 100, PotashRules.DropBp, PotashRules.FloorBp);
        Assert.That(potash.lastUnitCents, Is.EqualTo(400L), "Potash floors at $4.00");
    }

    [Test]
    public void LyeAndPotashHaveSeparatePoolsSoSellingOneDoesNotMoveTheOther()
    {
        var state = new MarketState();
        state.BeginNewGame();
        state.Advance(Table, 10, 28, 25);
        state.Sell(Table, "lye", 10);
        Assert.That(state.Get("lye"), Is.LessThan(10000));
        Assert.That(state.Get("potash"), Is.EqualTo(10000), "Potash is untouched by Lye sales");
        Assert.That(state.Get("ash"), Is.EqualTo(10000), "and so is Ash");
        state.Sell(Table, "potash", 3);
        Assert.That(state.Get("potash"), Is.LessThan(10000));
    }

    [Test]
    public void ThePotashPoolsRecoverAQuarterOfTheGapEachDay()
    {
        Assert.That(MarketRules.Recover(5000, 10000, 25, PotashRules.FloorBp), Is.EqualTo(6250));

        var state = new MarketState();
        state.BeginNewGame();
        state.Advance(Table, 10, 28, 25);
        state.Sell(Table, "lye", 10);
        int sold = state.Get("lye");
        state.Advance(Table, 11, 28, 25);
        int gap = 10000 - sold;
        Assert.That(state.Get("lye"), Is.EqualTo(sold + (gap + 2) / 4).Within(2), "a quarter of the gap back after one day");
        state.Advance(Table, 60, 28, 25);
        Assert.That(state.Get("lye"), Is.EqualTo(10000), "back to normal in the end, the same in every season");
    }

    [Test]
    public void AnOldMarketSaveWithNoLyeOrPotashPoolLoadsThemAtNormalPrice()
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
        Assert.That(state.Get("lye"), Is.EqualTo(10000));
        Assert.That(state.Get("potash"), Is.EqualTo(10000));
        Assert.That(MarketSaveData.CurrentVersion, Is.EqualTo(1));
    }

    [Test]
    public void TheNewPoolsAreSavedWithTheMarket()
    {
        var state = new MarketState();
        state.BeginNewGame();
        state.Advance(Table, 3, 28, 25);
        state.Sell(Table, "potash", 2);
        MarketSaveData data = state.Capture(Table);
        Assert.That(data.pools.Any(p => p.pool == "lye"));
        Assert.That(data.pools.Single(p => p.pool == "potash").bp, Is.EqualTo(state.Get("potash")));

        var back = new MarketState();
        back.Restore(Table, data);
        Assert.That(back.Get("potash"), Is.EqualTo(state.Get("potash")));
    }

    [Test]
    public void AshIsWorthLessThanLyeWhichIsWorthLessThanPotashPerAshAtFreshPrices()
    {
        long ash = Table.SellBaseCents("ash");
        long lye = Table.SellBaseCents("lye") / PotashRules.AshPerLye;
        long potash = Table.SellBaseCents("potash") / (PotashRules.LyePerPotash * PotashRules.AshPerLye);
        Assert.That((ash, lye, potash), Is.EqualTo((200L, 300L, 533L)));
        Assert.That(ash, Is.LessThan(lye));
        Assert.That(lye, Is.LessThan(potash));
        Assert.That(PotashRules.LyeCentsPerAsh, Is.EqualTo(lye));
        Assert.That(PotashRules.PotashCentsPerAsh, Is.EqualTo(potash));
    }

    [Test]
    public void TheOrderHoldsAfterFiveOfEachAreSold()
    {
        // Selling 5 of each into its own pool, then what one more Ash is worth as Ash, as Lye and as Potash (three Ash).
        var state = new MarketState();
        state.BeginNewGame();
        state.Advance(Table, 10, 28, 25);
        state.Sell(Table, "ash", 5);
        state.Sell(Table, "lye", 5);
        state.Sell(Table, "potash", 5);

        long ash = MarketRules.UnitPrice(Table.SellBaseCents("ash"), state.Get("ash"));
        long lye = MarketRules.UnitPrice(Table.SellBaseCents("lye"), state.Get("lye")) / PotashRules.AshPerLye;
        long potash = MarketRules.UnitPrice(Table.SellBaseCents("potash"), state.Get("potash")) / (PotashRules.LyePerPotash * PotashRules.AshPerLye);
        Assert.That(ash, Is.LessThan(lye));
        Assert.That(lye, Is.LessThan(potash));
    }

    [Test]
    public void TheOrderHoldsEvenAtTheFloorOfEveryPool()
    {
        long ash = MarketRules.UnitPrice(200, AshRules.FloorBp);
        long lye = MarketRules.UnitPrice(300, PotashRules.FloorBp);
        long potash = MarketRules.UnitPrice(1600, PotashRules.FloorBp) / 3;
        Assert.That(ash, Is.LessThan(lye));
        Assert.That(lye, Is.LessThan(potash));
    }

    [Test]
    public void TheAshMarketIsUnchanged()
    {
        Assert.That(Table.TryGetRow("ash", out MarketRow row), Is.True);
        Assert.That((row.group, row.kind, row.baseCents, row.pool), Is.EqualTo((MarketGroup.Ash, MarketKind.Bought, 200L, "ash")));
        Assert.That(Table.TryGetRule(MarketGroup.Ash, out MarketGroupRule rule), Is.True);
        Assert.That((rule.dropBp, rule.floorBp), Is.EqualTo((600, 2500)));
        Assert.That(new[] { rule.spring, rule.summer, rule.fall, rule.winter }, Is.All.EqualTo(10000));
        Assert.That(Table.Rows.Count(r => r.group == MarketGroup.Ash), Is.EqualTo(1));
    }

    [Test]
    public void ThePotashGroupWasAppendedAfterMapleSoTheSavedGroupIdsHold()
    {
        Assert.That((int)MarketGroup.Stocked, Is.EqualTo(7));
        Assert.That((int)MarketGroup.Ash, Is.EqualTo(8));
        Assert.That((int)MarketGroup.Maple, Is.EqualTo(9));
        Assert.That((int)MarketGroup.Potash, Is.EqualTo(10));
    }

    // --- The Potash Kettle ---

    [Test]
    public void TheKettleIsAStockedTradingPostItemAtTwentyFiveDollars()
    {
        Assert.That(Table.TryGetRow(TripodRules.KettleId, out MarketRow row), Is.True);
        Assert.That(row.kind, Is.EqualTo(MarketKind.Stocked));
        Assert.That(row.group, Is.EqualTo(MarketGroup.Stocked));
        Assert.That(row.baseCents, Is.EqualTo(2500L));
        Assert.That(row.seasonalBuy, Is.False);
        Assert.That(PotashRules.KettleBaseCents, Is.EqualTo(2500L));
        Assert.That(Table.BuyBaseCents(TripodRules.KettleId), Is.EqualTo(2500L));
        Assert.That(Table.Stock.Any(r => r.itemId == TripodRules.KettleId), Is.True, "it is on the Trading Post's shelf");
    }

    [Test]
    public void TheKettleSellsBackForTenDollars()
    {
        Assert.That(Table.SellBaseCents(TripodRules.KettleId), Is.EqualTo(1000L), "40% of $25.00");
        Assert.That(Table.HasPool(TripodRules.KettleId), Is.True);
    }

    [Test]
    public void TheStockedShelfIsTwentyItemsTotallingTwoHundredEightyDollars()
    {
        Assert.That(Table.Stock.Count, Is.EqualTo(20));
        Assert.That(Table.Stock.Sum(r => r.baseCents), Is.EqualTo(28000L));
        Assert.That(Table.Stock.Count(r => Table.SellBaseCents(r.itemId) > 0), Is.EqualTo(17));
    }

    [Test]
    public void TheTripodStillHasNoMarketRow()
    {
        Assert.That(Table.TryGetRow(TripodRules.TripodId, out _), Is.False);
    }

    // --- The ledger ---

    [Test]
    public void LyeAndPotashSalesCountWithTheWoodLine()
    {
        Assert.That(LedgerCategories.ForSale(MarketGroup.Potash), Is.EqualTo(LedgerCategory.SalesWoodStone));
        Assert.That(LedgerCategories.ForSale(Table.RuleOf("lye").group), Is.EqualTo(LedgerCategory.SalesWoodStone));
        Assert.That(LedgerCategories.ForSale(Table.RuleOf("potash").group), Is.EqualTo(LedgerCategory.SalesWoodStone));
        Assert.That(LedgerCategories.IsIncome(LedgerCategories.ForSale(MarketGroup.Potash)), Is.True);
        Assert.That(MarketRules.GroupName(MarketGroup.Potash), Is.EqualTo("Lye and Potash"));
    }

    [Test]
    public void TheKettlePurchaseIsCookingWaterAndFire()
    {
        Assert.That(LedgerCategories.PurchaseIsListed(TripodRules.KettleId), Is.True);
        Assert.That(LedgerCategories.ForPurchase(TripodRules.KettleId), Is.EqualTo(LedgerCategory.BuyCookingWaterFire));
        Assert.That(LedgerCategories.IsIncome(LedgerCategories.ForPurchase(TripodRules.KettleId)), Is.False);
        Assert.That(LedgerCategories.ForPurchase(TripodRules.PotId), Is.EqualTo(LedgerCategories.ForPurchase(TripodRules.KettleId)), "the same line as the Pot");
    }

    [Test]
    public void ThereIsNoNewLedgerCategoryAndTheSaveVersionStaysOne()
    {
        Assert.That(LedgerCategories.Count, Is.EqualTo(19));
        Assert.That(LedgerSaveData.CurrentVersion, Is.EqualTo(1));
    }

    [Test]
    public void ALyeSaleIsRecordedOnItsItemRowAndAddsToTheWoodLine()
    {
        var ledger = new LedgerState();
        ledger.Begin(100000, 0, 360, 28);
        SaleResult lye = MarketRules.SellStack(300, 10000, 4, PotashRules.DropBp, PotashRules.FloorBp);
        SaleResult potash = MarketRules.SellStack(1600, 10000, 2, PotashRules.DropBp, PotashRules.FloorBp);
        ledger.RecordSale(LedgerCategories.ForSale(MarketGroup.Potash), "lye", lye.count, lye.totalCents);
        ledger.RecordSale(LedgerCategories.ForSale(MarketGroup.Potash), "potash", potash.count, potash.totalCents);

        Assert.That(ledger.Open.items.Select(i => i.itemId).OrderBy(s => s), Is.EqualTo(new[] { "lye", "potash" }), "the item CSV tells them apart");
        Assert.That(ledger.Open.items.Single(i => i.itemId == "lye").units, Is.EqualTo(4));
        Assert.That(ledger.Open.cents[(int)LedgerCategory.SalesWoodStone], Is.EqualTo(lye.totalCents + potash.totalCents));
    }

    // --- The items ---

    [Test]
    public void LyeAndPotashAreMaterialsAndTheKettleIsATool()
    {
        object lye = Item("lye"), potash = Item("potash"), kettle = Item("potash_kettle");
        Assert.That(lye, Is.Not.Null);
        Assert.That(potash, Is.Not.Null);
        Assert.That(kettle, Is.Not.Null);
        Assert.That(Prop(lye, "DisplayName"), Is.EqualTo("Lye"));
        Assert.That(Prop(potash, "DisplayName"), Is.EqualTo("Potash"));
        Assert.That(Prop(kettle, "DisplayName"), Is.EqualTo("Potash Kettle"));
        Assert.That(Prop(lye, "Category").ToString(), Is.EqualTo("Material"));
        Assert.That(Prop(potash, "Category").ToString(), Is.EqualTo("Material"));
        Assert.That(Prop(kettle, "Category").ToString(), Is.EqualTo("Tool"));
    }

    [Test]
    public void TheNewItemsAreNotFoodAndNeverSpoil()
    {
        foreach (string id in new[] { "lye", "potash", "potash_kettle" })
        {
            object item = Item(id);
            Assert.That(Prop(item, "Id"), Is.EqualTo(id));
            Assert.That(Prop(item, "IsFood"), Is.EqualTo(false), id);
            Assert.That(Prop(item, "IsPerishable"), Is.EqualTo(false), id);
            Assert.That((float)Prop(item, "WeightKg"), Is.GreaterThan(0f), id);
        }
    }

    [Test]
    public void EveryNewMarketRowHasAnItemAndEveryNewItemHasAMarketRow()
    {
        foreach (string id in new[] { "lye", "potash", "potash_kettle" })
        {
            Assert.That(Item(id), Is.Not.Null, id + " has an item");
            Assert.That(Table.TryGetRow(id, out _), Is.True, id + " has a market row");
        }
    }
}
