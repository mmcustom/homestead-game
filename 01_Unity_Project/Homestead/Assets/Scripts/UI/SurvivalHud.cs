using UnityEngine;
using UnityEngine.UI;

// The survival meters, bottom-left of the HUD, always shown (top to bottom): Health, Stamina, Body temperature (was Warmth: centred, cold fills left, heat fills right), Hydration,
// Hunger and Load (Core_Survival_System.md, Health_System.md's Exposure System, First_Person_Controller.md,
// Inventory_System.md). Load is carried weight against the most the player can carry, with a tick at the Encumbered
// threshold — green below it, amber past it, red at the limit, the same numbers as the Inventory screen. Each fill is sized by anchoring rather than an Image fill,
// so it needs no sprite. Health, Hydration and Hunger turn to the warning colour and pulse below 25 (Hydration's
// Severe tier and its mirrors) — the row's label pulses too, so an empty bar still shows its warning. Stamina is measured against its full size: when low Hydration or Hunger caps it, the
// part it can't refill into is shaded, so the player can see why the bar stops short. While the player is sick (raw
// food, unpurified water), a pulsing "Sick" or "Very sick" line sits above the meters with the hours left, and each
// bout of vomiting is announced. The same line notes being wet or soaked, and warming by a fire.
public class SurvivalHud : MonoBehaviour
{
    [SerializeField] RectTransform hydrationFill;
    [SerializeField] RectTransform hungerFill;
    [SerializeField] RectTransform healthFill;
    [SerializeField] RectTransform staminaFill;
    [Tooltip("Shaded over the stretch of the stamina bar that low Hydration or Hunger won't let it refill into.")]
    [SerializeField] RectTransform staminaCap;
    [SerializeField] RectTransform warmthFill;
    [SerializeField] RectTransform loadFill;
    [Tooltip("Marks the Encumbered threshold on the Load bar.")]
    [SerializeField] RectTransform loadTick;

    [Header("Colours")]
    [SerializeField] Color hydrationColor = new Color(0.36f, 0.62f, 0.78f);
    [SerializeField] Color hungerColor = new Color(0.84f, 0.63f, 0.3f);
    [SerializeField] Color healthColor = new Color(0.72f, 0.3f, 0.24f);
    [SerializeField] Color staminaColor = new Color(0.62f, 0.74f, 0.4f);
    [SerializeField] Color warmthColor = new Color(0.86f, 0.46f, 0.3f);
    [SerializeField] Color loadColor = new Color(0.55f, 0.66f, 0.36f);
    [SerializeField] Color encumberedColor = new Color(0.84f, 0.63f, 0.3f);
    [SerializeField] Color warningColor = new Color(0.9f, 0.3f, 0.22f);

    [SerializeField, Min(0.1f)] float fillSmoothing = 8f;

    // The body-temperature meter that replaced Warmth: centred on normal core temperature, cold fills left, heat fills right.
    // Core °C -> position along the track (0-1); the cold stretch keeps Warmth's tiers (35 °C = 50, 32 °C = 25).
    static readonly float[] TempCore = { 28f, 32f, 35f, 36.5f, 37f, 37.5f, 38f, 39f, 40f, 41f };
    static readonly float[] TempPos = { 0f, 0.12f, 0.25f, 0.42f, 0.5f, 0.58f, 0.66f, 0.8f, 0.9f, 1f };
    static readonly float[] TempTicks = { 32f, 35f, 37f, 37.5f, 38f, 39f, 40f };
    [SerializeField] Color coldColor = new Color(0.42f, 0.66f, 0.88f);
    [SerializeField] Color hotColor = new Color(0.94f, 0.69f, 0.44f);
    [SerializeField] Color scorchColor = new Color(0.88f, 0.26f, 0.2f);
    float shownCore = 37f;

    Image hydrationImage, hungerImage, healthImage, staminaImage, warmthImage, loadImage;
    Text hydrationLabel, hungerLabel, healthLabel, warmthLabel;
    Color labelColor;
    Text sickLabel;
    float shownHydration = 1f, shownHunger = 1f, shownHealth = 1f, shownStamina = 1f, shownCap = 1f, shownWarmth = 1f, shownLoad;
    CanvasGroup group;
    PlayerController player;
    float nextPlayerSearch;

