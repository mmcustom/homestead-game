using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// EditMode tests for Art readiness A1 (Mike, 2026-11-04): ArtSwap, the one swap point where a prefab at Resources/Art/<id>.prefab replaces a structure's primitives, with the Kiln as the pilot.
// The parts in the main assembly are reached by reflection as the other world tests do. A prefab is supplied through ArtSwap.Loader, so no asset is needed in Resources. Nothing here touches
// the Saves folder or a scene, and the test prefab the Editor menu makes is never created by a test.
public class ArtSwapTests
{
    static Type Find(string name) =>
        AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => { try { return a.GetTypes(); } catch (Exception) { return new Type[0]; } })
                 .First(t => t.Name == name);

    static object Kind(string name) => Enum.Parse(Find("PileKind"), name);
    static Type Art => Find("ArtSwap");

    const BindingFlags Any = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    static object Spec(object kind) => Art.GetMethod("SpecFor", new[] { Find("PileKind") }).Invoke(null, new[] { kind });
    static Type SmokeType => Find("KilnSmoke");
    static float SmokeHeight(object smoke) => (float)SmokeType.GetField("height").GetValue(smoke);
    static bool SmokeFiring(object smoke) => (bool)SmokeType.GetProperty("IsFiring").GetValue(smoke);
    static int SmokesUnder(GameObject root) => root.GetComponentsInChildren(SmokeType, true).Length;

    static object Field(object target, string name) => target.GetType().GetField(name).GetValue(target);

    static object NewPile(string kind, int id = 1)
    {
        object state = Activator.CreateInstance(Find("WoodPileState"));
        Find("WoodPileState").GetField("kind").SetValue(state, Kind(kind));
        Find("WoodPileState").GetField("id").SetValue(state, id);
        return state;
    }

    static void Add(object pile, string id, int count) => pile.GetType().GetMethod("Add").Invoke(pile, new object[] { id, count, 0 });

    // A Kiln in each phase: empty, loaded (dry pieces and Firewood), firing, or done (fired pieces and Ash waiting).
    static object Kiln(string phase)
    {
        object kiln = NewPile("Kiln");
        switch (phase)
        {
            case "loaded":
                Add(kiln, PotteryRules.DryCupId, 2);
                Add(kiln, PotteryRules.DryBowlId, 1);
                Add(kiln, PotteryRules.FirewoodId, 10);
                break;
            case "firing":
                Add(kiln, PotteryRules.DryCupId, 2);
                Add(kiln, PotteryRules.FirewoodId, 10);
                kiln.GetType().GetField("jobItem").SetValue(kiln, PotteryRules.FiringMarker);
                break;
            case "done":
                Add(kiln, "clay_cup", 2);
                Add(kiln, PotteryRules.AshId, 3);
                break;
        }
        return kiln;
    }

    readonly List<GameObject> made = new List<GameObject>();

    GameObject Track(GameObject go)
    {
        made.Add(go);
        return go;
    }

    [SetUp]
    public void SetUp() => Art.GetMethod("ResetForTests").Invoke(null, null);

    [TearDown]
    public void TearDown()
    {
        Art.GetMethod("ResetForTests").Invoke(null, null);
        foreach (GameObject go in made)
            if (go != null)
                UnityEngine.Object.DestroyImmediate(go);
        made.Clear();
    }

    static void SetLoader(Func<string, GameObject> loader) => Art.GetField("Loader").SetValue(null, loader);

    // A stand-in prefab: an inactive scene object (as a prefab's root is), every shape with its collider still on, so the stripping can be seen.
    GameObject MakePrefab(bool anchors = true, bool solid = false, bool ownSmoke = false)
    {
        var root = Track(new GameObject("proto"));
        GameObject Child(string name, Vector3 at)
        {
            GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c.name = name;
            c.transform.SetParent(root.transform, false);
            c.transform.localPosition = at;
            return c;
        }
        Child("Body", new Vector3(0f, 0.15f, 0f));
        foreach (string state in new[] { "State_Empty", "State_Loaded", "State_Firing", "State_Done" })
            Child(state, new Vector3(0f, 0.8f, 0f));
        Child("On_Firing", new Vector3(0f, 0.45f, 1.15f));
        if (solid)
            Child("Solid_Wall", new Vector3(0f, 1f, -1f)).transform.localScale = new Vector3(2f, 2f, 0.2f);
        if (anchors)
        {
            new GameObject("Anchor_Smoke").transform.SetParent(root.transform, false);
            root.transform.Find("Anchor_Smoke").localPosition = new Vector3(0f, 1.1f, 0f);
            new GameObject("Anchor_Mouth").transform.SetParent(root.transform, false);
            root.transform.Find("Anchor_Mouth").localPosition = new Vector3(0f, 0.45f, 0.95f);
            new GameObject("Anchor_Stock").transform.SetParent(root.transform, false);
            root.transform.Find("Anchor_Stock").localPosition = new Vector3(-1.45f, 0f, 0f);
            if (ownSmoke)
                new GameObject("Puffs").AddComponent<ParticleSystem>().transform.SetParent(root.transform.Find("Anchor_Smoke"), false);
        }
        root.SetActive(false);
        return root;
    }

    // Calls StructureVisuals.Build (or BuildPrimitive); returns the pieces and the interaction box.
    (GameObject parent, List<GameObject> pieces, Vector3 center, Vector3 size) Build(object state, bool primitive = false, GameObject parent = null, bool nullList = false)
    {
        parent = parent != null ? parent : Track(new GameObject("structure test"));
        var pieces = nullList ? null : new List<GameObject>();
        object[] args = { parent.transform, state, null, pieces, Vector3.zero, Vector3.zero };
        Find("StructureVisuals").GetMethod(primitive ? "BuildPrimitive" : "Build", Any | BindingFlags.Instance).Invoke(null, args);
        return (parent, pieces, (Vector3)args[4], (Vector3)args[5]);
    }

    static Transform Instance(GameObject parent) => parent.transform.Find("structure_kiln");

    // --- The spec table ---

    [Test]
    public void TheSpecTableHasAKilnRowWithItsIdAnchorsStatesAndFlag()
    {
        object spec = Spec(Kind("Kiln"));
        Assert.That(spec, Is.Not.Null);
        Assert.That((string)Field(spec, "id"), Is.EqualTo("structure_kiln"));
        var anchors = ((IEnumerable)Field(spec, "anchors")).Cast<object>().ToList();
        Assert.That(anchors.Select(a => (string)Field(a, "name")), Is.EqualTo(new[] { "Anchor_Smoke", "Anchor_Mouth", "Anchor_Stock" }));
        Assert.That(anchors.Select(a => (bool)Field(a, "required")), Is.EqualTo(new[] { true, true, false }));
        Assert.That(anchors.Select(a => (Vector3)Field(a, "fallback")), Is.EqualTo(new[] { new Vector3(0f, 1.1f, 0f), new Vector3(0f, 0.45f, 0.95f), new Vector3(-1.45f, 0f, 0f) }));
        Assert.That((string[])Field(spec, "states"), Is.EqualTo(new[] { "State_Empty", "State_Loaded", "State_Firing", "State_Done" }));
        Assert.That((string[])Field(spec, "flags"), Is.EqualTo(new[] { "On_Firing" }));
        Assert.That((Vector3)Field(spec, "expectedSize"), Is.EqualTo(new Vector3(2.4f, 1.1f, 2.4f)));
        Assert.That(Art.GetMethod("SpecFor", new[] { typeof(string) }).Invoke(null, new object[] { "structure_kiln" }), Is.SameAs(spec));
    }

    [Test]
    public void AKindWithNoRowAndAnUnknownIdHaveNoSpec()
    {
        foreach (string kind in new[] { "Well", "Smithy", "KilnSite", "Cabin", "WoodStorage" })
            Assert.That(Spec(Kind(kind)), Is.Null, kind + " has no row yet");
        Assert.That(Art.GetMethod("SpecFor", new[] { typeof(string) }).Invoke(null, new object[] { "structure_nothing" }), Is.Null);
        Assert.That(((IEnumerable)Art.GetProperty("AllSpecs").GetValue(null)).Cast<object>().Count(), Is.EqualTo(1), "the Kiln is the only pilot");
    }

    [Test]
    public void TheKilnRowAgreesWithThePrimitiveBuilder()
    {
        object spec = Spec(Kind("Kiln"));
        var empty = Build(Kiln("empty"), primitive: true);
        Assert.That((Vector3)Field(spec, "boxCenter"), Is.EqualTo(empty.center));
        Assert.That((Vector3)Field(spec, "boxSize"), Is.EqualTo(empty.size));
        Assert.That(empty.pieces.Count, Is.EqualTo((int)Field(spec, "staticPieces")), "pad, dome, five ring stones, vent, mouth");

        foreach (string phase in new[] { "loaded", "firing", "done" })
            Assert.That(Build(Kiln(phase), primitive: true).pieces.Count, Is.GreaterThanOrEqualTo((int)Field(spec, "staticPieces")), phase);
    }

    [Test]
    public void TheSmokeHeightConstantIsKilnSmokesDefault()
    {
        var host = Track(new GameObject("smoke height"));
        Component smoke = host.AddComponent(SmokeType);
        Assert.That((float)Art.GetField("KilnSmokeHeight").GetValue(null), Is.EqualTo(SmokeHeight(smoke)));
    }

    // --- Finding the prefab ---

    [Test]
    public void ResolveAsksForTheArtPathOnceAndRemembersAFoundPrefab()
    {
        var asked = new List<string>();
        GameObject prefab = MakePrefab();
        SetLoader(path => { asked.Add(path); return prefab; });
        MethodInfo resolve = Art.GetMethod("Resolve");
        Assert.That(resolve.Invoke(null, new object[] { "structure_kiln" }), Is.SameAs(prefab));
        Assert.That(resolve.Invoke(null, new object[] { "structure_kiln" }), Is.SameAs(prefab));
        Assert.That(asked, Is.EqualTo(new[] { "Art/structure_kiln" }), "Resources/Art/<id>.prefab, loaded once");
    }

    [Test]
    public void ResolveRemembersAMissingPrefabToo()
    {
        int asked = 0;
        SetLoader(path => { asked++; return null; });
        MethodInfo resolve = Art.GetMethod("Resolve");
        Assert.That(resolve.Invoke(null, new object[] { "structure_kiln" }), Is.Null);
        Assert.That(resolve.Invoke(null, new object[] { "structure_kiln" }), Is.Null);
        Assert.That(asked, Is.EqualTo(1), "'none' is cached");
        Art.GetMethod("ResetCache").Invoke(null, null);
        resolve.Invoke(null, new object[] { "structure_kiln" });
        Assert.That(asked, Is.EqualTo(2), "ResetCache forgets it");
        Assert.That(resolve.Invoke(null, new object[] { "" }), Is.Null);
        Assert.That(resolve.Invoke(null, new object[] { null }), Is.Null);
        Assert.That(asked, Is.EqualTo(2), "an empty id never asks");
    }

    // --- With no prefab ---

    [Test]
    public void WithNoPrefabTheKilnIsDrawnExactlyAsBefore()
    {
        SetLoader(path => null);
        foreach (string phase in new[] { "empty", "loaded", "firing", "done" })
        {
            var swapped = Build(Kiln(phase));
            var plain = Build(Kiln(phase), primitive: true);
            Assert.That(Instance(swapped.parent), Is.Null, phase + ": nothing was instantiated");
            Assert.That(swapped.pieces.Count, Is.EqualTo(plain.pieces.Count), phase);
            Assert.That(swapped.center, Is.EqualTo(plain.center));
            Assert.That(swapped.size, Is.EqualTo(plain.size));
            for (int i = 0; i < plain.pieces.Count; i++)
            {
                Assert.That(swapped.pieces[i].transform.localPosition, Is.EqualTo(plain.pieces[i].transform.localPosition), phase + " piece " + i);
                Assert.That(swapped.pieces[i].transform.localScale, Is.EqualTo(plain.pieces[i].transform.localScale), phase + " piece " + i);
                Assert.That(swapped.pieces[i].transform.parent, Is.SameAs(swapped.parent.transform), "the primitives are children of the structure as before");
            }
        }
    }

    [Test]
    public void AKindWithNoRowIsUntouchedEvenWhenTheLoaderHasAPrefab()
    {
        SetLoader(path => MakePrefab());
        var well = Build(NewPile("Well"));
        Assert.That(well.pieces.Count, Is.GreaterThan(0));
        Assert.That(well.parent.transform.childCount, Is.EqualTo(well.pieces.Count), "only the primitives");
    }

    // --- With a prefab ---

    [Test]
    public void APrefabIsInstantiatedOnceAsAChildNamedByItsIdAndKeptAcrossRebuilds()
    {
        GameObject prefab = MakePrefab();
        int loads = 0;
        SetLoader(path => { loads++; return prefab; });
        var first = Build(Kiln("empty"));
        Transform art = Instance(first.parent);
        Assert.That(art, Is.Not.Null);
        Assert.That(art.gameObject.activeSelf, Is.True, "the prefab's root was inactive; the instance is shown");
        Assert.That(first.pieces.Count, Is.EqualTo(0), "an empty Kiln has no changing contents, and the fixed shapes are the prefab");

        for (int i = 0; i < 3; i++)
            Build(Kiln("loaded"), parent: first.parent);
        Assert.That(first.parent.transform.Cast<Transform>().Count(t => t.name == "structure_kiln"), Is.EqualTo(1), "Rebuild reuses the one instance");
        Assert.That(Instance(first.parent), Is.SameAs(art));
        Assert.That(loads, Is.EqualTo(1));
        Assert.That(first.size, Is.EqualTo(new Vector3(2.2f, 1.5f, 2.2f)));
        Assert.That(first.center, Is.EqualTo(new Vector3(0f, 0.75f, 0f)));
    }

    [Test]
    public void EveryColliderInThePrefabIsStrippedAndSolidChildrenGetOneBack()
    {
        GameObject prefab = MakePrefab(solid: true);
        Assert.That(prefab.GetComponentsInChildren<Collider>(true).Length, Is.GreaterThan(5), "the stand-in has colliders on every shape");
        SetLoader(path => prefab);
        Transform art = Instance(Build(Kiln("empty")).parent);

        Collider[] left = art.GetComponentsInChildren<Collider>(true);
        Assert.That(left.Length, Is.EqualTo(1), "only Solid_Wall is solid");
        Assert.That(left[0].gameObject.name, Is.EqualTo("Solid_Wall"));
        Assert.That(left[0], Is.TypeOf<BoxCollider>());
        Assert.That(((BoxCollider)left[0]).size, Is.EqualTo(new Vector3(1f, 1f, 1f)), "fitted to the cube mesh; the transform scales it");
        Assert.That(art.GetComponentsInChildren<Collider>(true).Count(c => c.gameObject.name != "Solid_Wall"), Is.EqualTo(0), "Body and the states have none");
    }

    [Test]
    public void ThePrefabKilnHasNoCollidersOfItsOwnSoTheBuildersBoxIsTheOne()
    {
        SetLoader(path => MakePrefab());
        Transform art = Instance(Build(Kiln("empty")).parent);
        Assert.That(art.GetComponentsInChildren<Collider>(true).Length, Is.EqualTo(0));
    }

    [Test]
    public void ABodyTheAuthorLeftOffIsSwitchedOn()
    {
        GameObject prefab = MakePrefab();
        prefab.transform.Find("Body").gameObject.SetActive(false);
        SetLoader(path => prefab);
        Transform art = Instance(Build(Kiln("empty")).parent);
        Assert.That(art.Find("Body").gameObject.activeSelf, Is.True);
    }

    [Test]
    public void EachPhaseShowsExactlyItsStateChildAndOnlyFiringShowsTheGlow()
    {
        GameObject prefab = MakePrefab();
        SetLoader(path => prefab);
        var expect = new Dictionary<string, string>
        {
            { "empty", "State_Empty" }, { "loaded", "State_Loaded" }, { "firing", "State_Firing" }, { "done", "State_Done" },
        };
        GameObject parent = null;
        foreach (KeyValuePair<string, string> pair in expect)
        {
            var built = Build(Kiln(pair.Key), parent: parent);
            parent = built.parent;
            Transform art = Instance(parent);
            foreach (string name in expect.Values)
                Assert.That(art.Find(name).gameObject.activeSelf, Is.EqualTo(name == pair.Value), pair.Key + ": " + name);
            Assert.That(art.Find("On_Firing").gameObject.activeSelf, Is.EqualTo(pair.Key == "firing"), pair.Key + ": On_Firing");
            Assert.That(art.Find("Body").gameObject.activeSelf, Is.True);
        }
    }

    [Test]
    public void TheLoadingAndReadyPhasesBothShowTheLoadedState()
    {
        GameObject prefab = MakePrefab();
        SetLoader(path => prefab);
        object loading = NewPile("Kiln");
        Add(loading, PotteryRules.DryCupId, 1);
        Assert.That(Static("WellsAndOuthouses", "KilnPhaseOf", loading), Is.EqualTo(PotteryRules.KilnPhase.Loading));
        Assert.That(Instance(Build(loading).parent).Find("State_Loaded").gameObject.activeSelf, Is.True);
        Assert.That(Static("WellsAndOuthouses", "KilnPhaseOf", Kiln("loaded")), Is.EqualTo(PotteryRules.KilnPhase.Ready));
        Assert.That(Instance(Build(Kiln("loaded")).parent).Find("State_Loaded").gameObject.activeSelf, Is.True);
    }

    static object Static(string type, string method, params object[] args) =>
        Find(type).GetMethod(method, args.Select(a => a.GetType()).ToArray()).Invoke(null, args);

    [Test]
    public void APrefabWithoutAStateOrGlowChildIsSimplySkipped()
    {
        GameObject prefab = MakePrefab();
        UnityEngine.Object.DestroyImmediate(prefab.transform.Find("State_Done").gameObject);
        UnityEngine.Object.DestroyImmediate(prefab.transform.Find("On_Firing").gameObject);
        SetLoader(path => prefab);
        Assert.DoesNotThrow(() => Build(Kiln("done")));
        Assert.DoesNotThrow(() => Build(Kiln("firing")));
    }

    [Test]
    public void ANullPiecesListDoesNotThrowWithOrWithoutAPrefab()
    {
        SetLoader(path => null);
        foreach (string phase in new[] { "empty", "loaded", "firing", "done" })
            Assert.DoesNotThrow(() => Build(Kiln(phase), nullList: true), "no prefab, " + phase);

        GameObject prefab = MakePrefab();
        SetLoader(path => prefab);
        Art.GetMethod("ResetCache").Invoke(null, null);
        foreach (string phase in new[] { "empty", "loaded", "firing", "done" })
            Assert.DoesNotThrow(() => Build(Kiln(phase), nullList: true), "prefab, " + phase);
    }

    // --- The changing contents stay primitives at the anchors ---

    [Test]
    public void ContentsLandWhereTheyDoToday_ForEveryPhase_WhenTheAnchorsSitAtTheirRulePositions()
    {
        GameObject prefab = MakePrefab();
        SetLoader(path => prefab);
        object spec = Spec(Kind("Kiln"));
        int fixedShapes = (int)Field(spec, "staticPieces");

        foreach (string phase in new[] { "empty", "loaded", "firing", "done" })
        {
            var plain = Build(Kiln(phase), primitive: true);
            var swapped = Build(Kiln(phase));
            Assert.That(swapped.pieces.Count, Is.EqualTo(plain.pieces.Count - fixedShapes), phase + ": only the changing contents are drawn");
            for (int i = 0; i < swapped.pieces.Count; i++)
            {
                Vector3 now = swapped.parent.transform.InverseTransformPoint(swapped.pieces[i].transform.position);
                Vector3 before = plain.pieces[fixedShapes + i].transform.localPosition;
                Assert.That(Vector3.Distance(now, before), Is.LessThan(1e-4f), phase + " piece " + i);
                Assert.That(swapped.pieces[i].transform.localScale, Is.EqualTo(plain.pieces[fixedShapes + i].transform.localScale));
            }
        }
    }

    [Test]
    public void FirewoodIsParentedToAnchorStockAndThePiecesAndAshToAnchorMouth()
    {
        SetLoader(path => MakePrefab());
        var loaded = Build(Kiln("loaded"));
        Transform art = Instance(loaded.parent);
        Transform stock = art.Find("Anchor_Stock"), mouth = art.Find("Anchor_Mouth");
        Assert.That(loaded.pieces.Count(p => p.transform.parent == stock), Is.EqualTo(PotteryRules.KilnFirewood), "ten sticks of Firewood");
        Assert.That(loaded.pieces.Count(p => p.transform.parent == mouth), Is.EqualTo(3), "two cups and a bowl at the mouth");
        Assert.That(loaded.pieces.All(p => p.transform.parent == stock || p.transform.parent == mouth));

        var done = Build(Kiln("done"));
        Transform doneMouth = Instance(done.parent).Find("Anchor_Mouth");
        Assert.That(done.pieces.Count, Is.EqualTo(3), "two fired cups and the heap of Ash");
        Assert.That(done.pieces.All(p => p.transform.parent == doneMouth));
    }

    [Test]
    public void AMissingAnchorIsMadeAtTheRulePositionAndMovingAnAnchorMovesItsContents()
    {
        GameObject bare = MakePrefab(anchors: false);
        SetLoader(path => bare);
        var built = Build(Kiln("loaded"));
        Transform art = Instance(built.parent);
        Assert.That(art.Find("Anchor_Mouth").localPosition, Is.EqualTo(new Vector3(0f, 0.45f, 0.95f)));
        Assert.That(art.Find("Anchor_Stock").localPosition, Is.EqualTo(new Vector3(-1.45f, 0f, 0f)));
        var plain = Build(Kiln("loaded"), primitive: true);
        Vector3 mouthPiece = built.parent.transform.InverseTransformPoint(built.pieces.First(p => p.transform.parent == art.Find("Anchor_Mouth")).transform.position);
        Assert.That(Vector3.Distance(mouthPiece, plain.pieces[9 + PotteryRules.KilnFirewood].transform.localPosition), Is.LessThan(1e-4f));

        // An author who puts the mouth 0.2 m higher moves the pieces with it.
        GameObject moved = MakePrefab();
        moved.transform.Find("Anchor_Mouth").localPosition += new Vector3(0f, 0.2f, 0f);
        SetLoader(path => moved);
        Art.GetMethod("ResetCache").Invoke(null, null);
        var up = Build(Kiln("loaded"));
        Vector3 raised = up.parent.transform.InverseTransformPoint(up.pieces.First(p => p.transform.parent == Instance(up.parent).Find("Anchor_Mouth")).transform.position);
        Assert.That(raised.y - mouthPiece.y, Is.EqualTo(0.2f).Within(1e-4f));
    }

    // --- The smoke ---

    [Test]
    public void WithAPrefabTheSmokeIsMadeOnceAtAnchorSmokeAndSwitchedByTheFiring()
    {
        SetLoader(path => MakePrefab());
        var built = Build(Kiln("firing"));
        MethodInfo drive = Art.GetMethod("DriveSmoke");
        object[] args = { built.parent.transform, Kiln("firing"), null };
        Assert.That((bool)drive.Invoke(null, args), Is.True);
        var smoke = (Component)args[2];
        Assert.That(smoke, Is.Not.Null);
        Transform anchor = Instance(built.parent).Find("Anchor_Smoke");
        Assert.That(smoke.transform.parent, Is.SameAs(anchor));
        Assert.That(SmokeHeight(smoke), Is.EqualTo(0f));
        Assert.That(smoke.transform.localPosition, Is.EqualTo(Vector3.zero), "exactly on the anchor");
        Assert.That(SmokeFiring(smoke), Is.True);

        args[1] = Kiln("empty");
        Assert.That((bool)drive.Invoke(null, args), Is.True);
        Assert.That(args[2], Is.SameAs(smoke), "the same smoke is kept");
        Assert.That(SmokeFiring(smoke), Is.False);
        Assert.That(SmokesUnder(anchor.gameObject), Is.EqualTo(1));
    }

    [Test]
    public void AMissingSmokeAnchorIsMadeWhereTheSmokeIsToday()
    {
        SetLoader(path => MakePrefab(anchors: false));
        var built = Build(Kiln("firing"));
        object[] args = { built.parent.transform, Kiln("firing"), null };
        Art.GetMethod("DriveSmoke").Invoke(null, args);
        var smoke = (Component)args[2];
        Assert.That(Instance(built.parent).Find("Anchor_Smoke").localPosition, Is.EqualTo(new Vector3(0f, 1.1f, 0f)));
        Assert.That(built.parent.transform.InverseTransformPoint(smoke.transform.position).y, Is.EqualTo(1.1f).Within(1e-4f), "KilnSmoke's own height is 1.1 above the Kiln");
    }

    [Test]
    public void APrefabsOwnParticleSystemUnderTheAnchorIsUsedInsteadOfKilnSmoke()
    {
        SetLoader(path => MakePrefab(ownSmoke: true));
        var built = Build(Kiln("firing"));
        object[] args = { built.parent.transform, Kiln("firing"), null };
        Assert.That((bool)Art.GetMethod("DriveSmoke").Invoke(null, args), Is.True);
        Assert.That(args[2], Is.Null, "no KilnSmoke is made");
        Assert.That(SmokesUnder(built.parent), Is.EqualTo(0));
        ParticleSystem own = Instance(built.parent).Find("Anchor_Smoke").GetComponentInChildren<ParticleSystem>(true);
        Assert.That(own.isPlaying, Is.True, "switched on while it fires");
        args[1] = Kiln("empty");
        Art.GetMethod("DriveSmoke").Invoke(null, args);
        Assert.That(own.isEmitting, Is.False, "and off again");
    }

    [Test]
    public void WithNoPrefabInstanceDriveSmokeLeavesTheSmokeToPlacedStructure()
    {
        SetLoader(path => null);
        var built = Build(Kiln("firing"));
        object[] args = { built.parent.transform, Kiln("firing"), null };
        Assert.That((bool)Art.GetMethod("DriveSmoke").Invoke(null, args), Is.False);
        Assert.That(args[2], Is.Null);
        object[] well = { built.parent.transform, NewPile("Well"), null };
        Assert.That((bool)Art.GetMethod("DriveSmoke").Invoke(null, well), Is.False, "a kind with no row");
    }

    // --- The ghost ---

    [Test]
    public void TheGhostShowsThePrefabWithTheGreenOrRedMaterialOnEveryRenderer()
    {
        GameObject prefab = MakePrefab();
        Material original = prefab.GetComponentInChildren<Renderer>(true).sharedMaterial;
        SetLoader(path => prefab);

        object ghost = Find("PlacementGhost").GetMethod("ForPile").Invoke(null, new[] { Kind("Kiln") });
        try
        {
            var root = (GameObject)Find("PlacementGhost").GetProperty("Root").GetValue(ghost);
            Transform art = root.transform.Find("structure_kiln");
            Assert.That(art, Is.Not.Null, "the ghost draws the prefab");
            Assert.That(art.Find("State_Empty").gameObject.activeSelf, Is.True, "a new Kiln is empty");
            Assert.That(root.GetComponentsInChildren<Collider>(true).Length, Is.EqualTo(0), "nothing to bump into or aim at");

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            Assert.That(renderers.Length, Is.EqualTo(6), "Body, four States and the glow");
            Find("PlacementGhost").GetMethod("SetValid").Invoke(ghost, new object[] { true, false });
            Material green = renderers[0].sharedMaterial;
            Assert.That(renderers.All(r => r.sharedMaterial == green), Is.True, "all the same green material, hidden States and glow included");
            Assert.That(green, Is.Not.SameAs(original));

            Find("PlacementGhost").GetMethod("SetValid").Invoke(ghost, new object[] { false, false });
            Material red = renderers[0].sharedMaterial;
            Assert.That(renderers.All(r => r.sharedMaterial == red), Is.True, "and all red");
            Assert.That(red, Is.Not.SameAs(green));
        }
        finally
        {
            Find("PlacementGhost").GetMethod("DestroyNow").Invoke(ghost, null);
        }
    }

    // --- The swap point in the source, and the menu ---

    static string SourceText(string relative) => File.ReadAllText(Path.Combine(Application.dataPath, relative));

    [Test]
    public void TheHookIsTheFirstThingStructureVisualsBuildDoesAndTheBuildersAreUnchanged()
    {
        string source = SourceText("Scripts/World/PlacedStructure.cs");
        int build = source.IndexOf("public static void Build(Transform parent", StringComparison.Ordinal);
        int hook = source.IndexOf("ArtSwap.TryBuild(parent, state, bark, pieces, out boxCenter, out boxSize)", build, StringComparison.Ordinal);
        int primitive = source.IndexOf("BuildPrimitive(parent, state, bark, pieces, out boxCenter, out boxSize);", hook, StringComparison.Ordinal);
        Assert.That(build >= 0 && hook > build && primitive > hook, Is.True, "Build: the hook, then the primitives");
        Assert.That(hook - build, Is.LessThan(200), "the hook is the first line of Build");
        Assert.That(source.Contains("case PileKind.Kiln: BuildKiln(parent, state, bark, pieces, random, out boxCenter, out boxSize); return;"), Is.True);
        Assert.That(source.Contains("kilnSmoke = KilnSmoke.Create(transform);"), Is.True, "the primitive Kiln's own smoke code is still there");
        Assert.That(source.Contains("ArtSwap.DriveSmoke(transform, state, ref kilnSmoke)"), Is.True);
    }

    [Test]
    public void TheEditorMenuMakesAndRemovesTheTestKilnPrefabAndRefusesToTouchAnyOther()
    {
        string menu = SourceText("Scripts/Editor/ArtSwapMenu.cs");
        Assert.That(menu.Contains("[MenuItem(\"Homestead/Art/Create Test Kiln Prefab\")]"), Is.True);
        Assert.That(menu.Contains("[MenuItem(\"Homestead/Art/Remove Test Kiln Prefab\")]"), Is.True);
        Assert.That(menu.Contains("\"Assets/Resources/Art\"") && menu.Contains("\"/structure_kiln.prefab\""), Is.True, "Resources/Art/structure_kiln.prefab");
        Assert.That(menu.Contains("is not the generated test prefab"), Is.True, "Remove leaves a real prefab alone");
        Assert.That(menu.Contains("already exists"), Is.True, "Create never overwrites");
        Assert.That(Directory.Exists(Path.Combine(Application.dataPath, "Resources", "Art")), Is.False, "no test prefab is left in the project");
    }

    [Test]
    public void ArtSwapKnowsNothingOfSavesOrRules()
    {
        string source = SourceText("Scripts/World/ArtSwap.cs");
        foreach (string word in new[] { "SaveVersion", "LedgerSaveData", "PlayerPrefs", "SaveData", "File.", "Directory." })
            Assert.That(source.Contains(word), Is.False, word);
    }
}
