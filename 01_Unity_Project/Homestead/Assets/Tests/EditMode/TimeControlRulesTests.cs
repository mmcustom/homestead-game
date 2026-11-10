using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

// EditMode tests for player time control (Phase 2a): how long each Wait runs, when a Wait cannot start, every reason one stops early and its threshold, the words said, and the stop
// rules' behaviour at the edges (a change stops a Wait, a condition already there does not).
public class TimeControlRulesTests
{
    // A calm, healthy world: nothing to stop for.
    static WorldWatch Calm() => new WorldWatch
    {
        hydration = 80f, hunger = 80f, warmth = 80f, health = 100f, coreC = 37f,
        severeWeather = 0, streamState = 0, litFiresNear = 0, lowestFireFuelHours = float.MaxValue,
        trapCatches = 0, notices = 0, danger = false, collapsed = false,
    };

    static StopReason Eval(WorldWatch start, WorldWatch now) => TimeControlRules.Evaluate(start, now, out _);

    // --- How long ---

    [TestCase(WaitKind.OneHour, 1f)]
    [TestCase(WaitKind.ThreeHours, 3f)]
    [TestCase(WaitKind.SixHours, 6f)]
    public void TheFixedWaitsRunTheirHoursWhateverTheTimeOfDay(WaitKind kind, float hours)
    {
        foreach (float now in new[] { 0f, 5.9f, 12f, 23.75f })
            Assert.That(TimeControlRules.HoursFor(kind, now, 6f, 20f), Is.EqualTo(hours));
    }

    [TestCase(12f, 6f, 20f, 18f, 8f)]     // noon: dawn is 18 h away, dusk 8 h
    [TestCase(5f, 6f, 20f, 1f, 15f)]      // before dawn
    [TestCase(21f, 6f, 20f, 9f, 23f)]     // after dusk
    [TestCase(6f, 6f, 20f, 24f, 14f)]     // exactly dawn: the next one
    [TestCase(20f, 6f, 20f, 10f, 24f)]    // exactly dusk: the next one
    [TestCase(19.999f, 6f, 20f, 10.001f, 24f)] // a few seconds before dusk is still "now": a day on
    [TestCase(4.5f, 4.5f, 21.5f, 24f, 17f)]   // Summer's hours
    [TestCase(0f, 7f, 18f, 7f, 18f)]      // midnight in Winter's hours
    public void DawnAndDuskAreTheNextSunriseAndSunset(float now, float sunrise, float sunset, float toDawn, float toDusk)
    {
        Assert.That(TimeControlRules.HoursFor(WaitKind.UntilDawn, now, sunrise, sunset), Is.EqualTo(toDawn).Within(0.002f));
        Assert.That(TimeControlRules.HoursFor(WaitKind.UntilDusk, now, sunrise, sunset), Is.EqualTo(toDusk).Within(0.002f));
    }

    [Test]
    public void NoWaitIsLongerThanADay()
    {
        for (float now = 0f; now < 24f; now += 0.37f)
            foreach (WaitKind kind in TimeControlRules.Kinds)
                Assert.That(TimeControlRules.HoursFor(kind, now, 6f, 20f), Is.InRange(0f, TimeControlRules.MaxWaitHours));
    }

    [Test]
    public void TheMenuOffersFiveWaitsInOrderWithPlainLabels()
    {
        Assert.That(TimeControlRules.Kinds, Is.EqualTo(new[] { WaitKind.OneHour, WaitKind.ThreeHours, WaitKind.SixHours, WaitKind.UntilDawn, WaitKind.UntilDusk }));
        Assert.That(TimeControlRules.Kinds.Select(TimeControlRules.Label),
            Is.EqualTo(new[] { "Wait 1 hour", "Wait 3 hours", "Wait 6 hours", "Wait until dawn", "Wait until dusk" }));
    }

    // --- Gating ---

    [Test]
    public void ACalmPlayerInPlainPlayCanWait()
    {
        Assert.That(TimeControlRules.CantWait(true, false, false, Calm()), Is.Null);
    }

