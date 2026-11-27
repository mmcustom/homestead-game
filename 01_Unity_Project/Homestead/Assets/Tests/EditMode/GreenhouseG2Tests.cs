using System.IO;
using NUnit.Framework;
using UnityEngine;

// Greenhouse slice G2: Hoop Houses (Mike, 2026-11-26: a staged site; hoops bought or forged at the Smithy; beds made with the house; snow swept with E or it tears).
public class GreenhouseG2Tests
{
    static string SourceText(string relative) => File.ReadAllText(Path.Combine(Application.dataPath, relative)).Replace("\r\n", "\n");

    static readonly HoopHouseRules.Size[] Sizes = { HoopHouseRules.Size.Small, HoopHouseRules.Size.Medium, HoopHouseRules.Size.Large };

    [Test]
    public void ThreeSizesWithTheirBedSquares()
    {
        Assert.That(HoopHouseRules.Squares(HoopHouseRules.Size.Small), Is.EqualTo(8));
        Assert.That(HoopHouseRules.Squares(HoopHouseRules.Size.Medium), Is.EqualTo(16));
        Assert.That(HoopHouseRules.Squares(HoopHouseRules.Size.Large), Is.EqualTo(36), "the bed squares Mike approved (Greenhouses.md 3)");
        Assert.That(HoopHouseRules.Name(HoopHouseRules.Size.Large), Is.EqualTo("Large Hoop House"));
        Assert.That(HoopHouseRules.SiteName(HoopHouseRules.Size.Small), Is.EqualTo("Small Hoop House Site"));
        Assert.That(HoopHouseRules.Height(HoopHouseRules.Size.Small), Is.GreaterThan(1.9f), "you can walk in");
    }

    [Test]
    public void EveryBedSquareFitsInsideAndNoneOverlap()
    {
        foreach (HoopHouseRules.Size size in Sizes)
        {
            int n = HoopHouseRules.Squares(size);
            var at = new (float x, float z)[n];
            for (int i = 0; i < n; i++)
            {
                at[i] = HoopHouseRules.SquareAt(size, i);
                float half = HoopHouseRules.SquareSize / 2f;
                Assert.That(HoopHouseRules.Inside(size, Mathf.Abs(at[i].x) + half, Mathf.Abs(at[i].z) + half, 0.001f), Is.True, $"{size} square {i} inside the walls");
            }
            for (int i = 0; i < n; i++)
                for (int j = i + 1; j < n; j++)
                    Assert.That(Mathf.Abs(at[i].x - at[j].x) >= HoopHouseRules.SquareSize - 0.001f || Mathf.Abs(at[i].z - at[j].z) >= HoopHouseRules.SquareSize - 0.001f, Is.True, $"{size} squares {i} and {j} overlap");
        }
    }

    [Test]
    public void TheDoorOpensOntoAPath()
    {
        // The Small and Medium houses have two rows with the path between them in line with the door; the Large one's middle row is in line with the door,
        // so it keeps an aisle across just inside.
        Assert.That(Mathf.Abs(HoopHouseRules.RowX(HoopHouseRules.Size.Small, 0)) - HoopHouseRules.SquareSize / 2f, Is.GreaterThanOrEqualTo(0.29f));
        Assert.That(Mathf.Abs(HoopHouseRules.RowX(HoopHouseRules.Size.Medium, 0)) - HoopHouseRules.SquareSize / 2f, Is.GreaterThanOrEqualTo(HoopHouseRules.DoorWidth / 2f));
        float frontOfMiddleRow = HoopHouseRules.SquareAt(HoopHouseRules.Size.Large, HoopHouseRules.PerRow(HoopHouseRules.Size.Large)).z - HoopHouseRules.SquareSize / 2f;
        Assert.That(frontOfMiddleRow - (-HoopHouseRules.Length(HoopHouseRules.Size.Large) / 2f), Is.GreaterThanOrEqualTo(0.85f), "the Large house's aisle inside the door");
    }

