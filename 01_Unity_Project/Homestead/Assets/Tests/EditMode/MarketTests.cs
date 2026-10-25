using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

// EditMode tests for the Trading Post market (Trading_Post_Market.md): MarketRules, MarketState and the item catalog in Homestead.Economy.
// All pure except the last group, which reads the MarketData asset as text. Hand-computed numbers come from an independent model of the
// rules (integer basis points, round half up).
public class MarketTests
{
    static MarketTable Table => MarketCatalog.Table;
    const int DaysPerSeason = 28;
    const int Recovery = 25;

    static MarketState Fresh() => new MarketState();

    static MarketSaveData Snapshot(MarketState state) => state.Capture(Table);

    static void AssertSame(MarketSaveData a, MarketSaveData b)
    {
        Assert.That(a.lastDay, Is.EqualTo(b.lastDay), "last day");
        Assert.That(a.pools.Length, Is.EqualTo(b.pools.Length));
        for (int i = 0; i < a.pools.Length; i++)
        {
            Assert.That(a.pools[i].pool, Is.EqualTo(b.pools[i].pool));
            Assert.That(a.pools[i].bp, Is.EqualTo(b.pools[i].bp), a.pools[i].pool);
        }
    }

    // --- Selling pressure ---

    [Test]
    public void OneUnitPaysTheCurrentPriceThenDropsByTheGroupRate()
    {
        SaleResult sale = MarketRules.SellStack(100, 10000, 1, 400, 2500);
        Assert.That(sale.count, Is.EqualTo(1));
        Assert.That(sale.totalCents, Is.EqualTo(100L));
        Assert.That(sale.firstUnitCents, Is.EqualTo(100L));
        Assert.That(sale.lastUnitCents, Is.EqualTo(100L));
        Assert.That(sale.newMultiplierBp, Is.EqualTo(9600));
    }

    [Test]
    public void TenFirewoodSellUnitByUnitAtTheFallingPrice()
    {
        SaleResult sale = MarketRules.SellStack(100, 10000, 10, 400, 2500);
        Assert.That(sale.totalCents, Is.EqualTo(837L), "100+96+92+88+85+82+78+75+72+69");
        Assert.That(sale.firstUnitCents, Is.EqualTo(100L));
        Assert.That(sale.lastUnitCents, Is.EqualTo(69L));
        Assert.That(sale.newMultiplierBp, Is.EqualTo(6647));
    }

    [Test]
    public void TheTotalIsTheSumOfTheRoundedUnitPrices()
    {
        long sum = 0;
        int m = 10000;
        for (int i = 0; i < 40; i++)
        {
            sum += MarketRules.UnitPrice(100, m);
            m = MarketRules.Drop(m, 400, 2500);
        }

        SaleResult sale = MarketRules.SellStack(100, 10000, 40, 400, 2500);
        Assert.That(sale.totalCents, Is.EqualTo(sum));
        Assert.That(sale.totalCents, Is.EqualTo(2024L));
    }

    [Test]
    public void TheMultiplierSequenceForFirewoodMatchesTheModel()
    {
        int[] expected = { 9600, 9216, 8847, 8493, 8153, 7827, 7514, 7213, 6924, 6647, 6381, 6126 };
        int m = 10000;
        foreach (int want in expected)
        {
            m = MarketRules.Drop(m, 400, 2500);
            Assert.That(m, Is.EqualTo(want));
        }
    }

    [Test]
    public void TheFirstFloorOf025IsReachedOnTheThirtyFourthFirewoodAndHolds()
    {
        int m = 10000;
        for (int sold = 1; sold <= 33; sold++)
            m = MarketRules.Drop(m, 400, 2500);
        Assert.That(m, Is.GreaterThan(2500), "33 sold: still above the floor");
        m = MarketRules.Drop(m, 400, 2500);
        Assert.That(m, Is.EqualTo(2500), "34th sold: the floor");
        Assert.That(MarketRules.Drop(m, 400, 2500), Is.EqualTo(2500), "and it holds");
    }

    [Test]
    public void CraftedGoodsDropTenPercentAndReachTheirFloorOf01OnTheTwentySecondUnit()
    {
        int m = 10000;
        for (int sold = 1; sold <= 21; sold++)
            m = MarketRules.Drop(m, 1000, 1000);
        Assert.That(m, Is.GreaterThan(1000));
        m = MarketRules.Drop(m, 1000, 1000);
        Assert.That(m, Is.EqualTo(1000));
        Assert.That(MarketRules.Drop(m, 1000, 1000), Is.EqualTo(1000));
    }

