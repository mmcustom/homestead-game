using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using NUnit.Framework;

// EditMode tests for the calendar's one source (GameCalendar / CalendarRules), real-calendar Phase 1: the real calendar (March to February, 365 days, real months), the uniform Legacy one
// the game shipped with, and the guard that every system which used to repeat a season length reads it.
public class CalendarRulesTests
{
    static readonly GameCalendar Real = CalendarRules.Real;
    static readonly GameCalendar Legacy = CalendarRules.Legacy;

    // From March: 31, 30, 31, 30, 31, 31, 30, 31, 30, 31, 31, 28.
    static readonly int[] MonthLengths = { 31, 30, 31, 30, 31, 31, 30, 31, 30, 31, 31, 28 };
    static readonly int[] MonthStarts = { 0, 31, 61, 92, 122, 153, 184, 214, 245, 275, 306, 337 };
    static readonly int[] SeasonStarts = { 0, 92, 184, 275 };
    static readonly int[] SeasonLengths = { 92, 92, 91, 90 };

    // --- The real calendar's tables ---

    [Test]
    public void TheGameUsesTheRealCalendarAndLegacyIsAnother()
    {
        Assert.That(CalendarRules.Current, Is.SameAs(CalendarRules.Real));
        Assert.That(CalendarRules.Legacy, Is.Not.SameAs(CalendarRules.Real));
        Assert.That(Real.HasRealMonths, Is.True);
        Assert.That(Legacy.HasRealMonths, Is.False);
    }

    [Test]
    public void TheRealYearIs365DaysOfTwelveRealMonthsMarchToFebruary()
    {
        Assert.That(Real.DaysPerYear, Is.EqualTo(365));
        Assert.That(Real.MonthsPerYear, Is.EqualTo(12));
        Assert.That(GameCalendar.SeasonsPerYear, Is.EqualTo(4));
        Assert.That(MonthLengths.Sum(), Is.EqualTo(365));
        for (int m = 0; m < 12; m++)
        {
            Assert.That(Real.DaysInMonth(m), Is.EqualTo(MonthLengths[m]), "month " + m);
            Assert.That(Real.MonthStartDay(m), Is.EqualTo(MonthStarts[m]), "month " + m);
        }
        Assert.That(Real.DaysInMonth(11), Is.EqualTo(28), "February has 28 days, always (no leap years)");
        Assert.That(Real.DaysInMonth(23), Is.EqualTo(28), "and in Year 2");
    }

    [Test]
    public void TheSeasonsStartWithMarchJuneSeptemberAndDecember()
    {
        Assert.That(SeasonLengths.Sum(), Is.EqualTo(365));
        for (int s = 0; s < 4; s++)
        {
            Assert.That(Real.SeasonStartDay(s), Is.EqualTo(SeasonStarts[s]), "season " + s);
            Assert.That(Real.SeasonLength(s), Is.EqualTo(SeasonLengths[s]), "season " + s);
        }
        Assert.That(Real.SeasonStartDay(4), Is.EqualTo(365), "Spring of Year 2");
        Assert.That(Real.SeasonStartDay(7), Is.EqualTo(365 + 275), "Winter of Year 2");
        Assert.That(Real.SeasonLength(4), Is.EqualTo(92), "the lengths repeat every year");
        Assert.That(Real.SeasonLength(7), Is.EqualTo(90));
        // March, June, September, December are the first days of Spring, Summer, Fall, Winter.
        Assert.That(new[] { 0, 3, 6, 9 }.Select(m => Real.MonthStartDay(m)), Is.EqualTo(SeasonStarts));
    }

