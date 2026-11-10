using System;
using UnityEngine;

// Player time control (real-calendar Phase 2a): Wait, and PassWorld, the one loop that passes the world's hours for both Wait and sleep (SleepManager.PassNight).
// The rules (what stops a Wait, when one cannot start, the step order) are TimeControlRules and WorldPass, pure and tested; this reads the real game into a WorldWatch and
// moves the real managers. WaitPanel (key Z) is the menu in front of it. F7's debug speed is not touched: it only scales the clock's own Update and never comes through here.
//
// A Wait runs in one frame, like sleep's loop, with the game left in plain play so the creek's rising toast, a loan banner or a trap's catch show as they happen; there is no
// fade (the player is awake, standing where they were). The caller closes the Wait menu first. One line of HUD text says how long it ran and, if it stopped early, why.
public class TimeControlManager : MonoBehaviour
{
    public static TimeControlManager Instance { get; private set; }

    // Nothing in the game is hostile yet (every animal flees), so nothing sets this; a future predator sets it while one is close and Wait refuses to start or stops.
    public static Func<bool> DangerSource;

    public bool IsWaiting { get; private set; }

    PlayerController player;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        Instance = null;
        DangerSource = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Create()
    {
        if (Instance != null)
            return;
        var go = new GameObject("Time Control");
        DontDestroyOnLoad(go);
        go.AddComponent<TimeControlManager>();
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

    // --- The shared loop ---

    // The real game behind WorldPass: survival, fires, wood, portable-light fuel, then the clock. Missing managers (a scene without them) are skipped.
    sealed class RealWorld : IWorldPass
    {
        public void Survival(float hours)
        {
            if (SurvivalManager.Instance != null)
                SurvivalManager.Instance.PassHours(hours);
        }

        public void Fire(float hours)
        {
            if (FireManager.Instance != null)
                FireManager.Instance.PassHours(hours);
        }

        public void Wood(float hours)
        {
            if (WoodManager.Instance != null)
                WoodManager.Instance.PassHours(hours);
        }

        public void Light(float hours)
        {
            if (PortableLight.Instance != null)
                PortableLight.Instance.PassHours(hours);
        }

        public void Clock(float hours)
        {
            if (TimeManager.Instance != null)
                TimeManager.Instance.AdvanceMinutes(hours * 60f);
        }
    }

    // Passes `hours` of game time in 0.25-hour steps (WorldPass.Run); afterStep(hoursSoFar) returns true to stop. Returns the hours passed. Used by sleep and by Wait.
    public static float PassWorld(float hours, bool tickPlayer, Func<float, bool> afterStep)
    {
        if (TimeManager.Instance == null)
            return 0f;
        return WorldPass.Run(hours, new RealWorld(), tickPlayer, afterStep);
    }

    // --- Reading the game ---

    public WorldWatch Read()
    {
        var w = new WorldWatch();
        SurvivalManager survival = SurvivalManager.Instance;
        if (survival != null)
        {
            w.hydration = survival.Hydration;
            w.hunger = survival.Hunger;
            w.warmth = survival.Warmth;
            w.health = survival.Health;
            w.coreC = survival.CoreTemperatureC;
            w.collapsed = survival.IsCollapsed;
        }
        else
        {
            w.hydration = w.hunger = w.warmth = w.health = 100f;
            w.coreC = 37f;
        }

        WeatherManager weather = WeatherManager.Instance;
        if (weather != null && weather.IsSevere)
        {
            w.severeWeather = (int)weather.Current + 1;
            w.severeName = weather.Current == WeatherType.Thunderstorm ? "a thunderstorm"
                         : weather.Current == WeatherType.ColdFront ? "a cold front" : "snow";
        }

        w.streamState = (int)StreamManager.State;

        w.lowestFireFuelHours = float.MaxValue;
        PlayerController p = Player();
        FireManager fires = FireManager.Instance;
        if (fires != null && p != null)
        {
            float reach = TimeControlRules.FireWatchMetres * TimeControlRules.FireWatchMetres;
            foreach (CampfireState fire in fires.Fires)
            {
                if (!fire.lit || (fire.position - p.transform.position).sqrMagnitude > reach)
                    continue;
                w.litFiresNear++;
                w.lowestFireFuelHours = Mathf.Min(w.lowestFireFuelHours, fire.fuelHours);
            }
        }

        TrapManager traps = TrapManager.Instance;
        if (traps != null)
            foreach (TrapState trap in traps.Traps)
                if (trap.result == TrapResult.SuccessfulCatch || trap.result == TrapResult.SmallCatch)
                    w.trapCatches++;

        w.notices = ToolStatus.BannersRaised;
        w.danger = DangerSource != null && DangerSource();
        return w;
    }

    PlayerController Player()
    {
        if (player == null)
            player = FindAnyObjectByType<PlayerController>();
        return player;
    }

    // --- Wait ---

    // Why a Wait cannot start right now (null if it can).
    public string CantWait() => CantWait(false);

    // fromMenu: asked by the Wait menu itself, which holds the game in the Menu state, so "not in plain play" does not count against it.
    public string CantWaitFromMenu() => CantWait(true);

    string CantWait(bool fromMenu)
    {
        GameManager game = GameManager.Instance;
        bool playing = game == null || game.State == GameState.Playing || fromMenu && game.State == GameState.Menu;
        SleepManager sleep = SleepManager.Instance;
        bool asleep = sleep != null && sleep.IsSleeping || IsWaiting;
        PlayerController p = Player();
        bool inWater = p != null && Animal.IsOverWater(p.transform.position, out _);
        return TimeControlRules.CantWait(playing, asleep, inWater, Read());
    }

    // Game hours a Wait of this kind would run from now.
    public float HoursFor(WaitKind kind)
    {
        TimeManager time = TimeManager.Instance;
        return time == null ? 0f : TimeControlRules.HoursFor(kind, time.HourOfDay, time.SunriseHour, time.SunsetHour);
    }

    // Runs a Wait now. Returns false (with the reason on screen) when it cannot start; otherwise true, with the HUD line saying how long it ran and why it stopped if it did.
    // Call it with the game in plain play: close the Wait menu first.
    public bool Wait(WaitKind kind)
    {
        string reason = CantWait();
        if (reason != null)
        {
            ToolStatus.Flash(reason);
            return false;
        }

        float hours = HoursFor(kind);
        WorldWatch start = Read();
        StopReason why = StopReason.None;
        string detail = null;

        IsWaiting = true;
        float passed;
        try
        {
            passed = PassWorld(hours, true, soFar =>
            {
                why = TimeControlRules.Evaluate(start, Read(), out detail);
                return why != StopReason.None;
            });
        }
        finally
        {
            IsWaiting = false;
        }

        ToolStatus.Flash(why == StopReason.None
            ? TimeControlRules.DoneMessage(passed)
            : TimeControlRules.StoppedMessage(why, detail, passed), 5f);
        return true;
    }
}