    [Test]
    public void TwentyLanternsAtTheCraftedRates()
    {
        SaleResult sale = MarketRules.SellStack(400, 10000, 20, 1000, 1000);
        Assert.That(sale.totalCents, Is.EqualTo(3515L));
        Assert.That(sale.lastUnitCents, Is.EqualTo(54L));
        Assert.That(sale.newMultiplierBp, Is.EqualTo(1217));
    }

    [Test]
    public void SellingNothingChangesNothing()
    {
        SaleResult sale = MarketRules.SellStack(100, 7000, 0, 400, 2500);
        Assert.That(sale.count, Is.EqualTo(0));
        Assert.That(sale.totalCents, Is.EqualTo(0L));
        Assert.That(sale.newMultiplierBp, Is.EqualTo(7000));
    }

    [Test]
    public void AQuoteChangesNothingAndASaleKeepsTheLowerMultiplier()
    {
        MarketState state = Fresh();
        SaleResult quote = state.Quote(Table, "firewood", 10);
        Assert.That(quote.totalCents, Is.EqualTo(837L));
        Assert.That(state.Get("firewood"), Is.EqualTo(10000), "a quote sells nothing");

        SaleResult sale = state.Sell(Table, "firewood", 10);
        Assert.That(sale.totalCents, Is.EqualTo(quote.totalCents), "the quote was exact");
        Assert.That(state.Get("firewood"), Is.EqualTo(6647));
    }

    [Test]
    public void CookedFishSharesTheRawFishPrice()
    {
        MarketState state = Fresh();
        state.Sell(Table, "bass", 3);
        Assert.That(state.Get("bass"), Is.EqualTo(8574));
        Assert.That(state.Quote(Table, "cooked_bass", 1).firstUnitCents, Is.EqualTo(MarketRules.UnitPrice(300, 8574)), "the cooked price is its own base times the shared multiplier");
    }

    [Test]
    public void SellBacksDropEightPercentAUnit()
    {
        MarketState state = Fresh();
        SaleResult sale = state.Sell(Table, "flint_and_steel", 3);
        Assert.That(sale.totalCents, Is.EqualTo(830L), "$3.00 + $2.76 + $2.54");
        Assert.That(state.Get("flint_and_steel"), Is.EqualTo(7787));
    }

    [Test]
    public void AnItemTheStoreDoesNotBuyDoesNothing()
    {
        MarketState state = Fresh();
        Assert.That(state.Sell(Table, "arrows", 5).count, Is.EqualTo(0));
        Assert.That(state.Sell(Table, "no_such_item", 5).count, Is.EqualTo(0));
    }

    // --- Rounding ---

    [Test]
    public void AUnitPriceRoundsHalfUpToTheCent()
    {
        Assert.That(MarketRules.UnitPrice(100, 9950), Is.EqualTo(100L), "99.5 cents");
        Assert.That(MarketRules.UnitPrice(100, 9949), Is.EqualTo(99L), "99.49 cents");
        Assert.That(MarketRules.UnitPrice(1, 5000), Is.EqualTo(1L), "half a cent");
        Assert.That(MarketRules.UnitPrice(1, 4999), Is.EqualTo(0L));
        Assert.That(MarketRules.UnitPrice(300, 10000), Is.EqualTo(300L));
    }

    // --- Recovery ---

    [Test]
    public void RecoveryMovesAQuarterOfTheGapRoundedHalfUp()
    {
        Assert.That(MarketRules.Recover(2500, 10000, 25, 2500), Is.EqualTo(4375));
        Assert.That(MarketRules.Recover(4375, 10000, 25, 2500), Is.EqualTo(5781), "1406.25 rounds down");
        Assert.That(MarketRules.Recover(5781, 10000, 25, 2500), Is.EqualTo(6836), "1054.75 rounds up");
    }

    [Test]
    public void RecoveryLandsOnTheTargetOnceTheGapIsUnderTenBasisPoints()
    {
        Assert.That(MarketRules.Recover(9991, 10000, 25, 2500), Is.EqualTo(10000), "gap 9");
        Assert.That(MarketRules.Recover(9990, 10000, 25, 2500), Is.EqualTo(9993), "gap 10 moves 2.5 and rounds up to 3");
        Assert.That(MarketRules.Recover(10000, 10000, 25, 2500), Is.EqualTo(10000));
    }

    [Test]
    public void RecoveryFromAboveRoundsHalfUpTowardTheTarget()
    {
        Assert.That(MarketRules.Recover(10010, 10000, 25, 2500), Is.EqualTo(10008), "-2.5 rounds up to -2");
        Assert.That(MarketRules.Recover(12000, 10000, 25, 2500), Is.EqualTo(11500));
    }

    [Test]
    public void RecoveryNeverGoesBelowTheFloorOrAboveTwo()
    {
        Assert.That(MarketRules.Recover(1000, 0, 25, 2500), Is.EqualTo(2500), "a stray low value is lifted to the floor");
        Assert.That(MarketRules.Recover(30000, 30000, 25, 2500), Is.EqualTo(MarketRules.MaxBp));
    }

