using System;
using System.Collections.Generic;

// Money_System.md Phase B: the ledger and the seasonal account statement, pure and engine-free so edit-mode tests cover it. All money is
// long cents (Money); nothing here uses float or double. The ledger keeps TOTALS, not clicks: one amount per category per season. It is a
// cash-basis record: money is written when cash actually moves (a sale, a purchase, a loan payment), not when a debt is assessed; the
// statement's memo line "Owed at end" shows what was still owed. LedgerManager writes to it and closes seasons; the screen reads it.

// What money was for. Ids are saved, so they are APPEND-ONLY: never reorder or reuse one.
public enum LedgerCategory
{
    // Income: Trading Post sales, one per market group (the MarketGroup order), then goods the store sold stock back for.
    SalesWoodStone = 0,
    SalesFish = 1,
    SalesGameMeat = 2,
    SalesHides = 3,
    SalesForage = 4,
    SalesCrafted = 5,
    SalesFarm = 6,
    SalesStockedBack = 7,

    // Expenses: the land loan.
    LoanInterest = 8,
    LoanPrincipal = 9,
    LoanExtra = 10,
    LoanFees = 11,
    DownPayment = 12,

    // Expenses: Trading Post purchases by kind of goods.
    BuyTools = 13,
    BuyHuntingFishing = 14,
    BuyShelterBedding = 15,
    BuyCookingWaterFire = 16,
    BuyBuildingMaterials = 17,
    BuyOtherGoods = 18,
}

public static class LedgerCategories
{
    public const int Count = 19;
    public const int LastIncomeId = 7;

    public static bool IsIncome(LedgerCategory category) => (int)category <= LastIncomeId;

    public static string Name(LedgerCategory category)
    {
        switch (category)
        {
            case LedgerCategory.SalesWoodStone: return "Firewood, logs and stone";
            case LedgerCategory.SalesFish: return "Fish";
            case LedgerCategory.SalesGameMeat: return "Game meat";
            case LedgerCategory.SalesHides: return "Hides and furs";
            case LedgerCategory.SalesForage: return "Forage";
            case LedgerCategory.SalesCrafted: return "Crafted goods";
            case LedgerCategory.SalesFarm: return "Farm produce";
            case LedgerCategory.SalesStockedBack: return "Stocked goods sold back";
            case LedgerCategory.LoanInterest: return "Land payment: interest";
            case LedgerCategory.LoanPrincipal: return "Land payment: principal";
            case LedgerCategory.LoanExtra: return "Extra payment to principal";
            case LedgerCategory.LoanFees: return "Late fees";
            case LedgerCategory.DownPayment: return "Down payment";
            case LedgerCategory.BuyTools: return "Bought: tools";
            case LedgerCategory.BuyHuntingFishing: return "Bought: hunting and fishing";
            case LedgerCategory.BuyShelterBedding: return "Bought: shelter and bedding";
            case LedgerCategory.BuyCookingWaterFire: return "Bought: cooking, water, fire";
            case LedgerCategory.BuyBuildingMaterials: return "Bought: building materials";
            default: return "Bought: other goods";
        }
    }

    // The income category for a sale of an item of this market group.
    public static LedgerCategory ForSale(MarketGroup group)
    {
        switch (group)
        {
            case MarketGroup.WoodStone: return LedgerCategory.SalesWoodStone;
            case MarketGroup.Fish: return LedgerCategory.SalesFish;
            case MarketGroup.GameMeat: return LedgerCategory.SalesGameMeat;
            case MarketGroup.Hides: return LedgerCategory.SalesHides;
            case MarketGroup.Forage: return LedgerCategory.SalesForage;
            case MarketGroup.Crafted: return LedgerCategory.SalesCrafted;
            case MarketGroup.Farm: return LedgerCategory.SalesFarm;
            default: return LedgerCategory.SalesStockedBack;
        }
    }

