using System;
using System.Collections.Generic;

// How many times in a row a recipe can be crafted, pure and engine-free so edit-mode tests cover it. The Craft tab's amount stepper uses it:
// the most is what the materials allow AND what the player can carry. Each craft pays with the first ingredient set that is affordable
// (Cordage has three), takes the ingredients out of the pack, then puts the result in, so the weight it adds is the result's weight minus
// the weight of the ingredients used up. A craft that adds weight stops the run once it would pass the free carry weight.
public struct CraftIngredient
{
    public string itemId;
    public int quantity;
    public float weightKg;   // one unit of this ingredient

    public CraftIngredient(string itemId, int quantity, float weightKg)
    {
        this.itemId = itemId;
        this.quantity = quantity;
        this.weightKg = weightKg;
    }
}

public static class CraftRules
{
    // The same small slack CarryRules and InventoryContainer.SpaceFor allow, so an exact fit is not refused by float error.
    public const float Tolerance = 0.0001f;

    // The most crafts, up to cap. sets: the ways to pay (the first affordable one is used each time). have: how many of each item are
    // carried. outputKgPerCraft: the weight of everything one craft makes. freeKg: the room left under the carry limit now.
    public static int MaxCrafts(IList<IList<CraftIngredient>> sets, IDictionary<string, int> have, float outputKgPerCraft, float freeKg, int cap)
    {
        if (sets == null || have == null || cap <= 0)
            return 0;

        var left = new Dictionary<string, int>(have);
        float free = freeKg;
        int crafts = 0;
        while (crafts < cap)
        {
            IList<CraftIngredient> paid = null;
            foreach (IList<CraftIngredient> set in sets)
            {
                if (set != null && set.Count > 0 && AllCovered(set, left))
                {
                    paid = set;
                    break;
                }
            }
            if (paid == null)
                break;

            float used = 0f;
            foreach (CraftIngredient i in paid)
                used += i.weightKg * i.quantity;

            float added = Math.Max(0f, outputKgPerCraft) - used;   // what this craft adds to the pack
            if (added > 0f && added > free + Tolerance)
                break;

            foreach (CraftIngredient i in paid)
                left[i.itemId] = Count(left, i.itemId) - i.quantity;
            free -= added;
            crafts++;
        }
        return crafts;
    }

    static int Count(Dictionary<string, int> counts, string itemId) => counts.TryGetValue(itemId ?? "", out int n) ? n : 0;

    static bool AllCovered(IList<CraftIngredient> set, Dictionary<string, int> left)
    {
        foreach (CraftIngredient i in set)
            if (Count(left, i.itemId) < i.quantity)
                return false;
        return true;
    }

    // What the Craft tab's stepper may offer: the crafts that fit, but at least one while the materials allow one, so a single craft keeps
    // the old rule (made, with any excess left on the ground) rather than being blocked outright.
    public static int StepperMax(int byMaterials, int fitting) => byMaterials <= 0 ? 0 : Math.Max(1, Math.Min(byMaterials, fitting));
}