    [Test]
    public void AGluttedPriceIsMostOfTheWayBackInNineDaysAndExactlyBackInTwentyFive()
    {
        int m = 2500;
        for (int day = 1; day <= 9; day++)
            m = MarketRules.Recover(m, 10000, 25, 2500);
        Assert.That(m, Is.EqualTo(9437), "within 8% of the starting gap after 9 days");

        for (int day = 10; day <= 24; day++)
            m = MarketRules.Recover(m, 10000, 25, 2500);
        Assert.That(m, Is.LessThan(10000));
        m = MarketRules.Recover(m, 10000, 25, 2500);
        Assert.That(m, Is.EqualTo(10000));
    }

    // --- Seasons ---

    [TestCase(0, 0)]
    [TestCase(27, 0)]
    [TestCase(28, 1)]
    [TestCase(55, 1)]
    [TestCase(56, 2)]
    [TestCase(83, 2)]
    [TestCase(84, 3)]
    [TestCase(111, 3)]
    [TestCase(112, 0)]
    [TestCase(223, 3)]
    [TestCase(224, 0)]
    public void TheSeasonOfADay(int day, int season)
    {
        Assert.That(MarketRules.SeasonOf(day, DaysPerSeason), Is.EqualTo(season));
    }

    [Test]
    public void ABadDayOrSeasonLengthStillGivesASeason()
    {
        Assert.That(MarketRules.SeasonOf(-5, 28), Is.EqualTo(0));
        Assert.That(MarketRules.SeasonOf(5, 0), Is.EqualTo(1), "a season length under 1 counts as 1");
    }

    [Test]
    public void TheApprovedSeasonalTargetsAndRates()
    {
        MarketGroupRule Rule(MarketGroup g) { Table.TryGetRule(g, out MarketGroupRule r); return r; }

        MarketGroupRule wood = Rule(MarketGroup.WoodStone);
        Assert.That((wood.dropBp, wood.floorBp, wood.spring, wood.summer, wood.fall, wood.winter), Is.EqualTo((400, 2500, 10000, 8000, 10000, 15000)));
        MarketGroupRule fish = Rule(MarketGroup.Fish);
        Assert.That((fish.dropBp, fish.floorBp, fish.spring, fish.summer, fish.fall, fish.winter), Is.EqualTo((500, 2500, 10000, 9000, 10000, 10000)));
        MarketGroupRule game = Rule(MarketGroup.GameMeat);
        Assert.That((game.dropBp, game.floorBp, game.spring, game.summer, game.fall, game.winter), Is.EqualTo((500, 2500, 10000, 10000, 10000, 13000)));
        MarketGroupRule hides = Rule(MarketGroup.Hides);
        Assert.That((hides.dropBp, hides.floorBp, hides.spring, hides.summer, hides.fall, hides.winter), Is.EqualTo((500, 2500, 10000, 10000, 10000, 13000)));
        MarketGroupRule forage = Rule(MarketGroup.Forage);
        Assert.That((forage.dropBp, forage.floorBp, forage.spring, forage.summer, forage.fall, forage.winter), Is.EqualTo((600, 2500, 8500, 8500, 8500, 8500)));
        MarketGroupRule crafted = Rule(MarketGroup.Crafted);
        Assert.That((crafted.dropBp, crafted.floorBp, crafted.spring, crafted.summer, crafted.fall, crafted.winter), Is.EqualTo((1000, 1000, 10000, 10000, 10000, 10000)));
        MarketGroupRule farm = Rule(MarketGroup.Farm);
        Assert.That((farm.dropBp, farm.floorBp, farm.spring, farm.summer, farm.fall, farm.winter), Is.EqualTo((500, 2500, 10000, 10000, 10000, 10000)));
        MarketGroupRule stocked = Rule(MarketGroup.Stocked);
        Assert.That((stocked.dropBp, stocked.floorBp, stocked.spring, stocked.summer, stocked.fall, stocked.winter), Is.EqualTo((800, 2500, 10000, 10000, 10000, 10000)));
    }

    // --- Advancing through days ---

    static MarketState Depressed()
    {
        var state = Fresh();
        state.Advance(Table, 10, DaysPerSeason, Recovery);
        state.Sell(Table, "firewood", 30);
        state.Sell(Table, "lantern", 15);
        state.Sell(Table, "bass", 12);
        state.Sell(Table, "blackberries", 20);
        state.Sell(Table, "deer_hide", 9);
        return state;
    }

