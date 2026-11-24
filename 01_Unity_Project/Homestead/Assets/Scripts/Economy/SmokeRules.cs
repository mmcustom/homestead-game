using System;

// The Smokehouse (Food_Spoilage_and_Preservation.md section 5, slice 5). Mike, 2026-11-22: a staged build like the Kiln ("Staged build"), smoked
// food is the top of the line ("Top of the line": one piece per dollar the raw piece is worth cooked, $3.00 each, so a Venison makes 4 Smoked Meat
// worth $12 against $10 salted, $6.40 dried and $4 cooked), and the fire works as the design doc proposed ("As proposed").
//
// The player loads raw meat and fish (up to MeatCap pieces) and Firewood (up to FirewoodCap) with R, lights it with E, and comes back a day later.
// A load smokes for SmokeHours game hours, counted one hour at a time like the Kiln and the Bloomery, so being away, sleeping and skipping days all
// count. One Firewood burns HoursPerFirewood hours, so a full day's smoking burns 6. If the wood runs out the fire dies down and smoking pauses with
// the hours it has left; the meat then ages again like any raw meat outdoors. Loading more Firewood starts it again from the embers.
// While the fire burns the meat does not age (SpoilageManager moves its stamp on by the hour's outdoor clock).
//
// How the saved pile holds it (no new field): the raw pieces, the Firewood and the smoked food waiting are ordinary stacks in contents (the raw
// and smoked keep their freshness stamps). A load is jobItem (SmokeMarker) and jobHours (smoking hours left); jobCount is the hours left in the
// Firewood burning now. When the hours run out every raw piece becomes its smoked pieces, fresh, and the job fields are cleared. Pure and
// engine-free so edit-mode tests cover it.
public static class SmokeRules
{
    public const string SmokedMeatId = "smoked_meat";
    public const string SmokedFishId = "smoked_fish";
    public const string FirewoodId = PotteryRules.FirewoodId;
    public const string LogsId = "logs";
    public const string LumberId = "lumber";
    public const string StoneId = "stone";
    public const string ClayId = "clay";

    public static readonly string[] SmokedIds = { SmokedMeatId, SmokedFishId };

    // Brain tanning's second step (Tanning_Hides_and_Furs.md section 4, slice T3): a Soft Hide or Soft Furs from the Hide Frame smokes with the meat
    // and comes out as Buckskin or Tanned Furs. A Soft Hide takes the room of 2 pieces of meat, Soft Furs 1; each makes one.
    public const string SoftHideId = "soft_hide";
    public const string SoftFursId = "soft_furs";
    public const string BuckskinId = "buckskin";
    public const string TannedFursId = "tanned_furs";
    public const long BuckskinCents = 2400;
    public const long TannedFursCents = 600;
    public static readonly string[] HideIds = { SoftHideId, SoftFursId };

    public static bool IsSoftHide(string itemId) => itemId == SoftHideId || itemId == SoftFursId;

    // The room one piece takes in a load, in pieces of meat.
    public static int RoomOf(string itemId) => itemId == SoftHideId ? 2 : 1;

    // The raw meat and fish it smokes (SaltRules.TryFind's list), in the order R loads them from the pack.
    public static readonly string[] RawIds = { "venison", "turkey_meat", "small_game_meat", "waterfowl_meat", "chicken_meat", "bass", "bluegill", "catfish", "crappie" };

    // --- The site: staged like the Kiln, finished with the Hammer (about 190 kg: 12 Logs at 8 kg, 6 Lumber at 5, 10 Stone at 5, 4 Clay at 3.5) ---
    // The Stone and Clay go in first (the fire pit), then the Logs and Lumber (the house over it).

    public const int SiteLogs = 12;
    public const int SiteLumber = 6;
    public const int SiteStone = 10;
    public const int SiteClay = 4;
    public const int BuildBlows = 48;

    // --- The load and the fire ---

    public const int MeatCap = 20;          // raw pieces of meat and fish in one load
    public const int FirewoodCap = 12;      // Firewood stocked beside the fire pit (two loads' worth)
    public const int SmokeHours = 24;       // one game day of smoke
    public const int HoursPerFirewood = 4;  // so a load burns 6 Firewood
    public const int FirewoodPerLoad = SmokeHours / HoursPerFirewood;
    public const string SmokeMarker = "smoking";

