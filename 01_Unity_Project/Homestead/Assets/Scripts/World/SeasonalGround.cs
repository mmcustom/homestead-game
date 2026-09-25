using System;
using UnityEngine;

[Serializable]
public struct GroundSnowSaveData
{
    public float amount;
}

// Seasonal ground cover painted onto the terrain (Weather_System.md and Season_System.md: Visual Feedback).
//
// Snow builds while it snows below freezing, lingers while it stays below freezing, and melts once it warms up —
// including a mid-Winter thaw. Less settles under tree canopy and none under a roof, the same cover the falling
// snow respects (OverheadCover). The amount is saved, since it depends on weather history.
//
// Leaf litter follows the calendar: it builds under the trees as their leaves drop over the second half of Fall,
// stays through Winter (under any snow), and breaks down over early Spring. Derived from the date, so not saved.
//
// Presentation only: no gameplay effect reads either. Painted as extra layers on RuntimeTerrain's copy of the
// terrain data, so the asset is never modified, and repainted over several frames to avoid a hitch. Footsteps keep
// the surface underneath (GroundSurface ignores these layers).
public class SeasonalGround : MonoBehaviour, ISaveable
{
    [Header("Snow")]
    [Tooltip("Share of full cover gained per in-game hour of snowfall below freezing.")]
    [SerializeField, Min(0f)] float buildPerHour = 0.15f;
    [Tooltip("Share lost per in-game hour just above freezing.")]
    [SerializeField, Min(0f)] float meltPerHour = 0.02f;
    [Tooltip("Extra share lost per in-game hour for each °C above freezing.")]
    [SerializeField, Min(0f)] float meltPerHourPerDegreeC = 0.01f;
    [Tooltip("How much a full tree canopy keeps off the ground, 0–1.")]
    [SerializeField, Range(0f, 1f)] float canopyShelter = 0.7f;
    [SerializeField, Min(0.1f)] float snowTileSize = 3f;

    [Header("Leaf litter")]
    [Tooltip("Season progress through Fall over which litter builds (matches SeasonalTrees' leaf drop).")]
    [SerializeField] Vector2 litterBuildsInFall = new Vector2(0.45f, 1f);
    [Tooltip("Season progress through Spring over which litter breaks down.")]
    [SerializeField] Vector2 litterFadesInSpring = new Vector2(0f, 0.5f);
    [SerializeField, Min(0.1f)] float litterTileSize = 2.5f;

    [Tooltip("Repaint once either amount has changed by this much.")]
    [SerializeField, Range(0.005f, 0.2f)] float repaintStep = 0.02f;

    const int RowsPerFrame = 32; // ~5ms of repainting per frame
    const int CoverResolution = 256;

    Terrain terrain;
    TerrainData data;
    float[,,] baseAlphas; // the terrain's own painting
    float[] snowMask;     // 0–1 per cover cell: how much snow can settle there
    float[] canopyMask;   // 0–1 per cover cell: how much of it sits under tree crowns
    float[,,] slice;
    TerrainLayer litterLayerAsset, snowLayerAsset;
    int res, baseLayers, litterLayer, snowLayer;

    float amount, litter;
    float paintedSnow = -1f, paintedLitter = -1f;
    int paintRow = -1;
    float paintSnow, paintLitter;
    float lastHours = -1f;
    bool restored;

    public float Amount => amount;
    public float Litter => litter;

