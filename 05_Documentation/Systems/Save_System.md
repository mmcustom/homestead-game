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
