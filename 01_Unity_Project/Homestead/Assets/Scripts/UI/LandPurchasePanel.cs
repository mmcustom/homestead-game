using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Money_System.md Phase A's land purchase, between the difficulty step and the slot pick at New Game. Three steps in one overlay,
// built in code in the game's UI look like the difficulty panel:
//   Land    - choose one of the three starter parcels (LandRules.Starters), with price, price per acre and the payment preview.
//   Split   - split the savings (tier percentage of the price, capped) between a down payment and a cash reserve. Skipped for a
//             tier with no savings (Pioneer), where the full price is financed.
//   Summary - the loan: amount, rate, number and spacing of payments, the payment, the first due date and the total interest if no
//             extra payments are made. Confirm hands the choice back; Back (or Esc) steps back, and from the first step closes.
// All the numbers come from LandRules and LoanState; this only shows them.
public class LandPurchasePanel : MonoBehaviour
{
    enum Step { Land, Split, Summary }

    const float PanelWidth = 1200f, PanelHeight = 700f;

    Difficulty tier;
    LoanTerms terms;
    Step step = Step.Land;
    int parcel;
    long savings, down;
    Action<LandChoice> onConfirmed;
    Action onBack;

    RectTransform landGroup, splitGroup, summaryGroup;
    Text titleLabel, noteLabel, splitProblem, savingsText;
    readonly Button[] cards = new Button[3];
    readonly Text[] splitValues = new Text[4];
    readonly Text[] summaryValues = new Text[10];
    InputField downField;
    Button backButton, nextButton;
    bool settingField;

    static readonly string[] SummaryLabels =
    {
        "Land", "Price", "Down payment", "Cash reserve", "Loan amount", "Interest", "Payments", "Each payment",
        "First payment due", "Interest if you make no extra payments",
    };

    // underCanvas is anything inside the canvas to cover. resume (coming back from the slot pick) reopens on the summary of that choice.
    public static LandPurchasePanel Open(Transform underCanvas, Difficulty tier, LandChoice? resume,
                                         Action<LandChoice> onConfirmed, Action onBack)
    {
        Canvas canvas = underCanvas.GetComponentInParent<Canvas>();
        Transform parent = canvas != null ? canvas.rootCanvas.transform : underCanvas;

        Image shade = UiKit.Image(parent, "Land Purchase", new Color(0f, 0f, 0f, 0.6f));
        shade.rectTransform.Fill();
        shade.raycastTarget = true;
        var overlay = shade.gameObject.AddComponent<Canvas>();
        overlay.overrideSorting = true;
        overlay.sortingOrder = 120;
        shade.gameObject.AddComponent<GraphicRaycaster>();

        var panel = shade.gameObject.AddComponent<LandPurchasePanel>();
        panel.tier = tier;
        panel.terms = MoneyManager.Instance != null ? MoneyManager.Instance.Terms : LoanTerms.Default;
        panel.onConfirmed = onConfirmed;
        panel.onBack = onBack;
        panel.Build();
        if (resume.HasValue)
        {
            panel.parcel = Mathf.Clamp(resume.Value.parcelIndex, 0, LandRules.Starters.Length - 1);
            panel.savings = panel.SavingsFor(panel.parcel);
            panel.down = resume.Value.downCents;
            panel.GoTo(Step.Summary);
        }
        else
        {
            panel.parcel = 0;
            panel.GoTo(Step.Land);
        }
        return panel;
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            Back();
    }

    // --- Numbers ---

    Parcel Selected => LandRules.Starters[parcel];

    long SavingsFor(int index) => LandRules.SavingsCents(LandRules.Starters[index].priceCents, DifficultyManager.SavingsPercent(tier));

    bool HasSavings => savings > 0;

    long LoanFor(long downCents) => Selected.priceCents - downCents;

    long PaymentFor(long downCents) => LoanState.PaymentFor(LoanFor(downCents), terms);

    static string Plain(long cents) => Money.Format(cents).Replace("$", "").Replace(",", "");

    // --- Layout ---

    void Build()
    {
        Color solid = UiKit.PanelColor;
        solid.a = 1f;
        Image panelImage = UiKit.Image(transform, "Panel", solid);
        panelImage.raycastTarget = true;
        RectTransform panel = panelImage.rectTransform;
        panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.sizeDelta = new Vector2(PanelWidth, PanelHeight);

        titleLabel = UiKit.Text(panel, "Title", "", 34, UiKit.Accent, TextAnchor.MiddleLeft, FontStyle.Italic);
        Place(titleLabel.rectTransform, 40f, 20f, PanelWidth - 80f, 54f);

        landGroup = Group(panel, "Land");
        splitGroup = Group(panel, "Split");
        summaryGroup = Group(panel, "Summary");
        BuildLand();
        BuildSplit();
        BuildSummary();

        backButton = UiKit.Button(panel, "Back", "Back", 24, () => { Play(SoundCue.UiBack); Back(); });
        Place((RectTransform)backButton.transform, 40f, PanelHeight - 80f, 200f, 56f);
        nextButton = UiKit.Button(panel, "Next", "Next", 26, () => { Play(SoundCue.UiClick); Next(); });
        Place((RectTransform)nextButton.transform, PanelWidth - 40f - 240f, PanelHeight - 80f, 240f, 56f);
        noteLabel = UiKit.Text(panel, "Note", "", 20, UiKit.Muted, TextAnchor.MiddleLeft);
        Place(noteLabel.rectTransform, 270f, PanelHeight - 80f, PanelWidth - 270f - 300f, 56f);
    }

