using System;

// Greenhouse slice G2: Hoop Houses (Greenhouses.md; Mike's answers 2026-11-26 in section 11). Pure and engine-free so edit-mode tests cover it.
//
// A Hoop House is a walk-in tunnel of steel hoops covered with greenhouse plastic, with plank end walls and a door at the front (local -Z, toward the player
// who placed it). Three sizes, each a staged site: marked out free, its Lumber, Greenhouse Hoops, Greenhouse Plastic and Cordage delivered over trips (R),
// then pounded with the Hammer. Finishing it lays out its bed squares inside, already dug, in rows along its length with paths between (8, 16 or 36):
// ordinary Garden Beds marked as under cover (GardenRules.FlagInFrame), like a Cold Frame's.
//
// The inside temperature works as a Cold Frame's but for a bigger, airier space: one layer of plastic keeps the night about 5.5 C (10 F) warmer, and the sun
// heats it a little less than a small glass box. E at the door rolls the sides up (the heat gets out) or down (for cold nights). No rain falls inside either
// way. Any crop fits. Heat harm is the Cold Frame's (ColdFrameRules.HeatDamage).
//
// The plastic: it wears out after WearYears and must be replaced with new sheets from the Trading Post; and snow builds up on it while it snows. Past WarnCm a
// banner says to sweep it (E at the door, an hour's work); left to reach TearCm, the plastic tears. Torn or worn out, the house gives no cover at all (the beds
// inside are as outdoors) until E at the door fits new sheets carried in the pack.
public static class HoopHouseRules
{
    public enum Size { Small, Medium, Large }

    public static string Name(Size size) => SizeWord(size) + " Hoop House";
    public static string SiteName(Size size) => Name(size) + " Site";
    static string SizeWord(Size size) => size == Size.Small ? "Small" : size == Size.Medium ? "Medium" : "Large";

    // --- Size (metres; Greenhouses.md 3: 10 x 20 ft, 14 x 32 ft and 20 x 50 ft) ---
    // Medium and Large are a little longer than the first proposal (30 and 48 ft) so their 16 and 36 bed squares and a way in fit.

    public const float SquareSize = 1.2f;   // one bed square, 4 ft

    public static float Width(Size size) => size == Size.Small ? 3.0f : size == Size.Medium ? 4.3f : 6.0f;
    public static float Length(Size size) => size == Size.Small ? 6.0f : size == Size.Medium ? 9.8f : 15.4f;
    public static float Height(Size size) => size == Size.Small ? 2.1f : size == Size.Medium ? 2.5f : 3.0f;

    public static int Rows(Size size) => size == Size.Large ? 3 : 2;
    public static int PerRow(Size size) => size == Size.Small ? 4 : size == Size.Medium ? 8 : 12;
    public static int Squares(Size size) => Rows(size) * PerRow(size);

    // The Large house has three rows, the middle one in line with the door, so it keeps a cross aisle just inside the door to step in on.
    public static float FrontAisle(Size size) => size == Size.Large ? 0.9f : 0f;

    static readonly float[] SmallRows = { -0.9f, 0.9f }, MediumRows = { -1.25f, 1.25f }, LargeRows = { -2.2f, 0f, 2.2f };

    public static float RowX(Size size, int row) => (size == Size.Small ? SmallRows : size == Size.Medium ? MediumRows : LargeRows)[row];

    // Where square i sits inside the house (local metres from its centre; -Z is the door end): row by row, front to back.
    public static (float x, float z) SquareAt(Size size, int i)
    {
        int per = PerRow(size);
        int row = i / per, k = i % per;
        float length = Length(size), aisle = FrontAisle(size);
        float start = -length / 2f + aisle + (length - aisle - per * SquareSize) / 2f;
        return (RowX(size, row), start + SquareSize * (k + 0.5f));
    }

