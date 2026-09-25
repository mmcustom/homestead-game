# Homestead
## Inventory System v1.0

Status: Design Draft

---

# Purpose

The Inventory System governs what the player can carry, store, and manage.

It is the connective tissue between nearly every other system — Foraging, Fishing, Trapping, Hunting, Water, Livestock, Economy, and Building and Housing all produce or consume items through Inventory.

---

# Core Philosophy

Carrying capacity should create real decisions, not just be a formality.

A player should have to choose between:

- Carrying more water
- Carrying more food
- Carrying tools
- Carrying harvested goods

Encumbrance should discourage hoarding everything at once and reward planned trips, consistent with Preparation Beats Panic.

---

# Item Categories

## Resources

Examples: Meat, Fish, Berries, Nuts, Small Furs, Hides, Antlers

---

## Tools

Examples: Axe, Recurve Bow, Bolt-Action Rifle, Fishing Rod, Traps

---

## Consumables

Examples: Cooked food, Water Container contents

---

## Materials

Examples: Lumber, Stone, Cordage

Full item detail — spoilage, sale value, and similar — lives in the relevant Game System doc. This document governs storage and carrying rules only.

---

# Carrying Capacity

The player has a limited carry weight, not slots — weight-based, tied to First Person Controller's movement speed.

Exceeding capacity:

- Prevents picking up more items, or
- Applies a movement speed penalty

Exact limits are a balancing pass, not a design decision, and should be tuned once the core survival loop is playable. First confirmed balancing pass, 2026-09-23 (Mike, after playtesting InventoryManager.cs): encumbered above 30 kg (about 66 lbs) — movement speed penalty begins; hard cap 45 kg (about 99 lbs) — can't carry more. Sprinting is blocked while overloaded. These remain Inspector-tunable, not locked.

---

# Storage

## On-Person Inventory

Limited. What the player is actively carrying.

---

## Home Storage

Root Cellar, Storage Shed, Barn — see Building and Housing System documentation.

Larger capacity, not carried, tied to a specific structure.

---

# Inventory Screen (UI)

Confirmed 2026-09-25 (Mike). A dedicated screen, opened and closed with the I key.

Shows the player's on-person inventory — items carried, current weight measured against the Carrying Capacity thresholds above, and whether the player is Encumbered — and lets the player select which Tool is currently equipped (`InventoryManager`'s existing `EquippedTool`).

No buildable structures exist yet in Alpha 0.1 (Building_Housing_System.md is still a Design Draft with nothing built), so Home Storage transfer — moving items between on-person and a Root Cellar/Shed/Barn — isn't needed by this screen yet. `InventoryManager` already supports per-structure storage containers in code (`GetOrCreateStorage`), so wiring an existing container into this same screen should be a small follow-up once a structure actually exists to interact with, not a redesign.

This screen doesn't need to solve eating by itself — no food-producing systems (Foraging, Fishing) exist yet to put food items in the inventory in the first place — but it's the piece Claude Code flagged as missing in its Hydration/Hunger implementation log: Hunger can currently only be refilled with a debug key since there's no inventory UI to consume an item from. This closes that gap for whenever Foraging/Fishing land and real food items exist to select and eat.

Layout (grid vs. list), interaction (drag-and-drop vs. click-to-equip/use), and whether opening it pauses gameplay are Claude Code's call, same as the Minimap and compass were left open.

---

# Spoilage Interaction

Raw food spoils whether carried or stored, per the Spoilage System defined in Core Survival System documentation.

Storage structures like the Root Cellar and Smokehouse slow or prevent spoilage; on-person inventory does not.

---

# Tool Durability

Future System

Not Alpha 0.1.

Tools may eventually degrade with use and require maintenance or repair.

---

# Economy Integration

Sellable items — Small Furs, Hides, Antlers, surplus produce, and similar — must be in Inventory or Home Storage to be sold.

Full sale detail is defined in Economy System documentation.

---

# Design Rules

1. Capacity should create real tradeoffs, not just be a number the player ignores.

2. Home storage should always outperform on-person carrying, to reward building infrastructure.

3. Inventory does not define what items do — that belongs to the system that produces them.

4. Exact capacity numbers are a balancing pass, not fixed by this document.

5. Nothing here should be added that isn't required to support the Alpha 0.1 systems already in scope.