    // Smoked pieces for every dollar the cooked piece is worth, and what each sells for (cents).
    public const int PiecesPerCookedDollar = 1;
    public const long SmokedMeatCents = 300;
    public const long SmokedFishCents = 300;

    // --- Where it may stand: a fire under a roof gives off smoke and sparks, so it keeps the Kiln's clear air from other buildings and trees ---

    public const float RadiusMetres = 1.2f;   // the 2 m square house's reach from its centre (to the middle of a side, plus its eaves)
    public const float ClearanceMetres = PotteryRules.KilnClearanceMetres;
    public const float TreeReachMetres = RadiusMetres + ClearanceMetres;

    // The doorway in the front wall (+Z), for its swinging door (SwingDoor): 0.7 m wide, from the sill to under the lintel.
    public const float DoorHalfWidth = 0.35f;
    public const float DoorSill = 0.17f;
    public const float DoorHeight = 1.34f;
    public const float DoorZ = 1.0f;

    // --- The smoke from the roof vent (KilnSmoke): presentation numbers ---

    public const float SmokeHeight = 2.5f;
    public const float SmokeSpread = 0.12f;
    public const float SmokeSizeMin = 0.4f;
    public const float SmokeSizeMax = 0.75f;
    public const float SmokePuffsPerSecond = 2f;
    public const float SmokeRiseSpeed = 0.45f;
    public const int SmokeMaxPuffs = 36;

    // --- What smokes ---

    // What a raw piece smokes into. False for anything that is not raw meat or fish.
    public static bool TryFind(string itemId, out string smokedId)
    {
        if (IsSoftHide(itemId))
        {
            smokedId = itemId == SoftHideId ? BuckskinId : TannedFursId;
            return true;
        }
        if (!SaltRules.TryFind(itemId, out string salted))
        {
            smokedId = null;
            return false;
        }
        smokedId = salted == SaltRules.SaltFishId ? SmokedFishId : SmokedMeatId;
        return true;
    }

    public static bool Smokes(string itemId) => TryFind(itemId, out _);

    public static bool IsSmoked(string itemId) => itemId == SmokedMeatId || itemId == SmokedFishId || itemId == BuckskinId || itemId == TannedFursId;

    public static bool IsTannedHide(string itemId) => itemId == BuckskinId || itemId == TannedFursId;

    // Smoked pieces one raw piece gives: one per dollar of its cooked price, at least one.
    public static int YieldOf(string rawId) => IsSoftHide(rawId) ? 1 : Smokes(rawId) ? Math.Max(1, DryingRules.CookedCents(rawId) * PiecesPerCookedDollar / 100) : 0;

    // --- Phases ---

    public enum Phase { Empty, Loading, Ready, Smoking, Paused, Done }

    // smoking: a load is in. fuelHours: hours left in the Firewood burning now. raw, firewood, smoked: what it holds.
    public static Phase PhaseOf(bool smoking, int fuelHours, int raw, int firewood, int smoked)
    {
        if (smoking)
            return fuelHours > 0 || firewood > 0 ? Phase.Smoking : Phase.Paused;
        if (smoked > 0)
            return Phase.Done;
        if (raw <= 0 && firewood <= 0)
            return Phase.Empty;
        return raw > 0 && firewood > 0 ? Phase.Ready : Phase.Loading;
    }

    public static bool CanStart(Phase phase) => phase == Phase.Ready;

    // The Hammer's R takes it down at any time but while a load is in (the fire is lit, or the meat is half smoked).
    public static bool CanDismantle(Phase phase) => phase != Phase.Smoking && phase != Phase.Paused;

    // --- Loading: raw meat and fish only between loads, Firewood at any time ---

    public static int MeatRoom(int rawHeld, bool smoking) => smoking ? 0 : Math.Max(0, MeatCap - Math.Max(0, rawHeld));

    public static int FirewoodRoom(int firewoodHeld) => Math.Max(0, FirewoodCap - Math.Max(0, firewoodHeld));

    public static bool Accepts(string itemId, int rawHeld, int firewoodHeld, bool smoking) =>
        (Smokes(itemId) && MeatRoom(rawHeld, smoking) >= RoomOf(itemId)) || (itemId == FirewoodId && FirewoodRoom(firewoodHeld) > 0);

    public struct LoadPlan
    {
        public int raw, firewood;
        public bool Any => raw > 0 || firewood > 0;
    }

