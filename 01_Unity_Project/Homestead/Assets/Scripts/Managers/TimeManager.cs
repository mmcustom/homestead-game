using System;
using UnityEngine;

public enum Season { Spring, Summer, Fall, Winter }

// Wildlife_System.md's Daily Activity Cycles: Dawn and Dusk are the high-activity hunting windows.
public enum DayPhase { Night, Dawn, Day, Dusk }

[Serializable]
public struct TimeSaveData
{
    // Save_Data_Model.md's World Block stores Current Day / Season / Year.
    // Season and Year are derived from totalDays, so only the day count and clock are persisted.
    public int totalDays;
    public float minuteOfDay;
}

// Clock, day/night, calendar and seasons (Unity_Architecture.md, Season_System.md).
public class TimeManager : MonoBehaviour, ISaveable
{
    public const int MinutesPerDay = 1440;
    const int SeasonCount = 4;

    public static TimeManager Instance { get; private set; }

    // Pacing values below aren't defined in the design docs yet — tuning defaults, not design decisions.
    [Header("Pacing")]
    [Tooltip("Real-time minutes for one full in-game day.")]
    [SerializeField, Min(0.1f)] float realMinutesPerDay = 24f;
    [SerializeField, Min(1)] int daysPerSeason = 28;
    [SerializeField, Range(0f, 24f)] float startHour = 6f;
    [SerializeField] bool runOnStart = true;

    [Header("Daylight")]
    [Tooltip("Sunrise (x) and sunset (y) hour per season, in Season order: Spring, Summer, Fall, Winter. " +
             "Season_System.md: Summer has long daylight hours, Fall has shorter days.")]
    [SerializeField] Vector2[] sunriseSunset =
    {
        new Vector2(6.5f, 19.5f),
        new Vector2(6f, 20.5f),
        new Vector2(7f, 18.5f),
        new Vector2(7.5f, 17f),
    };
    [Tooltip("Length of Dawn (after sunrise) and Dusk (before sunset). " +
             "Research.md: deer are most active in the first ~2 hours after sunrise and last ~2 before sunset.")]
    [SerializeField, Min(0f)] float twilightHours = 2f;

    int totalDays;
    float minuteOfDay;
    DayPhase phase;

    public event Action<int> HourChanged;
    public event Action DayChanged;
    public event Action<Season> SeasonChanged;
    public event Action<int> YearChanged;
    public event Action<DayPhase> PhaseChanged;

    public bool IsRunning { get; private set; }

    public int TotalDays => totalDays;
    public float MinuteOfDay => minuteOfDay;
    public float HourOfDay => minuteOfDay / 60f;
    public int Hour => (int)HourOfDay;
    public int Minute => (int)minuteOfDay % 60;

    public int DaysPerSeason => daysPerSeason;
    public int DayOfSeason => totalDays % daysPerSeason + 1;
    public Season CurrentSeason => (Season)(totalDays / daysPerSeason % SeasonCount);
    public int Year => totalDays / (daysPerSeason * SeasonCount) + 1;

    // 0 at the first minute of a season, approaching 1 at its end. Lets systems place windows
    // relatively (e.g. Large_Game.md's Rut Window is "the second half of Fall": SeasonProgress >= 0.5).
    public float SeasonProgress => (totalDays % daysPerSeason + minuteOfDay / MinutesPerDay) / daysPerSeason;

    public DayPhase Phase => phase;
    public float SunriseHour => sunriseSunset[(int)CurrentSeason].x;
    public float SunsetHour => sunriseSunset[(int)CurrentSeason].y;
    public bool IsDaylight => HourOfDay >= SunriseHour && HourOfDay < SunsetHour;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        ResetClock();
        IsRunning = runOnStart;
    }

    void Start()
    {
        if (Instance == this && SaveManager.Instance != null)
            SaveManager.Instance.Register(this);
    }

    void OnDestroy()
    {
        if (Instance != this)
            return;

        if (SaveManager.Instance != null)
            SaveManager.Instance.Unregister(this);
        Instance = null;
    }

    void OnValidate()
    {
        if (sunriseSunset == null || sunriseSunset.Length != SeasonCount)
            Array.Resize(ref sunriseSunset, SeasonCount);
    }

    void Update()
    {
        if (!IsRunning)
            return;

        AdvanceMinutes(Time.deltaTime * MinutesPerDay / (realMinutesPerDay * 60f));
    }

    public void Pause() => IsRunning = false;
    public void Resume() => IsRunning = true;

    // Advances the clock, firing every hour/day/season/year event crossed along the way.
    public void AdvanceMinutes(float minutes)
    {
        while (minutes > 0f)
        {
            float toNextHour = 60f - minuteOfDay % 60f;

            if (minutes < toNextHour)
            {
                minuteOfDay += minutes;
                minutes = 0f;
            }
            else
            {
                minuteOfDay = Mathf.Round(minuteOfDay + toNextHour);
                minutes -= toNextHour;

                if (minuteOfDay >= MinutesPerDay)
                {
                    minuteOfDay = 0f;
                    StartNewDay();
                }

                HourChanged?.Invoke(Hour);
            }

            UpdatePhase();
        }
    }

    // Advances forward to the next occurrence of the given hour, e.g. waking from Sleep
    // (Core_Survival_System.md). Does nothing if already at that time.
    public void AdvanceToHour(float hour)
    {
        float target = Mathf.Repeat(hour * 60f, MinutesPerDay);
        AdvanceMinutes(Mathf.Repeat(target - minuteOfDay, MinutesPerDay));
    }

    // Back to Spring day 1, Year 1 at the start hour — used when starting a new game. Fires no events.
    public void ResetClock()
    {
        totalDays = 0;
        minuteOfDay = startHour * 60f % MinutesPerDay;
        phase = ComputePhase();
    }

    public TimeSaveData CaptureState() => new TimeSaveData { totalDays = totalDays, minuteOfDay = minuteOfDay };

    // Restores silently — no change events fire, so listeners should re-read state after a load.
    public void RestoreState(TimeSaveData data)
    {
        totalDays = Mathf.Max(0, data.totalDays);
        minuteOfDay = Mathf.Repeat(data.minuteOfDay, MinutesPerDay);
        phase = ComputePhase();
    }

    // Save_Data_Model.md's World Block.
    string ISaveable.SaveFile => "world";
    string ISaveable.SaveKey => "time";
    object ISaveable.CaptureState() => CaptureState();
    void ISaveable.RestoreState(string json) => RestoreState(JsonUtility.FromJson<TimeSaveData>(json));

    void StartNewDay()
    {
        Season previousSeason = CurrentSeason;
        int previousYear = Year;

        totalDays++;
        DayChanged?.Invoke();

        if (CurrentSeason != previousSeason)
            SeasonChanged?.Invoke(CurrentSeason);

        if (Year != previousYear)
            YearChanged?.Invoke(Year);
    }

    void UpdatePhase()
    {
        DayPhase current = ComputePhase();
        if (current == phase)
            return;

        phase = current;
        PhaseChanged?.Invoke(phase);
    }

    DayPhase ComputePhase()
    {
        float hour = HourOfDay;

        if (hour < SunriseHour || hour >= SunsetHour)
            return DayPhase.Night;
        if (hour < SunriseHour + twilightHours)
            return DayPhase.Dawn;
        if (hour >= SunsetHour - twilightHours)
            return DayPhase.Dusk;
        return DayPhase.Day;
    }
}
