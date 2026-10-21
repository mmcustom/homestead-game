using System.Linq;
using NUnit.Framework;
using UnityEngine;

// EditMode tests for the land loan (LoanState), the New Game land rules (LandRules) and amount parsing (Money.TryParseCents).
// All pure: no files, no scene, no managers.
public class LoanTests
{
    static readonly LoanTerms Terms = LoanTerms.Default;
    const long Rich = 1000000000L; // cash that never runs out

    // The 5-acre parcel with $12,000 down: a $48,000 loan, $458.71 a payment.
    static LoanState Make(long priceDollars = 60000, long downDollars = 12000) =>
        LoanState.Create(5, Money.Dollars(priceDollars), Money.Dollars(downDollars), Terms);

    // --- The formula ---

    [TestCase(48000, 45871)]
    [TestCase(60000, 57339)]
    [TestCase(112000, 107033)]
    [TestCase(360000, 344035)]
    public void ThePaymentMatchesTheDesignNumbersToTheCent(long loanDollars, long expectedCents)
    {
        Assert.That(LoanState.PaymentFor(Money.Dollars(loanDollars), Terms), Is.EqualTo(expectedCents));
    }

    [Test]
    public void NoLoanNoPayment()
    {
        Assert.That(LoanState.PaymentFor(0, Terms), Is.EqualTo(0L));
        Assert.That(LoanState.PaymentFor(-100, Terms), Is.EqualTo(0L));
    }

    [Test]
    public void InterestRoundsHalfUpToTheCent()
    {
        Assert.That(LoanState.InterestFor(74, Terms), Is.EqualTo(0L), "0.49 cents");
        Assert.That(LoanState.InterestFor(75, Terms), Is.EqualTo(1L), "0.5 cents rounds up");
        Assert.That(LoanState.InterestFor(225, Terms), Is.EqualTo(2L), "1.5 cents rounds up");
        Assert.That(LoanState.InterestFor(Money.Dollars(48000), Terms), Is.EqualTo(32000L), "$48,000 at 8%/12 is $320.00");
    }

    [Test]
    public void ANewLoanStartsWithTheFirstPaymentOnDay112()
    {
        LoanState loan = Make();
        Assert.That(loan.originalCents, Is.EqualTo(Money.Dollars(48000)));
        Assert.That(loan.balanceCents, Is.EqualTo(Money.Dollars(48000)));
        Assert.That(loan.paymentCents, Is.EqualTo(45871L));
        Assert.That(loan.nextDueDay, Is.EqualTo(112));
        Assert.That(loan.open, Is.Empty);
    }

    [Test]
    public void AllHundredEightyPaymentsOnTimeEndAtZero()
    {
        LoanState loan = Make();
        long last = 0;
        for (int k = 0; k < 180; k++)
        {
            loan.Advance(112 + 28 * k);
            Assert.That(loan.open.Count, Is.EqualTo(1), $"payment {k + 1}");
            last = loan.OwedNow;
            PayResult result = loan.Pay(Rich, loan.OwedNow);
            Assert.That(result.cleared, Is.EqualTo(1));
            Assert.That(loan.open, Is.Empty);
        }
        Assert.That(loan.balanceCents, Is.EqualTo(0L));
        Assert.That(loan.paymentsMade, Is.EqualTo(180));
        Assert.That(loan.IsPaidOff, Is.True);
        Assert.That(loan.nextDueDay, Is.EqualTo(-1));
        Assert.That(loan.principalPaid, Is.EqualTo(loan.originalCents));
        Assert.That(loan.interestPaid, Is.EqualTo(LoanState.TotalInterest(loan.originalCents, Terms)), "the preview matches what was paid");
        Assert.That(System.Math.Abs(last - loan.paymentCents), Is.LessThan(500L), "the last payment only absorbs the rounding (within $5)");
        Assert.That(loan.Advance(112 + 28 * 400), Is.Empty, "nothing falls due after the loan is paid");
        Assert.That(loan.open, Is.Empty);
    }

