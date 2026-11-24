using System;

// Bark tanning (Tanning_Hides_and_Furs.md section 5, slice T4). Mike, 2026-11-23: the proposals ("proceed"), with one change: stripping bark turns the
// Log into a Peeled Log ("Log becomes Peeled Log"), which counts as a Log wherever Logs are used from the pack (InventoryContainer).
//
// Tanbark: with a Bark Spud carried (Mike, 2026-11-23: "Bark Spud, Smithy-forged"), a Logs row in the Inventory gets a Strip button. Each Log gives 2 Tanbark and becomes a Peeled Log.
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

    // The Bark Spud: a short iron blade on a wooden handle, pushed under the bark to peel it. Forged at the Smithy (1 Iron Bar, 1 Charcoal and a Stick,
    // 2 hours) or bought at the Trading Post. It is carried, not held: strip bark from the Inventory with it in the pack.
    public const string BarkSpudId = "bark_spud";
    public const long BarkSpudStoreCents = 1000;
    public const float BarkSpudKg = 0.6f;

    // --- What building one takes ---

    public const int Logs = 3;
    public const int Clay = 2;

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

    public static string Prompt(Phase phase, int hides, int tanbark, int water, int leather, float hoursLeft)
    {
        switch (phase)
        {
            case Phase.Empty:
                return "Tanning Vat (empty)\nLoad Rawhide, Tanbark and water (R): 4 Tanbark and 20 litres for each hide, up to 4 hides";
            case Phase.Loading:
            {
                int h = Math.Max(1, hides);
                return $"Tanning Vat: {Hides(hides)}, {tanbark} of {h * TanbarkPerHide} Tanbark, {water} of {h * WaterPerHide} litres\nLoad more (R)";
            }
            case Phase.Ready:
                return $"Tanning Vat: {Hides(hides)}, {tanbark} Tanbark, {water} litres\nStart the soak (E): {SoakDays} days";
            case Phase.Soaking:
                return $"Tanning Vat: soaking {Hides(hides)}, {DaysLeft(hoursLeft)} days left\nThe bark tans them slowly; nothing to do but wait";
            default:
                return $"Tanning Vat: {leather} Leather ready\nTake it (E)";
        }
    }

    public const string NothingToLoad = "You have nothing for the vat: Rawhide (from the Hide Frame), Tanbark (strip a Log with a Bark Spud) or water (a Bucket or Canteen)";
    public const string FullWhileSoaking = "The vat is soaking; wait for the Leather";
    public static string Loaded(LoadPlan plan) => $"Loaded {plan.hides} Rawhide, {plan.tanbark} Tanbark and {plan.water} litres of water";
    public static string Started(int hides) => $"The soak has started: {Hides(hides)} in the bark water for {SoakDays} days";
    public static string Finished(int leather) => leather == 1 ? "The Tanning Vat is done: 1 Leather is ready" : $"The Tanning Vat is done: {leather} Leather are ready";
    public static string Took(int took, int left) => took <= 0 ? "No room in your pack for the Leather" : left > 0 ? $"Took {took} Leather ({left} left: no more room)" : $"Took {took} Leather";
    public const string DismantlePrompt = "Dismantle the Tanning Vat (R)";
    public const string CantDismantle = "The vat is soaking hides; wait until they are Leather to take it down";
    public static string Stripped(int logs) => logs == 1 ? $"Stripped the bark from 1 Log: {TanbarkPerLog} Tanbark and a Peeled Log" : $"Stripped the bark from {logs} Logs: {logs * TanbarkPerLog} Tanbark and {logs} Peeled Logs";
    public const string NeedsSpud = "Stripping bark needs a Bark Spud in your pack (forge one at the Smithy, or buy one at the Trading Post)";
    public const string Help = "a sunken log trough. Load Rawhide, Tanbark and water (R): 4 Tanbark and 20 litres for each hide, up to 4. Start it (E) and in 7 days the hides are Leather.";
}
