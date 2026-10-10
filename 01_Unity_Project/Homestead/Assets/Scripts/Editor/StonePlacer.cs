using System.Collections.Generic;
using UnityEditor;
using UnityEngine.Rendering.HighDefinition;
using UnityEditor.SceneManagement;
using UnityEngine;

// Homestead > Place Stone: Stone_Gathering_System.md's stone, in the open World scene.
//   Loose stones — an even scatter across the whole property (about one every 15 m) and a denser band along the
//   creek and pond banks, each a hand-pickup LooseStone worth one Stone, with a stable id for the save.
//   South Ridge's rock outcrop — a cluster of big boulders on the steepest ground of the ridge's south face, clear of
//   the cabin site, the new-game spawn point and trees, carrying RockDeposit (mined with the Stone Pick Axe), with a scatter of loose stones at its foot.
// Rock shapes are generated low-poly meshes saved under Assets/Art/Rocks. Re-running replaces the previous set, with
// the same layout (fixed seed) and so the same ids. The original layout (150 scattered, 90 along the water, the
// outcrop's 10) comes first with its ids, and the 2026-10-07 density increase (Stone_Gathering_System.md) adds a second
// batch after it with ids from 1001 (scatter) and 2001 (water), so a save made before it still hides the right stones.
public static class StonePlacer
{
    const string StonesRoot = "Stones";
    const string OutcropName = "Rock Outcrop";
    const string ArtFolder = "Assets/Art/Rocks";
    const int Seed = 20260926;

    const int BaseCount = 150;
    const float BaseSpacing = 16f;
    const int BankCount = 90;
    const float BankSpacing = 5f;
    const int OutcropLooseCount = 10;
    const int ExtraBaseCount = 150;
    const float ExtraBaseSpacing = 11f;
    const int ExtraBaseIdStart = 1000;
    const int ExtraBankCount = 90;
    const float ExtraBankSpacing = 3.5f;
    const int ExtraBankIdStart = 2000;

    // Property_Layout.md's South Ridge high point and cabin site (PropertyTerrainBuilder's coordinates).
    static readonly Vector2 RidgeCenter = new Vector2(12f, -80f);
    static readonly Vector2 CabinSite = new Vector2(18f, -68f);
    static readonly Vector2 PlayerSpawn = new Vector2(20f, -40f);

    [MenuItem("Homestead/Place Stone")]
    public static void Place()
    {
        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null)
        {
            Debug.LogError("[StonePlacer] Needs the property terrain in the open scene.");
            return;
        }

        foreach (string name in new[] { StonesRoot, OutcropName })
        {
            GameObject old = GameObject.Find(name);
            if (old != null)
                Undo.DestroyObjectImmediate(old);
        }

        if (!AssetDatabase.IsValidFolder(ArtFolder))
            AssetDatabase.CreateFolder("Assets/Art", "Rocks");
        Mesh[] meshes = { RockMesh("Rock A", 1), RockMesh("Rock B", 2), RockMesh("Rock C", 3) };
        Material stone = StoneMaterial();

        var random = new System.Random(Seed);
        var root = new GameObject(StonesRoot);
        Undo.RegisterCreatedObjectUndo(root, "Place Stone");
        var placed = new List<Vector3>();
        int id = 1;

        TerrainData data = terrain.terrainData;
        Vector3 origin = terrain.transform.position;

        // The whole property, lightly.
        for (int attempt = 0; attempt < 6000 && placed.Count < BaseCount; attempt++)
        {
            float u = 0.04f + (float)random.NextDouble() * 0.92f, v = 0.04f + (float)random.NextDouble() * 0.92f;
            Vector3 p = origin + new Vector3(u * data.size.x, 0f, v * data.size.z);
            if (!Usable(terrain, ref p, 30f) || Crowded(placed, p, BaseSpacing))
                continue;
            placed.Add(p);
            CreateStone(root.transform, p, meshes, stone, random, id++, 0.26f, 0.42f);
        }
        int baseCount = placed.Count;