    static readonly Dictionary<string, LedgerCategory> Purchases = new Dictionary<string, LedgerCategory>
    {
        { "axe", LedgerCategory.BuyTools }, { "hammer", LedgerCategory.BuyTools }, { "knife", LedgerCategory.BuyTools },
        { "fishing_rod", LedgerCategory.BuyHuntingFishing }, { "recurve_bow", LedgerCategory.BuyHuntingFishing },
        { "bolt_action_rifle", LedgerCategory.BuyHuntingFishing }, { "arrows", LedgerCategory.BuyHuntingFishing },
        { "rifle_rounds", LedgerCategory.BuyHuntingFishing },
        { "tent", LedgerCategory.BuyShelterBedding }, { "tarp", LedgerCategory.BuyShelterBedding },
        { "sleeping_bag", LedgerCategory.BuyShelterBedding },
        { "cooking_pot", LedgerCategory.BuyCookingWaterFire }, { "canteen", LedgerCategory.BuyCookingWaterFire },
        { "bucket", LedgerCategory.BuyCookingWaterFire }, { "flint_and_steel", LedgerCategory.BuyCookingWaterFire },
        { "lamp_oil", LedgerCategory.BuyCookingWaterFire },
        { "lumber", LedgerCategory.BuyBuildingMaterials }, { "cordage", LedgerCategory.BuyBuildingMaterials },
        { "weather_station", LedgerCategory.BuyOtherGoods },
    };

    // The expense category for buying a stocked item; anything unlisted (a future stock item) is "other goods".
    public static LedgerCategory ForPurchase(string itemId) =>
        itemId != null && Purchases.TryGetValue(itemId, out LedgerCategory category) ? category : LedgerCategory.BuyOtherGoods;

    public static bool PurchaseIsListed(string itemId) => itemId != null && Purchases.ContainsKey(itemId);
}

// One season of the ledger: the totals per category (always positive amounts; the category says whether it is income or an expense), the
// cash and loan at the start and end, and how long the ledger covers (a game started at 6:00 covers 27.75 days of its first season).
[Serializable]
public class LedgerSeason
{
    public int index;            // 0 is the first season of the game: Spring, Year 1
    public long startMinute;     // game minutes (TotalHours x 60) when the ledger began covering this season
    public long endMinute;       // game minutes it was closed at (not used while open)
    public long cashStart;
    public long cashEnd;         // closed seasons only
    public long loanEnd;         // loan balance when closed
    public long owedEnd;         // late fees and open payments still owed when closed
    public long[] cents = new long[LedgerCategories.Count];

    public void Normalize()
    {
        if (cents != null && cents.Length == LedgerCategories.Count)
            return;
        var fixedUp = new long[LedgerCategories.Count];
        if (cents != null)
            Array.Copy(cents, fixedUp, Math.Min(cents.Length, fixedUp.Length));
        cents = fixedUp;
    }

    public long Income
    {
        get
        {
            long total = 0;
            for (int i = 0; i <= LedgerCategories.LastIncomeId; i++)
                total = Money.Add(total, cents[i]);
            return total;
        }
    }

    public long Expenses
    {
        get
        {
            long total = 0;
            for (int i = LedgerCategories.LastIncomeId + 1; i < LedgerCategories.Count; i++)
                total = Money.Add(total, cents[i]);
            return total;
        }
    }
}

// What a statement row (or a totals row) says, worked out from the ledger. Newest first in StatementRows.
public struct StatementRow
{
    public int index;            // the season, or -1 for a totals row
    public bool inProgress;      // the season is still open: its figures are so far
    public long minutes;         // how long the row covers
    public long cashStart, income, expenses, cashEnd, loanEnd, owedEnd;
    public long[] cents;         // by category

    public long Net => Money.Sub(income, expenses);

    // The difference between what the ledger explains and the real cash: zero in a normal game. Debug money (F12) lands here.
    public long Other => Money.Sub(cashEnd, Money.Add(cashStart, Money.Sub(income, expenses)));
}

// How much a figure moved from the season before.
public struct LedgerChange
{
    public long cents;
    public bool hasPercent;      // false when the season before was zero
    public int basisPoints;      // 10000 = 100%
}

// The ledger in a save (player file, key "ledger"). Version 0 (no section) means no ledger: it starts at the first ProcessNow.
[Serializable]
public struct LedgerSaveData
{
    public const int CurrentVersion = 1;

    public int version;
    public int daysPerSeason;
    public bool started;
    public bool unread;
    public LedgerSeason open;
    public LedgerSeason[] closed;
}