    [TestCase(10, 40)]
    [TestCase(10, 70)]   // across the Summer and Fall boundaries
    [TestCase(80, 90)]   // across the Fall to Winter boundary at day 84
    [TestCase(100, 130)] // across Winter to Spring and the year wrap at 112
    public void ManyDaysInOneCallEqualTheSameDaysOneAtATime(int from, int to)
    {
        MarketState bulk = Depressed(), singles = Depressed();
        bulk.Advance(Table, from, DaysPerSeason, Recovery);
        singles.Advance(Table, from, DaysPerSeason, Recovery);
        bulk.Sell(Table, "stone", 50);
        singles.Sell(Table, "stone", 50);

        bulk.Advance(Table, to, DaysPerSeason, Recovery);
        for (int day = from + 1; day <= to; day++)
            singles.Advance(Table, day, DaysPerSeason, Recovery);

        AssertSame(Snapshot(singles), Snapshot(bulk));
    }

    [Test]
    public void AdvancingToTheSameDayAgainChangesNothing()
    {
        MarketState state = Depressed();
        state.Advance(Table, 25, DaysPerSeason, Recovery);
        MarketSaveData once = Snapshot(state);
        state.Advance(Table, 25, DaysPerSeason, Recovery);
        state.Advance(Table, 25, DaysPerSeason, Recovery);
        AssertSame(once, Snapshot(state));
    }

    [Test]
    public void ASleepAcrossASeasonBoundaryUsesTheNewSeasonsTargetFromItsFirstDay()
    {
        MarketState state = Fresh();
        state.Advance(Table, 82, DaysPerSeason, Recovery);
        state.Set("firewood", 10000);

        state.Advance(Table, 83, DaysPerSeason, Recovery);
        Assert.That(state.Get("firewood"), Is.EqualTo(10000), "day 83 is still Fall, target 1.0");
        state.Advance(Table, 84, DaysPerSeason, Recovery);
        Assert.That(state.Get("firewood"), Is.EqualTo(11250), "day 84 is Winter, target 1.5");

        MarketState slept = Fresh();
        slept.Advance(Table, 82, DaysPerSeason, Recovery);
        slept.Set("firewood", 10000);
        slept.Advance(Table, 86, DaysPerSeason, Recovery);
        Assert.That(slept.Get("firewood"), Is.EqualTo(12891), "11250, 12188, 12891: the same as the days one by one");
    }

    [Test]
    public void ATimeThatWentBackwardsChangesNoPrices()
    {
        MarketState state = Depressed();
        MarketSaveData before = Snapshot(state);
        state.Advance(Table, 3, DaysPerSeason, Recovery);
        Assert.That(state.LastDay, Is.EqualTo(3));
        Assert.That(Snapshot(state).pools.Select(p => p.bp), Is.EqualTo(before.pools.Select(p => p.bp)));
    }

    // --- New games, old saves, saves ---

    [Test]
    public void ANewGameStartsEveryPoolAtItsCurrentSeasonTarget()
    {
        var winter = Fresh();
        winter.BeginNewGame();
        winter.Advance(Table, 84, DaysPerSeason, Recovery);
        Assert.That(winter.Get("firewood"), Is.EqualTo(15000));
        Assert.That(winter.Get("venison"), Is.EqualTo(13000));
        Assert.That(winter.Get("deer_hide"), Is.EqualTo(13000));
        Assert.That(winter.Get("bass"), Is.EqualTo(10000));
        Assert.That(winter.Get("blackberries"), Is.EqualTo(8500));
        Assert.That(winter.Get("lantern"), Is.EqualTo(10000));
        Assert.That(winter.LastDay, Is.EqualTo(84));

        var spring = Fresh();
        spring.BeginNewGame();
        spring.Advance(Table, 0, DaysPerSeason, Recovery);
        Assert.That(spring.Get("firewood"), Is.EqualTo(10000));
        Assert.That(spring.Get("blackberries"), Is.EqualTo(8500), "forage starts at its 0.85");
    }

    [Test]
    public void ASaveWithNoMarketStartsNormalAndRecoversNothingOnItsFirstDay()
    {
        var state = Fresh();
        state.Clear();
        state.Advance(Table, 90, DaysPerSeason, Recovery); // Winter: firewood's target is 1.5 and forage's 0.85
        Assert.That(state.LastDay, Is.EqualTo(90));
        foreach (string pool in Table.Pools)
            Assert.That(state.Get(pool), Is.EqualTo(10000), pool);

        state.Advance(Table, 91, DaysPerSeason, Recovery);
        Assert.That(state.Get("firewood"), Is.EqualTo(11250), "from the next day it recovers toward the season");
    }

