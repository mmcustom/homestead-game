using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

// Greenhouse slice G4: heat (Mike, 2026-11-26: built in a slot inside; the stove burns only while needed; the propane heater keeps frost off; hotbeds as proposed).
public class GreenhouseG4Tests
{
    static string SourceText(string relative) => File.ReadAllText(Path.Combine(Application.dataPath, relative)).Replace("\r\n", "\n");

    [Test]
    public void WhichHousesHaveRoomForWhat()
    {
        Assert.That(GreenhouseHeatRules.Fits(false, 0, GreenhouseHeatRules.Heat.Stove), Is.False, "the Small Hoop House is too small for a stove");
        Assert.That(GreenhouseHeatRules.Fits(false, 1, GreenhouseHeatRules.Heat.Stove), Is.True);
        Assert.That(GreenhouseHeatRules.Fits(true, 0, GreenhouseHeatRules.Heat.Propane), Is.True, "every Glass Greenhouse takes a heater");
        Assert.That(GreenhouseHeatRules.Fits(true, 0, GreenhouseHeatRules.Heat.Barrels), Is.False, "no room for barrels in the Small Glass Greenhouse");
        Assert.That(GreenhouseHeatRules.Fits(false, 0, GreenhouseHeatRules.Heat.Barrels), Is.True);
        Assert.That(GreenhouseHeatRules.HeaterSpot(false, 0, out _, out _), Is.False);
        Assert.That(GreenhouseHeatRules.BarrelSpot(true, 0, out _, out _, out _), Is.False);
    }

    // Each spot sits inside its house, off every bed square and bench, and clear of the doorway.
    [Test]
    public void EverySpotIsInsideAndClearOfTheBedsBenchesAndDoor()
    {
        for (int size = 0; size < 3; size++)
        {
            foreach (bool glass in new[] { false, true })
            {
                float w = glass ? GlassHouseRules.Width((GlassHouseRules.Size)size) : HoopHouseRules.Width((HoopHouseRules.Size)size);
                float l = glass ? GlassHouseRules.Length((GlassHouseRules.Size)size) : HoopHouseRules.Length((HoopHouseRules.Size)size);
                int squares = glass ? GlassHouseRules.Squares((GlassHouseRules.Size)size) : HoopHouseRules.Squares((HoopHouseRules.Size)size);
                var spots = new System.Collections.Generic.List<(float x, float z, float hx, float hz)>();
                if (GreenhouseHeatRules.HeaterSpot(glass, size, out float hx, out float hz))
                    spots.Add((hx, hz, 0.3f, 0.3f));
                if (GreenhouseHeatRules.BarrelSpot(glass, size, out float bx, out float bz, out bool along))
                    spots.Add((bx, bz, along ? 0.3f : 0.6f, along ? 0.6f : 0.3f));
                foreach (var s in spots)
                {
                    string what = $"{(glass ? "glass" : "hoop")} size {size} spot at ({s.x}, {s.z})";
                    Assert.That(Mathf.Abs(s.x) + s.hx, Is.LessThanOrEqualTo(w / 2f + 0.001f), what + " inside the side walls");
                    Assert.That(Mathf.Abs(s.z) + s.hz, Is.LessThanOrEqualTo(l / 2f + 0.001f), what + " inside the end walls");
                    for (int i = 0; i < squares; i++)
                    {
                        var q = glass ? GlassHouseRules.SquareAt((GlassHouseRules.Size)size, i) : HoopHouseRules.SquareAt((HoopHouseRules.Size)size, i);
                        Assert.That(Mathf.Abs(q.x - s.x) >= 0.6f + s.hx - 0.001f || Mathf.Abs(q.z - s.z) >= 0.6f + s.hz - 0.001f, Is.True, what + $" clear of square {i}");
                    }
                    if (glass)
                        for (int b = 0; b < GlassHouseRules.Benches((GlassHouseRules.Size)size); b++)
                        {
                            var bench = GlassHouseRules.BenchAt((GlassHouseRules.Size)size, b);
                            float bw = GlassHouseRules.BenchWidth((GlassHouseRules.Size)size) / 2f;
                            Assert.That(Mathf.Abs(bench.x - s.x) >= bw + s.hx - 0.001f || Mathf.Abs(bench.z - s.z) >= GlassHouseRules.BenchDepth / 2f + s.hz - 0.001f, Is.True, what + " clear of the bench");
                        }
                    Assert.That(s.z - s.hz > -l / 2f + 0.5f || Mathf.Abs(s.x) - s.hx >= 0.5f, Is.True, what + " clear of the doorway");
                }
            }
        }
    }

    [Test]
    public void TheStoveBurnsOnlyWhileItIsNeeded()
    {
        Assert.That(GreenhouseHeatRules.StoveBurns(true, 3, 2f), Is.True);
        Assert.That(GreenhouseHeatRules.StoveBurns(true, 3, 12f), Is.False, "warm enough: banked, no wood used");
        Assert.That(GreenhouseHeatRules.StoveBurns(true, 0, 2f), Is.False, "no wood");
        Assert.That(GreenhouseHeatRules.StoveBurns(false, 3, 2f), Is.False, "not lit");
        Assert.That(GreenhouseHeatRules.StoveFirewoodCap, Is.EqualTo(8), "a full load lasts a cold night");
        Assert.That(GreenhouseHeatRules.StoveGainC(0), Is.GreaterThan(GreenhouseHeatRules.StoveGainC(2)), "a small house warms more");
    }