    [Test]
    public void EveryDayOfTheYearHasOneMonthOneSeasonAndOneDayOfEach()
    {
        for (int day = 0; day < 365 * 3; day++)
        {
            int month = Real.MonthIndexOf(day);
            Assert.That(Real.MonthStartDay(month), Is.LessThanOrEqualTo(day), "day " + day);
            Assert.That(Real.MonthStartDay(month + 1), Is.GreaterThan(day), "day " + day);
            Assert.That(Real.MonthStartDay(month) + Real.DayOfMonth(day) - 1, Is.EqualTo(day), "day " + day);
            Assert.That(Real.DayOfMonth(day), Is.InRange(1, Real.DaysInMonth(month)), "day " + day);

            int season = Real.SeasonIndexOf(day);
            Assert.That(Real.SeasonStartDay(season), Is.LessThanOrEqualTo(day), "day " + day);
            Assert.That(Real.SeasonStartDay(season + 1), Is.GreaterThan(day), "day " + day);
            Assert.That(Real.SeasonStartDay(season) + Real.DayOfSeason(day) - 1, Is.EqualTo(day), "day " + day);
            Assert.That(Real.SeasonOfYear(day), Is.EqualTo(season % 4), "day " + day);
            Assert.That(Real.MonthOfYear(day), Is.EqualTo(month % 12), "day " + day);
        }
    }

    // --- Boundaries: every season, the year, the months around February ---

    [TestCase(0, 0)]
    [TestCase(91, 0)]
    [TestCase(92, 1)]
    [TestCase(183, 1)]
    [TestCase(184, 2)]
    [TestCase(274, 2)]
    [TestCase(275, 3)]
    [TestCase(364, 3)]
    [TestCase(365, 4)]
    [TestCase(365 + 91, 4)]
    [TestCase(365 + 92, 5)]
    [TestCase(365 + 275, 7)]
    [TestCase(730, 8)]
    [TestCase(-5, 0)]
    public void TheSeasonIndexCountsFromZeroWithoutEnd(int day, int index)
    {
        Assert.That(Real.SeasonIndexOf(day), Is.EqualTo(index));
    }

    [TestCase(0, 0)]
    [TestCase(91, 0)]
    [TestCase(92, 1)]
    [TestCase(184, 2)]
    [TestCase(274, 2)]
    [TestCase(275, 3)]
    [TestCase(364, 3)]
    [TestCase(365, 0)]
    [TestCase(365 + 184, 2)]
    public void TheSeasonOfTheYearIsTheSeasonEnumsOrder(int day, int season)
    {
        Assert.That(Real.SeasonOfYear(day), Is.EqualTo(season));
    }

    [TestCase(0, 0, 0, 1)]
    [TestCase(30, 0, 0, 31)]
    [TestCase(31, 1, 0, 1)]
    [TestCase(91, 2, 0, 31)]
    [TestCase(92, 3, 1, 1)]
    [TestCase(274, 8, 2, 30)]
    [TestCase(275, 9, 3, 1)]
    [TestCase(305, 9, 3, 31)]
    [TestCase(306, 10, 3, 1)]
    [TestCase(336, 10, 3, 31)]
    [TestCase(337, 11, 3, 1)]
    [TestCase(364, 11, 3, 28)]
    [TestCase(365, 12, 0, 1)]
    public void MonthBoundariesIncludingFebruaryAndTheYearRollover(int day, int monthIndex, int season, int dayOfMonth)
    {
        Assert.That(Real.MonthIndexOf(day), Is.EqualTo(monthIndex));
        Assert.That(Real.SeasonOfYear(day), Is.EqualTo(season));
        Assert.That(Real.DayOfMonth(day), Is.EqualTo(dayOfMonth));
    }

    [Test]
    public void TheYearAndTheDayOfTheYearFollowThe365DayCount()
    {
        Assert.That(Real.YearOfDay(0), Is.EqualTo(1));
        Assert.That(Real.YearOfDay(364), Is.EqualTo(1));
        Assert.That(Real.YearOfDay(365), Is.EqualTo(2));
        Assert.That(Real.YearOfDay(729), Is.EqualTo(2));
        Assert.That(Real.YearOfDay(730), Is.EqualTo(3));
        Assert.That(Real.DayOfYear(364), Is.EqualTo(364));
        Assert.That(Real.DayOfYear(365), Is.EqualTo(0));
        Assert.That(Real.DayOfYear(365 + 122), Is.EqualTo(122));
        Assert.That(Real.YearOfSeason(3), Is.EqualTo(1));
        Assert.That(Real.YearOfSeason(4), Is.EqualTo(2));
    }

