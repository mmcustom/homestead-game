using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

// EditMode tests for SlotSwap. Everything runs under a fresh folder in the system temp path; the real Saves folder is never
// touched. A "crash" is the hook throwing SimulatedCrash, which leaves the folders exactly as they were at that point.
public class SlotSwapTests
{
    string root;
    int savedAttempts;
    int savedDelay;

    [SetUp]
    public void SetUp()
    {
        root = Path.Combine(Path.GetTempPath(), "HomesteadSlotSwapTests", Guid.NewGuid().ToString("N"), "Saves");
        Directory.CreateDirectory(root);
        savedAttempts = SlotSwap.RenameAttempts;
        savedDelay = SlotSwap.RenameDelayMs;
        SlotSwap.RenameAttempts = 2; // keep the locked-rename tests quick
        SlotSwap.RenameDelayMs = 10;
    }

    [TearDown]
    public void TearDown()
    {
        SlotSwap.RenameAttempts = savedAttempts;
        SlotSwap.RenameDelayMs = savedDelay;
        try { Directory.Delete(Path.GetDirectoryName(root), true); } catch (Exception) { }
    }

    // --- helpers ---

    static readonly string[] Standard = { "meta", "slotinfo", "player", "world" };

    static Dictionary<string, string> Files(string tag, params string[] names) =>
        (names.Length == 0 ? Standard : names).ToDictionary(n => n, n => tag + ":" + n);

    static Action<string> CrashAt(string point) => p =>
    {
        if (p == point)
            throw new SlotSwap.SimulatedCrash(p);
    };

    // The slot folder holds exactly these files, every one from the same save.
    void AssertSlot(string id, string tag, params string[] names)
    {
        string folder = Path.Combine(root, id);
        Assert.IsTrue(Directory.Exists(folder), id + " should exist");
        Dictionary<string, string> actual = Directory.GetFiles(folder).ToDictionary(Path.GetFileName, File.ReadAllText);
        Dictionary<string, string> expected = (names.Length == 0 ? Standard : names).ToDictionary(n => n + ".json", n => tag + ":" + n);
        CollectionAssert.AreEquivalent(expected.Keys, actual.Keys, id + " file set");
        foreach (KeyValuePair<string, string> pair in expected)
            Assert.AreEqual(pair.Value, actual[pair.Key], id + "/" + pair.Key);
    }

    void AssertFolders(params string[] names) =>
        CollectionAssert.AreEquivalent(names, Directory.GetDirectories(root).Select(Path.GetFileName).ToArray());

    void Mk(string folder, params string[] files)
    {
        string path = Path.Combine(root, folder);
        Directory.CreateDirectory(path);
        foreach (string f in files)
            File.WriteAllText(Path.Combine(path, f), folder + ":" + f);
    }

    // --- save ---

    [Test]
    public void Save_FirstSaveToEmptySlot_WritesWholeSlot()
    {
        SlotSwap.Save(root, "slot2", Files("new"));
        AssertSlot("slot2", "new");
        AssertFolders("slot2");
    }

    [Test]
    public void Save_Overwrite_ReplacesWholeSlot_AndDropsFilesNoLongerWritten()
    {
        SlotSwap.Save(root, "slot2", Files("old", "meta", "slotinfo", "player", "world", "journal"));
        SlotSwap.Save(root, "slot2", Files("new"));

        AssertSlot("slot2", "new"); // no stale journal.json
        AssertFolders("slot2");
    }

    [Test]
    public void Save_WorksForEverySlotId()
    {
        foreach (string id in SlotSwap.ManualSlotIds().Append(SlotSwap.AutosaveId))
        {
            SlotSwap.Save(root, id, Files("old"));
            SlotSwap.Save(root, id, Files("new"));
            AssertSlot(id, "new");
        }
        Assert.AreEqual(11, Directory.GetDirectories(root).Length);
    }

    [Test]
    public void Save_WithoutMeta_Throws_AndChangesNothing()
    {
        SlotSwap.Save(root, "slot1", Files("old"));
        Assert.Throws<ArgumentException>(() => SlotSwap.Save(root, "slot1", Files("new", "player")));
        AssertSlot("slot1", "old");
        AssertFolders("slot1");
    }

    [Test]
    public void Save_Slot1OnExistingInstall_NeedsNoMigration()
    {
        // The step-1 layout: plain slot1 folder, per-file leftovers from the old writer, a section no longer written.
        Mk("slot1", "meta.json", "player.json", "world.json", "oldsection.json", "player.json.tmp");

        SlotSwap.Save(root, "slot1", Files("new"));

        AssertSlot("slot1", "new");
        AssertFolders("slot1");
    }

