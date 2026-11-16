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

---

**2026-09-22 — Unity Version Lock (Copilot; version corrected 2026-09-23 by Claude Code, confirmed by Mike)**
Homestead is locked to Unity 6000.3.24f1 LTS for Alpha development, matching `ProjectSettings/ProjectVersion.txt` and `05_Documentation/Technical_Design/Unity_Architecture.md`. Do not upgrade Unity during active development unless a critical bug requires it, a required package requires it, or Mike approves the upgrade. Upgrades require a full project backup and git commit before proceeding.

Correction note: this entry originally named 6000.5.9f1, which never matched the installed editor (flagged in Review_004_Unity_Architecture.md, Finding 5). Mike confirmed 6000.3.24f1 is correct on 2026-09-23.

---

**2026-09-22 — Unity Architecture review (Claude)**
Copilot reported the initial Unity project scaffolding complete and ready for review: Unity 6000.3.24f1 LTS, four scenes (Bootstrap, MainMenu, World, Loading), an eight-manager architecture (GameManager, SaveManager, TimeManager, DiscoveryManager, JournalManager, InventoryManager, WeatherManager, AudioManager), and a Unity_Architecture.md doc. Claude reviewed the actual project files on disk per AI_Collaboration_Rules.md's Handoff Protocol — full findings in `06_AI_Collaboration/Claude/Reviews/Review_004_Unity_Architecture.md`. Confirmed: HDRP active, all four scenes present, GameManager correctly attached to a Managers GameObject in Bootstrap.unity, all eight manager scripts present, and a Scripts folder structure already anticipating the documented systems (Core, Discovery, Inventory, Managers, Player, Saving, Survival, UI, Wildlife, World). Five things flagged back to Copilot: Unity_Architecture.md not found anywhere in the repo despite being reported as created; none of the four scenes registered in Build Settings yet; Bootstrap.unity carrying leftover default HDRP template objects (Sun, Sky and Fog Volume, Main Camera, StaticLightingSky) that likely belong in World.unity instead; leftover default template assets at the Assets root as cleanup candidates; and **this doc's own Unity Version Lock entry immediately above says 6000.5.9f1, which doesn't match the 6000.3.24f1 actually installed on the project or named in Copilot's status report** — needs Copilot to confirm which is correct. Nothing in 01_Unity_Project was touched; review was read-only.

---

**2026-09-23 — Copilot replaced by Claude Code as implementation partner (Mike)**
Mike reported Copilot kept running out of usage mid-task and asked about pivoting to Claude Code, plus giving it a live Unity MCP connection. Claude researched current options rather than relying on possibly-stale knowledge: Unity's own official MCP package exists but requires an active Unity AI tools beta subscription; two free/open-source third-party servers were found instead — CoplayDev/unity-mcp (MIT, 47 tools, supports Unity 2021.3 LTS through 6.x) and IvanMurzak/Unity-MCP (70+ tools, adds runtime/in-game debugging and Roslyn code execution). Mike decided, via three explicit choices: (1) Claude Code fully replaces Copilot rather than the two running in parallel; (2) CoplayDev/unity-mcp is the server to connect, over Unity's official subscription-gated option and over IvanMurzak/Unity-MCP; (3) Claude should draft all the pivot documentation now rather than just walk through setup steps in chat first.

Reasoning: Copilot's repeated usage exhaustion was blocking implementation progress outright, which outweighs any advantage to running two implementation AIs in parallel. CoplayDev/unity-mcp was preferred over Unity's official MCP because it needs no separate paid beta enrollment, and over IvanMurzak/Unity-MCP because its smaller, more focused tool set is a better fit for a first-time setup, even though IvanMurzak's is more feature-rich.

Everything Copilot already built (reviewed in Review_004_Unity_Architecture.md) carries forward as Claude Code's starting point rather than being redone. AI_Collaboration_Rules.md was rewritten to v2.0 (Copilot → Claude Code throughout, plus a new Naming section distinguishing "Claude," this session, from "Claude Code," the separate local CLI — both are Claude products, so the docs are explicit everywhere). A new setup guide, `05_Documentation/Unity_Architecture/Claude_Code_Unity_MCP_Setup.md`, walks Mike through installing Claude Code, installing the CoplayDev/unity-mcp package, connecting the two, and lists Review_004's five findings as Claude Code's first to-do list once it's running. Current_Task_List.md's "Needs Copilot" section is now "Needs Claude Code," carrying the same five items forward.

Installing Claude Code and the Unity MCP package on Mike's machine is a manual step Mike needs to do himself — Claude (this session) has no ability to install software or run commands there.

---

