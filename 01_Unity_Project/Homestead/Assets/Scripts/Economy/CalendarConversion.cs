using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

// One-off conversion of a save from the OLD calendar (4 seasons of 28 days, a 112-day year) onto the real one (real months, a 365-day year), real-calendar Phase 1. Pure and engine-free
// so edit-mode tests cover it; the Editor tool (Homestead > Dev) does the file work and writes a NEW slot. It never modifies the save it reads.
//
// The idea: the player keeps the SEASON and how far through it they are, so a game on Summer 11, Year 1 lands about a third of the way through Summer, Year 1. Then every number that is a
// day in the save is brought onto the new calendar by what it means:
//   a date (the clock, the journal, the discovery log, the market's last day, the loan's due days, the ledger's periods)   MapDay: the same season, the same fraction of it
//   how long ago something happened (an item's day acquired, wood's day, a grass clump cut, a forage patch picked, a dropped item)   the AGE is kept in days: a loaf two days old is still
//                                                                       two days old, whatever the calendar (shelf life, seasoning and regrowth timers are plain days)
//   a felled tree's regrowth stamp                                       the age is SCALED by 365/112, so the tree is the same fraction of the way back to full size
//   a dry pottery piece's ready hour                                     the hours still to go are kept
//   hours (fuel, sap age, illness, leaching, torch)                      untouched: they are durations, not dates
// The land loan is moved onto the monthly rule keeping its proportions: the first payment, which was due at the end of the first year (day 112), falls due on the 1st of the month the
// end of that year maps to (March 1, Year 2); every later payment is the 1st of a month. Payments already made, the balance, the open payments (their due days are mapped) and the
// late fees stay as they are.
public static class CalendarConversion
{
    static GameCalendar Old => CalendarRules.Legacy;
    static GameCalendar New => CalendarRules.Real;

    // --- Days, minutes, ages ---

    // The same season and the same fraction through it, on the real calendar. A day before 0 counts as day 0.
    public static int MapDay(int oldDay)
    {
        oldDay = Math.Max(0, oldDay);
        int season = Old.SeasonIndexOf(oldDay);
        int intoSeason = oldDay - Old.SeasonStartDay(season);
        int length = New.SeasonLength(season);
        return New.SeasonStartDay(season) + intoSeason * length / Old.SeasonLength(season);
    }

    public static long MapMinute(long oldMinute)
    {
        if (oldMinute < 0)
            oldMinute = 0;
        long oldDay = oldMinute / GameCalendar.MinutesPerDay;
        return (long)MapDay((int)oldDay) * GameCalendar.MinutesPerDay + oldMinute % GameCalendar.MinutesPerDay;
    }

    // A day stamp that means "when it happened" (0 means no stamp and stays 0): the age in days is kept. Never below 1, so it cannot become the "no stamp" value.
    public static int KeepAge(int oldStamp, int oldToday, int newToday)
    {
        if (oldStamp <= 0)
            return oldStamp;
        int age = Math.Max(0, oldToday - oldStamp);
        return Math.Max(1, newToday - age);
    }

    // A felled tree's stamp (the day it was felled plus one; 0 means none): the age is scaled so the tree is the same fraction of the way to full size.
    public static int ScaleRegrowStamp(int oldStamp, int oldToday, int newToday)
    {
        if (oldStamp <= 0)
            return oldStamp;
        int age = Math.Max(0, oldToday - (oldStamp - 1));
        int newAge = (int)Math.Round(age * (double)New.DaysPerYear / Old.DaysPerYear);
        return Math.Max(1, newToday - newAge + 1);
    }

    // An absolute game hour (days x 24 + hour) at which something will be ready: the hours still to go are kept.
    public static int KeepHoursToGo(int oldReadyHour, int oldNowHour, int newNowHour)
    {
        if (oldReadyHour <= 0)
            return oldReadyHour;
        int toGo = Math.Max(0, oldReadyHour - oldNowHour);
        return newNowHour + toGo;
    }

    // --- A save's sections ---

