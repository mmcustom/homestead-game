using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Money_System.md Phase B: the screen has two tabs, Loan and Statement (StatementPanel, the seasonal account statement). It opens on every
// save, with or without a loan: without one the Loan tab says so and the Statement tab works as usual. It opens on the Statement tab
// when a statement is unread (or there is no loan), otherwise on the tab last used.
//
// Money_System.md Phase A's Finances screen, on its own key (L), separate from the T window: the land loan at a glance, every open
// payment, and where to pay. "Pay what is due" pays every open payment as far as cash goes; the amount box pays any amount, oldest
// open payment first (late fee, interest, principal), and what is left over is an extra payment to principal. Built in code in the
// game's dark UI look. While it is open GameManager is in the Menu state, so the clock stops, like the other game screens.
public class FinancesScreen : MonoBehaviour
{
    static FinancesScreen instance;

    public static bool IsOpen => instance != null && instance.open;

    public static void Open()
    {
        if (MoneyManager.Instance == null)
            return;
        if (instance == null)
        {
            var go = new GameObject("Finances Screen");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<FinancesScreen>();
            instance.Build();
        }
        instance.Show();
    }

    public static void Close()
    {
        if (instance != null)
            instance.Hide(true);
    }

    public static void HideIfOpen()
    {
        if (IsOpen)
            instance.Hide(false);
    }

    const float PanelWidth = 1500f, PanelHeight = 880f;

    enum Tab { Loan, Statement }

    static Tab lastTab = Tab.Loan;

    bool open, subscribed;
    Tab tab;
    GameObject root;
    RectTransform loanRoot;
    Text titleLabel, noLoanLabel;
    Button loanTabButton, statementTabButton;
    StatementPanel statement;
    Text cashLabel, warningLabel, statusLabel, openHeading, termsHeading, termsLabel;
    readonly Text[] values = new Text[Labels.Length];
    RectTransform openList;
    InputField amountField;
    Button payDueButton, payAmountButton;

    static readonly string[] Labels =
    {
        "Land", "Down payment", "Original loan", "Balance", "Payment", "Next payment", "Owed now", "Missed payments",
        "Late fees owed", "Payments made", "Interest paid", "Extra payments", "Interest still to come",
    };

    // --- Layout ---

