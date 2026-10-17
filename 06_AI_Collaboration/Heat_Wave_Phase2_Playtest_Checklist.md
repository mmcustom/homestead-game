# Heat Wave Phase 2 Play Mode Checklist

Written 2026-10-17 (Claude). Run AFTER Claude Code reports the two follow-up fixes (wading no longer gated by air temperature; Homesteader Heat Wave odds x0.6) compiled clean with Play Mode off. Back up Slot1 before the save/load items (only one slot exists).

Keys: F6 next season (until Summer), F4 start Heat Wave, Shift+F4 strong wave, F4 during a wave ends it (debug end, no relief storm), F2 jumps a wave to its natural end (relief storm roll), F3 live body readout (shows wading share and water watts while wading), F9 refill and cure.

Reply with the item number and pass or fail, plus numbers where asked.

## A. Setup
1. Clean Console after Unity reloads and on entering Play Mode.
2. The new SurvivalManager and WeatherManager fields show in the Inspector with sensible defaults.

## B. Feels-like on the pause screen
3. Pause (Esc) on a hot day (80 F or above): the date line reads "..., NN F, feels like NN F (Band name)". Note the air temperature, feels-like and band.
4. Pause on a cool day (below 80 F): no feels-like text.
5. Pause during a Heat Wave: feels-like is at or above the air temperature in humid air; in very dry hot air the line may show "feels like" with no band (known edge case).

## C. Wading cools
6. Get Hot or worse (Shift+F4 strong wave, work in full sun). Wade into the creek (knee depth, about 0.45 to 0.8 m): does core temperature fall? Check F3 for the wading share and water watts, and say roughly how fast (C per game hour).
7. Wade deeper in the pond (waist or deeper): cooling stronger than the creek?
8. Stand in very shallow water (ankle depth): only a small effect?
9. Leave the water: a fading extra cooling for about 30 game minutes, then it stops?
10. Stay in the water while already cool: core temperature does NOT drop below about 36.5 C (no chilling).
11. Air cooler than 28 C (for example a mild Summer day, or after the relief storm) and you are overheated from work: does wading still cool you? (This is the fix being tested.)
12. Wading never makes you "Wet" or "Soaked" on its own.

## D. Cold side unchanged
13. Winter or cold rain or snow: Wet and Soaked still appear and still chill you exactly as before. Hard work in cool air still gives Damp.
14. CHANGED BEHAVIOUR (2026-10-17): wading now works at any air temperature, so wading in cold weather (for example Winter, creek knee-deep) chills you. Expect the Body temperature meter to dip toward cool and core temperature to stop falling at about 36.5 C (the taper), and nothing worse from the water itself. Tell me: is that chill too harsh, about right, or too mild for crossing the creek in Winter? Also check that you do not become Wet or Soaked from wading.

## E. Relief at the end of a wave
15. Start a wave (F4), then F2 (jump to natural end): the next weather is a Thunderstorm or a Cold Front. Repeat 5 or more times (new wave each time) and note how many Thunderstorms versus Cold Fronts (target about 70 / 30).
16. Start a wave (F4) and end it with F4 again: the next weather is NOT forced to a storm.
17. Let a wave run past the end of Summer, or F6 out of Summer during a wave: no forced storm.
18. After a relief Thunderstorm: does the temperature drop and the heat ease off? (Trees can fall in a Thunderstorm; note it if one does.)

## F. Nothing else broke
19. Save during a Heat Wave and reload: the wave and body temperature come back. Then an older save (no Heat Wave fields) loads with no errors.
20. Performance feels unchanged walking around, including near water.
21. Clean Console afterward.

Tell me for 6, 7 and 11 how fast the cooling feels (too weak, about right, too strong). Those are the tuning numbers.


## Added 2026-10-17 - "Clear" label check (from Mike's decision)
22. During a Heat Wave (F4), open the pause screen. The weather line reads "Clear" (never "Heat Wave"), and the feels-like line still shows the heat with its NWS band. Status: OPEN (built and compile-checked, not yet seen in Play Mode).
23. End the wave (F4 again) and open the pause screen. Normal weather names (Cloudy, Thunderstorm, etc.) still show correctly. Status: OPEN.


## Status update 2026-10-17 (after Mike's screenshots)
- 22: PASS. Pause line "Clear, 85F, feels like 83F (Caution)" during a Heat Wave.
- 23: OPEN. Screenshot showed "Clear, 79F", which cannot distinguish a natural Clear from the hidden Heat Wave. To prove it: F4 to start a wave, F2 to jump it to its natural end, open the pause screen; it should read Thunderstorm or Cold Front.
- 13: supportive evidence only (screenshots at 0 C and -1.5 C show wet 0.12 then 0.00, taper 0.00, Shivering, then fast warming at the fire). Awaiting Mike's confirmation.
- 11: OPEN. Needs core temperature at Hot (37.5 C) or above in air below 28 C while wading; the posted screenshot had core 36.82 C.


## Status update 2026-10-17 (second screenshot batch)
- 23: PASS. Pause line "Thunderstorm, 57F" after the wave ended.
- 13: PASS (supported). Wet/Damp labels and drying behave at 14 C, 11.5 C, 0 C, -1.5 C and -2.8 C air.
- Open: 8, 11, 19, 20, 21.


## Status update 2026-10-17 (Mike)
- 20: PASS. Performance near water is fine.
- 21: PASS. Console is clean.
- Open: 8, 11, 19.


## Status update 2026-10-17 (shallow-wade screenshot)
- 8: PASS (supported). Wading 0.34 gave 85 W taken versus 208 W at 0.83 depth.
- 11: DEFERRED (untested edge case).
- Open: 19.


## Status update 2026-10-17 (Mike)
- 19: PASS. Save during a wave and reload works; an older save with no Heat Wave fields loads cleanly.
- CHECKLIST COMPLETE. Only item 11 remains deferred (untested edge case).
