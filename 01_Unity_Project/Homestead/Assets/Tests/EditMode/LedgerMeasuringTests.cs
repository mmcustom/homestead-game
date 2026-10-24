using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

// EditMode tests for the Phase D1 measuring: real minutes played per season (recorded, shown, saved, and absent from an old ledger), the
// income per real hour, and the CSV text of the ledger. All pure: no manager, no scene.
public class LedgerMeasuringTests
{
    const int Dps = 28;

    static LedgerState Started(long openingCash = 100000)
    {
        var ledger = new LedgerState();
        ledger.Begin(openingCash, 0, 6 * 60, Dps);
        return ledger;
    }

    static long SeasonEnd(int index) => LedgerState.SeasonEndMinute(index, Dps);

    // --- Real time ---

    [Test]
    public void ARealTimeStartsAtZeroAndAddsUp()
    {
        LedgerState ledger = Started();
        Assert.That(ledger.Open.realMillis, Is.EqualTo(0L));
        ledger.AddRealMillis(1500);
        ledger.AddRealMillis(2500);
        Assert.That(ledger.Open.realMillis, Is.EqualTo(4000L));
    }

    [Test]
    public void ARealTimeIgnoresNothingGoingBackwardsAndAnUnstartedLedger()
    {
        LedgerState ledger = Started();
        ledger.AddRealMillis(0);
        ledger.AddRealMillis(-500);
        Assert.That(ledger.Open.realMillis, Is.EqualTo(0L));

        var blank = new LedgerState();
        blank.AddRealMillis(1000); // not started: nothing to add to, and no crash
        Assert.That(blank.Started, Is.False);
    }

    [Test]
    public void ARealTimeGoesToTheSeasonThatIsOpenAndStaysWithItWhenItCloses()
    {
        LedgerState ledger = Started();
        ledger.AddRealMillis(60000);
        ledger.CloseThrough(1, 0, 0, 0);
        ledger.AddRealMillis(30000);
        Assert.That(ledger.Closed[0].realMillis, Is.EqualTo(60000L));
        Assert.That(ledger.Open.realMillis, Is.EqualTo(30000L));

        var rows = ledger.Rows(0, 0, 0, SeasonEnd(1) - 100);
        Assert.That(rows[0].realMillis, Is.EqualTo(30000L)); // the season in progress, so far
        Assert.That(rows[1].realMillis, Is.EqualTo(60000L));
    }

    [Test]
    public void ATotalsRowAddsTheRealTimeOfTheSeasonsItCovers()
    {
        LedgerState ledger = Started();
        ledger.AddRealMillis(60000);
        ledger.CloseThrough(1, 0, 0, 0);
        ledger.AddRealMillis(90000);
        var rows = ledger.Rows(0, 0, 0, SeasonEnd(1) - 100);
        Assert.That(LedgerState.SinceStart(rows).realMillis, Is.EqualTo(150000L));
    }

    [Test]
    public void ARealTimeSurvivesSaveAndLoad()
    {
        LedgerState ledger = Started();
        ledger.AddRealMillis(754321);
        ledger.CloseThrough(1, 5000, 0, 0);
        ledger.AddRealMillis(12345);

        var loaded = new LedgerState();
        loaded.Restore(JsonUtility.FromJson<LedgerSaveData>(JsonUtility.ToJson(ledger.Capture())));
        Assert.That(loaded.Closed[0].realMillis, Is.EqualTo(754321L));
        Assert.That(loaded.Open.realMillis, Is.EqualTo(12345L));
    }

    [Test]
    public void AnOldLedgerWithoutTheFieldLoadsWithNoRealTime()
    {
        LedgerState ledger = Started();
        ledger.Record(LedgerCategory.SalesFish, 1234);
        ledger.AddRealMillis(99999);
        ledger.CloseThrough(1, 5000, 0, 0);
        string json = JsonUtility.ToJson(ledger.Capture());
        Assert.That(json, Does.Contain("realMillis"));
        string old = Regex.Replace(json, "\"realMillis\":\\d+,?", "");
        Assert.That(old, Does.Not.Contain("realMillis"));

        var loaded = new LedgerState();
        loaded.Restore(JsonUtility.FromJson<LedgerSaveData>(old));
        Assert.That(loaded.Started, Is.True);
        Assert.That(loaded.Closed.Count, Is.EqualTo(1));
        Assert.That(loaded.Closed[0].realMillis, Is.EqualTo(0L));
        Assert.That(loaded.Open.realMillis, Is.EqualTo(0L));
        Assert.That(loaded.Closed[0].cents[(int)LedgerCategory.SalesFish], Is.EqualTo(1234L)); // everything else loads as it was
        Assert.That(loaded.Closed[0].cashEnd, Is.EqualTo(5000L));
    }

