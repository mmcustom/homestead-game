using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// EditMode tests for Tree Regrowth (Mike, 2026-10-31; Tree_Regrowth.md): RegrowRules, the pure calendar, growth curve and obstruction numbers behind
// WoodManager's daily tick, the blows and Logs a young tree gives, and the one added saved field. WoodManager itself needs a scene, so its pieces are
// reached by reflection (the main assembly cannot be referenced from here). Nothing here touches the Saves folder or a scene.
public class RegrowTests
{
    static Type Find(string name) =>
        AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => { try { return a.GetTypes(); } catch (Exception) { return new Type[0]; } })
                 .First(t => t.Name == name);

    const float AverageSize = 1.12f;

    // --- The calendar ---

    [TestCase(0, 0)]
    [TestCase(13, 0)]
    [TestCase(14, 1)]
    [TestCase(20, 1)]
    [TestCase(21, 2)]
    [TestCase(35, 4)]
    [TestCase(56, 7)]
    [TestCase(77, 10)]
    [TestCase(98, 13)]
    [TestCase(105, 14)]
    [TestCase(111, 14)]
    [TestCase(112, 15)]
    [TestCase(500, 15)]
    public void TheStageFollowsTheDaysSinceFelling(int days, int stage)
    {
        Assert.That(RegrowRules.Stage(days), Is.EqualTo(stage));
    }

    [Test]
    public void ItSproutsAtDayFourteenGrowsInFourteenSevenDaySteps()
    {
        Assert.That(RegrowRules.SproutDays, Is.EqualTo(14));
        Assert.That(RegrowRules.FullDays, Is.EqualTo(112), "one game year: 4 seasons of 28 days");
        Assert.That(RegrowRules.StepDays, Is.EqualTo(7));
        Assert.That(RegrowRules.LastGrowingStage, Is.EqualTo(14));
        int changes = 0, last = 0;
        for (int day = 0; day <= 130; day++)
        {
            int stage = RegrowRules.Stage(day);
            Assert.That(stage, Is.GreaterThanOrEqualTo(last), "a spot never goes backwards");
            if (stage != last)
            {
                changes++;
                if (stage <= RegrowRules.LastGrowingStage)
                    Assert.That(day, Is.EqualTo(14 + 7 * (stage - 1)), "a step every seven days from the sprout");
            }
            last = stage;
        }
        Assert.That(changes, Is.EqualTo(15), "14 growing steps, then the full tree");
    }

    [Test]
    public void AYearSkippedInOneGoLandsOnTheFullTree()
    {
        const int felledOn = 40;
        Assert.That(RegrowRules.Stage(RegrowRules.DaysSince(RegrowRules.StampFor(felledOn + 1, felledOn + 112), felledOn + 112)), Is.EqualTo(RegrowRules.FullStage));
        Assert.That(RegrowRules.Stage(RegrowRules.DaysSince(felledOn + 1, felledOn + 300)), Is.EqualTo(RegrowRules.FullStage), "more days than a year is still just full");
    }

    // --- The growth curve ---

    [Test]
    public void GrowthRisesFromEightPercentToAWholeTreeInEvenSteps()
    {
        Assert.That(RegrowRules.GrowthFraction(0), Is.EqualTo(0f));
        Assert.That(RegrowRules.GrowthFraction(1), Is.EqualTo(0.08f).Within(1e-6f));
        Assert.That(RegrowRules.GrowthFraction(RegrowRules.FullStage), Is.EqualTo(1f));
        Assert.That(RegrowRules.GrowthFraction(RegrowRules.LastGrowingStage), Is.EqualTo(0.9343f).Within(1e-3f));
        float step = RegrowRules.GrowthFraction(2) - RegrowRules.GrowthFraction(1);
        for (int stage = 2; stage <= RegrowRules.LastGrowingStage; stage++)
            Assert.That(RegrowRules.GrowthFraction(stage) - RegrowRules.GrowthFraction(stage - 1), Is.EqualTo(step).Within(1e-5f), "linear");
    }

    [Test]
    public void TheDaysOfThePlanGiveItsGrowthFigures()
    {
        // day 14, 35, 56, 77, 98, 112 -> g 0.08, 0.277, 0.474, 0.671, 0.869, 1
        float[] expected = { 0.08f, 0.277f, 0.474f, 0.671f, 0.869f, 1f };
        int[] days = { 14, 35, 56, 77, 98, 112 };
        for (int i = 0; i < days.Length; i++)
            Assert.That(RegrowRules.GrowthFraction(RegrowRules.Stage(days[i])), Is.EqualTo(expected[i]).Within(0.002f), "day " + days[i]);
    }

    [Test]
    public void HeightAndWidthAreScaledSoSizeIsExactlyTheGrowthTimesFullSize()
    {
        foreach (float g in new[] { 0.08f, 0.277f, 0.5f, 0.75f, 0.934f, 1f })
        {
            float scale = RegrowRules.ScaleOf(g);
            Assert.That(AxeRules.TreeSize(scale, scale), Is.EqualTo(g).Within(1e-4f), "size = height x sqrt(width) = g at g " + g);
        }
        Assert.That(RegrowRules.ScaleOf(1f), Is.EqualTo(1f));
        Assert.That(RegrowRules.ScaleOf(0.5f), Is.EqualTo(0.63f).Within(0.005f), "half the size looks 63% as tall");
    }

    // --- Blows and Logs: a young tree takes fewer of each ---

    static readonly int[] PlanDays = { 14, 35, 56, 77, 98, 112 };

    static float SizeOnDay(int days) => AverageSize * RegrowRules.GrowthFraction(RegrowRules.Stage(days));

    [Test]
    public void BlowsAtEachStageForAnAverageTreeMatchThePlanForTheAxeAndTheSaw()
    {
        int[] broadAxe = { 1, 3, 5, 8, 10, 11 }, broadSaw = { 1, 2, 3, 5, 6, 7 };
        int[] tallAxe = { 1, 4, 7, 10, 13, 15 }, tallSaw = { 1, 3, 4, 6, 8, 9 };
        for (int i = 0; i < PlanDays.Length; i++)
        {
            float size = SizeOnDay(PlanDays[i]);
            string day = "day " + PlanDays[i];
            Assert.That(AxeRules.FellBlows(10f, size, 1f), Is.EqualTo(broadAxe[i]), "broad Axe " + day);
            Assert.That(AxeRules.FellBlows(10f, size, AxeRules.DefaultSawFactor), Is.EqualTo(broadSaw[i]), "broad Saw " + day);
            Assert.That(AxeRules.FellBlows(13f, size, 1f), Is.EqualTo(tallAxe[i]), "tall Axe " + day);
            Assert.That(AxeRules.FellBlows(13f, size, AxeRules.DefaultSawFactor), Is.EqualTo(tallSaw[i]), "tall Saw " + day);
        }
    }

    [Test]
    public void LogsAtEachStageForAnAverageTreeMatchThePlanAndASeedlingGivesNone()
    {
        int[] broad = { 0, 1, 1, 2, 2, 2 }, tall = { 0, 1, 2, 2, 3, 3 };
        for (int i = 0; i < PlanDays.Length; i++)
        {
            float size = SizeOnDay(PlanDays[i]);
            Assert.That(RegrowRules.LogsFor(2f, size), Is.EqualTo(broad[i]), "broad day " + PlanDays[i]);
            Assert.That(RegrowRules.LogsFor(3f, size), Is.EqualTo(tall[i]), "tall day " + PlanDays[i]);
        }
        Assert.That(RegrowRules.LogsFor(2f, 0.05f), Is.EqualTo(0), "no forced minimum");
    }

    [Test]
    public void AFullTreeGivesWhatItAlwaysDidAndTheSmallestOriginalTreeStillGivesALog()
    {
        // The smallest tree in the terrain: height 0.72, width 0.61 -> size about 0.56, so no forced minimum was ever in play for a full tree.
        float smallest = AxeRules.TreeSize(0.72f, 0.61f);
        Assert.That(Math.Max(1, (int)Math.Round(2f * smallest)), Is.EqualTo(RegrowRules.LogsFor(2f, smallest)));
        Assert.That(Math.Max(1, (int)Math.Round(3f * smallest)), Is.EqualTo(RegrowRules.LogsFor(3f, smallest)));
        foreach (float size in new[] { 0.56f, 0.8f, 1.0f, 1.12f, 1.4f, 1.8f })
        {
            Assert.That(RegrowRules.LogsFor(2f, size), Is.EqualTo(Math.Max(1, (int)Math.Round(2f * size))), "broad " + size);
            Assert.That(RegrowRules.LogsFor(3f, size), Is.EqualTo(Math.Max(1, (int)Math.Round(3f * size))), "tall " + size);
        }
    }

    [Test]
    public void ACutSaplingNeverGivesLogsFasterPerBlowThanAFullTree()
    {
        float fullPerBlow = RegrowRules.LogsFor(3f, AverageSize) / AxeRules.FellBlows(13f, AverageSize, 1f);
        foreach (int days in new[] { 35, 56, 77, 98 })
        {
            float size = SizeOnDay(days);
            float perBlow = RegrowRules.LogsFor(3f, size) / AxeRules.FellBlows(13f, size, 1f);
            Assert.That(perBlow, Is.LessThanOrEqualTo(fullPerBlow + 0.1f), "day " + days);
        }
    }

    [Test]
    public void AHalfGrownTreeTakesAboutHalfTheBlowsOfAFullOne()
    {
        float half = AverageSize * 0.5f;
        float fullBlows = AxeRules.FellBlows(13f, AverageSize, 1f);
        float halfBlows = AxeRules.FellBlows(13f, half, 1f);
        Assert.That(halfBlows, Is.LessThan(fullBlows));
        Assert.That(halfBlows, Is.EqualTo(fullBlows / 2f).Within(1f));
    }

    // --- Who regrows ---

    [TestCase(0, true, true)]
    [TestCase(1, true, true)]
    [TestCase(0, false, false)]
    [TestCase(1, false, false)]
    [TestCase(2, false, true)]
    [TestCase(2, true, true)]
    public void AShrubAlwaysRegrowsAndAHardwoodOnlyWhileItsStumpIsNotDugOut(int prototype, bool stump, bool regrows)
    {
        Assert.That(RegrowRules.CanRegrow(prototype, stump), Is.EqualTo(regrows));
    }

    // --- Old saves ---

    [Test]
    public void ASpotFromAnOlderSaveTakesTheDayItIsFirstSeenAndStartsItsYearThen()
    {
        const int today = 200;
        int stamp = RegrowRules.StampFor(0, today);
        Assert.That(stamp, Is.EqualTo(today + 1));
        Assert.That(RegrowRules.Stage(RegrowRules.DaysSince(stamp, today)), Is.EqualTo(0), "still a stump, not an instant tree");
        Assert.That(RegrowRules.Stage(RegrowRules.DaysSince(stamp, today + 14)), Is.EqualTo(1));
        Assert.That(RegrowRules.Stage(RegrowRules.DaysSince(stamp, today + 112)), Is.EqualTo(RegrowRules.FullStage));
    }

    [Test]
    public void AStampThatWasSavedIsKept()
    {
        Assert.That(RegrowRules.StampFor(31, 500), Is.EqualTo(31));
        Assert.That(RegrowRules.StampFor(1, 500), Is.EqualTo(1), "felled on day 0 is stamp 1, not 'unset'");
    }

    [Test]
    public void ABlockedSproutHoldsAtDayThirteenSoItsYearWaitsAndThenSproutsWhenClear()
    {
        const int today = 90;
        int held = RegrowRules.HeldStamp(today);
        Assert.That(RegrowRules.Stage(RegrowRules.DaysSince(held, today)), Is.EqualTo(0), "not yet sprouted today");
        Assert.That(RegrowRules.DaysSince(held, today), Is.EqualTo(13));
        Assert.That(RegrowRules.Stage(RegrowRules.DaysSince(held, today + 1)), Is.EqualTo(1), "tomorrow it tries again and sprouts if clear");
    }

    [Test]
    public void TheDevRegrowStampMakesASpotAFullYearOldToday()
    {
        Assert.That(RegrowRules.Stage(RegrowRules.DaysSince(RegrowRules.FullStamp(300), 300)), Is.EqualTo(RegrowRules.FullStage));
        Assert.That(RegrowRules.Stage(RegrowRules.DaysSince(RegrowRules.FullStamp(0), 0)), Is.EqualTo(RegrowRules.FullStage));
    }

    [Test]
    public void AnOldFelledTreeWithNoStampLoadsWithStampZeroAndTheSaveVersionStaysOne()
    {
        Type saveData = Find("WoodSaveData");
        string old = "{\"felled\":[{\"index\":12,\"stump\":true},{\"index\":40,\"stump\":false}],\"piles\":[],\"nextPileId\":3,\"snowLoadHours\":1.5,\"stormAfterglow\":0.0}";
        object loaded = JsonUtility.FromJson(old, saveData);
        object list = saveData.GetField("felled").GetValue(loaded);
        Type tree = Find("FelledTree");
        var entries = ((System.Collections.IEnumerable)list).Cast<object>().ToList();
        Assert.That(entries.Count, Is.EqualTo(2));
        foreach (object entry in entries)
            Assert.That((int)tree.GetField("fellStamp").GetValue(entry), Is.EqualTo(0), "no fellStamp in the old JSON");
        Assert.That((int)tree.GetField("index").GetValue(entries[0]), Is.EqualTo(12));
        Assert.That((bool)tree.GetField("stump").GetValue(entries[0]), Is.True);
        Assert.That((bool)tree.GetField("stump").GetValue(entries[1]), Is.False);
        Assert.That((int)saveData.GetField("nextPileId").GetValue(loaded), Is.EqualTo(3));

        FieldInfo version = Find("SaveManager").GetField("SaveVersion", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        Assert.That(version.GetRawConstantValue(), Is.EqualTo(1), "no save version bump");
        Assert.That(LedgerSaveData.CurrentVersion, Is.EqualTo(1), "nor the ledger's");
    }

    [Test]
    public void TheStampSurvivesASaveAndLoad()
    {
        Type tree = Find("FelledTree");
        object entry = Activator.CreateInstance(tree);
        tree.GetField("index").SetValue(entry, 7);
        tree.GetField("stump").SetValue(entry, true);
        tree.GetField("fellStamp").SetValue(entry, 42);
        object back = JsonUtility.FromJson(JsonUtility.ToJson(entry), tree);
        Assert.That((int)tree.GetField("fellStamp").GetValue(back), Is.EqualTo(42));
        Assert.That((int)tree.GetField("index").GetValue(back), Is.EqualTo(7));
    }

    [Test]
    public void OnlyTheOneStampFieldWasAddedToTheSavedWoodData()
    {
        string[] felled = Find("FelledTree").GetFields(BindingFlags.Public | BindingFlags.Instance).Select(f => f.Name).OrderBy(n => n).ToArray();
        Assert.That(felled, Is.EqualTo(new[] { "fellStamp", "index", "stump" }));
        string[] wood = Find("WoodSaveData").GetFields(BindingFlags.Public | BindingFlags.Instance).Select(f => f.Name).OrderBy(n => n).ToArray();
        Assert.That(wood, Is.EqualTo(new[] { "felled", "nextPileId", "piles", "snowLoadHours", "stormAfterglow" }), "WoodSaveData itself is unchanged");
    }

    [Test]
    public void PileKindNumbersAreUntouchedAndTheNewKindsStayOffThatList()
    {
        Type kind = Find("PileKind");
        string[] names = Enum.GetNames(kind);
        Assert.That(names.Length, Is.EqualTo(33));
        Assert.That(names[0], Is.EqualTo("Felled"));
        Assert.That(names.Last(), Is.EqualTo("HandCart"));
        Assert.That((int)Enum.Parse(kind, "HandCart"), Is.EqualTo(32));
    }

    // --- Stumps and Windthrow ---

    [Test]
    public void AStumpIsSwallowedFromAHalfGrownTreeOnAndWindthrowTakesOnlyThreeQuarterGrownOnes()
    {
        int firstGone = Enumerable.Range(1, RegrowRules.LastGrowingStage).First(s => RegrowRules.StumpGone(RegrowRules.GrowthFraction(s)));
        Assert.That(firstGone, Is.EqualTo(8));
        Assert.That(RegrowRules.GrowthFraction(7), Is.LessThan(0.5f), "still there at stage 7 (day 56)");
        int firstBlown = Enumerable.Range(1, RegrowRules.LastGrowingStage).First(s => RegrowRules.WindthrowEligible(RegrowRules.GrowthFraction(s)));
        Assert.That(firstBlown, Is.EqualTo(12));
        Assert.That(RegrowRules.WindthrowEligible(1f), Is.True);
        Assert.That(RegrowRules.WindthrowEligible(0.74f), Is.False);
    }

    // --- Obstruction at the sprout moment ---

    const float Cabin = 3.2f, Kitchen = 5.3f;

    [TestCase(RegrowRules.Blocker.Structure, 1.49f, true)]
    [TestCase(RegrowRules.Blocker.Structure, 1.5f, false)]
    [TestCase(RegrowRules.Blocker.Cabin, 3.19f, true)]
    [TestCase(RegrowRules.Blocker.Cabin, 3.2f, false)]
    [TestCase(RegrowRules.Blocker.Kitchen, 5.29f, true)]
    [TestCase(RegrowRules.Blocker.Kitchen, 5.3f, false)]
    [TestCase(RegrowRules.Blocker.Fire, 1.99f, true)]
    [TestCase(RegrowRules.Blocker.Fire, 2f, false)]
    [TestCase(RegrowRules.Blocker.Player, 1.49f, true)]
    [TestCase(RegrowRules.Blocker.Player, 1.5f, false)]
    public void ASproutIsBlockedInsideEachThingsClearance(RegrowRules.Blocker kind, float distance, bool blocked)
    {
        Assert.That(RegrowRules.Blocks(kind, distance, Cabin, Kitchen), Is.EqualTo(blocked));
    }

    [Test]
    public void RegisteredClearedGroundBlocksWithinItsOwnRadiusAndNothingElseDoes()
    {
        Assert.That(RegrowRules.Blocks(RegrowRules.Blocker.ClearedGround, 9.9f, Cabin, Kitchen, 10f), Is.True);
        Assert.That(RegrowRules.Blocks(RegrowRules.Blocker.ClearedGround, 10f, Cabin, Kitchen, 10f), Is.False);
        Assert.That(RegrowRules.Blocks(RegrowRules.Blocker.ClearedGround, 0f, Cabin, Kitchen, 0f), Is.False, "a zone of no radius blocks nothing");
        Assert.That(RegrowRules.BlockRadius(RegrowRules.Blocker.Cabin, Cabin, Kitchen), Is.EqualTo(3.2f), "the Cabin's own 3.2 m rule");
    }

    [Test]
    public void TheCabinClearanceMatchesWhatWoodManagerRequiresOfACabinSite()
    {
        // The Cabin's clearance is WoodManager's own tunable (a component field with its 3.2 m default), passed to the rules; the test only pins that it is still there.
        FieldInfo footprint = Find("WoodManager").GetField("cabinFootprint", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.That(footprint, Is.Not.Null);
        Assert.That(footprint.FieldType, Is.EqualTo(typeof(float)));
        string source = System.IO.File.ReadAllText(System.IO.Path.Combine(Application.dataPath, "Scripts/Managers/WoodManager.cs"));
        Assert.That(source, Does.Contain("float cabinFootprint = 3.2f;"));
    }

    // --- The hooks other code uses ---

    [Test]
    public void WoodManagerOffersTheTreeRevisionTheClearedGroundHookAndTheDevRegrow()
    {
        Type wood = Find("WoodManager");
        Assert.That(wood.GetProperty("TreeRevision")?.PropertyType, Is.EqualTo(typeof(int)));
        Assert.That(wood.GetMethod("AddClearedGround"), Is.Not.Null);
        Assert.That(wood.GetMethod("RemoveClearedGround"), Is.Not.Null);
        Assert.That(wood.GetMethod("DebugRegrowAll")?.ReturnType, Is.EqualTo(typeof(int)));
    }
}
