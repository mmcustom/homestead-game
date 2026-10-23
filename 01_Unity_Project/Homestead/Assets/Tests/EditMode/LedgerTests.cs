using System.Linq;
using NUnit.Framework;
using UnityEngine;

// EditMode tests for the ledger and the seasonal statement (Money_System.md, Phase B): season index and labels, the fractional first
// season, recording and closing, reconciliation and the Other line, the This year and Since the start rows, per game day averages,
// change from the season before, the red rule and the save struct. All pure: no manager, no scene.
public class LedgerTests
{
    const int Dps = 28;
    const long GameStartMinute = 6 * 60;          // a new game starts at 6:00 on day 0
    static long SeasonEnd(int index) => LedgerState.SeasonEndMinute(index, Dps);

    static LedgerState Started(long openingCash = 100000, int season = 0, long now = GameStartMinute)
    {
        var ledger = new LedgerState();
        ledger.Begin(openingCash, season, now, Dps);
        return ledger;
    }

    // --- Season index and labels ---

    [TestCase(0, 0)]
    [TestCase(27, 0)]
    [TestCase(28, 1)]
    [TestCase(55, 1)]
    [TestCase(56, 2)]
    [TestCase(111, 3)]
    [TestCase(112, 4)]
    [TestCase(223, 7)]
    [TestCase(224, 8)]
    public void TheSeasonOfADayCountsFromZero(int day, int season)
    {
        Assert.That(LedgerState.SeasonOf(day, Dps), Is.EqualTo(season));
    }

    [Test]
    public void ABadDayOrSeasonLengthStillGivesASeason()
    {
        Assert.That(LedgerState.SeasonOf(-4, Dps), Is.EqualTo(0));
        Assert.That(LedgerState.SeasonOf(5, 0), Is.EqualTo(5), "a season length under 1 counts as 1");
    }

    [TestCase(0, "Spring, Year 1")]
    [TestCase(1, "Summer, Year 1")]
    [TestCase(2, "Fall, Year 1")]
    [TestCase(3, "Winter, Year 1")]
    [TestCase(4, "Spring, Year 2")]
    [TestCase(7, "Winter, Year 2")]
    [TestCase(8, "Spring, Year 3")]
    [TestCase(179, "Winter, Year 45")]
    public void SeasonsAreLabelledByNameAndYear(int index, string label)
    {
        Assert.That(LedgerState.SeasonLabel(index), Is.EqualTo(label));
    }

    [Test]
    public void FourSeasonsMakeAYear()
    {
        Assert.That(new[] { 0, 1, 2, 3 }.Select(LedgerState.YearOf), Is.All.EqualTo(1));
        Assert.That(new[] { 4, 5, 6, 7 }.Select(LedgerState.YearOf), Is.All.EqualTo(2));
    }

    [Test]
    public void SeasonBoundariesAreInGameMinutes()
    {
        Assert.That(LedgerState.SeasonStartMinute(0, Dps), Is.EqualTo(0L));
        Assert.That(LedgerState.SeasonEndMinute(0, Dps), Is.EqualTo(28L * 1440));
        Assert.That(LedgerState.SeasonStartMinute(1, Dps), Is.EqualTo(LedgerState.SeasonEndMinute(0, Dps)));
        Assert.That(LedgerState.SeasonEndMinute(3, Dps), Is.EqualTo(112L * 1440));
    }

    // --- The fractional first season ---

    [Test]
    public void AGameStartedAtSixAmCoversTwentySevenAndThreeQuarterDaysOfItsFirstSeason()
    {
        LedgerState ledger = Started();
        ledger.CloseThrough(1, 5000, 0, 0);
        StatementRow first = ledger.Rows(5000, 0, 0, SeasonEnd(0))[1];
        Assert.That(first.minutes, Is.EqualTo(27L * 1440 + 18 * 60), "27.75 days");
        Assert.That(LedgerState.DaysText(first.minutes), Is.EqualTo("27.75 days"));
    }