    [Test]
    public void WaitIsRefusedAsleepCollapsedInAMenuOrInTheWater()
    {
        Assert.That(TimeControlRules.CantWait(true, true, false, Calm()), Is.EqualTo("You can't wait now."), "asleep (or already waiting)");
        WorldWatch collapsed = Calm();
        collapsed.collapsed = true;
        Assert.That(TimeControlRules.CantWait(true, false, false, collapsed), Is.EqualTo("You can't wait now."));
        Assert.That(TimeControlRules.CantWait(false, false, false, Calm()), Is.EqualTo("Not now."), "a menu, a pause, a load: not plain play");
        Assert.That(TimeControlRules.CantWait(true, false, true, Calm()), Is.EqualTo("You can't wait in the water."), "the creek or a pond");
    }

    [Test]
    public void TheOrderOfRefusalsIsAsleepThenMenuThenWaterThenStats()
    {
        WorldWatch hungry = Calm();
        hungry.hunger = 5f;
        Assert.That(TimeControlRules.CantWait(false, true, true, hungry), Is.EqualTo("You can't wait now."));
        Assert.That(TimeControlRules.CantWait(false, false, true, hungry), Is.EqualTo("Not now."));
        Assert.That(TimeControlRules.CantWait(true, false, true, hungry), Is.EqualTo("You can't wait in the water."));
        Assert.That(TimeControlRules.CantWait(true, false, false, hungry), Is.EqualTo("You're too hungry to wait."));
    }

    [TestCase("hydration", 24.9f, "thirsty")]
    [TestCase("hunger", 24.9f, "hungry")]
    [TestCase("warmth", 24.9f, "cold")]
    [TestCase("health", 24.9f, "hurt")]
    [TestCase("hydration", 0f, "thirsty")]
    public void AStatUnderTwentyFiveIsLowAndStopsAWaitFromStarting(string stat, float value, string word)
    {
        WorldWatch w = WithStat(stat, value);
        Assert.That(TimeControlRules.LowStatText(w), Is.EqualTo(word));
        Assert.That(TimeControlRules.CantWait(true, false, false, w), Is.EqualTo("You're too " + word + " to wait."));
    }

    [TestCase("hydration", 25f)]
    [TestCase("hunger", 25f)]
    [TestCase("warmth", 25f)]
    [TestCase("health", 25f)]
    [TestCase("hydration", 100f)]
    public void ExactlyTwentyFiveIsNotLowYet(string stat, float value)
    {
        Assert.That(TimeControlRules.LowStatText(WithStat(stat, value)), Is.Null);
    }

    [Test]
    public void TooHotIsACoreTemperatureAboveThirtyEight()
    {
        WorldWatch w = Calm();
        w.coreC = 38f;
        Assert.That(TimeControlRules.LowStatText(w), Is.Null, "38.0 is the edge, not over it");
        w.coreC = 38.1f;
        Assert.That(TimeControlRules.LowStatText(w), Is.EqualTo("hot"));
        Assert.That(TimeControlRules.CantWait(true, false, false, w), Is.EqualTo("You're too hot to wait."));
    }

    [Test]
    public void DangerNearRefusesAWait()
    {
        WorldWatch w = Calm();
        w.danger = true;
        Assert.That(TimeControlRules.CantWait(true, false, false, w), Is.Not.Null);
    }

    static WorldWatch WithStat(string stat, float value)
    {
        WorldWatch w = Calm();
        switch (stat)
        {
            case "hydration": w.hydration = value; break;
            case "hunger": w.hunger = value; break;
            case "warmth": w.warmth = value; break;
            default: w.health = value; break;
        }
        return w;
    }

    // --- Stopping: every reason ---

    [Test]
    public void NothingChangingNeverStopsAWait()
    {
        Assert.That(Eval(Calm(), Calm()), Is.EqualTo(StopReason.None));
    }

