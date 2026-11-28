using System;

// Slice P2, the Drilled Well (Modern_Systems_and_Services.md; Mike's answers 2026-11-27). Pure and engine-free so edit-mode tests cover it.
//
// The player marks out a Drilled Well Site (free; a well's setbacks, but no depth limit: a rig goes where a shovel can't) and hires the driller by phone.
// The quote is by depth at the spot: the deeper the water table under it (WellWater's depth to water), the deeper the drilled well, at real Ohio prices
// ($25 to $55 a foot; here $30 a foot plus $1,000 for the casing cap, the pump and the permit). Half is paid at booking; the rig comes LeadDays later and
// drills for DrillDays; the other half is paid when it is done (as soon as there is the cash). The finished well is a cased well with a cast-iron hand pump:
// excellent water, safe to drink, a big supply (BufferLitres) that refills fast, drought or not, and that flood water and a privy can't foul.
public static class DrillRules
{
    // --- The depth and the price ---

    public const int MinFeet = 80, MaxFeet = 250;
    public const float BaseFeet = 70f, FeetPerMetreToWater = 15f;
    public const long PerFootCents = 3000, FixedCents = 100000;

    // How deep the drilled well goes, from the water table's depth under the spot (metres; 0 when it isn't known: an average 140 ft well).
    public static int Feet(float depthToWaterMetres)
    {
        if (depthToWaterMetres <= 0f)
            return 140;
        return Math.Max(MinFeet, Math.Min(MaxFeet, (int)Math.Round(BaseFeet + FeetPerMetreToWater * depthToWaterMetres)));
    }

    public static long JobCents(float depthToWaterMetres) => FixedCents + Feet(depthToWaterMetres) * PerFootCents;
    public static long Deposit(long jobCents) => jobCents / 2;
    public static long Balance(long jobCents) => jobCents - Deposit(jobCents);

    // --- The crew ---

    public const int LeadDays = 3, DrillDays = 2;
    public const string CrewMarker = "well_crew";

    public static float Progress(int bookedDay, double nowHours)
    {
        double start = (bookedDay + LeadDays) * 24.0, end = start + DrillDays * 24.0;
        return nowHours <= start ? 0f : nowHours >= end ? 1f : (float)((nowHours - start) / (end - start));
    }

    // --- The water ---

    public const int BufferLitres = 1000;
    public const float RecoveryLitresPerHour = 40f;

    // --- Words ---

    public const string SiteName = "Drilled Well Site", Name = "Drilled Well";
    public const string NoSite = "Mark out a Drilled Well Site first (Build tab, Other) - the driller comes to it";
    public static string Quote(float depth) => $"{Money.Format(JobCents(depth))} for about {Feet(depth)} ft, half now";
    public static string Booked(long jobCents) => $"Driller booked - {Money.Format(Deposit(jobCents))} paid; the rig comes in {LeadDays} days and drills for {DrillDays}";
    public static string Waiting(int daysToStart) => daysToStart > 0 ? $"The rig comes in {daysToStart} day{(daysToStart == 1 ? "" : "s")}" : "The rig is drilling";
    public static string Done(long balance) => $"Your Drilled Well is finished - the {Money.Format(balance)} balance is paid. Pump it with E";
    public static string Owed(long balance) => $"Your Drilled Well is drilled - the {Money.Format(balance)} balance is due; it's paid as soon as you have it";
    public const string SitePrompt = "Drilled Well Site  (hire the driller by phone: P, Contractors)";
    public const string CrewHasIt = "The driller has this site - the rig will finish it";

    public static string SiteHelp(float depth) =>
        $"a cased well with a cast-iron hand pump: up to {BufferLitres} L of excellent, safe water that refills about {RecoveryLitresPerHour:0} L an hour, drought or not. " +
        $"Mark out the site (free; a well's distances from the outhouse and water, but no depth limit), then hire the driller by phone (P, Contractors): " +
        $"about $3,800 on low ground near water to about $8,500 on high ground, by the depth where you stake it. The rig comes {LeadDays} days after booking and drills for {DrillDays}.";
}
