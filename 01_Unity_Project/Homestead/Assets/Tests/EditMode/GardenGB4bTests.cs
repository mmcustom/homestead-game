using System.IO;
using NUnit.Framework;
using UnityEngine;

// Gardening slice GB4b (Mike, 2026-11-25 "See them when you're near"; built 2026-11-26): a raid on a bed near the player is a real deer or rabbit that walks in,
// eats, and only then takes its bite; chased off first, it takes nothing.
public class GardenGB4bTests
{
    static string SourceText(string relative) => File.ReadAllText(Path.Combine(Application.dataPath, relative)).Replace("\r\n", "\n");

    [Test]
    public void ARaidNearAWakePlayerIsAnAnimalYouCanSee()
    {
        Assert.That(FenceRules.VisibleRange, Is.EqualTo(90f));
        Assert.That(FenceRules.Visible(60f, false), Is.True);
        Assert.That(FenceRules.Visible(60f, true), Is.False, "asleep: worked out by the hour");
        Assert.That(FenceRules.Visible(120f, false), Is.False, "too far: worked out by the hour");
        Assert.That(FenceRules.RaiderSpawnMin, Is.LessThan(FenceRules.RaiderSpawnMax));
        Assert.That(FenceRules.RaiderMinFromPlayer, Is.GreaterThan(10f), "it does not appear at the player's feet");
    }

    [Test]
    public void ItEatsForAWhileBeforeItsBiteIsTaken()
    {
        Assert.That(FenceRules.EatSeconds("deer"), Is.EqualTo(20f));
        Assert.That(FenceRules.EatSeconds("rabbit"), Is.EqualTo(15f));
        Assert.That(FenceRules.Bite("deer"), Is.EqualTo(FenceRules.DeerBite));
        Assert.That(FenceRules.Bite("rabbit"), Is.EqualTo(FenceRules.RabbitBite));
        Assert.That(FenceRules.EatReach("deer"), Is.GreaterThan(FenceRules.EatReach("rabbit")), "a deer reaches in from the edge; a rabbit sits on the bed");
        Assert.That(FenceRules.Eats("deer", "potatoes", true), Is.False);
        Assert.That(FenceRules.Eats("rabbit", "potatoes", true), Is.True, "young plants of anything");
        Assert.That(FenceRules.EatingBanner("Deer", "Beans"), Is.EqualTo("A deer is eating your Beans - chase it off!"));
        Assert.That(FenceRules.AteBanner("Rabbit", "Lettuce"), Is.EqualTo("The rabbit ate some of your Lettuce - a fence keeps them out"));
    }

    [Test]
    public void TheWorldWiresItUp()
    {
        string animal = SourceText("Scripts/Wildlife/Animal.cs");
        Assert.That(animal, Does.Contain("enum State { Idle, Walk, Flee, Dead, Stunned, Raid }"), "appended");
        Assert.That(animal, Does.Contain("GardenBeds.RaiderArrived(raidBed, DisplayName);"));
        Assert.That(animal, Does.Contain("GardenBeds.RaiderAte(raidBed, speciesId, raidCrop, DisplayName);"));
        Assert.That(animal, Does.Contain("// A raider scared off before it has eaten its fill takes nothing (GB4b).\n        raidBed = null;"), "startled, it takes nothing");
        Assert.That(SourceText("Scripts/Managers/WildlifeManager.cs"), Does.Contain("public bool SendRaider(string speciesId, WoodPileState bed)"));
        string beds = SourceText("Scripts/World/GardenBeds.cs");
        Assert.That(beds, Does.Contain("if (rabbit && WildlifeManager.Instance.SendRaider(\"rabbit\", bed))"));
        Assert.That(beds, Does.Contain("if (speciesId == \"deer\" ? fenced.deer : fenced.rabbits)\n            return;"), "fenced off meanwhile: no bite");
        Assert.That(SourceText("Scripts/Debug/TimeDebugControls.cs"), Does.Contain("GardenBeds.DebugSendRaider("), "the / testing key");
    }
}
