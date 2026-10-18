# Save Slots Step 1 - Play Mode Checklist (for Mike)

Built 2026-10-17 by Claude Code; compiled clean (0 errors, 0 warnings); PLAY-TESTED by Mike 2026-10-17 (all listed items PASS, see the last Results entry); UNCOMMITTED. Design: Systems/Save_System.md ("Save Slots", Decisions 1 to 6). Step 2 (whole-slot atomic write) is NOT built yet and is not part of this checklist.

Status values: OPEN, PASS, FAIL, PARTIAL.

## A. Before you start (do this first, it is your only fallback)
1. BACK UP THE REAL SAVES FOLDER by hand: copy the whole `Saves` folder (it is C:\Users\mmcus\AppData\LocalLow\MM Custom\Homestead\Saves; AppData is a hidden folder, so paste `%USERPROFILE%\AppData\LocalLow\MM Custom\Homestead` into the File Explorer address bar. Ignore the empty DefaultCompany\Homestead\Saves folder, it is from before the company name was set.) to somewhere safe, so slot1 (your playtest save) can be restored byte for byte. Status: OPEN
2. Start Unity with a clean Console and enter Play Mode from the MainMenu scene. Status: OPEN

## B. Slot 1 and the main menu
3. Slot 1 untouched: the main menu shows Continue and a new "Load Game" button under it. In Load Game, Slot 1 shows as "Slot 1" with a saved-at time and an EMPTY in-game column (it has no slotinfo.json yet). Status: OPEN
4. Loading slot 1 restores your playtest game exactly (position, inventory, money, date, difficulty, weather). Status: OPEN
5. Layout (Claude Code could not see it): both menu columns grew by one button, the slot panel is 1500x764. Nothing overlaps, no text is cut off, every row and button is readable and clickable. Status: OPEN

## C. Saving
6. First save: in a game with no active slot, a plain Save opens the slot panel in Save mode; save to Slot 2; "Game saved." appears. The row then shows the in-game date and time, difficulty and saved-at time, marked "current". Status: OPEN
7. Save again: Save Game now writes Slot 2 directly (no panel). The pause date line matches the Slot 2 row. Status: OPEN
8. Autosave isolation: sleep, then use F5 to skip a day. Slot 2 does NOT change (saved-at time stays), and the Autosave row updates. Status: OPEN
9. Quit paths: (a) stop Play Mode, (b) use Main Menu from the pause menu. Each updates ONLY the Autosave row; every manual slot's saved-at time stays the same. Status: OPEN
10. The Autosave row has no Save button and no Delete button (Load and Copy only). Status: OPEN

## D. Continue and Load
11. Continue loads the NEWEST save across all slots. Test both ways: Autosave newer than Slot 2, and Slot 2 newer than Autosave. Status: OPEN
12. Load from the main menu (Load Game) and from the pause menu (it warns "Unsaved progress will be lost" first) both restore the right game. Status: OPEN
13. No state leaks between games: set up two different games (for example Slot 1 on one difficulty with certain money, inventory and weather, Slot 2 on another), load one from inside the other, and check inventory, money, difficulty, weather, time and Heat Wave state are all the loaded game's, nothing from the previous game. Status: OPEN
14. Loading the Autosave leaves NO "current" slot; the next plain Save opens the panel. Loading a manual slot makes it "current". Status: OPEN

## E. Copy and Delete
15. Copy slot 1 to slot 5: the copy row shows the target's name ("Slot 5"), and its in-game and difficulty columns are BLANK while slot 1 has no slotinfo.json (a copy of a slot that has been saved since the build shows the same date and difficulty as its source; Claude's first wording here expected them on a copy of slot 1, which was wrong); loading slot 5 gives the same game as slot 1. Copying onto an occupied slot asks for confirmation. Status: OPEN
16. Copy keeps the source's saved-at time, so a copy and its source count as the same age for Continue. Check that Continue still loads one of them sensibly (it should not fail or pick an empty slot). Status: OPEN
17. Delete asks for confirmation first. Deleting the current slot clears "current". Other slots are untouched. Status: OPEN

