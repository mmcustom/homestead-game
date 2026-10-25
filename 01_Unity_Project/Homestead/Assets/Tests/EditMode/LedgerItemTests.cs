using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

// EditMode tests for the item-level sales record: units and cents per item per season next to the category totals, its save, and the
// ledger_items CSV. All pure: no manager, no scene. The category CSV stays as LedgerMeasuringTests pins it.
public class LedgerItemTests
{
    const int Dps = 28;
    const long GameStartMinute = 6 * 60;
    static long SeasonEnd(int index) => LedgerState.SeasonEndMinute(index, Dps);

    static LedgerState Started()
    {
        var ledger = new LedgerState();
        ledger.Begin(100000, 0, GameStartMinute, Dps);
        return ledger;
    }

    static List<StatementRow> Rows(LedgerState ledger, long now) => ledger.Rows(100000, 0, 0, now);

    static LedgerItemSale Item(LedgerSeason season, string id) => season.items.Single(i => i.itemId == id);

    [Test]
    public void UnitsAndCentsAddUpPerItem()
    {
        LedgerState ledger = Started();
        ledger.RecordSale(LedgerCategory.SalesHides, "small_furs", 3, 450);
        ledger.RecordSale(LedgerCategory.SalesHides, "small_furs", 2, 280);
        ledger.RecordSale(LedgerCategory.SalesHides, "deer_hide", 1, 1500);

        Assert.That(ledger.Open.items.Count, Is.EqualTo(2));
        Assert.That((Item(ledger.Open, "small_furs").units, Item(ledger.Open, "small_furs").cents), Is.EqualTo((5, 730L)));
        Assert.That((Item(ledger.Open, "deer_hide").units, Item(ledger.Open, "deer_hide").cents), Is.EqualTo((1, 1500L)));
    }

    [Test]
    public void TheItemsOfASeasonAddUpToTheCategoryTotals()
    {
        LedgerState ledger = Started();
        ledger.RecordSale(LedgerCategory.SalesHides, "small_furs", 3, 450);
        ledger.RecordSale(LedgerCategory.SalesHides, "deer_hide", 1, 1500);
        ledger.RecordSale(LedgerCategory.SalesFish, "bass", 4, 800);
        ledger.RecordSale(LedgerCategory.SalesStockedBack, "axe", 1, 400);

        Assert.That(ledger.Open.items.Sum(i => i.cents), Is.EqualTo(ledger.Open.Income), "every income cent is on an item row");
        Assert.That(ledger.Open.cents[(int)LedgerCategory.SalesHides], Is.EqualTo(1950));
        Assert.That(ledger.Open.cents[(int)LedgerCategory.SalesStockedBack], Is.EqualTo(400), "a sell-back keeps its own category");
        Assert.That(Item(ledger.Open, "axe").cents, Is.EqualTo(400), "and its item row");
    }

    [Test]
    public void ASaleThatIsNotMoneyOrNotStartedIsIgnoredAndAMissingItemStillCountsTheCategory()
    {
        var notStarted = new LedgerState();
        notStarted.RecordSale(LedgerCategory.SalesFish, "bass", 1, 100);
        Assert.That(notStarted.Open, Is.Null);

        LedgerState ledger = Started();
        ledger.RecordSale(LedgerCategory.SalesFish, "bass", 1, 0);
        ledger.RecordSale(LedgerCategory.SalesFish, "bass", 1, -50);
        Assert.That(ledger.Open.items, Is.Empty);
        Assert.That(ledger.Open.Income, Is.EqualTo(0));

        ledger.RecordSale(LedgerCategory.SalesFish, null, 1, 100);
        ledger.RecordSale(LedgerCategory.SalesFish, "bass", 0, 100);
        Assert.That(ledger.Open.items, Is.Empty, "no id or no units: no item row");
        Assert.That(ledger.Open.Income, Is.EqualTo(200), "the category total still takes the money");
    }

    [Test]
    public void ASeasonCloseKeepsTheItemRowsAndTheNewSeasonStartsEmpty()
    {
        LedgerState ledger = Started();
        ledger.RecordSale(LedgerCategory.SalesHides, "small_furs", 3, 450);
        ledger.CloseThrough(1, 100450, 0, 0);
        ledger.RecordSale(LedgerCategory.SalesHides, "small_furs", 1, 160);

        Assert.That(ledger.Closed.Count, Is.EqualTo(1));
        Assert.That(Item(ledger.Closed[0], "small_furs").units, Is.EqualTo(3));
        Assert.That(Item(ledger.Closed[0], "small_furs").cents, Is.EqualTo(450));
        Assert.That(ledger.Open.items.Count, Is.EqualTo(1));
        Assert.That(Item(ledger.Open, "small_furs").cents, Is.EqualTo(160), "the next season counts on its own");

        ledger.CloseThrough(4, 100610, 0, 0); // a skip over several seasons: the empty ones stay empty
        Assert.That(ledger.Closed.Count, Is.EqualTo(4));
        Assert.That(ledger.Closed[1].items.Count, Is.EqualTo(1));
        Assert.That(ledger.Closed[2].items, Is.Empty);
    }

    [Test]
    public void TheItemsGoThroughASaveAndBack()
    {
        LedgerState ledger = Started();
        ledger.RecordSale(LedgerCategory.SalesHides, "small_furs", 3, 450);
        ledger.CloseThrough(1, 100450, 0, 0);
        ledger.RecordSale(LedgerCategory.SalesFish, "bass", 2, 300);

        LedgerSaveData loaded = JsonUtility.FromJson<LedgerSaveData>(JsonUtility.ToJson(ledger.Capture()));
        var back = new LedgerState();
        back.Restore(loaded);

        Assert.That(back.Closed[0].items.Count, Is.EqualTo(1));
        Assert.That((Item(back.Closed[0], "small_furs").units, Item(back.Closed[0], "small_furs").cents), Is.EqualTo((3, 450L)));
        Assert.That((Item(back.Open, "bass").units, Item(back.Open, "bass").cents), Is.EqualTo((2, 300L)));
    }

