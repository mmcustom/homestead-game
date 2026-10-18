# Homestead
## Save System v1.0

Status: Design Draft

---

# Purpose

The Save System preserves player progress between sessions.

Homestead is built around long-term development across seasons and years — nothing else in the game matters if progress can be lost.

---

# Core Philosophy

The player should never lose meaningful progress to a crash, accidental quit, or forgetting to manually save.

Saving should be reliable before it is convenient.

---

# What Must Be Saved

## Player State

- Position
- Hydration, Hunger, Fatigue, Health, Morale
- Inventory contents
- Equipped tools

---

## World State

- Current Day, Season, Year (Season System documentation)
- Weather state (Weather System documentation)
- Discovered locations (Discovery System documentation)
- Journal entries (Discovery System documentation)
- Minimap reveal state

---

## Property State

- Constructed buildings (Building and Housing System documentation)
- Livestock — count, health, feed reserves (Livestock System documentation)
- Placed traps and their status (Trapping System documentation)
- Stored resources — Home Storage (Inventory System documentation)

---

## Economy State

- Currency
- Active market conditions, if any

---

# Save Triggers

## Manual Save

Player-initiated at any time.

---

## Auto-Save

Triggered at meaningful checkpoints.

Examples:

- Sleeping
- Entering or exiting a building
- End of day transition

Auto-save should never interrupt gameplay with a noticeable hitch, if avoidable.

---

# Save Slots

Alpha 0.1 Scope (original):

A single save slot was sufficient for Alpha testing. As built, SaveManager uses one slot named slot1 (const SlotName, folder Saves/slot1 under persistentDataPath) and autosaves overwrite it. CORRECTION 2026-10-17 (Claude Code read the code; my earlier line "New Game deletes the save folder" was wrong): New Game never deletes anything and DeleteSave() has no callers. The real overwrite is the first autosave, or the quit or return-to-menu save, of the new game; MainMenuController.OnNewGame asks for confirmation when a save exists.

Current save behaviour (from the code, 2026-10-17): SaveGame() captures every registered ISaveable on the main thread into JSON strings, one file per SaveFile (player, inventory, world, journal, discovery, livestock and so on), plus meta.json {version=1, savedAtUtc}. A worker thread writes each file atomically (.tmp then replace), writes meta.json last, and deletes any *.json in the folder that this save did not produce. LoadGame() returns false if meta.json is missing (that is also the definition of HasSave). Atomicity is per file, not per slot, so a crash mid-save could leave a new player.json beside an old world.json. Autosave triggers: TimeManager.DayChanged ("End of day", also fires on the F5 debug skip), SleepManager ("sleep"), GameManager.ReturnToMainMenu(), SaveManager.OnApplicationQuit (synchronous; stopping Play Mode in the Editor triggers it too), and the pause menu's Save button. The "building entry/exit" checkpoint is not wired. Only GameManager.ContinueGame() calls load; it resets only difficulty, money and portable light, so it is safe from a fresh launch but not as a hot-swap between slots. Playtime is not tracked.

## Multiple save slots - REQUESTED 2026-10-17 (Mike), real in-game slots chosen (not debug-only)

Why: testing. Every Play Mode test that saves or loads overwrites the one playtest save, so Mike has to back up and restore Slot1 by hand (hash-verified) before and after each checklist.

First-pass design (Claude, not yet confirmed; Mike to approve before build):

