using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

// Gardening slice GB3a (Mike, 2026-11-25, Gardening_Basics.md 9c): seed saving for the one-year crops, corn needing 2 squares saved together, seed that ages,
// and Tomato Seed sown straight into a bed.
public class GardenGB3Tests
{
    static string SourceText(string relative) => File.ReadAllText(Path.Combine(Application.dataPath, relative)).Replace("\r\n", "\n");
    static GardenRules.Crop C(string id) => GardenRules.Find(id);

    [Test]
    public void TomatoSeedIsSownAfterTheTomatoPlantsAndStartsAMonthBehind()
    {
        Assert.That(GardenRules.Sowings.Length, Is.EqualTo(10), "the nine crops' own seed, and Tomato Seed");
        Assert.That(GardenRules.Sowings.Select(s => s.seedId).ToArray(), Is.EqualTo(new[]
            { "potatoes", "seed_field_corn", "seed_sweet_corn", "seed_beans", "tomato_plant", "seed_tomatoes", "seed_lettuce", "seed_carrots", "seed_squash", "seed_kale" }));
        GardenRules.Sowing seed = GardenRules.Sowings[5];
        Assert.That(seed.crop.id, Is.EqualTo("tomatoes"));
        Assert.That(seed.seeds, Is.EqualTo(1));
        Assert.That(seed.startDays, Is.EqualTo(-30f));
        Assert.That(seed.Days, Is.EqualTo(105f));
        Assert.That(GardenRules.ForSeed("seed_tomatoes"), Is.SameAs(C("tomatoes")));
        Assert.That(GardenRules.ForSeed("tomato_plant"), Is.SameAs(C("tomatoes")));
        Assert.That(GardenRules.IsSeed("seed_tomatoes"), Is.True);
        Assert.That(GardenRules.PlantPrompt(seed, 3), Does.StartWith("Sow Tomato Seed  (uses 1 packet, you carry 3;"));
        Assert.That(GardenRules.PlantPrompt(seed, 3), Does.Contain("about 105 growing days from seed"));
    }

    [Test]
    public void EveryCropButPotatoesCanBeSaved()
    {
        foreach (string id in new[] { "field_corn", "sweet_corn", "beans", "tomatoes", "lettuce", "squash", "carrots", "kale" })
            Assert.That(GardenRules.CanSaveSeed(C(id)), Is.True, id);
        Assert.That(GardenRules.CanSaveSeed(C("potatoes")), Is.False);
        Assert.That(GardenRules.CantSaveText(C("potatoes")), Is.EqualTo(GardenRules.PotatoSeedTip));
        Assert.That(GardenRules.PlanFor(C("tomatoes")).savedSeedId, Is.EqualTo("seed_tomatoes"));
        Assert.That(GardenRules.PlanFor(C("beans")).savedSeedId, Is.EqualTo("seed_beans"));
    }

    [TestCase("beans", 90f)]       // left to dry: the dry-bean day
    [TestCase("tomatoes", 95f)]    // 75 + 20
    [TestCase("lettuce", 85f)]     // 50 + 35, bolted and flowered
    [TestCase("field_corn", 110f)] // 95 + 15
    [TestCase("sweet_corn", 103f)] // 78 + 25
    [TestCase("squash", 120f)]     // 100 + 20
    public void TheSeedRipensSomeDaysAfterTheCropIsReady(string id, float days)
    {
        Assert.That(GardenRules.SeedDays(C(id)), Is.EqualTo(days).Within(0.001f));
        Assert.That(GardenRules.SeedRipe(C(id), days - 0.1f), Is.False);
        Assert.That(GardenRules.SeedRipe(C(id), days), Is.True);
    }

    [Test]
    public void ARipeSquareGivesSeedForFourSquaresAndALittleFood()
    {
        Assert.That(GardenRules.SeedSquaresBack, Is.EqualTo(4));
        Assert.That(GardenRules.SeedYield(C("beans"), 0f, false), Is.EqualTo(4), "one square of beans needs no partner");
        Assert.That(GardenRules.SeedYield(C("tomatoes"), 0.5f, false), Is.EqualTo(2), "a half-damaged crop gives half");
        Assert.That(GardenRules.SeedYield(C("squash"), 0.99f, false), Is.EqualTo(1), "at least 1 while it lived");
        Assert.That(GardenRules.SeedFood(C("beans"), 0f), Is.EqualTo(2));
        Assert.That(GardenRules.SeedFoodId(C("beans")), Is.EqualTo("dry_beans"));
        Assert.That(GardenRules.SeedFood(C("tomatoes"), 0f), Is.EqualTo(18));
        Assert.That(GardenRules.SeedFood(C("lettuce"), 0f), Is.EqualTo(0), "bolted lettuce is bitter");
        Assert.That(GardenRules.SeedFood(C("sweet_corn"), 0f), Is.EqualTo(0));
        Assert.That(GardenRules.SeedFood(C("squash"), 0f), Is.EqualTo(3));
        Assert.That(GardenRules.SeedCollected(C("beans"), 4, "Bean Seed", 2, "Dry Beans"), Is.EqualTo("Collected Bean Seed x4 (seed for 4 squares) and Dry Beans x2."));
    }

