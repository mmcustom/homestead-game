using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

// Greenhouse slice G3: Glass Greenhouses and seedling benches (Mike, 2026-11-26: no Iron Bars, Nails instead; benches start transplants; the glass lasts for good).
public class GreenhouseG3Tests
{
    static string SourceText(string relative) => File.ReadAllText(Path.Combine(Application.dataPath, relative)).Replace("\r\n", "\n");

    static readonly GlassHouseRules.Size[] Sizes = { GlassHouseRules.Size.Small, GlassHouseRules.Size.Medium, GlassHouseRules.Size.Large };

    [Test]
    public void TheApprovedBedSquaresAndTheBenches()
    {
        Assert.That(GlassHouseRules.Squares(GlassHouseRules.Size.Small), Is.EqualTo(4));
        Assert.That(GlassHouseRules.Squares(GlassHouseRules.Size.Medium), Is.EqualTo(9));
        Assert.That(GlassHouseRules.Squares(GlassHouseRules.Size.Large), Is.EqualTo(30), "Greenhouses.md 3");
        Assert.That(GlassHouseRules.Benches(GlassHouseRules.Size.Small), Is.EqualTo(1));
        Assert.That(GlassHouseRules.Benches(GlassHouseRules.Size.Large), Is.EqualTo(3));
        Assert.That(GlassHouseRules.Name(GlassHouseRules.Size.Medium), Is.EqualTo("Medium Glass Greenhouse"));
        Assert.That(GlassHouseRules.WallHeight, Is.LessThan(GlassHouseRules.RidgeHeight(GlassHouseRules.Size.Small)));
    }

    [Test]
    public void SquaresAndBenchesFitInsideWithoutOverlapping()
    {
        foreach (GlassHouseRules.Size size in Sizes)
        {
            float half = GlassHouseRules.SquareSize / 2f;
            int n = GlassHouseRules.Squares(size);
            var squares = Enumerable.Range(0, n).Select(i => GlassHouseRules.SquareAt(size, i)).ToArray();
            foreach (var s in squares)
                Assert.That(GlassHouseRules.Inside(size, Mathf.Abs(s.x) + half, Mathf.Abs(s.z) + half, 0.001f), Is.True, $"{size} square at {s}");
            for (int i = 0; i < n; i++)
                for (int j = i + 1; j < n; j++)
                    Assert.That(Mathf.Abs(squares[i].x - squares[j].x) >= GlassHouseRules.SquareSize - 0.001f || Mathf.Abs(squares[i].z - squares[j].z) >= GlassHouseRules.SquareSize - 0.001f, Is.True, $"{size} squares {i} and {j}");
            float bw = GlassHouseRules.BenchWidth(size) / 2f, bd = GlassHouseRules.BenchDepth / 2f;
            for (int b = 0; b < GlassHouseRules.Benches(size); b++)
            {
                var at = GlassHouseRules.BenchAt(size, b);
                Assert.That(GlassHouseRules.Inside(size, Mathf.Abs(at.x) + bw, Mathf.Abs(at.z) + bd, 0.001f), Is.True, $"{size} bench {b} inside");
                foreach (var s in squares)
                    Assert.That(Mathf.Abs(s.x - at.x) >= half + bw - 0.001f || Mathf.Abs(s.z - at.z) >= half + bd - 0.001f, Is.True, $"{size} bench {b} clear of the squares");
            }
            Assert.That(squares.Min(s => s.z) - half - (-GlassHouseRules.Length(size) / 2f), Is.GreaterThanOrEqualTo(0.29f), $"{size}: a step in at the door");
        }
    }

