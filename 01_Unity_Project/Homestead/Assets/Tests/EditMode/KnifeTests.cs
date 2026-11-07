using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// EditMode tests for Hands and tools M0e-2, the Knife (Mike, 2026-11-06). The numbers, alternation, timing and poses are pure (Economy assembly: KnifeRules, KnifeState) and tested directly. The
// reach and wall, what a blade does to each kind of animal, the hands' knife state and the art fallback are in the main assembly and reached by reflection (a real Animal and PlayerController
// added without running their Awake) and the source text. What these cannot show: the real click, Play Mode feel and the sound (frames were taken in Play Mode instead).
public class KnifeTests
{
    static Type Find(string name) =>
        AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => { try { return a.GetTypes(); } catch (Exception) { return new Type[0]; } })
                 .First(t => t.Name == name);

    static Type Art => Find("ArtSwap");
    static Type HandsType => Find("PlayerHands");
    static Type AnimalType => Find("Animal");
    static Type MeleeType => Find("MeleeAttack");
    static Type EventsType => Find("ToolEvents");
    static Type PlayerType => Find("PlayerController");
    static int ViewLayer => LayerMask.NameToLayer("Viewmodel");

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

    static void Load(Func<string, GameObject> loader)
    {
        Art.GetMethod("ResetForTests").Invoke(null, null);
        Art.GetField("Loader").SetValue(null, loader);
    }

    // ---- The numbers ----

    [Test]
    public void TheNumbersAreWhatMikeAskedForAndTheToolSwingAndTheFistsAreUntouched()
    {
        Assert.That(KnifeRules.SlashCycleSeconds, Is.EqualTo(0.7f));
        Assert.That(KnifeRules.StabCycleSeconds, Is.EqualTo(0.8f));
        Assert.That(KnifeRules.SlashContactSeconds, Is.EqualTo(0.12f));
        Assert.That(KnifeRules.StabContactSeconds, Is.EqualTo(0.10f));
        Assert.That(KnifeRules.StaminaPerSlash, Is.EqualTo(3f));
        Assert.That(KnifeRules.StaminaPerStab, Is.EqualTo(3f));
        Assert.That(KnifeRules.SlashReachMetres, Is.EqualTo(1.5f));
        Assert.That(KnifeRules.StabReachMetres, Is.EqualTo(1.6f));
        Assert.That(KnifeRules.AlertRadius, Is.EqualTo(6f));
        Assert.That(KnifeRules.TiredSeconds, Is.EqualTo(SwingRules.TiredSeconds));
        Assert.That(KnifeRules.StaminaPerSlash, Is.LessThan(4f), "a little under a tool blow");
        Assert.That(KnifeRules.SlashContactSeconds, Is.LessThan(SwingRules.ContactSeconds + 0.001f));
        Assert.That(SwingRules.SwingSeconds, Is.EqualTo(0.9f), "the tools' swing is unchanged");
        Assert.That(PunchRules.CycleSeconds, Is.EqualTo(0.6f), "and so are the fists'");
        Assert.That(Enum.GetValues(typeof(WorkKind)).Length, Is.EqualTo(6), "WorkKind is untouched: the Knife's timing is its own");
        Assert.That(KnifeRules.CycleSeconds(false), Is.EqualTo(0.7f));
        Assert.That(KnifeRules.CycleSeconds(true), Is.EqualTo(0.8f));
        Assert.That(KnifeRules.ReachMetres(true), Is.GreaterThan(KnifeRules.ReachMetres(false)), "a thrust reaches a little further than a slash");
    }

    // ---- Slash, then stab, and the pause ----

    [Test]
    public void ThePauseThatStartsOverWithASlashIsOnePointFiveSecondsAndNamed()
    {
        Assert.That(KnifeRules.PauseResetSeconds, Is.EqualTo(1.5f));
        Assert.That(KnifeRules.PauseResetSeconds, Is.GreaterThan(KnifeRules.StabCycleSeconds), "a held click never lets the pause run out between blows");
        Assert.That(KnifeRules.NextIsStab(true, false, KnifeRules.PauseResetSeconds - 0.01f), Is.True, "just inside the pause: the chain goes on");
        Assert.That(KnifeRules.NextIsStab(true, false, KnifeRules.PauseResetSeconds), Is.False, "at the pause: back to a slash");
        Assert.That(KnifeRules.NextIsStab(true, false, 10f), Is.False);
    }

    [Test]
    public void ClicksAlternateSlashThenStabAndTheFirstClickAfterAPauseIsASlash()
    {
        Assert.That(KnifeRules.NextIsStab(false, false, 0f), Is.False, "the very first blow is a slash");
        Assert.That(KnifeRules.NextIsStab(false, true, 0f), Is.False, "whatever the last flag says when there was no blow");
        bool hasLast = false, last = false;
        var seen = new List<bool>();
        for (int i = 0; i < 6; i++)
        {
            bool stab = KnifeRules.NextIsStab(hasLast, last, KnifeRules.CycleSeconds(last));
            seen.Add(stab);
            hasLast = true;
            last = stab;
        }
        CollectionAssert.AreEqual(new[] { false, true, false, true, false, true }, seen, "slash, stab, slash, stab...");
        Assert.That(KnifeRules.NextIsStab(true, true, 2f), Is.False, "after a stab and a pause: a slash, not a second stab");
        Assert.That(KnifeRules.NextIsStab(true, false, 2f), Is.False, "after a slash and a pause: a slash again");
    }

    // ---- Timing and poses ----

    [Test]
    public void NothingMovesAtTheClickTheBladeLandsAtContactAndBothComeBackExactlyAtTheEndOfTheirCycle()
    {
        foreach (bool stab in new[] { false, true })
        {
            HandPose from = KnifeRules.Cocked, loaded = KnifeRules.Loaded;
            Assert.That(KnifeRules.Offset(stab, 0f, from, loaded), Is.EqualTo(from), (stab ? "stab" : "slash") + ": at the click the hand has not moved (no delay)");
            float contact = KnifeRules.ContactSeconds(stab);
            Assert.That(KnifeRules.Offset(stab, contact, from, loaded), Is.EqualTo(KnifeRules.Strike(stab)), "lands at contact");
            Assert.That(KnifeRules.Offset(stab, contact + KnifeRules.DwellSeconds * 0.5f, from, loaded), Is.EqualTo(KnifeRules.Strike(stab)), "holds through the dwell");
            Assert.That(KnifeRules.Offset(stab, KnifeRules.CycleSeconds(stab), from, loaded), Is.EqualTo(loaded), "recovers to loaded exactly at the end of the cycle");
            Assert.That(KnifeRules.Offset(stab, KnifeRules.CycleSeconds(stab) + 1f, from, loaded), Is.EqualTo(loaded));
            HandPose mid = KnifeRules.Offset(stab, 0.5f, from, loaded);
            Assert.That(SwingRules.Distance(mid, loaded), Is.GreaterThan(0f).And.LessThan(SwingRules.Distance(KnifeRules.Strike(stab), loaded)), "partway back");
        }
        Assert.That(KnifeRules.SlashContactSeconds, Is.GreaterThan(KnifeRules.StabContactSeconds), "the slash's strike is drawn a little later than the stab's");
    }

    [Test]
    public void ASlashIsADiagonalArcAcrossTheViewAndAStabIsAStraightThrust()
    {
        HandPose slash = KnifeRules.Strike(false), stab = KnifeRules.Strike(true);
        Assert.That(Mathf.Abs(slash.X), Is.GreaterThan(0.08f), "a slash sweeps sideways");
        Assert.That(slash.X, Is.LessThan(0f), "from the right, across toward the left");
        Assert.That(Mathf.Abs(slash.Roll), Is.GreaterThan(15f), "the wrist rolls with the arc");
        Assert.That(Mathf.Abs(slash.Yaw), Is.GreaterThan(10f));
        Assert.That(stab.Z, Is.GreaterThan(slash.Z), "a thrust goes further forward");
        Assert.That(Mathf.Abs(stab.X), Is.LessThan(0.06f), "straight ahead");
        Assert.That(Mathf.Abs(stab.Yaw), Is.LessThan(10f), "almost no turn");
        Assert.That(Mathf.Abs(stab.Roll), Is.LessThan(10f));
        Assert.That(stab.Pitch, Is.InRange(0f, 12f), "a slight lean");
    }

    [Test]
    public void TheReadyPoseIsCockedTheLoadedPoseIsFartherBackAndEveryPoseStaysInsideMaxRise()
    {
        HandPose cocked = KnifeRules.Cocked, loaded = KnifeRules.Loaded;
        Assert.That(loaded.Z, Is.LessThan(cocked.Z), "loaded is farther back than cocked");
        Assert.That(loaded.Pitch, Is.LessThan(cocked.Pitch), "and tilted further up");
        Assert.That(cocked.Pitch, Is.LessThan(0f), "the blade drawn back and up");
        HandPose hold = HandPoseRules.Pose(HandSide.Right, true, 1f, 0f, 0f, HandMotion.Idle);
        var poses = new List<HandPose> { cocked, loaded, KnifeRules.Strike(false), KnifeRules.Strike(true) };
        for (float t = 0f; t <= 0.8f; t += 0.02f)
        {
            poses.Add(KnifeRules.Offset(false, t, cocked, loaded));
            poses.Add(KnifeRules.Offset(true, t, cocked, loaded));
        }
        foreach (HandPose pose in poses)
            Assert.That(HandPoseRules.Rise(hold.Y + pose.Y, hold.Z + pose.Z), Is.LessThanOrEqualTo(HandPoseRules.MaxRise), "no more arm than the Lantern showed");
    }

    // ---- The state over time ----

    [Test]
    public void TheKnifeStateIsAtRestUnlessTheKnifeIsInHandThenCockedAndABlowEndsByItself()
    {
        var state = new KnifeState();
        Assert.That(state.Offset, Is.EqualTo(KnifeRules.Zero), "not holding the Knife: nothing");
        state.Configure(true);
        Assert.That(state.Offset, Is.EqualTo(KnifeRules.Cocked), "the Knife in hand is held cocked");
        state.Start(false);
        Assert.That(state.Offset, Is.EqualTo(KnifeRules.Cocked), "at the click it has not moved");
        state.Tick(KnifeRules.SlashContactSeconds);
        Assert.That(state.Offset, Is.EqualTo(KnifeRules.Strike(false)));
        state.Tick(KnifeRules.SlashCycleSeconds);
        Assert.That(state.Active, Is.False, "the blow ends by itself");
        Assert.That(state.Offset, Is.EqualTo(KnifeRules.Loaded), "ends loaded");
        state.Tick(KnifeRules.SettleSeconds * 0.5f);
        Assert.That(SwingRules.Distance(state.Offset, KnifeRules.Cocked), Is.GreaterThan(0f).And.LessThan(SwingRules.Distance(KnifeRules.Loaded, KnifeRules.Cocked)), "settling");
        state.Tick(KnifeRules.SettleSeconds);
        Assert.That(state.Offset, Is.EqualTo(KnifeRules.Cocked), "and back to cocked with no new blow");
        state.Start(true);
        Assert.That(state.Stab, Is.True);
        state.Tick(KnifeRules.StabContactSeconds);
        Assert.That(state.Offset, Is.EqualTo(KnifeRules.Strike(true)));
        state.Configure(false);
        Assert.That(state.Active, Is.False, "changing tool drops a blow in progress");
        Assert.That(state.Offset, Is.EqualTo(KnifeRules.Zero));
    }

    [Test]
    public void ATiredBladeDroopsRetriesAboutEveryHalfSecondAndNeverReachesABlowsSize()
    {
        var state = new KnifeState();
        state.Configure(true);
        state.StartTired();
        Assert.That(state.Tired, Is.True);
        float strike = SwingRules.Magnitude(KnifeRules.Strike(false));
        for (float t = 0f; t < KnifeRules.TiredSeconds; t += 0.02f)
            Assert.That(SwingRules.Magnitude(KnifeRules.TiredOffset(t)), Is.LessThan(strike), "a droop is smaller than a blow");
        state.Tick(KnifeRules.TiredSeconds);
        Assert.That(state.Active, Is.False);
        Assert.That(state.Offset, Is.EqualTo(KnifeRules.Cocked), "a tired droop ends at cocked");
        Assert.That(KnifeRules.TiredSeconds, Is.EqualTo(0.5f));
    }

    // ---- When a click counts ----

    [Test]
    public void AClickCountsOnlyWithTheKnifeInHandWhileToolsCanBeUsedAndTheCycleIsOver()
    {
        Assert.That(KnifeRules.ShouldAttack(true, true, true, true), Is.True);
        Assert.That(KnifeRules.ShouldAttack(false, true, true, true), Is.False, "placing, knocked down by the creek, pushing the cart, or not playing: nothing");
        Assert.That(KnifeRules.ShouldAttack(true, false, true, true), Is.False, "another tool or empty hands: not the Knife's click");
        Assert.That(KnifeRules.ShouldAttack(true, true, false, true), Is.False, "no click, no blow");
        Assert.That(KnifeRules.ShouldAttack(true, true, true, false), Is.False, "one per cycle");
        Assert.That(KnifeRules.IsKnife("knife"), Is.True);
        Assert.That(KnifeRules.IsKnife(null), Is.False);
        Assert.That(KnifeRules.IsKnife("axe"), Is.False);
        Assert.That(PunchRules.ShouldPunch(true, false, true, true), Is.False, "and the fists still do not punch with a Knife in hand");
    }

    [Test]
    public void TheGateIsCanUseToolsWhichAlreadyPutsPlacementModeAheadAndTheFistsAreUntouched()
    {
        string melee = Source("Scripts/Player/MeleeAttack.cs");
        StringAssert.Contains("KnifeRules.ShouldAttack(player.CanUseTools, true, attack.IsPressed(), Time.time >= nextKnife)", melee);
        StringAssert.Contains("PunchRules.ShouldPunch(player.CanUseTools, HandsEmpty, attack.IsPressed(), Time.time >= nextPunch)", melee, "the fists' line is as it was");
        StringAssert.Contains("!PlacementMode.Active", Source("Scripts/Player/PlayerController.cs"), "CanUseTools: placing stays ahead of every melee click");
        int knifeBranch = melee.IndexOf("if (KnifeEquipped)", StringComparison.Ordinal);
        int punchGate = melee.IndexOf("if (!PunchRules.ShouldPunch(", StringComparison.Ordinal);
        Assert.That(knifeBranch, Is.GreaterThan(0).And.LessThan(punchGate), "the Knife branch is taken before the fists' line");
    }

    [Test]
    public void ATiredBlowSpendsNothingStrikesNothingNeverCountsAsABlowAndGivesNoFreeOnes()
    {
        string melee = Source("Scripts/Player/MeleeAttack.cs");
        int knife = melee.IndexOf("void UpdateKnife()", StringComparison.Ordinal);
        string body = melee.Substring(knife, melee.IndexOf("// The Knife's blow, at the click", knife, StringComparison.Ordinal) - knife);
        int spend = body.IndexOf("player.TrySpendStamina(KnifeRules.StaminaCost(stab))", StringComparison.Ordinal);
        int tiredRaise = body.IndexOf("ToolEvents.RaiseKnife(stab, true);", StringComparison.Ordinal);
        int remember = body.IndexOf("hasLastKnife = true;", StringComparison.Ordinal);
        int blow = body.IndexOf("ToolEvents.RaiseKnife(stab, false);", StringComparison.Ordinal);
        int strike = body.IndexOf("KnifeStrike(player, stab);", StringComparison.Ordinal);
        Assert.That(spend, Is.GreaterThan(0).And.LessThan(tiredRaise), "stamina is asked first");
        Assert.That(tiredRaise, Is.LessThan(remember), "a refusal returns before the blow is remembered, so it does not change slash and stab");
        Assert.That(remember, Is.LessThan(blow).And.LessThan(strike), "the blow, its event and its hit test all happen in the same click");
        StringAssert.Contains("nextKnife = Time.time + KnifeRules.TiredSeconds;", body, "retry about every half second");
        StringAssert.Contains("nextKnife = Time.time + KnifeRules.CycleSeconds(stab);", body);
    }

    // ---- Reach and the wall ----

    Component NewPlayer(out Transform eye)
    {
        GameObject go = Make("Test Player");
        Component player = go.AddComponent(PlayerType);
        eye = Make("Eye").transform;
        eye.SetParent(go.transform, false);
        eye.localPosition = new Vector3(0f, 1.6f, 0f);
        PlayerType.GetField("cameraTransform", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(player, eye);
        return player;
    }

    Component NewAnimal(string name, Vector3 at, string species, string tier = "Small")
    {
        GameObject go = Make(name);
        go.transform.position = at;
        go.AddComponent<SphereCollider>().radius = 0.3f;
        Component animal = go.AddComponent(AnimalType);
        AnimalType.GetField("tier", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(animal, Enum.Parse(Find("GameTier"), tier));
        AnimalType.GetField("speciesId", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(animal, species);
        return animal;
    }

    static object Target(Component player, float reach) => MeleeType.GetMethod("TargetAnimalWithin").Invoke(null, new object[] { player, reach });

    [Test]
    public void ASlashReachesOnePointFiveMetresAndAStabOnePointSixAndNoFurther()
    {
        Component player = NewPlayer(out Transform eye);
        eye.position = new Vector3(0f, 1.6f, 0f);
        eye.rotation = Quaternion.identity;
        Component rabbit = NewAnimal("Rabbit", new Vector3(0f, 1.6f, 1.5f), "rabbit");
        Physics.SyncTransforms();
        Assert.That(Target(player, KnifeRules.ReachMetres(false)), Is.SameAs(rabbit), "1.5 m: the near edge of its body is 1.2 m, inside a slash");
        rabbit.transform.position = new Vector3(0f, 1.6f, 1.9f);
        Physics.SyncTransforms();
        Assert.That(Target(player, KnifeRules.ReachMetres(false)), Is.Null, "1.9 m: its near edge is 1.6 m, out of a 1.5 m slash");
        Assert.That(Target(player, KnifeRules.ReachMetres(true)), Is.SameAs(rabbit), "but a 1.6 m stab just reaches it");
        rabbit.transform.position = new Vector3(0f, 1.6f, 2.2f);
        Physics.SyncTransforms();
        Assert.That(Target(player, KnifeRules.ReachMetres(true)), Is.Null, "2.2 m: out of reach for both");
    }

    [Test]
    public void AWallInTheWayBlocksTheKnifeSoItNeverHitsThroughIt()
    {
        Component player = NewPlayer(out Transform eye);
        eye.position = new Vector3(0f, 1.6f, 0f);
        eye.rotation = Quaternion.identity;
        NewAnimal("Rabbit behind a wall", new Vector3(0f, 1.6f, 1.2f), "rabbit");
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        made.Add(wall);
        wall.transform.position = new Vector3(0f, 1.6f, 0.7f);
        wall.transform.localScale = new Vector3(3f, 3f, 0.1f);
        Physics.SyncTransforms();
        Assert.That(Target(player, KnifeRules.ReachMetres(true)), Is.Null, "the first collider in the way is the wall");
        UnityEngine.Object.DestroyImmediate(wall);
        Physics.SyncTransforms();
        Assert.That(Target(player, KnifeRules.ReachMetres(true)), Is.Not.Null, "take the wall away and the same blow finds it");
    }

    // ---- What the blade does ----

    static object State(Component animal) => AnimalType.GetField("state", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(animal);
    static void SetField(Component animal, string field, object value) => AnimalType.GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(animal, value);
    static bool IsDead(Component animal) => (bool)AnimalType.GetProperty("IsDead").GetValue(animal);
    static void Hit(Component animal) => MeleeType.GetMethod("HitWithKnife").Invoke(null, new object[] { animal, Vector3.zero });

    [Test]
    public void TheKillTableIsSmallGameDiesADeerFleesAndTheDeadAreLeftAlone()
    {
        foreach (string species in new[] { "rabbit", "squirrel", "turkey", "waterfowl" })
        {
            Assert.That(KnifeRules.IsSmallGame(species), Is.True, species);
            Assert.That(KnifeRules.ForAnimal(false, true), Is.EqualTo(KnifeRules.Outcome.Kill), species + ": any solid hit kills");
        }
        Assert.That(KnifeRules.IsSmallGame("deer"), Is.False);
        Assert.That(KnifeRules.IsSmallGame(null), Is.False);
        Assert.That(KnifeRules.ForAnimal(false, false), Is.EqualTo(KnifeRules.Outcome.StartleOnly), "a Deer: startled and gone, never hurt");
        Assert.That(KnifeRules.ForAnimal(true, true), Is.EqualTo(KnifeRules.Outcome.Nothing), "a dead one or a Carcass: nothing");
        Assert.That(KnifeRules.ForAnimal(true, false), Is.EqualTo(KnifeRules.Outcome.Nothing));
        string melee = Source("Scripts/Player/MeleeAttack.cs");
        StringAssert.DoesNotContain("OnShot", melee, "the shot path is never used by a blade or a fist");
        StringAssert.DoesNotContain("Random.", Source("Scripts/Economy/KnifeRules.cs"), "no kill chance and no stab bonus: slash and stab have identical results");
        Assert.That(typeof(KnifeRules).GetMethod("ForAnimal").GetParameters().Length, Is.EqualTo(2), "the outcome takes no slash-or-stab argument");
    }

    [Test]
    public void ASmallLiveAnimalDiesFromAnySolidHitAliveOrWoundedAndLeavesACarcass()
    {
        foreach (string species in new[] { "rabbit", "squirrel", "turkey", "waterfowl" })
            foreach (bool wounded in new[] { false, true })
            {
                string tier = species == "turkey" || species == "waterfowl" ? "Medium" : "Small"; // as the prefabs have them
                Component animal = NewAnimal(species + wounded, Vector3.zero, species, tier);
                SetField(animal, "wounded", wounded);
                Hit(animal);
                Assert.That(IsDead(animal), Is.True, species + " wounded=" + wounded + ": dead from a knife hit");
                Assert.That(animal.GetComponent(Find("Carcass")), Is.Not.Null, species + ": a Carcass is left (field dressing works as it does today)");
            }
    }

    [Test]
    public void ADeerIsOnlyStartledNeverHurtAndADeadAnimalIsLeftExactlyAsItWas()
    {
        foreach (string tier in new[] { "Medium", "Large" })
        {
            Component deer = NewAnimal("Deer " + tier, Vector3.zero, "deer", tier);
            for (int i = 0; i < 5; i++)
                Hit(deer);
            Assert.That(State(deer).ToString(), Is.EqualTo("Flee"), tier + ": startled and gone");
            Assert.That(IsDead(deer), Is.False);
            Assert.That(AnimalType.GetField("wounded", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(deer), Is.EqualTo(false), "never wounded");
            Assert.That(deer.GetComponent(Find("Carcass")), Is.Null);
        }
        Component dead = NewAnimal("Dead", Vector3.zero, "rabbit");
        Hit(dead);
        Assert.That(IsDead(dead), Is.True);
        Component carcass = dead.GetComponent(Find("Carcass"));
        Hit(dead);
        Assert.That(dead.GetComponent(Find("Carcass")), Is.SameAs(carcass), "a second hit on the dead changes nothing (no second Carcass)");
        string animal = Source("Scripts/Wildlife/Animal.cs");
        int slay = animal.IndexOf("public void Slay()", StringComparison.Ordinal);
        string body = animal.Substring(slay, animal.IndexOf("void Die(bool reducedHarvest)", slay, StringComparison.Ordinal) - slay);
        StringAssert.Contains("Die(false)", body, "the existing kill path: a full harvest, as a clean shot");
    }

    [Test]
    public void EveryHitMakesASixMetreNoiseQuieterThanAFistAndAfterTheBlow()
    {
        string melee = Source("Scripts/Player/MeleeAttack.cs");
        StringAssert.Contains("WildlifeManager.Instance.Alert(from, KnifeRules.AlertRadius);", melee);
        Assert.That(KnifeRules.AlertRadius, Is.LessThan(PunchRules.AlertRadius), "quieter than a fist");
        Assert.That(KnifeRules.AlertRadius, Is.LessThan(250f / 10f), "far less than a gunshot");
        StringAssert.Contains("AudioManager.Instance.Play(SoundCue.ItemDrop)", melee, "an existing sound: no audio file added");
    }

    [Test]
    public void ATrappedAnimalIsOnlyAStringInTheTrapManagerSoTheKnifeHasNothingToHitThere()
    {
        string traps = Source("Scripts/Managers/TrapManager.cs");
        StringAssert.Contains("public string catchItem", traps);
        StringAssert.DoesNotContain("AddComponent<Animal>", traps);
        string melee = Source("Scripts/Player/MeleeAttack.cs");
        StringAssert.DoesNotContain("TrapManager.", melee, "no new path: a blow at a trap finds no Animal and only makes the noise");
        StringAssert.DoesNotContain("TrapManager.Instance", melee);
    }

    // ---- The hands ----

    [Test]
    public void TheKnifeIsInTheViewToolTableAndTheToolWorkTableIsOtherwiseUnchanged()
    {
        Assert.That(ToolWork.IsViewTool("knife"), Is.True);
        Assert.That(ToolWork.ViewToolIds.Count(id => id == "knife"), Is.EqualTo(1));
        Assert.That(ToolWork.ViewToolIds.Length, Is.EqualTo(30));
        Assert.That(ToolWork.KindFor("knife", false), Is.EqualTo(WorkKind.Chop), "the Knife is not a work tool: it has no WorkKind of its own");
        Assert.That(ToolWork.CockKind("knife", out WorkKind _), Is.False, "its ready pose is KnifeRules, not the tools' cock table");
        Assert.That(ToolWork.CockKind("axe", out WorkKind axe) && axe == WorkKind.Chop, Is.True, "the axe is as it was");
        Assert.That(ToolActionRules.Handles("knife"), Is.False, "and it is not one of the action tools either");
    }

    [Test]
    public void TheKnifeShapeFallsBackToThePrimitiveAndAPrefabReplacesItThroughTheOneSwapPoint()
    {
        Load(path => null);
        Transform parent = Make("parent").transform;
        var tool = (GameObject)Find("ToolViewPrimitives").GetMethod("Build").Invoke(null, new object[] { "knife", parent, ViewLayer });
        Assert.That(tool, Is.Not.Null);
        Assert.That(tool.name, Is.EqualTo("tool_knife"));
        Assert.That(tool.transform.Find("Anchor_Grip_R"), Is.Not.Null);
        Assert.That(tool.transform.Find("Anchor_Tip"), Is.Not.Null);
        Assert.That(tool.transform.Find("Anchor_Grip_L"), Is.Null, "no left grip: a one-handed tool");
        Assert.That(tool.transform.Find("Blade"), Is.Not.Null, "a blade...");
        Assert.That(tool.transform.Find("Handle"), Is.Not.Null, "...and a handle");
        Assert.That(Source("Scripts/Player/ToolViewPrimitives.cs"), Does.Contain("case \"knife\": return new Layout { twoHanded = false"));

        // the primitive is what the hands hold with no prefab...
        H hands = NewHands();
        hands.Apply("knife");
        Assert.That(hands.HeldRoot, Is.Not.Null);
        Assert.That(hands.HeldRoot.Find("Blade"), Is.Not.Null, "the primitive is the default");

        // ...and Resources/Art/tool_knife.prefab replaces it
        var prefab = new GameObject("tool_knife");
        made.Add(prefab);
        new GameObject("PrefabMarker").transform.SetParent(prefab.transform, false);
        new GameObject("Anchor_Grip_R").transform.SetParent(prefab.transform, false);
        string asked = null;
        Load(path => { asked = path; return path == "Art/tool_knife" ? prefab : null; });
        H second = NewHands();
        second.Apply("knife");
        Assert.That(asked, Is.EqualTo("Art/tool_knife"), "the swap point is Resources/Art/tool_knife.prefab");
        Assert.That(second.HeldRoot.Find("PrefabMarker"), Is.Not.Null, "the prefab is what is held");
        Assert.That(second.HeldRoot.Find("Blade"), Is.Null, "and the primitive is not");
    }

    sealed class H
    {
        public readonly Component Component;
        public H(Component component) { Component = component; }
        T Get<T>(string name) => (T)HandsType.GetProperty(name).GetValue(Component);
        public bool Build(Camera cam) => (bool)HandsType.GetMethod("Build").Invoke(Component, new object[] { cam });
        public void Apply(string tool, float raise = 1f) => HandsType.GetMethod("Apply").Invoke(Component, new object[] { tool, raise, 0f, 0f, HandMotion.Idle });
        public void Call(string method, params object[] args) => HandsType.GetMethod(method).Invoke(Component, args);
        public Transform LeftHand => Get<Transform>("LeftHand");
        public Transform RightHand => Get<Transform>("RightHand");
        public Transform HeldRoot => Get<Transform>("HeldRoot");
        public Transform LeftGrip => Get<Transform>("LeftGrip");
        public HandRaise Raise => Get<HandRaise>("Raise");
        public KnifeState Knife => Get<KnifeState>("Knife");
    }

    H NewHands()
    {
        Camera cam = Make("Test Camera").AddComponent<Camera>();
        cam.cullingMask = -1;
        var hands = new H(Make("Test Player").AddComponent(HandsType));
        hands.Build(cam);
        return hands;
    }

    [Test]
    public void TheHandsStartASlashOrStabOnlyWithTheKnifeShownAndTheLeftHandStaysIdle()
    {
        Load(path => null);
        H hands = NewHands();
        hands.Raise.Snap("axe");
        hands.Apply("axe");
        hands.Call("OnKnifed", false, false);
        Assert.That(hands.Knife.Active, Is.False, "an axe in hand: a knife event is ignored");

        hands.Raise.Snap("knife");
        hands.Apply("knife");
        Assert.That(hands.Knife.InHand, Is.True);
        Assert.That(hands.Knife.Offset, Is.EqualTo(KnifeRules.Cocked), "held cocked");
        hands.Call("OnKnifed", false, false);
        Assert.That(hands.Knife.Active && !hands.Knife.Stab, Is.True, "a slash");
        hands.Call("OnKnifed", true, false);
        Assert.That(hands.Knife.Active && hands.Knife.Stab, Is.True, "then a stab replaces it");
        hands.Call("OnKnifed", true, true);
        Assert.That(hands.Knife.Tired, Is.True, "a refusal droops");
        Assert.That(hands.LeftHand.gameObject.activeSelf, Is.False, "one-handed: the left hand is not on the Knife");
        Assert.That(hands.LeftGrip, Is.Null, "no left grip");

        hands.Raise.Snap(null);
        hands.Apply(null);
        Assert.That(hands.Knife.InHand, Is.False, "putting the Knife away drops its pose");
        Assert.That(hands.Knife.Active, Is.False);
    }

    [Test]
    public void TheKnifeStillDoesItsOldJobAndEveryOtherToolAndTheEventsAreUntouched()
    {
        string knife = Source("Scripts/Wildlife/Knife.cs");
        StringAssert.Contains("public const string Id = \"knife\";", knife);
        StringAssert.Contains("InventoryManager.Instance.Player.Has(Id)", knife, "field dressing and traps still only need one in the pack");
        Assert.That(KnifeRules.ToolId, Is.EqualTo("knife"));
        string hands = Source("Scripts/Player/PlayerHands.cs");
        StringAssert.Contains("knife.Configure(KnifeRules.IsKnife(toolId));", hands);
        StringAssert.Contains("ToolEvents.Knifed += OnKnifed;", hands);
        StringAssert.Contains("ToolEvents.Knifed -= OnKnifed;", hands);
        StringAssert.Contains("SelectPose(toolId != null);", hands, "idle hands when empty, grip hands when a tool is held: as before");
        string events = Source("Scripts/Player/ToolEvents.cs");
        StringAssert.Contains("public static event Action<string, WorkKind, bool> Swung;", events, "the tool swing event is as it was");
        StringAssert.Contains("public static event Action<bool, bool> Punched;", events, "and the fists' event");
        string axe = Source("Scripts/Player/AxeTool.cs");
        StringAssert.DoesNotContain("KnifeRules", axe, "no tool script reads the Knife's click");
    }
}