1. **Ten manual slots plus one separate Autosave slot.** Autosave writes only to the Autosave slot, so it can never overwrite a test save or a hand-made backup.
2. **Existing save is kept.** The current slot1 save becomes Manual Slot 1 as-is (no conversion, no loss). Nothing in the game data format changes, so older saves still load.
3. **Slot info:** each slot is a small separate metadata record (not new fields in the game save): name (default "Slot N", renamable is optional), in-game date and time of day, difficulty tier, and the real-world time it was saved. Playtime only if the game already tracks it. Empty slots show "Empty".
4. **Save/Load screen** reachable from the pause menu (Save and Load), and Load from the title/main menu if one exists. Saving over a used slot asks for confirmation. Deleting a slot asks for confirmation. The game remembers the active slot (last one loaded or saved) and a plain Save writes there.
5. **Copy slot** (slot to another slot, with overwrite confirmation): this replaces the manual Slot1 backup and restore, which is the point of the feature.
6. **New Game must NOT touch other slots.** It starts a fresh game and the player picks an empty slot (or an overwrite after confirmation). (Today New Game deletes nothing, but its first autosave overwrites slot1; see the correction above.)
7. **Rules:** no new fields in the game save; slot metadata only in the per-slot record; a slot is never overwritten or deleted without a confirmation; a failed write must not destroy the old slot (write to a temp file, then replace).
8. **Not in scope:** cloud saves, corruption recovery or rolling backups (still Future), screenshots or thumbnails in the slot list.

APPROVED TO BUILD 2026-10-17 (Mike: "use all recommendations"): Decisions 1 to 6 below are confirmed. Build step 1 = slots, Autosave slot, slot panel (pause menu and main menu), Copy and Delete, full-reset load path, Continue = newest save. Build step 2 (later, separate) = whole-slot atomic write. Build note: the writer's cleanup deletes any *.json in the folder it did not produce, so slotinfo.json must be part of what a save produces or it will be deleted.

BUILD STEP 1 BUILT 2026-10-17 (Claude Code; compiled clean, 0 errors, 0 warnings; PLAY-TESTED by Mike 2026-10-17, all checklist items PASS except optional item 24 not run; UNCOMMITTED; also includes the 2026-10-17 pause-menu "Save to Slot N" label, PauseMenuController.RefreshSaveLabel, number only, "Save Game..." when no slot is active, also play-tested). Files: SaveManager.cs (rewritten around slots), GameManager.cs, PauseMenuController.cs, MainMenuController.cs, PlayerController.cs (one condition so Esc in the slot panel does not also toggle pause), new Scripts/UI/SlotPanel.cs (+ .meta). ManualSlotCount = 10 and the id "autosave" are named constants; every slot id is validated before it reaches a path (slot1 to slot10 and autosave are accepted, slot11 and ../x are rejected), so a hand-edited slots.json cannot point outside Saves. slotinfo.json (name, in-game date and time, year, season, difficulty, savedAtUtc) is captured on the main thread and added to the same file dictionary as meta.json, so the writer's delete-unproduced-json cleanup keeps it; it is written before meta.json, which is still last. Active slot in slots.json (missing or invalid = none); a manual save or loading a manual slot sets it, loading the Autosave clears it. All autosave triggers write only the autosave slot; the pause Save writes the active slot or opens the panel in Save mode. SlotPanel has Save, Load and New Game pick modes with Save/Load/Select, Copy and Delete (confirmations; the Autosave row is load and copy only). Pause menu has a cloned "Save / Load..." button; main menu has a cloned "Load Game" button; New Game goes difficulty then slot pick (the old overwrite prompt is no longer shown). Continue loads the newest save across all slots. Every load runs the one reset list shared with New Game, loads World, then restores; a load from a running game goes to the main menu first with no autosave on the way; a failed load falls back to the menu. Deviations and notes: New Game on an occupied slot makes it active but does not write to it, so the old save stays until the first manual Save (which overwrites without asking again; the confirmation is at the pick); pause-menu Load asks "Unsaved progress will be lost" because it does not autosave first; Copy keeps the source's saved-at time (a copy and its source count as the same age for Continue); the old Delete Save menu item was removed; the main menu's old confirm-panel fields remain in the scene unused; layout not yet seen on screen (slot panel 1500x764). Slot1 is unaffected by reading the code (same folder, same files; without slotinfo.json it shows as "Slot 1" with the date from meta.savedAtUtc; its first slot-aware save adds slotinfo.json and rewrites the files as before). Play Mode checklist: 06_AI_Collaboration/Save_Slots_Step1_Playtest_Checklist.md.

