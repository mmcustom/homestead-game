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

    // Every species whose art exists, checked the same way, so each new animal is covered by adding its prefab name here.
    [TestCase("Deer")]
    [TestCase("Turkey")]
    [TestCase("Duck")]
    [TestCase("Rabbit")]
    [TestCase("Squirrel")]
    public void ASpeciesWithArtShowsItAndFallsAsItsCarcass(string prefab)
    {
        GameObject go = Spawn(prefab);
        try
        {
            Component animal = go.GetComponent(Find("Animal"));
            var model = (Transform)Field(animal, "model");
            Call(animal, "UseArt");
            Transform art = model.Find("Art");
            Assert.That(art, Is.Not.Null, prefab + ": its art is shown");
            foreach (Renderer r in model.GetComponentsInChildren<Renderer>(true))
                if (!r.transform.IsChildOf(art))
                    Assert.That(r.enabled, Is.False, r.name);
            var vitals = (Transform)Field(animal, "vitals");
            Assert.That(Vector3.Distance(vitals.position, art.Find("Vitals").position), Is.LessThan(0.001f));
            var capsule = model.GetComponentInChildren<CapsuleCollider>(true);
            float standingY = capsule.center.y;
            Assert.That((bool)Call(animal, "ShowDeadPose"), Is.True);
            Assert.That(art.Find("Pose_Alive").gameObject.activeSelf, Is.False);
            Assert.That(art.Find("Pose_Dead").gameObject.activeSelf, Is.True);
            Assert.That(capsule.center.y, Is.LessThan(standingY), "the capsule comes down with the carcass");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(go);
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
        // every wildlife species has art since 2026-11-19, so give one a made-up species id (a future animal added before its art)
        GameObject squirrel = Spawn("Squirrel");
        Component c0 = squirrel.GetComponent(Find("Animal"));
        c0.GetType().GetField("speciesId", Any).SetValue(c0, "test_species_without_art");
        Assert.That(Resources.Load<GameObject>("Art/animal_test_species_without_art"), Is.Null);
        try
        {
            Component animal = squirrel.GetComponent(Find("Animal"));
            var model = (Transform)Field(animal, "model");
            Call(animal, "UseArt");
            Assert.That(model.Find("Art"), Is.Null);
            foreach (Renderer r in model.GetComponentsInChildren<Renderer>(true))
                Assert.That(r.enabled, Is.True, r.name);
            Assert.That((bool)Call(animal, "ShowDeadPose"), Is.False, "no carcass pose: Die tips the model instead");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(squirrel);
        }
    }

    // Toms and hens (Mike, 2026-11-19): a species drawn by sex keeps each look in a Male and a Female group; only the animal's own shows,
    // standing and as its carcass (a tom's fan, a hen's folded tail).
    [TestCase(true)]
    [TestCase(false)]
    public void ATomAndAHenEachShowTheirOwnLook(bool male)
    {
        GameObject go = Spawn("Turkey");
        try
        {
            Component animal = go.GetComponent(Find("Animal"));
            var model = (Transform)Field(animal, "model");
            Call(animal, "UseArt");
            animal.GetType().GetProperty("IsMale", Any).GetSetMethod(true).Invoke(animal, new object[] { male });
            Transform art = model.Find("Art"), alive = art.Find("Pose_Alive"), dead = art.Find("Pose_Dead");
            animal.GetType().GetMethod("ShowSex", Any).Invoke(animal, new object[] { alive });
            Assert.That(alive.Find("Male").gameObject.activeSelf, Is.EqualTo(male), "the tom's look");
            Assert.That(alive.Find("Female").gameObject.activeSelf, Is.EqualTo(!male), "the hen's look");
            Assert.That((bool)Call(animal, "ShowDeadPose"), Is.True);
            Assert.That(dead.Find("Dead_Male").gameObject.activeSelf, Is.EqualTo(male), "a tom falls as a tom");
            Assert.That(dead.Find("Dead_Female").gameObject.activeSelf, Is.EqualTo(!male));
            var capsule = model.GetComponentInChildren<CapsuleCollider>(true);
            Assert.That(capsule.height, Is.InRange(male ? 0.52f : 0.43f, male ? 0.62f : 0.53f), "the capsule fits the shown body, breast to tail: a tom's 0.57 m, a hen's 0.49 m");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(go);
        }
    }
}