## F. New Game
18. New Game: after the difficulty step the slot pick appears. Picking an EMPTY slot starts the game. Status: OPEN
19. Picking an OCCUPIED slot asks you to confirm. Note (Claude Code): the old save in that slot stays intact until your first manual Save, which then overwrites it without asking again. Check that the old save is still there (Load Game, before you save). Status: OPEN
20. Back from the slot pick returns to the difficulty panel and changes no slot. Status: OPEN

## G. Esc, failure and Console
21. Esc inside the slot panel from the pause menu closes it (or backs out of a copy or confirmation) and does NOT also unpause. Status: OPEN
22. Failed load: with a slot whose files you have damaged on a COPY of the Saves folder (for example delete one game file), loading it returns to the main menu with a message and does not overwrite anything. Do this only on a throwaway copy. Status: OPEN
23. Console stays clean throughout (the unrelated MCP WebSocket notice may appear; no errors or warnings from the game). Status: OPEN
24. Optional: with the Saves folder empty or renamed (temporarily), Continue is disabled and Load Game shows all slots "Empty". Restore the folder afterwards. Status: OPEN

## H. Restore when you are done
25. Put your backed-up `Saves` folder back so slot1 is exactly your playtest save again (or tell Claude to keep the test slots). Status: OPEN

## Known notes from Claude Code (not bugs unless they look wrong)
- The old "Delete Save" context menu item was removed so a stray click cannot delete a save.
- The main menu's old confirm-panel fields are still in the scene but no longer shown (cleanup would be a scene edit; not done).
- Pause-menu Load does not autosave first (that would overwrite an Autosave you may be loading), hence the warning.

## Results log
(Add dated results here as items pass.)

### 2026-10-17 - first four screenshots (Mike, no comment)
Screenshots: (1) pause menu, Summer 9 Year 9, 10:48, Cold Front 79F; (2) Save Game panel with Slot 2 current; (3) Save Game panel after "Copied Slot 1 to Slot 4." with Slot 3 current; (4) Load Game panel with Slot 1 current and an Autosave row.
- 5 PARTIAL PASS (layout): the pause menu shows Resume, Save Game, "Save / Load...", Main Menu, Quit to Desktop with no overlap, and the slot panel fits on screen with all ten rows, the Save and Load tabs, the Back button and a status line readable and nothing cut off. The main menu (Continue, Load Game, difficulty and slot pick) was not in the screenshots, so its layout is still unchecked.
- 6 PARTIAL: Slot 2 shows "current", the in-game date and time (Summer 9, Year 9, 11:15), Pioneer and a saved-at time, and slots 3 to 10 are Empty. Not seen: the panel opening from a plain Save with no active slot, and the "Game saved." message.
- 7 PARTIAL (supported): later screenshots show Slot 2 re-saved (11:15 at 1:28 PM, then 11:51 at 1:36 PM) and Slot 3 (11:29 at 1:29 PM, then 11:43 at 1:34 PM), so saves update the same slot. Not confirmed: that the plain Save button wrote without opening the panel.
- 3 PARTIAL: Slot 1 reads "Slot 1", blank in-game and difficulty columns (no slotinfo.json yet) and saved-at Oct 17, 1:00 PM in all three panel shots; loading it did not change that time. The main menu (Continue and Load Game buttons) was not shown.
- 4 / 12 PARTIAL (supported): screenshot 4 shows Slot 1 marked "current" with the game at Summer 9, 10:49, 79F (the Cold Front 79F moment from the earlier Weather Station tests), consistent with slot 1 having been loaded. Not confirmed by Mike, and the pause-menu "Unsaved progress will be lost" warning was not shown.
- 14 PARTIAL: loading a manual slot made it "current" (Slot 1 in screenshot 4). Loading the Autosave (no current slot, next Save opens the panel) not yet seen.
- 10 PARTIAL: the Autosave row ("A") appears only in Load mode and has Load and Copy but no Delete; it is not listed in the Save-mode panel, so it cannot be saved to. Still to check: that it stays unchanged by manual saves.
- 15 PARTIAL: the copy worked ("Copied Slot 1 to Slot 4."); Slot 4 shows the target's name, blank in-game and difficulty columns (its source has no slotinfo) and saved-at Oct 17, 1:00 PM, the source's time, as Claude Code described. Not yet seen: loading the copy gives the same game, and the overwrite confirmation on an occupied target.
- Autosave row: "Summer 9, Year 9, 11:51, Pioneer, Oct 17, 1:36 PM" is identical to Slot 2's row. That fits a Main Menu autosave right after saving Slot 2 (item 9b), but it cannot be told from a copy of a manual save; items 8 and 9 still need a clean test (note Slot 2's saved-at, sleep or F5, check Slot 2 did not change and the Autosave row did).
- Still OPEN: 1-2 (setup, assumed done), 8, 9, 11 (newest-wins both ways), 13 (no state leaking between games), 16, 17, 18-20 (New Game), 21 (Esc), 22 (failed load, throwaway copy only), 23 (Console), 24, 25 (restore Saves folder).

