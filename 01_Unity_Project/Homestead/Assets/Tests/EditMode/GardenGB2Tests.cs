using System.IO;
using NUnit.Framework;
using UnityEngine;

// Gardening slice GB2 (Gardening_Basics.md section 9b, Mike 2026-11-25): soil fertility over 3 crops with Fertilizer to put it back, raised beds (warmer, drained),
// the Row Cover (about 3 C of frost kept off, a little slower in the heat), and the 6 pm frost alert.
public class GardenGB2Tests
{
    static string SourceText(string relative) => File.ReadAllText(Path.Combine(Application.dataPath, relative)).Replace("\r\n", "\n");

    static GardenRules.Crop Crop(string id) => GardenRules.Find(id);

    [Test]
    public void ABedWearsOutOverThreeCropsAndFourFertilizerBringItBack()
    {
        float spent = 0f;
        for (int i = 0; i < 3; i++)
            spent = GardenRules.AfterCrop(Crop("tomatoes"), spent);
        Assert.That(spent, Is.EqualTo(1f).Within(0.001f), "Mike: 3 crops before it runs low");
        Assert.That(GardenRules.YieldFactor(spent), Is.EqualTo(0.75f).Within(0.001f), "a spent bed still gives 75%");
        Assert.That(GardenRules.PickYield(Crop("potatoes"), false, 0f, spent), Is.EqualTo(27), "36 potatoes from a rich bed, 27 from a spent one");
        Assert.That(GardenRules.PickYield(Crop("potatoes"), false, 0f), Is.EqualTo(36), "a bed from before GB2 loads rich");
        Assert.That(GardenRules.AfterCrop(Crop("beans"), 0.5f), Is.EqualTo(0.4f).Within(0.001f), "beans put a little back");
        Assert.That(GardenRules.FertilizerToRich(spent), Is.EqualTo(4));
        for (int i = 0; i < 4; i++)
            spent = GardenRules.AfterFertilizer(spent);
        Assert.That(spent, Is.EqualTo(0f).Within(0.001f));
        Assert.That(GardenRules.FertilityText(0f), Is.EqualTo("rich soil"));
        Assert.That(GardenRules.FertilityText(1f), Does.Contain("75% harvest"));
        Assert.That(GardenRules.FertilizerId, Is.EqualTo(CompostRules.FertilizerId), "the Compost Bin's Fertilizer");
    }

    [Test]
    public void ARaisedBedIsWarmerAndDrainsSoItNeverWaterlogs()
    {
        GardenRules.Crop tomatoes = Crop("tomatoes");
        // A cool 14 C spring hour: the raised bed's warmer soil grows more.
        var dug = GardenRules.Hour(tomatoes, 10f, 0f, 0.6f, false, false, 14f, 0f, false, false);
        var raised = GardenRules.Hour(tomatoes, 10f, 0f, 0.6f, false, false, 14f, 0f, true, false);
        Assert.That(raised.growingDays - 10f, Is.GreaterThan(dug.growingDays - 10f));
        // Soaked soil: a dug bed waterlogs and grows slower; a raised one drains.
        var dugWet = GardenRules.Hour(tomatoes, 10f, 0f, 0.97f, false, false, 20f, 0f, false, false);
        var raisedWet = GardenRules.Hour(tomatoes, 10f, 0f, 0.97f, false, false, 20f, 0f, true, false);
        Assert.That(dugWet.growingDays - 10f, Is.EqualTo(GardenRules.WaterloggedFactor / 24f).Within(0.0001f));
        Assert.That(raisedWet.growingDays - 10f, Is.EqualTo(1f / 24f).Within(0.0001f));
        Assert.That(raisedWet.moisture, Is.LessThan(dugWet.moisture), "the raised bed drains");
        Assert.That(GardenRules.RaisedBedLumber, Is.EqualTo(2));
        Assert.That(GardenRules.RaisedHelp, Does.Contain("Lumber").And.Contain("Shovel"));
    }