    [Test]
    public void TheSaveRoundTripsThroughJson()
    {
        MarketState state = Depressed();
        state.Advance(Table, 31, DaysPerSeason, Recovery);
        MarketSaveData data = Snapshot(state);
        MarketSaveData back = JsonUtility.FromJson<MarketSaveData>(JsonUtility.ToJson(data));

        var loaded = Fresh();
        loaded.Restore(Table, back);
        AssertSame(data, Snapshot(loaded));
        Assert.That(back.version, Is.EqualTo(MarketSaveData.CurrentVersion));
        Assert.That(back.pools.Length, Is.EqualTo(Table.Pools.Count));
    }

    [Test]
    public void EmptyOrVersionZeroDataGivesNormalPricesAndNoDay()
    {
        foreach (string json in new[] { "{}", "{\"lastDay\":50}", "{\"version\":0,\"lastDay\":50,\"pools\":[{\"pool\":\"firewood\",\"bp\":3000}]}" })
        {
            var state = Depressed();
            state.Restore(Table, JsonUtility.FromJson<MarketSaveData>(json));
            Assert.That(state.LastDay, Is.EqualTo(-1), json);
            Assert.That(state.Get("firewood"), Is.EqualTo(10000), json);
            Assert.That(state.Get("lantern"), Is.EqualTo(10000), json);
        }
    }

    [Test]
    public void UnknownPoolsAreIgnoredAndMissingPoolsDefaultToNormal()
    {
        var data = new MarketSaveData
        {
            version = MarketSaveData.CurrentVersion,
            lastDay = 12,
            pools = new[]
            {
                new MarketPoolSave { pool = "firewood", bp = 6000 },
                new MarketPoolSave { pool = "unobtainium", bp = 1234 },
            },
        };

        var state = Depressed();
        state.Restore(Table, data);
        Assert.That(state.LastDay, Is.EqualTo(12));
        Assert.That(state.Get("firewood"), Is.EqualTo(6000));
        Assert.That(state.Get("unobtainium"), Is.EqualTo(10000), "not a pool of this table");
        Assert.That(state.Get("lantern"), Is.EqualTo(10000), "a pool the save lacks");
        Assert.That(Snapshot(state).pools.Any(p => p.pool == "unobtainium"), Is.False);
    }

    [Test]
    public void OutOfRangeSavedValuesAreKeptInRange()
    {
        var data = new MarketSaveData
        {
            version = MarketSaveData.CurrentVersion,
            lastDay = 5,
            pools = new[]
            {
                new MarketPoolSave { pool = "firewood", bp = -50 },
                new MarketPoolSave { pool = "lantern", bp = 50 },
                new MarketPoolSave { pool = "stone", bp = 99999 },
            },
        };

        var state = Fresh();
        state.Restore(Table, data);
        Assert.That(state.Get("firewood"), Is.EqualTo(2500));
        Assert.That(state.Get("lantern"), Is.EqualTo(1000), "crafted goods have the lower floor");
        Assert.That(state.Get("stone"), Is.EqualTo(MarketRules.MaxBp));
    }

    // --- Buying and selling back ---

    [Test]
    public void SurvivalGearIsTenPercentDearerInWinterAndFivePercentCheaperInSummer()
    {
        Assert.That(MarketRules.BuyPrice(2500, true, 3, 10, -5), Is.EqualTo(2750L), "tent, Winter");
        Assert.That(MarketRules.BuyPrice(2500, true, 1, 10, -5), Is.EqualTo(2375L), "tent, Summer");
        Assert.That(MarketRules.BuyPrice(1500, true, 3, 10, -5), Is.EqualTo(1650L), "sleeping bag");
        Assert.That(MarketRules.BuyPrice(1500, true, 1, 10, -5), Is.EqualTo(1425L));
        Assert.That(MarketRules.BuyPrice(800, true, 3, 10, -5), Is.EqualTo(880L), "tarp");
        Assert.That(MarketRules.BuyPrice(300, true, 1, 10, -5), Is.EqualTo(285L), "lamp oil");
        Assert.That(MarketRules.BuyPrice(2500, true, 0, 10, -5), Is.EqualTo(2500L), "Spring and Fall are unchanged");
        Assert.That(MarketRules.BuyPrice(2500, true, 2, 10, -5), Is.EqualTo(2500L));
    }

    [Test]
    public void OnlyTheFourSurvivalGearItemsDriftAndEveryOtherBuyPriceIsFlat()
    {
        var gear = Table.Stock.Where(r => r.seasonalBuy).Select(r => r.itemId).OrderBy(x => x).ToArray();
        Assert.That(gear, Is.EqualTo(new[] { "lamp_oil", "sleeping_bag", "tarp", "tent" }));
        foreach (MarketRow row in Table.Stock.Where(r => !r.seasonalBuy))
            for (int season = 0; season < 4; season++)
                Assert.That(MarketRules.BuyPrice(row.baseCents, row.seasonalBuy, season, 10, -5), Is.EqualTo(row.baseCents), row.itemId);
    }

