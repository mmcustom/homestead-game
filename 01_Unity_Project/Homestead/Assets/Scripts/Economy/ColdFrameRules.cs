using System;

// Greenhouse slice G1: Cold Frames (Greenhouses.md; Mike's answers 2026-11-26 in section 9). Pure and engine-free so edit-mode tests cover it.
//
// A Cold Frame is a knee-high wooden box with old window sashes for a lid, built in one click with the Hammer in three sizes: Small (1 bed square), Medium (2) and
// Large (3), in a row along the frame. Building it makes its bed squares too: ordinary Garden Beds, already dug, marked as in a frame (GardenRules.FlagInFrame), so
// they are planted, watered, saved for seed and harvested as any bed. The frame itself is the lid: E on its front board props it open or shuts it.
//
// The inside temperature, worked out each game hour from the outside air and the sun: with the lid shut, one layer of glass keeps the night about 5.5 C (10 F)
// warmer and the sun heats a small closed box fast (up to ShutSunGainC on a clear midday); propped open, it is close to the outside. The crops inside grow, take
// frost and take heat at that temperature. No rain falls in with the lid shut, and half with it open, so the beds need watering. Past a crop's heat limit by
// HeatHarmMarginC it is harmed every hour (Mike: stress, then harm; a whole sunny day shut can kill tender greens). Deer and rabbits cannot get in.
// Only low crops fit under the lid: lettuce, kale, carrots and beans.
public static class ColdFrameRules
{
    public enum Size { Small, Medium, Large }

    public static int Squares(Size size) => size == Size.Small ? 1 : size == Size.Medium ? 2 : 3;
    public static string Name(Size size) => (size == Size.Small ? "Small" : size == Size.Medium ? "Medium" : "Large") + " Cold Frame";

    // --- Size and cost ---

    public const float SquareSize = 1.2f;            // one bed square, 4 ft
    public const float Depth = 1.3f;                 // front to back, with the walls
    public const float FrontHeight = 0.30f, BackHeight = 0.50f;
    public const string SashId = "window_sash";
    public const long SashStoreCents = 800;          // an old window sash, about $8
    public const int SashesPerSquare = 2;
    public const string LumberId = "lumber";
    public const int LumberPerSquare = 2;

    public static int Sashes(Size size) => SashesPerSquare * Squares(size);
    public static int Lumber(Size size) => LumberPerSquare * Squares(size);

    // Where square i sits along the frame (metres along its local X from the middle).
    public static float SquareOffset(Size size, int i) => (i - (Squares(size) - 1) / 2f) * SquareSize;

    // --- The inside temperature ---

    public const float ShutNightLiftC = 5.5f;   // one layer of glass, lid shut: about 10 F warmer at night (Greenhouses.md 1)
    public const float ShutSunGainC = 25f;      // a small closed box in full midday sun climbs this far above the air
    public const float OpenNightLiftC = 1.5f;   // propped open: a little shelter still
    public const float OpenSunGainC = 4f;

    // The sun's strength on the glass, 0 to 1: its height through the day (a sine from sunrise to sunset) at the season's midday height, times how clear the sky is.
    public static float Sun(float hour, float sunrise, float sunset, float noonSin, float skyClear)
    {
        if (hour < sunrise || hour >= sunset || sunset <= sunrise)
            return 0f;
        float t = (hour - sunrise) / (sunset - sunrise);
        return Clamp01((float)Math.Sin(Math.PI * t) * Clamp01(noonSin) * Clamp01(skyClear));
    }

    // The midday sun's height (as a sine) by season, from the sky's own numbers (53, 76, 53 and 30 degrees for Spring, Summer, Fall and Winter).
    public static float NoonSin(int season)
    {
        float[] noon = { 53f, 76f, 53f, 30f };
        return (float)Math.Sin(noon[Math.Max(0, Math.Min(3, season))] * Math.PI / 180.0);
    }

    public static float Inside(float airC, float sun01, bool lidOpen) =>
        airC + (lidOpen ? OpenNightLiftC + OpenSunGainC * Clamp01(sun01) : ShutNightLiftC + ShutSunGainC * Clamp01(sun01));

    // How much of the rain reaches the soil inside.
    public static float RainShare(bool lidOpen) => lidOpen ? 0.5f : 0f;

    // --- Heat harm (inside a frame only) ---

    public const float HeatHarmMarginC = 4f;    // stalled from the crop's heat limit; harmed from this far past it
    public const float HeatHarmBase = 0.03f, HeatHarmPerC = 0.02f;

    public static float HeatDamage(float heatLimitC, float insideC)
    {
        float over = insideC - (heatLimitC + HeatHarmMarginC);
        return over <= 0f ? 0f : HeatHarmBase + HeatHarmPerC * over;
    }

    // --- What fits ---

    public static bool Fits(string cropId) => cropId == "lettuce" || cropId == "kale" || cropId == "carrots" || cropId == "beans";

    public static string TooTall(string cropName) => $"{cropName} grow too tall for a cold frame - lettuce, kale, carrots and beans fit under the lid";

    // --- Warnings (Mike: yes, banners) ---

    public const int WarnEveryHours = 3;        // an overheating frame is told about at most this often

    public static string OverheatBanner(float insideC) =>
        $"A cold frame is overheating ({Math.Round(insideC):0} C inside) - prop the lid open (E on its front)";

    public static string CloseLidsBanner(int frames, float lowC) =>
        (frames == 1 ? "Frost tonight" : "Frost tonight") + $" (about {Math.Round(lowC):0} C) - close the cold frame lid" + (frames == 1 ? "" : "s") + " (E on the front)";

    // --- Words ---

    public static string Header(Size size, bool lidOpen, float insideC, float outsideC) =>
        $"{Name(size)} - lid {(lidOpen ? "propped open" : "shut")}, {Math.Round(insideC):0} C inside ({Math.Round(outsideC):0} C outside)";

    public static string LidPrompt(bool lidOpen) => lidOpen ? "Shut the lid  (holds the warmth in: for cold nights)" : "Prop the lid open  (lets the heat out: for sunny days)";

    public static string DismantlePrompt(Size size) => $"Take the {Name(size)} down  (half the materials back, rounded down; the beds must be empty)";

    public const string NotEmpty = "Harvest or clear the beds in the cold frame first";

    public static string BedLine(bool lidOpen, float insideC) => $"in a cold frame: lid {(lidOpen ? "open" : "shut")}, {Math.Round(insideC):0} C inside";

    public static string Help(Size size) =>
        $"a knee-high wooden box with old window sashes for a lid, holding {Squares(size)} bed square{(Squares(size) == 1 ? "" : "s")} of soil ready to plant: {Sashes(size)} Window Sashes (Trading Post) and {Lumber(size)} Lumber. " +
        "The glass keeps the night about 10 F warmer and the frost off, so greens start early and last into winter. But on a sunny day a shut frame gets very hot: E on its front props the lid open, and shuts it again for cold nights. " +
        "No rain gets in with the lid shut, so water the beds. Lettuce, kale, carrots and beans fit under the lid. The Hammer builds it.";

    static float Clamp01(float v) => v < 0f ? 0f : v > 1f ? 1f : v;
}
