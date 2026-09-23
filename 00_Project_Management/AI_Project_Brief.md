# Homestead
## AI Project Brief v1.0

Status: Active

---

# Purpose

Orientation for whichever AI picks up this project — Claude or Copilot — so a new session doesn't have to re-derive context from scratch.

---

# What This Project Is

Homestead is a realistic first-person homesteading survival simulator: water, shelter, food, and long-term property development across seasons and years. Built in Unity 6 HDRP, modeled in Blender, tracked in Git.

Full vision and design philosophy: `00_Project_Management/GDD/Homestead_GDD_v2.0.md`.

Current status: Pre-Production. Alpha 0.1 scope is locked — see the GDD's "Current Alpha 0.1 Scope" and "Development Rule" sections before proposing anything beyond it.

---

# Where To Look

## Design vision and scope

`00_Project_Management/GDD/Homestead_GDD_v2.0.md`

---

## Gameplay system design (content systems)

`05_Documentation/Game_Systems/` — Hydration, Hunger, Fire, Fishing, Foraging, Hunting, Trapping, Livestock, Water, Weather, Wildlife, Health, Economy, Season, Building and Housing. All 14 complete as of this writing.

---

## Gameplay system design (engineering-layer systems)

`05_Documentation/Systems/` — First Person Controller, Inventory, Save System. These close the gap between the GDD's Alpha 0.1 Required Systems list and Game_Systems, which didn't cover them.

---

## Not yet started

`05_Documentation/Gameplay_Loops`, `Livestock`, `Plants`, `Research`, `Technical_Design`, `Unity_Architecture`, `Wildlife` — folders exist with confirmed purpose but no content yet. See Current_Task_List.md for what each is for.

---

## Unity implementation

`01_Unity_Project` — Copilot's domain. Claude does not review or modify this without being asked.

---

## Collaboration rules and live task status

`06_AI_Collaboration/AI_Collaboration_Rules.md` — roles, handoff protocol, file ownership.

`06_AI_Collaboration/Current_Task_List.md` — the live source of truth for what's done, in progress, or queued. Check this before assuming something needs doing.

`06_AI_Collaboration/Claude/Reviews/` — past consistency reviews of the design docs.

`00_Project_Management/Decisions_Log.md` — chronological record of concrete decisions made along the way.

---

# Current Collaboration Model

Claude: game design documentation, project file/folder organization, cross-document consistency review. May draft C# scaffolding when asked, always flagged as unverified until Copilot or Mike confirms it compiles.

Copilot: C# implementation, Unity Editor work (scenes, prefabs, Inspector configuration, and similar), anything requiring compiler or Play Mode verification.

This split was set 2026-09-22 at Mike's request — Claude has more usage headroom, so it now owns more of the day-to-day work than the split originally proposed during initial brainstorming with Copilot. Full detail: AI_Collaboration_Rules.md.

---

# Ground Rules Worth Repeating

- Don't redesign what's already decided. Identify gaps and inconsistencies; don't rewrite established design.
- Avoid feature creep. The GDD's Development Rule blocks Farming, Large Livestock Operations, Vehicles, Solar Arrays, and Automation until the core survival loop is proven fun.
- When in doubt about whether something is decided, check Current_Task_List.md and Decisions_Log.md before asking Mike to repeat himself.
