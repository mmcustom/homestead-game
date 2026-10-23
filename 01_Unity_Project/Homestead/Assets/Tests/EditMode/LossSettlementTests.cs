using NUnit.Framework;

// EditMode tests for the savings-to-debt rule on a loss (Money_System.md, "Phase C addition"): the cash goes against everything owed in the
// order a payment is applied, on a COPY of the loan, and the real loan is never touched.
public class LossSettlementTests
{
    // The loan on its day of loss with nothing ever paid: three missed payments (due 112, 140, 168), a $75 late fee on each.
    static LoanState LostLoan()
    {
        LoanState loan = LoanState.Create(5, Money.Dollars(60000), Money.Dollars(12000), LoanTerms.Default);
        loan.Advance(178);
        return loan;
    }

    // Fees, interest and all the principal: the open payments' amounts plus the principal not yet in a payment.
    static long Everything(LoanState loan) => loan.OwedNow + loan.UnassignedCents;

    [Test]
    public void TheLostLoanHasThreeMissedPaymentsAndTheExpectedFees()
    {
        LoanState loan = LostLoan();
        Assert.That(loan.MissedCount, Is.EqualTo(3));
        Assert.That(loan.FeesOwed, Is.EqualTo(3 * 7500L));
        Assert.That(Everything(loan), Is.EqualTo(loan.MaxPayableCents));
    }

    [Test]
    public void NoCashTakesNothingAndEverythingIsStillOwed()
    {
        LoanState loan = LostLoan();
        LossSettlement s = LossSettlement.Of(loan, 0);
        Assert.That(s.cashHad, Is.EqualTo(0));
        Assert.That(s.taken, Is.EqualTo(0));
        Assert.That(s.stillOwed, Is.EqualTo(loan.MaxPayableCents));
        Assert.That(s.cashLeft, Is.EqualTo(0));
    }

    [Test]
    public void CashBelowTheLateFeesAllGoesToFees()
    {
        LoanState loan = LostLoan();
        long cash = 10000; // $100, less than the $225 of fees
        LossSettlement s = LossSettlement.Of(loan, cash);
        Assert.That(s.taken, Is.EqualTo(cash));
        Assert.That(s.stillOwed, Is.EqualTo(loan.MaxPayableCents - cash));
        Assert.That(s.cashLeft, Is.EqualTo(0));
    }

    [Test]
    public void CashBetweenTheFeesAndEverythingIsAllTakenAndTheRestIsStillOwed()
    {
        LoanState loan = LostLoan();
        long cash = Money.Dollars(2000);
        Assert.That(cash, Is.GreaterThan(loan.FeesOwed));
        Assert.That(cash, Is.LessThan(loan.MaxPayableCents));
        LossSettlement s = LossSettlement.Of(loan, cash);
        Assert.That(s.cashHad, Is.EqualTo(cash));
        Assert.That(s.taken, Is.EqualTo(cash));
        Assert.That(s.stillOwed, Is.EqualTo(loan.MaxPayableCents - cash));
        Assert.That(s.cashLeft, Is.EqualTo(0));
    }

    [Test]
    public void CashExactlyEqualToEverythingClearsTheDebtWithNothingLeft()
    {
        LoanState loan = LostLoan();
        long cash = loan.MaxPayableCents;
        LossSettlement s = LossSettlement.Of(loan, cash);
        Assert.That(s.taken, Is.EqualTo(cash));
        Assert.That(s.stillOwed, Is.EqualTo(0));
        Assert.That(s.cashLeft, Is.EqualTo(0));
    }

    [Test]
    public void CashAboveEverythingShowsZeroOwedAndTheLeftover()
    {
        LoanState loan = LostLoan();
        long everything = loan.MaxPayableCents;
        long cash = everything + 123456;
        LossSettlement s = LossSettlement.Of(loan, cash);
        Assert.That(s.cashHad, Is.EqualTo(cash));
        Assert.That(s.taken, Is.EqualTo(everything));
        Assert.That(s.stillOwed, Is.EqualTo(0));
        Assert.That(s.cashLeft, Is.EqualTo(123456));
    }

