# Homestead
## Review 004 — Unity Architecture v1.0

Status: Complete

Reviewer: Claude

Date: 2026-09-22

---

# Purpose

Copilot reported (via Mike) that the initial Unity project architecture is implemented and ready for review: Unity 6000.3.24f1 LTS, a four-scene structure (Bootstrap, MainMenu, World, Loading), and an eight-manager architecture (GameManager, SaveManager, TimeManager, DiscoveryManager, JournalManager, InventoryManager, WeatherManager, AudioManager), plus a `Unity_Architecture.md` doc. Per AI_Collaboration_Rules.md's Handoff Protocol ("when Copilot produces something Claude should review"), Claude inspected the actual project files on disk against that report and against the existing design documentation.

This review does not touch or edit anything under `01_Unity_Project` — per AI_Collaboration_Rules.md, that folder is Copilot-primary. Findings are reported back for Copilot/Mike to act on.

---

# Scope

Read directly from the device: `01_Unity_Project/Homestead`'s folder structure (Assets, Scenes, Scripts), all four new scene files (Bootstrap.unity, MainMenu.unity, World.unity, Loading.unity), all four Manager scripts staged and inspected (GameManager.cs, SaveManager.cs, TimeManager.cs, WeatherManager.cs — the other four were confirmed present by directory listing but not individually opened, since the four inspected were representative and identical in structure), `ProjectSettings/ProjectVersion.txt`, `ProjectSettings/EditorBuildSettings.asset`, `ProjectSettings/GraphicsSettings.asset`, `Packages/manifest.json`, and a search of the likely doc locations for `Unity_Architecture.md` (`05_Documentation/Unity_Architecture/`, `06_AI_Collaboration/Copilot/`, and the project root).

---

# Confirmed Matching the Report

- Unity Editor version is genuinely 6000.3.24f1 LTS (`ProjectVersion.txt`), matching the GDD's Unity 6 requirement.
- HDRP is installed and set as the active render pipeline (`com.unity.render-pipelines.high-definition: 17.3.0` in `manifest.json`; `GraphicsSettings.asset` has `m_CustomRenderPipeline` pointed at an HDRenderPipeline asset) — matches the GDD's HDRP requirement.
- All four scenes exist as files: `Assets/Scenes/Bootstrap.unity`, `MainMenu.unity`, `World.unity`, `Loading.unity`.
- A `Managers` GameObject exists in Bootstrap.unity with `GameManager.cs` attached (verified by matching the scene's script GUID reference against `GameManager.cs.meta`'s GUID — they match).
- A `Scripts` folder structure exists beyond just Managers: `Core`, `Discovery`, `Inventory`, `Managers`, `Player`, `Saving`, `Survival`, `UI`, `Wildlife`, `World` — this wasn't explicitly called out in Copilot's status update, but it's a good sign the folder layout is already anticipating the documented systems (Survival ↔ Core_Survival_System.md, Discovery ↔ Discovery_System.md, Wildlife ↔ Wildlife_System.md/Hunting_System.md, and so on).
- All eight Manager script files exist on disk under `Assets/Scripts/Managers/`.

---

# Finding 1 — `Unity_Architecture.md` was not found anywhere in the repo

Checked `05_Documentation/Unity_Architecture/` (empty), `06_AI_Collaboration/Copilot/` (empty), and the project root — no file by that name exists. This may be a sync delay (this session has seen OneDrive sync issues on other files this session, though never on a brand-new file before) or the file may not have actually been written yet. **Needs Copilot to confirm the exact path it wrote to, or re-save it**, before Claude can review its content or fold it into `05_Documentation/Unity_Architecture`.

---

# Finding 2 — None of the four new scenes are registered in Build Settings

`ProjectSettings/EditorBuildSettings.asset` still lists only the original HDRP template scene (`Assets/OutdoorsScene.unity`) — Bootstrap, MainMenu, World, and Loading aren't in the build scene list at all yet. Until they're added (File > Build Settings, or `EditorBuildSettingsScene[]`), the game can't actually boot through the intended Bootstrap → MainMenu/World flow, whether in a build or via `Application.LoadScene` by index. This is likely just the next step rather than an oversight, but flagging it since "ready for architecture review" could otherwise be read as "ready to run."

