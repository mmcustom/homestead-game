using UnityEngine;

// Slice P1 (Mike, 2026-11-27): the phone's service plan and its bills (PhoneRules has every number and the words). Created at start-up like the Ledger Manager;
// saved with the player. Bills come out of the cash on the 15th as "Bills" in the ledger; one that can't be paid suspends the service until it is paid.
// The player starts with the phone (DifficultyManager's kit); a save from before the phone existed gets one in the pack when it loads.
public class PhoneManager : MonoBehaviour, ISaveable
{
    public static PhoneManager Instance { get; private set; }

    PhoneState state = new PhoneState();
    int checkedDay = -1;
    bool grantOnLoad;

    public PhoneState State => state;
    public bool HasService => PhoneRules.HasService(state);
    public event System.Action Changed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void CreateIfMissing()
    {
        if (Instance == null)
            new GameObject("Phone Manager").AddComponent<PhoneManager>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
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

    static GameCalendar Calendar => CalendarRules.Current;
    static int Today => TimeManager.Instance != null ? TimeManager.Instance.TotalDays : 0;

    // Pays from the cash (the ledger first, as every payment does). False if there isn't enough.
    static bool Pay(long cents)
    {
        TradingPost post = TradingPost.Instance;
        if (post == null || post.Cents < cents)
            return false;
        if (LedgerManager.Instance != null)
            LedgerManager.Instance.Record(LedgerCategory.Bills, cents);
        return post.TrySpend(cents);
    }

    public bool StartPlan(out string message)
    {
        bool wasCancelling = state.active && state.cancelling;
        if (!PhoneRules.SignUp(state, Calendar, Today, Pay))
        {
            message = state.active ? "Your plan is already on" : $"Not enough money - the plan is {Money.Format(PhoneRules.PlanCents)} a month";
            return false;
        }
        message = wasCancelling ? "Your plan carries on" : $"Plan started - {Money.Format(PhoneRules.PlanCents)} paid; next bill on {DateOf(state.nextBillDay)}";
        Changed?.Invoke();
        return true;
    }

    public void CancelPlan()
    {
        PhoneRules.Cancel(state);
        Changed?.Invoke();
    }

    public bool PayOverdue(out string message)
    {
        long owed = state.owedCents;
        if (!PhoneRules.PayOverdue(state, Calendar, Today, Pay))
        {
            message = $"Not enough money - {Money.Format(owed)} is owed";
            return false;
        }
        message = $"Paid {Money.Format(owed)} - service is back";
        Changed?.Invoke();
        return true;
    }

    public static string DateOf(int day) => TimeManager.Instance != null ? TimeManager.Instance.FormatDate(day) : Calendar.FormatDate(day);

    void Update()
    {
        if (Instance != this || GameManager.Instance == null || GameManager.Instance.State != GameState.Playing || TimeManager.Instance == null)
            return;

        // A save from before the phone: one in the pack, once.
        if (grantOnLoad && InventoryManager.Instance != null)
        {
            grantOnLoad = false;
            if (InventoryManager.Instance.Player.Count(PhoneRules.PhoneId) <= 0)
                InventoryManager.Instance.AddToPlayer(PhoneRules.PhoneId, 1);
        }

        int today = Today;
        if (today == checkedDay)
            return;
        checkedDay = today;
        bool changed = false;
        foreach (BillResult bill in PhoneRules.Advance(state, Calendar, today, Pay))
        {
            changed = true;
            if (bill.ended)
                ToolStatus.Banner(PhoneRules.Ended, 6f);
            else if (bill.paid)
                ToolStatus.Flash(PhoneRules.Paid(bill.cents), 4f);
            else
                ToolStatus.Banner(PhoneRules.Suspended(bill.cents), 10f);
        }
        if (changed)
            Changed?.Invoke();
    }

    // A new game: no plan (the phone itself comes in the starting kit).
    public void ResetForNewGame()
    {
        state = new PhoneState();
        checkedDay = -1;
        grantOnLoad = false;
        Changed?.Invoke();
    }

    // Before a load: no plan unless the save has one; a save without a phone section is from before the phone, so it gets one in the pack.
    public void ResetForLoad()
    {
        state = new PhoneState();
        checkedDay = -1;
        grantOnLoad = true;
        Changed?.Invoke();
    }

    public void RestoreState(PhoneState data)
    {
        state = data ?? new PhoneState();
        if (state.history == null)
            state.history = new System.Collections.Generic.List<BillRecord>();
        checkedDay = -1;
        grantOnLoad = false;
        Changed?.Invoke();
    }

    string ISaveable.SaveFile => "player";
    string ISaveable.SaveKey => "phone";
    object ISaveable.CaptureState() => state;
    void ISaveable.RestoreState(string json) => RestoreState(JsonUtility.FromJson<PhoneState>(json));
}
