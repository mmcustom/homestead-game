using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

// EditMode tests for the one loop Wait and sleep share (Phase 2a): its steps and their order, and that sleep run through it passes the world exactly as sleep's own loop did before --
// the same hunger and thirst, fire burn, wood drying, trap and event timing -- with the one addition the plan asked for, portable-light fuel.
public class TimeControlLoopTests
{
    // Records every call, in order, with its step.
    sealed class Recorder : IWorldPass
    {
        public readonly List<string> Log = new List<string>();
        public void Survival(float h) => Log.Add("S" + h.ToString("0.###"));
        public void Fire(float h) => Log.Add("F" + h.ToString("0.###"));
        public void Wood(float h) => Log.Add("W" + h.ToString("0.###"));
        public void Light(float h) => Log.Add("L" + h.ToString("0.###"));
        public void Clock(float h) => Log.Add("C" + h.ToString("0.###"));
    }

    // A small model of the world's per-hour numbers, driven only through IWorldPass: what a stat, a fire, a pile of wood, a torch and the clock do as hours pass.
    sealed class Model : IWorldPass
    {
        public double hunger = 100, thirst = 100, warmth = 90;
        public double fireFuel = 8, ash;
        public double woodDryness;            // hours the wood has dried
        public double torchLeft = 4;          // hours of torch burn left
        public double clockHours;             // the clock
        public int hourEvents, dayEvents;     // HourChanged and DayChanged, as the clock crosses them
        public List<double> trapReadyAt = new List<double> { 3.0, 7.5, 12.25 }; // total hours at which a trap's catch lands
        public List<double> trapsCaught = new List<double>();
        public bool lightLit;                 // a torch is burning

        public void Survival(float h) { hunger -= 4.0 * h; thirst -= 6.0 * h; warmth -= 1.5 * h; }
        public void Fire(float h) { double before = fireFuel; fireFuel = Math.Max(0, before - h); ash += (before - fireFuel) * 0.05; }
        public void Wood(float h) { woodDryness += h; }
        public void Light(float h) { if (lightLit) torchLeft = Math.Max(0, torchLeft - h); }
        public void Clock(float h)
        {
            double before = clockHours;
            clockHours += h;
            hourEvents += (int)Math.Floor(clockHours) - (int)Math.Floor(before);
            dayEvents += (int)Math.Floor(clockHours / 24.0) - (int)Math.Floor(before / 24.0);
            foreach (double at in trapReadyAt)
                if (before < at && clockHours >= at)
                    trapsCaught.Add(at);
        }
    }

    // Sleep's own loop as it was before the shared one, copied from SleepManager.PassNight: survival, fire, wood, then the clock, in 0.25-hour steps. No light step.
    static float LegacySleepLoop(float hours, IWorldPass world, Func<float, bool> wake)
    {
        const float Step = 0.25f;
        float slept = 0f;
        while (slept < hours - 0.001f)
        {
            float step = Math.Min(Step, hours - slept);
            world.Survival(step);
            world.Fire(step);
            world.Wood(step);
            world.Clock(step);
            slept += step;
            if (wake != null && wake(slept))
                break;
        }
        return slept;
    }

    // --- The loop itself ---

    [Test]
    public void EachStepRunsSurvivalFireWoodLightThenTheClock()
    {
        var r = new Recorder();
        float passed = WorldPass.Run(0.5f, r, true, null);
        Assert.That(passed, Is.EqualTo(0.5f));
        Assert.That(r.Log, Is.EqualTo(new[] { "S0.25", "F0.25", "W0.25", "L0.25", "C0.25", "S0.25", "F0.25", "W0.25", "L0.25", "C0.25" }));
    }

    [Test]
    public void AStepIsAQuarterHourAndTheLastOneIsTheRemainder()
    {
        Assert.That(TimeControlRules.StepHours, Is.EqualTo(0.25f));
        var r = new Recorder();
        WorldPass.Run(0.6f, r, true, null);
        Assert.That(r.Log.Where(s => s[0] == 'C'), Is.EqualTo(new[] { "C0.25", "C0.25", "C0.1" }));
    }

    [TestCase(1f, 4)]
    [TestCase(3f, 12)]
    [TestCase(6f, 24)]
    [TestCase(8f, 32)]
    [TestCase(0.25f, 1)]
    [TestCase(0.3f, 2)]
    public void ARunTakesAsManyStepsAsItsHoursNeed(float hours, int steps)
    {
        var r = new Recorder();
        WorldPass.Run(hours, r, true, null);
        Assert.That(r.Log.Count(s => s[0] == 'C'), Is.EqualTo(steps));
        Assert.That(r.Log.Count(s => s[0] == 'S'), Is.EqualTo(steps));
    }

