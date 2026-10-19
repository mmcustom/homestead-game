# Structures Slice 1, Phase A: Well + Outhouse - Play Mode Checklist

Built 2026-10-19; staged Outhouse added the same day. Compile-checked; 90 edit-mode tests pass; no work action in this build (digging the shaft, a new pit, pounding) advances the clock. PLAYTESTED 2026-10-19: all 58 items pass (Mike). UNCOMMITTED. All numbers are first-pass tuning (Claude, not yet confirmed); each is a public field on the "Wells And Outhouses" object (created at startup).

**Setup:** back up `slot1` first (restore and hash-verify after). Carry: Shovel, Hammer, Bucket, 10 Stone, 4 Logs, 12 Branches, 3 Tall Grass (debug item keys / Trading Post). Build tab: "Well Site", "Outhouse Site", "Water Barrel Site", "Rain Barrel Site", "Food Cache Site" (all staged). Carry the Outhouse materials in two lots to test the trips: 2 Logs first, then 6 Branches + 3 Tall Grass.
**Debug keys:** Numpad 0 nearest Well to 0 L (Shift fills, Ctrl makes it Unsafe); Numpad . nearest Outhouse pit to full (Shift empties); Numpad 8 creek stage (flash flood); F-keys / Numpad 2 skip time.
**Weights (kg):** (staged-site cutoff 15 kg, `stagedSiteWeightKg`; Water Barrel 24, Rain Barrel 26, Food Cache 20, Outhouse 22.3 are all staged now; Wood Pile 0.4, Storage Bin 6.6, Tool Rack 4.4 stay single-step) Stone 5, Logs 8, Branches 1, Tall Grass 0.1, Clay 3.5. Outhouse total 22.3 (too heavy for one trip, so it is a staged site: 2 Logs = 16 kg, then 6 Branches + 3 Tall Grass = 6.3 kg); Well lining 5 Stone per trip = 25; Well cap 2 Logs + 4 Branches = 20.

## A. Placement ghost and refusals
1. [ ] Build tab lists Well Site and Outhouse Site, both "free to mark out; the ... then takes ... over several trips".
2. [ ] Both show the green/red ghost; the Outhouse turns with the wheel (door faces the way you turn it); the Well Site does not rotate.
3. [ ] Outhouse within 30 m of the creek, pond zone edge or spring zone edge: red "Too close to the creek: 30 m needed." (names the water).
4. [ ] Outhouse within 7.5 m of the Small Cabin: red "Too close to the Small Cabin: 7.5 m needed."
5. [ ] Well Site within 3 m of the Small Cabin: red "Too close to the Small Cabin: 3 m needed."
6. [ ] Well Site inside Spring Hollow or the pond zone: red "Not inside <zone>...".
7. [ ] Low ground next to the creek: red "This ground floods." for both.
8. [ ] A dip in the ground (hollow): Well Site red "In a hollow: rain would run into the well."
9. [ ] With a Well down, an Outhouse within 30 m: red "Too close to the well: 30 m needed."; with an Outhouse down, a Well Site within 30 m: "Too close to the outhouse...".
10. [ ] Uphill/downhill: place an Outhouse, then a Well Site 31+ m away on LOWER ground: red "The outhouse is on higher ground: a well must stand uphill of it."; an Outhouse placed uphill of a Well (31-60 m): red "The well is downhill of here...".
11. [ ] Valid spots are green. A Well Site places with one click and NO Hammer, spending nothing. The Outhouse Site places the same way with NO Hammer (see E2).

## B. Well Site (staged)
12. [ ] Prompt on the site reads the stage: "Dig the shaft: equip the Shovel, hold click (0%)".
13. [ ] With the Shovel equipped, hold click on the site: about 40 blows land (one about every 0.9 s, so roughly 36 seconds of real time), the shaft darkens/deepens, the progress bar fills, stamina drains per blow, and the dig sound plays. Digging takes real time only: the game clock does NOT jump per blow or at the end (watch the clock), and nothing is skipped.
14. [ ] Stop half way, walk off, come back, save and load: the dig stays where it was.
15. [ ] Stone, Logs and Branches are refused (R) before the shaft is dug.
16. [ ] After the shaft: R with 5 Stone stores 5 (partial delivery); the prompt says "5 of 10". A second trip of 5 fills the lining.
17. [ ] Carrying more than needed (e.g. 12 Stone with 10 needed): R takes only what is needed.
18. [ ] Logs and Branches are refused until the lining is full; then R stores 2 Logs and 4 Branches (partial deliveries fine).
19. [ ] The last delivery does NOT finish the Well: a banner says "Well Site fully stocked - equip the Hammer and hold click on it to build it". The prompt reads "Fully stocked - equip the Hammer to build (0%)" without the Hammer (prompt only: click does nothing) and "Hold click with the Hammer to build (0%)" with it. Holding click with the Hammer lands blows (bar fills, stamina drains, chop sound; real time only, no clock jump); at 100% the site becomes the Well in place with a full buffer and the materials are used up (see E2).
20. [ ] E on the site with materials in it takes them back (as far as the pack allows), no Hammer needed; E on an empty site takes it down only with the Hammer (see E2).

