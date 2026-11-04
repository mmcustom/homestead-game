# Metal chain M1 (Iron Ore, Charcoal, Charcoal Pit): playtest checklist (2026-11-04)

Code baseline d096995. Not committed yet. Play from your slot 4 save. Maximize the Game tab with Shift+Space.

What M1 is: Iron Ore ($4) is bought at the Trading Post. The Charcoal Pit is an open-air earth mound you load with Logs (R), light (E), walk away from for 24 game hours, and collect Charcoal from (E). 3 Logs make 2 Charcoal, 4 make 2, 5 make 3, 6 make 4. Charcoal sells for $1. Nothing else of the metal chain exists yet (no Bloomery, Iron Bar or Smithy).

Dev keys (editor and dev builds; checked unused before):
- Comma gives a Hammer, 4 Clay and 6 Tall Grass (the Pit site kit) and stands a Wood Pile with 6 Logs 3 m ahead (take them with E; 6 Logs weigh 48 kg and the pack holds 45 kg, so a full load takes two trips).
- Shift+comma stands a finished Charcoal Pit 4 m ahead with 6 Logs in it, ready to light.
- Ctrl+comma finishes every running burn now.
- Equals gives 10 Iron Ore and 10 Charcoal (40 kg; what does not fit is left out and the message says so).
- PageUp gives $1,000. Numpad 2 skips 1 game hour, Shift+Numpad 2 skips 6.

Reply with the numbers that pass and describe anything that does not.

## A. Iron Ore at the Trading Post

1. Press PageUp for $1,000, go to the Trading Post and buy one Iron Ore ($4). The Know-how entry "Iron Ore at the Trading Post" shows.
2. Sell the Ore back: you get $1. Iron Ore is the 22nd item on the buy list, after the Wood Saw.
3. Open the Inventory and select the Ore row: the help line reads clearly and fits the row.
4. Press equals: Ore and Charcoal appear in the pack. Open the Journal: the Know-how entries "Iron Ore at the Trading Post" and "Charcoal: Burning Logs Slowly" read correctly.

## B. Building the Pit

5. Press comma for the site kit. Open the Build tab, go to Other, and place a Charcoal Pit Site on open ground. The Build-tab help for it reads clearly.
6. Try placing it under the cabin roof: refused ("A charcoal pit needs open sky: not under a roof."). Try near a tree or a building: refused with a clear reason (3 m clear needed).
7. Turn the site with the mouse wheel while placing.
8. Aim at the site and press R carrying only Clay, then only Tall Grass: partial deliveries work and the site shows what is still needed (4 Clay and 6 Tall Grass). E takes materials back.
9. Equip the Hammer and hold click for 20 blows: the Pit is built, a "Charcoal Pit built" message shows, and "Charcoal: Burning Logs Slowly" unlocks.
10. The Pit looks like a clay-coloured earth mound with a small vent on top and a draught hole at the front. Tell me what you think of the look (size, colour).

## C. Loading, lighting and burning

11. Press Shift+comma for a finished Pit with 6 Logs in it. The prompt reads like "Light the pit  (6 Logs -> 4 Charcoal in 24 h)".
12. Take Logs back out or use a fresh Pit and load 3 Logs (R, Hammer put away): "Load 3 Logs". With only 1 or 2 Logs inside, E says it needs more. At 3 or more the prompt reads "Light the pit  (3 Logs -> 2 Charcoal in 24 h)".
13. Light it with E. Watch: grey smoke rises from the vent, a dim ember glow shows at the draught hole, the mound darkens, and the Logs lying beside it disappear. The glow does not flash or change shape; the front of the Pit is steady from every side.
14. While it burns: E shows the hours left, R says it cannot load, and Hammer plus R is refused ("Can't take down the pit while it burns (N h to go)").
15. Press Numpad 2 or Shift+Numpad 2 and sleep once. When the 24 hours run out the smoke stops and a heap of Charcoal appears in front.
16. Ctrl+comma finishes a running burn at once.
17. Collect with E: you get Charcoal, as much as your pack holds, and the rest stays in the Pit.
18. Repeat with 4, 5 and 6 Logs (for 6 load 5 and then 1): you get 2, 3 and 4 Charcoal.
19. Rain does not stop a burn.

## D. Selling, dismantling, saving

20. Sell Charcoal at the Trading Post: $1 each, and the price drops as you sell more.
21. Load 2 Logs, equip the Hammer and press R on a Pit that is not burning: you get 2 Clay and 3 Tall Grass (half) back, plus the 2 Logs.
22. Save mid-burn, quit to menu and load: the Pit shows the same hours left and finishes when time passes. A finished Pit with uncollected Charcoal also survives save and load.
23. A tree does not sprout inside 1.4 m of the Pit.

## E. Regression

24. The Kiln, Potter's Shed, Workshop and Leach Hopper still work (a quick check of one is enough).
25. Console shows no new errors or warnings (the PlayerController.cs:217 NullReferenceException on stopping Play Mode is the old known one).
