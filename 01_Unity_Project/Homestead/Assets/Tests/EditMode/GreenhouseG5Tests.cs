using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

// Greenhouse slice G5: watering (Mike, 2026-11-27: outdoor beds too; a Hose Bib in each greenhouse and a Hose Reel outdoors; Hand Pump, drip timer and Fuel Pump;
// a banner when it fails, and the prompt says why).
public class GreenhouseG5Tests
{
    static string SourceText(string relative) => File.ReadAllText(Path.Combine(Application.dataPath, relative)).Replace("\r\n", "\n");

    [Test]
    public void TheBibSitsInsideTheFrontWallBesideTheDoor()
    {
        foreach (float length in new[] { HoopHouseRules.Length(HoopHouseRules.Size.Small), GlassHouseRules.Length(GlassHouseRules.Size.Small), HoopHouseRules.Length(HoopHouseRules.Size.Large) })
        {
            (float x, float z) = WateringRules.BibSpot(length);
            Assert.That(z, Is.GreaterThan(-length / 2f), "inside the front wall");
            Assert.That(z, Is.LessThan(-length / 2f + 0.2f), "on it");
            Assert.That(Mathf.Abs(x), Is.GreaterThan(GlassHouseRules.DoorWidth / 2f + 0.1f), "clear of the doorway");
            Assert.That(Mathf.Abs(x), Is.LessThan(HoopHouseRules.Width(HoopHouseRules.Size.Small) / 2f), "inside even the smallest house");
        }
    }

    [Test]
    public void ReachAndThePumps()
    {
        Assert.That(WateringRules.SourceReachMetres, Is.EqualTo(15f));
        Assert.That(WateringRules.ReelReachMetres, Is.EqualTo(12f));
        Assert.That(WateringRules.Pumped(0), Is.EqualTo(20), "a stroke");
        Assert.That(WateringRules.Pumped(190), Is.EqualTo(10), "up to the tank's 200 L");
        Assert.That(WateringRules.Pumped(200), Is.EqualTo(0));
        Assert.That(WateringRules.FuelPumpLitres(3, 150), Is.EqualTo(1350), "three cans and what is left of the one burning");
        Assert.That(WateringRules.DripHour, Is.EqualTo(6));
    }

    [Test]
    public void TheShop()
    {
        foreach ((string id, long cents) in new[] { (WateringRules.HoseId, 800L), (WateringRules.DripKitId, 1500L), (WateringRules.HandPumpId, 3000L), (WateringRules.FuelPumpId, 9000L), (WateringRules.GasolineId, 300L) })
            Assert.That(MarketCatalog.Table.Stock.Any(r => r.itemId == id && r.baseCents == cents), Is.True, id);
    }

    [Test]
    public void TheWordsSayWhy()
    {
        Assert.That(WateringRules.WaterPrompt(4, 60, "the Water Barrel 6 m away"), Is.EqualTo("Water the 4 beds  (60 L from the Water Barrel 6 m away)"));
        Assert.That(WateringRules.WaterPrompt(1, 0, "x"), Is.EqualTo("The 1 bed is soaked"));
        Assert.That(WateringRules.NoSource, Is.EqualTo("No water: no barrel or pump within 15 m"));
        Assert.That(WateringRules.Empty("Rain Barrel"), Is.EqualTo("No water: the Rain Barrel is empty"));
        Assert.That(WateringRules.Empty("Fuel Pump"), Is.EqualTo("No water: the Fuel Pump is out of Gasoline"));
        Assert.That(WateringRules.DripFailBanner("a greenhouse", "the Water Barrel is empty"), Is.EqualTo("Drip lines at a greenhouse went dry this morning - the Water Barrel is empty"));
        Assert.That(WateringRules.FittingHeader("Hose Reel", "the Rain Barrel 9 m away (80 L)", true), Is.EqualTo("Hose Reel - the Rain Barrel 9 m away (80 L); drip timer waters at 6 a.m."));
        Assert.That(WateringRules.PumpPrompt(120), Is.EqualTo("Pump  (+20 L)"));
        Assert.That(WateringRules.FuelPumpHeader(2, 800), Is.EqualTo("Fuel Pump - 2 cans of Gasoline (about 800 L of water to give)"));
        Assert.That(WateringRules.Help("HandPump"), Does.Contain("beside a Well"));
    }

    [Test]
    public void TheWorldWiresItUp()
    {
        string wood = SourceText("Scripts/Managers/WoodManager.cs");
        Assert.That(wood, Does.Contain("WaterBarrels, HoseBib, HoseReel, HandPump, FuelPump, VentOpener, DeliveryPad, PoleBarnSiteSmall"), "appended");
        Assert.That(wood, Does.Contain("public static bool IsHouseFitting(PileKind kind) => IsHeatKind(kind) || kind == PileKind.HoseBib || kind == PileKind.VentOpener;"), "the bib snaps into a greenhouse like the stove");
        Assert.That(wood, Does.Contain("if (WateringFittings.IsPump(kind) && !WateringFittings.PumpSiteOk(kind, spot.point))"), "pumps on the bank");
        string fittings = SourceText("Scripts/World/WateringFittings.cs");
        Assert.That(fittings, Does.Contain("(bed.cropFlags & GardenRules.FlagInFrame) == 0"), "a reel waters the beds not under cover");
        Assert.That(fittings, Does.Contain("TimeManager.Instance.Hour != WateringRules.DripHour"), "the drip timer runs each morning");
        Assert.That(fittings, Does.Contain("ToolStatus.Banner(fail, 10f);"), "a banner when it fails");
        Assert.That(SourceText("Scripts/World/GardenBeds.cs"), Does.Contain("WateringFittings.Hour(wood);"));
    }
}
