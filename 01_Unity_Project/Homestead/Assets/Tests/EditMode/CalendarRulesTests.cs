using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using NUnit.Framework;

// EditMode tests for the calendar's one source (real-calendar Phase 0): GameCalendar / CalendarRules, and the guard that every system which used to repeat the season length
// (28 days), the year (112) or the loan's first due day and spacing now reads it. Phase 0 changes no behaviour: Real and Legacy both hold today's numbers.
public class CalendarRulesTests
{
    static readonly GameCalendar Today = CalendarRules.Current;

    // --- The calendar object ---

    [Test]
    public void TheRealAndLegacyCalendarsHoldTodaysNumbers()
    {
        foreach (GameCalendar calendar in new[] { CalendarRules.Real, CalendarRules.Legacy })
        {
            Assert.That(calendar.DaysPerSeason, Is.EqualTo(28));
            Assert.That(calendar.DaysPerYear, Is.EqualTo(112));
            Assert.That(calendar.LoanFirstDueDay, Is.EqualTo(112));
            Assert.That(calendar.LoanIntervalDays, Is.EqualTo(28));
        }
        Assert.That(GameCalendar.SeasonsPerYear, Is.EqualTo(4));
        Assert.That(GameCalendar.MinutesPerDay, Is.EqualTo(1440));
    }

    [Test]
    public void TheGameUsesTheRealCalendarAndLegacyIsAnotherInstance()
    {
        Assert.That(CalendarRules.Current, Is.SameAs(CalendarRules.Real));
        Assert.That(CalendarRules.Legacy, Is.Not.SameAs(CalendarRules.Real));
    }

    [Test]
    public void AnyOtherSeasonLengthGetsTheOldLoanRule()
    {
        Assert.That(GameCalendar.Of(28), Is.SameAs(CalendarRules.Legacy));

        GameCalendar ten = GameCalendar.Of(10);
        Assert.That(ten.DaysPerSeason, Is.EqualTo(10));
        Assert.That(ten.DaysPerYear, Is.EqualTo(40));
        Assert.That(ten.LoanFirstDueDay, Is.EqualTo(40), "the end of the first year");
        Assert.That(ten.LoanIntervalDays, Is.EqualTo(10), "then one a season");

        Assert.That(GameCalendar.Of(0).DaysPerSeason, Is.EqualTo(1), "a season length under 1 counts as 1");
        Assert.That(GameCalendar.Of(-3).DaysPerSeason, Is.EqualTo(1));
    }

