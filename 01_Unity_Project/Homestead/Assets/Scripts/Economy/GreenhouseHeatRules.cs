using System;

// Greenhouse slice G4: heat (Greenhouses.md 5; Mike's answers 2026-11-26 in section 15). Pure and engine-free so edit-mode tests cover it.
//
// Each walk-in greenhouse has a heater spot and a barrel spot inside (not every size has room for both). In the heater spot goes a Wood Stove or a Propane
// Heater, in the barrel spot a pair of Water Barrels; each is a one-step build with the Hammer, like a Kitchen add-on.
//   Water Barrels: thermal mass, no fuel. They soak up the sun's heat by day (the house runs a little cooler) and give it back at night (a little warmer).
//   Wood Stove: R loads Firewood (up to StoveFirewoodCap), E lights it. Each hour the house would be below StoveTargetC it burns one Firewood and adds its heat;
//     warm hours cost nothing. When the wood is gone it goes out.
//   Propane Heater: R loads Propane Tanks, E turns it on. It keeps the house at PropaneHoldC or above, burning propane only for the warmth it adds; a tank
//     lasts TankHours of full output. When the last tank is empty it stops. (A choosable setting is G6, the Thermostat Controller.)
//   Hotbed: in a Cold Frame's bed, HotbedFertilizer Fertilizer dug in (E) warms the soil HotbedC for HotbedDays, and feeds the bed as Fertilizer does.
// Sizes are passed as 0 small, 1 medium, 2 large; glass says which kind of house.
public static class GreenhouseHeatRules
{
    public enum Heat { Stove, Propane, Barrels }

    // --- Where they go (local metres in the house; -Z is the door end) ---

    // Whether a house of this kind and size has room for it. Stoves and heaters: every Glass Greenhouse and the Medium and Large Hoop Houses; barrels: every
    // Hoop House and the Medium and Large Glass Greenhouses.
    public static bool Fits(bool glass, int size, Heat heat) => heat == Heat.Barrels ? (!glass || size >= 1) : (glass || size >= 1);

    // The heater spot.
    public static bool HeaterSpot(bool glass, int size, out float x, out float z)
    {
        x = z = 0f;
        if (glass)
        {
            if (size == 0) { x = 1.05f; z = 1.4f; }        // the back corner beside the bench
            else if (size == 1) { x = 1.9f; z = -2.6f; }   // the aisle inside the door
            else { x = 2.3f; z = -6.4f; }
            return true;
        }
        if (size == 0)
            return false;
        if (size == 1) { x = 0f; z = 3.3f; }               // on the path near the back
        else { x = 2.3f; z = -7.25f; }                     // the aisle inside the door
        return true;
    }

    // The barrel spot, and whether the pair stands along the house (true) or across it.
    public static bool BarrelSpot(bool glass, int size, out float x, out float z, out bool along)
    {
        x = z = 0f;
        along = false;
        if (glass)
        {
            if (size == 0)
                return false;
            if (size == 1) { x = -1.8f; z = -2.6f; }
            else { x = -2.3f; z = -6.4f; }
            return true;
        }
        if (size == 0) { x = 0f; z = 2.7f; }
        else if (size == 1) { x = 0f; z = 4.3f; along = true; }
        else { x = -2.3f; z = -7.25f; }
        return true;
    }

    // --- What they cost ---

    public const string StoveItemId = "greenhouse_stove";
    public const long StoveStoreCents = 4000;      // a small iron box stove and its pipe, about $40
    public const float StoveKg = 25f;
    public const int StoveStone = 2;                // the hearth it stands on
    public const string HeaterItemId = "propane_heater";
    public const long HeaterStoreCents = 3500;
    public const float HeaterKg = 8f;
    public const string TankId = "propane_tank";
    public const long TankStoreCents = 1200;       // a 20 lb tank, filled
    public const float TankKg = 9f;
    public const string LogsId = "logs";
    public const int BarrelLogs = 4;                // two barrels, coopered (a Water Barrel takes 3 Logs; these need no lid or tap)
    public const string StoneId = "stone";
    public const string FirewoodId = "firewood";

    // --- The Wood Stove ---

    public const int StoveFirewoodCap = 8;
    public const float StoveTargetC = 8f;          // it burns only while the house would be below this

    public static float StoveGainC(int size) => size == 0 ? 18f : size == 1 ? 14f : 10f;

    public static bool StoveBurns(bool lit, int firewood, float insideC) => lit && firewood > 0 && insideC < StoveTargetC;

    // --- The Propane Heater ---

    public const int TankCap = 2;
    public const float PropaneHoldC = 5f;

    public static float PropaneMaxGainC(int size) => size == 0 ? 20f : size == 1 ? 16f : 12f;
    public static float TankHours(int size) => size == 0 ? 30f : size == 1 ? 20f : 12f;