    [Test]
    public void TheInterestPreviewIsAboutWhatTheScheduleAdds()
    {
        long interest = LoanState.TotalInterest(Money.Dollars(48000), Terms);
        long byPayments = 180 * 45871L - Money.Dollars(48000);
        Assert.That(System.Math.Abs(interest - byPayments), Is.LessThan(500L), "within $5 of payment x 180 - loan");
    }

    // --- Due dates and lateness ---

    [Test]
    public void TheFirstPaymentFallsDueOnDay112()
    {
        LoanState loan = Make();
        loan.Advance(111);
        Assert.That(loan.open, Is.Empty);
        loan.Advance(112);
        Assert.That(loan.open.Count, Is.EqualTo(1));
        Assert.That(loan.open[0].dueDay, Is.EqualTo(112));
        Assert.That(loan.open[0].Owed, Is.EqualTo(loan.paymentCents));
        Assert.That(loan.open[0].interest, Is.EqualTo(32000L), "interest on the full balance");
    }

    [Test]
    public void PaymentsFallDueEvery28Days()
    {
        LoanState loan = Make();
        loan.Advance(140);
        Assert.That(loan.open.Select(p => p.dueDay), Is.EqualTo(new[] { 112, 140 }));
        loan.Advance(167);
        Assert.That(loan.open.Count, Is.EqualTo(2));
        loan.Advance(168);
        Assert.That(loan.open.Select(p => p.dueDay), Is.EqualTo(new[] { 112, 140, 168 }));
    }

    [Test]
    public void NotLateAtDuePlusNineLateAtDuePlusTen()
    {
        LoanState loan = Make();
        loan.Advance(112 + 9);
        Assert.That(loan.MissedCount, Is.EqualTo(0));
        Assert.That(loan.open[0].fee, Is.EqualTo(0L));
        loan.Advance(112 + 10);
        Assert.That(loan.MissedCount, Is.EqualTo(1));
        Assert.That(loan.open[0].fee, Is.EqualTo(7500L));
        Assert.That(loan.open[0].missed, Is.True);
    }

    [Test]
    public void TheLateFeeIsAddedOnlyOncePerPayment()
    {
        LoanState loan = Make();
        loan.Advance(122);
        loan.Advance(122);
        loan.Advance(125);
        Assert.That(loan.open[0].fee, Is.EqualTo(7500L));
        loan.Advance(139);
        Assert.That(loan.open[0].fee, Is.EqualTo(7500L), "still one $75 fee");
        Assert.That(loan.FeesOwed, Is.EqualTo(7500L));
    }

    [Test]
    public void MissedPaymentsStayOpenAndNewOnesKeepFallingDue()
    {
        LoanState loan = Make();
        loan.Advance(178);
        Assert.That(loan.open.Count, Is.EqualTo(3));
        Assert.That(loan.MissedCount, Is.EqualTo(3));
        loan.Advance(196);
        Assert.That(loan.open.Count, Is.EqualTo(4), "a fourth fell due while three were open");
        Assert.That(loan.MissedCount, Is.EqualTo(3));
    }

    [Test]
    public void CatchingUpClearsTheMissedCount()
    {
        LoanState loan = Make();
        loan.Advance(178);
        Assert.That(loan.MissedCount, Is.EqualTo(3));
        long owed = loan.OwedNow;
        Assert.That(owed, Is.EqualTo(3 * 45871L + 3 * 7500L));

        PayResult result = loan.Pay(Rich, owed);
        Assert.That(result.cleared, Is.EqualTo(3));
        Assert.That(loan.MissedCount, Is.EqualTo(0));
        Assert.That(loan.open, Is.Empty);
        Assert.That(loan.feesPaid, Is.EqualTo(22500L));
        Assert.That(loan.paymentsMade, Is.EqualTo(3));
    }