## C. Well
21. [ ] Prompt: "Draw water (Good water, 150 of 150 L)". E with the Bucket equipped fills it (10 L); the litres drop.
22. [ ] E with nothing equipped drinks (1 L, +20 Hydration, drinking sound); a small illness chance as other Good water.
23. [ ] Numpad 0: Well at 0 L; prompt "dry for now - it recovers 6 L per hour"; E says the same.
24. [ ] Skip 1 h (Numpad 2): +6 L; skip a night: refills. In a drought (Summer, 5+ dry days): +3 L per hour.
25. [ ] Shift+Numpad 0 fills it; the water disc in the shaft rises with the level.
26. [ ] Ctrl+Numpad 0: Unsafe water, prompt notes "flood water got in"; the filled Bucket holds Unsafe water; it counts down hourly and clears (48 h).
27. [ ] Flash flood (Numpad 8) with a Well within 40 m of the creek channel: it turns Unsafe at the next hour; a Well far away is untouched.
28. [ ] Water from the Well boils to Purified Water at a campfire like any other; barrels accept it.
29. [ ] Build an Outhouse within 30 m of an existing Well on a save made without the rule (or via debug): the Well's water shows Questionable and the prompt says why.

## D. Outhouse and the sanitary homestead
30. [ ] Prompt: "Outhouse (pit 0% full...)". Sleeping in a shelter within 60 m, then Numpad . (Shift empties): "sanitary homestead: yes".
31. [ ] Sanitary: drinking from a creek/pond and eating raw food make you ill a little less often; sleeping gives a slightly better Health recovery (+0.15 on the multiplier).
32. [ ] Numpad .: pit full; prompt "the pit is full" plus "Equip the Shovel and hold click to dig a new pit"; the shack looks darker; sanitary becomes "no" and the bonus is gone.
33. [ ] Full pit, Shovel equipped, hold click on the outhouse: about 10 blows land (one about every 0.9 s, roughly 9 seconds), the dig sound plays, stamina drains per blow, the progress bar and the prompt percentage fill. There is NO time jump: the game clock keeps running normally and never skips (watch the clock). At 100% the outhouse moves 2 m to the side onto a fresh pit and is 0% full; sanitary again. Stop half way, walk off and come back: the progress is kept (save/load too).
34. [ ] Full pit with a setback in the way both sides: the Shovel shows "No room to dig a new pit here - dismantle it and build the outhouse somewhere else" and no blow lands.
35. [ ] Without the Shovel, the prompt says to equip it and hold click; E on a full pit just tells you to hold click with the Shovel, and on a pit with room says how full it is.

## E. Dismantle
36. [ ] R on a finished Well or Outhouse without the Hammer says it needs the Hammer; the prompt says half the materials back, rounded down, a single item refunds nothing.
37. [ ] With the Hammer: Well returns 5 Stone, 1 Log, 2 Branches; Outhouse returns 1 Log, 3 Branches, 1 Tall Grass; what does not fit lands on the ground.

## E2. The Hammer rule (Mike's final wording, 2026-10-19)
The Build screen is permanent and the Hammer does the construction; the Craft tab never needs the Hammer. Free with no Hammer: the ghost, placing a staged site (Well, Outhouse, Water Barrel, Rain Barrel, Food Cache Site, Small Cabin site), delivering materials, taking materials back. Hammer: the click that builds a single-step structure, finishing a staged site by pounding it with the Hammer at the site (hold click; the Well still needs its Shovel dig first), taking an empty site down, Dismantle. The Shovel does the digging (the shaft, a full Outhouse pit). Equip the Hammer with its number key; the same key puts it away.

