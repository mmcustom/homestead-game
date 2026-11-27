using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

// Tanning slice T5a (Mike, 2026-11-23): five clothing slots, the five pieces made at the Workshop's Leatherwork, their warmth, the Moccasins, and winter colder without them.
public class ClothingTests
{
    static string SourceText(string relative) => File.ReadAllText(Path.Combine(Application.dataPath, relative)).Replace("\r\n", "\n");

    static int Pack(string id, (string id, int n)[] have)
    {
        foreach (var h in have)
            if (h.id == id)
                return h.n;
        return 0;
    }

    [Test]
    public void ThereAreFiveSlotsInMikesOrder()
    {
        Assert.That(ClothingRules.SlotCount, Is.EqualTo(5));
        Assert.That(System.Enum.GetNames(typeof(ClothingRules.Slot)), Is.EqualTo(new[] { "Head", "UpperBody", "LowerBody", "Hands", "Feet" }));
        Assert.That(Enumerable.Range(0, 5).Select(i => ClothingRules.SlotName((ClothingRules.Slot)i)), Is.EqualTo(new[] { "Head", "Upper Body", "Lower Body", "Hands", "Feet" }));
    }

    [Test]
    public void TheFivePiecesFillTheFiveSlotsWithThePlannedNumbers()
    {
        var p = ClothingRules.Wearables;
        Assert.That(p.Length, Is.EqualTo(5));
        Assert.That(p.Select(x => x.itemId), Is.EqualTo(new[] { "buckskin_coat", "buckskin_leggings", "fur_hat", "fur_mittens", "moccasins" }));
        Assert.That(p.Select(x => x.slot), Is.EqualTo(new[] { ClothingRules.Slot.UpperBody, ClothingRules.Slot.LowerBody, ClothingRules.Slot.Head, ClothingRules.Slot.Hands, ClothingRules.Slot.Feet }));
        Assert.That(p.Select(x => x.clo), Is.EqualTo(new[] { 0.5f, 0.3f, 0.2f, 0.15f, 0.05f }));
        Assert.That(p.Select(x => x.hours), Is.EqualTo(new[] { 6, 4, 2, 2, 2 }));
        Assert.That(p.Select(x => x.baseCents), Is.EqualTo(new[] { 9000L, 5000L, 1800L, 1600L, 3000L }));
        Assert.That(ClothingRules.NeedsText(p[0]), Is.EqualTo("3 Buckskin, 1 Tanned Furs, 2 Sinew"));
        Assert.That(ClothingRules.NeedsText(p[1]), Is.EqualTo("2 Buckskin, 1 Sinew"));
        Assert.That(ClothingRules.NeedsText(p[2]), Is.EqualTo("2 Tanned Furs, 1 Sinew"));
        Assert.That(ClothingRules.NeedsText(p[3]), Is.EqualTo("2 Tanned Furs, 1 Sinew"));
        Assert.That(ClothingRules.NeedsText(p[4]), Is.EqualTo("1 Buckskin, 1 Sinew"));
    }

    [Test]
    public void AFullSetAddsAboutOnePointTwoCloAndWinterStartsAtOnePointTwo()
    {
        string[] all = ClothingRules.Wearables.Select(x => x.itemId).ToArray();
        Assert.That(ClothingRules.WornClo(all), Is.EqualTo(1.2f).Within(0.001f));
        Assert.That(ClothingRules.WornClo(new string[5]), Is.EqualTo(0f));
        Assert.That(ClothingRules.WornClo(new[] { "fur_hat", "", null, "rock", "" }), Is.EqualTo(0.2f).Within(0.001f), "empty and unknown slots add nothing");
        Assert.That(ClothingRules.WinterEverydayClo, Is.EqualTo(1.2f), "Mike: winter is colder without the clothes (was 2.0)");
        Assert.That(ClothingRules.SummerEverydayClo, Is.EqualTo(0.8f));
        Assert.That(ClothingRules.WinterEverydayClo + ClothingRules.WornClo(all), Is.GreaterThan(2.0f), "a full set is warmer than the old winter clothes");
        Assert.That(SourceText("Scripts/Managers/SurvivalManager.cs"), Does.Contain("float winterClo = ClothingRules.WinterEverydayClo;"));
        Assert.That(SourceText("Scripts/Managers/SurvivalManager.cs"), Does.Contain("InventoryManager.Instance.WornClo"));
        Assert.That(SourceText("Scenes/Bootstrap.unity"), Does.Contain("winterClo: 1.2"));
    }

