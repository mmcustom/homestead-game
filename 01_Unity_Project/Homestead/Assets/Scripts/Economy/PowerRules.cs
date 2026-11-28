using System;
using System.Collections.Generic;

// Slice E1, the power system (Power_System.md; Mike's answers 2026-11-28). Pure and engine-free so edit-mode tests cover it.
//
// A real energy budget, worked out every game hour. Solar Panel Racks make watt-hours by the sun's height and the sky (and less in the shade of trees);
// the Power Shed's battery modules store them; every wired thing draws its real watts; a Generator starts by itself when the batteries run low (burning
// Gasoline) and stops when they are charged again. Everything is bought in pieces at real 2026 prices and installed by the player; things are wired to
// the Power Shed with buried cable, bought by the foot.
public static class PowerRules
{
    // --- The pieces (real 2026 prices; Power_System.md has the sources) ---

    public const string PanelId = "solar_panel", BatteryId = "battery_module", InverterId = "power_inverter", GeneratorId = "generator", CableId = "buried_cable";
    public const int PanelWatts = 400;
    public const long PanelCents = 26000;
    public const float PanelKg = 22f;
    public const int BatteryWh = 5120;
    public const long BatteryCents = 120000;
    public const float BatteryKg = 45f;
    public const int InverterWatts = 6000;
    public const long InverterCents = 155000;
    public const float InverterKg = 20f;
    public const int GeneratorWatts = 5000;
    public const long GeneratorCents = 120000;
    public const float GeneratorKg = 75f;
    public const long CableCentsPerFoot = 150;
    public const float CableKgPerFoot = 0.07f;

    public const int PanelsPerRack = 4;
    public const int BatteriesPerShed = 8;
    public const int GasolineCapacity = 5;     // cans a Generator holds
    public const float GallonsPerCan = 1f;     // WateringRules' Gasoline is a one-gallon can

    // --- The sun ---

    public const float SystemEfficiency = 0.8f;   // wiring, the inverter, dust and an imperfect angle (Power_System.md, step 3)

    // What one rack's panels make in an hour (watt-hours): sunHeight is ColdFrameRules.Sun with a clear sky (0..1, the sun's height through the day at the
    // season's midday height), skyClear the weather (DayNightCycle.SunlightFor), shade 0 (none) to 1 (fully shaded). The height counts twice, for the light's
    // angle on the panel and its path through the air, which brings Ohio's year to about 4 peak sun hours (5.5 in summer, 2 in winter).
    public static float SolarWh(int panels, float sunHeight, float skyClear, float shade) =>
        Math.Max(0, panels) * PanelWatts * SystemEfficiency * Clamp01(sunHeight) * Clamp01(sunHeight) * Clamp01(skyClear) * (1f - Clamp01(shade));

    // Trees within ShadeReach of a rack shade it, ShadePerTree each, up to MaxShade.
    public const float ShadeReachMetres = 12f, ShadePerTree = 0.15f, MaxShade = 0.9f;
    public static float Shade(int treesNear) => Math.Min(MaxShade, Math.Max(0, treesNear) * ShadePerTree);

    // --- The batteries ---

    public const float MinCharge = 0.1f;   // LiFePO4 is kept above 10% (a 90% depth of discharge)
    public const float GeneratorOnBelow = 0.2f, GeneratorOffAbove = 0.8f;
    public const float ChargerWatts = 3000f;   // the inverter's charger: the most the Generator can put into the batteries in an hour, beyond the loads

    public static float CapacityWh(int batteries) => Math.Max(0, batteries) * (float)BatteryWh;

    // --- The generator ---

    // Gallons an hour at a share of its rating (0..1): about 0.25 idling, 0.55 at half load, 0.85 at full (a real 5,000 W portable).
    public static float GallonsPerHour(float loadShare) => 0.25f + 0.6f * Clamp01(loadShare);

    // --- Lights: the first load ---

    // Lights are on from dusk until LightsOffHour, and again from LightsOnMorningHour until sunrise.
    public const float LightsOffHour = 22.5f, LightsOnMorningHour = 5.5f;
    public static bool LightsOn(float hour, float sunrise, float sunset) =>
        (hour >= sunset && hour < LightsOffHour) || (hour >= LightsOnMorningHour && hour < sunrise);

