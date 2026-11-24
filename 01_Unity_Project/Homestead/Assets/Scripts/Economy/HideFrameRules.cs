using System;

// The Hide Frame (Tanning_Hides_and_Furs.md section 3, slice T2). Mike, 2026-11-23: "Structure: a Hide Frame", built with the proposed numbers
// ("commit t1 and start t2").
//
// A square pole frame about 2 m tall, built in one go with the Hammer like the Drying Rack: 4 Sticks, 2 Branches and 2 Cordage. R laces one hide
// into it: a Deer Hide (raw or salted), or a bundle of up to 4 Small Furs (raw or salted). With the Knife equipped, holding click scrapes it, one
// stroke per Knife blow: 20 strokes for a Deer Hide, 8 for furs. Scraped, it dries on the frame in daylight while it is not raining (the Drying
// Rack's rule), 12 such hours. A dried Deer Hide is Rawhide; dried furs are Dried Furs, one for each fur. E takes them off. A hide on the frame does
// not age (SpoilageManager leaves the frame out of its sweep). The state lives on the frame's WoodPileState: the hide (or what it has become) in
// contents, the strokes so far in jobHours while scraping, and jobItem = DryingMarker with jobHours = the hours dried so far while drying.
// Pure and engine-free so edit-mode tests cover it.
public static class HideFrameRules
{
    public const string KnifeId = "knife";
    public const string RawhideId = "rawhide";
    public const string DriedFursId = "dried_furs";
    public const string DryingMarker = "drying";

    // --- What building one takes ---

    public const int Sticks = 4;
    public const int Branches = 2;
    public const int Cordage = 2;

    // --- The work ---

    public const int FursPerBundle = 4;
    public const int DeerStrokes = 20;
    public const int FursStrokes = 8;
    public const int DryHours = 12;
    public const float ReachMetres = 1.8f;

    // Trading Post prices (cents): Rawhide pays more than the raw hide ($8), Dried Furs a dollar more than raw ($3).
    public const long RawhideCents = 1200;
    public const long DriedFursCents = 400;

    public enum Kind { None, Deer, Furs }
    public enum Phase { Empty, Scraping, Drying, Done }

    // Which kind of hide an item is, for the frame.
    public static Kind KindOf(string itemId)
    {
        switch (itemId)
        {
            case HideRules.DeerHideId:
            case HideRules.SaltedDeerHideId: return Kind.Deer;
            case HideRules.SmallFursId:
            case HideRules.SaltedFursId: return Kind.Furs;
            default: return Kind.None;
        }
    }

    public static bool Laces(string itemId) => KindOf(itemId) != Kind.None;

    public static bool IsOutput(string itemId) => itemId == RawhideId || itemId == DriedFursId;

    // How many of a hide go on at once: one Deer Hide, or up to a bundle of furs.
    public static int MaxOn(Kind kind) => kind == Kind.Deer ? 1 : kind == Kind.Furs ? FursPerBundle : 0;

    public static int StrokesFor(Kind kind) => kind == Kind.Deer ? DeerStrokes : kind == Kind.Furs ? FursStrokes : 0;

    // What a scraped hide dries into, and how many: one Rawhide for a Deer Hide, one Dried Furs for each fur.
    public static string OutputFor(Kind kind) => kind == Kind.Deer ? RawhideId : kind == Kind.Furs ? DriedFursId : null;
    public static int OutputCount(Kind kind, int pieces) => kind == Kind.Deer ? Math.Min(1, pieces) : kind == Kind.Furs ? pieces : 0;

    // Where the frame is: nothing on it, a hide being scraped, a scraped hide drying, or Rawhide / Dried Furs ready to take.
    public static Phase PhaseOf(bool hasHide, bool hasOutput, string jobItem)
    {
        if (hasOutput)
            return jobItem == DryingMarker ? Phase.Drying : Phase.Done;
        return hasHide ? Phase.Scraping : Phase.Empty;
    }

    // One Knife stroke: the strokes so far plus one; done when they reach what the hide needs.
    public static float AfterStroke(float strokes, Kind kind, out bool done)
    {
        int needed = StrokesFor(kind);
        float next = Math.Max(0f, strokes) + 1f;
        done = needed > 0 && next >= needed - 0.001f;
        return done ? 0f : next;
    }

    // One hour that dries (daylight, no rain or snow): the hours so far plus one; done at DryHours.
    public static float AfterDryHour(float hours, out bool done)
    {
        float next = Math.Max(0f, hours) + 1f;
        done = next >= DryHours - 0.001f;
        return done ? 0f : next;
    }

    public static int DryHoursLeft(float hours) => Math.Max(0, (int)Math.Ceiling(DryHours - hours));

    public static int Percent(float strokes, Kind kind)
    {
        int needed = StrokesFor(kind);
        return needed <= 0 ? 0 : Math.Min(100, (int)Math.Round(100.0 * Math.Max(0f, strokes) / needed));
    }

    // --- Text ---

    static string HideName(Kind kind, int pieces) => kind == Kind.Deer ? "a Deer Hide" : pieces == 1 ? "1 Small Furs" : $"{pieces} Small Furs";

    // The frame's prompt, phase by phase.
    public static string Prompt(Phase phase, Kind kind, int pieces, float jobHours, bool knife, bool dryingNow, string outputName, int outputCount)
    {
        switch (phase)
        {
            case Phase.Empty:
                return "Hide Frame\nLace a hide into it (R): a Deer Hide, or up to 4 Small Furs, raw or salted";
            case Phase.Scraping:
                return $"Hide Frame: {HideName(kind, pieces)}, scraped {(int)jobHours} of {StrokesFor(kind)} strokes\n"
                     + (knife ? "Scrape it (hold click)" : "Equip the Knife to scrape it") + (jobHours <= 0f ? ";  take it back off (E)" : "");
            case Phase.Drying:
                return $"Hide Frame: drying, {DryHoursLeft(jobHours)} daylight hours left\n"
                     + (dryingNow ? "It dries in daylight while it is not raining" : "Drying paused: it dries only in daylight while it is not raining");
            default:
                return $"Hide Frame: {outputCount} {outputName} ready\nTake it (E)";
        }
    }

    public const string NoHide = "You have no hide to lace in: a Deer Hide or Small Furs, raw or salted";
    public const string Busy = "The frame already has a hide on it";
    public static string Laced(Kind kind, int pieces) => $"Laced {HideName(kind, pieces)} into the frame. Scrape it with the Knife (hold click)";
    public static string TookBack(Kind kind, int pieces) => $"Took {HideName(kind, pieces)} back off the frame";
    public static string Scraped(Kind kind) =>
        kind == Kind.Deer ? "Scraped clean of flesh and hair. Now it dries on the frame: 12 daylight hours" : "Scraped clean of flesh. Now they dry on the frame: 12 daylight hours";
    public static string Dried(string outputName) => $"A hide on the frame has dried: {outputName} is ready to take";
    public static string Took(int took, string outputName, int left) =>
        took <= 0 ? $"No room in your pack for the {outputName}" : left > 0 ? $"Took {took} {outputName} ({left} left: no more room)" : $"Took {took} {outputName}";
    public const string DismantlePrompt = "Dismantle the Hide Frame (R)";
    public const string Help = "lace a Deer Hide or up to 4 Small Furs into it (R), raw or salted, and scrape it with the Knife (hold click). It then dries in daylight while it is not raining, about 12 hours, into Rawhide or Dried Furs (E takes them).";
}
