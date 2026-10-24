using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Money_System.md Phase B's Statement tab, mounted in the Finances screen (L): the account statement on a light, paper-style panel with its
// own ink, red and paper colors (the rest of the UI is dark). A full season-by-season table, newest first, one row per season since the
// start: Season, Income, Expenses, Net, Cash at end, Loan balance at end. Under it two totals rows, This year and Since the start. Click a
// season row to see its detail on the right: income and expenses by category, the net, the memo line "Owed at end", an "Other" line when
// the ledger and the real cash disagree, per game day averages and the change from the season before. Positive figures (income, net,
// balances) are black, negative ones (expenses, a loss, what is owed) are red, zero is black. It is built when the tab opens (Show), not on
// every event: the clock is stopped while the screen is open, so the figures cannot change under it. The figures come from LedgerManager.
// The detail also shows the real minutes played in the season and the income per real hour (Money_System.md Phase D1), a dash when the
// season has no recorded real time (an older save).
public class StatementPanel
{
    static readonly Color Paper = new Color(0.96f, 0.94f, 0.86f, 1f);
    static readonly Color PaperDark = new Color(0.88f, 0.84f, 0.72f, 1f);
    static readonly Color PaperSelected = new Color(0.82f, 0.88f, 0.70f, 1f);
    static readonly Color PaperHover = new Color(0.92f, 0.89f, 0.78f, 1f);
    static readonly Color Ink = new Color(0.09f, 0.08f, 0.07f, 1f);
    static readonly Color InkMuted = new Color(0.40f, 0.36f, 0.30f, 1f);
    static readonly Color Red = new Color(0.74f, 0.11f, 0.09f, 1f);

    // Column x positions and widths inside the table (reference pixels); the numbers are right-aligned.
    static readonly float[] ColumnX = { 16f, 252f, 398f, 544f, 690f, 836f };
    static readonly float[] ColumnWidth = { 230f, 142f, 142f, 142f, 142f, 142f };
    static readonly string[] ColumnTitle = { "Season", "Income", "Expenses", "Net", "Cash at end", "Loan at end" };

    const float RowHeight = 40f;

    // Where a row's own left edge sits on the paper, so its cells line up under the header: the table starts 8 px in and the scroll list
    // pads its rows by 6; the totals rows sit directly at the 8 px.
    const float ListOrigin = 14f, TotalsOrigin = 8f;

    public RectTransform Root { get; private set; }

    RectTransform seasonList, detailList;
    Text hint;
    List<StatementRow> rows = new List<StatementRow>();
    int selectedIndex = -1;                                     // the season (LedgerSeason.index) whose detail shows
    readonly Dictionary<int, Image> rowBackgrounds = new Dictionary<int, Image>();
    RectTransform totalsArea;

