using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// EditMode tests for Hands and tools M0c-1 (Mike, 2026-11-05): the Fishing Rod, the Cane Pole, the Torch and the Lantern in hand. ToolActionRules and ToolActionState are pure (Economy assembly) and tested
// directly; the shapes, the tool table, ToolEvents, PlayerHands and the held light are in the main assembly and reached by reflection, as the earlier hands tests do. What these cannot show: the real
// FishingRod and PortableLight running (their hook lines are checked by source), the look of the poses, and the light on the real world at night.
public class RodsAndLightsInHandTests
{
    static Type Find(string name) =>
        AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => { try { return a.GetTypes(); } catch (Exception) { return new Type[0]; } })
                 .First(t => t.Name == name);

    static Type Art => Find("ArtSwap");
    static Type HandsType => Find("PlayerHands");
    static Type Primitives => Find("ToolViewPrimitives");
    static Type Events => Find("ToolEvents");
    static int ViewLayer => LayerMask.NameToLayer("Viewmodel");

    static readonly string[] Rods = { "fishing_rod", "cane_pole" };
    static readonly string[] Lights = { "torch", "lantern" };
    static readonly string[] Four = { "fishing_rod", "cane_pole", "torch", "lantern" };

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

    sealed class H
    {
        public readonly Component Component;
        public H(Component component) { Component = component; }
        T Get<T>(string name) => (T)HandsType.GetProperty(name).GetValue(Component);
        public bool Build(Camera cam) => (bool)HandsType.GetMethod("Build").Invoke(Component, new object[] { cam });
        public void Apply(string tool, float raise = 1f) => HandsType.GetMethod("Apply").Invoke(Component, new object[] { tool, raise, 0f, 0f, HandMotion.Idle });
        public void Call(string method, params object[] args) => HandsType.GetMethod(method).Invoke(Component, args);
        public void SetLit(Func<string, bool> query)
        {
            PropertyInfo p = HandsType.GetProperty("LitQuery");
            if (p != null)
                p.SetValue(Component, query);
            else
                HandsType.GetField("LitQuery").SetValue(Component, query);
        }
        public void UpdateLight(bool shown) => HandsType.GetMethod("UpdateLight").Invoke(Component, new object[] { shown });
        public Transform RightHand => Get<Transform>("RightHand");
        public Transform LeftHand => Get<Transform>("LeftHand");
        public Transform RightAnchor => Get<Transform>("RightAnchor");
        public Transform LeftAnchor => Get<Transform>("LeftAnchor");
        public Transform HeldRoot => Get<Transform>("HeldRoot");
        public Transform LeftGrip => Get<Transform>("LeftGrip");
        public Transform FlameAnchor => Get<Transform>("FlameAnchor");
        public Transform LineAnchor => Get<Transform>("LineAnchor");
        public ToolActionState Action => Get<ToolActionState>("Action");
        public HandRaise Raise => Get<HandRaise>("Raise");
        public GameObject Rig => Get<GameObject>("Rig");
    }

    H NewHands()
    {
        Camera cam = Make("Test Camera").AddComponent<Camera>();
        cam.cullingMask = -1;
        var hands = new H(Make("Test Player").AddComponent(HandsType));
        hands.Build(cam);
        return hands;
    }

    GameObject StandIn(params string[] anchorNames)
    {
        GameObject root = Make("tool_stand_in");
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Body";
        body.transform.SetParent(root.transform, false);
        foreach (string n in anchorNames)
        {
            var a = new GameObject(n);
            a.transform.SetParent(root.transform, false);
            a.transform.localPosition = new Vector3(0.01f, 0.02f, 0.3f);
        }
        root.SetActive(false);
        return root;
    }

    GameObject BuildPrimitive(string id, Transform parent) => (GameObject)Primitives.GetMethod("Build").Invoke(null, new object[] { id, parent, ViewLayer });

    // ---- Pure: the poses ----

    [Test]
    public void TheFourToolsHaveAReadyPoseAndEverythingElseHasNone()
    {
        foreach (string id in Four)
            Assert.That(ToolActionRules.Magnitude(ToolActionRules.Ready(id)), Is.GreaterThan(0f), id);
        foreach (string id in new[] { "axe", "hammer", "knife", null })
            Assert.That(ToolActionRules.Magnitude(ToolActionRules.Ready(id)), Is.EqualTo(0f), id);
        Assert.That(ToolActionRules.Ready("fishing_rod").Pitch, Is.LessThan(0f), "the rod tip is held out and up");
        Assert.That(ToolActionRules.Ready("torch").Pitch, Is.LessThan(0f), "the torch is held up and forward");
        object layout = Primitives.GetMethod("LayoutFor").Invoke(null, new object[] { "lantern" });
        Assert.That(((Vector3)layout.GetType().GetField("flame").GetValue(layout)).y, Is.LessThan(-0.1f), "the lantern hangs below the hand that holds it");
    }

    [Test]
    public void EveryOneShotStartsAndEndsAtNothingAndMovesInTheMiddle()
    {
        foreach (ToolAction action in Enum.GetValues(typeof(ToolAction)))
        {
            float end = ToolActionRules.Seconds(action);
            if (action != ToolAction.Release) // the bow's snap starts from the drawn pose, on purpose
                Assert.That(ToolActionRules.Magnitude(ToolActionRules.ActionOffset(action, 0f)), Is.EqualTo(0f), action + " start: no delay, no wind-up before the click's own moment");
            Assert.That(ToolActionRules.Magnitude(ToolActionRules.ActionOffset(action, end)), Is.EqualTo(0f).Within(1e-6f), action + " end");
            Assert.That(ToolActionRules.Magnitude(ToolActionRules.ActionOffset(action, end * 0.3f)), Is.GreaterThan(0.005f), action + " middle");
            Assert.That(ToolActionRules.ActionOffset(action, end * 0.37f), Is.EqualTo(ToolActionRules.ActionOffset(action, end * 0.37f)), action + " deterministic");
        }
    }

    [Test]
    public void TheCastDrawsBackThenFlicksForward()
    {
        Assert.That(ToolActionRules.ActionOffset(ToolAction.Cast, 0.22f).Pitch, Is.LessThan(-10f), "back first");
        Assert.That(ToolActionRules.ActionOffset(ToolAction.Cast, 0.36f).Pitch, Is.GreaterThan(12f), "then forward");
        Assert.That(ToolActionRules.CastSeconds, Is.EqualTo(0.6f));
    }

    [Test]
    public void ABiteDipsTheTipQuicklyAndAMissIsAJerk()
    {
        Assert.That(ToolActionRules.ActionOffset(ToolAction.Bite, 0.06f).Pitch, Is.GreaterThan(8f), "the tip dips");
        Assert.That(ToolActionRules.BiteSeconds, Is.LessThan(0.5f), "and it is quick");
        Assert.That(ToolActionRules.ActionOffset(ToolAction.Miss, 0.07f).Pitch, Is.LessThan(-10f), "a miss jerks up");
        Assert.That(ToolActionRules.ActionOffset(ToolAction.Miss, 0.18f).Pitch, Is.GreaterThan(5f), "and snaps down");
    }

    [Test]
    public void ACatchPullsUpAndBack()
    {
        HandPose peak = ToolActionRules.ActionOffset(ToolAction.Catch, 0.25f);
        Assert.That(peak.Pitch, Is.LessThan(-15f));
        Assert.That(peak.Z, Is.LessThan(-0.05f));
        Assert.That(peak.Y, Is.GreaterThan(0.02f));
    }

    [Test]
    public void LightingLiftsAndSnuffingDrops()
    {
        Assert.That(ToolActionRules.ActionOffset(ToolAction.Light, 0.2f).Y, Is.GreaterThan(0.03f));
        Assert.That(ToolActionRules.ActionOffset(ToolAction.Snuff, 0.15f).Y, Is.LessThan(-0.03f));
    }

    [Test]
    public void TheCrankIsASmallCircleScaledByItsWeight()
    {
        Assert.That(ToolActionRules.Magnitude(ToolActionRules.ReelOffset(1.3f, 0f)), Is.EqualTo(0f));
        HandPose a = ToolActionRules.ReelOffset(0.1f, 1f);
        Assert.That(Mathf.Sqrt(a.X * a.X + a.Y * a.Y), Is.EqualTo(0.012f).Within(1e-4f), "a small circle");
        HandPose again = ToolActionRules.ReelOffset(0.1f + 1f / ToolActionRules.ReelTurnsPerSecond, 1f);
        Assert.That(again.X, Is.EqualTo(a.X).Within(1e-4f), "periodic");
        Assert.That(ToolActionRules.Magnitude(ToolActionRules.ReelOffset(0.1f, 0.5f)), Is.LessThan(ToolActionRules.Magnitude(a)));
    }

    [Test]
    public void TheWaitAndFlickerSwaysAreTiny()
    {
        for (float t = 0f; t < 10f; t += 0.1f)
        {
            Assert.That(Mathf.Abs(ToolActionRules.WaitSway(t).Pitch), Is.LessThan(1f));
            Assert.That(Mathf.Abs(ToolActionRules.LightSway("torch", t).Roll), Is.LessThan(1.5f));
            Assert.That(Mathf.Abs(ToolActionRules.LightSway("lantern", t).Roll), Is.LessThan(3.5f));
        }
    }

    // ---- Pure: the state over time ----

    [Test]
    public void ARodWaitsStillAfterACastAndGoesBackToReadyWhenTheLineIsIn()
    {
        var state = new ToolActionState();
        state.Configure("fishing_rod");
        HandPose ready = ToolActionRules.Ready("fishing_rod");
        Assert.That(state.Offset(false), Is.EqualTo(ready));

        state.SetFishing(true);
        state.Start(ToolAction.Cast);
        state.Tick(ToolActionRules.CastSeconds + 0.01f);
        Assert.That(state.Active, Is.False);
        state.Tick(0.8f);
        HandPose waiting = state.Offset(false);
        Assert.That(Mathf.Abs(waiting.Pitch - ready.Pitch), Is.LessThan(1f), "a tiny sway about ready");
        Assert.That(waiting, Is.Not.EqualTo(ready), "but not frozen");

        state.SetFishing(false);
        Assert.That(state.Offset(false), Is.EqualTo(ready), "line in: ready again");
    }

    [Test]
    public void TheCrankEasesInAndOutWhileReelingAndNeedsALineOut()
    {
        var state = new ToolActionState();
        state.Configure("cane_pole");
        state.SetReeling(true);
        Assert.That(state.Reeling, Is.False, "no line out, no reeling");
        state.SetFishing(true);
        state.SetReeling(true);
        state.Tick(ToolActionRules.ReelRampSeconds * 0.5f);
        Assert.That(state.ReelWeight, Is.EqualTo(0.5f).Within(1e-4f));
        state.Tick(1f);
        Assert.That(state.ReelWeight, Is.EqualTo(1f));
        state.SetReeling(false);
        state.Tick(ToolActionRules.ReelRampSeconds * 2f);
        Assert.That(state.ReelWeight, Is.EqualTo(0f));
        state.SetReeling(true);
        state.SetFishing(false);
        Assert.That(state.Reeling, Is.False, "taking the line in stops the crank");
    }

    [Test]
    public void ALightSwaysOnlyWhileLitAndConfigureForgetsTheOldTool()
    {
        var state = new ToolActionState();
        state.Configure("lantern");
        state.Tick(0.7f);
        Assert.That(state.Offset(false), Is.EqualTo(ToolActionRules.Ready("lantern")), "unlit: still");
        Assert.That(state.Offset(true), Is.Not.EqualTo(ToolActionRules.Ready("lantern")), "lit: swings");

        state.Start(ToolAction.Light);
        state.Configure("torch");
        Assert.That(state.Active, Is.False);
        Assert.That(state.ToolId, Is.EqualTo("torch"));
        state.Configure("axe");
        Assert.That(ToolActionRules.Magnitude(state.Offset(true)), Is.EqualTo(0f), "a tool this does not handle moves nothing");
    }

    // ---- The shapes ----

    [Test]
    public void TheFourShapesHaveTheirAnchors()
    {
        Transform parent = Make("parent").transform;
        foreach (string id in Four)
        {
            GameObject tool = BuildPrimitive(id, parent);
            Assert.That(tool, Is.Not.Null, id);
            Assert.That(tool.name, Is.EqualTo("tool_" + id));
            Assert.That(tool.transform.Find("Anchor_Grip_R"), Is.Not.Null, id);
            Assert.That(tool.transform.Find("Anchor_Tip"), Is.Not.Null, id);
            Assert.That(tool.transform.Find("Anchor_Grip_L") != null, Is.EqualTo(Array.IndexOf(Rods, id) >= 0), id + ": left grip on the rods only");
            Assert.That(tool.transform.Find("Anchor_Line") != null, Is.EqualTo(Array.IndexOf(Rods, id) >= 0), id + ": line anchor on the rods only");
            Assert.That(tool.transform.Find("Anchor_Flame") != null, Is.EqualTo(Array.IndexOf(Lights, id) >= 0), id + ": flame anchor on the lights only");
        }
    }

    [Test]
    public void TheLightsHaveLitAndUnlitStatesWithAFlameUnderLitAndTheRodsHaveNone()
    {
        Transform parent = Make("parent").transform;
        foreach (string id in Lights)
        {
            GameObject tool = BuildPrimitive(id, parent);
            Assert.That(tool.transform.Find("State_Lit"), Is.Not.Null, id);
            Assert.That(tool.transform.Find("State_Unlit"), Is.Not.Null, id);
            Assert.That(tool.transform.Find("State_Lit/Flame"), Is.Not.Null, id + " flame");
            Assert.That(tool.transform.Find("State_Lit/Core"), Is.Not.Null, id + " core");
        }
        foreach (string id in Rods)
        {
            GameObject tool = BuildPrimitive(id, parent);
            Assert.That(tool.transform.Find("State_Lit"), Is.Null, id);
        }
    }

    [Test]
    public void TheFlameAnchorSitsAtTheFlameAndTheLineAnchorAtTheTip()
    {
        Transform parent = Make("parent").transform;
        foreach (string id in Lights)
        {
            GameObject tool = BuildPrimitive(id, parent);
            Vector3 flame = tool.transform.Find("Anchor_Flame").localPosition;
            Assert.That(Vector3.Distance(tool.transform.Find("State_Lit/Flame").localPosition, flame), Is.LessThan(1e-4f), id);
        }
        foreach (string id in Rods)
        {
            GameObject tool = BuildPrimitive(id, parent);
            Assert.That(Vector3.Distance(tool.transform.Find("Anchor_Line").localPosition, tool.transform.Find("Anchor_Tip").localPosition), Is.LessThan(0.05f), id);
            Assert.That(tool.transform.Find("Anchor_Tip").localPosition.z, Is.GreaterThan(1.2f), id + " a long rod");
        }
    }

    [Test]
    public void TheFourShapesAreOnTheViewmodelLayerWithNoCollidersOrShadows()
    {
        Transform parent = Make("parent").transform;
        foreach (string id in Four)
        {
            GameObject tool = BuildPrimitive(id, parent);
            Assert.That(tool.GetComponentsInChildren<Collider>(true).Length, Is.EqualTo(0), id);
            foreach (Transform t in tool.GetComponentsInChildren<Transform>(true))
                Assert.That(t.gameObject.layer, Is.EqualTo(ViewLayer), id + "/" + t.name);
            foreach (Renderer r in tool.GetComponentsInChildren<Renderer>(true))
                Assert.That(r.shadowCastingMode, Is.EqualTo(UnityEngine.Rendering.ShadowCastingMode.Off), id);
        }
    }

    // ---- The swap point ----

    [Test]
    public void TheTableRowsForTheFourNameTheNewAnchors()
    {
        foreach (string id in Four)
        {
            object spec = Art.GetMethod("ToolSpecFor").Invoke(null, new object[] { id });
            Assert.That(spec, Is.Not.Null, id);
            MethodInfo anchor = spec.GetType().GetMethod("Anchor");
            Assert.That(anchor.Invoke(spec, new object[] { "Anchor_Line" }) != null, Is.EqualTo(Array.IndexOf(Rods, id) >= 0), id);
            Assert.That(anchor.Invoke(spec, new object[] { "Anchor_Flame" }) != null, Is.EqualTo(Array.IndexOf(Lights, id) >= 0), id);
            Assert.That(anchor.Invoke(spec, new object[] { "Anchor_Grip_R" }), Is.Not.Null, id);
        }
    }

    object TryAttach(Transform parent, string id) => Art.GetMethod("TryAttachTool").Invoke(null, new object[] { parent, id, ViewLayer });

    [Test]
    public void ALookupForEachOfTheFourIsFoundMissingAndCached()
    {
        foreach (string id in Four)
        {
            int calls = 0;
            GameObject prefab = StandIn("Anchor_Grip_R");
            Load(path => { calls++; return path == "Art/tool_" + id ? prefab : null; });
            Transform parent = Make("parent").transform;
            Assert.That((Transform)TryAttach(parent, id), Is.Not.Null, id);
            TryAttach(parent, id);
            Assert.That(calls, Is.EqualTo(1), id + " looked up once");

            int missing = 0;
            Load(path => { missing++; return null; });
            Assert.That((Transform)TryAttach(Make("p2").transform, id), Is.Null, id);
            Assert.That((Transform)TryAttach(Make("p3").transform, id), Is.Null, id);
            Assert.That(missing, Is.EqualTo(1), id + " a miss is remembered");
        }
    }

    [Test]
    public void ShowHeldPicksThePrefabThenThePrimitiveForTheFour()
    {
        foreach (string id in Four)
        {
            Load(path => null);
            H primitive = NewHands();
            primitive.Apply(id);
            Assert.That(primitive.HeldRoot.name, Is.EqualTo("tool_" + id));
            Assert.That(primitive.HeldRoot.Find("Anchor_Grip_R"), Is.Not.Null, id + " primitive");
            UnityEngine.Object.DestroyImmediate(primitive.Component.gameObject);

            GameObject prefab = StandIn("Anchor_Grip_R");
            Load(path => path == "Art/tool_" + id ? prefab : null);
            H fromPrefab = NewHands();
            fromPrefab.Apply(id);
            Assert.That(fromPrefab.HeldRoot.Find("Body"), Is.Not.Null, id + " prefab's own part");
            Assert.That(fromPrefab.HeldRoot.Find("Head"), Is.Null, id + " none of the primitive parts");
        }
    }

    [Test]
    public void MissingLineAndFlameAnchorsFallBackToThePrimitivesPositions()
    {
        Load(path => path == "Art/tool_fishing_rod" || path == "Art/tool_torch" ? StandIn() : null);
        H hands = NewHands();
        hands.Apply("fishing_rod");
        object layout = Primitives.GetMethod("LayoutFor").Invoke(null, new object[] { "fishing_rod" });
        Assert.That(hands.LineAnchor, Is.Not.Null);
        Assert.That(Vector3.Distance(hands.LineAnchor.localPosition, (Vector3)layout.GetType().GetField("line").GetValue(layout)), Is.LessThan(1e-4f));
        hands.Apply("torch");
        layout = Primitives.GetMethod("LayoutFor").Invoke(null, new object[] { "torch" });
        Assert.That(Vector3.Distance(hands.FlameAnchor.localPosition, (Vector3)layout.GetType().GetField("flame").GetValue(layout)), Is.LessThan(1e-4f));
    }

    // ---- The left hand, the line anchor ----

    [Test]
    public void TheLeftHandFollowsTheRodsLeftGripAndIsHiddenOnTheLights()
    {
        Load(path => null);
        H hands = NewHands();
        foreach (string id in Rods)
        {
            hands.Apply(id);
            Assert.That(hands.LeftHand.gameObject.activeSelf, Is.True, id);
            Assert.That(Vector3.Distance(hands.LeftAnchor.position, hands.LeftGrip.position), Is.LessThan(1e-3f), id);
        }
        foreach (string id in Lights)
        {
            hands.Apply(id);
            Assert.That(hands.LeftHand.gameObject.activeSelf, Is.False, id);
        }
    }

    [Test]
    public void TheLineAnchorIsOnlyGivenWhileARodIsInHand()
    {
        Load(path => null);
        H hands = NewHands();
        hands.Apply("fishing_rod");
        Assert.That(hands.LineAnchor, Is.Not.Null);
        hands.Apply("cane_pole");
        Assert.That(hands.LineAnchor, Is.Not.Null);
        hands.Apply("torch");
        Assert.That(hands.LineAnchor, Is.Null);
        hands.Apply(null);
        Assert.That(hands.LineAnchor, Is.Null);
    }

    // ---- The lit and unlit states, and the held light ----

    [Test]
    public void StateLitAndStateUnlitFollowTheLitStateOnThePrimitiveAndOnAPrefab()
    {
        foreach (bool usePrefab in new[] { false, true })
        {
            GameObject prefab = null;
            if (usePrefab)
            {
                prefab = StandIn("Anchor_Grip_R", "Anchor_Flame");
                foreach (string n in new[] { "State_Lit", "State_Unlit" })
                {
                    var s = new GameObject(n);
                    s.transform.SetParent(prefab.transform, false);
                }
            }
            Load(path => usePrefab && path == "Art/tool_torch" ? prefab : null);
            H hands = NewHands();
            bool lit = false;
            hands.SetLit(id => lit);
            hands.Apply("torch");
            hands.UpdateLight(true);
            Transform on = hands.HeldRoot.Find("State_Lit"), off = hands.HeldRoot.Find("State_Unlit");
            Assert.That(on.gameObject.activeSelf, Is.False, "unlit: no flame");
            Assert.That(off.gameObject.activeSelf, Is.True);
            lit = true;
            hands.UpdateLight(true);
            Assert.That(on.gameObject.activeSelf, Is.True, "lit: the flame");
            Assert.That(off.gameObject.activeSelf, Is.False);
            UnityEngine.Object.DestroyImmediate(hands.Component.gameObject);
        }
    }

    [Test]
    public void TheHeldLightMovesToTheFlameAndBackWithItsIntensityRangeAndColourUntouched()
    {
        Transform flame = Make("flame").transform;
        flame.position = new Vector3(3f, 2f, 1f);
        Transform camera = Make("camera").transform;
        GameObject lightObject = Make("Held Light");
        lightObject.transform.SetParent(camera, false);
        Light l = lightObject.AddComponent<Light>();
        l.type = LightType.Point;
        l.intensity = 70f;
        l.range = 13f;
        l.color = new Color(1f, 0.62f, 0.28f);
        Vector3 home = (Vector3)HandsType.GetField("HeldLightHome").GetValue(null);
        MethodInfo place = HandsType.GetMethod("PlaceHeldLight");

        place.Invoke(null, new object[] { lightObject.transform, flame, true });
        float ahead = (float)HandsType.GetField("FlameLightAhead").GetValue(null);
        Assert.That(Vector3.Distance(lightObject.transform.position, flame.position + camera.forward * ahead), Is.LessThan(1e-5f), "at the flame, a hand's width ahead of it");
        Assert.That(l.intensity, Is.EqualTo(70f));
        Assert.That(l.range, Is.EqualTo(13f));
        Assert.That(l.color, Is.EqualTo(new Color(1f, 0.62f, 0.28f)));

        place.Invoke(null, new object[] { lightObject.transform, flame, false });
        Assert.That(lightObject.transform.localPosition, Is.EqualTo(home), "back where PortableLight made it");
        Assert.That(home, Is.EqualTo(new Vector3(0.45f, -0.3f, 0.35f)), "the same place PortableLight puts it");
        place.Invoke(null, new object[] { lightObject.transform, null, true });
        Assert.That(lightObject.transform.localPosition, Is.EqualTo(home), "no flame anchor: home");
        Assert.That(l.intensity, Is.EqualTo(70f));
    }

    [Test]
    public void TheHandsNeverWriteTheLightsStrengthRangeOrColour()
    {
        string hands = Source("Scripts/Player/PlayerHands.cs");
        StringAssert.DoesNotContain(".intensity", hands);
        StringAssert.DoesNotContain(".range", hands);
        StringAssert.DoesNotContain("heldLight.color", hands);
        string light = Source("Scripts/Survival/PortableLight.cs");
        StringAssert.Contains("const float TorchIntensity = 70f, TorchRange = 13f;", light);
        StringAssert.Contains("const float LanternIntensity = 120f, LanternRange = 19f;", light);
    }

    // ---- The hands react to ToolEvents ----

    [Test]
    public void TheHandsReactToTheRodsAndLightsEventsForTheToolInHandOnly()
    {
        Load(path => null);
        H hands = NewHands();
        hands.Apply("fishing_rod");
        hands.Raise.Snap("fishing_rod");

        hands.Call("OnCast", "fishing_rod");
        Assert.That(hands.Action.Fishing, Is.True);
        Assert.That(hands.Action.Action, Is.EqualTo(ToolAction.Cast));
        hands.Call("OnBite", "fishing_rod");
        Assert.That(hands.Action.Action, Is.EqualTo(ToolAction.Bite));
        hands.Call("OnReelStart", "fishing_rod");
        Assert.That(hands.Action.Reeling, Is.True);
        hands.Call("OnReelStop", "fishing_rod");
        Assert.That(hands.Action.Reeling, Is.False);
        hands.Call("OnLanded", "fishing_rod");
        Assert.That(hands.Action.Action, Is.EqualTo(ToolAction.Catch));
        hands.Call("OnFailed", "fishing_rod");
        Assert.That(hands.Action.Action, Is.EqualTo(ToolAction.Miss));
        hands.Call("OnLineIn", "fishing_rod");
        Assert.That(hands.Action.Fishing, Is.False);

        hands.Call("OnCast", "cane_pole"); // not the rod in hand
        Assert.That(hands.Action.Fishing, Is.False);

        hands.Apply("lantern");
        hands.Raise.Snap("lantern");
        hands.Call("OnLit", "lantern");
        Assert.That(hands.Action.Action, Is.EqualTo(ToolAction.Light));
        hands.Call("OnSnuffed", "lantern");
        Assert.That(hands.Action.Action, Is.EqualTo(ToolAction.Snuff));
    }

    [Test]
    public void ACastMovesTheRightHandAndItComesBackToReady()
    {
        Load(path => null);
        H hands = NewHands();
        hands.Apply("fishing_rod");
        hands.Raise.Snap("fishing_rod");
        Quaternion ready = hands.RightHand.localRotation;
        hands.Call("OnCast", "fishing_rod");
        hands.Action.Seek(0.22f);
        hands.Apply("fishing_rod");
        Assert.That(Quaternion.Angle(ready, hands.RightHand.localRotation), Is.GreaterThan(4f), "drawn back");
        hands.Action.Tick(1f);
        hands.Action.SetFishing(false);
        hands.Apply("fishing_rod");
        Assert.That(Quaternion.Angle(ready, hands.RightHand.localRotation), Is.LessThan(0.5f), "and back at ready");
    }

    // ---- ToolEvents ----

    [Test]
    public void TheReelRaisesStartAndStopOnlyOnChange()
    {
        var seen = new List<string>();
        Action<string> start = t => seen.Add("start:" + t), stop = t => seen.Add("stop:" + t);
        EventInfo a = Events.GetEvent("ReelStart"), b = Events.GetEvent("ReelStop");
        a.AddEventHandler(null, start);
        b.AddEventHandler(null, stop);
        MethodInfo set = Events.GetMethod("SetReeling");
        try
        {
            set.Invoke(null, new object[] { "fishing_rod", true });
            set.Invoke(null, new object[] { "fishing_rod", true });
            set.Invoke(null, new object[] { "fishing_rod", false });
            set.Invoke(null, new object[] { "fishing_rod", false });
            set.Invoke(null, new object[] { "cane_pole", true });
            set.Invoke(null, new object[] { null, false });
        }
        finally { a.RemoveEventHandler(null, start); b.RemoveEventHandler(null, stop); }
        CollectionAssert.AreEqual(new[] { "start:fishing_rod", "stop:fishing_rod", "start:cane_pole", "stop:cane_pole" }, seen);
    }

    [Test]
    public void EachOfTheNewEventsCarriesTheToolId()
    {
        foreach (string name in new[] { "Cast", "Bite", "Landed", "Failed", "LineIn", "Lit", "Snuffed" })
        {
            string got = null;
            Action<string> handler = t => got = t;
            EventInfo e = Events.GetEvent(name);
            e.AddEventHandler(null, handler);
            try { Events.GetMethod("Raise" + name).Invoke(null, new object[] { "torch" }); }
            finally { e.RemoveEventHandler(null, handler); }
            Assert.That(got, Is.EqualTo("torch"), name);
        }
    }

    // ---- Source checks: the hooks, the line start, the menu ----

    static int Count(string source, string pattern) => System.Text.RegularExpressions.Regex.Matches(source, pattern).Count;

    [Test]
    public void FishingRodRaisesItsEventsWhereTheyHappen()
    {
        string rod = Source("Scripts/Fishing/FishingRod.cs");
        Assert.That(Count(rod, @"ToolEvents\.RaiseCast\("), Is.EqualTo(1));
        Assert.That(Count(rod, @"ToolEvents\.RaiseBite\("), Is.EqualTo(1));
        Assert.That(Count(rod, @"ToolEvents\.RaiseLanded\("), Is.EqualTo(1));
        Assert.That(Count(rod, @"ToolEvents\.RaiseFailed\("), Is.EqualTo(3), "a missed hookset, a fish that let go, a snapped line");
        Assert.That(Count(rod, @"ToolEvents\.SetReeling\("), Is.EqualTo(2), "the reel each frame, and a stop when the cast ends");
        Assert.That(Count(rod, @"ToolEvents\.RaiseLineIn\("), Is.EqualTo(1));
        // the cast's own lines are still there and still first
        Assert.That(rod.IndexOf("PlaySound(a => a.Splash, bobberRest);"), Is.LessThan(rod.IndexOf("ToolEvents.RaiseCast(")));
        StringAssert.Contains("float biteWindow = 1.2f", rod);
    }

    [Test]
    public void PortableLightRaisesLitAndSnuffedAtTheToggle()
    {
        string light = Source("Scripts/Survival/PortableLight.cs");
        Assert.That(Count(light, @"ToolEvents\.RaiseLit\("), Is.EqualTo(1));
        Assert.That(Count(light, @"ToolEvents\.RaiseSnuffed\("), Is.EqualTo(1));
        StringAssert.Contains("Torch — burning, {left:0.0} h left. Click to snuff it.", light, "the idle text is untouched (M0d)");
    }

    [Test]
    public void TheRodsLineStartsAtTheLineAnchorWhenTheHandsShowOne()
    {
        string rod = Source("Scripts/Fishing/FishingRod.cs");
        StringAssert.Contains("PlayerHands.Instance.LineAnchor", rod);
        StringAssert.Contains("cam.position + cam.forward * 0.9f + cam.right * 0.35f + Vector3.up * 0.25f", rod, "the old start is still the fallback");
    }

    [Test]
    public void OnlyAxeToolFishingRodAndPortableLightMentionToolEvents()
    {
        foreach (string file in new[] { "Player/CartHandler.cs", "Wildlife/Carcass.cs", "Managers/TrapManager.cs" })
            StringAssert.DoesNotContain("ToolEvents", Source("Scripts/" + file), file);
    }

    [Test]
    public void TheRodAndTorchMenuItemsExistAndRemoveOnlyTheirTestPrefabs()
    {
        string menu = Source("Scripts/Editor/ArtSwapMenu.cs");
        foreach (string item in new[] { "Create Test Rod Prefab", "Remove Test Rod Prefab", "Create Test Torch Prefab", "Remove Test Torch Prefab" })
            StringAssert.Contains("Homestead/Art/" + item, menu);
        StringAssert.Contains("TestRod_Marker", menu);
        StringAssert.Contains("TestTorch_Marker", menu);
        foreach (string anchor in new[] { "AnchorLine", "AnchorFlame", "AnchorGripL", "StateLit", "StateUnlit" })
            StringAssert.Contains("ArtSwap." + anchor, menu);
        Assert.That(ArtFolderGuard.LeftoverTestArt(), Is.Empty, "no generated test prefab or test material folder is left under Resources/Art");
    }

    [Test]
    public void TheSwapPointNoteIsInTheArtSwapHeader()
    {
        string art = Source("Scripts/World/ArtSwap.cs");
        StringAssert.Contains("M0c-1 adds four rows", art);
        StringAssert.Contains("Anchor_Line", art);
        StringAssert.Contains("Anchor_Flame", art);
    }

    [Test]
    public void NoSaveFieldsOrHudTextChanged()
    {
        string all = Source("Scripts/Economy/ToolActionRules.cs") + Source("Scripts/Player/PlayerHands.cs") + Source("Scripts/Player/ToolEvents.cs");
        StringAssert.DoesNotContain("SaveVersion", all);
        StringAssert.DoesNotContain("LedgerSaveData", all);
        StringAssert.DoesNotContain("ToolStatus", all);
        StringAssert.DoesNotContain("ToolHud", all);
    }
}
