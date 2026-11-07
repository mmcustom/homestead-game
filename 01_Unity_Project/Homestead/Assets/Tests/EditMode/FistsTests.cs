using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

// EditMode tests for Hands and tools M0e-1, fists (Mike, 2026-11-05). The punch numbers, alternation, timing and poses are pure (Economy assembly: PunchRules, PunchState) and tested directly. The
// reach and wall, what a fist does to each kind of animal, the second hand state and the hook lines are in the main assembly and reached by reflection, a real Animal and PlayerController added
// without running their Awake, and the source text. What these cannot show: the real click, Play Mode feel, and the sound (frames were taken in Play Mode instead).
public class FistsTests
{
    static Type Find(string name) =>
        AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => { try { return a.GetTypes(); } catch (Exception) { return new Type[0]; } })
                 .First(t => t.Name == name);

    static Type Art => Find("ArtSwap");
    static Type HandsType => Find("PlayerHands");
    static Type AnimalType => Find("Animal");
    static Type MeleeType => Find("MeleeAttack");
    static Type EventsType => Find("ToolEvents");

    readonly List<GameObject> made = new List<GameObject>();

    [TearDown]
    public void TearDown()
    {
        foreach (GameObject go in made)
            if (go != null)
                UnityEngine.Object.DestroyImmediate(go);
        made.Clear();
        Art.GetMethod("ResetForTests").Invoke(null, null);
        EventsType.GetMethod("ResetForTests").Invoke(null, null);
    }

    GameObject Make(string name)
    {
        var go = new GameObject(name);
        made.Add(go);
        return go;
    }

    static string Source(string relative) => File.ReadAllText(Path.Combine(Application.dataPath, relative));
    static int Count(string source, string pattern) => Regex.Matches(source, pattern).Count;

    // ---- The numbers ----

    [Test]
    public void TheNumbersAreWhatMikeAskedForAndTheToolSwingIsUntouched()
    {
        Assert.That(PunchRules.CycleSeconds, Is.EqualTo(0.6f));
        Assert.That(PunchRules.ContactSeconds, Is.EqualTo(0.10f));
        Assert.That(PunchRules.StaminaPerPunch, Is.EqualTo(2f));
        Assert.That(PunchRules.ReachMetres, Is.EqualTo(1.4f));
        Assert.That(PunchRules.StunSeconds, Is.EqualTo(1f));
        Assert.That(PunchRules.AlertRadius, Is.EqualTo(8f));
        Assert.That(PunchRules.CycleSeconds, Is.LessThan(SwingRules.SwingSeconds), "faster than a tool swing");
        Assert.That(PunchRules.ContactSeconds, Is.LessThan(SwingRules.ContactSeconds));
        Assert.That(SwingRules.SwingSeconds, Is.EqualTo(0.9f), "the tools' swing is exactly as it was");
        Assert.That(SwingRules.ContactSeconds, Is.EqualTo(0.15f));
        StringAssert.Contains("float staminaPerSwing = 4f", Source("Scripts/Player/AxeTool.cs"));
        Assert.That(PunchRules.StaminaPerPunch, Is.EqualTo(4f / 2f), "half a tool blow");
        Assert.That(Enum.GetValues(typeof(WorkKind)).Length, Is.EqualTo(6), "WorkKind is untouched: the tools' timing is not the punch's");
    }

    [Test]
    public void PunchesAlternateRightLeftRightLeftStartingWithTheRight()
    {
        bool last = false; // nothing thrown yet: the first is the right
        bool next = true;
        var order = new List<bool>();
        for (int i = 0; i < 6; i++)
        {
            order.Add(next);
            last = next;
            next = PunchRules.NextIsRight(last);
        }
        CollectionAssert.AreEqual(new[] { true, false, true, false, true, false }, order);
        Assert.That((bool)MeleeType.GetProperty("NextIsRight").GetValue(Make("p").AddComponent(MeleeType)), Is.True, "the first punch of a new game is the right");
    }

    // ---- The picture ----

    [Test]
    public void ThereIsNoDelayTheFistHasNotMovedAtTheClickAndLandsAtContact()
    {
        foreach (bool right in new[] { true, false })
        {
            Assert.That(SwingRules.Magnitude(PunchRules.Offset(right, 0f)), Is.EqualTo(0f), "at the click nothing has moved");
            Assert.That(SwingRules.Magnitude(PunchRules.Offset(right, 0.02f)), Is.LessThan(SwingRules.Magnitude(PunchRules.Offset(right, 0.08f))), "and it moves away from rest, never back");
            Assert.That(PunchRules.Offset(right, PunchRules.ContactSeconds), Is.EqualTo(PunchRules.Strike(right)), "the strike lands at contact");
            Assert.That(PunchRules.Offset(right, PunchRules.ContactSeconds + PunchRules.DwellSeconds * 0.5f), Is.EqualTo(PunchRules.Strike(right)), "and holds through the dwell");
            float mid = SwingRules.Magnitude(PunchRules.Offset(right, 0.3f));
            Assert.That(mid, Is.GreaterThan(0f).And.LessThan(SwingRules.Magnitude(PunchRules.Strike(right))), "partway back");
            Assert.That(SwingRules.Magnitude(PunchRules.Offset(right, PunchRules.CycleSeconds)), Is.EqualTo(0f), "back at rest exactly when the cycle ends, so the next punch starts clean");
            Assert.That(SwingRules.Magnitude(PunchRules.Offset(right, PunchRules.CycleSeconds - 0.01f)), Is.LessThan(0.05f));
        }
    }

    [Test]
    public void TheFistTravelsTowardTheMiddleAndAlittleUpAndForwardAndTheLeftIsTheMirrorOfTheRight()
    {
        HandPose r = PunchRules.Strike(true), l = PunchRules.Strike(false);
        Assert.That(r.X, Is.LessThan(-0.1f), "the right hand travels toward the middle of the view");
        Assert.That(l.X, Is.GreaterThan(0.1f), "the left toward the middle too");
        Assert.That(r.Z, Is.GreaterThan(0.1f), "driven forward along the view");
        Assert.That(r.Y, Is.GreaterThan(0.02f), "and a little up");
        Assert.That(l.X, Is.EqualTo(-r.X).Within(1e-6f));
        Assert.That(l.Y, Is.EqualTo(r.Y));
        Assert.That(l.Z, Is.EqualTo(r.Z));
        Assert.That(l.Yaw, Is.EqualTo(-r.Yaw).Within(1e-6f));
        Assert.That(l.Roll, Is.EqualTo(-r.Roll).Within(1e-6f));
        Assert.That(r.Pitch, Is.EqualTo(l.Pitch));
    }

    [Test]
    public void ThereIsNoCockedPoseForFistsAndNothingAtRest()
    {
        var state = new PunchState();
        foreach (bool right in new[] { true, false })
        {
            Assert.That(SwingRules.Magnitude(state.Offset(right)), Is.EqualTo(0f), "idle: the hands are exactly at rest");
            state.Tick(5f);
            Assert.That(SwingRules.Magnitude(state.Offset(right)), Is.EqualTo(0f));
        }
        Assert.That(state.Any, Is.False);
    }

    // ---- The two fists ----

    [Test]
    public void EachHandPunchesAloneTheOtherStaysAtRestAndThePunchEndsByItself()
    {
        var state = new PunchState();
        state.Start(true);
        state.Tick(PunchRules.ContactSeconds);
        Assert.That(state.IsActive(true), Is.True);
        Assert.That(state.IsActive(false), Is.False, "the left hand did not move");
        Assert.That(SwingRules.Magnitude(state.Offset(false)), Is.EqualTo(0f));
        Assert.That(state.Offset(true), Is.EqualTo(PunchRules.Strike(true)));
        state.Tick(PunchRules.CycleSeconds);
        Assert.That(state.IsActive(true), Is.False, "done");
        state.Start(false);
        state.Tick(0.2f);
        Assert.That(state.IsActive(false), Is.True);
        Assert.That(state.IsActive(true), Is.False);
        state.Start(true); // a fresh right punch while the left is still coming back: both can be in the air
        Assert.That(state.IsActive(true) && state.IsActive(false), Is.True);
        state.Stop();
        Assert.That(state.Any, Is.False, "putting a fist away clears both");
    }

    [Test]
    public void TooTiredIsTheSameDroopAsATiredToolWithNoStrike()
    {
        var state = new PunchState();
        state.StartTired(true);
        Assert.That(state.IsTired(true), Is.True);
        state.Tick(0.2f);
        Assert.That(state.Offset(true), Is.EqualTo(SwingRules.TiredOffset(0.2f)), "the tired tool's droop");
        Assert.That(PunchRules.TiredSeconds, Is.EqualTo(SwingRules.TiredSeconds));
        float strike = SwingRules.Magnitude(PunchRules.Strike(true));
        for (float t = 0f; t < PunchRules.TiredSeconds; t += 0.02f)
            Assert.That(SwingRules.Magnitude(SwingRules.TiredOffset(t)), Is.LessThan(strike), "a droop never reaches a punch's size");
        state.Tick(PunchRules.TiredSeconds);
        Assert.That(state.IsActive(true), Is.False, "and it ends");
        Assert.That(SwingRules.Magnitude(state.Offset(true)), Is.EqualTo(0f));
    }

    // ---- When it acts ----

    [Test]
    public void AClickPunchesOnlyWithEmptyHandsWhileToolsCanBeUsedAndTheCycleIsOver()
    {
        Assert.That(PunchRules.ShouldPunch(true, true, true, true), Is.True);
        Assert.That(PunchRules.ShouldPunch(false, true, true, true), Is.False, "placing, knocked down by the creek, pushing the cart, or not playing: nothing");
        Assert.That(PunchRules.ShouldPunch(true, false, true, true), Is.False, "a tool in hand: the tool owns the click");
        Assert.That(PunchRules.ShouldPunch(true, true, false, true), Is.False, "no click, no punch");
        Assert.That(PunchRules.ShouldPunch(true, true, true, false), Is.False, "one per cycle: holding the click repeats, it does not stack");
        string melee = Source("Scripts/Player/MeleeAttack.cs");
        StringAssert.Contains("PunchRules.ShouldPunch(player.CanUseTools, HandsEmpty, attack.IsPressed(), Time.time >= nextPunch)", melee);
        StringAssert.Contains("inventory == null || inventory.EquippedTool == null", melee);
        StringAssert.Contains("InputSystem.actions.FindAction(\"Player/Attack\")", melee, "the same click action every tool reads");
        // and CanUseTools is the existing gate: placement, the creek knock-down and the cart push
        StringAssert.Contains("CanAct && !PlacementMode.Active && !CreekCrossing.InputLocked && !CartHandler.Pushing", Source("Scripts/Player/PlayerController.cs"));
    }

    [Test]
    public void ATiredPunchSpendsNothingThrowsNothingAndNeverGivesAFreeOne()
    {
        string melee = Source("Scripts/Player/MeleeAttack.cs").Replace("\r\n", "\n");
        int spend = melee.IndexOf("player.TrySpendStamina(PunchRules.StaminaPerPunch)", StringComparison.Ordinal);
        int tiredRaise = melee.IndexOf("ToolEvents.RaisePunch(nextRight, true);", StringComparison.Ordinal);
        int tiredReturn = melee.IndexOf("return;", tiredRaise, StringComparison.Ordinal);
        int blow = melee.IndexOf("ToolEvents.RaisePunch(right, false);", StringComparison.Ordinal);
        int strike = melee.IndexOf("Strike(player);", StringComparison.Ordinal);
        Assert.That(spend, Is.GreaterThan(0));
        Assert.That(tiredRaise, Is.GreaterThan(spend), "the tired droop is the answer to a refused spend");
        Assert.That(tiredReturn, Is.LessThan(blow), "and it returns before any blow, sound, noise or hit");
        Assert.That(strike, Is.GreaterThan(blow));
        StringAssert.Contains("nextPunch = Time.time + PunchRules.TiredSeconds;", melee, "and tries again only after the tired time: no free punches");
        StringAssert.DoesNotContain("Flash", melee, "no new HUD text");
    }

    // ---- Reach and the wall ----

    static Type PlayerType => Find("PlayerController");

    Component NewPlayer(out Transform eye)
    {
        GameObject go = Make("Test Player");
        Component player = go.AddComponent(PlayerType); // its Awake does not run in edit mode; the camera is set by hand
        eye = Make("Eye").transform;
        eye.SetParent(go.transform, false);
        eye.localPosition = new Vector3(0f, 1.6f, 0f);
        PlayerType.GetField("cameraTransform", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(player, eye);
        return player;
    }

    Component NewAnimal(string name, Vector3 at, string tier = "Small")
    {
        GameObject go = Make(name);
        go.transform.position = at;
        go.AddComponent<SphereCollider>().radius = 0.3f;
        Component animal = go.AddComponent(AnimalType);
        AnimalType.GetField("tier", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(animal, Enum.Parse(Find("GameTier"), tier));
        return animal;
    }

    static object Target(Component player) => MeleeType.GetMethod("TargetAnimal").Invoke(null, new object[] { player });

    [Test]
    public void APunchReachesAnAnimalInFrontWithinOnePointFourMetresAndNoFurther()
    {
        Component player = NewPlayer(out Transform eye);
        eye.position = new Vector3(0f, 1.6f, 0f);
        eye.rotation = Quaternion.identity; // looking along +Z
        Component near = NewAnimal("Near Rabbit", new Vector3(0f, 1.6f, 1.0f));
        Physics.SyncTransforms();
        Assert.That(Target(player), Is.SameAs(near), "1.0 m: in reach");
        near.transform.position = new Vector3(0f, 1.6f, 1.6f);
        Physics.SyncTransforms();
        Assert.That(Target(player), Is.SameAs(near), "1.6 m away: the near edge of its body is 1.3 m, inside the 1.4 m ray");
        near.transform.position = new Vector3(0f, 1.6f, 1.9f);
        Physics.SyncTransforms();
        Assert.That(Target(player), Is.Null, "1.9 m: its near edge is 1.6 m, out of the 1.4 m ray");
        near.transform.position = new Vector3(0f, 1.6f, 2.2f);
        Physics.SyncTransforms();
        Assert.That(Target(player), Is.Null, "2.2 m: out of reach");
    }

    [Test]
    public void AWallInTheWayBlocksTheHitSoAPunchNeverReachesThroughIt()
    {
        Component player = NewPlayer(out Transform eye);
        eye.position = new Vector3(0f, 1.6f, 0f);
        eye.rotation = Quaternion.identity;
        NewAnimal("Rabbit behind a wall", new Vector3(0f, 1.6f, 1.2f));
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        made.Add(wall);
        wall.transform.position = new Vector3(0f, 1.6f, 0.7f);
        wall.transform.localScale = new Vector3(3f, 3f, 0.1f);
        Physics.SyncTransforms();
        Assert.That(Target(player), Is.Null, "the first collider in the way is the wall, not the animal");
        UnityEngine.Object.DestroyImmediate(wall);
        Physics.SyncTransforms();
        Assert.That(Target(player), Is.Not.Null, "take the wall away and the same punch finds it");
    }

    // ---- What a fist does ----

    static object State(Component animal) => AnimalType.GetField("state", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(animal);
    static void SetField(Component animal, string field, object value) => AnimalType.GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(animal, value);

    static void Hit(Component animal) => MeleeType.GetMethod("Hit").Invoke(null, new object[] { animal, Vector3.zero });

    [Test]
    public void AFistNeverKillsAnythingNotASmallOneNotAMediumOneNotALargeOneNotAWoundedOne()
    {
        foreach (string tier in new[] { "Small", "Medium", "Large" })
            foreach (bool wounded in new[] { false, true })
            {
                Component animal = NewAnimal(tier + wounded, Vector3.zero, tier);
                SetField(animal, "wounded", wounded);
                SetField(animal, "willDie", true); // even one that would die on its own run is not finished off by a fist
                for (int i = 0; i < 10; i++)
                    Hit(animal);
                Assert.That((bool)AnimalType.GetProperty("IsDead").GetValue(animal), Is.False, tier + " wounded=" + wounded + ": never dead from a fist");
                Assert.That(animal.GetComponent(Find("Carcass")), Is.Null, tier + " wounded=" + wounded + ": no carcass");
                Assert.That(AnimalType.GetField("wounded", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(animal), Is.EqualTo(wounded), "a fist adds no wound and takes none away");
            }
        string animalSource = Source("Scripts/Wildlife/Animal.cs");
        int stun = animalSource.IndexOf("public void Stun(", StringComparison.Ordinal);
        int endOfStun = animalSource.IndexOf("// --- Movement ---", stun, StringComparison.Ordinal);
        string stunBody = animalSource.Substring(stun, endOfStun - stun);
        StringAssert.DoesNotContain("Die(", stunBody);
        StringAssert.DoesNotContain("wounded", stunBody);
        StringAssert.DoesNotContain("willDie", stunBody);
        StringAssert.DoesNotContain("OnShot", Source("Scripts/Player/MeleeAttack.cs"), "the shot path is never used by a fist");
    }

    [Test]
    public void ASmallLiveAnimalIsStunnedForAMomentAndABiggerOneIsJustStartled()
    {
        Component rabbit = NewAnimal("Rabbit", Vector3.zero, "Small");
        Hit(rabbit);
        Assert.That(State(rabbit).ToString(), Is.EqualTo("Stunned"));
        float until = (float)AnimalType.GetField("stateUntil", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(rabbit);
        Assert.That(until - Time.time, Is.EqualTo(PunchRules.StunSeconds).Within(0.05f), "about a second");
        foreach (string tier in new[] { "Medium", "Large" })
        {
            Component deer = NewAnimal("Deer " + tier, Vector3.zero, tier);
            Hit(deer);
            Assert.That(State(deer).ToString(), Is.EqualTo("Flee"), tier + ": startled and gone");
        }
        Assert.That(PunchRules.ForAnimal(true, true), Is.EqualTo(PunchRules.Outcome.Nothing), "a dead one or a Carcass: nothing happens");
        Assert.That(PunchRules.ForAnimal(true, false), Is.EqualTo(PunchRules.Outcome.Nothing));
        Assert.That(PunchRules.ForAnimal(false, true), Is.EqualTo(PunchRules.Outcome.StunAndStartle));
        Assert.That(PunchRules.ForAnimal(false, false), Is.EqualTo(PunchRules.Outcome.StartleOnly));
        Component dead = NewAnimal("Dead", Vector3.zero, "Small");
        SetField(dead, "state", Enum.Parse(AnimalType.GetNestedType("State", BindingFlags.NonPublic), "Dead"));
        Hit(dead);
        Assert.That(State(dead).ToString(), Is.EqualTo("Dead"), "a dead animal is left exactly as it was");
    }

    [Test]
    public void AStunnedAnimalStaysStoppedThroughTheNoiseOfTheNextPunchAndThenFleesFromTheBlow()
    {
        Component rabbit = NewAnimal("Rabbit", Vector3.zero, "Small");
        MeleeType.GetMethod("Hit").Invoke(null, new object[] { rabbit, new Vector3(5f, 0f, 0f) });
        AnimalType.GetMethod("Startle").Invoke(rabbit, new object[] { new Vector3(0f, 0f, 9f) }); // the next punch's Alert
        Assert.That(State(rabbit).ToString(), Is.EqualTo("Stunned"), "the Alert of another punch does not cut the stun short");
        Assert.That((Vector3)AnimalType.GetField("fleeFrom", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(rabbit), Is.EqualTo(new Vector3(0f, 0f, 9f)), "but it will run from the latest");
        string animal = Source("Scripts/Wildlife/Animal.cs");
        StringAssert.Contains("if (state != State.Flee && state != State.Stunned)", animal, "a stunned animal senses nothing");
        StringAssert.Contains("Startle(fleeFrom); // the moment is over: it runs as it normally would", animal);
    }

    [Test]
    public void EveryBlowMakesASmallNoiseOfEightMetresAndTheOnlyHitsAreOnAnimals()
    {
        string melee = Source("Scripts/Player/MeleeAttack.cs");
        StringAssert.Contains("WildlifeManager.Instance.Alert(from, PunchRules.AlertRadius);", melee);
        Assert.That(PunchRules.AlertRadius, Is.EqualTo(8f));
        StringAssert.Contains("gunshotEarshot = 250f", Source("Scripts/Wildlife/HuntingWeapon.cs"));
        Assert.That(PunchRules.AlertRadius, Is.LessThan(250f / 10f), "far less than a gunshot");
        int alert = melee.IndexOf("Alert(from", StringComparison.Ordinal);
        int hit = melee.IndexOf("Hit(target, from);", StringComparison.Ordinal);
        Assert.That(alert, Is.LessThan(hit), "the noise first, then the stun (so the Alert does not undo it)");
        StringAssert.Contains("AudioManager.Instance.Play(SoundCue.ItemDrop)", melee, "an existing sound: no audio file added");
    }

    // ---- The hands ----

    Component NewHands()
    {
        Camera cam = Make("Test Camera").AddComponent<Camera>();
        cam.cullingMask = -1;
        Component hands = Make("Test Hands").AddComponent(HandsType);
        HandsType.GetMethod("Build").Invoke(hands, new object[] { cam });
        return hands;
    }

    static void Apply(Component hands, string tool) => HandsType.GetMethod("Apply").Invoke(hands, new object[] { tool, 1f, 0f, 0f, HandMotion.Idle });
    static Transform Hand(Component hands, bool right) => (Transform)HandsType.GetProperty(right ? "RightHand" : "LeftHand").GetValue(hands);
    static PunchState PunchOf(Component hands) => (PunchState)HandsType.GetProperty("Punch").GetValue(hands);

    [Test]
    public void ThePunchMovesOneHandForwardTowardTheMiddleWhileTheOtherStaysAtRestAndNoArmIsStretchedOrOverMaxRise()
    {
        Art.GetField("Loader").SetValue(null, (Func<string, GameObject>)(path => null));
        Component hands = NewHands();
        Apply(hands, null);
        var rig = (GameObject)HandsType.GetProperty("Rig").GetValue(hands);
        Vector3 restR = Hand(hands, true).localPosition, restL = Hand(hands, false).localPosition;
        Vector3 forearm = Hand(hands, true).Find("Forearm").localScale, sleeve = Hand(hands, true).Find("Sleeve").localScale;
        PunchState punch = PunchOf(hands);
        foreach (bool right in new[] { true, false })
        {
            Transform fist = Hand(hands, right), other = Hand(hands, !right);
            Vector3 restFist = right ? restR : restL, restOther = right ? restL : restR;
            foreach (float t in new[] { 0f, 0.03f, 0.06f, PunchRules.ContactSeconds, 0.12f, 0.2f, 0.3f, 0.45f, 0.58f })
            {
                punch.Stop();
                punch.Start(right);
                punch.Seek(right, t);
                Apply(hands, null);
                Vector3 now = fist.localPosition;
                Assert.That(HandPoseRules.Rise(now.y, now.z), Is.LessThanOrEqualTo(HandPoseRules.MaxRise), (right ? "right" : "left") + " at " + t + " s: no more arm than the Lantern showed");
                Assert.That(Vector3.Distance(other.localPosition, restOther), Is.LessThan(1e-5f), "the other hand stays at rest");
                Assert.That(fist.Find("Forearm").localScale, Is.EqualTo(forearm), "the arm is moved, never stretched");
                Assert.That(fist.Find("Sleeve").localScale, Is.EqualTo(sleeve));
                Assert.That(fist.lossyScale.x, Is.EqualTo(rig.transform.lossyScale.x).Within(1e-5f));
                if (t == 0f)
                    Assert.That(Vector3.Distance(now, restFist), Is.LessThan(1e-5f), "no delay: at the click the fist is where it rested");
            }
            punch.Stop();
            punch.Start(right);
            punch.Seek(right, PunchRules.ContactSeconds);
            Apply(hands, null);
            Vector3 atContact = fist.localPosition;
            Assert.That(atContact.z, Is.GreaterThan(restFist.z + 0.08f), "driven forward");
            Assert.That(Mathf.Abs(atContact.x), Is.LessThan(Mathf.Abs(restFist.x) - 0.08f), "toward the middle of the view");
            Assert.That(atContact.y, Is.GreaterThan(restFist.y - 0.02f), "and not down");
            Assert.That(Mathf.Abs(atContact.x), Is.GreaterThan(0.05f), "it does not cross the crosshair");
            punch.Stop();
        }
    }

    [Test]
    public void AToolInHandKeepsItsSwingAndAFistEventNeverMovesIt()
    {
        Art.GetField("Loader").SetValue(null, (Func<string, GameObject>)(path => null));
        Component hands = NewHands();
        var raise = (HandRaise)HandsType.GetProperty("Raise").GetValue(hands);
        foreach (string tool in new[] { "axe", "hammer", "shovel", "recurve_bow", "bucket" })
        {
            Apply(hands, tool);
            raise.Snap(tool);
            Apply(hands, tool);
            Vector3 rightBefore = Hand(hands, true).localPosition, leftBefore = Hand(hands, false).localPosition;
            Quaternion rotBefore = Hand(hands, true).localRotation;
            HandsType.GetMethod("OnPunched").Invoke(hands, new object[] { true, false });
            HandsType.GetMethod("OnPunched").Invoke(hands, new object[] { false, true });
            Apply(hands, tool);
            Assert.That(PunchOf(hands).Any, Is.False, tool + ": a fist event is ignored while a tool is up");
            Assert.That(Vector3.Distance(Hand(hands, true).localPosition, rightBefore), Is.LessThan(1e-6f), tool);
            Assert.That(Vector3.Distance(Hand(hands, false).localPosition, leftBefore), Is.LessThan(1e-6f), tool);
            Assert.That(Quaternion.Angle(Hand(hands, true).localRotation, rotBefore), Is.LessThan(1e-4f), tool);
            Assert.That(((SwingState)HandsType.GetProperty("Swing").GetValue(hands)).Active, Is.False, tool + ": the tool swing state is its own");
        }
        // a tool swing still plays on its own hand: the Axe's blow is the same as before
        Apply(hands, "axe");
        raise.Snap("axe");
        HandsType.GetMethod("OnSwung").Invoke(hands, new object[] { "axe", WorkKind.Chop, false });
        Assert.That(((SwingState)HandsType.GetProperty("Swing").GetValue(hands)).Active, Is.True);
        Assert.That(PunchOf(hands).Any, Is.False, "and it does not start a fist");
    }

    [Test]
    public void PuttingAToolAwayOrPickingOneUpClearsAnyFistInTheAir()
    {
        Art.GetField("Loader").SetValue(null, (Func<string, GameObject>)(path => null));
        Component hands = NewHands();
        var raise = (HandRaise)HandsType.GetProperty("Raise").GetValue(hands);
        raise.Snap(null);
        HandsType.GetMethod("OnPunched").Invoke(hands, new object[] { true, false });
        Assert.That(PunchOf(hands).IsActive(true), Is.True, "empty hands: the punch is drawn");
        Apply(hands, "axe");
        Assert.That(PunchOf(hands).Any, Is.False, "a tool coming up clears it");
    }

    // ---- The hook, the event, the text, the swap point ----

    [Test]
    public void TheNewEventIsRaisedOnlyFromMeleeAttackInSingleStatementLines()
    {
        var seen = new List<(bool, bool)>();
        Action<bool, bool> listener = (right, tired) => seen.Add((right, tired));
        EventInfo punched = EventsType.GetEvent("Punched");
        punched.AddEventHandler(null, listener);
        try
        {
            EventsType.GetMethod("RaisePunch").Invoke(null, new object[] { true, false });
            EventsType.GetMethod("RaisePunch").Invoke(null, new object[] { false, true });
        }
        finally
        {
            punched.RemoveEventHandler(null, listener);
        }
        CollectionAssert.AreEqual(new[] { (true, false), (false, true) }, seen);

        string melee = Source("Scripts/Player/MeleeAttack.cs");
        Assert.That(Count(melee, @"ToolEvents\.RaisePunch\("), Is.EqualTo(2));
        foreach (Match line in Regex.Matches(melee, @"^.*ToolEvents\.RaisePunch\(.*$", RegexOptions.Multiline))
            Assert.That(Regex.IsMatch(line.Value.Trim(), @"^ToolEvents\.RaisePunch\([^;]*\);( //.*)?$"), Is.True, "a single statement: " + line.Value.Trim());
        foreach (string file in Directory.GetFiles(Path.Combine(Application.dataPath, "Scripts"), "*.cs", SearchOption.AllDirectories))
        {
            string name = Path.GetFileName(file);
            if (name == "MeleeAttack.cs" || name == "ToolEvents.cs")
                continue;
            StringAssert.DoesNotContain("RaisePunch(", File.ReadAllText(file), name + " must not raise the fist event");
        }
        string axe = Source("Scripts/Player/AxeTool.cs");
        Assert.That(Count(axe, @"ToolEvents\.RaiseSwung\("), Is.EqualTo(1), "the AxeTool hooks are as they were");
        Assert.That(Count(axe, @"ToolEvents\.RaiseTired\("), Is.EqualTo(1));
        StringAssert.DoesNotContain("Punch", axe);
    }

    [Test]
    public void NoHudTextWasAddedAndNoNewSwapPointOrSaveFieldAppeared()
    {
        string all = Source("Scripts/Economy/PunchRules.cs") + Source("Scripts/Player/MeleeAttack.cs");
        StringAssert.DoesNotContain("ToolStatus", all, "no HUD line, prompt, flash or banner from a fist");
        StringAssert.DoesNotContain("Flash(", all);
        StringAssert.DoesNotContain("SaveVersion", all);
        StringAssert.DoesNotContain("LedgerSaveData", all);
        var specs = ((System.Collections.IEnumerable)Art.GetProperty("AllToolSpecs").GetValue(null)).Cast<object>().ToList();
        Assert.That(specs.Count, Is.EqualTo(30), "the tool table has no fist row: the hands are hands_player.prefab through ArtSwap.TryAttachHands");
        StringAssert.DoesNotContain("Fist", Source("Scripts/World/ArtSwap.cs"), "no new swap point");
        Assert.That(ArtFolderGuard.LeftoverTestArt(), Is.Empty, "no generated test prefab or test material folder is left under Resources/Art");
        // the other readers of the click and the equipped line are untouched
        StringAssert.Contains("EquipFontSize = 28", Source("Scripts/UI/ToolHud.cs"));
    }
}
