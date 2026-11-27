using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

// Slice B1a: the Pole Barn (Mike, 2026-11-27: real parts built in stages, or a crew that starts in 3 days and takes about a week, half paid up
// front and half on completion; a walk-in barn with a storage bay; three sizes).
public class PoleBarnB1Tests
{
    static string SourceText(string relative) => File.ReadAllText(Path.Combine(Application.dataPath, relative)).Replace("\r\n", "\n");

    [Test]
    public void ThreeRealSizesWithAFourTwelveRoof()
    {
        Assert.That(BarnRules.Width(BarnRules.Size.Small), Is.EqualTo(7.32f).Within(0.01f), "24 ft");
        Assert.That(BarnRules.Length(BarnRules.Size.Small), Is.EqualTo(9.75f).Within(0.01f), "32 ft");
        Assert.That(BarnRules.Width(BarnRules.Size.Medium), Is.EqualTo(9.14f).Within(0.01f), "30 ft");
        Assert.That(BarnRules.Length(BarnRules.Size.Medium), Is.EqualTo(12.19f).Within(0.01f), "40 ft");
        Assert.That(BarnRules.Width(BarnRules.Size.Large), Is.EqualTo(12.19f).Within(0.01f), "40 ft");
        Assert.That(BarnRules.Length(BarnRules.Size.Large), Is.EqualTo(18.29f).Within(0.01f), "60 ft");
        Assert.That(BarnRules.EaveHeight(BarnRules.Size.Small), Is.EqualTo(3.05f).Within(0.01f));
        Assert.That(BarnRules.EaveHeight(BarnRules.Size.Large), Is.EqualTo(3.66f).Within(0.01f), "12 ft walls on the big one");
        Assert.That(BarnRules.RidgeRise(BarnRules.Size.Small), Is.EqualTo(7.32f / 2f / 3f).Within(0.001f), "4 in 12");
        Assert.That(BarnRules.DoorWidth(BarnRules.Size.Small), Is.LessThan(BarnRules.Width(BarnRules.Size.Small)));
        Assert.That(BarnRules.IsInside(BarnRules.Size.Small, 0f, 0f), Is.True);
        Assert.That(BarnRules.IsInside(BarnRules.Size.Small, 3.7f, 0f), Is.False, "outside the wall");
    }

    [Test]
    public void StorageIsByWeight()
    {
        Assert.That(BarnRules.CapacityKg(BarnRules.Size.Small), Is.EqualTo(5000f));
        Assert.That(BarnRules.CapacityKg(BarnRules.Size.Medium), Is.EqualTo(10000f));
        Assert.That(BarnRules.CapacityKg(BarnRules.Size.Large), Is.EqualTo(20000f));
        Assert.That(BarnRules.UnitsThatFit(BarnRules.Size.Small, 4990f, 5f), Is.EqualTo(2));
        Assert.That(BarnRules.UnitsThatFit(BarnRules.Size.Small, 5000f, 5f), Is.EqualTo(0));
        Assert.That(BarnRules.KgText(BarnRules.Size.Small, 1234.4f), Is.EqualTo("1234 of 5000 kg"));
    }

    [Test]
    public void TheKitIsRealPartsPricedLikeTheMidwest()
    {
        Assert.That(BarnRules.Posts(BarnRules.Size.Small), Is.EqualTo(10));
        Assert.That(BarnRules.Trusses(BarnRules.Size.Medium), Is.EqualTo(11));
        Assert.That(BarnRules.Sheets(BarnRules.Size.Large), Is.EqualTo(107));
        Assert.That(BarnRules.KitKg(BarnRules.Size.Small), Is.EqualTo(10 * 40f + 9 * 70f + 60 * 15f));
        Assert.That(BarnRules.KitCents(BarnRules.Size.Small), Is.EqualTo(750000L));
        Assert.That(BarnRules.CrewCents(BarnRules.Size.Large), Is.EqualTo(3600000L));
        Assert.That(BarnRules.CrewDeposit(BarnRules.Size.Medium) + BarnRules.CrewBalance(BarnRules.Size.Medium), Is.EqualTo(BarnRules.CrewCents(BarnRules.Size.Medium)), "half and half");
        Assert.That(DeliveryRules.FeeFor(BarnRules.KitCents(BarnRules.Size.Small)), Is.EqualTo(0L), "the kit ships free (over $1,000)");
        foreach (BarnRules.Size s in Enum.GetValues(typeof(BarnRules.Size)))
            Assert.That(BarnRules.CrewCents(s), Is.GreaterThan(BarnRules.KitCents(s)), "the crew costs more than the kit");
    }

