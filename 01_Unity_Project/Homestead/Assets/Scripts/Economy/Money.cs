using System;
using System.Globalization;

// All money in the game is a long number of cents, and every conversion, display and sum goes through here so no other script does
// its own money math. Never float or double. Sums and products saturate at the ends of the long range instead of wrapping, so a
// huge balance can never turn negative. Lives in the engine-free Homestead.Economy assembly so edit-mode tests can cover it.
public static class Money
{
    public const long CentsPerDollar = 100;

    // Trading Post rule, unchanged: a stocked item is bought back at 40% of its dollar price rounded down, at least $1, and
    // never when it is stocked under $3.
    public const long MinBuyBackStockCents = 3 * CentsPerDollar;

    public static long Dollars(long dollars) => Mul(dollars, CentsPerDollar);

    public static long Add(long a, long b)
    {
        long sum = unchecked(a + b);
        if (((a ^ sum) & (b ^ sum)) < 0)
            return a < 0 ? long.MinValue : long.MaxValue;
        return sum;
    }

    public static long Negate(long a) => a == long.MinValue ? long.MaxValue : -a;

    public static long Sub(long a, long b) => Add(a, Negate(b));

    public static long Mul(long cents, long times)
    {
        if (cents == 0 || times == 0)
            return 0;
        try
        {
            return checked(cents * times);
        }
        catch (OverflowException)
        {
            return (cents < 0) == (times < 0) ? long.MaxValue : long.MinValue;
        }
    }

    // "$1,234.50", "$12.00", "$0.00", "-$75.00". Always two decimals, always a comma every three digits, whatever the machine's culture.
    public static string Format(long cents)
    {
        bool negative = cents < 0;
        ulong magnitude = negative ? (ulong)(-(cents + 1)) + 1UL : (ulong)cents;
        string text = (magnitude / 100UL).ToString("N0", CultureInfo.InvariantCulture) + "." +
                      (magnitude % 100UL).ToString("00", CultureInfo.InvariantCulture);
        return negative ? "-$" + text : "$" + text;
    }

    // Reads what a player typed as an amount: "12", "12.5", "12.50", "$1,250.00". No sign, at most two decimals, digits only.
    public static bool TryParseCents(string text, out long cents)
    {
        cents = 0;
        if (string.IsNullOrWhiteSpace(text))
            return false;

        string clean = text.Replace("$", "").Replace(",", "").Replace(" ", "").Trim();
        if (clean.Length == 0)
            return false;

        string whole = clean, fraction = "";
        int dot = clean.IndexOf('.');
        if (dot >= 0)
        {
            whole = clean.Substring(0, dot);
            fraction = clean.Substring(dot + 1);
        }

        if (whole.Length == 0 && fraction.Length == 0)
            return false;
        if (whole.Length > 15 || fraction.Length > 2)
            return false;
        foreach (char c in whole + fraction)
            if (c < '0' || c > '9')
                return false;

        long dollars = whole.Length == 0 ? 0 : long.Parse(whole, CultureInfo.InvariantCulture);
        long extra = fraction.Length == 0 ? 0 : long.Parse(fraction.PadRight(2, '0'), CultureInfo.InvariantCulture);
        cents = Add(Dollars(dollars), extra);
        return true;
    }

    // What the store pays for one of an item it stocks at buyCents (0: it doesn't buy it back).
    public static long SellBackCents(long buyCents)
    {
        if (buyCents < MinBuyBackStockCents)
            return 0;
        long dollars = buyCents / CentsPerDollar * 40 / 100; // 40% of the whole-dollar price, rounded down
        return Dollars(Math.Max(1, dollars));
    }

    // How many of an item to buy: what was asked for, held back by the money and by the room to carry them.
    public static int BuyCount(long cents, long priceCents, int wanted, int space)
    {
        if (priceCents <= 0 || wanted <= 0 || space <= 0 || cents <= 0)
            return 0;
        return (int)Math.Min(wanted, Math.Min(space, cents / priceCents));
    }
}

// The Trading Post's money in a save (player file, key "money"). Version 0 is a save from before cents existed, where money is the
// whole-dollar int; version 1 holds cents. The old field is still written (whole dollars, clamped) but only read from version 0.
[Serializable]
public struct TradingPostSaveData
{
    public const int CentsVersion = 1;

    public int version;
    public int money;
    public long cents;

    public static TradingPostSaveData FromCents(long cents) => new TradingPostSaveData
    {
        version = CentsVersion,
        money = (int)Math.Max(int.MinValue, Math.Min(int.MaxValue, cents / Money.CentsPerDollar)),
        cents = cents,
    };

    public long ToCents() => version >= CentsVersion ? cents : Money.Dollars(money);
}
