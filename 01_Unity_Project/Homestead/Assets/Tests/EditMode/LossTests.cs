using System.Linq;
using NUnit.Framework;
using UnityEngine;

// EditMode tests for Money Phase C's pure logic: the loss rule (open missed payments against a threshold), the line under the loss
// text, the run summary and the new-game setup record with its save struct. No manager, no scene.
public class LossTests
{
    static readonly LoanTerms Terms = LoanTerms.Default;                     // first payment day 112, every 28 days, 10 days of grace
    const long Rich = 1000000000L;

    static LoanState NewLoan() => LoanState.Create(5, Money.Dollars(60000), Money.Dollars(12000), Terms);

    // The first day the loan is lost on, with nothing ever paid.
    static int FirstLossDay(LoanState loan, int threshold)
    {
        for (int day = 0; day < 1000; day++)
        {
            loan.Advance(day);
            if (LossRules.IsLoss(loan.MissedCount, threshold))
                return day;
        }
        return -1;
    }

    // --- The rule ---

    [TestCase(0, 3, false)]
    [TestCase(1, 3, false)]
    [TestCase(2, 3, false)]
    [TestCase(3, 3, true)]
    [TestCase(4, 3, true)]
    [TestCase(1, 1, true)]
    [TestCase(0, 1, false)]
    [TestCase(5, 5, true)]
    [TestCase(4, 5, false)]
    public void TwoOpenMissedPaymentsAreNotALossAndThreeAre(int open, int threshold, bool lost)
    {
        Assert.That(LossRules.IsLoss(open, threshold), Is.EqualTo(lost));
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void AThresholdOfZeroOrLessTurnsTheLossOff(int threshold)
    {
        Assert.That(LossRules.IsLoss(0, threshold), Is.False);
        Assert.That(LossRules.IsLoss(50, threshold), Is.False);
    }

    [Test]
    public void TheDefaultThresholdIsThree()
    {
        Assert.That(LossRules.DefaultThreshold, Is.EqualTo(3));
    }

    // --- The rule against the real loan ---

    [Test]
    public void WithNothingPaidTheThirdMissLandsOnDay178()
    {
        LoanState loan = NewLoan();
        Assert.That(FirstLossDay(loan, 3), Is.EqualTo(178), "payments fall due on 112, 140 and 168; each is missed 10 days later");
    }

    [Test]
    public void TheMissedCountGoesOneTwoThreeOnDays122150And178()
    {
        LoanState loan = NewLoan();
        loan.Advance(121);
        Assert.That(loan.MissedCount, Is.EqualTo(0), "day 121: the first payment is late, not yet missed");
        loan.Advance(122);
        Assert.That(loan.MissedCount, Is.EqualTo(1));
        loan.Advance(149);
        Assert.That(loan.MissedCount, Is.EqualTo(1));
        loan.Advance(150);
        Assert.That(loan.MissedCount, Is.EqualTo(2));
        Assert.That(LossRules.IsLoss(loan.MissedCount, 3), Is.False, "two open is not a loss");
        loan.Advance(177);
        Assert.That(loan.MissedCount, Is.EqualTo(2));
        loan.Advance(178);
        Assert.That(loan.MissedCount, Is.EqualTo(3));
        Assert.That(LossRules.IsLoss(loan.MissedCount, 3), Is.True, "three open is");
    }

    [Test]
    public void CatchingUpOneMissedPaymentClearsItAndThereIsNoLoss()
    {
        LoanState loan = NewLoan();
        loan.Advance(150); // two missed
        Assert.That(loan.MissedCount, Is.EqualTo(2));

        loan.Pay(Rich, loan.open[0].Owed); // the oldest one, fee and all
        Assert.That(loan.MissedCount, Is.EqualTo(1), "paying it cleared it");

        loan.Advance(178); // the third falls late: two open, not three
        Assert.That(loan.MissedCount, Is.EqualTo(2));
        Assert.That(LossRules.IsLoss(loan.MissedCount, 3), Is.False);
    }

    [Test]
    public void PayingOnTheDayBeforeTheThirdMissStopsTheLoss()
    {
        LoanState loan = NewLoan();
        loan.Advance(177);
        Assert.That(loan.MissedCount, Is.EqualTo(2));
        loan.Pay(Rich, loan.OwedNow); // everything open
        loan.Advance(178);
        Assert.That(loan.MissedCount, Is.EqualTo(0));
        Assert.That(LossRules.IsLoss(loan.MissedCount, 3), Is.False);
    }

    [Test]
    public void TheCountIsOpenPaymentsNotPaymentsEverMissed()
    {
        LoanState loan = NewLoan();
        loan.Advance(150);
        loan.Pay(Rich, loan.OwedNow); // two missed, both paid
        Assert.That(loan.MissedCount, Is.EqualTo(0));

        loan.Advance(206); // payments due 168 and 196 are missed on 178 and 206: two open now, four missed in all
        Assert.That(loan.MissedCount, Is.EqualTo(2));
        Assert.That(LossRules.IsLoss(loan.MissedCount, 3), Is.False, "four ever missed, only two open");
    }

    [Test]
    public void AGameWhoseLoanIsPaidOffCanNeverBeLost()
    {
        LoanState loan = NewLoan();
        loan.Pay(Rich, loan.MaxPayableCents);
        Assert.That(loan.IsPaidOff, Is.True);
        loan.Advance(5000);
        Assert.That(loan.MissedCount, Is.EqualTo(0));
        Assert.That(LossRules.IsLoss(loan.MissedCount, 3), Is.False);
    }

    [Test]
    public void ALowerThresholdLosesSooner()
    {
        Assert.That(FirstLossDay(NewLoan(), 1), Is.EqualTo(122));
        Assert.That(FirstLossDay(NewLoan(), 2), Is.EqualTo(150));
        Assert.That(FirstLossDay(NewLoan(), 0), Is.EqualTo(-1), "off: never");
    }

    [Test]
    public void APioneerWithAFullyFinancedLoanLosesOnDay94()
    {
        // Pioneer: no savings, so no down payment. The Money System doc's example has the first payment on day 28.
        var early = new LoanTerms { rateBasisPoints = 800, payments = 180, intervalDays = 28, firstDueDay = 28, graceDays = 10, lateFeeCents = 7500 };
        LoanState loan = LoanState.Create(5, Money.Dollars(60000), 0, early);
        Assert.That(FirstLossDay(loan, 3), Is.EqualTo(94), "due 28, 56, 84; missed 38, 66, 94");
    }

    // --- The line under the text ---

    [TestCase(3, "Three payments on the land went unpaid.")]
    [TestCase(2, "Two payments on the land went unpaid.")]
    [TestCase(1, "One payment on the land went unpaid.")]
    [TestCase(4, "Four payments on the land went unpaid.")]
    [TestCase(10, "Ten payments on the land went unpaid.")]
    [TestCase(12, "12 payments on the land went unpaid.")]
    public void TheMissLineNamesTheThreshold(int threshold, string expected)
    {
        Assert.That(LossRules.MissLine(threshold), Is.EqualTo(expected));
    }

    // --- The run summary ---

    [Test]
    public void TheSummaryIsBuiltFromPlainNumbers()
    {
        RunSummary s = RunSummary.Build(178, 28, 32695, 1299345, 4775000, new[] { 150, 122, 178 - 10 }, 133350);
        Assert.That(s.daysSurvived, Is.EqualTo(178));
        Assert.That(s.seasonsCompleted, Is.EqualTo(6), "178 / 28 = 6 whole seasons");
        Assert.That(s.incomeCents, Is.EqualTo(32695L));
        Assert.That(s.expensesCents, Is.EqualTo(1299345L));
        Assert.That(s.balanceCents, Is.EqualTo(4775000L));
        Assert.That(s.cashCents, Is.EqualTo(133350L));
        Assert.That(s.missedDueDays, Is.EqualTo(new[] { 122, 150, 168 }), "oldest first");
    }

    [TestCase(0, 0)]
    [TestCase(27, 0)]
    [TestCase(28, 1)]
    [TestCase(111, 3)]
    [TestCase(112, 4)]
    [TestCase(-5, 0)]
    public void SeasonsCompletedAreTheWholeSeasonsBehindThePlayer(int days, int seasons)
    {
        Assert.That(RunSummary.Build(days, 28, 0, 0, 0, null, 0).seasonsCompleted, Is.EqualTo(seasons));
    }

    [Test]
    public void ANegativeDayCountOrMoneyIsKeptAtZeroAndNoMissedDaysIsEmpty()
    {
        RunSummary s = RunSummary.Build(-3, 28, -10, -20, -30, null, 5);
        Assert.That(s.daysSurvived, Is.EqualTo(0));
        Assert.That(s.incomeCents, Is.EqualTo(0L));
        Assert.That(s.expensesCents, Is.EqualTo(0L));
        Assert.That(s.balanceCents, Is.EqualTo(0L));
        Assert.That(s.missedDueDays, Is.Empty);
        Assert.That(s.cashCents, Is.EqualTo(5L));
    }

    [Test]
    public void TheSummaryDoesNotKeepAHoldOfTheCallersList()
    {
        var days = new System.Collections.Generic.List<int> { 168, 140 };
        RunSummary s = RunSummary.Build(178, 28, 0, 0, 0, days, 0);
        days.Clear();
        Assert.That(s.missedDueDays, Is.EqualTo(new[] { 140, 168 }));
    }

    [Test]
    public void TheSummaryMatchesTheLedgerAndTheLoanItWasBuiltFrom()
    {
        // The same numbers LedgerManager and MoneyManager hand over at the loss: the ledger's totals since the start, the loan's balance
        // and its open missed payments.
        var ledger = new LedgerState();
        ledger.Begin(1400000, 0, 360, 28);
        ledger.Record(LedgerCategory.SalesFish, 5000);
        ledger.Record(LedgerCategory.DownPayment, 1200000);
        ledger.Record(LedgerCategory.BuyTools, 2000);
        ledger.CloseThrough(6, 183000, 4800000, 0);
        var rows = ledger.Rows(183000, 4800000, 0, LedgerState.SeasonStartMinute(6, 28) + 10 * 1440);
        StatementRow all = LedgerState.SinceStart(rows);

        LoanState loan = NewLoan();
        loan.Advance(178);
        RunSummary s = RunSummary.Build(178, 28, all.income, all.expenses, loan.balanceCents, loan.open.Where(p => p.missed).Select(p => p.dueDay), 183000);

        Assert.That(s.incomeCents, Is.EqualTo(5000L));
        Assert.That(s.expensesCents, Is.EqualTo(1202000L));
        Assert.That(s.balanceCents, Is.EqualTo(4800000L));
        Assert.That(s.missedDueDays, Is.EqualTo(new[] { 112, 140, 168 }));
        Assert.That(s.seasonsCompleted, Is.EqualTo(6));
    }

    // --- The new-game setup ---

    [Test]
    public void ASetupRoundTripsThroughJson()
    {
        NewGameSetup setup = NewGameSetup.Of(1, true, 2, Money.Dollars(10000), Money.Dollars(5000));
        NewGameSetupSaveData data = NewGameSetupSaveData.Of(setup);
        NewGameSetup? back = JsonUtility.FromJson<NewGameSetupSaveData>(JsonUtility.ToJson(data)).ToSetup();

        Assert.That(back.HasValue, Is.True);
        Assert.That(back.Value.difficulty, Is.EqualTo(1));
        Assert.That(back.Value.hasLand, Is.True);
        Assert.That(back.Value.parcelIndex, Is.EqualTo(2));
        Assert.That(back.Value.downCents, Is.EqualTo(1000000L));
        Assert.That(back.Value.reserveCents, Is.EqualTo(500000L));
    }

    [Test]
    public void AGameWithNoLandKeepsNoParcelOrMoney()
    {
        NewGameSetup setup = NewGameSetup.Of(2, false, 2, 999, 999);
        Assert.That(setup.hasLand, Is.False);
        Assert.That(setup.parcelIndex, Is.EqualTo(0));
        Assert.That(setup.downCents, Is.EqualTo(0L));
        Assert.That(setup.reserveCents, Is.EqualTo(0L));
        Assert.That(setup.IsValid, Is.True);
    }

    [Test]
    public void NoSectionOrAnEmptyOneMeansNoSetup()
    {
        foreach (string json in new[] { "{}", "{\"version\":1,\"hasSetup\":false}", "{\"version\":0,\"hasSetup\":true,\"setup\":{\"difficulty\":1}}" })
            Assert.That(JsonUtility.FromJson<NewGameSetupSaveData>(json).ToSetup().HasValue, Is.False, json);
        Assert.That(NewGameSetupSaveData.Of(null).ToSetup().HasValue, Is.False);
    }

    [TestCase(-1, 0, 0L, 0L)]
    [TestCase(3, 0, 0L, 0L)]
    [TestCase(1, -1, 0L, 0L)]
    [TestCase(1, 3, 0L, 0L)]
    [TestCase(1, 0, -5L, 0L)]
    [TestCase(1, 0, 0L, -5L)]
    public void ABadSetupIsNotUsedToRestart(int difficulty, int parcel, long down, long reserve)
    {
        NewGameSetup bad = new NewGameSetup { difficulty = difficulty, hasLand = true, parcelIndex = parcel, downCents = down, reserveCents = reserve };
        Assert.That(bad.IsValid, Is.False);
        Assert.That(NewGameSetupSaveData.Of(bad).ToSetup().HasValue, Is.False);
    }

    [Test]
    public void EveryRealNewGameChoiceIsAValidSetup()
    {
        for (int tier = 0; tier <= 2; tier++)
            for (int parcel = 0; parcel < LandRules.Starters.Length; parcel++)
            {
                long savings = LandRules.SavingsCents(LandRules.Starters[parcel].priceCents, LandRules.SavingsPercent(tier));
                LandChoice choice = LandRules.Choose(parcel, savings, LandRules.DefaultDown(savings));
                Assert.That(NewGameSetup.Of(tier, true, choice.parcelIndex, choice.downCents, choice.reserveCents).IsValid, Is.True);
            }
    }
}
