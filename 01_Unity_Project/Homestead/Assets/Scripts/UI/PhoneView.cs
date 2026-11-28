using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Slice P1 (Mike, 2026-11-27): the smartphone. P raises it in the first-person hands (PlayerHands takes the phone pose while IsOpen): a 3D phone on the
// Viewmodel layer, its screen facing the eye. What the screen shows is a uGUI panel drawn over the 3D screen each frame, fitted to where the screen's corners
// land on the display, so it is sharp, large and clickable with the mouse. The world keeps running while it is up (it is a phone, not a pause menu), but
// the player stands still and the mouse is free. P or Esc puts it away; so does any menu opening.
//
// Apps (PhoneRules.Apps): Contractors (hire the Pole Barn crew; slice P2 adds the well driller), Weather, Bills & Plan, Market, and Mike's own games and app
// (free advertising). With no service the home screen still shows the time and date with "No Service", and only Bills & Plan opens.
//
// THE SWAP POINT for the phone itself: Resources/Art/item_smartphone.prefab (also the item's icon art), with a child named Screen (a quad facing -Z, the
// screen's size); without it a primitive phone is drawn.
public class PhoneView : MonoBehaviour
{
    public static PhoneView Instance { get; private set; }
    public static bool IsOpen => Instance != null && Instance.open;
    static int escapeFrame = -1;
    // Esc this frame put the phone away: the pause menu doesn't open as well.
    public static bool TookEscape => IsOpen || escapeFrame == Time.frameCount;

    public const string ArtId = "item_smartphone";
    const float RefW = 360f, RefH = 780f;
    const float RaiseSeconds = 0.25f;