        // Denser along the water: shore points found by walking a grid, then stones set a little way up the bank.
        var shore = new List<Vector3>();
        for (float z = origin.z + 8f; z < origin.z + data.size.z - 8f; z += 3f)
        for (float x = origin.x + 8f; x < origin.x + data.size.x - 8f; x += 3f)
        {
            var p = new Vector3(x, 0f, z);
            p.y = terrain.SampleHeight(p) + origin.y;
            if (!Animal.IsOverWater(p, out _))
                continue;
            foreach (Vector3 step in new[] { Vector3.right, Vector3.left, Vector3.forward, Vector3.back })
            {
                Vector3 q = p + step * 3f;
                q.y = terrain.SampleHeight(q) + origin.y;
                if (!Animal.IsOverWater(q, out _))
                {
                    shore.Add(q);
                    break;
                }
            }
        }
        var bank = new List<Vector3>();
        for (int attempt = 0; attempt < 4000 && bank.Count < BankCount && shore.Count > 0; attempt++)
        {
            Vector3 p = shore[random.Next(shore.Count)];
            float angle = (float)random.NextDouble() * Mathf.PI * 2f;
            p += new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * (0.5f + (float)random.NextDouble() * 3.5f);
            if (!Usable(terrain, ref p, 35f) || Crowded(bank, p, BankSpacing) || Crowded(placed, p, 2.5f))
                continue;
            bank.Add(p);
            CreateStone(root.transform, p, meshes, stone, random, id++, 0.24f, 0.4f);
        }
        placed.AddRange(bank);

        // South Ridge's outcrop on the steepest ground near the ridge top, clear of the cabin site and trees.
        Vector3 outcrop = FindOutcropSite(terrain, random);
        var outcropRoot = new GameObject(OutcropName);
        Undo.RegisterCreatedObjectUndo(outcropRoot, "Place Stone");
        outcropRoot.transform.position = outcrop;
        Vector3 downhill = Downhill(terrain, outcrop);
        outcropRoot.transform.rotation = Quaternion.LookRotation(downhill);
        outcropRoot.AddComponent<RockDeposit>();
        for (int i = 0; i < 8; i++)
        {
            // Boulders in a rough band across the slope, the biggest in the middle, half-buried.
            float across = (i - 3.5f) * 1.5f + ((float)random.NextDouble() - 0.5f) * 0.8f;
            float along = ((float)random.NextDouble() - 0.5f) * 1.5f;
            float size = Mathf.Lerp(3.2f, 1.5f, Mathf.Abs(i - 3.5f) / 3.5f) * (0.85f + (float)random.NextDouble() * 0.3f);
            Vector3 p = outcrop + outcropRoot.transform.right * across + downhill * along;
            p.y = terrain.SampleHeight(p) + origin.y - size * 0.3f;
            var boulder = new GameObject($"Boulder {i + 1}");
            boulder.transform.SetParent(outcropRoot.transform);
            boulder.transform.position = p;
            boulder.transform.rotation = Quaternion.Euler((float)random.NextDouble() * 30f, (float)random.NextDouble() * 360f, (float)random.NextDouble() * 30f);
            boulder.transform.localScale = new Vector3(size, size * (0.7f + (float)random.NextDouble() * 0.4f), size);
            Mesh mesh = meshes[random.Next(meshes.Length)];
            boulder.AddComponent<MeshFilter>().sharedMesh = mesh;
            boulder.AddComponent<MeshRenderer>().sharedMaterial = stone;
            boulder.AddComponent<MeshCollider>().sharedMesh = mesh;
            GameObjectUtility.SetStaticEditorFlags(boulder, StaticEditorFlags.BatchingStatic);
        }
        // Loose stone broken off at its foot — hand pickup like any other.
        int foot = 0;
        for (int attempt = 0; attempt < 200 && foot < OutcropLooseCount; attempt++)
        {
            float angle = (float)random.NextDouble() * Mathf.PI * 2f;
            Vector3 p = outcrop + downhill * 4f + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * (1f + (float)random.NextDouble() * 5f);
            if (!Usable(terrain, ref p, 40f) || Crowded(placed, p, 1.8f))
                continue;
            placed.Add(p);
            CreateStone(root.transform, p, meshes, stone, random, id++, 0.26f, 0.4f);
            foot++;
        }