    [Test]
    public void AnOldSaveLoadedPartWayThroughASeasonCoversOnlyTheRestOfIt()
    {
        long midSeason = SeasonEnd(0) + 10L * 1440 + 12 * 60; // day 38 at noon: season 1
        var ledger = new LedgerState();
        ledger.Begin(7000, 1, midSeason, Dps);
        ledger.CloseThrough(2, 7000, 0, 0);
        StatementRow row = ledger.Rows(7000, 0, 0, SeasonEnd(1))[1];
        Assert.That(LedgerState.DaysText(row.minutes), Is.EqualTo("17.50 days"), "no invented past: it covers from the load");
        Assert.That(ledger.Closed.Count, Is.EqualTo(1));
        Assert.That(ledger.Closed[0].index, Is.EqualTo(1), "no earlier seasons exist");
    }

    [TestCase(0, "0.00 days")]
    [TestCase(1440, "1.00 days")]
    [TestCase(39960, "27.75 days")]
    [TestCase(40320, "28.00 days")]
    [TestCase(-5, "0.00 days")]
    public void DaysAreShownWithTwoDecimals(long minutes, string expected)
    {
        Assert.That(LedgerState.DaysText(minutes), Is.EqualTo(expected));
    }

    // --- Recording ---

    [Test]
    public void MoneyIsTotalledPerCategoryAndSplitIntoIncomeAndExpenses()
    {
        LedgerState ledger = Started();
        ledger.Record(LedgerCategory.SalesFish, 1200);
        ledger.Record(LedgerCategory.SalesFish, 300);
        ledger.Record(LedgerCategory.SalesWoodStone, 450);
        ledger.Record(LedgerCategory.BuyTools, 2000);
        ledger.Record(LedgerCategory.LoanInterest, 320);

        Assert.That(ledger.Open.cents[(int)LedgerCategory.SalesFish], Is.EqualTo(1500L));
        Assert.That(ledger.Open.Income, Is.EqualTo(1950L));
        Assert.That(ledger.Open.Expenses, Is.EqualTo(2320L));
    }

    [Test]
    public void NothingIsRecordedForZeroNegativeOrBeforeTheLedgerStarts()
    {
        var blank = new LedgerState();
        blank.Record(LedgerCategory.SalesFish, 500);
        Assert.That(blank.Started, Is.False);

        LedgerState ledger = Started();
        ledger.Record(LedgerCategory.SalesFish, 0);
        ledger.Record(LedgerCategory.SalesFish, -300);
        ledger.Record((LedgerCategory)99, 500);
        Assert.That(ledger.Open.Income, Is.EqualTo(0L));
        Assert.That(ledger.Open.Expenses, Is.EqualTo(0L));
    }

    [Test]
    public void RecordedMoneySaturatesInsteadOfWrapping()
    {
        LedgerState ledger = Started();
        ledger.Record(LedgerCategory.SalesFish, long.MaxValue - 5);
        ledger.Record(LedgerCategory.SalesFish, 100);
        Assert.That(ledger.Open.cents[(int)LedgerCategory.SalesFish], Is.EqualTo(long.MaxValue));
    }

    // --- Closing ---

    [Test]
    public void ASeasonClosesWithTheCashLoanAndAmountOwedAtThatMoment()
    {
        LedgerState ledger = Started(100000);
        ledger.Record(LedgerCategory.SalesFish, 4000);
        int closed = ledger.CloseThrough(1, 104000, 4800000, 7500);

        Assert.That(closed, Is.EqualTo(1));
        LedgerSeason s = ledger.Closed[0];
        Assert.That(s.index, Is.EqualTo(0));
        Assert.That(s.cashStart, Is.EqualTo(100000L));
        Assert.That(s.cashEnd, Is.EqualTo(104000L));
        Assert.That(s.loanEnd, Is.EqualTo(4800000L));
        Assert.That(s.owedEnd, Is.EqualTo(7500L));
        Assert.That(s.endMinute, Is.EqualTo(SeasonEnd(0)));
        Assert.That(ledger.OpenIndex, Is.EqualTo(1));
        Assert.That(ledger.Open.cashStart, Is.EqualTo(104000L), "the next season opens with the cash the last one closed with");
        Assert.That(ledger.Open.Income, Is.EqualTo(0L), "a fresh season");
        Assert.That(ledger.Unread, Is.True, "a statement is ready");
    }

