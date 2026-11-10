using System.Linq;
using NUnit.Framework;
using UnityEngine;

// EditMode tests for the land loan on the real calendar (Phase 1): a payment on the 1st of every month from July 1 (day 122), grace 10 days, a $75 late fee, the game lost at 3 open missed
// payments, 180 payments exactly 15 game years. The old schedule (day 112, then every 28 days) is LoanTerms.Legacy and is pinned by LoanTests.
public class MonthlyLoanTests
{
    static readonly GameCalendar Real = CalendarRules.Real;
    static LoanState Loan(long cents = 4800000L) => LoanState.Create(5, cents + 1200000L, 1200000L, LoanTerms.Default); // $60,000 land, $12,000 down

    // --- The payment ---

    [TestCase(4800000L, 45871L)]    // $48,000: $458.71 a month (Money_System.md's worked figure)
    [TestCase(6000000L, 57339L)]    // $60,000: $573.39 (what a Pioneer's 5-acre loan pays; slot 4's loan)
    [TestCase(11200000L, 107033L)]
    [TestCase(36000000L, 344035L)]
    public void TheMonthlyPaymentIsTheSameFormulaAsBefore(long loanCents, long paymentCents)
    {
        Assert.That(LoanState.PaymentFor(loanCents, LoanTerms.Default), Is.EqualTo(paymentCents));
        Assert.That(LoanState.PaymentFor(loanCents, LoanTerms.Legacy), Is.EqualTo(paymentCents), "the schedule changed, not the amount");
    }

    [Test]
    public void ThePaymentIsSaneAgainstADaysIncome()
    {
        // $573.39 a month is about $18.85 a game day over a 30.4-day month; Early_Income_Design's target is about $25 a game day.
        long perDayCents = LoanState.PaymentFor(6000000L, LoanTerms.Default) * 12 / 365;
        Assert.That(perDayCents, Is.InRange(1500L, 2000L));
        // And the interest is a twelfth of 8% a year on each payment, which twelve payments a year now makes literally true.
        Assert.That(LoanState.InterestFor(6000000L, LoanTerms.Default), Is.EqualTo(40000L), "8% / 12 of $60,000 is $400.00");
    }

    // --- The schedule ---

    [Test]
    public void TheFirstPaymentFallsDueOnJulyFirst()
    {
        LoanState loan = Loan();
        Assert.That(loan.nextDueDay, Is.EqualTo(122));
        Assert.That(Real.FormatDate(loan.nextDueDay), Is.EqualTo("Jul 1, Year 1"));
        Assert.That(loan.terms.dueDayOfMonth, Is.EqualTo(1));

        Assert.That(loan.Advance(121).Where(n => n.kind != LoanNoticeKind.DueSoon), Is.Empty);
        Assert.That(loan.open, Is.Empty);
        loan.Advance(122);
        Assert.That(loan.open.Select(p => p.dueDay), Is.EqualTo(new[] { 122 }));
        Assert.That(loan.nextDueDay, Is.EqualTo(153), "August 1");
    }

    [Test]
    public void EveryLaterPaymentIsTheFirstOfTheNextMonthAcrossTheYearEnd()
    {
        LoanState loan = Loan();
        loan.Advance(122 + 365 + 40); // well into Year 2
        int[] expected = { 122, 153, 184, 214, 245, 275, 306, 337, 365, 396, 426, 457 };
        Assert.That(loan.open.Select(p => p.dueDay).Take(expected.Length), Is.EqualTo(expected),
            "Jul 1, Aug 1, Sep 1, Oct 1, Nov 1, Dec 1, Jan 1, Feb 1, then Mar 1 of Year 2 (February is short but still has its 1st)");
        Assert.That(loan.open.Select(p => Real.DayOfMonth(p.dueDay)), Is.All.EqualTo(1));
        Assert.That(loan.open.Select(p => p.number), Is.EqualTo(Enumerable.Range(1, loan.open.Count)));
    }

    [Test]
    public void ASkipOverSeveralMonthsProcessesEveryDueDateInOrder()
    {
        LoanState loan = Loan();
        loan.Advance(250); // five payments have fallen due: Jul, Aug, Sep, Oct, Nov
        Assert.That(loan.open.Select(p => p.dueDay), Is.EqualTo(new[] { 122, 153, 184, 214, 245 }));
        Assert.That(loan.paymentsDue, Is.EqualTo(5));
        Assert.That(loan.Advance(250), Is.Empty, "the same day again does nothing");
    }

    [Test]
    public void ApaymentUnpaidTenDaysAfterItsDueDayIsMissedAndGetsTheLateFee()
    {
        LoanState loan = Loan();
        loan.Advance(122 + 9);
        Assert.That(loan.MissedCount, Is.EqualTo(0));
        loan.Advance(122 + 10);
        Assert.That(loan.MissedCount, Is.EqualTo(1), "the 11th of July");
        Assert.That(loan.open[0].fee, Is.EqualTo(7500L), "the $75 late fee, once");
        loan.Advance(122 + 20);
        Assert.That(loan.open[0].fee, Is.EqualTo(7500L));
    }

    [Test]
    public void TheThirdMissedPaymentLosesTheGameOnTheEleventhOfSeptember()
    {
        LoanState loan = Loan();
        loan.Advance(184 + 9);
        Assert.That(LossRules.IsLoss(loan.MissedCount, LossRules.DefaultThreshold), Is.False, "Jul and Aug are missed, Sep 1's is still in its grace");
        Assert.That(loan.MissedCount, Is.EqualTo(2));
        loan.Advance(184 + 10);
        Assert.That(loan.MissedCount, Is.EqualTo(3));
        Assert.That(LossRules.IsLoss(loan.MissedCount, LossRules.DefaultThreshold), Is.True);
        Assert.That(Real.FormatDate(184 + 10), Is.EqualTo("Sep 11, Year 1"));
    }

