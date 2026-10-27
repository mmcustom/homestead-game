#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;
using UnityEngine.InputSystem;

// Testing only: hotkeys to move the calendar forward, for reaching a season's content without hours of play
// (Season_System.md: 30-minute days, 28-day seasons). The whole file is compiled out of release builds, and it creates
// itself at startup, so nothing is added to any scene. Works only while playing (not paused, not in menus).
//
//   F3  toggle a live body readout (core temperature and its change per game hour, air, humidity, sun and sweat watts)
//   F4  start a Heat Wave (+6 C at its peak, 96 h; Shift+F4 a strong one, +12 C); pressed during a wave, ends it. Works in any
//       season: set the season to Summer first for the real temperatures, or the wave is added to Winter's cold
//   F2  jump a Heat Wave to its natural end now (not the F4 end): the forced Thunderstorm (70%) or Cold Front (30%) follows
//   F5  skip 1 day (Shift+F5: skip 10 days; each midnight crossed fires DayChanged, so land payments fall due and go late on the way)
//   F6  skip to the start of the next season
//   F7  cycle fast-forward: x1, x10, x60 (a day in 30s), x360 (a day in 5s) — calendar only, not movement
//   F8  skip to the start of Winter
//   F9  refill Hydration, Hunger, Health and Warmth, dry off and cure sickness (SurvivalManager)
//   F10 drop Hydration, Hunger and Warmth to 20, into the Severe tier, to check the warnings and Health loss
//   Shift+F10 set Health to 0, to test Player Collapse (SleepManager.CollapseRoutine)
//   F11 give the starting kit to older saves: Flint and Steel, a Bucket, the Axe, a Cooking Pot, a Sleeping Bag, a Tent,
//       a Knife, a Canteen, a Weather Station and 6 Firewood
//   End    export the ledger to a CSV file, one row per season (Money_System.md Phase D1), under Exports/ in the persistent data path;
//          the full path is printed in the Console and the file name shows on screen
//   PageUp add $1,000 cash per press (Shift+PageUp: set cash to $0, to test being broke, missed payments and the loss). Cash only, no
//          ledger entry: it is never income and shows only as the statement's "Other" line. F11 was already taken by the starting kit
//   Insert lose the game now, as if the third missed land payment had just landed (shows the loss screen; nothing is saved from it)
//   Home   toggle "sap always runs": every hour is a sap flow hour at every tap, whatever the season or the weather (TapManager). Shift+Home
//          fills every Sap Bucket to the top with fresh sap
//   PageDown teleport to the nearest Sugar Maple, standing about 2 m from the trunk and facing it. Press again for the next nearest, Shift+PageDown
//          for the one before (the order is fixed by where you stood at the first press); the message gives the distance and the new position
//   F12 give a food-gathering kit: Fishing Rod, Cane Pole, Recurve Bow + 20 arrows, Rifle + 10 rounds, 2 Rabbit Snares,
//       a Box Trap, a Fish Trap, 3 Cordage and 3 Wild Apples for bait — and $50 for the Trading Post
public class TimeDebugControls : MonoBehaviour
{
    static readonly float[] Speeds = { 1f, 10f, 60f, 360f };
    const float MessageSeconds = 2.5f;

    int speedIndex;
    bool showBody;
    double lastHours = -1.0;
    float lastCore, coreRate;
    string message;
    float messageUntil;
    GUIStyle style;
    System.Collections.Generic.List<int> mapleOrder; // standing Maples, nearest first from where PageDown was first pressed
    int mapleAt;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Create()
    {
        var go = new GameObject("Time Debug Controls");
        go.hideFlags = HideFlags.DontSave;
        DontDestroyOnLoad(go);
        go.AddComponent<TimeDebugControls>();
    }

