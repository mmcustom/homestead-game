using System.Linq;
using NUnit.Framework;

// EditMode tests for Potash B's pure rules (PotashRules, Homestead.Economy): what a hopper load takes (worst water first, the cap of 12), the
// leach clock, and the words the prompts and the Inventory's Lye row use. The hopper and the Kettle boil in the world are in PotashWorldTests.
public class PotashHopperTests
{
    // The order PotashRules.WaterWorstFirst lists: Unsafe, Questionable, Good, Excellent, boiled.
    static int[] Water(int unsafeL = 0, int questionable = 0, int good = 0, int excellent = 0, int boiled = 0) =>
        new[] { unsafeL, questionable, good, excellent, boiled };

    // --- The constants ---

    [Test]
    public void TheHopperCostsThreeLogsAndFourTallGrassAndTwentyBlows()
    {
        Assert.That(PotashRules.HopperLogs, Is.EqualTo(3));
        Assert.That(PotashRules.HopperTallGrass, Is.EqualTo(4));
        Assert.That(PotashRules.HopperBuildBlows, Is.EqualTo(20));
    }

    [Test]
    public void TheWaterIsListedWorstFirstAndTheBoiledWaterLast()
    {
        Assert.That(PotashRules.WaterWorstFirst, Is.EqualTo(new[] { "water_unsafe", "water_questionable", "water_good", "water_excellent", "water" }));
    }

    // --- A load ---

    [Test]
    public void ALoadTakesTheWorstWaterFirst()
    {
        PotashRules.LoadPlan plan = PotashRules.PlanLoad(12, Water(unsafeL: 3, questionable: 4, good: 10, excellent: 5), 12);
        Assert.That(plan.lye, Is.EqualTo(12));
        Assert.That(plan.ash, Is.EqualTo(12));
        Assert.That(plan.water, Is.EqualTo(new[] { 3, 4, 5, 0, 0 }), "all the Unsafe, all the Questionable, then 5 of the Good");
    }

    [Test]
    public void TheGoodWaterIsOnlyTouchedWhenThePoorWaterRunsOut()
    {
        PotashRules.LoadPlan plan = PotashRules.PlanLoad(5, Water(unsafeL: 9, good: 9), 12);
        Assert.That(plan.lye, Is.EqualTo(5));
        Assert.That(plan.water, Is.EqualTo(new[] { 5, 0, 0, 0, 0 }));
    }

    [Test]
    public void BoiledWaterIsUsedLast()
    {
        PotashRules.LoadPlan plan = PotashRules.PlanLoad(6, Water(unsafeL: 1, excellent: 1, boiled: 10), 12);
        Assert.That(plan.water, Is.EqualTo(new[] { 1, 0, 0, 1, 4 }));
    }

    [TestCase(30, 30, 12, 12)]
    [TestCase(30, 30, 7, 7)]
    [TestCase(30, 30, 0, 0)]
    [TestCase(3, 30, 12, 3)]
    [TestCase(30, 4, 12, 4)]
    [TestCase(0, 30, 12, 0)]
    [TestCase(30, 0, 12, 0)]
    public void ALoadIsCappedByTheAshTheWaterAndTheRoom(int ash, int litres, int room, int lye)
    {
        PotashRules.LoadPlan plan = PotashRules.PlanLoad(ash, Water(good: litres), room);
        Assert.That(plan.lye, Is.EqualTo(lye));
        Assert.That(plan.ash, Is.EqualTo(lye));
        Assert.That(plan.water.Sum(), Is.EqualTo(lye), "1 L of water for each Lye");
    }

    [Test]
    public void ALoadWithNothingCarriedIsEmpty()
    {
        PotashRules.LoadPlan none = PotashRules.PlanLoad(5, null, 12);
        Assert.That(none.lye, Is.EqualTo(0));
        Assert.That(none.water.Sum(), Is.EqualTo(0));
        Assert.That(PotashRules.PlanLoad(-4, Water(good: 3), 12).lye, Is.EqualTo(0));
        Assert.That(PotashRules.PlanLoad(4, Water(good: -3), 12).lye, Is.EqualTo(0));
    }

    [Test]
    public void TheRoomIsWhatIsLeftOfTwelveAfterTheAshAndLyeAlreadyInIt()
    {
        Assert.That(PotashRules.Room(0), Is.EqualTo(12));
        Assert.That(PotashRules.Room(5), Is.EqualTo(7));
        Assert.That(PotashRules.Room(12), Is.EqualTo(0));
        Assert.That(PotashRules.Room(20), Is.EqualTo(0));
        Assert.That(PotashRules.Room(-3), Is.EqualTo(12));
    }

    [Test]
    public void LyeWaitingToBeCollectedTakesRoomFromTheNextLoad()
    {
        PotashRules.LoadPlan plan = PotashRules.PlanLoad(12, Water(good: 12), PotashRules.Room(9));
        Assert.That(plan.lye, Is.EqualTo(3));
    }

    // --- The leach clock ---

    [Test]
    public void ABatchLeachesOneGameHourAtATimeForTwentyFour()
    {
        float left = PotashRules.LeachGameHours;
        int hours = 0;
        while (left > 0f)
        {
            left = PotashRules.LeachAfterHour(left);
            hours++;
        }
        Assert.That(hours, Is.EqualTo(24));
        Assert.That(PotashRules.LeachAfterHour(0f), Is.EqualTo(0f), "nothing running, nothing to count down");
        Assert.That(PotashRules.LeachAfterHour(0.4f), Is.EqualTo(0f));
        Assert.That(PotashRules.LeachAfterHour(-2f), Is.EqualTo(0f));
    }

