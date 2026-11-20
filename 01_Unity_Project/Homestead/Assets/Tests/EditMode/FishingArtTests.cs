using System;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

// The fishing bobber's art (Mike, 2026-11-19): FishingRod swaps the Bobber prefab's primitive spheres for Resources/Art/fishing_bobber.
public class FishingArtTests
{
    static Type Find(string name)
    {
        foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type t = a.GetType(name);
            if (t != null)
                return t;
        }
        throw new Exception(name + " not found");
    }

    [Test]
    public void TheBobberShowsItsArtAtTheWaterLineAndKeepsItsLine()
    {
        Assert.That(Resources.Load<GameObject>("Art/fishing_bobber"), Is.Not.Null);
        var bobber = (GameObject)UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Bobber.prefab"));
        try
        {
            MethodInfo use = Find("FishingRod").GetMethod("UseBobberArt", BindingFlags.Public | BindingFlags.Static);
            Assert.That((bool)use.Invoke(null, new object[] { bobber }), Is.True);
            Transform art = bobber.transform.Find("Art");
            Assert.That(art, Is.Not.Null);
            foreach (Renderer r in bobber.GetComponentsInChildren<MeshRenderer>(true))
                Assert.That(r.enabled, Is.EqualTo(r.transform.IsChildOf(art)), r.name + ": only the art is drawn");
            LineRenderer line = bobber.GetComponentInChildren<LineRenderer>();
            Assert.That(line, Is.Not.Null);
            Assert.That(line.enabled, Is.True, "the fishing line stays");
            Bounds b = art.GetComponentInChildren<Renderer>().bounds;
            Assert.That(b.min.y, Is.LessThan(bobber.transform.position.y), "part of it sits under the water line");
            Assert.That(b.max.y, Is.GreaterThan(bobber.transform.position.y + 0.03f), "the red top shows above it");
            Assert.That(b.size.x, Is.InRange(0.05f, 0.09f), "about the size of the placeholder");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(bobber);
        }
    }
}
