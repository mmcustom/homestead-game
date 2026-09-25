using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Discovery_System.md's World Map Integration (step 4 of the Discovery Process): the whole property at once, north up.
// It shows exactly what the minimap already knows — the same MapManager fog of war and the same DiscoveryManager
// markers, just zoomed out to fit — so it earns no knowledge of its own. Markers are labelled with the site's name,
// and the player's position and facing are shown. A list beside the map names every discovered site.
public class WorldMapScreen : GameScreen
{
    const int TerrainImageSize = 512; // matches MinimapHud, so both share one generated image
    const float MarkerSize = 30f;

    RectTransform mapArea;
    RawImage terrainImage;
    RawImage fogImage;
    RectTransform markerLayer;
    RectTransform playerArrow;
    Image playerHalo;
    RectTransform siteList;
    Text exploredLabel;
    readonly List<GameObject> markers = new List<GameObject>();
    PlayerController player;

    public override string Title => "Property Map";

    public override void Build(RectTransform area)
    {
        // Square map on the left, as tall as the area allows.
        mapArea = UiKit.Rect("Map", area);
        mapArea.anchorMin = new Vector2(0f, 0f);
        mapArea.anchorMax = new Vector2(0f, 1f);
        mapArea.pivot = new Vector2(0f, 0.5f);
        mapArea.sizeDelta = new Vector2(740f, 0f);
        mapArea.anchoredPosition = Vector2.zero; // the area is 740 tall, so this is square

        UiKit.Image(mapArea, "Frame", new Color(UiKit.Cream.r, UiKit.Cream.g, UiKit.Cream.b, 0.35f)).rectTransform.Fill(-3f, -3f, -3f, -3f);
        UiKit.Image(mapArea, "Unexplored", Color.black).rectTransform.Fill();

        terrainImage = UiKit.Rect("Terrain", mapArea).Fill().gameObject.AddComponent<RawImage>();
        terrainImage.raycastTarget = false;
        fogImage = UiKit.Rect("Fog", mapArea).Fill().gameObject.AddComponent<RawImage>();
        fogImage.color = Color.black;
        fogImage.raycastTarget = false;
        markerLayer = UiKit.Rect("Markers", mapArea).Fill();

        // The player: a larger arrow than the minimap's, over a pulsing halo so it stands out even on a marker.
        playerArrow = UiKit.Rect("Player", mapArea);
        playerArrow.anchorMin = playerArrow.anchorMax = Vector2.zero;
        playerArrow.sizeDelta = new Vector2(52f, 52f);
        playerHalo = UiKit.Image(playerArrow, "Halo", Color.white);
        playerHalo.sprite = MapIcons.Circle;
        playerHalo.rectTransform.Fill();
        Image pointer = UiKit.Image(playerArrow, "Arrow", Color.white);
        pointer.sprite = MapIcons.Arrow;
        pointer.rectTransform.Fill(10f, 10f, 10f, 10f);

        Text north = UiKit.Text(mapArea, "North", "N", 22, UiKit.Cream, TextAnchor.UpperRight, FontStyle.Bold);
        north.rectTransform.Fill(0f, 0f, 12f, 8f);

        // Legend on the right.
        RectTransform side = UiKit.Rect("Sites", area);
        side.anchorMin = new Vector2(0f, 0f);
        side.anchorMax = new Vector2(1f, 1f);
        side.offsetMin = new Vector2(780f, 0f);
        side.offsetMax = Vector2.zero;

        Text heading = UiKit.Text(side, "Heading", "Discovered", 26, UiKit.Accent, TextAnchor.MiddleLeft, FontStyle.Italic);
        heading.rectTransform.anchorMin = new Vector2(0f, 1f);
        heading.rectTransform.anchorMax = new Vector2(1f, 1f);
        heading.rectTransform.offsetMin = new Vector2(0f, -40f);
        heading.rectTransform.offsetMax = Vector2.zero;

        exploredLabel = UiKit.Text(side, "Explored", "", 20, UiKit.Muted);
        exploredLabel.rectTransform.anchorMin = new Vector2(0f, 0f);
        exploredLabel.rectTransform.anchorMax = new Vector2(1f, 0f);
        exploredLabel.rectTransform.offsetMin = Vector2.zero;
        exploredLabel.rectTransform.offsetMax = new Vector2(0f, 60f);

        siteList = UiKit.ScrollList(side, "List");
        siteList.parent.GetComponent<RectTransform>().Fill(0f, 70f, 0f, 52f);
    }

