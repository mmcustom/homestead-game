using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

// EditMode tests for the one-off conversion of a save from the old calendar onto the real one (Phase 1): CalendarConversion maps days, ages, regrowth, the loan and the ledger. The sample
// JSON is copied from a real old-calendar save (Pioneer, day 38, Summer 11, Year 1).
public class CalendarConversionTests
{
    static readonly GameCalendar Real = CalendarRules.Real;
    static readonly GameCalendar Legacy = CalendarRules.Legacy;

    const int OldToday = 38;
    const int Hour = 20;

    // --- Days ---

    [TestCase(0, 0)]
    [TestCase(1, 3)]
    [TestCase(27, 88)]
    [TestCase(28, 92)]
    [TestCase(38, 124)]
    [TestCase(55, 180)]
    [TestCase(56, 184)]
    [TestCase(83, 271)]
    [TestCase(84, 275)]
    [TestCase(111, 361)]
    [TestCase(112, 365)]
    [TestCase(140, 457)]
    [TestCase(224, 730)]
    [TestCase(-9, 0)]
    public void ADayKeepsItsSeasonAndItsFractionOfTheSeason(int oldDay, int newDay)
    {
        Assert.That(CalendarConversion.MapDay(oldDay), Is.EqualTo(newDay));
    }

    [Test]
    public void EveryMappedDayIsInTheSameSeasonAndYearAndTheOrderIsKept()
    {
        int previous = -1;
        for (int day = 0; day < 112 * 6; day++)
        {
            int mapped = CalendarConversion.MapDay(day);
            Assert.That(mapped, Is.GreaterThan(previous), "strictly increasing, day " + day);
            Assert.That(Real.SeasonIndexOf(mapped), Is.EqualTo(Legacy.SeasonIndexOf(day)), "same season, day " + day);
            Assert.That(Real.YearOfDay(mapped), Is.EqualTo(Legacy.YearOfDay(day)), "same year, day " + day);
            previous = mapped;
        }
    }

    [Test]
    public void ASeasonStartMapsToTheNewSeasonStart()
    {
        for (int season = 0; season < 12; season++)
            Assert.That(CalendarConversion.MapDay(Legacy.SeasonStartDay(season)), Is.EqualTo(Real.SeasonStartDay(season)), "season " + season);
    }

    [Test]
    public void MinutesFollowTheirDay()
    {
        Assert.That(CalendarConversion.MapMinute(360), Is.EqualTo(360L), "6:00 on day 0");
        Assert.That(CalendarConversion.MapMinute(40320), Is.EqualTo(92L * 1440), "the end of the old Spring is the start of Summer");
        Assert.That(CalendarConversion.MapMinute(38L * 1440 + 1241), Is.EqualTo(124L * 1440 + 1241), "the time of day is kept");
        Assert.That(CalendarConversion.MapMinute(-5), Is.EqualTo(0L));
    }

    // --- Ages, regrowth, hours ---

    [Test]
    public void AStampThatMeansWhenItHappenedKeepsItsAge()
    {
        int newToday = CalendarConversion.MapDay(OldToday);
        Assert.That(CalendarConversion.KeepAge(0, OldToday, newToday), Is.EqualTo(0), "0 is no stamp and stays 0");
        Assert.That(CalendarConversion.KeepAge(37, OldToday, newToday), Is.EqualTo(newToday - 1), "one day old");
        Assert.That(CalendarConversion.KeepAge(OldToday, OldToday, newToday), Is.EqualTo(newToday), "acquired today");
        Assert.That(CalendarConversion.KeepAge(8, OldToday, newToday), Is.EqualTo(newToday - 30), "thirty days old");
        Assert.That(CalendarConversion.KeepAge(1, 1000, 5), Is.EqualTo(1), "never below 1, which would read as no stamp");
    }

