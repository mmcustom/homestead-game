using UnityEngine;

// Structures Slice 1, phase B (Structures_Slice1_Dry_And_Clean.md, Wood Shed): wood is wet or dry. No new save keys: the day stamp every
// batch of wood already carries (ItemStack.acquiredDay in a pack, WoodStack.day in a pile; ItemDefinition.keepsBatchDay keeps it apart
// from other batches) is the day the batch was last made wet. A batch is dry once a season's worth of days has passed since then
// (the tunable seasoning length on WellsAndOuthouses), and day 0 means "no stamp": wood from before this existed, or marked dry,
// is dry. A fresh stamp is never below 1 so it can't be mistaken for that. Rain on an open Wood Pile restamps its wood to today, so
// it can't finish seasoning while it keeps being rained on (WoodManager.RainOnOpenPiles); a Wood Shed is never rained on.
//
// The maths is pure (days in, answer out) so the edit-mode tests can run it; the properties below read the live tunables.
public static class WoodMoisture
{
    public const int DefaultDaysPerSeason = 28;
    public const float DefaultSeasons = 1f;
    public const float DefaultWetBurnFraction = 0.7f;

    public static int Today => TimeManager.Instance != null ? TimeManager.Instance.TotalDays : 0;

    static WellsAndOuthouses Settings => WellsAndOuthouses.Instance;

    // Days a wet batch takes to dry: the tunable number of in-game seasons, scaled by the real length of a season.
    public static int SeasoningDays
    {
        get
        {
            int perSeason = TimeManager.Instance != null ? TimeManager.Instance.DaysPerSeason : DefaultDaysPerSeason;
            return DaysFor(perSeason, Settings != null ? Settings.seasoningSeasons : DefaultSeasons);
        }
    }

    // Fraction of its normal burn time wet wood gives (about 30 percent of the heat goes into driving off the water).
    public static float WetBurnFraction => Settings != null ? Settings.wetBurnFraction : DefaultWetBurnFraction;

    // The stamp for wood that is wet right now.
    public static int WetStamp => Mathf.Max(1, Today);

    // --- Pure ---

    public static int DaysFor(int daysPerSeason, float seasons) => Mathf.Max(1, Mathf.RoundToInt(Mathf.Max(1, daysPerSeason) * Mathf.Max(0f, seasons)));

    public static bool IsDry(int batchDay, int today, int seasoningDays) => batchDay <= 0 || today - batchDay >= seasoningDays;

    // The fraction of normal burn time a batch gives.
    public static float BurnFactor(int batchDay, int today, int seasoningDays, float wetFraction) =>
        IsDry(batchDay, today, seasoningDays) ? 1f : wetFraction;

    // Whole days of seasoning still to go (0 once dry).
    public static int DaysLeft(int batchDay, int today, int seasoningDays) =>
        IsDry(batchDay, today, seasoningDays) ? 0 : seasoningDays - (today - batchDay);

    // The same batch a number of days further along: its stamp moves back, and once that reaches 0 it is simply dry.
    public static int Aged(int batchDay, int days) => batchDay <= 0 ? 0 : Mathf.Max(0, batchDay - Mathf.Max(0, days));

    // e.g. "dry", "wet", "2 wet, 3 dry".
    public static string Label(int wet, int dry)
    {
        if (wet > 0 && dry > 0)
            return $"{wet} wet, {dry} dry";
        return wet > 0 ? "wet" : "dry";
    }

    // --- Live ---

    public static bool IsDry(int batchDay) => IsDry(batchDay, Today, SeasoningDays);

    public static float BurnFactor(int batchDay) => IsDry(batchDay) ? 1f : WetBurnFraction;

    // Whether an hour of this weather wets wood left in the open: any rain, and snow.
    public static bool Wets(WeatherType precipitation) =>
        precipitation == WeatherType.LightRain || precipitation == WeatherType.HeavyRain || precipitation == WeatherType.Thunderstorm || precipitation == WeatherType.Snow;

    // True for the items that have a wet or dry state.
    public static bool Applies(string itemId) => WoodManager.IsWood(itemId);
}
