using System;

// THE ONE SOURCE for the game's calendar (Calendar_and_Time_Scale_Options.md, option D). Pure and engine-free so edit-mode tests cover it. Every system that needs a season, a
// month, a year or a date asks the calendar (CalendarRules.Current); nothing else holds a season length.
//
// Two calendars exist:
//   CalendarRules.Real    the real one the running game uses. A game year runs MARCH TO FEBRUARY (so Winter is never split across two years), 365 days, no leap years. Day 0 is
//                         March 1. Months from March: 31, 30, 31, 30, 31, 31, 30, 31, 30, 31, 31, 28. Seasons start with March, June, September and December: Spring day 0
//                         (92 days), Summer day 92 (92), Fall day 184 (91), Winter day 275 (90). Day of year is TotalDays % 365 and Year is TotalDays / 365 + 1. A date reads
//                         "Mar 1, Year 1"; the accepted oddity is that "Jan 15, Year 1" comes after "Dec 31, Year 1" (the year is the game year, March to February).
//   CalendarRules.Legacy  the calendar the game shipped with: 4 seasons of exactly 28 days, a 112-day year, no real months, and a land loan that falls due on day 112 and then
//                         every 28 days. It stays for good: tests, old loans and old data that pin those numbers point at it. A season length given by a caller (GameCalendar.Of)
//                         is the same kind of calendar.
// What is NOT calendar and stays where it is: the time scale (TimeManager.realMinutesPerDay, 30 real minutes a day) and gameplay tunables measured in plain days
// (WellsAndOuthouses.outhousePitFillDays, seasoningDays).
public sealed class GameCalendar
{
    public const int MinutesPerDay = 1440;
    public const int HoursPerDay = 24;

    // The Season enum has four members (Spring, Summer, Fall, Winter) and every season table in the game has four rows, so this is a fixed fact, not a setting.
    public const int SeasonsPerYear = 4;

    static readonly string[] MonthNames = { "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec", "Jan", "Feb" };

    readonly int[] monthLength;     // days in each month of a game year, March first (the uniform calendar has four "months", one a season)
    readonly int[] monthStart;      // day of the year each month starts on
    readonly int[] seasonStart;     // day of the year each season starts on
    readonly bool namedMonths;      // true for the real calendar: dates read "Mar 1, Year 1"; false for the uniform one: "Spring 1, Year 1"
    readonly int daysPerYear;
    readonly int loanFirstDueDay;
    readonly int loanIntervalDays;
    readonly int loanDueDayOfMonth;

    internal GameCalendar(int[] monthLengths, int monthsPerSeason, bool namedMonths, int loanFirstDueDay, int loanIntervalDays, int loanDueDayOfMonth)
    {
        monthLength = (int[])monthLengths.Clone();
        monthStart = new int[monthLength.Length];
        int total = 0;
        for (int i = 0; i < monthLength.Length; i++)
        {
            monthStart[i] = total;
            total += monthLength[i];
        }
        daysPerYear = total;
        seasonStart = new int[SeasonsPerYear];
        for (int s = 0; s < SeasonsPerYear; s++)
            seasonStart[s] = monthStart[s * monthsPerSeason];
        this.namedMonths = namedMonths;
        this.loanFirstDueDay = loanFirstDueDay;
        this.loanIntervalDays = Math.Max(0, loanIntervalDays);
        this.loanDueDayOfMonth = Math.Max(0, loanDueDayOfMonth);
    }

    // The uniform calendar: four seasons of this many days (a length under 1 counts as 1), no real months, the loan on the old rule (first payment at the end of the first year, then one a
    // season). The 28-day length is CalendarRules.Legacy itself.
    public static GameCalendar Of(int daysPerSeason)
    {
        if (daysPerSeason == CalendarRules.Legacy.DaysPerSeason)
            return CalendarRules.Legacy;
        int length = Math.Max(1, daysPerSeason);
        return Uniform(length, length * SeasonsPerYear, length);
    }

    internal static GameCalendar Uniform(int daysPerSeason, int loanFirstDueDay, int loanIntervalDays) =>
        new GameCalendar(new[] { daysPerSeason, daysPerSeason, daysPerSeason, daysPerSeason }, 1, false, loanFirstDueDay, loanIntervalDays, 0);

