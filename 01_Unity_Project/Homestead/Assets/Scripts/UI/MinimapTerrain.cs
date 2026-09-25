using UnityEngine;

// Paints the minimap's base image from the terrain data rather than a camera render, so it reads as a hand-drawn
// field map and doesn't depend on time of day, weather or lighting: ground colour by terrain layer, hill shading and
// faint contour lines from the heightmap, tree canopy from the tree instances, and water from any WaterSource mesh.
// Built once when the World loads; the fog of war on top decides how much of it the player actually sees.
public static class MinimapTerrain
{
    // Terrain layer order from PropertyTerrainBuilder (matches SurfaceType): Grass, Dirt, Gravel.
    static readonly Color[] LayerColors =
    {
        new Color(0.42f, 0.51f, 0.28f),
        new Color(0.53f, 0.44f, 0.31f),
        new Color(0.64f, 0.61f, 0.55f),
    };
    static readonly Color Canopy = new Color(0.19f, 0.29f, 0.15f);
    static readonly Color Water = new Color(0.2f, 0.37f, 0.47f);
    static readonly Color OffTerrain = new Color(0.12f, 0.14f, 0.11f);

    const float ContourInterval = 4f;
    const float CanopyOpacity = 0.75f;

    static Texture2D cached;
    static Terrain cachedTerrain;
    static int cachedSize;

    // The image for this terrain, built on first use and shared by the minimap and World Map. A newly loaded World
    // has a new Terrain instance, so its image is rebuilt then.
    public static Texture2D Get(Terrain terrain, Vector2 worldMin, float worldSize, int size)
    {
        if (cached != null && cachedTerrain == terrain && cachedSize == size)
            return cached;

        if (cached != null)
            Object.Destroy(cached);
        cached = Build(terrain, worldMin, worldSize, size);
        cachedTerrain = terrain;
        cachedSize = size;
        return cached;
    }

    public static Texture2D Build(Terrain terrain, Vector2 worldMin, float worldSize, int size)
    {
        var pixels = new Color[size * size];
        float metresPerPixel = worldSize / size;

        TerrainData data = terrain != null ? terrain.terrainData : null;
        if (data == null)
        {
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = OffTerrain;
            return ToTexture(pixels, size);
        }

        Vector3 origin = terrain.transform.position;
        Vector3 terrainSize = data.size;
        float[,,] alpha = data.GetAlphamaps(0, 0, data.alphamapWidth, data.alphamapHeight);
        int layers = alpha.GetLength(2);
        Vector3 light = new Vector3(-0.6f, 0.75f, 0.35f).normalized; // from the north-west, the usual map convention

        for (int py = 0; py < size; py++)
        {
            for (int px = 0; px < size; px++)
            {
                float wx = worldMin.x + (px + 0.5f) * metresPerPixel;
                float wz = worldMin.y + (py + 0.5f) * metresPerPixel;
                float u = (wx - origin.x) / terrainSize.x, v = (wz - origin.z) / terrainSize.z;
                if (u < 0f || u > 1f || v < 0f || v > 1f)
                {
                    pixels[py * size + px] = OffTerrain;
                    continue;
                }

                int ax = Mathf.Clamp((int)(u * data.alphamapWidth), 0, data.alphamapWidth - 1);
                int ay = Mathf.Clamp((int)(v * data.alphamapHeight), 0, data.alphamapHeight - 1);
                Color ground = Color.black;
                for (int l = 0; l < layers; l++)
                    ground += LayerColors[Mathf.Min(l, LayerColors.Length - 1)] * alpha[ay, ax, l];

                float shade = 0.72f + 0.4f * Mathf.Max(0f, Vector3.Dot(data.GetInterpolatedNormal(u, v), light));
                Color c = ground * shade;

                // Contour where this pixel and its neighbour sit in different height bands.
                float h = data.GetInterpolatedHeight(u, v);
                float hx = data.GetInterpolatedHeight(Mathf.Min(1f, u + metresPerPixel / terrainSize.x), v);
                float hy = data.GetInterpolatedHeight(u, Mathf.Min(1f, v + metresPerPixel / terrainSize.z));
                int band = Mathf.FloorToInt(h / ContourInterval);
                if (band != Mathf.FloorToInt(hx / ContourInterval) || band != Mathf.FloorToInt(hy / ContourInterval))
                    c *= 0.86f;

                c.a = 1f;
                pixels[py * size + px] = c;
            }
        }

        StampTrees(pixels, size, data, origin, worldMin, metresPerPixel);
        foreach (WaterSource water in Object.FindObjectsByType<WaterSource>(FindObjectsSortMode.None))
            StampWater(pixels, size, water.GetComponent<MeshFilter>(), worldMin, metresPerPixel);

        return ToTexture(pixels, size);
    }