public sealed class LedgerState
{
    readonly List<LedgerSeason> closed = new List<LedgerSeason>();

    public int DaysPerSeason { get; private set; } = 28;
    public bool Started { get; private set; }

    // A season has closed that the player has not looked at yet (the "statement ready" flag).
    public bool Unread { get; set; }

    public LedgerSeason Open { get; private set; }
    public int OpenIndex => Open != null ? Open.index : -1;
    public IReadOnlyList<LedgerSeason> Closed => closed;

    public const int MinutesPerDay = 1440;

    // --- Time ---

    // The season a game day is in, counting from 0 (Spring, Year 1). Days before 0 count as the first season.
    public static int SeasonOf(int day, int daysPerSeason)
    {
        if (daysPerSeason < 1)
            daysPerSeason = 1;
        return Math.Max(0, day) / daysPerSeason;
    }

    public static long SeasonStartMinute(int index, int daysPerSeason) => (long)index * Math.Max(1, daysPerSeason) * MinutesPerDay;

    public static long SeasonEndMinute(int index, int daysPerSeason) => SeasonStartMinute(index + 1, daysPerSeason);

    public static int YearOf(int index) => index / 4 + 1;

    public static string SeasonLabel(int index) => $"{MarketRules.SeasonName(index % 4)}, Year {YearOf(index)}";

    // --- Starting, recording, closing ---

    public void Clear()
    {
        closed.Clear();
        Open = null;
        Started = false;
        Unread = false;
    }

    // Starts the ledger: cash at the start is openingCash (a new game's full savings, or an old save's cash when it loads), and the
    // open season is the one nowMinute is in, covered from nowMinute on.
    public void Begin(long openingCashCents, int seasonIndex, long nowMinute, int daysPerSeason)
    {
        Clear();
        DaysPerSeason = Math.Max(1, daysPerSeason);
        Started = true;
        Open = new LedgerSeason { index = Math.Max(0, seasonIndex), startMinute = nowMinute, cashStart = openingCashCents };
    }

    // Adds money that moved to the open season. Ignores anything that isn't positive, or if the ledger hasn't started.
    public void Record(LedgerCategory category, long cents)
    {
        if (!Started || cents <= 0)
            return;
        int id = (int)category;
        if (id < 0 || id >= LedgerCategories.Count)
            return;
        Open.cents[id] = Money.Add(Open.cents[id], cents);
    }

    // Closes every season before todaySeasonIndex, with the cash, loan balance and amount owed as they are now (a skip over several
    // seasons gives every one the same figures: nothing could change in between). Returns how many it closed.
    public int CloseThrough(int todaySeasonIndex, long cashNow, long loanBalanceNow, long owedNow)
    {
        if (!Started)
            return 0;
        int count = 0;
        while (Open.index < todaySeasonIndex)
        {
            LedgerSeason season = Open;
            season.endMinute = SeasonEndMinute(season.index, DaysPerSeason);
            season.cashEnd = cashNow;
            season.loanEnd = loanBalanceNow;
            season.owedEnd = owedNow;
            closed.Add(season);
            Open = new LedgerSeason { index = season.index + 1, startMinute = season.endMinute, cashStart = cashNow };
            count++;
        }

        if (count > 0)
            Unread = true;
        return count;
    }

    // --- The statement ---

    static StatementRow RowOf(LedgerSeason season, bool inProgress, long minutes, long cashEnd, long loanEnd, long owedEnd) => new StatementRow
    {
        index = season.index,
        inProgress = inProgress,
        minutes = Math.Max(0, minutes),
        cashStart = season.cashStart,
        income = season.Income,
        expenses = season.Expenses,
        cashEnd = cashEnd,
        loanEnd = loanEnd,
        owedEnd = owedEnd,
        cents = season.cents,
    };

    // Every season since the start, newest first. The open season comes first, marked in progress, with the cash, loan balance and
    // amount owed now as its end figures.
    public List<StatementRow> Rows(long cashNow, long loanNow, long owedNow, long nowMinute)
    {
        var rows = new List<StatementRow>();
        if (!Started)
            return rows;
        rows.Add(RowOf(Open, true, nowMinute - Open.startMinute, cashNow, loanNow, owedNow));
        for (int i = closed.Count - 1; i >= 0; i--)
        {
            LedgerSeason s = closed[i];
            rows.Add(RowOf(s, false, s.endMinute - s.startMinute, s.cashEnd, s.loanEnd, s.owedEnd));
        }
        return rows;
    }

