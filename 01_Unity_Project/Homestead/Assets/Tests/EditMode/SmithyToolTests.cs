using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

// EditMode tests for Metalworking phase M4 (Mike, 2026-11-04): the iron Shovel and iron Pick Axe, made at the Smithy. SmithyRules and ToolRules are pure; the saved pile, the visuals and the
// screen are reached by reflection as the other world tests do. Nothing here touches the Saves folder or a scene.
public class SmithyToolTests
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

    static bool Accepts(object pile, string id) => (bool)Find("WoodPileState").GetMethod("Accepts").Invoke(pile, new object[] { id });

    static object Get(object pile, string field) => Find("WoodPileState").GetField(field).GetValue(pile);

    static void Set(object pile, string field, object value) => Find("WoodPileState").GetField(field).SetValue(pile, value);

    static object Wells(string method, params object[] args) =>
        Find("WellsAndOuthouses").GetMethod(method, BindingFlags.Public | BindingFlags.Static).Invoke(null, args);

    static bool Working(object pile) => !string.IsNullOrEmpty((string)Get(pile, "jobItem")) && (float)Get(pile, "jobHours") > 0f;

    static bool Waiting(object pile) => SmithyRules.Recipes.Any(r => Count(pile, r.itemId) > 0);

    static SmithyRules.Gate Gate(object pile, SmithyRules.Recipe recipe) =>
        SmithyRules.CanStart(recipe, Working(pile), Waiting(pile), Count(pile, "iron_bar"), Count(pile, "charcoal"), Count(pile, "sticks"), Count(pile, "cordage"));

    // What PlacedStructure.TryStartForge does to the saved pile, step for step.
    static bool Start(object forge, SmithyRules.Recipe recipe)
    {
        if (Gate(forge, recipe) != SmithyRules.Gate.Ok)
            return false;
        Take(forge, "iron_bar", recipe.bars);
        Take(forge, "charcoal", recipe.charcoal);
        Take(forge, "sticks", recipe.sticks);
        Take(forge, "cordage", recipe.cordage);
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
    static SmithyRules.Recipe Shovel => SmithyRules.Recipes[3];
    static SmithyRules.Recipe PickAxe => SmithyRules.Recipes[4];

    static object StockedForge()
    {
        object forge = NewPile("Smithy");
        Add(forge, "iron_bar", 4);
        Add(forge, "charcoal", 6);
        Add(forge, "sticks", 6);
        Add(forge, "cordage", 2);
        return forge;
    }

    // --- The two recipes ---

    [Test]
    public void TheIronShovelAndPickAxeAreRowsOfTheTableWithMikesIngredientsAndHours()
    {
        Assert.That(SmithyRules.TryFind("iron_shovel", out SmithyRules.Recipe shovel), Is.True);
        Assert.That(shovel.name, Is.EqualTo("Iron Shovel"));
        Assert.That((shovel.bars, shovel.charcoal, shovel.sticks, shovel.cordage), Is.EqualTo((2, 2, 3, 1)));
        Assert.That(shovel.hours, Is.EqualTo(3));
        Assert.That(shovel.weightKg, Is.EqualTo(1.8f));
        Assert.That(shovel.OwnUse, Is.True);
        Assert.That(shovel.metalId, Is.EqualTo(MetalRules.IronId));

        Assert.That(SmithyRules.TryFind("iron_pick_axe", out SmithyRules.Recipe pick), Is.True);
        Assert.That(pick.name, Is.EqualTo("Iron Pick Axe"));
        Assert.That((pick.bars, pick.charcoal, pick.sticks, pick.cordage), Is.EqualTo((2, 2, 2, 1)));
        Assert.That(pick.hours, Is.EqualTo(3));
        Assert.That(pick.weightKg, Is.EqualTo(2f));
        Assert.That(pick.OwnUse, Is.True);

        Assert.That(SmithyRules.IsProduct("iron_shovel"), Is.True);
        Assert.That(SmithyRules.Recipes.Take(3).Any(r => r.OwnUse), Is.False, "the three goods are all for sale");
        Assert.That(SmithyRules.Recipes.Take(3).Select(r => r.itemId), Is.EqualTo(new[] { "door_hinges", "hook_set", "nails_box" }));
    }

    [Test]
    public void TheThreeGoodsKeepTheirRecipesHoursAndPrices()
    {
        Assert.That(SmithyRules.Recipes.Take(3).Select(r => (r.bars, r.charcoal, r.sticks, r.cordage, r.hours, r.baseCents)),
                    Is.EqualTo(new[] { (1, 1, 0, 0, 3, 2600L), (1, 1, 0, 0, 2, 1800L), (1, 1, 0, 0, 2, 1600L) }));
        Assert.That(SmithyRules.Recipes.Take(3).Select(r => r.weightKg), Is.EqualTo(new[] { 0.8f, 0.5f, 0.6f }));
        Assert.That(SmithyRules.CanStart(Hinges, false, false, 1, 1), Is.EqualTo(SmithyRules.Gate.Ok), "a good needs no Sticks or Cordage");
    }

    // --- The stock: Sticks and Cordage as well ---

    [Test]
    public void TheForgeAlsoHoldsSixSticksAndFourCordage()
    {
        Assert.That(SmithyRules.SticksCap, Is.EqualTo(6));
        Assert.That(SmithyRules.CordageCap, Is.EqualTo(4));
        Assert.That(SmithyRules.StockIds, Is.EqualTo(new[] { "iron_bar", "charcoal", "sticks", "cordage" }));
        Assert.That(SmithyRules.CapOf("sticks"), Is.EqualTo(6));
        Assert.That(SmithyRules.CapOf("cordage"), Is.EqualTo(4));
        Assert.That(SmithyRules.CapOf("iron_bar"), Is.EqualTo(8), "the old caps are unchanged");
        Assert.That(SmithyRules.CapOf("charcoal"), Is.EqualTo(8));
        Assert.That(SmithyRules.Accepts("sticks", 5), Is.True);
        Assert.That(SmithyRules.Accepts("sticks", 6), Is.False);
        Assert.That(SmithyRules.Accepts("cordage", 3), Is.True);
        Assert.That(SmithyRules.Accepts("cordage", 4), Is.False);
        Assert.That(SmithyRules.Accepts("iron_shovel", 0), Is.False, "a finished tool is not something to put back");
        Assert.That(SmithyRules.Accepts("logs", 0), Is.False);

        object forge = NewPile("Smithy");
        for (int i = 0; i < 6; i++)
        {
            Assert.That(Accepts(forge, "sticks"), Is.True, $"room for stick {i + 1}");
            Add(forge, "sticks", 1);
        }
        Assert.That(Accepts(forge, "sticks"), Is.False, "the Stick cap of 6");
        for (int i = 0; i < 4; i++)
            Add(forge, "cordage", 1);
        Assert.That(Accepts(forge, "cordage"), Is.False, "the Cordage cap of 4");
        Assert.That(Accepts(forge, "iron_bar"), Is.True, "Bars are separate");
        Assert.That(Accepts(forge, "iron_shovel"), Is.False);
    }

    [Test]
    public void TheStockButtonAndThePackLineKnowAllFourMaterials()
    {
        Assert.That(SmithyRules.StockButtonText, Is.EqualTo("PUT MY IRON, CHARCOAL, STICKS AND CORDAGE IN THE FORGE"));
        Assert.That(SmithyRules.PackLine(3, 5, 6, 2), Is.EqualTo("You carry: 3 Iron Bars, 5 Charcoal, 6 Sticks, 2 Cordage"));
        Assert.That(SmithyRules.PackLine(0, 0, 1, 0), Is.EqualTo("You carry: 1 Stick"));
        Assert.That(SmithyRules.PackLine(0, 0, 0, 3), Is.EqualTo("You carry: 3 Cordage"));
        Assert.That(SmithyRules.PackLine(0, 0, 0, 0), Is.EqualTo("You carry: nothing the forge uses"));

        // Carrying only Sticks or only Cordage is enough to stock, until the forge is full of it.
        Assert.That(SmithyRules.StockState(0, 0, 0, 0, carriedSticks: 3), Is.EqualTo(SmithyRules.StockGate.Can));
        Assert.That(SmithyRules.StockState(0, 0, 0, 0, carriedSticks: 3, forgeSticks: 6), Is.EqualTo(SmithyRules.StockGate.ForgeFull));
        Assert.That(SmithyRules.StockState(0, 0, 0, 0, carriedCordage: 1, forgeCordage: 3), Is.EqualTo(SmithyRules.StockGate.Can));
        Assert.That(SmithyRules.StockState(0, 0, 0, 0, carriedCordage: 1, forgeCordage: 4), Is.EqualTo(SmithyRules.StockGate.ForgeFull));
        Assert.That(SmithyRules.StockState(3, 0, 8, 0, carriedSticks: 2, forgeSticks: 6), Is.EqualTo(SmithyRules.StockGate.ForgeFull), "everything carried is already at its cap");
        Assert.That(SmithyRules.StockState(3, 0, 8, 0, carriedSticks: 2, forgeSticks: 5), Is.EqualTo(SmithyRules.StockGate.Can));
        Assert.That(SmithyRules.StockState(0, 0, 0, 0), Is.EqualTo(SmithyRules.StockGate.NothingCarried));
        Assert.That(SmithyRules.Stored("3 Sticks", 4, 6, 3, 1), Is.EqualTo("Stored 3 Sticks  (the forge holds 4 Iron Bars, 6 Charcoal, 3 Sticks, 1 Cordage)"));
        Assert.That(SmithyRules.CapsText, Is.EqualTo("8 Iron Bars, 8 Charcoal, 6 Sticks and 4 Cordage"));
    }

    [Test]
    public void ThePlacedStructureStocksAndStartsThroughTheFourMaterials()
    {
        string placed = SourceText("Scripts/World/PlacedStructure.cs");
        Assert.That(placed, Does.Contain("state.Take(SmithyRules.SticksId, recipe.sticks);"));
        Assert.That(placed, Does.Contain("state.Take(SmithyRules.CordageId, recipe.cordage);"));
        Assert.That(placed, Does.Contain("state.Count(SmithyRules.SticksId), state.Count(SmithyRules.CordageId));"), "the gate sees Sticks and Cordage");
        Assert.That(placed, Does.Contain("foreach (string id in SmithyRules.StockIds)"), "R stocks every stock material from the pack");
    }

    // --- Starting a tool job, refusals, the finish and handing over ---

    [Test]
    public void AnIronToolJobTakesItsFourIngredientsAtTheStartAndFinishesAfterThreeHours()
    {
        object forge = StockedForge();
        Assert.That(Start(forge, Shovel), Is.True);
        Assert.That(Count(forge, "iron_bar"), Is.EqualTo(2));
        Assert.That(Count(forge, "charcoal"), Is.EqualTo(4));
        Assert.That(Count(forge, "sticks"), Is.EqualTo(3));
        Assert.That(Count(forge, "cordage"), Is.EqualTo(1));
        Assert.That(Get(forge, "jobItem"), Is.EqualTo("iron_shovel"));
        Assert.That((float)Get(forge, "jobHours"), Is.EqualTo(3f));
        RunHours(forge, 2);
        Assert.That(Count(forge, "iron_shovel"), Is.EqualTo(0), "not before the third hour");
        Assert.That(Working(forge), Is.True);
        RunHours(forge, 1);
        Assert.That(Count(forge, "iron_shovel"), Is.EqualTo(1), "one Iron Shovel after 3 h");
        Assert.That(Get(forge, "jobItem"), Is.EqualTo(""));
        Assert.That(Count(forge, "iron_bar"), Is.EqualTo(2), "nothing more is taken at the finish");

        // The tool waits; nothing else starts until it is taken. Then the Pick Axe takes what is left (2 Bars, 2 Charcoal, 2 of 3 Sticks, the last Cordage).
        Assert.That(Gate(forge, PickAxe), Is.EqualTo(SmithyRules.Gate.ProductWaiting));
        Assert.That(Start(forge, PickAxe), Is.False);
        Take(forge, "iron_shovel", 1);
        Assert.That(Gate(forge, PickAxe), Is.EqualTo(SmithyRules.Gate.Ok));
        Assert.That(Start(forge, PickAxe), Is.True);
        Assert.That(Gate(forge, Hinges), Is.EqualTo(SmithyRules.Gate.Busy), "one job at a time");
        RunHours(forge, 3);
        Assert.That(Count(forge, "iron_pick_axe"), Is.EqualTo(1));
        Assert.That((Count(forge, "iron_bar"), Count(forge, "charcoal"), Count(forge, "sticks"), Count(forge, "cordage")), Is.EqualTo((0, 2, 1, 0)));
    }

    [Test]
    public void AToolJobIsRefusedForEachMissingIngredientAndSaysWhat()
    {
        // The Shovel needs 2 Bars, 2 Charcoal, 3 Sticks, 1 Cordage.
        Assert.That(SmithyRules.CanStart(Shovel, false, false, 2, 2, 3, 1), Is.EqualTo(SmithyRules.Gate.Ok));
        Assert.That(SmithyRules.CanStart(Shovel, false, false, 1, 2, 3, 1), Is.EqualTo(SmithyRules.Gate.ShortOfStock), "one Bar is not enough");
        Assert.That(SmithyRules.CanStart(Shovel, false, false, 2, 1, 3, 1), Is.EqualTo(SmithyRules.Gate.ShortOfStock), "one Charcoal is not enough");
        Assert.That(SmithyRules.CanStart(Shovel, false, false, 2, 2, 2, 1), Is.EqualTo(SmithyRules.Gate.ShortOfStock), "two Sticks are not enough for a Shovel");
        Assert.That(SmithyRules.CanStart(PickAxe, false, false, 2, 2, 2, 1), Is.EqualTo(SmithyRules.Gate.Ok), "but are for a Pick Axe");
        Assert.That(SmithyRules.CanStart(Shovel, false, false, 2, 2, 3, 0), Is.EqualTo(SmithyRules.Gate.ShortOfStock), "no Cordage");
        Assert.That(SmithyRules.CanStart(Shovel, true, false, 9, 9, 9, 9), Is.EqualTo(SmithyRules.Gate.Busy));
        Assert.That(SmithyRules.CanStart(Shovel, false, true, 9, 9, 9, 9), Is.EqualTo(SmithyRules.Gate.ProductWaiting));

        Assert.That(SmithyRules.ShortText(Shovel, 0, 0, 0, 0), Is.EqualTo("2 Iron Bars, 2 Charcoal, 3 Sticks, 1 Cordage"));
        Assert.That(SmithyRules.ShortText(Shovel, 2, 2, 2, 1), Is.EqualTo("1 Stick"));
        Assert.That(SmithyRules.ShortText(Shovel, 2, 2, 3, 0), Is.EqualTo("1 Cordage"));
        Assert.That(SmithyRules.ReasonText(SmithyRules.Gate.ShortOfStock, Shovel, default, 2, 2, 0f, 1, 0), Is.EqualTo("Still short 2 Sticks, 1 Cordage. Deliver more."));
        Assert.That(SmithyRules.GateMessage(SmithyRules.Gate.ShortOfStock, PickAxe, 0, 2, 0f, 2, 1), Is.EqualTo("The forge is short of 2 Iron Bars for Iron Pick Axe. Deliver more."));
        Assert.That(SmithyRules.ReasonText(SmithyRules.Gate.ProductWaiting, Shovel, default, 2, 2, 0f), Is.EqualTo("A finished item is waiting: collect it first."));
        Assert.That(SmithyRules.StartButtonText(SmithyRules.Gate.ProductWaiting, Shovel), Is.EqualTo("COLLECT THE ITEM FIRST"));
        Assert.That(SmithyRules.StartButtonText(SmithyRules.Gate.Ok, PickAxe), Is.EqualTo("START IRON PICK AXE"));

        // A short forge refuses on the pile too, and takes nothing.
        object forge = NewPile("Smithy");
        Add(forge, "iron_bar", 2);
        Add(forge, "charcoal", 2);
        Add(forge, "sticks", 2);
        Add(forge, "cordage", 1);
        Assert.That(Start(forge, Shovel), Is.False);
        Assert.That(Count(forge, "iron_bar"), Is.EqualTo(2), "a refused start takes nothing");
        Assert.That(Count(forge, "sticks"), Is.EqualTo(2));
    }

    [Test]
    public void TheNeedRowsShowSticksAndCordageOnlyForTheToolsAndSayWhatIsShort()
    {
        SmithyRules.NeedRow[] goods = SmithyRules.NeedRows(Hinges, 0, 1, 5, 5);
        Assert.That(goods.Length, Is.EqualTo(2), "a good needs only Bars and Charcoal");
        Assert.That(goods[0].Text, Is.EqualTo("Iron Bars - need 1 - forge has 0 - short by 1"));

        SmithyRules.NeedRow[] tool = SmithyRules.NeedRows(Shovel, 1, 2, 2, 0);
        Assert.That(tool.Length, Is.EqualTo(4));
        Assert.That(tool[0].Text, Is.EqualTo("Iron Bars - need 2 - forge has 1 - short by 1"));
        Assert.That(tool[1].Text, Is.EqualTo("Charcoal - need 2 - forge has 2 - enough"));
        Assert.That(tool[2].Text, Is.EqualTo("Sticks - need 3 - forge has 2 - short by 1"));
        Assert.That(tool[3].Text, Is.EqualTo("Cordage - need 1 - forge has 0 - short by 1"));
        Assert.That(SmithyRules.NeedRows(PickAxe, 2, 2, 2, 1).All(r => r.enough), Is.True);
    }

    [Test]
    public void ATextForAToolSaysItIsForYourOwnUseAndNotSold()
    {
        Assert.That(SmithyRules.ListLabel(Shovel, 0), Is.EqualTo("Iron Shovel\n3 h  -  own use"));
        Assert.That(SmithyRules.ListLabel(PickAxe, 999), Is.EqualTo("Iron Pick Axe\n3 h  -  own use"), "a price never shows for a tool");
        Assert.That(SmithyRules.DetailLine(Shovel), Is.EqualTo("3 h  -  1.8 kg  -  your own use, not sold"));
        Assert.That(SmithyRules.DetailLine(PickAxe), Is.EqualTo("3 h  -  2 kg  -  your own use, not sold"));
        Assert.That(SmithyRules.TookMessage(Shovel, 1, 0), Is.EqualTo("Took the Iron Shovel. It is yours to use; it cannot be sold."));
        Assert.That(SmithyRules.TookMessage(Hinges, 1, 0), Is.EqualTo("Took the Door Hinges. Sell it at the Trading Post."), "a good still says to sell it");
        Assert.That(SmithyRules.TookMessage(PickAxe, 1, 1), Is.EqualTo("Took 1 Iron Pick Axe (1 left in the Smithy, no room)."));
        Assert.That(SmithyRules.DonePrompt(Shovel), Is.EqualTo("Collect Iron Shovel  (ready in the Smithy)"));
        Assert.That(SmithyRules.WorkingPrompt(PickAxe, 2.4f), Is.EqualTo("Smithy  (forging Iron Pick Axe, 3 h to go)"));
        Assert.That(SmithyRules.ForgeLine(true, Shovel, 2f, false, default, 2, 4, 3, 1), Is.EqualTo("Forging Iron Shovel, 2 h to go"));
        Assert.That(SmithyRules.ForgeLine(false, default, 0f, true, PickAxe, 2, 4, 3, 1), Is.EqualTo("Iron Pick Axe is ready - collect it"));
        Assert.That(SmithyRules.ForgeLine(false, default, 0f, false, default, 4, 6, 6, 2), Is.EqualTo("Forge is idle - it holds 4 Iron Bars, 6 Charcoal, 6 Sticks, 2 Cordage"));
        Assert.That(SmithyRules.DeliverHelp, Does.Contain("Sticks and Cordage"));
        Assert.That(SmithyRules.BuiltMessage, Does.Contain("8 Iron Bars, 8 Charcoal, 6 Sticks and 4 Cordage"));
        Assert.That(SmithyRules.KeyHint, Is.EqualTo("1-5 choose    Up/Down move    Enter start    R stock    Esc close"));
    }

    [Test]
    public void TheDigitsOneToFiveChooseAndSixDoesNothing()
    {
        for (int i = 0; i < 5; i++)
        {
            var key = (SmithyRules.Key)((int)SmithyRules.Key.One + i);
            SmithyRules.KeyAction action = SmithyRules.Resolve(key, 0, 5);
            Assert.That(action.kind, Is.EqualTo(SmithyRules.ActionKind.Choose), key.ToString());
            Assert.That(action.choice, Is.EqualTo(i));
        }
        Assert.That(SmithyRules.Resolve(SmithyRules.Key.Five, 0, 4).kind, Is.EqualTo(SmithyRules.ActionKind.None), "past the end of a shorter list");
        Assert.That(SmithyRules.Resolve(SmithyRules.Key.Down, 4, 5).choice, Is.EqualTo(0), "Down wraps from the Pick Axe to the Hinges");
        Assert.That(SmithyRules.Resolve(SmithyRules.Key.Up, 0, 5).choice, Is.EqualTo(4), "Up wraps from the Hinges to the Pick Axe");
        Assert.That(SmithyRules.Resolve(SmithyRules.Key.Enter, 3, 5).kind, Is.EqualTo(SmithyRules.ActionKind.Start));
        Assert.That(SmithyRules.Resolve(SmithyRules.Key.Stock, 3, 5).kind, Is.EqualTo(SmithyRules.ActionKind.Stock));
        string screen = SourceText("Scripts/UI/SmithyScreen.cs");
        Assert.That(screen, Does.Contain("keyboard.digit4Key.wasPressedThisFrame"));
        Assert.That(screen, Does.Contain("keyboard.digit5Key.wasPressedThisFrame"));
    }

    // --- The shared tool helpers ---

    [Test]
    public void OneHelperSaysWhichIdsAreAShovelAndWhichArePickAxes()
    {
        Assert.That(ToolRules.ShovelIds, Is.EqualTo(new[] { "shovel", "iron_shovel" }));
        Assert.That(ToolRules.PickAxeIds, Is.EqualTo(new[] { "stone_pick_axe", "iron_pick_axe" }));
        Assert.That(ToolRules.IsShovel("shovel"), Is.True);
        Assert.That(ToolRules.IsShovel("iron_shovel"), Is.True);
        Assert.That(ToolRules.IsShovel("stone_pick_axe"), Is.False);
        Assert.That(ToolRules.IsShovel(null), Is.False);
        Assert.That(ToolRules.IsPickAxe("stone_pick_axe"), Is.True);
        Assert.That(ToolRules.IsPickAxe("iron_pick_axe"), Is.True);
        Assert.That(ToolRules.IsPickAxe("shovel"), Is.False);
        Assert.That(ToolRules.IsPickAxe("axe"), Is.False);
        Assert.That(ToolRules.IsDiggingTool("iron_shovel"), Is.True);
        Assert.That(ToolRules.IsDiggingTool("hammer"), Is.False);
        Assert.That(ToolRules.IsIron("iron_shovel"), Is.True);
        Assert.That(ToolRules.IsIron("shovel"), Is.False);
        Assert.That(ToolRules.IsIron("wood_saw"), Is.False);
        Assert.That(ToolRules.HasShovel(id => id == "iron_shovel"), Is.True, "an iron Shovel alone counts");
        Assert.That(ToolRules.HasShovel(id => id == "shovel"), Is.True);
        Assert.That(ToolRules.HasShovel(id => id == "stone_pick_axe"), Is.False);
        Assert.That(ToolRules.StoneShovelId, Is.EqualTo(Find("AxeTool").GetField("ShovelId").GetRawConstantValue()));
        Assert.That(ToolRules.StonePickAxeId, Is.EqualTo(Find("AxeTool").GetField("PickAxeId").GetRawConstantValue()));
    }

    [Test]
    public void EveryPlaceThatAskedIsItTheShovelOrThePickAxeNowAsksTheHelper()
    {
        var checks = new (string file, string expected)[]
        {
            ("Scripts/Player/AxeTool.cs", "ToolRules.IsDiggingTool(equipped)"),
            ("Scripts/Player/AxeTool.cs", "UpdateDigging(ToolRules.IsPickAxe(equipped));"),
            ("Scripts/World/Stump.cs", "ToolRules.IsShovel(InventoryManager.Instance.EquippedTool.Id)"),
            ("Scripts/World/RockDeposit.cs", "ToolRules.IsPickAxe(inventory.EquippedTool.Id)"),
            ("Scripts/World/ClayBank.cs", "ToolRules.IsShovel(inventory.EquippedTool.Id)"),
            ("Scripts/World/ClayBank.cs", "ToolRules.HasShovel("),
            ("Scripts/World/PlacedStructure.cs", "static bool EquippedShovel =>"),
        };
        foreach ((string file, string expected) in checks)
            Assert.That(SourceText(file).Contains(expected), Is.True, $"{file} uses {expected}");

        // None of them compares an id with the stone tool's constant any more (starting kits and the stone Shovel's price hint still name it).
        var pattern = new Regex(@"EquippedTool\.Id\s*(==|!=)\s*AxeTool\.|equipped\s*==\s*(PickAxeId|ShovelId)|Equipped\(AxeTool\.");
        foreach (string file in Directory.GetFiles(Path.Combine(Application.dataPath, "Scripts"), "*.cs", SearchOption.AllDirectories))
            Assert.That(pattern.IsMatch(File.ReadAllText(file)), Is.False, file);

        string placed = SourceText("Scripts/World/PlacedStructure.cs");
        Assert.That(Regex.Matches(placed, "EquippedShovel").Count, Is.EqualTo(4), "the definition and the three prompts (the shaft, the full pit, the new pit)");
    }

    // --- The speed: 25 percent fewer blows, per action ---

    [Test]
    public void TheIronToolsNeedAQuarterFewerBlowsOnEveryDigAndMine()
    {
        const float f = ToolRules.DefaultIronToolBlowFactor;
        Assert.That(f, Is.EqualTo(0.75f));

        // Mining a Stone (the Pick Axe): 4 blows -> 3.
        Assert.That(ToolRules.BlowsNeeded(4, "stone_pick_axe", f), Is.EqualTo(4f));
        Assert.That(ToolRules.BlowsNeeded(4, "iron_pick_axe", f), Is.EqualTo(3f));
        Assert.That(ToolRules.SwingsAtFullEfficiency(4, "iron_pick_axe", f), Is.EqualTo(3));
        // Digging a stump (the Shovel): 6 blows -> 4.5, which lands on the 5th swing.
        Assert.That(ToolRules.BlowsNeeded(6, "shovel", f), Is.EqualTo(6f));
        Assert.That(ToolRules.BlowsNeeded(6, "iron_shovel", f), Is.EqualTo(4.5f));
        Assert.That(ToolRules.SwingsAtFullEfficiency(6, "iron_shovel", f), Is.EqualTo(5));
        // Clay and Sand (the Shovel): 4 -> 3.
        Assert.That(ToolRules.BlowsNeeded(4, "shovel", f), Is.EqualTo(4f));
        Assert.That(ToolRules.SwingsAtFullEfficiency(4, "iron_shovel", f), Is.EqualTo(3));
        Assert.That(ToolRules.SwingsAtFullEfficiency(4, "shovel", f), Is.EqualTo(4), "the stone Shovel is as it was");
        Assert.That(ToolRules.SwingsAtFullEfficiency(6, "shovel", f), Is.EqualTo(6));

        // The Well shaft (shallow 20, medium 40, deep 60 blows) and a new Outhouse pit (10): the Shovel's swings count for more.
        foreach ((int blows, int ironSwings) in new[] { (20, 15), (40, 30), (60, 45), (10, 8) })
        {
            Assert.That(ToolRules.SwingsAtFullEfficiency(blows, "iron_shovel", f), Is.EqualTo(ironSwings), $"{blows} blows");
            Assert.That(ToolRules.SwingsAtFullEfficiency(blows, "shovel", f), Is.EqualTo(blows));
        }
        Assert.That(ToolRules.SwingWeight(1f, "iron_shovel", f), Is.EqualTo(1f / 0.75f).Within(0.0001f));
        Assert.That(ToolRules.SwingWeight(1f, "shovel", f), Is.EqualTo(1f));
        Assert.That(ToolRules.SwingWeight(0.5f, "iron_shovel", f), Is.EqualTo(0.5f / 0.75f).Within(0.0001f), "tiredness still slows the work");
        Assert.That(ToolRules.SwingWeight(1f, "hammer", f), Is.EqualTo(1f));
    }

    [Test]
    public void TheAxeTheSawAndTheOtherToolsAreNotTouchedByTheIronFactor()
    {
        const float f = ToolRules.DefaultIronToolBlowFactor;
        foreach (string id in new[] { "axe", "primitive_axe", "wood_saw", "hammer", "knife", null })
            Assert.That(ToolRules.BlowFactor(id, f), Is.EqualTo(1f), id ?? "null");
        Assert.That(AxeRules.FactorFor("axe", 1.5f, 0.65f), Is.EqualTo(1f));
        Assert.That(AxeRules.FactorFor("wood_saw", 1.5f, 0.65f), Is.EqualTo(0.65f));
        Assert.That(AxeRules.FactorFor("primitive_axe", 1.5f, 0.65f), Is.EqualTo(1.5f));
        Assert.That(AxeRules.FactorFor("iron_shovel", 1.5f, 0.65f), Is.EqualTo(1f), "the iron Shovel is no felling tool");
        Assert.That(AxeRules.IsFellingTool("iron_pick_axe"), Is.False);
        Assert.That(AxeRules.DefaultSawFactor, Is.EqualTo(0.65f));
        Assert.That(AxeRules.DefaultPrimitiveFactor, Is.EqualTo(1.5f));
    }

    [Test]
    public void TheAxeToolAppliesTheFactorToEachActionAndTheDigRuleIsUntouched()
    {
        string axe = SourceText("Scripts/Player/AxeTool.cs");
        Assert.That(axe, Does.Contain("[SerializeField, Range(0.1f, 1f)] float ironToolBlowFactor = ToolRules.DefaultIronToolBlowFactor;"), "a tunable on the component, as the Saw's");
        Assert.That(axe, Does.Contain("float stoneBlows = ToolRules.BlowsNeeded(blowsPerStone, Equipped, ironToolBlowFactor);"));
        Assert.That(axe, Does.Contain("float stumpBlows = ToolRules.BlowsNeeded(blowsPerStump, Equipped, ironToolBlowFactor);"));
        Assert.That(axe, Does.Contain("float clayBlows = ToolRules.BlowsNeeded(blowsPerClay, Equipped, ironToolBlowFactor);"));
        Assert.That(axe, Does.Contain("if (blows < stoneBlows)"));
        Assert.That(axe, Does.Contain("if (blows < stumpBlows)"));
        Assert.That(axe, Does.Contain("if (blows < clayBlows)"));
        Assert.That(axe, Does.Contain("rules.PitDigBlow(state, DigSwing)"));
        Assert.That(axe, Does.Contain("rules.DigBlow(state, swing)"));
        Assert.That(axe, Does.Contain("float AxeFactor => AxeRules.FactorFor(Equipped, primitiveAxeFactor, sawFactor);"), "the Axe's and the Saw's factor is as it was");
        Assert.That(axe, Does.Contain("[SerializeField, Min(1)] int blowsPerStone = 4;"));
        Assert.That(axe, Does.Contain("[SerializeField, Min(1)] int blowsPerStump = 6;"));
        Assert.That(axe, Does.Contain("[SerializeField, Min(1)] int blowsPerClay = 4;"), "the stone tools' blow counts are unchanged");

        // The dig rule (what the Shovel finds where it is aimed) knows nothing of tools.
        Assert.That(SourceText("Scripts/Economy/PotteryRules.cs").Contains("ToolRules"), Is.False);
        Assert.That(SourceText("Scripts/Economy/CreekBankRules.cs").Contains("ToolRules"), Is.False);
        Assert.That(SourceText("Scripts/Economy/PotteryRules.cs").Contains("ironTool"), Is.False);
        Assert.That(axe, Does.Contain("dig = DigAt(hit.point, hit.collider.GetComponentInParent<ClayBank>() != null);"), "DigFor is called exactly as before");
    }

    // --- The items ---

    [Test]
    public void TheTwoIronToolsAreToolItemsInTheStoneToolsFolder()
    {
        foreach ((string id, string name, float kg, string stoneId) in new[] { ("iron_shovel", "Iron Shovel", 1.8f, "shovel"), ("iron_pick_axe", "Iron Pick Axe", 2f, "stone_pick_axe") })
        {
            string path = $"Assets/Resources/Items/Tools/{id}.asset";
            ScriptableObject item = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            ScriptableObject stone = AssetDatabase.LoadAssetAtPath<ScriptableObject>($"Assets/Resources/Items/Tools/{stoneId}.asset");
            Assert.That(item, Is.Not.Null, path);
            Assert.That(stone, Is.Not.Null);
            Type type = item.GetType();
            Assert.That(type.GetProperty("Id").GetValue(item), Is.EqualTo(id));
            Assert.That(type.GetProperty("DisplayName").GetValue(item), Is.EqualTo(name));
            Assert.That((float)type.GetProperty("WeightKg").GetValue(item), Is.EqualTo(kg));
            Assert.That(type.GetProperty("Category").GetValue(item).ToString(), Is.EqualTo("Tool"));
            Assert.That(type.GetProperty("Category").GetValue(item), Is.EqualTo(type.GetProperty("Category").GetValue(stone)), "the same item type as the stone tool");
            Assert.That(type.GetProperty("ShelfLifeDays").GetValue(item), Is.EqualTo(0f));
            Assert.That(File.Exists(Path.Combine(Application.dataPath, $"Resources/Items/Tools/{id}.asset.meta")), Is.True, "with its meta");
            Assert.That(SmithyRules.TryFind(id, out SmithyRules.Recipe recipe) && recipe.weightKg == kg && recipe.name == name, Is.True, "the rules and the asset agree");
        }
        // The stone tools are unchanged.
        ScriptableObject shovel = AssetDatabase.LoadAssetAtPath<ScriptableObject>("Assets/Resources/Items/Tools/shovel.asset");
        Assert.That(shovel.GetType().GetProperty("DisplayName").GetValue(shovel), Is.EqualTo("Primitive Shovel"));
        Assert.That((float)shovel.GetType().GetProperty("WeightKg").GetValue(shovel), Is.EqualTo(1.8f));
        ScriptableObject pick = AssetDatabase.LoadAssetAtPath<ScriptableObject>("Assets/Resources/Items/Tools/stone_pick_axe.asset");
        Assert.That(pick.GetType().GetProperty("DisplayName").GetValue(pick), Is.EqualTo("Stone Pick Axe"));
        Assert.That((float)pick.GetType().GetProperty("WeightKg").GetValue(pick), Is.EqualTo(2f));
    }

    [Test]
    public void TheStoneToolsAreStillCraftableAtTheSamePriceAndTheIronOnesHaveNoRecipeOrRow()
    {
        string crafting = SourceText("Scripts/Inventory/Crafting.cs");
        Assert.That(crafting, Does.Contain("Make(\"stone_pick_axe\", (\"sticks\", 2), (\"cordage\", 1), (\"stone\", 1)),"));
        Assert.That(crafting, Does.Contain("Make(\"shovel\", (\"sticks\", 3), (\"cordage\", 1), (\"stone\", 1)),"));
        Assert.That(crafting.Contains("iron_shovel") || crafting.Contains("iron_pick_axe"), Is.False, "the iron tools are made only at the Smithy");

        MarketTable table = MarketCatalog.Table;
        Assert.That(table.SellBaseCents("shovel"), Is.EqualTo(400L), "the stone Shovel sells for $4 as before");
        Assert.That(table.SellBaseCents("stone_pick_axe"), Is.EqualTo(400L), "and the Stone Pick Axe");
        Assert.That(table.TryGetRow("shovel", out MarketRow shovelRow) && shovelRow.group == MarketGroup.Crafted && shovelRow.kind == MarketKind.Bought, Is.True);
        foreach (string id in new[] { "iron_shovel", "iron_pick_axe" })
        {
            Assert.That(table.TryGetRow(id, out _), Is.False, id + " has no market row");
            Assert.That(table.SellBaseCents(id), Is.EqualTo(0L), id + " cannot be sold, so the Trading Post list never shows it");
            Assert.That(table.BuyBaseCents(id), Is.EqualTo(0L), id + " cannot be bought");
        }
        Assert.That(table.Rows.Count, Is.EqualTo(92), "no market row was added");
        Assert.That(table.Rows.Count(r => r.group == MarketGroup.Crafted), Is.EqualTo(25));
        Assert.That(table.Stock.Count, Is.EqualTo(22));
        Assert.That(LedgerCategories.Count, Is.EqualTo(19), "and no Ledger line");
        Assert.That(SourceText("Scripts/Economy/MarketCatalog.cs").Contains("iron_shovel"), Is.False);
        Assert.That(SourceText("Resources/Market/MarketData.asset").Contains("iron_shovel"), Is.False);
        Assert.That(SourceText("Scripts/Economy/Ledger.cs").Contains("iron_shovel"), Is.False);
        Assert.That(SourceText("Scripts/Managers/DifficultyManager.cs").Contains("iron_"), Is.False, "the starting kits are the stone tools");
    }

    // --- The look: a tool waiting on the anvil ---

    static List<GameObject> BuildVisuals(object state)
    {
        var parent = new GameObject("Smithy tool visuals test");
        var pieces = new List<GameObject>();
        object[] args = { parent.transform, state, null, pieces, Vector3.zero, Vector3.zero };
        Find("StructureVisuals").GetMethod("Build").Invoke(null, args);
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
    public void AFinishedToolLiesOnTheAnvilAsAHaftAndAGreyHeadAndStockIsNotDrawnForSticks()
    {
        var empty = BuildVisuals(NewPile("Smithy"));
        object shovelPile = NewPile("Smithy");
        Add(shovelPile, "iron_shovel", 1);
        var shovel = BuildVisuals(shovelPile);
        object pickPile = NewPile("Smithy");
        Add(pickPile, "iron_pick_axe", 1);
        var pick = BuildVisuals(pickPile);
        object stickPile = NewPile("Smithy");
        Add(stickPile, "sticks", 6);
        Add(stickPile, "cordage", 4);
        var sticks = BuildVisuals(stickPile);
        try
        {
            Assert.That(shovel.Count - empty.Count, Is.EqualTo(2), "a wooden haft and a flat grey blade");
            Assert.That(pick.Count - empty.Count, Is.EqualTo(2), "a wooden haft and a grey head");
            Assert.That(sticks.Count - empty.Count, Is.EqualTo(0), "the stocked Sticks and Cordage add no pieces (the Bars and Charcoal are the drawn stock)");
        }
        finally
        {
            Cleanup(empty);
            Cleanup(shovel);
            Cleanup(pick);
            Cleanup(sticks);
        }
        string placed = SourceText("Scripts/World/PlacedStructure.cs");
        Assert.That(placed, Does.Contain("case ToolRules.IronShovelId:"));
        Assert.That(placed, Does.Contain("case ToolRules.IronPickAxeId:"));
        // The Shovel's blade (a 2.5 cm slab at y 0.025, centre x 0.26) overlaps the haft's end (x 0.25) and sits at a different height, so no faces share a plane.
        Assert.That(0.26f - 0.14f / 2f, Is.LessThan(0.25f));
        Assert.That(0.025f + 0.025f / 2f, Is.Not.EqualTo(0.03f + 0.015f), "the blade's top and the haft's top are different planes");
        // And no tool is drawn in the hand: the equipped tool is purely logical in this game, for the stone tools too.
        Assert.That(SourceText("Scripts/Player/AxeTool.cs").Contains("CreatePrimitive"), Is.False);
    }

    // --- The Smithy screen with five entries ---

    static Type Screen => Find("SmithyScreen");

    [Test]
    public void TheScreenBuildsFiveRowsAndFourNeedLinesWithNothingUnderTwentySixAndFitsThePanel()
    {
        Type text = Type.GetType("UnityEngine.UI.Text, UnityEngine.UI");
        var go = new GameObject("Smithy Screen Tool Test");
        Component instance = go.AddComponent(Screen);
        Screen.GetMethod("Build", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(instance, null);
        try
        {
            PropertyInfo size = text.GetProperty("fontSize");
            Component[] labels = go.GetComponentsInChildren(text, true);
            foreach (Component label in labels)
                Assert.That((int)size.GetValue(label), Is.GreaterThanOrEqualTo(26), label.name);
            Assert.That(labels.Count(l => l.name == "Label" && l.transform.parent.name.StartsWith("Row")), Is.EqualTo(5));
            Assert.That(labels.Count(l => l.name.StartsWith("Need ")), Is.EqualTo(4));
            Assert.That(labels.Where(l => l.name.StartsWith("Need ")).Select(l => (int)size.GetValue(l)), Is.All.EqualTo(28), "body text 28");
            Assert.That(labels.Where(l => l.name == "Detail").Select(l => (int)size.GetValue(l)), Is.All.EqualTo(28));
            Assert.That(labels.Where(l => l.name == "Label" && l.transform.parent.name.StartsWith("Row")).Select(l => (int)size.GetValue(l)), Is.All.EqualTo(32));

            RectTransform[] all = go.GetComponentsInChildren<RectTransform>(true);
            Rect Box(string name)
            {
                RectTransform rt = all.First(r => r.name == name);
                return new Rect(rt.anchoredPosition.x, -rt.anchoredPosition.y, rt.sizeDelta.x, rt.sizeDelta.y);
            }
            string[] names =
            {
                "Title", "Close", "Status", "Pack", "Row 0", "Row 1", "Row 2", "Row 3", "Row 4", "Stock Reason", "Stock", "Piece", "Piece Line", "Needs Heading", "Need 0", "Need 1", "Need 2", "Need 3",
                "Reason", "Start", "Hint",
            };
            var panel = new Rect(0f, 0f, SmithyRules.PanelWidthPx, SmithyRules.PanelHeightPx);
            foreach (string name in names)
            {
                Rect b = Box(name);
                Assert.That(b.xMin, Is.GreaterThanOrEqualTo(panel.xMin), name);
                Assert.That(b.yMin, Is.GreaterThanOrEqualTo(panel.yMin), name);
                Assert.That(b.xMax, Is.LessThanOrEqualTo(panel.xMax), name);
                Assert.That(b.yMax, Is.LessThanOrEqualTo(panel.yMax), name);
            }
            for (int a = 0; a < names.Length; a++)
                for (int b = a + 1; b < names.Length; b++)
                    Assert.That(Box(names[a]).Overlaps(Box(names[b])), Is.False, names[a] + " overlaps " + names[b]);

            // How five rows fit the 920 px panel: five 88 px rows and four 8 px gaps run from y 204 to 676; the 72 px reason line and the 88 px stock button end at 850, above the hint at 860.
            Assert.That(Box("Row 4").yMax, Is.EqualTo(676f));
            Assert.That(Box("Stock").yMax, Is.EqualTo(850f));
            Assert.That(Box("Hint").yMin, Is.EqualTo(860f));
            Assert.That(Box("Stock").yMax, Is.LessThan(Box("Hint").yMin));
            Assert.That(Box("Start").yMax, Is.LessThan(Box("Hint").yMin));
            Assert.That(Box("Row 4").yMax - Box("Row 0").yMin, Is.EqualTo(472f));
            Assert.That(Screen.GetField("RowHeight").GetRawConstantValue(), Is.EqualTo(88f));
            Assert.That(Screen.GetField("RowGap").GetRawConstantValue(), Is.EqualTo(8f));
            Assert.That(Box("Stock").height, Is.GreaterThanOrEqualTo(80f));
            Assert.That(Box("Row 0").height, Is.GreaterThanOrEqualTo(80f), "room for a 32 px name and a 28 px line");
            Assert.That(labels.Any(l => (string)text.GetProperty("text").GetValue(l) == SmithyRules.StockButtonText), Is.True);
            Assert.That(labels.Any(l => l.name == "Hint" && (string)text.GetProperty("text").GetValue(l) == SmithyRules.KeyHint), Is.True);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(go);
        }

        // The panel is the same 1500 x 920 and still never clips on 21:9 or 32:9 (the scaler matches the height there).
        Assert.That(SmithyRules.PanelWidthPx, Is.EqualTo(1500f));
        Assert.That(SmithyRules.PanelHeightPx, Is.EqualTo(920f));
        Assert.That(SmithyRules.ScreenMatch(21f / 9f), Is.EqualTo(1f));
        Assert.That(SmithyRules.ScreenMatch(32f / 9f), Is.EqualTo(1f));
        Assert.That(SmithyRules.ScreenMatch(4f / 3f), Is.EqualTo(0f));
    }

    // --- Know-how, help and the docs file ---

    [Test]
    public void TheSmithyEntryNowTeachesTheToolsAndTheItemHelpExists()
    {
        Assert.That(KnowHowRules.Entries.Length, Is.EqualTo(11), "updated in place, not a twelfth entry");
        Assert.That(KnowHowRules.TryGet(KnowHowRules.Smithy, out KnowHowRules.Entry smithy), Is.True);
        Assert.That(smithy.title, Is.EqualTo("The Smithy: Forging Iron Goods"));
        Assert.That(smithy.body, Does.Contain("an iron Shovel and Pick Axe for your own use"));
        Assert.That(smithy.body, Does.Contain("An Iron Shovel (2 Iron Bars, 2 Charcoal, 3 Sticks and 1 Cordage) and an Iron Pick Axe (2 Iron Bars, 2 Charcoal, 2 Sticks and 1 Cordage) take 3 game hours each."));
        Assert.That(smithy.body, Does.Contain("a quarter fewer blows"));
        Assert.That(smithy.body, Does.Contain("cannot be sold"));
        Assert.That(smithy.body, Does.Contain("it holds 8 Iron Bars, 8 Charcoal, 6 Sticks and 4 Cordage"));
        Assert.That(smithy.body, Does.Contain("Door Hinges take 3 game hours; a Hook Set and a Box of Nails take 2."));
        Assert.That(smithy.body, Does.Contain("about $26").And.Contain("about $18").And.Contain("about $16"), "the goods' prices are as they were");
        Assert.That(KnowHowRules.ForItem("iron_bar"), Is.EqualTo(new[] { "smelting", "smithy" }), "the unlocks are unchanged");
        Assert.That(KnowHowRules.ForItem("iron_shovel"), Is.Empty);
        Assert.That(KnowHowRules.ForHappening(KnowHowRules.Happening.SmithySitePlaced), Is.EqualTo(new[] { "smithy" }));
        KnowHowRules.TryGet(KnowHowRules.Smelting, out KnowHowRules.Entry smelting);
        Assert.That(smelting.body, Does.Contain("The Smithy: Forging Iron Goods"), "the Bloomery entry still points to it");

        foreach (string id in new[] { "iron_shovel", "iron_pick_axe" })
        {
            string tip = KnowHowRules.ItemTip(id);
            Assert.That(tip, Does.Contain("Made at a Smithy").And.Contain("not sold"), id);
            Assert.That(tip.Length, Is.LessThanOrEqualTo(80), id + " fits two lines of the item row");
        }
        Assert.That(KnowHowRules.ItemTip("door_hinges"), Does.Contain("$26"), "the goods' tips are unchanged");

        string craft = SourceText("Scripts/UI/CraftingScreen.cs");
        Assert.That(craft, Does.Contain("stock the forge with Iron Bars, Charcoal, Sticks and Cordage"));
        Assert.That(craft, Does.Contain("an Iron Shovel or Iron Pick Axe takes {SmithyRules.Recipes[3].hours}"));
        Assert.That(craft, Does.Contain("the iron tools are for your own use"));
        Assert.That(craft, Does.Contain("case \"shovel\": return \"digs out stumps, Clay at the water's edge and in the shallows, and Sand up the creek and pond banks.\";"), "the stone tools' help is unchanged");
        Assert.That(craft, Does.Contain("case PileKind.SmithySite:"), "the site's Build-tab help is unchanged");
    }

    [Test]
    public void TheDocsFileCarriesTheChangedEntryAndMatchesTheCode()
    {
        string path = Path.GetFullPath(Path.Combine(Application.dataPath, "../../../05_Documentation/Game_Systems/Journal_KnowHow_Text.md"));
        if (!File.Exists(path))
            Assert.Ignore("Journal_KnowHow_Text.md is not at " + path);
        string text = File.ReadAllText(path).Replace("\r\n", "\n");
        Assert.That(text, Does.Contain("## 11. The Smithy: Forging Iron Goods"));
        Assert.That(text, Does.Contain("Trigger: first Smithy site placed, or first Iron Bar held."));
        Assert.That(text, Does.Contain("and into an iron Shovel and Pick Axe for your own use."));
        Assert.That(text, Does.Contain("An Iron Shovel (2 Iron Bars, 2 Charcoal, 3 Sticks and 1 Cordage) and an Iron Pick Axe (2 Iron Bars, 2 Charcoal, 2 Sticks and 1 Cordage) take 3 game hours each."));
        Assert.That(text, Does.Not.Contain("it holds 8 of each"));
        Assert.That(Regex.Matches(text, @"^## \d+\. ", RegexOptions.Multiline).Count, Is.EqualTo(11));
    }

    // --- Saving ---

    [Test]
    public void NoSavedFieldWasAddedAToolJobUsesJobItemAndJobHoursAndOldSavesLoad()
    {
        string[] fields = Find("WoodPileState").GetFields(BindingFlags.Public | BindingFlags.Instance).Select(f => f.Name).ToArray();
        Assert.That(fields.Length, Is.EqualTo(17), "still the 17 fields");
        Assert.That(fields.Any(f => f.ToLowerInvariant().Contains("stick") || f.ToLowerInvariant().Contains("cordage") || f.ToLowerInvariant().Contains("tool")), Is.False);
        Assert.That(((int)Find("SaveManager").GetField("SaveVersion", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static).GetRawConstantValue()), Is.EqualTo(1));
        Assert.That(LedgerSaveData.CurrentVersion, Is.EqualTo(1));
        Assert.That(Enum.GetValues(Find("PileKind")).Length, Is.EqualTo(45), "no new kind");
        Assert.That((int)Enum.Parse(Find("PileKind"), "Smithy"), Is.EqualTo(44));
        Assert.That(CharcoalRules.BurnMarker, Is.EqualTo("charcoal_burn"));
        Assert.That(BloomeryRules.SmeltMarker, Is.EqualTo("iron_smelt"));

        // A save in the middle of an Iron Shovel job, with a Pick Axe's worth of Sticks left: the stock, the job and the hours come back.
        object forge = StockedForge();
        Assert.That(Start(forge, Shovel), Is.True);
        RunHours(forge, 1);
        string json = JsonUtility.ToJson(forge);
        object back = JsonUtility.FromJson(json, Find("WoodPileState"));
        Assert.That(Get(back, "jobItem"), Is.EqualTo("iron_shovel"));
        Assert.That((float)Get(back, "jobHours"), Is.EqualTo(2f));
        Assert.That((Count(back, "iron_bar"), Count(back, "charcoal"), Count(back, "sticks"), Count(back, "cordage")), Is.EqualTo((2, 4, 3, 1)));
        RunHours(back, 2);
        Assert.That(Count(back, "iron_shovel"), Is.EqualTo(1), "the loaded job finishes on time");

        // A finished tool waiting also survives a save.
        object waiting = JsonUtility.FromJson(JsonUtility.ToJson(back), Find("WoodPileState"));
        Assert.That(Count(waiting, "iron_shovel"), Is.EqualTo(1));

        // An M3 save: a Smithy with Bars and Charcoal and a good in the oven, and no Sticks or Cordage. It loads, shows 0 of each, and a tool simply reads as short.
        object old = JsonUtility.FromJson("{\"id\":9,\"kind\":44,\"jobItem\":\"hook_set\",\"jobHours\":1}", Find("WoodPileState"));
        Assert.That(Get(old, "jobItem"), Is.EqualTo("hook_set"));
        Assert.That(Count(old, "sticks"), Is.EqualTo(0));
        Assert.That(Gate(old, Shovel), Is.EqualTo(SmithyRules.Gate.Busy));
        RunHours(old, 1);
        Assert.That(Count(old, "hook_set"), Is.EqualTo(1), "the M3 job finishes as before");
        Add(old, "iron_bar", 2);
        Add(old, "charcoal", 2);
        Take(old, "hook_set", 1);
        Assert.That(Gate(old, Shovel), Is.EqualTo(SmithyRules.Gate.ShortOfStock));
    }

    [Test]
    public void DismantlingARunningSmithyIsStillRefusedAndAFinishedToolAndTheStockComeBackWhole()
    {
        Assert.That(SmithyRules.DismantlePrompt(true, 2f), Is.EqualTo("Can't dismantle the Smithy while something is being forged  (2 h to go)"));
        Assert.That(SmithyRules.DismantlePrompt(false, 0f), Does.Contain("any finished item come back whole"));
        string placed = SourceText("Scripts/World/PlacedStructure.cs");
        Assert.That(placed, Does.Contain("if (state.kind == PileKind.Smithy && SmithyWorking)"));
        Assert.That(placed, Does.Contain("foreach (string stockId in SmithyRules.StockIds)\n                refund.Return(stockId, state.Count(stockId));"), "Bars, Charcoal, Sticks and Cordage");
        Assert.That(placed, Does.Contain("foreach (SmithyRules.Recipe good in SmithyRules.Recipes)\n                refund.Return(good.itemId, state.Count(good.itemId));"), "the goods and the tools");
        Assert.That(SmithyRules.StockIds, Does.Contain("sticks").And.Contain("cordage"));
    }

    // --- The dev keys ---

    [Test]
    public void TheDevKeysStockTheSmithyForBothTools()
    {
        string keys = SourceText("Scripts/Debug/TimeDebugControls.cs");
        Assert.That(keys, Does.StartWith("#if UNITY_EDITOR || DEVELOPMENT_BUILD"));
        Assert.That(keys, Does.Contain("smithy.Add(SmithyRules.BarId, 4);"));
        Assert.That(keys, Does.Contain("smithy.Add(SmithyRules.CharcoalId, 6);"));
        Assert.That(keys, Does.Contain("smithy.Add(SmithyRules.SticksId, 6);"));
        Assert.That(keys, Does.Contain("smithy.Add(SmithyRules.CordageId, 2);"));
        Assert.That(keys, Does.Contain("PileKind.Smithy, ForgeAhead(4f)"));
        Assert.That(keys, Does.Contain("PileKind.RockStorage, ForgeAhead(3f, -1.2f)"), "plain minus is unchanged");
        Assert.That(keys, Does.Contain("DebugFinishForges()"), "Ctrl+minus is unchanged");
        Assert.That(keys, Does.Contain("keyboard.equalsKey.wasPressedThisFrame && keyboard.shiftKey.isPressed"), "Shift+equals still gives Iron Bars");

        // 4 Bars, 6 Charcoal, 6 Sticks and 2 Cordage cover one Shovel (2, 2, 3, 1) and one Pick Axe (2, 2, 2, 1) with 2 Charcoal and 1 Stick over.
        object forge = StockedForge();
        Assert.That(Start(forge, Shovel), Is.True);
        RunHours(forge, 3);
        Take(forge, "iron_shovel", 1);
        Assert.That(Start(forge, PickAxe), Is.True);
        RunHours(forge, 3);
        Assert.That(Count(forge, "iron_pick_axe"), Is.EqualTo(1));
        Assert.That((Count(forge, "iron_bar"), Count(forge, "charcoal"), Count(forge, "sticks"), Count(forge, "cordage")), Is.EqualTo((0, 2, 1, 0)));
        Assert.That(SmithyRules.BarsCap >= 4 && SmithyRules.CharcoalCap >= 6 && SmithyRules.SticksCap >= 6 && SmithyRules.CordageCap >= 2, Is.True, "the dev stock is within the caps");
    }

    // --- Nothing else changed ---

    [Test]
    public void ThePitBloomeryKilnHopperWorkshopPotteryAndTheMarketAreUntouched()
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
        Assert.That(SmithyRules.SiteStone + SmithyRules.SiteClay + SmithyRules.SiteLogs + SmithyRules.SiteBranches + SmithyRules.SiteTallGrass, Is.EqualTo(21));
        Assert.That(SmithyRules.BuildBlows, Is.EqualTo(40));
        Assert.That(SmithyRules.BarsCap, Is.EqualTo(8));
        Assert.That(MarketCatalog.Table.BuyBaseCents("iron_ore"), Is.EqualTo(400L));
        Assert.That(MarketCatalog.Table.SellBaseCents("door_hinges"), Is.EqualTo(2600L));
        Assert.That(MarketCatalog.Table.SellBaseCents("hook_set"), Is.EqualTo(1800L));
        Assert.That(MarketCatalog.Table.SellBaseCents("nails_box"), Is.EqualTo(1600L));
        Assert.That(MarketCatalog.Table.SellBaseCents("clay_cup"), Is.EqualTo(400L));
        Assert.That(MarketCatalog.Table.SellBaseCents("rough_stool"), Is.EqualTo(1400L));
        Assert.That(MarketCatalog.Table.SellBaseCents("wood_saw"), Is.GreaterThan(0L), "the Wood Saw is still sold");
        Assert.That(AxeRules.SawBaseCents, Is.EqualTo(3000L));
        Assert.That(SourceText("Scripts/Editor/MarketDataSeeder.cs").Contains("iron_"), Is.False, "the seeder was not touched");
    }
}
