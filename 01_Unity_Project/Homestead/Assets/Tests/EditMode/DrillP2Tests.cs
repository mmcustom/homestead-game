using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

// Slice P2: the Drilled Well (Mike, 2026-11-27: priced by depth at the spot; a big supply that refills; a staked site, hired by phone, the rig in 3 days,
// 2 days to drill, half now and half when done).
public class DrillP2Tests
{
    static string SourceText(string relative) => File.ReadAllText(Path.Combine(Application.dataPath, relative)).Replace("\r\n", "\n");
    static Type Find(string name) => AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(name)).FirstOrDefault(t => t != null);

    [Test]
    public void TheQuoteIsByDepthAtRealOhioPrices()
    {
        Assert.That(DrillRules.Feet(1.5f), Is.EqualTo(92), "low ground by the creek");
        Assert.That(DrillRules.Feet(9f), Is.EqualTo(205));
        Assert.That(DrillRules.Feet(30f), Is.EqualTo(DrillRules.MaxFeet), "never past 250 ft");
        Assert.That(DrillRules.Feet(0f), Is.EqualTo(140), "an unknown depth is the average Ohio well");
        Assert.That(DrillRules.JobCents(1.5f), Is.EqualTo(100000L + 92 * 3000L));
        Assert.That(DrillRules.JobCents(30f), Is.EqualTo(850000L), "about $8,500 at most");
        Assert.That(DrillRules.JobCents(0f), Is.EqualTo(520000L), "the average comes to about the real $5,320");
        Assert.That(DrillRules.Deposit(531000L) + DrillRules.Balance(531000L), Is.EqualTo(531000L), "half and half");
        Assert.That(DrillRules.Quote(9f), Is.EqualTo("$7,150.00 for about 205 ft, half now"));
    }

    [Test]
    public void TheRigComesInThreeDaysAndDrillsForTwo()
    {
        Assert.That(DrillRules.LeadDays, Is.EqualTo(3));
        Assert.That(DrillRules.DrillDays, Is.EqualTo(2));
        Assert.That(DrillRules.Progress(10, 13 * 24.0), Is.EqualTo(0f));
        Assert.That(DrillRules.Progress(10, 14 * 24.0), Is.EqualTo(0.5f).Within(0.001f));
        Assert.That(DrillRules.Progress(10, 15 * 24.0), Is.EqualTo(1f));
        Assert.That(DrillRules.Waiting(1), Is.EqualTo("The rig comes in 1 day"));
    }

    [Test]
    public void ItGivesABigSupplyOfExcellentWaterThatRefills()
    {
        object numbers = Find("WellsAndOuthouses").GetMethod("DrilledNumbers").Invoke(null, new object[] { 9f });
        Assert.That(Field(numbers, "bufferLitres"), Is.EqualTo(1000));
        Assert.That(Field(numbers, "recoveryLitresPerHour"), Is.EqualTo(40f));
        Assert.That(Field(numbers, "droughtRecoveryLitresPerHour"), Is.EqualTo(40f), "drought or not");
        string w = SourceText("Scripts/Managers/WellsAndOuthouses.cs");
        Assert.That(w, Does.Contain("if (well.kind == PileKind.DrilledWell)\n            return WaterQuality.Excellent;"), "flood water and a privy can't reach it");
        Assert.That(w, Does.Contain("if (flood && pile.kind == PileKind.Well &&"), "no flood contamination");
        Assert.That(w, Does.Contain("if (pile.kind != PileKind.Well && pile.kind != PileKind.DrilledWell)"), "it refills hourly");
    }

    static object Field(object o, string name) => o.GetType().GetField(name).GetValue(o);

    [Test]
    public void TheSiteIsBuiltAndTheWellComesFromTheDriller()
    {
        var buildable = ((System.Collections.IEnumerable)Find("WoodManager").GetField("Buildable").GetValue(null)).Cast<object>().Select(o => o.ToString()).ToArray();
        Assert.That(buildable, Does.Contain("DrilledWellSite"));
        Assert.That(buildable, Does.Not.Contain("DrilledWell"));
        string wood = SourceText("Scripts/Managers/WoodManager.cs");
        Assert.That(wood, Does.Contain("PoleBarnLarge, DrilledWellSite, DrilledWell, PowerShed, SolarRack, Generator }"), "appended");
        Assert.That(wood, Does.Contain("if (kind == PileKind.DrilledWellSite && WellsAndOuthouses.Instance != null && WellsAndOuthouses.Instance.WellWaterAt(spot.point, out WellWaterResult deep))"), "its depth is kept for the quote, however deep");
        Assert.That(SourceText("Scripts/Managers/WellsAndOuthouses.cs"), Does.Contain("bool well = kind == PileKind.WellSite || kind == PileKind.Well || kind == PileKind.DrilledWellSite || kind == PileKind.DrilledWell;"), "a well's setbacks");
        string post = SourceText("Scripts/Managers/TradingPost.cs");
        Assert.That(post, Does.Contain("public bool HireDriller(WoodPileState site, out string message)"));
        Assert.That(post, Does.Contain("LedgerManager.Instance.Record(LedgerCategory.Contractors, deposit);"));
        Assert.That(post, Does.Contain("site.kind = PileKind.DrilledWell;"));
        Assert.That(SourceText("Scripts/UI/PhoneView.cs"), Does.Contain("post.HireDriller(target, out string message);"), "hired by phone");
        Assert.That(SourceText("Scripts/World/WateringFittings.cs"), Does.Contain("(pile.kind == PileKind.Well || pile.kind == PileKind.DrilledWell)"), "a garden Hand Pump can stand beside it");
    }

    [Test]
    public void CrewsRememberTheirJob()
    {
        Assert.That(CrewJob.JobBarn, Is.EqualTo(0), "an older save's crews are barn crews");
        Assert.That(CrewJob.JobDrill, Is.EqualTo(1));
        var job = new CrewJob();
        Assert.That(job.job, Is.EqualTo(CrewJob.JobBarn));
    }

    [Test]
    public void ItHasAKnowHowEntryAndArt()
    {
        Assert.That(KnowHowRules.ForHappening(KnowHowRules.Happening.DrilledWellPlaced), Is.EqualTo(new[] { KnowHowRules.DrilledWell }));
        Assert.That(KnowHowRules.Entries.Any(e => e.title == "The Drilled Well"), Is.True);
        Assert.That(Resources.Load<GameObject>("Art/" + DrilledWellVisuals_ArtId), Is.Not.Null);
    }

    const string DrilledWellVisuals_ArtId = "structure_drilledwell";
}
