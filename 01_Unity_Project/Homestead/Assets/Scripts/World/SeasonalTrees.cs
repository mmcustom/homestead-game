using System.Collections.Generic;
using UnityEngine;

// The hardwood forest through the year (Season_System.md: Visual Feedback) and snow on the trees
// (Weather_System.md: Visual Feedback). Leaves turn from green to their Fall colour across Fall, drop over its second
// half, leave the trees bare through Winter, and come back fresh and pale over early Spring. Snow settles on the
// crowns and branches on the same terms as SeasonalGround's ground snow. Presentation only.
//
// How: the tree meshes (PropertyTerrainBuilder) give every leaf face and snow cap its own random value in the
// material's alpha. Raising the alpha cutoff hides faces one at a time, so crowns thin out and snow patches appear
// gradually rather than all at once. The terrain's tree prototypes are swapped for play-time copies with their own
// material instances (on RuntimeTerrain's copy of the terrain data), so no prefab or material asset is modified.
// Material slots on the tree prefabs: 0 bark, 1 leaves, 2 snow on the crown, 3 snow on the branches.
public class SeasonalTrees : MonoBehaviour
{
    [Tooltip("Season progress through Fall over which leaves change colour.")]
    [SerializeField] Vector2 colourChangeInFall = new Vector2(0f, 0.55f);
    [Tooltip("Season progress through Fall over which leaves drop (matches SeasonalGround's leaf litter).")]
    [SerializeField] Vector2 leafDropInFall = new Vector2(0.45f, 1f);
    [Tooltip("Season progress through Spring over which leaves grow back.")]
    [SerializeField] Vector2 leafOutInSpring = new Vector2(0f, 0.45f);
    [Tooltip("Late-Fall leaves still on the tree fade toward brown by this much.")]
    [SerializeField, Range(0f, 1f)] float lateFallBrowning = 0.4f;
    [SerializeField] Color brownLeaf = new Color(0.3f, 0.18f, 0.08f);

    const float UpdateInterval = 0.5f;

    sealed class Prototype
    {
        public SeasonalFoliage foliage;
        public Material leaves, crownSnow, branchSnow;
    }

    readonly List<Prototype> prototypes = new List<Prototype>();
    readonly List<Object> created = new List<Object>();
    SeasonalGround ground;
    float nextUpdate;

    void Start()
    {
        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null || terrain.terrainData == null)
        {
            enabled = false;
            return;
        }

        TerrainData data = RuntimeTerrain.Data(terrain);
        TreePrototype[] treePrototypes = data.treePrototypes;
        for (int i = 0; i < treePrototypes.Length; i++)
        {
            GameObject prefab = treePrototypes[i].prefab;
            if (prefab == null || !prefab.TryGetComponent(out SeasonalFoliage foliage) ||
                !prefab.TryGetComponent(out MeshRenderer prefabRenderer) || prefabRenderer.sharedMaterials.Length < 4)
                continue;

            // A hidden copy far below the world (the terrain only reads its mesh, materials and collider).
            GameObject copy = Instantiate(prefab, new Vector3(0f, -10000f, 0f), Quaternion.identity);
            copy.name = prefab.name + " (runtime)";
            copy.hideFlags = HideFlags.HideAndDontSave;
            created.Add(copy);

            var renderer = copy.GetComponent<MeshRenderer>();
            Material[] materials = renderer.sharedMaterials;
            for (int m = 0; m < materials.Length; m++)
            {
                materials[m] = new Material(materials[m]);
                created.Add(materials[m]);
            }
            renderer.sharedMaterials = materials;

            prototypes.Add(new Prototype
            {
                foliage = copy.GetComponent<SeasonalFoliage>(),
                leaves = materials[1],
                crownSnow = materials[2],
                branchSnow = materials[3],
            });
            treePrototypes[i] = new TreePrototype
            {
                prefab = copy,
                bendFactor = treePrototypes[i].bendFactor,
                navMeshLod = treePrototypes[i].navMeshLod,
            };
        }
        data.treePrototypes = treePrototypes;

        ground = FindAnyObjectByType<SeasonalGround>();
        Apply();
    }

    void OnDestroy()
    {
        foreach (Object item in created)
        {
            if (item != null)
                Destroy(item);
        }
    }

    void Update()
    {
        if (Time.unscaledTime < nextUpdate)
            return;
        nextUpdate = Time.unscaledTime + UpdateInterval;
        Apply();
    }

    void Apply()
    {
        TimeManager time = TimeManager.Instance;
        Season season = time != null ? time.CurrentSeason : Season.Summer;
        float progress = time != null ? time.SeasonProgress : 0.5f;
        float snow = ground != null ? ground.Amount : 0f;

        foreach (Prototype prototype in prototypes)
        {
            SeasonalFoliage foliage = prototype.foliage;
            float leaves = foliage.deciduous ? LeavesFor(season, progress) : 1f;
            Color colour = ColourFor(foliage, season, progress);

            // Alpha values run 0.02–0.98, so a cutoff of 0 shows every face and 1 hides them all.
            float leafCutoff = 1f - leaves;
            // Crown snow only sits on leaves that are still there, covering `snow` of them.
            float crownCutoff = leafCutoff + (1f - leafCutoff) * (1f - snow);

            prototype.leaves.SetColor("_BaseColor", colour);
            prototype.leaves.SetFloat("_AlphaCutoff", leafCutoff);
            prototype.crownSnow.SetFloat("_AlphaCutoff", leaves > 0f ? crownCutoff : 1f);
            prototype.branchSnow.SetFloat("_AlphaCutoff", 1f - snow);
        }
    }

    float LeavesFor(Season season, float progress)
    {
        switch (season)
        {
            case Season.Fall: return 1f - Smooth(leafDropInFall.x, leafDropInFall.y, progress);
            case Season.Winter: return 0f;
            case Season.Spring: return Smooth(leafOutInSpring.x, leafOutInSpring.y, progress);
            default: return 1f;
        }
    }

    Color ColourFor(SeasonalFoliage foliage, Season season, float progress)
    {
        switch (season)
        {
            case Season.Spring:
                return Color.Lerp(foliage.springColor, foliage.summerColor, Smooth(0.3f, 1f, progress));
            case Season.Fall:
                Color turning = Color.Lerp(foliage.summerColor, foliage.fallColor,
                                           Smooth(colourChangeInFall.x, colourChangeInFall.y, progress));
                return Color.Lerp(turning, brownLeaf, lateFallBrowning * Smooth(0.7f, 1f, progress));
            case Season.Winter:
                return brownLeaf;
            default:
                return foliage.summerColor;
        }
    }

    static float Smooth(float from, float to, float value) => Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(from, to, value));
}
