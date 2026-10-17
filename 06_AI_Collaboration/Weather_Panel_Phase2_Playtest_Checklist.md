# Weather Information Panel, Phase 2 (Weather Station) - Play Mode Checklist

Built 2026-10-17 (Claude Code), compile-checked only (no code errors), NOT yet Play Mode tested, UNCOMMITTED.
Spec: Weather_System.md, "Weather Information and Instruments", block "Phase 2 (next): Layer 3, the Weather Station". The wind-speed factor (40) and every wording choice are Claude's first pass (not yet confirmed); you confirm by feel.

Debug keys: F11 grants the camping kit and now one Weather Station ("Fire, water, wood and weather kit given"), F12 adds $50 (enough to buy one at $25), F4 start/end a Heat Wave, F5 skip a day, F6 next season, F3 body readout.

## A. Setup
1. Back up Slot1 first. Status: OPEN
2. Enter Play Mode with a normal game that has NO Weather Station. The Console starts at 0 errors and 0 warnings. Status: OPEN

## B. Without a station
3. Press K. The Weather tab shows only the free rows (Conditions, Precipitation, Temperature, Wind, Sunlight, Date, Daylight), exactly as before. No locked or placeholder rows. Status: OPEN

## C. Buying and selling
4. Open the Trading Post (T). "Weather Station" is at the end of the stock list at $25. Status: OPEN
5. Buy one (F12 first if you need cash). Switch to the Weather tab (K). Five extra rows appear below the free ones: Humidity, Dew point, Wind speed, Wind bearing, Sunlight value. The page grows to fit and nothing is cut off or overlapping (up to 12 rows; Claude Code added a layout fitter that has not been seen on screen yet). Status: OPEN
6. With the Weather tab open, go to the Trading Post tab, sell the station (you should get $10), then come back to Weather. The extra rows are gone with no stale rows left behind. Buy another to continue. Status: OPEN
7. With the station in the inventory, drop it and check the rows disappear. Pick it up again and they return. A station in Home Storage or lying on the ground does not count. Status: OPEN

## D. What the rows show
8. Humidity reads a sensible percentage (for example 40 to 99%), not a huge number. Status: OPEN
9. Dew point is in Fahrenheit and is never higher than the air temperature shown above it. Check on Settler, and, if you can, on a Homesteader or Pioneer copy (Claude Code left the dew point without the difficulty shift, so check it does not come out above the shifted air temperature). Status: OPEN
10. Wind speed is a number in mph and changes with the wind word (Calm near 0, Gale at 30 mph or more). Status: OPEN
11. Wind bearing shows degrees plus the compass point (for example "315 (north-west)") and agrees with the free "from the north-west" line. It changes after F5. Status: OPEN
12. Sunlight value is a percentage that matches the free word row: 0% at night and indoors, Intense (80% or more) on a clear midday, lower under cloud or in shade. Status: OPEN
13. The numbers update as the tab stays open or you reopen it (humidity and temperature drift with the day). Status: OPEN

## E. Heat Wave and other checks
14. Start a Heat Wave (F4). The free rows still say "Clear"; the Weather Station rows show only numbers (higher humidity and dew point, high sunlight %), and nothing says "Heat Wave". End the wave. Status: OPEN
15. F11 on an existing save gives one Weather Station if you do not already have one, and does not give a second one if you do. Status: OPEN
16. Save and reload with a station in the inventory: it is still there and the rows still show. Load an older save with no station: the tab shows only the free rows. Status: OPEN
17. Performance is unchanged with the tab open and the extra rows showing. Status: OPEN
18. Clean Console at the end of the session (the unrelated MCP WebSocket notice may appear; no errors, no warnings from the game). Status: OPEN

## Results log
(Add dated results here as items pass.)

### 2026-10-17 - note before testing
- Item 9 clarified: the Dew point row shows the RAW dew point (no difficulty shift), clamped so it never reads above the shown air temperature (Mike decided this 2026-10-17 after Claude Code showed the shifted version made Humidity, Dew point and Air temperature disagree on Homesteader and Pioneer). So on every tier the three rows (Humidity, Dew point, Temperature) should agree with each other. Check on Settler, and on a Homesteader or Pioneer copy if you can.
- The final code state is compiled clean (0 errors, 0 warnings). Run the checklist against this build.

