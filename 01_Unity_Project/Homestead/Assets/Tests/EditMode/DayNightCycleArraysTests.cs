using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// DayNightCycle's per-weather arrays grow at Awake as well as OnValidate (OnValidate does not run in a built game). The main assembly is
// reached by reflection, like WeatherWeightsTests: a bare component is made (Awake does not run in edit mode by itself), given the old
// 8-entry arrays a scene saved before the Heat Wave holds, and Awake is invoked by hand.
public class DayNightCycleArraysTests
{
    const BindingFlags Any = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

    static readonly Type CycleType = AppDomain.CurrentDomain.GetAssemblies()
        .SelectMany(a => { try { return a.GetTypes(); } catch (Exception) { return new Type[0]; } })
        .First(t => t.Name == "DayNightCycle");

    static readonly string[] Names = { "weatherSunlight", "weatherFogDistance", "weatherCloudCover" };

    // Heat Wave (the ninth entry) in the code defaults.
    static readonly float[] HeatWave = { 1f, 380f, 0.1f };

    // Values a designer might have tuned in the Inspector: deliberately not the code defaults.
    static readonly float[][] Tuned =
    {
        new[] { 0.9f, 0.45f, 0.3f, 0.25f, 0.1f, 0.7f, 0.4f, 0.85f },
        new[] { 450f, 310f, 210f, 130f, 70f, 360f, 160f, 410f },
        new[] { 0.05f, 0.8f, 0.85f, 0.95f, 0.9f, 0.4f, 0.9f, 0.25f },
    };

    GameObject host;

    [TearDown]
    public void TearDown()
    {
        if (host != null)
            UnityEngine.Object.DestroyImmediate(host);
    }

    Component Bare()
    {
        host = new GameObject("DayNightCycleArraysTests") { hideFlags = HideFlags.HideAndDontSave };
        return host.AddComponent(CycleType);
    }

    static float[] Get(Component c, string name) => (float[])CycleType.GetField(name, Any).GetValue(c);

    static void Set(Component c, string name, float[] values) => CycleType.GetField(name, Any).SetValue(c, values);

    static void RunAwake(Component c) => CycleType.GetMethod("Awake", Any).Invoke(c, null);

    [Test]
    public void EightEntryArraysGrowToNineAtAwakeKeepingTheScenesValues()
    {
        Component c = Bare();
        for (int i = 0; i < Names.Length; i++)
            Set(c, Names[i], (float[])Tuned[i].Clone());

        RunAwake(c);

        for (int i = 0; i < Names.Length; i++)
        {
            float[] grown = Get(c, Names[i]);
            Assert.That(grown.Length, Is.EqualTo(9), Names[i] + " has nine entries");
            Assert.That(grown.Take(8).ToArray(), Is.EqualTo(Tuned[i]), Names[i] + ": the first eight are the scene's values");
            Assert.That(grown[8], Is.EqualTo(HeatWave[i]), Names[i] + ": the ninth is the Heat Wave default");
        }
    }

    [Test]
    public void NineEntryArraysAreLeftAlone()
    {
        Component c = Bare();
        var nine = new float[3][];
        for (int i = 0; i < Names.Length; i++)
        {
            nine[i] = Tuned[i].Concat(new[] { 7f }).ToArray(); // a tuned ninth entry too
            Set(c, Names[i], (float[])nine[i].Clone());
        }

        RunAwake(c);

        for (int i = 0; i < Names.Length; i++)
            Assert.That(Get(c, Names[i]), Is.EqualTo(nine[i]), Names[i] + " is unchanged");
    }

    [Test]
    public void TheCodeDefaultsAlreadyHaveNineEntriesEndingInTheHeatWaveValues()
    {
        Component c = Bare();
        RunAwake(c);
        for (int i = 0; i < Names.Length; i++)
        {
            float[] values = Get(c, Names[i]);
            Assert.That(values.Length, Is.EqualTo(9), Names[i]);
            Assert.That(values[8], Is.EqualTo(HeatWave[i]), Names[i]);
        }
    }

    [Test]
    public void AMissingArrayIsFilledWithTheDefaults()
    {
        Component c = Bare();
        Set(c, "weatherFogDistance", null);
        RunAwake(c);
        Assert.That(Get(c, "weatherFogDistance"), Is.EqualTo(new[] { 400f, 300f, 200f, 120f, 80f, 350f, 150f, 400f, 380f }));
    }
}
