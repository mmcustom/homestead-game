using System;

// Bark tanning (Tanning_Hides_and_Furs.md section 5, slice T4). Mike, 2026-11-23: the proposals ("proceed"), with one change: stripping bark turns the
// Log into a Peeled Log ("Log becomes Peeled Log"), which counts as a Log wherever Logs are used from the pack (InventoryContainer).
//
// Tanbark: with the Draw Knife in hand, hold click on a Log laid on the Saw Pit (Mike, 2026-11-25: "a draw knife tool equipped at the saw pit to get tanbark
// instead of doing it from inventory"; it replaces the Bark Spud, which kept its item id so a Spud already owned is now a Draw Knife). Each Log takes
// StripStrokes strokes, gives 2 Tanbark stacked by the pit and stays on it as a Peeled Log, ready to saw (SawPitRules). (Until 2026-11-25 a Bark Spud carried
// put a Strip button on the Inventory's Logs row.)
// The Tanning Vat: a sunken log trough with a lid, 3 Logs and 2 Clay, one click with the Hammer. R loads Rawhide (up to 4), Tanbark (4 per hide) and
// water (20 litres per hide, from the buckets and canteens carried). E starts the soak once there is bark and water for every hide; the bark and water go
// in then. The hides soak 7 game days (halved from 14 for gameplay, Mike 2026-11-23), counted hour by hour like the Smokehouse, so being away or asleep counts, and come out as Leather, which E takes.
// The state lives on the vat's WoodPileState: Rawhide, Tanbark, water litres (as the water item) and Leather in contents, and while soaking jobItem =
// SoakMarker with jobHours = the hours left. Pure and engine-free so edit-mode tests cover it.
public static class TanningVatRules
{
    public const string LogsId = "logs";
    public const string PeeledLogId = "peeled_log";
    public const string TanbarkId = "tanbark";
    public const string RawhideId = HideFrameRules.RawhideId;
    public const string LeatherId = "leather";
    public const string ClayId = "clay";
    public const string WaterId = "vat_water"; // the litres poured in, kept as one stack whatever their quality (the vat does not care)
    public const string SoakMarker = "soaking";

    // --- Stripping bark ---

    public const int TanbarkPerLog = 2;

    // The Draw Knife: a long blade with a handle at each end, pulled toward you with both hands to peel the bark off a Log. Forged at the Smithy (1 Iron Bar,
    // 1 Charcoal and a Stick, 2 hours) or bought at the Trading Post. Held in hand and used at the Saw Pit. Its item id is still the Bark Spud's (it
    // replaced the Spud on 2026-11-25), so saves and the shop rows carry over: DrawKnifeId and BarkSpudId are the same id.
    public const string BarkSpudId = "bark_spud";
    public const string DrawKnifeId = BarkSpudId;
    public const string DrawKnifeName = "Draw Knife";
    public const int StripStrokes = 8;      // draw-knife strokes to peel one Log
    public const long BarkSpudStoreCents = 1000;
    public const float BarkSpudKg = 0.6f;

    // --- What building one takes ---

    public const int Logs = 3;
    public const int Clay = 2;

    // A staged site since 2026-11-25 (Mike: "tanning vat needs to be staged building site"; "Dig, stock, hammer"): marked out free, the pit dug with the
    // Shovel (it is a sunken trough), the 3 Logs and 2 Clay brought in any order, then hammered up, like the Saw Pit.
    public const int DigBlows = 10;
    public const int BuildBlows = 20;
    public const float ReachMetres = 1.0f;   // the trough's half-diagonal (1.6 m by 0.9 m), for the trees on its spot
    public const string BuiltMessage = "Tanning Vat built - load Rawhide, Tanbark and water (R), then start the soak (E).";

    // --- The soak ---

    public const int HidesCap = 4;
    public const int TanbarkPerHide = 4;
    public const int WaterPerHide = 20;
    public const int SoakDays = 7;                           // 14 until Mike halved it for gameplay (2026-11-23)
    public const int SoakHours = SoakDays * 24;

