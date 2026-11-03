using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

// EditMode tests for Pottery P3 (Mike, 2026-11-03): the Clay Cooking Pot (clay_pot) works as a Cooking Pot wherever the game needs one. TripodRules.IsCookingPot and Serves are the one
// question; hanging, swapping, the boil gate, the tripod's texts and the know-how all follow. The iron pot (cooking_pot) is checked unchanged. Pure rules, plus a few reads of the source
// files and assets as text (the test assembly cannot load the main assembly). Nothing here touches the Saves folder or a scene.
public class ClayPotTests
{
    const string Iron = "cooking_pot", Clay = "clay_pot", Kettle = "potash_kettle";

    static string SourceText(string relative) => File.ReadAllText(Path.Combine(Application.dataPath, relative));

    static string Name(string id) => id == Iron ? "Cooking Pot" : id == Clay ? "Clay Cooking Pot" : id == Kettle ? "Potash Kettle" : id;

    // --- The one question ---

    [Test]
    public void TheClayPotIdIsTheFiredItemsAndBothPotsAreCookingPots()
    {
        Assert.That(TripodRules.ClayPotId, Is.EqualTo("clay_pot"));
        Assert.That(TripodRules.ClayPotId, Is.EqualTo(PotteryRules.FiredPotId));
        Assert.That(TripodRules.PotId, Is.EqualTo("cooking_pot"), "the iron pot's id is unchanged");
        Assert.That(TripodRules.IsCookingPot(Iron), Is.True);
        Assert.That(TripodRules.IsCookingPot(Clay), Is.True);
        foreach (string id in new[] { Kettle, "dry_clay_pot", "tripod", "bucket", "", null })
            Assert.That(TripodRules.IsCookingPot(id), Is.False, id ?? "null");
        Assert.That(TripodRules.IsVessel(Clay), Is.True);
        Assert.That(TripodRules.IsVessel(Iron), Is.True);
        Assert.That(TripodRules.IsVessel(Kettle), Is.True);
        Assert.That(TripodRules.IsVessel("dry_clay_pot"), Is.False, "a dry pot is not yet a pot");
    }

    [Test]
    public void WhatHangsServesTheJobWhenItIsTheSameVesselOrAnyCookingPotForThePot()
    {
        Assert.That(TripodRules.Serves(Iron, Iron), Is.True);
        Assert.That(TripodRules.Serves(Clay, Iron), Is.True, "a clay pot hung serves a job that needs the pot");
        Assert.That(TripodRules.Serves(Clay, Clay), Is.True);
        Assert.That(TripodRules.Serves(Kettle, Kettle), Is.True);
        Assert.That(TripodRules.Serves(Iron, Clay), Is.False, "only the pot is asked for, never specifically the clay one");
        Assert.That(TripodRules.Serves(Kettle, Iron), Is.False);
        Assert.That(TripodRules.Serves(Iron, Kettle), Is.False);
        Assert.That(TripodRules.Serves(Clay, Kettle), Is.False, "Lye still needs the Kettle");
        Assert.That(TripodRules.Serves("", Iron), Is.False);
        Assert.That(TripodRules.Serves(null, Iron), Is.False);
        Assert.That(TripodRules.Serves(Clay, ""), Is.False);
        Assert.That(TripodRules.Serves(Clay, null), Is.False);
    }

    [Test]
    public void TheIronPotGoesOnFirstWhenBothAreCarried()
    {
        Assert.That(TripodRules.PotToHang(true, true), Is.EqualTo(Iron));
        Assert.That(TripodRules.PotToHang(true, false), Is.EqualTo(Iron));
        Assert.That(TripodRules.PotToHang(false, true), Is.EqualTo(Clay));
        Assert.That(TripodRules.PotToHang(false, false), Is.Null);
    }

    // --- Hanging, swapping, taking down ---

    [Test]
    public void ACarriedClayPotHangsOnAnEmptyTripodAndStaysSpecific()
    {
        TripodRules.Step step = TripodRules.NextStep("", TripodRules.PotToHang(false, true), false);
        Assert.That(step.kind, Is.EqualTo(TripodRules.StepKind.Hang));
        Assert.That(step.hang, Is.EqualTo(Clay), "the hung id is the clay pot, so the Tripod can draw it");
        Assert.That(TripodRules.NextStep(null, Clay, true).hang, Is.EqualTo(Clay), "a pot before the Kettle, as before");
        Assert.That(TripodRules.NextStep("", Iron, false).hang, Is.EqualTo(Iron));
        Assert.That(TripodRules.NextStep("", (string)null, false).kind, Is.EqualTo(TripodRules.StepKind.None));
        Assert.That(TripodRules.NextStep("", (string)null, true).hang, Is.EqualTo(Kettle));
    }

