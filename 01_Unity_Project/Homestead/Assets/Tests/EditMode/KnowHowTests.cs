using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

// EditMode tests for the Journal's Know-how section: the seven entries (word for word from Journal_KnowHow_Text.md), what unlocks each, the
// dedupe, the backfill for an old save, and the short hints the Inventory rows and prompts show. The save field is checked through JsonUtility.
public class KnowHowTests
{
    static Type Find(string name) =>
        AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => { try { return a.GetTypes(); } catch (Exception) { return new Type[0]; } })
                 .First(t => t.Name == name);

    // --- The entries ---

    [Test]
    public void ThereAreSevenEntriesWithUniqueIdsTitlesAndBodies()
    {
        Assert.That(KnowHowRules.Entries.Length, Is.EqualTo(7));
        Assert.That(KnowHowRules.Entries.Select(e => e.id).Distinct().Count(), Is.EqualTo(7));
        Assert.That(KnowHowRules.Entries.Select(e => e.title).Distinct().Count(), Is.EqualTo(7));
        foreach (KnowHowRules.Entry e in KnowHowRules.Entries)
        {
            Assert.That(e.title, Is.Not.Empty);
            Assert.That(e.body.Length, Is.GreaterThan(100), e.id);
            Assert.That(KnowHowRules.TryGet(e.id, out KnowHowRules.Entry found), Is.True);
            Assert.That(found.title, Is.EqualTo(e.title));
        }
        Assert.That(KnowHowRules.TryGet("nope", out _), Is.False);
        Assert.That(KnowHowRules.TryGet(null, out _), Is.False);
    }

    [Test]
    public void TheTitlesAreMikesInOrder()
    {
        Assert.That(KnowHowRules.Entries.Select(e => e.title), Is.EqualTo(new[]
        {
            "Ash: Collect and Sell", "Kindling Bundles", "The Tripod and Boiling Water", "Tapping Maples", "Sap to Syrup",
            "Using the Leach Hopper", "From Ash to Potash",
        }));
        Assert.That(KnowHowRules.Entries.Select(e => e.id), Is.EqualTo(new[]
        {
            KnowHowRules.Ash, KnowHowRules.Kindling, KnowHowRules.Tripod, KnowHowRules.Tapping, KnowHowRules.Syrup, KnowHowRules.Hopper, KnowHowRules.Potash,
        }));
    }

    // The text file is the source: every title and body in the code is the file's, character for character.
    [Test]
    public void TheEntriesMatchTheTextFileWordForWord()
    {
        string path = Path.GetFullPath(Path.Combine(Application.dataPath, "../../../05_Documentation/Game_Systems/Journal_KnowHow_Text.md"));
        if (!File.Exists(path))
            Assert.Ignore("Journal_KnowHow_Text.md is not at " + path);

        string text = File.ReadAllText(path).Replace("\r\n", "\n");
        string[] parts = Regex.Split(text, @"^## (\d+)\. (.+)$", RegexOptions.Multiline);
        var fromFile = new List<(string title, string body)>();
        for (int i = 1; i < parts.Length; i += 3)
        {
            string[] lines = parts[i + 2].Split(new[] { "\n---" }, StringSplitOptions.None)[0].Trim('\n').Split('\n');
            Assert.That(lines[0], Does.StartWith("Trigger:"));
            fromFile.Add((parts[i + 1].Trim(), string.Join("\n", lines.Skip(1)).Trim('\n')));
        }

        Assert.That(fromFile.Count, Is.EqualTo(KnowHowRules.Entries.Length));
        for (int i = 0; i < fromFile.Count; i++)
        {
            Assert.That(KnowHowRules.Entries[i].title, Is.EqualTo(fromFile[i].title));
            Assert.That(KnowHowRules.Entries[i].body, Is.EqualTo(fromFile[i].body), fromFile[i].title);
        }
    }

    // --- What unlocks what ---

    [TestCase("ash", new[] { "ash", "potash" })]
    [TestCase("kindling_bundle", new[] { "kindling" })]
    [TestCase("tripod", new[] { "tripod" })]
    [TestCase("spile", new[] { "tapping" })]
    [TestCase("sap_bucket", new[] { "tapping" })]
    [TestCase("maple_sap", new[] { "syrup" })]
    [TestCase("lye", new[] { "potash" })]
    [TestCase("potash_kettle", new[] { "potash" })]
    [TestCase("stone", new string[0])]
    [TestCase("potash", new string[0])]
    [TestCase("cooking_pot", new string[0])]
    public void AnItemOpensItsEntries(string itemId, string[] expected)
    {
        Assert.That(KnowHowRules.ForItem(itemId), Is.EqualTo(expected));
    }

    [Test]
    public void TheWorldHappeningsOpenTheirEntries()
    {
        Assert.That(KnowHowRules.ForHappening(KnowHowRules.Happening.PotHung), Is.EqualTo(new[] { "tripod" }));
        Assert.That(KnowHowRules.ForHappening(KnowHowRules.Happening.HopperSitePlaced), Is.EqualTo(new[] { "hopper", "potash" }));
    }

    [Test]
    public void EveryTriggerNamesARealEntry()
    {
        var ids = new List<string>();
        foreach (string itemId in new[] { "ash", "kindling_bundle", "tripod", "spile", "sap_bucket", "maple_sap", "lye", "potash_kettle" })
            ids.AddRange(KnowHowRules.ForItem(itemId));
        foreach (KnowHowRules.Happening h in Enum.GetValues(typeof(KnowHowRules.Happening)))
            ids.AddRange(KnowHowRules.ForHappening(h));
        Assert.That(ids, Is.Not.Empty);
        foreach (string id in ids)
            Assert.That(KnowHowRules.TryGet(id, out _), Is.True, id);
        Assert.That(ids.Distinct().Count(), Is.EqualTo(7), "and every entry has a way to unlock");
    }

    [Test]
    public void ARepeatedTriggerNeverUnlocksTwice()
    {
        List<string> first = KnowHowRules.NewUnlocks(new string[0], KnowHowRules.ForItem("ash"));
        Assert.That(first, Is.EqualTo(new[] { "ash", "potash" }));
        Assert.That(KnowHowRules.NewUnlocks(first, KnowHowRules.ForItem("ash")), Is.Empty);
        Assert.That(KnowHowRules.NewUnlocks(first, KnowHowRules.ForItem("lye")), Is.Empty, "Lye opens only the entry Ash already opened");
        Assert.That(KnowHowRules.NewUnlocks(first, KnowHowRules.ForItem("spile")), Is.EqualTo(new[] { "tapping" }));
    }

    [Test]
    public void TheDedupeHandlesTheStrangeCases()
    {
        Assert.That(KnowHowRules.NewUnlocks(null, new[] { "tripod", "tripod", "tripod" }), Is.EqualTo(new[] { "tripod" }));
        Assert.That(KnowHowRules.NewUnlocks(null, new[] { "made_up", null, "syrup" }), Is.EqualTo(new[] { "syrup" }), "unknown and null ids are dropped");
        Assert.That(KnowHowRules.NewUnlocks(new[] { "syrup" }, null), Is.Empty);
        Assert.That(KnowHowRules.NewUnlocks(null, null), Is.Empty);
    }

    // --- The backfill for a loaded game ---

    static List<string> Backfill(string[] held, bool hopper = false, bool tripod = false, bool taps = false) =>
        KnowHowRules.Backfill(id => held.Contains(id), hopper, tripod, taps);

    [Test]
    public void ALoadedGameEarnsWhatItAlreadyHolds()
    {
        Assert.That(Backfill(new string[0]), Is.Empty);
        Assert.That(Backfill(new[] { "ash" }), Is.EquivalentTo(new[] { "ash", "potash" }));
        Assert.That(Backfill(new[] { "spile", "sap_bucket", "maple_sap" }), Is.EquivalentTo(new[] { "tapping", "syrup" }));
        Assert.That(Backfill(new[] { "potash_kettle", "kindling_bundle", "tripod" }), Is.EquivalentTo(new[] { "potash", "kindling", "tripod" }));
    }

    [Test]
    public void ALoadedGameEarnsWhatItAlreadyBuilt()
    {
        Assert.That(Backfill(new string[0], hopper: true), Is.EquivalentTo(new[] { "hopper", "potash" }));
        Assert.That(Backfill(new string[0], tripod: true), Is.EquivalentTo(new[] { "tripod" }));
        Assert.That(Backfill(new string[0], taps: true), Is.EquivalentTo(new[] { "tapping" }));
        Assert.That(Backfill(new[] { "ash", "lye" }, hopper: true, tripod: true, taps: true).Count, Is.EqualTo(5), "each entry once, however many ways it was earned");
    }

    [Test]
    public void ASettlerAndAHomesteaderStartHoldingATripodAndSoEarnEntryThree()
    {
        Type tier = Find("Difficulty");
        foreach (string name in new[] { "Settler", "Homesteader" })
        {
            var kit = (Array)Find("DifficultyManager").GetMethod("Kit").Invoke(null, new[] { Enum.Parse(tier, name) });
            string[] ids = kit.Cast<object>().Select(k => (string)k.GetType().GetField("Item1").GetValue(k)).ToArray();
            Assert.That(ids, Does.Contain("tripod"), name);
            Assert.That(ids, Does.Contain("cooking_pot"), name);
            Assert.That(KnowHowRules.Backfill(id => ids.Contains(id), false, false, false), Is.EqualTo(new[] { "tripod" }), name + ": the Tripod only; a Pot carried opens nothing");
        }

        var pioneer = (Array)Find("DifficultyManager").GetMethod("Kit").Invoke(null, new[] { Enum.Parse(tier, "Pioneer") });
        string[] pioneerIds = pioneer.Cast<object>().Select(k => (string)k.GetType().GetField("Item1").GetValue(k)).ToArray();
        Assert.That(KnowHowRules.Backfill(id => pioneerIds.Contains(id), false, false, false), Is.Empty, "a Pioneer starts with none");
    }

    [Test]
    public void TheTripodOpensEntryThreeWhenHeldAndWhenAPotIsHung()
    {
        Assert.That(KnowHowRules.ForItem("tripod"), Is.EqualTo(new[] { KnowHowRules.Tripod }));
        Assert.That(KnowHowRules.ForHappening(KnowHowRules.Happening.PotHung), Is.EqualTo(new[] { KnowHowRules.Tripod }));
        Assert.That(KnowHowRules.NewUnlocks(KnowHowRules.ForItem("tripod"), KnowHowRules.ForHappening(KnowHowRules.Happening.PotHung)), Is.Empty,
                    "holding it first means hanging the Pot opens nothing new");
    }

    [Test]
    public void TheBackfillFlashNamesOneEntryAndCountsSeveral()
    {
        Assert.That(KnowHowRules.BackfillFlash(new[] { "tripod" }), Is.EqualTo("Journal: new know-how, The Tripod and Boiling Water"));
        Assert.That(KnowHowRules.BackfillFlash(new[] { "ash", "potash", "tripod" }), Is.EqualTo("Journal: 3 know-how entries added"));
    }

    // --- The words ---

    [Test]
    public void TheUnlockFlashNamesOneEntryOrCountsSeveral()
    {
        Assert.That(KnowHowRules.UnlockedMessage(new[] { "potash" }), Is.EqualTo("Journal: new know-how, From Ash to Potash"));
        Assert.That(KnowHowRules.UnlockedMessage(new[] { "ash", "potash" }), Is.EqualTo("Journal: 2 know-how entries added"));
        Assert.That(KnowHowRules.UnlockedMessage(new string[0]), Is.EqualTo(""));
        Assert.That(KnowHowRules.UnlockedMessage(null), Is.EqualTo(""));
        Assert.That(KnowHowRules.BackfilledMessage(1), Is.EqualTo("Journal: 1 know-how entry added"));
        Assert.That(KnowHowRules.BackfilledMessage(4), Is.EqualTo("Journal: 4 know-how entries added"));
    }

    [Test]
    public void TheVesselTipsSayHoldE()
    {
        Assert.That(KnowHowRules.ItemTip("cooking_pot"), Is.EqualTo("Hang it on a tripod at a fire (hold E) to boil water or sap."));
        Assert.That(KnowHowRules.ItemTip("potash_kettle"), Is.EqualTo("Hang it on a tripod at a lit fire (hold E), then Boil Lye (I)."));
    }

    [Test]
    public void EveryItemTipFitsTheRowAndTheOthersHaveNone()
    {
        string[] withTips = { "ash", "lye", "potash", "potash_kettle", "cooking_pot", "tripod", "spile", "sap_bucket", "maple_sap", "kindling_bundle" };
        foreach (string id in withTips)
        {
            string tip = KnowHowRules.ItemTip(id);
            Assert.That(tip, Is.Not.Empty, id);
            Assert.That(tip.Length, Is.LessThanOrEqualTo(80), id + " must fit in two lines of the row's info text");
        }
        Assert.That(KnowHowRules.ItemTip("stone"), Is.EqualTo(""));
        Assert.That(KnowHowRules.ItemTip(null), Is.EqualTo(""));
    }

    [Test]
    public void TheAshFlashSaysWhereAshGoesTheFirstTimeOnly()
    {
        Assert.That(KnowHowRules.AshCollected(3, 3, true), Is.EqualTo("Collected 3 Ash. Sell it, or leach it into Lye in a Leach Hopper (Build)."));
        Assert.That(KnowHowRules.AshCollected(3, 3, false), Is.EqualTo("Collected 3 Ash"));
        Assert.That(KnowHowRules.AshCollected(2, 5, false), Is.EqualTo("Collected 2 Ash — no room for the other 3"));
    }

    [Test]
    public void HangingAVesselPointsAtTheBoilButton()
    {
        Assert.That(KnowHowRules.AfterHang("cooking_pot"), Is.EqualTo(" Open Inventory (I) to Boil water or sap."));
        Assert.That(KnowHowRules.AfterHang("potash_kettle"), Is.EqualTo(" Open Inventory (I) to Boil Lye."));
        Assert.That(KnowHowRules.AfterHang("something_else"), Is.EqualTo(""));
    }

    [Test]
    public void TheHopperLinesNameTheHammerAndTheLoadStep()
    {
        Assert.That(KnowHowRules.HopperBuilt, Is.EqualTo("Leach Hopper built. Put the Hammer away, then hold R to load Ash and water."));
        Assert.That(KnowHowRules.HopperEmptyPrompt("Leach Hopper", false), Is.EqualTo("Leach Hopper  (empty)"));
        Assert.That(KnowHowRules.HopperEmptyPrompt("Leach Hopper", true), Is.EqualTo("Leach Hopper  (empty). Put the Hammer away to load Ash and water"));
        Assert.That(KnowHowRules.HopperLoadHelp(), Does.StartWith("Load Ash and water"));
        Assert.That(KnowHowRules.LyeTaken, Does.Contain("Potash Kettle"));
        Assert.That(KnowHowRules.SiteBuildInfo, Does.StartWith("Leaches Ash into Lye for Potash"));
    }

    [Test]
    public void TheTapHintShowsOnlyWhileThePackHasTooLittleSapForSyrup()
    {
        Assert.That(KnowHowRules.TapBoilHint(0), Is.EqualTo("  ·  10 L boils to 1 Syrup"));
        Assert.That(KnowHowRules.TapBoilHint(9), Is.EqualTo("  ·  10 L boils to 1 Syrup"));
        Assert.That(KnowHowRules.TapBoilHint(10), Is.EqualTo(""));
        Assert.That(KnowHowRules.TapBoilHint(40), Is.EqualTo(""));
    }

    [Test]
    public void TheEmptyTripodPromptMentionsTheKettle()
    {
        string prompt = TripodRules.StepPrompt(TripodRules.NextStep("", false, false), id => id);
        Assert.That(prompt, Does.Contain("Cooking Pot").And.Contain("Potash Kettle"));
    }

    // --- The save ---

    [Test]
    public void TheJournalSectionsKeepTheirSavedNumbersAndKnowHowIsAppended()
    {
        Type section = Find("JournalSection");
        string[] names = { "WaterSources", "Plants", "Wildlife", "Fishing", "PropertyFeatures", "History", "Notes", "KnowHow" };
        for (int i = 0; i < names.Length; i++)
            Assert.That((int)Enum.Parse(section, names[i]), Is.EqualTo(i), names[i]);
        Assert.That(Enum.GetValues(section).Length, Is.EqualTo(8));
    }

    [Test]
    public void AJournalSavedBeforeKnowHowLoadsWithNone()
    {
        Type data = Find("JournalSaveData");
        object loaded = JsonUtility.FromJson("{\"entries\":[],\"nextId\":4}", data);
        var none = data.GetField("knowHow").GetValue(loaded) as System.Collections.IEnumerable;
        Assert.That(none == null || !none.Cast<object>().Any(), Is.True, "the field is additive: absent means none");
        Assert.That((int)data.GetField("nextId").GetValue(loaded), Is.EqualTo(4));
    }

    [Test]
    public void TheUnlockedIdsRoundTripThroughJson()
    {
        Type data = Find("JournalSaveData"), unlock = Find("KnowHowUnlock");
        object one = Activator.CreateInstance(unlock);
        unlock.GetField("id").SetValue(one, "potash");
        unlock.GetField("day").SetValue(one, 12);
        unlock.GetField("read").SetValue(one, true);
        object list = Activator.CreateInstance(typeof(List<>).MakeGenericType(unlock));
        list.GetType().GetMethod("Add").Invoke(list, new[] { one });
        object save = Activator.CreateInstance(data);
        data.GetField("knowHow").SetValue(save, list);
        data.GetField("nextId").SetValue(save, 2);

        object back = JsonUtility.FromJson(JsonUtility.ToJson(save), data);
        var items = ((System.Collections.IEnumerable)data.GetField("knowHow").GetValue(back)).Cast<object>().ToList();
        Assert.That(items.Count, Is.EqualTo(1));
        Assert.That(unlock.GetField("id").GetValue(items[0]), Is.EqualTo("potash"));
        Assert.That(unlock.GetField("day").GetValue(items[0]), Is.EqualTo(12));
        Assert.That(unlock.GetField("read").GetValue(items[0]), Is.EqualTo(true));
    }
}