    [Test]
    public void ASellBackAtNormalPriceIsTheOldSellBackForEveryStockedItem()
    {
        foreach (MarketRow row in Table.Stock)
        {
            Assert.That(Table.SellBaseCents(row.itemId), Is.EqualTo(Money.SellBackCents(row.baseCents)), row.itemId);
            Assert.That(MarketRules.UnitPrice(Table.SellBaseCents(row.itemId), 10000), Is.EqualTo(Money.SellBackCents(row.baseCents)), row.itemId);
        }
    }

    [Test]
    public void TheSellBackShareIsAFieldNotAConstant()
    {
        Assert.That(Money.SellBackCents(Money.Dollars(25), 40), Is.EqualTo(Money.SellBackCents(Money.Dollars(25))));
        Assert.That(Money.SellBackCents(Money.Dollars(25), 50), Is.EqualTo(Money.Dollars(12)));
        var half = new MarketTable(MarketCatalog.Rows, MarketCatalog.Groups, 50);
        Assert.That(half.SellBaseCents("tent"), Is.EqualTo(Money.Dollars(12)));
    }

    [Test]
    public void NothingCanBeBoughtAndSoldBackAtAProfitInAnySeason()
    {
        foreach (MarketRow row in Table.Stock)
        {
            long back = MarketRules.UnitPrice(Table.SellBaseCents(row.itemId), 10000); // the best a sell-back can pay: the multiplier never rises above 1.0
            for (int season = 0; season < 4; season++)
            {
                long buy = MarketRules.BuyPrice(row.baseCents, row.seasonalBuy, season, 10, -5);
                Assert.That(back, Is.LessThan(buy), $"{row.itemId} in season {season}");
            }
        }
    }

    [Test]
    public void ASellBackCanNeverRecoverAboveNormal()
    {
        MarketState state = Fresh();
        state.Advance(Table, 0, DaysPerSeason, Recovery);
        state.Sell(Table, "tent", 5);
        for (int day = 1; day <= 200; day++)
        {
            state.Advance(Table, day, DaysPerSeason, Recovery);
            Assert.That(state.Get("tent"), Is.LessThanOrEqualTo(10000), "day " + day);
        }

        Assert.That(state.Get("tent"), Is.EqualTo(10000));
    }

    // --- Trend and the market note ---

    [TestCase(8000, 10000, MarketTrend.Up)]
    [TestCase(9699, 10000, MarketTrend.Up)]
    [TestCase(9700, 10000, MarketTrend.Flat)]
    [TestCase(10300, 10000, MarketTrend.Flat)]
    [TestCase(10301, 10000, MarketTrend.Down)]
    [TestCase(15000, 10000, MarketTrend.Down)]
    [TestCase(10000, 15000, MarketTrend.Up)]
    public void TheTrendComparesThePriceToItsSeasonalTargetWithAThreePercentBand(int multiplier, int target, MarketTrend expected)
    {
        Assert.That(MarketRules.Trend(multiplier, target, 3), Is.EqualTo(expected));
    }

    [Test]
    public void PercentOfNormalRoundsToAWholePercent()
    {
        Assert.That(MarketRules.PercentOfNormal(6200), Is.EqualTo(62));
        Assert.That(MarketRules.PercentOfNormal(6249), Is.EqualTo(62));
        Assert.That(MarketRules.PercentOfNormal(6250), Is.EqualTo(63));
        Assert.That(MarketRules.PercentOfNormal(10000), Is.EqualTo(100));
    }

    [Test]
    public void TheMarketNoteListsOnlyGroupsTenPercentOrMoreFromNormalAndSkipsForage()
    {
        Assert.That(MarketRules.SeasonNote(Table, 0), Is.EqualTo(""), "Spring: nothing stands out, and forage is 0.85 all year");
        Assert.That(MarketRules.SeasonNote(Table, 2), Is.EqualTo(""));

        string summer = MarketRules.SeasonNote(Table, 1);
        Assert.That(summer, Does.Contain("Firewood, logs and stone down 20%"));
        Assert.That(summer, Does.Contain("Fish down 10%"));
        Assert.That(summer, Does.Not.Contain("Forage"));

        string winter = MarketRules.SeasonNote(Table, 3);
        Assert.That(winter, Does.Contain("Firewood, logs and stone up 50%"));
        Assert.That(winter, Does.Contain("Game meat up 30%"));
        Assert.That(winter, Does.Contain("Hides and furs up 30%"));
        Assert.That(winter, Does.Not.Contain("Fish"));
        Assert.That(winter, Does.Not.Contain("Forage"));
    }

    // --- The catalog ---

