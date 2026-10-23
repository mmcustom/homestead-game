using System;
using System.Collections.Generic;
using UnityEngine;

// Money_System.md Phase B: the ledger of the running game. The record itself is LedgerState (engine-free, tested); this holds the one
// ledger of the current game, lets the Trading Post and the loan write to it, closes seasons and saves it (player/ledger). Every closed
// season is a statement the player can read in the Finances screen (L).
//
// Seasons close state-based, from TimeManager.TotalDays, exactly like MoneyManager and MarketManager: ProcessNow() calls MoneyManager's
// catch-up first (so the loan figures are current), then closes every season whose end day has passed, however many that is, and does
// nothing if there is nothing to close. It runs (1) from SaveManager.OnDayChanged BEFORE the end-of-day autosave is requested (TimeManager
// fires DayChanged BEFORE SeasonChanged, so a SeasonChanged handler would be too late for that save), (2) from Update, which covers a
// load and a debug skip, and (3) from CaptureState, so no save is ever written with a season unclosed.
//
// A new game starts the ledger at its first ProcessNow with the full savings as the opening cash and the down payment as a season-1
// expense (the player never held the down payment as cash, so cash starts at the reserve). An old save with no ledger starts it at its
// first ProcessNow after the load, with the cash it has then as the opening balance: no invented past. Money that moves by anything other
// than the hooks (the F12 debug money, loading) is never written: a statement shows any such difference as its "Other" line.
//
// It lives on the Managers object in the Bootstrap scene. Only if a game starts without a Bootstrap instance does it create itself, after
// the first scene's Awakes have run, so a scene instance always wins and there is never a second one.
public class LedgerManager : MonoBehaviour, ISaveable
{
    public static LedgerManager Instance { get; private set; }

    readonly LedgerState state = new LedgerState();
    bool pendingNew;
    long pendingSavingsCents, pendingDownCents;

    public LedgerState State => state;
    public bool Active => state.Started;

    // A season has closed that the player has not opened the statement for yet.
    public bool Unread => state.Unread;

    // The ledger changed (a season closed, a new or loaded ledger, the statement read).
    public event Action Changed;

    // AfterSceneLoad runs after the first scene's Awakes, so the Bootstrap instance has already claimed Instance.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void CreateIfMissing()
    {
        if (Instance == null)
            new GameObject("Ledger Manager").AddComponent<LedgerManager>();
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

    // New Game: a fresh ledger that starts when the game does (the clock is reset after this is called). savingsCents is the whole savings
    // (down payment plus cash reserve), downCents the part that went into the land.
    public void BeginNewGame(long savingsCents, long downCents)
    {
        ToolStatus.ClearBanners();
        state.Clear();
        pendingNew = true;
        pendingSavingsCents = Math.Max(0L, savingsCents);
        pendingDownCents = Math.Max(0L, downCents);
        Changed?.Invoke();
    }

    // Before every load: a blank ledger, so a save with no ledger section starts one at the first ProcessNow, and a save that has one
    // overwrites this.
    public void ResetLedger()
    {
        ToolStatus.ClearBanners();
        state.Clear();
        pendingNew = false;
        Changed?.Invoke();
    }

    // --- Time ---

    void Update()
    {
        GameManager game = GameManager.Instance;
        if (game == null || !game.InGame)
            return;
        ProcessNow();
    }

    static long NowMinutes(TimeManager time) => (long)Math.Round(time.TotalHours * 60.0);

    // Brings the ledger up to the calendar: starts it if it hasn't started, and closes every season that has ended. Safe to call at any
    // time and as often as wanted.
    public void ProcessNow()
    {
        TimeManager time = TimeManager.Instance;
        TradingPost post = TradingPost.Instance;
        if (time == null || post == null)
            return;

        MoneyManager money = MoneyManager.Instance;
        if (money != null)
            money.ProcessNow(); // payments and dates first, so the loan figures the season closes with are current

        int seasonLength = time.DaysPerSeason;
        int today = LedgerState.SeasonOf(time.TotalDays, seasonLength);

        if (!state.Started)
        {
            if (pendingNew)
            {
                state.Begin(pendingSavingsCents, today, NowMinutes(time), seasonLength);
                state.Record(LedgerCategory.DownPayment, pendingDownCents);
                pendingNew = false;
            }
            else
            {
                state.Begin(post.Cents, today, NowMinutes(time), seasonLength);
            }
            Changed?.Invoke();
            return;
        }

        if (today <= state.OpenIndex)
            return;

        long loan = money != null && money.Loan != null ? money.Loan.balanceCents : 0;
        long owed = money != null ? money.OwedNow : 0;
        int closed = state.CloseThrough(today, post.Cents, loan, owed);
        if (closed > 0)
        {
            Announce(closed);
            Changed?.Invoke();
        }
    }

    // The statement-ready message. It goes through ToolStatus's banner queue, so a payment-due banner on the same day shows too, and the
    // saved unread flag keeps a persistent line on the HUD until the statement is opened.
    void Announce(int closed)
    {
        LedgerSeason last = state.Closed[state.Closed.Count - 1];
        string label = LedgerState.SeasonLabel(last.index);
        ToolStatus.Banner(closed == 1 ? $"Your account statement for {label} is ready. Press L to read it."
                                      : $"{closed} account statements are ready, up to {label}. Press L to read them.", 8f);
    }

    // --- Writing to the ledger ---

    // Money that moved. Call it BEFORE the cash changes: if the call finds a season to close, it closes it with the cash as it was.
    public void Record(LedgerCategory category, long cents)
    {
        if (cents <= 0)
            return;
        ProcessNow();
        state.Record(category, cents);
    }

    // The four expense lines of a loan payment (what PayResult split it into).
    public void RecordLoanPayment(PayResult result)
    {
        Record(LedgerCategory.LoanFees, result.toFee);
        Record(LedgerCategory.LoanInterest, result.toInterest);
        Record(LedgerCategory.LoanPrincipal, result.toPrincipal);
        Record(LedgerCategory.LoanExtra, result.toExtra);
    }

    // The player has opened the statement.
    public void MarkRead()
    {
        if (!state.Unread)
            return;
        state.Unread = false;
        Changed?.Invoke();
    }

    // --- The statement ---

    // Every season since the start, newest first (the open season first, in progress), with the cash and loan figures as they are now.
    public List<StatementRow> Rows()
    {
        ProcessNow();
        TimeManager time = TimeManager.Instance;
        TradingPost post = TradingPost.Instance;
        MoneyManager money = MoneyManager.Instance;
        if (time == null || post == null)
            return new List<StatementRow>();
        long loan = money != null && money.Loan != null ? money.Loan.balanceCents : 0;
        long owed = money != null ? money.OwedNow : 0;
        return state.Rows(post.Cents, loan, owed, NowMinutes(time));
    }

    // --- Saving ---

    public LedgerSaveData CaptureState()
    {
        ProcessNow(); // never save with a season unclosed
        return state.Capture();
    }

    public void RestoreState(LedgerSaveData data)
    {
        state.Restore(data);
        pendingNew = false;
        Changed?.Invoke();
    }

    string ISaveable.SaveFile => "player";
    string ISaveable.SaveKey => "ledger";
    object ISaveable.CaptureState() => CaptureState();
    void ISaveable.RestoreState(string json) => RestoreState(JsonUtility.FromJson<LedgerSaveData>(json));
}
