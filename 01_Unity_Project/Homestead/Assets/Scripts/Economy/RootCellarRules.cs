using System;

// The Root Cellar (Food_Spoilage_and_Preservation.md section 6, slice 6). Mike, 2026-11-23: dug out with the Shovel first, then built ("Dig, then build"),
// a room you walk into ("Walk inside"), holding 300 kg ("300 kg").
//
// The site is marked out, dug with the Shovel (like the Well's shaft, DigBlows blows), stocked with Stone, Logs and Lumber in any order with R, and
// pounded up with the Hammer. The finished cellar is an earth-covered room at ground level (a bermed cellar: the dug earth is heaped back over its log
// roof), entered through a doorway in its stone front (+Z). Shelves line its back wall; E at them, from inside, opens the storage screen. It takes
// food only, up to CapacityKg by weight, and everything in it ages on the Cellar clock: a steady x 0.3 all year, never freezing (SpoilageRules).
// Pure and engine-free so edit-mode tests cover it.
public static class RootCellarRules
{
    public const string StoneId = "stone";
    public const string LogsId = "logs";
    public const string LumberId = "lumber";

    // --- The site (about 159 kg: 10 Stone at 5 kg, 8 Logs at 8, 4 Lumber at 5), after the digging ---

    public const int SiteStone = 10;
    public const int SiteLogs = 8;
    public const int SiteLumber = 4;
    public const int DigBlows = 30;
    public const int BuildBlows = 40;

    // --- What it holds ---

    public const float CapacityKg = 300f;

    // Whole units of an item weighing unitKg that still fit with loadKg already stored (a tiny tolerance keeps an exact fit).
    public static int UnitsThatFit(float loadKg, float unitKg)
    {
        float room = CapacityKg - Math.Max(0f, loadKg);
        if (room <= 0.0001f)
            return 0;
        if (unitKg <= 0f)
            return int.MaxValue;
        return (int)Math.Floor((room + 0.0001f) / unitKg);
    }

    public static string KgText(float loadKg) => $"{loadKg:0} of {CapacityKg:0} kg";

    // --- The room (local space: +Z is the front, the doorway; the floor is the ground) ---

    public const float OuterHalfWidth = 1.9f;   // the earth mound's base, either side
    public const float OuterBack = -2.4f;       // the mound's back toe
    public const float FrontZ = 1.4f;           // the stone front wall's outer face
    public const float InnerHalfWidth = 1.0f;   // the room, 2 m across
    public const float InnerBack = -1.5f;       // the back wall's inner face
    public const float InnerFront = 1.1f;       // the front wall's inner face
    public const float CeilingHeight = 2.3f;    // under the roof logs
    public const float DoorHalfWidth = 0.55f;   // a 1.1 m doorway, as the Cabin's
    public const float DoorHeight = 2.05f;
    public const float ReachMetres = 2.6f;      // the mound's reach from its centre, for spacing and placement

    // Whether a point (in the cellar's local space) is inside the room or standing in its doorway: the shelves answer only there.
    public static bool IsInside(float x, float z) => Math.Abs(x) <= InnerHalfWidth + 0.05f && z >= InnerBack - 0.05f && z <= FrontZ + 0.35f;

    // --- The words ---

    public static string CostText => $"{SiteStone} Stone, {SiteLogs} Logs and {SiteLumber} Lumber";

    public static string DugBanner => $"The cellar is dug - now deliver {CostText} with R, in any order.";

    public static string BuiltMessage =>
        $"Root Cellar built. Walk in through the door: the shelves at the back hold up to {CapacityKg:0} kg of food (E from inside), and food kept there ages a third as fast, all year, and never freezes.";

    public static string OutsidePrompt => "Root Cellar  (walk in through the door - the shelves are at the back)";

    public static string Full(string itemName) => $"The Root Cellar is full ({CapacityKg:0} kg) - no room for {itemName}.";
}
