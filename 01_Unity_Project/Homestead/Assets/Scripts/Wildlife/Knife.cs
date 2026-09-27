// The Knife (Hunting_System.md, 2026-09-26): a stone blade lashed to a stick handle, crafted from the Inventory, and in
// the new-game starting kit. Field dressing a kill (Carcass) and taking small game out of a snare or box trap
// (TrapManager) need one in the pack — it doesn't have to be equipped. Fish, whole from the line or a fish trap, don't.
public static class Knife
{
    public const string Id = "knife";

    public static bool Carried => InventoryManager.Instance != null && InventoryManager.Instance.Player.Has(Id);
}