    void Build()
    {
        root = new GameObject("Root", typeof(RectTransform));
        root.transform.SetParent(transform, false);
        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 150;
        var scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        root.AddComponent<GraphicRaycaster>();

        var rootRect = (RectTransform)root.transform;
        Image dim = UiKit.Image(rootRect, "Dim", new Color(0f, 0f, 0f, 0.6f));
        dim.rectTransform.Fill();
        dim.raycastTarget = true;

        Color solid = UiKit.PanelColor;
        solid.a = 1f;
        Image panelImage = UiKit.Image(rootRect, "Panel", solid);
        panelImage.raycastTarget = true;
        RectTransform panel = panelImage.rectTransform;
        panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.sizeDelta = new Vector2(PanelWidth, PanelHeight);

        titleLabel = UiKit.Text(panel, "Title", "Land Loan", 34, UiKit.Accent, TextAnchor.MiddleLeft, FontStyle.Italic);
        Place(titleLabel.rectTransform, 32f, 14f, 400f, 54f);
        loanTabButton = UiKit.Button(panel, "Loan Tab", "Loan", 22, () => ShowTab(Tab.Loan));
        Place((RectTransform)loanTabButton.transform, 440f, 18f, 190f, 46f);
        statementTabButton = UiKit.Button(panel, "Statement Tab", "Statement", 22, () => ShowTab(Tab.Statement));
        Place((RectTransform)statementTabButton.transform, 640f, 18f, 190f, 46f);
        cashLabel = UiKit.Text(panel, "Cash", "", 28, UiKit.Cream, TextAnchor.MiddleRight);
        Place(cashLabel.rectTransform, 860f, 14f, 400f, 54f);
        Button close = UiKit.Button(panel, "Close", "Close  [L]", 20, () => Hide(true));
        Place((RectTransform)close.transform, PanelWidth - 32f - 170f, 18f, 170f, 46f);

        Image rule = UiKit.Image(panel, "Rule", new Color(UiKit.Cream.r, UiKit.Cream.g, UiKit.Cream.b, 0.2f));
        Place(rule.rectTransform, 24f, 80f, PanelWidth - 48f, 2f);

        loanRoot = UiKit.Rect("Loan", panel);
        loanRoot.Fill();

        const float rowTop = 100f, rowStep = 40f;
        for (int i = 0; i < Labels.Length; i++)
        {
            Text label = UiKit.Text(loanRoot, Labels[i], Labels[i], 22, UiKit.Muted, TextAnchor.MiddleLeft);
            Place(label.rectTransform, 40f, rowTop + i * rowStep, 330f, rowStep);
            values[i] = UiKit.Text(loanRoot, Labels[i] + " Value", "", 22, UiKit.Cream, TextAnchor.MiddleLeft);
            values[i].horizontalOverflow = HorizontalWrapMode.Overflow;
            Place(values[i].rectTransform, 380f, rowTop + i * rowStep, 360f, rowStep);
        }

        openHeading = UiKit.Text(loanRoot, "Open Heading", "Open payments", 26, UiKit.Accent, TextAnchor.MiddleLeft, FontStyle.Italic);
        Place(openHeading.rectTransform, 780f, 98f, 600f, 40f);
        openList = UiKit.ScrollList(loanRoot, "Open Payments", 4f, true);
        Place((RectTransform)openList.parent, 780f, 144f, PanelWidth - 780f - 40f, 290f);

        termsHeading = UiKit.Text(loanRoot, "Terms Heading", "Terms of this game", 26, UiKit.Accent, TextAnchor.MiddleLeft, FontStyle.Italic);
        Place(termsHeading.rectTransform, 780f, 450f, 600f, 40f);
        termsLabel = UiKit.Text(loanRoot, "Terms", "", 22, UiKit.Cream, TextAnchor.UpperLeft);
        termsLabel.horizontalOverflow = HorizontalWrapMode.Overflow;
        Place(termsLabel.rectTransform, 780f, 494f, PanelWidth - 780f - 40f, 130f);

        warningLabel = UiKit.Text(loanRoot, "Warning", "", 24, UiKit.Warning, TextAnchor.MiddleLeft, FontStyle.Bold);
        Place(warningLabel.rectTransform, 40f, 640f, PanelWidth - 80f, 44f);

        Image rule2 = UiKit.Image(loanRoot, "Rule 2", new Color(UiKit.Cream.r, UiKit.Cream.g, UiKit.Cream.b, 0.2f));
        Place(rule2.rectTransform, 24f, 696f, PanelWidth - 48f, 2f);

        payDueButton = UiKit.Button(loanRoot, "Pay Due", "Pay what is due", 22, PayDue);
        Place((RectTransform)payDueButton.transform, 40f, 720f, 280f, 56f);

        Text amountLabel = UiKit.Text(loanRoot, "Amount Label", "Any amount:", 22, UiKit.Muted, TextAnchor.MiddleRight);
        Place(amountLabel.rectTransform, 340f, 720f, 170f, 56f);
        amountField = UiKit.Input(loanRoot, "Amount", "$0.00", 24, false);
        amountField.characterLimit = 20;
        Place((RectTransform)amountField.transform, 520f, 720f, 280f, 56f);
        payAmountButton = UiKit.Button(loanRoot, "Pay Amount", "Pay amount", 22, PayAmount);
        Place((RectTransform)payAmountButton.transform, 820f, 720f, 220f, 56f);

        Text hint = UiKit.Text(loanRoot, "Hint", "Extra money goes straight to the balance and lowers the interest. Esc or L closes.", 18, UiKit.Muted, TextAnchor.MiddleLeft);
        Place(hint.rectTransform, 40f, 790f, PanelWidth - 80f, 34f);
        statusLabel = UiKit.Text(loanRoot, "Status", "", 22, UiKit.Cream, TextAnchor.MiddleLeft);
        Place(statusLabel.rectTransform, 40f, 826f, PanelWidth - 80f, 40f);


        noLoanLabel = UiKit.Text(panel, "No Loan", "There is no land loan in this game.\nThe Statement tab shows your income and expenses season by season.",
                                 26, UiKit.Muted, TextAnchor.MiddleCenter);
        Place(noLoanLabel.rectTransform, 100f, 300f, PanelWidth - 200f, 140f);

        statement = new StatementPanel();
        statement.Build(panel, 24f, 96f, PanelWidth - 48f, PanelHeight - 96f - 24f);

        root.SetActive(false);
    }