### 2026-10-17 - first three screenshots (Mike, no comment)
Screenshots: Inventory with the Weather Station equipped; the Weather tab with the five extra rows; Inventory with the station unequipped.
- 5 FAIL (layout): the five extra rows appear (Humidity, Dew point, Wind speed, Wind bearing, Sunlight value), but the last row ("Sunlight value 71%") hangs below the bottom edge of the Weather panel, outside its frame. The panel does not grow to fit 12 rows; the layout fitter Claude Code added is not resizing the window background. Needs a fix.
- 8 PASS: Humidity reads 56%.
- 9 PASS (on the tier shown): Dew point 65F is below the air temperature (82F). Humidity, dew point and air temperature agree: 82F with a 65F dew point works out to about 56%, matching the Humidity row. Not yet checked on a Homesteader or Pioneer copy.
- 10 PARTIAL: Wind speed 8 mph agrees with "Light breeze". Not yet checked at Calm or Gale.
- 11 PARTIAL: "4 degrees (north)" agrees with the free "from the north" line. Not yet checked after F5.
- 12 PARTIAL: Sunlight value 71% agrees with the word "Strong" (0.55 to 0.80). Not yet checked at night, indoors, or in shade.
- 7 PARTIAL: with the station carried the rows show. Not yet checked drop, pickup, sell, or Home Storage.
- DEVIATION FOUND: the Weather Station can be EQUIPPED (inventory row has a Key slot and shows "Equipped Tool: Weather Station"). The spec says carried, not equipped, like the Tarp, Tent and Sleeping Bag (which have no Key slot). Equipping does nothing. Fix to match the spec unless Mike wants otherwise.
- Still OPEN: 1-4, 6, 13-18, and the remaining halves above.

### 2026-10-17 - fourth screenshot (Mike, no comment)
- 3 PASS (or 6/7 supported): the Weather tab at Summer 4, Year 9 (same moment as the earlier shots, 82F) shows only the seven free rows, no extra rows and no placeholders, and the panel is the normal size. It is not stated whether this was before buying, after selling, or after dropping the station, so items 6 and 7 still need their own confirmation.

### 2026-10-17 - Mike's storage test
- 7 PARTIAL PASS: with the Weather Station stored in the Tool Rack (tool storage) the Weather tab showed only the free rows, as designed (storage does not count).
- Mike also said "with the weather station equipped, it showed more". This does NOT settle whether the rows depend on the station being EQUIPPED or only CARRIED. Claude Code's report says the gate is Player.Has("weather_station") (inventory only), which would show the rows whether or not it is equipped. Not yet tested: station in the inventory but not equipped. Needed because the station is being changed to non-equippable (carried only), so the rows must show for a merely carried station.

## Added 2026-10-17 - re-test after the layout and carried-only fixes
Claude Code fixed (compile-checked clean, NOT Play Mode tested, UNCOMMITTED): rows tightened to 52 px with 4 px gaps (12 rows measure 668 px plus a 16 px top gap in a 740 px content area, so the last row ends 56 px above the bottom; "Esc to close" is in its own strip below); a new InventoryManager.IsCarriedOnly(id) covering Sleeping Bag, Tent and Weather Station (Equip and SetHotkey refuse them, the Inventory screen shows no Key button and the row is not clickable, a save with it equipped or hotkeyed loads un-equipped); the extra rows depend only on the station being in the player's inventory (watched.Has("weather_station")), not on EquippedTool.
Re-run from item 5, plus:
19. Layout: with a station carried, all 12 rows sit inside the Weather panel with a clear margin below the last row and "Esc to close" clear of the rows. Without a station the panel looks as before. Status: OPEN
20. Carried only: the Weather Station row in the Inventory shows no Key button, clicking it does nothing, and you cannot put it on a hotkey. The Tent and Sleeping Bag behave the same. The Tarp and Canteen are unchanged (Key slot, equippable); Mike decided to leave them alone. Status: OPEN
21. Carried but not equipped: with the station in the backpack and nothing equipped, the Weather tab shows the five extra rows. Status: OPEN
22. Old save: load a save where the station was equipped or on a hotkey. It loads with nothing in hand, no Console errors, and the rows still show. Status: OPEN