    [Test]
    public void ARowCoverKeepsALightFrostOffAndSlowsTheCropInTheHeat()
    {
        GardenRules.Crop tomatoes = Crop("tomatoes");
        Assert.That(GardenRules.Hour(tomatoes, 10f, 0f, 0.6f, false, false, -2f, 0f, false, false).damage, Is.GreaterThan(0f), "a light frost hurts a bare tender crop");
        Assert.That(GardenRules.Hour(tomatoes, 10f, 0f, 0.6f, false, false, -2f, 0f, false, true).damage, Is.EqualTo(0f), "under a Row Cover it is kept off");
        Assert.That(GardenRules.Hour(tomatoes, 10f, 0f, 0.6f, false, false, -4f, 0f, false, true).damage, Is.GreaterThan(0f), "a hard frost gets through");
        var open = GardenRules.Hour(tomatoes, 10f, 0f, 0.6f, false, false, 30f, 0f, false, false);
        var covered = GardenRules.Hour(tomatoes, 10f, 0f, 0.6f, false, false, 30f, 0f, false, true);
        Assert.That(covered.growingDays - 10f, Is.EqualTo((open.growingDays - 10f) * GardenRules.CoverHeatFactor).Within(0.0001f));
        Assert.That(GardenRules.RowCoverStoreCents, Is.EqualTo(600L));
        Assert.That(MarketCatalog.Table.BuyBaseCents(GardenRules.RowCoverId), Is.EqualTo(600L));
    }

    [Test]
    public void TheFrostAlertWarnsBeforeTheAirReachesFreezing()
    {
        GardenRules.Crop tomatoes = Crop("tomatoes");
        Assert.That(GardenRules.FrostRisk(tomatoes, false, false, 1f), Is.True, "a 1 C night can frost the ground");
        Assert.That(GardenRules.FrostRisk(tomatoes, false, false, 4f), Is.False);
        Assert.That(GardenRules.FrostRisk(tomatoes, false, true, 1f), Is.False, "a Row Cover keeps that off");
        Assert.That(GardenRules.FrostRisk(Crop("kale"), false, false, -5f), Is.False, "kale is hardy");
        Assert.That(GardenRules.FrostAlert(1, 0, -1f), Does.StartWith("Frost likely tonight").And.Contain("lay a Row Cover over it"));
        Assert.That(GardenRules.FrostAlert(2, 0, -1f), Does.Contain("lay Row Covers over them"));
        Assert.That(GardenRules.FrostAlert(0, 1, -5f), Does.Contain("even under its Row Cover"));
        Assert.That(SourceText("Scripts/World/GardenBeds.cs"), Does.Contain("WeatherManager.Instance.ExpectedNightLowC()"));
        Assert.That(SourceText("Scripts/Managers/WeatherManager.cs"), Does.Contain("public float ExpectedNightLowC()"));
    }

    [Test]
    public void TheRaisedBedSiteAndTheBedsFlagsAreWiredIn()
    {
        string wood = SourceText("Scripts/Managers/WoodManager.cs");
        Assert.That(wood, Does.Contain("PileKind.GardenBed, PileKind.RaisedBedSite, PileKind.WattleFence, PileKind.DeerFence, PileKind.FenceGate, PileKind.ColdFrameSmall, PileKind.ColdFrameMedium, PileKind.ColdFrameLarge, PileKind.HoopHouseSiteSmall, PileKind.HoopHouseSiteMedium, PileKind.HoopHouseSiteLarge, PileKind.GlassHouseSiteSmall, PileKind.GlassHouseSiteMedium, PileKind.GlassHouseSiteLarge, PileKind.GreenhouseStove, PileKind.PropaneHeater, PileKind.WaterBarrels, PileKind.HoseBib, PileKind.HoseReel, PileKind.HandPump, PileKind.FuelPump, PileKind.VentOpener, PileKind.DeliveryPad, PileKind.PoleBarnSiteSmall, PileKind.PoleBarnSiteMedium, PileKind.PoleBarnSiteLarge, PileKind.TanningVatSite,"), "on the Build tab");
        Assert.That(wood, Does.Contain("public float soilSpent;"));
        Assert.That(wood, Does.Contain("if (IsBedKind(kind) && IsBedKind(other.kind))"), "raised beds stand side by side with dug ones");
        string sites = SourceText("Scripts/Managers/WellsAndOuthouses.cs");
        Assert.That(sites, Does.Contain("case PileKind.RaisedBedSite: return PileKind.GardenBed;"));
        Assert.That(sites, Does.Contain("site.cropFlags = GardenRules.FlagRaised;"));
        string beds = SourceText("Scripts/World/GardenBeds.cs");
        Assert.That(beds, Does.Contain("s.cropFlags &= GardenRules.KeptFlags;"), "harvesting keeps the bed raised and covered");
        Assert.That(beds, Does.Contain("s.soilSpent = GardenRules.AfterCrop(crop, s.soilSpent);"));
        Assert.That(GardenRules.KeptFlags & GardenRules.FlagDead, Is.EqualTo(0));
        Assert.That(SourceText("Scripts/Economy/Ledger.cs"), Does.Contain("{ GardenRules.RowCoverId, LedgerCategory.BuyOtherGoods }"));
    }
}
