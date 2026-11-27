using System.Linq;
using NUnit.Framework;

// Everything the player needs or can build is on the Trading Post's shelf (Mike, 2026-11-27; Trading_Post_Market.md, "Change 2026-11-27").
public class SupplyTests
{
    static MarketTable Table => MarketCatalog.Table;

    // Every material a building, a craft, a forge job or a tanning step uses.
    static readonly string[] Materials =
    {
        "sticks", "branches", "tall_grass", "clay", "sand", "logs", "stone", "firewood", "lumber", "cordage", "nails_box", "salt", "iron_ore",
        "charcoal", "iron_bar", "fertilizer", "tanbark", "rawhide", "sinew", "leather", "buckskin", "tanned_furs", "wooden_wheel", "tripod",
        "window_sash", "greenhouse_hoop", "greenhouse_plastic", "glass_pane", "greenhouse_stove", "propane_heater", "propane_tank",
        "garden_hose", "drip_kit", "hand_pump", "fuel_pump", "gasoline_can", "vent_opener", "wire_mesh", "row_cover",
    };

    [Test]
    public void EveryMaterialCanBeBought()
    {
        foreach (string id in Materials)
            Assert.That(Table.BuyBaseCents(id), Is.GreaterThan(0L), id);
    }

    [Test]
    public void RawAndHomemadeGoodsSellAtTwoAndAHalfTimesWhatTheStorePays()
    {
        foreach ((string id, long pays) in new[] { ("sticks", 10L), ("branches", 20L), ("tall_grass", 10L), ("clay", 50L), ("sand", 30L), ("tanbark", 50L),
                                                    ("logs", 300L), ("stone", 100L), ("firewood", 100L), ("fertilizer", 50L), ("rawhide", 1200L), ("sinew", 100L),
                                                    ("wooden_wheel", 200L), ("tripod", 200L), ("leather_pack", 7500L), ("buckskin_coat", 9000L), ("moccasins", 3000L) })
        {
            Assert.That(Table.SellBaseCents(id), Is.EqualTo(pays), id + " bought");
            Assert.That(Table.BuyBaseCents(id), Is.EqualTo(pays * 5 / 2), id + " sold");
        }
    }

    [Test]
    public void ChainGoodsSellAtFourTimesSoMakingThemPays()
    {
        Assert.That(SupplyRules.ChainStoreCents(100), Is.EqualTo(400L));
        foreach ((string id, long pays) in new[] { ("charcoal", 100L), ("leather", 3000L), ("buckskin", 2400L), ("tanned_furs", 600L) })
        {
            Assert.That(Table.SellBaseCents(id), Is.EqualTo(pays), id + " bought");
            Assert.That(Table.BuyBaseCents(id), Is.EqualTo(pays * 4), id + " sold");
        }
        Assert.That(Table.BuyBaseCents("iron_bar"), Is.EqualTo(2000L), "an Iron Bar is $20");
        Assert.That(Table.SellBaseCents("iron_bar"), Is.EqualTo(0L), "and never bought back");
        Assert.That(Table.HasPool("iron_bar"), Is.False);
    }

    [Test]
    public void EveryShelfItemHasALedgerLineAndTheCatalogHasNoProblems()
    {
        foreach (MarketRow row in Table.Stock)
            Assert.That(LedgerCategories.PurchaseIsListed(row.itemId), Is.True, row.itemId);
        Assert.That(Table.Problems, Is.Empty);
    }
}
