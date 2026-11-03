using System.Diagnostics;
using Unity.Collections;
using UnityEngine;
using Debug = UnityEngine.Debug;

// The creek bank look (Mike, 2026-11-04): Wet Clay and Creek Sand painted along the water on RuntimeTerrain's copy of the terrain, so the places the Shovel digs can be seen.
// The terrain asset is never touched, and nothing here is saved.
//
// When: RuntimeTerrain adds this to the terrain and calls Build right after it makes the copy, so the two layers exist (as layers 3 and 4) before SeasonalGround stores
// its base painting, and SeasonalGround, GroundSurface and the minimap all see them.
//
// How: one ray down at the Water layer per square metre of the terrain (batched, run on the job threads), started and judged the way AxeTool.WaterAt / Animal.IsOverWater do
// (30 m above the ground; under water only if the ground is more than 5 cm below the surface). That gives the water's squares, shallow (the dig rule gives Clay from the bed,
// PotteryRules.WadeDigDepthMetres) or deep. An exact distance pass (CreekBankRules.DistanceToWater) then gives every square its distance to the water, and each painted
// cell takes its distance from the four squares around it. CreekBankRules.Weights turns that into the two layers' weights using the dig rule's own limits
// (PotteryRules.WetBankMetres, BankReachMetres). Beyond the reach the old painting stays.
//
// Tuning while playing: the colours, tile sizes, smoothness and texture noise below are public fields. Select the Terrain in the Hierarchy during Play and change them;
// OnValidate rebuilds the two layer textures. Changes last until you stop Play (the component is added at play time); to keep a look, copy the values into the defaults here.
//
// A later rebuild (if the water or the ground ever changes) is two calls, none wired up today: Scan(terrain, data, waterLayer) for a new Field, then Compose(...) on the
// pristine painting (RuntimeTerrain.Source) for the new five-layer painting. SeasonalGround would then have to take that as its base painting and repaint, and GroundSurface
// would have to drop its cached painting. See the report for the details.
public class CreekBanks : MonoBehaviour
{
    [Header("Wet Clay")]
    public Color wetClayColour = new Color(0.42f, 0.24f, 0.16f);
    [Min(0.5f)] public float wetClayTileMetres = 2.5f;
    [Range(0f, 1f)] public float wetClaySmoothness = 0.35f;

    [Header("Creek Sand")]
    public Color creekSandColour = new Color(0.76f, 0.68f, 0.5f);
    [Min(0.5f)] public float creekSandTileMetres = 3f;
    [Range(0f, 1f)] public float creekSandSmoothness = 0.05f;

    [Header("Both")]
    [Tooltip("How much the texture colour varies from patch to patch, 0 (flat) to 0.3.")]
    [Range(0f, 0.3f)] public float textureNoise = 0.1f;

    const int TextureSize = 128;
    const int Seed = 4271;

    // What the water scan found: one entry per square of CreekBankRules.GridMetres, row by row (index = z * cellsX + x).
    public sealed class Field
    {
        public int cellsX, cellsZ;
        public float cellMetres;
        public CreekBankRules.Water[] water;
        public float[] distance; // metres from the square's centre to the nearest water square's centre (infinity: none)
        public int waterCells, shallowCells;
    }

    TerrainLayer clayLayer, sandLayer;
    TerrainData builtData;
    Field field;
    int bankCells;

    public bool Built => clayLayer != null;
    public Field LastField => field;
    public int BankCells => bankCells;
    public TerrainLayer ClayLayer => clayLayer;
    public TerrainLayer SandLayer => sandLayer;

