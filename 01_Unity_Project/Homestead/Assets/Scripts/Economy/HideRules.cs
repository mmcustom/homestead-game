using System;

// Raw hides spoil, and salting them (Tanning_Hides_and_Furs.md section 2, slice T1). Mike, 2026-11-23: "Raw hides spoil, slowly", built with the
// proposed numbers ("yes, commit, then proceed").
//
// A Deer Hide and Small Furs now have a shelf life on the outdoor freshness clock (the item assets' shelfLifeDays: 7 and 5 days), so they age
// faster in heat and slower in the cold like food, and turn to Rot when they spoil. With Salt in the pack (no Crock Pot needed: the salt is
// rubbed into the flesh side), a raw hide row in the Inventory gets a Salt button: a Deer Hide takes 2 Salt and becomes a Salted Deer Hide,
// Small Furs take 1 and become Salted Furs. Salted hides keep for about a year and sell for what the raw ones do. Hides carried in a save from
// before this slice are stamped half-fresh when it loads (SpoilageManager). Pure rules; Salting does the work on the pack.
public static class HideRules
{
    public const string DeerHideId = "deer_hide";
    public const string SmallFursId = "small_furs";
    public const string SaltedDeerHideId = "salted_deer_hide";
    public const string SaltedFursId = "salted_furs";
    public const string SaltId = SaltRules.SaltId;

    // Shelf lives in days at a mild 10 to 20 C (the item assets carry them; kept here so tests can check the assets).
    public const float DeerHideShelfDays = 7f;
    public const float SmallFursShelfDays = 5f;
    public const float SaltedShelfDays = 360f;

    // Salt each raw hide takes.
    public const int DeerHideSalt = 2;
    public const int SmallFursSalt = 1;

    // Trading Post prices (cents): salted hides sell for what the raw ones do.
    public const long SaltedDeerHideCents = 800;
    public const long SaltedFursCents = 300;

    public static readonly string[] RawIds = { DeerHideId, SmallFursId };

    public static bool IsRawHide(string itemId) => itemId == DeerHideId || itemId == SmallFursId;

    // What a raw hide salts into, and the Salt it takes. False for anything else.
    public static bool TryFind(string itemId, out string saltedId, out int salt)
    {
        switch (itemId)
        {
            case DeerHideId: saltedId = SaltedDeerHideId; salt = DeerHideSalt; return true;
            case SmallFursId: saltedId = SaltedFursId; salt = SmallFursSalt; return true;
            default: saltedId = null; salt = 0; return false;
        }
    }

    public static bool Salts(string itemId) => TryFind(itemId, out _, out _);

    public static int SaltFor(string itemId) => TryFind(itemId, out _, out int salt) ? salt : 0;

    // How many raw hides can be salted with the Salt carried (and no more than are carried).
    public static int PiecesFor(string itemId, int pieces, int salt)
    {
        int each = SaltFor(itemId);
        return each <= 0 ? 0 : Math.Max(0, Math.Min(pieces, salt / each));
    }

    // The age an old save's hides are given when this slice first loads: half their life used.
    public static double HalfFreshAgeHours(double shelfLifeDays) => Math.Max(0.0, shelfLifeDays) * 24.0 * 0.5;

    // --- Text ---

    // The row's note: "2 Salt each, keeps about a year; 4 carried", or that there is no Salt.
    public static string RowInfo(string itemId, int salt)
    {
        int each = SaltFor(itemId);
        string cost = (each == 1 ? "1 Salt each" : $"{each} Salt each") + ", keeps about a year";
        return salt <= 0 ? $"{cost}; you have no Salt" : $"{cost}; {salt} carried";
    }

    public static string NeedsSalt(string hideName, int each) => each == 1 ? $"Salting {hideName} needs 1 Salt" : $"Salting {hideName} needs {each} Salt";

    // The flash after salting: "Salted 1 Deer Hide (2 Salt): it keeps about a year now".
    public static string SaltedMessage(int count, string hideName, int saltUsed) =>
        $"Salted {count} {hideName} ({saltUsed} Salt): it keeps about a year now";
}
