using System;
using System.Collections.Generic;

// Slice P1, the phone (Modern_Systems_and_Services.md, "The phone, bills and contractors"; Mike's answers 2026-11-27). Pure and engine-free so edit-mode tests cover it.
//
// The player starts with a smartphone. With no service plan it shows the time and date and "No Service"; the one plan is $25 a month (unlimited talk, text and data,
// the real 2026 cheap prepaid price), paid when it is started and then on the 15th of every month. A bill that can't be paid suspends the service until it is paid;
// nothing else is lost (only the land loan can end the game). Cancelling keeps the service to the next bill day and then ends it with nothing more to pay.
public static class PhoneRules
{
    public const string PhoneId = "smartphone";
    public const long PlanCents = 2500;
    public const int BillDayOfMonth = 15;
    public const int HistoryKept = 6;

    // The first bill day after today (the 15th of this month if today is before it, otherwise next month's).
    public static int NextBillDay(GameCalendar calendar, int today) => calendar.MonthDayOnOrAfter(today + 1, BillDayOfMonth);

    // --- The plan ---

    public static bool HasService(PhoneState s) => s != null && s.active && !s.suspended;

    // Starts the plan: the first month is paid now. Returns false (nothing changes) when it can't be paid or the plan is already on.
    public static bool SignUp(PhoneState s, GameCalendar calendar, int today, Func<long, bool> pay)
    {
        if (s.active && !s.cancelling)
            return false;
        if (s.active && s.cancelling)
        {
            s.cancelling = false; // still paid up to the next bill day: keeping it costs nothing now
            return true;
        }
        if (!pay(PlanCents))
            return false;
        s.active = true;
        s.suspended = false;
        s.cancelling = false;
        s.owedCents = 0;
        s.nextBillDay = NextBillDay(calendar, today);
        Note(s, today, PlanCents, true);
        return true;
    }

    // Cancels: the service runs to the next bill day, then ends with nothing more to pay. A suspended plan ends at once and its debt is dropped.
    public static void Cancel(PhoneState s)
    {
        if (!s.active)
            return;
        if (s.suspended)
        {
            End(s);
            return;
        }
        s.cancelling = true;
    }

    // Pays a suspended plan's bill: the service comes back and the next bill is the next 15th.
    public static bool PayOverdue(PhoneState s, GameCalendar calendar, int today, Func<long, bool> pay)
    {
        if (!s.active || !s.suspended || !pay(s.owedCents))
            return false;
        Note(s, today, s.owedCents, true);
        s.suspended = false;
        s.owedCents = 0;
        s.nextBillDay = NextBillDay(calendar, today);
        return true;
    }

    // The bill days that have come by today: each is paid if pay can, or the plan is suspended (and stays so, with nothing more added, until it is paid).
    // Returns what happened, oldest first.
    public static List<BillResult> Advance(PhoneState s, GameCalendar calendar, int today, Func<long, bool> pay)
    {
        var results = new List<BillResult>();
        while (s.active && !s.suspended && s.nextBillDay <= today)
        {
            int day = s.nextBillDay;
            if (s.cancelling)
            {
                End(s);
                results.Add(new BillResult { day = day, ended = true });
                break;
            }
            if (pay(PlanCents))
            {
                Note(s, day, PlanCents, true);
                results.Add(new BillResult { day = day, cents = PlanCents, paid = true });
                s.nextBillDay = NextBillDay(calendar, day);
            }
            else
            {
                s.suspended = true;
                s.owedCents = PlanCents;
                Note(s, day, PlanCents, false);
                results.Add(new BillResult { day = day, cents = PlanCents, paid = false });
            }
        }
        return results;
    }

    static void End(PhoneState s)
    {
        s.active = false;
        s.suspended = false;
        s.cancelling = false;
        s.owedCents = 0;
    }

    static void Note(PhoneState s, int day, long cents, bool paid)
    {
        s.history.Add(new BillRecord { day = day, cents = cents, paid = paid });
        while (s.history.Count > HistoryKept)
            s.history.RemoveAt(0);
    }

    // --- Words ---

    public const string NoService = "No Service";
    public const string NoServiceHelp = "No service - open Bills & Plan to start a plan";
    public const string NoPhone = "You don't have your phone";
    public const string NeedsService = "That needs service - start a plan in Bills & Plan";
    public static string PlanLine => $"Unlimited talk, text and data - {Money.Format(PlanCents)} a month, billed on the {BillDayOfMonth}th";
    public static string Status(PhoneState s) =>
        s == null || !s.active ? "No plan" : s.suspended ? $"Suspended - {Money.Format(s.owedCents)} overdue" : s.cancelling ? "Cancelled - service ends on the next bill day" : "Active";
    public static string Suspended(long cents) => $"Phone bill of {Money.Format(cents)} unpaid - service suspended until you pay it (phone, P)";
    public static string Paid(long cents) => $"Phone bill paid: {Money.Format(cents)}";
    public const string Ended = "Your phone plan has ended";

    // "8:05 AM"
    public static string Clock(int hour, int minute)
    {
        int h = ((hour % 24) + 24) % 24;
        int twelve = h % 12 == 0 ? 12 : h % 12;
        return $"{twelve}:{minute:00} {(h < 12 ? "AM" : "PM")}";
    }

    // --- The home screen's apps ---

    // id, label, needs service. Mike's own games and app sit after the four that work (free advertising, Mike 2026-11-27).
    public static readonly (string id, string label, bool needsService)[] Apps =
    {
        ("contractors", "Contractors", true),
        ("weather", "Weather", true),
        ("bills", "Bills & Plan", false),
        ("market", "Market", true),
        ("power", "Power", true),
        ("brandys_bakery", "Brandy's Bakery", true),
        ("sweet_swap", "Sweet Swap", true),
        ("fast_food_frenzy", "Fast Food Frenzy", true),
        ("starforge", "Starforge", true),
        ("my_tech_app", "My Tech App", true),
    };

    public static bool IsAd(string id) => id == "brandys_bakery" || id == "sweet_swap" || id == "fast_food_frenzy" || id == "starforge" || id == "my_tech_app";

    public static string AdLine(string id) =>
        id == "brandys_bakery" ? "A cozy bakery game by Mike Mack" :
        id == "sweet_swap" ? "A sweet match-three puzzle game by Mike Mack" :
        id == "fast_food_frenzy" ? "A fast food restaurant game by Mike Mack" :
        id == "starforge" ? "An idle mining game by Mike Mack" :
        id == "my_tech_app" ? "The field app for MM Custom Wiring" : "";
}

[Serializable]
public class PhoneState
{
    public bool active;       // a plan was started and not ended
    public bool suspended;    // a bill went unpaid: no service until it is paid
    public bool cancelling;   // cancelled: ends on the next bill day
    public int nextBillDay;   // the absolute game day of the next bill
    public long owedCents;    // what a suspended plan owes
    public List<BillRecord> history = new List<BillRecord>();
}

[Serializable]
public class BillRecord
{
    public int day;
    public long cents;
    public bool paid;
}

public struct BillResult
{
    public int day;
    public long cents;
    public bool paid;
    public bool ended;
}