    // Finds the water and adds the two layers to this copy of the terrain data. Does nothing (and leaves the data as it was) when there is no Water layer, no water, or no room
    // for the layers. Not called twice on one copy.
    public void Build(Terrain terrain, TerrainData data)
    {
        if (clayLayer != null || terrain == null || data == null)
            return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        var timer = Stopwatch.StartNew();
#endif
        int waterLayer = LayerMask.NameToLayer("Water");
        if (waterLayer < 0 || !CreekBankRules.CanAddBankLayers(data.terrainLayers.Length))
            return;

        field = Scan(terrain, data, waterLayer);
        if (field.waterCells == 0)
            return;

        MakeLayers();
        TerrainLayer[] layers = data.terrainLayers;
        int oldLayers = layers.Length;
        var all = new TerrainLayer[oldLayers + CreekBankRules.BankLayers];
        layers.CopyTo(all, 0);
        all[oldLayers] = clayLayer;
        all[oldLayers + 1] = sandLayer;
        data.terrainLayers = all; // the terrain keeps its painting; the two new layers start at weight 0

        // Only the rectangle of the map that holds ground near the water changes, so only that is read, painted and written back, in one call (a call costs the same whatever
        // its size, so one is cheapest). The rest of the map is never touched.
        int res = data.alphamapResolution;
        var grid = new Grid(res, field, data.size);
        int firstX = int.MaxValue, lastX = -1, firstZ = int.MaxValue, lastZ = -1;
        for (int z0 = 0; z0 < res; z0 += BandRows)
        {
            int rows = Mathf.Min(BandRows, res - z0);
            if (!grid.NearWater(z0, z0 + rows - 1, out int x0, out int x1))
                continue;
            firstX = Mathf.Min(firstX, x0);
            lastX = Mathf.Max(lastX, x1);
            firstZ = Mathf.Min(firstZ, z0);
            lastZ = Mathf.Max(lastZ, z0 + rows - 1);
        }
        bankCells = 0;
        if (lastX >= 0)
        {
            float[,,] rectangle = data.GetAlphamaps(firstX, firstZ, lastX - firstX + 1, lastZ - firstZ + 1);
            bankCells = PaintRegion(rectangle, firstX, firstZ, oldLayers, grid);
            data.SetAlphamaps(firstX, firstZ, rectangle);
        }
        builtData = data;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log($"[CreekBanks] built in {timer.Elapsed.TotalMilliseconds:0} ms, {field.waterCells} water hits, {bankCells} bank cells");
#endif
    }

    // --- The water scan ---

    public static Field Scan(Terrain terrain, TerrainData data, int waterLayer)
    {
        float cell = CreekBankRules.GridMetres;
        Vector3 origin = terrain.transform.position;
        Vector3 size = data.size;
        int nx = Mathf.CeilToInt(size.x / cell), nz = Mathf.CeilToInt(size.z / cell);
        int total = nx * nz;

        // A ray can only hit water inside a Water-layer collider's box, so only the squares under those boxes get a ray; every other square is dry for certain.
        var wanted = new bool[total];
        Collider[] colliders = Physics.OverlapBox(origin + size * 0.5f, new Vector3(size.x * 0.5f + 1f, 100000f, size.z * 0.5f + 1f), Quaternion.identity,
                                                  1 << waterLayer, QueryTriggerInteraction.Ignore);
        int rays = 0;
        foreach (Collider collider in colliders)
        {
            if (collider is MeshCollider meshCollider && meshCollider.sharedMesh != null && meshCollider.sharedMesh.isReadable)
            {
                // The squares under each triangle's own box: far fewer than the box round the whole mesh.
                Mesh mesh = meshCollider.sharedMesh;
                Vector3[] vertices = mesh.vertices;
                int[] triangles = mesh.triangles;
                Matrix4x4 toWorld = collider.transform.localToWorldMatrix;
                for (int t = 0; t + 2 < triangles.Length; t += 3)
                {
                    Vector3 a = toWorld.MultiplyPoint3x4(vertices[triangles[t]]), b = toWorld.MultiplyPoint3x4(vertices[triangles[t + 1]]),
                            c = toWorld.MultiplyPoint3x4(vertices[triangles[t + 2]]);
                    rays += Mark(wanted, nx, nz, origin, cell, Mathf.Min(a.x, Mathf.Min(b.x, c.x)), Mathf.Max(a.x, Mathf.Max(b.x, c.x)),
                                 Mathf.Min(a.z, Mathf.Min(b.z, c.z)), Mathf.Max(a.z, Mathf.Max(b.z, c.z)));
                }
            }
            else
            {
                Bounds box = collider.bounds;
                rays += Mark(wanted, nx, nz, origin, cell, box.min.x, box.max.x, box.min.z, box.max.z);
            }
        }

        var water = new CreekBankRules.Water[total];
        var isWater = new bool[total];
        int waterCells = 0, shallowCells = 0;
        if (rays > 0)
        {
            var commands = new NativeArray<RaycastCommand>(rays, Allocator.TempJob);
            var hits = new NativeArray<RaycastHit>(rays, Allocator.TempJob);
            var cellOf = new int[rays];
            var groundOf = new float[rays];
            try
            {
                var query = new QueryParameters(1 << waterLayer, false, QueryTriggerInteraction.Ignore, false);
                int next = 0;
                for (int i = 0; i < total; i++)
                {
                    if (!wanted[i])
                        continue;
                    int x = i % nx, z = i / nx;
                    float wx = origin.x + (x + 0.5f) * cell, wz = origin.z + (z + 0.5f) * cell;
                    float ground = terrain.SampleHeight(new Vector3(wx, 0f, wz)) + origin.y;
                    cellOf[next] = i;
                    groundOf[next] = ground;
                    commands[next] = new RaycastCommand(new Vector3(wx, ground + CreekBankRules.RayStartAboveMetres, wz), Vector3.down, query, CreekBankRules.RayLengthMetres);
                    next++;
                }

                RaycastCommand.ScheduleBatch(commands, hits, 64, 1, default).Complete();

                for (int k = 0; k < rays; k++)
                {
                    RaycastHit hit = hits[k];
                    var state = CreekBankRules.WaterAt(hit.colliderEntityId.IsValid(), hit.point.y, groundOf[k]);
                    if (state == CreekBankRules.Water.Dry)
                        continue;
                    int i = cellOf[k];
                    water[i] = state;
                    isWater[i] = true;
                    waterCells++;
                    if (state == CreekBankRules.Water.Shallow)
                        shallowCells++;
                }
            }
            finally
            {
                commands.Dispose();
                hits.Dispose();
            }
        }

        return new Field
        {
            cellsX = nx,
            cellsZ = nz,
            cellMetres = cell,
            water = water,
            distance = CreekBankRules.DistanceToWater(isWater, nx, nz, cell),
            waterCells = waterCells,
            shallowCells = shallowCells,
        };
    }

