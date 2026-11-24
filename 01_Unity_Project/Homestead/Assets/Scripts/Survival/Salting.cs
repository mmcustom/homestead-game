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

    // Salts up to count raw hides (HideRules, Tanning_Hides_and_Furs.md section 2): no Crock Pot needed, just the Salt. Returns how many were salted.
    public static int SaltHides(ItemDefinition item, int count)
    {
        InventoryManager inventory = InventoryManager.Instance;
        if (item == null || inventory == null || count <= 0 || !HideRules.TryFind(item.Id, out string saltedId, out int each))
            return 0;
        InventoryContainer pack = inventory.Player;
        int can = HideRules.PiecesFor(item.Id, Mathf.Min(count, pack.Count(item.Id)), pack.Count(HideRules.SaltId));
        if (can <= 0)
        {
            ToolStatus.Flash(HideRules.NeedsSalt(item.DisplayName, each));
            return 0;
        }

        if (AudioManager.Instance != null)
            AudioManager.Instance.SilenceNextRemoval();
        // The raw hides and Salt go first, so the salted hides have room where they were (a salted hide weighs what the raw one did).
        int removed = pack.Remove(item.Id, can);
        pack.Remove(HideRules.SaltId, removed * each);
        int made = inventory.AddToPlayer(saltedId, removed, SpoilageManager.FreshStamp());
        if (made < removed)
        {
            inventory.AddToPlayer(item.Id, removed - made, SpoilageManager.FreshStamp());
            inventory.AddToPlayer(HideRules.SaltId, (removed - made) * each);
        }
        if (made > 0)
            ToolStatus.Flash(HideRules.SaltedMessage(made, item.DisplayName, made * each));
        return made;
    }

    // Tanning slice T4: strips the bark from up to count Logs with a Bark Spud carried (TanningVatRules). Each Log gives 2 Tanbark and becomes a Peeled Log,
    // which counts as a Log wherever Logs are used from the pack. Peeled Logs are not stripped again. Returns how many Logs were stripped.
    public static bool SpudCarried =>
        InventoryManager.Instance != null && InventoryManager.Instance.Player.Count(TanningVatRules.BarkSpudId) > 0;

    public static int StripBark(int count)
    {
        InventoryManager inventory = InventoryManager.Instance;
        if (inventory == null || count <= 0)
            return 0;
        if (!SpudCarried)
        {
            ToolStatus.Flash(TanningVatRules.NeedsSpud);
            return 0;
        }
        InventoryContainer pack = inventory.Player;
        if (AudioManager.Instance != null)
            AudioManager.Instance.SilenceNextRemoval();
        // Only plain Logs: the batches are counted by the plain id so a Peeled Log is never stripped twice.
        int plain = 0;
        foreach ((int day, int quantity) in pack.Batches(TanningVatRules.LogsId))
            plain += quantity;
        int logs = pack.RemoveBatches(TanningVatRules.LogsId, Mathf.Min(count, plain), null);
        if (logs <= 0)
            return 0;
        inventory.AddToPlayer(TanningVatRules.PeeledLogId, logs);
        inventory.AddToPlayer(TanningVatRules.TanbarkId, logs * TanningVatRules.TanbarkPerLog);
        ToolStatus.Flash(TanningVatRules.Stripped(logs));
        return logs;
    }
}
