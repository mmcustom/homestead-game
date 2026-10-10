using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

// Homestead > Place Sticks and Branches: Wood_Gathering_System.md's Ground Sticks and Fallen Branches, in the open World
// scene. Hand-pickup wood so a Pioneer with no Axe can still get Sticks (campfire, Primitive Axe, Bow Drill) without
// felling a tree. Tree-driven (Mike's 2026-10-07 playtest: too few under the trees): every hardwood gets 1-3 stick
// bundles (3 Sticks each) and a 40% chance of one Fallen Branch (1 Branch), inside its drip line, 1.5-7 m from the
// trunk. A light scatter of bundles also lies in the open meadow. Nothing is placed on water, on slopes over 25°, or
// inside anything solid. Each piece is a GroundWood with a stable id (sticks from 1, branches from 10001) so
// GroundWoodManager can remember what's taken and bring it back. The shapes are generated low-poly meshes saved under
// Assets/Art/Wood, in a lighter weathered-wood material than the bark so they read against leaf litter and grass.
// Re-running replaces the previous set, with the same layout (fixed seed) and so the same ids — but this layout differs
// from the first, smaller one, so ids saved against that one point at different pieces.
// Counts and distances are first-pass numbers for Mike to tune by feel.
public static class GroundWoodPlacer
{
    const string RootName = "Ground Wood";
    const string ArtFolder = "Assets/Art/Wood";
    const int Seed = 20261008;

    const int MinBundlesPerTree = 1, MaxBundlesPerTree = 3;
    const float BranchChancePerTree = 0.4f;
    const float MinDistance = 1.5f, MaxDistance = 7f;
    const float BundleSpacing = 0.9f;   // between bundles, anywhere
    const float BranchSpacing = 1.8f;   // between branches
    const int MeadowBundles = 60;       // lighter in the open
    const float MaxSlope = 25f;
    const int BranchIdStart = 10000;

    // Where a new game starts (StonePlacer's spawn point), for the density report.
    static readonly Vector2 PlayerSpawn = new Vector2(20f, -40f);

    [MenuItem("Homestead/Place Sticks and Branches")]
    public static void Place()
    {
        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null)
        {
            Debug.LogError("[GroundWoodPlacer] Needs the property terrain in the open scene.");
            return;
        }

        GameObject old = GameObject.Find(RootName);
        if (old != null)
            Undo.DestroyObjectImmediate(old);
        if (!AssetDatabase.IsValidFolder(ArtFolder))
            AssetDatabase.CreateFolder("Assets/Art", "Wood");
        Mesh[] bundles = { BundleMesh("Stick Bundle A", 1), BundleMesh("Stick Bundle B", 2), BundleMesh("Stick Bundle C", 3) };
        Mesh[] branches = { BranchMesh("Fallen Branch A", 1), BranchMesh("Fallen Branch B", 2), BranchMesh("Fallen Branch C", 3) };
        Material wood = WoodMaterial();

        var root = new GameObject(RootName);
        Undo.RegisterCreatedObjectUndo(root, "Place Sticks and Branches");
        TerrainData data = terrain.terrainData;
        Vector3 origin = terrain.transform.position;
        var random = new System.Random(Seed);

        // Hardwoods (broad and tall), as ground positions; the understory shrubs don't shed enough to count.
        var hardwoods = new List<Vector3>();
        foreach (TreeInstance tree in data.treeInstances)
            if (tree.prototypeIndex != WoodManager.Shrub)
                hardwoods.Add(origin + Vector3.Scale(tree.position, data.size));

        var bundleGrid = new SpatialGrid(BundleSpacing);
        var branchGrid = new SpatialGrid(BranchSpacing);
        var bundlePositions = new List<Vector3>();
        var branchPositions = new List<Vector3>();
        int bundleId = 1, branchId = BranchIdStart + 1;