    // One totals row over the rows that match. Cash at the start is the oldest matching season's, and the end figures are the newest's.
    public static StatementRow Total(IList<StatementRow> newestFirst, Func<StatementRow, bool> include)
    {
        var total = new StatementRow { index = -1, cents = new long[LedgerCategories.Count] };
        bool any = false;
        for (int i = 0; i < newestFirst.Count; i++)
        {
            StatementRow row = newestFirst[i];
            if (!include(row))
                continue;
            if (!any)
            {
                total.cashEnd = row.cashEnd;
                total.loanEnd = row.loanEnd;
                total.owedEnd = row.owedEnd;
                total.inProgress = row.inProgress;
                any = true;
            }
            total.cashStart = row.cashStart; // ends as the oldest match's
            total.minutes += row.minutes;
            total.income = Money.Add(total.income, row.income);
            total.expenses = Money.Add(total.expenses, row.expenses);
            for (int c = 0; c < LedgerCategories.Count && row.cents != null; c++)
                total.cents[c] = Money.Add(total.cents[c], row.cents[c]);
        }
        return total;
    }

    public static StatementRow ThisYear(IList<StatementRow> newestFirst, int yearOfNow) =>
        Total(newestFirst, r => YearOf(r.index) == yearOfNow);

    public static StatementRow SinceStart(IList<StatementRow> newestFirst) => Total(newestFirst, r => true);

    // Money per game day: cents over the minutes covered, in cents, half up (half away from zero for a loss). 0 when nothing is covered.
    public static long PerGameDay(long cents, long minutes)
    {
        if (minutes <= 0)
            return 0;
        long scaled = Money.Mul(Math.Abs(cents), MinutesPerDay);
        long perDay = Money.Add(scaled, minutes / 2) / minutes;
        return cents < 0 ? Money.Negate(perDay) : perDay;
    }

    // "27.75 days": minutes as game days, two decimals.
    public static string DaysText(long minutes)
    {
        minutes = Math.Max(0, minutes);
        return $"{minutes / MinutesPerDay}.{minutes % MinutesPerDay * 100 / MinutesPerDay:00} days";
    }

    // How a figure moved from the season before: the change in cents and, when the season before was not zero, in basis points.
    public static LedgerChange ChangeFrom(long now, long before)
    {
        long delta = Money.Sub(now, before);
        var change = new LedgerChange { cents = delta };
        if (before == 0)
            return change;
        long scaled = Money.Mul(Math.Abs(delta), 10000);
        long bp = Money.Add(scaled, Math.Abs(before) / 2) / Math.Abs(before);
        change.hasPercent = true;
        change.basisPoints = (int)Math.Min(int.MaxValue, delta < 0 ? -bp : bp);
        return change;
    }

    // The statement shows a figure in red when it is below zero (an expense is shown as a negative, so it is red); zero is black.
    public static bool IsRed(long signedCents) => signedCents < 0;

    // --- Saving ---

    public LedgerSaveData Capture() => new LedgerSaveData
    {
        version = LedgerSaveData.CurrentVersion,
        daysPerSeason = DaysPerSeason,
        started = Started,
        unread = Unread,
        open = Started ? Open : null,
        closed = closed.ToArray(),
    };

    // Loads a save. A save with no ledger (version 0), or one that never started, leaves a blank ledger that starts at the next
    // ProcessNow. Amount arrays are brought to the current number of categories (a later build may add some).
    public void Restore(LedgerSaveData data)
    {
        Clear();
        if (data.version < LedgerSaveData.CurrentVersion || !data.started || data.open == null)
            return;

        DaysPerSeason = Math.Max(1, data.daysPerSeason);
        Open = data.open;
        Open.Normalize();
        if (data.closed != null)
        {
            foreach (LedgerSeason season in data.closed)
            {
                if (season == null)
                    continue;
                season.Normalize();
                closed.Add(season);
            }
        }

        Started = true;
        Unread = data.unread;
    }
}
