using UnityEngine;

// Cooking and boiling at a lit campfire (the Cooking and Water Purification entries, 2026-09-25). Standing within
// reach of a burning campfire, the Inventory screen offers Cook on raw meat and fish — each becomes its cooked
// Consumable, safe to eat, more filling and keeping longer — and Boil on collected raw water, which becomes Purified
// Water. Boiling needs a container: the Bucket (Water_System.md: Boiling requires Fire and a Container).
// Each piece takes a few seconds (CampfireCooking runs the queue and calls Cook as each one finishes); it doesn't burn
// extra fuel.
public static class Cooking
{
    public const string PurifiedWaterId = "water";
    const string ContainerId = "bucket";

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

    // The lit campfire the player can cook at, or null.
    public static CampfireState FireInReach(PlayerController player)
    {
        if (player == null || FireManager.Instance == null)
            return null;
        return FireManager.Instance.NearestLit(player.transform.position, Range);
    }

    public static bool HasContainer =>
        InventoryManager.Instance != null && InventoryManager.Instance.Player.Count(ContainerId) > 0;

    // Turns up to count of an item into its cooked (or boiled) version straight away. Returns how many were done.
    public static int Cook(ItemDefinition raw, int count, PlayerController player)
    {
        InventoryManager inventory = InventoryManager.Instance;
        if (inventory == null || raw == null || count <= 0 || FireInReach(player) == null)
            return 0;

        bool boil = IsBoilable(raw.Id);
        string outputId = boil ? PurifiedWaterId : CookedId(raw.Id);
        ItemDefinition output = outputId != null ? ItemDatabase.Get(outputId) : null;
        if (output == null)
            return 0;
        if (boil && !HasContainer)
        {
            ToolStatus.Flash("Boiling water needs a Bucket to boil it in");
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
}
