using System;

// Metalworking, phase M1 (Mike, 2026-11-04; Mining_Metalworking_System.md's addendum): the metals the game knows, as a table so copper, tin and bronze are later rows and not new
// code. Iron is the only row today; its ore is BOUGHT at the Trading Post (there is nothing to mine on the property). Pure and engine-free so edit-mode tests cover it.
//
// What a row holds: the metal's id and name, the id and name of its ore, what the ore costs at the Trading Post (cents) and weighs, and the fuel the furnace burns for it
// (Charcoal for every metal so far; CharcoalRules has the Charcoal Pit that makes it). The bars, the Bloomery and the Smithy come in later phases and will add their own columns.
public static class MetalRules
{
    public const string IronId = "iron";
    public const string IronOreId = "iron_ore";
    public const string CharcoalId = "charcoal";

    // Iron Ore: $4 at the Trading Post (a Stocked item, so it goes back for 40% of that, $1), 3 kg each.
    public const long IronOreBaseCents = 400;
    public const float IronOreKg = 3f;

    // Charcoal: 1 kg each. The Trading Post does not sell it; it buys it for $1 in the wood-and-stone group, so nothing can be bought and sold back at a profit.
    public const long CharcoalBaseCents = 100;
    public const float CharcoalKg = 1f;

    public readonly struct Metal
    {
        public readonly string id, name, oreId, oreName, fuelId;
        public readonly long oreBaseCents;
        public readonly float oreKg;

        public Metal(string id, string name, string oreId, string oreName, long oreBaseCents, float oreKg, string fuelId)
        {
            this.id = id;
            this.name = name;
            this.oreId = oreId;
            this.oreName = oreName;
            this.oreBaseCents = oreBaseCents;
            this.oreKg = oreKg;
            this.fuelId = fuelId;
        }
    }

    // One row per metal. Add copper and tin here later.
    public static readonly Metal[] Metals =
    {
        new Metal(IronId, "Iron", IronOreId, "Iron Ore", IronOreBaseCents, IronOreKg, CharcoalId),
    };

    public static bool TryFind(string metalId, out Metal metal)
    {
        foreach (Metal m in Metals)
        {
            if (m.id == metalId)
            {
                metal = m;
                return true;
            }
        }
        metal = default;
        return false;
    }

    public static bool TryFindByOre(string oreId, out Metal metal)
    {
        foreach (Metal m in Metals)
        {
            if (m.oreId == oreId)
            {
                metal = m;
                return true;
            }
        }
        metal = default;
        return false;
    }

    public static bool IsOre(string itemId) => TryFindByOre(itemId, out _);

    // Every ore id, in table order.
    public static string[] OreIds
    {
        get
        {
            var ids = new string[Metals.Length];
            for (int i = 0; i < ids.Length; i++)
                ids[i] = Metals[i].oreId;
            return ids;
        }
    }
}
