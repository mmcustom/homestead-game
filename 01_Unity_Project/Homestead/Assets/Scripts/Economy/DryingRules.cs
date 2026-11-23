using System;

// The Drying Rack (Food_Spoilage_and_Preservation.md section 3, slice 3). Pure rules; SpoilageManager runs the racks each hour.
//
// Fresh fruit, mushrooms, raw meat and raw fish hung on the rack dry in the open air, but only in daylight and while nothing is
// falling: each hour that is dry and light counts towards drying, and night, rain and snow pause it. Berries, apples and mushrooms
// need 24 such hours (about 2 days); meat and fish 36 (about 3 days). Meanwhile the food still ages, on the rack's own clock at
// half the outdoor rate (SpoilageRules.RackFactor), so fish hung in a heat wave can still be lost.
//
// A finished batch turns into its dried form. Fruit and mushrooms dry one for one. Meat and fish are cut into strips by what the piece is
// worth: two strips for every dollar the cooked piece sells for (Mike, 2026-11-22: "make salted meat and dried meat more profitable than
// just cooked meat and even more than fresh raw meat"). A Venison ($4 cooked) gives 8 strips, a Bass ($3) 6, a Bluegill ($2) 4, and a
// strip sells for $0.80, so every kind is worth more dried than cooked or raw, even after the price drops a little with each strip sold.
// Dried food is far lighter than fresh and keeps for months.
public static class DryingRules
{
    public const string DriedBerriesId = "dried_berries";
    public const string DriedApplesId = "dried_apples";
    public const string DriedMushroomsId = "dried_mushrooms";
    public const string DriedMeatId = "dried_meat";
    public const string DriedFishId = "dried_fish";

    public static readonly string[] DriedIds = { DriedBerriesId, DriedApplesId, DriedMushroomsId, DriedMeatId, DriedFishId };

    // Pieces of fresh food the rack holds at once (the dried food waiting to be taken does not count).
    public const int Capacity = 12;

    // Dry daylight hours to finish: fruit and mushrooms, meat and fish.
    public const int FruitHours = 24;
    public const int MeatHours = 36;

    // Strips of Dried Meat or Dried Fish for every dollar the cooked piece is worth.
    public const int StripsPerCookedDollar = 2;

    // Trading Post prices (proposed, cents): dried food pays more than fresh, and dried meat and fish more than cooked.
    public const long DriedBerriesCents = 150;
    public const long DriedApplesCents = 150;
    public const long DriedMushroomsCents = 200;
    public const long DriedMeatCents = 80;
    public const long DriedFishCents = 80;

    // What each raw meat and fish sells for cooked at the Trading Post (MarketCatalog's cooked rows; a test keeps the two the same).
    // Dried strips and salt pieces are counted from it, so a piece worth more cooked gives more of them.
    public static int CookedCents(string rawId)
    {
        switch (rawId)
        {
            case "venison": return 400;
            case "turkey_meat":
            case "waterfowl_meat":
            case "bass":
            case "catfish": return 300;
            case "small_game_meat":
            case "chicken_meat":
            case "bluegill":
            case "crappie": return 200;
            default: return 0;
        }
    }

    // What a fresh item dries into, and how many dry hours it needs. False for anything the rack does not dry.
    public static bool TryFind(string itemId, out string driedId, out int hours)
    {
        switch (itemId)
        {
            case "blackberries":
            case "raspberries": driedId = DriedBerriesId; hours = FruitHours; return true;
            case "wild_apples":
            case "pawpaw": driedId = DriedApplesId; hours = FruitHours; return true;
            case "morel":
            case "oyster_mushroom":
            case "dryads_saddle": driedId = DriedMushroomsId; hours = FruitHours; return true;
            case "venison":
            case "turkey_meat":
            case "small_game_meat":
            case "waterfowl_meat":
            case "chicken_meat": driedId = DriedMeatId; hours = MeatHours; return true;
            case "bass":
            case "bluegill":
            case "catfish":
            case "crappie": driedId = DriedFishId; hours = MeatHours; return true;
            default: driedId = null; hours = 0; return false;
        }
    }

    public static bool Dries(string itemId) => TryFind(itemId, out _, out _);

    public static bool IsDried(string itemId) => Array.IndexOf(DriedIds, itemId) >= 0;

    // The rack takes fresh food to dry, and takes back dried food the player puts on it.
    public static bool Accepts(string itemId) => Dries(itemId) || IsDried(itemId);

    // Dried pieces from one fresh piece: one for fruit and mushrooms; for meat and fish, two strips per dollar of its cooked price (at least one).
    public static int YieldOf(string itemId, double freshKg)
    {
        if (!TryFind(itemId, out string driedId, out _))
            return 0;
        if (driedId != DriedMeatId && driedId != DriedFishId)
            return 1;
        return Math.Max(1, CookedCents(itemId) * StripsPerCookedDollar / 100);
    }

    // Whether this hour counts towards drying: daylight, and no rain or snow falling.
    public static bool DriesNow(bool daylight, bool precipitating) => daylight && !precipitating;

    // Dry daylight hours still needed, never below 0.
    public static int HoursLeft(int hoursNeeded, float hoursDone) => Math.Max(0, (int)Math.Ceiling(hoursNeeded - hoursDone));

    // The row note for fresh food on the rack (its slowest batch): "drying, 10 daylight hours to go". Night, rain and snow do not count.
    public static string RowText(int hoursLeft) =>
        hoursLeft <= 0 ? "dry" : hoursLeft == 1 ? "drying, 1 daylight hour to go" : $"drying, {hoursLeft} daylight hours to go";

    // What the rack is doing: "drying 6 pieces", "drying paused for the night", "drying paused while it rains" (or snows); null when nothing is drying.
    public static string Status(int drying, bool daylight, bool raining, bool snowing)
    {
        if (drying <= 0)
            return null;
        if (raining)
            return "drying paused while it rains";
        if (snowing)
            return "drying paused while it snows";
        if (!daylight)
            return "drying paused for the night";
        return drying == 1 ? "drying 1 piece" : $"drying {drying} pieces";
    }
}
