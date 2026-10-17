using System.Collections.Generic;
using UnityEngine;

// How sheltered a spot is from the sky (Weather_System.md: Visual Feedback — precipitation is lighter under partial
// cover and absent under full cover). 1 = full cover, anything solid overhead (a roof, once
// Building_Housing_System.md adds structures); tree canopy counts as partial cover. General rather than
// tree-specific: canopies are read from the active terrain's trees (their colliders are trunks only, so a raycast
// can't see them), everything else by raycasting upward. Presentation only.
public static class OverheadCover
{
    const float CellSize = 8f;
    const float MinCanopyHeight = 4f; // shorter prototypes (shrubs) don't shelter a standing player
    const float ProbeHeight = 60f;

    static readonly Vector2[] Samples =
    {
        Vector2.zero, new Vector2(1.5f, 0f), new Vector2(-1.5f, 0f), new Vector2(0f, 1.5f), new Vector2(0f, -1.5f),
    };

    static Terrain indexedTerrain;

    // Call after trees are added or removed (a tree felled) so the crowns are indexed again.
    public static void Refresh() => indexedTerrain = null;
    static TreePrototype[] indexedPrototypes;
    static readonly Dictionary<Vector2Int, List<Vector3>> canopies = new Dictionary<Vector2Int, List<Vector3>>();

    // canopyShelter: how much a full tree canopy blocks, 0–1.
    public static float At(Vector3 position, float canopyShelter)
    {
        if (Roofed(position))
            return 1f;
        return canopyShelter * CanopyFraction(position);
    }

    // Anything solid straight overhead other than the terrain itself (whose collider includes tree trunks).
    public static bool Roofed(Vector3 position)
    {
        return Physics.Raycast(position + Vector3.up * 0.1f, Vector3.up, out RaycastHit hit, ProbeHeight, ~0,
                               QueryTriggerInteraction.Ignore) && !(hit.collider is TerrainCollider);
    }

    // Share of a small patch around the point that sits under a tree crown, 0–1.
    public static float CanopyFraction(Vector3 position)
    {
        if (!Index())
            return 0f;

        int covered = 0;
        foreach (Vector2 offset in Samples)
        {
            if (UnderCanopy(new Vector2(position.x + offset.x, position.z + offset.y)))
                covered++;
        }
        return covered / (float)Samples.Length;
    }

    // Heights above the ground at which a tree crown is tested along the sun's line (a tall tree shades from low to high).
    static readonly float[] SunCrownHeights = { 4f, 8f, 12f };
    const float MaxSunOffset = 40f; // a very low sun is tested no further than this along the ground

    // How much of the DIRECT sun is kept off the player at a point, 0 (open sky) to 1, for a sun at the given elevation and
    // azimuth (degrees; azimuth is a Unity yaw, 90 = east). Unlike At, which looks straight up, this probes toward the sun,
    // because a low sun (Winter noon, every dawn and dusk) is blocked by what stands to its side, not overhead.
    //   - Anything solid on the line to the sun other than the terrain (whose collider includes tree trunks), a roof or a wall
    //     or a Tent, blocks it entirely; roofed is then true (no sky or ground glare is left either).
    //   - Tree crowns block canopyBlock of it with leaves on, bareBlock with them off, scaled by leafAmount (0 in Winter), over
    //     the share of a small patch around the point that the crowns cover along that line.
    // The terrain itself is ignored, so a hill doesn't hide a low sun. Presentation-grade, like the rest of this class.
    public static float SunBlocked(Vector3 position, float elevationDeg, float azimuthDeg, float leafAmount,
                                   float canopyBlock, float bareBlock, out bool roofed)
    {
        roofed = false;
        if (elevationDeg <= 0.5f)
            return 1f;

        float el = elevationDeg * Mathf.Deg2Rad, az = azimuthDeg * Mathf.Deg2Rad;
        var toSun = new Vector3(Mathf.Sin(az) * Mathf.Cos(el), Mathf.Sin(el), Mathf.Cos(az) * Mathf.Cos(el));
        if (Physics.Raycast(position + toSun * 0.1f, toSun, out RaycastHit hit, ProbeHeight, ~0, QueryTriggerInteraction.Ignore) &&
            !(hit.collider is TerrainCollider))
        {
            roofed = true;
            return 1f;
        }

        if (!Index())
            return 0f;

        float block = Mathf.Lerp(bareBlock, canopyBlock, Mathf.Clamp01(leafAmount));
        float alongPerMetre = 1f / Mathf.Tan(el);
        var toward = new Vector2(Mathf.Sin(az), Mathf.Cos(az));
        int covered = 0;
        foreach (Vector2 offset in Samples)
        {
            var start = new Vector2(position.x + offset.x, position.z + offset.y);
            foreach (float height in SunCrownHeights)
            {
                if (UnderCanopy(start + toward * Mathf.Min(height * alongPerMetre, MaxSunOffset)))
                {
                    covered++;
                    break;
                }
            }
        }
        return Mathf.Clamp01(block * covered / Samples.Length);
    }

    static bool UnderCanopy(Vector2 point)
    {
        var cell = new Vector2Int(Mathf.FloorToInt(point.x / CellSize), Mathf.FloorToInt(point.y / CellSize));
        for (int dz = -1; dz <= 1; dz++)
        for (int dx = -1; dx <= 1; dx++)
        {
            if (!canopies.TryGetValue(new Vector2Int(cell.x + dx, cell.y + dz), out List<Vector3> trees))
                continue;
            foreach (Vector3 tree in trees) // x, z = trunk position, y = crown radius squared
            {
                float ox = point.x - tree.x, oz = point.y - tree.z;
                if (ox * ox + oz * oz <= tree.y)
                    return true;
            }
        }
        return false;
    }

    // Buckets the active terrain's tree crowns into a grid, once per terrain (and again if its trees change).
    static bool Index()
    {
        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null || terrain.terrainData == null)
            return false;
        TerrainData data = terrain.terrainData;
        if (terrain == indexedTerrain && data.treePrototypes.Length == (indexedPrototypes?.Length ?? -1))
            return true;

        indexedTerrain = terrain;
        indexedPrototypes = data.treePrototypes;
        canopies.Clear();

        var crownRadius = new float[indexedPrototypes.Length];
        for (int i = 0; i < indexedPrototypes.Length; i++)
        {
            GameObject prefab = indexedPrototypes[i].prefab;
            MeshFilter filter = prefab != null ? prefab.GetComponentInChildren<MeshFilter>() : null;
            if (filter == null || filter.sharedMesh == null)
                continue;
            Bounds bounds = filter.sharedMesh.bounds;
            if (bounds.max.y >= MinCanopyHeight)
                crownRadius[i] = Mathf.Max(bounds.extents.x, bounds.extents.z);
        }

        Vector3 origin = terrain.transform.position;
        foreach (TreeInstance tree in data.treeInstances)
        {
            float radius = tree.prototypeIndex < crownRadius.Length ? crownRadius[tree.prototypeIndex] * tree.widthScale : 0f;
            if (radius <= 0f)
                continue;

            Vector3 world = origin + Vector3.Scale(tree.position, data.size);
            var cell = new Vector2Int(Mathf.FloorToInt(world.x / CellSize), Mathf.FloorToInt(world.z / CellSize));
            if (!canopies.TryGetValue(cell, out List<Vector3> list))
                canopies[cell] = list = new List<Vector3>();
            list.Add(new Vector3(world.x, radius * radius, world.z));
        }
        return true;
    }
}