    [Test]
    public void ItsMaterials()
    {
        Assert.That(HoopHouseRules.Hoops(HoopHouseRules.Size.Small), Is.EqualTo(6), "one every 4 ft, one at each end");
        Assert.That(HoopHouseRules.Hoops(HoopHouseRules.Size.Medium), Is.EqualTo(9));
        Assert.That(HoopHouseRules.Hoops(HoopHouseRules.Size.Large), Is.EqualTo(13));
        Assert.That(HoopHouseRules.Sheets(HoopHouseRules.Size.Large), Is.EqualTo(4));
        Assert.That(HoopHouseRules.HoopStoreCents, Is.EqualTo(1200L));
        Assert.That(HoopHouseRules.PlasticStoreCents, Is.EqualTo(2000L));
        Assert.That(System.Linq.Enumerable.Any(MarketCatalog.Table.Stock, r => r.itemId == HoopHouseRules.HoopId), Is.True);
        Assert.That(System.Linq.Enumerable.Any(MarketCatalog.Table.Stock, r => r.itemId == HoopHouseRules.PlasticId), Is.True);
        Assert.That(SmithyRules.TryFind(HoopHouseRules.HoopId, out SmithyRules.Recipe forged), Is.True, "or forged at the Smithy");
        Assert.That(forged.bars, Is.EqualTo(1));
        Assert.That(forged.OwnUse, Is.True);
    }

    [Test]
    public void SidesDownItHoldsTheNightWarmthAndTheSunHeatsIt()
    {
        Assert.That(HoopHouseRules.InsideC(-3f, 0f, false, false), Is.EqualTo(2.5f).Within(1e-4f), "about 10 F warmer at night");
        Assert.That(HoopHouseRules.InsideC(-3f, 0f, true, false), Is.EqualTo(-2f).Within(1e-4f), "sides up: close to the air");
        Assert.That(HoopHouseRules.InsideC(15f, 1f, false, false), Is.EqualTo(42.5f).Within(1e-4f), "shut in full sun it overheats");
        Assert.That(HoopHouseRules.InsideC(15f, 1f, true, false), Is.EqualTo(19f).Within(1e-4f));
        Assert.That(HoopHouseRules.InsideC(15f, 1f, false, true), Is.EqualTo(15f), "torn: no cover at all");
        Assert.That(HoopHouseRules.RainShare(false), Is.EqualTo(0f), "no rain inside");
        Assert.That(HoopHouseRules.RainShare(true), Is.EqualTo(1f));
        Assert.That(HoopHouseRules.ShutSunGainC, Is.LessThan(ColdFrameRules.ShutSunGainC), "a big tunnel heats a little slower than a small glass box");
    }

    [Test]
    public void SnowBuildsMeltsAndTears()
    {
        float snow = 0f;
        for (int h = 0; h < 7; h++)
            snow = HoopHouseRules.SnowAfterHour(snow, true, -4f);
        Assert.That(snow, Is.EqualTo(14f));
        Assert.That(snow, Is.LessThan(HoopHouseRules.WarnCm));
        snow = HoopHouseRules.SnowAfterHour(snow, true, -4f);
        Assert.That(snow, Is.GreaterThanOrEqualTo(HoopHouseRules.WarnCm), "the eighth hour of snow brings the warning");
        Assert.That(HoopHouseRules.SnowAfterHour(snow, false, -2f), Is.EqualTo(snow), "no melting below freezing");
        Assert.That(HoopHouseRules.SnowAfterHour(snow, false, 4f), Is.EqualTo(snow - 2f).Within(1e-4f), "it melts above freezing");
        Assert.That(HoopHouseRules.SnowAfterHour(1f, false, 10f), Is.EqualTo(0f));
        Assert.That(HoopHouseRules.Tears(29.9f), Is.False);
        Assert.That(HoopHouseRules.Tears(HoopHouseRules.TearCm), Is.True);
        Assert.That(HoopHouseRules.SweepMinutes, Is.EqualTo(60), "an hour's work");
    }

    [Test]
    public void ThePlasticLastsFourYears()
    {
        const int year = 120;
        Assert.That(HoopHouseRules.WornOut(10, 10 + 4 * year - 1, year), Is.False);
        Assert.That(HoopHouseRules.WornOut(10, 10 + 4 * year, year), Is.True);
        Assert.That(HoopHouseRules.YearsLeft(0, year + 5, year), Is.EqualTo(3));
        Assert.That(HoopHouseRules.WornOut(0, 99999, 0), Is.False, "no calendar: never");
    }