    [TestCase(0, 1)]
    [TestCase(91, 92)]
    [TestCase(92, 1)]
    [TestCase(274, 91)]
    [TestCase(275, 1)]
    [TestCase(364, 90)]
    [TestCase(365, 1)]
    public void DayOfSeasonStartsAtOne(int day, int dayOfSeason)
    {
        Assert.That(Real.DayOfSeason(day), Is.EqualTo(dayOfSeason));
    }

    [Test]
    public void SeasonProgressIsAFractionOfThisSeasonWhateverItsLength()
    {
        Assert.That(Real.SeasonProgress(0, 0f), Is.EqualTo(0f).Within(1e-6f));
        Assert.That(Real.SeasonProgress(46, 0f), Is.EqualTo(0.5f).Within(1e-6f), "half of Spring's 92 days");
        Assert.That(Real.SeasonProgress(275 + 45, 0f), Is.EqualTo(0.5f).Within(1e-6f), "half of Winter's 90 days");
        Assert.That(Real.SeasonProgress(184 + 30, 720f), Is.EqualTo(30.5f / 91f).Within(1e-6f), "Fall is 91 days");
        Assert.That(Real.SeasonProgress(92, 0f), Is.EqualTo(0f).Within(1e-6f), "the next season starts again at 0");
        Assert.That(Real.SeasonProgress(91, 1439f), Is.LessThan(1f).And.GreaterThan(0.99f));
        Assert.That(Real.SeasonProgress(364, 1439f), Is.LessThan(1f).And.GreaterThan(0.99f), "the last minute of Winter");
        Assert.That(Real.SeasonProgress(365, 0f), Is.EqualTo(0f).Within(1e-6f), "and the new year's Spring begins");
    }

    // --- Dates ---

    [TestCase(0, "Mar 1, Year 1")]
    [TestCase(30, "Mar 31, Year 1")]
    [TestCase(31, "Apr 1, Year 1")]
    [TestCase(122, "Jul 1, Year 1")]
    [TestCase(274, "Nov 30, Year 1")]
    [TestCase(275, "Dec 1, Year 1")]
    [TestCase(305, "Dec 31, Year 1")]
    [TestCase(306, "Jan 1, Year 1")]
    [TestCase(320, "Jan 15, Year 1")]
    [TestCase(337, "Feb 1, Year 1")]
    [TestCase(364, "Feb 28, Year 1")]
    [TestCase(365, "Mar 1, Year 2")]
    [TestCase(365 + 122, "Jul 1, Year 2")]
    [TestCase(-3, "Mar 1, Year 1")]
    public void DatesAreFormattedAsMonthDayAndYear(int day, string text)
    {
        Assert.That(Real.FormatDate(day), Is.EqualTo(text));
    }

    [Test]
    public void JanuaryComesAfterDecemberInTheSameGameYear()
    {
        // The accepted oddity: the year is the game year, March to February.
        Assert.That(Real.FormatDate(305), Is.EqualTo("Dec 31, Year 1"));
        Assert.That(Real.FormatDate(306), Is.EqualTo("Jan 1, Year 1"));
        Assert.That(Real.YearOfDay(306), Is.EqualTo(Real.YearOfDay(305)));
    }

    [Test]
    public void MonthAndSeasonNames()
    {
        Assert.That(Enumerable.Range(0, 12).Select(Real.MonthName), Is.EqualTo(new[] { "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", "Jan", "Feb" }));
        Assert.That(new[] { 0, 1, 2, 3 }.Select(GameCalendar.SeasonName), Is.EqualTo(new[] { "Spring", "Summer", "Fall", "Winter" }));
        Assert.That(Real.SeasonLabel(0), Is.EqualTo("Spring, Year 1"));
        Assert.That(Real.SeasonLabel(3), Is.EqualTo("Winter, Year 1"));
        Assert.That(Real.SeasonLabel(4), Is.EqualTo("Spring, Year 2"));
    }

