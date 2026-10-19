using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// EditMode tests for Flash Floods phase 5 (creek crossing danger). The flow table, the timer and the health maths are pure and
// reference the Homestead.CreekCrossing assembly directly. WaterSource.ZoneAt and SurvivalManager.Injure live in the main
// assembly, which a test assembly cannot reference, so those two are reached by reflection on throwaway objects (Awake does
// not run in edit mode, so no singleton is touched). Nothing here touches the Saves folder.
public class CreekCrossingTests
{
    static CrossingTiming Timing => new CrossingTiming
    {
        knockDownAfterSeconds = 1.5f, lockoutSeconds = 1.5f, immunitySeconds = 4f, leaveGraceSeconds = 1f,
    };

    static Type Find(string name) =>
        AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => { try { return a.GetTypes(); } catch (Exception) { return new Type[0]; } })
                 .First(t => t.Name == name);

    // --- Flow direction ---

    [Test]
    public void FlowAtEveryControlPointFollowsTheCreekDownstream()
    {
        Vector2[] c = CreekFlow.ControlPoints;
        Vector2 pond = c[c.Length - 1];
        for (int i = 0; i < c.Length - 1; i++) // the pond end has no segment past it
        {
            Vector2 chord = (c[Mathf.Min(i + 1, c.Length - 1)] - c[Mathf.Max(i - 1, 0)]).normalized;
            Vector2 flow = CreekFlow.FlowAt(c[i]);
            Assert.That(flow.magnitude, Is.EqualTo(1f).Within(1e-4f), $"control {i} flow is a unit vector");
            Assert.That(Vector2.Dot(flow, chord), Is.GreaterThan(0.95f), $"control {i} flow matches the creek's direction");
            Assert.That(Vector2.Dot(flow, (pond - c[i]).normalized), Is.GreaterThan(0f), $"control {i} flows toward the pond");
        }
    }

    [Test]
    public void FlowMidSegmentPointsAlongThatSegment()
    {
        Vector2[] c = CreekFlow.ControlPoints;
        for (int i = 0; i + 1 < c.Length; i++)
        {
            int mid = (CreekFlow.ControlSample(i) + CreekFlow.ControlSample(i + 1)) / 2;
            Vector2 flow = CreekFlow.FlowAt(CreekFlow.PathPoint(mid));
            Assert.That(Vector2.Dot(flow, (c[i + 1] - c[i]).normalized), Is.GreaterThan(0.9f), $"mid segment {i}");
        }
    }

    [Test]
    public void NearestSegmentAtTheBendsIsTheOneYouAreOn()
    {
        for (int i = 1; i + 1 < CreekFlow.ControlPoints.Length; i++)
        {
            int bend = CreekFlow.ControlSample(i);
            int before = CreekFlow.NearestSegment(CreekFlow.PathPoint(bend - 5), out _);
            int after = CreekFlow.NearestSegment(CreekFlow.PathPoint(bend + 5), out _);
            Assert.That(before, Is.InRange(bend - 6, bend - 4), $"5 m upstream of bend {i}");
            Assert.That(after, Is.InRange(bend + 4, bend + 6), $"5 m downstream of bend {i}");
            Assert.That(CreekFlow.NearestSegment(CreekFlow.PathPoint(bend), out float d), Is.InRange(bend - 1, bend), $"on bend {i}");
            Assert.That(d, Is.LessThan(0.01f));
        }
    }

    [Test]
    public void ChannelWidthGrowsDownstreamAndTheMarginExtendsIt()
    {
        int upstream = CreekFlow.NearestSegment(CreekFlow.PathPoint(5), out _);
        Assert.That(CreekFlow.HalfWidthOfSegment(upstream), Is.InRange(0.85f, 1.1f));

        int s = CreekFlow.SampleCount / 2;
        Vector2 centre = CreekFlow.PathPoint(s);
        Vector2 flow = CreekFlow.FlowAt(centre);
        Vector2 side = new Vector2(-flow.y, flow.x);
        float half = CreekFlow.HalfWidthOfSegment(CreekFlow.NearestSegment(centre, out _));
        Assert.That(CreekFlow.InChannel(centre + side * (half + 0.5f), 1f, out _), Is.True, "inside the margin");
        Assert.That(CreekFlow.InChannel(centre + side * (half + 1.5f), 1f, out _), Is.False, "past the margin");
        Assert.That(CreekFlow.InChannel(centre + side * (half + 1.5f), 2f, out _), Is.True, "a bigger margin reaches it");
        Assert.That(CreekFlow.InChannel(new Vector2(500f, 500f), 1f, out _), Is.False, "far away");
    }

    // --- "In creek": zones, depth, channel ---

    object MakeWaterSource(out GameObject go)
    {
        go = new GameObject("test water");
        Component water = go.AddComponent(Find("WaterSource"));
        Type builder = Find("PropertyTerrainBuilder");
        Array zones = (Array)builder.GetMethod("WaterQualityZones", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, null);
        Type quality = Find("WaterQuality");
        water.GetType().GetMethod("Configure").Invoke(water, new object[] { Enum.ToObject(quality, 0), zones });
        return water;
    }

    static string ZoneAt(object water, Vector2 p) =>
        (string)water.GetType().GetMethod("ZoneAt").Invoke(water, new object[] { new Vector3(p.x, 0f, p.y) });

    [Test]
    public void InCreekHonoursTheSpringAndPondZonesDepthAndChannel()
    {
        object water = MakeWaterSource(out GameObject go);
        try
        {
            Vector2 spring = CreekFlow.ControlPoints[0];
            Vector2 pond = CreekFlow.ControlPoints[CreekFlow.ControlPoints.Length - 1];
            bool In(Vector2 p, float depth)
            {
                bool creekZone = ZoneAt(water, p) == null;
                return CreekFlow.InCreek(p, depth, 0.15f, 1f, creekZone, out _);
            }

            Vector2 nearSpring = CreekFlow.PathPoint(5);
            Vector2 nearPond = CreekFlow.PathPoint(CreekFlow.SampleCount - 6);
            Assert.That(ZoneAt(water, nearSpring), Is.EqualTo("Spring Hollow"));
            Assert.That(ZoneAt(water, nearPond), Does.Contain("pond"));
            Assert.That(In(nearSpring, 0.5f), Is.False, "creek water inside the 14 m spring zone does not count");
            Assert.That(In(nearPond, 0.5f), Is.False, "creek water inside the 32 m pond zone does not count");

            // Walk the path: just outside each zone counts, just inside does not (the zone edge is the 14 m / 32 m circle).
            int outsideSpring = Enumerable.Range(0, CreekFlow.SampleCount).First(i => Vector2.Distance(CreekFlow.PathPoint(i), spring) > 14.5f);
            int outsidePond = Enumerable.Range(0, CreekFlow.SampleCount).Last(i => Vector2.Distance(CreekFlow.PathPoint(i), pond) > 32.5f);
            Assert.That(In(CreekFlow.PathPoint(outsideSpring), 0.5f), Is.True, "just past the spring zone");
            Assert.That(In(CreekFlow.PathPoint(outsidePond), 0.5f), Is.True, "just before the pond zone");
            Assert.That(In(CreekFlow.PathPoint(outsideSpring - 3), 0.5f), Is.False, "just inside the spring zone");

            Vector2 mid = CreekFlow.PathPoint(CreekFlow.SampleCount / 2);
            Assert.That(In(mid, 0.5f), Is.True, "mid-creek, knee deep");
            Assert.That(In(mid, 0.15f), Is.True, "exactly ankle deep counts");
            Assert.That(In(mid, 0.14f), Is.False, "shallower than the ankle does not");
            Assert.That(In(mid, 0f), Is.False, "not over water");
            Assert.That(In(mid + new Vector2(12f, -12f), 0.5f), Is.False, "water far from the channel (not creek)");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(go);
        }
    }

    // --- Injure and the health floor ---

    [Test]
    public void HealthAfterInjuryNeverGoesBelowTheFloor()
    {
        Assert.That(CrossingMath.HealthAfterInjury(100f, 5f, 10f), Is.EqualTo(95f));
        Assert.That(CrossingMath.HealthAfterInjury(12f, 5f, 10f), Is.EqualTo(10f), "stops at the floor");
        Assert.That(CrossingMath.HealthAfterInjury(100f, 2f, 10f), Is.EqualTo(98f), "the default knock-down loss of 2");
        Assert.That(CrossingMath.HealthAfterInjury(11f, 2f, 10f), Is.EqualTo(10f), "11 - 2 would cross the floor: clamped to 10, not 9");
        Assert.That(CrossingMath.HealthAfterInjury(12f, 2f, 10f), Is.EqualTo(10f), "12 - 2 lands exactly on the floor");
        Assert.That(CrossingMath.HealthAfterInjury(10f, 5f, 10f), Is.EqualTo(10f), "already at the floor");
        Assert.That(CrossingMath.HealthAfterInjury(7f, 5f, 10f), Is.EqualTo(7f), "already below: nothing happens");
        Assert.That(CrossingMath.HealthAfterInjury(1f, 5f, 10f), Is.EqualTo(1f), "low health is not touched");
        Assert.That(CrossingMath.HealthAfterInjury(50f, 1000f, 10f), Is.EqualTo(10f), "a huge injury stops at the floor");
        Assert.That(CrossingMath.HealthAfterInjury(50f, 0f, 10f), Is.EqualTo(50f));
        Assert.That(CrossingMath.HealthAfterInjury(50f, -5f, 10f), Is.EqualTo(50f), "negative injury does not heal");
    }

    [Test]
    public void SurvivalManagerInjureRespectsTheFloorFromAnyHealth()
    {
        var go = new GameObject("test survival");
        try
        {
            Component survival = go.AddComponent(Find("SurvivalManager"));
            Type t = survival.GetType();
            MethodInfo injure = t.GetMethod("Injure");
            MethodInfo debugSet = t.GetMethod("DebugSet");
            float Health() => (float)t.GetProperty("Health").GetValue(survival);
            bool Collapsed() => (bool)t.GetProperty("IsCollapsed").GetValue(survival);
            float floor = (float)t.GetField("injuryHealthFloor").GetValue(survival);
            Assert.That(floor, Is.EqualTo(10f), "default floor");

            Assert.That(Health(), Is.EqualTo(100f));
            for (int i = 0; i < 40; i++)
            {
                injure.Invoke(survival, new object[] { 5f });
                Assert.That(Health(), Is.GreaterThanOrEqualTo(floor));
            }
            Assert.That(Health(), Is.EqualTo(floor), "repeated knock-downs end at the floor");
            Assert.That(Collapsed(), Is.False);
            Assert.That((float)injure.Invoke(survival, new object[] { 5f }), Is.EqualTo(0f), "nothing lost at the floor");

            debugSet.Invoke(survival, new object[] { 100f, 100f, 6f });
            Assert.That((float)injure.Invoke(survival, new object[] { 5f }), Is.EqualTo(0f));
            Assert.That(Health(), Is.EqualTo(6f), "below the floor: unchanged, no collapse");
            Assert.That(Collapsed(), Is.False);

            debugSet.Invoke(survival, new object[] { 100f, 100f, 12f });
            injure.Invoke(survival, new object[] { 5f });
            Assert.That(Health(), Is.EqualTo(10f), "12 - 5 stops at 10");

            debugSet.Invoke(survival, new object[] { 100f, 100f, 0f }); // already collapsed
            Assert.That((float)injure.Invoke(survival, new object[] { 5f }), Is.EqualTo(0f));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(go);
        }
    }

    [Test]
    public void StaminaNeverGoesNegativeFromALoss()
    {
        Assert.That(CrossingMath.StaminaAfterLoss(100f, 25f), Is.EqualTo(75f));
        Assert.That(CrossingMath.StaminaAfterLoss(10f, 25f), Is.EqualTo(0f));
        Assert.That(CrossingMath.StaminaAfterLoss(0f, 25f), Is.EqualTo(0f));
        Assert.That(CrossingMath.StaminaAfterLoss(40f, -10f), Is.EqualTo(40f), "a negative loss does not add stamina");
    }

    // --- Timing ---

    const float Dt = 0.05f;

    // Steps the timer for `seconds`, returning the times (from the start of the run) at which a knock-down happened.
    static System.Collections.Generic.List<float> Run(CrossingTimer timer, float seconds, CrossingMode mode, bool inCreek)
    {
        var knocks = new System.Collections.Generic.List<float>();
        int steps = Mathf.RoundToInt(seconds / Dt);
        for (int i = 1; i <= steps; i++)
            if (timer.Step(Dt, mode, inCreek, Timing).knockedDown)
                knocks.Add(i * Dt);
        return knocks;
    }

    [Test]
    public void FlashFloodKnocksDownAfterTheDelayThenLocksAndGivesImmunity()
    {
        var timer = new CrossingTimer();
        Assert.That(Run(timer, 1.4f, CrossingMode.FlashFlood, true), Is.Empty, "not before 1.5 s");
        Assert.That(timer.Locked, Is.False);

        var knock = Run(timer, 0.3f, CrossingMode.FlashFlood, true);
        Assert.That(knock.Count, Is.EqualTo(1));
        Assert.That(timer.Locked, Is.True, "input locked after the knock-down");

        Run(timer, 1.1f, CrossingMode.FlashFlood, true); // 1.3 s into the 1.5 s lockout
        Assert.That(timer.Locked, Is.True);
        Run(timer, 0.3f, CrossingMode.FlashFlood, true);
        Assert.That(timer.Locked, Is.False, "lockout over after about 1.5 s");
        Assert.That(timer.Immune, Is.True, "immunity starts when the lockout ends");

        Assert.That(Run(timer, 3.8f, CrossingMode.FlashFlood, true), Is.Empty, "no chain knock-down inside the 4 s");
        Assert.That(timer.Immune, Is.True);
        Run(timer, 0.3f, CrossingMode.FlashFlood, true);
        Assert.That(timer.Immune, Is.False, "immunity over after about 4 s");

        Assert.That(Run(timer, 1.7f, CrossingMode.FlashFlood, true).Count, Is.EqualTo(1), "stay in and it happens again");
    }

    [Test]
    public void HighWaterAndDryLandNeverKnockDown()
    {
        var timer = new CrossingTimer();
        Assert.That(Run(timer, 30f, CrossingMode.HighWater, true), Is.Empty);
        Assert.That(Run(timer, 30f, CrossingMode.FlashFlood, false), Is.Empty, "out of the creek");
        Assert.That(Run(timer, 30f, CrossingMode.None, true), Is.Empty);
        Assert.That(timer.Locked || timer.Immune, Is.False);
    }

    [Test]
    public void LeavingTheCreekResetsTheExposureClock()
    {
        var timer = new CrossingTimer();
        Run(timer, 1.2f, CrossingMode.FlashFlood, true);
        Run(timer, 1.2f, CrossingMode.FlashFlood, false); // out longer than the 1 s grace
        Assert.That(timer.Exposure, Is.EqualTo(0f));
        Assert.That(Run(timer, 1.2f, CrossingMode.FlashFlood, true), Is.Empty, "the clock started over");
    }

    [Test]
    public void WarningsAnnounceOncePerStayAndAgainAfterLeaving()
    {
        var timer = new CrossingTimer();
        int Announced(float seconds, CrossingMode mode, bool inCreek, CrossingMode want)
        {
            int n = 0;
            for (int i = 0; i < Mathf.RoundToInt(seconds / Dt); i++)
                if (timer.Step(Dt, mode, inCreek, Timing).announce == want)
                    n++;
            return n;
        }

        Assert.That(Announced(2f, CrossingMode.HighWater, true, CrossingMode.HighWater), Is.EqualTo(1), "once while wading");
        Assert.That(Announced(0.5f, CrossingMode.HighWater, false, CrossingMode.HighWater), Is.EqualTo(0));
        Assert.That(Announced(0.5f, CrossingMode.HighWater, true, CrossingMode.HighWater), Is.EqualTo(0), "a flicker at the bank doesn't repeat it");
        Assert.That(Announced(1.5f, CrossingMode.HighWater, false, CrossingMode.HighWater), Is.EqualTo(0));
        Assert.That(Announced(1f, CrossingMode.HighWater, true, CrossingMode.HighWater), Is.EqualTo(1), "a new stay announces again");
        Assert.That(Announced(0.5f, CrossingMode.FlashFlood, true, CrossingMode.FlashFlood), Is.EqualTo(1), "the flood line when it worsens");
        Assert.That(Announced(0.5f, CrossingMode.FlashFlood, true, CrossingMode.FlashFlood), Is.EqualTo(0));
    }

    [Test]
    public void ResetClearsLockoutAndImmunity()
    {
        var timer = new CrossingTimer();
        Run(timer, 1.6f, CrossingMode.FlashFlood, true);
        Assert.That(timer.Locked, Is.True);
        timer.Reset();
        Assert.That(timer.Locked, Is.False);
        Assert.That(timer.Immune, Is.False);
        Assert.That(timer.Exposure, Is.EqualTo(0f));

        Run(timer, 1.6f, CrossingMode.FlashFlood, true);
        Run(timer, 1.6f, CrossingMode.FlashFlood, true); // lockout over, immune
        Assert.That(timer.Immune, Is.True);
        timer.Reset();
        Assert.That(timer.Immune, Is.False);
    }
}
