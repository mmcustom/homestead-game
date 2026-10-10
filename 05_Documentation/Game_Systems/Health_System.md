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
- **Fade and time skip:** a fade to black, then 8 in-game hours pass. Fires burn down, wood and food keep ticking, and weather advances, because the world doesn't pause. The player's own survival meters (Hunger, Hydration, Warmth, Health) do not tick during the skip; they are set by the waking floors below. This is the main cost: lost daylight, and in winter, a cold night. (Revised 2026-10-07: the first draft said "food keeps ticking", which was ambiguous about the player's own meters. Claude Code found that Warmth drain uses the player's position, so a player left where they fell could freeze again during the skip.)
- **Where you wake:** at the nearest placed shelter that has a sleeping position: a finished Small Cabin, a Lean-To, or a Tent. A Sleeping Bag has no placed position and can't be a wake point. An unfinished CabinSite doesn't count. "Nearest" means at any distance, since the game has no concept of owning a shelter. If none exists, you wake where you fell. **Decided 2026-10-07 (Mike): no distance cap.**
- **Stats on waking (revised 2026-10-07):** Health set to 20. Hunger and Hydration are raised to at least 50 and Warmth to at least 50. Claude Code checked these against the code: the earlier floors of 25 (Hunger/Hydration) and 40 (Warmth) were too low. Anything under 50 Warmth is in the Reduced tier, which costs 1 Health/hour and blocks Health regeneration. Hydration drains about 2.8/hour, so a floor of 25 reaches the Severe tier within an hour and reaches 0 in roughly 9 hours, which with Health 20 is a weak buffer. Wetness is cleared, which needs a small public setter because only an editor-only debug method sets it today. Sickness is cleared by the existing `Cure()`, as the passed time is treated as a hard recovery.
- **Weakened state:** for 4 in-game hours after waking, stamina is multiplied by 0.7. Claude Code confirms this is easy to add: the stamina multiplier is already a product of factors, and the Sickness HUD line can take another line. The timer needs a new field in the save data, and old saves load it as 0, which is safe. A short HUD line shows the time remaining, like the "Sick" line.
- **Message (revised 2026-10-07):** a plain on-screen line such as "You collapsed and woke hours later, weak." Naming the cause (Starvation, Dehydration, Sickness or Exposure) is not free: nothing tracks the cause today. The collapse event fires from `SetHealth` with no cause, and Health loss is a sum of several drains in the per-tick step. The cause has to be worked out there, for example by taking the largest contributor. That makes naming the cause a small piece of extra work, so it is optional in the first build.
- **Saving:** an autosave is not triggered by collapse, and the Weakened timer is saved with the game.
- **No item loss on a first collapse.** The lost time is the cost. Item loss only appears at the 4th recent collapse (see "Repeated collapses" below).

**Implementation notes (Claude Code, 2026-10-07, from reading `SurvivalManager.cs` and `SleepManager.cs`; nothing built):**

- **Re-entry:** the `Collapsed` event fires from inside `SetHealth`, which runs inside the per-tick `Step`. The collapse handler must be deferred, as a coroutine like `SleepRoutine`, rather than calling `PassHours` from inside the event.
- **Loading a save at Health 0:** `RestoreState` sets `collapsed = health <= 0`, and nothing fires the event on load, so the player would be stuck. The handler should also check `IsCollapsed` on load.
- **Interaction with Sleep:** `SleepManager` already wakes the player when Health is below 15. If a collapse fires during sleep, it needs a guard on `IsSleeping`.
- **Event subscribers:** the `Collapsed` event has no subscribers yet.

**Repeated collapses — decided 2026-10-07 (Mike): they cost progressively more.** Mike chose the direction (yes, progressively) but not the numbers.

**Scale — confirmed 2026-10-07 (Mike): "your numbers look good for now."** The numbers were Claude's first pass and are accepted as the starting point. Claude Code should still correct anything that clashes with the code, and Mike tunes by feel in Play Mode:

- **Counting:** the game keeps a count of recent collapses. Each collapse adds 1, and the count falls by 1 for every 3 in-game days without a collapse, so a rough patch is remembered but a recovered player is eventually forgiven. The count is saved with the game, and old saves load it as 0.
- **Scale by count (including the new collapse):**

| Collapse | Time skipped | Weakened state | Health on waking | Item loss |
|---|---|---|---|---|
| 1st | 8 hours | 4 hours at stamina ×0.7 | 20 | none |
| 2nd | 12 hours | 6 hours at stamina ×0.6 | 15 | none |
| 3rd | 16 hours | 8 hours at stamina ×0.5 | 10 | none |
| 4th and later | 16 hours | 12 hours at stamina ×0.5 | 10 | one carried item dropped where you fell |

- **What doesn't scale:** the waking floors for Hunger, Hydration and Warmth (50 or higher) stay the same on every collapse, so repeated collapses can't turn into an unbreakable loop. The cost is lost time, a longer weakened stretch and, at the 4th collapse in a short span, an item.
- **Message:** the on-screen line says how many collapses are recent when it is 2 or more, for example "You have collapsed 2 times recently."
- **Why this shape:** a single collapse stays forgiving (it is a lesson), and a pattern of collapsing is what gets punished. The worst case is still not game over, which matches Mike's "wake weakened" decision.
- **Still open:** which carried item is dropped at the 4th collapse (not specified). The numbers are accepted for now and get tuned in a Winter Preparation playthrough. Per the GDD Development Rule, this is designed now and should be built after, or alongside, that playthrough.

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