    [TestCase("hydration")]
    [TestCase("hunger")]
    [TestCase("warmth")]
    [TestCase("health")]
    public void AStatFallingUnderTwentyFiveStopsTheWait(string stat)
    {
        StopReason reason = TimeControlRules.Evaluate(Calm(), WithStat(stat, 24f), out string detail);
        Assert.That(reason, Is.EqualTo(StopReason.StatLow));
        StringAssert.Contains("you're", detail);
        Assert.That(Eval(Calm(), WithStat(stat, 25f)), Is.EqualTo(StopReason.None), "25 is not yet low");
    }

    [Test]
    public void ACoreTemperatureOverThirtyEightStopsTheWait()
    {
        WorldWatch now = Calm();
        now.coreC = 38.5f;
        Assert.That(TimeControlRules.Evaluate(Calm(), now, out string detail), Is.EqualTo(StopReason.StatLow));
        StringAssert.Contains("overheating", detail);
    }

    [Test]
    public void SevereWeatherArrivingStopsTheWaitAndNamesIt()
    {
        WorldWatch now = Calm();
        now.severeWeather = 5;
        now.severeName = "a thunderstorm";
        StopReason reason = TimeControlRules.Evaluate(Calm(), now, out string detail);
        Assert.That(reason, Is.EqualTo(StopReason.SevereWeather));
        Assert.That(detail, Is.EqualTo("a thunderstorm is here"));
    }

    [Test]
    public void SevereWeatherAlreadyFallingDoesNotStopAWaitButADifferentOneDoes()
    {
        WorldWatch storm = Calm();
        storm.severeWeather = 5;
        storm.severeName = "a thunderstorm";
        Assert.That(Eval(storm, storm), Is.EqualTo(StopReason.None), "waiting in a storm you are already in");
        WorldWatch snow = Calm();
        snow.severeWeather = 7;
        snow.severeName = "snow";
        Assert.That(Eval(storm, snow), Is.EqualTo(StopReason.SevereWeather));
        Assert.That(Eval(storm, Calm()), Is.EqualTo(StopReason.None), "the storm passing is no reason to stop");
    }

    [Test]
    public void TheCreekRisingStopsTheWaitOnEachStepUp()
    {
        WorldWatch high = Calm(), flood = Calm();
        high.streamState = 1;
        flood.streamState = 2;
        Assert.That(TimeControlRules.Evaluate(Calm(), high, out string d1), Is.EqualTo(StopReason.Flood));
        Assert.That(d1, Is.EqualTo("the creek is rising"));
        Assert.That(TimeControlRules.Evaluate(Calm(), flood, out string d2), Is.EqualTo(StopReason.Flood));
        Assert.That(d2, Is.EqualTo("the creek is in flood"));
        Assert.That(Eval(high, flood), Is.EqualTo(StopReason.Flood), "high water to a flood is another rise");
    }

    [Test]
    public void ACreekAlreadyHighOrFallingDoesNotStopAWait()
    {
        WorldWatch high = Calm(), flood = Calm();
        high.streamState = 1;
        flood.streamState = 2;
        Assert.That(Eval(high, high), Is.EqualTo(StopReason.None));
        Assert.That(Eval(flood, flood), Is.EqualTo(StopReason.None));
        Assert.That(Eval(flood, high), Is.EqualTo(StopReason.None), "draining");
        Assert.That(Eval(high, Calm()), Is.EqualTo(StopReason.None));
    }

    [Test]
    public void AFireFallingToHalfAnHourOfFuelIsDyingAndStopsTheWait()
    {
        WorldWatch start = Calm(), now = Calm();
        start.litFiresNear = now.litFiresNear = 1;
        start.lowestFireFuelHours = 4f;
        now.lowestFireFuelHours = 0.5f;
        Assert.That(TimeControlRules.Evaluate(start, now, out string detail), Is.EqualTo(StopReason.FireDying), "half an hour exactly is dying");
        Assert.That(detail, Is.EqualTo("the fire is dying"));
        now.lowestFireFuelHours = 0.51f;
        Assert.That(Eval(start, now), Is.EqualTo(StopReason.None));
    }

