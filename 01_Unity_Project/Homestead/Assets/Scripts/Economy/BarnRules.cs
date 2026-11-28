using System;

// Slice B1a: the Pole Barn (Modern_Systems_and_Services.md, "Deliveries and the Pole Barn"; Mike's answers 2026-11-27). Pure and engine-free so
// edit-mode tests cover it.
//
// A post-frame building in three sizes. The player marks out a Pole Barn Site (free) and then, at the Trading Post in Deliver mode, either orders
// the kit (delivered onto the site: posts, trusses and steel sheets) and builds it in three stages with the Hammer (set the posts, raise the trusses,
// put on the roof and walls), or hires the crew (half at booking, half when done; they start LeadDays later and take CrewDays). The finished barn is
// a walk-in shell with a big doorway at its front (-Z) and storage shelves along the back, holding up to CapacityKg of anything but water.
public static class BarnRules
{
    public enum Size { Small, Medium, Large }

    // --- Size (metres; 24 x 32, 30 x 40 and 40 x 60 ft) ---

    public static float Width(Size s) => s == Size.Small ? 7.32f : s == Size.Medium ? 9.14f : 12.19f;    // across, X
    public static float Length(Size s) => s == Size.Small ? 9.75f : s == Size.Medium ? 12.19f : 18.29f;  // along, Z; the door is at -Z
    public static float EaveHeight(Size s) => s == Size.Large ? 3.66f : 3.05f;                           // 10 or 12 ft walls
    public static float RidgeRise(Size s) => Width(s) / 2f * 4f / 12f;                                   // a 4:12 roof
    public static float DoorWidth(Size s) => s == Size.Small ? 3.05f : 3.66f;                            // a 10 or 12 ft sliding door
    public static float DoorHeight(Size s) => s == Size.Small ? 2.74f : 3.05f;
    public static string Feet(Size s) => s == Size.Small ? "24 x 32 ft" : s == Size.Medium ? "30 x 40 ft" : "40 x 60 ft";

    // Whether a point (local metres, the barn's own frame) is inside its walls.
    public static bool IsInside(Size s, float x, float z) => Math.Abs(x) < Width(s) / 2f - 0.15f && Math.Abs(z) < Length(s) / 2f - 0.15f;

    // --- Storage ---

    public static float CapacityKg(Size s) => s == Size.Small ? 5000f : s == Size.Medium ? 10000f : 20000f;
    public static int UnitsThatFit(Size s, float loadKg, float unitKg) =>
        unitKg <= 0f ? int.MaxValue : Math.Max(0, (int)Math.Floor((CapacityKg(s) - loadKg) / unitKg + 1e-4));
    public static string KgText(Size s, float loadKg) => $"{Math.Round(loadKg):0} of {CapacityKg(s):0} kg";

    // --- The kit: real parts (Barn Posts every 8 ft along both sides, a Truss every 4 ft, Steel Sheets for the roof and walls) ---

    public const string PostId = "barn_post", TrussId = "barn_truss", SheetId = "steel_sheet";
    public const float PostKg = 40f, TrussKg = 70f, SheetKg = 15f;
    public static int Posts(Size s) => s == Size.Small ? 10 : s == Size.Medium ? 12 : 16;
    public static int Trusses(Size s) => s == Size.Small ? 9 : s == Size.Medium ? 11 : 16;
    public static int Sheets(Size s) => s == Size.Small ? 60 : s == Size.Medium ? 75 : 107;
    public static float KitKg(Size s) => Posts(s) * PostKg + Trusses(s) * TrussKg + Sheets(s) * SheetKg;

    // Real 2026 prices at the low (Midwest) end: about $10 a square foot for the kit, $17 built.
    public static long KitCents(Size s) => s == Size.Small ? 750000 : s == Size.Medium ? 1200000 : 2200000;
    public static long CrewCents(Size s) => s == Size.Small ? 1300000 : s == Size.Medium ? 2000000 : 3600000;
    public static long CrewDeposit(Size s) => CrewCents(s) / 2;
    public static long CrewBalance(Size s) => CrewCents(s) - CrewDeposit(s);

    // --- Building it yourself: three stages, each hammered (the materials of that stage must be on the site) ---

    public const int Stages = 3;
    public static string[] StageParts = { PostId, TrussId, SheetId };
    public static int StageCount(Size s, int stage) => stage == 0 ? Posts(s) : stage == 1 ? Trusses(s) : Sheets(s);
    public static int StageBlows(Size s, int stage) => stage == 0 ? Posts(s) * 8 : stage == 1 ? Trusses(s) * 10 : Sheets(s) * 2;
    public static string StageWork(int stage) => stage == 0 ? "set the posts" : stage == 1 ? "raise the trusses" : "put on the roof and walls";
    public static string StageDone(int stage) => stage == 0 ? "Posts set - now raise the trusses (hold click with the Hammer)"
                                               : "Trusses up - now put on the roof and walls (hold click with the Hammer)";

