using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

// EditMode tests for Pottery P2 (Mike, 2026-11-03): the Kiln. PotteryRules (the kiln's constants, the load space and Firewood gate, the start gate, the prompts, the fired id for each dry id, the Ash),
// the two-way clearance (Setbacks), the saved pile with a loaded and a firing kiln, the firing finishing through hours, the five fired items, and the market rows. The parts in the main assembly
// are reached by reflection as the other world tests do. Nothing here touches the Saves folder or a scene.
public class KilnTests
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
    static string IdOf(object stack) => (string)stack.GetType().GetField("itemId").GetValue(stack);
    static int CountOf(object stack) => (int)stack.GetType().GetField("count").GetValue(stack);
    static bool Accepts(object pile, string id) => (bool)pile.GetType().GetMethod("Accepts").Invoke(pile, new object[] { id });

    static PotteryRules.ShelfEntry Entry(string id, int count) => new PotteryRules.ShelfEntry { itemId = id, count = count, ready = 0 };

    static string AssetText(string folder, string id) => File.ReadAllText(Path.Combine(Application.dataPath, "Resources", "Items", folder, id + ".asset"));

    static string SourceText(string relative) => File.ReadAllText(Path.Combine(Application.dataPath, relative));

    // --- The numbers ---

    [Test]
    public void TheKilnIsSixStoneSixClayThirtyBlowsTwelveSpacesTenFirewoodTwentyFourHoursAndThreeAsh()
    {
        Assert.That(PotteryRules.KilnStone, Is.EqualTo(6));
        Assert.That(PotteryRules.KilnClay, Is.EqualTo(6));
        Assert.That(PotteryRules.KilnBuildBlows, Is.EqualTo(30));
        Assert.That(PotteryRules.KilnSpace, Is.EqualTo(12));
        Assert.That(PotteryRules.KilnFirewood, Is.EqualTo(10));
        Assert.That(PotteryRules.KilnFireHours, Is.EqualTo(24));
        Assert.That(PotteryRules.KilnAsh, Is.EqualTo(3), "AshRules.Whole(Owed(firewood, 10)) = 3.33 -> 3");
        Assert.That(PotteryRules.KilnRadiusMetres, Is.EqualTo(1.2f));
        Assert.That(PotteryRules.KilnClearanceMetres, Is.EqualTo(3f));
        Assert.That(PotteryRules.KilnTreeReachMetres, Is.EqualTo(4.2f).Within(1e-4f));
        Assert.That(PotteryRules.KilnCostText, Is.EqualTo("6 Stone and 6 Clay"));
    }

    [Test]
    public void TheSiteWeighsAboutFiftyOneKgAndTheHandCartCarriesStoneAndClayForIt()
    {
        float kg = PotteryRules.KilnStone * 5f + PotteryRules.KilnClay * 3.5f;
        Assert.That(kg, Is.EqualTo(51f).Within(0.01f));
        Assert.That(kg, Is.GreaterThan(15f), "so a staged site");
        Assert.That(kg, Is.LessThan(CartRules.CapacityKg), "one cart trip");
        Assert.That(CartRules.Carries("stone"), Is.True);
        Assert.That(CartRules.Carries("clay"), Is.True);
        Assert.That(CartRules.Carries("firewood"), Is.True, "the Firewood for the firing rides too");
    }

    [Test]
    public void EveryDryIdMapsToItsFiredIdWeightAndPriceAndBack()
    {
        string[] dry = { "dry_clay_cup", "dry_clay_bowl", "dry_clay_pot", "dry_water_crock", "dry_brick_batch" };
        string[] fired = { "clay_cup", "clay_bowl", "clay_pot", "water_crock", "brick_batch" };
        float[] kg = { 0.3f, 0.6f, 1.8f, 5.5f, 14f };
        long[] cents = { 400, 600, 1000, 1200, 600 };
        for (int i = 0; i < 5; i++)
        {
            Assert.That(PotteryRules.FiredIdOf(dry[i]), Is.EqualTo(fired[i]));
            Assert.That(PotteryRules.DryIdOf(fired[i]), Is.EqualTo(dry[i]));
            Assert.That(PotteryRules.IsFired(fired[i]), Is.True);
            Assert.That(PotteryRules.IsFired(dry[i]), Is.False);
            Assert.That(PotteryRules.Recipes[i].firedKg, Is.EqualTo(kg[i]));
            Assert.That(PotteryRules.Recipes[i].firedCents, Is.EqualTo(cents[i]));
            Assert.That(PotteryRules.TryFindFired(fired[i], out PotteryRules.Recipe recipe) && recipe.itemId == dry[i], Is.True);
        }
        Assert.That(PotteryRules.FiredIdOf("clay"), Is.Empty);
        Assert.That(PotteryRules.DryIdOf("stone"), Is.Empty);
        Assert.That(PotteryRules.IsFired("firewood"), Is.False);
    }

    [Test]
    public void TheFiveFiredItemsExistWithTheRightNamesWeightsAndCategories()
    {
        var expected = new[]
        {
            ("Materials", "clay_cup", "Clay Cup", "0.3", 3), ("Materials", "clay_bowl", "Clay Bowl", "0.6", 3), ("Tools", "clay_pot", "Clay Cooking Pot", "1.8", 1),
            ("Materials", "water_crock", "Water Crock", "5.5", 3), ("Materials", "brick_batch", "Brick Batch", "14", 3),
        };
        foreach ((string folder, string id, string name, string weight, int category) in expected)
        {
            string text = AssetText(folder, id);
            Assert.That(text, Does.Contain($"  id: {id}\n"), id);
            Assert.That(text, Does.Contain($"  displayName: {name}\n"), id);
            Assert.That(text, Does.Contain($"  weightKg: {weight}\n"), id);
            Assert.That(text, Does.Contain($"  category: {category}\n"), id);
            Assert.That(File.Exists(Path.Combine(Application.dataPath, "Resources", "Items", folder, id + ".asset.meta")), Is.True, id + " meta");
        }
    }

    [Test]
    public void TheClayCookingPotIsAToolSoTheTripodCanTakeItInP3WithoutChangingTheAsset()
    {
        Assert.That(AssetText("Tools", "cooking_pot"), Does.Contain("category: 1"), "the iron pot is a Tool");
        Assert.That(AssetText("Tools", "clay_pot"), Does.Contain("category: 1"), "so is the clay one");
        foreach (string id in new[] { "clay_cup", "clay_bowl", "water_crock", "brick_batch" })
            Assert.That(AssetText("Materials", id), Does.Contain("category: 3"), id + " stays a Material");
    }

    // --- The load: space and Firewood ---

    [Test]
    public void TheLoadSpaceIsTwelveUnitsInTheShelfsOwnUnits()
    {
        Assert.That(PotteryRules.KilnFreeSpace(new PotteryRules.ShelfEntry[0]), Is.EqualTo(12));
        var load = new[] { Entry("dry_clay_cup", 4), Entry("dry_clay_bowl", 2), Entry("dry_clay_pot", 1), Entry("dry_water_crock", 1) };
        Assert.That(PotteryRules.SpaceUsed(load), Is.EqualTo(4 + 2 + 2 + 2));
        Assert.That(PotteryRules.KilnFreeSpace(load), Is.EqualTo(2));
        Assert.That(PotteryRules.KilnFreeSpace(new[] { Entry("dry_brick_batch", 3) }), Is.EqualTo(0), "three Brick Batches fill it");
        Assert.That(PotteryRules.KilnFreeSpace(new[] { Entry("dry_brick_batch", 4) }), Is.EqualTo(0), "never negative");
    }

    [Test]
    public void TheRoomForEachPieceIsTheSpaceLeftOverItsSpaceAndFirewoodStopsAtTen()
    {
        Assert.That(PotteryRules.KilnRoomFor("dry_clay_cup", 0, 0), Is.EqualTo(12));
        Assert.That(PotteryRules.KilnRoomFor("dry_clay_bowl", 5, 0), Is.EqualTo(7));
        Assert.That(PotteryRules.KilnRoomFor("dry_clay_pot", 0, 0), Is.EqualTo(6));
        Assert.That(PotteryRules.KilnRoomFor("dry_water_crock", 11, 0), Is.EqualTo(0), "1 space left, a Crock needs 2");
        Assert.That(PotteryRules.KilnRoomFor("dry_brick_batch", 0, 0), Is.EqualTo(3));
        Assert.That(PotteryRules.KilnRoomFor("dry_brick_batch", 9, 0), Is.EqualTo(0));
        Assert.That(PotteryRules.KilnRoomFor("firewood", 12, 0), Is.EqualTo(10), "Firewood does not use the pieces' space");
        Assert.That(PotteryRules.KilnRoomFor("firewood", 0, 4), Is.EqualTo(6));
        Assert.That(PotteryRules.KilnRoomFor("firewood", 0, 10), Is.EqualTo(0));
        Assert.That(PotteryRules.KilnRoomFor("firewood", 0, 14), Is.EqualTo(0));
        Assert.That(PotteryRules.KilnRoomFor("clay", 0, 0), Is.EqualTo(0), "raw Clay and Sand are not loaded");
        Assert.That(PotteryRules.KilnRoomFor("clay_cup", 0, 0), Is.EqualTo(0), "nor are fired pieces");
        Assert.That(PotteryRules.KilnAccepts("dry_clay_cup", 11, 0), Is.True);
        Assert.That(PotteryRules.KilnAccepts("dry_clay_cup", 12, 0), Is.False);
    }

    [Test]
    public void ThePiecesLoadFirstInTheRecipeOrderAndThenTheFirewood()
    {
        Assert.That(PotteryRules.LoadOrder, Is.EqualTo(new[] { "dry_clay_cup", "dry_clay_bowl", "dry_clay_pot", "dry_water_crock", "dry_brick_batch", "firewood" }));
    }

    [Test]
    public void AKilnPileTakesPiecesUpToTheSpaceAndFirewoodUpToTenAndNothingElse()
    {
        object kiln = NewPile("Kiln");
        foreach (string id in new[] { "dry_clay_cup", "dry_water_crock", "firewood" })
            Assert.That(Accepts(kiln, id), Is.True, id);
        foreach (string id in new[] { "clay", "sand", "logs", "stone", "clay_cup", "ash" })
            Assert.That(Accepts(kiln, id), Is.False, id);

        Add(kiln, "dry_brick_batch", 3);
        Assert.That(Accepts(kiln, "dry_clay_cup"), Is.False, "12 of 12 spaces");
        Assert.That(Accepts(kiln, "firewood"), Is.True);
        Add(kiln, "firewood", 10);
        Assert.That(Accepts(kiln, "firewood"), Is.False, "10 is the fixed amount");
        Assert.That(kiln.GetType().GetProperty("IsPlacedStructure").GetValue(kiln), Is.EqualTo(true));
    }

    [Test]
    public void AKilnThatIsFiringOrHoldsFiredPiecesTakesNothing()
    {
        object firing = NewPile("Kiln");
        Add(firing, "dry_clay_cup", 2);
        firing.GetType().GetField("jobItem").SetValue(firing, PotteryRules.FiringMarker);
        firing.GetType().GetField("jobHours").SetValue(firing, 10f);
        Assert.That(Accepts(firing, "dry_clay_cup"), Is.False);
        Assert.That(Accepts(firing, "firewood"), Is.False);

        object done = NewPile("Kiln");
        Add(done, "clay_cup", 2);
        Assert.That(Accepts(done, "dry_clay_cup"), Is.False, "collect first");
        object ashOnly = NewPile("Kiln");
        Add(ashOnly, "ash", 3);
        Assert.That(Accepts(ashOnly, "firewood"), Is.False, "the Ash waits too");
    }

    // --- Phases, gates and prompts ---

    [Test]
    public void ThePhaseIsEmptyLoadingReadyFiringOrDone()
    {
        Assert.That(PotteryRules.PhaseOf(false, 0, 0, 0), Is.EqualTo(PotteryRules.KilnPhase.Empty));
        Assert.That(PotteryRules.PhaseOf(false, 0, 3, 0), Is.EqualTo(PotteryRules.KilnPhase.Loading));
        Assert.That(PotteryRules.PhaseOf(false, 0, 0, 10), Is.EqualTo(PotteryRules.KilnPhase.Loading), "Firewood but no piece");
        Assert.That(PotteryRules.PhaseOf(false, 0, 1, 9), Is.EqualTo(PotteryRules.KilnPhase.Loading));
        Assert.That(PotteryRules.PhaseOf(false, 0, 1, 10), Is.EqualTo(PotteryRules.KilnPhase.Ready), "one piece and 10 Firewood");
        Assert.That(PotteryRules.PhaseOf(true, 0, 5, 0), Is.EqualTo(PotteryRules.KilnPhase.Firing));
        Assert.That(PotteryRules.PhaseOf(true, 4, 0, 0), Is.EqualTo(PotteryRules.KilnPhase.Firing), "firing wins");
        Assert.That(PotteryRules.PhaseOf(false, 4, 0, 0), Is.EqualTo(PotteryRules.KilnPhase.Done));
        Assert.That(PotteryRules.CanFire(PotteryRules.KilnPhase.Ready), Is.True);
        foreach (PotteryRules.KilnPhase phase in new[] { PotteryRules.KilnPhase.Empty, PotteryRules.KilnPhase.Loading, PotteryRules.KilnPhase.Firing, PotteryRules.KilnPhase.Done })
            Assert.That(PotteryRules.CanFire(phase), Is.False, phase.ToString());
    }

    [Test]
    public void LoadingIsRefusedWhileFiringAndWhileFiredPiecesWaitAndNeedsSomethingToLoad()
    {
        Assert.That(PotteryRules.LoadGate(PotteryRules.KilnPhase.Firing, 5), Is.EqualTo(PotteryRules.KilnGate.Firing));
        Assert.That(PotteryRules.LoadGate(PotteryRules.KilnPhase.Done, 5), Is.EqualTo(PotteryRules.KilnGate.Waiting));
        Assert.That(PotteryRules.LoadGate(PotteryRules.KilnPhase.Empty, 0), Is.EqualTo(PotteryRules.KilnGate.NothingFits));
        Assert.That(PotteryRules.LoadGate(PotteryRules.KilnPhase.Loading, 0), Is.EqualTo(PotteryRules.KilnGate.NothingFits));
        foreach (PotteryRules.KilnPhase phase in new[] { PotteryRules.KilnPhase.Empty, PotteryRules.KilnPhase.Loading, PotteryRules.KilnPhase.Ready })
            Assert.That(PotteryRules.LoadGate(phase, 1), Is.EqualTo(PotteryRules.KilnGate.Ok), phase.ToString());
        Assert.That(PotteryRules.KilnLoadRefusal(PotteryRules.KilnGate.Firing), Is.EqualTo("The kiln is firing - nothing can be loaded until it is done."));
        Assert.That(PotteryRules.KilnLoadRefusal(PotteryRules.KilnGate.Waiting), Is.EqualTo("Fired pieces are waiting - collect them before loading again."));
        Assert.That(PotteryRules.KilnLoadRefusal(PotteryRules.KilnGate.NothingFits), Is.EqualTo("Nothing you carry fits (dry pieces up to 12 spaces, 10 Firewood)."));
    }

    [Test]
    public void TheKilnPromptsAreTheOnesMikeGave()
    {
        Assert.That(PotteryRules.KilnPrompt(PotteryRules.KilnPhase.Empty, 0, 0, 0, 0, 0f), Is.EqualTo("Kiln  (load dry pieces and 10 Firewood)"));
        Assert.That(PotteryRules.KilnPrompt(PotteryRules.KilnPhase.Loading, 3, 4, 0, 0, 0f), Is.EqualTo("Kiln  (3 pieces, 4 of 10 Firewood in)"));
        Assert.That(PotteryRules.KilnPrompt(PotteryRules.KilnPhase.Loading, 1, 0, 0, 0, 0f), Is.EqualTo("Kiln  (1 piece, 0 of 10 Firewood in)"));
        Assert.That(PotteryRules.KilnPrompt(PotteryRules.KilnPhase.Ready, 8, 10, 0, 0, 0f), Is.EqualTo("Fire the kiln  (8 pieces, 10 Firewood)"));
        Assert.That(PotteryRules.KilnPrompt(PotteryRules.KilnPhase.Firing, 8, 0, 0, 0, 17f), Is.EqualTo("Kiln  (firing, 17 h to go)"));
        Assert.That(PotteryRules.KilnPrompt(PotteryRules.KilnPhase.Done, 0, 0, 8, 3, 0f), Is.EqualTo("Collect fired pieces  (8)"));
        Assert.That(PotteryRules.KilnPrompt(PotteryRules.KilnPhase.Done, 0, 0, 0, 3, 0f), Is.EqualTo("Collect the Ash  (3)"));
        Assert.That(PotteryRules.KilnLoadPrompt(PotteryRules.KilnGate.Ok, "dry pieces and Firewood"), Is.EqualTo("Load dry pieces and Firewood"));
        Assert.That(PotteryRules.KilnLoadPrompt(PotteryRules.KilnGate.NothingFits, ""), Is.EqualTo("Load dry pieces and Firewood  (you're not carrying any)"));
        Assert.That(PotteryRules.KilnLoadPrompt(PotteryRules.KilnGate.Firing, ""), Is.EqualTo("Can't load the kiln while it is firing"));
        Assert.That(PotteryRules.KilnLoadPrompt(PotteryRules.KilnGate.Waiting, ""), Is.EqualTo("Collect the fired pieces before loading again"));
    }

    [Test]
    public void TheKilnTextsNameNoKeysAndStayShort()
    {
        var texts = new List<string>();
        foreach (PotteryRules.KilnPhase phase in Enum.GetValues(typeof(PotteryRules.KilnPhase)))
        {
            texts.Add(PotteryRules.KilnPrompt(phase, 12, 10, 12, 3, 24f));
            texts.Add(PotteryRules.KilnDismantlePrompt(phase, 24f));
        }
        foreach (PotteryRules.KilnGate gate in Enum.GetValues(typeof(PotteryRules.KilnGate)))
        {
            texts.Add(PotteryRules.KilnLoadPrompt(gate, "dry pieces and Firewood"));
            texts.Add(PotteryRules.KilnLoadRefusal(gate));
        }
        texts.Add(PotteryRules.KilnNotReady(0, 0));
        texts.Add(PotteryRules.KilnNotReady(3, 4));
        texts.Add(PotteryRules.KilnStarted(8));
        texts.Add(PotteryRules.KilnTook(8, 3, 0));
        texts.Add(PotteryRules.KilnLoaded("4 Clay Cups", 4, 0));
        foreach (string text in texts)
        {
            Assert.That(Regex.IsMatch(text, @"\b(E|R|Esc|Enter|Shift|Ctrl|click|press)\b"), Is.False, text);
            Assert.That(text.Length, Is.LessThanOrEqualTo(90), text);
        }
    }

    [Test]
    public void WhatELightsAndWhatItSaysWhenTheKilnIsNotReady()
    {
        Assert.That(PotteryRules.KilnNotReady(0, 0), Is.EqualTo("Load at least one dry piece and 10 Firewood first."));
        Assert.That(PotteryRules.KilnNotReady(0, 10), Is.EqualTo("Load at least one dry piece first."));
        Assert.That(PotteryRules.KilnNotReady(3, 4), Is.EqualTo("Needs 6 more Firewood to fire."));
        Assert.That(PotteryRules.KilnStarted(8), Is.EqualTo("The kiln is lit: 8 pieces will fire for 24 hours."));
        Assert.That(PotteryRules.KilnStarted(1), Is.EqualTo("The kiln is lit: 1 piece will fire for 24 hours."));
        Assert.That(PotteryRules.KilnBuiltMessage, Does.Contain("24 hours").And.Contain("3 Ash"));
    }

    [Test]
    public void CollectingSaysWhatCameAndWhatStayed()
    {
        Assert.That(PotteryRules.KilnTook(8, 3, 0), Is.EqualTo("Took 8 fired pieces and 3 Ash."));
        Assert.That(PotteryRules.KilnTook(1, 0, 0), Is.EqualTo("Took 1 fired piece."));
        Assert.That(PotteryRules.KilnTook(5, 3, 3), Is.EqualTo("Took 5 fired pieces and 3 Ash (3 left in the kiln, no room)."));
        Assert.That(PotteryRules.KilnTook(0, 3, 0), Is.EqualTo("Took 3 Ash."));
        Assert.That(PotteryRules.KilnTook(0, 0, 4), Is.EqualTo("Too heavy - no room to carry them"));
    }

    [Test]
    public void TheKilnComesDownOnlyWhenEmptyAndNotFiring()
    {
        Assert.That(PotteryRules.CanDismantleKiln(PotteryRules.KilnPhase.Empty), Is.True);
        foreach (PotteryRules.KilnPhase phase in new[] { PotteryRules.KilnPhase.Loading, PotteryRules.KilnPhase.Ready, PotteryRules.KilnPhase.Firing, PotteryRules.KilnPhase.Done })
            Assert.That(PotteryRules.CanDismantleKiln(phase), Is.False, phase.ToString());
        Assert.That(PotteryRules.KilnDismantlePrompt(PotteryRules.KilnPhase.Empty, 0f), Is.EqualTo("Take down the kiln  (half the materials back, rounded down)"));
        Assert.That(PotteryRules.KilnDismantlePrompt(PotteryRules.KilnPhase.Firing, 9f), Is.EqualTo("Can't take down the kiln while it is firing  (9 h to go)"));
        Assert.That(PotteryRules.KilnDismantlePrompt(PotteryRules.KilnPhase.Loading, 0f), Is.EqualTo("Empty the kiln before taking it down"));
        Assert.That(PotteryRules.KilnCostText, Is.EqualTo("6 Stone and 6 Clay"));
        Assert.That(PotteryRules.KilnStone / 2, Is.EqualTo(3), "half the Stone back");
        Assert.That(PotteryRules.KilnClay / 2, Is.EqualTo(3), "and half the Clay");
    }

    // --- The kinds, the site and the placement ---

    [Test]
    public void TheTwoNewKindsAreAppendedAfterThePotterKindsAndNothingMoved()
    {
        Assert.That((int)Kind("PotterSite"), Is.EqualTo(35));
        Assert.That((int)Kind("PotterShed"), Is.EqualTo(36));
        Assert.That((int)Kind("KilnSite"), Is.EqualTo(37));
        Assert.That((int)Kind("Kiln"), Is.EqualTo(38));
        Assert.That(Enum.GetValues(Find("PileKind")).Length, Is.EqualTo(43));
    }

    [Test]
    public void TheSiteIsStagedStoneThenClayBecomesAKilnAndStandsOnTheBuildTab()
    {
        Assert.That(Static("WellsAndOuthouses", "IsStagedSite", Kind("KilnSite")), Is.EqualTo(true));
        Assert.That(Static("WellsAndOuthouses", "IsStagedSite", Kind("Kiln")), Is.EqualTo(false));
        Assert.That(Static("WellsAndOuthouses", "FinishedKind", Kind("KilnSite")), Is.EqualTo(Kind("Kiln")));
        Assert.That(Static("WellsAndOuthouses", "FinalKind", Kind("KilnSite")), Is.EqualTo(Kind("Kiln")));
        Assert.That(Static("WoodManager", "PileName", Kind("KilnSite")), Is.EqualTo("Kiln Site"));
        Assert.That(Static("WoodManager", "PileName", Kind("Kiln")), Is.EqualTo("Kiln"));
        Assert.That(Static("WoodManager", "IsKiln", Kind("KilnSite")), Is.EqualTo(true));
        Assert.That(Static("WoodManager", "IsKiln", Kind("Kiln")), Is.EqualTo(true));
        Assert.That(Static("WoodManager", "IsKiln", Kind("PotterShed")), Is.EqualTo(false));
        Assert.That(((IList)Find("WoodManager").GetField("Buildable").GetValue(null)).Contains(Kind("KilnSite")), Is.True, "on the Build tab");

        object[] cost = ((Array)Static("WellsAndOuthouses", "FinishedCost", Kind("KilnSite"))).Cast<object>().ToArray();
        Assert.That(cost.Select(IdOf), Is.EqualTo(new[] { "stone", "clay" }));
        Assert.That(cost.Select(CountOf), Is.EqualTo(new[] { 6, 6 }));
        Assert.That(Static("WellsAndOuthouses", "RequiresDig", Kind("KilnSite")), Is.EqualTo(false));
        string[][] order = ((Array)Static("WellsAndOuthouses", "DeliveryOrder", Kind("KilnSite"))).Cast<string[]>().ToArray();
        Assert.That(order.Select(g => string.Join("+", g)), Is.EqualTo(new[] { "stone", "clay" }), "the Stone first, then the Clay");
        Assert.That(Static("WellsAndOuthouses", "FinishedMessage", Kind("KilnSite")), Is.EqualTo(PotteryRules.KilnBuiltMessage));

        object[] total = ((Array)Static("WellsAndOuthouses", "KilnCost")).Cast<object>().ToArray();
        Assert.That(total.Select(CountOf), Is.EqualTo(new[] { 6, 6 }), "what Dismantle halves");
    }

    [Test]
    public void TheSiteTakesStoneThenClayUpToSixAndTheBuildNeedsThirtyBlows()
    {
        object site = NewPile("KilnSite");
        Assert.That(Accepts(site, "stone"), Is.True);
        Assert.That(Accepts(site, "clay"), Is.False, "Stone first");
        Add(site, "stone", 6);
        Assert.That(Accepts(site, "stone"), Is.False, "six is all");
        Assert.That(Accepts(site, "clay"), Is.True);
        Add(site, "clay", 6);
        Assert.That(Accepts(site, "clay"), Is.False);
        Assert.That(Accepts(site, "logs"), Is.False);
        Assert.That(site.GetType().GetProperty("IsPlacedStructure").GetValue(site), Is.EqualTo(true));
        Assert.That(PotteryRules.KilnBuildBlows, Is.EqualTo(30));
        string source = SourceText("Scripts/Managers/WellsAndOuthouses.cs");
        Assert.That(source, Does.Contain("case PileKind.KilnSite: return PotteryRules.KilnBuildBlows;"));
    }

    [Test]
    public void TheSproutBlockerForTheKilnIsNamedAndTheOthersAreUnchanged()
    {
        Assert.That(RegrowRules.KilnMetres, Is.EqualTo(PotteryRules.RegrowMetres));
        Assert.That(RegrowRules.KilnMetres, Is.EqualTo(1.8f));
        Assert.That(RegrowRules.BlockRadius(RegrowRules.Blocker.Kiln, 3f, 5f), Is.EqualTo(1.8f));
        Assert.That(RegrowRules.BlockRadius(RegrowRules.Blocker.Pottery, 3f, 5f), Is.EqualTo(2.4f));
        Assert.That(RegrowRules.BlockRadius(RegrowRules.Blocker.Workshop, 3f, 5f), Is.EqualTo(2.9f));
        Assert.That(RegrowRules.BlockRadius(RegrowRules.Blocker.Structure, 3f, 5f), Is.EqualTo(1.5f));
    }

    [Test]
    public void TheKilnsSetbackFiguresInSetbacksAgreeWithPotteryRules()
    {
        SetbackRules rules = SetbackRules.Defaults;
        Assert.That(rules.kilnRadius, Is.EqualTo(PotteryRules.KilnRadiusMetres));
        Assert.That(rules.kilnFromStructure, Is.EqualTo(PotteryRules.KilnClearanceMetres));
        Assert.That(rules.kilnFromTree, Is.EqualTo(PotteryRules.KilnClearanceMetres));
    }

    static SiteInfo Site(float x, float radius, string name = "Small Cabin") =>
        new SiteInfo { kind = SiteKind.Building, position = new Vector3(x, 0f, 0f), name = name, radius = radius };

    [Test]
    public void ANewKilnKeepsThreeMetresOfClearAirFromEveryStructureAndTree()
    {
        SetbackRules rules = SetbackRules.Defaults;
        var cabin = new List<SiteInfo> { Site(0f, 2.5f) };
        // 1.2 (the dome) + 2.5 (the cabin reach) + 3 = 6.7 m centre to centre (6.71 m is clear of the float rounding).
        Assert.That(Setbacks.CanPlaceKiln(new Vector3(6.69f, 0f, 0f), cabin, false, rules, out string reason), Is.False);
        Assert.That(reason, Is.EqualTo("Too close to the Small Cabin: 3 m needed."));
        Assert.That(Setbacks.CanPlaceKiln(new Vector3(6.71f, 0f, 0f), cabin, false, rules, out reason), Is.True);
        Assert.That(reason, Is.Empty);
        Assert.That(Setbacks.CanPlaceKiln(new Vector3(20f, 0f, 0f), cabin, true, rules, out reason), Is.False, "a tree within reach");
        Assert.That(reason, Is.EqualTo("Too close to a tree: 3 m needed."));
        Assert.That(Setbacks.CanPlaceKiln(new Vector3(20f, 0f, 0f), new List<SiteInfo>(), false, rules, out _), Is.True);
    }

    [Test]
    public void AKilnThatStandsKeepsLaterBuildingsThreeMetresClearToo()
    {
        SetbackRules rules = SetbackRules.Defaults;
        var kiln = new List<SiteInfo> { Site(0f, PotteryRules.KilnRadiusMetres, "Kiln") };
        // A Cabin (reach 2.5) going up beside it: 1.2 + 2.5 + 3 = 6.7 m.
        Assert.That(Setbacks.CanPlaceBesideKiln(new Vector3(6.69f, 0f, 0f), 2.5f, kiln, rules, out string reason), Is.False);
        Assert.That(reason, Is.EqualTo("Too close to the Kiln: 3 m needed."));
        Assert.That(Setbacks.CanPlaceBesideKiln(new Vector3(6.71f, 0f, 0f), 2.5f, kiln, rules, out _), Is.True);
        // A Workshop (1.9), a Potter's Shed (1.6), a Kitchen (2.3), a Tent (1.2), a plain pile (0.7).
        foreach (float reach in new[] { 1.9f, 1.6f, 2.3f, 1.2f, 0.7f })
        {
            float needed = 1.2f + reach + 3f;
            Assert.That(Setbacks.CanPlaceBesideKiln(new Vector3(needed - 0.01f, 0f, 0f), reach, kiln, rules, out _), Is.False, $"reach {reach} just inside");
            Assert.That(Setbacks.CanPlaceBesideKiln(new Vector3(needed + 0.01f, 0f, 0f), reach, kiln, rules, out _), Is.True, $"reach {reach} just outside");
        }
        Assert.That(Setbacks.CanPlaceBesideKiln(new Vector3(0f, 0f, 0f), 0.7f, new List<SiteInfo>(), rules, out _), Is.True, "no kiln, no rule");
    }

    [Test]
    public void TheClearanceHoldsBothWaysWithTheSameGapForEveryReachAndDistance()
    {
        SetbackRules rules = SetbackRules.Defaults;
        foreach (float reach in new[] { 0.7f, 1.2f, 1.6f, 1.9f, 2.3f, 2.5f })
        {
            for (float distance = 0.5f; distance <= 8f; distance += 0.25f)
            {
                // The kiln goes up beside a structure that stands, or the structure goes up beside a kiln that stands: the same pair of positions, the same answer.
                bool kilnLast = Setbacks.CanPlaceKiln(new Vector3(distance, 0f, 0f), new List<SiteInfo> { Site(0f, reach) }, false, rules, out _);
                bool structureLast = Setbacks.CanPlaceBesideKiln(new Vector3(distance, 0f, 0f), reach, new List<SiteInfo> { Site(0f, PotteryRules.KilnRadiusMetres, "Kiln") }, rules, out _);
                Assert.That(structureLast, Is.EqualTo(kilnLast), $"reach {reach} at {distance} m");
            }
        }
    }

    [Test]
    public void TheKilnRuleIsStatelessSoALoadedOrFiringKilnKeepsTheSameClearance()
    {
        // Setbacks.CanPlaceBesideKiln is given only positions: whatever the kiln holds, it counts. The caller (WellsAndOuthouses.CanPlaceBesideKiln) lists every pile of kind Kiln or KilnSite.
        string source = SourceText("Scripts/Managers/WellsAndOuthouses.cs");
        Assert.That(source, Does.Contain("if (WoodManager.IsKiln(pile.kind))"));
        string wood = SourceText("Scripts/Managers/WoodManager.cs");
        Assert.That(wood, Does.Contain("WellsAndOuthouses.Instance.CanPlaceBesideKiln(kind, spot.point, out reason)"));
        Assert.That(wood, Does.Contain("!IsKiln(kind) && !IsHandCart(kind)"), "a parked Hand Cart is exempt, as from the 2 m spacing");
    }

    // --- Firing: hours, sleep, the result ---

    [Test]
    public void AFiringTakesTwentyFourGameHoursOneAtATimeAndSleepingJustTicksMoreHours()
    {
        float left = PotteryRules.KilnFireHours;
        for (int hour = 1; hour <= 23; hour++)
        {
            left = PotteryRules.AfterHour(left);
            Assert.That(PotteryRules.IsDone(left), Is.False, $"after {hour} h");
        }
        Assert.That(PotteryRules.HoursText(left), Is.EqualTo("1 h"));
        left = PotteryRules.AfterHour(left);
        Assert.That(PotteryRules.IsDone(left), Is.True, "after 24 h");

        // Lit at 22:00 and slept through eight hours (eight HourChanged ticks), then the rest of the day, then the night again.
        left = PotteryRules.KilnFireHours;
        for (int i = 0; i < 8; i++)
            left = PotteryRules.AfterHour(left);
        Assert.That(left, Is.EqualTo(16f));
        for (int i = 0; i < 16; i++)
            left = PotteryRules.AfterHour(left);
        Assert.That(PotteryRules.IsDone(left), Is.True);
        Assert.That(PotteryRules.AfterHour(0f), Is.EqualTo(0f), "never below zero");
    }

    [Test]
    public void WhenTheFiringEndsEveryDryStackBecomesItsFiredStackAndTheAshIsAdded()
    {
        object kiln = NewPile("Kiln");
        Add(kiln, "dry_clay_cup", 4);
        Add(kiln, "dry_clay_bowl", 2);
        Add(kiln, "dry_clay_pot", 1);
        Add(kiln, "dry_water_crock", 1);
        Add(kiln, "dry_brick_batch", 1);
        Add(kiln, "firewood", 10);
        Type pile = kiln.GetType();
        pile.GetMethod("Take").Invoke(kiln, new object[] { "firewood", 10 });
        pile.GetField("jobItem").SetValue(kiln, PotteryRules.FiringMarker);
        pile.GetField("jobHours").SetValue(kiln, 0f);

        Static("WellsAndOuthouses", "FinishFiring", kiln);

        Assert.That(Count(kiln, "clay_cup"), Is.EqualTo(4));
        Assert.That(Count(kiln, "clay_bowl"), Is.EqualTo(2));
        Assert.That(Count(kiln, "clay_pot"), Is.EqualTo(1));
        Assert.That(Count(kiln, "water_crock"), Is.EqualTo(1));
        Assert.That(Count(kiln, "brick_batch"), Is.EqualTo(1));
        Assert.That(Count(kiln, "ash"), Is.EqualTo(3));
        foreach (string dry in new[] { "dry_clay_cup", "dry_clay_bowl", "dry_clay_pot", "dry_water_crock", "dry_brick_batch", "firewood" })
            Assert.That(Count(kiln, dry), Is.EqualTo(0), dry + " is gone");
        Assert.That((string)pile.GetField("jobItem").GetValue(kiln), Is.Empty);
        Assert.That((float)pile.GetField("jobHours").GetValue(kiln), Is.EqualTo(0f));
        Assert.That(PotteryRules.PhaseOf(false, (int)pile.GetProperty("KilnWaiting").GetValue(kiln), 0, 0), Is.EqualTo(PotteryRules.KilnPhase.Done));
        Assert.That(Static("WellsAndOuthouses", "KilnPhaseOf", kiln), Is.EqualTo(PotteryRules.KilnPhase.Done));
    }

    [Test]
    public void FinishingAKilnThatIsNotFiringDoesNothingAndALoadedPieceThisBuildDoesNotKnowStays()
    {
        object idle = NewPile("Kiln");
        Add(idle, "dry_clay_cup", 2);
        Static("WellsAndOuthouses", "FinishFiring", idle);
        Assert.That(Count(idle, "dry_clay_cup"), Is.EqualTo(2));
        Assert.That(Count(idle, "ash"), Is.EqualTo(0));

        object odd = NewPile("Kiln");
        Add(odd, "dry_clay_cup", 1);
        Add(odd, "mystery_piece", 2);
        odd.GetType().GetField("jobItem").SetValue(odd, PotteryRules.FiringMarker);
        Static("WellsAndOuthouses", "FinishFiring", odd);
        Assert.That(Count(odd, "clay_cup"), Is.EqualTo(1));
        Assert.That(Count(odd, "mystery_piece"), Is.EqualTo(2));
    }

    [Test]
    public void TheLoadReadsBackAsPlainEntriesAndTheSpaceMathsMatches()
    {
        object kiln = NewPile("Kiln");
        Add(kiln, "dry_clay_cup", 3);
        Add(kiln, "dry_water_crock", 2);
        Add(kiln, "firewood", 6);
        var load = (List<PotteryRules.ShelfEntry>)Static("WellsAndOuthouses", "KilnLoadOf", kiln);
        Assert.That(load.Select(e => e.itemId), Is.EquivalentTo(new[] { "dry_clay_cup", "dry_water_crock" }), "the Firewood is not a piece");
        Assert.That(PotteryRules.SpaceUsed(load), Is.EqualTo(7));
        Assert.That(kiln.GetType().GetMethod("KilnSpaceUsed").Invoke(kiln, null), Is.EqualTo(7));
        Assert.That(Static("WellsAndOuthouses", "KilnPiecesIn", kiln), Is.EqualTo(5));
        Assert.That(Static("WellsAndOuthouses", "KilnPhaseOf", kiln), Is.EqualTo(PotteryRules.KilnPhase.Loading));
        Add(kiln, "firewood", 4);
        Assert.That(Static("WellsAndOuthouses", "KilnPhaseOf", kiln), Is.EqualTo(PotteryRules.KilnPhase.Ready));
    }

    // --- Saving ---

    [Test]
    public void ALoadedAndAFiringKilnRoundTripThroughSaveAndLoadWithNoNewField()
    {
        Type pile = Find("WoodPileState");
        object loaded = NewPile("Kiln");
        Add(loaded, "dry_clay_bowl", 3);
        Add(loaded, "dry_water_crock", 1);
        Add(loaded, "firewood", 10);
        object back = JsonUtility.FromJson(JsonUtility.ToJson(loaded), pile);
        Assert.That(pile.GetField("kind").GetValue(back), Is.EqualTo(Kind("Kiln")));
        Assert.That(Count(back, "dry_clay_bowl"), Is.EqualTo(3));
        Assert.That(Count(back, "dry_water_crock"), Is.EqualTo(1));
        Assert.That(Count(back, "firewood"), Is.EqualTo(10));
        Assert.That(Static("WellsAndOuthouses", "KilnPhaseOf", back), Is.EqualTo(PotteryRules.KilnPhase.Ready));

        object firing = NewPile("Kiln");
        Add(firing, "dry_clay_cup", 5);
        pile.GetField("jobItem").SetValue(firing, PotteryRules.FiringMarker);
        pile.GetField("jobHours").SetValue(firing, 13f);
        object firingBack = JsonUtility.FromJson(JsonUtility.ToJson(firing), pile);
        Assert.That((string)pile.GetField("jobItem").GetValue(firingBack), Is.EqualTo("kiln_firing"));
        Assert.That((float)pile.GetField("jobHours").GetValue(firingBack), Is.EqualTo(13f));
        Assert.That((int)pile.GetField("jobCount").GetValue(firingBack), Is.EqualTo(0), "the firing does not use jobCount");
        Assert.That(Static("WellsAndOuthouses", "KilnPhaseOf", firingBack), Is.EqualTo(PotteryRules.KilnPhase.Firing));

        object done = NewPile("Kiln");
        Add(done, "clay_pot", 2);
        Add(done, "ash", 3);
        object doneBack = JsonUtility.FromJson(JsonUtility.ToJson(done), pile);
        Assert.That(Count(doneBack, "clay_pot"), Is.EqualTo(2));
        Assert.That(Static("WellsAndOuthouses", "KilnPhaseOf", doneBack), Is.EqualTo(PotteryRules.KilnPhase.Done));

        string[] fields = pile.GetFields(BindingFlags.Public | BindingFlags.Instance).Select(f => f.Name).ToArray();
        Assert.That(fields.Length, Is.EqualTo(17), "WoodPileState: the Kiln added none; the Leach Hopper stock added hopperWater and hopperBatch");
        FieldInfo version = Find("SaveManager").GetField("SaveVersion", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        Assert.That(version.GetRawConstantValue(), Is.EqualTo(1));
        Assert.That(LedgerSaveData.CurrentVersion, Is.EqualTo(1));
    }

    [Test]
    public void AnOldSaveWithNoKilnKindsLoadsAsBeforeAndAPotterShedIsUnaffected()
    {
        Type pile = Find("WoodPileState");
        object old = JsonUtility.FromJson("{\"id\":4,\"kind\":36,\"jobItem\":\"dry_clay_bowl\",\"jobCount\":2,\"jobHours\":2.0}", pile);
        Assert.That(pile.GetField("kind").GetValue(old), Is.EqualTo(Kind("PotterShed")));
        Assert.That((string)pile.GetField("jobItem").GetValue(old), Is.EqualTo("dry_clay_bowl"));
        Assert.That((int)pile.GetField("jobCount").GetValue(old), Is.EqualTo(2));
        Assert.That((bool)pile.GetProperty("KilnFiring").GetValue(old), Is.True, "the property only means something on a Kiln; WellsAndOuthouses reads it for kind Kiln only");
        string ticks = SourceText("Scripts/Managers/WellsAndOuthouses.cs");
        Assert.That(ticks, Does.Contain("if (kiln.kind != PileKind.Kiln || !kiln.KilnFiring)"));
        Assert.That(ticks, Does.Contain("if (shed.kind != PileKind.PotterShed || string.IsNullOrEmpty(shed.jobItem))"));
    }

    // --- The market ---

    [Test]
    public void TheFiveFiredPiecesSellInTheCraftedGroupAtMikesPrices()
    {
        MarketTable table = MarketCatalog.Table;
        var prices = new Dictionary<string, long> { { "clay_cup", 400 }, { "clay_bowl", 600 }, { "clay_pot", 1000 }, { "water_crock", 1200 }, { "brick_batch", 600 } };
        foreach (KeyValuePair<string, long> pair in prices)
        {
            Assert.That(table.TryGetRow(pair.Key, out MarketRow row), Is.True, pair.Key);
            Assert.That(row.group, Is.EqualTo(MarketGroup.Crafted), pair.Key);
            Assert.That(row.kind, Is.EqualTo(MarketKind.Bought), pair.Key);
            Assert.That(row.baseCents, Is.EqualTo(pair.Value), pair.Key);
            Assert.That(row.pool, Is.EqualTo(pair.Key), pair.Key + " is its own pool");
            Assert.That(table.SellBaseCents(pair.Key), Is.EqualTo(pair.Value), pair.Key);
            Assert.That(table.BuyBaseCents(pair.Key), Is.EqualTo(0L), pair.Key + " is not stocked");
        }
        Assert.That(table.Rows.Count, Is.EqualTo(89));
        Assert.That(table.Stock.Count, Is.EqualTo(22), "Stocked is 22: Iron Ore joined it (Metalworking M1)");
        Assert.That(table.Rows.Count(r => r.group == MarketGroup.Crafted), Is.EqualTo(22));
        MarketGroupRule crafted = table.RuleOf("clay_cup");
        Assert.That(crafted.dropBp, Is.EqualTo(1000), "10% off per unit");
        Assert.That(crafted.floorBp, Is.EqualTo(1000), "floor 0.1");
        Assert.That(crafted.SeasonalChange, Is.False, "100% target in every season");
        Assert.That(new[] { crafted.spring, crafted.summer, crafted.fall, crafted.winter }, Is.All.EqualTo(10000));
        Assert.That(LedgerCategories.ForSale(table.RuleOf("water_crock").group), Is.EqualTo(LedgerCategory.SalesCrafted), "no ledger change");
    }

    [Test]
    public void TheDryPiecesAndSandStillHaveNoRowSoTheSellListOmitsThemAndTheFiredOnesAppear()
    {
        MarketTable table = MarketCatalog.Table;
        foreach (string id in new[] { "dry_clay_cup", "dry_clay_bowl", "dry_clay_pot", "dry_water_crock", "dry_brick_batch", "sand" })
        {
            Assert.That(table.SellBaseCents(id), Is.EqualTo(0L), id);
            Assert.That(table.TryGetRow(id, out _), Is.False, id);
        }
        // The Trading Post's sell list is the carried stacks that pass market.CanSell(itemId), which is SellBaseCents(itemId) > 0.
        Assert.That(SourceText("Scripts/Managers/MarketManager.cs"), Does.Contain("public bool CanSell(string itemId) => table.SellBaseCents(itemId) > 0;"));
        Assert.That(SourceText("Scripts/UI/TradingPostScreen.cs"), Does.Contain("market.CanSell(stack.itemId)"));
        var carried = new[] { "dry_clay_cup", "sand", "clay_cup", "clay_bowl", "clay_pot", "water_crock", "brick_batch", "dry_brick_batch" };
        Assert.That(carried.Where(id => table.SellBaseCents(id) > 0), Is.EqualTo(new[] { "clay_cup", "clay_bowl", "clay_pot", "water_crock", "brick_batch" }));
    }

    [Test]
    public void SellingFiredPiecesDropsTheirPriceTenPercentEachAndEveryRowHasAnItemFile()
    {
        MarketTable table = MarketCatalog.Table;
        var state = new MarketState();
        SaleResult sale = state.Sell(table, "clay_cup", 3);
        Assert.That(sale.firstUnitCents, Is.EqualTo(400L));
        Assert.That(sale.count, Is.EqualTo(3));
        Assert.That(sale.lastUnitCents, Is.LessThan(sale.firstUnitCents));
        Assert.That(sale.newMultiplierBp, Is.EqualTo(7290), "three sales at 0.9 each");

        string marketAsset = SourceText("Resources/Market/MarketData.asset");
        foreach (string id in new[] { "clay_cup", "clay_bowl", "clay_pot", "water_crock", "brick_batch" })
        {
            Assert.That(marketAsset, Does.Contain($"  - itemId: {id}\n    group: 5\n    pool: {id}\n"), id + " is in MarketData.asset by hand");
            Assert.That(File.Exists(Path.Combine(Application.dataPath, "Resources", "Items", id == "clay_pot" ? "Tools" : "Materials", id + ".asset")), Is.True, id);
        }
    }

    // --- The dev keys and the help ---

    [Test]
    public void TheKilnDevKeysAreFreeAndCtrlSemicolonFinishesFiringsToo()
    {
        string debug = SourceText("Scripts/Debug/TimeDebugControls.cs");
        Assert.That(Regex.Matches(debug, @"keyboard\.quoteKey\.").Count, Is.EqualTo(1), "the apostrophe key is read once, here");
        Assert.That(Regex.Matches(debug, @"keyboard\.semicolonKey\.").Count, Is.EqualTo(1));
        Assert.That(debug, Does.Contain("DebugFinishKilns()"));
        Assert.That(debug, Does.Contain("DebugFinishPottery()"));
        foreach (string file in Directory.GetFiles(Path.Combine(Application.dataPath, "Scripts"), "*.cs", SearchOption.AllDirectories).Where(f => !f.EndsWith("TimeDebugControls.cs")))
            Assert.That(File.ReadAllText(file), Does.Not.Contain("quoteKey"), file);
        string actions = File.ReadAllText(Path.Combine(Application.dataPath, "InputSystem_Actions.inputactions"));
        Assert.That(actions, Does.Not.Contain("<Keyboard>/quote").And.Not.Contain("apostrophe"));
    }

    [Test]
    public void TheCraftingScreenHasTheKilnInTheOtherTabAndHelpForBothKinds()
    {
        string crafting = SourceText("Scripts/UI/CraftingScreen.cs");
        Assert.That(crafting, Does.Contain("case PileKind.KilnSite: return BuildCategories[2];"));
        Assert.That(crafting, Does.Contain("case PileKind.KilnSite: return \"an open-air"));
        Assert.That(crafting, Does.Contain("case PileKind.Kiln: return \"fires dry pieces"));
        Assert.That(SourceText("Scripts/Player/PlacementMode.cs"), Does.Contain("case PileKind.KilnSite:"));
        Assert.That(crafting, Does.Not.Contain("they need firing"), "the shed's help no longer says firing does not exist");
    }
}
