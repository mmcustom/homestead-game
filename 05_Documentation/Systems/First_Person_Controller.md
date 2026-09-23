# Homestead
## First Person Controller v1.0

Status: Design Draft

---

# Purpose

The First Person Controller is the foundational movement and interaction system every other Alpha 0.1 system is built on top of.

It governs how the player moves through, looks at, and interacts with the homestead.

---

# Core Philosophy

Movement should feel grounded and physical, consistent with the realism goals of Homestead.

The controller should support:

- Walking
- Sprinting (Fatigue cost)
- Crouching (stalking, stealth during hunting)
- Interaction with the world (pickup, use, harvest)

Movement should never feel arcade-like or overly floaty. This is a survival simulator, not an action game.

---

# Movement States

## Walk

Default movement speed.

No Fatigue cost beyond passive Workload accumulation.

---

## Sprint

Increased speed.

Costs Fatigue.

Reduces stealth — affects Wildlife detection during Hunting, per Wildlife System documentation.

---

## Crouch

Reduced speed.

Reduced visibility to wildlife.

Required for close-range bow stalking, per Hunting System documentation.

---

## Swim

Future System

Not Alpha 0.1.

---

# Interaction System

The player interacts with the world through a context-sensitive prompt.

Examples:

- Harvest (Foraging)
- Check Trap (Trapping)
- Draw Water (Water System)
- Feed Animal (Livestock System)
- Open (Home Storage, Inventory System)

This document governs how the player performs an interaction. What each interaction produces is defined in that system's own documentation.

---

# Carrying and Encumbrance

Movement speed is affected by carried weight.

Full weight and capacity rules are defined in Inventory System documentation.

---

# Stamina

Stamina is a short-term, moment-to-moment resource, distinct from the longer-term Fatigue stat.

Stamina governs:

- Sprinting
- Climbing
- Swimming (future)

Stamina recovers quickly at rest. Fatigue recovers only through Sleep.

Full Fatigue detail is defined in Core Survival System documentation.

---

# Camera and View

First-person camera only.

No third-person mode planned for Alpha 0.1.

---

# Discovery Integration

Discovery triggers are detected by player proximity and line of sight while using the First Person Controller.

Full discovery behavior is defined in Discovery System documentation.

---

# Design Rules

1. Movement should feel grounded, not arcade-like.

2. Sprinting and crouching should have real gameplay tradeoffs, not just cosmetic differences.

3. Interaction should always be context-sensitive and clearly readable to the player.

4. This system defines how the player acts in the world; it does not define what actions produce. That belongs to the relevant Game System doc.

5. Nothing here should be added that isn't required to support the Alpha 0.1 systems already in scope.
