// Quantity_Controls.md's amount rules, pure and engine-free (integers only) so edit-mode tests cover them. The screens work out what each
// limit is (cash, carry weight, room, what is carried) with their own code and pass the numbers in; this keeps the stepper's maths in one
// place: an amount is at least 1 and at most the most that can move, and 0 means nothing can move, so the control is greyed out.
public static class QuantityRules
{
    // The most digits a typed amount keeps, and the largest amount there is.
    public const int MaxDigits = 6;
    public const int MaxAmount = 999999;

    // The amount kept in range: at least 1 and at most max; 0 when max is 0 (or less), meaning nothing can move.
    public static int Clamp(int amount, int max)
    {
        if (max <= 0)
            return 0;
        if (max > MaxAmount)
            max = MaxAmount;
        if (amount < 1)
            return 1;
        return amount > max ? max : amount;
    }

    // The amount after a plus or minus click, kept in range. delta is +1 or -1 (or 10 with Shift).
    public static int Step(int amount, int delta, int max)
    {
        long next = (long)amount + delta;
        if (next > MaxAmount)
            next = MaxAmount;
        if (next < 0)
            next = 0;
        return Clamp((int)next, max);
    }

    // How far a plus or minus click moves the amount.
    public static int StepFor(bool shiftHeld) => shiftHeld ? 10 : 1;

    // Reads a typed amount: digits only (spaces around them are ignored), leading zeros stripped, more than MaxDigits digits capped at
    // MaxAmount. Returns false for anything else (empty, a letter, a sign, a decimal point) so the caller can ignore it. "0" is a valid
    // 0, which Clamp lifts to 1.
    public static bool TryParse(string text, out int amount)
    {
        amount = 0;
        if (text == null)
            return false;

        string clean = text.Trim();
        if (clean.Length == 0)
            return false;
        foreach (char c in clean)
            if (c < '0' || c > '9')
                return false;

        clean = clean.TrimStart('0');
        if (clean.Length == 0)
            return true;      // all zeros
        if (clean.Length > MaxDigits)
        {
            amount = MaxAmount;
            return true;
        }

        amount = int.Parse(clean);
        return true;
    }

    // A typed amount applied: the parsed number kept in range, or the current amount (kept in range) if the text isn't a number.
    public static int ParseAndClamp(string text, int current, int max) =>
        Clamp(TryParse(text, out int parsed) ? parsed : current, max);

    // How many whole units fit in a space: free / each, never more than MaxAmount. Both in the same small unit (the lantern tank in
    // hundredths of an hour). Nothing fits when there is no free space; a unit that takes no room has no limit.
    public static int UnitsThatFit(long free, long each)
    {
        if (free <= 0)
            return 0;
        if (each <= 0)
            return MaxAmount;
        long units = free / each;
        return units > MaxAmount ? MaxAmount : (int)units;
    }

    // The most that can move: the smallest of the limits (what is available, what can be afforded, what can be carried, what has room).
    // Any limit of 0 or less means nothing can move; no limits at all means none either. Never more than MaxAmount.
    public static int MaxOf(params int[] limits)
    {
        if (limits == null || limits.Length == 0)
            return 0;

        int most = MaxAmount;
        foreach (int limit in limits)
        {
            if (limit <= 0)
                return 0;
            if (limit < most)
                most = limit;
        }

        return most;
    }
}