    [Test]
    public void TheMoccasinsMakeSprintingCheaperAndStepsQuieter()
    {
        Assert.That(ClothingRules.SprintFactor(true), Is.EqualTo(0.8f));
        Assert.That(ClothingRules.SprintFactor(false), Is.EqualTo(1f));
        Assert.That(ClothingRules.DetectionFactor(true), Is.EqualTo(0.8f));
        Assert.That(ClothingRules.DetectionFactor(false), Is.EqualTo(1f));
        string pc = SourceText("Scripts/Player/PlayerController.cs");
        Assert.That(pc, Does.Contain("sprintStaminaPerSecond * feet * dt"));
        Assert.That(pc, Does.Contain("case MovementState.Walking: return walkDetection * feet;"));
        Assert.That(pc, Does.Contain("case MovementState.Sprinting: return sprintDetection * feet;"));
    }

    [Test]
    public void APieceStartsOnlyWithTheKnifeAFreeBenchAndEverythingInThePack()
    {
        ClothingRules.Piece coat = ClothingRules.Pieces[0];
        var enough = new[] { ("buckskin", 3), ("tanned_furs", 1), ("sinew", 2) };
        var shortOf = new[] { ("buckskin", 2), ("tanned_furs", 1), ("sinew", 1) };
        Assert.That(ClothingRules.CanStart(coat, true, false, false, id => Pack(id, enough)), Is.EqualTo(WorkshopRules.Gate.Ok));
        Assert.That(ClothingRules.CanStart(coat, false, false, false, id => Pack(id, enough)), Is.EqualTo(WorkshopRules.Gate.NoTool));
        Assert.That(ClothingRules.CanStart(coat, true, true, false, id => Pack(id, enough)), Is.EqualTo(WorkshopRules.Gate.Busy));
        Assert.That(ClothingRules.CanStart(coat, true, false, true, id => Pack(id, enough)), Is.EqualTo(WorkshopRules.Gate.ProductWaiting));
        Assert.That(ClothingRules.CanStart(coat, true, false, false, id => Pack(id, shortOf)), Is.EqualTo(WorkshopRules.Gate.ShortOfStock));
        Assert.That(ClothingRules.ShortText(coat, id => Pack(id, shortOf)), Is.EqualTo("1 Buckskin, 1 Sinew"));
        Assert.That(ClothingRules.ReasonText(WorkshopRules.Gate.NoTool, coat, default, id => 0, 0f), Is.EqualTo("Carry a Knife to cut and sew leather."));
    }

    [Test]
    public void TheBenchKnowsTheLeatherPiecesAsItsOwnJobsButTheWoodworkListIsUnchanged()
    {
        Assert.That(WorkshopRules.Recipes.Length, Is.EqualTo(4), "the Woodwork tab still lists four");
        Assert.That(WorkshopRules.AllPieces.Length, Is.EqualTo(11), "four woodwork, five clothing, the Lashing and the Pack");
        Assert.That(WorkshopRules.TryFind("moccasins", out WorkshopRules.Recipe m), Is.True);
        Assert.That(m.name, Is.EqualTo("Moccasins"));
        Assert.That(m.hours, Is.EqualTo(2));
        Assert.That(m.logs + m.branches + m.cordage, Is.EqualTo(0), "a leather piece takes nothing from the bench's stock");
        Assert.That(WorkshopRules.IsProduct("buckskin_coat"), Is.True, "so a finished coat is put in the bench when its hours run out");
        string ps = SourceText("Scripts/World/PlacedStructure.cs");
        Assert.That(ps, Does.Contain("foreach (WorkshopRules.Recipe recipe in WorkshopRules.AllPieces)"), "collecting and drawing a finished piece");
        Assert.That(ps, Does.Contain("foreach (WorkshopRules.Recipe piece in WorkshopRules.AllPieces)"), "the dismantle refund");
        Assert.That(ps, Does.Contain("Inventory.RemoveFromPlayer(need.itemId, need.count - fromBench);"), "a leather piece takes what the bench lacks from the pack");
    }

