# Homestead
## Save Data Model v1.0

Status: Design Draft — Sketch, Not Implementation

---

# Purpose

A shared data shape for what Save_System.md calls "What Must Be Saved," so Claude, Copilot, and Mike are working from the same picture before any of it gets implemented.

This is a design-level sketch: informal field lists with type hints, not C# classes. Copilot owns the actual implementation, including real types, serialization method, and file format.

---

# Organizing Principle

Save_System.md's Design Rule #3 says adding a new system later shouldn't require restructuring the whole save file.

The proposed shape follows from that: each system below owns its own data block. A save file is a collection of independent blocks, not one flat structure. Adding Livestock species detail later, for example, extends the Property block without touching Player or World.

---

# Player Block

Source: Core Survival System, Health System, Systems/Inventory_System, Systems/First_Person_Controller

- Position (world coordinates)
- Hydration (0–100)
- Hunger (0–100)
- Fatigue (0–100)
- Health (0–100)
- Morale (0–100)
- Inventory contents (list of item id + quantity)
- Equipped tools (current weapon/tool reference)

---

# World Block

Source: Season System, Weather System, Discovery System

- Current Day (integer, counts up)
- Current Season (Spring / Summer / Fall / Winter)
- Current Year (integer)
- Weather state (current weather type, per Weather System)
- Discovered locations (list of location id + discovery date + recorded quality/notes, per Discovery System)
- Journal entries (list, categorized per Discovery System's Journal Integration)
- Minimap reveal state (list of revealed map marker ids)

---

# Property Block

Source: Building and Housing System, Livestock System, Trapping System, Systems/Inventory_System

- Constructed buildings (list of building type + position + construction date)
- Livestock (list of individual animals or herds — species, count, health, feed reserve level; per-species stat sheets are now written in 05_Documentation/Livestock: Goats.md, Chickens.md, Rabbits.md — maturation timers, breeding cooldowns, and yield state per animal should map to those docs' confirmed fields)
- Placed traps (list of trap id + type + position + bait status + last-checked date)
- Home storage contents (per storage structure — Root Cellar, Storage Shed, Barn — list of item id + quantity)

---

# Economy Block

Source: Economy System

- Currency (integer or decimal, TBD by Copilot)
- Active market conditions, if any exist at implementation time

---

# Save Triggers Reference

Per Save_System.md: Manual save at any time; Auto-save at Sleep, building entry/exit, and end-of-day transition. This document only describes what gets written at those triggers, not when — see Save_System.md for trigger timing.

---

# Open Questions for Copilot

These are implementation calls, not design decisions — flagged here so they don't get decided by accident inside unrelated code:

- Serialization format (JSON, binary, Unity's own serialization) — Copilot's call based on what Unity 6 supports well.
- Whether each block above becomes a separate file or one combined save file with sub-sections — either satisfies Save_System.md's extensibility rule.
- Currency's exact numeric type (integer cents vs. decimal) — depends on whether Economy System ever needs fractional currency, which isn't specified yet.

---

# Design Rules

1. Each system owns its own data block. No block should need to know the internal shape of another.

2. This document tracks what data exists, not how it's stored. Storage format is Copilot's decision.

3. When a new Game_Systems or Systems doc is added that implies new persistent state, add a block here rather than editing an existing one, unless the new state clearly belongs to an existing block's system.

4. Nothing here should be treated as final until Copilot confirms it's buildable against Unity's actual serialization tools.