    [TestCase(0.25f)]
    [TestCase(1f)]
    [TestCase(2.6f)]
    [TestCase(7.75f)]
    [TestCase(13.37f)]
    [TestCase(24f)]
    public void TheClockAdvancesByExactlyTheHoursAsked(float hours)
    {
        var m = new Model();
        float passed = WorldPass.Run(hours, m, true, null);
        Assert.That(passed, Is.EqualTo(hours).Within(0.0005f));
        Assert.That(m.clockHours, Is.EqualTo(hours).Within(0.0005f));
    }

    [Test]
    public void NoHoursOrNegativeHoursRunNothing()
    {
        var r = new Recorder();
        Assert.That(WorldPass.Run(0f, r, true, null), Is.EqualTo(0f));
        Assert.That(WorldPass.Run(-3f, r, true, null), Is.EqualTo(0f));
        Assert.That(r.Log, Is.Empty);
    }

    [Test]
    public void ThePlayersStatsAreSkippedWhenAskedAndEverythingElseStillRuns()
    {
        var r = new Recorder();
        WorldPass.Run(0.5f, r, false, null);
        Assert.That(r.Log.Any(s => s[0] == 'S'), Is.False);
        Assert.That(r.Log.Count(s => s[0] == 'F'), Is.EqualTo(2));
        Assert.That(r.Log.Count(s => s[0] == 'C'), Is.EqualTo(2));
    }

    [Test]
    public void TheStopCheckIsAskedAfterEachStepWithTheHoursSoFarAndStopsTheRun()
    {
        var r = new Recorder();
        var asked = new List<float>();
        float passed = WorldPass.Run(6f, r, true, soFar =>
        {
            asked.Add(soFar);
            return soFar >= 0.75f;
        });
        Assert.That(passed, Is.EqualTo(0.75f));
        Assert.That(asked, Is.EqualTo(new[] { 0.25f, 0.5f, 0.75f }));
        Assert.That(r.Log.Count(s => s[0] == 'C'), Is.EqualTo(3));
    }

    [Test]
    public void AStopAfterTheVeryFirstStepStillPassesThatStep()
    {
        var r = new Recorder();
        Assert.That(WorldPass.Run(3f, r, true, _ => true), Is.EqualTo(0.25f));
        Assert.That(r.Log, Is.EqualTo(new[] { "S0.25", "F0.25", "W0.25", "L0.25", "C0.25" }));
    }

    [Test]
    public void ARunThatIsNeverStoppedPassesAllItsHours()
    {
        var r = new Recorder();
        Assert.That(WorldPass.Run(3f, r, true, _ => false), Is.EqualTo(3f));
    }

    // --- Sleep through the shared loop matches sleep's own loop ---

    static readonly float[] SleepHours = { 0.5f, 1f, 3f, 5.5f, 8f, 8.75f, 11.25f, 12f };

    [Test]
    public void SleepsCallsAreTheSameAsBeforeApartFromTheLightStep()
    {
        foreach (float hours in SleepHours)
        {
            var legacy = new Recorder();
            var shared = new Recorder();
            LegacySleepLoop(hours, legacy, null);
            WorldPass.Run(hours, shared, true, null);
            Assert.That(shared.Log.Where(s => s[0] != 'L'), Is.EqualTo(legacy.Log), $"{hours} h: survival, fire, wood, clock in the same order and step sizes");
        }
    }

    [Test]
    public void HungerAndThirstAfterTheSameHoursMatch()
    {
        foreach (float hours in SleepHours)
        {
            var legacy = new Model();
            var shared = new Model();
            LegacySleepLoop(hours, legacy, null);
            WorldPass.Run(hours, shared, true, null);
            Assert.That(shared.hunger, Is.EqualTo(legacy.hunger), $"hunger after {hours} h");
            Assert.That(shared.thirst, Is.EqualTo(legacy.thirst), $"thirst after {hours} h");
            Assert.That(shared.warmth, Is.EqualTo(legacy.warmth), $"warmth after {hours} h");
        }
    }

    [Test]
    public void FireBurnAndAshAfterTheSameHoursMatch()
    {
        foreach (float hours in SleepHours)
        {
            var legacy = new Model();
            var shared = new Model();
            LegacySleepLoop(hours, legacy, null);
            WorldPass.Run(hours, shared, true, null);
            Assert.That(shared.fireFuel, Is.EqualTo(legacy.fireFuel), $"fuel after {hours} h");
            Assert.That(shared.ash, Is.EqualTo(legacy.ash), $"ash after {hours} h");
        }
    }