**Outhouse (staged site; builds over the 15 kg cutoff, first-pass tuning)**
44. [ ] Build tab: "Outhouse Site" (free to mark out; the Outhouse then takes 2 Logs, 6 Branches, 3 Tall Grass over several trips), Place Site. Hammer NOT equipped: the entry has no Hammer note and Build starts Placement Mode.
45. [ ] While aiming, the Outhouse Site ghost is the finished shack (door faces the way you turn it), turns with the wheel and is GREEN on a good spot with no Hammer; the click places the plot (four stakes round a pit square), spending nothing. A bad spot is red with its own reason (item 3-10 rules apply to the site).
46. [ ] Deliver with R, no Hammer: carry only the 2 Logs (16 kg) to the site and press R (stores 2); walk away, fetch 6 Branches + 3 Tall Grass (6.3 kg), R again. At no point is the pack Encumbered (30 kg): a 22.3 kg Outhouse never has to be carried in one trip. A partial delivery (3 Branches) works; Stone and Sticks are refused; over-delivery is refused. The prompt shows "n of m" for each material. Save and load half-delivered: the site and its deliveries are restored.
47. [ ] The last delivery shows the banner "Outhouse Site fully stocked - equip the Hammer and hold click on it to build it" and does NOT finish it. No Hammer: the prompt says "Fully stocked - equip the Hammer to build" and clicking does nothing. Hammer equipped: "Hold click with the Hammer to build (0%)"; hold click and the bar fills (about 20 blows, real time only); at 100% the site turns into the Outhouse in place, the materials are used up and the pit starts empty. There is NO Shovel stage on the way (the Shovel only digs the new pit of a full Outhouse, hold click). Partial pounding: stop at about 50%, walk away, come back and carry on from 50%. Save and load mid-pound: progress and deliveries are restored. After the first blow the materials are locked: E says "The materials are built in now" and the prompt says so; before it, E takes materials back with no Hammer. E on an EMPTY site takes it down only with the Hammer. There is no "Complete" entry on the Build tab.

**Well Site (staged)**
48. [ ] Placing the Well Site needs NO Hammer: while aiming, the ghost is the finished Well (ring, posts, roof), green on a good spot, and it does not turn; the click places the plot (stakes and earth). The same refusals show on the Well ghost: "Too close to the outhouse...", "In a hollow...", "The outhouse is on higher ground...", "This ground floods.".
49. [ ] Delivering (R) Stone, then Logs and Branches needs NO Hammer; the R prompt is just "Store ..." (no "Equip the Hammer" text anywhere on delivery). Taking materials back (E) needs no Hammer.
50. [ ] The shaft is dug with the Shovel only (the Hammer does not dig).
51. [ ] Fully stocked Well Site (shaft dug, lined, capped): the delivery that fills it shows the banner and does not finish it. There is no "Complete Well" entry on the Build tab (it keeps only "Well Site"). Without the Hammer: prompt only. With it, hold click on the site: about 30 blows (real time only, no clock jump) build the Well. Stop half way, walk off, come back, save/load mid-pound: the progress stays. Once pounding has begun E no longer takes the Stone, Logs or Branches back.
52. [ ] An EMPTY Well Site without the Hammer: the prompt says "Equip the Hammer to take the site down" and E only says so; with the Hammer, E fills it in.
53. [ ] Dismantle (R) on a FINISHED Well or Outhouse without the Hammer says "Can't take the ... down - equip the Hammer."; with it, half of each cost line back rounded down (Outhouse: 1 Log, 3 Branches, 1 Tall Grass). Nothing removes a Well, Well Site, Outhouse Site or Outhouse without the Hammer (try E, R, the Shovel).

