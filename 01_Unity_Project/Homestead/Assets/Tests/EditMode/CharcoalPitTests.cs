using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

// EditMode tests for Metalworking phase M1 (Mike, 2026-11-04): Iron Ore and Charcoal, the market rows, and the Charcoal Pit. MetalRules and CharcoalRules are pure (the burn, the yield
// table, the cap, the gates and every word); the main-assembly parts (the saved pile, the staged site, the kinds, the visuals) are reached by reflection as the other world tests do.
// Nothing here touches the Saves folder or a scene.
public class CharcoalPitTests
{
    static Type Find(string name) =>
        AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => { try { return a.GetTypes(); } catch (Exception) { return new Type[0]; } })
                 .First(t => t.Name == name);

    static object Kind(string name) => Enum.Parse(Find("PileKind"), name);

    static string SourceText(string relative) => File.ReadAllText(Path.Combine(Application.dataPath, relative)).Replace("\r\n", "\n");

    static object NewPile(string kind)
    {
        object state = Activator.CreateInstance(Find("WoodPileState"));
        Find("WoodPileState").GetField("kind").SetValue(state, Kind(kind));
        return state;
    }

    static void Add(object pile, string id, int count) => Find("WoodPileState").GetMethod("Add").Invoke(pile, new object[] { id, count, 0 });

    static int Count(object pile, string id) => (int)Find("WoodPileState").GetMethod("Count").Invoke(pile, new object[] { id });

    static object Prop(object pile, string name) => Find("WoodPileState").GetProperty(name).GetValue(pile);

    static bool Accepts(object pile, string id) => (bool)Find("WoodPileState").GetMethod("Accepts").Invoke(pile, new object[] { id });

    static object Wells(string method, params object[] args) =>
        Find("WellsAndOuthouses").GetMethod(method, BindingFlags.Public | BindingFlags.Static).Invoke(null, args);

    static void Set(object pile, string field, object value) => Find("WoodPileState").GetField(field).SetValue(pile, value);

    static object Get(object pile, string field) => Find("WoodPileState").GetField(field).GetValue(pile);

    // --- MetalRules: a table keyed by metal ---

    [Test]
    public void TheMetalTableHasIronOnlyAndIsKeyedSoMoreMetalsAreJustRows()
    {
        Assert.That(MetalRules.Metals.Length, Is.EqualTo(1));
        Assert.That(MetalRules.TryFind("iron", out MetalRules.Metal iron), Is.True);
        Assert.That(iron.name, Is.EqualTo("Iron"));
        Assert.That(iron.oreId, Is.EqualTo("iron_ore"));
        Assert.That(iron.oreName, Is.EqualTo("Iron Ore"));
        Assert.That(iron.fuelId, Is.EqualTo("charcoal"));
        Assert.That(iron.oreBaseCents, Is.EqualTo(400L));
        Assert.That(iron.oreKg, Is.EqualTo(3f));
        Assert.That(MetalRules.TryFind("copper", out _), Is.False, "copper and tin are later rows");
        Assert.That(MetalRules.TryFindByOre("iron_ore", out MetalRules.Metal byOre), Is.True);
        Assert.That(byOre.id, Is.EqualTo("iron"));
        Assert.That(MetalRules.IsOre("iron_ore"), Is.True);
        Assert.That(MetalRules.IsOre("charcoal"), Is.False);
        Assert.That(MetalRules.OreIds, Is.EqualTo(new[] { "iron_ore" }));
        Assert.That(MetalRules.Metals.Select(m => m.id).Distinct().Count(), Is.EqualTo(MetalRules.Metals.Length), "ids are unique");
        Assert.That(MetalRules.Metals.Select(m => m.oreId).Distinct().Count(), Is.EqualTo(MetalRules.Metals.Length), "ore ids are unique");
        Assert.That(CharcoalRules.CharcoalId, Is.EqualTo(MetalRules.CharcoalId), "one Charcoal id, shared by every metal's fuel column");
    }

    // --- The Pit's numbers ---

    [Test]
    public void ThePitNumbersAreMikes()
    {
        Assert.That(CharcoalRules.SiteClay, Is.EqualTo(4));
        Assert.That(CharcoalRules.SiteTallGrass, Is.EqualTo(6));
        Assert.That(CharcoalRules.BuildBlows, Is.EqualTo(20));
        Assert.That(CharcoalRules.LogsCap, Is.EqualTo(6));
        Assert.That(CharcoalRules.LogsToLight, Is.EqualTo(3));
        Assert.That(CharcoalRules.BurnHours, Is.EqualTo(24));
        Assert.That(CharcoalRules.ClearanceMetres, Is.EqualTo(PotteryRules.KilnClearanceMetres), "the Kiln's 3 m");
    }

    [Test]
    public void TheYieldIsTwoThirdsOfTheLogsRoundedDown()
    {
        Assert.That(CharcoalRules.Yield(3), Is.EqualTo(2));
        Assert.That(CharcoalRules.Yield(4), Is.EqualTo(2));
        Assert.That(CharcoalRules.Yield(5), Is.EqualTo(3));
        Assert.That(CharcoalRules.Yield(6), Is.EqualTo(4));
        Assert.That(CharcoalRules.Yield(2), Is.EqualTo(0), "too few to light");
        Assert.That(CharcoalRules.Yield(0), Is.EqualTo(0));
        Assert.That(CharcoalRules.Yield(9), Is.EqualTo(4), "the cap of 6 holds even if more were somehow loaded");
    }

    [Test]
    public void ThePhasesFollowTheLogsTheBurnAndTheCharcoalWaiting()
    {
        Assert.That(CharcoalRules.PhaseOf(false, 0, 0), Is.EqualTo(CharcoalRules.Phase.Empty));
        Assert.That(CharcoalRules.PhaseOf(false, 0, 1), Is.EqualTo(CharcoalRules.Phase.Loading));
        Assert.That(CharcoalRules.PhaseOf(false, 0, 2), Is.EqualTo(CharcoalRules.Phase.Loading));
        Assert.That(CharcoalRules.PhaseOf(false, 0, 3), Is.EqualTo(CharcoalRules.Phase.Ready));
        Assert.That(CharcoalRules.PhaseOf(false, 0, 6), Is.EqualTo(CharcoalRules.Phase.Ready));
        Assert.That(CharcoalRules.PhaseOf(true, 0, 0), Is.EqualTo(CharcoalRules.Phase.Burning));
        Assert.That(CharcoalRules.PhaseOf(false, 4, 0), Is.EqualTo(CharcoalRules.Phase.Done));
        Assert.That(CharcoalRules.CanLight(CharcoalRules.Phase.Ready), Is.True);
        Assert.That(CharcoalRules.CanLight(CharcoalRules.Phase.Loading), Is.False, "E lights it only with at least 3 Logs");
        Assert.That(CharcoalRules.CanDismantle(CharcoalRules.Phase.Burning), Is.False);
        Assert.That(CharcoalRules.CanDismantle(CharcoalRules.Phase.Done), Is.True);
        Assert.That(CharcoalRules.CanDismantle(CharcoalRules.Phase.Ready), Is.True, "Logs loaded come back whole");
    }

    [Test]
    public void LoadingTakesWhatIsCarriedUpToTheRoomAndNeverPastSix()
    {
        Assert.That(CharcoalRules.RoomFor(0), Is.EqualTo(6));
        Assert.That(CharcoalRules.RoomFor(4), Is.EqualTo(2));
        Assert.That(CharcoalRules.RoomFor(6), Is.EqualTo(0));
        Assert.That(CharcoalRules.LoadAmount(0, 3), Is.EqualTo(3));
        Assert.That(CharcoalRules.LoadAmount(0, 5), Is.EqualTo(5));
        Assert.That(CharcoalRules.LoadAmount(0, 9), Is.EqualTo(6), "take what is carried, up to room");
        Assert.That(CharcoalRules.LoadAmount(4, 5), Is.EqualTo(2));
        Assert.That(CharcoalRules.LoadAmount(6, 5), Is.EqualTo(0));
        Assert.That(CharcoalRules.LoadAmount(2, 0), Is.EqualTo(0));

        Assert.That(CharcoalRules.LoadGate(CharcoalRules.Phase.Empty, 0, 4), Is.EqualTo(CharcoalRules.Gate.Ok));
        Assert.That(CharcoalRules.LoadGate(CharcoalRules.Phase.Loading, 2, 0), Is.EqualTo(CharcoalRules.Gate.NothingCarried));
        Assert.That(CharcoalRules.LoadGate(CharcoalRules.Phase.Ready, 6, 3), Is.EqualTo(CharcoalRules.Gate.Full));
        Assert.That(CharcoalRules.LoadGate(CharcoalRules.Phase.Burning, 0, 3), Is.EqualTo(CharcoalRules.Gate.Burning));
        Assert.That(CharcoalRules.LoadGate(CharcoalRules.Phase.Done, 0, 3), Is.EqualTo(CharcoalRules.Gate.Waiting));
    }

    [Test]
    public void TheBurnCountsDownOneGameHourAtATimeAndEndsOnTheTwentyFourth()
    {
        float hours = CharcoalRules.BurnHours;
        int ticks = 0;
        while (!CharcoalRules.IsDone(hours))
        {
            hours = CharcoalRules.AfterHour(hours);
            ticks++;
        }
        Assert.That(ticks, Is.EqualTo(24));
        Assert.That(CharcoalRules.AfterHour(0f), Is.EqualTo(0f), "an idle pit does not go below zero");
        Assert.That(CharcoalRules.IsDone(1f), Is.False);
    }

    // --- The words ---

    [Test]
    public void ThePromptsSayWhatALoadWillMake()
    {
        Assert.That(CharcoalRules.Prompt(CharcoalRules.Phase.Ready, 6, 4, 0f), Is.EqualTo("Light the pit  (6 Logs -> 4 Charcoal in 24 h)"));
        Assert.That(CharcoalRules.Prompt(CharcoalRules.Phase.Ready, 3, 2, 0f), Is.EqualTo("Light the pit  (3 Logs -> 2 Charcoal in 24 h)"));
        Assert.That(CharcoalRules.Prompt(CharcoalRules.Phase.Empty, 0, 0, 0f), Is.EqualTo("Charcoal Pit  (empty, holds up to 6 Logs)"));
        Assert.That(CharcoalRules.Prompt(CharcoalRules.Phase.Loading, 2, 0, 0f), Is.EqualTo("Charcoal Pit  (2 Logs in, 3 needed to light)"));
        Assert.That(CharcoalRules.Prompt(CharcoalRules.Phase.Loading, 1, 0, 0f), Is.EqualTo("Charcoal Pit  (1 Log in, 3 needed to light)"));
        Assert.That(CharcoalRules.Prompt(CharcoalRules.Phase.Burning, 0, 4, 17.2f), Is.EqualTo("Charcoal Pit  (burning, 18 h to go, 4 Charcoal)"));
        Assert.That(CharcoalRules.Prompt(CharcoalRules.Phase.Done, 0, 4, 0f), Is.EqualTo("Collect Charcoal  (4)"));
    }

    [Test]
    public void TheLoadAndDismantleLinesAndTheMessages()
    {
        Assert.That(CharcoalRules.LoadPrompt(CharcoalRules.Gate.Ok, 5), Is.EqualTo("Load 5 Logs"));
        Assert.That(CharcoalRules.LoadPrompt(CharcoalRules.Gate.Ok, 1), Is.EqualTo("Load 1 Log"));
        Assert.That(CharcoalRules.LoadPrompt(CharcoalRules.Gate.NothingCarried, 0), Is.EqualTo("Load Logs  (you're not carrying any)"));
        Assert.That(CharcoalRules.LoadPrompt(CharcoalRules.Gate.Full, 0), Is.EqualTo("The pit is full  (6 Logs)"));
        Assert.That(CharcoalRules.LoadPrompt(CharcoalRules.Gate.Burning, 0), Is.EqualTo("Can't load the pit while it burns"));
        Assert.That(CharcoalRules.LoadPrompt(CharcoalRules.Gate.Waiting, 0), Is.EqualTo("Collect the Charcoal before loading again"));
        Assert.That(CharcoalRules.DismantlePrompt(CharcoalRules.Phase.Burning, 9f), Is.EqualTo("Can't take down the pit while it burns  (9 h to go)"));
        Assert.That(CharcoalRules.DismantlePrompt(CharcoalRules.Phase.Ready, 0f), Does.StartWith("Take down the pit").And.Contain("come back whole"));
        Assert.That(CharcoalRules.Loaded(4, 4), Is.EqualTo("Loaded 4 Logs  (the pit holds 4 of 6; lighting it makes 2 Charcoal)"));
        Assert.That(CharcoalRules.NotReady(2), Is.EqualTo("Needs 1 more Log to light (it holds 2)."));
        Assert.That(CharcoalRules.NotReady(0), Is.EqualTo("Load at least 3 Logs first (R)."));
        Assert.That(CharcoalRules.Started(6, 4), Is.EqualTo("The pit is lit: 6 Logs will burn for 24 hours and make 4 Charcoal."));
        Assert.That(CharcoalRules.Took(4, 0), Is.EqualTo("Took 4 Charcoal."));
        Assert.That(CharcoalRules.Took(3, 1), Is.EqualTo("Took 3 Charcoal (1 left in the pit, no room)."));
        Assert.That(CharcoalRules.Took(0, 4), Is.EqualTo("Too heavy - no room to carry it"));
        Assert.That(CharcoalRules.SiteHelp, Is.EqualTo("Deliver 4 Clay and 6 Tall Grass"));
        Assert.That(CharcoalRules.BuiltMessage, Does.Contain("press R to load Logs (up to 6)").And.Contain("then E to light it"));
    }

    // --- The kinds: appended after the Kiln, nothing moved ---

    [Test]
    public void TheTwoPitKindsAreThirtyNineAndFortyAndNothingElseWasAdded()
    {
        Type kind = Find("PileKind");
        Assert.That(Enum.GetValues(kind).Length, Is.EqualTo(45), "39 kinds before the Pit, plus the Pit's two, the Bloomery's two (41, 42) and the Smithy's two (43, 44)");
        Assert.That((int)Enum.Parse(kind, "Kiln"), Is.EqualTo(38), "nothing moved");
        Assert.That((int)Enum.Parse(kind, "CharcoalPitSite"), Is.EqualTo(39));
        Assert.That((int)Enum.Parse(kind, "CharcoalPit"), Is.EqualTo(40));
        string[] names = Enum.GetNames(kind);
        Assert.That((int)Enum.Parse(kind, "Bloomery"), Is.EqualTo(42), "the Bloomery came next (M2)");
        Assert.That((int)Enum.Parse(kind, "Smithy"), Is.EqualTo(44), "the Smithy came last (M3)");
        Assert.That(names.Any(n => n.Contains("Forge") || n.Contains("Anvil")), Is.False, "no separate Forge or Anvil kinds: one combined Smithy");
    }

    // --- The staged site and the finished pit ---

    [Test]
    public void TheSiteIsAStagedSiteOfFourClayAndSixTallGrassThatBecomesThePit()
    {
        Assert.That((bool)Wells("IsStagedSite", Kind("CharcoalPitSite")), Is.True);
        Assert.That((bool)Wells("IsStagedSite", Kind("CharcoalPit")), Is.False);
        Assert.That(Wells("FinishedKind", Kind("CharcoalPitSite")).ToString(), Is.EqualTo("CharcoalPit"));
        Assert.That((bool)Wells("RequiresDig", Kind("CharcoalPitSite")), Is.False, "RequiresDig is false: no Shovel stage, the Hammer builds it");

        var cost = ((IEnumerable)Wells("FinishedCost", Kind("CharcoalPitSite"))).Cast<object>()
                   .Select(c => (c.GetType().GetField("itemId").GetValue(c) as string, (int)c.GetType().GetField("count").GetValue(c))).ToArray();
        Assert.That(cost, Is.EqualTo(new[] { ("clay", 4), ("tall_grass", 6) }));

        var order = (string[][])Wells("DeliveryOrder", Kind("CharcoalPitSite"));
        Assert.That(order.Length, Is.EqualTo(1), "one group: the Clay and the Tall Grass in any order");
        Assert.That(order[0], Is.EquivalentTo(new[] { "clay", "tall_grass" }));

        object site = NewPile("CharcoalPitSite");
        Assert.That(Accepts(site, "clay"), Is.True);
        Assert.That(Accepts(site, "tall_grass"), Is.True);
        Assert.That(Accepts(site, "logs"), Is.False, "the site takes only its Clay and Tall Grass");
        for (int i = 0; i < 4; i++)
            Add(site, "clay", 1);
        Assert.That(Accepts(site, "clay"), Is.False, "full of Clay");
        Assert.That(Accepts(site, "tall_grass"), Is.True);
        Assert.That(Wells("SiteNeeds", site), Is.EqualTo("6 Tall Grass"));
        for (int i = 0; i < 6; i++)
            Add(site, "tall_grass", 1);
        Assert.That(Wells("SiteNeeds", site), Is.Null, "fully stocked: the Hammer finishes it");
    }

    [Test]
    public void TheSiteTakesTwentyHammerBlowsAndIsInTheBuildListButThePitIsNot()
    {
        Assert.That(CharcoalRules.BuildBlows, Is.EqualTo(20));
        var buildable = ((IEnumerable)Find("WoodManager").GetField("Buildable").GetValue(null)).Cast<object>().Select(o => o.ToString()).ToArray();
        Assert.That(buildable, Does.Contain("CharcoalPitSite"));
        Assert.That(buildable, Does.Not.Contain("CharcoalPit"));
        Type wood = Find("WoodManager");
        Assert.That(wood.GetMethod("PileName").Invoke(null, new[] { Kind("CharcoalPitSite") }), Is.EqualTo("Charcoal Pit Site"));
        Assert.That(wood.GetMethod("PileName").Invoke(null, new[] { Kind("CharcoalPit") }), Is.EqualTo("Charcoal Pit"));
        Assert.That(Wells("FinishedMessage", Kind("CharcoalPitSite")), Is.EqualTo(CharcoalRules.BuiltMessage));
        Assert.That(Prop(NewPile("CharcoalPit"), "IsPlacedStructure"), Is.EqualTo(true), "drawn and handled by PlacedStructure");
        Assert.That(Prop(NewPile("CharcoalPitSite"), "IsPlacedStructure"), Is.EqualTo(true));
    }

    [Test]
    public void ThePitTakesLogsUpToSixAndNothingElseAndNotWhileItBurnsOrCharcoalWaits()
    {
        object pit = NewPile("CharcoalPit");
        Assert.That(Accepts(pit, "logs"), Is.True);
        Assert.That(Accepts(pit, "branches"), Is.False);
        Assert.That(Accepts(pit, "firewood"), Is.False, "Logs only");
        Assert.That(Accepts(pit, "charcoal"), Is.False, "its Charcoal is not something to put back");
        for (int i = 0; i < 6; i++)
        {
            Assert.That(Accepts(pit, "logs"), Is.True, $"room for log {i + 1}");
            Add(pit, "logs", 1);
        }
        Assert.That(Accepts(pit, "logs"), Is.False, "the cap of 6");

        object burning = NewPile("CharcoalPit");
        Set(burning, "jobItem", CharcoalRules.BurnMarker);
        Assert.That(Prop(burning, "PitBurning"), Is.EqualTo(true));
        Assert.That(Accepts(burning, "logs"), Is.False, "not while it burns");

        object waiting = NewPile("CharcoalPit");
        Add(waiting, "charcoal", 2);
        Assert.That(Prop(waiting, "PitOpen"), Is.EqualTo(false));
        Assert.That(Accepts(waiting, "logs"), Is.False, "not while Charcoal waits to be collected");
    }

    [Test]
    public void ABurnFinishesIntoCharcoalExactlyAfterTwentyFourHoursAndClearsTheJob()
    {
        object pit = NewPile("CharcoalPit");
        Add(pit, "logs", 5);
        Assert.That(Wells("PitPhaseOf", pit), Is.EqualTo(CharcoalRules.Phase.Ready));

        // What PlacedStructure.PitLight does: the Logs are used up, the yield is fixed, the clock starts.
        Find("WoodPileState").GetMethod("Take").Invoke(pit, new object[] { "logs", 5 });
        Set(pit, "jobItem", CharcoalRules.BurnMarker);
        Set(pit, "jobCount", CharcoalRules.Yield(5));
        Set(pit, "jobHours", (float)CharcoalRules.BurnHours);
        Assert.That(Wells("PitPhaseOf", pit), Is.EqualTo(CharcoalRules.Phase.Burning));
        Assert.That(Count(pit, "logs"), Is.EqualTo(0), "the Logs are used up when it is lit");

        for (int hour = 1; hour <= 23; hour++)
        {
            Set(pit, "jobHours", CharcoalRules.AfterHour((float)Get(pit, "jobHours")));
            Assert.That(CharcoalRules.IsDone((float)Get(pit, "jobHours")), Is.False, $"after {hour} h");
        }
        Set(pit, "jobHours", CharcoalRules.AfterHour((float)Get(pit, "jobHours")));
        Assert.That(CharcoalRules.IsDone((float)Get(pit, "jobHours")), Is.True, "done on the 24th");

        Wells("FinishBurn", pit);
        Assert.That(Count(pit, "charcoal"), Is.EqualTo(3), "5 Logs make 3 Charcoal");
        Assert.That(Get(pit, "jobItem"), Is.EqualTo(""));
        Assert.That((float)Get(pit, "jobHours"), Is.EqualTo(0f));
        Assert.That((int)Get(pit, "jobCount"), Is.EqualTo(0));
        Assert.That(Wells("PitPhaseOf", pit), Is.EqualTo(CharcoalRules.Phase.Done));
        Wells("FinishBurn", pit);
        Assert.That(Count(pit, "charcoal"), Is.EqualTo(3), "a second call adds nothing");
    }

    [Test]
    public void EveryYieldComesOutOfAFinishedBurn()
    {
        foreach (int logs in new[] { 3, 4, 5, 6 })
        {
            object pit = NewPile("CharcoalPit");
            Set(pit, "jobItem", CharcoalRules.BurnMarker);
            Set(pit, "jobCount", CharcoalRules.Yield(logs));
            Set(pit, "jobHours", 1f);
            Wells("FinishBurn", pit);
            Assert.That(Count(pit, "charcoal"), Is.EqualTo(new Dictionary<int, int> { { 3, 2 }, { 4, 2 }, { 5, 3 }, { 6, 4 } }[logs]), $"{logs} Logs");
        }
    }

    // --- Saving: no new field; a burn survives a save and a load ---

    [Test]
    public void ThePitUsesTheExistingFieldsAndTheSavedPileGainedNone()
    {
        string[] fields = Find("WoodPileState").GetFields(BindingFlags.Public | BindingFlags.Instance).Select(f => f.Name).OrderBy(n => n).ToArray();
        Assert.That(fields.Length, Is.EqualTo(17), "the Hopper's two were the last additions");
        Assert.That(fields, Does.Contain("jobItem").And.Contain("jobHours").And.Contain("jobCount").And.Contain("contents"));
        Assert.That(fields, Does.Contain("hopperWater").And.Contain("hopperBatch"), "the Hopper's fields are untouched");
        Assert.That(fields.Any(f => f.ToLowerInvariant().Contains("pit") || f.ToLowerInvariant().Contains("charcoal")), Is.False);
        FieldInfo version = Find("SaveManager").GetField("SaveVersion", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        Assert.That(version.GetRawConstantValue(), Is.EqualTo(1));
        Assert.That(LedgerSaveData.CurrentVersion, Is.EqualTo(1));
    }

    [Test]
    public void ABurnInProgressSurvivesASaveAndALoad()
    {
        object pit = NewPile("CharcoalPit");
        Set(pit, "jobItem", CharcoalRules.BurnMarker);
        Set(pit, "jobCount", 4);
        Set(pit, "jobHours", 13f);
        Add(pit, "charcoal", 0);
        string json = JsonUtility.ToJson(pit);
        object back = JsonUtility.FromJson(json, Find("WoodPileState"));
        Assert.That(Get(back, "kind").ToString(), Is.EqualTo("CharcoalPit"));
        Assert.That(Prop(back, "PitBurning"), Is.EqualTo(true));
        Assert.That((int)Get(back, "jobCount"), Is.EqualTo(4));
        Assert.That((float)Get(back, "jobHours"), Is.EqualTo(13f));
        Assert.That(Wells("PitPhaseOf", back), Is.EqualTo(CharcoalRules.Phase.Burning));
        Assert.That(json, Does.Contain("\"kind\":40"));
    }

    [Test]
    public void AnOldSaveWithNoPitKindsLoadsAsBeforeAndAKilnIsNeverReadAsAPit()
    {
        // A pile saved before the Pit existed: a Kiln mid-firing, with its own marker.
        object kiln = NewPile("Kiln");
        Set(kiln, "jobItem", PotteryRules.FiringMarker);
        Set(kiln, "jobHours", 9f);
        Add(kiln, "dry_clay_cup", 3);
        object back = JsonUtility.FromJson(JsonUtility.ToJson(kiln), Find("WoodPileState"));
        Assert.That(Get(back, "kind").ToString(), Is.EqualTo("Kiln"));
        Assert.That(Prop(back, "KilnFiring"), Is.EqualTo(true));
        Assert.That(Prop(back, "PitBurning"), Is.EqualTo(false), "the Kiln's firing marker is not the Pit's burn marker");
        Assert.That(PotteryRules.FiringMarker, Is.Not.EqualTo(CharcoalRules.BurnMarker));

        // And a save JSON with none of the Pit's data at all (every older pile) loads with the fields at their defaults.
        object plain = JsonUtility.FromJson("{\"id\":3,\"kind\":1,\"contents\":[]}", Find("WoodPileState"));
        Assert.That(Prop(plain, "PitBurning"), Is.EqualTo(false));
        Assert.That(Prop(plain, "PitLogs"), Is.EqualTo(0));
    }

    // --- The market rows, by hand in the asset ---

    [Test]
    public void IronOreIsTheTwentySecondStockedItemAtFourDollarsAndSellsBackForOne()
    {
        MarketTable table = MarketCatalog.Table;
        Assert.That(table.Stock.Count, Is.EqualTo(22), "21 before Iron Ore");
        Assert.That(table.TryGetRow("iron_ore", out MarketRow row), Is.True);
        Assert.That(row.kind, Is.EqualTo(MarketKind.Stocked));
        Assert.That(row.group, Is.EqualTo(MarketGroup.Stocked));
        Assert.That(row.baseCents, Is.EqualTo(400L));
        Assert.That(table.BuyBaseCents("iron_ore"), Is.EqualTo(400L));
        Assert.That(table.SellBaseCents("iron_ore"), Is.EqualTo(100L), "sells back at 40%: $1, less than it costs");
        Assert.That(table.Stock[table.Stock.Count - 1].itemId, Is.EqualTo("iron_ore"), "it joins the shelf after the Wood Saw");
        Assert.That(table.Stock[table.Stock.Count - 2].itemId, Is.EqualTo("wood_saw"));
    }

    [Test]
    public void CharcoalSellsForOneDollarInTheWoodAndStoneGroupAndIsNeverInTheStore()
    {
        MarketTable table = MarketCatalog.Table;
        Assert.That(table.TryGetRow("charcoal", out MarketRow row), Is.True);
        Assert.That(row.kind, Is.EqualTo(MarketKind.Bought), "the store only buys it");
        Assert.That(row.group, Is.EqualTo(MarketGroup.WoodStone));
        Assert.That(row.baseCents, Is.EqualTo(100L));
        Assert.That(table.BuyBaseCents("charcoal"), Is.EqualTo(0L), "not sold in the store: no buy loop");
        Assert.That(table.SellBaseCents("charcoal"), Is.EqualTo(100L));
        Assert.That(table.Stock.Any(r => r.itemId == "charcoal"), Is.False);
        // Six Logs ($18) make four Charcoal ($4): selling the Charcoal never beats selling the Logs.
        Assert.That(CharcoalRules.Yield(6) * table.SellBaseCents("charcoal"), Is.LessThan(6 * table.SellBaseCents("logs")));
    }

    [Test]
    public void TheAssetHasTheTwoRowsByHandAndTheSameRowCountAsTheCatalog()
    {
        string asset = SourceText("Resources/Market/MarketData.asset");
        Assert.That(asset, Does.Contain("  - itemId: iron_ore\n    group: 7\n    pool: iron_ore\n    baseCents: 400\n    kind: 0\n    seasonalBuy: 0\n"));
        Assert.That(asset, Does.Contain("  - itemId: charcoal\n    group: 0\n    pool: charcoal\n    baseCents: 100\n    kind: 1\n    seasonalBuy: 0\n"));
        int rows = asset.Split(new[] { "\n  - itemId: " }, StringSplitOptions.None).Length - 1;
        Assert.That(rows, Is.EqualTo(MarketCatalog.Rows.Length), "the asset and the catalog agree");
        Assert.That(rows, Is.EqualTo(92), "87, the two M1 rows and the three Smithy goods");
        Assert.That(SourceText("Scripts/Editor/MarketDataSeeder.cs"), Does.Not.Contain("CharcoalRules"), "the seeder was not touched");
    }

    [Test]
    public void TheLedgerFilesOreAsOtherGoodsOnPurposeAndCharcoalSalesAsWoodAndStone()
    {
        Assert.That(LedgerCategories.PurchaseIsListed("iron_ore"), Is.True);
        Assert.That(LedgerCategories.ForPurchase("iron_ore"), Is.EqualTo(LedgerCategory.BuyOtherGoods));
        Assert.That(LedgerCategories.ForSale(MarketGroup.WoodStone), Is.EqualTo(LedgerCategory.SalesWoodStone), "Charcoal sells in this group");
        Assert.That(LedgerCategories.ForSale(MarketGroup.Stocked), Is.EqualTo(LedgerCategory.SalesStockedBack), "Ore sold back");
        Assert.That(LedgerCategories.Count, Is.EqualTo(19), "no new category");
    }

    [Test]
    public void TheTwoItemAssetsAreMaterialsOfThreeAndOneKilogram()
    {
        foreach ((string id, string name, float kg) in new[] { ("iron_ore", "Iron Ore", 3f), ("charcoal", "Charcoal", 1f) })
        {
            string path = $"Assets/Resources/Items/Materials/{id}.asset";
            ScriptableObject item = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            Assert.That(item, Is.Not.Null, path);
            Type type = item.GetType();
            Assert.That(type.GetProperty("Id").GetValue(item), Is.EqualTo(id));
            Assert.That(type.GetProperty("DisplayName").GetValue(item), Is.EqualTo(name));
            Assert.That((float)type.GetProperty("WeightKg").GetValue(item), Is.EqualTo(kg));
            Assert.That(type.GetProperty("Category").GetValue(item).ToString(), Is.EqualTo("Material"));
            Assert.That(type.GetProperty("ShelfLifeDays").GetValue(item), Is.EqualTo(0f), "it does not spoil");
            Assert.That(File.Exists(Path.Combine(Application.dataPath, $"Resources/Items/Materials/{id}.asset.meta")), Is.True, "with its meta");
        }
        Assert.That(MetalRules.IronOreKg, Is.EqualTo(3f));
        Assert.That(MetalRules.CharcoalKg, Is.EqualTo(1f));
    }

    // --- Placement: the Kiln's rules with the Pit's own reach; regrowth ---

    static SiteInfo Site(float x, float radius, string name = "Small Cabin") =>
        new SiteInfo { kind = SiteKind.Building, position = new Vector3(x, 0f, 0f), name = name, radius = radius };

    [Test]
    public void ANewPitKeepsThreeMetresOfClearAirFromEveryStructureAndTreeFromItsOwnEdge()
    {
        SetbackRules rules = SetbackRules.Defaults;
        rules.kilnRadius = CharcoalRules.PitRadiusMetres;
        rules.kilnFromStructure = CharcoalRules.ClearanceMetres;
        rules.kilnFromTree = CharcoalRules.ClearanceMetres;
        var cabin = new List<SiteInfo> { Site(0f, 2.5f) };
        // 0.9 (the mound) + 2.5 (the cabin reach) + 3 = 6.4 m centre to centre.
        Assert.That(Setbacks.CanPlaceKiln(new Vector3(6.39f, 0f, 0f), cabin, false, rules, out string reason), Is.False);
        Assert.That(reason, Is.EqualTo("Too close to the Small Cabin: 3 m needed."));
        Assert.That(Setbacks.CanPlaceKiln(new Vector3(6.41f, 0f, 0f), cabin, false, rules, out _), Is.True);
        Assert.That(Setbacks.CanPlaceKiln(new Vector3(20f, 0f, 0f), cabin, true, rules, out reason), Is.False, "a tree within reach");
        Assert.That(reason, Is.EqualTo("Too close to a tree: 3 m needed."));
        Assert.That(CharcoalRules.TreeReachMetres, Is.EqualTo(CharcoalRules.PitRadiusMetres + CharcoalRules.ClearanceMetres));
    }

    [Test]
    public void APitThatStandsKeepsLaterBuildingsThreeMetresClearFromItsOwnEdgeAndAKilnWithNoRadiusStillUsesTheRules()
    {
        SetbackRules rules = SetbackRules.Defaults;
        var pit = new List<SiteInfo> { Site(0f, CharcoalRules.PitRadiusMetres, "Charcoal Pit") };
        // A Cabin (reach 2.5) going up beside it: 0.9 + 2.5 + 3 = 6.4 m, not the Kiln's 6.7.
        Assert.That(Setbacks.CanPlaceBesideKiln(new Vector3(6.39f, 0f, 0f), 2.5f, pit, rules, out string reason), Is.False);
        Assert.That(reason, Is.EqualTo("Too close to the Charcoal Pit: 3 m needed."));
        Assert.That(Setbacks.CanPlaceBesideKiln(new Vector3(6.41f, 0f, 0f), 2.5f, pit, rules, out _), Is.True);

        var kilnNoRadius = new List<SiteInfo> { Site(0f, 0f, "Kiln") };
        Assert.That(Setbacks.CanPlaceBesideKiln(new Vector3(6.69f, 0f, 0f), 2.5f, kilnNoRadius, rules, out _), Is.False, "falls back to the rules' kiln radius (1.2): 6.7 m");
        Assert.That(Setbacks.CanPlaceBesideKiln(new Vector3(6.71f, 0f, 0f), 2.5f, kilnNoRadius, rules, out _), Is.True);
    }

    [Test]
    public void WellsAndOuthousesWiresThePitIntoThePlacementChecks()
    {
        string wells = SourceText("Scripts/Managers/WellsAndOuthouses.cs");
        Assert.That(wells, Does.Contain("kind == PileKind.CharcoalPitSite || kind == PileKind.CharcoalPit"));
        Assert.That(wells, Does.Contain("A charcoal pit needs open sky: not under a roof."));
        Assert.That(wells, Does.Contain("OverheadCover.Roofed(point)"));
        Assert.That(wells, Does.Contain("Setbacks.CanPlaceKiln(point, KilnNeighbours(ignore), treeNearPit, pitRules, out reason)"));
        Assert.That(wells, Does.Contain("case PileKind.CharcoalPitSite: return CharcoalRules.PitRadiusMetres;"), "other structures keep clear of its reach too");
        Assert.That(wells, Does.Contain("WoodManager.IsCharcoalPit(pile.kind)"), "a pit is among the structures later buildings keep 3 m from");
        string wood = SourceText("Scripts/Managers/WoodManager.cs");
        Assert.That(wood, Does.Contain("kind == PileKind.CharcoalPitSite || kind == PileKind.BloomerySite || kind == PileKind.SmithySite) && WellsAndOuthouses.Instance != null"), "the Build tab's placement check asks CanPlace");
    }

    [Test]
    public void ANewTreeDoesNotSproutInsideThePitsFootprint()
    {
        Assert.That(RegrowRules.PitMetres, Is.EqualTo(CharcoalRules.RegrowMetres));
        Assert.That(RegrowRules.Blocks(RegrowRules.Blocker.Pit, 1.0f, 0f, 0f), Is.True);
        Assert.That(RegrowRules.Blocks(RegrowRules.Blocker.Pit, 1.39f, 0f, 0f), Is.True);
        Assert.That(RegrowRules.Blocks(RegrowRules.Blocker.Pit, 1.5f, 0f, 0f), Is.False);
        Assert.That(RegrowRules.Blocks(RegrowRules.Blocker.Kiln, 1.7f, 0f, 0f), Is.True, "the Kiln's blocker is unchanged");
        string wood = SourceText("Scripts/Managers/WoodManager.cs");
        Assert.That(wood, Does.Contain("IsCharcoalPit(pile.kind) ? RegrowRules.Blocker.Pit"));
        Assert.That(Find("WoodManager").GetMethod("IsCharcoalPit").Invoke(null, new[] { Kind("CharcoalPitSite") }), Is.EqualTo(true));
        Assert.That(Find("WoodManager").GetMethod("IsCharcoalPit").Invoke(null, new[] { Kind("Kiln") }), Is.EqualTo(false));
        Assert.That(Find("WoodManager").GetMethod("IsKiln").Invoke(null, new[] { Kind("CharcoalPit") }), Is.EqualTo(false), "the Pit is not a Kiln");
    }

    // --- The look ---

    static List<GameObject> BuildVisuals(object state, out Vector3 size)
    {
        var parent = new GameObject("Pit visuals test");
        var pieces = new List<GameObject>();
        object[] args = { parent.transform, state, null, pieces, Vector3.zero, Vector3.zero };
        Find("StructureVisuals").GetMethod("Build").Invoke(null, args);
        size = (Vector3)args[5];
        pieces.Add(parent);
        return pieces;
    }

    static void Cleanup(List<GameObject> pieces)
    {
        for (int i = pieces.Count - 1; i >= 0; i--)
            if (pieces[i] != null)
                UnityEngine.Object.DestroyImmediate(pieces[i]);
    }

    [Test]
    public void ThePitDrawsAMoundWithLogsBesideItWhenLoadedAndCharcoalWhenDone()
    {
        object empty = NewPile("CharcoalPit");
        var emptyPieces = BuildVisuals(empty, out Vector3 emptySize);
        object loaded = NewPile("CharcoalPit");
        Add(loaded, "logs", 6);
        var loadedPieces = BuildVisuals(loaded, out _);
        object burning = NewPile("CharcoalPit");
        Set(burning, "jobItem", CharcoalRules.BurnMarker);
        Set(burning, "jobCount", 4);
        Set(burning, "jobHours", 20f);
        var burningPieces = BuildVisuals(burning, out _);
        object done = NewPile("CharcoalPit");
        Add(done, "charcoal", 4);
        var donePieces = BuildVisuals(done, out _);
        try
        {
            Assert.That(emptyPieces.Count - 1, Is.EqualTo(4), "the ring of earth, the mound, the vent and the draught hole");
            Assert.That(loadedPieces.Count - emptyPieces.Count, Is.EqualTo(6), "six Logs lie beside it");
            Assert.That(burningPieces.Count, Is.EqualTo(emptyPieces.Count), "the Logs are used up; the smoke is KilnSmoke, not a piece");
            Assert.That(donePieces.Count - emptyPieces.Count, Is.EqualTo(4), "a heap of four Charcoal");
            Assert.That(emptySize.y, Is.InRange(0.6f, 1.2f));
            Assert.That(emptySize.x, Is.InRange(1.4f, 2.0f), "about the size of the mound, 1.5 m across");
        }
        finally
        {
            Cleanup(emptyPieces);
            Cleanup(loadedPieces);
            Cleanup(burningPieces);
            Cleanup(donePieces);
        }
    }

    [Test]
    public void TheSiteDrawsItsStakesAndTheDeliveredMaterials()
    {
        object site = NewPile("CharcoalPitSite");
        var bare = BuildVisuals(site, out _);
        object stocked = NewPile("CharcoalPitSite");
        Add(stocked, "clay", 4);
        Add(stocked, "tall_grass", 6);
        var full = BuildVisuals(stocked, out _);
        try
        {
            Assert.That(full.Count - bare.Count, Is.EqualTo(10), "four lumps of Clay, the plot's three thatch lumps and three bundles of Tall Grass");
        }
        finally
        {
            Cleanup(bare);
            Cleanup(full);
        }
    }

    [Test]
    public void TheDraughtHoleStandsClearOfTheMoundSoNothingFlashesAndTheSmokeIsSwitchedByTheBurn()
    {
        string placed = SourceText("Scripts/World/PlacedStructure.cs");
        Assert.That(placed, Does.Contain("new Vector3(0f, 0.14f, 0.72f), Vector3.zero, new Vector3(0.24f, 0.2f, 0.2f)"));
        // The hole's front face is at z 0.72 + 0.1 = 0.82; the mound (a 1.5 m wide, 1.6 m tall ellipsoid centred at the ground) is at radius 0.75 x sqrt(1 - (y / 0.8)^2) at height y.
        float front = 0.72f + 0.2f / 2f;
        foreach (float y in new[] { 0.04f, 0.14f, 0.24f })
        {
            float surface = 0.75f * Mathf.Sqrt(1f - (y / 0.8f) * (y / 0.8f));
            Assert.That(front - surface, Is.GreaterThan(0.05f), $"at height {y} the hole's face is well clear of the mound");
        }
        Assert.That(placed, Does.Contain("pitSmoke.SetFiring(state.PitBurning)"));
        Assert.That(placed, Does.Contain("KilnSmoke.Create(transform)"));
        // No light and no flicker: nothing in the Pit's builder animates a scale or adds a light.
        int start = placed.IndexOf("static void BuildCharcoalPit(", StringComparison.Ordinal);
        int end = placed.IndexOf("static void KilnPiece(", StringComparison.Ordinal);
        string builder = placed.Substring(start, end - start);
        Assert.That(builder, Does.Not.Contain("Light"));
        Assert.That(builder, Does.Not.Contain("Time.time"));
        Assert.That(builder, Does.Not.Contain("Random.value"));
    }

    // --- Know-how and item help ---

    [Test]
    public void TheCharcoalAndOreEntriesUnlockOnTheirTriggers()
    {
        Assert.That(KnowHowRules.Entries.Length, Is.EqualTo(11));
        Assert.That(KnowHowRules.TryGet(KnowHowRules.Charcoal, out KnowHowRules.Entry charcoal), Is.True);
        Assert.That(charcoal.title, Is.EqualTo("Charcoal: Burning Logs Slowly"));
        Assert.That(KnowHowRules.TryGet(KnowHowRules.IronOre, out KnowHowRules.Entry ore), Is.True);
        Assert.That(ore.title, Is.EqualTo("Iron Ore at the Trading Post"));

        Assert.That(KnowHowRules.ForItem("charcoal"), Is.EqualTo(new[] { "charcoal" }), "holding Charcoal");
        Assert.That(KnowHowRules.ForItem("iron_ore"), Is.EqualTo(new[] { "ironore" }), "holding Ore");
        Assert.That(KnowHowRules.ForHappening(KnowHowRules.Happening.PitSitePlaced), Is.EqualTo(new[] { "charcoal" }), "first building a Pit site");
        Assert.That(KnowHowRules.ForItem("clay"), Is.Empty);

        List<string> firstOre = KnowHowRules.NewUnlocks(new string[0], KnowHowRules.ForItem("iron_ore"));
        Assert.That(firstOre, Is.EqualTo(new[] { "ironore" }));
        Assert.That(KnowHowRules.NewUnlocks(firstOre, KnowHowRules.ForItem("iron_ore")), Is.Empty, "never twice");
        Assert.That(KnowHowRules.NewUnlocks(new[] { "charcoal" }, KnowHowRules.ForHappening(KnowHowRules.Happening.PitSitePlaced)), Is.Empty, "a site placed after Charcoal was held adds nothing");
    }

    [Test]
    public void AnOlderGameGetsTheEntriesItHasAlreadyEarned()
    {
        Assert.That(KnowHowRules.Backfill(id => id == "charcoal", false, false, false), Is.EqualTo(new[] { "charcoal" }));
        Assert.That(KnowHowRules.Backfill(id => id == "iron_ore", false, false, false), Is.EqualTo(new[] { "ironore" }));
        Assert.That(KnowHowRules.Backfill(id => false, false, false, false, pitPlaced: true), Is.EqualTo(new[] { "charcoal" }));
        Assert.That(KnowHowRules.Backfill(id => false, false, false, false), Is.Empty, "nothing earned, nothing added");
        Assert.That(SourceText("Scripts/Managers/JournalManager.cs"), Does.Contain("hopper, tripod, taps, pit, bloomery, smithy)"));
        Assert.That(SourceText("Scripts/Managers/WoodManager.cs"), Does.Contain("KnowHowRules.Happening.PitSitePlaced"));
    }

    [Test]
    public void TheEntriesAndTheItemTipsSayWhatTheCodeDoes()
    {
        KnowHowRules.TryGet(KnowHowRules.Charcoal, out KnowHowRules.Entry charcoal);
        Assert.That(charcoal.body, Does.Contain("4 Clay and 6 Tall Grass"));
        Assert.That(charcoal.body, Does.Contain("up to 6"));
        Assert.That(charcoal.body, Does.Contain("once it holds 3 Logs"));
        Assert.That(charcoal.body, Does.Contain("24 game hours"));
        Assert.That(charcoal.body, Does.Contain("3 or 4 Logs make 2 Charcoal, 5 make 3 and 6 make 4"));
        Assert.That(charcoal.body, Does.Contain("about $1"));
        Assert.That(CharcoalRules.SiteClay, Is.EqualTo(4));
        Assert.That(CharcoalRules.SiteTallGrass, Is.EqualTo(6));

        KnowHowRules.TryGet(KnowHowRules.IronOre, out KnowHowRules.Entry ore);
        Assert.That(ore.body, Does.Contain("$4 each (3 kg)"));
        Assert.That(ore.body, Does.Contain("for only $1"));
        Assert.That(ore.body, Does.Contain("the Bloomery (see \"Smelting Iron in the Bloomery\")"), "the Ore entry points to the next step");
        Assert.That(charcoal.body, Does.Contain("Smelting Iron in the Bloomery"), "and so does the Charcoal entry");

        foreach (string id in new[] { "iron_ore", "charcoal" })
        {
            string tip = KnowHowRules.ItemTip(id);
            Assert.That(tip, Is.Not.Empty, id);
            Assert.That(tip.Length, Is.LessThanOrEqualTo(80), id + " must fit in two lines of the row's info text");
        }
        Assert.That(KnowHowRules.ItemTip("iron_ore"), Does.Contain("$1").And.Contain("Bloomery"));
        Assert.That(KnowHowRules.ItemTip("charcoal"), Does.Contain("Charcoal Pit"));

        string craft = SourceText("Scripts/UI/CraftingScreen.cs");
        Assert.That(craft, Does.Contain("case PileKind.CharcoalPitSite:"));
        Assert.That(craft, Does.Contain("{CharcoalRules.SiteClay} Clay and {CharcoalRules.SiteTallGrass} Tall Grass"));
        Assert.That(craft, Does.Contain("case PileKind.CharcoalPit:"));
        Assert.That(craft, Does.Contain("{CharcoalRules.LogsCap}").And.Contain("{CharcoalRules.BurnHours}"));
    }

    [Test]
    public void TheKnowHowTextFileCarriesTheTwoNewEntriesSoItTravelsWithTheCode()
    {
        string path = Path.GetFullPath(Path.Combine(Application.dataPath, "../../../05_Documentation/Game_Systems/Journal_KnowHow_Text.md"));
        if (!File.Exists(path))
            Assert.Ignore("Journal_KnowHow_Text.md is not at " + path);
        string text = File.ReadAllText(path).Replace("\r\n", "\n");
        Assert.That(text, Does.Contain("## 8. Charcoal: Burning Logs Slowly"));
        Assert.That(text, Does.Contain("## 9. Iron Ore at the Trading Post"));
        Assert.That(text, Does.Contain("Trigger: first Charcoal held, or a Charcoal Pit site first placed."));
        Assert.That(text, Does.Contain("Trigger: first Iron Ore held."));
    }

    // --- The dev keys ---

    [Test]
    public void TheDevKeysAreOnFreeKeysAndTheKitFitsThePackExceptTheLogsWhichStandAsAPile()
    {
        string keys = SourceText("Scripts/Debug/TimeDebugControls.cs");
        Assert.That(keys, Does.StartWith("#if UNITY_EDITOR || DEVELOPMENT_BUILD"), "compiled out of release builds");
        Assert.That(keys, Does.Contain("keyboard.commaKey.wasPressedThisFrame"));
        Assert.That(keys, Does.Contain("keyboard.equalsKey.wasPressedThisFrame"));
        Assert.That(keys, Does.Contain("DebugFinishBurns()"));
        Assert.That(keys, Does.Contain("PileKind.CharcoalPit, PitAhead(4f)"));

        // Nothing else reads these keys, in code or in the input actions.
        string scripts = Path.Combine(Application.dataPath, "Scripts");
        foreach (string file in Directory.GetFiles(scripts, "*.cs", SearchOption.AllDirectories))
        {
            if (file.EndsWith("TimeDebugControls.cs") || file.EndsWith("CharcoalPitTests.cs"))
                continue;
            string content = File.ReadAllText(file);
            Assert.That(content, Does.Not.Contain("commaKey"), file);
            Assert.That(content, Does.Not.Contain("equalsKey"), file);
        }
        foreach (string file in Directory.GetFiles(Application.dataPath, "*.inputactions", SearchOption.AllDirectories))
        {
            string content = File.ReadAllText(file);
            Assert.That(content, Does.Not.Contain("<Keyboard>/comma"), file);
            Assert.That(content, Does.Not.Contain("<Keyboard>/equals"), file);
        }

        // The pack takes 45 kg. The small kit (Hammer 1.2, 4 Clay 3.5 each, 6 Tall Grass 0.1 each) is 15.8 kg; the 6 Logs are 48 kg, more than the whole pack.
        float small = 1.2f + CharcoalRules.SiteClay * 3.5f + CharcoalRules.SiteTallGrass * 0.1f;
        float logs = CharcoalRules.LogsCap * 8f;
        Assert.That(small, Is.EqualTo(15.8f).Within(0.001f));
        Assert.That(small, Is.LessThan(45f));
        Assert.That(small + logs, Is.GreaterThan(45f), "the whole kit with its Logs would not fit, so the Logs stand as a Wood Pile 3 m ahead");
        Assert.That(10 * MetalRules.IronOreKg + 10 * MetalRules.CharcoalKg, Is.EqualTo(40f), "the = key's 10 Ore and 10 Charcoal fit an empty pack");
    }

    [Test]
    public void TheDevKeyForTheBurnsFinishesEveryRunningOneAndNothingElse()
    {
        string wells = SourceText("Scripts/Managers/WellsAndOuthouses.cs");
        int start = wells.IndexOf("public int DebugFinishBurns()", StringComparison.Ordinal);
        Assert.That(start, Is.GreaterThan(0));
        string body = wells.Substring(start, 700);
        Assert.That(body, Does.Contain("pit.kind != PileKind.CharcoalPit || !pit.PitBurning"));
        Assert.That(body, Does.Contain("FinishBurn(pit)"));
        Assert.That(wells, Does.Contain("A Charcoal Pit's burn runs one game hour at a time too"));
    }

    // --- Nothing else changed ---

    [Test]
    public void TheHopperAndTheKilnAndTheRestOfTheMarketAreUntouched()
    {
        Assert.That(PotashRules.HopperCapacity, Is.EqualTo(12));
        Assert.That(PotashRules.HopperWaterCapacity, Is.EqualTo(12));
        Assert.That(PotteryRules.KilnFirewood, Is.EqualTo(10));
        Assert.That(PotteryRules.KilnFireHours, Is.EqualTo(24));
        Assert.That(PotteryRules.KilnStone, Is.EqualTo(6));
        Assert.That(PotteryRules.KilnClay, Is.EqualTo(6));
        Assert.That(MarketCatalog.Table.SellBaseCents("logs"), Is.EqualTo(300L));
        Assert.That(MarketCatalog.Table.BuyBaseCents("axe"), Is.EqualTo(2000L));
        Assert.That(MarketCatalog.Table.Groups.Count(), Is.EqualTo(11), "no new market group");
        Assert.That(Enum.GetValues(typeof(MarketGroup)).Length, Is.EqualTo(11));
    }
}