### 2026-10-17 - re-test screenshot (Mike: "weather station in inventory, not equipped")
Screenshot: the Weather tab with all 12 rows (seven free rows plus Humidity 52%, Dew point 65F, Wind speed 8 mph, Wind bearing "4 degrees (north)", Sunlight value 78%).
- 19 PASS (supported by the screenshot): all 12 rows sit inside the panel with a clear margin below the last row, and "Esc to close" is clear of the rows. The overflow from item 5 is gone.
- 21 PASS: station carried but NOT equipped (Mike's caption) shows the five extra rows. This settles the earlier open question: the gate is "in the inventory", not "equipped".
- Values consistent: 84F air with a 65F dew point works out to about 52% humidity, matching the Humidity row; 8 mph agrees with "Light breeze"; 4 degrees (north) agrees with "from the north"; 78% agrees with "Strong" (0.55 to 0.80).
- Still OPEN: 1-2 (setup: backup, clean Console at start), 4-6 (Trading Post listing at $25, buy shows rows, sell $10 and rows disappear), 7 remaining halves (drop and pick up), 10 (Calm and Gale wind speed), 11 (bearing after F5), 12 (None at night or indoors, lower in shade or cloud), 13 (rows update over time), 14 (Heat Wave: numbers only, no label), 15 (F11 gives one, no duplicate), 16 (save/reload with station; older save without it shows free rows only), 17 (performance), 18 (clean Console), 20 (no Key button, not clickable; Tent and Sleeping Bag the same; Tarp and Canteen unchanged), 22 (old save with station equipped or hotkeyed loads un-equipped, no errors, rows still show), plus item 9 on a Homesteader or Pioneer copy if possible.


### 2026-10-17 - Mike's grouped results (with a Cold Front screenshot)
Mike reported: "trading post - passed", "carried-only - passed", "drop and pick up - pass", "f11 - pass", "live updates - pass", "saves - pass".
- 4, 5, 6 PASS (Trading Post listing, buy shows the rows, sell makes them disappear).
- 20 PASS (carried-only: no Key button, not clickable; Tent and Sleeping Bag the same; Tarp and Canteen unchanged), taken as reported.
- 7 PASS (storage half passed earlier; drop and pick up now pass too).
- 15 PASS (F11 gives one station, no duplicate).
- 13 PASS (rows update over time).
- 16 PASS (save/reload with a station; older save without one shows free rows only), taken as reported. Item 22 (old save where the station was EQUIPPED or on a hotkey) was not named separately, so it stays OPEN unless Mike confirms "saves" covered it.
- Screenshot (Summer 4, Year 9, Cold Front, 79F, wind "Windy, from the north"): Humidity 40%, Dew point 52F, Wind speed 20 mph, Wind bearing 4 degrees (north), Sunlight value 66%. Values agree: 79F with a 52F dew point is about 39-40% humidity (matches); 20 mph is exactly half of the 40 mph full-wind figure, which sits at the bottom of "Windy" (0.50 to 0.75), matching the free word; 4 degrees (north) matches "from the north"; 66% sits inside "Strong" (0.55 to 0.80), matching the free word. The free rows show the real name "Cold Front", so only Heat Wave is masked.
- 10 stays PARTIAL (Light breeze at 8 mph and Windy at 20 mph confirmed; Calm and Gale not yet seen). 11 stays PARTIAL (bearing has read 4 degrees at the same date both times; still need a reading after F5). 12 stays PARTIAL (Strong seen at 66-78%; None at night or indoors and the lower values in shade or cloud not yet seen).
- Still OPEN: 1-2 (setup), 9 on a Homesteader or Pioneer copy, 10 (Calm/Gale), 11 (after F5), 12 (night/indoors/shade), 14 (Heat Wave, numbers only, no label), 17 (performance), 18 (clean Console), 22 (old save with the station equipped or hotkeyed).


### 2026-10-17 - four Weather tab screenshots (Mike, no comment): wind, bearing and sunlight checks
Screenshots, in the order posted:
- A. Summer 8, Year 9: Conditions "Wind", 63F, wind "Gale, from the east", Sunlight "Intense"; Humidity 67%, Dew point 51F, Wind speed 32 mph, Wind bearing 83 degrees (east), Sunlight value 82%.
- B. Summer 9, Year 9: Conditions "Clear", 85F "feels like 85F (Caution)", "Light breeze, from the east", Sunlight "None"; Humidity 45%, Dew point 61F, Wind speed 8 mph, bearing 78 degrees (east), Sunlight value 0%.
- C. Summer 9, Year 9 (same day as B): same weather, Sunlight "Intense"; Humidity 46%, Dew point 61F, 8 mph, 78 degrees (east), Sunlight value 90%.
- D. Summer 9, Year 9: "Cold Front", 79F, "Windy, from the east", Sunlight "Strong"; Humidity 42%, Dew point 54F, 20 mph, 78 degrees (east), Sunlight value 73%.
Checks:
- 10 PASS except Calm: 32 mph is 0.80 of the 40 mph full-wind figure, which is Gale (0.75 and up) and matches the free word; 20 mph = Windy and 8 mph = Light breeze were confirmed earlier. Calm (under 0.10, i.e. under 4 mph) has not been seen yet.
- 11 PASS: the bearing moved with the days (4 degrees on Summer 4, 83 degrees on Summer 8, 78 degrees on Summer 9) and each reading agrees with the free "from the ..." word (4 = north, 83 and 78 = east).
- 12 PASS on the values seen, with one detail unconfirmed: Sunlight "None" reads 0% (B), and the same day outside reads "Intense" 90% (C). Intense at 82-90% (A, C) and Strong at 73% (D) match the word bands (Strong 0.55 to 0.80, Intense 0.80 and up). Not recorded whether B was taken indoors or at night (the 85F air matches C, so indoors looks likely). The lower values under cloud or shade are suggested by the Cold Front reading 73% against 90% when Clear, but not tested directly.
- 8/9 consistency holds on all four: 63F/51F is about 65-67% (shown 67%), 85F/61F about 45% (shown 45-46%), 79F/54F about 42% (shown 42%). No shift-related disagreement.
- Feels-like: 85F at 45% humidity reads "feels like 85F (Caution)"; at 63F and 79F no feels-like text appears (below the 80F heat-index start for the first, and the shown 79F is below 80F). Consistent with the spec.
- Free rows still show the real name for non-Heat-Wave weather ("Wind", "Cold Front", "Clear").
- 12 CONFIRMED PASS (Mike, later): screenshot B was taken inside the small cabin. Roofed/indoors gives Sunlight "None" and 0%, as designed.
- Still OPEN: 1-2 (setup), 9 on a Homesteader or Pioneer copy, 10 (Calm only), 14 (Heat Wave numbers only), 17 (performance), 18 (clean Console), 22 (old save with the station equipped or hotkeyed). Item 12's shade/cloud half is only suggested (Cold Front 73% vs Clear 90%), not tested directly.


### 2026-10-17 - Heat Wave check (Mike: "the last 2 screenshots were during and after a heat wave")
- Screenshot C (Summer 9, Year 9, during the Heat Wave): free Conditions row says "Clear", not "Heat Wave"; 85F "feels like 85F (Caution)", Sunlight "Intense"; Humidity 46%, Dew point 61F, Wind speed 8 mph, Sunlight value 90%. The extra rows show only numbers and nothing anywhere says "Heat Wave".
- Screenshot D (same day, after the wave): Conditions "Cold Front", 79F, Humidity 42%, Dew point 54F, Sunlight value 73%. The numbers came down after the wave (dew point 61F to 54F, humidity 46% to 42%, air 85F to 79F; the sunlight drop to 73% is also what a cloudier Cold Front would give).
- 14 PASS: during the wave the free rows show "Clear" and the Weather Station rows show numbers only; after it, the real name returns. Screenshot B (inside the cabin, same day) shows the same "Clear" and the same numbers except sunlight 0%, so it was very likely also during the wave, but Mike did not say so.
- Still OPEN: 1-2 (setup), 9 on a Homesteader or Pioneer copy (optional), 10 (Calm only, optional), 17 (performance), 18 (clean Console), 22 (old save with the station equipped or hotkeyed).


### 2026-10-17 - performance and Console (Mike: "frame rate seems fine with tab open and the console is clean")
- 17 PASS: frame rate fine with the Weather tab open and the extra rows showing.
- 18 PASS: Console clean.
- Items 1-2 (backup and clean Console at the start) are treated as done; Mike did not report them separately.
- Still OPEN: 22 (old save with the station equipped or hotkeyed loads un-equipped, no errors, rows still show), plus optional 9 (Homesteader or Pioneer copy) and 10 (Calm).