    static void StampTrees(Color[] pixels, int size, TerrainData data, Vector3 origin, Vector2 worldMin, float metresPerPixel)
    {
        TreePrototype[] prototypes = data.treePrototypes;
        var radii = new float[prototypes.Length];
        for (int i = 0; i < prototypes.Length; i++)
        {
            // Canopy radius from the prefab's renderer bounds; small plants (shrubs) read smaller.
            float radius = 2.5f;
            if (prototypes[i].prefab != null)
            {
                Renderer r = prototypes[i].prefab.GetComponentInChildren<Renderer>();
                if (r != null)
                    radius = Mathf.Clamp(Mathf.Max(r.bounds.extents.x, r.bounds.extents.z), 0.6f, 5f);
            }
            radii[i] = radius;
        }

        foreach (TreeInstance tree in data.treeInstances)
        {
            float wx = origin.x + tree.position.x * data.size.x;
            float wz = origin.z + tree.position.z * data.size.z;
            float radius = radii[Mathf.Clamp(tree.prototypeIndex, 0, radii.Length - 1)] * tree.widthScale;
            float cx = (wx - worldMin.x) / metresPerPixel, cy = (wz - worldMin.y) / metresPerPixel;
            float rp = radius / metresPerPixel;

            int x0 = Mathf.Max(0, Mathf.FloorToInt(cx - rp)), x1 = Mathf.Min(size - 1, Mathf.CeilToInt(cx + rp));
            int y0 = Mathf.Max(0, Mathf.FloorToInt(cy - rp)), y1 = Mathf.Min(size - 1, Mathf.CeilToInt(cy + rp));
            for (int y = y0; y <= y1; y++)
            {
                for (int x = x0; x <= x1; x++)
                {
                    float d = Mathf.Sqrt((x + 0.5f - cx) * (x + 0.5f - cx) + (y + 0.5f - cy) * (y + 0.5f - cy));
                    if (d > rp)
                        continue;

                    int i = y * size + x;
                    // Darker toward the crown's centre, so overlapping trees read as depth rather than a flat blob.
                    float k = CanopyOpacity * (1f - 0.35f * d / Mathf.Max(rp, 0.001f));
                    Color c = Color.Lerp(pixels[i], Canopy * (0.85f + 0.3f * (1f - d / Mathf.Max(rp, 0.001f))), k);
                    c.a = 1f;
                    pixels[i] = c;
                }
            }
        }
    }

    // Rasterises the water mesh's triangles, seen from above.
    static void StampWater(Color[] pixels, int size, MeshFilter filter, Vector2 worldMin, float metresPerPixel)
    {
        if (filter == null || filter.sharedMesh == null)
            return;

        Mesh mesh = filter.sharedMesh;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        var points = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 w = filter.transform.TransformPoint(vertices[i]);
            points[i] = new Vector2((w.x - worldMin.x) / metresPerPixel, (w.z - worldMin.y) / metresPerPixel);
        }

        for (int t = 0; t < triangles.Length; t += 3)
        {
            Vector2 a = points[triangles[t]], b = points[triangles[t + 1]], c = points[triangles[t + 2]];
            int x0 = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(a.x, Mathf.Min(b.x, c.x))));
            int x1 = Mathf.Min(size - 1, Mathf.CeilToInt(Mathf.Max(a.x, Mathf.Max(b.x, c.x))));
            int y0 = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(a.y, Mathf.Min(b.y, c.y))));
            int y1 = Mathf.Min(size - 1, Mathf.CeilToInt(Mathf.Max(a.y, Mathf.Max(b.y, c.y))));
            float area = Edge(a, b, c);
            if (Mathf.Abs(area) < 1e-6f)
                continue;

            for (int y = y0; y <= y1; y++)
            {
                for (int x = x0; x <= x1; x++)
                {
                    var p = new Vector2(x + 0.5f, y + 0.5f);
                    float w0 = Edge(b, c, p) / area, w1 = Edge(c, a, p) / area, w2 = Edge(a, b, p) / area;
                    if (w0 >= 0f && w1 >= 0f && w2 >= 0f)
                        pixels[y * size + x] = Water;
                }
            }
        }
    }

    static float Edge(Vector2 a, Vector2 b, Vector2 p) => (b.x - a.x) * (p.y - a.y) - (b.y - a.y) * (p.x - a.x);

    static Texture2D ToTexture(Color[] pixels, int size)
    {
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = "Minimap Terrain",
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            hideFlags = HideFlags.DontSave,
        };
        texture.SetPixels(pixels);
        texture.Apply(false, true);
        return texture;
    }
}