    // Trading Post price (cents): Leather pays the most of any hide ($8 raw, $12 Rawhide, $24 Buckskin).
    public const long LeatherCents = 3000;

    public enum Phase { Empty, Loading, Ready, Soaking, Done }

    public static Phase PhaseOf(bool soaking, int hides, int tanbark, int water, int leather)
    {
        if (soaking)
            return Phase.Soaking;
        if (leather > 0)
            return Phase.Done;
        if (hides <= 0 && tanbark <= 0 && water <= 0)
            return Phase.Empty;
        return CanStart(hides, tanbark, water) ? Phase.Ready : Phase.Loading;
    }

    public static bool CanStart(int hides, int tanbark, int water) =>
        hides > 0 && tanbark >= hides * TanbarkPerHide && water >= hides * WaterPerHide;

    // What one R press loads, given what the vat holds and what is carried: Rawhide up to the cap, then the bark and water those hides need (and no more).
    public struct LoadPlan
    {
        public int hides, tanbark, water;
        public bool Any => hides > 0 || tanbark > 0 || water > 0;
    }

    public static LoadPlan PlanLoad(int hidesHeld, int tanbarkHeld, int waterHeld, int hidesCarried, int tanbarkCarried, int waterCarried)
    {
        var plan = new LoadPlan { hides = Math.Max(0, Math.Min(hidesCarried, HidesCap - Math.Max(0, hidesHeld))) };
        int hides = Math.Max(0, hidesHeld) + plan.hides;
        int barkTarget = (hides > 0 ? hides : HidesCap) * TanbarkPerHide;
        int waterTarget = (hides > 0 ? hides : HidesCap) * WaterPerHide;
        plan.tanbark = Math.Max(0, Math.Min(tanbarkCarried, barkTarget - Math.Max(0, tanbarkHeld)));
        plan.water = Math.Max(0, Math.Min(waterCarried, waterTarget - Math.Max(0, waterHeld)));
        return plan;
    }

    public static float AfterHour(float hoursLeft, out bool done)
    {
        float next = Math.Max(0f, hoursLeft - 1f);
        done = next <= 0f;
        return next;
    }

    public static int DaysLeft(float hoursLeft) => Math.Max(0, (int)Math.Ceiling(hoursLeft / 24f - 0.0001f));

    // --- Text ---

    static string Hides(int n) => n == 1 ? "1 Rawhide" : $"{n} Rawhide";

    // The vat's status, shown above the key lines with no key of its own (InteractionPromptHud, IPromptHeader). The key lines say only what each key
    // does (Mike, 2026-11-25: the old prompt was confusing).
    public static string Header(Phase phase, int hides, int tanbark, int water, int leather, float hoursLeft)
    {
        switch (phase)
        {
            case Phase.Empty:
                return $"Tanning Vat (empty)\nIt tans Rawhide into Leather. Each hide needs {TanbarkPerHide} Tanbark and {WaterPerHide} litres of water; up to {HidesCap} hides";
            case Phase.Loading:
            {
                if (hides <= 0)
                    return $"Tanning Vat: no Rawhide yet ({tanbark} Tanbark, {water} litres in)\nIt takes Rawhide only: a Deer Hide scraped on the Hide Frame and dried there 12 daylight hours";
                int needBark = Math.Max(0, hides * TanbarkPerHide - tanbark), needWater = Math.Max(0, hides * WaterPerHide - water);
                string missing = needBark > 0 && needWater > 0 ? $"{needBark} more Tanbark and {needWater} more litres of water"
                               : needBark > 0 ? $"{needBark} more Tanbark" : $"{needWater} more litres of water";
                return $"Tanning Vat: {Hides(hides)}, {tanbark} Tanbark, {water} litres\nStill needs {missing}";
            }
            case Phase.Ready:
                return $"Tanning Vat: {Hides(hides)}, {tanbark} Tanbark, {water} litres\nReady to soak";
            case Phase.Soaking:
                return $"Tanning Vat: soaking {Hides(hides)}, {DaysLeft(hoursLeft)} days left\nThe bark tans them slowly; nothing to do but wait";
            default:
                return $"Tanning Vat: {leather} Leather ready";
        }
    }

