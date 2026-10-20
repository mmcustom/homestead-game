using System;
using System.Collections.Generic;

// The save/load screen's rules, kept free of Unity and of the UI so edit-mode tests can cover them (it lives beside SlotSwap, in
// the same engine-free assembly). The "current slot" is the last manual slot the player saved to or loaded from; each slot's
// slotinfo.json records it as lastSlot, and the Autosave carries it forward every time it is written. A slot id is "slot1" to
// "slot10" or SlotSwap.AutosaveId; null or "" means none.
public static class SlotRules
{
    static bool IsManual(string id) => !string.IsNullOrEmpty(id) && id != SlotSwap.AutosaveId && SlotSwap.IsValidSlot(id);

    // A stored lastSlot counts only when it names a manual slot that still holds a save; anything else (empty, missing file,
    // junk, the Autosave) is none.
    public static string ResolveCurrent(string lastSlot, Func<string, bool> slotExists) =>
        IsManual(lastSlot) && slotExists != null && slotExists(lastSlot) ? lastSlot : null;

    // The lastSlot to write into a slot's record as it is saved: a manual save names itself; the Autosave carries the current
    // slot forward (none stays none).
    public static string LastSlotToWrite(string writingSlot, string currentSlot)
    {
        if (IsManual(writingSlot))
            return writingSlot;
        return IsManual(currentSlot) ? currentSlot : null;
    }

    // The current slot after loading loadedSlot, whose record says loadedLastSlot (empty on a save from before this existed).
    // Loading a manual slot makes it current; loading the Autosave takes the slot it carried, if that still holds a save.
    public static string CurrentAfterLoad(string loadedSlot, string loadedLastSlot, Func<string, bool> slotExists) =>
        IsManual(loadedSlot) ? loadedSlot : ResolveCurrent(loadedLastSlot, slotExists);

    // Saving over a filled slot other than the current one asks first; the current slot and an empty slot are one click.
    public static bool NeedsOverwriteConfirm(bool targetExists, string targetSlot, string currentSlot) =>
        targetExists && !(IsManual(currentSlot) && targetSlot == currentSlot);

    // Which manual slot (1-based index into manualExists) the screen pre-selects: the current slot, else the first empty one,
    // else none (-1). The current slot is a valid pick only if it holds a save.
    public static int PreSelect(string currentSlot, IList<bool> manualExists)
    {
        for (int i = 0; i < manualExists.Count; i++)
            if (manualExists[i] && IsManual(currentSlot) && currentSlot == SlotId(i + 1))
                return i + 1;
        for (int i = 0; i < manualExists.Count; i++)
            if (!manualExists[i])
                return i + 1;
        return -1;
    }

    // With no manual slot current, the running game is the Autosave's.
    public static bool AutosaveIsCurrent(bool inGame, string currentSlot) => inGame && !IsManual(currentSlot);

    static string SlotId(int number) => "slot" + number;
}
