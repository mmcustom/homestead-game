# Creek bank look (Wet Clay and Creek Sand): playtest checklist (2026-11-04)

Code baseline 33ede69. Not committed yet. Maximize the Game tab with Shift+Space.

WARNING before you start: the season keys move the calendar. Every day skipped fires the midnight events, so land payments can fall due and go late (three missed payments lose the game), and stopping Play Mode autosaves. If you run this on slot 4 its calendar stays moved. SavesBackup41 is the way back (only restore it if you want to; tell me and I will word the steps). You can also run the season steps on a spare slot or a new game and keep slot 4 as it is.

What to look at: the creek, the pond and the spring now have a dark red-brown band at the water's edge (Wet Clay) and a pale tan band beyond it (Creek Sand), then grey Gravel further out. The bands are made when the game loads, not saved in the terrain.

Keys (editor and dev builds):
- F6 jumps to day 1 of the next season (Spring, Summer, Fall, Winter...). F8 jumps to day 1 of Winter (from Winter it goes to next year's Winter 1).
- F5 skips 1 day, Shift+F5 skips 10 days. Ctrl+F5 only regrows trees.
- Numpad 2 skips 1 hour, Shift+Numpad 2 skips 6 hours. F7 cycles the speed x1, x10, x60, x360.
- Ctrl+Home (new) sets the ground snow: 0, then 0.5, then 1, then back to 0. It holds for 24 game hours (30 real minutes at x1) and shows a yellow message.
- Esc shows the date, time, weather and temperature. A yellow message at the top left also shows the date after each skip.
- The semicolon key gives the Potter's Shed kit, which has a Shovel.

Reply with the numbers that pass and describe anything that does not.

## A. Start up

1. Load your save and open the Console. You see one line like "[CreekBanks] built in N ms, 2868 water hits, about 15200 bank cells". N is about 70 to 100 ms. Tell me if N is over 150 ms.
2. Press the semicolon key and equip the Shovel from your tool slots.

## B. Summer look

3. Get to Summer in daylight: press F6 until the message says "Summer 1". If the clock is not between about 10:00 and 17:00, press Numpad 2 or Shift+Numpad 2 until it is.
4. At the creek you see a dark red-brown band at the water's edge and a pale tan band beyond it, with grey Gravel further out. The two bands are clearly different from each other and from the Gravel, readable from walking distance.
5. The same at the pond and at the spring.
6. Aim the Shovel at the red-brown band: "Wet clay bank - hold click to dig Clay". At the tan band: "Sandy bank - hold click to dig Sand". At shallow water: "Creek bed".
7. Where the bands end, the Shovel prompt and the colour roughly agree (edges can differ by up to about a metre; tell me if it feels badly wrong).
8. Dig some Clay and some Sand. Both still work and the pack gets Clay and Sand as before.
9. Walk across both bands: the footsteps sound like gravel.
10. The minimap and the world map show brown along the water edge and tan beyond it, with no white.

## C. Spring, Fall, Winter

11. Spring: press F6 to Spring 1, then Shift+F5 once and F5 four times to reach "Spring 15". If snow is left, press Ctrl+Home until the message shows 0.0. The bands are fully visible.
12. Late Fall: press F6 until "Fall 1", then Shift+F5 twice and F5 seven times to reach "Fall 28". Walk the creek under trees. The bands under tree crowns keep only about a quarter of the leaf litter (still visible). Open banks look as they did in Summer.
13. Winter: press F8 (skip if the date already says Winter), then press Ctrl+Home twice. The yellow message reads like "Ground snow set to 1.0 (held 24 game hours)...". The land goes white over about half a second.
14. In full snow the bands are still visible as a darker red-brown and tan ribbon (about a quarter of the snow on them). Tell me how it looks: too hidden, about right, or too stark.
15. Press Ctrl+Home again to cycle to 0 and compare with winter. At 0.5 the land is half white. The bands are still readable.
16. Digging in snow still works and the prompts are the same.

## D. Tuning (optional)

17. Select the Terrain in the Hierarchy during Play and find the CreekBanks component. Change the Wet Clay or Creek Sand colour. Tell me whether it updates live. If it does not, tell me the colours and tile sizes you want and I will have them set as the defaults.
18. On the SeasonalGround component, try bankSnowShare and bankLitterShare (default 0.25 each). Tell me the values you like.
19. Right-click the CreekBanks component header and choose "Log bank summary". The Console shows a line with water squares and bank cells.

## E. Regression

20. The creek, pond and spring edges look right overall: no gaps between the clay and the water, no stripes, no odd blocky edges.
21. The creek, trees and snow still look as before apart from the bands. Snow and leaf litter appear and melt the same as before.
22. Console shows no new errors or warnings (the PlayerController.cs:217 NullReferenceException on stopping Play Mode is the old known one).
