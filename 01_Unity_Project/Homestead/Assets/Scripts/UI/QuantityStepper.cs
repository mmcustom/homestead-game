using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Quantity_Controls.md's shared control, built in code in the screens' look: a minus button, an amount box, a plus button and Max.
// Minus and plus change the amount by 1, by 10 with Shift held; Max sets the most that can move; a typed amount works and is kept in
// range (QuantityRules). The mouse alone is enough: nothing needs the keyboard. The screen that owns it works out the most that can move
// (cash, carry weight, room, what is carried) and calls SetMax; if that is 0 the whole control is greyed out. The amount lives in this
// object, not in the screen's rows, so a screen that rebuilds its rows keeps each item's amount in a dictionary and hands it back to
// Create each time. Focus: the amount box gives up the keyboard when typing ends (Enter, or clicking anything else) and after every
// button click, so the screen hotkeys work again. Esc while typing only ends the typing (GameScreens.NoteEscapeEndedTyping).
public sealed class QuantityStepper
{
    public const float Width = 230f;
    public const float Height = 34f;

    // The buttons stand out from the row behind them (the row and UiKit's buttons share one colour).
    static readonly Color ControlColor = new Color(0.25f, 0.32f, 0.21f, 1f);

    public RectTransform Root { get; private set; }

    // The amount shown: 1 to Max, or 0 when nothing can move.
    public int Amount { get; private set; }
    public int Max { get; private set; }

    // The player changed the amount (a click or a typed number). Not raised by SetMax.
    public event Action Changed;

    Button minus, plus, maxButton;
    InputField field;
    Text placeholder;
    FocusDriver driver;

    // Lives on the stepper's root so it can act on a later frame.
    sealed class FocusDriver : MonoBehaviour
    {
        public InputField field;
        public bool releasePending;

        void LateUpdate()
        {
            if (!releasePending)
                return;
            releasePending = false;
            if (field != null && EventSystem.current != null && EventSystem.current.currentSelectedGameObject == field.gameObject)
                UiKit.ReleaseFocus();
        }
    }

    QuantityStepper() { }

    public static QuantityStepper Create(Transform parent, int amount, int max)
    {
        var stepper = new QuantityStepper { Max = Mathf.Max(0, max) };
        stepper.Amount = QuantityRules.Clamp(amount, stepper.Max);

        RectTransform root = UiKit.Rect("Stepper", parent);
        stepper.Root = root;
        root.sizeDelta = new Vector2(Width, Height);

        stepper.driver = root.gameObject.AddComponent<FocusDriver>();
        stepper.minus = UiKit.Button(root, "Minus", "-", 24, () => stepper.Nudge(-1));
        Style(stepper.minus);
        Place((RectTransform)stepper.minus.transform, 0f, 36f);

        stepper.field = UiKit.Input(root, "Amount", "1", 20, false);
        Place((RectTransform)stepper.field.transform, 42f, 72f);
        stepper.field.characterLimit = QuantityRules.MaxDigits;
        stepper.field.textComponent.alignment = TextAnchor.MiddleCenter;
        stepper.field.textComponent.rectTransform.Fill(4f, 2f, 4f, 2f);
        stepper.placeholder = (Text)stepper.field.placeholder;
        stepper.placeholder.alignment = TextAnchor.MiddleCenter;
        stepper.placeholder.fontStyle = FontStyle.Normal;
        stepper.placeholder.rectTransform.Fill(4f, 2f, 4f, 2f);
        stepper.driver.field = stepper.field;
        stepper.field.onValidateInput = (text, index, c) => c >= '0' && c <= '9' ? c : '\0'; // digits only, pasted text included
        stepper.field.onEndEdit.AddListener(stepper.EndEdit);

        // Clicking the box clears it, so typing replaces the number instead of adding to it (the old number shows greyed in the box).
        var trigger = stepper.field.gameObject.AddComponent<EventTrigger>();
        var select = new EventTrigger.Entry { eventID = EventTriggerType.Select };
        select.callback.AddListener(_ => stepper.field.text = "");
        trigger.triggers.Add(select);

        stepper.plus = UiKit.Button(root, "Plus", "+", 24, () => stepper.Nudge(1));
        Style(stepper.plus);
        Place((RectTransform)stepper.plus.transform, 120f, 36f);

        stepper.maxButton = UiKit.Button(root, "Max", "Max", 18, stepper.PressMax);
        Style(stepper.maxButton);
        Place((RectTransform)stepper.maxButton.transform, 162f, 68f);

        stepper.Refresh();
        return stepper;
    }

    // A button that shows against the row: UiKit.Button insets its label 12 px each side, which leaves a 36 px button's "+" no room
    // (it is not drawn at all), so the label gets a small margin and may overflow; a tint sets it apart from the row.
    public static void Style(Button button)
    {
        Text label = button.GetComponentInChildren<Text>();
        label.rectTransform.Fill(2f, 0f, 2f, 0f);
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        ColorBlock colors = button.colors;
        colors.normalColor = ControlColor;
        colors.selectedColor = ControlColor;
        colors.highlightedColor = UiKit.SelectedColor;
        colors.disabledColor = new Color(ControlColor.r, ControlColor.g, ControlColor.b, 0.35f);
        button.colors = colors;
    }

    // Enables or greys out a button, dimming its label too (a Button only dims its background).
    public static void SetEnabled(Button button, bool enabled)
    {
        button.interactable = enabled;
        Text label = button.GetComponentInChildren<Text>();
        if (label != null)
            label.color = enabled ? UiKit.Cream : UiKit.Muted;
    }