**2026-09-25 — A generalized Crafting/Resources/Tools system is deferred to post-Alpha (Mike, via Claude)**
Mike asked whether resources, items, and crafting tools/items need real design attention now or can wait until after Alpha. Confirmed: it can wait. Grounding: the GDD's Alpha 0.1 Required Systems list names no "Crafting" or "Tools" system of its own; Item_Data.md already covers the item side that's actually needed (Resources/Materials/Tools/Consumables categories with weight and shelf-life per item); and Feature_Backlog.md already carries a "Tool Durability" entry (degradation, maintenance, repair), sourced from Inventory_System.md, sitting in the deferred/post-Alpha bucket — the design review run against the GDD independently placed "tool degradation" in the same lowest-priority, correctly-post-Alpha spot, citing the GDD's own Development Rule that nothing beyond the core survival loop gets built until that loop is proven fun. What's actually been happening in practice already fits this: each new feature defines its own small recipe as it needs one rather than routing through any generic system — Fire Building's "Build → Campfire (3 Firewood)" this week is the precedent, done directly in FireManager/InventoryManager. Decision: keep that per-feature ad-hoc pattern for anything still needing a build cost (Tent Placement will likely need one), and hold off on a real generalized crafting layer — workbenches, recipe tiers, tool durability/repair — until after Alpha, same as it's already filed in Feature_Backlog.md. Nothing on Current_Task_List.md changes as a result; nothing was blocked on this.


**2026-10-09 - A separate Crafting screen is wanted; the deferral of crafting mechanics stands (Mike, via Claude)**
Mike, after the Craft grid grew to 13 buttons: "I'm thinking we are going to need a separate 'crafting' screen by the time we are done adding craft able items." This changes the 2026-09-25 decision only for the **screen**: crafting moves out of the Inventory screen into its own Crafting screen (design draft in Crafting_System.md). Crafting **mechanics** (skills, stations, timers, durability, discovery) stay deferred to post-Alpha. Not yet confirmed by Mike: the first-pass layout, and whether placed Build items should join the screen later.
## 2026-10-09 - Crafting/Build screen approved to build (Mike)
Decision: build the separate screen now ("think we need to move forward with the separate craft/build screen"). One screen, two tabs: Craft and Build. Crafting mechanics stay as is (instant, no skills, stations or discovery); the 2026-09-25 deferral of a full Crafting system still holds. Spec: 05_Documentation/Game_Systems/Crafting_System.md. Scope reading (Craft and Build together) is Claude's default for "craft/build"; Mike has not corrected it.

## 2026-10-10 - Seasonal forage: pool of patches (Option A); no Winter forage (Mike)
Decision: forage frequency follows the seasons using a pool of patches per type (22 patches: the original 13 plus 9), activated per season by a per-save seed and hidden when out of season, with discovered patches greyed on the map, minimap and journal. Built and confirmed in Play Mode 2026-10-10. Existing species only, each in its own season, so Spring has plants, cattail and mushrooms, Summer has berries, and Fall has fruit and nuts. Mike: "no winter forage is fine," so Winter has none (this overrides the Winter values in the first-pass table). Species for the other empty Summer/Fall cells can be added later through the species' "Also Bears In" season list. Spec: 05_Documentation/Game_Systems/Foraging_System.md.

## 2026-10-11 - Winter Preparation playthrough passed; Alpha 0.1 survival loop proven fun as built (Mike)
Result: Mike played the full Winter Preparation run (new Pioneer game, Spring through 14+ game days of Winter) and answered pass to all 25 checks, saying the playtest was fun. No bugs were reported. Mike also passed the three design questions (Fall tension without spoilage/preservation, firewood pressure, Trading Post income), which Claude reads as: the core loop works as built, so spoilage, smoking/drying and Winter water freezing are NOT required to call Alpha 0.1 proven. Whether to add them is now a post-Alpha 0.1 design choice for Mike, not a blocker. Record: 05_Documentation/Gameplay_Loops/Winter_Preparation_Playthrough.md and Current_Task_List.md (2026-10-11).


---

**2026-10-23 — Starting weapon fixed by difficulty; savings go to the debt on a loss (Mike)**
Weapon choice removed from New Game: Pioneer (hardest) starts with the Recurve Bow and 20 arrows, Settler and Homesteader start with the Bolt-Action Rifle and 10 rounds. On a loss, all cash goes against everything owed on the loan (open payments and late fees first, then interest, then principal) before the loss screen totals what is still owed; shown as cash you had, taken for the debt, land loan still owed; display only, nothing saved. See Money_System.md and Difficulty_System.md.

---

# Back-fill, 2026-10-24 to 2026-11-08

