using System;
using System.Linq;
using NUnit.Framework;

// EditMode tests for SlotRules: the save/load screen's current-slot, pre-selection and overwrite rules. No files are touched.
public class SlotRulesTests
{
    static Func<string, bool> Holding(params string[] ids) => ids.Contains;
    static bool[] Slots(params int[] filled) => Enumerable.Range(1, 10).Select(n => filled.Contains(n)).ToArray();

    [Test]
    public void AManualSaveNamesItselfAndTheAutosaveCarriesTheCurrentSlot()
    {
        Assert.That(SlotRules.LastSlotToWrite("slot3", null), Is.EqualTo("slot3"));
        Assert.That(SlotRules.LastSlotToWrite("slot3", "slot7"), Is.EqualTo("slot3"));
        Assert.That(SlotRules.LastSlotToWrite(SlotSwap.AutosaveId, "slot7"), Is.EqualTo("slot7"));
        Assert.That(SlotRules.LastSlotToWrite(SlotSwap.AutosaveId, null), Is.Null);
        Assert.That(SlotRules.LastSlotToWrite(SlotSwap.AutosaveId, "bogus"), Is.Null);
    }

    [Test]
    public void LoadingAManualSlotMakesItCurrentWhateverItsRecordSays()
    {
        Assert.That(SlotRules.CurrentAfterLoad("slot2", "", Holding("slot2")), Is.EqualTo("slot2"));
        Assert.That(SlotRules.CurrentAfterLoad("slot2", "slot9", Holding("slot2", "slot9")), Is.EqualTo("slot2"));
    }

    [Test]
    public void LoadingTheAutosaveTakesTheSlotItCarriedIfThatSlotStillHoldsASave()
    {
        Assert.That(SlotRules.CurrentAfterLoad(SlotSwap.AutosaveId, "slot4", Holding("slot4")), Is.EqualTo("slot4"));
        Assert.That(SlotRules.CurrentAfterLoad(SlotSwap.AutosaveId, "slot4", Holding("slot1")), Is.Null, "that slot was deleted");
    }

    [Test]
    public void AnOldSaveWithNoLastSlotLoadsWithNone()
    {
        Assert.That(SlotRules.CurrentAfterLoad(SlotSwap.AutosaveId, null, Holding("slot1")), Is.Null);
        Assert.That(SlotRules.CurrentAfterLoad(SlotSwap.AutosaveId, "", Holding("slot1")), Is.Null);
    }

    [Test]
    public void ABadLastSlotIsNone()
    {
        var any = Holding("slot1", "slot2", SlotSwap.AutosaveId);
        Assert.That(SlotRules.ResolveCurrent("slot11", any), Is.Null);
        Assert.That(SlotRules.ResolveCurrent("..\\slot1", any), Is.Null);
        Assert.That(SlotRules.ResolveCurrent(SlotSwap.AutosaveId, any), Is.Null, "the Autosave is never the current slot");
        Assert.That(SlotRules.ResolveCurrent("slot5", any), Is.Null, "an empty slot or missing file");
        Assert.That(SlotRules.ResolveCurrent("slot2", any), Is.EqualTo("slot2"));
    }

    [Test]
    public void OnlyAFilledSlotThatIsNotTheCurrentOneAsksBeforeBeingOverwritten()
    {
        Assert.That(SlotRules.NeedsOverwriteConfirm(false, "slot3", "slot1"), Is.False, "empty: one click");
        Assert.That(SlotRules.NeedsOverwriteConfirm(true, "slot1", "slot1"), Is.False, "the current slot: one click");
        Assert.That(SlotRules.NeedsOverwriteConfirm(true, "slot2", "slot1"), Is.True);
        Assert.That(SlotRules.NeedsOverwriteConfirm(true, "slot2", null), Is.True, "no current slot: every filled slot asks");
    }

    [Test]
    public void ThePreSelectionIsTheCurrentSlotThenTheFirstEmptyOneThenNothing()
    {
        Assert.That(SlotRules.PreSelect("slot3", Slots(1, 2, 3)), Is.EqualTo(3));
        Assert.That(SlotRules.PreSelect(null, Slots(1, 2, 3)), Is.EqualTo(4), "no current slot: the first empty");
        Assert.That(SlotRules.PreSelect("slot6", Slots(1, 2, 3)), Is.EqualTo(4), "current points at an empty slot: treated as none");
        Assert.That(SlotRules.PreSelect(null, Slots()), Is.EqualTo(1));
        Assert.That(SlotRules.PreSelect(null, Slots(1, 2, 3, 4, 5, 6, 7, 8, 9, 10)), Is.EqualTo(-1), "all full, none current: nothing");
        Assert.That(SlotRules.PreSelect("slot10", Slots(1, 2, 3, 4, 5, 6, 7, 8, 9, 10)), Is.EqualTo(10));
    }

    [Test]
    public void TheAutosaveIsTheCurrentGameOnlyInGameWithNoCurrentManualSlot()
    {
        Assert.That(SlotRules.AutosaveIsCurrent(true, null), Is.True);
        Assert.That(SlotRules.AutosaveIsCurrent(true, "slot2"), Is.False);
        Assert.That(SlotRules.AutosaveIsCurrent(false, null), Is.False);
    }
}
