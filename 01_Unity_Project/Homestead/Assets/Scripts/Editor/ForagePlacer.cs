using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Homestead > Place Forage Patches: Foraging_System.md's forage patches, one per species from 05_Documentation/Plants,
// each a DiscoverySite found on first harvest. Writes the species data (Assets/Resources/Forage) from the Plants
// sheets, builds each patch from the environment prefabs plus simple shapes for the ripe crop, and places it by
// habitat near a landmark: berries and fruit on forest edges and the old field, nuts and mushrooms in the woods,
// greens in open pasture, cattails on the pond shore. The Briar Patch discovery site becomes the Blackberry patch.
// Re-running rebuilds everything; picked state lives in the save by site id, which stays the same.
public static class ForagePlacer
{
    const string RootName = "Forage";
    const string SpeciesFolder = "Assets/Resources/Forage";
    const string MaterialFolder = "Assets/Art/Forage";
    const int Seed = 424242;

    enum Habitat { Forest, Edge, Open, Shore }

    struct Spec
    {
        public string item, patch, icon;
        public ForageLook look;
        public Season season;
        public float windowStart, windowEnd;
        public int yield;
        public ForageRegrowth regrowth;
        public int regrowDays;
        public Habitat habitat;
        public Vector2 anchor;
    }

    // Numbers from the Plants sheets (yields and regrowth confirmed 2026-09-22); anchors are rough landmarks from
    // PropertyTerrainBuilder's layout. Blackberries use the existing Briar Patch site instead of an anchor.
    static readonly Spec[] Specs =
    {
        S("blackberries", "Blackberry Patch", "berry", ForageLook.BerryBush, Season.Summer, 3, ForageRegrowth.Annual, 0, Habitat.Edge, Vector2.zero),
        S("raspberries", "Raspberry Canes", "berry", ForageLook.BerryBush, Season.Summer, 3, ForageRegrowth.Annual, 0, Habitat.Edge, new Vector2(95f, 70f)),
        S("wild_apples", "Old Apple Tree", "fruit", ForageLook.FruitTree, Season.Fall, 4, ForageRegrowth.Annual, 0, Habitat.Edge, new Vector2(135f, -125f)),
        S("pawpaw", "Pawpaw Grove", "fruit", ForageLook.FruitTree, Season.Fall, 3, ForageRegrowth.Annual, 0, Habitat.Edge, new Vector2(-80f, 30f)),
        S("hickory_nuts", "Hickory Stand", "nut", ForageLook.NutTree, Season.Fall, 5, ForageRegrowth.Annual, 0, Habitat.Forest, new Vector2(-120f, 120f)),
        S("walnuts", "Black Walnut Trees", "nut", ForageLook.NutTree, Season.Fall, 5, ForageRegrowth.Annual, 0, Habitat.Forest, new Vector2(-70f, -70f)),
        S("chestnuts", "Chestnut Tree", "nut", ForageLook.NutTree, Season.Fall, 4, ForageRegrowth.Annual, 0, Habitat.Forest, new Vector2(70f, -160f)),
        S("dandelion", "Dandelion Meadow", "greens", ForageLook.GroundGreens, Season.Spring, 2, ForageRegrowth.Days, 4, Habitat.Open, new Vector2(45f, -35f)),
        S("wild_onion", "Wild Onion Patch", "greens", ForageLook.GroundGreens, Season.Spring, 2, ForageRegrowth.Days, 4, Habitat.Open, new Vector2(140f, 140f)),
        S("cattail", "Cattail Marsh", "greens", ForageLook.Cattail, Season.Spring, 2, ForageRegrowth.Days, 4, Habitat.Shore, new Vector2(-150f, -125f)),
        S("morel", "Morel Ground", "mushroom", ForageLook.GroundMushroom, Season.Spring, 2, ForageRegrowth.Annual, 0, Habitat.Forest, new Vector2(-40f, 160f), 0.1f, 0.6f),
        S("dryads_saddle", "Dryad's Saddle Log", "mushroom", ForageLook.LogMushroom, Season.Spring, 2, ForageRegrowth.Days, 3, Habitat.Forest, new Vector2(175f, 55f)),
        S("oyster_mushroom", "Oyster Mushroom Log", "mushroom", ForageLook.LogMushroom, Season.Spring, 2, ForageRegrowth.Days, 3, Habitat.Forest, new Vector2(-160f, 10f)),
    };