Code-reading answers (Claude Code, 2026-10-17) and the plan they led to (confirmed by Mike 2026-10-17):
- Layout under Saves/: slot1 unchanged (becomes Manual Slot 1), slot2 to slot10, autosave. Each slot folder gets an optional slotinfo.json (name, in-game date and time, difficulty tier, saved-at UTC); slot1 without one shows "Slot 1" and the date from meta.savedAtUtc until its next save. Saves/slots.json remembers the active slot. No new fields in the game sections.
- UI: one code-built slot panel (shared class using UiKit, overlay with its own Canvas like BuildDifficultyPanel), shown over the pause menu and over the main menu. GameScreens tabs only open from Playing, so the pause menu (state Paused) cannot use a tab. The main-menu use gives "Load Game" for free.
- Decision 1: the quit-save and return-to-menu save go to the Autosave slot (they are autosaves in spirit and the main way a test save gets overwritten); the pause-menu Save button writes the active slot.
- Decision 2: Continue loads the newest save across all slots including Autosave; a separate Load Game button lets the player pick any slot.
- Decision 3: the New Game flow picks its slot up front (an empty slot, or an overwrite after confirmation), so there is always an active slot and the first manual Save never needs a picker.
- Decision 4 (build rule): the target slot is captured at save time and passed to the writer thread so switching slots cannot mix files; Copy, Delete and Load call WaitForPendingWrite first.
- Decision 5: whole-slot atomic write (write to a temp folder, then swap) done as a SEPARATE second step after the slots work and are tested, because it changes how existing saves are written. Until then the per-file atomic write stays.
- Decision 6: loading a slot from the pause menu goes through the main-menu load path with the same manager reset list StartNewGame uses (older saves missing a section must not inherit the previous game's state); ContinueGame uses that full reset too.

### Step 2: whole-slot atomic write - READ-ONLY REPORT RECEIVED 2026-10-18 (Claude Code), PLAN CONFIRMED BY MIKE 2026-10-18 ("use all five"), BUILT 2026-10-18 (compiles clean, 49 EditMode tests pass, PLAY-TESTED by Mike 2026-10-18 with all 13 checklist items PASS, UNCOMMITTED)
**Current write (step 1, per-file atomic only):** SaveGame(slotId) validates the id, queues if a save is running, and CaptureFiles serializes every ISaveable to in-memory JSON strings plus meta (version, UTC time) and slotinfo. A worker thread (Task.Run WriteFiles) creates the folder, writes each section file with WriteAtomic (write <file>.tmp, then File.Replace, or File.Move for a new file; dictionary order), writes meta.json last, then deletes every *.json in the folder not in this save's file set. The main thread then does a plain File.WriteAllText of slots.json (manual saves only) and fires SaveCompleted. SlotExists means "meta.json exists". Each file is atomic; the SET of files is not.
**Where a slot can end up mixed (found by the read):** (a) a crash or power cut mid-OVERWRITE leaves some new and some old section files under the OLD meta.json, so it looks complete and loads as a mix (meta-last only protects a FIRST save to an empty slot); WriteAllText does not fsync, so a power cut can also leave a file empty or short; (b) a crash after the new meta but before cleanup leaves stale section files, and LoadGame restores any registered saveable's file that exists, so a stale file can overwrite fresh state; (c) DeleteSave (recursive delete) can leave a "ghost slot" with meta plus a few files; (d) CopySlot deletes the target then moves the temp folder in, so a crash between the two leaves the target empty after the player confirmed replacing it (the source is safe); (e) all autosave triggers (end of day, sleep, ReturnToMainMenu, OnApplicationQuit) share the exposure but only hit the autosave slot; a force-kill during the quit write is case (a); (f) slots.json is a plain WriteAllText (a torn file reads as "no active slot", harmless); (g) separate existing race: WaitForPendingWrite waits only for the current task, so a queued save can start right after a Delete or Copy returns and recreate the slot. Leftover *.json.tmp files do not match the *.json cleanup and are never removed.
**Proposed swap (same volume, Windows; sibling folders under Saves/: <id>, <id>.tmp, <id>.old, <id>.del):** on the worker thread: (1) recover this id; (2) create a fresh <id>.tmp, write all files into it (plain writes, flush each with FileStream.Flush(true), meta.json last); (3) if <id> exists rename it to <id>.old; (4) rename <id>.tmp to <id>; (5) delete <id>.old (best effort). After a crash: during step 2 or between 2 and 3, <id> is the whole OLD slot and the .tmp is discarded (that one new save is lost); between 3 and 4, <id> is missing, <id>.old is the old slot: restore it and discard the .tmp; between 4 and 5, <id> is the NEW slot and the .old is deleted. **Recovery** runs on the main thread in SaveManager.Awake before ReadActiveSlot and before anything calls HasSave: if <id> exists delete <id>.tmp, .old and .del; if <id> is missing and <id>.old exists rename it back and delete the .tmp; otherwise delete the .tmp. A leftover .tmp is always discarded, never promoted; a .old with no slot is always restored. The swap helper also recovers at the start of every save. The cleanup-of-unproduced-files step goes away (every save starts from an empty folder). **Delete** = rename <id> to <id>.del, then delete it (recovery deletes a leftover .del; a leftover .old is restored instead, hence the separate suffix). **Copy** = build in <target>.tmp and swap in with the same helper, so the target is never briefly empty.
**Safety and compatibility:** works for slot1 to slot10 and autosave (slot1.tmp and slot10.tmp cannot collide). Existing slot1 needs no migration: the first swap renames it to slot1.old and replaces it (stray *.json.tmp files go with it). Names are always SlotFolder(id) + a constant suffix, id passes the IsValidSlot whitelist, and the recovery scan only acts on names ending .tmp, .old or .del whose base passes IsValidSlot (everything else, including .copy_tmp until Copy is migrated, is ignored). AllSlotIds() is a fixed whitelist, so a .tmp, .old or .del folder can never appear as a slot or be picked by NewestSlot. Saves live in AppData\LocalLow (OneDrive is not involved).
**Gaps to close in the build:** between steps 3 and 4 <id> briefly does not exist, and SlotExists, GetSlotInfo and NewestSlot do not wait for a write (after ReturnToMainMenu fires an autosave a write is still running on the main menu), so make those three call WaitForPendingWrite() first; write slots.json atomically too; fix the queued-save race after Delete or Copy in the same pass. **Rename failures** (Defender scanning fresh files, an Explorer window or terminal inside the folder, a backup or indexer tool): retry each rename about 5 times with 100 to 200 ms gaps on the worker thread; if step 3 fails the old slot is untouched, the .tmp is discarded and SaveFailed fires (existing message); if step 4 fails try to roll .old back at once, otherwise leave it for recovery at next start; on the quit path nothing is visible, only an error is logged.
**Testing:** put the swap and recovery in a small static class (new Scripts/Saving/SlotSwap.cs) that takes a root folder path, so tests run on a fresh temp folder and never touch the real Saves path; an optional Action<string> crashPoint callback (null in the game) throws SimulatedCrash at AfterTempWritten, AfterOldRenamed, AfterNewRenamed and mid-temp-write, skipping the swap's own error handling so it simulates a power cut; EditMode tests assert the slot is exactly whole-old or whole-new at each point, plus a first save, slot1 with stray files, a locked-rename failure and a hostile id. No Play Mode needed; the hook is not wired to any button. Needs an EditMode test assembly (Claude Code has not checked whether one exists).
**Risks:** fsync is needed for power-cut safety (today's writes lack it too: an improvement but a real change in write behaviour); realistic failure mode is a rename blocked by antivirus or an open folder; recovery runs in Awake before everything else, so a bug could hide a slot (hence the tests); a future slot-RENAME feature must read the old slot's name before the swap (BuildSlotInfo always writes the default name today). **Files expected:** SaveManager.cs, new SlotSwap.cs, a new EditMode test file (plus assembly if none), Save_System.md, Current_Task_List.md; PauseMenuController and SlotPanel should not change.
**Open questions for Mike (Claude Code's recommendations in brackets):** (1) if the swap fails after retries, fail visibly with the old slot untouched [yes; the alternative, falling back to the old per-file write, can leave a mixed slot]; (2) discard a complete leftover .tmp [yes, costs at most one save]; (3) route Delete and Copy through the swap in this step [yes]; (4) test with EditMode tests on a temp folder rather than an in-game debug switch [yes]; (5) keep the .old as a one-save-back backup instead of deleting it [no for now; rolling backups are a listed future item]. Mike answered "use all five" 2026-10-18.
**STEP 2 BUILT 2026-10-18 (Claude Code; compiled 0 errors / 0 warnings; 49 EditMode tests pass, 0 failed, 0 skipped; Play Mode NOT entered; UNCOMMITTED; ProjectSettings.asset and the AudioManager.cs null guard untouched).** Files: new Assets/Scripts/Saving/SlotSwap/SlotSwap.cs (static class: save swap, copy swap, delete swap, recovery) and Homestead.SlotSwap.asmdef (own assembly, no engine references, because the game has no asmdefs and Assembly-CSharp cannot be referenced by a test assembly); SaveManager.cs changed to use SlotSwap; new Assets/Tests/EditMode/Homestead.Tests.EditMode.asmdef (editor-only, defineConstraints UNITY_INCLUDE_TESTS, not auto-referenced, cannot reach a build) and SlotSwapTests.cs; Unity created the .meta files and regenerated Homestead.slnx on its own. The Unity Test Framework package was already present (transitive); no packages installed. Tests (all on a fresh temp folder, never the real Saves path): all four crash points on an overwrite and a first save, all 11 slot ids, slot1 with step-1 leftovers and no migration, two locked-rename tests (old-slot rename blocked; temp-into-place blocked with rollback), 14 hostile ids (.., C:\Windows, /etc, slot11, SLOT1, empty id and more), the recovery scan only acting on valid names with the three suffixes, Delete including a crash after the .del rename, Copy onto an occupied target with a crash at each point and the source untouched, atomic slots.json. **Deviations from the plan:** SlotSwap lives in its own asmdef subfolder; recovery ALWAYS deletes a leftover .del, even when the slot folder is missing (a crash after the delete rename leaves exactly that state); start-up recovery also removes step 1's old .copy_tmp folder; the slot-id whitelist and the slot-count and autosave constants moved into SlotSwap (SaveManager points at them, one definition); Delete and Copy first finish the write in flight and then run any queued saves on the main thread, so a queued save cannot recreate the slot, and after a write a manual save only becomes the active slot if the slot still exists; rename retry count and delay are public statics (5 attempts, 150 ms; tests set them lower). Checklist: 06_AI_Collaboration/Save_Slots_Step2_Playtest_Checklist.md.
- Slot rename is NOT in the first build (name defaults to "Slot N").

---

# Corruption and Failure Handling

Future System

Not Alpha 0.1.

Should eventually include a backup or rolling save to protect against a corrupted save file.

---

# Design Rules

1. Never lose meaningful player progress silently.

2. Auto-save at natural checkpoints, not just on a raw timer.

3. Save data should be organized so that adding a new system later doesn't require restructuring the whole save file.

4. Save and load should be invisible to the player when working correctly.

5. Nothing here should be added that isn't required to support the Alpha 0.1 systems already in scope.
