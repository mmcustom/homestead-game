# Homestead
## AI Collaboration Rules v2.0

Status: Active

---

# Purpose

This document defines how Claude and Claude Code collaborate on Homestead, so work doesn't get duplicated, overwritten, or lost between sessions.

---

# A Note on Naming

Both AI tools on this project are Claude products, which makes clear naming important. Throughout this document and every other Homestead doc:

- **"Claude"** always means this session — Claude in Cowork, working through the device bridge to Mike's computer. Docs, project organization, cross-file consistency.
- **"Claude Code"** always means the separate Claude Code CLI, installed and run locally on Mike's Windows machine (in a terminal or VS Code), connected live to the Unity Editor via the Unity MCP server. Unity Editor work and C# implementation.

Neither name is ever shortened in a way that could mean the other. "Claude Code" is always written out in full — never just "Claude" — anywhere it appears in these docs.

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

## Claude Code

Owns:

- C# implementation
- Unity Editor work (scenes, prefabs, Inspector configuration, NavMesh, sprite atlases, and similar)
- Anything requiring compiler or Play Mode verification

Via the Unity MCP server (CoplayDev/unity-mcp, connected 2026-09-23), Claude Code also has live access to the running Unity Editor — reading console output, scene hierarchy, and component state directly, and running tests, rather than only writing files blindly. This is a real capability gain over the previous Copilot-based setup, where several early scaffolding issues (Review_004_Unity_Architecture.md) went unnoticed until Claude reviewed the files after the fact.

Does not own:

- Primary authorship of design docs. May reference them, should not restructure them without flagging Mike.

---

## Mike

Final decision-maker on design direction and role changes.

Carries handoffs between Claude and Claude Code — there is no direct channel between the two, even though both are Claude products. They run as separate, unconnected sessions.

---

# Why This Split

Claude (this session) has no Unity Editor access and cannot compile or run C# — it works through a device bridge to Mike's files, not a live Editor connection. Claude Code, running locally with the Unity MCP server, has exactly the live Editor access and compiler feedback that implementation work needs.

Documentation and cross-file consistency work benefit from higher usage headroom, which Claude (Cowork) has relative to a locally-run coding session.

---

## Revision History Context

This is the second version of this split. The first (v1.0, 2026-09-22) assigned the implementation role to GitHub Copilot. That role now moves to Claude Code as of 2026-09-23, at Mike's request, because Copilot repeatedly ran out of usage mid-task. See Decisions_Log.md for the full decision record, including the Unity MCP server chosen (CoplayDev/unity-mcp) and the setup guide (05_Documentation/Unity_Architecture/Claude_Code_Unity_MCP_Setup.md).

Everything Copilot produced before the pivot (the initial scene/manager scaffolding reviewed in Review_004_Unity_Architecture.md) stays as project history and as Claude Code's starting point — it isn't being redone from scratch, just picked up and continued.

---

# Handoff Protocol

## When Claude produces something Claude Code needs to act on

Examples:

- A new or changed system doc that implies a code change
- Draft C# scaffolding that needs to compile and be wired up in the Editor

Claude must:

1. Say so explicitly in the chat response to Mike.
2. Add an entry to Current_Task_List.md under "Needs Claude Code," naming the file(s) involved and what's needed.

Mike carries the notification to the Claude Code session.

---

## When Claude Code produces something Claude should review

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

Claude primary. Claude Code may reference but should flag Mike before restructuring.

---

## 01_Unity_Project

Claude Code primary. Claude may propose C# scaffolding here only when asked, and only as a clearly flagged, unverified draft.

---

## 06_AI_Collaboration/Claude

Claude only.

---

## 06_AI_Collaboration/Claude_Code

Claude Code only. Replaces the old `06_AI_Collaboration/Copilot` folder, which stays in place (empty) as project history rather than being deleted — Claude has no way to delete files on Mike's machine.

---

## 00_Project_Management

Shared. Either AI may write here, but should note who wrote it and when.

---

# Collision Avoidance

Before editing a file that may be in progress elsewhere:

1. Check Current_Task_List.md for an open entry on that file.
2. If none exists, proceed.
3. If one exists and it isn't yours, don't touch it — ask Mike first.

This matters more now than under the old split: Claude Code runs with live Editor access and can save changes at any time, so a file that looked stable a minute ago may not be. When in doubt, re-check before writing.

---

# Revision History

v1.0 — 2026-09-22 — Rules established. Claude given the expanded documentation and project organization role; Copilot scoped to Unity Editor work and C# implementation, per Mike's decision.

v2.0 — 2026-09-23 — Copilot replaced by Claude Code as the implementation partner, connected to the Unity Editor via the Unity MCP server (CoplayDev/unity-mcp), because Copilot repeatedly ran out of usage. Added the Naming section to keep "Claude" and "Claude Code" unambiguous across all docs. File ownership and Handoff Protocol updated to name Claude Code in place of Copilot; the old `06_AI_Collaboration/Copilot` folder is retired in favor of `06_AI_Collaboration/Claude_Code`.
