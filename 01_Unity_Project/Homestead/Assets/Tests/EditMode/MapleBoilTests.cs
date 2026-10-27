using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

// EditMode tests for Maple C: boiling sap into Syrup. The yield rules are pure (MapleRules.Boil, Homestead.Economy); the swap in a pack
// (Cooking.BoilSapInto) and the items live in the main assembly, reached by reflection as the other Maple tests do.
public class MapleBoilTests
{
    // --- The yield: whole batches of 10 L ---

    [TestCase(0, 0, 0)]
    [TestCase(8, 0, 8)]
    [TestCase(9, 0, 9)]
    [TestCase(10, 1, 0)]
    [TestCase(16, 1, 6)]
    [TestCase(19, 1, 9)]
    [TestCase(20, 2, 0)]
    [TestCase(100, 10, 0)]
    public void AFullBoilOfAStackMakesWholeSyrupAndKeepsTheRest(int litres, int syrup, int left)
    {
        MapleRules.Boiled boiled = MapleRules.Boil(litres, int.MaxValue);
        Assert.That(boiled.syrup, Is.EqualTo(syrup));
        Assert.That(boiled.litresLeft, Is.EqualTo(left));
        Assert.That(boiled.litresUsed, Is.EqualTo(syrup * 10));
    }

    [Test]
    public void EightTenAndSixteenLitresGiveNoneOneAndOneSyrup()
    {
        Assert.That(MapleRules.Boil(8, 5).syrup, Is.EqualTo(0));
        Assert.That(MapleRules.Boil(10, 5).syrup, Is.EqualTo(1));
        Assert.That(MapleRules.Boil(16, 5).syrup, Is.EqualTo(1));
        Assert.That(MapleRules.Boil(16, 5).litresLeft, Is.EqualTo(6), "the 6 L stay as sap, to top up later");
    }

    [Test]
    public void ABoilNeverMakesMoreBatchesThanAskedOrThanTheSapAllows()
    {
        Assert.That(MapleRules.Boil(35, 2).syrup, Is.EqualTo(2));
        Assert.That(MapleRules.Boil(35, 2).litresLeft, Is.EqualTo(15));
        Assert.That(MapleRules.Boil(35, 9).syrup, Is.EqualTo(3), "only three whole batches in 35 L");
        Assert.That(MapleRules.Boil(35, 0).syrup, Is.EqualTo(0));
        Assert.That(MapleRules.Boil(35, -3).syrup, Is.EqualTo(0));
        Assert.That(MapleRules.Boil(-5, 3).syrup, Is.EqualTo(0));
        Assert.That(MapleRules.Boil(-5, 3).litresLeft, Is.EqualTo(0));
    }

    [Test]
    public void NoSapIsEverLostInABoil()
    {
        for (int litres = 0; litres <= 120; litres++)
        for (int wanted = 0; wanted <= 13; wanted++)
        {
            MapleRules.Boiled b = MapleRules.Boil(litres, wanted);
            Assert.That(b.litresUsed + b.litresLeft, Is.EqualTo(litres), $"{litres} L, {wanted} batches");
            Assert.That(b.litresLeft, Is.GreaterThanOrEqualTo(0));
            Assert.That(b.litresUsed, Is.EqualTo(b.syrup * MapleRules.SapPerSyrup));
        }
    }

    [Test]
    public void ABatchTakesTwelveSeconds()
    {
        Assert.That(MapleRules.SecondsPerSyrup, Is.EqualTo(12f).Within(1e-4f));
    }

    // --- The words ---

    [Test]
    public void TheRowSaysWhyAStackCannotBoilAndWhatABoilWouldMake()
    {
        Assert.That(MapleRules.NeedMessage(8), Is.EqualTo("needs 10 L of sap, have 8 L"));
        Assert.That(MapleRules.BoilInfo(8), Is.EqualTo("needs 10 L of sap, have 8 L"));
        Assert.That(MapleRules.BoilInfo(10), Is.EqualTo("10 L -> 1 Syrup"));
        Assert.That(MapleRules.BoilInfo(16), Is.EqualTo("16 L -> 1 Syrup, 6 L left over"));
        Assert.That(MapleRules.BoiledMessage(2, 20), Is.EqualTo("Boiled 20 L of Maple Sap into 2 Maple Syrup"));
    }

    // --- The swap in a pack (Cooking.BoilSapInto) ---