    [Test]
    public void AnOldSaveWithNoItemsLoadsWithAnEmptyListAndNoErrorsAndTheVersionIsUnchanged()
    {
        Assert.That(LedgerSaveData.CurrentVersion, Is.EqualTo(1), "a bump would make Restore drop every existing ledger");

        // A save written before items existed: seasons carry only their old fields.
        string oldJson = "{\"version\":1,\"daysPerSeason\":28,\"started\":true,\"unread\":false,"
            + "\"open\":{\"index\":1,\"startMinute\":40320,\"endMinute\":0,\"cashStart\":5000,\"cashEnd\":0,\"loanEnd\":0,\"owedEnd\":0,\"realMillis\":0,"
            + "\"cents\":[0,0,0,1200,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]},"
            + "\"closed\":[{\"index\":0,\"startMinute\":360,\"endMinute\":40320,\"cashStart\":9000,\"cashEnd\":5000,\"loanEnd\":0,\"owedEnd\":0,\"realMillis\":0,"
            + "\"cents\":[0,0,0,800,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]}]}";

        var ledger = new LedgerState();
        Assert.DoesNotThrow(() => ledger.Restore(JsonUtility.FromJson<LedgerSaveData>(oldJson)));

        Assert.That(ledger.Started, Is.True);
        Assert.That(ledger.Closed[0].items, Is.Not.Null.And.Empty);
        Assert.That(ledger.Open.items, Is.Not.Null.And.Empty);
        Assert.That(ledger.Closed[0].cents[(int)LedgerCategory.SalesHides], Is.EqualTo(800), "the category totals are untouched");
        Assert.DoesNotThrow(() => ledger.RecordSale(LedgerCategory.SalesHides, "small_furs", 1, 100), "and it can start recording items");
        Assert.That(ledger.Open.items.Count, Is.EqualTo(1));
    }

    [Test]
    public void TheItemCsvHasTheRightRowsOldestSeasonFirstAndItemsAToZ()
    {
        LedgerState ledger = Started();
        ledger.RecordSale(LedgerCategory.SalesHides, "small_furs", 3, 450);
        ledger.RecordSale(LedgerCategory.SalesHides, "deer_hide", 1, 1500);
        ledger.RecordSale(LedgerCategory.SalesHides, "small_furs", 4, 700);
        ledger.CloseThrough(1, 102650, 0, 0);
        ledger.RecordSale(LedgerCategory.SalesFish, "bass", 3, 100);

        string csv = LedgerCsv.BuildItems(Rows(ledger, SeasonEnd(1) + 600));
        string[] lines = csv.Split('\n');

        Assert.That(lines[0], Is.EqualTo("Season,Item,Units,Dollars,Average price"));
        Assert.That(lines[1], Is.EqualTo("\"Spring, Year 1\",deer_hide,1,15.00,15.00"));
        Assert.That(lines[2], Is.EqualTo("\"Spring, Year 1\",small_furs,7,11.50,1.64"), "1150 cents over 7 units, half up");
        Assert.That(lines[3], Is.EqualTo("\"Summer, Year 1 (so far)\",bass,3,1.00,0.33"));
        Assert.That(lines[4], Is.EqualTo(""), "the file ends in a newline");
        Assert.That(lines.Length, Is.EqualTo(5));
    }

    [Test]
    public void ASeasonWithNoSalesHasNoRowsAndNoSalesAtAllIsJustTheHeader()
    {
        LedgerState ledger = Started();
        Assert.That(LedgerCsv.BuildItems(Rows(ledger, GameStartMinute + 600)), Is.EqualTo(LedgerCsv.ItemsHeader() + "\n"));

        ledger.CloseThrough(1, 100000, 0, 0);
        ledger.RecordSale(LedgerCategory.SalesFish, "bass", 1, 250);
        string[] lines = LedgerCsv.BuildItems(Rows(ledger, SeasonEnd(1) + 600)).Split('\n');
        Assert.That(lines.Length, Is.EqualTo(3), "header, one row, and the trailing newline: the empty Spring has none");
        Assert.That(lines[1], Does.StartWith("\"Summer, Year 1 (so far)\",bass,1,2.50,2.50"));
    }

    [Test]
    public void TheItemRowsMatchTheCategoryCsvTotalsForEachSeason()
    {
        LedgerState ledger = Started();
        ledger.RecordSale(LedgerCategory.SalesHides, "small_furs", 3, 450);
        ledger.RecordSale(LedgerCategory.SalesGameMeat, "small_game_meat", 6, 1230);
        ledger.CloseThrough(1, 101680, 0, 0);

        StatementRow spring = Rows(ledger, SeasonEnd(1) + 600).Last();
        Assert.That(spring.items.Sum(i => i.cents), Is.EqualTo(spring.income));
        Assert.That(LedgerCsv.RowLine(spring), Does.Contain(LedgerCsv.Dollars(spring.income)));
    }

    [Test]
    public void TheCategoryCsvKeepsItsExactShape()
    {
        LedgerState ledger = Started();
        ledger.RecordSale(LedgerCategory.SalesHides, "small_furs", 3, 450);
        string header = LedgerCsv.Header();
        Assert.That(header, Does.EndWith("Total expenses,Net,Cash at end,Loan balance at end,Owed at end"));
        Assert.That(header, Does.StartWith("Season,Days covered,Real minutes,\"Income: Firewood, logs and stone\",Income: Fish"));
        Assert.That(header, Does.Not.Contain("Item"));
    }
}