    static Spec S(string item, string patch, string icon, ForageLook look, Season season, int yield, ForageRegrowth regrowth,
                  int regrowDays, Habitat habitat, Vector2 anchor, float windowStart = 0f, float windowEnd = 1f) => new Spec
    {
        item = item, patch = patch, icon = icon, look = look, season = season, yield = yield, regrowth = regrowth,
        regrowDays = regrowDays, habitat = habitat, anchor = anchor, windowStart = windowStart, windowEnd = windowEnd,
    };

    static System.Random random;
    static readonly Dictionary<string, Material> materials = new Dictionary<string, Material>();

    [MenuItem("Homestead/Place Forage Patches")]
    public static void Place()
    {
        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null)
        {
            Debug.LogError("[ForagePlacer] Open the World scene with the property terrain first.");
            return;
        }

        random = new System.Random(Seed);
        materials.Clear();

        GameObject old = GameObject.Find(RootName);
        if (old != null)
            Undo.DestroyObjectImmediate(old);
        var root = new GameObject(RootName);
        Undo.RegisterCreatedObjectUndo(root, "Place Forage Patches");

        var taken = new List<Vector3>();
        foreach (DiscoverySite existing in Object.FindObjectsByType<DiscoverySite>(FindObjectsSortMode.None))
            taken.Add(existing.transform.position);

        int placed = 0;
        foreach (Spec spec in Specs)
        {
            ForageSpecies species = WriteSpecies(spec);
            GameObject node;
            if (spec.item == "blackberries")
            {
                node = FindBriarPatch();
                if (node == null)
                {
                    Debug.LogWarning("[ForagePlacer] No Briar Patch site found for the blackberries.");
                    continue;
                }
                ClearChildren(node.transform);
            }
            else
            {
                if (!FindSpot(terrain, spec, taken, out Vector3 position))
                {
                    Debug.LogWarning($"[ForagePlacer] No spot found for {spec.patch}.");
                    continue;
                }
                taken.Add(position);
                node = new GameObject(spec.patch);
                node.transform.SetParent(root.transform);
                node.transform.position = position;
                node.transform.rotation = Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f);

                var site = node.AddComponent<DiscoverySite>();
                site.Configure("forage_" + spec.item, spec.patch, DiscoveryCategory.Plant, Facts(spec, species),
                               spec.icon, onApproach: false, bySight: false);
            }