    [Test]
    public void Save_ExistingSlot1WithNoNewFilesStillReadableUntilSwap()
    {
        Mk("slot1", "meta.json", "player.json");
        // A crash before the swap leaves the existing install's slot exactly as it was.
        Assert.Throws<SlotSwap.SimulatedCrash>(() => SlotSwap.Save(root, "slot1", Files("new"), CrashAt(SlotSwap.CrashAfterTemp)));
        SlotSwap.RecoverAll(root);

        string folder = Path.Combine(root, "slot1");
        CollectionAssert.AreEquivalent(new[] { "meta.json", "player.json" }, Directory.GetFiles(folder).Select(Path.GetFileName).ToArray());
        Assert.AreEqual("slot1:player.json", File.ReadAllText(Path.Combine(folder, "player.json")));
        AssertFolders("slot1");
    }

    // --- crash recovery: save over an existing slot ---

    [TestCase(SlotSwap.CrashMidTemp, "old")]
    [TestCase(SlotSwap.CrashAfterTemp, "old")]
    [TestCase(SlotSwap.CrashAfterOld, "old")]
    [TestCase(SlotSwap.CrashAfterNew, "new")]
    public void Save_CrashThenRecover_LeavesWholeOldOrWholeNew(string point, string expectedTag)
    {
        SlotSwap.Save(root, "slot3", Files("old", "meta", "slotinfo", "player", "world", "journal"));

        Assert.Throws<SlotSwap.SimulatedCrash>(() => SlotSwap.Save(root, "slot3", Files("new"), CrashAt(point)));
        SlotSwap.RecoverAll(root);

        if (expectedTag == "old")
            AssertSlot("slot3", "old", "meta", "slotinfo", "player", "world", "journal");
        else
            AssertSlot("slot3", "new");
        AssertFolders("slot3");
    }

    [Test]
    public void Save_CrashStates_AreAsDocumented_BeforeRecovery()
    {
        SlotSwap.Save(root, "slot3", Files("old"));

        Assert.Throws<SlotSwap.SimulatedCrash>(() => SlotSwap.Save(root, "slot3", Files("new"), CrashAt(SlotSwap.CrashAfterOld)));
        AssertFolders("slot3.old", "slot3.tmp"); // slot missing, old intact, temp complete

        // A start-up after that restores the old slot and discards the temp.
        SlotSwap.RecoverAll(root);
        AssertSlot("slot3", "old");
        AssertFolders("slot3");
    }

    [Test]
    public void Save_CrashAfterNewRenamed_LeavesOldFolderForRecoveryToDelete()
    {
        SlotSwap.Save(root, "slot3", Files("old"));
        Assert.Throws<SlotSwap.SimulatedCrash>(() => SlotSwap.Save(root, "slot3", Files("new"), CrashAt(SlotSwap.CrashAfterNew)));

        AssertFolders("slot3", "slot3.old");
        AssertSlot("slot3", "new");
        SlotSwap.RecoverAll(root);
        AssertFolders("slot3");
        AssertSlot("slot3", "new");
    }

    // --- crash recovery: first save to an empty slot ---

    [TestCase(SlotSwap.CrashMidTemp, false)]
    [TestCase(SlotSwap.CrashAfterTemp, false)]
    [TestCase(SlotSwap.CrashAfterOld, false)]
    [TestCase(SlotSwap.CrashAfterNew, true)]
    public void FirstSave_CrashThenRecover_LeavesNothingOrWholeNew(string point, bool expectSlot)
    {
        Assert.Throws<SlotSwap.SimulatedCrash>(() => SlotSwap.Save(root, "slot4", Files("new"), CrashAt(point)));
        SlotSwap.RecoverAll(root);

        if (expectSlot)
        {
            AssertSlot("slot4", "new");
            AssertFolders("slot4");
        }
        else
        {
            AssertFolders(); // nothing: an incomplete temp is discarded, never promoted
        }
    }

    // --- failures that are not crashes ---

    [Test]
    public void Save_WhenOldSlotCannotBeRenamed_FailsAndLeavesOldSlotUntouched()
    {
        SlotSwap.Save(root, "slot5", Files("old"));

        // An open file with no delete-sharing blocks the folder rename (like antivirus or an Explorer window would).
        using (var held = new FileStream(Path.Combine(root, "slot5", "player.json"), FileMode.Open, FileAccess.Read, FileShare.None))
        {
            Assert.Catch<Exception>(() => SlotSwap.Save(root, "slot5", Files("new")));
        }

        AssertSlot("slot5", "old");
        AssertFolders("slot5"); // the temp was cleaned up
    }

