using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Building_Housing_System.md's first build, the Sleep System (2026-09-26): the first real Sleep in the game. The player
// sleeps in a placed Tent or Lean-To (Shelter), or anywhere on the bare Sleeping Bag (from the Inventory). Sleep fades
// out, passes the night — until 6 in the morning, or a 3-hour rest in the daytime — with Hydration, Hunger, Warmth,
// Health, the fires and the weather all carrying on, wakes the player, refills their stamina, and auto-saves
// (Save_Data_Model.md's "Auto-save at Sleep").
//
// What the bed is worth goes through the survival stats already there (Health_System.md's Warmth and Recovery), not a
// new one. First pass, for Mike to confirm by feel:
//   Sleeping Bag alone — cuts Warmth loss by a quarter; no cover from rain or wind. Health recovers 1.5x while asleep.
//   Lean-To — cuts Warmth loss by half, keeps off most rain (80%) and wind (60%). Health 2x.
//   Tent — cuts Warmth loss by 60%, keeps off all rain and most wind (90%). Health 2x.
//   Small Cabin (Building_Housing_System.md's first permanent residence) — the best tier yet: cuts Warmth loss by 75%,
//   keeps off all rain and wind (a real enclosed shell). Health 2.5x. Its furnished hearth (FireManager.BuildFurnished)
//   is a real campfire, so a lit one warms the sleeper the same way any nearby campfire already does — no extra code.
//   Tarp Shelter (a pitched Tarp, 2026-10-09) — the lowest tier: cuts Warmth loss by a quarter, keeps off most rain (90%)
//   and half the wind. Health 1.5x. Above sleeping rough, below the Lean-To, so the tiers run nothing, Tarp, Lean-To, Tent, Cabin.
// A Sleeping Bag inside a shelter adds its own quarter on top. Sleeping out, a carried Tarp strung overhead keeps off
// most rain (90%) and some wind (30%) (Difficulty_System.md's kit). Your Tarp keeps the rain off while you sleep
// outdoors, or pitch it for a proper shelter. A campfire close by still warms as usual. The cold,
// thirst or hunger can wake the player early rather than let them sleep into real harm.
public class SleepManager : MonoBehaviour
{
    public const string SleepingBagId = "sleeping_bag";
    public const string TarpId = "tarp";
    public const string TarpNote = "Your Tarp keeps the rain off while you sleep outdoors, or pitch it for a proper shelter.";

    public static SleepManager Instance { get; private set; }

    [SerializeField, Min(0f)] float morningHour = 6f;
    [Tooltip("From this hour on it's night: sleep runs to morning. Earlier, sleep is a short rest.")]
    [SerializeField, Min(0f)] float nightFromHour = 19f;
    [SerializeField, Min(0.5f)] float restHours = 3f;
    [SerializeField, Min(0.05f)] float fadeSeconds = 0.8f;
    [SerializeField, Min(0f)] float holdSeconds = 1.4f;

    [Header("Wake early if")]
    [SerializeField] float wakeBelowWarmth = 20f;
    [SerializeField] float wakeBelowHealth = 15f;
    [Tooltip("Core temperature (°C) above which the player wakes too hot to sleep (heat exhaustion starts at 38).")]
    [SerializeField] float wakeAboveCoreC = 38f;

    [Header("Collapse from heat")]
    [Tooltip("A heat collapse doesn't end after its 8-16 hours at the hottest part of the day: the skip runs on to the next " +
             "dusk or dawn that falls after those hours.")]
    [SerializeField] bool heatCollapseWakesAtDuskOrDawn = true;

    CanvasGroup overlay;
    Text overlayText;

    public bool IsSleeping { get; private set; }