Written by Claude on 2026-11-08 from `06_AI_Collaboration/Current_Task_List.md`, which is the primary record (it holds the full wording, commit hashes and playtest results). Entries are in date order. Anything marked "proposed" was a recommendation Mike accepted as proposed unless noted.

**2026-10-24 - Skills and Progression drafted and parked (Mike)**
`Skills_and_Progression.md` v0.1 was drafted (four v1 skills: Felling, Digging, Mining, Hauling; three tiers; time saved, not cash). Mike: "yes, defer that decision". Whether to lift the post-Alpha deferral for gathering and carry skills waits until after Money Phase D2 sets the tier numbers. Nothing to build.

**2026-10-24 - Income measuring: lighter first round (Mike)**
A full year per tier is about 56 real hours, too much for a first pass. First round: finish Spring of year 1 on Settler and Pioneer (5 acres), then send the numbers. Homesteader and Settler 10-acre wait. Mike's observation: too few ways to earn when only furs and hides sell, so the next step is building income sources (Ash first, then the wood and fiber workshop), then measuring again. No F12 and no cash keys during measuring runs.

**2026-10-25 - Early income targets and market rules (Mike, answers to Early_Income_Design questions)**
Target: every 28-day month covers its own payment (about $20 a game day). Firewood burning leaves Ash (1 per 3); station-made goods sit in a milder "Handmade wares" market group (about 6% price drop, floor 0.25) while instant recipes keep 10% and floor 0.1; no "what sells well today" hint line; a sell-surplus button with a player-set keep amount per item (saved with the game); logs get their own market group. Bigger parcels are a later-game goal, not an early-income target. Correction recorded: the $112,000 loan ($1,070.33 a payment) is the Settler 10-acre loan, not a Homesteader loan.

**2026-10-25 - Locked parcels (Mike)**
New Game shows the 10-acre and 40-acre cards locked with the reason "Needs more income than the early game offers. Unlocks later."; only 5 acres is selectable. Built and pushed (ad2c226).

**2026-10-25 - Ash details, crafting amount, Campfire removal (Mike)**
Sticks leave 1 Ash per 12; Ash is collected with a separate key (works while the fire is lit or out); Logs and Branches also leave Ash by burn hours (Branches 1 per 6, Logs about 1.7 each); Ash held in a dismantled hearth or grill is returned. Crafting gets an amount stepper per recipe (default 1, Shift-click for max). A plain Campfire can be removed: unspent fuel and held Ash come back in full, plus half the build materials rounded down.

**2026-10-25 - Pioneer measuring run next; trap counters undecided (Mike)**
Pioneer 5 acres, full Spring Y1 with Ash and kindling in the game, 2 to 3 real hours. The item-level ledger export Part 1 was approved; Part 2 (trap counters) stays undecided.

**2026-10-26 - Maple first, sizing (Mike)**
Maple before Potash. About 1 in 10 large hardwoods are tappable (about 290 trees, not 1 in 3). Felling a tapped tree is allowed and refunds the Spile and Bucket.

**2026-10-27 - Maple finding aids (Mike)**
After difficulty finding trees: a floating marker over each tappable Maple while a Spile is equipped (from the start, not always-on, not discovery-only), then expanded to markers on the player's own tapped trees visible from a distance. Built (Maple C and D).

**2026-10-27 - Potash and Lye (Mike)**
Potash sells for $16. Lye is sellable at a low price, so per Ash the order is raw Ash < Lye < Potash. The Leach Hopper works passively over one game day.

**2026-10-27 - Tripod replaces the carried Cooking Pot (Mike)**
The Tripod is an add-on to an existing Campfire (one per fire; dismantling returns the Tripod and the Pot). The Potash Kettle becomes a second vessel hanging on the same Tripod (still $25, buy-only). Boiling water and sap move onto the Tripod.

**2026-10-27 - Teaching the player (Mike)**
The Potash B playtest passed 28 of 28, but only because Mike had the checklist; nothing in the game taught the chain. Decision: teach with both in-the-moment hints (item help text and prompts that name the next step) and a Journal Know-how section. Built as the Know-how journal.

**2026-10-31 - Hand Cart, wood, regrowth, Workshop (Mike, plan decisions accepted as recommended)**
Hand Cart phase 2 (pushing) decisions D1 to D12 accepted. One Log makes 2 Wooden Wheels. Tree regrowth D1 to D11: a felled tree sprouts after 14 days and grows linearly to full size over 112 days, no season gating, Logs proportional to size, shrubs regrow, the stump stays until growth 0.5 (a Shovel can clear it earlier), the Pick Axe stays rock-only, windthrow only at growth 0.75 or more. Workshop W1(a) decisions D1 to D12 accepted, plus clearance against a parked cart and a Workshop radius for tree sprouts.