    static void Place(RectTransform rt, float x, float width)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 0.5f);
        rt.sizeDelta = new Vector2(width, Height);
        rt.anchoredPosition = new Vector2(x, 0f);
    }

    // The most that can move has changed (a refresh); keeps the amount in range without raising Changed.
    public void SetMax(int max)
    {
        Max = Mathf.Max(0, max);
        Amount = QuantityRules.Clamp(Amount == 0 ? 1 : Amount, Max);
        Refresh();
    }

    void Nudge(int sign)
    {
        Apply(QuantityRules.Step(Amount, sign * QuantityRules.StepFor(UiKit.ShiftHeld), Max));
        UiKit.ReleaseFocus();
    }

    void PressMax()
    {
        Apply(Max);
        UiKit.ReleaseFocus();
    }

    void EndEdit(string text)
    {
        UnityEngine.InputSystem.Keyboard keyboard = UnityEngine.InputSystem.Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            GameScreens.NoteEscapeEndedTyping();

        Apply(QuantityRules.ParseAndClamp(text, Amount, Max)); // nonsense, an empty box or a cancelled edit keeps the amount

        // Let go of the keyboard, but not from inside this callback: it also runs while the EventSystem is deselecting the box (the
        // player clicked something else), and selecting then is an error. The driver does it a frame later, if the box still holds it.
        if (driver != null)
            driver.releasePending = true;
    }

    void Apply(int amount)
    {
        bool changed = amount != Amount;
        Amount = amount;
        Refresh();
        if (changed)
            Changed?.Invoke();
    }

    void Refresh()
    {
        bool can = Max > 0;
        string text = Amount.ToString();
        field.SetTextWithoutNotify(text);
        placeholder.text = text;
        field.interactable = can;
        SetEnabled(minus, can && Amount > 1);
        SetEnabled(plus, can && Amount < Max);
        SetEnabled(maxButton, can && Amount < Max);
    }
}

// One two-line row for a screen that moves items by count (Trading Post, Storage): line one is the name (with its small notes), an
// optional trend marker and the price; line two is the stepper, the live total and the action button. The screen fills in the texts and
// reads the stepper. Plain click moves the stepper's amount; Shift-click is the old shortcut (the whole stack, or ten), so the action
// callback is told whether Shift was held.
public sealed class QuantityRowUi
{
    public const float RowHeight = 84f;

    public RectTransform Row { get; private set; }
    public Text Name { get; private set; }
    public Text Trend { get; private set; }
    public Text Price { get; private set; }
    public Text Total { get; private set; }
    public QuantityStepper Stepper { get; private set; }
    public Button Action { get; private set; }

    QuantityRowUi() { }

    public static QuantityRowUi Create(RectTransform list, string actionLabel, bool withPrice, int amount, int max, UnityAction<bool> onAction)
    {
        var ui = new QuantityRowUi();
        RectTransform row = UiKit.Image(list, "Row", UiKit.RaisedColor).rectTransform;
        UiKit.Height(row, RowHeight);
        ui.Row = row;

        ui.Name = UiKit.Text(row, "Name", "", 20, UiKit.Cream, TextAnchor.MiddleLeft);
        ui.Name.horizontalOverflow = HorizontalWrapMode.Overflow;
        Region(ui.Name.rectTransform, 0f, 0.5f, withPrice ? 0.62f : 1f, 1f, 12f, 0f);

        if (withPrice)
        {
            ui.Trend = UiKit.Text(row, "Trend", "", 26, UiKit.Cream, TextAnchor.MiddleCenter, FontStyle.Bold);
            Region(ui.Trend.rectTransform, 0.62f, 0.5f, 0.68f, 1f, 0f, 0f);
            ui.Price = UiKit.Text(row, "Price", "", 20, UiKit.Cream, TextAnchor.MiddleRight);
            Region(ui.Price.rectTransform, 0.68f, 0.5f, 1f, 1f, 0f, 12f);
        }

        ui.Stepper = QuantityStepper.Create(row, amount, max);
        RectTransform stepperRt = ui.Stepper.Root;
        stepperRt.anchorMin = stepperRt.anchorMax = stepperRt.pivot = new Vector2(0f, 0f);
        stepperRt.anchoredPosition = new Vector2(12f, 5f);

        ui.Total = UiKit.Text(row, "Total", "", 18, UiKit.Cream, TextAnchor.MiddleLeft);
        ui.Total.horizontalOverflow = HorizontalWrapMode.Overflow;
        Region(ui.Total.rectTransform, 0f, 0f, 1f, 0.5f, 254f, 130f);

        ui.Action = UiKit.Button(row, actionLabel, actionLabel, 18, () =>
        {
            UnityAction<bool> act = onAction;
            bool shift = UiKit.ShiftHeld;
            UiKit.ReleaseFocus();
            act?.Invoke(shift);
        });
        QuantityStepper.Style(ui.Action);
        var actionRt = (RectTransform)ui.Action.transform;
        actionRt.anchorMin = actionRt.anchorMax = actionRt.pivot = new Vector2(1f, 0f);
        actionRt.sizeDelta = new Vector2(112f, QuantityStepper.Height);
        actionRt.anchoredPosition = new Vector2(-8f, 5f);
        return ui;
    }

    // Enables or greys out the action button (its label dims too).
    public void SetActionEnabled(bool enabled) => QuantityStepper.SetEnabled(Action, enabled);

    // Anchored to part of the row, with a left and a right margin in pixels.
    static void Region(RectTransform rt, float xMin, float yMin, float xMax, float yMax, float left, float right)
    {
        rt.anchorMin = new Vector2(xMin, yMin);
        rt.anchorMax = new Vector2(xMax, yMax);
        rt.offsetMin = new Vector2(left, 0f);
        rt.offsetMax = new Vector2(-right, 0f);
    }
}
