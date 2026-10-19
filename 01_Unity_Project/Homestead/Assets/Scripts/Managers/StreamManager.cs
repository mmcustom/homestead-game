using System;
using UnityEngine;

public enum StreamState { Normal, HighWater, FlashFlood }

[Serializable]
public struct StreamSaveData
{
    public float stageMm;
    public float creekMurkHours;
}

// Water_System.md's "Flash Floods, Rising Creek and Pond Overflow", phases 1 to 4. One number, the Stream Stage, is the creek's
// runoff in millimetres: it gains each game hour of rain (the weather's own Light, Heavy and Thunderstorm rates) and drains
// with a time constant of about six hours (a leaky bucket, integrated exactly one hour at a time). It reads:
//   Normal       below highWaterAtMm
//   High water   from highWaterAtMm
//   Flash flood  from flashFloodAtMm
// "Creek" everywhere means a WaterSource zone of null (the default water): the 14 m Spring Hollow zone and the 32 m pond zone
// are excluded. A flash flood leaves the creek murky (one tier worse) for creekMurkHoursAfterFlood hours; High water is one tier
// worse on its own; a flash flood is Unsafe. WaterSource.QualityAt adds these to Pond Drying's shift and caps at Unsafe.
//
// Driven by the weather's HourEnded, which Sleep, collapse and day skips cross one hour at a time. This manager is the single
// subscriber for the rain hour: it gives the hour to PondManager first (rain gain), updates its own stage, and then tells
// PondManager the new state (overflow), so the order never depends on event order. Creates itself at startup.
// Save: stage and murk hours, key "stream"; a save without them loads calm.
public class StreamManager : MonoBehaviour, ISaveable
{
    public static StreamManager Instance { get; private set; }

    // Raised the moment the stage rises into Flash flood (never on a load). Fish Traps listen to this.
    public static event Action FlashFloodStarted;

    [Header("Stream Stage (millimetres of runoff)")]
    [Tooltip("The creek is at High water from this stage.")]
    public float highWaterAtMm = 15f;
    [Tooltip("A flash flood from this stage.")]
    public float flashFloodAtMm = 25f;
    [Tooltip("The stage drains with this time constant, in game hours.")]
    public float drainTimeConstantHours = 6f;

    [Header("After a flood")]
    [Tooltip("The creek stays one tier worse for this many game hours after a flash flood.")]
    public float creekMurkHoursAfterFlood = 12f;

    [Header("Creek water quality (tiers worse)")]
    public int creekTiersWorseHighWater = 1;
    [Tooltip("Murk after a flood: one tier worse. It does not add to High water; the larger of the two applies.")]
    public int creekTiersWorseMurk = 1;
    [Tooltip("3 is far enough to reach Unsafe from any quality.")]
    public int creekTiersWorseFlashFlood = 3;

    [Header("Messages")]
    public float risingToastSeconds = 4f;
    public float floodBannerSeconds = 8f;
    [Tooltip("The wake-up summary shows this long after waking, so it doesn't replace the wake message.")]
    public float wakeSummaryDelaySeconds = 4.5f;

    float stage;
    float creekMurkHours;

    bool linked;
    WeatherManager linkedWeather;

    bool wasAway;
    StreamState awayPeak;
    int awaySwept;
    float summaryAt = -1f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        Instance = null;
        FlashFloodStarted = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Create()
    {
        if (Instance != null)
            return;
        var go = new GameObject("Stream Manager");
        go.hideFlags = HideFlags.DontSave;
        DontDestroyOnLoad(go);
        go.AddComponent<StreamManager>();
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
    }

    void Update()
    {
        if (!linked)
            Link();
        TrackAway();
    }

    void Link()
    {
        if (Instance != this || WeatherManager.Instance == null || SaveManager.Instance == null)
            return;

        linkedWeather = WeatherManager.Instance;
        linkedWeather.HourEnded += OnHourEnded;
        SaveManager.Instance.Register(this);
        linked = true;
    }

    void OnDestroy()
    {
        if (Instance != this)
            return;

        if (linked)
        {
            if (linkedWeather != null)
                linkedWeather.HourEnded -= OnHourEnded;
            if (SaveManager.Instance != null)
                SaveManager.Instance.Unregister(this);
        }
        Instance = null;
    }

