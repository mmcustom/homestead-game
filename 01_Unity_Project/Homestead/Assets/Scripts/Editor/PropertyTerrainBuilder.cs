using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

// Builds World.unity's property from Property_Layout.md: rolling ground with the South Ridge high point, the
// Spring Hollow -> creek -> Bass Hole water system, forest over about two-thirds of the land thickening at the
// edges, open pasture around the cabin site, and texture layers matched to GroundSurface's footstep tags. Also
// moves the five Discovery_Test_Sites.md sites and the player spawn onto the new ground.
//
// Deterministic and rerunnable: running it again replaces the generated assets and the scene's "Property"
// object, so layout changes are made here rather than by hand-painting the terrain (hand edits would be lost).
public static class PropertyTerrainBuilder
{
    const string WorldScenePath = "Assets/Scenes/World.unity";
    const string ArtFolder = "Assets/Art/Terrain";
    const string PrefabFolder = "Assets/Prefabs/Environment";
    const string RootName = "Property";
    const int Seed = 1851;

    // Property_Layout.md: roughly 400-500m across.
    const float Size = 480f;
    const float MaxHeight = 60f;
    const int HeightRes = 513;
    const int AlphaRes = 1024; // ~0.47m texels, fine enough for a footpath
    static readonly Vector2 Origin = new Vector2(-Size / 2f, -Size / 2f);

    // Layout, in world metres (x east, y north). Trail and Briar Patch sit on the pasture's edge, so they're
    // derived from it below rather than fixed here.
    static readonly Vector2 CabinSite = new Vector2(18f, -68f);
    static readonly Vector2 SpringCenter = new Vector2(-15f, 115f);
    static readonly Vector2 PondCenter = new Vector2(-185f, -150f);
    const float PondRadius = 24f;
    static readonly Vector2 RidgeCenter = new Vector2(12f, -80f);
    static readonly Vector2 PastureCenter = new Vector2(25f, -5f);
    static readonly Vector2 PastureRadii = new Vector2(165f, 100f);
    const float PastureAngle = 10f;
    static readonly Vector2 NorthEastGlade = new Vector2(140f, 140f);
    static readonly Vector2 OldField = new Vector2(135f, -125f);
    static readonly Vector2 CreekBottom = new Vector2(-75f, 45f);
    static readonly Vector2 PlayerSpawn = new Vector2(20f, -40f);
    const float PlayerSpawnYaw = 180f; // facing the cabin site up the ridge, as the old flat spawn did

    // Creek from the spring, south and west into the pond.
    static readonly Vector2[] CreekControl =
    {
        SpringCenter, new Vector2(-35f, 92f), new Vector2(-62f, 70f), new Vector2(-88f, 42f), new Vector2(-104f, 8f),
        new Vector2(-118f, -30f), new Vector2(-136f, -68f), new Vector2(-155f, -102f), new Vector2(-172f, -128f),
        PondCenter,
    };

    const float EdgeBand = 30f; // forest thickens over this distance from the property edge

    enum Layer { Grass, Dirt, Gravel }

    static Vector2[] noiseOffsets;

