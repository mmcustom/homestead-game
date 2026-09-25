using System;

// Simple item recipes for the Inventory screen's Build section. Trapping_System.md: the Rabbit Snare needs Cordage and
// the Box Trap needs Wood; Fishing_System.md's Fish Trap loop starts with "Build Trap". Quantities are Claude Code's
// first pass (2026-09-25) — Firewood stands in for "Wood" since it's the wood the player can gather now.
public static class Crafting
{
    public struct Ingredient
    {
        public string itemId;
        public int quantity;
    }

    public struct Recipe
    {
        public string outputId;
        public Ingredient[] ingredients;
    }

    public static readonly Recipe[] Recipes =
    {
        Make("rabbit_snare", ("cordage", 1)),
        Make("box_trap", ("firewood", 3)),
        Make("fish_trap", ("firewood", 3), ("cordage", 1)),
    };

    static Recipe Make(string output, params (string item, int quantity)[] ingredients) => new Recipe
    {
        outputId = output,
        ingredients = Array.ConvertAll(ingredients, i => new Ingredient { itemId = i.item, quantity = i.quantity }),
    };

    // e.g. "1 Cordage" or "3 Firewood, 1 Cordage".
    public static string Cost(Recipe recipe) =>
        string.Join(", ", Array.ConvertAll(recipe.ingredients, i => $"{i.quantity} {ItemDatabase.Get(i.itemId)?.DisplayName ?? i.itemId}"));

    public static bool CanCraft(Recipe recipe, out string reason)
    {
        InventoryManager inventory = InventoryManager.Instance;
        if (inventory == null)
        {
            reason = "Not available here.";
            return false;
        }

        foreach (Ingredient ingredient in recipe.ingredients)
        {
            int have = inventory.Player.Count(ingredient.itemId);
            if (have < ingredient.quantity)
            {
                reason = $"Needs {ingredient.quantity} {ItemDatabase.Get(ingredient.itemId)?.DisplayName ?? ingredient.itemId} (carrying {have}).";
                return false;
            }
        }

        reason = "";
        return true;
    }

    public static bool Craft(Recipe recipe)
    {
        if (!CanCraft(recipe, out _))
            return false;

        InventoryManager inventory = InventoryManager.Instance;
        foreach (Ingredient ingredient in recipe.ingredients)
            inventory.RemoveFromPlayer(ingredient.itemId, ingredient.quantity);
        // The ingredients weigh at least as much as the trap, so it always fits once they're used.
        inventory.AddToPlayer(recipe.outputId, 1);
        return true;
    }
}
