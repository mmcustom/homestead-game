using System.Collections.Generic;
using UnityEngine;

public enum SiteKind { Well, Outhouse, Building, AnimalPen }

// Something already standing that a new Well or Outhouse is measured against. position.y is the ground height there.
public struct SiteInfo
{
    public SiteKind kind;
    public Vector3 position;
    public string name; // for the refusal text, e.g. "Small Cabin"
    public float radius; // how far the thing itself reaches from its position (metres); 0 for a point, used by the Covered Kitchen's gap
}

// How far the point is from the edge of a body of water (the creek channel, a pond or spring zone). 0 or less is in it.
public struct WaterEdge
{
    public string name;
    public float distanceMetres;
}

// The real figures turned into game metres (Structures_Slice1_Dry_And_Clean.md). The WellsAndOuthouses component owns the
// tunable fields and copies them in here; every number is first-pass.
public struct SetbackRules
{
    public float privyFromWater;      // leaching pit privy to a water source (30 m)
    public float privyFromWell;       // leaching pit privy to a dug well (30 m)
    public float privyFromBuilding;   // outhouse to a building (7.5 m)
    public float wellFromBuilding;    // well to a building foundation (3 m)
    public float wellFromPen;         // well to animal pens and manure (15 m)
    public float shedFromBuilding;    // wood shed to a building (3 m, fire practice)
    public float kitchenFromStructure; // covered kitchen's roof edge to any other structure (3 m, fire safety)
    public float kitchenFromTree;     // covered kitchen's roof edge to a tree trunk (3 m, fire safety)
    public float kitchenRadius;       // the kitchen roof's reach from its centre, so the gaps are measured from its edge (2.3 m, the roof corner)
    public float upgradientRange;     // a well must be higher than any privy within this distance
    public float hollowDepth;         // a point this much lower than the ground around it is a hollow
    public float floodBand;           // ground within this distance of the channel edge ...
    public float floodRise;           // ... and no higher than the creek surface plus this, floods

    public static SetbackRules Defaults => new SetbackRules
    {
        privyFromWater = 30f, privyFromWell = 30f, privyFromBuilding = 7.5f, wellFromBuilding = 3f, wellFromPen = 15f, shedFromBuilding = 3f, kitchenFromStructure = 3f, kitchenFromTree = 3f, kitchenRadius = 2.3f,
        upgradientRange = 60f, hollowDepth = 0.5f, floodBand = 25f, floodRise = 1.5f,
    };
}

