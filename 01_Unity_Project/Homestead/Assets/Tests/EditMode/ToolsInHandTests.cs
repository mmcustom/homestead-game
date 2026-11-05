using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// EditMode tests for Hands and tools M0b (Mike, 2026-11-05): the chopping, digging, mining and building tools drawn in hand, with swing poses. SwingRules, SwingState, ToolWork and HandPoseRules are
// pure (Economy assembly) and tested directly; ToolViewPrimitives, the tool table in ArtSwap, ToolEvents and PlayerHands are in the main assembly and reached by reflection, as the hands tests do.
// A tool prefab is supplied through ArtSwap.Loader, so no asset is needed in Resources. What these cannot show: how the swing looks and feels in the real game, and the real tool scripts at
// real stumps, rocks and trees. Nothing here touches the Saves folder or a scene.
public class ToolsInHandTests
{
    static Type Find(string name) =>
        AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => { try { return a.GetTypes(); } catch (Exception) { return new Type[0]; } })
                 .First(t => t.Name == name);

    static Type Art => Find("ArtSwap");
    static Type HandsType => Find("PlayerHands");
    static Type Primitives => Find("ToolViewPrimitives");
    static Type Events => Find("ToolEvents");
    static int ViewLayer => LayerMask.NameToLayer("Viewmodel");

    static readonly string[] EightIds = { "axe", "primitive_axe", "wood_saw", "hammer", "shovel", "iron_shovel", "stone_pick_axe", "iron_pick_axe" };
    static readonly string[] FourIds = { "fishing_rod", "cane_pole", "torch", "lantern" };
    static readonly string[] RestIds = { "primitive_bow", "recurve_bow", "bolt_action_rifle", "bow_drill", "bucket", "sap_bucket", "canteen", "spile", "box_trap", "fish_trap", "rabbit_snare", "knife", "flint_and_steel", "pouch", "clay_pot", "cooking_pot", "potash_kettle", "tarp" };
    static readonly string[] TwoHandedIds = { "axe", "primitive_axe", "wood_saw", "shovel", "iron_shovel", "stone_pick_axe", "iron_pick_axe" };

    readonly List<GameObject> made = new List<GameObject>();

    [TearDown]
    public void TearDown()
    {
        foreach (GameObject go in made)
            if (go != null)
                UnityEngine.Object.DestroyImmediate(go);
        made.Clear();
        Art.GetMethod("ResetForTests").Invoke(null, null);
        Events.GetMethod("ResetForTests").Invoke(null, null);
    }

    GameObject Make(string name)
    {
        var go = new GameObject(name);
        made.Add(go);
        return go;
    }

    static string Source(string relative) => File.ReadAllText(Path.Combine(Application.dataPath, relative));

    static void Load(Func<string, GameObject> loader)
    {
        Art.GetMethod("ResetForTests").Invoke(null, null);
        Art.GetField("Loader").SetValue(null, loader);
    }

    // The hands, built for a camera and driven through reflection.
    sealed class H
    {
        public readonly Component Component;
        public H(Component component) { Component = component; }
        T Get<T>(string name) => (T)HandsType.GetProperty(name).GetValue(Component);
        public bool Build(Camera cam) => (bool)HandsType.GetMethod("Build").Invoke(Component, new object[] { cam });
        public void Apply(string tool, float raise = 1f) => HandsType.GetMethod("Apply").Invoke(Component, new object[] { tool, raise, 0f, 0f, HandMotion.Idle });
        public void OnSwung(string tool, WorkKind kind, bool finishing) => HandsType.GetMethod("OnSwung").Invoke(Component, new object[] { tool, kind, finishing });
        public void OnTired(string tool) => HandsType.GetMethod("OnTired").Invoke(Component, new object[] { tool });
        public Transform RightHand => Get<Transform>("RightHand");
        public Transform LeftHand => Get<Transform>("LeftHand");
        public Transform RightAnchor => Get<Transform>("RightAnchor");
        public Transform LeftAnchor => Get<Transform>("LeftAnchor");
        public Transform HeldRoot => Get<Transform>("HeldRoot");
        public Transform LeftGrip => Get<Transform>("LeftGrip");
        public SwingState Swing => Get<SwingState>("Swing");
        public HandRaise Raise => Get<HandRaise>("Raise");
    }

    H NewHands()
    {
        GameObject camObject = Make("Test Camera");
        Camera cam = camObject.AddComponent<Camera>();
        cam.cullingMask = -1;
        var hands = new H(Make("Test Player").AddComponent(HandsType));
        hands.Build(cam);
        return hands;
    }

    // A stand-in tool prefab with the grip away from the origin.
    GameObject StandInTool(bool withAnchors, Vector3 gripR)
    {
        GameObject root = Make("tool_stand_in");
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube); // a cube comes with a collider and a renderer
        body.name = "Body";
        body.transform.SetParent(root.transform, false);
        if (withAnchors)
        {
            var g = new GameObject("Anchor_Grip_R");
            g.transform.SetParent(root.transform, false);
            g.transform.localPosition = gripR;
        }
        root.SetActive(false);
        return root;
    }

    // ---- Pure: the swing ----

    [Test]
    public void TheContactMomentIsANamedConstantInsideTheSwing()
    {
        Assert.That(SwingRules.ContactSeconds, Is.EqualTo(0.15f));
        Assert.That(SwingRules.ContactSeconds + SwingRules.DwellSeconds, Is.LessThan(SwingRules.SwingSeconds));
    }

    [Test]
    public void TheSwingIsTheExistingZeroPointNineSeconds()
    {
        Assert.That(SwingRules.SwingSeconds, Is.EqualTo(0.9f));
        StringAssert.Contains("float swingSeconds = 0.9f", Source("Scripts/Player/AxeTool.cs"));
    }

    [Test]
    public void ThereIsNoWindUpTheHandHasNotMovedAtTheClick()
    {
        foreach (WorkKind kind in Enum.GetValues(typeof(WorkKind)))
        {
            HandPose atClick = SwingRules.Offset(kind, 0f, false);
            Assert.That(SwingRules.Distance(atClick, SwingRules.Cocked(kind)), Is.EqualTo(0f), kind + ": the hand is exactly where it was resting when the click lands");
            // and it moves away from there toward the strike, never back, in the first moments
            Assert.That(SwingRules.Distance(atClick, SwingRules.Offset(kind, 0.02f, false)), Is.LessThan(SwingRules.Distance(atClick, SwingRules.Offset(kind, 0.10f, false))), kind.ToString());
        }
    }

    [Test]
    public void TheStrikeLandsAtContactAndHoldsThroughTheDwell()
    {
        foreach (WorkKind kind in Enum.GetValues(typeof(WorkKind)))
        {
            HandPose strike = SwingRules.Strike(kind);
            HandPose atContact = SwingRules.Offset(kind, SwingRules.ContactSeconds, false);
            HandPose inDwell = SwingRules.Offset(kind, SwingRules.ContactSeconds + SwingRules.DwellSeconds * 0.5f, false);
            Assert.That(atContact.Pitch, Is.EqualTo(strike.Pitch).Within(1e-4f), kind.ToString());
            Assert.That(atContact.Z, Is.EqualTo(strike.Z).Within(1e-5f), kind.ToString());
            Assert.That(inDwell.Pitch, Is.EqualTo(strike.Pitch).Within(1e-4f), kind.ToString());
        }
    }

    [Test]
    public void TheRecoveryComesBackToLoadedExactlyAtTheEndOfTheSwing()
    {
        foreach (WorkKind kind in Enum.GetValues(typeof(WorkKind)))
        {
            HandPose loaded = SwingRules.Loaded(kind);
            Assert.That(SwingRules.Distance(SwingRules.Offset(kind, SwingRules.SwingSeconds, false), loaded), Is.EqualTo(0f), kind + " ends on the loaded pose at 0.9 s exactly");
            Assert.That(SwingRules.Distance(SwingRules.Offset(kind, SwingRules.SwingSeconds - 0.01f, false), loaded), Is.LessThan(0.05f), kind.ToString());
            // the recovery is visible: partway through it the hand is neither at the strike nor at loaded
            HandPose mid = SwingRules.Offset(kind, 0.5f, false);
            Assert.That(SwingRules.Distance(mid, loaded), Is.GreaterThan(0f), kind.ToString());
            Assert.That(SwingRules.Distance(mid, SwingRules.Strike(kind)), Is.GreaterThan(0f), kind.ToString());
        }
    }

    [Test]
    public void ASwingIsDeterministic()
    {
        HandPose a = SwingRules.Offset(WorkKind.Dig, 0.33f, true);
        HandPose b = SwingRules.Offset(WorkKind.Dig, 0.33f, true);
        Assert.That(a, Is.EqualTo(b));
    }

    [Test]
    public void TheFinishingBlowIsABiggerSwing()
    {
        foreach (WorkKind kind in Enum.GetValues(typeof(WorkKind)))
        {
            float normal = SwingRules.Magnitude(SwingRules.Offset(kind, SwingRules.ContactSeconds, false));
            float finishing = SwingRules.Magnitude(SwingRules.Offset(kind, SwingRules.ContactSeconds, true));
            Assert.That(finishing, Is.GreaterThan(normal), kind.ToString());
            Assert.That(finishing / normal, Is.EqualTo(SwingRules.FinishingScale).Within(0.02f), kind.ToString());
        }
    }

    [Test]
    public void TiredIsAHalfRaiseThatDroopsWithNoStrike()
    {
        Assert.That(SwingRules.Magnitude(SwingRules.TiredOffset(0f)), Is.EqualTo(0f));
        Assert.That(SwingRules.Magnitude(SwingRules.TiredOffset(SwingRules.TiredSeconds)), Is.EqualTo(0f));
        float earlyPitch = SwingRules.TiredOffset(SwingRules.TiredSeconds * 0.3f).Pitch;
        float latePitch = SwingRules.TiredOffset(SwingRules.TiredSeconds * 0.65f).Pitch;
        Assert.That(earlyPitch, Is.LessThan(0f), "it rises first (tool tilts up)");
        Assert.That(latePitch, Is.GreaterThan(0f), "then droops below ready");
        float most = 0f;
        for (float t = 0f; t < SwingRules.TiredSeconds; t += 0.01f)
            most = Mathf.Max(most, Mathf.Abs(SwingRules.TiredOffset(t).Pitch));
        Assert.That(most, Is.LessThan(SwingRules.Strike(WorkKind.Chop).Pitch * 0.5f), "nothing like a strike");
    }

    [Test]
    public void TheSawIsAShortPushPullNotAnArc()
    {
        float sawPitch = 0f, chopPitch = 0f;
        for (float t = 0f; t < SwingRules.SwingSeconds; t += 0.01f)
        {
            sawPitch = Mathf.Max(sawPitch, Mathf.Abs(SwingRules.Offset(WorkKind.Saw, t, false).Pitch));
            chopPitch = Mathf.Max(chopPitch, Mathf.Abs(SwingRules.Offset(WorkKind.Chop, t, false).Pitch));
        }
        Assert.That(sawPitch, Is.LessThan(4f));
        Assert.That(chopPitch, Is.GreaterThan(20f));
        Assert.That(SwingRules.Strike(WorkKind.Saw).Z, Is.GreaterThan(0.05f), "it pushes forward");
        Assert.That(SwingRules.Lift(WorkKind.Saw).Z, Is.LessThan(0f), "and pulls back");
    }

    [Test]
    public void TheWorkKindsAreOrderedByHowFarTheyReach()
    {
        float chop = SwingRules.Strike(WorkKind.Chop).Pitch, mine = SwingRules.Strike(WorkKind.Mine).Pitch, dig = SwingRules.Strike(WorkKind.Dig).Pitch, build = SwingRules.Strike(WorkKind.Build).Pitch;
        Assert.That(chop, Is.GreaterThan(mine), "mining is shorter than chopping");
        Assert.That(mine, Is.GreaterThan(dig));
        Assert.That(dig, Is.GreaterThan(build), "the hammer tap is the shortest");
        Assert.That(SwingRules.Strike(WorkKind.Mine).Z, Is.GreaterThan(SwingRules.Strike(WorkKind.Chop).Z), "mining reaches more forward");
        Assert.That(SwingRules.Strike(WorkKind.Dig).Roll, Is.Not.EqualTo(0f), "digging leans");
    }

    [Test]
    public void SplitIsTheSameArcAsChop()
    {
        Assert.That(SwingRules.Strike(WorkKind.Split), Is.EqualTo(SwingRules.Strike(WorkKind.Chop)));
        Assert.That(SwingRules.Lift(WorkKind.Split), Is.EqualTo(SwingRules.Lift(WorkKind.Chop)));
    }

    [Test]
    public void AHeldClickChainsSwingsSeamlessly()
    {
        var state = new SwingState();
        float previous = -1f;
        for (int blow = 0; blow < 3; blow++)
        {
            state.Start(WorkKind.Chop, false);
            Assert.That(SwingRules.Magnitude(state.Offset), Is.EqualTo(0f), "every blow starts from ready");
            float peak = 0f;
            int steps = Mathf.RoundToInt(SwingRules.SwingSeconds / 0.01f) + 2; // a hair past the end, for float rounding
            for (int i = 0; i < steps; i++)
            {
                state.Tick(0.01f);
                peak = Mathf.Max(peak, SwingRules.Magnitude(state.Offset));
            }
            Assert.That(state.Active, Is.False, "ready again by the time the next blow lands");
            if (previous >= 0f)
                Assert.That(peak, Is.EqualTo(previous).Within(1e-4f), "the same blow each time");
            previous = peak;
        }
    }

    [Test]
    public void ABlowReplacesTheOneInProgress()
    {
        var state = new SwingState();
        state.Start(WorkKind.Chop, false);
        state.Tick(0.4f);
        state.Start(WorkKind.Dig, false);
        Assert.That(state.Kind, Is.EqualTo(WorkKind.Dig));
        Assert.That(state.Time, Is.EqualTo(0f));
    }

    [Test]
    public void TheFinishingFlagOnlyCountsInTheSwingsFirstMoments()
    {
        var early = new SwingState();
        early.Start(WorkKind.Chop, false);
        early.MakeFinishing();
        Assert.That(early.Finishing, Is.True);

        var late = new SwingState();
        late.Start(WorkKind.Chop, false);
        late.Tick(0.3f);
        late.MakeFinishing();
        Assert.That(late.Finishing, Is.False);

        var tired = new SwingState();
        tired.StartTired();
        tired.MakeFinishing();
        Assert.That(tired.Finishing, Is.False);
    }

    [Test]
    public void TheTiredDroopRunsItsCourseAndEnds()
    {
        var state = new SwingState();
        state.StartTired();
        state.Tick(SwingRules.TiredSeconds * 0.5f);
        Assert.That(state.Active, Is.True);
        Assert.That(SwingRules.Magnitude(state.Offset), Is.GreaterThan(0f));
        state.Tick(SwingRules.TiredSeconds);
        Assert.That(state.Active, Is.False);
        Assert.That(SwingRules.Magnitude(state.Offset), Is.EqualTo(0f));
    }

    // ---- Pure: which tools, which work ----

    [Test]
    public void TheEightToolsAreTheOnesM0bDraws()
    {
        CollectionAssert.AreEquivalent(EightIds.Concat(FourIds).Concat(RestIds), ToolWork.ViewToolIds);
        Assert.That(ToolWork.IsViewTool("no_such_tool"), Is.False);
        Assert.That(ToolWork.IsViewTool(null), Is.False);
    }

    [Test]
    public void EachToolDoesItsKindOfWork()
    {
        Assert.That(ToolWork.KindFor("axe", false), Is.EqualTo(WorkKind.Chop));
        Assert.That(ToolWork.KindFor("primitive_axe", false), Is.EqualTo(WorkKind.Chop));
        Assert.That(ToolWork.KindFor("axe", true), Is.EqualTo(WorkKind.Split));
        Assert.That(ToolWork.KindFor("primitive_axe", true), Is.EqualTo(WorkKind.Split));
        Assert.That(ToolWork.KindFor("wood_saw", false), Is.EqualTo(WorkKind.Saw));
        Assert.That(ToolWork.KindFor("wood_saw", true), Is.EqualTo(WorkKind.Saw), "the saw still saws at a pile");
        Assert.That(ToolWork.KindFor("hammer", false), Is.EqualTo(WorkKind.Build));
        Assert.That(ToolWork.KindFor("shovel", false), Is.EqualTo(WorkKind.Dig));
        Assert.That(ToolWork.KindFor("iron_shovel", false), Is.EqualTo(WorkKind.Dig));
        Assert.That(ToolWork.KindFor("stone_pick_axe", false), Is.EqualTo(WorkKind.Mine));
        Assert.That(ToolWork.KindFor("iron_pick_axe", false), Is.EqualTo(WorkKind.Mine));
    }

    // ---- ToolViewPrimitives ----

    GameObject BuildPrimitive(string id, Transform parent) =>
        (GameObject)Primitives.GetMethod("Build").Invoke(null, new object[] { id, parent, ViewLayer });

    [Test]
    public void EachOfTheEightToolsHasAShapeWithItsAnchors()
    {
        Transform parent = Make("parent").transform;
        foreach (string id in EightIds)
        {
            GameObject tool = BuildPrimitive(id, parent);
            Assert.That(tool, Is.Not.Null, id);
            Assert.That(tool.name, Is.EqualTo("tool_" + id));
            Assert.That(tool.transform.Find("Anchor_Grip_R"), Is.Not.Null, id + " grip R");
            Assert.That(tool.transform.Find("Anchor_Tip"), Is.Not.Null, id + " tip");
            Assert.That(tool.transform.Find("Head"), Is.Not.Null, id + " head");
            Assert.That(tool.transform.Find("Handle"), Is.Not.Null, id + " handle");
            bool two = Array.IndexOf(TwoHandedIds, id) >= 0;
            Assert.That(tool.transform.Find("Anchor_Grip_L") != null, Is.EqualTo(two), id + " grip L only on two-handed tools");
        }
    }

    [Test]
    public void TheSharedHandedTableAgreesWithTheToolShapes()
    {
        foreach (string id in EightIds)
            Assert.That(HeldTools.IsTwoHanded(id), Is.EqualTo(Array.IndexOf(TwoHandedIds, id) >= 0), id);
    }

    [Test]
    public void TheGripSitsAtTheOriginAndTheToolRunsAlongPlusZ()
    {
        Transform parent = Make("parent").transform;
        foreach (string id in EightIds)
        {
            GameObject tool = BuildPrimitive(id, parent);
            Assert.That(tool.transform.Find("Anchor_Grip_R").localPosition, Is.EqualTo(Vector3.zero), id);
            Assert.That(tool.transform.Find("Anchor_Tip").localPosition.z, Is.GreaterThan(0.25f), id + " the working end is out along +Z");
        }
    }

    static void AssertColour(Color actual, Color expected)
    {
        Assert.That(actual.r, Is.EqualTo(expected.r).Within(1e-4f));
        Assert.That(actual.g, Is.EqualTo(expected.g).Within(1e-4f));
        Assert.That(actual.b, Is.EqualTo(expected.b).Within(1e-4f));
    }

    [Test]
    public void StoneAndIronHeadsAreDifferentColours()
    {
        Transform parent = Make("parent").transform;
        Color Head(string id)
        {
            GameObject tool = BuildPrimitive(id, parent);
            var block = new MaterialPropertyBlock();
            tool.transform.Find("Head").GetComponent<Renderer>().GetPropertyBlock(block);
            return block.GetColor("_BaseColor");
        }
        Assert.That(Head("shovel"), Is.Not.EqualTo(Head("iron_shovel")));
        Assert.That(Head("stone_pick_axe"), Is.Not.EqualTo(Head("iron_pick_axe")));
        Assert.That(Head("primitive_axe"), Is.Not.EqualTo(Head("axe")));
        AssertColour(Head("shovel"), (Color)Primitives.GetField("StoneGrey").GetValue(null));
        AssertColour(Head("iron_shovel"), (Color)Primitives.GetField("Steel").GetValue(null));
    }

    [Test]
    public void ThePrimitiveToolsAreOnTheViewmodelLayerWithNoCollidersOrShadows()
    {
        Transform parent = Make("parent").transform;
        foreach (string id in EightIds)
        {
            GameObject tool = BuildPrimitive(id, parent);
            Assert.That(tool.GetComponentsInChildren<Collider>(true).Length, Is.EqualTo(0), id);
            foreach (Transform t in tool.GetComponentsInChildren<Transform>(true))
                Assert.That(t.gameObject.layer, Is.EqualTo(ViewLayer), id + "/" + t.name);
            foreach (Renderer r in tool.GetComponentsInChildren<Renderer>(true))
                Assert.That(r.shadowCastingMode, Is.EqualTo(UnityEngine.Rendering.ShadowCastingMode.Off), id);
        }
    }

    [Test]
    public void AToolOutsideTheEightGetsNoShape()
    {
        Transform parent = Make("parent").transform;
        Assert.That(BuildPrimitive("no_such_tool", parent), Is.Null);
        Assert.That(BuildPrimitive("sleeping_bag", parent), Is.Null);
    }

    // ---- The tool swap point in ArtSwap ----

    [Test]
    public void TheToolTableHasARowForEachOfTheEightAndNothingElse()
    {
        var specs = ((System.Collections.IEnumerable)Art.GetProperty("AllToolSpecs").GetValue(null)).Cast<object>().ToList();
        Assert.That(specs.Count, Is.EqualTo(30));
        var ids = specs.Select(s => (string)s.GetType().GetField("id").GetValue(s)).ToList();
        CollectionAssert.AreEquivalent(EightIds.Concat(FourIds).Concat(RestIds).Select(i => "tool_" + i), ids);
        Assert.That(Art.GetMethod("ToolSpecFor").Invoke(null, new object[] { "no_such_tool" }), Is.Null);
        Assert.That(Art.GetMethod("ToolSpecFor").Invoke(null, new object[] { "tool_axe" }), Is.Null, "keyed by the item id, not the prefab id");
    }

    [Test]
    public void EveryRowNamesTheThreeAnchors()
    {
        foreach (string id in EightIds)
        {
            object spec = Art.GetMethod("ToolSpecFor").Invoke(null, new object[] { id });
            MethodInfo anchor = spec.GetType().GetMethod("Anchor");
            foreach (string name in new[] { "Anchor_Grip_R", "Anchor_Grip_L", "Anchor_Tip" })
                Assert.That(anchor.Invoke(spec, new object[] { name }), Is.Not.Null, id + "/" + name);
        }
    }

    object TryAttach(Transform parent, string id) => Art.GetMethod("TryAttachTool").Invoke(null, new object[] { parent, id, ViewLayer });

    [Test]
    public void AMissingToolPrefabIsLookedUpOnceAndRemembered()
    {
        int calls = 0;
        Load(path => { calls++; return null; });
        Transform parent = Make("parent").transform;
        Assert.That((Transform)TryAttach(parent, "axe"), Is.Null);
        Assert.That((Transform)TryAttach(parent, "axe"), Is.Null);
        Assert.That(calls, Is.EqualTo(1));
    }

    [Test]
    public void AFoundToolPrefabIsInstancedWithNoCollidersOnTheLayerCastingNoShadows()
    {
        GameObject prefab = StandInTool(true, new Vector3(0f, 0f, 0.2f));
        int calls = 0;
        Load(path => { calls++; return path == "Art/tool_axe" ? prefab : null; });
        Transform parent = Make("parent").transform;
        var tool = (Transform)TryAttach(parent, "axe");
        Assert.That(tool, Is.Not.Null);
        Assert.That(tool.name, Is.EqualTo("tool_axe"));
        Assert.That(tool.gameObject.activeSelf, Is.True);
        Assert.That(tool.GetComponentsInChildren<Collider>(true).Length, Is.EqualTo(0));
        foreach (Transform t in tool.GetComponentsInChildren<Transform>(true))
            Assert.That(t.gameObject.layer, Is.EqualTo(ViewLayer), t.name);
        foreach (Renderer r in tool.GetComponentsInChildren<Renderer>(true))
            Assert.That(r.shadowCastingMode, Is.EqualTo(UnityEngine.Rendering.ShadowCastingMode.Off));
        TryAttach(parent, "axe");
        Assert.That(calls, Is.EqualTo(1), "the prefab was looked up once");
    }

    [Test]
    public void StoneAndIronHaveSeparateIdsWithNoFallback()
    {
        GameObject stone = StandInTool(true, Vector3.zero);
        Load(path => path == "Art/tool_shovel" ? stone : null);
        Transform parent = Make("parent").transform;
        Assert.That((Transform)TryAttach(parent, "shovel"), Is.Not.Null);
        Assert.That((Transform)TryAttach(parent, "iron_shovel"), Is.Null, "no tool_iron_shovel prefab: the iron shovel is not drawn from the stone one");
    }

    [Test]
    public void AToolOutsideTheTableIsNeverLookedUp()
    {
        int calls = 0;
        Load(path => { calls++; return null; });
        Assert.That((Transform)TryAttach(Make("parent").transform, "no_such_tool"), Is.Null);
        Assert.That(calls, Is.EqualTo(0));
    }

    [Test]
    public void MissingToolAnchorsFallBackToThePrimitivesGrip()
    {
        Load(path => path == "Art/tool_axe" ? StandInTool(false, Vector3.zero) : null);
        var tool = (Transform)TryAttach(Make("parent").transform, "axe");
        object spec = Art.GetMethod("ToolSpecFor").Invoke(null, new object[] { "axe" });
        MethodInfo anchorOf = Art.GetMethod("ToolAnchor");
        var gripR = (Transform)anchorOf.Invoke(null, new object[] { tool, spec, "Anchor_Grip_R" });
        var gripL = (Transform)anchorOf.Invoke(null, new object[] { tool, spec, "Anchor_Grip_L" });
        var tip = (Transform)anchorOf.Invoke(null, new object[] { tool, spec, "Anchor_Tip" });
        Assert.That(gripR.localPosition, Is.EqualTo(Vector3.zero));
        object layout = Primitives.GetMethod("LayoutFor").Invoke(null, new object[] { "axe" });
        Assert.That(gripL.localPosition, Is.EqualTo((Vector3)layout.GetType().GetField("gripLeft").GetValue(layout)));
        Assert.That(tip.localPosition, Is.EqualTo((Vector3)layout.GetType().GetField("tip").GetValue(layout)));
    }

    [Test]
    public void AOneHandedToolHasNoLeftGripAnchor()
    {
        Load(path => path == "Art/tool_hammer" ? StandInTool(true, Vector3.zero) : null);
        var tool = (Transform)TryAttach(Make("parent").transform, "hammer");
        object spec = Art.GetMethod("ToolSpecFor").Invoke(null, new object[] { "hammer" });
        Assert.That(Art.GetMethod("ToolAnchor").Invoke(null, new object[] { tool, spec, "Anchor_Grip_L" }), Is.Null);
    }

    // ---- ShowHeld: prefab, primitive or generic ----

    [Test]
    public void WithNoPrefabTheEightToolsAreTheirPrimitives()
    {
        Load(path => null);
        H hands = NewHands();
        foreach (string id in EightIds)
        {
            hands.Apply(id);
            Assert.That(hands.HeldRoot, Is.Not.Null, id);
            Assert.That(hands.HeldRoot.name, Is.EqualTo("tool_" + id), id);
            Assert.That(hands.HeldRoot.Find("Head"), Is.Not.Null, id);
        }
    }

    [Test]
    public void WithAPrefabTheToolIsThePrefab()
    {
        Load(path => path == "Art/tool_axe" ? StandInTool(true, new Vector3(0f, 0f, 0.25f)) : null);
        H hands = NewHands();
        hands.Apply("axe");
        Assert.That(hands.HeldRoot.name, Is.EqualTo("tool_axe"));
        Assert.That(hands.HeldRoot.Find("Body"), Is.Not.Null, "the prefab's own part");
        Assert.That(hands.HeldRoot.Find("Head"), Is.Null, "none of the primitive parts");
    }

    [Test]
    public void ThePrefabsGripIsPlacedOnTheHandsPalm()
    {
        Load(path => path == "Art/tool_axe" ? StandInTool(true, new Vector3(0.03f, 0.05f, 0.25f)) : null);
        H hands = NewHands();
        hands.Apply("axe");
        Transform gripR = hands.HeldRoot.Find("Anchor_Grip_R");
        Assert.That(Vector3.Distance(gripR.position, hands.RightAnchor.position), Is.LessThan(1e-4f));
    }

    [Test]
    public void ToolsOutsideTheEightKeepTheGenericPlaceholder()
    {
        Load(path => null);
        H hands = NewHands();
        foreach (string id in new[] { "no_such_tool", "mystery_tool", "tent" })
        {
            hands.Apply(id);
            Assert.That(hands.HeldRoot.name, Is.EqualTo("Held Placeholder"), id);
        }
    }

    [Test]
    public void APutAwayFreesTheTool()
    {
        Load(path => null);
        H hands = NewHands();
        hands.Apply("axe");
        hands.Apply("shovel");
        Assert.That(hands.RightAnchor.childCount, Is.EqualTo(1), "the axe was replaced, not added to");
        hands.Apply(null);
        Assert.That(hands.RightAnchor.childCount, Is.EqualTo(0));
        Assert.That(hands.HeldRoot, Is.Null);
    }

    [Test]
    public void TheLeftHandFollowsTheLeftGripOnTwoHandedToolsAndIsHiddenOnTheHammer()
    {
        Load(path => null);
        H hands = NewHands();
        foreach (string id in TwoHandedIds)
        {
            hands.Apply(id);
            Assert.That(hands.LeftHand.gameObject.activeSelf, Is.True, id);
            Assert.That(hands.LeftGrip, Is.Not.Null, id);
            Assert.That(Vector3.Distance(hands.LeftAnchor.position, hands.LeftGrip.position), Is.LessThan(1e-3f), id + " the left palm is on the left grip");
        }
        hands.Apply("hammer");
        Assert.That(hands.LeftHand.gameObject.activeSelf, Is.False);
        Assert.That(hands.LeftGrip, Is.Null);
    }

    [Test]
    public void TheLeftHandStaysOnTheGripWhileTheSwingMovesTheTool()
    {
        Load(path => null);
        H hands = NewHands();
        hands.Apply("axe");
        hands.Raise.Snap("axe"); // the hands only swing for the tool they are showing
        hands.OnSwung("axe", WorkKind.Chop, false);
        hands.Swing.Seek(SwingRules.ContactSeconds);
        hands.Apply("axe");
        Assert.That(Vector3.Distance(hands.LeftAnchor.position, hands.LeftGrip.position), Is.LessThan(1e-3f));
    }

    // ---- The swing in the hands ----

    [Test]
    public void ABlowMovesTheRightHandAndTheRecoveryBringsItBackToLoadedThenCocked()
    {
        Load(path => null);
        H hands = NewHands();
        hands.Apply("axe");
        Quaternion ready = hands.RightHand.localRotation; // cocked
        hands.Raise.Snap("axe"); // the hands only swing for the tool they are showing
        hands.OnSwung("axe", WorkKind.Chop, false);
        hands.Swing.Seek(SwingRules.ContactSeconds);
        hands.Apply("axe");
        Assert.That(Quaternion.Angle(ready, hands.RightHand.localRotation), Is.GreaterThan(10f), "the hand has turned down into the strike from the cocked pose");
        hands.Swing.Seek(SwingRules.SwingSeconds - 0.001f);
        hands.Swing.Tick(0.002f); // the swing ends
        Assert.That(hands.Swing.Active, Is.False);
        hands.Apply("axe");
        float loadedAngle = Quaternion.Angle(ready, hands.RightHand.localRotation);
        float expected = Mathf.Abs(SwingRules.ChopLoadPitch - SwingRules.ChopCockPitch) * SwingRules.HandPitchShare;
        Assert.That(loadedAngle, Is.GreaterThan(expected - 3f).And.LessThan(expected + 4f), "it ends loaded: pulled back further than cocked");
        hands.Swing.Tick(SwingRules.SettleSeconds + 0.01f);
        hands.Apply("axe");
        Assert.That(Quaternion.Angle(ready, hands.RightHand.localRotation), Is.LessThan(1f), "and with no new blow settles back to cocked");
    }

    [Test]
    public void TheSwingTurnsTheHandAboutTheElbowSoTheForearmStaysPut()
    {
        Load(path => null);
        H hands = NewHands();
        hands.Apply("axe");
        Vector3 wristBefore = hands.RightHand.localPosition;
        Vector3 elbowBefore = wristBefore + hands.RightHand.localRotation * new Vector3(0f, 0f, -SwingRules.ElbowBack);
        hands.Raise.Snap("axe"); // the hands only swing for the tool they are showing
        hands.OnSwung("axe", WorkKind.Chop, false);
        hands.Swing.Seek(SwingRules.ContactSeconds);
        hands.Apply("axe");
        Vector3 elbowAfter = hands.RightHand.localPosition + hands.RightHand.localRotation * new Vector3(0f, 0f, -SwingRules.ElbowBack);
        float elbowMoved = Vector3.Distance(elbowBefore, elbowAfter);
        float wristMoved = Vector3.Distance(wristBefore, hands.RightHand.localPosition);
        HandPose strike = SwingRules.Strike(WorkKind.Chop), cocked = SwingRules.Cocked(WorkKind.Chop);
        float push = new Vector3(strike.X - cocked.X, strike.Y - cocked.Y, strike.Z - cocked.Z).magnitude;
        Assert.That(elbowMoved, Is.EqualTo(push).Within(0.005f), "the elbow moves only by the strike's own push from cocked, not by the turn");
        Assert.That(wristMoved, Is.GreaterThan(elbowMoved * 0.8f), "while the wrist also moves, turning about the elbow");
    }

    [Test]
    public void ABlowForAnotherToolIsIgnored()
    {
        Load(path => null);
        H hands = NewHands();
        hands.Apply("axe");
        hands.Raise.Snap("axe");
        hands.OnSwung("shovel", WorkKind.Dig, false);
        Assert.That(hands.Swing.Active, Is.False);
        hands.OnTired("shovel");
        Assert.That(hands.Swing.Active, Is.False);
    }

    [Test]
    public void ASwingFollowsTheEventsTheHandsListenTo()
    {
        Load(path => null);
        H hands = NewHands();
        hands.Raise.Snap("axe");
        hands.OnSwung("axe", WorkKind.Chop, false);
        Assert.That(hands.Swing.Active, Is.True);
        Assert.That(hands.Swing.Kind, Is.EqualTo(WorkKind.Chop));
        hands.OnSwung("axe", WorkKind.Chop, true); // the same frame: the same swing, made bigger
        Assert.That(hands.Swing.Finishing, Is.True);
        Assert.That(hands.Swing.Time, Is.EqualTo(0f), "not restarted");
        hands.OnTired("axe");
        Assert.That(hands.Swing.Tired, Is.True);
    }

    // ---- ToolEvents ----

    object Subscribe(string eventName, Delegate handler)
    {
        EventInfo e = Events.GetEvent(eventName);
        e.AddEventHandler(null, handler);
        return e;
    }

    [Test]
    public void ABlowRaisesSwungWithTheKindAndAFinishingBlowRaisesItAgainFlagged()
    {
        var seen = new List<(string tool, WorkKind kind, bool finishing)>();
        Action<string, WorkKind, bool> handler = (tool, kind, finishing) => seen.Add((tool, kind, finishing));
        EventInfo e = Events.GetEvent("Swung");
        e.AddEventHandler(null, handler);
        try
        {
            Events.GetMethod("RaiseSwung").Invoke(null, new object[] { "iron_shovel" });
            Events.GetMethod("JobFinished").Invoke(null, null);
        }
        finally { e.RemoveEventHandler(null, handler); }
        Assert.That(seen.Count, Is.EqualTo(2));
        Assert.That(seen[0], Is.EqualTo(("iron_shovel", WorkKind.Dig, false)));
        Assert.That(seen[1], Is.EqualTo(("iron_shovel", WorkKind.Dig, true)));
    }

    [Test]
    public void AJobFinishedWithNoBlowThisFrameRaisesNothing()
    {
        int count = 0;
        Action<string, WorkKind, bool> handler = (t, k, f) => count++;
        EventInfo e = Events.GetEvent("Swung");
        e.AddEventHandler(null, handler);
        try { Events.GetMethod("JobFinished").Invoke(null, null); }
        finally { e.RemoveEventHandler(null, handler); }
        Assert.That(count, Is.EqualTo(0));
    }

    [Test]
    public void AWoodPileBlowIsASplitForTheAxesButTheSawStillSaws()
    {
        var kinds = new List<WorkKind>();
        Action<string, WorkKind, bool> handler = (t, k, f) => kinds.Add(k);
        EventInfo e = Events.GetEvent("Swung");
        e.AddEventHandler(null, handler);
        try
        {
            Events.GetMethod("WorkingOn").Invoke(null, new object[] { WorkKind.Split });
            Events.GetMethod("RaiseSwung").Invoke(null, new object[] { "axe" });
            Events.GetMethod("RaiseSwung").Invoke(null, new object[] { "wood_saw" });
        }
        finally { e.RemoveEventHandler(null, handler); }
        Assert.That(kinds[0], Is.EqualTo(WorkKind.Split));
        Assert.That(kinds[1], Is.EqualTo(WorkKind.Saw));
    }

    [Test]
    public void TiredRaisesItsOwnEvent()
    {
        var seen = new List<string>();
        Action<string> handler = t => seen.Add(t);
        EventInfo e = Events.GetEvent("Tired");
        e.AddEventHandler(null, handler);
        try { Events.GetMethod("RaiseTired").Invoke(null, new object[] { "hammer" }); }
        finally { e.RemoveEventHandler(null, handler); }
        CollectionAssert.AreEqual(new[] { "hammer" }, seen);
    }

    // ---- The hooks in AxeTool, the swap point and the menu (source) ----

    [Test]
    public void AxeToolRaisesTheEventsWithTheSmallestHooks()
    {
        string axe = Source("Scripts/Player/AxeTool.cs");
        Assert.That(System.Text.RegularExpressions.Regex.Matches(axe, @"ToolEvents\.RaiseSwung\(Equipped\)").Count, Is.EqualTo(1), "one line where a blow lands");
        Assert.That(System.Text.RegularExpressions.Regex.Matches(axe, @"ToolEvents\.RaiseTired\(Equipped\)").Count, Is.EqualTo(1), "one line where a blow is refused");
        Assert.That(System.Text.RegularExpressions.Regex.Matches(axe, @"ToolEvents\.WorkingOn\(WorkKind\.Split\)").Count, Is.EqualTo(1), "one line before a blow at a wood pile");
        Assert.That(System.Text.RegularExpressions.Regex.Matches(axe, @"ToolEvents\.JobFinished\(\)").Count, Is.EqualTo(8), "one line at each job's finishing point");
        // the blow's own lines are still there, in order: stamina, the interval, then the event
        int spend = axe.IndexOf("player.TrySpendStamina(staminaPerSwing)");
        int interval = axe.IndexOf("nextSwing = Time.time + swingSeconds;");
        int raised = axe.IndexOf("ToolEvents.RaiseSwung(Equipped)");
        Assert.That(spend, Is.GreaterThan(0));
        Assert.That(interval, Is.GreaterThan(spend));
        Assert.That(raised, Is.GreaterThan(interval));
    }

    [Test]
    public void NoOtherToolScriptRaisesToolEventsYet()
    {
        foreach (string file in new[] { "Player/CartHandler.cs", "Wildlife/Carcass.cs", "Managers/TrapManager.cs" })
            StringAssert.DoesNotContain("ToolEvents", Source("Scripts/" + file), file);
    }

    [Test]
    public void ShowHeldAsksForThePrefabBeforeTheToolShape()
    {
        string hands = Source("Scripts/Player/PlayerHands.cs");
        int prefab = hands.IndexOf("ArtSwap.TryAttachTool");
        int primitive = hands.IndexOf("ToolViewPrimitives.Build(toolId");
        Assert.That(prefab, Is.GreaterThan(0));
        Assert.That(primitive, Is.GreaterThan(prefab), "the prefab is asked first");
    }

    [Test]
    public void TheHudAndTheToolScriptsIdleTextAreUntouched()
    {
        string hands = Source("Scripts/Player/PlayerHands.cs") + Source("Scripts/Player/ToolViewPrimitives.cs") + Source("Scripts/Economy/SwingRules.cs") + Source("Scripts/Player/ToolEvents.cs");
        StringAssert.DoesNotContain("ToolStatus", hands);
        StringAssert.DoesNotContain("ToolHud", hands);
        StringAssert.DoesNotContain("SaveVersion", hands);
        StringAssert.DoesNotContain("LedgerSaveData", hands);
        StringAssert.Contains("hold click on a tree trunk to chop it down", Source("Scripts/Player/AxeTool.cs"));
    }

    [Test]
    public void TheTestAxeMenuItemsExistAndRemoveOnlyTheTestPrefab()
    {
        string menu = Source("Scripts/Editor/ArtSwapMenu.cs");
        StringAssert.Contains("Homestead/Art/Create Test Axe Prefab", menu);
        StringAssert.Contains("Homestead/Art/Remove Test Axe Prefab", menu);
        StringAssert.Contains("TestAxe_Marker", menu);
        StringAssert.Contains("Anchor", menu);
        Assert.That(File.Exists(Path.Combine(Application.dataPath, "Resources/Art/tool_axe.prefab")), Is.False, "the test prefab is never left behind");
    }

    // ---- The pull-back: cocked ready pose, loaded pose, settle (visual only, no delay) ----

    static readonly string[] CockedIds = { "axe", "primitive_axe", "hammer", "stone_pick_axe", "iron_pick_axe" };

    [Test]
    public void OnlyTheFiveToolsAreHeldCocked()
    {
        foreach (string id in CockedIds)
            Assert.That(ToolWork.CockKind(id, out _), Is.True, id);
        foreach (string id in new[] { "wood_saw", "shovel", "iron_shovel", "bucket", "knife", "knife" })
            Assert.That(ToolWork.CockKind(id, out _), Is.False, id);
        Assert.That(ToolWork.CockKind(null, out _), Is.False);
        ToolWork.CockKind("stone_pick_axe", out WorkKind mine);
        ToolWork.CockKind("hammer", out WorkKind build);
        ToolWork.CockKind("axe", out WorkKind chop);
        Assert.That(new[] { chop, mine, build }, Is.EqualTo(new[] { WorkKind.Chop, WorkKind.Mine, WorkKind.Build }));
    }

    [Test]
    public void TheCockedPoseIsDrawnBackAndUpAndZeroForTheSawAndTheShovel()
    {
        foreach (WorkKind kind in new[] { WorkKind.Chop, WorkKind.Mine, WorkKind.Build })
        {
            HandPose cocked = SwingRules.Cocked(kind);
            Assert.That(cocked.Pitch, Is.LessThanOrEqualTo(-10f), kind + ": the head is tilted back and up");
            Assert.That(cocked.Z, Is.LessThan(0f), kind + ": the hand is pulled back toward the player");
            Assert.That(cocked.Y, Is.GreaterThan(0f), kind + ": and rides a little higher");
        }
        Assert.That(SwingRules.Cocked(WorkKind.Split), Is.EqualTo(SwingRules.Cocked(WorkKind.Chop)));
        foreach (WorkKind kind in new[] { WorkKind.Saw, WorkKind.Dig })
        {
            Assert.That(SwingRules.Cocked(kind), Is.EqualTo(SwingRules.Zero), kind.ToString());
            Assert.That(SwingRules.Loaded(kind), Is.EqualTo(SwingRules.Zero), kind.ToString());
        }
    }

    [Test]
    public void TheLoadedPoseIsFartherBackThanCockedAndTheAxeAndPickAreFartherThanTheHammer()
    {
        foreach (WorkKind kind in new[] { WorkKind.Chop, WorkKind.Mine, WorkKind.Build })
        {
            HandPose cocked = SwingRules.Cocked(kind), loaded = SwingRules.Loaded(kind);
            Assert.That(loaded.Pitch, Is.LessThan(cocked.Pitch), kind.ToString());
            Assert.That(loaded.Z, Is.LessThan(cocked.Z), kind.ToString());
        }
        Assert.That(SwingRules.Loaded(WorkKind.Chop).Pitch, Is.LessThan(SwingRules.Loaded(WorkKind.Build).Pitch));
        Assert.That(SwingRules.Loaded(WorkKind.Mine).Pitch, Is.LessThan(SwingRules.Loaded(WorkKind.Build).Pitch));
        Assert.That(SwingRules.Loaded(WorkKind.Chop).Z, Is.LessThan(SwingRules.Loaded(WorkKind.Build).Z));
        Assert.That(SwingRules.Loaded(WorkKind.Mine).Z, Is.LessThan(SwingRules.Loaded(WorkKind.Build).Z));
    }

    [Test]
    public void TheCycleStillEndsAtZeroPointNineAndTheContactTimingIsUnchanged()
    {
        Assert.That(SwingRules.SwingSeconds, Is.EqualTo(0.9f));
        Assert.That(SwingRules.ContactSeconds, Is.EqualTo(0.15f), "the strike still lands 0.15 s after the click");
        foreach (WorkKind kind in new[] { WorkKind.Chop, WorkKind.Mine, WorkKind.Build })
        {
            // from cocked (a blow after a pause) and from loaded (a held click), the strike is reached at exactly ContactSeconds
            foreach (HandPose from in new[] { SwingRules.Cocked(kind), SwingRules.Loaded(kind) })
            {
                HandPose atContact = SwingRules.Offset(kind, SwingRules.ContactSeconds, false, from, SwingRules.Loaded(kind));
                Assert.That(SwingRules.Distance(atContact, SwingRules.Strike(kind)), Is.EqualTo(0f).Within(1e-5f), kind.ToString());
                Assert.That(SwingRules.Distance(SwingRules.Offset(kind, SwingRules.ContactSeconds * 0.5f, false, from, SwingRules.Loaded(kind)), SwingRules.Strike(kind)), Is.GreaterThan(0.01f), "not there yet halfway");
            }
            Assert.That(SwingRules.Distance(SwingRules.Offset(kind, SwingRules.SwingSeconds - 0.0001f, false), SwingRules.Loaded(kind)), Is.LessThan(0.001f), kind.ToString());
        }
    }

    [Test]
    public void AHeldClickChainsFromLoadedWithAPullBackBeforeEveryBlow()
    {
        var state = new SwingState();
        state.Configure(true, WorkKind.Chop);
        Assert.That(SwingRules.Distance(state.Offset, SwingRules.Cocked(WorkKind.Chop)), Is.EqualTo(0f), "ready is cocked");

        state.Start(WorkKind.Chop, false);
        Assert.That(SwingRules.Distance(state.Offset, SwingRules.Cocked(WorkKind.Chop)), Is.EqualTo(0f), "the first blow starts from cocked, with no movement at the click");
        for (int i = 0; i < 92; i++)
            state.Tick(0.01f);
        Assert.That(state.Active, Is.False, "the cycle is over at 0.9 s");
        Assert.That(SwingRules.Distance(state.Offset, SwingRules.Loaded(WorkKind.Chop)), Is.LessThan(0.02f), "and the hand is loaded, farther back than cocked");

        state.Start(WorkKind.Chop, false); // the next blow of a held click
        Assert.That(SwingRules.Distance(state.Offset, SwingRules.Loaded(WorkKind.Chop)), Is.LessThan(0.02f), "the next strike starts from the loaded pose");
        Assert.That(SwingRules.Distance(state.Offset, SwingRules.Cocked(WorkKind.Chop)), Is.GreaterThan(0.1f), "not from cocked");
    }

    [Test]
    public void WithNoNewBlowTheHandsSettleFromLoadedToCockedAndStayThere()
    {
        Assert.That(SwingRules.SettleSeconds, Is.EqualTo(0.25f));
        var state = new SwingState();
        state.Configure(true, WorkKind.Mine);
        state.Start(WorkKind.Mine, false); // one click, released
        state.Tick(SwingRules.SwingSeconds + 0.001f);
        Assert.That(state.Active, Is.False);
        HandPose loaded = SwingRules.Loaded(WorkKind.Mine), cocked = SwingRules.Cocked(WorkKind.Mine);
        Assert.That(SwingRules.Distance(state.Offset, loaded), Is.LessThan(0.01f), "it starts settling from loaded");
        state.Tick(SwingRules.SettleSeconds * 0.5f);
        HandPose mid = state.Offset;
        Assert.That(SwingRules.Distance(mid, loaded), Is.GreaterThan(0.005f));
        Assert.That(SwingRules.Distance(mid, cocked), Is.GreaterThan(0.005f), "partway between");
        state.Tick(SwingRules.SettleSeconds);
        Assert.That(SwingRules.Distance(state.Offset, cocked), Is.EqualTo(0f).Within(1e-5f), "settled at cocked");
        state.Tick(5f);
        Assert.That(SwingRules.Distance(state.Offset, cocked), Is.EqualTo(0f).Within(1e-5f), "and never stuck loaded");
    }

    [Test]
    public void ABlowDuringTheSettleStartsFromWhereTheHandIs()
    {
        var state = new SwingState();
        state.Configure(true, WorkKind.Chop);
        state.Start(WorkKind.Chop, false);
        state.Tick(SwingRules.SwingSeconds + 0.001f);
        state.Tick(SwingRules.SettleSeconds * 0.4f);
        HandPose here = state.Offset;
        state.Start(WorkKind.Chop, false);
        Assert.That(SwingRules.Distance(state.Offset, here), Is.EqualTo(0f).Within(1e-5f), "no jump at the click");
    }

    [Test]
    public void ATiredDroopIsAHalfRaiseFromTheCockedPoseWithNoStrike()
    {
        var state = new SwingState();
        state.Configure(true, WorkKind.Chop);
        HandPose cocked = SwingRules.Cocked(WorkKind.Chop);
        state.StartTired();
        Assert.That(SwingRules.Distance(state.Offset, cocked), Is.EqualTo(0f), "starts at cocked");
        state.Tick(SwingRules.TiredSeconds * 0.3f);
        Assert.That(state.Offset.Pitch, Is.LessThan(cocked.Pitch), "rises: the tool tilts further up");
        state.Tick(SwingRules.TiredSeconds * 0.35f);
        Assert.That(state.Offset.Pitch, Is.GreaterThan(cocked.Pitch), "then droops below cocked");
        float most = float.MinValue;
        state.StartTired();
        for (int i = 0; i < 60; i++)
        {
            state.Tick(0.01f);
            most = Mathf.Max(most, state.Offset.Pitch);
        }
        Assert.That(most, Is.LessThan(SwingRules.Strike(WorkKind.Chop).Pitch * 0.5f), "nothing like a strike");
        state.Tick(1f);
        Assert.That(state.Active, Is.False);
        Assert.That(SwingRules.Distance(state.Offset, cocked), Is.EqualTo(0f).Within(1e-5f), "and ends at cocked");
    }

    [Test]
    public void TheFinishingBlowFromCockedIsStillBigger()
    {
        foreach (WorkKind kind in new[] { WorkKind.Chop, WorkKind.Mine, WorkKind.Build })
        {
            var normal = new SwingState();
            normal.Configure(true, kind);
            normal.Start(kind, false);
            normal.Seek(SwingRules.ContactSeconds);
            var finishing = new SwingState();
            finishing.Configure(true, kind);
            finishing.Start(kind, true);
            finishing.Seek(SwingRules.ContactSeconds);
            Assert.That(SwingRules.Distance(finishing.Offset, SwingRules.Cocked(kind)), Is.GreaterThan(SwingRules.Distance(normal.Offset, SwingRules.Cocked(kind))), kind.ToString());
        }
    }

    [Test]
    public void TheSawAndTheShovelsKeepTheirNeutralReadyAndTheirOwnStrokes()
    {
        foreach (WorkKind kind in new[] { WorkKind.Saw, WorkKind.Dig })
        {
            var state = new SwingState();
            state.Configure(false, kind);
            Assert.That(SwingRules.Distance(state.Offset, SwingRules.Zero), Is.EqualTo(0f), "neutral ready");
            state.Start(kind, false);
            state.Seek(SwingRules.ContactSeconds);
            Assert.That(SwingRules.Distance(state.Offset, SwingRules.Strike(kind)), Is.EqualTo(0f).Within(1e-5f));
            state.Seek(SwingRules.SwingSeconds);
            Assert.That(SwingRules.Distance(state.Offset, SwingRules.Zero), Is.EqualTo(0f), "back to neutral at the end of the cycle");
        }
        Assert.That(SwingRules.Lift(WorkKind.Saw).Z, Is.LessThan(0f), "the saw still pulls back");
    }

    [Test]
    public void TheCockedToolsAreHeldDifferentlyFromTheNeutralOnesInHand()
    {
        Load(path => null);
        H hands = NewHands();
        Quaternion Rotation(string id)
        {
            hands.Apply(id);
            return hands.RightHand.localRotation;
        }
        Quaternion shovel = Rotation("shovel"), saw = Rotation("wood_saw");
        Quaternion axe = Rotation("axe");
        float expected = Mathf.Abs(SwingRules.ChopCockPitch) * SwingRules.HandPitchShare;
        Assert.That(Quaternion.Angle(shovel, axe), Is.GreaterThan(expected - 3f).And.LessThan(expected + 6f), "the axe is cocked relative to the neutral hold the shovel has");
        Assert.That(Quaternion.Angle(shovel, saw), Is.LessThan(0.5f), "the saw and the shovel share the neutral hold");
        Assert.That(Quaternion.Angle(shovel, Rotation("no_such_tool")), Is.LessThan(0.5f), "so does the generic placeholder (the Bucket is carried with the hand turned: the carry pose)");
        foreach (string id in CockedIds)
            Assert.That(Quaternion.Angle(shovel, Rotation(id)), Is.GreaterThan(3f), id + " is held at a visibly different angle from the neutral hold");
    }

    [Test]
    public void SwitchingToolsMidSwingDropsTheSwingAndTakesTheNewToolsReadyPose()
    {
        Load(path => null);
        H hands = NewHands();
        hands.Apply("axe");
        hands.Raise.Snap("axe");
        hands.OnSwung("axe", WorkKind.Chop, false);
        hands.Swing.Seek(0.3f);
        Assert.That(hands.Swing.Active, Is.True);
        hands.Apply("shovel");
        Assert.That(hands.Swing.Active, Is.False, "the swing is gone with the tool");
        Assert.That(hands.Swing.Cocked, Is.False, "and the shovel is not cocked");
        hands.Apply("hammer");
        Assert.That(hands.Swing.Cocked, Is.True);
        hands.Apply(null);
        Assert.That(hands.Swing.Cocked, Is.False, "empty hands are not cocked");
    }

    [Test]
    public void TheCockedToolHeadStaysOutOfTheMiddleOfTheScreen()
    {
        Load(path => null);
        H hands = NewHands();
        Camera cam = hands.RightHand.GetComponentInParent<Camera>();
        foreach (string id in CockedIds)
        {
            hands.Apply(id);
            Transform tip = hands.HeldRoot.Find("Anchor_Tip");
            Vector3 view = cam.transform.InverseTransformPoint(tip.position);
            float horizontal = Mathf.Atan2(view.x, view.z) * Mathf.Rad2Deg;
            float vertical = Mathf.Atan2(view.y, view.z) * Mathf.Rad2Deg;
            Assert.That(view.z, Is.GreaterThan(0.05f), id + " in front of the camera");
            bool nearCrosshair = Mathf.Abs(horizontal) < 4f && Mathf.Abs(vertical) < 4f;
            Assert.That(nearCrosshair, Is.False, id + " the working end is not on the crosshair (" + horizontal.ToString("F1") + ", " + vertical.ToString("F1") + " degrees)");
            Assert.That(Mathf.Abs(vertical), Is.LessThan(30f), id + " and inside the 60 degree view");
        }
    }
}
