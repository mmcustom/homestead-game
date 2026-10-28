using NUnit.Framework;

// EditMode tests for the Tripod (the Cooking Pot redesign, commit T): the pure rules in TripodRules and the refund in FireRefund, both in
// Homestead.Economy. The saved state, the recipe, the kits and the items live in the main assembly and are in TripodWorldTests.
public class TripodTests
{
    static string Name(string id) => id == TripodRules.PotId ? "Cooking Pot" : id == TripodRules.KettleId ? "Potash Kettle" : id;

    // --- The numbers ---

    [Test]
    public void TheTripodIsThreePolesAndOneCordageAndSnapsToAFireThreeMetresAway()
    {
        Assert.That(TripodRules.TripodId, Is.EqualTo("tripod"));
        Assert.That(TripodRules.Branches, Is.EqualTo(3));
        Assert.That(TripodRules.Cordage, Is.EqualTo(1));
        Assert.That(TripodRules.SnapMetres, Is.EqualTo(3f));
    }

    [Test]
    public void TheVesselIdsAreTheSavedOnes()
    {
        Assert.That(TripodRules.PotId, Is.EqualTo("cooking_pot"), "the id the kits, the market and the saves already use");
        Assert.That(TripodRules.KettleId, Is.EqualTo("potash_kettle"));
    }

    [TestCase("cooking_pot", true)]
    [TestCase("potash_kettle", true)]
    [TestCase("bucket", false)]
    [TestCase("tripod", false)]
    [TestCase("", false)]
    [TestCase(null, false)]
    public void OnlyThePotAndTheKettleAreVessels(string id, bool expected) =>
        Assert.That(TripodRules.IsVessel(id), Is.EqualTo(expected));

    // --- Setting one up ---

    [Test]
    public void ATripodGoesOnAFireThatHasNoneWhenOneIsCarried() =>
        Assert.That(TripodRules.CanSetUp(true, false, false), Is.EqualTo(TripodRules.SetUpResult.Ok));

    [Test]
    public void TheGrillsFireTakesNoTripodAndAFireTakesOnlyOne()
    {
        Assert.That(TripodRules.CanSetUp(true, false, true), Is.EqualTo(TripodRules.SetUpResult.GrillFire));
        Assert.That(TripodRules.CanSetUp(true, true, false), Is.EqualTo(TripodRules.SetUpResult.AlreadyHasTripod), "one tripod per fire");
        Assert.That(TripodRules.CanSetUp(false, false, false), Is.EqualTo(TripodRules.SetUpResult.NoTripod));
    }

    [Test]
    public void TheReasonsForRefusingAreCheckedInOrder()
    {
        Assert.That(TripodRules.CanSetUp(false, true, true), Is.EqualTo(TripodRules.SetUpResult.GrillFire), "the Grill beats everything");
        Assert.That(TripodRules.CanSetUp(false, true, false), Is.EqualTo(TripodRules.SetUpResult.AlreadyHasTripod), "a taken fire beats a missing item");
    }

    [Test]
    public void EveryRefusalHasAMessageAndOkHasNone()
    {
        foreach (TripodRules.SetUpResult result in new[] { TripodRules.SetUpResult.GrillFire, TripodRules.SetUpResult.AlreadyHasTripod, TripodRules.SetUpResult.NoTripod })
            Assert.That(TripodRules.SetUpMessage(result), Is.Not.Empty, result.ToString());
        Assert.That(TripodRules.SetUpMessage(TripodRules.SetUpResult.Ok), Is.Empty);
        Assert.That(TripodRules.SetUpMessage(TripodRules.SetUpResult.NoTripod), Does.Contain("3 Branches").And.Contain("1 Cordage"));
    }

    // --- E on the tripod: hang, swap or take down ---

