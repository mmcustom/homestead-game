using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

// EditMode tests for the Wood Saw (Mike, 2026-10-31): the Trading Post row and its sell-back, the ledger category, the item asset, and AxeRules, the pure
// arithmetic behind the felling tools (AxeTool itself needs a scene, so its numbers live in AxeRules). The item asset and the AxeTool type are in the main
// assembly, which a test assembly cannot reference, so they are reached by reflection; the MarketData asset is read as text, as MarketTests does.
// Nothing here touches the Saves folder or a scene.
public class WoodSawTests
{
    static Type Find(string name) =>
        AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => { try { return a.GetTypes(); } catch (Exception) { return new Type[0]; } })
                 .First(t => t.Name == name);

    static MarketTable Table => MarketCatalog.Table;

    const float Primitive = AxeRules.DefaultPrimitiveFactor;
    const float Saw = AxeRules.DefaultSawFactor;

    // --- The Trading Post row ---

    [Test]
    public void TheSawIsAStockedItemAtThirtyDollarsOneAndAHalfTimesTheAxe()
    {
        Assert.That(Table.TryGetRow("wood_saw", out MarketRow row), Is.True);
        Assert.That(row.kind, Is.EqualTo(MarketKind.Stocked));
        Assert.That(row.group, Is.EqualTo(MarketGroup.Stocked));
        Assert.That(row.baseCents, Is.EqualTo(3000L));
        Assert.That(row.seasonalBuy, Is.False);
        Assert.That(AxeRules.SawBaseCents, Is.EqualTo(3000L));
        Assert.That(Table.BuyBaseCents("wood_saw"), Is.EqualTo(Table.BuyBaseCents("axe") * 3 / 2));
        Assert.That(Table.Stock.Any(r => r.itemId == "wood_saw"), Is.True, "it is on the Trading Post's shelf");
    }

    [Test]
    public void TheSawSellsBackForTwelveDollars()
    {
        Assert.That(Table.SellBaseCents("wood_saw"), Is.EqualTo(1200L), "40% of $30.00");
        Assert.That(Table.HasPool("wood_saw"), Is.True);
    }

    [Test]
    public void TheSawCannotBeBoughtAndSoldForProfit()
    {
        MarketGroupRule rule = Table.RuleOf("wood_saw");
        SaleResult sale = MarketRules.SellStack(Table.SellBaseCents("wood_saw"), MarketRules.One, 1, rule.dropBp, rule.floorBp);
        Assert.That(sale.totalCents, Is.LessThan(Table.BuyBaseCents("wood_saw")));
    }

    [Test]
    public void ThePurchaseIsCountedUnderTools()
    {
        Assert.That(LedgerCategories.PurchaseIsListed("wood_saw"), Is.True);
        Assert.That(LedgerCategories.ForPurchase("wood_saw"), Is.EqualTo(LedgerCategory.BuyTools));
        Assert.That(LedgerCategories.ForPurchase("axe"), Is.EqualTo(LedgerCategory.BuyTools), "the Axe is where it was");
    }

    // The asset is what the game reads and it is not merged with the catalog, so a catalog row missing from it would simply never be on sale.
    [Test]
    public void EveryStockedCatalogRowIsInTheMarketDataAssetWithTheSameSettings()
    {
        string path = Path.Combine(Application.dataPath, "Resources", "Market", "MarketData.asset");
        Assume.That(File.Exists(path), "MarketData.asset has not been seeded");
        string text = File.ReadAllText(path).Replace("\r\n", "\n");
        foreach (MarketRow row in MarketCatalog.Rows.Where(r => r.kind == MarketKind.Stocked))
        {
            string want = $"- itemId: {row.itemId}\n    group: {(int)row.group}\n    pool: {row.pool}\n    baseCents: {row.baseCents}\n    kind: {(int)row.kind}\n    seasonalBuy: {(row.seasonalBuy ? 1 : 0)}";
            Assert.That(text, Does.Contain(want), row.itemId + " is on sale in the game only if the asset has its row");
        }
        Assert.That(Regex.Matches(text, @"- itemId: wood_saw\n").Count, Is.EqualTo(1), "one row, not two");
    }

    // --- The item asset ---

    [Test]
    public void TheSawIsATwoKilogramToolCalledWoodSaw()
    {
        object item = Find("ItemDatabase").GetMethod("Get").Invoke(null, new object[] { "wood_saw" });
        Assert.That(item, Is.Not.Null, "Resources/Items/Tools/wood_saw.asset");
        Type def = item.GetType();
        Assert.That((string)def.GetProperty("DisplayName").GetValue(item), Is.EqualTo("Wood Saw"));
        Assert.That(def.GetProperty("Category").GetValue(item).ToString(), Is.EqualTo("Tool"));
        Assert.That((float)def.GetProperty("WeightKg").GetValue(item), Is.EqualTo(AxeRules.SawWeightKg));
        Assert.That(AxeRules.SawWeightKg, Is.EqualTo(2f), "the Axe's weight");
        Assert.That((bool)def.GetProperty("IsPerishable").GetValue(item), Is.False);
    }

    // --- Saves and old games ---

    [Test]
    public void NothingTheSawAddsIsSaved()
    {
        Assert.That(Find("ISaveable").IsAssignableFrom(Find("AxeTool")), Is.False, "AxeTool saves nothing, so old saves load as they were");
        Assert.That(LedgerSaveData.CurrentVersion, Is.EqualTo(1), "the ledger version is unchanged");
    }

    [Test]
    public void TheToolIdsAreUnchangedAndTheFactorsAreTunablesOnTheComponent()
    {
        Assert.That(AxeRules.AxeId, Is.EqualTo("axe"));
        Assert.That(AxeRules.PrimitiveAxeId, Is.EqualTo("primitive_axe"));
        Assert.That(AxeRules.SawId, Is.EqualTo("wood_saw"));
        Assert.That(Find("AxeTool").GetField("SawId").GetRawConstantValue(), Is.EqualTo("wood_saw"));
        BindingFlags field = BindingFlags.NonPublic | BindingFlags.Instance;
        foreach (string name in new[] { "primitiveAxeFactor", "sawFactor" })
            Assert.That(Find("AxeTool").GetField(name, field)?.GetCustomAttribute<SerializeField>(), Is.Not.Null, name + " is a serialized field on the component");
        Assert.That(Primitive, Is.EqualTo(1.5f));
        Assert.That(Saw, Is.EqualTo(0.65f));
    }

    // --- Which tools fell ---

    [TestCase("axe", true)]
    [TestCase("primitive_axe", true)]
    [TestCase("wood_saw", true)]
    [TestCase("hammer", false)]
    [TestCase("shovel", false)]
    [TestCase("stone_pick_axe", false)]
    [TestCase(null, false)]
    public void OnlyTheThreeFellingToolsFell(string id, bool fells) => Assert.That(AxeRules.IsFellingTool(id), Is.EqualTo(fells));

    [Test]
    public void TheSawIsFasterThanTheAxeWhichIsFasterThanThePrimitiveAxe()
    {
        Assert.That(AxeRules.FactorFor("wood_saw", Primitive, Saw), Is.EqualTo(0.65f));
        Assert.That(AxeRules.FactorFor("axe", Primitive, Saw), Is.EqualTo(1f));
        Assert.That(AxeRules.FactorFor("primitive_axe", Primitive, Saw), Is.EqualTo(1.5f));
        Assert.That(AxeRules.FactorFor("hammer", Primitive, Saw), Is.EqualTo(1f), "anything else is the plain factor");
        Assert.That(AxeRules.FactorFor(null, Primitive, Saw), Is.EqualTo(1f));
    }

    // --- Blows to fell ---

    [TestCase(10f, 1.12f, 11f, 17f, 7f)]   // an average broad hardwood
    [TestCase(13f, 1.12f, 15f, 22f, 9f)]   // an average tall hardwood
    [TestCase(10f, 0.72f, 7f, 11f, 5f)]    // a small broad hardwood
    [TestCase(13f, 0.72f, 9f, 14f, 6f)]    // a small tall hardwood
    [TestCase(10f, 1.57f, 16f, 24f, 10f)]  // a large broad hardwood
    [TestCase(13f, 1.57f, 20f, 31f, 13f)]  // a large tall hardwood
    public void BlowsToFellAtThreeSizes(float baseBlows, float size, float axe, float primitive, float saw)
    {
        Assert.That(AxeRules.FellBlows(baseBlows, size, 1f), Is.EqualTo(axe), "Axe");
        Assert.That(AxeRules.FellBlows(baseBlows, size, Primitive), Is.EqualTo(primitive), "Primitive Axe");
        Assert.That(AxeRules.FellBlows(baseBlows, size, Saw), Is.EqualTo(saw), "Wood Saw");
    }

    [Test]
    public void ATreeNeverTakesFewerThanOneBlow()
    {
        Assert.That(AxeRules.FellBlows(3f, 0.3f, Saw), Is.EqualTo(1f), "a tiny shrub with the Saw");
        Assert.That(AxeRules.FellBlows(3f, 1f, Saw), Is.EqualTo(2f), "a shrub of size 1 with the Saw: 1.95 rounds to 2");
    }

    [Test]
    public void TheSawNeedsAboutAThirdFewerBlowsThanTheAxeAtEveryTreeSize()
    {
        for (float size = 0.72f; size <= 1.571f; size += 0.05f)
            foreach (float baseBlows in new[] { 10f, 13f })
            {
                float axe = AxeRules.FellBlows(baseBlows, size, 1f);
                float saw = AxeRules.FellBlows(baseBlows, size, Saw);
                Assert.That(saw, Is.LessThan(axe), $"size {size:0.00} base {baseBlows}");
                Assert.That(saw, Is.LessThanOrEqualTo(axe * 0.75f), $"size {size:0.00} base {baseBlows}: at least a quarter fewer");
            }
    }

    [Test]
    public void ATreesSizeIsItsHeightTimesTheRootOfItsWidth()
    {
        Assert.That(AxeRules.TreeSize(1.2f, 1.44f), Is.EqualTo(1.44f).Within(0.0001f));
        Assert.That(AxeRules.TreeSize(1f, 1f), Is.EqualTo(1f));
    }

    // --- Stamina: a blow is the only thing that costs it ---

    [Test]
    public void StaminaPerTreeFollowsTheBlowsSoTheSawSavesAboutAThirdOfIt()
    {
        const float perSwing = 4f; // AxeTool.staminaPerSwing
        // An average broad hardwood (size 1.12) and an average tall one.
        Assert.That(AxeRules.StaminaForBlows(AxeRules.FellBlows(10f, 1.12f, 1f), perSwing), Is.EqualTo(44f));
        Assert.That(AxeRules.StaminaForBlows(AxeRules.FellBlows(10f, 1.12f, Saw), perSwing), Is.EqualTo(28f));
        Assert.That(AxeRules.StaminaForBlows(AxeRules.FellBlows(13f, 1.12f, 1f), perSwing), Is.EqualTo(60f));
        Assert.That(AxeRules.StaminaForBlows(AxeRules.FellBlows(13f, 1.12f, Saw), perSwing), Is.EqualTo(36f));

        // With 57% broad and 43% tall among the felled trees: about 50.9 with the Axe, 31.4 with the Saw.
        float axeMix = 0.57f * 44f + 0.43f * 60f, sawMix = 0.57f * 28f + 0.43f * 36f;
        Assert.That(axeMix, Is.EqualTo(50.9f).Within(0.1f));
        Assert.That(sawMix, Is.EqualTo(31.4f).Within(0.1f));
        Assert.That(sawMix / axeMix, Is.LessThan(0.65f));
    }

    // --- Splitting: at a pile (the equipped tool) and from the Inventory (the fastest carried) ---

    [TestCase(4, 4, 6, 3)]   // a Log
    [TestCase(2, 2, 3, 1)]   // a lot of Branches
    public void BlowsToSplitAtAPile(int baseBlows, int axe, int primitive, int saw)
    {
        Assert.That(AxeRules.SplitBlows(baseBlows, 1f), Is.EqualTo(axe));
        Assert.That(AxeRules.SplitBlows(baseBlows, Primitive), Is.EqualTo(primitive));
        Assert.That(AxeRules.SplitBlows(baseBlows, Saw), Is.EqualTo(saw));
    }

    [Test]
    public void SecondsToSplitACarriedLogOrBranches()
    {
        Assert.That(AxeRules.SplitSeconds(4f, 1f), Is.EqualTo(4f));
        Assert.That(AxeRules.SplitSeconds(4f, Primitive), Is.EqualTo(6f));
        Assert.That(AxeRules.SplitSeconds(4f, Saw), Is.EqualTo(2.6f).Within(0.0001f));
        Assert.That(AxeRules.SplitSeconds(2f, Saw), Is.EqualTo(1.3f).Within(0.0001f));
    }

    // axe, primitive axe, saw carried -> the speed: the fastest one, or 0 when none can split.
    [TestCase(false, false, false, 0f)]
    [TestCase(true, false, false, 1f)]      // only an Axe: as before
    [TestCase(false, true, false, 1.5f)]    // only a Primitive Axe: as before
    [TestCase(true, true, false, 1f)]       // both old axes: the Axe, as before
    [TestCase(false, false, true, 0.65f)]   // only a Saw works
    [TestCase(true, false, true, 0.65f)]
    [TestCase(false, true, true, 0.65f)]
    [TestCase(true, true, true, 0.65f)]
    public void TheFastestCarriedToolSetsTheInventorysSplittingSpeed(bool axe, bool primitive, bool saw, float want) =>
        Assert.That(AxeRules.FastestCarried(axe, primitive, saw, Primitive, Saw), Is.EqualTo(want));
}