    // What one R press loads: the raw meat and fish carried up to the meat room, and the Firewood carried up to the Firewood room.
    public static LoadPlan PlanLoad(int rawHeld, int firewoodHeld, bool smoking, int rawCarried, int firewoodCarried) => new LoadPlan
    {
        raw = Math.Max(0, Math.Min(rawCarried, MeatRoom(rawHeld, smoking))),
        firewood = Math.Max(0, Math.Min(firewoodCarried, FirewoodRoom(firewoodHeld))),
    };

    // --- The fire, one game hour at a time ---

    public struct Hour
    {
        public float hoursLeft;  // smoking hours left after this hour
        public int fuelHours;    // hours left in the Firewood burning
        public int firewoodUsed; // Firewood put on the fire this hour (0 or 1)
        public bool smoked;      // whether the fire burned this hour (false: out of wood, paused)
    }

    // One hour of a lit load: a new Firewood goes on when the last has burned through, if there is one; without it the hour is lost.
    public static Hour AfterHour(float hoursLeft, int fuelHours, int firewood)
    {
        var hour = new Hour { hoursLeft = Math.Max(0f, hoursLeft), fuelHours = Math.Max(0, fuelHours) };
        if (hour.fuelHours <= 0 && firewood > 0)
        {
            hour.firewoodUsed = 1;
            hour.fuelHours = HoursPerFirewood;
        }
        if (hour.fuelHours <= 0)
            return hour;
        hour.fuelHours--;
        hour.hoursLeft = Math.Max(0f, hour.hoursLeft - 1f);
        hour.smoked = true;
        return hour;
    }

    public static bool IsDone(float hoursLeft) => hoursLeft <= 0f;

    // Firewood the rest of a load still burns, beyond what is burning now.
    public static int FirewoodStillNeeded(float hoursLeft, int fuelHours)
    {
        double after = Math.Max(0.0, Math.Ceiling(hoursLeft - 0.0001) - Math.Max(0, fuelHours));
        return (int)Math.Ceiling(after / HoursPerFirewood);
    }

    // --- The words ---

    public static string HoursText(float hoursLeft) => $"{Math.Max(0, (int)Math.Ceiling(hoursLeft - 0.0001f))} h";

    public static string PiecesText(int n) => n == 1 ? "1 piece" : $"{n} pieces";

    public static string FirewoodText(int n) => n == 1 ? "1 Firewood" : $"{n} Firewood";

    // "12 Smoked Meat and 6 Smoked Fish", "4 Smoked Meat".
    public static string SmokedText(int meat, int fish)
    {
        if (meat > 0 && fish > 0)
            return $"{meat} Smoked Meat and {fish} Smoked Fish";
        return fish > 0 ? $"{fish} Smoked Fish" : $"{meat} Smoked Meat";
    }

    // "4 Smoked Meat, 1 Buckskin and 2 Tanned Furs": everything smoked waiting, meat and hides.
    public static string SmokedGoodsText(int meat, int fish, int buckskin, int tannedFurs)
    {
        var parts = new System.Collections.Generic.List<string>();
        if (meat > 0) parts.Add($"{meat} Smoked Meat");
        if (fish > 0) parts.Add($"{fish} Smoked Fish");
        if (buckskin > 0) parts.Add($"{buckskin} Buckskin");
        if (tannedFurs > 0) parts.Add($"{tannedFurs} Tanned Furs");
        if (parts.Count == 0) return "nothing";
        if (parts.Count == 1) return parts[0];
        return string.Join(", ", parts.GetRange(0, parts.Count - 1)) + " and " + parts[parts.Count - 1];
    }

    public static string CostText => $"{SiteStone} Stone, {SiteClay} Clay, {SiteLogs} Logs and {SiteLumber} Lumber";

    public static string BuiltMessage =>
        $"Smokehouse built. Put the Hammer away, press R to load raw meat or fish (up to {MeatCap}) and Firewood, then E to light it; a load smokes for {SmokeHours} hours and burns {FirewoodPerLoad} Firewood.";

