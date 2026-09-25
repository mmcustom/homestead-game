using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Homestead > Build Phase 2 Assets: the food-gathering batch's assets, built from simple shapes so they can be
// rebuilt or swapped for real models later — the Arrows and Rifle Rounds items; the fishing bobber; Rabbit Snare, Box
// Trap and Fish Trap prefabs; Deer, Turkey, Rabbit, Squirrel and Duck prefabs — then wires them in: the Foraging,
// Fishing, Trap and Wildlife managers on Bootstrap's Managers, the rod, weapon and trap-setting components on the
// Player prefab, and the tool HUD (crosshair, status line, progress bar) on the HUD prefab. Safe to re-run.
public static class Phase2Builder
{
    const string MaterialFolder = "Assets/Art/Wildlife";
    static readonly Dictionary<string, Material> materials = new Dictionary<string, Material>();

    [MenuItem("Homestead/Build Phase 2 Assets")]
    public static void Build()
    {
        materials.Clear();
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Wildlife"))
            AssetDatabase.CreateFolder("Assets/Prefabs", "Wildlife");
        Item("arrows", "Arrows", ItemCategory.Material, 0.04f, "Materials");
        Item("rifle_rounds", "Rifle Rounds", ItemCategory.Material, 0.02f, "Materials");

        BuildBobber();
        BuildSnare();
        BuildBoxTrap();
        BuildFishTrap();
        var animals = new Dictionary<string, Animal>
        {
            ["deer"] = BuildDeer(),
            ["turkey"] = BuildTurkey(),
            ["waterfowl"] = BuildDuck(),
            ["rabbit"] = BuildSmall("Rabbit", 0.35f, new Color(0.5f, 0.43f, 0.35f), ears: true),
            ["squirrel"] = BuildSmall("Squirrel", 0.25f, new Color(0.45f, 0.44f, 0.42f), ears: false),
        };
        AssetDatabase.SaveAssets();

        WireManagers(animals);
        WirePlayer();
        WireHud();
        AssetDatabase.SaveAssets();
        Debug.Log("[Phase2Builder] Phase 2 assets built and wired.");
    }

    // --- Items ---

    static void Item(string id, string displayName, ItemCategory category, float weight, string folder)
    {
        string path = $"Assets/Resources/Items/{folder}/{id}.asset";
        var item = AssetDatabase.LoadAssetAtPath<ItemDefinition>(path);
        if (item == null)
        {
            item = ScriptableObject.CreateInstance<ItemDefinition>();
            AssetDatabase.CreateAsset(item, path);
        }
        var so = new SerializedObject(item);
        so.FindProperty("id").stringValue = id;
        so.FindProperty("displayName").stringValue = displayName;
        so.FindProperty("category").enumValueIndex = (int)category;
        so.FindProperty("weightKg").floatValue = weight;
        so.FindProperty("shelfLifeDays").floatValue = 0f;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // --- Shapes and materials ---

    static Material Mat(string name, Color color, bool unlit = false)
    {
        if (materials.TryGetValue(name, out Material cached))
            return cached;
        if (!AssetDatabase.IsValidFolder(MaterialFolder))
            AssetDatabase.CreateFolder("Assets/Art", "Wildlife");

        string path = $"{MaterialFolder}/{name}.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            string source = unlit ? "Assets/Art/Weather/Lightning Bolt.mat" : "Assets/Art/Fire/Fire Stone.mat";
            material = new Material(AssetDatabase.LoadAssetAtPath<Material>(source));
            AssetDatabase.CreateAsset(material, path);
        }
        material.SetColor(unlit ? "_UnlitColor" : "_BaseColor", color);
        EditorUtility.SetDirty(material);
        materials[name] = material;
        return material;
    }

    static GameObject Part(PrimitiveType type, Transform parent, string name, Vector3 position, Vector3 scale, Material material,
                           Vector3? euler = null)
    {
        GameObject part = GameObject.CreatePrimitive(type);
        Object.DestroyImmediate(part.GetComponent<Collider>());
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = position;
        part.transform.localRotation = Quaternion.Euler(euler ?? Vector3.zero);
        part.transform.localScale = scale;
        part.GetComponent<MeshRenderer>().sharedMaterial = material;
        return part;
    }