    [Test]
    public void ItsMaterialsHaveNoIron()
    {
        Assert.That(GlassHouseRules.Lumber(GlassHouseRules.Size.Small), Is.EqualTo(16));
        Assert.That(GlassHouseRules.Stone(GlassHouseRules.Size.Medium), Is.EqualTo(18));
        Assert.That(GlassHouseRules.Panes(GlassHouseRules.Size.Large), Is.EqualTo(110));
        Assert.That(GlassHouseRules.Nails(GlassHouseRules.Size.Large), Is.EqualTo(4));
        Assert.That(GlassHouseRules.NailsId, Is.EqualTo(SmithyRules.NailsId));
        Assert.That(GlassHouseRules.SiteHelp(GlassHouseRules.Size.Small), Does.Not.Contain("Iron"), "Mike: no Iron Bars");
        Assert.That(GlassHouseRules.SiteHelp(GlassHouseRules.Size.Small), Does.Contain("16 Lumber, 10 Stone, 24 Glass Panes and 1 Box of Nails"));
        Assert.That(MarketCatalog.Table.Stock.Any(r => r.itemId == GlassHouseRules.PaneId && r.baseCents == 300), Is.True);
        MarketRow nails = MarketCatalog.Table.Stock.First(r => r.itemId == SmithyRules.NailsId);
        Assert.That(nails.baseCents, Is.EqualTo(4000L), "on the shelf at the homemade 2.5x");
        Assert.That(MarketCatalog.Table.SellBaseCents(SmithyRules.NailsId), Is.EqualTo(1600L), "and still bought back at the forged price");
    }

    [Test]
    public void GlassHoldsTheNightBetterThanPlastic()
    {
        Assert.That(GlassHouseRules.InsideC(-5f, 0f, false), Is.EqualTo(2f).Within(1e-4f), "about 12 F warmer at night");
        Assert.That(GlassHouseRules.InsideC(-5f, 0f, true), Is.EqualTo(-3.5f).Within(1e-4f));
        Assert.That(GlassHouseRules.InsideC(15f, 1f, false), Is.EqualTo(42f).Within(1e-4f), "shut on a sunny day it overheats");
        Assert.That(GlassHouseRules.ShutNightLiftC, Is.GreaterThan(HoopHouseRules.ShutNightLiftC));
        Assert.That(GlassHouseRules.ShutSunGainC, Is.LessThan(HoopHouseRules.ShutSunGainC));
    }

    [Test]
    public void TraysGiveTransplantsWithAHeadStart()
    {
        Assert.That(GlassHouseRules.Trays.Select(t => t.cropId), Is.EqualTo(new[] { "tomatoes", "lettuce", "kale", "squash" }));
        Assert.That(GlassHouseRules.TryTray("tomatoes", out GlassHouseRules.Tray tomato), Is.True);
        Assert.That(tomato.seedId, Is.EqualTo(GardenRules.TomatoSeedId));
        Assert.That(tomato.transplantId, Is.EqualTo("tomato_plant"), "a tray of Tomato Seed gives Tomato Plants, as the store sells");
        Assert.That(tomato.days, Is.EqualTo(GardenRules.TomatoSeedlingDays), "the weeks Tomato Seed sown outdoors lags a bought plant");
        Assert.That(tomato.transplants, Is.EqualTo(4 * GlassHouseRules.TraySquares), "four plants a square");
        foreach (string startId in GlassHouseRules.StartIds)
        {
            GlassHouseRules.Tray tray = GlassHouseRules.Trays.First(t => t.transplantId == startId);
            Assert.That(GardenRules.ForSeed(startId)?.id, Is.EqualTo(tray.cropId), startId + " plants its crop");
            GardenRules.Sowing sowing = GardenRules.Sowings.First(s => s.seedId == startId);
            Assert.That(sowing.startDays, Is.EqualTo(tray.days), startId + " goes in already grown its days on the bench");
            Assert.That(sowing.seeds, Is.EqualTo(1));
            Assert.That(tray.transplants, Is.EqualTo(GlassHouseRules.TraySquares));
        }
        Assert.That(GlassHouseRules.TryTray("potatoes", out _), Is.False, "potatoes, corn, beans and carrots go straight in the ground");
    }