    [Test]
    public void ALedgerPeriodIsASeasonAndEndsWhereTheNextBegins()
    {
        Assert.That(Real.PeriodsPerYear, Is.EqualTo(4));
        for (int day = -2; day < 800; day++)
            Assert.That(Real.PeriodIndexOf(day), Is.EqualTo(Real.SeasonIndexOf(day)), "day " + day);
        for (int i = 0; i < 12; i++)
        {
            Assert.That(Real.PeriodEndMinute(i), Is.EqualTo(Real.PeriodStartMinute(i + 1)));
            Assert.That(Real.PeriodStartMinute(i), Is.EqualTo((long)Real.SeasonStartDay(i) * 1440));
        }
        Assert.That(Real.PeriodEndMinute(0), Is.EqualTo(92L * 1440), "Spring is 92 days, not 28");
        Assert.That(Real.PeriodEndMinute(3), Is.EqualTo(365L * 1440));
        Assert.That(Real.YearOfPeriod(7), Is.EqualTo(2));
    }

    // --- The months, general (the loan and Bills use them) ---

    [TestCase(0, 1, 31)]     // Mar 1 -> Apr 1
    [TestCase(30, 1, 31)]    // Mar 31 -> Apr 1
    [TestCase(31, 1, 61)]    // Apr 1 -> May 1: the first day after, not the day itself
    [TestCase(122, 1, 153)]  // Jul 1 -> Aug 1
    [TestCase(305, 1, 306)]  // Dec 31 -> Jan 1
    [TestCase(336, 1, 337)]  // Jan 31 -> Feb 1
    [TestCase(337, 1, 365)]  // Feb 1 -> Mar 1 of the next year
    [TestCase(364, 1, 365)]  // Feb 28 -> Mar 1
    [TestCase(365, 1, 396)]  // Mar 1 Y2 -> Apr 1 Y2
    [TestCase(0, 15, 14)]    // Mar 1 -> Mar 15
    [TestCase(14, 15, 45)]   // Mar 15 -> Apr 15
    [TestCase(337, 31, 364)] // Feb 1 with the 31st: February's last day
    [TestCase(364, 31, 395)] // Feb 28 -> Mar 31 of the next year
    [TestCase(0, 28, 27)]
    [TestCase(-4, 1, 31)]    // before day 0 counts as day 0
    public void NextMonthDayIsTheFirstSuchDayAfterTheGivenDay(int day, int dayOfMonth, int expected)
    {
        Assert.That(Real.NextMonthDay(day, dayOfMonth), Is.EqualTo(expected));
    }

    [TestCase(0, 1, 0)]
    [TestCase(1, 1, 31)]
    [TestCase(122, 1, 122)]
    [TestCase(123, 1, 153)]
    [TestCase(364, 1, 365)]
    [TestCase(10, 15, 14)]
    [TestCase(14, 15, 14)]
    [TestCase(15, 15, 45)]
    public void MonthDayOnOrAfterKeepsTheDayItselfWhenItFits(int day, int dayOfMonth, int expected)
    {
        Assert.That(Real.MonthDayOnOrAfter(day, dayOfMonth), Is.EqualTo(expected));
    }

    [Test]
    public void ARunOfNextMonthDaysIsTheFirstOfEveryMonthAcrossTheYears()
    {
        int day = Real.LoanFirstDueDay;
        for (int i = 0; i < 40; i++)
        {
            Assert.That(Real.DayOfMonth(day), Is.EqualTo(1), "payment " + i);
            Assert.That(Real.MonthIndexOf(day), Is.EqualTo(4 + i), "one a month, no month skipped or doubled");
            day = Real.NextMonthDay(day, 1);
        }
    }

