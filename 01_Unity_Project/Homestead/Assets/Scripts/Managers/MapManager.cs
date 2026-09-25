using System;
using System.IO;
using System.IO.Compression;
using UnityEngine;

[Serializable]
public struct MapSaveData
{
    public int resolution;
    // Explored mask, one byte per cell (0 = unexplored, 255 = fully revealed), gzipped then base64'd.
    public string explored;
}

// Discovery_System.md's Minimap Fog of War (confirmed 2026-09-25): the map starts black and the ground the player
// physically walks is revealed permanently. This manager owns the explored mask so it persists across scenes and
// saves; MinimapHud draws it. Discovery markers are a separate layer that comes straight from DiscoveryManager.
public class MapManager : MonoBehaviour, ISaveable
{
    public static MapManager Instance { get; private set; }

    [Header("Area (world XZ)")]
    [Tooltip("South-west corner of the mapped area. Defaults to the property terrain (PropertyTerrainBuilder).")]
    [SerializeField] Vector2 worldMin = new Vector2(-240f, -240f);
    [SerializeField, Min(1f)] float worldSize = 480f;
    [Tooltip("Cells per side. 240 over 480 m is 2 m per cell.")]
    [SerializeField, Range(64, 1024)] int resolution = 240;

    [Header("Reveal")]
    [Tooltip("Ground within this distance of the player is fully revealed (metres). A first proposal: roughly how far " +
             "you can take in the land around you through the woods.")]
    [SerializeField, Min(1f)] float revealRadius = 30f;
    [Tooltip("Soft edge beyond the reveal radius, so explored ground fades out rather than ending in a hard ring.")]
    [SerializeField, Min(0f)] float revealFalloff = 10f;
    [Tooltip("The player has to move this far before the next reveal pass (metres).")]
    [SerializeField, Min(0.1f)] float revealStep = 2f;

    byte[] explored;
    byte[] fogPixels;
    Texture2D fogTexture;
    bool textureDirty = true;
    PlayerController player;
    Vector3 lastReveal = new Vector3(float.NaN, 0f, float.NaN);
    float nextPlayerSearch;

    public Vector2 WorldMin => worldMin;
    public float WorldSize => worldSize;

    // Alpha = how UNexplored each cell is, so it can be laid over the map as black fog with the stock UI shader.
    // Bilinear, so revealed ground has soft edges on the minimap.
    public Texture2D FogTexture
    {
        get
        {
            if (fogTexture == null)
            {
                fogTexture = new Texture2D(resolution, resolution, TextureFormat.Alpha8, false)
                {
                    name = "Minimap Fog",
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = FilterMode.Bilinear,
                };
                textureDirty = true;
            }

            if (textureDirty)
            {
                if (fogPixels == null || fogPixels.Length != explored.Length)
                    fogPixels = new byte[explored.Length];
                for (int i = 0; i < explored.Length; i++)
                    fogPixels[i] = (byte)(255 - explored[i]);
                fogTexture.SetPixelData(fogPixels, 0);
                fogTexture.Apply(false);
                textureDirty = false;
            }

            return fogTexture;
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        explored = new byte[resolution * resolution];
    }

    void Start()
    {
        if (Instance == this && SaveManager.Instance != null)
            SaveManager.Instance.Register(this);
    }

    void OnDestroy()
    {
        if (Instance != this)
            return;

        if (SaveManager.Instance != null)
            SaveManager.Instance.Unregister(this);
        if (fogTexture != null)
            Destroy(fogTexture);
        Instance = null;
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing)
            return;

        if (player == null)
        {
            if (Time.unscaledTime < nextPlayerSearch)
                return;
            nextPlayerSearch = Time.unscaledTime + 1f;
            player = FindAnyObjectByType<PlayerController>();
            if (player == null)
                return;
        }

        Vector3 position = player.transform.position;
        float dx = position.x - lastReveal.x, dz = position.z - lastReveal.z;
        if (float.IsNaN(lastReveal.x) || dx * dx + dz * dz >= revealStep * revealStep)
        {
            Reveal(position, revealRadius, revealFalloff);
            lastReveal = position;
        }
    }

