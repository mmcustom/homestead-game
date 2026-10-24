using System.Globalization;

// Money_System.md Phase D1: the four things that differ between difficulty tiers, in one table, pure and engine-free so edit-mode tests
// cover it. Money is long cents and every rate is an integer in basis points (10000 = 100%); nothing here uses float or double.
// D1 holds TODAY'S values on every tier, so nothing in play changes; D2 changes only the numbers in the table below.
public struct TierTerms
{
    public int missedBeforeLoss;   // missed payments open at once that lose the game
    public long lateFeeCents;      // added to a payment that is still unpaid after its grace days
    public int rateBasisPoints;    // the land loan's yearly interest: 800 = 8%
    public int sellBasisPoints;    // what the Trading Post pays on a sale total: 10000 = 100%

    public bool IsValid =>
        missedBeforeLoss >= 1 && missedBeforeLoss <= 10 &&
        lateFeeCents >= 0 && lateFeeCents <= 100000L &&
        rateBasisPoints >= 0 && rateBasisPoints <= 3000 &&
        sellBasisPoints >= 1 && sellBasisPoints <= MarketRules.MaxBp;
}

public static class TierRules
{
    public const int TierCount = 3;
    public const int Settler = 1;            // the tier an unknown difficulty (a save from before difficulty existed) plays as
    public const int FullSellBasisPoints = 10000;

    // Index = the Difficulty enum as a number: Homesteader 0, Settler 1, Pioneer 2 (the enum lives in the main assembly). One row per
    // tier, so changing a tier's terms is changing one line here.
    static readonly TierTerms[] Table =
    {
        new TierTerms { missedBeforeLoss = 3, lateFeeCents = 7500, rateBasisPoints = 800, sellBasisPoints = 10000 }, // Homesteader
        new TierTerms { missedBeforeLoss = 3, lateFeeCents = 7500, rateBasisPoints = 800, sellBasisPoints = 10000 }, // Settler
        new TierTerms { missedBeforeLoss = 3, lateFeeCents = 7500, rateBasisPoints = 800, sellBasisPoints = 10000 }, // Pioneer
    };

    // The terms of a tier. A number outside 0-2 gets the Settler row.
    public static TierTerms For(int difficulty) => Table[difficulty >= 0 && difficulty < Table.Length ? difficulty : Settler];

    // The loan terms a new loan on this tier starts with: the base terms (payments, spacing, first due day, grace days) with the tier's
    // rate and late fee. The result is saved with the loan, so a save keeps the terms it started with.
    public static LoanTerms LoanTermsFor(LoanTerms baseTerms, TierTerms tier)
    {
        baseTerms.rateBasisPoints = tier.rateBasisPoints;
        baseTerms.lateFeeCents = tier.lateFeeCents;
        return baseTerms;
    }

    // What a Trading Post sale pays after the tier's share: the total times basisPoints / 10000, rounded half up to the cent.
    // 10000 returns the total unchanged.
    public static long ApplySell(long totalCents, int basisPoints)
    {
        if (totalCents <= 0 || basisPoints <= 0)
            return 0;
        if (basisPoints == FullSellBasisPoints)
            return totalCents;
        return Money.Add(Money.Mul(totalCents, basisPoints), FullSellBasisPoints / 2) / FullSellBasisPoints;
    }

    // "100%", "90%", "105.5%": basis points as a percentage, up to two decimals, whatever the machine's culture.
    public static string PercentText(int basisPoints)
    {
        decimal percent = basisPoints / 100m;
        return percent.ToString("0.##", CultureInfo.InvariantCulture) + "%";
    }

    public static string MissedLine(int threshold) => threshold == 1 ? "1 missed payment" : threshold + " missed payments";
}
