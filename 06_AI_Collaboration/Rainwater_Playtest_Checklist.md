# Rainwater Collection (Phase 1) - Play Mode Checklist

Written 2026-10-18 (Claude). Feature: Rain Barrel (Water_System.md, "RAINWATER COLLECTION PHASE 1 BUILT 2026-10-18"). Built and compile-checked, never run in Play Mode. Reply with "1. pass", "7. fail - what happened", and so on.

**Setup.**
- Use a COPY of a save that has a finished Small Cabin (use the slot panel's Copy, then load the copy). Item 21 dismantles the cabin, so do not do it on your main slot.
- Have at least 8 Logs and 6 Branches (a Rain Barrel costs 3 Logs + 2 Branches, changed 2026-10-18 after item 2), the Bucket, and the Hammer for the take-down item. Wood gets heavy (Log 8 kg), so stash spare tools in a Tool Rack first.
- Debug keys (editor / development build only, numpad): Numpad 1 = force 12 h of the next weather (Clear, Light Rain, Heavy Rain, Thunderstorm, Clear); Numpad 2 = +1 h; Shift+Numpad 2 = +6 h; Numpad 3 = fill the nearest Rain Barrel within 6 m; Shift+Numpad 3 = empty it; Numpad 7 = show barrel litres and dry days.
- Expected fill per barrel-slope per hour: Light Rain about 14 L, Heavy Rain about 74 L, Thunderstorm about 111 L. A barrel holds 80 L.

## A. Building

1. Craft and Build screen (B) lists a Rain Barrel with a description and a cost. **PASS (Mike, 2026-10-18), shown with the old cost of 5 Logs. After the cost change it should read 3 Logs + 2 Branches: glance at it again.** **2026-10-18 recheck (Mike's screenshot): row still reads "5 Logs, 2 Branches" and Needs shows Logs 3/5, so the Logs cost did not change in the running game (saved scene value overrides the new default). Also the footer said "Needs 4 more Sticks." and the description is cut off after "Holds". Fix BUILT 2026-10-18 (tunable renamed rainBarrelLogCount, footer reason now follows the hovered entry, description shortened); recheck after Mike is back in Play Mode (stop and restart Play Mode first so the old value is gone). Also clear the Console and note whether SeasonalGround NullReferenceExceptions come back.**
2. **BLOCKED (Mike, 2026-10-18): "i can't carry 5 logs at 1 time." 5 Logs is 40 kg, the whole Encumbered threshold. Cost changed to 3 Logs + 2 Branches (26 kg); redo items 2 to 8 after the fix.** Try to place one far from the cabin (out in the open): it is refused with the message "A Rain Barrel stands just outside a Small Cabin's long wall, under the roof's edge."
3. Try at the cabin's door end (the +Z end wall): refused.
4. Try right against the wall, as close as it will go: it is either refused or placed just outside the wall, never inside or overlapping it.
5. Place one just outside a long wall, under the roof edge (roughly half a metre to a metre out): it builds, takes 3 Logs + 2 Branches, and looks bigger than the old Water Barrel with a dark rim band.
6. Place more along the same wall about 1.2 m apart. Note how many fit before it refuses (expected five per side).
7. The other long wall also accepts barrels.
8. The existing Water Barrel still places where it did before (nothing about its placement changed) and still holds 40 L.

## B. Filling

9. Stand next to a Rain Barrel and press Numpad 3: it fills to 80 L (open it with E to see the water). Shift+Numpad 3 empties it.
10. Empty barrel, press Numpad 1 once (Light Rain), then Numpad 2 once (+1 h): about 14 L in the barrel.
11. Empty barrel, Numpad 1 until Heavy Rain, then Numpad 2 twice (+2 h): one barrel on that wall is full (80 L), and with two barrels on the same wall the second holds about 67 L (overflow spills into the next barrel).
12. Thunderstorm: +1 h gives about 110 L spread across the barrels on that wall (first full at 80, the rest into the next).
13. Clear weather: advance 6 h with Shift+Numpad 2, nothing is added.
14. Snow (Winter, or F8): nothing is added even though it is precipitating.
15. Barrels on the opposite wall fill separately from their own slope.
16. Numpad 7 prints a line with barrel litres and the dry-day count.
17. Make it rain (Numpad 1 to Heavy Rain), sleep, wake up: the barrels filled during the sleep.
18. Optional: F5 (skip day) while it is raining, or after a collapse, still fills the barrels.

## C. Using the water

19. Open a barrel with E: the two-way transfer screen shows the water by quality (Good), the capacity as litres out of 80, and the Rain Barrel's own text.
20. A Bucket fills from the barrel; the water in the Bucket is Good quality ("slight risk" when drunk) and can be boiled like any Good water. Pouring a carried Bucket into the barrel also works.
21. With the Hammer equipped and the barrel empty, Take Down Structure refunds half the build materials rounded down: 1 Log and 1 Branch (half of 3 Logs + 2 Branches).

## D. Saving and loading

22. Leave a barrel with an odd amount (for example about 37 L), save, load: the same litres are there. Check with a slot save too, not only autosave.
23. Load an older save from before this feature: it loads normally, no errors, no barrels.
24. On the COPY only: dismantle the cabin (Hammer) while a barrel with a few litres stands next to it, then make it rain and advance a few hours: the barrel's litres do not rise (no roof).

## E. Regression

25. The old Water Barrel still takes water poured from a Bucket, still gives it back, still shows the per-quality rows.
26. The Console is clean through all of this (apart from the usual unrelated warnings).

## Not covered by this checklist (nothing to test yet)

Pond Drying (Phase 2), the lowered pond surface, first-flush, a lid, spoilage, Cistern. A barrel does not freeze in Winter in this version.


## RESULTS (Mike, 2026-10-18)

Mike: "rainwater checklist passed". Logged as ALL 26 items PASS (item 1 earlier by screenshot; items 2-8 also re-run through Placement Mode as item 40 of Build_Placement_Preview_Checklist.md; item 28 of that list spot-checked rain filling). Rainwater Collection Phase 1 is PLAYTESTED. Still uncommitted. Not covered by any checklist: Rain Barrel freezing (waits for the Winter water system), first-flush, lids and spoilage, Cisterns (all later items).
