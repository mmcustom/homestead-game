# Flash Floods (Water Phases 1-4) - Play Mode Checklist

Written 2026-10-19 (Claude). Feature: Water_System.md, "Flash Floods, Rising Creek and Pond Overflow". Phases 1 to 4 are built and compile-checked (0 errors, 0 warnings); edit-mode checks passed; never run in Play Mode. **Phase 5 (creek crossing danger: slower wading, being pushed, knock-down) is NOT built**, so the creek is walkable at every stage. The water surface does not rise (no visual change). Reply with "1. pass", "7. fail - what happened", and so on.

**Setup.**
- Back up slot 1 first, or work on a COPY of a save (items for saving and loading change the save). Stop and restart Play Mode so the old scripts are gone.
- Have a Fish Trap or two, a Rod or Cane Pole, a Bucket or Canteen, a Rain Barrel (finished cabin) and a Weather Station if you want to check the panel with it.
- Debug keys (editor / dev build, numpad, while playing): **8** steps the creek's Stream Stage: Normal (0 mm), then High water (18 mm), then Flash flood (28 mm), then back to 0. A rise announces itself and sweeps creek Fish Traps exactly as a real hour would. **9** shows the creek's stage, state, murk hours (and "watch"). **7** shows everything: barrel, weather, pond and creek. **1** forces the next weather (Clear, Light Rain, Heavy Rain, Thunderstorm) for 12 hours. **2** skips 1 hour (Shift: 6). **4/5/6** move the nearest pond level.
- Numbers (first pass): High water from 15 mm, Flash flood from 25 mm, stage drains with a 6-hour time constant; rain adds Light 1.5, Heavy 8, Thunderstorm 12 mm per hour. Murk lasts 12 hours after a flood. Creek: High water one tier worse, flood Unsafe, murk one tier worse. A trap has a 25 percent chance of being swept. Creek bite wait is doubled at High water.
- "Creek" means the default water: the 14 m around Spring Hollow and the 32 m around the Bass Hole are NOT creek and never get the creek effects.

## A. The stage and the states

1. Numpad 9 on a new game or a normal save: "creek Normal 0 mm, murk 0 h".
2. Numpad 8: High water. Numpad 8 again: Flash flood. Again: back to Normal 0.
3. **Heavy Rain** (Numpad 1 twice from Clear): skip hours with Numpad 2 and read Numpad 9 each hour. The stage climbs about 7, 14, 19 (High water after 3 hours), 23, 27 (flash flood after 5 hours). It levels off near 48 mm.
4. **Thunderstorm**: High water after about 2 hours, flash flood after about 3.
5. **Light Rain**: never reaches High water; it levels off near 9 mm.
6. **Clear** after a flood: the stage drains; below 25 mm (no longer a flood) in under an hour from a fresh flood, below 15 mm (no longer High water) in about 4 hours.
7. Snow adds nothing (Winter, if you can get it).

## B. Murk after a flood

8. Reach a flash flood (Numpad 8 twice). Numpad 9 shows murk 12 h.
9. End the storm (Numpad 1 to Clear, or set Numpad 8 to Normal) and skip hours: murk counts down 11, 10, ... and reaches 0 after about 12 hours with no rain.
10. Murk stays at 12 while the creek is in flood, and only starts counting down after it ends.

## C. Creek water quality (aim at the creek away from Spring Hollow and the Bass Hole, so the prompt says Good water at Normal)

11. Normal: prompt reads "Good water, slight risk of sickness" and no creek note.
12. High water: "Questionable water" **and** ends "Creek water (high)".
13. Flash flood: "Unsafe water" ends "Creek water (flooding)".
14. After the flood, back to High water or Normal but still murky (murk above 0): Questionable (one tier worse), no creek note at Normal, "Creek water (high)" at High water.
15. Fill a Bucket or Canteen in a flood: the water you carry is **Unsafe**. Fill after the murk ends: Good again.
16. **Spring Hollow** (stand near the spring pool): Excellent at every stage, no creek note.
17. The creek's last stretch inside the pond's 32 m zone: it behaves as the pond (unchanged by the stage unless the pond is overflowing). This is expected.

## D. Pond overflow (Bass Hole, standing in its 32 m zone)

18. Pond at 100 percent (Numpad 5 until it stops), creek Normal: Questionable, no pond note.
19. Creek at High water (Numpad 8): the pond is "overflowing": Unsafe (Questionable one tier worse) and the prompt ends "Pond water (overflowing)".
20. Pond below 100 percent (Numpad 4 once) with the creek at High water or a flash flood: **no overflow**, no note.
21. Overflowing, then creek back to Normal (Numpad 8 to 0): the pond stays one tier worse and keeps the note for about 12 hours while it is still full; skip 12 hours and it clears.
22. Overflowing, then drop the pond (Numpad 4): the overflow ends at once.
23. Pond Low (about 60 percent) at High water: only the Low effects (no overflow), as in Pond Drying. Pond Very Low: Unsafe, as before.
24. Rain makes the pond rise the same way as before while the stream stage climbs (Pond Drying rain gain: Light 2, Heavy 6, Thunderstorm 10 points an hour).

## E. Weather screen

