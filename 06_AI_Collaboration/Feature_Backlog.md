# Homestead
## Feature Backlog v1.0

Status: Active — living doc, add to it as new Future System items are flagged in Game_Systems, Systems, Wildlife, Plants, or Livestock docs

---

# Purpose

Every Game_Systems, Systems, Wildlife, Plants, and Livestock doc has its own "Future System" or "Not Alpha 0.1" notes scattered through it. This document pulls every one of them into a single list so nothing gets lost or forgotten, and so post-Alpha-0.1 planning has one place to start from instead of fifteen.

This is a catalog, not a roadmap. Nothing here is scheduled, prioritized, or promised — it's everything that's already been explicitly deferred, in one place. Development_Roadmap.md's Stage 2–4 breakdown remains the actual staging plan; this doc feeds into that planning rather than replacing it.

---

# How to Use This Doc

Each entry links back to the doc that first flagged it, so the reasoning and any partial design thinking isn't duplicated here — just the pointer. When an item eventually gets scoped for real, that work happens in its source doc (or a new one), and the entry here gets marked Scoped or removed.

---

# Wildlife & Hunting

- **Wounded Animal Tracking** — blood trail → tracking → recovery, for a non-clean shot. Currently a lost/reduced harvest instead. Source: Hunting_System.md, Wildlife_System.md.
- **Predator Species** — Fox, Coyote, Bobcat. Livestock threats and ecosystem balance. Source: Wildlife_System.md.
- **Predator Hunting** — hunting the predator species above, once they exist. Source: Hunting_System.md.
- **Animal Population System** — hunting pressure, habitat destruction, weather, and resource availability actually shifting local populations over time (overhunting deer, overtrapping rabbits reducing future catches). Source: Wildlife_System.md.
- **Additional Hunting Weapons** — Crossbow, Shotgun (the intended Turkey/Waterfowl upgrade), Compound Bow, Black Powder Muzzleloader. Source: Hunting_System.md.
- **Constructed Ground Blinds and Tree Stands** — fixed hunting positions beyond natural cover. Source: Hunting_System.md.
- **Scent Control Mechanics** — beyond the current wind-direction awareness. Source: Hunting_System.md.
- **Poisonous Mushroom Species / Misidentification** — a real lookalike-risk system, with Morel/False Morel (05_Documentation/Plants/Mushrooms.md) already identified as the intended seed species. Source: Foraging_System.md, Plants/Mushrooms.md.

---

# Trapping

- **Cage Traps, Deadfall Traps, Water Traps, Advanced Fur Traps** — beyond the Alpha 0.1 Rabbit Snare and Box Trap. Source: Trapping_System.md.

---

# Livestock

- **Sheep** — Wool, Meat. Source: Livestock_System.md.
- **Pigs** — Meat, Land Clearing. Source: Livestock_System.md.
- **Cattle** — Milk, Beef. Source: Livestock_System.md.
- **Horses** — Transportation, Work Animals. Source: Livestock_System.md.
- **Detailed Diseases / Veterinary Systems** — beyond the current general health/neglect model. Source: Core_Survival_System.md.

---

# Water

- **Filtration** — a distinct purification method alongside Boiling. Source: Water_System.md.
- **Piped Distribution, Pressure Water Systems, Powered Pumps** — beyond Alpha 0.1's hand-carrying and cart-hauling. Source: Water_System.md.

---

# Fishing

- **Ice Fishing** — Winter's "fishing becomes difficult" note explicitly leaves this open. Source: Fishing_System.md.

---

# Player Systems

- **Swim** — a fourth movement state. Source: Systems/First_Person_Controller.md.
- **Tool Durability** — degradation, maintenance, and repair for tools. Source: Systems/Inventory_System.md.
- **Multiple Save Slots** — Alpha 0.1 uses a single slot. Source: Systems/Save_System.md.
- **Save Corruption / Failure Handling** — a backup or rolling save to protect against a corrupted file. Source: Systems/Save_System.md.

---

# Economy

- **Transportation Costs** — Fuel, Vehicle Maintenance, Trailer Repairs, once vehicles exist. Source: Economy_System.md.
- **Debt** — borrowing for wells, livestock, or buildings, with real risk attached. Source: Economy_System.md.

---

# Environment

- **Advanced Temperature Systems** — beyond the current Exposure System's tiered effects. Source: Core_Survival_System.md.
- **Beekeeping** — already anticipated as an Economy revenue source (Honey) without a system behind it yet. Source: Core_Survival_System.md, Economy_System.md.
- **Maple Syrup Production** — same situation as Beekeeping. Source: Core_Survival_System.md, Economy_System.md.

---

# Larger-Scope Deferrals (already tracked in Development_Roadmap.md)

These are bigger than a single feature and are already covered by Development_Roadmap.md's Stage 2–4 breakdown — listed here only as a pointer, not duplicated in detail:

- Farming (crops beyond current Foraging/Plants scope)
- Large Livestock Operations
- Vehicles
- Solar Arrays
- Automation

Per the GDD's Development Rule, none of these — or anything else in this backlog — should be designed or built until the core Alpha 0.1 survival loop is proven fun.

---

# Design Rules

1. This doc only catalogs what's already been explicitly deferred elsewhere — it never introduces a new feature idea of its own. A brand-new idea belongs in its own conversation with Mike first, then gets added here once flagged in a source doc.

2. When a source doc's Future System note changes or gets scoped for real, update the entry here to match — don't let this drift the way the Fur/Small Furs terminology did.

3. This is a catalog, not a priority order. Sequencing is Development_Roadmap.md's job.
