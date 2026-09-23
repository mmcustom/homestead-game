# Homestead
## Review 003 — Post-Batch Consistency Check v1.0

Status: Complete

Reviewer: Claude

Date: 2026-09-22

---

# Purpose

A full re-read of every doc in 05_Documentation after this session's large batch of new material (Livestock, Wildlife, Plants, Gameplay_Loops, Technical_Design, Research, and the three Systems docs), matching the pattern of Review_001 (initial GDD review) and Review_002 (Game_Systems consistency check). The goal: catch terminology drift and stale cross-references introduced or exposed by the new docs before they compound.

---

# Scope

Read in full for this pass: Core_Survival_System, Health_System, Water_System, Weather_System, Building_Housing_System, Discovery_System, Economy_System, Fishing_System, Trapping_System, Wildlife_System, Foraging_System, Season_System, Livestock_System, Hunting_System (all Game_Systems); First_Person_Controller, Inventory_System, Save_System (Systems); Save_Data_Model (Technical_Design); Core_Loops (Gameplay_Loops); all Livestock, Wildlife, and Plants species sheets authored this session.

---

# Finding 1 — "Fur" vs. "Small Furs" Terminology Drift

Mike's original correction ("trapping can also output small furs") was applied to Trapping_System.md's Harvestable Materials section, Core_Survival_System.md, and the Economy Integration section earlier this session — but the term didn't fully propagate. Found generic "Fur" still in place in five spots across four other docs, all referring to the same Rabbit/Squirrel resource:

- Wildlife_System.md — Small Game category's Uses list
- Hunting_System.md — Small Game entry in Huntable Species
- Economy_System.md — Early Survival Revenue examples ("Rabbit Fur, Squirrel Fur")
- Trapping_System.md — Rabbit Snare's own Purpose list (missed in the original pass; only Harvestable Materials was caught)
- Livestock_System.md — Rabbits section, in two places (Primary Benefits and Detailed Role Benefits — this one refers to livestock rabbit fur, which 05_Documentation/Livestock/Rabbits.md later confirmed shares the same Small Furs resource)

**Fixed.** All five now read "Small Furs," consistent with Trapping_System.md's Harvestable Materials, Core_Survival_System.md's Trapping summary, Inventory_System.md's Resources examples (which were already correct), and every Livestock/Wildlife species sheet.

---

# Finding 2 — Save_Data_Model.md's Stale Forward Reference

The Property Block's Livestock line said per-species stat sheets "will live in 05_Documentation/Livestock once written" — true when Save_Data_Model.md was authored, no longer true now that Goats.md, Chickens.md, and Rabbits.md exist with confirmed maturation, breeding cooldown, and yield fields.

**Fixed.** Updated to point at the three actual files and note that save-state fields should map to their confirmed values.

---

# Checked, No Issue Found

- Core_Survival_System.md's Water Sources and Trapping summaries still correctly point at Water_System.md and Trapping_System.md as sources of truth (Review_002's fix held).
- Health_System.md's Morale Decreases list (including "Overwork / Lack of rest") remains the single source of truth; Core_Survival_System.md's Morale section still correctly summarizes rather than duplicates.
- Weather_System.md's cross-references to Fishing_System.md (Cold Fronts) and Hunting_System.md (Wind) both check out against the actual content in those docs.
- Economy_System.md's Livestock economic sink (Goats, Chickens, Rabbits, Feed) and Early Homestead Revenue (Eggs, Goat Milk, Cheese) both match the confirmed Livestock species sheets.
- Building_Housing_System.md's Animal Structures (Chicken Coop, Goat Shelter, Rabbit Hutch) match the shelter requirements confirmed in the Livestock species sheets.
- Inventory_System.md's Resources examples already used "Small Furs" correctly (written after the original terminology fix).
- Hunting_System.md's Medium Game yield ("Moderate meat, Feathers") stays appropriately generic rather than hardcoding numbers — Medium_Game.md is correctly the source of truth for the actual 4/3 split.
- Discovery_System.md's Plants and Wildlife example lists (Hickory Groves, Walnut Groves, Deer Trails, Rabbit Habitats, Turkey Roosts, Waterfowl Areas) are illustrative, not exhaustive, and don't contradict the newer Livestock/Wildlife/Plants species sheets — no fix needed, but see Note below.

---

# Note — Not a Fix, a Possible Future Gap

Fishing_System.md's four species (Bluegill, Catfish, Bass, Crappie) have no per-species stat sheet the way Livestock, Wildlife, and Plants now do — there's no `05_Documentation/Fishing` folder. This wasn't part of what Mike asked to fill this session, so nothing was added, but if Fishing ever needs the same concrete-stats treatment, it would follow the same propose-then-confirm pattern used for the other three. Flagged for awareness, not action.

---

# Outcome

Five terminology fixes applied across four files, one stale cross-reference updated in a fifth. No open questions raised — everything in this pass was mechanical cleanup of decisions already made, not new design work.
