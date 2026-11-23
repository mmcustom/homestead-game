using System;

// Salt curing (Food_Spoilage_and_Preservation.md section 4, slice 4). Mike, 2026-11-22: the crock is used from the pack, like boiling sap ("Use it
// from the pack"), and the crock is the Crock Pot already shaped at the Potter's Shed and fired in the Kiln ("Use the Water Crock"; renamed "Crock Pot" by Mike the same day), so the
// Potter screen keeps its five large rows. Later the same day: "make salted meat and dried meat more profitable than just cooked meat and even more
// than fresh raw meat", so a piece no longer salts one for one: it gives one Salt Meat or Salt Fish for every dollar it is worth cooked (a Venison
// gives 4, a Bass 3, a Bluegill 2; DryingRules.CookedCents), each selling for $2.50.
//
// Carrying a Crock Pot, a raw meat or fish row in the Inventory gets a Salt button (when no fire is in reach to cook it). Each raw piece takes Salt
// by its weight, 1 Salt for every 2 kg (at least 1, so a Venison takes 2), and is cut and salted at once. The crock is not used up. Salt food keeps
// for 180 days and is eaten as it is. Salt is bought at the Trading Post. Pure rules; Salting does the work on the pack.
public static class SaltRules
{
    public const string SaltId = "salt";
    public const string SaltMeatId = "salt_meat";
    public const string SaltFishId = "salt_fish";
    public const string CrockId = PotteryRules.FiredCrockId; // the Crock Pot

    public const double KgPerSalt = 2.0;

    // Salt pieces for every dollar the cooked piece is worth.
    public const int PiecesPerCookedDollar = 1;

    // Trading Post prices (proposed, cents): Salt is sold in 1 kg bags; the salt food is bought.
    public const long SaltCents = 200;
    public const long SaltMeatCents = 250;
    public const long SaltFishCents = 250;

    // What a raw piece salts into. False for anything that is not raw meat or fish.
    public static bool TryFind(string itemId, out string saltedId)
    {
        switch (itemId)
        {
            case "venison":
            case "turkey_meat":
            case "small_game_meat":
            case "waterfowl_meat":
            case "chicken_meat": saltedId = SaltMeatId; return true;
            case "bass":
            case "bluegill":
            case "catfish":
            case "crappie": saltedId = SaltFishId; return true;
            default: saltedId = null; return false;
        }
    }

    public static bool Salts(string itemId) => TryFind(itemId, out _);

    // Salt for one raw piece: 1 for every 2 kg, rounded up, at least 1.
    public static int SaltFor(double pieceKg) => Math.Max(1, (int)Math.Ceiling(pieceKg / KgPerSalt - 1e-9));

    // Salt pieces one raw piece gives: one per dollar of its cooked price, at least one.
    public static int YieldOf(string rawId) => Salts(rawId) ? Math.Max(1, DryingRules.CookedCents(rawId) * PiecesPerCookedDollar / 100) : 0;

    // How many raw pieces can be salted with the Salt carried (and no more than are carried).
    public static int PiecesFor(int pieces, int salt, double pieceKg) => Math.Max(0, Math.Min(pieces, salt / SaltFor(pieceKg)));

    // The row's note: "1 Salt each, makes 2; 4 carried", or that there is no Salt.
    public static string RowInfo(string rawId, int salt, double pieceKg)
    {
        int each = SaltFor(pieceKg), makes = YieldOf(rawId);
        string cost = (each == 1 ? "1 Salt each" : $"{each} Salt each") + $", makes {makes}";
        return salt <= 0 ? $"{cost}; you have no Salt" : $"{cost}; {salt} carried";
    }

    // The flash after salting: "Salted 1 Venison into 4 Salt Meat (2 Salt)".
    public static string SaltedMessage(int raw, string rawName, int made, string saltedName, int saltUsed) =>
        $"Salted {raw} {rawName} into {made} {saltedName} ({saltUsed} Salt)";
}