    [Test]
    public void AClayPotHungSwapsWithTheKettleAndTheKettleSwapsWithEitherPot()
    {
        TripodRules.Step toKettle = TripodRules.NextStep(Clay, (string)null, true);
        Assert.That(toKettle.kind, Is.EqualTo(TripodRules.StepKind.Swap));
        Assert.That(toKettle.hang, Is.EqualTo(Kettle));
        Assert.That(toKettle.takeDown, Is.EqualTo(Clay), "the very pot that hangs comes down");

        TripodRules.Step ironToKettle = TripodRules.NextStep(Iron, Iron, true);
        Assert.That(ironToKettle.takeDown, Is.EqualTo(Iron));

        TripodRules.Step toClay = TripodRules.NextStep(Kettle, Clay, false);
        Assert.That(toClay.kind, Is.EqualTo(TripodRules.StepKind.Swap));
        Assert.That(toClay.hang, Is.EqualTo(Clay));
        Assert.That(toClay.takeDown, Is.EqualTo(Kettle));
    }

    [Test]
    public void AHungPotComesDownWhenNothingElseIsCarriedAndABadSaveIdJustComesDown()
    {
        TripodRules.Step down = TripodRules.NextStep(Clay, (string)null, false);
        Assert.That(down.kind, Is.EqualTo(TripodRules.StepKind.TakeDown));
        Assert.That(down.takeDown, Is.EqualTo(Clay));
        TripodRules.Step swapPots = TripodRules.NextStep(Iron, Clay, false);
        Assert.That(swapPots.kind, Is.EqualTo(TripodRules.StepKind.TakeDown), "a pot hung and another pot carried: it just comes down, as before");
        Assert.That(TripodRules.NextStep("mystery", Clay, true).kind, Is.EqualTo(TripodRules.StepKind.TakeDown));
    }

    [Test]
    public void TheOldBoolOverloadStillBehavesExactlyAsBefore()
    {
        Assert.That(TripodRules.NextStep("", true, false).hang, Is.EqualTo(Iron));
        Assert.That(TripodRules.NextStep("", false, true).hang, Is.EqualTo(Kettle));
        Assert.That(TripodRules.NextStep("", false, false).kind, Is.EqualTo(TripodRules.StepKind.None));
        Assert.That(TripodRules.NextStep(Iron, false, true).takeDown, Is.EqualTo(Iron));
        Assert.That(TripodRules.NextStep(Kettle, true, false).hang, Is.EqualTo(Iron));
        Assert.That(TripodRules.NextStep(Iron, false, false).kind, Is.EqualTo(TripodRules.StepKind.TakeDown));
    }

    [Test]
    public void TheTripodsTextsNameWhicheverPotIsHung()
    {
        Assert.That(TripodRules.StepPrompt(TripodRules.NextStep("", Clay, false), Name), Is.EqualTo("Hang the Clay Cooking Pot on the tripod"));
        Assert.That(TripodRules.StepPrompt(TripodRules.NextStep(Clay, (string)null, true), Name), Is.EqualTo("Swap the Clay Cooking Pot for the Potash Kettle"));
        Assert.That(TripodRules.StepPrompt(TripodRules.NextStep(Clay, (string)null, false), Name), Is.EqualTo("Take the Clay Cooking Pot down"));
        Assert.That(TripodRules.StepDone(TripodRules.NextStep("", Clay, false), Name), Is.EqualTo("Hung the Clay Cooking Pot on the tripod."));
        Assert.That(TripodRules.StepDone(TripodRules.NextStep(Clay, (string)null, false), Name), Is.EqualTo("Took the Clay Cooking Pot down."));
        Assert.That(TripodRules.StepPrompt(TripodRules.NextStep("", Iron, false), Name), Is.EqualTo("Hang the Cooking Pot on the tripod"), "the iron pot's text is unchanged");
    }

    // --- Boiling ---

