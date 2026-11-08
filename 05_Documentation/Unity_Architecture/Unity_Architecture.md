# Homestead
## Unity Architecture v2.0

Status: Current as of 2026-11-08 (verified against the project, git and a full EditMode test run). Supersedes v1.0 (2026-09-23), which described 4 scenes, 8 managers and 6 save files.

---

# Purpose

Unity project conventions for Homestead: scenes, managers, assemblies, saves, art plumbing, tests, and known technical debt. The onboarding overview for new developers is `00_Project_Management/Project_Overview_for_Developers.md`; this file is the technical reference and carries the same architecture facts.

---

# Engine

Unity 6.3 LTS, version 6000.3.24f1, HDRP 17.3.0, new Input System 1.20.0. Windows target.

---
# Scenes

- **Bootstrap:** a `Managers` object with 24 manager components; `GameManager.Start` then loads MainMenu.
- **MainMenu:** `MainMenuController` plus UI.
- **Loading:** no scripts; `GameManager.LoadRoutine` shows it, then async-loads World.
- **World:** the property. **47 MB of text YAML in normal git (not LFS).** See the risks in section 14.

# Managers

About 30 MonoBehaviour singletons in `Scripts/Managers`, each reached through a static `Instance`, marked `DontDestroyOnLoad`, with duplicates destroying themselves in `Awake`. `Start` is used for cross-manager wiring because every `Awake` has run by then. 24 are placed in Bootstrap; GroundWood, Pond, Stream, Tap, Wells and CreekCrossing create themselves with `RuntimeInitializeOnLoadMethod`; Ledger, Money and Market also have a `CreateIfMissing` fallback.

| Group | Managers (role) |
|---|---|
| Flow and persistence | GameManager (state machine: Booting, MainMenu, Loading, Playing, Paused, Menu, Lost; scene loads; new game), SaveManager (slots, JSON), DifficultyManager (tier and starting kit) |
| Clock and environment | TimeManager (clock, calendar, seasons), WeatherManager (weather, heat waves), StreamManager (creek stage, floods), PondManager, CreekCrossing, AudioManager |
| Player state | SurvivalManager (health, hydration, hunger, warmth), SleepManager, InventoryManager (pack, storage, equipment), JournalManager (journal and Know-how), DiscoveryManager, MapManager (fog of war) |
| Resources and world | FireManager (campfires, fuel), WoodManager (trees, piles, every placed structure), WellsAndOuthouses (wells, outhouses, kilns, workshops and other structures), StoneManager, GrassManager, GroundWoodManager, ForagingManager, TapManager (maple taps) |
| Food and wildlife | FishingManager, TrapManager, WildlifeManager |
| Economy | TradingPost (cash and trading), MarketManager (simulated prices), MoneyManager (land loan), LedgerManager (seasonal ledger) |

`MarketData` (a ScriptableObject) and `WoodMoisture` (a static helper) live in the same folder but are not managers.

# Assemblies, namespaces and folders

- Four asmdefs: **Homestead.Economy** (44 files, `noEngineReferences`, so it cannot use UnityEngine), **Homestead.SlotSwap** (also no engine), **Homestead.CreekCrossing** and **Homestead.Setbacks** (engine allowed). Editor code (`Scripts/Editor`) compiles to `Assembly-CSharp-Editor` with no asmdef.
- **There are no namespaces.** Every type is in the global namespace. "Economy" is an assembly, not a namespace.
- Files under `Assets/Scripts` (199 `.cs` including 11 Editor files): Economy 44, World 33, Managers 32, UI 32, Player 13, Survival 13, Editor 11, Inventory 6, Wildlife 4, Saving 3, Debug 2, Discovery 2, Foraging 2, Core 1, Fishing 1. Tests: 66 files in `Assets/Tests/EditMode`.

# Rules versus behaviour

Engine-free logic goes in `*Rules` and `*State` classes (for example `LoanState`, `Ledger`, `CarryRules`, `MapleRules`, `TierRules`, `LossRules`, `PunchRules`, `KnifeRules`), mostly in the Economy folder. MonoBehaviour managers hold the scene side and the save, and call into those. Tests prefer the pure classes.

# Saves

- Location: `<persistentDataPath>/Saves/<slot>/`. Slots `slot1` to `slot10` (manual) plus one separate autosave; `Saves/slots.json` records the active slot.
- Files per slot: `player.json`, `world.json`, `inventory.json`, `journal.json`, `discovery.json`, `meta.json`, `slotinfo.json`. Each system implements `ISaveable` (SaveFile, SaveKey, CaptureState, RestoreState) and registers with SaveManager.
- Format: `JsonUtility` JSON, `{sections:[{key, data}]}` where `data` is itself a JSON string. State is captured on the main thread and written on a worker thread through SlotSwap, with crash recovery at startup (`SlotSwap.RecoverAll`).
- Autosave: end of day, sleep, returning to the main menu and quit; it writes only the autosave slot. A lost game is never saved.
- Versioning: `SaveVersion = 1` in `meta.json`. A mismatch only logs a warning. **There is no migration code.**
- Adding fields safely: `JsonUtility` ignores unknown fields and leaves missing ones at defaults; a system with no section in an old save keeps its fresh state; **enums are only ever appended** (they save as integers); the journal saves ids, not text. New sections are called "additive".

# Art plumbing