    // Marks the squares whose area meets a box on the ground; returns how many were newly marked. A ray at a square's centre can only hit inside the box that holds the shape.
    static int Mark(bool[] wanted, int nx, int nz, Vector3 origin, float cell, float minX, float maxX, float minZ, float maxZ)
    {
        int x0 = Mathf.Max(0, Mathf.FloorToInt((minX - origin.x) / cell)), x1 = Mathf.Min(nx - 1, Mathf.FloorToInt((maxX - origin.x) / cell));
        int z0 = Mathf.Max(0, Mathf.FloorToInt((minZ - origin.z) / cell)), z1 = Mathf.Min(nz - 1, Mathf.FloorToInt((maxZ - origin.z) / cell));
        int added = 0;
        for (int z = z0; z <= z1; z++)
        for (int x = x0; x <= x1; x++)
        {
            if (wanted[z * nx + x])
                continue;
            wanted[z * nx + x] = true;
            added++;
        }
        return added;
    }

    // --- Painting ---

    // The terrain's own painting with Wet Clay and Creek Sand added as two more layers (source is [z, x, layer]; the result has two more layers). Every old layer is scaled by what
    // the bank leaves, so the weights still add up to 1. Pure: it changes nothing but its result. Build paints the same way, a band at a time, through PaintRegion.
    public static float[,,] Compose(float[,,] source, Field field, Vector3 terrainSize, out int bankCells)
    {
        int res = source.GetLength(0), layers = source.GetLength(2);
        var painted = new float[res, res, layers + CreekBankRules.BankLayers];
        for (int z = 0; z < res; z++)
            for (int x = 0; x < res; x++)
                for (int l = 0; l < layers; l++)
                    painted[z, x, l] = source[z, x, l];
        bankCells = PaintRegion(painted, 0, 0, layers, new Grid(res, field, terrainSize));
        return painted;
    }

    const int BandRows = 32;
    const int Seed0 = Seed;

    // Where each row and column of painted cells sits among the water squares: the two squares round it and how far between them. The same for every row, so made once.
    sealed class Grid
    {
        public readonly Field field;
        public readonly int res;
        public readonly int[] lo, hi, rowLo, rowHi;
        public readonly float[] frac, rowFrac, metres, rowMetres;
        public readonly float far; // nothing is painted when all four squares round a cell are farther than this

