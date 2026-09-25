using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Homestead > Place Deadfall: scatters piles of fallen branches through the property's woods as Firewood pickups for
// the Fire System (Core_Survival_System.md: fire needs fuel). Placed near trees, on gentle ground, away from water
// and spread apart. Re-running replaces the previous set. Like every ItemPickup, a pile that's been gathered comes back
// when the World reloads (pickups aren't saved yet), which for now stands in for new deadfall coming down.
public static class DeadfallPlacer
{
    const string RootName = "Deadfall";
    const int PileCount = 40;
    const float MinSpacing = 22f;
    const int Seed = 20260925;

    [MenuItem("Homestead/Place Deadfall")]
    public static void Place()
    {
        Terrain terrain = Terrain.activeTerrain;
        ItemDefinition firewood = AssetDatabase.LoadAssetAtPath<ItemDefinition>("Assets/Resources/Items/Materials/firewood.asset");
        Material bark = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Terrain/Bark.mat");
        if (terrain == null || firewood == null)
        {
            Debug.LogError("[DeadfallPlacer] Needs the property terrain in the open scene and the firewood item asset.");
            return;
        }

        GameObject old = GameObject.Find(RootName);
        if (old != null)
            Undo.DestroyObjectImmediate(old);
        var root = new GameObject(RootName);
        Undo.RegisterCreatedObjectUndo(root, "Place Deadfall");

        TerrainData data = terrain.terrainData;
        Vector3 origin = terrain.transform.position;
        var random = new System.Random(Seed);
        var placed = new List<Vector3>();
        TreeInstance[] trees = data.treeInstances;

        for (int attempt = 0; attempt < 4000 && placed.Count < PileCount; attempt++)
        {
            // Start beside a random tree so piles land in the woods, not the open pasture.
            TreeInstance tree = trees[random.Next(trees.Length)];
            float angle = (float)(random.NextDouble() * Mathf.PI * 2.0);
            float distance = 2.5f + (float)random.NextDouble() * 3f;
            Vector3 p = origin + Vector3.Scale(tree.position, data.size) +
                        new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * distance;

            float u = (p.x - origin.x) / data.size.x, v = (p.z - origin.z) / data.size.z;
            if (u < 0.05f || u > 0.95f || v < 0.05f || v > 0.95f)
                continue;
            if (data.GetSteepness(u, v) > 18f)
                continue;
            p.y = origin.y + data.GetInterpolatedHeight(u, v);

            // Not in or right beside water.
            if (Physics.Raycast(p + Vector3.up * 5f, Vector3.down, out RaycastHit hit, 10f, ~0, QueryTriggerInteraction.Ignore) &&
                hit.collider.GetComponentInParent<WaterSource>() != null)
                continue;
            if (Physics.CheckSphere(p, 3f, 1 << LayerMask.NameToLayer("Water"), QueryTriggerInteraction.Ignore))
                continue;

            bool crowded = false;
            foreach (Vector3 other in placed)
            {
                if (Vector3.Distance(other, p) < MinSpacing)
                {
                    crowded = true;
                    break;
                }
            }
            if (crowded)
                continue;

            placed.Add(p);
            CreatePile(root.transform, p, firewood, bark, random, placed.Count);
        }

        EditorSceneManager.MarkSceneDirty(root.scene);
        Debug.Log($"[DeadfallPlacer] Placed {placed.Count} deadfall piles.");
    }

    static void CreatePile(Transform parent, Vector3 position, ItemDefinition firewood, Material bark, System.Random random, int index)
    {
        var pile = new GameObject($"Deadfall {index}");
        pile.transform.SetParent(parent);
        pile.transform.position = position;
        pile.transform.rotation = Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f);

        // A few crossed branches lying on the ground.
        int branches = 3 + random.Next(2);
        for (int i = 0; i < branches; i++)
        {
            GameObject branch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Object.DestroyImmediate(branch.GetComponent<Collider>());
            branch.name = "Branch";
            branch.transform.SetParent(pile.transform, false);
            float length = 0.9f + (float)random.NextDouble() * 0.6f;
            float thickness = 0.07f + (float)random.NextDouble() * 0.05f;
            branch.transform.localScale = new Vector3(thickness, length * 0.5f, thickness);
            branch.transform.localPosition = new Vector3(((float)random.NextDouble() - 0.5f) * 0.3f, thickness * 0.5f + i * 0.05f,
                                                         ((float)random.NextDouble() - 0.5f) * 0.3f);
            branch.transform.localRotation = Quaternion.Euler(90f, i * 55f + (float)random.NextDouble() * 20f, 0f);
            if (bark != null)
                branch.GetComponent<MeshRenderer>().sharedMaterial = bark;
        }

        // Something solid for the interaction raycast to hit.
        var collider = pile.AddComponent<BoxCollider>();
        collider.center = new Vector3(0f, 0.15f, 0f);
        collider.size = new Vector3(1.2f, 0.3f, 1.2f);

        var pickup = pile.AddComponent<ItemPickup>();
        var serialized = new SerializedObject(pickup);
        serialized.FindProperty("item").objectReferenceValue = firewood;
        serialized.FindProperty("quantity").intValue = 2 + random.Next(2);
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }
}