    // The warmth it adds this hour, and the share of an hour's full output that took (what it burns).
    public static float PropaneGainC(float insideC, int size) => Math.Max(0f, Math.Min(PropaneHoldC - insideC, PropaneMaxGainC(size)));
    public static float PropaneBurn(float insideC, int size) => PropaneGainC(insideC, size) / PropaneMaxGainC(size);

    // --- Water Barrels ---

    public const float BarrelNightC = 2.5f;
    public const float BarrelDayC = -2f;

    public static float BarrelEffectC(float sun01) => sun01 < 0.05f ? BarrelNightC : sun01 > 0.5f ? BarrelDayC : 0f;

    // --- Hotbeds ---

    public const int HotbedFertilizer = 2;
    public const float HotbedC = 8f;
    public const int HotbedDays = 28;

    public static bool HotbedActive(int startDay, int today) => today >= startDay && today - startDay < HotbedDays;
    public static int HotbedDaysLeft(int startDay, int today) => Math.Max(0, HotbedDays - (today - startDay));

    // --- Words ---

    public static string StoveHeader(bool lit, bool burning, int firewood) =>
        $"Wood Stove - {(lit ? (burning ? "burning" : "lit, banked (warm enough)") : "out")}, {firewood} Firewood";

    public static string StovePrompt(bool lit, int firewood) =>
        lit ? "Let it go out" : firewood > 0 ? $"Light it  (burns while the greenhouse is below {StoveTargetC:0} C)" : "Load Firewood with R to light it";

    public static string StoveLoadPrompt(int carried, int held) =>
        held >= StoveFirewoodCap ? "" : carried > 0 ? $"Load Firewood  ({held}/{StoveFirewoodCap})" : "";

    public static string HeaterHeader(bool on, bool burning, int tanks, float hoursLeft) =>
        $"Propane Heater - {(on ? (burning ? "heating" : "on, idle (warm enough)") : "off")}; {tanks} full tank{(tanks == 1 ? "" : "s")} and {Math.Round(hoursLeft):0} h in the one fitted";

    public static string HeaterPrompt(bool on, bool fuel) =>
        on ? "Turn it off" : fuel ? $"Turn it on  (keeps the greenhouse at {PropaneHoldC:0} C or above)" : "Load a Propane Tank with R";

    public static string HeaterLoadPrompt(int carried, int held) =>
        held >= TankCap ? "" : carried > 0 ? $"Load a Propane Tank  ({held}/{TankCap})" : "";

    public const string BarrelsHeader = "Water Barrels - they soak up the sun by day and give the warmth back at night";

    public const string StoveOutBanner = "A greenhouse stove has burned all its Firewood and gone out";
    public const string PropaneOutBanner = "A greenhouse heater has run out of propane";

    public const string NoSpot = "Aim at the heater or barrel spot inside a greenhouse.";
    public const string SpotTaken = "Something already stands in that spot.";
    public static string NoRoom(Heat heat) => heat == Heat.Barrels ? "This greenhouse has no room for barrels." : "This greenhouse has no room for a heater.";
    public const string HeatersFirst = "Take the stove, heater, barrels, hose bib and vent opener out first";

    public static string HotbedPrompt(int carried) => $"Dig in a hotbed  ({HotbedFertilizer} Fertilizer: warms the soil about 14 F for 4 weeks; you carry {carried})";
    public static string HotbedLine(int daysLeft) => $"hotbed: warming the soil, {daysLeft} days left";

    public static string Help(Heat heat)
    {
        switch (heat)
        {
            case Heat.Stove:
                return $"a small iron box stove on a stone hearth, for the heater spot inside a Glass Greenhouse or a Medium or Large Hoop House: 1 Greenhouse Stove (Trading Post) and {StoveStone} Stone, built with the Hammer. " +
                       $"R loads Firewood (up to {StoveFirewoodCap}) and E lights it: it burns one an hour, only while the greenhouse would be below {StoveTargetC:0} C, so a full load lasts a cold night.";
            case Heat.Propane:
                return $"a propane heater for the heater spot inside a Glass Greenhouse or a Medium or Large Hoop House: 1 Propane Heater (Trading Post), built with the Hammer. " +
                       $"R loads Propane Tanks (up to {TankCap}, Trading Post) and E turns it on: it keeps the greenhouse at {PropaneHoldC:0} C or above, burning only what that takes.";
            default:
                return $"two barrels of water for the barrel spot inside a Hoop House or a Medium or Large Glass Greenhouse: {BarrelLogs} Logs, built with the Hammer. " +
                       "They soak up the sun's heat by day, so the greenhouse runs a little cooler, and give it back at night, a little warmer, with no fuel.";
        }
    }
}
