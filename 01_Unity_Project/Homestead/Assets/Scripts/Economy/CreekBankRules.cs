using System;

// The creek bank look (Mike, 2026-11-04): Wet Clay and Creek Sand painted along the water so the places the Shovel digs can be seen. Pure and engine-free so edit-mode
// tests cover it; CreekBanks (World) finds the water and paints, this file holds the rules: what counts as water, the distance pass, and how far from the water each layer
// reaches. The reach comes from PotteryRules, the same numbers the dig rule uses (Clay inside WetBankMetres or in water up to WadeDigDepthMetres, Sand out to BankReachMetres),
// so the look and the digging cannot drift apart.
public static class CreekBankRules
{
    // The two layers' names. SeasonalGround, GroundSurface and MinimapTerrain find them by these.
    public const string ClayLayerName = "Wet Clay (runtime)";
    public const string SandLayerName = "Creek Sand (runtime)";
    public const int BankLayers = 2;

    // The water scan: one ray per square this size, started this far above the ground and this long, the same as AxeTool.WaterAt / Animal.IsOverWater (30 m above, 80 m down);
    // the ground counts as under water when it lies more than WaterMarginMetres below the surface (Animal.IsOverWater's 5 cm).
    public const float GridMetres = 1f;
    public const float RayStartAboveMetres = 30f;
    public const float RayLengthMetres = 80f;
    public const float WaterMarginMetres = 0.05f;

    // How soft the edges are, in metres. Each fade is a band of between Min and Max, its width chosen by noise, always ending exactly on the dig rule's limit.
    public const float InnerFadeMinMetres = 0.2f, InnerFadeMaxMetres = 0.8f; // Clay gives way to Sand ending at WetBankMetres
    public const float OuterFadeMinMetres = 0.3f, OuterFadeMaxMetres = 1.0f; // Sand gives way to the old painting ending at BankReachMetres

    public enum Water : byte { Dry = 0, Shallow = 1, Deep = 2 }

    public static bool IsBankLayer(string layerName) => layerName == ClayLayerName || layerName == SandLayerName;

    // Whether the ground at a point lies under water, and how kind: Shallow is where the dig rule gives Clay from the bed, Deep is where it refuses.
    public static Water WaterAt(bool hitWater, float surfaceY, float groundY)
    {
        if (!hitWater || !(groundY < surfaceY - WaterMarginMetres))
            return Water.Dry;
        return surfaceY - groundY <= PotteryRules.WadeDigDepthMetres ? Water.Shallow : Water.Deep;
    }

    // Whether the layer budget (TerrainLit takes 8) has room for the bank layers on top of the terrain's own and SeasonalGround's two.
    public static bool CanAddBankLayers(int terrainLayers) =>
        terrainLayers + BankLayers + GroundCoverRules.SeasonalLayers <= GroundCoverRules.MaxTerrainLayers;

    // --- Distance to water ---

