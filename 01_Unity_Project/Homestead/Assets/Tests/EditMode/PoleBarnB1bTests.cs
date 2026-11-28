using System.IO;
using NUnit.Framework;
using UnityEngine;

// Slice B1b: the barn feeds builds and crafting within 50 m and takes deliveries (Mike, 2026-11-27: R at a site loads the pack, then the barn;
// a barn is a drop point, so no pad is needed; what doesn't fit goes on the pad; the barn nearest the pad fills first).
public class PoleBarnB1bTests
{
    static string SourceText(string relative) => File.ReadAllText(Path.Combine(Application.dataPath, relative)).Replace("\r\n", "\n");

    [Test]
    public void ABarnFeedsWithinFiftyMetres()
    {
        Assert.That(BarnRules.FeedRadius, Is.EqualTo(50f));
        Assert.That(BarnRules.Feeds(30f, 40f), Is.True, "exactly 50 m");
        Assert.That(BarnRules.Feeds(30f, 40.1f), Is.False);
        Assert.That(BarnRules.Feeds(0f, 0f), Is.True);
    }

    [Test]
    public void TheNearestBarnIsUsedFirst()
    {
        Assert.That(BarnRules.Allocate(10, new[] { 4, 20 }), Is.EqualTo(new[] { 4, 6 }));
        Assert.That(BarnRules.Allocate(10, new[] { 30, 20 }), Is.EqualTo(new[] { 10, 0 }));
        Assert.That(BarnRules.Allocate(10, new[] { 3, 2 }), Is.EqualTo(new[] { 3, 2 }), "short when the barns run out");
        Assert.That(BarnRules.Allocate(0, new[] { 3 }), Is.EqualTo(new[] { 0 }));
    }

    [Test]
    public void TheWords()
    {
        Assert.That(BarnRules.LoadPrompt("4 Logs"), Is.EqualTo("Load from the barn  (4 Logs)"));
        Assert.That(BarnRules.StorePrompt("2 Stone"), Is.EqualTo("Store 2 Stone, then the rest from the barn"));
        Assert.That(BarnRules.FeedNote, Does.Contain("50 m"));
        Assert.That(DeliveryRules.NoPad, Does.Contain("Delivery Pad or a Pole Barn"));
        Assert.That(DeliveryRules.Placed(1, true), Is.EqualTo("Order placed - it arrives at your Pole Barn tomorrow at 8 a.m."));
        Assert.That(DeliveryRules.Placed(1, false), Is.EqualTo(DeliveryRules.Placed(1)));
        Assert.That(BarnRules.SiteHelp(BarnRules.Size.Small), Does.Contain("within 50 m"));
        Assert.That(KnowHowRules.Entries[34].body, Does.Contain("within 50 m of a barn"));
    }

    [Test]
    public void BuildsAndCraftingSpendThePackThenTheBarn()
    {
        string wood = SourceText("Scripts/Managers/WoodManager.cs");
        Assert.That(wood, Does.Contain("shortfalls.Add((c.itemId, c.count - Have(c.itemId)));"), "the Build tab counts the barn");
        Assert.That(wood, Does.Contain("Spend(c.itemId, c.count); // the pack first"), "a one-step build pays from the pack, then the barn");
        Assert.That(wood, Does.Contain("int spent = InventoryManager.Instance.RemoveFromPlayer(itemId, count);"));
        string craft = SourceText("Scripts/Inventory/Crafting.cs");
        Assert.That(craft, Does.Contain("WoodManager.Spend(ingredient.itemId, ingredient.quantity);"));
        Assert.That(craft, Does.Not.Contain("inventory.Player.Count("), "every count goes through WoodManager.Have");
        Assert.That(SourceText("Scripts/UI/CraftingScreen.cs"), Does.Contain("static int Pack(InventoryManager inventory, string itemId) => inventory != null ? WoodManager.Have(itemId) : 0;"));
    }

    [Test]
    public void SitesAreToppedUpFromTheBarnAfterThePack()
    {
        string placed = SourceText("Scripts/World/PlacedStructure.cs");
        Assert.That(placed, Does.Contain("wood.TakeFromBarns(state.position, id, n)"));
        Assert.That(placed.IndexOf("Inventory.Player.RemoveBatches(id, n"), Is.LessThan(placed.IndexOf("wood.TakeFromBarns(state.position, id, n)")), "the pack first");
        Assert.That(placed, Does.Contain("return BarnRules.LoadPrompt(fromBarn);"));
        string pile = SourceText("Scripts/World/WoodPile.cs");
        Assert.That(pile, Does.Contain("stored += TopUpCabinFromBarns();"), "the Small Cabin site too");
    }

    [Test]
    public void DeliveriesGoIntoTheBarnThenThePad()
    {
        string post = SourceText("Scripts/Managers/TradingPost.cs");
        Assert.That(post, Does.Contain("if (!HasDropPoint)"), "a barn is a drop point");
        Assert.That(post, Does.Contain("return wood.BarnsNearestTo(pad.position);"), "the barn nearest the pad first");
        Assert.That(post, Does.Contain("barns.Sort((a, b) => a.id.CompareTo(b.id));"), "with no pad, the first built");
        Assert.That(post.IndexOf("if (IntoBarns(deliveries[i]))"), Is.LessThan(post.IndexOf("WoodPileState pad = WoodManager.Instance.DeliveryPad;\n            if (pad == null)")), "the barn before the pad");
        Assert.That(SourceText("Scripts/UI/TradingPostScreen.cs"), Does.Contain("bool pad = TradingPost.HasDropPoint;"));
    }
}