    [Test]
    public void ThePartsAreItems()
    {
        foreach (var (id, kg) in new[] { (BarnRules.PostId, 40f), (BarnRules.TrussId, 70f), (BarnRules.SheetId, 15f) })
        {
            ScriptableObject item = Resources.Load<ScriptableObject>("Items/Materials/" + id);
            Assert.That(item, Is.Not.Null, id);
            Assert.That((float)item.GetType().GetField("weightKg", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(item), Is.EqualTo(kg), id);
            Assert.That(KnowHowRules.ItemTip(id), Does.Contain("Pole Barn kit"), id);
            Assert.That(KnowHowRules.ItemTip(id).Length, Is.LessThanOrEqualTo(80), id);
        }
    }

    [Test]
    public void ThreeHammeredStagesInOrder()
    {
        Assert.That(BarnRules.Stages, Is.EqualTo(3));
        Assert.That(BarnRules.StageParts, Is.EqualTo(new[] { "barn_post", "barn_truss", "steel_sheet" }));
        Assert.That(BarnRules.StageCount(BarnRules.Size.Small, 0), Is.EqualTo(10));
        Assert.That(BarnRules.StageCount(BarnRules.Size.Small, 2), Is.EqualTo(60));
        Assert.That(BarnRules.StageBlows(BarnRules.Size.Small, 0), Is.EqualTo(80));
        Assert.That(BarnRules.StageBlows(BarnRules.Size.Small, 1), Is.EqualTo(90));
        Assert.That(BarnRules.StageBlows(BarnRules.Size.Small, 2), Is.EqualTo(120));
        Assert.That(BarnRules.StageDone(0), Does.Contain("trusses"));
        Assert.That(BarnRules.StageDone(1), Does.Contain("roof and walls"));
    }

    [Test]
    public void TheCrewStartsInThreeDaysAndTakesAboutAWeek()
    {
        Assert.That(BarnRules.LeadDays, Is.EqualTo(3));
        Assert.That(BarnRules.CrewDays(BarnRules.Size.Small), Is.EqualTo(5));
        Assert.That(BarnRules.CrewDays(BarnRules.Size.Medium), Is.EqualTo(7));
        Assert.That(BarnRules.CrewDays(BarnRules.Size.Large), Is.EqualTo(10));
        Assert.That(BarnRules.CrewProgress(BarnRules.Size.Small, 10, 13 * 24.0), Is.EqualTo(0f), "not started yet");
        Assert.That(BarnRules.CrewProgress(BarnRules.Size.Small, 10, 13 * 24.0 + 60.0), Is.EqualTo(0.5f).Within(0.001f));
        Assert.That(BarnRules.CrewProgress(BarnRules.Size.Small, 10, 18 * 24.0), Is.EqualTo(1f));
        Assert.That(BarnRules.CrewStage(0.1f), Is.EqualTo(0));
        Assert.That(BarnRules.CrewStage(0.5f), Is.EqualTo(1));
        Assert.That(BarnRules.CrewStage(0.9f), Is.EqualTo(2));
        Assert.That(BarnRules.CrewBooked(BarnRules.Size.Small), Does.Contain("$6,500.00"));
        Assert.That(BarnRules.CrewOwed(BarnRules.Size.Small), Does.Contain("as soon as you have it"));
    }

    [Test]
    public void ContractorsHaveTheirOwnLedgerLine()
    {
        Assert.That((int)LedgerCategory.Contractors, Is.EqualTo(20), "appended after Delivery fees");
        Assert.That((int)LedgerCategory.DeliveryFees, Is.EqualTo(19));
    }

    [Test]
    public void TheBarnHasAKnowHowEntry()
    {
        Assert.That(KnowHowRules.Entries.Select(e => e.id), Does.Contain(KnowHowRules.Barn));
        Assert.That(KnowHowRules.ForHappening(KnowHowRules.Happening.BarnPlaced), Is.EqualTo(new[] { KnowHowRules.Barn }));
        Assert.That(SourceText("../../../05_Documentation/Game_Systems/Journal_KnowHow_Text.md"), Does.Contain("The Pole Barn"));
    }

    [Test]
    public void TheKindsAreAppendedAndOnlyTheSitesAreBuilt()
    {
        string wood = SourceText("Scripts/Managers/WoodManager.cs");
        Assert.That(wood, Does.Contain("DeliveryPad, PoleBarnSiteSmall, PoleBarnSiteMedium, PoleBarnSiteLarge, PoleBarnSmall, PoleBarnMedium, PoleBarnLarge }"), "appended");
        var buildable = ((System.Collections.IEnumerable)Find("WoodManager").GetField("Buildable").GetValue(null)).Cast<object>().Select(o => o.ToString()).ToArray();
        Assert.That(buildable, Does.Contain("PoleBarnSiteSmall"));
        Assert.That(buildable, Does.Contain("PoleBarnSiteLarge"));
        Assert.That(buildable, Does.Not.Contain("PoleBarnSmall"), "the barn only comes from finishing the site");
    }

    [Test]
    public void TheTradingPostSellsTheKitAndHiresTheCrew()
    {
        string post = SourceText("Scripts/Managers/TradingPost.cs");
        Assert.That(post, Does.Contain("OrderBarnKit"));
        Assert.That(post, Does.Contain("HireBarnCrew"));
        Assert.That(post, Does.Contain("LedgerCategory.Contractors"));
        Assert.That(SourceText("Scripts/UI/TradingPostScreen.cs"), Does.Contain("BarnRules.KitRow"));
    }

    static Type Find(string name) => AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(name)).FirstOrDefault(t => t != null);
}
