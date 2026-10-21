# Save / Load Screen Redesign - Play Mode Checklist

Built 2026-10-20. Compile-checked; 154 edit-mode tests pass (146 old + 8 new in `SlotRulesTests.cs`). PLAYTESTED 2026-10-20: all 37 items pass (Mike). COMMITTED 493e35c and pushed to master 2026-10-20. No save file was touched while building or testing: only edit-mode tests ran, and they use temp folders.

**Setup:** BACK UP YOUR SAVE FOLDER FIRST. Close the game, then copy the whole folder `C:\Users\mmcus\AppData\LocalLow\MM Custom\Homestead\Saves` somewhere safe. It holds `slot1`, `slot2`, `slot3`, `autosave` (each a folder) and `slots.json` (the remembered slot). Play testing writes to `autosave`, to any slot you save to, and to `slots.json`; Copy and Delete in the panel change slots too. When done, close the game and restore the folder from the backup if you want your saves back exactly as they were.
**What changed:** one screen for both the pause menu's "Save / Load..." and the main menu's "Load Game". It lists the Autosave on top, then slots 1 to 10, all at once, with no Save/Load tabs. Each row: slot, name, a CURRENT badge, in-game date and time (the date includes the season), difficulty, when it was saved, then Save, Load, Copy, Delete. Playtime is not stored in saves, so it is not shown. New Game's slot pick is unchanged in purpose (it uses the same screen, with Select buttons).
**The rules:** the current slot is the last manual slot you saved to or loaded from. It is written into each slot's `slotinfo.json` as `lastSlot`, and every autosave carries it forward. Saving over the current slot or an empty slot is one click; saving over any other filled slot asks "Overwrite Slot N (date, time)?" with Yes and No. Loading from the pause menu still asks "Unsaved progress in the current game will be lost." (unchanged); loading from the main menu does not.
**Have ready:** at least 3 manual saves in different slots (you have slot1 to slot3), one empty slot, and a way to make the Autosave newer than the rest (sleep, wait for a day to end, or Main Menu).

## A. Opening the screen
1. [ ] Pause menu (Esc) > "Save / Load...": the screen opens at once with every slot visible. There are no Save and Load tabs and no mode to pick. Title reads "Save / Load".
2. [ ] The Autosave row is on top (slot "A"), then slots 1 to 10. Empty slots read "Empty". Filled rows show the name, the in-game date with season and the time (for example "Spring 3, Year 1 . 14:30"), the difficulty and the real date and time saved.
3. [ ] Main menu > "Load Game": the same screen opens, title "Load Game". There are NO Save buttons anywhere. Every filled row has Load; the Autosave row has Load only (no Save, no Delete).
4. [ ] In game, every manual slot (filled or empty) has a Save button. The Autosave row never has one.
5. [ ] Esc and Back close the screen. From the pause menu you are back at the pause menu; from the main menu you are back at the main menu with its buttons usable.

## B. Current slot and pre-selection
6. [ ] Load slot 2 from the main menu. In game, open Save / Load: slot 2 shows a "[CURRENT]" badge and a green highlighted row. No other manual slot has a badge, and the Autosave row has no "[CURRENT GAME]" badge.
7. [ ] Slot 2 is pre-selected: its row is tinted and keyboard focus is on its Save button (press Enter/Space to confirm the focus goes there; arrow keys move it).
8. [ ] Save to slot 3 (see D). Reopen: the badge has moved to slot 3, and slot 3 is pre-selected.
9. [ ] Continue from the main menu (loads the newest save, usually the Autosave). Open Save / Load: the badge is still on the slot you last used, not lost. (This is the main fix: Continue used to forget it.)
10. [ ] Autosave carrying the badge: with slot 3 current, sleep or let a day end so an autosave is written, quit to the main menu and press Continue. Slot 3 still shows [CURRENT].
11. [ ] Load slot 1 (a manual slot) from the pause menu, confirm. Reopen: the badge is on slot 1.

## C. No manual save yet, and the first empty slot
12. [ ] Start a New Game and pick any slot (it is chosen up front). Open Save / Load before saving anything: that slot has no badge (it holds nothing yet). The Autosave row shows "[CURRENT GAME]". The first empty manual slot is pre-selected.
13. [ ] With no current slot, filled all ten slots (or imagine it): nothing is pre-selected. (If you cannot fill ten slots, this one is covered by the edit-mode test.)
14. [ ] A game loaded from the Autosave that has no `lastSlot` in it (see H): the Autosave row shows "[CURRENT GAME]" and no manual slot has a badge; the first empty manual slot is pre-selected.