    [Test]
    public void AFelledTreeIsTheSameFractionOfTheWayBackToFullSize()
    {
        const int oldToday = 300; // late enough in the game that a year-old stamp is still positive
        int newToday = CalendarConversion.MapDay(oldToday);
        Assert.That(CalendarConversion.ScaleRegrowStamp(0, oldToday, newToday), Is.EqualTo(0), "0: a save from before regrowth");
        // Felled today (stamp = day + 1): still brand new.
        int fresh = CalendarConversion.ScaleRegrowStamp(oldToday + 1, oldToday, newToday);
        Assert.That(RegrowRules.DaysSince(fresh, newToday), Is.EqualTo(0));
        // Felled 56 days ago: half the old year, so about half the new one.
        int half = CalendarConversion.ScaleRegrowStamp(oldToday - 56 + 1, oldToday, newToday);
        Assert.That(RegrowRules.DaysSince(half, newToday), Is.EqualTo(183).Within(1), "56 of 112 days is about 183 of 365");
        // Felled ten days ago.
        int ten = CalendarConversion.ScaleRegrowStamp(oldToday - 10 + 1, oldToday, newToday);
        Assert.That(RegrowRules.DaysSince(ten, newToday), Is.EqualTo(33).Within(1));
        Assert.That(RegrowRules.Stage(RegrowRules.DaysSince(ten, newToday)), Is.EqualTo(1), "a sprout");
        // A whole old year ago is a whole new year ago: fully grown.
        int full = CalendarConversion.ScaleRegrowStamp(oldToday - 112 + 1, oldToday, newToday);
        Assert.That(RegrowRules.Stage(RegrowRules.DaysSince(full, newToday)), Is.EqualTo(RegrowRules.FullStage));
        // A stamp that would land before day 1 is held at 1, which is not the "no stamp" value.
        Assert.That(CalendarConversion.ScaleRegrowStamp(2, 1000, 5), Is.EqualTo(1));
    }

    [Test]
    public void ADryPieceKeepsTheHoursStillToGo()
    {
        int oldNow = OldToday * 24 + Hour, newNow = CalendarConversion.MapDay(OldToday) * 24 + Hour;
        Assert.That(CalendarConversion.KeepHoursToGo(oldNow + 30, oldNow, newNow), Is.EqualTo(newNow + 30));
        Assert.That(CalendarConversion.KeepHoursToGo(oldNow - 5, oldNow, newNow), Is.EqualTo(newNow), "already dry: dry now");
        Assert.That(CalendarConversion.KeepHoursToGo(0, oldNow, newNow), Is.EqualTo(0), "no stamp");
    }

    // --- Sections (copied from a real old-calendar save) ---

    static string Convert(string file, string key, string json, List<string> log = null) =>
        CalendarConversion.ConvertSection(file, key, json, OldToday, Hour, log);

    [Test]
    public void TheClockMovesToTheMappedDayAndKeepsTheTimeOfDay()
    {
        string result = Convert("world", "time", "{\"totalDays\":38,\"minuteOfDay\":1241.863525390625}");
        Assert.That(result, Is.EqualTo("{\"totalDays\":124,\"minuteOfDay\":1241.863525390625}"));
        Assert.That(Real.FormatDate(124), Is.EqualTo("Jul 3, Year 1"));
    }

    const string OldLoan = "{\"version\":1,\"hasLoan\":true,\"loan\":{\"acres\":5,\"priceCents\":6000000,\"downCents\":0,\"originalCents\":6000000,\"paymentCents\":57339,\"balanceCents\":6000000,\"nextDueDay\":112,\"paymentsDue\":0,\"paymentsMade\":0,\"interestPaid\":0,\"feesPaid\":0,\"principalPaid\":0,\"extraPaid\":0,\"soonNoticeDue\":-1,\"terms\":{\"rateBasisPoints\":800,\"payments\":180,\"intervalDays\":28,\"firstDueDay\":112,\"graceDays\":10,\"lateFeeCents\":7500},\"open\":[]}}";

    [Test]
    public void TheLoanMovesToTheMonthlyRuleKeepingItsFirstPaymentAtTheEndOfTheFirstYear()
    {
        var log = new List<string>();
        LoanState loan = JsonUtility.FromJson<LoanSaveData>(Convert("player", "loan", OldLoan, log)).ToLoan();

        Assert.That(loan.nextDueDay, Is.EqualTo(365), "day 112, the end of the first year, is March 1 of Year 2");
        Assert.That(Real.FormatDate(loan.nextDueDay), Is.EqualTo("Mar 1, Year 2"));
        Assert.That(loan.terms.dueDayOfMonth, Is.EqualTo(1));
        Assert.That(loan.terms.intervalDays, Is.EqualTo(0));
        Assert.That(loan.terms.firstDueDay, Is.EqualTo(365));
        Assert.That(loan.balanceCents, Is.EqualTo(6000000L));
        Assert.That(loan.paymentCents, Is.EqualTo(57339L), "the amount is unchanged");
        Assert.That(loan.paymentsDue, Is.EqualTo(0));
        Assert.That(loan.terms.rateBasisPoints, Is.EqualTo(800));
        Assert.That(loan.terms.lateFeeCents, Is.EqualTo(7500L));
        Assert.That(log.Any(l => l.Contains("monthly")), Is.True);

        loan.Advance(124);
        Assert.That(loan.open, Is.Empty, "nothing is due on the day it lands");
        loan.Advance(365);
        Assert.That(loan.open.Select(p => p.dueDay), Is.EqualTo(new[] { 365 }));
        Assert.That(loan.nextDueDay, Is.EqualTo(396), "then April 1, the 1st of every month");
    }

