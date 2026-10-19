using UnityEngine;

// Water_System.md's Rainwater Collection, as pure functions: which Small Cabin roof slope a Rain Barrel stands under, and how
// much rain that slope sends into it. No state and no Unity scene access, so WoodManager owns when it runs and what it
// changes. Litres = slope area (m2) x rain (mm) x efficiency: 1 mm of rain on 1 m2 is 1 litre.
public static class RainCatchment
{
    // Litres one roof slope collects in one hour of rain. The slope is the part of the roof seen from above on one side
    // of the ridge (Shelter.RoofSlopePlanArea, about 11.5 m2).
    public static float LitresPerHour(float rainMmPerHour, float efficiency) =>
        Shelter.RoofSlopePlanArea * Mathf.Max(0f, rainMmPerHour) * Mathf.Clamp01(efficiency);

    // What the player calls a roof slope, as seen standing outside the door looking at it: the cabin's -X side is on the right
    // (the door is at +Z). The same side numbers IsAtEave returns.
    public static string SlopeName(int side) => side < 0 ? "right slope" : "left slope";

    // A point in a cabin's own space (X across the width, Z along the ridge, door at +Z).
    public static Vector3 ToCabinSpace(Vector3 cabinPosition, float cabinYaw, Vector3 world) =>
        Quaternion.Inverse(Quaternion.Euler(0f, cabinYaw, 0f)) * (world - cabinPosition);

    // Whether a spot at the foot of a cabin's long wall is under a slope's eave: outside the roof edge by between
    // minOutside and maxOutside metres (centre of the barrel), and no further along than the roof's ends. Returns which
    // slope: -1 for the left (-X) side, +1 for the right. The door end has no eave.
    public static bool IsAtEave(Vector3 cabinPosition, float cabinYaw, Vector3 spot, float minOutside, float maxOutside, out int side)
    {
        side = 0;
        Vector3 local = ToCabinSpace(cabinPosition, cabinYaw, spot);
        float outside = Mathf.Abs(local.x) - Shelter.RoofEaveX;
        if (outside < minOutside || outside > maxOutside)
            return false;
        if (Mathf.Abs(local.z) > Shelter.RoofPanelDepth / 2f)
            return false;
        if (Mathf.Abs(local.y) > 2f) // the cabin and the spot are on the same ground, give or take a slope
            return false;

        side = local.x < 0f ? -1 : 1;
        return true;
    }
}
