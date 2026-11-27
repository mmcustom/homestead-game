using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

// Greenhouse slice G6: automatic venting (Mike, 2026-11-27: the wax Vent Opener fits all three kinds; the Thermostat Controller waits for power; built in a spot).
public class GreenhouseG6Tests
{
    static string SourceText(string relative) => File.ReadAllText(Path.Combine(Application.dataPath, relative)).Replace("\r\n", "\n");

    [Test]
    public void ItOpensWarmAndShutsCoolWithAGapBetween()
    {
        Assert.That(VentRules.OpenC, Is.EqualTo(24f));
        Assert.That(VentRules.ShutC, Is.EqualTo(18f));
        Assert.That(VentRules.OpenAfter(false, 30f), Is.True, "hot: opens");
        Assert.That(VentRules.OpenAfter(true, 10f), Is.False, "cold: shuts");
        Assert.That(VentRules.OpenAfter(true, 21f), Is.True, "in the gap it stays open");
        Assert.That(VentRules.OpenAfter(false, 21f), Is.False, "... or shut");
        Assert.That(VentRules.OpenAfter(false, 24f), Is.True);
        Assert.That(VentRules.OpenAfter(true, 18f), Is.False);
    }

    [Test]
    public void ItsSpotIsClearOfTheDoorTheBibAndTheHeat()
    {
        foreach (bool glass in new[] { false, true })
            for (int size = 0; size < 3; size++)
            {
                float length = glass ? GlassHouseRules.Length((GlassHouseRules.Size)size) : HoopHouseRules.Length((HoopHouseRules.Size)size);
                float width = glass ? GlassHouseRules.Width((GlassHouseRules.Size)size) : HoopHouseRules.Width((HoopHouseRules.Size)size);
                (float x, float z) = VentRules.HouseSpot(length);
                Assert.That(z, Is.GreaterThan(-length / 2f).And.LessThan(-length / 2f + 0.2f), "on the inside of the front wall");
                Assert.That(Mathf.Abs(x), Is.GreaterThan(GlassHouseRules.DoorWidth / 2f + 0.1f), "clear of the doorway");
                Assert.That(Mathf.Abs(x), Is.LessThan(width / 2f), "inside the house");
                (float bx, float bz) = WateringRules.BibSpot(length);
                Assert.That(Vector2.Distance(new Vector2(x, z), new Vector2(bx, bz)), Is.GreaterThan(1f), "across the door from the bib");
                if (GreenhouseHeatRules.HeaterSpot(glass, size, out float hx, out float hz))
                    Assert.That(Vector2.Distance(new Vector2(x, z), new Vector2(hx, hz)), Is.GreaterThan(0.8f), $"clear of the heater ({glass} {size})");
                if (GreenhouseHeatRules.BarrelSpot(glass, size, out float rx, out float rz, out _))
                    Assert.That(Vector2.Distance(new Vector2(x, z), new Vector2(rx, rz)), Is.GreaterThan(0.8f), $"clear of the barrels ({glass} {size})");
            }
        (float fx, float fz) = VentRules.FrameSpot();
        Assert.That(fx, Is.EqualTo(0f));
        Assert.That(fz, Is.GreaterThan(0f).And.LessThan(ColdFrameRules.Depth / 2f), "inside a cold frame's back wall");
    }

    [Test]
    public void TheShopAndTheWords()
    {
        Assert.That(MarketCatalog.Table.Stock.Any(r => r.itemId == VentRules.ItemId && r.baseCents == 3500L), Is.True);
        Assert.That(VentRules.Header("ColdFrame", true), Is.EqualTo("Vent Opener - opens the lid above 24 C and shuts it below 18 C (open now)"));
        Assert.That(VentRules.Header("HoopHouse", false), Is.EqualTo("Vent Opener - opens the sides above 24 C and shuts them below 18 C (shut now)"));
        Assert.That(VentRules.Header("GlassHouse", false), Does.StartWith("Vent Opener - opens the vents"));
        Assert.That(VentRules.Help(), Does.Contain("needs no power"));
        Assert.That(GreenhouseHeatRules.HeatersFirst, Does.Contain("vent opener"));
    }

    [Test]
    public void TheWorldWiresItUp()
    {
        string wood = SourceText("Scripts/Managers/WoodManager.cs");
        Assert.That(wood, Does.Contain("HandPump, FuelPump, VentOpener, DeliveryPad, PoleBarnSiteSmall"), "appended");
        Assert.That(wood, Does.Contain("IsWalkIn(host) || (fitting == PileKind.VentOpener && IsColdFrame(host))"), "a cold frame takes one too");
        Assert.That(wood, Does.Contain("kind == PileKind.VentOpener ? 3 : 0"), "its own spot");
        string beds = SourceText("Scripts/World/GardenBeds.cs");
        Assert.That(beds, Does.Contain("VentOpeners.Hour(wood, airC);\n        // Greenhouse stoves"), "before the heat decides");
        string openers = SourceText("Scripts/World/VentOpeners.cs");
        Assert.That(openers, Does.Contain("VentRules.OpenAfter(open, GardenBeds.InsideIf(host, airC, false))"), "it judges by what the inside would reach shut");
        Assert.That(openers, Does.Contain("HoopIsTorn(host)"), "no plastic, nothing to roll");
        Assert.That(SourceText("Scripts/World/PlacedStructure.cs"), Does.Contain("ToolStatus.Flash(VentRules.OpenerFirst, 3f);"), "a cold frame comes down after its opener");
    }
}
