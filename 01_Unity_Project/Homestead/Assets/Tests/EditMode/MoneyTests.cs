using System;
using NUnit.Framework;
using UnityEngine;

// EditMode tests for Money: whole dollars as cents, formatting, the Trading Post's unchanged sell-back rule, buying a batch, and the
// save format. No files are touched.
public class MoneyTests
{
    // The Trading Post's stock prices in dollars, and the rule as it was before cents (a float 40%, rounded down, min $1, none under $3).
    static readonly int[] StockDollars = { 8, 4, 5, 6, 12, 8, 15, 25, 20, 10, 12, 30, 1, 60, 1, 2, 3, 8, 25 };
    static long OldSellDollars(int stocked) => stocked >= 3 ? Mathf.Max(1, Mathf.FloorToInt(stocked * 0.4f)) : 0;

    [Test]
    public void DollarsBecomeCents()
    {
        Assert.That(Money.Dollars(0), Is.EqualTo(0L));
        Assert.That(Money.Dollars(1), Is.EqualTo(100L));
        Assert.That(Money.Dollars(50), Is.EqualTo(5000L));
        Assert.That(Money.Dollars(-75), Is.EqualTo(-7500L));
    }

    [Test]
    public void FormatsWithSeparatorsTwoDecimalsAndASignInFront()
    {
        Assert.That(Money.Format(0), Is.EqualTo("$0.00"));
        Assert.That(Money.Format(1200), Is.EqualTo("$12.00"));
        Assert.That(Money.Format(5), Is.EqualTo("$0.05"));
        Assert.That(Money.Format(60), Is.EqualTo("$0.60"));
        Assert.That(Money.Format(123450), Is.EqualTo("$1,234.50"));
        Assert.That(Money.Format(100000000), Is.EqualTo("$1,000,000.00"));
        Assert.That(Money.Format(-7500), Is.EqualTo("-$75.00"));
        Assert.That(Money.Format(-5), Is.EqualTo("-$0.05"));
        Assert.That(Money.Format(-123450), Is.EqualTo("-$1,234.50"));
    }