    [Test]
    public void ABatchFinishesOnTheHourItReachesZeroAndNotBefore()
    {
        Assert.That(PotashRules.FinishedThisHour(1f, 0f), Is.True);
        Assert.That(PotashRules.FinishedThisHour(2f, 1f), Is.False);
        Assert.That(PotashRules.FinishedThisHour(0f, 0f), Is.False, "an idle hopper does not finish again");
        Assert.That(PotashRules.FinishedThisHour(24f, 23f), Is.False);
    }

    [Test]
    public void SleepingATwentyFourHourBatchOutFinishesItInOneReplay()
    {
        float left = PotashRules.LeachGameHours;
        bool finished = false;
        for (int hour = 0; hour < 24; hour++)
        {
            float before = left;
            left = PotashRules.LeachAfterHour(before);
            finished |= PotashRules.FinishedThisHour(before, left);
        }
        Assert.That(finished, Is.True);
        Assert.That(left, Is.EqualTo(0f));
    }

    [TestCase(24f, "24 h")]
    [TestCase(23.5f, "24 h")]
    [TestCase(1f, "1 h")]
    [TestCase(0.2f, "1 h")]
    [TestCase(0f, "0 h")]
    public void TheHoursToGoAreRoundedUp(float left, string text)
    {
        Assert.That(PotashRules.HoursText(left), Is.EqualTo(text));
    }

    // --- The words ---

    [Test]
    public void TheHopperMessages()
    {
        Assert.That(PotashRules.LoadedMessage(12, 12, 12), Is.EqualTo("Loaded 12 Ash and 12 L of water - 12 Lye in 24 h"));
        Assert.That(PotashRules.LoadRefusedWhileLeaching(10.5f), Is.EqualTo("The hopper is still leaching - 11 h to go. Collect the Lye when it is done."));
    }

    [Test]
    public void TheLyeRowSaysWhatIsMissing()
    {
        Assert.That(PotashRules.RowInfo(TripodRules.Gate.NoFire, 9, 6), Is.EqualTo("needs a burning campfire"));
        Assert.That(PotashRules.RowInfo(TripodRules.Gate.NoTripod, 9, 6), Is.EqualTo("needs the Potash Kettle hung on a tripod"));
        Assert.That(PotashRules.RowInfo(TripodRules.Gate.NoVessel, 9, 6), Is.EqualTo("needs the Potash Kettle hung on a tripod"));
        Assert.That(PotashRules.RowInfo(TripodRules.Gate.WrongVessel, 9, 6), Is.EqualTo("hang the Potash Kettle (another vessel is on the tripod)"), "the Pot is hung");
    }

    [Test]
    public void TheLyeRowSaysWhatABoilWouldMake()
    {
        Assert.That(PotashRules.RowInfo(TripodRules.Gate.Ok, 2, 6), Is.EqualTo("needs 3 Lye and 2 Firewood (have 2 Lye and 6 Firewood)"));
        Assert.That(PotashRules.RowInfo(TripodRules.Gate.Ok, 9, 1), Is.EqualTo("needs 3 Lye and 2 Firewood (have 9 Lye and 1 Firewood)"));
        Assert.That(PotashRules.RowInfo(TripodRules.Gate.Ok, 3, 2), Is.EqualTo("3 Lye + 2 Firewood -> 1 Potash"));
        Assert.That(PotashRules.RowInfo(TripodRules.Gate.Ok, 12, 8), Is.EqualTo("12 Lye + 8 Firewood -> 4 Potash"));
        Assert.That(PotashRules.RowInfo(TripodRules.Gate.Ok, 7, 20), Is.EqualTo("6 Lye + 4 Firewood -> 2 Potash (1 Lye, 16 Firewood left)"));
        Assert.That(PotashRules.NeedMessage(-1, -1), Is.EqualTo("needs 3 Lye and 2 Firewood (have 0 Lye and 0 Firewood)"));
        Assert.That(PotashRules.BoiledMessage(2), Is.EqualTo("Boiled 6 Lye and 4 Firewood into 2 Potash"));
    }

    [Test]
    public void TheKettleGateUsesTheSameWordsAsThePotsBoilsWithTheKettleAsTheVessel()
    {
        string kettle = "Potash Kettle";
        Assert.That(TripodRules.GateMessage(TripodRules.Gate.NoVessel, "Boiling lye", kettle), Is.EqualTo("Boiling lye needs the Potash Kettle hung on the tripod."));
        Assert.That(TripodRules.GateMessage(TripodRules.Gate.NoTripod, "Boiling lye", kettle), Is.EqualTo("Boiling lye needs a tripod with the Potash Kettle hung on it."));
        Assert.That(TripodRules.PausedMessage("Boiling", kettle), Is.EqualTo("Boiling paused — hang the Potash Kettle on a tripod at this fire"));
        // A hung Kettle blocks the Pot's boils and a hung Pot blocks the Kettle's, each naming the other as "another vessel".
        Assert.That(TripodRules.BoilGate(true, true, TripodRules.KettleId, TripodRules.PotId), Is.EqualTo(TripodRules.Gate.WrongVessel));
        Assert.That(TripodRules.BoilGate(true, true, TripodRules.PotId, TripodRules.KettleId), Is.EqualTo(TripodRules.Gate.WrongVessel));
        Assert.That(TripodRules.BoilGate(true, true, TripodRules.KettleId, TripodRules.KettleId), Is.EqualTo(TripodRules.Gate.Ok));
        Assert.That(TripodRules.RowInfo(TripodRules.Gate.WrongVessel, "Cooking Pot"), Is.EqualTo("hang the Cooking Pot (another vessel is on the tripod)"), "the Pot rows are correct with a Kettle hung");
    }
}
