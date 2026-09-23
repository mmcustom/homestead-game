# Homestead
## Development Roadmap

Status: Active

---

# Purpose

Tracks where Homestead stands against its own GDD, so design work stays paced ahead of implementation without racing past what's been proven fun.

This is a design-documentation roadmap, not a Unity implementation schedule — 01_Unity_Project status is Copilot's domain and isn't tracked here.

---

# Current Phase

Pre-Production. Alpha 0.1 scope is locked per the GDD. Nothing beyond it should be designed or built until the core survival loop is proven fun, per the GDD's Development Rule.

---

# Alpha 0.1 Required Systems — Design Documentation Status

The GDD's "Current Alpha 0.1 Scope" lists 16 required systems. All are now covered by a design doc:

- First Person Controller — `05_Documentation/Systems/First_Person_Controller.md`
- Inventory — `05_Documentation/Systems/Inventory_System.md`
- Hydration — `05_Documentation/Game_Systems/Core_Survival_System.md`
- Hunger — `05_Documentation/Game_Systems/Core_Survival_System.md`
- Fire Building — `05_Documentation/Game_Systems/Core_Survival_System.md`
- Water Collection — `05_Documentation/Game_Systems/Water_System.md`
- Water Purification — `05_Documentation/Game_Systems/Water_System.md`
- Foraging — `05_Documentation/Game_Systems/Foraging_System.md`
- Fishing — `05_Documentation/Game_Systems/Fishing_System.md`
- Fish Traps — `05_Documentation/Game_Systems/Fishing_System.md`
- Tent Placement — `05_Documentation/Game_Systems/Building_Housing_System.md`
- Sleeping — `05_Documentation/Game_Systems/Core_Survival_System.md`
- Discovery System — `05_Documentation/Game_Systems/Discovery_System.md`
- Journal — `05_Documentation/Game_Systems/Discovery_System.md`
- Minimap — `05_Documentation/Game_Systems/Discovery_System.md`
- Save System — `05_Documentation/Systems/Save_System.md`

Design documentation for Alpha 0.1 is complete. Implementation status is tracked by Copilot, not here.

---

# Supporting Documentation Beyond the Alpha 0.1 List

Hunting, Trapping, Livestock, Economy, Season, Weather, Wildlife, and Health are all documented in Game_Systems even though the GDD's Alpha 0.1 Required Systems list doesn't name them directly — they support Year One's five food pillars and the seasonal/economic backbone the GDD describes elsewhere. All complete.

---

# Not Yet Started

These folders have a confirmed purpose (see Current_Task_List.md) but no content yet. None are blockers for Alpha 0.1 implementation — they're either content-tuning data or later-stage architecture:

- `05_Documentation/Livestock` — per-species stat sheets (goats, chickens, rabbits)
- `05_Documentation/Plants` — per-species foraging/plant data
- `05_Documentation/Wildlife` — per-species wildlife stat sheets
- `05_Documentation/Gameplay_Loops` — day-to-day pacing breakdowns
- `05_Documentation/Technical_Design` — architecture and data models
- `05_Documentation/Unity_Architecture` — Unity project conventions
- `05_Documentation/Research` — real-world homesteading reference material

---

# Stages 2 Through 4 — Explicitly Deferred

Early Homestead, Established Homestead, and Modern Homestead (per the GDD's Progression Overview) are not in scope for design work right now. The GDD's Development Rule blocks Farming, Large Livestock Operations, Vehicles, Solar Arrays, and Automation until the Stage 1 survival loop is proven fun. This roadmap will not add tasks for those stages until that milestone is reached and Mike says so.
