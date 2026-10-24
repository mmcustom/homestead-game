using System.Collections.Generic;
using System.Text;

// Well Water Table, B2: what the Well Site ghost says about the spot under it. Pure text and rules over numbers the caller measured, no scene
// access, so the edit-mode tests can run it. A refusal stays red with its own reason; a legal spot is green, or amber when it has a warning.
// It shows the depth to water and its tier, and (B3, now that the Well uses them) the dig blows, lining Stone, buffer and recovery for the spot,
// straight from the WellWaterResult the placed well will get.
public enum PreviewLevel
{
    Clear,    // legal, nothing to warn about: green
    Warning,  // legal, with a warning: amber
    Refused,  // not allowed here: red
}

// Something near the spot that matters to a well, and how far (metres) it is: an outhouse, an animal pen, the creek or a pond.
public struct PreviewHazard
{
    public string name;
    public float metres;
}

public readonly struct WellPreviewResult
{
    public readonly PreviewLevel level;
    public readonly string text; // the info lines, joined by newlines (for a refusal: the reason alone)

    public WellPreviewResult(PreviewLevel level, string text)
    {
        this.level = level;
        this.text = text;
    }
}

public static class WellPreview
{
    public const string TooDeepMessage = "The water is too deep to dig by hand here (a drilled well needs a rig).";

    // Hazards farther than this are not worth a line.
    public const float NearbyRangeMetres = 100f;
    public const int MaxNearbyLines = 3;

    // A legal spot whose creek channel edge is this far or nearer gets the flood warning. reachMetres is the distance flash floods
    // reach a well (WellsAndOuthouses.wellFloodReachMetres); the test is "within", as the hourly flood check is, so exactly on the line warns.
    public static bool FloodWarning(float creekEdgeMetres, float reachMetres) => creekEdgeMetres <= reachMetres;

    // refusal: the reason this spot is not allowed (from the placement rules), or null/empty when it is. water: the depth answer for the spot,
    // or null when no permanent water could be found (the depth line is then left out). creekEdgeMetres: distance to the creek channel's edge.
    public static WellPreviewResult Build(string refusal, WellWaterResult? water, float creekEdgeMetres, float floodReachMetres,
                                          IList<PreviewHazard> hazards)
    {
        if (!string.IsNullOrEmpty(refusal))
            return new WellPreviewResult(PreviewLevel.Refused, refusal);
        if (water.HasValue && !water.Value.Diggable)
            return new WellPreviewResult(PreviewLevel.Refused, TooDeepMessage);

        var text = new StringBuilder();
        if (water.HasValue)
        {
            WellWaterResult w = water.Value;
            text.Append($"Depth to water: {w.depthToWater:0.0} m ({w.tier})");
            Append(text, DigLine(w));
            Append(text, SupplyLine(w));
        }

        string near = NearbyLine(hazards);
        if (near.Length > 0)
            Append(text, near);

        bool floods = FloodWarning(creekEdgeMetres, floodReachMetres);
        if (floods)
            Append(text, $"Floods can foul this well here: the creek is {creekEdgeMetres:0} m away and a flash flood reaches {floodReachMetres:0} m.");

        // The one hint that is true today: low ground by the water is the exposed ground.
        bool shallow = water.HasValue && water.Value.tier == WellTier.Shallow;
        if (floods || shallow)
            Append(text, "Higher ground is safer but deeper.");

        return new WellPreviewResult(floods ? PreviewLevel.Warning : PreviewLevel.Clear, text.ToString());
    }

    // "Dig: 40 blows, lining 10 Stone"
    public static string DigLine(WellWaterResult w) => $"Dig: {w.digBlows} blows, lining {w.liningStone} Stone";

    // "Supply: 150 L, refills 6 L/h (3 L/h in a drought)"
    public static string SupplyLine(WellWaterResult w) =>
        $"Supply: {w.bufferLitres} L, refills {w.recoveryLitresPerHour:0.#} L/h ({w.droughtRecoveryLitresPerHour:0.#} L/h in a drought)";

    // "Nearby: outhouse 42 m, the creek 31 m" - the nearest few within range, nearest first; empty when there are none.
    public static string NearbyLine(IList<PreviewHazard> hazards)
    {
        if (hazards == null)
            return "";
        var near = new List<PreviewHazard>();
        foreach (PreviewHazard hazard in hazards)
            if (hazard.metres >= 0f && hazard.metres <= NearbyRangeMetres)
                near.Add(hazard);
        if (near.Count == 0)
            return "";
        near.Sort((a, b) => a.metres.CompareTo(b.metres));

        var parts = new List<string>();
        for (int i = 0; i < near.Count && i < MaxNearbyLines; i++)
            parts.Add($"{near[i].name} {near[i].metres:0} m");
        return "Nearby: " + string.Join(", ", parts);
    }

    static void Append(StringBuilder text, string line)
    {
        if (text.Length > 0)
            text.Append('\n');
        text.Append(line);
    }
}
