using System;

// Simple item recipes for the Inventory screen's Build section. Trapping_System.md: the Rabbit Snare needs Cordage and
// the Box Trap needs Wood; Fishing_System.md's Fish Trap loop starts with "Build Trap". Quantities are Claude Code's
// first pass (2026-09-25) — Firewood stands in for "Wood" since it's the wood the player can gather now.
// 2026-09-26: the primitive hand tools — the Stone Pick Axe (Stone_Gathering_System.md) and Primitive Shovel
// (Wood_Gathering_System.md) are a stone head lashed to a stick handle; the Pouch (Primitive_Storage_System.md) is a
// hide sewn with cordage. Each takes one Cordage, which the player starts with three of.
// Cordage itself (Trapping_System.md's sourcing, 2026-09-26) twists from whichever natural fibre the player has: 3 Tall
// Grass, 2 Cattail stalks, or 1 Sinew from a deer or turkey — one recipe with alternative ingredients.
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
        // Other ingredient sets that make the same thing; the first the player can afford is used.
        public Ingredient[][] alternatives;
    }

    public static readonly Recipe[] Recipes =
    {
        Make("rabbit_snare", ("cordage", 1)),
        Make("box_trap", ("firewood", 3)),
        Make("fish_trap", ("firewood", 3), ("cordage", 1)),
        Make("stone_pick_axe", ("sticks", 2), ("cordage", 1), ("stone", 1)),
        Make("shovel", ("sticks", 3), ("cordage", 1), ("stone", 1)),
        Make("pouch", ("deer_hide", 1), ("cordage", 1)),
        Either("cordage", new[] { ("tall_grass", 3) }, new[] { ("cattail", 2) }, new[] { ("sinew", 1) }),
    };

    static Recipe Either(string output, params (string item, int quantity)[][] sets)
    {
        Recipe recipe = Make(output, sets[0]);
        recipe.alternatives = Array.ConvertAll(sets, set =>
            Array.ConvertAll(set, i => new Ingredient { itemId = i.item, quantity = i.quantity }));
        return recipe;
    }

    static Ingredient[][] Sets(Recipe recipe) => recipe.alternatives ?? new[] { recipe.ingredients };

    // The ingredient set the player can afford right now, or null.
    static Ingredient[] Affordable(Recipe recipe)
    {
        InventoryManager inventory = InventoryManager.Instance;
        if (inventory == null)
            return null;
        foreach (Ingredient[] set in Sets(recipe))
            if (Array.TrueForAll(set, i => inventory.Player.Count(i.itemId) >= i.quantity))
                return set;
        return null;
    }

    static Recipe Make(string output, params (string item, int quantity)[] ingredients) => new Recipe
    {
        outputId = output,
        ingredients = Array.ConvertAll(ingredients, i => new Ingredient { itemId = i.item, quantity = i.quantity }),
    };

    // e.g. "1 Cordage", "3 Firewood, 1 Cordage", or "3 Tall Grass / 2 Cattail / 1 Sinew" for alternatives.
    public static string Cost(Recipe recipe) =>
        string.Join(" / ", Array.ConvertAll(Sets(recipe), set =>
            string.Join(", ", Array.ConvertAll(set, i => $"{i.quantity} {ItemDatabase.Get(i.itemId)?.DisplayName ?? i.itemId}"))));

    public static bool CanCraft(Recipe recipe, out string reason)
    {
        InventoryManager inventory = InventoryManager.Instance;
        if (inventory == null)
        {
            reason = "Not available here.";
            return false;
        }

        if (Affordable(recipe) != null)
        {
            reason = "";
            return true;
        }
        // Say what's short in the first set (or "any of" for alternatives).
        if (recipe.alternatives != null)
        {
            reason = $"Needs {Cost(recipe)}.";
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
        return false;
    }

    public static bool Craft(Recipe recipe)
    {
        if (!CanCraft(recipe, out _))
            return false;

        InventoryManager inventory = InventoryManager.Instance;
        foreach (Ingredient ingredient in Affordable(recipe))
            inventory.RemoveFromPlayer(ingredient.itemId, ingredient.quantity);
        // The ingredients weigh at least as much as what's made, so it always fits once they're used.
        inventory.AddToPlayer(recipe.outputId, 1);
        return true;
    }
}