    // --- The crew ---

    public const int LeadDays = 3;
    public static int CrewDays(Size s) => s == Size.Small ? 5 : s == Size.Medium ? 7 : 10;
    public const string CrewMarker = "barn_crew";   // a booked site's jobItem: the crew has it, so the player leaves it alone

    // How far along the crew is (0 before they start, 1 when done), from the day booked and the game hour now.
    public static float CrewProgress(Size s, int bookedDay, double nowHours)
    {
        double start = (bookedDay + LeadDays) * 24.0, end = start + CrewDays(s) * 24.0;
        return nowHours <= start ? 0f : nowHours >= end ? 1f : (float)((nowHours - start) / (end - start));
    }
    public static int CrewStage(float progress) => progress < 1f / 3f ? 0 : progress < 2f / 3f ? 1 : 2;

    // --- Words ---

    public static string Name(Size s) => $"{s} Pole Barn";
    public static string SiteName(Size s) => $"{s} Pole Barn Site";
    public const string NoSite = "Mark out a Pole Barn Site first (Build tab, Other) - the kit and the crew come to it";
    public const string OutsidePrompt = "Walk into the barn to reach its shelves";
    public const string CrewHasIt = "The crew has this site - they'll finish it";
    public static string KitRow(Size s) => $"Order the kit  {Money_Format(KitCents(s))}  ({Posts(s)} posts, {Trusses(s)} trusses, {Sheets(s)} steel sheets, delivered to the site)";
    public static string CrewRow(Size s) => $"Hire the crew  {Money_Format(CrewCents(s))}  (half now; they start in {LeadDays} days and take {CrewDays(s)})";
    public static string CrewByPhone(Size s) => $"Or hire the crew by phone (P, Contractors): {Money_Format(CrewCents(s))}, half now";
    public static string CrewBooked(Size s) => $"Crew booked - {Money_Format(CrewDeposit(s))} paid; they start in {LeadDays} days and finish in about {LeadDays + CrewDays(s)}";
    public static string CrewWaiting(Size s, int daysToStart) => daysToStart > 0 ? $"The crew starts in {daysToStart} day{(daysToStart == 1 ? "" : "s")}" : "The crew is at work";
    public static string CrewDone(Size s) => $"The crew has finished your {Name(s)} - the {Money_Format(CrewBalance(s))} balance is paid";
    public static string CrewOwed(Size s) => $"The crew has finished your {Name(s)} - the {Money_Format(CrewBalance(s))} balance is due; it's paid as soon as you have it";
    public static string Built(Size s) => $"{Name(s)} built - walk in through the big door; its shelves at the back hold up to {CapacityKg(s):0} kg";

    public static string SiteHelp(Size s) =>
        $"a {Feet(s)} post-frame barn with a big sliding doorway and storage shelves at the back for up to {CapacityKg(s):0} kg of anything but water. " +
        $"Mark out the site (free), then at the Trading Post (T, switch to Deliver) order the kit ({Money_Format(KitCents(s))}: {Posts(s)} posts, {Trusses(s)} trusses, " +
        $"{Sheets(s)} steel sheets, delivered to the site) and build it in three stages with the Hammer, or hire the crew by phone (P, Contractors; {Money_Format(CrewCents(s))}, half now, half when done). " +
        $"Once it stands, anything built or crafted within {FeedRadius:0} m can use what is on its shelves, and deliveries go into it.";

    // --- Slice B1b (Mike, 2026-11-27): a barn feeds what is built and crafted within FeedRadius, and takes deliveries ---

    public const float FeedRadius = 50f;
    public static bool Feeds(float dx, float dz) => dx * dx + dz * dz <= FeedRadius * FeedRadius;

    // How many to take from each source in turn (the first sources first) to make up want; the counts add up to less when they run out.
    public static int[] Allocate(int want, System.Collections.Generic.IList<int> have)
    {
        var take = new int[have.Count];
        for (int i = 0; i < have.Count && want > 0; i++)
        {
            take[i] = Math.Max(0, Math.Min(want, have[i]));
            want -= take[i];
        }
        return take;
    }

    public static string LoadPrompt(string what) => $"Load from the barn  ({what})";
    public static string StorePrompt(string what) => $"Store {what}, then the rest from the barn";
    public const string FeedNote = "Counts what is on the shelves of a Pole Barn within 50 m";
    public const string InBarn = "Your Trading Post delivery is in the Pole Barn";
    public const string InBarnAndPad = "Your Trading Post delivery is in the Pole Barn - what didn't fit is on the Delivery Pad";
    public const string BarnFull = "Part of a Trading Post delivery is waiting - make room in the Pole Barn or build a Delivery Pad";

    static string Money_Format(long cents) => Money.Format(cents);
}
