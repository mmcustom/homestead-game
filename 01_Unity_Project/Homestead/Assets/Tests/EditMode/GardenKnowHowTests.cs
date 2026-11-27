using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

// Know-how and Inventory tips for Gardening GB2 to GB4 and Greenhouses G1 to G6 (Mike, 2026-11-27: they had none).
public class GardenKnowHowTests
{
    static string SourceText(string relative) => File.ReadAllText(Path.Combine(Application.dataPath, relative)).Replace("\r\n", "\n");

    [TestCase("row_cover", "better_beds")]
    [TestCase("fertilizer", "better_beds")]
    [TestCase("tomato", "seed_saving")]
    [TestCase("kale", "seed_saving")]
    [TestCase("seed_tomatoes", "seed_saving")]
    [TestCase("wire_mesh", "fences")]
    [TestCase("window_sash", "cold_frames")]
    [TestCase("greenhouse_hoop", "hoop_houses")]
    [TestCase("greenhouse_plastic", "hoop_houses")]
    [TestCase("glass_pane", "glass_houses")]
    [TestCase("starts_kale", "glass_houses")]
    [TestCase("greenhouse_stove", "greenhouse_heat")]
    [TestCase("propane_tank", "greenhouse_heat")]
    [TestCase("garden_hose", "watering")]
    [TestCase("gasoline_can", "watering")]
    [TestCase("vent_opener", "venting")]
    public void AnItemOpensItsEntry(string itemId, string entry)
    {
        Assert.That(KnowHowRules.ForItem(itemId), Is.EqualTo(new[] { entry }));
    }

    [Test]
    public void BoughtSeedPotatoesDoNotOpenSeedSaving()
    {
        Assert.That(KnowHowRules.ForItem("potatoes"), Is.Empty);
    }

    [Test]
    public void EachBuildOpensItsEntry()
    {
        Assert.That(KnowHowRules.ForHappening(KnowHowRules.Happening.RaisedBedPlaced), Is.EqualTo(new[] { KnowHowRules.BetterBeds }));
        Assert.That(KnowHowRules.ForHappening(KnowHowRules.Happening.FencePlaced), Is.EqualTo(new[] { KnowHowRules.Fences }));
        Assert.That(KnowHowRules.ForHappening(KnowHowRules.Happening.ColdFramePlaced), Is.EqualTo(new[] { KnowHowRules.ColdFrames }));
        Assert.That(KnowHowRules.ForHappening(KnowHowRules.Happening.HoopSitePlaced), Is.EqualTo(new[] { KnowHowRules.HoopHouses }));
        Assert.That(KnowHowRules.ForHappening(KnowHowRules.Happening.GlassSitePlaced), Is.EqualTo(new[] { KnowHowRules.GlassHouses }));
        Assert.That(KnowHowRules.ForHappening(KnowHowRules.Happening.HeatPlaced), Is.EqualTo(new[] { KnowHowRules.GreenhouseHeat }));
        Assert.That(KnowHowRules.ForHappening(KnowHowRules.Happening.WateringPlaced), Is.EqualTo(new[] { KnowHowRules.Watering }));
        Assert.That(KnowHowRules.ForHappening(KnowHowRules.Happening.VentOpenerPlaced), Is.EqualTo(new[] { KnowHowRules.Venting }));
        string wood = SourceText("Scripts/Managers/WoodManager.cs");
        Assert.That(wood, Does.Contain("if (KnowHowHappeningFor(kind) is KnowHowRules.Happening built && JournalManager.Instance != null)"), "building one opens it");
        Assert.That(SourceText("Scripts/Managers/JournalManager.cs"), Does.Contain("WoodManager.KnowHowHappeningFor(pile.kind)"), "and a loaded game earns what it already built");
    }

    [Test]
    public void ALoadedGameEarnsItsGardenAndGreenhouseEntries()
    {
        var earned = KnowHowRules.Backfill(id => id == "vent_opener", false, false, false, built: new[] { KnowHowRules.Happening.ColdFramePlaced, KnowHowRules.Happening.FencePlaced });
        Assert.That(earned, Is.EquivalentTo(new[] { "venting", "cold_frames", "fences" }));
    }

    [Test]
    public void EveryGardenAndGreenhouseItemHasATipThatFits()
    {
        string[] ids =
        {
            "hoe", "watering_can", "seed_field_corn", "seed_beans", "seed_kale", "seed_tomatoes", "tomato_plant", "starts_lettuce", "potatoes",
            "row_cover", "wire_mesh", "fertilizer", "window_sash", "greenhouse_hoop", "greenhouse_plastic", "glass_pane", "greenhouse_stove",
            "propane_heater", "propane_tank", "garden_hose", "drip_kit", "hand_pump", "fuel_pump", "gasoline_can", "vent_opener", "iron_bar",
        };
        foreach (string id in ids)
        {
            string tip = KnowHowRules.ItemTip(id);
            Assert.That(tip, Is.Not.Empty, id);
            Assert.That(tip.Length, Is.LessThanOrEqualTo(80), id + " must fit in two lines of the row's info text");
        }
        Assert.That(KnowHowRules.ItemTip("iron_bar"), Does.Contain("$20").And.Contain("Smithy"));
    }

    [Test]
    public void TheEntriesSayWhereToBuyThings()
    {
        KnowHowRules.TryGet(KnowHowRules.ColdFrames, out KnowHowRules.Entry frames);
        Assert.That(frames.body, Does.Contain("$8 each at the Trading Post"));
        KnowHowRules.TryGet(KnowHowRules.Venting, out KnowHowRules.Entry vents);
        Assert.That(vents.body, Does.Contain("24 C").And.Contain("18 C"));
    }
}