## D. Saving
15. [ ] Save over the CURRENT slot: one click on its Save button, no question asked. The screen closes and the pause menu says "Saving..." then "Game saved." The slot's saved time is now.
16. [ ] Save over a different FILLED slot: the box asks "Overwrite Slot N (date . time)?" with the slot's own in-game date and time. No leaves everything as it was and returns to the screen. Yes saves, closes, and makes that slot current.
17. [ ] Save to an EMPTY slot: one click, no question. It becomes current.
18. [ ] Esc while the overwrite question is up dismisses it (same as No).
19. [ ] The pause menu's "Save to Slot N" button (above Save / Load...) appears ONLY when there is a slot to save to, and saves to it in one click with no screen. With no current slot it is not shown at all: the menu is just Resume, Save / Load..., Main Menu, Quit, with no gap and no overlap, Resume is focused, arrow keys/controller move through the four in order, and Esc still resumes. Check both cases (and that Save to Slot N appears after you save once).

## E. Loading
20. [ ] Load a slot from the pause menu: it asks "Load Slot N? Unsaved progress in the current game will be lost." Yes loads that slot; No cancels. (Unchanged behaviour.)
21. [ ] Load a slot from the main menu's Load Game: no question; the game loads.
22. [ ] Load the Autosave from the screen (pause menu and main menu both): it loads. Afterwards the badge shows the slot the autosave carried (if any).
23. [ ] The loaded game is the right one: day, time, position, inventory match the row you chose.

## F. Copy, Delete and New Game (existing features)
24. [ ] Copy a slot onto an empty slot and onto a filled one (asks first), from a manual slot and from the Autosave. The copy's row works like any slot. Copying onto slot N then loading slot N makes N current.
25. [ ] Delete the CURRENT slot (asks first): the badge disappears, nothing is current, and the first empty slot becomes the pre-selection. Delete another slot: the badge stays.
26. [ ] New Game > difficulty > Start opens the same screen with "Choose a slot for your new game", Select buttons, no Autosave row, no Load/Save. An empty slot is one click; a filled slot asks "Overwrite Slot N (...)?" first.

## G. Quit and Continue
27. [ ] With slot 3 current, quit the game from the pause menu (or close it), start it again, press Continue, open Save / Load: slot 3 still has the badge.
28. [ ] Main menu > Load Game after a fresh start: the last slot you used is pre-selected (focus on its Load button); if there is none it is the Autosave's Load button.

## H. Old and bad data
29. [ ] An old save with no `lastSlot` (any slot you saved before this build; the existing Autosave and slot1 to slot3 are old): load it. It loads exactly as before, nothing is lost, and no error appears. If it is a manual slot it becomes current; if it is the Autosave nothing is current until you save.
30. [ ] Bad `lastSlot`: close the game, open the Autosave's `slotinfo.json`, set `"lastSlot": "slot9"` (a slot you have not used, so it is empty), save the file, start the game, Continue. No badge appears on slot 9 or anywhere, and there is no error. Repeat with `"lastSlot": "banana"` and with `"lastSlot": "autosave"`: same, no badge, no error.
31. [ ] Missing file: set `"lastSlot"` to a slot whose folder you then delete in the screen. Same: treated as none.

## I. Regression
32. [ ] Normal saving: Save to the current slot, then load it. Everything you did is there (a built structure, inventory, a lit fire, the time).
33. [ ] The autosave still writes at the end of a day, on sleep, on Main Menu and on quit, and only ever to the `autosave` slot (a manual slot's saved time does not change).
34. [ ] The main menu still works: Continue (grey when there is no save), Load Game, New Game, Quit. The difficulty step is unchanged.
35. [ ] The pause menu still works: Resume, Save, Save / Load..., Main Menu, Quit. Esc with the screen open closes the screen only, not also the pause menu.
36. [ ] With 154 edit-mode tests green and nothing else changed, the Console is free of errors and warnings through all of the above.
37. [ ] No text overlaps or clips on any row, at 1920x1080 and at a smaller Game view such as 1280x720. Worst case on one screen: the Autosave row with "[CURRENT GAME]" and a date like "Autumn 28, Year 10 · 23:59" (badge, date, difficulty "Homesteader" and a saved time like "Oct 20, 10:25 AM" each in their own column), a manual row with "[CURRENT]", and Save, Load, Copy and Delete all on the one row inside the panel.