    void Awake()
    {
        hydrationImage = hydrationFill.GetComponent<Image>();
        hungerImage = hungerFill.GetComponent<Image>();
        healthImage = healthFill.GetComponent<Image>();
        staminaImage = staminaFill.GetComponent<Image>();
        warmthImage = warmthFill.GetComponent<Image>();
        loadImage = loadFill.GetComponent<Image>();
        warmthLabel = LabelOf(warmthFill);
        if (warmthLabel != null)
            warmthLabel.text = "Body";
        BuildTemperatureTicks();
        hydrationLabel = LabelOf(hydrationFill);
        hungerLabel = LabelOf(hungerFill);
        healthLabel = LabelOf(healthFill);
        labelColor = healthLabel != null ? healthLabel.color : Color.white;
        group = GetComponent<CanvasGroup>();

        sickLabel = UiKit.Text(transform, "Sick", "", 20, warningColor, TextAnchor.LowerLeft);
        RectTransform sick = sickLabel.rectTransform;
        sick.anchorMin = sick.anchorMax = new Vector2(0f, 1f);
        sick.pivot = new Vector2(0f, 0f);
        sick.sizeDelta = new Vector2(420f, 26f);
        sick.anchoredPosition = new Vector2(0f, 4f);
        sickLabel.horizontalOverflow = HorizontalWrapMode.Overflow;
        sickLabel.gameObject.AddComponent<Shadow>().effectColor = new Color(0f, 0f, 0f, 0.8f);
        sickLabel.enabled = false;
    }

    SurvivalManager watched;

    void OnDisable()
    {
        if (watched != null)
            watched.Vomited -= OnVomited;
        watched = null;
    }

    static void OnVomited() => ToolStatus.Flash("You vomit — losing water and food", 4f);

    void OnEnable()
    {
        // Start at the real values rather than sliding up from full.
        SurvivalManager survival = SurvivalManager.Instance;
        if (survival != null)
        {
            shownHydration = survival.Hydration / SurvivalManager.MaxValue;
            shownHunger = survival.Hunger / SurvivalManager.MaxValue;
            shownHealth = survival.Health / SurvivalManager.MaxValue;
            shownWarmth = survival.Warmth / SurvivalManager.MaxValue;
            shownCore = survival.CoreTemperatureC;
        }
    }

    void Update()
    {
        SurvivalManager survival = SurvivalManager.Instance;
        if (group != null)
            group.alpha = survival != null ? 1f : 0f;
        if (survival == null)
            return;
        if (watched != survival)
        {
            if (watched != null)
                watched.Vomited -= OnVomited;
            watched = survival;
            watched.Vomited += OnVomited;
        }

        if (player == null && Time.unscaledTime >= nextPlayerSearch)
        {
            nextPlayerSearch = Time.unscaledTime + 1f;
            player = FindAnyObjectByType<PlayerController>();
        }

        float k = 1f - Mathf.Exp(-fillSmoothing * Time.unscaledDeltaTime);
        shownHydration = Mathf.Lerp(shownHydration, survival.Hydration / SurvivalManager.MaxValue, k);
        shownHunger = Mathf.Lerp(shownHunger, survival.Hunger / SurvivalManager.MaxValue, k);
        shownHealth = Mathf.Lerp(shownHealth, survival.Health / SurvivalManager.MaxValue, k);
        shownWarmth = Mathf.Lerp(shownWarmth, survival.Warmth / SurvivalManager.MaxValue, k);
        shownCore = Mathf.Lerp(shownCore, survival.CoreTemperatureC, k);
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
        ApplyTemperature(survival, pulse);
        UpdateLoad(k);

        UpdateStatus(survival, pulse);

        staminaCap.anchorMin = new Vector2(Mathf.Clamp01(shownCap), 0f);
        staminaCap.gameObject.SetActive(shownCap < 0.995f);
    }

    // Fill runs from the centre (normal core temperature) to the shown core temperature: blue to the left when cold, orange to
    // red to the right when overheating. The label and fill pulse in the warning colour in the Severe cold tier and from severe heat.
    void ApplyTemperature(SurvivalManager survival, float pulse)
    {
        float centre = Position(37f);
        float at = Position(shownCore);
        warmthFill.anchorMin = new Vector2(Mathf.Min(centre, at), 0f);
        warmthFill.anchorMax = new Vector2(Mathf.Max(centre, at), 1f);

        bool cold = shownCore < 37f;
        bool alarm = survival.Warmth < 25f || survival.HeatLevel >= SurvivalManager.HeatStage.Severe;
        Color warning = Color.Lerp(warningColor, Color.Lerp(warningColor, Color.white, 0.35f), pulse);
        Color normal = cold ? coldColor : Color.Lerp(hotColor, scorchColor, Mathf.InverseLerp(37.5f, 40f, shownCore));
        warmthImage.color = alarm ? warning : normal;
        if (warmthLabel != null)
            warmthLabel.color = alarm ? warning : labelColor;
    }