**Gap found 2026-10-07 (Mike, Pioneer playtest):** "i noticed my new game started in the wintertime when temperatures were lower, so my warmth dropped pretty rapidly. I think we need to figure out how to increase our body temperature by using stamina? just like if we are getting a workout?" Today Warmth is driven only by weather, wind, wetness and fire proximity, so a player's own activity makes no difference to it. Pioneer also multiplies drain by ×1.3 and lowers temperature by 3°C (Difficulty_System.md), which makes a cold start harsher still.

**First-pass scope: Exertion Warmth — SUPERSEDED the same day (2026-10-07) by the "Core Temperature Model" below, after Mike asked for the real physiology. Kept for reference only; it was a simple +°F fudge, and the research shows exertion, shivering, sweat and wet clothing are all real, separate effects:**

- **Exertion level:** the game tracks how hard the player has been working, from 0 to 1, based on recent stamina spending (sprinting, swinging the Axe, Pick Axe or Shovel, splitting wood). It is smoothed so it rises over roughly 30 real seconds of work and fades over about a minute of rest. Standing still, sleeping and sitting at a fire count as 0. Walking without sprinting adds a small flat amount.
- **Effect:** exertion adds to the "feels-like" temperature the Warmth system already computes, up to about +12°F at full exertion. Warmth then drains more slowly in the cold. It does not add Warmth directly, and it can't push feels-like above the 50°F comfort line by itself.
- **What it does and doesn't fix:** at a feels-like of 25°F, drain is about 7.5 Warmth/hour at rest and about 4/hour at full exertion. That nearly halves the drain but doesn't stop it, so a fire or shelter is still what actually recovers Warmth. The intent is that working hard in the cold buys time and feels right, not that it replaces a fire.
- **Trade-off (optional, Claude's idea):** while exertion is high, Hydration drains about 50% faster, so working to stay warm costs something. Claude Code can leave this out of the first build.
- **Feedback:** a short HUD line such as "Working up a sweat" while the bonus is active, so the player can see why Warmth is dropping slower.
- **Not included:** sweating making a player wet and colder afterward. It is realistic, and could become a later layer, but it adds a second drain to balance.
- **Stacks with** Difficulty's drain multiplier (Pioneer ×1.3 applies after the bonus) and with fire warming, as an extra term, not a replacement.

---

**Core Temperature Model — 2026-10-07 (Mike asked for real physiology: "we are 'humans' and we all have our 'best temperature' ranges... on average humans have a minimum and a maximum temperature that we can physically tolerate before our bodies shut down. lets build this as accurate as possible by checking our research on the internet and go from there.").** The sources and numbers are in `Research/Human_Thermoregulation.md`. **Everything below is a first pass by Claude, not yet confirmed by Mike — Claude Code should correct anything that doesn't fit the code, and Mike confirms by feel in Play Mode.**

What the research changes: the real body has one underlying number, core temperature (normal about 37 °C / 98.6 °F), and a few separate effects push it up or down. Heavy work really does warm you (5 to 6 times resting heat for chopping, digging or carrying), shivering is the body's own automatic exercise (up to about 4.9 times resting) that fades when fuel runs out, and sweat or wet clothing makes you colder (wet clothing about 5 times the conductive heat loss, wet feet about 25 times). So the plan is to model those effects directly instead of a single "exertion bonus."

- **1. Core temperature is the real value; the Warmth meter is derived from it.** Mapping (straight lines between points): Warmth 100 is 37.0 °C, 75 is 36.5 °C (where shivering starts), 50 is 35.0 °C (where hypothermia is defined to begin), 25 is 32.0 °C, and 0 is 28.0 °C. This lines up the existing Warmth tiers with the real stages: Mild Exposure (50-74) is the shivering zone just above hypothermia; Moderate (25-49) is clinical mild hypothermia (35-32 °C: shivering, confusion, poor coordination); Severe (1-24) is clinical moderate (32-28 °C: shivering stops, dazed, irrational); and Warmth 0 (28 °C) is where the real body goes unconscious and risks cardiac arrest, which is exactly where the game's collapse path begins. The existing tier effects (stamina multipliers, Health loss per hour, movement penalties) stay as they are. Saves store core temperature; an old save's Warmth converts through the same table.
- **2. Heat balance each tick.** Core temperature changes with heat produced minus heat lost. **Production:** a resting base (about 1 MET, roughly 80 W for a 70 kg person) multiplied by what the player is doing: sleeping or sitting about 1, walking about 3.5, chopping, digging or hauling about 5 to 6, sprinting about 10 (sustained only as long as stamina lasts). **Shivering** adds involuntary production when core temperature drops below about 36.5 °C, ramping up to about 4.9 times resting at most; it costs Hunger and stamina, is weak when Hunger is low (the research says shivering fades as glycogen runs out), and stops entirely below about 32 °C. **Loss:** depends on air temperature, wind (more convective loss), wetness (wet clothing about 5 times the conductive loss, which is the real basis for the existing "soaked" penalty) and clothing insulation. Campfire warming becomes radiant heat gained, using the existing 1.5 m / 6 m falloff.
- **3. Clothing baseline.** The research gives a thermoneutral zone (no shivering or sweating needed) of about 15 to 24.5 °C (59 to 76 °F) for a lightly clothed adult, and 28 to 32 °C unclothed. The game's current comfort line is 50 °F, which is more like heavy clothing. First pass: the player's starting clothes should be the "lightly clothed" baseline, so Warmth really does fall at 50 °F air if the player stands still. That is realistic, but it makes the cold start in this playtest harsher at rest, and exertion (point 4) is what saves it. Real clothing and shelter (Future) widen the cold side of the zone later. The exact insulation value is a tuning knob for Claude Code.
- **4. Exertion falls out of the model.** Mike's idea needs no separate bonus: working hard produces several times resting heat, so core temperature holds or rises. Chopping, digging, mining, splitting and hauling a heavy pack keep a player warm in the cold; standing still, fishing and sleeping don't. Stamina already limits how long the player can keep it up.
- **5. Sweat (built with the cold side — Mike's call 2026-10-07).** In the cold, sweat soaks clothing and increases heat loss afterward. First pass: hard work for a long stretch builds a "damp" state that raises heat loss like a mild version of being wet, and clears by resting near a fire or in a warm shelter. In the heat, sweating costs Hydration (the research gives 1 to 2 L per hour at heat exhaustion). This adds a second thing to balance, but Mike chose to build it together with the cold side.
- **6. The hot end (built together with the cold side — Mike's call 2026-10-07).** Real limits: sweating starts near 37.5 °C, heat exhaustion is roughly 37 to 40 °C (weakness, nausea, dizziness; stamina and work-efficiency penalties), and heat stroke is above 40 °C (confusion, collapse, and sweating can stop). First pass: a separate "Overheating" status line, not an extension of the Warmth meter, that feeds the same Player Collapse path with the cause named "Heat." Needs summer heat and humidity from Weather_System.md to drive it.
- **7. Individual variation.** The research says peak shivering depends on fitness, body size and age, so people differ. First pass: one average player. Fitness as a trait that improves with work is a possible later layer, not part of this build.
- **8. Difficulty.** Pioneer's temperature offset of −3 °C stays. The ×1.3 drain multiplier no longer maps cleanly to "drain," so the first pass applies it as ×1.3 on heat loss instead (Mike can change that).
- **9. Feedback.** Keep the Warmth meter. Add status words as the body reacts: "Shivering" (below 36.5 °C), "Working up a heat" (production well above rest), "Damp" when sweat has soaked the player's clothes, and "Overheating" when core temperature is above about 37.5 °C. Showing the actual core temperature number is an optional setting (a realism choice for Mike).
- **10. Timing.** The game compresses time (a day is 30 real minutes), so real cooling or warming rates can't be copied one to one. The research pages didn't give a reliable cooling rate for a clothed person (see the research doc's "Not Found" list). So the exact rates are calibrated by feel; the thresholds, relative effort multipliers and wet/wind multipliers above come from the research.
- **Build scope — decided 2026-10-07 (Mike): cold and hot together, in one pass,** not cold-side-first as Claude recommended. That covers core temperature, heat balance, exertion, shivering, wet and wind, sweat and the damp state, and the hot end (overheating, heat exhaustion, heat stroke). The hot end needs summer heat and humidity from Weather_System.md, so Claude Code should say if the current weather values can't drive it yet. Because this replaces tuned Warmth math, it should be built behind the playtest first (Development Rule) and calibrated by feel; Mike still has to confirm the numbers above.

**Review corrections — 2026-10-07 (Claude Code's read-only code review of this section; nothing built).** These override points 1 to 10 above wherever they conflict.

- **Keep Warmth derived, and keep one variable.** Core temperature is the only stored value. Warmth becomes a derived getter of it, so the HUD, tiers, sleep wake thresholds and debug keys all keep working; the F10 debug key needs the inverse mapping. The hot side uses the same variable (core temperature simply runs above 37 °C), so there is no separate "Overheating" state to drift out of step. "Overheating" stays as a status label only.
- **Mapping:** the 75, 50 and 25 breakpoints match `TierOf` and the clinical stages, but the slope is very uneven: about 0.02 °C per Warmth point at the top of the meter and 0.16 °C per point at the bottom. A 0.5 °C exertion bump moves Warmth by about 25 points near the top, while the Severe tier takes about 8 times more heat per point. That follows from using real temperatures, and the meter will jump near the top and move slowly near the bottom. If it feels wrong in play, Mike can remap the top band.
- **Heat balance structure (replaces point 2's loss description):** production is resting watts × MET for the current activity, plus shivering. Cold-side loss is conductive: (33 °C skin minus effective air temperature) × body area ÷ R_total, where R_total is clothing resistance plus an air layer that thins with wind. Wet divides the clothing resistance; the ×5 applies to the clothing resistance only, not to total loss. Hot-side loss is sweat evaporation, limited by humidity and cut by dehydration. The campfire is radiant gain using the existing 1.5 m / 6 m falloff. This replaces `StepWarmth`'s feels-like calculation. The shelter hooks (`SleepInsulation`, rain and wind shelter, `OverheadCover`) stay, and sleep insulation becomes added resistance.
- **Activity input:** `MovementState` only knows Idle, Walking, Sprinting, Crouching and Jumping, and only `AxeTool` calls `TrySpendStamina`. So work heat needs a smoothed exertion value fed from those two places (the Pick Axe, Shovel and splitting would need to report effort too), plus carried weight.
- **Calibration (Claude Code's arithmetic, standard physiology values):** body heat capacity about 3.5 kJ/kg/°C, roughly 67 Wh per °C for a 70 kg player. Run per game hour, the real physics lands within an order of magnitude of today's tuning, so point 10's claim that real rates can't be used is too strong. Use the real constants per game hour and tune from there.
- **The clothing baseline in point 3 is too harsh.** A lightly clothed player resting at −4 °C loses about 300 W against about 80 W produced, so core temperature falls about 3.5 °C per game hour and reaches 35 °C in roughly 40 real seconds. Even at 10 °C it falls about 1.7 °C per game hour. Until a clothing system exists, use a heavier baseline of about 2 clo (R ≈ 0.4 m²K/W). Point 1's and point 3's references to the old 50 °F comfort line are obsolete, since the new model drops that line.
- **Double counting to remove when built:** Hunger's cold multiplier (`coolF`/`frigidF`) duplicates the cost of shivering; Hydration's hot multiplier (`warmF`/`hotF`) duplicates the cost of sweat; and Hunger's activity multipliers (walk ×1.15, sprint ×1.6) don't track MET (3.5 and 10). Don't couple the new model to them. Derive the Hunger cost of shivering and the Hydration cost of sweating from the heat model itself, and retire the old multipliers.
- **Shivering cliff:** shivering stops at 32 °C, which is exactly Warmth 25, so net loss jumps at the Severe tier boundary. That may be intended (the real body loses its main heat source there), and it is now stated on purpose.
- **Hot-side thresholds:** "37–40 °C" for heat exhaustion overlaps normal temperature. Start heat exhaustion near 38 °C, keep the "Overheating" warning at 37.5 °C, and heat stroke stays above 40 °C.
- **Damp rule (point 9) — Claude's first pass, not confirmed:** damp builds while work heat is high and the air is cold. It clears over about 1 in-game hour near a lit fire or inside a heated cabin, about 3 in-game hours in dry shelter, and not at all in rain.
- **Stamina link — Claude's first pass, not confirmed:** shivering and heat exhaustion each cut stamina recovery to about 70%. The research gives no numbers for this, so it is tuned by feel.
- **Pioneer ×1.3 (Claude Code):** apply it in the harmful direction only: loss × 1.3 when cooling, net heat gain × 1.3 when overheating. Today's code multiplies losses only, so the cold side already matches.
- **Difficulty temperature offset:** the ±3 °C offset makes Homesteader summers 3 °C hotter and Pioneer summers cooler, which is backwards for heat. Claude's first pass: apply it in the harmful direction too (Pioneer colder in cold weather and hotter in hot weather, Homesteader the reverse).
- **Save format:** add `coreTempC` and a damp/sweat field. Keep writing the old `chill` value (derived from Warmth) so an older build can still read a new save. For old saves, `JsonUtility` reads a missing float as 0 and a real core temperature is never below 28, so `coreTempC <= 20` means "unset": convert with Warmth = 100 − `chill`, then invert the mapping. Wetness stays as it is.
- **Weather additions needed before the hot side can be built:** the weather system has temperature only (a Summer high of 30 °C plus about 4 °C of variation, so about 34 °C at most), with no humidity and no solar load. At 34 °C in dry air a lightly clothed player wouldn't get heat illness except through extreme exertion. It needs: humidity (per season, adjusted by rain and cloud); sun load (clear sky, hour of day and shade, where `OverheadCover` already gives shade); heat waves (an event, or reuse the existing drought flag); and heat-based Health-loss tiers, because the collapse path is Health-driven and heat needs its own tiers like `ColdLoss`. **So Mike's cold-and-hot-together scope can't be built from this doc as written until those weather additions are designed.**

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

---

# Hot-Side Weather Inputs (pointer, 2026-10-07)

**Claude, first pass, not yet confirmed.** The hot side of the Core Temperature Model needs weather inputs that did not exist. They are designed in Weather_System.md under "Heat Inputs for the Core Temperature Model": a humidity value, a sun-load value, a Heat Wave weather type (Summer, about +6 C for an ordinary one, no design ceiling), and the overheating tiers summarized there (Hot 37.5-38, Heat exhaustion 38-39, Severe 39-40, Heat stroke above 40 C). Sweat cooling is limited by humidity and by Hydration, which is the main way dehydration becomes dangerous. Confirmed 2026-10-07 (Mike): the seasonal automatic clothing assumption (about 2 clo in Winter down to about 0.8 clo in Summer until clothing exists; Claude Code's review raised the Summer end from 0.5 so ordinary Summer nights don't cause shivering) is OK for now. Open items there: the HUD for overheating, and pond drying in Heat Waves. **Decided 2026-10-07 (Mike): heat is handled like cold.** Overheating does not kill the player outright. When heat drains Health to 0, the player passes out and wakes weakened, with penalties that grow with repeated collapses. It uses the same Player Collapse system as cold, and the same recent-collapse count (one shared counter, whatever the cause; confirmed 2026-10-07, Mike: "progressing penalties" means the same repeated-collapse scale as cold). The heat tiers (Hot, Heat exhaustion, Severe, Heat stroke) are the warning steps on the way there, as the cold tiers are. First-pass wake rules for heat (Claude, not yet confirmed): the time skip is spent cooling in the wake shelter, so core temperature is clamped into 35-37.5 C on waking, using about 37.2 C for heat (Claude Code's review, 2026-10-07: 37.5 is exactly where Hot starts, and the clamp also covers the cold-side Warmth floor; the other collapse gaps it found are listed in Weather_System.md under "Heat and Player Collapse"); Hydration floor of 50 already applies; the message may name heat as the cause. Also decided 2026-10-07 (Mike): there is no 100 F ceiling on Heat Waves (see Weather_System.md). Nothing here is built; per the GDD Development Rule it waits for the playthrough.


**Build status 2026-10-07:** Mike approved building the cold side of the Core Temperature Model now, because Warmth dropping too fast was a playtest problem. Scope: the single stored core temperature, Warmth as a derived value, resistance-based heat loss, the activity hook, shivering, damp clothing, the seasonal clothing assumption, and the save change, all as written in the Core Temperature Model and its review corrections. The hot side (humidity, sun load, Heat Waves, overheating tiers) and Player Collapse stay unbuilt until Mike says so. The model should leave hooks for the hot side.

# Core Temperature Model, Cold Side: Built 2026-10-07 (Claude Code), compile-checked only

Not tested in Play Mode (UnityMCP was down), not committed. Claude Code ran the physics on its own (a pure-math class with no Unity types) to get tuning numbers, and the game itself has not run it. Every number below is a first pass for Mike to confirm by feel.

**What was built**

- New `Survival/BodyHeat.cs`: pure math. The Warmth to core temperature table (100 = 37.0 C, 75 = 36.5, 50 = 35.0, 25 = 32.0, 0 = 28.0), heat production, resistance-based loss, shivering, and the Advance step. Its physics numbers sit in an Inspector-tunable Settings block on `SurvivalManager`.
- `SurvivalManager.cs`: core temperature is the stored value and Warmth is a getter derived from it. `StepWarmth` is replaced by `StepBody`. Rain wetness and the fire's drying are kept from the old code. Bedding and shelter hooks are kept: `SleepInsulation` ("cuts the cold by X") is converted to added resistance against a 0.44 m2K/W reference.
- Seasonal clothing: 2.0 clo at -1 C and below, falling to 0.8 clo at 24 C and above, driven by a 24-hour smoothed temperature.
- A campfire is radiant gain, scaled by the existing falloff. Pioneer's multiplier scales heat lost, never heat gained.
- Retired: the Hunger cold multiplier, the Hydration hot multiplier, and the Hunger walk and sprint multipliers. Hunger now scales with body heat output (0.06 per extra MET, which reproduces about x1.15 walking and about x1.54 sprinting). Hydration's own walk and sprint multipliers stay.
- Save: still writes `chill`, and adds `coreTempC` and `damp`. A `coreTempC` of 20 or less means an old save and is converted through the table.
- `ClampCoreTemperature(35, 37.5)` exists but nothing calls it yet (for waking from Player Collapse). `HeatLevel` and `HeatLoss` are hooks for the hot side; `HeatLoss` returns 0.
- `PlayerController.TrySpendStamina` calls `NoteWork()`, so every tool swing counts as work. `SurvivalHud` adds "Shivering", "Working up a heat" and "Damp".
- `WeatherManager`: the difficulty temperature offset now goes in the harmful direction: full strength at 14 C and below, reversed at 26 C and above, easing through zero between. This is the only weather change. None of the hot-side weather work is built.

**Numbers Claude Code chose or changed**

- Resting heat 80 W, skin 33 C, body area 1.8 m2. Still-air transfer 7.8 W/m2K (0.128 m2K/W). Wind transfer 8.3 x sqrt(speed). Clothing 0.155 m2K/W per clo.
- Wind: the 0-1 strength is mapped to 8 m/s at full strength, cut in proportion under tree shelter.
- Wet clothing: x5 loss on clothing resistance, 4.0 in the code per the doc; sweat damp x2.
- Activity (MET): 0.9 sleeping, 1 idle, 2.5 crouching, 3.5 walking, 10 sprinting, +1.5 carrying at the limit, 5.5 working with a tool (each swing adds 0.35, fades in about 7 seconds).
- Fire: 250 W radiant at full heat. It started at 400 W, but the standalone run overheated a seated player.
- Shivering: starts at 36.5 C, full by 35.5 C, peak 4.9x resting, weakens below 25 Hunger, stamina down to x0.7 taken as the lowest of the existing factors (so it never stacks on the Warmth tier's own penalty).
- Placeholder heat dissipation of 400 W per C above 37 C until the hot side exists. Core temperature limits 24 to 41 C.
- Damp builds 0.5 per hour when MET is 4.5 or more, core is above 36.9 C and the air is under 15 C. It clears per hour: 1.0 by a fire, 0.33 in shelter, 0.1 in the open, not at all in rain.

**Doc points not followed as written**

- The shivering cliff at Warmth 25 was removed by fading shivering from full at 33 C to zero at 30 C, instead of stopping at 32 C.
- "Shivering fades after a few hours" is not built; low Hunger as fuel stands in for it.
- Damp builds from hard work (MET 4.5 or more and core above 36.9 C), so sitting by a fire does not count.
- Stamina goes through `StaminaMultiplier`, which scales maximum stamina and recovery together, not recovery alone.
- Inventory log splitting is not hooked, only tool swings.

**Standalone run (75 real seconds per game hour)**

- Winter (-1 C, light wind), Settler, resting: shivering at about 30 seconds, settling at Warmth about 70. Walking stays at 100. Chopping holds core about 37.3 to 37.5 C. Chop 3 minutes then rest: Warmth falls to about 64 within 60 seconds (sweat chill) and recovers slowly; by a fire it is back to 100 in about a minute.
- Pioneer, Spring dawn (2 C), resting: shivering at about 21 seconds, settling at Warmth about 68. Walking stays at 100. Winter night (-9 C), resting: settles at about 64. Same with Hunger at 15: Warmth under 50 at about 135 seconds, under 25 at about 330, 0 at about 445.
- Soaked at 4 C with wind (x5): Warmth under 50 at 42 seconds, 0 at about 196 seconds, even when walking. Sleeping in a bag at -5 C: about 72. In a tent with the bag: 100.

**Tuning risks Claude Code flagged**

- Soaked is the biggest risk: about three times faster than the old model, survivable only with a fire or shelter. A wet factor of 3 collapses in about 5 minutes, and 2 holds. The knob is `wetFactor` in `BodyHeat.Settings`.
- Plateau: a resting player in any cold air sits at Warmth about 65 to 72 with shivering showing, including Spring at 12 C and Summer nights at 18 C. The meter drops fast near the top (25 points per 0.5 C), then holds. That is by design in the model but will feel different. Watch for shivering on ordinary Summer nights, which the 0.8 clo floor was meant to prevent; if it shows up, raise the Summer floor or lower the shivering onset.
- Shivering costs Hunger very little at 0.06 per MET, so a shivering player is not punished much for it.

**Still to do:** Mike tests in Play Mode (list in Current_Task_List.md log), then Claude Code commits. The hot side and Player Collapse are still not built.
# Shivering Threshold: Mike's Feedback 2026-10-10

**Mike (Play Mode, after the cold-side checks passed):** "i do think the shivering threshold should be a little lower though. i shouldn't start shivering until my temp gets alot lower."

**What it is today (from the cold-side build notes above):** shivering starts at core 36.5 C (Warmth 75, the top of Mild Exposure), is full by 35.5 C, peaks at 4.9x resting heat, fades from full at 33 C to zero at 30 C. `SurvivalManager.IsShivering` is true when ShiverIntensity is above 0.05, and that one flag drives the "Shivering" HUD word and the new Shiver.wav loop. In a standalone run a resting Settler in Winter shivered within about 30 s and settled near Warmth 70, and a Pioneer at a Spring dawn (2 C) shivered within about 21 s and settled near Warmth 68. So the player is "shivering" almost any time they stand still in cool weather, which is what Mike is reacting to.

**First pass (Claude, not yet confirmed):** change only WHEN the player is shown and heard shivering, not the heat physics.

- The visible state (HUD "Shivering" word and the Shiver.wav loop) starts at core about 35.5 C (Warmth about 58) instead of 36.5 C (Warmth 75), and reaches full intensity by about 34.5 C (Warmth about 46). That is mid Mild Exposure, not the top of it. Resting at Spring dawn or in a cool Summer night (plateau Warmth 65-72) no longer shows shivering; a soaked, windy or Winter-night rest does.
- The heat-production ramp (shivering's extra heat, 4.9x peak) is left exactly as built, so the resting plateaus, tier effects and everything Mike already confirmed in the cold-side test do not change. The trade-off: the body does "shiver" internally earlier than the player is told, which is a game-feel choice, not physiology.
- Alternative if Mike wants the physiology to match (Claude's second option, not recommended without a re-tune): move the heat-production onset down too. That lowers resting plateaus (roughly 10 Warmth points colder at rest in cool air by Claude's estimate, unverified) and would need clothing or production retuning.
- The fade-out (full at 33 C, zero at 30 C) stays.
- Tunable: onset and full-intensity core temperatures as Inspector fields so Mike can adjust by feel.

Mike confirms by feel in Play Mode.

**Built 2026-10-10 (Claude Code), BodyHeat compiled and run standalone; SurvivalManager not yet compiled in Unity or Play Mode tested:** `SurvivalManager` has two new Inspector fields, Visible Shiver Onset C (35.5) and Visible Shiver Full C (34.5). `IsShivering` and `ShiverIntensity` (HUD word, Shiver.wav) now follow that visible ramp (`BodyHeat.VisibleShiver`, same fade-out and Hunger fuel). The heat-making ramp, its 4.9x peak, Hunger cost, stamina penalty and `IsWorkingUpHeat` are unchanged, so every resting plateau is unchanged. Standalone results (8 h at rest, full winter clothing, Hunger 80): Pioneer Spring dawn 2 C plateaus at Warmth 70, Settler Winter -1 C at 72, Settler Winter night -9 C at 70 (66-68 with wind, Pioneer -12 C with wind 63): none of them ever show shivering. Soaked at 4 C with wind starts showing in 13-20 min. A dry rest in full winter clothing therefore never shows shivering at these settings, because the body's own shivering holds core temperature above 35.8 C; only wet, sheltered-less or under-dressed cases do. Raise the onset toward 36.0-36.2 to show it on very cold dry nights.

**Built 2026-10-10 (Claude Code report, reconciled by Claude; BodyHeat compiled standalone with dotnet, SurvivalManager NOT compile-checked in Unity, NOT Play Mode tested, uncommitted):** BodyHeat.cs has a new VisibleShiver(...) with its own onset and full temperatures, same fade-out (full at 33 C, zero at 30 C) and Hunger fuel; the heat-making ramp is untouched (starts 36.5 C, peaks 4.9x). SurvivalManager.cs: two Inspector fields under "Shivering and sweat", visibleShiverOnsetC = 35.5 (about Warmth 58) and visibleShiverFullC = 34.5 (about Warmth 46); IsShivering and ShiverIntensity follow the visible ramp, so the HUD word and the Shiver.wav loop follow it with no further edits. The old ramp still drives stamina, the Hunger cost, IsWorkingUpHeat and the heat itself, so resting plateaus are unchanged.

Standalone results (8-hour rests, activity 1, full seasonal clothing, Hunger 80, steady air; Pioneer loses heat 1.3x faster): dry rests NEVER reach the visible onset (Pioneer Spring dawn 2 C plateau Warmth 70; Settler Winter -1 C plateau 72; Settler Winter night -9 C plateau 70, also with wind 3 or 8, plateau 68 and 67; Pioneer -12 C with wind 3 plateau 63), because the body's own shivering holds core near 36.0-36.3 C, above the 35.5 C onset. Old visible onset was 24-38 minutes into those rests. Soaked at 4 C: visible shivering after 20 min (Settler, wind 4 m/s), 17 min (wind 8), 13 min (Pioneer, wind 4), core falling to 24 C with no rescue (old onset 4-5 min). So in practice, visible shivering now appears only when soaked (or otherwise losing heat faster than the body can make it).

**Open decisions for Mike (Claude's recommendations, not confirmed):** (1) Winter night at -9 C dry rest shows no shivering; Claude's read is that this fits "a lot lower", but raising visibleShiverOnsetC toward 36.0-36.2 would show it on very cold dry nights; left at 35.5 as specified. (2) Stamina penalty from shivering stays on the old heat-making ramp, so at plateaus around Warmth 68-70 the old ramp is about 0.2-0.5 and stamina is silently about 6-15% lower; Claude Code and Claude both recommend moving it to the visible ramp (one line in ShiverStaminaFactor) so the penalty only appears when the player is told they are shivering. That would raise stamina slightly in ordinary cool weather versus what Mike tested and passed.

**Decided 2026-10-10 (Mike):** (1) Visible shiver onset stays at the default 35.5 C for now ("i'll start with the default"); he may tune visibleShiverOnsetC by feel after the playtest. (2) The shivering stamina penalty moves to the visible ramp ("I'll take your suggestion"), so it only applies when the player is told they are shivering. Claude Code to change ShiverStaminaFactor to use the visible ShiverIntensity; the heat-making ramp, Hunger cost and plateaus stay as built. Not yet built.

# Warmth Regeneration Too Slow: Mike's Feedback 2026-10-10

**Mike (Play Mode):** "i think warmth regeneration is too slow. we should regenerate heat faster as long as we're moving or even faster if we're working like chopping or digging and running."

**What it is today (from the cold-side build notes above):** core temperature changes with heat produced minus heat lost, divided by the body's heat capacity. Activity already raises heat production through the MET values (0.9 sleeping, 1 idle, 2.5 crouching, 3.5 walking, 5.5 working with a tool, 10 sprinting, plus 1.5 when carrying at the limit), so walking and working do rewarm the player, and a campfire is 250 W of radiant gain. The standalone run found walking holds Warmth at 100 in ordinary winter air and a campfire restores 100 in about a minute, but a player who is already cold (after being soaked, after a sweat chill, after a long cold rest) climbs back slowly because the same heat capacity that makes cooling gradual also makes rewarming gradual. Claude does not know which situation Mike hit; Claude Code should report the actual rewarm times (see below) rather than guess.

**First pass (Claude, not yet confirmed):** keep the physics and add a game-feel rewarming boost that only speeds up the rise, never the fall.

- **Rewarm multiplier on net heat gain while the player is below normal temperature.** Whenever net heat flow is positive and core is below 37.0 C, multiply that net gain by an activity factor: resting or sleeping x1.0 (as built), crouching x1.0, walking x1.5, working with a tool x2.0, sprinting or running x2.5. Taking the highest applicable factor if the player is both working and moving. Campfire warmth is unchanged (it is already fast).
- **Never overshoots:** the boost stops at 37.0 C, so it cannot push the player into Overheating, and it never applies to heat loss, so cooling rates and every resting plateau stay exactly as confirmed in the cold-side tests.
- **Does not make cold free:** if the player is soaked or in a gale, net flow stays negative, so the multiplier does nothing and they must still dry off, build a fire or find shelter. The boost only matters once the body is actually producing more heat than it loses.
- **Cost:** hard movement already burns Hunger through the existing 0.06-per-MET rule and stamina limits sprinting, so no new cost is added in this first pass.
- **Tunable:** the four activity factors and the 37.0 C ceiling as Inspector fields on SurvivalManager so Mike can tune by feel.
- **Alternative (not recommended):** lower the body heat capacity so everything changes faster. That also makes cooling faster and would break the plateaus and the timing Mike already confirmed.

Mike confirms the numbers by feel in Play Mode.

**Built 2026-10-10 (Claude Code; BodyHeat compiled standalone with dotnet, SurvivalManager NOT compile-checked in Unity, NOT Play Mode tested, uncommitted):** BodyHeat.Conditions has rewarmMultiplier and rewarmBelowC; in Advance only the body's own net gain (production minus loss minus dissipation) is multiplied, and only while core is below the ceiling and that gain is positive, so loss, every plateau and campfire watts are untouched. SurvivalManager: Inspector header "Rewarming" with rewarmWalking 1.5 (also Jumping), rewarmWorking 2.0 (eases in with the working level like the work MET), rewarmSprinting 2.5, rewarmCrouching 1.0, rewarmCeilingC 37.0; sleeping and resting x1. Highest of moving and working applies. No defaults changed from the first pass.

Standalone real seconds from Warmth 50 to 90 (75 s per game hour, continuous activity, winter clothing, Hunger 80, wind 1.5 m/s), before then after: Settler -1 C dry: walking 64 -> 43, working 33 -> 17, sprinting 14 -> 6. Pioneer 2 C dawn: walking 98 -> 66, working 40 -> 20, sprinting 16 -> 6. Resting never reaches 90 either way (plateaus Warmth 70 and 68, unchanged). Soaked at 4 C: no rewarming while resting, walking or working (working still falls: after 4 hours Settler Warmth 33, Pioneer 0), only sprinting recovers (Settler 28 -> 11 s, Pioneer 49 -> 20 s); the boost cannot rescue a net loss, as designed.

**Built 2026-10-10 (Claude Code report, reconciled by Claude) and PLAYTEST-CONFIRMED (Mike: "tested it and everything passed"):** warmth rewarming boost. BodyHeat.Conditions gets rewarmMultiplier and rewarmBelowC; in Advance only the body's own net gain (production minus loss minus dissipation) is multiplied, only when positive and core is below the ceiling; campfire watts are added afterwards so the fire is unchanged. SurvivalManager has a "Rewarming" Inspector block and RewarmFactor(activity) takes the higher of the moving factor and the tool-work factor; sleeping is x1; jumping counts as walking; the work factor eases in with the working level like the work MET. Defaults unchanged from the first pass: walking 1.5, working 2.0, sprinting 2.5, crouching 1.0, ceiling 37.0 C.

Standalone numbers (75 real s per game hour, continuous activity, winter clothing, Hunger 80, 1.5 m/s wind), seconds from Warmth 50 to 90, BEFORE the boost: Settler -1 C dry: rest never (plateau 70), walk 64, work 33, sprint 14. Pioneer 2 C dawn: rest never (plateau 68), walk 98, work 40, sprint 16. Soaked Settler 4 C: never for rest, walk and work, sprint 28. Soaked Pioneer 4 C: sprint 49 only. AFTER: Settler -1 C walk 43, work 17, sprint 6; Pioneer 2 C walk 66, work 20, sprint 6; soaked Settler sprint 11, soaked Pioneer sprint 20; soaked walking and working still never rewarm (net flow stays negative, as designed). Resting plateaus unchanged (70 and 68). Finding: walking in cool air (a minute to a minute and a half) was the slow case; working and sprinting dry were already fast; the real problem for soaked players is net-negative heat flow, which this boost deliberately does not fix. Seasonal clothing, damp and sweat chill, sleeping insulation, Pioneer x1.3 loss, saves and the visible shiver ramp were not separately re-simulated (the boost multiplies positive body net only), but Mike's Play Mode test passed.

**CONFIRMED 2026-10-10 (Mike, Play Mode, checks 7-17 all pass):** visible shiver threshold (onset 35.5 C, full 34.5 C; dry rests including a Winter night in full clothing show no shivering, soaked shows it after about 15-20 minutes and fades when very cold; Mike accepts the Winter-night result and the Inspector onset field adjusts it), shivering stamina penalty on the visible ramp (none without visible shivering, toward 70% when visibly shivering), and the warmth rewarming boost (walking, working and sprinting rewarm faster; resting unchanged; stops at 100 with no Overheating; soaked players still lose warmth).