    public bool HasRealMonths => namedMonths;

    public int DaysPerYear => daysPerYear;

    // The length of a typical season: exact for the uniform calendar, the average (365 / 4, rounded down) for the real one. Display and rough sums only: the real seasons are 92, 92, 91 and
    // 90 days, so anything that needs a season's length asks SeasonLength.
    public int DaysPerSeason => daysPerYear / SeasonsPerYear;

    public int MonthsPerYear => monthLength.Length;

    // The land loan's schedule (Money_System.md): TotalDays of the first payment, and either the day of the month every payment falls due on (the real calendar: the 1st) or, when that is
    // 0, the days between payments (the old rule).
    public int LoanFirstDueDay => loanFirstDueDay;
    public int LoanIntervalDays => loanIntervalDays;
    public int LoanDueDayOfMonth => loanDueDayOfMonth;

    // --- Years and days ---

    public int DayOfYear(int day) => Math.Max(0, day) % daysPerYear;

    public int YearOfDay(int day) => Math.Max(0, day) / daysPerYear + 1;

    // --- Seasons ---

    // 0 Spring, 1 Summer, 2 Fall, 3 Winter (the Season enum's order), for a day of the year (0-based).
    int SeasonOfDayOfYear(int dayOfYear)
    {
        int season = 0;
        for (int s = 1; s < SeasonsPerYear; s++)
            if (dayOfYear >= seasonStart[s])
                season = s;
        return season;
    }

    // The season a game day is in, counting from 0 (Spring, Year 1) without end: 0 to 3 is Year 1, 4 to 7 is Year 2. Days before 0 count as the first season.
    public int SeasonIndexOf(int day)
    {
        day = Math.Max(0, day);
        return day / daysPerYear * SeasonsPerYear + SeasonOfDayOfYear(day % daysPerYear);
    }

    public int SeasonOfYear(int day) => SeasonOfDayOfYear(DayOfYear(day));

    // The day (TotalDays) a season starts on. A season index below 0 counts as the first.
    public int SeasonStartDay(int seasonIndex)
    {
        seasonIndex = Math.Max(0, seasonIndex);
        return seasonIndex / SeasonsPerYear * daysPerYear + seasonStart[seasonIndex % SeasonsPerYear];
    }

    public int SeasonLength(int seasonIndex) => SeasonStartDay(seasonIndex + 1) - SeasonStartDay(seasonIndex);

    // 1 on the first day of a season.
    public int DayOfSeason(int day) => Math.Max(0, day) - SeasonStartDay(SeasonIndexOf(day)) + 1;

    public int YearOfSeason(int seasonIndex) => Math.Max(0, seasonIndex) / SeasonsPerYear + 1;

    // 0 at the first minute of a season, approaching 1 at its end (TimeManager.SeasonProgress): the seasons are different lengths, so it is a fraction of THIS season.
    public float SeasonProgress(int day, float minuteOfDay)
    {
        int index = SeasonIndexOf(day);
        return (Math.Max(0, day) - SeasonStartDay(index) + minuteOfDay / MinutesPerDay) / SeasonLength(index);
    }

    public static string SeasonName(int seasonOfYear)
    {
        switch (seasonOfYear)
        {
            case 0: return "Spring";
            case 1: return "Summer";
            case 2: return "Fall";
            default: return "Winter";
        }
    }

    public string SeasonLabel(int seasonIndex) => $"{SeasonName(Math.Max(0, seasonIndex) % SeasonsPerYear)}, Year {YearOfSeason(seasonIndex)}";

    // --- Months ---
    //
    // General on purpose, not loan-specific: Bills (a charge on a day of the month) use the same functions. A month index counts from 0 (March, Year 1) without end, like a season
    // index: Year 1 is 0 to 11, Year 2 is 12 to 23.

    // 0 for March to 11 for February.
    public int MonthOfYear(int day) => MonthOfDayOfYear(DayOfYear(day));

    int MonthOfDayOfYear(int dayOfYear)
    {
        int month = 0;
        for (int m = 1; m < monthStart.Length; m++)
            if (dayOfYear >= monthStart[m])
                month = m;
        return month;
    }