    [Test]
    public void AnOpenPaymentKeepsItsStateAndItsDueDayIsMapped()
    {
        const string json = "{\"version\":1,\"hasLoan\":true,\"loan\":{\"acres\":5,\"priceCents\":6000000,\"downCents\":0,\"originalCents\":6000000,\"paymentCents\":57339,\"balanceCents\":6000000,\"nextDueDay\":140,\"paymentsDue\":1,\"paymentsMade\":0,\"interestPaid\":0,\"feesPaid\":0,\"principalPaid\":0,\"extraPaid\":0,\"soonNoticeDue\":112,\"terms\":{\"rateBasisPoints\":800,\"payments\":180,\"intervalDays\":28,\"firstDueDay\":112,\"graceDays\":10,\"lateFeeCents\":7500},\"open\":[{\"number\":1,\"dueDay\":112,\"fee\":7500,\"interest\":40000,\"principal\":17339,\"missed\":true,\"stage\":3}]}}";
        LoanState loan = JsonUtility.FromJson<LoanSaveData>(CalendarConversion.ConvertSection("player", "loan", json, 130, 8, null)).ToLoan();
        Assert.That(loan.open.Single().dueDay, Is.EqualTo(365));
        Assert.That(loan.open.Single().missed, Is.True);
        Assert.That(loan.open.Single().fee, Is.EqualTo(7500L));
        Assert.That(loan.open.Single().Owed, Is.EqualTo(64839L));
        Assert.That(loan.nextDueDay, Is.EqualTo(457), "old day 140 is the start of Summer, Year 2: June 1");
        Assert.That(Real.DayOfMonth(loan.nextDueDay), Is.EqualTo(1));
        Assert.That(loan.soonNoticeDue, Is.EqualTo(-1), "the due-soon warning starts over");
    }

    const string OldLedger = "{\"version\":1,\"daysPerSeason\":28,\"started\":true,\"unread\":false,\"open\":{\"index\":1,\"startMinute\":40320,\"endMinute\":0,\"cashStart\":7509,\"cashEnd\":0,\"loanEnd\":0,\"owedEnd\":0,\"realMillis\":11228142,\"cents\":[2608,0,200,3974,0,4573,0,0,0,0,0,0,0,3000,2000,0,0,0,1600],\"items\":[]},\"closed\":[{\"index\":0,\"startMinute\":360,\"endMinute\":40320,\"cashStart\":0,\"cashEnd\":7509,\"loanEnd\":6000000,\"owedEnd\":0,\"realMillis\":15175737,\"cents\":[7670,0,0,12003,0,2036,1000,0,0,0,0,0,0,0,9200,1500,4500,0,0],\"items\":[]}]}";

    [Test]
    public void TheLedgerKeepsItsSeasonsAndTheirTotalsAndMovesToTheRealCalendar()
    {
        LedgerSaveData data = JsonUtility.FromJson<LedgerSaveData>(Convert("player", "ledger", OldLedger));
        Assert.That(data.daysPerSeason, Is.EqualTo(0), "the game's real calendar");
        Assert.That(data.open.index, Is.EqualTo(1), "still Summer, Year 1");
        Assert.That(data.open.startMinute, Is.EqualTo(92L * 1440), "Summer begins on June 1");
        Assert.That(data.closed[0].startMinute, Is.EqualTo(360L));
        Assert.That(data.closed[0].endMinute, Is.EqualTo(92L * 1440), "Spring was closed when Summer began");
        Assert.That(data.open.cents[0], Is.EqualTo(2608L), "the money is untouched");
        Assert.That(data.closed[0].cashEnd, Is.EqualTo(7509L));
        Assert.That(data.open.realMillis, Is.EqualTo(11228142L));

        var ledger = new LedgerState();
        ledger.Restore(data);
        Assert.That(ledger.Calendar, Is.SameAs(Real));
        Assert.That(ledger.OpenIndex, Is.EqualTo(Real.SeasonIndexOf(CalendarConversion.MapDay(OldToday))), "the open season is the one the mapped day is in");
        ledger.CloseThrough(2, 100000, 6000000, 0);
        Assert.That(ledger.Closed.Last().endMinute, Is.EqualTo(184L * 1440), "Summer ends where Fall begins: 92 days, not 28");
    }

