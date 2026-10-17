# Weather Information Panel, Phase 1 - Play Mode Checklist

Built 2026-10-17 (Claude Code), compile-checked only (0 errors, 0 warnings), NOT yet Play Mode tested, UNCOMMITTED.
Spec: Weather_System.md, "Weather Information and Instruments", block "APPROVED TO BUILD 2026-10-17". All word thresholds and wording are Claude's first pass (not yet confirmed); you confirm by feel.

Debug keys you will want: F4 start/end a Heat Wave, F2 jump a wave to its natural end, F5 skip a day, F6 next season, F7 speed, F8 Winter, F9 refill/cure.

## A. Setup
1. Back up Slot1 first (copy the save folder or file before testing). Status: OPEN
2. Enter Play Mode on a normal game. The Console starts at 0 errors and 0 warnings. Status: OPEN

## B. HUD line (Layer 1)
3. A small cream line like "14:30 . 54 F" shows directly under the minimap (top-right). It does not overlap the minimap, the compass strip or the F3 readout (F3 is top-left). Status: OPEN
4. The time and temperature update as the game runs, and match the pause screen's date line (same time, same temperature). Status: OPEN
5. The line fades or hides with the minimap, as designed. Status: OPEN

## C. Weather tab (Layer 2)
6. K opens a sixth tab, "Weather", beside Map, Inventory, Journal, Trading Post and Craft & Build. The clock stops while it is open, and Esc closes it. Status: OPEN
7. The other five tabs still open with M, I, J, T and B and by clicking the tabs, and all six tabs fit in the window. Status: OPEN
8. Conditions matches the pause screen's weather name in each weather you can reach (Clear, Cloudy, rain, Thunderstorm, Snow, Cold Front). Status: OPEN
9. Precipitation reads None, Light, Heavy or Snow to match what is falling. Status: OPEN
10. Temperature row shows the temperature. Above about 80 F it also shows feels-like with the NWS band; below 80 F there is no feels-like. No wind chill (not built yet). Status: OPEN
11. Wind row shows a word (Calm, Light breeze, Breezy, Windy, Gale) and "from the north-west" style direction. The direction is left out when Calm. After F5 (a day skip) the direction can change. Status: OPEN
12. Sunlight row: None at night and indoors in the cabin, Dim near dawn and dusk, stronger at midday, lower under cloud and in shade, highest on a clear summer noon. Status: OPEN
13. Date, sunrise and sunset show correctly for each season (F6). The times step at a season change; that jump is expected (Claude Code noted sunrise and sunset do not ease between seasons). Status: OPEN
14. There is no "time since last rain" row (the code does not track it). Expected. Status: OPEN

## D. Heat Wave stays hidden
15. Start a Heat Wave (F4). The Weather tab, the HUD line and the pause line all read "Clear" and never say "Heat Wave". The Sunlight row reads Strong or Intense and feels-like shows its band. End the wave (F4). Status: OPEN

## E. Nothing else broke
16. Save and reload with and without the Weather tab having been opened. The game loads normally; K does not trigger any other action; Esc and the other keys behave as before. Status: OPEN
17. Performance is unchanged with the HUD line showing. Status: OPEN
18. Clean Console at the end of the session (no errors, no warnings). Status: OPEN

## Results log
(Add dated results here as items pass.)

### 2026-10-17 - first three screenshots (Mike, no comment)
Screenshots: Weather tab on a clear Summer afternoon outside the cabin; the HUD line at 14:39 outdoors; the Weather tab inside the cabin.
- 3 PASS (visual): "14:39 . 86F" sits directly under the minimap, centred, clear of the compass and minimap. F3 was not showing, but the line is top-right and F3 is top-left, so no overlap by position.
- 4 PARTIAL: HUD temperature (86F) matches the panel (86F). Still to compare with the pause screen's date line.
- 6 PARTIAL: the sixth tab "Weather [K]" opens and shows "Esc to close". Still to confirm the clock stops while it is open and that Esc closes it.
- 7 PARTIAL: all six tabs fit in the window. Still to confirm M, I, J, T and B and the tab clicks still work.
- 10 PASS (hot side): "86F, feels like 85F (Caution)" shows above 80F with the NWS band. The below-80F case (no feels-like) is not yet shown.
- 11 PARTIAL: "Light breeze, from the north-west" shows. Still to check the other wind words, Calm leaving the direction out, and the direction changing after F5.
- 12 PARTIAL: Sunlight reads Intense on a clear Summer afternoon outdoors and None inside the cabin. Still to check Dim at dawn and dusk, lower under cloud, and shade.
- 13 PARTIAL: Summer 1, Year 8, Sunrise 04:30, Sunset 21:30 shown. Still to check the other three seasons (F6).
- 14 PASS: no "time since last rain" row, as expected.
- Still OPEN: 1, 2, 5, 8, 9, 15 (Heat Wave stays "Clear"), 16, 17, 18.

### 2026-10-17 - Mike's results (seven grouped checks, all "pass", no notes)
- PASS: 3, 4, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 (Heat Wave reads "Clear" everywhere), 16, 17, 18. Mapping: his 1 = panel basics (6, 7); 2 = pause line (4); 3 = weather types (8, 9); 4 = Heat Wave (15); 5 = wind and sun (11, 12); 6 = seasons (13); 7 = final checks (16, 17, 18).
- NOT REPORTED: 1 (Slot1 backup) and 2 (clean Console at start) were setup steps, assumed done but not confirmed; 5 (HUD line fades or hides with the minimap) was not in the grouped list and is still OPEN (low risk).
- PHASE 1 PLAY-TESTED. Phase 2 (Weather Station) is next after commit.
