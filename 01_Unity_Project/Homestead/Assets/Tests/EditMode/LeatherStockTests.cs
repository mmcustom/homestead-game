using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

// Mike's play-test (2026-11-25): "we can't stage materials for leatherwork?" The Workshop bench now stocks the leather goods as well as the wood, R (or the Leatherwork
// screen's button) puts them in, and a leather piece takes from the bench first, then the pack.
public class LeatherStockTests
{
    static string SourceText(string relative) => File.ReadAllText(Path.Combine(Application.dataPath, relative)).Replace("\r\n", "\n");

    [Test]
    public void TheBenchStocksTheLeatherGoodsButNotTheLashing()
    {
        Assert.That(WorkshopRules.LeatherStockIds, Is.EqualTo(new[] { "buckskin", "tanned_furs", "sinew", "rawhide", "leather" }));
        Assert.That(WorkshopRules.AllStockIds, Is.EqualTo(new[] { "logs", "branches", "cordage", "buckskin", "tanned_furs", "sinew", "rawhide", "leather" }));
        Assert.That(WorkshopRules.StockIds, Is.EqualTo(new[] { "logs", "branches", "cordage" }), "the woodwork's own list is unchanged");
        Assert.That(WorkshopRules.IsStockId("rawhide_lashing"), Is.False, "a Lashing in the bench would read as a finished piece waiting");
        foreach (string id in WorkshopRules.LeatherStockIds)
            Assert.That(WorkshopRules.IsProduct(id), Is.False, id + " must never look like a finished piece");
    }

    [TestCase("buckskin", 6)]
    [TestCase("tanned_furs", 4)]
    [TestCase("sinew", 4)]
    [TestCase("rawhide", 4)]
    [TestCase("leather", 4)]
    [TestCase("logs", 6)]
    [TestCase("rawhide_lashing", 0)]
    public void EachLeatherGoodHasACap(string id, int cap)
    {
        Assert.That(WorkshopRules.CapOf(id), Is.EqualTo(cap));
        if (cap > 0)
        {
            Assert.That(WorkshopRules.Accepts(id, cap - 1), Is.True);
            Assert.That(WorkshopRules.Accepts(id, cap), Is.False);
        }
        else
            Assert.That(WorkshopRules.Accepts(id, 0), Is.False);
    }

    [Test]
    public void TheCapsHoldAtLeastTwoCoatsWorthOfTheBiggestNeed()
    {
        foreach (string id in WorkshopRules.LeatherStockIds)
        {
            int biggest = ClothingRules.Pieces.SelectMany(p => p.needs).Where(n => n.itemId == id).Select(n => n.count).DefaultIfEmpty(0).Max();
            Assert.That(WorkshopRules.CapOf(id), Is.GreaterThanOrEqualTo(biggest * 2), id);
        }
    }

    [Test]
    public void APieceTakesFromTheBenchFirst()
    {
        Assert.That(ClothingRules.TakeFromBench(3, 1), Is.EqualTo(1));
        Assert.That(ClothingRules.TakeFromBench(3, 5), Is.EqualTo(3));
        Assert.That(ClothingRules.TakeFromBench(2, 0), Is.EqualTo(0));
        Assert.That(ClothingRules.TakeFromBench(2, -1), Is.EqualTo(0));
        string ps = SourceText("Scripts/World/PlacedStructure.cs");
        Assert.That(ps, Does.Contain("state.Take(need.itemId, fromBench);"));
        Assert.That(ps, Does.Contain("ClothingRules.CanStart(piece, Inventory != null && Inventory.Player.Has(ClothingRules.KnifeId), WorkshopWorking, WorkshopProduct(out _, out _), LeatherCount);"));
        Assert.That(ps, Does.Contain("foreach (string stockId in WorkshopRules.AllStockIds)"), "dismantling gives the leather stock back too");
        Assert.That(ps, Does.Contain("foreach (string id in WorkshopRules.AllStockIds)"), "R stores leather goods");
    }