        foreach (Vector3 tree in hardwoods)
        {
            int wantBundles = random.Next(MinBundlesPerTree, MaxBundlesPerTree + 1);
            for (int i = 0; i < wantBundles; i++)
                if (TryPlace(terrain, root.transform, random, tree, bundleGrid, bundles, wood, GroundWood.Kind.Sticks, bundleId, bundlePositions))
                    bundleId++;
            if (random.NextDouble() < BranchChancePerTree)
                if (TryPlace(terrain, root.transform, random, tree, branchGrid, branches, wood, GroundWood.Kind.Branch, branchId, branchPositions))
                    branchId++;
        }

        // A light scatter in the open meadow and pasture.
        int meadow = 0;
        for (int attempt = 0; attempt < 8000 && meadow < MeadowBundles; attempt++)
        {
            Vector3 p = origin + new Vector3((0.04f + (float)random.NextDouble() * 0.92f) * data.size.x, 0f,
                                             (0.04f + (float)random.NextDouble() * 0.92f) * data.size.z);
            if (!Usable(terrain, ref p) || bundleGrid.Crowded(p))
                continue;
            if (Create(root.transform, random, p, bundles, wood, GroundWood.Kind.Sticks, bundleId))
            {
                bundleGrid.Add(p);
                bundlePositions.Add(p);
                bundleId++;
                meadow++;
            }
        }

