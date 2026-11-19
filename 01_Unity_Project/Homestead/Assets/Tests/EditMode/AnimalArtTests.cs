using System;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

// Wave 5 animal art (2026-11-19): Animal loads Resources/Art/animal_<speciesId> the same way for every species. The deer is the first.
public class AnimalArtTests
{
    const BindingFlags Any = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

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

    static GameObject Spawn(string prefab) =>
        (GameObject)UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Wildlife/" + prefab + ".prefab"));

    static object Field(Component c, string name) => c.GetType().GetField(name, Any).GetValue(c);
    static object Call(Component c, string name) => c.GetType().GetMethod(name, Any).Invoke(c, null);

    [Test]
    public void TheDeerShowsItsArtWithTheVitalsAntlersAndBodyFittedAndFallsAsItsCarcass()
    {
        Assert.That(Find("Animal").GetField("ArtPrefix", Any).GetValue(null), Is.EqualTo("Art/animal_"));
        Assert.That(Resources.Load<GameObject>("Art/animal_deer"), Is.Not.Null);
        GameObject deer = Spawn("Deer");
        try
        {
            Component animal = deer.GetComponent(Find("Animal"));
            var model = (Transform)Field(animal, "model");
            Call(animal, "UseArt");

            Transform art = model.Find("Art");
            Assert.That(art, Is.Not.Null, "the art goes under Model");
            foreach (Renderer r in model.GetComponentsInChildren<Renderer>(true))
                if (!r.transform.IsChildOf(art))
                    Assert.That(r.enabled, Is.False, r.name + ": the primitive stops drawing");
            Transform alive = art.Find("Pose_Alive"), dead = art.Find("Pose_Dead");
            Assert.That(alive.gameObject.activeSelf, Is.True);
            Assert.That(dead.gameObject.activeSelf, Is.False, "the carcass pose waits for death");

            var vitals = (Transform)Field(animal, "vitals");
            Assert.That(Vector3.Distance(vitals.position, art.Find("Vitals").position), Is.LessThan(0.001f), "the kill zone is in the real chest");
            Assert.That(vitals.localPosition.y, Is.InRange(0.55f, 0.75f));

            var antlers = (GameObject)Field(animal, "antlers");
            Assert.That(antlers.transform.IsChildOf(alive), Is.True, "a buck shows the art's antlers");

            var capsule = model.GetComponentInChildren<CapsuleCollider>(true);
            Assert.That(capsule.direction, Is.EqualTo(2));
            Assert.That(capsule.height, Is.InRange(1.6f, 1.9f), "nose to tail");
            Assert.That(capsule.center.y, Is.InRange(0.5f, 0.85f), "around the body, not the old oversized one");

            antlers.SetActive(true);
            Assert.That((bool)Call(animal, "ShowDeadPose"), Is.True);
            Assert.That(alive.gameObject.activeSelf, Is.False);
            Assert.That(dead.gameObject.activeSelf, Is.True, "it falls as its carcass");
            Assert.That(dead.Find("Dead_Antlers").gameObject.activeSelf, Is.True, "a buck's carcass keeps its antlers");
            Assert.That(capsule.center.y, Is.LessThan(0.35f), "the capsule lies with the carcass, so it can be aimed at and dressed");
            Assert.That(capsule.height, Is.InRange(1.6f, 1.9f));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(deer);
        }
    }

    [Test]
    public void ADoesCarcassHasNoAntlers()
    {
        GameObject deer = Spawn("Deer");
        try
        {
            Component animal = deer.GetComponent(Find("Animal"));
            Call(animal, "UseArt");
            ((GameObject)Field(animal, "antlers")).SetActive(false);
            Call(animal, "ShowDeadPose");
            var model = (Transform)Field(animal, "model");
            Assert.That(model.Find("Art/Pose_Dead/Dead_Antlers").gameObject.activeSelf, Is.False);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(deer);
        }
    }

    [Test]
    public void ASpeciesWithNoArtYetKeepsItsPrimitivesAndTipsOverAsBefore()
    {
        Assert.That(Resources.Load<GameObject>("Art/animal_turkey"), Is.Null, "pick a species without art");
        GameObject turkey = Spawn("Turkey");
        try
        {
            Component animal = turkey.GetComponent(Find("Animal"));
            var model = (Transform)Field(animal, "model");
            Call(animal, "UseArt");
            Assert.That(model.Find("Art"), Is.Null);
            foreach (Renderer r in model.GetComponentsInChildren<Renderer>(true))
                Assert.That(r.enabled, Is.True, r.name);
            Assert.That((bool)Call(animal, "ShowDeadPose"), Is.False, "no carcass pose: Die tips the model instead");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(turkey);
        }
    }
}
