using System;

// Greenhouse slice G5: watering upgrades (Greenhouses.md 6; Mike's answers 2026-11-27 in section 17). Pure and engine-free so edit-mode tests cover it.
//
// Two kinds of fitting, each a one-step build with the Hammer:
//   Hose Bib: a tap and hose fitted on the inside of a greenhouse's front wall, beside the door (it snaps there like the stove). It waters that greenhouse's beds.
//   Hose Reel: a hose on a reel and post, outdoors. It waters the garden beds (not those under cover) within ReelReachMetres.
// E at either waters every one of its beds to soaked at once, drawing the water from its source. With a Drip Kit fitted (E with one in the pack), it also does
// so by itself every morning at DripHour. When it cannot, a banner says so that morning and its prompt says why.
// A source is the nearest of these within SourceReachMetres of the fitting that can give water: a Water Barrel or Rain Barrel (the water in it), a Hand Pump
// (the water pumped into its tank: E pumps PumpStrokeLitres at a time, from the creek, a pond or a Well beside it), or a Fuel Pump (as much as it likes from
// the creek or a pond while it has Gasoline: one can pumps LitresPerCan).
public static class WateringRules
{
    // --- Reach ---

    public const float SourceReachMetres = 15f;   // the hose from the source to the fitting
    public const float ReelReachMetres = 12f;     // a reel's hose from the reel to the beds
    public const float PumpWaterMetres = 5f;      // a pump stands within this of the creek or a pond
    public const float PumpWellMetres = 2.5f;     // ... or of a Well (the Hand Pump only)

    // The Hose Bib's spot in a greenhouse: on the inside of the front wall, left of the door, at hand height (local metres; -Z is the door end).
    public const float BibX = -0.8f, BibInset = 0.08f;
    public static (float x, float z) BibSpot(float houseLength) => (BibX, -houseLength / 2f + BibInset);

    // --- Materials ---

    public const string HoseId = "garden_hose";
    public const long HoseStoreCents = 800;
    public const float HoseKg = 3f;
    public const string DripKitId = "drip_kit";
    public const long DripKitStoreCents = 1500;   // drip lines and a wind-up timer
    public const float DripKitKg = 2f;
    public const string HandPumpId = "hand_pump";
    public const long HandPumpStoreCents = 3000;
    public const float HandPumpKg = 12f;
    public const string FuelPumpId = "fuel_pump";
    public const long FuelPumpStoreCents = 9000;
    public const float FuelPumpKg = 20f;
    public const string GasolineId = "gasoline_can";
    public const long GasolineStoreCents = 300;   // a gallon can
    public const float GasolineKg = 3f;
    public const string LumberId = "lumber";
    public const int ReelLumber = 1, PumpLumber = 2;

    // --- Water ---

    public const int PumpTankLitres = 200;
    public const int PumpStrokeLitres = 20;
    public const int GasolineCap = 4;
    public const int LitresPerCan = 400;
    public const int DripHour = 6;

    // What a bed takes to soak (GardenRules.LitresToSoak) is summed over the beds; a source gives what it can.
    public static int Pumped(int tankLitres) => Math.Min(PumpTankLitres, tankLitres + PumpStrokeLitres) - tankLitres;

    // Litres a Fuel Pump can still give: its cans, and what is left of the can it is burning (credit, in litres).
    public static int FuelPumpLitres(int cans, int creditLitres) => cans * LitresPerCan + creditLitres;

    // --- Words ---

    public static string SourceName(string kindName, float metres) => $"the {kindName} {Math.Round(metres):0} m away";

    public static string WaterPrompt(int beds, int litres, string source) =>
        beds == 0 ? "No beds to water" : litres == 0 ? $"The {beds} bed{(beds == 1 ? " is" : "s are")} soaked" : $"Water the {beds} bed{(beds == 1 ? "" : "s")}  ({litres} L from {source})";

    public const string NoSource = "No water: no barrel or pump within 15 m";
    public static string Empty(string kindName) => kindName == "Fuel Pump" ? "No water: the Fuel Pump is out of Gasoline" : $"No water: the {kindName} is empty";

    public static string FittingHeader(string name, string source, bool drip) =>
        $"{name} - {(source ?? "no water within 15 m")}{(drip ? $"; drip timer waters at {DripHour} a.m." : "")}";

    public const string FitDripPrompt = "Fit the Drip Kit  (waters these beds every morning)";
    public const string DripFitted = "Fitted drip lines and a timer - the beds are watered every morning";

    public static string PumpHeader(int tank) => $"Hand Pump - {tank}/{PumpTankLitres} L in its tank";
    public static string PumpPrompt(int tank) => tank >= PumpTankLitres ? "The tank is full" : $"Pump  (+{PumpStrokeLitres} L)";
    public static string FuelPumpHeader(int cans, int litres) => $"Fuel Pump - {cans} can{(cans == 1 ? "" : "s")} of Gasoline (about {litres:N0} L of water to give)";
    public static string FuelLoadPrompt(int carried, int held) => held >= GasolineCap || carried == 0 ? "" : $"Load Gasoline  ({held}/{GasolineCap})";

    public static string DripFailBanner(string where, string why) => $"Drip lines at {where} went dry this morning - {why}";

    public const string NotNearWater = "A pump stands within 5 m of the creek or a pond.";
    public const string HandPumpNotNearWater = "A Hand Pump stands within 5 m of the creek or a pond, or beside a Well.";

    public static string Help(string kind)
    {
        switch (kind)
        {
            case "HoseBib":
                return "a tap and hose fitted inside a greenhouse, beside its door: 1 Garden Hose (Trading Post), built with the Hammer. E waters every bed in the greenhouse at once from a Water Barrel, Rain Barrel or pump within 15 m. " +
                       "Fit a Drip Kit (E with one in your pack) and it waters them by itself every morning.";
            case "HoseReel":
                return $"a hose on a reel and post, for outdoor garden beds: 1 Garden Hose and {ReelLumber} Lumber, built with the Hammer. E waters every garden bed within {ReelReachMetres:0} m at once from a Water Barrel, Rain Barrel or pump within 15 m. " +
                       "Fit a Drip Kit (E) and it waters them by itself every morning.";
            case "HandPump":
                return $"a cast-iron pitcher pump for the bank of the creek or a pond, or beside a Well: 1 Hand Pump (Trading Post) and {PumpLumber} Lumber, built with the Hammer. E pumps {PumpStrokeLitres} L into its {PumpTankLitres} L tank, which hose fittings within 15 m draw from.";
            default:
                return $"a gasoline engine pump for the bank of the creek or a pond: 1 Fuel Pump (Trading Post) and {PumpLumber} Lumber, built with the Hammer. R loads Gasoline (up to {GasolineCap} cans; one pumps about {LitresPerCan} L). " +
                       "Hose fittings within 15 m draw all the water they want from it while it has fuel, drip lines too.";
        }
    }
}
