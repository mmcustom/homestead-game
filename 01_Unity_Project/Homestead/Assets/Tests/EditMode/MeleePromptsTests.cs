using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// EditMode tests for Hands and tools M0e-3 (Mike, 2026-11-07): the aimed-target prompts, the Know-how entry "Fighting with your hands and a Knife", the one small-game list the fists and the Knife
// share, and the Knife's sound cues. The strings and tables are pure (Economy assembly: MeleeRules, KnowHowRules) and tested directly; the real Animal, the raycast, the save classes and the hook
// lines are in the main assembly and reached by reflection and the source text. What these cannot show: the HUD drawing the line, the real click and the sound (Play Mode).
public class MeleePromptsTests
{
    static Type Find(string name) =>
        AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => { try { return a.GetTypes(); } catch (Exception) { return new Type[0]; } })
                 .First(t => t.Name == name);

    static Type AnimalType => Find("Animal");
    static Type MeleeType => Find("MeleeAttack");
    static Type PlayerType => Find("PlayerController");

    readonly List<GameObject> made = new List<GameObject>();

    [TearDown]
    public void TearDown()
    {
        foreach (GameObject go in made)
            if (go != null)
                UnityEngine.Object.DestroyImmediate(go);
        made.Clear();
    }

    GameObject Make(string name)
    {
        var go = new GameObject(name);
        made.Add(go);
        return go;
    }

    static string Source(string relative) => File.ReadAllText(Path.Combine(Application.dataPath, relative));

    // ---- The prompt strings ----

    [Test]
    public void ThePromptIsTheAnimalsNameThenClickToAndTheVerb()
    {
        Assert.That(MeleeRules.Prompt("Rabbit", "punch"), Is.EqualTo("Rabbit: click to punch"));
        Assert.That(MeleeRules.Prompt("Deer", MeleeRules.SlashVerb), Is.EqualTo("Deer: click to slash"));
        Assert.That(MeleeRules.Prompt("Squirrel", MeleeRules.StabVerb), Is.EqualTo("Squirrel: click to stab"));
        Assert.That(MeleeRules.Prompt(null, "punch"), Is.EqualTo("Animal: click to punch"), "a blank name still reads sensibly");
        Assert.That(MeleeRules.Prompt("", "punch"), Is.EqualTo("Animal: click to punch"));
    }

    [Test]
    public void EmptyHandsPromptToPunchAtALiveAnimalInReach()
    {
        Assert.That(MeleeRules.PromptFor(true, MeleeRules.Mode.Fists, false, true, false, "Rabbit"), Is.EqualTo("Rabbit: click to punch"));
        Assert.That(MeleeRules.PromptFor(true, MeleeRules.Mode.Fists, true, true, false, "Turkey"), Is.EqualTo("Turkey: click to punch"), "the stab flag means nothing to a fist");
        Assert.That(MeleeRules.PromptFor(true, MeleeRules.Mode.Fists, false, true, false, "Deer"), Is.EqualTo("Deer: click to punch"), "a Deer too: the fist startles it");
    }

    [Test]
    public void TheKnifePromptShowsOnlyTheNextBlowAndNoSprintIsMentionedAnywhere()
    {
        string slash = MeleeRules.PromptFor(true, MeleeRules.Mode.Knife, false, true, false, "Rabbit");
        string stab = MeleeRules.PromptFor(true, MeleeRules.Mode.Knife, true, true, false, "Rabbit");
        Assert.That(slash, Is.EqualTo("Rabbit: click to slash"));
        Assert.That(stab, Is.EqualTo("Rabbit: click to stab"));
        StringAssert.DoesNotContain("stab", slash, "not both at once");
        StringAssert.DoesNotContain("slash", stab);
        StringAssert.DoesNotContain("Sprint", slash + stab);
        StringAssert.DoesNotContain("Sprint", Source("Scripts/Economy/MeleeRules.cs").Split(new[] { "public static string Prompt(" }, StringSplitOptions.None).Last(), "no Sprint text in the prompt code (only a comment above it says the old plan is gone)");
        StringAssert.DoesNotContain("Sprint", KnowHowRules.Entries.First(e => e.id == KnowHowRules.Melee).body);
    }

    [Test]
    public void TheNextBlowFromNextIsStabDrivesTheKnifePromptThroughAChainOfBlows()
    {
        bool hasLast = false, last = false;
        var prompts = new List<string>();
        for (int i = 0; i < 4; i++)
        {
            bool stab = KnifeRules.NextIsStab(hasLast, last, KnifeRules.CycleSeconds(last)); // what MeleeAttack.NextIsStab computes
            prompts.Add(MeleeRules.PromptFor(true, MeleeRules.Mode.Knife, stab, true, false, "Rabbit"));
            hasLast = true;
            last = stab;
        }
        CollectionAssert.AreEqual(new[] { "Rabbit: click to slash", "Rabbit: click to stab", "Rabbit: click to slash", "Rabbit: click to stab" }, prompts);
        bool stabAfterPause = KnifeRules.NextIsStab(true, true, KnifeRules.PauseResetSeconds);
        Assert.That(MeleeRules.PromptFor(true, MeleeRules.Mode.Knife, stabAfterPause, true, false, "Rabbit"), Is.EqualTo("Rabbit: click to slash"), "after a pause the prompt says slash again");
        string melee = Source("Scripts/Player/MeleeAttack.cs");
        StringAssert.Contains("bool stab = NextIsStab;", melee);
        StringAssert.Contains("public bool NextIsStab => KnifeRules.NextIsStab(hasLastKnife, lastWasStab, Time.time - lastKnifeTime);", melee);
        StringAssert.Contains("MeleeRules.PromptFor(canUse, mode, stab,", melee);
    }

    [Test]
    public void NoPromptForADeadAnimalOrACarcassSoItsOwnPromptIsUntouched()
    {
        foreach (MeleeRules.Mode mode in new[] { MeleeRules.Mode.Fists, MeleeRules.Mode.Knife })
            foreach (bool stab in new[] { false, true })
                Assert.That(MeleeRules.PromptFor(true, mode, stab, true, true, "Rabbit"), Is.Null, mode + " stab=" + stab);
        string carcass = Source("Scripts/Wildlife/Carcass.cs");
        StringAssert.Contains("$\"Field Dress {animal.DisplayName}  ({Freshness})\"", carcass, "the existing field-dress prompt is as it was");
        StringAssert.Contains("needs a Knife to field dress", carcass);
        string melee = Source("Scripts/Player/MeleeAttack.cs");
        StringAssert.DoesNotContain("InteractionPrompt", melee, "the melee line is a ToolStatus line; the interaction prompt is not touched");
        StringAssert.DoesNotContain("IInteractable", melee);
        StringAssert.DoesNotContain("Carcass", melee.Substring(melee.IndexOf("void UpdatePrompt()", StringComparison.Ordinal), 900));
    }

    [Test]
    public void NoPromptOutOfReachBehindAWallOrWithNothingMelee()
    {
        Assert.That(MeleeRules.PromptFor(true, MeleeRules.Mode.Fists, false, false, false, "Rabbit"), Is.Null, "nothing in reach");
        Assert.That(MeleeRules.PromptFor(true, MeleeRules.Mode.Knife, false, false, false, "Rabbit"), Is.Null);
        Assert.That(MeleeRules.PromptFor(true, MeleeRules.Mode.None, false, true, false, "Rabbit"), Is.Null, "an axe or any other tool in hand: no melee prompt");
        Assert.That(MeleeRules.ModeFor(true, false), Is.EqualTo(MeleeRules.Mode.Fists));
        Assert.That(MeleeRules.ModeFor(false, true), Is.EqualTo(MeleeRules.Mode.Knife));
        Assert.That(MeleeRules.ModeFor(false, false), Is.EqualTo(MeleeRules.Mode.None));
        Assert.That(MeleeRules.ReachFor(MeleeRules.Mode.Fists, false), Is.EqualTo(1.4f));
        Assert.That(MeleeRules.ReachFor(MeleeRules.Mode.Knife, false), Is.EqualTo(1.5f));
        Assert.That(MeleeRules.ReachFor(MeleeRules.Mode.Knife, true), Is.EqualTo(1.6f));
        Assert.That(MeleeRules.ReachFor(MeleeRules.Mode.None, false), Is.EqualTo(0f));
        string melee = Source("Scripts/Player/MeleeAttack.cs");
        StringAssert.Contains("TargetAnimalWithin(player, MeleeRules.ReachFor(mode, stab))", melee, "the prompt uses the blow's own raycast and reach");
    }

    [Test]
    public void ThePromptRaycastIsTheBlowsRaycastSoAWallAndTheReachDecideIt()
    {
        GameObject playerObject = Make("Test Player");
        Component player = playerObject.AddComponent(PlayerType);
        Transform eye = Make("Eye").transform;
        eye.SetParent(playerObject.transform, false);
        PlayerType.GetField("cameraTransform", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(player, eye);
        eye.position = new Vector3(0f, 1.6f, 0f);
        eye.rotation = Quaternion.identity;
        Component rabbit = NewAnimal("Rabbit", new Vector3(0f, 1.6f, 1.6f), "rabbit");
        Physics.SyncTransforms();
        Func<float, object> target = reach => MeleeType.GetMethod("TargetAnimalWithin").Invoke(null, new object[] { player, reach });
        Assert.That(target(MeleeRules.ReachFor(MeleeRules.Mode.Fists, false)), Is.SameAs(rabbit), "1.6 m: the near edge is 1.3 m, a fist reaches it");
        rabbit.transform.position = new Vector3(0f, 1.6f, 1.9f);
        Physics.SyncTransforms();
        Assert.That(target(MeleeRules.ReachFor(MeleeRules.Mode.Fists, false)), Is.Null, "1.9 m: out of a fist's reach, so no punch prompt");
        Assert.That(target(MeleeRules.ReachFor(MeleeRules.Mode.Knife, false)), Is.Null, "or a slash's");
        Assert.That(target(MeleeRules.ReachFor(MeleeRules.Mode.Knife, true)), Is.SameAs(rabbit), "but the next blow being a stab reaches it: the prompt follows the next blow's reach");
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        made.Add(wall);
        wall.transform.position = new Vector3(0f, 1.6f, 0.8f);
        wall.transform.localScale = new Vector3(3f, 3f, 0.1f);
        Physics.SyncTransforms();
        Assert.That(target(MeleeRules.ReachFor(MeleeRules.Mode.Knife, true)), Is.Null, "a wall in the way: no prompt");
    }

    [Test]
    public void NoPromptWhilePlacingOnTheCartKnockedDownOrInAMenuAndItIsAnExistingHudLine()
    {
        Assert.That(MeleeRules.PromptFor(false, MeleeRules.Mode.Fists, false, true, false, "Rabbit"), Is.Null, "CanUseTools false: placing, pushing the cart, knocked down, a menu");
        Assert.That(MeleeRules.PromptFor(false, MeleeRules.Mode.Knife, true, true, false, "Rabbit"), Is.Null);
        string melee = Source("Scripts/Player/MeleeAttack.cs");
        StringAssert.Contains("bool canUse = player.CanUseTools;", melee);
        StringAssert.Contains("canUse && mode != MeleeRules.Mode.None ? TargetAnimalWithin(", melee, "no raycast at all while gated");
        StringAssert.Contains("ToolStatus.Report(text);", melee, "the existing aimed-prompt path: no new HUD element");
        StringAssert.Contains("!PlacementMode.Active", Source("Scripts/Player/PlayerController.cs"), "CanUseTools has placing ahead of everything");
        StringAssert.DoesNotContain("ReportIdle", melee, "no permanent equipped line (M0d)");
    }

    // ---- The Know-how entry ----

    [Test]
    public void TheKnowHowEntryHasMikesTitleAndSaysWhatEachWeaponDoes()
    {
        Assert.That(KnowHowRules.TryGet(KnowHowRules.Melee, out KnowHowRules.Entry entry), Is.True);
        Assert.That(entry.id, Is.EqualTo("melee"));
        Assert.That(entry.title, Is.EqualTo("Fighting with your hands and a Knife"));
        StringAssert.Contains("Fists only stun and startle", entry.body);
        StringAssert.Contains("a Rabbit, Squirrel, Turkey or Duck", entry.body);
        StringAssert.Contains("A Knife kills small game", entry.body);
        StringAssert.Contains("a Deer is only startled, never hurt", entry.body);
        StringAssert.Contains("click to slash, then click again to stab", entry.body);
        StringAssert.Contains("alternate", entry.body);
        StringAssert.Contains("first click after a short pause is always a slash", entry.body);
        StringAssert.Contains("Carcass", entry.body);
        StringAssert.Contains("2 Stamina", entry.body);
        StringAssert.Contains("3", entry.body);
        Assert.That(entry.body.Length, Is.GreaterThan(100));
    }

    [Test]
    public void TheEntryOpensOnTheFirstPunchOrTheFirstKnifeInThePackWhicheverIsFirstAndOnlyOnce()
    {
        Assert.That(KnowHowRules.ForItem("knife"), Is.EqualTo(new[] { "melee" }));
        Assert.That(KnowHowRules.ForHappening(KnowHowRules.Happening.Punched), Is.EqualTo(new[] { "melee" }));
        List<string> byPunch = KnowHowRules.NewUnlocks(new string[0], KnowHowRules.ForHappening(KnowHowRules.Happening.Punched));
        Assert.That(byPunch, Is.EqualTo(new[] { "melee" }));
        Assert.That(KnowHowRules.NewUnlocks(byPunch, KnowHowRules.ForItem("knife")), Is.Empty, "punched first: the Knife adds nothing");
        List<string> byKnife = KnowHowRules.NewUnlocks(new string[0], KnowHowRules.ForItem("knife"));
        Assert.That(byKnife, Is.EqualTo(new[] { "melee" }));
        Assert.That(KnowHowRules.NewUnlocks(byKnife, KnowHowRules.ForHappening(KnowHowRules.Happening.Punched)), Is.Empty, "Knife first: the first punch adds nothing");
        Assert.That(KnowHowRules.ForItem("axe"), Is.Empty);
        string melee = Source("Scripts/Player/MeleeAttack.cs");
        int blow = melee.IndexOf("ToolEvents.RaisePunch(right, false);", StringComparison.Ordinal);
        int note = melee.IndexOf("NoteHappening(KnowHowRules.Happening.Punched)", StringComparison.Ordinal);
        int tired = melee.IndexOf("ToolEvents.RaisePunch(nextRight, true);", StringComparison.Ordinal);
        Assert.That(note, Is.GreaterThan(blow), "noted when a punch is thrown");
        Assert.That(note, Is.GreaterThan(tired), "and not on a tired refusal, which throws nothing");
        StringAssert.Contains("JournalManager.Instance.NoteHappening", melee);
    }

    [Test]
    public void NoSaveFieldIsAddedAndAnOldSaveLoadsWithTheEntryUnseenUntilItsTrigger()
    {
        // the journal saves only the ids of unlocked entries, in a list that already exists
        CollectionAssert.AreEquivalent(new[] { "id", "day", "read" }, Find("KnowHowUnlock").GetFields().Select(f => f.Name).ToArray(), "the unlock record has the same three fields");
        CollectionAssert.AreEquivalent(new[] { "entries", "nextId", "knowHow" }, Find("JournalSaveData").GetFields().Select(f => f.Name).ToArray(), "the journal save has no new field");
        // an old save: its know-how list has only older ids; JsonUtility reads it, and the new id is simply not in it
        string oldJson = "{\"entries\":[],\"nextId\":1,\"knowHow\":[{\"id\":\"ash\",\"day\":2,\"read\":true},{\"id\":\"tripod\",\"day\":3,\"read\":false}]}";
        object data = JsonUtility.FromJson(oldJson, Find("JournalSaveData"));
        var list = (System.Collections.IEnumerable)Find("JournalSaveData").GetField("knowHow").GetValue(data);
        var ids = list.Cast<object>().Select(o => (string)o.GetType().GetField("id").GetValue(o)).ToList();
        CollectionAssert.AreEqual(new[] { "ash", "tripod" }, ids);
        Assert.That(ids, Does.Not.Contain(KnowHowRules.Melee), "unseen in an old save");
        Assert.That(KnowHowRules.NewUnlocks(ids, KnowHowRules.ForItem("knife")), Is.EqualTo(new[] { "melee" }), "and it unlocks the first time its trigger happens");
        // an id this build does not know is still dropped on load, and a new save with the entry round-trips
        string newJson = "{\"entries\":[],\"nextId\":1,\"knowHow\":[{\"id\":\"melee\",\"day\":4,\"read\":false}]}";
        object loaded = JsonUtility.FromJson(newJson, Find("JournalSaveData"));
        var loadedIds = ((System.Collections.IEnumerable)Find("JournalSaveData").GetField("knowHow").GetValue(loaded)).Cast<object>().Select(o => (string)o.GetType().GetField("id").GetValue(o));
        Assert.That(loadedIds, Is.EqualTo(new[] { "melee" }));
        Assert.That(KnowHowRules.TryGet("melee", out _), Is.True, "so RestoreState keeps it");
    }

    [Test]
    public void AnOldSaveThatCarriesAKnifeIsBackfilledLikeEveryOtherItemEntry()
    {
        Assert.That(KnowHowRules.Backfill(id => id == "knife", false, false, false), Is.EqualTo(new[] { "melee" }), "the existing backfill: a loaded game that holds the trigger item gets its entry");
        Assert.That(KnowHowRules.Backfill(id => false, false, false, false), Is.Empty, "nothing carried, nothing added");
        Assert.That(KnowHowRules.Backfill(id => id == "knife" || id == "ash", false, false, false), Is.EqualTo(new[] { "ash", "potash", "melee" }));
    }

    // ---- One small-game list ----

    [Test]
    public void TheFistsAndTheKnifeShareOneSmallGameListOfFourSpeciesAndTheDeerIsNotInIt()
    {
        CollectionAssert.AreEqual(new[] { "rabbit", "squirrel", "turkey", "waterfowl" }, MeleeRules.SmallGameIds);
        foreach (string id in new[] { "rabbit", "squirrel", "turkey", "waterfowl", "deer", null, "", "fox" })
        {
            bool expected = id != null && Array.IndexOf(MeleeRules.SmallGameIds, id) >= 0;
            Assert.That(MeleeRules.IsSmallGame(id), Is.EqualTo(expected), id ?? "null");
            Assert.That(PunchRules.IsSmallGame(id), Is.EqualTo(expected), "fists: " + id);
            Assert.That(KnifeRules.IsSmallGame(id), Is.EqualTo(expected), "knife: " + id);
        }
        Assert.That(MeleeRules.IsSmallGame("deer"), Is.False);
        StringAssert.DoesNotContain("SmallGameIds =", Source("Scripts/Economy/PunchRules.cs"), "not duplicated");
        StringAssert.DoesNotContain("SmallGameIds =", Source("Scripts/Economy/KnifeRules.cs"), "not duplicated");
        StringAssert.Contains("PunchRules.IsSmallGame(animal.SpeciesId)", Source("Scripts/Player/MeleeAttack.cs"), "the fists key on the species id, not the tier");
        StringAssert.DoesNotContain("GameTier.Small", Source("Scripts/Player/MeleeAttack.cs").Replace("// ", "//"), "no tier check is left in MeleeAttack");
    }

    Component NewAnimal(string name, Vector3 at, string species, string tier = "Medium")
    {
        GameObject go = Make(name);
        go.transform.position = at;
        go.AddComponent<SphereCollider>().radius = 0.3f;
        Component animal = go.AddComponent(AnimalType);
        AnimalType.GetField("tier", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(animal, Enum.Parse(Find("GameTier"), tier));
        AnimalType.GetField("speciesId", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(animal, species);
        return animal;
    }

    static object State(Component animal) => AnimalType.GetField("state", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(animal);
    static float StunUntil(Component animal) => (float)AnimalType.GetField("stateUntil", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(animal);
    static void Fist(Component animal) => MeleeType.GetMethod("Hit").Invoke(null, new object[] { animal, Vector3.zero });

    [Test]
    public void AFistStunsATurkeyAndADuckForAboutASecondAndNeverKillsOrWoundsThem()
    {
        foreach (string species in new[] { "turkey", "waterfowl" })
            foreach (bool wounded in new[] { false, true })
            {
                Component bird = NewAnimal(species + wounded, Vector3.zero, species, "Medium"); // tier Medium, as in the prefabs
                AnimalType.GetField("wounded", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(bird, wounded);
                Fist(bird);
                Assert.That(State(bird).ToString(), Is.EqualTo("Stunned"), species + " wounded=" + wounded);
                Assert.That(StunUntil(bird) - Time.time, Is.EqualTo(PunchRules.StunSeconds).Within(0.05f), "about a second");
                Assert.That((bool)AnimalType.GetProperty("IsDead").GetValue(bird), Is.False, "never killed");
                Assert.That(bird.GetComponent(Find("Carcass")), Is.Null, "no carcass");
                Assert.That(AnimalType.GetField("wounded", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(bird), Is.EqualTo(wounded), "no wound added or taken away");
            }
    }

    [Test]
    public void AFistStillOnlyStartlesADeerAndLeavesTheDeadAloneAndRabbitsAndSquirrelsStillStun()
    {
        foreach (string tier in new[] { "Medium", "Large" })
        {
            Component deer = NewAnimal("Deer" + tier, Vector3.zero, "deer", tier);
            Fist(deer);
            Assert.That(State(deer).ToString(), Is.EqualTo("Flee"), tier + ": startled only");
            Assert.That((bool)AnimalType.GetProperty("IsDead").GetValue(deer), Is.False);
        }
        foreach (string species in new[] { "rabbit", "squirrel" })
        {
            Component small = NewAnimal(species, Vector3.zero, species, "Small");
            Fist(small);
            Assert.That(State(small).ToString(), Is.EqualTo("Stunned"), species);
        }
        Component dead = NewAnimal("Dead", Vector3.zero, "turkey");
        AnimalType.GetField("state", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(dead, Enum.Parse(AnimalType.GetNestedType("State", BindingFlags.NonPublic), "Dead"));
        Fist(dead);
        Assert.That(State(dead).ToString(), Is.EqualTo("Dead"), "a dead Turkey is left exactly as it was");
        Assert.That(PunchRules.ForAnimal(false, true), Is.EqualTo(PunchRules.Outcome.StunAndStartle));
        Assert.That(PunchRules.ForAnimal(false, false), Is.EqualTo(PunchRules.Outcome.StartleOnly));
        Assert.That(PunchRules.ForAnimal(true, true), Is.EqualTo(PunchRules.Outcome.Nothing));
        Assert.That(PunchRules.StunSeconds, Is.EqualTo(1f), "the fists' numbers are untouched");
        Assert.That(PunchRules.StaminaPerPunch, Is.EqualTo(2f));
        Assert.That(PunchRules.ReachMetres, Is.EqualTo(1.4f));
        Assert.That(PunchRules.CycleSeconds, Is.EqualTo(0.6f));
    }

    // ---- The Knife's sound ----

    [Test]
    public void TheKnifeKeepsItemDropAtTheClickAddsAThudOnAHitAndTheFistsKeepTheirSoundAndNoCueOrFileIsAdded()
    {
        string melee = Source("Scripts/Player/MeleeAttack.cs");
        int knifeStrike = melee.IndexOf("public static void KnifeStrike(", StringComparison.Ordinal);
        int hitWithKnife = melee.IndexOf("public static void HitWithKnife(", StringComparison.Ordinal);
        int fistStrike = melee.IndexOf("public static void Strike(PlayerController player)", StringComparison.Ordinal);
        string knifeBlow = melee.Substring(knifeStrike, hitWithKnife - knifeStrike);
        string knifeHit = melee.Substring(hitWithKnife, melee.IndexOf("// The animal the aim raycast reaches first within", hitWithKnife, StringComparison.Ordinal) - hitWithKnife);
        string fists = melee.Substring(fistStrike);
        StringAssert.Contains("AudioManager.Instance.Play(SoundCue.ItemDrop)", knifeBlow, "the blow at the click: ItemDrop (no swish exists)");
        StringAssert.DoesNotContain("PlayMeleeHit", knifeBlow, "the hit cue is not played at a miss");
        StringAssert.Contains("outcome != KnifeRules.Outcome.Nothing", knifeHit);
        StringAssert.Contains("AudioManager.Instance.PlayMeleeHit(animal.transform.position)", knifeHit, "a hit on a live animal: the thud");
        StringAssert.Contains("AudioManager.Instance.Play(SoundCue.ItemDrop)", fists, "the fists keep their sound");
        StringAssert.DoesNotContain("PlayMeleeHit", fists.Substring(0, fists.IndexOf("public static Animal TargetAnimal(", StringComparison.Ordinal)), "and get no hit cue");
        Assert.That(Enum.GetNames(Find("SoundCue")).Length, Is.EqualTo(14), "no SoundCue was added");
        string audio = Source("Scripts/Managers/AudioManager.cs");
        int play = audio.IndexOf("public void PlayMeleeHit(", StringComparison.Ordinal);
        string body = audio.Substring(play, 700);
        StringAssert.Contains("bowRelease.clip == null", body, "no recording, no sound: the placeholder twang is not a thud");
        StringAssert.Contains("clip = bowRelease.clip", body, "the existing arrow-strike recording, no new file");
        StringAssert.Contains("startTime = bowRelease.startTime", body);
        StringAssert.Contains("maxDuration = meleeHitSeconds", body);
        string[] audioFiles = Directory.GetFiles(Application.dataPath, "*.wav", SearchOption.AllDirectories).Concat(Directory.GetFiles(Application.dataPath, "*.mp3", SearchOption.AllDirectories)).Concat(Directory.GetFiles(Application.dataPath, "*.ogg", SearchOption.AllDirectories)).ToArray();
        Assert.That(audioFiles.Any(f => Path.GetFileName(f).ToLowerInvariant().Contains("knife") || Path.GetFileName(f).ToLowerInvariant().Contains("melee") || Path.GetFileName(f).ToLowerInvariant().Contains("slash")), Is.False, "no audio file for melee was added");
    }

    [Test]
    public void ABlowAndAHitStillStartleEverythingNearbyThroughTheExistingAlertAtTheSameRadii()
    {
        string melee = Source("Scripts/Player/MeleeAttack.cs");
        StringAssert.Contains("WildlifeManager.Instance.Alert(from, PunchRules.AlertRadius);", melee);
        StringAssert.Contains("WildlifeManager.Instance.Alert(from, KnifeRules.AlertRadius);", melee);
        Assert.That(PunchRules.AlertRadius, Is.EqualTo(8f));
        Assert.That(KnifeRules.AlertRadius, Is.EqualTo(6f));
        string wildlife = Source("Scripts/Managers/WildlifeManager.cs");
        int alert = wildlife.IndexOf("public void Alert(", StringComparison.Ordinal);
        Assert.That(alert, Is.GreaterThan(0));
        StringAssert.Contains("Startle(", wildlife.Substring(alert, 600), "Alert startles the animals inside the radius");
        Assert.That(KnifeRules.StaminaPerSlash, Is.EqualTo(3f));
        Assert.That(KnifeRules.StaminaPerStab, Is.EqualTo(3f));
        Assert.That(KnifeRules.SlashReachMetres, Is.EqualTo(1.5f));
        Assert.That(KnifeRules.StabReachMetres, Is.EqualTo(1.6f));
        Assert.That(KnifeRules.SlashCycleSeconds, Is.EqualTo(0.7f));
        Assert.That(KnifeRules.StabCycleSeconds, Is.EqualTo(0.8f), "the Knife's stamina, reach and cadence are unchanged");
    }
}