    [Test]
    public void CallingCloseAgainForTheSameSeasonDoesNothing()
    {
        LedgerState ledger = Started();
        ledger.CloseThrough(1, 100, 0, 0);
        ledger.Unread = false;
        Assert.That(ledger.CloseThrough(1, 999, 0, 0), Is.EqualTo(0));
        Assert.That(ledger.Closed.Count, Is.EqualTo(1));
        Assert.That(ledger.Unread, Is.False, "nothing new to read");
    }

    [Test]
    public void ASkipOverSeveralSeasonsClosesEveryOneWithTheSameFigures()
    {
        LedgerState ledger = Started(50000);
        ledger.Record(LedgerCategory.SalesForage, 1000);
        int closed = ledger.CloseThrough(5, 51000, 3000000, 12000);

        Assert.That(closed, Is.EqualTo(5));
        Assert.That(ledger.Closed.Select(s => s.index), Is.EqualTo(new[] { 0, 1, 2, 3, 4 }));
        Assert.That(ledger.OpenIndex, Is.EqualTo(5));
        Assert.That(ledger.Closed[0].Income, Is.EqualTo(1000L), "what happened before the skip stays in the first season");
        for (int i = 1; i < 5; i++)
        {
            Assert.That(ledger.Closed[i].Income, Is.EqualTo(0L));
            Assert.That(ledger.Closed[i].cashStart, Is.EqualTo(51000L), "season " + i);
            Assert.That(ledger.Closed[i].cashEnd, Is.EqualTo(51000L), "season " + i);
            Assert.That(ledger.Closed[i].loanEnd, Is.EqualTo(3000000L), "season " + i);
            Assert.That(ledger.Closed[i].owedEnd, Is.EqualTo(12000L), "season " + i);
        }
        Assert.That(ledger.Closed.Select(s => s.endMinute - s.startMinute).Skip(1), Is.All.EqualTo(40320L), "every skipped season is a whole 28 days");
    }

    [Test]
    public void TheSeasonsAreContiguousWithNoGaps()
    {
        LedgerState ledger = Started();
        ledger.CloseThrough(4, 100, 0, 0);
        for (int i = 1; i < ledger.Closed.Count; i++)
            Assert.That(ledger.Closed[i].startMinute, Is.EqualTo(ledger.Closed[i - 1].endMinute));
        Assert.That(ledger.Open.startMinute, Is.EqualTo(ledger.Closed.Last().endMinute));
    }

    [Test]
    public void ABlankLedgerClosesNothing()
    {
        Assert.That(new LedgerState().CloseThrough(5, 100, 0, 0), Is.EqualTo(0));
    }

    // --- Reconciliation and the Other line ---

    [Test]
    public void ACleanSeasonReconcilesToTheRealCash()
    {
        LedgerState ledger = Started(100000);
        ledger.Record(LedgerCategory.SalesFish, 4000);
        ledger.Record(LedgerCategory.BuyTools, 1500);
        ledger.CloseThrough(1, 102500, 0, 0); // 100000 + 4000 - 1500

        StatementRow row = ledger.Rows(102500, 0, 0, SeasonEnd(0))[1];
        Assert.That(row.cashStart + row.income - row.expenses, Is.EqualTo(row.cashEnd));
        Assert.That(row.Other, Is.EqualTo(0L));
    }

    [Test]
    public void NewGameSavingsAndTheDownPaymentReconcileSeasonOne()
    {
        // Savings $24,000: $12,000 down payment, $12,000 cash reserve. Cash starts at the reserve; the ledger starts at the savings.
        LedgerState ledger = Started(2400000);
        ledger.Record(LedgerCategory.DownPayment, 1200000);
        ledger.Record(LedgerCategory.SalesFish, 5000);
        ledger.CloseThrough(1, 1205000, 4800000, 0);

        StatementRow row = ledger.Rows(1205000, 4800000, 0, SeasonEnd(0))[1];
        Assert.That(row.Other, Is.EqualTo(0L), "$24,000 - $12,000 + $50 = $12,050");
        Assert.That(row.cashStart, Is.EqualTo(2400000L));
        Assert.That(row.expenses, Is.EqualTo(1200000L));
    }