    static Type Find(string name) =>
        AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => { try { return a.GetTypes(); } catch (Exception) { return new Type[0]; } })
                 .First(t => t.Name == name);

    static object Item(string id) => Find("ItemDatabase").GetMethod("Get").Invoke(null, new object[] { id });

    static object Pack(float maxKg) => Activator.CreateInstance(Find("InventoryContainer"), "test pack", maxKg);

    static int Count(object pack, string id) => (int)pack.GetType().GetMethod("Count").Invoke(pack, new object[] { id });

    static void Give(object pack, string id, int quantity) =>
        pack.GetType().GetMethod("Add").Invoke(pack, new object[] { Item(id), quantity, 0 });

    static int Boil(object pack, int batches) =>
        (int)Find("Cooking").GetMethod("BoilSapInto").Invoke(null, new object[] { pack, batches, 0 });

    [Test]
    public void BoilingSixteenLitresInAPackLeavesOneSyrupAndSixLitres()
    {
        object pack = Pack(100f);
        Give(pack, MapleRules.SapId, 16);
        Assert.That(Boil(pack, int.MaxValue), Is.EqualTo(1));
        Assert.That(Count(pack, MapleRules.SapId), Is.EqualTo(6));
        Assert.That(Count(pack, MapleRules.SyrupId), Is.EqualTo(1));
    }

    [Test]
    public void BoilingEightLitresChangesNothing()
    {
        object pack = Pack(100f);
        Give(pack, MapleRules.SapId, 8);
        Assert.That(Boil(pack, int.MaxValue), Is.EqualTo(0));
        Assert.That(Count(pack, MapleRules.SapId), Is.EqualTo(8));
        Assert.That(Count(pack, MapleRules.SyrupId), Is.EqualTo(0));
    }

    [Test]
    public void BoilingTwentyLitresOneBatchAtATimeTakesTenEachTime()
    {
        object pack = Pack(100f);
        Give(pack, MapleRules.SapId, 20);
        Assert.That(Boil(pack, 1), Is.EqualTo(1));
        Assert.That(Count(pack, MapleRules.SapId), Is.EqualTo(10));
        Assert.That(Boil(pack, 1), Is.EqualTo(1));
        Assert.That(Count(pack, MapleRules.SapId), Is.EqualTo(0));
        Assert.That(Count(pack, MapleRules.SyrupId), Is.EqualTo(2));
    }

    [Test]
    public void ABoilWorksWithTheLoadAtTheLimitBecauseSyrupWeighsLessThanTheSap()
    {
        object pack = Pack(10f); // exactly 10 kg of sap and no spare room
        Give(pack, MapleRules.SapId, 10);
        Assert.That(Boil(pack, 1), Is.EqualTo(1));
        Assert.That(Count(pack, MapleRules.SapId), Is.EqualTo(0));
        Assert.That(Count(pack, MapleRules.SyrupId), Is.EqualTo(1));
    }

    // --- Selling Syrup (the market rows are MapleTests'; the CSVs are checked here) ---

    [Test]
    public void ASyrupSaleShowsInTheItemCsvAndFarmProduceInTheSeasonCsv()
    {
        var ledger = new LedgerState();
        ledger.Begin(100000, 0, 360, 28);
        SaleResult sale = MarketRules.SellStack(1000, 10000, 3, 500, 2500);
        ledger.RecordSale(LedgerCategories.ForSale(MarketGroup.Maple), "maple_syrup", sale.count, sale.totalCents);
        List<StatementRow> rows = ledger.Rows(100000, 0, 0, 400);

        Assert.That(LedgerCsv.BuildItems(rows), Does.Contain("maple_syrup,3,"), "the item row: units sold");
        Assert.That(LedgerCategories.Name(LedgerCategories.ForSale(MarketGroup.Maple)), Is.EqualTo("Farm produce"));
        Assert.That(LedgerCsv.Build(rows), Does.Contain("Farm produce"));
        Assert.That(rows[0].cents[(int)LedgerCategory.SalesFarm], Is.EqualTo(sale.totalCents));
    }

    [Test]
    public void SapCannotBeSoldButSyrupCan()
    {
        MarketTable table = MarketCatalog.Table;
        Assert.That(table.SellBaseCents(MapleRules.SapId), Is.EqualTo(0));
        Assert.That(table.SellBaseCents(MapleRules.SyrupId), Is.EqualTo(MapleRules.SyrupBaseCents));
    }
}
