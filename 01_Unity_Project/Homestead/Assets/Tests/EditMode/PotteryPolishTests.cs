using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

// EditMode tests for the polish batch after Pottery P3 (Mike, 2026-11-03): the Kiln's R prompt when nothing is carried, the Kiln's smoke and the front mouth that flashed, and the
// creek's "While you slept" line. The main-assembly parts are reached by reflection as the other world tests do. Nothing here touches the Saves folder or a scene.
public class PotteryPolishTests
{
    static Type Find(string name) =>
        AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => { try { return a.GetTypes(); } catch (Exception) { return new Type[0]; } })
                 .First(t => t.Name == name);

    static string SourceText(string relative) => File.ReadAllText(Path.Combine(Application.dataPath, relative));

    // --- 1. The prompt ---

    [Test]
    public void KilnLoadPromptSaysYouAreNotCarryingAnyWhenNothingFits()
    {
        string text = PotteryRules.KilnLoadPrompt(PotteryRules.KilnGate.NothingFits, "");
        Assert.That(text, Does.Contain("you're not carrying any"));
        Assert.That(text, Does.Not.Contain("carry some"));
    }

    [Test]
    public void NoPotteryPromptTellsYouToCarrySome()
    {
        Assert.That(SourceText("Scripts/Economy/PotteryRules.cs"), Does.Not.Contain("carry some"));
        Assert.That(SourceText("Scripts/World/PlacedStructure.cs"), Does.Not.Contain("carry some"));
    }

    // --- 2. The smoke ---

    [Test]
    public void TheSmokeMaterialExistsAndIsAnUnlitTransparentMaterial()
    {
        Material material = Resources.Load<Material>("KilnSmoke");
        Assert.That(material, Is.Not.Null, "Resources/KilnSmoke.mat is missing");
        Assert.That(material.shader.name, Does.Contain("Unlit"), "smoke must not use a lit or glossy shader");
        Assert.That(material.GetFloat("_SurfaceType"), Is.EqualTo(1f), "smoke must be transparent");
        Assert.That(material.GetColor("_UnlitColor").a, Is.InRange(0.05f, 0.6f), "smoke should be translucent");
    }

    [Test]
    public void SmokeOnlyEmitsWhileTheFiringRuns()
    {
        var host = new GameObject("Kiln for the smoke test");
        try
        {
            object smoke = Find("KilnSmoke").GetMethod("Create").Invoke(null, new object[] { host.transform });
            var component = (Component)smoke;
            var system = component.GetComponent<ParticleSystem>();
            Assert.That(system, Is.Not.Null);
            Assert.That(system.emission.enabled, Is.False, "no smoke before the kiln is fired");

            Find("KilnSmoke").GetMethod("SetFiring").Invoke(smoke, new object[] { true });
            Assert.That(system.emission.enabled, Is.True);
            Find("KilnSmoke").GetMethod("SetFiring").Invoke(smoke, new object[] { false });
            Assert.That(system.emission.enabled, Is.False, "the smoke stops when the firing ends");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(host);
        }
    }

    [Test]
    public void SmokeIsRoundSoftPuffsNotGlossySpheres()
    {
        string visuals = SourceText("Scripts/World/PlacedStructure.cs");
        Assert.That(visuals, Does.Not.Contain("three smoke puffs"));
        Assert.That(visuals, Does.Contain("KilnSmoke.Create"));
        string smoke = SourceText("Scripts/World/KilnSmoke.cs");
        Assert.That(smoke, Does.Not.Contain("CreatePrimitive"));
        Assert.That(smoke, Does.Not.Contain("_Metallic"));
        Assert.That(smoke, Does.Not.Contain("_Smoothness"));
    }

    // --- 3. The flashing ---

    // The mouth's front face (centre z + half its 0.3 depth) must not sit in the same plane as the front ring stone's (0.92 + half its 0.3 size), or the two z-fight.
    [Test]
    public void TheMouthDoesNotShareAFacePlaneWithTheFrontStone()
    {
        string visuals = SourceText("Scripts/World/PlacedStructure.cs");
        Assert.That(visuals, Does.Contain("new Vector3(0f, 0.45f, 0.95f), Vector3.zero, new Vector3(0.62f, 0.5f, 0.3f)"), "the mouth should stand at z 0.95");
        Assert.That(visuals, Does.Not.Contain("new Vector3(0f, 0.45f, 0.92f), Vector3.zero, new Vector3(0.62f, 0.5f, 0.3f)"), "the mouth must not be back at the stone's plane");
        const float mouthFront = 0.95f + 0.3f / 2f, stoneFront = 0.92f + 0.3f / 2f;
        Assert.That(mouthFront - stoneFront, Is.GreaterThanOrEqualTo(0.02f));
    }

    [Test]
    public void TheKilnVisualsHaveNoPerFrameWorkAndNothingScalesOnAFlicker()
    {
        string visuals = SourceText("Scripts/World/PlacedStructure.cs");
        int start = visuals.IndexOf("static void BuildKiln(", StringComparison.Ordinal);
        int end = visuals.IndexOf("static void KilnPiece(", StringComparison.Ordinal);
        Assert.That(start, Is.GreaterThan(0));
        string kiln = visuals.Substring(start, end - start);
        Assert.That(kiln, Does.Not.Contain("Time.time"));
        Assert.That(kiln, Does.Not.Contain("Sin(Time"));
        Assert.That(kiln, Does.Not.Contain("PerlinNoise"));
        Assert.That(SourceText("Scripts/World/KilnSmoke.cs"), Does.Not.Contain("localScale"));
    }

    // --- 4. The creek line ---

    static string Away(string peak, bool slept) =>
        (string)Find("StreamManager").GetMethod("AwaySummaryText").Invoke(null, new[] { Enum.Parse(Find("StreamState"), peak), (object)slept });

    [Test]
    public void TheSleptWordingIsOnlyForARealSleep()
    {
        Assert.That(Away("HighWater", true), Is.EqualTo("While you slept the creek rose."));
        Assert.That(Away("FlashFlood", true), Is.EqualTo("While you slept the creek flooded."));
        Assert.That(Away("HighWater", false), Is.EqualTo("The creek is rising."));
        Assert.That(Away("FlashFlood", false), Is.EqualTo("The creek is in flood."));
        Assert.That(Away("Normal", true), Is.EqualTo(""));
        Assert.That(Away("Normal", false), Is.EqualTo(""));
    }

    [Test]
    public void AMenuOrPauseIsNotSleepInTheStreamManager()
    {
        string stream = SourceText("Scripts/Managers/StreamManager.cs");
        Assert.That(stream, Does.Contain("awaySlept |= IsAsleep"));
        Assert.That(stream, Does.Contain("AwaySummaryText(awayPeak, awaySlept)"));
        // A rise is only kept for the summary while the player cannot see it (a rise on screen has already shown its own toast).
        int changed = stream.IndexOf("void StateChanged(", StringComparison.Ordinal);
        string body = stream.Substring(changed, 700);
        Assert.That(body, Does.Contain("if (Suppressed)"));
    }
}
