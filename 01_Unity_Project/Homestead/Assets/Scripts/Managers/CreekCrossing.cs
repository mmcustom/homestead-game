using UnityEngine;

// What the creek does to the player this frame (PlayerController.Move applies it).
public struct CreekEffect
{
    public float speedFactor;   // multiplies walking, crouching and sprinting speed
    public Vector3 push;        // metres per second added to the move, along the flow
    public bool locked;         // knocked down: no input
    public static CreekEffect None => new CreekEffect { speedFactor = 1f };
}

// Water_System.md's Flash Floods phase 5, creek crossing danger. "Creek" is WaterSource.ZoneAt == null (the definition quality,
// fishing and traps use), so the 14 m Spring Hollow zone and the 32 m pond zone never count. The player is in the creek when
// over that water at ankle depth or deeper (the same raycast SurvivalManager.Submersion reads) and inside the channel
// (CreekFlow). While in the creek:
//   High water   slower and tiring (no push, no knock-down), one "running high" line per stay.
//   Flash flood  slower, very tiring, pushed downstream through CharacterController.Move; staying knockDownAfterSeconds knocks the
//                player down (input locked, carried downstream, a stamina hit and SurvivalManager.Injure, which has a health floor so
//                a flood can never collapse the player), then knockDownImmunitySeconds of immunity.
// Nothing is saved; the clocks reset on teleport, load and new game. Nothing happens while sleeping, collapsed, placing,
// or when the game is not in plain play. Creates itself at startup. Every number is a first pass (Claude, not yet confirmed).
public class CreekCrossing : MonoBehaviour
{
    public static CreekCrossing Instance { get; private set; }

    [Header("Where the creek counts")]
    [Tooltip("Water this deep at the feet (m) or deeper counts as being in the creek: the ankle figure.")]
    public float creekEffectMinDepthMetres = 0.15f;
    [Tooltip("How far past the channel's half-width (0.9 m upstream to 1.8 m downstream) still counts, in metres.")]
    public float creekEffectChannelMarginMetres = 1f;

    [Header("High water")]
    public float highWaterWadeSpeedFactor = 0.85f;
    [Tooltip("Extra stamina drain while in the creek, as a share of the sprint drain rate.")]
    public float highWaterWadeDrainShareOfSprint = 0.5f;

    [Header("Flash flood")]
    public float floodWadeSpeedFactor = 0.6f;
    public float floodWadeDrainShareOfSprint = 1.2f;
    [Tooltip("Steady push downstream while in the creek, in metres per second.")]
    public float floodPushMetresPerSecond = 1.5f;
    [Tooltip("Seconds in the water before the current knocks the player down.")]
    public float floodKnockDownAfterSeconds = 1.5f;
    public float floodLockoutSeconds = 1.5f;
    [Tooltip("How fast the current carries a knocked-down player downstream, in metres per second.")]
    public float floodLockoutCarryMetresPerSecond = 2.5f;
    [Tooltip("After getting up, seconds during which the player can't be knocked down again.")]
    public float floodImmunitySeconds = 4f;
    public float floodKnockDownStaminaLoss = 25f;
    [Tooltip("Health lost per knock-down. SurvivalManager's Injury Health Floor stops it going lower.")]
    public float floodKnockDownHealthLoss = 2f;

    [Header("Messages")]
    [Tooltip("The player counts as having left the creek after this long out of it, so a flicker at the bank doesn't repeat the warning.")]
    public float creekLeaveGraceSeconds = 1f;
    public float creekMessageSeconds = 3f;

    readonly CrossingTimer timer = new CrossingTimer();
    WaterSource water;
    float nextWaterSearch;