    [Test]
    public void AClayPotHungAtALitFireBoilsWaterAndSapJustAsTheIronOneDoes()
    {
        Assert.That(TripodRules.BoilGate(true, true, Clay, Iron), Is.EqualTo(TripodRules.Gate.Ok), "a boil asks for the pot; the clay one serves");
        Assert.That(TripodRules.BoilGate(true, true, Iron, Iron), Is.EqualTo(TripodRules.Gate.Ok), "the iron pot is unchanged");
        Assert.That(TripodRules.BoilGate(true, true, Kettle, Iron), Is.EqualTo(TripodRules.Gate.WrongVessel));
        Assert.That(TripodRules.BoilGate(true, true, Clay, Kettle), Is.EqualTo(TripodRules.Gate.WrongVessel), "Lye still needs the Kettle");
        Assert.That(TripodRules.BoilGate(true, true, Kettle, Kettle), Is.EqualTo(TripodRules.Gate.Ok));
        Assert.That(TripodRules.BoilGate(true, true, "", Iron), Is.EqualTo(TripodRules.Gate.NoVessel));
        Assert.That(TripodRules.BoilGate(true, false, "", Iron), Is.EqualTo(TripodRules.Gate.NoTripod));
        Assert.That(TripodRules.BoilGate(false, true, Clay, Iron), Is.EqualTo(TripodRules.Gate.NoFire));
    }

    [Test]
    public void EveryPlaceThatAsksWhetherAPotServesGoesThroughTheOneHelper()
    {
        string cooking = SourceText("Scripts/Survival/Cooking.cs");
        Assert.That(cooking, Does.Contain("!TripodRules.Serves(fire.vessel, vesselId)"), "the nearest fire with the vessel hung");
        Assert.That(cooking, Does.Not.Contain("fire.vessel != vesselId"));
        string campfireCooking = SourceText("Scripts/Survival/CampfireCooking.cs");
        Assert.That(campfireCooking, Does.Contain("!TripodRules.Serves(vesselId, Cooking.BoilVessel("), "the tripod's job text");
        string fires = SourceText("Scripts/Managers/FireManager.cs");
        Assert.That(fires, Does.Contain("TripodRules.PotToHang("));
        Assert.That(fires, Does.Contain("TripodRules.IsCookingPot(step.hang) && JournalManager.Instance != null"), "the Journal note fires for either pot");
        Assert.That(SourceText("Scripts/World/Tripod.cs"), Does.Contain("TripodRules.PotToHang("));
        Assert.That(SourceText("Scripts/World/Campfire.cs"), Does.Contain("TripodRules.IsCookingPot(state.vessel)"));

        // No code outside TripodRules compares a hung vessel with a pot id any more.
        foreach (string file in Directory.GetFiles(Path.Combine(Application.dataPath, "Scripts"), "*.cs", SearchOption.AllDirectories).Where(f => !f.EndsWith("TripodRules.cs")))
        {
            string text = File.ReadAllText(file);
            Assert.That(Regex.IsMatch(text, @"vessel\s*(==|!=)\s*(TripodRules\.)?(PotId|ClayPotId)"), Is.False, file);
            Assert.That(Regex.IsMatch(text, @"(PotId|ClayPotId)\s*(==|!=)\s*[\w.]*vessel", RegexOptions.IgnoreCase), Is.False, file);
        }
    }

    [Test]
    public void TheInventorysBoilRowsStillAskForThePotAndTheTripodChecksItThroughServes()
    {
        string inventory = SourceText("Scripts/UI/InventoryScreen.cs");
        Assert.That(inventory, Does.Contain("lyeBoil ? TripodRules.KettleId : TripodRules.PotId"), "the row still asks for the Cooking Pot; VesselFireInReach lets either pot answer");
        Assert.That(inventory, Does.Contain("Cooking.VesselFireInReach(player, vesselId)"));
        Assert.That(SourceText("Scripts/Survival/Cooking.cs"), Does.Contain("IsBoilable(itemId) || IsSapBoil(itemId) ? TripodRules.PotId"), "water and sap still ask for the pot");
    }

    // --- The look, the Journal and the hints ---

    [Test]
    public void AHungClayPotIsDrawnInClayBrownAndTheIronOneIsStillIron()
    {
        string tripod = SourceText("Scripts/World/Tripod.cs");
        Assert.That(tripod, Does.Contain("BuildPot(vesselRoot.transform, hung == TripodRules.ClayPotId)"));
        Assert.That(tripod, Does.Contain("clay ? ClayPotColor : PotColor"));
        Assert.That(tripod, Does.Contain("new Color(0.52f, 0.34f, 0.22f)"), "the Kiln's clay brown");
        Assert.That(tripod, Does.Contain("new Color(0.13f, 0.13f, 0.15f)"), "the iron pot's colour is untouched");
    }