    [Test]
    public void CashThatMovedOutsideTheLedgerShowsAsOther()
    {
        LedgerState ledger = Started(100000);
        ledger.Record(LedgerCategory.SalesFish, 4000);
        ledger.CloseThrough(1, 109000, 0, 0); // $50 of debug money on top of the $40 sale

        StatementRow row = ledger.Rows(109000, 0, 0, SeasonEnd(0))[1];
        Assert.That(row.Other, Is.EqualTo(5000L));
        Assert.That(row.cashStart + row.income - row.expenses + row.Other, Is.EqualTo(row.cashEnd), "with Other, every season reconciles");
    }

    [Test]
    public void CashThatFellOutsideTheLedgerIsANegativeOther()
    {
        LedgerState ledger = Started(100000);
        ledger.CloseThrough(1, 98000, 0, 0);
        Assert.That(ledger.Rows(98000, 0, 0, SeasonEnd(0))[1].Other, Is.EqualTo(-2000L));
    }

    [Test]
    public void TheOpenSeasonAlsoReconcilesAgainstTheCashNow()
    {
        LedgerState ledger = Started(100000);
        ledger.Record(LedgerCategory.SalesCrafted, 800);
        StatementRow open = ledger.Rows(100800, 0, 0, GameStartMinute + 3 * 1440)[0];
        Assert.That(open.inProgress, Is.True);
        Assert.That(open.Other, Is.EqualTo(0L));
    }

    // --- Rows, This year and Since the start ---

    static LedgerState ThreeYears()
    {
        // Seasons 0..5 closed, season 6 open. Each season earns $10 x (index + 1) from fish and spends $4 on tools; cash follows.
        LedgerState ledger = Started(10000);
        long cash = 10000;
        for (int s = 0; s < 6; s++)
        {
            long earned = 1000L * (s + 1);
            ledger.Record(LedgerCategory.SalesFish, earned);
            ledger.Record(LedgerCategory.BuyTools, 400);
            cash += earned - 400;
            ledger.CloseThrough(s + 1, cash, 4800000 - 10000L * s, 0);
        }
        ledger.Record(LedgerCategory.SalesFish, 700);
        return ledger;
    }

    [Test]
    public void RowsRunNewestFirstWithTheOpenSeasonOnTop()
    {
        LedgerState ledger = ThreeYears();
        System.Collections.Generic.List<StatementRow> rows = ledger.Rows(30000, 4740000, 0, SeasonEnd(5) + 5 * 1440);
        Assert.That(rows.Select(r => r.index), Is.EqualTo(new[] { 6, 5, 4, 3, 2, 1, 0 }));
        Assert.That(rows[0].inProgress, Is.True);
        Assert.That(rows.Skip(1).Select(r => r.inProgress), Is.All.False);
        Assert.That(rows[0].income, Is.EqualTo(700L));
        Assert.That(rows[1].income, Is.EqualTo(6000L));
        Assert.That(rows[6].income, Is.EqualTo(1000L));
        Assert.That(rows[1].Net, Is.EqualTo(5600L));
    }

    [Test]
    public void AClosedRowKeepsItsOwnEndFigures()
    {
        LedgerState ledger = ThreeYears();
        StatementRow row = ledger.Rows(0, 0, 0, SeasonEnd(5))[3]; // season 3
        Assert.That(row.index, Is.EqualTo(3));
        Assert.That(row.loanEnd, Is.EqualTo(4800000L - 10000L * 3));
    }

    [Test]
    public void ThisYearAddsTheSeasonsOfTheCurrentYearOnly()
    {
        LedgerState ledger = ThreeYears();
        var rows = ledger.Rows(30000, 4740000, 0, SeasonEnd(5) + 5 * 1440);
        StatementRow year2 = LedgerState.ThisYear(rows, 2); // seasons 4, 5 and the open 6
        Assert.That(year2.index, Is.EqualTo(-1));
        Assert.That(year2.income, Is.EqualTo(5000L + 6000L + 700L));
        Assert.That(year2.expenses, Is.EqualTo(800L), "seasons 4 and 5 bought tools; the open season has not yet");
        Assert.That(year2.cashStart, Is.EqualTo(rows.First(r => r.index == 4).cashStart), "starts where the year's first season started");
        Assert.That(year2.cashEnd, Is.EqualTo(30000L), "ends with the cash now");
        Assert.That(year2.minutes, Is.EqualTo(rows.Where(r => r.index >= 4).Sum(r => r.minutes)));
    }