    [MenuItem("Homestead/Build Property Terrain")]
    public static void Build()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.path != WorldScenePath)
        {
            Debug.LogError($"[PropertyTerrainBuilder] Open {WorldScenePath} first.");
            return;
        }

        var random = new System.Random(Seed);
        noiseOffsets = new Vector2[16];
        for (int i = 0; i < noiseOffsets.Length; i++)
            noiseOffsets[i] = new Vector2(random.Next(1000, 9000), random.Next(1000, 9000));

        EnsureFolder(ArtFolder);
        EnsureFolder(ArtFolder + "/Textures");
        EnsureFolder(PrefabFolder);

        // Sites that sit on the pasture's edge.
        Vector2 trailSite = PastureEdge((SpringCenter - PastureCenter).normalized, -2f);
        Vector2 briarSite = PastureEdge(Vector2.right, 2f);
        Vector2 bassDir = (CabinSite - PondCenter).normalized;
        Vector2 bassSite = PondCenter + bassDir * (PondRadiusAt(bassDir) + 3f);

        var trail = new List<Vector2>();
        float springAngle = Mathf.Atan2(SpringCenter.y - PastureCenter.y, SpringCenter.x - PastureCenter.x) * Mathf.Rad2Deg;
        for (float a = springAngle - 40f; a <= springAngle + 40f; a += 4f)
            trail.Add(PastureEdge(Dir(a), -2.5f));

        var paths = new List<List<Vector2>>
        {
            WanderingPath(CabinSite, trailSite, 1),
            WanderingPath(trailSite, SpringCenter + (trailSite - SpringCenter).normalized * 7f, 2),
            WanderingPath(CabinSite, briarSite, 3),
            WanderingPath(CabinSite, bassSite, 4),
        };

        // Creek centreline, water level (never rising downstream), channel half-width and depth.
        List<Vector2> creek = CatmullRom(CreekControl, 1f);
        float springLevel = NaturalHeight(SpringCenter) - 0.3f;
        float pondLevel = float.MaxValue;
        for (int i = 0; i < 64; i++)
        {
            Vector2 dir = Dir(i * 360f / 64f);
            for (float r = PondRadiusAt(dir); r <= PondRadiusAt(dir) + 6f; r += 1f)
                pondLevel = Mathf.Min(pondLevel, NaturalHeight(PondCenter + dir * r) - 0.5f);
        }

        int n = creek.Count;
        var creekLevel = new float[n];
        var creekHalf = new float[n];
        var creekDepth = new float[n];
        float length = 0f, minClearance = float.MaxValue;
        var along = new float[n];
        for (int i = 1; i < n; i++)
            along[i] = length += Vector2.Distance(creek[i - 1], creek[i]);
        for (int i = 0; i < n; i++)
        {
            float t = along[i] / length;
            float fromPond = Vector2.Distance(creek[i], PondCenter);
            float clearance = Mathf.Lerp(0.3f, 1.1f, along[i] / 20f);
            float level = Mathf.Min(i > 0 ? creekLevel[i - 1] : springLevel, Mathf.Lerp(springLevel, pondLevel, t),
                                    NaturalHeight(creek[i]) - clearance);
            creekLevel[i] = Mathf.Max(level, pondLevel);
            creekHalf[i] = Mathf.Lerp(0.9f, 1.8f, t) + Mathf.Lerp(2.2f, 0f, Mathf.InverseLerp(PondRadius, PondRadius + 25f, fromPond));
            creekDepth[i] = Mathf.Lerp(0.45f, 0.8f, t);
            if (fromPond > PondRadius + 4f && Vector2.Distance(creek[i], SpringCenter) > 6f)
                minClearance = Mathf.Min(minClearance, NaturalHeight(creek[i]) - creekLevel[i]);
        }

        // Heights.
        var heightCreek = new DistanceField(HeightRes, Size / (HeightRes - 1), 0f);
        heightCreek.Stamp(creek, 12f);
        var heights = new float[HeightRes, HeightRes];
        float highest = 0f, highestAwayFromRidge = 0f;
        Vector2 highestAt = Vector2.zero;
        for (int z = 0; z < HeightRes; z++)
        for (int x = 0; x < HeightRes; x++)
        {
            Vector2 p = heightCreek.Position(x, z);
            float h = NaturalHeight(p);
            h = CarvePond(p, h, pondLevel);
            h = CarveSpring(p, h, springLevel);

            int k = z * HeightRes + x;
            float d = heightCreek.Distance[k];
            if (d < 12f)
            {
                int i = heightCreek.Nearest[k];
                h = CarveCreek(d, h, creekLevel[i], creekHalf[i], creekDepth[i]);
            }

            heights[z, x] = Mathf.Clamp01(h / MaxHeight);
            if (h > highest)
            {
                highest = h;
                highestAt = p;
            }
            if (Vector2.Distance(p, RidgeCenter) > 110f)
                highestAwayFromRidge = Mathf.Max(highestAwayFromRidge, h);
        }

        // Ground surfaces, and the distance fields trees keep clear of.
        float cell = Size / AlphaRes;
        var creekField = new DistanceField(AlphaRes, cell, 0.5f);
        creekField.Stamp(creek, 8f);
        var trailField = new DistanceField(AlphaRes, cell, 0.5f);
        trailField.Stamp(trail, 5f);
        var pathField = new DistanceField(AlphaRes, cell, 0.5f);
        foreach (List<Vector2> path in paths)
            pathField.Stamp(path, 5f);

        var alphas = new float[AlphaRes, AlphaRes, 3];
        for (int z = 0; z < AlphaRes; z++)
        for (int x = 0; x < AlphaRes; x++)
        {
            Vector2 p = creekField.Position(x, z);
            int k = z * AlphaRes + x;
            float edgeNoise = Noise(p, 1f / 6f, 5) * 0.8f;

            float gravel = 0f;
            if (creekField.Distance[k] < 8f)
            {
                float half = creekHalf[creekField.Nearest[k]];
                gravel = 1f - Smooth(half + 2.2f, half + 3.2f, creekField.Distance[k] + edgeNoise);
            }
            Vector2 fromPond = p - PondCenter;
            float pondR = fromPond.sqrMagnitude > 0.01f ? PondRadiusAt(fromPond.normalized) : PondRadius;
            gravel = Mathf.Max(gravel, 1f - Smooth(pondR + 4f, pondR + 5.5f, fromPond.magnitude + edgeNoise));
            gravel = Mathf.Max(gravel, 1f - Smooth(7f, 8.5f, Vector2.Distance(p, SpringCenter) + edgeNoise));

            float dirt = Mathf.Max(1f - Smooth(1.3f, 2.1f, trailField.Distance[k] + edgeNoise),
                                   1f - Smooth(0.9f, 1.7f, pathField.Distance[k] + edgeNoise));
            dirt *= 1f - gravel;

            alphas[z, x, (int)Layer.Gravel] = gravel;
            alphas[z, x, (int)Layer.Dirt] = dirt;
            alphas[z, x, (int)Layer.Grass] = 1f - gravel - dirt;
        }

        // Assets.
        TerrainLayer[] layers =
        {
            MakeLayer("Grass", GrassTexture(), 4f),
            MakeLayer("Dirt", DirtTexture(), 3f),
            MakeLayer("Gravel", GravelTexture(), 2f),
        };

        string dataPath = ArtFolder + "/PropertyTerrainData.asset";
        AssetDatabase.DeleteAsset(dataPath);
        // Created as an asset before filling it: creating it afterwards drops the painted alphamaps.
        var data = new TerrainData();
        AssetDatabase.CreateAsset(data, dataPath);
        data.heightmapResolution = HeightRes;
        data.size = new Vector3(Size, MaxHeight, Size);
        data.alphamapResolution = AlphaRes;
        data.baseMapResolution = 1024;
        data.SetHeights(0, 0, heights);
        data.terrainLayers = layers;
        data.SetAlphamaps(0, 0, alphas);
        data.treePrototypes = new[]
        {
            new TreePrototype { prefab = TreePrefab("Hardwood Broad", 0) },
            new TreePrototype { prefab = TreePrefab("Hardwood Tall", 1) },
            new TreePrototype { prefab = TreePrefab("Understory Shrub", 2) },
        };

        // Trees.
        var sites = new[] { CabinSite, SpringCenter, trailSite, briarSite, bassSite, PlayerSpawn };
        var trees = new List<TreeInstance>();
        var treeRandom = new System.Random(Seed + 7);
        const float TreeCell = 3.2f;
        int forestTexels = 0;
        for (float cz = Origin.y; cz < Origin.y + Size; cz += TreeCell)
        for (float cx = Origin.x; cx < Origin.x + Size; cx += TreeCell)
        {
            float edge = EdgeDistance(new Vector2(cx, cz));
            int candidates = edge < 15f ? 2 : 1;
            for (int c = 0; c < candidates; c++)
            {
                var p = new Vector2(cx + (float)treeRandom.NextDouble() * TreeCell, cz + (float)treeRandom.NextDouble() * TreeCell);
                float roll = (float)treeRandom.NextDouble();
                int kind = treeRandom.Next(100);
                float scale = 0.8f + (float)treeRandom.NextDouble() * 0.55f;
                float width = scale * (0.85f + (float)treeRandom.NextDouble() * 0.3f);
                float rotation = (float)treeRandom.NextDouble() * Mathf.PI * 2f;

                edge = EdgeDistance(p);
                if (edge < 0.5f)
                    continue;
                float forest = 1f - Openness(p);
                float chance = forest * (0.4f + 0.2f * Noise(p, 1f / 60f, 6));
                chance = Mathf.Max(chance, forest * (1f - edge / EdgeBand));
                if (roll >= chance || Blocked(p, creekField, creekHalf, trailField, pathField, sites))
                    continue;

                // Brushier where forest meets open ground.
                bool forestEdge = forest < 0.85f;
                int prototype = kind < (forestEdge ? 35 : 18) ? 2 : kind < 65 ? 0 : 1;
                trees.Add(new TreeInstance
                {
                    position = new Vector3((p.x - Origin.x) / Size, 0f, (p.y - Origin.y) / Size),
                    prototypeIndex = prototype,
                    heightScale = prototype == 2 ? scale * 0.9f : scale,
                    widthScale = prototype == 2 ? width * 1.1f : width,
                    rotation = rotation,
                    color = Color.white,
                    lightmapColor = Color.white,
                });
            }
        }
        data.SetTreeInstances(trees.ToArray(), true);
        EditorUtility.SetDirty(data);

        for (int z = 0; z < AlphaRes; z += 4)
        for (int x = 0; x < AlphaRes; x += 4)
            if (Openness(creekField.Position(x, z)) < 0.5f)
                forestTexels++;
        float forestShare = forestTexels / (float)((AlphaRes / 4) * (AlphaRes / 4));

        // Scene objects.
        GameObject oldRoot = GameObject.Find(RootName);
        if (oldRoot != null)
            Undo.DestroyObjectImmediate(oldRoot);
        GameObject placeholder = GameObject.Find("Ground (Placeholder)");
        if (placeholder != null)
            Undo.DestroyObjectImmediate(placeholder);

        var root = new GameObject(RootName);
        Undo.RegisterCreatedObjectUndo(root, "Build Property Terrain");

        GameObject terrainObject = Terrain.CreateTerrainGameObject(data);
        terrainObject.name = "Property Terrain";
        terrainObject.transform.SetParent(root.transform);
        terrainObject.transform.position = new Vector3(Origin.x, 0f, Origin.y);
        GameObjectUtility.SetStaticEditorFlags(terrainObject, (StaticEditorFlags)~0);
        var terrain = terrainObject.GetComponent<Terrain>();
        terrain.materialTemplate = TerrainMaterial();
        terrain.drawInstanced = true;
        terrain.heightmapPixelError = 4f;
        terrain.basemapDistance = 300f;
        terrain.treeDistance = 350f;
        terrain.treeBillboardDistance = 350f;

        var surface = terrainObject.AddComponent<GroundSurface>();
        var serialized = new SerializedObject(surface);
        SerializedProperty mappings = serialized.FindProperty("terrainLayers");
        mappings.arraySize = layers.Length;
        for (int i = 0; i < layers.Length; i++)
        {
            SerializedProperty mapping = mappings.GetArrayElementAtIndex(i);
            mapping.FindPropertyRelative("layer").objectReferenceValue = layers[i];
            mapping.FindPropertyRelative("surface").enumValueIndex = (int)(SurfaceType)i;
        }
        serialized.ApplyModifiedPropertiesWithoutUndo();

        var water = new GameObject("Water (Placeholder)");
        water.transform.SetParent(root.transform);
        water.AddComponent<MeshFilter>().sharedMesh = WaterMesh(creek, creekLevel, creekHalf, springLevel, pondLevel);
        water.AddComponent<MeshRenderer>().sharedMaterial = LitMaterial("Water", new Color(0.08f, 0.16f, 0.18f), 0.95f);
        GameObjectUtility.SetStaticEditorFlags(water, (StaticEditorFlags)~0);

        // Sites and spawn onto the new ground.
        MoveSite("South Ridge Cabin Site", CabinSite, terrain);
        MoveSite("Spring Hollow", SpringCenter + (trailSite - SpringCenter).normalized * 6f, terrain);
        MoveSite("Old Fence Line Trail", trailSite, terrain);
        MoveSite("Briar Patch", briarSite, terrain);
        MoveSite("Bass Hole", bassSite, terrain);

        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            Undo.RecordObject(player.transform, "Build Property Terrain");
            player.transform.SetPositionAndRotation(OnGround(PlayerSpawn, terrain) + Vector3.up * 0.05f,
                                                    Quaternion.Euler(0f, PlayerSpawnYaw, 0f));
        }

        EditorSceneManager.MarkSceneDirty(scene);
        AssetDatabase.SaveAssets();

        Debug.Log($"[PropertyTerrainBuilder] Built {Size}m property: highest point {highest:F1}m at {highestAt} " +
                  $"(highest away from South Ridge {highestAwayFromRidge:F1}m), spring {springLevel:F1}m -> pond {pondLevel:F1}m, " +
                  $"creek {length:F0}m long with at least {minClearance:F2}m bank above water, forest {forestShare:P0}, " +
                  $"{data.treeInstanceCount} trees. Sites from cabin: spring {Vector2.Distance(CabinSite, SpringCenter):F0}m, " +
                  $"trail {Vector2.Distance(CabinSite, trailSite):F0}m, briar {Vector2.Distance(CabinSite, briarSite):F0}m, " +
                  $"bass hole {Vector2.Distance(CabinSite, bassSite):F0}m.");
    }

    // --- Shape -------------------------------------------------------------------------------------------------

    // Gently rolling ground tilting down toward the pond, with the South Ridge rise, the spring's hollow and a
    // broad bowl around the pond. Channels are carved into this afterwards.
    static float NaturalHeight(Vector2 p)
    {
        float h = 21f + 2.6f * Noise(p, 1f / 140f, 0) + 1.2f * Noise(p, 1f / 55f, 1) + 0.35f * Noise(p, 1f / 18f, 2);
        h += 0.010f * p.x + 0.006f * p.y;

        Vector2 q = Rotate(p - RidgeCenter, -15f);
        h += 11.5f * Mathf.Exp(-(q.x * q.x / (2f * 80f * 80f) + q.y * q.y / (2f * 40f * 40f)));

        h -= 4f * Bump(Vector2.Distance(p, SpringCenter), 34f);
        h -= 2.5f * Bump(Vector2.Distance(p, PondCenter), 70f);
        return h;
    }

    static float CarvePond(Vector2 p, float h, float level)
    {
        Vector2 offset = p - PondCenter;
        float r = offset.magnitude;
        float radius = r > 0.01f ? PondRadiusAt(offset / r) : PondRadius;
        if (r < radius)
            return Mathf.Min(h, level - 0.25f - 2.8f * (1f - r * r / (radius * radius)));
        if (r < radius + 2.5f)
            return Mathf.Min(h, Mathf.Lerp(level - 0.25f, level + 0.3f, (r - radius) / 2.5f));
        return Mathf.Min(h, Mathf.Lerp(level + 0.3f, h, Smooth(radius + 2.5f, radius + 16f, r)));
    }

    static float CarveSpring(Vector2 p, float h, float level)
    {
        float r = Vector2.Distance(p, SpringCenter);
        if (r < 4.5f)
            return Mathf.Min(h, level - 0.12f - 0.7f * (1f - r * r / 20.25f));
        if (r < 5.5f)
            return Mathf.Min(h, Mathf.Lerp(level - 0.12f, level + 0.3f, r - 4.5f));
        return Mathf.Min(h, Mathf.Lerp(level + 0.3f, h, Smooth(5.5f, 12f, r)));
    }

    static float CarveCreek(float d, float h, float level, float half, float depth)
    {
        if (d <= half)
            return Mathf.Min(h, level - 0.12f - depth * (1f - d * d / (half * half)));
        if (d <= half + 1f)
            return Mathf.Min(h, Mathf.Lerp(level - 0.12f, level + 0.35f, d - half));
        return Mathf.Min(h, Mathf.Lerp(level + 0.35f, h, Smooth(half + 1f, half + 7f, d)));
    }

    static float PondRadiusAt(Vector2 dir) =>
        PondRadius * (1f + 0.18f * Noise(dir * 1.5f, 1f, 7) + 0.06f * Noise(dir * 4f, 1f, 8));

    static float Pasture(Vector2 p)
    {
        Vector2 q = Rotate(p - PastureCenter, -PastureAngle);
        float m = Mathf.Sqrt(q.x * q.x / (PastureRadii.x * PastureRadii.x) + q.y * q.y / (PastureRadii.y * PastureRadii.y));
        m += 0.16f * Noise(p, 1f / 45f, 10) + 0.05f * Noise(p, 1f / 12f, 11);
        return 1f - Smooth(0.94f, 1.06f, m);
    }

    // 1 = open ground, 0 = forest.
    static float Openness(Vector2 p)
    {
        float open = Pasture(p);
        open = Mathf.Max(open, 1f - Smooth(PondRadius + 12f, PondRadius + 20f, Vector2.Distance(p, PondCenter)));
        open = Mathf.Max(open, 0.85f * (1f - Smooth(16f, 22f, Vector2.Distance(p, SpringCenter))));
        open = Mathf.Max(open, Glade(p, NorthEastGlade, 36f));
        open = Mathf.Max(open, Glade(p, OldField, 46f));
        open = Mathf.Max(open, Glade(p, CreekBottom, 30f));
        return open;
    }

    static float Glade(Vector2 p, Vector2 center, float radius) =>
        1f - Smooth(radius, radius + 8f, Vector2.Distance(p, center) + 6f * Noise(p, 1f / 20f, 12));

    // The point where the pasture gives way to forest, walking out from its centre; negative offset = pasture side.
    static Vector2 PastureEdge(Vector2 dir, float offset)
    {
        float r = 0f;
        while (r < Size && Pasture(PastureCenter + dir * r) >= 0.5f)
            r += 0.5f;
        return PastureCenter + dir * (r + offset);
    }

    static bool Blocked(Vector2 p, DistanceField creek, float[] creekHalf, DistanceField trail, DistanceField paths, Vector2[] sites)
    {
        int k = creek.Index(p);
        if (creek.Distance[k] < 8f && creek.Distance[k] < creekHalf[creek.Nearest[k]] + 1.6f)
            return true;
        if (trail.Distance[k] < 2.6f || paths.Distance[k] < 2.2f)
            return true;
        Vector2 fromPond = p - PondCenter;
        if (fromPond.magnitude < (fromPond.sqrMagnitude > 0.01f ? PondRadiusAt(fromPond.normalized) : PondRadius) + 2.5f)
            return true;
        if (Vector2.Distance(p, SpringCenter) < 7f)
            return true;
        foreach (Vector2 site in sites)
        {
            if (Vector2.Distance(p, site) < 5f)
                return true;
        }
        return false;
    }

    static float EdgeDistance(Vector2 p) =>
        Mathf.Min(Mathf.Min(p.x - Origin.x, Origin.x + Size - p.x), Mathf.Min(p.y - Origin.y, Origin.y + Size - p.y));

    static List<Vector2> WanderingPath(Vector2 from, Vector2 to, int seed)
    {
        float distance = Vector2.Distance(from, to);
        int segments = Mathf.Max(2, Mathf.RoundToInt(distance / 25f));
        Vector2 across = Vector2.Perpendicular((to - from).normalized);
        float amplitude = Mathf.Min(8f, distance * 0.06f);
        var control = new Vector2[segments + 1];
        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            float sway = Mathf.Sin(t * Mathf.PI) * amplitude * Noise(new Vector2(t * 3f, seed * 10f), 1f, 13);
            control[i] = Vector2.Lerp(from, to, t) + across * sway;
        }
        return CatmullRom(control, 1f);
    }

    static void MoveSite(string siteName, Vector2 position, Terrain terrain)
    {
        foreach (DiscoverySite site in Object.FindObjectsByType<DiscoverySite>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (site.name != siteName)
                continue;
            Undo.RecordObject(site.transform, "Build Property Terrain");
            site.transform.position = OnGround(position, terrain);
            return;
        }
        Debug.LogWarning($"[PropertyTerrainBuilder] No discovery site named '{siteName}' in the scene.");
    }

    static Vector3 OnGround(Vector2 p, Terrain terrain)
    {
        var world = new Vector3(p.x, 0f, p.y);
        world.y = terrain.SampleHeight(world) + terrain.transform.position.y;
        return world;
    }

    // --- Water -------------------------------------------------------------------------------------------------

    // Flat surfaces for the spring pool, creek and pond, each overlapping its banks so the edges hide under ground.
    static Mesh WaterMesh(List<Vector2> creek, float[] level, float[] half, float springLevel, float pondLevel)
    {
        var vertices = new List<Vector3>();
        var triangles = new List<int>();

        void Disc(Vector2 center, float y, System.Func<Vector2, float> radius)
        {
            int start = vertices.Count;
            vertices.Add(new Vector3(center.x, y, center.y));
            const int Segments = 64;
            for (int i = 0; i < Segments; i++)
            {
                Vector2 dir = Dir(i * 360f / Segments);
                Vector2 edge = center + dir * radius(dir);
                vertices.Add(new Vector3(edge.x, y, edge.y));
            }
            for (int i = 0; i < Segments; i++)
                triangles.AddRange(new[] { start, start + 1 + (i + 1) % Segments, start + 1 + i });
        }

        Disc(SpringCenter, springLevel, _ => 5.2f);
        Disc(PondCenter, pondLevel, dir => PondRadiusAt(dir) + 2f);

        int previous = -1;
        for (int i = 0; i < creek.Count; i++)
        {
            if (Vector2.Distance(creek[i], SpringCenter) < 4f)
                continue;
            if (Vector2.Distance(creek[i], PondCenter) < PondRadius * 0.7f)
                break;

            Vector2 forward = (creek[Mathf.Min(i + 1, creek.Count - 1)] - creek[Mathf.Max(i - 1, 0)]).normalized;
            Vector2 side = Vector2.Perpendicular(forward) * (half[i] + 0.6f);
            int start = vertices.Count;
            vertices.Add(new Vector3(creek[i].x - side.x, level[i], creek[i].y - side.y));
            vertices.Add(new Vector3(creek[i].x + side.x, level[i], creek[i].y + side.y));
            if (previous >= 0)
                triangles.AddRange(new[] { previous, previous + 1, start, start, previous + 1, start + 1 });
            previous = start;
        }

        var mesh = new Mesh { name = "Property Water", indexFormat = IndexFormat.UInt32 };
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        FaceUp(mesh);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return SaveAsset(mesh, ArtFolder + "/PropertyWater.asset");
    }

    // Flips any triangle facing down, so winding mistakes can't make water invisible from above.
    static void FaceUp(Mesh mesh)
    {
        Vector3[] v = mesh.vertices;
        int[] t = mesh.triangles;
        for (int i = 0; i < t.Length; i += 3)
        {
            if (Vector3.Cross(v[t[i + 1]] - v[t[i]], v[t[i + 2]] - v[t[i]]).y < 0f)
                (t[i + 1], t[i + 2]) = (t[i + 2], t[i + 1]);
        }
        mesh.triangles = t;
    }

    // --- Trees -------------------------------------------------------------------------------------------------

    // Low-poly placeholder hardwoods (bark + leaves) and a leafy understory shrub. Hardwood trunks get a collider,
    // which the terrain uses for its tree colliders; shrubs can be walked through.
    static GameObject TreePrefab(string prefabName, int kind)
    {
        var random = new System.Random(Seed + 100 + kind);
        var builder = new MeshBuilder(2);
        switch (kind)
        {
            case 0:
                builder.Trunk(6.5f, 0.3f, 0.2f);
                builder.Blob(new Vector3(0f, 8.6f, 0f), 3.8f, new Vector3(1f, 0.8f, 1f), 0.35f, random);
                builder.Blob(new Vector3(1.4f, 10f, 0.6f), 2.5f, new Vector3(1f, 0.85f, 1f), 0.3f, random);
                builder.Blob(new Vector3(-1.2f, 9.6f, -1f), 2.3f, new Vector3(1f, 0.85f, 1f), 0.3f, random);
                break;
            case 1:
                builder.Trunk(9.5f, 0.27f, 0.16f);
                builder.Blob(new Vector3(0f, 11.8f, 0f), 2.7f, new Vector3(1f, 1.7f, 1f), 0.3f, random);
                builder.Blob(new Vector3(0.6f, 9.2f, 0.4f), 2.1f, new Vector3(1f, 1f, 1f), 0.3f, random);
                break;
            default:
                builder.Blob(new Vector3(0f, 0.8f, 0f), 1.3f, new Vector3(1f, 0.75f, 1f), 0.3f, random);
                builder.Blob(new Vector3(0.9f, 0.6f, 0.3f), 0.9f, new Vector3(1f, 0.8f, 1f), 0.3f, random);
                builder.Blob(new Vector3(-0.7f, 0.65f, -0.5f), 1f, new Vector3(1f, 0.8f, 1f), 0.3f, random);
                break;
        }

        Mesh mesh = SaveAsset(builder.ToMesh(prefabName), $"{ArtFolder}/{prefabName}.asset");
        Material bark = LitMaterial("Bark", new Color(0.27f, 0.2f, 0.14f), 0.1f);
        Material leaves = kind == 2
            ? LitMaterial("Shrub Leaves", new Color(0.2f, 0.28f, 0.11f), 0.15f)
            : LitMaterial("Leaves", new Color(0.19f, 0.33f, 0.12f), 0.15f);

        var go = new GameObject(prefabName);
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        var renderer = go.AddComponent<MeshRenderer>();
        renderer.sharedMaterials = new[] { bark, leaves };
        // A LODGroup makes the terrain draw this as a plain mesh tree (no Soft Occlusion billboards).
        go.AddComponent<LODGroup>().SetLODs(new[] { new LOD(0.004f, new Renderer[] { renderer }) });
        if (kind != 2)
        {
            var trunk = go.AddComponent<CapsuleCollider>();
            float height = kind == 0 ? 6.5f : 9.5f;
            trunk.radius = kind == 0 ? 0.3f : 0.27f;
            trunk.height = height;
            trunk.center = new Vector3(0f, height / 2f, 0f);
        }

        string path = $"{PrefabFolder}/{prefabName}.prefab";
        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);

        // The in-memory prefab can keep a stale mesh reference until reimported, which leaves the terrain's
        // trees invisible even though the file on disk is correct.
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        return AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }

    sealed class MeshBuilder
    {
        readonly List<Vector3> vertices = new List<Vector3>();
        readonly List<Vector3> normals = new List<Vector3>();
        readonly List<int>[] submeshes;

        public MeshBuilder(int submeshCount)
        {
            submeshes = new List<int>[submeshCount];
            for (int i = 0; i < submeshCount; i++)
                submeshes[i] = new List<int>();
        }

        // Flat-shaded triangle, wound to face away from `inside`.
        void Triangle(Vector3 a, Vector3 b, Vector3 c, Vector3 inside, int submesh)
        {
            Vector3 normal = Vector3.Cross(b - a, c - a).normalized;
            if (Vector3.Dot(normal, (a + b + c) / 3f - inside) < 0f)
            {
                (b, c) = (c, b);
                normal = -normal;
            }
            foreach (Vector3 v in new[] { a, b, c })
            {
                submeshes[submesh].Add(vertices.Count);
                vertices.Add(v);
                normals.Add(normal);
            }
        }

        public void Trunk(float height, float bottomRadius, float topRadius)
        {
            const int Sides = 7;
            for (int i = 0; i < Sides; i++)
            {
                Vector2 d0 = Dir(i * 360f / Sides), d1 = Dir((i + 1) * 360f / Sides);
                var b0 = new Vector3(d0.x * bottomRadius, -0.3f, d0.y * bottomRadius);
                var b1 = new Vector3(d1.x * bottomRadius, -0.3f, d1.y * bottomRadius);
                var t0 = new Vector3(d0.x * topRadius, height, d0.y * topRadius);
                var t1 = new Vector3(d1.x * topRadius, height, d1.y * topRadius);
                Triangle(b0, b1, t1, new Vector3(0f, height / 2f, 0f), 0);
                Triangle(b0, t1, t0, new Vector3(0f, height / 2f, 0f), 0);
            }
        }

        // A lumpy, once-subdivided icosphere.
        public void Blob(Vector3 center, float radius, Vector3 scale, float jitter, System.Random random)
        {
            Icosphere(out List<Vector3> points, out List<int> faces);
            for (int i = 0; i < points.Count; i++)
            {
                float lump = 1f + jitter * ((float)random.NextDouble() * 2f - 1f);
                points[i] = center + Vector3.Scale(points[i] * radius * lump, scale);
            }
            for (int i = 0; i < faces.Count; i += 3)
                Triangle(points[faces[i]], points[faces[i + 1]], points[faces[i + 2]], center, submeshes.Length - 1);
        }

        public Mesh ToMesh(string meshName)
        {
            var mesh = new Mesh { name = meshName };
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.subMeshCount = submeshes.Length;
            for (int i = 0; i < submeshes.Length; i++)
                mesh.SetTriangles(submeshes[i], i);
            mesh.RecalculateBounds();
            return mesh;
        }
    }

    static void Icosphere(out List<Vector3> points, out List<int> faces)
    {
        float t = (1f + Mathf.Sqrt(5f)) / 2f;
        points = new List<Vector3>
        {
            new Vector3(-1, t, 0), new Vector3(1, t, 0), new Vector3(-1, -t, 0), new Vector3(1, -t, 0),
            new Vector3(0, -1, t), new Vector3(0, 1, t), new Vector3(0, -1, -t), new Vector3(0, 1, -t),
            new Vector3(t, 0, -1), new Vector3(t, 0, 1), new Vector3(-t, 0, -1), new Vector3(-t, 0, 1),
        };
        int[] ico =
        {
            0, 11, 5, 0, 5, 1, 0, 1, 7, 0, 7, 10, 0, 10, 11, 1, 5, 9, 5, 11, 4, 11, 10, 2, 10, 7, 6, 7, 1, 8,
            3, 9, 4, 3, 4, 2, 3, 2, 6, 3, 6, 8, 3, 8, 9, 4, 9, 5, 2, 4, 11, 6, 2, 10, 8, 6, 7, 9, 8, 1,
        };

        var midpoints = new Dictionary<long, int>();
        var pointList = points;
        int Midpoint(int a, int b)
        {
            long key = ((long)Mathf.Min(a, b) << 32) | (uint)Mathf.Max(a, b);
            if (!midpoints.TryGetValue(key, out int index))
            {
                index = pointList.Count;
                pointList.Add((pointList[a] + pointList[b]) / 2f);
                midpoints[key] = index;
            }
            return index;
        }

        faces = new List<int>();
        for (int i = 0; i < ico.Length; i += 3)
        {
            int a = ico[i], b = ico[i + 1], c = ico[i + 2];
            int ab = Midpoint(a, b), bc = Midpoint(b, c), ca = Midpoint(c, a);
            faces.AddRange(new[] { a, ab, ca, b, bc, ab, c, ca, bc, ab, bc, ca });
        }
        for (int i = 0; i < points.Count; i++)
            points[i] = points[i].normalized;
    }

    // --- Textures and materials --------------------------------------------------------------------------------

    const int TextureSize = 512;

    static Texture2D GrassTexture() => PaintTexture("Grass", (u, v) =>
    {
        float body = Fbm(u, v, 4, 5, 1);
        float blades = TileNoise(u, v, 128, 2);
        float dry = Fbm(u, v, 3, 3, 3);
        Color c = Color.Lerp(new Color(0.18f, 0.27f, 0.1f), new Color(0.3f, 0.42f, 0.17f), body * 0.7f + blades * 0.3f);
        return Color.Lerp(c, new Color(0.42f, 0.43f, 0.22f), Smooth(0.6f, 0.8f, dry) * 0.6f);
    });

    static Texture2D DirtTexture() => PaintTexture("Dirt", (u, v) =>
    {
        float body = Fbm(u, v, 4, 5, 4);
        Cellular(u, v, 40, 5, out float f1, out _, out int id);
        Color c = Color.Lerp(new Color(0.24f, 0.17f, 0.11f), new Color(0.38f, 0.29f, 0.19f), body);
        return f1 < 0.12f && Hash(id, 0, 6) > 0.6f ? Color.Lerp(c, new Color(0.45f, 0.4f, 0.33f), 0.6f) : c;
    });

    static Texture2D GravelTexture() => PaintTexture("Gravel", (u, v) =>
    {
        Cellular(u, v, 24, 7, out float f1, out float f2, out int id);
        float tone = Hash(id, 1, 8);
        Color stone = Color.Lerp(new Color(0.33f, 0.32f, 0.3f), new Color(0.55f, 0.53f, 0.49f), tone);
        stone *= 0.85f + 0.3f * TileNoise(u, v, 96, 9);
        return Color.Lerp(new Color(0.16f, 0.15f, 0.13f), stone, Smooth(0.02f, 0.09f, f2 - f1));
    });

    static Texture2D PaintTexture(string textureName, System.Func<float, float, Color> paint)
    {
        var texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGB24, false);
        var pixels = new Color[TextureSize * TextureSize];
        for (int y = 0; y < TextureSize; y++)
        for (int x = 0; x < TextureSize; x++)
            pixels[y * TextureSize + x] = paint((x + 0.5f) / TextureSize, (y + 0.5f) / TextureSize);
        texture.SetPixels(pixels);

        string path = $"{ArtFolder}/Textures/{textureName}.png";
        File.WriteAllBytes(path, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(path);

        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.wrapMode = TextureWrapMode.Repeat;
        importer.sRGBTexture = true;
        importer.mipmapEnabled = true;
        importer.maxTextureSize = TextureSize;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    static TerrainLayer MakeLayer(string layerName, Texture2D texture, float tileSize)
    {
        string path = $"{ArtFolder}/{layerName}.terrainlayer";
        var layer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(path);
        if (layer == null)
        {
            layer = new TerrainLayer();
            AssetDatabase.CreateAsset(layer, path);
        }
        layer.diffuseTexture = texture;
        layer.tileSize = new Vector2(tileSize, tileSize);
        layer.smoothness = 0f;
        layer.metallic = 0f;
        EditorUtility.SetDirty(layer);
        return layer;
    }

    // A project copy of HDRP's default terrain material, so terrain settings never write into the package.
    static Material TerrainMaterial()
    {
        string path = ArtFolder + "/Property Terrain.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null && GraphicsSettings.defaultRenderPipeline != null)
        {
            material = new Material(GraphicsSettings.defaultRenderPipeline.defaultTerrainMaterial);
            AssetDatabase.CreateAsset(material, path);
        }
        return material;
    }

    static Material LitMaterial(string materialName, Color color, float smoothness)
    {
        string path = $"{ArtFolder}/{materialName}.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("HDRP/Lit"));
            AssetDatabase.CreateAsset(material, path);
        }
        material.SetColor("_BaseColor", color);
        material.SetFloat("_Smoothness", smoothness);
        material.enableInstancing = true;
        HDMaterial.ValidateMaterial(material);
        EditorUtility.SetDirty(material);
        return material;
    }

    // --- Helpers -----------------------------------------------------------------------------------------------

    // Nearest distance from each grid point to a polyline, and which polyline point is closest.
    sealed class DistanceField
    {
        public readonly float[] Distance;
        public readonly int[] Nearest;
        readonly int res;
        readonly float cell, offset;

        public DistanceField(int res, float cell, float offset)
        {
            this.res = res;
            this.cell = cell;
            this.offset = offset;
            Distance = new float[res * res];
            Nearest = new int[res * res];
            for (int i = 0; i < Distance.Length; i++)
                Distance[i] = float.MaxValue;
        }

        public Vector2 Position(int x, int z) => Origin + new Vector2((x + offset) * cell, (z + offset) * cell);

        public int Index(Vector2 p)
        {
            int x = Mathf.Clamp((int)((p.x - Origin.x) / cell - offset + 0.5f), 0, res - 1);
            int z = Mathf.Clamp((int)((p.y - Origin.y) / cell - offset + 0.5f), 0, res - 1);
            return z * res + x;
        }

        public void Stamp(List<Vector2> line, float reach)
        {
            for (int i = 0; i + 1 < line.Count; i++)
            {
                Vector2 a = line[i], b = line[i + 1];
                Vector2 min = Vector2.Min(a, b) - Vector2.one * reach, max = Vector2.Max(a, b) + Vector2.one * reach;
                int x0 = Mathf.Max(0, (int)((min.x - Origin.x) / cell - offset));
                int x1 = Mathf.Min(res - 1, (int)((max.x - Origin.x) / cell - offset) + 1);
                int z0 = Mathf.Max(0, (int)((min.y - Origin.y) / cell - offset));
                int z1 = Mathf.Min(res - 1, (int)((max.y - Origin.y) / cell - offset) + 1);
                Vector2 ab = b - a;
                float lengthSq = Mathf.Max(ab.sqrMagnitude, 1e-6f);
                for (int z = z0; z <= z1; z++)
                for (int x = x0; x <= x1; x++)
                {
                    Vector2 p = Position(x, z);
                    float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / lengthSq);
                    float d = Vector2.Distance(p, a + ab * t);
                    int k = z * res + x;
                    if (d < Distance[k])
                    {
                        Distance[k] = d;
                        Nearest[k] = t < 0.5f ? i : i + 1;
                    }
                }
            }
        }
    }

    static List<Vector2> CatmullRom(IList<Vector2> control, float step)
    {
        var points = new List<Vector2> { control[0] };
        for (int i = 0; i + 1 < control.Count; i++)
        {
            Vector2 p0 = control[Mathf.Max(i - 1, 0)], p1 = control[i], p2 = control[i + 1];
            Vector2 p3 = control[Mathf.Min(i + 2, control.Count - 1)];
            int samples = Mathf.Max(1, Mathf.CeilToInt(Vector2.Distance(p1, p2) / step));
            for (int s = 1; s <= samples; s++)
            {
                float t = s / (float)samples;
                points.Add(0.5f * (2f * p1 + (p2 - p0) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * (t * t) +
                                   (3f * p1 - p0 - 3f * p2 + p3) * (t * t * t)));
            }
        }
        return points;
    }

    // Perlin noise in -1..1.
    static float Noise(Vector2 p, float frequency, int channel)
    {
        Vector2 o = noiseOffsets[channel % noiseOffsets.Length];
        return Mathf.PerlinNoise(p.x * frequency + o.x, p.y * frequency + o.y) * 2f - 1f;
    }

    static float Smooth(float from, float to, float value) => Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(from, to, value));

    // Smooth dome: 1 at the centre, 0 at `radius`.
    static float Bump(float distance, float radius) =>
        distance >= radius ? 0f : 0.5f + 0.5f * Mathf.Cos(Mathf.PI * distance / radius);

    static Vector2 Dir(float degrees) => new Vector2(Mathf.Cos(degrees * Mathf.Deg2Rad), Mathf.Sin(degrees * Mathf.Deg2Rad));

    static Vector2 Rotate(Vector2 v, float degrees)
    {
        float c = Mathf.Cos(degrees * Mathf.Deg2Rad), s = Mathf.Sin(degrees * Mathf.Deg2Rad);
        return new Vector2(v.x * c - v.y * s, v.x * s + v.y * c);
    }

    // Tileable noise for the terrain textures, 0..1.
    static float Hash(int x, int y, int seed)
    {
        unchecked
        {
            uint h = (uint)(x * 374761393 + y * 668265263 + (seed + Seed) * 982451653);
            h = (h ^ (h >> 13)) * 1274126177u;
            h ^= h >> 16;
            return (h & 0xFFFFFF) / 16777216f;
        }
    }

    static float TileNoise(float u, float v, int period, int seed)
    {
        float x = u * period, y = v * period;
        int x0 = Mathf.FloorToInt(x), y0 = Mathf.FloorToInt(y);
        float fx = Mathf.SmoothStep(0f, 1f, x - x0), fy = Mathf.SmoothStep(0f, 1f, y - y0);
        float Corner(int cx, int cy) => Hash(((cx % period) + period) % period, ((cy % period) + period) % period, seed);
        return Mathf.Lerp(Mathf.Lerp(Corner(x0, y0), Corner(x0 + 1, y0), fx),
                          Mathf.Lerp(Corner(x0, y0 + 1), Corner(x0 + 1, y0 + 1), fx), fy);
    }

    static float Fbm(float u, float v, int basePeriod, int octaves, int seed)
    {
        float sum = 0f, amplitude = 0.5f, total = 0f;
        for (int i = 0; i < octaves; i++)
        {
            sum += TileNoise(u, v, basePeriod << i, seed + i) * amplitude;
            total += amplitude;
            amplitude *= 0.5f;
        }
        return sum / total;
    }

    // Distances to the nearest and second-nearest scattered point (wrapping), and the nearest point's cell id.
    static void Cellular(float u, float v, int period, int seed, out float f1, out float f2, out int id)
    {
        float x = u * period, y = v * period;
        int cx = Mathf.FloorToInt(x), cy = Mathf.FloorToInt(y);
        f1 = f2 = float.MaxValue;
        id = 0;
        for (int dy = -1; dy <= 1; dy++)
        for (int dx = -1; dx <= 1; dx++)
        {
            int wx = ((cx + dx) % period + period) % period, wy = ((cy + dy) % period + period) % period;
            float px = cx + dx + Hash(wx, wy, seed), py = cy + dy + Hash(wx, wy, seed + 1);
            float d = Mathf.Sqrt((px - x) * (px - x) + (py - y) * (py - y));
            if (d < f1)
            {
                f2 = f1;
                f1 = d;
                id = wy * period + wx;
            }
            else if (d < f2)
            {
                f2 = d;
            }
        }
    }

    // Overwrites an existing asset in place, so prefabs and scene objects already pointing at it stay linked.
    static T SaveAsset<T>(T asset, string path) where T : Object
    {
        var existing = AssetDatabase.LoadAssetAtPath<T>(path);
        if (existing == null)
        {
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }
        EditorUtility.CopySerialized(asset, existing);
        existing.name = Path.GetFileNameWithoutExtension(path);
        EditorUtility.SetDirty(existing);
        Object.DestroyImmediate(asset);
        return existing;
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;
        string parent = Path.GetDirectoryName(path).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }
}
