using System.IO;
using NUnit.Framework;
using UnityEngine;

// Gardening slice GB3b (2026-11-25, Gardening_Basics.md 13): carrots and kale saved for seed need a winter in the ground (cold days once ready), then regrow and
// set seed; a carrot root left in for seed is no longer frost-proof, and Tall Grass mulch keeps winter cold off the roots.
public class GardenGB3bTests
{
    static string SourceText(string relative) => File.ReadAllText(Path.Combine(Application.dataPath, relative)).Replace("\r\n", "\n");
    static GardenRules.Crop C(string id) => GardenRules.Find(id);

    [Test]
    public void CarrotsAndKaleNeedAWinter()
    {
        Assert.That(GardenRules.NeedsWinter(C("carrots")), Is.True);
        Assert.That(GardenRules.NeedsWinter(C("kale")), Is.True);
        Assert.That(GardenRules.NeedsWinter(C("beans")), Is.False);
        Assert.That(GardenRules.ChillDaysNeeded, Is.EqualTo(45f));
        Assert.That(GardenRules.ChillC, Is.EqualTo(7f));
        Assert.That(GardenRules.SeedDays(C("carrots")), Is.EqualTo(130f), "ready at 70, then 60 growing days after the winter");
        Assert.That(GardenRules.SeedDays(C("kale")), Is.EqualTo(110f), "ready at 60, then 50");
        Assert.That(GardenRules.SeedRipe(C("carrots"), 200f, 44f), Is.False, "never ripe before its winter, however long it grows");
        Assert.That(GardenRules.SeedRipe(C("carrots"), 129f, 45f), Is.False);
        Assert.That(GardenRules.SeedRipe(C("carrots"), 130f, 45f), Is.True);
        Assert.That(GardenRules.SeedRipe(C("carrots"), 200f), Is.False, "days alone never ripen a second-year crop");
        Assert.That(GardenRules.SeedRipe(C("beans"), 90f, 0f), Is.True, "one-year crops need no winter");
    }

    [Test]
    public void OnlyColdHoursOnceReadyCount()
    {
        Assert.That(GardenRules.ChillAfterHour(0f, 5f, true), Is.EqualTo(1f / 24f).Within(1e-6f));
        Assert.That(GardenRules.ChillAfterHour(0f, 7f, true), Is.EqualTo(1f / 24f).Within(1e-6f), "7 C counts");
        Assert.That(GardenRules.ChillAfterHour(0f, 7.1f, true), Is.EqualTo(0f));
        Assert.That(GardenRules.ChillAfterHour(0f, -5f, false), Is.EqualTo(0f), "not before the crop is ready");
        float chill = 0f;
        for (int h = 0; h < 45 * 24; h++)
            chill = GardenRules.ChillAfterHour(chill, 0f, true);
        Assert.That(GardenRules.Chilled(C("kale"), chill), Is.True, "45 cold days");
    }

    [Test]
    public void ACarrotLeftForSeedIsNotFrostProofButMulchHelps()
    {
        GardenRules.Crop carrots = C("carrots");
        float ready = carrots.days + 1f;
        var dug = GardenRules.Hour(carrots, ready, 0f, 0.5f, false, false, -15f, 0f, false, false, false, false);
        Assert.That(dug.damage, Is.EqualTo(0f), "a ready carrot to be dug sits safe under the ground");
        var seed = GardenRules.Hour(carrots, ready, 0f, 0.5f, false, false, -15f, 0f, false, false, true, false);
        Assert.That(seed.damage, Is.GreaterThan(0f), "a root left over winter for seed is hurt by a hard freeze");
        var mulched = GardenRules.Hour(carrots, ready, 0f, 0.5f, false, false, -12f, 0f, false, false, true, true);
        Assert.That(mulched.damage, Is.EqualTo(0f), "-12 C with 8 C of mulch is -4 C at the roots, above the carrot's -6 C");
        Assert.That(GardenRules.FrostRisk(carrots, true, false, -15f), Is.False);
        Assert.That(GardenRules.FrostRisk(carrots, true, false, -15f, true, false), Is.True);
        Assert.That(GardenRules.FrostRisk(carrots, true, false, -10f, true, true), Is.False);
        var kale = GardenRules.Hour(C("kale"), C("kale").days + 1f, 0f, 0.5f, false, false, -10f, 0f, false, false, true, false);
        Assert.That(kale.damage, Is.EqualTo(0f), "kale is hardy to about -12 C on its own");
    }

    [Test]
    public void TheWords()
    {
        Assert.That(GardenRules.SeedStatusText(C("kale"), 30f, 0f), Does.Contain("stay in the ground over winter"));
        Assert.That(GardenRules.SeedStatusText(C("kale"), 61f, 0f), Is.EqualTo("waiting for winter (it needs about 45 cold days)"));
        Assert.That(GardenRules.SeedStatusText(C("kale"), 61f, 20f), Is.EqualTo("wintering over, about 25 cold days to go"));
        Assert.That(GardenRules.SeedStatusText(C("kale"), 61f, 45f), Does.StartWith("going to seed"));
        Assert.That(GardenRules.SeedStatusText(C("kale"), 110f, 45f), Is.EqualTo("seed ripe"));
        Assert.That(GardenRules.SavingStarted(C("carrots")), Does.Contain("Mulch it with 4 Tall Grass (E)"));
        Assert.That(GardenRules.MulchPrompt(6), Is.EqualTo("Mulch it with Tall Grass  (uses 4, you carry 6; keeps about 8 C of winter cold off the roots)"));
        Assert.That(GardenRules.SeedFood(C("kale"), 0f), Is.EqualTo(2));
        Assert.That(GardenRules.SeedFood(C("carrots"), 0f), Is.EqualTo(0));
        Assert.That(GardenRules.KeptFlags & GardenRules.FlagMulched, Is.EqualTo(0), "the mulch goes with the crop");
    }

    [Test]
    public void TheBedCountsItsWinter()
    {
        string gb = SourceText("Scripts/World/GardenBeds.cs");
        Assert.That(gb, Does.Contain("bed.chillDays = GardenRules.ChillAfterHour(bed.chillDays, air, bed.jobHours >= readyDays);"));
        Assert.That(gb, Does.Contain("bed.jobHours = Mathf.Min(bed.jobHours, readyDays);"), "after the winter, growth starts again from ready");
        Assert.That(gb, Does.Contain("s.chillDays = 0f;"), "a new crop starts with no winter");
        Assert.That(gb, Does.Contain("s.cropFlags |= GardenRules.FlagMulched;"));
        Assert.That(SourceText("Scripts/Managers/WoodManager.cs"), Does.Contain("public float chillDays;"));
    }
}