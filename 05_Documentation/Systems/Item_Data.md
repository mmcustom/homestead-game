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
| lumber | Building_Housing_System.md | 5.0 | — |
| stone | (general building material) | 5.0 | — |
| cordage | Trapping_System.md | 0.3 | — |
| firewood | Core_Survival_System.md (Fire System) | 1.5 | — |

Stone's 5 kg matches the exact test value Claude Code already used when verifying the 45 kg hard cap ("nine 5 kg stones fit, the tenth was refused") — carried over here rather than picking a different number. Firewood added 2026-09-25, matching Claude Code's Fire Building implementation: 40 deadfall piles give 2-3 Firewood each, one Firewood burns for 2 in-game hours. Weight is a first proposal, same status as everything else in this doc.

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
| bucket | Water_System.md (hand carrying) | 1.5 |
| flint_and_steel | Core_Survival_System.md (Fire System) | 0.2 |

Flint and Steel added 2026-09-25, matching Claude Code's Fire Building implementation — the ignition source the doc's Fire System requires, carried (not equipped) to light a built campfire.

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

Cooked, smoked, and dried food variants (Cooked Meat, Dried Berries, and similar) aren't included here — they're Core_Survival_System.md's Spoilage/Preservation mechanics to define, not something to invent as a side effect of this pass. Raw Resources above are enough to get InventoryManager populated and testable; Consumables can grow once that system gets its own numbers worked out.

---

# Design Rules

1. Every id here traces back to an already-confirmed doc — this file doesn't introduce new items, only the weight/shelf-life numbers those items need to exist as real `ItemDefinition` assets.

2. Weight and shelf life are a first proposal, not locked — same balancing-pass status as every other number in this doc set, revisit after playtesting.

3. Shared items (small_game_meat, small_furs, feathers) stay shared across every species that produces them — don't split them into per-species duplicates without updating this table and the source docs together.

4. Cooked/preserved Consumable variants belong to Core_Survival_System.md's Preservation System, not this file — don't add them here ahead of that system being scoped.