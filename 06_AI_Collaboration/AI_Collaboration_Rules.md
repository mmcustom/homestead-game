# Homestead
## AI Collaboration Rules v1.0

Status: Active

---

# Purpose

This document defines how Claude and GitHub Copilot collaborate on Homestead, so work doesn't get duplicated, overwritten, or lost between sessions.

---

# Roles

## Claude

Owns:

- Game design documentation (05_Documentation)
- Project file and folder organization
- Cross-document consistency reviews (06_AI_Collaboration/Claude/Reviews)
- Draft C# scaffolding when useful — must be flagged as unverified (see Handoff Protocol)

Does not own:

- Unity Editor work (scenes, prefabs, Inspector wiring, asset import/configuration)
- Final or compiled C# implementation

---

## Copilot

Owns:

- C# implementation
- Unity Editor work (scenes, prefabs, Inspector configuration, NavMesh, sprite atlases, and similar)
- Anything requiring compiler or Play Mode verification

Does not own:

- Primary authorship of design docs. May reference them, should not restructure them without flagging Mike.

---

## Mike

Final decision-maker on design direction and role changes.

Carries handoffs between Claude and Copilot — there is no direct channel between the two.

---

# Why This Split

Claude has no Unity Editor access in this environment and cannot compile or run C# against this project. Copilot is embedded in the IDE with live compiler feedback and Editor access, which makes it the better fit for implementation and Editor-native work.

Documentation and cross-file consistency work benefit from higher usage headroom, which Claude currently has more of than Copilot.

This split was set on 2026-09-22 at Mike's request, replacing an earlier split proposed during initial brainstorming with Copilot.

---

# Handoff Protocol

## When Claude produces something Copilot needs to act on

Examples:

- A new or changed system doc that implies a code change
- Draft C# scaffolding that needs to compile and be wired up in the Editor

Claude must:

1. Say so explicitly in the chat response to Mike.
2. Add an entry to Current_Task_List.md under "Needs Copilot," naming the file(s) involved and what's needed.

Mike carries the notification to Copilot's session.

---

## When Copilot produces something Claude should review

Examples:

- A new or changed system doc
- A design decision made during implementation that should be reflected back into documentation

Mike must:

1. Tell Claude directly, or
2. Point Claude at the specific file(s) to review.

Claude will also flag anything found during a routine consistency pass that appears to have changed since Claude last read it, but this is a safety net, not a substitute for being told directly.

---

# File Ownership by Folder

## 05_Documentation

Claude primary. Copilot may reference but should flag Mike before restructuring.

---

## 01_Unity_Project

Copilot primary. Claude may propose C# scaffolding here only when asked, and only as a clearly flagged, unverified draft.

---

## 06_AI_Collaboration/Claude

Claude only.

---

## 06_AI_Collaboration/Copilot

Copilot only.

---

## 00_Project_Management

Shared. Either AI may write here, but should note who wrote it and when.

---

# Collision Avoidance

Before editing a file that may be in progress elsewhere:

1. Check Current_Task_List.md for an open entry on that file.
2. If none exists, proceed.
3. If one exists and it isn't yours, don't touch it — ask Mike first.

---

# Revision History

v1.0 — 2026-09-22 — Rules established. Claude given the expanded documentation and project organization role; Copilot scoped to Unity Editor work and C# implementation, per Mike's decision.
