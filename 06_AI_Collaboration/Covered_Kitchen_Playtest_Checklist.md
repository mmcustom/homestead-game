# Structures Slice 1, Phase C: Covered Outdoor Kitchen - Play Mode Checklist

Built 2026-10-19. Compile-checked; 126 edit-mode tests pass (13 new in `KitchenTests.cs`). PLAYTESTED 2026-10-19: all 75 items pass (Mike). UNCOMMITTED. Nothing in this build advances the game clock (only Sleep, rest, collapse and the debug time-skip keys do). All numbers are public fields on `WellsAndOuthouses` (first-pass tuning, Claude, not confirmed).

**Setup:** back up `slot1` first (restore and hash-verify after). Carry or have on hand: Hammer, Axe, Flint and Steel, Bow Drill, Cooking Pot, 8 Stone, 4 Clay (Shovel from clay banks), 4 Logs, 12 Branches, 8 Tall Grass, 10 Firewood, some food to eat. Build tab: "Covered Kitchen Site" (Storage category, last in the list). A finished Small Cabin, a Wood Shed, a Well and a few trees are useful for the refusals and the regression items. Weather keys: Numpad 1 forces the next weather for 12 h (Clear, Light Rain, Heavy Rain, Thunderstorm); Numpad 2 skips 1 game hour (Shift: 6). F4 starts a Heat Wave (set the season to Summer first); F3 shows the body readout including sun watts; F9 refills the stats.
**Debug keys:** Numpad * prints, for the nearest Covered Kitchen (any stage, within 12 m): roof present, shade (high-sun probe), rain-sheltered, hearth lit or out, whether the Bow Drill counts the hearth as covered. It shows on screen for 8 s and in the Console as `[Kitchen] ...`. Numpad +, -, 0 and . and the rest are unchanged.
**Weights (kg):** Stone 5, Clay 3.5, Logs 8, Branches 1, Tall Grass 0.1. Hearth stage = 4 Stone + 2 Clay = 27 kg. Roof stage = 2 Logs + 6 Branches + 4 Tall Grass = 22.4 kg. Both are over the 15 kg staged-site cutoff, so the kitchen is a staged site; each stage is under the 30 kg Encumbered line, but the Stone goes first and the Clay in a second lot to test trips.
**Numbers:** Hammer blows: hearth 15 (`kitchenHearthBuildBlows`, about 14 s), roof 30 (`kitchenRoofBuildBlows`, about 27 s). Clear gap from the roof edge to any other structure 3 m, to any tree trunk 3 m (`setbackKitchenFromStructureMetres`, `setbackKitchenFromTreeMetres`); the roof's reach from its centre is 2.3 m, so centre to centre that is about 5.3 m from a trunk or a small pile and about 7.8 m from the Small Cabin (it counts 2.5 m for the cabin's own reach). Roof 2.45 m up at its middle; footprint 3.4 m wide by 3.0 m deep, open on the +Z side (the front, where the ghost's wheel turns it). Sheltered meal: Health recovers 25 percent faster for 4 game hours (`shelteredMealRecoveryBonus` 0.25, `shelteredMealHours` 4). There is no Morale stat in the game yet, so this is the whole comfort bonus.
**What the roof does for the fire:** a lit fire is not slowed or put out by rain anywhere (that was never modelled). The roof's real benefit at the hearth is that the Bow Drill's rain/snow penalty does not apply under it (it catches like calm weather: 45 percent a try instead of 20 or 10). Flint and Steel has no weather chance at all.

## A. Ghost and refusals
1. [ ] Build tab lists "Covered Kitchen Site": "free to mark out; the Covered Outdoor Kitchen then takes 4 Stone, 2 Clay, 2 Logs, 6 Branches, 4 Tall Grass over several trips". The detail panel lists Stage 1 (hearth, Stone first then Clay) and Stage 2 (roof) separately. Button reads "Place Site".
2. [ ] Placement Mode shows the FINISHED kitchen as the ghost: four posts, a plank roof, a stone slab with a clay backing, a small prep table at the back. Green where valid, red where not. It does not show the stakes.
3. [ ] The mouse wheel turns the ghost. The roof stays level and the prep table stays at the back.
4. [ ] Within about 7.8 m (centre to centre) of the finished Small Cabin: red "Too close to the Small Cabin: 3 m needed." Just outside it: green.
5. [ ] Near another structure (Wood Shed, Well, Outhouse, a barrel, a Tent): red "Too close to the Wood Shed: 3 m needed." (the named structure). Roughly 5 to 6 m of centre distance is clear for the small ones.
6. [ ] Near a standing tree (within about 5.3 m of its trunk): red "Too close to a tree: 3 m needed." A felled tree's stump does not count; a standing one does. A clearing: green.
7. [ ] On water: red "Can't build that in water." Too steep: red "The ground there is too steep."
8. [ ] Low ground beside the creek (the floodplain): red "This ground floods."
9. [ ] Near a lit campfire: red "Too close to the fire."
10. [ ] A valid spot: one click places the site with NO Hammer in hand and spends nothing. The pack is unchanged.

## B. Stage 1: the hearth (two lots, Stone then Clay)
11. [ ] The placed site shows the plot look (four stakes round a 3 m by 2.6 m square of bare earth), not the kitchen. Prompt: "Covered Kitchen Site / Hearth stage: Needs 0 of 4 Stone, 0 of 2 Clay (R stores what you carry) / Equip the Hammer to take the site down".
12. [ ] Carrying Clay first: R refuses ("Nothing you carry fits yet"): the Stone has to go in first. Tall Grass, Logs, Branches are refused at this stage.
13. [ ] Lot one: carry 4 Stone (20 kg), R stores them. The Stone appears at the front edge of the plot; prompt "4 of 4 Stone, 0 of 2 Clay".
14. [ ] Lot two: carry 2 Clay (7 kg), R stores them. A banner says "Covered Kitchen Site fully stocked - equip the Hammer and hold click on it to build it". Nothing is built yet and the prompt reads "Fully stocked - equip the Hammer to build the hearth (0%)".
15. [ ] Carrying more than needed (6 Stone): R takes only 4. Part deliveries are fine (2 Stone now, 2 later).
16. [ ] Save and load with the site half stocked (4 Stone in, no Clay): the Stone is still there and the site still wants the Clay.

## C. Take-back before pounding
17. [ ] With the site stocked and no blow struck yet, E takes the materials back (as far as the pack allows), no Hammer needed: "Take Materials". Restock it.
18. [ ] A full pack: E takes what fits and says "Left behind: ..." for the rest. Nothing is lost.
19. [ ] An EMPTY site with the Hammer equipped: E removes it ("Took down the covered kitchen site"). Without the Hammer E says "Equip the Hammer to take the site down." Nothing is refunded because nothing was spent.

## D. Hammer pounding the hearth (stage 1)
20. [ ] Without the Hammer clicking does nothing. Hammer equipped, hold click on the plot: about 15 blows (one every 0.9 s, roughly 14 s), the progress bar fills, the chop sound plays, stamina drops per blow. Prompt "Hold click with the Hammer to build the hearth (n%)".
21. [ ] After the first blow the prompt says the materials are built in; E refuses ("The materials are built in now - keep hammering to finish it").
22. [ ] Releasing and coming back keeps the percentage (saved on the site).
23. [ ] The game clock does not jump while hammering (watch the clock; it runs at its normal rate).
24. [ ] The last blow: "Hearth built - now deliver the roof: 2 Logs, 6 Branches, 4 Tall Grass." The plot GROWS: the same stakes and earth, now with a stone slab, a clay-and-stone backing wall and an unlit campfire on it. The Stone and Clay are used up.
25. [ ] The new hearth is an ordinary campfire: looking at it reads like any campfire ("Add dry Log (+10 h)" etc.). Add Firewood, light it with Flint and Steel, it burns and goes out like any other.

## E. Stage 2: the roof (delivery and pounding)
26. [ ] The site is now "Covered Kitchen Site (roof)". Prompt: "Hearth built. Roof stage: Needs 0 of 2 Logs, 0 of 6 Branches, 0 of 4 Tall Grass (R stores what you carry)".
27. [ ] Stone and Clay are refused now (the hearth is built). Logs, Branches and Tall Grass go in any order, in as many lots as you like: the Logs and Branches show beside the plot, the grass at the back.
28. [ ] Fully stocked: banner "fully stocked - equip the Hammer ... ", prompt "Fully stocked - equip the Hammer to build the roof (0%)". E takes the roof materials back until the first blow.
29. [ ] Hammer: about 30 blows (roughly 27 s). After the first the materials are built in. Progress is saved.
30. [ ] The last blow: "Covered Outdoor Kitchen built." The stakes and earth are replaced by the finished kitchen at the same spot and facing; the campfire is still there on the slab, with whatever fuel and lit state it had (a lit fire stays lit through the build).
31. [ ] The clock does not jump while hammering the roof.

## F. The finished kitchen
32. [ ] Four posts at the corners, a plank roof (higher at the front, overlapping planks, a beam along each long edge), a prep table at the back right, the hearth slab with its clay backing. The roof is clearly clear of the fire (2.45 m up).
33. [ ] You can walk in under the roof from any side; you bump into the posts and the table but not into thin air. You can stand next to the fire. Aiming at a post, the roof or the table shows "Covered Outdoor Kitchen (hearth ...)"; E flashes the hearth state; R is Dismantle.
34. [ ] The finished kitchen matches the ghost you aimed with (shape and facing).

## G. Hearth: lighting and burning (wet and dry wood)
35. [ ] Light the hearth with Flint and Steel (Firewood in it first): it lights and burns. The cooking and warming range works the same as a campfire.
36. [ ] Wet and dry wood behave exactly as at any campfire: "Add dry Log (+10 h)" vs "Add wet Log (+7 h)"; driest batch first; 1 wet Firewood lasts 1.4 h, 1 dry one 2 h; the 12 h cap holds.
37. [ ] In Heavy Rain (Numpad 1 twice, standing under the roof): a Bow Drill attempt at the hearth catches at the calm-weather rate (about 45 percent per try, with the growing-ember bonus), not the 10 percent rain rate. A normal campfire in the open in the same rain still has the poor rate. Numpad * says "Bow Drill covered yes".
38. [ ] A lit hearth is not put out or slowed by rain or snow (it never is anywhere): leave it lit through a Light Rain hour (Numpad 2) and it only loses its normal hour of fuel.
39. [ ] Cooking: with the Cooking Pot (or the Cooking screen) at the lit hearth, raw food cooks as at any campfire. Boiling water: purify water at the lit hearth as at any campfire.
40. [ ] The hearth's fire counts for warmth by distance just like a campfire (stand beside it on a cold night; F3 body readout shows it).

## H. Rain and snow shelter
41. [ ] In Heavy Rain (Numpad 1) standing OUTSIDE the kitchen, the HUD shows Wet after a while. Standing under the roof for the same time you do not get wetter and dry off (the HUD shows nothing). The rain sound/visuals under the roof are muffled like under the Tent.
42. [ ] The same in a Thunderstorm and in Snow (Winter): under the roof you stay dry; outside you get wet.
43. [ ] Standing at the very edge of the roof, one step beyond the posts: rain wets you again (the footprint is the roof's shadow, 3.4 m by 3.0 m).
44. [ ] The roof is a real collider: you cannot jump through it, and a Bow Drill/fire at the hearth finds it. Numpad * says "roof present" and "rain-sheltered yes".

## I. Shade in a Heat Wave
45. [ ] Summer, F4 (Heat Wave), around midday, F3 body readout open. Standing in the open: sun watts high. Step under the roof: sun watts drop to the shade/roof level (with a roof overhead none of the direct sun reaches you). Step out: they come back.
46. [ ] At dawn or dusk (low sun) the shade follows the sun's line: standing at the front edge with the sun low in front you may still be lit under the roof's front edge; standing deeper in the back you are shaded.
47. [ ] Numpad * reports "shade yes (high sun)".

## J. Sheltered meal
48. [ ] Hurt yourself a little first so Health recovery is visible (F10 for a minute, then eat/drink so Hunger and Hydration are back over Half Fed; stop F10's drain). Eat cooked food standing UNDER the roof: the message reads "Ate <food> - a sheltered meal: you feel settled".
49. [ ] For the next 4 game hours (about 5 real minutes) Health recovers about 25 percent faster than before (3.75 per hour instead of 3). Compare with the same meal outside: message is just "Ate <food>", no bonus.
50. [ ] Eating at the edge, just outside the posts: not a sheltered meal. Drinking (water) under the roof: not a meal, no bonus.
51. [ ] A second meal under the roof within the 4 hours refreshes the timer; it does not stack to 50 percent.
52. [ ] The bonus is lost on save and load (it is only 4 hours long and is not saved) - a known simplification. It does not change Hunger/Hydration, sickness chance, or anything else.

## K. Dismantle
53. [ ] With the hearth LIT: R with the Hammer says "Can't dismantle the kitchen - put the hearth fire out first." Nothing changes. Put the fire out (R on the fire), try again.
54. [ ] Without the Hammer the prompt reads "Dismantle the Covered Outdoor Kitchen (needs the Hammer; half the materials back, rounded down)".
55. [ ] With the Hammer and the hearth out: R takes it down. Refund is half of each line, rounded down: 2 Stone, 1 Clay, 1 Log, 3 Branches, 2 Tall Grass; the wood comes back DRY. Any Firewood left in the hearth comes back too (fuel hours / 2, rounded down). The roof, posts, table, slab and hearth all disappear.
56. [ ] A full pack: the surplus is left as pickup piles where the kitchen stood (nothing lost). The wood in them is dry when picked up.
57. [ ] Dismantle the hearth-only site (stage 2, nothing delivered yet): R with the Hammer takes the hearth down for 2 Stone and 1 Clay; the campfire goes with it. A lit hearth refuses. If roof materials are in the site but the roof is unstarted, it says "take the roof materials back first (E)". Once the roof is begun (built in) it can still be dismantled: the refund is half the hearth plus half the roof.
58. [ ] E on the hearth-only site with nothing in it does NOT remove the site (it would orphan the fire): it says to deliver the roof or dismantle the hearth (R).

## L. Numpad *
59. [ ] Standing under a finished kitchen with a lit hearth, press Numpad *: "Kitchen (Covered Outdoor Kitchen): roof present (2.5 m up), shade yes (high sun), rain-sheltered yes, hearth lit (x h of fuel), Bow Drill covered yes, you are under the roof".
60. [ ] With the hearth out: "hearth out (x h of fuel)". At a stage-1 site: roof absent, shade no, rain-sheltered no, hearth none (the site's stage shows in the name). At a hearth-built site: roof absent, hearth out.
61. [ ] More than 12 m from any kitchen: "Kitchen: none within 12 m". The key does nothing else (no time change, no item).
62. [ ] The Console line `[Kitchen] ...` appears with the same text.

## M. Saves
63. [ ] Save with a half-pounded hearth stage (e.g. 40 percent), a hearth-built site with the roof half delivered, and a finished kitchen with a lit hearth. Load: all three come back as they were (progress, contents, facing, the fire's lit state and fuel).
64. [ ] A finished kitchen: save, quit to the main menu, continue. The roof is still a roof (Numpad * "roof present", rain shelter works), the hearth is the same fire.
65. [ ] Load a save made before this build (no kitchen anywhere): loads unchanged, no errors, no kitchen, everything else as it was.
66. [ ] Mid-hearth-stage save: reload and finish pounding from the saved percentage.

## N. Regression
67. [ ] A normal Campfire: build (3 Firewood or 6 Sticks), light, add fuel, cook, boil water, put out: unchanged. A normal campfire lit by the Bow Drill in rain still has the poor chance (20 percent light rain, 10 percent heavy); one under a Tent/Lean-To/Tarp or tree-free roof still counts as covered.
68. [ ] The Small Cabin and its hearth: build, light, cook, sleep, Dismantle (hearth out) - unchanged. Rain Barrels still fill from the cabin roof.
69. [ ] Wood Shed: place, deliver, pound, store wood, wet/dry labels, dismantle - unchanged. Wet/dry wood at every fire as in phase B.
70. [ ] Well, Outhouse, Water Barrel, Rain Barrel and Food Cache sites: place, deliver, pound, finish, dismantle - unchanged. The Well and Outhouse placement distances are unchanged (the kitchen counts as a structure only for its own placement).
71. [ ] Placing a Wood Shed or any other build near a finished kitchen is still allowed by those structures' own rules (the kitchen only constrains itself, plus the usual pile spacing).
72. [ ] Eating and drinking elsewhere (not under the kitchen roof) is unchanged: the same messages, no bonus.
73. [ ] Lighting a normal campfire in rain is unchanged (Flint and Steel always lights; Bow Drill chance by weather as before).
74. [ ] Sleep, rest, collapse and time keys still work; nothing in the kitchen build, pounding, cooking or eating advanced the game clock.
75. [ ] The Console is free of errors and warnings through all of the above.
