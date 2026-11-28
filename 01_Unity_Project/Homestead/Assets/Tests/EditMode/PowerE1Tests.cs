using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

// Slice E1: power (Mike, 2026-11-28: solar, batteries and a generator on a real energy budget; buy the parts and install them or hire it done; run the
// buried cable yourself from the shed to each building; the racks on the ground; lights are the first load; watch it on the phone and at the shed panel).
public class PowerE1Tests
{
    static string SourceText(string relative) => File.ReadAllText(Path.Combine(Application.dataPath, relative)).Replace("\r\n", "\n");
    static Type Find(string name) => AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(name)).FirstOrDefault(t => t != null);

    [Test]
    public void ThePartsAreRealPartsAtRealPrices()
    {
        Assert.That(PowerRules.PanelWatts, Is.EqualTo(400));
        Assert.That(PowerRules.PanelCents, Is.EqualTo(26000L), "$260 a 400 W panel");
        Assert.That(PowerRules.BatteryWh, Is.EqualTo(5120), "a 48 V 100 Ah LiFePO4 rack battery");
        Assert.That(PowerRules.BatteryCents, Is.EqualTo(120000L));
        Assert.That(PowerRules.InverterWatts, Is.EqualTo(6000));
        Assert.That(PowerRules.InverterCents, Is.EqualTo(155000L));
        Assert.That(PowerRules.GeneratorWatts, Is.EqualTo(5000));
        Assert.That(PowerRules.GeneratorCents, Is.EqualTo(120000L));
        Assert.That(PowerRules.CableCentsPerFoot, Is.EqualTo(150L), "$1.50 a foot of direct-burial cable");
        Assert.That(PowerRules.PanelsPerRack, Is.EqualTo(4));
        Assert.That(PowerRules.BatteriesPerShed, Is.EqualTo(8));
    }

    [Test]
    public void ThePanelsFollowTheSunTheSkyAndTheShade()
    {
        Assert.That(PowerRules.SolarWh(4, 1f, 1f, 0f), Is.EqualTo(1280f).Within(0.01f), "four panels at noon in a clear sky, after 80% system losses");
        Assert.That(PowerRules.SolarWh(4, 0.5f, 1f, 0f), Is.EqualTo(320f).Within(0.01f), "the sun's height counts twice");
        Assert.That(PowerRules.SolarWh(4, 1f, 0.5f, 0f), Is.EqualTo(640f).Within(0.01f), "cloud");
        Assert.That(PowerRules.SolarWh(4, 1f, 1f, 0.3f), Is.EqualTo(896f).Within(0.01f), "shade");
        Assert.That(PowerRules.SolarWh(0, 1f, 1f, 0f), Is.EqualTo(0f));
        Assert.That(PowerRules.SolarWh(4, 0f, 1f, 0f), Is.EqualTo(0f), "night");
        Assert.That(PowerRules.Shade(0), Is.EqualTo(0f));
        Assert.That(PowerRules.Shade(2), Is.EqualTo(0.3f).Within(0.0001f));
        Assert.That(PowerRules.Shade(20), Is.EqualTo(PowerRules.MaxShade), "never fully dark");
    }

    [Test]
    public void TheBatteriesCarryTheLoadAndKeepTheirFloor()
    {
        float cap = PowerRules.CapacityWh(2);
        Assert.That(cap, Is.EqualTo(10240f));
        var h = PowerRules.Hour(5000f, 2, true, 0f, 200f, false, false);
        Assert.That(h.powered, Is.True);
        Assert.That(h.chargeWh, Is.EqualTo(4800f).Within(0.01f));
        h = PowerRules.Hour(5000f, 2, true, 1000f, 200f, false, false);
        Assert.That(h.chargeWh, Is.EqualTo(5800f).Within(0.01f), "the sun charges what the loads leave");
        h = PowerRules.Hour(10000f, 2, true, 1000f, 200f, false, false);
        Assert.That(h.chargeWh, Is.EqualTo(cap), "full");
        Assert.That(h.spillWh, Is.EqualTo(560f).Within(0.01f), "and the rest is spilled");
        h = PowerRules.Hour(1100f, 2, true, 0f, 200f, false, false);
        Assert.That(h.powered, Is.False, "below the 10% floor the lights go out");
        Assert.That(h.problem, Is.EqualTo(PowerRules.BatteriesLow));
        Assert.That(h.chargeWh, Is.EqualTo(1100f).Within(0.01f), "and the batteries are kept");
    }

    [Test]
    public void NoInverterOrTooMuchLoadMeansNoPower()
    {
        var h = PowerRules.Hour(5000f, 2, false, 1000f, 100f, false, true);
        Assert.That(h.powered, Is.False);
        Assert.That(h.problem, Is.EqualTo(PowerRules.NoInverter));
        h = PowerRules.Hour(5000f, 2, true, 0f, 7000f, false, true);
        Assert.That(h.powered, Is.False);
        Assert.That(h.problem, Does.StartWith("Overloaded (7000 W"));
    }

    [Test]
    public void TheGeneratorComesOnLowAndGoesOffFull()
    {
        float cap = PowerRules.CapacityWh(2);
        var h = PowerRules.Hour(cap * 0.15f, 2, true, 0f, 500f, false, true);
        Assert.That(h.generatorOn, Is.True, "on below 20%");
        Assert.That(h.generatorWh, Is.EqualTo(3500f).Within(0.01f), "the loads plus the charger's 3,000 W");
        Assert.That(h.gallons, Is.EqualTo(PowerRules.GallonsPerHour(0.7f)).Within(0.0001f));
        h = PowerRules.Hour(cap * 0.5f, 2, true, 0f, 500f, true, true);
        Assert.That(h.generatorOn, Is.True, "it keeps running until 80%");
        h = PowerRules.Hour(cap * 0.85f, 2, true, 0f, 500f, true, true);
        Assert.That(h.generatorOn, Is.False, "off above 80%");
        h = PowerRules.Hour(cap * 0.15f, 2, true, 0f, 500f, false, false);
        Assert.That(h.generatorOn, Is.False, "not without gasoline");
        Assert.That(PowerRules.GallonsPerHour(0f), Is.EqualTo(0.25f).Within(0.0001f));
        Assert.That(PowerRules.GallonsPerHour(1f), Is.EqualTo(0.85f).Within(0.0001f));
    }

    [Test]
    public void LightsComeOnAtDuskAndBeforeDawn()
    {
        Assert.That(PowerRules.LightsOn(19f, 6.5f, 18.5f), Is.True);
        Assert.That(PowerRules.LightsOn(23f, 6.5f, 18.5f), Is.False, "off for the night at 10:30");
        Assert.That(PowerRules.LightsOn(5.75f, 6.5f, 18.5f), Is.True, "on again at 5:30");
        Assert.That(PowerRules.LightsOn(12f, 6.5f, 18.5f), Is.False);
    }

    [Test]
    public void CableIsSoldByTheFoot()
    {
        Assert.That(PowerRules.CableFeet(10f), Is.EqualTo(33));
        Assert.That(PowerRules.CableFeet(0f), Is.EqualTo(0));
        Assert.That(PowerRules.CableNeeded(33), Is.EqualTo("33 ft of Buried Cable"));
        Assert.That(PowerRules.Percent(5120f, 2), Is.EqualTo("50%"));
        Assert.That(PowerRules.Kwh(5120f), Is.EqualTo("5.1 kWh"));
    }

    [Test]
    public void TheThreeStructuresAreAppendedAndBuildable()
    {
        var kind = Find("PileKind");
        string[] names = Enum.GetNames(kind);
        Assert.That(names.Skip(97).ToArray(), Is.EqualTo(new[] { "PowerShed", "SolarRack", "Generator" }));
        string wood = SourceText("Scripts/Managers/WoodManager.cs");
        Assert.That(wood, Does.Contain("PileKind.DrilledWellSite, PileKind.PowerShed, PileKind.SolarRack, PileKind.Generator,"), "on the Build tab");
        string ps = SourceText("Scripts/World/PowerStructures.cs");
        Assert.That(ps, Does.Contain("\"Panel_\" + i"), "the rack shows as many panels as are mounted");
    }

    [Test]
    public void TheFiveItemsAreSoldAtTheTradingPost()
    {
        foreach (string id in new[] { PowerRules.PanelId, PowerRules.BatteryId, PowerRules.InverterId, PowerRules.GeneratorId, PowerRules.CableId })
        {
            Assert.That(MarketCatalog.Table.Stock.Any(r => r.itemId == id), Is.True, id + " is stocked");
            Assert.That(LedgerCategories.ForPurchase(id), Is.EqualTo(LedgerCategory.BuyBuildingMaterials), id + " is a building material in the ledger");
            Assert.That(KnowHowRules.ItemTip(id), Is.Not.Null.And.Not.Empty, id + " has a tip");
            Assert.That(KnowHowRules.ItemTip(id).Length, Is.LessThanOrEqualTo(80), id + "'s tip fits");
        }
    }

    [Test]
    public void ThePowerKnowHowAndThePhoneApp()
    {
        Assert.That(KnowHowRules.Entries.Last().title, Is.EqualTo("Power: Solar, Batteries and the Generator"));
        Assert.That(PhoneRules.Apps.Select(a => a.label), Does.Contain("Power"));
        Assert.That(File.Exists(Path.Combine(Application.dataPath, "Resources/Phone/app_power.png")), Is.True);
    }

    [Test]
    public void TheArtIsInPlace()
    {
        foreach (string id in new[] { "structure_powershed", "structure_solarrack", "structure_generator",
                                      "item_solar_panel", "item_battery_module", "item_power_inverter", "item_generator", "item_buried_cable" })
            Assert.That(Resources.Load<GameObject>("Art/" + id), Is.Not.Null, id);
        var rack = Resources.Load<GameObject>("Art/structure_solarrack");
        for (int i = 1; i <= 4; i++)
            Assert.That(rack.transform.Find("Panel_" + i), Is.Not.Null, "Panel_" + i);
    }
}