    // Whether a point (local metres) is inside the house's footprint, with a margin (positive: a little outside counts too).
    public static bool Inside(Size size, float localX, float localZ, float margin = 0f) =>
        Math.Abs(localX) <= Width(size) / 2f + margin && Math.Abs(localZ) <= Length(size) / 2f + margin;

    // The door: a gap in the front end wall, centred.
    public const float DoorWidth = 1.0f, DoorHeight = 1.9f;

    // --- Materials (delivered to the site; Dismantle gives half back) ---

    public const string LumberId = "lumber";
    public const string CordageId = "cordage";
    public const string HoopId = "greenhouse_hoop";
    public const string PlasticId = "greenhouse_plastic";
    public const long HoopStoreCents = 1200;     // a bent steel hoop, about $12
    public const long PlasticStoreCents = 2000;  // a sheet of greenhouse plastic, about $20
    public const float HoopKg = 4f, PlasticKg = 9f;

    // A hoop every 4 ft along the length, one at each end.
    public static int Hoops(Size size) => (int)Math.Floor(Length(size) / SquareSize + 0.001f) + 1;
    public static int Sheets(Size size) => size == Size.Small ? 1 : size == Size.Medium ? 2 : 4;
    public static int Lumber(Size size) => size == Size.Small ? 8 : size == Size.Medium ? 12 : 20;
    public static int Cordage(Size size) => size == Size.Small ? 2 : size == Size.Medium ? 3 : 4;
    public static int BuildBlows(Size size) => size == Size.Small ? 40 : size == Size.Medium ? 60 : 90;

    // The Smithy's homemade hoop: cheaper than the store's if you make your own iron.
    public const int ForgeBars = 1, ForgeCharcoal = 1, ForgeHours = 2;

    // --- The inside temperature ---

    public const float ShutNightLiftC = 5.5f;   // one layer of plastic, sides down: about 10 F warmer at night (Greenhouses.md 1)
    public const float ShutSunGainC = 22f;      // a closed tunnel in full midday sun (a little less than a small glass box)
    public const float OpenNightLiftC = 1f;     // sides rolled up: the roof still shelters a little
    public const float OpenSunGainC = 3f;

    public static float InsideC(float airC, float sun01, bool sidesUp, bool torn)
    {
        if (torn)
            return airC;
        float sun = sun01 < 0f ? 0f : sun01 > 1f ? 1f : sun01;
        return airC + (sidesUp ? OpenNightLiftC + OpenSunGainC * sun : ShutNightLiftC + ShutSunGainC * sun);
    }

    // No rain falls on the beds inside, sides up or down; through torn plastic, all of it.
    public static float RainShare(bool torn) => torn ? 1f : 0f;

    // --- The plastic ---

    public const int WearYears = 4;

    public static int YearsOld(int fittedDay, int today, int daysPerYear) => daysPerYear <= 0 ? 0 : Math.Max(0, today - fittedDay) / daysPerYear;
    public static bool WornOut(int fittedDay, int today, int daysPerYear) => daysPerYear > 0 && today - fittedDay >= WearYears * daysPerYear;
    public static int YearsLeft(int fittedDay, int today, int daysPerYear) => Math.Max(0, WearYears - YearsOld(fittedDay, today, daysPerYear));

    // Snow on the plastic (cm): it builds while it snows and melts when the air is above freezing.
    public const float SnowCmPerHour = 2f;
    public const float MeltCmPerDegreeHour = 0.5f;
    public const float WarnCm = 15f;           // the banner, and the prompt to sweep
    public const float TearCm = 30f;           // the plastic tears
    public const float SweepMinCm = 2f;        // less than this is not worth sweeping
    public const int SweepMinutes = 60;

    public static float SnowAfterHour(float snowCm, bool snowing, float airC)
    {
        float after = snowing ? snowCm + SnowCmPerHour : snowCm - Math.Max(0f, airC) * MeltCmPerDegreeHour;
        return after < 0f ? 0f : after;
    }