        public Grid(int res, Field field, Vector3 terrainSize)
        {
            this.res = res;
            this.field = field;
            far = PotteryRules.BankReachMetres + field.cellMetres * 1.5f;
            lo = new int[res]; hi = new int[res]; rowLo = new int[res]; rowHi = new int[res];
            frac = new float[res]; rowFrac = new float[res]; metres = new float[res]; rowMetres = new float[res];
            float perCellX = terrainSize.x / res, perCellZ = terrainSize.z / res;
            for (int i = 0; i < res; i++)
            {
                metres[i] = (i + 0.5f) * perCellX;
                float g = metres[i] / field.cellMetres - 0.5f;
                lo[i] = Mathf.Clamp(Mathf.FloorToInt(g), 0, field.cellsX - 1);
                hi[i] = Mathf.Min(lo[i] + 1, field.cellsX - 1);
                frac[i] = Mathf.Clamp01(g - lo[i]);

                rowMetres[i] = (i + 0.5f) * perCellZ;
                float gz = rowMetres[i] / field.cellMetres - 0.5f;
                rowLo[i] = Mathf.Clamp(Mathf.FloorToInt(gz), 0, field.cellsZ - 1);
                rowHi[i] = Mathf.Min(rowLo[i] + 1, field.cellsZ - 1);
                rowFrac[i] = Mathf.Clamp01(gz - rowLo[i]);
            }
        }

        // Whether any cell in these rows could take paint; if so, the first and last column that could (a safe range, found from the water squares, not cell by cell).
        public bool NearWater(int zFrom, int zTo, out int firstX, out int lastX)
        {
            int firstSquare = int.MaxValue, lastSquare = -1;
            for (int sz = rowLo[zFrom]; sz <= rowHi[zTo]; sz++)
                for (int sx = 0; sx < field.cellsX; sx++)
                {
                    if (field.distance[sz * field.cellsX + sx] > far)
                        continue;
                    if (sx < firstSquare) firstSquare = sx;
                    if (sx > lastSquare) lastSquare = sx;
                }

            firstX = 0;
            lastX = -1;
            if (lastSquare < 0)
                return false;
            while (firstX < res && hi[firstX] < firstSquare)
                firstX++;
            lastX = res - 1;
            while (lastX >= 0 && lo[lastX] > lastSquare)
                lastX--;
            return firstX <= lastX;
        }
    }

    // Paints the bank into `alphas`, which holds cells x0.. and z0.. of the map with oldLayers of the terrain's own painting followed by two empty layers. Returns the cells painted.
    static int PaintRegion(float[,,] alphas, int x0, int z0, int oldLayers, Grid grid)
    {
        Field field = grid.field;
        int rows = alphas.GetLength(0), columns = alphas.GetLength(1);
        int clay = oldLayers, sand = oldLayers + 1;
        float[] distance = field.distance;
        int cellsX = field.cellsX;
        int painters = 0;

        for (int z = 0; z < rows; z++)
        {
            int row = z0 + z;
            int lowRow = grid.rowLo[row] * cellsX, highRow = grid.rowHi[row] * cellsX;
            float fz = grid.rowFrac[row];
            for (int x = 0; x < columns; x++)
            {
                int column = x0 + x;
                int i00 = lowRow + grid.lo[column], i10 = lowRow + grid.hi[column], i01 = highRow + grid.lo[column], i11 = highRow + grid.hi[column];
                float d00 = distance[i00], d10 = distance[i10], d01 = distance[i01], d11 = distance[i11];
                if (d00 > grid.far && d10 > grid.far && d01 > grid.far && d11 > grid.far)
                    continue;

                float fx = grid.frac[column];
                // The square the cell sits in decides shallow, deep or dry; a deep square that touches dry or shallow ground is the edge of the deep water and counts as bank.
                int nearest = fz < 0.5f ? (fx < 0.5f ? i00 : i10) : (fx < 0.5f ? i01 : i11);
                CreekBankRules.Water state = field.water[nearest];
                if (state == CreekBankRules.Water.Deep && (field.water[i00] != CreekBankRules.Water.Deep || field.water[i10] != CreekBankRules.Water.Deep ||
                                                          field.water[i01] != CreekBankRules.Water.Deep || field.water[i11] != CreekBankRules.Water.Deep))
                    state = CreekBankRules.Water.Shallow;

                float low = d00 + (d10 - d00) * fx, high = d01 + (d11 - d01) * fx;
                float edge = CreekBankRules.EdgeDistance(low + (high - low) * fz, field.cellMetres);
                float noiseInner = CreekBankRules.HashNoise(grid.metres[column], grid.rowMetres[row], 3f, Seed0);
                float noiseOuter = CreekBankRules.HashNoise(grid.metres[column], grid.rowMetres[row], 2.5f, Seed0 + 1);
                CreekBankRules.Weights(state, edge, noiseInner, noiseOuter, out float clayHere, out float sandHere);
                if (clayHere + sandHere <= 0f)
                    continue;

                float keep = 1f - clayHere - sandHere;
                for (int l = 0; l < oldLayers; l++)
                    alphas[z, x, l] *= keep;
                alphas[z, x, clay] = clayHere;
                alphas[z, x, sand] = sandHere;
                if (clayHere + sandHere > 0.01f)
                    painters++;
            }
        }
        return painters;
    }