        EditorSceneManager.MarkSceneDirty(root.scene);
        AssetDatabase.SaveAssets();
        Report(hardwoods, bundlePositions, branchPositions, origin);
    }

    // Places one piece within the tree's drip line; false if every try landed somewhere unusable.
    static bool TryPlace(Terrain terrain, Transform root, System.Random random, Vector3 tree, SpatialGrid grid, Mesh[] meshes, Material material,
                         GroundWood.Kind kind, int id, List<Vector3> positions)
    {
        for (int attempt = 0; attempt < 14; attempt++)
        {
            float angle = (float)random.NextDouble() * Mathf.PI * 2f;
            float distance = MinDistance + (float)random.NextDouble() * (MaxDistance - MinDistance);
            Vector3 p = tree + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * distance;
            if (!Usable(terrain, ref p) || grid.Crowded(p))
                continue;
            if (!Create(root, random, p, meshes, material, kind, id))
                continue;
            grid.Add(p);
            positions.Add(p);
            return true;
        }
        return false;
    }

    // On land, gentle ground, off the water, and not inside anything solid; snaps p to the ground.
    static bool Usable(Terrain terrain, ref Vector3 p)
    {
        TerrainData data = terrain.terrainData;
        Vector3 origin = terrain.transform.position;
        float u = (p.x - origin.x) / data.size.x, v = (p.z - origin.z) / data.size.z;
        if (u < 0.03f || u > 0.97f || v < 0.03f || v > 0.97f || data.GetSteepness(u, v) > MaxSlope)
            return false;
        p.y = origin.y + data.GetInterpolatedHeight(u, v);
        return !Animal.IsOverWater(p, out _) && !Physics.CheckSphere(p + Vector3.up * 0.5f, 0.3f, ~0, QueryTriggerInteraction.Ignore);
    }

    static bool Create(Transform root, System.Random random, Vector3 p, Mesh[] meshes, Material material, GroundWood.Kind kind, int id)
    {
        bool sticks = kind == GroundWood.Kind.Sticks;
        var go = new GameObject(sticks ? $"Sticks {id}" : $"Fallen Branch {id - BranchIdStart}");
        go.transform.SetParent(root);
        go.transform.position = p + Vector3.up * 0.03f;
        go.transform.rotation = Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f);
        go.AddComponent<MeshFilter>().sharedMesh = meshes[random.Next(meshes.Length)];
        go.AddComponent<MeshRenderer>().sharedMaterial = material;
        // A generous box, so a thin stick is easy to look at; branches lie along local X.
        var box = go.AddComponent<BoxCollider>();
        box.size = sticks ? new Vector3(1.2f, 0.35f, 1.2f) : new Vector3(2.2f, 0.35f, 0.9f);
        box.center = new Vector3(0f, 0.12f, 0f);
        go.AddComponent<GroundWood>().Setup(id, kind);
        return true;
    }

    // What the run found, for tuning: totals, and how much lies within 50 m of the new-game spawn and under the ten
    // hardwoods nearest it.
    static void Report(List<Vector3> hardwoods, List<Vector3> bundles, List<Vector3> branches, Vector3 origin)
    {
        var spawn = new Vector3(PlayerSpawn.x, 0f, PlayerSpawn.y);
        int nearBundles = CountWithin(bundles, spawn, 50f), nearBranches = CountWithin(branches, spawn, 50f);

        var nearest = new List<Vector3>(hardwoods);
        nearest.Sort((a, b) => Flat(a, spawn).CompareTo(Flat(b, spawn)));
        int tenth = Mathf.Min(10, nearest.Count);
        float bundleSum = 0f, branchSum = 0f;
        for (int i = 0; i < tenth; i++)
        {
            bundleSum += CountWithin(bundles, nearest[i], MaxDistance);
            branchSum += CountWithin(branches, nearest[i], MaxDistance);
        }

        Debug.Log($"[GroundWoodPlacer] {hardwoods.Count} hardwoods; placed {bundles.Count} stick bundles and {branches.Count} fallen branches. " +
                  $"Within 50 m of the spawn: {nearBundles} bundles, {nearBranches} branches. " +
                  $"Within 7 m of each of the {tenth} nearest hardwoods, on average: {bundleSum / Mathf.Max(1, tenth):0.0} bundles, {branchSum / Mathf.Max(1, tenth):0.0} branches.");
    }

    static float Flat(Vector3 a, Vector3 b) => new Vector2(a.x - b.x, a.z - b.z).sqrMagnitude;

    static int CountWithin(List<Vector3> points, Vector3 centre, float radius)
    {
        int n = 0;
        foreach (Vector3 p in points)
            if (Flat(p, centre) <= radius * radius)
                n++;
        return n;
    }

    // Pieces bucketed on a grid, so "too close to another" is quick with a few thousand of them.
    class SpatialGrid
    {
        readonly float spacing;
        readonly Dictionary<Vector2Int, List<Vector3>> cells = new Dictionary<Vector2Int, List<Vector3>>();

        public SpatialGrid(float spacing) => this.spacing = spacing;

        Vector2Int Cell(Vector3 p) => new Vector2Int(Mathf.FloorToInt(p.x / spacing), Mathf.FloorToInt(p.z / spacing));

        public void Add(Vector3 p)
        {
            Vector2Int c = Cell(p);
            if (!cells.TryGetValue(c, out List<Vector3> list))
                cells[c] = list = new List<Vector3>();
            list.Add(p);
        }

        public bool Crowded(Vector3 p)
        {
            Vector2Int c = Cell(p);
            for (int dz = -1; dz <= 1; dz++)
            for (int dx = -1; dx <= 1; dx++)
                if (cells.TryGetValue(new Vector2Int(c.x + dx, c.y + dz), out List<Vector3> list))
                    foreach (Vector3 other in list)
                        if (Flat(other, p) < spacing * spacing)
                            return true;
            return false;
        }
    }

    // Weathered, lighter wood, so a stick on dark leaf litter or green grass is easy to pick out.
    static Material WoodMaterial()
    {
        string path = $"{ArtFolder}/Ground Wood.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("HDRP/Lit"));
            AssetDatabase.CreateAsset(material, path);
        }
        material.SetColor("_BaseColor", new Color(0.58f, 0.45f, 0.3f));
        material.SetFloat("_Smoothness", 0.12f);
        material.enableInstancing = true;
        HDMaterial.ValidateMaterial(material);
        EditorUtility.SetDirty(material);
        return material;
    }

    // Six sticks laid across each other on the ground, a little stacked — thicker and longer than a real twig bundle
    // so they show from a few metres away.
    static Mesh BundleMesh(string name, int seed)
    {
        var random = new System.Random(Seed + seed);
        var verts = new List<Vector3>();
        var tris = new List<int>();
        for (int i = 0; i < 6; i++)
        {
            float yaw = (float)random.NextDouble() * Mathf.PI;
            float length = 0.75f + (float)random.NextDouble() * 0.35f;
            var centre = new Vector3(((float)random.NextDouble() - 0.5f) * 0.3f, 0.03f + i * 0.035f, ((float)random.NextDouble() - 0.5f) * 0.3f);
            var along = new Vector3(Mathf.Cos(yaw), 0.04f * ((float)random.NextDouble() - 0.5f), Mathf.Sin(yaw)) * (length * 0.5f);
            float radius = 0.028f + (float)random.NextDouble() * 0.012f;
            AddStick(verts, tris, centre - along, centre + along, radius, radius * 0.6f);
        }
        return SaveMesh(name, verts, tris);
    }

    // One long branch along X, tapering, with a couple of side twigs.
    static Mesh BranchMesh(string name, int seed)
    {
        var random = new System.Random(Seed + 100 + seed);
        var verts = new List<Vector3>();
        var tris = new List<int>();
        float length = 1.6f + (float)random.NextDouble() * 0.6f;
        float thick = 0.065f + (float)random.NextDouble() * 0.02f;
        var start = new Vector3(-length * 0.5f, 0.07f, 0f);
        var end = new Vector3(length * 0.5f, 0.05f, ((float)random.NextDouble() - 0.5f) * 0.2f);
        AddStick(verts, tris, start, end, thick, thick * 0.4f);
        int twigs = 2 + random.Next(2);
        for (int i = 0; i < twigs; i++)
        {
            float t = 0.2f + (float)random.NextDouble() * 0.6f;
            Vector3 from = Vector3.Lerp(start, end, t);
            float side = random.NextDouble() < 0.5 ? -1f : 1f;
            Vector3 to = from + new Vector3(0.2f + (float)random.NextDouble() * 0.2f, 0.03f, side * (0.22f + (float)random.NextDouble() * 0.2f));
            AddStick(verts, tris, from, to, thick * 0.45f, thick * 0.2f);
        }
        return SaveMesh(name, verts, tris);
    }

    // A tapered five-sided stick from a to b, with a cap at each end.
    static void AddStick(List<Vector3> verts, List<int> tris, Vector3 a, Vector3 b, float radiusA, float radiusB)
    {
        const int Sides = 5;
        Vector3 axis = (b - a).normalized;
        Vector3 side = Vector3.Cross(axis, Vector3.up);
        if (side.sqrMagnitude < 0.0001f)
            side = Vector3.right;
        side.Normalize();
        Vector3 up = Vector3.Cross(side, axis).normalized;
        int start = verts.Count;
        for (int ring = 0; ring < 2; ring++)
            for (int i = 0; i < Sides; i++)
            {
                float angle = i / (float)Sides * Mathf.PI * 2f;
                verts.Add((ring == 0 ? a : b) + (side * Mathf.Cos(angle) + up * Mathf.Sin(angle)) * (ring == 0 ? radiusA : radiusB));
            }
        for (int i = 0; i < Sides; i++)
        {
            int n = (i + 1) % Sides;
            int a0 = start + i, a1 = start + n, b0 = start + Sides + i, b1 = start + Sides + n;
            tris.AddRange(new[] { a0, b0, a1, a1, b0, b1 });
        }
        int capA = verts.Count;
        verts.Add(a);
        int capB = verts.Count;
        verts.Add(b);
        for (int i = 0; i < Sides; i++)
        {
            int n = (i + 1) % Sides;
            tris.AddRange(new[] { capA, start + i, start + n });
            tris.AddRange(new[] { capB, start + Sides + n, start + Sides + i });
        }
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
