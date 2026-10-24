using NUnit.Framework;

// EditMode tests for the tier rules table (Money_System.md Phase D1): every tier holds today's values, the lookup, the loan terms a tier
// gives a new loan, and the sell share maths. All pure.
public class TierRulesTests
{
    // --- The table ---

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    public void EveryTierHoldsTodaysValues(int difficulty)
    {
        TierTerms tier = TierRules.For(difficulty);
        Assert.That(tier.missedBeforeLoss, Is.EqualTo(LossRules.DefaultThreshold));
        Assert.That(tier.missedBeforeLoss, Is.EqualTo(3));
        Assert.That(tier.lateFeeCents, Is.EqualTo(LoanTerms.Default.lateFeeCents));
        Assert.That(tier.lateFeeCents, Is.EqualTo(7500L));
        Assert.That(tier.rateBasisPoints, Is.EqualTo(LoanTerms.Default.rateBasisPoints));
        Assert.That(tier.rateBasisPoints, Is.EqualTo(800));
        Assert.That(tier.sellBasisPoints, Is.EqualTo(10000));
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    public void EveryTierIsValid(int difficulty)
    {
        Assert.That(TierRules.For(difficulty).IsValid, Is.True);
    }

    [Test]
    public void TheTableHasOneRowForEachOfTheThreeDifficulties()
    {
        Assert.That(TierRules.TierCount, Is.EqualTo(3));
        for (int d = 0; d < TierRules.TierCount; d++)
            Assert.That(TierRules.For(d).IsValid, Is.True);
    }

    [TestCase(-1)]
    [TestCase(3)]
    [TestCase(99)]
    public void ANumberOutsideTheThreeTiersGetsTheSettlerRow(int difficulty)
    {
        Assert.That(TierRules.For(difficulty), Is.EqualTo(TierRules.For(TierRules.Settler)));
    }

    [Test]
    public void ABadTierIsNotValid()
    {
        TierTerms ok = TierRules.For(1);
        TierTerms noLoss = ok; noLoss.missedBeforeLoss = 0;
        TierTerms tooMany = ok; tooMany.missedBeforeLoss = 11;
        TierTerms negativeFee = ok; negativeFee.lateFeeCents = -1;
        TierTerms hugeRate = ok; hugeRate.rateBasisPoints = 3001;
        TierTerms noSell = ok; noSell.sellBasisPoints = 0;
        TierTerms tooMuchSell = ok; tooMuchSell.sellBasisPoints = MarketRules.MaxBp + 1;
        foreach (TierTerms bad in new[] { noLoss, tooMany, negativeFee, hugeRate, noSell, tooMuchSell })
            Assert.That(bad.IsValid, Is.False);
    }

    // --- The loan terms a tier gives ---

    [Test]
    public void ALoanTakesItsRateAndLateFeeFromTheTierAndKeepsTheRest()
    {
        LoanTerms baseTerms = LoanTerms.Default;
        baseTerms.graceDays = 12;
        var tier = new TierTerms { missedBeforeLoss = 2, lateFeeCents = 10000, rateBasisPoints = 900, sellBasisPoints = 9000 };
        LoanTerms terms = TierRules.LoanTermsFor(baseTerms, tier);
        Assert.That(terms.rateBasisPoints, Is.EqualTo(900));
        Assert.That(terms.lateFeeCents, Is.EqualTo(10000L));
        Assert.That(terms.payments, Is.EqualTo(baseTerms.payments));
        Assert.That(terms.intervalDays, Is.EqualTo(baseTerms.intervalDays));
        Assert.That(terms.firstDueDay, Is.EqualTo(baseTerms.firstDueDay));
        Assert.That(terms.graceDays, Is.EqualTo(12));
    }

    [TestCase(0, 4800000L, 45871L)]
    [TestCase(1, 6000000L, 57339L)]
    [TestCase(2, 11200000L, 107033L)]
    public void EveryTierGivesTodaysPayments(int difficulty, long loanCents, long paymentCents)
    {
        LoanTerms terms = TierRules.LoanTermsFor(LoanTerms.Default, TierRules.For(difficulty));
        Assert.That(terms, Is.EqualTo(LoanTerms.Default));
        Assert.That(LoanState.PaymentFor(loanCents, terms), Is.EqualTo(paymentCents));
    }

    [Test]
    public void ALoanKeepsTheTermsItStartedWithWhenTheTableChanges()
    {
        // The terms are frozen into the loan: a loan made at 8% still charges 8% when a tier is later set to 9%.
        LoanState loan = LoanState.Create(5, 6000000L, 0L, TierRules.LoanTermsFor(LoanTerms.Default, TierRules.For(1)));
        LoanState back = UnityEngine.JsonUtility.FromJson<LoanSaveData>(UnityEngine.JsonUtility.ToJson(LoanSaveData.Of(loan))).ToLoan();
        var laterTier = new TierTerms { missedBeforeLoss = 3, lateFeeCents = 9900, rateBasisPoints = 900, sellBasisPoints = 10000 };
        Assert.That(TierRules.LoanTermsFor(LoanTerms.Default, laterTier).rateBasisPoints, Is.EqualTo(900));
        Assert.That(back.terms.rateBasisPoints, Is.EqualTo(800));
        Assert.That(back.terms.lateFeeCents, Is.EqualTo(7500L));
        Assert.That(back.paymentCents, Is.EqualTo(57339L));
    }

    [Test]
    public void ALoanMadeOnAnotherRateHasItsOwnPayment()
    {
        var tier = new TierTerms { missedBeforeLoss = 3, lateFeeCents = 7500, rateBasisPoints = 900, sellBasisPoints = 10000 };
        long payment = LoanState.PaymentFor(4800000L, TierRules.LoanTermsFor(LoanTerms.Default, tier));
        Assert.That(payment, Is.GreaterThan(45871L));
    }

    // --- The loss threshold ---

    [TestCase(1, 2, false)]
    [TestCase(2, 2, true)]
    [TestCase(3, 3, true)]
    [TestCase(3, 4, false)]
    [TestCase(4, 4, true)]
    public void TheLossComesAtTheTiersThreshold(int open, int threshold, bool lost)
    {
        Assert.That(LossRules.IsLoss(open, threshold), Is.EqualTo(lost));
    }

    // --- The sell share ---

    [TestCase(1000L, 10000, 1000L)]
    [TestCase(1000L, 10500, 1050L)]
    [TestCase(1000L, 9500, 950L)]
    [TestCase(1000L, 9000, 900L)]
    [TestCase(999L, 10500, 1049L)]   // 1048.95 rounds up
    [TestCase(999L, 9500, 949L)]     // 949.05 rounds down
    [TestCase(1L, 9500, 1L)]         // 0.95 rounds up
    [TestCase(1L, 5000, 1L)]         // exactly half rounds up
    [TestCase(1L, 4999, 0L)]
    [TestCase(3L, 5000, 2L)]         // 1.5 rounds up
    [TestCase(0L, 10500, 0L)]
    [TestCase(-5L, 10500, 0L)]
    [TestCase(1000L, 0, 0L)]
    public void TheSellShareScalesTheTotalAndRoundsHalfUp(long total, int basisPoints, long expected)
    {
        Assert.That(TierRules.ApplySell(total, basisPoints), Is.EqualTo(expected));
    }

    [Test]
    public void AShareOfTenThousandGivesExactlyTodaysTotals()
    {
        for (long total = 0; total <= 5000; total++)
            Assert.That(TierRules.ApplySell(total, 10000), Is.EqualTo(total));
        Assert.That(TierRules.ApplySell(long.MaxValue, 10000), Is.EqualTo(long.MaxValue));
    }

    [Test]
    public void ASaleAtFullShareMatchesTheMarketSaleToTheCent()
    {
        // A real market sale: 10 firewood at the falling price, then the tier at 100% and at 90%.
        SaleResult sale = MarketRules.SellStack(100, 10000, 10, 400, 2500);
        Assert.That(TierRules.ApplySell(sale.totalCents, 10000), Is.EqualTo(sale.totalCents));
        long ninety = TierRules.ApplySell(sale.totalCents, 9000);
        Assert.That(ninety, Is.EqualTo((sale.totalCents * 9000 + 5000) / 10000));
        Assert.That(ninety, Is.LessThan(sale.totalCents));
    }

    [Test]
    public void TheSellShareNeverOverflows()
    {
        Assert.That(TierRules.ApplySell(long.MaxValue, 20000), Is.EqualTo(long.MaxValue / 10000));
    }

    [TestCase(10000, "100%")]
    [TestCase(9000, "90%")]
    [TestCase(10500, "105%")]
    [TestCase(9550, "95.5%")]
    [TestCase(9525, "95.25%")]
    [TestCase(800, "8%")]
    public void ThePercentTextShowsBasisPointsAsAPercentage(int basisPoints, string expected)
    {
        Assert.That(TierRules.PercentText(basisPoints), Is.EqualTo(expected));
    }

    [TestCase(1, "1 missed payment")]
    [TestCase(3, "3 missed payments")]
    public void TheMissedLineIsPlural(int threshold, string expected)
    {
        Assert.That(TierRules.MissedLine(threshold), Is.EqualTo(expected));
    }
}
