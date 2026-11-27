using System;

// Greenhouse slice G3: Glass Greenhouses and their seedling benches (Greenhouses.md; Mike's answers 2026-11-26 in section 13). Pure and engine-free so edit-mode
// tests cover it.
//
// A Glass Greenhouse is a walk-in timber-framed house on a stone footing, glazed walls and a pitched glass roof, the door in the front gable at local -Z. Three
// sizes, each a staged site like the Hoop House: marked out free, its Lumber, Stone, Glass Panes (Trading Post) and Boxes of Nails delivered over trips (R), then
// pounded with the Hammer. No Iron Bars (Mike). Finishing it lays out its bed squares inside (4, 9 or 30, any crop) and its seedling benches along the back wall.
// The glass lasts for good: no wear, no breakage.
//
// It holds the warmth better than plastic (glass and the stone footing): about 7 C warmer on a still night with the vents shut, and the sun heats it a little
// less than a tunnel. E at the door opens the roof vents and the door (the heat gets out) or shuts them. No rain falls inside.
//
// A seedling bench holds one tray. E with seed in the pack sows it (R picks which); it grows at the house's inside temperature like a bed (but needs no watering),
// and when it is ready E takes up the transplants: Tomato Plants from Tomato Seed, and Lettuce, Kale and Squash Starts, which go into a garden bed already
// TrayDays grown (GardenRules.Sowings). One seed (a packet, as a square takes) fills a tray, enough transplants for TraySquares squares.
public static class GlassHouseRules
{
    public enum Size { Small, Medium, Large }

    public static string Name(Size size) => SizeWord(size) + " Glass Greenhouse";
    public static string SiteName(Size size) => Name(size) + " Site";
    static string SizeWord(Size size) => size == Size.Small ? "Small" : size == Size.Medium ? "Medium" : "Large";

    // --- Size (metres; Greenhouses.md 3 proposed 8 x 12, 12 x 20 and 20 x 40 ft: each is a little bigger so its approved bed squares, a path and the benches fit) ---

    public const float SquareSize = 1.2f;

    public static float Width(Size size) => size == Size.Small ? 3.0f : size == Size.Medium ? 4.9f : 6.0f;     // 10, 16 and 20 ft
    public static float Length(Size size) => size == Size.Small ? 3.6f : size == Size.Medium ? 6.1f : 13.7f;   // 12, 20 and 45 ft
    public const float WallHeight = 1.6f;
    public static float RidgeHeight(Size size) => size == Size.Small ? 2.5f : size == Size.Medium ? 3.0f : 3.4f;
    public static float Height(Size size) => RidgeHeight(size);

    public static int Rows(Size size) => size == Size.Small ? 2 : 3;
    public static int PerRow(Size size) => size == Size.Small ? 2 : size == Size.Medium ? 3 : 10;
    public static int Squares(Size size) => Rows(size) * PerRow(size);

    // With a middle row in line with the door, a cross aisle just inside it.
    public static float FrontAisle(Size size) => size == Size.Small ? 0.3f : 0.9f;

    static readonly float[] SmallRows = { -0.9f, 0.9f }, MediumRows = { -1.8f, 0f, 1.8f }, LargeRows = { -2.2f, 0f, 2.2f };
    public static float RowX(Size size, int row) => (size == Size.Small ? SmallRows : size == Size.Medium ? MediumRows : LargeRows)[row];

    // Square i (local metres from the centre; -Z is the door end): row by row, from just inside the aisle toward the benches.
    public static (float x, float z) SquareAt(Size size, int i)
    {
        int per = PerRow(size);
        int row = i / per, k = i % per;
        float start = -Length(size) / 2f + FrontAisle(size);
        return (RowX(size, row), start + SquareSize * (k + 0.5f));
    }

    public static bool Inside(Size size, float localX, float localZ, float margin = 0f) =>
        Math.Abs(localX) <= Width(size) / 2f + margin && Math.Abs(localZ) <= Length(size) / 2f + margin;

    public const float DoorWidth = 1.0f, DoorHeight = 1.9f;

    // --- The seedling benches, along the back wall ---

    public const float BenchDepth = 0.6f, BenchHeight = 0.8f;
    public static int Benches(Size size) => size == Size.Small ? 1 : size == Size.Medium ? 2 : 3;
    public static float BenchWidth(Size size) => size == Size.Small ? 1.6f : size == Size.Medium ? 2.0f : 1.8f;   // the Small one leaves the back corner for a heater (G4)

    public static (float x, float z) BenchAt(Size size, int i)
    {
        if (size == Size.Small)
            return (-0.6f, Length(size) / 2f - BenchDepth / 2f - 0.1f);
        int n = Benches(size);
        float spacing = Width(size) / n;
        return (-Width(size) / 2f + spacing * (i + 0.5f), Length(size) / 2f - BenchDepth / 2f - 0.1f);
    }

    // --- Materials (delivered to the site; Dismantle gives half back) ---

    public const string LumberId = "lumber";
    public const string StoneId = "stone";
    public const string PaneId = "glass_pane";
    public const string NailsId = "nails_box";
    public const long PaneStoreCents = 300;   // a 2 x 3 ft pane of glass, about $3
    public const float PaneKg = 2f;

    public static int Lumber(Size size) => size == Size.Small ? 16 : size == Size.Medium ? 28 : 60;
    public static int Stone(Size size) => size == Size.Small ? 10 : size == Size.Medium ? 18 : 36;
    public static int Panes(Size size) => size == Size.Small ? 24 : size == Size.Medium ? 48 : 110;
    public static int Nails(Size size) => size == Size.Small ? 1 : size == Size.Medium ? 2 : 4;
    public static int BuildBlows(Size size) => size == Size.Small ? 60 : size == Size.Medium ? 90 : 140;