    // --- The hour ---

    // One game hour of the whole system. Returns what happened; charge is updated in place, and generator fuel is reported for the caller to burn.
    public static PowerHour Hour(float chargeWh, int batteries, bool hasInverter, float solarWh, float loadWatts, bool generatorWasOn, bool generatorHasFuel)
    {
        var h = new PowerHour { solarWh = solarWh, loadWh = loadWatts };
        float cap = CapacityWh(batteries);
        float floor = cap * MinCharge;
        if (!hasInverter)
        {
            h.chargeWh = Math.Min(chargeWh, cap);
            h.powered = false;
            h.problem = NoInverter;
            return h;
        }
        if (loadWatts > InverterWatts)
        {
            h.chargeWh = Math.Min(chargeWh, cap);
            h.powered = false;
            h.problem = Overload(loadWatts);
            return h;
        }
        // The generator's own switch: on below 20%, off above 80%, and only with fuel.
        bool genOn = generatorHasFuel && (generatorWasOn ? chargeWh < cap * GeneratorOffAbove : chargeWh < cap * GeneratorOnBelow || cap <= 0f);
        float genWh = 0f;
        if (genOn)
        {
            // It covers what the sun doesn't of the loads, and charges the batteries at up to the charger's rate into the room the sun leaves.
            float room = Math.Max(0f, cap - chargeWh - Math.Max(0f, solarWh - loadWatts));
            genWh = Math.Min(GeneratorWatts, Math.Max(0f, loadWatts - solarWh) + Math.Min(ChargerWatts, room));
        }
        h.generatorOn = genOn;
        h.generatorWh = genWh;
        h.gallons = genOn ? GallonsPerHour(genWh / GeneratorWatts) : 0f;

        float net = solarWh + genWh - loadWatts;
        float next = chargeWh + net;
        if (next >= floor || net >= 0f)
        {
            h.powered = true;
            h.chargeWh = Math.Min(cap, Math.Max(next, Math.Min(chargeWh, floor)));
            h.spillWh = Math.Max(0f, next - cap);
        }
        else
        {
            // Not enough: the loads go off; what came in still charges the batteries.
            h.powered = false;
            h.chargeWh = Math.Min(cap, chargeWh + solarWh + genWh);
            h.problem = BatteriesLow;
        }
        return h;
    }

    // --- Cable ---

    public static int CableFeet(float metres) => (int)Math.Ceiling(Math.Max(0f, metres) * 3.28084f);
    public const float MaxRunMetres = 150f;

    // --- Words ---

    public const string NoInverter = "No inverter - put one in the Power Shed";
    public const string BatteriesLow = "Batteries empty - the lights are out until the sun or the generator charges them";
    public static string Overload(float watts) => $"Overloaded ({watts:0} W, the inverter gives {InverterWatts} W) - unplug something";
    public static string Percent(float chargeWh, int batteries) => batteries <= 0 ? "no batteries" : $"{100f * chargeWh / CapacityWh(batteries):0}%";
    public static string Kwh(float wh) => $"{wh / 1000f:0.0} kWh";
    public static string CableNeeded(int feet) => $"{feet} ft of Buried Cable";

    static float Clamp01(float v) => v < 0f ? 0f : v > 1f ? 1f : v;
}

public struct PowerHour
{
    public float solarWh, generatorWh, loadWh, chargeWh, spillWh, gallons;
    public bool powered, generatorOn;
    public string problem;
}

// A buried cable from the Power Shed to something it powers (saved).
[Serializable]
public class WireRun
{
    public int shedId, targetId;
    public float metres;
}

// What the power system keeps between hours (saved with the world).
[Serializable]
public class PowerState
{
    public float chargeWh;
    public bool generatorOn, powered;
    public float lastSolarWh, lastLoadWh, lastGeneratorWh;
    public float fuelLeft;   // gallons left in the can the Generator is burning (it opens a new can when this runs out)
    public List<WireRun> wires = new List<WireRun>();
}
