using System.Collections.Generic;
using UnityEngine;

// Flash Floods phase 5: where the creek runs and which way it flows. PropertyTerrainBuilder (Editor-only) bakes the creek into
// the terrain from these control points; the same points are copied here so the runtime can ask "which way is downstream
// here?" and "is this inside the channel?". The path is the same Catmull-Rom curve the builder carves (1 m samples), so the
// nearest segment follows the real bed rather than the straight chords between control points. Flow is the direction of the
// nearest segment, downstream (spring to pond). Keep ControlPoints and the width figures in step with PropertyTerrainBuilder.
// Pure maths, no scene access, so the edit-mode tests can run it.
public static class CreekFlow
{
    public static readonly Vector2[] ControlPoints =
    {
        new Vector2(-15f, 115f), // spring
        new Vector2(-35f, 92f), new Vector2(-62f, 70f), new Vector2(-88f, 42f), new Vector2(-104f, 8f),
        new Vector2(-118f, -30f), new Vector2(-136f, -68f), new Vector2(-155f, -102f), new Vector2(-172f, -128f),
        new Vector2(-185f, -150f), // pond centre
    };

    // PropertyTerrainBuilder: creekHalf = Lerp(0.9, 1.8, progress) + Lerp(2.2, 0, InverseLerp(PondRadius, PondRadius + 25, fromPond)).
    const float UpstreamHalfWidth = 0.9f;
    const float DownstreamHalfWidth = 1.8f;
    const float PondWideningMax = 2.2f;
    const float BuilderPondRadius = 24f;
    const float PondWideningRange = 25f;

    static readonly List<Vector2> path = new List<Vector2>();
    static readonly List<float> half = new List<float>();
    static readonly int[] controlSample = new int[ControlPoints.Length];
    static Vector2 boundsMin, boundsMax;
    static float maxHalf;

    public static int SampleCount => path.Count;
    public static Vector2 PathPoint(int sample) => path[sample];
    // The path sample that is control point i.
    public static int ControlSample(int control) => controlSample[control];

    static CreekFlow()
    {
        Vector2[] c = ControlPoints;
        path.Add(c[0]);
        controlSample[0] = 0;
        for (int i = 0; i + 1 < c.Length; i++)
        {
            Vector2 p0 = c[Mathf.Max(i - 1, 0)], p1 = c[i], p2 = c[i + 1];
            Vector2 p3 = c[Mathf.Min(i + 2, c.Length - 1)];
            int samples = Mathf.Max(1, Mathf.CeilToInt(Vector2.Distance(p1, p2) / 1f));
            for (int s = 1; s <= samples; s++)
            {
                float t = s / (float)samples;
                path.Add(0.5f * (2f * p1 + (p2 - p0) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * (t * t) +
                                 (3f * p1 - p0 - 3f * p2 + p3) * (t * t * t)));
            }
            controlSample[i + 1] = path.Count - 1;
        }

        var along = new float[path.Count];
        float length = 0f;
        for (int i = 1; i < path.Count; i++)
            along[i] = length += Vector2.Distance(path[i - 1], path[i]);

        Vector2 pond = c[c.Length - 1];
        boundsMin = boundsMax = path[0];
        for (int i = 0; i < path.Count; i++)
        {
            float progress = along[i] / length;
            float fromPond = Vector2.Distance(path[i], pond);
            float h = Mathf.Lerp(UpstreamHalfWidth, DownstreamHalfWidth, progress) +
                      Mathf.Lerp(PondWideningMax, 0f, Mathf.InverseLerp(BuilderPondRadius, BuilderPondRadius + PondWideningRange, fromPond));
            half.Add(h);
            maxHalf = Mathf.Max(maxHalf, h);
            boundsMin = Vector2.Min(boundsMin, path[i]);
            boundsMax = Vector2.Max(boundsMax, path[i]);
        }
    }

    // The path segment nearest the point (segment i runs from sample i to i + 1) and the distance to it. On a tie the upstream
    // segment wins, so a point equally near both sides of a bend resolves the same way every time.
    public static int NearestSegment(Vector2 p, out float distance)
    {
        int best = 0;
        float bestSqr = float.MaxValue;
        for (int i = 0; i + 1 < path.Count; i++)
        {
            Vector2 a = path[i], ab = path[i + 1] - a;
            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / Mathf.Max(1e-6f, ab.sqrMagnitude));
            float sqr = (p - (a + ab * t)).sqrMagnitude;
            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                best = i;
            }
        }
        distance = Mathf.Sqrt(bestSqr);
        return best;
    }

    // Unit vector downstream along a segment.
    public static Vector2 SegmentFlow(int segment) => (path[segment + 1] - path[segment]).normalized;

    // Downstream direction at the point (nearest segment's), wherever the point is.
    public static Vector2 FlowAt(Vector2 p) => SegmentFlow(NearestSegment(p, out _));

    public static float HalfWidthOfSegment(int segment) => 0.5f * (half[segment] + half[segment + 1]);

    // Within the channel's half-width plus margin of the centre line. flow is the nearest segment's direction when true.
    public static bool InChannel(Vector2 p, float margin, out Vector2 flow)
    {
        flow = Vector2.zero;
        float reach = maxHalf + margin;
        if (p.x < boundsMin.x - reach || p.x > boundsMax.x + reach || p.y < boundsMin.y - reach || p.y > boundsMax.y + reach)
            return false;

        int segment = NearestSegment(p, out float distance);
        if (distance > HalfWidthOfSegment(segment) + margin)
            return false;
        flow = SegmentFlow(segment);
        return true;
    }

    // The player "is in the creek": over creek water (zone null, not Spring Hollow or the pond), at least minDepth deep at the
    // feet, and inside the channel. waterDepth is the water surface minus the feet height (negative or 0 when not over water).
    public static bool InCreek(Vector2 p, float waterDepth, float minDepth, float margin, bool zoneIsCreek, out Vector2 flow)
    {
        flow = Vector2.zero;
        if (!zoneIsCreek || waterDepth < minDepth)
            return false;
        return InChannel(p, margin, out flow);
    }
}
