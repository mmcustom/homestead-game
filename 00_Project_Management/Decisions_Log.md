# Homestead
## Decisions Log

Status: Active — chronological, newest at the bottom

---

# Purpose

A record of concrete decisions made on this project, so neither AI has to re-litigate something already settled.

This log starts 2026-09-22, when it was created. Decisions made earlier — during Mike's initial brainstorming with Copilot, including the original AI role split — predate this log and aren't captured here in detail. If those are worth preserving, they can be backfilled.

---

# Log

**2026-09-22 — GDD Review (Claude)**
Senior design consultant review of Homestead_GDD_v2.0.md. Identified Season System, food spoilage, water contamination sources, Trapping detail, Health/Morale formalization, and an economy sink as the highest-priority gaps. Full detail: `06_AI_Collaboration/Claude/Reviews/Review_001_GDD_v2.md`.

---

**2026-09-22 — Hunting_System.md authored (Claude)**
Closed the last undocumented Year One food pillar. Early-game weapons set as Recurve Bow (silent, close-range) and Bolt-Action Rifle (longer range, louder). Wounded Animal Tracking deferred to Wildlife System's existing Future System entry rather than re-scoped.

---

**2026-09-22 — System docs consistency review (Claude)**
Cross-checked all 14 Game_Systems docs. Found Water_System.md and Weather_System.md truncated mid-write; Core_Survival_System.md duplicating Water and Trapping content with drifted details (Pond quality mismatch, Hide incorrectly listed as a Trapping output); Morale independently defined in two docs with mismatched triggers. Full detail: `06_AI_Collaboration/Claude/Reviews/Review_002_System_Docs_Consistency.md`.

---

**2026-09-22 — Water_System.md and Weather_System.md completed (Claude)**
Finished both truncated docs: Water Transportation, Water Storage, and integration sections for Water; remaining weather types (Cold Front, Snow, Wind) and an explicit Exposure Connection to Health_System.md for Weather.

---

**2026-09-22 — Core_Survival_System.md trimmed (Claude)**
Water Sources, Water Quality Ratings, and Trapping System sections collapsed to short summaries pointing at Water_System.md and Trapping_System.md as the sources of truth, instead of duplicating them. Hide removed from the Trapping outputs summary.

---

**2026-09-22 — Trapping output terminology changed to "Small Furs" (Mike)**
Trapping_System.md and Core_Survival_System.md updated: Rabbit and Squirrel trapping yields are now named "Small Furs," not generic "Fur."

---

**2026-09-22 — Morale trigger expanded (Mike)**
Health_System.md's Morale Decreases list now includes "Overwork / Lack of Rest" as its own trigger, distinct from "Lack of Sleep."

---

**2026-09-22 — AI collaboration role split changed (Mike)**
Claude given the larger day-to-day role — documentation, project organization, cross-doc consistency work. Copilot scoped to Unity Editor work and C# implementation. Reasoning: Claude has no Unity Editor access or compiler in this environment, so implementation work stays with Copilot; Claude has more usage headroom, so documentation and organization move to Claude. Supersedes the original split proposed during initial brainstorming with Copilot. Mike to be notified any time a handoff is needed in either direction — see AI_Collaboration_Rules.md's Handoff Protocol. Full detail: `AI_Collaboration_Rules.md` v1.0.

---

**2026-09-22 — Purpose confirmed for 8 previously-undefined 05_Documentation subfolders (Mike)**
Gameplay_Loops = pacing breakdowns. Livestock/Plants/Wildlife = per-species data sheets, distinct from their same-named Game_Systems docs. Research = real-world reference material. Systems = engineering-layer docs missing from Game_Systems. Technical_Design = architecture/data models. Unity_Architecture = Unity project conventions. Full detail: `06_AI_Collaboration/Current_Task_List.md`.

---

**2026-09-22 — First_Person_Controller.md, Inventory_System.md, Save_System.md authored (Claude)**
Closed the gap between the GDD's Alpha 0.1 Required Systems list and what Game_Systems had documented. All three placed in `05_Documentation/Systems/`.

---

