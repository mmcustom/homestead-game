# Homestead
## Health System v1.0

Status: Design Draft

---

# Purpose

The Health System represents the player's overall physical condition.

Health is not the same as hunger, hydration, or fatigue.

Those systems influence health.

Health is the final measure of a player's ability to survive.

---

# Core Philosophy

Players rarely die from a single mistake.

Most health problems result from:

- Poor planning
- Ignoring warning signs
- Repeated bad decisions

Examples:

Unsafe Water
↓
Illness
↓
Dehydration
↓
Health Loss

---

# Health Attribute

Range:

0-100

Health changes slowly.

Most actions affect other systems first.

Those systems then affect health.

---

# Health States

## 75-100

Healthy

No penalties.

---

## 50-74

Minor Health Issues

Effects:

- Reduced stamina recovery
- Increased fatigue accumulation

---

## 25-49

Poor Health

Effects:

- Slower movement
- Reduced work efficiency
- Faster fatigue accumulation

---

## 1-24

Critical Health

Effects:

- Severe movement penalties
- Risk of collapse

---

## 0

Player Collapse

Player becomes incapacitated.

Final implementation TBD.

---

# Illness System

Illness is primarily connected to water quality.

**Confirmed 2026-09-25 (Claude Code, Phase 3 — built and tested, not yet committed):** implemented as a single Sickness mechanic, triggered by the illness-risk roll on raw meat/fish (see Item_Data.md's Consumables) and on sub-Excellent water (see Water_System.md's Water Quality Levels). One bout lasts 8 in-game hours (about 10 real minutes): stamina is multiplied by 0.6, Hydration drains 1.75× faster, Health is lost at 1.5/hour, and Health doesn't recover at all while sick. Getting sick again while already sick adds another 8 hours rather than replacing the timer, stacking up to a 16-hour cap. On screen, a pulsing red "Sick" line appears above the HUD's Hunger/Hydration meters showing hours remaining, plus a message naming what caused it. Sickness is saved with the game; a save from before this system existed loads as healthy. The F9 debug key, in addition to its existing function, now also cures sickness outright. Tested: cut stamina from 0.75 to 0.45 and cost 1.5 Health over an hour as specced; stacked correctly to the 16-hour cap; survived a save and reload intact. This is deliberately still not the "detailed diseases" Core_Survival_System.md defers to Future Expansion — one generic Sickness state, not per-cause illness types — consistent with the lightweight approach the Water Purification and Cooking entries on Current_Task_List.md called for.

---

## Mild Illness

Causes:

- Questionable water
- Poor nutrition

Effects:

- Increased thirst
- Reduced energy

---

## Moderate Illness

Effects:

- Significant dehydration
- Reduced productivity

---

## Severe Illness

Effects:

- Large health reduction
- Extended recovery requirements

---

# Exposure System

Exposure occurs when environmental conditions exceed shelter protection.

Examples:

- Extreme cold
- Storms
- Long-term wet conditions

**Confirmed 2026-09-26 (Mike):** this becomes a real Warmth attribute, Range 0-100, shown on the HUD alongside Hunger and Hydration — moved out of Core_Survival_System.md's Future Expansion into current scope. Decreases driven by the factors already tracked elsewhere in the game rather than anything new: outdoor temperature (Season_System.md/WeatherManager's existing per-season ranges), precipitation (rain/snow — being wet should drain Warmth faster than being dry), and Wind (Weather_System.md's confirmed dual role as both a weather type and a standing property, already understood to carry a wind-chill-like effect). Increases near a lit campfire, the same proximity `FireManager` already uses for ambient_campfire_crackle (Audio_System.md) — the only warmth source that exists in the game right now. Shelter as a protection factor (the "exceeds shelter protection" framing above, and clothing) stays Future until Building_Housing_System.md and a clothing/apparel system actually exist — Warmth launches driven by temperature/weather/wind and fire proximity only. Effect tiers below follow the same shape as Hydration's confirmed tiers (Core_Survival_System.md) for consistency across the survival stats. Exact decay rate, how strongly temperature/wet/wind each weigh in, and fire's warming radius/rate are Claude Code's call — propose a first pass, Mike confirms by feel in Play Mode, same pattern as the rest of this doc set.

---

## 75-100 (No penalties)

Comfortable. No effects.

---

## Mild Exposure (50-74)

Effects:

- Increased fatigue

---

## Moderate Exposure (25-49)

Effects:

- Reduced stamina
- Health loss begins

---

## Severe Exposure (1-24)

Effects:

- Rapid health decline
- Severe movement penalties

---

## 0

Same collapse risk as Health reaching 0 — Warmth bottoming out is a path to collapse, not a separate death condition.

---

# Recovery

Health recovers through:

- Clean water
- Quality food
- Adequate sleep
- Shelter

Recovery should be gradual.

**Confirmed 2026-09-26 (Mike, playtest):** Health correctly drops from starvation (Hunger reaching empty), but currently never regenerates even once Hunger and Hydration are refilled — Recovery above was never actually built. Mike's rule: Health should slowly rebuild on its own as long as neither Hunger nor Hydration is empty (0). This stacks with the existing Sickness rule that Health doesn't recover at all while sick (see Illness System above) — sick overrides everything else, and otherwise Health regens whenever both meters are above zero. Exact regen rate, and whether food/sleep/shelter quality should scale it up (per the "Clean water, Quality food, Adequate sleep, Shelter" list above) versus a single flat rate for now, are Claude Code's call — see Current_Task_List.md.

---

# Nutrition

Future Expansion

Health benefits from balanced diets.

Food Categories:

- Meat
- Fish
- Fruits
- Nuts
- Vegetables

Balanced nutrition improves long-term resilience.

---

# Morale

Morale represents the player's motivation and outlook.

Range:

0-100

---

## Morale Increases

Examples:

- Good meals
- Warm shelter
- Successful hunts
- Completed projects

---

## Morale Decreases

Examples:

- Hunger
- Illness
- Lack of sleep
- Overwork / Lack of rest
- Storm damage
- Livestock loss

---

# Morale Effects

High Morale

Benefits:

- Small work efficiency bonus
- Slight recovery bonus

Low Morale

Effects:

- Reduced productivity
- Faster fatigue accumulation

Morale should matter but never dominate gameplay.

---

# Design Rules

1. Health declines gradually.

2. Small problems become major problems if ignored.

3. Recovery requires preparation.

4. Clean water is the most important health factor.

5. Health supports realism without becoming overly punitive.