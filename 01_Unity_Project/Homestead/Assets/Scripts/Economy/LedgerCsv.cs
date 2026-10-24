using System.Collections.Generic;
using System.Globalization;
using System.Text;

// Money_System.md Phase D1's measuring export: the ledger as CSV text, one row per season, oldest first. Pure and engine-free so edit-mode
// tests cover it; LedgerManager writes the file (development builds only). Money is dollars with two decimals and no thousands comma,
// computed from cents with integers only. Real minutes is empty when the season has no recorded real time.
public static class LedgerCsv
{
    public static string Header()
    {
        var columns = new List<string> { "Season", "Days covered", "Real minutes" };
        for (int c = 0; c <= LedgerCategories.LastIncomeId; c++)
            columns.Add("Income: " + LedgerCategories.Name((LedgerCategory)c));
        columns.Add("Total income");
        for (int c = LedgerCategories.LastIncomeId + 1; c < LedgerCategories.Count; c++)
            columns.Add("Expense: " + LedgerCategories.Name((LedgerCategory)c));
        columns.Add("Total expenses");
        columns.Add("Net");
        columns.Add("Cash at end");
        columns.Add("Loan balance at end");
        columns.Add("Owed at end");
        return Join(columns);
    }

    // The whole file for the statement rows (newest first, as LedgerManager.Rows gives them; written oldest first). Lines end in \n.
    public static string Build(IList<StatementRow> newestFirst)
    {
        var text = new StringBuilder();
        text.Append(Header()).Append('\n');
        for (int i = newestFirst.Count - 1; i >= 0; i--)
            text.Append(RowLine(newestFirst[i])).Append('\n');
        return text.ToString();
    }

    public static string RowLine(StatementRow row)
    {
        var columns = new List<string>
        {
            LedgerState.SeasonLabel(row.index) + (row.inProgress ? " (so far)" : ""),
            DaysCovered(row.minutes),
            LedgerState.HasRealTime(row.realMillis) ? MinutesOneDecimal(row.realMillis) : "",
        };
        for (int c = 0; c <= LedgerCategories.LastIncomeId; c++)
            columns.Add(Dollars(Cent(row, c)));
        columns.Add(Dollars(row.income));
        for (int c = LedgerCategories.LastIncomeId + 1; c < LedgerCategories.Count; c++)
            columns.Add(Dollars(Cent(row, c)));
        columns.Add(Dollars(row.expenses));
        columns.Add(Dollars(row.Net));
        columns.Add(Dollars(row.cashEnd));
        columns.Add(Dollars(row.loanEnd));
        columns.Add(Dollars(row.owedEnd));
        return Join(columns);
    }

    static long Cent(StatementRow row, int category) => row.cents != null && category < row.cents.Length ? row.cents[category] : 0L;

    // Cents as dollars with two decimals, no symbol and no thousands comma: 123456 -> "1234.56", -7500 -> "-75.00".
    public static string Dollars(long cents)
    {
        bool negative = cents < 0;
        ulong magnitude = negative ? (ulong)(-(cents + 1)) + 1UL : (ulong)cents;
        string text = (magnitude / 100UL).ToString(CultureInfo.InvariantCulture) + "." + (magnitude % 100UL).ToString("00", CultureInfo.InvariantCulture);
        return negative ? "-" + text : text;
    }

    // Game minutes as days, two decimals ("27.75"), the number behind LedgerState.DaysText.
    static string DaysCovered(long minutes)
    {
        minutes = System.Math.Max(0, minutes);
        return $"{minutes / LedgerState.MinutesPerDay}.{minutes % LedgerState.MinutesPerDay * 100 / LedgerState.MinutesPerDay:00}";
    }

    static string MinutesOneDecimal(long realMillis)
    {
        long tenths = Money.Add(realMillis, 3000) / 6000;
        return $"{tenths / 10}.{tenths % 10}";
    }

    // A field with a comma, a quote or a line break is wrapped in quotes, with inner quotes doubled.
    public static string Escape(string field)
    {
        if (field == null)
            return "";
        if (field.IndexOfAny(new[] { ',', '"', '\n', '\r' }) < 0)
            return field;
        return "\"" + field.Replace("\"", "\"\"") + "\"";
    }

    static string Join(List<string> columns)
    {
        for (int i = 0; i < columns.Count; i++)
            columns[i] = Escape(columns[i]);
        return string.Join(",", columns);
    }
}
