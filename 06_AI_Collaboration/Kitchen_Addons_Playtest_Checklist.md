# Covered Kitchen Add-ons, Batch 1: Kitchen-Countertop, Kitchen-Shelves, Kitchen-Grill, Kitchen-Sink - Play Mode Checklist

Built 2026-10-19. Compile-checked; 146 edit-mode tests pass (126 old + 20 new in `KitchenAddonsTests.cs`). PLAYTESTED 2026-10-20: all 77 items pass (Mike; item 10 typed "past", read as pass). UNCOMMITTED. Nothing in this build advances the game clock (only Sleep, rest, collapse and the debug time-skip keys do). All numbers are public fields on `WellsAndOuthouses` (first-pass tuning, Claude, not confirmed).

**Setup:** back up `slot1` first (restore and hash-verify after). You need a FINISHED Covered Outdoor Kitchen (kind 24), so either build one (see `Covered_Kitchen_Playtest_Checklist.md`) or load a save that has one. Carry: Hammer, Flint and Steel, Bow Drill, Bucket (and a Canteen), Cooking Pot, raw meat or fish to cook, some Firewood. For the four builds: 2 Logs, 16 Branches, 6 Sticks, 4 Stone, 3 Clay (each add-on once; more for repeats). A second kitchen site (placed only, never finished) and a spare Wood Pile are useful for the refusal items. Weather keys: Numpad 1 forces the next weather for 12 h (Clear, Light Rain, Heavy Rain, Thunderstorm); Numpad 2 skips 1 game hour; F4 starts a Heat Wave (set Summer first); F3 shows the body readout including sun watts; F9 refills the stats.
**Debug keys:** Numpad `/` prints, for the nearest FINISHED kitchen within 12 m: what stands in each of its four slots, the grill's lit state and fuel, the sink's litres, the shelving's stack and item counts, the 0.75 m check that the hearth lookup cannot pick a grill's fire. It shows on screen for 8 s and in the Console as `[KitchenAddons] ...`. Numpad `*` (the kitchen report) and the others are unchanged.
**Build tab:** all four are in the "Storage and Piles" category, after "Covered Kitchen Site", in this order: Kitchen-Countertop, Kitchen-Shelves, Kitchen-Grill, Kitchen-Sink.
**Weights (kg):** Logs 8, Branches 1, Sticks 0.1, Stone 5, Clay 3.5. Costs, all single-step builds under the 15 kg staged line, all need the Hammer to build and to dismantle:
- Kitchen-Countertop: 1 Log + 4 Branches = 12 kg (`countertopLogs`, `countertopBranches`)
- Kitchen-Shelves: 10 Branches + 6 Sticks = 10.6 kg (`shelvingBranches`, `shelvingSticks`)
- Kitchen-Grill: 2 Stone + 1 Clay = 13.5 kg (`grillStone`, `grillClay`)
- Kitchen-Sink: 1 Log + 1 Clay + 2 Branches = 13.5 kg (`slopSinkLogs`, `slopSinkClay`, `slopSinkBranches`)
- Dismantle refund (half of each line, rounded down, dry): Kitchen-Countertop 0 Logs + 2 Branches; Kitchen-Shelves 5 Branches + 3 Sticks; Kitchen-Grill 1 Stone + 0 Clay (plus the fire's leftover Firewood); Kitchen-Sink 0 Logs + 0 Clay + 1 Branch.
**Numbers:** the Kitchen-Sink holds 20 L of any quality (`slopSinkLitres`). Four fixed slots, generic (any add-on in any slot). Slots are named as you see them standing at the open front (+Z) looking in toward the hearth and the back (kitchen local +X is then your LEFT); the names follow the kitchen if it is turned: slot 1 "left, near the entrance" (local 1.15, 0.78), slot 2 "left, toward the back" beside the prep table (1.15, -0.20), slot 3 "right, near the entrance" (-1.37, 0.80), slot 4 "right, toward the back" (-1.37, -0.12). Each slot is 0.88 m along the wall and 0.5 m deep, the item faces the middle of the kitchen. Every slot is at least 1.06 m from the hearth fire (the rule is more than 0.75 m). A Kitchen-Grill's fire stands 0.45 m up on the firebox and is drawn at 0.6 of a normal campfire so it fits. All add-on colliders are 1.0 m high or less (shelving 1.4 m); the roof is 2.45 m up.

## A. Build tab and the "no kitchen" refusals
1. [ ] Build tab lists Kitchen-Countertop, Kitchen-Shelves, Kitchen-Grill and Kitchen-Sink in "Storage and Piles" after the Covered Kitchen Site, with the costs above and a help text for each. The button reads "Build" (not "Place Site"); each says to equip the Hammer.
2. [ ] On a save with NO finished kitchen: each of the four is greyed with "Needs a finished Covered Outdoor Kitchen." (checked before the materials). Having only a stage-1 site, or a hearth-built roof site, is not enough: same refusal.
3. [ ] With a finished kitchen and the materials, Build starts Placement Mode (the Build screen closes). No materials are spent yet.
4. [ ] Placement Mode with no kitchen near the aim point but a finished kitchen elsewhere: the head line reads "Aim at a slot inside the Covered Outdoor Kitchen." and no ghost is drawn.
5. [ ] Esc or right click cancels: "Placement cancelled - nothing was spent." Pack unchanged.

## B. Slot snapping
6. [ ] Aiming near a finished kitchen (within about 3 m of a slot), the Kitchen-Countertop ghost jumps to the nearest FREE slot, however you aim (ground in front, a post, the roof, the table). It faces the middle of the kitchen and sits on the ground. The head line names the slot: "Place Kitchen-Countertop here - left, near the entrance". Stand at the open front facing in: that slot is on your LEFT, by the entrance. The other three read "left, toward the back", "right, near the entrance", "right, toward the back" and match what you see.
7. [ ] The ghost is green with the Hammer equipped; without it the ghost shows but is red and reads "Equip the Hammer to build." A click builds nothing.
8. [ ] The mouse wheel does NOT turn an add-on ghost (the slot sets its facing). The hint line shows only "Left click: build    Esc / Right click: cancel".
9. [ ] Moving the aim around the kitchen moves the ghost between the four slots; with the aim at your left wall (as you face in from the front) it picks a "left" slot, at your right wall a "right" slot.
10. [ ] Build a Kitchen-Countertop in slot 1 (left, near the entrance). Start another add-on: the ghost no longer chooses slot 1 while any slot is free. Fill all four slots (any mix, e.g. a second Kitchen-Countertop, Kitchen-Shelves, Kitchen-Grill, Kitchen-Sink): once all four are taken, the ghost still appears at the nearest slot and is red: "That spot is taken."
11. [ ] Free slots are chosen before a taken one, even when the taken one is nearer the aim.
12. [ ] The ghost's colour and reason match what the click does. No Hammer: "Equip the Hammer to build." Too far (aim more than 6 m away): "Too far - come within 6 m." Materials short: the Build tab was greyed with what is missing.
13. [ ] Two kitchens (build a second one far from the first): the ghost snaps to the nearer kitchen's slots and builds there.
14. [ ] A turned kitchen (built facing another way): the slots turn with it. The add-on stands at the same slot and faces the middle at any facing.
15. [ ] Add-ons build only in a slot: standing in the open with no kitchen near, there is no way to put one on open ground.
16. [ ] An unrelated pile (a Wood Pile) within 2 m of a slot (put one just outside the roof edge first, if you can) still refuses an add-on there: "Too close to another pile." A lit ordinary campfire within 2 m of a slot: "Too close to the fire." The kitchen's own hearth, its prep table and the other add-ons never trigger either refusal.

## C. Kitchen-Countertop
17. [ ] Build it (Hammer, 1 Log + 4 Branches spent): "Kitchen-Countertop built - ...". It stands in the slot: a work surface about 0.9 m high on four legs, a lower board, a back lip, a chopping board on top. The existing prep table at the back right is still there, unchanged.
18. [ ] Looking at it: "Kitchen-Countertop / A work surface under the kitchen roof - nothing to do at it yet". E flashes the same. R (Hammer equipped) offers "Dismantle the Kitchen-Countertop (half back, rounded down ...)"; without the Hammer it says it needs the Hammer.
19. [ ] It has its own collider: you cannot walk through it; you can stand beside it; you cannot jump onto the roof from it. The hearth and the other slots are still reachable.
20. [ ] R with the Hammer: dismantled, refund 2 Branches (the single Log refunds nothing), dry. The slot is free again (the ghost picks it).

## D. Kitchen-Shelves
21. [ ] Build it: an open shelf unit 1.4 m high with four boards against the wall of its slot. Standing under the roof next to it, it is clearly below the roof.
22. [ ] Looking at it: "Open Kitchen-Shelves  (empty)". E or R opens the same two-panel transfer screen the Storage Bin uses, titled "Kitchen-Shelves".
23. [ ] Accepts dry goods AND food: Cordage, hides, furs, feathers, arrows, rounds, Lumber, Clay, Tall Grass, Lamp Oil, and any food that is not water (raw and cooked meat, fish, berries, nuts, eggs, milk). Refuses Tools, wood (Logs, Branches, Sticks, Firewood), Stone and all water ("the Kitchen-Shelves doesn't hold that" in grey).
24. [ ] Store several foods and goods: each stack shows as a small box on the shelves (tan for food, grey-brown for dry goods), up to 16 boxes.
25. [ ] Day stamps: store cooked food, wait a day (or load a save a day later) and take it back: it keeps its day (the Inventory shows the same age). Take it out and store it again: still its original day. Stack batches of different days stay apart.
26. [ ] Take one, Shift-click takes the stack, "Store All Eligible" and "Take Everything" work as on the Bin.
27. [ ] "Remove" in the screen: refused while anything is on the shelves ("empty it first"); without the Hammer ("equip the Hammer"); with the Hammer and empty: dismantled, 5 Branches + 3 Sticks back, dry.

## E. Kitchen-Grill
28. [ ] Build it (2 Stone + 1 Clay): a clay firebox 0.45 m high with flat stone slabs either side of the middle, small stones at the base, a rack of green sticks over the middle, and a small campfire (a ring of stones, no fire yet) on top. It stays unlit.
29. [ ] Looking at the firebox: "Kitchen-Grill (out) / Light and feed the fire on top; cook meat and fish at it from the Inventory". Looking at the fire on top: the normal campfire prompt, prefixed "Kitchen-Grill:" (for example "Kitchen-Grill:  Add dry Firewood (+2 h)").
30. [ ] Add Firewood to the grill's fire (E) and light it with Flint and Steel. It lights, burns, shows flames and a light, and goes out when the fuel is gone, exactly like a campfire. The 12 h cap holds.
31. [ ] Wet and dry wood: dry Firewood gives 2 h each, wet Firewood 1.4 h each (the same driest-first rule); the grill's fuel readout in the firebox prompt follows.
32. [ ] Bow Drill at the grill: with no Flint and Steel, equip the Bow Drill and hold click on the grill's fire. In Heavy Rain (Numpad 1 twice) under the roof it catches at the calm rate (about 45 percent a try); a normal campfire in the open in the same rain still has the poor rate. Numpad `*` still says the HEARTH is "Bow Drill covered yes".
33. [ ] Cooking: with the grill lit, stand within 3 m and open the Inventory: raw meat and fish show Cook; they cook as at any campfire; boiling water works with the Cooking Pot. With the HEARTH lit and the grill out, cooking still works (either fire is enough). With both out, Cook is not offered.
34. [ ] The grill's fire gives warmth by distance like any fire; its crackle plays from the grill. It is drawn smaller than the hearth.
35. [ ] The 0.75 m rule: with both the hearth and a grill built, Numpad `/` says "hearth lookup ok" and "Kitchen-Grill fire x m from the hearth fire (ok)". Try the grill in every one of the four slots (dismantle and rebuild it): the distance is 1.06 m or more each time. The hearth's own prompt, Numpad `*` and the sheltered-meal check are unchanged.
36. [ ] Light the hearth and the grill together, eat under the roof: still a sheltered meal; both fuels burn down independently.
37. [ ] Dismantle with the grill LIT: R with the Hammer says "Can't take the Kitchen-Grill down - put the grill's fire out first." Nothing changes. Without the Hammer it says to equip the Hammer.
38. [ ] Put the fire out, add some Firewood to it, then R with the Hammer: dismantled, 1 Stone back (the single Clay refunds nothing), plus Firewood equal to the fuel hours left divided by 2, rounded down. The firebox, the rack AND the fire all disappear.

## F. Kitchen-Sink
39. [ ] Build it (1 Log + 1 Clay + 2 Branches): a work-height table with a clay basin in the top, a rim of boards, a spout at the front. Empty it shows the dry clay floor.
40. [ ] Looking at it empty: "Kitchen-Sink (empty, holds 20 L)". R with nothing carried and the Hammer: "Dismantle the Kitchen-Sink ...".
41. [ ] Fill your Bucket at the creek or a barrel, look at the sink: R reads "Pour In Water (10 L)" (the smaller of what you carry and the room). R pours it in: pour sound, "Poured in 10 L - the Kitchen-Sink holds 10 L". Water shows in the basin; the level rises with the litres.
42. [ ] Mixed qualities: pour Purified, Good and Unsafe water in. All go in (the 20 L cap is shared); the cap holds: with the sink full R no longer pours and instead offers "Drain the Kitchen-Sink".
43. [ ] E on a sink with water and a Bucket or Canteen carried with room: "Draw water" fills them with the best quality first (purified, then excellent, good, questionable, unsafe): "Filled 10 L from the Kitchen-Sink (10 L left)". Full containers: "Your water containers are full." No container: "Needs a Bucket or Canteen to carry water." Empty sink: "The Kitchen-Sink is empty."
44. [ ] Drain: with water in the sink and nothing to pour (carry no water, or the sink is full), R drains it onto the ground: "Drained the Kitchen-Sink." and the sound. The basin is empty. It takes no game time. The same R press does not also dismantle it; a second deliberate press about two seconds later, with the Hammer, does.
45. [ ] Water does not collect by itself: leave the sink empty through Heavy Rain and a Thunderstorm (Numpad 1 twice, then Numpad 2 a few times): it stays at 0 L. A full sink does not overflow or evaporate in an hour.
46. [ ] Washing does nothing: there is no wash action, no hygiene change, no Health, Hunger or sickness effect from the sink. (The only things you can do are pour, draw, drain, dismantle.)
47. [ ] Dismantle with water in it: "Can't take the Kitchen-Sink down - drain the Kitchen-Sink first." Empty, Hammer: dismantled, 1 Branch back (the single Log and Clay refund nothing), dry.

## G. Dismantle rules and the kitchen
48. [ ] With any add-on standing in a slot, R on the kitchen with the Hammer (hearth out): "Can't dismantle the kitchen - remove the add-ons first." Nothing changes. Try with just one add-on, with a Kitchen-Grill only, with a Kitchen-Shelves only, with a Kitchen-Sink only.
49. [ ] The kitchen prompt without the Hammer is unchanged; nothing else changes when the kitchen has no add-ons: with all four removed it dismantles exactly as in phase C (refund 2 Stone, 1 Clay, 1 Log, 3 Branches, 2 Tall Grass; leftover Firewood).
50. [ ] An add-on's own Dismantle is never blocked by the kitchen: a Kitchen-Countertop comes down while the hearth is lit and the roof stands.
51. [ ] Each add-on's refund goes to the pack; a full pack leaves the rest as dry pickup piles on the spot.
52. [ ] A hearth-built roof site, or a stage-1 site, is not affected (it has no slots).

## H. Numpad /
53. [ ] Standing near a finished kitchen with nothing built: "Kitchen n: slot 1 left, near the entrance: empty; slot 2 left, toward the back: empty; slot 3 right, near the entrance: empty; slot 4 right, toward the back: empty | hearth lookup ok; no Kitchen-Grill".
54. [ ] With all four built: "slot 1 left, near the entrance: Kitchen-Countertop; slot 2 ...: Kitchen-Grill (lit, 5.3 h fuel); ...: Kitchen-Sink (12/20 L); ...: Kitchen-Shelves (3 stacks, 14 items) | hearth lookup ok; Kitchen-Grill fire 1.xx m from the hearth fire (ok)".
55. [ ] More than 12 m from any finished kitchen: "no finished Covered Outdoor Kitchen within 12 m". The key does nothing else (no time change, no item). A stage-1 or stage-2 site does not count.
56. [ ] The Console line `[KitchenAddons] ...` appears with the same text.

## I. Saves
57. [ ] Build all four in different slots; put 20 L in the sink (mixed quality), food and goods on the shelving (two batches of different days), Firewood in the grill, light it. Save, quit to the main menu, continue. All four stand in their slots, the sink has the same litres (the same qualities when drawn), the shelving has the same stacks and day stamps, the grill's fire is lit with the same fuel, the countertop is there.
58. [ ] The grill's fire is drawn small after the load (not as a full campfire), the prompt still says "Kitchen-Grill:", and Numpad `/` reports the same.
59. [ ] Load a save made before this build (no add-ons anywhere, a kitchen or none): loads unchanged, no errors, the kitchen exactly as before.
60. [ ] After a load, building and dismantling add-ons works the same as before the save (the slot is only the add-on's position; no link was saved).
61. [ ] The sheltered-meal bonus is still not saved (as before); nothing about the add-ons depends on it.

## J. Walk-in, open front, shelter and shade
62. [ ] With all four slots full: you can still walk in from the open front (the doorway between the side items is about 2 m) and straight up to the hearth; you can stand in front of the hearth and reach it. You are not trapped and the posts, table and add-ons are all walk-round.
63. [ ] The hearth interaction is never blocked: looking at the hearth fire from the front, the side and from between the add-ons always gives the hearth's prompt (not an add-on's). The Hammer ray on a kitchen SITE is unaffected (add-ons exist only on finished kitchens).
64. [ ] Rain and Snow shelter under the roof is unchanged with all four built: Heavy Rain, Thunderstorm and Snow all leave you dry under the roof and wet outside. Numpad `*` still reads "rain-sheltered yes".
65. [ ] Heat Wave shade is unchanged: sun watts drop under the roof exactly as in phase C, with and without add-ons; a tall shelf does not change it. Numpad `*` still reads "shade yes (high sun)".
66. [ ] The Bow Drill cover test at the hearth is unchanged with the add-ons built: "Bow Drill covered yes". Nothing built is above head height (the shelving tops out at 1.4 m).
67. [ ] The sheltered meal still works with all four built, and the message is unchanged.

## K. Regression
68. [ ] The kitchen's Numpad `*` report is unchanged and still correct with and without add-ons.
69. [ ] A normal Campfire: build, light, add fuel, cook, boil, put out: unchanged, and its prompt does NOT say "Kitchen-Grill". The Small Cabin hearth likewise.
70. [ ] Placing a normal campfire near the kitchen follows the usual campfire rules; building a Wood Pile or Storage Bin near it follows the usual 2 m pile spacing (a pile can be placed next to the kitchen's roof edge but not on top of an add-on's slot).
71. [ ] The Wood Shed, Well, Outhouse, barrels (Water, Rain), Food Cache, Storage Bin, Tool Rack, Rock Pile and Wood Pile: place, use, dismantle unchanged. Their placement distances are unchanged: add-ons never count as neighbours.
72. [ ] Placing a SECOND covered kitchen next to a kitchen with add-ons follows the phase C rules (its roof edge 3 m from the first kitchen); the add-ons do not make the first one "bigger".
73. [ ] The Hammer, the Shovel and the Axe do nothing odd on an add-on (the Hammer only pounds staged sites; the Shovel does not dig an add-on; the Axe does not chop it).
74. [ ] Old kitchens (built before this build) get slots with no migration: add-ons can be built in a kitchen made by the earlier phase C build.
75. [ ] No time jumps: building, drawing, pouring, draining, lighting, cooking and dismantling never advance the game clock.
76. [ ] Sleep, rest, collapse and the time keys still work.
77. [ ] The Console is free of errors and warnings through all of the above.