    RectTransform Group(RectTransform parent, string name)
    {
        RectTransform group = UiKit.Rect(name, parent);
        group.Fill();
        return group;
    }

    void BuildLand()
    {
        for (int i = 0; i < cards.Length; i++)
        {
            int index = i;
            cards[i] = UiKit.Button(landGroup, "Parcel " + i, "", 21, () => { parcel = index; Play(SoundCue.UiClick); RefreshLand(); });
            Place((RectTransform)cards[i].transform, 40f + i * 385f, 100f, 370f, 440f);
        }
    }

    void BuildSplit()
    {
        savingsText = UiKit.Text(splitGroup, "Savings", "", 28, UiKit.Cream, TextAnchor.MiddleLeft);
        Place(savingsText.rectTransform, 40f, 96f, PanelWidth - 80f, 44f);

        Text prompt = UiKit.Text(splitGroup, "Prompt", "How much goes to the down payment? The rest is your cash reserve.", 22, UiKit.Muted, TextAnchor.MiddleLeft);
        Place(prompt.rectTransform, 40f, 150f, PanelWidth - 80f, 36f);

        Text amountLabel = UiKit.Text(splitGroup, "Down Label", "Down payment:", 24, UiKit.Cream, TextAnchor.MiddleLeft);
        Place(amountLabel.rectTransform, 40f, 200f, 220f, 56f);
        downField = UiKit.Input(splitGroup, "Down", "0.00", 26, false);
        downField.characterLimit = 20;
        Place((RectTransform)downField.transform, 270f, 200f, 260f, 56f);
        downField.onValueChanged.AddListener(_ => { if (!settingField) UpdateSplit(); });

        string[] names = { "All down", "Half and half", "None down" };
        for (int i = 0; i < names.Length; i++)
        {
            int preset = i;
            Button b = UiKit.Button(splitGroup, names[i], names[i], 20, () => { Play(SoundCue.UiClick); SetDown(preset == 0 ? savings : preset == 1 ? LandRules.DefaultDown(savings) : 0); });
            Place((RectTransform)b.transform, 560f + i * 190f, 200f, 180f, 56f);
        }

        splitProblem = UiKit.Text(splitGroup, "Problem", "", 22, UiKit.Warning, TextAnchor.MiddleLeft);
        Place(splitProblem.rectTransform, 40f, 268f, PanelWidth - 80f, 36f);

        string[] labels = { "Down payment", "Cash reserve", "Loan amount", "Payment every " + terms.intervalDays + " days" };
        for (int i = 0; i < labels.Length; i++)
        {
            Text label = UiKit.Text(splitGroup, labels[i], labels[i], 24, UiKit.Muted, TextAnchor.MiddleLeft);
            Place(label.rectTransform, 40f, 330f + i * 52f, 380f, 48f);
            splitValues[i] = UiKit.Text(splitGroup, labels[i] + " Value", "", 26, UiKit.Cream, TextAnchor.MiddleLeft);
            splitValues[i].horizontalOverflow = HorizontalWrapMode.Overflow;
            Place(splitValues[i].rectTransform, 440f, 330f + i * 52f, 400f, 48f);
        }
    }

    void BuildSummary()
    {
        for (int i = 0; i < SummaryLabels.Length; i++)
        {
            Text label = UiKit.Text(summaryGroup, SummaryLabels[i], SummaryLabels[i], 24, UiKit.Muted, TextAnchor.MiddleLeft);
            Place(label.rectTransform, 40f, 96f + i * 46f, 520f, 44f);
            summaryValues[i] = UiKit.Text(summaryGroup, SummaryLabels[i] + " Value", "", 26, UiKit.Cream, TextAnchor.MiddleLeft);
            summaryValues[i].horizontalOverflow = HorizontalWrapMode.Overflow;
            Place(summaryValues[i].rectTransform, 580f, 96f + i * 46f, 580f, 44f);
        }
    }

