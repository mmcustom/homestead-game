# Metal chain M2 (Bloomery and Iron Bar): playtest checklist (2026-11-04)

Code baseline e4e806f. Not committed yet. Play from your slot 4 save. Maximize the Game tab with Shift+Space.

What M2 is: the Bloomery is an open-air stone-and-clay furnace. You load Iron Ore and Charcoal separately and at any time (R, each up to 4), start a smelt with E, walk away for 10 game hours, and collect Iron Bars with E. 2 Ore + 2 Charcoal make 1 Bar, up to 2 Bars per smelt. Leftovers stay as stock; anything added during a smelt waits for the next one. Iron Bars are not sold anywhere yet (the Smithy that uses them comes in M3).

Dev keys (editor and dev builds; checked unused):
- Period gives a Hammer and 6 Clay, and stands a Rock Pile with 8 Stone 3 m ahead. The whole kit is 62 kg against the 45 kg pack: store the Clay at the site first, then carry the Stone (40 kg) in one trip.
- Shift+period stands a finished Bloomery 4 m ahead with 4 Ore and 4 Charcoal in it, ready to start.
- Ctrl+period finishes every running smelt now.
- Shift+equals gives 4 Iron Bars (8 kg). Equals still gives 10 Iron Ore and 10 Charcoal.
- PageUp gives $1,000. Numpad 2 skips 1 game hour, Shift+Numpad 2 skips 6.

Reply with the numbers that pass and describe anything that does not.

## A. Building the Bloomery

1. Press period: you get a Hammer and 6 Clay, and a Rock Pile with 8 Stone appears ahead.
2. Open the Build tab, go to Other, and place a Bloomery Site on open ground. The Build-tab help reads clearly. "Smelting Iron in the Bloomery" unlocks in the Journal.
3. Try placing it under the cabin roof (refused: "A bloomery needs open sky: not under a roof.") and near a tree or a building (refused, 3 m clear needed). Turn it with the mouse wheel while placing.
4. Aim at the site and press R to store the Clay. Take the Stone from the pile with E, return and press R again. Partial deliveries work and the site shows what is still needed (8 Stone and 6 Clay). E takes materials back.
5. Equip the Hammer and hold click for 36 blows: the Bloomery is built.
6. The Bloomery looks like a clay-and-stone stack about 1.2 m tall with a short chimney and a mouth at the front. Tell me what you think of the look (size, colour).

## B. Loading and starting

7. Put the Hammer away and press equals for 10 Ore and 10 Charcoal. Sell the Charcoal at the Trading Post so you carry Ore only, then press R at the furnace: it loads 4 Ore and says there is no Charcoal in your pack. The E line says it needs 2 more Charcoal.
8. Get Charcoal again and drop or sell the Ore so you carry Charcoal only: R loads 4 Charcoal. Now it holds both and the E line reads "Start the smelt  (4 Ore + 4 Charcoal -> 2 Iron Bars in 10 h)".
9. With nothing in your pack that can be loaded, the R line reads "Load Ore and Charcoal  (you're not carrying any)". With the Ore full, loading more says "(the Ore is full)". With 4 and 4 inside, R says it already holds 4 Ore and 4 Charcoal.
10. Try different amounts (use the Inventory I and the Drop amount to keep exactly what you want; press equals when you need more): 2 Ore and 2 Charcoal gives "1 Iron Bar"; 3 Ore and 4 Charcoal gives 1 Bar and leaves "1 Ore" as a leftover; 4 and 4 gives 2 Bars.

## C. Smelting

11. Press E to start a smelt. The stack darkens, smoke rises from the chimney, and a dim ember glows at the mouth. The glow does not flash or change shape and the front of the furnace is steady from every side.
12. While it smelts: E shows the hours left and what is waiting; Hammer plus R is refused ("Can't take down the bloomery while it smelts (N h to go)").
13. Top up while it smelts: press R with more Ore or Charcoal. It loads up to 4 of each, the smelt timer does not change, and the prompt shows what is waiting for the next smelt.
14. Press Numpad 2 or Shift+Numpad 2 and sleep once. After 10 hours the smoke stops and the Bars lie at the front (and rust-coloured Ore and black Charcoal lie beside it when stocked).
15. Ctrl+period finishes a running smelt at once. Rain does not stop a smelt.
16. Press E: "Took 2 Iron Bars." (as many as your pack holds; the rest stays). With stock still inside, E then starts the next smelt.

## D. Iron Bars, dismantling, saving

17. Iron Bar help line (Inventory I) says it is not sold anywhere. The Trading Post sell list has no Iron Bar row. Holding a Bar unlocks the Know-how entry. Shift+equals gives 4 Bars if you need more.
18. Dismantle: while it smelts the Hammer plus R is refused. After the smelt it returns 4 Stone, 3 Clay (half) and all stocked Ore, Charcoal and Bars.
19. Save mid-smelt, quit to menu and load: the hours left, the stock and the pending Bars are the same, and the smelt finishes when time passes. A finished Bloomery with uncollected Bars also survives save and load.
20. Press Shift+period for a ready Bloomery and check it loads and starts normally (the quick route).
21. A tree does not sprout within 1.2 m of the Bloomery.

## E. Regression

22. The Charcoal Pit still works (build or use one, light it, collect Charcoal).
23. The Kiln and the Leach Hopper still work (a quick check of one is enough).
24. Console shows no new errors or warnings (the PlayerController.cs:217 NullReferenceException on stopping Play Mode is the old known one).
