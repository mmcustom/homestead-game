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

**Decided 2026-10-04 (Mike):** collapse is "wake weakened" — the player is not killed and the save is not ended. The screen fades out, time passes, and the player wakes up weakened. This closes the "Failure/death state" gap from the 2026-09-22 design review. Mike chose the direction only; the specifics below are a first pass.

**First-pass specifics (Claude, not yet confirmed) — Claude Code should correct if this doesn't match, and Mike confirms by feel in Play Mode:**

- **Trigger:** Health reaching 0, from any cause (starvation, dehydration, Sickness, Exposure). Warmth reaching 0 is not a separate trigger; it only leads here by draining Health, as the Exposure System already says.
- **Fade and time skip:** a fade to black, then 8 in-game hours pass. Fires burn down, fuel and food keep ticking, and weather advances, because the world doesn't pause. This is the main cost: lost daylight, and in winter, a cold night.
- **Where you wake:** at the nearest sleeping spot you own (Small Cabin bed, Lean-To, or Tent) if one exists. Otherwise you wake where you fell.
- **Stats on waking:** Health set to 20. Hunger and Hydration are raised to at least 25, so a starvation or thirst collapse can't loop straight back into another collapse. Warmth is raised to at least 40 and the wet status is cleared. Sickness is cleared, as the passed time is treated as a hard recovery.
- **Weakened state:** for 4 in-game hours after waking, stamina is multiplied by 0.7. A short HUD line shows the time remaining, like the "Sick" line.
- **Message:** a plain on-screen line such as "You collapsed and woke hours later, weak." The message names the cause (Starvation, Dehydration, Sickness or Exposure) so the player learns what went wrong.
- **Saving:** an autosave is not triggered by collapse, and the Weakened timer is saved with the game.
- **No item loss in the first pass.** Mike's chosen option allowed for a cost such as a dropped item, but the lost 8 hours is the cost. Item loss can be added later if collapse turns out to be too forgiving in playtesting.

**Open for Mike or Claude Code:** repeated collapses (for example, whether a second collapse within one in-game day should cost more) are not specified. Wait to see how often collapse happens in a Winter Preparation playthrough first. Per the GDD Development Rule, this is designed now but should be built after, or alongside, that playthrough.

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

**Built and tested 2026-09-26 (Claude Code) — committed and pushed (`0563b9e`).** A "feels-like" temperature drives Warmth: outdoor air temperature, minus up to 15°F for wind chill, minus up to another 12°F when the player is soaked. Above 50°F feels-like, Warmth recovers up to 5/hour; below that it falls at up to 12/hour at its worst, at a rate of 0.3/hour per degree under 50°F, faster still while wet. At a steady 25°F feels-like standing exposed, it takes about 10 real minutes to drop from full to the Moderate tier. A wet/soaked status line shows on the HUD alongside the meter. A lit campfire at full heat adds Warmth at 30/hour within 1.5 m, fading to nothing by 6 m away, and also warms and dries the player as a side effect — still the only warmth source in the game. The F9 debug key now also warms/dries the player; a new F10 debug key drops Warmth to 20 for testing the low tiers. The tiers below carry the real numbers Claude Code implemented, replacing the earlier vague bullets.

---

## 75-100 (No penalties)

Comfortable. No effects.

---

## Mild Exposure (50-74)

Effects:

- Stamina ×0.9

---

## Moderate Exposure (25-49)

Effects:

- Stamina ×0.7
- 1 Health/hour lost

---

## Severe Exposure (1-24)

Effects:

- Stamina ×0.5
- 4 Health/hour lost
- Movement ×0.75

---

## 0

8 Health/hour lost, Movement ×0.6. Same collapse risk as Health reaching 0 — Warmth bottoming out is a path to collapse, not a separate death condition.

---

# Recovery

Health recovers through:

- Clean water
- Quality food
- Adequate sleep
- Shelter

Recovery should be gradual.

**Confirmed 2026-09-26 (Mike, playtest):** Health correctly drops from starvation (Hunger reaching empty), but currently never regenerates even once Hunger and Hydration are refilled — Recovery above was never actually built. Mike's rule: Health should slowly rebuild on its own as long as neither Hunger nor Hydration is empty (0). This stacks with the existing Sickness rule that Health doesn't recover at all while sick (see Illness System above) — sick overrides everything else, and otherwise Health regens whenever both meters are above zero. Exact regen rate, and whether food/sleep/shelter quality should scale it up (per the "Clean water, Quality food, Adequate sleep, Shelter" list above) versus a single flat rate for now, are Claude Code's call — see Current_Task_List.md.

**Built and tested 2026-09-26 (Claude Code) — committed and pushed (`0563b9e`).** Health regens at 3/hour when both Hunger and Hydration are at 50 or higher ("well-fed"), and 1.5/hour when either is lower but still above 0. Claude Code additionally made regen pause completely whenever anything else is actively costing Health — Severe Hunger, Severe Hydration (Core_Survival_System.md's tiers), or the Warmth Severe tier below — rather than letting regen and drain net out simultaneously at the same time. **This "anything actively draining Health also blocks regen" rule was Claude Code's own call, not specified by Mike, and is flagged in Current_Task_List.md as awaiting his confirmation.** Along the way, Claude Code found the pre-existing Recovery code only ever healed when *both* Hunger and Hydration were at 75+ — effectively dead code in practice — and fixed it to match Mike's actual spec above.

**Confirmed 2026-09-26 (Mike):** agrees with Claude Code's rule that regen pauses under Severe Hunger or Severe Hydration, and wants it to also pause while Sick — which it already does, independently, via the existing Illness System rule above ("Health doesn't recover at all while sick"), predating this batch. So the full pause list is: Severe Hunger, Severe Hydration, Severe Warmth (Exposure System below), or Sick. On how long the pause lasts for sickness specifically: it isn't a separate number — regen simply resumes automatically once the Sickness state itself clears, per the Illness System's existing 8-hour bout / 16-hour stacked cap. Mike still wants to playtest the full Hunger/Hydration/Sickness regen behavior in Play Mode before this is fully locked.

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