    // hung, carries a Pot, carries a Kettle -> what E does, what goes on, what comes off
    [TestCase("", true, true, TripodRules.StepKind.Hang, "cooking_pot", null)]
    [TestCase("", true, false, TripodRules.StepKind.Hang, "cooking_pot", null)]
    [TestCase("", false, true, TripodRules.StepKind.Hang, "potash_kettle", null)]
    [TestCase(null, true, false, TripodRules.StepKind.Hang, "cooking_pot", null)]
    [TestCase("", false, false, TripodRules.StepKind.None, null, null)]
    [TestCase("cooking_pot", false, false, TripodRules.StepKind.TakeDown, null, "cooking_pot")]
    [TestCase("cooking_pot", true, false, TripodRules.StepKind.TakeDown, null, "cooking_pot")]
    [TestCase("cooking_pot", false, true, TripodRules.StepKind.Swap, "potash_kettle", "cooking_pot")]
    [TestCase("cooking_pot", true, true, TripodRules.StepKind.Swap, "potash_kettle", "cooking_pot")]
    [TestCase("potash_kettle", true, false, TripodRules.StepKind.Swap, "cooking_pot", "potash_kettle")]
    [TestCase("potash_kettle", false, true, TripodRules.StepKind.TakeDown, null, "potash_kettle")]
    [TestCase("potash_kettle", false, false, TripodRules.StepKind.TakeDown, null, "potash_kettle")]
    [TestCase("mystery", true, true, TripodRules.StepKind.TakeDown, null, "mystery")]
    public void EDoesTheNextThingForWhatIsHungAndWhatIsCarried(string hung, bool pot, bool kettle, TripodRules.StepKind kind, string hang, string takeDown)
    {
        TripodRules.Step step = TripodRules.NextStep(hung, pot, kettle);
        Assert.That(step.kind, Is.EqualTo(kind));
        Assert.That(step.hang, Is.EqualTo(hang));
        Assert.That(step.takeDown, Is.EqualTo(takeDown));
    }

    [Test]
    public void ThePromptsNameTheVesselsAndTheMessagesSayWhatHappened()
    {
        Assert.That(TripodRules.StepPrompt(TripodRules.NextStep("", true, false), Name), Is.EqualTo("Hang the Cooking Pot on the tripod"));
        Assert.That(TripodRules.StepPrompt(TripodRules.NextStep("cooking_pot", false, true), Name), Is.EqualTo("Swap the Cooking Pot for the Potash Kettle"));
        Assert.That(TripodRules.StepPrompt(TripodRules.NextStep("potash_kettle", false, false), Name), Is.EqualTo("Take the Potash Kettle down"));
        Assert.That(TripodRules.StepPrompt(TripodRules.NextStep("", false, false), Name), Does.StartWith("Tripod").And.Contain("Cooking Pot"));

        Assert.That(TripodRules.StepDone(TripodRules.NextStep("", true, false), Name), Is.EqualTo("Hung the Cooking Pot on the tripod."));
        Assert.That(TripodRules.StepDone(TripodRules.NextStep("cooking_pot", false, true), Name), Is.EqualTo("Took the Cooking Pot down and hung the Potash Kettle."));
        Assert.That(TripodRules.StepDone(TripodRules.NextStep("cooking_pot", false, false), Name), Is.EqualTo("Took the Cooking Pot down."));
        Assert.That(TripodRules.StepDone(TripodRules.NextStep("", false, false), Name), Is.Not.Empty);
    }

    // --- Dismantling ---

    [Test]
    public void TheTripodComesDownOnlyWithTheFireOutAndTheHammer()
    {
        Assert.That(TripodRules.CanDismantle(false, true), Is.EqualTo(TripodRules.DismantleResult.Ok));
        Assert.That(TripodRules.CanDismantle(true, true), Is.EqualTo(TripodRules.DismantleResult.FireLit));
        Assert.That(TripodRules.CanDismantle(false, false), Is.EqualTo(TripodRules.DismantleResult.NoHammer));
        Assert.That(TripodRules.CanDismantle(true, false), Is.EqualTo(TripodRules.DismantleResult.FireLit), "the fire is asked about first, as for the Campfire");
    }

    [Test]
    public void TheDismantleMessagesMatchTheCampfiresWording()
    {
        Assert.That(TripodRules.DismantleMessage(TripodRules.DismantleResult.FireLit), Is.EqualTo("Put the fire out first."));
        Assert.That(TripodRules.DismantleMessage(TripodRules.DismantleResult.NoHammer), Is.EqualTo("Equip a Hammer to dismantle."));
        Assert.That(TripodRules.DismantleMessage(TripodRules.DismantleResult.Ok), Is.Empty);
    }

    // --- The boil gate ---

