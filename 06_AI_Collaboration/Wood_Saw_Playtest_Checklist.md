# Wood Saw and Wooden Wheels Playtest Checklist

Build: Wooden Wheels (1 Log only) and the Wood Saw, both uncommitted, 1132 edit-mode tests passing. Code baseline 8d69a5c.
Reply with item numbers and pass or fail (and what you saw if fail). For items 6 and 7 please send the numbers.

Before you start
- Maximize the Game tab (Shift+Space). Use a throwaway new game. SavesBackup38 is the safe copy of the current Saves.
- F12 gives the full kit (an Axe and materials). PageUp gives $1,000.
- You need the Axe and the Saw to compare. Keep both in your pack.

Wooden Wheels
1. Craft tab, Materials: the Wooden Wheel row costs 1 Log only (no Cordage), needs no tool, crafts instantly. The help text reads "a wheel cut from a Log. Two go into a Hand Cart (Build tab); no tool needed."
2. Craft 2 Wooden Wheels from 2 Logs. The Build tab's Hand Cart Site cost line still reads "2 Wooden Wheels, 3 Logs, 6 Branches, 3 Cordage".

Trading Post
3. The Trading Post lists a Wood Saw at $30. Buy it. Your cash drops by $30 and the Saw is in your pack (2 kg).
4. Open the Finances statement: the $30 shows under Tools in your purchases.
5. Sell the Saw back: you get $12. Buy it again for the rest of the test.

Felling (use the same kind of tree for both tools)
6. Fell an average broad hardwood with the Axe, then a similar one with the Saw. Count blows and use a stopwatch. Expected: Axe about 11 blows (about 10 s), Saw about 7 blows (about 6 s). A tall hardwood: Axe about 15, Saw about 9. A shrub with the Saw: 2 blows. Send the blows and seconds for each tree.
7. Stamina: note the stamina bar before and after one average tree with each tool. Expected (full food and water): Axe about 44 to 60, Saw about 28 to 36 of 100 used.
8. The Saw swings like the Axe (same animation and sound). The progress bar fills and the tree falls normally.

Splitting
9. Equip the Saw and hold click on a pile of Logs: each Log takes 3 blows and gives 5 Firewood (the Axe takes 4).
10. Inventory split with only the Saw in your pack (sell or drop the Axe for this): each Log takes about 2.6 s.
11. Carry both Saw and Axe and split from the Inventory: it uses the Saw's speed (about 2.6 s per Log). With only the Axe it is 4 s per Log.

Nothing broken
12. The Axe and Primitive Axe still work as before (Primitive: about 17 blows for an average broad tree, 6 s per Log splitting from the Inventory).
13. Load one of your older real saves with this build: it loads with no errors and your tools still work.

Result (2026-10-31): items 1 to 13 passed (blow and stamina numbers for items 6 and 7 not recorded).

Recheck after the 2-wheels change (1 Log makes 2 Wooden Wheels)
14. Craft tab, Materials: the row reads "Wooden Wheels x2" and costs 1 Log. The help text reads "one Log is cut into two wheels. Two wheels go into a Hand Cart (Build tab); no tool needed."
15. Craft it once: 1 Log is used and you get 2 Wooden Wheels in your pack (6 kg total). With a nearly full pack the craft still works and anything that does not fit is dropped on the ground.
16. The Build tab's Hand Cart Site still reads "2 Wooden Wheels, 3 Logs, 6 Branches, 3 Cordage", and one craft covers the wheels.

Result (2026-10-31): recheck items 14 to 16 passed (2-wheels change).
