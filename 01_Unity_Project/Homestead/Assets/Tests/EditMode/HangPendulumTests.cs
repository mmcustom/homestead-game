using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

// EditMode tests for HangPendulum (Mike, 2026-11-14): hanging items in hand hang by gravity, swing when the hand accelerates, settle, and never fly past the limit.
// The game scripts live in Assembly-CSharp, which this test assembly cannot reference, so the pendulum is reached by reflection (as HandsTests does).
public class HangPendulumTests
{
    const float Dt = 1f / 60f;

    static Type Find(string name) =>
        AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(name)).FirstOrDefault(t => t != null);

    sealed class P
    {
        readonly object inst;
        static Type T => Find("HangPendulum");
        public P() { inst = Activator.CreateInstance(T); }
        public Vector3 Step(Vector3 pivot, float dt) => (Vector3)T.GetMethod("Step").Invoke(inst, new object[] { pivot, dt });
        public void Reset() => T.GetMethod("Reset").Invoke(inst, null);
        public Vector3 Direction => (Vector3)T.GetProperty("Direction").GetValue(inst);
        public static float MaxAngle => (float)T.GetField("MaxAngle").GetValue(null);
    }

    static Vector3 Run(P p, Func<int, Vector3> pivotAt, int frames)
    {
        Vector3 d = p.Direction;
        for (int i = 0; i < frames; i++)
            d = p.Step(pivotAt(i), Dt);
        return d;
    }

    [Test]
    public void AStillHandHoldsItStraightDown()
    {
        var p = new P();
        Vector3 d = Run(p, i => new Vector3(1f, 2f, 3f), 120);
        Assert.That(Vector3.Angle(Vector3.down, d), Is.LessThan(0.01f));
    }

    [Test]
    public void AHandSpeedingForwardThrowsTheBodyBackThenItSettles()
    {
        var p = new P();
        Run(p, i => Vector3.zero, 10);
        Vector3 pos = Vector3.zero; float speed = 0f;
        Vector3 d = Vector3.down;
        for (int i = 0; i < 15; i++) { speed += 8f * Dt; pos += Vector3.forward * speed * Dt; d = p.Step(pos, Dt); }
        Assert.That(d.z, Is.LessThan(-0.05f), "the body lags behind the hand");
        Vector3 stop = pos;
        d = Run(p, i => stop, 180);
        Assert.That(Vector3.Angle(Vector3.down, d), Is.LessThan(1f), "settled within three seconds");
    }

    [Test]
    public void AHandMovingAtASteadySpeedDoesNotTiltIt()
    {
        var p = new P();
        Vector3 d = Run(p, i => Vector3.forward * (1.4f * i * Dt), 240);
        Assert.That(Vector3.Angle(Vector3.down, d), Is.LessThan(0.5f));
    }

    [Test]
    public void PausedChangesNothing()
    {
        var p = new P();
        Run(p, i => Vector3.zero, 5);
        Vector3 before = p.Step(Vector3.zero, Dt);
        Vector3 after = p.Step(new Vector3(5f, 0f, 0f), 0f);
        Assert.That(after, Is.EqualTo(before));
    }

    [Test]
    public void AViolentJerkNeverSwingsItPastTheLimit()
    {
        var p = new P();
        Run(p, i => Vector3.zero, 3);
        float worst = 0f;
        for (int i = 0; i < 120; i++)
            worst = Mathf.Max(worst, Vector3.Angle(Vector3.down, p.Step(new Vector3(i % 2 == 0 ? 3f : -3f, 0f, 0f), Dt)));
        Assert.That(worst, Is.LessThanOrEqualTo(P.MaxAngle + 0.01f));
    }

    [Test]
    public void ResetHangsItStraightDownAgain()
    {
        var p = new P();
        Run(p, i => Vector3.right * (i * i * 0.001f), 30);
        p.Reset();
        Assert.That(p.Direction, Is.EqualTo(Vector3.down));
    }

    [Test]
    public void TheLanternHangsByGravityLikeTheBucket()
    {
        string path = System.IO.Path.Combine(UnityEngine.Application.dataPath, "Scripts/Player/PlayerHands.cs");
        string source = System.IO.File.ReadAllText(path);
        Assert.That(source, Does.Contain("hanging = ToolActionRules.IsHanging(toolId) || toolId == ToolActionRules.LanternId;"), "the Lantern swings on its bail by gravity");
    }

    [Test]
    public void APrefabBowsStringHalvesFollowTheDrawingHand()
    {
        string source = System.IO.File.ReadAllText(System.IO.Path.Combine(UnityEngine.Application.dataPath, "Scripts/Player/PlayerHands.cs"));
        Assert.That(source, Does.Not.Contain("if (heldIsPrimitive)" + "\n" + "                    stringHalves"), "the string halves are looked up on a prefab too");
        var bow = Resources.Load<GameObject>("Art/tool_primitive_bow");
        Assume.That(bow, Is.Not.Null, "no tool_primitive_bow prefab in this checkout");
        Assert.That(bow.transform.Find("String Upper"), Is.Not.Null);
        Assert.That(bow.transform.Find("String Lower"), Is.Not.Null);
    }

    [Test]
    public void TheBowsAimWithTheRightMouseAtAGentleZoomAndKeepTheHands()
    {
        Type weapon = Find("HuntingWeapon");
        Assert.That(weapon.GetProperty("BowAimFraction"), Is.Not.Null, "a read-only bow aim fraction");
        string source = System.IO.File.ReadAllText(System.IO.Path.Combine(Application.dataPath, "Scripts/Wildlife/HuntingWeapon.cs"));
        StringAssert.Contains("float bowAimZoom = 1.5f", source, "far less than the scope's 6x");
        StringAssert.Contains("float scopeZoom = 6f", source, "the rifle's scope is unchanged");
        StringAssert.Contains("bool wantBowAim = equipped != RifleId && aim != null && aim.IsPressed();", source);
        StringAssert.Contains("public float ScopeFraction => scope;", source, "the bows still report no scope, so the hands stay in view");
    }
}
