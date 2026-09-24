using System;
using UnityEngine;

[Serializable]
public struct GroundSnowSaveData
{
    public float amount;
}

// Snow lying on the ground (Weather_System.md: Visual Feedback). Builds while it snows below freezing, lingers while
// it stays below freezing, and melts once it warms up — including a mid-Winter thaw. Less settles under tree canopy
// and none under a roof, the same cover the falling snow respects (OverheadCover). Presentation only: none of Snow's
// gameplay effects read this.
//
// Painted as an extra terrain layer on a runtime copy of the terrain data, so the terrain asset is never modified.
// Repainting is spread over several frames to avoid a hitch. Footsteps keep the surface underneath the snow.
public class GroundSnow : MonoBehaviour, ISaveable
{
    [Tooltip("Share of full cover gained per in-game hour of snowfall below freezing.")]
    [SerializeField, Min(0f)] float buildPerHour = 0.15f;
    [Tooltip("Share lost per in-game hour just above freezing.")]
    [SerializeField, Min(0f)] float meltPerHour = 0.02f;
    [Tooltip("Extra share lost per in-game hour for each °C above freezing.")]
    [SerializeField, Min(0f)] float meltPerHourPerDegreeC = 0.01f;
    [Tooltip("How much a full tree canopy keeps off the ground, 0–1.")]
    [SerializeField, Range(0f, 1f)] float canopyShelter = 0.7f;
    [Tooltip("Repaint once the amount has changed by this much.")]
    [SerializeField, Range(0.005f, 0.2f)] float repaintStep = 0.02f;
    [SerializeField, Min(0.1f)] float snowTileSize = 3f;

    const int RowsPerFrame = 64;
    const int CoverResolution = 256;

    Terrain terrain;
    TerrainData data;
    float[,,] baseAlphas; // the terrain's own painting, without snow
    float[] coverMask;    // 0–1 per cover cell: how much snow can settle there
    float[,,] slice;
    TerrainLayer snowLayerAsset;
    int res, baseLayers, snowLayer;

    float amount, painted = -1f;
    int paintRow = -1;
    float paintAmount;
    float lastHours = -1f;
    bool restored;

    public float Amount => amount;

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

        // Copy first: everything below writes to the copy.
        data = Instantiate(terrain.terrainData);
        data.name = terrain.terrainData.name + " (runtime)";
        terrain.terrainData = data;
        if (terrain.TryGetComponent(out TerrainCollider terrainCollider))
            terrainCollider.terrainData = data;

        res = data.alphamapResolution;
        baseLayers = data.alphamapLayers;
        baseAlphas = data.GetAlphamaps(0, 0, res, res);

        TerrainLayer[] layers = data.terrainLayers;
        var withSnow = new TerrainLayer[layers.Length + 1];
        layers.CopyTo(withSnow, 0);
        withSnow[layers.Length] = snowLayerAsset = SnowLayer();
        data.terrainLayers = withSnow;
        snowLayer = layers.Length;
        slice = new float[RowsPerFrame, res, snowLayer + 1];

        BuildCoverMask();
        if (!restored)
            amount = 0f;
        Repaint(immediate: true);
    }

    void OnDestroy()
    {
        if (snowLayerAsset != null)
        {
            Destroy(snowLayerAsset.diffuseTexture);
            Destroy(snowLayerAsset);
        }
        if (data != null)
            Destroy(data);
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
            else if (weather.Current == WeatherType.Snow)
                amount += buildPerHour * elapsed;
            amount = Mathf.Clamp01(amount);
        }

        if (paintRow < 0 && (Mathf.Abs(amount - painted) >= repaintStep || (amount == 0f && painted > 0f) ||
                             (amount == 1f && painted < 1f)))
            Repaint(immediate: false);

        if (paintRow >= 0)
            PaintRows();
    }

    void Repaint(bool immediate)
    {
        paintAmount = amount;
        paintRow = 0;
        if (immediate)
        {
            while (paintRow >= 0)
                PaintRows();
        }
    }

    void PaintRows()
    {
        int rows = Mathf.Min(RowsPerFrame, res - paintRow);
        for (int z = 0; z < rows; z++)
        {
            int row = paintRow + z;
            int coverZ = row * CoverResolution / res;
            for (int x = 0; x < res; x++)
            {
                float snow = paintAmount * coverMask[coverZ * CoverResolution + x * CoverResolution / res];
                for (int layer = 0; layer < baseLayers; layer++)
                    slice[z, x, layer] = baseAlphas[row, x, layer] * (1f - snow);
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
            painted = paintAmount;
        }
    }

    // Where snow can settle: everywhere in the open, less under canopy, nowhere under a roof.
    void BuildCoverMask()
    {
        coverMask = new float[CoverResolution * CoverResolution];
        Vector3 origin = terrain.transform.position;
        Vector3 size = data.size;
        for (int z = 0; z < CoverResolution; z++)
        for (int x = 0; x < CoverResolution; x++)
        {
            var point = new Vector3(origin.x + (x + 0.5f) / CoverResolution * size.x, 0f,
                                    origin.z + (z + 0.5f) / CoverResolution * size.z);
            point.y = terrain.SampleHeight(point) + origin.y + 0.5f;
            coverMask[z * CoverResolution + x] = 1f - OverheadCover.At(point, canopyShelter);
        }
    }

    TerrainLayer SnowLayer()
    {
        const int Size = 128;
        var texture = new Texture2D(Size, Size, TextureFormat.RGB24, true) { wrapMode = TextureWrapMode.Repeat };
        var random = new System.Random(1851);
        var pixels = new Color[Size * Size];
        for (int i = 0; i < pixels.Length; i++)
        {
            float speck = (float)random.NextDouble();
            float shade = 0.86f + 0.1f * speck;
            pixels[i] = new Color(shade, shade + 0.01f, shade + 0.04f);
        }
        texture.SetPixels(pixels);
        texture.Apply(true);

        return new TerrainLayer
        {
            name = "Snow (runtime)",
            diffuseTexture = texture,
            tileSize = new Vector2(snowTileSize, snowTileSize),
            smoothness = 0.35f,
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
