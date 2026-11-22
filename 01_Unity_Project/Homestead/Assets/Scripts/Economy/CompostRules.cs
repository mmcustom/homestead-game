using System;

// The Compost Bin (Food_Spoilage_and_Preservation.md section 2, Mike 2026-11-22: "spoiled food becomes rot, which can be put in a compost
// container with straw or tall grass to make fertilizer for future farming and crops"). Pure rules; SpoilageManager runs the bins each hour.
//
// The bin holds Rot and Tall Grass (straw, once it exists). Whenever it is idle and holds at least one batch's worth, a job starts: every
// whole batch it can make (4 Rot and 2 Tall Grass each) is taken out and composts together. It takes 7 days at mild to warm temperatures,
// slower in the cold and not at all while frozen; then each batch becomes 1 Fertilizer in the bin. What is added meanwhile waits for the next job.
public static class CompostRules
{
    public const string RotId = SpoilageRules.RotId;
    public const string GrassId = "tall_grass";
    public const string FertilizerId = "fertilizer";
    public const string Marker = "compost";       // WoodPileState.jobItem while a job runs (jobCount batches, jobHours left on the compost clock)

    public const int RotPerBatch = 4;
    public const int GrassPerBatch = 2;
    public const int MaxBatchesPerJob = 10;
    public const double HoursPerJob = 7 * 24; // Mike 2026-11-22: "maybe down to a week" (was 14 days)

    // The Trading Post buys Fertilizer until farming exists (proposed $0.50).
    public const long FertilizerBaseCents = 50;

    public static bool Accepts(string itemId) => itemId == RotId || itemId == GrassId;

    public static int Batches(int rot, int grass) =>
        Math.Max(0, Math.Min(MaxBatchesPerJob, Math.Min(rot / RotPerBatch, grass / GrassPerBatch)));

    // Compost hours per game hour: nothing at or below freezing, slowly in the cold, full speed from 15 to 32 C, a little slower in great heat.
    public static double RateAt(double tempC)
    {
        if (double.IsNaN(tempC) || tempC <= 0)
            return 0;
        if (tempC <= 5)
            return 0.4 * tempC / 5.0;
        if (tempC <= 15)
            return 0.4 + 0.6 * (tempC - 5) / 10.0;
        if (tempC <= 32)
            return 1.0;
        return 0.8;
    }

    // "Composting 3 batches: about 9 days left" (at today's rate), "stopped: frozen" when it is not working.
    public static string Status(int batches, double hoursLeft, double rate)
    {
        if (batches <= 0)
            return null;
        string what = batches == 1 ? "1 batch" : $"{batches} batches";
        if (rate <= 0)
            return $"composting {what}, stopped while frozen";
        double days = Math.Max(0, hoursLeft) / rate / 24.0;
        string left = days >= 1.5 ? $"about {(int)Math.Round(days)} days left" : days >= 1 ? "about 1 day left" : "less than a day left";
        return $"composting {what}, {left}";
    }

    // An idle bin (Mike 2026-11-22: "nothing shows or says anything about fertilizer"): what it still needs before a batch can start,
    // or that one starts within the hour.
    public static string IdleStatus(int rot, int grass)
    {
        if (Batches(rot, grass) > 0)
            return "starts composting within the hour";
        int needRot = Math.Max(0, RotPerBatch - rot), needGrass = Math.Max(0, GrassPerBatch - grass);
        string rotText = needRot > 0 ? $"{needRot} more Rot" : null;
        string grassText = needGrass > 0 ? $"{needGrass} more Tall Grass" : null;
        string need = rotText != null && grassText != null ? $"{rotText} and {grassText}" : rotText ?? grassText;
        return $"needs {need} to start";
    }
}