    void OnEnable()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.Register(this);
    }

    void OnDisable()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.Unregister(this);
    }

    void Start()
    {
        terrain = Terrain.activeTerrain;
        if (terrain == null || terrain.terrainData == null)
        {
            enabled = false;
            return;
        }

        data = RuntimeTerrain.Data(terrain);
        res = data.alphamapResolution;
        baseLayers = data.alphamapLayers;
        baseAlphas = data.GetAlphamaps(0, 0, res, res);

        TerrainLayer[] layers = data.terrainLayers;
        var withSeasonal = new TerrainLayer[layers.Length + 2];
        layers.CopyTo(withSeasonal, 0);
        litterLayer = layers.Length;
        snowLayer = layers.Length + 1;
        withSeasonal[litterLayer] = litterLayerAsset = MakeLayer("Leaf Litter (runtime)", LitterColor, litterTileSize, 0.1f);
        withSeasonal[snowLayer] = snowLayerAsset = MakeLayer("Snow (runtime)", SnowColor, snowTileSize, 0.35f);
        data.terrainLayers = withSeasonal;
        slice = new float[RowsPerFrame, res, snowLayer + 1];

        BuildMasks();
        if (!restored)
            amount = 0f;
        litter = LitterForDate();
        Repaint(immediate: true);
    }

    void OnDestroy()
    {
        foreach (TerrainLayer layer in new[] { litterLayerAsset, snowLayerAsset })
        {
            if (layer == null)
                continue;
            Destroy(layer.diffuseTexture);
            Destroy(layer);
        }
    }

    void Update()
    {
        if (data == null)
            return;

        TimeManager time = TimeManager.Instance;
        WeatherManager weather = WeatherManager.Instance;
        if (time != null && weather != null)
        {
            float hours = time.TotalDays * 24f + time.HourOfDay;
            float elapsed = lastHours < 0f ? 0f : Mathf.Max(0f, hours - lastHours);
            lastHours = hours;

            float temperature = weather.TemperatureC;
            if (temperature > 0f)
                amount -= (meltPerHour + meltPerHourPerDegreeC * temperature) * elapsed;
            else if (weather.IsSnowing)
                amount += buildPerHour * elapsed;
            amount = Mathf.Clamp01(amount);
        }
        litter = LitterForDate();

        if (paintRow < 0 && (Changed(amount, paintedSnow) || Changed(litter, paintedLitter)))
            Repaint(immediate: false);

        if (paintRow >= 0)
            PaintRows();
    }

    bool Changed(float now, float painted) =>
        Mathf.Abs(now - painted) >= repaintStep || (now == 0f && painted > 0f) || (now == 1f && painted < 1f);

    float LitterForDate()
    {
        TimeManager time = TimeManager.Instance;
        if (time == null)
            return 0f;
        float progress = time.SeasonProgress;
        switch (time.CurrentSeason)
        {
            case Season.Fall: return Smooth(litterBuildsInFall.x, litterBuildsInFall.y, progress);
            case Season.Winter: return 1f;
            case Season.Spring: return 1f - Smooth(litterFadesInSpring.x, litterFadesInSpring.y, progress);
            default: return 0f;
        }
    }

    static float Smooth(float from, float to, float value) => Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(from, to, value));

    void Repaint(bool immediate)
    {
        paintSnow = amount;
        paintLitter = litter;
        paintRow = 0;
        if (immediate)
        {
            while (paintRow >= 0)
                PaintRows();
        }
    }

    // Snow lies over litter, which lies over the terrain's own painting; the weights always sum to 1.
    void PaintRows()
    {
        int rows = Mathf.Min(RowsPerFrame, res - paintRow);
        for (int z = 0; z < rows; z++)
        {
            int row = paintRow + z;
            int coverRow = row * CoverResolution / res * CoverResolution;
            for (int x = 0; x < res; x++)
            {
                int cell = coverRow + x * CoverResolution / res;
                float snow = paintSnow * snowMask[cell];
                float leaves = paintLitter * canopyMask[cell];
                float keep = (1f - snow) * (1f - leaves);
                for (int layer = 0; layer < baseLayers; layer++)
                    slice[z, x, layer] = baseAlphas[row, x, layer] * keep;
                slice[z, x, litterLayer] = leaves * (1f - snow);
                slice[z, x, snowLayer] = snow;
            }
        }

        if (rows == RowsPerFrame)
        {
            data.SetAlphamaps(0, paintRow, slice);
        }
        else
        {
            var last = new float[rows, res, snowLayer + 1];
            Array.Copy(slice, last, last.Length);
            data.SetAlphamaps(0, paintRow, last);
        }

        paintRow += rows;
        if (paintRow >= res)
        {
            paintRow = -1;
            paintedSnow = paintSnow;
            paintedLitter = paintLitter;
        }
    }

    // Where snow can settle (open ground fully, under canopy partly, under a roof not at all) and where leaves fall.
    void BuildMasks()
    {
        snowMask = new float[CoverResolution * CoverResolution];
        canopyMask = new float[CoverResolution * CoverResolution];
        Vector3 origin = terrain.transform.position;
        Vector3 size = data.size;
        for (int z = 0; z < CoverResolution; z++)
        for (int x = 0; x < CoverResolution; x++)
        {
            var point = new Vector3(origin.x + (x + 0.5f) / CoverResolution * size.x, 0f,
                                    origin.z + (z + 0.5f) / CoverResolution * size.z);
            point.y = terrain.SampleHeight(point) + origin.y + 0.5f;
            float canopy = OverheadCover.CanopyFraction(point);
            int cell = z * CoverResolution + x;
            canopyMask[cell] = canopy;
            snowMask[cell] = OverheadCover.Roofed(point) ? 0f : 1f - canopyShelter * canopy;
        }
    }

    static Color SnowColor(System.Random random)
    {
        float shade = 0.86f + 0.1f * (float)random.NextDouble();
        return new Color(shade, shade + 0.01f, shade + 0.04f);
    }

    // Brown leaves with scattered orange and yellow.
    static Color LitterColor(System.Random random)
    {
        float pick = (float)random.NextDouble();
        Color color = pick < 0.6f ? new Color(0.33f, 0.2f, 0.1f)
                    : pick < 0.85f ? new Color(0.5f, 0.26f, 0.08f)
                    : new Color(0.56f, 0.42f, 0.12f);
        return color * (0.75f + 0.35f * (float)random.NextDouble());
    }

    static TerrainLayer MakeLayer(string layerName, Func<System.Random, Color> paint, float tileSize, float smoothness)
    {
        const int Size = 128;
        var texture = new Texture2D(Size, Size, TextureFormat.RGB24, true) { wrapMode = TextureWrapMode.Repeat };
        var random = new System.Random(1851);
        var pixels = new Color[Size * Size];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = paint(random);
        texture.SetPixels(pixels);
        texture.Apply(true);

        return new TerrainLayer
        {
            name = layerName,
            diffuseTexture = texture,
            tileSize = new Vector2(tileSize, tileSize),
            smoothness = smoothness,
        };
    }

    string ISaveable.SaveFile => "world";
    string ISaveable.SaveKey => "groundSnow";
    object ISaveable.CaptureState() => new GroundSnowSaveData { amount = amount };

    void ISaveable.RestoreState(string json)
    {
        amount = Mathf.Clamp01(JsonUtility.FromJson<GroundSnowSaveData>(json).amount);
        restored = true;
        if (data != null)
            Repaint(immediate: true);
    }
}