### 2026-10-17 - backup (Mike: "i copied the folder to a safe location")
- 1 DONE (late): Mike copied the whole Saves folder to a safe location. Note the copy was made AFTER testing began, so it holds the current test state (slot1 already re-saved at about 1:41 PM, slot2 to slot4 and autosave), not the original 1:00 PM playtest save. It is a safe restore point for the rest of this checklist (item 25), not for the pre-test slot1.

### 2026-10-17 - Load Game panel with a new Homesteader game in Slot 6 (Mike, no comment)
Screenshot (in game, Load tab): Slot 1 Summer 9 Year 9 12:52 Pioneer (saved 1:51 PM); Slot 2 Summer 9 Year 9 15:04 Pioneer (1:55 PM); Slot 3 Summer 9 Year 9 11:43 Pioneer (1:34 PM, unchanged); Slot 4 blank (1:00 PM, the copy, unchanged); Slot 5 Empty; Slot 6 "current", Spring 1 Year 1 08:46 Homesteader (1:59 PM); Slots 7 to 10 Empty; Autosave Summer 9 Year 9 15:00 Pioneer (1:54 PM), Load and Copy only. HUD: minimap 08:46, 44F; an orange "Hot" stage word above the Health bar.
- 18 PASS (supported): a new Homesteader game exists in Slot 6, an empty slot, and every other slot and the Autosave row are unchanged by starting it (Slot 3 and Slot 4 identical to before, Autosave still the Pioneer Summer 9 15:00 save). Mike did not describe the steps.
- 14 PARTIAL: Slot 6 is marked "current" (a manual slot becomes current).
- 8 PARTIAL (supported): the Autosave row (15:00, 1:54 PM) differs from the manual saves of the same game (Slot 2 at 15:04, 1:55 PM), so manual saves did not overwrite the Autosave and the Autosave did not overwrite them. A clean sleep/F5 test is still the proper check.
- Slot 1 has been re-saved again (12:52 at 1:51 PM); the by-hand backup is the only copy of earlier states.
- 13 POSSIBLE FAIL, NEEDS A LOOK: the HUD shows an orange "Hot" heat-stage word in a brand-new Spring game at 08:46 and 44F, where no heat stage should show. The previous game was a Pioneer Summer 9 game in hot weather. If the Pioneer game's core body temperature carried into the new game, the New Game reset list misses it (this could also be old behaviour on any New Game from a running session, not caused by the slot work). Not confirmed: the screenshot does not show the Body meter or F3 readout.
- Still OPEN: 1-2 (setup, assumed), 9, 11, 12 (pause load warning, main menu load), 13 (rest of the state checks), 16, 17, 19, 20, 21, 22, 23, 24, 25.