    [Test]
    public void PayingOnTimeClearsThePaymentAndTheNextOneIsStillTheFirstOfTheNextMonth()
    {
        LoanState loan = Loan();
        loan.Advance(122);
        PayResult paid = loan.Pay(1000000L, loan.OwedNow);
        Assert.That(paid.cleared, Is.EqualTo(1));
        Assert.That(loan.open, Is.Empty);
        Assert.That(loan.paymentsMade, Is.EqualTo(1));
        loan.Advance(153);
        Assert.That(loan.open.Select(p => p.dueDay), Is.EqualTo(new[] { 153 }));
    }

    [Test]
    public void The180PaymentsAreExactlyFifteenGameYears()
    {
        LoanState loan = Loan();
        loan.Advance(15 * 365 + 400);
        Assert.That(loan.paymentsDue, Is.EqualTo(180));
        Assert.That(loan.nextDueDay, Is.EqualTo(-1), "no payment after the 180th");
        int last = loan.open.Last().dueDay;
        Assert.That(Real.MonthIndexOf(last) - Real.MonthIndexOf(loan.open.First().dueDay), Is.EqualTo(179), "180 consecutive months");
        Assert.That(Real.MonthIndexOf(last), Is.EqualTo(4 + 179));
        Assert.That(Real.FormatDate(last), Is.EqualTo("Jun 1, Year 16"), "July of Year 1 plus 179 months");
        Assert.That(last - loan.open.First().dueDay, Is.EqualTo(15 * 365 - 30), "Jul 1 of Year 1 to Jun 1 of Year 16: one month short of 15 years");
    }

    [Test]
    public void ALoanMadeOnTheOldRuleKeepsItsEveryTwentyEightDaysWhateverTheCalendar()
    {
        LoanState legacy = LoanState.Create(5, 6000000L, 1200000L, LoanTerms.Legacy);
        Assert.That(legacy.terms.dueDayOfMonth, Is.EqualTo(0));
        legacy.Advance(112 + 56);
        Assert.That(legacy.open.Select(p => p.dueDay), Is.EqualTo(new[] { 112, 140, 168 }));
    }

    // --- Saving ---

    [Test]
    public void TheDayOfTheMonthIsSavedAndASaveFromBeforeItReadsTheOldRule()
    {
        LoanState loan = Loan();
        loan.Advance(122);
        LoanState back = JsonUtility.FromJson<LoanSaveData>(JsonUtility.ToJson(LoanSaveData.Of(loan))).ToLoan();
        Assert.That(back.terms.dueDayOfMonth, Is.EqualTo(1));
        Assert.That(back.nextDueDay, Is.EqualTo(153));
        back.Advance(153);
        Assert.That(back.open.Select(p => p.dueDay), Is.EqualTo(new[] { 122, 153 }));

        // The JSON of a loan saved before the field existed: no dueDayOfMonth, so it reads 0 and keeps every intervalDays.
        const string old = "{\"version\":1,\"hasLoan\":true,\"loan\":{\"acres\":5,\"priceCents\":6000000,\"downCents\":0,\"originalCents\":6000000,\"paymentCents\":57339,\"balanceCents\":6000000,\"nextDueDay\":112,\"paymentsDue\":0,\"paymentsMade\":0,\"interestPaid\":0,\"feesPaid\":0,\"principalPaid\":0,\"extraPaid\":0,\"soonNoticeDue\":-1,\"terms\":{\"rateBasisPoints\":800,\"payments\":180,\"intervalDays\":28,\"firstDueDay\":112,\"graceDays\":10,\"lateFeeCents\":7500},\"open\":[]}}";
        LoanState oldLoan = JsonUtility.FromJson<LoanSaveData>(old).ToLoan();
        Assert.That(oldLoan.terms.dueDayOfMonth, Is.EqualTo(0));
        oldLoan.Advance(140);
        Assert.That(oldLoan.open.Select(p => p.dueDay), Is.EqualTo(new[] { 112, 140 }));
    }

    // --- Wording ---

    [Test]
    public void TheScheduleReadsAsMonthlyForTheNewLoanAndEveryNDaysForTheOldOne()
    {
        Assert.That(LoanScheduleText.Cadence(LoanTerms.Default), Is.EqualTo("a month, due on the 1st"));
        Assert.That(LoanScheduleText.Cadence(LoanTerms.Legacy), Is.EqualTo("every 28 days"));
        Assert.That(LoanScheduleText.PaymentEvery(LoanTerms.Default), Is.EqualTo("Payment every month"));
        Assert.That(LoanScheduleText.PaymentEvery(LoanTerms.Legacy), Is.EqualTo("Payment every 28 days"));
        Assert.That(LoanScheduleText.Count(LoanTerms.Default), Is.EqualTo("180, one a month (the 1st)"));
        Assert.That(LoanScheduleText.Count(LoanTerms.Legacy), Is.EqualTo("180, one every 28 days"));
    }

    [TestCase(1, "1st")]
    [TestCase(2, "2nd")]
    [TestCase(3, "3rd")]
    [TestCase(4, "4th")]
    [TestCase(11, "11th")]
    [TestCase(12, "12th")]
    [TestCase(13, "13th")]
    [TestCase(15, "15th")]
    [TestCase(21, "21st")]
    [TestCase(22, "22nd")]
    [TestCase(23, "23rd")]
    public void OrdinalsReadRight(int n, string text)
    {
        Assert.That(LoanScheduleText.Ordinal(n), Is.EqualTo(text));
    }
}
