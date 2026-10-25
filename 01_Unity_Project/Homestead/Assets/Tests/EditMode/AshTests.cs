using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

// EditMode tests for Ash and the Kindling Bundle (Early_Income_Design.md section 1 and 1b): the yield per piece of fuel, the fractional carry,
// the Ash market group and its numbers, the ledger mapping, and the old-save defaults. The pure rules are AshRules (Homestead.Economy); the
// fire's saved state and the crafting list are in the main assembly, reached by reflection on throwaway objects as the other tests do.
public class AshTests
{
    static MarketTable Table => MarketCatalog.Table;

    static Type Find(string name) =>
        AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => { try { return a.GetTypes(); } catch (Exception) { return new Type[0]; } })
                 .First(t => t.Name == name);

    // A fire's Ash bookkeeping, driven the way FireManager drives it: fuel goes in with its Ash owed, hours burn, the share made moves over.
    sealed class Ring
    {
        public float fuel, owed, made;

        public void Add(string itemId, int count, float hoursEach)
        {
            fuel += count * hoursEach;
            owed += AshRules.Owed(itemId, count);
        }

        public void Burn(float hours)
        {
            float before = fuel;
            fuel = Mathf.Max(0f, before - hours);
            float released = AshRules.Release(owed, before, before - fuel);
            owed = Mathf.Max(0f, owed - released);
            made += released;
            if (fuel <= 0f)
            {
                made += owed;
                owed = 0f;
            }
        }
    }

    // --- Yield per piece ---

    [Test]
    public void EachFuelLeavesItsOwnAsh()
    {
        Assert.That(AshRules.AshPerPiece("firewood"), Is.EqualTo(1f / 3f).Within(1e-6f));
        Assert.That(AshRules.AshPerPiece("branches"), Is.EqualTo(1f / 6f).Within(1e-6f));
        Assert.That(AshRules.AshPerPiece("sticks"), Is.EqualTo(1f / 12f).Within(1e-6f));
        Assert.That(AshRules.AshPerPiece("logs"), Is.EqualTo(5f / 3f).Within(1e-6f));
        Assert.That(AshRules.AshPerPiece("stone"), Is.EqualTo(0f));
        Assert.That(AshRules.AshPerPiece(null), Is.EqualTo(0f));
    }

    [Test]
    public void SticksAreOnePerTwelveAndTheOtherNumbersAreNamedConstants()
    {
        Assert.That(AshRules.SticksPerAsh, Is.EqualTo(12f), "Mike, 2026-10-25: 1 Ash per 12 Sticks");
        Assert.That(AshRules.FirewoodPerAsh, Is.EqualTo(3f));
        Assert.That(AshRules.BranchesPerAsh, Is.EqualTo(6f));
        Assert.That(AshRules.AshPerLog, Is.EqualTo(5f / 3f).Within(1e-6f));
    }

    [Test]
    public void BranchesAndLogsFollowFirewoodByBurnHours()
    {
        // Two Branches are one Firewood and a Log is five (WoodManager's splits), so they leave the same Ash as the Firewood they equal.
        Assert.That(2 * AshRules.AshPerPiece("branches"), Is.EqualTo(AshRules.AshPerPiece("firewood")).Within(1e-6f));
        Assert.That(AshRules.AshPerPiece("logs"), Is.EqualTo(5 * AshRules.AshPerPiece("firewood")).Within(1e-6f));
    }

    [Test]
    public void OwedIsThePiecesTimesTheYieldAndNothingForNoPieces()
    {
        Assert.That(AshRules.Owed("firewood", 6), Is.EqualTo(2f).Within(1e-6f));
        Assert.That(AshRules.Owed("sticks", 24), Is.EqualTo(2f).Within(1e-6f));
        Assert.That(AshRules.Owed("logs", 3), Is.EqualTo(5f).Within(1e-5f));
        Assert.That(AshRules.Owed("firewood", 0), Is.EqualTo(0f));
        Assert.That(AshRules.Owed("firewood", -2), Is.EqualTo(0f));
    }

    // --- The carry ---

    [Test]
    public void ThreeFirewoodBurnedInThreeSeparateBurnsMakeOneAsh()
    {
        var ring = new Ring();
        for (int burn = 1; burn <= 3; burn++)
        {
            ring.Add("firewood", 1, 2f);
            ring.Burn(2f);
            Assert.That(AshRules.Whole(ring.made), Is.EqualTo(burn < 3 ? 0 : 1), $"after burn {burn}");
        }
        Assert.That(ring.made, Is.EqualTo(1f).Within(0.001f));
    }

    [Test]
    public void TwelveSticksOneAtATimeMakeOneAshAndSixMakeNone()
    {
        var ring = new Ring();
        for (int stick = 1; stick <= 12; stick++)
        {
            ring.Add("sticks", 1, 0.15f);
            ring.Burn(0.15f);
            Assert.That(AshRules.Whole(ring.made), Is.EqualTo(stick < 12 ? 0 : 1), $"after stick {stick}");
        }
    }

    [Test]
    public void SixBranchesMakeOneAshAndALogMakesOneAndTwoThirds()
    {
        var branches = new Ring();
        branches.Add("branches", 6, 1f);
        branches.Burn(6f);
        Assert.That(AshRules.Whole(branches.made), Is.EqualTo(1));

        var log = new Ring();
        log.Add("logs", 1, 10f);
        log.Burn(10f);
        Assert.That(AshRules.Whole(log.made), Is.EqualTo(1));
        Assert.That(log.made, Is.EqualTo(5f / 3f).Within(0.001f), "the two thirds stay in the ring");
    }

    [Test]
    public void MixedFuelAddsUpAndTheFractionStaysAfterCollecting()
    {
        var ring = new Ring();
        ring.Add("firewood", 3, 2f);   // 1
        ring.Add("sticks", 12, 0.15f); // 1
        ring.Add("branches", 6, 1f);   // 1
        ring.Add("logs", 1, 10f);      // 5/3
        ring.Burn(ring.fuel);
        Assert.That(ring.made, Is.EqualTo(4f + 2f / 3f).Within(0.001f));
        Assert.That(AshRules.Whole(ring.made), Is.EqualTo(4));

        ring.made = AshRules.AfterCollect(ring.made, 4);
        Assert.That(ring.made, Is.EqualTo(2f / 3f).Within(0.001f), "the fraction carries on");
        ring.Add("firewood", 1, 2f);
        ring.Burn(2f);
        Assert.That(AshRules.Whole(ring.made), Is.EqualTo(1), "and the next burn completes it");
    }

    [Test]
    public void ABurnInTinyFrameStepsMakesTheSameAsOneBigBurn()
    {
        var ring = new Ring();
        ring.Add("firewood", 6, 2f); // 12 h, 2 Ash
        for (int i = 0; i < 6000; i++)
            ring.Burn(0.001f);
        Assert.That(ring.made, Is.EqualTo(1f).Within(0.01f), "half the fuel burned, half the Ash made");
        for (int i = 0; i < 6000; i++)
            ring.Burn(0.001f);
        ring.Burn(1f);
        Assert.That(AshRules.Whole(ring.made), Is.EqualTo(2));
        Assert.That(ring.owed, Is.EqualTo(0f));
    }

    [Test]
    public void ReleaseIsProportionalAndAllOfItWhenTheFuelRunsOut()
    {
        Assert.That(AshRules.Release(2f, 12f, 6f), Is.EqualTo(1f).Within(1e-6f), "half the hours, half the Ash");
        Assert.That(AshRules.Release(2f, 12f, 12f), Is.EqualTo(2f));
        Assert.That(AshRules.Release(2f, 4f, 9f), Is.EqualTo(2f), "more hours than the fuel held: just what is owed");
        Assert.That(AshRules.Release(2f, 0f, 1f), Is.EqualTo(2f), "no fuel hours left: the rest is made");
        Assert.That(AshRules.Release(0f, 12f, 6f), Is.EqualTo(0f));
        Assert.That(AshRules.Release(2f, 12f, 0f), Is.EqualTo(0f));
        Assert.That(AshRules.Release(2f, 12f, -3f), Is.EqualTo(0f));
        Assert.That(AshRules.Release(-1f, 12f, 3f), Is.EqualTo(0f));
    }

    [Test]
    public void WholeAndAfterCollectHandleTheEdges()
    {
        Assert.That(AshRules.Whole(0f), Is.EqualTo(0));
        Assert.That(AshRules.Whole(-2f), Is.EqualTo(0));
        Assert.That(AshRules.Whole(0.9f), Is.EqualTo(0));
        Assert.That(AshRules.Whole(0.99995f), Is.EqualTo(1), "float noise just under a whole");
        Assert.That(AshRules.Whole(3.4f), Is.EqualTo(3));
        Assert.That(AshRules.AfterCollect(2.5f, 2), Is.EqualTo(0.5f).Within(1e-6f));
        Assert.That(AshRules.AfterCollect(1f, 1), Is.EqualTo(0f));
        Assert.That(AshRules.AfterCollect(1.00005f, 1), Is.EqualTo(0f), "a speck of noise is dropped");
        Assert.That(AshRules.AfterCollect(0.5f, 3), Is.EqualTo(0f), "never below zero");
        Assert.That(AshRules.AfterCollect(2f, -4), Is.EqualTo(2f), "taking nothing changes nothing");
    }

    // --- The market ---

    [Test]
    public void AshIsItsOwnGroupSoldRawAtTwoDollars()
    {
        Assert.That(Table.Problems, Is.Empty);
        Assert.That(Table.TryGetRow("ash", out MarketRow row), Is.True);
        Assert.That(row.group, Is.EqualTo(MarketGroup.Ash));
        Assert.That(row.kind, Is.EqualTo(MarketKind.Bought));
        Assert.That(row.baseCents, Is.EqualTo(200L));
        Assert.That(row.pool, Is.EqualTo("ash"));
        Assert.That(Table.Rows.Count(r => r.group == MarketGroup.Ash), Is.EqualTo(1), "nothing else shares the group");
        Assert.That(Table.SellBaseCents("ash"), Is.EqualTo(200L));
        Assert.That(Table.HasPool("ash"), Is.True);
    }

    [Test]
    public void TheAshGroupDropsSixPercentFloorsAtAQuarterAndHasNoSeasons()
    {
        Assert.That(Table.TryGetRule(MarketGroup.Ash, out MarketGroupRule rule), Is.True);
        Assert.That((rule.dropBp, rule.floorBp), Is.EqualTo((600, 2500)));
        Assert.That(new[] { rule.spring, rule.summer, rule.fall, rule.winter }, Is.All.EqualTo(10000));
        Assert.That(rule.SeasonalChange, Is.False);
    }

    [Test]
    public void SellingAshFallsSixPercentAUnitAndStopsAtTheFloor()
    {
        SaleResult four = MarketRules.SellStack(200, 10000, 4, 600, 2500);
        Assert.That(four.firstUnitCents, Is.EqualTo(200L));
        Assert.That(four.lastUnitCents, Is.LessThan(four.firstUnitCents));
        Assert.That(four.newMultiplierBp, Is.EqualTo(Mathf.RoundToInt(10000f * Mathf.Pow(0.94f, 4)) / 1).Within(5), "about 94% a unit");

        SaleResult many = MarketRules.SellStack(200, 10000, 100, 600, 2500);
        Assert.That(many.newMultiplierBp, Is.EqualTo(2500), "never below 0.25 of normal");
        Assert.That(many.lastUnitCents, Is.EqualTo(50L));
    }

    [Test]
    public void AshPriceRecoversAQuarterOfTheGapEachDay()
    {
        Assert.That(MarketRules.Recover(5000, 10000, 25, AshRules.FloorBp), Is.EqualTo(6250));
    }

    [Test]
    public void AnOldMarketSaveWithNoAshPoolLoadsAshAtNormalPrice()
    {
        var data = new MarketSaveData
        {
            version = MarketSaveData.CurrentVersion,
            lastDay = 12,
            pools = new[] { new MarketPoolSave { pool = "firewood", bp = 6000 } },
        };
        var state = new MarketState();
        state.Restore(Table, data);
        Assert.That(state.Get("firewood"), Is.EqualTo(6000));
        Assert.That(state.Get("ash"), Is.EqualTo(10000), "a pool the save lacks");
        Assert.That(state.Get("kindling_bundle"), Is.EqualTo(10000));
    }

    // --- The Kindling Bundle ---

    [Test]
    public void TheKindlingBundleIsACheapCraftedGoodAtOneDollar()
    {
        Assert.That(Table.TryGetRow("kindling_bundle", out MarketRow row), Is.True);
        Assert.That(row.group, Is.EqualTo(MarketGroup.Crafted));
        Assert.That(row.baseCents, Is.EqualTo(100L));
        Assert.That(row.pool, Is.EqualTo("kindling_bundle"));
        Assert.That(AshRules.SticksPerBundle, Is.EqualTo(10));
    }

    [Test]
    public void TenSticksCraftOneKindlingBundle()
    {
        Type crafting = Find("Crafting");
        Array recipes = (Array)crafting.GetField("Recipes").GetValue(null);
        Type recipeType = recipes.GetType().GetElementType();
        object recipe = recipes.Cast<object>().Single(r => (string)recipeType.GetField("outputId").GetValue(r) == "kindling_bundle");
        Assert.That(recipeType.GetField("outputCount").GetValue(recipe), Is.EqualTo(1));

        Array ingredients = (Array)recipeType.GetField("ingredients").GetValue(recipe);
        Assert.That(ingredients.Length, Is.EqualTo(1));
        Type ingredient = ingredients.GetType().GetElementType();
        object only = ingredients.GetValue(0);
        Assert.That(ingredient.GetField("itemId").GetValue(only), Is.EqualTo("sticks"));
        Assert.That(ingredient.GetField("quantity").GetValue(only), Is.EqualTo(10));
    }

    // --- The ledger ---

    [Test]
    public void AshAndTheBundleShowUnderTheRightIncomeLines()
    {
        Assert.That(LedgerCategories.ForSale(MarketGroup.Ash), Is.EqualTo(LedgerCategory.SalesWoodStone));
        Assert.That(LedgerCategories.ForSale(Table.RuleOf("kindling_bundle").group), Is.EqualTo(LedgerCategory.SalesCrafted));
        Assert.That(LedgerCategories.IsIncome(LedgerCategories.ForSale(MarketGroup.Ash)), Is.True);
        Assert.That(LedgerCategories.Count, Is.EqualTo(19), "no new ledger category");
        Assert.That(LedgerSaveData.CurrentVersion, Is.EqualTo(1));
        Assert.That(MarketRules.GroupName(MarketGroup.Ash), Is.EqualTo("Ash"));
    }

    [Test]
    public void AnAshSaleIsRecordedOnItsItemRowAndAddsToTheWoodLine()
    {
        var ledger = new LedgerState();
        ledger.Begin(100000, 0, 360, 28);
        SaleResult sale = MarketRules.SellStack(200, 10000, 4, 600, 2500);
        ledger.RecordSale(LedgerCategories.ForSale(MarketGroup.Ash), "ash", sale.count, sale.totalCents);
        Assert.That(ledger.Open.items.Single().itemId, Is.EqualTo("ash"));
        Assert.That(ledger.Open.items.Single().units, Is.EqualTo(4));
        Assert.That(ledger.Open.cents[(int)LedgerCategory.SalesWoodStone], Is.EqualTo(sale.totalCents));
    }

    // --- Old fires ---

    [Test]
    public void AnOldFireSaveLoadsWithNoAshAndKeepsItsFuel()
    {
        const string oldJson = "{\"id\":3,\"position\":{\"x\":1.0,\"y\":2.0,\"z\":3.0},\"yaw\":45.0,\"fuelHours\":7.5,\"lit\":true}";
        Type fire = Find("CampfireState");
        object state = JsonUtility.FromJson(oldJson, fire);
        Assert.That(fire.GetField("fuelHours").GetValue(state), Is.EqualTo(7.5f));
        Assert.That(fire.GetField("lit").GetValue(state), Is.EqualTo(true));
        Assert.That(fire.GetField("ashOwed").GetValue(state), Is.EqualTo(0f));
        Assert.That(fire.GetField("ashMade").GetValue(state), Is.EqualTo(0f));
    }

    [Test]
    public void TheAshFieldsRoundTripThroughTheSave()
    {
        Type fire = Find("CampfireState");
        object state = Activator.CreateInstance(fire);
        fire.GetField("ashOwed").SetValue(state, 1.25f);
        fire.GetField("ashMade").SetValue(state, 2.5f);
        object back = JsonUtility.FromJson(JsonUtility.ToJson(state), fire);
        Assert.That(fire.GetField("ashOwed").GetValue(back), Is.EqualTo(1.25f));
        Assert.That(fire.GetField("ashMade").GetValue(back), Is.EqualTo(2.5f));
    }
}