    // Places a child by its top-left corner within its parent, in pixels.
    static void Place(RectTransform rt, float x, float y, float width, float height)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
        rt.sizeDelta = new Vector2(width, height);
        rt.anchoredPosition = new Vector2(x, -y);
    }

    // --- Showing ---

    void Show()
    {
        if (open || GameManager.Instance == null || !GameManager.Instance.OpenMenu())
            return;

        open = true;
        root.SetActive(true);
        Subscribe();
        statusLabel.text = "";
        amountField.text = "";

        bool hasLoan = MoneyManager.Instance != null && MoneyManager.Instance.Active;
        bool unread = LedgerManager.Instance != null && LedgerManager.Instance.Unread;
        ShowTab(!hasLoan || unread ? Tab.Statement : lastTab);
        if (AudioManager.Instance != null)
            AudioManager.Instance.Play(SoundCue.UiClick);
    }

    // Loan or Statement. The Statement tab builds its rows now and marks the new statement as read.
    void ShowTab(Tab to)
    {
        tab = to;
        lastTab = to;
        bool hasLoan = MoneyManager.Instance != null && MoneyManager.Instance.Active;
        loanRoot.gameObject.SetActive(to == Tab.Loan && hasLoan);
        noLoanLabel.gameObject.SetActive(to == Tab.Loan && !hasLoan);
        statement.Root.gameObject.SetActive(to == Tab.Statement);
        titleLabel.text = to == Tab.Loan ? "Land Loan" : "Account Statement";
        UiKit.SetSelected(loanTabButton, to == Tab.Loan);
        UiKit.SetSelected(statementTabButton, to == Tab.Statement);
        if (to == Tab.Statement)
        {
            statement.Show();
            if (LedgerManager.Instance != null)
                LedgerManager.Instance.MarkRead();
        }
        UiKit.ReleaseFocus();
        Refresh();
    }

    void Hide(bool closeMenu)
    {
        if (!open)
            return;

        open = false;
        root.SetActive(false);
        Unsubscribe();
        if (UnityEngine.EventSystems.EventSystem.current != null)
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        if (closeMenu && GameManager.Instance != null && GameManager.Instance.State == GameState.Menu)
            GameManager.Instance.CloseMenu();
        if (AudioManager.Instance != null)
            AudioManager.Instance.Play(SoundCue.UiBack);
    }

    void Subscribe()
    {
        if (subscribed)
            return;
        subscribed = true;
        if (MoneyManager.Instance != null)
            MoneyManager.Instance.Changed += Refresh;
        if (TradingPost.Instance != null)
            TradingPost.Instance.MoneyChanged += Refresh;
        if (GameManager.Instance != null)
            GameManager.Instance.StateChanged += OnStateChanged;
    }

    void Unsubscribe()
    {
        if (!subscribed)
            return;
        subscribed = false;
        if (MoneyManager.Instance != null)
            MoneyManager.Instance.Changed -= Refresh;
        if (TradingPost.Instance != null)
            TradingPost.Instance.MoneyChanged -= Refresh;
        if (GameManager.Instance != null)
            GameManager.Instance.StateChanged -= OnStateChanged;
    }

    // Esc (back to Playing), the main menu or a load: any way out of the Menu state closes the screen.
    void OnStateChanged(GameState state)
    {
        if (state != GameState.Menu)
            Hide(false);
    }

    void OnDestroy()
    {
        Unsubscribe();
        if (instance == this)
            instance = null;
    }

    // --- Content ---

    void Refresh()
    {
        MoneyManager money = MoneyManager.Instance;
        LoanState loan = money != null ? money.Loan : null;
        if (!open)
            return;

        TimeManager time = TimeManager.Instance;
        TradingPost post = TradingPost.Instance;
        long cash = post != null ? post.Cents : 0;
        cashLabel.text = "Cash  " + Money.Format(cash);
        if (loan == null || tab != Tab.Loan)
            return; // no loan (the Loan tab says so), or the Statement tab: nothing of the loan to fill in

        string next = "None: the loan is paid";
        if (loan.nextDueDay >= 0)
        {
            int inDays = time != null ? loan.nextDueDay - time.TotalDays : 0;
            string when = time != null ? time.FormatDate(loan.nextDueDay) : "day " + loan.nextDueDay;
            next = inDays > 0 ? $"{when}  (in {inDays} day{(inDays == 1 ? "" : "s")})" : $"{when}  (due now)";
        }
        else if (loan.open.Count > 0)
        {
            next = "None: only the open payments remain";
        }

        int missed = loan.MissedCount;
        string[] texts =
        {
            $"{loan.acres} acres, {Money.Format(loan.priceCents)}",
            Money.Format(loan.downCents),
            Money.Format(loan.originalCents),
            Money.Format(loan.balanceCents),
            $"{Money.Format(loan.paymentCents)} every {loan.terms.intervalDays} days",
            next,
            Money.Format(loan.OwedNow),
            missed.ToString(),
            Money.Format(loan.FeesOwed),
            loan.paymentsMade.ToString(),
            Money.Format(loan.interestPaid),
            Money.Format(loan.extraPaid),
            Money.Format(loan.InterestRemaining()),
        };
        for (int i = 0; i < values.Length; i++)
        {
            values[i].text = texts[i];
            values[i].color = UiKit.Cream;
        }
        const int missedRow = 7, owedRow = 6, feesRow = 8;
        if (missed > 0)
            values[missedRow].color = UiKit.Warning;
        if (loan.OwedNow > 0)
            values[owedRow].color = UiKit.Warning;
        if (loan.FeesOwed > 0)
            values[feesRow].color = UiKit.Warning;

        // The loan's own rate and late fee (frozen when it was made), and the tier's loss threshold and sell share now.
        TierTerms tierNow = money.Tier;
        string terms = $"Interest: {TierRules.PercentText(loan.terms.rateBasisPoints)} a year, fixed\n" +
                       $"Late fee: {Money.Format(loan.terms.lateFeeCents)} after {loan.terms.graceDays} days\n" +
                       $"The game is lost at {TierRules.MissedLine(tierNow.missedBeforeLoss)}";
        if (tierNow.sellBasisPoints != TierRules.FullSellBasisPoints)
            terms += $"\nTrading Post pays {TierRules.PercentText(tierNow.sellBasisPoints)} of a sale";
        termsLabel.text = terms;

        warningLabel.text = money.WarnMissed
            ? $"WARNING: {missed} missed payments are open. Pay them as soon as you can."
            : "";

        RebuildOpenPayments(loan, time);

        payDueButton.interactable = loan.OwedNow > 0 && cash > 0;
        payAmountButton.interactable = cash > 0 && loan.MaxPayableCents > 0;
    }

    void RebuildOpenPayments(LoanState loan, TimeManager time)
    {
        UiKit.Clear(openList);
        openHeading.text = loan.open.Count == 0 ? "Open payments: none" : $"Open payments ({loan.open.Count})";
        if (loan.open.Count == 0)
        {
            UiKit.Height(UiKit.Text(openList, "None", loan.IsPaidOff ? "The land is paid off." : "Nothing is due right now.", 20, UiKit.Muted), 50f);
            return;
        }

        foreach (OpenPayment p in loan.open)
        {
            Image row = UiKit.Image(openList, "Payment " + p.number, UiKit.RaisedColor);
            UiKit.Height(row.rectTransform, 74f);
            string due = time != null ? time.FormatDate(p.dueDay) : "day " + p.dueDay;
            Text head = UiKit.Text(row.rectTransform, "Head", $"Payment {p.number}  ·  due {due}", 21, UiKit.Cream, TextAnchor.UpperLeft);
            head.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            head.rectTransform.anchorMax = Vector2.one;
            head.rectTransform.offsetMin = new Vector2(12f, 0f);
            head.rectTransform.offsetMax = new Vector2(-150f, -6f);
            Text state = UiKit.Text(row.rectTransform, "State", p.missed ? "MISSED" : "owed", 20, p.missed ? UiKit.Warning : UiKit.Muted, TextAnchor.UpperRight, FontStyle.Bold);
            state.rectTransform.anchorMin = new Vector2(0.6f, 0.5f);
            state.rectTransform.anchorMax = Vector2.one;
            state.rectTransform.offsetMin = Vector2.zero;
            state.rectTransform.offsetMax = new Vector2(-12f, -6f);
            string parts = $"Late fee {Money.Format(p.fee)}  ·  Interest {Money.Format(p.interest)}  ·  Principal {Money.Format(p.principal)}";
            Text detail = UiKit.Text(row.rectTransform, "Detail", $"{parts}   =  {Money.Format(p.Owed)}", 18, p.missed ? UiKit.Warning : UiKit.Muted, TextAnchor.LowerLeft);
            detail.rectTransform.anchorMin = Vector2.zero;
            detail.rectTransform.anchorMax = new Vector2(1f, 0.5f);
            detail.rectTransform.offsetMin = new Vector2(12f, 6f);
            detail.rectTransform.offsetMax = new Vector2(-8f, 0f);
            detail.horizontalOverflow = HorizontalWrapMode.Overflow;
        }
    }

    // --- Paying ---

    void PayDue()
    {
        MoneyManager money = MoneyManager.Instance;
        if (money == null || !money.Active)
            return;

        long owed = money.OwedNow;
        if (owed <= 0)
        {
            Say("Nothing is due right now.", true);
            return;
        }

        TradingPost post = TradingPost.Instance;
        if (post == null || post.Cents <= 0)
        {
            Say("You have no cash to pay with.", true);
            return;
        }
        Report(money.PayDue(), owed);
    }

    void PayAmount()
    {
        MoneyManager money = MoneyManager.Instance;
        if (money == null || !money.Active)
            return;

        if (!Money.TryParseCents(amountField.text, out long cents) || cents <= 0)
        {
            Say("Type an amount to pay, like 250 or 250.50.", true);
            return;
        }

        TradingPost post = TradingPost.Instance;
        if (post == null || post.Cents <= 0)
        {
            Say("You have no cash to pay with.", true);
            return;
        }
        Report(money.Pay(cents), cents);
    }

    void Report(PayResult result, long asked)
    {
        if (result.applied <= 0)
        {
            Say("Nothing was paid.", true);
            return;
        }

        var parts = new List<string>();
        if (result.toFee > 0)
            parts.Add($"{Money.Format(result.toFee)} late fees");
        if (result.toInterest > 0)
            parts.Add($"{Money.Format(result.toInterest)} interest");
        if (result.toPrincipal > 0)
            parts.Add($"{Money.Format(result.toPrincipal)} principal");
        if (result.toExtra > 0)
            parts.Add($"{Money.Format(result.toExtra)} extra to principal");
        string text = $"Paid {Money.Format(result.applied)}: " + string.Join(", ", parts) + ".";
        if (result.applied < asked)
            text += $" (You asked for {Money.Format(asked)}; that is all your cash or the loan could take.)";
        if (result.paidOff)
            text += " The land is paid off!";
        amountField.text = "";
        Say(text, false);
        if (AudioManager.Instance != null)
            AudioManager.Instance.Play(SoundCue.UiClick);
    }

    void Say(string message, bool problem)
    {
        statusLabel.color = problem ? UiKit.Warning : UiKit.Cream;
        statusLabel.text = message;
    }
}