    // --- The layers ---

    void MakeLayers()
    {
        clayLayer = new TerrainLayer { name = CreekBankRules.ClayLayerName };
        sandLayer = new TerrainLayer { name = CreekBankRules.SandLayerName };
        Restyle();
    }

    // (Re)makes both textures and applies the tile sizes and smoothness from the fields above.
    void Restyle()
    {
        Style(clayLayer, wetClayColour, wetClayTileMetres, wetClaySmoothness, Seed + 10);
        Style(sandLayer, creekSandColour, creekSandTileMetres, creekSandSmoothness, Seed + 20);
        if (builtData != null)
        {
            TerrainLayer[] layers = builtData.terrainLayers;
            builtData.terrainLayers = layers; // hands the layers to the terrain again, so it picks up the new textures
        }
    }

    void Style(TerrainLayer layer, Color colour, float tileMetres, float smoothness, int seed)
    {
        Discard(layer.diffuseTexture);
        layer.diffuseTexture = MakeTexture(colour, textureNoise, seed);
        layer.tileSize = new Vector2(tileMetres, tileMetres);
        layer.smoothness = smoothness;
    }

    // A small seamless texture: the colour, varied gently by soft patches and a fine speckle.
    public static Texture2D MakeTexture(Color colour, float noise, int seed)
    {
        const int Period = 8;
        var texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGB24, true) { wrapMode = TextureWrapMode.Repeat };
        var random = new System.Random(seed);
        var pixels = new Color[TextureSize * TextureSize];
        for (int y = 0; y < TextureSize; y++)
        for (int x = 0; x < TextureSize; x++)
        {
            float patch = CreekBankRules.PeriodicNoise(x * Period / (float)TextureSize, y * Period / (float)TextureSize, Period, seed);
            float speckle = (float)random.NextDouble();
            float vary = 1f + noise * (((patch * 0.7f + speckle * 0.3f) - 0.5f) * 2f);
            pixels[y * TextureSize + x] = new Color(colour.r * vary, colour.g * vary, colour.b * vary);
        }
        texture.SetPixels(pixels);
        texture.Apply(true);
        return texture;
    }

    void OnValidate()
    {
        if (clayLayer != null && sandLayer != null)
            Restyle();
    }

    void OnDestroy()
    {
        foreach (TerrainLayer layer in new[] { clayLayer, sandLayer })
        {
            if (layer == null)
                continue;
            Discard(layer.diffuseTexture);
            Discard(layer);
        }
    }

    // Destroy at play time; edit-mode tests need the immediate form.
    static void Discard(Object thing)
    {
        if (thing == null)
            return;
        if (Application.isPlaying)
            Destroy(thing);
        else
            DestroyImmediate(thing);
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    // Testing only: right-click the component's header in the Inspector during Play. Prints what the scan found and where the layers are.
    [ContextMenu("Log bank summary")]
    void LogSummary()
    {
        if (field == null)
        {
            Debug.Log("[CreekBanks] nothing built: no Water layer, no water found, or no room for the two layers");
            return;
        }
        int deep = field.waterCells - field.shallowCells;
        Debug.Log($"[CreekBanks] {field.cellsX}x{field.cellsZ} squares of {field.cellMetres} m: {field.waterCells} water ({field.shallowCells} shallow, {deep} deep), " +
                  $"{bankCells} bank cells painted; terrain layers now {(builtData != null ? builtData.alphamapLayers : 0)} (Wet Clay and Creek Sand are the last two before SeasonalGround's)");
    }
#endif
}