    // For the Numpad 9 overlay.
    bool debugInCreek;
    Vector2 debugFlow;
    float debugSpeedFactor = 1f;
    bool debugSuppressed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => Instance = null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Create()
    {
        if (Instance != null)
            return;
        var go = new GameObject("Creek Crossing");
        go.hideFlags = HideFlags.DontSave;
        DontDestroyOnLoad(go);
        go.AddComponent<CreekCrossing>();
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

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // --- Called by PlayerController ---

    public static CreekEffect Evaluate(PlayerController player, float dt) =>
        Instance != null ? Instance.Run(player, dt) : CreekEffect.None;

    // True while knocked down: tools and interactions wait too.
    public static bool InputLocked => Instance != null && Instance.timer.Locked;

    // Teleport (a load, a collapse's move), a new game or a load: nothing carries over.
    public static void ResetCrossing()
    {
        if (Instance != null)
            Instance.timer.Reset();
    }

    CrossingTiming Timing => new CrossingTiming
    {
        knockDownAfterSeconds = floodKnockDownAfterSeconds,
        lockoutSeconds = floodLockoutSeconds,
        immunitySeconds = floodImmunitySeconds,
        leaveGraceSeconds = creekLeaveGraceSeconds,
    };

    static bool Suppressed
    {
        get
        {
            GameManager game = GameManager.Instance;
            if (game != null && game.State != GameState.Playing)
                return true;
            SleepManager sleep = SleepManager.Instance;
            if (sleep != null && sleep.IsSleeping)
                return true;
            SurvivalManager survival = SurvivalManager.Instance;
            if (survival != null && survival.IsCollapsed)
                return true;
            return PlacementMode.Active; // placing never gets cancelled, and never gets pushed
        }
    }

    CreekEffect Run(PlayerController player, float dt)
    {
        if (player == null || Suppressed)
        {
            timer.Reset();
            debugInCreek = false;
            debugSuppressed = true;
            debugSpeedFactor = 1f;
            return CreekEffect.None;
        }
        debugSuppressed = false;

        StreamState stream = StreamManager.State;
        CrossingMode mode = stream == StreamState.FlashFlood ? CrossingMode.FlashFlood
                          : stream == StreamState.HighWater ? CrossingMode.HighWater : CrossingMode.None;

        bool inCreek = IsInCreek(player.transform.position, out Vector2 flow);
        CrossingTick tick = timer.Step(dt, mode, inCreek, Timing);

        if (tick.announce == CrossingMode.HighWater)
            ToolStatus.Flash("The creek is running high.", creekMessageSeconds);
        else if (tick.announce == CrossingMode.FlashFlood)
            ToolStatus.Flash("The water is too fast to cross.", creekMessageSeconds);

        if (tick.knockedDown)
        {
            player.LoseStamina(floodKnockDownStaminaLoss);
            if (SurvivalManager.Instance != null)
                SurvivalManager.Instance.Injure(floodKnockDownHealthLoss);
            ToolStatus.Flash("The current knocks you off your feet!", creekMessageSeconds);
        }

        CreekEffect effect = CreekEffect.None;
        effect.locked = timer.Locked;
        debugInCreek = inCreek;
        debugFlow = flow;

        if (inCreek && mode != CrossingMode.None)
        {
            bool flood = mode == CrossingMode.FlashFlood;
            effect.speedFactor = flood ? floodWadeSpeedFactor : highWaterWadeSpeedFactor;
            float share = flood ? floodWadeDrainShareOfSprint : highWaterWadeDrainShareOfSprint;
            player.LoseStamina(share * player.SprintStaminaPerSecond * dt);
            if (flood)
            {
                float speed = timer.Locked ? floodLockoutCarryMetresPerSecond : floodPushMetresPerSecond;
                effect.push = new Vector3(flow.x, 0f, flow.y) * speed;
            }
        }
        debugSpeedFactor = effect.speedFactor;
        return effect;
    }

    // Over creek water at ankle depth or deeper, inside the channel. The cheap channel test runs first; the water raycast and
    // the zone lookup only when the player is close to the creek.
    bool IsInCreek(Vector3 feet, out Vector2 flow)
    {
        flow = Vector2.zero;
        var at = new Vector2(feet.x, feet.z);
        if (!CreekFlow.InChannel(at, creekEffectChannelMarginMetres, out _))
            return false;

        float depth = Animal.IsOverWater(feet, out float surface) ? surface - feet.y : 0f;
        WaterSource source = Source();
        bool creekZone = source == null || StreamManager.IsCreekZone(source.ZoneAt(feet)); // no WaterSource: same fallback as FishWaterAt
        return CreekFlow.InCreek(at, depth, creekEffectMinDepthMetres, creekEffectChannelMarginMetres, creekZone, out flow);
    }

    WaterSource Source()
    {
        if (water == null && Time.unscaledTime >= nextWaterSearch)
        {
            nextWaterSearch = Time.unscaledTime + 1f;
            water = FindAnyObjectByType<WaterSource>();
        }
        return water;
    }

    // Numpad 9 overlay.
    public static string DebugText()
    {
        if (Instance == null)
            return "creek crossing off";
        CreekCrossing c = Instance;
        if (c.debugSuppressed)
            return "crossing paused";
        string flow = c.debugFlow.sqrMagnitude > 0f
            ? $"flow {Mathf.Atan2(c.debugFlow.x, c.debugFlow.y) * Mathf.Rad2Deg:0}° ({c.debugFlow.x:0.00}, {c.debugFlow.y:0.00})" : "flow -";
        string state = c.timer.Locked ? ", knocked down" : c.timer.Immune ? $", immune {c.timer.ImmuneLeft:0.0}s" : "";
        return $"in creek: {(c.debugInCreek ? "yes" : "no")}, {flow}, speed x{c.debugSpeedFactor:0.00}{state}";
    }
}
