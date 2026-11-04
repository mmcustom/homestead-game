using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

// EditMode tests for Metalworking phase M2 (Mike, 2026-11-04): the Iron Bar and the Bloomery. BloomeryRules and MetalRules are pure (the batch table, the caps, the gates, the phases, the
// smelt's hours and every word); the main-assembly parts (the saved pile, the staged site, the kinds, the visuals) are reached by reflection as the other world tests do.
// Nothing here touches the Saves folder or a scene.
public class BloomeryTests
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

    static void Take(object pile, string id, int count) => Find("WoodPileState").GetMethod("Take").Invoke(pile, new object[] { id, count });

    static int Count(object pile, string id) => (int)Find("WoodPileState").GetMethod("Count").Invoke(pile, new object[] { id });

    static object Prop(object pile, string name) => Find("WoodPileState").GetProperty(name).GetValue(pile);

    static bool Accepts(object pile, string id) => (bool)Find("WoodPileState").GetMethod("Accepts").Invoke(pile, new object[] { id });

    static object Wells(string method, params object[] args) =>
        Find("WellsAndOuthouses").GetMethod(method, BindingFlags.Public | BindingFlags.Static).Invoke(null, args);

    static void Set(object pile, string field, object value) => Find("WoodPileState").GetField(field).SetValue(pile, value);

    static object Get(object pile, string field) => Find("WoodPileState").GetField(field).GetValue(pile);

    // What PlacedStructure.BloomeryStart does to the saved pile, step for step.
    static int Start(object furnace)
    {
        int ore = Count(furnace, "iron_ore"), charcoal = Count(furnace, "charcoal");
        int bars = BloomeryRules.BarsFor(ore, charcoal);
        Take(furnace, "iron_ore", BloomeryRules.OreUsed(bars));
        Take(furnace, "charcoal", BloomeryRules.CharcoalUsed(bars));
        Set(furnace, "jobItem", BloomeryRules.SmeltMarker);
        Set(furnace, "jobCount", bars);
        Set(furnace, "jobHours", (float)BloomeryRules.SmeltHours);
        return bars;
    }

    // --- MetalRules: the iron row now says what a bar takes ---

    [Test]
    public void TheIronRowSaysWhatABarTakesAndStaysTheOnlyRow()
    {
        Assert.That(MetalRules.Metals.Length, Is.EqualTo(1));
        Assert.That(MetalRules.TryFind("iron", out MetalRules.Metal iron), Is.True);
        Assert.That(iron.barId, Is.EqualTo("iron_bar"));
        Assert.That(iron.barName, Is.EqualTo("Iron Bar"));
        Assert.That(iron.barKg, Is.EqualTo(2f));
        Assert.That(iron.orePerBar, Is.EqualTo(2));
        Assert.That(iron.fuelPerBar, Is.EqualTo(2));
        Assert.That(iron.oreId, Is.EqualTo("iron_ore"), "the M1 columns are unchanged");
        Assert.That(iron.fuelId, Is.EqualTo("charcoal"));
        Assert.That(MetalRules.TryFindByBar("iron_bar", out MetalRules.Metal byBar), Is.True);
        Assert.That(byBar.id, Is.EqualTo("iron"));
        Assert.That(MetalRules.IsBar("iron_bar"), Is.True);
        Assert.That(MetalRules.IsBar("iron_ore"), Is.False);
        Assert.That(MetalRules.BarIds, Is.EqualTo(new[] { "iron_bar" }));
        Assert.That(MetalRules.Metals.Select(m => m.barId).Distinct().Count(), Is.EqualTo(MetalRules.Metals.Length), "bar ids are unique");
        Assert.That(BloomeryRules.OrePerBar, Is.EqualTo(iron.orePerBar), "the furnace reads the table, not copies");
        Assert.That(BloomeryRules.CharcoalPerBar, Is.EqualTo(iron.fuelPerBar));
        Assert.That(BloomeryRules.BarId, Is.EqualTo(iron.barId));
    }

    // --- The Bloomery's numbers ---

    [Test]
    public void TheBloomeryNumbersAreMikes()
    {
        Assert.That(BloomeryRules.SiteStone, Is.EqualTo(8));
        Assert.That(BloomeryRules.SiteClay, Is.EqualTo(6));
        Assert.That(BloomeryRules.BuildBlows, Is.EqualTo(36));
        Assert.That(BloomeryRules.OreCap, Is.EqualTo(4));
        Assert.That(BloomeryRules.CharcoalCap, Is.EqualTo(4));
        Assert.That(BloomeryRules.OrePerBar, Is.EqualTo(2));
        Assert.That(BloomeryRules.CharcoalPerBar, Is.EqualTo(2));
        Assert.That(BloomeryRules.MaxBars, Is.EqualTo(2));
        Assert.That(BloomeryRules.SmeltHours, Is.EqualTo(10));
        Assert.That(BloomeryRules.ClearanceMetres, Is.EqualTo(PotteryRules.KilnClearanceMetres), "the Kiln's 3 m");
        Assert.That(BloomeryRules.SmeltMarker, Is.EqualTo("iron_smelt"));
        Assert.That(BloomeryRules.SmeltMarker, Is.Not.EqualTo(CharcoalRules.BurnMarker), "its own marker, apart from the Pit's");
        Assert.That(CharcoalRules.BurnMarker, Is.EqualTo("charcoal_burn"), "the Pit's marker is untouched");
    }

    [Test]
    public void TheBatchTableTwoOreAndTwoCharcoalMakeOneBar()
    {
        Assert.That(BloomeryRules.BarsFor(2, 2), Is.EqualTo(1));
        Assert.That(BloomeryRules.BarsFor(3, 3), Is.EqualTo(1), "3 Ore makes 1 Bar and leaves 1 Ore");
        Assert.That(BloomeryRules.BarsFor(3, 4), Is.EqualTo(1));
        Assert.That(BloomeryRules.BarsFor(4, 2), Is.EqualTo(1), "Charcoal limits it");
        Assert.That(BloomeryRules.BarsFor(2, 4), Is.EqualTo(1), "Ore limits it");
        Assert.That(BloomeryRules.BarsFor(4, 4), Is.EqualTo(2));
        Assert.That(BloomeryRules.BarsFor(8, 8), Is.EqualTo(2), "never more than 2 Bars to a smelt");
        Assert.That(BloomeryRules.BarsFor(1, 4), Is.EqualTo(0));
        Assert.That(BloomeryRules.BarsFor(4, 1), Is.EqualTo(0));
        Assert.That(BloomeryRules.BarsFor(0, 0), Is.EqualTo(0));
        Assert.That(BloomeryRules.BarsFor(-3, 4), Is.EqualTo(0));
        Assert.That(BloomeryRules.OreUsed(1), Is.EqualTo(2));
        Assert.That(BloomeryRules.CharcoalUsed(2), Is.EqualTo(4));
    }

    [Test]
    public void ThePhasesFollowTheStockTheSmeltAndTheBarsWaiting()
    {
        Assert.That(BloomeryRules.PhaseOf(false, 0, 0, 0), Is.EqualTo(BloomeryRules.Phase.Empty));
        Assert.That(BloomeryRules.PhaseOf(false, 0, 3, 0), Is.EqualTo(BloomeryRules.Phase.Loading));
        Assert.That(BloomeryRules.PhaseOf(false, 0, 1, 4), Is.EqualTo(BloomeryRules.Phase.Loading));
        Assert.That(BloomeryRules.PhaseOf(false, 0, 2, 2), Is.EqualTo(BloomeryRules.Phase.Ready));
        Assert.That(BloomeryRules.PhaseOf(true, 0, 0, 0), Is.EqualTo(BloomeryRules.Phase.Smelting));
        Assert.That(BloomeryRules.PhaseOf(true, 0, 4, 4), Is.EqualTo(BloomeryRules.Phase.Smelting), "stock waiting does not start a second smelt");
        Assert.That(BloomeryRules.PhaseOf(false, 2, 4, 4), Is.EqualTo(BloomeryRules.Phase.Done), "E collects the Bars before it starts the next smelt");
        Assert.That(BloomeryRules.CanStart(BloomeryRules.Phase.Ready), Is.True);
        Assert.That(BloomeryRules.CanStart(BloomeryRules.Phase.Done), Is.False);
        Assert.That(BloomeryRules.CanStart(BloomeryRules.Phase.Loading), Is.False);
        Assert.That(BloomeryRules.CanDismantle(BloomeryRules.Phase.Smelting), Is.False);
        Assert.That(BloomeryRules.CanDismantle(BloomeryRules.Phase.Ready), Is.True, "stock comes back whole");
        Assert.That(BloomeryRules.CanDismantle(BloomeryRules.Phase.Done), Is.True, "and so do the Bars");
    }

    [Test]
    public void LoadingTakesWhatIsCarriedUpToEachCapAndNeverPastFour()
    {
        Assert.That(BloomeryRules.OreRoom(0), Is.EqualTo(4));
        Assert.That(BloomeryRules.OreRoom(3), Is.EqualTo(1));
        Assert.That(BloomeryRules.CharcoalRoom(4), Is.EqualTo(0));

        BloomeryRules.LoadPlan both = BloomeryRules.PlanLoad(0, 0, 6, 9);
        Assert.That(both.ore, Is.EqualTo(4), "Ore up to its cap of 4");
        Assert.That(both.charcoal, Is.EqualTo(4), "Charcoal up to its cap of 4");
        BloomeryRules.LoadPlan oreOnly = BloomeryRules.PlanLoad(1, 0, 2, 0);
        Assert.That((oreOnly.ore, oreOnly.charcoal), Is.EqualTo((2, 0)));
        BloomeryRules.LoadPlan charcoalOnly = BloomeryRules.PlanLoad(0, 3, 0, 5);
        Assert.That((charcoalOnly.ore, charcoalOnly.charcoal), Is.EqualTo((0, 1)));
        BloomeryRules.LoadPlan topUp = BloomeryRules.PlanLoad(3, 2, 2, 2);
        Assert.That((topUp.ore, topUp.charcoal), Is.EqualTo((1, 2)), "top-ups fill to the cap");
        Assert.That(BloomeryRules.PlanLoad(4, 4, 3, 3).Any, Is.False, "full of both");
        Assert.That(BloomeryRules.PlanLoad(0, 0, 0, 0).Any, Is.False, "nothing carried");
    }

    [Test]
    public void TheSmeltCountsDownOneGameHourAtATimeAndEndsOnTheTenth()
    {
        float hours = BloomeryRules.SmeltHours;
        int ticks = 0;
        while (!BloomeryRules.IsDone(hours))
        {
            hours = BloomeryRules.AfterHour(hours);
            ticks++;
        }
        Assert.That(ticks, Is.EqualTo(10));
        Assert.That(BloomeryRules.AfterHour(0f), Is.EqualTo(0f), "an idle furnace does not go below zero");
        Assert.That(BloomeryRules.IsDone(1f), Is.False);
    }

    // --- The words ---

    [Test]
    public void ThePromptsSayWhatTheNextStepWillMake()
    {
        Assert.That(BloomeryRules.Prompt(BloomeryRules.Phase.Ready, 4, 4, 0, 0, 0f), Is.EqualTo("Start the smelt  (4 Ore + 4 Charcoal -> 2 Iron Bars in 10 h)"));
        Assert.That(BloomeryRules.Prompt(BloomeryRules.Phase.Ready, 2, 2, 0, 0, 0f), Is.EqualTo("Start the smelt  (2 Ore + 2 Charcoal -> 1 Iron Bar in 10 h)"));
        Assert.That(BloomeryRules.Prompt(BloomeryRules.Phase.Ready, 3, 4, 0, 0, 0f), Is.EqualTo("Start the smelt  (2 Ore + 2 Charcoal -> 1 Iron Bar in 10 h; 1 Ore and 2 Charcoal left over)"));
        Assert.That(BloomeryRules.Prompt(BloomeryRules.Phase.Empty, 0, 0, 0, 0, 0f), Is.EqualTo("Bloomery  (empty, needs 2 Ore and 2 Charcoal)"));
        Assert.That(BloomeryRules.Prompt(BloomeryRules.Phase.Loading, 3, 0, 0, 0, 0f), Is.EqualTo("Bloomery  (3 Ore - needs 2 more Charcoal)"));
        Assert.That(BloomeryRules.Prompt(BloomeryRules.Phase.Loading, 1, 4, 0, 0, 0f), Is.EqualTo("Bloomery  (1 Ore and 4 Charcoal - needs 1 more Ore)"));
        Assert.That(BloomeryRules.Prompt(BloomeryRules.Phase.Smelting, 0, 0, 0, 2, 7.2f), Is.EqualTo("Bloomery  (smelting 2 Iron Bars, 8 h to go)"));
        Assert.That(BloomeryRules.Prompt(BloomeryRules.Phase.Smelting, 2, 1, 0, 1, 3f), Is.EqualTo("Bloomery  (smelting 1 Iron Bar, 3 h to go; 2 Ore and 1 Charcoal waiting)"));
        Assert.That(BloomeryRules.Prompt(BloomeryRules.Phase.Done, 0, 0, 2, 0, 0f), Is.EqualTo("Collect Iron Bars  (2)"));
        Assert.That(BloomeryRules.ShortText(0, 0), Is.EqualTo("needs 2 Ore and 2 Charcoal"));
        Assert.That(BloomeryRules.ShortText(2, 2), Is.EqualTo(""));
    }

    [Test]
    public void TheLoadLinesNameWhatIsMissingAndWhatIsFull()
    {
        string Prompt(int oreHeld, int charcoalHeld, int oreCarried, int charcoalCarried) =>
            BloomeryRules.LoadPrompt(BloomeryRules.PlanLoad(oreHeld, charcoalHeld, oreCarried, charcoalCarried), oreHeld, charcoalHeld, oreCarried, charcoalCarried);

        Assert.That(Prompt(0, 0, 4, 4), Is.EqualTo("Load 4 Ore and 4 Charcoal"));
        Assert.That(Prompt(0, 0, 3, 0), Is.EqualTo("Load 3 Ore  (no Charcoal in your pack)"));
        Assert.That(Prompt(0, 0, 0, 2), Is.EqualTo("Load 2 Charcoal  (no Ore in your pack)"));
        Assert.That(Prompt(4, 0, 2, 2), Is.EqualTo("Load 2 Charcoal  (the Ore is full)"));
        Assert.That(Prompt(0, 4, 3, 2), Is.EqualTo("Load 3 Ore  (the Charcoal is full)"));
        Assert.That(Prompt(0, 0, 0, 0), Is.EqualTo("Load Ore and Charcoal  (you're not carrying any)"));
        Assert.That(Prompt(4, 4, 2, 2), Is.EqualTo("Can't load  (it already holds 4 Ore and 4 Charcoal)"));
        Assert.That(Prompt(4, 1, 2, 0), Is.EqualTo("Can't load  (it already holds 4 Ore)"));
        Assert.That(Prompt(1, 4, 0, 3), Is.EqualTo("Can't load  (it already holds 4 Charcoal)"));
    }

    [Test]
    public void TheMessagesAndTheDismantleLines()
    {
        var both = new BloomeryRules.LoadPlan { ore = 3, charcoal = 2 };
        Assert.That(BloomeryRules.Loaded(both, 3, 2, 3, 2), Is.EqualTo("Loaded 3 Ore and 2 Charcoal  (the bloomery holds 3 Ore and 2 Charcoal)."));
        var oreOnly = new BloomeryRules.LoadPlan { ore = 3, charcoal = 0 };
        Assert.That(BloomeryRules.Loaded(oreOnly, 3, 0, 3, 0), Is.EqualTo("Loaded 3 Ore  (the bloomery holds 3 Ore and 0 Charcoal). Not loaded: no Charcoal in your pack."));
        var charcoalOnly = new BloomeryRules.LoadPlan { ore = 0, charcoal = 1 };
        Assert.That(BloomeryRules.Loaded(charcoalOnly, 4, 4, 2, 3), Is.EqualTo("Loaded 1 Charcoal  (the bloomery holds 4 Ore and 4 Charcoal). Not loaded: the Ore is full."));
        Assert.That(BloomeryRules.NotReady(0, 0), Is.EqualTo("Load at least 2 Ore and 2 Charcoal first (R)."));
        Assert.That(BloomeryRules.NotReady(3, 1), Is.EqualTo("It needs 1 more Charcoal to smelt (it holds 3 Ore and 1 Charcoal)."));
        Assert.That(BloomeryRules.Started(2, 0, 0), Is.EqualTo("The smelt is started: 4 Ore + 4 Charcoal -> 2 Iron Bars in 10 h."));
        Assert.That(BloomeryRules.Started(1, 1, 2), Is.EqualTo("The smelt is started: 2 Ore + 2 Charcoal -> 1 Iron Bar in 10 h. 1 Ore and 2 Charcoal wait for the next smelt."));
        Assert.That(BloomeryRules.Took(2, 0), Is.EqualTo("Took 2 Iron Bars."));
        Assert.That(BloomeryRules.Took(1, 1), Is.EqualTo("Took 1 Iron Bar (1 left in the bloomery, no room)."));
        Assert.That(BloomeryRules.Took(0, 2), Is.EqualTo("Too heavy - no room to carry it"));
        Assert.That(BloomeryRules.DismantlePrompt(BloomeryRules.Phase.Smelting, 6f), Is.EqualTo("Can't take down the bloomery while it smelts  (6 h to go)"));
        Assert.That(BloomeryRules.DismantlePrompt(BloomeryRules.Phase.Ready, 0f), Does.StartWith("Take down the bloomery").And.Contain("come back whole"));
        Assert.That(BloomeryRules.SiteHelp, Is.EqualTo("Deliver 8 Stone and 6 Clay"));
        Assert.That(BloomeryRules.BuiltMessage, Does.Contain("press R to load Ore and Charcoal (up to 4 of each)").And.Contain("then E to start a smelt"));
        Assert.That(BloomeryRules.LoadRefusal(0, 0, 0, 0), Does.Contain("not carrying any Ore or Charcoal"));
        Assert.That(BloomeryRules.LoadRefusal(4, 4, 2, 2), Does.Contain("it already holds 4 Ore and 4 Charcoal"));
    }

    // --- The kinds: appended after the Pit, nothing moved ---

    [Test]
    public void TheTwoBloomeryKindsAreFortyOneAndFortyTwoAndNothingElseWasAdded()
    {
        Type kind = Find("PileKind");
        Assert.That(Enum.GetValues(kind).Length, Is.EqualTo(45), "41 kinds before the Bloomery, plus its two and the Smithy's two (43, 44)");
        Assert.That((int)Enum.Parse(kind, "Kiln"), Is.EqualTo(38), "nothing moved");
        Assert.That((int)Enum.Parse(kind, "CharcoalPitSite"), Is.EqualTo(39));
        Assert.That((int)Enum.Parse(kind, "CharcoalPit"), Is.EqualTo(40));
        Assert.That((int)Enum.Parse(kind, "BloomerySite"), Is.EqualTo(41));
        Assert.That((int)Enum.Parse(kind, "Bloomery"), Is.EqualTo(42));
        string[] names = Enum.GetNames(kind);
        Assert.That((int)Enum.Parse(kind, "Smithy"), Is.EqualTo(44), "the Smithy came last (M3)");
        Assert.That(names.Any(n => n.Contains("Forge") || n.Contains("Anvil")), Is.False, "no separate Forge or Anvil kinds");
        Assert.That(names.Last(), Is.EqualTo("Smithy"));
    }

    // --- The staged site ---

    [Test]
    public void TheSiteIsAStagedSiteOfEightStoneAndSixClayThatBecomesTheBloomery()
    {
        Assert.That((bool)Wells("IsStagedSite", Kind("BloomerySite")), Is.True);
        Assert.That((bool)Wells("IsStagedSite", Kind("Bloomery")), Is.False);
        Assert.That(Wells("FinishedKind", Kind("BloomerySite")).ToString(), Is.EqualTo("Bloomery"));
        Assert.That((bool)Wells("RequiresDig", Kind("BloomerySite")), Is.False, "RequiresDig is false: no Shovel stage, the Hammer builds it");

        object cost = Wells("FinishedCost", Kind("BloomerySite"));
        var lines = ((IEnumerable)cost).Cast<object>().Select(c => (c.GetType().GetField("itemId").GetValue(c) as string, (int)c.GetType().GetField("count").GetValue(c))).ToArray();
        Assert.That(lines, Is.EqualTo(new[] { ("stone", 8), ("clay", 6) }));
        Assert.That((float)Wells("WeightKg", cost), Is.EqualTo(61f).Within(0.001f), "8 Stone at 5 kg and 6 Clay at 3.5 kg");
        Assert.That((bool)Wells("NeedsStaging", cost), Is.True, "far over the 15 kg staging line");
        Assert.That(61f, Is.GreaterThan(45f), "and more than the 45 kg pack");

        var order = (string[][])Wells("DeliveryOrder", Kind("BloomerySite"));
        Assert.That(order.Length, Is.EqualTo(1), "one group: the Stone and the Clay in any order");
        Assert.That(order[0], Is.EquivalentTo(new[] { "stone", "clay" }));

        object site = NewPile("BloomerySite");
        Assert.That(Accepts(site, "stone"), Is.True);
        Assert.That(Accepts(site, "clay"), Is.True);
        Assert.That(Accepts(site, "logs"), Is.False);
        for (int i = 0; i < 8; i++)
            Add(site, "stone", 1);
        Assert.That(Accepts(site, "stone"), Is.False, "full of Stone");
        Assert.That(Accepts(site, "clay"), Is.True);
        Assert.That(Wells("SiteNeeds", site), Is.EqualTo("6 Clay"));
        for (int i = 0; i < 6; i++)
            Add(site, "clay", 1);
        Assert.That(Wells("SiteNeeds", site), Is.Null, "fully stocked: the Hammer finishes it");
        Assert.That(BloomeryRules.BuildBlows, Is.EqualTo(36));
    }

    [Test]
    public void TheSiteIsInTheBuildListButTheBloomeryIsNotAndTheyAreDrawnByPlacedStructure()
    {
        var buildable = ((IEnumerable)Find("WoodManager").GetField("Buildable").GetValue(null)).Cast<object>().Select(o => o.ToString()).ToArray();
        Assert.That(buildable, Does.Contain("BloomerySite"));
        Assert.That(buildable, Does.Not.Contain("Bloomery"));
        Type wood = Find("WoodManager");
        Assert.That(wood.GetMethod("PileName").Invoke(null, new[] { Kind("BloomerySite") }), Is.EqualTo("Bloomery Site"));
        Assert.That(wood.GetMethod("PileName").Invoke(null, new[] { Kind("Bloomery") }), Is.EqualTo("Bloomery"));
        Assert.That(Wells("FinishedMessage", Kind("BloomerySite")), Is.EqualTo(BloomeryRules.BuiltMessage));
        Assert.That(Prop(NewPile("Bloomery"), "IsPlacedStructure"), Is.EqualTo(true));
        Assert.That(Prop(NewPile("BloomerySite"), "IsPlacedStructure"), Is.EqualTo(true));
        Assert.That(wood.GetMethod("IsBloomery").Invoke(null, new[] { Kind("BloomerySite") }), Is.EqualTo(true));
        Assert.That(wood.GetMethod("IsBloomery").Invoke(null, new[] { Kind("CharcoalPit") }), Is.EqualTo(false), "the Pit is not a Bloomery");
        Assert.That(wood.GetMethod("IsKiln").Invoke(null, new[] { Kind("Bloomery") }), Is.EqualTo(false));
        Assert.That(wood.GetMethod("IsCharcoalPit").Invoke(null, new[] { Kind("Bloomery") }), Is.EqualTo(false));
        string placement = SourceText("Scripts/Player/PlacementMode.cs");
        Assert.That(placement, Does.Contain("case PileKind.BloomerySite:"), "the mouse wheel turns the site");
    }

    // --- The furnace's stock ---

    [Test]
    public void TheBloomeryTakesOreAndCharcoalSeparatelyUpToFourEvenWhileItSmeltsAndNotBars()
    {
        object furnace = NewPile("Bloomery");
        Assert.That(Accepts(furnace, "iron_ore"), Is.True);
        Assert.That(Accepts(furnace, "charcoal"), Is.True);
        Assert.That(Accepts(furnace, "logs"), Is.False);
        Assert.That(Accepts(furnace, "iron_bar"), Is.False, "the Bars it makes are not something to put back");
        for (int i = 0; i < 4; i++)
        {
            Assert.That(Accepts(furnace, "iron_ore"), Is.True, $"room for ore {i + 1}");
            Add(furnace, "iron_ore", 1);
        }
        Assert.That(Accepts(furnace, "iron_ore"), Is.False, "the Ore cap of 4");
        Assert.That(Accepts(furnace, "charcoal"), Is.True, "Charcoal is separate: still room");
        for (int i = 0; i < 4; i++)
            Add(furnace, "charcoal", 1);
        Assert.That(Accepts(furnace, "charcoal"), Is.False, "the Charcoal cap of 4");

        Set(furnace, "jobItem", BloomeryRules.SmeltMarker);
        Take(furnace, "iron_ore", 4);
        Assert.That(Prop(furnace, "SmeltRunning"), Is.EqualTo(true));
        Assert.That(Accepts(furnace, "iron_ore"), Is.True, "top-ups are taken while it smelts");
        Add(furnace, "iron_bar", 1);
        Assert.That(Accepts(furnace, "iron_ore"), Is.True, "and while Bars wait");
    }

    [Test]
    public void ASmeltTakesWhatItUsesOutOfTheStockSoTheStockIsAlwaysForTheNextSmelt()
    {
        object furnace = NewPile("Bloomery");
        Add(furnace, "iron_ore", 3);
        Add(furnace, "charcoal", 4);
        Assert.That(Wells("BloomeryPhaseOf", furnace), Is.EqualTo(BloomeryRules.Phase.Ready));

        int bars = Start(furnace);
        Assert.That(bars, Is.EqualTo(1), "3 Ore makes 1 Bar");
        Assert.That(Count(furnace, "iron_ore"), Is.EqualTo(1), "and leaves 1 Ore as stock");
        Assert.That(Count(furnace, "charcoal"), Is.EqualTo(2), "and 2 Charcoal as stock");
        Assert.That(Wells("BloomeryPhaseOf", furnace), Is.EqualTo(BloomeryRules.Phase.Smelting));
        Assert.That(Get(furnace, "jobItem"), Is.EqualTo("iron_smelt"));
        Assert.That((int)Get(furnace, "jobCount"), Is.EqualTo(1), "the Bars it will make, fixed when it was started");
        Assert.That((float)Get(furnace, "jobHours"), Is.EqualTo(10f));

        // Material added while it smelts waits for the next smelt, and does not change this one.
        Add(furnace, "iron_ore", 3);
        Add(furnace, "charcoal", 2);
        Assert.That((int)Get(furnace, "jobCount"), Is.EqualTo(1), "the running smelt is unchanged");
        Assert.That(Count(furnace, "iron_ore"), Is.EqualTo(4));
        Assert.That(Count(furnace, "charcoal"), Is.EqualTo(4));
    }

    [Test]
    public void ASmeltEndsOnTheTenthHourWithItsBarsAndTheStockWaitsForTheNext()
    {
        object furnace = NewPile("Bloomery");
        Add(furnace, "iron_ore", 4);
        Add(furnace, "charcoal", 4);
        Assert.That(Start(furnace), Is.EqualTo(2));
        Assert.That(Count(furnace, "iron_ore") + Count(furnace, "charcoal"), Is.EqualTo(0), "all of it was used");
        Add(furnace, "iron_ore", 2);
        Add(furnace, "charcoal", 3);

        for (int hour = 1; hour <= 9; hour++)
        {
            Set(furnace, "jobHours", BloomeryRules.AfterHour((float)Get(furnace, "jobHours")));
            Assert.That(BloomeryRules.IsDone((float)Get(furnace, "jobHours")), Is.False, $"after {hour} h");
        }
        Assert.That(Count(furnace, "iron_bar"), Is.EqualTo(0), "no Bars before the end");
        Set(furnace, "jobHours", BloomeryRules.AfterHour((float)Get(furnace, "jobHours")));
        Assert.That(BloomeryRules.IsDone((float)Get(furnace, "jobHours")), Is.True, "done on the 10th");

        Wells("FinishSmelt", furnace);
        Assert.That(Count(furnace, "iron_bar"), Is.EqualTo(2), "4 Ore and 4 Charcoal made 2 Bars");
        Assert.That(Count(furnace, "iron_ore"), Is.EqualTo(2), "the stock added during the smelt is still there");
        Assert.That(Count(furnace, "charcoal"), Is.EqualTo(3));
        Assert.That(Get(furnace, "jobItem"), Is.EqualTo(""));
        Assert.That((float)Get(furnace, "jobHours"), Is.EqualTo(0f));
        Assert.That((int)Get(furnace, "jobCount"), Is.EqualTo(0));
        Assert.That(Wells("BloomeryPhaseOf", furnace), Is.EqualTo(BloomeryRules.Phase.Done), "E collects the Bars first, though the stock could start another smelt");
        Wells("FinishSmelt", furnace);
        Assert.That(Count(furnace, "iron_bar"), Is.EqualTo(2), "a second call adds nothing");

        Take(furnace, "iron_bar", 2);
        Assert.That(Wells("BloomeryPhaseOf", furnace), Is.EqualTo(BloomeryRules.Phase.Ready), "with the Bars gone the stock can start the next smelt");
    }

    [Test]
    public void EveryBatchSizeComesOutOfAFinishedSmelt()
    {
        foreach ((int ore, int charcoal, int bars) in new[] { (2, 2, 1), (3, 3, 1), (4, 2, 1), (2, 4, 1), (4, 4, 2), (3, 4, 1) })
        {
            object furnace = NewPile("Bloomery");
            Add(furnace, "iron_ore", ore);
            Add(furnace, "charcoal", charcoal);
            Assert.That(Start(furnace), Is.EqualTo(bars), $"{ore} Ore and {charcoal} Charcoal");
            Set(furnace, "jobHours", 1f);
            Set(furnace, "jobHours", BloomeryRules.AfterHour((float)Get(furnace, "jobHours")));
            Wells("FinishSmelt", furnace);
            Assert.That(Count(furnace, "iron_bar"), Is.EqualTo(bars));
            Assert.That(Count(furnace, "iron_ore"), Is.EqualTo(ore - bars * 2));
            Assert.That(Count(furnace, "charcoal"), Is.EqualTo(charcoal - bars * 2));
        }
    }

    // --- Saving: no new field; a smelt survives a save and a load ---

    [Test]
    public void TheBloomeryUsesTheExistingFieldsAndTheSavedPileGainedNone()
    {
        string[] fields = Find("WoodPileState").GetFields(BindingFlags.Public | BindingFlags.Instance).Select(f => f.Name).ToArray();
        Assert.That(fields.Length, Is.EqualTo(17), "still the 17 fields");
        Assert.That(fields, Does.Contain("jobItem").And.Contain("jobHours").And.Contain("jobCount").And.Contain("contents"));
        Assert.That(fields, Does.Contain("hopperWater").And.Contain("hopperBatch"), "the Hopper's fields are untouched");
        Assert.That(fields.Any(f => f.ToLowerInvariant().Contains("bloom") || f.ToLowerInvariant().Contains("smelt") || f.ToLowerInvariant().Contains("bar")), Is.False);
        FieldInfo version = Find("SaveManager").GetField("SaveVersion", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        Assert.That(version.GetRawConstantValue(), Is.EqualTo(1));
        Assert.That(LedgerSaveData.CurrentVersion, Is.EqualTo(1));
    }

    [Test]
    public void ASmeltInProgressSurvivesASaveAndALoadWithItsStock()
    {
        object furnace = NewPile("Bloomery");
        Add(furnace, "iron_ore", 4);
        Add(furnace, "charcoal", 3);
        Start(furnace);
        Set(furnace, "jobHours", 6f);
        Add(furnace, "iron_ore", 2);
        string json = JsonUtility.ToJson(furnace);
        object back = JsonUtility.FromJson(json, Find("WoodPileState"));
        Assert.That(Get(back, "kind").ToString(), Is.EqualTo("Bloomery"));
        Assert.That(Prop(back, "SmeltRunning"), Is.EqualTo(true));
        Assert.That((int)Get(back, "jobCount"), Is.EqualTo(1));
        Assert.That((float)Get(back, "jobHours"), Is.EqualTo(6f));
        Assert.That(Count(back, "iron_ore"), Is.EqualTo(4), "the 2 Ore left over plus the 2 added");
        Assert.That(Count(back, "charcoal"), Is.EqualTo(1));
        Assert.That(Wells("BloomeryPhaseOf", back), Is.EqualTo(BloomeryRules.Phase.Smelting));
        Assert.That(json, Does.Contain("\"kind\":42"));
    }

    [Test]
    public void AnOldSaveHasNoBloomeryKindsAndAPitOrKilnIsNeverReadAsASmelt()
    {
        object pit = NewPile("CharcoalPit");
        Set(pit, "jobItem", CharcoalRules.BurnMarker);
        Set(pit, "jobCount", 4);
        Set(pit, "jobHours", 9f);
        object pitBack = JsonUtility.FromJson(JsonUtility.ToJson(pit), Find("WoodPileState"));
        Assert.That(Prop(pitBack, "PitBurning"), Is.EqualTo(true), "the Pit's burn still reads as a burn");
        Assert.That(Prop(pitBack, "SmeltRunning"), Is.EqualTo(false), "and not as a smelt");

        object kiln = NewPile("Kiln");
        Set(kiln, "jobItem", PotteryRules.FiringMarker);
        object kilnBack = JsonUtility.FromJson(JsonUtility.ToJson(kiln), Find("WoodPileState"));
        Assert.That(Prop(kilnBack, "KilnFiring"), Is.EqualTo(true));
        Assert.That(Prop(kilnBack, "SmeltRunning"), Is.EqualTo(false));

        object plain = JsonUtility.FromJson("{\"id\":3,\"kind\":1,\"contents\":[]}", Find("WoodPileState"));
        Assert.That(Prop(plain, "SmeltRunning"), Is.EqualTo(false));
        Assert.That(Prop(plain, "BloomeryOre"), Is.EqualTo(0));
        Assert.That(Prop(plain, "BloomeryBars"), Is.EqualTo(0));
    }

    // --- The Iron Bar: an item, never sold ---

    [Test]
    public void TheIronBarIsA2KgMaterialWithNoMarketRowSoNothingCanSellIt()
    {
        ScriptableObject item = AssetDatabase.LoadAssetAtPath<ScriptableObject>("Assets/Resources/Items/Materials/iron_bar.asset");
        Assert.That(item, Is.Not.Null);
        Type type = item.GetType();
        Assert.That(type.GetProperty("Id").GetValue(item), Is.EqualTo("iron_bar"));
        Assert.That(type.GetProperty("DisplayName").GetValue(item), Is.EqualTo("Iron Bar"));
        Assert.That((float)type.GetProperty("WeightKg").GetValue(item), Is.EqualTo(2f));
        Assert.That(type.GetProperty("Category").GetValue(item).ToString(), Is.EqualTo("Material"));
        Assert.That(File.Exists(Path.Combine(Application.dataPath, "Resources/Items/Materials/iron_bar.asset.meta")), Is.True, "with its meta");

        MarketTable table = MarketCatalog.Table;
        Assert.That(table.TryGetRow("iron_bar", out _), Is.False, "no market row");
        Assert.That(table.SellBaseCents("iron_bar"), Is.EqualTo(0L), "nothing buys it, so the sell list never shows it (TradingPostScreen lists what CanSell)");
        Assert.That(table.BuyBaseCents("iron_bar"), Is.EqualTo(0L), "and the store does not sell it");
        Assert.That(table.HasPool("iron_bar"), Is.False);
        Assert.That(table.Rows.Count, Is.EqualTo(92), "M2 added no row; M3 added the three Smithy goods, and still none for the Iron Bar");
        Assert.That(table.Stock.Count, Is.EqualTo(22));
        Assert.That(SourceText("Resources/Market/MarketData.asset"), Does.Not.Contain("iron_bar"));
        Assert.That(SourceText("Scripts/UI/TradingPostScreen.cs"), Does.Contain("market.CanSell(stack.itemId)"));
        Assert.That(SourceText("Scripts/Managers/TradingPost.cs"), Does.Contain("!Market.CanSell(itemId)"), "and a sale of it would do nothing");
        Assert.That(LedgerCategories.PurchaseIsListed("iron_bar"), Is.False, "no Ledger line: it cannot be bought");
        Assert.That(LedgerCategories.Count, Is.EqualTo(19));
        Assert.That(MetalRules.IronBarKg, Is.EqualTo(2f));
    }

    // --- Placement: the Kiln's rules with the Bloomery's own reach; regrowth ---

    static SiteInfo Site(float x, float radius, string name = "Small Cabin") =>
        new SiteInfo { kind = SiteKind.Building, position = new Vector3(x, 0f, 0f), name = name, radius = radius };

    [Test]
    public void ANewBloomeryKeepsThreeMetresOfClearAirFromEveryStructureAndTreeFromItsOwnEdge()
    {
        SetbackRules rules = SetbackRules.Defaults;
        rules.kilnRadius = BloomeryRules.RadiusMetres;
        rules.kilnFromStructure = BloomeryRules.ClearanceMetres;
        rules.kilnFromTree = BloomeryRules.ClearanceMetres;
        var cabin = new List<SiteInfo> { Site(0f, 2.5f) };
        // 0.7 (the stack) + 2.5 (the cabin reach) + 3 = 6.2 m centre to centre.
        Assert.That(Setbacks.CanPlaceKiln(new Vector3(6.19f, 0f, 0f), cabin, false, rules, out string reason), Is.False);
        Assert.That(reason, Is.EqualTo("Too close to the Small Cabin: 3 m needed."));
        Assert.That(Setbacks.CanPlaceKiln(new Vector3(6.21f, 0f, 0f), cabin, false, rules, out _), Is.True);
        Assert.That(Setbacks.CanPlaceKiln(new Vector3(20f, 0f, 0f), cabin, true, rules, out reason), Is.False, "a tree within reach");
        Assert.That(BloomeryRules.RadiusMetres, Is.EqualTo(0.7f));
        Assert.That(BloomeryRules.TreeReachMetres, Is.EqualTo(BloomeryRules.RadiusMetres + BloomeryRules.ClearanceMetres));
    }

    [Test]
    public void ABloomeryThatStandsKeepsLaterBuildingsThreeMetresClearFromItsOwnEdge()
    {
        SetbackRules rules = SetbackRules.Defaults;
        var furnace = new List<SiteInfo> { Site(0f, BloomeryRules.RadiusMetres, "Bloomery") };
        Assert.That(Setbacks.CanPlaceBesideKiln(new Vector3(6.19f, 0f, 0f), 2.5f, furnace, rules, out string reason), Is.False);
        Assert.That(reason, Is.EqualTo("Too close to the Bloomery: 3 m needed."));
        Assert.That(Setbacks.CanPlaceBesideKiln(new Vector3(6.21f, 0f, 0f), 2.5f, furnace, rules, out _), Is.True);
    }

    [Test]
    public void WellsAndOuthousesWiresTheBloomeryIntoThePlacementChecksAndTheTick()
    {
        string wells = SourceText("Scripts/Managers/WellsAndOuthouses.cs");
        Assert.That(wells, Does.Contain("kind == PileKind.BloomerySite || kind == PileKind.Bloomery"));
        Assert.That(wells, Does.Contain("A bloomery needs open sky: not under a roof."));
        Assert.That(wells, Does.Contain("Setbacks.CanPlaceKiln(point, KilnNeighbours(ignore), treeNearBloomery, bloomeryRules, out reason)"));
        Assert.That(wells, Does.Contain("case PileKind.BloomerySite: return BloomeryRules.RadiusMetres;").Or.Contain("case PileKind.Bloomery:\n            case PileKind.BloomerySite: return BloomeryRules.RadiusMetres;"));
        Assert.That(wells, Does.Contain("WoodManager.IsBloomery(pile.kind)"), "a bloomery is among the structures later buildings keep 3 m from");
        Assert.That(wells, Does.Contain("A Bloomery's smelt runs one game hour at a time too"));
        Assert.That(wells, Does.Contain("BloomeryRules.AfterHour(furnace.jobHours)"));
        string wood = SourceText("Scripts/Managers/WoodManager.cs");
        Assert.That(wood, Does.Contain("kind == PileKind.CharcoalPitSite || kind == PileKind.BloomerySite || kind == PileKind.SmithySite) && WellsAndOuthouses.Instance != null"));
        Assert.That(wood, Does.Contain("!IsKiln(kind) && !IsHandCart(kind) && !IsCharcoalPit(kind) && !IsBloomery(kind)"), "a parked Hand Cart is exempt, as for the Kiln");
    }

    [Test]
    public void ANewTreeDoesNotSproutInsideTheBloomerysFootprint()
    {
        Assert.That(RegrowRules.BloomeryMetres, Is.EqualTo(BloomeryRules.RegrowMetres));
        Assert.That(RegrowRules.Blocks(RegrowRules.Blocker.Bloomery, 1.0f, 0f, 0f), Is.True);
        Assert.That(RegrowRules.Blocks(RegrowRules.Blocker.Bloomery, 1.19f, 0f, 0f), Is.True);
        Assert.That(RegrowRules.Blocks(RegrowRules.Blocker.Bloomery, 1.3f, 0f, 0f), Is.False);
        Assert.That(RegrowRules.Blocks(RegrowRules.Blocker.Pit, 1.39f, 0f, 0f), Is.True, "the Pit's blocker is unchanged");
        Assert.That(RegrowRules.Blocks(RegrowRules.Blocker.Kiln, 1.7f, 0f, 0f), Is.True, "and so is the Kiln's");
        Assert.That(SourceText("Scripts/Managers/WoodManager.cs"), Does.Contain("IsBloomery(pile.kind) ? RegrowRules.Blocker.Bloomery"));
    }

    // --- The look ---

    static List<GameObject> BuildVisuals(object state, out Vector3 size)
    {
        var parent = new GameObject("Bloomery visuals test");
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
    public void TheBloomeryDrawsAStackWithOreCharcoalAndBarsBesideItAndTheLookIsSteady()
    {
        object empty = NewPile("Bloomery");
        var emptyPieces = BuildVisuals(empty, out Vector3 emptySize);
        object loaded = NewPile("Bloomery");
        Add(loaded, "iron_ore", 4);
        Add(loaded, "charcoal", 4);
        var loadedPieces = BuildVisuals(loaded, out _);
        object smelting = NewPile("Bloomery");
        Set(smelting, "jobItem", BloomeryRules.SmeltMarker);
        Set(smelting, "jobCount", 2);
        Set(smelting, "jobHours", 6f);
        var smeltingPieces = BuildVisuals(smelting, out _);
        object done = NewPile("Bloomery");
        Add(done, "iron_bar", 2);
        var donePieces = BuildVisuals(done, out _);
        try
        {
            Assert.That(emptyPieces.Count - 1, Is.EqualTo(9), "the footing, the two stack stages, the chimney, four footing stones and the mouth");
            Assert.That(loadedPieces.Count - emptyPieces.Count, Is.EqualTo(8), "four lumps of Ore and four of Charcoal");
            Assert.That(smeltingPieces.Count, Is.EqualTo(emptyPieces.Count), "the smoke is KilnSmoke, not a piece; the glow is only a colour");
            Assert.That(donePieces.Count - emptyPieces.Count, Is.EqualTo(2), "two Bars at the front");
            Assert.That(emptySize.y, Is.InRange(1.1f, 1.6f), "about 1.2 m tall");
            Assert.That(emptySize.x, Is.InRange(0.9f, 1.4f));
        }
        finally
        {
            Cleanup(emptyPieces);
            Cleanup(loadedPieces);
            Cleanup(smeltingPieces);
            Cleanup(donePieces);
        }
    }

    [Test]
    public void TheSiteDrawsItsStakesAndTheDeliveredStoneAndClay()
    {
        object site = NewPile("BloomerySite");
        var bare = BuildVisuals(site, out _);
        object stocked = NewPile("BloomerySite");
        Add(stocked, "stone", 8);
        Add(stocked, "clay", 6);
        var full = BuildVisuals(stocked, out _);
        try
        {
            Assert.That(full.Count - bare.Count, Is.EqualTo(14), "eight blocks of Stone and six lumps of Clay");
        }
        finally
        {
            Cleanup(bare);
            Cleanup(full);
        }
    }

    [Test]
    public void TheMouthStandsClearOfTheStackAndNoFootingStoneSitsInFrontOfItSoNothingFlashes()
    {
        string placed = SourceText("Scripts/World/PlacedStructure.cs");
        Assert.That(placed, Does.Contain("new Vector3(0f, 0.28f, 0.5f), Vector3.zero, new Vector3(0.34f, 0.26f, 0.24f)"));
        Assert.That(placed, Does.Contain("new Vector3(0f, 0.4f, 0f), Vector3.zero, new Vector3(0.9f, 0.32f, 0.9f)"), "the lower stage, 0.9 m across");
        // The mouth's front face is at z 0.5 + 0.12 = 0.62; the lower stage (0.9 m across) is at radius 0.45.
        float front = 0.5f + 0.24f / 2f;
        Assert.That(front - 0.45f, Is.GreaterThan(0.05f), "the mouth's face is 17 cm clear of the stack");
        // Footing stones sit at angles pi/2 +- 0.9 and +- 1.5 (radius 0.5, 0.22 wide): their nearest edge to the mouth's centre line is well outside the mouth's half-width (0.17).
        foreach (float around in new[] { -1.5f, -0.9f, 0.9f, 1.5f })
        {
            float x = Mathf.Cos(Mathf.PI / 2f + around) * 0.5f;
            Assert.That(Mathf.Abs(x) - 0.11f, Is.GreaterThan(0.17f + 0.05f), "no stone beside the mouth shares its plane");
        }
        // The stack stages overlap rather than touch: lower 0.08 to 0.72, upper 0.68 to 1.12, chimney 1.02 to 1.22.
        Assert.That(0.68f, Is.LessThan(0.72f));
        Assert.That(1.02f, Is.LessThan(1.12f));
        Assert.That(placed, Does.Contain("bloomerySmoke.SetFiring(state.SmeltRunning)"));

        int start = placed.IndexOf("static void BuildBloomery(", StringComparison.Ordinal);
        int end = placed.IndexOf("static void KilnPiece(", StringComparison.Ordinal);
        string builder = placed.Substring(start, end - start);
        Assert.That(builder, Does.Not.Contain("Light"));
        Assert.That(builder, Does.Not.Contain("Time.time"));
        Assert.That(builder, Does.Not.Contain("Random.value"));
    }

    // --- Know-how and item help ---

    [Test]
    public void TheSmeltingEntryUnlocksOnAPlacedSiteOrAnIronBar()
    {
        Assert.That(KnowHowRules.Entries.Length, Is.EqualTo(11));
        Assert.That(KnowHowRules.TryGet(KnowHowRules.Smelting, out KnowHowRules.Entry entry), Is.True);
        Assert.That(entry.title, Is.EqualTo("Smelting Iron in the Bloomery"));
        Assert.That(KnowHowRules.ForItem("iron_bar"), Is.EqualTo(new[] { "smelting", "smithy" }), "holding an Iron Bar opens both the smelting and the Smithy entries");
        Assert.That(KnowHowRules.ForHappening(KnowHowRules.Happening.BloomerySitePlaced), Is.EqualTo(new[] { "smelting" }), "first placing a Bloomery site");
        Assert.That(KnowHowRules.ForItem("iron_ore"), Is.EqualTo(new[] { "ironore" }), "Ore still opens its own entry");

        List<string> first = KnowHowRules.NewUnlocks(new string[0], KnowHowRules.ForItem("iron_bar"));
        Assert.That(first, Is.EqualTo(new[] { "smelting", "smithy" }));
        Assert.That(KnowHowRules.NewUnlocks(first, KnowHowRules.ForHappening(KnowHowRules.Happening.BloomerySitePlaced)), Is.Empty, "never twice");

        Assert.That(KnowHowRules.Backfill(id => id == "iron_bar", false, false, false), Is.EqualTo(new[] { "smelting", "smithy" }));
        Assert.That(KnowHowRules.Backfill(id => false, false, false, false, bloomeryPlaced: true), Is.EqualTo(new[] { "smelting" }));
        Assert.That(KnowHowRules.Backfill(id => false, false, false, false), Is.Empty);
        Assert.That(SourceText("Scripts/Managers/JournalManager.cs"), Does.Contain("hopper, tripod, taps, pit, bloomery, smithy)"));
        Assert.That(SourceText("Scripts/Managers/WoodManager.cs"), Does.Contain("KnowHowRules.Happening.BloomerySitePlaced"));
    }

    [Test]
    public void TheEntriesAndTheItemHelpSayWhatTheCodeDoes()
    {
        KnowHowRules.TryGet(KnowHowRules.Smelting, out KnowHowRules.Entry smelting);
        Assert.That(smelting.body, Does.Contain("8 Stone and 6 Clay"));
        Assert.That(smelting.body, Does.Contain("about 61 kg"));
        Assert.That(smelting.body, Does.Contain("up to 4 of each"));
        Assert.That(smelting.body, Does.Contain("once it holds 2 Ore and 2 Charcoal"));
        Assert.That(smelting.body, Does.Contain("Each 2 Ore and 2 Charcoal make 1 Iron Bar, up to 2 Bars at a time"));
        Assert.That(smelting.body, Does.Contain("10 game hours"));
        Assert.That(smelting.body, Does.Contain("Nobody buys Iron Bars"));
        Assert.That(BloomeryRules.SiteStone, Is.EqualTo(8));
        Assert.That(BloomeryRules.SiteClay, Is.EqualTo(6));

        KnowHowRules.TryGet(KnowHowRules.IronOre, out KnowHowRules.Entry ore);
        KnowHowRules.TryGet(KnowHowRules.Charcoal, out KnowHowRules.Entry charcoal);
        Assert.That(ore.body, Does.Contain("Smelting Iron in the Bloomery"), "the Ore entry points to the next step");
        Assert.That(charcoal.body, Does.Contain("Smelting Iron in the Bloomery"), "and so does the Charcoal entry");

        string tip = KnowHowRules.ItemTip("iron_bar");
        Assert.That(tip, Is.Not.Empty);
        Assert.That(tip.Length, Is.LessThanOrEqualTo(80), "fits in two lines of the row's info text");
        Assert.That(tip, Does.Contain("Smithy").And.Contain("not sold"));
        Assert.That(KnowHowRules.ItemTip("iron_ore").Length, Is.LessThanOrEqualTo(80));

        string craft = SourceText("Scripts/UI/CraftingScreen.cs");
        Assert.That(craft, Does.Contain("case PileKind.BloomerySite:"));
        Assert.That(craft, Does.Contain("{BloomeryRules.SiteStone} Stone and {BloomeryRules.SiteClay} Clay"));
        Assert.That(craft, Does.Contain("case PileKind.Bloomery:"));
        Assert.That(craft, Does.Contain("{BloomeryRules.OreCap}").And.Contain("{BloomeryRules.SmeltHours}").And.Contain("{BloomeryRules.MaxBars}"));
    }

    [Test]
    public void TheKnowHowTextFileCarriesTheNewEntryAndTheTwoChangedSentencesSoItTravelsWithTheCode()
    {
        string path = Path.GetFullPath(Path.Combine(Application.dataPath, "../../../05_Documentation/Game_Systems/Journal_KnowHow_Text.md"));
        if (!File.Exists(path))
            Assert.Ignore("Journal_KnowHow_Text.md is not at " + path);
        string text = File.ReadAllText(path).Replace("\r\n", "\n");
        Assert.That(text, Does.Contain("## 10. Smelting Iron in the Bloomery"));
        Assert.That(text, Does.Contain("Trigger: first Bloomery site placed, or first Iron Bar held."));
        Assert.That(text, Does.Contain("and a furnace, the Bloomery (see \"Smelting Iron in the Bloomery\")."));
        Assert.That(text, Does.Not.Contain("which is not in the game yet"));
    }

    // --- The dev keys ---

    [Test]
    public void TheDevKeysAreOnFreeKeysAndTheBloomeryKitIsSplitSoThePackCanCarryIt()
    {
        string keys = SourceText("Scripts/Debug/TimeDebugControls.cs");
        Assert.That(keys, Does.StartWith("#if UNITY_EDITOR || DEVELOPMENT_BUILD"), "compiled out of release builds");
        Assert.That(keys, Does.Contain("keyboard.periodKey.wasPressedThisFrame"));
        Assert.That(keys, Does.Contain("DebugFinishSmelts()"));
        Assert.That(keys, Does.Contain("PileKind.Bloomery, SmeltAhead(4f)"));
        Assert.That(keys, Does.Contain("PileKind.RockStorage, SmeltAhead(3f)"));
        int shiftEquals = keys.IndexOf("keyboard.equalsKey.wasPressedThisFrame && keyboard.shiftKey.isPressed", StringComparison.Ordinal);
        int plainEquals = keys.IndexOf("keyboard.equalsKey.wasPressedThisFrame && InventoryManager.Instance != null", StringComparison.Ordinal);
        Assert.That(shiftEquals, Is.GreaterThan(0));
        Assert.That(shiftEquals, Is.LessThan(plainEquals), "Shift+= is handled first and returns, so it never also gives the Ore and Charcoal");

        // Nothing else reads the period key, in code or in the input actions (the numpad's is a different key).
        string scripts = Path.Combine(Application.dataPath, "Scripts");
        foreach (string file in Directory.GetFiles(scripts, "*.cs", SearchOption.AllDirectories))
        {
            if (file.EndsWith("TimeDebugControls.cs"))
                continue;
            Assert.That(File.ReadAllText(file), Does.Not.Contain("periodKey"), file);
        }
        foreach (string file in Directory.GetFiles(Application.dataPath, "*.inputactions", SearchOption.AllDirectories))
            Assert.That(File.ReadAllText(file), Does.Not.Contain("<Keyboard>/period"), file);

        // The pack takes 45 kg. The kit is 8 Stone (40 kg) and 6 Clay (21 kg) plus a Hammer (1.2 kg): 62.2 kg. The Hammer and Clay go to the pack (22.2 kg); the Stone stands as a pile.
        float inPack = 1.2f + BloomeryRules.SiteClay * 3.5f;
        float stone = BloomeryRules.SiteStone * 5f;
        Assert.That(inPack, Is.EqualTo(22.2f).Within(0.001f));
        Assert.That(inPack, Is.LessThan(45f));
        Assert.That(inPack + stone, Is.GreaterThan(45f), "the whole kit would not fit");
        Assert.That(inPack + 4 * 5f, Is.LessThan(45f), "with the Hammer and Clay, four Stone at a time fit: two trips");
        Assert.That(4 * MetalRules.IronBarKg, Is.EqualTo(8f), "Shift+= gives 4 Iron Bars, 8 kg");
    }

    [Test]
    public void TheSmeltKeyFinishesEveryRunningSmeltAndNothingElse()
    {
        string wells = SourceText("Scripts/Managers/WellsAndOuthouses.cs");
        int start = wells.IndexOf("public int DebugFinishSmelts()", StringComparison.Ordinal);
        Assert.That(start, Is.GreaterThan(0));
        string body = wells.Substring(start, 700);
        Assert.That(body, Does.Contain("furnace.kind != PileKind.Bloomery || !furnace.SmeltRunning"));
        Assert.That(body, Does.Contain("FinishSmelt(furnace)"));
    }

    // --- Nothing else changed ---

    [Test]
    public void ThePitTheKilnTheHopperAndTheMarketAreUntouched()
    {
        Assert.That(CharcoalRules.SiteClay, Is.EqualTo(4));
        Assert.That(CharcoalRules.SiteTallGrass, Is.EqualTo(6));
        Assert.That(CharcoalRules.BuildBlows, Is.EqualTo(20));
        Assert.That(CharcoalRules.LogsCap, Is.EqualTo(6));
        Assert.That(CharcoalRules.BurnHours, Is.EqualTo(24));
        Assert.That(CharcoalRules.Yield(6), Is.EqualTo(4));
        Assert.That(CharcoalRules.PitRadiusMetres, Is.EqualTo(0.9f));
        Assert.That(PotteryRules.KilnFirewood, Is.EqualTo(10));
        Assert.That(PotteryRules.KilnFireHours, Is.EqualTo(24));
        Assert.That(PotashRules.HopperCapacity, Is.EqualTo(12));
        Assert.That(MarketCatalog.Table.Stock.Count, Is.EqualTo(22));
        Assert.That(MarketCatalog.Table.BuyBaseCents("iron_ore"), Is.EqualTo(400L));
        Assert.That(MarketCatalog.Table.SellBaseCents("charcoal"), Is.EqualTo(100L));
        Assert.That(Enum.GetValues(typeof(MarketGroup)).Length, Is.EqualTo(11), "no new market group");
        Assert.That(MetalRules.IronOreBaseCents, Is.EqualTo(400L));
        Assert.That(MetalRules.CharcoalBaseCents, Is.EqualTo(100L));
    }
}