    [Test]
    public void SinceTheStartAddsEverySeason()
    {
        LedgerState ledger = ThreeYears();
        var rows = ledger.Rows(30000, 4740000, 0, SeasonEnd(5) + 5 * 1440);
        StatementRow all = LedgerState.SinceStart(rows);
        Assert.That(all.income, Is.EqualTo(1000L + 2000 + 3000 + 4000 + 5000 + 6000 + 700));
        Assert.That(all.expenses, Is.EqualTo(6 * 400L));
        Assert.That(all.cashStart, Is.EqualTo(10000L), "the opening cash");
        Assert.That(all.cents[(int)LedgerCategory.SalesFish], Is.EqualTo(all.income));
        Assert.That(all.cents[(int)LedgerCategory.BuyTools], Is.EqualTo(all.expenses));
    }

    [Test]
    public void TheTotalsRowReconcilesToo()
    {
        LedgerState ledger = ThreeYears();
        long cashNow = 10000 + (1000 + 2000 + 3000 + 4000 + 5000 + 6000) - 6 * 400 + 700;
        var rows = ledger.Rows(cashNow, 0, 0, SeasonEnd(5) + 5 * 1440);
        StatementRow all = LedgerState.SinceStart(rows);
        Assert.That(all.cashEnd, Is.EqualTo(cashNow));
        Assert.That(all.Other, Is.EqualTo(0L));
    }

    [Test]
    public void ATotalOverNoRowsIsAllZero()
    {
        StatementRow none = LedgerState.SinceStart(new System.Collections.Generic.List<StatementRow>());
        Assert.That(none.income, Is.EqualTo(0L));
        Assert.That(none.minutes, Is.EqualTo(0L));
    }

    [Test]
    public void ABlankLedgerHasNoRows()
    {
        Assert.That(new LedgerState().Rows(100, 0, 0, 0), Is.Empty);
    }

    // --- Per game day, change, red ---

    [Test]
    public void AveragesAreCentsPerGameDayRoundedHalfUp()
    {
        Assert.That(LedgerState.PerGameDay(10000, 28 * 1440), Is.EqualTo(357L), "357.14");
        Assert.That(LedgerState.PerGameDay(27750, 39960), Is.EqualTo(1000L), "27.75 days of $277.50: $10.00 a day");
        Assert.That(LedgerState.PerGameDay(1, 2 * 1440), Is.EqualTo(1L), "half a cent a day rounds up");
        Assert.That(LedgerState.PerGameDay(-1, 2 * 1440), Is.EqualTo(-1L), "and away from zero for a loss");
        Assert.That(LedgerState.PerGameDay(-10000, 28 * 1440), Is.EqualTo(-357L));
        Assert.That(LedgerState.PerGameDay(5000, 0), Is.EqualTo(0L), "nothing covered");
    }

    [Test]
    public void ChangeFromTheSeasonBeforeInCentsAndBasisPoints()
    {
        LedgerChange up = LedgerState.ChangeFrom(11500, 10000);
        Assert.That(up.cents, Is.EqualTo(1500L));
        Assert.That(up.hasPercent, Is.True);
        Assert.That(up.basisPoints, Is.EqualTo(1500));

        LedgerChange down = LedgerState.ChangeFrom(5000, 10000);
        Assert.That(down.cents, Is.EqualTo(-5000L));
        Assert.That(down.basisPoints, Is.EqualTo(-5000));

        Assert.That(LedgerState.ChangeFrom(10000, 10000).basisPoints, Is.EqualTo(0));
    }

