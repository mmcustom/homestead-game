using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

// EditMode tests for the Leach Hopper's separate stock (Mike, 2026-11-04): Ash and water go in separately and at any time, E starts a batch. The pure rules are PotashRules (Homestead.Economy);
// the saved pile and the hourly finish are in the main assembly, reached by reflection as the other world tests do. Nothing here touches the Saves folder or a scene.
public class HopperStockTests
{
    static Type Find(string name) =>
        AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => { try { return a.GetTypes(); } catch (Exception) { return new Type[0]; } })
                 .First(t => t.Name == name);

    static string SourceText(string relative) => File.ReadAllText(Path.Combine(Application.dataPath, relative));

    // The order PotashRules.WaterWorstFirst lists: Unsafe, Questionable, Good, Excellent, boiled.
    static int[] Water(int unsafeL = 0, int questionable = 0, int good = 0, int excellent = 0, int boiled = 0) => new[] { unsafeL, questionable, good, excellent, boiled };

    static object NewHopper()
    {
        Type pile = Find("WoodPileState");
        object state = Activator.CreateInstance(pile);
        pile.GetField("kind").SetValue(state, Enum.Parse(Find("PileKind"), "LeachHopper"));
        return state;
    }

    static void Add(object pile, string id, int count) => pile.GetType().GetMethod("Add").Invoke(pile, new object[] { id, count, 0 });

    static int Count(object pile, string id) => (int)pile.GetType().GetMethod("Count").Invoke(pile, new object[] { id });

    static void Set(object pile, string field, object value) => pile.GetType().GetField(field).SetValue(pile, value);

    static T Get<T>(object pile, string field) => (T)pile.GetType().GetField(field).GetValue(pile);

    static void Finish(object hopper) => Find("WellsAndOuthouses").GetMethod("TurnAshIntoLye").Invoke(null, new[] { hopper });

    // --- The caps ---

    [Test]
    public void TheWaterHoldsTwelveLitresAndTheAshAndLyeTogetherTwelve()
    {
        Assert.That(PotashRules.HopperWaterCapacity, Is.EqualTo(12));
        Assert.That(PotashRules.HopperCapacity, Is.EqualTo(12));
        Assert.That(PotashRules.WaterRoom(0), Is.EqualTo(12));
        Assert.That(PotashRules.WaterRoom(5), Is.EqualTo(7));
        Assert.That(PotashRules.WaterRoom(12), Is.EqualTo(0));
        Assert.That(PotashRules.WaterRoom(30), Is.EqualTo(0));
        Assert.That(PotashRules.WaterRoom(-4), Is.EqualTo(12));
        Assert.That(PotashRules.Room(5 + 4), Is.EqualTo(3), "a running batch, stock and Lye waiting share the 12");
    }

    [Test]
    public void AshAloneCanBeStockedAndWaterAloneCanBeStocked()
    {
        PotashRules.StockPlan ash = PotashRules.PlanStock(7, Water(), 12, 12);
        Assert.That(ash.ash, Is.EqualTo(7));
        Assert.That(ash.litres, Is.EqualTo(0));
        PotashRules.StockPlan water = PotashRules.PlanStock(0, Water(good: 9), 12, 12);
        Assert.That(water.ash, Is.EqualTo(0));
        Assert.That(water.litres, Is.EqualTo(9));
    }

    [Test]
    public void EachStockStopsAtItsOwnCapAndTheOtherIsNotHeldBackByIt()
    {
        PotashRules.StockPlan plan = PotashRules.PlanStock(30, Water(good: 30), 4, 12);
        Assert.That(plan.ash, Is.EqualTo(4), "only 4 Ash of room");
        Assert.That(plan.litres, Is.EqualTo(12));
        PotashRules.StockPlan full = PotashRules.PlanStock(30, Water(good: 30), 0, 3);
        Assert.That(full.ash, Is.EqualTo(0));
        Assert.That(full.litres, Is.EqualTo(3));
    }

    [Test]
    public void TheWorstWaterGoesInFirstAndTheBoiledLast()
    {
        PotashRules.StockPlan plan = PotashRules.PlanStock(0, Water(unsafeL: 3, questionable: 4, good: 10, excellent: 5), 0, 12);
        Assert.That(plan.water, Is.EqualTo(new[] { 3, 4, 5, 0, 0 }));
        Assert.That(PotashRules.PlanStock(0, Water(unsafeL: 1, excellent: 1, boiled: 10), 0, 6).water, Is.EqualTo(new[] { 1, 0, 0, 1, 4 }));
    }

    [Test]
    public void NothingCarriedOrNoRoomStocksNothing()
    {
        Assert.That(PotashRules.PlanStock(0, null, 12, 12).litres, Is.EqualTo(0));
        Assert.That(PotashRules.PlanStock(-3, Water(good: -2), 12, 12).ash, Is.EqualTo(0));
        Assert.That(PotashRules.PlanStock(5, Water(good: 5), 0, 0).ash + PotashRules.PlanStock(5, Water(good: 5), 0, 0).litres, Is.EqualTo(0));
    }

    // --- The running batch and the stock ---

    [Test]
    public void WhileABatchRunsTheAshBeyondItIsStockForTheNextOne()
    {
        Assert.That(PotashRules.RunningBatch(8, 5), Is.EqualTo(5));
        Assert.That(PotashRules.AshStock(8, true, 5), Is.EqualTo(3), "3 Ash added while the batch of 5 runs wait");
        Assert.That(PotashRules.AshStock(8, false, 0), Is.EqualTo(8), "nothing leaching: all of it is stock");
        Assert.That(PotashRules.AshStock(0, true, 5), Is.EqualTo(0));
        Assert.That(PotashRules.RunningBatch(0, 5), Is.EqualTo(0));
    }

    [Test]
    public void ASavedBatchLargerThanTheAshThereNeverTakesMoreThanIsThere()
    {
        Assert.That(PotashRules.RunningBatch(4, 9), Is.EqualTo(4));
    }

    // --- The batch size ---

    [TestCase(8, 5, 5)]
    [TestCase(5, 8, 5)]
    [TestCase(12, 12, 12)]
    [TestCase(30, 30, 12)]
    [TestCase(0, 9, 0)]
    [TestCase(9, 0, 0)]
    [TestCase(-1, 4, 0)]
    public void ABatchIsTheSmallerOfTheAshAndTheLitresUpToTwelve(int ash, int litres, int batch)
    {
        Assert.That(PotashRules.NextBatch(ash, litres), Is.EqualTo(batch));
    }

    [Test]
    public void TheLeftoverStaysAsStockForTheNextBatch()
    {
        // 8 Ash and 5 L: a batch of 5 uses 5 Ash and all 5 L; 3 Ash are left.
        int batch = PotashRules.NextBatch(8, 5);
        Assert.That(PotashRules.AshFor(batch), Is.EqualTo(5));
        Assert.That(PotashRules.WaterFor(batch), Is.EqualTo(5));
        Assert.That(8 - PotashRules.AshFor(batch), Is.EqualTo(3));
        Assert.That(5 - PotashRules.WaterFor(batch), Is.EqualTo(0));
    }

    // --- E ---

    [Test]
    public void ECollectsLyeFirstEvenWhileABatchRunsOrStockWaits()
    {
        Assert.That(PotashRules.ActionFor(4, false, 6, 6), Is.EqualTo(PotashRules.HopperAction.CollectLye));
        Assert.That(PotashRules.ActionFor(4, true, 6, 6), Is.EqualTo(PotashRules.HopperAction.CollectLye));
        Assert.That(PotashRules.ActionFor(1, true, 0, 0), Is.EqualTo(PotashRules.HopperAction.CollectLye));
    }

    [Test]
    public void WithNoLyeReadyEReportsARunningBatchOtherwiseStartsOrSaysWhatIsMissing()
    {
        Assert.That(PotashRules.ActionFor(0, true, 6, 6), Is.EqualTo(PotashRules.HopperAction.StillLeaching));
        Assert.That(PotashRules.ActionFor(0, false, 6, 6), Is.EqualTo(PotashRules.HopperAction.Start));
        Assert.That(PotashRules.ActionFor(0, false, 6, 0), Is.EqualTo(PotashRules.HopperAction.NeedWater));
        Assert.That(PotashRules.ActionFor(0, false, 0, 6), Is.EqualTo(PotashRules.HopperAction.NeedAsh));
        Assert.That(PotashRules.ActionFor(0, false, 0, 0), Is.EqualTo(PotashRules.HopperAction.NeedBoth));
    }

    // --- Finishing ---

    [Test]
    public void FinishingTurnsOnlyTheBatchesAshIntoLyeAndKeepsTheStock()
    {
        object hopper = NewHopper();
        Add(hopper, "ash", 8);
        Add(hopper, "lye", 2);
        Set(hopper, "hopperBatch", 5);
        Set(hopper, "hopperWater", 4);
        Finish(hopper);
        Assert.That(Count(hopper, "ash"), Is.EqualTo(3), "the 3 Ash added during the batch are still Ash");
        Assert.That(Count(hopper, "lye"), Is.EqualTo(7));
        Assert.That(Get<int>(hopper, "hopperBatch"), Is.EqualTo(0), "the batch is over");
        Assert.That(Get<int>(hopper, "hopperWater"), Is.EqualTo(4), "stored water is not touched by a finish");
    }

    [Test]
    public void AnOldBatchWithNoSavedSizeTurnsAllItsAshIntoLyeAsBefore()
    {
        object hopper = NewHopper();
        Add(hopper, "ash", 8);
        Add(hopper, "lye", 3);
        Finish(hopper);
        Assert.That(Count(hopper, "ash"), Is.EqualTo(0));
        Assert.That(Count(hopper, "lye"), Is.EqualTo(11));
    }

    // --- Saves ---

    [Test]
    public void TheTwoNewFieldsAreAdditiveAndStartAtZero()
    {
        Type pile = Find("WoodPileState");
        Assert.That(pile.GetField("hopperWater").FieldType, Is.EqualTo(typeof(int)));
        Assert.That(pile.GetField("hopperBatch").FieldType, Is.EqualTo(typeof(int)));
        object fresh = Activator.CreateInstance(pile);
        Assert.That(Get<int>(fresh, "hopperWater"), Is.EqualTo(0));
        Assert.That(Get<int>(fresh, "hopperBatch"), Is.EqualTo(0));
    }

    [Test]
    public void ASaveFromBeforeTheFieldsLoadsAMidBatchHopperAsOneWholeBatch()
    {
        Type pile = Find("WoodPileState");
        object old = JsonUtility.FromJson("{\"id\":9,\"kind\":30,\"leachHours\":17.0,\"contents\":[{\"itemId\":\"ash\",\"count\":8,\"day\":0},{\"itemId\":\"lye\",\"count\":2,\"day\":0}]}", pile);
        Assert.That(Get<float>(old, "leachHours"), Is.EqualTo(17f));
        Assert.That(Get<int>(old, "hopperBatch"), Is.EqualTo(0));
        Assert.That(Get<int>(old, "hopperWater"), Is.EqualTo(0));
        int ash = Count(old, "ash");
        Assert.That(ash, Is.EqualTo(8));
        Assert.That(PotashRules.RunningBatch(ash, Get<int>(old, "hopperBatch")), Is.EqualTo(8), "the whole Ash count is the batch");
        Assert.That(PotashRules.AshStock(ash, true, 0), Is.EqualTo(0), "no stock appears from nowhere");
        Finish(old);
        Assert.That(Count(old, "lye"), Is.EqualTo(10), "it finishes exactly as it always did");
    }

    [Test]
    public void ABatchWithStockSurvivesASaveAndALoad()
    {
        Type pile = Find("WoodPileState");
        object hopper = NewHopper();
        Set(hopper, "id", 12);
        Set(hopper, "leachHours", 9f);
        Set(hopper, "hopperBatch", 5);
        Set(hopper, "hopperWater", 7);
        Add(hopper, "ash", 8);
        object back = JsonUtility.FromJson(JsonUtility.ToJson(hopper), pile);
        Assert.That(Get<int>(back, "hopperBatch"), Is.EqualTo(5));
        Assert.That(Get<int>(back, "hopperWater"), Is.EqualTo(7));
        Assert.That(Get<float>(back, "leachHours"), Is.EqualTo(9f));
        Assert.That(Count(back, "ash"), Is.EqualTo(8));
    }

    [Serializable]
    class PileAsTheOldCodeSawIt
    {
        public int id;
        public float leachHours;
        public int litres;
    }

    [Test]
    public void OldCodeReadingANewSaveIgnoresTheTwoNewFields()
    {
        object hopper = NewHopper();
        Set(hopper, "id", 3);
        Set(hopper, "leachHours", 6f);
        Set(hopper, "hopperBatch", 4);
        Set(hopper, "hopperWater", 9);
        string json = JsonUtility.ToJson(hopper);
        Assert.That(json, Does.Contain("hopperWater"));
        PileAsTheOldCodeSawIt oldView = JsonUtility.FromJson<PileAsTheOldCodeSawIt>(json);
        Assert.That(oldView.id, Is.EqualTo(3));
        Assert.That(oldView.leachHours, Is.EqualTo(6f));
        Assert.That(oldView.litres, Is.EqualTo(0), "the Well's litres are untouched by the Hopper's water");
    }

    [Test]
    public void NoOtherSavedFieldWasAddedToThePile()
    {
        string[] expected =
        {
            "id", "kind", "position", "yaw", "contents", "litres", "contaminatedHours", "digProgress", "builtDay", "buildProgress", "depthToWater", "leachHours",
            "hopperWater", "hopperBatch", "jobItem", "jobHours", "jobCount",
        };
        string[] actual = Find("WoodPileState").GetFields().Where(f => !f.IsStatic).Select(f => f.Name).ToArray();
        Assert.That(actual, Is.EquivalentTo(expected));
    }

    // --- Weight ---

    [Test]
    public void StoredWaterAddsNoWeightAndNoPileContents()
    {
        object hopper = NewHopper();
        Add(hopper, "ash", 3);
        int total = (int)hopper.GetType().GetProperty("Total").GetValue(hopper);
        Set(hopper, "hopperWater", 12);
        Assert.That((int)hopper.GetType().GetProperty("Total").GetValue(hopper), Is.EqualTo(total), "the water is a litre count, not a stack");
        Assert.That(((IList)hopper.GetType().GetField("contents").GetValue(hopper)).Count, Is.EqualTo(1));
        Assert.That(PotashRules.Room(total), Is.EqualTo(9), "the stored water does not take room from the Ash");
    }

    // --- The words ---

    [Test]
    public void RWithNothingCarriedSaysYouAreNotCarryingAny()
    {
        PotashRules.StockPlan none = PotashRules.PlanStock(0, Water(), 12, 12);
        Assert.That(PotashRules.LoadPrompt(none, 0, 0, 12, 12), Is.EqualTo("Load Ash and water  (you're not carrying any)"));
        Assert.That(KnowHowRules.HopperLoadHelp(), Is.EqualTo("Load Ash and water  (you're not carrying any)"));
        Assert.That(PotashRules.LoadRefusedMessage(0, 0, 12, 12), Does.Contain("You're not carrying any Ash or water"));
    }

    [Test]
    public void RWithOnlyOneCarriedLoadsItAndSaysWhatIsMissing()
    {
        PotashRules.StockPlan ash = PotashRules.PlanStock(4, Water(), 12, 12);
        Assert.That(PotashRules.LoadPrompt(ash, 4, 0, 12, 12), Is.EqualTo("Load 4 Ash  (no water in your pack)"));
        PotashRules.StockPlan water = PotashRules.PlanStock(0, Water(good: 6), 12, 12);
        Assert.That(PotashRules.LoadPrompt(water, 0, 6, 12, 12), Is.EqualTo("Load 6 L of water  (no Ash in your pack)"));
        PotashRules.StockPlan both = PotashRules.PlanStock(4, Water(good: 6), 12, 12);
        Assert.That(PotashRules.LoadPrompt(both, 4, 6, 12, 12), Is.EqualTo("Load 4 Ash and 6 L of water"));
    }

    [Test]
    public void RWhenOneIsFullLoadsTheOtherAndNamesWhichIsFull()
    {
        PotashRules.StockPlan plan = PotashRules.PlanStock(5, Water(good: 5), 0, 12);
        Assert.That(PotashRules.LoadPrompt(plan, 5, 5, 0, 12), Is.EqualTo("Load 5 L of water  (the Ash is full)"));
        PotashRules.StockPlan other = PotashRules.PlanStock(5, Water(good: 5), 12, 0);
        Assert.That(PotashRules.LoadPrompt(other, 5, 5, 12, 0), Is.EqualTo("Load 5 Ash  (the water is full)"));
    }

    [Test]
    public void RWhenBothCarriedAreFullSaysWhichOneIsFull()
    {
        PotashRules.StockPlan none = PotashRules.PlanStock(5, Water(), 0, 12);
        Assert.That(PotashRules.LoadPrompt(none, 5, 0, 0, 12), Does.Contain("full of Ash and Lye"));
        PotashRules.StockPlan none2 = PotashRules.PlanStock(0, Water(good: 5), 12, 0);
        Assert.That(PotashRules.LoadPrompt(none2, 0, 5, 12, 0), Does.Contain("full of water"));
        Assert.That(PotashRules.LoadRefusedMessage(5, 5, 0, 0), Does.Contain("Ash and Lye").And.Contain("water"));
    }

    [Test]
    public void TheLoadedMessageSaysWhatWentInWhatItHoldsAndWhatWasLeftOut()
    {
        PotashRules.StockPlan plan = PotashRules.PlanStock(4, Water(), 12, 12);
        Assert.That(PotashRules.LoadedMessage(plan, 9, 3, 4, 0), Is.EqualTo("Loaded 4 Ash  (the hopper holds 9 Ash and 3 L of water). Not loaded: no water in your pack."));
        PotashRules.StockPlan both = PotashRules.PlanStock(4, Water(good: 6), 12, 12);
        Assert.That(PotashRules.LoadedMessage(both, 4, 6, 4, 6), Is.EqualTo("Loaded 4 Ash and 6 L of water  (the hopper holds 4 Ash and 6 L of water)."));
    }

    [Test]
    public void TheEPromptsStartCollectOrSayWhatIsMissing()
    {
        Assert.That(PotashRules.StockedPrompt("Leach Hopper", 5, 5), Is.EqualTo("Start leaching  (5 Ash + 5 L -> 5 Lye in 24 h)"));
        Assert.That(PotashRules.StockedPrompt("Leach Hopper", 8, 5), Is.EqualTo("Start leaching  (5 Ash + 5 L -> 5 Lye in 24 h; 3 Ash left over)"));
        Assert.That(PotashRules.StockedPrompt("Leach Hopper", 5, 8), Is.EqualTo("Start leaching  (5 Ash + 5 L -> 5 Lye in 24 h; 3 L of water left over)"));
        Assert.That(PotashRules.StockedPrompt("Leach Hopper", 3, 0), Is.EqualTo("Leach Hopper  (3 Ash, 0 L of water - needs water to start)"));
        Assert.That(PotashRules.StockedPrompt("Leach Hopper", 0, 4), Is.EqualTo("Leach Hopper  (0 Ash, 4 L of water - needs Ash to start)"));
        Assert.That(PotashRules.LeachingPrompt("Leach Hopper", 5, 17f, 0, 0, 0), Is.EqualTo("Leach Hopper  (leaching 5 Lye, 17 h to go)"));
        Assert.That(PotashRules.LeachingPrompt("Leach Hopper", 5, 17f, 2, 3, 2), Is.EqualTo("Leach Hopper  (leaching 5 Lye, 17 h to go, 2 ready; 3 Ash and 2 L of water waiting)"));
        Assert.That(PotashRules.LeachingPrompt("Leach Hopper", 5, 17f, 0, 3, 0), Is.EqualTo("Leach Hopper  (leaching 5 Lye, 17 h to go; 3 Ash waiting)"));
    }

    [Test]
    public void TheStartAndStillLeachingAndNeedMessages()
    {
        Assert.That(PotashRules.StartedMessage(5, 3, 0), Is.EqualTo("Started leaching 5 Ash with 5 L of water - 5 Lye in 24 h. 3 Ash wait for the next batch."));
        Assert.That(PotashRules.StartedMessage(12, 0, 0), Is.EqualTo("Started leaching 12 Ash with 12 L of water - 12 Lye in 24 h."));
        Assert.That(PotashRules.StillLeachingMessage(10.5f), Is.EqualTo("Still leaching - 11 h to go."));
        Assert.That(PotashRules.NeedToStartMessage(PotashRules.HopperAction.NeedAsh, 0, 4), Does.Contain("no Ash"));
        Assert.That(PotashRules.NeedToStartMessage(PotashRules.HopperAction.NeedWater, 4, 0), Does.Contain("no water"));
        Assert.That(PotashRules.NeedToStartMessage(PotashRules.HopperAction.NeedBoth, 0, 0), Does.Contain("empty"));
    }

    // --- Rules that must not change ---

    [Test]
    public void TheLyeNumbersAreAsTheyWere()
    {
        Assert.That(PotashRules.AshPerLye, Is.EqualTo(1));
        Assert.That(PotashRules.WaterLitresPerLye, Is.EqualTo(1));
        Assert.That(PotashRules.LeachGameHours, Is.EqualTo(24f));
        Assert.That(PotashRules.LyePerPotash, Is.EqualTo(3));
        Assert.That(PotashRules.FirewoodPerPotash, Is.EqualTo(2));
        Assert.That(PotashRules.LyeBaseCents, Is.EqualTo(300));
        Assert.That(PotashRules.PotashBaseCents, Is.EqualTo(1600));
        Assert.That(PotashRules.KettleBaseCents, Is.EqualTo(2500));
        Assert.That(PotashRules.HopperLogs, Is.EqualTo(3));
        Assert.That(PotashRules.HopperTallGrass, Is.EqualTo(4));
    }

    // --- The world code ---

    [Test]
    public void TheHopperCodeWiresRToStockEToCollectOrStartAndTheRefundKeepsAllTheAshAndLoosesTheWater()
    {
        string world = SourceText("Scripts/World/PlacedStructure.cs");
        Assert.That(world, Does.Contain("case PileKind.LeachHopper: HopperInteract(); break;"));
        Assert.That(world, Does.Contain("PotashRules.ActionFor("));
        Assert.That(world, Does.Not.Contain("LoadRefusedWhileLeaching"));
        Assert.That(world, Does.Contain("refund.Return(AshRules.ItemId, state.Count(AshRules.ItemId));"), "all the Ash, batch and stock, comes back");
        Assert.That(world, Does.Contain("refund.Return(PotashRules.LyeId, state.Count(PotashRules.LyeId));"));
        Assert.That(world, Does.Not.Contain("refund.Return(\"water"), "the stored water is lost");
        Assert.That(world, Does.Contain("the water is lost"));
    }

    [Test]
    public void TheDevKitGivesThePackAshAndWaterAndFinishRunsTheNormalFinish()
    {
        string debug = SourceText("Scripts/Debug/TimeDebugControls.cs");
        Assert.That(debug, Does.Contain("InventoryManager.Instance.AddToPlayer(AshRules.ItemId, 12)"));
        Assert.That(debug, Does.Contain("R stocks them, E starts the batch"));
        Assert.That(SourceText("Scripts/Managers/WellsAndOuthouses.cs"), Does.Contain("PotashRules.RunningBatch(hopper.Count(AshRules.ItemId), hopper.hopperBatch)"));
    }
}
