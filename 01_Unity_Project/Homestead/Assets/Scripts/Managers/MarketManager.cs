using System;
using System.Collections.Generic;
using UnityEngine;

// Trading_Post_Market.md: the Trading Post's simulated market in the running game. The rules and the multipliers are MarketRules and
// MarketState (engine-free, tested); this holds the one market of the current game, reads the prices from the MarketData asset, brings the
// market up to the calendar and saves it (player/market). TradingPost asks it for prices and sells through it.
//
// Processing is state-based, from TimeManager.TotalDays, exactly like MoneyManager: ProcessNow() applies recovery for every game day since
// it last ran, in order, and does nothing if it already has. It runs (1) from Update whenever the day has changed since it last ran, which
// covers a day passing, a sleep that skips many days, a debug skip and a load, (2) from SaveManager.OnDayChanged BEFORE the end-of-day
// autosave is requested, and (3) from CaptureState, so no save is ever written with the market behind the calendar. Load order between
// managers is not fixed, so a load only stores the state and the catch-up waits for Update (processedDay = -1).
//
// It lives on the Managers object in the Bootstrap scene, where the scalars below are inspector fields. Only if a game starts without a
// Bootstrap instance (a scene played on its own) does it create itself, after the first scene's Awakes have run, loading the asset from
// Resources; a scene instance always wins, so there is never a second one.
public class MarketManager : MonoBehaviour, ISaveable
{
    public static MarketManager Instance { get; private set; }

    [Header("Market (Trading_Post_Market.md)")]
    [Tooltip("The prices and group rules. If empty, Resources/Market/MarketData is used, then the built-in defaults.")]
    [SerializeField] MarketData data;
    [Tooltip("Percent of the gap to the seasonal target that a price recovers each game day.")]
    [SerializeField, Range(0, 100)] int recoveryPercent = 25;
    [Tooltip("What the store pays for a stocked item it bought back, as a percent of its whole-dollar base buy price.")]
    [SerializeField, Range(0, 100)] int sellBackPercent = 40;
    [Tooltip("Survival gear (tent, sleeping bag, tarp, lamp oil) costs this many percent more in Winter.")]
    [SerializeField, Range(-50, 100)] int winterBuyDriftPercent = 10;
    [Tooltip("Survival gear costs this many percent more in Summer (negative: cheaper).")]
    [SerializeField, Range(-50, 100)] int summerBuyDriftPercent = -5;
    [Tooltip("A price within this many percent of normal from its seasonal target counts as flat in the trend marker.")]
    [SerializeField, Range(0, 50)] int flatTrendBandPercent = 3;

    MarketTable table;
    readonly MarketState state = new MarketState();
    int processedDay = -1;

    // The market changed (a sale, a day's recovery, a new or loaded game).
    public event Action Changed;

    public MarketTable Table => table;
    public IReadOnlyList<MarketRow> Stock => table.Stock;

    // AfterSceneLoad runs after the first scene's Awakes, so the Bootstrap instance (inspector scalars) has already claimed Instance.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void CreateIfMissing()
    {
        if (Instance == null)
            new GameObject("Market Manager").AddComponent<MarketManager>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this); // only this component: the object may be the shared Managers root
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildTable();
    }

    void BuildTable()
    {
        MarketData source = data != null ? data : Resources.Load<MarketData>(MarketData.ResourcePath);
        if (source != null)
        {
            table = source.Build(sellBackPercent);
            if (table.Problems.Count == 0)
                return;
            Debug.LogWarning($"[Market] MarketData has problems, using the built-in defaults: {string.Join("; ", table.Problems)}");
        }
        else
        {
            Debug.LogWarning("[Market] No MarketData asset found, using the built-in defaults.");
        }

        table = new MarketTable(MarketCatalog.Rows, MarketCatalog.Groups, sellBackPercent);
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

    // --- New game, loading ---

    // New Game: every pool starts at its current-season target. Loading a save: a blank market (every multiplier 1.0, no day processed),
    // so a save with no market section starts normal and nothing recovers on its first day; a save that has one overwrites this.
    public void ResetMarket(bool newGame)
    {
        if (newGame)
            state.BeginNewGame();
        else
            state.Clear();
        processedDay = -1;
        Changed?.Invoke();
    }

    // --- Time ---

    int Today => TimeManager.Instance != null ? TimeManager.Instance.TotalDays : 0;
    int DaysPerSeason => TimeManager.Instance != null ? TimeManager.Instance.DaysPerSeason : 28;
    public int Season => MarketRules.SeasonOf(Today, DaysPerSeason);

    void Update()
    {
        GameManager game = GameManager.Instance;
        if (game == null || !game.InGame)
            return;

        TimeManager time = TimeManager.Instance;
        if (time != null && time.TotalDays != processedDay)
            ProcessNow();
    }

    // Catches the market up to the calendar. Safe to call at any time and as often as wanted.
    public void ProcessNow()
    {
        TimeManager time = TimeManager.Instance;
        if (time == null || time.TotalDays == processedDay)
            return;

        processedDay = time.TotalDays;
        state.Advance(table, time.TotalDays, time.DaysPerSeason, recoveryPercent);
        Changed?.Invoke();
    }

    // --- Prices ---

    public bool CanSell(string itemId) => table.SellBaseCents(itemId) > 0;

    public int Multiplier(string itemId) => state.Get(table.PoolOf(itemId));

    // Where the price is heading: the seasonal target of the item's group.
    public int Target(string itemId)
    {
        MarketGroupRule rule = table.RuleOf(itemId);
        return MarketRules.Clamp(rule.TargetFor(Season), rule.floorBp);
    }

    // What the store pays for one now, in cents (0: it doesn't buy it).
    public long SellPrice(string itemId)
    {
        long baseCents = table.SellBaseCents(itemId);
        return baseCents > 0 ? MarketRules.UnitPrice(baseCents, Multiplier(itemId)) : 0;
    }

    public long SellBase(string itemId) => table.SellBaseCents(itemId);

    // What the store charges for a stocked item now, season included (0: not for sale). Buying never changes it.
    public long BuyPrice(string itemId) =>
        table.TryGetRow(itemId, out MarketRow row) && row.kind == MarketKind.Stocked
            ? MarketRules.BuyPrice(row.baseCents, row.seasonalBuy, Season, winterBuyDriftPercent, summerBuyDriftPercent)
            : 0;

    public MarketTrend Trend(string itemId) => MarketRules.Trend(Multiplier(itemId), Target(itemId), flatTrendBandPercent);

    public string Note => MarketRules.SeasonNote(table, Season);

    // --- Selling ---

    public SaleResult Quote(string itemId, int count) => state.Quote(table, itemId, count);

    // Sells count units unit by unit at the falling price. The caller has already taken the items and pays the total.
    public SaleResult Sell(string itemId, int count)
    {
        ProcessNow();
        SaleResult result = state.Sell(table, itemId, count);
        if (result.count > 0)
            Changed?.Invoke();
        return result;
    }

    // --- Saving ---

    public MarketSaveData CaptureState()
    {
        ProcessNow(); // never save the market behind the calendar
        return state.Capture(table);
    }

    public void RestoreState(MarketSaveData data)
    {
        state.Restore(table, data);
        processedDay = -1;
        Changed?.Invoke();
    }

    string ISaveable.SaveFile => "player";
    string ISaveable.SaveKey => "market";
    object ISaveable.CaptureState() => CaptureState();
    void ISaveable.RestoreState(string json) => RestoreState(JsonUtility.FromJson<MarketSaveData>(json));
}