    [Test]
    public void ChangeRoundsHalfAwayFromZeroAndHasNoPercentFromZero()
    {
        Assert.That(LedgerState.ChangeFrom(4, 3).basisPoints, Is.EqualTo(3333));
        Assert.That(LedgerState.ChangeFrom(5, 3).basisPoints, Is.EqualTo(6667), "6666.67");
        Assert.That(LedgerState.ChangeFrom(2, 3).basisPoints, Is.EqualTo(-3333));
        Assert.That(LedgerState.ChangeFrom(9, 8).basisPoints, Is.EqualTo(1250));

        LedgerChange fromNothing = LedgerState.ChangeFrom(500, 0);
        Assert.That(fromNothing.cents, Is.EqualTo(500L));
        Assert.That(fromNothing.hasPercent, Is.False);
    }

    [Test]
    public void ALossComparedWithALossHasASignedPercent()
    {
        LedgerChange change = LedgerState.ChangeFrom(-3000, -2000); // lost more than before
        Assert.That(change.cents, Is.EqualTo(-1000L));
        Assert.That(change.basisPoints, Is.EqualTo(-5000));
    }

    [TestCase(-1L, true)]
    [TestCase(-100000L, true)]
    [TestCase(0L, false)]
    [TestCase(1L, false)]
    [TestCase(500000L, false)]
    public void ANegativeFigureIsRedAndZeroIsBlack(long cents, bool red)
    {
        Assert.That(LedgerState.IsRed(cents), Is.EqualTo(red));
    }

    // --- Saving ---

    [Test]
    public void TheLedgerRoundTripsThroughJson()
    {
        LedgerState ledger = ThreeYears();
        ledger.Unread = true;
        LedgerSaveData data = ledger.Capture();
        LedgerSaveData back = JsonUtility.FromJson<LedgerSaveData>(JsonUtility.ToJson(data));

        var loaded = new LedgerState();
        loaded.Restore(back);
        Assert.That(loaded.Started, Is.True);
        Assert.That(loaded.Unread, Is.True, "the unread flag survives");
        Assert.That(loaded.DaysPerSeason, Is.EqualTo(Dps), "the season length is saved");
        Assert.That(loaded.OpenIndex, Is.EqualTo(6));
        Assert.That(loaded.Closed.Count, Is.EqualTo(6));
        Assert.That(loaded.Open.Income, Is.EqualTo(700L));
        for (int i = 0; i < 6; i++)
        {
            Assert.That(loaded.Closed[i].cents, Is.EqualTo(ledger.Closed[i].cents), "season " + i);
            Assert.That(loaded.Closed[i].cashEnd, Is.EqualTo(ledger.Closed[i].cashEnd));
            Assert.That(loaded.Closed[i].loanEnd, Is.EqualTo(ledger.Closed[i].loanEnd));
            Assert.That(loaded.Closed[i].startMinute, Is.EqualTo(ledger.Closed[i].startMinute));
        }
    }

    [Test]
    public void ALoadedLedgerKeepsRecordingAndClosing()
    {
        LedgerState ledger = Started(5000);
        ledger.Record(LedgerCategory.SalesFish, 300);
        var loaded = new LedgerState();
        loaded.Restore(JsonUtility.FromJson<LedgerSaveData>(JsonUtility.ToJson(ledger.Capture())));
        loaded.Record(LedgerCategory.SalesFish, 200);
        Assert.That(loaded.Open.cents[(int)LedgerCategory.SalesFish], Is.EqualTo(500L));
        Assert.That(loaded.CloseThrough(1, 5500, 0, 0), Is.EqualTo(1));
    }

    [Test]
    public void NoSectionOrAnEmptyOneLeavesABlankLedgerThatStartsLater()
    {
        foreach (string json in new[] { "{}", "{\"version\":0,\"started\":true}", "{\"version\":1,\"started\":false}" })
        {
            var ledger = ThreeYears();
            ledger.Restore(JsonUtility.FromJson<LedgerSaveData>(json));
            Assert.That(ledger.Started, Is.False, json);
            Assert.That(ledger.Closed, Is.Empty, json);
            Assert.That(ledger.Unread, Is.False, json);
            Assert.That(ledger.Rows(100, 0, 0, 0), Is.Empty, json);
        }
    }