    bool open;
    float raise;          // 0 down, 1 up
    GameObject phone;
    Transform screenQuad;
    Mesh screenMesh;
    Canvas canvas;
    RectTransform screenRoot, page;
    Text statusTime, statusSignal, homeClock, toast;
    string current = "home";
    float refreshAt;
    Font font;
    Sprite rounded;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void CreateIfMissing()
    {
        if (Instance == null)
        {
            var go = new GameObject("Phone View");
            DontDestroyOnLoad(go);
            go.AddComponent<PhoneView>();
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
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    static bool Live => GameManager.Instance != null && GameManager.Instance.State == GameState.Playing;
    static bool HasPhone => InventoryManager.Instance != null && InventoryManager.Instance.Player.Count(PhoneRules.PhoneId) > 0;
    static bool Service => PhoneManager.Instance != null && PhoneManager.Instance.HasService;

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (open)
        {
            if (!Live || !HasPhone || PlayerHands.Instance == null)
            {
                Close();
                return;
            }
            if (keyboard != null && (keyboard.pKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame))
            {
                if (keyboard.escapeKey.wasPressedThisFrame)
                    escapeFrame = Time.frameCount;
                Close();
                return;
            }
            if (Time.unscaledTime >= refreshAt)
            {
                refreshAt = Time.unscaledTime + 1f;
                RefreshClock();
            }
            return;
        }
        if (keyboard == null || !keyboard.pKey.wasPressedThisFrame || GameScreens.IsTyping || !Live)
            return;
        if (PlacementMode.Active || CartHandler.Pushing || CreekCrossing.InputLocked || PlayerHands.Instance == null || !PlayerHands.Instance.IsBuilt)
            return;
        if (!HasPhone)
        {
            ToolStatus.Flash(PhoneRules.NoPhone, 3f);
            return;
        }
        Open();
    }

    void Open()
    {
        open = true;
        raise = 0f;
        BuildPhone();
        BuildCanvas();
        canvas.gameObject.SetActive(true);
        Show("home");
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Close()
    {
        open = false;
        if (phone != null)
            phone.SetActive(false);
        if (canvas != null)
            canvas.gameObject.SetActive(false);
        if (Live)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void LateUpdate()
    {
        if (!open || phone == null || PlayerHands.Instance == null)
            return;
        raise = Mathf.MoveTowards(raise, 1f, Time.unscaledDeltaTime / RaiseSeconds);
        float e = HandPoseRules.Ease(raise);
        var at = HandPoseRules.PhoneAt;
        phone.SetActive(PlayerHands.Instance.Rig != null && PlayerHands.Instance.Rig.activeInHierarchy);
        phone.transform.localPosition = new Vector3(at.x, at.y - (1f - e) * 0.22f, at.z);
        phone.transform.localRotation = Quaternion.Euler((1f - e) * 30f, 0f, 0f);
        FitScreen();
    }

    // --- The 3D phone ---

    void BuildPhone()
    {
        Transform rig = PlayerHands.Instance.Rig.transform;
        if (phone != null && phone.transform.parent == rig)
        {
            phone.SetActive(true);
            return;
        }
        if (phone != null)
            Destroy(phone);
        GameObject prefab = Resources.Load<GameObject>("Art/" + ArtId);
        if (prefab != null)
        {
            phone = Instantiate(prefab, rig, false);
            phone.name = prefab.name;
            screenQuad = FindDeep(phone.transform, "Screen");
        }
        if (phone == null || screenQuad == null)
        {
            if (phone != null)
                Destroy(phone);
            phone = PrimitivePhone(rig);
        }
        foreach (Collider c in phone.GetComponentsInChildren<Collider>())
            Destroy(c);
        SetLayer(phone.transform, PlayerHands.Instance.Layer);
        phone.transform.localScale = Vector3.one * HandPoseRules.PhoneScale;
        MeshFilter mf = screenQuad.GetComponent<MeshFilter>();
        screenMesh = mf != null ? mf.sharedMesh : null;
    }

    GameObject PrimitivePhone(Transform rig)
    {
        var root = new GameObject(ArtId);
        root.transform.SetParent(rig, false);
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "Body";
        body.transform.SetParent(root.transform, false);
        body.transform.localScale = new Vector3(0.072f, 0.152f, 0.008f);
        Tint(body, new Color(0.08f, 0.08f, 0.09f));
        GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Quad);
        screen.name = "Screen";
        screen.transform.SetParent(root.transform, false);
        screen.transform.localPosition = new Vector3(0f, 0f, -0.0042f);
        screen.transform.localScale = new Vector3(0.066f, 0.143f, 1f);
        Tint(screen, Color.black);
        screenQuad = screen.transform;
        return root;
    }

    static void Tint(GameObject piece, Color color)
    {
        var block = new MaterialPropertyBlock();
        block.SetColor("_BaseColor", color);
        piece.GetComponent<MeshRenderer>().SetPropertyBlock(block);
    }

    static Transform FindDeep(Transform t, string name)
    {
        if (t.name == name)
            return t;
        foreach (Transform child in t)
        {
            Transform found = FindDeep(child, name);
            if (found != null)
                return found;
        }
        return null;
    }

    static void SetLayer(Transform t, int layer)
    {
        t.gameObject.layer = layer;
        foreach (Transform child in t)
            SetLayer(child, layer);
    }

    // Fits the screen panel over where the 3D screen's corners land on the display.
    void FitScreen()
    {
        Camera view = PlayerHands.Instance.ViewCamera;
        if (view == null || screenQuad == null || screenRoot == null)
            return;
        Bounds b = screenMesh != null ? screenMesh.bounds : new Bounds(Vector3.zero, Vector3.one);
        Vector2 min = new Vector2(float.MaxValue, float.MaxValue), max = new Vector2(float.MinValue, float.MinValue);
        for (int i = 0; i < 4; i++)
        {
            Vector3 local = new Vector3(i % 2 == 0 ? b.min.x : b.max.x, i < 2 ? b.min.y : b.max.y, b.center.z);
            Vector3 p = view.WorldToScreenPoint(screenQuad.TransformPoint(local));
            min = Vector2.Min(min, p);
            max = Vector2.Max(max, p);
        }
        float h = max.y - min.y;
        if (h <= 1f)
            return;
        float scale = h / RefH / canvas.scaleFactor;
        screenRoot.anchoredPosition = (min + max) / 2f / canvas.scaleFactor;
        screenRoot.localScale = new Vector3(scale, scale, 1f);
    }

    // --- The screen ---

    void BuildCanvas()
    {
        if (canvas != null)
            return;
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        rounded = RoundedSprite();
        var go = new GameObject("Phone Screen");
        go.transform.SetParent(transform, false);
        canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 40;
        go.AddComponent<GraphicRaycaster>();

        screenRoot = Rect("Screen", go.transform);
        screenRoot.anchorMin = screenRoot.anchorMax = Vector2.zero;
        screenRoot.pivot = new Vector2(0.5f, 0.5f);
        screenRoot.sizeDelta = new Vector2(RefW, RefH);
        Image back = screenRoot.gameObject.AddComponent<Image>();
        back.sprite = GradientSprite(new Color(0.09f, 0.20f, 0.32f), new Color(0.03f, 0.07f, 0.13f));
        screenRoot.gameObject.AddComponent<RectMask2D>();

        // The status bar: the time, and the signal and battery.
        RectTransform bar = Rect("Status", screenRoot);
        Place(bar, new Vector2(0f, 1f), Vector2.one, new Vector2(0f, -34f), Vector2.zero);
        statusTime = Label(bar, "", 18, Color.white, TextAnchor.MiddleLeft, true);
        Place(statusTime.rectTransform, Vector2.zero, Vector2.one, new Vector2(20f, 0f), Vector2.zero);
        statusSignal = Label(bar, "", 16, Color.white, TextAnchor.MiddleRight, true);
        Place(statusSignal.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-20f, 0f));

        page = Rect("Page", screenRoot);
        Place(page, Vector2.zero, Vector2.one, new Vector2(0f, 26f), new Vector2(0f, -34f));

        // The home bar: click it to go home.
        Image home = Panel(screenRoot, "Home Bar", new Color(1f, 1f, 1f, 0.85f));
        Place(home.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-60f, 8f), new Vector2(60f, 14f));
        Button homeButton = home.gameObject.AddComponent<Button>();
        homeButton.onClick.AddListener(() => Show("home"));

        // A message for a moment at the bottom of the screen.
        Image toastPanel = Panel(screenRoot, "Toast", new Color(0f, 0f, 0f, 0.82f));
        Place(toastPanel.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(16f, 34f), new Vector2(-16f, 110f));
        toast = Label(toastPanel.rectTransform, "", 17, Color.white, TextAnchor.MiddleCenter, true);
        Place(toast.rectTransform, Vector2.zero, Vector2.one, new Vector2(10f, 4f), new Vector2(-10f, -4f));
        toastPanel.gameObject.SetActive(false);
        toastRoot = toastPanel.gameObject;
    }

