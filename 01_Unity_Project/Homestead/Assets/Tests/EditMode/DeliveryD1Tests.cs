using System.IO;
using NUnit.Framework;
using UnityEngine;

// Slice D1: delivery from the Trading Post (Mike, 2026-11-27: an order list with Place Order; a Delivery Pad you build; 1 to 3 days by weight;
// kept on the pad like a store with no limit).
public class DeliveryD1Tests
{
    static string SourceText(string relative) => File.ReadAllText(Path.Combine(Application.dataPath, relative)).Replace("\r\n", "\n");

    [Test]
    public void FiftyDollarsADeliveryFreeFromAThousand()
    {
        Assert.That(DeliveryRules.FeeFor(0), Is.EqualTo(0L), "nothing ordered, nothing to pay");
        Assert.That(DeliveryRules.FeeFor(800), Is.EqualTo(5000L));
        Assert.That(DeliveryRules.FeeFor(99999), Is.EqualTo(5000L));
        Assert.That(DeliveryRules.FeeFor(100000), Is.EqualTo(0L), "free at $1,000");
    }

    [Test]
    public void OneToThreeDaysByWeightAtEightInTheMorning()
    {
        Assert.That(DeliveryRules.DaysFor(10f), Is.EqualTo(1));
        Assert.That(DeliveryRules.DaysFor(500f), Is.EqualTo(1));
        Assert.That(DeliveryRules.DaysFor(501f), Is.EqualTo(2));
        Assert.That(DeliveryRules.DaysFor(2000f), Is.EqualTo(2));
        Assert.That(DeliveryRules.DaysFor(2001f), Is.EqualTo(3));
        Assert.That(DeliveryRules.ArrivalHours(10, 100f), Is.EqualTo(11 * 24.0 + 8));
        Assert.That(DeliveryRules.ArrivalHours(10, 3000f), Is.EqualTo(13 * 24.0 + 8));
        Assert.That(DeliveryRules.Due(11 * 24.0 + 8, 11 * 24.0 + 7.9), Is.False);
        Assert.That(DeliveryRules.Due(11 * 24.0 + 8, 11 * 24.0 + 8), Is.True);
    }

    [Test]
    public void TheWordsSayWhatItCostsAndWhenItComes()
    {
        Assert.That(DeliveryRules.Summary(48000, 120f), Is.EqualTo("Goods $480.00  +  delivery $50.00  =  $530.00\n120 kg, arrives tomorrow at 8 a.m.  (free over $1,000.00)"));
        Assert.That(DeliveryRules.Summary(150000, 2500f), Does.StartWith("Goods $1,500.00  +  delivery free  =  $1,500.00\n2500 kg, arrives in 3 days"));
        Assert.That(DeliveryRules.Placed(2), Is.EqualTo("Order placed - it arrives on the Delivery Pad in 2 days at 8 a.m."));
        Assert.That(DeliveryRules.NoPad, Does.Contain("Delivery Pad"));
    }

    [Test]
    public void TheFeeHasItsOwnLedgerLine()
    {
        Assert.That((int)LedgerCategory.DeliveryFees, Is.EqualTo(19), "appended");
        Assert.That(LedgerCategories.Name(LedgerCategory.DeliveryFees), Is.EqualTo("Delivery fees"));
        Assert.That(LedgerCategories.IsIncome(LedgerCategory.DeliveryFees), Is.False);
    }

    [Test]
    public void AnOrderOnItsWayIsSavedWithTheMoney()
    {
        var data = TradingPostSaveData.FromCents(12345);
        data.deliveries = new System.Collections.Generic.List<DeliveryOrder>
        {
            new DeliveryOrder { arriveHours = 32, lines = { new DeliveryLine { itemId = "lumber", count = 60 } } },
        };
        var back = JsonUtility.FromJson<TradingPostSaveData>(JsonUtility.ToJson(data));
        Assert.That(back.ToCents(), Is.EqualTo(12345L));
        Assert.That(back.deliveries.Count, Is.EqualTo(1));
        Assert.That(back.deliveries[0].lines[0].itemId, Is.EqualTo("lumber"));
        Assert.That(back.deliveries[0].lines[0].count, Is.EqualTo(60));
        Assert.That(back.deliveries[0].arriveHours, Is.EqualTo(32.0));
        var old = JsonUtility.FromJson<TradingPostSaveData>("{\"version\":1,\"money\":5,\"cents\":500}");
        Assert.That(old.ToCents(), Is.EqualTo(500L), "an older save loads");
    }

    [Test]
    public void BuildingThePadOpensTheDeliveriesEntry()
    {
        Assert.That(KnowHowRules.ForHappening(KnowHowRules.Happening.DeliveryPadPlaced), Is.EqualTo(new[] { KnowHowRules.Deliveries }));
        Assert.That(KnowHowRules.TryGet(KnowHowRules.Deliveries, out KnowHowRules.Entry entry), Is.True);
        Assert.That(entry.body, Does.Contain("$50").And.Contain("$1,000").And.Contain("8 a.m."));
        Assert.That(SourceText("Scripts/Managers/WoodManager.cs"), Does.Contain("if (kind == PileKind.DeliveryPad) return KnowHowRules.Happening.DeliveryPadPlaced;"));
    }

    [Test]
    public void TheWorldWiresItUp()
    {
        string wood = SourceText("Scripts/Managers/WoodManager.cs");
        Assert.That(wood, Does.Contain("VentOpener, DeliveryPad, PoleBarnSiteSmall"), "appended");
        Assert.That(wood, Does.Contain("case PileKind.DeliveryPad: return !WoodManager.IsWater(itemId);"), "it takes anything but water");
        Assert.That(wood, Does.Contain("if (kind == PileKind.DeliveryPad && DeliveryPad != null)"), "one at a time");
        string post = SourceText("Scripts/Managers/TradingPost.cs");
        Assert.That(post, Does.Contain("if (!HasDropPoint)"), "no pad (and since B1b no Pole Barn), no order");
        Assert.That(post, Does.Contain("LedgerManager.Instance.Record(LedgerCategory.DeliveryFees, fee);"));
        Assert.That(post, Does.Contain(": DeliveryRules.Arrived, 8f);"), "on the pad (since B1b, the barn first)");
        Assert.That(post, Does.Contain(": DeliveryRules.Waiting, 8f);"), "and says so if the pad was taken up meanwhile");
        Assert.That(SourceText("Scripts/World/WoodPile.cs"), Does.Contain("state.kind == PileKind.DeliveryPad || WoodManager.IsBarn(state.kind);"), "a store: E takes, R puts back");
        Assert.That(SourceText("Scripts/UI/TradingPostScreen.cs"), Does.Contain("bool ok = post.PlaceOrder(out string message);"));
        Assert.That(SourceText("Scripts/Managers/GameManager.cs"), Does.Contain("TradingPost.Instance.ResetDeliveries();"));
    }
}
