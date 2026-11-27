using System.IO;
using NUnit.Framework;
using UnityEngine;

// Greenhouse slice G1: Cold Frames (Mike, 2026-11-26: its own build with soil inside; bought sashes and Lumber; stress, then harm; yes, banners).
public class GreenhouseG1Tests
{
    static string SourceText(string relative) => File.ReadAllText(Path.Combine(Application.dataPath, relative)).Replace("\r\n", "\n");

    [Test]
    public void ThreeSizesOfOneTwoAndThreeSquares()
    {
        Assert.That(ColdFrameRules.Squares(ColdFrameRules.Size.Small), Is.EqualTo(1));
        Assert.That(ColdFrameRules.Squares(ColdFrameRules.Size.Medium), Is.EqualTo(2));
        Assert.That(ColdFrameRules.Squares(ColdFrameRules.Size.Large), Is.EqualTo(3));
        Assert.That(ColdFrameRules.Name(ColdFrameRules.Size.Medium), Is.EqualTo("Medium Cold Frame"));
        Assert.That(ColdFrameRules.Sashes(ColdFrameRules.Size.Large), Is.EqualTo(6), "two sashes a square");
        Assert.That(ColdFrameRules.Lumber(ColdFrameRules.Size.Small), Is.EqualTo(2));
        Assert.That(ColdFrameRules.SquareOffset(ColdFrameRules.Size.Small, 0), Is.EqualTo(0f));
        Assert.That(ColdFrameRules.SquareOffset(ColdFrameRules.Size.Large, 0), Is.EqualTo(-1.2f).Within(1e-4f), "in a row, centred on the frame");
        Assert.That(ColdFrameRules.SquareOffset(ColdFrameRules.Size.Medium, 1), Is.EqualTo(0.6f).Within(1e-4f));
        Assert.That(ColdFrameRules.BackHeight, Is.GreaterThan(ColdFrameRules.FrontHeight), "the lid slopes to the sun");
    }

    [Test]
    public void TheSashIsBoughtAtTheTradingPost()
    {
        Assert.That(ColdFrameRules.SashId, Is.EqualTo("window_sash"));
        Assert.That(ColdFrameRules.SashStoreCents, Is.EqualTo(800L));
        Assert.That(System.Linq.Enumerable.Any(MarketCatalog.Table.Stock, r => r.itemId == "window_sash"), Is.True);
    }

    [Test]
    public void ShutItHoldsTheNightWarmthAndTheSunHeatsItFast()
    {
        Assert.That(ColdFrameRules.Inside(-2f, 0f, false), Is.EqualTo(3.5f).Within(1e-4f), "about 10 F warmer at night, lid shut");
        Assert.That(ColdFrameRules.Inside(-2f, 0f, true), Is.EqualTo(-0.5f).Within(1e-4f), "propped open: close to the air");
        Assert.That(ColdFrameRules.Inside(20f, 1f, false), Is.EqualTo(50.5f).Within(1e-4f), "a shut frame in full sun gets very hot");
        Assert.That(ColdFrameRules.Inside(20f, 1f, true), Is.EqualTo(25.5f).Within(1e-4f));
        Assert.That(ColdFrameRules.RainShare(false), Is.EqualTo(0f), "no rain with the lid shut");
        Assert.That(ColdFrameRules.RainShare(true), Is.EqualTo(0.5f));
    }

    [Test]
    public void TheSunFollowsTheDayTheSeasonAndTheSky()
    {
        float summer = ColdFrameRules.NoonSin(1), winter = ColdFrameRules.NoonSin(3);
        Assert.That(summer, Is.GreaterThan(winter));
        Assert.That(ColdFrameRules.Sun(3f, 6f, 20f, summer, 1f), Is.EqualTo(0f), "night");
        Assert.That(ColdFrameRules.Sun(13f, 6f, 20f, summer, 1f), Is.EqualTo(summer).Within(1e-3f), "midday, clear");
        Assert.That(ColdFrameRules.Sun(13f, 6f, 20f, summer, 0.3f), Is.LessThan(ColdFrameRules.Sun(13f, 6f, 20f, summer, 1f)), "cloud cuts it");
        Assert.That(ColdFrameRules.Sun(8f, 6f, 20f, summer, 1f), Is.LessThan(ColdFrameRules.Sun(13f, 6f, 20f, summer, 1f)), "morning is weaker");
    }

    [Test]
    public void StressThenHarm()
    {
        Assert.That(ColdFrameRules.HeatDamage(27f, 30f), Is.EqualTo(0f), "past the limit but inside the margin: stalled only");
        Assert.That(ColdFrameRules.HeatDamage(27f, 31f), Is.EqualTo(0f));
        Assert.That(ColdFrameRules.HeatDamage(27f, 36f), Is.EqualTo(0.03f + 0.02f * 5f).Within(1e-4f), "harmed, more the hotter it is");
    }

