using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Maple D: two kinds of marker floating over Maples, so a tree can be spotted without standing at its trunk.
//   - A droplet over each untapped, standing Maple within 80 m, only while a Spile is equipped (the nearest 30; hidden at the 12-tap cap).
//   - A pail over each tapped Maple within 100 m, always (no Spile needed), coloured by what the bucket holds: cream-grey while empty or
//     filling, amber and pulsing once 4 L or more are ready, muted brown-red when sour. The minimap draws the same pail as a pin (MinimapHud).
// Maples are terrain tree instances, not objects, so the markers are two pools of UI images placed on screen from each tree's top
// (Camera.WorldToViewportPoint). Which Maples, the fades and the tap state are MapleMarkerRules (Homestead.Economy, tested). The nearest
// untapped Maples are chosen four times a second from a list of the map's Maples built once; each frame only the chosen ones are projected.
// A tapped tree is never in the untapped set, so a tree never has both markers. Nothing is saved. Markers are not hidden behind hills or
// other trees (there is no occlusion check).
//
// Self-installing, like TapManager: no scene or prefab holds it. Its canvas sits under the HUD (sorting order below 0), so the minimap, the
// panels and the rifle scope draw over a marker.
public class MapleMarkerHud : MonoBehaviour
{
    const int CanvasOrder = -5;
    static readonly Vector2 DropletSize = new Vector2(26f, 32f);
    static readonly Vector2 PailSize = new Vector2(34f, 36f); // larger: seen from up to 100 m, and the one that matters for collecting
    const float PulseHz = 1.5f, PulseAmount = 0.08f;

    static readonly Color DropletColor = new Color(0.72f, 0.92f, 0.18f);

    static MapleMarkerHud instance;

    // The colour of a tap's pail or minimap pin by state; MinimapHud uses it too.
    public static Color TapColor(MapleMarkerRules.TapMarkerState state)
    {
        switch (state)
        {
            case MapleMarkerRules.TapMarkerState.Ready: return new Color(1f, 0.72f, 0.08f);       // bright amber
            case MapleMarkerRules.TapMarkerState.Sour: return new Color(0.6f, 0.32f, 0.24f);      // muted brown-red
            case MapleMarkerRules.TapMarkerState.Filling: return new Color(0.84f, 0.82f, 0.74f);  // cream-grey
            default: return new Color(0.64f, 0.62f, 0.56f);                                        // empty: the same, dimmer
        }
    }

    readonly List<(float x, float z)> positions = new List<(float x, float z)>(); // every Maple on the map, flat
    readonly List<int> trees = new List<int>();                                   // the tree index of each entry in positions
    readonly Vector3[] tops = new Vector3[MapleMarkerRules.MaxMarkers];          // where the chosen droplets float
    int shownCount;
    readonly TapState[] tapped = new TapState[MapleRules.MaxTaps];               // the tapped trees' states (the live objects, read each frame)
    readonly Vector3[] tapTops = new Vector3[MapleRules.MaxTaps];                // where their pails float
    int tapCount;
    int builtForTreeCount = -1;
    int lastTapCount = -1;
    bool untappedWasOn;
    float nextRefresh;
    bool warnedHeight;

