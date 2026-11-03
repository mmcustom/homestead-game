using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

// EditMode tests for Pottery P1 (Mike, 2026-11-02): PotteryRules (the recipe table, the caps, the shelf's stamp maths, the gates, the texts, the keys, the bank rule) and the
// parts in the main assembly (pile kinds, the staged site's rows, the saved pile and its one new field, the shelf through a save and load, the wood-moisture code leaving the
// shelf alone, FinishPotteryJob and the Potter screen's layout), reached by reflection as the other world tests do. Nothing here touches the Saves folder or a scene.
public class PotteryTests
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

    static void Add(object pile, string id, int count, int day = 0) => pile.GetType().GetMethod("Add").Invoke(pile, new object[] { id, count, day });
    static int Count(object pile, string id) => (int)pile.GetType().GetMethod("Count").Invoke(pile, new object[] { id });
    static IList Contents(object pile) => (IList)pile.GetType().GetField("contents").GetValue(pile);
    static int DayOf(object stack) => (int)stack.GetType().GetField("day").GetValue(stack);
    static string IdOf(object stack) => (string)stack.GetType().GetField("itemId").GetValue(stack);
    static int CountOf(object stack) => (int)stack.GetType().GetField("count").GetValue(stack);

    static PotteryRules.Recipe Cup => PotteryRules.Recipes[0];
    static PotteryRules.Recipe Bowl => PotteryRules.Recipes[1];
    static PotteryRules.Recipe Pot => PotteryRules.Recipes[2];
    static PotteryRules.Recipe Crock => PotteryRules.Recipes[3];
    static PotteryRules.Recipe Brick => PotteryRules.Recipes[4];

    static PotteryRules.ShelfEntry Entry(string id, int count, int ready) => new PotteryRules.ShelfEntry { itemId = id, count = count, ready = ready };

    // --- The numbers ---

    [Test]
    public void TheSiteIsThreeLogsEightBranchesEightTallGrassTwoCordageAndTwentyFourBlows()
    {
        Assert.That(PotteryRules.SiteLogs, Is.EqualTo(3));
        Assert.That(PotteryRules.SiteBranches, Is.EqualTo(8));
        Assert.That(PotteryRules.SiteTallGrass, Is.EqualTo(8));
        Assert.That(PotteryRules.SiteCordage, Is.EqualTo(2));
        Assert.That(PotteryRules.BuildBlows, Is.EqualTo(24));
        Assert.That(PotteryRules.SiteCostText, Is.EqualTo("3 Logs, 8 Branches, 8 Tall Grass and 2 Cordage"));
        float kg = 3 * 8f + 8 * 1f + 8 * 0.1f + 2 * 0.3f;
        Assert.That(kg, Is.EqualTo(33.4f).Within(0.01f));
        Assert.That(kg, Is.GreaterThan(15f), "so a staged site, not a single trip");
    }

    [Test]
    public void TheFiveRecipesAreMikesTable()
    {
        string[] ids = { "dry_clay_cup", "dry_clay_bowl", "dry_clay_pot", "dry_water_crock", "dry_brick_batch" };
        Assert.That(PotteryRules.Recipes.Select(r => r.itemId), Is.EqualTo(ids));
        Assert.That(PotteryRules.Recipes.Select(r => r.name), Is.EqualTo(new[] { "Clay Cup", "Clay Bowl", "Clay Cooking Pot", "Water Crock", "Brick Batch" }));
        Assert.That(PotteryRules.Recipes.Select(r => r.clay), Is.EqualTo(new[] { 1, 2, 3, 4, 10 }));
        Assert.That(PotteryRules.Recipes.Select(r => r.sand), Is.EqualTo(new[] { 1, 1, 1, 1, 1 }));
        Assert.That(PotteryRules.Recipes.Select(r => r.shapeHours), Is.EqualTo(new[] { 1, 1, 2, 2, 2 }));
        Assert.That(PotteryRules.Recipes.Select(r => r.dryDays), Is.EqualTo(new[] { 1, 2, 3, 3, 3 }));
        Assert.That(PotteryRules.Recipes.Select(r => r.space), Is.EqualTo(new[] { 1, 1, 2, 2, 4 }));
        Assert.That(PotteryRules.Recipes.Select(r => r.dryKg), Is.EqualTo(new[] { 0.3f, 0.6f, 2f, 6f, 16f }));
        Assert.That(PotteryRules.IsPiece("dry_clay_cup"), Is.True);
        Assert.That(PotteryRules.IsPiece("clay"), Is.False);
    }

    [Test]
    public void EachRecipeIsAnAssetOfThatIdAndWeight()
    {
        foreach (PotteryRules.Recipe r in PotteryRules.Recipes)
        {
            object item = Static("ItemDatabase", "Get", r.itemId);
            Assert.That(item, Is.Not.Null, r.itemId);
            Assert.That((float)item.GetType().GetProperty("WeightKg").GetValue(item), Is.EqualTo(r.dryKg).Within(0.001f), r.itemId);
            Assert.That(item.GetType().GetProperty("DisplayName").GetValue(item), Is.EqualTo("Dry " + r.name), r.itemId);
        }
        object sand = Static("ItemDatabase", "Get", "sand");
        Assert.That(sand, Is.Not.Null);
        Assert.That((float)sand.GetType().GetProperty("WeightKg").GetValue(sand), Is.EqualTo(2f));
        Assert.That(sand.GetType().GetProperty("DisplayName").GetValue(sand), Is.EqualTo("Sand"));
    }

    [Test]
    public void TheDryPiecesHaveNoMarketRowSoTheyCannotBeSold()
    {
        foreach (PotteryRules.Recipe r in PotteryRules.Recipes)
            Assert.That(MarketCatalog.Table.SellBaseCents(r.itemId), Is.EqualTo(0L), r.itemId);
        Assert.That(MarketCatalog.Table.SellBaseCents("sand"), Is.EqualTo(0L));
    }

    [Test]
    public void TheCapsLetOneFullJobOfFourOfAnyPieceButBricksAndOneBrickBatchFit()
    {
        foreach (PotteryRules.Recipe r in new[] { Cup, Bowl, Pot, Crock })
        {
            Assert.That(PotteryRules.ClayFor(r, 4), Is.LessThanOrEqualTo(PotteryRules.ClayCap), r.name);
            Assert.That(PotteryRules.SandFor(r, 4), Is.LessThanOrEqualTo(PotteryRules.SandCap), r.name);
            Assert.That(PotteryRules.SpaceFor(r, 4), Is.LessThanOrEqualTo(PotteryRules.ShelfSpace), r.name);
        }
        Assert.That(PotteryRules.ClayFor(Brick, 1), Is.LessThanOrEqualTo(PotteryRules.ClayCap));
        Assert.That((PotteryRules.ClayCap, PotteryRules.SandCap, PotteryRules.ShelfSpace), Is.EqualTo((16, 8, 12)));
        Assert.That(PotteryRules.SpaceFor(Brick, 4), Is.GreaterThan(PotteryRules.ShelfSpace), "four Brick Batches never fit, three do");
        Assert.That(PotteryRules.SpaceFor(Brick, 3), Is.LessThanOrEqualTo(PotteryRules.ShelfSpace));
    }

    [TestCase("clay", 15, true)]
    [TestCase("clay", 16, false)]
    [TestCase("sand", 7, true)]
    [TestCase("sand", 8, false)]
    [TestCase("logs", 0, false)]
    [TestCase("dry_clay_cup", 0, false)]
    public void TheShedTakesClayAndSandUpToItsCaps(string itemId, int held, bool accepted)
    {
        Assert.That(PotteryRules.Accepts(itemId, held), Is.EqualTo(accepted));
    }

    [Test]
    public void TheCountIsOneToFourAndASavedZeroMeansOne()
    {
        Assert.That(PotteryRules.ClampCount(0), Is.EqualTo(1));
        Assert.That(PotteryRules.ClampCount(3), Is.EqualTo(3));
        Assert.That(PotteryRules.ClampCount(9), Is.EqualTo(4));
        Assert.That(PotteryRules.SavedCount(0), Is.EqualTo(1));
        Assert.That(PotteryRules.SavedCount(-2), Is.EqualTo(1));
        Assert.That(PotteryRules.SavedCount(3), Is.EqualTo(3));
        Assert.That(PotteryRules.SavedCount(99), Is.EqualTo(4));
    }

    [Test]
    public void ATotalIsTheCountTimesThePiecesOwnNumbers()
    {
        Assert.That(PotteryRules.ClayFor(Bowl, 3), Is.EqualTo(6));
        Assert.That(PotteryRules.SandFor(Bowl, 3), Is.EqualTo(3));
        Assert.That(PotteryRules.HoursFor(Pot, 3), Is.EqualTo(6));
        Assert.That(PotteryRules.SpaceFor(Crock, 4), Is.EqualTo(8));
        Assert.That(PotteryRules.HoursFor(Cup, 4), Is.EqualTo(4));
    }

    // --- Time: the stamps ---

    [Test]
    public void TheStampIsTheEndHourPlusTwentyFourTimesTheDryDays()
    {
        int end = PotteryRules.AbsoluteHour(5, 14);
        Assert.That(end, Is.EqualTo(5 * 24 + 14));
        Assert.That(PotteryRules.ReadyStamp(end, 2), Is.EqualTo(end + 48));
        Assert.That(PotteryRules.ReadyStamp(end, 1), Is.EqualTo(end + 24));
    }

    [TestCase(100, 100, true, 0)]
    [TestCase(100, 101, true, 0)]
    [TestCase(100, 99, false, 1)]
    [TestCase(100, 76, false, 1)]
    [TestCase(100, 75, false, 2)]
    [TestCase(100, 28, false, 3)]
    [TestCase(100, 27, false, 4)]
    public void DaysLeftCountsAStartedDayAndIsZeroOnceDry(int ready, int now, bool dry, int daysLeft)
    {
        Assert.That(PotteryRules.IsDry(ready, now), Is.EqualTo(dry));
        Assert.That(PotteryRules.DaysLeft(ready, now), Is.EqualTo(daysLeft));
    }

    [Test]
    public void ADayAfterTheNightOfSleepTheStampStillReadsRight()
    {
        // A job ends at day 3 hour 10, a Bowl dries 2 days, the player sleeps 8 hours and skips a day: all plain arithmetic on the hour.
        int ready = PotteryRules.ReadyStamp(PotteryRules.AbsoluteHour(3, 10), 2);
        Assert.That(PotteryRules.IsDry(ready, PotteryRules.AbsoluteHour(5, 9)), Is.False);
        Assert.That(PotteryRules.IsDry(ready, PotteryRules.AbsoluteHour(5, 10)), Is.True);
        Assert.That(PotteryRules.DaysLeft(ready, PotteryRules.AbsoluteHour(3, 18)), Is.EqualTo(2));
        Assert.That(PotteryRules.DaysLeft(ready, PotteryRules.AbsoluteHour(4, 11)), Is.EqualTo(1));
    }

    [Test]
    public void TheJobsHoursTickDownOneAtATimeAndAreDoneAtZero()
    {
        Assert.That(PotteryRules.AfterHour(3f), Is.EqualTo(2f));
        Assert.That(PotteryRules.AfterHour(1f), Is.EqualTo(0f));
        Assert.That(PotteryRules.AfterHour(0f), Is.EqualTo(0f));
        Assert.That(PotteryRules.IsDone(0f), Is.True);
        Assert.That(PotteryRules.IsDone(1f), Is.False);
        Assert.That(PotteryRules.HoursText(2.5f), Is.EqualTo("3 h"));
    }

    // --- The shelf ---

    [Test]
    public void TheShelfCountsSpaceDryAndWetPieces()
    {
        var shelf = new List<PotteryRules.ShelfEntry> { Entry("dry_clay_cup", 3, 50), Entry("dry_clay_pot", 2, 200), Entry("dry_water_crock", 1, 10) };
        Assert.That(PotteryRules.SpaceUsed(shelf), Is.EqualTo(3 * 1 + 2 * 2 + 1 * 2));
        Assert.That(PotteryRules.FreeSpace(shelf), Is.EqualTo(12 - 9));
        Assert.That(PotteryRules.DryPieces(shelf, 100), Is.EqualTo(4), "3 Cups and the Crock");
        Assert.That(PotteryRules.WetPieces(shelf, 100), Is.EqualTo(2), "the two Cooking Pots");
        Assert.That(PotteryRules.SpaceUsed(new List<PotteryRules.ShelfEntry>()), Is.EqualTo(0));
    }

    [Test]
    public void TheShelfLinesSayWhatIsDryingAndWhatIsReady()
    {
        var shelf = new List<PotteryRules.ShelfEntry>
        {
            Entry("dry_clay_cup", 2, 100 + 24),    // 1 day left at hour 100
            Entry("dry_clay_bowl", 3, 90),          // ready
            Entry("dry_clay_cup", 1, 100 + 30),    // 2 days left
        };
        string[] lines = PotteryRules.ShelfLines(shelf, 100, 5);
        Assert.That(lines, Is.EqualTo(new[] { "Drying: 2 Cups, 1 day left", "Drying: 1 Cup, 2 days left", "Ready: 3 Bowls" }));
        Assert.That(PotteryRules.ShelfLines(new List<PotteryRules.ShelfEntry>(), 100, 3), Is.EqualTo(new[] { "The shelf is empty" }));
        Assert.That(PotteryRules.ShelfLines(shelf, 100, 2), Is.EqualTo(new[] { "Drying: 2 Cups, 1 day left", "...and 2 more" }), "too many lines fold into one");
        Assert.That(PotteryRules.SpaceLine(5), Is.EqualTo("On the shelf  -  5 of 12 space used"));
    }

    // --- The gate ---

    [Test]
    public void AJobStartsOnlyWhenIdleWithRoomOnTheShelfAndEnoughStock()
    {
        Assert.That(PotteryRules.CanStart(Bowl, 3, false, 6, 3, 12), Is.EqualTo(PotteryRules.Gate.Ok));
        Assert.That(PotteryRules.CanStart(Bowl, 3, true, 6, 3, 12), Is.EqualTo(PotteryRules.Gate.Busy));
        Assert.That(PotteryRules.CanStart(Crock, 4, false, 16, 4, 7), Is.EqualTo(PotteryRules.Gate.ShelfFull), "4 Crocks need 8 space");
        Assert.That(PotteryRules.CanStart(Crock, 4, false, 16, 4, 8), Is.EqualTo(PotteryRules.Gate.Ok));
        Assert.That(PotteryRules.CanStart(Bowl, 3, false, 5, 3, 12), Is.EqualTo(PotteryRules.Gate.ShortOfStock), "6 Clay needed");
        Assert.That(PotteryRules.CanStart(Bowl, 3, false, 6, 2, 12), Is.EqualTo(PotteryRules.Gate.ShortOfStock), "3 Sand needed");
        Assert.That(PotteryRules.CanStart(Brick, 1, false, 10, 1, 4), Is.EqualTo(PotteryRules.Gate.Ok));
        Assert.That(PotteryRules.CanStart(Brick, 1, false, 9, 1, 4), Is.EqualTo(PotteryRules.Gate.ShortOfStock));
        Assert.That(PotteryRules.CanStart(Brick, 4, true, 40, 4, 12), Is.EqualTo(PotteryRules.Gate.Busy), "busy beats everything");
        Assert.That(PotteryRules.CanStart(Brick, 4, false, 40, 4, 12), Is.EqualTo(PotteryRules.Gate.ShelfFull), "the shelf is checked before the stock");
    }

    [Test]
    public void TheStartButtonAndReasonsReadInAFewWords()
    {
        Assert.That(PotteryRules.StartButtonText(PotteryRules.Gate.Ok, Bowl, 3), Is.EqualTo("START 3 BOWLS"));
        Assert.That(PotteryRules.StartButtonText(PotteryRules.Gate.Ok, Cup, 1), Is.EqualTo("START 1 CUP"));
        Assert.That(PotteryRules.StartButtonText(PotteryRules.Gate.Busy, Bowl, 3), Is.EqualTo("SHED IS BUSY"));
        Assert.That(PotteryRules.StartButtonText(PotteryRules.Gate.ShelfFull, Bowl, 3), Is.EqualTo("SHELF IS FULL"));
        Assert.That(PotteryRules.StartButtonText(PotteryRules.Gate.ShortOfStock, Bowl, 3), Is.EqualTo("SHORT OF MATERIALS"));
        Assert.That(PotteryRules.ReasonText(PotteryRules.Gate.ShortOfStock, Bowl, 3, 4, 1, 12, 0f, default, 0), Is.EqualTo("Still short 2 Clay, 2 Sand. Deliver more."));
        Assert.That(PotteryRules.ReasonText(PotteryRules.Gate.Busy, Bowl, 3, 6, 3, 12, 2f, Pot, 2), Is.EqualTo("2 Cooking Pots are 2 h from done."));
        Assert.That(PotteryRules.ReasonText(PotteryRules.Gate.ShelfFull, Crock, 4, 16, 4, 7, 0f, default, 0), Does.StartWith("Only 7 shelf spaces are free; 4 Water Crocks need 8."));
        Assert.That(PotteryRules.ReasonText(PotteryRules.Gate.Ok, Bowl, 3, 6, 3, 12, 0f, default, 0), Is.EqualTo(""));
        Assert.That(PotteryRules.ShortText(Brick, 1, 4, 0), Is.EqualTo("6 Clay, 1 Sand"));
    }

    [Test]
    public void TheNeedRowsAndLinesUseTheCountAndNameNoKeys()
    {
        PotteryRules.NeedRow[] rows = PotteryRules.NeedRows(Bowl, 3, 8, 2);
        Assert.That(rows[0].Text, Is.EqualTo("Clay - need 6 - shed has 8 - enough"));
        Assert.That(rows[1].Text, Is.EqualTo("Sand - need 3 - shed has 2 - short by 1"));
        Assert.That(PotteryRules.StatusLine(true, Cup, 3, 2f, 0, 0), Is.EqualTo("Shaping 3 Cups, 2 h to go"));
        Assert.That(PotteryRules.StatusLine(false, default, 0, 0f, 6, 2), Is.EqualTo("Shed is idle - it holds 6 Clay, 2 Sand"));
        Assert.That(PotteryRules.ListLabel(Bowl), Is.EqualTo("Clay Bowl\n1 h each  -  dries in 2 days"));
        Assert.That(PotteryRules.ListLabel(Cup), Is.EqualTo("Clay Cup\n1 h each  -  dries in 1 day"));
        Assert.That(PotteryRules.TotalLine(Pot, 3), Is.EqualTo("Total shaping time: 6 h  -  6 shelf space"));
        Assert.That(PotteryRules.CountLabel(Crock, 4), Is.EqualTo("4 Water Crocks"));
        foreach (string prompt in new[] { PotteryRules.OpenPrompt, PotteryRules.WorkingPrompt(Cup, 2, 2f), PotteryRules.CollectPrompt(3), PotteryRules.DeliverHelp, PotteryRules.StorePrompt("Clay"),
                                          PotteryRules.StockButtonText, PotteryRules.PackLine(1, 1) })
            Assert.That(Regex.IsMatch(prompt, @"\b[ER]\b"), Is.False, prompt);
    }

    [Test]
    public void ThePromptsAndTheDismantleRuleNameTheirReasons()
    {
        Assert.That(PotteryRules.WorkingPrompt(Cup, 3, 3f), Is.EqualTo("Potter's Shed  (shaping 3 Cups, 3 h to go)"));
        Assert.That(PotteryRules.CollectPrompt(1), Is.EqualTo("Collect 1 dry piece  (ready in the Potter's Shed)"));
        Assert.That(PotteryRules.CollectPrompt(4), Does.StartWith("Collect 4 dry pieces"));
        Assert.That(PotteryRules.DismantlePrompt(true, 2f, 0), Does.StartWith("Can't dismantle the Potter's Shed while pieces are being shaped"));
        Assert.That(PotteryRules.DismantlePrompt(false, 0f, 2), Does.Contain("2 pieces are still drying"));
        Assert.That(PotteryRules.DismantlePrompt(false, 0f, 1), Does.Contain("1 piece is still drying"));
        Assert.That(PotteryRules.DismantlePrompt(false, 0f, 0), Does.StartWith("Dismantle the Potter's Shed"));
        Assert.That(PotteryRules.TookMessage(3, 0), Does.StartWith("Took 3 dry pieces."));
        Assert.That(PotteryRules.TookMessage(2, 1), Does.Contain("1 left on the shelf"));
    }

    // --- The pack line and the stock button ---

    [Test]
    public void ThePackLineNamesClayAndSandAndNothingElse()
    {
        Assert.That(PotteryRules.PackLine(6, 2), Is.EqualTo("You carry: 6 Clay, 2 Sand"));
        Assert.That(PotteryRules.PackLine(6, 0), Is.EqualTo("You carry: 6 Clay"));
        Assert.That(PotteryRules.PackLine(0, 3), Is.EqualTo("You carry: 3 Sand"));
        Assert.That(PotteryRules.PackLine(0, 0), Is.EqualTo("You carry: nothing the shed uses"));
    }

    [Test]
    public void TheStockButtonIsOnOnlyWhenThePackHasSomethingTheShedHasRoomFor()
    {
        Assert.That(PotteryRules.StockState(0, 0, 0, 0), Is.EqualTo(PotteryRules.StockGate.NothingCarried));
        Assert.That(PotteryRules.StockState(0, 0, 16, 8), Is.EqualTo(PotteryRules.StockGate.NothingCarried));
        Assert.That(PotteryRules.StockState(3, 0, 15, 0), Is.EqualTo(PotteryRules.StockGate.Can));
        Assert.That(PotteryRules.StockState(3, 0, 16, 0), Is.EqualTo(PotteryRules.StockGate.ShedFull));
        Assert.That(PotteryRules.StockState(3, 1, 16, 7), Is.EqualTo(PotteryRules.StockGate.Can), "Sand still has room");
        Assert.That(PotteryRules.StockState(3, 1, 16, 8), Is.EqualTo(PotteryRules.StockGate.ShedFull));
        Assert.That(PotteryRules.StockReason(PotteryRules.StockGate.NothingCarried), Is.EqualTo("Your pack has no Clay or Sand."));
        Assert.That(PotteryRules.StockReason(PotteryRules.StockGate.ShedFull), Is.EqualTo("The shed is already full of what you carry."));
        Assert.That(PotteryRules.StockReason(PotteryRules.StockGate.Can), Is.EqualTo(""));
        Assert.That(PotteryRules.StockButtonText, Is.EqualTo("PUT MY CLAY AND SAND IN THE SHED"));
    }

    // --- The keys ---

    [Test]
    public void TheDigitsChooseTheFivePiecesInOrder()
    {
        PotteryRules.Key[] digits = { PotteryRules.Key.One, PotteryRules.Key.Two, PotteryRules.Key.Three, PotteryRules.Key.Four, PotteryRules.Key.Five };
        for (int i = 0; i < digits.Length; i++)
        {
            PotteryRules.KeyAction action = PotteryRules.Resolve(digits[i], 2, 5);
            Assert.That(action.kind, Is.EqualTo(PotteryRules.ActionKind.Choose), digits[i].ToString());
            Assert.That(action.choice, Is.EqualTo(i));
        }
        Assert.That(PotteryRules.Resolve(PotteryRules.Key.Five, 0, 4).kind, Is.EqualTo(PotteryRules.ActionKind.None), "a digit past the end of a shorter list does nothing");
    }

    [Test]
    public void UpAndDownWrapAndLeftAndRightChangeTheCount()
    {
        Assert.That(PotteryRules.Resolve(PotteryRules.Key.Down, 4, 5).choice, Is.EqualTo(0));
        Assert.That(PotteryRules.Resolve(PotteryRules.Key.Up, 0, 5).choice, Is.EqualTo(4));
        Assert.That(PotteryRules.Resolve(PotteryRules.Key.Down, 1, 5).choice, Is.EqualTo(2));
        Assert.That(PotteryRules.Resolve(PotteryRules.Key.Up, 3, 5).choice, Is.EqualTo(2));
        PotteryRules.KeyAction left = PotteryRules.Resolve(PotteryRules.Key.Left, 0, 5), right = PotteryRules.Resolve(PotteryRules.Key.Right, 0, 5);
        Assert.That((left.kind, left.delta), Is.EqualTo((PotteryRules.ActionKind.Count, -1)));
        Assert.That((right.kind, right.delta), Is.EqualTo((PotteryRules.ActionKind.Count, 1)));
    }

    [Test]
    public void EnterStartsRStocksAndOnlyThoseElevenKeysDoAnything()
    {
        Assert.That(PotteryRules.Resolve(PotteryRules.Key.Enter, 1, 5).kind, Is.EqualTo(PotteryRules.ActionKind.Start));
        Assert.That(PotteryRules.Resolve(PotteryRules.Key.Stock, 1, 5).kind, Is.EqualTo(PotteryRules.ActionKind.Stock));
        Assert.That(PotteryRules.Resolve(PotteryRules.Key.None, 1, 5).kind, Is.EqualTo(PotteryRules.ActionKind.None));
        Assert.That(PotteryRules.Resolve(PotteryRules.Key.Enter, 1, 0).kind, Is.EqualTo(PotteryRules.ActionKind.None), "no list, nothing to do");
        var doing = Enum.GetValues(typeof(PotteryRules.Key)).Cast<PotteryRules.Key>().Where(k => PotteryRules.Resolve(k, 0, 5).kind != PotteryRules.ActionKind.None).ToArray();
        Assert.That(doing.Length, Is.EqualTo(11));
        Assert.That(doing.Contains(PotteryRules.Key.None), Is.False);
    }

    [Test]
    public void TheKeyHintIsOneShortLineThatNamesEveryKey()
    {
        string hint = PotteryRules.KeyHint;
        Assert.That(hint, Does.Not.Contain("\n"));
        foreach (string word in new[] { "1-5", "Up/Down", "Left/Right", "Enter", "R", "Esc" })
            Assert.That(hint, Does.Contain(word));
        Assert.That(hint.Length, Is.LessThanOrEqualTo(90), "one line at 28 px across 1436 px");
    }

    [Test]
    public void TheWorkshopsOwnKeysAreUntouched()
    {
        Assert.That(Enum.GetNames(typeof(WorkshopRules.Key)), Is.EqualTo(new[] { "None", "One", "Two", "Three", "Four", "Up", "Down", "Enter", "Stock" }));
        Assert.That(WorkshopRules.KeyHint, Does.Contain("1-4"));
    }

    // --- The bank ---

    static readonly float Far = float.PositiveInfinity;

    [Test]
    public void AClayPatchGivesClayEvenWithNoWaterNearby()
    {
        Assert.That(PotteryRules.DigFor(true, false, 0f, Far), Is.EqualTo(PotteryRules.Dig.Patch));
        Assert.That(PotteryRules.DigFor(true, false, 0f, 2.5f), Is.EqualTo(PotteryRules.Dig.Patch), "a patch beats the Sand band");
        Assert.That(PotteryRules.GivesClay(PotteryRules.Dig.Patch), Is.True);
    }

    [Test]
    public void DryGroundWithWaterWithinTwoMetresIsTheWetBankAndGivesClay()
    {
        foreach (float waterline in new[] { 0.2f, 1f, 1.99f, PotteryRules.WetBankMetres })
            Assert.That(PotteryRules.DigFor(false, false, 0f, waterline), Is.EqualTo(PotteryRules.Dig.WetBank), $"{waterline} m");
    }

    [Test]
    public void DryGroundFromTwoToThreeMetresIsTheDryBankAndGivesSand()
    {
        foreach (float waterline in new[] { 2.01f, 2.5f, PotteryRules.BankReachMetres })
            Assert.That(PotteryRules.DigFor(false, false, 0f, waterline), Is.EqualTo(PotteryRules.Dig.DryBank), $"{waterline} m");
        Assert.That(PotteryRules.GivesSand(PotteryRules.Dig.DryBank), Is.True);
        Assert.That(PotteryRules.GivesClay(PotteryRules.Dig.DryBank), Is.False);
    }

    [Test]
    public void NoWaterWithinReachGivesNothing()
    {
        Assert.That(PotteryRules.DigFor(false, false, 0f, 3.01f), Is.EqualTo(PotteryRules.Dig.None));
        Assert.That(PotteryRules.DigFor(false, false, 0f, Far), Is.EqualTo(PotteryRules.Dig.None));
        Assert.That(PotteryRules.CanDig(PotteryRules.Dig.None), Is.False);
        Assert.That(PotteryRules.DigText(PotteryRules.Dig.None), Is.Empty);
    }

    [Test]
    public void TheWetBankIsInsideTheBankReachAndTheProbeRingsNestInsideTheWetBank()
    {
        Assert.That(PotteryRules.WetBankMetres, Is.EqualTo(2f));
        Assert.That(PotteryRules.BankReachMetres, Is.EqualTo(3f));
        Assert.That(PotteryRules.WetBankMetres, Is.LessThan(PotteryRules.BankReachMetres));
        Assert.That(PotteryRules.InnerProbeMetres, Is.EqualTo(1f));
        Assert.That(PotteryRules.InnerProbeMetres, Is.LessThan(PotteryRules.WetBankMetres));
        Assert.That(PotteryRules.WadeDigDepthMetres, Is.EqualTo(0.3f));
    }

    [Test]
    public void WaterShallowerThanTheLimitGivesClayFromTheBedAndDeeperWaterIsTooDeep()
    {
        // The aim point over water: the waterline distance does not matter, only the depth.
        Assert.That(PotteryRules.DigFor(false, true, 0f, 0f), Is.EqualTo(PotteryRules.Dig.CreekBed));
        Assert.That(PotteryRules.DigFor(false, true, 0.05f, 0f), Is.EqualTo(PotteryRules.Dig.CreekBed));
        Assert.That(PotteryRules.DigFor(false, true, 0.299f, Far), Is.EqualTo(PotteryRules.Dig.CreekBed), "just under");
        Assert.That(PotteryRules.DigFor(false, true, PotteryRules.WadeDigDepthMetres, Far), Is.EqualTo(PotteryRules.Dig.CreekBed), "at the limit");
        Assert.That(PotteryRules.DigFor(false, true, 0.301f, 0f), Is.EqualTo(PotteryRules.Dig.TooDeep), "just over");
        Assert.That(PotteryRules.DigFor(false, true, 2.5f, 0f), Is.EqualTo(PotteryRules.Dig.TooDeep), "deep water");
        Assert.That(PotteryRules.GivesClay(PotteryRules.Dig.CreekBed), Is.True);
        Assert.That(PotteryRules.CanDig(PotteryRules.Dig.TooDeep), Is.False, "nothing to dig in deep water, and so none in a flood");
    }

    [Test]
    public void TheHudNamesWhatTheNextDigGives()
    {
        Assert.That(PotteryRules.DigText(PotteryRules.Dig.WetBank), Is.EqualTo("Wet clay bank - hold click to dig Clay"));
        Assert.That(PotteryRules.DigText(PotteryRules.Dig.DryBank), Is.EqualTo("Dry creek bank - hold click to dig Sand"));
        Assert.That(PotteryRules.DigText(PotteryRules.Dig.Patch), Is.EqualTo("Clay patch - hold click to dig Clay"));
        Assert.That(PotteryRules.DigText(PotteryRules.Dig.CreekBed), Is.EqualTo("Creek bed - hold click to dig Clay"));
        Assert.That(PotteryRules.DigText(PotteryRules.Dig.TooDeep), Is.EqualTo("Water too deep to dig - try the shallows or the wet bank"));
        Assert.That(PotteryRules.DiggingText(PotteryRules.Dig.CreekBed), Is.EqualTo("Digging Clay out of the creek bed"));
        Assert.That(PotteryRules.DiggingText(PotteryRules.Dig.DryBank), Is.EqualTo("Digging Sand out of the bank"));
        Assert.That(PotteryRules.DugText(PotteryRules.Dig.CreekBed), Is.EqualTo("Dug a lump of Clay out of the creek bed"));
        Assert.That(PotteryRules.DugText(PotteryRules.Dig.WetBank), Is.EqualTo("Dug a lump of Clay out of the bank"));
        Assert.That(PotteryRules.DigName(PotteryRules.Dig.WetBank), Is.EqualTo("Clay"));
        Assert.That(PotteryRules.DigName(PotteryRules.Dig.DryBank), Is.EqualTo("Sand"));
    }

    [Test]
    public void TheIdleShovelLineAndTheShedHelpDoNotPromiseAnywhereOrAPatch()
    {
        foreach (string text in new[] { PotteryRules.ShovelIdleText, PotteryRules.DigHelp, PotteryRules.BuiltMessage })
        {
            Assert.That(text, Does.Not.Contain("patch").IgnoreCase, text);
            Assert.That(text, Does.Not.Contain("anywhere").IgnoreCase, text);
        }
        Assert.That(PotteryRules.ShovelIdleText, Does.Contain("water's edge").And.Contain("shallows").And.Contain("Clay").And.Contain("Sand"));
        Assert.That(PotteryRules.DigHelp, Does.Contain("water's edge").And.Contain("shallows").And.Contain("Sand"));
        Assert.That(PotteryRules.BuiltMessage, Does.Contain(PotteryRules.DigHelp));
    }

    [Test]
    public void EveryWaterQuestionInTheDigRuleGoesThroughOneFunction()
    {
        MethodInfo waterAt = Find("AxeTool").GetMethod("WaterAt", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.That(waterAt, Is.Not.Null);
        Assert.That(waterAt.ReturnType, Is.EqualTo(Find("AxeTool").GetNestedType("WaterReading")));
        Assert.That(Find("AxeTool").GetNestedType("WaterReading").GetField("surface"), Is.Not.Null);
        Assert.That(Find("AxeTool").GetNestedType("WaterReading").GetField("depth"), Is.Not.Null);

        string source = System.IO.File.ReadAllText(System.IO.Path.Combine(Application.dataPath, "Scripts/Player/AxeTool.cs"));
        int calls = source.Split('\n').Count(line => !line.TrimStart().StartsWith("//") && line.Contains("Animal.IsOverWater("));
        Assert.That(calls, Is.EqualTo(1), "only WaterAt asks Animal.IsOverWater; the rings, the aim point and the depth all go through it");
    }

    [Test]
    public void TheDigRuleTakesPlainNumbersAndNeedsNoScene()
    {
        Assert.That(PotteryRules.DigFor(false, true, 0.1f, 99f), Is.EqualTo(PotteryRules.Dig.CreekBed));
        foreach (MethodInfo method in typeof(PotteryRules).GetMethods(BindingFlags.Public | BindingFlags.Static).Where(m => m.Name == "DigFor"))
            Assert.That(method.GetParameters().Select(p => p.ParameterType), Is.EqualTo(new[] { typeof(bool), typeof(bool), typeof(float), typeof(float) }));
    }

    // --- The pieces in the main assembly ---

    [Test]
    public void TheTwoNewKindsAreAppendedAndNothingMoved()
    {
        Assert.That((int)Kind("PotterSite"), Is.EqualTo(35));
        Assert.That((int)Kind("PotterShed"), Is.EqualTo(36));
        Assert.That((int)Kind("WorkshopSite"), Is.EqualTo(33));
        Assert.That((int)Kind("Workshop"), Is.EqualTo(34));
        Assert.That((int)Kind("Felled"), Is.EqualTo(0));
        Assert.That(Enum.GetValues(Find("PileKind")).Length, Is.EqualTo(37));
    }

    [Test]
    public void TheSiteIsStagedBecomesAShedCostsWhatTheRulesSayAndStandsOnTheBuildTab()
    {
        Assert.That(Static("WellsAndOuthouses", "IsStagedSite", Kind("PotterSite")), Is.EqualTo(true));
        Assert.That(Static("WellsAndOuthouses", "IsStagedSite", Kind("PotterShed")), Is.EqualTo(false));
        Assert.That(Static("WellsAndOuthouses", "FinishedKind", Kind("PotterSite")), Is.EqualTo(Kind("PotterShed")));
        Assert.That(Static("WellsAndOuthouses", "FinalKind", Kind("PotterSite")), Is.EqualTo(Kind("PotterShed")));
        Assert.That(Static("WoodManager", "PileName", Kind("PotterSite")), Is.EqualTo("Potter's Shed Site"));
        Assert.That(Static("WoodManager", "PileName", Kind("PotterShed")), Is.EqualTo("Potter's Shed"));
        Assert.That(Static("WoodManager", "IsPotter", Kind("PotterSite")), Is.EqualTo(true));
        Assert.That(Static("WoodManager", "IsPotter", Kind("Workshop")), Is.EqualTo(false));
        Assert.That(Static("WoodManager", "IsWideShed", Kind("Workshop")), Is.EqualTo(true), "the Workshop keeps its clearances");
        Assert.That(Static("WoodManager", "IsWideShed", Kind("PotterShed")), Is.EqualTo(true));
        Assert.That(Static("WoodManager", "IsWideShed", Kind("HandCart")), Is.EqualTo(false));
        Assert.That(((IList)Find("WoodManager").GetField("Buildable").GetValue(null)).Contains(Kind("PotterSite")), Is.True, "on the Build tab");

        object[] cost = ((Array)Static("WellsAndOuthouses", "FinishedCost", Kind("PotterSite"))).Cast<object>().ToArray();
        Assert.That(cost.Select(IdOf), Is.EqualTo(new[] { "logs", "branches", "tall_grass", "cordage" }));
        Assert.That(cost.Select(CountOf), Is.EqualTo(new[] { 3, 8, 8, 2 }));
        Assert.That(Static("WellsAndOuthouses", "RequiresDig", Kind("PotterSite")), Is.EqualTo(false));
        Assert.That(Static("WellsAndOuthouses", "NeedOf", Kind("PotterSite"), "logs"), Is.EqualTo(3));
        string[][] order = ((Array)Static("WellsAndOuthouses", "DeliveryOrder", Kind("PotterSite"))).Cast<string[]>().ToArray();
        Assert.That(order.Length, Is.EqualTo(1));
        Assert.That(order[0], Is.EquivalentTo(new[] { "logs", "branches", "tall_grass", "cordage" }));
        Assert.That(Static("WellsAndOuthouses", "FinishedMessage", Kind("PotterSite")), Is.EqualTo(PotteryRules.BuiltMessage));
    }

    [Test]
    public void TheSiteTakesItsMaterialsAndTheFinishedShedTakesOnlyClayAndSandUpToTheCaps()
    {
        object site = NewPile("PotterSite");
        MethodInfo accepts = site.GetType().GetMethod("Accepts");
        foreach (string id in new[] { "logs", "branches", "tall_grass", "cordage" })
            Assert.That(accepts.Invoke(site, new object[] { id }), Is.EqualTo(true), id);
        Assert.That(accepts.Invoke(site, new object[] { "clay" }), Is.EqualTo(false));
        Add(site, "logs", 3);
        Assert.That(accepts.Invoke(site, new object[] { "logs" }), Is.EqualTo(false), "full of Logs");
        Assert.That(site.GetType().GetProperty("IsPlacedStructure").GetValue(site), Is.EqualTo(true));

        object shed = NewPile("PotterShed");
        accepts = shed.GetType().GetMethod("Accepts");
        Assert.That(accepts.Invoke(shed, new object[] { "clay" }), Is.EqualTo(true));
        Assert.That(accepts.Invoke(shed, new object[] { "sand" }), Is.EqualTo(true));
        foreach (string id in new[] { "logs", "dry_clay_cup", "stone", "firewood" })
            Assert.That(accepts.Invoke(shed, new object[] { id }), Is.EqualTo(false), id);
        Add(shed, "clay", 16);
        Assert.That(accepts.Invoke(shed, new object[] { "clay" }), Is.EqualTo(false), "16 is the cap");
        Add(shed, "sand", 7);
        Assert.That(accepts.Invoke(shed, new object[] { "sand" }), Is.EqualTo(true));
        Assert.That(shed.GetType().GetProperty("IsPlacedStructure").GetValue(shed), Is.EqualTo(true));
    }

    [Test]
    public void TheSproutBlockerForTheShedIs2Point4MetresAndTheOthersAreUnchanged()
    {
        Assert.That(RegrowRules.PotteryMetres, Is.EqualTo(2.4f));
        Assert.That(RegrowRules.BlockRadius(RegrowRules.Blocker.Pottery, 3f, 5f), Is.EqualTo(2.4f));
        Assert.That(RegrowRules.BlockRadius(RegrowRules.Blocker.Workshop, 3f, 5f), Is.EqualTo(2.9f));
        Assert.That(RegrowRules.BlockRadius(RegrowRules.Blocker.Structure, 3f, 5f), Is.EqualTo(1.5f));
    }

    // --- The saved pile ---

    [Test]
    public void OneIntFieldWasAppendedToTheSavedPileAndTheVersionsStayOne()
    {
        string[] fields = Find("WoodPileState").GetFields(BindingFlags.Public | BindingFlags.Instance).Select(f => f.Name).OrderBy(n => n).ToArray();
        Assert.That(fields, Is.EqualTo(new[]
        {
            "buildProgress", "builtDay", "contaminatedHours", "contents", "depthToWater", "digProgress", "id", "jobCount", "jobHours", "jobItem", "kind", "leachHours", "litres", "position", "yaw",
        }));
        Assert.That(Find("WoodPileState").GetField("jobCount").FieldType, Is.EqualTo(typeof(int)));
        FieldInfo version = Find("SaveManager").GetField("SaveVersion", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        Assert.That(version.GetRawConstantValue(), Is.EqualTo(1));
        Assert.That(LedgerSaveData.CurrentVersion, Is.EqualTo(1));
    }

    [Test]
    public void AnOldSaveWithNoJobCountAndNoPotterKindsLoadsAsIdleWithACountOfOne()
    {
        Type pile = Find("WoodPileState");
        string old = "{\"id\":4,\"kind\":34,\"position\":{\"x\":1.0,\"y\":2.0,\"z\":3.0},\"yaw\":10.0,\"contents\":[{\"itemId\":\"logs\",\"count\":2,\"day\":0}],\"litres\":0,\"contaminatedHours\":0.0,\"digProgress\":0.0,\"builtDay\":0,\"buildProgress\":0.0,\"depthToWater\":0.0,\"leachHours\":0.0,\"jobItem\":\"chair\",\"jobHours\":2.0}";
        object loaded = JsonUtility.FromJson(old, pile);
        Assert.That((int)pile.GetField("jobCount").GetValue(loaded), Is.EqualTo(0));
        Assert.That(PotteryRules.SavedCount((int)pile.GetField("jobCount").GetValue(loaded)), Is.EqualTo(1));
        Assert.That(pile.GetField("kind").GetValue(loaded), Is.EqualTo(Kind("Workshop")), "kind 34 is still the Workshop");
        Assert.That((string)pile.GetField("jobItem").GetValue(loaded), Is.EqualTo("chair"), "an old Workshop job is untouched");
        Assert.That(Count(loaded, "logs"), Is.EqualTo(2));
    }

    [Test]
    public void AShelfWithWetAndDryStacksAndARunningJobSurviveASaveAndLoad()
    {
        Type pile = Find("WoodPileState");
        object shed = NewPile("PotterShed");
        Add(shed, "clay", 9);
        Add(shed, "sand", 4);
        Add(shed, "dry_clay_cup", 3, 120);   // dry (ready earlier)
        Add(shed, "dry_clay_cup", 2, 480);   // wet
        Add(shed, "dry_clay_bowl", 1, 510);  // wet, another day
        pile.GetField("jobItem").SetValue(shed, "dry_water_crock");
        pile.GetField("jobCount").SetValue(shed, 3);
        pile.GetField("jobHours").SetValue(shed, 4f);

        object back = JsonUtility.FromJson(JsonUtility.ToJson(shed), pile);
        Assert.That(pile.GetField("kind").GetValue(back), Is.EqualTo(Kind("PotterShed")));
        Assert.That((string)pile.GetField("jobItem").GetValue(back), Is.EqualTo("dry_water_crock"));
        Assert.That((int)pile.GetField("jobCount").GetValue(back), Is.EqualTo(3));
        Assert.That((float)pile.GetField("jobHours").GetValue(back), Is.EqualTo(4f));
        Assert.That(Count(back, "clay"), Is.EqualTo(9));
        Assert.That(Count(back, "sand"), Is.EqualTo(4));
        Assert.That(Count(back, "dry_clay_cup"), Is.EqualTo(5));
        var stamps = Contents(back).Cast<object>().Where(s => IdOf(s) == "dry_clay_cup").Select(s => (DayOf(s), CountOf(s))).OrderBy(t => t.Item1).ToArray();
        Assert.That(stamps, Is.EqualTo(new[] { (120, 3), (480, 2) }), "the two stamps stay apart");

        var shelf = (List<PotteryRules.ShelfEntry>)Static("WellsAndOuthouses", "ShelfOf", back);
        Assert.That(PotteryRules.DryPieces(shelf, 300), Is.EqualTo(3));
        Assert.That(PotteryRules.WetPieces(shelf, 300), Is.EqualTo(3));
        Assert.That(PotteryRules.SpaceUsed(shelf), Is.EqualTo(3 + 2 + 1));
    }

    [Test]
    public void TakingFromTheShelfTakesTheSoonestDryFirst()
    {
        object shed = NewPile("PotterShed");
        Add(shed, "dry_clay_cup", 2, 480);
        Add(shed, "dry_clay_cup", 3, 120);
        shed.GetType().GetMethod("Take").Invoke(shed, new object[] { "dry_clay_cup", 3 });
        Assert.That(Count(shed, "dry_clay_cup"), Is.EqualTo(2));
        Assert.That(DayOf(Contents(shed)[0]), Is.EqualTo(480), "the wet pair is what is left");
    }

    // --- The wood-moisture code leaves the shelf and the stock alone ---

    [Test]
    public void RestampAgeAndCountMoistureLeaveTheShelfStampsAlone()
    {
        Type pile = Find("WoodPileState");
        object shed = NewPile("PotterShed");
        Add(shed, "dry_clay_cup", 3, 120);
        Add(shed, "dry_clay_cup", 2, 480);
        Add(shed, "clay", 5);

        Assert.That((bool)pile.GetMethod("RestampWood").Invoke(shed, new object[] { 77 }), Is.False, "no wood to re-stamp");
        pile.GetMethod("AgeWood").Invoke(shed, new object[] { 10 });
        var args = new object[] { "logs", 600, 7, 0, 0 };
        pile.GetMethod("CountMoisture").Invoke(shed, args);
        Assert.That(((int)args[3], (int)args[4]), Is.EqualTo((0, 0)), "nothing of wood counted");

        var after = Contents(shed).Cast<object>().Select(s => (IdOf(s), DayOf(s), CountOf(s))).OrderBy(t => t.Item1).ThenBy(t => t.Item2).ToArray();
        Assert.That(after, Is.EqualTo(new[] { ("clay", 0, 5), ("dry_clay_cup", 120, 3), ("dry_clay_cup", 480, 2) }));
    }

    [Test]
    public void TheMoistureFilterNeverAppliesToThePotteryItemsSoTheirPackBatchesAreNotMapped()
    {
        MethodInfo applies = Find("WoodMoisture").GetMethod("Applies", new[] { typeof(string) });
        foreach (string id in new[] { "sand", "clay", "dry_clay_cup", "dry_clay_bowl", "dry_clay_pot", "dry_water_crock", "dry_brick_batch" })
            Assert.That((bool)applies.Invoke(null, new object[] { id }), Is.False, id);
        Assert.That((bool)applies.Invoke(null, new object[] { "logs" }), Is.True);
        Assert.That((bool)Find("WoodManager").GetMethod("IsWood", new[] { typeof(string) }).Invoke(null, new object[] { "dry_clay_cup" }), Is.False);
    }

    // --- Finishing and the hour ---

    [Test]
    public void FinishingAJobPutsAllThePiecesOnTheShelfTogetherEachStampedAndFreesTheShed()
    {
        Type pile = Find("WoodPileState");
        object shed = NewPile("PotterShed");
        pile.GetField("jobItem").SetValue(shed, "dry_clay_bowl");
        pile.GetField("jobCount").SetValue(shed, 3);
        pile.GetField("jobHours").SetValue(shed, 0f);
        Add(shed, "clay", 4);
        Find("WellsAndOuthouses").GetMethod("FinishPotteryJob", BindingFlags.Public | BindingFlags.Static).Invoke(null, new[] { shed, (object)1000 });
        Assert.That(Count(shed, "dry_clay_bowl"), Is.EqualTo(3));
        object stack = Contents(shed).Cast<object>().First(s => IdOf(s) == "dry_clay_bowl");
        Assert.That(DayOf(stack), Is.EqualTo(1000 + 48), "a Bowl dries 2 days after the end hour");
        Assert.That(CountOf(stack), Is.EqualTo(3), "all three on one stamp");
        Assert.That(string.IsNullOrEmpty((string)pile.GetField("jobItem").GetValue(shed)), Is.True);
        Assert.That((float)pile.GetField("jobHours").GetValue(shed), Is.EqualTo(0f));
        Assert.That((int)pile.GetField("jobCount").GetValue(shed), Is.EqualTo(0));
        Assert.That(Count(shed, "clay"), Is.EqualTo(4), "the stock is untouched");

        // An idle shed finishes nothing; a piece this build does not know is dropped, not shelved.
        Find("WellsAndOuthouses").GetMethod("FinishPotteryJob", BindingFlags.Public | BindingFlags.Static).Invoke(null, new[] { shed, (object)2000 });
        Assert.That(Count(shed, "dry_clay_bowl"), Is.EqualTo(3));
        pile.GetField("jobItem").SetValue(shed, "mystery");
        Find("WellsAndOuthouses").GetMethod("FinishPotteryJob", BindingFlags.Public | BindingFlags.Static).Invoke(null, new[] { shed, (object)2000 });
        Assert.That(Count(shed, "mystery"), Is.EqualTo(0));
        Assert.That(string.IsNullOrEmpty((string)pile.GetField("jobItem").GetValue(shed)), Is.True);
    }

    // --- The Potter screen ---

    [Test]
    public void EveryLabelOnThePotterScreenIsAtLeast26AndTheBodyIs28()
    {
        Type screen = Find("PotteryScreen");
        var go = new GameObject("Potter Screen Layout Test");
        try
        {
            Component instance = go.AddComponent(screen);
            screen.GetMethod("Build", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(instance, null);
            Text[] texts = go.GetComponentsInChildren<Text>(true);
            Assert.That(texts.Length, Is.GreaterThan(20));
            foreach (Text text in texts)
                Assert.That(text.fontSize, Is.GreaterThanOrEqualTo(26), text.name);
            Assert.That(texts.First(t => t.name == "Need 0").fontSize, Is.EqualTo(28));
            Assert.That(texts.First(t => t.name == "Reason").fontSize, Is.EqualTo(28));
            Assert.That(go.GetComponentsInChildren<Button>(true).First(b => b.name == "Row 0").GetComponentInChildren<Text>(true).fontSize, Is.EqualTo(32));
            Assert.That(texts.First(t => t.name == "Piece").fontSize, Is.EqualTo(40));
            Assert.That(go.GetComponentsInChildren<Button>(true).First(b => b.name == "Start").GetComponentInChildren<Text>(true).fontSize, Is.EqualTo(36));
            Assert.That(go.GetComponentsInChildren<Button>(true).First(b => b.name == "Stock").GetComponentInChildren<Text>(true).fontSize, Is.EqualTo(32));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(go);
        }
    }

    [Test]
    public void EverythingOnThePotterScreenFitsInsideTheThousandHighPanelWithoutOverlap()
    {
        Type screen = Find("PotteryScreen");
        var go = new GameObject("Potter Screen Layout Test 2");
        try
        {
            Component instance = go.AddComponent(screen);
            screen.GetMethod("Build", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(instance, null);
            string[] names =
            {
                "Title", "Close", "Status", "Pack", "Row 0", "Row 1", "Row 2", "Row 3", "Row 4", "Stock Reason", "Stock", "Piece", "Piece Line", "Count Minus", "Count Label", "Count Plus",
                "Needs Heading", "Need 0", "Need 1", "Total", "Reason", "Start", "Shelf Heading", "Shelf 0", "Shelf 1", "Shelf 2", "Hint",
            };
            RectTransform[] all = go.GetComponentsInChildren<RectTransform>(true);
            var boxes = new Dictionary<string, Rect>();
            foreach (string name in names)
            {
                RectTransform rt = all.First(r => r.name == name);
                boxes[name] = new Rect(rt.anchoredPosition.x, -rt.anchoredPosition.y, rt.sizeDelta.x, rt.sizeDelta.y); // top-left placed: x right, y down
            }
            Assert.That((float)screen.GetField("PanelHeight").GetRawConstantValue(), Is.LessThanOrEqualTo(1000f));
            var panel = new Rect(0f, 0f, (float)screen.GetField("PanelWidth").GetRawConstantValue(), (float)screen.GetField("PanelHeight").GetRawConstantValue());
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
            Assert.That(boxes["Start"].height, Is.GreaterThanOrEqualTo(80f));
            Assert.That(boxes["Count Minus"].height, Is.GreaterThanOrEqualTo(80f));
            Assert.That(boxes["Count Plus"].height, Is.GreaterThanOrEqualTo(80f));
            Assert.That(boxes["Count Minus"].width, Is.GreaterThanOrEqualTo(80f));
            Assert.That(boxes["Row 1"].yMin - boxes["Row 0"].yMax, Is.GreaterThanOrEqualTo(12f), "gap between rows");
            Assert.That(boxes["Row 0"].height, Is.EqualTo(96f));
            Assert.That(panel.yMax - boxes["Hint"].yMax, Is.GreaterThanOrEqualTo(8f), "a margin under the key line");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(go);
        }
    }

    [Test]
    public void ThePlacedStructureStocksAndStartsThroughOneMethodEachAndHasNoPublicDismantle()
    {
        Type structure = Find("PlacedStructure");
        MethodInfo stock = structure.GetMethod("PotteryStockFromPack", BindingFlags.Public | BindingFlags.Instance);
        Assert.That(stock, Is.Not.Null);
        Assert.That(stock.ReturnType, Is.EqualTo(typeof(bool)));
        Assert.That(stock.GetParameters().Select(p => p.ParameterType.Name), Is.EqualTo(new[] { "String&" }));
        Assert.That(structure.GetMethod("PotteryStartJob", BindingFlags.Public | BindingFlags.Instance), Is.Not.Null);
        Assert.That(structure.GetMethod("PotteryStockGateNow", BindingFlags.Public | BindingFlags.Instance), Is.Not.Null);
        Assert.That(structure.GetMethods(BindingFlags.Public | BindingFlags.Instance).Any(m => m.Name.Contains("Dismantle")), Is.False);
    }
}
