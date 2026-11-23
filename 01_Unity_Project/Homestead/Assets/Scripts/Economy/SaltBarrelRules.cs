using System;

// The Salt Barrel (Saw_Pit_Sawmill_and_Salt_Barrel.md section 3). Mike, 2026-11-23: "we need a barrel or something to store any salt surplus",
// holding "Salt and dry goods", built as proposed ("Use the proposals").
//
// A lidded wooden barrel built in one go with the Hammer, like the Storage Bin: 3 Logs and 2 Cordage. It takes the same dry goods the Storage Bin
// does (Salt, Cordage, Sinew and the like: no Tools, wood, Stone, water or food), up to CapacityKg by weight. Pure and engine-free so edit-mode
// tests cover it.
public static class SaltBarrelRules
{
    public const string LogsId = "logs";
    public const string CordageId = "cordage";

    // --- What building one takes ---

    public const int Logs = 3;
    public const int Cordage = 2;

    // --- What it holds ---

    public const float CapacityKg = 100f;

    // Whole units of an item weighing unitKg that still fit with loadKg already stored (a tiny tolerance keeps an exact fit).
    public static int UnitsThatFit(float loadKg, float unitKg)
    {
        float room = CapacityKg - Math.Max(0f, loadKg);
        if (room <= 0.0001f)
            return 0;
        if (unitKg <= 0f)
            return int.MaxValue;
        return (int)Math.Floor((room + 0.0001f) / unitKg);
    }

    public static string KgText(float loadKg) => $"{loadKg:0} of {CapacityKg:0} kg";

    // --- Text ---

    public const string Holds = "A Salt Barrel holds Salt and other dry goods (Cordage, Sinew and the like), up to 100 kg. Not Tools, wood, Stone, food or water.";
    public const string Help = "a lidded barrel for Salt and other dry goods (Cordage, Sinew and the like), up to 100 kg. Store them (R), take them back (E).";
    public static string FullReason(float loadKg) => $"the barrel is full ({KgText(loadKg)})";
}
