using UnityEngine;
using UnityEngine.UI;

// Hydration and Hunger meters (Core_Survival_System.md), bottom-left of the HUD. Each fill is sized by anchoring
// rather than an Image fill, so it needs no sprite. A meter in the Severe tier (below 25) turns to its warning colour
// and pulses. Health only appears once it drops below full, so the HUD stays quiet while the player is healthy.
public class SurvivalHud : MonoBehaviour
{
    [SerializeField] RectTransform hydrationFill;
    [SerializeField] RectTransform hungerFill;
    [SerializeField] RectTransform healthFill;
    [Tooltip("The Health row, faded in only while Health is below full.")]
    [SerializeField] CanvasGroup healthRow;

    [Header("Colours")]
    [SerializeField] Color hydrationColor = new Color(0.36f, 0.62f, 0.78f);
    [SerializeField] Color hungerColor = new Color(0.84f, 0.63f, 0.3f);
    [SerializeField] Color healthColor = new Color(0.72f, 0.3f, 0.24f);
    [SerializeField] Color warningColor = new Color(0.9f, 0.3f, 0.22f);

    [SerializeField, Min(0.1f)] float fillSmoothing = 8f;
    [SerializeField, Min(0.1f)] float healthFadeSpeed = 3f;

    Image hydrationImage, hungerImage, healthImage;
    float shownHydration = 1f, shownHunger = 1f, shownHealth = 1f;
    CanvasGroup group;

    void Awake()
    {
        hydrationImage = hydrationFill.GetComponent<Image>();
        hungerImage = hungerFill.GetComponent<Image>();
        healthImage = healthFill.GetComponent<Image>();
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
            healthRow.alpha = shownHealth < 0.995f ? 1f : 0f;
        }
    }

    void Update()
    {
        SurvivalManager survival = SurvivalManager.Instance;
        if (group != null)
            group.alpha = survival != null ? 1f : 0f;
        if (survival == null)
            return;

        float k = 1f - Mathf.Exp(-fillSmoothing * Time.unscaledDeltaTime);
        shownHydration = Mathf.Lerp(shownHydration, survival.Hydration / SurvivalManager.MaxValue, k);
        shownHunger = Mathf.Lerp(shownHunger, survival.Hunger / SurvivalManager.MaxValue, k);
        shownHealth = Mathf.Lerp(shownHealth, survival.Health / SurvivalManager.MaxValue, k);

        float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 6f);
        Apply(hydrationFill, hydrationImage, shownHydration, hydrationColor, survival.Hydration < 25f, pulse);
        Apply(hungerFill, hungerImage, shownHunger, hungerColor, survival.Hunger < 25f, pulse);
        Apply(healthFill, healthImage, shownHealth, healthColor, survival.Health < 25f, pulse);

        float healthTarget = survival.Health < SurvivalManager.MaxValue - 0.5f ? 1f : 0f;
        healthRow.alpha = Mathf.MoveTowards(healthRow.alpha, healthTarget, healthFadeSpeed * Time.unscaledDeltaTime);
    }

    void Apply(RectTransform fill, Image image, float value, Color normal, bool low, float pulse)
    {
        fill.anchorMax = new Vector2(Mathf.Clamp01(value), 1f);
        image.color = low ? Color.Lerp(warningColor, Color.Lerp(warningColor, Color.white, 0.35f), pulse) : normal;
    }
}
