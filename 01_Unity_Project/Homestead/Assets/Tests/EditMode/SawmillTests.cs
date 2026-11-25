using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

// The water-powered Sawmill (Saw_Pit_Sawmill_and_Salt_Barrel.md 2; Mike 2026-11-24): on the creek bank, the big build with Lumber and Iron, 2 Logs an hour holding 20,
// stops during a flash flood.
public class SawmillTests
{
    static string SourceText(string relative) => File.ReadAllText(Path.Combine(Application.dataPath, relative)).Replace("\r\n", "\n");

    [Test]
    public void TheSiteTakesMikesBigBuildWithLumberAndIron()
    {
        Assert.That(SawmillRules.SiteLogs, Is.EqualTo(12));
        Assert.That(SawmillRules.SiteLumber, Is.EqualTo(10));
        Assert.That(SawmillRules.SiteIronBars, Is.EqualTo(2));
        Assert.That(SawmillRules.SiteCordage, Is.EqualTo(4));
        Assert.That(SawmillRules.SiteStone, Is.EqualTo(6));
        Assert.That(SawmillRules.BuildBlows, Is.EqualTo(60));
        Assert.That(SawmillRules.IronBarId, Is.EqualTo("iron_bar"));
        Assert.That(SawmillRules.CostText, Is.EqualTo("12 Logs, 10 Lumber, 2 Iron Bars, 4 Cordage and 6 Stone"));
        string w = SourceText("Scripts/Managers/WellsAndOuthouses.cs");
        Assert.That(w, Does.Contain("case PileKind.SawmillSite: return PileKind.Sawmill;"));
        Assert.That(w, Does.Contain("case PileKind.SawmillSite: return SawmillCost();"));
        Assert.That(w, Does.Contain("case PileKind.SawmillSite: return SawmillRules.BuildBlows;"));
        Assert.That(w, Does.Contain("new[] { new[] { SawmillRules.LogsId, SawmillRules.LumberId, SawmillRules.IronBarId, SawmillRules.CordageId, SawmillRules.StoneId } }"), "all five, in any order");
    }

    [Test]
    public void ItSawsTwoLogsIntoThreeLumberAnHourHoldingTwenty()
    {
        Assert.That(SawmillRules.LogsCap, Is.EqualTo(20));
        SawmillRules.AfterHour(20, false, out int used, out int made);
        Assert.That((used, made), Is.EqualTo((2, 3)));
        SawmillRules.AfterHour(1, false, out used, out made);
        Assert.That((used, made), Is.EqualTo((0, 0)), "a single Log waits for a second");
        Assert.That(SawmillRules.HoursLeft(20), Is.EqualTo(10), "a full load is 30 Lumber in 10 hours");
        int logs = 20, lumber = 0;
        for (int h = 0; h < 12; h++)
        {
            SawmillRules.AfterHour(logs, false, out used, out made);
            logs -= used; lumber += made;
        }
        Assert.That((logs, lumber), Is.EqualTo((0, 30)));
        Assert.That(SawmillRules.Accepts("logs", 19), Is.True);
        Assert.That(SawmillRules.Accepts("logs", 20), Is.False);
        Assert.That(SawmillRules.Accepts("lumber", 0), Is.False, "its Lumber is not something to put back");
    }

    [Test]
    public void ItStopsDuringAFlashFloodWithoutDamage()
    {
        SawmillRules.AfterHour(20, true, out int used, out int made);
        Assert.That((used, made), Is.EqualTo((0, 0)));
        Assert.That(SawmillRules.StatusOf(20, true), Is.EqualTo(SawmillRules.Status.Flooded));
        Assert.That(SawmillRules.StatusOf(20, false), Is.EqualTo(SawmillRules.Status.Running));
        Assert.That(SawmillRules.StatusOf(1, false), Is.EqualTo(SawmillRules.Status.WaitingForLogs));
        Assert.That(SawmillRules.Prompt(20, 0, true), Is.EqualTo("Sawmill  (20 Logs, stopped: flash flood)"));
        Assert.That(SourceText("Scripts/Managers/WellsAndOuthouses.cs"), Does.Contain("bool millFlood = StreamManager.State == StreamState.FlashFlood;"));
    }

