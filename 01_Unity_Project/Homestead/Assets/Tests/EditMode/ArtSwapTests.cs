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
        foreach (string kind in new[] { "KilnSite", "Cabin", "WoodStorage" })
            Assert.That(Spec(Kind(kind)), Is.Null, kind + " has no row yet");
        Assert.That(Art.GetMethod("SpecFor", new[] { typeof(string) }).Invoke(null, new object[] { "structure_nothing" }), Is.Null);
        Assert.That(((IEnumerable)Art.GetProperty("AllSpecs").GetValue(null)).Cast<object>().Count(), Is.EqualTo(13), "the Kiln, the six Wave 2 rows (2026-11-15), the Smithy, the Leach Hopper, the Kitchen and its three add-ons (2026-11-16)");
    }

    // The Wave 2 rows (Mike, 2026-11-15): each row's box and fixed-piece count are the primitive builder's for an empty one, so the changing contents start right after them.
    [Test]
    public void EachWaveTwoRowAgreesWithItsPrimitiveBuilder()
    {
        var rows = new[] { ("Well", "structure_well", 1), ("Outhouse", "structure_outhouse", 0), ("Workshop", "structure_workshop", 0),
                           ("PotterShed", "structure_pottershed", 0), ("CharcoalPit", "structure_charcoalpit", 0), ("Bloomery", "structure_bloomery", 0),
                           ("Smithy", "structure_smithy", 0), ("LeachHopper", "structure_leachhopper", 0),
                           ("Kitchen", "structure_kitchen", 0), ("Countertop", "structure_countertop", 0), ("Grill", "structure_grill", 0),
                           ("SlopSink", "structure_slopsink", 0) };
        foreach (var (kind, id, contentsWhenEmpty) in rows)
        {
            object spec = Spec(Kind(kind));
            Assert.That(spec, Is.Not.Null, kind);
            Assert.That(Field(spec, "id"), Is.EqualTo(id));
            var parent = new GameObject(kind + " primitive test");
            made.Add(parent);
            var pieces = new List<GameObject>();
            object[] args = { parent.transform, NewPile(kind), null, pieces, Vector3.zero, Vector3.zero };
            Find("StructureVisuals").GetMethod("BuildPrimitive", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic).Invoke(null, args);
            Assert.That((Vector3)Field(spec, "boxCenter"), Is.EqualTo((Vector3)args[4]), kind + " box centre");
            Assert.That((Vector3)Field(spec, "boxSize"), Is.EqualTo((Vector3)args[5]), kind + " box size");
            Assert.That(pieces.Count, Is.EqualTo((int)Field(spec, "staticPieces") + contentsWhenEmpty), kind + ": the fixed pieces, then the contents (the Well's water is always drawn, last)");
        }
        string source = SourceText("Scripts/World/PlacedStructure.cs");
        int bucket = source.IndexOf("// the bucket on its rope", StringComparison.Ordinal);
        int water = source.IndexOf("new Vector3(0f, 0.04f + 0.14f * fill, 0f)", StringComparison.Ordinal);
        Assert.That(bucket > 0 && water > bucket, Is.True, "BuildWell draws the water after every fixed piece");
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
        Assert.That(ArtFolderGuard.LeftoverTestArt(), Is.Empty, "no generated test prefab or test material folder is left under Resources/Art");
    }

    [Test]
    public void ArtSwapKnowsNothingOfSavesOrRules()
    {
        string source = SourceText("Scripts/World/ArtSwap.cs");
        foreach (string word in new[] { "SaveVersion", "LedgerSaveData", "PlayerPrefs", "SaveData", "File.", "Directory." })
            Assert.That(source.Contains(word), Is.False, word);
    }

    // --- The traps set out in the world (Mike, 2026-11-15) ---

    [Test]
    public void ASetTrapUsesItsArtByNameAndKeepsItsPrimitivesWithoutIt()
    {
        Type trap = Find("Trap");
        Assert.That(trap, Is.Not.Null);
        Assert.That(trap.GetField("ArtPrefix").GetValue(null), Is.EqualTo("Art/trap_"), "Resources/Art/trap_<item id>: trap_box_trap, trap_fish_trap, trap_rabbit_snare");
        Assert.That(trap.GetField("ArtBodyName").GetValue(null), Is.EqualTo("Body"));
        Assert.That(trap.GetField("ArtCatchName").GetValue(null), Is.EqualTo("Catch"));
        Assert.That(trap.GetField("ArtBaitName").GetValue(null), Is.EqualTo("Bait"));
        Assert.That(trap.GetField("ArtSetName").GetValue(null), Is.EqualTo("State_Set"), "the Box Trap's raised door, hidden once something is caught");

        string source = SourceText("Scripts/World/Trap.cs");
        Assert.That(source, Does.Contain("Resources.Load<GameObject>(ArtPrefix + TrapManager.ItemFor(state.type))"));
        Assert.That(source, Does.Contain("if (prefab == null)\n            return;").Or.Contain("if (prefab == null)\r\n            return;"), "no art: the prefab's own primitives stay");
        Assert.That(source, Does.Contain("setVisual.SetActive(!caught)"));
    }

    // --- The shelters: Small Cabin, Tent, Tarp Shelter, Lean-To (Mike, 2026-11-15) ---

    [Test]
    public void EveryShelterDrawsItsArtByNameAndKeepsThePrimitiveCollidersForShelterAndRain()
    {
        Type shelter = Find("Shelter");
        Assert.That(shelter, Is.Not.Null);
        Assert.That(shelter.GetField("ArtPrefix").GetValue(null), Is.EqualTo("Art/"));
        Type artSwap = Find("ArtSwap");
        Type kind = Find("PileKind");
        var ids = new System.Collections.Generic.List<string>();
        foreach (string name in new[] { "Cabin", "Tent", "TarpShelter", "LeanTo" })
            ids.Add((string)artSwap.GetMethod("StructureId", new[] { kind }).Invoke(null, new[] { Enum.Parse(kind, name) }));
        Assert.That(ids, Is.EqualTo(new[] { "structure_cabin", "structure_tent", "structure_tarpshelter", "structure_leanto" }));

        string source = SourceText("Scripts/World/Shelter.cs");
        Assert.That(source, Does.Contain("BuildLeanTo();\n        UseArt();").Or.Contain("BuildLeanTo();\r\n        UseArt();"), "the art goes in after every kind's primitives and colliders are built");
        Assert.That(source, Does.Contain("Resources.Load<GameObject>(ArtPrefix + ArtSwap.StructureId(state.kind))"));
        Assert.That(source, Does.Contain("if (prefab == null)\n            return;").Or.Contain("if (prefab == null)\r\n            return;"), "no art: the primitives stay");
        Assert.That(source, Does.Contain("primitive.enabled = false;"), "the primitives stop drawing but keep their colliders");
        Assert.That(source, Does.Not.Contain("Destroy(primitive"), "the roof panels' and the sheet's colliders keep the rain off, so nothing is destroyed");
    }

    // --- Wave 3 (Mike, 2026-11-16): storage and piles (WoodPile), the tripod and the campfire ---

    [Test]
    public void StorageAndPilesHideOnlyTheirContainerPiecesForTheirArt()
    {
        Type pile = Find("WoodPile");
        Assert.That(pile.GetField("ArtPrefix").GetValue(null), Is.EqualTo("Art/"));
        MethodInfo frame = pile.GetMethod("FramePieces");
        var expected = new[] { ("RockStorage", 6), ("WaterBarrel", 1), ("RainBarrel", 2), ("FoodCache", 2), ("StorageBin", 2), ("ToolRack", 3),
                               ("WoodShed", 18), ("Shelving", 7), ("HandCart", 10), ("Tent", 0), ("Felled", 0), ("CompostBin", 2), ("DryingRack", 5) };
        foreach (var (kind, count) in expected)
            Assert.That(frame.Invoke(null, new[] { NewPile(kind) }), Is.EqualTo(count), kind);

        string source = SourceText("Scripts/World/WoodPile.cs");
        Assert.That(source, Does.Contain("BuildPrimitives();\n        UseArt();").Or.Contain("BuildPrimitives();\r\n        UseArt();"), "the primitives first, then the art");
        Assert.That(source, Does.Contain("Resources.Load<GameObject>(ArtPrefix + ArtSwap.StructureId(state.kind))"));
        Assert.That(source, Does.Contain("primitive.enabled = false;"), "the container primitives stop drawing; the contents drawn after them stay");
        int rim = source.IndexOf("if (rain) // the rim is drawn with the barrel", StringComparison.Ordinal);
        int water = source.IndexOf("float fill = Mathf.Clamp01(state.Total / (float)Mathf.Max(1, capacity));", StringComparison.Ordinal);
        Assert.That(rim > 0 && water > rim, Is.True, "the Rain Barrel's rim is drawn before its water");
    }

    [Test]
    public void TheTripodFrameAndItsVesselUseTheirArtAndKeepTheirColliders()
    {
        Type tripod = Find("Tripod");
        Assert.That(tripod.GetField("FrameArtPath").GetValue(null), Is.EqualTo("Art/structure_tripod"));
        Assert.That(tripod.GetField("VesselArtPrefix").GetValue(null), Is.EqualTo("Art/tool_"), "the hung pot, clay pot or kettle is its own tool art");
        string source = SourceText("Scripts/World/Tripod.cs");
        Assert.That(source, Does.Contain("UseFrameArt();"));
        Assert.That(source, Does.Contain("UseVesselArt(hung);"));
        Assert.That(source, Does.Contain("art.transform.localScale *= Mathf.Max(1f, scale);"), "a hand-sized pot is grown to the drawn vessel's width on the tripod");
        Assert.That(source, Does.Contain("scale = Mathf.Min(scale, (ChainEnd - LowestVesselBottom) / height);"), "but never down into the fire's logs");
        Assert.That(source, Does.Contain("chainEnd.x - after.center.x"), "and centred under the chain");
        Assert.That(source, Does.Contain("primitive.enabled = false;"));
        Assert.That(source, Does.Not.Contain("Destroy(primitive"), "the poles' and the vessel's colliders stay");
    }

    [Test]
    public void TheCampfireArtTakesOverTheLogsAndAshItSwitches()
    {
        Type fire = Find("Campfire");
        Assert.That(fire.GetField("ArtPath").GetValue(null), Is.EqualTo("Art/effect_campfire"));
        string source = SourceText("Scripts/World/Campfire.cs");
        Assert.That(source, Does.Contain("logs = artLogs.gameObject;"));
        Assert.That(source, Does.Contain("ash = artAsh.gameObject;"));
        Assert.That(source, Does.Contain("void Awake()\n    {\n        UseArt();").Or.Contain("void Awake()\r\n    {\r\n        UseArt();"));
    }

    // Art Wave 4 (2026-11-16): a stump shows Resources/Art/world_stump, standing on the ground, widened with the trunk but not
    // taller, while the hidden cylinder stays the collider the Shovel digs at.
    [Test]
    public void AStumpShowsItsArtWidenedWithTheTrunkOnTheGround()
    {
        Type wood = Find("WoodManager");
        Assert.That(wood.GetField("StumpArtPath", Any).GetValue(null), Is.EqualTo("Art/world_stump"));
        Assert.That(Resources.Load<GameObject>("Art/world_stump"), Is.Not.Null);
        MethodInfo use = wood.GetMethod("UseStumpArt", Any);
        var made = new List<GameObject>();
        try
        {
            float lastWidth = 0f;
            foreach (float radius in new[] { 0.12f, 0.27f, 0.5f })
            {
                // set up exactly as WoodManager.SpawnStump does
                var stump = GameObject.CreatePrimitive(PrimitiveType.Cylinder); made.Add(stump);
                const float height = 0.45f, sink = 0.05f;
                var ground = new Vector3(10f, 3f, 4f);
                stump.transform.position = ground + Vector3.up * (height / 2f - sink);
                stump.transform.localScale = new Vector3(radius * 2.1f, height / 2f, radius * 2.1f);
                stump.transform.rotation = Quaternion.Euler(0f, 30f, 0f);
                use.Invoke(null, new object[] { stump, radius, height, sink });

                Transform art = stump.transform.Find("Art");
                Assert.That(art, Is.Not.Null, radius.ToString());
                Assert.That(stump.GetComponent<MeshRenderer>().enabled, Is.False, "the cylinder stops drawing");
                Assert.That(stump.GetComponent<Collider>(), Is.Not.Null, "the cylinder still takes the Shovel");
                Assert.That(art.GetComponentsInChildren<Collider>(true), Is.Empty);
                var drawn = new Bounds(); bool any = false;
                foreach (Renderer r in art.GetComponentsInChildren<Renderer>())
                {
                    if (!any) { drawn = r.bounds; any = true; } else drawn.Encapsulate(r.bounds);
                }
                Assert.That(drawn.max.y - ground.y, Is.InRange(0.38f, 0.46f), radius + ": the cut is at stump height whatever the trunk");
                Assert.That(drawn.min.y, Is.LessThan(ground.y).And.GreaterThan(ground.y - 0.12f), radius + ": the roots run just into the ground");
                Assert.That(drawn.center.x, Is.EqualTo(ground.x).Within(0.12f * radius / 0.3f + 0.02f), radius + ": centred on the tree");
                Assert.That(drawn.size.x, Is.GreaterThan(lastWidth), radius + ": a wider trunk leaves a wider stump");
                lastWidth = drawn.size.x;
            }
        }
        finally
        {
            foreach (GameObject g in made)
                UnityEngine.Object.DestroyImmediate(g);
        }
    }

    // Art Wave 4 (2026-11-16): a dropped pile of a raw material shows Resources/Art/item_<id> standing on the ground at real
    // size; the cube stays as the pickup (hidden) with its collider fitted around the art; an item with no art keeps its cube.
    [Test]
    public void ADroppedPileShowsItsItemArtStandingOnTheGroundInsideItsCollider()
    {
        Type dropped = Find("DroppedItems");
        Assert.That(dropped.GetField("ArtPrefix", Any).GetValue(null), Is.EqualTo("Art/item_"));
        MethodInfo spawn = dropped.GetMethod("Spawn", Any);
        MethodInfo get = Find("ItemDatabase").GetMethod("Get");
        var made = new List<GameObject>();
        try
        {
            foreach (string id in new[] { "logs", "branches", "firewood", "stone", "clay", "charcoal", "ash",
                                           "sticks", "kindling_bundle", "lumber", "sand", "iron_ore", "iron_bar", "nails_box",
                                           "clay_cup", "clay_bowl", "water_crock", "brick_batch", "dry_clay_cup", "dry_clay_bowl", "dry_water_crock",
                                           "dry_brick_batch", "dry_clay_pot", "door_hinges", "hook_set",
                                           "bass", "bluegill", "catfish", "crappie", "chicken_meat", "waterfowl_meat", "turkey_meat", "small_game_meat", "venison",
                                           "cooked_bass", "cooked_bluegill", "cooked_catfish", "cooked_crappie", "cooked_chicken", "cooked_waterfowl",
                                           "cooked_turkey", "cooked_small_game", "cooked_venison",
                                           "chicken_eggs", "feathers", "deer_hide", "small_furs", "deer_antlers", "sinew", "goat_milk",
            "blackberries", "raspberries", "chestnuts", "hickory_nuts", "walnuts", "wild_apples", "pawpaw", "morel", "oyster_mushroom", "dryads_saddle", "dandelion", "wild_onion", "cattail", "tall_grass",
            "rough_stool", "end_table", "chair", "table", "wooden_wheel", "arrows", "cordage", "tripod",
            "water", "water_excellent", "water_good", "water_questionable", "water_unsafe", "maple_sap", "maple_syrup", "lye", "lamp_oil", "potash", "rifle_rounds", "sleeping_bag", "tent", "weather_station", "axe", "fishing_rod", "shovel", "tarp", "bucket", "lantern" })
            {
                bool tool = Resources.Load<GameObject>("Art/item_" + id) == null;
                Assert.That(Resources.Load<GameObject>(tool ? "Art/tool_" + id : "Art/item_" + id), Is.Not.Null, id);
                object item = get.Invoke(null, new object[] { id });
                Assert.That(item, Is.Not.Null, id);
                var ground = new Vector3(5f, 2f, -3f);
                // unturned (yaw 0), so the world-axis boxes measured below are the model's own
                var pile = ((Component)spawn.Invoke(null, new object[] { item, 3, 10, ground, 0f, false })).gameObject;
                made.Add(pile);
                Transform art = pile.transform.Find("Art");
                Assert.That(art, Is.Not.Null, id + ": the art is shown");
                Assert.That(pile.GetComponent<MeshRenderer>().enabled, Is.False, id + ": the cube stops drawing");
                Assert.That(art.GetComponentsInChildren<Collider>(true), Is.Empty, id + ": the cube's collider is the only one");
                var drawn = new Bounds(); bool any = false;
                foreach (Renderer r in art.GetComponentsInChildren<Renderer>(false))
                {
                    if (!any) { drawn = r.bounds; any = true; } else drawn.Encapsulate(r.bounds);
                }
                Assert.That(any, Is.True, id);
                Assert.That(drawn.min.y, Is.EqualTo(ground.y).Within(0.006f), id + ": stands on the ground");
                Assert.That(drawn.center.x, Is.EqualTo(ground.x).Within(0.006f), id + ": centred on the pile");
                Assert.That(drawn.center.z, Is.EqualTo(ground.z).Within(0.006f), id + ": centred on the pile");
                Physics.SyncTransforms();
                Bounds box = pile.GetComponent<BoxCollider>().bounds;
                box.Expand(0.01f);
                Assert.That(box.Contains(drawn.min) && box.Contains(drawn.max), Is.True, id + ": the collider covers the whole model");
                if (tool)
                {
                    bool upright = id == "bucket" || id == "lantern";
                    foreach (Transform t in art.GetComponentsInChildren<Transform>(true))
                        if (t.name == "State_Lit" || t.name == "Fill")
                            Assert.That(t.gameObject.activeSelf, Is.False, id + ": dropped unlit and empty");
                    Transform model = art.GetChild(0);
                    if (upright)
                        Assert.That(Quaternion.Angle(model.localRotation, Quaternion.identity), Is.EqualTo(0f).Within(0.1f), id + ": a carried container stands upright");
                    else
                        Assert.That(drawn.size.y, Is.LessThan(Mathf.Min(drawn.size.x, drawn.size.z) + 0.001f).Or.LessThan(0.1f), id + ": a tool lies down, its thin side up");
                }
            }

            // Every item in the database has art now (Wave 4 batch 8, 2026-11-19), so the no-art case uses a made-up item.
            Type def = Find("ItemDefinition");
            var plain = ScriptableObject.CreateInstance(def);
            def.GetField("id", Any | BindingFlags.Instance).SetValue(plain, "test_item_without_art");
            def.GetField("displayName", Any | BindingFlags.Instance).SetValue(plain, "Test Item");
            def.GetField("weightKg", Any | BindingFlags.Instance).SetValue(plain, 1f);
            Assert.That(Resources.Load<GameObject>("Art/item_test_item_without_art"), Is.Null);
            var cube = ((Component)spawn.Invoke(null, new object[] { plain, 1, 10, Vector3.zero, 0f, false })).gameObject;
            made.Add(cube);
            Assert.That(cube.transform.Find("Art"), Is.Null);
            Assert.That(cube.GetComponent<MeshRenderer>().enabled, Is.True, "no art yet: the tinted cube still draws");
            UnityEngine.Object.DestroyImmediate(plain);
        }
        finally
        {
            foreach (GameObject g in made)
                UnityEngine.Object.DestroyImmediate(g);
        }
    }

    // Wave 4 item icons (2026-11-19): every item has a 128 px transparent icon in Resources/Icons, rendered from its art (ItemIconRenderer),
    // and ItemIcons finds it by id; an unknown id gives no icon rather than an error.
    [Test]
    public void EveryItemHasAnIconTheInventoryCanFind()
    {
        Type icons = Find("ItemIcons");
        Assert.That(icons.GetField("Prefix", Any).GetValue(null), Is.EqualTo("Icons/item_"));
        MethodInfo forId = icons.GetMethod("For", new[] { typeof(string) });
        int count = 0;
        foreach (object item in (IEnumerable)Find("ItemDatabase").GetProperty("All").GetValue(null))
        {
            string id = (string)item.GetType().GetProperty("Id").GetValue(item);
            var sprite = (Sprite)forId.Invoke(null, new object[] { id });
            Assert.That(sprite, Is.Not.Null, id + ": has an icon");
            Assert.That(sprite.texture.width, Is.EqualTo(128), id);
            Assert.That(sprite.texture.height, Is.EqualTo(128), id);
            count++;
        }
        Assert.That(count, Is.GreaterThan(100));
        Assert.That(forId.Invoke(null, new object[] { "no_such_item" }), Is.Null);
        Assert.That(forId.Invoke(null, new object[] { "" }), Is.Null);
    }

    // Build tab icons (2026-11-19): every structure kind (a ... Site shows the building it becomes), the Campfire and the Tripod have one in
    // Resources/Icons/build_<key>, and ItemIcons.ForBuild finds it by the Build entry's key.
    [Test]
    public void EveryBuildEntryHasAnIcon()
    {
        MethodInfo forBuild = Find("ItemIcons").GetMethod("ForBuild");
        var keys = new List<string> { "campfire", "tripod" };
        foreach (object kind in Enum.GetValues(Find("PileKind")))
            if (kind.ToString() != "Felled")
                keys.Add("pile:" + kind);
        foreach (string key in keys)
        {
            var sprite = (Sprite)forBuild.Invoke(null, new object[] { key });
            Assert.That(sprite, Is.Not.Null, key + ": has an icon");
            Assert.That(sprite.texture.width, Is.EqualTo(128), key);
        }
        Assert.That(keys.Count, Is.GreaterThan(40));
        Assert.That(forBuild.Invoke(null, new object[] { "pile:NoSuchKind" }), Is.Null);
    }
}
