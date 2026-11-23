using System;

// Food spoilage (Food_Spoilage_and_Preservation.md, Mike 2026-11-22): each perishable batch carries a freshness stamp on a
// "freshness clock" that runs faster in heat and slower in cold. The shelf life on an item (Item_Data.md) is its life at a
// mild 10 to 20 C; a batch is spoiled once its clock has run that many hours past its stamp, and then turns to Rot.
//
// There are three clocks: the outdoors (the pack, dropped piles and most storage), the Food Cache (shaded and off the
// ground: x 0.8 of the outdoors, Mike's "a little preservation") and the Root Cellar (a steady 4 C all year). A batch's stamp
// is on the clock of wherever it is; moving it between clocks converts the stamp so it keeps exactly the life it had left.
// Pure rules: SpoilageManager runs the clocks and the sweep.
// Slice 3 adds the Drying Rack's clock: food hung to dry ages at half the outdoor rate while it dries (DryingRules).
public enum FreshClock { Outdoor = 0, Cache = 1, Cellar = 2, Rack = 3 }

public enum Freshness { Fresh, Aging, GoingOff }

public static class SpoilageRules
{
    public const string RotId = "rot";

    // Stamps are whole clock hours, rounded to this step so food gathered over a few hours shares one batch.
    public const int StampStepHours = 6;

    // The Food Cache runs at this share of the outdoor rate; the Root Cellar at this fixed rate (about 4 C).
    public const double CacheFactor = 0.8;
    public const double CellarRate = 0.3;
    // The Drying Rack runs at this share of the outdoor rate (the design doc's "slower than in the pack: x 0.5").
    public const double RackFactor = 0.5;

    // The rate (clock hours per real game hour) by air temperature, blended between these points:
    // frozen x0.1, near freezing x0.3, cool x0.6, mild x1.0, warm x1.5, hot x2.0 (the design doc's table, centred on each band).
    static readonly double[] PointC = { -1.0, 2.0, 7.0, 15.0, 23.5, 30.0 };
    static readonly double[] PointRate = { 0.1, 0.3, 0.6, 1.0, 1.5, 2.0 };

    public static double RateAt(double tempC)
    {
        if (double.IsNaN(tempC) || tempC <= PointC[0])
            return PointRate[0];
        for (int i = 1; i < PointC.Length; i++)
        {
            if (tempC <= PointC[i])
            {
                double t = (tempC - PointC[i - 1]) / (PointC[i] - PointC[i - 1]);
                return PointRate[i - 1] + (PointRate[i] - PointRate[i - 1]) * t;
            }
        }
        return PointRate[PointRate.Length - 1];
    }

    // How fast one clock runs this hour. difficulty is DifficultyManager.SpoilageMultiplier (Settler 0.75, Pioneer 1, Homesteader 1.3).
    public static double ClockRate(FreshClock clock, double tempC, double difficulty)
    {
        double d = difficulty > 0 ? difficulty : 1.0;
        switch (clock)
        {
            case FreshClock.Cellar: return CellarRate * d;
            case FreshClock.Cache: return RateAt(tempC) * CacheFactor * d;
            case FreshClock.Rack: return RateAt(tempC) * RackFactor * d;
            default: return RateAt(tempC) * d;
        }
    }

    // The stamp for food made fresh now: the clock rounded to the nearest step, never below 1 (0 means "no stamp" in a pile).
    public static int Stamp(double clockHours)
    {
        long rounded = (long)Math.Floor((clockHours + StampStepHours / 2.0) / StampStepHours) * StampStepHours;
        return (int)Math.Max(1, Math.Min(int.MaxValue, rounded));
    }

    // Fresh now, less ageHours of life already used (late field dressing).
    public static int StampAged(double clockHours, double ageHours) => Math.Max(1, (int)Math.Round(clockHours - Math.Max(0.0, ageHours)));

    public static double AgeHours(int stamp, double clockHours) => Math.Max(0.0, clockHours - stamp);

    // 0 fresh, 1 at the end of its life. Non-perishables (shelf life 0) are always 0.
    public static double FractionUsed(int stamp, double clockHours, double shelfLifeDays) =>
        shelfLifeDays <= 0 ? 0.0 : AgeHours(stamp, clockHours) / (shelfLifeDays * 24.0);

    public static bool IsSpoiled(int stamp, double clockHours, double shelfLifeDays) =>
        shelfLifeDays > 0 && FractionUsed(stamp, clockHours, shelfLifeDays) >= 1.0;

    // Moving a batch from one clock to another: it keeps the age it had.
    public static int Convert(int stamp, double fromClock, double toClock)
    {
        double age = AgeHours(stamp, fromClock);
        return Math.Max(1, (int)Math.Round(toClock - age));
    }

    // Normal in its first half, amber ("aging") after, red ("going off") in its last fifth.
    public static Freshness StageOf(double fractionUsed) =>
        fractionUsed < 0.5 ? Freshness.Fresh : fractionUsed < 0.8 ? Freshness.Aging : Freshness.GoingOff;

    // Mike, 2026-11-22: "a slight chance of feeling sick the closer it is to spoiling". None in the first half of its life,
    // rising to MaxIllnessChance at the very end.
    public const double MaxIllnessChance = 0.15;

    public static double IllnessChance(double fractionUsed)
    {
        if (fractionUsed <= 0.5)
            return 0.0;
        return MaxIllnessChance * Math.Min(1.0, (fractionUsed - 0.5) / 0.5);
    }

    // Two chances of falling ill from one bite (the food's own, e.g. raw meat, and its age) combined.
    public static double Combine(double a, double b) => 1.0 - (1.0 - Clamp01(a)) * (1.0 - Clamp01(b));

    // Game days left at the clock's current rate (ratePerHour clock hours per game hour). Very slow clocks (frozen) give large numbers.
    public static double DaysLeft(int stamp, double clockHours, double shelfLifeDays, double ratePerHour)
    {
        double leftClockHours = Math.Max(0.0, shelfLifeDays * 24.0 - AgeHours(stamp, clockHours));
        return leftClockHours / Math.Max(0.01, ratePerHour) / 24.0;
    }

    // "3 days left", "1 day left", "5 hours left", "less than an hour left"; a clock this slow (frozen) says so.
    public static string LeftText(double daysLeft, double ratePerHour)
    {
        if (ratePerHour <= 0.15 && daysLeft > 2.0)
            return "keeping while frozen";
        if (daysLeft >= 1.5)
            return $"{(int)Math.Round(daysLeft)} days left";
        if (daysLeft >= 1.0)
            return "1 day left";
        int hours = (int)Math.Floor(daysLeft * 24.0);
        if (hours >= 2)
            return $"{hours} hours left";
        return hours == 1 ? "1 hour left" : "less than an hour left";
    }

    // Saves from before spoilage stored a calendar day on each perishable batch. It becomes a stamp on the outdoor clock that
    // keeps the batch's age, but never more than half its life, so nothing spoils the moment the update loads (design doc 1).
    public static int MigrateDayStamp(int oldDay, int today, double clockHours, double shelfLifeDays)
    {
        double age = Math.Max(0, today - oldDay) * 24.0;
        if (shelfLifeDays > 0)
            age = Math.Min(age, shelfLifeDays * 24.0 * 0.5);
        return StampAged(clockHours, age);
    }

    static double Clamp01(double v) => v < 0 ? 0 : v > 1 ? 1 : v;
}