    [Test]
    public void TheDefaultCatalogHasNoProblems()
    {
        Assert.That(Table.Problems, Is.Empty);
        Assert.That(Table.Rows.Count, Is.EqualTo(73), "the 71 of the old tables, plus Ash and the Kindling Bundle");
        Assert.That(Table.Stock.Count, Is.EqualTo(19));
    }

    [Test]
    public void EverySellableItemHasAGroupAndAPool()
    {
        foreach (MarketRow row in Table.Rows)
        {
            Assert.That(Table.TryGetRule(row.group, out _), Is.True, row.itemId + " group");
            Assert.That(string.IsNullOrEmpty(row.pool), Is.False, row.itemId + " pool");
            if (Table.SellBaseCents(row.itemId) > 0)
                Assert.That(Table.HasPool(row.pool), Is.True, row.itemId + " has a sell row, so its pool is tracked");
        }
    }

    [Test]
    public void ArrowsRifleRoundsAndCordageNeverSellBack()
    {
        foreach (string id in new[] { "arrows", "rifle_rounds", "cordage" })
        {
            Assert.That(Table.BuyBaseCents(id), Is.GreaterThan(0L), id + " is stocked");
            Assert.That(Table.SellBaseCents(id), Is.EqualTo(0L), id + " has no sell row");
            Assert.That(Table.HasPool(id), Is.False, id);
        }

        Assert.That(Table.Stock.Count(r => Table.SellBaseCents(r.itemId) > 0), Is.EqualTo(16));
    }

    [Test]
    public void CookedMeatAndFishShareTheirRawItemsPool()
    {
        var pairs = new Dictionary<string, string>
        {
            { "cooked_bluegill", "bluegill" }, { "cooked_crappie", "crappie" }, { "cooked_bass", "bass" }, { "cooked_catfish", "catfish" },
            { "cooked_venison", "venison" }, { "cooked_turkey", "turkey_meat" }, { "cooked_waterfowl", "waterfowl_meat" },
            { "cooked_small_game", "small_game_meat" }, { "cooked_chicken", "chicken_meat" },
        };
        foreach (KeyValuePair<string, string> pair in pairs)
        {
            Assert.That(Table.PoolOf(pair.Key), Is.EqualTo(pair.Value), pair.Key);
            Assert.That(Table.PoolOf(pair.Value), Is.EqualTo(pair.Value), pair.Value);
            Assert.That(Table.TryGetRow(pair.Key, out MarketRow cooked) && Table.TryGetRow(pair.Value, out MarketRow raw) && cooked.group == raw.group, Is.True, pair.Key + " shares the group too");
        }
    }

    [Test]
    public void EveryOtherItemIsItsOwnPool()
    {
        var shared = new HashSet<string>
        {
            "cooked_bluegill", "cooked_crappie", "cooked_bass", "cooked_catfish", "cooked_venison", "cooked_turkey",
            "cooked_waterfowl", "cooked_small_game", "cooked_chicken",
        };
        foreach (MarketRow row in Table.Rows.Where(r => !shared.Contains(r.itemId)))
            Assert.That(row.pool, Is.EqualTo(row.itemId), row.itemId);
    }

    [Test]
    public void TheGroupsHoldTheRightItems()
    {
        Dictionary<MarketGroup, int> counts = Table.Rows.GroupBy(r => r.group).ToDictionary(g => g.Key, g => g.Count());
        Assert.That(counts[MarketGroup.WoodStone], Is.EqualTo(3));
        Assert.That(counts[MarketGroup.Fish], Is.EqualTo(8));
        Assert.That(counts[MarketGroup.GameMeat], Is.EqualTo(8));
        Assert.That(counts[MarketGroup.Hides], Is.EqualTo(5));
        Assert.That(counts[MarketGroup.Forage], Is.EqualTo(12));
        Assert.That(counts[MarketGroup.Crafted], Is.EqualTo(13), "12 and the Kindling Bundle");
        Assert.That(counts[MarketGroup.Ash], Is.EqualTo(1));
        Assert.That(counts[MarketGroup.Farm], Is.EqualTo(4));
        Assert.That(counts[MarketGroup.Stocked], Is.EqualTo(19));

        MarketGroup GroupOf(string id) { Table.TryGetRow(id, out MarketRow r); return r.group; }
        Assert.That(GroupOf("chicken_eggs"), Is.EqualTo(MarketGroup.Farm));
        Assert.That(GroupOf("goat_milk"), Is.EqualTo(MarketGroup.Farm));
        Assert.That(GroupOf("chicken_meat"), Is.EqualTo(MarketGroup.Farm));
        Assert.That(GroupOf("cooked_chicken"), Is.EqualTo(MarketGroup.Farm));
        Assert.That(GroupOf("lantern"), Is.EqualTo(MarketGroup.Crafted));
        Assert.That(GroupOf("sinew"), Is.EqualTo(MarketGroup.Hides));
    }