    [Test]
    public void AnOldLedgerShowsADashAndStillTakesNewTime()
    {
        LedgerState ledger = Started();
        ledger.Restore(JsonUtility.FromJson<LedgerSaveData>(Regex.Replace(JsonUtility.ToJson(ledger.Capture()), "\"realMillis\":\\d+,?", "")));
        Assert.That(LedgerState.HasRealTime(ledger.Open.realMillis), Is.False);
        Assert.That(LedgerState.RealMinutesText(ledger.Open.realMillis), Is.EqualTo("-"));
        ledger.AddRealMillis(6000);
        Assert.That(LedgerState.RealMinutesText(ledger.Open.realMillis), Is.EqualTo("0.1"));
    }

    // --- Shown ---

    [TestCase(0L, "-")]
    [TestCase(-1L, "-")]
    [TestCase(6000L, "0.1")]
    [TestCase(2999L, "0.0")]
    [TestCase(3000L, "0.1")]       // half a tenth rounds up
    [TestCase(60000L, "1.0")]
    [TestCase(150000L, "2.5")]
    [TestCase(2550000L, "42.5")]
    [TestCase(36000000L, "600.0")]
    public void RealMinutesAreShownToOneDecimal(long millis, string expected)
    {
        Assert.That(LedgerState.RealMinutesText(millis), Is.EqualTo(expected));
    }

    [TestCase(10000L, 3600000L, 10000L)]    // $100 in exactly one real hour
    [TestCase(10000L, 1800000L, 20000L)]    // $100 in half an hour: $200 an hour
    [TestCase(10000L, 7200000L, 5000L)]
    [TestCase(1L, 3600000L, 1L)]
    [TestCase(-5000L, 3600000L, -5000L)]    // a loss per hour is negative
    [TestCase(10000L, 0L, 0L)]              // nothing recorded: 0 (the screen shows a dash)
    [TestCase(10000L, -5L, 0L)]
    [TestCase(0L, 3600000L, 0L)]
    [TestCase(100L, 7200001L, 50L)]
    public void IncomePerRealHourIsCentsOverRealTimeRoundedHalfUp(long cents, long millis, long expected)
    {
        Assert.That(LedgerState.PerRealHour(cents, millis), Is.EqualTo(expected));
    }

    [Test]
    public void IncomePerRealHourRoundsHalfUp()
    {
        // 1 cent over 2 hours is 0.5 cents an hour: half rounds up to 1
        Assert.That(LedgerState.PerRealHour(1, 7200000L), Is.EqualTo(1L));
        Assert.That(LedgerState.PerRealHour(-1, 7200000L), Is.EqualTo(-1L));
    }

    // --- CSV ---

    [TestCase(0L, "0.00")]
    [TestCase(5L, "0.05")]
    [TestCase(7500L, "75.00")]
    [TestCase(123456L, "1234.56")]
    [TestCase(100000000L, "1000000.00")]
    [TestCase(-7500L, "-75.00")]
    [TestCase(-5L, "-0.05")]
    [TestCase(long.MaxValue, "92233720368547758.07")]
    [TestCase(long.MinValue, "-92233720368547758.08")]
    public void CsvMoneyIsDollarsWithTwoDecimalsAndNoComma(long cents, string expected)
    {
        Assert.That(LedgerCsv.Dollars(cents), Is.EqualTo(expected));
    }

    [Test]
    public void TheCsvHeaderNamesEveryColumnOnce()
    {
        string[] columns = SplitCsv(LedgerCsv.Header()); // some category names hold commas, so the header quotes them
        // Season, Days covered, Real minutes, 8 income categories, total income, 11 expense categories, total expenses, Net, Cash, Loan, Owed
        Assert.That(columns.Length, Is.EqualTo(3 + 8 + 1 + 11 + 1 + 4));
        Assert.That(columns[0], Is.EqualTo("Season"));
        Assert.That(columns[1], Is.EqualTo("Days covered"));
        Assert.That(columns[2], Is.EqualTo("Real minutes"));
        Assert.That(columns.Last(), Is.EqualTo("Owed at end"));
        Assert.That(columns.Distinct().Count(), Is.EqualTo(columns.Length));
    }