25. Open the Weather tab (K): there is a **Creek** row under Daylight reading Normal, High water or Flash flood. All rows, including the 5 Weather Station rows when you carry a Weather Station, fit on screen with room below.
26. During Heavy Rain or a Thunderstorm with the creek below flood (set Numpad 8 to Normal or High water, then force Heavy Rain), the row ends "— Flash Flood Watch". It is gone in Clear weather and in Light Rain, and gone once the creek is already in flood.
27. The Creek row appears with and without a Weather Station.

## F. Toasts, banner, sleep and load

28. Normal, then Numpad 8 to High water: the toast "The creek is rising." shows for a few seconds.
29. Numpad 8 again to Flash flood: a large top banner "The creek is in flood." shows for about 8 seconds.
30. Falling back (Numpad 8 to Normal, or the storm ending): no toast.
31. Normal to flash flood in one go (Heavy rain skipped in big steps, or Shift+Numpad 2): only the flood banner, not the rising toast.
32. **Sleep** through a storm (force Heavy Rain, sleep 6 to 8 hours): no toast or banner during the sleep. A moment after the wake message ("Slept ... rested") a line says "While you slept the creek flooded." (or "rose").
33. **Collapse** (Shift+F10 if you have it) through a storm: the same, after waking.
34. **Load** a save that was made during a flood or at High water: no toast and no banner on load, and Numpad 9 shows the saved stage.
35. Menus and pause: no toast while a screen is open.

## G. Fishing

36. **Bite wait at High water.** Cast a Rod or Cane Pole into the creek at Normal several times and note the waits (about 11 s base, wide spread). At High water they are about twice as long.
37. **Pond fishing** during a flood is unchanged by the stage (Bass Hole waits as before, apart from Pond Drying's Low effects).
38. **No casting in a flood.** At a flash flood, aim at the creek with the Rod or Cane Pole: the line reads "The water is too high and fast to fish." and a click does nothing. Aim at the pond: you can cast.
39. **Mid-cast reel-in.** Cast into the creek at High water, wait for the bobber, then Numpad 8 to a flash flood: the line reels in and "The creek is in flood — reeled the line in" shows.
40. A cast in the pond is not reeled in by a flood.

## H. Fish Traps

Set at least two Fish Traps in the creek (away from the 14 m spring zone and the 32 m pond zone) and one in the pond. Put a catch in one if you can.

41. Numpad 8 up to a flash flood: each creek Fish Trap has about a 25 percent chance of being swept away (try several floods or several traps). When one goes the trap and its object disappear and "A flood swept away a Fish Trap." shows (more than one: "... N Fish Traps."). Its contents go with it.
42. The **pond** trap is never swept.
43. A flood sweeps traps **once**: with the creek already in flood, leaving it there for hours or pressing Numpad 8 for the flood level again does not roll again. Down to Normal and up to a flash flood again is a second flood and rolls again.
44. **Never on load**: save while in a flash flood with traps in the creek, quit, load: every trap that was there is still there, and loading again never sweeps one.
45. Sleep through a flood with creek traps: any swept are reported in the summary after waking ("A flood swept away ...") and are gone.
46. A swept trap stays gone after save and load. The Fish Traps you kept still check and pick up normally.

## I. Save, load and old saves

47. At High water, save, quit, load: Numpad 9 shows the same stage and murk. Quit and load a flash-flood save: still a flood, no banner.
48. Load an **old save** (the slot 1 backup copy from before floods): the creek is Normal, 0 mm, murk 0, and the pond has no overflow note.
49. Start a **New Game**: calm.
50. Load a different save from a running game: its own stage, not the previous one's.

## J. Regression: Rain Barrel, Pond Drying, Placement Preview

51. **Rain Barrel**: still fills in rain exactly as in the Rainwater checklist (Numpad 1 then Numpad 2, Numpad 3 fill and empty). Its water is Good and does not get worse in a flood or when the creek is murky.
52. **Pond Drying**: the daily drying by season, Heat Wave doubling, rain refill by weather type, the 35 percent floor, Low and Very Low effects, and the "Pond water (low)" notes all work as in the Pond Drying checklist. Pond Drying's quality shift and the creek/overflow shift add up and never go past Unsafe.
53. **Pond Drying rain** is still applied once an hour with the stage (forcing Heavy Rain for an hour raises the pond about 6 points and the stage about 6 mm).
54. **Placement Preview**: open Craft and Build, start a Wood Pile and a Rain Barrel: ghost, green or red, wheel and Esc behave as in the Placement checklist. Nothing about placing changed.
55. No red errors in the console while playing, skipping days, sleeping and loading.

## Known gaps (not bugs to report unless they get in your way)

- **No creek danger yet** (Phase 5): the creek can be waded at every stage, with no slowing, pushing or knock-down.
- The water surface and the minimap do not change; only state, text, quality, fishing and traps do.
- Creek water inside the 32 m pond zone and the 14 m spring zone is not "creek": no flood quality, fishing effect or trap sweep there.
- Murk and overflow are quiet: the prompt shows a creek note only at High water or in a flood, and the pond note only while overflowing; the water simply stays one tier worse during murk.
- A Fish Trap in creek water is swept only at the moment the creek rises into a flash flood; a trap set during a flood is safe from that flood.


## RESULTS (Mike, 2026-10-19)

Items 1-55: ALL PASS (reported by Mike in one message). Flash Floods phases 1-4 are PLAYTESTED. Still uncommitted. Not covered: phase 5 (creek crossing danger: slower wading, being pushed, knock-down; not built), any visual change to the water surface or minimap (not built), and the known gaps listed above.