        // The density increase: a second scatter and a second water band, laid out after everything above so the original
        // stones and their ids don't move.
        int extraBase = 0;
        for (int attempt = 0; attempt < 12000 && extraBase < ExtraBaseCount; attempt++)
        {
            float u = 0.04f + (float)random.NextDouble() * 0.92f, v = 0.04f + (float)random.NextDouble() * 0.92f;
            Vector3 p = origin + new Vector3(u * data.size.x, 0f, v * data.size.z);
            if (!Usable(terrain, ref p, 30f) || Crowded(placed, p, ExtraBaseSpacing))
                continue;
            placed.Add(p);
            CreateStone(root.transform, p, meshes, stone, random, ExtraBaseIdStart + ++extraBase, 0.26f, 0.42f);
        }
        var extraBank = new List<Vector3>();
        for (int attempt = 0; attempt < 8000 && extraBank.Count < ExtraBankCount && shore.Count > 0; attempt++)
        {
            Vector3 p = shore[random.Next(shore.Count)];
            float angle = (float)random.NextDouble() * Mathf.PI * 2f;
            p += new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * (0.5f + (float)random.NextDouble() * 3.5f);
            if (!Usable(terrain, ref p, 35f) || Crowded(placed, p, ExtraBankSpacing))
                continue;
            placed.Add(p);
            extraBank.Add(p);
            CreateStone(root.transform, p, meshes, stone, random, ExtraBankIdStart + extraBank.Count, 0.24f, 0.4f);
        }

