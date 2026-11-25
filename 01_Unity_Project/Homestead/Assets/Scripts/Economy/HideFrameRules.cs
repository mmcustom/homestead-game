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

    // Brain tanning on the frame (Tanning_Hides_and_Furs.md section 4, slice T3): Rawhide or Dried Furs, worked with the Knife while the brain-and-water
    // mix soaks in, become a Soft Hide or Soft Furs for the Smokehouse (SmokeRules). The first stroke takes WaterLitres of water from the pack, and needs a
    // Crock Pot carried to mix it in. Each animal's brains do for its own hide, so there is no Brains item (Mike, 2026-11-23: the proposals).
    public const string SoftHideId = "soft_hide";
    public const string SoftFursId = "soft_furs";
    public const string SofteningMarker = "softening";
    public const int DeerSoftenStrokes = 30;
    public const int FursSoftenStrokes = 10;
    public const int WaterLitres = 1;

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
    public enum Phase { Empty, Scraping, Drying, Softening, Done }

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

    public static bool IsOutput(string itemId) => itemId == RawhideId || itemId == DriedFursId || itemId == SoftHideId || itemId == SoftFursId;

    // What brain tanning on the frame turns an output into: Rawhide into a Soft Hide, Dried Furs into Soft Furs. False for anything else.
    public static bool TrySoften(string itemId, out string softId)
    {
        softId = itemId == RawhideId ? SoftHideId : itemId == DriedFursId ? SoftFursId : null;
        return softId != null;
    }

    public static bool Softens(string itemId) => TrySoften(itemId, out _);

    // The kind of hide a frame output is (for the strokes it takes and how it is drawn).
    public static Kind KindOfOutput(string itemId) =>
        itemId == RawhideId || itemId == SoftHideId ? Kind.Deer : itemId == DriedFursId || itemId == SoftFursId ? Kind.Furs : Kind.None;

    // Laced in from the pack to be softened: Rawhide (one) or Dried Furs (a bundle of up to 4).
    public static bool LacesToSoften(string itemId) => itemId == RawhideId || itemId == DriedFursId;

    public static int SoftenStrokesFor(Kind kind) => kind == Kind.Deer ? DeerSoftenStrokes : kind == Kind.Furs ? FursSoftenStrokes : 0;

    // One softening stroke: the strokes so far plus one; done when they reach what the hide needs.
    public static float AfterSoftenStroke(float strokes, Kind kind, out bool done)
    {
        int needed = SoftenStrokesFor(kind);
        float next = Math.Max(0f, strokes) + 1f;
        done = needed > 0 && next >= needed - 0.001f;
        return done ? 0f : next;
    }

    public static int SoftenPercent(float strokes, Kind kind)
    {
        int needed = SoftenStrokesFor(kind);
        return needed <= 0 ? 0 : Math.Min(100, (int)Math.Round(100.0 * Math.Max(0f, strokes) / needed));
    }

    // What the first softening stroke needs: a Crock Pot and the water. Null when it can start.
    public static string SoftenRefusal(bool crock, int litres) =>
        !crock && litres < WaterLitres ? "Softening needs a Crock Pot and 1 litre of water in your pack (a Canteen or Bucket)"
        : !crock ? "Softening needs a Crock Pot in your pack to mix the brains and water"
        : litres < WaterLitres ? "Softening needs 1 litre of water in your pack (a Canteen or Bucket)" : null;

    public static string Softened(Kind kind) =>
        kind == Kind.Deer ? "Worked soft and white: a Soft Hide. Smoke it in the Smokehouse to make Buckskin, or it will stiffen"
                          : "Worked soft: Soft Furs. Smoke them in the Smokehouse to make Tanned Furs";

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
            return jobItem == DryingMarker ? Phase.Drying : jobItem == SofteningMarker ? Phase.Softening : Phase.Done;
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

    // The frame's status, shown above the key lines with no key of its own (InteractionPromptHud, IPromptHeader): where the hide is and what to do
    // next. Each key line then says only what that key does (Mike, 2026-11-25: the old prompt repeated itself on the E and R lines).
    public static string Header(Phase phase, Kind kind, int pieces, float jobHours, bool knife, bool dryingNow, string outputName, int outputCount, bool softenable = false)
    {
        switch (phase)
        {
            case Phase.Empty:
                return "Hide Frame (empty)";
            case Phase.Scraping:
                return $"Hide Frame: {HideName(kind, pieces)}, scraped {(int)jobHours} of {StrokesFor(kind)} strokes\n"
                     + (knife ? "Hold click to scrape it" : "Equip the Knife, then hold click to scrape it");
            case Phase.Drying:
                return $"Hide Frame: drying, {DryHoursLeft(jobHours)} daylight hours left\n"
                     + (dryingNow ? "It dries by itself in daylight while it is not raining" : "Drying paused until daylight with no rain");
            case Phase.Softening:
                return $"Hide Frame: softening the {outputName}, {(int)jobHours} of {SoftenStrokesFor(kind)} strokes\n"
                     + (knife ? "Hold click to keep working it soft" : "Equip the Knife, then hold click to keep working it soft");
            default:
            {
                string head = $"Hide Frame: {outputCount} {outputName} ready";
                if (!softenable)
                    return head + (kind == Kind.Deer ? "\nSmoke it in the Smokehouse to make Buckskin" : "\nSmoke them in the Smokehouse to make Tanned Furs");
                string soften = (knife ? "hold click" : "equip the Knife and hold click") + " (needs a Crock Pot and 1 litre of water in your pack)";
                return kind == Kind.Deer
                    ? head + "\nFor Leather: take it to the Tanning Vat\nFor Buckskin: soften it here - " + soften
                    : head + "\nFor Tanned Furs: soften them here - " + soften;
            }
        }
    }

    // What E does at the frame, or null when E does nothing there.
    public static string ActionE(Phase phase, float jobHours, string outputName) =>
        phase == Phase.Scraping && jobHours <= 0f ? "Take the hide back off"
        : phase == Phase.Done ? $"Take the {outputName}"
        : null;

    // Header and E together, for the message flashed when E has nothing to do.
    public static string Prompt(Phase phase, Kind kind, int pieces, float jobHours, bool knife, bool dryingNow, string outputName, int outputCount, bool softenable = false) =>
        Header(phase, kind, pieces, jobHours, knife, dryingNow, outputName, outputCount, softenable);

    public static string LacePrompt(string hideName) => $"Lace in your {hideName}";

    public const string NoHide = "No hide to lace in (a Deer Hide or Small Furs to scrape; Rawhide or Dried Furs to soften)";
    public const string Busy = "The frame already has a hide on it";
    public static string Laced(Kind kind, int pieces) => $"Laced {HideName(kind, pieces)} into the frame. Equip the Knife and hold click to scrape it";
    public static string LacedToSoften(Kind kind, int pieces) =>
        (kind == Kind.Deer ? "Laced the Rawhide into the frame" : pieces == 1 ? "Laced 1 Dried Furs into the frame" : $"Laced {pieces} Dried Furs into the frame")
        + ". Equip the Knife and hold click to work it soft (needs a Crock Pot and 1 litre of water)";
    public static string TookBack(Kind kind, int pieces) => $"Took {HideName(kind, pieces)} back off the frame";
    public static string Scraped(Kind kind) =>
        kind == Kind.Deer ? "Scraped clean of flesh and hair. Now it dries on the frame: 12 daylight hours" : "Scraped clean of flesh. Now they dry on the frame: 12 daylight hours";
    public static string Dried(string outputName) => $"A hide on the frame has dried: {outputName} is ready to take";
    public static string Took(int took, string outputName, int left) =>
        took <= 0 ? $"No room in your pack for the {outputName}" : left > 0 ? $"Took {took} {outputName} ({left} left: no more room)" : $"Took {took} {outputName}";
    public const string DismantlePrompt = "Dismantle the Hide Frame";
    public const string Help = "lace a Deer Hide or up to 4 Small Furs into it (R), raw or salted, and scrape it with the Knife (hold click). It then dries in daylight while it is not raining, about 12 hours, into Rawhide or Dried Furs (E takes them).";
}