    [Test]
    public void ABlankLedgerStartsFromTheCashAtTheLoad()
    {
        var ledger = new LedgerState();
        ledger.Restore(default(LedgerSaveData));
        ledger.Begin(123456, 9, 9L * 40320 + 600, Dps); // what LedgerManager does at the first ProcessNow
        Assert.That(ledger.Open.cashStart, Is.EqualTo(123456L));
        Assert.That(ledger.OpenIndex, Is.EqualTo(9));
        Assert.That(ledger.Closed, Is.Empty, "no invented past seasons");
    }

    [Test]
    public void SavedAmountArraysAreBroughtToTheCurrentCategoryCount()
    {
        var shortSeason = new LedgerSeason { index = 2, cents = new long[] { 10, 20, 30 } };
        var data = new LedgerSaveData
        {
            version = LedgerSaveData.CurrentVersion, daysPerSeason = Dps, started = true,
            open = new LedgerSeason { index = 3, cents = null },
            closed = new[] { shortSeason, null },
        };
        var ledger = new LedgerState();
        ledger.Restore(data);
        Assert.That(ledger.Open.cents.Length, Is.EqualTo(LedgerCategories.Count));
        Assert.That(ledger.Closed.Count, Is.EqualTo(1), "a null season is skipped");
        Assert.That(ledger.Closed[0].cents.Length, Is.EqualTo(LedgerCategories.Count));
        Assert.That(ledger.Closed[0].cents.Take(3), Is.EqualTo(new long[] { 10, 20, 30 }));
        Assert.That(ledger.Closed[0].cents.Skip(3), Is.All.EqualTo(0L));
    }

    [Test]
    public void ALedgerWithManySeasonsSavesInAReasonableSize()
    {
        LedgerState ledger = Started(100000);
        for (int s = 1; s <= 180; s++)
        {
            foreach (LedgerCategory c in System.Enum.GetValues(typeof(LedgerCategory)))
                ledger.Record(c, 123456789L);
            ledger.CloseThrough(s, 99999999L, 4800000L, 7500L);
        }

        int bytes = JsonUtility.ToJson(ledger.Capture()).Length;
        Assert.That(bytes, Is.LessThan(120000), "180 full seasons: " + bytes + " bytes of JSON");
    }

    // --- Categories ---

    [Test]
    public void CategoryIdsAreAppendOnly()
    {
        Assert.That(LedgerCategories.Count, Is.EqualTo(19));
        Assert.That((int)LedgerCategory.SalesWoodStone, Is.EqualTo(0));
        Assert.That((int)LedgerCategory.SalesStockedBack, Is.EqualTo(7));
        Assert.That((int)LedgerCategory.LoanInterest, Is.EqualTo(8));
        Assert.That((int)LedgerCategory.LoanPrincipal, Is.EqualTo(9));
        Assert.That((int)LedgerCategory.LoanExtra, Is.EqualTo(10));
        Assert.That((int)LedgerCategory.LoanFees, Is.EqualTo(11));
        Assert.That((int)LedgerCategory.DownPayment, Is.EqualTo(12));
        Assert.That((int)LedgerCategory.BuyTools, Is.EqualTo(13));
        Assert.That((int)LedgerCategory.BuyOtherGoods, Is.EqualTo(18));
        Assert.That(System.Enum.GetValues(typeof(LedgerCategory)).Length, Is.EqualTo(LedgerCategories.Count));
    }

    [Test]
    public void IncomeIsTheFirstEightCategoriesAndTheRestAreExpenses()
    {
        foreach (LedgerCategory c in System.Enum.GetValues(typeof(LedgerCategory)))
            Assert.That(LedgerCategories.IsIncome(c), Is.EqualTo((int)c <= 7), c.ToString());
    }

    [Test]
    public void EveryCategoryHasAName()
    {
        var names = System.Enum.GetValues(typeof(LedgerCategory)).Cast<LedgerCategory>().Select(LedgerCategories.Name).ToList();
        Assert.That(names, Is.All.Not.Empty);
        Assert.That(names.Distinct().Count(), Is.EqualTo(names.Count), "no two categories share a name");
    }

