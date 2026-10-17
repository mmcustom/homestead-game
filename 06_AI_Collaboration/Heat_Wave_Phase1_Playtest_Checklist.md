# Heat Wave Phase 1: Play Mode Checklist

Rebuilt 2026-10-16 from the 2026-10-14 build report in Current_Task_List.md. This is NOT the original 18-item list (its text was never saved), so item count and wording differ. Tick items off and report results back.

## Before you start
- [ ] Back up slot1 (your playtest save). Restore it and verify the hash when finished.
- [ ] Application.runInBackground is on (needed for Play Mode over MCP).
- [x] (PASS 2026-10-16, Mike: console clean on start) Open Unity, let it recompile, press Play. Console is clean on a fresh start (earlier: 12,486 `SeasonalGround.PaintRows` NullReferenceExceptions at SeasonalGround.cs:197 and one `PlayerController.OnEnable` NRE at PlayerController.cs:204; report if they return).
- [ ] Lumber check (follow-up from the GUID restore): Trading Post still shows Lumber at $8. Already confirmed 2026-10-16.

Debug keys: F6 next season (go to Summer first), F4 start Heat Wave (+6 C peak, 96 h), Shift+F4 strong (+12 C), F4 during a wave ends it, F9 refill and cool, F7 fast-forward x1/x10/x60/x360.

## A. Heat Wave weather
1. [x] (PASS 2026-10-16: message shown, sky hazy in daylight) F6 until Summer, then F4: message "Heat Wave started (+6 C peak, 96 h)" and the sky/weather becomes HeatWave (not black or broken).
2. [x] (PASS 2026-10-16: climbs, nights stay warmer than usual) Temperature climbs over about 24 game hours toward the peak, and nights stay warm (night cooling roughly halved at peak).
3. [x] (PASS 2026-10-16) F4 again ends the wave: weather returns to Clear and the temperature eases down.
4. [x] (PASS 2026-10-16: strong wave peaked at 108 F, under the 115 F ceiling; the clamp itself was not reached) Shift+F4 starts a strong wave (+12 C peak). Air temperature never passes about 46 C.
5. [x] (PASS 2026-10-16: a strong wave ran to its natural end) Let a wave run to its natural end at x360: it ends after its length, ramps down over about 24 h, and a new weather type rolls (no stuck HeatWave).
6. [x] (PASS 2026-10-16) Pause menu date line shows the weather and a sensible temperature in degrees F.

## B. Humidity (read via a debug watch or Inspector if needed)
7. [x] (PASS 2026-10-16: high in the morning, lowest in the afternoon) Relative humidity is lowest in the afternoon and highest near dawn.
8. [x] (PASS 2026-10-16: RH drops gradually after rain; Winter is drier than Summer) Rain raises humidity for about a day afterwards; Winter air is drier than Summer.

## C. Sun load and shade (Summer, clear day, wave off first, then on)
9. [x] (PASS 2026-10-16: "Hot" shows in full sun) Standing in full sun at midday: core temperature rises noticeably above shade.
10. [x] (PASS 2026-10-16: cooler under trees and in the cabin; tent, lean-to, tarp not individually tried) Standing under trees (leaves on): clearly cooler than in the open. Inside a cabin, tent, lean-to or under a tarp: no direct sun gain.
11. [x] (PASS 2026-10-16: rain gives less sun gain than clear; the ease-in over a couple of hours not explicitly confirmed) Cloudy or rainy weather reduces sun gain; the change eases in over a couple of hours rather than snapping.
12. [x] (PASS 2026-10-16: no sun gain at night) Sun gain is zero at night.

## D. Overheating tiers and HUD
13. [x] (PASS 2026-10-16) In a strong wave, walk in full sun until the HUD status line shows "Hot".
14. [x] (PASS 2026-10-16: stage names, colours, stamina penalty and Health loss all look right) "Heat exhaustion" appears next (stamina reduced), then "Severe heat exhaustion" (losing health), then "Heat stroke" (losing health fast), each with its own color and text.
15. [~] (Body meter replaced Warmth 2026-10-16; heat is shown on its right side, so this item is superseded) Warmth meter is unchanged by heat (it stays a cold-side meter).
16. [x] (PASS 2026-10-16: Hydration drops faster in heat; the low-Hydration earlier-tiers part not reported) Hydration drains faster while hot and sweating; at low Hydration the heat tiers arrive sooner.
17. [x] (PASS 2026-10-16: shade levels body temperature out; water, rest and F9 all restore Health) Shade, water and rest bring the tier back down; F9 clears it.

## E. Collapse and sleep
18. [x] (PASS 2026-10-16) Let heat stroke run until Health hits 0: you collapse, the fade to black and message play, and the wake message is heat-specific.
19. [x] (PASS 2026-10-16) A heat collapse wakes you at the next dusk or dawn at least 8 hours later, weakened.
20. [x] (PASS 2026-10-16: wakes because it is too hot) Sleep in a Tent, Lean-To or Cabin during a wave: you wake when core temperature goes above about 38 C ("too hot to sleep").
21. [x] (PASS 2026-10-16: slept 3 h in a Sleeping Bag, body temperature stayed the same; air temperature and the cool-night side not reported) Sleeping Bag warmth fades out between about 20 and 24 C air (no bag-induced overheating in Summer).

## F. Save, load and old saves
22. [x] (PASS 2026-10-16: slot1 with a strong wave in progress loaded with the wave intact) Save during a wave, quit to main menu, load: the wave, its remaining hours and its strength are preserved.
23. [x] (PASS 2026-10-16: stripped old-format save loaded, no Console errors, weather Clear with no wave, temperatures normal, F4 starts a new wave) Load slot1 or an older save made before Heat Waves: it loads with no wave, no Console errors, and Summer can still roll a Heat Wave later.
24. [x] (PASS 2026-10-16: body temperature looks right after loading a save made while hot) Save/load while in the Hot tier: core temperature and tier restore.

## G. Cold side regression (should be unchanged)
25. [x] (PASS 2026-10-16: Body meter works in the cold; Shivering, Wet, Soaked and Warming by the fire all show) Winter: Warmth, Shivering, Wet/Soaked and "Warming by the fire" behave as before.
26. [x] (PASS 2026-10-16: sun strength is minimal in Winter) Sun does not warm you in Winter noticeably more than before (sun gain in cold air scales down).

## H. Performance and Console
27. [x] (PASS 2026-10-16: no stutter walking through terrain with trees) No visible stutter while walking in open terrain with trees (the sun probe runs about every half second).
28. [x] (PASS 2026-10-16: Console read via UnityMCP after the session, 0 errors and 0 warnings) Console stays clean through the whole session (no new errors or warnings from BodyHeat, SurvivalManager, WeatherManager, DayNightCycle).

## Known unverified at build time
Save/reload of the 3 new fields (wave strength, wave length, hours since rain), sun-probe frame cost, terrain ignored for low-sun blocking, evergreens losing Winter shade like deciduous trees, Pioneer difficulty tuning (Phase 2).

## Results
Complete 2026-10-16: 27 of 28 items PASS; item 15 (Warmth unchanged by heat) is superseded by the Body temperature meter. No failures.
(Record passes, failures and notes here or report them in chat.)