    [Test]
    public void TheLoanScheduleIsJulyFirstThenThe1stOfEveryMonth()
    {
        Assert.That(Real.LoanFirstDueDay, Is.EqualTo(122));
        Assert.That(Real.LoanFirstDueDay, Is.EqualTo(Real.MonthStartDay(4)), "July 1");
        Assert.That(Real.FormatDate(Real.LoanFirstDueDay), Is.EqualTo("Jul 1, Year 1"));
        Assert.That(Real.LoanDueDayOfMonth, Is.EqualTo(1));
        Assert.That(Real.LoanIntervalDays, Is.EqualTo(0), "a monthly loan has no interval in days");
    }

    // --- The Legacy (uniform) calendar the game shipped with ---

    [Test]
    public void LegacyIsFourSeasonsOf28DaysWithNoRealMonths()
    {
        Assert.That(Legacy.DaysPerSeason, Is.EqualTo(28));
        Assert.That(Legacy.DaysPerYear, Is.EqualTo(112));
        Assert.That(Legacy.LoanFirstDueDay, Is.EqualTo(112));
        Assert.That(Legacy.LoanIntervalDays, Is.EqualTo(28));
        Assert.That(Legacy.LoanDueDayOfMonth, Is.EqualTo(0), "the old rule: every 28 days");
        Assert.That(Legacy.SeasonIndexOf(111), Is.EqualTo(3));
        Assert.That(Legacy.SeasonIndexOf(112), Is.EqualTo(4));
        Assert.That(Legacy.FormatDate(0), Is.EqualTo("Spring 1, Year 1"));
        Assert.That(Legacy.FormatDate(28), Is.EqualTo("Summer 1, Year 1"));
        Assert.That(Legacy.FormatDate(111), Is.EqualTo("Winter 28, Year 1"));
        Assert.That(Legacy.FormatDate(112), Is.EqualTo("Spring 1, Year 2"));
        Assert.That(Legacy.SeasonProgress(14, 0f), Is.EqualTo(0.5f).Within(1e-6f));
    }

    [Test]
    public void AnyOtherSeasonLengthIsAUniformCalendarWithTheOldLoanRule()
    {
        Assert.That(GameCalendar.Of(28), Is.SameAs(CalendarRules.Legacy));
        GameCalendar ten = GameCalendar.Of(10);
        Assert.That(ten.DaysPerSeason, Is.EqualTo(10));
        Assert.That(ten.DaysPerYear, Is.EqualTo(40));
        Assert.That(ten.LoanFirstDueDay, Is.EqualTo(40), "the end of the first year");
        Assert.That(ten.LoanIntervalDays, Is.EqualTo(10), "then one a season");
        Assert.That(GameCalendar.Of(0).DaysPerSeason, Is.EqualTo(1), "a season length under 1 counts as 1");
        Assert.That(GameCalendar.Of(-3).DaysPerSeason, Is.EqualTo(1));
        Assert.That(ten.SeasonStartMinuteOf(5), Is.EqualTo(5L * 10 * 1440));
    }

