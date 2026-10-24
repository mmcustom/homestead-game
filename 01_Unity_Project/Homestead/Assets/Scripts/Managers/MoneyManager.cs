using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Money_System.md Phase A: the land loan in the running game. The loan itself is LoanState (engine-free, tested); this holds the one
// loan of the current game, catches it up to the calendar, takes payments out of the Trading Post's cash, gives the warnings and
// saves it (player/loan). A game with no loan data (an old save) has no loan, no warnings and no Finances screen.
//
// Processing is state-based, from TimeManager.TotalDays: ProcessNow() catches up on every due date and late date to today, in order,
// and does nothing if it already has. It runs (1) from Update whenever the day has changed since it last ran, which covers a day
// passing, a sleep that skips many days, a debug skip and a load, (2) from SaveManager.OnDayChanged BEFORE the end-of-day autosave is
// requested, and (3) from CaptureState, so no save can ever be written with the loan behind the calendar.
//
// Difficulty tiers (Money_System.md Phase D1): the interest rate, the late fee, the missed payments that lose the game and the Trading Post's
// sell share come from the tier table (TierRules), by the game's difficulty. The inspector fields of the same names are an OVERRIDE for
// testing: only when "Override Tier Terms" is ticked do they replace the table, for every tier. A new loan freezes its rate and late fee
// (a loan keeps the terms it was made with); the loss threshold and the sell share are read from the tier each time.
//
// It lives on the Managers object in the Bootstrap scene, where the loan terms below are inspector fields. Only if a game starts without
// a Bootstrap instance (a scene played on its own) does it create itself, after the first scene's Awakes have run, so a scene instance
// always wins and there is never a second one.
public class MoneyManager : MonoBehaviour, ISaveable
{
    public static MoneyManager Instance { get; private set; }

    [Header("Tier override (testing only)")]
    [Tooltip("Off (the normal state): the interest rate, late fee, missed payments before a loss and sell share come from the tier table (TierRules). " +
             "On: the four fields marked 'override' below replace the table for every tier.")]
    [SerializeField] bool overrideTierTerms;
    [Tooltip("Override only: the Trading Post's share of a sale total in basis points (10000 = 100%).")]
    [SerializeField, Min(1)] int sellBasisPoints = TierRules.FullSellBasisPoints;

    [Header("Land loan (Money_System.md)")]
    [Tooltip("Override only (see above): yearly interest in basis points: 800 is 8%, spread over 12 payments a year.")]
    [SerializeField, Min(0)] int rateBasisPoints = 800;
    [Tooltip("Payments in the whole loan (15 years).")]
    [SerializeField, Min(1)] int payments = 180;
    [Tooltip("Game days between payments (a season).")]
    [SerializeField, Min(1)] int intervalDays = 28;
    [Tooltip("TotalDays the first payment falls due: the end of the first year.")]
    [SerializeField, Min(1)] int firstDueDay = 112;
    [Tooltip("A payment still unpaid this many days after its due day gets the late fee and counts as missed.")]
    [SerializeField, Min(0)] int graceDays = 10;
    [Tooltip("Override only (see above): the one-time late fee, in cents ($75).")]
    [SerializeField, Min(0)] long lateFeeCents = 7500;
    [Tooltip("Open payments at which the Finances screen and the messages show the red warning.")]
    [SerializeField, Min(1)] int missedWarningCount = 3;
    [Tooltip("Override only (see above): missed payments open at once that lose the game (Money_System.md Phase C). 0 turns the loss off.")]
    [SerializeField, Min(0)] int lossMissedCount = LossRules.DefaultThreshold;

    LoanState loan;
    int processedDay = -1;
    InputAction financesAction;

    public bool Active => loan != null;
    public LoanState Loan => loan;
    public int MissedWarningCount => missedWarningCount;
    public bool WarnMissed => loan != null && loan.MissedCount >= missedWarningCount;

    // The game is lost: the missed payments open at once reached lossMissedCount. Set the moment day processing finds it (even in the
    // middle of a sleep), cleared by a new loan, a reset or a load. While it is set nothing may be saved (SaveManager) and no season closes.
    public bool Lost { get; private set; }
    public RunSummary LossSummary { get; private set; }
    public int LossThreshold => Tier.missedBeforeLoss;

    // The loan changed (a payment, a due date, a late fee, a new or loaded loan).
    public event Action Changed;