    public override void OnShow()
    {
        MapManager map = MapManager.Instance;
        if (map != null)
        {
            terrainImage.texture = MinimapTerrain.Get(Terrain.activeTerrain, map.WorldMin, map.WorldSize, TerrainImageSize);
            fogImage.texture = map.FogTexture;
            exploredLabel.text = $"You've walked {map.ExploredFraction * 100f:0}% of the property.\nOnly ground you've walked is shown.";
        }
        else
        {
            exploredLabel.text = "";
        }

        player = FindAnyObjectByType<PlayerController>();
        RebuildMarkers();
        LateUpdate();
    }

    void LateUpdate()
    {
        MapManager map = MapManager.Instance;
        bool show = map != null && player != null;
        playerArrow.gameObject.SetActive(show);
        if (!show)
            return;

        Vector2 size = mapArea.rect.size;
        playerArrow.anchoredPosition = Vector2.Scale(map.ToNormalized(player.transform.position), size);
        playerArrow.localEulerAngles = new Vector3(0f, 0f, -player.transform.eulerAngles.y);
        float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 4f); // unscaled: the game is paused behind the map
        playerHalo.color = new Color(0.93f, 0.89f, 0.78f, Mathf.Lerp(0.15f, 0.45f, pulse));
    }

    void RebuildMarkers()
    {
        foreach (GameObject marker in markers)
            Destroy(marker);
        markers.Clear();
        UiKit.Clear(siteList);

        MapManager map = MapManager.Instance;
        DiscoveryManager discovery = DiscoveryManager.Instance;
        if (discovery == null || discovery.Records.Count == 0)
        {
            UiKit.Height(UiKit.Text(siteList, "Empty", "Nothing found yet. Explore the property to fill in the map.",
                                    20, UiKit.Muted), 60f);
            return;
        }

        Vector2 size = mapArea.rect.size;
        foreach (DiscoveryRecord record in discovery.Records)
        {
            if (map != null)
                markers.Add(CreateMarker(record, Vector2.Scale(map.ToNormalized(record.position), size)));

            // Legend row: icon, name, category.
            RectTransform row = UiKit.Height(UiKit.Rect(record.displayName, siteList), 44f);
            Image icon = UiKit.Image(row, "Icon", Color.white);
            icon.sprite = MapIcons.Marker(record.category, record.markerIcon);
            icon.rectTransform.anchorMin = icon.rectTransform.anchorMax = new Vector2(0f, 0.5f);
            icon.rectTransform.pivot = new Vector2(0f, 0.5f);
            icon.rectTransform.sizeDelta = new Vector2(32f, 32f);
            icon.rectTransform.anchoredPosition = new Vector2(4f, 0f);
            Text name = UiKit.Text(row, "Name", $"{record.displayName}  <color=#EDE3C799>{DiscoveryCategoryNames.Of(record.category)}</color>",
                                   21, UiKit.Cream);
            name.rectTransform.Fill(48f, 0f, 0f, 0f);
        }
        playerArrow.SetAsLastSibling();
    }

    GameObject CreateMarker(DiscoveryRecord record, Vector2 position)
    {
        RectTransform marker = UiKit.Rect(record.displayName, markerLayer);
        marker.anchorMin = marker.anchorMax = Vector2.zero;
        marker.anchoredPosition = position;
        marker.sizeDelta = new Vector2(MarkerSize, MarkerSize);

        Image icon = UiKit.Image(marker, "Icon", Color.white);
        icon.sprite = MapIcons.Marker(record.category, record.markerIcon);
        icon.rectTransform.Fill();

        Text label = UiKit.Text(marker, "Label", record.displayName, 17, UiKit.Cream, TextAnchor.MiddleLeft, FontStyle.Bold);
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        label.rectTransform.anchorMin = label.rectTransform.anchorMax = new Vector2(1f, 0.5f);
        label.rectTransform.pivot = new Vector2(0f, 0.5f);
        label.rectTransform.sizeDelta = new Vector2(200f, 24f);
        label.rectTransform.anchoredPosition = new Vector2(4f, 0f);
        label.gameObject.AddComponent<Shadow>().effectColor = new Color(0f, 0f, 0f, 0.9f);
        return marker.gameObject;
    }
}