    [Test]
    public void CornNeedsTwoSquaresOfItsKindSavedTogether()
    {
        foreach (string id in new[] { "field_corn", "sweet_corn" })
        {
            Assert.That(GardenRules.PlanFor(C(id)).squares, Is.EqualTo(2), id);
            Assert.That(GardenRules.Pollinates(C(id), 1), Is.False);
            Assert.That(GardenRules.Pollinates(C(id), 2), Is.True);
            Assert.That(GardenRules.SeedYield(C(id), 0f, false), Is.EqualTo(0), "corn that flowered alone sets no good seed");
            Assert.That(GardenRules.SeedYield(C(id), 0f, true), Is.EqualTo(4));
        }
        Assert.That(GardenRules.PlanFor(C("squash")).squares, Is.EqualTo(1), "Mike: squash needs 1");
        Assert.That(GardenRules.SaveSeedPrompt(C("field_corn")), Does.Contain("2 or more squares of Field Corn"));
        Assert.That(GardenRules.SeedCollected(C("field_corn"), 0, "Field Corn Seed", 4, "Field Corn"), Does.StartWith("No good seed: corn needs 2 or more squares"));
    }

    [Test]
    public void SeedAgesGoodForItsLifeThenHalfThenNone()
    {
        Assert.That(GardenRules.AgeYears(10, 10 + 365 * 2 + 100, 365), Is.EqualTo(2));
        Assert.That(GardenRules.AgeYears(500, 400, 365), Is.EqualTo(0), "never negative");
        GardenRules.Crop beans = C("beans");   // about 3 years
        Assert.That(GardenRules.Sprouts(beans, 0), Is.EqualTo(1f));
        Assert.That(GardenRules.Sprouts(beans, 2), Is.EqualTo(1f));
        Assert.That(GardenRules.Sprouts(beans, 3), Is.EqualTo(0.5f));
        Assert.That(GardenRules.Sprouts(beans, 4), Is.EqualTo(0f));
        Assert.That(GardenRules.Sprouts(C("field_corn"), 2), Is.EqualTo(0.5f), "corn seed keeps only about 2 years");
        Assert.That(GardenRules.Sprouts(C("tomatoes"), 3), Is.EqualTo(1f));
        Assert.That(GardenRules.AgeWarning(beans, 1), Is.Null);
        Assert.That(GardenRules.AgeWarning(beans, 3), Does.Contain("only about half of it will come up"));
        Assert.That(GardenRules.AgeWarning(beans, 4), Does.Contain("too old"));
        Assert.That(GardenRules.AgeTag(beans, 0), Is.Null);
        Assert.That(GardenRules.AgeTag(beans, 1), Is.EqualTo("1 yr old"));
        Assert.That(GardenRules.AgeTag(beans, 3), Is.EqualTo("3 yrs old - half sprout"));
        Assert.That(GardenRules.AgeTag(beans, 5), Is.EqualTo("5 yrs old - dead"));
        Assert.That(GardenRules.AgesAsSeed("seed_beans"), Is.True);
        Assert.That(GardenRules.AgesAsSeed("seed_tomatoes"), Is.True);
        Assert.That(GardenRules.AgesAsSeed("tomato_plant"), Is.False);
        Assert.That(GardenRules.AgesAsSeed("potatoes"), Is.False);
    }

    [Test]
    public void LettuceSavedForSeedBoltsWithoutDying()
    {
        GardenRules.Crop lettuce = C("lettuce");
        float pastBolt = lettuce.days + lettuce.boltAfterDays + 1f;
        var eaten = GardenRules.Hour(lettuce, pastBolt, 0f, 0.5f, false, false, 30f, 0f, false, false, false);
        Assert.That(eaten.died, Is.True, "lettuce grown to eat dies when it bolts");
        var seed = GardenRules.Hour(lettuce, pastBolt, 0f, 0.5f, false, false, 30f, 0f, false, false, true);
        Assert.That(seed.died, Is.False);
        Assert.That(seed.damage, Is.EqualTo(0f), "heat does not hurt lettuce going to seed");
    }

    [Test]
    public void TheFlagsGoWithTheCrop()
    {
        Assert.That(GardenRules.FlagSeed, Is.EqualTo(16));
        Assert.That(GardenRules.FlagPollinated, Is.EqualTo(32));
        Assert.That(GardenRules.KeptFlags & (GardenRules.FlagSeed | GardenRules.FlagPollinated), Is.EqualTo(0));
    }

    [Test]
    public void TheBedWiresItUp()
    {
        string gb = SourceText("Scripts/World/GardenBeds.cs");
        Assert.That(gb, Does.Contain("Inventory.Player.RemoveBatches(sowing.seedId, sowing.seeds"), "the oldest seed goes in first, and its age is read");
        Assert.That(gb, Does.Contain("s.jobHours = sowing.startDays;"), "Tomato Seed starts behind a plant");
        Assert.That(gb, Does.Contain("bed.cropFlags |= GardenRules.FlagPollinated;"));
        Assert.That(gb, Does.Contain("Saving(s) ? 0 :"), "a square saved for seed is not picked");
        Assert.That(gb, Does.Contain("covered, forSeed, mulched);"), "the hour knows the square is going to seed");
        Assert.That(gb, Does.Contain("s.soilSpent = GardenRules.AfterCrop(crop, s.soilSpent);\n        ClearCrop(s);"), "collecting seed wears the soil as any crop does");
        Assert.That(SourceText("Scripts/UI/InventoryScreen.cs"), Does.Contain("SeedAgeOfPack(item)"));
        foreach (string id in new[] { "seed_beans", "seed_carrots", "seed_field_corn", "seed_kale", "seed_lettuce", "seed_squash", "seed_sweet_corn", "seed_tomatoes" })
        {
            string asset = SourceText($"Resources/Items/Materials/{id}.asset");
            Assert.That(asset, Does.Contain("keepsBatchDay: 1"), id + " keeps the day it was bought or saved");
        }
        Assert.That(SourceText("Resources/Items/Materials/seed_tomatoes.asset"), Does.Contain("displayName: Tomato Seed"));
    }
}