    GameObject canvasRoot;
    Image[] droplets;
    Image[] pails;
    PlayerController player;
    Camera cachedCamera;
    float nextPlayerSearch;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Create()
    {
        if (instance == null)
            new GameObject("Maple Marker HUD").AddComponent<MapleMarkerHud>();
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    void LateUpdate()
    {
        GameManager game = GameManager.Instance;
        WoodManager wood = WoodManager.Instance;
        if (game == null || game.State != GameState.Playing || wood == null || !wood.TreesReady)
        {
            Hide();
            return;
        }

        if (player == null)
        {
            if (Time.unscaledTime >= nextPlayerSearch)
            {
                nextPlayerSearch = Time.unscaledTime + 1f;
                player = FindAnyObjectByType<PlayerController>();
            }
            Hide();
            return;
        }

        Camera cam = CameraOf(player);
        if (cam == null)
        {
            Hide();
            return;
        }

        TapManager tapManager = TapManager.Instance;
        int tapsOut = tapManager != null ? tapManager.TapCount : 0;
        bool untappedOn = UntappedWanted(player, tapsOut);

        EnsureBuilt();
        canvasRoot.SetActive(true);

        if (untappedOn && wood.TreeCount != builtForTreeCount)
            BuildMapleList(wood);

        Vector3 at = player.transform.position;
        if (Time.unscaledTime >= nextRefresh || tapsOut != lastTapCount || untappedOn != untappedWasOn)
        {
            nextRefresh = Time.unscaledTime + MapleMarkerRules.RefreshSeconds;
            lastTapCount = tapsOut;
            untappedWasOn = untappedOn;
            ChooseUntapped(wood, tapManager, at, untappedOn);
            PlaceTapTops(wood, tapManager);
        }

        DrawUntapped(cam, at);
        DrawTapped(cam, at);
    }

    // The droplets' gate: a Spile equipped, tools usable, a tap to spare (MapleMarkerRules.ShouldShow). The pails have no gate beyond the
    // world being live.
    static bool UntappedWanted(PlayerController owner, int tapsOut)
    {
        InventoryManager inventory = InventoryManager.Instance;
        bool spile = inventory != null && inventory.EquippedTool != null && inventory.EquippedTool.Id == MapleRules.SpileId;
        return MapleMarkerRules.ShouldShow(spile, owner.CanUseTools, tapsOut);
    }

    Camera CameraOf(PlayerController owner)
    {
        if (cachedCamera == null && owner.CameraTransform != null)
            cachedCamera = owner.CameraTransform.GetComponentInChildren<Camera>();
        return cachedCamera != null ? cachedCamera : Camera.main;
    }

    void Hide()
    {
        if (canvasRoot != null && canvasRoot.activeSelf)
            canvasRoot.SetActive(false);
        nextRefresh = 0f; // choose again the moment the markers come back
        lastTapCount = -1;
    }

    // Every Maple on the map, standing or not: whether a tree is a Maple never changes (it is the index, the prototype and the size), so this is
    // built once per world. Felled and tapped trees are skipped when the nearest set is chosen.
    void BuildMapleList(WoodManager wood)
    {
        positions.Clear();
        trees.Clear();
        for (int i = 0; i < wood.TreeCount; i++)
        {
            if (!MapleRules.IsMaple(i, wood.Prototype(i), wood.HeightScale(i)))
                continue;
            Vector3 world = wood.WorldPosition(i);
            positions.Add((world.x, world.z));
            trees.Add(i);
        }
        builtForTreeCount = wood.TreeCount;
    }

    void ChooseUntapped(WoodManager wood, TapManager taps, Vector3 at, bool on)
    {
        if (!on)
        {
            shownCount = 0;
            return;
        }

        List<int> chosen = MapleMarkerRules.Nearest(positions, at.x, at.z, MapleMarkerRules.HideUnderMetres, MapleMarkerRules.RadiusMetres,
            MapleMarkerRules.MaxMarkers, i => !wood.IsStanding(trees[i]) || (taps != null && taps.TapAt(trees[i]) != null));

        shownCount = chosen.Count;
        for (int k = 0; k < shownCount; k++)
            tops[k] = TopOf(wood, trees[chosen[k]]);
    }

    void PlaceTapTops(WoodManager wood, TapManager taps)
    {
        tapCount = 0;
        if (taps == null)
            return;
        foreach (TapState tap in taps.Taps)
        {
            if (tapCount == tapped.Length)
                break;
            if (!wood.IsStanding(tap.treeIndex))
                continue;
            tapped[tapCount] = tap;
            tapTops[tapCount] = TopOf(wood, tap.treeIndex);
            tapCount++;
        }
    }

    // Just over the top of a tree. A prefab height that reads as 0 uses the fallback and says so once.
    Vector3 TopOf(WoodManager wood, int tree)
    {
        float height = wood.TreeHeight(tree);
        if (height <= 0f)
        {
            height = MapleMarkerRules.FallbackTreeHeightMetres;
            if (!warnedHeight)
            {
                warnedHeight = true;
                Debug.LogWarning($"Maple markers: a tree prefab's height read as 0, so the {height:0} m fallback is used.");
            }
        }
        return wood.WorldPosition(tree) + Vector3.up * (height + MapleMarkerRules.ClearAboveTreeMetres);
    }

    // Each frame: project the chosen tops to the screen, fade by distance, and hide what is behind the camera, near an edge, near the
    // crosshair or too close.
    void DrawUntapped(Camera cam, Vector3 at)
    {
        float width = Screen.width, height = Screen.height;
        int used = 0;
        for (int k = 0; k < shownCount; k++)
        {
            Vector3 top = tops[k];
            float distance = FlatDistance(top, at);
            if (distance < MapleMarkerRules.HideUnderMetres || distance > MapleMarkerRules.RadiusMetres)
                continue;

            Vector3 view = cam.WorldToViewportPoint(top);
            if (!MapleMarkerRules.ScreenVisible(view.x, view.y, view.z, width, height))
                continue;

            Place(droplets[used++], view, DropletColor, MapleMarkerRules.Fade(distance), 1f);
        }
        HideFrom(droplets, used);
    }

    void DrawTapped(Camera cam, Vector3 at)
    {
        float width = Screen.width, height = Screen.height;
        int used = 0;
        for (int k = 0; k < tapCount; k++)
        {
            Vector3 top = tapTops[k];
            float distance = FlatDistance(top, at);
            if (distance < MapleMarkerRules.TapHideUnderMetres || distance > MapleMarkerRules.TapRadiusMetres)
                continue;

            Vector3 view = cam.WorldToViewportPoint(top);
            if (!MapleMarkerRules.ScreenVisible(view.x, view.y, view.z, width, height))
                continue;

            MapleMarkerRules.TapMarkerState state = MapleMarkerRules.StateOf(tapped[k].litres, tapped[k].sour);
            float pulse = state == MapleMarkerRules.TapMarkerState.Ready
                ? 1f + PulseAmount * Mathf.Sin(Time.unscaledTime * PulseHz * 2f * Mathf.PI)
                : 1f;
            Place(pails[used++], view, TapColor(state), MapleMarkerRules.TapFade(distance), pulse);
        }
        HideFrom(pails, used);
    }

    static float FlatDistance(Vector3 a, Vector3 b)
    {
        float dx = a.x - b.x, dz = a.z - b.z;
        return Mathf.Sqrt(dx * dx + dz * dz);
    }

    static void Place(Image marker, Vector3 viewport, Color color, float alpha, float scale)
    {
        if (!marker.gameObject.activeSelf)
            marker.gameObject.SetActive(true);
        RectTransform rt = marker.rectTransform;
        rt.anchorMin = rt.anchorMax = new Vector2(viewport.x, viewport.y);
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = new Vector3(scale, scale, 1f);
        color.a = alpha;
        marker.color = color;
    }

    static void HideFrom(Image[] pool, int used)
    {
        for (int k = used; k < pool.Length; k++)
            if (pool[k].gameObject.activeSelf)
                pool[k].gameObject.SetActive(false);
    }

    // --- The canvas and the pools, built the first time the markers are wanted ---

    void EnsureBuilt()
    {
        if (canvasRoot != null)
            return;

        canvasRoot = new GameObject("Maple Markers", typeof(RectTransform));
        canvasRoot.transform.SetParent(transform, false);
        var canvas = canvasRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = CanvasOrder;
        var scaler = canvasRoot.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        // Droplets first, pails after: the pails draw on top where two markers cross on screen.
        droplets = BuildPool("Droplet", MapleMarkerRules.MaxMarkers, MapIcons.Droplet, DropletSize);
        pails = BuildPool("Pail", MapleRules.MaxTaps, MapIcons.Pail, PailSize);
    }

    Image[] BuildPool(string name, int count, Sprite sprite, Vector2 size)
    {
        var pool = new Image[count];
        for (int i = 0; i < count; i++)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(canvasRoot.transform, false);
            ((RectTransform)go.transform).sizeDelta = size;
            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            go.SetActive(false);
            pool[i] = image;
        }
        return pool;
    }
}
