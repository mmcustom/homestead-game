using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Discovery_System.md's Minimap Integration: a round, north-up minimap in the top-right corner of the HUD, centred on
// the player. Two independent layers sit on the terrain image: fog of war (MapManager — black until the player has
// walked that ground) and Discovery markers, which appear only once their site has actually been found, even inside
// ground that's already revealed. A found site beyond the minimap's edge is pinned to the rim, pointing the way back.
public class MinimapHud : MonoBehaviour
{
    [SerializeField] RawImage mapImage;
    [SerializeField] RawImage fogImage;
    [SerializeField] RectTransform markerLayer;
    [SerializeField] RectTransform playerArrow;
    [Tooltip("The round mask the map is drawn inside.")]
    [SerializeField] Image mask;
    [SerializeField] Image backdrop;
    [SerializeField] Image rim;

    [Tooltip("Metres of ground shown across the minimap.")]
    [SerializeField, Min(10f)] float viewMeters = 160f;
    [Tooltip("Pixels per side of the generated terrain image.")]
    [SerializeField, Range(128, 2048)] int terrainImageSize = 512;
    [SerializeField, Min(8f)] float markerSize = 26f;

    readonly List<Image> markers = new List<Image>();
    PlayerController player;
    float nextPlayerSearch;
    Texture2D terrainImage;
    CanvasGroup group;
    int markerRecordCount = -1;

    void Awake()
    {
        group = GetComponent<CanvasGroup>();
        mask.sprite = MapIcons.Circle;
        backdrop.sprite = MapIcons.Circle;
        rim.sprite = MapIcons.Ring;
        playerArrow.GetComponent<Image>().sprite = MapIcons.Arrow;
    }

    void Start()
    {
        MapManager map = MapManager.Instance;
        if (map == null)
            return;

        terrainImage = MinimapTerrain.Get(Terrain.activeTerrain, map.WorldMin, map.WorldSize, terrainImageSize);
        mapImage.texture = terrainImage;
    }

    void LateUpdate()
    {
        MapManager map = MapManager.Instance;
        if (player == null && Time.unscaledTime >= nextPlayerSearch)
        {
            nextPlayerSearch = Time.unscaledTime + 1f;
            player = FindAnyObjectByType<PlayerController>();
        }

        bool visible = map != null && player != null && terrainImage != null;
        if (group != null)
            group.alpha = visible ? 1f : 0f;
        if (!visible)
            return;

        Vector3 position = player.transform.position;
        Vector2 center = map.ToNormalized(position);
        float span = viewMeters / map.WorldSize;
        var uv = new Rect(center.x - span / 2f, center.y - span / 2f, span, span);
        mapImage.uvRect = uv;
        fogImage.texture = map.FogTexture;
        fogImage.uvRect = uv;

        playerArrow.localEulerAngles = new Vector3(0f, 0f, -player.transform.eulerAngles.y);
        UpdateMarkers(position);
    }

    void UpdateMarkers(Vector3 playerPosition)
    {
        DiscoveryManager discovery = DiscoveryManager.Instance;
        IReadOnlyList<DiscoveryRecord> records = discovery != null ? discovery.Records : null;
        int count = records != null ? records.Count : 0;

        // A discovery or a loaded save changes the set of markers; rebuild only then.
        if (count != markerRecordCount)
        {
            markerRecordCount = count;
            for (int i = 0; i < count; i++)
            {
                if (i == markers.Count)
                    markers.Add(CreateMarker());
                markers[i].sprite = MapIcons.Marker(records[i].category);
                markers[i].gameObject.name = records[i].displayName;
            }
            for (int i = 0; i < markers.Count; i++)
                markers[i].gameObject.SetActive(i < count);
        }

        float radius = markerLayer.rect.width / 2f;
        float pixelsPerMetre = markerLayer.rect.width / viewMeters;
        float rimRadius = radius - markerSize * 0.5f;
        for (int i = 0; i < count; i++)
        {
            Vector3 site = records[i].position;
            var offset = new Vector2(site.x - playerPosition.x, site.z - playerPosition.z) * pixelsPerMetre;
            bool beyond = offset.magnitude > rimRadius;
            if (beyond)
                offset = offset.normalized * rimRadius;

            RectTransform rt = markers[i].rectTransform;
            rt.anchoredPosition = offset;
            rt.localScale = Vector3.one * (beyond ? 0.75f : 1f);
            markers[i].color = new Color(1f, 1f, 1f, beyond ? 0.7f : 1f);
        }
    }

    Image CreateMarker()
    {
        var go = new GameObject("Marker", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(markerLayer, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(markerSize, markerSize);
        var image = go.GetComponent<Image>();
        image.raycastTarget = false;
        return image;
    }
}
