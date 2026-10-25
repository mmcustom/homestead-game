using System.Linq;
using NUnit.Framework;
using UnityEngine;

// EditMode tests for the New Game parcel lock: only the 5-acre parcel is available in the early game. The lock is an availability rule
// (LandRules.IsAvailable); the money maths, setup records and saves still accept any parcel, so existing 10 and 40 acre games load and restart.
public class LandLockTests
{
    [Test]
    public void OnlyTheFiveAcreParcelIsAvailable()
    {
        Assert.That(LandRules.Starters[0].acres, Is.EqualTo(5));
        Assert.That(LandRules.IsAvailable(0), Is.True, "5 acres");
        Assert.That(LandRules.IsAvailable(1), Is.False, "10 acres");
        Assert.That(LandRules.IsAvailable(2), Is.False, "40 acres");
    }

    [Test]
    public void ExactlyOneStarterIsAvailableAndTheOthersHaveAReason()
    {
        Assert.That(Enumerable.Range(0, LandRules.Starters.Length).Count(LandRules.IsAvailable), Is.EqualTo(1));
        Assert.That(LandRules.LockedReason, Is.EqualTo("Needs more income than the early game offers. Unlocks later."));
    }

    [TestCase(-1)]
    [TestCase(3)]
    [TestCase(100)]
    [TestCase(int.MinValue)]
    [TestCase(int.MaxValue)]
    public void AnIndexOutOfRangeIsNotAvailableAndDoesNotThrow(int index)
    {
        bool available = true;
        Assert.DoesNotThrow(() => available = LandRules.IsAvailable(index));
        Assert.That(available, Is.False);
    }

    [Test]
    public void ChooseStaysUnrestrictedForTheLockedParcels()
    {
        for (int parcel = 0; parcel < LandRules.Starters.Length; parcel++)
        {
            long savings = LandRules.SavingsCents(LandRules.Starters[parcel].priceCents, LandRules.SavingsPercent(1));
            LandChoice choice = LandRules.Choose(parcel, savings, LandRules.DefaultDown(savings));
            Assert.That(choice.parcelIndex, Is.EqualTo(parcel));
            Assert.That(choice.downCents + choice.reserveCents, Is.EqualTo(savings));
        }
    }

    [Test]
    public void ASetupForALockedParcelIsStillValidSoRestartAndOldSavesKeepWorking()
    {
        foreach (int parcel in new[] { 1, 2 })
        {
            NewGameSetup setup = NewGameSetup.Of(1, true, parcel, 1400000, 1400000);
            Assert.That(setup.IsValid, Is.True, $"parcel {parcel}");
            NewGameSetupSaveData save = NewGameSetupSaveData.Of(setup);
            NewGameSetup? back = JsonUtility.FromJson<NewGameSetupSaveData>(JsonUtility.ToJson(save)).ToSetup();
            Assert.That(back.HasValue, Is.True);
            Assert.That(back.Value.parcelIndex, Is.EqualTo(parcel), "the saved choice comes back as it was");
        }
    }
}
