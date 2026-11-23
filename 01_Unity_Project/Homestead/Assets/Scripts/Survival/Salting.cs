using UnityEngine;

// Salt curing from the pack (SaltRules; Food_Spoilage_and_Preservation.md section 4). The Inventory's Salt button calls Salt: with a Crock Pot
// carried, each raw piece takes its Salt and is cut into Salt Meat or Salt Fish straight away (one per dollar it is worth cooked). The oldest
// raw pieces go first.
public static class Salting
{
    public static bool CrockCarried =>
        InventoryManager.Instance != null && InventoryManager.Instance.Player.Count(SaltRules.CrockId) > 0;

    // Salts up to count raw pieces of a meat or fish. Returns how many raw pieces were salted.
    public static int Salt(ItemDefinition item, int count)
    {
        InventoryManager inventory = InventoryManager.Instance;
        if (item == null || inventory == null || count <= 0 || !SaltRules.TryFind(item.Id, out string saltedId))
            return 0;
        if (!CrockCarried)
        {
            ToolStatus.Flash("Salting needs a Crock Pot in your pack");
            return 0;
        }
        InventoryContainer pack = inventory.Player;
        int each = SaltRules.SaltFor(item.WeightKg), per = SaltRules.YieldOf(item.Id);
        int can = SaltRules.PiecesFor(Mathf.Min(count, pack.Count(item.Id)), pack.Count(SaltRules.SaltId), item.WeightKg);
        if (can <= 0)
        {
            ToolStatus.Flash(each == 1 ? $"Salting {item.DisplayName} needs 1 Salt" : $"Salting {item.DisplayName} needs {each} Salt");
            return 0;
        }

        if (AudioManager.Instance != null)
            AudioManager.Instance.SilenceNextRemoval();
        // The raw meat and Salt go first, so the lighter salt food has room where they were.
        int removed = pack.Remove(item.Id, can);
        pack.Remove(SaltRules.SaltId, removed * each);
        int made = inventory.AddToPlayer(saltedId, removed * per, SpoilageManager.FreshStamp());
        int done = made / per;
        if (done < removed)
        {
            // Not enough room after all: the part-piece's salt food comes back out, and the raw pieces and Salt not salted go back in.
            if (made > done * per)
                pack.Remove(saltedId, made - done * per);
            inventory.AddToPlayer(item.Id, removed - done, SpoilageManager.FreshStamp());
            inventory.AddToPlayer(SaltRules.SaltId, (removed - done) * each);
        }
        if (done > 0)
            ToolStatus.Flash(SaltRules.SaltedMessage(done, item.DisplayName, done * per, ItemDatabase.Get(saltedId)?.DisplayName ?? saltedId, done * each));
        return done;
    }
}