**2026-09-22 — AI_Project_Brief.md, Decisions_Log.md, Development_Roadmap.md authored (Claude)**
Populated the three previously-empty 00_Project_Management docs from existing material — no new creative decisions required.

---

**2026-09-22 — Core_Loops.md, Save_Data_Model.md, Research.md authored (Claude)**
Filled three of the seven remaining empty 05_Documentation subfolders while Copilot's usage limit was resetting, per Mike's selection. Core_Loops.md (Gameplay_Loops) synthesizes existing systems into Daily/Weekly/Winter/Economic/Year-Over-Year play loops — no new mechanics. Save_Data_Model.md (Technical_Design) is an explicitly-labeled design sketch, not an implementation spec, with open questions flagged for Copilot (serialization format, file structure, currency type). Research.md (Research) grounds Trapping, Hunting/Wildlife, Livestock, Water, and Foraging claims in real-world sourcing; flags two open items — no primary-source boiling time/temperature confirmed for Water_System's purification step, and a possible (undecided) Fall rut daytime-activity bonus as a realism hook for Hunting_System/Season_System. Livestock, Plants, Wildlife, and Unity_Architecture remain not started — the first three need Mike's input on concrete species stats, and Unity_Architecture needs access to 01_Unity_Project, which wasn't opened up this round.

---

**2026-09-22 — Livestock species differentiation principle set; Goats confirmed (Mike)**
Each of the three Alpha 0.1 livestock types must have a distinct, non-overlapping advantage/drawback rather than similar stats under different names. Rabbits specifically confirmed to be the fastest breeder of the three. Goats.md locked in: Spring-only breeding, 1 kid/cycle (twins deferred to Stage 2+), 30-day maturation, missed milking reduces yield rather than spoiling it. Chickens.md and Rabbits.md drafted to the same differentiation principle (Chickens = mid-speed, unconditional daily eggs; Rabbits = fastest maturation/breeding cycle, largest litters, no ongoing daily resource) and were pending Mike's review before being locked the same way.

---

**2026-09-22 — Chickens.md and Rabbits.md confirmed (Mike)**
Chickens: Winter halves egg output (matching real hens laying less in cold/low light); culling a hen for meat carries no downside beyond the lost future eggs. Rabbits: livestock rabbit fur reuses Trapping_System.md's existing "Small Furs" resource rather than a new one; rabbit population growth stays uncapped, regulated by Livestock_System.md's existing "uncontrolled breeding creates feed shortages" design rule rather than a hard limit. All three Alpha 0.1 livestock species (Goats, Chickens, Rabbits) are now confirmed and locked.

---

**2026-09-22 — Wildlife and Plants species sheets drafted (Claude)**
Drafted 05_Documentation/Wildlife (Small_Game.md, Medium_Game.md, Large_Game.md) and 05_Documentation/Plants (Berries.md, Fruit.md, Nuts.md, Edible_Plants.md, Mushrooms.md), grouped by category to match Wildlife_System.md's and Foraging_System.md's existing structure rather than one file per individual species. Proposed a three-tier Wildlife yield curve (Small Game 1 unit → Medium Game 3 units → Large Game 8 units) and carried Foraging_System.md's existing annual berry-patch regrowth example forward into Fruit and Nuts, with faster regrowth proposed for Edible Plants and Mushrooms. Two open decisions raised for Mike beyond simple number-tuning: whether to add a Fall Deer "Rut Window" mechanic (Large_Game.md — follows up on the hook Research.md flagged), and whether Mushrooms should name real species now or stay generic pending the poisonous-species Future System. All eight files pending Mike's review before being locked the way Goats/Chickens/Rabbits were.

---

