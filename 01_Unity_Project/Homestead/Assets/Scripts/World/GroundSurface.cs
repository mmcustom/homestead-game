using System;
using System.Collections.Generic;
using UnityEngine;

// Audio_System.md's footstep surfaces.
public enum SurfaceType { Grass, Dirt, Gravel }

[Serializable]
public struct TerrainLayerSurface
{
    public TerrainLayer layer;
    public SurfaceType surface;
}

// Marks what a ground collider is made of, for footstep sounds. Put it on the collider's object or a parent.
// Ground without one uses PlayerAudio's default surface. On a Terrain, each texture layer can be mapped to a
// surface instead (Property_Layout.md: Grass by default, Dirt on trails, Gravel at creek and pond banks) — the
// listed layer painted strongest at the player's feet wins. Unlisted layers (e.g. GroundSnow's) are ignored, so
// snow over a trail still sounds like the trail.
public class GroundSurface : MonoBehaviour
{
    [Tooltip("The surface for plain colliders, and for terrain layers not listed below.")]
    [SerializeField] SurfaceType surface = SurfaceType.Grass;
    [Tooltip("On a Terrain: the surface each texture layer counts as.")]
    [SerializeField] List<TerrainLayerSurface> terrainLayers = new List<TerrainLayerSurface>();

    // Terrain painting is read once. The only runtime repaint is GroundSnow's unlisted layer, which scales the
    // listed layers evenly, so the strongest listed layer stays the same.
    Terrain terrain;
    float[,,] alphamaps;
    SurfaceType[] layerSurfaces;
    bool[] layerListed;

    public SurfaceType Surface => surface;

    public SurfaceType SurfaceAt(Vector3 worldPoint)
    {
        if (terrainLayers.Count == 0 || !CacheTerrain())
            return surface;

        TerrainData data = terrain.terrainData;
        Vector3 local = worldPoint - terrain.transform.position;
        int res = data.alphamapResolution;
        int x = Mathf.Clamp((int)(local.x / data.size.x * res), 0, res - 1);
        int z = Mathf.Clamp((int)(local.z / data.size.z * res), 0, res - 1);

        int strongest = -1;
        for (int i = 0; i < layerSurfaces.Length; i++)
        {
            if (layerListed[i] && (strongest < 0 || alphamaps[z, x, i] > alphamaps[z, x, strongest]))
                strongest = i;
        }
        return strongest >= 0 ? layerSurfaces[strongest] : surface;
    }

    bool CacheTerrain()
    {
        if (alphamaps != null)
            return true;
        if (!TryGetComponent(out terrain) || terrain.terrainData == null)
            return false;

        TerrainData data = terrain.terrainData;
        alphamaps = data.GetAlphamaps(0, 0, data.alphamapWidth, data.alphamapHeight);
        layerSurfaces = new SurfaceType[data.alphamapLayers];
        layerListed = new bool[data.alphamapLayers];
        for (int i = 0; i < layerSurfaces.Length; i++)
        {
            TerrainLayer layer = i < data.terrainLayers.Length ? data.terrainLayers[i] : null;
            int mapped = terrainLayers.FindIndex(m => m.layer == layer);
            layerListed[i] = mapped >= 0;
            layerSurfaces[i] = mapped >= 0 ? terrainLayers[mapped].surface : surface;
        }
        return layerSurfaces.Length > 0;
    }
}