    [Test]
    public void TheBenchAndThePackCountTogether()
    {
        int bench(string id) => id == "buckskin" ? 2 : id == "sinew" ? 1 : 0;
        int pack(string id) => id == "buckskin" ? 1 : id == "sinew" ? 1 : id == "tanned_furs" ? 1 : 0;
        Func<string, int> both = id => bench(id) + pack(id);
        ClothingRules.Piece coat = ClothingRules.Pieces[0];
        Assert.That(ClothingRules.Covers(coat, both), Is.True, "3 Buckskin, 1 Tanned Furs and 2 Sinew between them");
        Assert.That(ClothingRules.Covers(coat, pack), Is.False, "the pack alone is short");
        var rows = ClothingRules.NeedRows(coat, both);
        Assert.That(ClothingRules.NeedText(rows[0], bench("buckskin")), Is.EqualTo("Buckskin - need 3 - bench 2 + pack 1 - enough"));
        int shortPack(string id) => 0;
        var shortRows = ClothingRules.NeedRows(coat, id => bench(id) + shortPack(id));
        Assert.That(ClothingRules.NeedText(shortRows[0], 2), Is.EqualTo("Buckskin - need 3 - bench 2 + pack 0 - short by 1"));
    }

    [Test]
    public void TheWordsSayTheBenchAndThePack()
    {
        ClothingRules.Piece coat = ClothingRules.Pieces[0];
        Assert.That(ClothingRules.ReasonText(WorkshopRules.Gate.ShortOfStock, coat, default, id => 0, 0f),
                    Is.EqualTo("The bench and your pack are short 3 Buckskin, 1 Tanned Furs, 2 Sinew. Bring more: R puts it in the bench."));
        Assert.That(ClothingRules.ReasonText(WorkshopRules.Gate.Ok, coat, default, id => 9, 0f), Is.EqualTo("Takes the materials from the bench first, then your pack."));
        Assert.That(ClothingRules.BenchLine(false, default, 0f, false, default, id => id == "buckskin" ? 2 : 0),
                    Is.EqualTo("Bench holds: 2 Buckskin, 0 Tanned Furs, 0 Sinew, 0 Rawhide, 0 Leather"));
        Assert.That(ClothingRules.StockButtonText(WorkshopRules.StockGate.Can), Is.EqualTo("PUT MY MATERIALS IN THE BENCH"));
        Assert.That(ClothingRules.StockButtonText(WorkshopRules.StockGate.NothingCarried), Is.EqualTo("NOTHING TO PUT IN THE BENCH"));
        Assert.That(ClothingRules.StockButtonText(WorkshopRules.StockGate.BenchFull), Is.EqualTo("BENCH IS FULL OF WHAT YOU CARRY"));
        Assert.That(ClothingRules.KeyHint, Does.Contain("R stock"));
    }

    [Test]
    public void TheStockGateOverAnyList()
    {
        string[] ids = WorkshopRules.AllStockIds;
        Assert.That(WorkshopRules.StockStateOf(ids, id => 0, id => 0), Is.EqualTo(WorkshopRules.StockGate.NothingCarried));
        Assert.That(WorkshopRules.StockStateOf(ids, id => id == "sinew" ? 2 : 0, id => 0), Is.EqualTo(WorkshopRules.StockGate.Can));
        Assert.That(WorkshopRules.StockStateOf(ids, id => id == "sinew" ? 2 : 0, id => id == "sinew" ? 4 : 0), Is.EqualTo(WorkshopRules.StockGate.BenchFull));
        Assert.That(WorkshopRules.StockStateOf(ids, id => id == "sinew" || id == "logs" ? 2 : 0, id => id == "sinew" ? 4 : 0), Is.EqualTo(WorkshopRules.StockGate.Can));
    }

    [Test]
    public void TheLeatherworkScreenHasAStockButtonAndTheRKey()
    {
        Assert.That(ClothingRules.Resolve(ClothingRules.Key.Stock, 0, 3, 7), Is.EqualTo((ClothingRules.ActionKind.Stock, 3)));
        string ls = SourceText("Scripts/UI/LeatherworkScreen.cs");
        Assert.That(ls, Does.Contain("bench.StockFromPack(out string message);"));
        Assert.That(ls, Does.Contain("keyboard.rKey.wasPressedThisFrame"));
        Assert.That(ls, Does.Contain("bench.LeatherStockGateNow()"));
    }
}