    [Test]
    public void TheCalendarIsImmutable()
    {
        foreach (PropertyInfo property in typeof(GameCalendar).GetProperties(BindingFlags.Instance | BindingFlags.Public))
            Assert.That(property.CanWrite, Is.False, property.Name + " must be read-only");
        foreach (FieldInfo field in typeof(GameCalendar).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            Assert.That(field.IsInitOnly, Is.True, field.Name + " must be readonly");
    }

    // --- The guard: every system that used to repeat the numbers reads the calendar ---

    [Test]
    public void TheLedgerCountsItsPeriodsWithTheRealCalendarAndKeepsTheOriginalForms()
    {
        var plain = new LedgerState();
        Assert.That(plain.Calendar, Is.SameAs(Real));
        for (int day = -2; day < 800; day++)
            Assert.That(LedgerState.PeriodOf(day), Is.EqualTo(Real.PeriodIndexOf(day)), "day " + day);
        Assert.That(LedgerState.SeasonLabel(6), Is.EqualTo(Real.PeriodLabel(6)));
        Assert.That(LedgerState.YearOf(6), Is.EqualTo(Real.YearOfPeriod(6)));
        Assert.That(LedgerState.MinutesPerDay, Is.EqualTo(GameCalendar.MinutesPerDay));
        // The original forms that take a season length are uniform seasons of that length.
        Assert.That(LedgerState.SeasonOf(112, 28), Is.EqualTo(4));
        Assert.That(LedgerState.SeasonStartMinute(5, 10), Is.EqualTo(5L * 10 * 1440));
    }

    [Test]
    public void ALedgerBegunWithoutALengthUsesTheRealCalendarsPeriodsAndWithOneKeepsIt()
    {
        var real = new LedgerState();
        real.Begin(100000, 0, 360);
        Assert.That(real.Calendar, Is.SameAs(Real));
        real.CloseThrough(1, 100000, 0, 0);
        Assert.That(real.Closed[0].endMinute, Is.EqualTo(92L * 1440), "Spring ends after 92 days");
        real.CloseThrough(3, 100000, 0, 0);
        Assert.That(real.Closed.Select(s => s.endMinute), Is.EqualTo(new[] { 92L * 1440, 184L * 1440, 275L * 1440 }));

        var uniform = new LedgerState();
        uniform.Begin(100000, 0, 360, 10);
        Assert.That(uniform.DaysPerSeason, Is.EqualTo(10), "a length given is the length used");
        uniform.CloseThrough(1, 100000, 0, 0);
        Assert.That(uniform.Closed[0].endMinute, Is.EqualTo(10L * 1440));
    }

    [Test]
    public void ALedgerSavesTheRealCalendarAsZeroAndAUniformOneWithItsLength()
    {
        var real = new LedgerState();
        real.Begin(100000, 0, 360);
        LedgerSaveData saved = real.Capture();
        Assert.That(saved.daysPerSeason, Is.EqualTo(0), "0: the game's real calendar");
        var loaded = new LedgerState();
        loaded.Restore(saved);
        Assert.That(loaded.Calendar, Is.SameAs(Real));

        var uniform = new LedgerState();
        uniform.Begin(100000, 0, 360, 28);
        Assert.That(uniform.Capture().daysPerSeason, Is.EqualTo(28));
        var reloaded = new LedgerState();
        reloaded.Restore(uniform.Capture());
        Assert.That(reloaded.DaysPerSeason, Is.EqualTo(28));
    }

    [Test]
    public void TheMarketsSeasonOfADayIsTheRealCalendars()
    {
        for (int day = -2; day < 800; day++)
            Assert.That(MarketRules.SeasonOf(day), Is.EqualTo(Real.SeasonOfYear(day)), "day " + day);
        Assert.That(MarketRules.SeasonOf(91), Is.EqualTo(0));
        Assert.That(MarketRules.SeasonOf(92), Is.EqualTo(1));
        Assert.That(MarketRules.SeasonOf(274), Is.EqualTo(2));
        Assert.That(MarketRules.SeasonOf(275), Is.EqualTo(3));
        Assert.That(MarketRules.SeasonOf(84, 28), Is.EqualTo(3), "the original form takes a uniform length");
    }

    [Test]
    public void TheLoanTermsScheduleIsTheCalendarsAndLegacyKeepsTheOldRule()
    {
        LoanTerms terms = LoanTerms.Default;
        Assert.That(terms.firstDueDay, Is.EqualTo(Real.LoanFirstDueDay));
        Assert.That(terms.dueDayOfMonth, Is.EqualTo(Real.LoanDueDayOfMonth));
        Assert.That(terms.intervalDays, Is.EqualTo(Real.LoanIntervalDays));
        Assert.That(terms.firstDueDay, Is.EqualTo(122));

        LoanTerms legacy = LoanTerms.Legacy;
        Assert.That(legacy.firstDueDay, Is.EqualTo(112));
        Assert.That(legacy.intervalDays, Is.EqualTo(28));
        Assert.That(legacy.dueDayOfMonth, Is.EqualTo(0));
        Assert.That(legacy.rateBasisPoints, Is.EqualTo(terms.rateBasisPoints));
        Assert.That(legacy.payments, Is.EqualTo(terms.payments));
        Assert.That(legacy.graceDays, Is.EqualTo(terms.graceDays));
        Assert.That(legacy.lateFeeCents, Is.EqualTo(terms.lateFeeCents));
    }

    [Test]
    public void TreeRegrowthTakesTheCalendarsYear()
    {
        Assert.That(RegrowRules.FullDays, Is.EqualTo(Real.DaysPerYear));
        Assert.That(RegrowRules.FullDays, Is.EqualTo(365));
    }

    [Test]
    public void TheRunSummaryCountsWholeSeasonsWithTheRealCalendarWhenNoLengthIsGiven()
    {
        RunSummary byCalendar = RunSummary.Build(200, 32695, 1299345, 4775000, new[] { 122, 153, 184 }, 133350);
        Assert.That(byCalendar.seasonsCompleted, Is.EqualTo(2), "day 200 is in Fall (day 184 on): two whole seasons behind");
        Assert.That(RunSummary.Build(365, 0, 0, 0, null, 0).seasonsCompleted, Is.EqualTo(4));
        RunSummary withLength = RunSummary.Build(178, 28, 32695, 1299345, 4775000, new[] { 112, 140, 168 }, 133350);
        Assert.That(withLength.seasonsCompleted, Is.EqualTo(6), "the original form still counts uniform seasons");
    }

    // The main assembly's types are reached by reflection, as the other world tests do.

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
        Assert.That(type.GetField("daysPerSeason", Any), Is.Null, "no serialized season length");
        Assert.That(type.GetField("MinutesPerDay").GetRawConstantValue(), Is.EqualTo(GameCalendar.MinutesPerDay));

        object time = Bare("TimeManager");
        Assert.That((int)type.GetProperty("DaysPerSeason").GetValue(time), Is.EqualTo(Real.DaysPerSeason));
        FieldInfo days = type.GetField("totalDays", Any);
        foreach (int day in new[] { 0, 91, 92, 122, 183, 184, 274, 275, 305, 306, 337, 364, 365, 365 + 122, 1000 })
        {
            days.SetValue(time, day);
            Assert.That((int)type.GetProperty("Year").GetValue(time), Is.EqualTo(Real.YearOfDay(day)), "year, day " + day);
            Assert.That((int)type.GetProperty("DayOfSeason").GetValue(time), Is.EqualTo(Real.DayOfSeason(day)), "day of season, day " + day);
            Assert.That((int)type.GetProperty("DayOfMonth").GetValue(time), Is.EqualTo(Real.DayOfMonth(day)), "day of month, day " + day);
            Assert.That((int)type.GetProperty("MonthIndex").GetValue(time), Is.EqualTo(Real.MonthIndexOf(day)), "month index, day " + day);
            Assert.That((int)type.GetProperty("MonthOfYear").GetValue(time), Is.EqualTo(Real.MonthOfYear(day)), "month of year, day " + day);
            Assert.That((int)type.GetProperty("CurrentSeason").GetValue(time), Is.EqualTo(Real.SeasonOfYear(day)), "season, day " + day);
            Assert.That((float)type.GetProperty("SeasonProgress").GetValue(time), Is.EqualTo(Real.SeasonProgress(day, 0f)).Within(1e-6f), "progress, day " + day);
            Assert.That((string)type.GetMethod("FormatDate").Invoke(time, new object[] { day }), Is.EqualTo(Real.FormatDate(day)), "date, day " + day);
        }
    }

