# Workshop a2-1 Playtest Checklist (2026-11-01)

Build: HEAD 6eb5812 plus uncommitted a2-1 (18 paths). Suite 1259 of 1259, Console 0 errors, no Play Mode run by Claude Code.
Reply with item numbers and pass or fail. Numbers in square brackets are things to write down.

## Setup
Play Mode, Game tab maximized (Shift+Space), a normal playtest save. Press \ (Workshop kit: Hammer, Axe, Branches, Tall Grass, Cordage and a Wood Pile). Press Shift+\ (a finished Workshop with the bench stocked 5 Logs, 8 Branches, 3 Cordage). Walk to the bench. If Shift+\ makes a second Workshop instead of stocking the first, say so; that is fine.

## Reading the screen (first, this is the big-text check)
1. Walk up to the bench. The prompt reads "Choose what to make at the Workshop" (the HUD adds the key itself).
2. Press E. The Workshop screen opens. From where you normally sit, can you read every word without leaning in? Say which text, if any, is still too small.
3. The whole panel fits on the screen: nothing cut off at the edges, nothing wrapped onto a second line by accident, no text overlapping.
4. The list shows four rows: Rough Stool, End Table, Chair, Table, each with hours and a price. [Write down the four prices shown.]
5. Click each row. The right side changes to that piece: name, hours and kg and base price, then what it needs. The Rough Stool and End Table show no Cordage row.
6. With the bench at 5 Logs, 8 Branches, 3 Cordage, every row reads "enough".

## Making a piece
7. Choose Table and click START TABLE. A green message says it started, the top line reads "Making a Table, 5 h to go", and the button turns grey.
8. While it runs, click Chair. The button reads BENCH IS BUSY and the reason names the Table (the piece being made), not the Chair.
9. Esc closes the screen. Outside, the prompt reads "Workshop (making a Table, ...h to go)". Press E again: the screen opens and is read-only.
10. Close button (mouse) also closes it.
11. With the screen open, press I, B, T and L. None of them opens another window on top. The game clock does not move while the screen is open (watch it for 10 seconds). [Write down anything that does leak.]
12. Close the screen, press Ctrl+\ to finish the job. The prompt reads "Collect Table (ready in the Workshop)" and a Table stands on the bench: long top on four legs, not poking through the bench or the shed wall.
13. Press E. The Table goes into your pack (24 kg). Check it in the Inventory.
14. Open the screen again. The bench holds 2 Logs, 4 Branches, 1 Cordage. Choose Table: the button reads SHORT OF MATERIALS, the short rows are orange, and the reason says what is short (1 Log and 1 Cordage).
15. Choose Chair: the stock is exactly enough. Start it, press Esc, Ctrl+\, collect it. The finished Chair showed a seat with a back.

## The other two pieces
16. Press Shift+\ again (or stock a bench by hand if it will not). Start an End Table, finish it, look at it on the bench (square top, low shelf), collect it.
17. Start a Rough Stool, finish it, collect it. It looks as it did in a1.

## Tool gate (only if it is easy)
18. Put the Axe, Primitive Axe and Wood Saw out of your pack (drop or stow them). Open the screen with a stocked bench: START is grey and reads NEED A TOOL, with the tool names in the reason. Put the Axe back and it clears.

## Time, saving and selling
19. Start a Chair (4 h). Sleep, or skip time (F5 / F7). The Chair is ready when you wake. [Write down the clock hours if you noticed.]
20. Start a Table, save, return to the main menu with the screen open (it must close by itself), load the save. The job is still running with the right hours left.
21. Take the four pieces to the Trading Post and sell one of each. The prices match the list (stool $14, End Table $12, Chair $18, Table $28 at full price) and the sales show as Crafted goods. [Write down the four sale prices.]
22. Back at the bench, open the screen. The list prices show what the Trading Post pays now (lower for what you sold). [Write down what each row shows.]

## Odds and ends
23. Open the Craft and Build screen: its help text lists the four pieces.
24. Hammer plus R on the bench while a job runs is still refused (as in a1).
25. Console: any errors? (The known NullReferenceException at PlayerController.cs:217 when Play Mode stops does not count.)

Afterwards: note anything that looked or felt wrong even if it passes. Then I restore Saves from SavesBackup38 and we stage and commit.