**2026-09-22 — Wildlife and Plants species sheets confirmed (Mike)**
Worked through all eight files one at a time. Large Game: Fall Rut Window confirmed — Deer active all day during the second half of Fall; Hide and Antlers are year-round rather than Fall-only, just rarer outside the Rut Window (Hunting_System.md's "Antlers (seasonal)" wording updated to match, and now points to Large_Game.md for detail). Medium Game: Turkey and Waterfowl differentiated — Turkey yields slightly more (4 vs. 3 Meat) but is Dawn-only, Waterfowl has its own all-day timing but is wetland-gated and migrates (ties into Season_System.md's existing "Waterfowl migrate" note). Small Game: Rabbit and Squirrel kept intentionally identical, no nut-grove gating for Squirrel. Plants: Berries kept identical (Blackberries/Raspberries); Fruit differentiated (Pawpaw spoils faster than Wild Apples); Nuts mostly identical (Hickory/Walnut) with Chestnuts as the one shelf-life exception, and confirmed Nuts still benefit from active Preservation rather than being exempt; Edible Plants confirmed fast Spring regrowth, Cattail tied to Water_System.md's discovered water sources; Mushrooms named to three real species — Morel (rare, annual, the intended seed for the future poisonous-species system via False Morel), Dryad's Saddle and Oyster Mushroom (common, fast-regrowing, low-risk). 05_Documentation/Wildlife and 05_Documentation/Plants are both fully done. Only Unity_Architecture remains, pending access to 01_Unity_Project.

---

**2026-09-22 — 01_Unity_Project confirmed empty; post-batch consistency pass and Feature_Backlog.md consolidation (Claude)**
Verified 01_Unity_Project is genuinely empty (no stray Unity project elsewhere in the Documents folder) — Copilot hasn't started Unity work yet; Mike confirmed this was expected, so Unity_Architecture stays blocked until Copilot begins. While waiting, ran a full re-read of every Game_Systems, Systems, and Technical_Design doc plus the new Livestock/Wildlife/Plants sheets. Found and fixed a "Fur" vs. "Small Furs" terminology drift left over from the earlier correction — five spots across Wildlife_System.md, Hunting_System.md, Economy_System.md, Trapping_System.md, and Livestock_System.md that the original pass missed — and updated a stale forward-reference in Save_Data_Model.md's Property Block to point at the now-written Goats.md, Chickens.md, and Rabbits.md. Full detail: `06_AI_Collaboration/Claude/Reviews/Review_003_Post_Batch_Consistency.md`. Also populated the previously-empty Feature_Backlog.md, consolidating every "Future System / Not Alpha 0.1" note scattered across the documentation set into one catalog organized by category (Wildlife & Hunting, Trapping, Livestock, Water, Fishing, Player Systems, Economy, Environment), with a pointer to Development_Roadmap.md for the larger Stage 2–4 deferrals rather than duplicating them.

---

**2026-09-22 — 05_Documentation/Fishing species sheets confirmed (Mike)**
Closed the Fishing per-species gap Review_003 had flagged for awareness only. Added a Fish Species Behavior Grounding section to Research.md first, then worked through Bluegill, Crappie, Bass, and Catfish one at a time, same propose-then-confirm pattern as Livestock/Wildlife/Plants — one file per species, matching Livestock's structure since Fishing_System.md already treats each species individually rather than by category. Confirmed: Bluegill is the easiest, most reliable catch (active all day, works with the cheapest gear) with the smallest yield (1 Food/catch); Crappie is normally the hardest to find (deep structure, dawn/dusk/night only, 2 Food/catch) but gets a real Spring Spawn Window — schools move to shallow water and become dramatically easier to catch, modeled as an actual behavior shift rather than a label, the same way the Deer Rut Window works; Bass (3 Food/catch) is effectively locked out of the passive Fish Trap method since it's a sight hunter that needs active lure presentation, making it the one species that specifically rewards Rod and Reel / Cane Pole; Catfish (4 Food/catch, top of the scale) has no hard time-of-day gating and is especially effective in Fish Traps as a scent-driven bottom scavenger — the mirror image of Bass. Fishing_System.md's Fish Species and Fish Traps sections were updated with cross-references, and Feature_Backlog.md's now-closed Fishing awareness note was removed. All four 05_Documentation species/data categories (Livestock, Wildlife, Plants, Fishing) are now fully confirmed and locked; only Unity_Architecture remains, pending access to 01_Unity_Project.

## 2026-09-22

### Unity Version Lock

Homestead is locked to:

Unity 6000.5.9f1

This version will be used for Alpha development.

Do not upgrade Unity during active development unless:

- A critical bug requires it
- A required package requires it
- Project leadership approves the upgrade

Upgrades require a full project backup and git commit before proceeding.