    // For every square, the distance in metres from its centre to the nearest water square's centre (0 on water; infinity when there is no water). Exact (Felzenszwalb).
    // water is width x height, row by row (index = z * width + x).
    public static float[] DistanceToWater(bool[] water, int width, int height, float cellMetres)
    {
        const float Inf = 1e20f;
        var field = new float[width * height];
        for (int i = 0; i < field.Length; i++)
            field[i] = water[i] ? 0f : Inf;

        int longest = Math.Max(width, height);
        var f = new float[longest];
        var d = new float[longest];
        var v = new int[longest];
        var z = new float[longest + 1];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
                f[y] = field[y * width + x];
            Transform(f, d, v, z, height);
            for (int y = 0; y < height; y++)
                field[y * width + x] = d[y];
        }
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
                f[x] = field[y * width + x];
            Transform(f, d, v, z, width);
            for (int x = 0; x < width; x++)
                field[y * width + x] = d[x];
        }

        for (int i = 0; i < field.Length; i++)
            field[i] = field[i] >= Inf * 0.5f ? float.PositiveInfinity : (float)Math.Sqrt(field[i]) * cellMetres;
        return field;
    }

    // One pass of the exact squared-distance transform over n values (the lower envelope of parabolas).
    static void Transform(float[] f, float[] d, int[] v, float[] z, int n)
    {
        int k = 0;
        v[0] = 0;
        z[0] = float.NegativeInfinity;
        z[1] = float.PositiveInfinity;
        for (int q = 1; q < n; q++)
        {
            float s;
            while (true)
            {
                int p = v[k];
                s = ((f[q] + (float)q * q) - (f[p] + (float)p * p)) / (2f * q - 2f * p);
                if (s <= z[k] && k > 0)
                    k--;
                else
                    break;
            }
            k++;
            v[k] = q;
            z[k] = s;
            z[k + 1] = float.PositiveInfinity;
        }
        k = 0;
        for (int q = 0; q < n; q++)
        {
            while (z[k + 1] < q)
                k++;
            float diff = q - v[k];
            d[q] = diff * diff + f[v[k]];
        }
    }

    // The distance from a dry point to the water's edge, from the distance between square centres: the edge lies about half a square nearer.
    public static float EdgeDistance(float centreDistance, float cellMetres) => Math.Max(0f, centreDistance - cellMetres * 0.5f);

    // --- The layers' weights ---

    static float Smooth(float from, float to, float value)
    {
        float t = to > from ? (value - from) / (to - from) : (value >= to ? 1f : 0f);
        t = Math.Min(1f, Math.Max(0f, t));
        return t * t * (3f - 2f * t);
    }

    // Wet Clay and Creek Sand weights at a point (they never add up to more than 1; the rest stays the old painting).
    //   water      what lies at the point: in Shallow water the bed is Clay; in Deep water nothing is painted (the dig rule refuses there).
    //   distance   metres from the point to the water's edge, for dry ground.
    //   noiseInner, noiseOuter  0 to 1, steady for a place (HashNoise): how wide the soft edges are, so they are not straight.
    // Dry ground out to WetBankMetres is Clay (fading to Sand over the last InnerFade metres), Sand out to BankReachMetres (fading to the old painting over the last OuterFade
    // metres), nothing beyond. The fades end ON those limits, so nothing is ever painted past BankReachMetres.
    public static void Weights(Water water, float distance, float noiseInner, float noiseOuter, out float clay, out float sand)
    {
        if (water == Water.Deep)
        {
            clay = 0f;
            sand = 0f;
            return;
        }
        if (water == Water.Shallow)
        {
            clay = 1f;
            sand = 0f;
            return;
        }

        float inner = InnerFadeMinMetres + (InnerFadeMaxMetres - InnerFadeMinMetres) * Clamp01(noiseInner);
        float outer = OuterFadeMinMetres + (OuterFadeMaxMetres - OuterFadeMinMetres) * Clamp01(noiseOuter);
        float pastClay = Smooth(PotteryRules.WetBankMetres - inner, PotteryRules.WetBankMetres, distance);
        float pastSand = Smooth(PotteryRules.BankReachMetres - outer, PotteryRules.BankReachMetres, distance);
        clay = 1f - pastClay;
        sand = pastClay * (1f - pastSand);
    }

    static float Clamp01(float value) => Math.Min(1f, Math.Max(0f, value));

    // Steady value noise, 0 to 1: random values on a grid of cellMetres squares, blended smoothly. The same place always gives the same value (no randomness at play time).
    public static float HashNoise(float x, float z, float cellMetres, int seed)
    {
        float gx = x / cellMetres, gz = z / cellMetres;
        int ix = (int)Math.Floor(gx), iz = (int)Math.Floor(gz);
        float fx = gx - ix, fz = gz - iz;
        fx = fx * fx * (3f - 2f * fx);
        fz = fz * fz * (3f - 2f * fz);
        float a = Lattice(ix, iz, seed), b = Lattice(ix + 1, iz, seed);
        float c = Lattice(ix, iz + 1, seed), e = Lattice(ix + 1, iz + 1, seed);
        float low = a + (b - a) * fx, high = c + (e - c) * fx;
        return low + (high - low) * fz;
    }

    // The same noise as HashNoise, but repeating every `period` grid squares in both directions, so a texture made from it tiles without a seam.
    public static float PeriodicNoise(float u, float v, int period, int seed)
    {
        int iu = (int)Math.Floor(u), iv = (int)Math.Floor(v);
        float fu = u - iu, fv = v - iv;
        fu = fu * fu * (3f - 2f * fu);
        fv = fv * fv * (3f - 2f * fv);
        int u0 = Wrap(iu, period), u1 = Wrap(iu + 1, period), v0 = Wrap(iv, period), v1 = Wrap(iv + 1, period);
        float a = Lattice(u0, v0, seed), b = Lattice(u1, v0, seed), c = Lattice(u0, v1, seed), e = Lattice(u1, v1, seed);
        float low = a + (b - a) * fu, high = c + (e - c) * fu;
        return low + (high - low) * fv;
    }

    static int Wrap(int value, int period) => ((value % period) + period) % period;

    static float Lattice(int x, int z, int seed)
    {
        unchecked
        {
            uint h = (uint)(x * 374761393 + z * 668265263 + seed * 1274126177);
            h = (h ^ (h >> 13)) * 1274126177u;
            h ^= h >> 16;
            return (h & 0xFFFFFF) / (float)0x1000000;
        }
    }
}