    // The terms of the game's difficulty tier now (the table, or the inspector override).
    public TierTerms Tier => TierFor(DifficultyManager.Tier);

    // The same for any tier, for the New Game summary before the difficulty is set.
    public TierTerms TierFor(Difficulty tier) => overrideTierTerms
        ? new TierTerms { missedBeforeLoss = lossMissedCount, lateFeeCents = lateFeeCents, rateBasisPoints = rateBasisPoints, sellBasisPoints = sellBasisPoints }
        : TierRules.For((int)tier);

    // The tier now, without needing a manager (a scene played on its own gets the table).
    public static TierTerms CurrentTier => Instance != null ? Instance.Tier : TierRules.For((int)DifficultyManager.Tier);

    // The loan terms a new loan starts with on this tier: the inspector's payments, spacing, first due day and grace days, with the tier's
    // rate and late fee.
    public LoanTerms TermsFor(Difficulty tier) => TierRules.LoanTermsFor(new LoanTerms
    {
        rateBasisPoints = rateBasisPoints,
        payments = payments,
        intervalDays = intervalDays,
        firstDueDay = firstDueDay,
        graceDays = graceDays,
        lateFeeCents = lateFeeCents,
    }, TierFor(tier));

    public LoanTerms Terms => TermsFor(DifficultyManager.Tier);

