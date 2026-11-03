# Pottery P3 Playtest Checklist (2026-11-03)

Build: HEAD ed27b39 plus uncommitted pottery P3 (7 modified paths, 2 new, expected 9 to stage). Suite 1382 of 1382, Console 0 errors, no Play Mode run by Claude Code.
Reply with item numbers and pass or fail. Write down anything in [brackets].

## Setup
Play Mode, Game tab maximized (Shift+Space). Get a Clay Cooking Pot: press ' (a finished, loaded Kiln 4 m ahead), E to light it, Ctrl+; to finish the firing, E to collect. A Clay Cooking Pot is among the pieces. For items 1 to 7 do NOT carry the iron Cooking Pot (start as a Pioneer, or drop or sell it). You also need a campfire, a Tripod and something to boil, built the way you normally do. [Claude Code did not report a dev key for a quick Tripod setup; if you used one, say which.]

## Using the clay pot
1. The Clay Cooking Pot is in your pack and its Inventory tip reads sensibly. [Write down what the tip says.]
2. Carrying only the clay pot, aim at the built Tripod: it offers to hang a pot.
3. Hang it. The Tripod shows a squat pot in clay brown with a paler rim, not iron. [Describe how it looks.]
4. The Journal note for hanging a pot appears (it used to fire only for the iron pot).
5. With the clay pot hung and the fire lit, the "Cook or boil" hint appears. Boil water: it works. [Compare the time and amount with the iron pot if you can.]
6. Boil Maple sap with the clay pot: it works. (Skip if no sap is handy.)
7. Take the pot down. You get the clay pot back, not an iron one.

## Both pots, and the iron pot
8. Carry both the iron and the clay pot: the Tripod hangs the iron pot first. With the clay pot hung and the iron pot in your pack, taking down or swapping gives back the clay pot (the one that hangs), not the iron one.
9. The iron pot still works exactly as before: hang it, boil, take it down.
10. Lye still needs the Potash Kettle, and the Kettle works as before.
11. Put the clay pot in a hotkey slot and hold it: nothing happens (same as the iron pot).

## Selling, saving, odds and ends
12. At the Trading Post the Clay Cooking Pot sells for about $10, and the iron pot sells back for $4.00 (not $4.80: the sell-back rounds down to whole dollars).
13. Save with the clay pot hung and a boil in progress, go to the menu and load. It is still hung and the boil carries on.
14. Pottery still works: the Potter's Shed and the Kiln behave as in P1 and P2 (a quick look is enough).
15. Console: any errors? (The known NullReferenceException at PlayerController.cs:217 when Play Mode stops does not count.)

Afterwards: note anything that looked or felt wrong even if it passes. Then I restore Saves from SavesBackup38 and we stage the 9 paths and commit.