    [Test]
    public void AgesAreKeptOnItemsDroppedThingsGroundWoodAndGrass()
    {
        // Mapped today is 124: ages are kept, so "28 days ago" is day 96, not day 28 of the new calendar.
        string inv = Convert("inventory", "inventory", "{\"player\":{\"id\":\"player\",\"stacks\":[{\"itemId\":\"knife\",\"quantity\":1,\"acquiredDay\":0},{\"itemId\":\"rifle_rounds\",\"quantity\":5,\"acquiredDay\":28},{\"itemId\":\"axe\",\"quantity\":1,\"acquiredDay\":37}]}}");
        Assert.That(inv, Does.Contain("\"acquiredDay\":0}"));
        Assert.That(inv, Does.Contain("\"acquiredDay\":114}"), "ten days old stays ten days old");
        Assert.That(inv, Does.Contain("\"acquiredDay\":123}"));

        Assert.That(Convert("world", "dropped", "{\"items\":[{\"itemId\":\"sticks\",\"quantity\":6,\"acquiredDay\":34}]}"), Does.Contain("\"acquiredDay\":120}"));
        Assert.That(Convert("world", "groundwood", "{\"ids\":[1,2,3],\"takenDays\":[35,33,36]}"), Is.EqualTo("{\"ids\":[1,2,3],\"takenDays\":[121,119,122]}"));
        Assert.That(Convert("world", "grass", "{\"ids\":[51],\"cutDays\":[37]}"), Is.EqualTo("{\"ids\":[51],\"cutDays\":[123]}"));
        Assert.That(Convert("world", "grass", "{\"ids\":[],\"cutDays\":[]}"), Is.EqualTo("{\"ids\":[],\"cutDays\":[]}"));
        Assert.That(Convert("world", "forage", "{\"patches\":[{\"nodeId\":\"a\",\"year\":1,\"remaining\":0,\"depletedDay\":17,\"season1\":1}],\"seed\":25889109}"),
            Is.EqualTo("{\"patches\":[{\"nodeId\":\"a\",\"year\":1,\"remaining\":0,\"depletedDay\":103,\"season1\":1}],\"seed\":25889109}"), "the year and season of the patch stay");
    }

    [Test]
    public void WoodRegrowthWoodBatchesAndDryPotteryAreConverted()
    {
        const string wood = "{\"felled\":[{\"index\":5326,\"stump\":false,\"fellStamp\":0},{\"index\":5548,\"stump\":true,\"fellStamp\":29}],\"piles\":[{\"id\":3,\"kind\":1,\"contents\":[{\"itemId\":\"sticks\",\"count\":725,\"day\":38},{\"itemId\":\"logs\",\"count\":3,\"day\":33},{\"itemId\":\"stone\",\"count\":1,\"day\":0},{\"itemId\":\"dry_clay_cup\",\"count\":2,\"day\":930}],\"builtDay\":30},{\"id\":4,\"kind\":2,\"contents\":[],\"builtDay\":0}],\"nextPileId\":60}";
        var log = new List<string>();
        string result = Convert("world", "wood", wood, log);

        Assert.That(result, Does.Contain("\"fellStamp\":0}"), "a tree with no stamp keeps none");
        // Felled on day 28 (stamp 29), ten days ago: scaled to about 33 days old, on day 124 -> stamp 124 - 33 + 1 = 92.
        int stamp = int.Parse(System.Text.RegularExpressions.Regex.Match(result, "\"fellStamp\":(\\d+)\\}\\]").Groups[1].Value);
        Assert.That(124 - (stamp - 1), Is.EqualTo(33).Within(1));
        Assert.That(result, Does.Contain("{\"itemId\":\"sticks\",\"count\":725,\"day\":124}"), "wet today stays wet today");
        Assert.That(result, Does.Contain("{\"itemId\":\"logs\",\"count\":3,\"day\":119}"), "five days wet stays five days wet");
        Assert.That(result, Does.Contain("{\"itemId\":\"stone\",\"count\":1,\"day\":0}"), "no stamp stays none");
        // A dry cup ready at hour 930 (38 * 24 = 912, hour 20: now is 932, so it was ready 2 hours ago): ready now.
        Assert.That(result, Does.Contain("{\"itemId\":\"dry_clay_cup\",\"count\":2,\"day\":2996}"), "124 * 24 + 20 = 2996");
        Assert.That(result, Does.Contain("\"builtDay\":116}"), "built eight days ago stays eight days ago");
        Assert.That(result, Does.Contain("\"builtDay\":0}"));
        Assert.That(result, Does.Contain("\"nextPileId\":60"));
        Assert.That(log.Any(l => l.Contains("regrowth")), Is.True);
        Assert.That(log.Any(l => l.Contains("wood batches")), Is.True);
    }