    // lit fire in reach, it has a tripod, what hangs on it, what the job needs -> the answer
    [TestCase(true, true, "cooking_pot", "cooking_pot", TripodRules.Gate.Ok)]
    [TestCase(false, false, null, "cooking_pot", TripodRules.Gate.NoFire)]
    [TestCase(false, true, "cooking_pot", "cooking_pot", TripodRules.Gate.NoFire)]
    [TestCase(true, false, null, "cooking_pot", TripodRules.Gate.NoTripod)]
    [TestCase(true, false, "cooking_pot", "cooking_pot", TripodRules.Gate.NoTripod)]
    [TestCase(true, true, "", "cooking_pot", TripodRules.Gate.NoVessel)]
    [TestCase(true, true, null, "cooking_pot", TripodRules.Gate.NoVessel)]
    [TestCase(true, true, "potash_kettle", "cooking_pot", TripodRules.Gate.WrongVessel)]
    [TestCase(true, true, "cooking_pot", "potash_kettle", TripodRules.Gate.WrongVessel)]
    [TestCase(true, true, "potash_kettle", "potash_kettle", TripodRules.Gate.Ok)]
    public void ABoilNeedsItsVesselHungOnATripodAtALitFire(bool lit, bool tripod, string hung, string needed, TripodRules.Gate expected) =>
        Assert.That(TripodRules.BoilGate(lit, tripod, hung, needed), Is.EqualTo(expected));

    [Test]
    public void EveryReasonAJobCannotRunHasAFlashAndARowNote()
    {
        foreach (TripodRules.Gate gate in new[] { TripodRules.Gate.NoFire, TripodRules.Gate.NoTripod, TripodRules.Gate.NoVessel, TripodRules.Gate.WrongVessel })
        {
            Assert.That(TripodRules.GateMessage(gate, "Boiling sap", "Cooking Pot"), Does.StartWith("Boiling sap"), gate.ToString());
            Assert.That(TripodRules.RowInfo(gate, "Cooking Pot"), Is.Not.Empty, gate.ToString());
        }
        Assert.That(TripodRules.GateMessage(TripodRules.Gate.Ok, "Boiling sap", "Cooking Pot"), Is.Empty);
        Assert.That(TripodRules.RowInfo(TripodRules.Gate.Ok, "Cooking Pot"), Is.Empty);
        Assert.That(TripodRules.GateMessage(TripodRules.Gate.NoTripod, "Boiling water", "Cooking Pot"), Is.EqualTo("Boiling water needs a tripod with the Cooking Pot hung on it."));
        Assert.That(TripodRules.RowInfo(TripodRules.Gate.NoTripod, "Cooking Pot"), Is.EqualTo("needs a tripod with the Cooking Pot hung"));
    }

    [Test]
    public void ThePausedLineNamesTheVesselToHang() =>
        Assert.That(TripodRules.PausedMessage("Boiling", "Cooking Pot"), Is.EqualTo("Boiling paused — hang the Cooking Pot on a tripod at this fire"));

    // --- What comes back when a fire goes ---

    [Test]
    public void ATripodAndItsVesselComeBackWholeWhateverTheCampfiresHalfBackIs()
    {
        // 3 Firewood to build gives 1 back, 2 h of fuel 1 Firewood, 4 Ash 4: the tripod and the Pot are on top, never halved.
        FireRefundResult r = FireRefund.Of(false, 3, 6, 2f, 2f, 4.4f, true, "cooking_pot");
        Assert.That(r.firewood, Is.EqualTo(2));
        Assert.That(r.sticks, Is.EqualTo(0));
        Assert.That(r.ash, Is.EqualTo(4));
        Assert.That(r.tripod, Is.EqualTo(1));
        Assert.That(r.vessel, Is.EqualTo("cooking_pot"));
    }

    [Test]
    public void TheOldRefundIsUnchangedAndHasNoTripod()
    {
        FireRefundResult r = FireRefund.Of(true, 3, 6, 0f, 2f, 0f);
        Assert.That(r.sticks, Is.EqualTo(3));
        Assert.That(r.tripod, Is.EqualTo(0));
        Assert.That(r.vessel, Is.Null);
        Assert.That(FireRefund.Of(true, 3, 6, 0f, 2f, 0f, false, null).sticks, Is.EqualTo(3), "the longer form with no tripod agrees");
    }

    [Test]
    public void ATripodWithNothingHungStillCountsAndAnEmptyVesselIsNull()
    {
        FireRefundResult empty = FireRefund.Of(false, 1, 1, 0f, 2f, 0f, false, "");
        Assert.That(empty.IsEmpty, Is.True);
        Assert.That(empty.vessel, Is.Null, "the empty string is no vessel");

        FireRefundResult tripodOnly = FireRefund.Of(false, 1, 1, 0f, 2f, 0f, true, "");
        Assert.That(tripodOnly.IsEmpty, Is.False);
        Assert.That(tripodOnly.tripod, Is.EqualTo(1));
        Assert.That(tripodOnly.vessel, Is.Null);
    }

    [Test]
    public void AVesselAloneIsNotAnEmptyRefund() =>
        Assert.That(FireRefund.Of(false, 1, 1, 0f, 2f, 0f, false, "potash_kettle").IsEmpty, Is.False);
}
