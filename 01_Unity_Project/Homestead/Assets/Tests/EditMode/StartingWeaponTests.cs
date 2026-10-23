using NUnit.Framework;
using UnityEngine;

// EditMode tests for the fixed starting weapon (Mike, 2026-10-23): Pioneer carries the bow and arrows, Settler and Homesteader the rifle
// and rounds, with no choice at New Game; and the setup record no longer holds a weapon but still loads a section that has one.
public class StartingWeaponTests
{
    [TestCase(0, StartingWeapon.Rifle)] // Homesteader
    [TestCase(1, StartingWeapon.Rifle)] // Settler
    [TestCase(2, StartingWeapon.Bow)]   // Pioneer
    public void TheWeaponFollowsTheDifficulty(int difficulty, StartingWeapon expected)
    {
        Assert.That(StartingWeapons.For(difficulty), Is.EqualTo(expected));
    }

    [Test]
    public void TheRifleComesWithTenRoundsAndTheBowWithTwentyArrows()
    {
        Assert.That(StartingWeapons.Items(StartingWeapon.Rifle), Is.EqualTo(new[] { ("bolt_action_rifle", 1), ("rifle_rounds", 10) }));
        Assert.That(StartingWeapons.Items(StartingWeapon.Bow), Is.EqualTo(new[] { ("recurve_bow", 1), ("arrows", 20) }));
    }

    [Test]
    public void ARestartedSetupGivesTheSameWeaponAsTheOriginalDifficulty()
    {
        for (int tier = 0; tier <= 2; tier++)
        {
            NewGameSetup setup = NewGameSetup.Of(tier, true, 1, Money.Dollars(9000), Money.Dollars(1000));
            NewGameSetup? back = JsonUtility.FromJson<NewGameSetupSaveData>(JsonUtility.ToJson(NewGameSetupSaveData.Of(setup))).ToSetup();
            Assert.That(back.HasValue, Is.True);
            Assert.That(StartingWeapons.For(back.Value.difficulty), Is.EqualTo(StartingWeapons.For(tier)));
        }
    }

    [Test]
    public void TheSetupRoundTripsWithoutAWeaponField()
    {
        string json = JsonUtility.ToJson(NewGameSetupSaveData.Of(NewGameSetup.Of(2, true, 0, 1200000L, 200000L)));
        Assert.That(json, Does.Not.Contain("weapon"));
        NewGameSetup? back = JsonUtility.FromJson<NewGameSetupSaveData>(json).ToSetup();
        Assert.That(back.HasValue, Is.True);
        Assert.That(back.Value.difficulty, Is.EqualTo(2));
        Assert.That(back.Value.downCents, Is.EqualTo(1200000L));
        Assert.That(back.Value.reserveCents, Is.EqualTo(200000L));
    }

    [Test]
    public void ASetupSavedWithTheOldWeaponFieldStillLoads()
    {
        // The Phase C build wrote "weapon" inside the setup; JsonUtility ignores the field and nothing else changes.
        const string old = "{\"version\":1,\"hasSetup\":true,\"setup\":{\"difficulty\":1,\"weapon\":1,\"hasLand\":true,\"parcelIndex\":1,\"downCents\":900000,\"reserveCents\":100000}}";
        NewGameSetup? back = JsonUtility.FromJson<NewGameSetupSaveData>(old).ToSetup();
        Assert.That(back.HasValue, Is.True);
        Assert.That(back.Value.difficulty, Is.EqualTo(1));
        Assert.That(back.Value.hasLand, Is.True);
        Assert.That(back.Value.parcelIndex, Is.EqualTo(1));
        Assert.That(back.Value.downCents, Is.EqualTo(900000L));
        Assert.That(back.Value.reserveCents, Is.EqualTo(100000L));
        Assert.That(StartingWeapons.For(back.Value.difficulty), Is.EqualTo(StartingWeapon.Rifle));
    }
}