    public static bool Tears(float snowCm) => snowCm >= TearCm;

    // --- Words ---

    public static string Header(Size size, bool sidesUp, bool torn, float insideC, float outsideC, float snowCm, int yearsLeft)
    {
        if (torn)
            return $"{Name(size)} - the plastic is torn: no cover until new sheets are fitted";
        string line = $"{Name(size)} - sides {(sidesUp ? "rolled up" : "down")}, {Math.Round(insideC):0} C inside ({Math.Round(outsideC):0} C outside)";
        if (snowCm >= SweepMinCm)
            line += $", {Math.Round(snowCm):0} cm of snow on it";
        return line + $"; plastic good for {yearsLeft} more year{(yearsLeft == 1 ? "" : "s")}";
    }

    public static string SidesPrompt(bool sidesUp) => sidesUp ? "Roll the sides down  (holds the warmth in: for cold nights)" : "Roll the sides up  (lets the heat out: for sunny days)";

    public static string SweepPrompt(float snowCm) => $"Sweep the snow off  ({Math.Round(snowCm):0} cm; about an hour)";

    public static string FitPrompt(Size size, int carried) =>
        carried >= Sheets(size) ? $"Fit new plastic  ({Sheets(size)} Greenhouse Plastic)" : $"Needs {Sheets(size)} Greenhouse Plastic to re-cover (you have {carried}; Trading Post)";

    public static string DismantlePrompt(Size size) => $"Take the {Name(size)} down  (half the materials back, rounded down; the beds must be empty)";

    public const string NotEmpty = "Harvest or clear the beds in the hoop house first";

    public static string BedLine(bool sidesUp, bool torn, float insideC) =>
        torn ? "in a hoop house with torn plastic: no cover" : $"in a hoop house: sides {(sidesUp ? "up" : "down")}, {Math.Round(insideC):0} C inside";

    public static string OverheatBanner(float insideC) =>
        $"A hoop house is overheating ({Math.Round(insideC):0} C inside) - roll the sides up (E at its door)";

    public static string CloseSidesBanner(int houses, float lowC) =>
        $"Frost tonight (about {Math.Round(lowC):0} C) - roll down the sides " + (houses == 1 ? "of the hoop house" : $"of {houses} hoop houses") + " (E at the door)";

    public static string SnowBanner(float snowCm) => $"Snow is piling up on a hoop house ({Math.Round(snowCm):0} cm) - sweep it off (E at its door) before the plastic tears";

    public const string TornBanner = "The snow tore a hoop house's plastic - fit new Greenhouse Plastic (E at its door) to cover it again";
    public const string WornBanner = "A hoop house's plastic has worn out after 4 years - fit new Greenhouse Plastic (E at its door) to cover it again";
    public const string Swept = "Swept the snow off the hoop house";
    public const string Refitted = "Fitted new plastic - the hoop house is covered again, good for 4 years";

    public static string SiteHelp(Size size) =>
        $"a walk-in tunnel of steel hoops under greenhouse plastic, {Math.Round(Width(size) / 0.3048f):0} by {Math.Round(Length(size) / 0.3048f):0} ft, with {Squares(size)} bed squares inside ready to plant. " +
        $"Mark the site out (free, no Hammer; turn it with the wheel so the door faces where you want to walk in), deliver {Lumber(size)} Lumber, {Hoops(size)} Greenhouse Hoops, {Sheets(size)} Greenhouse Plastic and {Cordage(size)} Cordage with R over as many trips as it takes " +
        "(hoops and plastic from the Trading Post, or forge the hoops at a Smithy), then equip the Hammer and hold click on the site until it is built. " +
        "Inside, nights stay about 10 F warmer and frost stays off, any crop grows, but no rain gets in, so water the beds. On a sunny day E at the door rolls the sides up to let the heat out. " +
        "Sweep snow off it (E) before it tears the plastic; the plastic lasts 4 years.";
}