    public static bool CarryingBag => InventoryManager.Instance != null && InventoryManager.Instance.Player.Has(SleepingBagId);

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildOverlay();
    }

    void OnDestroy()
    {
        if (watched != null)
            watched.Collapsed -= OnCollapsed;
        if (Instance == this)
            Instance = null;
    }

    // Why sleep isn't possible right now (null if it is). shelter: the Tent or Lean-To being slept in, or null for
    // the bare Sleeping Bag.
    public string CantSleepReason(WoodPileState shelter)
    {
        if (IsSleeping)
            return "Already asleep.";
        if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing && GameManager.Instance.State != GameState.Menu)
            return "Not now.";
        if (shelter == null && !CarryingBag)
            return "You need a Sleeping Bag, or a Tent or Lean-To, to sleep.";
        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null && Animal.IsOverWater(player.transform.position, out _))
            return "You can't sleep in the water.";
        return null;
    }

    public bool Sleep(WoodPileState shelter)
    {
        string reason = CantSleepReason(shelter);
        if (reason != null)
        {
            ToolStatus.Flash(reason);
            return false;
        }
        StartCoroutine(SleepRoutine(shelter));
        return true;
    }

    IEnumerator SleepRoutine(WoodPileState shelter)
    {
        IsSleeping = true;
        // Where the player sleeps is the homestead's heart for the sanitary-homestead test (WellsAndOuthouses).
        PlayerController sleeper = FindAnyObjectByType<PlayerController>();
        if (shelter != null)
            WellsAndOuthouses.NoteSleepPlace(shelter.position);
        else if (sleeper != null)
            WellsAndOuthouses.NoteSleepPlace(sleeper.transform.position);
        GameManager game = GameManager.Instance;
        bool openedMenu = game != null && game.State == GameState.Playing && game.OpenMenu();

        bool roughWithTarp = shelter == null && InventoryManager.Instance != null && InventoryManager.Instance.Player.Has(TarpId);
        yield return Fade(1f, shelter != null ? $"Sleeping in the {WoodManager.PileName(shelter.kind)}…"
                              : roughWithTarp ? $"Sleeping…\n{TarpNote}" : "Sleeping…");

        float slept = PassNight(shelter, out string woke);
        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null)
            player.Rest();

        TimeManager time = TimeManager.Instance;
        string clock = time != null ? $"{time.Hour:00}:{time.Minute:00}" : "";
        overlayText.text = woke ?? $"You slept {slept:0.#} hours.  {clock}";
        yield return new WaitForSecondsRealtime(holdSeconds);
        yield return Fade(0f, overlayText.text);

        if (openedMenu && game != null)
            game.CloseMenu();
        IsSleeping = false;
        ToolStatus.Flash(woke ?? $"Slept {slept:0.#} hours — rested", 4f);
        if (SaveManager.Instance != null)
            SaveManager.Instance.RequestAutoSave("sleep");
    }

    // Runs the hours of sleep in short steps. Returns the hours slept, with why the player woke early (or null).
    float PassNight(WoodPileState shelter, out string wokeEarly)
    {
        wokeEarly = null;
        TimeManager time = TimeManager.Instance;
        SurvivalManager survival = SurvivalManager.Instance;
        if (time == null)
            return 0f;

        float hour = time.HourOfDay;
        float hours = hour >= nightFromHour ? 24f - hour + morningHour
                    : hour < morningHour ? morningHour - hour
                    : restHours;

        // What the bed and shelter keep off.
        float insulation = 0f, rain = 0f, wind = 0f, recovery = 1f;
        if (shelter != null)
        {
            bool tent = shelter.kind == PileKind.Tent;
            bool cabin = shelter.kind == PileKind.Cabin;
            bool tarp = shelter.kind == PileKind.TarpShelter;
            insulation = cabin ? 0.75f : tent ? 0.6f : tarp ? 0.25f : 0.5f;
            rain = cabin || tent ? 1f : tarp ? 0.9f : 0.8f;
            wind = cabin ? 1f : tent ? 0.9f : tarp ? 0.5f : 0.6f;
            recovery = cabin ? 2.5f : tarp ? 1.5f : 2f;
        }
        float shelterInsulation = insulation; // without the bag's quarter: SurvivalManager drops the bag in warm air
        if (CarryingBag)
        {
            insulation = 1f - (1f - insulation) * 0.75f;
            recovery = Mathf.Max(recovery, 1.5f);
        }
        if (shelter == null && InventoryManager.Instance != null && InventoryManager.Instance.Player.Has(TarpId))
        {
            rain = Mathf.Max(rain, 0.9f);
            wind = Mathf.Max(wind, 0.3f);
        }
        if (survival != null)
        {
            survival.SleepInsulation = insulation;
            survival.SleepShelterInsulation = shelterInsulation;
            survival.SleepRainShelter = rain;
            survival.SleepWindShelter = wind;
            survival.SleepRecovery = recovery + WellsAndOuthouses.SleepRecoveryBonus; // a sanitary homestead rests a little better
        }

        const float Step = 0.25f;
        float slept = 0f;
        while (slept < hours - 0.001f)
        {
            float step = Mathf.Min(Step, hours - slept);
            if (survival != null)
                survival.PassHours(step);
            if (FireManager.Instance != null)
                FireManager.Instance.PassHours(step);
            if (WoodManager.Instance != null)
                WoodManager.Instance.PassHours(step);
            time.AdvanceMinutes(step * 60f);
            slept += step;

            if (survival == null)
                continue;
            if (survival.Warmth < wakeBelowWarmth)
                wokeEarly = "You wake shivering — too cold to sleep.";
            else if (survival.CoreTemperatureC > wakeAboveCoreC)
                wokeEarly = "You wake drenched in sweat — too hot to sleep.";
            else if (survival.Hydration <= 0f)
                wokeEarly = "You wake parched — you need water.";
            else if (survival.Hunger <= 0f)
                wokeEarly = "You wake starving — you need food.";
            else if (survival.Health < wakeBelowHealth)
                wokeEarly = "You wake feeling terrible.";
            if (wokeEarly != null && slept >= 0.5f)
                break;
            wokeEarly = null;
        }

        if (survival != null)
        {
            survival.SleepInsulation = survival.SleepShelterInsulation = survival.SleepRainShelter = survival.SleepWindShelter = 0f;
            survival.SleepRecovery = 1f;
        }
        if (wokeEarly != null)
            wokeEarly += $"  (slept {slept:0.#} h)";
        return slept;
    }

    // --- Player Collapse (Health_System.md, "wake weakened") ---
    //
    // Health reaching 0, whatever drained it, fades the screen out, runs the world on for 8-16 hours (fires, wood and
    // weather; the player's own Hunger, Hydration, Warmth and Health do not tick), and wakes the player at the nearest
    // placed Tent, Lean-To or finished Small Cabin at any distance — or where they fell if there is none — weakened.
    // SurvivalManager holds the numbers and the waking state. Polled here rather than run from the Collapsed event, which
    // fires inside SurvivalManager's per-tick Step; polling also catches a save loaded at Health 0. Not while asleep:
    // the sleep ends first. No autosave.

    float nextCollapseCheck;
    bool collapsePending;
    SurvivalManager watched;

    // Latched from the Collapsed event as well as polled: with nothing else draining Health (the Shift+F10 debug collapse,
    // or any cause that stops the moment Health hits 0) the next survival tick regenerates Health above 0 and clears
    // IsCollapsed before this Update sees it, and the collapse would be lost with no fade.
    void OnCollapsed() => collapsePending = true;

    void Update()
    {
        SurvivalManager survival = SurvivalManager.Instance;
        if (survival != null && watched != survival)
        {
            if (watched != null)
                watched.Collapsed -= OnCollapsed;
            watched = survival;
            watched.Collapsed += OnCollapsed;
        }

        GameManager game = GameManager.Instance;
        if (survival == null || !(collapsePending || survival.IsCollapsed) || IsSleeping || game == null || game.State != GameState.Playing)
            return;
        if (Time.unscaledTime < nextCollapseCheck)
            return;

        nextCollapseCheck = Time.unscaledTime + 0.5f;
        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null && TimeManager.Instance != null)
        {
            collapsePending = false;
            StartCoroutine(CollapseRoutine(player));
        }
    }

    IEnumerator CollapseRoutine(PlayerController player)
    {
        IsSleeping = true; // blocks pausing and the other sleep-time rules, same as SleepRoutine
        GameManager game = GameManager.Instance;
        bool openedMenu = game != null && game.State == GameState.Playing && game.OpenMenu();
        SurvivalManager survival = SurvivalManager.Instance;
        TimeManager time = TimeManager.Instance;

        SurvivalManager.CollapsePlan plan = SurvivalManager.PlanFor(survival.RegisterCollapse(time.TotalDays));
        Vector3 fell = player.transform.position;

        yield return Fade(1f, "…");

        string dropped = plan.dropsItem ? DropOneCarriedItem(fell, player.transform) : null;

        WoodPileState shelter = NearestWakeShelter(fell);
        if (shelter != null)
        {
            FindWakeSpot(shelter, out Vector3 spot, out float yaw);
            player.Teleport(spot, yaw);
        }

        // A morning heat collapse would wake the player at the hottest hour: that one runs on to the next dusk or dawn.
        bool fromHeat = survival.CollapseCause == SurvivalManager.LossCause.Heat;
        float skipHours = fromHeat && heatCollapseWakesAtDuskOrDawn ? HoursToDuskOrDawn(time, plan.skipHours) : plan.skipHours;

        // The world carries on; the player's own stats do not.
        const float Step = 0.25f;
        for (float passed = 0f; passed < skipHours - 0.001f; passed += Step)
        {
            float step = Mathf.Min(Step, skipHours - passed);
            if (FireManager.Instance != null)
                FireManager.Instance.PassHours(step);
            if (WoodManager.Instance != null)
                WoodManager.Instance.PassHours(step);
            time.AdvanceMinutes(step * 60f);
        }

        survival.WakeFromCollapse(plan);
        player.Rest();

        string message = fromHeat ? "The heat overcame you. You woke hours later, weak." : "You collapsed and woke hours later, weak.";
        if (plan.count >= 2)
            message += $"\nYou have collapsed {plan.count} times recently.";
        if (dropped != null)
            message += $"\n{dropped} was left where you fell.";
        overlayText.text = message;
        yield return new WaitForSecondsRealtime(holdSeconds + 1.2f);
        yield return Fade(0f, message);

        if (openedMenu && game != null)
            game.CloseMenu();
        IsSleeping = false;
        collapsePending = false; // the Health set on waking is not a new collapse
    }

    // In-game hours from now until the first sunrise or sunset that is at least minHours away.
    static float HoursToDuskOrDawn(TimeManager time, float minHours)
    {
        float now = time.HourOfDay;
        float best = minHours + 24f;
        foreach (float hour in new[] { time.SunriseHour, time.SunsetHour })
        {
            for (int day = 0; day < 2; day++)
            {
                float ahead = Mathf.Repeat(hour - now, 24f) + day * 24f;
                if (ahead >= minHours && ahead < best)
                    best = ahead;
            }
        }
        return best;
    }

    // The nearest Tent, Lean-To or finished Small Cabin, at any distance. A Sleeping Bag, a Tarp Shelter and an unfinished
    // cabin site are not wake points.
    static WoodPileState NearestWakeShelter(Vector3 from)
    {
        WoodManager wood = WoodManager.Instance;
        if (wood == null)
            return null;

        WoodPileState best = null;
        float bestDistance = float.MaxValue;
        foreach (WoodPileState pile in wood.Piles)
        {
            if (pile.kind != PileKind.Tent && pile.kind != PileKind.LeanTo && pile.kind != PileKind.Cabin)
                continue;
            Vector3 d = pile.position - from;
            d.y = 0f;
            if (d.sqrMagnitude < bestDistance)
            {
                bestDistance = d.sqrMagnitude;
                best = pile;
            }
        }
        return best;
    }

    // Where the player wakes: on the floor of a Small Cabin, otherwise on clear ground beside a Tent or Lean-To (the
    // first of eight spots around it with room to stand), facing it.
    static void FindWakeSpot(WoodPileState shelter, out Vector3 spot, out float yaw)
    {
        yaw = shelter.yaw;
        spot = shelter.position + Vector3.up * (Shelter.FloorHeight + 0.3f);
        if (shelter.kind == PileKind.Cabin)
            return;

        float radius = shelter.kind == PileKind.Tent ? 1.9f : 2.1f;
        for (int i = 0; i < 8; i++)
        {
            Vector3 dir = Quaternion.Euler(0f, shelter.yaw + i * 45f, 0f) * Vector3.forward;
            Vector3 from = shelter.position + dir * radius + Vector3.up * 4f;
            if (!Physics.Raycast(from, Vector3.down, out RaycastHit hit, 10f, ~0, QueryTriggerInteraction.Ignore))
                continue;
            Vector3 feet = hit.point + Vector3.up * 0.05f;
            if (Physics.CheckCapsule(feet + Vector3.up * 0.4f, feet + Vector3.up * 1.5f, 0.3f, ~0, QueryTriggerInteraction.Ignore))
                continue;
            spot = feet;
            yaw = Mathf.Atan2(-dir.x, -dir.z) * Mathf.Rad2Deg;
            return;
        }
    }

    // 4th and later recent collapse: one unit of one randomly chosen kind of carried item, never the equipped tool, is left
    // on the ground where the player fell. Returns its name, or null when nothing qualifies.
    static string DropOneCarriedItem(Vector3 at, Transform player)
    {
        InventoryManager inventory = InventoryManager.Instance;
        if (inventory == null)
            return null;

        string equipped = inventory.EquippedTool != null ? inventory.EquippedTool.Id : null;
        var options = new List<string>();
        foreach (ItemStack stack in inventory.Player.Stacks)
            if (stack.quantity > 0 && stack.itemId != equipped && !options.Contains(stack.itemId))
                options.Add(stack.itemId);
        if (options.Count == 0)
            return null;

        string itemId = options[Random.Range(0, options.Count)];
        int day = -1;
        if (AudioManager.Instance != null)
            AudioManager.Instance.SilenceNextRemoval(); // not an item dropped by hand
        if (inventory.Player.RemoveBatches(itemId, 1, (batchDay, count) => day = batchDay) <= 0)
            return null;
        // The player's own collider isn't ground, and the probe starts just above the feet so a collapse under a roof (the
        // Small Cabin, a Tarp Shelter) lands on the floor, not on the roof overhead.
        DroppedItems.Place(itemId, 1, at, player, day, 0.4f);
        return $"1 {ItemDatabase.Get(itemId)?.DisplayName ?? itemId}";
    }

    IEnumerator Fade(float to, string text)
    {
        overlayText.text = text;
        overlay.gameObject.SetActive(true);
        float from = overlay.alpha;
        for (float t = 0f; t < fadeSeconds; t += Time.unscaledDeltaTime)
        {
            overlay.alpha = Mathf.Lerp(from, to, t / fadeSeconds);
            yield return null;
        }
        overlay.alpha = to;
        if (to <= 0f)
            overlay.gameObject.SetActive(false);
    }

    void BuildOverlay()
    {
        var root = new GameObject("Sleep Overlay", typeof(RectTransform));
        root.transform.SetParent(transform, false);
        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;
        var scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        overlay = root.AddComponent<CanvasGroup>();
        overlay.alpha = 0f;
        overlay.blocksRaycasts = true;

        var rt = (RectTransform)root.transform;
        Image black = UiKit.Image(rt, "Black", Color.black);
        black.rectTransform.Fill();
        overlayText = UiKit.Text(rt, "Text", "", 30, UiKit.Cream, TextAnchor.MiddleCenter);
        overlayText.rectTransform.Fill();
        root.SetActive(false);
    }
}
