# Save Slots Step 2 (Whole-Slot Atomic Write) - Play Mode Checklist (for Mike)

Built 2026-10-18 by Claude Code; compiled clean (0 errors, 0 warnings); 49 EditMode tests PASS (they ran on a temp folder, not the real Saves); PLAY-TESTED by Mike 2026-10-18 (all 13 items PASS, see the last Results entry); UNCOMMITTED. Design and report: Systems/Save_System.md ("Step 2: whole-slot atomic write").
Status values: OPEN, PASS, FAIL, PARTIAL. Reply with a number and PASS or FAIL.

## A. Before you start
1. BACK UP THE SAVES FOLDER by hand (C:\Users\mmcus\AppData\LocalLow\MM Custom\Homestead\Saves; paste `%USERPROFILE%\AppData\LocalLow\MM Custom\Homestead` into the File Explorer address bar) to a safe place. Do not skip this: step 2 changes how EVERY save is written. Status: OPEN
2. Start with a clean Console and enter Play Mode. Status: OPEN

## B. Normal saving (slots must behave exactly as before)
3. Save to an EMPTY slot, then save again over it (pause menu Save to Slot N). The slot panel updates each time ("Game saved." shows) and there are NO .tmp, .old or .del folders left inside Saves\ afterwards (look in File Explorer). Status: OPEN
4. Save over a slot that already has an older save from step 1 (for example slot 1, your playtest slot). It saves fine and the old slot is replaced whole. Status: OPEN
5. Autosave: sleep, or press F5 (skip day), or let a day end. ONLY the Autosave row's time changes; manual slots keep their saved-at times. Status: OPEN
6. Quit paths: (a) Main Menu from the pause menu, then Continue; (b) quit from gameplay (stop Play Mode) and check the Autosave row updates. Status: OPEN

## C. Copy, Delete, Continue and Load
7. Copy a slot onto an OCCUPIED slot. The target takes the source's game, shows its own name, the source is unchanged, and loading the target gives the same game. Status: OPEN
8. Delete a slot. It disappears from the panel and Saves\ has no leftover slotN.del folder. Deleting the CURRENT slot clears "current" (the pause button reads "Save Game..."). Status: OPEN
9. Right after a save or autosave, Continue loads the newest save and Load Game lists every slot correctly (a slot never looks empty just after saving it). Status: OPEN

## D. Failure and recovery (do these carefully)
10. Failure message: with an Explorer window open INSIDE a slot folder (for example slot3), save to that slot. Either it saves normally, or it shows the existing save-failed message and the OLD slot stays intact. Then close the window; the next save works. Mark PASS if nothing is lost either way. Status: OPEN
11. Hands-on recovery on the real folder with the game CLOSED and your backup in place (or on a copy if you can point the game at it). Each check after the next start-up:
   a. Create an empty folder slot3.tmp: it is gone.
   b. Rename slot2 to slot2.old (so slot2 is missing): slot2 is back and the Console/log shows a "Restored slot2" warning.
   c. Put a slot4.old next to an existing slot4: the .old is deleted and slot4 is untouched.
   d. Create a folder slot11.old: it is left alone.
   Status: OPEN
12. Console stays clean during all of this (the unrelated MCP WebSocket notice may appear). Status: OPEN

## E. Restore when you are done
13. Put your backed-up Saves folder back if you want slot 1 and the other test slots exactly as before. Status: OPEN

## Results log
(Add dated results here.)

### 2026-10-18 - Mike's pass/fail run (all PASS)
Mike reported PASS, one word per item, no notes, for items 1 to 13: backup made, clean Console start, save to empty slot and overwrite (no .tmp/.old/.del left), overwrite of an older step-1 slot, autosave isolation, quit paths, Copy onto an occupied slot, Delete, Continue and Load Game right after saving, failure with an Explorer window open in the slot folder, hands-on recovery (stray .tmp removed, .old restored with warning, .old beside a real slot deleted, slot11.old ignored), clean Console, Saves folder restored.
- Verdict: save slots STEP 2 is PLAY-TESTED, all items PASS, plus the 49 EditMode tests. Still UNCOMMITTED. Per-item detail (for example whether item 10 saved normally or showed the failure message) was not reported.
- 2026-10-18 (Mike, follow-up on item 10): "yes it saved normally." Item 10: with an Explorer window open inside the slot folder, the save went through normally (no failure message), so the rename was not blocked in that test; the retry-and-fail path was therefore not exercised by hand, only by the EditMode locked-rename tests.