    [Test]
    public void TheCsvHasOneRowPerSeasonOldestFirst()
    {
        LedgerState ledger = Started(200000);
        ledger.Record(LedgerCategory.SalesFish, 5000);
        ledger.Record(LedgerCategory.BuyTools, 1250);
        ledger.AddRealMillis(2550000);
        ledger.CloseThrough(1, 203750, 4800000, 0);
        ledger.Record(LedgerCategory.SalesHides, 7777);
        var rows = ledger.Rows(211527, 4800000, 0, SeasonEnd(1) + 1440 * 5);

        string[] lines = LedgerCsv.Build(rows).TrimEnd('\n').Split('\n');
        Assert.That(lines.Length, Is.EqualTo(3)); // header and two seasons
        Assert.That(lines[0], Is.EqualTo(LedgerCsv.Header()));
        Assert.That(lines[1], Does.StartWith("\"Spring, Year 1\","));
        Assert.That(lines[2], Does.StartWith("\"Summer, Year 1 (so far)\","));
    }

    [Test]
    public void ACsvRowMatchesTheStatementRowToTheCent()
    {
        LedgerState ledger = Started(200000);
        ledger.Record(LedgerCategory.SalesFish, 5000);
        ledger.Record(LedgerCategory.SalesGameMeat, 3000);
        ledger.Record(LedgerCategory.BuyTools, 1250);
        ledger.Record(LedgerCategory.LoanInterest, 31000);
        ledger.AddRealMillis(2550000);
        ledger.CloseThrough(1, 165750, 4789000, 7500);
        StatementRow row = ledger.Rows(165750, 4789000, 7500, SeasonEnd(1) + 10).Last();

        string[] cells = SplitCsv(LedgerCsv.RowLine(row));
        string[] head = SplitCsv(LedgerCsv.Header());
        Assert.That(cells.Length, Is.EqualTo(head.Length));
        string Cell(string column) => cells[System.Array.IndexOf(head, column)];

        Assert.That(Cell("Season"), Is.EqualTo("Spring, Year 1"));
        Assert.That(Cell("Days covered"), Is.EqualTo("27.75"));
        Assert.That(Cell("Real minutes"), Is.EqualTo("42.5"));
        Assert.That(Cell("Income: Fish"), Is.EqualTo("50.00"));
        Assert.That(Cell("Income: Game meat"), Is.EqualTo("30.00"));
        Assert.That(Cell("Income: Forage"), Is.EqualTo("0.00"));
        Assert.That(Cell("Total income"), Is.EqualTo("80.00"));
        Assert.That(Cell("Expense: Bought: tools"), Is.EqualTo("12.50"));
        Assert.That(Cell("Expense: Land payment: interest"), Is.EqualTo("310.00"));
        Assert.That(Cell("Total expenses"), Is.EqualTo("322.50"));
        Assert.That(Cell("Net"), Is.EqualTo("-242.50"));
        Assert.That(Cell("Cash at end"), Is.EqualTo("1657.50"));
        Assert.That(Cell("Loan balance at end"), Is.EqualTo("47890.00"));
        Assert.That(Cell("Owed at end"), Is.EqualTo("75.00"));
    }

    [Test]
    public void ACsvRealMinutesIsEmptyWhenNothingWasRecorded()
    {
        LedgerState ledger = Started();
        ledger.CloseThrough(1, 100000, 0, 0);
        StatementRow row = ledger.Rows(100000, 0, 0, SeasonEnd(1) + 10).Last();
        string[] cells = SplitCsv(LedgerCsv.RowLine(row));
        Assert.That(cells[2], Is.EqualTo(""));
    }

    [TestCase("plain", "plain")]
    [TestCase("Spring, Year 1", "\"Spring, Year 1\"")]
    [TestCase("say \"hi\"", "\"say \"\"hi\"\"\"")]
    [TestCase("two\nlines", "\"two\nlines\"")]
    [TestCase("", "")]
    public void CsvFieldsAreQuotedOnlyWhenTheyNeedIt(string field, string expected)
    {
        Assert.That(LedgerCsv.Escape(field), Is.EqualTo(expected));
    }

    [Test]
    public void AnEmptyLedgerGivesOnlyTheHeader()
    {
        Assert.That(LedgerCsv.Build(new System.Collections.Generic.List<StatementRow>()), Is.EqualTo(LedgerCsv.Header() + "\n"));
    }

    // Splits one CSV line on commas outside quotes, removing the quotes.
    static string[] SplitCsv(string line)
    {
        var cells = new System.Collections.Generic.List<string>();
        var current = new System.Text.StringBuilder();
        bool quoted = false;
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                if (quoted && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    quoted = !quoted;
                }
            }
            else if (c == ',' && !quoted)
            {
                cells.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }
        cells.Add(current.ToString());
        return cells.ToArray();
    }
}