    // Converts one section's JSON text (the text inside a save file's "data"). file and key are the section's address (world/time, player/loan ...). oldToday and hourOfDay are the saved
    // game's day and hour. log gets one line for each kind of thing changed; a section with nothing to convert comes back unchanged.
    public static string ConvertSection(string file, string key, string json, int oldToday, int hourOfDay, List<string> log)
    {
        int newToday = MapDay(oldToday);
        int oldNowHour = oldToday * GameCalendar.HoursPerDay + hourOfDay;
        int newNowHour = newToday * GameCalendar.HoursPerDay + hourOfDay;
        string address = file + "/" + key;
        string result = json;

        switch (address)
        {
            case "world/time":
                result = ReplaceInts(result, "totalDays", MapDay, address, "the clock (day)", log);
                break;

            case "player/loan":
                result = ReplaceInts(result, "nextDueDay", d => d < 0 ? d : New.MonthDayOnOrAfter(MapDay(d), 1), address, "the next payment's due day (now the 1st of a month)", log);
                result = ReplaceInts(result, "firstDueDay", MapDay, address, "the first payment's day", log);
                result = ReplaceInts(result, "dueDay", MapDay, address, "open payments' due days", log);
                result = ReplaceInts(result, "soonNoticeDue", d => -1, address, "the due-soon notice (reset)", log);
                result = Regex.Replace(result, "\"intervalDays\":-?\\d+", "\"intervalDays\":0,\"dueDayOfMonth\":1");
                log?.Add(address + ": payments moved to the monthly rule (the 1st)");
                break;

            case "player/ledger":
                result = ReplaceLongs(result, "startMinute", MapMinute, address, "statement periods' start", log);
                result = ReplaceLongs(result, "endMinute", m => m == 0 ? 0 : MapMinute(m), address, "statement periods' end", log);
                result = ReplaceInts(result, "daysPerSeason", d => 0, address, "ledger calendar (the game's real one)", log);
                break;

            case "player/market":
                result = ReplaceInts(result, "lastDay", MapDay, address, "the market's last processed day", log);
                break;

            case "player/survival":
                result = ReplaceInts(result, "collapseClockDay", d => KeepAge(d, oldToday, newToday), address, "last collapse day (age kept)", log);
                break;

            case "inventory/inventory":
                result = ReplaceInts(result, "acquiredDay", d => KeepAge(d, oldToday, newToday), address, "items' day acquired (age kept)", log);
                break;

            case "world/dropped":
                result = ReplaceInts(result, "acquiredDay", d => KeepAge(d, oldToday, newToday), address, "dropped items' day (age kept)", log);
                break;

            case "world/groundwood":
                result = ReplaceIntArrays(result, "takenDays", d => KeepAge(d, oldToday, newToday), address, "ground wood taken days (age kept)", log);
                break;

            case "world/grass":
                result = ReplaceIntArrays(result, "cutDays", d => KeepAge(d, oldToday, newToday), address, "tall grass cut days (age kept)", log);
                break;

            case "world/forage":
                result = ReplaceInts(result, "depletedDay", d => KeepAge(d, oldToday, newToday), address, "forage patches picked (age kept)", log);
                break;

            case "world/wood":
                result = ReplaceInts(result, "fellStamp", s => ScaleRegrowStamp(s, oldToday, newToday), address, "felled trees' regrowth (progress kept)", log);
                result = ReplaceInts(result, "builtDay", d => KeepAge(d, oldToday, newToday), address, "structures' day built (age kept)", log);
                result = ConvertWoodStacks(result, oldToday, newToday, oldNowHour, newNowHour, address, log);
                break;

            case "journal/journal":
                result = ReplaceInts(result, "day", MapDay, address, "journal dates", log);
                break;

            case "discovery/discovery":
                result = ReplaceInts(result, "day", MapDay, address, "discovery dates", log);
                result = ReplaceInts(result, "dayDiscovered", MapDay, address, "discovery dates", log);
                break;
        }
        return result;
    }

    // A wood pile's stacks: {"itemId":"logs","count":3,"day":38}. The day is when the batch was last made wet (age kept: wood seasoning is 28 plain days), except on a dry pottery piece,
    // where it is the absolute hour the piece is dry (the hours still to go are kept).
    static readonly Regex Stack = new Regex("\\{\"itemId\":\"([^\"]*)\",\"count\":(-?\\d+),\"day\":(-?\\d+)\\}");

    static string ConvertWoodStacks(string json, int oldToday, int newToday, int oldNowHour, int newNowHour, string address, List<string> log)
    {
        int days = 0, hours = 0;
        string result = Stack.Replace(json, m =>
        {
            string item = m.Groups[1].Value;
            int stamp = int.Parse(m.Groups[3].Value, CultureInfo.InvariantCulture);
            int converted;
            if (item.StartsWith("dry_", StringComparison.Ordinal))
            {
                converted = KeepHoursToGo(stamp, oldNowHour, newNowHour);
                if (converted != stamp)
                    hours++;
            }
            else
            {
                converted = KeepAge(stamp, oldToday, newToday);
                if (converted != stamp)
                    days++;
            }
            return "{\"itemId\":\"" + item + "\",\"count\":" + m.Groups[2].Value + ",\"day\":" + converted.ToString(CultureInfo.InvariantCulture) + "}";
        });
        if (days > 0)
            log?.Add($"{address}: {days} wood batches' day (age kept)");
        if (hours > 0)
            log?.Add($"{address}: {hours} dry pottery pieces' ready hour (hours to go kept)");
        return result;
    }

    // --- Small JSON helpers (JsonUtility writes compact JSON with no spaces) ---

    static string ReplaceInts(string json, string field, Func<int, int> convert, string address, string what, List<string> log)
    {
        int changed = 0;
        string result = Regex.Replace(json, "\"" + field + "\":(-?\\d+)", m =>
        {
            int old = int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
            int now = convert(old);
            if (now != old)
                changed++;
            return "\"" + field + "\":" + now.ToString(CultureInfo.InvariantCulture);
        });
        if (changed > 0)
            log?.Add($"{address}: {changed} x {what}");
        return result;
    }

    static string ReplaceLongs(string json, string field, Func<long, long> convert, string address, string what, List<string> log)
    {
        int changed = 0;
        string result = Regex.Replace(json, "\"" + field + "\":(-?\\d+)", m =>
        {
            long old = long.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
            long now = convert(old);
            if (now != old)
                changed++;
            return "\"" + field + "\":" + now.ToString(CultureInfo.InvariantCulture);
        });
        if (changed > 0)
            log?.Add($"{address}: {changed} x {what}");
        return result;
    }

    static string ReplaceIntArrays(string json, string field, Func<int, int> convert, string address, string what, List<string> log)
    {
        int changed = 0;
        string result = Regex.Replace(json, "\"" + field + "\":\\[([^\\]]*)\\]", m =>
        {
            string body = m.Groups[1].Value;
            if (body.Length == 0)
                return m.Value;
            string[] parts = body.Split(',');
            for (int i = 0; i < parts.Length; i++)
            {
                int old = int.Parse(parts[i], CultureInfo.InvariantCulture);
                int now = convert(old);
                if (now != old)
                    changed++;
                parts[i] = now.ToString(CultureInfo.InvariantCulture);
            }
            return "\"" + field + "\":[" + string.Join(",", parts) + "]";
        });
        if (changed > 0)
            log?.Add($"{address}: {changed} x {what}");
        return result;
    }
}