    // --- The stage ---

    public float StageMm => stage;

    public StreamState StateOfStage(float mm) =>
        mm >= flashFloodAtMm ? StreamState.FlashFlood : mm >= highWaterAtMm ? StreamState.HighWater : StreamState.Normal;

    public static StreamState State => Instance != null ? Instance.StateOfStage(Instance.stage) : StreamState.Normal;

    void OnHourEnded(WeatherType precipitation)
    {
        PondManager ponds = PondManager.Instance;
        if (ponds != null)
            ponds.HourPassed(precipitation); // Pond Drying's rain gain, first

        StepHour(linkedWeather != null ? linkedWeather.RainMmPerHour(precipitation) : 0f);

        if (ponds != null)
            ponds.ApplyOverflow(State); // then the overflow check, on the new state and the new pond level
    }

    // One game hour of rain at mmPerHour. Exact for a constant rate over the hour: S' = S e^(-1/T) + rT (1 - e^(-1/T)).
    public void StepHour(float mmPerHour)
    {
        StreamState before = StateOfStage(stage);
        float t = Mathf.Max(0.01f, drainTimeConstantHours);
        float k = Mathf.Exp(-1f / t);
        stage = Mathf.Max(0f, stage * k + Mathf.Max(0f, mmPerHour) * t * (1f - k));
        Settle(before);
    }

    // Murk follows the new state; a change of state is announced.
    void Settle(StreamState before)
    {
        StreamState after = StateOfStage(stage);
        creekMurkHours = after == StreamState.FlashFlood ? creekMurkHoursAfterFlood : Mathf.Max(0f, creekMurkHours - 1f);
        if (after != before)
            StateChanged(before, after);
    }

    void StateChanged(StreamState before, StreamState after)
    {
        if (after <= before)
            return; // only a rise is announced

        awayPeak = (StreamState)Mathf.Max((int)awayPeak, (int)after);
        if (!Suppressed)
        {
            if (after == StreamState.FlashFlood)
                ToolStatus.Banner("The creek is in flood.", floodBannerSeconds);
            else
                ToolStatus.Flash("The creek is rising.", risingToastSeconds);
        }
        if (after == StreamState.FlashFlood)
            FlashFloodStarted?.Invoke();
    }

    // "Flash Flood Watch": a Heavy Rain or Thunderstorm is falling, the creek is below a flood, and the rain is still adding
    // faster than the stage drains (rate x time constant is where the stage would level off).
    public bool FlashFloodWatch
    {
        get
        {
            WeatherManager weather = WeatherManager.Instance;
            if (weather == null || StateOfStage(stage) == StreamState.FlashFlood)
                return false;
            WeatherType precipitation = weather.PrecipitationWeather;
            if (precipitation != WeatherType.HeavyRain && precipitation != WeatherType.Thunderstorm)
                return false;
            return weather.RainMmPerHour(precipitation) * drainTimeConstantHours > stage;
        }
    }

    // --- Creek water (zone null) ---

    public static bool IsCreekZone(string zone) => zone == null;

    public float CreekMurkHours => creekMurkHours;

    // Tiers worse the creek's water is right now: Unsafe in a flash flood, otherwise the larger of High water and murk.
    public int CreekTiersWorse()
    {
        StreamState state = StateOfStage(stage);
        if (state == StreamState.FlashFlood)
            return creekTiersWorseFlashFlood;
        int tiers = state == StreamState.HighWater ? creekTiersWorseHighWater : 0;
        if (creekMurkHours > 0f)
            tiers = Mathf.Max(tiers, creekTiersWorseMurk);
        return tiers;
    }

    // What WaterSource.QualityAt adds for a zone: nothing for a pond, Spring Hollow or anything with a name.
    public static int TiersWorse(string zone) =>
        Instance != null && IsCreekZone(zone) ? Instance.CreekTiersWorse() : 0;