    // AfterSceneLoad runs after the first scene's Awakes, so the Bootstrap instance (inspector terms) has already claimed Instance.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void CreateIfMissing()
    {
        if (Instance == null)
            new GameObject("Money Manager").AddComponent<MoneyManager>();
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

    // --- Starting, ending and loading a loan ---

    // New Game: the land bought with this down payment, on the loan terms set here.
    public void BeginLoan(int acres, long priceCents, long downCents)
    {
        loan = LoanState.Create(acres, priceCents, downCents, Terms);
        processedDay = -1;
        ClearLoss();
        Changed?.Invoke();
    }

    // New Game without land, and before every load: no loan, so a save that has none stays exactly as it was.
    public void ResetLoan()
    {
        loan = null;
        processedDay = -1;
        ClearLoss();
        FinancesScreen.HideIfOpen();
        Changed?.Invoke();
    }

    // --- Time ---

    void Update()
    {
        GameManager game = GameManager.Instance;
        if (game == null || !game.InGame)
            return;

        // The Finances screen (L) opens on every save, with or without a loan (the Statement tab always works). Only the loan's own
        // catch-up needs a loan.
        TimeManager time = TimeManager.Instance;
        if (loan != null && time != null && time.TotalDays != processedDay)
            ProcessNow();

        HandleKey(game);
    }

    // Catches the loan up to the calendar. Safe to call at any time and as often as wanted.
    public void ProcessNow()
    {
        TimeManager time = TimeManager.Instance;
        if (loan == null || time == null || time.TotalDays == processedDay)
            return;

        processedDay = time.TotalDays;
        List<LoanNotice> notices = loan.Advance(time.TotalDays);
        foreach (LoanNotice notice in notices)
            Announce(notice, time);
        if (!Lost && LossRules.IsLoss(loan.MissedCount, Tier.missedBeforeLoss))
            DeclareLoss(time);
        Changed?.Invoke();
    }

    void ClearLoss()
    {
        Lost = false;
        LossSummary = default;
    }

    // Money_System.md Phase C. Lost is set FIRST so nothing below (the ledger rows, a banner) can close a season or save a lost game. The
    // summary is taken now, from the ledger and the loan as they stand on the day of the third miss.
    void DeclareLoss(TimeManager time)
    {
        Lost = true;

        long income = 0, expenses = 0;
        if (LedgerManager.Instance != null)
        {
            StatementRow since = LedgerState.SinceStart(LedgerManager.Instance.Rows());
            income = since.income;
            expenses = since.expenses;
        }

        var dueDays = new List<int>();
        if (loan != null)
        {
            foreach (OpenPayment payment in loan.open)
            {
                if (payment.missed)
                    dueDays.Add(payment.dueDay);
            }
        }

        TradingPost post = TradingPost.Instance;
        LossSummary = RunSummary.Build(time.TotalDays, time.DaysPerSeason, income, expenses, loan != null ? loan.balanceCents : 0,
                                       dueDays, post != null ? post.Cents : 0)
            .WithSettlement(LossSettlement.Of(loan, post != null ? post.Cents : 0)); // the cash goes to the debt on a copy: display only
        ToolStatus.ClearBanners(); // the loss screen is what the player sees now, not a queue of payment banners
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    // Testing only (TimeDebugControls, Insert): loses the game now, as if the last missed payment had just landed.
    public void DebugForceLoss()
    {
        TimeManager time = TimeManager.Instance;
        if (Lost || time == null)
            return;
        DeclareLoss(time);
        Changed?.Invoke();
    }
#endif

    string DateOf(TimeManager time, int day) => time != null ? time.FormatDate(day) : "day " + day;

    void Announce(LoanNotice notice, TimeManager time)
    {
        switch (notice.kind)
        {
            case LoanNoticeKind.DueSoon:
                ToolStatus.Flash($"Land payment of {Money.Format(notice.amount)} due in {notice.days} day{(notice.days == 1 ? "" : "s")} ({DateOf(time, notice.dueDay)}). Press L for Finances.", 6f);
                break;
            case LoanNoticeKind.DueToday:
                ToolStatus.Banner($"Your land payment of {Money.Format(notice.amount)} is due today. Press L to pay.", 8f);
                break;
            case LoanNoticeKind.Late:
                ToolStatus.Banner($"Your land payment is overdue ({Money.Format(notice.amount)}). Pay by {DateOf(time, notice.dueDay + loan.terms.graceDays - 1)} or a {Money.Format(loan.terms.lateFeeCents)} late fee is added.", 10f);
                break;
            case LoanNoticeKind.Missed:
                int missed = loan.MissedCount;
                string text = $"MISSED land payment: a {Money.Format(loan.terms.lateFeeCents)} late fee was added ({Money.Format(notice.amount)} now owed). {missed} missed payment{(missed == 1 ? " is" : "s are")} open.";
                if (missed >= missedWarningCount)
                    text = $"WARNING: {missed} missed land payments are open! " + text;
                ToolStatus.Banner(text, 12f);
                break;
        }
    }

    // --- Paying ---

    public long OwedNow => loan != null ? loan.OwedNow : 0;

    // Pays what is owed on every open payment (as far as cash goes).
    public PayResult PayDue() => Pay(OwedNow);

    // Pays up to this amount, never more than cash. Open payments first, oldest first; any rest is an extra payment to principal.
    public PayResult Pay(long amountCents)
    {
        TradingPost post = TradingPost.Instance;
        if (loan == null || post == null)
            return default;

        ProcessNow();
        if (LedgerManager.Instance != null)
            LedgerManager.Instance.ProcessNow(); // a season that ended closes with the loan and cash as they were before this payment
        PayResult result = loan.Pay(post.Cents, amountCents);
        if (result.applied > 0)
        {
            post.TrySpend(result.applied);
            if (LedgerManager.Instance != null)
                LedgerManager.Instance.RecordLoanPayment(result); // late fees, interest, principal and extra payment lines
            if (result.paidOff)
                ToolStatus.Banner($"Congratulations! The land is paid off. {Money.Format(loan.interestPaid)} in interest paid over the loan.", 12f);
        }
        Changed?.Invoke();
        return result;
    }

    // --- The Finances screen's key ---

    void HandleKey(GameManager game)
    {
        if (financesAction == null && InputSystem.actions != null)
            financesAction = InputSystem.actions.FindAction("Player/Finances");
        if (financesAction == null || !financesAction.WasPressedThisFrame() || GameScreens.IsTyping)
            return;

        if (FinancesScreen.IsOpen)
            FinancesScreen.Close();
        else if (game.State == GameState.Playing && !(SleepManager.Instance != null && SleepManager.Instance.IsSleeping))
            FinancesScreen.Open();
    }

    // --- Saving ---

    public LoanSaveData CaptureState()
    {
        ProcessNow(); // never save the loan behind the calendar
        return LoanSaveData.Of(loan);
    }

    public void RestoreState(LoanSaveData data)
    {
        loan = data.ToLoan();
        processedDay = -1;
        ClearLoss();
        Changed?.Invoke();
    }

    string ISaveable.SaveFile => "player";
    string ISaveable.SaveKey => "loan";
    object ISaveable.CaptureState() => CaptureState();
    void ISaveable.RestoreState(string json) => RestoreState(JsonUtility.FromJson<LoanSaveData>(json));
}
