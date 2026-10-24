using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// Drift guard for WeatherManager's season weights (the Weather weights check, 2026-10-23). WeatherManager lives in the main assembly,
// which this test assembly cannot reference, so it is reached by reflection like the Setbacks tests do. A bare component is made in
// the test (Awake does not run in edit mode, so nothing is registered), and the table is read through EnsureArrays, the same step the
// game runs at startup. Two sources are checked: the code defaults, and the weights saved in Bootstrap.unity (read as text, never
// opened or written), because the scene wins over the defaults in the game and its arrays are older (8 entries, grown at startup).
// When the weather is changed on purpose, update Expected below.
public class WeatherWeightsTests
{
    // Clear, Cloudy, LightRain, HeavyRain, Thunderstorm, ColdFront, Snow, Wind, HeatWave. Spring, Summer, Fall, Winter.
    static readonly float[][] Expected =
    {
        new float[] { 30, 30, 22, 10, 3, 2, 0, 3, 0 },
        new float[] { 49, 25, 3, 1, 2, 0, 0, 5, 15 },
        new float[] { 40, 28, 10, 4, 1, 7, 0, 10, 0 },
        new float[] { 20, 25, 3, 0, 0, 15, 30, 7, 0 },
    };

    const BindingFlags Any = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

    static readonly Type WeatherManagerType = Find("WeatherManager");
    static readonly Type ClimateType = WeatherManagerType.GetNestedType("SeasonClimate");

    GameObject host;

    static Type Find(string name) =>
        AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => { try { return a.GetTypes(); } catch (Exception) { return new Type[0]; } })
                 .First(t => t.Name == name);

    [TearDown]
    public void TearDown()
    {
        if (host != null)
            UnityEngine.Object.DestroyImmediate(host);
    }

    // A bare WeatherManager with its arrays grown by EnsureArrays; sceneSeasons (if given) replaces the code defaults first.
    Array RuntimeSeasons(float[][] sceneSeasons = null)
    {
        host = new GameObject("WeatherWeightsTests") { hideFlags = HideFlags.HideAndDontSave };
        Component weather = host.AddComponent(WeatherManagerType);
        FieldInfo field = WeatherManagerType.GetField("seasons", Any);

        if (sceneSeasons != null)
        {
            Array array = Array.CreateInstance(ClimateType, sceneSeasons.Length);
            for (int i = 0; i < sceneSeasons.Length; i++)
            {
                object climate = Activator.CreateInstance(ClimateType);
                ClimateType.GetField("weights").SetValue(climate, sceneSeasons[i]);
                array.SetValue(climate, i);
            }
            field.SetValue(weather, array);
        }

        WeatherManagerType.GetMethod("EnsureArrays", Any).Invoke(weather, null);
        return (Array)field.GetValue(weather);
    }

    static float[] WeightsOf(object climate) => (float[])ClimateType.GetField("weights").GetValue(climate);

    static void AssertTable(Array seasons, string source)
    {
        Assert.That(seasons.Length, Is.EqualTo(4), source + ": four seasons");
        for (int s = 0; s < seasons.Length; s++)
        {
            float[] weights = WeightsOf(seasons.GetValue(s));
            Assert.That(weights.Length, Is.EqualTo(9), $"{source}: season {s} has nine weather types");
            Assert.That(weights.All(w => w >= 0f), Is.True, $"{source}: season {s} has no negative weight");
            Assert.That(weights.Sum(), Is.EqualTo(100f).Within(0.0001f), $"{source}: season {s} sums to 100");
            Assert.That(weights, Is.EqualTo(Expected[s]), $"{source}: season {s} matches the table");
        }
    }

    // The weights as saved in Bootstrap.unity: the WeatherManager's "  seasons:" list, one "- weights:" group per season.
    static float[][] SceneWeights()
    {
        string path = Path.Combine(Application.dataPath, "Scenes", "Bootstrap.unity");
        var seasons = new List<float[]>();
        List<float> current = null;
        bool inside = false;
        foreach (string raw in File.ReadLines(path))
        {
            string line = raw.TrimEnd();
            if (!inside)
            {
                inside = line == "  seasons:";
                continue;
            }
            if (line == "  weatherTypes:")
                break;
            if (line.StartsWith("  - weights:"))
                seasons.Add((current = new List<float>()).ToArray());
            else if (current != null && line.StartsWith("    - "))
            {
                current.Add(float.Parse(line.Substring(6), System.Globalization.CultureInfo.InvariantCulture));
                seasons[seasons.Count - 1] = current.ToArray();
            }
        }
        Assert.That(seasons, Is.Not.Empty, "found the seasons block in Bootstrap.unity");
        return seasons.ToArray();
    }

    [Test]
    public void TheCountsAreFourSeasonsAndNineWeatherTypes()
    {
        Assert.That(WeatherManagerType.GetField("SeasonCount", Any).GetRawConstantValue(), Is.EqualTo(4));
        Assert.That(WeatherManagerType.GetField("WeatherTypeCount", Any).GetRawConstantValue(), Is.EqualTo(9));
        Assert.That(Enum.GetValues(Find("WeatherType")).Length, Is.EqualTo(9));
        Assert.That(Enum.GetValues(Find("Season")).Length, Is.EqualTo(4));
    }

    [Test]
    public void TheCodeDefaultsMatchTheTable()
    {
        AssertTable(RuntimeSeasons(), "code defaults");
    }

    [Test]
    public void TheBootstrapSceneWeightsGrowToTheTableAtStartup()
    {
        float[][] scene = SceneWeights();
        Assert.That(scene.All(w => w.Length <= 9), Is.True, "the scene never holds more than nine entries per season");
        AssertTable(RuntimeSeasons(scene), "Bootstrap.unity after EnsureArrays");
    }
}