    [Test]
    public void Save_WhenTempCannotBeRenamedIntoPlace_RollsTheOldSlotBack()
    {
        SlotSwap.Save(root, "slot5", Files("old"));

        FileStream held = null;
        try
        {
            // After the old slot is renamed away, hold a file inside the temp folder so renaming it into place fails.
            Action<string> hook = p =>
            {
                if (p == SlotSwap.CrashAfterOld)
                    held = new FileStream(Path.Combine(root, "slot5.tmp", "player.json"), FileMode.Open, FileAccess.Read, FileShare.None);
            };
            Assert.Catch<Exception>(() => SlotSwap.Save(root, "slot5", Files("new"), hook));
        }
        finally
        {
            held?.Dispose();
        }

        AssertSlot("slot5", "old"); // rolled back
        SlotSwap.RecoverAll(root);
        AssertFolders("slot5");
        AssertSlot("slot5", "old");
    }

    // --- path safety ---

    [TestCase("..")]
    [TestCase("../slot1")]
    [TestCase("..\\slot1")]
    [TestCase("slot1/..")]
    [TestCase("C:\\Windows")]
    [TestCase("/etc")]
    [TestCase("slot11")]
    [TestCase("slot0")]
    [TestCase("slot1.tmp")]
    [TestCase("slot1.old")]
    [TestCase("SLOT1")]
    [TestCase("Autosave")]
    [TestCase("")]
    [TestCase(null)]
    public void HostileIds_AreRejectedEverywhere_AndNothingIsCreated(string id)
    {
        Assert.IsFalse(SlotSwap.IsValidSlot(id));
        Assert.Throws<ArgumentException>(() => SlotSwap.Save(root, id, Files("new")));
        Assert.Throws<ArgumentException>(() => SlotSwap.Delete(root, id));
        Assert.Throws<ArgumentException>(() => SlotSwap.Recover(root, id));
        Assert.Throws<ArgumentException>(() => SlotSwap.Copy(root, id, "slot2"));
        Assert.Throws<ArgumentException>(() => SlotSwap.Copy(root, "slot2", id));

        AssertFolders();
        Assert.AreEqual(0, Directory.GetFileSystemEntries(Path.GetDirectoryName(root)).Count(e => Path.GetFileName(e) != "Saves"));
    }

    [Test]
    public void ValidIds_AreExactlySlot1To10AndAutosave()
    {
        var valid = SlotSwap.ManualSlotIds().Append("autosave").ToList();
        Assert.AreEqual(11, valid.Count);
        foreach (string id in valid)
            Assert.IsTrue(SlotSwap.IsValidSlot(id), id);
    }

    // --- start-up recovery scan ---

    [Test]
    public void RecoverAll_ActsOnlyOnValidSlotNamesWithTheThreeSuffixes()
    {
        Mk("slot3.old", "meta.json");                  // slot3 missing: restored
        Mk("slot3.tmp", "meta.json");                  // and its temp discarded
        Mk("slot4", "meta.json");                      // slot4 present...
        Mk("slot4.old", "meta.json");                  // ...so its old is deleted
        Mk("slot5.tmp", "meta.json");                  // temp alone: discarded
        Mk("autosave.del", "meta.json");               // delete in progress: finished
        Mk(".copy_tmp", "meta.json");                  // step 1's copy folder: removed
        Mk("slot11.old", "meta.json");                 // not a slot id: left alone
        Mk("notes.tmp", "a.txt");                      // not a slot id: left alone
        Mk("slot6", "meta.json");                      // plain slot: left alone
        File.WriteAllText(Path.Combine(root, "slots.json"), "{}");

        List<string> log = SlotSwap.RecoverAll(root);

        AssertFolders("slot3", "slot4", "slot6", "slot11.old", "notes.tmp");
        Assert.IsTrue(File.Exists(Path.Combine(root, "slot3", "meta.json")));
        Assert.IsTrue(File.Exists(Path.Combine(root, "slots.json")));
        Assert.IsNotEmpty(log);
    }

    [Test]
    public void RecoverAll_WhenSlotExists_NeverTouchesIt()
    {
        SlotSwap.Save(root, "slot7", Files("keep"));
        Mk("slot7.tmp", "meta.json");
        SlotSwap.RecoverAll(root);
        AssertSlot("slot7", "keep");
        AssertFolders("slot7");
    }

    [Test]
    public void RecoverAll_WithNoSavesFolder_DoesNothing()
    {
        Assert.IsEmpty(SlotSwap.RecoverAll(Path.Combine(root, "missing")));
    }

    [Test]
    public void RecoverAll_IsRepeatable()
    {
        Mk("slot3.old", "meta.json");
        SlotSwap.RecoverAll(root);
        Assert.IsEmpty(SlotSwap.RecoverAll(root));
        AssertFolders("slot3");
    }

    // --- delete ---

    [Test]
    public void Delete_RemovesTheSlot_AndIsHarmlessWhenEmpty()
    {
        SlotSwap.Save(root, "slot8", Files("old"));
        SlotSwap.Delete(root, "slot8");
        AssertFolders();
        Assert.DoesNotThrow(() => SlotSwap.Delete(root, "slot8"));
    }

