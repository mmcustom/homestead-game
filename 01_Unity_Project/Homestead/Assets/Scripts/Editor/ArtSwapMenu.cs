using UnityEditor;
using UnityEngine;

// Mike's playtest helpers for the prefab swap point (ArtSwap): one menu item makes a simple test prefab for the Kiln at Resources/Art/structure_kiln.prefab, the other removes it
// again, so the Kiln goes back to its primitives. The test prefab (and its materials folder) is NOT to be committed; both items refuse to touch a prefab that is not the test one.
public static class ArtSwapMenu
{
    const string ArtFolder = "Assets/Resources/Art";
    const string PrefabPath = ArtFolder + "/structure_kiln.prefab";
    const string MaterialFolder = ArtFolder + "/_TestKilnMaterials";
    const string MarkerName = "TestKiln_Marker"; // an empty child that says "this is the generated test prefab"

    // The hands test prefab (Hands and tools M0a): Resources/Art/hands_player.prefab, with its own marker and materials folder so it never touches the Kiln's.
    const string HandsPrefabPath = ArtFolder + "/hands_player.prefab";
    const string HandsMaterialFolder = ArtFolder + "/_TestHandsMaterials";
    const string HandsMarkerName = "TestHands_Marker";

    [MenuItem("Homestead/Art/Create Test Kiln Prefab")]
    public static void CreateTestKiln()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null)
        {
            Debug.LogWarning("[Art] " + PrefabPath + " already exists; use Homestead/Art/Remove Test Kiln Prefab first (it only removes the generated test prefab).");
            return;
        }
        if (!AssetDatabase.IsValidFolder(ArtFolder))
            AssetDatabase.CreateFolder("Assets/Resources", "Art");
        if (!AssetDatabase.IsValidFolder(MaterialFolder))
            AssetDatabase.CreateFolder(ArtFolder, "_TestKilnMaterials");

        var root = new GameObject("structure_kiln");
        new GameObject(MarkerName).transform.SetParent(root.transform, false);

        // The always-on body: a low slab.
        Cube(root.transform, "Body", new Vector3(0f, 0.15f, 0f), new Vector3(2.4f, 0.3f, 2.4f), new Color(0.55f, 0.5f, 0.42f));

        // One block per state (only the one for the Kiln's phase is shown) and a glow for the firing.
        Cube(root.transform, "State_Empty", new Vector3(0f, 0.65f, 0f), new Vector3(1.2f, 0.7f, 1.2f), new Color(0.75f, 0.55f, 0.4f));
        Cube(root.transform, "State_Loaded", new Vector3(0f, 0.65f, 0f), new Vector3(1.2f, 0.7f, 1.2f), new Color(0.3f, 0.45f, 0.8f));
        Cube(root.transform, "State_Firing", new Vector3(0f, 0.65f, 0f), new Vector3(1.2f, 0.7f, 1.2f), new Color(0.25f, 0.2f, 0.2f));
        Cube(root.transform, "State_Done", new Vector3(0f, 0.65f, 0f), new Vector3(1.2f, 0.7f, 1.2f), new Color(0.85f, 0.85f, 0.8f));
        Cube(root.transform, "On_Firing", new Vector3(0f, 0.45f, 1.15f), new Vector3(0.5f, 0.4f, 0.2f), new Color(1f, 0.55f, 0.1f));

        // The anchors, each an empty object with a small marker cube so the author can see it. Where the code puts them if they were missing.
        Anchor(root.transform, ArtSwap.AnchorSmoke, new Vector3(0f, ArtSwap.KilnSmokeHeight, 0f), new Color(0.2f, 0.9f, 0.9f));
        Anchor(root.transform, ArtSwap.AnchorMouth, new Vector3(0f, 0.45f, 0.95f), new Color(1f, 0.6f, 0.1f));
        Anchor(root.transform, ArtSwap.AnchorStock, new Vector3(-1.45f, 0f, 0f), new Color(0.3f, 0.9f, 0.3f));

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        ArtSwap.ResetCache();
        Debug.Log("[Art] Test Kiln prefab created at " + PrefabPath + " (not to be committed). Remove it with Homestead/Art/Remove Test Kiln Prefab.");
    }

    [MenuItem("Homestead/Art/Remove Test Kiln Prefab")]
    public static void RemoveTestKiln()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null)
        {
            Debug.Log("[Art] No Kiln prefab at " + PrefabPath + "; nothing to remove.");
            ArtSwap.ResetCache();
            return;
        }
        if (prefab.transform.Find(MarkerName) == null)
        {
            Debug.LogWarning("[Art] " + PrefabPath + " is not the generated test prefab (it has no " + MarkerName + " child), so it was left alone.");
            return;
        }
        AssetDatabase.DeleteAsset(PrefabPath);
        if (AssetDatabase.IsValidFolder(MaterialFolder))
            AssetDatabase.DeleteAsset(MaterialFolder);
        if (AssetDatabase.IsValidFolder(ArtFolder) && AssetDatabase.FindAssets("", new[] { ArtFolder }).Length == 0)
            AssetDatabase.DeleteAsset(ArtFolder);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        ArtSwap.ResetCache();
        Debug.Log("[Art] Test Kiln prefab removed; the Kiln draws its primitives again.");
    }

    [MenuItem("Homestead/Art/Create Test Hands Prefab")]
    public static void CreateTestHands()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(HandsPrefabPath) != null)
        {
            Debug.LogWarning("[Art] " + HandsPrefabPath + " already exists; use Homestead/Art/Remove Test Hands Prefab first (it only removes the generated test prefab).");
            return;
        }
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder(ArtFolder))
            AssetDatabase.CreateFolder("Assets/Resources", "Art");
        if (!AssetDatabase.IsValidFolder(HandsMaterialFolder))
            AssetDatabase.CreateFolder(ArtFolder, "_TestHandsMaterials");

        var root = new GameObject(ArtSwap.HandsId);
        new GameObject(HandsMarkerName).transform.SetParent(root.transform, false);

        // Two clearly different hands (a blue right, a magenta left) so the swap is unmistakable: a flat slab forearm and a block mitten each, wrist at the hand's origin.
        BuildTestHand(root.transform, ArtSwap.HandRName, ArtSwap.AnchorHandR, new Color(0.2f, 0.45f, 0.95f));
        BuildTestHand(root.transform, ArtSwap.HandLName, ArtSwap.AnchorHandL, new Color(0.95f, 0.25f, 0.7f));

        PrefabUtility.SaveAsPrefabAsset(root, HandsPrefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        ArtSwap.ResetCache();
        Debug.Log("[Art] Test Hands prefab created at " + HandsPrefabPath + " (not to be committed). Enter Play Mode to see it; remove it with Homestead/Art/Remove Test Hands Prefab.");
    }

    [MenuItem("Homestead/Art/Remove Test Hands Prefab")]
    public static void RemoveTestHands()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(HandsPrefabPath);
        if (prefab == null)
        {
            Debug.Log("[Art] No hands prefab at " + HandsPrefabPath + "; nothing to remove.");
            ArtSwap.ResetCache();
            return;
        }
        if (prefab.transform.Find(HandsMarkerName) == null)
        {
            Debug.LogWarning("[Art] " + HandsPrefabPath + " is not the generated test prefab (it has no " + HandsMarkerName + " child), so it was left alone.");
            return;
        }
        AssetDatabase.DeleteAsset(HandsPrefabPath);
        if (AssetDatabase.IsValidFolder(HandsMaterialFolder))
            AssetDatabase.DeleteAsset(HandsMaterialFolder);
        if (AssetDatabase.IsValidFolder(ArtFolder) && AssetDatabase.FindAssets("", new[] { ArtFolder }).Length == 0)
            AssetDatabase.DeleteAsset(ArtFolder);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        ArtSwap.ResetCache();
        Debug.Log("[Art] Test Hands prefab removed; the player has the primitive hands again.");
    }

    static void BuildTestHand(Transform parent, string handName, string anchorName, Color color)
    {
        var hand = new GameObject(handName).transform;
        hand.SetParent(parent, false);
        Cube(hand, "Forearm", new Vector3(0f, 0f, -0.2f), new Vector3(0.09f, 0.05f, 0.4f), color, HandsMaterialFolder);
        Cube(hand, "Mitten", new Vector3(0f, 0f, 0.06f), new Vector3(0.11f, 0.05f, 0.14f), color * 0.8f, HandsMaterialFolder);
        var anchor = new GameObject(anchorName).transform;
        anchor.SetParent(hand, false);
        anchor.localPosition = ArtSwap.PalmFallback;
        Cube(anchor, "Marker", Vector3.zero, Vector3.one * 0.03f, Color.yellow, HandsMaterialFolder);
    }

    static GameObject Cube(Transform parent, string name, Vector3 position, Vector3 scale, Color color, string materialFolder = MaterialFolder)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Object.DestroyImmediate(cube.GetComponent<Collider>());
        cube.name = name;
        cube.transform.SetParent(parent, false);
        cube.transform.localPosition = position;
        cube.transform.localScale = scale;
        cube.GetComponent<MeshRenderer>().sharedMaterial = MaterialFor(color, materialFolder);
        return cube;
    }

    static void Anchor(Transform parent, string name, Vector3 position, Color color)
    {
        var anchor = new GameObject(name);
        anchor.transform.SetParent(parent, false);
        anchor.transform.localPosition = position;
        Cube(anchor.transform, "Marker", Vector3.zero, Vector3.one * 0.15f, color);
    }

    static Material MaterialFor(Color color, string folder = MaterialFolder)
    {
        string path = folder + "/Test_" + ColorUtility.ToHtmlStringRGB(color) + ".mat";
        var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null)
            return existing;
        Shader shader = Shader.Find("HDRP/Lit");
        Material material;
        if (shader != null)
        {
            material = new Material(shader);
        }
        else
        {
            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            material = new Material(temp.GetComponent<MeshRenderer>().sharedMaterial);
            Object.DestroyImmediate(temp);
        }
        material.SetColor("_BaseColor", color);
        AssetDatabase.CreateAsset(material, path);
        return material;
    }
}
