# Pottery P2 Playtest Checklist (2026-11-03)

Build: HEAD 719376b plus uncommitted pottery P2 (16 modified paths, 12 new, expected 28 to stage). Suite 1366 of 1366, Console 0 errors, no Play Mode run by Claude Code.
Reply with item numbers and pass or fail. Write down anything in [brackets].

## Setup
Play Mode, Game tab maximized (Shift+Space). Dev keys for this build: Shift+' (Kiln site kit: Hammer, 6 Stone, 6 Clay, 10 Firewood, 3 dry Cups, 2 dry Bowls, about 62 kg, so some may not fit your pack), ' (a finished Kiln 4 m ahead, loaded with 4 Cups, 2 Bowls, a Cooking Pot, a Crock and 10 Firewood, ready to light), Ctrl+; (finish every shaping job, drying and firing), Shift+; (a stocked Potter's Shed with 3 dry Cups).

## Building the Kiln
1. The Build tab lists the Kiln (category Other) with its cost: 6 Stone, 6 Clay, 30 Hammer blows.
2. The ghost is refused with a reason in each of these: under a roof (for example under the Workshop or Potter's Shed roof: "A kiln needs open sky"); within 3 m of a Cabin, the Workshop, the Potter's Shed or a tree; on flood-prone ground. [Say which ones you tried.]
3. Place the Kiln site. Now try to place a Workshop, Potter's Shed or Cabin site within 3 m of it: refused (the clearance works both ways). A parked Hand Cart beside the site is allowed.
4. Press Shift+' and see what fits in your pack. Deliver the 6 Stone first, then the 6 Clay, with R (press Shift+' again for the rest if something did not fit), and hammer 30 blows. [Write down which items did not fit.]
5. The finished Kiln appears and looks like a kiln (empty). [Describe how it looks: shape, size, anything floating or poking through.]

## Loading and lighting
6. With the Hammer NOT in hand, aim at the Kiln. The prompt reads "Kiln (load dry pieces and 10 Firewood)". Is the prompt text readable at its size? [Say if it is too small.]
7. Press R: the dry pieces go in first, then the Firewood, with a message like "Loaded 3 Clay Cups". The prompt then shows what is in, for example "Kiln (5 pieces, 10 of 10 Firewood in)".
8. Press E with too little in it: with fewer than 10 Firewood, "Needs N more Firewood to fire"; with no pieces, "Load at least one dry piece first".
9. Use the ' kiln (10 of 12 space used, 10 Firewood). With 3 dry Cups in your pack, press R: only 2 go in (2 space left) and 1 stays in your pack.
10. Press E on a kiln with at least one piece and 10 Firewood: "The kiln is lit: N pieces will fire for 24 hours." The prompt now reads "Kiln (firing, 24 h to go)" (or the hours left) and the Kiln looks different (glow or smoke). [Describe the look.]
11. While it fires: R says you cannot load it, Hammer plus R says you cannot take it down (with the hours left), and E shows the hours left.

## Firing and collecting
12. Sleep, or skip time (F5). The hours left count down properly, and after 24 hours the prompt reads "Collect fired pieces (N)" and the Kiln looks different (fired pieces on it, an ash heap). [Write down the prompt and the hours you saw.]
13. Before collecting, press R: "Collect the fired pieces before loading again".
14. Press E: "Took 8 fired pieces and 3 Ash." Your pack now has Clay Cups, Bowls, a Clay Cooking Pot, a Water Crock and 3 Ash (0.3, 0.6, 1.8 and 5.5 kg).
15. If your pack is nearly full, a piece that is too heavy stays in the Kiln with "Too heavy - no room to carry them"; when only Ash is left the prompt reads "Collect the Ash". [Skip if you cannot easily fill your pack.]
16. Ctrl+; finishes a running firing at once.

## Selling
17. At the Trading Post, the sell list shows the five fired pieces with their prices: Clay Cup $4, Clay Bowl $6, Clay Cooking Pot $10, Water Crock $12, Brick Batch $6. Sand and the dry pieces are not on the list. [Write down the prices shown.]
18. Sell several Clay Cups. Each sale drops the Cup's price by about 10 percent, and the sales show under Crafted goods in the Finances ledger. [Write down the prices after 1, 2 and 3 sales.]
19. The Clay Cooking Pot sells for about $10 (not a lower tool sell-back price).
20. The Clay Cooking Pot is a Tool item: in the Inventory, can it go in a hotkey slot, and does equipping or using it do anything odd or break anything? [Describe.]

## Taking it down, saving, the full chain
21. Hammer plus R on a loaded Kiln: refused ("Empty the kiln before taking it down"). On an empty Kiln it dismantles and gives back 3 Stone and 3 Clay. A Kiln holding only Ash cannot be taken down until you collect the Ash.
22. Save with a loaded Kiln, then with a firing Kiln, then with fired pieces waiting. Go to the main menu and load. Each state is the same after loading, with the same hours left.
23. Do the whole chain once (optional): dig Clay and Sand, shape 2 Cups in the Potter's Shed, wait a day, collect them, load the Kiln with them and 10 Firewood (F11 or F12 kit, or split some Logs), fire it, collect, and sell. [Write down anything awkward.]
24. The Potter's Shed help (in the Craft and Build screen) says dry pieces must be fired in a Kiln first.
25. The Workshop and the Potter's Shed still work as before.
26. Console: any errors? (The known NullReferenceException at PlayerController.cs:217 when Play Mode stops does not count.)

Afterwards: note anything that looked or felt wrong even if it passes. Then I restore Saves from SavesBackup38 and we stage the 28 paths and commit.