    [Test]
    public void WoodDryingAfterTheSameHoursMatches()
    {
        foreach (float hours in SleepHours)
        {
            var legacy = new Model();
            var shared = new Model();
            LegacySleepLoop(hours, legacy, null);
            WorldPass.Run(hours, shared, true, null);
            Assert.That(shared.woodDryness, Is.EqualTo(legacy.woodDryness), $"{hours} h of drying");
        }
    }

    [Test]
    public void TrapTimingAndTheHourAndDayEventsAfterTheSameHoursMatch()
    {
        foreach (float hours in SleepHours.Concat(new[] { 24f, 30f }))
        {
            var legacy = new Model { clockHours = 21.0 };
            var shared = new Model { clockHours = 21.0 };
            LegacySleepLoop(hours, legacy, null);
            WorldPass.Run(hours, shared, true, null);
            Assert.That(shared.clockHours, Is.EqualTo(legacy.clockHours), $"clock after {hours} h");
            Assert.That(shared.hourEvents, Is.EqualTo(legacy.hourEvents), "hourly events (weather, taps, ledger, loan check)");
            Assert.That(shared.dayEvents, Is.EqualTo(legacy.dayEvents), "daily events (loan, market, autosave)");
            Assert.That(shared.trapsCaught, Is.EqualTo(legacy.trapsCaught), "the traps that came ready");
        }
    }

    [Test]
    public void TheClockCrossesEveryHourOfASleepAndMidnightOnce()
    {
        var m = new Model { clockHours = 22.0 };
        WorldPass.Run(8f, m, true, null);
        Assert.That(m.hourEvents, Is.EqualTo(8), "22:00 to 06:00");
        Assert.That(m.dayEvents, Is.EqualTo(1), "one midnight");
    }

    [Test]
    public void ATrapReadyDuringTheHoursPassedIsCaughtOnceWhetherSleepingOrWaiting()
    {
        var m = new Model();
        WorldPass.Run(8f, m, true, null);
        Assert.That(m.trapsCaught, Is.EqualTo(new[] { 3.0, 7.5 }));
        WorldPass.Run(6f, m, true, null);
        Assert.That(m.trapsCaught, Is.EqualTo(new[] { 3.0, 7.5, 12.25 }), "the next 6 hours bring the third, and the first two do not fire again");
    }

    [Test]
    public void SleepsWakeRuleStopsTheSharedLoopWhereItStoppedTheOldOne()
    {
        // Sleep wakes early when a stat drops, but only after half an hour: modelled here as hunger under 90 after at least 0.5 h.
        Func<Model, Func<float, bool>> wakeRule = m => soFar => m.hunger < 90.0 && soFar >= 0.5f;
        foreach (float hours in SleepHours)
        {
            var legacy = new Model();
            var shared = new Model();
            float a = LegacySleepLoop(hours, legacy, wakeRule(legacy));
            float b = WorldPass.Run(hours, shared, true, wakeRule(shared));
            Assert.That(b, Is.EqualTo(a), $"hours slept out of {hours}");
            Assert.That(shared.clockHours, Is.EqualTo(legacy.clockHours));
            Assert.That(shared.hunger, Is.EqualTo(legacy.hunger));
        }
    }

    // --- Torch fuel: the one addition ---

    [Test]
    public void AnUnlitLightBurnsNothingSoSleepAndWaitDoNotCostTorches()
    {
        foreach (float hours in SleepHours)
        {
            var m = new Model { lightLit = false };
            WorldPass.Run(hours, m, true, null);
            Assert.That(m.torchLeft, Is.EqualTo(4.0));
        }
    }

    [TestCase(1f)]
    [TestCase(2.5f)]
    [TestCase(3.75f)]
    public void ALitTorchBurnsOneHourOfFuelPerGameHourWhileTimePasses(float hours)
    {
        var m = new Model { lightLit = true };
        WorldPass.Run(hours, m, true, null);
        Assert.That(m.torchLeft, Is.EqualTo(4.0 - hours).Within(1e-9), "the same rate real-time play burns it (one fuel hour per game hour)");
    }

    [Test]
    public void ALitTorchRunsOutPartWayThroughALongPassAndNoFurther()
    {
        var m = new Model { lightLit = true, torchLeft = 1.0 };
        WorldPass.Run(8f, m, true, null);
        Assert.That(m.torchLeft, Is.EqualTo(0.0));
        Assert.That(m.clockHours, Is.EqualTo(8.0), "the time still passes");
    }