    [Test]
    public void TheCalendarIsImmutable()
    {
        foreach (PropertyInfo property in typeof(GameCalendar).GetProperties(BindingFlags.Instance | BindingFlags.Public))
            Assert.That(property.CanWrite, Is.False, property.Name + " must be read-only");
        foreach (FieldInfo field in typeof(GameCalendar).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            Assert.That(field.IsInitOnly, Is.True, field.Name + " must be readonly");
    }

    [TestCase(0, 0)]
    [TestCase(27, 0)]
    [TestCase(28, 1)]
    [TestCase(55, 1)]
    [TestCase(56, 2)]
    [TestCase(111, 3)]
    [TestCase(112, 4)]
    [TestCase(224, 8)]
    [TestCase(-5, 0)]
    public void TheSeasonIndexCountsFromZeroWithoutEnd(int day, int index)
    {
        Assert.That(Today.SeasonIndexOf(day), Is.EqualTo(index));
    }

    [TestCase(0, 0)]
    [TestCase(28, 1)]
    [TestCase(56, 2)]
    [TestCase(84, 3)]
    [TestCase(111, 3)]
    [TestCase(112, 0)]
    [TestCase(140, 1)]
    public void TheSeasonOfTheYearIsTheSeasonEnumsOrder(int day, int season)
    {
        Assert.That(Today.SeasonOfYear(day), Is.EqualTo(season));
    }

    [Test]
    public void DayOfSeasonDayOfYearAndYearFollowTheLengths()
    {
        Assert.That(Today.DayOfSeason(0), Is.EqualTo(1));
        Assert.That(Today.DayOfSeason(27), Is.EqualTo(28));
        Assert.That(Today.DayOfSeason(28), Is.EqualTo(1));
        Assert.That(Today.DayOfYear(111), Is.EqualTo(111));
        Assert.That(Today.DayOfYear(112), Is.EqualTo(0));
        Assert.That(Today.YearOfDay(111), Is.EqualTo(1));
        Assert.That(Today.YearOfDay(112), Is.EqualTo(2));
        Assert.That(Today.YearOfSeason(3), Is.EqualTo(1));
        Assert.That(Today.YearOfSeason(4), Is.EqualTo(2));
    }

    [Test]
    public void SeasonsStartAndEndOnWholeDaysAndJoinUp()
    {
        Assert.That(Today.SeasonStartDay(0), Is.EqualTo(0));
        Assert.That(Today.SeasonStartDay(3), Is.EqualTo(84));
        for (int i = 0; i < 8; i++)
        {
            Assert.That(Today.PeriodEndMinute(i), Is.EqualTo(Today.PeriodStartMinute(i + 1)));
            Assert.That(Today.PeriodStartMinute(i), Is.EqualTo((long)Today.SeasonStartDay(i) * 1440));
        }
        Assert.That(Today.PeriodEndMinute(3), Is.EqualTo(112L * 1440));
    }

    [Test]
    public void SeasonProgressRunsFromZeroTowardOne()
    {
        Assert.That(Today.SeasonProgress(0, 0f), Is.EqualTo(0f).Within(1e-6f));
        Assert.That(Today.SeasonProgress(14, 0f), Is.EqualTo(0.5f).Within(1e-6f));
        Assert.That(Today.SeasonProgress(28, 0f), Is.EqualTo(0f).Within(1e-6f), "the next season starts again at 0");
        Assert.That(Today.SeasonProgress(27, 1439f), Is.LessThan(1f).And.GreaterThan(0.99f));
        Assert.That(Today.SeasonProgress(0, 720f), Is.EqualTo(0.5f / 28f).Within(1e-6f), "half a day is half of one of 28 days");
    }

    [TestCase(0, "Spring 1, Year 1")]
    [TestCase(27, "Spring 28, Year 1")]
    [TestCase(28, "Summer 1, Year 1")]
    [TestCase(56, "Fall 1, Year 1")]
    [TestCase(84, "Winter 1, Year 1")]
    [TestCase(111, "Winter 28, Year 1")]
    [TestCase(112, "Spring 1, Year 2")]
    [TestCase(-3, "Spring 1, Year 1")]
    public void DatesAreFormattedAsSeasonDayAndYear(int day, string text)
    {
        Assert.That(Today.FormatDate(day), Is.EqualTo(text));
    }

    [Test]
    public void SeasonNamesAreInTheSeasonEnumsOrder()
    {
        Assert.That(new[] { 0, 1, 2, 3 }.Select(GameCalendar.SeasonName), Is.EqualTo(new[] { "Spring", "Summer", "Fall", "Winter" }));
    }

    [Test]
    public void ALedgerPeriodIsASeasonToday()
    {
        Assert.That(Today.PeriodsPerYear, Is.EqualTo(4));
        for (int day = -2; day < 300; day++)
            Assert.That(Today.PeriodIndexOf(day), Is.EqualTo(Today.SeasonIndexOf(day)), "day " + day);
        Assert.That(Today.PeriodLabel(0), Is.EqualTo("Spring, Year 1"));
        Assert.That(Today.PeriodLabel(3), Is.EqualTo("Winter, Year 1"));
        Assert.That(Today.PeriodLabel(4), Is.EqualTo("Spring, Year 2"));
        Assert.That(Today.YearOfPeriod(7), Is.EqualTo(2));
    }

    // --- The systems that used to repeat the numbers ---

    [Test]
    public void TheLedgerCountsItsPeriodsWithTheCalendarAndKeepsTheOriginalForms()
    {
        Assert.That(new LedgerState().DaysPerSeason, Is.EqualTo(Today.DaysPerSeason));
        for (int day = -2; day < 300; day++)
        {
            Assert.That(LedgerState.PeriodOf(day), Is.EqualTo(Today.PeriodIndexOf(day)), "day " + day);
            Assert.That(LedgerState.SeasonOf(day, 28), Is.EqualTo(LedgerState.PeriodOf(day)), "the original form agrees, day " + day);
        }
        Assert.That(LedgerState.SeasonStartMinute(5, 28), Is.EqualTo(Today.PeriodStartMinute(5)));
        Assert.That(LedgerState.SeasonEndMinute(5, 28), Is.EqualTo(Today.PeriodEndMinute(5)));
        Assert.That(LedgerState.SeasonStartMinute(5, 10), Is.EqualTo(5L * 10 * 1440), "another length still works, as before");
        Assert.That(LedgerState.SeasonLabel(6), Is.EqualTo(Today.PeriodLabel(6)));
        Assert.That(LedgerState.YearOf(6), Is.EqualTo(Today.YearOfPeriod(6)));
        Assert.That(LedgerState.MinutesPerDay, Is.EqualTo(GameCalendar.MinutesPerDay));
    }

    [Test]
    public void ALedgerBegunWithoutALengthTakesTheCalendarsAndWithOneKeepsIt()
    {
        var plain = new LedgerState();
        plain.Begin(100000, 0, 360);
        Assert.That(plain.DaysPerSeason, Is.EqualTo(Today.DaysPerSeason));

        var other = new LedgerState();
        other.Begin(100000, 0, 360, 10);
        Assert.That(other.DaysPerSeason, Is.EqualTo(10), "a length given is the length used (a loaded ledger keeps its saved one)");
    }

    [Test]
    public void TheMarketsSeasonOfADayIsTheCalendars()
    {
        for (int day = -2; day < 300; day++)
        {
            Assert.That(MarketRules.SeasonOf(day), Is.EqualTo(Today.SeasonOfYear(day)), "day " + day);
            Assert.That(MarketRules.SeasonOf(day, 28), Is.EqualTo(MarketRules.SeasonOf(day)), "the original form agrees, day " + day);
        }
        Assert.That(new[] { 0, 1, 2, 3 }.Select(MarketRules.SeasonName), Is.EqualTo(new[] { "Spring", "Summer", "Fall", "Winter" }));
    }

    [Test]
    public void TheLoanTermsScheduleIsTheCalendars()
    {
        LoanTerms terms = LoanTerms.Default;
        Assert.That(terms.firstDueDay, Is.EqualTo(Today.LoanFirstDueDay));
        Assert.That(terms.intervalDays, Is.EqualTo(Today.LoanIntervalDays));

        LoanTerms legacy = LoanTerms.Legacy;
        Assert.That(legacy.firstDueDay, Is.EqualTo(112));
        Assert.That(legacy.intervalDays, Is.EqualTo(28));
        Assert.That(legacy.rateBasisPoints, Is.EqualTo(terms.rateBasisPoints));
        Assert.That(legacy.payments, Is.EqualTo(terms.payments));
        Assert.That(legacy.graceDays, Is.EqualTo(terms.graceDays));
        Assert.That(legacy.lateFeeCents, Is.EqualTo(terms.lateFeeCents));
    }

    [Test]
    public void TreeRegrowthTakesTheCalendarsYear()
    {
        Assert.That(RegrowRules.FullDays, Is.EqualTo(Today.DaysPerYear));
    }

    [Test]
    public void TheRunSummaryCountsSeasonsWithTheCalendarWhenNoLengthIsGiven()
    {
        RunSummary withCalendar = RunSummary.Build(178, 32695, 1299345, 4775000, new[] { 112, 140, 168 }, 133350);
        RunSummary withLength = RunSummary.Build(178, 28, 32695, 1299345, 4775000, new[] { 112, 140, 168 }, 133350);
        Assert.That(withCalendar.seasonsCompleted, Is.EqualTo(6));
        Assert.That(withCalendar.seasonsCompleted, Is.EqualTo(withLength.seasonsCompleted));
        Assert.That(withCalendar.daysSurvived, Is.EqualTo(withLength.daysSurvived));
        Assert.That(withCalendar.missedDueDays, Is.EqualTo(withLength.missedDueDays));
    }

    // --- The guard: the main assembly's systems read the one source (reached by reflection, as the other world tests do) ---

    const BindingFlags Any = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    // The game's own type of that name: Unity has internal types with some of the same names (TimeManager among them), so the main assembly is asked first.
    static Type Find(string name)
    {
        Assembly[] all = AppDomain.CurrentDomain.GetAssemblies();
        Type[] TypesOf(Assembly a) { try { return a.GetTypes(); } catch (Exception) { return new Type[0]; } }
        Type own = all.Where(a => a.GetName().Name == "Assembly-CSharp").SelectMany(TypesOf).FirstOrDefault(t => t.Name == name);
        return own ?? all.SelectMany(TypesOf).First(t => t.Name == name);
    }

    // A manager without a GameObject: nothing here touches Unity, only the managed properties that work the calendar out.
    static object Bare(string typeName) => RuntimeHelpers.GetUninitializedObject(Find(typeName));

    [Test]
    public void TimeManagerReadsTheCalendarAndHasNoSeasonLengthOfItsOwn()
    {
        Type type = Find("TimeManager");
        Assert.That(type.GetField("daysPerSeason", Any), Is.Null, "the serialized season length was removed");

        object time = Bare("TimeManager");
        Assert.That((int)type.GetProperty("DaysPerSeason").GetValue(time), Is.EqualTo(Today.DaysPerSeason));
        Assert.That(type.GetField("MinutesPerDay").GetRawConstantValue(), Is.EqualTo(GameCalendar.MinutesPerDay));

        // Day 0 is Spring 1, Year 1; day 112 is Spring 1, Year 2; day 56 is Fall.
        Assert.That((int)type.GetProperty("Year").GetValue(time), Is.EqualTo(1));
        type.GetField("totalDays", Any).SetValue(time, 112);
        Assert.That((int)type.GetProperty("Year").GetValue(time), Is.EqualTo(Today.YearOfDay(112)));
        Assert.That((int)type.GetProperty("DayOfSeason").GetValue(time), Is.EqualTo(1));
        type.GetField("totalDays", Any).SetValue(time, 56);
        Assert.That(type.GetProperty("CurrentSeason").GetValue(time).ToString(), Is.EqualTo("Fall"));
        Assert.That((float)type.GetProperty("SeasonProgress").GetValue(time), Is.EqualTo(Today.SeasonProgress(56, 0f)).Within(1e-6f));

        MethodInfo format = type.GetMethod("FormatDate");
        foreach (int day in new[] { 0, 27, 28, 111, 112, 300 })
            Assert.That((string)format.Invoke(time, new object[] { day }), Is.EqualTo(Today.FormatDate(day)), "day " + day);
    }

    [Test]
    public void MarketManagerFallsBackToTheCalendarsSeasonLength()
    {
        object market = Bare("MarketManager");
        PropertyInfo length = Find("MarketManager").GetProperty("DaysPerSeason", Any);
        Assert.That((int)length.GetValue(market), Is.EqualTo(Today.DaysPerSeason));
    }

    [Test]
    public void WoodMoistureDefaultsToTheCalendarsSeasonLength()
    {
        Type type = Find("WoodMoisture");
        Assert.That(type.GetProperty("DefaultDaysPerSeason").GetValue(null), Is.EqualTo(Today.DaysPerSeason));
        Assert.That(type.GetProperty("SeasoningDays").GetValue(null), Is.EqualTo(Today.DaysPerSeason), "one season of seasoning with no game running");
    }

    [Test]
    public void MoneyManagerTakesTheLoanScheduleFromTheCalendar()
    {
        Type type = Find("MoneyManager");
        Assert.That(type.GetField("intervalDays", Any), Is.Null, "the serialized spacing was removed");
        Assert.That(type.GetField("firstDueDay", Any), Is.Null, "the serialized first due day was removed");

        object money = Bare("MoneyManager");
        object tier = Enum.ToObject(Find("Difficulty"), 1);
        var terms = (LoanTerms)type.GetMethod("TermsFor").Invoke(money, new[] { tier });
        Assert.That(terms.firstDueDay, Is.EqualTo(Today.LoanFirstDueDay));
        Assert.That(terms.intervalDays, Is.EqualTo(Today.LoanIntervalDays));
    }

    [Test]
    public void EverySystemAgreesOnTheYearAndTheSeason()
    {
        // The one table the guard tests above spread out: season length and year length are the same number everywhere.
        object time = Bare("TimeManager");
        Type timeType = Find("TimeManager");
        Assert.That((int)timeType.GetProperty("DaysPerSeason").GetValue(time), Is.EqualTo(new LedgerState().DaysPerSeason));
        Assert.That(RegrowRules.FullDays, Is.EqualTo(4 * (int)timeType.GetProperty("DaysPerSeason").GetValue(time)));
        Assert.That(LoanTerms.Default.firstDueDay, Is.EqualTo(RegrowRules.FullDays), "the first payment is at the end of the first year");
        Assert.That(LoanTerms.Default.intervalDays, Is.EqualTo(Today.DaysPerSeason), "then one a season");
    }
}