- **ArtSwap** (`Scripts/World/ArtSwap.cs`) is the one swap point. A prefab at `Resources/Art/<id>.prefab` (loaded with `Resources.Load`, cached) replaces the primitive; with no prefab the primitive is drawn unchanged.
- Entry points: `StructureVisuals.Build` (structures), `PlayerHands.BuildRig` (hands), `PlayerHands.ShowHeld` (tools).
- Naming: structures `structure_<pilekind>` (only `structure_kiln` has a spec so far); hands `hands_player` and `hands_player_grip`; tools `tool_<itemid>`; FBX files `<id>_model.fbx` (for example `tool_axe_model.fbx`).
- Structure prefab rules: a child `Body` stays on; children named `State_*` show one at a time; `On_*` children show while a flag is true; `Solid_*` children get box colliders; all other colliders in the prefab are removed; anchors are named children such as `Anchor_Smoke` (a missing anchor is created at the code's default position).
- Currently in `Resources/Art` (52 tracked files): the hands, the grip hands and an axe. Editor menu: Homestead > Art > Create/Remove Test ... Prefab.

# Conventions and gotchas

- Put tunables on the component, never in nested serialized classes (a past problem with `BodyHeat.Settings`).
- Tests: NUnit, one class per feature (`XxxTests.cs`). Long explanatory comments at the top of files reference the design docs.
- Serialized enums: append only.
- New `Homestead/...` menu items must be mirrored in the tests that grep for them.
- Play Mode driven over the Unity MCP needs `Application.runInBackground = true`.
- Back up the playtest save (`slot1`) before running Play Mode tests.

---

# Packages

 `Packages/manifest.json` and `packages-lock.json` are tracked. HDRP, Input System, uGUI, Timeline, Visual Scripting and the development feature set are standard. Two to know about:

- `com.coplaydev.unity-mcp` is a git URL (`CoplayDev/unity-mcp ... #main`). It is **editor tooling only** (nothing under `Assets/` references it) but it is still a hard dependency of the manifest, it follows a moving `#main` branch, and the lock file pins a commit. It is how the AI assistant drives the Editor.
- `com.unity.pipeline` is 0.7.0-exp.1, an experimental package.


---

# Tests

- Run: Window > General > Test Runner > EditMode. Command line (Unity's standard form, not yet run from the CLI here): `Unity.exe -batchmode -projectPath <path> -runTests -testPlatform EditMode -testResults <file.xml>`.
- **Result on 2026-11-08: 1,900 run, 1,900 passed, 0 failed, 0 skipped, 255 seconds.** (1,435 `[Test]` plus 465 `[TestCase]`.) There is no PlayMode test folder.
- One test assembly, `Homestead.Tests.EditMode` (Editor-only, references NUnit plus the four asmdefs below). The main game code is in the default `Assembly-CSharp`, which an asmdef cannot reference, so tests reach it by **reflection** (a `Find(name)` helper scans loaded assemblies for a type by name). Pure rule classes in the four asmdefs are tested directly. Some tests read source files as text and assert on them (for example that menu items exist).
- Tests must never touch the real `Saves` folder.


---

# Technical debt and known risks

An honest list from the code side, 2026-11-08.

1. **World scene weight.** `World.unity` is 47 MB of text with 13,449 GameObjects, 12,646 MonoBehaviours, about 12,600 colliders (11,915 box, 516 sphere, 200 capsule, 9 mesh). Edits produce huge, hard-to-merge diffs, so **only one person can reliably edit the scene at a time.** A performance pass has not been done.
2. **Oversized classes:** PlacedStructure (2,769 lines), WoodManager (2,427), WellsAndOuthouses (1,818), PropertyTerrainBuilder (1,169), WoodPile (1,099), SurvivalManager (1,039), CraftingScreen (1,009), TimeDebugControls (775). WoodManager and WellsAndOuthouses grew into catch-alls for every structure type, rules included.
3. **No namespaces and one big game assembly.** Every script edit recompiles all of Assembly-CSharp; name clashes are possible; tests need reflection for most game code.
4. **Manager pattern is bypassed in places:** 63 `FindAnyObjectByType` / `FindFirstObjectByType` / `FindObjectsByType` calls in game code, a mutable public static (`TapManager.DebugForceFlow`), `ArtSwap.Loader` and its static cache (tests reset it by hand), managers reaching into each other's `Instance` with null checks. `GameObject.Find` appears only in Editor tools.
5. **Save versioning has no migrations.** It relies on additive changes only.
6. **Debug keys are live in development builds** and some are destructive. A packaged build must not include them.
7. **Packages:** unity-mcp follows a floating `#main` URL and needs git on the PATH; `com.unity.pipeline` is experimental.
8. **`Homestead.slnx` is tracked but should not be.** `*.sln` and `*.csproj` are ignored but `*.slnx` is not, so the generated file always shows as modified. It should be added to `.gitignore` and removed from the index (a decision for Mike).
9. **Stale notes about art in git.** The `ArtFolderGuard.cs` comment says art is "never committed from the playtest machine", and an old note says the presence of `Resources/Art` fails about 8 tests. `Resources/Art` is tracked (52 files) and the suite passes 1,900 of 1,900 with it present, so both are out of date.
10. **Not checked:** an actual clean clone compile, the Windows build target and build settings, and whether Library cache or other modules are needed.
