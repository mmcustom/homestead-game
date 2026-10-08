# Homestead
## Item Data v1.0

Status: Design Draft — first balancing pass, pending Mike's confirmation (same propose-then-confirm pattern as Livestock/Plants/Wildlife/Fishing)

---

# Purpose

InventoryManager.cs needs a real `ItemDefinition` ScriptableObject (id, category, weight in kg, shelf life in days) for every item the game currently produces, and none exist yet. This document is that master list — the concrete data Inventory_System.md's own Design Rule 4 calls "a balancing pass, not fixed by this document," now that the core loop is playable enough to actually balance.

Every item below already exists conceptually in a confirmed doc (a Plants/Livestock/Wildlife/Fishing sheet, or a Game_Systems doc's own examples). This file doesn't invent new resources — it converts "3 units per patch" into the numbers `ItemDefinition` actually needs.

Weight and shelf life are new numbers, proposed here for the first time, grounded in realistic scale but not yet confirmed. `—` means non-perishable (doesn't use the Spoilage System).

---

# Resources — Foraged Plants

| id | Source | Weight (kg) | Shelf life (days) |
|---|---|---|---|
| blackberries | Berries.md | 0.3 | 3 |
| raspberries | Berries.md | 0.3 | 3 |
| wild_apples | Fruit.md | 0.4 | 14 |
| pawpaw | Fruit.md | 0.3 | 4 |
| hickory_nuts | Nuts.md | 0.5 | 60 |
| walnuts | Nuts.md | 0.5 | 60 |
| chestnuts | Nuts.md | 0.5 | 30 |
| dandelion | Edible_Plants.md | 0.2 | 2 |
| wild_onion | Edible_Plants.md | 0.2 | 5 |
| cattail | Edible_Plants.md | 0.3 | 3 |
| tall_grass | Trapping_System.md (Sourcing Cordage) | 0.1 | — |
| morel | Mushrooms.md | 0.2 | 5 |
| dryads_saddle | Mushrooms.md | 0.3 | 4 |
| oyster_mushroom | Mushrooms.md | 0.3 | 4 |

Shelf life follows each species' own confirmed preservation notes: Wild Apples' "stores well" reputation gets the longest fresh window in this category, Pawpaw and the Berries the shortest (matching their "eat it soon" framing), Nuts the longest overall per their confirmed long-natural-shelf-life role.

---

# Resources — Hunting, Trapping, and Fishing

| id | Source | Weight (kg) | Shelf life (days) | Notes |
|---|---|---|---|---|
| small_game_meat | Small_Game.md | 0.4 | 3 | Shared by Rabbit and Squirrel — confirmed identical in Small_Game.md. Proposed here to also cover culled livestock Rabbit meat, extending Rabbits.md's confirmed Small Furs-sharing to meat too — flagging this extension for your confirmation, it wasn't explicitly stated for meat. |
| small_furs | Small_Game.md, Rabbits.md | 0.2 | — | Shared by wild Rabbit, wild Squirrel, and livestock Rabbits — confirmed sharing in Rabbits.md. |
| turkey_meat | Medium_Game.md | 1.5 | 3 | |
| waterfowl_meat | Medium_Game.md | 1.3 | 3 | |
| feathers | Medium_Game.md | 0.05 | — | Shared by Turkey and Waterfowl. |
| venison | Large_Game.md | 2.5 | 3 | A full 8-unit Deer harvest is 20 kg — deliberately close to the 30 kg encumbrance line, so hauling a whole deer home in one trip is a real decision, not a given. |
| deer_hide | Large_Game.md | 3.0 | — | |
| deer_antlers | Large_Game.md | 1.0 | — | |
| sinew | Large_Game.md / Medium_Game.md (field dressing byproduct) | 0.05 | — | Trapping_System.md's Sourcing Cordage — 2 per deer, 1 per turkey; day-to-day Cordage source alongside Tall Grass. |
| bluegill | Bluegill.md | 0.3 | 2 | |
| crappie | Crappie.md | 0.5 | 2 | |
| bass | Bass.md | 0.6 | 2 | |
| catfish | Catfish.md | 0.8 | 2 | |

Fish get the shortest shelf life of any Resource (2 days) — fish spoils fastest of anything the game produces, consistent with every species' Preservation section flagging Cook/Smoke/Dry as standard, expected practice rather than optional.

---

# Resources — Livestock

| id | Source | Weight (kg) | Shelf life (days) |
|---|---|---|---|
| goat_milk | Goats.md | 1.0 | 2 |
| chicken_eggs | Chickens.md | 0.1 | 21 |
| chicken_meat | Chickens.md | 0.8 | 3 |

Milk gets the shortest shelf life in the game (dairy spoils fast); eggs get one of the longest (real eggs keep for weeks unrefrigerated), matching Chickens.md's "steady, unconditional" identity.

---

# Materials

| id | Source | Weight (kg) | Shelf life |
|---|---|---|---|
| lumber | Building_Housing_System.md; **confirmed source 2026-09-26 (Mike):** a Lumber Mill (new, unbuilt production building) or purchased at the Trading Post (Difficulty_System.md — **built and tested 2026-09-26**, but its catalog doesn't stock Lumber yet; confirmed 2026-09-26 (Mike) as an agreed addition, not yet sent to Claude Code) until the player has their own mill — this is also the "Board" material requested for the Wooden Hoe (Small Cabin and the Primitive Shovel no longer need it — see Building_Housing_System.md's Small Cabin Requirements, resolved 2026-09-26). Quality tiers by wood species confirmed (Pine → Birch → Oak → Walnut, ascending) but not yet modeled — Wood_Gathering_System.md's trees have no species tag today | 5.0 | — |
| stone | Stone_Gathering_System.md (picked up by hand, scattered property-wide, denser near water/cliffs) | 5.0 | — |
| clay | Building_Housing_System.md's Small Cabin stove/fireplace furnishing. **Built and tested 2026-09-27 (Claude Code):** a creek/pond bank dig site, as proposed — the Primitive Shovel (AxeTool) digs it straight from dry ground close to the water's edge (checked against the existing water-surface raycast, not a placed deposit), so unlike the South Ridge rock outcrop it never runs out | 3.5 | — |
| cordage | Trapping_System.md (crafted: 3 Tall Grass, 2 Cattail, or 1 Sinew) | 0.3 | — |
| firewood | Core_Survival_System.md (Fire System) | 1.5 | — |
| arrows | Hunting_System.md (Recurve Bow ammunition) | 0.05 | — |
| rifle_rounds | Hunting_System.md (Bolt-Action Rifle ammunition) | 0.02 | — |
| sticks | Wood_Gathering_System.md (chopping trees) | 0.1 | — |
| branches | Wood_Gathering_System.md (chopping trees) | 1.0 | — |
| logs | Wood_Gathering_System.md (chopping trees) | 8.0 | — |

Stone's 5 kg matches the exact test value Claude Code already used when verifying the 45 kg hard cap ("nine 5 kg stones fit, the tenth was refused") — carried over here rather than picking a different number. Source updated 2026-09-26 once Mike requested a real gathering method (Stone_Gathering_System.md, Design Draft, not yet built) — previously just a placeholder "(general building material)" with no way to actually obtain it. Firewood added 2026-09-25, matching Claude Code's Fire Building implementation: 40 deadfall piles give 2-3 Firewood each, one Firewood burns for 2 in-game hours. Weight is a first proposal, same status as everything else in this doc. Arrows and Rifle Rounds added 2026-09-25 with Phase 2's Hunting implementation — grouped here with the other stackable, non-perishable materials rather than Tools, since they're consumed by use (shot) rather than equipped. Claude Code's own note: nothing on the property currently produces either — per Hunting_System.md, ammunition is meant to be purchased later (ties into Economy_System.md once that's built) — so for now they only come from the F12 debug test kit. Weights are rough first proposals (a wood arrow, a rifle cartridge), same unlocked status as everything else here.

Sticks/Branches/Logs added 2026-09-26 with Wood_Gathering_System.md (Design Draft, requested 2026-09-26, not yet built) — the three yields from chopping down a tree with the Axe. All non-perishable, same as the other Materials here. Weights are first-pass proposals (a stick vs. a branch vs. a full log), scaled relative to each other and to Firewood/Lumber; Claude Code can adjust by feel once the feature is actually built.

---

# Tools

Only one can be equipped at a time per InventoryManager.cs; don't spoil. Correction (2026-09-23, per Claude Code's implementation notes): Tools stack in inventory like every other item (carrying two Rabbit Snares is one stack of 2) — this doc originally said "not stacked," which doesn't match InventoryManager's actual behavior. Stacking has no effect on the one-equipped-tool rule; it's just how the carried quantity displays.

