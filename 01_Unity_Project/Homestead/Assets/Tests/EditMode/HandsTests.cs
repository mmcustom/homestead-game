using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// EditMode tests for Hands and tools M0a (Mike, 2026-11-04): the first-person hands. HandPoseRules, HeldTools and HandRaise are pure (Economy assembly) and tested directly; PlayerHands, the hands swap point
// in ArtSwap and the Viewmodel layer are in the main assembly and reached by reflection, as the A1 tests do. A hands prefab is supplied through ArtSwap.Loader, so no asset is needed in Resources.
// What these cannot show: the custom pass drawing in the real game (that needs a render), the hands' look and lighting, and how it feels. Nothing here touches the Saves folder or a scene.
public class HandsTests
{
    static Type Find(string name) =>
        AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => { try { return a.GetTypes(); } catch (Exception) { return new Type[0]; } })
                 .First(t => t.Name == name);

    static Type Art => Find("ArtSwap");
    static Type HandsType => Find("PlayerHands");

    readonly List<GameObject> made = new List<GameObject>();

    [TearDown]
    public void TearDown()
    {
        foreach (GameObject go in made)
            if (go != null)
                UnityEngine.Object.DestroyImmediate(go);
        made.Clear();
        Art.GetMethod("ResetForTests").Invoke(null, null);
    }

    GameObject Make(string name)
    {
        var go = new GameObject(name);
        made.Add(go);
        return go;
    }

    // A stand-in hands prefab: Hand_R and Hand_L each with a collider and a renderer, plus optional anchors.
    GameObject StandInHands(bool hands, bool anchors, bool anchorsOutsideHands = false)
    {
        GameObject root = Make("hands_stand_in");
        if (hands)
        {
            foreach (string name in new[] { "Hand_R", "Hand_L" })
            {
                GameObject hand = GameObject.CreatePrimitive(PrimitiveType.Cube); // a cube comes with a collider and a renderer
                hand.name = name;
                hand.transform.SetParent(root.transform, false);
                if (anchors && !anchorsOutsideHands)
                {
                    var a = new GameObject(name == "Hand_R" ? "Anchor_Hand_R" : "Anchor_Hand_L");
                    a.transform.SetParent(hand.transform, false);
                    a.transform.localPosition = new Vector3(0f, 0f, 0.5f);
                }
            }
        }
        if (anchors && anchorsOutsideHands)
        {
            foreach (string name in new[] { "Anchor_Hand_R", "Anchor_Hand_L" })
            {
                var a = new GameObject(name);
                a.transform.SetParent(root.transform, false);
                a.transform.localPosition = new Vector3(0.1f, 0.2f, 0.3f);
            }
        }
        root.SetActive(false); // like a prefab asset: the instance is switched on by the code
        return root;
    }

    static void Load(GameObject prefab, Func<string, GameObject> custom = null)
    {
        Art.GetMethod("ResetForTests").Invoke(null, null);
        Art.GetField("Loader").SetValue(null, custom ?? (Func<string, GameObject>)(path => path == "Art/hands_player" ? prefab : null));
    }

    static int ViewLayer => LayerMask.NameToLayer("Viewmodel");

    static Transform Attach(Transform parent) => (Transform)Art.GetMethod("TryAttachHands").Invoke(null, new object[] { parent, ViewLayer });

    // ---- HandPoseRules: the poses ----

    [Test]
    public void RestRightHandIsLowAndOutToTheRight()
    {
        HandPose p = HandPoseRules.Pose(HandSide.Right, false, 1f, 0f, 0f, HandMotion.Idle);
        Assert.That(p.X, Is.GreaterThan(0.15f));
        Assert.That(p.Y, Is.LessThan(-0.15f));
        Assert.That(p.Z, Is.GreaterThan(0.3f));
    }

    [Test]
    public void RestLeftHandMirrorsTheRight()
    {
        HandPose right = HandPoseRules.Target(HandSide.Right, false);
        HandPose left = HandPoseRules.Target(HandSide.Left, false);
        Assert.That(left.X, Is.EqualTo(-right.X).Within(1e-6f));
        Assert.That(left.Y, Is.EqualTo(right.Y).Within(1e-6f));
        Assert.That(left.Z, Is.EqualTo(right.Z).Within(1e-6f));
        Assert.That(left.Yaw, Is.EqualTo(-right.Yaw).Within(1e-6f));
    }

    [Test]
    public void PosesAreDeterministic()
    {
        HandPose a = HandPoseRules.Pose(HandSide.Right, true, 0.6f, 1.7f, 3.3f, HandMotion.Walk);
        HandPose b = HandPoseRules.Pose(HandSide.Right, true, 0.6f, 1.7f, 3.3f, HandMotion.Walk);
        Assert.That(a, Is.EqualTo(b));
    }

    [Test]
    public void BreathingMovesTheHandSlightlyAndNeverStops()
    {
        float min = float.MaxValue, max = float.MinValue;
        for (float t = 0f; t < 4.5f; t += 0.1f)
        {
            float y = HandPoseRules.Pose(HandSide.Right, false, 1f, t, 0f, HandMotion.Idle).Y;
            min = Math.Min(min, y);
            max = Math.Max(max, y);
        }
        Assert.That(max - min, Is.GreaterThan(0.004f));
        Assert.That(max - min, Is.LessThan(0.012f), "a slight sway, not a bob");
    }

    [Test]
    public void WalkingSwaysTheHandsButStandingStillDoesNot()
    {
        // At the same breath time, the step phase changes the pose only while walking.
        HandPose still0 = HandPoseRules.Pose(HandSide.Right, false, 1f, 0f, 0f, HandMotion.Idle);
        HandPose still1 = HandPoseRules.Pose(HandSide.Right, false, 1f, 0f, 0.25f, HandMotion.Idle);
        HandPose walk0 = HandPoseRules.Pose(HandSide.Right, false, 1f, 0f, 0f, HandMotion.Walk);
        HandPose walk1 = HandPoseRules.Pose(HandSide.Right, false, 1f, 0f, 0.25f, HandMotion.Walk);
        Assert.That(still1, Is.EqualTo(still0));
        Assert.That(walk1.X, Is.Not.EqualTo(walk0.X).Within(1e-5f));
        Assert.That(walk1.Y, Is.Not.EqualTo(walk0.Y).Within(1e-5f));
    }

    [Test]
    public void TheTwoHandsSwayOppositeWaysSideToSide()
    {
        float right = HandPoseRules.Pose(HandSide.Right, false, 1f, 0f, 0.25f, HandMotion.Walk).X - HandPoseRules.Target(HandSide.Right, false).X;
        float left = HandPoseRules.Pose(HandSide.Left, false, 1f, 0f, 0.25f, HandMotion.Walk).X - HandPoseRules.Target(HandSide.Left, false).X;
        Assert.That(right * left, Is.LessThan(0f));
    }

    [Test]
    public void SprintingSitsLowerAndSwaysMoreThanWalking()
    {
        float walkY = HandPoseRules.Pose(HandSide.Right, false, 1f, 0f, 0f, HandMotion.Walk).Y;
        float sprintY = HandPoseRules.Pose(HandSide.Right, false, 1f, 0f, 0f, HandMotion.Sprint).Y;
        Assert.That(sprintY, Is.LessThan(walkY - 0.02f));
        Assert.That(HandPoseRules.StepAmount(HandMotion.Sprint), Is.GreaterThan(HandPoseRules.StepAmount(HandMotion.Walk)));
        Assert.That(HandPoseRules.StepRate(HandMotion.Sprint), Is.GreaterThan(HandPoseRules.StepRate(HandMotion.Walk)));
    }

    [Test]
    public void CrouchingSitsLowerAndSwaysLessThanWalking()
    {
        float idleY = HandPoseRules.Pose(HandSide.Right, false, 1f, 0f, 0f, HandMotion.Idle).Y;
        float crouchY = HandPoseRules.Pose(HandSide.Right, false, 1f, 0f, 0f, HandMotion.Crouch).Y;
        Assert.That(crouchY, Is.LessThan(idleY - 0.015f));
        Assert.That(HandPoseRules.StepAmount(HandMotion.CrouchWalk), Is.LessThan(HandPoseRules.StepAmount(HandMotion.Walk)));
        Assert.That(HandPoseRules.StepRate(HandMotion.Crouch), Is.EqualTo(0f));
    }

    [Test]
    public void MotionFollowsWhatThePlayerIsDoing()
    {
        Assert.That(HandPoseRules.MotionFor(false, false, false), Is.EqualTo(HandMotion.Idle));
        Assert.That(HandPoseRules.MotionFor(true, false, false), Is.EqualTo(HandMotion.Walk));
        Assert.That(HandPoseRules.MotionFor(true, true, false), Is.EqualTo(HandMotion.Sprint));
        Assert.That(HandPoseRules.MotionFor(false, true, false), Is.EqualTo(HandMotion.Idle), "sprint with no movement is standing still");
        Assert.That(HandPoseRules.MotionFor(false, false, true), Is.EqualTo(HandMotion.Crouch));
        Assert.That(HandPoseRules.MotionFor(true, false, true), Is.EqualTo(HandMotion.CrouchWalk));
        Assert.That(HandPoseRules.MotionFor(true, true, true), Is.EqualTo(HandMotion.CrouchWalk), "crouch wins over sprint");
    }

    // ---- HandPoseRules: the raise ----

    [Test]
    public void ARaiseOfZeroSitsAtTheBottomOfTheScreen()
    {
        HandPose up = HandPoseRules.Pose(HandSide.Right, true, 1f, 0f, 0f, HandMotion.Idle);
        HandPose bottom = HandPoseRules.Pose(HandSide.Right, true, 0f, 0f, 0f, HandMotion.Idle);
        Assert.That(up.Y - bottom.Y, Is.EqualTo(HandPoseRules.BottomDrop).Within(1e-5f));
    }

    [Test]
    public void ARaiseOfOneIsThePlainTarget()
    {
        HandPose target = HandPoseRules.Target(HandSide.Right, true);
        HandPose up = HandPoseRules.Pose(HandSide.Right, true, 1f, 0f, 0f, HandMotion.Idle);
        Assert.That(up.X, Is.EqualTo(target.X).Within(1e-5f));
        Assert.That(up.Z, Is.EqualTo(target.Z).Within(1e-5f));
        Assert.That(up.Pitch, Is.EqualTo(target.Pitch).Within(1e-4f));
    }

    [Test]
    public void TheMiddleOfARaiseIsBetweenTheEnds()
    {
        float bottom = HandPoseRules.Pose(HandSide.Right, true, 0f, 0f, 0f, HandMotion.Idle).Y;
        float middle = HandPoseRules.Pose(HandSide.Right, true, 0.5f, 0f, 0f, HandMotion.Idle).Y;
        float up = HandPoseRules.Pose(HandSide.Right, true, 1f, 0f, 0f, HandMotion.Idle).Y;
        Assert.That(middle, Is.GreaterThan(bottom));
        Assert.That(middle, Is.LessThan(up));
        Assert.That(HandPoseRules.Ease(0.5f), Is.EqualTo(0.5f).Within(1e-6f));
    }

    [Test]
    public void EaseIsClampedAndNeverGoesBackwards()
    {
        Assert.That(HandPoseRules.Ease(-1f), Is.EqualTo(0f));
        Assert.That(HandPoseRules.Ease(2f), Is.EqualTo(1f));
        float last = 0f;
        for (float t = 0f; t <= 1f; t += 0.05f)
        {
            float e = HandPoseRules.Ease(t);
            Assert.That(e, Is.GreaterThanOrEqualTo(last));
            last = e;
        }
    }

    [Test]
    public void EquippingLowersTheEmptyHandsThenRaisesTheTool()
    {
        var raise = new HandRaise();
        Assert.That(raise.Shown, Is.Null);
        Assert.That(raise.Raise, Is.EqualTo(1f));

        raise.Tick(HandPoseRules.QuickLowerSeconds * 0.5f, "axe");
        Assert.That(raise.Shown, Is.Null, "still the empty hands, halfway down");
        Assert.That(raise.Raise, Is.EqualTo(0.5f).Within(1e-5f));

        raise.Tick(HandPoseRules.QuickLowerSeconds * 0.5f, "axe");
        Assert.That(raise.Shown, Is.EqualTo("axe"), "the tool changes at the bottom");
        Assert.That(raise.Raise, Is.EqualTo(0f));

        raise.Tick(HandPoseRules.RaiseSeconds * 0.5f, "axe");
        Assert.That(raise.Raise, Is.EqualTo(0.5f).Within(1e-5f));
        raise.Tick(HandPoseRules.RaiseSeconds, "axe");
        Assert.That(raise.Raise, Is.EqualTo(1f));
    }

    [Test]
    public void PuttingAwayReversesTheRaise()
    {
        var raise = new HandRaise();
        raise.Snap("axe");
        Assert.That(raise.Raise, Is.EqualTo(1f));

        raise.Tick(HandPoseRules.RaiseSeconds * 0.5f, null);
        Assert.That(raise.Shown, Is.EqualTo("axe"));
        Assert.That(raise.Raise, Is.EqualTo(0.5f).Within(1e-5f));

        raise.Tick(HandPoseRules.RaiseSeconds * 0.5f, null);
        Assert.That(raise.Shown, Is.Null, "at the bottom the tool is gone");
        Assert.That(raise.Raise, Is.EqualTo(0f));

        raise.Tick(HandPoseRules.RaiseSeconds * 2f, null);
        Assert.That(raise.Raise, Is.EqualTo(1f), "the empty hands come back to rest");
    }

    [Test]
    public void SwitchingToolsLowersTheOldOneAndRaisesTheNew()
    {
        var raise = new HandRaise();
        raise.Snap("axe");
        raise.Tick(HandPoseRules.RaiseSeconds, "hammer");
        Assert.That(raise.Shown, Is.EqualTo("hammer"));
        Assert.That(raise.Raise, Is.EqualTo(0f));
        raise.Tick(HandPoseRules.RaiseSeconds, "hammer");
        Assert.That(raise.Raise, Is.EqualTo(1f));
    }

    [Test]
    public void ChangingYourMindMidLowerRaisesTheSameToolAgain()
    {
        var raise = new HandRaise();
        raise.Snap("axe");
        raise.Tick(HandPoseRules.RaiseSeconds * 0.5f, null);
        raise.Tick(HandPoseRules.RaiseSeconds * 0.25f, "axe");
        Assert.That(raise.Shown, Is.EqualTo("axe"));
        Assert.That(raise.Raise, Is.GreaterThan(0.25f));
    }

    [Test]
    public void SnapPutsTheToolFullyUpAtOnce()
    {
        var raise = new HandRaise();
        raise.Tick(HandPoseRules.RaiseSeconds, "axe"); // mid-switch
        raise.Snap("shovel");
        Assert.That(raise.Shown, Is.EqualTo("shovel"));
        Assert.That(raise.Raise, Is.EqualTo(1f));
    }

    [Test]
    public void TheRaiseIsDeterministic()
    {
        var a = new HandRaise();
        var b = new HandRaise();
        float[] steps = { 0.016f, 0.02f, 0.016f, 0.1f, 0.016f, 0.05f, 0.2f };
        string[] wants = { "axe", "axe", "axe", null, null, "shovel", "shovel" };
        for (int i = 0; i < steps.Length; i++)
        {
            a.Tick(steps[i], wants[i]);
            b.Tick(steps[i], wants[i]);
            Assert.That(a.Raise, Is.EqualTo(b.Raise));
            Assert.That(a.Shown, Is.EqualTo(b.Shown));
        }
    }

    // ---- Which hands a tool needs ----

    [Test]
    public void TheTwoHandedToolsAreThoseInThePlan()
    {
        foreach (string id in new[] { "axe", "primitive_axe", "wood_saw", "shovel", "iron_shovel", "stone_pick_axe", "iron_pick_axe",
                                      "fishing_rod", "cane_pole", "primitive_bow", "recurve_bow", "bolt_action_rifle" })
            Assert.That(HeldTools.IsTwoHanded(id), Is.True, id);
    }

    [Test]
    public void EverythingElseIsOneHanded()
    {
        foreach (string id in new[] { "hammer", "torch", "lantern", "knife", "bucket", "sap_bucket", "canteen", "bow_drill", "flint_and_steel",
                                      "spile", "box_trap", "fish_trap", "rabbit_snare", "clay_pot", "cooking_pot", "potash_kettle", "pouch", "tarp" })
            Assert.That(HeldTools.IsTwoHanded(id), Is.False, id);
        Assert.That(HeldTools.IsTwoHanded(null), Is.False);
        Assert.That(HeldTools.IsTwoHanded("no_such_tool"), Is.False);
    }

    [Test]
    public void TheLeftHandShowsEmptyHandedAndOnTwoHandedToolsOnly()
    {
        Assert.That(HeldTools.LeftHandShown(null), Is.True, "empty hands: both at rest");
        Assert.That(HeldTools.LeftHandShown("axe"), Is.True);
        Assert.That(HeldTools.LeftHandShown("hammer"), Is.False);
        Assert.That(HeldTools.LeftHandShown("torch"), Is.False);
        Assert.That(HeldTools.LeftHandHolds(null), Is.False, "at rest it holds nothing");
        Assert.That(HeldTools.LeftHandHolds("shovel"), Is.True);
    }

    [Test]
    public void TheViewmodelLayerComesOutOfACullingMask()
    {
        int all = -1;
        int without = HandPoseRules.MaskWithout(all, 6);
        Assert.That(HandPoseRules.MaskHas(all, 6), Is.True);
        Assert.That(HandPoseRules.MaskHas(without, 6), Is.False);
        Assert.That(HandPoseRules.MaskHas(without, 5), Is.True, "other layers are kept");
        Assert.That(HandPoseRules.MaskWithout(without, 6), Is.EqualTo(without), "taking it out twice changes nothing");
        Assert.That(HandPoseRules.MaskWithout(all, -1), Is.EqualTo(all), "no layer, no change");
    }

    // ---- The Viewmodel layer and when hands show ----

    [Test]
    public void TheProjectHasAViewmodelLayerOnLayerSix()
    {
        Assert.That(LayerMask.NameToLayer("Viewmodel"), Is.EqualTo(6));
    }

    [Test]
    public void HandsShowOnlyWhileThePlayerIsPlaying()
    {
        MethodInfo visible = HandsType.GetMethod("VisibleIn");
        Type stateType = Find("GameState");
        foreach (object state in Enum.GetValues(stateType))
            Assert.That((bool)visible.Invoke(null, new[] { state }), Is.EqualTo(state.ToString() == "Playing"), state.ToString());
    }

    [Test]
    public void NoEquippedToolMeansEmptyHands()
    {
        MethodInfo wanted = HandsType.GetMethod("WantedToolId");
        Assert.That(wanted.Invoke(null, new object[] { null }), Is.Null);
        object axe = Find("ItemDatabase").GetMethod("Get", new[] { typeof(string) }).Invoke(null, new object[] { "axe" });
        Assert.That(axe, Is.Not.Null, "the Axe item exists");
        Assert.That(wanted.Invoke(null, new[] { axe }), Is.EqualTo("axe"));
    }

    // ---- ArtSwap: the hands swap point ----

    [Test]
    public void TheHandsSpecNamesItsAnchors()
    {
        object spec = Art.GetMethod("ViewSpecFor").Invoke(null, new object[] { "hands_player" });
        Assert.That(spec, Is.Not.Null);
        MethodInfo anchor = spec.GetType().GetMethod("Anchor");
        Assert.That(anchor.Invoke(spec, new object[] { "Anchor_Hand_R" }), Is.Not.Null);
        Assert.That(anchor.Invoke(spec, new object[] { "Anchor_Hand_L" }), Is.Not.Null);
        Assert.That(Art.GetMethod("ViewSpecFor").Invoke(null, new object[] { "hands_nobody" }), Is.Null);
    }

    [Test]
    public void AMissingHandsPrefabIsLookedUpOnceAndRemembered()
    {
        int calls = 0;
        Load(null, path => { calls++; return null; });
        Transform parent = Make("parent").transform;
        Assert.That(Attach(parent), Is.Null);
        Assert.That(Attach(parent), Is.Null);
        Assert.That(calls, Is.EqualTo(1));
    }

    [Test]
    public void AFoundHandsPrefabIsLookedUpOnceAndRemembered()
    {
        int calls = 0;
        GameObject prefab = StandInHands(true, true);
        Load(null, path => { calls++; return prefab; });
        Transform parent = Make("parent").transform;
        Assert.That(Attach(parent), Is.Not.Null);
        Assert.That(Art.GetMethod("Resolve").Invoke(null, new object[] { "hands_player" }), Is.SameAs(prefab));
        Assert.That(calls, Is.EqualTo(1));
    }

    [Test]
    public void TheHandsPrefabIsInstancedOnceAndKept()
    {
        Load(StandInHands(true, true));
        Transform parent = Make("parent").transform;
        Transform first = Attach(parent);
        Transform second = Attach(parent);
        Assert.That(second, Is.SameAs(first));
        Assert.That(parent.childCount, Is.EqualTo(1));
        Assert.That(first.name, Is.EqualTo("hands_player"));
        Assert.That(first.gameObject.activeSelf, Is.True);
    }

    [Test]
    public void TheHandsInstanceHasNoCollidersAndIsOnTheViewmodelLayerCastingNoShadows()
    {
        Load(StandInHands(true, true));
        Transform parent = Make("parent").transform;
        Transform art = Attach(parent);
        Assert.That(art.GetComponentsInChildren<Collider>(true).Length, Is.EqualTo(0));
        foreach (Transform t in art.GetComponentsInChildren<Transform>(true))
            Assert.That(t.gameObject.layer, Is.EqualTo(ViewLayer), t.name);
        Renderer[] renderers = art.GetComponentsInChildren<Renderer>(true);
        Assert.That(renderers.Length, Is.EqualTo(2));
        foreach (Renderer r in renderers)
            Assert.That(r.shadowCastingMode, Is.EqualTo(UnityEngine.Rendering.ShadowCastingMode.Off));
    }

    [Test]
    public void MissingHandRootsAndAnchorsAreMadeAtTheFallback()
    {
        Load(StandInHands(false, false));
        Transform art = Attach(Make("parent").transform);
        Transform handR = (Transform)Art.GetMethod("HandRoot").Invoke(null, new object[] { art, HandSide.Right });
        Transform anchorR = (Transform)Art.GetMethod("HandAnchor").Invoke(null, new object[] { art, HandSide.Right });
        Transform anchorL = (Transform)Art.GetMethod("HandAnchor").Invoke(null, new object[] { art, HandSide.Left });
        Assert.That(handR.name, Is.EqualTo("Hand_R"));
        Assert.That(handR.localPosition, Is.EqualTo(Vector3.zero));
        Assert.That(anchorR.name, Is.EqualTo("Anchor_Hand_R"));
        Assert.That(anchorR.parent, Is.SameAs(handR));
        Assert.That(anchorR.localPosition, Is.EqualTo((Vector3)Art.GetField("PalmFallback").GetValue(null)));
        Assert.That(anchorL.parent.name, Is.EqualTo("Hand_L"));
    }

    [Test]
    public void APrefabsOwnAnchorsAreUsedWhereTheyAre()
    {
        Load(StandInHands(true, true));
        Transform art = Attach(Make("parent").transform);
        Transform handR = (Transform)Art.GetMethod("HandRoot").Invoke(null, new object[] { art, HandSide.Right });
        Transform anchorR = (Transform)Art.GetMethod("HandAnchor").Invoke(null, new object[] { art, HandSide.Right });
        Assert.That(handR.parent, Is.SameAs(art));
        Assert.That(anchorR.parent, Is.SameAs(handR));
        Assert.That(anchorR.localPosition, Is.EqualTo(new Vector3(0f, 0f, 0.5f)), "the author's position, not the fallback");
    }

    [Test]
    public void AnAnchorFoundOutsideItsHandIsMovedUnderIt()
    {
        Load(StandInHands(true, true, anchorsOutsideHands: true));
        Transform art = Attach(Make("parent").transform);
        Transform handL = (Transform)Art.GetMethod("HandRoot").Invoke(null, new object[] { art, HandSide.Left });
        Transform anchorL = (Transform)Art.GetMethod("HandAnchor").Invoke(null, new object[] { art, HandSide.Left });
        Assert.That(anchorL.IsChildOf(handL), Is.True);
    }

    // ---- PlayerHands: the whole thing, built for a camera ----

    // PlayerHands lives in the main assembly, which this one cannot see, so it is driven through reflection.
    sealed class H
    {
        public readonly Component Component;
        public H(Component component) { Component = component; }

        T Get<T>(string name) => (T)HandsType.GetProperty(name).GetValue(Component);
        public bool Build(Camera cam) => (bool)HandsType.GetMethod("Build").Invoke(Component, new object[] { cam });
        public void Apply(string tool, float raise, float breath, float steps, HandMotion motion) =>
            HandsType.GetMethod("Apply").Invoke(Component, new object[] { tool, raise, breath, steps, motion });

        public GameObject Rig => Get<GameObject>("Rig");
        public Transform ArtRoot => Get<Transform>("ArtRoot");
        public Transform RightHand => Get<Transform>("RightHand");
        public Transform LeftHand => Get<Transform>("LeftHand");
        public Transform RightAnchor => Get<Transform>("RightAnchor");
        public Transform LeftAnchor => Get<Transform>("LeftAnchor");
        public Camera ViewCamera => Get<Camera>("ViewCamera");
        public object Pass => Get<object>("Pass");
        public string HeldShown => Get<string>("HeldShown");
    }

    H NewHands(out Camera cam)
    {
        GameObject camObject = Make("Test Camera");
        cam = camObject.AddComponent<Camera>();
        cam.cullingMask = -1;
        GameObject holder = Make("Test Player");
        return new H(holder.AddComponent(HandsType));
    }

    [Test]
    public void BuildingTakesTheViewmodelLayerOutOfTheMainCamera()
    {
        H hands = NewHands(out Camera cam);
        Assert.That(HandPoseRules.MaskHas(cam.cullingMask, ViewLayer), Is.True);
        Assert.That(hands.Build(cam), Is.True);
        Assert.That(HandPoseRules.MaskHas(cam.cullingMask, ViewLayer), Is.False);
        Assert.That(HandPoseRules.MaskHas(cam.cullingMask, 0), Is.True, "the world is still drawn");
    }

    [Test]
    public void TheHiddenCameraSeesOnlyTheViewmodelLayerWithItsOwnFieldOfView()
    {
        H hands = NewHands(out Camera cam);
        cam.fieldOfView = 20f; // the scope has narrowed the main camera
        hands.Build(cam);
        Assert.That(hands.ViewCamera.enabled, Is.False, "it never renders by itself");
        Assert.That(hands.ViewCamera.cullingMask, Is.EqualTo(1 << ViewLayer));
        Assert.That(hands.ViewCamera.fieldOfView, Is.EqualTo(60f));
        Assert.That(hands.ViewCamera.transform.parent, Is.SameAs(cam.transform));
    }

    [Test]
    public void ThePassKnowsBothCamerasAndStartsNotDrawing()
    {
        H hands = NewHands(out Camera cam);
        hands.Build(cam);
        Type passType = Find("ViewmodelPass");
        Assert.That(hands.Pass, Is.Not.Null);
        Assert.That(hands.Pass.GetType(), Is.EqualTo(passType));
        Assert.That(passType.GetField("viewCamera").GetValue(hands.Pass), Is.SameAs(hands.ViewCamera));
        Assert.That(passType.GetField("worldCamera").GetValue(hands.Pass), Is.SameAs(cam));
        Assert.That((bool)passType.GetField("draw").GetValue(hands.Pass), Is.False);
    }

    [Test]
    public void WithNoPrefabThePrimitiveHandsAreBuiltWithTheSwapPointsNames()
    {
        Load(null, path => null);
        H hands = NewHands(out Camera cam);
        hands.Build(cam);
        Assert.That(hands.ArtRoot.name, Is.EqualTo("hands_player"));
        Assert.That(hands.RightHand.name, Is.EqualTo("Hand_R"));
        Assert.That(hands.LeftHand.name, Is.EqualTo("Hand_L"));
        Assert.That(hands.RightAnchor.name, Is.EqualTo("Anchor_Hand_R"));
        Assert.That(hands.LeftAnchor.name, Is.EqualTo("Anchor_Hand_L"));
        Assert.That(hands.RightHand.Find("Forearm"), Is.Not.Null);
        Assert.That(hands.RightHand.Find("Mitten"), Is.Not.Null);
        Assert.That(hands.RightHand.Find("Sleeve Cuff"), Is.Not.Null);
    }

    [Test]
    public void ThePrimitiveHandsAreOnTheViewmodelLayerWithNoCollidersOrShadows()
    {
        Load(null, path => null);
        H hands = NewHands(out Camera cam);
        hands.Build(cam);
        Assert.That(hands.Rig.GetComponentsInChildren<Collider>(true).Length, Is.EqualTo(0));
        foreach (Transform t in hands.Rig.GetComponentsInChildren<Transform>(true))
            Assert.That(t.gameObject.layer, Is.EqualTo(ViewLayer), t.name);
        foreach (Renderer r in hands.Rig.GetComponentsInChildren<Renderer>(true))
            Assert.That(r.shadowCastingMode, Is.EqualTo(UnityEngine.Rendering.ShadowCastingMode.Off), r.name);
    }

    [Test]
    public void ADroppedInPrefabReplacesThePrimitiveHands()
    {
        Load(StandInHands(true, true));
        H hands = NewHands(out Camera cam);
        hands.Build(cam);
        Assert.That(hands.ArtRoot.name, Is.EqualTo("hands_player"));
        Assert.That(hands.ArtRoot.Find("Hand_R").Find("Forearm"), Is.Null, "none of the primitive parts");
        Assert.That(hands.RightAnchor.localPosition, Is.EqualTo(new Vector3(0f, 0f, 0.5f)));
        Assert.That(hands.Rig.GetComponentsInChildren<Collider>(true).Length, Is.EqualTo(0));
    }

    [Test]
    public void EmptyHandedBothHandsShowAtRest()
    {
        Load(null, path => null);
        H hands = NewHands(out Camera cam);
        hands.Build(cam);
        hands.Apply(null, 1f, 0f, 0f, HandMotion.Idle);
        Assert.That(hands.RightHand.gameObject.activeSelf, Is.True);
        Assert.That(hands.LeftHand.gameObject.activeSelf, Is.True);
        Assert.That(hands.HeldShown, Is.Null);
        Assert.That(hands.RightAnchor.childCount, Is.EqualTo(0), "nothing in hand");
        Assert.That(hands.RightHand.localPosition.x, Is.GreaterThan(0f));
        Assert.That(hands.LeftHand.localPosition.x, Is.LessThan(0f));
    }

    [Test]
    public void ATwoHandedToolShowsBothHandsAndTheOneHandedHidesTheLeft()
    {
        Load(null, path => null);
        H hands = NewHands(out Camera cam);
        hands.Build(cam);

        hands.Apply("recurve_bow", 1f, 0f, 0f, HandMotion.Idle);
        Assert.That(hands.LeftHand.gameObject.activeSelf, Is.True);
        Assert.That(hands.LeftAnchor.GetChild(0).name, Is.EqualTo("tool_recurve_bow"), "the bow shape in the LEFT hand (M0c-2 fix: the right hand draws the string)");
        Assert.That(hands.RightAnchor.childCount, Is.EqualTo(0));

        hands.Apply("bucket", 1f, 0f, 0f, HandMotion.Idle);
        Assert.That(hands.LeftHand.gameObject.activeSelf, Is.False);
        Assert.That(hands.LeftAnchor.childCount, Is.EqualTo(0), "the bow was freed");
        Assert.That(hands.RightAnchor.childCount, Is.EqualTo(1), "and the bucket is in the right hand");

        hands.Apply(null, 1f, 0f, 0f, HandMotion.Idle);
        Assert.That(hands.LeftHand.gameObject.activeSelf, Is.True);
        Assert.That(hands.RightAnchor.childCount, Is.EqualTo(0), "put away: the placeholder is gone");
    }

    [Test]
    public void ThePlaceholderIsOnTheViewmodelLayerHasNoColliderAndTakesTheCategoryColour()
    {
        Load(null, path => null);
        H hands = NewHands(out Camera cam);
        hands.Build(cam);
        hands.Apply("no_such_tool", 1f, 0f, 0f, HandMotion.Idle);
        Transform held = hands.RightAnchor.Find("Held Placeholder");
        Assert.That(held.gameObject.layer, Is.EqualTo(ViewLayer));
        Assert.That(held.GetComponent<Collider>(), Is.Null);

        var block = new MaterialPropertyBlock();
        held.GetComponent<Renderer>().GetPropertyBlock(block);
        Color tool = (Color)Find("DroppedItems").GetMethod("Tint", Any).Invoke(null, new object[] { Enum.Parse(Find("ItemCategory"), "Tool") });
        Assert.That(block.GetColor("_BaseColor"), Is.EqualTo(tool));
    }

    [Test]
    public void ApplyingAPoseMovesTheHandsToWhatTheRulesSay()
    {
        Load(null, path => null);
        H hands = NewHands(out Camera cam);
        hands.Build(cam);
        hands.Apply(null, 1f, 1.3f, 0.4f, HandMotion.Walk);
        HandPose expected = HandPoseRules.Pose(HandSide.Right, false, 1f, 1.3f, 0.4f, HandMotion.Walk);
        Assert.That(hands.RightHand.localPosition.x, Is.EqualTo(expected.X).Within(1e-5f));
        Assert.That(hands.RightHand.localPosition.y, Is.EqualTo(expected.Y).Within(1e-5f));
        Assert.That(hands.RightHand.localPosition.z, Is.EqualTo(expected.Z).Within(1e-5f));
    }

    [Test]
    public void TheRigIsScaledInTowardTheEyeSoItStaysInsideThePlayersBody()
    {
        Load(null, path => null);
        H hands = NewHands(out Camera cam);
        hands.Build(cam);
        Assert.That(hands.Rig.transform.localScale, Is.EqualTo(Vector3.one * 0.5f));
        // The farthest a hand's wrist sits from the eye, scaled in, is inside the 0.35 m body radius.
        float farthest = Mathf.Max(HandPoseRules.Target(HandSide.Right, false).Z, HandPoseRules.Target(HandSide.Left, true).Z, HandPoseRules.Target(HandSide.Right, true).Z);
        Assert.That(farthest * hands.Rig.transform.localScale.z, Is.LessThan(0.35f));
    }

    [Test]
    public void AWiderWindowPushesTheHandsOutSoTheyStayAtTheEdges()
    {
        Load(null, path => null);
        H hands = NewHands(out Camera cam);
        hands.Build(cam);
        hands.Apply(null, 1f, 0f, 0f, HandMotion.Idle);
        float narrow = hands.RightHand.localPosition.x;
        HandsType.GetProperty("WidthScale").SetValue(hands.Component, 1.5f);
        hands.Apply(null, 1f, 0f, 0f, HandMotion.Idle);
        Assert.That(hands.RightHand.localPosition.x, Is.EqualTo(narrow * 1.5f).Within(1e-5f));
        Assert.That((float)HandsType.GetField("MaxWidthScale").GetValue(null), Is.EqualTo(1.5f), "ultrawide is capped");
    }

    [Test]
    public void ThePassOverridesTheDepthTestSoLitMaterialsDrawAgainstItsOwnDepth()
    {
        string pass = Source("Scripts/Player/ViewmodelPass.cs");
        StringAssert.Contains("CompareFunction.LessEqual", pass);
        StringAssert.Contains("overrideRenderState", pass);
    }

    [Test]
    public void TheHdrpAssetsAreLeftAtDeferredOnly()
    {
        // The lit-shader-mode setting was tried at Both and put back: the depth override is the fix, not the mode. Compared by text so no asset has to load.
        foreach (string file in new[] { "Settings/HDRP High Fidelity.asset", "Settings/HDRP Balanced.asset", "Settings/HDRP Performant.asset", "Settings/HDRPDefaultResources/HDRenderPipelineAsset.asset" })
            StringAssert.Contains("supportedLitShaderMode: 2", Source(file), file);
    }

    // ---- The rifle scope: no hands while the overlay shows ----

    [Test]
    public void TheHandsAreHiddenAtFullScopeAndShownAtZero()
    {
        Assert.That(HandPoseRules.HiddenByScope(0f), Is.False);
        Assert.That(HandPoseRules.HiddenByScope(0.49f), Is.False);
        Assert.That(HandPoseRules.HiddenByScope(HandPoseRules.ScopeHideAt), Is.True, "hidden from the moment the overlay shows (the rifle draws it past 0.5)");
        Assert.That(HandPoseRules.HiddenByScope(1f), Is.True);
    }

    [Test]
    public void TheHandsSlideDownSmoothlyAsTheScopeComesUp()
    {
        Assert.That(HandPoseRules.ScopeLowering(0f), Is.EqualTo(1f));
        Assert.That(HandPoseRules.ScopeLowering(HandPoseRules.ScopeHideAt), Is.EqualTo(0f).Within(1e-6f), "fully down by the time they are hidden");
        Assert.That(HandPoseRules.ScopeLowering(1f), Is.EqualTo(0f));
        Assert.That(HandPoseRules.ScopeLowering(HandPoseRules.ScopeHideAt * 0.5f), Is.EqualTo(0.5f).Within(1e-5f));
        float last = 1f;
        for (float s = 0f; s <= 0.5f; s += 0.025f)
        {
            float lowering = HandPoseRules.ScopeLowering(s);
            Assert.That(lowering, Is.LessThanOrEqualTo(last + 1e-6f), "never comes back up while the scope rises");
            last = lowering;
        }
    }

    [Test]
    public void ALoweredHandIsAtTheBottomOfTheScreenLikeAnUnraisedOne()
    {
        // The scope scales the raise, so a fully lowered hand sits exactly where the equip raise starts.
        float scoped = HandPoseRules.Pose(HandSide.Right, true, 1f * HandPoseRules.ScopeLowering(0.5f), 0f, 0f, HandMotion.Idle).Y;
        float bottom = HandPoseRules.Pose(HandSide.Right, true, 0f, 0f, 0f, HandMotion.Idle).Y;
        Assert.That(scoped, Is.EqualTo(bottom).Within(1e-5f));
    }

    [Test]
    public void TheRifleExposesItsScopeFractionReadOnly()
    {
        Type weapon = Find("HuntingWeapon");
        PropertyInfo scope = weapon.GetProperty("ScopeFraction", BindingFlags.Public | BindingFlags.Instance);
        Assert.That(scope, Is.Not.Null);
        Assert.That(scope.PropertyType, Is.EqualTo(typeof(float)));
        Assert.That(scope.CanRead, Is.True);
        Assert.That(scope.CanWrite, Is.False, "read-only");
    }

    [Test]
    public void ThePlayerHandsReadTheScopeAndTheBowHasNone()
    {
        string hands = Source("Scripts/Player/PlayerHands.cs");
        StringAssert.Contains("weapon.ScopeFraction", hands);
        StringAssert.Contains("HandPoseRules.HiddenByScope", hands);
        StringAssert.Contains("HandPoseRules.ScopeLowering", hands);
        // The scope only ever rises for the rifle, so the bows read zero.
        StringAssert.Contains("equipped == RifleId && aim != null", Source("Scripts/Wildlife/HuntingWeapon.cs"));
    }

    // ---- The primitive hand reads as a hand ----

    [Test]
    public void ThePrimitiveHandHasASleeveACuffAForearmAMittenFingersAndAThumb()
    {
        Load(null, path => null);
        H hands = NewHands(out Camera cam);
        hands.Build(cam);
        foreach (Transform hand in new[] { hands.RightHand, hands.LeftHand })
            foreach (string part in new[] { "Sleeve", "Sleeve Cuff", "Forearm", "Mitten", "Fingers", "Thumb" })
                Assert.That(hand.Find(part), Is.Not.Null, hand.name + "/" + part);
    }

    [Test]
    public void TheCuffIsThickerThanTheForearmAndTheSleeveContrastsWithTheSkin()
    {
        Load(null, path => null);
        H hands = NewHands(out Camera cam);
        hands.Build(cam);
        Transform hand = hands.RightHand;
        Assert.That(hand.Find("Sleeve Cuff").localScale.x, Is.GreaterThan(hand.Find("Forearm").localScale.x * 1.4f));
        Assert.That(hand.Find("Sleeve").localScale.x, Is.GreaterThan(hand.Find("Forearm").localScale.x));

        Color Colour(string part)
        {
            var block = new MaterialPropertyBlock();
            hand.Find(part).GetComponent<Renderer>().GetPropertyBlock(block);
            return block.GetColor("_BaseColor");
        }
        Color skin = Colour("Forearm"), sleeve = Colour("Sleeve"), cuff = Colour("Sleeve Cuff");
        Assert.That(Mathf.Abs(skin.r - sleeve.r) + Mathf.Abs(skin.g - sleeve.g) + Mathf.Abs(skin.b - sleeve.b), Is.GreaterThan(0.4f), "sleeve and skin are clearly different colours");
        Assert.That(cuff, Is.Not.EqualTo(sleeve), "the band is a different shade from the sleeve");
        Assert.That(Colour("Mitten"), Is.EqualTo(skin));
    }

    [Test]
    public void TheMittenIsAFlatPaddleNotABall()
    {
        Load(null, path => null);
        H hands = NewHands(out Camera cam);
        hands.Build(cam);
        Vector3 s = hands.RightHand.Find("Mitten").localScale;
        Assert.That(s.y, Is.LessThan(s.x * 0.6f), "flat");
        Assert.That(hands.RightHand.Find("Fingers").localPosition.z, Is.GreaterThan(hands.RightHand.Find("Mitten").localPosition.z), "fingers sit ahead of the palm");
        Assert.That(hands.RightHand.Find("Thumb").localPosition.x, Is.LessThan(0f), "the right thumb is on the inner (left) side");
        Assert.That(hands.LeftHand.Find("Thumb").localPosition.x, Is.GreaterThan(0f), "and the left one on the right");
    }

    [Test]
    public void TheGripAnchorStillSitsInThePalm()
    {
        Load(null, path => null);
        H hands = NewHands(out Camera cam);
        hands.Build(cam);
        Transform palm = hands.RightHand.Find("Mitten");
        float z = hands.RightAnchor.localPosition.z;
        Assert.That(z, Is.InRange(palm.localPosition.z - palm.localScale.z * 0.5f, palm.localPosition.z + palm.localScale.z * 0.5f + 0.05f));
        Assert.That(hands.RightAnchor.localPosition, Is.EqualTo((Vector3)Art.GetField("PalmFallback").GetValue(null)));
    }

    const BindingFlags Any = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    // ---- Source checks ----

    static string Source(string relative)
    {
        string path = Path.Combine(Application.dataPath, relative);
        return File.ReadAllText(path);
    }

    [Test]
    public void ThePlayerGetsTheHandsAtRunTimeWithNoPrefabChange()
    {
        string source = Source("Scripts/Player/PlayerController.cs");
        StringAssert.Contains("AddComponent<PlayerHands>()", source);
    }

    [Test]
    public void ThePassDrawsBeforePostProcessingFromTheHiddenCamera()
    {
        StringAssert.Contains("CustomPassInjectionPoint.BeforePostProcess", Source("Scripts/Player/PlayerHands.cs"));
        string pass = Source("Scripts/Player/ViewmodelPass.cs");
        StringAssert.Contains("CustomPassUtils.RenderFromCamera", pass);
        StringAssert.Contains("ClearFlag.Depth", pass);
    }

    [Test]
    public void TheHandsLeaveTheHudAndTheToolScriptsAlone()
    {
        string hands = Source("Scripts/Player/PlayerHands.cs") + Source("Scripts/Player/ViewmodelPass.cs") + Source("Scripts/Economy/HandPoseRules.cs");
        StringAssert.DoesNotContain("ToolStatus", hands);
        StringAssert.DoesNotContain("ToolHud", hands);
        StringAssert.DoesNotContain("SaveVersion", hands);
        StringAssert.DoesNotContain("LedgerSaveData", hands);
    }

    [Test]
    public void TheHandsMenuItemsExistAndRemoveOnlyTheTestPrefab()
    {
        string menu = Source("Scripts/Editor/ArtSwapMenu.cs");
        StringAssert.Contains("Homestead/Art/Create Test Hands Prefab", menu);
        StringAssert.Contains("Homestead/Art/Remove Test Hands Prefab", menu);
        StringAssert.Contains("TestHands_Marker", menu);
        StringAssert.Contains("is not the generated test prefab", menu);
    }

    [Test]
    public void NoHandsTestPrefabIsLeftInResources()
    {
        Assert.That(File.Exists(Path.Combine(Application.dataPath, "Resources/Art/hands_player.prefab")), Is.False);
        Assert.That(Directory.Exists(Path.Combine(Application.dataPath, "Resources/Art/_TestHandsMaterials")), Is.False);
    }
}