**Existing Build-tab structures (Water Barrel, Rain Barrel and Food Cache now staged; the rest single-step)**
54. [ ] Water Barrel Site (3 Logs, 24 kg), Rain Barrel Site (3 Logs + 2 Branches, 26 kg), Food Cache Site (2 Logs + 4 Branches, 20 kg): each is "free to mark out; the ... then takes ... over several trips". Place Site with NO Hammer (while aiming the ghost is the finished barrel or cache; the placed site is the plot; green ghost, one click, nothing spent; the Food Cache Site turns with the wheel). Deliver in TWO trips each, no Hammer, never Encumbered: Water Barrel 1 Log then 2 Logs; Rain Barrel 2 Logs then 1 Log + 2 Branches; Food Cache 1 Log, then 1 Log + 4 Branches. R prints "n of m"; partial deliveries work; over-delivery and other items are refused; E takes materials back with no Hammer; E on an EMPTY site needs the Hammer. The last delivery shows the "fully stocked - equip the Hammer and hold click on it to build it" banner and does not finish it. No Hammer = prompt only. With the Hammer, hold click: Water Barrel about 15 blows, Rain Barrel about 16, Food Cache about 12 (real time only: the clock does not jump per blow). Walk away at half way and come back: progress kept. Save/load mid-pound: progress kept. After the first blow E no longer takes materials back. At 100% the finished barrel/cache appears in the same spot and facing, empty, and works as before (pour in, draw, store food). Save/load half-delivered sites: deliveries restored.
54b. [ ] Ghost check (all five staged sites): while aiming, the ghost is the FINISHED structure (Well: stone ring, posts, roof; Outhouse: shack; Water Barrel; Rain Barrel; Food Cache: lidded box), green/red as before; the Food Cache and Outhouse turn with the wheel, the Well and the barrels do not; the placed site is the plot (stakes and earth) until completed. The ghost cannot be aimed at, bumped into or interacted with, and a placed Well's interaction box and water do not block or confuse aiming a second Well ghost.
54a. [ ] Wood Pile, Rock Pile, Storage Bin, Tool Rack stay single-step: the Build tab starts Placement Mode with no Hammer; the ghost is red "Equip the Hammer to build." until the Hammer is equipped, then the click builds as before.
55. [ ] Rain Barrel eave: while aiming, the ghost is the finished Rain Barrel outline (the hollowed-log barrel with the dark rim band), green/red as before; once placed, the site is the plot (four stakes, earth, delivered wood) and is not a barrel until completed. The same holds for every staged site (see the Ghost check below). The ghost snaps to a finished Small Cabin's long-wall eave exactly as the barrel did (slope name and roof area in the note; red "A Rain Barrel stands just outside a Small Cabin's long wall, under the roof's edge." when no eave is in reach; 1.2 m spacing from another Rain Barrel or Rain Barrel Site; same refusals for water, steepness, spacing, fire). The Rain Barrel completed from that site stands where the site was snapped and fills in rain (Numpad rain debug) from that roof slope. A Rain Barrel Site does NOT collect rain while still a site. If the cabin is dismantled before the site is completed, the finished barrel still completes but no longer fills (a plain barrel).
55a. [ ] Old saves: load a save made before this change with a finished Water Barrel (some water), a finished Rain Barrel (rain collected) and a Food Cache (stored food): all three load as finished storage with the same contents, no sites. Dismantle (Hammer, R) still refunds half the Logs/Branches, rounded down.
56. [ ] Small Cabin: placing a site and delivering stay free. There is no "Complete Small Cabin" entry any more: the Build tab only has "Small Cabin Site" (Place Site). Fully stocked with no Hammer: prompt only ("Fully stocked - equip the Hammer to build (0%)"). With the Hammer, hold click on the site: about 60 blows (real time only, no clock jump) build the cabin, hearth and all, as before. Walk away mid-pound and come back, and save/load mid-pound: progress kept. After the first blow E no longer takes materials back and R with nothing to store no longer tears the site down. Taking down an EMPTY cabin site (E, or R with nothing to store) needs the Hammer ("equip the Hammer"); taking materials back before pounding (E) stays free.
57. [ ] Unchanged (Mike's list did not include them): Tent, Lean-To and Tarp Shelter placement and pack-up, the Campfire, and setting traps need no Hammer. Craft-tab items are unchanged.

## F. Saves and regressions
38. [ ] Save with a Well Site (half dug), a Well (some litres, contaminated), a half-delivered Outhouse Site and a finished Outhouse (pit part full; finish one by pounding it, or load an older save that already holds one), load: all restored, same positions, levels and dig progress.
39. [ ] Load an OLD save (before this phase): loads normally; no wells; illness and sleep unchanged (no outhouse = multiplier 1.0).
40. [ ] Build tab: all other items and their ghosts unchanged apart from the Hammer rule in E2; Rain Barrel eave snapping still works. Saves and structures built before this change load and behave the same (the rule affects actions, not saved state).
41. [ ] Shovel still digs stumps and Clay; Pick Axe, Axe unchanged.
42. [ ] Creek crossing, fishing, traps, Pond Drying and flood warnings unaffected.
43. [ ] Esc/right-click cancels the Build-tab placement as before.

## G. Bow Drill (real time, built 2026-10-19; added by Claude, not yet in Claude Code's list)
58. [ ] Bow Drill attempt: hold click, the bar fills over 6 seconds of real time, 15 stamina is spent when it fills. The game clock does NOT jump (watch it): no 10-game-minute skip. Success and failure messages are as before; the 45% calm-weather chance is unchanged; a quick run of failed attempts keeps the growing-ember bonus as before; fuel, wood and survival tick normally around an attempt. Sleep, rest and collapse still skip time as before.
