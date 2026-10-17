# Wood as Direct Fuel - Play Mode Checklist

Built 2026-10-17 by Claude Code (compiles 0 errors / 0 warnings). Not yet play-tested. Design: Core_Survival_System.md, "Wood as Direct Fuel".
Burn times (first pass, Claude, Stick not confirmed): Firewood 2 h, Branch 1 h, Stick 0.15 h (9 min), Log 10 h. Fuel cap 12 h. Cycle key: **F**. E adds one of the selected fuel, Shift+E adds as many as fit.
Setup tips: F11 gives the camping kit; build a Campfire (3 Firewood) or use the Small Cabin hearth. Use the Trading Post or chopping to get Sticks, Branches, Logs and Firewood. Do not play on your only copy of the Saves folder if you want to keep slot states.

Result format: PASS / FAIL / NOTE, with a screenshot for anything odd. Claude logs results here and in Current_Task_List.md.

## A. Basics
1. One kind only: carry only Firewood, look at a fire, press E. Adds 2 h as before. Prompt reads "Add Firewood (+2 h)" and shows no "F: change fuel".
2. Only Logs carried: E adds a Log (+10 h) when there is room (default order only matters when several kinds are carried).
3. Fuel readout and item count both change after each E.

## B. Choosing fuel
4. Carry Firewood, Branches, Sticks and Logs; look at a fire. Prompt starts on Firewood.
5. F cycles Firewood, Branch, Stick, Log, then back to Firewood. A flash shows "Fuel: ... (+N h each)".
6. Cycling skips any kind you do not carry.
7. Drop the selected kind (so you carry none of it): selection falls back cleanly to the first carried kind, no error.
8. E adds exactly one of the selected kind: +2 h, +1 h, +0.15 h, +10 h.

## C. Shift+E
9. 20 Sticks, Shift+E: adds 20 (3.0 h), pack count drops by 20.
10. Many Branches, Shift+E: stops at the cap, extras stay in the pack.
11. Shift+E with Logs: adds as many as fit (at most one Log from an empty fire is 10 h; a second would exceed 12 h).

## D. Cap and tolerance
12. At about 3 h, E with a Log shows "Not enough room for a Log (needs 10 h)"; the Log stays in the pack.
13. At 2 h, a Log fits exactly (12 h).
14. At 11.9 h, Sticks, Branches and Firewood all refuse (no room), nothing consumed.
15. A burning fire at 12 h reads "Burning (..., no room for a ...)" and E gives the flash.
16. A burnt-out fire with leftover fuel reads "Burnt out - add wood to rebuild it" and behaves the same at the cap.

## E. Fire starting
17. Build a Campfire, add only Sticks or a Log, light it with the Bow Drill. Works with no Firewood in the pack.
18. Same with Flint and Steel.

## F. Small Cabin hearth and dismantle
19. Same cycling, adding and cap behaviour on the hearth.
20. Dismantle a cabin with a loaded hearth: refund is whole Firewood (floor(hours / 2)); Logs do not come back as Logs. Note whether that feels fair.

## G. Regression
21. Old save with a burning fire loads and keeps burning at its saved hours.
22. Save, load: the fuel pick resets to the default order (not saved), no error.
23. F away from a fire does nothing; Shift+E on other objects behaves as before.
24. R on a lit fire still puts it out; R on an out fire still adds one piece.
25. Console clean (no errors or warnings) for the whole session.

## H. Feel (Mike's call, tunables on FireManager)
26. Is 0.15 h per Stick right? Is 1 h per Branch and 10 h per Log fine? Note any number to change.
27. Does F feel natural, or would you rather have another key or the mouse wheel?
28. Is the prompt readable at a glance (fuel name, hours, fuel left)?

## Results
- 2026-10-17 (Mike): item 26 PARTIAL: "0.15 h per stick is fine" (Stick value CONFIRMED; Branch 1 h and Log 10 h not separately commented). Item 27 PASS: "F works as described" (F stays as the cycle key). Items 1-25 and 28 not individually reported yet, so they stay open.
- 2026-10-17 (Mike, later): "everything plays good, the cap, shift+E, hearth, everything works normally." Logged as a blanket PASS for sections A-G (items 1-24: basics, choosing fuel, Shift+E, cap and tolerance, fire starting, hearth and dismantle, regression) as reported in one line, not itemised. Item 25 (clean Console) was not explicitly mentioned, so it stays NOT CONFIRMED. Item 20 (dismantle refund feel), 26 (Branch 1 h and Log 10 h) and 28 (prompt readability) were not specifically commented on. Verdict: play-tested, ready to commit once Mike says so.
- 2026-10-17 (Mike): "yes, clean console." Item 25 PASS. Items 1-25 now all PASS (blanket report plus Console). Still uncommented: 20 (dismantle refund feel), 26 (Branch 1 h, Log 10 h), 28 (prompt readability).

## I. Campfire recipe: 3 Firewood OR 6 Sticks (written 2026-10-17, compile pending, not play-tested)
29. Firewood only: with 3 Firewood and no Sticks, the Build tab Campfire entry shows Firewood 3/3 green and Sticks 0/6 red. Building spends 3 Firewood; the fire starts at 6 h.
30. Sticks only: with 6 Sticks and no Firewood, it builds, spends 6 Sticks; the fire starts at 0.9 h (unless Mike changes it to 6 h).
31. Both: spends Firewood, leaves the Sticks.
32. Neither: 2 Firewood and 5 Sticks leaves Build disabled; red reason "Needs 3 Firewood or 6 Sticks."
33. Counts refresh after building; building too close to another fire still gives the spacing error.
34. A Stick-built fire still takes F, E and Shift+E fuel and lights with the Bow Drill; Console clean.