    // --- The inside temperature ---

    public const float ShutNightLiftC = 7f;    // glass on a stone footing, vents shut: a little better than plastic
    public const float ShutSunGainC = 20f;
    public const float OpenNightLiftC = 1.5f;  // vents and door open
    public const float OpenSunGainC = 3f;

    public static float InsideC(float airC, float sun01, bool ventsOpen)
    {
        float sun = sun01 < 0f ? 0f : sun01 > 1f ? 1f : sun01;
        return airC + (ventsOpen ? OpenNightLiftC + OpenSunGainC * sun : ShutNightLiftC + ShutSunGainC * sun);
    }

    // --- Trays ---

    public struct Tray
    {
        public string cropId, seedId, transplantId, transplantName;
        public int transplants;   // what one tray gives
        public float days;        // growing days on the bench (and the head start the transplants carry)
    }

    public const int TraySquares = 3;   // one tray is enough for this many squares

    public static readonly Tray[] Trays =
    {
        new Tray { cropId = "tomatoes", seedId = "seed_tomatoes", transplantId = "tomato_plant", transplantName = "Tomato Plants", transplants = 4 * TraySquares, days = 30f },
        new Tray { cropId = "lettuce", seedId = "seed_lettuce", transplantId = "starts_lettuce", transplantName = "Lettuce Starts", transplants = TraySquares, days = 20f },
        new Tray { cropId = "kale", seedId = "seed_kale", transplantId = "starts_kale", transplantName = "Kale Starts", transplants = TraySquares, days = 20f },
        new Tray { cropId = "squash", seedId = "seed_squash", transplantId = "starts_squash", transplantName = "Squash Starts", transplants = TraySquares, days = 25f },
    };

    public static bool TryTray(string cropId, out Tray tray)
    {
        foreach (Tray t in Trays)
            if (t.cropId == cropId) { tray = t; return true; }
        tray = default;
        return false;
    }

    // The new transplant items (Tomato Plants already exist), each planting one square.
    public static readonly string[] StartIds = { "starts_lettuce", "starts_kale", "starts_squash" };

    // --- Words ---

    public static string Header(Size size, bool ventsOpen, float insideC, float outsideC) =>
        $"{Name(size)} - vents {(ventsOpen ? "open" : "shut")}, {Math.Round(insideC):0} C inside ({Math.Round(outsideC):0} C outside)";

    public static string VentsPrompt(bool ventsOpen) => ventsOpen ? "Shut the vents  (holds the warmth in: for cold nights)" : "Open the vents  (lets the heat out: for sunny days)";

    public static string DismantlePrompt(Size size) => $"Take the {Name(size)} down  (half the materials back, rounded down; the beds and benches must be empty)";

    public const string NotEmpty = "Harvest or clear the beds and benches in the greenhouse first";

    public static string BedLine(bool ventsOpen, float insideC) => $"in a glass greenhouse: vents {(ventsOpen ? "open" : "shut")}, {Math.Round(insideC):0} C inside";

    public static string OverheatBanner(float insideC) =>
        $"A glass greenhouse is overheating ({Math.Round(insideC):0} C inside) - open the vents (E at its door)";

    public static string CloseVentsBanner(int houses, float lowC) =>
        $"Frost tonight (about {Math.Round(lowC):0} C) - shut the vents " + (houses == 1 ? "of the glass greenhouse" : $"of {houses} glass greenhouses") + " (E at the door)";

    public static string BenchHeader(string trayName, float days, float needDays, float insideC) =>
        trayName == null ? $"Seedling bench - empty ({Math.Round(insideC):0} C)"
                         : $"Seedling bench - {trayName}: {(days >= needDays ? "ready to take up" : $"{Math.Round(needDays - days):0} days to go")} ({Math.Round(insideC):0} C)";

    public static string SowPrompt(Tray tray) => $"Sow a tray of {tray.transplantName}  (1 seed; {tray.transplants} transplants in about {tray.days:0} days)";
    public static string TakePrompt(Tray tray) => $"Take up the {tray.transplantName}  ({tray.transplants})";
    public const string NoSeedPrompt = "Carry Tomato, Lettuce, Kale or Squash seed to sow a tray here";
    public const string ClearPrompt = "Clear the dead seedlings";
    public const string PickPrompt = "Next tray";
    public const string TrayDiedBanner = "A seedling tray died in the greenhouse - mind the vents";

    public static string SiteHelp(Size size) =>
        $"a walk-in timber-and-glass house on a stone footing, {Math.Round(Width(size) / 0.3048f):0} by {Math.Round(Length(size) / 0.3048f):0} ft, with {Squares(size)} bed squares inside ready to plant and " +
        $"{Benches(size)} seedling bench{(Benches(size) == 1 ? "" : "es")} along the back. Mark the site out (free, no Hammer; turn it with the wheel so the door faces where you want to walk in), " +
        $"deliver {Lumber(size)} Lumber, {Stone(size)} Stone, {Panes(size)} Glass Panes and {Nails(size)} Box{(Nails(size) == 1 ? "" : "es")} of Nails with R over as many trips as it takes " +
        "(the glass and nails from the Trading Post, or forge the nails at a Smithy), then equip the Hammer and hold click on the site until it is built. " +
        "Inside, nights stay about 12 F warmer and frost stays off, any crop grows, but no rain gets in, so water the beds; on a sunny day E at the door opens the vents. " +
        "On a bench, E sows a tray of Tomato, Lettuce, Kale or Squash seed and later takes up the transplants, which go into an outdoor bed weeks ahead. The glass lasts for good.";
}