    static T SavePrefab<T>(GameObject root, string path) where T : Component
    {
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab.GetComponent<T>();
    }

    static void Set(Object target, string field, Object value)
    {
        var so = new SerializedObject(target);
        so.FindProperty(field).objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void SetFloat(Object target, string field, float value)
    {
        var so = new SerializedObject(target);
        so.FindProperty(field).floatValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // --- Fishing bobber ---

    static GameObject BuildBobber()
    {
        var root = new GameObject("Bobber");
        Part(PrimitiveType.Sphere, root.transform, "Top", new Vector3(0f, 0.03f, 0f), Vector3.one * 0.07f, Mat("Bobber Red", new Color(0.8f, 0.1f, 0.08f)));
        Part(PrimitiveType.Sphere, root.transform, "Bottom", new Vector3(0f, -0.01f, 0f), Vector3.one * 0.065f, Mat("Bobber White", new Color(0.92f, 0.92f, 0.9f)));

        var lineObject = new GameObject("Line");
        lineObject.transform.SetParent(root.transform, false);
        var line = lineObject.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = 2;
        line.widthMultiplier = 0.006f;
        line.sharedMaterial = Mat("Fishing Line", new Color(0.75f, 0.75f, 0.72f), unlit: true);
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        return SavePrefab<Transform>(root, "Assets/Prefabs/Bobber.prefab").gameObject;
    }

    // --- Traps ---

    static Trap BuildSnare()
    {
        var root = new GameObject("Rabbit Snare");
        var body = new GameObject("Body");
        body.transform.SetParent(root.transform, false);
        Material wood = Mat("Stake", new Color(0.4f, 0.3f, 0.2f));
        Material wire = Mat("Snare Cord", new Color(0.55f, 0.5f, 0.4f));
        Part(PrimitiveType.Cylinder, body.transform, "Stake", new Vector3(0f, 0.2f, 0f), new Vector3(0.035f, 0.2f, 0.035f), wood);
        for (int i = 0; i < 10; i++)
        {
            float a = i / 10f * 360f;
            Vector3 p = Quaternion.Euler(0f, 0f, a) * new Vector3(0.09f, 0f, 0f);
            Part(PrimitiveType.Cylinder, body.transform, "Loop", new Vector3(0.16f, 0.14f + p.y, p.x),
                 new Vector3(0.008f, 0.03f, 0.008f), wire, new Vector3(a + 90f, 90f, 0f));
        }
        GameObject caught = Part(PrimitiveType.Sphere, root.transform, "Catch", new Vector3(0.25f, 0.08f, 0.05f), new Vector3(0.22f, 0.16f, 0.34f),
                                 Mat("Rabbit Fur", new Color(0.5f, 0.43f, 0.35f)));
        AddBox(root, new Vector3(0.1f, 0.2f, 0f), new Vector3(0.6f, 0.4f, 0.5f));

        Trap trap = root.AddComponent<Trap>();
        Set(trap, "catchVisual", caught);
        Set(trap, "body", body.transform);
        return SavePrefab<Trap>(root, "Assets/Prefabs/Rabbit Snare.prefab");
    }

    static Trap BuildBoxTrap()
    {
        var root = new GameObject("Box Trap");
        var body = new GameObject("Body");
        body.transform.SetParent(root.transform, false);
        Material plank = Mat("Box Trap Wood", new Color(0.45f, 0.34f, 0.22f));
        Part(PrimitiveType.Cube, body.transform, "Floor", new Vector3(0f, 0.02f, 0f), new Vector3(0.36f, 0.04f, 0.75f), plank);
        Part(PrimitiveType.Cube, body.transform, "Left", new Vector3(-0.17f, 0.17f, 0f), new Vector3(0.02f, 0.3f, 0.75f), plank);
        Part(PrimitiveType.Cube, body.transform, "Right", new Vector3(0.17f, 0.17f, 0f), new Vector3(0.02f, 0.3f, 0.75f), plank);
        Part(PrimitiveType.Cube, body.transform, "Roof", new Vector3(0f, 0.33f, 0f), new Vector3(0.36f, 0.02f, 0.75f), plank);
        Part(PrimitiveType.Cube, body.transform, "Back", new Vector3(0f, 0.17f, -0.37f), new Vector3(0.36f, 0.3f, 0.02f), plank);
        Part(PrimitiveType.Cube, body.transform, "Door Open", new Vector3(0f, 0.42f, 0.37f), new Vector3(0.34f, 0.02f, 0.2f), plank, new Vector3(-60f, 0f, 0f));
        GameObject shut = Part(PrimitiveType.Cube, root.transform, "Catch", new Vector3(0f, 0.17f, 0.375f), new Vector3(0.34f, 0.3f, 0.02f), plank);
        GameObject bait = Part(PrimitiveType.Sphere, root.transform, "Bait", new Vector3(0f, 0.08f, -0.2f), Vector3.one * 0.08f,
                               Mat("Bait Apple", new Color(0.72f, 0.12f, 0.08f)));
        AddBox(root, new Vector3(0f, 0.2f, 0f), new Vector3(0.4f, 0.4f, 0.8f));

        Trap trap = root.AddComponent<Trap>();
        Set(trap, "catchVisual", shut);
        Set(trap, "baitVisual", bait);
        Set(trap, "body", body.transform);
        return SavePrefab<Trap>(root, "Assets/Prefabs/Box Trap.prefab");
    }

    static Trap BuildFishTrap()
    {
        var root = new GameObject("Fish Trap");
        var body = new GameObject("Body");
        body.transform.SetParent(root.transform, false);
        Material wicker = Mat("Wicker", new Color(0.4f, 0.32f, 0.18f));
        Part(PrimitiveType.Cylinder, body.transform, "Basket", new Vector3(0f, -0.05f, 0f), new Vector3(0.4f, 0.4f, 0.4f), wicker, new Vector3(90f, 0f, 0f));
        Part(PrimitiveType.Cylinder, body.transform, "Marker Pole", new Vector3(0.3f, 0.5f, 0f), new Vector3(0.03f, 0.6f, 0.03f), Mat("Marker White", new Color(0.9f, 0.9f, 0.88f)));
        Part(PrimitiveType.Cube, body.transform, "Flag", new Vector3(0.38f, 1.02f, 0f), new Vector3(0.16f, 0.1f, 0.01f), Mat("Flag Cloth", new Color(0.75f, 0.2f, 0.1f)));
        var caught = new GameObject("Catch");
        caught.transform.SetParent(root.transform, false);
        Material scales = Mat("Fish Scales", new Color(0.6f, 0.62f, 0.55f));
        Part(PrimitiveType.Sphere, caught.transform, "Fish", new Vector3(-0.05f, 0.14f, 0.05f), new Vector3(0.08f, 0.06f, 0.26f), scales, new Vector3(0f, 20f, 0f));
        Part(PrimitiveType.Sphere, caught.transform, "Fish", new Vector3(0.07f, 0.13f, -0.08f), new Vector3(0.07f, 0.05f, 0.22f), scales, new Vector3(0f, -35f, 0f));
        AddBox(root, new Vector3(0.1f, 0.3f, 0f), new Vector3(0.6f, 1.2f, 0.9f));

        Trap trap = root.AddComponent<Trap>();
        Set(trap, "catchVisual", caught);
        Set(trap, "body", body.transform);
        return SavePrefab<Trap>(root, "Assets/Prefabs/Fish Trap.prefab");
    }

    static void AddBox(GameObject go, Vector3 center, Vector3 size)
    {
        var box = go.AddComponent<BoxCollider>();
        box.center = center;
        box.size = size;
    }

    // --- Animals ---

    static (GameObject root, Transform model, Transform vitals) AnimalRoot(string name, Vector3 vitalsAt)
    {
        var root = new GameObject(name);
        var model = new GameObject("Model");
        model.transform.SetParent(root.transform, false);
        var vitals = new GameObject("Vitals");
        vitals.transform.SetParent(model.transform, false);
        vitals.transform.localPosition = vitalsAt;
        return (root, model.transform, vitals.transform);
    }

    static Animal Finish(GameObject root, Transform model, Transform vitals, float vitalsRadius, string species, GameTier tier,
                         float walk, float run, float hearing, bool swims, bool hops, GameObject antlers, string path)
    {
        Animal animal = root.AddComponent<Animal>();
        var so = new SerializedObject(animal);
        so.FindProperty("speciesId").stringValue = species;
        so.FindProperty("tier").enumValueIndex = (int)tier;
        so.FindProperty("walkSpeed").floatValue = walk;
        so.FindProperty("runSpeed").floatValue = run;
        so.FindProperty("hearing").floatValue = hearing;
        so.FindProperty("swims").boolValue = swims;
        so.FindProperty("hops").boolValue = hops;
        so.FindProperty("antlers").objectReferenceValue = antlers;
        so.FindProperty("model").objectReferenceValue = model;
        so.FindProperty("vitals").objectReferenceValue = vitals;
        so.FindProperty("vitalsRadius").floatValue = vitalsRadius;
        so.ApplyModifiedPropertiesWithoutUndo();
        return SavePrefab<Animal>(root, path);
    }

    static void BodyCollider(Transform model, Vector3 center, float radius, float length)
    {
        var body = new GameObject("Body");
        body.transform.SetParent(model, false);
        var capsule = body.AddComponent<CapsuleCollider>();
        capsule.center = center;
        capsule.radius = radius;
        capsule.height = length;
        capsule.direction = 2; // along Z, nose to tail
    }

    static Animal BuildDeer()
    {
        var (root, model, vitals) = AnimalRoot("Deer", new Vector3(0f, 1.0f, 0.45f));
        Material coat = Mat("Deer Coat", new Color(0.47f, 0.33f, 0.2f));
        Material pale = Mat("Deer Belly", new Color(0.82f, 0.76f, 0.66f));
        Material dark = Mat("Hoof", new Color(0.12f, 0.1f, 0.08f));
        Part(PrimitiveType.Capsule, model, "Torso", new Vector3(0f, 1.02f, 0f), new Vector3(0.52f, 0.72f, 0.56f), coat, new Vector3(90f, 0f, 0f));
        Part(PrimitiveType.Sphere, model, "Belly", new Vector3(0f, 0.9f, 0f), new Vector3(0.4f, 0.3f, 1.1f), pale);
        Part(PrimitiveType.Cylinder, model, "Neck", new Vector3(0f, 1.35f, 0.62f), new Vector3(0.2f, 0.3f, 0.22f), coat, new Vector3(35f, 0f, 0f));
        Part(PrimitiveType.Sphere, model, "Head", new Vector3(0f, 1.62f, 0.85f), new Vector3(0.2f, 0.22f, 0.38f), coat, new Vector3(20f, 0f, 0f));
        Part(PrimitiveType.Sphere, model, "Nose", new Vector3(0f, 1.55f, 1.02f), Vector3.one * 0.07f, dark);
        foreach (float x in new[] { -0.08f, 0.08f })
            Part(PrimitiveType.Sphere, model, "Ear", new Vector3(x * 1.6f, 1.78f, 0.78f), new Vector3(0.05f, 0.14f, 0.08f), coat, new Vector3(0f, 0f, x > 0 ? -30f : 30f));
        Part(PrimitiveType.Sphere, model, "Tail", new Vector3(0f, 1.1f, -0.72f), new Vector3(0.1f, 0.16f, 0.08f), pale);
        foreach (float x in new[] { -0.16f, 0.16f })
        {
            foreach (float z in new[] { -0.48f, 0.45f })
            {
                Part(PrimitiveType.Cylinder, model, "Leg", new Vector3(x, 0.42f, z), new Vector3(0.08f, 0.42f, 0.08f), coat);
                Part(PrimitiveType.Cylinder, model, "Hoof", new Vector3(x, 0.03f, z), new Vector3(0.07f, 0.03f, 0.07f), dark);
            }
        }

        var antlers = new GameObject("Antlers");
        antlers.transform.SetParent(model, false);
        Material bone = Mat("Antler", new Color(0.72f, 0.64f, 0.5f));
        foreach (float side in new[] { -1f, 1f })
        {
            Part(PrimitiveType.Cylinder, antlers.transform, "Beam", new Vector3(side * 0.12f, 1.92f, 0.75f), new Vector3(0.035f, 0.2f, 0.035f), bone, new Vector3(-10f, 0f, side * -35f));
            Part(PrimitiveType.Cylinder, antlers.transform, "Tine", new Vector3(side * 0.2f, 2.05f, 0.85f), new Vector3(0.025f, 0.1f, 0.025f), bone, new Vector3(30f, 0f, side * -10f));
            Part(PrimitiveType.Cylinder, antlers.transform, "Tine", new Vector3(side * 0.24f, 2.12f, 0.7f), new Vector3(0.025f, 0.1f, 0.025f), bone, new Vector3(-10f, 0f, side * -15f));
        }

        BodyCollider(model, new Vector3(0f, 1.0f, 0.1f), 0.32f, 1.9f);
        return Finish(root, model, vitals, 0.22f, "deer", GameTier.Large, 1.1f, 9f, 32f, false, false, antlers, "Assets/Prefabs/Wildlife/Deer.prefab");
    }

    static Animal BuildTurkey()
    {
        var (root, model, vitals) = AnimalRoot("Turkey", new Vector3(0f, 0.55f, 0.08f));
        Material plumage = Mat("Turkey Plumage", new Color(0.24f, 0.18f, 0.12f));
        Material fan = Mat("Turkey Fan", new Color(0.35f, 0.25f, 0.15f));
        Material skin = Mat("Turkey Head", new Color(0.7f, 0.2f, 0.18f));
        Material leg = Mat("Turkey Leg", new Color(0.6f, 0.45f, 0.35f));
        Part(PrimitiveType.Sphere, model, "Body", new Vector3(0f, 0.55f, 0f), new Vector3(0.42f, 0.4f, 0.55f), plumage);
        Part(PrimitiveType.Sphere, model, "Tail Fan", new Vector3(0f, 0.7f, -0.3f), new Vector3(0.6f, 0.45f, 0.06f), fan, new Vector3(-20f, 0f, 0f));
        Part(PrimitiveType.Cylinder, model, "Neck", new Vector3(0f, 0.85f, 0.22f), new Vector3(0.07f, 0.15f, 0.07f), skin, new Vector3(20f, 0f, 0f));
        Part(PrimitiveType.Sphere, model, "Head", new Vector3(0f, 1.0f, 0.28f), new Vector3(0.09f, 0.1f, 0.13f), skin);
        foreach (float x in new[] { -0.08f, 0.08f })
            Part(PrimitiveType.Cylinder, model, "Leg", new Vector3(x, 0.17f, 0.02f), new Vector3(0.03f, 0.17f, 0.03f), leg);
        BodyCollider(model, new Vector3(0f, 0.6f, 0.05f), 0.22f, 0.7f);
        return Finish(root, model, vitals, 0.12f, "turkey", GameTier.Medium, 0.8f, 6.5f, 30f, false, false, null, "Assets/Prefabs/Wildlife/Turkey.prefab");
    }

    static Animal BuildDuck()
    {
        var (root, model, vitals) = AnimalRoot("Duck", new Vector3(0f, 0.1f, 0.05f));
        Material body = Mat("Duck Body", new Color(0.45f, 0.4f, 0.35f));
        Material head = Mat("Duck Head", new Color(0.1f, 0.35f, 0.18f));
        Material bill = Mat("Duck Bill", new Color(0.85f, 0.65f, 0.15f));
        Part(PrimitiveType.Sphere, model, "Body", new Vector3(0f, 0.08f, 0f), new Vector3(0.28f, 0.2f, 0.45f), body);
        Part(PrimitiveType.Sphere, model, "Head", new Vector3(0f, 0.28f, 0.2f), Vector3.one * 0.13f, head);
        Part(PrimitiveType.Sphere, model, "Bill", new Vector3(0f, 0.26f, 0.29f), new Vector3(0.05f, 0.025f, 0.09f), bill);
        BodyCollider(model, new Vector3(0f, 0.12f, 0.05f), 0.15f, 0.55f);
        return Finish(root, model, vitals, 0.12f, "waterfowl", GameTier.Medium, 0.5f, 7f, 26f, true, false, null, "Assets/Prefabs/Wildlife/Duck.prefab");
    }

    static Animal BuildSmall(string name, float length, Color color, bool ears)
    {
        var (root, model, vitals) = AnimalRoot(name, new Vector3(0f, length * 0.45f, 0f));
        Material fur = Mat(name + " Fur", color);
        Material pale = Mat(name + " Belly", Color.Lerp(color, Color.white, 0.5f));
        Part(PrimitiveType.Sphere, model, "Body", new Vector3(0f, length * 0.42f, 0f), new Vector3(length * 0.6f, length * 0.55f, length), fur);
        Part(PrimitiveType.Sphere, model, "Head", new Vector3(0f, length * 0.7f, length * 0.5f), Vector3.one * length * 0.42f, fur);
        if (ears)
        {
            foreach (float x in new[] { -0.03f, 0.03f })
                Part(PrimitiveType.Sphere, model, "Ear", new Vector3(x, length * 1.05f, length * 0.42f), new Vector3(0.03f, 0.13f, 0.05f), fur);
            Part(PrimitiveType.Sphere, model, "Tail", new Vector3(0f, length * 0.5f, -length * 0.5f), Vector3.one * 0.07f, pale);
        }
        else
        {
            Part(PrimitiveType.Sphere, model, "Tail", new Vector3(0f, length * 0.9f, -length * 0.55f), new Vector3(0.09f, 0.26f, 0.09f), fur, new Vector3(-20f, 0f, 0f));
        }
        BodyCollider(model, new Vector3(0f, length * 0.45f, length * 0.1f), length * 0.35f, length * 1.4f);
        float hearing = ears ? 16f : 14f;
        return Finish(root, model, vitals, length * 0.4f, name.ToLower(), GameTier.Small, 0.8f, 6f, hearing, false, ears, null,
                      $"Assets/Prefabs/Wildlife/{name}.prefab");
    }

    // --- Wiring ---

    static void WireManagers(Dictionary<string, Animal> animals)
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/Bootstrap.unity");
        GameObject managers = GameObject.Find("Managers");
        Get<ForagingManager>(managers);
        Get<FishingManager>(managers);

        // Reloaded by path: references held from the save calls don't survive into the scene reliably.
        var traps = new SerializedObject(Get<TrapManager>(managers));
        traps.FindProperty("prefabs.rabbitSnare").objectReferenceValue = Load<Trap>("Assets/Prefabs/Rabbit Snare.prefab");
        traps.FindProperty("prefabs.boxTrap").objectReferenceValue = Load<Trap>("Assets/Prefabs/Box Trap.prefab");
        traps.FindProperty("prefabs.fishTrap").objectReferenceValue = Load<Trap>("Assets/Prefabs/Fish Trap.prefab");
        traps.ApplyModifiedPropertiesWithoutUndo();

        var wildlife = new SerializedObject(Get<WildlifeManager>(managers));
        SerializedProperty species = wildlife.FindProperty("species");
        for (int i = 0; i < species.arraySize; i++)
        {
            SerializedProperty entry = species.GetArrayElementAtIndex(i);
            string id = entry.FindPropertyRelative("speciesId").stringValue;
            if (animals.TryGetValue(id, out Animal prefab) && prefab != null)
                entry.FindPropertyRelative("prefab").objectReferenceValue = Load<Animal>(AssetDatabase.GetAssetPath(prefab));
        }
        wildlife.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    static T Load<T>(string path) where T : Component
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        return prefab != null ? prefab.GetComponent<T>() : null;
    }

    static T Get<T>(GameObject go) where T : Component
    {
        T component = go.GetComponent<T>();
        return component != null ? component : go.AddComponent<T>();
    }

    static void WirePlayer()
    {
        const string path = "Assets/Prefabs/Player.prefab";
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        var player = root.GetComponent<PlayerController>();
        FishingRod rod = Get<FishingRod>(root);
        Set(rod, "player", player);
        Set(rod, "bobberPrefab", AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Bobber.prefab"));
        Set(Get<HuntingWeapon>(root), "player", player);
        Set(Get<TrapSetter>(root), "player", player);
        PrefabUtility.SaveAsPrefabAsset(root, path);
        PrefabUtility.UnloadPrefabContents(root);
    }

    static void WireHud()
    {
        const string path = "Assets/Prefabs/HUD.prefab";
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        Transform old = root.transform.Find("Tool HUD");
        if (old != null)
            Object.DestroyImmediate(old.gameObject);

        var hudObject = new GameObject("Tool HUD", typeof(RectTransform));
        hudObject.transform.SetParent(root.transform, false);
        ((RectTransform)hudObject.transform).Fill();

        Image crosshair = UiKit.Image(hudObject.transform, "Crosshair", new Color(0.93f, 0.89f, 0.78f, 0.85f));
        crosshair.rectTransform.anchorMin = crosshair.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        crosshair.rectTransform.sizeDelta = new Vector2(5f, 5f);

        Text status = UiKit.Text(hudObject.transform, "Status", "", 22, UiKit.Cream, TextAnchor.MiddleCenter);
        status.rectTransform.anchorMin = status.rectTransform.anchorMax = new Vector2(0.5f, 0.3f);
        status.rectTransform.sizeDelta = new Vector2(900f, 34f);
        status.horizontalOverflow = HorizontalWrapMode.Overflow;
        status.gameObject.AddComponent<Shadow>().effectColor = new Color(0f, 0f, 0f, 0.8f);

        Image bar = UiKit.Image(hudObject.transform, "Bar", new Color(0f, 0f, 0f, 0.5f));
        bar.rectTransform.anchorMin = bar.rectTransform.anchorMax = new Vector2(0.5f, 0.3f);
        bar.rectTransform.sizeDelta = new Vector2(260f, 10f);
        bar.rectTransform.anchoredPosition = new Vector2(0f, -26f);
        Image fill = UiKit.Image(bar.rectTransform, "Fill", new Color(0.78f, 0.84f, 0.55f, 1f));
        fill.rectTransform.anchorMin = Vector2.zero;
        fill.rectTransform.anchorMax = new Vector2(0f, 1f);
        fill.rectTransform.offsetMin = fill.rectTransform.offsetMax = Vector2.zero;

        Image flashPanel = UiKit.Image(hudObject.transform, "Flash", UiKit.PanelColor);
        flashPanel.rectTransform.anchorMin = flashPanel.rectTransform.anchorMax = new Vector2(0.5f, 0.64f);
        flashPanel.rectTransform.sizeDelta = new Vector2(760f, 50f);
        var flashGroup = flashPanel.gameObject.AddComponent<CanvasGroup>();
        flashGroup.alpha = 0f;
        flashGroup.blocksRaycasts = false;
        Text flash = UiKit.Text(flashPanel.transform, "Label", "", 24, UiKit.Cream, TextAnchor.MiddleCenter);
        flash.rectTransform.Fill(12f, 0f, 12f, 0f);

        ToolHud hud = hudObject.AddComponent<ToolHud>();
        Set(hud, "crosshair", crosshair);
        Set(hud, "statusLabel", status);
        Set(hud, "bar", bar.rectTransform);
        Set(hud, "barFill", fill.rectTransform);
        Set(hud, "flashGroup", flashGroup);
        Set(hud, "flashLabel", flash);

        PrefabUtility.SaveAsPrefabAsset(root, path);
        PrefabUtility.UnloadPrefabContents(root);
    }
}