    // What E does at the vat, or null when E does nothing there.
    public static string ActionE(Phase phase) =>
        phase == Phase.Ready ? $"Start the soak ({SoakDays} days)" : phase == Phase.Done ? "Take the Leather" : null;

    // What R would load, as its key line.
    public static string LoadPrompt(LoadPlan plan)
    {
        var parts = new System.Collections.Generic.List<string>();
        if (plan.hides > 0) parts.Add(Hides(plan.hides));
        if (plan.tanbark > 0) parts.Add($"{plan.tanbark} Tanbark");
        if (plan.water > 0) parts.Add($"{plan.water} litres of water");
        return "Load " + string.Join(", ", parts);
    }

    public static string Prompt(Phase phase, int hides, int tanbark, int water, int leather, float hoursLeft) => Header(phase, hides, tanbark, water, leather, hoursLeft);

    public const string NothingToLoad = "Nothing to load: it takes Rawhide, Tanbark and water";

    // Why R loads nothing, for what the player is carrying (Mike, 2026-11-25: "it doesn't allow me to add any hides or furs?"). rawHides: a Deer Hide
    // (raw or salted) carried; furs: Small Furs or Dried Furs. Null when neither, so the plain NothingToLoad shows.
    public static string WrongHideHint(bool rawHides, bool furs) =>
        rawHides ? "A Deer Hide goes on the Hide Frame first: scrape it there, and it dries into Rawhide for the vat"
        : furs ? "Furs are not bark-tanned: soften them on the Hide Frame, then smoke them for Tanned Furs"
        : null;
    public const string FullWhileSoaking = "The vat is soaking; wait for the Leather";
    public static string Loaded(LoadPlan plan) => $"Loaded {plan.hides} Rawhide, {plan.tanbark} Tanbark and {plan.water} litres of water";
    public static string Started(int hides) => $"The soak has started: {Hides(hides)} in the bark water for {SoakDays} days";
    public static string Finished(int leather) => leather == 1 ? "The Tanning Vat is done: 1 Leather is ready" : $"The Tanning Vat is done: {leather} Leather are ready";
    public static string Took(int took, int left) => took <= 0 ? "No room in your pack for the Leather" : left > 0 ? $"Took {took} Leather ({left} left: no more room)" : $"Took {took} Leather";
    public const string DismantlePrompt = "Dismantle the Tanning Vat";
    public const string CantDismantle = "The vat is soaking hides; wait until they are Leather to take it down";
    public static string Stripped(int logs) => logs == 1 ? $"Stripped the bark from 1 Log: {TanbarkPerLog} Tanbark and a Peeled Log" : $"Stripped the bark from {logs} Logs: {logs * TanbarkPerLog} Tanbark and {logs} Peeled Logs";
    public const string NeedsDrawKnife = "Strip bark at the Saw Pit: lay Logs on it, equip the Draw Knife (Smithy or Trading Post) and hold click";
    public const string Help = "a sunken log trough that tans Rawhide into Leather. Load Rawhide, Tanbark and water (R): 4 Tanbark and 20 litres for each hide, up to 4. Start the soak (E) and in 7 days the hides are Leather.";
    public static readonly string SiteHelp = $"a sunken log trough that tans Rawhide into Leather with Tanbark and water. Mark the site out (free, no Hammer), dig the pit with the Shovel (hold click), deliver {Logs} Logs and {Clay} Clay with R in any order, then equip the Hammer and hold click on the site until it is built.";
}