    // Builds the paper panel inside parent (the Finances panel), at the given place.
    public void Build(RectTransform parent, float x, float y, float width, float height)
    {
        Image paper = UiKit.Image(parent, "Statement", Paper);
        paper.raycastTarget = true;
        Root = paper.rectTransform;
        Root.anchorMin = Root.anchorMax = Root.pivot = new Vector2(0f, 1f);
        Root.sizeDelta = new Vector2(width, height);
        Root.anchoredPosition = new Vector2(x, -y);

        // Header row.
        for (int c = 0; c < ColumnTitle.Length; c++)
        {
            Text title = UiKit.Text(Root, "Header " + ColumnTitle[c], ColumnTitle[c], 20, InkMuted, c == 0 ? TextAnchor.MiddleLeft : TextAnchor.MiddleRight, FontStyle.Bold);
            title.horizontalOverflow = HorizontalWrapMode.Overflow;
            Place(title.rectTransform, ColumnX[c], 10f, ColumnWidth[c] - (c == 0 ? 0f : 10f), 34f);
        }
        Image rule = UiKit.Image(Root, "Rule", InkMuted);
        Place(rule.rectTransform, 12f, 48f, ColumnX[5] + ColumnWidth[5] - 2f, 2f);

        // The seasons, scrolling.
        seasonList = UiKit.ScrollList(Root, "Seasons", 2f, true);
        var viewport = (RectTransform)seasonList.parent;
        Place(viewport, 8f, 54f, ColumnX[5] + ColumnWidth[5] + 4f, height - 54f - 150f);
        TintScrollList(viewport);

        // This year and Since the start, always in view under the table.
        totalsArea = UiKit.Rect("Totals", Root);
        Place(totalsArea, 8f, height - 142f, ColumnX[5] + ColumnWidth[5] + 4f, 2f * RowHeight + 6f);

        hint = UiKit.Text(Root, "Hint", "Click a season to see its detail. Figures in red are money out, or owed.", 18, InkMuted, TextAnchor.MiddleLeft);
        hint.horizontalOverflow = HorizontalWrapMode.Overflow;
        Place(hint.rectTransform, 16f, height - 52f, ColumnX[5] + ColumnWidth[5], 36f);

        // The detail pane on the right.
        Image divider = UiKit.Image(Root, "Divider", PaperDark);
        Place(divider.rectTransform, ColumnX[5] + ColumnWidth[5] + 26f, 14f, 2f, height - 28f);
        detailList = UiKit.ScrollList(Root, "Detail", 2f, true);
        var detailViewport = (RectTransform)detailList.parent;
        float detailX = ColumnX[5] + ColumnWidth[5] + 40f;
        Place(detailViewport, detailX, 10f, width - detailX - 10f, height - 20f);
        TintScrollList(detailViewport);

        Root.gameObject.SetActive(false);
    }

    // Makes UiKit's dark scroll list read on paper: no dark tint behind the rows, a thin ink scrollbar.
    static void TintScrollList(RectTransform viewport)
    {
        Image background = viewport.GetComponent<Image>();
        if (background != null)
            background.color = new Color(0f, 0f, 0f, 0.001f); // still catches the scroll wheel
        Transform bar = viewport.Find("Scrollbar");
        if (bar == null)
            return;
        Image track = bar.GetComponent<Image>();
        if (track != null)
            track.color = new Color(0f, 0f, 0f, 0.10f);
        Transform handle = bar.Find("Handle");
        Image handleImage = handle != null ? handle.GetComponent<Image>() : null;
        if (handleImage != null)
            handleImage.color = new Color(InkMuted.r, InkMuted.g, InkMuted.b, 0.75f);
    }