    [Test]
    public void PayingOnlyTheOldestOneLeavesTheOthersMissed()
    {
        LoanState loan = Make();
        loan.Advance(178);
        loan.Pay(Rich, loan.open[0].Owed);
        Assert.That(loan.open.Count, Is.EqualTo(2));
        Assert.That(loan.MissedCount, Is.EqualTo(2));
        Assert.That(loan.open[0].dueDay, Is.EqualTo(140));
    }

    // --- Paying ---

    [Test]
    public void AShortPaymentGoesToTheFeeThenTheInterestThenThePrincipal()
    {
        LoanState loan = Make();
        loan.Advance(122);
        OpenPayment p = loan.open[0];
        long interest = p.interest, principal = p.principal;

        PayResult first = loan.Pay(Rich, 5000);
        Assert.That(first.toFee, Is.EqualTo(5000L));
        Assert.That(p.fee, Is.EqualTo(2500L));
        Assert.That(p.interest, Is.EqualTo(interest));

        PayResult second = loan.Pay(Rich, 2500 + interest + 100);
        Assert.That(second.toFee, Is.EqualTo(2500L));
        Assert.That(second.toInterest, Is.EqualTo(interest));
        Assert.That(second.toPrincipal, Is.EqualTo(100L));
        Assert.That(p.fee, Is.EqualTo(0L));
        Assert.That(p.interest, Is.EqualTo(0L));
        Assert.That(p.principal, Is.EqualTo(principal - 100));
        Assert.That(loan.balanceCents, Is.EqualTo(Money.Dollars(48000) - 100));
        Assert.That(loan.open.Count, Is.EqualTo(1), "a short payment leaves it open");
        Assert.That(loan.MissedCount, Is.EqualTo(1), "and still missed");
    }

    [Test]
    public void AnExtraPaymentLowersTheBalanceAndTheNextInterest()
    {
        LoanState plain = Make(), extra = Make();
        plain.Advance(112);
        extra.Advance(112);
        plain.Pay(Rich, plain.OwedNow);
        PayResult result = extra.Pay(Rich, extra.OwedNow + Money.Dollars(1000));
        Assert.That(result.toExtra, Is.EqualTo(Money.Dollars(1000)));
        Assert.That(extra.extraPaid, Is.EqualTo(Money.Dollars(1000)));
        Assert.That(extra.balanceCents, Is.EqualTo(plain.balanceCents - Money.Dollars(1000)));

        plain.Advance(140);
        extra.Advance(140);
        Assert.That(extra.open[0].interest, Is.LessThan(plain.open[0].interest));
        Assert.That(extra.open[0].Owed, Is.EqualTo(extra.paymentCents), "the payment itself is unchanged");
        Assert.That(extra.open[0].principal, Is.GreaterThan(plain.open[0].principal));
    }

    [Test]
    public void ExtraMoneyClearsEveryOpenPaymentBeforeItCountsAsExtra()
    {
        LoanState loan = Make();
        loan.Advance(140);
        Assert.That(loan.open.Count, Is.EqualTo(2));
        long owed = loan.OwedNow;
        PayResult result = loan.Pay(Rich, owed + Money.Dollars(500));
        Assert.That(result.cleared, Is.EqualTo(2));
        Assert.That(result.toExtra, Is.EqualTo(Money.Dollars(500)));
        Assert.That(loan.open, Is.Empty);
    }

    [Test]
    public void AnExtraPaymentNeverCoversAFuturePayment()
    {
        LoanState loan = Make();
        loan.Advance(112);
        loan.Pay(Rich, loan.OwedNow + Money.Dollars(5000));
        Assert.That(loan.open, Is.Empty);
        Assert.That(loan.nextDueDay, Is.EqualTo(140), "the next payment still falls due on schedule");
        loan.Advance(140);
        Assert.That(loan.open.Count, Is.EqualTo(1));
        Assert.That(loan.open[0].Owed, Is.EqualTo(loan.paymentCents), "and is the full payment");
    }

