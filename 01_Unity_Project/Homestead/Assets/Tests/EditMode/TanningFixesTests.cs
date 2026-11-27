using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

// Mike's play-test of tanning (2026-11-25): the Tanning Vat is a staged site (dig, stock, hammer), the Draw Knife strips Logs at the Saw Pit instead of
// the Bark Spud's Inventory button, and the Crock Pot is on the Trading Post shelf (Tanning_Hides_and_Furs.md, "Changes after Mike's play-test").
// Game types in Assembly-CSharp are reached by reflection, as in the other tests.
public class TanningFixesTests
{
    static string SourceText(string relative) => File.ReadAllText(Path.Combine(Application.dataPath, relative)).Replace("\r\n", "\n");

    static Type Find(string name) => AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(name)).FirstOrDefault(t => t != null);

    static object Kind(string name) => Enum.Parse(Find("PileKind"), name);

    static object CallStatic(string type, string method, params object[] args) =>
        Find(type).GetMethods(BindingFlags.Public | BindingFlags.Static).First(m => m.Name == method && m.GetParameters().Length == args.Length).Invoke(null, args);

    [Test]
    public void TheTanningVatIsAStagedSiteThatIsDugStockedAndHammered()
    {
        object site = Kind("TanningVatSite");
        Assert.That((bool)CallStatic("WellsAndOuthouses", "IsStagedSite", site), Is.True);
        Assert.That(CallStatic("WellsAndOuthouses", "FinishedKind", site).ToString(), Is.EqualTo("TanningVat"));
        Assert.That((bool)CallStatic("WellsAndOuthouses", "RequiresDig", site), Is.True, "a sunken trough: dug first, like the Saw Pit");
        var cost = ((IEnumerable)CallStatic("WellsAndOuthouses", "FinishedCost", site)).Cast<object>()
            .Select(s => s.GetType().GetField("itemId").GetValue(s) + " " + s.GetType().GetField("count").GetValue(s)).ToArray();
        Assert.That(cost, Is.EqualTo(new[] { "logs 3", "clay 2" }), "the same 3 Logs and 2 Clay as before");
        var buildable = ((IEnumerable)Find("WoodManager").GetField("Buildable", BindingFlags.Public | BindingFlags.Static).GetValue(null)).Cast<object>().Select(k => k.ToString()).ToArray();
        Assert.That(buildable, Does.Contain("TanningVatSite"));
        Assert.That(buildable, Does.Not.Contain("TanningVat"), "no longer built in one click");
        Assert.That(TanningVatRules.DigBlows, Is.EqualTo(10));
        Assert.That(TanningVatRules.BuildBlows, Is.EqualTo(20));
        Assert.That(SourceText("Scripts/Managers/WellsAndOuthouses.cs"), Does.Contain("well.kind == PileKind.TanningVatSite ? TanningVatRules.DigBlows"));
        Assert.That(SourceText("Scripts/Managers/WellsAndOuthouses.cs"), Does.Contain("case PileKind.TanningVatSite: return TanningVatRules.BuildBlows;"));
        Assert.That(SourceText("Scripts/World/PlacedStructure.cs"), Does.Contain("case PileKind.TanningVatSite: BuildTanningVatSite("));
    }

    [Test]
    public void TheDrawKnifeStripsLogsAtTheSawPitAndReplacesTheBarkSpud()
    {
        Assert.That(TanningVatRules.DrawKnifeId, Is.EqualTo("bark_spud"), "the Spud's item id, so a Spud already owned is a Draw Knife");
        Assert.That(SmithyRules.Recipes[5].name, Is.EqualTo("Draw Knife"), "after the iron tools; the Greenhouse Hoop (G2) came after it");
        Assert.That((bool)CallStatic("InventoryManager", "IsCarriedOnly", TanningVatRules.DrawKnifeId), Is.False, "held in hand now");
        Assert.That(ToolWork.IsViewTool(TanningVatRules.DrawKnifeId), Is.True);
        Assert.That(HeldTools.IsTwoHanded(TanningVatRules.DrawKnifeId), Is.True, "a handle in each hand");
        Assert.That(ToolWork.KindFor(TanningVatRules.DrawKnifeId, false), Is.EqualTo(WorkKind.Saw), "pulled back and forth");

        float strokes = 0f;
        bool done = false;
        int pulls = 0;
        while (!done && pulls < 100)
        {
            strokes = SawPitRules.AfterStripStroke(strokes, 1f, out done);
            pulls++;
        }
        Assert.That(pulls, Is.EqualTo(TanningVatRules.StripStrokes), "8 strokes peel a Log");
        Assert.That(SawPitRules.Accepts(SawPitRules.PeeledLogId, 0), Is.True, "Peeled Logs go on the pit to saw");
        Assert.That(SawPitRules.Accepts(SawPitRules.PeeledLogId, SawPitRules.LogsCap), Is.False);
        Assert.That(SawPitRules.ActionE(0, 2), Is.EqualTo("Take the Tanbark"));
        Assert.That(SawPitRules.ActionE(3, 2), Is.EqualTo("Take the Lumber and Tanbark"));
        Assert.That(SawPitRules.Header(1, 0, 0, 0, 0f, 0f, false, true), Does.Contain("Hold click to strip a Log's bark"));

        string axe = SourceText("Scripts/Player/AxeTool.cs");
        Assert.That(axe, Does.Contain("void StripAtPit(PlacedStructure pit, Vector3 point)"));
        Assert.That(axe, Does.Contain("state.Add(SawPitRules.PeeledLogId, 1, 0);"));
        Assert.That(SourceText("Scripts/UI/InventoryScreen.cs"), Does.Not.Contain("AddButton(\"Strip\""), "no Strip button in the Inventory");
    }

    [Test]
    public void TheCrockPotIsOnTheShelfAtThirtyDollarsAndStillSellsForTwelve()
    {
        Assert.That(MarketCatalog.Table.BuyBaseCents(PotteryRules.FiredCrockId), Is.EqualTo(3000L));
        Assert.That(MarketCatalog.Table.SellBaseCents(PotteryRules.FiredCrockId), Is.EqualTo(1200L));
        Assert.That(SourceText("Scripts/Economy/Ledger.cs"), Does.Contain("{ PotteryRules.FiredCrockId, LedgerCategory.BuyCookingWaterFire }"));
    }
}