    [Test]
    public void TheWords()
    {
        Assert.That(HoopHouseRules.Header(HoopHouseRules.Size.Small, false, false, 12f, 6f, 0f, 4), Is.EqualTo("Small Hoop House - sides down, 12 C inside (6 C outside); plastic good for 4 more years"));
        Assert.That(HoopHouseRules.Header(HoopHouseRules.Size.Small, true, false, 8f, 6f, 16f, 1), Is.EqualTo("Small Hoop House - sides rolled up, 8 C inside (6 C outside), 16 cm of snow on it; plastic good for 1 more year"));
        Assert.That(HoopHouseRules.Header(HoopHouseRules.Size.Medium, false, true, 0f, 0f, 0f, 0), Does.Contain("the plastic is torn"));
        Assert.That(HoopHouseRules.FitPrompt(HoopHouseRules.Size.Medium, 2), Is.EqualTo("Fit new plastic  (2 Greenhouse Plastic)"));
        Assert.That(HoopHouseRules.FitPrompt(HoopHouseRules.Size.Large, 1), Does.StartWith("Needs 4 Greenhouse Plastic"));
        Assert.That(HoopHouseRules.SweepPrompt(17.4f), Is.EqualTo("Sweep the snow off  (17 cm; about an hour)"));
        Assert.That(HoopHouseRules.CloseSidesBanner(1, -2f), Is.EqualTo("Frost tonight (about -2 C) - roll down the sides of the hoop house (E at the door)"));
        Assert.That(HoopHouseRules.CloseSidesBanner(2, -2f), Does.Contain("of 2 hoop houses"));
        Assert.That(HoopHouseRules.BedLine(false, false, 9f), Is.EqualTo("in a hoop house: sides down, 9 C inside"));
        Assert.That(HoopHouseRules.SiteHelp(HoopHouseRules.Size.Small), Does.Contain("10 by 20 ft"));
        Assert.That(HoopHouseRules.SiteHelp(HoopHouseRules.Size.Large), Does.Contain("20 Lumber, 13 Greenhouse Hoops, 4 Greenhouse Plastic and 4 Cordage"));
    }

    [Test]
    public void TheWorldWiresItUp()
    {
        string wood = SourceText("Scripts/Managers/WoodManager.cs");
        Assert.That(wood, Does.Contain("ColdFrameLarge, HoopHouseSiteSmall, HoopHouseSiteMedium, HoopHouseSiteLarge, HoopHouseSmall, HoopHouseMedium, HoopHouseLarge, GlassHouseSiteSmall, GlassHouseSiteMedium, GlassHouseSiteLarge, GlassHouseSmall, GlassHouseMedium, GlassHouseLarge, SeedlingBench, GreenhouseStove, PropaneHeater, WaterBarrels, HoseBib, HoseReel, HandPump, FuelPump, VentOpener }"), "appended, so saves keep their numbers");
        Assert.That(wood, Does.Contain("return ColdFrameOf(bed) ?? HouseAt(bed.position);"), "a bed's cover is its frame or its house");
        Assert.That(wood, Does.Contain("if (InsideAnyHouse(spot.point, 0.3f))"), "nothing else is built inside");
        string sites = SourceText("Scripts/Managers/WellsAndOuthouses.cs");
        Assert.That(sites, Does.Contain("case PileKind.HoopHouseSiteSmall: return PileKind.HoopHouseSmall;"), "a staged site");
        Assert.That(sites, Does.Contain("WoodManager.Instance.BuildHouseBeds(site);"), "the beds come with the house");
        string beds = SourceText("Scripts/World/GardenBeds.cs");
        Assert.That(beds, Does.Contain("HoopHouseHour(wood, airC);"));
        Assert.That(beds, Does.Contain("if (frame != null && WoodManager.IsHoopHouse(frame.kind) && WoodManager.HoopIsTorn(frame))\n                frame = null;"), "torn: as outdoors");
        Assert.That(beds, Does.Contain("bool framed = cover != null && WoodManager.IsColdFrame(cover.kind);"), "any crop in a hoop house");
        string placed = SourceText("Scripts/World/PlacedStructure.cs");
        Assert.That(placed, Does.Contain("case PileKind.HoopHouseLarge: HoopInteract(); break;"));
        Assert.That(placed, Does.Contain("TimeManager.Instance.AdvanceMinutes(HoopHouseRules.SweepMinutes);"), "sweeping takes an hour");
        Assert.That(placed, Does.Contain("&& !WoodManager.IsWalkIn(state.kind))"), "its walls are solid");
    }
}