    [Test]
    public void ExtraPaymentsLetTheLastPaymentBeSmaller()
    {
        LoanState loan = Make();
        loan.Advance(112);
        loan.Pay(Rich, loan.OwedNow + loan.UnassignedCents - Money.Dollars(100));
        Assert.That(loan.balanceCents, Is.EqualTo(Money.Dollars(100)));
        loan.Advance(140);
        Assert.That(loan.open.Count, Is.EqualTo(1));
        Assert.That(loan.open[0].principal, Is.EqualTo(Money.Dollars(100)), "the last payment clears what is left");
        Assert.That(loan.open[0].Owed, Is.LessThan(loan.paymentCents));
        Assert.That(loan.nextDueDay, Is.EqualTo(-1));
    }

    [Test]
    public void PayingOffTheWholeLoanIsReportedOnce()
    {
        LoanState loan = Make();
        loan.Advance(112);
        PayResult result = loan.Pay(Rich, Rich);
        Assert.That(result.applied, Is.EqualTo(loan.originalCents + 32000L), "all the principal plus the one interest charge");
        Assert.That(result.paidOff, Is.True);
        Assert.That(loan.IsPaidOff, Is.True);
        Assert.That(loan.balanceCents, Is.EqualTo(0L));
        Assert.That(loan.nextDueDay, Is.EqualTo(-1));
        Assert.That(loan.Pay(Rich, Rich).applied, Is.EqualTo(0L));
        Assert.That(loan.Advance(500), Is.Empty);
    }

    [Test]
    public void CashNeverGoesNegativeAndAPaymentNeverExceedsTheLoan()
    {
        LoanState loan = Make();
        loan.Advance(112);
        PayResult tight = loan.Pay(1000, Money.Dollars(10000));
        Assert.That(tight.applied, Is.EqualTo(1000L), "no more than the cash");
        Assert.That(loan.Pay(0, 5000).applied, Is.EqualTo(0L));
        Assert.That(loan.Pay(-5, 5000).applied, Is.EqualTo(0L));
        Assert.That(loan.Pay(Rich, 0).applied, Is.EqualTo(0L));
        Assert.That(loan.Pay(Rich, -100).applied, Is.EqualTo(0L));

        PayResult all = loan.Pay(Rich, Rich);
        Assert.That(all.applied, Is.LessThanOrEqualTo(loan.originalCents + 32000L));
        Assert.That(loan.balanceCents, Is.EqualTo(0L));
    }

    // --- Skipping time ---

    [Test]
    public void SleepingOverADueDateCreatesThePaymentAndWarnsOnce()
    {
        LoanState loan = Make();
        loan.Advance(100);
        var notices = loan.Advance(118);
        Assert.That(loan.open.Count, Is.EqualTo(1));
        Assert.That(loan.MissedCount, Is.EqualTo(0));
        Assert.That(notices.Select(n => n.kind), Is.EqualTo(new[] { LoanNoticeKind.Late }), "one warning, not one per day");
    }

    [Test]
    public void SleepingOverTheLateDateAddsTheFeeAndWarnsOnce()
    {
        LoanState loan = Make();
        loan.Advance(100);
        var notices = loan.Advance(130);
        Assert.That(loan.MissedCount, Is.EqualTo(1));
        Assert.That(loan.open[0].fee, Is.EqualTo(7500L));
        Assert.That(notices.Select(n => n.kind), Is.EqualTo(new[] { LoanNoticeKind.Missed }));
    }

    [Test]
    public void ALongSkipProcessesEveryDueDateInOrder()
    {
        LoanState loan = Make();
        loan.Advance(0);
        loan.Advance(200);
        Assert.That(loan.open.Select(p => p.dueDay), Is.EqualTo(new[] { 112, 140, 168, 196 }));
        Assert.That(loan.open.Select(p => p.number), Is.EqualTo(new[] { 1, 2, 3, 4 }));
        Assert.That(loan.MissedCount, Is.EqualTo(3), "112, 140 and 168 are past their tenth day; 196 is not");
        Assert.That(loan.nextDueDay, Is.EqualTo(224));
    }