    void Update()
    {
        TimeManager time = TimeManager.Instance;
        GameManager game = GameManager.Instance;
        Keyboard keyboard = Keyboard.current;
        bool playing = game != null && game.State == GameState.Playing;

        // Leaving the game (menu, loading) drops back to normal speed; pausing keeps it.
        if (time != null && speedIndex != 0 && (game == null || !game.InGame))
            SetSpeed(time, 0);
        if (game == null || !game.InGame)
            mapleOrder = null; // a new world has different trees

        if (time == null || keyboard == null || !playing)
            return;

        UpdateBodyReadout(time, keyboard);

        if (keyboard.homeKey.wasPressedThisFrame && TapManager.Instance != null)
        {
            if (keyboard.shiftKey.isPressed)
                Show($"Filled {TapManager.Instance.DebugFillBuckets()} sap buckets", time);
            else
            {
                TapManager.DebugForceFlow = !TapManager.DebugForceFlow;
                Show(TapManager.DebugForceFlow ? "Sap always runs: ON" : "Sap always runs: OFF", time);
            }
        }

        if (keyboard.pageDownKey.wasPressedThisFrame)
        {
            Show(TeleportToMaple(keyboard.shiftKey.isPressed ? -1 : 1), time);
            return;
        }

        if (keyboard.insertKey.wasPressedThisFrame && MoneyManager.Instance != null)
        {
            MoneyManager.Instance.DebugForceLoss();
            Show("Loss forced", time);
            return;
        }

        if (keyboard.pageUpKey.wasPressedThisFrame && TradingPost.Instance != null)
        {
            TradingPost post = TradingPost.Instance;
            if (keyboard.shiftKey.isPressed)
            {
                post.ResetMoney(DebugCash.ZeroCents);
                Show(DebugCash.ZeroedMessage, time);
            }
            else
            {
                post.ResetMoney(DebugCash.After(post.Cents));
                Show(DebugCash.AddedMessage(post.Cents), time);
            }
            return;
        }

        if (keyboard.endKey.wasPressedThisFrame && LedgerManager.Instance != null)
        {
            string path = LedgerManager.Instance.DebugExportCsv();
            Show(path != null ? "Ledger exported: " + System.IO.Path.GetFileName(path) : "No ledger to export yet", time);
            return;
        }

        if (keyboard.f4Key.wasPressedThisFrame && WeatherManager.Instance != null)
        {
            WeatherManager weather = WeatherManager.Instance;
            if (weather.IsHeatWave)
            {
                weather.DebugEndHeatWave();
                Show("Heat Wave ended", time);
            }
            else
            {
                bool strong = keyboard.shiftKey.isPressed;
                weather.DebugStartHeatWave(strong ? 12f : 6f, 96);
                Show(strong ? "Strong Heat Wave started (+12 C peak, 96 h)" : "Heat Wave started (+6 C peak, 96 h)", time);
            }
        }
        else if (keyboard.f2Key.wasPressedThisFrame && WeatherManager.Instance != null)
        {
            bool ended = WeatherManager.Instance.DebugExpireHeatWave();
            Show(ended ? $"Heat Wave ran its course: {WeatherManager.Instance.Current}" : "No Heat Wave to end", time);
        }
        else if (keyboard.f5Key.wasPressedThisFrame)
        {
            int days = keyboard.shiftKey.isPressed ? 10 : 1; // Shift+F5: 10 days at once, to reach a loan due date or late date
            time.DebugSkipDays(days);
            Show(days == 1 ? "Skipped 1 day" : $"Skipped {days} days", time);
        }
        else if (keyboard.f6Key.wasPressedThisFrame)
        {
            time.DebugSkipToSeason((Season)(((int)time.CurrentSeason + 1) % 4));
            Show("Skipped to next season", time);
        }
        else if (keyboard.f7Key.wasPressedThisFrame)
        {
            SetSpeed(time, (speedIndex + 1) % Speeds.Length);
            Show($"Time speed x{Speeds[speedIndex]:0}", time);
        }
        else if (keyboard.f8Key.wasPressedThisFrame)
        {
            time.DebugSkipToSeason(Season.Winter);
            Show("Skipped to Winter", time);
        }
        else if (keyboard.f9Key.wasPressedThisFrame && SurvivalManager.Instance != null)
        {
            SurvivalManager.Instance.DebugSet(100f, 100f, 100f);
            SurvivalManager.Instance.Cure();
            SurvivalManager.Instance.DebugSetWarmth(100f, 0f);
            Show("Survival stats refilled, warmed and dried, sickness cured", time);
        }
        else if (keyboard.f10Key.wasPressedThisFrame && keyboard.shiftKey.isPressed && SurvivalManager.Instance != null)
        {
            SurvivalManager.Instance.DebugSet(SurvivalManager.Instance.Hydration, SurvivalManager.Instance.Hunger, 0f);
            Show("Health set to 0 — collapse", time);
        }
        else if (keyboard.f10Key.wasPressedThisFrame && SurvivalManager.Instance != null)
        {
            SurvivalManager.Instance.DebugSet(20f, 20f, SurvivalManager.Instance.Health);
            SurvivalManager.Instance.DebugSetWarmth(20f, SurvivalManager.Instance.Wetness);
            Show("Hydration, Hunger and Warmth set to 20", time);
        }
        else if (keyboard.f11Key.wasPressedThisFrame && InventoryManager.Instance != null)
        {
            InventoryManager inventory = InventoryManager.Instance;
            if (!inventory.Player.Has(FireManager.IgnitionId))
                inventory.AddToPlayer(FireManager.IgnitionId, 1);
            if (!inventory.Player.Has(WaterSource.BucketItemId))
                inventory.AddToPlayer(WaterSource.BucketItemId, 1);
            if (!inventory.Player.Has(AxeTool.AxeId))
                inventory.AddToPlayer(AxeTool.AxeId, 1);
            if (!inventory.Player.Has(Cooking.PotId))
                inventory.AddToPlayer(Cooking.PotId, 1);
            if (!inventory.Player.Has(SleepManager.SleepingBagId))
                inventory.AddToPlayer(SleepManager.SleepingBagId, 1);
            if (!inventory.Player.Has(WoodManager.TentId))
                inventory.AddToPlayer(WoodManager.TentId, 1);
            if (!inventory.Player.Has(Knife.Id))
                inventory.AddToPlayer(Knife.Id, 1);
            if (!inventory.Player.Has(WaterQualities.CanteenId))
                inventory.AddToPlayer(WaterQualities.CanteenId, 1);
            if (!inventory.Player.Has(WeatherScreen.WeatherStationId))
                inventory.AddToPlayer(WeatherScreen.WeatherStationId, 1);
            int firewood = inventory.AddToPlayer(FireManager.FirewoodId, 6);
            Show($"Fire, water, wood and weather kit given ({firewood} Firewood fit)", time);
        }
        else if (keyboard.f12Key.wasPressedThisFrame && InventoryManager.Instance != null)
        {
            InventoryManager inventory = InventoryManager.Instance;
            (string id, int count)[] kit =
            {
                ("fishing_rod", 1), ("cane_pole", 1), ("recurve_bow", 1), ("arrows", 20), ("bolt_action_rifle", 1),
                ("rifle_rounds", 10), ("rabbit_snare", 2), ("box_trap", 1), ("fish_trap", 1), ("cordage", 3), ("wild_apples", 3),
            };
            int missed = 0;
            foreach ((string id, int count) in kit)
                missed += count - inventory.AddToPlayer(id, count);
            if (TradingPost.Instance != null)
                TradingPost.Instance.DebugAddMoney(50);
            Show(missed == 0 ? "Food-gathering kit and $50 given" : $"Food-gathering kit and $50 given ({missed} items didn't fit)", time);
        }
    }