    [Test]
    public void ASaleGoesToTheIncomeCategoryOfItsMarketGroup()
    {
        Assert.That(LedgerCategories.ForSale(MarketGroup.WoodStone), Is.EqualTo(LedgerCategory.SalesWoodStone));
        Assert.That(LedgerCategories.ForSale(MarketGroup.Fish), Is.EqualTo(LedgerCategory.SalesFish));
        Assert.That(LedgerCategories.ForSale(MarketGroup.GameMeat), Is.EqualTo(LedgerCategory.SalesGameMeat));
        Assert.That(LedgerCategories.ForSale(MarketGroup.Hides), Is.EqualTo(LedgerCategory.SalesHides));
        Assert.That(LedgerCategories.ForSale(MarketGroup.Forage), Is.EqualTo(LedgerCategory.SalesForage));
        Assert.That(LedgerCategories.ForSale(MarketGroup.Crafted), Is.EqualTo(LedgerCategory.SalesCrafted));
        Assert.That(LedgerCategories.ForSale(MarketGroup.Farm), Is.EqualTo(LedgerCategory.SalesFarm));
        Assert.That(LedgerCategories.ForSale(MarketGroup.Stocked), Is.EqualTo(LedgerCategory.SalesStockedBack));
    }

    [Test]
    public void EveryMarketItemThatSellsHasAnIncomeCategory()
    {
        foreach (MarketRow row in MarketCatalog.Table.Rows)
            Assert.That(LedgerCategories.IsIncome(LedgerCategories.ForSale(row.group)), Is.True, row.itemId);
    }

    [Test]
    public void EveryStockedItemHasAListedPurchaseCategory()
    {
        foreach (MarketRow row in MarketCatalog.Table.Stock)
        {
            Assert.That(LedgerCategories.PurchaseIsListed(row.itemId), Is.True, row.itemId + " needs a purchase category");
            Assert.That(LedgerCategories.IsIncome(LedgerCategories.ForPurchase(row.itemId)), Is.False, row.itemId);
        }
    }

    [Test]
    public void ThereAreSixPurchaseCategoriesAndEachIsUsed()
    {
        var used = MarketCatalog.Table.Stock.Select(r => LedgerCategories.ForPurchase(r.itemId)).Distinct().OrderBy(c => (int)c).ToList();
        Assert.That(used, Is.EqualTo(new[]
        {
            LedgerCategory.BuyTools, LedgerCategory.BuyHuntingFishing, LedgerCategory.BuyShelterBedding,
            LedgerCategory.BuyCookingWaterFire, LedgerCategory.BuyBuildingMaterials, LedgerCategory.BuyOtherGoods,
        }));
    }

    [Test]
    public void SpecificPurchasesGoWhereTheyShould()
    {
        Assert.That(LedgerCategories.ForPurchase("axe"), Is.EqualTo(LedgerCategory.BuyTools));
        Assert.That(LedgerCategories.ForPurchase("arrows"), Is.EqualTo(LedgerCategory.BuyHuntingFishing));
        Assert.That(LedgerCategories.ForPurchase("tent"), Is.EqualTo(LedgerCategory.BuyShelterBedding));
        Assert.That(LedgerCategories.ForPurchase("lamp_oil"), Is.EqualTo(LedgerCategory.BuyCookingWaterFire));
        Assert.That(LedgerCategories.ForPurchase("lumber"), Is.EqualTo(LedgerCategory.BuyBuildingMaterials));
        Assert.That(LedgerCategories.ForPurchase("weather_station"), Is.EqualTo(LedgerCategory.BuyOtherGoods));
    }

    [Test]
    public void AnUnlistedOrMissingItemIsOtherGoods()
    {
        Assert.That(LedgerCategories.ForPurchase("a_future_item"), Is.EqualTo(LedgerCategory.BuyOtherGoods));
        Assert.That(LedgerCategories.ForPurchase(null), Is.EqualTo(LedgerCategory.BuyOtherGoods));
        Assert.That(LedgerCategories.PurchaseIsListed("a_future_item"), Is.False);
    }
}