    [Test]
    public void TimeManagerCountsSeasonsAndYearsOfTheRealCalendar()
    {
        Type type = Find("TimeManager");
        object time = Bare("TimeManager");
        FieldInfo days = type.GetField("totalDays", Any);
        days.SetValue(time, 91);
        Assert.That(type.GetProperty("CurrentSeason").GetValue(time).ToString(), Is.EqualTo("Spring"));
        days.SetValue(time, 92);
        Assert.That(type.GetProperty("CurrentSeason").GetValue(time).ToString(), Is.EqualTo("Summer"));
        days.SetValue(time, 275);
        Assert.That(type.GetProperty("CurrentSeason").GetValue(time).ToString(), Is.EqualTo("Winter"));
        days.SetValue(time, 364);
        Assert.That((int)type.GetProperty("Year").GetValue(time), Is.EqualTo(1));
        days.SetValue(time, 365);
        Assert.That((int)type.GetProperty("Year").GetValue(time), Is.EqualTo(2));
        Assert.That(type.GetProperty("CurrentSeason").GetValue(time).ToString(), Is.EqualTo("Spring"));
    }

    [Test]
    public void MarketManagerUsesTheCalendarsSeason()
    {
        object market = Bare("MarketManager");
        PropertyInfo season = Find("MarketManager").GetProperty("Season", Any);
        Assert.That((int)season.GetValue(market), Is.EqualTo(Real.SeasonOfYear(0)), "no game running: day 0 is Spring");
        Assert.That(Find("MarketManager").GetProperty("DaysPerSeason", Any), Is.Null, "no season length of its own");
    }