            BuildPlant(node.transform, spec, terrain);
            GameObject crop = BuildCrop(node.transform, spec, terrain);
            ForageNode forage = node.GetComponent<ForageNode>();
            if (forage == null)
                forage = node.AddComponent<ForageNode>();
            forage.Configure(species, crop);
            EditorUtility.SetDirty(forage);
            placed++;
        }

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(root.scene);
        Debug.Log($"[ForagePlacer] Placed {placed} forage patches.");
    }

    static IEnumerable<DiscoveryFact> Facts(Spec spec, ForageSpecies species)
    {
        yield return new DiscoveryFact { label = "Harvest Season", value = species.SeasonLabel };
        yield return new DiscoveryFact { label = "Yield", value = $"{spec.yield} per harvest" };
        yield return new DiscoveryFact
        {
            label = "Regrowth",
            value = spec.regrowth == ForageRegrowth.Annual ? $"Next {spec.season}" : $"Every {spec.regrowDays} days or so in season",
        };
    }

    static GameObject FindBriarPatch()
    {
        foreach (DiscoverySite site in Object.FindObjectsByType<DiscoverySite>(FindObjectsSortMode.None))
        {
            if (site.DisplayName == "Briar Patch")
                return site.gameObject;
        }
        return null;
    }

    static void ClearChildren(Transform node)
    {
        for (int i = node.childCount - 1; i >= 0; i--)
        {
            string childName = node.GetChild(i).name;
            if (childName == "Plant" || childName == "Crop")
                Undo.DestroyObjectImmediate(node.GetChild(i).gameObject);
        }
    }

    static ForageSpecies WriteSpecies(Spec spec)
    {
        if (!AssetDatabase.IsValidFolder(SpeciesFolder))
            AssetDatabase.CreateFolder("Assets/Resources", "Forage");

        string path = $"{SpeciesFolder}/{spec.item}.asset";
        var species = AssetDatabase.LoadAssetAtPath<ForageSpecies>(path);
        if (species == null)
        {
            species = ScriptableObject.CreateInstance<ForageSpecies>();
            AssetDatabase.CreateAsset(species, path);
        }

        species.itemId = spec.item;
        species.patchName = spec.patch;
        species.markerIcon = spec.icon;
        species.look = spec.look;
        species.season = spec.season;
        species.windowStart = spec.windowStart;
        species.windowEnd = spec.windowEnd;
        species.yield = spec.yield;
        species.regrowth = spec.regrowth;
        species.regrowDays = Mathf.Max(1, spec.regrowDays);
        EditorUtility.SetDirty(species);
        return species;
    }

    // --- Placement ---

    static bool FindSpot(Terrain terrain, Spec spec, List<Vector3> taken, out Vector3 position)
    {
        TerrainData data = terrain.terrainData;
        Vector3 origin = terrain.transform.position;
        TreeInstance[] trees = data.treeInstances;
        var treePositions = new List<Vector2>(trees.Length);
        foreach (TreeInstance tree in trees)
            treePositions.Add(new Vector2(origin.x + tree.position.x * data.size.x, origin.z + tree.position.z * data.size.z));

        // Search outward from the anchor for the first spot that fits the habitat.
        for (float radius = 0f; radius <= 120f; radius += 4f)
        {
            int steps = Mathf.Max(1, Mathf.RoundToInt(radius * 0.5f));
            int start = random.Next(steps);
            for (int s = 0; s < steps; s++)
            {
                float angle = (s + start) / (float)steps * Mathf.PI * 2f;
                var p2 = spec.anchor + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                if (!Fits(terrain, spec, p2, treePositions, taken))
                    continue;

                position = new Vector3(p2.x, Height(terrain, p2), p2.y);
                return true;
            }
        }

        position = Vector3.zero;
        return false;
    }

    static bool Fits(Terrain terrain, Spec spec, Vector2 p, List<Vector2> trees, List<Vector3> taken)
    {
        TerrainData data = terrain.terrainData;
        Vector3 origin = terrain.transform.position;
        float u = (p.x - origin.x) / data.size.x, v = (p.y - origin.z) / data.size.z;
        if (u < 0.06f || u > 0.94f || v < 0.06f || v > 0.94f || data.GetSteepness(u, v) > 16f)
            return false;

        foreach (Vector3 other in taken)
        {
            if (Vector2.Distance(new Vector2(other.x, other.z), p) < 25f)
                return false;
        }

        float nearest = float.MaxValue;
        int within8 = 0, within14 = 0;
        foreach (Vector2 t in trees)
        {
            float d = Vector2.Distance(t, p);
            nearest = Mathf.Min(nearest, d);
            if (d < 8f) within8++;
            if (d < 14f) within14++;
        }

        // Keep the patch's own footprint clear of trunks (trees and bushes need more room than ground patches).
        bool bigPlant = spec.look == ForageLook.FruitTree || spec.look == ForageLook.NutTree;
        if (nearest < (bigPlant ? 5f : 3f))
            return false;

        // Water is judged from above (surface over ground), since a sphere at the lakebed can miss deep water.
        bool wet = Animal.IsOverWater(new Vector3(p.x, Height(terrain, p), p.y), out _);
        bool nearWater = false;
        for (int i = 0; i < 12 && !nearWater; i++)
        {
            Vector2 around = p + new Vector2(Mathf.Cos(i * 0.5236f), Mathf.Sin(i * 0.5236f)) * 5f;
            nearWater = Animal.IsOverWater(new Vector3(around.x, Height(terrain, around), around.y), out _);
        }
        if (!nearWater && spec.habitat != Habitat.Shore)
        {
            for (int i = 0; i < 12 && !nearWater; i++)
            {
                Vector2 around = p + new Vector2(Mathf.Cos(i * 0.5236f), Mathf.Sin(i * 0.5236f)) * 9f;
                nearWater = Animal.IsOverWater(new Vector3(around.x, Height(terrain, around), around.y), out _);
            }
        }

        switch (spec.habitat)
        {
            case Habitat.Forest: return !nearWater && within8 >= 3;
            case Habitat.Edge: return !nearWater && within14 >= 2 && within8 <= 3;
            case Habitat.Open: return !nearWater && within14 == 0;
            case Habitat.Shore: return nearWater && !wet;
            default: return false;
        }
    }

    static float Height(Terrain terrain, Vector2 p) => terrain.SampleHeight(new Vector3(p.x, 0f, p.y)) + terrain.transform.position.y;

    // --- Visuals ---

    static void BuildPlant(Transform node, Spec spec, Terrain terrain)
    {
        var plant = new GameObject("Plant");
        plant.transform.SetParent(node, false);

        switch (spec.look)
        {
            case ForageLook.BerryBush:
                Prefab("Understory Shrub", plant.transform, 1.25f);
                AddBox(plant, new Vector3(0f, 0.7f, 0f), new Vector3(1.8f, 1.4f, 1.8f));
                break;
            case ForageLook.FruitTree:
                Prefab("Hardwood Broad", plant.transform, spec.item == "pawpaw" ? 0.45f : 0.55f);
                break;
            case ForageLook.NutTree:
                Prefab("Hardwood Tall", plant.transform, 0.9f);
                AddBox(plant, new Vector3(0f, 0.1f, 0f), new Vector3(6f, 0.2f, 6f)); // the nut drop around the trunk
                break;
            case ForageLook.GroundGreens:
                for (int i = 0; i < 9; i++)
                {
                    Vector3 at = Scatter(1.1f);
                    if (spec.item == "wild_onion")
                    {
                        for (int k = 0; k < 4; k++)
                            Shape(PrimitiveType.Cylinder, plant.transform, at + Scatter(0.08f) + Vector3.up * 0.18f,
                                  new Vector3(0.015f, 0.18f, 0.015f), "Onion Leaf", new Color(0.3f, 0.5f, 0.2f),
                                  Quaternion.Euler(Rand(-10f, 10f), 0f, Rand(-10f, 10f)));
                    }
                    else
                    {
                        Shape(PrimitiveType.Sphere, plant.transform, at + Vector3.up * 0.02f, new Vector3(0.28f, 0.04f, 0.28f),
                              "Dandelion Leaves", new Color(0.28f, 0.45f, 0.18f));
                    }
                }
                AddBox(plant, new Vector3(0f, 0.15f, 0f), new Vector3(2.6f, 0.3f, 2.6f));
                break;
            case ForageLook.Cattail:
                for (int i = 0; i < 16; i++)
                {
                    float height = Rand(0.7f, 1f);
                    Shape(PrimitiveType.Cylinder, plant.transform, Scatter(1.3f) + Vector3.up * height, new Vector3(0.025f, height, 0.025f),
                          "Cattail Stem", new Color(0.35f, 0.45f, 0.2f), Quaternion.Euler(Rand(-6f, 6f), 0f, Rand(-6f, 6f)));
                }
                AddBox(plant, new Vector3(0f, 0.9f, 0f), new Vector3(2.6f, 1.8f, 2.6f));
                break;
            case ForageLook.GroundMushroom:
                Shape(PrimitiveType.Cylinder, plant.transform, Vector3.up * 0.01f, new Vector3(1.6f, 0.01f, 1.6f), "Leaf Litter",
                      new Color(0.3f, 0.22f, 0.14f));
                AddBox(plant, new Vector3(0f, 0.1f, 0f), new Vector3(1.8f, 0.2f, 1.8f));
                break;
            case ForageLook.LogMushroom:
                Shape(PrimitiveType.Cylinder, plant.transform, Vector3.up * 0.28f, new Vector3(0.55f, 1.6f, 0.55f), "Fallen Log",
                      new Color(0.27f, 0.2f, 0.14f), Quaternion.Euler(0f, 0f, 90f));
                AddBox(plant, new Vector3(0f, 0.28f, 0f), new Vector3(3.2f, 0.56f, 0.6f));
                break;
        }

        SnapChildrenToGround(plant.transform, terrain);
    }

    static GameObject BuildCrop(Transform node, Spec spec, Terrain terrain)
    {
        var crop = new GameObject("Crop");
        crop.transform.SetParent(node, false);
        Bounds canopy = Canopy(node);

        switch (spec.look)
        {
            case ForageLook.BerryBush:
            {
                Color berry = spec.item == "raspberries" ? new Color(0.7f, 0.08f, 0.12f) : new Color(0.12f, 0.04f, 0.1f);
                for (int i = 0; i < 26; i++)
                    Shape(PrimitiveType.Sphere, crop.transform, OnEllipsoid(canopy, 0.25f), Vector3.one * 0.07f, spec.item, berry);
                break;
            }
            case ForageLook.FruitTree:
            {
                bool pawpaw = spec.item == "pawpaw";
                Color fruit = pawpaw ? new Color(0.55f, 0.6f, 0.2f) : new Color(0.72f, 0.12f, 0.08f);
                Vector3 size = pawpaw ? new Vector3(0.1f, 0.16f, 0.1f) : Vector3.one * 0.12f;
                for (int i = 0; i < 22; i++)
                    Shape(PrimitiveType.Sphere, crop.transform, OnEllipsoid(canopy, -0.6f), size, spec.item, fruit);
                for (int i = 0; i < 5; i++)
                    Shape(PrimitiveType.Sphere, crop.transform, RingOnGround(0.8f, 2.5f) + Vector3.up * 0.05f, size, spec.item, fruit);
                break;
            }
            case ForageLook.NutTree:
            {
                // Light enough to spot on the forest floor: tan hickory, green-husked black walnut, pale chestnut burrs.
                Color nut = spec.item == "chestnuts" ? new Color(0.66f, 0.62f, 0.3f)
                          : spec.item == "walnuts" ? new Color(0.42f, 0.52f, 0.18f) : new Color(0.66f, 0.55f, 0.34f);
                for (int i = 0; i < 40; i++)
                    Shape(PrimitiveType.Sphere, crop.transform, RingOnGround(0.7f, 2.8f) + Vector3.up * 0.05f, Vector3.one * 0.11f, spec.item, nut);
                break;
            }
            case ForageLook.GroundGreens:
            {
                bool onion = spec.item == "wild_onion";
                for (int i = 0; i < 16; i++)
                    Shape(PrimitiveType.Sphere, crop.transform, Scatter(1.1f) + Vector3.up * (onion ? 0.38f : 0.12f),
                          onion ? new Vector3(0.05f, 0.05f, 0.05f) : new Vector3(0.07f, 0.03f, 0.07f), spec.item,
                          onion ? new Color(0.85f, 0.8f, 0.9f) : new Color(0.95f, 0.8f, 0.1f));
                break;
            }
            case ForageLook.Cattail:
            {
                Transform plant = node.Find("Plant");
                foreach (Transform stem in plant)
                {
                    if (stem.name.StartsWith("Cattail Stem") && random.NextDouble() < 0.7)
                        Shape(PrimitiveType.Cylinder, crop.transform, stem.localPosition + Vector3.up * stem.localScale.y * 0.85f,
                              new Vector3(0.06f, 0.12f, 0.06f), spec.item, new Color(0.35f, 0.2f, 0.1f), stem.localRotation);
                }
                break;
            }
            case ForageLook.GroundMushroom:
                for (int i = 0; i < 7; i++)
                {
                    Vector3 at = Scatter(0.7f);
                    Shape(PrimitiveType.Cylinder, crop.transform, at + Vector3.up * 0.04f, new Vector3(0.03f, 0.04f, 0.03f), "Stem",
                          new Color(0.85f, 0.8f, 0.7f));
                    Shape(PrimitiveType.Sphere, crop.transform, at + Vector3.up * 0.13f, new Vector3(0.07f, 0.13f, 0.07f), spec.item,
                          new Color(0.45f, 0.35f, 0.22f));
                }
                break;
            case ForageLook.LogMushroom:
            {
                Color cap = spec.item == "dryads_saddle" ? new Color(0.75f, 0.6f, 0.4f) : new Color(0.78f, 0.76f, 0.72f);
                for (int i = 0; i < 9; i++)
                {
                    float x = Rand(-1.3f, 1.3f);
                    float side = random.NextDouble() < 0.5 ? -1f : 1f;
                    Shape(PrimitiveType.Sphere, crop.transform, new Vector3(x, Rand(0.25f, 0.45f), side * 0.27f),
                          new Vector3(Rand(0.18f, 0.28f), 0.04f, Rand(0.14f, 0.2f)), spec.item, cap,
                          Quaternion.Euler(side * Rand(5f, 15f), Rand(0f, 360f), 0f));
                }
                break;
            }
        }

        if (spec.look != ForageLook.FruitTree && spec.look != ForageLook.LogMushroom && spec.look != ForageLook.Cattail &&
            spec.look != ForageLook.BerryBush)
            SnapChildrenToGround(crop.transform, terrain);
        return crop;
    }

    // The canopy's bounds in the node's local space, from the plant's renderers.
    static Bounds Canopy(Transform node)
    {
        var bounds = new Bounds(Vector3.up, Vector3.one);
        bool first = true;
        Transform plant = node.Find("Plant");
        if (plant == null)
            return bounds;

        foreach (Renderer r in plant.GetComponentsInChildren<Renderer>())
        {
            Bounds b = r.bounds;
            var local = new Bounds(node.InverseTransformPoint(b.center), b.size);
            if (first) bounds = local; else bounds.Encapsulate(local);
            first = false;
        }
        return bounds;
    }

    // A point on the canopy's surface, in its upper part (minY -1..1 of the ellipsoid's height).
    static Vector3 OnEllipsoid(Bounds canopy, float minY)
    {
        float y = Rand(minY, 0.85f);
        float angle = Rand(0f, Mathf.PI * 2f);
        float r = Mathf.Sqrt(Mathf.Max(0f, 1f - y * y));
        // The foliage sits in the top part of a tree's bounds; the trunk makes up the rest.
        Vector3 extents = canopy.extents;
        Vector3 center = canopy.center;
        if (canopy.size.y > 3f)
        {
            extents.y = canopy.size.y * 0.3f;
            center.y = canopy.max.y - extents.y;
        }
        return center + new Vector3(Mathf.Cos(angle) * r * extents.x * 0.92f, y * extents.y * 0.92f, Mathf.Sin(angle) * r * extents.z * 0.92f);
    }

    static Vector3 RingOnGround(float min, float max)
    {
        float angle = Rand(0f, Mathf.PI * 2f), r = Rand(min, max);
        return new Vector3(Mathf.Cos(angle) * r, 0f, Mathf.Sin(angle) * r);
    }

    static Vector3 Scatter(float radius)
    {
        float angle = Rand(0f, Mathf.PI * 2f), r = Mathf.Sqrt((float)random.NextDouble()) * radius;
        return new Vector3(Mathf.Cos(angle) * r, 0f, Mathf.Sin(angle) * r);
    }

    static float Rand(float min, float max) => min + (float)random.NextDouble() * (max - min);

    // Lifts or lowers each child to follow the ground under it, keeping its height above the node's base.
    static void SnapChildrenToGround(Transform parent, Terrain terrain)
    {
        float baseY = parent.parent.position.y;
        foreach (Transform child in parent)
        {
            Vector3 world = child.position;
            float ground = terrain.SampleHeight(world) + terrain.transform.position.y;
            world.y += ground - baseY;
            child.position = world;
        }
    }

    static void Prefab(string name, Transform parent, float scale)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/Environment/{name}.prefab");
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one * scale;
    }

    static void AddBox(GameObject go, Vector3 center, Vector3 size)
    {
        var box = go.AddComponent<BoxCollider>();
        box.center = center;
        box.size = size;
    }

    static GameObject Shape(PrimitiveType type, Transform parent, Vector3 localPosition, Vector3 scale, string materialName,
                            Color color, Quaternion? rotation = null)
    {
        GameObject shape = GameObject.CreatePrimitive(type);
        Object.DestroyImmediate(shape.GetComponent<Collider>());
        shape.name = materialName;
        shape.transform.SetParent(parent, false);
        shape.transform.localPosition = localPosition;
        shape.transform.localRotation = rotation ?? Quaternion.Euler(0f, Rand(0f, 360f), 0f);
        shape.transform.localScale = scale;
        var renderer = shape.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = MaterialFor(materialName, color);
        renderer.shadowCastingMode = scale.magnitude < 0.3f ? UnityEngine.Rendering.ShadowCastingMode.Off : UnityEngine.Rendering.ShadowCastingMode.On;
        return shape;
    }

    // One HDRP Lit material per crop colour, saved under Assets/Art/Forage.
    static Material MaterialFor(string name, Color color)
    {
        string key = name.Replace("'", "");
        if (materials.TryGetValue(key, out Material cached))
            return cached;

        if (!AssetDatabase.IsValidFolder(MaterialFolder))
            AssetDatabase.CreateFolder("Assets/Art", "Forage");

        string path = $"{MaterialFolder}/{key}.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Fire/Fire Stone.mat"));
            AssetDatabase.CreateAsset(material, path);
        }
        material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Smoothness"))
            material.SetFloat("_Smoothness", name.Contains("berr") || name.Contains("apple") ? 0.55f : 0.2f);
        EditorUtility.SetDirty(material);
        materials[key] = material;
        return material;
    }
}
