using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// Wave 8 effects (2026-11-20): the textures load, the body-state levels follow the survival tiers, the hot structures get a glow in their mouth,
// and nothing is made outside Play Mode. Game types are reached by reflection (the test assembly cannot see Assembly-CSharp).
public class Wave8EffectsTests
{
    static Type Find(string name) =>
        AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(name)).FirstOrDefault(t => t != null);

    static object Static(Type t, string method, params object[] args) =>
        t.GetMethods(BindingFlags.Public | BindingFlags.Static).First(m => m.Name == method && m.GetParameters().Length == args.Length).Invoke(null, args);

    static object Enum(string type, string value) => System.Enum.Parse(Find(type), value);

    [TestCase("Art/fx_ripple")]
    [TestCase("Art/fx_spark")]
    [TestCase("Art/fx_droplet")]
    [TestCase("Art/fx_footprint")]
    [TestCase("Art/fx_smoke_puff")]
    public void TheEffectTexturesLoad(string path)
    {
        Assert.That(Resources.Load<Texture2D>(path), Is.Not.Null, path);
    }

    [Test]
    public void NothingIsMadeOutsidePlayMode()
    {
        Type fx = Find("WorldFx");
        Assert.That(Static(fx, "Ensure"), Is.Null);
        // With no instance every call is a no-op rather than an error.
        Static(fx, "Blow", "axe", Vector3.zero, Vector3.forward);
        Static(fx, "Steam", true, Vector3.zero, true);
        Static(fx, "Splash", Vector3.zero, 1f);
        Static(fx, "Ripple", Vector3.zero, 1f);
        Static(fx, "Footstep", Vector3.zero, 0f, Enum("SurfaceType", "Dirt"));
        Assert.That(GameObject.Find("World FX"), Is.Null);
    }

    [Test]
    public void ColdHeatAndTiredLevelsFollowTheBody()
    {
        Type body = Find("BodyStateFx");
        Type survival = Find("SurvivalManager");
        object Tier(string v) => System.Enum.Parse(survival.GetNestedType("Tier"), v);
        object Heat(string v) => System.Enum.Parse(survival.GetNestedType("HeatStage"), v);
        Assert.That((float)Static(body, "ColdLevel", Tier("Fine")), Is.EqualTo(0f));
        Assert.That((float)Static(body, "ColdLevel", Tier("Reduced")), Is.EqualTo(0.5f));
        Assert.That((float)Static(body, "ColdLevel", Tier("Empty")), Is.EqualTo(1f));
        Assert.That((float)Static(body, "HeatLevel", Heat("None")), Is.EqualTo(0f));
        Assert.That((float)Static(body, "HeatLevel", Heat("Stroke")), Is.EqualTo(1f));
        Assert.That((float)Static(body, "TiredLevel", 100f, 100f), Is.EqualTo(0f));
        Assert.That((float)Static(body, "TiredLevel", 30f, 100f), Is.EqualTo(0f).Within(1e-4));
        Assert.That((float)Static(body, "TiredLevel", 0f, 100f), Is.EqualTo(1f));
    }

    [Test]
    public void AHotStructureGetsAFlickeringGlowAndItSwitchesOff()
    {
        Type glowType = Find("StructureGlow");
        var structure = new GameObject("Kiln Test");
        try
        {
            object kiln = Enum("PileKind", "Kiln");
            Assert.That(Static(glowType, "Drive", structure.transform, kiln, false), Is.Null, "nothing is made just to be off");
            var glow = (Component)Static(glowType, "Drive", structure.transform, kiln, true);
            Assert.That(glow, Is.Not.Null);
            Assert.That(glow.name, Is.EqualTo("Fire Glow"));
            Assert.That(glow.GetComponent<Light>(), Is.Not.Null);
            Assert.That(glow.GetComponentInChildren<ParticleSystem>(), Is.Not.Null);
            Assert.That(glow.transform.localPosition, Is.EqualTo(new Vector3(0f, 0.45f, 0.95f)));
            var again = (Component)Static(glowType, "Drive", structure.transform, kiln, false);
            Assert.That(again, Is.SameAs(glow), "one glow per structure, reused");
            Assert.That((bool)glowType.GetProperty("On").GetValue(again), Is.False);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(structure);
        }
    }

    // The fallback mouth (structures without art) is where the art puts its Anchor_Mouth.
    [TestCase("structure_kiln", "Kiln")]
    [TestCase("structure_bloomery", "Bloomery")]
    [TestCase("structure_smithy", "Smithy")]
    public void TheDefaultMouthMatchesTheArt(string prefab, string kind)
    {
        var art = Resources.Load<GameObject>("Art/" + prefab);
        Assert.That(art, Is.Not.Null);
        Transform mouth = art.GetComponentsInChildren<Transform>(true).First(t => t.name == "Anchor_Mouth");
        Vector3 local = art.transform.InverseTransformPoint(mouth.position);
        var expected = (Vector3)Static(Find("StructureGlow"), "DefaultMouth", Enum("PileKind", kind));
        Assert.That(Vector3.Distance(local, expected), Is.LessThan(0.02f));
    }
}
