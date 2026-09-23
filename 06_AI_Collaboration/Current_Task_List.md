# Homestead
## Current Task List

Status: Active — updated by both Claude and Copilot

---

# Legend

Status values: Not Started / In Progress / Needs Review / Needs Copilot / Blocked / Done

Owner values: Claude / Copilot / Mike

See AI_Collaboration_Rules.md for the handoff protocol and file ownership behind this list.

---

# Needs Copilot

Nothing currently queued.

---

# Needs Claude Review

Nothing currently queued.

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

Status: Not Started

Purpose (confirmed 2026-09-22): Unity project-specific conventions — scene organization, prefab structure, folder/namespace conventions inside 01_Unity_Project. Requires visibility into 01_Unity_Project, which is Copilot's domain and hasn't been opened up to Claude yet.

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

Status: Not reviewed by Claude — Copilot's domain, per AI_Collaboration_Rules.md.

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