    public int MonthIndexOf(int day)
    {
        day = Math.Max(0, day);
        return day / daysPerYear * MonthsPerYear + MonthOfDayOfYear(day % daysPerYear);
    }

    public int MonthStartDay(int monthIndex)
    {
        monthIndex = Math.Max(0, monthIndex);
        return monthIndex / MonthsPerYear * daysPerYear + monthStart[monthIndex % MonthsPerYear];
    }

    public int DaysInMonth(int monthIndex) => monthLength[Math.Max(0, monthIndex) % MonthsPerYear];

    // 1 on the first day of a month.
    public int DayOfMonth(int day) => Math.Max(0, day) - MonthStartDay(MonthIndexOf(day)) + 1;

    // "Mar" to "Feb" for the real calendar; the season's name for the uniform one (whose four "months" are its seasons).
    public string MonthName(int monthOfYear) => namedMonths ? MonthNames[Math.Max(0, monthOfYear) % MonthNames.Length] : SeasonName(Math.Max(0, monthOfYear) % SeasonsPerYear);

    // The first day AFTER the given day that is the dayOfMonth-th of its month. A day of the month a month is too short for (the 31st of a 30-day month, the 29th of February) is that
    // month's last day. Calling it with a payment's own due day gives the next one: NextMonthDay(122, 1) is August 1.
    public int NextMonthDay(int day, int dayOfMonth)
    {
        day = Math.Max(0, day);
        int month = MonthIndexOf(day);
        int candidate = DayInMonth(month, dayOfMonth);
        return candidate > day ? candidate : DayInMonth(month + 1, dayOfMonth);
    }

    // The first day ON OR AFTER the given day that is the dayOfMonth-th of its month.
    public int MonthDayOnOrAfter(int day, int dayOfMonth)
    {
        day = Math.Max(0, day);
        int month = MonthIndexOf(day);
        int candidate = DayInMonth(month, dayOfMonth);
        return candidate >= day ? candidate : DayInMonth(month + 1, dayOfMonth);
    }

    int DayInMonth(int monthIndex, int dayOfMonth) => MonthStartDay(monthIndex) + Math.Min(Math.Max(1, dayOfMonth), DaysInMonth(monthIndex)) - 1;

    // A day count as "Mar 1, Year 1" (the uniform calendar: "Spring 3, Year 1").
    public string FormatDate(int day) => namedMonths
        ? $"{MonthName(MonthOfYear(day))} {DayOfMonth(day)}, Year {YearOfDay(day)}"
        : $"{SeasonName(SeasonOfYear(day))} {DayOfSeason(day)}, Year {YearOfDay(day)}";

    // --- Periods ---
    //
    // The ledger's statement periods. They are a calendar function of their own so the account statement does not depend on how long a season is: today a period is a season, and a
    // later calendar can make it a month without touching the ledger. Everything the ledger needs of time goes through these.

    public int PeriodsPerYear => SeasonsPerYear;

    public int PeriodIndexOf(int day) => SeasonIndexOf(day);

    public int YearOfPeriod(int periodIndex) => YearOfSeason(periodIndex);

    public string PeriodLabel(int periodIndex) => SeasonLabel(periodIndex);

    // Game minutes (TotalHours x 60) at which a period begins and ends.
    public long PeriodStartMinute(int periodIndex) => (long)SeasonStartDay(periodIndex) * MinutesPerDay;

    public long PeriodEndMinute(int periodIndex) => PeriodStartMinute(periodIndex + 1);
}

public static class CalendarRules
{
    // The calendar-version number written into meta.json and slotinfo.json (CalendarSaveRules). A save without it (or with another) was made on the old calendar.
    public const int SaveVersion = 1;

    // The real calendar: March to February, 365 days, the land loan falling due on the 1st of every month from July 1 (day 122).
    public static readonly GameCalendar Real = new GameCalendar(new[] { 31, 30, 31, 30, 31, 31, 30, 31, 30, 31, 31, 28 }, 3, true, 122, 0, 1);

    // The 28-day-season calendar the game shipped with, kept as it was: tests and anything that pins the old numbers use this.
    public static readonly GameCalendar Legacy = GameCalendar.Uniform(28, 112, 28);

    // The calendar the running game uses.
    public static GameCalendar Current => Real;
}