    [Test]
    public void WoodSeasoningIs28PlainDaysNotACalendarSeason()
    {
        Type moisture = Find("WoodMoisture");
        Assert.That(moisture.GetField("DefaultSeasoningDays").GetRawConstantValue(), Is.EqualTo(28));
        Assert.That(moisture.GetProperty("SeasoningDays").GetValue(null), Is.EqualTo(28), "no game running");
        Assert.That(moisture.GetProperty("DefaultDaysPerSeason"), Is.Null, "no season length of its own");
        Type wells = Find("WellsAndOuthouses");
        Assert.That(wells.GetField("seasoningSeasons"), Is.Null, "the tunable is days now");
        FieldInfo seasoningDays = wells.GetField("seasoningDays");
        Assert.That(seasoningDays, Is.Not.Null);
        Assert.That(seasoningDays.FieldType, Is.EqualTo(typeof(int)));
        Assert.That(wells.GetField("outhousePitFillDays").FieldType, Is.EqualTo(typeof(int)), "the outhouse pit stays its own tunable in days");
    }

    [Test]
    public void MoneyManagerTakesTheLoanScheduleFromTheCalendar()
    {
        Type type = Find("MoneyManager");
        Assert.That(type.GetField("intervalDays", Any), Is.Null);
        Assert.That(type.GetField("firstDueDay", Any), Is.Null);

        object money = Bare("MoneyManager");
        object tier = Enum.ToObject(Find("Difficulty"), 1);
        var terms = (LoanTerms)type.GetMethod("TermsFor").Invoke(money, new[] { tier });
        Assert.That(terms.firstDueDay, Is.EqualTo(Real.LoanFirstDueDay));
        Assert.That(terms.dueDayOfMonth, Is.EqualTo(Real.LoanDueDayOfMonth));
        Assert.That(terms.intervalDays, Is.EqualTo(Real.LoanIntervalDays));
    }

    [Test]
    public void EverySystemAgreesOnTheYearTheSeasonsAndTheLoanSchedule()
    {
        Type timeType = Find("TimeManager");
        object time = Bare("TimeManager");
        Assert.That((int)timeType.GetProperty("DaysPerSeason").GetValue(time), Is.EqualTo(new LedgerState().DaysPerSeason));
        Assert.That(RegrowRules.FullDays, Is.EqualTo(Real.DaysPerYear));
        Assert.That(LoanTerms.Default.firstDueDay, Is.EqualTo(Real.LoanFirstDueDay));
        Assert.That(LoanTerms.Default.firstDueDay, Is.EqualTo(Real.MonthStartDay(4)), "July 1");
        Assert.That(LedgerState.PeriodOf(Real.SeasonStartDay(5)), Is.EqualTo(5));
        Assert.That(MarketRules.SeasonOf(Real.SeasonStartDay(5)), Is.EqualTo(1));
    }
}

static class GameCalendarTestExtensions
{
    public static long SeasonStartMinuteOf(this GameCalendar calendar, int index) => calendar.PeriodStartMinute(index);
}
