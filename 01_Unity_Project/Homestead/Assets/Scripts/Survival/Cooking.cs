using UnityEngine;

// Cooking and boiling at a lit campfire (the Cooking and Water Purification entries, 2026-09-25). Standing within
// reach of a burning campfire, the Inventory screen offers Cook on raw meat and fish — each becomes its cooked
// Consumable, safe to eat, more filling and keeping longer — and Boil on collected raw water, which becomes Purified
// Water. Boiling needs a pot to boil it in: the metal Cooking Pot (Water_System.md's Boiling correction, 2026-09-26 —
// the wooden Bucket only carries water; it would burn over the fire).
// Each piece takes a few seconds (CampfireCooking runs the queue and calls Cook as each one finishes); it doesn't burn
// extra fuel.
public static class Cooking
{
    public const string PurifiedWaterId = "water";
    public const string PotId = "cooking_pot";

    // How close to a lit campfire the player must be to cook (metres).
    public const float Range = 3f;

    // Raw item → cooked item.
    static readonly string[,] Recipes =
    {
        { "venison", "cooked_venison" },
        { "turkey_meat", "cooked_turkey" },
        { "waterfowl_meat", "cooked_waterfowl" },
        { "chicken_meat", "cooked_chicken" },
        { "small_game_meat", "cooked_small_game" },
        { "bluegill", "cooked_bluegill" },
        { "crappie", "cooked_crappie" },
        { "bass", "cooked_bass" },
        { "catfish", "cooked_catfish" },
    };

    // The cooked version of a raw item, or null if it isn't cooked.
    public static string CookedId(string rawId)
    {
        for (int i = 0; i < Recipes.GetLength(0); i++)
            if (Recipes[i, 0] == rawId)
                return Recipes[i, 1];
        return null;
    }

    public static bool IsCookable(string itemId) => CookedId(itemId) != null;
    public static bool IsBoilable(string itemId) => WaterQualities.IsRawWater(itemId);

    // Maple Sap boils down to Maple Syrup in the Cooking Pot (Maple C): a batch of 10 L at a time, not litre by litre.
    public static bool IsSapBoil(string itemId) => itemId == MapleRules.SapId;

    // The lit campfire the player can cook at, or null.
    public static CampfireState FireInReach(PlayerController player)
    {
        if (player == null || FireManager.Instance == null)
            return null;
        return FireManager.Instance.NearestLit(player.transform.position, Range);
    }

    public static bool HasPot =>
        InventoryManager.Instance != null && InventoryManager.Instance.Player.Count(PotId) > 0;

    // Turns up to count of an item into its cooked (or boiled) version straight away. Returns how many were done.
    public static int Cook(ItemDefinition raw, int count, PlayerController player)
    {
        InventoryManager inventory = InventoryManager.Instance;
        if (inventory == null || raw == null || count <= 0 || FireInReach(player) == null)
            return 0;
        if (IsSapBoil(raw.Id))
            return BoilSap(count, inventory);

        bool boil = IsBoilable(raw.Id);
        string outputId = boil ? PurifiedWaterId : CookedId(raw.Id);
        ItemDefinition output = outputId != null ? ItemDatabase.Get(outputId) : null;
        if (output == null)
            return 0;
        if (boil && !HasPot)
        {
            ToolStatus.Flash("Boiling water needs a Cooking Pot — the wooden Bucket would burn");
            return 0;
        }

        // Swapping raw for cooked isn't dropping anything; the cooked item arriving plays the pickup sound.
        if (AudioManager.Instance != null)
            AudioManager.Instance.SilenceNextRemoval();
        int done = inventory.RemoveFromPlayer(raw.Id, count);
        if (done <= 0)
            return 0;
        inventory.AddToPlayer(outputId, done); // cooked food is dated today: its shelf life starts now

        string what = done > 1 ? $"{done} × {raw.DisplayName}" : raw.DisplayName;
        ToolStatus.Flash(boil ? $"Boiled {done} L of {raw.DisplayName} — now {output.DisplayName}" : $"Cooked {what}");
        return done;
    }

    // Boils batches of sap into Syrup straight away (the queue calls it once a batch has run its 12 s). Returns the Syrup made.
    static int BoilSap(int batches, InventoryManager inventory)
    {
        if (!HasPot)
        {
            ToolStatus.Flash("Boiling sap needs a Cooking Pot — the wooden Bucket would burn");
            return 0;
        }

        int carried = inventory.Player.Count(MapleRules.SapId);
        if (MapleRules.SyrupFrom(carried) <= 0)
        {
            ToolStatus.Flash("Boiling " + MapleRules.NeedMessage(carried));
            return 0;
        }

        if (AudioManager.Instance != null)
            AudioManager.Instance.SilenceNextRemoval();
        int made = BoilSapInto(inventory.Player, batches, TimeManager.Instance != null ? TimeManager.Instance.TotalDays : 0);
        if (made <= 0)
        {
            ToolStatus.Flash("No room to carry the Syrup — the sap is kept");
            return 0;
        }

        ToolStatus.Flash(MapleRules.BoiledMessage(made, made * MapleRules.SapPerSyrup));
        return made;
    }

    // The swap itself: the sap comes out and the Syrup goes in, and if the Syrup cannot all be added the sap is put back exactly (nothing is
    // ever lost). Returns the Syrup made, 0 if nothing changed.
    public static int BoilSapInto(InventoryContainer pack, int batches, int day)
    {
        ItemDefinition sap = ItemDatabase.Get(MapleRules.SapId), syrup = ItemDatabase.Get(MapleRules.SyrupId);
        if (pack == null || sap == null || syrup == null)
            return 0;

        MapleRules.Boiled plan = MapleRules.Boil(pack.Count(MapleRules.SapId), batches);
        if (plan.syrup <= 0)
            return 0;

        int taken = pack.Remove(MapleRules.SapId, plan.litresUsed);
        int made = taken == plan.litresUsed ? pack.Add(syrup, plan.syrup, day) : 0;
        if (made == plan.syrup)
            return made;

        if (made > 0)
            pack.Remove(MapleRules.SyrupId, made);
        if (taken > 0)
            pack.Add(sap, taken, day);
        return 0;
    }
}