    // World position → 0-1 across the mapped area.
    public Vector2 ToNormalized(Vector3 world) =>
        new Vector2((world.x - worldMin.x) / worldSize, (world.z - worldMin.y) / worldSize);

    // Reveals a soft-edged circle. Cells only ever brighten, so revealed ground never re-fogs.
    public void Reveal(Vector3 center, float radius, float falloff)
    {
        float cellSize = worldSize / resolution;
        float outer = radius + falloff;
        Vector2 c = ToNormalized(center) * resolution;
        int r = Mathf.CeilToInt(outer / cellSize);
        int x0 = Mathf.Max(0, Mathf.FloorToInt(c.x) - r), x1 = Mathf.Min(resolution - 1, Mathf.FloorToInt(c.x) + r);
        int y0 = Mathf.Max(0, Mathf.FloorToInt(c.y) - r), y1 = Mathf.Min(resolution - 1, Mathf.FloorToInt(c.y) + r);

        bool changed = false;
        for (int y = y0; y <= y1; y++)
        {
            for (int x = x0; x <= x1; x++)
            {
                float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), c) * cellSize;
                if (distance >= outer)
                    continue;

                float t = falloff > 0f ? Mathf.Clamp01((outer - distance) / falloff) : 1f;
                byte value = (byte)Mathf.RoundToInt(Mathf.SmoothStep(0f, 1f, t) * 255f);
                int i = y * resolution + x;
                if (value > explored[i])
                {
                    explored[i] = value;
                    changed = true;
                }
            }
        }

        if (changed)
            textureDirty = true;
    }

    // 0-1 share of the mapped area that's been explored.
    public float ExploredFraction
    {
        get
        {
            long sum = 0;
            foreach (byte b in explored)
                sum += b;
            return sum / (255f * explored.Length);
        }
    }

    // Everything black again — used when starting a new game.
    public void ResetMap()
    {
        Array.Clear(explored, 0, explored.Length);
        textureDirty = true;
        lastReveal = new Vector3(float.NaN, 0f, float.NaN);
    }

    public MapSaveData CaptureState() => new MapSaveData { resolution = resolution, explored = Pack(explored) };

    public void RestoreState(MapSaveData data)
    {
        byte[] loaded = data.resolution == resolution ? Unpack(data.explored, explored.Length) : null;
        if (loaded != null)
            explored = loaded;
        else
            Array.Clear(explored, 0, explored.Length);

        textureDirty = true;
        lastReveal = new Vector3(float.NaN, 0f, float.NaN);
    }

    static string Pack(byte[] bytes)
    {
        using (var output = new MemoryStream())
        {
            using (var gzip = new GZipStream(output, System.IO.Compression.CompressionLevel.Optimal))
                gzip.Write(bytes, 0, bytes.Length);
            return Convert.ToBase64String(output.ToArray());
        }
    }

    static byte[] Unpack(string packed, int length)
    {
        if (string.IsNullOrEmpty(packed))
            return null;

        try
        {
            var bytes = new byte[length];
            using (var gzip = new GZipStream(new MemoryStream(Convert.FromBase64String(packed)), CompressionMode.Decompress))
            {
                int read = 0, n;
                while (read < length && (n = gzip.Read(bytes, read, length - read)) > 0)
                    read += n;
                return read == length ? bytes : null;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[MapManager] Couldn't read the explored map from the save: {e.Message}");
            return null;
        }
    }

    // Save_Data_Model.md's Discovery Block: minimap reveal state.
    string ISaveable.SaveFile => "discovery";
    string ISaveable.SaveKey => "map";
    object ISaveable.CaptureState() => CaptureState();
    void ISaveable.RestoreState(string json) => RestoreState(JsonUtility.FromJson<MapSaveData>(json));
}
