// The calendar gate on saves (real-calendar Phase 1), pure and engine-free so edit-mode tests cover it.
//
// Every save written from now on carries calendarVersion (CalendarRules.SaveVersion) in meta.json and slotinfo.json. A save without it (or with another number) was made on the OLD calendar,
// where a season was 28 days and a day count meant something else: it is never loaded, because every date, loan due day, ledger period and regrowth stamp in it would be read wrongly. It is
// not changed or deleted either: the slot list labels it "old calendar", Continue skips it, and saving over it asks first.
public static class CalendarSaveRules
{
    // What the slot list shows in place of the date for a save from the old calendar.
    public const string LegacyLabel = "old calendar";

    // A save made before the field existed reads 0, which is the old calendar.
    public static bool IsCurrent(int calendarVersion) => calendarVersion == CalendarRules.SaveVersion;

    public static bool IsLegacy(int calendarVersion) => !IsCurrent(calendarVersion);

    // Only a slot that holds a save can be a legacy one.
    public static bool IsLegacySlot(bool exists, int calendarVersion) => exists && IsLegacy(calendarVersion);

    // What the slot's date column shows: the label for an old-calendar save, otherwise the slot's own text.
    public static string DateColumn(bool exists, int calendarVersion, string inGame) =>
        IsLegacySlot(exists, calendarVersion) ? LegacyLabel : inGame ?? "";

    // The slot list's Load button, and Continue, only work on a save from this calendar.
    public static bool CanLoad(bool exists, int calendarVersion) => exists && IsCurrent(calendarVersion);

    // Saving, copying or starting a new game over an old-calendar save always asks, even in the current slot (which is otherwise one click).
    public static bool NeedsOverwriteWarning(bool exists, int calendarVersion) => IsLegacySlot(exists, calendarVersion);

    public static string RefusalMessage(string slotName) =>
        $"{slotName} was saved on the old calendar and can't be loaded in this version.";

    public static string OverwriteWarning(string slotName, string inGame)
    {
        string details = string.IsNullOrEmpty(inGame) ? "" : $" ({inGame})";
        return $"{slotName}{details} was saved on the old calendar and can't be loaded in this version.\nOverwrite it anyway? A copy is kept in the Saves folder under _old_calendar.";
    }

    // Where a copy of an old-calendar slot is kept before the first thing writes over it (an autosave has no question to ask): Saves/_old_calendar/<slot>_<stamp>.
    public const string PreservedFolder = "_old_calendar";
}