    [Test]
    public void AFireGoingOutStopsTheWait()
    {
        WorldWatch start = Calm();
        start.litFiresNear = 1;
        start.lowestFireFuelHours = 0.2f; // already dying when the Wait began
        Assert.That(TimeControlRules.Evaluate(start, Calm(), out string detail), Is.EqualTo(StopReason.FireDying));
        Assert.That(detail, Is.EqualTo("a fire has gone out"));
    }

    [Test]
    public void AFireAlreadyDyingAtTheStartDoesNotStopTheWaitUntilItGoesOut()
    {
        WorldWatch start = Calm(), now = Calm();
        start.litFiresNear = now.litFiresNear = 1;
        start.lowestFireFuelHours = 0.3f;
        now.lowestFireFuelHours = 0.1f;
        Assert.That(Eval(start, now), Is.EqualTo(StopReason.None));
    }

    [Test]
    public void NoFireNearNeverStopsAWaitForFuel()
    {
        WorldWatch now = Calm();
        now.lowestFireFuelHours = 0f; // meaningless with no fire
        Assert.That(Eval(Calm(), now), Is.EqualTo(StopReason.None));
    }

    [Test]
    public void AFireComingIntoReachAlreadyLowCountsAsDying()
    {
        WorldWatch start = Calm(), now = Calm();
        now.litFiresNear = 1;
        now.lowestFireFuelHours = 0.4f;
        Assert.That(Eval(start, now), Is.EqualTo(StopReason.FireDying));
    }

    [Test]
    public void ATrapCatchingSomethingStopsTheWait()
    {
        WorldWatch now = Calm();
        now.trapCatches = 1;
        Assert.That(TimeControlRules.Evaluate(Calm(), now, out string detail), Is.EqualTo(StopReason.TrapCatch));
        Assert.That(detail, Is.EqualTo("a trap has caught something"));
        WorldWatch start = Calm();
        start.trapCatches = 1;
        Assert.That(Eval(start, now), Is.EqualTo(StopReason.None), "a catch already waiting when the Wait began");
        now.trapCatches = 2;
        Assert.That(Eval(start, now), Is.EqualTo(StopReason.TrapCatch), "a second one");
    }

    [Test]
    public void ABannerArrivingStopsTheWait()
    {
        WorldWatch start = Calm(), now = Calm();
        start.notices = 40;
        now.notices = 41;
        Assert.That(TimeControlRules.Evaluate(start, now, out string detail), Is.EqualTo(StopReason.Notice));
        Assert.That(detail, Is.EqualTo("there is a notice"));
        now.notices = 40;
        Assert.That(Eval(start, now), Is.EqualTo(StopReason.None));
    }

    [Test]
    public void DangerAppearingStopsTheWaitAndCollapseAlwaysDoes()
    {
        WorldWatch now = Calm();
        now.danger = true;
        Assert.That(Eval(Calm(), now), Is.EqualTo(StopReason.Danger));
        Assert.That(Eval(now, now), Is.EqualTo(StopReason.None), "already there");
        WorldWatch down = Calm();
        down.collapsed = true;
        Assert.That(Eval(Calm(), down), Is.EqualTo(StopReason.Collapsed));
        Assert.That(Eval(down, down), Is.EqualTo(StopReason.Collapsed), "never carries on after a collapse");
    }

