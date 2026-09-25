using UnityEngine;

// A play-time copy of a terrain's data, so seasonal changes (SeasonalGround's snow and leaf litter, SeasonalTrees'
// leaves) can repaint and restyle the terrain without ever modifying the TerrainData asset. The first caller makes
// the copy; everyone after gets the same one. The copy is destroyed with the terrain.
public class RuntimeTerrain : MonoBehaviour
{
    TerrainData copy;

    public static TerrainData Data(Terrain terrain)
    {
        if (terrain.TryGetComponent(out RuntimeTerrain existing))
            return existing.copy;

        var owner = terrain.gameObject.AddComponent<RuntimeTerrain>();
        owner.hideFlags = HideFlags.HideAndDontSave;
        owner.copy = Instantiate(terrain.terrainData);
        owner.copy.name = terrain.terrainData.name + " (runtime)";
        terrain.terrainData = owner.copy;
        if (terrain.TryGetComponent(out TerrainCollider terrainCollider))
            terrainCollider.terrainData = owner.copy;
        return owner.copy;
    }

    void OnDestroy()
    {
        if (copy != null)
            Destroy(copy);
    }
}