    static float Position(float coreC)
    {
        if (coreC <= TempCore[0])
            return TempPos[0];
        for (int i = 1; i < TempCore.Length; i++)
        {
            if (coreC <= TempCore[i])
                return Mathf.Lerp(TempPos[i - 1], TempPos[i], Mathf.InverseLerp(TempCore[i - 1], TempCore[i], coreC));
        }
        return TempPos[TempPos.Length - 1];
    }

    // Thin marks on the track: the tier boundaries, with the centre (37 °C) brighter.
    void BuildTemperatureTicks()
    {
        Transform track = warmthFill.parent;
        if (track == null)
            return;
        foreach (float core in TempTicks)
        {
            bool centre = Mathf.Approximately(core, 37f);
            Image tick = UiKit.Image(track, centre ? "Temp Centre" : "Temp Tick", new Color(1f, 1f, 1f, centre ? 0.8f : 0.4f));
            tick.raycastTarget = false;
            RectTransform rt = tick.rectTransform;
            float x = Position(core);
            rt.anchorMin = new Vector2(x, 0f);
            rt.anchorMax = new Vector2(x, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(centre ? 3f : 2f, 0f);
            rt.anchoredPosition = Vector2.zero;
        }
    }

    // Sickness, then being wet and warming by a fire, on the line above the meters.
    void UpdateStatus(SurvivalManager survival, float pulse)
    {
        var parts = new System.Collections.Generic.List<string>();
        if (survival.IsSick)
            parts.Add(survival.SicknessLevel == SurvivalManager.Sickness.Severe
                ? $"Very sick  <size=16>{survival.IllnessHoursLeft:0.0} h — vomiting, very weak, losing health</size>"
                : $"Sick  <size=16>{survival.IllnessHoursLeft:0.0} h — upset stomach, weak, thirsty</size>");
        if (survival.IsWeakened)
            parts.Add($"Weakened  <size=16>{survival.WeakenedHoursLeft:0.0} h — stamina reduced after the collapse</size>");
        if (survival.Wetness > 0.6f)
            parts.Add("<color=#8FB8D8>Soaked</color>");
        else if (survival.Wetness > 0.15f)
            parts.Add("<color=#8FB8D8>Wet</color>");
        if (survival.IsShivering)
            parts.Add("<color=#9CC4E4>Shivering</color>");
        else if (survival.IsWorkingUpHeat)
            parts.Add("<color=#F0B070>Working up some heat</color>");
        // Overheating (core temperature above 37.5 °C): its own indicator, also shown on the Body temperature meter's right side.
        switch (survival.HeatLevel)
        {
            case SurvivalManager.HeatStage.Hot:
                parts.Add("<color=#F0B070>Hot</color>");
                break;
            case SurvivalManager.HeatStage.Exhaustion:
                parts.Add("<color=#F09A50>Heat exhaustion</color>  <size=16>stamina reduced — find shade and water</size>");
                break;
            case SurvivalManager.HeatStage.Severe:
                parts.Add("<color=#E8704A>Severe heat exhaustion</color>  <size=16>losing health — cool down now</size>");
                break;
            case SurvivalManager.HeatStage.Stroke:
                parts.Add("<color=#E04A3A>Heat stroke</color>  <size=16>losing health fast</size>");
                break;
        }
        if (survival.Damp > 0.25f)
            parts.Add("<color=#8FB8D8>Damp</color>");
        if (survival.NearFire && survival.Warmth < SurvivalManager.MaxValue - 0.5f)
            parts.Add("<color=#F0B070>Warming by the fire</color>");

        sickLabel.enabled = parts.Count > 0;
        if (!sickLabel.enabled)
            return;
        sickLabel.text = string.Join("   ", parts);
        sickLabel.color = survival.IsSick ? Color.Lerp(warningColor, Color.Lerp(warningColor, Color.white, 0.35f), pulse) : labelColor;
    }

    // Carried weight against the carry limit, with the Encumbered threshold marked.
    void UpdateLoad(float k)
    {
        InventoryManager inventory = InventoryManager.Instance;
        if (inventory == null || inventory.MaxCarryWeightKg <= 0f)
            return;
        float max = inventory.MaxCarryWeightKg;
        shownLoad = Mathf.Lerp(shownLoad, Mathf.Clamp01(inventory.CarriedWeightKg / max), k);
        loadFill.anchorMax = new Vector2(shownLoad, 1f);
        loadImage.color = inventory.CarriedWeightKg >= max - 0.001f ? warningColor
                        : inventory.IsEncumbered ? encumberedColor : loadColor;
        float tick = Mathf.Clamp01(inventory.EncumberedWeightKg / max);
        loadTick.anchorMin = new Vector2(tick, 0f);
        loadTick.anchorMax = new Vector2(tick, 1f);
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