    [Test]
    public void ItStandsOnTheCreekBankWithinFourMetresOfTheWater()
    {
        Assert.That(SawmillRules.FitToCreek(-0.5f), Is.EqualTo(SawmillRules.CreekFit.InTheCreek));
        Assert.That(SawmillRules.FitToCreek(0.9f), Is.EqualTo(SawmillRules.CreekFit.InTheCreek));
        Assert.That(SawmillRules.FitToCreek(1.0f), Is.EqualTo(SawmillRules.CreekFit.Ok));
        Assert.That(SawmillRules.FitToCreek(4.0f), Is.EqualTo(SawmillRules.CreekFit.Ok));
        Assert.That(SawmillRules.FitToCreek(4.1f), Is.EqualTo(SawmillRules.CreekFit.TooFar));
        Assert.That(SawmillRules.CreekReason(SawmillRules.CreekFit.TooFar, 7f), Does.StartWith("Too far from the creek (7 m)"));
        string w = SourceText("Scripts/Managers/WellsAndOuthouses.cs");
        int mill = w.IndexOf("if (kind == PileKind.SawmillSite || kind == PileKind.Sawmill)");
        int floods = w.IndexOf("if (Floods(point))");
        Assert.That(mill, Is.GreaterThan(0));
        Assert.That(mill, Is.LessThan(floods), "the creek bank floods, so the mill's own rule comes before the floodplain test");
    }

    [Test]
    public void TheWheelFacesTheCreekAndTurnsOnlyWhileSawing()
    {
        string wheel = SourceText("Scripts/World/SawmillWheel.cs");
        Assert.That(wheel, Does.Contain("WellsAndOuthouses.TowardCreek(transform.position)"));
        Assert.That(wheel, Does.Contain("Mathf.Round(yaw / 90f) * 90f"), "in quarter turns, so the wheel sits on a wall");
        string ps = SourceText("Scripts/World/PlacedStructure.cs");
        Assert.That(ps, Does.Contain("side.AddComponent<SawmillWheel>().Bind(state, wheel.transform);"));
        Assert.That(ps, Does.Contain("case PileKind.Sawmill: SawmillInteract(); break;"));
    }

    [Test]
    public void ItIsBuiltFromTheOtherTabAndHasItsKnowHowEntry()
    {
        Assert.That(SourceText("Scripts/Managers/WoodManager.cs"), Does.Contain("PileKind.SawPitSite, PileKind.SawmillSite,"));
        Assert.That(SourceText("Scripts/UI/CraftingScreen.cs"), Does.Contain("case PileKind.SawmillSite:\n            case PileKind.GardenBed:\n            case PileKind.HideFrame:"));
        Assert.That(KnowHowRules.Entries.Select(e => e.id).ElementAt(22), Is.EqualTo("sawmill"));
        Assert.That(KnowHowRules.Entries[22].title, Is.EqualTo("The Sawmill"));
        Assert.That(KnowHowRules.ForHappening(KnowHowRules.Happening.SawmillSitePlaced), Is.EqualTo(new[] { "sawmill" }));
        string journal = File.ReadAllText(Path.Combine(Application.dataPath, "../../../05_Documentation/Game_Systems/Journal_KnowHow_Text.md")).Replace("\r\n", "\n");
        foreach (string para in KnowHowRules.Entries[22].body.Split(new[] { "\n\n" }, System.StringSplitOptions.None))
            Assert.That(journal, Does.Contain(para));
    }

    // The art (structure_sawmill): its WheelSide/Wheel gets the same SawmillWheel the primitive mill's wheel has, so the art's wheel faces the creek and turns too.
    [Test]
    public void TheArtWheelIsBoundLikeThePrimitiveOne()
    {
        string art = SourceText("Scripts/World/ArtSwap.cs");
        Assert.That(art, Does.Contain("id = \"structure_sawmill\"").And.Contain("kind = PileKind.Sawmill,"));
        Assert.That(art, Does.Contain("if (state.kind == PileKind.Sawmill)\n            BindSawmillWheel(art, state);"));
        Assert.That(art, Does.Contain("Named(art, \"WheelSide\")").And.Contain(".Find(\"Wheel\")").And.Contain("spin.Bind(state, wheel);"));
    }
}