    [Test]
    public void ADryPieceStillDryingKeepsItsHoursToGo()
    {
        // Now is hour 38 * 24 + 20 = 932; ready at 950 is 18 hours to go; on day 124 it is hour 2996 + 18.
        string result = Convert("world", "wood", "{\"piles\":[{\"contents\":[{\"itemId\":\"dry_clay_bowl\",\"count\":1,\"day\":950}]}]}");
        Assert.That(result, Does.Contain("\"day\":3014}"));
    }

    [Test]
    public void JournalAndDiscoveryDatesAreMappedLikeTheClock()
    {
        Assert.That(Convert("journal", "journal", "{\"entries\":[{\"day\":28,\"text\":\"a\"},{\"day\":0,\"text\":\"b\"}]}"),
            Is.EqualTo("{\"entries\":[{\"day\":92,\"text\":\"a\"},{\"day\":0,\"text\":\"b\"}]}"));
        string discovery = Convert("discovery", "discovery", "{\"records\":[{\"day\":5,\"x\":1},{\"dayDiscovered\":7,\"y\":2}]}");
        Assert.That(discovery, Does.Contain("\"day\":" + CalendarConversion.MapDay(5)));
        Assert.That(discovery, Does.Contain("\"dayDiscovered\":" + CalendarConversion.MapDay(7)));
        Assert.That(Convert("player", "market", "{\"version\":1,\"lastDay\":38,\"pools\":[]}"), Is.EqualTo("{\"version\":1,\"lastDay\":124,\"pools\":[]}"));
        Assert.That(Convert("player", "survival", "{\"health\":100.0,\"collapseClockDay\":30}"), Is.EqualTo("{\"health\":100.0,\"collapseClockDay\":116}"));
    }

    [Test]
    public void SectionsWithNothingToConvertComeBackUnchanged()
    {
        const string weather = "{\"type\":1,\"hoursRemaining\":6,\"temperatureC\":29.3,\"dryDays\":3,\"rainedToday\":true,\"seed\":25888218}";
        Assert.That(Convert("world", "weather", weather), Is.EqualTo(weather));
        const string fires = "{\"fires\":[{\"id\":1,\"fuelHours\":2.38,\"lit\":false}]}";
        Assert.That(Convert("world", "fires", fires), Is.EqualTo(fires), "hours are durations, not dates");
        const string taps = "{\"taps\":[{\"id\":1,\"litres\":0.0,\"ageHours\":0.0}]}";
        Assert.That(Convert("world", "taps", taps), Is.EqualTo(taps));
        Assert.That(Convert("player", "setup", "{\"difficulty\":2}"), Is.EqualTo("{\"difficulty\":2}"));
    }

    [Test]
    public void TheLogSaysWhatChangedSectionBySection()
    {
        var log = new List<string>();
        Convert("inventory", "inventory", "{\"player\":{\"stacks\":[{\"itemId\":\"axe\",\"quantity\":1,\"acquiredDay\":37}]}}", log);
        Convert("player", "market", "{\"lastDay\":38}", log);
        Assert.That(log, Has.Count.EqualTo(2));
        Assert.That(log[0], Does.StartWith("inventory/inventory:"));
        Assert.That(log[1], Does.StartWith("player/market:"));
    }
}
