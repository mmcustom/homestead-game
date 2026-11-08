using System;

// THE ONE SOURCE for the game's calendar (Calendar_and_Time_Scale_Options.md, real-calendar Phase 0). Pure and engine-free so edit-mode tests cover it.
//
// Before this file the same numbers (28 days a season, a 112-day year, the loan's first due day and spacing) were repeated in TimeManager, LedgerState, MarketManager,
// WoodMoisture, RegrowRules, LoanTerms.Default and MoneyManager. They all read GameCalendar now; to change the calendar, change it here.
//
// Phase 0 holds TODAY'S numbers and changes no behaviour: 4 seasons of 28 days, a 112-day year, the first land payment on day 112 and one every 28 days. CalendarRules.Real is the
// calendar the running game uses; CalendarRules.Legacy is the 28-day calendar kept for good, so tests and old data that pin the old numbers have somewhere to point once Real changes.
// What is NOT calendar and stays where it is: the time scale (TimeManager.realMinutesPerDay, 30 real minutes a day) and gameplay tunables that happen to equal 28
// (WellsAndOuthouses.outhousePitFillDays).
public sealed class GameCalendar
{
    public const int MinutesPerDay = 1440;
    public const int HoursPerDay = 24;

    // The Season enum has four members (Spring, Summer, Fall, Winter) and every season table in the game has four rows, so this is a fixed fact, not a setting.
    public const int SeasonsPerYear = 4;

    readonly int daysPerSeason;
    readonly int loanFirstDueDay;
    readonly int loanIntervalDays;

    internal GameCalendar(int daysPerSeason, int loanFirstDueDay, int loanIntervalDays)
    {
        this.daysPerSeason = Math.Max(1, daysPerSeason);
        this.loanFirstDueDay = loanFirstDueDay;
        this.loanIntervalDays = Math.Max(1, loanIntervalDays);
    }

    // A calendar of seasons this long, with the loan on the old rule (first payment at the end of the first year, then one a season). The 28-day length is CalendarRules.Legacy itself.
    // A length under 1 counts as 1, as the old functions did.
    public static GameCalendar Of(int daysPerSeason)
    {
        if (daysPerSeason == CalendarRules.Legacy.daysPerSeason)
            return CalendarRules.Legacy;
        int length = Math.Max(1, daysPerSeason);
        return new GameCalendar(length, length * SeasonsPerYear, length);
    }

    public int DaysPerSeason => daysPerSeason;
    public int DaysPerYear => daysPerSeason * SeasonsPerYear;

    // The land loan's schedule (Money_System.md): TotalDays of the first payment, and the days between payments.
    public int LoanFirstDueDay => loanFirstDueDay;
    public int LoanIntervalDays => loanIntervalDays;

    // --- Seasons ---

    // The season a game day is in, counting from 0 (Spring, Year 1) without end: 0 to 3 is Year 1, 4 to 7 is Year 2. Days before 0 count as the first season.
    public int SeasonIndexOf(int day) => Math.Max(0, day) / daysPerSeason;

    // 0 Spring, 1 Summer, 2 Fall, 3 Winter (the Season enum's order).
    public int SeasonOfYear(int day) => SeasonIndexOf(day) % SeasonsPerYear;

    // 1 on the first day of a season.
    public int DayOfSeason(int day) => Math.Max(0, day) % daysPerSeason + 1;

    public int DayOfYear(int day) => Math.Max(0, day) % DaysPerYear;

    public int YearOfDay(int day) => Math.Max(0, day) / DaysPerYear + 1;

    public int YearOfSeason(int seasonIndex) => seasonIndex / SeasonsPerYear + 1;

    public int SeasonStartDay(int seasonIndex) => seasonIndex * daysPerSeason;

    // 0 at the first minute of a season, approaching 1 at its end (TimeManager.SeasonProgress).
    public float SeasonProgress(int day, float minuteOfDay) =>
        (Math.Max(0, day) % daysPerSeason + minuteOfDay / MinutesPerDay) / daysPerSeason;

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

    public string SeasonLabel(int seasonIndex) => $"{SeasonName(seasonIndex % SeasonsPerYear)}, Year {YearOfSeason(seasonIndex)}";

    // A day count as "Spring 3, Year 1".
    public string FormatDate(int day) => $"{SeasonName(SeasonOfYear(day))} {DayOfSeason(day)}, Year {YearOfDay(day)}";

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
    // The calendar the running game uses. Phase 0: the same numbers as Legacy.
    public static readonly GameCalendar Real = new GameCalendar(28, 112, 28);

    // The 28-day-season calendar the game shipped with, kept as it was: tests and anything that pins the old numbers use this.
    public static readonly GameCalendar Legacy = new GameCalendar(28, 112, 28);

    public static GameCalendar Current => Real;
}