    [Test]
    public void TheKnowHowHintsTreatTheClayPotLikeThePot()
    {
        Assert.That(KnowHowRules.AfterHang(Clay), Is.EqualTo(KnowHowRules.AfterHang(Iron)));
        Assert.That(KnowHowRules.AfterHang(Clay), Does.Contain("Boil water or sap"));
        Assert.That(KnowHowRules.AfterHang(Kettle), Does.Contain("Lye"));
        Assert.That(KnowHowRules.ItemTip(Clay), Does.Contain("tripod").And.Contain("boil water or sap"));
        Assert.That(KnowHowRules.ItemTip(Iron), Is.EqualTo("Hang it on a tripod at a fire (hold E) to boil water or sap."), "the iron pot's tip is unchanged");
    }

    // --- The item, the market and what did not change ---

    [Test]
    public void TheClayPotIsAToolSoItIsTheSameKindOfThingAsTheIronPotInTheInventory()
    {
        string root = Path.Combine(Application.dataPath, "Resources", "Items", "Tools");
        string clay = File.ReadAllText(Path.Combine(root, "clay_pot.asset"));
        string iron = File.ReadAllText(Path.Combine(root, "cooking_pot.asset"));
        Assert.That(clay, Does.Contain("category: 1"));
        Assert.That(iron, Does.Contain("category: 1"));
        Assert.That(clay, Does.Contain("id: clay_pot"));
        Assert.That(clay, Does.Contain("displayName: Clay Cooking Pot"));
        Assert.That(SourceText("Scripts/Managers/InventoryManager.cs"), Does.Contain("itemId == SleepManager.SleepingBagId || itemId == WoodManager.TentId || itemId == WeatherScreen.WeatherStationId"),
                    "neither pot is carried-only: both may take a hotkey slot and be held, and holding either does nothing");
    }

    [Test]
    public void NoBuyAndSellLoopExistsBetweenTheTwoPots()
    {
        MarketTable table = MarketCatalog.Table;
        long ironBuy = table.BuyBaseCents(Iron), ironBack = table.SellBaseCents(Iron), claySell = table.SellBaseCents(Clay);
        Assert.That(ironBuy, Is.EqualTo(1200L), "the bought pot is $12");
        Assert.That(ironBack, Is.EqualTo(400L), "and sells back at 40% of the whole-dollar price, rounded down: $4.00 (Money.SellBackCents), not $4.80");
        Assert.That(claySell, Is.EqualTo(1000L), "the clay pot sells for $10");
        Assert.That(table.BuyBaseCents(Clay), Is.EqualTo(0L), "the store never sells a clay pot, so there is nothing to buy and resell");
        Assert.That(ironBack, Is.LessThan(ironBuy), "buying the iron pot and selling it back always loses");
        Assert.That(claySell, Is.LessThan(ironBuy), "and a sold clay pot does not pay for an iron one");
        Assert.That(table.RuleOf(Clay).group, Is.EqualTo(MarketGroup.Crafted));
        Assert.That(table.RuleOf(Iron).group, Is.EqualTo(MarketGroup.Stocked));
        Assert.That(table.RuleOf(Clay).dropBp, Is.EqualTo(1000));
        Assert.That(table.PoolOf(Clay), Is.Not.EqualTo(table.PoolOf(Iron)), "separate pools: selling one never moves the other's price");
    }

    [Test]
    public void TheLedgerTheStartingKitAndTheDevKitsStillMeanTheIronPot()
    {
        Assert.That(SourceText("Scripts/Economy/Ledger.cs"), Does.Contain("{ \"cooking_pot\", LedgerCategory.BuyCookingWaterFire }"));
        Assert.That(SourceText("Scripts/Economy/Ledger.cs"), Does.Not.Contain("clay_pot"), "a clay pot is made, never bought, so the ledger has no purchase line for it");
        Assert.That(SourceText("Scripts/Managers/DifficultyManager.cs"), Does.Contain("(Cooking.PotId, 1)"));
        Assert.That(SourceText("Scripts/Managers/DifficultyManager.cs"), Does.Not.Contain("clay_pot"));
        Assert.That(SourceText("Scripts/Debug/TimeDebugControls.cs"), Does.Contain("inventory.AddToPlayer(Cooking.PotId, 1);"));
        Assert.That(SourceText("Scripts/Survival/CampfireCooking.cs"), Does.Contain("int potLitres = 4;"), "the clay pot holds the same 4 litres");
    }
}
