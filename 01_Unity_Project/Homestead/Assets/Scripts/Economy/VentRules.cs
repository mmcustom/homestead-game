using System;

// Greenhouse slice G6: automatic venting (Greenhouses.md 7; Mike's answers 2026-11-27 in section 19). Pure and engine-free so edit-mode tests cover it.
//
// The Vent Opener: a wax cylinder that needs no power. The wax swells as it warms and pushes a rod out, and a spring pulls it back as it cools. It is a
// one-step build with the Hammer into its own spot, like the stove:
//   on a Cold Frame: on the inside of its back wall, under the lid (it props the lid);
//   in a Hoop House: on the inside of the front wall, right of the door (it works the roll-up sides through a winder);
//   in a Glass Greenhouse: the same spot (its rod and chain work the roof vents and the door's top light).
// Every game hour it looks at what the inside would reach shut: above OpenC it opens, below ShutC it shuts, and in between it stays as it was (the gap
// keeps it from flapping open and shut every hour). E at the cover still opens or shuts it by hand, but the opener has its say again the next hour.
// The Thermostat Controller (electric vents and fans, and a heater setting to choose) waits for Power_System.
public static class VentRules
{
    // --- The item ---

    public const string ItemId = "vent_opener";
    public const long StoreCents = 3500;    // a wax-cylinder vent opener, about $35
    public const float Kg = 1.5f;

    // --- When it works ---

    public const float OpenC = 24f;   // about 75 F: it starts to open
    public const float ShutC = 18f;   // about 64 F: it has shut again

    // Whether the cover is open after this hour: from whether it is open now and the inside temperature it would reach shut (sun and heat included).
    public static bool OpenAfter(bool openNow, float shutInsideC) =>
        shutInsideC >= OpenC ? true : shutInsideC <= ShutC ? false : openNow;

    // --- Where it goes (local metres in the host's own frame) ---

    // In a walk-in greenhouse: on the inside of the front wall, right of the door, opposite the Hose Bib (-Z is the door end).
    public const float HouseX = 0.8f, Inset = 0.08f;
    public static (float x, float z) HouseSpot(float houseLength) => (HouseX, -houseLength / 2f + Inset);

    // On a Cold Frame: the middle of the inside of its back wall (+Z is the back).
    public static (float x, float z) FrameSpot() => (0f, ColdFrameRules.Depth / 2f - Inset);

    // --- Words ---

    public const string Name = "Vent Opener";
    public const string NoSpot = "Aim at a cold frame's back wall, or beside a greenhouse's door inside.";
    public const string OpenerFirst = "Take the vent opener off first";

    // What it works on each kind of cover.
    public static string Works(string coverKind) =>
        coverKind == "ColdFrame" ? "the lid" : coverKind == "HoopHouse" ? "the sides" : "the vents";

    public static string Header(string coverKind, bool open) =>
        $"{Name} - opens {Works(coverKind)} above {OpenC:0} C and shuts {(coverKind == "HoopHouse" ? "them" : coverKind == "ColdFrame" ? "it" : "them")} below {ShutC:0} C ({(open ? "open now" : "shut now")})";

    public const string Prompt = "The wax does the work - no power needed";

    public static string Flash(string coverKind, bool open) => coverKind == "ColdFrame"
        ? (open ? "The vent opener propped the lid" : "The vent opener let the lid down")
        : coverKind == "HoopHouse" ? (open ? "The vent opener rolled the sides up" : "The vent opener rolled the sides down")
        : (open ? "The vent opener opened the vents" : "The vent opener shut the vents");

    public static string Help() =>
        $"a wax-cylinder opener that needs no power: it opens a Cold Frame's lid, a Hoop House's sides or a Glass Greenhouse's vents above {OpenC:0} C inside and shuts them below {ShutC:0} C. " +
        "1 Vent Opener (Trading Post), built with the Hammer on the inside of a cold frame's back wall, or beside a greenhouse's door inside.";
}