    [Test]
    public void OnlyLowCropsFitUnderTheLid()
    {
        foreach (var crop in new[] { "lettuce", "kale", "carrots", "beans" })
            Assert.That(ColdFrameRules.Fits(crop), Is.True, crop);
        foreach (var crop in new[] { "tomatoes", "corn", "squash", "potatoes" })
            Assert.That(ColdFrameRules.Fits(crop), Is.False, crop);
        Assert.That(ColdFrameRules.TooTall("Corn"), Does.StartWith("Corn grow too tall for a cold frame"));
    }

    [Test]
    public void TheBannersAndWords()
    {
        Assert.That(ColdFrameRules.OverheatBanner(41.4f), Is.EqualTo("A cold frame is overheating (41 C inside) - prop the lid open (E on its front)"));
        Assert.That(ColdFrameRules.CloseLidsBanner(2, -3f), Is.EqualTo("Frost tonight (about -3 C) - close the cold frame lids (E on the front)"));
        Assert.That(ColdFrameRules.CloseLidsBanner(1, -1f), Does.EndWith("close the cold frame lid (E on the front)"));
        Assert.That(ColdFrameRules.WarnEveryHours, Is.EqualTo(3));
        Assert.That(ColdFrameRules.Header(ColdFrameRules.Size.Small, true, 18f, 12f), Is.EqualTo("Small Cold Frame - lid propped open, 18 C inside (12 C outside)"));
        Assert.That(ColdFrameRules.BedLine(false, 9f), Is.EqualTo("in a cold frame: lid shut, 9 C inside"));
        Assert.That(ColdFrameRules.Help(ColdFrameRules.Size.Medium), Does.Contain("4 Window Sashes (Trading Post) and 4 Lumber"));
        Assert.That(ColdFrameRules.Help(ColdFrameRules.Size.Small), Does.Contain("1 bed square of soil"));
    }

    [Test]
    public void FramedBedsKeepTheirMarkThroughAHarvest()
    {
        Assert.That(GardenRules.FlagInFrame, Is.EqualTo(128));
        Assert.That(GardenRules.KeptFlags & GardenRules.FlagInFrame, Is.Not.EqualTo(0));
    }

    [Test]
    public void TheWorldWiresItUp()
    {
        string wood = SourceText("Scripts/Managers/WoodManager.cs");
        Assert.That(wood, Does.Contain("FenceGate, ColdFrameSmall, ColdFrameMedium, ColdFrameLarge, HoopHouseSiteSmall, HoopHouseSiteMedium, HoopHouseSiteLarge, HoopHouseSmall, HoopHouseMedium, HoopHouseLarge, GlassHouseSiteSmall, GlassHouseSiteMedium, GlassHouseSiteLarge, GlassHouseSmall, GlassHouseMedium, GlassHouseLarge, SeedlingBench, GreenhouseStove, PropaneHeater, WaterBarrels, HoseBib, HoseReel, HandPump, FuelPump, VentOpener, DeliveryPad, PoleBarnSiteSmall"), "appended, so saves keep their numbers");
        Assert.That(wood, Does.Contain("BuildFrameBeds(pile);"), "building the frame makes its beds");
        string placed = SourceText("Scripts/World/PlacedStructure.cs");
        Assert.That(placed, Does.Contain("ColdFrameVisuals.Build(parent, state, pieces, out boxCenter, out boxSize)"));
        Assert.That(placed, Does.Contain("ToggleFrameLid(); break;"), "E on the front props the lid");
        Assert.That(placed, Does.Contain("ToolStatus.Flash(WoodManager.IsHoopHouse(state.kind) ? HoopHouseRules.NotEmpty : WoodManager.IsGlassHouse(state.kind) ? GlassHouseRules.NotEmpty : ColdFrameRules.NotEmpty, 3f);"), "a frame with crops is not taken down");
        Assert.That(placed, Does.Contain("WoodManager.Instance.RemoveFrameBeds(state);"));
        string beds = SourceText("Scripts/World/GardenBeds.cs");
        Assert.That(beds, Does.Contain("rain = rainMm * CoverRainShare(frame);"));
        Assert.That(beds, Does.Contain("ColdFrameRules.HeatDamage(crop.heatC, air)"));
        Assert.That(beds, Does.Contain("ColdFrameRules.Fits(sowing.crop.id)"), "only low crops are offered");
        Assert.That(SourceText("Scripts/World/ColdFrameVisuals.cs"), Does.Contain("Quaternion.Euler(open ? OpenDegrees : 0f, 0f, 0f)"), "the art's sashes swing open");
    }
}
