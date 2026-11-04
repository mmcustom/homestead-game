using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

// EditMode tests for Metalworking phase M3 (Mike, 2026-11-04): the Smithy, the Smithy screen and the first three iron goods. SmithyRules is pure (the recipe table, the caps, the gates, the
// job's hours, the screen's words and keys); the main-assembly parts (the saved pile, the staged site, the kinds, the visuals, the screen) are reached by reflection as the other world tests
// do. Nothing here touches the Saves folder or a scene.
public class SmithyTests
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

    static bool Working(object pile) => !string.IsNullOrEmpty((string)Get(pile, "jobItem")) && (float)Get(pile, "jobHours") > 0f;

    static bool Waiting(object pile) => SmithyRules.Recipes.Any(r => Count(pile, r.itemId) > 0);

    static SmithyRules.Gate Gate(object pile, SmithyRules.Recipe recipe) =>
        SmithyRules.CanStart(recipe, Working(pile), Waiting(pile), Count(pile, "iron_bar"), Count(pile, "charcoal"));

    // What PlacedStructure.TryStartForge does to the saved pile, step for step (the same gate, the same takes, the same job fields).
    static bool Start(object forge, SmithyRules.Recipe recipe)
    {
        if (Gate(forge, recipe) != SmithyRules.Gate.Ok)
            return false;
        Take(forge, "iron_bar", recipe.bars);
        Take(forge, "charcoal", recipe.charcoal);
        Set(forge, "jobItem", recipe.itemId);
        Set(forge, "jobHours", (float)recipe.hours);
        return true;
    }

    static void RunHours(object forge, int hours)
    {
        for (int i = 0; i < hours; i++)
        {
            if (string.IsNullOrEmpty((string)Get(forge, "jobItem")))
                return;
            Set(forge, "jobHours", SmithyRules.AfterHour((float)Get(forge, "jobHours")));
            if (SmithyRules.IsDone((float)Get(forge, "jobHours")))
                Wells("FinishForgeJob", forge);
        }
    }

    static SmithyRules.Recipe Hinges => SmithyRules.Recipes[0];
    static SmithyRules.Recipe Hooks => SmithyRules.Recipes[1];
    static SmithyRules.Recipe Nails => SmithyRules.Recipes[2];

    // --- The recipe table ---

    [Test]
    public void TheRecipeTableHasMikesThreeGoodsInOrderAndNoSkilletOrTools()
    {
        Assert.That(SmithyRules.Recipes.Length, Is.EqualTo(3));
        Assert.That(SmithyRules.Recipes.Select(r => r.itemId), Is.EqualTo(new[] { "door_hinges", "hook_set", "nails_box" }));
        Assert.That(SmithyRules.Recipes.Select(r => r.name), Is.EqualTo(new[] { "Door Hinges", "Hook Set", "Box of Nails (20)" }));
        Assert.That(SmithyRules.Recipes.Select(r => r.hours), Is.EqualTo(new[] { 3, 2, 2 }));
        Assert.That(SmithyRules.Recipes.Select(r => r.baseCents), Is.EqualTo(new[] { 2600L, 1800L, 1600L }));
        Assert.That(SmithyRules.Recipes.Select(r => r.weightKg), Is.EqualTo(new[] { 0.8f, 0.5f, 0.6f }));
        Assert.That(SmithyRules.Recipes.All(r => r.bars == 1 && r.charcoal == 1), Is.True, "1 Iron Bar and 1 Charcoal each");
        Assert.That(SmithyRules.Recipes.All(r => r.metalId == MetalRules.IronId), Is.True);
        Assert.That(SmithyRules.Recipes.Any(r => r.itemId.Contains("skillet")), Is.False, "the Iron Skillet is left out for now");
        Assert.That(SmithyRules.Recipes.Any(r => r.itemId.Contains("shovel") || r.itemId.Contains("pick")), Is.False, "and the iron tools (M4)");
    }

    [Test]
    public void TheTableIsKeyedByGoodAndEachRowNamesARealMetal()
    {
        Assert.That(SmithyRules.Recipes.Select(r => r.itemId).Distinct().Count(), Is.EqualTo(SmithyRules.Recipes.Length), "ids are unique");
        foreach (SmithyRules.Recipe recipe in SmithyRules.Recipes)
        {
            Assert.That(MetalRules.TryFind(recipe.metalId, out MetalRules.Metal metal), Is.True, recipe.itemId);
            Assert.That(recipe.bars, Is.GreaterThan(0));
            Assert.That(metal.fuelId, Is.EqualTo(SmithyRules.CharcoalId), "every metal burns Charcoal here");
            Assert.That(SmithyRules.TryFind(recipe.itemId, out SmithyRules.Recipe found), Is.True);
            Assert.That(found.name, Is.EqualTo(recipe.name));
            Assert.That(SmithyRules.IsProduct(recipe.itemId), Is.True);
        }
        Assert.That(SmithyRules.TryFind("nope", out _), Is.False);
        Assert.That(SmithyRules.IsProduct("iron_bar"), Is.False);
        Assert.That(SmithyRules.BarId, Is.EqualTo(MetalRules.IronBarId));
    }

    [Test]
    public void TheForgeHoldsEightBarsAndEightCharcoalAndTakesNothingElse()
    {
        Assert.That(SmithyRules.BarsCap, Is.EqualTo(8));
        Assert.That(SmithyRules.CharcoalCap, Is.EqualTo(8));
        Assert.That(SmithyRules.StockIds, Is.EqualTo(new[] { "iron_bar", "charcoal" }));
        Assert.That(SmithyRules.IsStockId("iron_bar"), Is.True);
        Assert.That(SmithyRules.IsStockId("iron_ore"), Is.False);
        Assert.That(SmithyRules.CapOf("iron_bar"), Is.EqualTo(8));
        Assert.That(SmithyRules.CapOf("logs"), Is.EqualTo(0));
        Assert.That(SmithyRules.Accepts("iron_bar", 7), Is.True);
        Assert.That(SmithyRules.Accepts("iron_bar", 8), Is.False);
        Assert.That(SmithyRules.Accepts("charcoal", 7), Is.True);
        Assert.That(SmithyRules.Accepts("charcoal", 8), Is.False);
        Assert.That(SmithyRules.Accepts("door_hinges", 0), Is.False, "a finished good is not something to put back");
        Assert.That(SmithyRules.Accepts("iron_ore", 0), Is.False);
    }

    [Test]
    public void TheNumbersAreMikes()
    {
        Assert.That(SmithyRules.SiteStone, Is.EqualTo(6));
        Assert.That(SmithyRules.SiteClay, Is.EqualTo(3));
        Assert.That(SmithyRules.SiteLogs, Is.EqualTo(2));
        Assert.That(SmithyRules.SiteBranches, Is.EqualTo(4));
        Assert.That(SmithyRules.SiteTallGrass, Is.EqualTo(6));
        Assert.That(SmithyRules.BuildBlows, Is.EqualTo(40));
        Assert.That(SmithyRules.FireClearanceMetres, Is.EqualTo(3f));
        Assert.That(SmithyRules.ReachMetres, Is.EqualTo(1.9f), "the Workshop's reach");
        Assert.That(SmithyRules.TreeReachMetres, Is.EqualTo(SmithyRules.ReachMetres + SmithyRules.FireClearanceMetres));
        Assert.That(SmithyRules.RegrowMetres, Is.EqualTo(RegrowRules.WorkshopMetres), "the Workshop's footprint");
    }

    // --- The gates and the hours ---

    [Test]
    public void AJobStartsOnlyWithTheForgeIdleNothingWaitingAndABarAndCharcoalIn()
    {
        Assert.That(SmithyRules.CanStart(Hinges, false, false, 1, 1), Is.EqualTo(SmithyRules.Gate.Ok));
        Assert.That(SmithyRules.CanStart(Hinges, true, false, 5, 5), Is.EqualTo(SmithyRules.Gate.Busy), "one job at a time");
        Assert.That(SmithyRules.CanStart(Hinges, false, true, 5, 5), Is.EqualTo(SmithyRules.Gate.ProductWaiting), "collect the good first");
        Assert.That(SmithyRules.CanStart(Hinges, true, true, 5, 5), Is.EqualTo(SmithyRules.Gate.Busy), "busy wins over a waiting good");
        Assert.That(SmithyRules.CanStart(Hinges, false, false, 0, 5), Is.EqualTo(SmithyRules.Gate.ShortOfStock));
        Assert.That(SmithyRules.CanStart(Hinges, false, false, 5, 0), Is.EqualTo(SmithyRules.Gate.ShortOfStock));
        Assert.That(SmithyRules.CanStart(Hinges, false, false, 0, 0), Is.EqualTo(SmithyRules.Gate.ShortOfStock));
        Assert.That(typeof(SmithyRules).GetMethod("CanStart").GetParameters().Select(p => p.ParameterType.Name), Does.Not.Contain("Boolean").Or.Not.Empty, "no tool argument");
        Assert.That(typeof(SmithyRules).GetMethod("CanStart").GetParameters().Length, Is.EqualTo(5), "recipe, working, waiting, bars, charcoal: no tool is needed in the pack");
    }

    [Test]
    public void AJobTakesItsBarAndCharcoalAtTheStartAndMakesOneGoodAfterItsHours()
    {
        foreach (SmithyRules.Recipe recipe in SmithyRules.Recipes)
        {
            object forge = NewPile("Smithy");
            Add(forge, "iron_bar", 3);
            Add(forge, "charcoal", 5);
            Assert.That(Start(forge, recipe), Is.True, recipe.name);
            Assert.That(Count(forge, "iron_bar"), Is.EqualTo(2), "one Bar taken at the start");
            Assert.That(Count(forge, "charcoal"), Is.EqualTo(4), "and one Charcoal");
            Assert.That(Get(forge, "jobItem"), Is.EqualTo(recipe.itemId));
            Assert.That((float)Get(forge, "jobHours"), Is.EqualTo((float)recipe.hours));
            Assert.That(Count(forge, recipe.itemId), Is.EqualTo(0), "nothing yet");

            RunHours(forge, recipe.hours - 1);
            Assert.That(Count(forge, recipe.itemId), Is.EqualTo(0), $"not before the last hour of {recipe.name}");
            Assert.That(Working(forge), Is.True);
            RunHours(forge, 1);
            Assert.That(Count(forge, recipe.itemId), Is.EqualTo(1), $"one {recipe.name} after {recipe.hours} h");
            Assert.That(Get(forge, "jobItem"), Is.EqualTo(""));
            Assert.That((float)Get(forge, "jobHours"), Is.EqualTo(0f));
            Assert.That(Count(forge, "iron_bar"), Is.EqualTo(2));
            Assert.That(Count(forge, "charcoal"), Is.EqualTo(4));
        }
    }

    [Test]
    public void AFinishedGoodWaitsInTheSmithyAndBlocksTheNextJobUntilItIsTaken()
    {
        object forge = NewPile("Smithy");
        Add(forge, "iron_bar", 2);
        Add(forge, "charcoal", 2);
        Assert.That(Start(forge, Hooks), Is.True);
        RunHours(forge, 2);
        Assert.That(Count(forge, "hook_set"), Is.EqualTo(1));
        Assert.That(Gate(forge, Nails), Is.EqualTo(SmithyRules.Gate.ProductWaiting), "stock is there, but the Hook Set must be taken first");
        Assert.That(Start(forge, Nails), Is.False);

        Take(forge, "hook_set", 1);
        Assert.That(Gate(forge, Nails), Is.EqualTo(SmithyRules.Gate.Ok), "taken: the forge is free again");
        Assert.That(Start(forge, Nails), Is.True);
        Assert.That(Start(forge, Hinges), Is.False, "a second job cannot start while one runs");
        Assert.That(Gate(forge, Hinges), Is.EqualTo(SmithyRules.Gate.Busy));
        RunHours(forge, 2);
        Assert.That(Count(forge, "nails_box"), Is.EqualTo(1));
        Assert.That(Count(forge, "iron_bar"), Is.EqualTo(0));
        Assert.That(Count(forge, "charcoal"), Is.EqualTo(0));
    }

    [Test]
    public void AJobCountsDownOneGameHourAtATimeAndAnIdleForgeStaysAtZero()
    {
        float hours = 3f;
        int ticks = 0;
        while (!SmithyRules.IsDone(hours))
        {
            hours = SmithyRules.AfterHour(hours);
            ticks++;
        }
        Assert.That(ticks, Is.EqualTo(3));
        Assert.That(SmithyRules.AfterHour(0f), Is.EqualTo(0f));
        Assert.That(SmithyRules.IsDone(1f), Is.False);
        string wells = SourceText("Scripts/Managers/WellsAndOuthouses.cs");
        Assert.That(wells, Does.Contain("forge.jobHours = SmithyRules.AfterHour(forge.jobHours);"), "the hourly tick, so sleeping, being away and skipping days all count");
        Assert.That(wells, Does.Contain("forge.kind != PileKind.Smithy || string.IsNullOrEmpty(forge.jobItem)"));
    }

    [Test]
    public void AGoodThisBuildDoesNotKnowIsDroppedNotLeftInTheForge()
    {
        object forge = NewPile("Smithy");
        Set(forge, "jobItem", "something_from_the_future");
        Set(forge, "jobHours", 1f);
        Wells("FinishForgeJob", forge);
        Assert.That(Get(forge, "jobItem"), Is.EqualTo(""));
        Assert.That(((IList)Get(forge, "contents")).Count, Is.EqualTo(0));
    }

    // --- The kinds: appended after the Bloomery, nothing moved ---

    [Test]
    public void TheTwoSmithyKindsAreFortyThreeAndFortyFourAndNothingElseWasAdded()
    {
        Type kind = Find("PileKind");
        Assert.That(Enum.GetValues(kind).Length, Is.EqualTo(45));
        Assert.That((int)Enum.Parse(kind, "Bloomery"), Is.EqualTo(42), "nothing moved");
        Assert.That((int)Enum.Parse(kind, "SmithySite"), Is.EqualTo(43));
        Assert.That((int)Enum.Parse(kind, "Smithy"), Is.EqualTo(44));
        string[] names = Enum.GetNames(kind);
        Assert.That(names.Last(), Is.EqualTo("Smithy"));
        Assert.That(names.Any(n => n.Contains("Forge") || n.Contains("Anvil") || n.Contains("Skillet")), Is.False, "one combined Smithy: no separate Forge or Anvil kinds");
    }

    // --- The staged site ---

    [Test]
    public void TheSiteIsAStagedSiteOfFiveMaterialsThatBecomesTheSmithy()
    {
        Assert.That((bool)Wells("IsStagedSite", Kind("SmithySite")), Is.True);
        Assert.That((bool)Wells("IsStagedSite", Kind("Smithy")), Is.False);
        Assert.That(Wells("FinishedKind", Kind("SmithySite")).ToString(), Is.EqualTo("Smithy"));
        Assert.That((bool)Wells("RequiresDig", Kind("SmithySite")), Is.False, "RequiresDig is false: no Shovel stage, the Hammer builds it");

        object cost = Wells("FinishedCost", Kind("SmithySite"));
        var lines = ((IEnumerable)cost).Cast<object>().Select(c => (c.GetType().GetField("itemId").GetValue(c) as string, (int)c.GetType().GetField("count").GetValue(c))).ToArray();
        Assert.That(lines, Is.EqualTo(new[] { ("stone", 6), ("clay", 3), ("logs", 2), ("branches", 4), ("tall_grass", 6) }));
        Assert.That((float)Wells("WeightKg", cost), Is.EqualTo(61.1f).Within(0.001f), "6 Stone 30 kg, 3 Clay 10.5, 2 Logs 16, 4 Branches 4, 6 Tall Grass 0.6");
        Assert.That((bool)Wells("NeedsStaging", cost), Is.True, "far over the 15 kg staging line");

        var order = (string[][])Wells("DeliveryOrder", Kind("SmithySite"));
        Assert.That(order.Length, Is.EqualTo(1), "one group: all five in any order");
        Assert.That(order[0], Is.EquivalentTo(new[] { "stone", "clay", "logs", "branches", "tall_grass" }));

        object site = NewPile("SmithySite");
        foreach (string id in order[0])
            Assert.That(Accepts(site, id), Is.True, id);
        Assert.That(Accepts(site, "iron_bar"), Is.False);
        for (int i = 0; i < 6; i++)
            Add(site, "stone", 1);
        Assert.That(Accepts(site, "stone"), Is.False, "full of Stone");
        Assert.That(Accepts(site, "clay"), Is.True, "any order");
        Assert.That(Wells("SiteNeeds", site), Is.EqualTo("3 Clay, 2 Logs, 4 Branches, 6 Tall Grass"));
        Add(site, "clay", 3);
        Add(site, "logs", 2);
        Add(site, "branches", 4);
        Add(site, "tall_grass", 6);
        Assert.That(Wells("SiteNeeds", site), Is.Null, "fully stocked: the Hammer finishes it");
        Assert.That(Wells("FinishedMessage", Kind("SmithySite")), Is.EqualTo(SmithyRules.BuiltMessage));
    }

    [Test]
    public void TheSiteIsInTheBuildListAndTheSmithyIsAWideShedWithTheWorkshopsClearances()
    {
        var buildable = ((IEnumerable)Find("WoodManager").GetField("Buildable").GetValue(null)).Cast<object>().Select(o => o.ToString()).ToArray();
        Assert.That(buildable, Does.Contain("SmithySite"));
        Assert.That(buildable, Does.Not.Contain("Smithy"));
        Type wood = Find("WoodManager");
        Assert.That(wood.GetMethod("PileName").Invoke(null, new[] { Kind("SmithySite") }), Is.EqualTo("Smithy Site"));
        Assert.That(wood.GetMethod("PileName").Invoke(null, new[] { Kind("Smithy") }), Is.EqualTo("Smithy"));
        Assert.That(Prop(NewPile("Smithy"), "IsPlacedStructure"), Is.EqualTo(true));
        Assert.That(Prop(NewPile("SmithySite"), "IsPlacedStructure"), Is.EqualTo(true));
        Assert.That(wood.GetMethod("IsSmithy").Invoke(null, new[] { Kind("SmithySite") }), Is.EqualTo(true));
        Assert.That(wood.GetMethod("IsSmithy").Invoke(null, new[] { Kind("Workshop") }), Is.EqualTo(false));
        Assert.That(wood.GetMethod("IsWideShed").Invoke(null, new[] { Kind("Smithy") }), Is.EqualTo(true), "the Workshop's spacing, trunk-free footprint and pair rule");
        Assert.That(wood.GetMethod("IsWideShed").Invoke(null, new[] { Kind("Workshop") }), Is.EqualTo(true), "unchanged");
        Assert.That(wood.GetMethod("IsWideShed").Invoke(null, new[] { Kind("HandCart") }), Is.EqualTo(false));
        Assert.That(wood.GetMethod("IsKiln").Invoke(null, new[] { Kind("Smithy") }), Is.EqualTo(false));
        Assert.That(wood.GetMethod("IsBloomery").Invoke(null, new[] { Kind("Smithy") }), Is.EqualTo(false));
        Assert.That(SourceText("Scripts/Player/PlacementMode.cs"), Does.Contain("case PileKind.SmithySite:"), "the mouse wheel turns the site");
    }

    // --- The forge's stock ---

    [Test]
    public void TheSmithyTakesBarsAndCharcoalUpToEightEvenWhileAJobRunsAndNoGoods()
    {
        object forge = NewPile("Smithy");
        Assert.That(Accepts(forge, "iron_bar"), Is.True);
        Assert.That(Accepts(forge, "charcoal"), Is.True);
        Assert.That(Accepts(forge, "logs"), Is.False);
        Assert.That(Accepts(forge, "door_hinges"), Is.False);
        for (int i = 0; i < 8; i++)
        {
            Assert.That(Accepts(forge, "iron_bar"), Is.True, $"room for bar {i + 1}");
            Add(forge, "iron_bar", 1);
        }
        Assert.That(Accepts(forge, "iron_bar"), Is.False, "the Bar cap of 8");
        Assert.That(Accepts(forge, "charcoal"), Is.True, "Charcoal is separate");
        for (int i = 0; i < 8; i++)
            Add(forge, "charcoal", 1);
        Assert.That(Accepts(forge, "charcoal"), Is.False, "the Charcoal cap of 8");

        Assert.That(Start(forge, Hinges), Is.True);
        Assert.That(Prop(forge, "SmithyBars"), Is.EqualTo(7));
        Assert.That(Accepts(forge, "iron_bar"), Is.True, "top-ups are taken while a job runs");
    }

    // --- Saving: no new field; a job survives a save and a load ---

    [Test]
    public void TheSmithyUsesTheExistingFieldsAndTheSavedPileGainedNone()
    {
        string[] fields = Find("WoodPileState").GetFields(BindingFlags.Public | BindingFlags.Instance).Select(f => f.Name).ToArray();
        Assert.That(fields.Length, Is.EqualTo(17), "still the 17 fields");
        Assert.That(fields, Does.Contain("jobItem").And.Contain("jobHours").And.Contain("contents"));
        Assert.That(fields, Does.Contain("jobCount"), "the Pottery field, which the Smithy does not use");
        Assert.That(fields, Does.Contain("hopperWater").And.Contain("hopperBatch"), "the Hopper's fields are untouched");
        Assert.That(fields.Any(f => f.ToLowerInvariant().Contains("smithy") || f.ToLowerInvariant().Contains("forge") || f.ToLowerInvariant().Contains("anvil")), Is.False);
        Assert.That(CharcoalRules.BurnMarker, Is.EqualTo("charcoal_burn"), "the Pit's marker is untouched");
        Assert.That(BloomeryRules.SmeltMarker, Is.EqualTo("iron_smelt"), "and the Bloomery's");
        FieldInfo version = Find("SaveManager").GetField("SaveVersion", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        Assert.That(version.GetRawConstantValue(), Is.EqualTo(1));
        Assert.That(LedgerSaveData.CurrentVersion, Is.EqualTo(1));
    }

    [Test]
    public void AJobInProgressSurvivesASaveAndALoadWithItsStockAndAWaitingGood()
    {
        object forge = NewPile("Smithy");
        Add(forge, "iron_bar", 4);
        Add(forge, "charcoal", 6);
        Add(forge, "nails_box", 1);
        Set(forge, "jobItem", "door_hinges");
        Set(forge, "jobHours", 2f);
        string json = JsonUtility.ToJson(forge);
        object back = JsonUtility.FromJson(json, Find("WoodPileState"));
        Assert.That(Get(back, "kind").ToString(), Is.EqualTo("Smithy"));
        Assert.That(Get(back, "jobItem"), Is.EqualTo("door_hinges"));
        Assert.That((float)Get(back, "jobHours"), Is.EqualTo(2f));
        Assert.That(Count(back, "iron_bar"), Is.EqualTo(4));
        Assert.That(Count(back, "charcoal"), Is.EqualTo(6));
        Assert.That(Count(back, "nails_box"), Is.EqualTo(1));
        Assert.That(json, Does.Contain("\"kind\":44"));
        RunHours(back, 2);
        Assert.That(Count(back, "door_hinges"), Is.EqualTo(1), "the loaded job finishes on time");
    }

    [Test]
    public void AnOldSaveHasNoSmithyKindsAndNoOtherStationsJobIsReadAsASmithyJob()
    {
        object old = JsonUtility.FromJson("{\"id\":3,\"kind\":1,\"contents\":[]}", Find("WoodPileState"));
        Assert.That(Prop(old, "SmithyBars"), Is.EqualTo(0));
        Assert.That(Prop(old, "SmithyCharcoal"), Is.EqualTo(0));
        // The Workshop's job fields are the same fields and the same kind of value, but only a pile of kind Smithy is ticked or drawn as one.
        string wells = SourceText("Scripts/Managers/WellsAndOuthouses.cs");
        Assert.That(wells, Does.Contain("forge.kind != PileKind.Smithy"));
        Assert.That(wells, Does.Contain("bench.kind != PileKind.Workshop || string.IsNullOrEmpty(bench.jobItem)"), "the Workshop tick is unchanged");
    }

    // --- Dismantling ---

    [Test]
    public void DismantlingIsRefusedWhileAJobRunsAndOtherwiseReturnsEverythingWhole()
    {
        Assert.That(SmithyRules.DismantlePrompt(true, 2f), Is.EqualTo("Can't dismantle the Smithy while a good is being forged  (2 h to go)"));
        Assert.That(SmithyRules.DismantlePrompt(false, 0f), Does.StartWith("Dismantle the Smithy").And.Contain("come back whole"));
        string placed = SourceText("Scripts/World/PlacedStructure.cs");
        Assert.That(placed, Does.Contain("if (state.kind == PileKind.Smithy && SmithyWorking)"));
        Assert.That(placed, Does.Contain("SmithyRules.DismantlePrompt(true, state.jobHours)"));
        Assert.That(placed, Does.Contain("foreach (string stockId in SmithyRules.StockIds)\n                refund.Return(stockId, state.Count(stockId));"));
        Assert.That(placed, Does.Contain("foreach (SmithyRules.Recipe good in SmithyRules.Recipes)\n                refund.Return(good.itemId, state.Count(good.itemId));"));
        // Half the building materials, rounded down, as every structure: 3 Stone, 1 Clay, 1 Log, 2 Branches, 3 Tall Grass.
        Assert.That(new[] { 6, 3, 2, 4, 6 }.Select(n => n / 2), Is.EqualTo(new[] { 3, 1, 1, 2, 3 }));
        Assert.That(SourceText("Scripts/Managers/WoodManager.cs"), Does.Contain("SmithyCost"), "the finished Smithy refunds from the site cost");
    }

    // --- The market rows, by hand in the asset ---

    [Test]
    public void TheThreeGoodsSellInTheCraftedGroupAtMikesPricesAndAreNotInTheStore()
    {
        MarketTable table = MarketCatalog.Table;
        foreach ((string id, long cents) in new[] { ("door_hinges", 2600L), ("hook_set", 1800L), ("nails_box", 1600L) })
        {
            Assert.That(table.TryGetRow(id, out MarketRow row), Is.True, id);
            Assert.That(row.kind, Is.EqualTo(MarketKind.Bought), id + " is only bought");
            Assert.That(row.group, Is.EqualTo(MarketGroup.Crafted));
            Assert.That(row.baseCents, Is.EqualTo(cents));
            Assert.That(row.pool, Is.EqualTo(id), "each its own pool");
            Assert.That(table.BuyBaseCents(id), Is.EqualTo(0L), "not sold in the store");
            Assert.That(table.SellBaseCents(id), Is.EqualTo(cents));
        }
        Assert.That(table.Rows.Count, Is.EqualTo(92), "89 and the three");
        Assert.That(table.Rows.Count(r => r.group == MarketGroup.Crafted), Is.EqualTo(25), "22 and the three");
        Assert.That(table.Stock.Count, Is.EqualTo(22), "stocked stays 22");
        Assert.That(table.TryGetRow("iron_bar", out _), Is.False, "the Iron Bar still has no row");
        Assert.That(table.TryGetRule(MarketGroup.Crafted, out MarketGroupRule rule), Is.True);
        Assert.That(rule.dropBp, Is.EqualTo(1000), "10% off per unit sold");
        Assert.That(rule.floorBp, Is.EqualTo(1000), "floor 0.1");
        Assert.That((rule.spring, rule.summer, rule.fall, rule.winter), Is.EqualTo((10000, 10000, 10000, 10000)), "100% target in every season");
        Assert.That(MarketRules.Drop(10000, rule.dropBp, rule.floorBp), Is.EqualTo(9000), "the second Door Hinges sells at 90%");
        Assert.That(SmithyRules.Recipes.Select(r => r.baseCents), Is.EqualTo(new[] { table.SellBaseCents("door_hinges"), table.SellBaseCents("hook_set"), table.SellBaseCents("nails_box") }));
    }

    [Test]
    public void TheAssetHasTheThreeRowsByHandAndTheSameRowCountAsTheCatalog()
    {
        string asset = SourceText("Resources/Market/MarketData.asset");
        foreach ((string id, int cents) in new[] { ("door_hinges", 2600), ("hook_set", 1800), ("nails_box", 1600) })
            Assert.That(asset, Does.Contain($"  - itemId: {id}\n    group: 5\n    pool: {id}\n    baseCents: {cents}\n    kind: 1\n    seasonalBuy: 0\n"));
        int rows = asset.Split(new[] { "\n  - itemId: " }, StringSplitOptions.None).Length - 1;
        Assert.That(rows, Is.EqualTo(MarketCatalog.Rows.Length), "the asset and the catalog agree");
        Assert.That(rows, Is.EqualTo(92));
        Assert.That(asset, Does.Not.Contain("iron_bar"));
    }

    [Test]
    public void TheGoodsSellUnderTheCraftedLedgerLineLikeTheWorkshopPiecesAndNeedNoNewCategory()
    {
        Assert.That(LedgerCategories.ForSale(MarketGroup.Crafted), Is.EqualTo(LedgerCategory.SalesCrafted), "Crafted goods");
        Assert.That(LedgerCategories.PurchaseIsListed("door_hinges"), Is.False, "they cannot be bought");
        Assert.That(LedgerCategories.Count, Is.EqualTo(19), "no new category");
        Assert.That(MarketCatalog.Table.RuleOf("door_hinges").group, Is.EqualTo(MarketCatalog.Table.RuleOf("rough_stool").group), "the same group as the Workshop's pieces");
    }

    [Test]
    public void TheThreeItemAssetsAreMaterialsOfTheRightWeights()
    {
        foreach ((string id, string name, float kg) in new[] { ("door_hinges", "Door Hinges", 0.8f), ("hook_set", "Hook Set", 0.5f), ("nails_box", "Box of Nails (20)", 0.6f) })
        {
            string path = $"Assets/Resources/Items/Materials/{id}.asset";
            ScriptableObject item = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            Assert.That(item, Is.Not.Null, path);
            Type type = item.GetType();
            Assert.That(type.GetProperty("Id").GetValue(item), Is.EqualTo(id));
            Assert.That(type.GetProperty("DisplayName").GetValue(item), Is.EqualTo(name));
            Assert.That((float)type.GetProperty("WeightKg").GetValue(item), Is.EqualTo(kg));
            Assert.That(type.GetProperty("Category").GetValue(item).ToString(), Is.EqualTo("Material"));
            Assert.That(File.Exists(Path.Combine(Application.dataPath, $"Resources/Items/Materials/{id}.asset.meta")), Is.True, "with its meta");
            Assert.That(SmithyRules.TryFind(id, out SmithyRules.Recipe recipe) && recipe.weightKg == kg && recipe.name == name, Is.True, "the rules and the asset agree");
        }
    }

    // --- Placement ---

    [Test]
    public void TheSmithyKeepsThreeMetresOfClearAirFromTreesForTheFireAndTheWorkshopsRulesOtherwise()
    {
        Assert.That(SmithyRules.TreeReachMetres, Is.EqualTo(4.9f).Within(0.0001f), "no tree within 3 m of the shed's edge: 4.9 m from its centre");
        string wells = SourceText("Scripts/Managers/WellsAndOuthouses.cs");
        Assert.That(wells, Does.Contain("if (kind == PileKind.SmithySite || kind == PileKind.Smithy)"));
        Assert.That(wells, Does.Contain("Setbacks.CanPlaceShed(point, Sites(ignore), rules, out reason)"));
        Assert.That(wells, Does.Contain("WoodManager.Instance.FindTree(point, SmithyRules.TreeReachMetres) >= 0"));
        Assert.That(wells, Does.Contain("Too close to a tree: 3 m needed for the forge fire."));
        Assert.That(wells, Does.Contain("case PileKind.Smithy:\n            case PileKind.SmithySite: return SmithyRules.ReachMetres;"), "later buildings keep their gaps from its reach, as the Workshop's");
        string wood = SourceText("Scripts/Managers/WoodManager.cs");
        Assert.That(wood, Does.Contain("IsWorkshop(kind) || IsPotter(kind) || IsSmithy(kind)"), "wide-shed spacing, pair clearance and the 2 m trunk-free footprint");
        Assert.That(wood, Does.Contain("kind == PileKind.BloomerySite || kind == PileKind.SmithySite) && WellsAndOuthouses.Instance != null"));
        Assert.That(SourceText("Scripts/Survival/Setbacks/Setbacks.cs"), Does.Not.Contain("Smithy"), "Setbacks.cs needed no change: the shed rule and the tree check already cover it");
    }

    [Test]
    public void ANewTreeDoesNotSproutInsideTheSmithysFootprint()
    {
        Assert.That(RegrowRules.SmithyMetres, Is.EqualTo(SmithyRules.RegrowMetres));
        Assert.That(RegrowRules.Blocks(RegrowRules.Blocker.Smithy, 2.0f, 0f, 0f), Is.True);
        Assert.That(RegrowRules.Blocks(RegrowRules.Blocker.Smithy, 2.89f, 0f, 0f), Is.True);
        Assert.That(RegrowRules.Blocks(RegrowRules.Blocker.Smithy, 3.1f, 0f, 0f), Is.False);
        Assert.That(RegrowRules.Blocks(RegrowRules.Blocker.Workshop, 2.8f, 0f, 0f), Is.True, "the Workshop's blocker is unchanged");
        Assert.That(RegrowRules.Blocks(RegrowRules.Blocker.Bloomery, 1.3f, 0f, 0f), Is.False, "and the Bloomery's");
        Assert.That(SourceText("Scripts/Managers/WoodManager.cs"), Does.Contain("IsSmithy(pile.kind) ? RegrowRules.Blocker.Smithy"));
    }

    // --- The look ---

    static List<GameObject> BuildVisuals(object state, out Vector3 size)
    {
        var parent = new GameObject("Smithy visuals test");
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
    public void TheSmithyDrawsTheShedForgeAndAnvilWithStockAndGoodsBesideItAndSolidRoofAndPosts()
    {
        object empty = NewPile("Smithy");
        var emptyPieces = BuildVisuals(empty, out Vector3 emptySize);
        object stocked = NewPile("Smithy");
        Add(stocked, "iron_bar", 8);
        Add(stocked, "charcoal", 8);
        var stockedPieces = BuildVisuals(stocked, out _);
        object working = NewPile("Smithy");
        Set(working, "jobItem", "door_hinges");
        Set(working, "jobHours", 2f);
        var workingPieces = BuildVisuals(working, out _);
        object hinges = NewPile("Smithy");
        Add(hinges, "door_hinges", 1);
        var hingePieces = BuildVisuals(hinges, out _);
        object hooks = NewPile("Smithy");
        Add(hooks, "hook_set", 1);
        var hookPieces = BuildVisuals(hooks, out _);
        object nails = NewPile("Smithy");
        Add(nails, "nails_box", 1);
        var nailPieces = BuildVisuals(nails, out _);
        try
        {
            Assert.That(emptyPieces.Count - 1, Is.EqualTo(21), "floor, 4 posts, back wall, 7 roof bundles, hearth, fire bed, mouth, chimney, stump, anvil body, face plate and horn");
            Assert.That(emptyPieces.Count(p => p != null && p.name.StartsWith("Solid")), Is.EqualTo(12), "4 posts, the back wall and 7 roof bundles are solid");
            Assert.That(stockedPieces.Count - emptyPieces.Count, Is.EqualTo(10), "at most 4 Bars and 6 lumps of Charcoal are drawn");
            Assert.That(workingPieces.Count - emptyPieces.Count, Is.EqualTo(1), "a hot piece on the anvil; the glow is only a colour and the smoke is KilnSmoke");
            Assert.That(hingePieces.Count - emptyPieces.Count, Is.EqualTo(2), "two hinge leaves");
            Assert.That(hookPieces.Count - emptyPieces.Count, Is.EqualTo(3), "three hooks");
            Assert.That(nailPieces.Count - emptyPieces.Count, Is.EqualTo(1), "a box of nails");
            Assert.That(emptySize.x, Is.InRange(2.0f, 3.0f));
            Assert.That(emptySize.y, Is.InRange(0.9f, 1.4f));
        }
        finally
        {
            Cleanup(emptyPieces);
            Cleanup(stockedPieces);
            Cleanup(workingPieces);
            Cleanup(hingePieces);
            Cleanup(hookPieces);
            Cleanup(nailPieces);
        }
    }

    [Test]
    public void TheSiteDrawsItsStakesAndTheDeliveredMaterials()
    {
        object site = NewPile("SmithySite");
        var bare = BuildVisuals(site, out _);
        object stocked = NewPile("SmithySite");
        Add(stocked, "stone", 6);
        Add(stocked, "clay", 3);
        Add(stocked, "logs", 2);
        Add(stocked, "branches", 4);
        Add(stocked, "tall_grass", 6);
        var full = BuildVisuals(stocked, out _);
        try
        {
            Assert.That(full.Count - bare.Count, Is.EqualTo(18), "6 Stone, 3 Clay, and the plot's own 2 Logs, 4 Branches and 3 thatch lumps");
        }
        finally
        {
            Cleanup(bare);
            Cleanup(full);
        }
    }

    [Test]
    public void NothingOnTheSmithyShareAPlaneSoNothingFlashesAndNoLightOrFlickerIsDrawn()
    {
        string placed = SourceText("Scripts/World/PlacedStructure.cs");
        Assert.That(placed, Does.Contain("var hearth = new Vector3(-0.8f, 0f, -0.62f);"));
        Assert.That(placed, Does.Contain("hearth + new Vector3(0f, 0.4f, 0f), Vector3.zero, new Vector3(0.9f, 0.8f, 0.7f)"));
        Assert.That(placed, Does.Contain("hearth + new Vector3(0f, 0.3f, 0.35f), Vector3.zero, new Vector3(0.5f, 0.3f, 0.16f)"));
        Assert.That(placed, Does.Contain("new Vector3(0.55f, 0.46f, 0.15f), Vector3.zero, new Vector3(0.2f, 0.14f, 0.16f)"));

        // The hearth is 0.9 x 0.8 x 0.7 at (-0.8, -0.62): its back face is at z -0.97 and its front face at -0.27. The back wall (0.08 thick, centred -1.08) has its front face at -1.04.
        float hearthBack = -0.62f - 0.35f, hearthFront = -0.62f + 0.35f, wallFront = -(1.2f - 0.12f) + 0.04f;
        Assert.That(hearthBack - wallFront, Is.GreaterThan(0.05f), "the hearth's back is clear of the wall");
        // The mouth is 0.16 deep, centred on the hearth's front face: its front face is 8 cm proud of it.
        float mouthFront = hearthFront + 0.16f / 2f;
        Assert.That(mouthFront - hearthFront, Is.GreaterThan(0.05f), "the mouth stands proud of the hearth's front");
        // The chimney (0.34 across, centre -1.0, -0.78) lies inside the hearth's plan (x -1.25 to -0.35, z -0.97 to -0.27) so its foot is buried; it never touches a hearth face.
        float cx = SmithyRules.ChimneyX, cz = SmithyRules.ChimneyZ, r = 0.17f;
        Assert.That(cx - r, Is.GreaterThan(-0.8f - 0.45f + 0.05f));
        Assert.That(cx + r, Is.LessThan(-0.8f + 0.45f - 0.05f));
        Assert.That(cz - r, Is.GreaterThan(hearthBack + 0.01f), "2 cm inside the hearth's back face, not tangent to it");
        Assert.That(cz + r, Is.LessThan(hearthFront - 0.05f));
        // The anvil overlaps instead of touching: stump top 0.40 vs body 0.39 to 0.53, plate 0.525 to 0.595, horn 3 cm into the plate's end and its top below the plate's.
        Assert.That(0.46f - 0.07f, Is.LessThan(0.40f));
        Assert.That(0.56f - 0.035f, Is.LessThan(0.46f + 0.07f));
        Assert.That(0.92f - 0.15f, Is.LessThan(0.55f + 0.25f), "the horn runs into the plate");
        Assert.That(0.55f + 0.035f, Is.LessThan(0.56f + 0.035f), "and its top is below the plate's");
        // The hot piece is above the plate's top face (0.595), not on it.
        Assert.That(0.62f - 0.0175f, Is.GreaterThan(0.56f + 0.035f));

        Assert.That(placed, Does.Contain("smithySmoke.SetFiring(SmithyWorking)"));
        Assert.That(placed, Does.Contain("new Vector3(SmithyRules.ChimneyX, 0f, SmithyRules.ChimneyZ)"), "the smoke rises from the chimney");
        Assert.That(SmithyRules.SmokeHeight, Is.EqualTo(3.0f), "the chimney's top");
        int start = placed.IndexOf("static void BuildSmithy(", StringComparison.Ordinal);
        int end = placed.IndexOf("static readonly Color OreColor", StringComparison.Ordinal);
        string builder = placed.Substring(start, end - start);
        Assert.That(builder, Does.Not.Contain("Light"));
        Assert.That(builder, Does.Not.Contain("Time.time"));
        Assert.That(builder, Does.Not.Contain("Random.value"));
    }

    // --- The words ---

    [Test]
    public void ThePromptsAndMessagesSayWhatIsRunningAndWhatIsReady()
    {
        Assert.That(SmithyRules.OpenPrompt, Is.EqualTo("Choose what to forge at the Smithy"));
        Assert.That(SmithyRules.WorkingPrompt(Hinges, 2.2f), Is.EqualTo("Smithy  (forging Door Hinges, 3 h to go)"));
        Assert.That(SmithyRules.DonePrompt(Hooks), Is.EqualTo("Collect Hook Set  (ready in the Smithy)"));
        Assert.That(SmithyRules.StorePrompt("Iron Bar, Charcoal"), Is.EqualTo("Store Iron Bar, Charcoal"));
        Assert.That(SmithyRules.DeliverHelp, Is.EqualTo("Deliver Iron Bars and Charcoal  (the forge holds 8 Iron Bars and 8 Charcoal)"));
        Assert.That(SmithyRules.StartedMessage(Hinges), Is.EqualTo("Started Door Hinges. It will be ready in 3 h, even while you sleep or are away."));
        Assert.That(SmithyRules.TookMessage(Hinges, 1, 0), Is.EqualTo("Took the Door Hinges. Sell it at the Trading Post."));
        Assert.That(SmithyRules.TookMessage(Hinges, 1, 1), Is.EqualTo("Took 1 Door Hinges (1 left in the Smithy, no room)."));
        Assert.That(SmithyRules.BuiltMessage, Does.StartWith("Smithy built.").And.Contain("8 Iron Bars and 8 Charcoal"));
        Assert.That(SmithyRules.GateMessage(SmithyRules.Gate.Busy, Hooks, 2, 2, 3f), Is.EqualTo("The forge is busy: Hook Set is 3 h from done."));
        Assert.That(SmithyRules.GateMessage(SmithyRules.Gate.ProductWaiting, Hooks, 2, 2, 0f), Is.EqualTo("A finished good is waiting: collect it first."));
        Assert.That(SmithyRules.GateMessage(SmithyRules.Gate.ShortOfStock, Hooks, 0, 1, 0f), Is.EqualTo("The forge is short of 1 Iron Bar for Hook Set. Deliver more."));
        Assert.That(SmithyRules.ShortText(Hinges, 0, 0), Is.EqualTo("1 Iron Bar, 1 Charcoal"));
        Assert.That(SmithyRules.ShortText(Hinges, 1, 0), Is.EqualTo("1 Charcoal"));
        Assert.That(SmithyRules.ShortText(Hinges, 3, 3), Is.EqualTo(""));
    }

    [Test]
    public void TheScreensTextsSayWhatIsMissingWhenAJobCannotStart()
    {
        Assert.That(SmithyRules.ForgeLine(false, default, 0f, false, default, 4, 6), Is.EqualTo("Forge is idle - it holds 4 Iron Bars, 6 Charcoal"));
        Assert.That(SmithyRules.ForgeLine(true, Nails, 1.5f, false, default, 4, 6), Is.EqualTo("Forging Box of Nails (20), 2 h to go"));
        Assert.That(SmithyRules.ForgeLine(false, default, 0f, true, Hooks, 4, 6), Is.EqualTo("Hook Set is ready - collect it"));
        Assert.That(SmithyRules.ListLabel(Hinges, 0), Is.EqualTo("Door Hinges\n3 h  -  $26.00"), "no market price yet: the base price");
        Assert.That(SmithyRules.ListLabel(Hinges, 2340), Is.EqualTo("Door Hinges\n3 h  -  $23.40"), "what the Trading Post pays now");
        Assert.That(SmithyRules.DetailLine(Hooks), Is.EqualTo("2 h  -  0.5 kg  -  base price $18.00"));
        Assert.That(SmithyRules.StartButtonText(SmithyRules.Gate.Ok, Nails), Is.EqualTo("START BOX OF NAILS (20)"));
        Assert.That(SmithyRules.StartButtonText(SmithyRules.Gate.Busy, Nails), Is.EqualTo("FORGE IS BUSY"));
        Assert.That(SmithyRules.StartButtonText(SmithyRules.Gate.ProductWaiting, Nails), Is.EqualTo("COLLECT THE GOOD FIRST"));
        Assert.That(SmithyRules.StartButtonText(SmithyRules.Gate.ShortOfStock, Nails), Is.EqualTo("SHORT OF MATERIALS"));
        Assert.That(SmithyRules.ReasonText(SmithyRules.Gate.Busy, Hinges, Hooks, 2, 2, 1f), Is.EqualTo("Hook Set is 1 h from done."), "names the good being made, not the one chosen");
        Assert.That(SmithyRules.ReasonText(SmithyRules.Gate.ShortOfStock, Hinges, default, 0, 3, 0f), Is.EqualTo("Still short 1 Iron Bar. Deliver more."));
        Assert.That(SmithyRules.ReasonText(SmithyRules.Gate.Ok, Hinges, default, 1, 1, 0f), Is.EqualTo(""));

        SmithyRules.NeedRow[] rows = SmithyRules.NeedRows(Hinges, 0, 1);
        Assert.That(rows.Length, Is.EqualTo(2));
        Assert.That(rows[0].Text, Is.EqualTo("Iron Bars - need 1 - forge has 0 - short by 1"));
        Assert.That(rows[1].Text, Is.EqualTo("Charcoal - need 1 - forge has 1 - enough"));

        Assert.That(SmithyRules.PackLine(3, 5), Is.EqualTo("You carry: 3 Iron Bars, 5 Charcoal"));
        Assert.That(SmithyRules.PackLine(1, 0), Is.EqualTo("You carry: 1 Iron Bar"));
        Assert.That(SmithyRules.PackLine(0, 2), Is.EqualTo("You carry: 2 Charcoal"));
        Assert.That(SmithyRules.PackLine(0, 0), Is.EqualTo("You carry: nothing the forge uses"));
        Assert.That(SmithyRules.StockButtonText, Is.EqualTo("PUT MY IRON BARS AND CHARCOAL IN THE FORGE"));
        Assert.That(SmithyRules.Stored("3 Iron Bars", 3, 0), Is.EqualTo("Stored 3 Iron Bars  (the forge holds 3 Iron Bars, 0 Charcoal)"));
    }

    [Test]
    public void TheStockButtonIsOnOnlyWhenThePackHasSomethingTheForgeHasRoomFor()
    {
        Assert.That(SmithyRules.StockState(0, 0, 0, 0), Is.EqualTo(SmithyRules.StockGate.NothingCarried));
        Assert.That(SmithyRules.StockState(0, 0, 8, 8), Is.EqualTo(SmithyRules.StockGate.NothingCarried), "a full forge does not matter when nothing is carried");
        Assert.That(SmithyRules.StockState(3, 0, 0, 0), Is.EqualTo(SmithyRules.StockGate.Can));
        Assert.That(SmithyRules.StockState(0, 2, 8, 0), Is.EqualTo(SmithyRules.StockGate.Can));
        Assert.That(SmithyRules.StockState(3, 0, 8, 0), Is.EqualTo(SmithyRules.StockGate.ForgeFull), "carrying only what the forge is full of");
        Assert.That(SmithyRules.StockState(3, 2, 8, 8), Is.EqualTo(SmithyRules.StockGate.ForgeFull));
        Assert.That(SmithyRules.StockState(3, 2, 8, 7), Is.EqualTo(SmithyRules.StockGate.Can));
        Assert.That(SmithyRules.StockReason(SmithyRules.StockGate.Can), Is.EqualTo(""));
        Assert.That(SmithyRules.StockReason(SmithyRules.StockGate.NothingCarried), Is.EqualTo("Your pack has no Iron Bars or Charcoal."));
        Assert.That(SmithyRules.StockReason(SmithyRules.StockGate.ForgeFull), Is.EqualTo("The forge is already full of what you carry."));
    }

    [Test]
    public void TheKeysChooseMoveStartAndStockAndNothingElse()
    {
        Assert.That(SmithyRules.Resolve(SmithyRules.Key.One, 2, 3).choice, Is.EqualTo(0));
        Assert.That(SmithyRules.Resolve(SmithyRules.Key.Two, 0, 3).choice, Is.EqualTo(1));
        Assert.That(SmithyRules.Resolve(SmithyRules.Key.Three, 0, 3).choice, Is.EqualTo(2));
        Assert.That(SmithyRules.Resolve(SmithyRules.Key.Three, 0, 2).kind, Is.EqualTo(SmithyRules.ActionKind.None), "past the end of the list");
        Assert.That(SmithyRules.Resolve(SmithyRules.Key.Up, 0, 3).choice, Is.EqualTo(2), "wraps round");
        Assert.That(SmithyRules.Resolve(SmithyRules.Key.Down, 2, 3).choice, Is.EqualTo(0));
        Assert.That(SmithyRules.Resolve(SmithyRules.Key.Enter, 1, 3).kind, Is.EqualTo(SmithyRules.ActionKind.Start));
        Assert.That(SmithyRules.Resolve(SmithyRules.Key.Stock, 1, 3).kind, Is.EqualTo(SmithyRules.ActionKind.Stock));
        Assert.That(SmithyRules.Resolve(SmithyRules.Key.None, 1, 3).kind, Is.EqualTo(SmithyRules.ActionKind.None));
        Assert.That(SmithyRules.Resolve(SmithyRules.Key.One, 0, 0).kind, Is.EqualTo(SmithyRules.ActionKind.None));
        Assert.That(SmithyRules.KeyHint, Is.EqualTo("1-3 choose    Up/Down move    Enter start    R stock    Esc close"));
    }

    // --- The screen ---

    static Type Screen => Find("SmithyScreen");

    static float ScreenConstant(string constant) => Convert.ToSingle(Screen.GetField(constant, BindingFlags.Public | BindingFlags.Static).GetValue(null));

    static GameObject BuildScreen(out Component instance)
    {
        var go = new GameObject("Smithy Screen Test");
        instance = go.AddComponent(Screen);
        Screen.GetMethod("Build", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(instance, null);
        return go;
    }

    [Test]
    public void TheScreenBuildsWithNothingUnderTwentySixAndBodyTextOfTwentyEight()
    {
        Type text = Type.GetType("UnityEngine.UI.Text, UnityEngine.UI");
        Assert.That(text, Is.Not.Null);
        GameObject go = BuildScreen(out _);
        try
        {
            PropertyInfo size = text.GetProperty("fontSize"), style = text.GetProperty("fontStyle");
            Component[] labels = go.GetComponentsInChildren(text, true);
            Assert.That(labels.Length, Is.GreaterThan(14));
            foreach (Component label in labels)
                Assert.That((int)size.GetValue(label), Is.GreaterThanOrEqualTo(26), label.name);

            int SizeOf(string name) => (int)size.GetValue(labels.First(l => l.name == name));
            Assert.That(SizeOf("Title"), Is.EqualTo(36));
            Assert.That(SizeOf("Status"), Is.EqualTo(28));
            Assert.That(SizeOf("Piece"), Is.EqualTo(40));
            Assert.That(SizeOf("Piece Line"), Is.EqualTo(28));
            Assert.That(SizeOf("Needs Heading"), Is.EqualTo(28));
            Assert.That(SizeOf("Need 0"), Is.EqualTo(28), "body text 28");
            Assert.That(SizeOf("Need 1"), Is.EqualTo(28));
            Assert.That(SizeOf("Reason"), Is.EqualTo(28));
            Assert.That(SizeOf("Hint"), Is.EqualTo(28));
            Assert.That(SizeOf("Pack"), Is.EqualTo(28));
            Assert.That(SizeOf("Stock Reason"), Is.EqualTo(28));
            Component[] stocks = labels.Where(l => l.transform.parent != null && l.transform.parent.name == "Stock").ToArray();
            Assert.That(stocks.Length, Is.EqualTo(1));
            Assert.That((int)size.GetValue(stocks[0]), Is.EqualTo(32), "the stock button");
            Assert.That((int)style.GetValue(stocks[0]), Is.EqualTo((int)FontStyle.Bold));
            Assert.That(labels.Where(l => l.name == "Label" && l.transform.parent.name.StartsWith("Row")).Select(l => (int)size.GetValue(l)), Is.All.EqualTo(32), "the list names");
            Assert.That(labels.Count(l => l.name == "Label" && l.transform.parent.name.StartsWith("Row")), Is.EqualTo(3), "three goods");
            Assert.That(labels.Where(l => l.name == "Detail").Select(l => (int)size.GetValue(l)), Is.All.EqualTo(28), "the hours and price");
            Component[] starts = labels.Where(l => l.transform.parent != null && l.transform.parent.name == "Start").ToArray();
            Assert.That(starts.Length, Is.EqualTo(1));
            Assert.That((int)size.GetValue(starts[0]), Is.EqualTo(36), "the Start button");
            Component[] closes = labels.Where(l => l.transform.parent != null && l.transform.parent.name == "Close").ToArray();
            Assert.That(closes.Length, Is.EqualTo(1));
            Assert.That((int)size.GetValue(closes[0]), Is.EqualTo(28), "the Close button");
            Assert.That(ScreenConstant("MinimumSize"), Is.EqualTo(26f));
            Assert.That(ScreenConstant("RowHeight"), Is.GreaterThanOrEqualTo(96f));
            Assert.That(ScreenConstant("RowGap"), Is.GreaterThanOrEqualTo(12f));
            Assert.That(ScreenConstant("StockButtonHeight"), Is.GreaterThanOrEqualTo(80f));
            Assert.That(labels.Select(l => l.GetComponent(text)).All(l => l != null), Is.True);
            Assert.That(labels.Any(l => l.name == "Hint" && (string)text.GetProperty("text").GetValue(l) == SmithyRules.KeyHint), Is.True);
            Assert.That(labels.Any(l => (string)text.GetProperty("text").GetValue(l) == SmithyRules.StockButtonText), Is.True, "the stock button says what Mike asked for");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(go);
        }
    }

    [Test]
    public void EverythingOnTheScreenFitsInsideThePanelWithoutOverlap()
    {
        GameObject go = BuildScreen(out _);
        try
        {
            string[] names =
            {
                "Title", "Close", "Status", "Pack", "Row 0", "Row 1", "Row 2", "Stock Reason", "Stock", "Piece", "Piece Line", "Needs Heading", "Need 0", "Need 1", "Reason", "Start", "Hint",
            };
            RectTransform[] all = go.GetComponentsInChildren<RectTransform>(true);
            var boxes = new Dictionary<string, Rect>();
            foreach (string name in names)
            {
                RectTransform rt = all.First(r => r.name == name);
                boxes[name] = new Rect(rt.anchoredPosition.x, -rt.anchoredPosition.y, rt.sizeDelta.x, rt.sizeDelta.y); // top-left placed: x right, y down
            }
            var panel = new Rect(0f, 0f, SmithyRules.PanelWidthPx, SmithyRules.PanelHeightPx);
            foreach (var pair in boxes)
            {
                Assert.That(pair.Value.xMin, Is.GreaterThanOrEqualTo(panel.xMin), pair.Key);
                Assert.That(pair.Value.yMin, Is.GreaterThanOrEqualTo(panel.yMin), pair.Key);
                Assert.That(pair.Value.xMax, Is.LessThanOrEqualTo(panel.xMax), pair.Key);
                Assert.That(pair.Value.yMax, Is.LessThanOrEqualTo(panel.yMax), pair.Key);
            }
            for (int a = 0; a < names.Length; a++)
                for (int b = a + 1; b < names.Length; b++)
                    Assert.That(boxes[names[a]].Overlaps(boxes[names[b]]), Is.False, names[a] + " overlaps " + names[b]);
            Assert.That(boxes["Stock"].height, Is.GreaterThanOrEqualTo(80f));
            Assert.That(boxes["Row 1"].yMin - boxes["Row 0"].yMax, Is.GreaterThanOrEqualTo(12f), "gap between rows");
            RectTransform panelRect = all.First(r => r.name == "Panel");
            Assert.That(panelRect.sizeDelta, Is.EqualTo(new Vector2(1500f, 920f)));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(go);
        }
    }

    [Test]
    public void ThePanelNeverClipsOnAnyWindowBecauseTheScalerMatchesTheHeightOnWideOnes()
    {
        Assert.That(SmithyRules.PanelHeightPx, Is.LessThanOrEqualTo(SmithyRules.ReferenceHeightPx), "at match-height the canvas is always 1080 tall, taller than the 920 panel");
        Assert.That(SmithyRules.ScreenMatch(16f / 9f), Is.EqualTo(1f));
        Assert.That(SmithyRules.ScreenMatch(21f / 9f), Is.EqualTo(1f), "21:9");
        Assert.That(SmithyRules.ScreenMatch(32f / 9f), Is.EqualTo(1f), "32:9");
        Assert.That(SmithyRules.ScreenMatch(16f / 10f), Is.EqualTo(1f));
        Assert.That(SmithyRules.ScreenMatch(SmithyRules.PanelWidthPx / SmithyRules.ReferenceHeightPx), Is.EqualTo(1f), "the narrowest window that still holds the panel's width");
        Assert.That(SmithyRules.ScreenMatch(4f / 3f), Is.EqualTo(0f), "a narrow window matches the width instead");
        Assert.That(SmithyRules.ScreenMatch(5f / 4f), Is.EqualTo(0f));
        // At match-width the canvas is 1920 wide and 1920 / aspect tall: for the narrow windows that get it, that is taller than the panel.
        foreach (float aspect in new[] { 5f / 4f, 4f / 3f, 1.38f })
        {
            Assert.That(SmithyRules.ScreenMatch(aspect), Is.EqualTo(0f));
            Assert.That(1920f / aspect, Is.GreaterThanOrEqualTo(SmithyRules.PanelHeightPx), $"aspect {aspect}");
        }
        // At match-height the canvas is 1080 * aspect wide: for every window that gets it, that is wider than the panel.
        foreach (float aspect in new[] { 16f / 10f, 16f / 9f, 21f / 9f, 32f / 9f })
            Assert.That(SmithyRules.ReferenceHeightPx * aspect, Is.GreaterThanOrEqualTo(SmithyRules.PanelWidthPx), $"aspect {aspect}");

        GameObject go = BuildScreen(out Component screen);
        try
        {
            var scaler = go.GetComponentInChildren<UnityEngine.UI.CanvasScaler>(true);
            Assert.That(scaler.referenceResolution, Is.EqualTo(new Vector2(1920f, 1080f)));
            Assert.That(scaler.uiScaleMode, Is.EqualTo(UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize));
            Assert.That(scaler.matchWidthOrHeight, Is.EqualTo(1f), "match-height, not the Workshop's blended 0.5");
            Assert.That(go.GetComponentInChildren<Canvas>(true).sortingOrder, Is.EqualTo(150), "the Workshop screen's sort order");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(go);
        }
        Assert.That(SourceText("Scripts/UI/SmithyScreen.cs"), Does.Contain("SmithyRules.ScreenMatch("), "chosen each time the screen opens");
    }

    [Test]
    public void TheScreenIsClosedUntilOpenedAndTheSmithyHandsItOnlyTheMethodsTheWorkshopDid()
    {
        Assert.That((bool)Screen.GetProperty("IsOpen", BindingFlags.Public | BindingFlags.Static).GetValue(null), Is.False);
        Assert.That(Screen.GetMethod("Open", BindingFlags.Public | BindingFlags.Static), Is.Not.Null);
        Assert.That(Screen.GetMethod("Close", BindingFlags.Public | BindingFlags.Static), Is.Not.Null);
        Type placed = Find("PlacedStructure");
        MethodInfo start = placed.GetMethod("TryStartForge", BindingFlags.Public | BindingFlags.Instance);
        Assert.That(start, Is.Not.Null);
        Assert.That(start.GetParameters().Select(p => p.ParameterType.Name), Is.EqualTo(new[] { "Recipe", "String&" }));
        MethodInfo stock = placed.GetMethod("ForgeStockFromPack", BindingFlags.Public | BindingFlags.Instance);
        Assert.That(stock, Is.Not.Null);
        Assert.That(stock.GetParameters().Select(p => p.ParameterType.Name), Is.EqualTo(new[] { "String&" }));
        Assert.That(placed.GetMethod("ForgeStockGateNow", BindingFlags.Public | BindingFlags.Instance), Is.Not.Null);
        Assert.That(placed.GetMethod("SmithyRunning", BindingFlags.Public | BindingFlags.Instance), Is.Not.Null);
        Assert.That(placed.GetMethod("SmithyWaiting", BindingFlags.Public | BindingFlags.Instance), Is.Not.Null);
        Assert.That(placed.GetMethod("SmithyGateFor", BindingFlags.Public | BindingFlags.Instance), Is.Not.Null);
        Assert.That(placed.GetMethods(BindingFlags.Public | BindingFlags.Instance).Any(m => m.Name.Contains("Dismantle")), Is.False, "dismantling stays inside the structure and the Hammer's R");
        string source = SourceText("Scripts/UI/SmithyScreen.cs");
        Assert.That(source, Does.Contain("smithy.TryStartForge(SmithyRules.Recipes[selected], out string message)"));
        Assert.That(source, Does.Contain("smithy.ForgeStockFromPack(out string message)"));
        Assert.That(source, Does.Not.Contain("Dismantle"), "the screen never dismantles");
        Assert.That(source, Does.Contain("keyboard.rKey.wasPressedThisFrame"), "R stocks, as at the Workshop");
        Assert.That(source, Does.Contain("OnStateChanged"), "Esc and any way out of the Menu state close it");
        Assert.That(source, Does.Contain("Hide(true)"), "the Close button");
    }

    // --- Know-how and item help ---

    [Test]
    public void TheSmithyEntryUnlocksOnAPlacedSiteOrAnIronBarAndTheBloomeryAndBarHelpPointToIt()
    {
        Assert.That(KnowHowRules.Entries.Length, Is.EqualTo(11));
        Assert.That(KnowHowRules.TryGet(KnowHowRules.Smithy, out KnowHowRules.Entry entry), Is.True);
        Assert.That(entry.title, Is.EqualTo("The Smithy: Forging Iron Goods"));
        Assert.That(KnowHowRules.ForItem("iron_bar"), Is.EqualTo(new[] { "smelting", "smithy" }));
        Assert.That(KnowHowRules.ForHappening(KnowHowRules.Happening.SmithySitePlaced), Is.EqualTo(new[] { "smithy" }), "first placing a Smithy site");
        Assert.That(KnowHowRules.ForItem("door_hinges"), Is.Empty, "the goods open nothing new");

        List<string> afterBloomery = KnowHowRules.NewUnlocks(new[] { "smelting" }, KnowHowRules.ForItem("iron_bar"));
        Assert.That(afterBloomery, Is.EqualTo(new[] { "smithy" }), "a player who placed a Bloomery site first gets the Smithy entry when the first Bar arrives");
        Assert.That(KnowHowRules.NewUnlocks(new[] { "smithy" }, KnowHowRules.ForHappening(KnowHowRules.Happening.SmithySitePlaced)), Is.Empty, "never twice");
        Assert.That(KnowHowRules.Backfill(id => false, false, false, false, smithyPlaced: true), Is.EqualTo(new[] { "smithy" }));
        Assert.That(KnowHowRules.Backfill(id => id == "iron_bar", false, false, false), Is.EqualTo(new[] { "smelting", "smithy" }));
        Assert.That(SourceText("Scripts/Managers/JournalManager.cs"), Does.Contain("hopper, tripod, taps, pit, bloomery, smithy)"));
        Assert.That(SourceText("Scripts/Managers/WoodManager.cs"), Does.Contain("KnowHowRules.Happening.SmithySitePlaced"));

        KnowHowRules.TryGet(KnowHowRules.Smelting, out KnowHowRules.Entry smelting);
        Assert.That(smelting.body, Does.Contain("The Smithy: Forging Iron Goods"), "the Bloomery entry points to the next step");
        Assert.That(smelting.body, Does.Not.Contain("not in the game yet"));
        Assert.That(KnowHowRules.ItemTip("iron_bar"), Does.Contain("Smithy"));
        string craft = SourceText("Scripts/UI/CraftingScreen.cs");
        Assert.That(craft, Does.Contain("forge them into goods at a Smithy"), "the Bloomery's Build-tab help points to it too");
    }

    [Test]
    public void TheEntryAndTheItemHelpSayWhatTheCodeDoes()
    {
        KnowHowRules.TryGet(KnowHowRules.Smithy, out KnowHowRules.Entry smithy);
        Assert.That(smithy.body, Does.Contain("6 Stone, 3 Clay, 2 Logs, 4 Branches and 6 Tall Grass"));
        Assert.That(smithy.body, Does.Contain("about 61 kg"));
        Assert.That(smithy.body, Does.Contain("it holds 8 of each"));
        Assert.That(smithy.body, Does.Contain("Door Hinges take 3 game hours; a Hook Set and a Box of Nails take 2"));
        Assert.That(smithy.body, Does.Contain("Each uses 1 Iron Bar and 1 Charcoal"));
        Assert.That(smithy.body, Does.Contain("about $26").And.Contain("about $18").And.Contain("about $16"));
        Assert.That(smithy.body, Does.Contain("3 m clear of trees"));
        Assert.That(smithy.body, Does.Contain("not while a good is being forged"));
        Assert.That(SmithyRules.SiteStone + SmithyRules.SiteClay + SmithyRules.SiteLogs + SmithyRules.SiteBranches + SmithyRules.SiteTallGrass, Is.EqualTo(21));

        foreach (string id in new[] { "door_hinges", "hook_set", "nails_box", "iron_bar" })
        {
            string tip = KnowHowRules.ItemTip(id);
            Assert.That(tip, Is.Not.Empty, id);
            Assert.That(tip.Length, Is.LessThanOrEqualTo(80), id + " must fit in two lines of the row's info text");
        }
        Assert.That(KnowHowRules.ItemTip("door_hinges"), Does.Contain("$26"));
        Assert.That(KnowHowRules.ItemTip("hook_set"), Does.Contain("$18"));
        Assert.That(KnowHowRules.ItemTip("nails_box"), Does.Contain("$16"));

        string craft = SourceText("Scripts/UI/CraftingScreen.cs");
        Assert.That(craft, Does.Contain("case PileKind.SmithySite:"));
        Assert.That(craft, Does.Contain("{SmithyRules.SiteStone} Stone, {SmithyRules.SiteClay} Clay, {SmithyRules.SiteLogs} Logs, {SmithyRules.SiteBranches} Branches and {SmithyRules.SiteTallGrass} Tall Grass"));
        Assert.That(craft, Does.Contain("case PileKind.Smithy:"));
        Assert.That(craft, Does.Contain("{SmithyRules.BarsCap}").And.Contain("{SmithyRules.Recipes[0].hours}"));
    }

    [Test]
    public void TheKnowHowTextFileCarriesTheNewEntryAndTheChangedSentenceSoItTravelsWithTheCode()
    {
        string path = Path.GetFullPath(Path.Combine(Application.dataPath, "../../../05_Documentation/Game_Systems/Journal_KnowHow_Text.md"));
        if (!File.Exists(path))
            Assert.Ignore("Journal_KnowHow_Text.md is not at " + path);
        string text = File.ReadAllText(path).Replace("\r\n", "\n");
        Assert.That(text, Does.Contain("## 11. The Smithy: Forging Iron Goods"));
        Assert.That(text, Does.Contain("Trigger: first Smithy site placed, or first Iron Bar held."));
        Assert.That(text, Does.Contain("forge them into iron goods at a Smithy (see \"The Smithy: Forging Iron Goods\")."));
        Assert.That(text, Does.Not.Contain("tools that are not in the game yet"));
    }

    // --- The dev keys ---

    [Test]
    public void TheMinusKeyIsFreeAndTheSmithyKitIsSplitIntoPilesSoThePackCanCarryIt()
    {
        string keys = SourceText("Scripts/Debug/TimeDebugControls.cs");
        Assert.That(keys, Does.StartWith("#if UNITY_EDITOR || DEVELOPMENT_BUILD"), "compiled out of release builds");
        Assert.That(keys, Does.Contain("keyboard.minusKey.wasPressedThisFrame"));
        Assert.That(keys, Does.Contain("DebugFinishForges()"));
        Assert.That(keys, Does.Contain("PileKind.Smithy, ForgeAhead(4f)"));
        Assert.That(keys, Does.Contain("PileKind.RockStorage, ForgeAhead(3f, -1.2f)"));
        Assert.That(keys, Does.Contain("PileKind.WoodStorage, ForgeAhead(3f, 1.2f)"));
        Assert.That(keys, Does.Contain("smithy.Add(SmithyRules.BarId, 4);"));
        Assert.That(keys, Does.Contain("smithy.Add(SmithyRules.CharcoalId, 6);"));
        Assert.That(keys, Does.Contain("keyboard.equalsKey.wasPressedThisFrame && keyboard.shiftKey.isPressed"), "Shift+= still gives 4 Iron Bars");

        string scripts = Path.Combine(Application.dataPath, "Scripts");
        foreach (string file in Directory.GetFiles(scripts, "*.cs", SearchOption.AllDirectories))
        {
            if (file.EndsWith("TimeDebugControls.cs"))
                continue;
            Assert.That(File.ReadAllText(file), Does.Not.Contain("minusKey"), file);
        }
        foreach (string file in Directory.GetFiles(Application.dataPath, "*.inputactions", SearchOption.AllDirectories))
            Assert.That(File.ReadAllText(file), Does.Not.Contain("<Keyboard>/minus"), file);

        // The pack takes 45 kg. In the pack: a Hammer 1.2, 3 Clay 10.5, 4 Branches 4, 6 Tall Grass 0.6 = 16.3 kg. The 6 Stone (30 kg) and the 2 Logs (16 kg) stand as two piles.
        float inPack = 1.2f + SmithyRules.SiteClay * 3.5f + SmithyRules.SiteBranches * 1f + SmithyRules.SiteTallGrass * 0.1f;
        float stone = SmithyRules.SiteStone * 5f, logs = SmithyRules.SiteLogs * 8f;
        Assert.That(inPack, Is.EqualTo(16.3f).Within(0.001f));
        Assert.That(inPack + stone + logs, Is.EqualTo(62.3f).Within(0.001f), "the whole kit, with the Hammer");
        Assert.That(inPack + stone + logs, Is.GreaterThan(45f), "would not fit the pack");
        Assert.That(stone + 1.2f, Is.LessThan(45f), "once the small materials are stored, all the Stone fits in one carry");
        Assert.That(logs + 1.2f, Is.LessThan(45f));
    }

    [Test]
    public void TheForgeKeyFinishesEveryRunningJobAndNothingElse()
    {
        string wells = SourceText("Scripts/Managers/WellsAndOuthouses.cs");
        int start = wells.IndexOf("public int DebugFinishForges()", StringComparison.Ordinal);
        Assert.That(start, Is.GreaterThan(0));
        string body = wells.Substring(start, 600);
        Assert.That(body, Does.Contain("forge.kind != PileKind.Smithy || string.IsNullOrEmpty(forge.jobItem)"));
        Assert.That(body, Does.Contain("FinishForgeJob(forge)"));
    }

    // --- Nothing else changed ---

    [Test]
    public void ThePitTheBloomeryTheKilnTheHopperTheWorkshopAndTheRestOfTheMarketAreUntouched()
    {
        Assert.That(CharcoalRules.SiteClay, Is.EqualTo(4));
        Assert.That(CharcoalRules.LogsCap, Is.EqualTo(6));
        Assert.That(CharcoalRules.BurnHours, Is.EqualTo(24));
        Assert.That(BloomeryRules.SiteStone, Is.EqualTo(8));
        Assert.That(BloomeryRules.SiteClay, Is.EqualTo(6));
        Assert.That(BloomeryRules.SmeltHours, Is.EqualTo(10));
        Assert.That(BloomeryRules.OreCap, Is.EqualTo(4));
        Assert.That(PotteryRules.KilnFirewood, Is.EqualTo(10));
        Assert.That(PotashRules.HopperCapacity, Is.EqualTo(12));
        Assert.That(WorkshopRules.Recipes.Select(r => r.baseCents), Is.EqualTo(new[] { 1400L, 1200L, 1800L, 2800L }));
        Assert.That(WorkshopRules.Recipes.Select(r => r.hours), Is.EqualTo(new[] { 3, 2, 4, 5 }));
        Assert.That(WorkshopRules.SiteLogs, Is.EqualTo(5));
        Assert.That(WorkshopRules.LogsCap, Is.EqualTo(6));
        Assert.That(MarketCatalog.Table.Stock.Count, Is.EqualTo(22));
        Assert.That(MarketCatalog.Table.BuyBaseCents("iron_ore"), Is.EqualTo(400L));
        Assert.That(MarketCatalog.Table.SellBaseCents("charcoal"), Is.EqualTo(100L));
        Assert.That(MarketCatalog.Table.SellBaseCents("rough_stool"), Is.EqualTo(1400L));
        Assert.That(MarketCatalog.Table.SellBaseCents("clay_cup"), Is.EqualTo(400L));
        Assert.That(Enum.GetValues(typeof(MarketGroup)).Length, Is.EqualTo(11), "no new market group");
        Assert.That(MetalRules.Metals.Length, Is.EqualTo(1));
        Assert.That(SourceText("Scripts/Player/AxeTool.cs"), Does.Not.Contain("Smithy"));
        Assert.That(SourceText("Scripts/Editor/MarketDataSeeder.cs"), Does.Not.Contain("Smithy"), "the seeder was not touched");
    }
}