    // E's line, by phase. smokedMeat, smokedFish: what waits to be collected; yieldNow: what the load in it makes.
    public static string Prompt(Phase phase, int raw, int firewood, int yieldNow, int smokedMeat, int smokedFish, float hoursLeft, int fuelHours)
    {
        switch (phase)
        {
            case Phase.Empty: return "Smokehouse  (empty - load raw meat or fish and Firewood with R)";
            case Phase.Loading:
                return raw > 0 ? $"Smokehouse  ({PiecesText(raw)} of meat or fish - needs Firewood, R)"
                               : $"Smokehouse  ({FirewoodText(firewood)} - needs raw meat or fish, R)";
            case Phase.Ready:
            {
                string wood = firewood < FirewoodPerLoad ? $"; {FirewoodText(firewood)}, add {FirewoodPerLoad - firewood} more or it will go out" : $"; burns {FirewoodPerLoad} of the {firewood} Firewood";
                return $"Light the fire  ({PiecesText(raw)} -> {yieldNow} smoked in {SmokeHours} h{wood})";
            }
            case Phase.Smoking:
            {
                int need = FirewoodStillNeeded(hoursLeft, fuelHours);
                string wood = firewood < need ? $"; {FirewoodText(firewood)} left, add {need - firewood} more" : $"; {FirewoodText(firewood)} left";
                return $"Smokehouse  (smoking {PiecesText(raw)}, {HoursText(hoursLeft)} to go{wood})";
            }
            case Phase.Paused:
                return $"Smokehouse  (out of Firewood - smoking stopped with {HoursText(hoursLeft)} to go; load Firewood with R)";
            default: return $"Collect smoked food  ({SmokedText(smokedMeat, smokedFish)})";
        }
    }

    // R's line without the Hammer.
    public static string LoadPrompt(LoadPlan plan, bool smoking, int rawCarried, int firewoodCarried)
    {
        if (!plan.Any)
        {
            if (rawCarried <= 0 && firewoodCarried <= 0)
                return "Load raw meat, fish or Firewood  (you're not carrying any)";
            if (smoking && rawCarried > 0 && firewoodCarried <= 0)
                return "Can't add meat while it smokes  (wait for this load to finish)";
            return "Can't load  (the smokehouse is full)";
        }
        string what = LoadText(plan.raw, plan.firewood);
        if (plan.raw <= 0 && smoking && rawCarried > 0)
            return $"Load {what}  (meat waits until this load is done)";
        return $"Load {what}";
    }

    static string LoadText(int raw, int firewood)
    {
        if (raw > 0 && firewood > 0)
            return $"{PiecesText(raw)} of meat or fish and {FirewoodText(firewood)}";
        return raw > 0 ? $"{PiecesText(raw)} of meat or fish" : FirewoodText(firewood);
    }

    // What R flashes after loading.
    public static string Loaded(LoadPlan plan, int rawNow, int firewoodNow) =>
        $"Loaded {LoadText(plan.raw, plan.firewood)}  (the smokehouse holds {PiecesText(rawNow)} and {FirewoodText(firewoodNow)}).";

    // What R flashes when it can load nothing.
    public static string LoadRefusal(bool smoking, int rawCarried, int firewoodCarried)
    {
        if (rawCarried <= 0 && firewoodCarried <= 0)
            return "You're not carrying any raw meat, fish or Firewood. Load them with R, then light the fire with E.";
        if (smoking && firewoodCarried <= 0)
            return "Meat can't go in while a load smokes. Wait for it to finish, then collect it with E.";
        return $"The smokehouse is full ({MeatCap} pieces of meat and {FirewoodCap} Firewood at most).";
    }

    // What E says when it is not ready to light.
    public static string NotReady(int raw, int firewood) =>
        raw <= 0 ? "Load raw meat or fish first (R)." : "Load Firewood first (R): a load burns " + FirewoodPerLoad + " over the day.";

    public static string Lit(int raw, int made, int firewood)
    {
        string note = firewood < FirewoodPerLoad ? $" It has {FirewoodText(firewood)}; add {FirewoodPerLoad - firewood} more before it runs out or the smoking stops." : "";
        return $"The fire is lit: {PiecesText(raw)} -> {made} smoked in {SmokeHours} h.{note}";
    }

    public static string Took(int took, int stayed)
    {
        if (took <= 0)
            return "Too heavy - no room to carry it";
        string text = took == 1 ? "Took 1 piece of smoked food" : $"Took {took} pieces of smoked food";
        return stayed > 0 ? text + $" ({stayed} left in the smokehouse, no room)." : text + ".";
    }

    // R with the Hammer in hand.
    public static string DismantlePrompt(Phase phase, float hoursLeft) =>
        phase == Phase.Smoking || phase == Phase.Paused
            ? $"Can't take down the smokehouse while a load is in  ({HoursText(hoursLeft)} to go)"
            : "Take down the smokehouse  (half the materials back, rounded down; the meat, Firewood and smoked food in it come back whole)";
}