    GameObject toastRoot;

    void Toast(string message)
    {
        if (string.IsNullOrEmpty(message) || toastRoot == null)
            return;
        toast.text = message;
        toastRoot.SetActive(true);
        toastRoot.transform.SetAsLastSibling();
        CancelInvoke(nameof(HideToast));
        Invoke(nameof(HideToast), 4f);
    }

    void HideToast()
    {
        if (toastRoot != null)
            toastRoot.SetActive(false);
    }

    void RefreshClock()
    {
        TimeManager time = TimeManager.Instance;
        string clock = time != null ? PhoneRules.Clock(time.Hour, time.Minute) : "";
        statusTime.text = clock;
        statusSignal.text = Service ? "5G    100%" : PhoneRules.NoService + "   100%";
        if (homeClock != null)
            homeClock.text = clock;
    }

    void Show(string id)
    {
        current = id;
        for (int i = page.childCount - 1; i >= 0; i--)
            Destroy(page.GetChild(i).gameObject);
        homeClock = null;
        if (id == "home")
            BuildHome();
        else
            BuildApp(id);
        RefreshClock();
        if (toastRoot != null)
            toastRoot.transform.SetAsLastSibling();
    }

    // --- Home ---

    void BuildHome()
    {
        TimeManager time = TimeManager.Instance;
        WeatherManager weather = WeatherManager.Instance;
        homeClock = Label(page, "", 64, Color.white, TextAnchor.MiddleCenter, false);
        Place(homeClock.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(0f, -96f), new Vector2(0f, -12f));
        Text date = Label(page, time != null ? time.FormatDate(time.TotalDays) : "", 20, new Color(1f, 1f, 1f, 0.9f), TextAnchor.MiddleCenter, false);
        Place(date.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(0f, -126f), new Vector2(0f, -96f));
        string line = !Service ? PhoneRules.NoServiceHelp
            : weather != null ? $"{weather.TemperatureF:0}°F  ·  {weather.PlayerWeatherName()}" : "";
        Text sub = Label(page, line, 18, Service ? Color.white : new Color(1f, 0.78f, 0.35f), TextAnchor.MiddleCenter, true);
        Place(sub.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(16f, -170f), new Vector2(-16f, -130f));

        // The apps: three across.
        const float cellW = 112f, cellH = 122f, top = 196f;
        for (int i = 0; i < PhoneRules.Apps.Length; i++)
        {
            var app = PhoneRules.Apps[i];
            bool works = Service || !app.needsService;
            int col = i % 3, row = i / 3;
            RectTransform cell = Rect(app.id, page);
            cell.anchorMin = cell.anchorMax = new Vector2(0.5f, 1f);
            cell.pivot = new Vector2(0.5f, 1f);
            cell.sizeDelta = new Vector2(cellW, cellH);
            cell.anchoredPosition = new Vector2((col - 1) * cellW, -top - row * cellH);
            Image icon = Panel(cell, "Icon", Color.white);
            icon.sprite = Resources.Load<Sprite>("Phone/app_" + app.id);
            icon.preserveAspect = true;
            icon.color = works ? Color.white : new Color(1f, 1f, 1f, 0.35f);
            Place(icon.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-38f, -80f), new Vector2(38f, -4f));
            Text label = Label(cell, app.label, 15, works ? Color.white : new Color(1f, 1f, 1f, 0.45f), TextAnchor.UpperCenter, false);
            Place(label.rectTransform, Vector2.zero, new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 40f));
            Button button = icon.gameObject.AddComponent<Button>();
            string id = app.id;
            button.onClick.AddListener(() =>
            {
                if (works)
                    Show(id);
                else
                    Toast(PhoneRules.NeedsService);
            });
        }
    }

    // --- The apps ---

    RectTransform content;

    void BuildApp(string id)
    {
        string title = "";
        foreach (var app in PhoneRules.Apps)
            if (app.id == id)
                title = app.label;

        Image header = Panel(page, "Header", new Color(0f, 0f, 0f, 0.35f));
        Place(header.rectTransform, new Vector2(0f, 1f), Vector2.one, new Vector2(0f, -52f), Vector2.zero);
        Button back = Btn(header.rectTransform, "‹", 30, new Color(1f, 1f, 1f, 0.12f), () => Show("home"));
        Place((RectTransform)back.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(8f, -20f), new Vector2(52f, 20f));
        Text t = Label(header.rectTransform, title, 22, Color.white, TextAnchor.MiddleCenter, true);
        Place(t.rectTransform, Vector2.zero, Vector2.one, new Vector2(56f, 0f), new Vector2(-56f, 0f));

        // A scrolling column.
        RectTransform view = Rect("View", page);
        Place(view, Vector2.zero, Vector2.one, new Vector2(0f, 0f), new Vector2(0f, -56f));
        view.gameObject.AddComponent<RectMask2D>();
        Image catcher = view.gameObject.AddComponent<Image>();
        catcher.color = new Color(0f, 0f, 0f, 0f);
        content = Rect("Content", view);
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = Vector2.one;
        content.pivot = new Vector2(0.5f, 1f);
        content.sizeDelta = Vector2.zero;
        var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(12, 12, 6, 40);
        layout.spacing = 8f;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        var scroll = view.gameObject.AddComponent<ScrollRect>();
        scroll.content = content;
        scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 30f;

        if (PhoneRules.IsAd(id)) BuildAd(id);
        else if (id == "weather") BuildWeather();
        else if (id == "market") BuildMarket();
        else if (id == "bills") BuildBills();
        else if (id == "contractors") BuildContractors();
        else if (id == "power") foreach (var row in PowerStructures.Summary()) Row(row.label, row.value); // the power system (E1), the same rows as the Power Shed's panel
    }

    void BuildAd(string id)
    {
        Image icon = Panel(content, "Big Icon", Color.white);
        icon.sprite = Resources.Load<Sprite>("Phone/app_" + id);
        icon.preserveAspect = true;
        Fixed(icon.gameObject, 190f);
        string name = id;
        foreach (var app in PhoneRules.Apps)
            if (app.id == id)
                name = app.label;
        Fixed(Label(content, name, 26, Color.white, TextAnchor.MiddleCenter, true).gameObject, 40f);
        Fixed(Label(content, PhoneRules.AdLine(id), 18, new Color(1f, 1f, 1f, 0.85f), TextAnchor.UpperCenter, false).gameObject, 60f);
    }

    void BuildWeather()
    {
        WeatherManager weather = WeatherManager.Instance;
        TimeManager time = TimeManager.Instance;
        if (weather == null)
        {
            Row("Weather", "Not available");
            return;
        }
        Fixed(Label(content, $"{weather.TemperatureF:0}°", 64, Color.white, TextAnchor.MiddleCenter, false).gameObject, 80f);
        Fixed(Label(content, weather.PlayerWeatherName(), 24, Color.white, TextAnchor.MiddleCenter, true).gameObject, 34f);
        string feels = weather.FeelsLikeText();
        if (feels.Length > 0)
            Fixed(Label(content, feels, 18, new Color(1f, 1f, 1f, 0.85f), TextAnchor.MiddleCenter, false).gameObject, 28f);
        Row("Precipitation", weather.PrecipitationWord);
        Row("Wind", weather.WindWord + (weather.WindWord == "Calm" ? "" : $", {weather.WindSpeedMph:0} mph from the {weather.WindFromWords}"));
        Row("Humidity", $"{weather.RelativeHumidity:0}%");
        float low = weather.ExpectedNightLowC() * 9f / 5f + 32f;
        Row("Tonight's low", $"about {low:0}°F" + (low <= 32f ? " - frost likely" : ""));
        if (time != null)
            Row("Sun", $"Up {PhoneRules.Clock((int)time.SunriseHour, (int)(time.SunriseHour % 1f * 60f))}  ·  Down {PhoneRules.Clock((int)time.SunsetHour, (int)(time.SunsetHour % 1f * 60f))}");
        Row("Creek", StreamManager.CreekRowText());
    }

    void BuildMarket()
    {
        MarketManager market = MarketManager.Instance;
        if (market == null)
        {
            Row("Market", "Not available");
            return;
        }
        if (!string.IsNullOrEmpty(market.Note))
            Fixed(Label(content, market.Note, 16, new Color(1f, 1f, 1f, 0.85f), TextAnchor.MiddleLeft, false).gameObject, 46f);
        Section("The store buys from you");
        var sells = new List<(string name, long cents)>();
        foreach (MarketRow row in market.Table.Rows)
            if (market.CanSell(row.itemId))
                sells.Add((ItemDatabase.Get(row.itemId)?.DisplayName ?? row.itemId, market.SellPrice(row.itemId)));
        sells.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase));
        foreach (var s in sells)
            Row(s.name, Money.Format(s.cents));
        Section("The store sells");
        var buys = new List<(string name, long cents)>();
        foreach ((string item, long price) in TradingPost.Catalog)
            buys.Add((ItemDatabase.Get(item)?.DisplayName ?? item, price));
        buys.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase));
        foreach (var b in buys)
            Row(b.name, Money.Format(b.cents));
    }

    void BuildBills()
    {
        PhoneManager phone = PhoneManager.Instance;
        if (phone == null)
            return;
        PhoneState s = phone.State;
        Section("Your plan");
        Row("Plan", PhoneRules.PlanLine);
        Row("Status", PhoneRules.Status(s));
        if (s.active && !s.suspended)
            Row(s.cancelling ? "Ends on" : "Next bill", PhoneManager.DateOf(s.nextBillDay));
        Row("Cash", TradingPost.Instance != null ? Money.Format(TradingPost.Instance.Cents) : "-");
        if (!s.active || s.cancelling)
            Action(s.active ? "Keep my plan" : $"Start the plan  ({Money.Format(PhoneRules.PlanCents)} now)", new Color(0.2f, 0.62f, 0.36f), () =>
            {
                phone.StartPlan(out string message);
                Show("bills");
                Toast(message);
            });
        if (s.active && s.suspended)
            Action($"Pay {Money.Format(s.owedCents)} now", new Color(0.2f, 0.62f, 0.36f), () =>
            {
                phone.PayOverdue(out string message);
                Show("bills");
                Toast(message);
            });
        if (s.active && !s.cancelling)
            Action("Cancel the plan", new Color(0.62f, 0.22f, 0.2f), () =>
            {
                phone.CancelPlan();
                Show("bills");
                Toast(phone.State.active ? "Cancelled - service runs to the next bill day" : "Plan ended");
            });
        if (s.history.Count > 0)
        {
            Section("Recent bills");
            for (int i = s.history.Count - 1; i >= 0; i--)
                Row(PhoneManager.DateOf(s.history[i].day), Money.Format(s.history[i].cents) + (s.history[i].paid ? "  paid" : "  unpaid"));
        }
    }

    void BuildContractors()
    {
        WoodManager wood = WoodManager.Instance;
        TradingPost post = TradingPost.Instance;
        Section("Pole Barn crews");
        List<WoodPileState> sites = wood != null ? wood.BarnSites() : new List<WoodPileState>();
        if (sites.Count == 0)
            Fixed(Label(content, BarnRules.NoSite, 17, new Color(1f, 1f, 1f, 0.85f), TextAnchor.MiddleLeft, false).gameObject, 66f);
        foreach (WoodPileState site in sites)
        {
            BarnRules.Size size = WoodManager.BarnSizeOf(site.kind);
            CrewJob job = post != null ? post.CrewFor(site.id) : null;
            Row(BarnRules.SiteName(size), job != null ? CrewWords(size, job) : $"{Money.Format(BarnRules.CrewCents(size))}, half now");
            if (job == null && post != null)
            {
                WoodPileState target = site;
                Action($"Hire the crew  ({Money.Format(BarnRules.CrewDeposit(size))} now)", new Color(0.85f, 0.5f, 0.15f), () =>
                {
                    post.HireBarnCrew(target, out string message);
                    Show("contractors");
                    Toast(message);
                });
            }
        }
        Fixed(Label(content, "They start " + BarnRules.LeadDays + " days after booking; the balance is paid when they finish.", 15, new Color(1f, 1f, 1f, 0.7f), TextAnchor.UpperLeft, false).gameObject, 44f);

        // The well driller (slice P2): a quote by the depth where each site was staked.
        Section("Well drilling");
        var wells = new List<WoodPileState>();
        if (wood != null)
            foreach (WoodPileState pile in wood.Piles)
                if (pile.kind == PileKind.DrilledWellSite)
                    wells.Add(pile);
        if (wells.Count == 0)
            Fixed(Label(content, DrillRules.NoSite, 17, new Color(1f, 1f, 1f, 0.85f), TextAnchor.MiddleLeft, false).gameObject, 66f);
        foreach (WoodPileState site in wells)
        {
            CrewJob job = post != null ? post.CrewFor(site.id) : null;
            Row(DrillRules.SiteName, job != null ? DrillWords(job) : DrillRules.Quote(site.depthToWater));
            if (job == null && post != null)
            {
                WoodPileState target = site;
                Action($"Hire the driller  ({Money.Format(DrillRules.Deposit(DrillRules.JobCents(site.depthToWater)))} now)", new Color(0.2f, 0.5f, 0.85f), () =>
                {
                    post.HireDriller(target, out string message);
                    Show("contractors");
                    Toast(message);
                });
            }
        }
        Fixed(Label(content, $"The rig comes {DrillRules.LeadDays} days after booking and drills for {DrillRules.DrillDays}; the balance is paid when it is done.", 15, new Color(1f, 1f, 1f, 0.7f), TextAnchor.UpperLeft, false).gameObject, 44f);
    }

    static string DrillWords(CrewJob job)
    {
        double now = TimeManager.Instance != null ? TimeManager.Instance.TotalHours : 0;
        float progress = DrillRules.Progress(job.bookedDay, now);
        int today = TimeManager.Instance != null ? TimeManager.Instance.TotalDays : 0;
        return progress <= 0f ? DrillRules.Waiting(job.bookedDay + DrillRules.LeadDays - today) : progress >= 1f ? "Drilled - balance due" : $"Drilling - {progress * 100f:0}% done";
    }

    static string CrewWords(BarnRules.Size size, CrewJob job)
    {
        double now = TimeManager.Instance != null ? TimeManager.Instance.TotalHours : 0;
        float progress = BarnRules.CrewProgress(size, job.bookedDay, now);
        int today = TimeManager.Instance != null ? TimeManager.Instance.TotalDays : 0;
        return progress <= 0f ? BarnRules.CrewWaiting(size, job.bookedDay + BarnRules.LeadDays - today) : progress >= 1f ? "Finished - balance due" : $"Building - {progress * 100f:0}% done";
    }

    // --- Pieces ---

    void Section(string text) => Fixed(Label(content, text, 18, new Color(0.6f, 0.82f, 1f), TextAnchor.LowerLeft, true).gameObject, 34f);

    void Row(string label, string value)
    {
        Image card = Panel(content, label, new Color(1f, 1f, 1f, 0.08f));
        card.sprite = rounded;
        card.type = Image.Type.Sliced;
        Text name = Label(card.rectTransform, label, 16, new Color(1f, 1f, 1f, 0.75f), TextAnchor.UpperLeft, false);
        Place(name.rectTransform, Vector2.zero, Vector2.one, new Vector2(12f, 6f), new Vector2(-12f, -6f));
        Text val = Label(card.rectTransform, value, 18, Color.white, TextAnchor.LowerLeft, true);
        Place(val.rectTransform, Vector2.zero, Vector2.one, new Vector2(12f, 6f), new Vector2(-12f, -26f));
        val.horizontalOverflow = HorizontalWrapMode.Wrap;
        float lines = Mathf.Max(1f, Mathf.Ceil(value.Length / 28f));
        Fixed(card.gameObject, 34f + 22f * lines);
    }

    void Action(string label, Color color, UnityEngine.Events.UnityAction onClick)
    {
        Button button = Btn(content, label, 18, color, onClick);
        Fixed(button.gameObject, 50f);
    }

    static void Fixed(GameObject go, float height)
    {
        var le = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
        le.minHeight = le.preferredHeight = height;
    }

    Button Btn(Transform parent, string label, int size, Color color, UnityEngine.Events.UnityAction onClick)
    {
        Image back = Panel(parent, label, color);
        back.sprite = rounded;
        back.type = Image.Type.Sliced;
        Button button = back.gameObject.AddComponent<Button>();
        button.onClick.AddListener(onClick);
        Text t = Label(back.rectTransform, label, size, Color.white, TextAnchor.MiddleCenter, true);
        Place(t.rectTransform, Vector2.zero, Vector2.one, new Vector2(6f, 0f), new Vector2(-6f, 0f));
        return button;
    }

    Text Label(Transform parent, string text, int size, Color color, TextAnchor anchor, bool bold)
    {
        RectTransform rt = Rect("Text", parent);
        Text t = rt.gameObject.AddComponent<Text>();
        t.font = font;
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.alignment = anchor;
        t.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }

    static Image Panel(Transform parent, string name, Color color)
    {
        RectTransform rt = Rect(name, parent);
        Image image = rt.gameObject.AddComponent<Image>();
        image.color = color;
        return image;
    }

    static RectTransform Rect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    static void Place(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
    }

    static Sprite RoundedSprite()
    {
        const int n = 48, r = 14;
        var tex = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float dx = Mathf.Max(0f, Mathf.Max(r - x - 0.5f, x + 0.5f - (n - r))), dy = Mathf.Max(0f, Mathf.Max(r - y - 0.5f, y + 0.5f - (n - r)));
                float a = Mathf.Clamp01(r - Mathf.Sqrt(dx * dx + dy * dy) + 0.5f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(r, r, r, r));
    }

    static Sprite GradientSprite(Color top, Color bottom)
    {
        var tex = new Texture2D(1, 64, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
        for (int y = 0; y < 64; y++)
            tex.SetPixel(0, y, Color.Lerp(bottom, top, y / 63f));
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 64), new Vector2(0.5f, 0.5f));
    }
}
