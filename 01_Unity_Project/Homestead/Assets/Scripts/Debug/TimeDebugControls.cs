#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;
using UnityEngine.InputSystem;

// Testing only: hotkeys to move the calendar forward, for reaching a season's content without hours of play
// (Season_System.md: 30-minute days, 28-day seasons). The whole file is compiled out of release builds, and it creates
// itself at startup, so nothing is added to any scene. Works only while playing (not paused, not in menus).
//
//   F5  skip 1 day
//   F6  skip to the start of the next season
//   F7  cycle fast-forward: x1, x10, x60 (a day in 30s), x360 (a day in 5s) — calendar only, not movement
//   F8  skip to the start of Winter
//   F9  refill Hydration, Hunger and Health (SurvivalManager)
//   F10 drop Hydration and Hunger to 20, into the Severe tier, to check the warnings and Health loss
//   F11 give a fire and water kit: Flint and Steel, a Bucket and 6 Firewood (for saves made before the starting kit)
//   F12 give a food-gathering kit: Fishing Rod, Cane Pole, Recurve Bow + 20 arrows, Rifle + 10 rounds, 2 Rabbit Snares,
//       a Box Trap, a Fish Trap, 3 Cordage and 3 Wild Apples for bait
public class TimeDebugControls : MonoBehaviour
{
    static readonly float[] Speeds = { 1f, 10f, 60f, 360f };
    const float MessageSeconds = 2.5f;

    int speedIndex;
    string message;
    float messageUntil;
    GUIStyle style;

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

        if (time == null || keyboard == null || !playing)
            return;

        if (keyboard.f5Key.wasPressedThisFrame)
        {
            time.DebugSkipDays(1);
            Show("Skipped 1 day", time);
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
            Show("Survival stats refilled", time);
        }
        else if (keyboard.f10Key.wasPressedThisFrame && SurvivalManager.Instance != null)
        {
            SurvivalManager.Instance.DebugSet(20f, 20f, SurvivalManager.Instance.Health);
            Show("Hydration and Hunger set to 20", time);
        }
        else if (keyboard.f11Key.wasPressedThisFrame && InventoryManager.Instance != null)
        {
            InventoryManager inventory = InventoryManager.Instance;
            if (!inventory.Player.Has(FireManager.IgnitionId))
                inventory.AddToPlayer(FireManager.IgnitionId, 1);
            if (!inventory.Player.Has(WaterSource.BucketItemId))
                inventory.AddToPlayer(WaterSource.BucketItemId, 1);
            int firewood = inventory.AddToPlayer(FireManager.FirewoodId, 6);
            Show($"Fire and water kit given ({firewood} Firewood fit)", time);
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
            Show(missed == 0 ? "Food-gathering kit given" : $"Food-gathering kit given ({missed} items didn't fit)", time);
        }
    }

    void SetSpeed(TimeManager time, int index)
    {
        speedIndex = index;
        time.DebugSpeed = Speeds[index];
    }

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
