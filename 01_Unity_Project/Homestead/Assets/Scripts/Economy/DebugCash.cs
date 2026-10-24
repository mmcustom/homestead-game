// Testing only (TimeDebugControls, PageUp): the amounts and messages of the debug cash keys. Pure so a test can pin them. The cash itself
// moves through TradingPost.ResetMoney and nothing is written to the ledger, so debug cash is never income (it only shows as the
// statement's "Other" line).
public static class DebugCash
{
    public const long StepCents = 1000L * Money.CentsPerDollar; // one press of the add key
    public const long ZeroCents = 0L;                           // the set-to-zero key

    public static long After(long cash) => Money.Add(cash, StepCents);

    public static string AddedMessage(long cashAfter) => $"Debug: +{Money.Format(StepCents)} (cash {Money.Format(cashAfter)})";

    public const string ZeroedMessage = "Debug: cash set to $0";
}
