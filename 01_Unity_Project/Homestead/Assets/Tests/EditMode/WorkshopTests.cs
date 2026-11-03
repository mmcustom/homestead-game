using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

// EditMode tests for the Workshop, phase W1(a1) (Mike, 2026-10-31): WorkshopRules (the numbers, the gates that stop a job starting, the job's hours, the stock caps, the
// clearances and the prompt texts), the Rough Stool's market row and sale, the sprout radius, and the parts in the main assembly (pile kinds, the staged site's rows, the
// two saved fields, FinishJob), reached by reflection as the other world tests do. Nothing here touches the Saves folder or a scene.
public class WorkshopTests
{
    static Type Find(string name) =>
        AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => { try { return a.GetTypes(); } catch (Exception) { return new Type[0]; } })
                 .First(t => t.Name == name);

    static object Kind(string name) => Enum.Parse(Find("PileKind"), name);

    static object Static(string type, string method, params object[] args) =>
        Find(type).GetMethod(method, args.Select(a => a.GetType()).ToArray()).Invoke(null, args);

    static object NewPile(string kind)
    {
        object state = Activator.CreateInstance(Find("WoodPileState"));
        Find("WoodPileState").GetField("kind").SetValue(state, Kind(kind));
        return state;
    }

    static void Add(object pile, string id, int count) => pile.GetType().GetMethod("Add").Invoke(pile, new object[] { id, count, 0 });
    static int Count(object pile, string id) => (int)pile.GetType().GetMethod("Count").Invoke(pile, new object[] { id });

    static WorkshopRules.Recipe Stool => WorkshopRules.Recipes[0];
    static MarketTable Table => MarketCatalog.Table;

    // --- The numbers ---

    [Test]
    public void TheSiteCostsFiveLogsTenBranchesTenTallGrassAndThreeCordageAndFortyBlows()
    {
        Assert.That(WorkshopRules.SiteLogs, Is.EqualTo(5));
        Assert.That(WorkshopRules.SiteBranches, Is.EqualTo(10));
        Assert.That(WorkshopRules.SiteTallGrass, Is.EqualTo(10));
        Assert.That(WorkshopRules.SiteCordage, Is.EqualTo(3));
        Assert.That(WorkshopRules.BuildBlows, Is.EqualTo(40));
        Assert.That(WorkshopRules.SiteCostText, Is.EqualTo("5 Logs, 10 Branches, 10 Tall Grass and 3 Cordage"));
        float kg = 5 * 8f + 10 * 1f + 10 * 0.1f + 3 * 0.3f;
        Assert.That(kg, Is.GreaterThan(15f), "so a staged site, not a single trip");
    }

    [Test]
    public void TheRoughStoolIsTwoLogsTwoBranchesThreeHoursFourteenDollarsTenKilograms()
    {
        Assert.That(Stool.itemId, Is.EqualTo("rough_stool"));
        Assert.That(Stool.name, Is.EqualTo("Rough Stool"));
        Assert.That((Stool.logs, Stool.branches, Stool.cordage), Is.EqualTo((2, 2, 0)));
        Assert.That(Stool.hours, Is.EqualTo(3));
        Assert.That(Stool.baseCents, Is.EqualTo(1400L));
        Assert.That(Stool.weightKg, Is.EqualTo(10f));
        Assert.That(WorkshopRules.IsProduct("rough_stool"), Is.True);
        Assert.That(WorkshopRules.IsProduct("logs"), Is.False);
    }

    // --- The stock caps ---

    [TestCase("logs", 5, true)]
    [TestCase("logs", 6, false)]
    [TestCase("branches", 7, true)]
    [TestCase("branches", 8, false)]
    [TestCase("cordage", 3, true)]
    [TestCase("cordage", 4, false)]
    [TestCase("firewood", 0, false)]
    [TestCase("tall_grass", 0, false)]
    [TestCase("rough_stool", 0, false)]
    public void TheBenchTakesLogsBranchesAndCordageUpToItsCaps(string itemId, int held, bool accepted)
    {
        Assert.That(WorkshopRules.Accepts(itemId, held), Is.EqualTo(accepted));
    }

    [Test]
    public void TheCapsAreTwiceTheBiggestSingleNeed()
    {
        Assert.That((WorkshopRules.LogsCap, WorkshopRules.BranchesCap, WorkshopRules.CordageCap), Is.EqualTo((6, 8, 4)));
        Assert.That(WorkshopRules.CapOf("logs"), Is.EqualTo(6));
        Assert.That(WorkshopRules.CapOf("branches"), Is.EqualTo(8));
        Assert.That(WorkshopRules.CapOf("cordage"), Is.EqualTo(4));
        Assert.That(WorkshopRules.CapOf("stone"), Is.EqualTo(0));
        Assert.That(WorkshopRules.StockIds, Is.EquivalentTo(new[] { "logs", "branches", "cordage" }));
        Assert.That(WorkshopRules.CapsText, Is.EqualTo("6 Logs, 8 Branches and 4 Cordage"));
    }

    [Test]
    public void ThePiecesToEverStockFitTheCaps()
    {
        foreach (WorkshopRules.Recipe recipe in WorkshopRules.Recipes)
        {
            Assert.That(recipe.logs, Is.LessThanOrEqualTo(WorkshopRules.LogsCap), recipe.name);
            Assert.That(recipe.branches, Is.LessThanOrEqualTo(WorkshopRules.BranchesCap), recipe.name);
            Assert.That(recipe.cordage, Is.LessThanOrEqualTo(WorkshopRules.CordageCap), recipe.name);
        }
    }

    // --- The gates that stop a job starting ---

    [Test]
    public void AJobStartsWithAToolAndTheStock()
    {
        Assert.That(WorkshopRules.CanStart(Stool, true, false, false, 2, 2, 0), Is.EqualTo(WorkshopRules.Gate.Ok));
        Assert.That(WorkshopRules.CanStart(Stool, true, false, false, 6, 8, 4), Is.EqualTo(WorkshopRules.Gate.Ok), "more than enough is fine");
    }

    [Test]
    public void WithoutAFellingToolNoJobStarts()
    {
        Assert.That(WorkshopRules.CanStart(Stool, false, false, false, 2, 2, 0), Is.EqualTo(WorkshopRules.Gate.NoTool));
    }

    [TestCase(1, 2, 0)]
    [TestCase(2, 1, 0)]
    [TestCase(0, 0, 0)]
    [TestCase(1, 1, 4)]
    public void ShortOfStockNoJobStarts(int logs, int branches, int cordage)
    {
        Assert.That(WorkshopRules.CanStart(Stool, true, false, false, logs, branches, cordage), Is.EqualTo(WorkshopRules.Gate.ShortOfStock));
    }

    [Test]
    public void WhileAJobRunsTheBenchIsBusyAndAWaitingPieceBlocksTheNext()
    {
        Assert.That(WorkshopRules.CanStart(Stool, true, true, false, 6, 8, 4), Is.EqualTo(WorkshopRules.Gate.Busy));
        Assert.That(WorkshopRules.CanStart(Stool, true, false, true, 6, 8, 4), Is.EqualTo(WorkshopRules.Gate.ProductWaiting));
    }

    [Test]
    public void TheGatesAreCheckedInOrderBusyThenWaitingThenToolThenStock()
    {
        Assert.That(WorkshopRules.CanStart(Stool, false, true, true, 0, 0, 0), Is.EqualTo(WorkshopRules.Gate.Busy));
        Assert.That(WorkshopRules.CanStart(Stool, false, false, true, 0, 0, 0), Is.EqualTo(WorkshopRules.Gate.ProductWaiting));
        Assert.That(WorkshopRules.CanStart(Stool, false, false, false, 0, 0, 0), Is.EqualTo(WorkshopRules.Gate.NoTool));
        Assert.That(WorkshopRules.CanStart(Stool, true, false, false, 0, 0, 0), Is.EqualTo(WorkshopRules.Gate.ShortOfStock));
    }

    [Test]
    public void AnyFellingToolInThePackOpensTheBench()
    {
        const float p = AxeRules.DefaultPrimitiveFactor, s = AxeRules.DefaultSawFactor;
        Assert.That(AxeRules.FastestCarried(true, false, false, p, s), Is.GreaterThan(0f), "the Axe");
        Assert.That(AxeRules.FastestCarried(false, true, false, p, s), Is.GreaterThan(0f), "the Primitive Axe");
        Assert.That(AxeRules.FastestCarried(false, false, true, p, s), Is.GreaterThan(0f), "the Wood Saw");
        Assert.That(AxeRules.FastestCarried(false, false, false, p, s), Is.EqualTo(0f), "no tool, no job");
    }

    // --- The job's hours ---

    [Test]
    public void AThreeHourJobIsDoneOnTheThirdHourAndStaysDone()
    {
        float hours = Stool.hours;
        Assert.That(WorkshopRules.IsDone(hours), Is.False);
        int ticks = 0;
        while (!WorkshopRules.IsDone(hours))
        {
            float before = hours;
            hours = WorkshopRules.AfterHour(before);
            ticks++;
            Assert.That(WorkshopRules.FinishedThisHour(before, hours), Is.EqualTo(WorkshopRules.IsDone(hours)));
        }
        Assert.That(ticks, Is.EqualTo(3));
        Assert.That(WorkshopRules.AfterHour(0f), Is.EqualTo(0f), "an idle bench stays at zero");
        Assert.That(WorkshopRules.FinishedThisHour(0f, 0f), Is.False);
    }

    [Test]
    public void ASleepOrASkipOfManyHoursFinishesTheJobOnce()
    {
        float hours = Stool.hours;
        int finished = 0;
        for (int hour = 0; hour < 24 * 7; hour++)
        {
            float before = hours;
            hours = WorkshopRules.AfterHour(before);
            if (WorkshopRules.FinishedThisHour(before, hours))
                finished++;
        }
        Assert.That(finished, Is.EqualTo(1));
    }

    [TestCase(3f, "3 h")]
    [TestCase(2.5f, "3 h")]
    [TestCase(1f, "1 h")]
    [TestCase(0.4f, "1 h")]
    [TestCase(0f, "0 h")]
    public void TheHoursLeftAreShownRoundedUp(float hours, string text)
    {
        Assert.That(WorkshopRules.HoursText(hours), Is.EqualTo(text));
    }

    // --- Where it may stand ---

    [Test]
    public void TheWorkshopKeepsItsOwnClearanceFromPilesCartsAndCabins()
    {
        Assert.That(WorkshopRules.PairClearance(false, false, 3.2f), Is.EqualTo(3f), "wider than the usual 2 m between piles");
        Assert.That(WorkshopRules.PairClearance(true, false, 3.2f), Is.EqualTo(2.6f), "from a parked cart");
        Assert.That(WorkshopRules.PairClearance(false, true, 3.2f), Is.EqualTo(3.2f), "the Cabin's own clearance");
        Assert.That(WorkshopRules.PairClearance(true, true, 3.2f), Is.EqualTo(3.2f), "a cabin pair wins over a cart pair");
    }

    [Test]
    public void TheCartsOwnSpacingExemptionCannotPutACartOnTheShed()
    {
        Assert.That(CartRules.SpacingMetres, Is.EqualTo(1.2f), "the cart's exemption");
        Assert.That(WorkshopRules.CartClearanceMetres, Is.GreaterThan(CartRules.SpacingMetres));
        // The shed is 3 m by 2.4 m: its half-width (1.5 m) plus the cart's half-length (about 0.9 m) is the least gap that keeps them apart.
        Assert.That(WorkshopRules.CartClearanceMetres, Is.GreaterThanOrEqualTo(1.5f + 0.9f));
        Assert.That(WorkshopRules.SpacingMetres, Is.GreaterThanOrEqualTo(1.5f + 1.2f), "and an ordinary pile stands outside the shed's half-width");
        Assert.That(WorkshopRules.TreeClearanceMetres, Is.GreaterThanOrEqualTo(1.5f), "no tree trunk inside the footprint");
    }

    [Test]
    public void NoSproutComesUpWithinAboutThreeMetresOfTheShed()
    {
        Assert.That(RegrowRules.WorkshopMetres, Is.EqualTo(2.9f));
        Assert.That(RegrowRules.WorkshopMetres, Is.GreaterThan(RegrowRules.StructureMetres), "wider than an ordinary structure");
        Assert.That(RegrowRules.Blocks(RegrowRules.Blocker.Workshop, 2.89f, 3.2f, 5.3f), Is.True);
        Assert.That(RegrowRules.Blocks(RegrowRules.Blocker.Workshop, 2.9f, 3.2f, 5.3f), Is.False);
        Assert.That(RegrowRules.Blocks(RegrowRules.Blocker.Structure, 2.89f, 3.2f, 5.3f), Is.False, "an ordinary structure would let it grow there");
        Assert.That(RegrowRules.BlockRadius(RegrowRules.Blocker.Workshop, 3.2f, 5.3f), Is.EqualTo(2.9f));
    }

    // --- The prompts ---

    [Test]
    public void ThePromptsReadAsPlannedAndCarryNoKeyNames()
    {
        Assert.That(WorkshopRules.OpenPrompt, Is.EqualTo("Choose what to make at the Workshop"));
        Assert.That(WorkshopRules.WorkingPrompt(Stool, 2f), Is.EqualTo("Workshop  (making a Rough Stool, 2 h to go)"));
        Assert.That(WorkshopRules.DonePrompt(Stool), Is.EqualTo("Collect Rough Stool  (ready in the Workshop)"));
        Assert.That(WorkshopRules.StorePrompt("Logs, Branches"), Is.EqualTo("Store Logs, Branches"));
        Assert.That(WorkshopRules.DeliverHelp, Is.EqualTo("Deliver Logs, Branches and Cordage  (the bench holds 6 Logs, 8 Branches and 4 Cordage)"));
        Assert.That(WorkshopRules.DismantlePrompt(true, 2f), Does.StartWith("Can't dismantle the Workshop while a piece is being made"));
        Assert.That(WorkshopRules.DismantlePrompt(false, 0f), Does.StartWith("Dismantle the Workshop  (half the materials back"));

        var prompts = new System.Collections.Generic.List<string>
        {
            WorkshopRules.OpenPrompt, WorkshopRules.StorePrompt("Logs"), WorkshopRules.DeliverHelp, WorkshopRules.BuiltMessage,
            WorkshopRules.DismantlePrompt(true, 2f), WorkshopRules.DismantlePrompt(false, 0f),
        };
        foreach (WorkshopRules.Recipe recipe in WorkshopRules.Recipes)
        {
            prompts.Add(WorkshopRules.WorkingPrompt(recipe, 2f));
            prompts.Add(WorkshopRules.DonePrompt(recipe));
        }
        foreach (string prompt in prompts)
        {
            Assert.That(prompt, Does.Not.Contain("["), prompt);
            Assert.That(Regex.IsMatch(prompt, @"\b[ER]\b"), Is.False, "no key name in: " + prompt);
        }
    }

    [Test]
    public void TheWordsAreSingularWhenOneIsMissing()
    {
        Assert.That(WorkshopRules.ShortText(Stool, 1, 1, 0), Is.EqualTo("1 Log, 1 Branch"));
        Assert.That(WorkshopRules.ShortText(Stool, 2, 2, 0), Is.EqualTo(""));
        Assert.That(WorkshopRules.NeedsText(new WorkshopRules.Recipe { logs = 1, branches = 1, cordage = 1 }), Is.EqualTo("1 Log, 1 Branch, 1 Cordage"));
        Assert.That(WorkshopRules.NeedsText(new WorkshopRules.Recipe { logs = 3, branches = 4, cordage = 2 }), Is.EqualTo("3 Logs, 4 Branches, 2 Cordage"));
    }

    [Test]
    public void EachGateHasItsOwnMessage()
    {
        Assert.That(WorkshopRules.GateMessage(WorkshopRules.Gate.Busy, Stool, 0, 0, 0, 2f), Is.EqualTo("The bench is busy: a Rough Stool is 2 h from done."));
        Assert.That(WorkshopRules.GateMessage(WorkshopRules.Gate.ProductWaiting, Stool, 0, 0, 0, 0f), Does.Contain("collect it first"));
        Assert.That(WorkshopRules.GateMessage(WorkshopRules.Gate.NoTool, Stool, 0, 0, 0, 0f), Is.EqualTo("You need an Axe, Primitive Axe or Wood Saw in your pack to work the bench."));
        Assert.That(WorkshopRules.GateMessage(WorkshopRules.Gate.ShortOfStock, Stool, 1, 2, 0, 0f), Is.EqualTo("The bench is short of 1 Log for a Rough Stool. Deliver more."));
        Assert.That(WorkshopRules.GateMessage(WorkshopRules.Gate.Ok, Stool, 2, 2, 0, 0f), Is.EqualTo(""));
        Assert.That(WorkshopRules.StartedMessage(Stool), Does.Contain("3 h"));
        Assert.That(WorkshopRules.BuiltMessage, Does.Contain("Workshop built"));
        Assert.That(WorkshopRules.BuiltMessage, Does.Not.Contain("Rough Stool"));
    }

    // --- The Rough Stool in the market ---

    [Test]
    public void TheStoolIsABoughtItemInTheCraftedGroupAtFourteenDollars()
    {
        Assert.That(Table.TryGetRow("rough_stool", out MarketRow row), Is.True);
        Assert.That(row.group, Is.EqualTo(MarketGroup.Crafted));
        Assert.That(row.kind, Is.EqualTo(MarketKind.Bought));
        Assert.That(row.pool, Is.EqualTo("rough_stool"));
        Assert.That(row.baseCents, Is.EqualTo(1400L));
        Assert.That(row.baseCents, Is.EqualTo(Stool.baseCents));
        Assert.That(Table.SellBaseCents("rough_stool"), Is.EqualTo(1400L));
        Assert.That(Table.BuyBaseCents("rough_stool"), Is.EqualTo(0L), "the store does not sell it");
        Assert.That(Table.Stock.Any(r => r.itemId == "rough_stool"), Is.False);
    }

    [Test]
    public void StoolsSellAtTheCraftedRatesTenPercentOffEachUnitWithAFloorOfOneTenth()
    {
        MarketGroupRule rule = Table.RuleOf("rough_stool");
        Assert.That(rule.dropBp, Is.EqualTo(1000));
        Assert.That(rule.floorBp, Is.EqualTo(1000));
        SaleResult three = MarketRules.SellStack(1400, MarketRules.One, 3, rule.dropBp, rule.floorBp);
        Assert.That(three.firstUnitCents, Is.EqualTo(1400L));
        Assert.That(three.lastUnitCents, Is.EqualTo(1134L));
        Assert.That(three.totalCents, Is.EqualTo(1400L + 1260L + 1134L));
    }

    [Test]
    public void OneStoolADaySettlesAtAboutElevenDollars()
    {
        MarketGroupRule rule = Table.RuleOf("rough_stool");
        int m = MarketRules.One;
        long total = 0;
        int days = 0;
        for (int day = 0; day < 240; day++)
        {
            SaleResult sale = MarketRules.SellStack(1400, m, 1, rule.dropBp, rule.floorBp);
            m = MarketRules.Recover(sale.newMultiplierBp, MarketRules.One, 25, rule.floorBp);
            if (day >= 120)
            {
                total += sale.totalCents;
                days++;
            }
        }
        Assert.That(total / (double)days / 100.0, Is.EqualTo(10.77).Within(0.05), "$10.77 a game day at steady state");
    }

    [Test]
    public void FurnitureSellsUnderCraftedGoodsAndNoLedgerCategoryWasAdded()
    {
        Assert.That(LedgerCategories.ForSale(MarketGroup.Crafted), Is.EqualTo(LedgerCategory.SalesCrafted));
        Assert.That(LedgerCategories.Name(LedgerCategory.SalesCrafted), Is.EqualTo("Crafted goods"));
        Assert.That(LedgerCategories.Count, Is.EqualTo(19));
        Assert.That(LedgerCategories.LastIncomeId, Is.EqualTo(7));
        Assert.That(Enum.GetValues(typeof(LedgerCategory)).Length, Is.EqualTo(19));
        Assert.That(LedgerSaveData.CurrentVersion, Is.EqualTo(1));
    }

    [Test]
    public void TheRoughStoolItemIsATenKilogramMaterialCalledRoughStool()
    {
        object item = Find("ItemDatabase").GetMethod("Get").Invoke(null, new object[] { "rough_stool" });
        Assert.That(item, Is.Not.Null, "Resources/Items/Materials/rough_stool.asset");
        Type def = item.GetType();
        Assert.That((string)def.GetProperty("DisplayName").GetValue(item), Is.EqualTo("Rough Stool"));
        Assert.That(def.GetProperty("Category").GetValue(item).ToString(), Is.EqualTo("Material"));
        Assert.That((float)def.GetProperty("WeightKg").GetValue(item), Is.EqualTo(Stool.weightKg));
        Assert.That((bool)def.GetProperty("IsPerishable").GetValue(item), Is.False);
    }

    [Test]
    public void AStoolIsLightEnoughToCarryUnslowedAndFourFitThePack()
    {
        // The pack holds 45 kg and is slowed from 30 kg: a stool (10 kg) is nothing, and four stools fit.
        Assert.That(Stool.weightKg * 4f, Is.LessThanOrEqualTo(45f));
        Assert.That(Stool.weightKg, Is.LessThan(30f));
    }

    // --- The pile kinds and the staged site's rows ---

    [Test]
    public void TheWorkshopKindsAreAppendedAsThirtyThreeAndThirtyFour()
    {
        Assert.That((int)Kind("WorkshopSite"), Is.EqualTo(33));
        Assert.That((int)Kind("Workshop"), Is.EqualTo(34));
        Assert.That((int)Kind("HandCart"), Is.EqualTo(32));
        Assert.That((int)Kind("HandCartSite"), Is.EqualTo(31));
        Assert.That((int)Kind("LeachHopper"), Is.EqualTo(30));
        Assert.That((int)Kind("Felled"), Is.EqualTo(0));
        Assert.That(Enum.GetValues(Find("PileKind")).Length, Is.EqualTo(37), "nothing was inserted in the middle");
    }

    [Test]
    public void TheSiteIsStagedBecomesAWorkshopAndCostsWhatTheRulesSay()
    {
        Assert.That(Static("WellsAndOuthouses", "IsStagedSite", Kind("WorkshopSite")), Is.EqualTo(true));
        Assert.That(Static("WellsAndOuthouses", "IsStagedSite", Kind("Workshop")), Is.EqualTo(false));
        Assert.That(Static("WellsAndOuthouses", "FinishedKind", Kind("WorkshopSite")), Is.EqualTo(Kind("Workshop")));
        Assert.That(Static("WellsAndOuthouses", "FinalKind", Kind("WorkshopSite")), Is.EqualTo(Kind("Workshop")));
        Assert.That(Static("WoodManager", "PileName", Kind("WorkshopSite")), Is.EqualTo("Workshop Site"));
        Assert.That(Static("WoodManager", "PileName", Kind("Workshop")), Is.EqualTo("Workshop"));
        Assert.That(Static("WoodManager", "IsWorkshop", Kind("WorkshopSite")), Is.EqualTo(true));
        Assert.That(Static("WoodManager", "IsWorkshop", Kind("Workshop")), Is.EqualTo(true));
        Assert.That(Static("WoodManager", "IsWorkshop", Kind("HandCart")), Is.EqualTo(false));
        Assert.That(((IList)Find("WoodManager").GetField("Buildable").GetValue(null)).Contains(Kind("WorkshopSite")), Is.True, "on the Build tab");
        Assert.That(((IList)Find("WoodManager").GetField("Buildable").GetValue(null)).Contains(Kind("Workshop")), Is.False, "only the site is built");

        object[] cost = ((Array)Static("WellsAndOuthouses", "WorkshopCost")).Cast<object>().ToArray();
        Type stack = Find("WoodStack");
        string[] have = cost.Select(c => (string)stack.GetField("itemId").GetValue(c) + ":" + (int)stack.GetField("count").GetValue(c)).ToArray();
        Assert.That(have, Is.EquivalentTo(new[] { "logs:5", "branches:10", "tall_grass:10", "cordage:3" }));
        object[] siteCost = ((Array)Static("WellsAndOuthouses", "FinishedCost", Kind("WorkshopSite"))).Cast<object>().ToArray();
        Assert.That(siteCost.Length, Is.EqualTo(4));
        Assert.That(Static("WellsAndOuthouses", "NeedOf", Kind("WorkshopSite"), "logs"), Is.EqualTo(5));
        Assert.That(Static("WellsAndOuthouses", "NeedOf", Kind("WorkshopSite"), "tall_grass"), Is.EqualTo(10));
        Assert.That(Static("WellsAndOuthouses", "NeedOf", Kind("WorkshopSite"), "firewood"), Is.EqualTo(0));
        Assert.That(Static("WellsAndOuthouses", "RequiresDig", Kind("WorkshopSite")), Is.EqualTo(false), "no dig stage");
    }

    [Test]
    public void TheSiteTakesItsFourMaterialsInAnyOrderAndStopsAtTheCost()
    {
        object order = Static("WellsAndOuthouses", "DeliveryOrder", Kind("WorkshopSite"));
        string[][] groups = ((Array)order).Cast<string[]>().ToArray();
        Assert.That(groups.Length, Is.EqualTo(1));
        Assert.That(groups[0], Is.EquivalentTo(new[] { "logs", "branches", "tall_grass", "cordage" }));

        object site = NewPile("WorkshopSite");
        Type pile = site.GetType();
        MethodInfo accepts = pile.GetMethod("Accepts");
        foreach (string id in new[] { "logs", "branches", "tall_grass", "cordage" })
            Assert.That(accepts.Invoke(site, new object[] { id }), Is.EqualTo(true), id);
        Assert.That(accepts.Invoke(site, new object[] { "firewood" }), Is.EqualTo(false));
        Add(site, "logs", 5);
        Assert.That(accepts.Invoke(site, new object[] { "logs" }), Is.EqualTo(false), "full of Logs");
        Assert.That(accepts.Invoke(site, new object[] { "branches" }), Is.EqualTo(true));
        Assert.That(pile.GetProperty("IsPlacedStructure").GetValue(site), Is.EqualTo(true));
        Assert.That(pile.GetProperty("IsPlacedStructure").GetValue(NewPile("Workshop")), Is.EqualTo(true));
    }

    [Test]
    public void TheFinishedBenchTakesOnlyItsStockUpToTheCaps()
    {
        object bench = NewPile("Workshop");
        MethodInfo accepts = bench.GetType().GetMethod("Accepts");
        foreach (string id in new[] { "logs", "branches", "cordage" })
            Assert.That(accepts.Invoke(bench, new object[] { id }), Is.EqualTo(true), id);
        foreach (string id in new[] { "tall_grass", "firewood", "rough_stool", "stone", "water_good" })
            Assert.That(accepts.Invoke(bench, new object[] { id }), Is.EqualTo(false), id);
        Add(bench, "logs", 5);
        Assert.That(accepts.Invoke(bench, new object[] { "logs" }), Is.EqualTo(true), "one more fits");
        Add(bench, "logs", 1);
        Assert.That(accepts.Invoke(bench, new object[] { "logs" }), Is.EqualTo(false), "six is the cap");
        Add(bench, "branches", 8);
        Assert.That(accepts.Invoke(bench, new object[] { "branches" }), Is.EqualTo(false));
    }

    // --- The saved pile ---

    [Test]
    public void TwoFieldsWereAppendedToTheSavedPileAndNothingElse()
    {
        string[] fields = Find("WoodPileState").GetFields(BindingFlags.Public | BindingFlags.Instance).Select(f => f.Name).OrderBy(n => n).ToArray();
        Assert.That(fields, Is.EqualTo(new[]
        {
            "buildProgress", "builtDay", "contaminatedHours", "contents", "depthToWater", "digProgress", "id", "jobCount", "jobHours", "jobItem", "kind", "leachHours", "litres", "position", "yaw",
        }));
        Assert.That(Find("WoodPileState").GetField("jobItem").FieldType, Is.EqualTo(typeof(string)));
        Assert.That(Find("WoodPileState").GetField("jobHours").FieldType, Is.EqualTo(typeof(float)));
    }

    [Test]
    public void AnOldPileLoadsWithAnIdleBenchAndTheSaveVersionStaysOne()
    {
        Type pile = Find("WoodPileState");
        string old = "{\"id\":4,\"kind\":15,\"position\":{\"x\":1.0,\"y\":2.0,\"z\":3.0},\"yaw\":10.0,\"contents\":[],\"litres\":0,\"contaminatedHours\":0.0,\"digProgress\":0.0,\"builtDay\":0,\"buildProgress\":0.0,\"depthToWater\":0.0,\"leachHours\":0.0}";
        object loaded = JsonUtility.FromJson(old, pile);
        Assert.That(string.IsNullOrEmpty((string)pile.GetField("jobItem").GetValue(loaded)), Is.True, "no job");
        Assert.That((float)pile.GetField("jobHours").GetValue(loaded), Is.EqualTo(0f));
        Assert.That((int)pile.GetField("id").GetValue(loaded), Is.EqualTo(4));

        FieldInfo version = Find("SaveManager").GetField("SaveVersion", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        Assert.That(version.GetRawConstantValue(), Is.EqualTo(1), "no save version bump");
    }

    [Test]
    public void AJobInProgressAndItsStockSurviveASaveAndLoad()
    {
        Type pile = Find("WoodPileState");
        object bench = NewPile("Workshop");
        pile.GetField("jobItem").SetValue(bench, "rough_stool");
        pile.GetField("jobHours").SetValue(bench, 2f);
        Add(bench, "logs", 3);
        Add(bench, "branches", 5);
        object back = JsonUtility.FromJson(JsonUtility.ToJson(bench), pile);
        Assert.That((string)pile.GetField("jobItem").GetValue(back), Is.EqualTo("rough_stool"));
        Assert.That((float)pile.GetField("jobHours").GetValue(back), Is.EqualTo(2f));
        Assert.That(pile.GetField("kind").GetValue(back), Is.EqualTo(Kind("Workshop")));
        Assert.That(Count(back, "logs"), Is.EqualTo(3));
        Assert.That(Count(back, "branches"), Is.EqualTo(5));
    }

    [Test]
    public void FinishingAJobPutsTheStoolInTheBenchAndFreesIt()
    {
        Type pile = Find("WoodPileState");
        object bench = NewPile("Workshop");
        pile.GetField("jobItem").SetValue(bench, "rough_stool");
        pile.GetField("jobHours").SetValue(bench, 0f);
        Add(bench, "logs", 4);
        Find("WellsAndOuthouses").GetMethod("FinishJob", BindingFlags.Public | BindingFlags.Static).Invoke(null, new[] { bench });
        Assert.That(Count(bench, "rough_stool"), Is.EqualTo(1));
        Assert.That(string.IsNullOrEmpty((string)pile.GetField("jobItem").GetValue(bench)), Is.True);
        Assert.That((float)pile.GetField("jobHours").GetValue(bench), Is.EqualTo(0f));
        Assert.That(Count(bench, "logs"), Is.EqualTo(4), "the stock is untouched");

        // An idle bench finishes nothing.
        Find("WellsAndOuthouses").GetMethod("FinishJob", BindingFlags.Public | BindingFlags.Static).Invoke(null, new[] { bench });
        Assert.That(Count(bench, "rough_stool"), Is.EqualTo(1), "no second stool");
    }

    [Test]
    public void ThePileKindNumbersLedgerVersionAndFelledEntriesAreUntouched()
    {
        string[] names = Enum.GetNames(Find("PileKind"));
        Assert.That(names.Take(33), Is.EqualTo(new[]
        {
            "Felled", "WoodStorage", "RockStorage", "WaterBarrel", "FoodCache", "StorageBin", "Tent", "LeanTo", "Cabin", "CabinSite", "ToolRack", "TarpShelter", "RainBarrel",
            "WellSite", "Well", "Outhouse", "OuthouseSite", "WaterBarrelSite", "RainBarrelSite", "FoodCacheSite", "WoodShedSite", "WoodShed", "KitchenSite", "KitchenRoofSite",
            "Kitchen", "Countertop", "Shelving", "Grill", "SlopSink", "LeachHopperSite", "LeachHopper", "HandCartSite", "HandCart",
        }), "every existing kind keeps its number");
        string[] felled = Find("FelledTree").GetFields(BindingFlags.Public | BindingFlags.Instance).Select(f => f.Name).OrderBy(n => n).ToArray();
        Assert.That(felled, Is.EqualTo(new[] { "fellStamp", "index", "stump" }), "the tree regrowth field is the only one added there");
    }

    [Test]
    public void TheHooksOtherCodeUsesAreThere()
    {
        Type wells = Find("WellsAndOuthouses");
        Assert.That(wells.GetMethod("DebugFinishJobs")?.ReturnType, Is.EqualTo(typeof(int)));
        Assert.That(wells.GetMethod("FinishJob", BindingFlags.Public | BindingFlags.Static), Is.Not.Null);
        Assert.That(wells.GetMethod("WorkshopCost", BindingFlags.Public | BindingFlags.Static), Is.Not.Null);
    }

    // --- Phase a2-1: the End Table, Chair and Table, and the Workshop screen's words ---

    static WorkshopRules.Recipe ByName(string name) => WorkshopRules.Recipes.First(r => r.name == name);

    [TestCase("Rough Stool", "rough_stool", 2, 2, 0, 3, 1400L, 10f)]
    [TestCase("End Table", "end_table", 1, 3, 0, 2, 1200L, 8f)]
    [TestCase("Chair", "chair", 2, 4, 1, 4, 1800L, 12f)]
    [TestCase("Table", "table", 3, 4, 2, 5, 2800L, 24f)]
    public void EachPieceHasItsPlannedNumbers(string name, string id, int logs, int branches, int cordage, int hours, long cents, float kg)
    {
        WorkshopRules.Recipe recipe = ByName(name);
        Assert.That(recipe.itemId, Is.EqualTo(id));
        Assert.That((recipe.logs, recipe.branches, recipe.cordage), Is.EqualTo((logs, branches, cordage)));
        Assert.That(recipe.hours, Is.EqualTo(hours));
        Assert.That(recipe.baseCents, Is.EqualTo(cents));
        Assert.That(recipe.weightKg, Is.EqualTo(kg));
    }

    [Test]
    public void TheListIsTheFourPiecesInTheAgreedOrder()
    {
        Assert.That(WorkshopRules.Recipes.Select(r => r.name), Is.EqualTo(new[] { "Rough Stool", "End Table", "Chair", "Table" }));
        Assert.That(WorkshopRules.IsProduct("table"), Is.True);
        Assert.That(WorkshopRules.IsProduct("chair"), Is.True);
        Assert.That(WorkshopRules.IsProduct("end_table"), Is.True);
        Assert.That(WorkshopRules.IsProduct("logs"), Is.False);
    }

    [Test]
    public void TheCapsStillHoldATableAndAChairTogetherAndTwoTables()
    {
        WorkshopRules.Recipe table = ByName("Table"), chair = ByName("Chair");
        Assert.That(table.logs + chair.logs, Is.LessThanOrEqualTo(WorkshopRules.LogsCap));
        Assert.That(table.branches + chair.branches, Is.LessThanOrEqualTo(WorkshopRules.BranchesCap));
        Assert.That(table.cordage + chair.cordage, Is.LessThanOrEqualTo(WorkshopRules.CordageCap));
        Assert.That(table.cordage * 2, Is.LessThanOrEqualTo(WorkshopRules.CordageCap));
    }

    [TestCase("Rough Stool")]
    [TestCase("End Table")]
    [TestCase("Chair")]
    [TestCase("Table")]
    public void EachPieceStartsOnExactlyItsStockAndNotOneShort(string name)
    {
        WorkshopRules.Recipe r = ByName(name);
        Assert.That(WorkshopRules.CanStart(r, true, false, false, r.logs, r.branches, r.cordage), Is.EqualTo(WorkshopRules.Gate.Ok));
        Assert.That(WorkshopRules.CanStart(r, true, false, false, r.logs - 1, r.branches, r.cordage), Is.EqualTo(WorkshopRules.Gate.ShortOfStock));
        Assert.That(WorkshopRules.CanStart(r, true, false, false, r.logs, r.branches - 1, r.cordage), Is.EqualTo(WorkshopRules.Gate.ShortOfStock));
        if (r.cordage > 0)
            Assert.That(WorkshopRules.CanStart(r, true, false, false, r.logs, r.branches, r.cordage - 1), Is.EqualTo(WorkshopRules.Gate.ShortOfStock));
    }

    [Test]
    public void TheNeedRowsReadInPlainWords()
    {
        WorkshopRules.NeedRow[] rows = WorkshopRules.NeedRows(ByName("Chair"), 4, 3, 0);
        Assert.That(rows.Select(r => r.Text), Is.EqualTo(new[]
        {
            "Logs - need 2 - bench has 4 - enough",
            "Branches - need 4 - bench has 3 - short by 1",
            "Cordage - need 1 - bench has 0 - short by 1",
        }));
        Assert.That(rows.Select(r => r.enough), Is.EqualTo(new[] { true, false, false }));
        Assert.That(WorkshopRules.NeedRows(ByName("Rough Stool"), 2, 2, 5), Has.Length.EqualTo(2), "no Cordage row for a piece that uses none");
        Assert.That(WorkshopRules.NeedRows(ByName("Table"), 3, 4, 2).All(r => r.enough), Is.True);
        Assert.That(WorkshopRules.NeedRows(ByName("Table"), 0, 0, 0)[0].Text, Is.EqualTo("Logs - need 3 - bench has 0 - short by 3"));
    }

    [Test]
    public void TheBenchLineSaysIdleWorkingAndReady()
    {
        WorkshopRules.Recipe chair = ByName("Chair"), table = ByName("Table"), none = default;
        Assert.That(WorkshopRules.BenchLine(false, none, 0f, false, none, 4, 5, 1), Is.EqualTo("Bench is idle - it holds 4 Logs, 5 Branches, 1 Cordage"));
        Assert.That(WorkshopRules.BenchLine(false, none, 0f, false, none, 1, 1, 0), Is.EqualTo("Bench is idle - it holds 1 Log, 1 Branch, 0 Cordage"));
        Assert.That(WorkshopRules.BenchLine(true, chair, 2f, false, none, 0, 0, 0), Is.EqualTo("Making a Chair, 2 h to go"));
        Assert.That(WorkshopRules.BenchLine(true, chair, 1.5f, false, none, 0, 0, 0), Is.EqualTo("Making a Chair, 2 h to go"), "part hours round up");
        Assert.That(WorkshopRules.BenchLine(false, none, 0f, true, table, 0, 0, 0), Is.EqualTo("A Table is ready - collect it"));
    }

    [Test]
    public void TheListRowsShowTheCurrentPriceOrTheBaseWhenThereIsNone()
    {
        Assert.That(WorkshopRules.ListLabel(ByName("Rough Stool"), 1077), Is.EqualTo("Rough Stool\n3 h  -  $10.77"));
        Assert.That(WorkshopRules.ListLabel(ByName("Table"), 0), Is.EqualTo("Table\n5 h  -  $28.00"));
        Assert.That(WorkshopRules.DetailLine(ByName("Table")), Is.EqualTo("5 h  -  24 kg  -  base price $28.00"));
        Assert.That(WorkshopRules.DetailLine(ByName("Rough Stool")), Is.EqualTo("3 h  -  10 kg  -  base price $14.00"));
    }

    [Test]
    public void TheStartButtonAndTheReasonLineFollowTheGate()
    {
        WorkshopRules.Recipe chair = ByName("Chair"), table = ByName("Table");
        Assert.That(WorkshopRules.StartButtonText(WorkshopRules.Gate.Ok, chair), Is.EqualTo("START CHAIR"));
        Assert.That(WorkshopRules.StartButtonText(WorkshopRules.Gate.Busy, chair), Is.EqualTo("BENCH IS BUSY"));
        Assert.That(WorkshopRules.StartButtonText(WorkshopRules.Gate.NoTool, chair), Is.EqualTo("NEED A TOOL"));
        Assert.That(WorkshopRules.StartButtonText(WorkshopRules.Gate.ShortOfStock, chair), Is.EqualTo("SHORT OF MATERIALS"));
        Assert.That(WorkshopRules.StartButtonText(WorkshopRules.Gate.ProductWaiting, chair), Is.EqualTo("COLLECT THE PIECE FIRST"));

        Assert.That(WorkshopRules.ReasonText(WorkshopRules.Gate.Ok, chair, default, 9, 9, 9, 0f), Is.EqualTo(""));
        Assert.That(WorkshopRules.ReasonText(WorkshopRules.Gate.Busy, chair, table, 0, 0, 0, 3f), Is.EqualTo("A Table is 3 h from done."), "busy names the piece being made, not the one chosen");
        Assert.That(WorkshopRules.ReasonText(WorkshopRules.Gate.NoTool, chair, default, 9, 9, 9, 0f), Is.EqualTo("Carry an Axe, Primitive Axe or Wood Saw to work the bench."));
        Assert.That(WorkshopRules.ReasonText(WorkshopRules.Gate.ShortOfStock, table, default, 1, 4, 0, 0f), Is.EqualTo("Still short 2 Logs, 2 Cordage. Deliver more."));
        Assert.That(WorkshopRules.ReasonText(WorkshopRules.Gate.ProductWaiting, chair, default, 9, 9, 9, 0f), Does.Contain("collect it first"));
    }

    [Test]
    public void TheScreenWordsCarryNoKeyNames()
    {
        foreach (WorkshopRules.Recipe recipe in WorkshopRules.Recipes)
        {
            var words = new System.Collections.Generic.List<string> { WorkshopRules.DetailLine(recipe), WorkshopRules.ListLabel(recipe, 0), WorkshopRules.StartedMessage(recipe) };
            foreach (WorkshopRules.Gate gate in Enum.GetValues(typeof(WorkshopRules.Gate)))
            {
                words.Add(WorkshopRules.StartButtonText(gate, recipe));
                words.Add(WorkshopRules.ReasonText(gate, recipe, recipe, 0, 0, 0, 2f));
            }
            foreach (WorkshopRules.NeedRow row in WorkshopRules.NeedRows(recipe, 0, 0, 0))
                words.Add(row.Text);
            foreach (string text in words)
                Assert.That(Regex.IsMatch(text, @"\b[ER]\b") || text.Contains("["), Is.False, text);
        }
    }

    [TestCase("end_table", 1200L, 9.23)]
    [TestCase("chair", 1800L, 13.85)]
    [TestCase("table", 2800L, 21.55)]
    public void ANewPieceIsABoughtCraftedItemThatSettlesAtTheDocumentedDailyTake(string id, long cents, double perDay)
    {
        Assert.That(Table.TryGetRow(id, out MarketRow row), Is.True);
        Assert.That(row.group, Is.EqualTo(MarketGroup.Crafted));
        Assert.That(row.kind, Is.EqualTo(MarketKind.Bought));
        Assert.That(row.pool, Is.EqualTo(id));
        Assert.That(row.baseCents, Is.EqualTo(cents));
        Assert.That(Table.BuyBaseCents(id), Is.EqualTo(0L), "the store does not sell it");

        MarketGroupRule rule = Table.RuleOf(id);
        int m = MarketRules.One;
        long total = 0;
        int days = 0;
        for (int day = 0; day < 240; day++)
        {
            SaleResult sale = MarketRules.SellStack(cents, m, 1, rule.dropBp, rule.floorBp);
            m = MarketRules.Recover(sale.newMultiplierBp, MarketRules.One, 25, rule.floorBp);
            if (day >= 120)
            {
                total += sale.totalCents;
                days++;
            }
        }
        Assert.That(total / (double)days / 100.0, Is.EqualTo(perDay).Within(0.06), "a game day at steady state, one a day");
    }

    [Test]
    public void EveryPieceHasAnItemAndAMarketRowAndALedgerBucket()
    {
        foreach (WorkshopRules.Recipe recipe in WorkshopRules.Recipes)
        {
            object item = Find("ItemDatabase").GetMethod("Get").Invoke(null, new object[] { recipe.itemId });
            Assert.That(item, Is.Not.Null, recipe.name + " needs Resources/Items/Materials/" + recipe.itemId + ".asset");
            Type def = item.GetType();
            Assert.That((string)def.GetProperty("DisplayName").GetValue(item), Is.EqualTo(recipe.name));
            Assert.That(def.GetProperty("Category").GetValue(item).ToString(), Is.EqualTo("Material"));
            Assert.That((float)def.GetProperty("WeightKg").GetValue(item), Is.EqualTo(recipe.weightKg));
            Assert.That((bool)def.GetProperty("IsPerishable").GetValue(item), Is.False);
            Assert.That(Table.TryGetRow(recipe.itemId, out MarketRow row), Is.True, recipe.name);
            Assert.That(row.baseCents, Is.EqualTo(recipe.baseCents), recipe.name);
            Assert.That(LedgerCategories.ForSale(row.group), Is.EqualTo(LedgerCategory.SalesCrafted), recipe.name);
        }
    }

    [Test]
    public void EveryFinishedPieceAppearsInTheBenchWhenItsJobEnds()
    {
        Type pile = Find("WoodPileState");
        foreach (WorkshopRules.Recipe recipe in WorkshopRules.Recipes)
        {
            object bench = NewPile("Workshop");
            pile.GetField("jobItem").SetValue(bench, recipe.itemId);
            pile.GetField("jobHours").SetValue(bench, 0f);
            Find("WellsAndOuthouses").GetMethod("FinishJob", BindingFlags.Public | BindingFlags.Static).Invoke(null, new[] { bench });
            Assert.That(Count(bench, recipe.itemId), Is.EqualTo(1), recipe.name);
            Assert.That(string.IsNullOrEmpty((string)pile.GetField("jobItem").GetValue(bench)), Is.True, recipe.name);
        }
    }

    [Test]
    public void AJobWithAnUnknownPieceFinishesSafelyAndLeavesNothingStuckInTheBench()
    {
        Type pile = Find("WoodPileState");
        object bench = NewPile("Workshop");
        pile.GetField("jobItem").SetValue(bench, "mystery_chest");
        pile.GetField("jobHours").SetValue(bench, 0f);
        Add(bench, "logs", 2);
        Assert.DoesNotThrow(() => Find("WellsAndOuthouses").GetMethod("FinishJob", BindingFlags.Public | BindingFlags.Static).Invoke(null, new[] { bench }));
        Assert.That(string.IsNullOrEmpty((string)pile.GetField("jobItem").GetValue(bench)), Is.True, "the bench is free again");
        Assert.That((float)pile.GetField("jobHours").GetValue(bench), Is.EqualTo(0f));
        Assert.That(Count(bench, "mystery_chest"), Is.EqualTo(0), "nothing that could never be collected");
        Assert.That(Count(bench, "logs"), Is.EqualTo(2), "the stock is untouched");
    }

    [Test]
    public void ThePlacedStructureStartsJobsThroughOneMethodTheScreenCalls()
    {
        MethodInfo start = Find("PlacedStructure").GetMethod("TryStartJob", BindingFlags.Public | BindingFlags.Instance);
        Assert.That(start, Is.Not.Null);
        Assert.That(start.ReturnType, Is.EqualTo(typeof(bool)));
        Assert.That(start.GetParameters().Select(p => p.ParameterType.Name), Is.EqualTo(new[] { "Recipe", "String&" }));
    }

    [Test]
    public void TheSavedPileStateGainedNoFieldAndNoVersionMoved()
    {
        string[] names = Find("WoodPileState").GetFields(BindingFlags.Public | BindingFlags.Instance).Select(f => f.Name).ToArray();
        Assert.That(names, Does.Contain("jobItem"));
        Assert.That(names, Does.Contain("jobHours"));
        Assert.That(names.Length, Is.EqualTo(15), "a2-1 added no saved field; Pottery P1 added jobCount");
        Assert.That(LedgerSaveData.CurrentVersion, Is.EqualTo(1));
        Assert.That(Enum.GetValues(Find("PileKind")).Length, Is.EqualTo(37), "only the two Potter kinds were added");
    }

    // --- The Workshop screen's text ---

    [Test]
    public void TheScreenBuildsWithNothingUnderTwentySixAndTheAgreedSizes()
    {
        Type screen = Find("WorkshopScreen");
        Type text = Type.GetType("UnityEngine.UI.Text, UnityEngine.UI");
        Assert.That(text, Is.Not.Null);
        var go = new GameObject("Workshop Screen Test");
        try
        {
            Component instance = go.AddComponent(screen);
            screen.GetMethod("Build", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(instance, null);
            PropertyInfo size = text.GetProperty("fontSize"), style = text.GetProperty("fontStyle");
            Component[] labels = go.GetComponentsInChildren(text, true);
            Assert.That(labels.Length, Is.GreaterThan(15));
            foreach (Component label in labels)
                Assert.That((int)size.GetValue(label), Is.GreaterThanOrEqualTo(26), label.name);

            int SizeOf(string name) => (int)size.GetValue(labels.First(l => l.name == name));
            Assert.That(SizeOf("Title"), Is.EqualTo(36));
            Assert.That(SizeOf("Status"), Is.EqualTo(28));
            Assert.That(SizeOf("Piece"), Is.EqualTo(40));
            Assert.That(SizeOf("Piece Line"), Is.EqualTo(28));
            Assert.That(SizeOf("Needs Heading"), Is.EqualTo(28));
            Assert.That(SizeOf("Need 0"), Is.EqualTo(28));
            Assert.That(SizeOf("Reason"), Is.EqualTo(28));
            Assert.That(SizeOf("Hint"), Is.EqualTo(28), "the key line, one line along the bottom");
            Assert.That(SizeOf("Pack"), Is.EqualTo(28), "what the pack carries");
            Assert.That(SizeOf("Stock Reason"), Is.EqualTo(28));
            Component[] stocks = labels.Where(l => l.transform.parent != null && l.transform.parent.name == "Stock").ToArray();
            Assert.That(stocks.Length, Is.EqualTo(1));
            Assert.That((int)size.GetValue(stocks[0]), Is.EqualTo(32), "the stock button");
            Assert.That((int)style.GetValue(stocks[0]), Is.EqualTo((int)FontStyle.Bold));
            Assert.That(ScreenConstant("StockButtonHeight"), Is.GreaterThanOrEqualTo(80f));
            Assert.That(labels.Where(l => l.name == "Label" && l.transform.parent.name.StartsWith("Row")).Select(l => (int)size.GetValue(l)), Is.All.EqualTo(32), "the list names");
            Assert.That(labels.Where(l => l.name == "Label" && l.transform.parent.name.StartsWith("Row")).All(l => (int)style.GetValue(l) == (int)FontStyle.Bold), Is.True);
            Assert.That(labels.Where(l => l.name == "Detail").Select(l => (int)size.GetValue(l)), Is.All.EqualTo(28), "the hours and price");
            Component[] starts = labels.Where(l => l.transform.parent != null && l.transform.parent.name == "Start").ToArray();
            Assert.That(starts.Length, Is.EqualTo(1));
            Assert.That((int)size.GetValue(starts[0]), Is.EqualTo(36), "the Start button");
            Assert.That((int)style.GetValue(starts[0]), Is.EqualTo((int)FontStyle.Bold));
            Component[] closes = labels.Where(l => l.transform.parent != null && l.transform.parent.name == "Close").ToArray();
            Assert.That(closes.Length, Is.EqualTo(1));
            Assert.That((int)size.GetValue(closes[0]), Is.EqualTo(28), "the Close button");
            Assert.That(ScreenConstant("MinimumSize"), Is.EqualTo(26f));
            Assert.That(ScreenConstant("RowHeight"), Is.GreaterThanOrEqualTo(96f));
            Assert.That(ScreenConstant("RowGap"), Is.GreaterThanOrEqualTo(12f));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(go);
        }
    }

    static float ScreenConstant(string constant) => Convert.ToSingle(Find("WorkshopScreen").GetField(constant, BindingFlags.Public | BindingFlags.Static).GetValue(null));

    [Test]
    public void TheScreenIsClosedUntilOpened()
    {
        Assert.That((bool)Find("WorkshopScreen").GetProperty("IsOpen", BindingFlags.Public | BindingFlags.Static).GetValue(null), Is.False);
    }

    // --- Phase a2-2: the pack line, the stock button and the keys ---

    [Test]
    public void ThePackLineNamesWhatTheBenchUsesAndNothingElse()
    {
        Assert.That(WorkshopRules.PackLine(4, 6, 1), Is.EqualTo("You carry: 4 Logs, 6 Branches, 1 Cordage"));
        Assert.That(WorkshopRules.PackLine(1, 1, 1), Is.EqualTo("You carry: 1 Log, 1 Branch, 1 Cordage"));
        Assert.That(WorkshopRules.PackLine(0, 0, 3), Is.EqualTo("You carry: 3 Cordage"));
        Assert.That(WorkshopRules.PackLine(2, 0, 0), Is.EqualTo("You carry: 2 Logs"));
        Assert.That(WorkshopRules.PackLine(0, 5, 2), Is.EqualTo("You carry: 5 Branches, 2 Cordage"));
        Assert.That(WorkshopRules.PackLine(0, 0, 0), Is.EqualTo("You carry: nothing the bench uses"));
    }

    [Test]
    public void TheStockButtonIsOnOnlyWhenThePackHasSomethingTheBenchHasRoomFor()
    {
        // (carried logs, branches, cordage; bench logs, branches, cordage)
        Assert.That(WorkshopRules.StockState(0, 0, 0, 0, 0, 0), Is.EqualTo(WorkshopRules.StockGate.NothingCarried));
        Assert.That(WorkshopRules.StockState(0, 0, 0, 6, 8, 4), Is.EqualTo(WorkshopRules.StockGate.NothingCarried), "a full bench does not matter when nothing is carried");
        Assert.That(WorkshopRules.StockState(3, 0, 0, 0, 0, 0), Is.EqualTo(WorkshopRules.StockGate.Can));
        Assert.That(WorkshopRules.StockState(3, 0, 0, 5, 0, 0), Is.EqualTo(WorkshopRules.StockGate.Can), "one more Log fits under the cap of 6");
        Assert.That(WorkshopRules.StockState(3, 0, 0, 6, 0, 0), Is.EqualTo(WorkshopRules.StockGate.BenchFull), "Logs are at the cap");
        Assert.That(WorkshopRules.StockState(3, 2, 0, 6, 8, 0), Is.EqualTo(WorkshopRules.StockGate.BenchFull), "both carried kinds are at their caps");
        Assert.That(WorkshopRules.StockState(3, 2, 0, 6, 7, 0), Is.EqualTo(WorkshopRules.StockGate.Can), "Branches still have room");
        Assert.That(WorkshopRules.StockState(0, 0, 1, 6, 8, 3), Is.EqualTo(WorkshopRules.StockGate.Can), "Cordage under its cap of 4");
        Assert.That(WorkshopRules.StockState(0, 0, 1, 6, 8, 4), Is.EqualTo(WorkshopRules.StockGate.BenchFull));
        Assert.That(WorkshopRules.StockState(1, 1, 1, 6, 8, 4), Is.EqualTo(WorkshopRules.StockGate.BenchFull));
    }

    [Test]
    public void TheStockButtonWorksWhileAJobRunsBecauseTheGateDoesNotLookAtTheJob()
    {
        // The stock rule takes no job at all: R has always stocked a bench that is making something, and the button follows it.
        MethodInfo state = typeof(WorkshopRules).GetMethod("StockState");
        Assert.That(state.GetParameters().Select(p => p.Name), Is.EqualTo(new[] { "carriedLogs", "carriedBranches", "carriedCordage", "benchLogs", "benchBranches", "benchCordage" }));
        Assert.That(WorkshopRules.StockState(2, 4, 1, 1, 1, 0), Is.EqualTo(WorkshopRules.StockGate.Can));
    }

    [Test]
    public void TheStockReasonsReadInAFewWordsAndNeverNameAKey()
    {
        Assert.That(WorkshopRules.StockReason(WorkshopRules.StockGate.NothingCarried), Is.EqualTo("Your pack has none of these materials."));
        Assert.That(WorkshopRules.StockReason(WorkshopRules.StockGate.BenchFull), Is.EqualTo("The bench is already full of what you carry."));
        Assert.That(WorkshopRules.StockReason(WorkshopRules.StockGate.Can), Is.EqualTo(""));
        Assert.That(WorkshopRules.StockButtonText, Is.EqualTo("PUT MY MATERIALS IN THE BENCH"));
        foreach (string text in new[] { WorkshopRules.StockButtonText, WorkshopRules.StockReason(WorkshopRules.StockGate.BenchFull), WorkshopRules.PackLine(1, 1, 1) })
            Assert.That(Regex.IsMatch(text, @"\b[ER]\b"), Is.False, text);
    }

    [Test]
    public void TheDigitsChooseTheFourPiecesInOrder()
    {
        (WorkshopRules.Key key, int index)[] digits =
        {
            (WorkshopRules.Key.One, 0), (WorkshopRules.Key.Two, 1), (WorkshopRules.Key.Three, 2), (WorkshopRules.Key.Four, 3),
        };
        foreach ((WorkshopRules.Key key, int index) in digits)
        {
            WorkshopRules.KeyAction action = WorkshopRules.Resolve(key, 2, 4);
            Assert.That(action.kind, Is.EqualTo(WorkshopRules.ActionKind.Choose), key.ToString());
            Assert.That(action.choice, Is.EqualTo(index), key.ToString());
        }
        Assert.That(WorkshopRules.Resolve(WorkshopRules.Key.Four, 0, 3).kind, Is.EqualTo(WorkshopRules.ActionKind.None), "a digit past the end of a shorter list does nothing");
    }

    [Test]
    public void UpAndDownMoveTheChoiceAndWrapRoundAtTheEnds()
    {
        Assert.That(WorkshopRules.Resolve(WorkshopRules.Key.Down, 0, 4).choice, Is.EqualTo(1));
        Assert.That(WorkshopRules.Resolve(WorkshopRules.Key.Down, 2, 4).choice, Is.EqualTo(3));
        Assert.That(WorkshopRules.Resolve(WorkshopRules.Key.Down, 3, 4).choice, Is.EqualTo(0), "Down from the last wraps to the first");
        Assert.That(WorkshopRules.Resolve(WorkshopRules.Key.Up, 3, 4).choice, Is.EqualTo(2));
        Assert.That(WorkshopRules.Resolve(WorkshopRules.Key.Up, 1, 4).choice, Is.EqualTo(0));
        Assert.That(WorkshopRules.Resolve(WorkshopRules.Key.Up, 0, 4).choice, Is.EqualTo(3), "Up from the first wraps to the last");
        Assert.That(WorkshopRules.Resolve(WorkshopRules.Key.Up, 0, 4).kind, Is.EqualTo(WorkshopRules.ActionKind.Choose));
    }

    [Test]
    public void EnterStartsRStocksAndAnythingElseIsIgnored()
    {
        Assert.That(WorkshopRules.Resolve(WorkshopRules.Key.Enter, 1, 4).kind, Is.EqualTo(WorkshopRules.ActionKind.Start));
        Assert.That(WorkshopRules.Resolve(WorkshopRules.Key.Stock, 1, 4).kind, Is.EqualTo(WorkshopRules.ActionKind.Stock));
        Assert.That(WorkshopRules.Resolve(WorkshopRules.Key.None, 1, 4).kind, Is.EqualTo(WorkshopRules.ActionKind.None));
        Assert.That(WorkshopRules.Resolve(WorkshopRules.Key.Enter, 1, 0).kind, Is.EqualTo(WorkshopRules.ActionKind.None), "no list, nothing to do");
        // Every key the screen can see maps to an action or to nothing, and only the keys named above do anything.
        var doing = Enum.GetValues(typeof(WorkshopRules.Key)).Cast<WorkshopRules.Key>().Where(k => WorkshopRules.Resolve(k, 0, 4).kind != WorkshopRules.ActionKind.None).ToArray();
        Assert.That(doing, Is.EquivalentTo(new[]
        {
            WorkshopRules.Key.One, WorkshopRules.Key.Two, WorkshopRules.Key.Three, WorkshopRules.Key.Four,
            WorkshopRules.Key.Up, WorkshopRules.Key.Down, WorkshopRules.Key.Enter, WorkshopRules.Key.Stock,
        }));
    }

    [Test]
    public void TheKeyHintIsOneShortLineThatNamesEveryKey()
    {
        string hint = WorkshopRules.KeyHint;
        Assert.That(hint, Does.Not.Contain("\n"));
        foreach (string word in new[] { "1-4", "Up/Down", "Enter", "R", "Esc" })
            Assert.That(hint, Does.Contain(word));
        Assert.That(hint.Length, Is.LessThanOrEqualTo(70), "one line at 28 px across 1436 px");
    }

    [Test]
    public void ThePlacedStructureStocksThroughOneMethodTheScreenAndTheRKeyShare()
    {
        MethodInfo stock = Find("PlacedStructure").GetMethod("StockFromPack", BindingFlags.Public | BindingFlags.Instance);
        Assert.That(stock, Is.Not.Null);
        Assert.That(stock.ReturnType, Is.EqualTo(typeof(bool)));
        Assert.That(stock.GetParameters().Select(p => p.ParameterType.Name), Is.EqualTo(new[] { "String&" }));
        Assert.That(Find("PlacedStructure").GetMethod("StockGateNow", BindingFlags.Public | BindingFlags.Instance), Is.Not.Null);
        Assert.That(Find("PlacedStructure").GetMethods(BindingFlags.Public | BindingFlags.Instance).Any(m => m.Name.Contains("Dismantle")), Is.False, "dismantling stays inside the structure and the Hammer's R");
    }

    [Test]
    public void EverythingOnTheScreenFitsInsideThePanelWithoutOverlap()
    {
        Type screen = Find("WorkshopScreen");
        var go = new GameObject("Workshop Screen Layout Test");
        try
        {
            Component instance = go.AddComponent(screen);
            screen.GetMethod("Build", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(instance, null);
            string[] names =
            {
                "Title", "Close", "Status", "Pack", "Row 0", "Row 1", "Row 2", "Row 3", "Stock Reason", "Stock", "Piece", "Piece Line", "Needs Heading", "Need 0", "Need 1", "Need 2",
                "Reason", "Start", "Hint",
            };
            RectTransform[] all = go.GetComponentsInChildren<RectTransform>(true);
            var boxes = new System.Collections.Generic.Dictionary<string, Rect>();
            foreach (string name in names)
            {
                RectTransform rt = all.First(r => r.name == name);
                boxes[name] = new Rect(rt.anchoredPosition.x, -rt.anchoredPosition.y, rt.sizeDelta.x, rt.sizeDelta.y); // top-left placed: x right, y down
            }
            var panel = new Rect(0f, 0f, 1500f, 920f);
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
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(go);
        }
    }
}