    // Core temperature's change per game hour, smoothed, so the rate of heat gain can be read off while testing.
    void UpdateBodyReadout(TimeManager time, Keyboard keyboard)
    {
        if (keyboard.f3Key.wasPressedThisFrame)
            showBody = !showBody;

        SurvivalManager survival = SurvivalManager.Instance;
        if (survival == null)
            return;

        double hours = time.TotalHours;
        float core = survival.CoreTemperatureC;
        if (lastHours < 0.0 || hours < lastHours)
        {
            lastHours = hours;
            lastCore = core;
            coreRate = 0f;
        }
        else if (hours - lastHours > 0.02)
        {
            coreRate = Mathf.Lerp(coreRate, (core - lastCore) / (float)(hours - lastHours), 0.3f);
            lastHours = hours;
            lastCore = core;
        }
    }

    string BodyText()
    {
        SurvivalManager survival = SurvivalManager.Instance;
        WeatherManager weather = WeatherManager.Instance;
        if (survival == null)
            return "";
        string text = $"Core {survival.CoreTemperatureC:0.00} C ({coreRate:+0.00;-0.00;0.00} C/h)  {survival.HeatLevel}  Warmth {survival.Warmth:0}";
        if (weather != null)
            text += $"\nAir {weather.TemperatureC:0.0} C ({weather.TemperatureF:0} F)  RH {weather.RelativeHumidity:0}%  dew {weather.DewPointC:0.0} C  wind {weather.WindStrength:0.00}  difficulty shift {weather.DifficultyShiftC:+0.0;-0.0;0.0} C ({DifficultyManager.Tier})";
        text += $"\nSun {survival.SolarWatts:0} W  sweat {survival.SweatWatts:0} W (evap {survival.SweatEvaporation:0.00})  wet {survival.Wetness:0.00}";
        if (survival.WadeShare > 0f || survival.WaterCoolingWatts > 0f)
            text += $"\nWading {survival.WadeShare:0.00}  water {survival.WaterTakenWatts:0} W taken = {survival.WaterCoolingWatts:0} W raw x taper {survival.WaterTaper:0.00}";
        return text;
    }

    void SetSpeed(TimeManager time, int index)
    {
        speedIndex = index;
        time.DebugSpeed = Speeds[index];
    }

