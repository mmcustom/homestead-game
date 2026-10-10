using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

// Homestead > Place Clay Banks: visible patches of wet clay along the creek and pond banks in the open World scene
// (Building_Housing_System.md's Clay; Mike couldn't find any on 2026-10-09 because nothing marked a bank). The Shovel
// already digs Clay anywhere on dry ground within 3 m of water (AxeTool.IsCreekBank); these patches only show where,
// and dig the same way. They never run out, so there's no manager and nothing to save.
//
// The shoreline is found by sampling the terrain on a grid for water (Animal.IsOverWater); every dry sample within
// BankBand of water is a candidate. Candidates are shuffled with a fixed seed and kept if no other patch is within
// Spacing. Each patch is a ClayBank with a stable id (1, 2, 3...). Re-running replaces the previous set with the same
// layout, so the ids don't change. Patches are a low irregular mound of generated mesh saved under Assets/Art/Clay in
// a grey-orange, glossy wet-clay material. Run it with the World scene open, then save the scene (File > Save).
public static class ClayBankPlacer
{
    const string RootName = "Clay Banks";
    const string ArtFolder = "Assets/Art/Clay";
    const int Seed = 20261009;

    const float GridStep = 1.5f;     // metres between samples
    const float BankBand = 3f;       // a dry sample within this of water is bank (matches AxeTool.bankReach)
    const float Spacing = 4f;        // between patches
    const float MaxSlope = 30f;
    const int MaxPatches = 1000;

    [MenuItem("Homestead/Place Clay Banks")]
    public static void Place()
    {
        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null)
        {
            Debug.LogError("[ClayBankPlacer] Needs the property terrain in the open scene.");
            return;
        }

        GameObject old = GameObject.Find(RootName);
        if (old != null)
            Undo.DestroyObjectImmediate(old);
        if (!AssetDatabase.IsValidFolder(ArtFolder))
            AssetDatabase.CreateFolder("Assets/Art", "Clay");
        Mesh[] meshes = { PatchMesh("Clay Patch A", 1), PatchMesh("Clay Patch B", 2), PatchMesh("Clay Patch C", 3) };
        Material clay = ClayMaterial();

        var root = new GameObject(RootName);
        Undo.RegisterCreatedObjectUndo(root, "Place Clay Banks");
        var random = new System.Random(Seed);

        List<Vector3> candidates = FindBank(terrain, out int waterSamples);
        // A fixed shuffle, so every run lays out the same patches in the same order.
        for (int i = candidates.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
        }

        var placed = new List<Vector3>();
        var cells = new Dictionary<Vector2Int, List<Vector3>>();
        foreach (Vector3 spot in candidates)
        {
            if (placed.Count >= MaxPatches)
                break;
            Vector3 p = spot + new Vector3(((float)random.NextDouble() - 0.5f) * GridStep, 0f, ((float)random.NextDouble() - 0.5f) * GridStep);
            if (!Usable(terrain, ref p) || Crowded(cells, p))
                continue;
            Add(cells, p);
            placed.Add(p);
            Create(terrain, root.transform, random, p, meshes, clay, placed.Count);
        }