// Structures Slice 1, phase A: where a Well and an Outhouse may stand. Pure maths over positions, ground heights and
// distances, no scene access, so the edit-mode tests can run it. Distances are horizontal (x, z); heights are y.
// Every refusal names its reason in the same plain style as the trap ghost ("Too close to the outhouse: 30 m needed.").
public static class Setbacks
{
    public static float Flat(Vector3 a, Vector3 b) => Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));

    static string Metres(float m) => m.ToString("0.#");

    static string TooClose(string what, float needed) => $"Too close to {what}: {Metres(needed)} m needed.";

    // A Well: 3 m from any building, 30 m from any outhouse, 15 m from any animal pen, and on higher ground than any
    // outhouse within upgradientRange (so what seeps from a pit runs away from the well).
    public static bool CanPlaceWell(Vector3 point, IList<SiteInfo> sites, SetbackRules rules, out string reason)
    {
        foreach (SiteInfo site in sites)
        {
            float d = Flat(point, site.position);
            if (site.kind == SiteKind.Building && d < rules.wellFromBuilding)
            {
                reason = TooClose($"the {site.name}", rules.wellFromBuilding);
                return false;
            }
            if (site.kind == SiteKind.Outhouse && d < rules.privyFromWell)
            {
                reason = TooClose("the outhouse", rules.privyFromWell);
                return false;
            }
            if (site.kind == SiteKind.AnimalPen && d < rules.wellFromPen)
            {
                reason = TooClose("the animal pen", rules.wellFromPen);
                return false;
            }
        }
        foreach (SiteInfo site in sites)
        {
            if (site.kind == SiteKind.Outhouse && Flat(point, site.position) <= rules.upgradientRange && site.position.y > point.y)
            {
                reason = "The outhouse is on higher ground: a well must stand uphill of it.";
                return false;
            }
        }
        reason = "";
        return true;
    }

    // An Outhouse: 30 m from water and from any well, 7.5 m from any building, and not uphill of a well within upgradientRange.
    public static bool CanPlaceOuthouse(Vector3 point, IList<SiteInfo> sites, IList<WaterEdge> water, SetbackRules rules, out string reason)
    {
        foreach (WaterEdge edge in water)
        {
            if (edge.distanceMetres < rules.privyFromWater)
            {
                reason = TooClose(edge.name, rules.privyFromWater);
                return false;
            }
        }
        foreach (SiteInfo site in sites)
        {
            float d = Flat(point, site.position);
            if (site.kind == SiteKind.Well && d < rules.privyFromWell)
            {
                reason = TooClose("the well", rules.privyFromWell);
                return false;
            }
            if (site.kind == SiteKind.Building && d < rules.privyFromBuilding)
            {
                reason = TooClose($"the {site.name}", rules.privyFromBuilding);
                return false;
            }
        }
        foreach (SiteInfo site in sites)
        {
            if (site.kind == SiteKind.Well && Flat(point, site.position) <= rules.upgradientRange && site.position.y < point.y)
            {
                reason = "The well is downhill of here: an outhouse must stand below a well.";
                return false;
            }
        }
        reason = "";
        return true;
    }

    // A Wood Shed: 3 m from any building (a Small Cabin or its site; wood stored against a house is a fire and pest risk).
    public static bool CanPlaceShed(Vector3 point, IList<SiteInfo> sites, SetbackRules rules, out string reason)
    {
        foreach (SiteInfo site in sites)
        {
            if (site.kind == SiteKind.Building && Flat(point, site.position) < rules.shedFromBuilding)
            {
                reason = TooClose($"the {site.name}", rules.shedFromBuilding);
                return false;
            }
        }
        reason = "";
        return true;
    }

    // A Covered Outdoor Kitchen (a fire under a roof): its roof edge keeps kitchenFromStructure clear of every other structure and
    // kitchenFromTree clear of a tree. A structure's own reach (SiteInfo.radius) counts, so the gap is clear air, not centre to centre.
    // treeNear: a standing tree trunk is within kitchenRadius + kitchenFromTree of the point (the caller asks the trees).
    public static bool CanPlaceKitchen(Vector3 point, IList<SiteInfo> structures, bool treeNear, SetbackRules rules, out string reason)
    {
        foreach (SiteInfo site in structures)
        {
            if (Flat(point, site.position) < rules.kitchenRadius + site.radius + rules.kitchenFromStructure)
            {
                reason = TooClose($"the {site.name}", rules.kitchenFromStructure);
                return false;
            }
        }
        if (treeNear)
        {
            reason = TooClose("a tree", rules.kitchenFromTree);
            return false;
        }
        reason = "";
        return true;
    }

    // Covered Outdoor Kitchen layout, in the kitchen's own space (+Z is the open front, +X right). One source for the roof, the hearth
    // and "standing under the roof", so what is drawn and what counts as sheltered never differ.
    public static class KitchenLayout
    {
        public const float HalfWidth = 1.7f;   // roof edge to centre, side to side (x)
        public const float HalfDepth = 1.5f;   // roof edge to centre, front to back (z)
        public const float RoofHeight = 2.45f; // the roof is well clear of the fire
        public static readonly Vector3 HearthLocal = new Vector3(-0.35f, 0.14f, 0.15f);

        // Whether a world point stands under the roof of a kitchen at pivot with the given yaw (degrees), by the roof's footprint.
        public static bool UnderRoof(Vector3 pivot, float yawDegrees, Vector3 point)
        {
            Vector3 local = Quaternion.Euler(0f, -yawDegrees, 0f) * (point - pivot);
            return Mathf.Abs(local.x) <= HalfWidth && Mathf.Abs(local.z) <= HalfDepth;
        }

        public static Vector3 HearthAt(Vector3 pivot, float yawDegrees) => pivot + Quaternion.Euler(0f, yawDegrees, 0f) * HearthLocal;

        // --- Add-on slots (Kitchen add-ons, batch 1) ---
        // Four fixed, generic slots under the roof: two along the +X wall and two along the -X wall, every item facing the
        // middle. Each is SlotHalfWidth * 2 along the wall and SlotHalfDepth * 2 deep, which keeps clear of the posts, the hearth slab
        // (x -1.1 to 0.4, z -0.6 to 0.9) and the prep table (x 0.4 to 1.5, z -1.3 to -0.7), leaves the open front (+Z) and the aisle from it
        // to the hearth free, and puts every slot more than SlotMinHearthGap from the hearth fire so KitchenHearthOf never mistakes a
        // grill's fire for the hearth. A slot is just a position: an add-on stands in one if its position is within SlotTolerance of it.
        public const int SlotCount = 4;
        public const float SlotHalfWidth = 0.44f;
        public const float SlotHalfDepth = 0.25f;
        public const float SlotTolerance = 0.3f;
        public const float SlotMinHearthGap = 0.75f;

        static readonly Vector3[] SlotLocal =
        {
            new Vector3(1.15f, 0f, 0.78f),    // 0: +X wall, front (the player's left, near the entrance)
            new Vector3(1.15f, 0f, -0.2f),    // 1: +X wall, back (the player's left, toward the back; beside the prep table)
            new Vector3(-1.37f, 0f, 0.80f),   // 2: -X wall, front (the player's right, near the entrance)
            new Vector3(-1.37f, 0f, -0.12f),  // 3: -X wall, back (the player's right, toward the back; beside the hearth wall)
        };

        // What the player calls each slot, standing at the open front (+Z) and looking in toward the back (-Z): local +X is then
        // their left. Relative to the kitchen, so it holds however the kitchen is turned.
        static readonly string[] SlotLabels = { "left, near the entrance", "left, toward the back", "right, near the entrance", "right, toward the back" };

        public static string SlotLabel(int index) => SlotLabels[Mathf.Clamp(index, 0, SlotLabels.Length - 1)];

        // The way an item in each slot faces, in the kitchen's own space (its local +Z points at the middle of the kitchen).
        static readonly float[] SlotYawLocal = { -90f, -90f, 90f, 90f };

        public static Vector3 SlotLocalPosition(int index) => SlotLocal[index];

        // Where slot index stands in the world for a kitchen at pivot with the given yaw (degrees). Height is the pivot's; callers find the ground.
        public static Vector3 SlotAt(Vector3 pivot, float yawDegrees, int index) => pivot + Quaternion.Euler(0f, yawDegrees, 0f) * SlotLocal[index];

        // The yaw (degrees) an add-on standing in slot index is built with.
        public static float SlotYaw(float kitchenYawDegrees, int index) => Mathf.Repeat(kitchenYawDegrees + SlotYawLocal[index], 360f);

        static float Flat(Vector3 a, Vector3 b) => Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));

        // The slot a point stands in (within SlotTolerance of its centre, level ground ignored), or -1.
        public static int SlotIndexOf(Vector3 pivot, float yawDegrees, Vector3 point, float tolerance = SlotTolerance)
        {
            int best = -1;
            float bestDistance = tolerance;
            for (int i = 0; i < SlotCount; i++)
            {
                float d = Flat(SlotAt(pivot, yawDegrees, i), point);
                if (d <= bestDistance)
                {
                    best = i;
                    bestDistance = d;
                }
            }
            return best;
        }

        // The slot nearest a point that nothing stands in (occupied holds the positions of the add-ons already there), or -1 when all four
        // are taken. distance is how far that slot is from the point.
        public static int NearestFreeSlot(Vector3 pivot, float yawDegrees, Vector3 point, IList<Vector3> occupied, out float distance)
        {
            int best = -1;
            distance = float.MaxValue;
            for (int i = 0; i < SlotCount; i++)
            {
                Vector3 slot = SlotAt(pivot, yawDegrees, i);
                bool taken = false;
                if (occupied != null)
                    foreach (Vector3 other in occupied)
                        if (Flat(slot, other) <= SlotTolerance)
                            taken = true;
                if (taken)
                    continue;
                float d = Flat(slot, point);
                if (d < distance)
                {
                    best = i;
                    distance = d;
                }
            }
            return best;
        }

        // Metres from a slot's centre to the hearth fire, for the rule that no slot sits within SlotMinHearthGap of it.
        public static float SlotToHearth(int index) => Flat(SlotLocal[index], HearthLocal);
    }

    // An existing Well that a privy or pen has ended up too close to (or uphill of), say because the outhouse was built after it
    // or in a save from before these rules existed. Placement refuses new ones; this is what makes the water worse.
    public static bool Fouls(Vector3 well, IList<SiteInfo> sites, SetbackRules rules, out string reason)
    {
        foreach (SiteInfo site in sites)
        {
            float d = Flat(well, site.position);
            if (site.kind == SiteKind.Outhouse && (d < rules.privyFromWell || (d <= rules.upgradientRange && site.position.y > well.y)))
            {
                reason = d < rules.privyFromWell ? "an outhouse is too close" : "an outhouse stands uphill";
                return true;
            }
            if (site.kind == SiteKind.AnimalPen && d < rules.wellFromPen)
            {
                reason = "an animal pen is too close";
                return true;
            }
        }
        reason = "";
        return false;
    }

    // Ground the creek reaches: near the channel edge and no higher than the creek's surface plus floodRise.
    public static bool Floods(float groundY, float creekSurfaceY, float distanceFromChannelEdge, SetbackRules rules) =>
        distanceFromChannelEdge <= rules.floodBand && groundY <= creekSurfaceY + rules.floodRise;

    // A point lower than the average of a ring of ground around it by more than hollowDepth: rain would run into it.
    public static bool InHollow(float centreY, IList<float> ringY, float hollowDepth)
    {
        if (ringY == null || ringY.Count == 0)
            return false;
        float sum = 0f;
        foreach (float y in ringY)
            sum += y;
        return sum / ringY.Count - centreY > hollowDepth;
    }
}
