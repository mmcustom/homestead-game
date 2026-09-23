# Homestead
## Save System v1.0

Status: Design Draft

---

# Purpose

The Save System preserves player progress between sessions.

Homestead is built around long-term development across seasons and years — nothing else in the game matters if progress can be lost.

---

# Core Philosophy

The player should never lose meaningful progress to a crash, accidental quit, or forgetting to manually save.

Saving should be reliable before it is convenient.

---

# What Must Be Saved

## Player State

- Position
- Hydration, Hunger, Fatigue, Health, Morale
- Inventory contents
- Equipped tools

---

## World State

- Current Day, Season, Year (Season System documentation)
- Weather state (Weather System documentation)
- Discovered locations (Discovery System documentation)
- Journal entries (Discovery System documentation)
- Minimap reveal state

---

## Property State

- Constructed buildings (Building and Housing System documentation)
- Livestock — count, health, feed reserves (Livestock System documentation)
- Placed traps and their status (Trapping System documentation)
- Stored resources — Home Storage (Inventory System documentation)

---

## Economy State

- Currency
- Active market conditions, if any

---

# Save Triggers

## Manual Save

Player-initiated at any time.

---

## Auto-Save

Triggered at meaningful checkpoints.

Examples:

- Sleeping
- Entering or exiting a building
- End of day transition

Auto-save should never interrupt gameplay with a noticeable hitch, if avoidable.

---

# Save Slots

Alpha 0.1 Scope:

A single save slot is sufficient for Alpha testing.

Future Expansion:

Multiple save slots.

---

# Corruption and Failure Handling

Future System

Not Alpha 0.1.

Should eventually include a backup or rolling save to protect against a corrupted save file.

---

# Design Rules

1. Never lose meaningful player progress silently.

2. Auto-save at natural checkpoints, not just on a raw timer.

3. Save data should be organized so that adding a new system later doesn't require restructuring the whole save file.

4. Save and load should be invisible to the player when working correctly.

5. Nothing here should be added that isn't required to support the Alpha 0.1 systems already in scope.