    static void Place(RectTransform rt, float x, float y, float width, float height)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f);
        rt.sizeDelta = new Vector2(width, height);
        rt.anchoredPosition = new Vector2(x, -y);
    }

    // --- Showing ---

    // Called when the tab opens: reads the ledger and builds every row. The newest closed season is selected (or the season in progress
    // when none has closed yet).
    public void Show()
    {
        Root.gameObject.SetActive(true);
        LedgerManager ledger = LedgerManager.Instance;
        rows = ledger != null ? ledger.Rows() : new List<StatementRow>();

        // The tab always opens on the newest closed season (the statement that is ready), or the season in progress when none has closed.
        selectedIndex = rows.Count == 0 ? -1 : rows.Count > 1 ? rows[1].index : rows[0].index;

        BuildSeasons();
        BuildTotals();
        BuildDetail();
    }

    // --- The table ---

    // A figure as text and ink: red below zero, black otherwise.
    static Color InkFor(long signedCents) => LedgerState.IsRed(signedCents) ? Red : Ink;

    void Cell(RectTransform row, int column, long signedCents, bool bold, float origin)
    {
        Text text = UiKit.Text(row, "Cell " + column, Money.Format(signedCents), 22, InkFor(signedCents), TextAnchor.MiddleRight, bold ? FontStyle.Bold : FontStyle.Normal);
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        Place(text.rectTransform, ColumnX[column] - origin, 0f, ColumnWidth[column] - 10f, RowHeight);
    }

    static string SeasonText(StatementRow row) => row.index < 0 ? "" : LedgerState.SeasonLabel(row.index) + (row.inProgress ? " (so far)" : "");

    void BuildSeasons()
    {
        UiKit.Clear(seasonList);
        rowBackgrounds.Clear();
        if (rows.Count == 0)
        {
            UiKit.Height(UiKit.Text(seasonList, "Empty", "The ledger has not started yet.", 22, InkMuted), 50f);
            return;
        }

        foreach (StatementRow row in rows)
        {
            StatementRow captured = row;
            Button button = UiKit.Button(seasonList, "Season " + row.index, "", 22, () => Select(captured.index));
            Destroy(button.GetComponentInChildren<Text>().gameObject);
            UiKit.Height(button, RowHeight);
            ColorBlock colors = button.colors;
            colors.normalColor = row.index == selectedIndex ? PaperSelected : Paper;
            colors.selectedColor = colors.normalColor;
            colors.highlightedColor = row.index == selectedIndex ? PaperSelected : PaperHover;
            colors.pressedColor = PaperSelected;
            button.colors = colors;
            rowBackgrounds[row.index] = button.GetComponent<Image>();

            var rt = (RectTransform)button.transform;
            Text label = UiKit.Text(rt, "Season", SeasonText(row), 22, row.inProgress ? InkMuted : Ink, TextAnchor.MiddleLeft, row.inProgress ? FontStyle.Italic : FontStyle.Normal);
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            Place(label.rectTransform, ColumnX[0] - ListOrigin, 0f, ColumnWidth[0], RowHeight);
            Cell(rt, 1, row.income, false, ListOrigin);
            Cell(rt, 2, Money.Negate(row.expenses), false, ListOrigin);
            Cell(rt, 3, row.Net, true, ListOrigin);
            Cell(rt, 4, row.cashEnd, false, ListOrigin);
            Cell(rt, 5, row.loanEnd, false, ListOrigin);
        }
    }

    // The columns are measured from the left edge of the table, but a row's own origin is the list's left edge.
    static void Destroy(GameObject go) => Object.Destroy(go);

    void BuildTotals()
    {
        UiKit.Clear(totalsArea);
        if (rows.Count == 0)
            return;

        int year = LedgerState.YearOf(rows[0].index);
        TotalRow(0f, $"This year (Year {year})", LedgerState.ThisYear(rows, year));
        TotalRow(RowHeight + 2f, "Since the start", LedgerState.SinceStart(rows));
    }

    void TotalRow(float y, string label, StatementRow total)
    {
        Image back = UiKit.Image(totalsArea, label, PaperDark);
        Place(back.rectTransform, 0f, y, ColumnX[5] + ColumnWidth[5] + 4f, RowHeight);
        Text title = UiKit.Text(back.rectTransform, "Label", label, 22, Ink, TextAnchor.MiddleLeft, FontStyle.Bold);
        title.horizontalOverflow = HorizontalWrapMode.Overflow;
        Place(title.rectTransform, ColumnX[0] - TotalsOrigin, 0f, ColumnWidth[0] + 40f, RowHeight);
        Cell(back.rectTransform, 1, total.income, true, TotalsOrigin);
        Cell(back.rectTransform, 2, Money.Negate(total.expenses), true, TotalsOrigin);
        Cell(back.rectTransform, 3, total.Net, true, TotalsOrigin);
        Cell(back.rectTransform, 4, total.cashEnd, true, TotalsOrigin);
        Cell(back.rectTransform, 5, total.loanEnd, true, TotalsOrigin);
    }

    void Select(int index)
    {
        selectedIndex = index;
        foreach (KeyValuePair<int, Image> pair in rowBackgrounds)
        {
            if (pair.Value == null)
                continue;
            var button = pair.Value.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = pair.Key == index ? PaperSelected : Paper;
            colors.selectedColor = colors.normalColor;
            colors.highlightedColor = pair.Key == index ? PaperSelected : PaperHover;
            button.colors = colors;
        }
        BuildDetail();
        UiKit.ReleaseFocus();
        if (AudioManager.Instance != null)
            AudioManager.Instance.Play(SoundCue.UiClick);
    }

    // --- The detail pane ---

    void BuildDetail()
    {
        UiKit.Clear(detailList);
        int at = rows.FindIndex(r => r.index == selectedIndex);
        if (at < 0)
        {
            UiKit.Height(UiKit.Text(detailList, "None", "No season yet.", 22, InkMuted), 44f);
            return;
        }

        StatementRow row = rows[at];
        Line(detailList, SeasonText(row), "", 28, true, Ink, Ink, 40f);
        Line(detailList, LedgerState.DaysText(row.minutes) + (row.inProgress ? " so far" : ""), "", 18, false, InkMuted, InkMuted, 26f);
        bool hasReal = LedgerState.HasRealTime(row.realMillis);
        long incomeHour = LedgerState.PerRealHour(row.income, row.realMillis);
        Line(detailList, "Real minutes played", LedgerState.RealMinutesText(row.realMillis), 20, false, Ink, Ink, 28f);
        Line(detailList, "Income per real hour", hasReal ? Money.Format(incomeHour) + " /hr" : "-", 20, false, Ink, hasReal ? InkFor(incomeHour) : Ink, 28f);
        Gap(detailList, 4f);
        Line(detailList, "Cash at start", Money.Format(row.cashStart), 22, false, Ink, InkFor(row.cashStart), 32f);

        Section(detailList, "Income");
        int shown = 0;
        for (int c = 0; c <= LedgerCategories.LastIncomeId; c++)
            if (row.cents != null && row.cents[c] != 0)
            {
                Line(detailList, LedgerCategories.Name((LedgerCategory)c), Money.Format(row.cents[c]), 20, false, Ink, InkFor(row.cents[c]), 28f);
                shown++;
            }
        if (shown == 0)
            Line(detailList, "none", "", 20, false, InkMuted, InkMuted, 28f);
        Line(detailList, "Total income", Money.Format(row.income), 22, true, Ink, InkFor(row.income), 32f);

        Section(detailList, "Expenses");
        shown = 0;
        for (int c = LedgerCategories.LastIncomeId + 1; c < LedgerCategories.Count; c++)
            if (row.cents != null && row.cents[c] != 0)
            {
                long shownCents = Money.Negate(row.cents[c]);
                Line(detailList, LedgerCategories.Name((LedgerCategory)c), Money.Format(shownCents), 20, false, Ink, InkFor(shownCents), 28f);
                shown++;
            }
        if (shown == 0)
            Line(detailList, "none", "", 20, false, InkMuted, InkMuted, 28f);
        long expensesShown = Money.Negate(row.expenses);
        Line(detailList, "Total expenses", Money.Format(expensesShown), 22, true, Ink, InkFor(expensesShown), 32f);

        Gap(detailList, 6f);
        Line(detailList, "Net", Money.Format(row.Net), 24, true, Ink, InkFor(row.Net), 36f);
        long owed = Money.Negate(row.owedEnd);
        Line(detailList, "Owed at end", Money.Format(owed), 20, false, Red, InkFor(owed), 28f); // the memo line, under the net
        if (row.Other != 0)
            Line(detailList, "Other (not in the ledger)", Money.Format(row.Other), 20, false, Ink, InkFor(row.Other), 28f);
        Line(detailList, "Cash at end", Money.Format(row.cashEnd), 22, true, Ink, InkFor(row.cashEnd), 32f);
        Line(detailList, "Loan balance at end", Money.Format(row.loanEnd), 20, false, Ink, InkFor(row.loanEnd), 28f);

        Section(detailList, "Per game day");
        long incomeDay = LedgerState.PerGameDay(row.income, row.minutes);
        long expensesDay = Money.Negate(LedgerState.PerGameDay(row.expenses, row.minutes));
        long netDay = LedgerState.PerGameDay(row.Net, row.minutes);
        Line(detailList, "Income", Money.Format(incomeDay) + " /day", 20, false, Ink, InkFor(incomeDay), 28f);
        Line(detailList, "Expenses", Money.Format(expensesDay) + " /day", 20, false, Ink, InkFor(expensesDay), 28f);
        Line(detailList, "Net", Money.Format(netDay) + " /day", 20, false, Ink, InkFor(netDay), 28f);

        Section(detailList, "Change from the season before");
        StatementRow before = at + 1 < rows.Count ? rows[at + 1] : default;
        bool hasBefore = at + 1 < rows.Count && before.minutes > 0;
        if (!hasBefore)
        {
            Line(detailList, "No earlier season to compare.", "", 20, false, InkMuted, InkMuted, 28f);
        }
        else
        {
            ChangeLine("Income", incomeDay, LedgerState.PerGameDay(before.income, before.minutes));
            ChangeLine("Expenses", expensesDay, Money.Negate(LedgerState.PerGameDay(before.expenses, before.minutes)));
            ChangeLine("Net", netDay, LedgerState.PerGameDay(before.Net, before.minutes));
            Line(detailList, "per game day, expenses shown as a loss", "", 16, false, InkMuted, InkMuted, 24f);
        }
        Gap(detailList, 10f);
    }

    void ChangeLine(string label, long nowPerDay, long beforePerDay)
    {
        LedgerChange change = LedgerState.ChangeFrom(nowPerDay, beforePerDay);
        string text = (change.cents > 0 ? "+" : "") + Money.Format(change.cents) + (change.hasPercent ? "  (" + PercentText(change.basisPoints) + ")" : "");
        Line(detailList, label, text, 20, false, Ink, InkFor(change.cents), 28f);
    }

    // Basis points as a percentage to one decimal, signed: 1250 is "+12.5%", -3333 is "-33.3%", 0 is "0%".
    public static string PercentText(int basisPoints)
    {
        if (basisPoints == 0)
            return "0%";
        int abs = System.Math.Abs(basisPoints);
        string number = abs % 100 < 10 ? $"{abs / 100}" : $"{abs / 100}.{abs % 100 / 10}";
        return (basisPoints > 0 ? "+" : "-") + number + "%";
    }

    static void Section(RectTransform list, string heading)
    {
        Gap(list, 8f);
        Line(list, heading, "", 24, true, InkMuted, InkMuted, 34f);
    }

    static void Gap(RectTransform list, float height)
    {
        RectTransform gap = UiKit.Rect("Gap", list);
        UiKit.Height(gap, height);
    }

    // One line: a label on the left and a figure on the right, with its own ink.
    static void Line(RectTransform list, string label, string value, int size, bool bold, Color labelInk, Color valueInk, float height)
    {
        RectTransform row = UiKit.Rect("Line", list);
        UiKit.Height(row, height);
        Text left = UiKit.Text(row, "Label", label, size, labelInk, TextAnchor.MiddleLeft, bold ? FontStyle.Bold : FontStyle.Normal);
        left.horizontalOverflow = HorizontalWrapMode.Overflow;
        left.rectTransform.anchorMin = Vector2.zero;
        left.rectTransform.anchorMax = new Vector2(0.62f, 1f);
        left.rectTransform.offsetMin = new Vector2(4f, 0f);
        left.rectTransform.offsetMax = Vector2.zero;
        if (string.IsNullOrEmpty(value))
            return;
        Text right = UiKit.Text(row, "Value", value, size, valueInk, TextAnchor.MiddleRight, bold ? FontStyle.Bold : FontStyle.Normal);
        right.horizontalOverflow = HorizontalWrapMode.Overflow;
        right.rectTransform.anchorMin = new Vector2(0.38f, 0f);
        right.rectTransform.anchorMax = Vector2.one;
        right.rectTransform.offsetMin = Vector2.zero;
        right.rectTransform.offsetMax = new Vector2(-12f, 0f);
    }
}