| id | Source | Weight (kg) |
|---|---|---|
| axe | Inventory_System.md | 2.0 |
| recurve_bow | Hunting_System.md | 1.5 |
| bolt_action_rifle | Hunting_System.md | 3.5 |
| fishing_rod | Fishing_System.md (Rod and Reel) | 1.0 |
| cane_pole | Fishing_System.md | 0.8 |
| rabbit_snare | Trapping_System.md | 0.3 |
| box_trap | Trapping_System.md | 2.0 |
| fish_trap | Fishing_System.md | 3.0 |
| bucket | Water_System.md (hand carrying — water/liquid transport only, not boiling) | 1.5 |
| flint_and_steel | Core_Survival_System.md (Fire System) | 0.2 |
| bow_drill | Core_Survival_System.md (Fire System, Bow Drill). **Added 2026-10-07 (Mike), first pass (Claude, not yet confirmed):** crafted 2 Sticks + 1 Cordage, lights a Campfire without Flint and Steel but can fail. Not built yet. | 0.4 |
| stone_pickaxe | Stone_Gathering_System.md (crafted: 2 Sticks + 1 Cordage + 1 Stone) | 2.5 |
| primitive_shovel | Wood_Gathering_System.md (crafted: 3 Sticks + 1 Cordage + 1 Stone — **resolved 2026-09-26 (Mike): recipe unchanged**; joins the Axe/Pick Axe's Primitive → Iron → Steel tiering instead of switching to Board) | 2.0 |
| metal_cooking_pot | Water_System.md (Boiling vessel, replaces the Bucket — new-game starting kit, NOT craftable; F11 grants one to existing saves) | 1.0 |
| pouch_bag | Primitive_Storage_System.md (crafted: 1 Deer Hide + 1 Cordage) | 0.4 |
| tent | Building_Housing_System.md "First Build: Sleep System" (new-game starting kit / Trading Post item, NOT craftable) | 4.0 |
| sleeping_bag | Building_Housing_System.md "First Build: Sleep System" (new-game starting kit, NOT craftable) | 1.5 |
| primitive_axe | Wood_Gathering_System.md (**committed and pushed 2026-09-26** — code `67d85da`: 2 Sticks + 1 Cordage + 1 Stone, as proposed) | 2.5 |
| hammer | Building_Housing_System.md / Difficulty_System.md — purchasable/kit item, Homesteader starting kit, $10 at the Trading Post. **Built and tested 2026-09-27 (Claude Code):** now also craftable (2 Sticks + 1 Cordage + 1 Log, as proposed) and functional — equipping it gates building a Small Cabin, same as the Axe/Pick Axe/Shovel gate their own actions. Weight corrected to match the shipped item asset (was listed as 2.0 here, pre-dating this row's own note) | 1.2 |
| knife | Hunting_System.md (**committed and pushed 2026-09-26** — code `67d85da`: 1 Stick + 1 Cordage + 1 Stone; **confirmed 2026-09-26 (Mike): also joins the new-game starting kit**, F11 grants one to existing saves, tested in Play Mode both ways before pushing) | 0.5 |
| wooden_hoe | Wood_Gathering_System.md (crafted: 2 Sticks + 1 Cordage + 1 Stone — **confirmed 2026-09-26 (Mike): "keep it like the stone sickle"**; Future, no Gardening/Farming system exists yet) | 1.5 |
| stone_sickle | Wood_Gathering_System.md (crafted, proposed: 2 Sticks + 1 Cordage + 1 Stone — Future, no Gardening/Farming system exists yet) | 1.0 |
| tarp | Difficulty_System.md (**committed 2026-09-26 as `a815628`, not yet pushed:** new-game starting kit / Trading Post item ($8), NOT craftable; carried, blocks 90% rain/30% wind when sleeping on a bare Sleeping Bag) | 0.8 |
| canteen | Difficulty_System.md (**committed 2026-09-26 as `a815628`, not yet pushed:** new-game starting kit / Trading Post item ($5), NOT craftable; 2 L water container, equips and fills at water sources like the Bucket) | 0.4 |
| torch | Core_Survival_System.md (Portable Lighting). **Built and tested 2026-10-03 (Claude Code):** crafted 1 Sticks + 1 Cordage, confirmed and functional. | 0.5 |
| lantern | Core_Survival_System.md (Portable Lighting). **Built and tested 2026-10-03 (Claude Code):** crafted 2 Clay + 1 Cordage (Claude Code's own material call, not Mike's), burns the new `lamp_oil` consumable rather than being disposable. | 1.0 |
| lamp_oil | Core_Survival_System.md (Portable Lighting). **Built and tested 2026-10-03 (Claude Code):** Trading Post item, $3/bottle, not craftable; 8 hours of Lantern light per bottle, tank holds 2 bottles (16 hours). | 0.5 |

Torch and Lantern added 2026-10-03 (Mike) — new portable lighting Tools, alongside a Flashlight left Future pending Power_System.md defining a battery item. **Built, tested, and reported 2026-10-03 (Claude Code)** — **committed `fd5bfe4` and pushed (confirmed 2026-10-04);** weights above (0.5/1.0/0.5 kg) are Claude's original first-pass proposals, unchanged by Claude Code's build.

Flint and Steel added 2026-09-25, matching Claude Code's Fire Building implementation — the ignition source the doc's Fire System requires, carried (not equipped) to light a built campfire. Stone Pick Axe and Primitive Shovel added 2026-09-26 (Mike requested both) — the Pick Axe's materials are Mike's own (Sticks, Cordage, Stone), the Shovel's a first-pass proposal using the same three (not confirmed by Mike). **Built, tested, committed and pushed 2026-09-26 (Claude Code)** as part of commit `4145888` (`8b3ea6e` carries the doc updates): both crafted from a new Craft row in Inventory — Pick Axe 2 Sticks/1 Cordage/1 Stone, Shovel 3 Sticks/1 Cordage/1 Stone. Weights are still first-pass proposals.

Metal Cooking Pot and Pouch/Bag added 2026-09-26 (Mike). The cooking pot reverses the Bucket's role in Water_System.md's already-shipped Boiling mechanic — Mike pointed out a wooden Bucket would burn over a campfire, so Boiling should require this new pot instead, narrowing the Bucket to water/liquid transport only; this is a real correction to committed code, not just a doc update. The Pouch/Bag is a new carrying Tool parallel to the Bucket but for non-liquid resources, feeding the new primitive storage types in Primitive_Storage_System.md; Mike didn't specify materials, so Deer Hide + Cordage was proposed as a first pass. **Built, tested, committed and pushed 2026-09-26 (Claude Code)** as part of commit `4145888` (`8b3ea6e` carries the doc updates): the Pouch crafts from exactly the proposed 1 Deer Hide + 1 Cordage. **Claude Code's own call, flagged for Mike to confirm:** the cooking pot is NOT craftable — nothing on the property can produce metal, so it's in the new-game starting kit alongside the Bucket (F11 grants one to existing saves); a craftable clay-pot alternative was offered as a quick change if Mike would rather it be earnable. Both weights are still first-pass proposals.

Tent and Sleeping Bag added 2026-09-26 — Building_Housing_System.md's "First Build: Sleep System." **Built, tested, committed and pushed 2026-09-26 (Claude Code)** as part of commit `4145888` (`8b3ea6e` carries the doc updates): both are new-game starting-kit items, not craftable, same status as the metal cooking pot; F11 grants both to existing saves, including Mike's own. Weights (4.0 kg Tent, 1.5 kg Sleeping Bag) are Claude's own first-pass proposals — Claude Code's report didn't specify a weight for either, so these aren't yet confirmed by testing.

Primitive Axe, Hammer, and Knife added 2026-09-26 (Mike). **Built, tested, committed and pushed 2026-09-26 (Claude Code)** as commit `67d85da` (13 files, code) with the matching doc updates in `b6cf76d`, both on `master` and pushed to GitHub: Primitive Axe (2 Sticks/1 Cordage/1 Stone, does everything the metal Axe does but 1.5x slower) and Knife (1 Stick/1 Cordage/1 Stone, gates Field Dressing and Trapping's catch-processing — see Hunting_System.md and Trapping_System.md) are both built and confirmed working on a copy of Mike's save. **Both open questions resolved 2026-09-26 (Mike):** "smooth it out" — the Knife joins the new-game starting kit; "i agree with claude code" — the Hammer stays out of this batch and out of Item_Data.md as a real row until Building/Housing gets its first Unity implementation. Before pushing, Claude Code ran the Knife starting-kit change in Play Mode two ways: F11 on a copy of Mike's own save (added the Knife plus the Sleeping Bag/Tent/Cooking Pot his save predated) and a brand-new game (started with Flint and Steel, Bucket, 3 Cordage, the Axe, Cooking Pot, Sleeping Bag, Tent, and the Knife — 12.5 kg total). Both passed; Mike's save was restored and hash-checked afterward. This batch is fully closed out — nothing left open. Iron/Steel tiers for the Axe and Pick Axe are still blocked on the Mining_Metalworking_System.md chain (mining, charcoal, bloomery, forge/anvil/workbench).

Wooden Hoe and Stone Sickle added 2026-09-26 (Mike — "the same for a wooden hoe and stone sickle for harvesting crops later"), Future/not buildable: no Gardening/Farming system exists anywhere in the project to harvest crops from. Recipes are Claude Code's own reading of "the same," following each existing tool's material pattern (Hoe like the Shovel's Board/Lumber-based build, Sickle like the Axe/Pick Axe's Stone-bladed build) — see Wood_Gathering_System.md's "Tools: Wooden Hoe and Stone Sickle" section.

Tarp and Canteen added 2026-09-26 — Difficulty_System.md. **Built, tested, and committed 2026-09-26 as `a815628` on master (not yet pushed):** both new-game starting-kit/Trading Post items, not craftable, same non-craftable status as the metal cooking pot and Tent/Sleeping Bag; F11 now also grants a Canteen. **Tarp weight confirmed at 0.8 kg** (Claude Code's own pick, within Mike's requested 0.5–1 kg range) — lowers the Homesteader kit from 23.1 kg to about 22.4 kg; not re-verified in Play Mode after the change, so worth a glance next time Mike's in there. **The Hammer's status changed with this batch:** it's now a real row above (not held out of Item_Data.md as previously planned) because Difficulty_System.md's Homesteader kit and Trading Post both needed a real Hammer item to grant and sell — it remains non-functional, same non-building-use status as before, just no longer absent from the data. The Trading Post's full buy/sell price list (including sellable Resources like Hide, Furs, Logs, Stone, and foraged plants) lives in Difficulty_System.md rather than being duplicated here, to avoid the two drifting out of sync.

**Resolved 2026-09-26 (Mike):** Board is the existing `lumber` Material, not a separate item — "a board is a type of lumber," sourced from a new Lumber Mill or Trading Post purchase, with quality tiers by wood species (Pine → Birch → Oak → Walnut). That closes the near-duplicate-material question. The follow-up sequencing question it raised (Lumber/Boards needing two unbuilt systems) is also resolved: **"agreed, keep your shovel recipe; and include metal versions for the future upgrades"** — the shipped Primitive Shovel recipe (3 Sticks/1 Cordage/1 Stone, commit `4145888`) stays exactly as shipped, no code change, and the Shovel instead gets Iron/Steel upgrade tiers later, joining the Axe/Pick Axe's Primitive → Iron → Steel progression (same Iron/Steel material gap as those two — see Wood_Gathering_System.md's Tiering notes). The Wooden Hoe's still-Future proposed recipe (Board-based) no longer has a live Shovel recipe to mirror, flagged as worth a fresh look whenever Gardening/Farming actually gets built.

---

# Consumables

Deliberately thin for now.

| id | Source | Weight (kg) |
|---|---|---|
| water | Water_System.md | 1.0 |
| water_excellent | Water_System.md (Water Quality Levels) | 1.0 per litre |
| water_good | Water_System.md (Water Quality Levels) | 1.0 per litre |
| water_questionable | Water_System.md (Water Quality Levels) | 1.0 per litre |
| water_unsafe | Water_System.md (Water Quality Levels) | 1.0 per litre |

The four quality-tagged water items added 2026-09-25, matching Claude Code's Water Collection implementation: carried water keeps its source's quality as a separate item rather than one generic `water` id, so purification (boiling, not yet built) has something concrete to act on. All non-perishable, no illness risk modeled yet — that's the Water Purification entry's job. The original generic `water` item is untouched for now; Claude Code's own note suggests it could become Purified Water's id once that system lands, but that's a call for whoever builds it, not decided here.

**Confirmed 2026-09-25 (Claude Code, Phase 3 — built and tested, not yet committed):** Water Purification landed, and the original generic `water` id is exactly what became Purified Water, at 0% illness risk — no new id needed. `water_excellent/good/questionable/unsafe` each now carry a real per-litre illness risk on drink/eat: Excellent 0%, Good 5%, Questionable 20%, Unsafe 45% (see Water_System.md's Water Quality Levels for the confirmed numbers). Eating also landed, giving every Consumable and Resource food item a real Food/Water restore value and, where relevant, that same illness-risk shape — the raw items below are Claude Code's first-pass proposal, for Mike to judge by feel in Play Mode:

| id | Food value | Water value | Sickness risk |
|---|---|---|---|
| blackberries / raspberries | 6 | 3 | none |
| wild_apples | 8 | 5 | none |
| pawpaw | 8 | 3 | none |
| chestnuts | 12 | 0 | none |
| hickory_nuts / walnuts | 15 | 0 | none |
| dandelion / wild_onion / cattail / morel / dryads_saddle / oyster_mushroom | 3–6 | 1 | none |
| goat_milk | 6 | 15 | none |
| chicken_eggs | 6 | 1 | none |
| venison | 30 | 0 | 30% |
| turkey_meat | 22 | 0 | 30% |
| waterfowl_meat | 19 | 0 | 30% |
| chicken_meat | 15 | 0 | 30% |
| small_game_meat | 9 | 0 | 30% |
| bluegill | 6 | 0 | 20% |
| crappie | 9 | 0 | 20% |
| bass | 11 | 0 | 20% |
| catfish | 15 | 0 | 20% |

Nine new Cooked Consumable items were also added — a Cooked version of each of the raw meats and fish above (venison, turkey, waterfowl, chicken, small game, bluegill, crappie, bass, catfish) — each with no sickness risk, a bit lighter than its raw counterpart, roughly 4/3 the raw Food value (e.g. cooked venison is 40), and a longer shelf life: cooked meat keeps 5 days versus 3 raw, cooked fish 4 days versus 2 raw. Cooking (see Current_Task_List.md) produces these at a lit campfire; exact ids and weights weren't fully visible in what was relayed and should be confirmed against the actual `cooked_*.asset` files next time Claude Code touches this table. Design Rule 4 below, which scoped cooked variants out of this file until Preservation was defined, is superseded now that Cooking is actually built — see the note there.

Cooked, smoked, and dried food variants (Cooked Meat, Dried Berries, and similar) aren't included here — they're Core_Survival_System.md's Spoilage/Preservation mechanics to define, not something to invent as a side effect of this pass. Raw Resources above are enough to get InventoryManager populated and testable; Consumables can grow once that system gets its own numbers worked out.

---

# Design Rules

1. Every id here traces back to an already-confirmed doc — this file doesn't introduce new items, only the weight/shelf-life numbers those items need to exist as real `ItemDefinition` assets.

2. Weight and shelf life are a first proposal, not locked — same balancing-pass status as every other number in this doc set, revisit after playtesting.

3. Shared items (small_game_meat, small_furs, feathers) stay shared across every species that produces them — don't split them into per-species duplicates without updating this table and the source docs together.

4. Cooked/preserved Consumable variants belong to Core_Survival_System.md's Preservation System, not this file — don't add them here ahead of that system being scoped. **Superseded 2026-09-25 (Claude Code, Phase 3):** Cooking is now actually built (see Current_Task_List.md), producing nine real cooked_* Consumable items — this rule no longer holds as a blanket exclusion. Spoilage/aging itself still isn't built (the longer cooked shelf life is set on the items but nothing ages food yet), so full Preservation mechanics (smoking, drying) still belong to Core_Survival_System.md, not here.