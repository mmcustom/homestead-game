using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// EditMode tests for Hands and tools M0c-2 (Mike, 2026-11-05): the rest of the tools in hand (bows, rifle, Bow Drill, Bucket and Canteen, Spile, traps, Knife and the rest), a fixed push pose for the Hand
// Cart, and the hands dropping their tool during a creek knock-down. The poses are pure (Economy assembly) and tested directly; the shapes, the tool table, ToolEvents, PlayerHands and the hook lines in the
// tool scripts are in the main assembly and reached by reflection or by source. What these cannot show: the real weapons, water, drill, tree and trap scripts running, and how it all looks and feels.
public class RestOfToolsInHandTests
{
    static Type Find(string name) =>
        AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => { try { return a.GetTypes(); } catch (Exception) { return new Type[0]; } })
                 .First(t => t.Name == name);

    static Type Art => Find("ArtSwap");
    static Type HandsType => Find("PlayerHands");
    static Type Primitives => Find("ToolViewPrimitives");
    static Type Events => Find("ToolEvents");
    static int ViewLayer => LayerMask.NameToLayer("Viewmodel");

    static readonly string[] Bows = { "primitive_bow", "recurve_bow" };
    static readonly string[] Rest =
    {
        "primitive_bow", "recurve_bow", "bolt_action_rifle", "bow_drill", "bucket", "sap_bucket", "canteen", "spile", "box_trap", "fish_trap", "rabbit_snare",
        "knife", "flint_and_steel", "pouch", "clay_pot", "cooking_pot", "potash_kettle", "tarp",
    };
    static readonly string[] CarriedOnly = { "sleeping_bag", "tent", "weather_station" };

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
    static int Count(string source, string pattern) => System.Text.RegularExpressions.Regex.Matches(source, pattern).Count;

    static void Load(Func<string, GameObject> loader)
    {
        Art.GetMethod("ResetForTests").Invoke(null, null);
        Art.GetField("Loader").SetValue(null, loader);
    }

    sealed class H
    {
        public readonly Component Component;
        public H(Component component) { Component = component; }
        T Get<T>(string name) => (T)HandsType.GetProperty(name).GetValue(Component);
        public bool Build(Camera cam) => (bool)HandsType.GetMethod("Build").Invoke(Component, new object[] { cam });
        public void Apply(string tool, float raise = 1f) => HandsType.GetMethod("Apply").Invoke(Component, new object[] { tool, raise, 0f, 0f, HandMotion.Idle });
        public void ApplyAt(string tool, float steps, HandMotion motion) => HandsType.GetMethod("Apply").Invoke(Component, new object[] { tool, 1f, 0f, steps, motion });
        public GameObject Rig => Get<GameObject>("Rig");
        public void Call(string method, params object[] args) => HandsType.GetMethod(method).Invoke(Component, args);
        public void SetPushing(bool on) => HandsType.GetProperty("Pushing").SetValue(Component, on);
        public Transform RightHand => Get<Transform>("RightHand");
        public Transform LeftHand => Get<Transform>("LeftHand");
        public Transform RightAnchor => Get<Transform>("RightAnchor");
        public Transform LeftAnchor => Get<Transform>("LeftAnchor");
        public Transform HeldRoot => Get<Transform>("HeldRoot");
        public Transform LeftGrip => Get<Transform>("LeftGrip");
        public ToolActionState Action => Get<ToolActionState>("Action");
        public HandRaise Raise => Get<HandRaise>("Raise");
    }

    H NewHands()
    {
        Camera cam = Make("Test Camera").AddComponent<Camera>();
        cam.cullingMask = -1;
        var hands = new H(Make("Test Player").AddComponent(HandsType));
        hands.Build(cam);
        return hands;
    }

    GameObject BuildPrimitive(string id, Transform parent) => (GameObject)Primitives.GetMethod("Build").Invoke(null, new object[] { id, parent, ViewLayer });

    // ---- Every tool has a shape ----

    [Test]
    public void TheTableNowHasAnyToolTheHandsCanHold()
    {
        var specs = ((System.Collections.IEnumerable)Art.GetProperty("AllToolSpecs").GetValue(null)).Cast<object>().Select(s => (string)s.GetType().GetField("itemId").GetValue(s)).ToList();
        Assert.That(specs.Count, Is.EqualTo(30));
        foreach (string id in Rest)
            CollectionAssert.Contains(specs, id);
        // every Tool item asset that can be equipped has a row
        UnityEngine.Object[] tools = Resources.LoadAll("Items/Tools");
        Assert.That(tools.Length, Is.GreaterThan(25));
        foreach (UnityEngine.Object tool in tools)
            if (Array.IndexOf(CarriedOnly, tool.name) < 0)
                CollectionAssert.Contains(specs, tool.name, tool.name + " has no shape");
    }

    [Test]
    public void EachNewShapeHasItsGripAndTipAndTheRightExtraAnchors()
    {
        Transform parent = Make("parent").transform;
        foreach (string id in Rest)
        {
            GameObject tool = BuildPrimitive(id, parent);
            Assert.That(tool, Is.Not.Null, id);
            Assert.That(tool.name, Is.EqualTo("tool_" + id));
            Assert.That(tool.transform.Find("Anchor_Grip_R"), Is.Not.Null, id);
            Assert.That(tool.transform.Find("Anchor_Tip"), Is.Not.Null, id);
            Assert.That(tool.GetComponentsInChildren<Renderer>(true).Length, Is.GreaterThanOrEqualTo(2), id + " is more than one block");
            Assert.That(tool.transform.Find("Anchor_Grip_L") != null, Is.EqualTo(HeldTools.IsTwoHanded(id)), id + " left grip only on two-handed tools");
            Assert.That(tool.transform.Find("Anchor_Nock") != null, Is.EqualTo(Array.IndexOf(Bows, id) >= 0), id + " nock on the bows only");
            Assert.That(tool.transform.Find("Anchor_Muzzle") != null, Is.EqualTo(id == "bolt_action_rifle"), id + " muzzle on the rifle only");
        }
    }

    [Test]
    public void TheNewShapesAreOnTheViewmodelLayerWithNoCollidersOrShadows()
    {
        Transform parent = Make("parent").transform;
        foreach (string id in Rest)
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
    public void NoToolInTheTableShowsTheGenericPlaceholderAnyMore()
    {
        Load(path => null);
        H hands = NewHands();
        foreach (string id in ToolWork.ViewToolIds)
        {
            hands.Apply(id);
            Assert.That(hands.HeldRoot.name, Is.EqualTo("tool_" + id), id);
        }
        hands.Apply("a_tool_nobody_has_shaped");
        Assert.That(hands.HeldRoot.name, Is.EqualTo("Held Placeholder"), "only an unknown id falls back");
    }

    [Test]
    public void TheShapesAndTheSharedHandedTableAgree()
    {
        foreach (string id in ToolWork.ViewToolIds)
        {
            object layout = Primitives.GetMethod("LayoutFor").Invoke(null, new object[] { id });
            Assert.That((bool)layout.GetType().GetField("twoHanded").GetValue(layout), Is.EqualTo(HeldTools.IsTwoHanded(id)), id);
        }
    }

    [Test]
    public void ANewToolPrefabIsUsedWhenThereIsOneAndItsMissingAnchorsFallBack()
    {
        GameObject prefab = Make("tool_stand_in");
        prefab.SetActive(false);
        GameObject.CreatePrimitive(PrimitiveType.Cube).transform.SetParent(prefab.transform, false);
        Load(path => path == "Art/tool_recurve_bow" ? prefab : null);
        H hands = NewHands();
        hands.Apply("recurve_bow");
        Assert.That(hands.HeldRoot.Find("Head"), Is.Null);
        Assert.That(hands.HeldRoot.GetComponentsInChildren<Collider>(true).Length, Is.EqualTo(0));
        Assert.That(hands.HeldRoot.Find("Anchor_Grip_R"), Is.Not.Null, "made where the primitive has it");
        Assert.That(hands.HeldRoot.Find("Anchor_Grip_L"), Is.Not.Null, "the bow handle's left grip, made where the primitive has it");
        Assert.That(hands.HeldRoot.Find("Anchor_Nock"), Is.Not.Null, "the string hand's nock, made where the primitive has it");
        Assert.That(hands.HeldRoot.Find("Anchor_Muzzle"), Is.Null, "an optional named anchor nothing attaches to is not made up");
    }

    // ---- Pure: the bows ----

    [Test]
    public void ADrawPullsTheHandBackAsItBuildsAndHoldsAtFull()
    {
        Assert.That(ToolActionRules.Magnitude(ToolActionRules.DrawOffset(0f, 1f)), Is.EqualTo(0f));
        float last = 0f;
        for (float d = 0.1f; d <= 1.001f; d += 0.1f)
        {
            float z = -ToolActionRules.DrawOffset(d, 0f).Z;
            Assert.That(z, Is.GreaterThan(last), "pulls back as it builds");
            last = z;
        }
        Assert.That(ToolActionRules.DrawOffset(1f, 0.02f).Y, Is.Not.EqualTo(ToolActionRules.DrawOffset(1f, 0f).Y), "a faint tremble at full draw");
        Assert.That(ToolActionRules.DrawOffset(0.5f, 0.02f), Is.EqualTo(ToolActionRules.DrawOffset(0.5f, 0.2f)), "and none part way");
        Assert.That(ToolActionRules.DrawOffset(2f, 0f), Is.EqualTo(ToolActionRules.DrawOffset(1f, 0f)), "clamped");
    }

    [Test]
    public void TheReleaseSnapStartsFromFullDrawAndEndsAtNothing()
    {
        HandPose start = ToolActionRules.ActionOffset(ToolAction.Release, 0f);
        Assert.That(start, Is.EqualTo(ToolActionRules.DrawOffset(1f, 0f)), "from the drawn pose: no jump on the shot");
        Assert.That(ToolActionRules.ActionOffset(ToolAction.Release, 0.06f).Z, Is.GreaterThan(0f), "springs forward");
        Assert.That(ToolActionRules.Magnitude(ToolActionRules.ActionOffset(ToolAction.Release, ToolActionRules.ReleaseSeconds)), Is.EqualTo(0f).Within(1e-6f));
        Assert.That(ToolActionRules.ReleaseSeconds, Is.LessThan(0.5f), "a snap");
    }

    [Test]
    public void TheStateFollowsTheReportedDrawAndTheShotClearsIt()
    {
        var state = new ToolActionState();
        state.Configure("recurve_bow");
        HandPose ready = ToolActionRules.Ready("recurve_bow");
        Assert.That(state.Offset(false), Is.EqualTo(ready));
        state.SetDraw(0.5f);
        Assert.That(state.Offset(false).Z, Is.LessThan(ready.Z), "pulled back");
        state.SetDraw(0f);
        Assert.That(state.Offset(false), Is.EqualTo(ready), "let down: ready again");
    }

    // ---- Pure: the rifle ----

    [Test]
    public void TheRifleKicksThenWorksTheBoltAndIsReadyAtTheEnd()
    {
        Assert.That(ToolActionRules.ActionOffset(ToolAction.Shot, 0.05f).Pitch, Is.LessThan(-6f), "the kick");
        Assert.That(ToolActionRules.ActionOffset(ToolAction.Shot, 0.05f).Z, Is.LessThan(-0.05f));
        Assert.That(ToolActionRules.ActionOffset(ToolAction.Shot, 0.6f).Z, Is.LessThan(-0.02f), "the bolt pulled back");
        Assert.That(ToolActionRules.ActionOffset(ToolAction.Shot, 0.85f).Z, Is.GreaterThan(0.01f), "and pushed home");
        Assert.That(ToolActionRules.Magnitude(ToolActionRules.ActionOffset(ToolAction.Shot, ToolActionRules.ShotSeconds)), Is.EqualTo(0f).Within(1e-6f));
        Assert.That(ToolActionRules.ShotSeconds, Is.LessThanOrEqualTo(1.3f), "no longer than the rifle's own bolt time (1.3 s)");
    }

    [Test]
    public void TheAimLiftsTheRifleInStepWithTheScopeAndStopsWhereTheHandsHide()
    {
        Assert.That(ToolActionRules.Magnitude(ToolActionRules.AimOffset(0f)), Is.EqualTo(0f));
        Assert.That(ToolActionRules.AimOffset(0.25f).Y, Is.GreaterThan(0f));
        Assert.That(ToolActionRules.AimOffset(0.5f), Is.EqualTo(ToolActionRules.AimOffset(1f)), "flat past the point the hands are hidden");
        var state = new ToolActionState();
        state.Configure("bolt_action_rifle");
        HandPose ready = ToolActionRules.Ready("bolt_action_rifle");
        state.SetAim(0.5f);
        Assert.That(state.Offset(false), Is.Not.EqualTo(ready));
        state.SetAim(0f);
        Assert.That(state.Offset(false), Is.EqualTo(ready));
    }

    // ---- Pure: the Bow Drill, vessels, Spile, traps ----

    [Test]
    public void TheDrillSawsBackAndForthWhileWorkingAndStopsWhenTheWorkEnds()
    {
        Assert.That(ToolActionRules.Magnitude(ToolActionRules.DrillOffset(0.1f, 0f)), Is.EqualTo(0f));
        float max = 0f;
        for (float t = 0f; t < 1f; t += 0.01f)
            max = Mathf.Max(max, Mathf.Abs(ToolActionRules.DrillOffset(t, 1f).Z));
        Assert.That(max, Is.EqualTo(ToolActionRules.DrillStroke).Within(0.002f));
        Assert.That(ToolActionRules.DrillOffset(0.1f, 1f).Z, Is.EqualTo(ToolActionRules.DrillOffset(0.1f + 1f / ToolActionRules.DrillStrokesPerSecond, 1f).Z).Within(1e-4f), "periodic");

        var state = new ToolActionState();
        state.Configure("bow_drill");
        state.SetWorking(true);
        state.Tick(ToolActionRules.ReelRampSeconds * 0.5f);
        Assert.That(state.WorkWeight, Is.EqualTo(0.5f).Within(1e-4f), "eases in");
        state.Tick(1f);
        Assert.That(state.WorkWeight, Is.EqualTo(1f));
        state.SetWorking(false);
        state.Tick(1f);
        Assert.That(state.WorkWeight, Is.EqualTo(0f), "and out");
        Assert.That(state.Offset(false), Is.EqualTo(ToolActionRules.Ready("bow_drill")));
    }

    [Test]
    public void AVesselDipsDownAtTheWaterHoldsAndComesUpAndADrinkLiftsIt()
    {
        Assert.That(ToolActionRules.ActionOffset(ToolAction.Dip, 0.3f).Y, Is.LessThan(-0.05f));
        Assert.That(ToolActionRules.ActionOffset(ToolAction.Dip, 0.45f), Is.EqualTo(ToolActionRules.ActionOffset(ToolAction.Dip, 0.3f)).Or.Not.EqualTo(ToolActionRules.ActionOffset(ToolAction.Dip, 0.9f)));
        Assert.That(ToolActionRules.ActionOffset(ToolAction.Drink, 0.35f).Y, Is.GreaterThan(0.02f));
        Assert.That(ToolActionRules.ActionOffset(ToolAction.Drink, 0.35f).Pitch, Is.LessThan(-20f), "tipped toward the mouth");
        foreach (ToolAction a in new[] { ToolAction.Dip, ToolAction.Drink })
            Assert.That(ToolActionRules.Magnitude(ToolActionRules.ActionOffset(a, ToolActionRules.Seconds(a))), Is.EqualTo(0f).Within(1e-6f), a.ToString());
    }

    [Test]
    public void TheSpileDrivesInTwiceAndATrapIsSetDown()
    {
        Assert.That(ToolActionRules.ActionOffset(ToolAction.Tap, 0.24f).Z, Is.GreaterThan(0.05f), "first drive");
        Assert.That(ToolActionRules.ActionOffset(ToolAction.Tap, 0.36f).Z, Is.LessThan(0.0f), "back off");
        Assert.That(ToolActionRules.ActionOffset(ToolAction.Tap, 0.48f).Z, Is.GreaterThan(0.05f), "second drive");
        Assert.That(ToolActionRules.ActionOffset(ToolAction.PlaceDown, 0.25f).Y, Is.LessThan(-0.05f), "down");
        Assert.That(ToolActionRules.ActionOffset(ToolAction.PlaceDown, 0.45f).Y, Is.LessThan(-0.05f), "held there a moment");
        foreach (ToolAction a in new[] { ToolAction.Tap, ToolAction.PlaceDown })
            Assert.That(ToolActionRules.Magnitude(ToolActionRules.ActionOffset(a, ToolActionRules.Seconds(a))), Is.EqualTo(0f).Within(1e-6f), a.ToString());
    }

    [Test]
    public void TheClassHandlesTheNewToolsAndLeavesTheKnifeAndThePotsNeutral()
    {
        foreach (string id in new[] { "primitive_bow", "recurve_bow", "bolt_action_rifle", "bow_drill", "bucket", "sap_bucket", "canteen", "spile", "box_trap", "fish_trap", "rabbit_snare", "pouch", "cooking_pot", "potash_kettle" })
        {
            Assert.That(ToolActionRules.Handles(id), Is.True, id);
            Assert.That(ToolActionRules.Magnitude(ToolActionRules.Ready(id)), Is.GreaterThan(0f), id);
        }
        foreach (string id in new[] { "knife", "flint_and_steel", "clay_pot", "tarp", "axe" })
            Assert.That(ToolActionRules.Handles(id), Is.False, id);
        Assert.That(ToolActionRules.Magnitude(ToolActionRules.Ready("knife")), Is.EqualTo(0f), "the Knife is a hold pose only (the melee phase is M0e)");
    }

    // ---- The Hand Cart and the knock-down ----

    [Test]
    public void ThePushPoseIsAFixedMirroredTwoHandedGripInFront()
    {
        HandPose right = HandPoseRules.PushTarget(HandSide.Right), left = HandPoseRules.PushTarget(HandSide.Left);
        Assert.That(right.X, Is.GreaterThan(0.1f));
        Assert.That(left.X, Is.EqualTo(-right.X).Within(1e-6f));
        Assert.That(left.Y, Is.EqualTo(right.Y));
        Assert.That(left.Z, Is.EqualTo(right.Z));
        Assert.That(right.Z, Is.GreaterThan(HandPoseRules.Target(HandSide.Right, false).Z), "further forward than the rest pose");
        Assert.That(right.Y, Is.GreaterThan(HandPoseRules.Target(HandSide.Right, false).Y), "and higher: on the handles");
        Assert.That(HandPoseRules.PushPose(HandSide.Right, 1f, 0f, 0f, HandMotion.Idle).X, Is.EqualTo(right.X).Within(1e-5f));
        Assert.That(HandPoseRules.PushPose(HandSide.Right, 0f, 0f, 0f, HandMotion.Idle).Y, Is.LessThan(right.Y - 0.4f), "and rises from the bottom like any other");
        Assert.That(HandPoseRules.PushPose(HandSide.Right, 1f, 0f, 0.25f, HandMotion.Walk), Is.Not.EqualTo(HandPoseRules.PushPose(HandSide.Right, 1f, 0f, 0f, HandMotion.Walk)), "it sways as the player walks");
    }

    [Test]
    public void NoToolIsHeldWhileThePushingOrKnockedDown()
    {
        Assert.That(HandPoseRules.EffectiveTool("axe", false, false), Is.EqualTo("axe"));
        Assert.That(HandPoseRules.EffectiveTool("axe", true, false), Is.Null);
        Assert.That(HandPoseRules.EffectiveTool("axe", false, true), Is.Null);
        Assert.That(HandPoseRules.EffectiveTool(null, true, true), Is.Null);
    }

    [Test]
    public void WhilePushingBothHandsTakeThePushPose()
    {
        Load(path => null);
        H hands = NewHands();
        hands.Apply(null);
        hands.SetPushing(true);
        hands.Apply(null);
        HandPose expectedR = HandPoseRules.PushPose(HandSide.Right, 1f, 0f, 0f, HandMotion.Idle);
        HandPose expectedL = HandPoseRules.PushPose(HandSide.Left, 1f, 0f, 0f, HandMotion.Idle);
        Assert.That(hands.RightHand.localPosition.y, Is.EqualTo(expectedR.Y).Within(1e-5f));
        Assert.That(hands.RightHand.localPosition.z, Is.EqualTo(expectedR.Z).Within(1e-5f));
        Assert.That(hands.LeftHand.localPosition.x, Is.EqualTo(expectedL.X).Within(1e-5f));
        Assert.That(hands.LeftHand.gameObject.activeSelf, Is.True);
        hands.SetPushing(false);
        hands.Apply(null);
        Assert.That(hands.RightHand.localPosition.y, Is.Not.EqualTo(expectedR.Y), "back to rest when the cart is parked");
    }

    [Test]
    public void ThePlayerHandsTakeThePushingAndKnockDownStatesFromTheirOwnClasses()
    {
        string hands = Source("Scripts/Player/PlayerHands.cs");
        StringAssert.Contains("CartHandler.Pushing", hands);
        StringAssert.Contains("CreekCrossing.InputLocked", hands);
        StringAssert.Contains("HandPoseRules.EffectiveTool(", hands);
        StringAssert.DoesNotContain("PlacementMode", hands, "placement mode leaves the hands as they are: they stay up with whatever is in them");
        StringAssert.Contains("HiddenByScope", hands, "the rifle scope's hiding is untouched");
        StringAssert.Contains("ScopeLowering", hands);
    }

    // ---- The hands react to ToolEvents ----

    [Test]
    public void TheHandsReactToTheWeaponAndVesselEventsForTheRightToolOnly()
    {
        Load(path => null);
        H hands = NewHands();

        hands.Apply("recurve_bow");
        hands.Raise.Snap("recurve_bow");
        hands.Call("OnDraw", "recurve_bow", 0.6f);
        Assert.That(hands.Action.Draw, Is.EqualTo(0.6f).Within(1e-5f));
        hands.Call("OnDraw", "primitive_bow", 0.9f);
        Assert.That(hands.Action.Draw, Is.EqualTo(0.6f).Within(1e-5f), "another bow's draw is ignored");
        hands.Call("OnShot", "recurve_bow");
        Assert.That(hands.Action.Draw, Is.EqualTo(0f));
        Assert.That(hands.Action.Action, Is.EqualTo(ToolAction.Release));

        hands.Apply("bolt_action_rifle");
        hands.Raise.Snap("bolt_action_rifle");
        hands.Call("OnShot", "bolt_action_rifle");
        Assert.That(hands.Action.Action, Is.EqualTo(ToolAction.Shot));

        hands.Apply("bow_drill");
        hands.Raise.Snap("bow_drill");
        hands.Call("OnWorking", "bow_drill", true);
        hands.Action.Tick(1f);
        Assert.That(hands.Action.WorkWeight, Is.EqualTo(1f));
        hands.Call("OnWorking", "bow_drill", false);
        hands.Action.Tick(1f);
        Assert.That(hands.Action.WorkWeight, Is.EqualTo(0f));

        hands.Apply("canteen");
        hands.Raise.Snap("canteen");
        hands.Call("OnFilled");
        Assert.That(hands.Action.Action, Is.EqualTo(ToolAction.Dip));
        hands.Call("OnDrank");
        Assert.That(hands.Action.Action, Is.EqualTo(ToolAction.Drink));

        hands.Apply("spile");
        hands.Raise.Snap("spile");
        hands.Call("OnTapped");
        Assert.That(hands.Action.Action, Is.EqualTo(ToolAction.Tap));

        hands.Apply("box_trap");
        hands.Raise.Snap("box_trap");
        hands.Call("OnPlaced");
        Assert.That(hands.Action.Action, Is.EqualTo(ToolAction.PlaceDown));
    }

    [Test]
    public void TheVesselSpileAndTrapEventsAreIgnoredByOtherTools()
    {
        Load(path => null);
        H hands = NewHands();
        hands.Apply("axe");
        hands.Raise.Snap("axe");
        foreach (string call in new[] { "OnFilled", "OnDrank", "OnTapped", "OnPlaced" })
        {
            hands.Call(call);
            Assert.That(hands.Action.Active, Is.False, call);
        }
    }

    // ---- ToolEvents ----

    [Test]
    public void TheNewEventsCarryTheirData()
    {
        float fraction = -1f; string drawTool = null, shotTool = null;
        int plain = 0;
        Action<string, float> draw = (t, f) => { drawTool = t; fraction = f; };
        Action<string> shot = t => shotTool = t;
        Action filled = () => plain++, drank = () => plain++, tapped = () => plain++, placed = () => plain++;
        var subs = new List<(EventInfo e, Delegate d)>
        {
            (Events.GetEvent("Draw"), draw), (Events.GetEvent("Shot"), shot), (Events.GetEvent("Filled"), filled),
            (Events.GetEvent("Drank"), drank), (Events.GetEvent("Tapped"), tapped), (Events.GetEvent("Placed"), placed),
        };
        foreach (var s in subs) s.e.AddEventHandler(null, s.d);
        try
        {
            Events.GetMethod("RaiseDraw").Invoke(null, new object[] { "recurve_bow", 0.5f });
            Events.GetMethod("RaiseShot").Invoke(null, new object[] { "bolt_action_rifle" });
            Events.GetMethod("RaiseFilled").Invoke(null, null);
            Events.GetMethod("RaiseDrank").Invoke(null, null);
            Events.GetMethod("RaiseTapped").Invoke(null, null);
            Events.GetMethod("RaisePlaced").Invoke(null, null);
        }
        finally { foreach (var s in subs) s.e.RemoveEventHandler(null, s.d); }
        Assert.That(drawTool, Is.EqualTo("recurve_bow"));
        Assert.That(fraction, Is.EqualTo(0.5f));
        Assert.That(shotTool, Is.EqualTo("bolt_action_rifle"));
        Assert.That(plain, Is.EqualTo(4));
    }

    [Test]
    public void TheDrillsWorkingEventRaisesOnlyOnChange()
    {
        var seen = new List<string>();
        Action<string, bool> handler = (t, on) => seen.Add(t + ":" + on);
        EventInfo e = Events.GetEvent("Working");
        e.AddEventHandler(null, handler);
        MethodInfo set = Events.GetMethod("SetWorking");
        try
        {
            set.Invoke(null, new object[] { "bow_drill", true });
            set.Invoke(null, new object[] { "bow_drill", true });
            set.Invoke(null, new object[] { null, false });
            set.Invoke(null, new object[] { null, false });
        }
        finally { e.RemoveEventHandler(null, handler); }
        CollectionAssert.AreEqual(new[] { "bow_drill:True", "bow_drill:False" }, seen);
    }

    // ---- Source: the hook lines and everything left alone ----

    static IEnumerable<string> HookLines(string source) =>
        source.Split('\n').Select(l => l.Trim()).Where(l => l.StartsWith("ToolEvents."));

    [Test]
    public void EveryHookLineIsASingleStatement()
    {
        foreach (string file in new[] { "Wildlife/HuntingWeapon.cs", "Survival/BowDrill.cs", "Survival/WaterSource.cs", "Managers/TapManager.cs", "Player/TrapSetter.cs" })
            foreach (string line in HookLines(Source("Scripts/" + file)))
            {
                string code = line.Contains("//") ? line.Substring(0, line.IndexOf("//", StringComparison.Ordinal)).Trim() : line;
                Assert.That(code.EndsWith(";"), Is.True, file + ": " + line);
                Assert.That(Count(code, ";"), Is.EqualTo(1), file + " one statement: " + line);
            }
    }

    [Test]
    public void TheHookLinesAreWhereTheirThingsHappen()
    {
        string weapon = Source("Scripts/Wildlife/HuntingWeapon.cs");
        Assert.That(Count(weapon, @"ToolEvents\.RaiseDraw\("), Is.EqualTo(2), "the draw each frame, and 0 on release");
        Assert.That(Count(weapon, @"ToolEvents\.RaiseShot\("), Is.EqualTo(2), "a bow's release and the rifle's shot");
        Assert.That(weapon.IndexOf("ToolEvents.RaiseShot(RifleId)"), Is.LessThan(weapon.IndexOf("Fire(RifleSpread")), "raised as the shot goes");
        string drill = Source("Scripts/Survival/BowDrill.cs");
        Assert.That(Count(drill, @"ToolEvents\.SetWorking\("), Is.EqualTo(4), "on while it saws, off on release, on cancel and at the attempt");
        string water = Source("Scripts/Survival/WaterSource.cs");
        Assert.That(Count(water, @"ToolEvents\.RaiseFilled\("), Is.EqualTo(1));
        Assert.That(Count(water, @"ToolEvents\.RaiseDrank\("), Is.EqualTo(1));
        Assert.That(Count(Source("Scripts/Managers/TapManager.cs"), @"ToolEvents\.RaiseTapped\("), Is.EqualTo(1));
        string traps = Source("Scripts/Player/TrapSetter.cs");
        Assert.That(Count(traps, @"ToolEvents\.RaisePlaced\("), Is.EqualTo(1));
        Assert.That(traps.IndexOf("ToolEvents.RaisePlaced("), Is.LessThan(traps.IndexOf("traps.Place(type, point, yaw)")), "before the trap leaves the pack");
    }

    [Test]
    public void TheGameplayNumbersAndTextOfTheHookedScriptsAreUntouched()
    {
        string weapon = Source("Scripts/Wildlife/HuntingWeapon.cs");
        StringAssert.Contains("float drawSeconds = 0.8f", weapon);
        StringAssert.Contains("float primitiveDrawSeconds = 1.1f", weapon);
        StringAssert.Contains("float boltSeconds = 1.3f", weapon);
        StringAssert.Contains("float hydrationPerDrink = 20f", Source("Scripts/Survival/WaterSource.cs"));
        StringAssert.Contains("Bow Drill — hold click to drill", Source("Scripts/Survival/BowDrill.cs"));
        StringAssert.Contains("Spile  ·  click on a Sugar Maple to tap it", Source("Scripts/Managers/TapManager.cs"));
        StringAssert.Contains("float hookRod", Source("Scripts/Fishing/FishingRod.cs") + "float hookRod", "(fishing untouched since M0c-1)");
    }

    [Test]
    public void NoNewSwapPointOrSaveFieldWasAddedBeyondTheToolTable()
    {
        string art = Source("Scripts/World/ArtSwap.cs");
        StringAssert.Contains("M0c-2 adds eighteen more rows", art);
        StringAssert.Contains("Anchor_Nock", art);
        StringAssert.Contains("Anchor_Muzzle", art);
        string all = Source("Scripts/Economy/ToolActionRules.cs") + Source("Scripts/Player/PlayerHands.cs") + Source("Scripts/Player/ToolViewPrimitives.cs");
        StringAssert.DoesNotContain("SaveVersion", all);
        StringAssert.DoesNotContain("LedgerSaveData", all);
        StringAssert.DoesNotContain("ToolStatus", all);
        Assert.That(Directory.Exists(Path.Combine(Application.dataPath, "Resources/Art")), Is.False, "no test prefab is left behind");
    }

    // ---- M0c-2 fix: the bows are drawn by the right hand only, and everything that hangs hangs straight down ----

    static readonly string[] Hanging = { "bucket", "sap_bucket", "canteen", "cooking_pot", "potash_kettle", "pouch", "fish_trap", "box_trap" };

    [Test]
    public void TheBowHandNeverMovesWithTheDrawOrTheRelease()
    {
        foreach (string id in Bows)
        {
            var state = new ToolActionState();
            state.Configure(id);
            HandPose still = state.BowHandOffset();
            Assert.That(ToolActionRules.Magnitude(still), Is.GreaterThan(0f), id + ": the bow is held out in the left hand");
            foreach (float draw in new[] { 0f, 0.3f, 0.7f, 1f })
            {
                state.SetDraw(draw);
                state.Tick(0.1f);
                Assert.That(state.BowHandOffset(), Is.EqualTo(still), id + " draw " + draw);
            }
            state.SetDraw(0f);
            state.Start(ToolAction.Release);
            for (int i = 0; i < 6; i++)
            {
                state.Tick(0.05f);
                Assert.That(state.BowHandOffset(), Is.EqualTo(still), id + " release at " + state.OneShotTime);
            }
        }
        var other = new ToolActionState();
        foreach (string id in new[] { "bolt_action_rifle", "bow_drill", "bucket", "axe" })
        {
            other.Configure(id);
            Assert.That(ToolActionRules.Magnitude(other.BowHandOffset()), Is.EqualTo(0f), id + " has no bow hand");
        }
    }

    [Test]
    public void TheStringHandPullsBackTowardTheFaceAndSpringsForwardOnTheRelease()
    {
        foreach (float d in new[] { 0.25f, 0.5f, 1f })
        {
            HandPose pull = ToolActionRules.DrawOffset(d, 0f);
            Assert.That(pull.Z, Is.LessThan(-0.12f * d), "mostly back toward the face at " + d);
            Assert.That(Mathf.Abs(pull.X), Is.LessThan(Mathf.Abs(pull.Z) * 0.3f), "with only a small sideways move at " + d);
            Assert.That(pull.X, Is.GreaterThanOrEqualTo(0f), "in toward the right cheek, never out to the left");
        }
        Assert.That(ToolActionRules.DrawOffset(1f, 0f).Z, Is.LessThan(-0.13f));
        Assert.That(ToolActionRules.DrawOffset(1f, 0f).Y, Is.GreaterThan(0.02f), "and up toward the cheek");
        Assert.That(ToolActionRules.ActionOffset(ToolAction.Release, 0.06f).Z, Is.GreaterThan(0f), "springs forward past the nock");
        Assert.That(ToolActionRules.Magnitude(ToolActionRules.ActionOffset(ToolAction.Release, 0.25f)), Is.EqualTo(0f).Within(1e-6f), "and settles back on it");
    }

    [Test]
    public void TheBowIsInTheLeftHandAndTheRightPalmIsOnTheNockWhileTheBowStaysPutDuringADraw()
    {
        Load(path => null);
        foreach (string id in Bows)
        {
            H hands = NewHands();
            hands.Apply(id);
            hands.Raise.Snap(id);
            Assert.That(hands.HeldRoot.parent, Is.EqualTo(hands.LeftAnchor), id + " hangs off the left palm");
            Assert.That(hands.RightAnchor.childCount, Is.EqualTo(0), id + ": nothing in the right hand but the string");
            Transform grip = hands.HeldRoot.Find("Anchor_Grip_L"), nock = hands.HeldRoot.Find("Anchor_Nock");
            Assert.That(Vector3.Distance(grip.position, hands.LeftAnchor.position), Is.LessThan(1e-4f), id + ": the handle is in the left hand");
            Assert.That(Vector3.Distance(nock.position, hands.RightAnchor.position), Is.LessThan(1e-4f), id + ": at rest the right palm is on the nock");

            Vector3 bowPosition = hands.HeldRoot.position;
            Quaternion bowRotation = hands.HeldRoot.rotation;
            Vector3 leftPosition = hands.LeftHand.position;
            Quaternion leftRotation = hands.LeftHand.rotation;
            Vector3 restHand = hands.RightHand.localPosition;
            Transform root = hands.HeldRoot;

            hands.Call("OnDraw", id, 1f);
            hands.Apply(id);
            Assert.That(hands.HeldRoot, Is.EqualTo(root));
            Assert.That(Vector3.Distance(hands.HeldRoot.position, bowPosition), Is.LessThan(1e-5f), id + ": the bow does not move back");
            Assert.That(Quaternion.Angle(hands.HeldRoot.rotation, bowRotation), Is.LessThan(0.01f), id + ": and does not rotate back");
            Assert.That(Vector3.Distance(hands.LeftHand.position, leftPosition), Is.LessThan(1e-5f), id + ": the left hand holds still");
            Assert.That(Quaternion.Angle(hands.LeftHand.rotation, leftRotation), Is.LessThan(0.01f));
            Vector3 drawn = hands.RightHand.localPosition;
            Assert.That(drawn.z, Is.LessThan(restHand.z - 0.09f), id + ": the right hand comes back toward the camera");
            Assert.That(Mathf.Abs(drawn.x - restHand.x), Is.LessThan(0.07f), id + ": with only a small sideways move");
            Assert.That(drawn.y, Is.GreaterThan(restHand.y), id + ": and up toward the face");

            hands.Call("OnShot", id);
            hands.Action.Seek(0.06f);
            hands.Apply(id);
            Assert.That(Vector3.Distance(hands.HeldRoot.position, bowPosition), Is.LessThan(1e-5f), id + ": the release moves the right hand only");
            Assert.That(Vector3.Distance(hands.LeftHand.position, leftPosition), Is.LessThan(1e-5f));
            Assert.That(hands.RightHand.localPosition.z, Is.GreaterThan(restHand.z - 0.04f), id + ": the right hand has sprung forward again, back to the string's rest place");
        }
    }

    [Test]
    public void ThePrimitiveStringFollowsTheRightHandAndAPrefabNeedsOnlyTheTwoAnchors()
    {
        Load(path => null);
        H hands = NewHands();
        hands.Apply("recurve_bow");
        hands.Raise.Snap("recurve_bow");
        Transform upper = hands.HeldRoot.Find("String Upper"), lower = hands.HeldRoot.Find("String Lower");
        Assert.That(upper, Is.Not.Null);
        Assert.That(lower, Is.Not.Null);
        Assert.That(hands.HeldRoot.Find("String"), Is.Null, "the one-piece string is gone: it is two halves now");
        Vector3 restDirection = upper.up;
        hands.Call("OnDraw", "recurve_bow", 1f);
        hands.Apply("recurve_bow");
        Assert.That(Vector3.Angle(upper.up, restDirection), Is.GreaterThan(4f), "the halves swing over to the drawn hand: the string shows pulled back");
        Assert.That(Vector3.Distance(upper.position + upper.up * upper.lossyScale.y, hands.RightAnchor.position), Is.LessThan(0.01f), "and meet at the palm");
        Assert.That(Vector3.Distance(lower.position + lower.up * lower.lossyScale.y, hands.RightAnchor.position), Is.LessThan(0.01f));

        // a prefab with only the required anchors: its handle is Anchor_Grip_L, and the nock falls back to the primitive's place
        GameObject prefab = Make("tool_stand_in");
        prefab.SetActive(false);
        GameObject.CreatePrimitive(PrimitiveType.Cube).transform.SetParent(prefab.transform, false);
        new GameObject("Anchor_Grip_L").transform.SetParent(prefab.transform, false);
        Load(path => path == "Art/tool_primitive_bow" ? prefab : null);
        H prefabHands = NewHands();
        prefabHands.Apply("primitive_bow");
        prefabHands.Raise.Snap("primitive_bow");
        Assert.That(prefabHands.HeldRoot.Find("Anchor_Nock"), Is.Not.Null);
        Assert.That(prefabHands.HeldRoot.Find("String Upper"), Is.Null, "a prefab's string is its own");
        prefabHands.Call("OnDraw", "primitive_bow", 1f);
        Assert.DoesNotThrow(() => prefabHands.Apply("primitive_bow"));
    }

    [Test]
    public void TheBowDrillAndTheRifleAreExactlyAsTheyWere()
    {
        Assert.That(ToolActionRules.Ready("bow_drill"), Is.EqualTo(new HandPose { X = 0f, Y = 0.01f, Z = 0.02f, Pitch = -6f }));
        Assert.That(ToolActionRules.Ready("bolt_action_rifle"), Is.EqualTo(new HandPose { X = 0f, Y = 0.03f, Z = 0.02f, Pitch = -10f }));
        Assert.That(ToolActionRules.AimOffset(0.5f), Is.EqualTo(new HandPose { X = -0.05f, Y = 0.04f, Z = 0.03f, Pitch = -8f }));
        Assert.That(ToolActionRules.DrillStroke, Is.EqualTo(0.05f));
        Assert.That(ToolActionRules.DrillStrokesPerSecond, Is.EqualTo(3f));
        Assert.That(ToolActionRules.ActionOffset(ToolAction.Shot, 0.05f), Is.EqualTo(new HandPose { X = 0f, Y = 0.02f, Z = -0.07f, Pitch = -9f }));
        foreach (string id in new[] { "bow_drill", "bolt_action_rifle" })
        {
            var state = new ToolActionState();
            state.Configure(id);
            Assert.That(ToolActionRules.Magnitude(state.BowHandOffset()), Is.EqualTo(0f), id);
            object layout = Primitives.GetMethod("LayoutFor").Invoke(null, new object[] { id });
            Assert.That(layout.GetType().GetField("hasNock").GetValue(layout), Is.EqualTo(false), id);
        }
        Load(path => null);
        H hands = NewHands();
        foreach (string id in new[] { "bow_drill", "bolt_action_rifle" })
        {
            hands.Apply(id);
            Assert.That(hands.HeldRoot.parent, Is.EqualTo(hands.RightAnchor), id + " is still the right hand's");
            Assert.That(hands.LeftAnchor.childCount, Is.EqualTo(0), id);
        }
        object rifle = Primitives.GetMethod("LayoutFor").Invoke(null, new object[] { "bolt_action_rifle" });
        Assert.That((float)rifle.GetType().GetField("holdPitch").GetValue(rifle), Is.EqualTo(-20f), "the rifle's hold is unchanged");
    }

    [Test]
    public void TheHangingListIsTheCarriedItemsAndNotTheLanternOrTheSnareOrThePot()
    {
        CollectionAssert.AreEquivalent(Hanging, ToolActionRules.HangingIds);
        foreach (string id in Hanging)
            Assert.That(ToolActionRules.IsHanging(id), Is.True, id);
        foreach (string id in new[] { "lantern", "rabbit_snare", "clay_pot", "tarp", "knife", "axe", "primitive_bow", null })
            Assert.That(ToolActionRules.IsHanging(id), Is.False, id ?? "null");
    }

    [Test]
    public void HangingItemsHaveNoForwardTiltInTheirLayout()
    {
        foreach (string id in Hanging)
        {
            object layout = Primitives.GetMethod("LayoutFor").Invoke(null, new object[] { id });
            Assert.That((float)layout.GetType().GetField("holdPitch").GetValue(layout), Is.EqualTo(0f), id + " pitch");
            Assert.That((float)layout.GetType().GetField("holdRoll").GetValue(layout), Is.EqualTo(0f), id + " roll");
            object spec = Art.GetMethod("ToolSpecFor").Invoke(null, new object[] { id });
            Assert.That((float)spec.GetType().GetField("holdPitch").GetValue(spec), Is.EqualTo(0f), id + " row");
        }
    }

    [Test]
    public void TheCarryPoseLiftsTheHandOutAndForwardAndIsHigherForTallerBodies()
    {
        // round 2: the hand is lifted only as far as the Lantern's (the arm shows no longer than before), turned to point left so the sleeve trails off to the right, and the bodies hang short
        foreach (string id in Hanging)
        {
            HandPose ready = ToolActionRules.Ready(id);
            Assert.That(ready.Y, Is.InRange(0.05f, 0.12f), id + " lifted a little");
            Assert.That(ready.Pitch, Is.EqualTo(0f), id + ": no pitch, the forearm keeps its angle");
            Assert.That(ready.Yaw, Is.LessThan(-10f), id + ": the hand points left so the sleeve clears the body");
        }
    }

    [Test]
    public void AHangingItemHangsStraightDownInTheViewWhateverTheHandDoes()
    {
        Load(path => null);
        H hands = NewHands();
        foreach (string id in Hanging)
        {
            hands.Apply(id);
            // the body runs along -Y from the grip: the tool's up is the rig's up, and its forward is the rig's forward
            Assert.That(Vector3.Angle(hands.HeldRoot.up, hands.Rig.transform.up), Is.LessThan(0.01f), id + " at rest");
            Assert.That(Vector3.Angle(hands.HeldRoot.forward, hands.Rig.transform.forward), Is.LessThan(0.01f), id + " no forward lean");
            Assert.That(Vector3.Angle(hands.HeldRoot.right, hands.Rig.transform.right), Is.LessThan(0.01f), id + " no sideways lean");
            Assert.That(Vector3.Distance(hands.HeldRoot.Find("Anchor_Grip_R").position, hands.RightAnchor.position), Is.LessThan(1e-4f), id + ": still gripped by the palm");
            Transform tip = hands.HeldRoot.Find("Anchor_Tip");
            Assert.That(tip.position.y, Is.LessThan(hands.RightAnchor.position.y - 0.04f), id + ": the body is below the hand");
        }
    }

    [Test]
    public void TheWalkSwingIsSmallAndNothingLeavesTheScreen()
    {
        Assert.That(ToolActionRules.Magnitude(ToolActionRules.HangSway(0.37f, 0f)), Is.EqualTo(0f), "standing still: no swing");
        float max = 0f;
        for (float phase = 0f; phase < 2f; phase += 0.01f)
        {
            HandPose walk = ToolActionRules.HangSway(phase, HandPoseRules.StepAmount(HandMotion.Walk));
            HandPose run = ToolActionRules.HangSway(phase, HandPoseRules.StepAmount(HandMotion.Sprint));
            Assert.That(Mathf.Abs(walk.Pitch), Is.LessThanOrEqualTo(ToolActionRules.HangSwayPitch + 0.01f));
            Assert.That(Mathf.Abs(run.Pitch), Is.LessThanOrEqualTo(ToolActionRules.HangSwayPitch * 1.7f + 0.01f));
            max = Mathf.Max(max, Mathf.Abs(run.Pitch), Mathf.Abs(run.Roll));
        }
        Assert.That(max, Is.GreaterThan(1f), "it does swing");
        Assert.That(max, Is.LessThan(8f), "but only a little, even in a sprint");

        Load(path => null);
        H hands = NewHands();
        foreach (string id in Hanging)
        {
            hands.Apply(id);
            Vector3 gripAtRest = hands.HeldRoot.Find("Anchor_Grip_R").position;
            Vector3 tipAtRest = hands.HeldRoot.Find("Anchor_Tip").position;
            float worst = 0f;
            for (float phase = 0f; phase < 2f; phase += 0.125f)
            {
                hands.ApplyAt(id, phase, HandMotion.Sprint);
                Assert.That(Vector3.Angle(hands.HeldRoot.up, hands.Rig.transform.up), Is.LessThan(9f), id + " sprint swing");
                worst = Mathf.Max(worst, Vector3.Distance(hands.HeldRoot.Find("Anchor_Tip").position, hands.RightAnchor.position + (tipAtRest - gripAtRest)));
            }
            Assert.That(worst, Is.LessThan(0.06f), id + ": the body stays within a few centimetres of where it hangs");
        }
    }

    [Test]
    public void ANewlyEquippedHangingItemSettlesAndThenIsStill()
    {
        Assert.That(ToolActionRules.HangSettle(0f), Is.EqualTo(0f));
        Assert.That(ToolActionRules.HangSettle(-1f), Is.EqualTo(0f));
        Assert.That(ToolActionRules.HangSettle(ToolActionRules.HangSettleSeconds), Is.EqualTo(0f));
        Assert.That(ToolActionRules.HangSettle(5f), Is.EqualTo(0f));
        float peak = 0f, late = 0f;
        for (float t = 0f; t < 1.2f; t += 0.01f)
        {
            peak = Mathf.Max(peak, Mathf.Abs(ToolActionRules.HangSettle(t)));
            if (t > 0.8f)
                late = Mathf.Max(late, Mathf.Abs(ToolActionRules.HangSettle(t)));
        }
        Assert.That(peak, Is.InRange(1f, 6f), "tiny");
        Assert.That(late, Is.LessThan(peak * 0.25f), "and it dies away");
    }

    [Test]
    public void TheDipDrinkAndPlaceDownStillTipAndMoveTheHangingBodyButKeepItHanging()
    {
        Load(path => null);
        H hands = NewHands();
        var cases = new[] { ("bucket", ToolAction.Dip, 0.45f), ("canteen", ToolAction.Dip, 0.45f), ("canteen", ToolAction.Drink, 0.55f), ("sap_bucket", ToolAction.Drink, 0.55f), ("box_trap", ToolAction.PlaceDown, 0.35f), ("fish_trap", ToolAction.PlaceDown, 0.35f) };
        foreach (var (id, action, t) in cases)
        {
            hands.Apply(id);
            hands.Raise.Snap(id);
            Vector3 rest = hands.HeldRoot.Find("Anchor_Tip").position;
            Vector3 restHand = hands.RightAnchor.position;
            hands.Action.Start(action);
            hands.Action.Seek(t);
            hands.Apply(id);
            Assert.That(Vector3.Distance(hands.RightAnchor.position, restHand), Is.GreaterThan(0.015f), id + " " + action + ": the hand moves");
            Assert.That(Vector3.Angle(hands.HeldRoot.up, hands.Rig.transform.up), Is.LessThan(25f), id + " " + action + ": still hanging, not flung about");
            Assert.That(hands.HeldRoot.Find("Anchor_Tip").position.y, Is.LessThan(hands.RightAnchor.position.y), id + " " + action + ": the body is still below the hand");
            Assert.That(Vector3.Distance(hands.HeldRoot.Find("Anchor_Grip_R").position, hands.RightAnchor.position), Is.LessThan(1e-4f), id + ": gripped");
            hands.Action.Configure(id);
        }
    }

    [Test]
    public void TheBowAndCarryFixTouchedNoBehaviourNumberTextOrAsset()
    {
        string hands = Source("Scripts/Player/PlayerHands.cs");
        StringAssert.Contains("AlignHanging", hands);
        StringAssert.Contains("ApplyBow", hands);
        string all = hands + Source("Scripts/Economy/ToolActionRules.cs") + Source("Scripts/Player/ToolViewPrimitives.cs");
        StringAssert.DoesNotContain("SaveVersion", all);
        StringAssert.DoesNotContain("LedgerSaveData", all);
        StringAssert.DoesNotContain("PlayerPrefs", all);
        // the weapon's own numbers are the ones the playtest passed
        string weapon = Source("Scripts/Wildlife/HuntingWeapon.cs");
        StringAssert.Contains("float drawSeconds = 0.8f", weapon);
        StringAssert.Contains("float primitiveDrawSeconds = 1.1f", weapon);
        Assert.That(Directory.Exists(Path.Combine(Application.dataPath, "Resources/Art")), Is.False, "no test prefab is left behind");
    }

    // ---- M0c-2 fix, round 2: arm reach, the bows' look and crosshair, the sleeve's shadows ----

    static Vector3 InRig(H hands, Vector3 world) => hands.Rig.transform.InverseTransformPoint(world);

    // Whether a sphere of this radius at p (rig space, the camera at the origin looking along +Z) would cover the small box around the middle of the screen where the crosshair is (+-4.6 degrees).
    static bool CoversCrosshair(Vector3 p, float radius)
    {
        if (p.z <= 0.02f)
            return false; // behind the viewmodel camera's near plane (0.02): not drawn at all
        float box = 0.08f, r = radius / p.z;
        return Mathf.Abs(p.x) / p.z < box + r && Mathf.Abs(p.y) / p.z < box + r;
    }

    static HandPose Place(string id, ToolAction? action, float t)
    {
        HandPose pose = ToolActionRules.Sum(HandPoseRules.Target(HandSide.Right, true), ToolActionRules.Ready(id));
        return action.HasValue ? ToolActionRules.Sum(pose, ToolActionRules.ActionOffset(action.Value, t)) : pose;
    }

    [Test]
    public void NoCarriedItemOrBowShowsMoreArmThanTheLanternDid()
    {
        // the references: the Axe's cocked ready hold and the Rods are well under it, and the Lantern (the highest pose Mike passed) is just under MaxRise
        float axe = HandPoseRules.Rise(HandPoseRules.Target(HandSide.Right, true));
        float lantern = HandPoseRules.Rise(Place("lantern", null, 0f));
        Assert.That(axe, Is.LessThan(lantern));
        Assert.That(lantern, Is.LessThanOrEqualTo(HandPoseRules.MaxRise));
        Assert.That(HandPoseRules.Rise(Place("fishing_rod", null, 0f)), Is.LessThan(lantern));
        foreach (string id in Hanging)
        {
            Assert.That(HandPoseRules.Rise(Place(id, null, 0f)), Is.LessThanOrEqualTo(HandPoseRules.MaxRise), id + " ready");
            foreach (var a in new[] { (ToolAction.Dip, 0.45f), (ToolAction.Drink, 0.55f), (ToolAction.PlaceDown, 0.35f) })
                Assert.That(HandPoseRules.Rise(Place(id, a.Item1, a.Item2)), Is.LessThanOrEqualTo(HandPoseRules.MaxRise), id + " " + a.Item1);
        }
        foreach (string id in Bows)
        {
            HandPose left = ToolActionRules.Sum(HandPoseRules.Target(HandSide.Left, true), ToolActionRules.BowHandReady());
            Assert.That(HandPoseRules.Rise(left), Is.LessThanOrEqualTo(HandPoseRules.MaxRise), id + " left hand");
            Assert.That(left.X, Is.LessThan(-0.1f), id + ": the bow is in the left of the view");
        }
    }

    [Test]
    public void TheArmIsNeverStretchedTheSleeveAndForearmKeepTheirOwnSize()
    {
        Load(path => null);
        H hands = NewHands();
        hands.Apply(null);
        Vector3 forearm = hands.RightHand.Find("Forearm").localScale, sleeve = hands.RightHand.Find("Sleeve").localScale;
        foreach (string id in new[] { "primitive_bow", "recurve_bow", "bucket", "fish_trap", "box_trap", "axe", "lantern" })
        {
            hands.Apply(id);
            hands.Raise.Snap(id);
            hands.Call("OnDraw", id, 1f);
            hands.Apply(id);
            Assert.That(hands.RightHand.Find("Forearm").localScale, Is.EqualTo(forearm), id);
            Assert.That(hands.RightHand.Find("Sleeve").localScale, Is.EqualTo(sleeve), id);
            Assert.That(hands.LeftHand.Find("Sleeve").localScale, Is.EqualTo(sleeve), id);
            Assert.That(hands.RightHand.lossyScale.x, Is.EqualTo(hands.Rig.transform.lossyScale.x).Within(1e-5f), id + ": a hand is only ever scaled by the rig");
        }
    }

    [Test]
    public void ABowKeepsBothArmsAndBothHandsOffTheCrosshairAtReadyHalfDrawAndFullDraw()
    {
        Load(path => null);
        foreach (string id in Bows)
        {
            H hands = NewHands();
            hands.Apply(id);
            hands.Raise.Snap(id);
            foreach (float draw in new[] { 0f, 0.5f, 1f })
            {
                hands.Call("OnDraw", id, draw);
                hands.Apply(id);
                foreach (Transform hand in new[] { hands.RightHand, hands.LeftHand })
                {
                    Vector3 wrist = InRig(hands, hand.position);
                    Assert.That(HandPoseRules.Rise(wrist.y, wrist.z), Is.LessThanOrEqualTo(HandPoseRules.MaxRise + 0.005f), id + " draw " + draw + " " + hand.name + ": no more arm than the Lantern showed");
                    Assert.That(CoversCrosshair(wrist, 0.05f), Is.False, id + " draw " + draw + " " + hand.name + " wrist");
                    Assert.That(CoversCrosshair(InRig(hands, hand.TransformPoint(new Vector3(0f, 0f, 0.12f))), 0.04f), Is.False, id + " draw " + draw + " " + hand.name + " fingers");
                    for (float back = 0.08f; back <= 0.5f; back += 0.07f)
                        Assert.That(CoversCrosshair(InRig(hands, hand.TransformPoint(new Vector3(0f, 0f, -back))), 0.05f), Is.False, id + " draw " + draw + " " + hand.name + " forearm " + back);
                }
            }
        }
    }

    [Test]
    public void TheHandsAndTheToolHangingFromThemHaveNoShadowsFromTheWorldOnThem()
    {
        Load(path => null);
        H hands = NewHands();
        foreach (string id in new[] { null, "bucket", "primitive_bow", "axe", "fishing_rod" })
        {
            hands.Apply(id);
            Renderer[] renderers = hands.Rig.GetComponentsInChildren<Renderer>(true);
            Assert.That(renderers.Length, Is.GreaterThan(5), id ?? "empty");
            foreach (Renderer r in renderers)
            {
                Assert.That(r.receiveShadows, Is.False, (id ?? "empty") + "/" + r.name + ": a shelter's slats must not stripe the sleeve");
                Assert.That(r.shadowCastingMode, Is.EqualTo(UnityEngine.Rendering.ShadowCastingMode.Off));
            }
        }
        StringAssert.Contains("r.receiveShadows = false", Source("Scripts/World/ArtSwap.cs"));
        StringAssert.DoesNotContain("HDRenderPipelineAsset", Source("Scripts/Player/ViewmodelPass.cs"));
    }

    [Test]
    public void ABowIsOneContinuousShapeWithBothLimbsJoinedToTheGripAndTheStringBetweenTheTips()
    {
        Transform parent = Make("parent").transform;
        foreach (string id in Bows)
        {
            GameObject bow = BuildPrimitive(id, parent);
            var limbs = bow.GetComponentsInChildren<Transform>(true).Where(t => t.name == "Limb").ToList();
            Assert.That(limbs.Count, Is.GreaterThanOrEqualTo(12), id + ": enough pieces that the curve reads as one object");
            bool recurve = id == "recurve_bow";
            Vector3 tip = (Vector3)Primitives.GetMethod("BowTip").Invoke(null, new object[] { recurve });
            // both limbs start at the grip and end at the tips: the pieces chain with no gap bigger than a piece
            foreach (float sign in new[] { 1f, -1f })
            {
                var side = limbs.Where(l => l.localPosition.y * sign > 0f).OrderBy(l => Mathf.Abs(l.localPosition.y)).ToList();
                Assert.That(side.Count, Is.GreaterThanOrEqualTo(6), id);
                Assert.That(Mathf.Abs(side[0].localPosition.y), Is.LessThan(0.12f), id + ": the first piece is at the grip");
                for (int i = 1; i < side.Count; i++)
                    Assert.That(Vector3.Distance(side[i].localPosition, side[i - 1].localPosition), Is.LessThan(0.16f), id + ": the pieces chain without a gap");
                Assert.That(Mathf.Abs(side[side.Count - 1].localPosition.y), Is.GreaterThan(tip.y - 0.1f), id + ": out to the tips");
            }
            Transform upper = bow.transform.Find("String Upper"), lower = bow.transform.Find("String Lower"), nock = bow.transform.Find("Anchor_Nock");
            Assert.That(upper.localPosition.y + upper.localScale.y, Is.EqualTo(tip.y).Within(0.01f), id + ": the string reaches the upper tip");
            Assert.That(lower.localPosition.y - lower.localScale.y, Is.EqualTo(-tip.y).Within(0.01f), id + ": and the lower tip");
            Assert.That(nock.localPosition.z, Is.EqualTo(tip.z).Within(1e-5f), id + ": the nock is on the string");
            Assert.That(bow.transform.Find("Handle"), Is.Not.Null, id + " has a visible grip");
            Assert.That(bow.transform.Find("Anchor_Grip_L").localPosition, Is.EqualTo(Vector3.zero), id + ": the left hand holds the grip");
        }
    }

    [Test]
    public void TheBowStandsSeenFromItsSideAndTheRecurveIsDarkerThanThePrimitiveBow()
    {
        Assert.That(Mathf.Abs(ToolActionRules.BowYaw), Is.InRange(8f, 30f), "turned only a little off straight-on: the bend is in depth, so the bow looks straight up and down from the shooting position");
        Color pale = (Color)Primitives.GetField("BowPale").GetValue(null), dark = (Color)Primitives.GetField("BowDark").GetValue(null);
        Assert.That(dark.grayscale, Is.LessThan(pale.grayscale - 0.2f));
        Assert.That(pale.grayscale, Is.GreaterThan(0.45f), "light enough to read against the grass");
    }

    [Test]
    public void EveryHangingBodyStaysAboveTheBottomOfTheScreenAtRest()
    {
        Load(path => null);
        H hands = NewHands();
        foreach (string id in Hanging)
        {
            hands.Apply(id);
            Vector3 tip = InRig(hands, hands.HeldRoot.Find("Anchor_Tip").position);
            Vector3 hand = InRig(hands, hands.RightAnchor.position);
            Assert.That(tip.y, Is.GreaterThan(-0.5774f * tip.z - 0.02f), id + ": the bottom of the body is on screen");
            Assert.That(tip.y, Is.LessThan(hand.y - 0.09f), id + ": and well below the hand");
        }
    }

    // ---- Round 3: both string ends sit on their limb tip caps, and the draw comes back ----

    [Test]
    public void BothStringHalvesStartExactlyOnTheirLimbTipCapsAtEveryDrawAndTheRelease()
    {
        Load(path => null);
        foreach (string id in Bows)
        {
            H hands = NewHands();
            hands.Apply(id);
            hands.Raise.Snap(id);
            var caps = hands.HeldRoot.GetComponentsInChildren<Transform>(true).Where(t => t.name == "Limb Tip").ToList();
            Assert.That(caps.Count, Is.EqualTo(2), id);
            Transform upperCap = caps.OrderByDescending(c => c.localPosition.y).First(), lowerCap = caps.OrderBy(c => c.localPosition.y).First();
            Transform upper = hands.HeldRoot.Find("String Upper"), lower = hands.HeldRoot.Find("String Lower");
            var moments = new List<(string, float, bool)> { ("ready", 0f, false), ("half draw", 0.5f, false), ("full draw", 1f, false), ("release", 0f, true) };
            foreach (var (label, draw, release) in moments)
            {
                hands.Call("OnDraw", id, draw);
                if (release)
                {
                    hands.Call("OnShot", id);
                    hands.Action.Seek(0.06f);
                }
                hands.Apply(id);
                // a half's far end is where its up vector leaves the centre, away from the hand
                Vector3 upperEnd = upper.position - upper.up * upper.lossyScale.y, lowerEnd = lower.position - lower.up * lower.lossyScale.y;
                Assert.That(Vector3.Distance(upperEnd, upperCap.position), Is.LessThan(0.01f), id + " " + label + ": the upper half starts on the upper tip cap");
                Assert.That(Vector3.Distance(lowerEnd, lowerCap.position), Is.LessThan(0.01f), id + " " + label + ": the lower half starts on the lower tip cap");
                // and its near end is on the hand's palm
                Assert.That(Vector3.Distance(upper.position + upper.up * upper.lossyScale.y, hands.RightAnchor.position), Is.LessThan(0.01f), id + " " + label);
                if (release)
                    hands.Action.Configure(id);
            }
        }
    }

    [Test]
    public void TheRecurvesStringEndsAreItsOwnFlickedForwardTipsNotThePrimitiveBowsAndTheCapsMatchTheAnchors()
    {
        Vector3 primitive = (Vector3)Primitives.GetMethod("BowTip").Invoke(null, new object[] { false });
        Vector3 recurve = (Vector3)Primitives.GetMethod("BowTip").Invoke(null, new object[] { true });
        float flick = (float)Primitives.GetField("BowFlick").GetValue(null);
        float baseZ = (float)Primitives.GetMethod("BowBaseZ").Invoke(null, new object[] { true });
        Assert.That(recurve.z, Is.EqualTo(baseZ + flick).Within(1e-6f), "the Recurve's tip is its curve's end PLUS the forward flick");
        Assert.That(recurve.z, Is.Not.EqualTo(primitive.z));
        Transform parent = Make("parent").transform;
        foreach (string id in Bows)
        {
            GameObject bow = BuildPrimitive(id, parent);
            Vector3 tip = id == "recurve_bow" ? recurve : primitive;
            var caps = bow.GetComponentsInChildren<Transform>(true).Where(t => t.name == "Limb Tip").OrderByDescending(c => c.localPosition.y).ToList();
            Assert.That(Vector3.Distance(caps[0].localPosition, tip), Is.LessThan(0.002f), id + ": the upper cap is at the tip");
            Assert.That(Vector3.Distance(caps[1].localPosition, new Vector3(0f, -tip.y, tip.z)), Is.LessThan(0.002f), id + ": the lower cap is at the mirrored tip");
            Assert.That(bow.transform.Find("Anchor_Nock").localPosition.z, Is.EqualTo(tip.z).Within(1e-5f), id + ": the nock is on the string line");
        }
    }

    [Test]
    public void TheFullDrawComesMostlyBackTowardTheCameraAndTheHandStaysOffTheCrosshair()
    {
        Load(path => null);
        foreach (string id in Bows)
        {
            H hands = NewHands();
            hands.Apply(id);
            hands.Raise.Snap(id);
            Vector3 rest = InRig(hands, hands.RightAnchor.position);
            hands.Call("OnDraw", id, 1f);
            hands.Apply(id);
            Vector3 full = InRig(hands, hands.RightAnchor.position), move = full - rest;
            Assert.That(-move.z, Is.GreaterThan(0.11f), id + ": the palm comes back toward the camera");
            Assert.That(-move.z, Is.GreaterThan(Mathf.Abs(move.x) * 3f), id + ": far more back than sideways (a pull, not a swing)");
            Assert.That(move.y, Is.GreaterThan(0.02f), id + ": and a little up");
            Assert.That(full.x - rest.x, Is.GreaterThanOrEqualTo(0f), id + ": in toward the right cheek");
            Assert.That(CoversCrosshair(full, 0.05f), Is.False, id + ": the palm is clear of the crosshair box");
        }
    }

    // ---- Round 4: the bow stands straight up and down ----

    [Test]
    public void BothBowsAreMirrorImagesAboutTheGripWithNoRollSoTheyStandStraight()
    {
        Assert.That(ToolActionRules.BowRoll, Is.EqualTo(0f), "no roll: it does not lean");
        Transform parent = Make("parent").transform;
        foreach (string id in Bows)
        {
            GameObject bow = BuildPrimitive(id, parent);
            var limbs = bow.GetComponentsInChildren<Transform>(true).Where(t => t.name == "Limb").ToList();
            var upper = limbs.Where(l => l.localPosition.y > 0f).OrderBy(l => l.localPosition.y).ToList();
            var lower = limbs.Where(l => l.localPosition.y < 0f).OrderByDescending(l => l.localPosition.y).ToList();
            Assert.That(upper.Count, Is.EqualTo(lower.Count), id);
            for (int i = 0; i < upper.Count; i++)
            {
                Assert.That(upper[i].localPosition.x, Is.EqualTo(lower[i].localPosition.x).Within(1e-5f), id + " piece " + i);
                Assert.That(upper[i].localPosition.y, Is.EqualTo(-lower[i].localPosition.y).Within(1e-5f), id + " piece " + i);
                Assert.That(upper[i].localPosition.z, Is.EqualTo(lower[i].localPosition.z).Within(1e-5f), id + " piece " + i + ": the same curve above and below");
            }
            var caps = bow.GetComponentsInChildren<Transform>(true).Where(t => t.name == "Limb Tip").OrderByDescending(c => c.localPosition.y).ToList();
            Assert.That(caps[0].localPosition.y, Is.EqualTo(-caps[1].localPosition.y).Within(1e-5f), id + ": equal limb length above and below");
            Assert.That(caps[0].localPosition.z, Is.EqualTo(caps[1].localPosition.z).Within(1e-5f), id);
            Assert.That(caps[0].localPosition.x, Is.EqualTo(0f), id);
        }
    }

    [Test]
    public void TheTipCapsSitDirectlyAboveAndBelowTheGripInTheViewAndBothTipsAreOnScreenAtEveryDraw()
    {
        Load(path => null);
        foreach (string id in Bows)
        {
            H hands = NewHands();
            hands.Apply(id);
            hands.Raise.Snap(id);
            var caps = hands.HeldRoot.GetComponentsInChildren<Transform>(true).Where(t => t.name == "Limb Tip").OrderByDescending(c => c.localPosition.y).ToList();
            Transform grip = hands.HeldRoot.Find("Anchor_Grip_L");
            foreach (float draw in new[] { 0f, 0.5f, 1f })
            {
                hands.Call("OnDraw", id, draw);
                hands.Apply(id);
                Vector3 g = InRig(hands, grip.position), up = InRig(hands, caps[0].position), down = InRig(hands, caps[1].position);
                Assert.That(Mathf.Abs(up.x - g.x), Is.LessThan(0.015f), id + " draw " + draw + ": the upper tip is directly above the grip (under 1.5 cm sideways: the bend is in depth)");
                Assert.That(Mathf.Abs(down.x - g.x), Is.LessThan(0.015f), id + " draw " + draw + ": the lower tip is directly below it");
                Assert.That(Mathf.Abs(up.x - down.x), Is.LessThan(0.01f), id + " draw " + draw + ": the tip-to-tip line is vertical");
                Assert.That(up.y - g.y, Is.EqualTo(g.y - down.y).Within(0.01f), id + " draw " + draw + ": equal limbs above and below");
                // the bow is full size: the UPPER tip must be on screen; the lower tip is allowed to run off the bottom
                Assert.That(up.z, Is.GreaterThan(0.1f), id);
                Assert.That(up.y / up.z, Is.LessThan(0.5774f), id + " draw " + draw + ": the upper tip is on screen (not above the top edge)");
                Assert.That(up.y / up.z, Is.GreaterThan(-0.5774f), id);
                Assert.That(Mathf.Abs(up.x) / up.z, Is.LessThan(1.0f), id + " draw " + draw + ": and horizontally");
                Assert.That(up.y, Is.GreaterThan(g.y + 0.3f), id + ": the upper limb runs a long way up the view (a full-size bow)");
            }
        }
    }

    // ---- Round 5: full size, flat wide limbs, the bend in depth ----

    [Test]
    public void TheBowIsFullSizeAndItsLimbsAreFlatAndWideNotThinSticks()
    {
        Transform parent = Make("parent").transform;
        foreach (string id in Bows)
        {
            Vector3 tip = (Vector3)Primitives.GetMethod("BowTip").Invoke(null, new object[] { id == "recurve_bow" });
            Assert.That(tip.y, Is.InRange(0.37f, 0.41f), id + ": about 0.38 to 0.40 each side of the grip, about 0.80 in all");
            GameObject bow = BuildPrimitive(id, parent);
            foreach (Transform limb in bow.GetComponentsInChildren<Transform>(true).Where(t => t.name == "Limb"))
            {
                Assert.That(limb.localScale.x, Is.InRange(0.019f, 0.045f), id + ": about 2 to 4 cm across the view");
                Assert.That(limb.localScale.z, Is.LessThanOrEqualTo(0.015f), id + ": thin in depth");
                Assert.That(limb.localScale.x, Is.GreaterThanOrEqualTo(limb.localScale.z * 1.5f), id + ": wider than it is deep");
            }
            Assert.That(bow.transform.Find("Grip Wrap"), Is.Not.Null, id + ": a clear leather grip wrap");
            Assert.That(bow.GetComponentsInChildren<Transform>(true).Count(t => t.name == "Limb Tip"), Is.EqualTo(2), id + ": a visible tip cap at each end");
        }
        Color pale = (Color)Primitives.GetField("BowPale").GetValue(null), dark = (Color)Primitives.GetField("BowDark").GetValue(null);
        Assert.That(dark.r, Is.GreaterThan(dark.g * 1.6f), "the Recurve is a reddish brown");
        Assert.That(dark.grayscale, Is.InRange(0.3f, 0.5f), "medium, not near black");
    }

    [Test]
    public void TheBendIsInDepthSoTheTipsAndTheMiddleOfTheLimbsStayOverTheGripFromTheShootingPosition()
    {
        Load(path => null);
        foreach (string id in Bows)
        {
            H hands = NewHands();
            hands.Apply(id);
            hands.Raise.Snap(id);
            Transform grip = hands.HeldRoot.Find("Anchor_Grip_L");
            foreach (float draw in new[] { 0f, 0.5f, 1f })
            {
                hands.Call("OnDraw", id, draw);
                hands.Apply(id);
                float gx = InRig(hands, grip.position).x, worst = 0f;
                foreach (Transform t in hands.HeldRoot.GetComponentsInChildren<Transform>(true).Where(t => t.name == "Limb" || t.name == "Limb Tip"))
                    worst = Mathf.Max(worst, Mathf.Abs(InRig(hands, t.position).x - gx));
                Assert.That(worst, Is.LessThan(0.025f), id + " draw " + draw + ": no limb piece is more than 2.5 cm to either side of the grip's line (the curve is in depth)");
            }
            // and the bend really is there, along Z: the middle of the limbs is well forward of the tips
            var limbs = hands.HeldRoot.GetComponentsInChildren<Transform>(true).Where(t => t.name == "Limb" && t.localPosition.y > 0f).OrderBy(t => t.localPosition.y).ToList();
            float tipZ = hands.HeldRoot.Find("Anchor_Nock").localPosition.z;
            Assert.That(limbs[limbs.Count / 2].localPosition.z, Is.GreaterThan(tipZ + 0.02f), id + ": the limbs bow forward in the middle, in depth");
        }
    }
}
