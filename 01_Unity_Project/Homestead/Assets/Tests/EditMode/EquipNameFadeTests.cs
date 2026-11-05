using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

// EditMode tests for Hands and tools M0d (Mike, 2026-11-05): the permanent equipped-tool line at the crosshair is gone; the same text now fades in for about 2.5 seconds when a tool is picked up (and
// again when its count changes), then disappears. The timing is pure (Economy assembly, EquipNameFade) and tested directly; ToolStatus, ToolHud and the tool scripts are in the main assembly and
// reached by reflection or by source text. What these cannot show: the real HUD drawing, the font and how it looks (frames were taken in Play Mode instead).
public class EquipNameFadeTests
{
    static Type Find(string name) =>
        AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => { try { return a.GetTypes(); } catch (Exception) { return new Type[0]; } })
                 .First(t => t.Name == name);

    static Type Status => Find("ToolStatus");
    static string Source(string relative) => File.ReadAllText(Path.Combine(Application.dataPath, relative));
    static int Count(string source, string pattern) => Regex.Matches(source, pattern).Count;

    // ---- The timing ----

    [Test]
    public void TheConstantsAreAQuickFadeInAHoldAndASlowerFadeOutAboutTwoAndAHalfSeconds()
    {
        Assert.That(EquipNameFade.FadeInSeconds, Is.EqualTo(0.25f));
        Assert.That(EquipNameFade.HoldSeconds, Is.EqualTo(1.75f));
        Assert.That(EquipNameFade.FadeOutSeconds, Is.EqualTo(0.5f));
        Assert.That(EquipNameFade.TotalSeconds, Is.EqualTo(2.5f).Within(1e-6f));
    }

    [Test]
    public void IdleShowsNothingBeforeAnyToolAndWithEmptyHands()
    {
        var fade = new EquipNameFade();
        Assert.That(fade.Alpha, Is.EqualTo(0f));
        fade.Tick(0.1f, null, -1);
        fade.Tick(5f, null, -1);
        Assert.That(fade.Alpha, Is.EqualTo(0f), "empty hands: no text, ever");
        Assert.That(fade.Pick(null, false, "Axe"), Is.Null);
    }

    [Test]
    public void EquippingShowsTheTextThenFadesToNothingAfterTheTotalTime()
    {
        var fade = new EquipNameFade();
        fade.Tick(0f, "axe", -1);
        Assert.That(fade.Alpha, Is.EqualTo(0f), "starts from nothing");
        fade.Tick(0.125f, "axe", -1);
        Assert.That(fade.Alpha, Is.EqualTo(0.5f).Within(1e-4f), "fading in");
        fade.Tick(0.125f, "axe", -1);
        Assert.That(fade.Alpha, Is.EqualTo(1f).Within(1e-4f), "full at the end of the fade in");
        fade.Tick(1.0f, "axe", -1);
        Assert.That(fade.Alpha, Is.EqualTo(1f), "held");
        Assert.That(fade.Pick("Axe — hold click", true, "Axe"), Is.EqualTo("Axe — hold click"));
        fade.Tick(0.9f, "axe", -1); // 2.15 s: into the fade out
        Assert.That(fade.Alpha, Is.InRange(0.2f, 0.9f));
        fade.Tick(0.4f, "axe", -1);  // 2.525 s: over
        Assert.That(fade.Alpha, Is.EqualTo(0f));
        Assert.That(fade.Pick("Axe — hold click", true, "Axe"), Is.Null, "gone after the total time");
        fade.Tick(100f, "axe", -1);
        Assert.That(fade.Alpha, Is.EqualTo(0f), "and it stays gone");
    }

    [Test]
    public void AChangedCountOnTheSameToolRestartsItButNoCountAndTheSameCountDoNot()
    {
        var fade = new EquipNameFade();
        fade.Tick(0f, "recurve_bow", 10);
        fade.Tick(3f, "recurve_bow", 10);
        Assert.That(fade.Alpha, Is.EqualTo(0f));
        fade.Tick(0.016f, "recurve_bow", 10);
        Assert.That(fade.Alpha, Is.EqualTo(0f), "the same count: nothing");
        fade.Tick(0.016f, "recurve_bow", -1);
        Assert.That(fade.Alpha, Is.EqualTo(0f), "no count reported this frame: nothing, and the count is remembered");
        fade.Tick(0.016f, "recurve_bow", 10);
        Assert.That(fade.Alpha, Is.EqualTo(0f), "back to the same count: nothing");
        fade.Tick(0.1f, "recurve_bow", 9); // an arrow used
        Assert.That(fade.Alpha, Is.GreaterThan(0f), "an arrow was used: the line shows again");
        Assert.That(fade.Age, Is.EqualTo(0.1f).Within(1e-5f));
        fade.Tick(2.0f, "recurve_bow", 9);
        fade.Tick(0.6f, "recurve_bow", 9);
        Assert.That(fade.Alpha, Is.EqualTo(0f), "and it runs the same 2.5 s");
        fade.Tick(0.1f, "recurve_bow", 20); // picked some up
        Assert.That(fade.Alpha, Is.GreaterThan(0f), "arrows picked up: the line shows again");
    }

    [Test]
    public void SwitchingToolsMidFadeRestartsItWithTheNewText()
    {
        var fade = new EquipNameFade();
        fade.Tick(0.3f, "axe", -1);
        fade.Tick(1.2f, "axe", -1);
        float before = fade.Age;
        Assert.That(before, Is.GreaterThan(1f));
        fade.Tick(0.05f, "hammer", -1);
        Assert.That(fade.Age, Is.EqualTo(0.05f).Within(1e-5f), "restarted from nothing");
        Assert.That(fade.ToolKey, Is.EqualTo("hammer"));
        Assert.That(fade.Pick("Hammer - stock a building site", true, "Hammer"), Is.EqualTo("Hammer - stock a building site"), "with the new tool's text");
        fade.Tick(0.5f, null, -1);
        Assert.That(fade.Alpha, Is.EqualTo(0f), "putting the tool away mid-fade: nothing");
        fade.Tick(0.05f, "axe", -1);
        Assert.That(fade.Age, Is.EqualTo(0.05f).Within(1e-5f), "and picking one up again starts it again");
    }

    [Test]
    public void ALoadedGameWithAToolEquippedShowsItOnceAndAMenuPauseDoesNotUseUpTheTime()
    {
        var fade = new EquipNameFade();
        fade.Tick(0.016f, "axe", -1); // the first world frame after a load
        Assert.That(fade.Alpha, Is.GreaterThan(0f));
        // ToolHud ticks only while the world is live: no Tick calls in a menu means the clock does not run
        Assert.That(fade.Age, Is.EqualTo(0.016f).Within(1e-5f));
    }

    [Test]
    public void PickShowsTheIdleLineOrTheNameButNeverOverAPrompt()
    {
        var fade = new EquipNameFade();
        fade.Tick(0.5f, "rod", -1);
        Assert.That(fade.Pick("Rod — aim at water within 20 m and click to cast", true, "Fishing Rod"), Is.EqualTo("Rod — aim at water within 20 m and click to cast"), "the idle line");
        Assert.That(fade.Pick(null, false, "Pouch"), Is.EqualTo("Pouch"), "a tool with no idle line: its name");
        Assert.That(fade.Pick(null, false, null), Is.Null);
        Assert.That(fade.Pick("Waiting for a bite…  (click to reel in)", false, "Fishing Rod"), Is.Null, "a prompt owns the place");
        Assert.That(fade.Pick("Full draw — release to shoot   Arrows: 10", false, "Recurve Bow"), Is.Null, "an in-use line stays on its own");
    }

    // ---- ToolStatus ----

    [Test]
    public void ReportIdleMarksTheLineAsIdleAndAPlainReportClearsTheMark()
    {
        MethodInfo idle = Status.GetMethod("ReportIdle"), report = Status.GetMethod("Report"), countMethod = Status.GetMethod("ReportCount");
        PropertyInfo isIdle = Status.GetProperty("LineIsIdle"), line = Status.GetProperty("Line"), count = Status.GetProperty("Count");
        idle.Invoke(null, new object[] { "Axe — hint", true });
        Assert.That((bool)isIdle.GetValue(null), Is.True);
        Assert.That((string)line.GetValue(null), Is.EqualTo("Axe — hint"), "the same text a Report would have carried");
        report.Invoke(null, new object[] { "Chopping Oak — keep swinging", 0.5f, true, false });
        Assert.That((bool)isIdle.GetValue(null), Is.False, "a prompt is not an idle line");
        Assert.That((string)line.GetValue(null), Is.EqualTo("Chopping Oak — keep swinging"));
        countMethod.Invoke(null, new object[] { 7 });
        Assert.That((int)count.GetValue(null), Is.EqualTo(7));
    }

    // ---- The idle strings are exactly the old ones, and the prompts are untouched ----

    static readonly (string file, string text)[] IdleLines =
    {
        ("Scripts/Player/AxeTool.cs", "ToolStatus.ReportIdle($\"{InventoryManager.Instance.EquippedTool.DisplayName} — hold click on a tree trunk to chop it down, or on a wood pile to split Firewood\")"),
        ("Scripts/Player/AxeTool.cs", "ToolStatus.ReportIdle(pick ? $\"{InventoryManager.Instance.EquippedTool.DisplayName} — hold click on the rock outcrop on South Ridge to mine Stone\" : PotteryRules.ShovelIdleText)"),
        ("Scripts/Player/AxeTool.cs", "ToolStatus.ReportIdle(\"Hammer - stock a building site with its materials, then hold click on it to build it\")"),
        ("Scripts/Fishing/FishingRod.cs", "ToolStatus.ReportIdle($\"{rodName} — aim at water within {range:0} m and click to cast\")"),
        ("Scripts/Managers/TapManager.cs", "ToolStatus.ReportIdle(\"Spile  ·  click on a Sugar Maple to tap it\")"),
        ("Scripts/Player/TrapSetter.cs", "ToolStatus.ReportIdle(type == TrapType.FishTrap ? $\"{name} — aim at water close by\" : $\"{name} — aim at the ground close by\", false)"),
        ("Scripts/Survival/BowDrill.cs", "ToolStatus.ReportIdle(\"Bow Drill — face a built Campfire with fuel in it and hold click to start a fire\")"),
        ("Scripts/Survival/PortableLight.cs", "ToolStatus.ReportIdle(line)"),
        ("Scripts/Wildlife/HuntingWeapon.cs", "ToolStatus.ReportIdle($\"{name} — no arrows\")"),
        ("Scripts/Wildlife/HuntingWeapon.cs", "ToolStatus.ReportIdle($\"{name} — hold to draw, release to shoot   Arrows: {arrows}\")"),
        ("Scripts/Wildlife/HuntingWeapon.cs", "ToolStatus.ReportIdle(\"Rifle — no ammunition\")"),
        ("Scripts/Wildlife/HuntingWeapon.cs", "ToolStatus.ReportIdle($\"Bolt-Action Rifle — right-click to aim, click to fire   Rounds: {rounds}\")"),
    };

    [Test]
    public void EveryIdleLineKeepsItsExactOldWordingButIsNowReportedAsIdle()
    {
        foreach (var (file, text) in IdleLines)
            StringAssert.Contains(text, Source(file), file);
    }

    [Test]
    public void EveryPromptForWhatYouAimAtOrAreDoingStillGoesThroughPlainReportWithItsOldWording()
    {
        string axe = Source("Scripts/Player/AxeTool.cs"), rod = Source("Scripts/Fishing/FishingRod.cs"), weapon = Source("Scripts/Wildlife/HuntingWeapon.cs");
        string drill = Source("Scripts/Survival/BowDrill.cs"), trap = Source("Scripts/Player/TrapSetter.cs"), tap = Source("Scripts/Managers/TapManager.cs"), light = Source("Scripts/Survival/PortableLight.cs");
        // exact counts: every call that was not an idle line is still a plain Report
        Assert.That(Count(axe, @"ToolStatus\.Report\("), Is.EqualTo(16));
        Assert.That(Count(axe, @"ToolStatus\.ReportIdle\("), Is.EqualTo(3));
        Assert.That(Count(rod, @"ToolStatus\.Report\("), Is.EqualTo(6));
        Assert.That(Count(rod, @"ToolStatus\.ReportIdle\("), Is.EqualTo(1));
        Assert.That(Count(weapon, @"ToolStatus\.Report\("), Is.EqualTo(3));
        Assert.That(Count(weapon, @"ToolStatus\.ReportIdle\("), Is.EqualTo(4));
        Assert.That(Count(drill, @"ToolStatus\.Report\("), Is.EqualTo(6));
        Assert.That(Count(drill, @"ToolStatus\.ReportIdle\("), Is.EqualTo(1));
        Assert.That(Count(trap, @"ToolStatus\.Report\("), Is.EqualTo(2));
        Assert.That(Count(trap, @"ToolStatus\.ReportIdle\("), Is.EqualTo(1));
        Assert.That(Count(tap, @"ToolStatus\.Report\("), Is.EqualTo(1));
        Assert.That(Count(tap, @"ToolStatus\.ReportIdle\("), Is.EqualTo(1));
        Assert.That(Count(light, @"ToolStatus\.ReportIdle\("), Is.EqualTo(1));
        // the in-use and target prompts, word for word
        StringAssert.Contains("ToolStatus.Report(draw >= 1f ? $\"Full draw — release to shoot   Arrows: {arrows}\" : $\"Drawing…   Arrows: {arrows}\", draw);", weapon);
        StringAssert.Contains("ToolStatus.Report($\"Working the bolt…   Rounds: {rounds}\", 1f - (readyAt - Time.time) / boltSeconds);", weapon);
        StringAssert.Contains("ToolStatus.Report($\"Rounds: {rounds}\");", weapon);
        StringAssert.Contains("ToolStatus.Report(\"Waiting for a bite…  (click to reel in)\", -1f);", rod);
        StringAssert.Contains("ToolStatus.Report(\"Bite!  Click to set the hook\");", rod);
        StringAssert.Contains("ToolStatus.Report($\"Click to cast  ({(kind == FishWater.Pond ? \"pond\" : \"creek\")})\");", rod);
        StringAssert.Contains("ToolStatus.Report($\"Bow Drill — hold click to drill  ({chance:P0} chance to catch)\");", drill);
        StringAssert.Contains("ToolStatus.Report($\"Place {name} here — {placementWord} spot\", -1f, false);", trap);
        StringAssert.Contains("ToolStatus.Report(PotteryRules.DigText(dig));", axe);
        StringAssert.Contains("ToolStatus.Report(line, -1f, spile);", tap);
        // the Hand Cart and the placement preview are in-use lines and are not touched
        Assert.That(Source("Scripts/Player/CartHandler.cs"), Does.Contain("ToolStatus.Report(CartRules.PushingLine(loadKg));"));
        Assert.That(Source("Scripts/Player/PlacementMode.cs"), Does.Contain("ToolStatus.Report(head + \"\\n\" + keys"));
    }

    [Test]
    public void TheHudDrawsTheFadeInLineAtLeastSize26OnTheStatusLinesPlaceAndOnlyWhileTheWorldIsLive()
    {
        string hud = Source("Scripts/UI/ToolHud.cs");
        Match size = Regex.Match(hud, @"EquipFontSize = (\d+)");
        Assert.That(size.Success, Is.True);
        Assert.That(int.Parse(size.Groups[1].Value), Is.GreaterThanOrEqualTo(26), "Mike has trouble reading small text");
        StringAssert.Contains("rect.anchorMin = status.anchorMin;", hud);
        StringAssert.Contains("rect.anchoredPosition = status.anchoredPosition;", hud);
        StringAssert.Contains("equipLabel.font = statusLabel.font;", hud);
        StringAssert.Contains("Color color = statusLabel.color;", hud);
        StringAssert.Contains("if (playing)\n            equipFade.Tick(Time.unscaledDeltaTime", hud.Replace("\r\n", "\n"));
        StringAssert.Contains("if (ToolStatus.LineIsIdle)", hud);
        // the status line's own size and the other HUD strings are untouched
        StringAssert.Contains("\"Account statement ready: press L\"", hud);
        StringAssert.Contains("statusLabel.verticalOverflow", hud);
        StringAssert.DoesNotContain("SaveVersion", hud);
    }

    [Test]
    public void NoGameBehaviourChangedTheOnlyAdditionsToTheToolScriptsAreCountReportsAndTheIdleMark()
    {
        string weapon = Source("Scripts/Wildlife/HuntingWeapon.cs"), trap = Source("Scripts/Player/TrapSetter.cs");
        Assert.That(Count(weapon, @"ToolStatus\.ReportCount\("), Is.EqualTo(2));
        Assert.That(Count(trap, @"ToolStatus\.ReportCount\("), Is.EqualTo(1));
        StringAssert.Contains("float drawSeconds = 0.8f", weapon);
        StringAssert.Contains("float primitiveDrawSeconds = 1.1f", weapon);
        StringAssert.Contains("float boltSeconds = 1.3f", weapon);
        Assert.That(Directory.Exists(Path.Combine(Application.dataPath, "Resources/Art")), Is.False);
    }
}