    [Test]
    public void ProcessingTheSameDayAgainChangesNothing()
    {
        LoanState loan = Make();
        loan.Advance(122);
        string before = JsonUtility.ToJson(loan);
        Assert.That(loan.Advance(122), Is.Empty);
        Assert.That(JsonUtility.ToJson(loan), Is.EqualTo(before));
    }

    [Test]
    public void TheWarningsComeOncePerStage()
    {
        LoanState loan = Make();
        Assert.That(loan.Advance(104), Is.Empty, "eight days out is too early");
        var soon = loan.Advance(105);
        Assert.That(soon.Select(n => n.kind), Is.EqualTo(new[] { LoanNoticeKind.DueSoon }));
        Assert.That(soon[0].days, Is.EqualTo(7));
        Assert.That(loan.Advance(106), Is.Empty, "not again");
        Assert.That(loan.Advance(112).Select(n => n.kind), Is.EqualTo(new[] { LoanNoticeKind.DueToday }));
        Assert.That(loan.Advance(113).Select(n => n.kind), Is.EqualTo(new[] { LoanNoticeKind.Late }));
        Assert.That(loan.Advance(121), Is.Empty);
        Assert.That(loan.Advance(122).Select(n => n.kind), Is.EqualTo(new[] { LoanNoticeKind.Missed }));
    }

    // --- Saving ---

    [Test]
    public void ALoanWithOpenPaymentsRoundTripsExactly()
    {
        LoanState loan = Make();
        loan.Advance(178);
        loan.Pay(Rich, 12345);
        string json = JsonUtility.ToJson(LoanSaveData.Of(loan));
        LoanState back = JsonUtility.FromJson<LoanSaveData>(json).ToLoan();

        Assert.That(back, Is.Not.Null);
        Assert.That(JsonUtility.ToJson(back), Is.EqualTo(JsonUtility.ToJson(loan)));
        Assert.That(back.open.Count, Is.EqualTo(3));
        Assert.That(back.balanceCents, Is.EqualTo(loan.balanceCents));
        Assert.That(back.nextDueDay, Is.EqualTo(196));

        // And it behaves the same afterwards.
        loan.Advance(250);
        back.Advance(250);
        Assert.That(JsonUtility.ToJson(back), Is.EqualTo(JsonUtility.ToJson(loan)));
    }

    [Test]
    public void ASaveWithNoLoanLoadsWithNoLoan()
    {
        Assert.That(JsonUtility.FromJson<LoanSaveData>("{}").ToLoan(), Is.Null);
        Assert.That(JsonUtility.FromJson<LoanSaveData>("{\"version\":1,\"hasLoan\":false}").ToLoan(), Is.Null);
        Assert.That(LoanSaveData.Of(null).ToLoan(), Is.Null);
        string json = JsonUtility.ToJson(LoanSaveData.Of(null));
        Assert.That(JsonUtility.FromJson<LoanSaveData>(json).hasLoan, Is.False);
    }

    // --- New Game: land, savings and the split ---

    [TestCase(0, 2400000L, 3000000L, 3000000L)]   // Homesteader 40%: $24,000, $56,000 capped to $30,000, capped
    [TestCase(1, 1200000L, 2800000L, 3000000L)]   // Settler 20%: $12,000, $28,000, $90,000 capped to $30,000
    [TestCase(2, 0L, 0L, 0L)]                     // Pioneer: none
    public void SavingsByTierAndParcelWithTheThirtyThousandCap(int tier, long five, long ten, long forty)
    {
        int percent = LandRules.SavingsPercent(tier);
        Assert.That(LandRules.SavingsCents(LandRules.Starters[0].priceCents, percent), Is.EqualTo(five), "5 acres");
        Assert.That(LandRules.SavingsCents(LandRules.Starters[1].priceCents, percent), Is.EqualTo(ten), "10 acres");
        Assert.That(LandRules.SavingsCents(LandRules.Starters[2].priceCents, percent), Is.EqualTo(forty), "40 acres");
    }

