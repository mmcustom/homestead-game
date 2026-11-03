using UnityEngine;

// A play-time copy of a terrain's data, so seasonal changes (SeasonalGround's snow and leaf litter, SeasonalTrees'
// leaves) can repaint and restyle the terrain without ever modifying the TerrainData asset. The first caller makes
// the copy; everyone after gets the same one. The copy is destroyed with the terrain.
//
// The creek banks (CreekBanks) are painted into the copy the moment it is made, before anyone else can store its painting,
// so every later reader (SeasonalGround, GroundSurface, the minimap) sees them as part of the terrain's own painting.
public class RuntimeTerrain : MonoBehaviour
{
    TerrainData copy;
    TerrainData source;

    // The terrain asset the copy was made from, untouched: the pristine painting a later rebuild of the banks would start from.
    public TerrainData Source => source;

    public static TerrainData Data(Terrain terrain)
    {
        if (terrain.TryGetComponent(out RuntimeTerrain existing))
            return existing.copy;

        var owner = terrain.gameObject.AddComponent<RuntimeTerrain>();
        owner.hideFlags = HideFlags.HideAndDontSave;
        owner.source = terrain.terrainData;
        owner.copy = Instantiate(terrain.terrainData);
        owner.copy.name = terrain.terrainData.name + " (runtime)";
        terrain.terrainData = owner.copy;
        if (terrain.TryGetComponent(out TerrainCollider terrainCollider))
            terrainCollider.terrainData = owner.copy;

        try
        {
            terrain.gameObject.AddComponent<CreekBanks>().Build(terrain, owner.copy);
        }
        catch (System.Exception problem)
        {
            Debug.LogError("[CreekBanks] the bank layers could not be built, the terrain keeps its own painting: " + problem);
        }
        return owner.copy;
    }

    void OnDestroy()
    {
        if (copy != null)
            Destroy(copy);
    }
}