    static void Place(RectTransform rt, float x, float y, float width, float height)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
        rt.sizeDelta = new Vector2(width, height);
        rt.anchoredPosition = new Vector2(x, -y);
    }

    static void Play(SoundCue cue)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.Play(cue);
    }

    // --- Steps ---

    void GoTo(Step next)
    {
        step = next;
        landGroup.gameObject.SetActive(step == Step.Land);
        splitGroup.gameObject.SetActive(step == Step.Split);
        summaryGroup.gameObject.SetActive(step == Step.Summary);

        switch (step)
        {
            case Step.Land:
                titleLabel.text = "Choose your land";
                nextButton.GetComponentInChildren<Text>().text = "Next";
                RefreshLand();
                break;
            case Step.Split:
                titleLabel.text = "Your savings";
                nextButton.GetComponentInChildren<Text>().text = "Next";
                savings = SavingsFor(parcel);
                SetDown(down); // Next set the half-and-half default; coming back from the summary keeps what was chosen
                break;
            case Step.Summary:
                titleLabel.text = "Your land loan";
                nextButton.GetComponentInChildren<Text>().text = "Confirm";
                noteLabel.text = "";
                RefreshSummary();
                break;
        }
    }

    void Next()
    {
        if (step == Step.Land)
        {
            savings = SavingsFor(parcel);
            if (HasSavings)
            {
                down = LandRules.DefaultDown(savings);
                GoTo(Step.Split);
            }
            else
            {
                down = 0;
                GoTo(Step.Summary);
            }
        }
        else if (step == Step.Split)
        {
            if (LandRules.ValidDown(savings, down))
                GoTo(Step.Summary);
        }
        else
        {
            LandChoice choice = LandRules.Choose(parcel, savings, down);
            Action<LandChoice> confirmed = onConfirmed;
            Destroy(gameObject);
            confirmed?.Invoke(choice);
        }
    }

    void Back()
    {
        if (step == Step.Summary)
            GoTo(HasSavings ? Step.Split : Step.Land);
        else if (step == Step.Split)
            GoTo(Step.Land);
        else
        {
            Action back = onBack;
            Destroy(gameObject);
            back?.Invoke();
        }
    }

    // --- Land step ---

    void RefreshLand()
    {
        for (int i = 0; i < cards.Length; i++)
        {
            Parcel p = LandRules.Starters[i];
            long save = SavingsFor(i);
            string text = $"<size=30><b>{p.name}</b></size>\n{Money.Format(p.priceCents)}\n{Money.Format(p.PricePerAcreCents)} per acre\n\n";
            string every = "Payment every " + terms.intervalDays + " days";
            if (save > 0)
            {
                text += $"Your savings: {Money.Format(save)}\n\n{every}:\n<b>{Money.Format(LoanState.PaymentFor(p.priceCents - save, terms))}</b> with all savings down\n" +
                        $"<b>{Money.Format(LoanState.PaymentFor(p.priceCents, terms))}</b> with none down";
            }
            else
            {
                text += $"No savings: fully financed\n\n{every}:\n<b>{Money.Format(LoanState.PaymentFor(p.priceCents, terms))}</b>";
            }
            cards[i].GetComponentInChildren<Text>().text = text;
            UiKit.SetSelected(cards[i], i == parcel);
        }

        int percent = DifficultyManager.SavingsPercent(tier);
        noteLabel.text = percent > 0
            ? $"{DifficultyManager.Name(tier)}: savings of {percent}% of the price, up to {Money.Format(LandRules.SavingsCapCents)}."
            : $"{DifficultyManager.Name(tier)}: no savings. The whole price is borrowed.";
    }

    // --- Split step ---

    void SetDown(long cents)
    {
        settingField = true;
        downField.text = Plain(cents);
        settingField = false;
        UpdateSplit();
    }

    void UpdateSplit()
    {
        noteLabel.text = "";
        bool ok = Money.TryParseCents(downField.text, out long typed) && LandRules.ValidDown(savings, typed);
        if (ok)
        {
            down = typed;
            splitProblem.text = "";
        }
        else
        {
            splitProblem.text = string.IsNullOrWhiteSpace(downField.text) || !Money.TryParseCents(downField.text, out _)
                ? "Type an amount, like 12000 or 12000.50."
                : $"The down payment can be at most your savings, {Money.Format(savings)}.";
        }

        savingsText.text = $"Your savings: {Money.Format(savings)}";
        splitValues[0].text = Money.Format(ok ? down : 0);
        splitValues[1].text = ok ? Money.Format(LandRules.Reserve(savings, down)) : "";
        splitValues[2].text = ok ? Money.Format(LoanFor(down)) : "";
        splitValues[3].text = ok ? Money.Format(PaymentFor(down)) : "";
        nextButton.interactable = ok;
    }

    // --- Summary step ---

    void RefreshSummary()
    {
        long loan = LoanFor(down);
        long payment = LoanState.PaymentFor(loan, terms);
        TimeManager time = TimeManager.Instance;
        string first = time != null ? $"{time.FormatDate(terms.firstDueDay)}  (day {terms.firstDueDay})" : $"day {terms.firstDueDay}";

        string[] texts =
        {
            Selected.name,
            Money.Format(Selected.priceCents),
            Money.Format(down),
            Money.Format(LandRules.Reserve(savings, down)),
            Money.Format(loan),
            $"{terms.rateBasisPoints / 100m:0.##}% a year, fixed",
            $"{terms.payments}, one every {terms.intervalDays} days",
            Money.Format(payment),
            first,
            Money.Format(LoanState.TotalInterest(loan, terms)),
        };
        for (int i = 0; i < summaryValues.Length; i++)
            summaryValues[i].text = texts[i];
    }
}