    [Test]
    public void WhenSeveralThingsHappenInOneStepTheMostUrgentIsReported()
    {
        WorldWatch now = Calm();
        now.collapsed = true;
        now.danger = true;
        now.hunger = 1f;
        now.severeWeather = 5;
        now.streamState = 2;
        now.trapCatches = 3;
        now.notices = 9;
        Assert.That(Eval(Calm(), now), Is.EqualTo(StopReason.Collapsed));
        now.collapsed = false;
        Assert.That(Eval(Calm(), now), Is.EqualTo(StopReason.Danger));
        now.danger = false;
        Assert.That(Eval(Calm(), now), Is.EqualTo(StopReason.StatLow));
        now.hunger = 80f;
        Assert.That(Eval(Calm(), now), Is.EqualTo(StopReason.SevereWeather));
        now.severeWeather = 0;
        Assert.That(Eval(Calm(), now), Is.EqualTo(StopReason.Flood));
        now.streamState = 0;
        Assert.That(Eval(Calm(), now), Is.EqualTo(StopReason.TrapCatch));
        now.trapCatches = 0;
        Assert.That(Eval(Calm(), now), Is.EqualTo(StopReason.Notice));
    }

    [Test]
    public void TheFloodBannerIsAFloodNotJustANotice()
    {
        // A flood raises a banner too ("The creek is in flood."); the creek is the better reason to give.
        WorldWatch now = Calm();
        now.streamState = 2;
        now.notices = 1;
        Assert.That(Eval(Calm(), now), Is.EqualTo(StopReason.Flood));
    }

    // --- Words ---

    [TestCase(0f, "no time")]
    [TestCase(0.1f, "no time")]
    [TestCase(0.25f, "0.25 hours")]
    [TestCase(0.5f, "0.5 hours")]
    [TestCase(0.75f, "0.75 hours")]
    [TestCase(1f, "1 hour")]
    [TestCase(1.25f, "1.25 hours")]
    [TestCase(1.5f, "1.5 hours")]
    [TestCase(2f, "2 hours")]
    [TestCase(6f, "6 hours")]
    [TestCase(9.75f, "9.75 hours")]
    public void HoursReadPlainly(float hours, string text)
    {
        Assert.That(TimeControlRules.HoursText(hours), Is.EqualTo(text));
    }

    [Test]
    public void ADoneWaitSaysHowLongItRan()
    {
        Assert.That(TimeControlRules.DoneMessage(3f), Is.EqualTo("Waited 3 hours."));
        Assert.That(TimeControlRules.DoneMessage(1f), Is.EqualTo("Waited 1 hour."));
    }

    [Test]
    public void AnEarlyStopIsOneShortLineWithWhyAndHowLong()
    {
        string line = TimeControlRules.StoppedMessage(StopReason.Flood, "the creek is in flood", 2.5f);
        Assert.That(line, Is.EqualTo("Stopped waiting after 2.5 hours: the creek is in flood."));
        Assert.That(line, Does.Not.Contain("\n"));
        Assert.That(line.Length, Is.LessThan(80));
        Assert.That(TimeControlRules.StoppedMessage(StopReason.Notice, null, 1f), Is.EqualTo("Stopped waiting after 1 hour: something happened."));
    }

    [Test]
    public void EveryReasonsMessageStaysOneShortLine()
    {
        foreach (WorldWatch now in EveryStop())
        {
            StopReason reason = TimeControlRules.Evaluate(Calm(), now, out string detail);
            Assert.That(reason, Is.Not.EqualTo(StopReason.None));
            string line = TimeControlRules.StoppedMessage(reason, detail, 1.75f);
            Assert.That(line, Does.StartWith("Stopped waiting after 1.75 hours: "));
            Assert.That(line.Length, Is.LessThan(90), line);
        }
    }

    static IEnumerable<WorldWatch> EveryStop()
    {
        WorldWatch w;
        w = Calm(); w.collapsed = true; yield return w;
        w = Calm(); w.danger = true; yield return w;
        w = Calm(); w.hydration = 3f; yield return w;
        w = Calm(); w.coreC = 39f; yield return w;
        w = Calm(); w.severeWeather = 6; w.severeName = "a cold front"; yield return w;
        w = Calm(); w.streamState = 2; yield return w;
        w = Calm(); w.litFiresNear = 1; w.lowestFireFuelHours = 0.1f; yield return w;
        w = Calm(); w.trapCatches = 1; yield return w;
        w = Calm(); w.notices = 1; yield return w;
    }
}
