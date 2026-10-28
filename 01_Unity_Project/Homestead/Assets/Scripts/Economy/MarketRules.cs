using System;
using System.Collections.Generic;
using System.Text;

// Trading_Post_Market.md's price rules, pure and engine-free so edit-mode tests cover them. Every multiplier is an integer in basis points
// (10000 = 1.0), every price is whole cents (Money), and nothing here uses float or double.
public static class MarketRules
{
    public const int One = 10000;
    public const int MaxBp = 20000;       // the multiplier never goes above 2.0 (and never below its group's floor)
    public const int SnapBp = 10;         // recovery lands exactly on the target once the gap is under this

    // 0 Spring, 1 Summer, 2 Fall, 3 Winter (the Season enum's order).
    public static int SeasonOf(int day, int daysPerSeason)
    {
        if (daysPerSeason < 1)
            daysPerSeason = 1;
        if (day < 0)
            day = 0;
        return day / daysPerSeason % 4;
    }

    public static string SeasonName(int season)
    {
        switch (season)
        {
            case 0: return "Spring";
            case 1: return "Summer";
            case 2: return "Fall";
            default: return "Winter";
        }
    }

    public static int Clamp(int multiplierBp, int floorBp) => Math.Min(MaxBp, Math.Max(floorBp, multiplierBp));

    static long FloorDiv(long a, long b)
    {
        long q = a / b;
        if (a % b != 0 && (a < 0) != (b < 0))
            q--;
        return q;
    }

    // One game day of recovery: the multiplier moves recoveryPercent of the way to the target, rounded half up, and lands exactly on the
    // target once the gap is under SnapBp. Never below the floor.
    public static int Recover(int multiplierBp, int targetBp, int recoveryPercent, int floorBp)
    {
        recoveryPercent = Math.Max(0, Math.Min(100, recoveryPercent));
        int target = Clamp(targetBp, floorBp);
        int m = Clamp(multiplierBp, floorBp);
        int gap = target - m;
        if (Math.Abs(gap) < SnapBp)
            return target;
        int delta = (int)FloorDiv((long)gap * recoveryPercent * 2 + 100, 200); // round half up
        return Clamp(m + delta, floorBp);
    }

    // The multiplier after one unit is sold: down by dropBp of itself, rounded half up, never below the floor.
    public static int Drop(int multiplierBp, int dropBp, int floorBp)
    {
        long next = ((long)multiplierBp * (One - dropBp) + One / 2) / One;
        return Clamp((int)next, floorBp);
    }

    // What one unit pays: base times multiplier, rounded half up to the cent.
    public static long UnitPrice(long baseCents, int multiplierBp) =>
        Money.Add(Money.Mul(baseCents, multiplierBp), One / 2) / One;

    // Sells count units one at a time at the falling price. Does not touch any state: the new multiplier is in the result.
    public static SaleResult SellStack(long baseCents, int multiplierBp, int count, int dropBp, int floorBp)
    {
        var result = new SaleResult { newMultiplierBp = multiplierBp };
        if (baseCents <= 0 || count <= 0)
            return result;

        int m = Clamp(multiplierBp, floorBp);
        for (int i = 0; i < count; i++)
        {
            long price = UnitPrice(baseCents, m);
            if (i == 0)
                result.firstUnitCents = price;
            result.lastUnitCents = price;
            result.totalCents = Money.Add(result.totalCents, price);
            m = Drop(m, dropBp, floorBp);
        }

        result.count = count;
        result.newMultiplierBp = m;
        return result;
    }

    // The store's price for a stocked item in a season: the base, changed by the winter or summer drift percent if it is survival gear.
    // winterPercent +10 is 10% dearer, summerPercent -5 is 5% cheaper.
    public static long BuyPrice(long baseCents, bool seasonalBuy, int season, int winterPercent, int summerPercent)
    {
        int percent = !seasonalBuy ? 0 : season == 3 ? winterPercent : season == 1 ? summerPercent : 0;
        if (percent == 0)
            return baseCents;
        return Money.Add(Money.Mul(baseCents, 100 + percent), 50) / 100;
    }

    // Below its target by more than the band is recovering (Up), above by more than the band is falling (Down), else Flat.
    public static MarketTrend Trend(int multiplierBp, int targetBp, int bandPercent)
    {
        int band = bandPercent * 100;
        if (targetBp - multiplierBp > band)
            return MarketTrend.Up;
        if (multiplierBp - targetBp > band)
            return MarketTrend.Down;
        return MarketTrend.Flat;
    }

    public static int PercentOfNormal(int multiplierBp) => (multiplierBp + 50) / 100;

    public static string GroupName(MarketGroup group)
    {
        switch (group)
        {
            case MarketGroup.WoodStone: return "Firewood, logs and stone";
            case MarketGroup.Fish: return "Fish";
            case MarketGroup.GameMeat: return "Game meat";
            case MarketGroup.Hides: return "Hides and furs";
            case MarketGroup.Forage: return "Forage";
            case MarketGroup.Crafted: return "Crafted goods";
            case MarketGroup.Farm: return "Farm produce";
            case MarketGroup.Ash: return "Ash";
            case MarketGroup.Maple: return "Maple syrup";
            case MarketGroup.Potash: return "Lye and Potash";
            default: return "Stocked goods";
        }
    }

    // The one-line market note: the groups whose target this season is 10 percent or more away from normal, skipping any group whose
    // target is the same all year (forage). Empty when nothing stands out.
    public static string SeasonNote(MarketTable table, int season)
    {
        var parts = new List<string>();
        foreach (MarketGroupRule rule in table.Groups)
        {
            int target = rule.TargetFor(season);
            if (!rule.SeasonalChange || Math.Abs(target - One) < 1000)
                continue;
            int percent = Math.Abs(target - One) / 100;
            parts.Add($"{GroupName(rule.group)} {(target > One ? "up" : "down")} {percent}%");
        }

        if (parts.Count == 0)
            return "";
        var text = new StringBuilder(SeasonName(season)).Append(" prices: ");
        text.Append(string.Join(", ", parts)).Append('.');
        return text.ToString();
    }
}
