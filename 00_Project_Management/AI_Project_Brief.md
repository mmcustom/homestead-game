# Homestead
## AI Project Brief v2.0

Status: Active. Rewritten 2026-11-08 (v1.0 dated 2026-09-22 was out of date: it named Copilot, called the game a survival simulator and listed finished folders as not started).

---

# Purpose

Orientation for whichever AI or developer picks up this project, so a new session does not have to re-derive context from scratch. A human developer should start with `00_Project_Management/Project_Overview_for_Developers.md`, which has the full picture; this brief is the short version for AI sessions.

---

# What This Project Is

Homestead is a realistic first-person homesteading life simulator with a tycoon spine, built in Unity 6 HDRP (6000.3.24f1), modeled in Blender, tracked in Git. It is not a survival game or a farming sim: the player buys raw land with a loan, survives and develops it from primitive to modern, pays off the debt, and profits from the land. Three missed loan payments open at once ends the game.

Full vision and pillars: `00_Project_Management/GDD/Homestead_GDD_v2.0.md` (updated to v2.1 on 2026-11-08; see `GDD/GDD_v2.1_Proposed_Changes.md` for what changed; where the GDD and the roadmap disagree, the roadmap and the system docs are more current).

Current status: pre-release, solo project with AI assistants. The core loop was proven fun on 2026-10-11 and the game has been extended heavily since (money and debt, production chains, structures, hauling, hands and melee). Not packaged. The picture of what is built and what is next is the 2026-11-07 status section at the end of `00_Project_Management/Development_Roadmap.md`.

---

# Where To Look

## Start here

1. `00_Project_Management/Project_Overview_for_Developers.md` (scope, status, setup, architecture, risks)
2. `00_Project_Management/Development_Roadmap.md` (end of file: current status and next-step candidates)
3. `00_Project_Management/GDD/Homestead_GDD_v2.0.md` (vision, pillars, loan rules)

## Gameplay system design

`05_Documentation/Game_Systems/`: about 45 docs covering survival, water, weather, food, money and the Trading Post, production chains (Ash, Maple, Potash, Pottery, Metal, Workshop), structures, hauling, hands and melee, art lists and audio. Most carry a dated status or UPDATE line at the top; trust the newest dated line. `Art_Needed_List.md` (v0.3) is the full art list and tracker.

## Engineering-layer systems

`05_Documentation/Systems/` (Inventory, Item Data, Save System, First Person Controller) and `05_Documentation/Unity_Architecture/Unity_Architecture.md` (v2.0, current: scenes, managers, assemblies, saves, art plumbing, tests, technical debt).

## Other folders

`Gameplay_Loops`, `Research`, `Wildlife`, `Plants`, `Livestock`, `Fishing` hold loop write-ups, research and early species notes; `Livestock` and `Plants` are still thin because those systems are not built.

## Unity implementation

`01_Unity_Project/Homestead`. Claude Code owns it. Claude does not modify it without being asked.

## Collaboration rules, task log, decisions

- `06_AI_Collaboration/AI_Collaboration_Rules.md`: roles, handoff protocol, file ownership.
- `06_AI_Collaboration/Current_Task_List.md`: append-only history of everything done. It is very large; search it rather than reading it top to bottom.
- `06_AI_Collaboration/*_Checklist.md`: one playtest checklist per feature.
- `06_AI_Collaboration/Measurements/` and `Income_Measuring_Plan.md`: income measuring for Money Phase D2.
- `00_Project_Management/Decisions_Log.md`: chronological record of decisions (being back-filled from the task log).

---

# Current Collaboration Model

- **Claude** (Claude in Cowork): design documentation, folder organisation, cross-document consistency, playtest checklists, Blender and ComfyUI work. Cannot compile or run Unity.
- **Claude Code** (separate command-line tool, live Unity Editor via the Unity MCP): C# and Unity Editor work, tests, anything needing the compiler or Play Mode.
- **Mike**: design lead and final decision-maker; playtests everything; carries messages between the two (there is no direct channel).

GitHub Copilot was the original implementation partner and was replaced by Claude Code on 2026-09-23. See `AI_Collaboration_Rules.md` v2.0 and the Decisions Log.

---

# Ground Rules Worth Repeating

- Do not redesign what is already decided. Identify gaps and inconsistencies; ask Mike before changing a rule, a price or a difficulty number.
- Check the system doc, `Decisions_Log.md`, `Development_Roadmap.md` and the task log before asking Mike to repeat himself.
- Scope is set by Mike through the roadmap's next-step choices; the GDD's Alpha 0.1 lock was retired on 2026-11-08. Do not start a big system (farming, livestock, power, vehicles beyond the hand cart) unless Mike has chosen it.
- Commit code and docs separately. Never commit `Captures/`, `Art/`, `Blender/`, `Claude outputs/`, `08_Backups/` or the generated `Homestead.slnx`.
- Docs are Markdown with LF line endings; put a dated UPDATE line above any stale Status line instead of silently rewriting.