**2026-11-02 - Workshop prices fine; cart does not feed the bench (Mike)**
Workshop prices stay as designed for now (no price cut, no Pioneer re-measure at that time). The cart does not feed the bench automatically; materials are a manual transfer by R or the Workshop screen. Workshop a3 dropped.

**2026-11-02 - Pottery (Mike)**
A Potter's Shed (shape and dry on its own shelf) plus a separate Kiln (fire a batch with Firewood), hand-built first, Kick Wheel later as an upgrade of the same shed. Drying takes 1 to 3 game days with no random cracking; a piece cannot be fired until dry. The Clay Cooking Pot works as a Cooking Pot on the Tripod. Sand and Clay come from the creek banks (painted terrain layers).

**2026-11-04 - Digging idea and metalwork (Mike)**
Idea logged: dig into the ground and mine ores, only where realistic (not planned). Decision: ores can be bought at the Trading Post, so the metal chain does not depend on terrain digging; real digging stays a separate later feature. Metal plan: all 8 recommendations accepted (one roofed Smithy; iron Pick Axe and Shovel take 25 percent fewer blows; Charcoal is the only fuel; 10-hour smelt; first goods Hinges, Hooks and Nails plus iron tools; no bloom step; no per-visit ore limit; Charcoal sellable at $1). A later phase M5 would add building uses for iron. After playing M4, Mike confirmed 25 percent feels right: iron tool factor 0.75 stays.

**2026-11-04 - Art swap-point rules (Mike)**
All 10 art-readiness decisions accepted: art loads from `Resources/Art/<id>.prefab` with ids `structure_`, `item_`, `tool_`, `effect_`; named children carry state; dynamic contents stay primitives at anchors; finished structures only; Kiln is the pilot; art numbers stay in rules until anchors replace them. Spec: `Art_Readiness_and_Placeholders.md`.

**2026-11-04 - Hands and tools in hand (Mike)**
All 8 decisions accepted: a custom Viewmodel layer with its own FOV and depth clear, pure pose math, swap points `hands_player` and `tool_<itemid>`, no wind-up at first. Follow-up 2026-11-05: swinging tools get a visual-only pull-back with no gameplay delay (the blow, sound, count and stamina still happen at the click). Spec: `Hands_and_Held_Tools.md`.

**2026-11-05 - Art style decided: realistic PBR (Mike)**
After reviewing the reference folder (reference only, nothing usable as game art), the style is realistic with physically based materials. The full list is `Art_Needed_List.md`.

**2026-11-05 - Melee (Mike)**
The Knife alternates slash and stab on each click (no extra key). Fists never kill (stun and startle only); only the Knife, traps, bows and the rifle kill. Small game only; animals cannot hurt the player. Build order: rest of the tools in hand (M0c), remove the equipped-tool line (M0d), then melee (M0e). Spec: `Melee_Fists_and_Knife.md`.

**2026-11-06 - Hands v3 approved (Mike)**
After rejecting tube fingers, Mike approved a one-piece hand rebuild ("if this is the best we can do, then they're fine"), later reshaped from photos of his own hand with a photo-based skin texture. Hands are the only real art in the game so far.

**2026-11-07 - Melee Know-how and rifle pose (Mike)**
The Melee Know-how entry unlocks at the start of every new game (the starting Knife counts). The rifle held pose is accepted as a placeholder; the stock and shoulder seat are revisited when a real rifle model replaces the blocky one.

**2026-11-07 - PlayerLoop message closed**
The "PlayerLoop internal function has been called recursively" console message was traced to the Unity MCP package's synchronous screenshot call (editor only, cannot affect a built game). No game change.

**2026-11-07 - Finish Money Phase D2 by re-measuring Pioneer first (Mike)**
The last full-year Pioneer run (2026-10-28 to 10-29) predates the Workshop, pottery and metal. A new Pioneer 5-acre run is being played (`Pioneer_Remeasure_Checklist.md`); then Claude proposes the D2 tier numbers and Mike decides.

**2026-11-08 - Documentation brought up to date for a possible second developer (Mike)**
`Project_Overview_for_Developers.md`, `AI_Project_Brief.md` v2.0, `Unity_Architecture.md` v2.0 written; GDD v2.1 proposed (see `GDD/GDD_v2.1_Proposed_Changes.md`).

