using System;

// One starter parcel (Money_System.md, Land Price): Bethel, Ohio area numbers, rounded.
public struct Parcel
{
    public string name;
    public int acres;
    public long priceCents;

    public Parcel(int acres, long priceDollars)
    {
        this.acres = acres;
        name = acres + (acres == 1 ? " acre" : " acres");
        priceCents = Money.Dollars(priceDollars);
    }

    public long PricePerAcreCents => acres > 0 ? priceCents / acres : 0;
}

// What the player picked at a new game: the parcel, and how the savings split between the down payment and the cash reserve.
public struct LandChoice
{
    public int parcelIndex;
    public long downCents;
    public long reserveCents;
}

// New Game's land purchase rules, engine-free so tests cover them: the three starter parcels, the savings each difficulty starts
// with, and the down payment and cash reserve split. Savings are a percentage of the land price by tier, capped for every tier.
public static class LandRules
{
    public static readonly Parcel[] Starters =
    {
        new Parcel(5, 60000),
        new Parcel(10, 140000),
        new Parcel(40, 450000),
    };

    public const long SavingsCapCents = 3000000; // $30,000

    // Savings as a percentage of the land price, by tier: Homesteader 40, Settler 20, Pioneer none. Tier numbers follow the
    // Difficulty enum (Homesteader 0, Settler 1, Pioneer 2).
    public static int SavingsPercent(int tier) => tier == 0 ? 40 : tier == 1 ? 20 : 0;

    public static long SavingsCents(long priceCents, int percent)
    {
        if (priceCents <= 0 || percent <= 0)
            return 0;
        return Math.Min(SavingsCapCents, priceCents / 100 * percent);
    }

    // Half and half; any odd cent goes to the reserve.
    public static long DefaultDown(long savingsCents) => savingsCents / 2;

    public static bool ValidDown(long savingsCents, long downCents) => downCents >= 0 && downCents <= savingsCents;

    public static long Reserve(long savingsCents, long downCents) => savingsCents - downCents;

    public static LandChoice Choose(int parcelIndex, long savingsCents, long downCents) => new LandChoice
    {
        parcelIndex = parcelIndex,
        downCents = downCents,
        reserveCents = Reserve(savingsCents, downCents),
    };
}
