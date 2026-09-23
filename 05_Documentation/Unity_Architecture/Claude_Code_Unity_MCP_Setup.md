# Homestead
## Claude Code + Unity MCP Setup v1.0

Status: Design Draft

---

# Purpose

Step-by-step setup for Mike to install Claude Code and connect it to the Homestead Unity project via the Unity MCP server, replacing GitHub Copilot as the implementation partner. See AI_Collaboration_Rules.md v2.0 and Decisions_Log.md for why this changed (Copilot kept running out of usage) and what it means for the AI role split.

This is a guide for Mike to follow on his own Windows machine — Claude (this session) has no ability to install software or run commands there directly.

---

# Part 1 — Install Claude Code

Claude Code requires a Pro, Max, Team, Enterprise, or Console Claude account — the free claude.ai plan does not include it.

Recommended: the native installer, since it auto-updates in the background.

**Windows PowerShell** (open PowerShell, not CMD — the prompt shows `PS C:\` when you're in PowerShell):

```powershell
irm https://claude.ai/install.ps1 | iex
```

**Windows CMD**, if that's what's open instead:

```batch
curl -fsSL https://claude.ai/install.cmd -o install.cmd && install.cmd && del install.cmd
```

Installing [Git for Windows](https://git-scm.com/downloads/win) alongside is optional but recommended — it gives Claude Code a proper Bash tool instead of falling back to PowerShell only.

After installing, verify it worked:

```powershell
claude --version
```

Then log in — run `claude` in any terminal and follow the browser prompt to authenticate with the Claude account.

---

# Part 2 — Install the Unity MCP package

This adds the CoplayDev/unity-mcp bridge to the Homestead Unity project (free, MIT-licensed, supports Unity 2021.3 LTS through 6.x — covers the project's current 6000.3.24f1).

1. Open the Homestead project in Unity Editor.
2. Window → Package Manager.
3. Click the `+` button → **Add package from git URL**.
4. Paste: `https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#main`
5. Wait for the package to install.

---

# Part 3 — Connect Claude Code to Unity

1. In Unity: **Window → MCP for Unity → Configure All Detected Clients**. This should detect the local Claude Code installation and write its connection config automatically.
2. If Claude Code isn't auto-detected, the manual fallback is the `claude mcp add` command — run it from a terminal open in the Homestead project folder (`01_Unity_Project/Homestead`). The exact command Unity's MCP window shows may differ slightly by version; use what the Configure window displays rather than guessing.
3. Start (or restart) Claude Code in the project folder:

```powershell
cd "C:\Users\mmcus\OneDrive\Documents\Homestead Game\01_Unity_Project\Homestead"
claude
```

4. Ask Claude Code to list its available tools, or something simple like "summarize the current Unity console messages" — if it responds with real console output rather than saying it has no such tool, the connection is working.

---

# Part 4 — First things to have Claude Code do

Once connected, these pick up exactly where the Copilot-built scaffolding left off — see `06_AI_Collaboration/Claude/Reviews/Review_004_Unity_Architecture.md` for the full findings Claude (this session) already found by inspecting the files directly:

1. **Write (or re-write) `Unity_Architecture.md`** — the previous one was reported created but never actually found on disk. Once it exists in `05_Documentation/Unity_Architecture/`, Claude (this session) will read and fold it into the doc set.
2. **Register the four scenes in Build Settings** — Bootstrap, MainMenu, World, Loading aren't in the build scene list yet, so nothing can actually boot through the intended flow.
3. **Decide what belongs in Bootstrap.unity vs. World.unity** — Bootstrap currently carries the default HDRP template's Sun / Sky and Fog Volume / Main Camera / StaticLightingSky objects, which likely belong in World.unity instead if Bootstrap is meant to be a lightweight init-only scene.
4. **Clean up the leftover default template assets** — `OutdoorsScene.unity`, `Readme.asset`, `TutorialInfo/` at the Assets root.
5. **Resolve the Unity version discrepancy** — Decisions_Log.md's "Unity Version Lock" entry says 6000.5.9f1; the actual installed editor is 6000.3.24f1. Whichever is correct, the other needs fixing so the record is accurate.

---

# Design Rules

1. This doc describes setup steps for Mike's machine, not a design decision — if the exact CoplayDev menu wording or install flow has changed since this was written, follow what's actually on screen over what's written here.

2. Once Claude Code is up and running, its own session notes and implementation logs belong in `06_AI_Collaboration/Claude_Code/`, not this doc — this doc is one-time setup reference, not a living log.