    // "Creek water (high)" or "(flooding)" for the interaction prompt; nothing at Normal and away from the creek.
    public static string PromptNote(string zone)
    {
        if (Instance == null || !IsCreekZone(zone))
            return "";
        switch (State)
        {
            case StreamState.HighWater: return "Creek water (high)";
            case StreamState.FlashFlood: return "Creek water (flooding)";
            default: return "";
        }
    }

    // For the Weather screen: "Normal", "High water" or "Flash flood", with the Watch line appended when it applies.
    public static string CreekRowText()
    {
        if (Instance == null)
            return "—";
        string text;
        switch (State)
        {
            case StreamState.HighWater: text = "High water"; break;
            case StreamState.FlashFlood: text = "Flash flood"; break;
            default: text = "Normal"; break;
        }
        return Instance.FlashFloodWatch ? text + " — Flash Flood Watch" : text;
    }

    // --- Messages while the player can't see them ---

    // Not shown while asleep or collapsed, or when the game is not in plain play (a load, a menu, a pause).
    static bool Suppressed
    {
        get
        {
            SleepManager sleep = SleepManager.Instance;
            if (sleep != null && sleep.IsSleeping)
                return true;
            GameManager game = GameManager.Instance;
            return game != null && game.State != GameState.Playing;
        }
    }

    // Fish Traps tell us what a flood swept away so the message can wait for the wake-up.
    public static void ReportSweptTraps(int count)
    {
        if (count <= 0)
            return;
        if (Instance != null && Suppressed)
            Instance.awaySwept += count;
        else
            ToolStatus.Flash(count == 1 ? "A flood swept away a Fish Trap." : $"A flood swept away {count} Fish Traps.", 5f);
    }

    // After a sleep or collapse that saw the creek rise, one line a moment after the wake message.
    void TrackAway()
    {
        if (Suppressed)
        {
            wasAway = true;
            return;
        }

        if (wasAway)
        {
            wasAway = false;
            if (awayPeak > StreamState.Normal || awaySwept > 0)
                summaryAt = Time.unscaledTime + wakeSummaryDelaySeconds;
        }

        if (summaryAt >= 0f && Time.unscaledTime >= summaryAt)
        {
            summaryAt = -1f;
            string text = awayPeak == StreamState.FlashFlood ? "While you slept the creek flooded."
                        : awayPeak == StreamState.HighWater ? "While you slept the creek rose." : "";
            if (awaySwept > 0)
                text += (text.Length > 0 ? " " : "") + (awaySwept == 1 ? "A flood swept away a Fish Trap." : $"A flood swept away {awaySwept} Fish Traps.");
            if (text.Length > 0)
                ToolStatus.Flash(text, 6f);
            awayPeak = StreamState.Normal;
            awaySwept = 0;
        }
    }

    void ClearAway()
    {
        wasAway = false;
        awayPeak = StreamState.Normal;
        awaySwept = 0;
        summaryAt = -1f;
    }

    // --- Reset, debug, save ---

    // Calm: used for a new game and before every load.
    public void ResetStream()
    {
        stage = 0f;
        creekMurkHours = 0f;
        ClearAway();
    }

    // Testing: set the stage straight to a value; a rise is announced and a rise into a flash flood fires FlashFloodStarted, as an hour would.
    public void DebugSetStage(float mm)
    {
        StreamState before = StateOfStage(stage);
        stage = Mathf.Max(0f, mm);
        Settle(before);
    }

    public string DebugText() =>
        $"creek {StateOfStage(stage)} {stage:0.#} mm, murk {creekMurkHours:0} h" + (FlashFloodWatch ? ", watch" : "");

    string ISaveable.SaveFile => "world";
    string ISaveable.SaveKey => "stream";

    object ISaveable.CaptureState() => new StreamSaveData { stageMm = stage, creekMurkHours = creekMurkHours };

    // Restores silently: no toast, no flood event, so a trap is never swept by a load.
    void ISaveable.RestoreState(string json)
    {
        StreamSaveData data = JsonUtility.FromJson<StreamSaveData>(json);
        stage = Mathf.Max(0f, data.stageMm);
        creekMurkHours = Mathf.Max(0f, data.creekMurkHours);
        ClearAway();
    }
}