    [Test]
    public void ThePropaneHeaterKeepsFrostOffAndBurnsOnlyWhatThatTakes()
    {
        Assert.That(GreenhouseHeatRules.PropaneHoldC, Is.EqualTo(5f));
        Assert.That(GreenhouseHeatRules.PropaneGainC(8f, 1), Is.EqualTo(0f), "above 5 C it idles");
        Assert.That(GreenhouseHeatRules.PropaneGainC(-3f, 1), Is.EqualTo(8f).Within(1e-4f));
        Assert.That(GreenhouseHeatRules.PropaneBurn(-3f, 1), Is.EqualTo(0.5f).Within(1e-4f), "half its output for half an hour's propane");
        Assert.That(GreenhouseHeatRules.PropaneGainC(-30f, 2), Is.EqualTo(GreenhouseHeatRules.PropaneMaxGainC(2)), "it can only do so much");
        Assert.That(GreenhouseHeatRules.PropaneBurn(-30f, 2), Is.EqualTo(1f).Within(1e-4f));
    }

    [Test]
    public void BarrelsCoolTheDayAndWarmTheNight()
    {
        Assert.That(GreenhouseHeatRules.BarrelEffectC(0f), Is.EqualTo(2.5f));
        Assert.That(GreenhouseHeatRules.BarrelEffectC(0.8f), Is.EqualTo(-2f));
        Assert.That(GreenhouseHeatRules.BarrelEffectC(0.3f), Is.EqualTo(0f));
    }

    [Test]
    public void HotbedsWarmTheSoilForFourWeeks()
    {
        Assert.That(GreenhouseHeatRules.HotbedC, Is.EqualTo(8f));
        Assert.That(GreenhouseHeatRules.HotbedActive(100, 100), Is.True);
        Assert.That(GreenhouseHeatRules.HotbedActive(100, 127), Is.True);
        Assert.That(GreenhouseHeatRules.HotbedActive(100, 128), Is.False, "spent after 28 days");
        Assert.That(GreenhouseHeatRules.HotbedDaysLeft(100, 110), Is.EqualTo(18));
        Assert.That(GardenRules.KeptFlags & GardenRules.FlagHotbed, Is.Not.EqualTo(0), "a harvest does not undo it");
        Assert.That(GreenhouseHeatRules.HotbedPrompt(3), Does.StartWith("Dig in a hotbed  (2 Fertilizer"));
    }

    [Test]
    public void TheShopAndTheWords()
    {
        Assert.That(MarketCatalog.Table.Stock.Any(r => r.itemId == GreenhouseHeatRules.StoveItemId && r.baseCents == 4000), Is.True);
        Assert.That(MarketCatalog.Table.Stock.Any(r => r.itemId == GreenhouseHeatRules.HeaterItemId && r.baseCents == 3500), Is.True);
        Assert.That(MarketCatalog.Table.Stock.Any(r => r.itemId == GreenhouseHeatRules.TankId && r.baseCents == 1200), Is.True);
        Assert.That(GreenhouseHeatRules.StoveHeader(true, false, 5), Is.EqualTo("Wood Stove - lit, banked (warm enough), 5 Firewood"));
        Assert.That(GreenhouseHeatRules.StovePrompt(false, 4), Is.EqualTo("Light it  (burns while the greenhouse is below 8 C)"));
        Assert.That(GreenhouseHeatRules.StoveLoadPrompt(6, 8), Is.EqualTo(""), "full");
        Assert.That(GreenhouseHeatRules.HeaterPrompt(false, true), Is.EqualTo("Turn it on  (keeps the greenhouse at 5 C or above)"));
        Assert.That(GreenhouseHeatRules.HeaterHeader(true, true, 1, 7.6f), Is.EqualTo("Propane Heater - heating; 1 full tank and 8 h in the one fitted"));
        Assert.That(GreenhouseHeatRules.Help(GreenhouseHeatRules.Heat.Barrels), Does.Contain("4 Logs"));
    }

    [Test]
    public void TheWorldWiresItUp()
    {
        string wood = SourceText("Scripts/Managers/WoodManager.cs");
        Assert.That(wood, Does.Contain("SeedlingBench, GreenhouseStove, PropaneHeater, WaterBarrels, HoseBib, HoseReel, HandPump, FuelPump, VentOpener, DeliveryPad, PoleBarnSiteSmall"), "appended");
        Assert.That(wood, Does.Contain("if (IsHouseFitting(kind))\n            return CanBuildHeatAt(kind, spot, out reason);"), "only in its spot");
        Assert.That(SourceText("Scripts/Player/PlacementMode.cs"), Does.Contain("SnapToHeatSlot("), "the ghost snaps to the spot");
        string beds = SourceText("Scripts/World/GardenBeds.cs");
        Assert.That(beds, Does.Contain("GreenhouseHeat.Hour(wood, airC);\n        // Glass Greenhouses' seedling benches (G3).\n        GreenhouseBenches.Hour(wood, airC);"), "heat before the benches and beds");
        Assert.That(beds, Does.Contain("return GreenhouseHeat.Apply(frame, inside, sun);"));
        Assert.That(beds, Does.Contain("air += GreenhouseHeatRules.HotbedC;"));
        string heat = SourceText("Scripts/World/GreenhouseHeat.cs");
        Assert.That(heat, Does.Contain("if (GreenhouseHeatRules.StoveBurns(true, wood_, inside))"), "one Firewood an hour only while needed");
        string placed = SourceText("Scripts/World/PlacedStructure.cs");
        Assert.That(placed, Does.Contain("ToolStatus.Flash(GreenhouseHeatRules.HeatersFirst, 3f);"), "a greenhouse with heat in it is not taken down");
    }
}
