using System;

// Metalworking (Mike, 2026-11-04; Mining_Metalworking_System.md's addendum): the metals the game knows, as a table so copper, tin and bronze are later rows and not new code.
// Iron is the only row today; its ore is BOUGHT at the Trading Post (there is nothing to mine on the property). Pure and engine-free so edit-mode tests cover it.
//
// What a row holds: the metal's id and name; its ore (id, name, what it costs at the Trading Post in cents, what it weighs); the fuel the furnace burns for it (Charcoal for every
// metal so far; CharcoalRules has the Charcoal Pit that makes it); and what the Bloomery makes from it (phase M2): the bar's id, name and weight, and how much ore and fuel one bar
// takes. BloomeryRules has the furnace itself. The Smithy and the goods come in later phases and will add their own columns.
public static class MetalRules
{
    public const string IronId = "iron";
    public const string IronOreId = "iron_ore";
    public const string IronBarId = "iron_bar";
    public const string CharcoalId = "charcoal";

    // Iron Ore: $4 at the Trading Post (a Stocked item, so it goes back for 40% of that, $1), 3 kg each.
    public const long IronOreBaseCents = 400;
    public const float IronOreKg = 3f;

    // Charcoal: 1 kg each. The Trading Post does not sell it; it buys it for $1 in the wood-and-stone group, so nothing can be bought and sold back at a profit.
    public const long CharcoalBaseCents = 100;
    public const float CharcoalKg = 1f;

    // The Iron Bar (Metalworking M2): 2 kg, made at the Bloomery from 2 Ore and 2 Charcoal. It has no market row in v1: nothing buys it, so nothing can sell it.
    public const float IronBarKg = 2f;
    public const int IronOrePerBar = 2;
    public const int IronCharcoalPerBar = 2;

    public readonly struct Metal
    {
        public readonly string id, name, oreId, oreName, fuelId, barId, barName;
        public readonly long oreBaseCents;
        public readonly float oreKg, barKg;
        public readonly int orePerBar, fuelPerBar;

        public Metal(string id, string name, string oreId, string oreName, long oreBaseCents, float oreKg, string fuelId,
                     string barId, string barName, float barKg, int orePerBar, int fuelPerBar)
        {
            this.id = id;
            this.name = name;
            this.oreId = oreId;
            this.oreName = oreName;
            this.oreBaseCents = oreBaseCents;
            this.oreKg = oreKg;
            this.fuelId = fuelId;
            this.barId = barId;
            this.barName = barName;
            this.barKg = barKg;
            this.orePerBar = orePerBar;
            this.fuelPerBar = fuelPerBar;
        }
    }

    // One row per metal. Add copper and tin here later.
    public static readonly Metal[] Metals =
    {
        new Metal(IronId, "Iron", IronOreId, "Iron Ore", IronOreBaseCents, IronOreKg, CharcoalId, IronBarId, "Iron Bar", IronBarKg, IronOrePerBar, IronCharcoalPerBar),
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

    public static bool TryFindByBar(string barId, out Metal metal)
    {
        foreach (Metal m in Metals)
        {
            if (m.barId == barId)
            {
                metal = m;
                return true;
            }
        }
        metal = default;
        return false;
    }

    public static bool IsOre(string itemId) => TryFindByOre(itemId, out _);

    public static bool IsBar(string itemId) => TryFindByBar(itemId, out _);

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

    // Every bar id, in table order.
    public static string[] BarIds
    {
        get
        {
            var ids = new string[Metals.Length];
            for (int i = 0; i < ids.Length; i++)
                ids[i] = Metals[i].barId;
            return ids;
        }
    }
}