### 2026-10-17 - "Hot" label follow-up (Mike: "i did sleep for 3 hours"; Hot "was never there until the screenshot")
- Mike slept 3 hours in the Slot 6 (new Homesteader) game, and the "Hot" word was NOT showing at the start of the new game; it appeared later on its own.
- So it is NOT a carried-over core temperature from the earlier Pioneer game (a leak would show Hot straight away). Claude's "possible fail" note on item 13 above is WITHDRAWN as a state-leak finding. Item 13 goes back to OPEN (the real leak test, loading one game from inside another and comparing inventory, money, difficulty, weather, time and Heat Wave state, has not been done).
- The Hot word itself is unexplained and probably unrelated to save slots: 44F air with a heat stage showing after a 3 hour sleep is unexpected, but it may come from the sleep setup (for example a Sleeping Bag or Tent, a fire or hearth, heavy clothing) or from a separate body-temperature bug. Not investigated. If it recurs in a clean game, check the F3 body readout (core temperature and what is adding heat).

### 2026-10-17 - pause-menu Load warning (Mike: "all loads and saves seem to be clean")
- Screenshot: Load Game panel from the pause menu with a confirm box "Load Slot 2? Unsaved progress in the current game will be lost." with Yes and No. Slot 6 is "current"; the Autosave row is Load and Copy only.
- 12 PASS: the pause-menu load warning appears and names the slot. Mike reports loads and saves "seem to be clean" without per-item detail, so which games he loaded and whether state carried between them was not stated.
- 13 PARTIAL (supported by Mike's summary, not itemised): no leaks reported. A specific check of difficulty, money, inventory, weather, time and Heat Wave state after a cross-game load is still worth one confirmation.
- Still OPEN: 1-2 (assumed done), 8, 9 (clean sleep or F5 test), 11 (Continue newest-wins both ways), 16, 17 (Delete confirmation), 19, 20, 21 (Esc closes without unpausing), 22 (throwaway copy only), 23 (Console), 24, 25 (restore the Saves folder).

## I. Save Game button label "Save to Slot N" (written 2026-10-17 by Claude Code, compiled clean 0 errors / 0 warnings, not play-tested, uncommitted)
Change: PauseMenuController.cs only. RefreshSaveLabel() sets the Save Game button text from SaveManager.Instance.ActiveSlot every time the pause menu opens, and when the slot panel closes. Label uses the slot NUMBER only ("Save to Slot 2"). No active slot reads "Save Game..." and still opens the panel. Behaviour is unchanged (Save Game writes straight to the active slot; Save / Load... always opens the panel).
26. With an active slot, the pause menu reads "Save to Slot N" for that N; clicking it writes straight to that slot, no panel.
27. Load another slot, or start a New Game on a different slot, then pause: the label shows the new number.
28. Delete the current slot in the panel and close it: label reads "Save Game..." (no active slot). A Copy leaves the label on the same slot.
29. Load the Autosave: label reads "Save Game..."; clicking it opens the panel in Save mode.
30. Save to a slot from the panel and close it: label updates to that slot.
31. Save / Load... still always opens the panel; Console clean.

### 2026-10-17 - Mike's pass/fail run (all PASS)
Mike reported PASS, one word per item, no notes, for: 6, 7, 8, 9, 11, 13, 14, 15, 16, 17, 19, 20, 21, 22, 23, 25, 26, 27, 28, 29, 30, 31.
- Combined with earlier results (1 done, 2 assumed, 3, 4, 5, 10, 12, 18 passed or supported earlier), every item is now PASS except **24 (optional, empty Saves folder), which Mike did not report and stays OPEN/skipped**. Items 3, 4, 5, 10, 18 were earlier PARTIAL/supported and Mike's later items (11, 12-17, 19-23) exercise them, but he did not re-report them individually.
- Items 22 and 25 were run as reported (22 on a throwaway copy of the Saves folder per the checklist; 25 means the backed-up Saves folder was put back).
- Item 23: Console clean throughout, as reported.
- Section I (Save Game label "Save to Slot N", items 26-31): all PASS.
- Verdict: save-slot step 1 is PLAY-TESTED and ready to commit (still uncommitted, together with the label tweak). Step 2 (whole-slot atomic write) is NOT built.