    [Test]
    public void TheWords()
    {
        Assert.That(GlassHouseRules.Header(GlassHouseRules.Size.Small, false, 14f, 3f), Is.EqualTo("Small Glass Greenhouse - vents shut, 14 C inside (3 C outside)"));
        Assert.That(GlassHouseRules.VentsPrompt(false), Does.StartWith("Open the vents"));
        Assert.That(GlassHouseRules.BedLine(true, 18f), Is.EqualTo("in a glass greenhouse: vents open, 18 C inside"));
        Assert.That(GlassHouseRules.CloseVentsBanner(1, -4f), Is.EqualTo("Frost tonight (about -4 C) - shut the vents of the glass greenhouse (E at the door)"));
        Assert.That(GlassHouseRules.BenchHeader(null, 0f, 0f, 12f), Is.EqualTo("Seedling bench - empty (12 C)"));
        Assert.That(GlassHouseRules.BenchHeader("Kale Starts", 5f, 20f, 12f), Is.EqualTo("Seedling bench - Kale Starts: 15 days to go (12 C)"));
        Assert.That(GlassHouseRules.BenchHeader("Kale Starts", 20f, 20f, 12f), Does.Contain("ready to take up"));
        GlassHouseRules.TryTray("lettuce", out GlassHouseRules.Tray lettuce);
        Assert.That(GlassHouseRules.SowPrompt(lettuce), Is.EqualTo("Sow a tray of Lettuce Starts  (1 seed; 3 transplants in about 20 days)"));
        Assert.That(GlassHouseRules.TakePrompt(lettuce), Is.EqualTo("Take up the Lettuce Starts  (3)"));
    }

    [Test]
    public void TheWorldWiresItUp()
    {
        string wood = SourceText("Scripts/Managers/WoodManager.cs");
        Assert.That(wood, Does.Contain("HoopHouseLarge, GlassHouseSiteSmall, GlassHouseSiteMedium, GlassHouseSiteLarge, GlassHouseSmall, GlassHouseMedium, GlassHouseLarge, SeedlingBench, GreenhouseStove, PropaneHeater, WaterBarrels, HoseBib, HoseReel, HandPump, FuelPump, VentOpener }"), "appended");
        Assert.That(wood, Does.Contain("AddHousePile(house, PileKind.SeedlingBench, turn, terrain, x, z);"), "the benches come with the house");
        Assert.That(wood, Does.Contain("foreach (WoodPileState bench in BenchesIn(frame))\n            RemovePile(bench);"), "and go with it");
        string sites = SourceText("Scripts/Managers/WellsAndOuthouses.cs");
        Assert.That(sites, Does.Contain("case PileKind.GlassHouseSiteSmall: return PileKind.GlassHouseSmall;"));
        Assert.That(sites, Does.Contain("GlassHouseRules.LumberId, GlassHouseRules.StoneId, GlassHouseRules.PaneId, GlassHouseRules.NailsId"));
        string beds = SourceText("Scripts/World/GardenBeds.cs");
        Assert.That(beds, Does.Contain("GreenhouseBenches.Hour(wood, airC);"));
        Assert.That(beds, Does.Contain("WoodManager.IsGlassHouse(frame.kind) ? GlassHouseRules.InsideC(airC, sun, open)"));
        string placed = SourceText("Scripts/World/PlacedStructure.cs");
        Assert.That(placed, Does.Contain("case PileKind.GlassHouseLarge: GlassInteract(); break;"));
        Assert.That(placed, Does.Contain("case PileKind.SeedlingBench: GreenhouseBenches.Interact(state); break;"));
        string bench = SourceText("Scripts/World/GreenhouseBenches.cs");
        Assert.That(bench, Does.Contain("bench.jobHours += GardenRules.GrowthRate(crop, inside) / 24f;"), "grows at the inside temperature");
        Assert.That(bench, Does.Contain("Inventory.Player.Remove(sow.seedId, 1);"), "one seed a tray");
    }
}
