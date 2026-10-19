using System.Collections.Generic;
using UnityEngine;

public enum SiteKind { Well, Outhouse, Building, AnimalPen }

// Something already standing that a new Well or Outhouse is measured against. position.y is the ground height there.
public struct SiteInfo
{
    public SiteKind kind;
    public Vector3 position;
    public string name; // for the refusal text, e.g. "Small Cabin"
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
    public float upgradientRange;     // a well must be higher than any privy within this distance
    public float hollowDepth;         // a point this much lower than the ground around it is a hollow
    public float floodBand;           // ground within this distance of the channel edge ...
    public float floodRise;           // ... and no higher than the creek surface plus this, floods

    public static SetbackRules Defaults => new SetbackRules
    {
        privyFromWater = 30f, privyFromWell = 30f, privyFromBuilding = 7.5f, wellFromBuilding = 3f, wellFromPen = 15f,
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