    [Test]
    public void TheLeatherworkScreenKeysAndTheTabs()
    {
        Assert.That(ClothingRules.Resolve(ClothingRules.Key.Digit, 5, 0, 5), Is.EqualTo((ClothingRules.ActionKind.Choose, 4)));
        Assert.That(ClothingRules.Resolve(ClothingRules.Key.Digit, 6, 2, 5), Is.EqualTo((ClothingRules.ActionKind.None, 2)));
        Assert.That(ClothingRules.Resolve(ClothingRules.Key.Up, 0, 0, 5), Is.EqualTo((ClothingRules.ActionKind.Choose, 0)));
        Assert.That(ClothingRules.Resolve(ClothingRules.Key.Down, 0, 4, 5), Is.EqualTo((ClothingRules.ActionKind.Choose, 4)));
        Assert.That(ClothingRules.Resolve(ClothingRules.Key.Enter, 0, 3, 5), Is.EqualTo((ClothingRules.ActionKind.Start, 3)));
        Assert.That(ClothingRules.Resolve(ClothingRules.Key.Woodwork, 0, 3, 5).kind, Is.EqualTo(ClothingRules.ActionKind.Woodwork));
        Assert.That(SourceText("Scripts/UI/WorkshopScreen.cs"), Does.Contain("LeatherworkScreen.Open(target);"));
        Assert.That(SourceText("Scripts/UI/LeatherworkScreen.cs"), Does.Contain("WorkshopScreen.Open(target);"));
        // Large text: nothing under 26 on the Leatherwork screen.
        string ls = SourceText("Scripts/UI/LeatherworkScreen.cs");
        Assert.That(ls, Does.Contain("public const int MinimumSize = 26;"));
        Assert.That(ls, Does.Contain("TitleSize = 36, StatusSize = 28, PackSize = 28, ListNameSize = 32, ListDetailSize = 28, DetailNameSize = 40, BodySize = 28, StartSize = 36,"));
    }

    [Test]
    public void ThePiecesAreClothingItemsSoldInTheCraftedGroup()
    {
        MarketTable table = MarketCatalog.Table;
        foreach (ClothingRules.Piece piece in ClothingRules.Wearables)
        {
            Object item = Resources.Load("Items/Clothing/" + piece.itemId);
            Assert.That(item, Is.Not.Null, piece.itemId + " asset");
            var so = new UnityEditor.SerializedObject(item);
            Assert.That(so.FindProperty("displayName").stringValue, Is.EqualTo(piece.name));
            Assert.That(so.FindProperty("weightKg").floatValue, Is.EqualTo(piece.weightKg));
            var cat = so.FindProperty("category");
            Assert.That(cat.enumNames[cat.enumValueIndex], Is.EqualTo("Clothing"));
            Assert.That(table.TryGetRow(piece.itemId, out MarketRow row), Is.True, piece.itemId);
            Assert.That(row.group, Is.EqualTo(MarketGroup.Crafted));
            Assert.That(row.kind, Is.EqualTo(MarketKind.Stocked), "on the shelf since 2026-11-27");
            Assert.That(row.baseCents, Is.EqualTo(ToolRules.StoreCents(piece.baseCents)), "at 2.5x");
            Assert.That(table.SellBaseCents(piece.itemId), Is.EqualTo(piece.baseCents), "and still bought at its price");
        }
    }

    // --- T5b (Mike, 2026-11-24): Rawhide Lashing and the Leather Pack ---

    [Test]
    public void TheLashingAndThePackAreLeatherworkButNotWorn()
    {
        Assert.That(ClothingRules.Pieces.Length, Is.EqualTo(7));
        Assert.That(ClothingRules.TryFind("rawhide_lashing", out ClothingRules.Piece lashing), Is.True);
        Assert.That(lashing.Makes, Is.EqualTo(4));
        Assert.That(lashing.hours, Is.EqualTo(1));
        Assert.That(ClothingRules.NeedsText(lashing), Is.EqualTo("1 Rawhide"));
        Assert.That(ClothingRules.TryFind("leather_pack", out ClothingRules.Piece pack), Is.True);
        Assert.That(pack.Makes, Is.EqualTo(1));
        Assert.That(pack.hours, Is.EqualTo(4));
        Assert.That(pack.baseCents, Is.EqualTo(7500L));
        Assert.That(ClothingRules.NeedsText(pack), Is.EqualTo("2 Leather, 2 Sinew, 2 Rawhide Lashing"));
        foreach (string id in new[] { "rawhide_lashing", "leather_pack" })
        {
            Assert.That(ClothingRules.IsWearable(id), Is.False, id);
            Assert.That(ClothingRules.SlotOf(id), Is.EqualTo(-1), id);
        }
        Assert.That(ClothingRules.WornClo(new[] { "leather_pack", "rawhide_lashing", "", "", "" }), Is.EqualTo(0f));
        Assert.That(WorkshopRules.MakesOf("rawhide_lashing"), Is.EqualTo(4));
        Assert.That(WorkshopRules.MakesOf("chair"), Is.EqualTo(1));
        Assert.That(SourceText("Scripts/Managers/WellsAndOuthouses.cs"), Does.Contain("bench.Add(bench.jobItem, WorkshopRules.MakesOf(bench.jobItem));"));
        Assert.That(ClothingRules.ListLabel(lashing, 0).Split('\n')[1], Is.EqualTo("Makes 4  -  1 h  -  not sold"));
    }

