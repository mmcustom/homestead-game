using UnityEngine;
using UnityEngine.UI;

// The survival meters, bottom-left of the HUD, always shown (top to bottom): Health, Stamina, Hydration and Hunger
// (Core_Survival_System.md, First_Person_Controller.md). Each fill is sized by anchoring rather than an Image fill,
// so it needs no sprite. Health, Hydration and Hunger turn to the warning colour and pulse below 25 (Hydration's
// Severe tier and its mirrors) — the row's label pulses too, so an empty bar still shows its warning. Stamina is measured against its full size: when low Hydration or Hunger caps it, the
// part it can't refill into is shaded, so the player can see why the bar stops short.
public class SurvivalHud : MonoBehaviour
{
    [SerializeField] RectTransform hydrationFill;
    [SerializeField] RectTransform hungerFill;
    [SerializeField] RectTransform healthFill;
    [SerializeField] RectTransform staminaFill;
    [Tooltip("Shaded over the stretch of the stamina bar that low Hydration or Hunger won't let it refill into.")]
    [SerializeField] RectTransform staminaCap;

    [Header("Colours")]
    [SerializeField] Color hydrationColor = new Color(0.36f, 0.62f, 0.78f);
    [SerializeField] Color hungerColor = new Color(0.84f, 0.63f, 0.3f);
    [SerializeField] Color healthColor = new Color(0.72f, 0.3f, 0.24f);
    [SerializeField] Color staminaColor = new Color(0.62f, 0.74f, 0.4f);
    [SerializeField] Color warningColor = new Color(0.9f, 0.3f, 0.22f);

    [SerializeField, Min(0.1f)] float fillSmoothing = 8f;

    Image hydrationImage, hungerImage, healthImage, staminaImage;
    Text hydrationLabel, hungerLabel, healthLabel;
    Color labelColor;
    float shownHydration = 1f, shownHunger = 1f, shownHealth = 1f, shownStamina = 1f, shownCap = 1f;
    CanvasGroup group;
    PlayerController player;
    float nextPlayerSearch;

    void Awake()
    {
        hydrationImage = hydrationFill.GetComponent<Image>();
        hungerImage = hungerFill.GetComponent<Image>();
        healthImage = healthFill.GetComponent<Image>();
        staminaImage = staminaFill.GetComponent<Image>();
        hydrationLabel = LabelOf(hydrationFill);
        hungerLabel = LabelOf(hungerFill);
        healthLabel = LabelOf(healthFill);
        labelColor = healthLabel != null ? healthLabel.color : Color.white;
        group = GetComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        // Start at the real values rather than sliding up from full.
        SurvivalManager survival = SurvivalManager.Instance;
        if (survival != null)
        {
            shownHydration = survival.Hydration / SurvivalManager.MaxValue;
            shownHunger = survival.Hunger / SurvivalManager.MaxValue;
            shownHealth = survival.Health / SurvivalManager.MaxValue;
        }
    }

    void Update()
    {
        SurvivalManager survival = SurvivalManager.Instance;
        if (group != null)
            group.alpha = survival != null ? 1f : 0f;
        if (survival == null)
            return;

        if (player == null && Time.unscaledTime >= nextPlayerSearch)
        {
            nextPlayerSearch = Time.unscaledTime + 1f;
            player = FindAnyObjectByType<PlayerController>();
        }

        float k = 1f - Mathf.Exp(-fillSmoothing * Time.unscaledDeltaTime);
        shownHydration = Mathf.Lerp(shownHydration, survival.Hydration / SurvivalManager.MaxValue, k);
        shownHunger = Mathf.Lerp(shownHunger, survival.Hunger / SurvivalManager.MaxValue, k);
        shownHealth = Mathf.Lerp(shownHealth, survival.Health / SurvivalManager.MaxValue, k);
        if (player != null)
        {
            // Stamina moves fast (sprints, jumps), so it follows more tightly than the slow survival stats.
            float kFast = 1f - Mathf.Exp(-fillSmoothing * 2f * Time.unscaledDeltaTime);
            shownStamina = Mathf.Lerp(shownStamina, player.Stamina / player.MaxStamina, kFast);
            shownCap = Mathf.Lerp(shownCap, player.EffectiveMaxStamina / player.MaxStamina, k);
        }

        float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 6f);
        Apply(hydrationFill, hydrationImage, hydrationLabel, shownHydration, hydrationColor, survival.Hydration < 25f, pulse);
        Apply(hungerFill, hungerImage, hungerLabel, shownHunger, hungerColor, survival.Hunger < 25f, pulse);
        Apply(healthFill, healthImage, healthLabel, shownHealth, healthColor, survival.Health < 25f, pulse);
        Apply(staminaFill, staminaImage, null, shownStamina, staminaColor, false, pulse);

        staminaCap.anchorMin = new Vector2(Mathf.Clamp01(shownCap), 0f);
        staminaCap.gameObject.SetActive(shownCap < 0.995f);
    }

    void Apply(RectTransform fill, Image image, Text label, float value, Color normal, bool low, float pulse)
    {
        fill.anchorMax = new Vector2(Mathf.Clamp01(value), 1f);
        Color warning = Color.Lerp(warningColor, Color.Lerp(warningColor, Color.white, 0.35f), pulse);
        image.color = low ? warning : normal;
        if (label != null)
            label.color = low ? warning : labelColor;
    }

    // The row's name label: Fill sits in the row's Track, beside its Label.
    static Text LabelOf(RectTransform fill)
    {
        Transform row = fill.parent != null ? fill.parent.parent : null;
        Transform label = row != null ? row.Find("Label") : null;
        return label != null ? label.GetComponent<Text>() : null;
    }
}
