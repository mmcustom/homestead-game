# Pond Drying (Water Phase 2) - Play Mode Checklist

Written 2026-10-18 (Claude). Feature: Water_System.md, "Rainwater Collection and Pond Drying", Part 2. Built and compile-checked, never run in Play Mode. The water surface does NOT move in this version (level, effects and prompt text only). Reply with "1. pass", "7. fail - what happened", and so on.

**Setup.**
- Back up slot 1 first, or work on a COPY of a save. Stop and restart Play Mode so the old scripts are gone.
- Debug keys (editor / dev build, numpad, while playing): **4** lowers the nearest pond by 10 points, **5** raises it by 10, **6** sets it to 20 percent (below the Bass Hole floor on purpose), **7** shows the Rain Barrel line, weather, dry days and **the nearest pond's level, state and floor**. Numpad 1 forces weather, Numpad 2 skips an hour (Shift: 6 hours). F2 to F12 are the time controls (day skip, season skip, Heat Wave).
- Stand near Bass Hole (centre about -185, -150) for the keys; "nearest pond" is the one whose centre is closest to you.
- Reference numbers: Spring 1 percent a day, Summer 3, Fall 1, Winter 0, doubled in a Heat Wave. Rain per hour: Light +2, Heavy +6, Thunderstorm +10 points. Floor 35 percent. Normal above 66, Low 33 to 66, Very Low below 33.

## A. Level and the numpad 7 overlay

1. New game or a normal save: press Numpad 7 near Bass Hole: it reads "Bass Hole pond 100% (Normal, floor 35%)".
2. Numpad 4 three times: 90, 80, 70 percent; the flash shows the new level each time. Numpad 5 raises it again; it never goes above 100.
3. Numpad 6: 20 percent, state VeryLow.

## B. Drying by season (skip days with the time controls; read the level with Numpad 7)

4. **Summer**, no Heat Wave: one day lowers the level by about 3 points; five days about 15.
5. **Winter**: a day or more changes nothing.
6. **Spring** and **Fall**: about 1 point a day.
7. **Heat Wave** in Summer (the Heat Wave debug key): a day now costs about 6 points, twice the Summer figure. It returns to about 3 when the wave ends.
8. The level is applied once per game day: sleeping through the night costs one day, not several; skipping several days costs one day each.

## C. Rain refill (Numpad 1 cycles Clear, Light, Heavy, Thunderstorm; Numpad 2 skips an hour)

Start from about 50 percent (Numpad 4 until it reads ~50).

9. Light Rain: each skipped hour adds about 2 points.
10. Heavy Rain: about 6 points an hour.
11. Thunderstorm: about 10 points an hour.
12. Clear, Cloudy and **Snow** (if you can get it in Winter): nothing is added.
13. It stops at 100 percent.

## D. Floor at 35 percent

14. Skip many Summer days (or a Heat Wave): the level stops at 35 percent and does not go lower from drying.
15. Numpad 6 (20 percent): state is VeryLow right away. After the next day change the level is back at 35 percent (the floor), state Low.

## E. Effects on drinking and filling

Check the prompt at the pond (aim at the Bass Hole water within about 32 m of its centre).

16. Normal (above 66): "Drink (Questionable water, risk of sickness)" and no pond note.
17. Low (33 to 66; press Numpad 4 until it reads 60): the prompt now says Unsafe water ("high risk of sickness") **and** ends with "Pond water (low)".
18. Very Low (Numpad 6): Unsafe, ending "Pond water (very low)".
19. Fill the Bucket or a Canteen at a Low pond: the water you carry is **Unsafe** (check the item name in the pack), and boiling it works as it does for any Unsafe water (the boiling and filter prompts see Unsafe, not Questionable).
20. Fill at the same pond when Normal: the water is Questionable as before.
21. Drinking straight from a Low pond carries the Unsafe sickness chance (try several drinks, or trust the prompt wording).
22. **Spring Hollow** and the **creek**: prompt and quality are the same at every pond level (Excellent / Good, no pond note), even with the pond at 20 percent.

## F. Fishing

23. Rod or Cane Pole at a **Normal** pond: bite waits as before (about 11 s base, with the usual spread).
24. **Low** pond: waits are clearly longer (about 1.5 times; take a few casts).
25. **Very Low** pond: longer again (about 2.5 times).
26. Creek fishing is unchanged at every pond level.
27. A **Fish Trap** set in the pond: fewer catches over the same hours at Low (about 70 percent of normal) and Very Low (about 40 percent). Compare by skipping the same number of hours at each level; the chance is random, so use several trials. A Fish Trap in the creek is unchanged.
28. No fish disappear from the pond or the traps when the level drops.

## G. Save and load

29. Set the pond to about 45 percent, save, quit to the main menu, load: Numpad 7 reads about 45 percent and Low.
30. Load an **old save** (from before this build, e.g. your slot 1 backup copy): the pond is **100 percent**, Normal.
31. Start a New Game: 100 percent.
32. Load a different save from a running game: its own level, not the previous game's.

## H. Unaffected systems

33. **Rain Barrel**: still fills from rain exactly as in the Rainwater checklist, and the barrel's water is Good, whatever the pond level (Numpad 4 down to 20 percent, then fill from a barrel).
34. The minimap and the pond's shoreline do **not** change with the level (expected in this version).
35. Wading in the pond is the same depth at every level (the surface does not move yet).
36. No red errors in the console while playing, skipping days and loading.

## Known gaps (not bugs to report unless they get in your way)

- The water surface, the shoreline and the minimap do not move; only level, quality, fishing and the prompt change.
- The level isn't shown as a number or gauge in play; only the debug overlay (Numpad 7) shows it.
- The daily drying uses the season and weather at the moment the day changes.
- Water already carried keeps the quality it was collected at.
- A second pond would need a zone with "pond" in its name and, if it should have a floor, an entry in Pond Floor Names / Levels on the Pond Manager (otherwise its floor is 0).


## RESULTS (Mike, 2026-10-19)

Items 1-36: ALL PASS (reported by Mike in one message). Pond Drying (Water Phase 2) is PLAYTESTED. Still uncommitted unless Mike says otherwise in Claude Code. Not covered: the lowered water surface, shoreline and minimap (not built), fish die-off (not built), and the "water already carried keeps its quality" design call (passed as built; Mike has not changed it).