    [Test]
    public void TheLeatherPackGivesTwentyKgInsteadOfThePouchsTen()
    {
        Assert.That(ClothingRules.PackBonusKg, Is.EqualTo(20f));
        Assert.That(ClothingRules.CarryBonusKg(false, 10f, false), Is.EqualTo(0f));
        Assert.That(ClothingRules.CarryBonusKg(true, 10f, false), Is.EqualTo(10f));
        Assert.That(ClothingRules.CarryBonusKg(false, 10f, true), Is.EqualTo(20f));
        Assert.That(ClothingRules.CarryBonusKg(true, 10f, true), Is.EqualTo(20f), "the two do not add up");
        string inv = SourceText("Scripts/Managers/InventoryManager.cs");
        Assert.That(inv, Does.Contain("ClothingRules.CarryBonusKg(Player.Has(PouchId), pouchBonusKg, Player.Has(ClothingRules.PackId))"));
        Assert.That(inv, Does.Contain("|| itemId == ClothingRules.PackId;"), "carried, not held");
        Assert.That(Resources.Load("Items/Tools/leather_pack"), Is.Not.Null);
        Assert.That(MarketCatalog.Table.TryGetRow("leather_pack", out MarketRow row), Is.True);
        Assert.That(row.group, Is.EqualTo(MarketGroup.Crafted));
        Assert.That(row.baseCents, Is.EqualTo(18750L), "on the shelf at 2.5x since 2026-11-27");
        Assert.That(MarketCatalog.Table.SellBaseCents("leather_pack"), Is.EqualTo(7500L), "and still bought at $75");
        Assert.That(MarketCatalog.Table.TryGetRow("rawhide_lashing", out _), Is.False, "the Lashing is not sold");
    }

    [Test]
    public void RawhideLashingCountsAsCordagePlainCordageFirst()
    {
        // InventoryContainer is in the game assembly, which tests reach by reflection.
        System.Type container = System.AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("InventoryContainer")).First(t => t != null);
        System.Reflection.MethodInfo standIn = container.GetMethod("StandInFor");
        Assert.That(standIn.Invoke(null, new object[] { "cordage" }), Is.EqualTo("rawhide_lashing"));
        Assert.That(standIn.Invoke(null, new object[] { "logs" }), Is.EqualTo("peeled_log"));
        Assert.That(standIn.Invoke(null, new object[] { "rope" }), Is.Null);
        Assert.That(Resources.Load("Items/Materials/rawhide_lashing"), Is.Not.Null);
        string ic = SourceText("Scripts/Inventory/InventoryContainer.cs");
        Assert.That(ic, Does.Contain("int total = standIn != null ? Count(standIn) : 0;"));
        Assert.That(ic, Does.Contain("removed += Remove(standIn, quantity - removed);"), "the stand-in is taken only after the plain item runs out");
    }

    [Test]
    public void WornClothingIsSavedAndComesOffWhenItLeavesThePack()
    {
        string inv = SourceText("Scripts/Managers/InventoryManager.cs");
        Assert.That(inv, Does.Contain("public List<string> worn;"));
        Assert.That(inv, Does.Contain("data.worn.Add(id ?? \"\");"));
        Assert.That(inv, Does.Contain("if (worn[i] != null && !Player.Has(worn[i]))"));
        Assert.That(inv, Does.Contain("ClothingRules.SlotOf(id) == i && Player.Has(id)"), "a load keeps only clothing in its own slot that is still carried");
        string screen = SourceText("Scripts/UI/InventoryScreen.cs");
        Assert.That(screen, Does.Contain("Heading(right, \"Wearing\", WornTop);"));
        Assert.That(screen, Does.Contain("ClothingRules.IsWearable(item.Id) ? () => ToggleWear(item)"));
        Assert.That(ClothingRules.WornLine(ClothingRules.Slot.Feet, "Moccasins"), Is.EqualTo("Feet: Moccasins"));
        Assert.That(ClothingRules.WornLine(ClothingRules.Slot.Head, null), Is.EqualTo("Head: -"));
    }
}