    [Test]
    public void FormatIgnoresTheMachinesCulture()
    {
        var saved = System.Threading.Thread.CurrentThread.CurrentCulture;
        try
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("de-DE");
            Assert.That(Money.Format(123450), Is.EqualTo("$1,234.50"));
        }
        finally
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = saved;
        }
    }

    [Test]
    public void SellBackIsTheSameAsBeforeForEveryStockedItem()
    {
        foreach (int dollars in StockDollars)
            Assert.That(Money.SellBackCents(Money.Dollars(dollars)), Is.EqualTo(Money.Dollars(OldSellDollars(dollars))), $"${dollars} item");
        for (int dollars = 0; dollars <= 500; dollars++)
            Assert.That(Money.SellBackCents(Money.Dollars(dollars)), Is.EqualTo(Money.Dollars(OldSellDollars(dollars))), $"${dollars}");
    }

    [Test]
    public void ItemsStockedUnderThreeDollarsAreNeverBoughtBack()
    {
        Assert.That(Money.SellBackCents(Money.Dollars(1)), Is.EqualTo(0L), "arrows, rifle rounds");
        Assert.That(Money.SellBackCents(Money.Dollars(2)), Is.EqualTo(0L), "cordage");
        Assert.That(Money.SellBackCents(299), Is.EqualTo(0L));
        Assert.That(Money.SellBackCents(Money.Dollars(3)), Is.EqualTo(Money.Dollars(1)), "the $1 minimum");
    }

    [Test]
    public void SpecificSellBacksMatchTheDesignNumbers()
    {
        Assert.That(Money.SellBackCents(Money.Dollars(8)), Is.EqualTo(300L), "Flint and Steel, Tarp, Lumber");
        Assert.That(Money.SellBackCents(Money.Dollars(25)), Is.EqualTo(1000L), "Tent");
        Assert.That(Money.SellBackCents(Money.Dollars(60)), Is.EqualTo(2400L), "Rifle");
        Assert.That(Money.SellBackCents(Money.Dollars(4)), Is.EqualTo(100L), "Knife: 40% of $4 is $1.60, rounded down");
    }

    [Test]
    public void BuyingTenWithShiftIsHeldBackByMoneyAndByRoom()
    {
        Assert.That(Money.BuyCount(Money.Dollars(100), Money.Dollars(8), 10, 99), Is.EqualTo(10), "$100 buys 10 at $8");
        Assert.That(Money.Sub(Money.Dollars(100), Money.Mul(Money.Dollars(8), 10)), Is.EqualTo(Money.Dollars(20)), "$20 left");
        Assert.That(Money.BuyCount(Money.Dollars(50), Money.Dollars(8), 10, 99), Is.EqualTo(6), "money runs out first");
        Assert.That(Money.BuyCount(Money.Dollars(500), Money.Dollars(1), 10, 4), Is.EqualTo(4), "room runs out first");
        Assert.That(Money.BuyCount(799, 800, 10, 99), Is.EqualTo(0), "one cent short");
        Assert.That(Money.BuyCount(800, 800, 10, 99), Is.EqualTo(1));
        Assert.That(Money.BuyCount(0, 100, 10, 99), Is.EqualTo(0));
        Assert.That(Money.BuyCount(-500, 100, 10, 99), Is.EqualTo(0));
        Assert.That(Money.BuyCount(1000, 0, 10, 99), Is.EqualTo(0), "not for sale");
    }

    [Test]
    public void AnOldSaveWithWholeDollarsLoadsAsCents()
    {
        var old = JsonUtility.FromJson<TradingPostSaveData>("{\"money\":50}");
        Assert.That(old.version, Is.EqualTo(0));
        Assert.That(old.ToCents(), Is.EqualTo(5000L));
        Assert.That(JsonUtility.FromJson<TradingPostSaveData>("{\"money\":0}").ToCents(), Is.EqualTo(0L));
        Assert.That(JsonUtility.FromJson<TradingPostSaveData>("{\"money\":123456}").ToCents(), Is.EqualTo(12345600L));
        Assert.That(JsonUtility.FromJson<TradingPostSaveData>("{}").ToCents(), Is.EqualTo(0L), "a save with no money at all");
    }

    [Test]
    public void ANewSaveRoundTripsExactly()
    {
        foreach (long cents in new long[] { 0, 1, 99, 5000, 123450, 99999999999L, long.MaxValue })
        {
            string json = JsonUtility.ToJson(TradingPostSaveData.FromCents(cents));
            var back = JsonUtility.FromJson<TradingPostSaveData>(json);
            Assert.That(back.version, Is.EqualTo(TradingPostSaveData.CentsVersion));
            Assert.That(back.ToCents(), Is.EqualTo(cents), json);
        }
    }

    [Test]
    public void TheNewFormatIgnoresTheOldDollarField()
    {
        var data = JsonUtility.FromJson<TradingPostSaveData>("{\"version\":1,\"money\":7,\"cents\":1234}");
        Assert.That(data.ToCents(), Is.EqualTo(1234L));
    }

    [Test]
    public void BigValuesDoNotOverflow()
    {
        Assert.That(Money.Add(long.MaxValue, 1), Is.EqualTo(long.MaxValue));
        Assert.That(Money.Add(long.MinValue, -1), Is.EqualTo(long.MinValue));
        Assert.That(Money.Sub(long.MinValue, 1), Is.EqualTo(long.MinValue));
        Assert.That(Money.Sub(0, long.MinValue), Is.EqualTo(long.MaxValue));
        Assert.That(Money.Mul(long.MaxValue, 2), Is.EqualTo(long.MaxValue));
        Assert.That(Money.Mul(long.MaxValue, -2), Is.EqualTo(long.MinValue));
        Assert.That(Money.Dollars(long.MaxValue), Is.EqualTo(long.MaxValue));
        Assert.That(Money.Dollars(10000000000L), Is.EqualTo(1000000000000L), "ten billion dollars");
        Assert.That(Money.Format(long.MaxValue), Is.EqualTo("$92,233,720,368,547,758.07"));
        Assert.That(Money.Format(long.MinValue), Is.EqualTo("-$92,233,720,368,547,758.08"));
        Assert.That(Money.BuyCount(long.MaxValue, 100, 10, 99), Is.EqualTo(10));
        Assert.That(TradingPostSaveData.FromCents(long.MaxValue).money, Is.EqualTo(int.MaxValue), "the old field is clamped");
        Assert.That(TradingPostSaveData.FromCents(long.MinValue).money, Is.EqualTo(int.MinValue));
    }
}
