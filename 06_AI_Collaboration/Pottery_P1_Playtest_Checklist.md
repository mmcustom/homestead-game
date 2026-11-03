# Pottery P1 Playtest Checklist (2026-11-02)

Build: HEAD 96cbd74 plus uncommitted pottery P1 (14 modified paths, 18 new, expected 32 to stage). Suite 1329 of 1329, Console 0 errors, no Play Mode run by Claude Code.
Reply with item numbers and pass or fail. Write down anything in [brackets].

## Setup
Play Mode, Game tab maximized (Shift+Space). Press ; (pottery kit: Hammer, Shovel, 8 Branches, 8 Tall Grass, 2 Cordage, 12 Clay, 6 Sand, and a Wood Pile with 3 Logs 3 m ahead). For the second half you will also press Shift+; (a finished, stocked Shed with 2 wet Bowls and 3 dry Cups on the shelf).

## Digging (Shovel, at a creek or pond bank)
1. Stand within about 2 m of the water on dry ground and aim the Shovel at the ground. The HUD says 'Wet clay bank - hold click to dig Clay'. Hold click: 4 blows, 1 Clay in the pack. [Write down roughly how wide the Clay strip felt along the water, and say if the HUD line flickered while you walked along the bank.]
2. Aim at dry ground 2 to 3 m back from the water. The HUD says 'Dry creek bank - hold click to dig Sand'. Hold click: 1 Sand (2 kg) in the pack.
3. Aim into shallow water, standing on the bank (up to about knee-high water, 0.3 m): 'Creek bed - hold click to dig Clay' and you get Clay. Aim at deeper water (the middle of the pond or a deep part of the creek): 'Water too deep to dig - try the shallows or the wet bank' and nothing digs. More than 3 m from any water the Shovel does nothing and its idle line mentions Clay at the water's edge and in the shallows, and Sand up the bank. [Say if a thin creek or the pond edge fails, or if you find a steep waterfall-like drop.]

## Building the Shed
4. The Build tab lists the Potter's Shed (category Other) with its cost: 3 Logs, 8 Branches, 8 Tall Grass, 2 Cordage, 24 Hammer blows.
5. The ghost is refused, with a reason, in each of these: within 3 m of the Cabin, a Well or an Outhouse; on flood-prone ground; with a tree trunk within 2 m; within 3 m of the Workshop or a Wood Pile. [Say which ones you tried.]
6. Place the site, deliver the materials with R over a few trips (Logs from the Wood Pile), hammer 24 blows. The finished Shed appears. [Describe how it looks: roof, shelf, anything poking through or floating.]

## Stocking and the screen
7. With the Hammer NOT in hand, aim at the Shed and press R: Clay and Sand go in (caps 16 Clay and 8 Sand), with a message.
8. Press E. The Potter screen opens. Can you read everything without leaning in? [Say which text, if any, is too small.] Is the panel cut off at the top or bottom? [Say what shape your monitor is.]
9. The screen shows five rows, the pack line ("You carry: ..."), the stock button, the status line and a shelf block with the space used out of 12.
10. Keys: 1 to 5 choose a piece. Up and Down move the choice and wrap. Left and Right change the count from 1 to 4 and stop at the ends. The plus and minus buttons work with the mouse too.
11. The needs rows ("Clay - need 6 - shed has 8 - enough") and the total shaping time change with the count (the Cup is 1 hour each, the Bowl 1, the Pot, Crock and Brick Batch 2).
12. Start 3 Cups: a green message appears, the status line says it is shaping 3 Cups with hours left, and the shed's Clay and Sand each drop by 3. Start is greyed while it runs.
13. Choose 4 Crocks with too little Clay in the shed: the button says short of materials, and the reason says what is short.
14. The stock button is greyed with a reason when you carry no Clay or Sand, and when the shed is full of what you carry.
15. Esc and the Close button close it. Outside, the prompt says the shed is shaping; E reopens it read-only.
16. With the screen open, I, B, T and L open nothing, and the clock does not move.

## Time, shelf and collecting
17. Press Ctrl+;: the 3 Cups appear on the shelf (dark means wet), and the screen's shelf block says "Drying: 3 Cups, 1 day left" or similar.
18. Skip a day (F5) or sleep: the Cups turn pale (dry), the prompt says to collect, and E puts 3 Dry Clay Cups in your pack (0.3 kg each).
19. Shape 2 Bowls (2 days): after one day they are still wet, after two they are dry. [Write down what the shelf block said each day.]
20. Press Shift+; for the stocked Shed (3 dry Cups, 2 wet Bowls = 5 space used). Try to start 4 Crocks (needs 8): refused with a reason about the shelf. Try 3 Crocks (needs 6, total 11): allowed.
21. Start a 2 hour job and sleep. It is finished when you wake.
22. Collect a Dry Water Crock (6 kg) and a Dry Brick Batch (16 kg). If your pack is too full, the piece stays on the shelf with a message.
23. Hammer plus R on the Shed: refused while a job runs or any piece is still wet. With the shelf holding only dry pieces and no job, it dismantles and gives back half the materials, the stock and the dry pieces whole.
24. At the Trading Post, Sand and the dry pieces do not appear in the sell list.

## Saving, the Workshop, odds and ends
25. With a job running and wet and dry pieces on the shelf, save. Go to the main menu with the screen open (it must close), then load. The shelf, the job hours and the days left are the same.
26. The Workshop still works: E at the bench opens its screen, its keys still work.
27. Console: any errors? (The known NullReferenceException at PlayerController.cs:217 when Play Mode stops does not count.)

Afterwards: note anything that looked or felt wrong even if it passes. Then I restore Saves from SavesBackup38 and we stage the paths and commit.
