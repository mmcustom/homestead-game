# Homestead
## Current Task List

Status: Active — updated by both Claude and Copilot

---

# Legend

Status values: Not Started / In Progress / Needs Review / Needs Copilot / Blocked / Done

Owner values: Claude / Claude Code / Mike ("Claude Code" is always written in full — see AI_Collaboration_Rules.md's Naming note)

See AI_Collaboration_Rules.md for the handoff protocol and file ownership behind this list.

---

# Needs Claude Code

- **Done 2026-09-23 (Claude Code) — see Log.** ~~**Unity Architecture review findings, carried over from Copilot (originally 2026-09-22)**~~ — Claude reviewed the initial Unity project scaffolding (see `06_AI_Collaboration/Claude/Reviews/Review_004_Unity_Architecture.md`) and found five things needing attention, now redirected to Claude Code since Copilot has been replaced (see Log below): (1) `Unity_Architecture.md` wasn't found anywhere in the repo despite being reported as created — needs writing for real; (2) none of the four new scenes (Bootstrap, MainMenu, World, Loading) are registered in Build Settings yet, so the intended scene flow can't run; (3) Bootstrap.unity still carries the default HDRP template's Sun/Sky and Fog Volume/Main Camera/StaticLightingSky objects — worth confirming whether those belong there or in World.unity; (4) leftover default template assets (`OutdoorsScene.unity`, `Readme.asset`, `TutorialInfo/`) at the Assets root are cleanup candidates; (5) **Decisions_Log.md's "Unity Version Lock" entry says 6000.5.9f1, but the actual installed editor (`ProjectVersion.txt`) is 6000.3.24f1** — needs Claude Code to say which is correct so the other can be fixed. None of the five are blocking or urgent — first things to tackle once Claude Code is connected, per `05_Documentation/Unity_Architecture/Claude_Code_Unity_MCP_Setup.md`'s Part 4.

---

# Needs Claude Review

- **Done 2026-09-23 (Claude) — see Log.** ~~**Unity_Architecture.md location**~~ — the doc existed at `05_Documentation/Technical_Design/Unity_Architecture.md`; moved into `05_Documentation/Unity_Architecture/`, whose confirmed purpose is Unity project conventions. Content unchanged, only reformatted to house style. Old location left as a short stub pointing to the new one, since Claude has no way to delete files on Mike's machine.
- **TimeManager pacing defaults (Claude Code, 2026-09-23)** — `Assets/Scripts/Managers/TimeManager.cs` implements the clock/calendar/seasons from Season_System.md, but the docs don't define pacing, so these are Inspector-tunable placeholder defaults pending Mike's confirmation: 24 real minutes per in-game day; 28 days per season (112-day year); game starts 06:00 on Spring day 1; sunrise/sunset per season Spring 6:30–19:30, Summer 6:00–20:30, Fall 7:00–18:30, Winter 7:30–17:00; Dawn = 2h after sunrise, Dusk = 2h before sunset (per Research.md's deer activity grounding). Also exposes `SeasonProgress` (0–1) so relative windows like Large_Game.md's "second half of Fall" Rut Window work without fixed day ranges. Once confirmed, Season_System.md could record them — and Large_Game.md's note about converting the Rut Window to day numbers could be resolved.
- **WeatherManager climate defaults (Claude Code, 2026-09-23)** — `Assets/Scripts/Managers/WeatherManager.cs` implements Weather_System.md's seven types plus a standing wind direction/strength (Hunting_System.md), temperature, and drought. The docs give seasonal tendencies but no numbers, so these are Inspector-tunable placeholder defaults pending Mike's confirmation. Share of each season's time (Clear/Cloudy/Light Rain/Heavy Rain/Thunderstorm/Cold Front/Snow/Wind): Spring 30/30/22/10/3/2/0/3, Summer 64/25/3/1/2/0/0/5, Fall 40/28/10/4/1/7/0/10, Winter 20/25/3/0/0/15/30/7. Temperature highs/lows °C: Spring 18/5, Summer 30/18, Fall 17/5, Winter 5/−6, ±4 °C day-to-day, blended across season boundaries, peaking at 15:00; Cold Front −8 °C, rain −2 to −3 °C, Snow holds at or below 0 °C; changes at most 3 °C per in-game hour. Durations: Clear 6–24h, Cloudy 4–16h, Light Rain 2–8h, Heavy Rain 2–6h, Thunderstorm 1–3h, Cold Front 24–48h, Snow 4–18h, Wind 4–12h. Drought = 5 consecutive rain-free Summer days (a 10-Summer simulation had a drought in 9 of them, averaging ~2.5 drought days). Open design questions: whether Wind should stay both a weather type and a standing property, whether Snow should ever occur in late Fall, and whether players get any forecast (none implemented). Once confirmed, Weather_System.md could record them.
- **InventoryManager capacity defaults and item data (Claude Code, 2026-09-23)** — `Assets/Scripts/Managers/InventoryManager.cs` implements Inventory_System.md with weight-based capacity (no slots, since First_Person_Controller.md ties movement speed to carried weight). Placeholder defaults pending Mike's balancing pass: encumbered above 25 kg (movement penalty starts, exposed as `Encumbrance` 0–1 for the future controller), hard limit 40 kg (can't pick up more). Home Storage capacity is set per structure when the building registers its container; a warning fires if one holds no more than the player can carry (Design Rule 2). One equipped tool slot. Perishable stacks keep the day they were acquired and are used oldest-first, so the future Spoilage System can age them — spoilage itself isn't implemented. No item assets exist yet: items are ScriptableObjects (Create > Homestead > Item) placed under `Assets/Resources/Items`, each needing an id, category, weight and shelf life. Those per-item numbers are design data for Claude/Mike to define alongside the relevant Game System docs.
- **DiscoveryManager and discovery content (Claude Code, 2026-09-23)** — `Assets/Scripts/Managers/DiscoveryManager.cs` implements Discovery_System.md: sites are `DiscoverySite` components placed in World, discovered when the player walks into their trigger (default 5 m radius), each carrying the category-specific facts Discovery_System.md lists (e.g. Water Quality = Excellent) as label/value pairs. Records double as map markers (position + category); observations can be added to known sites over time (Knowledge Progression); Historical Records are permanent milestones — "First <category> found: <name>" is recorded automatically for each category, and other systems can record their own (e.g. First Successful Deer Harvest). No sites are placed yet: which sites exist, where, and their facts are design content. JournalManager (next) will turn discoveries into journal entries; the notification UI and minimap aren't built.
- **JournalManager sections (Claude Code, 2026-09-23)** — `Assets/Scripts/Managers/JournalManager.cs` writes an entry for every discovery (title = site name, body = its facts), every observation ("Bass Hole — Slower after cold fronts", per Weather_System.md's example) and every milestone, and supports player notes (optionally attached to a discovered site). Discovery_System.md lists five journal categories; two sections were added beyond them for design confirmation: **History** (Discovery_System.md's Historical Records / milestones) and **Notes** (player notes not tied to a site — Unity_Architecture.md lists "player notes"). Automatic entries are permanent; only player notes can be edited or deleted. Entries track read/unread for the "Journal Updated" notification. No journal UI exists yet.
- **AudioManager content (Claude Code, 2026-09-23)** — `Assets/Scripts/Managers/AudioManager.cs` is implemented as infrastructure only; no design doc covers audio beyond Unity_Architecture.md's "Music, ambient effects, sound effects", and the project has no audio assets. It provides: Master/Music/Ambient/SFX volume settings (saved as player preferences, not in the save slot); menu and world music tracks crossfaded on game state; ambient layers that fade in when their season / time-of-day phase / weather conditions match (e.g. rain during Light Rain, Heavy Rain and Thunderstorm; crickets at Night in Summer); and pooled 2D, positional 3D and UI sound effects. Gameplay audio stops while paused; music and UI sounds continue. Open: which music and ambient layers the game should have (an audio design doc), and sourcing the clips.

---

# Documentation Status

## 05_Documentation/Game_Systems

Status: Done (14/14 docs complete and cross-checked for consistency)

Owner: Claude

---

## 05_Documentation/Systems

Status: In Progress (3/3 identified Alpha 0.1 gaps drafted)

Purpose (confirmed 2026-09-22): engineering-layer systems required by the GDD's Alpha 0.1 scope that Game_Systems doesn't cover (movement, inventory, persistence — not survival content).

Docs added 2026-09-22 by Claude:

- First_Person_Controller.md
- Inventory_System.md
- Save_System.md

These close the gap between the GDD's Alpha 0.1 Required Systems list and what Game_Systems had documented (First Person Controller, Inventory, and Save System were previously undocumented anywhere).

Owner: Claude

---

## 05_Documentation/Gameplay_Loops

Status: Done

Purpose (confirmed 2026-09-22): day-to-day and moment-to-moment pacing breakdowns — how individual systems combine into the player's actual play session, distinct from any one system's own doc.

Doc added 2026-09-22 by Claude: Core_Loops.md — Daily Loop, Weekly Loop (Fall), Winter Loop, Economic Loop, Year-Over-Year Loop. Pure synthesis of existing Game_Systems docs; no new design decisions.

Owner: Claude

---

## 05_Documentation/Technical_Design

Status: Done

Purpose (confirmed 2026-09-22): architecture and data model documentation — how systems are structured in code, not what they do in gameplay terms.

Doc added 2026-09-22 by Claude: Save_Data_Model.md — informal field-level sketch of Save_System.md's "What Must Be Saved," organized into Player/World/Property/Economy blocks. Explicitly marked Design Draft — Sketch, Not Implementation, with an "Open Questions for Copilot" section (serialization format, file structure, currency type) flagged as implementation calls, not design decisions.

Unity_Architecture.md briefly lived here too (Copilot wrote it 2026-09-22 but it wasn't found until Claude Code's Review_004 follow-up work); moved to 05_Documentation/Unity_Architecture on 2026-09-23 by Claude, since that's its correct home. A stub pointing to the new location remains here — Claude can't delete files on Mike's machine.

Owner: Claude

---

## 05_Documentation/Research

Status: Done

Purpose (confirmed 2026-09-22): real-world homesteading reference material used to ground the game's realism — not game design docs themselves.

Doc added 2026-09-22 by Claude: Research.md — grounds Trapping_System, Hunting_System/Wildlife_System, Livestock_System, Water_System, and Foraging_System claims in real-world sourcing. Flags two open items for later: no primary-source boiling time/temperature was confirmed for Water_System's purification step, and a Fall rut daytime-activity bonus is noted as a possible (not decided) mechanical hook for Hunting_System/Season_System.

Owner: Claude

---

## 05_Documentation/Livestock

Status: Done (3/3 Alpha 0.1 species confirmed)

Purpose (confirmed 2026-09-22): per-species data sheets (concrete stats — feed consumption, milk yield, breeding cooldown, and similar) for each livestock animal. Distinct from Game_Systems/Livestock_System.md, which covers the general mechanic and is done.

Docs added and confirmed 2026-09-22 by Claude, via a propose-then-confirm pattern (Claude drafts real-world-grounded numbers, Mike confirms or adjusts before they're locked):

- Goats.md — Slow breeder (Spring-only, 1 kid/cycle, 30-day maturation), high-value ongoing milk. Missed milking reduces yield rather than spoiling it.
- Chickens.md — Mid-speed maturation (20 days), unconditional daily eggs (not breeding-gated, unlike goat milk), halved egg output in Winter, non-seasonal hatching. Culling a hen for meat has no downside beyond the lost future eggs.
- Rabbits.md — Fastest breeder of the three by explicit design direction (15-day maturation, ~20-day full breeding cycle, 4 kits/litter), no ongoing daily resource between litters. Shares Trapping_System.md's "Small Furs" resource rather than a separate one. Population is intentionally uncapped — regulated by Livestock_System.md's existing feed-shortage pressure, not a hard limit.

Design principle established 2026-09-22 (Mike): each livestock type has a clear, non-overlapping advantage/drawback rather than similar stats with different names.

Owner: Claude

---

## 05_Documentation/Plants

Status: Done (5/5 Foraging categories confirmed)

Purpose (confirmed 2026-09-22): per-species data sheets for foraged/grown plants (identification, nutritional value, seasonal windows, and similar), distinct from Game_Systems/Foraging_System.md's general mechanic.

Docs added and confirmed 2026-09-22 by Claude, grouped by Foraging_System.md's own category structure:

- Berries.md (Blackberries, Raspberries) — confirmed identical, 3 units/patch, annual regrowth
- Fruit.md (Wild Apples, Pawpaw) — confirmed; Pawpaw has a shorter shelf life/worse Preservation outcome than Wild Apples, the one deliberate differentiator in this category
- Nuts.md (Hickory Nuts, Walnuts, Chestnuts) — confirmed; Hickory/Walnut identical, Chestnuts the one shorter-shelf-life exception; Nuts still benefit from active Preservation (drying/storage) on top of a long natural shelf life, not exempt from it
- Edible_Plants.md (Dandelion, Wild Onion, Cattail) — confirmed; fast Spring regrowth (not annual); Cattail tied to Water_System.md's discovered water sources, same pattern as Waterfowl
- Mushrooms.md — confirmed; named real species (Morel, Dryad's Saddle, Oyster Mushroom) rather than staying generic. Morel is the rare, annual, ground-growing prize (and the intended seed for the future poisonous-species system via its real False Morel lookalike); Dryad's Saddle and Oyster Mushroom are common, fast-regrowing, wood-growing, and low-risk

Owner: Claude

---

## 05_Documentation/Wildlife

Status: Done (3/3 Wildlife categories confirmed)

Purpose (confirmed 2026-09-22): per-species data sheets (concrete stats) for wildlife, distinct from Game_Systems/Wildlife_System.md, which covers the general ecosystem design and is done.

Docs added and confirmed 2026-09-22 by Claude, grouped by Wildlife_System.md's own category structure:

- Small_Game.md (Rabbit, Squirrel) — confirmed identical (1 Meat + 1 Small Furs each); Squirrel not gated behind nut-grove discovery, kept simple for Alpha 0.1
- Medium_Game.md (Turkey, Waterfowl) — confirmed differentiated: Turkey is Dawn-only, 4 Meat + Feathers; Waterfowl has its own all-day-but-wetland-gated timing, 3 Meat + Feathers, and migrates (absent in Winter) per Season_System.md's existing note
- Large_Game.md (White-Tailed Deer) — confirmed; 8 Meat; Hide and Antlers are year-round with a higher chance during a confirmed Fall "Rut Window" (second half of Fall, all-day activity). Hunting_System.md's "Antlers (seasonal)" wording was updated to match.

Owner: Claude

---

## 05_Documentation/Fishing

Status: Done (4/4 Alpha 0.1 species confirmed)

Purpose (confirmed 2026-09-22): per-species data sheets for the four Fishing_System.md species, distinct from that doc's general mechanic — one file per species, matching the Livestock pattern rather than Wildlife/Plants' category grouping, since Fishing_System.md already treats each species individually.

Docs added and confirmed 2026-09-22 by Claude, via the same propose-then-confirm pattern as Livestock/Wildlife/Plants, grounded in a new Fish Species Behavior Grounding section added to Research.md:

- Bluegill.md — easiest/most reliable catch (common, active all day, works with the cheapest gear), smallest yield (1 Food/catch)
- Crappie.md — normally the hardest to find (deep structure, dawn/dusk/night only), 2 Food/catch, but gets a real Spring Spawn Window where schools move shallow and become dramatically easier to catch — modeled as an actual behavior shift, not just a label, the same way the Deer Rut Window works
- Bass.md — 3 Food/catch, effectively locked out of the passive Fish Trap method (sight hunter, needs active lure presentation) — the one species that specifically rewards Rod and Reel / Cane Pole over passive infrastructure
- Catfish.md — 4 Food/catch (top of the four-species scale), no hard time-of-day gating, and especially effective in Fish Traps (scent-driven bottom scavenger) — the mirror image of Bass

Fishing_System.md's Fish Species and Fish Traps sections were updated with cross-references to each new file. Feature_Backlog.md's "Fishing per-species stat sheets" awareness-only entry was removed now that the gap is closed.

Owner: Claude

---

## 05_Documentation/Unity_Architecture

Status: Done — Unity_Architecture.md now lives here

Purpose (confirmed 2026-09-22): Unity project-specific conventions — scene organization, prefab structure, folder/namespace conventions inside 01_Unity_Project.

Copilot reported writing a `Unity_Architecture.md` on 2026-09-22, but Review_004 couldn't find it anywhere in the repo. Claude Code (Copilot's replacement — see Log) tracked it down on 2026-09-23: it existed all along, just at `05_Documentation/Technical_Design/Unity_Architecture.md` instead of here. Claude Code flagged the location/content call for Claude to make (05_Documentation is Claude-primary); Claude moved it here the same day, content unchanged, reformatted to house style. `Claude_Code_Unity_MCP_Setup.md` also lives in this folder — a one-time setup guide for Mike, not the architecture doc itself.

Owner: Claude

---

# Project Management

## 00_Project_Management/AI_Project_Brief.md

Status: Done

Owner: Claude

---

## 00_Project_Management/Decisions_Log.md

Status: Done — living doc, append new decisions as they're made

Owner: Claude (both AIs and Mike may add entries)

---

## 00_Project_Management/Development_Roadmap.md

Status: Done — living doc, update as scope/status changes

Owner: Claude

---

# 01_Unity_Project

Status: In Progress — initial architecture scaffolded by Copilot 2026-09-22, reviewed by Claude at Mike's request (see Review_004 below). Copilot has since been replaced by Claude Code as of 2026-09-23 (see Log below and AI_Collaboration_Rules.md v2.0) — Claude Code picks up from Copilot's scaffolding rather than starting over. Still not Claude's (this session's) domain per AI_Collaboration_Rules.md; Claude's review was read-only, no files touched.

Confirmed in place: Unity 6000.3.24f1 LTS, HDRP installed and active. Four scenes (Bootstrap, MainMenu, World, Loading) exist under Assets/Scenes. A Scripts folder structure (Core, Discovery, Inventory, Managers, Player, Saving, Survival, UI, Wildlife, World) anticipates the documented systems. Eight Manager scripts exist (GameManager, SaveManager, TimeManager, DiscoveryManager, JournalManager, InventoryManager, WeatherManager, AudioManager); only GameManager is currently attached to the Managers GameObject in Bootstrap.unity, and all eight are still empty default templates pending real implementation.

---

# Log

2026-09-22 — Claude — Created this task list alongside AI_Collaboration_Rules.md v1.0. Populated with current, verified state of 05_Documentation and 00_Project_Management. Did not touch 01_Unity_Project.

2026-09-22 — Claude — Mike confirmed the proposed purpose for all 8 empty 05_Documentation subfolders. Mike also confirmed drafting the three Alpha 0.1 gap systems. Drafted and committed First_Person_Controller.md, Inventory_System.md, and Save_System.md to 05_Documentation/Systems.

2026-09-22 — Claude — Drafted and committed AI_Project_Brief.md, Decisions_Log.md, and Development_Roadmap.md to 00_Project_Management, closing out the last of the previously-empty project management files.

2026-09-22 — Claude — Mike selected Gameplay_Loops, Technical_Design, and Research as the next work items to fill while Copilot's usage limit was reset. Drafted and committed Core_Loops.md (Gameplay_Loops), Save_Data_Model.md (Technical_Design), and Research.md (Research). Noted: this file reverted to an earlier snapshot on the device between commits (same sync issue seen once before with this file) — rebuilt from the verified current state of every doc and recommitted; re-staged afterward to confirm it stuck.

2026-09-22 — Claude — Completed 05_Documentation/Livestock (3/3 Alpha 0.1 species). Drafted Goats.md with proposed stats grounded in Research.md; Mike reviewed and confirmed the milking-penalty, maturation-pace, and litter-size questions, and set a new design principle (each livestock type needs a distinct, non-overlapping advantage/drawback — rabbits specifically should out-breed the others). Finalized Goats.md, then drafted and finalized Chickens.md (winter egg reduction confirmed, no cull penalty confirmed) and Rabbits.md (shares Trapping's Small Furs resource, no population cap, per Livestock_System.md's existing feed-shortage pressure) to that same principle. Every file in this batch reverted at least once on the device mid-commit (same recurring sync issue as before) — each was recommitted and re-staged to confirm it actually stuck before moving on.

2026-09-22 — Claude — Drafted all of 05_Documentation/Wildlife (3/3: Small_Game.md, Medium_Game.md, Large_Game.md) and 05_Documentation/Plants (5/5: Berries.md, Fruit.md, Nuts.md, Edible_Plants.md, Mushrooms.md), matching Wildlife_System.md's and Foraging_System.md's own category groupings rather than one file per individual species. All eight are proposed and pending Mike's review — two raise decisions bigger than a number tweak: a possible Fall Deer "Rut Window" (extended midday activity, following up on Research.md's flagged hook) and whether Mushrooms should name real species now or stay generic until the poisonous-species system is designed. All eight files committed and verified on the first pass — no sync reversion this time (only previously-existing, frequently-edited files like this one have shown that issue).