    [Test]
    public void Delete_CrashAfterDelRenamed_RecoveryFinishesTheDelete()
    {
        SlotSwap.Save(root, "slot8", Files("old"));
        SlotSwap.Save(root, "slot9", Files("other"));

        Assert.Throws<SlotSwap.SimulatedCrash>(() => SlotSwap.Delete(root, "slot8", CrashAt(SlotSwap.CrashAfterDel)));
        AssertFolders("slot8.del", "slot9"); // the slot is already gone from the player's point of view

        SlotSwap.RecoverAll(root);
        AssertFolders("slot9");
        AssertSlot("slot9", "other");
    }

    [Test]
    public void Delete_WhenSlotCannotBeRenamed_FailsAndKeepsTheSlot()
    {
        SlotSwap.Save(root, "slot8", Files("old"));
        using (var held = new FileStream(Path.Combine(root, "slot8", "meta.json"), FileMode.Open, FileAccess.Read, FileShare.None))
        {
            Assert.Catch<Exception>(() => SlotSwap.Delete(root, "slot8"));
        }
        AssertSlot("slot8", "old");
        AssertFolders("slot8");
    }

    // --- copy ---

    [Test]
    public void Copy_OntoOccupiedTarget_ReplacesItWhole_AndAdjustsTheTemp()
    {
        SlotSwap.Save(root, "slot1", Files("src"));
        SlotSwap.Save(root, "slot2", Files("old", "meta", "slotinfo", "player", "world", "journal"));

        SlotSwap.Copy(root, "slot1", "slot2", temp => SlotSwap.WriteFlushed(Path.Combine(temp, "slotinfo.json"), "adjusted"));

        AssertSlot("slot1", "src");
        Assert.AreEqual("adjusted", File.ReadAllText(Path.Combine(root, "slot2", "slotinfo.json")));
        Assert.AreEqual("src:player", File.ReadAllText(Path.Combine(root, "slot2", "player.json")));
        Assert.IsFalse(File.Exists(Path.Combine(root, "slot2", "journal.json")));
        AssertFolders("slot1", "slot2");
    }

    [Test]
    public void Copy_ToEmptyTarget_Works()
    {
        SlotSwap.Save(root, "autosave", Files("src"));
        SlotSwap.Copy(root, "autosave", "slot6");
        AssertSlot("slot6", "src");
        AssertSlot("autosave", "src");
        AssertFolders("autosave", "slot6");
    }

    [TestCase(SlotSwap.CrashMidTemp, "old")]
    [TestCase(SlotSwap.CrashAfterTemp, "old")]
    [TestCase(SlotSwap.CrashAfterOld, "old")]
    [TestCase(SlotSwap.CrashAfterNew, "src")]
    public void Copy_CrashThenRecover_LeavesTargetWholeOldOrWholeCopy_AndSourceUntouched(string point, string expectedTag)
    {
        SlotSwap.Save(root, "slot1", Files("src"));
        SlotSwap.Save(root, "slot2", Files("old", "meta", "slotinfo", "player", "world", "journal"));

        Assert.Throws<SlotSwap.SimulatedCrash>(() => SlotSwap.Copy(root, "slot1", "slot2", null, CrashAt(point)));
        SlotSwap.RecoverAll(root);

        AssertSlot("slot1", "src");
        if (expectedTag == "old")
            AssertSlot("slot2", "old", "meta", "slotinfo", "player", "world", "journal");
        else
            AssertSlot("slot2", "src");
        AssertFolders("slot1", "slot2");
    }

    [Test]
    public void Copy_FromEmptySlot_Throws_AndChangesNothing()
    {
        SlotSwap.Save(root, "slot2", Files("old"));
        Assert.Throws<DirectoryNotFoundException>(() => SlotSwap.Copy(root, "slot1", "slot2"));
        AssertSlot("slot2", "old");
        AssertFolders("slot2");
    }

    [Test]
    public void Copy_OntoItself_Throws()
    {
        SlotSwap.Save(root, "slot2", Files("old"));
        Assert.Throws<ArgumentException>(() => SlotSwap.Copy(root, "slot2", "slot2"));
        AssertSlot("slot2", "old");
    }

    // --- single-file atomic write (slots.json) ---

    [Test]
    public void WriteFileAtomic_CreatesThenReplaces_LeavingNoTemp()
    {
        string path = Path.Combine(root, "slots.json");
        SlotSwap.WriteFileAtomic(path, "one");
        SlotSwap.WriteFileAtomic(path, "two");
        Assert.AreEqual("two", File.ReadAllText(path));
        CollectionAssert.AreEquivalent(new[] { "slots.json" }, Directory.GetFiles(root).Select(Path.GetFileName).ToArray());
    }
}