---

# Finding 3 — Bootstrap.unity carries leftover HDRP template content

Bootstrap.unity contains `Sun`, `Sky and Fog Volume`, `Main Camera`, and `StaticLightingSky` GameObjects — the default objects HDRP's New Scene template includes — in addition to the `Managers` GameObject. If Bootstrap is meant to be a lightweight, no-visuals scene that just initializes managers and immediately loads MainMenu or World (the standard pattern for this kind of architecture), these four objects probably belong in World.unity instead, not Bootstrap. If Bootstrap is intentionally meant to render something (a splash/loading visual), this is fine as-is — worth Copilot or Mike confirming which was intended.

---

# Finding 4 — Leftover default template assets at the Assets root

`Assets/OutdoorsScene.unity`, `Assets/Readme.asset`, and `Assets/TutorialInfo/` are all default content that ships with Unity's HDRP template — not part of the planned four-scene architecture. Not a functional problem, but worth a cleanup pass so they don't get mistaken for real project content later, and so `OutdoorsScene.unity` (currently the only scene in Build Settings — see Finding 2) doesn't end up shipping by accident.

---

# Finding 5 — Decisions_Log.md's Unity Version Lock doesn't match the actual project

While adding this review's entry to `Decisions_Log.md`, found that Copilot had already written directly to it with a "Unity Version Lock" entry stating Homestead is locked to **Unity 6000.5.9f1**. The actual installed editor version, per `ProjectVersion.txt` on the project itself, is **6000.3.24f1** — the same version named in Copilot's status report to Mike. Two different Unity versions are now on record for the same project: whichever is correct, the other should be corrected before anyone reads the lock note as current fact. Also noting for consistency only (not urgent): that entry doesn't follow Decisions_Log.md's established format (no attribution tag, different heading style than every other entry) — AI_Collaboration_Rules.md allows either AI to write to 00_Project_Management, so this isn't a rule violation, just worth keeping consistent going forward.

---

# Note — Manager wiring and implementation are still ahead, not a gap

Per Copilot's own report, only `GameManager` is actually attached to the `Managers` GameObject so far — confirmed by inspection. The other seven manager scripts exist as files but aren't wired into any scene yet. All eight scripts (the four inspected directly, consistent with the other four by file size and the fact they were generated together) are still Unity's raw default `MonoBehaviour` template — empty `Start()`/`Update()`, no singleton/static-access pattern, no `DontDestroyOnLoad`. This is expected for a first architecture pass and isn't a finding against the work done so far, just a note that the "eight managers" are currently eight named placeholders, not eight working systems yet.

---

# Note — Manager list vs. documented systems, for Copilot's awareness

The eight managers map cleanly onto existing docs: SaveManager ↔ Save_System.md/Save_Data_Model.md, TimeManager ↔ Season_System.md's day/season/year tracking, DiscoveryManager and JournalManager ↔ Discovery_System.md, InventoryManager ↔ Systems/Inventory_System.md, WeatherManager ↔ Weather_System.md. Two things not covered by a manager, flagged for awareness only, not as something wrong: Core_Survival_System.md's Hunger/Hydration/Health/Morale stats (may belong on the player controller rather than a global manager — a reasonable choice either way) and Save_Data_Model.md's Economy Block (currency, market conditions — may just not be built yet). Not raising these as gaps, just naming them in case Copilot wants Claude's input when that part gets built.

---

# Outcome

Five findings, none blocking: a missing `Unity_Architecture.md` that needs a path confirmed or a re-save, an empty Build Settings scene list, leftover HDRP template content in Bootstrap.unity worth a design call, leftover default template assets worth a cleanup pass, and a Unity version discrepancy between Decisions_Log.md's Version Lock entry (6000.5.9f1) and the actual installed editor (6000.3.24f1) that needs resolving before it causes confusion. Nothing here was edited — all five findings are for Copilot (or Mike, relaying) to act on in `01_Unity_Project`, per AI_Collaboration_Rules.md's file ownership.