    [Test]
    public void TheThreeStarterParcelsHaveTheDesignPrices()
    {
        Assert.That(LandRules.Starters.Select(p => p.acres), Is.EqualTo(new[] { 5, 10, 40 }));
        Assert.That(LandRules.Starters.Select(p => p.priceCents), Is.EqualTo(new[] { 6000000L, 14000000L, 45000000L }));
        Assert.That(LandRules.Starters.Select(p => p.PricePerAcreCents), Is.EqualTo(new[] { 1200000L, 1400000L, 1125000L }));
    }

    [Test]
    public void ThePaymentPreviewForEachParcelMatchesTheDesign()
    {
        Assert.That(LoanState.PaymentFor(LandRules.Starters[0].priceCents - 1200000L, Terms), Is.EqualTo(45871L), "5 acres, Settler, all down");
        Assert.That(LoanState.PaymentFor(LandRules.Starters[0].priceCents, Terms), Is.EqualTo(57339L), "5 acres, no down payment");
        Assert.That(LoanState.PaymentFor(LandRules.Starters[1].priceCents - 2800000L, Terms), Is.EqualTo(107033L), "10 acres, Settler, all down");
        Assert.That(LoanState.PaymentFor(LandRules.Starters[2].priceCents - 9000000L, Terms), Is.EqualTo(344035L), "40 acres, 20% down");
    }

    [Test]
    public void TheSplitAlwaysAddsUpToTheSavings()
    {
        long savings = 1200000;
        Assert.That(LandRules.DefaultDown(savings), Is.EqualTo(600000L), "half and half");
        foreach (long down in new long[] { 0, 1, 599999, 600000, 1199999, 1200000 })
        {
            Assert.That(LandRules.ValidDown(savings, down), Is.True);
            Assert.That(down + LandRules.Reserve(savings, down), Is.EqualTo(savings));
            LandChoice choice = LandRules.Choose(0, savings, down);
            Assert.That(choice.downCents + choice.reserveCents, Is.EqualTo(savings));
        }
        Assert.That(LandRules.ValidDown(savings, -1), Is.False);
        Assert.That(LandRules.ValidDown(savings, savings + 1), Is.False);
    }

    [Test]
    public void AnOddCentInTheSavingsGoesToTheReserve()
    {
        long savings = 100001;
        long down = LandRules.DefaultDown(savings);
        Assert.That(down, Is.EqualTo(50000L));
        Assert.That(LandRules.Reserve(savings, down), Is.EqualTo(50001L));
    }

    [Test]
    public void PioneerFinancesTheWholePrice()
    {
        long price = LandRules.Starters[0].priceCents;
        long savings = LandRules.SavingsCents(price, LandRules.SavingsPercent(2));
        Assert.That(savings, Is.EqualTo(0L));
        LoanState loan = LoanState.Create(5, price, 0, Terms);
        Assert.That(loan.originalCents, Is.EqualTo(price));
        Assert.That(loan.paymentCents, Is.EqualTo(57339L));
    }

    // --- Typing an amount ---

    [TestCase("12", 1200L)]
    [TestCase("12.5", 1250L)]
    [TestCase("12.50", 1250L)]
    [TestCase("$1,250.00", 125000L)]
    [TestCase("  250  ", 25000L)]
    [TestCase(".5", 50L)]
    [TestCase("5.", 500L)]
    [TestCase("0", 0L)]
    [TestCase("0.01", 1L)]
    public void AmountsTypedByThePlayerAreRead(string text, long expected)
    {
        Assert.That(Money.TryParseCents(text, out long cents), Is.True, text);
        Assert.That(cents, Is.EqualTo(expected));
    }

    [TestCase("")]
    [TestCase("   ")]
    [TestCase("abc")]
    [TestCase("-5")]
    [TestCase("1.234")]
    [TestCase("1.2.3")]
    [TestCase("1e5")]
    [TestCase(".")]
    [TestCase("$")]
    [TestCase("1234567890123456789")]
    public void NonsenseAmountsAreRefused(string text)
    {
        Assert.That(Money.TryParseCents(text, out _), Is.False, text);
    }
}