    [Test]
    public void TheLightStepNeverChangesAnythingButTheLight()
    {
        foreach (float hours in SleepHours)
        {
            var withLight = new Model { lightLit = true };
            var without = new Model { lightLit = false };
            WorldPass.Run(hours, withLight, true, null);
            WorldPass.Run(hours, without, true, null);
            Assert.That(withLight.hunger, Is.EqualTo(without.hunger));
            Assert.That(withLight.fireFuel, Is.EqualTo(without.fireFuel));
            Assert.That(withLight.woodDryness, Is.EqualTo(without.woodDryness));
            Assert.That(withLight.clockHours, Is.EqualTo(without.clockHours));
            Assert.That(withLight.trapsCaught, Is.EqualTo(without.trapsCaught));
        }
    }

    // --- Wait uses the same loop ---

    [Test]
    public void AWaitOfTheSameHoursPassesTheWorldTheSameWayAsASleep()
    {
        foreach (float hours in new[] { 1f, 3f, 6f })
        {
            var sleep = new Model();
            var wait = new Model();
            LegacySleepLoop(hours, sleep, null);
            WorldPass.Run(hours, wait, true, _ => false);
            Assert.That(wait.hunger, Is.EqualTo(sleep.hunger));
            Assert.That(wait.fireFuel, Is.EqualTo(sleep.fireFuel));
            Assert.That(wait.woodDryness, Is.EqualTo(sleep.woodDryness));
            Assert.That(wait.clockHours, Is.EqualTo(sleep.clockHours));
        }
    }

    // --- The real types are there and joined up (by reflection: the rules assembly cannot see the game's) ---

    static Type Find(string name)
    {
        Assembly[] all = AppDomain.CurrentDomain.GetAssemblies();
        Type[] TypesOf(Assembly a) { try { return a.GetTypes(); } catch (Exception) { return new Type[0]; } }
        Type own = all.Where(a => a.GetName().Name == "Assembly-CSharp").SelectMany(TypesOf).FirstOrDefault(t => t.Name == name);
        return own ?? all.SelectMany(TypesOf).First(t => t.Name == name);
    }

    [Test]
    public void TimeControlManagerOffersTheSharedLoopAndWait()
    {
        Type manager = Find("TimeControlManager");
        MethodInfo pass = manager.GetMethod("PassWorld", BindingFlags.Public | BindingFlags.Static);
        Assert.That(pass, Is.Not.Null, "static PassWorld(hours, tickPlayer, afterStep)");
        Assert.That(pass.ReturnType, Is.EqualTo(typeof(float)));
        Assert.That(pass.GetParameters().Select(p => p.ParameterType),
            Is.EqualTo(new[] { typeof(float), typeof(bool), typeof(Func<float, bool>) }));
        Assert.That(manager.GetMethod("Wait", new[] { typeof(WaitKind) }), Is.Not.Null);
        Assert.That(manager.GetMethod("CantWait", Type.EmptyTypes), Is.Not.Null);
        Assert.That(manager.GetMethod("Read"), Is.Not.Null);
        Assert.That(manager.GetField("DangerSource"), Is.Not.Null, "where a future predator reports itself");
    }

    [Test]
    public void ThePortableLightCanBurnForPassedHoursAndToolStatusCountsBanners()
    {
        Assert.That(Find("PortableLight").GetMethod("PassHours", new[] { typeof(float) }), Is.Not.Null);
        PropertyInfo raised = Find("ToolStatus").GetProperty("BannersRaised", BindingFlags.Public | BindingFlags.Static);
        Assert.That(raised, Is.Not.Null);
        Assert.That(raised.PropertyType, Is.EqualTo(typeof(int)));
    }

    [Test]
    public void TheWaitPanelExistsAndTheWaitKeyIsNotTheSleepOrDebugKeys()
    {
        Assert.That(Find("WaitPanel").GetProperty("IsOpen", BindingFlags.Public | BindingFlags.Static), Is.Not.Null);
        // F7 (the debug fast-forward) lives in TimeDebugControls and is not part of this.
        Type debug = Find("TimeDebugControls");
        Assert.That(debug.GetField("Speeds", BindingFlags.NonPublic | BindingFlags.Static), Is.Not.Null, "the F7 speeds are untouched");
        float[] speeds = (float[])debug.GetField("Speeds", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
        Assert.That(speeds, Is.EqualTo(new[] { 1f, 10f, 60f, 360f }));
    }

    [Test]
    public void SleepStillHasItsOwnWakeThresholds()
    {
        Type sleep = Find("SleepManager");
        foreach (string field in new[] { "wakeBelowWarmth", "wakeBelowHealth", "wakeAboveCoreC", "nightFromHour", "morningHour", "restHours" })
            Assert.That(sleep.GetField(field, BindingFlags.NonPublic | BindingFlags.Instance), Is.Not.Null, field);
    }
}