    [Test]
    public void TheRealLoanIsNeverChanged()
    {
        LoanState loan = LostLoan();
        long balance = loan.balanceCents, owed = loan.OwedNow, fees = loan.FeesOwed, interestPaid = loan.interestPaid;
        int open = loan.open.Count, missed = loan.MissedCount;

        LossSettlement.Of(loan, Money.Dollars(2000));
        LossSettlement.Of(loan, loan.MaxPayableCents + 1);

        Assert.That(loan.balanceCents, Is.EqualTo(balance));
        Assert.That(loan.OwedNow, Is.EqualTo(owed));
        Assert.That(loan.FeesOwed, Is.EqualTo(fees));
        Assert.That(loan.interestPaid, Is.EqualTo(interestPaid));
        Assert.That(loan.open.Count, Is.EqualTo(open));
        Assert.That(loan.MissedCount, Is.EqualTo(missed));
    }

    [Test]
    public void TheCashIsAppliedInTheOrderOfAPayment()
    {
        // The settlement must equal a real payment of the same cash on a separate copy: same order, same rounding.
        foreach (long cash in new long[] { 1, 7500, 22500, 22501, 80000, Money.Dollars(2000), Money.Dollars(5000) })
        {
            LoanState loan = LostLoan();
            LoanState paying = LostLoan();
            PayResult r = paying.Pay(cash, cash);
            LossSettlement s = LossSettlement.Of(loan, cash);
            Assert.That(s.taken, Is.EqualTo(r.applied), "taken, cash " + cash);
            Assert.That(s.stillOwed, Is.EqualTo(paying.MaxPayableCents), "owed, cash " + cash);
        }
    }

    [Test]
    public void TheOldestOpenPaymentIsClearedWholeBeforeTheNextOneIsTouched()
    {
        // A payment's own order is fee, interest, principal, and the oldest open payment goes first (Phase A): the cash does the same.
        LoanState loan = LostLoan();
        LoanState paying = LostLoan();
        long oldest = loan.open[0].Owed;
        long cash = oldest + 100;
        paying.Pay(cash, cash);
        Assert.That(paying.open.Count, Is.EqualTo(2), "the oldest payment is gone");
        Assert.That(paying.open[0].dueDay, Is.EqualTo(140));
        Assert.That(paying.open[0].fee, Is.EqualTo(7500 - 100), "the 100 went to the next payment's late fee first");
        Assert.That(paying.open[1].fee, Is.EqualTo(7500));
    }

    [Test]
    public void NoLoanTakesNothingAndTheCashStays()
    {
        LossSettlement s = LossSettlement.Of(null, 50000);
        Assert.That(s.cashHad, Is.EqualTo(50000));
        Assert.That(s.taken, Is.EqualTo(0));
        Assert.That(s.stillOwed, Is.EqualTo(0));
        Assert.That(s.cashLeft, Is.EqualTo(50000));
    }

    [Test]
    public void NegativeCashIsTreatedAsZero()
    {
        LossSettlement s = LossSettlement.Of(LostLoan(), -500);
        Assert.That(s.cashHad, Is.EqualTo(0));
        Assert.That(s.taken, Is.EqualTo(0));
        Assert.That(s.cashLeft, Is.EqualTo(0));
    }

    [Test]
    public void TheSummaryTakesTheSettlementAndKeepsIncomeAndExpensesUnchanged()
    {
        LoanState loan = LostLoan();
        RunSummary basic = RunSummary.Build(178, 28, 32695, 1299345, loan.balanceCents, new[] { 112, 140, 168 }, Money.Dollars(2000));
        RunSummary s = basic.WithSettlement(LossSettlement.Of(loan, Money.Dollars(2000)));
        Assert.That(s.incomeCents, Is.EqualTo(32695));
        Assert.That(s.expensesCents, Is.EqualTo(1299345));
        Assert.That(s.cashCents, Is.EqualTo(Money.Dollars(2000)));
        Assert.That(s.takenCents, Is.EqualTo(Money.Dollars(2000)));
        Assert.That(s.owedAfterCents, Is.EqualTo(loan.MaxPayableCents - Money.Dollars(2000)));
        Assert.That(s.cashLeftCents, Is.EqualTo(0));
        Assert.That(s.missedDueDays, Is.EqualTo(new[] { 112, 140, 168 }));
    }
}