**2026-11-08 - GDD v2.1 applied (Mike)**
Mike approved the proposed GDD changes: the Alpha 0.1 scope lock and Development Rule are retired and replaced by roadmap-driven scope (farming, large livestock, powered vehicles, solar arrays and automation stay deferred until Mike chooses them); milestones live in the roadmap only; the vision is unchanged but now states the move past the primitive level to modern systems. The GDD file keeps its name (Homestead_GDD_v2.0.md) with a v2.1 revision note. Details: GDD/GDD_v2.1_Proposed_Changes.md.

**2026-11-08 - Real 365-day calendar, consolidation first, player time control (Mike)**
After a read-only calendar audit, Mike chose option D: a real 365-day calendar with 12 real-length months and no leap years (Spring March to May, Summer June to August, Fall September to November, Winter December to February), plus step 0 (consolidate every duplicated 28/112 constant so one calendar source drives everything, no behaviour change) and step 3 (a player-facing time control: sleep skip, wait, fast-forward). The land payment becomes monthly, bills fall on the 1st, and the loan becomes a true 15 game years. Reason: real-world knowledge (frost, planting, sap and hunting dates) transfers, the money becomes honest (about $6,876 a year in payments against about $6,880 earned at $20.48 a game day), and farming and winter get realistic length. Cost accepted: about 182 real hours per game year at 30-minute days, and a large change (about 20 to 25 files, 80 to 120 tests, old saves incompatible without migration). Build order: plan first by Claude Code, then step 0, then the calendar, then the time control, after Mike finishes the current Pioneer re-measure run. Record: Calendar_and_Time_Scale_Options.md. Modern systems and monthly bills wait on this (Modern_Systems_and_Services.md).

**2026-11-08 - Calendar build plan decisions (Mike)**
First land payment July 1 (day 122, close to today's day 112). Monthly bills (phone, internet, utilities) are charged on the 15th, so they do not land on the loan's due day, the 1st. Phase 1 (the real calendar) is one build, not split. Old-calendar saves are labeled and refused, with a one-off conversion of one named save (slot 4, named by Mike). Accepted as recommended: statements stay one per season, wood seasoning is converted to 28 days, tree regrowth is sprout 14 days, full at 365, steps of 25, the outhouse pit stays a 28-day gameplay tunable, and the player time control starts with Wait and Sleep skip (2a) before a live fast-forward (2b). Build order: Phase 0 (one calendar source, no behaviour change), Phase 1 (real calendar), Phase 2a, each with its own playtest, after the Pioneer re-measure run. Record: Calendar_and_Time_Scale_Options.md.

**2026-11-10 - Real calendar Phase 1 built and playtested (Mike, Claude Code)**
The real 365-day calendar is in the game: March to February, real month lengths, seasons of 92, 92, 91 and 90 days, dates like "Mar 1, Year 1". The land loan is monthly on the 1st from July 1 (day 122), 180 payments; the $60,000 payment is $573.39, which is $18.85 a game day. Old-calendar saves are labeled and refused, and are copied to Saves\_old_calendar before being overwritten. Slot 4 was converted to a new slot 5. The loan banners now clear when a payment is settled. 2,077 of 2,077 tests pass and Mike's 16-step playtest passed 16 of 16. The Pioneer re-measure waits until after Phase 2a (Wait and Sleep time control). Summer heat waves are not retuned.
**2026-11-10 - Phase 2a Wait control built and playtested (Mike, Claude Code)**
A player-facing Wait (key Z: 1, 3 or 6 hours, until dawn, until dusk) is in the game. Wait and sleep share one world-passing loop, and sleep results match the old loop in tests. Wait stops early for a low stat, severe weather, a rising creek, a dying fire, a trap catch or a notice, and refuses to start in water or with a stat already low. Torch and lantern fuel now burn during sleep (kept as built). Danger has a rule and hook but no source yet. The slot-4 conversion tool was deleted. 2,177 of 2,177 tests pass and the 16-step playtest passed 16 of 16. Next: the Pioneer re-measure on the final calendar.
**2026-11-10 - Pioneer gets a longer loan term; early income to be raised (Mike)**
Mike's guess is the Pioneer earns about $2 a game day (the last measured run was $5.58), against a payment of $18.85 a day, so the gap is too big for the tier levers alone. Decided: do both a cheaper Pioneer loan and higher early income. For the loan, Mike chose a LONGER TERM over a cheaper or smaller parcel: planned 30 years (360 payments) at the same 8%, about $440.26 a month, $14.47 a game day (total interest about $98,500 against about $43,200 on 15 years). Going to 40 years would only reach about $13.72 a day, so the term alone is not enough and the early-income ramp still has to do most of the work. Not built: the term is held until after the Pioneer re-measure, so all the tier levers are set once. Settler and Homesteader terms are undecided.