        EditorSceneManager.MarkSceneDirty(root.scene);
        AssetDatabase.SaveAssets();
        Debug.Log($"[ClayBankPlacer] {waterSamples} water samples, {candidates.Count} bank candidates; placed {placed.Count} clay patches " +
                  $"(at least {Spacing} m apart). Save the scene (File > Save) to keep them.");
    }

    // Dry, gentle ground within BankBand of water, one point per grid cell (the water mask is sampled once per cell, so
    // the "near water" test is a lookup, not a fan of raycasts).
    public static List<Vector3> FindBank(Terrain terrain, out int waterSamples)
    {
        TerrainData data = terrain.terrainData;
        Vector3 origin = terrain.transform.position;
        int nx = Mathf.FloorToInt(data.size.x / GridStep), nz = Mathf.FloorToInt(data.size.z / GridStep);
        var water = new bool[nx, nz];
        waterSamples = 0;
        for (int z = 0; z < nz; z++)
        for (int x = 0; x < nx; x++)
        {
            var p = new Vector3(origin.x + (x + 0.5f) * GridStep, 0f, origin.z + (z + 0.5f) * GridStep);
            p.y = terrain.SampleHeight(p) + origin.y;
            if (Animal.IsOverWater(p, out _))
            {
                water[x, z] = true;
                waterSamples++;
            }
        }

        int reach = Mathf.CeilToInt(BankBand / GridStep);
        var found = new List<Vector3>();
        for (int z = 0; z < nz; z++)
        for (int x = 0; x < nx; x++)
        {
            if (water[x, z] || !WaterWithin(water, nx, nz, x, z, reach))
                continue;
            var p = new Vector3(origin.x + (x + 0.5f) * GridStep, 0f, origin.z + (z + 0.5f) * GridStep);
            if (Usable(terrain, ref p))
                found.Add(p);
        }
        return found;
    }

    static bool WaterWithin(bool[,] water, int nx, int nz, int x, int z, int reach)
    {
        for (int dz = -reach; dz <= reach; dz++)
        for (int dx = -reach; dx <= reach; dx++)
        {
            int ax = x + dx, az = z + dz;
            if (ax >= 0 && az >= 0 && ax < nx && az < nz && water[ax, az] && dx * dx + dz * dz <= reach * reach)
                return true;
        }
        return false;
    }

    // Dry, gentle ground, and not inside anything solid; snaps p to the ground.
    static bool Usable(Terrain terrain, ref Vector3 p)
    {
        TerrainData data = terrain.terrainData;
        Vector3 origin = terrain.transform.position;
        float u = (p.x - origin.x) / data.size.x, v = (p.z - origin.z) / data.size.z;
        if (u < 0.03f || u > 0.97f || v < 0.03f || v > 0.97f || data.GetSteepness(u, v) > MaxSlope)
            return false;
        p.y = origin.y + data.GetInterpolatedHeight(u, v);
        return !Animal.IsOverWater(p, out _) && !Physics.CheckSphere(p + Vector3.up * 0.5f, 0.5f, ~0, QueryTriggerInteraction.Ignore);
    }

    static Vector2Int Cell(Vector3 p) => new Vector2Int(Mathf.FloorToInt(p.x / Spacing), Mathf.FloorToInt(p.z / Spacing));

    static void Add(Dictionary<Vector2Int, List<Vector3>> cells, Vector3 p)
    {
        Vector2Int c = Cell(p);
        if (!cells.TryGetValue(c, out List<Vector3> list))
            cells[c] = list = new List<Vector3>();
        list.Add(p);
    }

    static bool Crowded(Dictionary<Vector2Int, List<Vector3>> cells, Vector3 p)
    {
        Vector2Int c = Cell(p);
        for (int dz = -1; dz <= 1; dz++)
        for (int dx = -1; dx <= 1; dx++)
            if (cells.TryGetValue(new Vector2Int(c.x + dx, c.y + dz), out List<Vector3> list))
                foreach (Vector3 other in list)
                    if (new Vector2(other.x - p.x, other.z - p.z).sqrMagnitude < Spacing * Spacing)
                        return true;
        return false;
    }

    static void Create(Terrain terrain, Transform root, System.Random random, Vector3 p, Mesh[] meshes, Material material, int id)
    {
        var go = new GameObject($"Clay Bank {id}");
        go.transform.SetParent(root);
        // Lying on the ground's slope, turned a random way round, a hair above it.
        TerrainData data = terrain.terrainData;
        Vector3 up = data.GetInterpolatedNormal((p.x - terrain.transform.position.x) / data.size.x,
                                                (p.z - terrain.transform.position.z) / data.size.z);
        go.transform.position = p + up * 0.02f;
        go.transform.rotation = Quaternion.AngleAxis((float)random.NextDouble() * 360f, up) * Quaternion.FromToRotation(Vector3.up, up);
        float scale = 0.85f + (float)random.NextDouble() * 0.5f;
        go.transform.localScale = new Vector3(scale, 1f, scale);
        go.AddComponent<MeshFilter>().sharedMesh = meshes[random.Next(meshes.Length)];
        go.AddComponent<MeshRenderer>().sharedMaterial = material;
        // Low and broad, so it's easy to look at and walk over.
        var box = go.AddComponent<BoxCollider>();
        box.size = new Vector3(2.2f, 0.16f, 2.2f);
        box.center = new Vector3(0f, 0.04f, 0f);
        go.AddComponent<ClayBank>().Setup(id);
    }

    // Wet clay: grey with an orange cast, a little glossy.
    static Material ClayMaterial()
    {
        string path = $"{ArtFolder}/Wet Clay.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("HDRP/Lit"));
            AssetDatabase.CreateAsset(material, path);
        }
        material.SetColor("_BaseColor", new Color(0.56f, 0.43f, 0.34f));
        material.SetFloat("_Smoothness", 0.55f);
        material.enableInstancing = true;
        HDMaterial.ValidateMaterial(material);
        EditorUtility.SetDirty(material);
        return material;
    }

    // An irregular low mound about 2 m across: a raised centre, a ring, and an outer rim tucked just under the ground.
    static Mesh PatchMesh(string name, int seed)
    {
        const int Sides = 14;
        var random = new System.Random(Seed + seed);
        var verts = new List<Vector3> { new Vector3(0f, 0.07f, 0f) };
        float[] radius = { 0.55f, 1.0f };
        float[] height = { 0.05f, -0.02f };
        for (int ring = 0; ring < 2; ring++)
            for (int i = 0; i < Sides; i++)
            {
                float angle = i / (float)Sides * Mathf.PI * 2f;
                float r = radius[ring] * (0.78f + (float)random.NextDouble() * 0.3f);
                verts.Add(new Vector3(Mathf.Cos(angle) * r, height[ring], Mathf.Sin(angle) * r));
            }

        var tris = new List<int>();
        for (int i = 0; i < Sides; i++)
        {
            int n = (i + 1) % Sides;
            int inner0 = 1 + i, inner1 = 1 + n, outer0 = 1 + Sides + i, outer1 = 1 + Sides + n;
            tris.AddRange(new[] { 0, inner1, inner0 }); // clockwise from above
            tris.AddRange(new[] { inner0, inner1, outer0, outer0, inner1, outer1 });
        }
        return SaveMesh(name, verts, tris);
    }

    // Flat shading (every triangle its own vertices), saved as a mesh asset so the scene doesn't carry the geometry.
    static Mesh SaveMesh(string name, List<Vector3> verts, List<int> tris)
    {
        string path = $"{ArtFolder}/{name}.asset";
        var flatVerts = new Vector3[tris.Count];
        var flatTris = new int[tris.Count];
        for (int i = 0; i < tris.Count; i++)
        {
            flatVerts[i] = verts[tris[i]];
            flatTris[i] = i;
        }
        var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        bool created = mesh == null;
        if (created)
            mesh = new Mesh();
        mesh.Clear();
        mesh.name = name;
        mesh.vertices = flatVerts;
        mesh.triangles = flatTris;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        if (created)
            AssetDatabase.CreateAsset(mesh, path);
        else
            EditorUtility.SetDirty(mesh);
        return mesh;
    }
}