        EditorSceneManager.MarkSceneDirty(root.scene);
        AssetDatabase.SaveAssets();
        Debug.Log($"[StonePlacer] {baseCount} scattered stones, {bank.Count} along the water, outcrop at {outcrop} with {foot} loose at its foot; " +
                  $"density increase added {extraBase} scattered and {extraBank.Count} along the water.");
    }

    // On land, not too steep, off the water; snaps p to the ground.
    static bool Usable(Terrain terrain, ref Vector3 p, float maxSlope)
    {
        TerrainData data = terrain.terrainData;
        Vector3 origin = terrain.transform.position;
        float u = (p.x - origin.x) / data.size.x, v = (p.z - origin.z) / data.size.z;
        if (u < 0.03f || u > 0.97f || v < 0.03f || v > 0.97f || data.GetSteepness(u, v) > maxSlope)
            return false;
        p.y = origin.y + data.GetInterpolatedHeight(u, v);
        if (Animal.IsOverWater(p, out _))
            return false;
        // Not inside anything (a campfire site, a building, a boulder).
        return !Physics.CheckSphere(p + Vector3.up * 0.6f, 0.4f, ~0, QueryTriggerInteraction.Ignore);
    }

    static bool Crowded(List<Vector3> placed, Vector3 p, float spacing)
    {
        foreach (Vector3 other in placed)
            if ((other - p).sqrMagnitude < spacing * spacing)
                return true;
        return false;
    }

    static Vector3 FindOutcropSite(Terrain terrain, System.Random random)
    {
        TerrainData data = terrain.terrainData;
        Vector3 origin = terrain.transform.position;
        Vector3 best = Vector3.zero;
        float bestScore = float.MinValue;
        for (int i = 0; i < 600; i++)
        {
            // The south face: from due east round through south to due west of the ridge top.
            float angle = Mathf.PI + (float)random.NextDouble() * Mathf.PI, distance = 15f + (float)random.NextDouble() * 30f;
            var flat = RidgeCenter + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
            if (Vector2.Distance(flat, CabinSite) < 28f || Vector2.Distance(flat, PlayerSpawn) < 40f)
                continue;
            var p = new Vector3(flat.x, 0f, flat.y);
            float u = (p.x - origin.x) / data.size.x, v = (p.z - origin.z) / data.size.z;
            p.y = origin.y + data.GetInterpolatedHeight(u, v);
            if (Animal.IsOverWater(p, out _) || TreeWithin(terrain, p, 5f))
                continue;
            float score = data.GetSteepness(u, v) - Vector2.Distance(flat, RidgeCenter) * 0.1f;
            if (score > bestScore)
            {
                bestScore = score;
                best = p;
            }
        }
        return best;
    }

    static bool TreeWithin(Terrain terrain, Vector3 p, float radius)
    {
        TerrainData data = terrain.terrainData;
        Vector3 origin = terrain.transform.position;
        foreach (TreeInstance tree in data.treeInstances)
        {
            Vector3 w = origin + Vector3.Scale(tree.position, data.size);
            if (new Vector2(w.x - p.x, w.z - p.z).sqrMagnitude < radius * radius)
                return true;
        }
        return false;
    }

    static Vector3 Downhill(Terrain terrain, Vector3 p)
    {
        TerrainData data = terrain.terrainData;
        Vector3 origin = terrain.transform.position;
        Vector3 normal = data.GetInterpolatedNormal((p.x - origin.x) / data.size.x, (p.z - origin.z) / data.size.z);
        Vector3 down = new Vector3(normal.x, 0f, normal.z);
        return down.sqrMagnitude > 0.0001f ? down.normalized : Vector3.forward;
    }

    static void CreateStone(Transform parent, Vector3 p, Mesh[] meshes, Material material, System.Random random, int id, float min, float max)
    {
        var go = new GameObject($"Stone {id}");
        go.transform.SetParent(parent);
        float size = Mathf.Lerp(min, max, (float)random.NextDouble());
        go.transform.position = p + Vector3.up * size * 0.25f;
        go.transform.rotation = Quaternion.Euler((float)random.NextDouble() * 360f, (float)random.NextDouble() * 360f, 0f);
        go.transform.localScale = new Vector3(size, size * 0.7f, size * (0.8f + (float)random.NextDouble() * 0.3f));
        Mesh mesh = meshes[random.Next(meshes.Length)];
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        go.AddComponent<MeshRenderer>().sharedMaterial = material;
        var collider = go.AddComponent<SphereCollider>();
        collider.radius = 0.6f; // a little generous, so a small stone is easy to look at
        go.AddComponent<LooseStone>().SetId(id);
    }

    // A lumpy low-poly rock: a subdivided octahedron pushed in and out by a seeded noise, flat-shaded.
    static Mesh RockMesh(string name, int seed)
    {
        string path = $"{ArtFolder}/{name}.asset";
        var random = new System.Random(Seed + seed);
        var verts = new List<Vector3> { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
        var tris = new List<int> { 0, 4, 3, 0, 3, 5, 0, 5, 2, 0, 2, 4, 1, 3, 4, 1, 5, 3, 1, 2, 5, 1, 4, 2 };
        for (int level = 0; level < 2; level++)
        {
            var next = new List<int>();
            var midpoints = new Dictionary<long, int>();
            int Mid(int a, int b)
            {
                long key = a < b ? ((long)a << 32) | (uint)b : ((long)b << 32) | (uint)a;
                if (midpoints.TryGetValue(key, out int m))
                    return m;
                verts.Add(((verts[a] + verts[b]) * 0.5f).normalized);
                midpoints[key] = verts.Count - 1;
                return verts.Count - 1;
            }
            for (int i = 0; i < tris.Count; i += 3)
            {
                int a = tris[i], b = tris[i + 1], c = tris[i + 2];
                int ab = Mid(a, b), bc = Mid(b, c), ca = Mid(c, a);
                next.AddRange(new[] { a, ab, ca, b, bc, ab, c, ca, bc, ab, bc, ca });
            }
            tris = next;
        }
        // Lumps, and a flatter bottom so it sits on the ground.
        for (int i = 0; i < verts.Count; i++)
        {
            Vector3 v = verts[i];
            float lump = 0.78f + (float)random.NextDouble() * 0.36f;
            v *= lump;
            if (v.y < -0.35f)
                v.y = -0.35f - (v.y + 0.35f) * 0.2f;
            verts[i] = v * 0.5f;
        }
        // Flat shading: every triangle its own vertices.
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

    static Material StoneMaterial()
    {
        string path = $"{ArtFolder}/Stone.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("HDRP/Lit"));
            AssetDatabase.CreateAsset(material, path);
        }
        material.SetColor("_BaseColor", new Color(0.46f, 0.45f, 0.42f));
        material.SetFloat("_Smoothness", 0.15f);
        material.enableInstancing = true;
        HDMaterial.ValidateMaterial(material);
        EditorUtility.SetDirty(material);
        return material;
    }
}
