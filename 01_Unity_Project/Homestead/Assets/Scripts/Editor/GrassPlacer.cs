using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

// Homestead > Place Tall Grass: clumps of tall meadow grass for Cordage (Trapping_System.md's sourcing) in the open
// ground — pasture and meadows, away from trees, off the water and steep slopes — each a TallGrass with a stable id.
// The clump is a generated mesh of tapered blades saved under Assets/Art/Grass. Re-running replaces the previous set
// with the same layout (fixed seed) and ids.
public static class GrassPlacer
{
    const string RootName = "Tall Grass";
    const string ArtFolder = "Assets/Art/Grass";
    const int Seed = 20260927;
    const int Count = 90;
    const float Spacing = 12f;
    const float TreeClearance = 7f;

    [MenuItem("Homestead/Place Tall Grass")]
    public static void Place()
    {
        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null)
        {
            Debug.LogError("[GrassPlacer] Needs the property terrain in the open scene.");
            return;
        }

        GameObject old = GameObject.Find(RootName);
        if (old != null)
            Undo.DestroyObjectImmediate(old);
        if (!AssetDatabase.IsValidFolder(ArtFolder))
            AssetDatabase.CreateFolder("Assets/Art", "Grass");
        Mesh mesh = ClumpMesh();
        Material material = GrassMaterial();

        var root = new GameObject(RootName);
        Undo.RegisterCreatedObjectUndo(root, "Place Tall Grass");
        TerrainData data = terrain.terrainData;
        Vector3 origin = terrain.transform.position;
        var random = new System.Random(Seed);

        // Tree positions bucketed, for a quick "open ground" test.
        var trees = new Dictionary<Vector2Int, List<Vector2>>();
        foreach (TreeInstance tree in data.treeInstances)
        {
            Vector3 w = origin + Vector3.Scale(tree.position, data.size);
            var cell = new Vector2Int(Mathf.FloorToInt(w.x / 10f), Mathf.FloorToInt(w.z / 10f));
            if (!trees.TryGetValue(cell, out List<Vector2> list))
                trees[cell] = list = new List<Vector2>();
            list.Add(new Vector2(w.x, w.z));
        }

        var placed = new List<Vector3>();
        for (int attempt = 0; attempt < 8000 && placed.Count < Count; attempt++)
        {
            float u = 0.05f + (float)random.NextDouble() * 0.9f, v = 0.05f + (float)random.NextDouble() * 0.9f;
            if (data.GetSteepness(u, v) > 20f)
                continue;
            var p = origin + new Vector3(u * data.size.x, data.GetInterpolatedHeight(u, v), v * data.size.z);
            if (Animal.IsOverWater(p, out _) || NearTree(trees, p) || Physics.CheckSphere(p + Vector3.up * 0.8f, 0.5f, ~0, QueryTriggerInteraction.Ignore))
                continue;
            bool crowded = false;
            foreach (Vector3 other in placed)
                if ((other - p).sqrMagnitude < Spacing * Spacing)
                {
                    crowded = true;
                    break;
                }
            if (crowded)
                continue;

            placed.Add(p);
            var go = new GameObject($"Tall Grass {placed.Count}");
            go.transform.SetParent(root.transform);
            go.transform.position = p;
            go.transform.rotation = Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f);
            float scale = 0.85f + (float)random.NextDouble() * 0.35f;
            go.transform.localScale = new Vector3(scale, scale * (0.9f + (float)random.NextDouble() * 0.25f), scale);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = material;
            var collider = go.AddComponent<CapsuleCollider>();
            collider.radius = 0.4f;
            collider.height = 1.1f;
            collider.center = new Vector3(0f, 0.55f, 0f);
            collider.isTrigger = false;
            go.AddComponent<TallGrass>().SetId(placed.Count);
        }

        EditorSceneManager.MarkSceneDirty(root.scene);
        AssetDatabase.SaveAssets();
        Debug.Log($"[GrassPlacer] Placed {placed.Count} tall grass clumps.");
    }

    static bool NearTree(Dictionary<Vector2Int, List<Vector2>> trees, Vector3 p)
    {
        var centre = new Vector2Int(Mathf.FloorToInt(p.x / 10f), Mathf.FloorToInt(p.z / 10f));
        for (int dz = -1; dz <= 1; dz++)
        for (int dx = -1; dx <= 1; dx++)
            if (trees.TryGetValue(new Vector2Int(centre.x + dx, centre.y + dz), out List<Vector2> list))
                foreach (Vector2 t in list)
                    if ((t - new Vector2(p.x, p.z)).sqrMagnitude < TreeClearance * TreeClearance)
                        return true;
        return false;
    }

    // Tapered blades leaning out from the centre: each a thin four-sided spike, faced both ways so it reads from any
    // side without a double-sided material.
    static Mesh ClumpMesh()
    {
        string path = $"{ArtFolder}/Tall Grass Clump.asset";
        var random = new System.Random(Seed);
        var verts = new List<Vector3>();
        var tris = new List<int>();
        for (int b = 0; b < 22; b++)
        {
            float angle = (float)random.NextDouble() * Mathf.PI * 2f;
            float r = (float)random.NextDouble() * 0.25f;
            var baseCentre = new Vector3(Mathf.Cos(angle) * r, 0f, Mathf.Sin(angle) * r);
            float height = 0.8f + (float)random.NextDouble() * 0.5f;
            Vector3 lean = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * (0.15f + (float)random.NextDouble() * 0.3f);
            Vector3 tip = baseCentre + lean + Vector3.up * height;
            const float w = 0.025f;
            int start = verts.Count;
            verts.Add(baseCentre + new Vector3(-w, 0f, 0f));
            verts.Add(baseCentre + new Vector3(0f, 0f, w));
            verts.Add(baseCentre + new Vector3(w, 0f, 0f));
            verts.Add(baseCentre + new Vector3(0f, 0f, -w));
            verts.Add(tip);
            // Both windings, so every face shows whichever side it's seen from.
            for (int i = 0; i < 4; i++)
            {
                tris.AddRange(new[] { start + i, start + 4, start + (i + 1) % 4 });
                tris.AddRange(new[] { start + (i + 1) % 4, start + 4, start + i });
            }
        }
        // Flat shading.
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
        mesh.name = "Tall Grass Clump";
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

    static Material GrassMaterial()
    {
        string path = $"{ArtFolder}/Tall Grass.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("HDRP/Lit"));
            AssetDatabase.CreateAsset(material, path);
        }
        material.SetColor("_BaseColor", new Color(0.52f, 0.52f, 0.28f)); // a dry, straw-green meadow grass
        material.SetFloat("_Smoothness", 0.1f);
        material.enableInstancing = true;
        HDMaterial.ValidateMaterial(material);
        EditorUtility.SetDirty(material);
        return material;
    }
}