2026-09-22 — Claude — Worked through all 8 Wildlife/Plants files one at a time with Mike, same propose-then-confirm pattern as Livestock. Confirmed: Fall Rut Window for Deer (all-day activity, second half of Fall, Hide/Antlers year-round-but-rarer outside it — updated Hunting_System.md's wording to match); Mushrooms named to real species (Morel, Dryad's Saddle, Oyster Mushroom) instead of staying generic; Turkey now yields slightly more than Waterfowl and has its own Dawn-only timing while Waterfowl got distinct all-day-but-wetland-gated timing plus migration (tying into Season_System.md's existing "Waterfowl migrate" note); Pawpaw and Chestnuts each kept as the one deliberate shelf-life exception in their category; Nuts confirmed to still benefit from active Preservation rather than being exempt; Cattail tied to Water_System.md's discovered water sources. 05_Documentation/Wildlife and 05_Documentation/Plants are both now Done. Only 05_Documentation/Unity_Architecture remains not started, pending access to 01_Unity_Project.

2026-09-22 — Claude — Confirmed 01_Unity_Project is empty (Copilot hasn't started Unity work yet); Mike confirmed this was expected. While waiting, ran a full post-batch consistency pass (Review_003_Post_Batch_Consistency.md) across every Game_Systems, Systems, and Technical_Design doc — found and fixed a "Fur" vs. "Small Furs" terminology drift left over from an earlier correction, in Wildlife_System.md, Hunting_System.md, Economy_System.md, Trapping_System.md, and Livestock_System.md (5 spots across 4 docs), plus a stale forward-reference in Save_Data_Model.md that predated the Livestock species sheets being written. Also consolidated every "Future System / Not Alpha 0.1" note scattered across all docs into Feature_Backlog.md (was empty), organized by category (Wildlife & Hunting, Trapping, Livestock, Water, Fishing, Player Systems, Economy, Environment), with a pointer to Development_Roadmap.md for the larger Stage 2–4 deferrals rather than duplicating them.

2026-09-22 — Claude — Completed 05_Documentation/Fishing (4/4 species), closing the gap Review_003 had flagged for awareness. Added a Fish Species Behavior Grounding section to Research.md first (Bluegill vs. Crappie vs. Bass vs. Catfish real-world feeding/activity/habitat differences), then drafted and confirmed one file per species with Mike, one at a time: Bluegill.md (easiest catch, smallest yield, active all day), Crappie.md (hardest to find normally, but gets a confirmed Spring Spawn Window — schools move shallow and become dramatically easier to catch, modeled as a real behavior shift like the Deer Rut Window), Bass.md (locked out of passive Fish Traps, rewards active fishing), Catfish.md (top yield, especially effective in Fish Traps, the mirror of Bass). Updated Fishing_System.md's Fish Species and Fish Traps sections with cross-references, and removed the now-closed "Fishing per-species stat sheets" entry from Feature_Backlog.md.

2026-09-22 — Claude — Copilot began Unity work and Mike relayed its status report for review (Bootstrap/MainMenu/World/Loading scenes, an 8-manager architecture, Unity_Architecture.md). Reviewed the actual project files on disk against the report — see `06_AI_Collaboration/Claude/Reviews/Review_004_Unity_Architecture.md`. Confirmed: Unity 6000.3.24f1 LTS, HDRP active, all four scenes present, GameManager correctly attached to a Managers GameObject in Bootstrap.unity, all eight Manager scripts present, and a Scripts folder structure already anticipating the documented systems. Four things flagged back to Copilot via the new "Needs Copilot" entry above: Unity_Architecture.md not found anywhere in the repo, none of the four scenes registered in Build Settings yet, Bootstrap.unity carrying leftover default HDRP template objects (Sun/Sky and Fog Volume/Main Camera/StaticLightingSky) that likely belong in World.unity instead, and leftover default template assets at the Assets root as cleanup candidates. While adding this entry, also found that Copilot had separately written a "Unity Version Lock" entry directly to Decisions_Log.md naming 6000.5.9f1 — which doesn't match the 6000.3.24f1 actually installed (per ProjectVersion.txt) or reported. Added as a fifth item. Nothing in 01_Unity_Project was touched — review was read-only per AI_Collaboration_Rules.md.

2026-09-23 — Claude — Mike reported Copilot kept running out of usage and asked about pivoting to Claude Code plus a Unity MCP server. Researched current options and, per Mike's decisions, fully replaced Copilot with Claude Code (connected to the Unity Editor via the free, open-source CoplayDev/unity-mcp server rather than Unity's official MCP, which needs an AI-beta subscription). Published AI_Collaboration_Rules.md v2.0 (Copilot → Claude Code throughout, plus a new Naming section to keep "Claude" and "Claude Code" unambiguous since both are Claude products), a new setup guide (`05_Documentation/Unity_Architecture/Claude_Code_Unity_MCP_Setup.md` — install steps, MCP connection steps, and a pointer back to Review_004's findings as Claude Code's first to-do list), and a Decisions_Log.md entry. The "Needs Copilot" list is now "Needs Claude Code" and carries the same five items from Review_004 forward — nothing is being redone, Claude Code picks up where Copilot's scaffolding left off.

2026-09-23 — Claude Code — Closed out Review_004's five findings. (1) Unity_Architecture.md does exist, at `05_Documentation/Technical_Design/Unity_Architecture.md` — queued under "Needs Claude Review" for a location/content call. (2) Build Settings already listed Bootstrap, MainMenu, World, Loading (all enabled); no change needed. (3) Per Unity_Architecture.md ("Bootstrap: initialize all game managers… loads MainMenu"), removed the HDRP template Sun, Sky and Fog Volume, Main Camera, and StaticLightingSky from Bootstrap.unity — it now holds only the Managers GameObject. World.unity already has its own Global Volume, Directional Light, and Main Camera prefab instance, so nothing needed moving. (4) Deleted the unused template assets `Assets/OutdoorsScene.unity`, `Assets/Readme.asset`, `Assets/TutorialInfo/`, plus two empty duplicate scenes found at the Assets root (`Assets/World.unity`, `Assets/Loading.unity`, unreferenced copies of the real ones under Assets/Scenes). Unity console clean afterward. (5) Mike confirmed 6000.3.24f1 is the correct version; Decisions_Log.md's Version Lock entry corrected and reformatted to match the log's style.

2026-09-23 — Claude — Closed the "Unity_Architecture.md location" review Claude Code flagged. Moved the doc from `05_Documentation/Technical_Design/Unity_Architecture.md` to `05_Documentation/Unity_Architecture/Unity_Architecture.md`, its correct home per that folder's confirmed purpose. Content unchanged — Copilot's original Engine/Scene Architecture/Bootstrap/Manager Architecture/Save Architecture/Script Organization/Design Rules content carried over as-is, only reformatted from one-line paragraphs into this doc set's usual prose-with-`---`-dividers style. Left a short stub at the old location pointing to the new one, since files can't be deleted from this session. 05_Documentation/Unity_Architecture and 05_Documentation/Technical_Design are both Done again.