    // --- Base prices are today's prices ---

    [Test]
    public void BasePricesMatchTheFingerprintOfTheOldTradingPostTables()
    {
        // 19 stocked items totalling $255.00 and 52 bought items totalling $110.00, taken from the b03ebc3 TradingPost.cs; Ash ($2.00) and the
        // Kindling Bundle ($1.00) came later, so the bought items are now 54 totalling $113.00.
        Assert.That(Table.Stock.Sum(r => r.baseCents), Is.EqualTo(25500L));
        Assert.That(Table.Rows.Where(r => r.kind == MarketKind.Bought && r.itemId != "ash" && r.itemId != "kindling_bundle").Count(), Is.EqualTo(52));
        Assert.That(Table.Rows.Where(r => r.kind == MarketKind.Bought && r.itemId != "ash" && r.itemId != "kindling_bundle").Sum(r => r.baseCents), Is.EqualTo(11000L));
        Assert.That(Table.Rows.Where(r => r.kind == MarketKind.Bought).Sum(r => r.baseCents), Is.EqualTo(11300L));
        Assert.That(Table.BuyBaseCents("tent"), Is.EqualTo(2500L));
        Assert.That(Table.BuyBaseCents("bolt_action_rifle"), Is.EqualTo(6000L));
        Assert.That(Table.SellBaseCents("deer_hide"), Is.EqualTo(800L));
        Assert.That(Table.SellBaseCents("lantern"), Is.EqualTo(400L));
        Assert.That(Table.SellBaseCents("firewood"), Is.EqualTo(100L));
        Assert.That(Table.SellBaseCents("logs"), Is.EqualTo(300L));
    }

    // --- The MarketData asset, read as text (the test assembly cannot load main-assembly types) ---

    static string AssetPath => Path.Combine(Application.dataPath, "Resources", "Market", "MarketData.asset");

    [Test]
    public void TheMarketDataAssetHoldsExactlyTheDefaultRowsAndGroups()
    {
        Assume.That(File.Exists(AssetPath), "MarketData.asset has not been seeded");
        string text = File.ReadAllText(AssetPath).Replace("\r\n", "\n");

        string rowsText = text.Substring(text.IndexOf("  rows:\n", System.StringComparison.Ordinal), text.IndexOf("  groups:\n", System.StringComparison.Ordinal) - text.IndexOf("  rows:\n", System.StringComparison.Ordinal));
        MatchCollection rows = Regex.Matches(rowsText,
            @"- itemId: (\S+)\n    group: (\d+)\n    pool: (\S+)\n    baseCents: (\d+)\n    kind: (\d+)\n    seasonalBuy: (\d+)");
        Assert.That(rows.Count, Is.EqualTo(MarketCatalog.Rows.Length), "row count");
        for (int i = 0; i < rows.Count; i++)
        {
            MarketRow want = MarketCatalog.Rows[i];
            GroupCollection got = rows[i].Groups;
            Assert.That(got[1].Value, Is.EqualTo(want.itemId), "row " + i);
            Assert.That(int.Parse(got[2].Value), Is.EqualTo((int)want.group), want.itemId + " group");
            Assert.That(got[3].Value, Is.EqualTo(want.pool), want.itemId + " pool");
            Assert.That(long.Parse(got[4].Value), Is.EqualTo(want.baseCents), want.itemId + " base price");
            Assert.That(int.Parse(got[5].Value), Is.EqualTo((int)want.kind), want.itemId + " kind");
            Assert.That(int.Parse(got[6].Value), Is.EqualTo(want.seasonalBuy ? 1 : 0), want.itemId + " seasonalBuy");
        }

        string groupsText = text.Substring(text.IndexOf("  groups:\n", System.StringComparison.Ordinal));
        MatchCollection groups = Regex.Matches(groupsText,
            @"- group: (\d+)\n    dropBp: (\d+)\n    floorBp: (\d+)\n    spring: (\d+)\n    summer: (\d+)\n    fall: (\d+)\n    winter: (\d+)");
        Assert.That(groups.Count, Is.EqualTo(MarketCatalog.Groups.Length), "group count");
        for (int i = 0; i < groups.Count; i++)
        {
            MarketGroupRule want = MarketCatalog.Groups[i];
            int[] got = groups[i].Groups.Cast<Group>().Skip(1).Select(g => int.Parse(g.Value)).ToArray();
            Assert.That(got, Is.EqualTo(new[] { (int)want.group, want.dropBp, want.floorBp, want.spring, want.summer, want.fall, want.winter }), "group " + want.group);
        }
    }
}
