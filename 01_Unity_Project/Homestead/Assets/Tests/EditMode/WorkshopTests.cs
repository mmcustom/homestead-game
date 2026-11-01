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
        Assert.That(WorkshopRules.StartPrompt(Stool), Is.EqualTo("Start Rough Stool  (2 Logs, 2 Branches, 3 h)"));
        Assert.That(WorkshopRules.EmptyPrompt(Stool), Is.EqualTo("Workshop  (empty: deliver 2 Logs, 2 Branches for a Rough Stool)"));
        Assert.That(WorkshopRules.ShortPrompt(Stool, 1, 0, 0), Is.EqualTo("Workshop  (a Rough Stool needs 2 Logs, 2 Branches; still short 1 Log, 2 Branches)"));
        Assert.That(WorkshopRules.NoToolPrompt(Stool), Is.EqualTo("Workshop  (stocked for a Rough Stool; carry an Axe, Primitive Axe or Wood Saw to start)"));
        Assert.That(WorkshopRules.WorkingPrompt(Stool, 2f), Is.EqualTo("Workshop  (making a Rough Stool, 2 h to go)"));
        Assert.That(WorkshopRules.DonePrompt(Stool), Is.EqualTo("Collect Rough Stool  (ready in the Workshop)"));
        Assert.That(WorkshopRules.StorePrompt("Logs, Branches"), Is.EqualTo("Store Logs, Branches"));
        Assert.That(WorkshopRules.DeliverHelp, Is.EqualTo("Deliver Logs, Branches and Cordage  (the bench holds 6 Logs, 8 Branches and 4 Cordage)"));
        Assert.That(WorkshopRules.DismantlePrompt(true, 2f), Does.StartWith("Can't dismantle the Workshop while a piece is being made"));
        Assert.That(WorkshopRules.DismantlePrompt(false, 0f), Does.StartWith("Dismantle the Workshop  (half the materials back"));

        string[] prompts =
        {
            WorkshopRules.StartPrompt(Stool), WorkshopRules.EmptyPrompt(Stool), WorkshopRules.ShortPrompt(Stool, 1, 0, 0), WorkshopRules.NoToolPrompt(Stool),
            WorkshopRules.WorkingPrompt(Stool, 2f), WorkshopRules.DonePrompt(Stool), WorkshopRules.StorePrompt("Logs"), WorkshopRules.DeliverHelp,
            WorkshopRules.DismantlePrompt(true, 2f), WorkshopRules.DismantlePrompt(false, 0f),
        };
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
        Assert.That(Enum.GetValues(Find("PileKind")).Length, Is.EqualTo(35), "nothing was inserted in the middle");
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
            "buildProgress", "builtDay", "contaminatedHours", "contents", "depthToWater", "digProgress", "id", "jobHours", "jobItem", "kind", "leachHours", "litres", "position", "yaw",
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
}