    // PageDown: stands the player about 2 m from the trunk of the next Maple (step 1) or the one before (step -1), on the side they came from,
    // looking at it. Teleport lifts nothing and keeps no fall speed, so the player is set just above the terrain and settles under gravity.
    string TeleportToMaple(int step)
    {
        const float StandOffMetres = 2f;
        const float LookDownDegrees = 10f;

        WoodManager wood = WoodManager.Instance;
        PlayerController player = FindAnyObjectByType<PlayerController>();
        Terrain terrain = Terrain.activeTerrain;
        if (wood == null || player == null || terrain == null || !wood.TreesReady)
            return "No trees to go to yet";

        Vector3 from = player.transform.position;
        if (mapleOrder == null)
        {
            mapleOrder = new System.Collections.Generic.List<int>();
            for (int i = 0; i < wood.TreeCount; i++)
                if (wood.IsStanding(i) && MapleRules.IsMaple(i, wood.Prototype(i), wood.HeightScale(i)))
                    mapleOrder.Add(i);
            mapleOrder.Sort((a, b) => FlatDistance(wood.WorldPosition(a), from).CompareTo(FlatDistance(wood.WorldPosition(b), from)));
            mapleAt = step > 0 ? -1 : 1; // the first press always goes to the nearest
        }
        if (mapleOrder.Count == 0)
            return "No Maples on this map";

        int count = mapleOrder.Count;
        int tree = -1;
        for (int tries = 0; tries < count && tree < 0; tries++)
        {
            mapleAt = ((mapleAt + step) % count + count) % count;
            if (wood.IsStanding(mapleOrder[mapleAt]))
                tree = mapleOrder[mapleAt]; // one felled since the list was made is skipped
        }
        if (tree < 0)
            return "No standing Maples left";

        Vector3 trunk = wood.WorldPosition(tree);
        Vector3 away = new Vector3(from.x - trunk.x, 0f, from.z - trunk.z);
        away = away.sqrMagnitude > 0.01f ? away.normalized : Vector3.back;
        float reach = wood.TrunkRadius(tree) + StandOffMetres;

        // The side the player came from first; if something solid is there, the next side round.
        Vector3 stand = trunk;
        for (int k = 0; k < 8; k++)
        {
            Vector3 dir = Quaternion.Euler(0f, k * 45f, 0f) * away;
            Vector3 spot = trunk + dir * reach;
            spot.y = terrain.SampleHeight(spot) + terrain.transform.position.y + 0.05f;
            if (k == 0)
                stand = spot;
            if (!Physics.CheckSphere(spot + Vector3.up * 1f, 0.4f, ~0, QueryTriggerInteraction.Ignore))
            {
                stand = spot;
                break;
            }
        }

        Vector3 toTrunk = trunk - stand;
        float yaw = Mathf.Atan2(toTrunk.x, toTrunk.z) * Mathf.Rad2Deg;
        player.Teleport(stand, yaw, LookDownDegrees);
        return $"Maple {mapleAt + 1} of {count}: {Vector3.Distance(from, stand):0} m from where you were, now at ({stand.x:0}, {stand.y:0}, {stand.z:0})";
    }

    static float FlatDistance(Vector3 a, Vector3 b) => new Vector2(a.x - b.x, a.z - b.z).magnitude;

    void Show(string text, TimeManager time)
    {
        message = $"{text} — {time.FormatDate(time.TotalDays)} {time.Hour:00}:{time.Minute:00}";
        messageUntil = Time.unscaledTime + MessageSeconds;
        Debug.Log("[TimeDebug] " + message);
    }

    void OnGUI()
    {
        TimeManager time = TimeManager.Instance;
        bool fast = time != null && speedIndex != 0;
        bool recent = Time.unscaledTime < messageUntil;
        style ??= new GUIStyle(GUI.skin.label) { fontSize = 18, normal = { textColor = Color.yellow } };
        if (showBody && GameManager.Instance != null && GameManager.Instance.InGame)
            GUI.Label(new Rect(12f, 84f, 1100f, 100f), BodyText(), style);
        if (!fast && !recent)
            return;

        style ??= new GUIStyle(GUI.skin.label) { fontSize = 18, normal = { textColor = Color.yellow } };
        string text = recent ? message : "";
        if (fast)
            text = $"DEBUG time x{Speeds[speedIndex]:0} — {time.FormatDate(time.TotalDays)} {time.Hour:00}:{time.Minute:00}" +
                   (recent ? "\n" + message : "");
        GUI.Label(new Rect(12f, 12f, 900f, 60f), text, style);
    }
}
#endif
